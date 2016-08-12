//
// System.Console.cs
//
// Author:
// 	Dietmar Maurer (dietmar@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2004,2005 Novell, Inc. (http://www.novell.com)
// Copyright 2013 Xamarin Inc. (http://www.xamarin.com)

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

using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace System
{
	public static partial class Console
	{
#if !NET_2_1
		private class WindowsConsole
		{
			public static bool ctrlHandlerAdded = false;
			private delegate bool WindowsCancelHandler (int keyCode);
			private static WindowsCancelHandler cancelHandler = new WindowsCancelHandler (DoWindowsConsoleCancelEvent);

			[DllImport ("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
			private static extern int GetConsoleCP ();
			[DllImport ("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
			private static extern int GetConsoleOutputCP ();

			[DllImport ("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
			private static extern bool SetConsoleCtrlHandler (WindowsCancelHandler handler, bool addHandler);

			// Only call the event handler if Control-C was pressed (code == 0), nothing else
			private static bool DoWindowsConsoleCancelEvent (int keyCode)
			{
				if (keyCode == 0)
					DoConsoleCancelEvent ();
				return keyCode == 0;
			}

			[MethodImpl (MethodImplOptions.NoInlining)]
			public static int GetInputCodePage ()
			{
				return GetConsoleCP ();
			}

			[MethodImpl (MethodImplOptions.NoInlining)]
			public static int GetOutputCodePage ()
			{
				return GetConsoleOutputCP ();
			}

			public static void AddCtrlHandler ()
			{
				SetConsoleCtrlHandler (cancelHandler, true);
				ctrlHandlerAdded = true;
			}
			
			public static void RemoveCtrlHandler ()
			{
				SetConsoleCtrlHandler (cancelHandler, false);
				ctrlHandlerAdded = false;
			}
		}
#endif

		internal static TextWriter stdout;
		private static TextWriter stderr;
		private static TextReader stdin;

		static Console ()
		{
			if (Environment.IsRunningOnWindows) {
				//
				// On Windows, follow the Windows tradition
				//
#if NET_2_1
				// should never happen since Moonlight does not run on windows
				inputEncoding = outputEncoding = Encoding.Default;
#else			
				try {
					inputEncoding = Encoding.GetEncoding (WindowsConsole.GetInputCodePage ());
					outputEncoding = Encoding.GetEncoding (WindowsConsole.GetOutputCodePage ());
					// ArgumentException and NotSupportedException can be thrown as well
				} catch {
					// FIXME: I18N assemblies are not available when compiling mcs
					// Use Latin 1 as it is fast and UTF-8 is never used as console code page
					inputEncoding = outputEncoding = Encoding.Default;
				}
#endif
			} else {
				//
				// On Unix systems (128), do not output the
				// UTF-8 ZWNBSP (zero-width non-breaking space).
				//
				int code_page = 0;
				EncodingHelper.InternalCodePage (ref code_page);

				if (code_page != -1 && ((code_page & 0x0fffffff) == 3 // UTF8Encoding.UTF8_CODE_PAGE
					|| ((code_page & 0x10000000) != 0)))
					inputEncoding = outputEncoding = EncodingHelper.UTF8Unmarked;
				else
					inputEncoding = outputEncoding = Encoding.Default;
			}

			SetupStreams (inputEncoding, outputEncoding);
		}

		static void SetupStreams (Encoding inputEncoding, Encoding outputEncoding)
		{
#if !NET_2_1
			if (!Environment.IsRunningOnWindows && ConsoleDriver.IsConsole) {
				stdin = new CStreamReader (OpenStandardInput (0), inputEncoding);
				stdout = TextWriter.Synchronized (new CStreamWriter (OpenStandardOutput (0), outputEncoding, true) { AutoFlush = true });
				stderr = TextWriter.Synchronized (new CStreamWriter (OpenStandardError (0), outputEncoding, true) { AutoFlush = true });
			} else
#endif
			{
				stdin = TextReader.Synchronized (new UnexceptionalStreamReader (OpenStandardInput (0), inputEncoding));

#if MONOTOUCH
				stdout = new NSLogWriter ();
				stderr = new NSLogWriter ();
#else
				stdout = TextWriter.Synchronized (new UnexceptionalStreamWriter (OpenStandardOutput (0), outputEncoding) { AutoFlush = true });
				stderr = TextWriter.Synchronized (new UnexceptionalStreamWriter (OpenStandardError (0), outputEncoding) { AutoFlush = true });

#if MONODROID
				if (LogcatTextWriter.IsRunningOnAndroid ()) {
					stdout = TextWriter.Synchronized (new LogcatTextWriter ("mono-stdout", stdout));
					stderr = TextWriter.Synchronized (new LogcatTextWriter ("mono-stderr", stderr));
				}
#endif // MONODROID
#endif // MONOTOUCH
			}

			GC.SuppressFinalize (stdout);
			GC.SuppressFinalize (stderr);
			GC.SuppressFinalize (stdin);
		}

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

		private static Stream Open (IntPtr handle, FileAccess access, int bufferSize)
		{
			try {
				// TODO: Should use __ConsoleStream from reference sources
				return new FileStream (handle, access, false, bufferSize, false, true);
			} catch (IOException) {
				return Stream.Null;
			}
		}

		public static Stream OpenStandardError ()
		{
			return OpenStandardError (0);
		}

		// calling any FileStream constructor with a handle normally
		// requires permissions UnmanagedCode permissions. In this 
		// case we assert this permission so the console can be used
		// in partial trust (i.e. without having UnmanagedCode).
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public static Stream OpenStandardError (int bufferSize)
		{
			return Open (MonoIO.ConsoleError, FileAccess.Write, bufferSize);
		}

		public static Stream OpenStandardInput ()
		{
			return OpenStandardInput (0);
		}

		// calling any FileStream constructor with a handle normally
		// requires permissions UnmanagedCode permissions. In this 
		// case we assert this permission so the console can be used
		// in partial trust (i.e. without having UnmanagedCode).
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public static Stream OpenStandardInput (int bufferSize)
		{
			return Open (MonoIO.ConsoleInput, FileAccess.Read, bufferSize);
		}

		public static Stream OpenStandardOutput ()
		{
			return OpenStandardOutput (0);
		}

		// calling any FileStream constructor with a handle normally
		// requires permissions UnmanagedCode permissions. In this 
		// case we assert this permission so the console can be used
		// in partial trust (i.e. without having UnmanagedCode).
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public static Stream OpenStandardOutput (int bufferSize)
		{
			return Open (MonoIO.ConsoleOutput, FileAccess.Write, bufferSize);
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

		public static void Write (char[] buffer)
		{
			stdout.Write (buffer);
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
			if (arg == null)
				stdout.Write (format);
			else
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

		public static void WriteLine (char[] buffer)
		{
			stdout.WriteLine (buffer);
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
			if (arg == null)
				stdout.WriteLine (format);
			else
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

#if !NET_2_1
		public static int Read ()
		{
			if ((stdin is CStreamReader) && ConsoleDriver.IsConsole) {
				return ConsoleDriver.Read ();
			} else {
				return stdin.Read ();
			}
		}

		public static string ReadLine ()
		{
			if ((stdin is CStreamReader) && ConsoleDriver.IsConsole) {
				return ConsoleDriver.ReadLine ();
			} else {
				return stdin.ReadLine ();
			}
		}
#else
		public static int Read ()
		{
			return stdin.Read ();
		}

		public static string ReadLine ()
		{
			return stdin.ReadLine ();
		}

#endif

		// FIXME: Console should use these encodings when changed
		static Encoding inputEncoding;
		static Encoding outputEncoding;

		public static Encoding InputEncoding {
			get { return inputEncoding; }
			set {
				inputEncoding = value;
				SetupStreams (inputEncoding, outputEncoding);
			}
		}

		public static Encoding OutputEncoding {
			get { return outputEncoding; }
			set {
				outputEncoding = value;
				SetupStreams (inputEncoding, outputEncoding);
			}
		}

#if !NET_2_1
		public static ConsoleColor BackgroundColor {
			get { return ConsoleDriver.BackgroundColor; }
			set { ConsoleDriver.BackgroundColor = value; }
		}

		public static int BufferHeight {
			get { return ConsoleDriver.BufferHeight; }
			[MonoLimitation ("Implemented only on Windows")]
			set { ConsoleDriver.BufferHeight = value; }
		}

		public static int BufferWidth {
			get { return ConsoleDriver.BufferWidth; }
			[MonoLimitation ("Implemented only on Windows")]
			set { ConsoleDriver.BufferWidth = value; }
		}

		[MonoLimitation ("Implemented only on Windows")]
		public static bool CapsLock {
			get { return ConsoleDriver.CapsLock; }
		}

		public static int CursorLeft {
			get { return ConsoleDriver.CursorLeft; }
			set { ConsoleDriver.CursorLeft = value; }
		}

		public static int CursorTop {
			get { return ConsoleDriver.CursorTop; }
			set { ConsoleDriver.CursorTop = value; }
		}

		public static int CursorSize {
			get { return ConsoleDriver.CursorSize; }
			set { ConsoleDriver.CursorSize = value; }
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

		public static int LargestWindowHeight {
			get { return ConsoleDriver.LargestWindowHeight; }
		}

		public static int LargestWindowWidth {
			get { return ConsoleDriver.LargestWindowWidth; }
		}

		[MonoLimitation ("Only works on windows")]
		public static bool NumberLock {
			get { return ConsoleDriver.NumberLock; }
		}

		public static string Title {
			get { return ConsoleDriver.Title; }
			set { ConsoleDriver.Title = value; }
		}

		public static bool TreatControlCAsInput {
			get { return ConsoleDriver.TreatControlCAsInput; }
			set { ConsoleDriver.TreatControlCAsInput = value; }
		}

		[MonoLimitation ("Only works on windows")]
		public static int WindowHeight {
			get { return ConsoleDriver.WindowHeight; }
			set { ConsoleDriver.WindowHeight = value; }
		}

		[MonoLimitation ("Only works on windows")]
		public static int WindowLeft {
			get { return ConsoleDriver.WindowLeft; }
			set { ConsoleDriver.WindowLeft = value; }
		}

		[MonoLimitation ("Only works on windows")]
		public static int WindowTop {
			get { return ConsoleDriver.WindowTop; }
			set { ConsoleDriver.WindowTop = value; }
		}

		[MonoLimitation ("Only works on windows")]
		public static int WindowWidth {
			get { return ConsoleDriver.WindowWidth; }
			set { ConsoleDriver.WindowWidth = value; }
		}

		public static bool IsErrorRedirected {
			get {
				return ConsoleDriver.IsErrorRedirected;
			}
		}

		public static bool IsOutputRedirected {
			get {
				return ConsoleDriver.IsOutputRedirected;
			}
		}

		public static bool IsInputRedirected {
			get {
				return ConsoleDriver.IsInputRedirected;
			}
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

		[MonoLimitation ("Implemented only on Windows")]
		public static void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
						int targetLeft, int targetTop)
		{
			ConsoleDriver.MoveBufferArea (sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);
		}

		[MonoLimitation ("Implemented only on Windows")]
		public static void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
						int targetLeft, int targetTop, Char sourceChar,
						ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
		{
			ConsoleDriver.MoveBufferArea (sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop,
							sourceChar, sourceForeColor, sourceBackColor);
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

		[MonoLimitation ("Only works on windows")]
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

		static ConsoleCancelEventHandler cancel_event;
		public static event ConsoleCancelEventHandler CancelKeyPress {
			add {
				if (ConsoleDriver.Initialized == false)
					ConsoleDriver.Init ();

				cancel_event += value;

				if (Environment.IsRunningOnWindows && !WindowsConsole.ctrlHandlerAdded)
					WindowsConsole.AddCtrlHandler();
			}
			remove {
				if (ConsoleDriver.Initialized == false)
					ConsoleDriver.Init ();

				cancel_event -= value;

				if (cancel_event == null && Environment.IsRunningOnWindows)
				{
					// Need to remove our hook if there's nothing left in the event
					if (WindowsConsole.ctrlHandlerAdded)
						WindowsConsole.RemoveCtrlHandler();
				}
			}
		}

		delegate void InternalCancelHandler ();
		
#pragma warning disable 414
		//
		// Used by console-io.c
		//
		static readonly InternalCancelHandler cancel_handler = new InternalCancelHandler (DoConsoleCancelEvent);
#pragma warning restore 414		

		internal static void DoConsoleCancelEvent ()
		{
			bool exit = true;
			if (cancel_event != null) {
				ConsoleCancelEventArgs args = new ConsoleCancelEventArgs (ConsoleSpecialKey.ControlC);
				Delegate [] delegates = cancel_event.GetInvocationList ();
				foreach (ConsoleCancelEventHandler d in delegates){
					try {
						// Sender is always null here.
						d (null, args);
					} catch {} // Ignore any exception.
				}
				exit = !args.Cancel;
			}

			if (exit)
				Environment.Exit (58);
		}
#else
		// largely inspired by https://github.com/dotnet/corefx/blob/be8d2ce3964968cec9322a64211e37682085db70/src/System.Console/src/System/ConsolePal.WinRT.cs, because it's a similar platform where a console might not be available

		// provide simply color tracking that allows round-tripping
		internal const ConsoleColor UnknownColor = (ConsoleColor)(-1);
		private static ConsoleColor s_trackedForegroundColor = UnknownColor;
		private static ConsoleColor s_trackedBackgroundColor = UnknownColor;

		public static ConsoleColor ForegroundColor
		{
			get
			{
				return s_trackedForegroundColor;
			}
			set
			{
				lock (Console.Out) // synchronize with other writers
				{
					s_trackedForegroundColor = value;
				}
			}
		}

		public static ConsoleColor BackgroundColor
		{
			get
			{
				return s_trackedBackgroundColor;
			}
			set
			{
				lock (Console.Out) // synchronize with other writers
				{
					s_trackedBackgroundColor = value;
				}
			}
		}

		public static int BufferWidth
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static int BufferHeight
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static bool CapsLock { get { throw new PlatformNotSupportedException (); } }

		public static int CursorLeft
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static int CursorTop
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static int CursorSize
		{
			get { return 100; }
			set { throw new PlatformNotSupportedException(); }
		}

		public static bool CursorVisible
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static bool KeyAvailable { get { throw new PlatformNotSupportedException (); } }

		public static int LargestWindowWidth
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static int LargestWindowHeight
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static bool NumberLock { get { throw new PlatformNotSupportedException (); } }

		public static string Title
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static bool TreatControlCAsInput
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static int WindowHeight
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static int WindowLeft
		{
			get { return 0; }
			set { throw new PlatformNotSupportedException (); }
		}

		public static int WindowTop
		{
			get { return 0; }
			set { throw new PlatformNotSupportedException (); }
		}

		public static int WindowWidth
		{
			get { throw new PlatformNotSupportedException (); }
			set { throw new PlatformNotSupportedException (); }
		}

		public static bool IsErrorRedirected { get { throw new PlatformNotSupportedException (); } }

		public static bool IsInputRedirected { get { throw new PlatformNotSupportedException (); } }

		public static bool IsOutputRedirected { get { throw new PlatformNotSupportedException (); } }

		public static void Beep () { throw new PlatformNotSupportedException (); }

		public static void Beep (int frequency, int duration) { throw new PlatformNotSupportedException (); }

		public static void Clear () { throw new PlatformNotSupportedException (); }

		public static void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop) { throw new PlatformNotSupportedException(); }

		public static void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor) { throw new PlatformNotSupportedException(); }


		public static ConsoleKeyInfo ReadKey ()
		{
			return ReadKey (false);
		}

		public static ConsoleKeyInfo ReadKey (bool intercept) { throw new PlatformNotSupportedException (); }

		public static void ResetColor ()
		{
			lock (Console.Out) // synchronize with other writers
			{
				s_trackedForegroundColor = UnknownColor;
				s_trackedBackgroundColor = UnknownColor;
			}
		}

		public static void SetBufferSize (int width, int height) { throw new PlatformNotSupportedException (); }

		public static void SetCursorPosition (int left, int top) { throw new PlatformNotSupportedException (); }

		public static void SetWindowPosition (int left, int top) { throw new PlatformNotSupportedException (); }

		public static void SetWindowSize (int width, int height) { throw new PlatformNotSupportedException (); }

		public static event ConsoleCancelEventHandler CancelKeyPress {
			add {
				throw new PlatformNotSupportedException ();
			}
			remove {
				throw new PlatformNotSupportedException ();
			}
		}
#endif
	}
}

