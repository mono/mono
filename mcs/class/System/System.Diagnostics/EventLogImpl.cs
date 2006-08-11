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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Win32;

namespace System.Diagnostics
{
	internal abstract class EventLogImpl
	{
		readonly EventLog _coreEventLog;

		protected EventLogImpl (EventLog coreEventLog)
		{
			_coreEventLog = coreEventLog;
		}

		public event EntryWrittenEventHandler EntryWritten;

		protected EventLog CoreEventLog {
			get { return _coreEventLog; }
		}

		public int EntryCount {
			get {
				if (_coreEventLog.Log == null || _coreEventLog.Log.Length == 0) {
					throw new ArgumentException ("Log property is not set.");
				}

				if (!EventLog.Exists (_coreEventLog.Log, _coreEventLog.MachineName)) {
					throw new InvalidOperationException (string.Format (
						CultureInfo.InvariantCulture, "The event log '{0}' on "
						+ " computer '{1}' does not exist.", _coreEventLog.Log,
						_coreEventLog.MachineName));
				}

				return GetEntryCount ();
			}
		}

		public EventLogEntry this[int index] {
			get {
				if (_coreEventLog.Log == null || _coreEventLog.Log.Length == 0) {
					throw new ArgumentException ("Log property is not set.");
				}

				if (!EventLog.Exists (_coreEventLog.Log, _coreEventLog.MachineName)) {
					throw new InvalidOperationException (string.Format (
						CultureInfo.InvariantCulture, "The event log '{0}' on "
						+ " computer '{1}' does not exist.", _coreEventLog.Log,
						_coreEventLog.MachineName));
				}

				if (index < 0 || index >= EntryCount)
					throw new ArgumentException ("Index out of range");

				return GetEntry (index);
			}
		}

		public string LogDisplayName {
			get {
#if NET_2_0
				// to-do perform valid character checks
				if (_coreEventLog.Log != null && _coreEventLog.Log.Length == 0) {
					throw new InvalidOperationException ("Event log names must"
						+ " consist of printable characters and cannot contain"
						+ " \\, *, ?, or spaces.");
				}
#endif
				if (_coreEventLog.Log != null) {
					if (!EventLog.Exists (_coreEventLog.Log, _coreEventLog.MachineName)) {
						throw new InvalidOperationException (string.Format (
							CultureInfo.InvariantCulture, "Cannot find Log {0}"
							+ " on computer {1}.", _coreEventLog.Log,
							_coreEventLog.MachineName));
					}
				}

				return GetLogDisplayName ();
			}
		}

		public abstract void BeginInit ();

		public abstract void Clear ();

		public abstract void Close ();

		public abstract void Dispose (bool disposing);

		public abstract void EndInit ();

		public abstract EventLogEntry[] GetEntries ();

		protected abstract int GetEntryCount ();

		protected abstract EventLogEntry GetEntry (int index);

		protected abstract string GetLogDisplayName ();

		protected abstract void WriteEventLogEntry (EventLogEntry entry);

		public void WriteEntry (string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
		{
			EventLogEntry entry = new EventLogEntry (string.Empty, category, 0, eventID,
				_coreEventLog.Source, message, string.Empty, _coreEventLog.MachineName,
				type, DateTime.Now, DateTime.Now, rawData, new string [] { message }, eventID);
			WriteEventLogEntry (entry);
			if (EntryWritten != null)
				EntryWritten (null, new EntryWrittenEventArgs (entry));
		}
	}

	// Empty implementation that does not need any specific platform
	// but should be enough to get applications to run that WRITE to eventlog
	internal class NullEventLog : EventLogImpl
	{
		public NullEventLog (EventLog coreEventLog)
			: base (coreEventLog)
		{
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

		public override EventLogEntry[] GetEntries ()
		{
			return new EventLogEntry[0];
		}

		protected override int GetEntryCount ()
		{
			return 0;
		}

		protected override EventLogEntry GetEntry (int index)
		{
			return null;
		}

		protected override string GetLogDisplayName ()
		{
			return CoreEventLog.Log;
		}

		protected override void WriteEventLogEntry (EventLogEntry entry)
		{
		}
	}
}
