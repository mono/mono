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
using System.Runtime.InteropServices;

namespace System.Diagnostics {

	[ComVisible(false)]
	public class DefaultTraceListener : TraceListener {

		private string logFileName = null;

		public DefaultTraceListener () : base ("Default")
		{
		}

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

 		public override void Fail(string message, string detailMessage)
 		{
      base.Fail (message, detailMessage);
      WriteLine (new StackTrace().ToString());
 		}

		#if USE_NATIVE_WIN32_OUTPUT_DEBUG_STRING

      [DllImport ("kernel32.dll")]
      private extern static void OutputDebugString (string message);

		#else

      private static void OutputDebugString (string message)
      {
        Console.Write ("**ods** " + message);
      }

		#endif

		private void WriteImpl (string message)
		{
			if (NeedIndent)
				WriteIndent ();

			OutputDebugString (message);

			if (Debugger.IsLogging())
				Debugger.Log (0, null, message);

			WriteLogFile (message);
		}

		private void WriteLogFile (string message)
		{
			string fname = LogFileName;
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

