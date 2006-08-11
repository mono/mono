//
// System.Diagnostics.UnixEventLog.cs
//
// Author:
//	Gert Driesen <driesen@users.sourceforge.net>
//	Atsushi Enum <atsushi@ximian.com>
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

namespace System.Diagnostics
{
	internal class UnixEventLog : EventLogImpl
	{
		const string DateFormat = "yyyyMMddHHmmssfff";
		static readonly object lockObject = new object ();

		public UnixEventLog (EventLog coreEventLog) : base (coreEventLog)
		{
		}

		public override void BeginInit () {
		}

		public override void Clear ()
		{
			if (!Directory.Exists (FileStore))
				return;

			foreach (string file in Directory.GetFiles (FileStore, "*.log"))
				File.Delete (file);
			Directory.Delete (FileStore);
		}

		public override void Close ()
		{
			// we don't hold any unmanaged resources
		}

		public override void Dispose (bool disposing)
		{
			Close ();
		}

		public override void EndInit () { }

		public override EventLogEntry[] GetEntries ()
		{
			if (!Directory.Exists (FileStore))
				return new EventLogEntry [0];

			int entryCount = GetEntryCount ();
			EventLogEntry [] entries = new EventLogEntry [entryCount];
			for (int i = 0; i < entryCount; i++) {
				entries [i] = GetEntry (i);
			}
			return entries;
		}

		protected override int GetEntryCount ()
		{
			if (!Directory.Exists (FileStore))
				return 0;

			string[] logFiles = Directory.GetFiles (FileStore, "*.log");
			return logFiles.Length;
		}

		protected override EventLogEntry GetEntry (int index)
		{
			// our file names are one-based
			string file = Path.Combine (FileStore, (index + 1).ToString (
				CultureInfo.InvariantCulture) + ".log");

			using (TextReader tr = File.OpenText (file)) {
				int eventIndex = int.Parse (Path.GetFileNameWithoutExtension (file),
					CultureInfo.InvariantCulture);
				int id = int.Parse (tr.ReadLine ().Substring (9));
				long instanceId = long.Parse (tr.ReadLine ().Substring (12),
					CultureInfo.InvariantCulture);
				EventLogEntryType type = (EventLogEntryType)
					Enum.Parse (typeof (EventLogEntryType), tr.ReadLine ().Substring (11));
				string source = tr.ReadLine ().Substring (8);
				string category = tr.ReadLine ().Substring (10);
				short categoryNumber = short.Parse(category, CultureInfo.InvariantCulture);
				string categoryName = "(" + category + ")";
				DateTime date = DateTime.ParseExact (tr.ReadLine ().Substring (15),
					DateFormat, CultureInfo.InvariantCulture);
				int size = int.Parse (tr.ReadLine ().Substring (15));
				char [] buf = new char [size];
				tr.Read (buf, 0, size);
				ArrayList replacementTemp = new ArrayList ();
				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < buf.Length; i++) {
					char c = buf[i];
					if (c == '\0') {
						replacementTemp.Add (sb.ToString ());
						sb.Length = 0;
					} else {
						sb.Append (c);
					}
				}
				if (sb.Length > 0) {
					replacementTemp.Add (sb.ToString ());
				}
				string [] replacementStrings = new string [replacementTemp.Count];
				replacementTemp.CopyTo (replacementStrings, 0);
				DateTime timeWritten = File.GetLastWriteTime (file);

				byte [] bin = Convert.FromBase64String (tr.ReadToEnd ());
				return new EventLogEntry (categoryName, categoryNumber, eventIndex,
					id, source, new string(buf), null, Environment.MachineName, 
					type, date, timeWritten, bin, replacementStrings, instanceId);
			}
		}

		[MonoTODO]
		protected override string GetLogDisplayName ()
		{
			return CoreEventLog.Log;
		}

		protected override void WriteEventLogEntry (EventLogEntry entry)
		{
			if (!Directory.Exists (FileStore))
				Directory.CreateDirectory (FileStore);

			lock (lockObject) {
				int index = GetNewIndex ();
				string logPath = Path.Combine (FileStore, index.ToString (CultureInfo.InvariantCulture) + ".log");
				try {
					using (TextWriter w = File.CreateText (logPath)) {
						w.WriteLine ("EventID: {0}", entry.EventID);
#if NET_2_0
						w.WriteLine ("InstanceID: {0}", entry.InstanceId);
#else
						w.WriteLine ("InstanceID: {0}", entry.EventID);
#endif
						w.WriteLine ("EntryType: {0}", entry.EntryType);
						w.WriteLine ("Source: {0}", entry.Source);
						w.WriteLine ("Category: {0}", entry.CategoryNumber.ToString (
							CultureInfo.InvariantCulture));
						w.WriteLine ("TimeGenerated: {0}", entry.TimeGenerated.ToString (
							DateFormat, CultureInfo.InvariantCulture));
						StringBuilder sb = new StringBuilder ();
						if (entry.ReplacementStrings != null) {
							for (int i = 0; i < entry.ReplacementStrings.Length; i++) {
								if (i > 0)
									sb.Append ('\0');
								string replacement = entry.ReplacementStrings[i];
								sb.Append (replacement);
							}
						}
						w.WriteLine ("MessageLength: {0}", sb.Length);
						w.Write (sb.ToString ());
						if (entry.Data != null)
							w.Write (Convert.ToBase64String (entry.Data));
					}
				} catch (IOException) {
					File.Delete (logPath);
				}
			}
		}

		private string FileStore {
			get {
				string eventLogRoot = Path.Combine (Environment.GetFolderPath (
					Environment.SpecialFolder.Personal), ".mono/eventlog");
				return Path.Combine (eventLogRoot, CoreEventLog.Log.ToLower ());
			}
		}

		private int GetNewIndex () {
			// our file names are one-based
			int maxIndex = 0;
			string[] logFiles = Directory.GetFiles (FileStore, "*.log");
			for (int i = 0; i < logFiles.Length; i++) {
				try {
					string file = logFiles[i];
					int index = int.Parse (Path.GetFileNameWithoutExtension (
						file), CultureInfo.InvariantCulture);
					if (index > maxIndex)
						maxIndex = index;
				} catch {
				}
			}
			return ++maxIndex;
		}
	}
}
