//
// Helper for Console to allow indirect access to `stdout` using NSLog
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2014 Xamarin Inc. All rights reserved.
//

#if MONOTOUCH

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace System {

	public static partial class Console {

		class NSLogWriter : TextWriter {
			[DllImport ("__Internal", CharSet=CharSet.Unicode)]
			extern static void xamarin_log (string s);

			[DllImport ("/usr/lib/libSystem.dylib")]
			extern static /* ssize_t */ IntPtr write (int fd, byte [] buffer, /* size_t */ IntPtr n);
			
			StringBuilder sb;
			
			public NSLogWriter ()
			{
				sb = new StringBuilder ();
			}
			
			public override System.Text.Encoding Encoding {
				get { return System.Text.Encoding.UTF8; }
			}

			static void direct_write_to_stdout (string s)
			{
				byte [] b = Encoding.Default.GetBytes (s);
				var len = (IntPtr) b.Length;
				while ((int) write (1, b, len) == -1 && Marshal.GetLastWin32Error () == /* EINTR*/ 4)
					;
			}
			
			public override void Flush ()
			{
				string s;
				lock (sb) {
					s = sb.ToString ();
					sb.Length = 0;
				}
				try {
					xamarin_log (s);
				}
				catch (Exception) {
					try {
						direct_write_to_stdout (s);
						direct_write_to_stdout (Environment.NewLine);
					} catch (Exception){}
				}
			}
			
			// minimum to override - see http://msdn.microsoft.com/en-us/library/system.io.textwriter.aspx
			public override void Write (char value)
			{
				try {
					lock (sb)
						sb.Append (value);
				}
				catch (Exception) {
				}
			}
			
			// optimization (to avoid concatening chars)
			public override void Write (string value)
			{
				try {
					lock (sb) {
						sb.Append (value);
						if (EndsWithNewLine (sb))
							Flush ();
					}
				}
				catch (Exception) {
				}
			}

			/* Called from TextWriter:WriteLine(string) */
			public override void Write(char[] buffer, int index, int count) {
				try {
					lock (sb) {
						sb.Append (buffer, index, count);
						if (EndsWithNewLine (sb))
							Flush ();
					}
				}
				catch (Exception) {
				}
			}
			
			bool EndsWithNewLine (StringBuilder value)
			{
				if (value.Length < CoreNewLine.Length)
					return false;

				for (int i = 0, v = value.Length - CoreNewLine.Length; i < CoreNewLine.Length; ++i, ++v) {
					if (value [v] != CoreNewLine [i])
						return false;
				}

				return true;
			}
			
			public override void WriteLine ()
			{
				try {
					Flush ();
				}
				catch (Exception) {
				}
			}
		}
	}
}

#endif
