//
// FileLogger.cs: Logs to file
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
// 
// (C) 2005 Marek Sieradzki
// Copyright 2011 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	public class FileLogger : ConsoleLogger {
		StreamWriter sw;
		string logfile;
		string encoding = null;
		IEventSource eventSource;

		public FileLogger ()
		{
			base.Verbosity = LoggerVerbosity.Detailed;
		}

		public override void Initialize (IEventSource eventSource)
		{
			this.eventSource = eventSource;
			logfile = "msbuild.log";

			bool append = false;
			string key, value;
			string[] splittedParameters = Parameters.Split (new char [] {';'}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string s in splittedParameters) {
				if (String.Compare (s, "Append") == 0) {
					append = true;
					continue;
				}

				if (s.StartsWith ("Encoding")) {
					if (!TrySplitKeyValuePair (s, out key, out value))
						throw new LoggerException ("Encoding should be specified as: Encoding=<encoding>, eg. Encoding=UTF-8");

					if (String.IsNullOrEmpty (value))
						throw new LoggerException ("Encoding must be non-empty");

					encoding = value;
					continue;
				}

				if (s.StartsWith ("LogFile")) {
					if (!TrySplitKeyValuePair (s, out key, out value))
						throw new LoggerException ("LogFile should be specified as: LogFile=<encoding>, eg. LogFile=foo.log");

					if (String.IsNullOrEmpty (value))
						throw new LoggerException ("LogFile value must be non-empty");

					logfile = value;
					continue;
				}
			}

			// Attach *our* HandleBuildStarted as the first one,
			// as it needs to create the writer!
			eventSource.BuildStarted += HandleBuildStarted;
			base.Initialize (eventSource);
			// Attach *our* HandleBuildFinished as the last one,
			// as it needs to close the writer!
			eventSource.BuildFinished += HandleBuildFinished;

			CreateWriter (append);
		}

		void CreateWriter (bool append_to)
		{
			if (sw != null)
				return;

			if (!String.IsNullOrEmpty (encoding))
				sw = new StreamWriter (logfile, append_to, Encoding.GetEncoding (encoding));
			else
				sw = new StreamWriter (logfile, append_to, Encoding.Default);
			WriteHandler = sw.WriteLine;
		}

		void HandleBuildStarted (object sender, BuildStartedEventArgs args)
		{
			CreateWriter (true);
		}

		void HandleBuildFinished (object sender, BuildFinishedEventArgs args)
		{
			base.WriteHandler = null;
			if (sw != null) {
				sw.Close ();
				sw = null;
			}
		}

		bool TrySplitKeyValuePair (string pair, out string key, out string value)
		{
			key = value = null;
			string[] parts = pair.Split ('=');
			if (parts.Length != 2)
				return false;

			key = parts [0];
			value = parts [1];
			return true;
		}

		public override void Shutdown ()
		{
			base.WriteHandler = null;
			if (sw != null) {
				sw.Close ();
				sw = null;
			}

			if (eventSource != null) {
				eventSource.BuildStarted -= HandleBuildStarted;
				eventSource.BuildFinished -= HandleBuildFinished;
			}

			base.Shutdown ();
		}
	}
}

#endif
