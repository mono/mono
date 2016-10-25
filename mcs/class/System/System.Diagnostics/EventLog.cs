//
// System.Diagnostics.EventLog.cs
//
// Authors:
//	Jonathan Pryor (jonpryor@vt.edu)
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2002
// Copyright (C) 2003 Andreas Nahr
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
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

#pragma warning disable 618

namespace System.Diagnostics 
{
	[DefaultEvent ("EntryWritten")]
	[InstallerType (typeof (EventLogInstaller))]
	[MonitoringDescription ("Represents an event log")]
	public class EventLog : Component, ISupportInitialize 
	{
		private string source;
		private string logName;
		private string machineName;
		private bool doRaiseEvents = false;
		private ISynchronizeInvoke synchronizingObject = null;

		// IMPORTANT: also update constants in EventLogTest
		internal const string LOCAL_FILE_IMPL = "local";
		private const string WIN32_IMPL = "win32";
		private const string NULL_IMPL = "null";

		internal const string EVENTLOG_TYPE_VAR = "MONO_EVENTLOG_TYPE";

		private EventLogImpl Impl;

		public EventLog() : this (string.Empty)
		{
		}

		public EventLog(string logName) : this (logName, ".")
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
			if (machineName == null || machineName.Trim ().Length == 0)
				throw new ArgumentException (string.Format (
					CultureInfo.InvariantCulture, "Invalid value '{0}' for"
					+ " parameter 'machineName'.", machineName));

			this.source = source;
			this.machineName = machineName;
			this.logName = logName;

			Impl = CreateEventLogImpl (this);
		}

		[Browsable (false), DefaultValue (false)]
		[MonitoringDescription ("If enabled raises event when a log is written.")]
		public bool EnableRaisingEvents {
			get {return doRaiseEvents;}
			set {
				if (value == doRaiseEvents)
					return;

				if (value)
					Impl.EnableNotification ();
				else
					Impl.DisableNotification ();
				doRaiseEvents = value;
			}
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

				// log name is treated case-insensitively on all platforms
				if (string.Compare (logName, value, true) != 0) {
					logName = value;
					Reset ();
				}
			}
		}

		[Browsable (false)]
		public string LogDisplayName {
			get { return Impl.LogDisplayName; }
		}

		[ReadOnly (true), DefaultValue ("."), RecommendedAsConfigurable (true)]
		[MonitoringDescription ("Name of the machine that this log get written to.")]
		public string MachineName {
			get { return machineName; }
			set {
				if (value == null || value.Trim ().Length == 0)
					throw new ArgumentException (string.Format (
						CultureInfo.InvariantCulture, "Invalid value {0} for"
						+ " property MachineName.", value));

				if (string.Compare (machineName, value, true) != 0) {
					Close ();
					machineName = value;
				}
			}
		}

		[ReadOnly (true), DefaultValue (""), RecommendedAsConfigurable (true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[MonitoringDescription ("The application name that writes the log.")]
		public string Source {
			get { return source; }
			set {
				if (value == null)
					value = string.Empty;

				// Source only affects eventlog implementation if Source was set
				// and no Log was set
				if (source == null || source.Length == 0 && (logName == null ||logName.Length == 0)) {
					source = value;
				} else if (string.Compare (source, value, true) != 0) {
					source = value;
					Reset ();
				}
			}
		}

		[Browsable (false), DefaultValue (null)]
		[MonitoringDescription ("An object that synchronizes event handler calls.")]
		public ISynchronizeInvoke SynchronizingObject {
			get {return synchronizingObject;}
			set {synchronizingObject = value;}
		}

		[MonoTODO]
		[ComVisibleAttribute (false)]
		[Browsable (false)]
		public OverflowAction OverflowAction {
			get { return Impl.OverflowAction; }
		}

		[MonoTODO]
		[ComVisibleAttribute (false)]
		[Browsable (false)]
		public int MinimumRetentionDays {
			get { return Impl.MinimumRetentionDays; }
		}

		[MonoTODO]
		[ComVisibleAttribute (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public long MaximumKilobytes {
			get { return Impl.MaximumKilobytes; }
			set { Impl.MaximumKilobytes = value; }
		}

		[MonoTODO]
		[ComVisible (false)]
		public void ModifyOverflowPolicy (OverflowAction action, int retentionDays)
		{
			Impl.ModifyOverflowPolicy (action, retentionDays);
		}

		[MonoTODO]
		[ComVisibleAttribute (false)]
		public void RegisterDisplayName (string resourceFile, long resourceId)
		{
			Impl.RegisterDisplayName (resourceFile, resourceId);
		}

		public void BeginInit ()
		{
			Impl.BeginInit();
		}

		public void Clear ()
		{
			string logName = Log;
			if (logName == null || logName.Length == 0)
				throw new ArgumentException ("Log property value has not been specified.");

			if (!EventLog.Exists (logName, MachineName))
				throw new InvalidOperationException (string.Format (
					CultureInfo.InvariantCulture, "Event Log '{0}'"
					+ " does not exist on computer '{1}'.", logName,
					machineName));

			Impl.Clear ();
			Reset ();
		}

		public void Close ()
		{
			Impl.Close();
			EnableRaisingEvents = false;
		}

		internal void Reset ()
		{
			bool enableRaisingEvents = EnableRaisingEvents;
			Close ();
			EnableRaisingEvents = enableRaisingEvents;
		}

		public static void CreateEventSource (string source, string logName)
		{
			CreateEventSource (source, logName, ".");
		}

		[Obsolete ("use CreateEventSource(EventSourceCreationData) instead")]
		public static void CreateEventSource (string source, 
			string logName, 
			string machineName)
		{
			CreateEventSource (new EventSourceCreationData (source, logName,
				machineName));
		}

		[MonoNotSupported ("remote machine is not supported")]
		public static void CreateEventSource (EventSourceCreationData sourceData)
		{
			if (sourceData.Source == null || sourceData.Source.Length == 0)
				throw new ArgumentException ("Source property value has not been specified.");

			if (sourceData.LogName == null || sourceData.LogName.Length == 0)
				throw new ArgumentException ("Log property value has not been specified.");

			if (SourceExists (sourceData.Source, sourceData.MachineName))
				throw new ArgumentException (string.Format (CultureInfo.InvariantCulture,
					"Source '{0}' already exists on '{1}'.", sourceData.Source,
					sourceData.MachineName));

			EventLogImpl impl = CreateEventLogImpl (sourceData.LogName,
				sourceData.MachineName, sourceData.Source);
			impl.CreateEventSource (sourceData);
		}

		public static void Delete (string logName)
		{
			Delete (logName, ".");
		}

		[MonoNotSupported ("remote machine is not supported")]
		public static void Delete (string logName, string machineName)
		{
			if (machineName == null || machineName.Trim ().Length == 0)
				throw new ArgumentException ("Invalid format for argument"
					+ " machineName.");

			if (logName == null || logName.Length == 0)
				throw new ArgumentException ("Log to delete was not specified.");

			EventLogImpl impl = CreateEventLogImpl (logName, machineName, 
				string.Empty);
			impl.Delete (logName, machineName);
		}

		public static void DeleteEventSource (string source)
		{
			DeleteEventSource (source, ".");
		}

		[MonoNotSupported ("remote machine is not supported")]
		public static void DeleteEventSource (string source, string machineName)
		{
			if (machineName == null || machineName.Trim ().Length == 0)
				throw new ArgumentException (string.Format (
					CultureInfo.InvariantCulture, "Invalid value '{0}' for"
					+ " parameter 'machineName'.", machineName));

			EventLogImpl impl = CreateEventLogImpl (string.Empty, machineName,
				source);
			impl.DeleteEventSource (source, machineName);
		}

		protected override void Dispose (bool disposing)
		{
			if (Impl != null)
				Impl.Dispose (disposing);
		}

		public void EndInit()
		{
			Impl.EndInit();
		}

		public static bool Exists (string logName)
		{
			return Exists (logName, ".");
		}

		[MonoNotSupported ("remote machine is not supported")]
		public static bool Exists (string logName, string machineName)
		{
			if (machineName == null || machineName.Trim ().Length == 0)
				throw new ArgumentException ("Invalid format for argument machineName.");

			if (logName == null || logName.Length == 0)
				return false; 

			EventLogImpl impl = CreateEventLogImpl (logName, machineName,
				string.Empty);
			return impl.Exists (logName, machineName);
		}

		public static EventLog[] GetEventLogs ()
		{
			return GetEventLogs (".");
		}

		[MonoNotSupported ("remote machine is not supported")]
		public static EventLog[] GetEventLogs (string machineName)
		{
			EventLogImpl impl = CreateEventLogImpl (new EventLog ());
			return impl.GetEventLogs (machineName);
		}

		[MonoNotSupported ("remote machine is not supported")]
		public static string LogNameFromSourceName (string source, string machineName)
		{
			if (machineName == null || machineName.Trim ().Length == 0)
				throw new ArgumentException (string.Format (
					CultureInfo.InvariantCulture, "Invalid value '{0}' for"
					+ " parameter 'MachineName'.", machineName));

			EventLogImpl impl = CreateEventLogImpl (string.Empty, machineName,
				source);
			return impl.LogNameFromSourceName (source, machineName);
		}

		public static bool SourceExists (string source)
		{
			return SourceExists (source, ".");
		}

		[MonoNotSupported ("remote machine is not supported")]
		public static bool SourceExists (string source, string machineName)
		{
			if (machineName == null || machineName.Trim ().Length == 0)
				throw new ArgumentException (string.Format (
					CultureInfo.InvariantCulture, "Invalid value '{0}' for"
					+ " parameter 'machineName'.", machineName));

			EventLogImpl impl = CreateEventLogImpl (string.Empty, machineName,
				source);
			return impl.SourceExists (source, machineName);
		}

		public void WriteEntry (string message)
		{
			WriteEntry (message, EventLogEntryType.Information);
		}

		public void WriteEntry (string message, EventLogEntryType type)
		{
			WriteEntry (message, type, 0);
		}

		public void WriteEntry (string message, EventLogEntryType type, 
			int eventID)
		{
			WriteEntry (message, type, eventID, 0);
		}

		public void WriteEntry (string message, EventLogEntryType type, 
			int eventID,
			short category)
		{
			WriteEntry (message, type, eventID, category, null);
		}

		public void WriteEntry (string message, EventLogEntryType type, 
			int eventID,
			short category, byte[] rawData)
		{
			WriteEntry (new string [] { message }, type, eventID,
				category, rawData);
		}

		public static void WriteEntry (string source, string message)
		{
			WriteEntry (source, message, EventLogEntryType.Information);
		}

		public static void WriteEntry (string source, string message, 
			EventLogEntryType type)
		{
			WriteEntry (source, message, type, 0);
		}

		public static void WriteEntry (string source, string message, 
			EventLogEntryType type, int eventID)
		{
			WriteEntry (source, message, type, eventID, 0);
		}

		public static void WriteEntry (string source, string message, 
			EventLogEntryType type, int eventID, short category)
		{
			WriteEntry (source, message, type, eventID, category, null);
		}

		public static void WriteEntry (string source, string message, 
			EventLogEntryType type, int eventID, short category, 
			byte[] rawData)
		{
			using (EventLog eventLog = new EventLog ()) {
				eventLog.Source = source;
				eventLog.WriteEntry (message, type, eventID, category, rawData);
			}
		}

		[ComVisible (false)]
		public void WriteEvent (EventInstance instance, params object [] values)
		{
			WriteEvent (instance, null, values);
		}

		[ComVisible (false)]
		public void WriteEvent (EventInstance instance, byte [] data, params object [] values)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");

			string [] replacementStrings = null;
			if (values != null) {
				replacementStrings = new string [values.Length];
				for (int i = 0; i < values.Length; i++) {
					object value = values [i];
					if (value == null)
						replacementStrings [i] = string.Empty;
					else
						replacementStrings [i] = values [i].ToString ();
				}
			} else {
				replacementStrings = new string [0];
			}

			WriteEntry (replacementStrings, instance.EntryType, instance
				.InstanceId, (short) instance.CategoryId, data);
		}

		public static void WriteEvent (string source, EventInstance instance, params object [] values)
		{
			WriteEvent (source, instance, null, values);
		}

		public static void WriteEvent (string source, EventInstance instance, byte [] data, params object [] values)
		{
			using (EventLog eventLog = new EventLog ()) {
				eventLog.Source = source;
				eventLog.WriteEvent (instance, data, values);
			}
		}

		internal void OnEntryWritten (EventLogEntry newEntry)
		{
			if (doRaiseEvents && EntryWritten != null)
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

		private static EventLogImpl CreateEventLogImpl (string logName, string machineName, string source)
		{
			EventLog eventLog = new EventLog (logName, machineName, source);
			return CreateEventLogImpl (eventLog);
		}

		private static EventLogImpl CreateEventLogImpl (EventLog eventLog)
		{
			switch (EventLogImplType) {
			case LOCAL_FILE_IMPL:
				return new LocalFileEventLog (eventLog);
			case WIN32_IMPL:
				return new Win32EventLog (eventLog);
			case NULL_IMPL:
				return new NullEventLog (eventLog);
			default:
				// we should never get here
				throw new NotSupportedException (string.Format (
					CultureInfo.InvariantCulture, "Eventlog implementation"
					+ " '{0}' is not supported.", EventLogImplType));
			}
		}

		private static bool Win32EventLogEnabled {
			get {
				return (Environment.OSVersion.Platform == PlatformID.Win32NT);
			}
		}

		// IMPORTANT: also modify corresponding property in EventLogTest
		private static string EventLogImplType {
			get {
				string implType = Environment.GetEnvironmentVariable (EVENTLOG_TYPE_VAR);
				if (implType == null) {
					if (Win32EventLogEnabled)
						return WIN32_IMPL;
					implType = NULL_IMPL;
				} else {
					if (Win32EventLogEnabled && string.Compare (implType, WIN32_IMPL, true) == 0)
						implType = WIN32_IMPL;
					else if (string.Compare (implType, NULL_IMPL, true) == 0)
						implType = NULL_IMPL;
					else if (string.Compare (implType, 0, LOCAL_FILE_IMPL, 0, LOCAL_FILE_IMPL.Length, true) == 0)
						implType = LOCAL_FILE_IMPL;
					else
						throw new NotSupportedException (string.Format (
							CultureInfo.InvariantCulture, "Eventlog implementation"
							+ " '{0}' is not supported.", implType));
				}
				return implType;
			}
		}

		private void WriteEntry (string [] replacementStrings, EventLogEntryType type, long instanceID, short category, byte [] rawData)
		{
			if (Source.Length == 0)
				throw new ArgumentException ("Source property was not set"
					+ "before writing to the event log.");

			if (!Enum.IsDefined (typeof (EventLogEntryType), type))
				throw new InvalidEnumArgumentException ("type", (int) type,
					typeof (EventLogEntryType));

			ValidateEventID (instanceID);

			if (!SourceExists (Source, MachineName)) {
				if (Log == null || Log.Length == 0) {
					Log = "Application";
				}
				CreateEventSource (Source, Log, MachineName);

			} else if (logName != null && logName.Length != 0) {
				string actualLog = LogNameFromSourceName (Source, MachineName);
				if (string.Compare (logName, actualLog, true, CultureInfo.InvariantCulture) != 0)
					throw new ArgumentException (string.Format (
						CultureInfo.InvariantCulture, "The source '{0}' is not"
						+ " registered in log '{1}' (it is registered in log"
						+ " '{2}'). The Source and Log properties must be"
						+ " matched, or you may set Log to the empty string,"
						+ " and it will automatically be matched to the Source"
						+ " property.", Source, logName, actualLog));
			}


			if (rawData == null)
				rawData = new byte [0];

			Impl.WriteEntry (replacementStrings, type, (uint) instanceID, category, rawData);
		}

		private void ValidateEventID (long instanceID)
		{
			int eventID = GetEventID (instanceID);
			if (eventID < ushort.MinValue || eventID > ushort.MaxValue)
				throw new ArgumentException (string.Format (CultureInfo.InvariantCulture,
					"Invalid eventID value '{0}'. It must be in the range between"
					+ " '{1}' and '{2}'.", instanceID, ushort.MinValue, ushort.MaxValue));
		}

		internal static int GetEventID (long instanceID)
		{
			long inst = (instanceID < 0) ? -instanceID : instanceID;

			// MSDN: eventID equals the InstanceId with the top two bits masked
			int eventID = (int) (inst & 0x3fffffff);
			return (instanceID < 0) ? -eventID : eventID;
		}
	}
}
