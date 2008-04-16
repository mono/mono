//
// report.cs: report errors and warnings.
//
// Author: Miguel de Icaza (miguel@ximian.com)
//         Marek Safar (marek.safar@seznam.cz)         
//
// (C) 2001 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

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

		static public TextWriter Stderr = Console.Error;
		
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
		public static Hashtable warning_ignore_table;

		static Hashtable warning_regions_table;

		//
		// This is used to save/restore the error state.  When the
		// error stack contains elements, warnings and errors are not
		// reported to the user.  This is used by the Lambda expression
		// support to compile the code with various parameter values.
		// A stack because of `Report.Errors == errors;'
		//
		static Stack error_stack;
		static Stack warning_stack;
		static bool reporting_disabled;
		
		static int warning_level;
		
		/// <summary>
		/// List of symbols related to reported error/warning. You have to fill it before error/warning is reported.
		/// </summary>
		static ArrayList extra_information = new ArrayList ();

		// 
		// IF YOU ADD A NEW WARNING YOU HAVE TO ADD ITS ID HERE
		//
		public static readonly int[] AllWarnings = new int[] {
			28, 67, 78,
			105, 108, 109, 114, 162, 164, 168, 169, 183, 184, 197,
			219, 251, 252, 253, 278, 282,
			419, 420, 429, 436, 440, 465, 467, 469, 472,
			612, 618, 626, 628, 642, 649, 652, 658, 659, 660, 661, 665, 672, 675,
			809,
			1030,
			1522, 1570, 1571, 1572, 1573, 1574, 1580, 1581, 1584, 1587, 1589, 1590, 1591, 1592,
			1616, 1633, 1634, 1635, 1685, 1690, 1691, 1692,
			1717, 1718, 1720,
			1901,
			2002, 2023, 2029,
			3005, 3012, 3018, 3019, 3021, 3022, 3023, 3026, 3027,
#if GMCS_SOURCE
			402, 414, 458, 464, 693, 1058, 1700, 3024
#endif
		};

		static Report ()
		{
			// Just to be sure that binary search is working
			Array.Sort (AllWarnings);
		}

		public static void Reset ()
		{
			Errors = Warnings = 0;
			WarningsAreErrors = false;
			warning_ignore_table = null;
			warning_regions_table = null;
			reporting_disabled = false;
			error_stack = warning_stack = null;
		}

		public static void DisableReporting ()
		{
			if (error_stack == null)
				error_stack = new Stack ();
			error_stack.Push (Errors);

			if (Warnings > 0) {
				if (warning_stack == null)
					warning_stack = new Stack ();
				warning_stack.Push (Warnings);
			}

			reporting_disabled = true;
		}

		public static void EnableReporting ()
		{
			if (warning_stack != null)
				Warnings = (int) warning_stack.Pop ();
			else
				Warnings = 0;

			Errors = (int) error_stack.Pop ();
			if (error_stack.Count == 0) {
				reporting_disabled = false;
			}
		}

		public static IMessageRecorder msg_recorder;

		public static IMessageRecorder SetMessageRecorder (IMessageRecorder recorder)
		{
			IMessageRecorder previous = msg_recorder;
			msg_recorder = recorder;
			return previous;
		}

		public interface IMessageRecorder
		{
			void EndSession ();
			void AddMessage (AbstractMessage msg);
			bool PrintMessages ();
		}

		//
		// Default message recorder, it uses two types of message groups.
		// Common messages: messages reported in all sessions.
		// Merged messages: union of all messages in all sessions. 
		//		
		public struct MessageRecorder : IMessageRecorder
		{
			ArrayList session_messages;
			//
			// A collection of exactly same messages reported in all sessions
			//
			ArrayList common_messages;

			//
			// A collection of unique messages reported in all sessions
			//
			ArrayList merged_messages;

			public void EndSession ()
			{
				if (session_messages == null)
					return;

				//
				// Handles the first session
				//
				if (common_messages == null) {
					common_messages = new ArrayList (session_messages);
					merged_messages = session_messages;
					session_messages = null;
					return;
				}

				//
				// Store common messages if any
				//
				for (int i = 0; i < common_messages.Count; ++i) {
					AbstractMessage cmsg = (AbstractMessage) common_messages [i];
					bool common_msg_found = false;
					foreach (AbstractMessage msg in session_messages) {
						if (cmsg.Equals (msg)) {
							common_msg_found = true;
							break;
						}
					}

					if (!common_msg_found)
						common_messages.RemoveAt (i);
				}

				//
				// Merge session and previous messages
				//
				for (int i = 0; i < session_messages.Count; ++i) {
					AbstractMessage msg = (AbstractMessage) session_messages [i];
					bool msg_found = false;
					for (int ii = 0; ii < merged_messages.Count; ++ii) {
						if (msg.Equals (merged_messages [ii])) {
							msg_found = true;
							break;
						}
					}

					if (!msg_found)
						merged_messages.Add (msg);
				}
			}

			public void AddMessage (AbstractMessage msg)
			{
				if (session_messages == null)
					session_messages = new ArrayList ();

				session_messages.Add (msg);
			}

			//
			// Prints collected messages, common messages have a priority
			//
			public bool PrintMessages ()
			{
				ArrayList messages_to_print = merged_messages;
				if (common_messages != null && common_messages.Count > 0) {
					messages_to_print = common_messages;
				}

				if (messages_to_print == null)
					return false;

				foreach (AbstractMessage msg in messages_to_print)
					msg.Print ();

				return true;
			}
		}
		
		public abstract class AbstractMessage
		{
			readonly string[] extra_info;
			protected readonly int code;
			protected readonly Location location;
			readonly string message;

			protected AbstractMessage (int code, Location loc, string msg, ArrayList extraInfo)
			{
				this.code = code;
				if (code < 0)
					this.code = 8000 - code;

				this.location = loc;
				this.message = msg;
				if (extraInfo.Count != 0) {
					this.extra_info = (string[])extraInfo.ToArray (typeof (string));
				}
			}

			protected AbstractMessage (AbstractMessage aMsg)
			{
				this.code = aMsg.code;
				this.location = aMsg.location;
				this.message = aMsg.message;
				this.extra_info = aMsg.extra_info;
			}

			static void Check (int code)
			{
				if (code == expected_error) {
					Environment.Exit (0);
				}
			}

			public override bool Equals (object obj)
			{
				AbstractMessage msg = obj as AbstractMessage;
				if (msg == null)
					return false;

				return code == msg.code && location.Equals (msg.location) && message == msg.message;
			}

			public override int GetHashCode ()
			{
				return code.GetHashCode ();
			}

			public abstract bool IsWarning { get; }

			public abstract string MessageType { get; }

			public virtual void Print ()
			{
				if (msg_recorder != null) {
					//
					// This line is useful when debugging messages recorder
					//
					// Console.WriteLine ("RECORDING: {0} {1} {2}", code, location, message);
					msg_recorder.AddMessage (this);
					return;
				}

				if (reporting_disabled)
					return;

				StringBuilder msg = new StringBuilder ();
				if (!location.IsNull) {
					msg.Append (location.ToString ());
					msg.Append (" ");
				}
				msg.AppendFormat ("{0} CS{1:0000}: {2}", MessageType, code, message);

				//
				// 
				if (Stderr == Console.Error)
					Stderr.WriteLine (ColorFormat (msg.ToString ()));
				else
					Stderr.WriteLine (msg.ToString ());

				if (extra_info != null) {
					foreach (string s in extra_info)
						Stderr.WriteLine (s + MessageType + ")");
				}

				if (Stacktrace)
					Console.WriteLine (FriendlyStackTrace (new StackTrace (true)));

				if (Fatal) {
					if (!IsWarning || WarningsAreErrors)
						throw new Exception (message);
				}

				Check (code);
			}

			protected virtual string ColorFormat (string s)
			{
				return s;
			}
		}

		sealed class WarningMessage : AbstractMessage
		{
			readonly int Level;

			public WarningMessage (int code, int level, Location loc, string message, ArrayList extra_info)
				: base (code, loc, message, extra_info)
			{
				Level = level;
			}

			public override bool IsWarning {
				get { return true; }
			}

			bool IsEnabled ()
			{
				if (WarningLevel < Level)
					return false;

				if (warning_ignore_table != null) {
					if (warning_ignore_table.Contains (code)) {
						return false;
					}
				}

				if (warning_regions_table == null || location.IsNull)
					return true;

				WarningRegions regions = (WarningRegions)warning_regions_table [location.Name];
				if (regions == null)
					return true;

				return regions.IsWarningEnabled (code, location.Row);
			}

			public override void Print ()
			{
				if (!IsEnabled ())
					return;

				if (WarningsAreErrors) {
					new ErrorMessage (this).Print ();
					return;
				}

				Warnings++;
				base.Print ();
			}

			public override string MessageType {
				get {
					return "warning";
				}
			}
		}

		static int NameToCode (string s)
		{
			switch (s){
			case "black":
				return 0;
			case "red":
				return 1;
			case "green":
				return 2;
			case "yellow":
				return 3;
			case "blue":
				return 4;
			case "magenta":
				return 5;
			case "cyan":
				return 6;
			case "grey":
			case "white":
				return 7;
			}
			return 7;
		}
		
		//
		// maps a color name to its xterm color code
		//
		static string GetForeground (string s)
		{
			string highcode;

			if (s.StartsWith ("bright")){
				highcode = "1;";
				s = s.Substring (6);
			} else
				highcode = "";

			return "\x001b[" + highcode + (30 + NameToCode (s)).ToString () + "m";
		}

		static string GetBackground (string s)
		{
			return "\x001b[" + (40 + NameToCode (s)).ToString () + "m";
		}
		
		sealed class ErrorMessage : AbstractMessage
		{
			static string prefix, postfix;

			[System.Runtime.InteropServices.DllImport ("libc", EntryPoint="isatty")]
			extern static int _isatty (int fd);
			
			static bool isatty (int fd)
			{
				try {
					return _isatty (fd) == 1;
				} catch {
					return false;
				}
			}
			
			static ErrorMessage ()
			{
				string term = Environment.GetEnvironmentVariable ("TERM");
				bool xterm_colors = false;
				
				switch (term){
				case "xterm":
				case "rxvt":
				case "rxvt-unicode": 
					if (Environment.GetEnvironmentVariable ("COLORTERM") != null){
						xterm_colors = true;
					}
					break;

				case "xterm-color":
					xterm_colors = true;
					break;
				}
				if (!xterm_colors)
					return;

				if (!(isatty (1) && isatty (2)))
					return;
				
				string config = Environment.GetEnvironmentVariable ("MCS_COLORS");
				if (config == null){
					config = "errors=red";
					//config = "brightwhite,red";
				}

				if (config == "disable")
					return;

				if (!config.StartsWith ("errors="))
					return;

				config = config.Substring (7);
				
				int p = config.IndexOf (",");
				if (p == -1)
					prefix = GetForeground (config);
				else
					prefix = GetBackground (config.Substring (p+1)) + GetForeground (config.Substring (0, p));
				postfix = "\x001b[0m";
			}

			public ErrorMessage (int code, Location loc, string message, ArrayList extraInfo)
				: base (code, loc, message, extraInfo)
			{
			}

			public ErrorMessage (AbstractMessage aMsg)
				: base (aMsg)
			{
			}

			protected override string ColorFormat (string s)
			{
				if (prefix != null)
					return prefix + s + postfix;
				return s;
			}
			
			public override void Print()
			{
				Errors++;
				base.Print ();
			}

			public override bool IsWarning {
				get { return false; }
			}

			public override string MessageType {
				get {
					return "error";
				}
			}
		}

		public static void FeatureIsNotAvailable (Location loc, string feature)
		{
			string version;
			switch (RootContext.Version) {
			case LanguageVersion.ISO_1:
				version = "1.0";
				break;
			case LanguageVersion.ISO_2:
				version = "2.0";
				break;
			case LanguageVersion.Default_MCS:
				Report.Error (1644, loc, "Feature `{0}' is not available in Mono mcs1 compiler. Consider using the `gmcs' compiler instead",
				              feature);
				return;
			default:
				throw new InternalErrorException ("Invalid feature version", RootContext.Version);
			}

			Report.Error (1644, loc,
				"Feature `{0}' cannot be used because it is not part of the C# {1} language specification",
				      feature, version);
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

		public static void StackTrace ()
		{
			Console.WriteLine (FriendlyStackTrace (new StackTrace (true)));
		}

		public static bool IsValidWarning (int code)
		{	
			return Array.BinarySearch (AllWarnings, code) >= 0;
		}
		        
		static public void RuntimeMissingSupport (Location loc, string feature) 
		{
			Report.Error (-88, loc, "Your .NET Runtime does not support `{0}'. Please use the latest Mono runtime instead.", feature);
		}

		/// <summary>
		/// In most error cases is very useful to have information about symbol that caused the error.
		/// Call this method before you call Report.Error when it makes sense.
		/// </summary>
		static public void SymbolRelatedToPreviousError (Location loc, string symbol)
		{
			SymbolRelatedToPreviousError (loc.ToString (), symbol);
		}

		static public void SymbolRelatedToPreviousError (MemberInfo mi)
		{
			if (reporting_disabled)
				return;

			Type dt = TypeManager.DropGenericTypeArguments (mi.DeclaringType);
			if (TypeManager.IsDelegateType (dt)) {
				SymbolRelatedToPreviousError (dt);
				return;
			}			
			
			DeclSpace temp_ds = TypeManager.LookupDeclSpace (dt);
			if (temp_ds == null) {
				SymbolRelatedToPreviousError (dt.Assembly.Location, TypeManager.GetFullNameSignature (mi));
			} else {
				MethodBase mb = mi as MethodBase;
				if (mb != null) {
					mb = TypeManager.DropGenericMethodArguments (mb);
					IMethodData md = TypeManager.GetMethod (mb);
					SymbolRelatedToPreviousError (md.Location, md.GetSignatureForError ());
					return;
				}

				MemberCore mc = temp_ds.GetDefinition (mi.Name);
				SymbolRelatedToPreviousError (mc);
			}
		}

		static public void SymbolRelatedToPreviousError (MemberCore mc)
		{
			SymbolRelatedToPreviousError (mc.Location, mc.GetSignatureForError ());
		}

		static public void SymbolRelatedToPreviousError (Type type)
		{
			if (reporting_disabled)
				return;

			type = TypeManager.DropGenericTypeArguments (type);

			if (TypeManager.IsGenericParameter (type)) {
				TypeParameter tp = TypeManager.LookupTypeParameter (type);
				if (tp != null) {
					SymbolRelatedToPreviousError (tp.Location, "");
					return;
				}
			}

			if (type is TypeBuilder) {
				DeclSpace temp_ds = TypeManager.LookupDeclSpace (type);
				SymbolRelatedToPreviousError (temp_ds.Location, TypeManager.CSharpName (type));
			} else if (TypeManager.HasElementType (type)) {
				SymbolRelatedToPreviousError (type.GetElementType ());
			} else {
				SymbolRelatedToPreviousError (type.Assembly.Location, TypeManager.CSharpName (type));
			}
		}

		static void SymbolRelatedToPreviousError (string loc, string symbol)
		{
			extra_information.Add (String.Format ("{0} (Location of the symbol related to previous ", loc));
		}

		public static void ExtraInformation (Location loc, string msg)
		{
			extra_information.Add (String.Format ("{0} {1}", loc, msg));
		}

		public static WarningRegions RegisterWarningRegion (Location location)
		{
			if (warning_regions_table == null)
				warning_regions_table = new Hashtable ();

			WarningRegions regions = (WarningRegions)warning_regions_table [location.Name];
			if (regions == null) {
				regions = new WarningRegions ();
				warning_regions_table.Add (location.Name, regions);
			}
			return regions;
		}

		static public void Warning (int code, int level, Location loc, string message)
		{
			WarningMessage w = new WarningMessage (code, level, loc, message, extra_information);
			extra_information.Clear ();
			w.Print ();
		}

		static public void Warning (int code, int level, Location loc, string format, string arg)
		{
			WarningMessage w = new WarningMessage (code, level, loc, String.Format (format, arg), extra_information);
			extra_information.Clear ();
			w.Print ();
		}

		static public void Warning (int code, int level, Location loc, string format, string arg1, string arg2)
		{
			WarningMessage w = new WarningMessage (code, level, loc, String.Format (format, arg1, arg2), extra_information);
			extra_information.Clear ();
			w.Print ();
		}

		static public void Warning (int code, int level, Location loc, string format, params object[] args)
		{
			WarningMessage w = new WarningMessage (code, level, loc, String.Format (format, args), extra_information);
			extra_information.Clear ();
			w.Print ();
		}

		static public void Warning (int code, int level, string message)
		{
			Warning (code, level, Location.Null, message);
		}

		static public void Warning (int code, int level, string format, string arg)
		{
			Warning (code, level, Location.Null, format, arg);
		}

		static public void Warning (int code, int level, string format, string arg1, string arg2)
		{
			Warning (code, level, Location.Null, format, arg1, arg2);
		}

		static public void Warning (int code, int level, string format, params string[] args)
		{
			Warning (code, level, Location.Null, String.Format (format, args));
		}

		static public void Error (int code, Location loc, string error)
		{
			new ErrorMessage (code, loc, error, extra_information).Print ();
			extra_information.Clear ();
		}

		static public void Error (int code, Location loc, string format, string arg)
		{
			new ErrorMessage (code, loc, String.Format (format, arg), extra_information).Print ();
			extra_information.Clear ();
		}

		static public void Error (int code, Location loc, string format, string arg1, string arg2)
		{
			new ErrorMessage (code, loc, String.Format (format, arg1, arg2), extra_information).Print ();
			extra_information.Clear ();
		}

		static public void Error (int code, Location loc, string format, params object[] args)
		{
			Error (code, loc, String.Format (format, args));
		}

		static public void Error (int code, string error)
		{
			Error (code, Location.Null, error);
		}

		static public void Error (int code, string format, string arg)
		{
			Error (code, Location.Null, format, arg);
		}

		static public void Error (int code, string format, string arg1, string arg2)
		{
			Error (code, Location.Null, format, arg1, arg2);
		}

		static public void Error (int code, string format, params string[] args)
		{
			Error (code, Location.Null, String.Format (format, args));
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
		
		public static int WarningLevel {
			get {
				return warning_level;
			}
			set {
				warning_level = value;
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
		public InternalErrorException (MemberCore mc, Exception e)
			: base (mc.Location + " " + mc.GetSignatureForError (), e)
		{
		}

		public InternalErrorException ()
			: base ("Internal error")
		{
		}

		public InternalErrorException (string message)
			: base (message)
		{
		}

		public InternalErrorException (string message, params object[] args)
			: base (String.Format (message, args))
		{ }
		
		public InternalErrorException (Exception e, Location loc)
			: base (loc.ToString (), e)
		{
		}
	}

	/// <summary>
	/// Handles #pragma warning
	/// </summary>
	public class WarningRegions {

		abstract class PragmaCmd
		{
			public int Line;

			protected PragmaCmd (int line)
			{
				Line = line;
			}

			public abstract bool IsEnabled (int code, bool previous);
		}
		
		class Disable : PragmaCmd
		{
			int code;
			public Disable (int line, int code)
				: base (line)
			{
				this.code = code;
			}

			public override bool IsEnabled (int code, bool previous)
			{
				return this.code == code ? false : previous;
			}
		}

		class DisableAll : PragmaCmd
		{
			public DisableAll (int line)
				: base (line) {}

			public override bool IsEnabled(int code, bool previous)
			{
				return false;
			}
		}

		class Enable : PragmaCmd
		{
			int code;
			public Enable (int line, int code)
				: base (line)
			{
				this.code = code;
			}

			public override bool IsEnabled(int code, bool previous)
			{
				return this.code == code ? true : previous;
			}
		}

		class EnableAll : PragmaCmd
		{
			public EnableAll (int line)
				: base (line) {}

			public override bool IsEnabled(int code, bool previous)
			{
				return true;
			}
		}


		ArrayList regions = new ArrayList ();

		public void WarningDisable (int line)
		{
			regions.Add (new DisableAll (line));
		}

		public void WarningDisable (Location location, int code)
		{
			if (CheckWarningCode (code, location))
				regions.Add (new Disable (location.Row, code));
		}

		public void WarningEnable (int line)
		{
			regions.Add (new EnableAll (line));
		}

		public void WarningEnable (Location location, int code)
		{
			if (CheckWarningCode (code, location))
				regions.Add (new Enable (location.Row, code));
		}

		public bool IsWarningEnabled (int code, int src_line)
		{
			bool result = true;
			foreach (PragmaCmd pragma in regions) {
				if (src_line < pragma.Line)
					break;

				result = pragma.IsEnabled (code, result);
			}
			return result;
		}

		static bool CheckWarningCode (int code, Location loc)
		{
			if (Report.IsValidWarning (code))
				return true;

			Report.Warning (1691, 1, loc, "`{0}' is not a valid warning number", code.ToString ());
			return false;
		}
	}
}
