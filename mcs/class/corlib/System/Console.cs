//
// System.Console.cs
//
// Author:
// 	Dietmar Maurer (dietmar@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2004,2005 Novell, Inc. (http://www.novell.com)
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

using System.IO;
using System.Security.Permissions;
using System.Text;

namespace System
{
	public
#if NET_2_0
	static
#else
	sealed
#endif
	class Console
	{
		private static TextWriter stdout;
		private static TextWriter stderr;
		private static TextReader stdin;

		static Console ()
		{
			int code_page = 0;
			Encoding.InternalCodePage (ref code_page);
			Encoding encoding;

			if (((int) Environment.Platform) == 128){
				//
				// On Unix systems (128), do not output the
				// UTF-8 ZWNBSP (zero-width non-breaking space).
				//
				if (code_page == UTF8Encoding.UTF8_CODE_PAGE || ((code_page & 0x10000000) != 0))
					encoding = Encoding.UTF8Unmarked;
				else
					encoding = Encoding.Default;
			} else {
				//
				// On Windows, follow the Windows tradition
				//
				encoding = Encoding.Default;
			}

			stderr = new UnexceptionalStreamWriter (OpenStandardError (0), encoding); 
			((StreamWriter)stderr).AutoFlush = true;
			stderr = TextWriter.Synchronized (stderr, true);

			stdout = new UnexceptionalStreamWriter (OpenStandardOutput (0), encoding);
			((StreamWriter)stdout).AutoFlush = true;
			stdout = TextWriter.Synchronized (stdout, true);

			stdin  = new UnexceptionalStreamReader (OpenStandardInput (0), encoding);
			stdin = TextReader.Synchronized (stdin);
			GC.SuppressFinalize (stdout);
			GC.SuppressFinalize (stderr);
			GC.SuppressFinalize (stdin);
		}

#if !NET_2_0
		private Console ()
		{
		}
#endif

		public static TextWriter Error {
			get {
				return stderr;
			}
		}

		public static TextWriter Out {
			get {
				return stdout;
			}
		}

		public static TextReader In {
			get {
				return stdin;
			}
		}

		public static Stream OpenStandardError ()
		{
			return OpenStandardError (0);
		}

		// calling any FileStream constructor with an handle normally
		// requires permissions UnmanagedCode permissions. In this 
		// case we assert this permission so the console can be used
		// in partial trust (i.e. without having UnmanagedCode).
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public static Stream OpenStandardError (int bufferSize)
		{
			try {
				return new FileStream (MonoIO.ConsoleError, FileAccess.Write, false, bufferSize, false, bufferSize == 0);
			} catch (IOException) {
				return new NullStream ();
			}
		}

		public static Stream OpenStandardInput ()
		{
			return OpenStandardInput (0);
		}

		// calling any FileStream constructor with an handle normally
		// requires permissions UnmanagedCode permissions. In this 
		// case we assert this permission so the console can be used
		// in partial trust (i.e. without having UnmanagedCode).
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public static Stream OpenStandardInput (int bufferSize)
		{
			try {
				return new FileStream (MonoIO.ConsoleInput, FileAccess.Read, false, bufferSize, false, bufferSize == 0);
			} catch (IOException) {
				return new NullStream ();
			}
		}

		public static Stream OpenStandardOutput ()
		{
			return OpenStandardOutput (0);
		}

		// calling any FileStream constructor with an handle normally
		// requires permissions UnmanagedCode permissions. In this 
		// case we assert this permission so the console can be used
		// in partial trust (i.e. without having UnmanagedCode).
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public static Stream OpenStandardOutput (int bufferSize)
		{
			try {
				return new FileStream (MonoIO.ConsoleOutput, FileAccess.Write, false, bufferSize, false, bufferSize == 0);
			} catch (IOException) {
				return new NullStream ();
			}
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public static void SetError (TextWriter newError)
		{
			if (newError == null)
				throw new ArgumentNullException ("newError");

			stderr = newError;
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public static void SetIn (TextReader newIn)
		{
			if (newIn == null)
				throw new ArgumentNullException ("newIn");

			stdin = newIn;
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public static void SetOut (TextWriter newOut)
		{
			if (newOut == null)
				throw new ArgumentNullException ("newOut");

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

		[CLSCompliant (false)]
		public static void Write (uint value)
		{
			stdout.Write (value);
		}

		[CLSCompliant (false)]
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

#if ! BOOTSTRAP_WITH_OLDLIB
		[CLSCompliant (false)]
		public static void Write (string format, object arg0, object arg1, object arg2, object arg3, __arglist)
		{
			ArgIterator iter = new ArgIterator (__arglist);
			int argCount = iter.GetRemainingCount();

			object[] args = new object [argCount + 4];
			args [0] = arg0;
			args [1] = arg1;
			args [2] = arg2;
			args [3] = arg3;
			for (int i = 0; i < argCount; i++) {
				TypedReference typedRef = iter.GetNextArg ();
				args [i + 4] = TypedReference.ToObject (typedRef);
			}

			stdout.Write (String.Format (format, args));
		}
#endif

		public static void WriteLine ()
		{
			stdout.WriteLine ();
		}

		public static void WriteLine (bool value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (char value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (char[] value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (decimal value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (double value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (int value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (long value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (object value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (float value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (string value)
		{
			stdout.WriteLine (value);
		}

		[CLSCompliant (false)]
		public static void WriteLine (uint value)
		{
			stdout.WriteLine (value);
		}

		[CLSCompliant (false)]
		public static void WriteLine (ulong value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (string format, object arg0)
		{
			stdout.WriteLine (format, arg0);
		}

		public static void WriteLine (string format, params object[] arg)
		{
			stdout.WriteLine (format, arg);
		}

		public static void WriteLine (char[] buffer, int index, int count)
		{
			stdout.WriteLine (buffer, index, count);
		}

		public static void WriteLine (string format, object arg0, object arg1)
		{
			stdout.WriteLine (format, arg0, arg1);
		}

		public static void WriteLine (string format, object arg0, object arg1, object arg2)
		{
			stdout.WriteLine (format, arg0, arg1, arg2);
		}

#if ! BOOTSTRAP_WITH_OLDLIB
		[CLSCompliant (false)]
		public static void WriteLine (string format, object arg0, object arg1, object arg2, object arg3, __arglist)
		{
			ArgIterator iter = new ArgIterator (__arglist);
			int argCount = iter.GetRemainingCount();

			object[] args = new object [argCount + 4];
			args [0] = arg0;
			args [1] = arg1;
			args [2] = arg2;
			args [3] = arg3;
			for (int i = 0; i < argCount; i++) {
				TypedReference typedRef = iter.GetNextArg ();
				args [i + 4] = TypedReference.ToObject (typedRef);
			}

			stdout.WriteLine (String.Format (format, args));
		}
#endif

		public static int Read ()
		{
			return stdin.Read ();
		}

#if NET_2_0
		public static string ReadLine ()
		{
			bool prevEcho = false;
			if (ConsoleDriver.Initialized) {
				prevEcho = ConsoleDriver.Echo;
				ConsoleDriver.Echo = true;
			}

			string ret = stdin.ReadLine ();
			if (ConsoleDriver.Initialized)
				ConsoleDriver.Echo = prevEcho;

			return ret;
		}
#else
		public static string ReadLine ()
		{
			return stdin.ReadLine ();
		}

#endif

#if NET_2_0
		// On windows, for en-US the Default is Windows-1252, while input/output is IBM437
		// We might want to initialize these fields in the ConsoleDriver instead.
		static Encoding inputEncoding = Encoding.Default;
		static Encoding outputEncoding = Encoding.Default;

		public static Encoding InputEncoding {
			get { return inputEncoding; }
			set { inputEncoding = value; }
		}

		public static Encoding OutputEncoding {
			get { return outputEncoding; }
			set { outputEncoding = value; }
		}

		public static ConsoleColor BackgroundColor {
			get { return ConsoleDriver.BackgroundColor; }
			set { ConsoleDriver.BackgroundColor = value; }
		}

		public static int BufferHeight {
			get { return ConsoleDriver.BufferHeight; }
			set { ConsoleDriver.BufferHeight = value; }
		}

		public static int BufferWidth {
			get { return ConsoleDriver.BufferWidth; }
			set { ConsoleDriver.BufferWidth = value; }
		}

		public static int CursorLeft {
			get { return ConsoleDriver.CursorLeft; }
			set { ConsoleDriver.CursorLeft = value; }
		}

		public static int CursorTop {
			get { return ConsoleDriver.CursorTop; }
			set { ConsoleDriver.CursorTop = value; }
		}

		public static bool CursorVisible {
			get { return ConsoleDriver.CursorVisible; }
			set { ConsoleDriver.CursorVisible = value; }
		}

		public static ConsoleColor ForegroundColor {
			get { return ConsoleDriver.ForegroundColor; }
			set { ConsoleDriver.ForegroundColor = value; }
		}

		public static bool KeyAvailable {
			get { return ConsoleDriver.KeyAvailable; }
		}

		public static string Title {
			get { return ConsoleDriver.Title; }
			set { ConsoleDriver.Title = value; }
		}

		public static bool TreatControlCAsInput {
			get { return ConsoleDriver.TreatControlCAsInput; }
			set { ConsoleDriver.TreatControlCAsInput = value; }
		}

		public static int WindowHeight {
			get { return ConsoleDriver.WindowHeight; }
			set { ConsoleDriver.WindowHeight = value; }
		}

		public static int WindowLeft {
			get { return ConsoleDriver.WindowLeft; }
			set { ConsoleDriver.WindowLeft = value; }
		}

		public static int WindowTop {
			get { return ConsoleDriver.WindowTop; }
			set { ConsoleDriver.WindowTop = value; }
		}

		public static int WindowWidth {
			get { return ConsoleDriver.WindowWidth; }
			set { ConsoleDriver.WindowWidth = value; }
		}

		public static void Beep ()
		{
			Beep (1000, 500);
		}

		public static void Beep (int frequency, int duration)
		{
			if (frequency < 37 || frequency > 32767)
				throw new ArgumentOutOfRangeException ("frequency");

			if (duration <= 0)
				throw new ArgumentOutOfRangeException ("duration");

			ConsoleDriver.Beep (frequency, duration);
		}

		public static void Clear ()
		{
			ConsoleDriver.Clear ();
		}

		[MonoTODO]
		public static void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
						int targetLeft, int targetTop)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
						int targetLeft, int targetTop, Char sourceChar,
						ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
		{
			throw new NotImplementedException ();
		}

		public static ConsoleKeyInfo ReadKey ()
		{
			return ReadKey (false);
		}

		public static ConsoleKeyInfo ReadKey (bool intercept)
		{
			return ConsoleDriver.ReadKey (intercept);
		}

		public static void ResetColor ()
		{
			ConsoleDriver.ResetColor ();
		}

		public static void SetBufferSize (int width, int height)
		{
			ConsoleDriver.SetBufferSize (width, height);
		}

		public static void SetCursorPosition (int left, int top)
		{
			ConsoleDriver.SetCursorPosition (left, top);
		}

		public static void SetWindowPosition (int left, int top)
		{
			ConsoleDriver.SetWindowPosition (left, top);
		}

		public static void SetWindowSize (int width, int height)
		{
			ConsoleDriver.SetWindowSize (width, height);
		}

		[MonoTODO ("Implement add/remove hooks")]
		public static event ConsoleCancelEventHandler CancelKeyPress;
#endif
	}
}

