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

namespace System.Diagnostics 
{
	[DefaultEvent ("EntryWritten"), InstallerType (typeof (EventLogInstaller))]
	[Designer ("Microsoft.VisualStudio.Install.EventLogInstallableComponentDesigner, " + Consts.AssemblyMicrosoft_VisualStudio, typeof (IDesigner))]
	public class EventLog : Component, ISupportInitialize 
	{

		private string source;
		private string logName;
		private string machineName;
		private bool doRaiseEvents = false;
		private ISynchronizeInvoke synchronizingObject = null;

		private EventLogImpl Impl;

		public EventLog()
			: this ("")
		{
		}

		public EventLog(string logName)
			: this (logName, ".")
		{
		}

		public EventLog(string logName, string machineName) 
			: this (logName, machineName, "")
		{
		}

		public EventLog(string logName, string machineName, string source)
		{
			this.source = source;
			this.machineName = machineName;
			this.logName = logName;

			this.Impl = new EventLogImpl (this);
			EventLogImpl.EntryWritten += new EntryWrittenEventHandler (EntryWrittenHandler);
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
			get {return Impl.Entries;}
		}

		[ReadOnly (true), DefaultValue (""), RecommendedAsConfigurable (true)]
		[TypeConverter ("System.Diagnostics.Design.LogConverter, " + Consts.AssemblySystem_Design)]
		[MonitoringDescription ("Name of the log that is read and written.")]
		public string Log {
			get {return logName;}
			set {logName = value;}
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
			get {return source;}
			set {source = value;}
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
			EventLogImpl.CreateEventSource (source, logName, machineName);
		}

		public static void Delete(string logName)
		{
			Delete (logName, ".");
		}

		public static void Delete(string logName, string machineName)
		{
			EventLogImpl.Delete (logName, machineName);
		}

		public static void DeleteEventSource(string source)
		{
			DeleteEventSource (source, ".");
		}

		public static void DeleteEventSource(string source, 
			string machineName)
		{
			EventLogImpl.DeleteEventSource (source, machineName);
		}

		protected override void Dispose(bool disposing)
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

		public static bool Exists(string logName, string machineName)
		{
			return EventLogImpl.Exists (logName, machineName);
		}

		public static EventLog[] GetEventLogs()
		{
			return GetEventLogs (".");
		}

		public static EventLog[] GetEventLogs(string machineName)
		{
			return EventLogImpl.GetEventLogs (machineName);
		}

		public static string LogNameFromSourceName(string source, 
			string machineName)
		{
			return EventLogImpl.LogNameFromSourceName (source, machineName);
		}

		public static bool SourceExists(string source)
		{
			return SourceExists (source, ".");
		}

		public static bool SourceExists(string source, string machineName)
		{
			return EventLogImpl.SourceExists (source, machineName);
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
			EventLogImpl.WriteEntry (source, message, type, eventID, category, rawData);
		}

		internal void OnEntryWritten (EventLogEntry newEntry)
		{
			if (EntryWritten != null)
				EntryWritten (this, new EntryWrittenEventArgs (newEntry));
		}

		[MonitoringDescription ("Raised for each EventLog entry written.")]
		public event EntryWrittenEventHandler EntryWritten;
	}
}

