//
// report.cs: report errors and warnings.
//
// Author: Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc. (http://www.ximian.com)
//

//
// FIXME: currently our class library does not support custom number format strings
//
using System;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace Mono.CSharp {

	/// <summary>
	///   This class is used to report errors and warnings t te user.
	/// </summary>
	public class Report {
		/// <summary>  
		///   Errors encountered so far
		/// </summary>
		static public int Errors;

		/// <summary>  
		///   Warnings encountered so far
		/// </summary>
		static public int Warnings;

		/// <summary>  
		///   Whether errors should be throw an exception
		/// </summary>
		static public bool Fatal;
		
		/// <summary>  
		///   Whether warnings should be considered errors
		/// </summary>
		static public bool WarningsAreErrors;

		/// <summary>  
		///   Whether to dump a stack trace on errors. 
		/// </summary>
		static public bool Stacktrace;
		
		//
		// If the 'expected' error code is reported then the
                // compilation succeeds.
		//
		// Used for the test suite to excercise the error codes
		//
		static int expected_error = 0;

		//
		// Keeps track of the warnings that we are ignoring
		//
		static Hashtable warning_ignore_table;
		
		struct WarningData {
			public WarningData (int level, string text) {
				Level = level;
				Message = text;
			}

			public bool IsEnabled ()
			{
				return RootContext.WarningLevel >= Level;
			}

			public string Format (params object[] args)
			{
				return String.Format (Message, args);
			}

			readonly string Message;
			readonly int Level;
		}

		static string GetErrorMsg (int error_no)
		{
			switch (error_no) {
				case 3001: return "Argument type '{0}' is not CLS-compliant";
				case 3002: return "Return type of '{0}' is not CLS-compliant";
				case 3003: return "Type of '{0}' is not CLS-compliant";
				case 3005: return "Identifier '{0}' differing only in case is not CLS-compliant";
				case 3006: return "Overloaded method '{0}' differing only in ref or out, or in array rank, is not CLS-compliant";
				case 3008: return "Identifier '{0}' is not CLS-compliant";
				case 3009: return "'{0}': base type '{1}' is not CLS-compliant";
				case 3010: return "'{0}': CLS-compliant interfaces must have only CLS-compliant members";
				case 3011: return "'{0}': only CLS-compliant members can be abstract";
				case 3013: return "Added modules must be marked with the CLSCompliant attribute to match the assembly";
				case 3014: return "'{0}' cannot be marked as CLS-compliant because the assembly does not have a CLSCompliant attribute";
				case 3015: return "'{0}' has no accessible constructors which use only CLS-compliant types";
				case 3016: return "Arrays as attribute arguments are not CLS-compliant";
			}
			throw new InternalErrorException (String.Format ("Missing error '{0}' text", error_no));
		}

		static WarningData GetWarningMsg (int warn_no)
		{
			switch (warn_no) {
				case 3012: return new WarningData (1, "You must specify the CLSCompliant attribute on the assembly, not the module, to enable CLS compliance checking");
			}

			throw new InternalErrorException (String.Format ("Wrong warning number '{0}'", warn_no));
		}
		
		static void Check (int code)
		{
			if (code == expected_error){
				if (Fatal)
					throw new Exception ();
				
				Environment.Exit (0);
			}
		}
		
		public static string FriendlyStackTrace (Exception e)
		{
			return FriendlyStackTrace (new StackTrace (e, true));
		}
		
		static string FriendlyStackTrace (StackTrace t)
		{		
			StringBuilder sb = new StringBuilder ();
			
			bool foundUserCode = false;
			
			for (int i = 0; i < t.FrameCount; i++) {
				StackFrame f = t.GetFrame (i);
				MethodBase mb = f.GetMethod ();
				
				if (!foundUserCode && mb.ReflectedType == typeof (Report))
					continue;
				
				foundUserCode = true;
				
				sb.Append ("\tin ");
				
				if (f.GetFileLineNumber () > 0)
					sb.AppendFormat ("(at {0}:{1}) ", f.GetFileName (), f.GetFileLineNumber ());
				
				sb.AppendFormat ("{0}.{1} (", mb.ReflectedType.Name, mb.Name);
				
				bool first = true;
				foreach (ParameterInfo pi in mb.GetParameters ()) {
					if (!first)
						sb.Append (", ");
					first = false;
					
					sb.Append (TypeManager.CSharpName (pi.ParameterType));
				}
				sb.Append (")\n");
			}
	
			return sb.ToString ();
		}
		
		static public void LocationOfPreviousError (Location loc) {
			Console.WriteLine (String.Format ("{0}({1}) (Location of symbol related to previous error)", loc.Name, loc.Row));
		}

		static public void RealError (string msg)
		{
			Errors++;
			Console.WriteLine (msg);

			if (Stacktrace)
				Console.WriteLine (FriendlyStackTrace (new StackTrace (true)));
			
			if (Fatal)
				throw new Exception (msg);
		}


		/// <summary>
		/// Method reports warning message. Only one reason why exist Warning and Report methods is beter code readability.
		/// </summary>
		static public void Warning (int code, Location loc, params object[] args)
		{
			WarningData warning = GetWarningMsg (code);
			if (!warning.IsEnabled ())
				return;

			Warning (code, loc, warning.Format (args));
		}

		/// <summary>
		/// Reports error message.
		/// </summary>
		static public void Error_T (int code, Location loc, params object[] args)
		{
			Error_T (code, String.Format ("{0}({1})", loc.Name, loc.Row), args);
		}

		static public void Error_T (int code, string location, params object[] args)
		{
			string errorText = String.Format (GetErrorMsg (code), args);
			PrintError (code, location, errorText);
		}

		static void PrintError (int code, string l, string text)
		{
			if (code < 0)
				code = 8000-code;
			
			string msg = String.Format ("{0} error CS{1:0000}: {2}", l, code, text);
			RealError (msg);
			Check (code);
		}

		static public void Error (int code, Location l, string text)
		{
			if (code < 0)
				code = 8000-code;
			
			string msg = String.Format (
				"{0}({1}) error CS{2:0000}: {3}", l.Name, l.Row, code, text);
//				"{0}({1}) error CS{2}: {3}", l.Name, l.Row, code, text);
			
			RealError (msg);
			Check (code);
		}

		static public void Warning (int code, Location l, string text)
		{
			if (code < 0)
				code = 8000-code;
			
			if (warning_ignore_table != null){
				if (warning_ignore_table.Contains (code))
					return;
			}
			
			if (WarningsAreErrors)
				Error (code, l, text);
			else {
				string row;
				
				if (Location.IsNull (l))
					row = "";
				else
					row = l.Row.ToString ();
				
				Console.WriteLine (String.Format (
					"{0}({1}) warning CS{2:0000}: {3}",
//					"{0}({1}) warning CS{2}: {3}",
					l.Name,  row, code, text));
				Warnings++;
				Check (code);

				if (Stacktrace)
					Console.WriteLine (new StackTrace ().ToString ());
			}
		}
		
		static public void Warning (int code, string text)
		{
			Warning (code, Location.Null, text);
		}

		static public void Warning (int code, int level, string text)
		{
			if (RootContext.WarningLevel >= level)
				Warning (code, Location.Null, text);
		}

		static public void Warning (int code, int level, Location l, string text)
		{
			if (RootContext.WarningLevel >= level)
				Warning (code, l, text);
		}

		static public void Error (int code, string text)
		{
			if (code < 0)
				code = 8000-code;
			
			string msg = String.Format ("error CS{0:0000}: {1}", code, text);
//			string msg = String.Format ("error CS{0}: {1}", code, text);
			
			RealError (msg);
			Check (code);
		}

		static public void Error (int code, Location loc, string format, params object[] args)
		{
			Error (code, loc, String.Format (format, args));
		}

		static public void Warning (int code, Location loc, string format, params object[] args)
		{
			Warning (code, loc, String.Format (format, args));
		}

		static public void Warning (int code, string format, params object[] args)
		{
			Warning (code, String.Format (format, args));
		}

		static public void Message (Message m)
		{
			if (m is ErrorMessage)
				Error (m.code, m.text);
			else
				Warning (m.code, m.text);
		}

		static public void SetIgnoreWarning (int code)
		{
			if (warning_ignore_table == null)
				warning_ignore_table = new Hashtable ();

			warning_ignore_table [code] = true;
		}
		
                static public int ExpectedError {
                        set {
                                expected_error = value;
                        }
                        get {
                                return expected_error;
                        }
                }

		public static int DebugFlags = 0;

		[Conditional ("MCS_DEBUG")]
		static public void Debug (string message, params object[] args)
		{
			Debug (4, message, args);
		}
			
		[Conditional ("MCS_DEBUG")]
		static public void Debug (int category, string message, params object[] args)
		{
			if ((category & DebugFlags) == 0)
				return;

			StringBuilder sb = new StringBuilder (message);

			if ((args != null) && (args.Length > 0)) {
				sb.Append (": ");

				bool first = true;
				foreach (object arg in args) {
					if (first)
						first = false;
					else
						sb.Append (", ");
					if (arg == null)
						sb.Append ("null");
					else if (arg is ICollection)
						sb.Append (PrintCollection ((ICollection) arg));
					else
						sb.Append (arg);
				}
			}

			Console.WriteLine (sb.ToString ());
		}

		static public string PrintCollection (ICollection collection)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (collection.GetType ());
			sb.Append ("(");

			bool first = true;
			foreach (object o in collection) {
				if (first)
					first = false;
				else
					sb.Append (", ");
				sb.Append (o);
			}

			sb.Append (")");
			return sb.ToString ();
		}
	}

	public class Message {
		public int code;
		public string text;
		
		public Message (int code, string text)
		{
			this.code = code;
			this.text = text;
		}
	}

	public class WarningMessage : Message {
		public WarningMessage (int code, string text) : base (code, text)
		{
		}
	}

	public class ErrorMessage : Message {
		public ErrorMessage (int code, string text) : base (code, text)
		{
		}

		//
		// For compatibility reasons with old code.
		//
		public static void report_error (string error)
		{
			Console.Write ("ERROR: ");
			Console.WriteLine (error);
		}
	}

	public enum TimerType {
		FindMembers	= 0,
		TcFindMembers	= 1,
		MemberLookup	= 2,
		CachedLookup	= 3,
		CacheInit	= 4,
		MiscTimer	= 5,
		CountTimers	= 6
	}

	public enum CounterType {
		FindMembers	= 0,
		MemberCache	= 1,
		MiscCounter	= 2,
		CountCounters	= 3
	}

	public class Timer
	{
		static DateTime[] timer_start;
		static TimeSpan[] timers;
		static long[] timer_counters;
		static long[] counters;

		static Timer ()
		{
			timer_start = new DateTime [(int) TimerType.CountTimers];
			timers = new TimeSpan [(int) TimerType.CountTimers];
			timer_counters = new long [(int) TimerType.CountTimers];
			counters = new long [(int) CounterType.CountCounters];

			for (int i = 0; i < (int) TimerType.CountTimers; i++) {
				timer_start [i] = DateTime.Now;
				timers [i] = TimeSpan.Zero;
			}
		}

		[Conditional("TIMER")]
		static public void IncrementCounter (CounterType which)
		{
			++counters [(int) which];
		}

		[Conditional("TIMER")]
		static public void StartTimer (TimerType which)
		{
			timer_start [(int) which] = DateTime.Now;
		}

		[Conditional("TIMER")]
		static public void StopTimer (TimerType which)
		{
			timers [(int) which] += DateTime.Now - timer_start [(int) which];
			++timer_counters [(int) which];
		}

		[Conditional("TIMER")]
		static public void ShowTimers ()
		{
			ShowTimer (TimerType.FindMembers, "- FindMembers timer");
			ShowTimer (TimerType.TcFindMembers, "- TypeContainer.FindMembers timer");
			ShowTimer (TimerType.MemberLookup, "- MemberLookup timer");
			ShowTimer (TimerType.CachedLookup, "- CachedLookup timer");
			ShowTimer (TimerType.CacheInit, "- Cache init");
			ShowTimer (TimerType.MiscTimer, "- Misc timer");

			ShowCounter (CounterType.FindMembers, "- Find members");
			ShowCounter (CounterType.MemberCache, "- Member cache");
			ShowCounter (CounterType.MiscCounter, "- Misc counter");
		}

		static public void ShowCounter (CounterType which, string msg)
		{
			Console.WriteLine ("{0} {1}", counters [(int) which], msg);
		}

		static public void ShowTimer (TimerType which, string msg)
		{
			Console.WriteLine (
				"[{0:00}:{1:000}] {2} (used {3} times)",
				(int) timers [(int) which].TotalSeconds,
				timers [(int) which].Milliseconds, msg,
				timer_counters [(int) which]);
		}
	}

	public class InternalErrorException : Exception {
		public InternalErrorException ()
			: base ("Internal error")
		{
		}

		public InternalErrorException (string message)
			: base (message)
		{
		}
	}
}
