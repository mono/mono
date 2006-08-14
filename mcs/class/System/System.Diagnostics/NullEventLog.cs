//
// NullEventLog.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
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
	internal class NullEventLog : EventLogImpl
	{
		EventLogEntryCollection empty_entries =
			new EventLogEntryCollection (new EventLogEntry [0]);

		public NullEventLog (EventLog coreEventLog)
			: base (coreEventLog)
		{
		}

		public override EventLogEntryCollection Entries {
			get { return empty_entries; }
		}

		public override string LogDisplayName {
			get { return String.Empty; }
		}

		public override void BeginInit ()
		{
		}

		public override void Clear ()
		{
		}

		public override void Close ()
		{
		}

		public override void Dispose (bool disposing)
		{
		}

		public override void EndInit ()
		{
		}
	}

	internal class NullEventLogFactory : EventLogFactory
	{
		EventLog [] empty_logs = new EventLog [0];

		public override EventLogImpl Create (EventLog source)
		{
			return new NullEventLog (source);
		}

		public override void CreateEventSource (string source, string logName, string machineName)
		{
		}

		public override void Delete (string logName, string machineName)
		{
		}

		public override void DeleteEventSource (string source, string machineName)
		{
		}

		public override bool Exists (string logName, string machineName)
		{
			return false;
		}

		public override EventLog [] GetEventLogs (string machineName)
		{
			return empty_logs;
		}

		public override string LogNameFromSourceName (string source, string machineName)
		{
			return String.Empty;
		}

		public override bool SourceExists (string source, string machineName)
		{
			return false;
		}

		public override void WriteEntry (string source, string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
		{
		}
	}
}
