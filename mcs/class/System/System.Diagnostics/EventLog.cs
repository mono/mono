//
// System.Diagnostics.EventLog.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;
using System.ComponentModel;

namespace System.Diagnostics {

	[MonoTODO("This class is just stubbed out")]
	public class EventLog : Component, ISupportInitialize {

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

//		[MonoTODO]
//		public bool EnableRaisingEvents {
//			get {return false;}
//			set {/* ignore */}
//		}
//
//		[MonoTODO]
//		public EventLogEntryCollection Entries {
//			get {return null;}
//		}
//
//		[MonoTODO]
//		public string Log {
//			get {return log;}
//			set {log = value;}
//		}
//
//		[MonoTODO]
//		public string LogDisplayName {
//			get {return "";}
//		}
//
//		[MonoTODO]
//		public string MachineName {
//			get {return machineName;}
//			set {/* ignore */}
//		}
//
//		[MonoTODO]
//		public string Source {
//			get {return source;}
//			set {/* ignore */}
//		}
//
//		[MonoTODO]
//		public ISynchronizeInvoke SynchronizingObject {
//			get {return null;}
//			set {/* ignore */}
//		}
//
		[MonoTODO]
		public void BeginInit()
		{
			throw new NotImplementedException ();
		}

//		[MonoTODO]
//		public void Clear()
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public void Close()
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void CreateEventSource(string source, 
//			string logName)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void CreateEventSource(string source, 
//			string logName, 
//			string machineName)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void Delete(string logName)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void Delete(string logName, string machineName)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void DeleteEventSource(string source)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void DeleteEventSource(string source, 
//			string machineName)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		protected override void Dispose(bool disposing)
//		{
//			throw new NotImplementedException ();
//		}
//
		[MonoTODO]
		public void EndInit()
		{
			throw new NotImplementedException ();
		}

//		[MonoTODO]
//		public static bool Exists(string logName)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static bool Exists(string logName, string machineName)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static EventLog[] GetEventLogs()
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static EventLog[] GetEventLogs(string machineName)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static string LogNameFromSourceName(string source, 
//			string machineName)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static bool SourceExists(string source)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static bool SourceExists(string source, 
//			string machineName)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public void WriteEntry(string message)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public void WriteEntry(string message, EventLogEntryType type)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void WriteEntry(string source, string message)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public void WriteEntry(string message, EventLogEntryType type, 
//			int eventID)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void WriteEntry(string source, string message, 
//			EventLogEntryType type)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public void WriteEntry(string message, EventLogEntryType type, 
//			int eventID,
//			short category)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void WriteEntry(string source, string message, 
//			EventLogEntryType type, int eventID)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public void WriteEntry(string message, EventLogEntryType type, 
//			int eventID,
//			short category, byte[] rawData)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void WriteEntry(string source, string message, 
//			EventLogEntryType type, int eventID, short category)
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void WriteEntry(string source, string message, 
//			EventLogEntryType type, int eventID, short category, 
//			byte[] rawData)
//		{
//			throw new NotImplementedException ();
//		}

		public event EntryWrittenEventHandler EntryWritten;
	}
}

