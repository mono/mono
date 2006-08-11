//
// System.Diagnostics.EventLog.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002
// (C) 2003 Andreas Nahr
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
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;

using Microsoft.Win32;

namespace System.Diagnostics 
{
	[DefaultEvent ("EntryWritten")]
	[InstallerType (typeof (EventLogInstaller))]
	[Designer ("Microsoft.VisualStudio.Install.EventLogInstallableComponentDesigner, " + Consts.AssemblyMicrosoft_VisualStudio)]
	public class EventLog : Component, ISupportInitialize 
	{
		private string source;
		private string logName;
		private string machineName;
		private bool doRaiseEvents = false;
		private ISynchronizeInvoke synchronizingObject = null;

		private EventLogImpl Impl;

		public EventLog()
			: this (string.Empty)
		{
		}

		public EventLog(string logName)
			: this (logName, ".")
		{
		}

		public EventLog(string logName, string machineName)
			: this (logName, machineName, string.Empty)
		{
		}

		public EventLog(string logName, string machineName, string source)
		{
			if (logName == null) {
				throw new ArgumentNullException ("logName");
			}
			if (machineName == null || machineName.Length == 0)
				throw new ArgumentException (string.Format (
					CultureInfo.InvariantCulture, "Invalid value '{0}' for"
					+ " parameter 'machineName'.", machineName));

			this.source = source;
			this.machineName = machineName;
			this.logName = logName;

			if (Win32EventLogEnabled) {
				Impl = new Win32EventLog (this);
			} else {
				Impl = new UnixEventLog (this);
			}
			Impl.EntryWritten += new EntryWrittenEventHandler (EntryWrittenHandler);
		}

		private void EntryWrittenHandler (object sender, EntryWrittenEventArgs e)
		{
			if (doRaiseEvents)
				OnEntryWritten (e.Entry);
		}

		[Browsable (false), DefaultValue (false)]
		[MonitoringDescription ("If enabled raises event when a log is written.")]
		public bool EnableRaisingEvents {
			get {return doRaiseEvents;}
			set {doRaiseEvents = value;}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The entries in the log.")]
		public EventLogEntryCollection Entries {
			get {return new EventLogEntryCollection(Impl);}
		}

		[ReadOnly (true), DefaultValue (""), RecommendedAsConfigurable (true)]
		[TypeConverter ("System.Diagnostics.Design.LogConverter, " + Consts.AssemblySystem_Design)]
		[MonitoringDescription ("Name of the log that is read and written.")]
		public string Log {
			get {
				if (source != null && source.Length > 0)
					return GetLogName ();
				return logName;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				logName = value;
			}
		}

		[Browsable (false)]
		public string LogDisplayName {
			get {return Impl.LogDisplayName;}
		}

		[ReadOnly (true), DefaultValue ("."), RecommendedAsConfigurable (true)]
		[MonitoringDescription ("Name of the machine that this log get written to.")]
		public string MachineName {
			get {return machineName;}
			set {machineName = value;}
		}

		[ReadOnly (true), DefaultValue (""), RecommendedAsConfigurable (true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[MonitoringDescription ("The application name that writes the log.")]
		public string Source {
			get { return source; }
			set { source = (value == null) ? string.Empty : value; }
		}

		[Browsable (false), DefaultValue (null)]
		[MonitoringDescription ("An object that synchronizes event handler calls.")]
		public ISynchronizeInvoke SynchronizingObject {
			get {return synchronizingObject;}
			set {synchronizingObject = value;}
		}

		public void BeginInit()
		{
			Impl.BeginInit();
		}

		public void Clear()
		{
			Impl.Clear();
		}

		public void Close()
		{
			Impl.Close();
		}

		public static void CreateEventSource(string source, string logName)
		{
			CreateEventSource (source, logName, ".");
		}

		public static void CreateEventSource(string source, 
			string logName, 
			string machineName)
		{
			CreateEventSource (new EventSourceCreationData (source, logName,
				machineName));
		}

#if NET_2_0
		[MonoTODO ("Support remote machine")]
		public
#else
		private
#endif
		static void CreateEventSource (EventSourceCreationData sourceData)
		{
			if (sourceData.Source == null || sourceData.Source.Length == 0) {
				throw new ArgumentException ("Source is not set");
			}
			if (sourceData.LogName == null || sourceData.LogName.Length == 0) {
				throw new ArgumentException ("LogName is not set");
			}

			if (SourceExists (sourceData.Source, sourceData.MachineName)) {
				throw new ArgumentException (string.Format (CultureInfo.InvariantCulture,
					"Source '{0}' already exists on '{1}'.", sourceData.Source,
					sourceData.MachineName));
			}

			using (RegistryKey eventLogKey = GetEventLogKey (sourceData.MachineName, true)) {
				if (eventLogKey == null)
					throw new InvalidOperationException ("EventLog registry key is missing.");

				bool logKeyCreated = false;
				RegistryKey logKey = null;
				try {
					logKey = eventLogKey.OpenSubKey (sourceData.LogName, true);
					if (logKey == null) {
						logKey = eventLogKey.CreateSubKey (sourceData.LogName);
						logKey.SetValue ("Sources", new string[] { sourceData.LogName,
							sourceData.Source });
						UpdateLogRegistry (logKey);

						using (RegistryKey sourceKey = logKey.CreateSubKey (sourceData.LogName)) {
							UpdateSourceRegistry (sourceKey, sourceData);
						}

						logKeyCreated = true;
					}

					if (sourceData.LogName != sourceData.Source) {
						if (!logKeyCreated) {
							string[] sources = (string[]) logKey.GetValue ("Sources");
							if (sources == null) {
								logKey.SetValue ("Sources", new string[] { sourceData.LogName,
									sourceData.Source });
							} else {
								bool found = false;
								for (int i = 0; i < sources.Length; i++) {
									if (sources[i] == sourceData.Source) {
										found = true;
										break;
									}
								}
								if (!found) {
									string[] newSources = new string[sources.Length + 1];
									Array.Copy (sources, 0, newSources, 0, sources.Length);
									newSources[sources.Length] = sourceData.Source;
									logKey.SetValue ("Sources", newSources);
								}
							}
						}
						using (RegistryKey sourceKey = logKey.CreateSubKey (sourceData.Source)) {
							UpdateSourceRegistry (sourceKey, sourceData);
						}
					}
				} finally {
					if (logKey != null)
						logKey.Close ();
				}
			}
		}

		public static void Delete(string logName)
		{
			Delete (logName, ".");
		}

		[MonoTODO ("Support remote machine")]
		public static void Delete (string logName, string machineName)
		{
			if (machineName == null || machineName.Length == 0)
				throw new ArgumentException ("Invalid format for argument"
					+ " machineName.");

			if (logName == null || logName.Length == 0)
				throw new ArgumentException ("Log to delete was not specified.");

			using (RegistryKey eventLogKey = GetEventLogKey (machineName, true)) {
				if (eventLogKey == null)
					throw new InvalidOperationException ("The event log key does not exist.");

				using (RegistryKey logKey = eventLogKey.OpenSubKey (logName, false)) {
					if (logKey == null)
						throw new InvalidOperationException (string.Format (
							CultureInfo.InvariantCulture, "Event Log '{0}'"
							+ " does not exist on computer '{1}'.", logName,
							machineName));

					// remove all eventlog entries for specified log
					using (EventLog eventLog = new EventLog (logName, machineName)) {
						eventLog.Clear ();
					}

					// remove file holding event log entries
					string file = (string) logKey.GetValue ("File");
					if (file != null) {
						try {
							File.Delete (file);
						} catch (Exception) {
							// .NET seems to ignore failures here
						}
					}
				}

				eventLogKey.DeleteSubKeyTree (logName);
			}
		}

		public static void DeleteEventSource(string source)
		{
			DeleteEventSource (source, ".");
		}

		[MonoTODO ("Support remote machine")]
		public static void DeleteEventSource (string source, string machineName)
		{
			if (machineName == null || machineName.Length == 0)
				throw new ArgumentException (string.Format (
					CultureInfo.InvariantCulture, "Invalid value '{0}' for"
					+ " parameter 'machineName'.", machineName));

			using (RegistryKey logKey = FindLogKeyBySource (source, machineName, true)) {
				if (logKey == null) {
					throw new ArgumentException (string.Format (
						CultureInfo.InvariantCulture, "The source '{0}' is not"
						+ " registered on computer '{1}'.", source, machineName));
				}

				logKey.DeleteSubKeyTree (source);

				string[] sources = (string[]) logKey.GetValue ("Sources");
				if (sources != null) {
					ArrayList temp = new ArrayList ();
					for (int i = 0; i < sources.Length; i++)
						if (sources[i] != source)
							temp.Add (sources[i]);
					string[] newSources = new string[temp.Count];
					temp.CopyTo (newSources, 0);
					logKey.SetValue ("Sources", newSources);
				}
			}
		}

		protected override void Dispose (bool disposing)
		{
			Impl.Dispose (disposing);
		}

		public void EndInit()
		{
			Impl.EndInit();
		}

		public static bool Exists(string logName)
		{
			return Exists (logName, ".");
		}

		[MonoTODO ("Support remote machine")]
		public static bool Exists (string logName, string machineName)
		{
			using (RegistryKey logKey = FindLogKeyByName (logName, machineName, false)) {
				return (logKey != null);
			}
		}

		public static EventLog[] GetEventLogs ()
		{
			return GetEventLogs (".");
		}

		[MonoTODO ("Support remote machine")]
		public static EventLog[] GetEventLogs (string machineName)
		{
			using (RegistryKey eventLogKey = GetEventLogKey (machineName, false)) {
				if (eventLogKey == null) {
					throw new InvalidOperationException ("TODO");
				}
				string[] logNames = eventLogKey.GetSubKeyNames ();
				EventLog[] eventLogs = new EventLog[logNames.Length];
				for (int i = 0; i < logNames.Length; i++) {
					EventLog eventLog = new EventLog (logNames[i], machineName);
					eventLogs[i] = eventLog;
				}
				return eventLogs;
			}
		}

		[MonoTODO ("Support remote machine")]
		public static string LogNameFromSourceName (string source, string machineName)
		{
			using (RegistryKey logKey = FindLogKeyBySource (source, machineName, false)) {
				if (logKey == null)
					return string.Empty;

				return GetLogName (logKey);
			}
		}

		public static bool SourceExists(string source)
		{
			return SourceExists (source, ".");
		}

		[MonoTODO ("Support remote machines")]
		public static bool SourceExists (string source, string machineName)
		{
			if (machineName == null || machineName.Length == 0)
				throw new ArgumentException (string.Format (
					CultureInfo.InvariantCulture, "Invalid value '{0}' for"
					+ " parameter 'machineName'.", machineName));

			RegistryKey logKey = FindLogKeyBySource (source, machineName, false);
			if (logKey != null) {
				logKey.Close ();
				return true;
			}
			return false;
		}

		public void WriteEntry(string message)
		{
			WriteEntry (message, EventLogEntryType.Information);
		}

		public void WriteEntry(string message, EventLogEntryType type)
		{
			WriteEntry (message, type, 0);
		}

		public void WriteEntry(string message, EventLogEntryType type, 
			int eventID)
		{
			WriteEntry (message, type, eventID, 0);
		}

		public void WriteEntry(string message, EventLogEntryType type, 
			int eventID,
			short category)
		{
			if (Source.Length == 0) {
				throw new ArgumentException ("Source property was not set"
					+ "before writing to the event log.");
			}

			if (!SourceExists (Source, MachineName)) {
				if (Log == null || Log.Length == 0) {
					Log = "Application";
				}
				CreateEventSource (Source, Log, MachineName);
			}

			WriteEntry (message, type, eventID, category, null);
		}

		public void WriteEntry(string message, EventLogEntryType type, 
			int eventID,
			short category, byte[] rawData)
		{
			Impl.WriteEntry (message, type, eventID, category, rawData);
		}

		public static void WriteEntry(string source, string message)
		{
			WriteEntry (source, message, EventLogEntryType.Information);
		}

		public static void WriteEntry(string source, string message, 
			EventLogEntryType type)
		{
			WriteEntry (source, message, EventLogEntryType.Information, 0);
		}

		public static void WriteEntry(string source, string message, 
			EventLogEntryType type, int eventID)
		{
			WriteEntry (source, message, EventLogEntryType.Information, eventID, 0);
		}

		public static void WriteEntry(string source, string message, 
			EventLogEntryType type, int eventID, short category)
		{
			WriteEntry (source, message, EventLogEntryType.Information, eventID, category, null);
		}

		public static void WriteEntry(string source, string message, 
			EventLogEntryType type, int eventID, short category, 
			byte[] rawData)
		{
			using (EventLog eventLog = new EventLog ()) {
				eventLog.Source = source;
				eventLog.WriteEntry (message, type, eventID, category, rawData);
			}
		}

		internal void OnEntryWritten (EventLogEntry newEntry)
		{
			if (EntryWritten != null)
				EntryWritten (this, new EntryWrittenEventArgs (newEntry));
		}

		[MonitoringDescription ("Raised for each EventLog entry written.")]
		public event EntryWrittenEventHandler EntryWritten;

		internal string GetLogName ()
		{
			if (logName != null && logName.Length > 0)
				return logName;

			// if no log name has been set, then use source to determine name of log
			logName = LogNameFromSourceName (source, machineName);
			return logName;
		}

		private static bool Win32EventLogEnabled {
			get {
				return (Environment.OSVersion.Platform == PlatformID.Win32NT);
			}
		}

		private static void UpdateLogRegistry (RegistryKey logKey)
		{
			if (!Win32EventLogEnabled)
				return;

			// TODO: write other Log values:
			// - MaxSize
			// - Retention
			// - AutoBackupLogFiles

			if (logKey.GetValue ("File") == null) {
				string logName = GetLogName (logKey);
				string file;
				if (logName.Length > 8) {
					file = logName.Substring (0, 8) + ".evt";
				} else {
					file = logName + ".evt";
				}
				string configPath = Path.Combine (Environment.GetFolderPath (
					Environment.SpecialFolder.System), "config");
				logKey.SetValue ("File", Path.Combine (configPath, file));
			}

		}

		private static void UpdateSourceRegistry (RegistryKey sourceKey, EventSourceCreationData data)
		{
			if (data.CategoryCount > 0)
				sourceKey.SetValue ("CategoryCount", data.CategoryCount);

			if (data.CategoryResourceFile != null && data.CategoryResourceFile.Length > 0)
				sourceKey.SetValue ("CategoryMessageFile", data.CategoryResourceFile);

			if (data.MessageResourceFile != null && data.MessageResourceFile.Length > 0)
				sourceKey.SetValue ("EventMessageFile", data.MessageResourceFile);

			if (data.ParameterResourceFile != null && data.ParameterResourceFile.Length > 0)
				sourceKey.SetValue ("ParameterMessageFile", data.ParameterResourceFile);
		}

		private static string GetLogName (RegistryKey logKey) {
			string logName = logKey.Name;
			return logName.Substring (logName.LastIndexOf ("\\") + 1);
		}

		[MonoTODO ("Support remote machines")]
		private static RegistryKey GetEventLogKey (string machineName, bool writable)
		{
			return Registry.LocalMachine.OpenSubKey (@"SYSTEM\CurrentControlSet\Services\EventLog", writable);
		}

		private static RegistryKey FindLogKeyByName (string logName, string machineName, bool writable)
		{
			using (RegistryKey eventLogKey = GetEventLogKey (machineName, writable)) {
				if (eventLogKey == null) {
					return null;
				}

				return eventLogKey.OpenSubKey (logName, writable);
			}
		}

		private static RegistryKey FindLogKeyBySource (string source, string machineName, bool writable)
		{
			if (source == null || source.Length == 0)
				return null;

			RegistryKey eventLogKey = null;
			try {
				eventLogKey = GetEventLogKey (machineName, writable);
				if (eventLogKey == null)
					return null;

				string[] subKeys = eventLogKey.GetSubKeyNames ();
				for (int i = 0; i < subKeys.Length; i++) {
					RegistryKey sourceKey = null;
					try {
						RegistryKey logKey = eventLogKey.OpenSubKey (subKeys[i], writable);
						if (logKey != null) {
							sourceKey = logKey.OpenSubKey (source, writable);
							if (sourceKey != null)
								return logKey;
						}
					} finally {
						if (sourceKey != null)
							sourceKey.Close ();
					}
				}
				return null;
			} finally {
				if (eventLogKey != null)
					eventLogKey.Close ();
			}
		}
	}
}
