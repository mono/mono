//
// report.cs: report errors and warnings.
//
// Author: Miguel de Icaza (miguel@ximian.com)
//         Marek Safar (marek.safar@seznam.cz)         
//
// Copyright 2001 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mono.CSharp {

	//
	// Errors and warnings manager
	//
	public class Report
	{
		/// <summary>  
		///   Whether warnings should be considered errors
		/// </summary>
		public bool WarningsAreErrors;
		List<int> warnings_as_error;
		List<int> warnings_only;

		public static int DebugFlags = 0;

		public const int RuntimeErrorId = 10000;

		//
		// Keeps track of the warnings that we are ignoring
		//
		HashSet<int> warning_ignore_table;

		Dictionary<int, WarningRegions> warning_regions_table;

		int warning_level;

		ReportPrinter printer;

		int reporting_disabled;
		
		/// <summary>
		/// List of symbols related to reported error/warning. You have to fill it before error/warning is reported.
		/// </summary>
		List<string> extra_information = new List<string> ();

		// 
		// IF YOU ADD A NEW WARNING YOU HAVE TO ADD ITS ID HERE
		//
		public static readonly int[] AllWarnings = new int[] {
			28, 67, 78,
			105, 108, 109, 114, 162, 164, 168, 169, 183, 184, 197,
			219, 251, 252, 253, 278, 282,
			402, 414, 419, 420, 429, 436, 440, 458, 464, 465, 467, 469, 472,
			612, 618, 626, 628, 642, 649, 652, 658, 659, 660, 661, 665, 672, 675, 693,
			728,
			809,
			1030, 1058, 1066,
			1522, 1570, 1571, 1572, 1573, 1574, 1580, 1581, 1584, 1587, 1589, 1590, 1591, 1592,
			1616, 1633, 1634, 1635, 1685, 1690, 1691, 1692, 1695, 1696, 1699,
			1700, 1709, 1717, 1718, 1720,
			1901, 1981,
			2002, 2023, 2029,
			3000, 3001, 3002, 3003, 3005, 3006, 3007, 3008, 3009,
			3010, 3011, 3012, 3013, 3014, 3015, 3016, 3017, 3018, 3019,
			3021, 3022, 3023, 3024, 3026, 3027
		};

		static HashSet<int> AllWarningsHashSet;

		public Report (ReportPrinter printer)
		{
			if (printer == null)
				throw new ArgumentNullException ("printer");

			this.printer = printer;
			warning_level = 4;
		}

		public void DisableReporting ()
		{
			++reporting_disabled;
		}

		public void EnableReporting ()
		{
			--reporting_disabled;
		}

		public void FeatureIsNotAvailable (Location loc, string feature)
		{
			string version;
			switch (RootContext.Version) {
			case LanguageVersion.ISO_1:
				version = "1.0";
				break;
			case LanguageVersion.ISO_2:
				version = "2.0";
				break;
			case LanguageVersion.V_3:
				version = "3.0";
				break;
			default:
				throw new InternalErrorException ("Invalid feature version", RootContext.Version);
			}

			Error (1644, loc,
				"Feature `{0}' cannot be used because it is not part of the C# {1} language specification",
				      feature, version);
		}

		public void FeatureIsNotSupported (Location loc, string feature)
		{
			Error (1644, loc,
				"Feature `{0}' is not supported in Mono mcs1 compiler. Consider using the `gmcs' compiler instead",
				feature);
		}
		
		bool IsWarningEnabled (int code, int level, Location loc)
		{
			if (WarningLevel < level)
				return false;

			if (IsWarningDisabledGlobally (code))
				return false;

			if (warning_regions_table == null || loc.IsNull)
				return true;

			WarningRegions regions;
			if (!warning_regions_table.TryGetValue (loc.File, out regions))
				return true;

			return regions.IsWarningEnabled (code, loc.Row);
		}

		public bool IsWarningDisabledGlobally (int code)
		{
			return warning_ignore_table != null && warning_ignore_table.Contains (code);
		}

		bool IsWarningAsError (int code)
		{
			bool is_error = WarningsAreErrors;

			// Check specific list
			if (warnings_as_error != null)
				is_error |= warnings_as_error.Contains (code);

			// Ignore excluded warnings
			if (warnings_only != null && warnings_only.Contains (code))
				is_error = false;

			return is_error;
		}
		        
		public void RuntimeMissingSupport (Location loc, string feature) 
		{
			Error (-88, loc, "Your .NET Runtime does not support `{0}'. Please use the latest Mono runtime instead.", feature);
		}

		/// <summary>
		/// In most error cases is very useful to have information about symbol that caused the error.
		/// Call this method before you call Report.Error when it makes sense.
		/// </summary>
		public void SymbolRelatedToPreviousError (Location loc, string symbol)
		{
			SymbolRelatedToPreviousError (loc.ToString (), symbol);
		}

		public void SymbolRelatedToPreviousError (MemberSpec ms)
		{
			if (reporting_disabled > 0 || !printer.HasRelatedSymbolSupport)
				return;

			var mc = ms.MemberDefinition as MemberCore;
			while (ms is ElementTypeSpec) {
				ms = ((ElementTypeSpec) ms).Element;
				mc = ms.MemberDefinition as MemberCore;
			}

			if (mc != null) {
				SymbolRelatedToPreviousError (mc);
			} else if (ms.MemberDefinition != null) {
				SymbolRelatedToPreviousError (ms.MemberDefinition.Assembly.Location, "");
			}
		}

		public void SymbolRelatedToPreviousError (MemberCore mc)
		{
			SymbolRelatedToPreviousError (mc.Location, mc.GetSignatureForError ());
		}

		void SymbolRelatedToPreviousError (string loc, string symbol)
		{
			string msg = String.Format ("{0} (Location of the symbol related to previous ", loc);
			if (extra_information.Contains (msg))
				return;

			extra_information.Add (msg);
		}

		public void AddWarningAsError (string warningId)
		{
			int id;
			try {
				id = int.Parse (warningId);
			} catch {
				CheckWarningCode (warningId, Location.Null);
				return;
			}

			if (!CheckWarningCode (id, Location.Null))
				return;

			if (warnings_as_error == null)
				warnings_as_error = new List<int> ();
			
			warnings_as_error.Add (id);
		}

		public void RemoveWarningAsError (string warningId)
		{
			int id;
			try {
				id = int.Parse (warningId);
			} catch {
				CheckWarningCode (warningId, Location.Null);
				return;
			}

			if (!CheckWarningCode (id, Location.Null))
				return;

			if (warnings_only == null)
				warnings_only = new List<int> ();

			warnings_only.Add (id);
		}

		public bool CheckWarningCode (string code, Location loc)
		{
			Warning (1691, 1, loc, "`{0}' is not a valid warning number", code);
			return false;
		}

		public bool CheckWarningCode (int code, Location loc)
		{
			if (AllWarningsHashSet == null)
				AllWarningsHashSet = new HashSet<int> (AllWarnings);

			if (AllWarningsHashSet.Contains (code))
				return true;

			return CheckWarningCode (code.ToString (), loc);
		}

		public void ExtraInformation (Location loc, string msg)
		{
			extra_information.Add (String.Format ("{0} {1}", loc, msg));
		}

		public WarningRegions RegisterWarningRegion (Location location)
		{
			WarningRegions regions;
			if (warning_regions_table == null) {
				regions = null;
				warning_regions_table = new Dictionary<int, WarningRegions> ();
			} else {
				warning_regions_table.TryGetValue (location.File, out regions);
			}

			if (regions == null) {
				regions = new WarningRegions ();
				warning_regions_table.Add (location.File, regions);
			}

			return regions;
		}

		public void Warning (int code, int level, Location loc, string message)
		{
			if (reporting_disabled > 0)
				return;

			if (!IsWarningEnabled (code, level, loc))
				return;

			AbstractMessage msg;
			if (IsWarningAsError (code))
				msg = new ErrorMessage (code, loc, message, extra_information);
			else
				msg = new WarningMessage (code, loc, message, extra_information);

			extra_information.Clear ();
			printer.Print (msg);
		}

		public void Warning (int code, int level, Location loc, string format, string arg)
		{
			Warning (code, level, loc, String.Format (format, arg));
		}

		public void Warning (int code, int level, Location loc, string format, string arg1, string arg2)
		{
			Warning (code, level, loc, String.Format (format, arg1, arg2));
		}

		public void Warning (int code, int level, Location loc, string format, params object[] args)
		{
			Warning (code, level, loc, String.Format (format, args));
		}

		public void Warning (int code, int level, string message)
		{
			Warning (code, level, Location.Null, message);
		}

		public void Warning (int code, int level, string format, string arg)
		{
			Warning (code, level, Location.Null, format, arg);
		}

		public void Warning (int code, int level, string format, string arg1, string arg2)
		{
			Warning (code, level, Location.Null, format, arg1, arg2);
		}

		public void Warning (int code, int level, string format, params string[] args)
		{
			Warning (code, level, Location.Null, String.Format (format, args));
		}

		//
		// Warnings encountered so far
		//
		public int Warnings {
			get { return printer.WarningsCount; }
		}

		public void Error (int code, Location loc, string error)
		{
			if (reporting_disabled > 0)
				return;

			ErrorMessage msg = new ErrorMessage (code, loc, error, extra_information);
			extra_information.Clear ();

			printer.Print (msg);
		}

		public void Error (int code, Location loc, string format, string arg)
		{
			Error (code, loc, String.Format (format, arg));
		}

		public void Error (int code, Location loc, string format, string arg1, string arg2)
		{
			Error (code, loc, String.Format (format, arg1, arg2));
		}

		public void Error (int code, Location loc, string format, params string[] args)
		{
			Error (code, loc, String.Format (format, args));
		}

		public void Error (int code, string error)
		{
			Error (code, Location.Null, error);
		}

		public void Error (int code, string format, string arg)
		{
			Error (code, Location.Null, format, arg);
		}

		public void Error (int code, string format, string arg1, string arg2)
		{
			Error (code, Location.Null, format, arg1, arg2);
		}

		public void Error (int code, string format, params string[] args)
		{
			Error (code, Location.Null, String.Format (format, args));
		}

		//
		// Errors encountered so far
		//
		public int Errors {
			get { return printer.ErrorsCount; }
		}

		public bool IsDisabled {
			get {
				return reporting_disabled > 0;
			}
		}

		public ReportPrinter Printer {
			get { return printer; }
		}

		public void SetIgnoreWarning (int code)
		{
			if (warning_ignore_table == null)
				warning_ignore_table = new HashSet<int> ();

			warning_ignore_table.Add (code);
		}

		public ReportPrinter SetPrinter (ReportPrinter printer)
		{
			ReportPrinter old = this.printer;
			this.printer = printer;
			return old;
		}

		public int WarningLevel {
			get {
				return warning_level;
			}
			set {
				warning_level = value;
			}
		}

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
//					else if (arg is ICollection)
//						sb.Append (PrintCollection ((ICollection) arg));
					else
						sb.Append (arg);
				}
			}

			Console.WriteLine (sb.ToString ());
		}
/*
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
*/ 
	}

	public abstract class AbstractMessage
	{
		readonly string[] extra_info;
		protected readonly int code;
		protected readonly Location location;
		readonly string message;

		protected AbstractMessage (int code, Location loc, string msg, List<string> extraInfo)
		{
			this.code = code;
			if (code < 0)
				this.code = 8000 - code;

			this.location = loc;
			this.message = msg;
			if (extraInfo.Count != 0) {
				this.extra_info = extraInfo.ToArray ();
			}
		}

		protected AbstractMessage (AbstractMessage aMsg)
		{
			this.code = aMsg.code;
			this.location = aMsg.location;
			this.message = aMsg.message;
			this.extra_info = aMsg.extra_info;
		}

		public int Code {
			get { return code; }
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

		public Location Location {
			get { return location; }
		}

		public abstract string MessageType { get; }

		public string[] RelatedSymbols {
			get { return extra_info; }
		}

		public string Text {
			get { return message; }
		}
	}

	sealed class WarningMessage : AbstractMessage
	{
		public WarningMessage (int code, Location loc, string message, List<string> extra_info)
			: base (code, loc, message, extra_info)
		{
		}

		public override bool IsWarning {
			get { return true; }
		}

		public override string MessageType {
			get {
				return "warning";
			}
		}
	}

	sealed class ErrorMessage : AbstractMessage
	{
		public ErrorMessage (int code, Location loc, string message, List<string> extraInfo)
			: base (code, loc, message, extraInfo)
		{
		}

		public ErrorMessage (AbstractMessage aMsg)
			: base (aMsg)
		{
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

	//
	// Generic base for any message writer
	//
	public abstract class ReportPrinter {
		/// <summary>  
		///   Whether to dump a stack trace on errors. 
		/// </summary>
		public bool Stacktrace;
		
		int warnings, errors;

		public int WarningsCount {
			get { return warnings; }
		}
		
		public int ErrorsCount {
			get { return errors; }
		}

		protected virtual string FormatText (string txt)
		{
			return txt;
		}

		//
		// When (symbols related to previous ...) can be used
		//
		public virtual bool HasRelatedSymbolSupport {
			get { return true; }
		}

		public virtual void Print (AbstractMessage msg)
		{
			if (msg.IsWarning)
				++warnings;
			else
				++errors;
		}

		protected void Print (AbstractMessage msg, TextWriter output)
		{
			StringBuilder txt = new StringBuilder ();
			if (!msg.Location.IsNull) {
				txt.Append (msg.Location.ToString ());
				txt.Append (" ");
			}

			txt.AppendFormat ("{0} CS{1:0000}: {2}", msg.MessageType, msg.Code, msg.Text);

			if (!msg.IsWarning)
				output.WriteLine (FormatText (txt.ToString ()));
			else
				output.WriteLine (txt.ToString ());

			if (msg.RelatedSymbols != null) {
				foreach (string s in msg.RelatedSymbols)
					output.WriteLine (s + msg.MessageType + ")");
			}
		}
	}

	//
	// Default message recorder, it uses two types of message groups.
	// Common messages: messages reported in all sessions.
	// Merged messages: union of all messages in all sessions. 
	//
	// Used by the Lambda expressions to compile the code with various
	// parameter values, or by attribute resolver
	//
	class SessionReportPrinter : ReportPrinter
	{
		List<AbstractMessage> session_messages;
		//
		// A collection of exactly same messages reported in all sessions
		//
		List<AbstractMessage> common_messages;

		//
		// A collection of unique messages reported in all sessions
		//
		List<AbstractMessage> merged_messages;

		public override void Print (AbstractMessage msg)
		{
			//
			// This line is useful when debugging recorded messages
			//
			// Console.WriteLine ("RECORDING: {0}", msg.ToString ());

			if (session_messages == null)
				session_messages = new List<AbstractMessage> ();

			session_messages.Add (msg);

			base.Print (msg);
		}

		public void EndSession ()
		{
			if (session_messages == null)
				return;

			//
			// Handles the first session
			//
			if (common_messages == null) {
				common_messages = new List<AbstractMessage> (session_messages);
				merged_messages = session_messages;
				session_messages = null;
				return;
			}

			//
			// Store common messages if any
			//
			for (int i = 0; i < common_messages.Count; ++i) {
				AbstractMessage cmsg = common_messages[i];
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
				AbstractMessage msg = session_messages[i];
				bool msg_found = false;
				for (int ii = 0; ii < merged_messages.Count; ++ii) {
					if (msg.Equals (merged_messages[ii])) {
						msg_found = true;
						break;
					}
				}

				if (!msg_found)
					merged_messages.Add (msg);
			}
		}

		public bool IsEmpty {
			get {
				return merged_messages == null && common_messages == null;
			}
		}

		//
		// Prints collected messages, common messages have a priority
		//
		public bool Merge (ReportPrinter dest)
		{
			var messages_to_print = merged_messages;
			if (common_messages != null && common_messages.Count > 0) {
				messages_to_print = common_messages;
			}

			if (messages_to_print == null)
				return false;

			bool error_msg = false;
			foreach (AbstractMessage msg in messages_to_print) {
				dest.Print (msg);
				error_msg |= !msg.IsWarning;
			}

			return error_msg;
		}
	}

	class StreamReportPrinter : ReportPrinter
	{
		readonly TextWriter writer;

		public StreamReportPrinter (TextWriter writer)
		{
			this.writer = writer;
		}

		public override void Print (AbstractMessage msg)
		{
			Print (msg, writer);
			base.Print (msg);
		}
	}

	class ConsoleReportPrinter : StreamReportPrinter
	{
		static readonly string prefix, postfix;

		static ConsoleReportPrinter ()
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

			if (!(UnixUtils.isatty (1) && UnixUtils.isatty (2)))
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

		public ConsoleReportPrinter ()
			: base (Console.Error)
		{
		}

		public ConsoleReportPrinter (TextWriter writer)
			: base (writer)
		{
		}

		public int Fatal { get; set; }

		static int NameToCode (string s)
		{
			switch (s) {
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

			if (s.StartsWith ("bright")) {
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

		protected override string FormatText (string txt)
		{
			if (prefix != null)
				return prefix + txt + postfix;

			return txt;
		}

		static string FriendlyStackTrace (StackTrace t)
		{		
			StringBuilder sb = new StringBuilder ();
			
			bool foundUserCode = false;
			
			for (int i = 0; i < t.FrameCount; i++) {
				StackFrame f = t.GetFrame (i);
				var mb = f.GetMethod ();
				
				if (!foundUserCode && mb.ReflectedType == typeof (Report))
					continue;
				
				foundUserCode = true;
				
				sb.Append ("\tin ");
				
				if (f.GetFileLineNumber () > 0)
					sb.AppendFormat ("(at {0}:{1}) ", f.GetFileName (), f.GetFileLineNumber ());
				
				sb.AppendFormat ("{0}.{1} (", mb.ReflectedType.Name, mb.Name);
				
				bool first = true;
				foreach (var pi in mb.GetParameters ()) {
					if (!first)
						sb.Append (", ");
					first = false;

					sb.Append (pi.ParameterType.FullName);
				}
				sb.Append (")\n");
			}
	
			return sb.ToString ();
		}

		int print_count;
		public override void Print (AbstractMessage msg)
		{
			base.Print (msg);

			if (Stacktrace)
				Console.WriteLine (FriendlyStackTrace (new StackTrace (true)));

			if (++print_count == Fatal)
				throw new Exception (msg.Text);
		}

		public static string FriendlyStackTrace (Exception e)
		{
			return FriendlyStackTrace (new StackTrace (e, true));
		}

		public static void StackTrace ()
		{
			Console.WriteLine (FriendlyStackTrace (new StackTrace (true)));
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
		{
		}

		public InternalErrorException (Exception exception, string message, params object[] args)
			: base (String.Format (message, args), exception)
		{
		}
		
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


		List<PragmaCmd> regions = new List<PragmaCmd> ();

		public void WarningDisable (int line)
		{
			regions.Add (new DisableAll (line));
		}

		public void WarningDisable (Location location, int code, Report Report)
		{
			if (Report.CheckWarningCode (code, location))
				regions.Add (new Disable (location.Row, code));
		}

		public void WarningEnable (int line)
		{
			regions.Add (new EnableAll (line));
		}

		public void WarningEnable (Location location, int code, Report Report)
		{
			if (!Report.CheckWarningCode (code, location))
				return;

			if (Report.IsWarningDisabledGlobally (code))
				Report.Warning (1635, 1, location, "Cannot restore warning `CS{0:0000}' because it was disabled globally", code);

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
	}
}
