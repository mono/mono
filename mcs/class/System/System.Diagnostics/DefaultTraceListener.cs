//
// System.Diagnostics.DefaultTraceListener.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Diagnostics {

	[ComVisible(false)]
	public class DefaultTraceListener : TraceListener {

		private static readonly bool OnWin32;

		private static readonly string ConsoleOutTrace = "Console.Out";
		private static readonly string ConsoleErrorTrace = "Console.Error";

		private static readonly string MonoTracePrefix;
		private static readonly string MonoTraceFile;

		static DefaultTraceListener ()
		{
			// Determine what platform we're on.  This impacts how where we send
			// messages.  On Win32 platforms (OnWin32 = true), we use the
			// `OutputDebugString' api.
			//
			// On Linux platforms, we use MONO_TRACE to figure things out.  See the
			// API documentation for more information on MONO_TRACE.
			OnWin32 = (Path.DirectorySeparatorChar == '\\');

			if (!OnWin32) {
				// If we're running on Unix, we don't have OutputDebugString.
				// Instead, send output to...wherever the MONO_TRACE environment
				// variables says to.
				String where = Environment.GetEnvironmentVariable("MONO_TRACE");

				if (where != null) {
					string file = null;
					string prefix = null;

					if (where.StartsWith (ConsoleOutTrace)) {
						file = ConsoleOutTrace;
						prefix = GetPrefix (where, ConsoleOutTrace);
					}
					else if (where.StartsWith (ConsoleErrorTrace)) {
						file = ConsoleErrorTrace;
						prefix = GetPrefix (where, ConsoleErrorTrace);
					}
					else {
						file = where;

						// We can't firgure out what the prefix would be, as ':' is a
						// valid filename character.  Thus, arbitrary files don't support
						// prefixes.
						//
						// I don't consider this to be a major issue.  Prefixes are useful 
						// with Console.Out and Console.Error to help separate trace
						// output from the actual program output.  Writing to an arbitrary
						// file doesn't introduce issues with disambiguation.
						prefix = "";
					}

					MonoTraceFile = file;
					MonoTracePrefix = prefix;
				}
			}
		}

		/**
		 * Get the prefix for the specified variable.
		 *
		 * "Prefixes" are used in the MONO_TRACE variable, and specify text that
		 * should precede each message printed to the console.  The prefix is
		 * appended to the console location with a colon (':') separating them.
		 * For example, if MONO_TRACE is "Console.Out:** my prefix", the prefix is
		 * "** my prefix".
		 *
		 * Everything after the colon, if the colon is present, is used as the
		 * prefix.
		 *
		 * @param	var		The current MONO_TRACE variable
		 * @param	where	The name of the output location, e.g. "Console.Out"
		 */
		private static string GetPrefix (string var, string where)
		{
			// actually, we permit any character to separate `where' and the prefix;
			// we just skip over where the ':' would be.  This means that a space or
			// anything else would suffice, as long as it was only a single
			// character.
			if (var.Length > where.Length)
				return var.Substring (where.Length + 1);
			return "";
		}

		private string logFileName = null;

		public DefaultTraceListener () : base ("Default")
		{
		}

		// It's hard to do anything with a UI when we don't have Windows.Forms...
		[MonoTODO]
		public bool AssertUiEnabled {
			get {return false;}
			set {/* ignore */}
		}

		[MonoTODO]
		public string LogFileName {
			get {return logFileName;}
			set {logFileName = value;}
		}

		public override void Fail (string message)
		{
			base.Fail (message);
			WriteLine (new StackTrace().ToString());
		}

		public override void Fail (string message, string detailMessage)
		{
			base.Fail (message, detailMessage);
			WriteLine (new StackTrace().ToString());
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void WriteWindowsDebugString (string message);

		private void WriteDebugString (string message)
		{
			if (OnWin32)
				WriteWindowsDebugString (message);
			else
				WriteMonoTrace (message);
		}

		private void WriteMonoTrace (string message)
		{
			switch (MonoTraceFile) {
			case "Console.Out":
				Console.Out.Write (message);
				break;
			case "Console.Error":
				Console.Error.Write (message);
				break;
			default:
				WriteLogFile (message, MonoTraceFile);
				break;
			}
		}

		private void WritePrefix ()
		{
			if (!OnWin32) {
				WriteMonoTrace (MonoTracePrefix);
			}
		}

		private void WriteImpl (string message)
		{
			if (NeedIndent) {
				WriteIndent ();
				WritePrefix ();
			}

			WriteDebugString (message);

			if (Debugger.IsLogging())
				Debugger.Log (0, null, message);

			WriteLogFile (message, LogFileName);
		}

		private void WriteLogFile (string message, string logFile)
		{
			string fname = logFile;
			if (fname != null && fname.Length != 0) {
				FileInfo info = new FileInfo (fname);
				StreamWriter sw = null;

				// Open the file
				try {
					if (info.Exists)
						sw = info.AppendText ();
					else
						sw = info.CreateText ();
				}
				catch {
					// We weren't able to open the file for some reason.
					// We can't write to the log file; so give up.
					return;
				}

				using (sw) {
					sw.Write (message);
					sw.Flush ();
				}
			}
		}

		public override void Write (string message)
		{
			WriteImpl (message);
		}

		public override void WriteLine (string message)
		{
			string msg = message + Environment.NewLine;
			WriteImpl (msg);

			NeedIndent = true;
		}
	}
}

