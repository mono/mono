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
using System.Collections.Specialized;
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

		/// <summary>
		/// List of symbols related to reported error/warning. You have to fill it before error/warning is reported.
		/// </summary>
		static StringCollection related_symbols = new StringCollection ();
		
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
 				case 0122: return "'{0}' is inaccessible due to its protection level";
				case 0145: return "A const field requires a value to be provided";
				case 0160: return "A previous catch clause already catches all exceptions of this or a super type '{0}'";
 				case 0243: return "Conditional not valid on '{0}' because it is an override method";
				case 0247: return "Cannot use a negative size with stackalloc";
				case 0415: return "The 'IndexerName' attribute is valid only on an indexer that is not an explicit interface member declaration";
 				case 0553: return "'{0}' : user defined conversion to/from base class";
 				case 0554: return "'{0}' : user defined conversion to/from derived class";
 				case 0577: return "Conditional not valid on '{0}' because it is a destructor, operator, or explicit interface implementation";
 				case 0578: return "Conditional not valid on '{0}' because its return type is not void";
 				case 0582: return "Conditional not valid on interface members";
				case 0592: return "Attribute '{0}' is not valid on this declaration type. It is valid on {1} declarations only.";
				case 0601: return "The DllImport attribute must be specified on a method marked `static' and `extern'";
				case 0609: return "Cannot set the 'IndexerName' attribute on an indexer marked override";
				case 0610: return "Field or property cannot be of type '{0}'";
				case 0619: return "'{0}' is obsolete: '{1}'";
				case 0626: return "Method, operator, or accessor '{0}' is marked external and has no attributes on it. Consider adding a DllImport attribute to specify the external implementation";
 				case 0629: return "Conditional member '{0}' cannot implement interface member";
				case 0633: return "The argument to the 'IndexerName' attribute must be a valid identifier";
				case 0657: return "'{0}' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are '{1}'";
				case 1555: return "Could not find '{0}' specified for Main method";
				case 1556: return "'{0}' specified for Main method must be a valid class or struct";                                    
 				case 1618: return "Cannot create delegate with '{0}' because it has a Conditional attribute";
				case 1667: return "'{0}' is not valid on property or event accessors. It is valid on '{1}' declarations only";
				case 1669: return "__arglist is not valid in this context";                                    
				case 3000: return "Methods with variable arguments are not CLS-compliant";
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
				case -24: return new WarningData (1, "The Microsoft Runtime cannot set this marshal info. Please use the Mono runtime instead.");
				case -28: return new WarningData (1, "The Microsoft .NET Runtime 1.x does not permit setting custom attributes on the return type");
				case 0612: return new WarningData (1, "'{0}' is obsolete");
				case 0618: return new WarningData (2, "'{0}' is obsolete: '{1}'");
				case 0672: return new WarningData (1, "Member '{0}' overrides obsolete member. Add the Obsolete attribute to '{0}'");
				case 3012: return new WarningData (1, "You must specify the CLSCompliant attribute on the assembly, not the module, to enable CLS compliance checking");
				case 3019: return new WarningData (2, "CLS compliance checking will not be performed on '{0}' because it is private or internal");
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
		
		[Obsolete ("Use SymbolRelatedToPreviousError for better error description")]
		static public void LocationOfPreviousError (Location loc)
		{
			Console.WriteLine (String.Format ("{0}({1}) (Location of symbol related to previous error)", loc.Name, loc.Row));
		}                

		/// <summary>
                /// In most error cases is very useful to have information about symbol that caused the error.
                /// Call this method before you call Report.Error when it makes sense.
		/// </summary>
		static public void SymbolRelatedToPreviousError (Location loc, string symbol)
		{
			SymbolRelatedToPreviousError (String.Format ("{0}({1})", loc.Name, loc.Row), symbol);
		}

		static public void SymbolRelatedToPreviousError (MemberInfo mi)
		{
			DeclSpace temp_ds = TypeManager.LookupDeclSpace (mi.DeclaringType);
			if (temp_ds == null) {
				SymbolRelatedToPreviousError (mi.DeclaringType.Assembly.Location, TypeManager.GetFullNameSignature (mi));
			} else {
				string name = String.Concat (temp_ds.Name, ".", mi.Name);
				MemberCore mc = temp_ds.GetDefinition (name) as MemberCore;
				SymbolRelatedToPreviousError (mc.Location, mc.GetSignatureForError ());
			}
		}

		static public void SymbolRelatedToPreviousError (Type type)
		{
			SymbolRelatedToPreviousError (type.Assembly.Location, TypeManager.CSharpName (type));
		}

		static void SymbolRelatedToPreviousError (string loc, string symbol)
		{
			related_symbols.Add (String.Format ("{0}: ('{1}' name of symbol related to previous error)", loc, symbol));
		}

		static public void RealError (string msg)
		{
			Errors++;
			Console.WriteLine (msg);

			foreach (string s in related_symbols)
				Console.WriteLine (s);
			related_symbols.Clear ();

			if (Stacktrace)
				Console.WriteLine (FriendlyStackTrace (new StackTrace (true)));
			
			if (Fatal)
				throw new Exception (msg);
		}


		/// <summary>
		/// Method reports warning message. Only one reason why exist Warning and Report methods is beter code readability.
		/// </summary>
		static public void Warning_T (int code, Location loc, params object[] args)
		{
			WarningData warning = GetWarningMsg (code);
			if (warning.IsEnabled ())
				Warning (code, loc, warning.Format (args));

			related_symbols.Clear ();
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
				if (warning_ignore_table.Contains (code)) {
					related_symbols.Clear ();
					return;
				}
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

				foreach (string s in related_symbols)
					Console.WriteLine (s);
				related_symbols.Clear ();

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
