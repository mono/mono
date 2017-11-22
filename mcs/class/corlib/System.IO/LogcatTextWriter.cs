#if MONODROID

using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.IO {

	class LogcatTextWriter : TextWriter {

		const string LibLog = "/system/lib/liblog.so";
		const string LibLog64 = "/system/lib64/liblog.so";

		readonly byte[] appname;

		TextWriter stdout;
		StringBuilder line = new StringBuilder ();

		public LogcatTextWriter (string appname, TextWriter stdout)
		{
			this.appname = Encoding.UTF8.GetBytes (appname);
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

			unsafe {
				fixed (byte *b_appname = appname)
				fixed (byte *b_message = Encoding.UTF8.GetBytes(o)) {
					Log (b_appname, 1 << 5 /* G_LOG_LEVEL_MESSAGE */, b_message);
				}
			}
			stdout.WriteLine (o);
		}

		public static bool IsRunningOnAndroid ()
		{
			return File.Exists (LibLog) || File.Exists (LibLog64);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		static unsafe extern void Log (byte *appname, int level, byte *message);
	}
}

#endif  // MONODROID
