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
namespace System {
	class TermInfoDriver : IConsoleDriver {
		static string [] locations = { "/etc/terminfo", "/usr/share/terminfo", "/usr/lib/terminfo" };

		TermInfoReader reader;
		int cursorLeft;
		int cursorTop;
		string title = "";
		string titleFormat = "";
		bool cursorVisible = true;
		string csrVisible;
		string csrInvisible;
		string clear;
		string bell;
		string term;
		Stream stdout;
		Stream stdin;

		int windowWidth;
		int windowHeight;
		//int windowTop;
		//int windowLeft;
		int bufferHeight;
		int bufferWidth;

		string enterCA, exitCA;
		bool controlCAsInput;
		bool inited;
		bool noEcho;
		string origPair;
		string origColors;
		string cursorAddress;
		ConsoleColor fgcolor = ConsoleColor.White;
		ConsoleColor bgcolor = ConsoleColor.Black;
		string setafcolor; // TODO: use setforeground/setbackground if available for better
		string setabcolor; // color mapping.
		bool noGetPosition;

		static string SearchTerminfo (string term)
		{
			if (term == null || term == "")
				return null;

			// Ignore TERMINFO and TERMINFO_DIRS by now
			//string terminfo = Environment.GetEnvironmentVariable ("TERMINFO");
			//string terminfoDirs = Environment.GetEnvironmentVariable ("TERMINFO_DIRS");

			foreach (string dir in locations) {
				if (!Directory.Exists (dir))
					continue;

				string path = Path.Combine (dir, term.Substring (0, 1));
				if (!Directory.Exists (dir))
					continue;

				path = Path.Combine (path, term);
				if (!File.Exists (path))
					continue;

				return path;
			}

			return null;
		}

		static void WriteConsole (string str)
		{
			if (str == null)
				return;

			Console.Write (str);
		}

		public TermInfoDriver ()
			: this (Environment.GetEnvironmentVariable ("TERM"))
		{
		}

		public TermInfoDriver (string term)
		{
			this.term = term;

			if (term == "xterm") {
				reader = new TermInfoReader (term, KnownTerminals.xterm);
			} else if (term == "linux") {
				reader = new TermInfoReader (term, KnownTerminals.linux);
			} else {
				string filename = SearchTerminfo (term);
				if (filename != null)
					reader = new TermInfoReader (term, filename);
			}

			if (reader == null)
				reader = new TermInfoReader (term, KnownTerminals.ansi);
		}

		public bool Initialized {
			get { return inited; }
		}
		
		void Init ()
		{
			if (inited)
				return;

			inited = true;
			enterCA = reader.Get (TermInfoStrings.EnterCaMode);
			exitCA = reader.Get (TermInfoStrings.ExitCaMode);

			if (!ConsoleDriver.Isatty (MonoIO.ConsoleOutput) || !ConsoleDriver.Isatty (MonoIO.ConsoleInput))
				throw new IOException ("Not a tty.");

			origPair = reader.Get (TermInfoStrings.OrigPair);
			origColors = reader.Get (TermInfoStrings.OrigColors);
			setafcolor = MangleParameters (reader.Get (TermInfoStrings.SetAForeground));
			setabcolor = MangleParameters (reader.Get (TermInfoStrings.SetABackground));
			string resetColors = (origColors == null) ? origPair : origColors;
			if (!ConsoleDriver.TtySetup (exitCA + resetColors))
				throw new IOException ("Error initializing terminal.");

			if (enterCA != null && exitCA != null)
				WriteConsole (enterCA);

			stdout = Console.OpenStandardOutput (0);
			stdin = Console.OpenStandardInput (0);
			clear = reader.Get (TermInfoStrings.ClearScreen);
			bell = reader.Get (TermInfoStrings.Bell);
			if (clear == null) {
				clear = reader.Get (TermInfoStrings.CursorHome);
				clear += reader.Get (TermInfoStrings.ClrEos);
			}

			csrVisible = reader.Get (TermInfoStrings.CursorNormal);
			if (csrVisible == null)
				csrVisible = reader.Get (TermInfoStrings.CursorVisible);

			csrInvisible = reader.Get (TermInfoStrings.CursorInvisible);
			if (term == "cygwin" || term == "linux" || term.StartsWith ("xterm") ||
				term == "rxvt" || term == "dtterm") {
				titleFormat = "\x1b]0;{0}\x7"; // icon + window title
			} else if (term == "iris-ansi") {
				titleFormat = "\x1bP1.y{0}\x1b\\"; // not tested
			} else if (term == "sun-cmd") {
				titleFormat = "\x1b]l{0}\x1b\\"; // not tested
			}

			cursorAddress = reader.Get (TermInfoStrings.CursorAddress);
			if (cursorAddress != null) {
				string result = cursorAddress.Replace ("%i", "");
				cursorAddress = MangleParameters (result);
			}
		}

		static string MangleParameters (string str)
		{
			if (str == null)
				return null;

			str = str.Replace ("{", "{{");
			str = str.Replace ("}", "}}");
			str = str.Replace ("%p1%d", "{0}");
			return str.Replace ("%p2%d", "{1}");
		}

		static int TranslateColor (ConsoleColor desired)
		{
			switch (desired) {
			case ConsoleColor.Black:
			case ConsoleColor.DarkGray:
				return 0;
			case ConsoleColor.DarkBlue:
			case ConsoleColor.Blue:
				return 4;
			case ConsoleColor.DarkGreen:
			case ConsoleColor.Green:
				return 2;
			case ConsoleColor.DarkCyan:
			case ConsoleColor.Cyan:
				return 6;
			case ConsoleColor.DarkRed:
			case ConsoleColor.Red:
				return 1;
			case ConsoleColor.DarkMagenta:
			case ConsoleColor.Magenta:
				return 5;
			case ConsoleColor.DarkYellow:
			case ConsoleColor.Yellow:
				return 3;
			case ConsoleColor.Gray:
			case ConsoleColor.White:
				return 7;
			}

			return 0;
		}

		public ConsoleColor BackgroundColor {
			get { return bgcolor; }
			set {
				bgcolor = value;
				WriteConsole (String.Format (setabcolor, TranslateColor (value)));
			}
		}

		public ConsoleColor ForegroundColor {
			get { return fgcolor; }
			set {
				fgcolor = value;
				WriteConsole (String.Format (setafcolor, TranslateColor (value)));
			}
		}

		void GetCursorPosition ()
		{
			if (noGetPosition)
				return;

			int row = 0, col = 0;
			bool prevEcho = Echo;
			Echo = false;
			try {
				WriteConsole ("\x1b[6n");
				if (ConsoleDriver.InternalKeyAvailable (1000) <= 0) {
					noGetPosition = true;
					return;
				}

				int b = stdin.ReadByte ();
				if (b != '\x1b') {
					// Fix this with the buffer
					return;
				}

				b = stdin.ReadByte ();
				if (b != '[') {
					return;
				}

				b = stdin.ReadByte ();
				if (b != ';') {
					row = b - '0';
					b = stdin.ReadByte ();
					if (b != ';')
						row = row * 10 + b - '0';
				}

				b = stdin.ReadByte ();
				if (b != 'R') {
					col = b - '0';
					b = stdin.ReadByte ();
					if (b != 'R') {
						col = col * 10 + b - '0';
						stdin.ReadByte (); // This should be the 'R'
					}
				}
			} finally {
				Echo = prevEcho;
			}

			cursorLeft = col;
			cursorTop = row;
		}

		public int BufferHeight {
			get {
				GetWindowDimensions ();
				return bufferHeight;
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public int BufferWidth {
			get {
				GetWindowDimensions ();
				return bufferWidth;
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public bool CapsLock {
			get { throw new NotSupportedException (); }
		}

		public int CursorLeft {
			get {
				Init ();
				GetCursorPosition ();
				return cursorLeft;
			}
			set {
				Init ();
				SetCursorPosition (value, CursorTop);
				cursorLeft = value;
			}
		}

		public int CursorTop {
			get {
				Init ();
				GetCursorPosition ();
				return cursorTop;
			}
			set {
				Init ();
				SetCursorPosition (CursorLeft, value);
				cursorTop = value;
			}
		}

		public bool CursorVisible {
			get {
				Init ();
				return cursorVisible;
			}
			set {
				Init ();
				cursorVisible = value;
				WriteConsole ((value ? csrVisible : csrInvisible));
			}
		}

		// we have CursorNormal vs. CursorVisible...
		[MonoTODO]
		public int CursorSize {
			get { return 1; }
			set {}
		}

		public bool Echo {
			get { return !noEcho; }
			set {
				if (value != noEcho)
					return;

				noEcho = !value;
				ConsoleDriver.SetEcho (value);
			}
		}

		public bool KeyAvailable {
			get {
				Init ();
				return (ConsoleDriver.InternalKeyAvailable (0) > 0);
			}
		}

		// We don't know these next 2 values, so return something reasonable
		public int LargestWindowHeight {
			get { return WindowHeight; }
		}

		public int LargestWindowWidth {
			get { return WindowWidth; }
		}

		public bool NumberLock {
			get { throw new NotSupportedException (); }
		}

		public string Title {
			get { return title; }
			
			set {
				Init ();
				title = value;
				WriteConsole (String.Format (titleFormat, value));
			}
		}

		public bool TreatControlCAsInput {
			get { return controlCAsInput; }
			set {
				if (controlCAsInput == value)
					return;

				Init ();
				ConsoleDriver.SetBreak (value);
				controlCAsInput = value;
			}
		}

		void GetWindowDimensions ()
		{
			//TODO: Handle SIGWINCH
			windowWidth = reader.Get (TermInfoNumbers.Columns);
			string env = Environment.GetEnvironmentVariable ("COLUMNS");
			if (env != null) {
				try {
					windowWidth = (int) UInt32.Parse (env);
				} catch {
				}
			}

			windowHeight = reader.Get (TermInfoNumbers.Lines);
			env = Environment.GetEnvironmentVariable ("LINES");
			if (env != null) {
				try {
					windowHeight = (int) UInt32.Parse (env);
				} catch {
				}
			}

			//windowTop = 0;
			//windowLeft = 0;
			bufferHeight = windowHeight;
			bufferWidth = windowWidth;
		}

		public int WindowHeight {
			get {
				GetWindowDimensions ();
				return windowHeight;
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public int WindowLeft {
			get {
				//GetWindowDimensions ();
				return 0;
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public int WindowTop {
			get {
				//GetWindowDimensions ();
				return 0;
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public int WindowWidth {
			get {
				GetWindowDimensions ();
				return windowWidth;
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public void Clear ()
		{
			Init ();
			WriteConsole (clear);
			cursorTop = 0;
			cursorLeft = 0;
		}

		public void Beep (int frequency, int duration)
		{
			Init ();
			WriteConsole (bell);
		}

		public void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
					int targetLeft, int targetTop, Char sourceChar,
					ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
		{
			throw new NotImplementedException ();
		}

		public ConsoleKeyInfo ReadKey (bool intercept)
		{
			Init ();
			bool prevEcho = Echo;
			Echo  = !intercept;
			int b = stdin.ReadByte ();
			Echo = prevEcho;
			return new ConsoleKeyInfo ((char) b, (ConsoleKey) b, false, false, false);
		}

		public void ResetColor ()
		{
			Init ();
			string str = (origPair != null) ? origPair : origColors;
			WriteConsole (str);
		}

		public void SetBufferSize (int width, int height)
		{
			throw new NotImplementedException ("");
		}

		public void SetCursorPosition (int left, int top)
		{
			if (bufferWidth == 0)
				GetWindowDimensions ();

			if (left < 0 || left >= bufferWidth)
				throw new ArgumentOutOfRangeException ("left", "Value must be positive and below the buffer width.");

			if (top < 0 || top >= bufferHeight)
				throw new ArgumentOutOfRangeException ("top", "Value must be positive and below the buffer height.");

			Init ();

			// Either CursorAddress or nothing.
			// We might want to play with up/down/left/right/home when ca is not available.
			WriteConsole (String.Format (cursorAddress, top, left));
			cursorLeft = left;
			cursorTop = top;
		}

		public void SetWindowPosition (int left, int top)
		{
			throw new NotSupportedException ();
		}

		public void SetWindowSize (int width, int height)
		{
			throw new NotSupportedException ();
		}
	}
}
#endif

