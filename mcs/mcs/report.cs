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

		static Hashtable warning_regions_table;

		/// <summary>
		/// List of symbols related to reported error/warning. You have to fill it before error/warning is reported.
		/// </summary>
		static StringCollection related_symbols = new StringCollection ();

		abstract class AbstractMessage {

			static void Check (int code)
			{
				if (code == expected_error) {
					Environment.Exit (0);
				}
			}

			public abstract string MessageType { get; }

			public virtual void Print (int code, string location, string text)
			{
				if (code < 0)
					code = 8000-code;

				StringBuilder msg = new StringBuilder ();
				if (location.Length != 0) {
					msg.Append (location);
					msg.Append (' ');
				}
				msg.AppendFormat ("{0} CS{1:0000}: {2}", MessageType, code, text);
				Console.WriteLine (msg.ToString ());

				foreach (string s in related_symbols) {
					Console.WriteLine (String.Concat (s, MessageType, ')'));
				}
				related_symbols.Clear ();

				if (Stacktrace)
					Console.WriteLine (FriendlyStackTrace (new StackTrace (true)));

				if (Fatal)
					throw new Exception (text);

				Check (code);
			}

			public virtual void Print (int code, Location location, string text)
			{
				if (location.Equals (Location.Null)) {
					Print (code, "", text);
					return;
				}
				Print (code, String.Format ("{0}({1})", location.Name, location.Row), text);
			}
		}

		sealed class WarningMessage: AbstractMessage {
			Location loc = Location.Null;
			readonly int Level;

			public WarningMessage ():
				this (-1) {}

			public WarningMessage (int level)
			{
				Level = level;
			}

			bool IsEnabled (int code)
			{
				if (RootContext.WarningLevel < Level)
					return false;

				if (warning_ignore_table != null) {
					if (warning_ignore_table.Contains (code)) {
						return false;
					}
				}

				if (warning_regions_table == null || loc.Equals (Location.Null))
					return true;

				WarningRegions regions = (WarningRegions)warning_regions_table [loc.Name];
				return regions.IsWarningEnabled (code, loc.Row);
			}

			public override void Print(int code, string location, string text)
			{
				if (!IsEnabled (code)) {
					related_symbols.Clear ();
					return;
				}

				if (WarningsAreErrors) {
					new ErrorMessage ().Print (code, location, text);
					return;
				}

				Warnings++;
				base.Print (code, location, text);
			}

			public override void Print(int code, Location location, string text)
			{
				loc = location;
				base.Print (code, location, text);
			}

			public override string MessageType {
				get {
					return "warning";
				}
			}
		}

		sealed class ErrorMessage: AbstractMessage {

			public override void Print(int code, string location, string text)
			{
				Errors++;
				base.Print (code, location, text);
			}

			public override string MessageType {
				get {
					return "error";
				}
			}

		}

		public static void FeatureIsNotStandardized (Location loc, string feature)
		{
			Report.Error (1644, loc, "Feature '{0}' cannot be used because it is not part of the standardized ISO C# language specification", feature);
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

		// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		// IF YOU ADD A NEW WARNING YOU HAVE TO DUPLICATE ITS ID HERE
		public static bool IsValidWarning (int code)
		{
			int[] all_warnings = new int[] { 28, 67, 78, 105, 108, 109, 114, 192, 168, 169, 183, 184, 219, 251, 612, 618, 626, 628, 642, 649,
											 659, 660, 661, 672, 1030, 1522, 1616, 1691, 1692, 1901, 2002, 2023, 3012, 3019, 8024, 8028
										   };
			foreach (int i in all_warnings) {
				if (i == code)
					return true;
			}
			return false;
		}
		
		static public void LocationOfPreviousError (Location loc)
		{
			Console.WriteLine (String.Format ("{0}({1}) (Location of symbol related to previous error)", loc.Name, loc.Row));
		}    
        
		static public void RuntimeMissingSupport (string feature) 
		{
			Report.Error (-88, "Your .NET Runtime does not support '{0}'. Please use the latest Mono runtime instead.");
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
			TypeContainer temp_ds = TypeManager.LookupTypeContainer (mi.DeclaringType);
			if (temp_ds == null) {
				SymbolRelatedToPreviousError (mi.DeclaringType.Assembly.Location, TypeManager.GetFullNameSignature (mi));
			} else {
				if (mi is MethodBase) {
					IMethodData md = TypeManager.GetMethod ((MethodBase)mi);
					SymbolRelatedToPreviousError (md.Location, md.GetSignatureForError (temp_ds));
					return;
				}

				string name = String.Concat (temp_ds.Name, ".", mi.Name);
				MemberCore mc = temp_ds.GetDefinition (name);
				SymbolRelatedToPreviousError (mc);
			}
		}

		static public void SymbolRelatedToPreviousError (MemberCore mc)
		{
			SymbolRelatedToPreviousError (mc.Location, mc.GetSignatureForError ());
		}

		static public void SymbolRelatedToPreviousError (Type type)
		{
			DeclSpace temp_ds = TypeManager.LookupDeclSpace (type);
			if (temp_ds == null)
				SymbolRelatedToPreviousError (type.Assembly.Location, TypeManager.CSharpName (type));
			else 
				SymbolRelatedToPreviousError (temp_ds.Location, TypeManager.CSharpName (type));
		}

		static void SymbolRelatedToPreviousError (string loc, string symbol)
		{
			related_symbols.Add (String.Format ("{0}: '{1}' (name of symbol related to previous ", loc, symbol));
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

		static public void Warning (int code, int level, Location loc, string format, params object[] args)
		{
			WarningMessage w = new WarningMessage (level);
			w.Print (code, loc, String.Format (format, args));
		}

		static public void Warning (int code, Location loc, string format, params object[] args)
		{
			WarningMessage w = new WarningMessage ();
			w.Print (code, loc, String.Format (format, args));
		}

		static public void Warning (int code, string format, params object[] args)
		{
			Warning (code, Location.Null, String.Format (format, args));
		}

		/// <summary>
		/// Did you test your WarningLevel, that you use this method
		/// </summary>
		static public void Warning (int code, string text)
		{
			Warning (code, Location.Null, text);
		}

		static public void Error (int code, string format, params object[] args)
		{
			Error (code, Location.Null, String.Format (format, args));
		}

		static public void Error (int code, Location loc, string format, params object[] args)
		{
			ErrorMessage e = new ErrorMessage ();
			e.Print (code, loc, String.Format (format, args));
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
		
		class Disable: PragmaCmd
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

		class DisableAll: PragmaCmd
		{
			public DisableAll (int line)
				: base (line) {}

			public override bool IsEnabled(int code, bool previous)
			{
				return false;
			}
		}

		class Enable: PragmaCmd
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

		class EnableAll: PragmaCmd
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

		bool CheckWarningCode (int code, Location loc)
		{
			if (Report.IsValidWarning (code))
				return true;

			Report.Warning (1691, 1, loc, "'{0}' is not a valid warning number", code);
			return false;
		}
	}
}
