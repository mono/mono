//
// System.Diagnostics.DefaultTraceListener.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// (C) 2002 Jonathan Pryor
// (C) 2007 Novell, Inc.
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
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Diagnostics {
#if NET_2_0
#else
	[ComVisible(false)]
#endif
	public class DefaultTraceListener : TraceListener {

		private static readonly bool OnWin32;

		private const string ConsoleOutTrace = "Console.Out";
		private const string ConsoleErrorTrace = "Console.Error";

		private static readonly string MonoTracePrefix;
		private static readonly string MonoTraceFile;

		static DefaultTraceListener ()
		{
			// Determine what platform we're on.  This impacts how where we send
			// messages.  On Win32 platforms (OnWin32 = true), we use the
			// `OutputDebugString' api.
			//
			// On Linux platforms, we use MONO_TRACE_LISTENER to figure things out.  See the
			// API documentation for more information on MONO_TRACE_LISTENER.
			OnWin32 = (Path.DirectorySeparatorChar == '\\');

			if (!OnWin32) {
#if TARGET_JVM
				string trace = java.lang.System.getProperty("MONO_TRACE");
#else
				// If we're running on Unix, we don't have OutputDebugString.
				// Instead, send output to...wherever the MONO_TRACE_LISTENER environment
				// variables says to.
				String trace = Environment.GetEnvironmentVariable("MONO_TRACE_LISTENER");
#endif

				if (trace != null) {
					string file = null;
					string prefix = null;

					if (trace.StartsWith (ConsoleOutTrace)) {
						file = ConsoleOutTrace;
						prefix = GetPrefix (trace, ConsoleOutTrace);
					}
					else if (trace.StartsWith (ConsoleErrorTrace)) {
						file = ConsoleErrorTrace;
						prefix = GetPrefix (trace, ConsoleErrorTrace);
					}
					else {
						file = trace;

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
		 * "Prefixes" are used in the MONO_TRACE_LISTENER variable, and specify text that
		 * should precede each message printed to the console.  The prefix is
		 * appended to the console location with a colon (':') separating them.
		 * For example, if MONO_TRACE_LISTENER is "Console.Out:** my prefix", the prefix is
		 * "** my prefix".
		 *
		 * Everything after the colon, if the colon is present, is used as the
		 * prefix.
		 *
		 * @param	var		The current MONO_TRACE_LISTENER variable
		 * @param	target	The name of the output location, e.g. "Console.Out"
		 */
		private static string GetPrefix (string var, string target)
		{
			// actually, we permit any character to separate `target' and the prefix;
			// we just skip over target the ':' would be.  This means that a space or
			// anything else would suffice, as long as it was only a single
			// character.
			if (var.Length > target.Length)
				return var.Substring (target.Length + 1);
			return "";
		}

		private string logFileName = null;

		private bool assertUiEnabled = false;

		public DefaultTraceListener () : base ("Default")
		{
		}

		public bool AssertUiEnabled {
			get { return assertUiEnabled; }
			set { assertUiEnabled = value; }
		}

		[MonoTODO]
		public string LogFileName {
			get {return logFileName;}
			set {logFileName = value;}
		}

		public override void Fail (string message)
		{
			base.Fail (message);
		}

		public override void Fail (string message, string detailMessage)
		{
			base.Fail (message, detailMessage);
			if (ProcessUI (message, detailMessage) == DialogResult.Abort)
				Thread.CurrentThread.Abort ();
			WriteLine (new StackTrace().ToString());
		}

		DialogResult ProcessUI (string message, string detailMessage)
		{
			
			if (!AssertUiEnabled)
				return DialogResult.None;

			object messageBoxButtonsAbortRetryIgnore;
			MethodInfo msgboxShow;
			
			try {
				Assembly wfAsm = Assembly.Load (Consts.AssemblySystem_Windows_Forms);
				if (wfAsm == null)
				    return DialogResult.None;
				
				Type buttons = wfAsm.GetType ("System.Windows.Forms.MessageBoxButtons");
				messageBoxButtonsAbortRetryIgnore = Enum.Parse (buttons, "AbortRetryIgnore");
				msgboxShow = wfAsm.GetType ("System.Windows.Forms.MessageBox").GetMethod (
					"Show",
					new Type [] {typeof (string), typeof (string), buttons});
			} catch {
				return DialogResult.None;
			}

			if (msgboxShow == null || messageBoxButtonsAbortRetryIgnore == null)
				return DialogResult.None;

			string caption = String.Format ("Assertion Failed: {0} to quit, {1} to debug, {2} to continue", "Abort", "Retry", "Ignore");
			string msg = String.Format ("{0}{1}{2}{1}{1}{3}", message, Environment.NewLine, detailMessage, new StackTrace ());

			switch (msgboxShow.Invoke (null, new object [] {msg, caption, messageBoxButtonsAbortRetryIgnore}).ToString ()) {
			case "Ignore":
				return DialogResult.Ignore;
			case "Abort":
				return DialogResult.Abort;
			default:
				return DialogResult.Retry;
			}
		}

		enum DialogResult {
			None,
			Retry,
			Ignore,
			Abort
		}

#if TARGET_JVM
		private void WriteDebugString (string message)
		{
#else
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void WriteWindowsDebugString (string message);

		private void WriteDebugString (string message)
		{
			if (OnWin32)
				WriteWindowsDebugString (message);
			else
#endif
				WriteMonoTrace (message);
		}

		private void WriteMonoTrace (string message)
		{
			switch (MonoTraceFile) {
			case ConsoleOutTrace:
				Console.Out.Write (message);
				break;
			case ConsoleErrorTrace:
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

