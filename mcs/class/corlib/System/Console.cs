//
// System.Console.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.IO;
using System.Text;

namespace System {

	public sealed class Console {

		private static TextWriter stdout;
		private static TextWriter stderr;
		private static TextReader stdin;

		static Console ()
		{
			stderr = new StreamWriter (OpenStandardError (), Encoding.Default);
			((StreamWriter)stderr).AutoFlush = true;
			stderr = TextWriter.Synchronized (stderr);
			
			stdout = new StreamWriter (OpenStandardOutput (), Encoding.Default);
			((StreamWriter)stdout).AutoFlush = true;
			stdout = TextWriter.Synchronized (stdout);
			
			stdin  = new StreamReader (OpenStandardInput (), Encoding.Default);
			stdin = TextReader.Synchronized (stdin);
		}

		private Console () {}
		
		public static TextWriter Error
		{
			get {
				return stderr;
			}
		}

		public static TextWriter Out
		{
			get {
				return stdout;
			}
		}

		public static TextReader In
		{
			get {
				return stdin;
			}
		}

		public static Stream OpenStandardError ()
		{
			return OpenStandardError (0);
		}
		
		public static Stream OpenStandardError (int bufferSize)
		{
			return new FileStream (MonoIO.ConsoleError,
					       FileAccess.Write,
					       false,  bufferSize, false, bufferSize == 0);
		}

		public static Stream OpenStandardInput ()
		{
			return OpenStandardInput (0);
		}
		
		public static Stream OpenStandardInput (int bufferSize)
		{
			return new FileStream (MonoIO.ConsoleInput,
					       FileAccess.Read,
					       false,  bufferSize, false, bufferSize == 0);
		}

		public static Stream OpenStandardOutput ()
		{
			return OpenStandardOutput (0);
		}
		
		public static Stream OpenStandardOutput (int bufferSize)
		{
			return new FileStream (MonoIO.ConsoleOutput,
					       FileAccess.Write,
					       false,  bufferSize, false, bufferSize == 0);
		}

		public static void SetError (TextWriter newError)
		{
			if (newError == null)
				throw new ArgumentNullException ();

			stderr = newError;
		}

		public static void SetIn (TextReader newIn)
		{
			if (newIn == null)
				throw new ArgumentNullException ();

			stdin = newIn;
		}

		public static void SetOut (TextWriter newOut)
		{
			if (newOut == null)
				throw new ArgumentNullException ();

			stdout = newOut;
		}

                public static void Write (bool value)
		{
			stdout.Write (value);
		}

                public static void Write (char value)
		{
			stdout.Write (value);
		}

                public static void Write (char[] value)
		{
			stdout.Write (value);
		}
		
                public static void Write (decimal value)
		{
			stdout.Write (value);
		}
		
                public static void Write (double value)
		{
			stdout.Write (value);
		}

                public static void Write (int value)
		{
			stdout.Write (value);
		}
		
                public static void Write (long value)
		{
			stdout.Write (value);
		}
		
                public static void Write (object value)
		{
			stdout.Write (value);
		}
		
                public static void Write (float value)
		{
			stdout.Write (value);
		}
		
                public static void Write (string value)
		{
			stdout.Write (value);
		}
		
		[CLSCompliant(false)]
                public static void Write (uint value)
		{
			stdout.Write (value);
		}
		
		[CLSCompliant(false)]
                public static void Write (ulong value)
		{
			stdout.Write (value);
		}
		
                public static void Write (string format, object arg0)
		{
			stdout.Write (format, arg0);
		}
		
                public static void Write (string format, params object[] arg)
		{
			stdout.Write (format, arg);
		}
		
                public static void Write (char[] buffer, int index, int count)
		{
			stdout.Write (buffer, index, count);
		}
		
                public static void Write (string format, object arg0, object arg1)
		{
			stdout.Write (format, arg0, arg1);
		}
		
                public static void Write (string format, object arg0, object arg1, object arg2 )
		{
			stdout.Write (format, arg0, arg1, arg2);
		}
                
                public static void WriteLine ()
		{
			stdout.WriteLine ();
		}
		
                public static void WriteLine (bool value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
                public static void WriteLine (char value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
                public static void WriteLine (char[] value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
                public static void WriteLine (decimal value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
                public static void WriteLine (double value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
                public static void WriteLine (int value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
                public static void WriteLine (long value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
                public static void WriteLine (object value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
                public static void WriteLine (float value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
                public static void WriteLine (string value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
		[CLSCompliant(false)]
                public static void WriteLine (uint value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
		[CLSCompliant(false)]
                public static void WriteLine (ulong value)
		{
			stdout.Write (value);
			stdout.WriteLine();
		}
		
                public static void WriteLine (string format, object arg0)
		{
			stdout.Write (format, arg0);
			stdout.WriteLine();
		}
		
                public static void WriteLine (string format, params object[] arg)
		{
			stdout.Write (format, arg);
			stdout.WriteLine();
		}
		
                public static void WriteLine (char[] buffer, int index, int count)
		{
			stdout.Write (buffer, index, count);
			stdout.WriteLine();
		}
		
                public static void WriteLine (string format, object arg0, object arg1)
		{
			stdout.Write (format, arg0, arg1);
			stdout.WriteLine();
		}
		
                public static void WriteLine (string format, object arg0, object arg1, object arg2)
		{
			stdout.Write (format, arg0, arg1, arg2);
			stdout.WriteLine();
		}

		public static int Read ()
		{
			return stdin.Read ();
		}
		
		public static string ReadLine ()
		{
			return stdin.ReadLine ();
		}
		
	}
}
