//
// ConsoleLogger.cs: Outputs to the console
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Microsoft.Build.Framework;

#if MICROSOFT_BUILD_DLL
namespace Microsoft.Build.Logging
#else
namespace Microsoft.Build.BuildEngine
#endif
{
	public class ConsoleLogger : ILogger {
	
		string		parameters;
		LoggerVerbosity	verbosity;
		WriteHandler	writeHandler;
		bool		skipProjectStartedText;
		ConsoleColor errorColor, warningColor, eventColor, messageColor, highMessageColor;
		ColorSetter colorSet;
		ColorResetter colorReset;
		IEventSource eventSource;
		bool no_message_color, use_colors;
		ConsoleLoggerParameter config = new ConsoleLoggerParameter ();		
		
		public ConsoleLogger ()
			: this (LoggerVerbosity.Normal, null, null, null)
		{
		}

		public ConsoleLogger (LoggerVerbosity verbosity)
			: this (verbosity, null, null, null)
		{
		}
		
		public ConsoleLogger (LoggerVerbosity verbosity,
				      WriteHandler write,
				      ColorSetter colorSet,
				      ColorResetter colorReset)
		{
			this.verbosity = verbosity;
			if (write == null)
				this.writeHandler += new WriteHandler (WriteHandlerFunction);
			else
				this.writeHandler += write;
			this.skipProjectStartedText = false;
			this.colorSet = colorSet;
			this.colorReset = colorReset;

			//defaults
			errorColor = ConsoleColor.DarkRed;
			warningColor = ConsoleColor.DarkYellow;
			eventColor = ConsoleColor.DarkCyan;
			messageColor = ConsoleColor.DarkGray;
			highMessageColor = ConsoleColor.White;

			// if message color is not set via the env var,
			// then don't use any color for it.
			no_message_color = true;

			use_colors = false;
			if (colorSet == null || colorReset == null)
				return;

			// color support
			string config = Environment.GetEnvironmentVariable ("XBUILD_COLORS");
			if (config == null) {
				use_colors = true;
				return;
			}

			if (config == "disable")
				return;

			use_colors = true;
			string [] pairs = config.Split (new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string pair in pairs) {
				string [] parts = pair.Split (new char[] {'='}, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length != 2)
					continue;

				if (parts [0] == "errors")
					TryParseConsoleColor (parts [1], ref errorColor);
				else if (parts [0] == "warnings")
					TryParseConsoleColor (parts [1], ref warningColor);
				else if (parts [0] == "events")
					TryParseConsoleColor (parts [1], ref eventColor);
				else if (parts [0] == "messages") {
					if (TryParseConsoleColor (parts [1], ref messageColor)) {
						highMessageColor = GetBrightColorFor (messageColor);
						no_message_color = false;
					}
				}
			}
		}
		
		private void WriteHandlerFunction (string message)
		{
			Console.WriteLine (message);
		}

		bool TryParseConsoleColor (string color_str, ref ConsoleColor color)
		{
			switch (color_str.ToLowerInvariant ()) {
			case "black": color = ConsoleColor.Black; break;

			case "blue": color = ConsoleColor.DarkBlue; break;
			case "green": color = ConsoleColor.DarkGreen; break;
			case "cyan": color = ConsoleColor.DarkCyan; break;
			case "red": color = ConsoleColor.DarkRed; break;
			case "magenta": color = ConsoleColor.DarkMagenta; break;
			case "yellow": color = ConsoleColor.DarkYellow; break;
			case "grey": color = ConsoleColor.DarkGray; break;

			case "brightgrey": color = ConsoleColor.Gray; break;
			case "brightblue": color = ConsoleColor.Blue; break;
			case "brightgreen": color = ConsoleColor.Green; break;
			case "brightcyan": color = ConsoleColor.Cyan; break;
			case "brightred": color = ConsoleColor.Red; break;
			case "brightmagenta": color = ConsoleColor.Magenta; break;
			case "brightyellow": color = ConsoleColor.Yellow; break;

			case "white":
			case "brightwhite": color = ConsoleColor.White; break;
			default: return false;
			}

			return true;
		}

		ConsoleColor GetBrightColorFor (ConsoleColor color)
		{
			switch (color) {
			case ConsoleColor.DarkBlue: return ConsoleColor.Blue;
			case ConsoleColor.DarkGreen: return ConsoleColor.Green;
			case ConsoleColor.DarkCyan: return ConsoleColor.Cyan;
			case ConsoleColor.DarkRed: return ConsoleColor.Red;
			case ConsoleColor.DarkMagenta: return ConsoleColor.Magenta;
			case ConsoleColor.DarkYellow: return ConsoleColor.Yellow;
			case ConsoleColor.DarkGray: return ConsoleColor.Gray;
			case ConsoleColor.Gray: return ConsoleColor.White;

			default: return color;
			}
		}
		
		class ConsoleLoggerParameter
		{
			public ConsoleLoggerParameter ()
			{
				ShowSummary = true;
			}
			public bool PerformanceSummary { get; set; }
			public bool ShowSummary { get; set; }
			public bool NoItemAndPropertyList { get; set; }
		}
		
		public void ApplyParameter (string parameterName,
					    string parameterValue)
		{
			switch (parameterName) {
				case "PerformanceSummary":
					config.PerformanceSummary = true;
					break;
				case "Summary":
					config.ShowSummary = true;
					break;
				case "NoSummary":
					config.ShowSummary = false;
					break;
				case "NoItemAndPropertyList":
					config.NoItemAndPropertyList = true;
					break;
				default:
					if (parameterName.StartsWith ("Verbosity="))
						ParseVerbosity (parameterName);
					break;
			}
		}

		void ParseVerbosity (string s)
		{
			string key, value;
			if (!TrySplitKeyValuePair (s, out key, out value))
				throw new LoggerException ("Unknown Verbosity, should be set as 'Verbosity=<verbosity>'");

			switch (value) {
			case "q":
			case "quiet":
				Verbosity = LoggerVerbosity.Quiet;
				break;
			case "m":
			case "minimal":
				Verbosity = LoggerVerbosity.Minimal;
				break;
			case "n":
			case "normal":
				Verbosity = LoggerVerbosity.Normal;
				break;
			case "d":
			case "detailed":
				Verbosity = LoggerVerbosity.Detailed;
				break;
			case "diag":
			case "diagnostic":
				Verbosity = LoggerVerbosity.Diagnostic;
				break;
			default:
				throw new LoggerException (String.Format ("Unknown verbosity - '{0}'", s));
			}
		}

		bool TrySplitKeyValuePair (string pair, out string key, out string value)
		{
			key = value = null;
			string[] parts = pair.Split ('=');
			if (parts.Length != 2)
				return false;

			key = parts [0];
			value = parts [1];
			return true;
		}

		public virtual void Initialize (IEventSource eventSource)
		{
			this.eventSource = eventSource;

			eventSource.BuildStarted += BuildStartedHandler;
			eventSource.BuildFinished += BuildFinishedHandler;

			eventSource.ProjectStarted += PushEvent;
			eventSource.ProjectFinished += PopEvent;

			eventSource.TargetStarted += PushEvent;
			eventSource.TargetFinished += PopEvent;

			eventSource.TaskStarted += PushEvent;
			eventSource.TaskFinished += PopEvent;

			eventSource.MessageRaised += MessageHandler;
			eventSource.WarningRaised += WarningHandler;
			eventSource.ErrorRaised += ErrorHandler;

			if (!String.IsNullOrEmpty (parameters))
				ParseParameters ();
		}
		
		ConcurrentDictionary<object,BuildRecord> build_records = new ConcurrentDictionary<object, BuildRecord> ();
		
		object dummy_key = new object ();
		
		BuildRecord GetBuildRecord (object sender)
		{
			BuildRecord r;
			// FIXME: our Microsoft.Build.Engine shouldn't give different "sender" object for each event
			// during the same build run. But it actually does.
			// It is problematic for parallel build because it is impossible to determine right "ongoing build"
			// record for the event without correct sender object.
			// Hence we expect sender as a valid object only if it is IBuildEngine4 -
			// only Microsoft.Build.Internal.BuildEngine4 implements it so far. 
			// (Used IBuildEngine3 because it needs to build for NET_4_0).
			var key = sender as IBuildEngine3 ?? dummy_key;
			return build_records.GetOrAdd (key, _ => new BuildRecord (this));
		}

		public void BuildStartedHandler (object sender, BuildStartedEventArgs e)
		{
			GetBuildRecord (sender).BuildStartedHandler (sender, e);
		}
		
		public void BuildFinishedHandler (object sender, BuildFinishedEventArgs e)
		{
			GetBuildRecord (sender).BuildFinishedHandler (e);
			((IDictionary) build_records).Remove (sender);
		}
		
		void PushEvent<T> (object sender, T args) where T: BuildStatusEventArgs
		{
			GetBuildRecord (sender).PushEvent (sender, args);
		}
		void PopEvent<T> (object sender, T args) where T: BuildStatusEventArgs
		{
			GetBuildRecord (sender).PopEvent (args);
		}
		public void ProjectStartedHandler (object sender, ProjectStartedEventArgs e)
		{
			GetBuildRecord (sender).ProjectStartedHandler (e);
		}
		public void ProjectFinishedHandler (object sender, ProjectFinishedEventArgs e)
		{
			GetBuildRecord (sender).ProjectFinishedHandler (e);
		}		
		public void TargetStartedHandler (object sender, TargetStartedEventArgs e)
		{
			GetBuildRecord (sender).TargetStartedHandler (e);
		}
		public void TargetFinishedHandler (object sender, TargetFinishedEventArgs e)
		{
			GetBuildRecord (sender).TargetFinishedHandler (e);
		}
		public void TaskStartedHandler (object sender, TaskStartedEventArgs e)
		{
			GetBuildRecord (sender).TaskStartedHandler (e);
		}
		public void TaskFinishedHandler (object sender, TaskFinishedEventArgs e)
		{
			GetBuildRecord (sender).TaskFinishedHandler (e);
		}
		public void MessageHandler (object sender, BuildMessageEventArgs e)
		{
			GetBuildRecord (sender).MessageHandler (e);
		}
		public void WarningHandler (object sender, BuildWarningEventArgs e)
		{
			GetBuildRecord (sender).WarningHandler (e);
		}
		public void ErrorHandler (object sender, BuildErrorEventArgs e)
		{
			GetBuildRecord (sender).ErrorHandler (e);
		}
		
		[MonoTODO]
		public void CustomEventHandler (object sender, CustomBuildEventArgs e)
		{
			build_records [sender].CustomHandler (e);
		}

		void SetColor (ConsoleColor color)
		{
			if (use_colors)
				colorSet (color);
		}

		void ResetColor ()
		{
			if (use_colors)
				colorReset ();
		}
		
		private void ParseParameters ()
		{
			string[] splittedParameters = parameters.Split (';');
			foreach (string s in splittedParameters )
				ApplyParameter (s, null);
		}
		
		public virtual void Shutdown ()
		{
			if (eventSource == null)
				return;

			eventSource.BuildStarted -= BuildStartedHandler;
			eventSource.BuildFinished -= BuildFinishedHandler;

			eventSource.ProjectStarted -= PushEvent;
			eventSource.ProjectFinished -= PopEvent;

			eventSource.TargetStarted -= PushEvent;
			eventSource.TargetFinished -= PopEvent;

			eventSource.TaskStarted -= PushEvent;
			eventSource.TaskFinished -= PopEvent;

			eventSource.MessageRaised -= MessageHandler;
			eventSource.WarningRaised -= WarningHandler;
			eventSource.ErrorRaised -= ErrorHandler;
		}

		public string Parameters {
			get {
				return parameters;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ();
				parameters = value;
				ParseParameters ();
			}
		}
		
		public bool ShowSummary {
			get { return config.ShowSummary; }
			set { config.ShowSummary = value; }
		}
		
		public bool SkipProjectStartedText {
			get { return skipProjectStartedText; }
			set { skipProjectStartedText = value; }
		}

		public LoggerVerbosity Verbosity {
			get { return verbosity;	}
			set { verbosity = value; }
		}

		protected WriteHandler WriteHandler {
			get { return writeHandler; }
			set { writeHandler = value; }
		}
		
		class BuildRecord
		{
			readonly ConsoleLogger parent;
			
			readonly List<BuildEvent> events;
			readonly Dictionary<string, List<string>> errorsTable;
			readonly Dictionary<string, List<string>> warningsTable;
			readonly SortedDictionary<string, PerfInfo> targetPerfTable, tasksPerfTable;
			List<string> errors, warnings;
			int		indent;
			int		errorCount;
			int		warningCount;
			bool		projectFailed;
			DateTime		buildStart;
	
			string current_events_string;
			
			ConsoleColor eventColor {
				get { return parent.eventColor; }
			}
			LoggerVerbosity verbosity {
				get { return parent.Verbosity; }
			}
			
			public BuildRecord (ConsoleLogger parent)
			{
				this.parent = parent;
				
				this.indent = 0;
				this.errorCount = 0;
				this.warningCount = 0;
				errors = new List<string> ();
				warnings = new List<string> ();
				events = new List<BuildEvent> ();
				errorsTable = new Dictionary<string, List<string>> ();
				warningsTable = new Dictionary<string, List<string>> ();
				targetPerfTable = new SortedDictionary<string, PerfInfo> ();
				tasksPerfTable = new SortedDictionary<string, PerfInfo> ();
			}
	
			internal void PushEvent<T> (object sender, T args) where T: BuildStatusEventArgs
			{
				BuildEvent be = new BuildEvent {
					Sender = sender,
					EventArgs = args,
					StartHandlerHasExecuted = false,
					ConsoleLogger = this.parent
				};
	
				events.Add (be);
				current_events_string = null;
			}
	
			void PopEvent<T> (object sender, T finished_args) where T: BuildStatusEventArgs
			{
				PopEvent (finished_args);
			}
	
			internal void PopEvent<T> (T finished_args) where T: BuildStatusEventArgs
			{
				if (events.Count == 0)
					throw new InvalidOperationException ("INTERNAL ERROR: Trying to pop from an empty events stack");
	
				BuildEvent be = events [events.Count - 1];
				if (parent.config.PerformanceSummary || verbosity == LoggerVerbosity.Diagnostic) {
					var args = be.EventArgs;
					TargetStartedEventArgs tgt_args = args as TargetStartedEventArgs;
					if (tgt_args != null) {
						AddPerfInfo (tgt_args.TargetName, args.Timestamp, targetPerfTable);
					} else {
						TaskStartedEventArgs tsk_args = args as TaskStartedEventArgs;
						if (tsk_args != null)
							AddPerfInfo (tsk_args.TaskName, args.Timestamp, tasksPerfTable);
					}
				}
	
				be.ExecuteFinishedHandler (finished_args);
				events.RemoveAt (events.Count - 1);
				current_events_string = null;
			}
	
			public void ResetBuildState ()
			{
				// Reset
				events.Clear ();
				errorsTable.Clear ();
				warningsTable.Clear ();
				targetPerfTable.Clear ();
				tasksPerfTable.Clear ();
				errors.Clear ();
				warnings.Clear ();
	
				indent = 0;
				errorCount = 0;
				warningCount = 0;
				projectFailed = false;
			}
	
			void AddPerfInfo (string name, DateTime start, IDictionary<string, PerfInfo> perf_table)
			{
				PerfInfo pi;
				if (!perf_table.TryGetValue (name, out pi)) {
					pi = new PerfInfo ();
					perf_table [name] = pi;
				}
	
				pi.Time += DateTime.Now - start;
				pi.NumberOfCalls ++;
			}
	
			public void BuildStartedHandler (object sender, BuildStartedEventArgs args)
			{
				if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) {
					WriteLine (String.Empty);
					WriteLine (String.Format ("Build started {0}.", args.Timestamp));
					WriteLine ("__________________________________________________");
				}
				buildStart = args.Timestamp;
	
				PushEvent (sender, args);
			}
			
			public void BuildFinishedHandler (BuildFinishedEventArgs args)
			{
				BuildFinishedHandlerActual (args);
				
				ResetBuildState ();
			}
	
			void BuildFinishedHandlerActual (BuildFinishedEventArgs args)
			{
				if (!IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) {
					PopEvent (args);
					return;
				}
	
				TimeSpan timeElapsed = args.Timestamp - buildStart;
				if (parent.config.PerformanceSummary || verbosity == LoggerVerbosity.Diagnostic)
					DumpPerformanceSummary ();
	
				if (args.Succeeded == true && !projectFailed) {
					WriteLine ("Build succeeded.");
				} else {
					WriteLine ("Build FAILED.");
				}
				if (warnings.Count > 0) {
					WriteLine (Environment.NewLine + "Warnings:");
					SetColor (parent.warningColor);
	
					WriteLine (String.Empty);
					foreach (KeyValuePair<string, List<string>> pair in warningsTable) {
						if (!String.IsNullOrEmpty (pair.Key))
							WriteLine (pair.Key);
	
						string indent_str = String.IsNullOrEmpty (pair.Key) ? String.Empty : "\t";
						foreach (string msg in pair.Value)
							WriteLine (String.Format ("{0}{1}", indent_str, msg));
	
						WriteLine (String.Empty);
					}
	
					ResetColor ();
				}
	
				if (errors.Count > 0) {
					WriteLine ("Errors:");
					SetColor (parent.errorColor);
	
					WriteLine (String.Empty);
					foreach (KeyValuePair<string, List<string>> pair in errorsTable) {
						if (!String.IsNullOrEmpty (pair.Key))
							WriteLine (pair.Key);
	
						string indent_str = String.IsNullOrEmpty (pair.Key) ? String.Empty : "\t";
						foreach (string msg in pair.Value)
							WriteLine (String.Format ("{0}{1}", indent_str, msg));
	
						WriteLine (String.Empty);
					}
					ResetColor ();
				}
	
				if (parent.ShowSummary == true){
					WriteLine (String.Format ("\t {0} Warning(s)", warningCount));
					WriteLine (String.Format ("\t {0} Error(s)", errorCount));
					WriteLine (String.Empty);
					WriteLine (String.Format ("Time Elapsed {0}", timeElapsed));
				}
	
				PopEvent (args);
			}
	
			public void ProjectStartedHandler (ProjectStartedEventArgs args)
			{
				if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) {
					SetColor (eventColor);
					WriteLine (String.Format ("Project \"{0}\" ({1} target(s)):", args.ProjectFile,
								String.IsNullOrEmpty (args.TargetNames) ? "default" : args.TargetNames));
					ResetColor ();
					DumpProperties (args.Properties);
					DumpItems (args.Items);
				}
			}
			
			public void ProjectFinishedHandler (ProjectFinishedEventArgs args)
			{
				if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) {
					if (indent == 1)
						indent --;
					SetColor (eventColor);
					WriteLine (String.Format ("Done building project \"{0}\".{1}", args.ProjectFile,
								args.Succeeded ? String.Empty : "-- FAILED"));
					ResetColor ();
					WriteLine (String.Empty);
				}
				if (!projectFailed)
					// no project has failed yet, so update the flag
					projectFailed = !args.Succeeded;
			}
			
			public void TargetStartedHandler (TargetStartedEventArgs args)
			{
				if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) {
					indent++;
					SetColor (eventColor);
					WriteLine (String.Empty);
					WriteLine (String.Format ("Target {0}:",args.TargetName));
					ResetColor ();
				}
			}
			
			public void TargetFinishedHandler (TargetFinishedEventArgs args)
			{
				if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Detailed) ||
						(!args.Succeeded && IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal))) {
					SetColor (eventColor);
					WriteLine (String.Format ("Done building target \"{0}\" in project \"{1}\".{2}",
						args.TargetName, args.ProjectFile,
						args.Succeeded ? String.Empty : "-- FAILED"));
					ResetColor ();
					WriteLine (String.Empty);
				}
				indent--;
			}
			
			public void TaskStartedHandler (TaskStartedEventArgs args)
			{
				if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Detailed)) {
					SetColor (eventColor);
					WriteLine (String.Format ("Task \"{0}\"",args.TaskName));
					ResetColor ();
				}
				indent++;
			}
			
			public void TaskFinishedHandler (TaskFinishedEventArgs args)
			{
				indent--;
				if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Detailed) ||
						(!args.Succeeded && IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal))) {
					SetColor (eventColor);
					if (args.Succeeded)
						WriteLine (String.Format ("Done executing task \"{0}\"", args.TaskName));
					else
						WriteLine (String.Format ("Task \"{0}\" execution -- FAILED", args.TaskName));
					ResetColor ();
				}
			}
			
			public void MessageHandler (BuildMessageEventArgs args)
			{
				if (IsMessageOk (args)) {
					if (parent.no_message_color) {
						ExecutePendingEventHandlers ();
						WriteLine (args.Message);
					} else {
						ExecutePendingEventHandlers ();
						SetColor (args.Importance == MessageImportance.High ? parent.highMessageColor : parent.messageColor);
						WriteLine (args.Message);
						ResetColor ();
					}
				}
			}
			
			public void WarningHandler (BuildWarningEventArgs args)
			{
				string msg = FormatWarningEvent (args);
				if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Quiet)) {
					ExecutePendingEventHandlers ();
					SetColor (parent.warningColor);
					WriteLineWithoutIndent (msg);
					ResetColor ();
				}
				warnings.Add (msg);
	
				List<string> list = null;
				if (!warningsTable.TryGetValue (EventsAsString, out list))
					warningsTable [EventsAsString] = list = new List<string> ();
				list.Add (msg);
	
				warningCount++;
			}
			
			public void ErrorHandler (BuildErrorEventArgs args)
			{
				string msg = FormatErrorEvent (args);
				if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Quiet)) {
					ExecutePendingEventHandlers ();
					SetColor (parent.errorColor);
					WriteLineWithoutIndent (msg);
					ResetColor ();
				}
				errors.Add (msg);
	
				List<string> list = null;
				if (!errorsTable.TryGetValue (EventsAsString, out list))
					errorsTable [EventsAsString] = list = new List<string> ();
				list.Add (msg);
				errorCount++;
			}
			
			public void CustomHandler (CustomBuildEventArgs args)
			{
			}
			
			private bool IsVerbosityGreaterOrEqual (LoggerVerbosity v)
			{
				if (v == LoggerVerbosity.Diagnostic) {
					return LoggerVerbosity.Diagnostic <= verbosity;
				} else if (v == LoggerVerbosity.Detailed) {
					return LoggerVerbosity.Detailed <= verbosity;
				} else if (v == LoggerVerbosity.Normal) {
					return LoggerVerbosity.Normal <= verbosity;
				} else if (v == LoggerVerbosity.Minimal) {
					return LoggerVerbosity.Minimal <= verbosity;
				} else if (v == LoggerVerbosity.Quiet) {
					return true;
				} else
					return false;
			}
	
			void DumpItems (IEnumerable items)
			{
				if (parent.config.NoItemAndPropertyList || !IsVerbosityGreaterOrEqual (LoggerVerbosity.Diagnostic) || items == null)
					return;
	
				SetColor (eventColor);
				WriteLine (String.Empty);
				WriteLine ("Initial Items:");
				ResetColor ();
				if (items == null)
					return;
	
				var items_table = new SortedDictionary<string, List<ITaskItem>> ();
				foreach (DictionaryEntry de in items) {
					string key = (string)de.Key;
					if (!items_table.ContainsKey (key))
						items_table [key] = new List<ITaskItem> ();
	
					items_table [key].Add ((ITaskItem) de.Value);
				}
	
				foreach (string name in items_table.Keys) {
					WriteLine (name);
					indent ++;
					foreach (ITaskItem item in items_table [name])
						WriteLine (item.ItemSpec);
					indent--;
				}
			}
	
			string EventsAsString {
				get {
					if (current_events_string == null)
						current_events_string = EventsToString ();
					return current_events_string;
				}
			}
			
			private bool IsMessageOk (BuildMessageEventArgs bsea)
			{
				if (bsea.Importance == MessageImportance.High && IsVerbosityGreaterOrEqual (LoggerVerbosity.Minimal)) {
					return true;
				} else if (bsea.Importance == MessageImportance.Normal && IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) {
					return true;
				} else if (bsea.Importance == MessageImportance.Low && IsVerbosityGreaterOrEqual (LoggerVerbosity.Detailed)) {
					return true;
				} else
					return false;
			}
	
			void DumpProperties (IEnumerable properties)
			{
				if (parent.config.NoItemAndPropertyList || !IsVerbosityGreaterOrEqual (LoggerVerbosity.Diagnostic))
					return;
	
				SetColor (eventColor);
				WriteLine (String.Empty);
				WriteLine ("Initial Properties:");
				ResetColor ();
	
				if (properties == null)
					return;
	
				var dict = new SortedDictionary<string, string> ();
				foreach (DictionaryEntry de in properties)
					dict [(string)de.Key] = (string)de.Value;
	
				foreach (KeyValuePair<string, string> pair in dict)
					WriteLine (String.Format ("{0} = {1}", pair.Key, pair.Value));
			}
	
			private void WriteLine (string message)
			{
				if (indent > 0) {
					StringBuilder sb = new StringBuilder ();
					for (int i = 0; i < indent; i++)
						sb.Append ('\t');
	
					string indent_str = sb.ToString ();
	
					foreach (string line in message.Split (new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
						parent.writeHandler (indent_str + line);
				} else {
					parent.writeHandler (message);
				}
			}
	
			void ExecutePendingEventHandlers ()
			{
				foreach (var be in events)
					be.ExecuteStartedHandler ();
			}
	
			string EventsToString ()
			{
				StringBuilder sb = new StringBuilder ();
	
				string last_imported_target_file = String.Empty;
				for (int i = 0; i < events.Count; i ++) {
					var args = events [i].EventArgs;
					ProjectStartedEventArgs pargs = args as ProjectStartedEventArgs;
					if (pargs != null) {
						sb.AppendFormat ("{0} ({1}) ->\n", pargs.ProjectFile,
								String.IsNullOrEmpty (pargs.TargetNames) ?
									"default targets" :
									pargs.TargetNames);
						last_imported_target_file = String.Empty;
						continue;
					}
	
					TargetStartedEventArgs targs = args as TargetStartedEventArgs;
					if (targs != null) {
						if (targs.TargetFile != targs.ProjectFile && targs.TargetFile != last_imported_target_file)
							// target from an imported file,
							// and it hasn't been mentioned as yet
							sb.AppendFormat ("{0} ", targs.TargetFile);
	
						last_imported_target_file = targs.TargetFile;
						sb.AppendFormat ("({0} target) ->\n", targs.TargetName);
					}
				}
	
				return sb.ToString ();
			}
	
			void DumpPerformanceSummary ()
			{
				SetColor (eventColor);
				WriteLine ("Target performance summary:");
				ResetColor ();
	
				foreach (var pi in targetPerfTable.OrderBy (pair => pair.Value.Time))
					WriteLine (String.Format ("{0,10:0.000} ms  {1,-50}  {2,5} calls", pi.Value.Time.TotalMilliseconds, pi.Key, pi.Value.NumberOfCalls));
	
				WriteLine (String.Empty);
	
				SetColor (eventColor);
				WriteLine ("Tasks performance summary:");
				ResetColor ();
	
				foreach (var pi in tasksPerfTable.OrderBy (pair => pair.Value.Time))
					WriteLine (String.Format ("{0,10:0.000} ms  {1,-50}  {2,5} calls", pi.Value.Time.TotalMilliseconds, pi.Key, pi.Value.NumberOfCalls));
	
				WriteLine (String.Empty);
			}
			
			private string FormatErrorEvent (BuildErrorEventArgs args)
			{
				// For some reason we get an 1-char empty string as Subcategory somtimes.
				string subprefix = args.Subcategory == null || args.Subcategory == "" || args.Subcategory == " " ? "" : " ";
				string subcat = subprefix == "" ? "" : args.Subcategory;
					
				if (args.LineNumber != 0){
					if (args.ColumnNumber != 0 && !InEmacs) 
						return String.Format ("{0}({1},{2}): {3}{4}error {5}: {6}",
								      args.File, args.LineNumber, args.ColumnNumber,
								      subprefix, subcat, args.Code, args.Message);
	
					return String.Format ("{0}({1}): {2}{3}error {4}: {5}",
							      args.File, args.LineNumber,
							      subprefix, subcat, args.Code, args.Message);
				} else {
					return String.Format ("{0}: {1}{2}error {3}: {4}", args.File, subprefix, subcat, args.Code,
						args.Message);
				}
			}
	
			static bool InEmacs = Environment.GetEnvironmentVariable ("EMACS") == "t";
	
			private string FormatWarningEvent (BuildWarningEventArgs args)
			{
				// For some reason we get an 1-char empty string as Subcategory somtimes.
				string subprefix = args.Subcategory == null || args.Subcategory == "" || args.Subcategory == " " ? "" : " ";
				string subcat = subprefix == "" ? "" : args.Subcategory;
	
				// FIXME: show more complicated args
				if (args.LineNumber != 0){
	
					if (args.ColumnNumber != 0 && !InEmacs) {
						return String.Format ("{0}({1},{2}): {3}{4}warning {5}: {6}",
								      args.File, args.LineNumber, args.ColumnNumber,
								      subprefix, subcat, args.Code, args.Message);
					}
					return String.Format ("{0}({1}): {2}{3}warning {4}: {5}",
							      args.File, args.LineNumber,
							      subprefix, subcat, args.Code, args.Message);
				} else {
					return String.Format ("{0}: {1} warning {2}: {3}", args.File, args.Subcategory, args.Code,
						args.Message);
				}
			}
	
			void SetColor (ConsoleColor color)
			{
				if (parent.use_colors)
					parent.colorSet (color);
			}
	
			void ResetColor ()
			{
				if (parent.use_colors)
					parent.colorReset ();
			}
			
			private void WriteLineWithoutIndent (string message)
			{
				parent.writeHandler (message);
			}
		}
	}

	class BuildEvent {
		public object Sender; 
		public BuildStatusEventArgs EventArgs;
		public bool StartHandlerHasExecuted;
		public ConsoleLogger ConsoleLogger;

		public void ExecuteStartedHandler ()
		{
			if (StartHandlerHasExecuted)
				return;

			if (EventArgs is ProjectStartedEventArgs)
				ConsoleLogger.ProjectStartedHandler (Sender, (ProjectStartedEventArgs)EventArgs);
			else if (EventArgs is TargetStartedEventArgs)
				ConsoleLogger.TargetStartedHandler (Sender, (TargetStartedEventArgs)EventArgs);
			else if (EventArgs is TaskStartedEventArgs)
				ConsoleLogger.TaskStartedHandler (Sender, (TaskStartedEventArgs)EventArgs);
			else if (!(EventArgs is BuildStartedEventArgs))
				throw new InvalidOperationException ("Unexpected event on the stack, type: " + EventArgs.GetType ());

			StartHandlerHasExecuted = true;
		}

		public void ExecuteFinishedHandler (BuildStatusEventArgs finished_args)
		{
			if (!StartHandlerHasExecuted)
				return;

			if (EventArgs is ProjectStartedEventArgs) {
				var pfa = finished_args as ProjectFinishedEventArgs;
				// FIXME: BuildFinishedHandlerActual sends us BuildFinishedEventArgs via PopEvent
				if (pfa == null)
					return;

				ConsoleLogger.ProjectFinishedHandler (Sender, pfa);
			} else if (EventArgs is TargetStartedEventArgs) {
				var fa = finished_args as TargetFinishedEventArgs;
				// FIXME: BuildFinishedHandlerActual sends us BuildFinishedEventArgs via PopEvent
				if (fa == null)
					return;

				ConsoleLogger.TargetFinishedHandler (Sender, fa);
			} else if (EventArgs is TaskStartedEventArgs) {
				// FIXME: BuildFinishedHandlerActual sends us BuildFinishedEventArgs via PopEvent
				if (!(finished_args is TaskFinishedEventArgs))
					return;

				ConsoleLogger.TaskFinishedHandler (Sender, (TaskFinishedEventArgs) finished_args);
			} else if (!(EventArgs is BuildStartedEventArgs))
				throw new InvalidOperationException ("Unexpected event on the stack, type: " + EventArgs.GetType ());
		}
	}

	class PerfInfo {
		public TimeSpan Time;
		public int NumberOfCalls;
	}
}
