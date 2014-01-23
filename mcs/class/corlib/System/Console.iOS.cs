//
// Helper for Console to allow indirect access to `stdout` using NSLog
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2013 Xamarin Inc. All rights reserved.
//

#if FULL_AOT_RUNTIME

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace System {

	public static partial class Console {

		class NSLogWriter : TextWriter {
			[DllImport ("__Internal", CharSet=CharSet.Unicode)]
			extern static void monotouch_log (string s);

			[DllImport ("/usr/lib/libSystem.dylib")]
			extern static int write (int fd, byte [] buffer, int n);
			
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
				while (write (1, b, b.Length) == -1 && Marshal.GetLastWin32Error () == /* EINTR*/ 4)
					;
			}
			
			public override void Flush ()
			{
				string s = sb.ToString ();
				try {
					monotouch_log (s);
				}
				catch (Exception) {
					try {
						direct_write_to_stdout (s);
						direct_write_to_stdout (Environment.NewLine);
					} catch (Exception){}
				}
				sb.Length = 0;
			}
			
			// minimum to override - see http://msdn.microsoft.com/en-us/library/system.io.textwriter.aspx
			public override void Write (char value)
			{
				try {
					sb.Append (value);
				}
				catch (Exception) {
				}
			}
			
			// optimization (to avoid concatening chars)
			public override void Write (string value)
			{
				try {
					sb.Append (value);
					if (value != null && value.Length >= CoreNewLine.Length && EndsWithNewLine (value))
						Flush ();
				}
				catch (Exception) {
				}
			}
			
			bool EndsWithNewLine (string value)
			{
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
