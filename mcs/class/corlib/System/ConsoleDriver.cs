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
#if NET_2_0
using System.IO;
using System.Runtime.CompilerServices;

namespace System {
	class ConsoleDriver {
		static IConsoleDriver driver;

		static ConsoleDriver ()
		{
			string term = Environment.GetEnvironmentVariable ("TERM");
			if (term == null && Environment.IsRunningOnWindows) {
			} else {
				driver = new TermInfoDriver (term);
			}
		}

		public static bool Echo {
			get { return driver.Echo; }
			set { driver.Echo = value; }
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

		//int BufferHeight { get; set; }
		//int BufferWidth { get; set; }
		//bool CapsLock { get; }

		public static int CursorLeft {
			get { return driver.CursorLeft; }
			set { driver.CursorLeft = value; }
		}

		//int CursorSize { get; set; }

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

		//int LargestWindowHeight { get; set; }
		//int LargestWindowWidth { get; set; }
		//bool NumberLock { get; }
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

		public static void Beep (int frequency, int duration)
		{
			driver.Beep (frequency, duration);
		}
		
		public static void Clear ()
		{
			driver.Clear ();
		}

		//void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
	//				int targetLeft, int targetTop);
	//	void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
	//				int targetLeft, int targetTop, Char sourceChar,
					//ConsoleColor sourceForeColor, ConsoleColor sourceBackColor);

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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool Isatty (IntPtr handle);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern int InternalKeyAvailable (int ms_timeout);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool TtySetup (string teardown);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool SetEcho (bool wantEcho);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool SetBreak (bool wantBreak);
	}
}
#endif

