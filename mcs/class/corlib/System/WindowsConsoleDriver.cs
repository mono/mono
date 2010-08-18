//
// System.WindowsConsoleDriver
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
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
#if !NET_2_1
using System.Runtime.InteropServices;
using System.Text;
namespace System {
	struct ConsoleCursorInfo {
		public int Size;
		public bool Visible;
	}

#pragma warning disable 169
	struct InputRecord {
		public short EventType;
		// This is KEY_EVENT_RECORD
		public bool KeyDown;
		public short RepeatCount;
		public short VirtualKeyCode;
		public short VirtualScanCode;
		public char Character;
		public int ControlKeyState;
		int pad1;
		bool pad2;
		//
	}
#pragma warning restore 169

	struct CharInfo {
		public char Character;
		public short Attributes;
	}
	
	struct Coord {
		public short X;
		public short Y;

		public Coord (int x, int y)
		{
			X = (short) x;
			Y = (short) y;
		}
	}

	struct SmallRect {
		public short Left;
		public short Top;
		public short Right;
		public short Bottom;

		public SmallRect (int left, int top, int right, int bottom)
		{
			Left = (short) left;
			Top = (short) top;
			Right = (short) right;
			Bottom = (short) bottom;
		}
	}

	struct ConsoleScreenBufferInfo {
		public Coord Size;
		public Coord CursorPosition;
		public short Attribute;
		public SmallRect Window;
		public Coord MaxWindowSize;
	}

	enum Handles {
		STD_INPUT = -10,
		STD_OUTPUT = -11,
		STD_ERROR = -12
	}

	unsafe class WindowsConsoleDriver : IConsoleDriver {
		IntPtr inputHandle;
		IntPtr outputHandle;
		short defaultAttribute;

		public WindowsConsoleDriver ()
		{
			outputHandle = GetStdHandle (Handles.STD_OUTPUT);
			inputHandle = GetStdHandle (Handles.STD_INPUT);
			ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
			GetConsoleScreenBufferInfo (outputHandle, out info);
			defaultAttribute = info.Attribute; // Not sure about this...
		}

		// FOREGROUND_BLUE	1
		// FOREGROUND_GREEN	2
		// FOREGROUND_RED	4
		// FOREGROUND_INTENSITY	8
		// BACKGROUND_BLUE	16
		// BACKGROUND_GREEN	32
		// BACKGROUND_RED	64
		// BACKGROUND_INTENSITY	128
		static ConsoleColor GetForeground (short attr)
		{
			attr &= 0x0F;
			return (ConsoleColor) attr;
		}

		static ConsoleColor GetBackground (short attr)
		{
			attr &= 0xF0;
			attr >>= 4;
			return (ConsoleColor) attr;
		}

		static short GetAttrForeground (int attr, ConsoleColor color)
		{
			attr &= ~15;
			return (short) (attr | (int) color);
		}

		static short GetAttrBackground (int attr, ConsoleColor color)
		{
			attr &= ~0xf0;
			int c = ((int) color) << 4;
			return (short) (attr | c);
		}

		public ConsoleColor BackgroundColor {
			get {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				return GetBackground (info.Attribute);
			}
			set {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				short attr = GetAttrBackground (info.Attribute, value);
				SetConsoleTextAttribute (outputHandle, attr);
			}
		}

		public int BufferHeight {
			get {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				return info.Size.Y;
			}
			set { SetBufferSize (BufferWidth, value); }
		}

		public int BufferWidth {
			get {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				return info.Size.X;
			}
			set { SetBufferSize (value, BufferHeight); }
		}

		public bool CapsLock {
			get {
				short state = GetKeyState (20); // VK_CAPITAL
				return ((state & 1) == 1);
			}
		}

		public int CursorLeft {
			get {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				return info.CursorPosition.X;
			}
			set { SetCursorPosition (value, CursorTop); }
		}

		public int CursorSize {
			get {
				ConsoleCursorInfo info = new ConsoleCursorInfo ();
				GetConsoleCursorInfo (outputHandle, out info);
				return info.Size;
			}
			set {
				if (value < 1 || value > 100)
					throw new ArgumentOutOfRangeException ("value");

				ConsoleCursorInfo info = new ConsoleCursorInfo ();
				GetConsoleCursorInfo (outputHandle, out info);
				info.Size = value;
				if (!SetConsoleCursorInfo (outputHandle, ref info))
					throw new Exception ("SetConsoleCursorInfo failed");
			}
		}

		public int CursorTop {
			get {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				return info.CursorPosition.Y;
			}
			set { SetCursorPosition (CursorLeft, value); }
		}

		public bool CursorVisible {
			get {
				ConsoleCursorInfo info = new ConsoleCursorInfo ();
				GetConsoleCursorInfo (outputHandle, out info);
				return info.Visible;
			}
			set {
				ConsoleCursorInfo info = new ConsoleCursorInfo ();
				GetConsoleCursorInfo (outputHandle, out info);
				if (info.Visible == value)
					return;

				info.Visible = value;
				if (!SetConsoleCursorInfo (outputHandle, ref info))
					throw new Exception ("SetConsoleCursorInfo failed");
			}
		}

		public ConsoleColor ForegroundColor {
			get {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				return GetForeground (info.Attribute);
			}
			set {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				short attr = GetAttrForeground (info.Attribute, value);
				SetConsoleTextAttribute (outputHandle, attr);
			}
		}

		public bool KeyAvailable {
			get {
				int eventsRead;
				InputRecord record = new InputRecord ();
				while (true) {
					// Use GetNumberOfConsoleInputEvents and remove the while?
					if (!PeekConsoleInput (inputHandle, out record, 1, out eventsRead))
						throw new InvalidOperationException ("Error in PeekConsoleInput " +
										Marshal.GetLastWin32Error ());

					if (eventsRead == 0)
						return false;

					//KEY_EVENT == 1
					if (record.EventType == 1 && record.KeyDown)
						return true;

					if (!ReadConsoleInput (inputHandle, out record, 1, out eventsRead))
						throw new InvalidOperationException ("Error in ReadConsoleInput " +
										Marshal.GetLastWin32Error ());
				}
			}
		}

		public bool Initialized { // Not useful on windows, so false.
			get { return false; }
		}

		public int LargestWindowHeight {
			get {
				Coord coord = GetLargestConsoleWindowSize (outputHandle);
				if (coord.X == 0 && coord.Y == 0)
					throw new Exception ("GetLargestConsoleWindowSize" + Marshal.GetLastWin32Error ());

				return coord.Y;
			}
		}

		public int LargestWindowWidth {
			get {
				Coord coord = GetLargestConsoleWindowSize (outputHandle);
				if (coord.X == 0 && coord.Y == 0)
					throw new Exception ("GetLargestConsoleWindowSize" + Marshal.GetLastWin32Error ());

				return coord.X;
			}
		}

		public bool NumberLock {
			get {
				short state = GetKeyState (144); // VK_NUMLOCK
				return ((state & 1) == 1);
			}
		}

		public string Title {
			get {
				StringBuilder sb = new StringBuilder (1024); // hope this is enough
				if (GetConsoleTitle (sb, 1024) == 0) {
					// Try the maximum
					sb = new StringBuilder (26001);
					if (GetConsoleTitle (sb, 26000) == 0)
						throw new Exception ("Got " + Marshal.GetLastWin32Error ());
				}

				return sb.ToString ();
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				if (!SetConsoleTitle (value))
					throw new Exception ("Got " + Marshal.GetLastWin32Error ());
			}
		}

		public bool TreatControlCAsInput {
			get {
				int mode;
				if (!GetConsoleMode (inputHandle, out mode))
					throw new Exception ("Failed in GetConsoleMode: " + Marshal.GetLastWin32Error ());

				// ENABLE_PROCESSED_INPUT
				return ((mode & 1) == 0);
			}

			set {
				int mode;
				if (!GetConsoleMode (inputHandle, out mode))
					throw new Exception ("Failed in GetConsoleMode: " + Marshal.GetLastWin32Error ());

				bool cAsInput = ((mode & 1) == 0);
				if (cAsInput == value)
					return;

				if (value)
					mode &= ~1;
				else
					mode |= 1;

				if (!SetConsoleMode (inputHandle, mode))
					throw new Exception ("Failed in SetConsoleMode: " + Marshal.GetLastWin32Error ());
			}
		}

		public int WindowHeight {
			get {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				return info.Window.Bottom - info.Window.Top + 1;
			}
			set { SetWindowSize (WindowWidth, value); }
		}

		public int WindowLeft {
			get {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				return info.Window.Left;
			}
			set { SetWindowPosition (value, WindowTop); }
		}

		public int WindowTop {
			get {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				return info.Window.Top;
			}
			set { SetWindowPosition (WindowLeft, value); }
		}

		public int WindowWidth {
			get {
				ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
				GetConsoleScreenBufferInfo (outputHandle, out info);
				return info.Window.Right - info.Window.Left + 1;
			}
			set { SetWindowSize (value, WindowHeight); }
		}

		public void Beep (int frequency, int duration)
		{
			_Beep (frequency, duration);
		}

		public void Clear ()
		{
			Coord coord = new Coord ();
			ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
			GetConsoleScreenBufferInfo (outputHandle, out info);

			int size = info.Size.X * info.Size.Y;
			int written;
			FillConsoleOutputCharacter (outputHandle, ' ', size, coord, out written);

			GetConsoleScreenBufferInfo (outputHandle, out info);

			FillConsoleOutputAttribute (outputHandle, info.Attribute, size, coord, out written);
			SetConsoleCursorPosition (outputHandle, coord);
		}

		public void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
					int targetLeft, int targetTop, Char sourceChar,
					ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
		{
			if (sourceForeColor < 0)
				throw new ArgumentException ("Cannot be less than 0.", "sourceForeColor");

			if (sourceBackColor < 0)
				throw new ArgumentException ("Cannot be less than 0.", "sourceBackColor");

			if (sourceWidth == 0 || sourceHeight == 0)
				return;

			ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
			GetConsoleScreenBufferInfo (outputHandle, out info);
			CharInfo [] buffer = new CharInfo [sourceWidth * sourceHeight];
			Coord bsize = new Coord (sourceWidth, sourceHeight);
			Coord bpos = new Coord (0, 0);
			SmallRect region = new SmallRect (sourceLeft, sourceTop, sourceLeft + sourceWidth - 1, sourceTop + sourceHeight - 1);
			fixed (void *ptr = &buffer [0]) {
				if (!ReadConsoleOutput (outputHandle, ptr, bsize, bpos, ref region))
					throw new ArgumentException (String.Empty, "Cannot read from the specified coordinates.");
			}

			int written;
			short attr  = GetAttrForeground (0, sourceForeColor);
			attr  = GetAttrBackground (attr, sourceBackColor);
			bpos = new Coord (sourceLeft, sourceTop);
			for (int i = 0; i < sourceHeight; i++, bpos.Y++) {
				FillConsoleOutputCharacter (outputHandle, sourceChar, sourceWidth, bpos, out written);
				FillConsoleOutputAttribute (outputHandle, attr, sourceWidth, bpos, out written);
			}

			bpos = new Coord (0, 0);
			region = new SmallRect (targetLeft, targetTop, targetLeft + sourceWidth - 1, targetTop + sourceHeight - 1);
			if (!WriteConsoleOutput (outputHandle, buffer, bsize, bpos, ref region))
				throw new ArgumentException (String.Empty, "Cannot write to the specified coordinates.");
		}

		public void Init ()
		{
		}

		public string ReadLine ()
		{
			StringBuilder builder = new StringBuilder ();
			bool exit = false;
			do {
				ConsoleKeyInfo key = ReadKey (false);
				char c = key.KeyChar;
				exit = (c == '\n');
				if (!exit)
					builder.Append (key.KeyChar);
			} while (!exit);
			return builder.ToString ();
		}

		public ConsoleKeyInfo ReadKey (bool intercept)
		{
			int eventsRead;
			InputRecord record = new InputRecord ();
			for (;;) {
				if (!ReadConsoleInput (inputHandle, out record, 1, out eventsRead))
					throw new InvalidOperationException ("Error in ReadConsoleInput " +
									Marshal.GetLastWin32Error ());
				if (record.KeyDown && record.EventType == 1 && !IsModifierKey (record.VirtualKeyCode))
					break;
			}

			// RIGHT_ALT_PRESSED 1
			// LEFT_ALT_PRESSED 2
			// RIGHT_CTRL_PRESSED 4
			// LEFT_CTRL_PRESSED 8
			// SHIFT_PRESSED 16
			bool alt = ((record.ControlKeyState & 3) != 0);
			bool ctrl = ((record.ControlKeyState & 12) != 0);
			bool shift = ((record.ControlKeyState & 16) != 0);
			return new ConsoleKeyInfo (record.Character, (ConsoleKey) record.VirtualKeyCode, shift, alt, ctrl);
		}

		public void ResetColor ()
		{
			SetConsoleTextAttribute (outputHandle, defaultAttribute);
		}

		public void SetBufferSize (int width, int height)
		{
			ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
			GetConsoleScreenBufferInfo (outputHandle, out info);

			if (width - 1 > info.Window.Right)
				throw new ArgumentOutOfRangeException ("width");

			if (height - 1 > info.Window.Bottom)
				throw new ArgumentOutOfRangeException ("height");

			Coord coord = new Coord (width, height);
			if (!SetConsoleScreenBufferSize (outputHandle, coord))
				throw new ArgumentOutOfRangeException ("height/width", "Cannot be smaller than the window size.");
		}

		public void SetCursorPosition (int left, int top)
		{
			Coord coord = new Coord (left, top);
			SetConsoleCursorPosition (outputHandle, coord);
		}

		public void SetWindowPosition (int left, int top)
		{
			ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
			GetConsoleScreenBufferInfo (outputHandle, out info);
			SmallRect rect = info.Window;
			rect.Left = (short) left;
			rect.Top = (short) top;
			if (!SetConsoleWindowInfo (outputHandle, true, ref rect))
				throw new ArgumentOutOfRangeException ("left/top", "Windows error " + Marshal.GetLastWin32Error ());
		}

		public void SetWindowSize (int width, int height)
		{
			ConsoleScreenBufferInfo info = new ConsoleScreenBufferInfo ();
			GetConsoleScreenBufferInfo (outputHandle, out info);
			SmallRect rect = info.Window;
			rect.Right = (short) (rect.Left + width - 1);
			rect.Bottom = (short) (rect.Top + height - 1);
			if (!SetConsoleWindowInfo (outputHandle, true, ref rect))
				throw new ArgumentOutOfRangeException ("left/top", "Windows error " + Marshal.GetLastWin32Error ());
		}

		private bool IsModifierKey(short virtualKeyCode)
		{
			// 0x10 through 0x14 is shift/control/alt/pause/capslock
			// 0x2C is print screen, 0x90 is numlock, 0x91 is scroll lock
			switch (virtualKeyCode) {
			case 0x10:
			case 0x11:
			case 0x12:
			case 0x13:
			case 0x14:
				return true;
			case 0x2C:
			case 0x90:
			case 0x91:
				return true;
			default:
				return false;
			}
		}

		//
		// Imports
		//
		[DllImport ("kernel32.dll", EntryPoint="GetStdHandle", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static IntPtr GetStdHandle (Handles handle);

		[DllImport ("kernel32.dll", EntryPoint="Beep", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static void _Beep (int frequency, int duration);

		[DllImport ("kernel32.dll", EntryPoint="GetConsoleScreenBufferInfo", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool GetConsoleScreenBufferInfo (IntPtr handle, out ConsoleScreenBufferInfo info);

		[DllImport ("kernel32.dll", EntryPoint="FillConsoleOutputCharacter", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool FillConsoleOutputCharacter (IntPtr handle, char c, int size, Coord coord, out int written);

		[DllImport ("kernel32.dll", EntryPoint="FillConsoleOutputAttribute", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool FillConsoleOutputAttribute (IntPtr handle, short c, int size, Coord coord, out int written);

		[DllImport ("kernel32.dll", EntryPoint="SetConsoleCursorPosition", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool SetConsoleCursorPosition (IntPtr handle, Coord coord);

		[DllImport ("kernel32.dll", EntryPoint="SetConsoleTextAttribute", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool SetConsoleTextAttribute (IntPtr handle, short attribute);

		[DllImport ("kernel32.dll", EntryPoint="SetConsoleScreenBufferSize", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool SetConsoleScreenBufferSize (IntPtr handle, Coord newSize);

		[DllImport ("kernel32.dll", EntryPoint="SetConsoleWindowInfo", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool SetConsoleWindowInfo (IntPtr handle, bool absolute, ref SmallRect rect);

		[DllImport ("kernel32.dll", EntryPoint="GetConsoleTitle", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static int GetConsoleTitle (StringBuilder sb, int size);

		[DllImport ("kernel32.dll", EntryPoint="SetConsoleTitle", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool SetConsoleTitle (string title);

		[DllImport ("kernel32.dll", EntryPoint="GetConsoleCursorInfo", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool GetConsoleCursorInfo (IntPtr handle, out ConsoleCursorInfo info);

		[DllImport ("kernel32.dll", EntryPoint="SetConsoleCursorInfo", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool SetConsoleCursorInfo (IntPtr handle, ref ConsoleCursorInfo info);

		[DllImport ("user32.dll", EntryPoint="GetKeyState", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static short GetKeyState (int virtKey);

		[DllImport ("kernel32.dll", EntryPoint="GetConsoleMode", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool GetConsoleMode (IntPtr handle, out int mode);

		[DllImport ("kernel32.dll", EntryPoint="SetConsoleMode", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool SetConsoleMode (IntPtr handle, int mode);

		[DllImport ("kernel32.dll", EntryPoint="PeekConsoleInput", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool PeekConsoleInput (IntPtr handle, out InputRecord record, int length, out int eventsRead);

		[DllImport ("kernel32.dll", EntryPoint="ReadConsoleInput", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool ReadConsoleInput (IntPtr handle, out InputRecord record, int length, out int nread);

		[DllImport ("kernel32.dll", EntryPoint="GetLargestConsoleWindowSize", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static Coord GetLargestConsoleWindowSize (IntPtr handle);

		[DllImport ("kernel32.dll", EntryPoint="ReadConsoleOutput", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool ReadConsoleOutput (IntPtr handle, void *buffer, Coord bsize, Coord bpos, ref SmallRect region);

		[DllImport ("kernel32.dll", EntryPoint="WriteConsoleOutput", SetLastError=true, CharSet=CharSet.Unicode)]
		extern static bool WriteConsoleOutput (IntPtr handle, CharInfo [] buffer, Coord bsize, Coord bpos, ref SmallRect region);
	}
}
#endif

