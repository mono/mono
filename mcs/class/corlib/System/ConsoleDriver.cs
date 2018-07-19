//
// System.ConsoleDriver
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
#if MONO_FEATURE_CONSOLE
using System.IO;
using System.Runtime.CompilerServices;

namespace System {
	static class ConsoleDriver {
		internal static IConsoleDriver driver;
		static bool is_console;
		static bool called_isatty;

		static ConsoleDriver ()
		{
			// Put the actual new statements into separate methods to avoid initalizing
			// three classes when only one is needed.
			if (!IsConsole) {
				driver = CreateNullConsoleDriver ();
			} else if (Environment.IsRunningOnWindows) {
				driver = CreateWindowsConsoleDriver ();
			} else {
				string term = Environment.GetEnvironmentVariable ("TERM");

				// Perhaps we should let the Terminfo driver return a
				// success/failure flag based on the terminal properties
				if (term == "dumb"){
					is_console = false;
					driver = CreateNullConsoleDriver ();
				} else
					driver = CreateTermInfoDriver (term);
			}
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static IConsoleDriver CreateNullConsoleDriver () {
			return new NullConsoleDriver ();
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static IConsoleDriver CreateWindowsConsoleDriver () {
			return new WindowsConsoleDriver ();
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static IConsoleDriver CreateTermInfoDriver (string term) {
			return new TermInfoDriver (term);
		}
		
		public static bool Initialized {
			get { return driver.Initialized; }
		}

		public static ConsoleColor BackgroundColor {
			get { return driver.BackgroundColor; }
			set {
				if (value < ConsoleColor.Black || value > ConsoleColor.White)
					throw new ArgumentOutOfRangeException ("value", "Not a ConsoleColor value.");

				driver.BackgroundColor = value;
			}
		}

		public static int BufferHeight {
			get { return driver.BufferHeight; }
			set { driver.BufferHeight = value; }
		}

		public static int BufferWidth {
			get { return driver.BufferWidth; }
			set { driver.BufferWidth = value; }
		}

		public static bool CapsLock {
			get { return driver.CapsLock; }
		}

		public static int CursorLeft {
			get { return driver.CursorLeft; }
			set { driver.CursorLeft = value; }
		}

		public static int CursorSize {
			get { return driver.CursorSize; }
			set { driver.CursorSize = value; }
		}

		public static int CursorTop {
			get { return driver.CursorTop; }
			set { driver.CursorTop = value; }
		} 
		
		public static bool CursorVisible {
			get { return driver.CursorVisible; }
			set { driver.CursorVisible = value; }
		}

		public static bool KeyAvailable {
			get { return driver.KeyAvailable; }
		}

		public static ConsoleColor ForegroundColor {
			get { return driver.ForegroundColor; }
			set {
				if (value < ConsoleColor.Black || value > ConsoleColor.White)
					throw new ArgumentOutOfRangeException ("value", "Not a ConsoleColor value.");

				driver.ForegroundColor = value;
			}
		}

		public static int LargestWindowHeight {
			get { return driver.LargestWindowHeight; }
		}

		public static int LargestWindowWidth {
			get { return driver.LargestWindowWidth; }
		}

		public static bool NumberLock {
			get { return driver.NumberLock; }
		}

		public static string Title {
			get { return driver.Title; }
			set { driver.Title = value; }
		}

		public static bool TreatControlCAsInput {
			get { return driver.TreatControlCAsInput; }
			set { driver.TreatControlCAsInput = value; }
		} 

		public static int WindowHeight {
			get { return driver.WindowHeight; }
			set { driver.WindowHeight = value; }
		}

		public static int WindowLeft {
			get { return driver.WindowLeft; }
			set { driver.WindowLeft = value; }
		}

		public static int WindowTop {
			get { return driver.WindowTop; }
			set { driver.WindowTop = value; }
		}

		public static int WindowWidth {
			get { return driver.WindowWidth; }
			set { driver.WindowWidth = value; }
		}

		public static bool IsErrorRedirected {
			get {
				return !Isatty (MonoIO.ConsoleError);
			}
		}

		public static bool IsOutputRedirected {
			get {
				return !Isatty (MonoIO.ConsoleOutput);
			}
		}

		public static bool IsInputRedirected {
			get {
				return !Isatty (MonoIO.ConsoleInput);
			}
		}

		public static void Beep (int frequency, int duration)
		{
			driver.Beep (frequency, duration);
		}
		
		public static void Clear ()
		{
			driver.Clear ();
		}

		public static void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
					int targetLeft, int targetTop)
		{
			MoveBufferArea (sourceLeft, sourceTop, sourceWidth, sourceHeight,
					targetLeft, targetTop, ' ', 0, 0);
		}

		public static void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
					int targetLeft, int targetTop, char sourceChar,
					ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
		{
			driver.MoveBufferArea (sourceLeft, sourceTop, sourceWidth, sourceHeight,
					targetLeft, targetTop, sourceChar, sourceForeColor, sourceBackColor);
		}

		public static void Init ()
		{
			driver.Init ();
		}

		public static int Read ()
		{
			return ReadKey (false).KeyChar;
		}

		public static string ReadLine ()
		{
			return driver.ReadLine ();
		}

		public static ConsoleKeyInfo ReadKey (bool intercept)
		{
			return driver.ReadKey (intercept);
		}

		public static void ResetColor ()
		{
			driver.ResetColor ();
		}

		public static void SetBufferSize (int width, int height)
		{
			driver.SetBufferSize (width, height);
		}

		public static void SetCursorPosition (int left, int top)
		{
			driver.SetCursorPosition (left, top);
		}

		public static void SetWindowPosition (int left, int top)
		{
			driver.SetWindowPosition (left, top);
		}

		public static void SetWindowSize (int width, int height)
		{
			driver.SetWindowSize (width, height);
		}

		public static bool IsConsole {
			get {
				if (called_isatty)
					return is_console;

				is_console = (Isatty (MonoIO.ConsoleOutput) && Isatty (MonoIO.ConsoleInput));
				called_isatty = true;
				return is_console;
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool Isatty (IntPtr handle);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern int InternalKeyAvailable (int ms_timeout);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe internal static extern bool TtySetup (string keypadXmit, string teardown, out byte [] control_characters, out int *address);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool SetEcho (bool wantEcho);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool SetBreak (bool wantBreak);
	}
}
#endif

