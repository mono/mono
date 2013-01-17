#if MONODROID

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO {

	class LogcatTextWriter : TextWriter {

		const string LibLog = "/system/lib/liblog.so";

		TextWriter stdout;
		readonly string appname;
		StringBuilder line = new StringBuilder ();

		public LogcatTextWriter (string appname, TextWriter stdout)
		{
			this.appname = appname;
			this.stdout = stdout;
		}

		public override Encoding Encoding {
			get {return Encoding.UTF8;}
		}

		public override void Write (string s)
		{
			if (s != null)
				foreach (char c in s)
					Write (c);
		}

		public override void Write (char value)
		{
			if (value == '\n')
				WriteLine ();
			else
				line.Append (value);
		}

		public override void WriteLine ()
		{
			var o = line.ToString ();
			line.Clear ();

			Log (LogLevel.Info, appname, o);
			stdout.WriteLine (o);
		}

		enum LogLevel {
			Unknown,
			Default,
			Verbose,
			Debug,
			Info,
			Warn,
			Error,
			Fatal,
			Silent
		}

		public static bool IsRunningOnAndroid ()
		{
			return File.Exists (LibLog);
		}

		[DllImport (LibLog)]
		static extern void __android_log_print (LogLevel level, string appname, string format, string args, IntPtr zero);

		static void Log (LogLevel level, string appname, string log)
		{
			__android_log_print (level, appname, "%s", log, IntPtr.Zero);
		}
	}
}

#endif  // MONODROID
