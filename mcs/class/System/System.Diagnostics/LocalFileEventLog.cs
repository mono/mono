//
// LocalFileEventLog.cs
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
	class LocalFileEventLogUtil
	{
		public const string DateFormat = "yyyyMMddHHmmssfff";

		static readonly string path;

		static LocalFileEventLogUtil ()
		{
			string env = Environment.GetEnvironmentVariable ("MONO_LOCAL_EVENTLOG_PATH");
			if (env != null)
				path = Path.GetFullPath (env);
		}

		public static bool IsEnabled {
			get { return path != null && Directory.Exists (path); }
		}

		public static string GetSourceDir (string source)
		{
			foreach (string log in GetLogDirectories ()) {
				string sd = Path.Combine (log, source);
				if (Directory.Exists (sd))
					return sd;
			}
			return null;
		}

		public static string GetLogDir (string logName)
		{
			return Path.Combine (Path.Combine (path, "logs"), logName);
		}

		public static string [] GetLogDirectories ()
		{
			return Directory.GetDirectories (Path.Combine (path, "logs"));
		}
	}

	class LocalFileEventLog : EventLogImpl
	{
		static readonly string [] empty_strings = new string [0];

		EventLog log;
		string source_path;

		public LocalFileEventLog (EventLog log)
			: base (log)
		{
			this.log = log;
			source_path = LocalFileEventLogUtil.GetSourceDir (log.Source);
			if (!Directory.Exists (source_path))
				throw new SystemException (String.Format ("INTERNAL ERROR: directory for {0} does not exist.", log.Source));
		}

		public override EventLogEntryCollection Entries {
			get {
				ArrayList list = new ArrayList ();
				int index = 0;
				foreach (string file in Directory.GetFiles (source_path, "*.log"))
					list.Add (LoadLogEntry (file, index++));
				return new EventLogEntryCollection ((EventLogEntry []) list.ToArray (typeof (EventLogEntry)));
			}
		}

		public override string LogDisplayName {
			get { return log.Log; }
		}

		EventLogEntry LoadLogEntry (string file, int index)
		{
			using (TextReader tr = File.OpenText (file)) {
				int id = int.Parse (tr.ReadLine ().Substring (9));
				EventLogEntryType type = (EventLogEntryType)
					Enum.Parse (typeof (EventLogEntryType), tr.ReadLine ().Substring (11));
				string category = tr.ReadLine ().Substring (10);
				int size = int.Parse (tr.ReadLine ().Substring (15));
				char [] buf = new char [size];
				tr.Read (buf, 0, size);
				string filename = Path.GetFileName (file).Substring (0, LocalFileEventLogUtil.DateFormat.Length);
				DateTime date = DateTime.ParseExact (filename, LocalFileEventLogUtil.DateFormat, CultureInfo.InvariantCulture);
				byte [] bin = Convert.FromBase64String (tr.ReadToEnd ());
				// FIXME: categoryNumber, index, userName, two dates
				return new EventLogEntry (category, 0, index,
					id, new string (buf), log.Source, "", log.MachineName,
					type, date, date, bin, empty_strings);
			}
		}

		public override void BeginInit ()
		{
		}

		public override void Clear ()
		{
			foreach (string file in Directory.GetFiles (source_path, "*.log"))
				File.Delete (file);
		}

		public override void Close ()
		{
		}

		public override void Dispose (bool disposing)
		{
			Close ();
		}

		public override void EndInit ()
		{
		}
	}

	// Creates a log repository at MONO_LOCAL_EVENTLOG_DIR, which consists of
	// 	- 
	internal class LocalFileEventLogFactory : EventLogFactory
	{
		static readonly IPAddress local_ip = IPAddress.Parse ("127.0.0.1");

		public LocalFileEventLogFactory ()
		{
		}

		public override EventLogImpl Create (EventLog log)
		{
			if (!SourceExists (log.Source, log.MachineName))
				CreateEventSource (log.Source, log.Log, log.MachineName);
			return new LocalFileEventLog (log);
		}

		void VerifyMachine (string machineName)
		{
			if (machineName != ".") {
				IPHostEntry entry =
#if NET_2_0
					Dns.GetHostEntry (machineName);
#else
					Dns.Resolve (machineName);
#endif
				if (Array.IndexOf (entry.AddressList, local_ip) < 0)
					throw new NotSupportedException (String.Format ("LocalFileEventLog does not support remote machine: {0}", machineName));
			}
		}

		public override void CreateEventSource (string source, string logName, string machineName)
		{
			VerifyMachine (machineName);

			string sourceDir = LocalFileEventLogUtil.GetSourceDir (source);
			if (sourceDir != null)
				throw new ArgumentException (String.Format ("Source '{0}' already exists on the local machine.", source));

			string logDir = LocalFileEventLogUtil.GetLogDir (logName);
			if (!Directory.Exists (logDir))
				Directory.CreateDirectory (logDir);
			Directory.CreateDirectory (Path.Combine (logDir, source));
		}

		public override void Delete (string logName, string machineName)
		{
			VerifyMachine (machineName);

			string logDir = LocalFileEventLogUtil.GetLogDir (logName);
			if (Directory.Exists (logDir))
				Directory.Delete (logDir);
		}

		public override void DeleteEventSource (string source, string machineName)
		{
			VerifyMachine (machineName);

			string sourceDir = LocalFileEventLogUtil.GetSourceDir (source);
			if (Directory.Exists (sourceDir))
				Directory.Delete (sourceDir);
			else
				throw new ArgumentException (String.Format ("Event source '{0}' does not exist on the local machine."), source);
		}

		public override bool Exists (string logName, string machineName)
		{
			VerifyMachine (machineName);

			return Directory.Exists (LocalFileEventLogUtil.GetLogDir (logName));
		}

		public override EventLog[] GetEventLogs (string machineName)
		{
			VerifyMachine (machineName);

			ArrayList al = new ArrayList ();
			foreach (string log in LocalFileEventLogUtil.GetLogDirectories ())
				al.Add (new EventLog (log));
			return (EventLog []) al.ToArray (typeof (EventLog));
		}

		public override string LogNameFromSourceName (string source, string machineName)
		{
			VerifyMachine (machineName);

			string sourceDir = LocalFileEventLogUtil.GetSourceDir (source);
			if (sourceDir == null)
				throw new ArgumentException (String.Format ("Event source '{0}' does not exist on the local machine."), source);
			return Directory.GetParent (sourceDir).Name;
		}

		public override bool SourceExists (string source, string machineName)
		{
			VerifyMachine (machineName);

			return LocalFileEventLogUtil.GetSourceDir (source) != null;
		}

		public override void WriteEntry (string source, string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
		{
			if (!SourceExists (source, "."))
				throw new ArgumentException (String.Format ("Event source '{0}' does not exist on the local machine."), source);
			string sourceDir = LocalFileEventLogUtil.GetSourceDir (source);
			string path = Path.Combine (sourceDir, DateTime.Now.ToString (LocalFileEventLogUtil.DateFormat) + ".log");
			try {
				using (TextWriter w = File.CreateText (path)) {
					w.WriteLine ("EventID: {0}", eventID);
					w.WriteLine ("EntryType: {0}", type);
					w.WriteLine ("Category: {0}", category);
					w.WriteLine ("MessageLength: {0}", message.Length);
					w.Write (message);
					if (rawData != null)
						w.Write (Convert.ToBase64String (rawData));
				}
			} catch (IOException) {
				File.Delete (path);
			}
		}
	}
}
