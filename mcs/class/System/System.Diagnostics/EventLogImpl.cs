//
// System.Diagnostics.EventLogImpl.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Diagnostics
{

// FIXME set a symbol for every implementation and include the implementation
#if (EVENTLOG_WIN32)

	// TODO implement the EventLog for Win32 platforms

#elif (EVENTLOG_GENERIC)

	// TODO implement a generic (XML - based?) Eventlog for non - Win32 platforms

#else
	// Empty implementation that does not need any specific platform
	// but should be enough to get applications to run that WRITE to eventlog
	internal class EventLogImpl
	{
		public EventLogImpl (EventLog coreEventLog)
		{
		}

		public static event EntryWrittenEventHandler EntryWritten;

		public EventLogEntryCollection Entries {
			get {return new EventLogEntryCollection ();}
		}

		public string LogDisplayName {
			get {return "";}
		}

		public void BeginInit () {}

		public void Clear () {}

		public void Close () {}

		public static void CreateEventSource (string source, string logName, string machineName) {}

		public static void Delete (string logName, string machineName) {}

		public static void DeleteEventSource (string source, string machineName) {}

		public void Dispose (bool disposing) {}

		public void EndInit () {}

		public static bool Exists (string logName, string machineName)
		{
			return false;
		}

		public static EventLog[] GetEventLogs (string machineName)
		{
			return new EventLog[0];
		}

		public static string LogNameFromSourceName (string source, string machineName)
		{
			return String.Empty;
		}

		public static bool SourceExists (string source, string machineName)
		{
			return false;
		}

		public void WriteEntry (string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
		{
			WriteEntry ("", message, type, eventID, category, rawData);
		}

		public static void WriteEntry (string source, string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
		{
			EventLogEntry Entry;
			Entry = new EventLogEntry ("", category, 0, eventID, message, source, 
				"", "", type, DateTime.Now, DateTime.Now, rawData, null);
			if (EntryWritten != null)
				EntryWritten (null, new EntryWrittenEventArgs (Entry));
		}
	}

#endif

}
