//
// System.Diagnostics.EventLogImpl.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2003 Andreas Nahr
// (C) 2006 Novell, Inc.
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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;

namespace System.Diagnostics
{
	internal abstract class EventLogImpl
	{
		static EventLogFactory factory;

		static EventLogImpl ()
		{
			factory = GetFactory ();
		}

		static EventLogFactory GetFactory ()
		{
			if (LocalFileEventLogUtil.IsEnabled)
				return new LocalFileEventLogFactory ();

			//throw new NotSupportedException (String.Format ("No EventLog implementation is supported. Consider setting MONO_LOCAL_EVENTLOG_PATH environment variable."));
			return new NullEventLogFactory ();
		}

		EventLog log;

		protected EventLogImpl (EventLog coreEventLog)
		{
			this.log = coreEventLog;
		}

		public static EventLogImpl Create (EventLog source)
		{
			return factory.Create (source);
		}

		public static event EntryWrittenEventHandler EntryWritten;

		public abstract EventLogEntryCollection Entries { get; }

		public abstract string LogDisplayName { get; }

		public abstract void BeginInit ();

		public abstract void Clear ();

		public abstract void Close ();

		public static void CreateEventSource (string source, string logName, string machineName)
		{
			factory.CreateEventSource (source, logName, machineName);
		}

		public static void Delete (string logName, string machineName)
		{
			factory.Delete (logName, machineName);
		}

		public static void DeleteEventSource (string source, string machineName)
		{
			factory.DeleteEventSource (source, machineName);
		}

		public abstract void Dispose (bool disposing);

		public abstract void EndInit ();

		public static bool Exists (string logName, string machineName)
		{
			return factory.Exists (logName, machineName);
		}

		public static EventLog[] GetEventLogs (string machineName)
		{
			return factory.GetEventLogs (machineName);
		}

		public static string LogNameFromSourceName (string source, string machineName)
		{
			return factory.LogNameFromSourceName (source, machineName);
		}

		public static bool SourceExists (string source, string machineName)
		{
			return factory.SourceExists (source, machineName);
		}

		public void WriteEntry (string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
		{
			WriteEntry (log.Source, message, type, eventID, category, rawData);
		}

		public static void WriteEntry (string source, string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
		{
			factory.WriteEntry (source, message, type, eventID, category, rawData);
			if (EntryWritten != null) {
				// FIXME: some arguments are improper.
				EventLogEntry e = new EventLogEntry ("",
					category, 0, eventID, message, source,
					"", ".", type, DateTime.Now, DateTime.Now,
					rawData, null);
				EntryWritten (null, new EntryWrittenEventArgs (e));
			}
		}
	}

	internal abstract class EventLogFactory
	{
		public abstract EventLogImpl Create (EventLog source);

		public abstract void CreateEventSource (string source, string logName, string machineName);

		public abstract void Delete (string logName, string machineName);

		public abstract void DeleteEventSource (string source, string machineName);

		public abstract bool Exists (string logName, string machineName);

		public abstract EventLog[] GetEventLogs (string machineName);

		public abstract string LogNameFromSourceName (string source, string machineName);

		public abstract bool SourceExists (string source, string machineName);

		public abstract void WriteEntry (string source, string message, EventLogEntryType type, int eventID, short category, byte[] rawData);
	}
}
