//
// System.Diagnostics.EventLogImpl.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Atsushi Enomoto  <atsushi@ximian.com>
//   Gert Driesen (drieseng@users.sourceforge.net)
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
				// to-do perform valid character checks
				if (_coreEventLog.Log != null && _coreEventLog.Log.Length == 0) {
					throw new InvalidOperationException ("Event log names must"
						+ " consist of printable characters and cannot contain"
						+ " \\, *, ?, or spaces.");
				}
				if (_coreEventLog.Log != null) {
					if (_coreEventLog.Log.Length == 0)
						return string.Empty;

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

		public EventLogEntry [] GetEntries ()
		{
			string logName = CoreEventLog.Log;
			if (logName == null || logName.Length == 0)
				throw new ArgumentException ("Log property value has not been specified.");

			if (!EventLog.Exists (logName))
				throw new InvalidOperationException (string.Format (
					CultureInfo.InvariantCulture, "The event log '{0}' on "
					+ " computer '{1}' does not exist.", logName,
					_coreEventLog.MachineName));

			int entryCount = GetEntryCount ();
			EventLogEntry [] entries = new EventLogEntry [entryCount];
			for (int i = 0; i < entryCount; i++) {
				entries [i] = GetEntry (i);
			}
			return entries;
		}

		public abstract void DisableNotification ();

		public abstract void EnableNotification ();

		public abstract void BeginInit ();

		public abstract void Clear ();

		public abstract void Close ();

		public abstract void CreateEventSource (EventSourceCreationData sourceData);

		public abstract void Delete (string logName, string machineName);

		public abstract void DeleteEventSource (string source, string machineName);

		public abstract void Dispose (bool disposing);

		public abstract void EndInit ();

		public abstract bool Exists (string logName, string machineName);

		protected abstract int GetEntryCount ();

		protected abstract EventLogEntry GetEntry (int index);

		public EventLog [] GetEventLogs (string machineName)
		{
			string [] logNames = GetLogNames (machineName);
			EventLog [] eventLogs = new EventLog [logNames.Length];
			for (int i = 0; i < logNames.Length; i++) {
				EventLog eventLog = new EventLog (logNames [i], machineName);
				eventLogs [i] = eventLog;
			}
			return eventLogs;
		}

		protected abstract string GetLogDisplayName ();

		public abstract string LogNameFromSourceName (string source, string machineName);

		public abstract bool SourceExists (string source, string machineName);

		public abstract void WriteEntry (string [] replacementStrings, EventLogEntryType type, uint instanceID, short category, byte[] rawData);

		protected abstract string FormatMessage (string source, uint messageID, string [] replacementStrings);

		protected abstract string [] GetLogNames (string machineName);

		protected void ValidateCustomerLogName (string logName, string machineName)
		{
			if (logName.Length >= 8) {
				string significantName = logName.Substring (0, 8);
				if (string.Compare (significantName, "AppEvent", true) == 0 || string.Compare (significantName, "SysEvent", true) == 0 || string.Compare (significantName, "SecEvent", true) == 0)
					throw new ArgumentException (string.Format (
						CultureInfo.InvariantCulture, "The log name: '{0}' is"
						+ " invalid for customer log creation.", logName));

				// the first 8 characters of the log name are used as  filename
				// for .evt file and as such no two logs with 8 characters or 
				// more should have the same first 8 characters (or the .evt
				// would be overwritten)
				//
				// this check is not strictly necessary on unix
				string [] logs = GetLogNames (machineName);
				for (int i = 0; i < logs.Length; i++) {
					string log = logs [i];
					if (log.Length >= 8 && string.Compare (log, 0, significantName, 0, 8, true) == 0)
						throw new ArgumentException (string.Format (
							CultureInfo.InvariantCulture, "Only the first eight"
							+ " characters of a custom log name are significant,"
							+ " and there is already another log on the system"
							+ " using the first eight characters of the name given."
							+ " Name given: '{0}', name of existing log: '{1}'.",
							logName, log));
				}
			}

			// LAMESPEC: check if the log name matches an existing source
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=186552
			if (SourceExists (logName, machineName)) {
				if (machineName == ".")
					throw new ArgumentException (string.Format (
						CultureInfo.InvariantCulture, "Log {0} has already been"
						+ " registered as a source on the local computer.", 
						logName));
				else
					throw new ArgumentException (string.Format (
						CultureInfo.InvariantCulture, "Log {0} has already been"
						+ " registered as a source on the computer {1}.",
						logName, machineName));
			}
		}

		public abstract OverflowAction OverflowAction { get; }

		public abstract int MinimumRetentionDays { get; }

		public abstract long MaximumKilobytes { get; set; }

		public abstract void ModifyOverflowPolicy (OverflowAction action, int retentionDays);

		public abstract void RegisterDisplayName (string resourceFile, long resourceId);
	}
}
