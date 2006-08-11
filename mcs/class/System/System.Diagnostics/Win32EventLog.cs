//
// System.Diagnostics.Win32EventLog.cs
//
// Author:
//	Gert Driesen <driesen@users.sourceforge.net>
//
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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Diagnostics
{
	internal class Win32EventLog : EventLogImpl
	{
		private int _oldestEventLogEntry = -1;

		public Win32EventLog (EventLog coreEventLog) : base (coreEventLog)
		{
		}

		public override void BeginInit () { }

		public override void Clear () {
			IntPtr hEventLog = OpenEventLog ();
			int ret = PInvoke.ClearEventLog (hEventLog, null);
			if (ret != 1) {
				throw new Win32Exception (Marshal.GetLastWin32Error ());
			}
			CloseEventLog (hEventLog);
		}

		public override void Close () {
			// we don't hold any unmanaged resources
		}

		public override void Dispose (bool disposing) {
			Close ();
		}

		public override void EndInit () { }

		public override EventLogEntry[] GetEntries ()
		{
			return new EventLogEntry[0];
		}

		protected override int GetEntryCount ()
		{
			IntPtr hEventLog = OpenEventLog ();
			try {
				int entryCount = 0;
				int retVal = PInvoke.GetNumberOfEventLogRecords (hEventLog, ref entryCount);
				if (retVal != 1) {
					throw new Win32Exception (Marshal.GetLastWin32Error ());
				}
				return entryCount;
			} finally {
				CloseEventLog (hEventLog);
			}
		}

		[MonoTODO ("Read buffer data")]
		protected override EventLogEntry GetEntry (int index)
		{
			index += OldestEventLogEntry;

			IntPtr hEventLog = OpenEventLog ();
			try {
				int bytesRead = 0;
				int minBufferNeeded = 0;
				byte[] buffer = new byte[0x7ffff]; // according to MSDN this is the max size of the buffer

				int ret = PInvoke.ReadEventLog (hEventLog, ReadFlags.Seek |
					ReadFlags.ForwardsRead, index, buffer, buffer.Length,
					ref bytesRead, ref minBufferNeeded);
				if (ret != 1) {
					throw new InvalidOperationException ("Event log cannot be read.");
				}

				// TODO read data from buffer and construct EventLogEntry !!!!!!!!!!!!
				return null;
			} finally {
				CloseEventLog (hEventLog);
			}
		}

		[MonoTODO]
		protected override string GetLogDisplayName ()
		{
			return CoreEventLog.Log;
		}

		protected override void WriteEventLogEntry (EventLogEntry entry)
		{
			IntPtr hEventLog = OpenEventLog ();
			try {
				byte[] rawData = (entry.Data == null) ? new byte[0] : entry.Data;
				int ret = PInvoke.ReportEvent (hEventLog, (ushort) entry.EntryType,
					(ushort) entry.CategoryNumber, (uint) entry.EventID, IntPtr.Zero,
					(ushort) 1, (uint) rawData.Length, new string[] { entry.Message },
					rawData);
				if (ret != 1) {
					throw new Win32Exception (Marshal.GetLastWin32Error ());
				}
			} finally {
				CloseEventLog (hEventLog);
			}
		}

		private int OldestEventLogEntry {
			get {
				if (_oldestEventLogEntry == -1) {
					IntPtr hEventLog = OpenEventLog ();
					try {
						int ret = PInvoke.GetOldestEventLogRecord (hEventLog, ref _oldestEventLogEntry);
						if (ret != 1) {
							throw new Win32Exception (Marshal.GetLastWin32Error ());
						}
					} finally {
						CloseEventLog (hEventLog);
					}
				}

				return _oldestEventLogEntry;
			}
		}


		private IntPtr OpenEventLog ()
		{
			string logName = CoreEventLog.GetLogName ();
			IntPtr hEventLog = PInvoke.OpenEventLog (CoreEventLog.MachineName,
				logName);
			if (hEventLog == IntPtr.Zero) {
				// TODO: include cause of error
				throw new InvalidOperationException (string.Format (
					CultureInfo.InvariantCulture, "Event Log '{0}' on computer"
					+ " '{1}' cannot be opened."));
			}
			return hEventLog;
		}

		private void CloseEventLog (IntPtr hEventLog)
		{
			int ret = PInvoke.CloseEventLog (hEventLog);
			if (ret != 1) {
				throw new Win32Exception (Marshal.GetLastWin32Error ());
			}
		}

		private class PInvoke
		{
			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int ClearEventLog (IntPtr hEventLog, string lpBackupFileName);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int CloseEventLog (IntPtr hEventLog);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int GetNumberOfEventLogRecords (IntPtr hEventLog, ref int NumberOfRecords);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int GetOldestEventLogRecord (IntPtr hEventLog, ref int OldestRecord);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern IntPtr OpenEventLog (string machineName, string logName);

			[DllImport ("Advapi32.dll", SetLastError=true)]
			public static extern int ReportEvent (IntPtr hHandle, ushort wType,
				ushort wCategory, uint dwEventID, IntPtr sid, ushort wNumStrings,
				uint dwDataSize, string[] lpStrings, byte[] lpRawData);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int ReadEventLog (IntPtr hEventLog, ReadFlags dwReadFlags, int dwRecordOffset, byte[] buffer, int nNumberOfBytesToRead, ref int pnBytesRead, ref int pnMinNumberOfBytesNeeded);
		}

		private enum ReadFlags
		{
			Sequential = 0x001,
			Seek = 0x002,
			ForwardsRead = 0x004,
			BackwardsRead = 0x008
		}
	}
}

// http://msdn.microsoft.com/library/en-us/eventlog/base/eventlogrecord_str.asp:
//
// struct EVENTLOGRECORD {
//	int Length;
//	int Reserved;
//	int RecordNumber;
//	int TimeGenerated;
//	int TimeWritten;
//	int EventID;
//	ushort EventType;
//	ushort NumStrings;
//	ushort EventCategory;
//	ushort ReservedFlags;
//	int ClosingRecordNumber;
//	int StringOffset;
//	int UserSidLength;
//	int UserSidOffset;
//	int DataLength;  DWORD DataOffset;
// }
