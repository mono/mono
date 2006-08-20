//
// System.Diagnostics.LocalFileEventLog.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//   Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
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
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.Diagnostics
{
	internal class LocalFileEventLog : EventLogImpl
	{
		const string DateFormat = "yyyyMMddHHmmssfff";
		static readonly object lockObject = new object ();

		public LocalFileEventLog (EventLog coreEventLog) : base (coreEventLog)
		{
		}

		public override void BeginInit () {
		}

		public override void Clear ()
		{
			string logDir = FindLogStore (CoreEventLog.Log);
			if (!Directory.Exists (logDir))
				return;

			foreach (string file in Directory.GetFiles (logDir, "*.log"))
				File.Delete (file);
		}

		public override void Close ()
		{
			// we don't hold any unmanaged resources
		}

		public override void CreateEventSource (EventSourceCreationData sourceData)
		{
			// construct path for storing log entries
			string logDir = FindLogStore (sourceData.LogName);
			// create event log store (if necessary), and modify access
			// permissions (unix only)
			CreateLogStore (logDir, sourceData.LogName);
			// create directory for event source, so we can check if the event
			// source already exists
			string sourceDir = Path.Combine (logDir, sourceData.Source);
			Directory.CreateDirectory (sourceDir);
		}

		public override void Delete (string logName, string machineName)
		{
			string logDir = FindLogStore (logName);
			if (!Directory.Exists (logDir))
				throw new InvalidOperationException (string.Format (
					CultureInfo.InvariantCulture, "Event Log '{0}'"
					+ " does not exist on computer '{1}'.", logName,
					machineName));

			Directory.Delete (logDir, true);
		}

		public override void DeleteEventSource (string source, string machineName)
		{
			if (!Directory.Exists (EventLogStore))
				throw new ArgumentException (string.Format (
					CultureInfo.InvariantCulture, "The source '{0}' is not"
					+ " registered on computer '{1}'.", source, machineName));

			string sourceDir = FindSourceDirectory (source);
			if (sourceDir == null)
				throw new ArgumentException (string.Format (
					CultureInfo.InvariantCulture, "The source '{0}' is not"
					+ " registered on computer '{1}'.", source, machineName));
			Directory.Delete (sourceDir);
		}

		public override void Dispose (bool disposing)
		{
			Close ();
		}

		public override void EndInit () { }

		public override bool Exists (string logName, string machineName)
		{
			string logDir = FindLogStore (logName);
			return Directory.Exists (logDir);
		}

		[MonoTODO ("Use MessageTable from PE for lookup")]
		protected override string FormatMessage (string source, uint eventID, string [] replacementStrings)
		{
			return string.Join (", ", replacementStrings);
		}

		protected override int GetEntryCount ()
		{
			string logDir = FindLogStore (CoreEventLog.Log);
			if (!Directory.Exists (logDir))
				return 0;

			string[] logFiles = Directory.GetFiles (logDir, "*.log");
			return logFiles.Length;
		}

		protected override EventLogEntry GetEntry (int index)
		{
			string logDir = FindLogStore (CoreEventLog.Log);

			// our file names are one-based
			string file = Path.Combine (logDir, (index + 1).ToString (
				CultureInfo.InvariantCulture) + ".log");

			using (TextReader tr = File.OpenText (file)) {
				int eventIndex = int.Parse (Path.GetFileNameWithoutExtension (file),
					CultureInfo.InvariantCulture);
				uint instanceID = uint.Parse (tr.ReadLine ().Substring (12),
					CultureInfo.InvariantCulture);
				EventLogEntryType type = (EventLogEntryType)
					Enum.Parse (typeof (EventLogEntryType), tr.ReadLine ().Substring (11));
				string source = tr.ReadLine ().Substring (8);
				string category = tr.ReadLine ().Substring (10);
				short categoryNumber = short.Parse(category, CultureInfo.InvariantCulture);
				string categoryName = "(" + category + ")";
				DateTime timeGenerated = DateTime.ParseExact (tr.ReadLine ().Substring (15),
					DateFormat, CultureInfo.InvariantCulture);
				DateTime timeWritten = File.GetLastWriteTime (file);
				int stringNums = int.Parse (tr.ReadLine ().Substring (20));
				ArrayList replacementTemp = new ArrayList ();
				StringBuilder sb = new StringBuilder ();
				while (replacementTemp.Count < stringNums) {
					char c = (char) tr.Read ();
					if (c == '\0') {
						replacementTemp.Add (sb.ToString ());
						sb.Length = 0;
					} else {
						sb.Append (c);
					}
				}
				string [] replacementStrings = new string [replacementTemp.Count];
				replacementTemp.CopyTo (replacementStrings, 0);

				string message = FormatMessage (source, instanceID, replacementStrings);
				int eventID = EventLog.GetEventID (instanceID);

				byte [] bin = Convert.FromBase64String (tr.ReadToEnd ());
				return new EventLogEntry (categoryName, categoryNumber, eventIndex,
					eventID, source, message, null, Environment.MachineName,
					type, timeGenerated, timeWritten, bin, replacementStrings,
					instanceID);
			}
		}

		public override EventLog [] GetEventLogs (string machineName)
		{
			if (!Directory.Exists (EventLogStore))
				return new EventLog [0];
			
			string [] logDirs = Directory.GetDirectories (EventLogStore, "*");
			EventLog [] eventLogs = new EventLog [logDirs.Length];
			for (int i = 0; i < logDirs.Length; i++) {
				EventLog eventLog = new EventLog (Path.GetFileName (
					logDirs [i]), machineName);
				eventLogs [i] = eventLog;
			}
			return eventLogs;
		}

		[MonoTODO]
		protected override string GetLogDisplayName ()
		{
			return CoreEventLog.Log;
		}

		public override string	LogNameFromSourceName (string source, string machineName)
		{
			if (!Directory.Exists (EventLogStore))
				return string.Empty;

			string sourceDir = FindSourceDirectory (source);
			if (sourceDir == null)
				return string.Empty;
			DirectoryInfo info = new DirectoryInfo (sourceDir);
			return info.Parent.Name;
		}

		public override bool SourceExists (string source, string machineName)
		{
			if (!Directory.Exists (EventLogStore))
				return false;
			string sourceDir = FindSourceDirectory (source);
			return (sourceDir != null);
		}

		public override void WriteEntry (string [] replacementStrings, EventLogEntryType type, uint instanceID, short category, byte [] rawData)
		{
			lock (lockObject) {
				string logDir = FindLogStore (CoreEventLog.Log);

				int index = GetNewIndex ();
				string logPath = Path.Combine (logDir, index.ToString (CultureInfo.InvariantCulture) + ".log");
				try {
					using (TextWriter w = File.CreateText (logPath)) {
#if NET_2_0
						w.WriteLine ("InstanceID: {0}", instanceID.ToString (CultureInfo.InvariantCulture));
#else
						w.WriteLine ("InstanceID: {0}", instanceID.ToString (CultureInfo.InvariantCulture));
#endif
						w.WriteLine ("EntryType: {0}", (int) type);
						w.WriteLine ("Source: {0}", CoreEventLog.Source);
						w.WriteLine ("Category: {0}", category.ToString (CultureInfo.InvariantCulture));
						w.WriteLine ("TimeGenerated: {0}", DateTime.Now.ToString (
							DateFormat, CultureInfo.InvariantCulture));
						w.WriteLine ("ReplacementStrings: {0}", replacementStrings.
							Length.ToString (CultureInfo.InvariantCulture));
						StringBuilder sb = new StringBuilder ();
						for (int i = 0; i < replacementStrings.Length; i++) {
							string replacement = replacementStrings [i];
							sb.Append (replacement);
							sb.Append ('\0');
						}
						w.Write (sb.ToString ());
						w.Write (Convert.ToBase64String (rawData));
					}
				} catch (IOException) {
					File.Delete (logPath);
				}
			}
		}

		private string FindSourceDirectory (string source)
		{
			string sourceDir = null;

			string [] logDirs = Directory.GetDirectories (EventLogStore, "*");
			for (int i = 0; i < logDirs.Length; i++) {
				string [] sourceDirs = Directory.GetDirectories (logDirs [i], "*");
				for (int j = 0; j < sourceDirs.Length; j++) {
					string relativeDir = Path.GetFileName (sourceDirs [j]);
					// use a case-insensitive comparison
					if (string.Compare (relativeDir, source, true, CultureInfo.InvariantCulture) == 0) {
						sourceDir = sourceDirs [j];
						break;
					}
				}
			}
			return sourceDir;
		}

		private bool RunningOnLinux {
			get {
				return ((int) Environment.OSVersion.Platform == 4 ||
#if NET_2_0
					Environment.OSVersion.Platform == PlatformID.Unix);
#else
					(int) Environment.OSVersion.Platform == 128);
#endif
			}
		}

		private string FindLogStore (string logName) {
			// we'll use a case-insensitive lookup to match the MS behaviour
			// while still allowing the original casing of the log name to be
			// retained
			string [] logDirs = Directory.GetDirectories (EventLogStore, "*");
			for (int i = 0; i < logDirs.Length; i++) {
				string relativeDir = Path.GetFileName (logDirs [i]);
				// use a case-insensitive comparison
				if (string.Compare (relativeDir, logName, true, CultureInfo.InvariantCulture) == 0) {
					return logDirs [i];
				}
			}

			return Path.Combine (EventLogStore, logName);
		}

		private string EventLogStore {
			get {
				// for the local file implementation, the MONO_EVENTLOG_TYPE
				// environment variable can contain the path of the event log
				// store by using the following syntax: local:<path>
				string eventLogType = Environment.GetEnvironmentVariable (EventLog.EVENTLOG_TYPE_VAR);
				if (eventLogType != null && eventLogType.Length > EventLog.LOCAL_FILE_IMPL.Length + 1)
					return eventLogType.Substring (EventLog.LOCAL_FILE_IMPL.Length + 1);
				if (RunningOnLinux) {
					return "/var/lib/mono/eventlog";
				} else {
					return Path.Combine (Environment.GetFolderPath (
						Environment.SpecialFolder.CommonApplicationData),
						"mono/eventlog");
				}
			}
		}

		private void CreateLogStore (string logDir, string logName)
		{
			if (!Directory.Exists (logDir)) {
				Directory.CreateDirectory (logDir);
				// MS does not allow an event source to be named after an already
				// existing event log. To speed up checking whether a given event
				// source already exists (either as a event source or event log)
				// we create an event source directory named after the event log.
				// This matches what MS does with the registry-based registration.
				Directory.CreateDirectory (Path.Combine (logDir, logName));
				if (RunningOnLinux) {
					ModifyAccessPermissions (logDir, "777");
					ModifyAccessPermissions (logDir, "+t");
				}
			}
		}

		private int GetNewIndex () {
			// our file names are one-based
			int maxIndex = 0;
			string[] logFiles = Directory.GetFiles (FindLogStore (CoreEventLog.Log), "*.log");
			for (int i = 0; i < logFiles.Length; i++) {
				try {
					string file = logFiles[i];
					int index = int.Parse (Path.GetFileNameWithoutExtension (
						file), CultureInfo.InvariantCulture);
					if (index > maxIndex)
						maxIndex = index;
				} catch {
				}
			}
			return ++maxIndex;
		}

		private static void ModifyAccessPermissions (string path, string permissions)
		{
			ProcessStartInfo pi = new ProcessStartInfo ();
			pi.FileName = "chmod";
			pi.RedirectStandardOutput = true;
			pi.RedirectStandardError = true;
			pi.UseShellExecute = false;
			pi.Arguments = string.Format ("{0} \"{1}\"", permissions, path);

			Process p = null;
			try {
				p = Process.Start (pi);
			} catch (Exception ex) {
				throw new SecurityException ("Access permissions could not be modified.", ex);
			}

			p.WaitForExit ();
			if (p.ExitCode != 0) {
				p.Close ();
				throw new SecurityException ("Access permissions could not be modified.");
			}
			p.Close ();
		}
	}
}
