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

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Diagnostics {

	[MonoTODO("This class is just stubbed out")]
	[DefaultEvent ("EntryWritten"), DesignerCategory ("Category"), InstallerType (typeof (EventLogInstaller))]
	#if (NET_1_0)
		[Designer ("Microsoft.VisualStudio.Install.EventLogInstallableComponentDesigner, Microsoft.VisualStudio, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof (IDesigner))]
	#endif
	#if (NET_1_1)
    		[Designer ("Microsoft.VisualStudio.Install.EventLogInstallableComponentDesigner, Microsoft.VisualStudio, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof (IDesigner))]
	#endif
	public class EventLog : Component, ISupportInitialize 
	{

		private string source;
		private string logName;
		private string machineName;

		public EventLog() : this ("")
		{
		}

		public EventLog(string logName) : this (logName, "")
		{
		}

		public EventLog(string logName, string machineName) 
			: this (logName, machineName, "")
		{
		}

		public EventLog(string logName, string machineName, 
			string source)
		{
			this.source = source;
			this.machineName = machineName;
			this.logName = logName;
		}

		[MonoTODO]
		[Browsable (false), DefaultValue (false)]
		[MonitoringDescription ("If enabled raises event when a log is written.")]
		public bool EnableRaisingEvents {
			get {return false;}
			set {/* ignore */}
		}

		[MonoTODO]
		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription ("The entries in the log.")]
		public EventLogEntryCollection Entries {
			get {return null;}
		}

		[MonoTODO]
		[ReadOnly (true), DefaultValue (""), RecommendedAsConfigurable (true)]
		#if (NET_1_0)
			[TypeConverter ("System.Diagnostics.Design.LogConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		#if (NET_1_1)
    			[TypeConverter ("System.Diagnostics.Design.LogConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		[MonitoringDescription ("Name of the log that is read and written.")]
		public string Log {
			get {return logName;}
			set {logName = value;}
		}

		[MonoTODO]
		[Browsable (false)]
		public string LogDisplayName {
			get {return "";}
		}

		[MonoTODO]
		[ReadOnly (true), DefaultValue ("."), RecommendedAsConfigurable (true)]
		[MonitoringDescription ("Name of the machine that this log get written to.")]
		public string MachineName {
			get {return machineName;}
			set {/* ignore */}
		}

		[MonoTODO]
		[ReadOnly (true), DefaultValue (""), RecommendedAsConfigurable (true)]
		#if (NET_1_0)
			[TypeConverter ("System.Diagnostics.Design.StringValueConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		#if (NET_1_1)
    			[TypeConverter ("System.Diagnostics.Design.StringValueConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		[MonitoringDescription ("The application name that writes the log.")]
		public string Source {
			get {return source;}
			set {/* ignore */}
		}

		[MonoTODO]
		[Browsable (false), DefaultValue (null)]
		[MonitoringDescription ("An object that synchronizes event handler calls.")]
		public ISynchronizeInvoke SynchronizingObject {
			get {return null;}
			set {/* ignore */}
		}

		[MonoTODO]
		public void BeginInit()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Close()
		{
			throw new NotImplementedException ();
		}

		public static void CreateEventSource(string source, string logName)
		{
			CreateEventSource (source, logName, ".");
		}

		[MonoTODO]
		public static void CreateEventSource(string source, 
			string logName, 
			string machineName)
		{
			throw new NotImplementedException ();
		}

		public static void Delete(string logName)
		{
			Delete (logName, ".");
		}

		[MonoTODO]
		public static void Delete(string logName, string machineName)
		{
			throw new NotImplementedException ();
		}

		public static void DeleteEventSource(string source)
		{
			DeleteEventSource (source, ".");
		}

		[MonoTODO]
		public static void DeleteEventSource(string source, 
			string machineName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose(bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndInit()
		{
			throw new NotImplementedException ();
		}

		public static bool Exists(string logName)
		{
			return Exists (logName, ".");
		}

		[MonoTODO]
		public static bool Exists(string logName, string machineName)
		{
			throw new NotImplementedException ();
		}

		public static EventLog[] GetEventLogs()
		{
			return GetEventLogs (".");
		}

		[MonoTODO]
		public static EventLog[] GetEventLogs(string machineName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string LogNameFromSourceName(string source, 
			string machineName)
		{
			throw new NotImplementedException ();
		}

		public static bool SourceExists(string source)
		{
			return SourceExists (source, ".");
		}

		[MonoTODO]
		public static bool SourceExists(string source, string machineName)
		{
			throw new NotImplementedException ();
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
			WriteEntry (message, type, eventID, category, null);
		}

		[MonoTODO]
		public void WriteEntry(string message, EventLogEntryType type, 
			int eventID,
			short category, byte[] rawData)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public static void WriteEntry(string source, string message, 
			EventLogEntryType type, int eventID, short category, 
			byte[] rawData)
		{
			throw new NotImplementedException ();
		}

		[MonitoringDescription ("Raised for each eventlog entry written.")]
		public event EntryWrittenEventHandler EntryWritten;
	}
}

