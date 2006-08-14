//
// System.Diagnostics.EventLogImpl.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
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
