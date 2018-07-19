#if MONODROID

using System;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Mono;

namespace System.IO {

	class LogcatTextWriter : TextWriter {

		const string LibLog = "/system/lib/liblog.so";
		const string LibLog64 = "/system/lib64/liblog.so";

		readonly byte[] appname;

		TextWriter stdout;
		StringBuilder line = new StringBuilder ();

		public LogcatTextWriter (string appname, TextWriter stdout)
		{
			this.appname = Encoding.UTF8.GetBytes (appname + '\0');
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

			Log (o);
			stdout.WriteLine (o);
		}

		void Log (string message)
		{
			const int buffer_size = 512;

			unsafe {
				if (Encoding.UTF8.GetByteCount (message) < buffer_size) {
					try {
						fixed (char *b_message = message) {
							byte* buffer = stackalloc byte[buffer_size];
							int written = Encoding.UTF8.GetBytes (b_message, message.Length, buffer, buffer_size - 1);
							buffer [written] = (byte)'\0';

							Log (buffer);
						}

						return;
					} catch (ArgumentException) {
						/* It might be due to a failure to encode a character, or due to a smaller than necessary buffer. In the
						* secode case, we want to fallback to simply allocating on the heap. */
					}
				}

				using (SafeStringMarshal str = new SafeStringMarshal(message)) {
					Log ((byte*) str.Value);
				}
			}
		}

		unsafe void Log (byte* b_message)
		{
			fixed (byte *b_appname = appname) {
				Log (b_appname, 1 << 5 /* G_LOG_LEVEL_MESSAGE */, b_message);
			}
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
