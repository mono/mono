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
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Microsoft.Win32;

namespace System.Diagnostics
{
	internal class Win32EventLog : EventLogImpl
	{
		private const int MESSAGE_NOT_FOUND = 317;
		private ManualResetEvent _notifyResetEvent;
		private IntPtr _readHandle;
		private Thread _notifyThread;
		private int _lastEntryWritten;
		private bool _notifying;

		public Win32EventLog (EventLog coreEventLog)
			: base (coreEventLog)
		{
		}

		public override void BeginInit ()
		{
		}

		public override void Clear ()
		{
			int ret = PInvoke.ClearEventLog (ReadHandle, null);
			if (ret != 1)
				throw new Win32Exception (Marshal.GetLastWin32Error ());
		}

		public override void Close ()
		{
			if (_readHandle != IntPtr.Zero) {
				CloseEventLog (_readHandle);
				_readHandle = IntPtr.Zero;
			}
		}

		public override void CreateEventSource (EventSourceCreationData sourceData)
		{
			using (RegistryKey eventLogKey = GetEventLogKey (sourceData.MachineName, true)) {
				if (eventLogKey == null)
					throw new InvalidOperationException ("EventLog registry key is missing.");

				bool logKeyCreated = false;
				RegistryKey logKey = null;
				try {
					logKey = eventLogKey.OpenSubKey (sourceData.LogName, true);
					if (logKey == null) {
						ValidateCustomerLogName (sourceData.LogName, 
							sourceData.MachineName);

						logKey = eventLogKey.CreateSubKey (sourceData.LogName);
						logKey.SetValue ("Sources", new string [] { sourceData.LogName,
							sourceData.Source });
						UpdateLogRegistry (logKey);

						using (RegistryKey sourceKey = logKey.CreateSubKey (sourceData.LogName)) {
							UpdateSourceRegistry (sourceKey, sourceData);
						}

						logKeyCreated = true;
					}

					if (sourceData.LogName != sourceData.Source) {
						if (!logKeyCreated) {
							string [] sources = (string []) logKey.GetValue ("Sources");
							if (sources == null) {
								logKey.SetValue ("Sources", new string [] { sourceData.LogName,
									sourceData.Source });
							} else {
								bool found = false;
								for (int i = 0; i < sources.Length; i++) {
									if (sources [i] == sourceData.Source) {
										found = true;
										break;
									}
								}
								if (!found) {
									string [] newSources = new string [sources.Length + 1];
									Array.Copy (sources, 0, newSources, 0, sources.Length);
									newSources [sources.Length] = sourceData.Source;
									logKey.SetValue ("Sources", newSources);
								}
							}
						}
						using (RegistryKey sourceKey = logKey.CreateSubKey (sourceData.Source)) {
							UpdateSourceRegistry (sourceKey, sourceData);
						}
					}
				} finally {
					if (logKey != null)
						logKey.Close ();
				}
			}
		}

		public override void Delete (string logName, string machineName)
		{
			using (RegistryKey eventLogKey = GetEventLogKey (machineName, true)) {
				if (eventLogKey == null)
					throw new InvalidOperationException ("The event log key does not exist.");

				using (RegistryKey logKey = eventLogKey.OpenSubKey (logName, false)) {
					if (logKey == null)
						throw new InvalidOperationException (string.Format (
							CultureInfo.InvariantCulture, "Event Log '{0}'"
							+ " does not exist on computer '{1}'.", logName,
							machineName));

					// remove all eventlog entries for specified log
					CoreEventLog.Clear ();

					// remove file holding event log entries
					string file = (string) logKey.GetValue ("File");
					if (file != null) {
						try {
							File.Delete (file);
						} catch (Exception) {
							// .NET seems to ignore failures here
						}
					}
				}

				eventLogKey.DeleteSubKeyTree (logName);
			}
		}

		public override void DeleteEventSource (string source, string machineName)
		{
			using (RegistryKey logKey = FindLogKeyBySource (source, machineName, true)) {
				if (logKey == null) {
					throw new ArgumentException (string.Format (
						CultureInfo.InvariantCulture, "The source '{0}' is not"
						+ " registered on computer '{1}'.", source, machineName));
				}

				logKey.DeleteSubKeyTree (source);

				string [] sources = (string []) logKey.GetValue ("Sources");
				if (sources != null) {
					ArrayList temp = new ArrayList ();
					for (int i = 0; i < sources.Length; i++)
						if (sources [i] != source)
							temp.Add (sources [i]);
					string [] newSources = new string [temp.Count];
					temp.CopyTo (newSources, 0);
					logKey.SetValue ("Sources", newSources);
				}
			}
		}

		public override void Dispose (bool disposing)
		{
			Close ();
		}

		public override void EndInit ()
		{
		}

		public override bool Exists (string logName, string machineName)
		{
			using (RegistryKey logKey = FindLogKeyByName (logName, machineName, false)) {
				return (logKey != null);
			}
		}

		[MonoTODO] // ParameterResourceFile ??
		protected override string FormatMessage (string source, uint messageID, string [] replacementStrings)
		{
			string formattedMessage = null;

			string [] msgResDlls = GetMessageResourceDlls (source, "EventMessageFile");
			for (int i = 0; i < msgResDlls.Length; i++) {
				formattedMessage = FetchMessage (msgResDlls [i],
					messageID, replacementStrings);
				if (formattedMessage != null)
					break;
			}

			return formattedMessage != null ? formattedMessage : string.Join (
				", ", replacementStrings);
		}

		private string FormatCategory (string source, int category)
		{
			string formattedCategory = null;

			string [] msgResDlls = GetMessageResourceDlls (source, "CategoryMessageFile");
			for (int i = 0; i < msgResDlls.Length; i++) {
				formattedCategory = FetchMessage (msgResDlls [i],
					(uint) category, new string [0]);
				if (formattedCategory != null)
					break;
			}

			return formattedCategory != null ? formattedCategory : "(" +
				category.ToString (CultureInfo.InvariantCulture) + ")";
		}

		protected override int GetEntryCount ()
		{
			int entryCount = 0;
			int retVal = PInvoke.GetNumberOfEventLogRecords (ReadHandle, ref entryCount);
			if (retVal != 1)
				throw new Win32Exception (Marshal.GetLastWin32Error ());
			return entryCount;
		}

		protected override EventLogEntry GetEntry (int index)
		{
			// http://msdn.microsoft.com/library/en-us/eventlog/base/readeventlog.asp
			// http://msdn.microsoft.com/library/en-us/eventlog/base/eventlogrecord_str.asp
			// http://www.whitehats.ca/main/members/Malik/malik_eventlogs/malik_eventlogs.html

			index += OldestEventLogEntry;

			int bytesRead = 0;
			int minBufferNeeded = 0;
			byte [] buffer = new byte [0x7ffff]; // according to MSDN this is the max size of the buffer

			ReadEventLog (index, buffer, ref bytesRead, ref minBufferNeeded);

			MemoryStream ms = new MemoryStream (buffer);
			BinaryReader br = new BinaryReader (ms);

			// skip first 8 bytes
			br.ReadBytes (8);

			int recordNumber = br.ReadInt32 (); // 8

			int timeGeneratedSeconds = br.ReadInt32 (); // 12
			int timeWrittenSeconds = br.ReadInt32 (); // 16
			uint instanceID = br.ReadUInt32 ();
			int eventID = EventLog.GetEventID (instanceID);
			short eventType = br.ReadInt16 (); // 24
			short numStrings = br.ReadInt16 (); ; // 26
			short categoryNumber = br.ReadInt16 (); ; // 28
			// skip reservedFlags
			br.ReadInt16 (); // 30
			// skip closingRecordNumber
			br.ReadInt32 (); // 32
			int stringOffset = br.ReadInt32 (); // 36
			int userSidLength = br.ReadInt32 (); // 40
			int userSidOffset = br.ReadInt32 (); // 44
			int dataLength = br.ReadInt32 (); // 48
			int dataOffset = br.ReadInt32 (); // 52

			DateTime timeGenerated = new DateTime (1970, 1, 1).AddSeconds (
				timeGeneratedSeconds);

			DateTime timeWritten = new DateTime (1970, 1, 1).AddSeconds (
				timeWrittenSeconds);

			StringBuilder sb = new StringBuilder ();
			while (br.PeekChar () != '\0')
				sb.Append (br.ReadChar ());
			br.ReadChar (); // skip the null-char

			string sourceName = sb.ToString ();

			sb.Length = 0;
			while (br.PeekChar () != '\0')
				sb.Append (br.ReadChar ());
			br.ReadChar (); // skip the null-char
			string machineName = sb.ToString ();

			sb.Length = 0;
			while (br.PeekChar () != '\0')
				sb.Append (br.ReadChar ());
			br.ReadChar (); // skip the null-char

			string userName = null;
			if (userSidLength != 0) {
				// TODO: lazy init ?
				ms.Position = userSidOffset;
				byte [] sid = br.ReadBytes (userSidLength);
				userName = LookupAccountSid (machineName, sid);
			}

			ms.Position = stringOffset;
			string [] replacementStrings = new string [numStrings];
			for (int i = 0; i < numStrings; i++) {
				sb.Length = 0;
				while (br.PeekChar () != '\0')
					sb.Append (br.ReadChar ());
				br.ReadChar (); // skip the null-char
				replacementStrings [i] = sb.ToString ();
			}

			byte [] data = new byte [dataLength];
			ms.Position = dataOffset;
			br.Read (data, 0, dataLength);

			// TODO: lazy fetch ??
			string message = this.FormatMessage (sourceName, instanceID, replacementStrings);
			string category = FormatCategory (sourceName, categoryNumber);

			return new EventLogEntry (category, (short) categoryNumber, recordNumber,
				eventID, sourceName, message, userName, machineName,
				(EventLogEntryType) eventType, timeGenerated, timeWritten,
				data, replacementStrings, instanceID);
		}

		[MonoTODO]
		protected override string GetLogDisplayName ()
		{
			return CoreEventLog.Log;
		}

		protected override string [] GetLogNames (string machineName)
		{
			using (RegistryKey eventLogKey = GetEventLogKey (machineName, true)) {
				if (eventLogKey == null)
					return new string [0];

				return eventLogKey.GetSubKeyNames ();
			}
		}

		public override string LogNameFromSourceName (string source, string machineName)
		{
			using (RegistryKey logKey = FindLogKeyBySource (source, machineName, false)) {
				if (logKey == null)
					return string.Empty;

				return GetLogName (logKey);
			}
		}

		public override bool SourceExists (string source, string machineName)
		{
			RegistryKey logKey = FindLogKeyBySource (source, machineName, false);
			if (logKey != null) {
				logKey.Close ();
				return true;
			}
			return false;
		}

		public override void WriteEntry (string [] replacementStrings, EventLogEntryType type, uint instanceID, short category, byte [] rawData)
		{
			IntPtr hEventLog = RegisterEventSource ();
			try {
				int ret = PInvoke.ReportEvent (hEventLog, (ushort) type,
					(ushort) category, instanceID, IntPtr.Zero,
					(ushort) replacementStrings.Length,
					(uint) rawData.Length, replacementStrings, rawData);
				if (ret != 1) {
					throw new Win32Exception (Marshal.GetLastWin32Error ());
				}
			} finally {
				DeregisterEventSource (hEventLog);
			}
		}

		private static void UpdateLogRegistry (RegistryKey logKey)
		{
			// TODO: write other Log values:
			// - MaxSize
			// - Retention
			// - AutoBackupLogFiles

			if (logKey.GetValue ("File") == null) {
				string logName = GetLogName (logKey);
				string file;
				if (logName.Length > 8) {
					file = logName.Substring (0, 8) + ".evt";
				} else {
					file = logName + ".evt";
				}
				string configPath = Path.Combine (Environment.GetFolderPath (
					Environment.SpecialFolder.System), "config");
				logKey.SetValue ("File", Path.Combine (configPath, file));
			}
		}

		private static void UpdateSourceRegistry (RegistryKey sourceKey, EventSourceCreationData data)
		{
			if (data.CategoryCount > 0)
				sourceKey.SetValue ("CategoryCount", data.CategoryCount);

			if (data.CategoryResourceFile != null && data.CategoryResourceFile.Length > 0)
				sourceKey.SetValue ("CategoryMessageFile", data.CategoryResourceFile);

			if (data.MessageResourceFile != null && data.MessageResourceFile.Length > 0) {
				sourceKey.SetValue ("EventMessageFile", data.MessageResourceFile);
			} else {
				// FIXME: write default once we have approval for shipping EventLogMessages.dll
			}

			if (data.ParameterResourceFile != null && data.ParameterResourceFile.Length > 0)
				sourceKey.SetValue ("ParameterMessageFile", data.ParameterResourceFile);
		}

		private static string GetLogName (RegistryKey logKey)
		{
			string logName = logKey.Name;
			return logName.Substring (logName.LastIndexOf ("\\") + 1);
		}

		private void ReadEventLog (int index, byte [] buffer, ref int bytesRead, ref int minBufferNeeded)
		{
			const int max_retries = 3;

			// if the eventlog file changed since the handle was
			// obtained, then we need to re-try multiple times
			for (int i = 0; i < max_retries; i++) {
				int ret = PInvoke.ReadEventLog (ReadHandle, 
					ReadFlags.Seek | ReadFlags.ForwardsRead,
					index, buffer, buffer.Length, ref bytesRead,
					ref minBufferNeeded);
				if (ret != 1) {
					int error = Marshal.GetLastWin32Error ();
					if (i < (max_retries - 1)) {
						CoreEventLog.Reset ();
					} else {
						throw new Win32Exception (error);
					}
				}
			}
		}


		[MonoTODO ("Support remote machines")]
		private static RegistryKey GetEventLogKey (string machineName, bool writable)
		{
			return Registry.LocalMachine.OpenSubKey (@"SYSTEM\CurrentControlSet\Services\EventLog", writable);
		}

		private static RegistryKey FindSourceKeyByName (string source, string machineName, bool writable)
		{
			if (source == null || source.Length == 0)
				return null;

			RegistryKey eventLogKey = null;
			try {
				eventLogKey = GetEventLogKey (machineName, writable);
				if (eventLogKey == null)
					return null;

				string [] subKeys = eventLogKey.GetSubKeyNames ();
				for (int i = 0; i < subKeys.Length; i++) {
					using (RegistryKey logKey = eventLogKey.OpenSubKey (subKeys [i], writable)) {
						if (logKey == null)
							break;

						RegistryKey sourceKey = logKey.OpenSubKey (source, writable);
						if (sourceKey != null)
							return sourceKey;
					}
				}
				return null;
			} finally {
				if (eventLogKey != null)
					eventLogKey.Close ();
			}
		}

		private static RegistryKey FindLogKeyByName (string logName, string machineName, bool writable)
		{
			using (RegistryKey eventLogKey = GetEventLogKey (machineName, writable)) {
				if (eventLogKey == null) {
					return null;
				}

				return eventLogKey.OpenSubKey (logName, writable);
			}
		}

		private static RegistryKey FindLogKeyBySource (string source, string machineName, bool writable)
		{
			if (source == null || source.Length == 0)
				return null;

			RegistryKey eventLogKey = null;
			try {
				eventLogKey = GetEventLogKey (machineName, writable);
				if (eventLogKey == null)
					return null;

				string [] subKeys = eventLogKey.GetSubKeyNames ();
				for (int i = 0; i < subKeys.Length; i++) {
					RegistryKey sourceKey = null;
					try {
						RegistryKey logKey = eventLogKey.OpenSubKey (subKeys [i], writable);
						if (logKey != null) {
							sourceKey = logKey.OpenSubKey (source, writable);
							if (sourceKey != null)
								return logKey;
						}
					} finally {
						if (sourceKey != null)
							sourceKey.Close ();
					}
				}
				return null;
			} finally {
				if (eventLogKey != null)
					eventLogKey.Close ();
			}
		}

		private int OldestEventLogEntry {
			get {
				int oldestEventLogEntry = 0;
				int ret = PInvoke.GetOldestEventLogRecord (ReadHandle, ref oldestEventLogEntry);
				if (ret != 1) {
					throw new Win32Exception (Marshal.GetLastWin32Error ());
				}
				return oldestEventLogEntry;
			}
		}

		private void CloseEventLog (IntPtr hEventLog)
		{
			int ret = PInvoke.CloseEventLog (hEventLog);
			if (ret != 1) {
				throw new Win32Exception (Marshal.GetLastWin32Error ());
			}
		}

		private void DeregisterEventSource (IntPtr hEventLog)
		{
			int ret = PInvoke.DeregisterEventSource (hEventLog);
			if (ret != 1) {
				throw new Win32Exception (Marshal.GetLastWin32Error ());
			}
		}

		private static string LookupAccountSid (string machineName, byte [] sid)
		{
			// http://www.pinvoke.net/default.aspx/advapi32/LookupAccountSid.html
			// http://msdn.microsoft.com/library/en-us/secauthz/security/lookupaccountsid.asp

			StringBuilder name = new StringBuilder ();
			uint cchName = (uint) name.Capacity;
			StringBuilder referencedDomainName = new StringBuilder ();
			uint cchReferencedDomainName = (uint) referencedDomainName.Capacity;
			SidNameUse sidUse;

			string accountName = null;

			while (accountName == null) {
				bool retOk = PInvoke.LookupAccountSid (machineName, sid, name, ref cchName,
					referencedDomainName, ref cchReferencedDomainName,
					out sidUse);
				if (!retOk) {
					int err = Marshal.GetLastWin32Error ();
					if (err == PInvoke.ERROR_INSUFFICIENT_BUFFER) {
						name.EnsureCapacity ((int) cchName);
						referencedDomainName.EnsureCapacity ((int) cchReferencedDomainName);
					} else {
						// TODO: write warning ?
						accountName = string.Empty;
					}
				} else {
					accountName = string.Format ("{0}\\{1}", referencedDomainName.ToString (),
						name.ToString ());
				}
			}
			return accountName;
		}

		private static string FetchMessage (string msgDll, uint messageID, string [] replacementStrings)
		{
			// http://msdn.microsoft.com/library/en-us/debug/base/formatmessage.asp
			// http://msdn.microsoft.com/msdnmag/issues/02/08/CQA/
			// http://msdn.microsoft.com/netframework/programming/netcf/cffaq/default.aspx

			IntPtr msgDllHandle = PInvoke.LoadLibraryEx (msgDll, IntPtr.Zero,
				LoadFlags.LibraryAsDataFile);
			if (msgDllHandle == IntPtr.Zero)
				// TODO: write warning
				return null;

			IntPtr lpMsgBuf = IntPtr.Zero;
			IntPtr [] arguments = new IntPtr [replacementStrings.Length];

			try {
				for (int i = 0; i < replacementStrings.Length; i++) {
					arguments [i] = Marshal.StringToHGlobalAuto (
						replacementStrings [i]);
				}

				int ret = PInvoke.FormatMessage (FormatMessageFlags.ArgumentArray |
					FormatMessageFlags.FromHModule | FormatMessageFlags.AllocateBuffer,
					msgDllHandle, messageID, 0, ref lpMsgBuf, 0, arguments);
				if (ret != 0) {
					string sRet = Marshal.PtrToStringAuto (lpMsgBuf);
					lpMsgBuf = PInvoke.LocalFree (lpMsgBuf);
					// remove trailing whitespace (CRLF)
					return sRet.TrimEnd (null);
				} else {
					int err = Marshal.GetLastWin32Error ();
					if (err == MESSAGE_NOT_FOUND) {
						// do not consider this a failure (or even warning) as
						// multiple message resource DLLs may have been configured
						// and as such we just need to try the next library if
						// the current one does not contain a message for this
						// ID
					} else {
						// TODO: report warning
					}
				}
			} finally {
				// release unmanaged memory allocated for replacement strings
				for (int i = 0; i < arguments.Length; i++) {
					IntPtr argument = arguments [i];
					if (argument != IntPtr.Zero)
						Marshal.FreeHGlobal (argument);
				}

				PInvoke.FreeLibrary (msgDllHandle);
			}
			return null;
		}

		private string [] GetMessageResourceDlls (string source, string valueName)
		{
			// Some event sources (such as Userenv) have multiple message
			// resource DLLs, delimited by a semicolon.

			RegistryKey sourceKey = FindSourceKeyByName (source,
				CoreEventLog.MachineName, false);
			if (sourceKey != null) {
				string value = sourceKey.GetValue (valueName) as string;
				if (value != null) {
					string [] msgResDlls = value.Split (';');
					return msgResDlls;
				}
			}
			return new string [0];
		}

		private IntPtr ReadHandle {
			get {
				if (_readHandle != IntPtr.Zero)
					return _readHandle;

				string logName = CoreEventLog.GetLogName ();
				_readHandle = PInvoke.OpenEventLog (CoreEventLog.MachineName,
					logName);
				if (_readHandle == IntPtr.Zero)
					throw new InvalidOperationException (string.Format (
						CultureInfo.InvariantCulture, "Event Log '{0}' on computer"
						+ " '{1}' cannot be opened.", logName, CoreEventLog.MachineName),
						new Win32Exception ());
				return _readHandle;
			}
		}

		private IntPtr RegisterEventSource ()
		{
			IntPtr hEventLog = PInvoke.RegisterEventSource (
				CoreEventLog.MachineName, CoreEventLog.Source);
			if (hEventLog == IntPtr.Zero) {
				throw new InvalidOperationException (string.Format (
					CultureInfo.InvariantCulture, "Event source '{0}' on computer"
					+ " '{1}' cannot be opened.", CoreEventLog.Source,
					CoreEventLog.MachineName), new Win32Exception ());
			}
			return hEventLog;
		}

		public override void DisableNotification ()
		{
			if (_notifyResetEvent != null) {
				_notifyResetEvent.Close ();
				_notifyResetEvent = null;
			}

			if (_notifyThread != null) {
				if (_notifyThread.ThreadState == System.Threading.ThreadState.Running)
					_notifyThread.Abort ();
				_notifyThread = null;
			}
		}

		public override void EnableNotification ()
		{
			_notifyResetEvent = new ManualResetEvent (false);
			_lastEntryWritten = OldestEventLogEntry + EntryCount;
			if (PInvoke.NotifyChangeEventLog (ReadHandle, _notifyResetEvent.Handle) == 0)
				throw new InvalidOperationException (string.Format (
					CultureInfo.InvariantCulture, "Unable to receive notifications"
					+ " for log '{0}' on computer '{1}'.", CoreEventLog.GetLogName (),
					CoreEventLog.MachineName), new Win32Exception ());
			_notifyThread = new Thread (new ThreadStart (NotifyEventThread));
			_notifyThread.IsBackground = true;
			_notifyThread.Start ();
		}

		private void NotifyEventThread ()
		{
			while (true) {
				_notifyResetEvent.WaitOne ();
				lock (this) {
					// after a clear, we something get notified
					// twice for the same entry
					if (_notifying)
						return;
					_notifying = true;
				}

				try {
					int oldest_entry = OldestEventLogEntry;
					if (_lastEntryWritten < oldest_entry)
						_lastEntryWritten = oldest_entry;
					int current_entry = _lastEntryWritten - oldest_entry;
					int last_entry = EntryCount + oldest_entry;
					for (int i = current_entry; i < (last_entry - 1); i++) {
						EventLogEntry entry = GetEntry (i);
						CoreEventLog.OnEntryWritten (entry);
					}
					_lastEntryWritten = last_entry;
				} finally {
					lock (this)
						_notifying = false;
				}
			}
		}

#if NET_2_0
		public override OverflowAction OverflowAction {
			get { throw new NotImplementedException (); }
		}

		public override int MinimumRetentionDays {
			get { throw new NotImplementedException (); }
		}

		public override long MaximumKilobytes {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public override void ModifyOverflowPolicy (OverflowAction action, int retentionDays)
		{
			throw new NotImplementedException ();
		}

		public override void RegisterDisplayName (string resourceFile, long resourceId)
		{
			throw new NotImplementedException ();
		}
#endif

		private class PInvoke
		{
			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int ClearEventLog (IntPtr hEventLog, string lpBackupFileName);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int CloseEventLog (IntPtr hEventLog);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int DeregisterEventSource (IntPtr hEventLog);

			[DllImport ("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
			public static extern int FormatMessage (FormatMessageFlags dwFlags, IntPtr lpSource, uint dwMessageId, int dwLanguageId, ref IntPtr lpBuffer, int nSize, IntPtr [] arguments);

			[DllImport ("kernel32.dll", SetLastError=true)]
			public static extern bool FreeLibrary (IntPtr hModule);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int GetNumberOfEventLogRecords (IntPtr hEventLog, ref int NumberOfRecords);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int GetOldestEventLogRecord (IntPtr hEventLog, ref int OldestRecord);

			[DllImport ("kernel32.dll", SetLastError=true)]
			public static extern IntPtr LoadLibraryEx (string lpFileName, IntPtr hFile, LoadFlags dwFlags);

			[DllImport ("kernel32.dll", SetLastError=true)]
			public static extern IntPtr LocalFree (IntPtr hMem);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern bool LookupAccountSid (
				string lpSystemName,
				[MarshalAs (UnmanagedType.LPArray)] byte [] Sid,
				StringBuilder lpName,
				ref uint cchName,
				StringBuilder ReferencedDomainName,
				ref uint cchReferencedDomainName,
				out SidNameUse peUse);

			[DllImport ("Advapi32.dll", SetLastError = true)]
			public static extern int NotifyChangeEventLog (IntPtr hEventLog, IntPtr hEvent);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern IntPtr OpenEventLog (string machineName, string logName);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern IntPtr RegisterEventSource (string machineName, string sourceName);

			[DllImport ("Advapi32.dll", SetLastError=true)]
			public static extern int ReportEvent (IntPtr hHandle, ushort wType,
				ushort wCategory, uint dwEventID, IntPtr sid, ushort wNumStrings,
				uint dwDataSize, string [] lpStrings, byte [] lpRawData);

			[DllImport ("advapi32.dll", SetLastError=true)]
			public static extern int ReadEventLog (IntPtr hEventLog, ReadFlags dwReadFlags, int dwRecordOffset, byte [] buffer, int nNumberOfBytesToRead, ref int pnBytesRead, ref int pnMinNumberOfBytesNeeded);

			public const int ERROR_INSUFFICIENT_BUFFER = 122;
			public const int ERROR_EVENTLOG_FILE_CHANGED = 1503;
		}

		private enum ReadFlags
		{
			Sequential = 0x001,
			Seek = 0x002,
			ForwardsRead = 0x004,
			BackwardsRead = 0x008
		}

		private enum LoadFlags: uint
		{
			LibraryAsDataFile = 0x002
		}

		[Flags]
		private enum FormatMessageFlags
		{
			AllocateBuffer = 0x100,
			IgnoreInserts = 0x200,
			FromHModule = 0x0800,
			FromSystem = 0x1000,
			ArgumentArray = 0x2000
		}

		private enum SidNameUse
		{
			User = 1,
			Group,
			Domain,
			lias,
			WellKnownGroup,
			DeletedAccount,
			Invalid,
			Unknown,
			Computer
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
//	short EventType;
//	short NumStrings;
//	short EventCategory;
//	short ReservedFlags;
//	int ClosingRecordNumber;
//	int StringOffset;
//	int UserSidLength;
//	int UserSidOffset;
//	int DataLength;
//	int DataOffset;
// }
//
// http://www.whitehats.ca/main/members/Malik/malik_eventlogs/malik_eventlogs.html
