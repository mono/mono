//
// System.ConsoleDriver
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005,2006 Novell, Inc (http://www.novell.com)
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
//#define DEBUG
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
namespace System {
	class TermInfoDriver : IConsoleDriver {
		/* Do not rename this field, its looked up from the runtime */
		static bool need_window_dimensions = true;
		
		static string [] locations = { "/etc/terminfo", "/usr/share/terminfo", "/usr/lib/terminfo" };

		TermInfoReader reader;
		int cursorLeft;
		int cursorTop;
		string title = String.Empty;
		string titleFormat = String.Empty;
		bool cursorVisible = true;
		string csrVisible;
		string csrInvisible;
		string clear;
		string bell;
		string term;
		Stream stdin;
		internal byte verase;
		byte vsusp;
		byte intr;

		int windowWidth;
		int windowHeight;
		//int windowTop;
		//int windowLeft;
		int bufferHeight;
		int bufferWidth;

		byte [] buffer;
		int readpos;
		int writepos;
		string keypadXmit, keypadLocal;
		bool controlCAsInput;
		bool inited;
		bool initKeys;
		string origPair;
		string origColors;
		string cursorAddress;
		ConsoleColor fgcolor = ConsoleColor.White;
		ConsoleColor bgcolor = ConsoleColor.Black;
		string setafcolor; // TODO: use setforeground/setbackground if available for better
		string setabcolor; // color mapping.
		bool noGetPosition;
		Hashtable keymap;
		ByteMatcher rootmap;
		bool home_1_1; // if true, we have to add 1 to x and y when using cursorAddress
		int rl_startx = -1, rl_starty = -1;
#if DEBUG
		StreamWriter logger;
#endif

		static string SearchTerminfo (string term)
		{
			if (term == null || term == String.Empty)
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

			((CStreamWriter) Console.stdout).InternalWriteString (str);
		}

		public TermInfoDriver ()
			: this (Environment.GetEnvironmentVariable ("TERM"))
		{
		}

		public TermInfoDriver (string term)
		{
#if DEBUG
			//File.Delete ("console.log");
			logger = new StreamWriter (File.OpenWrite ("console.log"));
#endif
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

		public void Init ()
		{
			if (inited)
				return;
			
			/* This should not happen any more, since it is checked for in Console */
			if (!ConsoleDriver.IsConsole)
				throw new IOException ("Not a tty.");

			inited = true;

			ConsoleDriver.SetEcho (false);

			string endString = null;
			keypadXmit = reader.Get (TermInfoStrings.KeypadXmit);
			keypadLocal = reader.Get (TermInfoStrings.KeypadLocal);
			if (keypadXmit != null) {
				WriteConsole (keypadXmit); // Needed to get the arrows working
				if (keypadLocal != null)
					endString += keypadLocal;
			}

			origPair = reader.Get (TermInfoStrings.OrigPair);
			origColors = reader.Get (TermInfoStrings.OrigColors);
			setafcolor = MangleParameters (reader.Get (TermInfoStrings.SetAForeground));
			setabcolor = MangleParameters (reader.Get (TermInfoStrings.SetABackground));
			string resetColors = (origColors == null) ? origPair : origColors;
			if (resetColors != null)
				endString += resetColors;

			if (!ConsoleDriver.TtySetup (endString, out verase, out vsusp, out intr))
				throw new IOException ("Error initializing terminal.");

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
			if (term == "cygwin" || term == "linux" || (term != null && term.StartsWith ("xterm")) ||
				term == "rxvt" || term == "dtterm") {
				titleFormat = "\x1b]0;{0}\x7"; // icon + window title
			} else if (term == "iris-ansi") {
				titleFormat = "\x1bP1.y{0}\x1b\\"; // not tested
			} else if (term == "sun-cmd") {
				titleFormat = "\x1b]l{0}\x1b\\"; // not tested
			}

			cursorAddress = reader.Get (TermInfoStrings.CursorAddress);
			if (cursorAddress != null) {
				string result = cursorAddress.Replace ("%i", String.Empty);
				home_1_1 = (cursorAddress != result);
				cursorAddress = MangleParameters (result);
			}

			GetCursorPosition ();
#if DEBUG
			logger.WriteLine ("noGetPosition: {0} left: {1} top: {2}", noGetPosition, cursorLeft, cursorTop);
			logger.Flush ();
#endif
			if (noGetPosition) {
				WriteConsole (clear);
				cursorLeft = 0;
				cursorTop = 0;
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

		void IncrementX ()
		{
			cursorLeft++;
			if (cursorLeft >= WindowWidth) {
				cursorTop++;
				cursorLeft = 0;
				if (cursorTop >= WindowHeight) {
					// Writing beyond the initial screen
					if (rl_starty != -1) rl_starty--;
					cursorTop--;
				}
			}
		}

		// Should never get called unless inited
		public void WriteSpecialKey (ConsoleKeyInfo key)
		{
			switch (key.Key) {
			case ConsoleKey.Backspace:
				if (cursorLeft > 0) {
					if (cursorLeft <= rl_startx && cursorTop == rl_starty)
						break;
					cursorLeft--;
					SetCursorPosition (cursorLeft, cursorTop);
					WriteConsole (" ");
					SetCursorPosition (cursorLeft, cursorTop);
				}
#if DEBUG
				logger.WriteLine ("BS left: {0} top: {1}", cursorLeft, cursorTop);
				logger.Flush ();
#endif
				break;
			case ConsoleKey.Tab:
				int n = 8 - (cursorLeft % 8);
				for (int i = 0; i < n; i++)
					IncrementX ();
				break;
			case ConsoleKey.Clear:
				break;
			case ConsoleKey.Enter:
				break;
			default:
				break;
			}
#if DEBUG
			logger.WriteLine ("left: {0} top: {1}", cursorLeft, cursorTop);
			logger.Flush ();
#endif
		}

		// Should never get called unless inited
		public void WriteSpecialKey (char c)
		{
			WriteSpecialKey (CreateKeyInfoFromInt (c));
		}

		public bool IsSpecialKey (ConsoleKeyInfo key)
		{
			if (!inited)
				return false;

			switch (key.Key) {
			case ConsoleKey.Backspace:
				return true;
			case ConsoleKey.Tab:
				return true;
			case ConsoleKey.Clear:
				return true;
			case ConsoleKey.Enter:
				cursorLeft = 0;
				cursorTop++;
				if (cursorTop >= WindowHeight) {
					cursorTop--;
					//TODO: scroll up
				}
				return false;
			default:
				// CStreamWriter will handle writing this key
				IncrementX ();
				return false;
			}
		}

		public bool IsSpecialKey (char c)
		{
			return IsSpecialKey (CreateKeyInfoFromInt (c));
		}

		public ConsoleColor BackgroundColor {
			get {
				if (!inited) {
					Init ();
				}

				return bgcolor;
			}
			set {
				if (!inited) {
					Init ();
				}

				bgcolor = value;
				WriteConsole (String.Format (setabcolor, TranslateColor (value)));
			}
		}

		public ConsoleColor ForegroundColor {
			get {
				if (!inited) {
					Init ();
				}

				return fgcolor;
			}
			set {
				if (!inited) {
					Init ();
				}

				fgcolor = value;
				WriteConsole (String.Format (setafcolor, TranslateColor (value)));
			}
		}

		void GetCursorPosition ()
		{
			int row = 0, col = 0;

			WriteConsole ("\x1b[6n");
			if (ConsoleDriver.InternalKeyAvailable (1000) <= 0) {
				noGetPosition = true;
				return;
			}

			int b = stdin.ReadByte ();
			while (b != '\x1b') {
				AddToBuffer (b);
				if (ConsoleDriver.InternalKeyAvailable (100) <= 0)
					return;
				b = stdin.ReadByte ();
			}

			b = stdin.ReadByte ();
			if (b != '[') {
				AddToBuffer ('\x1b');
				AddToBuffer (b);
				return;
			}

			b = stdin.ReadByte ();
			if (b != ';') {
				row = b - '0';
				b = stdin.ReadByte ();
				while ((b >= '0') && (b <= '9')) {
					row = row * 10 + b - '0';
					b = stdin.ReadByte ();
				}
				// Row/col is 0 based
				row --;
			}

			b = stdin.ReadByte ();
			if (b != 'R') {
				col = b - '0';
				b = stdin.ReadByte ();
				while ((b >= '0') && (b <= '9')) {
					col = col * 10 + b - '0';
					b = stdin.ReadByte ();
				}
				// Row/col is 0 based
				col --;
			}

#if DEBUG
			logger.WriteLine ("GetCursorPosition: {0}, {1}", col, row);
			logger.Flush ();
#endif

			cursorLeft = col;
			cursorTop = row;
		}

		public int BufferHeight {
			get {
				if (!inited) {
					Init ();
				}

				return bufferHeight;
			}
			set {
				if (!inited) {
					Init ();
				}

				throw new NotSupportedException ();
			}
		}

		public int BufferWidth {
			get {
				if (!inited) {
					Init ();
				}

				return bufferWidth;
			}
			set {
				if (!inited) {
					Init ();
				}

				throw new NotSupportedException ();
			}
		}

		public bool CapsLock {
			get {
				if (!inited) {
					Init ();
				}

				throw new NotSupportedException ();
			}
		}

		public int CursorLeft {
			get {
				if (!inited) {
					Init ();
				}

				return cursorLeft;
			}
			set {
				if (!inited) {
					Init ();
				}

				SetCursorPosition (value, CursorTop);
				cursorLeft = value;
			}
		}

		public int CursorTop {
			get {
				if (!inited) {
					Init ();
				}

				return cursorTop;
			}
			set {
				if (!inited) {
					Init ();
				}

				SetCursorPosition (CursorLeft, value);
				cursorTop = value;
			}
		}

		public bool CursorVisible {
			get {
				if (!inited) {
					Init ();
				}

				return cursorVisible;
			}
			set {
				if (!inited) {
					Init ();
				}

				cursorVisible = value;
				WriteConsole ((value ? csrVisible : csrInvisible));
			}
		}

		// we have CursorNormal vs. CursorVisible...
		[MonoTODO]
		public int CursorSize {
			get {
				if (!inited) {
					Init ();
				}
				return 1;
			}
			set {
				if (!inited) {
					Init ();
				}
			}

		}

		public bool KeyAvailable {
			get {
				if (!inited) {
					Init ();
				}

				return (writepos > readpos || ConsoleDriver.InternalKeyAvailable (0) > 0);
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
			get {
				if (!inited) {
					Init ();
				}

				throw new NotSupportedException ();
			}
		}

		public string Title {
			get {
				if (!inited) {
					Init ();
				}
				return title;
			}
			
			set {
				if (!inited) {
					Init ();
				}

				title = value;
				WriteConsole (String.Format (titleFormat, value));
			}
		}

		public bool TreatControlCAsInput {
			get {
				if (!inited) {
					Init ();
				}
				return controlCAsInput;
			}
			set {
				if (!inited) {
					Init ();
				}

				if (controlCAsInput == value)
					return;

				ConsoleDriver.SetBreak (value);
				controlCAsInput = value;
			}
		}

		void GetWindowDimensions ()
		{
			/* Try the ioctl first */
			if (!ConsoleDriver.GetTtySize (MonoIO.ConsoleOutput, out windowWidth, out windowHeight)) {
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
			}

			bufferHeight = windowHeight;
			bufferWidth = windowWidth;
			need_window_dimensions = false;
		}

		public int WindowHeight {
			get {
				if (!inited) {
					Init ();
				}

				if (need_window_dimensions)
					GetWindowDimensions ();
				return windowHeight;
			}
			set {
				if (!inited) {
					Init ();
				}

				throw new NotSupportedException ();
			}
		}

		public int WindowLeft {
			get {
				if (!inited) {
					Init ();
				}

				//GetWindowDimensions ();
				return 0;
			}
			set {
				if (!inited) {
					Init ();
				}

				throw new NotSupportedException ();
			}
		}

		public int WindowTop {
			get {
				if (!inited) {
					Init ();
				}

				//GetWindowDimensions ();
				return 0;
			}
			set {
				if (!inited) {
					Init ();
				}

				throw new NotSupportedException ();
			}
		}

		public int WindowWidth {
			get {
				if (!inited) {
					Init ();
				}

				if (need_window_dimensions)
					GetWindowDimensions ();
				return windowWidth;
			}
			set {
				if (!inited) {
					Init ();
				}

				throw new NotSupportedException ();
			}
		}

		public void Clear ()
		{
			if (!inited) {
				Init ();
			}

			WriteConsole (clear);
		}

		public void Beep (int frequency, int duration)
		{
			if (!inited) {
				Init ();
			}

			WriteConsole (bell);
		}

		public void MoveBufferArea (int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
					int targetLeft, int targetTop, Char sourceChar,
					ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
		{
			if (!inited) {
				Init ();
			}

			throw new NotImplementedException ();
		}

		void AddToBuffer (int b)
		{
			if (buffer == null) {
				buffer = new byte [1024];
			} else if (writepos >= buffer.Length) {
				byte [] newbuf = new byte [buffer.Length * 2];
				Buffer.BlockCopy (buffer, 0, newbuf, 0, buffer.Length);
				buffer = newbuf;
			}

			buffer [writepos++] = (byte) b;
		}

		void AdjustBuffer ()
		{
			if (readpos >= writepos) {
				readpos = writepos = 0;
			}
		}

		ConsoleKeyInfo CreateKeyInfoFromInt (int n)
		{
			char c = (char) n;
			ConsoleKey key = (ConsoleKey)n;
			bool shift = false;
			bool ctrl = false;
			bool alt = false;

			if (n == 10) {
				key = ConsoleKey.Enter;
			} else if (n == 8 || n == 9 || n == 12 || n == 13 || n == 19) {
				/* Values in ConsoleKey */
			} else if (n >= 1 && n <= 26) {
				// For Ctrl-a to Ctrl-z.
				ctrl = true;
				key = ConsoleKey.A + n - 1;
			} else if (n == 27) {
				key = ConsoleKey.Escape;
			} else if (n >= 'a' && n <= 'z') {
				key = ConsoleKey.A - 'a' + n;
			} else if (n >= 'A' && n <= 'Z') {
				shift = true;
			} else if (n >= '0' && n <= '9') {
			} else {
				key = 0;
			}

			return new ConsoleKeyInfo (c, key, shift, alt, ctrl);
		}

		object GetKeyFromBuffer (bool cooked)
		{
			if (readpos >= writepos)
				return null;

			int next = buffer [readpos];
			if (!cooked || !rootmap.StartsWith (next)) {
				readpos++;
				AdjustBuffer ();
				return CreateKeyInfoFromInt (next);
			}

			int used;
			TermInfoStrings str = rootmap.Match (buffer, readpos, writepos - readpos, out used);
			if ((int) str == -1)
				return null;

			ConsoleKeyInfo key;
			if (keymap [str] != null) {
				key = (ConsoleKeyInfo) keymap [str];
			} else {
				readpos++;
				AdjustBuffer ();
				return CreateKeyInfoFromInt (next);
			}

			readpos += used;
			AdjustBuffer ();
			return key;
		}

		ConsoleKeyInfo ReadKeyInternal ()
		{
			if (!inited) {
				Init ();
			}

			InitKeys ();
			object o = null;
			while (o == null) {
				o = GetKeyFromBuffer (true);
				if (o == null) {
					if (ConsoleDriver.InternalKeyAvailable (150) > 0) {
						do {
							AddToBuffer (stdin.ReadByte ());
						} while (ConsoleDriver.InternalKeyAvailable (0) > 0);
					} else {
						o = GetKeyFromBuffer (false);
						if (o == null)
							AddToBuffer (stdin.ReadByte ());
					}
				}
			}

			return (ConsoleKeyInfo) o;
		}

		public ConsoleKeyInfo ReadKey (bool intercept)
		{
			if (!inited)
				Init ();
			
			ConsoleKeyInfo key = ReadKeyInternal ();
			
			if (!intercept) {
				// echo the key back to the console
				CStreamWriter writer = Console.stdout as CStreamWriter;

				if (writer != null)
					writer.WriteKey (key);
				else
					Console.stdout.Write (key.KeyChar);
			}
			
			return key;
		}

		public string ReadLine ()
 		{
			if (!inited)
				Init ();

			// Hack to make Iron Python work (since it goes behind our backs
			// when writing to the console thus preventing us from keeping
			// cursor state normally).
			GetCursorPosition ();

			StringBuilder builder = new StringBuilder ();
			bool exit = false;
			CStreamWriter writer = Console.stdout as CStreamWriter;
			rl_startx = cursorLeft;
			rl_starty = cursorTop;
			do {
				ConsoleKeyInfo key = ReadKeyInternal ();
				char c = key.KeyChar;
				exit = (c == '\n');
				if (!exit) {
					if (key.Key != ConsoleKey.Backspace)
						builder.Append (c);
					else if (builder.Length > 0)
						builder.Length--;
					else {
						// skips over echoing the key to the console
						continue;
					}
				}
				
				// echo the key back to the console
				if (writer != null)
					writer.WriteKey (key);
				else
					Console.stdout.Write (c);
			} while (!exit);
			
			rl_startx = -1;
			rl_starty = -1;
			return builder.ToString ();
 		}

		public void ResetColor ()
		{
			if (!inited) {
				Init ();
			}

			string str = (origPair != null) ? origPair : origColors;
			WriteConsole (str);
		}

		public void SetBufferSize (int width, int height)
		{
			if (!inited) {
				Init ();
			}

			throw new NotImplementedException (String.Empty);
		}

		public void SetCursorPosition (int left, int top)
		{
			if (!inited) {
				Init ();
			}

			if (bufferWidth == 0 && need_window_dimensions)
				GetWindowDimensions ();

			if (left < 0 || left >= bufferWidth)
				throw new ArgumentOutOfRangeException ("left", "Value must be positive and below the buffer width.");

			if (top < 0 || top >= bufferHeight)
				throw new ArgumentOutOfRangeException ("top", "Value must be positive and below the buffer height.");

			// Either CursorAddress or nothing.
			// We might want to play with up/down/left/right/home when ca is not available.
			if (cursorAddress == null)
				throw new NotSupportedException ("This terminal does not suport setting the cursor position.");

			int one = (home_1_1 ? 1 : 0);
			WriteConsole (String.Format (cursorAddress, top + one, left + one));
			cursorLeft = left;
			cursorTop = top;
		}

		public void SetWindowPosition (int left, int top)
		{
			if (!inited) {
				Init ();
			}

			// No need to throw exceptions here.
			//throw new NotSupportedException ();
		}

		public void SetWindowSize (int width, int height)
		{
			if (!inited) {
				Init ();
			}

			// No need to throw exceptions here.
			//throw new NotSupportedException ();
		}


		void CreateKeyMap ()
		{
			keymap = new Hashtable ();
			keymap [TermInfoStrings.KeyBackspace] = new ConsoleKeyInfo ('\0', ConsoleKey.Backspace, false, false, false);
			keymap [TermInfoStrings.KeyClear] = new ConsoleKeyInfo ('\0', ConsoleKey.Clear, false, false, false);
			// Delete character...
			keymap [TermInfoStrings.KeyDown] = new ConsoleKeyInfo ('\0', ConsoleKey.DownArrow, false, false, false);
			keymap [TermInfoStrings.KeyF1] = new ConsoleKeyInfo ('\0', ConsoleKey.F1, false, false, false);
			keymap [TermInfoStrings.KeyF10] = new ConsoleKeyInfo ('\0', ConsoleKey.F10, false, false, false);
			keymap [TermInfoStrings.KeyF2] = new ConsoleKeyInfo ('\0', ConsoleKey.F2, false, false, false);
			keymap [TermInfoStrings.KeyF3] = new ConsoleKeyInfo ('\0', ConsoleKey.F3, false, false, false);
			keymap [TermInfoStrings.KeyF4] = new ConsoleKeyInfo ('\0', ConsoleKey.F4, false, false, false);
			keymap [TermInfoStrings.KeyF5] = new ConsoleKeyInfo ('\0', ConsoleKey.F5, false, false, false);
			keymap [TermInfoStrings.KeyF6] = new ConsoleKeyInfo ('\0', ConsoleKey.F6, false, false, false);
			keymap [TermInfoStrings.KeyF7] = new ConsoleKeyInfo ('\0', ConsoleKey.F7, false, false, false);
			keymap [TermInfoStrings.KeyF8] = new ConsoleKeyInfo ('\0', ConsoleKey.F8, false, false, false);
			keymap [TermInfoStrings.KeyF9] = new ConsoleKeyInfo ('\0', ConsoleKey.F9, false, false, false);
			keymap [TermInfoStrings.KeyHome] = new ConsoleKeyInfo ('\0', ConsoleKey.Home, false, false, false);

			keymap [TermInfoStrings.KeyLeft] = new ConsoleKeyInfo ('\0', ConsoleKey.LeftArrow, false, false, false);
			keymap [TermInfoStrings.KeyLl] = new ConsoleKeyInfo ('\0', ConsoleKey.NumPad1, false, false, false);
			keymap [TermInfoStrings.KeyNpage] = new ConsoleKeyInfo ('\0', ConsoleKey.PageDown, false, false, false);
			keymap [TermInfoStrings.KeyPpage] = new ConsoleKeyInfo ('\0', ConsoleKey.PageUp, false, false, false);
			keymap [TermInfoStrings.KeyRight] = new ConsoleKeyInfo ('\0', ConsoleKey.RightArrow, false, false, false);
			keymap [TermInfoStrings.KeySf] = new ConsoleKeyInfo ('\0', ConsoleKey.PageDown, false, false, false);
			keymap [TermInfoStrings.KeySr] = new ConsoleKeyInfo ('\0', ConsoleKey.PageUp, false, false, false);
			keymap [TermInfoStrings.KeyUp] = new ConsoleKeyInfo ('\0', ConsoleKey.UpArrow, false, false, false);
			keymap [TermInfoStrings.KeyA1] = new ConsoleKeyInfo ('\0', ConsoleKey.NumPad7, false, false, false);
			keymap [TermInfoStrings.KeyA3] = new ConsoleKeyInfo ('\0', ConsoleKey.NumPad9, false, false, false);
			keymap [TermInfoStrings.KeyB2] = new ConsoleKeyInfo ('\0', ConsoleKey.NumPad5, false, false, false);
			keymap [TermInfoStrings.KeyC1] = new ConsoleKeyInfo ('\0', ConsoleKey.NumPad1, false, false, false);
			keymap [TermInfoStrings.KeyC3] = new ConsoleKeyInfo ('\0', ConsoleKey.NumPad3, false, false, false);
			keymap [TermInfoStrings.KeyBtab] = new ConsoleKeyInfo ('\0', ConsoleKey.Tab, true, false, false);
			keymap [TermInfoStrings.KeyBeg] = new ConsoleKeyInfo ('\0', ConsoleKey.Home, false, false, false);
			keymap [TermInfoStrings.KeyCopy] = new ConsoleKeyInfo ('C', ConsoleKey.C, false, true, false);
			keymap [TermInfoStrings.KeyEnd] = new ConsoleKeyInfo ('\0', ConsoleKey.End, false, false, false);
			keymap [TermInfoStrings.KeyEnter] = new ConsoleKeyInfo ('\n', ConsoleKey.Enter, false, false, false);
			keymap [TermInfoStrings.KeyHelp] = new ConsoleKeyInfo ('\0', ConsoleKey.Help, false, false, false);
			keymap [TermInfoStrings.KeyPrint] = new ConsoleKeyInfo ('\0', ConsoleKey.Print, false, false, false);
			keymap [TermInfoStrings.KeyUndo] = new ConsoleKeyInfo ('Z', ConsoleKey.Z , false, true, false);
			keymap [TermInfoStrings.KeySbeg] = new ConsoleKeyInfo ('\0', ConsoleKey.Home, true, false, false);
			keymap [TermInfoStrings.KeyScopy] = new ConsoleKeyInfo ('C', ConsoleKey.C , true, true, false);
			keymap [TermInfoStrings.KeySdc] = new ConsoleKeyInfo ('\x9', ConsoleKey.Delete, true, false, false);
			keymap [TermInfoStrings.KeyShelp] = new ConsoleKeyInfo ('\0', ConsoleKey.Help, true, false, false);
			keymap [TermInfoStrings.KeyShome] = new ConsoleKeyInfo ('\0', ConsoleKey.Home, true, false, false);
			keymap [TermInfoStrings.KeySleft] = new ConsoleKeyInfo ('\0', ConsoleKey.LeftArrow, true, false, false);
			keymap [TermInfoStrings.KeySprint] = new ConsoleKeyInfo ('\0', ConsoleKey.Print, true, false, false);
			keymap [TermInfoStrings.KeySright] = new ConsoleKeyInfo ('\0', ConsoleKey.RightArrow, true, false, false);
			keymap [TermInfoStrings.KeySundo] = new ConsoleKeyInfo ('Z', ConsoleKey.Z, true, false, false);
			keymap [TermInfoStrings.KeyF11] = new ConsoleKeyInfo ('\0', ConsoleKey.F11, false, false, false);
			keymap [TermInfoStrings.KeyF12] = new ConsoleKeyInfo ('\0', ConsoleKey.F12 , false, false, false);
			keymap [TermInfoStrings.KeyF13] = new ConsoleKeyInfo ('\0', ConsoleKey.F13, false, false, false);
			keymap [TermInfoStrings.KeyF14] = new ConsoleKeyInfo ('\0', ConsoleKey.F14, false, false, false);
			keymap [TermInfoStrings.KeyF15] = new ConsoleKeyInfo ('\0', ConsoleKey.F15, false, false, false);
			keymap [TermInfoStrings.KeyF16] = new ConsoleKeyInfo ('\0', ConsoleKey.F16, false, false, false);
			keymap [TermInfoStrings.KeyF17] = new ConsoleKeyInfo ('\0', ConsoleKey.F17, false, false, false);
			keymap [TermInfoStrings.KeyF18] = new ConsoleKeyInfo ('\0', ConsoleKey.F18, false, false, false);
			keymap [TermInfoStrings.KeyF19] = new ConsoleKeyInfo ('\0', ConsoleKey.F19, false, false, false);
			keymap [TermInfoStrings.KeyF20] = new ConsoleKeyInfo ('\0', ConsoleKey.F20, false, false, false);
			keymap [TermInfoStrings.KeyF21] = new ConsoleKeyInfo ('\0', ConsoleKey.F21, false, false, false);
			keymap [TermInfoStrings.KeyF22] = new ConsoleKeyInfo ('\0', ConsoleKey.F22, false, false, false);
			keymap [TermInfoStrings.KeyF23] = new ConsoleKeyInfo ('\0', ConsoleKey.F23, false, false, false);
			keymap [TermInfoStrings.KeyF24] = new ConsoleKeyInfo ('\0', ConsoleKey.F24, false, false, false);
		}

		void InitKeys ()
		{
			if (initKeys)
				return;

			CreateKeyMap ();
			rootmap = new ByteMatcher ();
			AddStringMapping (TermInfoStrings.KeyBackspace);
			AddStringMapping (TermInfoStrings.KeyClear);
			AddStringMapping (TermInfoStrings.KeyDown);
			AddStringMapping (TermInfoStrings.KeyF1);
			AddStringMapping (TermInfoStrings.KeyF10);
			AddStringMapping (TermInfoStrings.KeyF2);
			AddStringMapping (TermInfoStrings.KeyF3);
			AddStringMapping (TermInfoStrings.KeyF4);
			AddStringMapping (TermInfoStrings.KeyF5);
			AddStringMapping (TermInfoStrings.KeyF6);
			AddStringMapping (TermInfoStrings.KeyF7);
			AddStringMapping (TermInfoStrings.KeyF8);
			AddStringMapping (TermInfoStrings.KeyF9);
			AddStringMapping (TermInfoStrings.KeyHome);
			AddStringMapping (TermInfoStrings.KeyLeft);
			AddStringMapping (TermInfoStrings.KeyLl);
			AddStringMapping (TermInfoStrings.KeyNpage);
			AddStringMapping (TermInfoStrings.KeyPpage);
			AddStringMapping (TermInfoStrings.KeyRight);
			AddStringMapping (TermInfoStrings.KeySf);
			AddStringMapping (TermInfoStrings.KeySr);
			AddStringMapping (TermInfoStrings.KeyUp);
			AddStringMapping (TermInfoStrings.KeyA1);
			AddStringMapping (TermInfoStrings.KeyA3);
			AddStringMapping (TermInfoStrings.KeyB2);
			AddStringMapping (TermInfoStrings.KeyC1);
			AddStringMapping (TermInfoStrings.KeyC3);
			AddStringMapping (TermInfoStrings.KeyBtab);
			AddStringMapping (TermInfoStrings.KeyBeg);
			AddStringMapping (TermInfoStrings.KeyCopy);
			AddStringMapping (TermInfoStrings.KeyEnd);
			AddStringMapping (TermInfoStrings.KeyEnter);
			AddStringMapping (TermInfoStrings.KeyHelp);
			AddStringMapping (TermInfoStrings.KeyPrint);
			AddStringMapping (TermInfoStrings.KeyUndo);
			AddStringMapping (TermInfoStrings.KeySbeg);
			AddStringMapping (TermInfoStrings.KeyScopy);
			AddStringMapping (TermInfoStrings.KeySdc);
			AddStringMapping (TermInfoStrings.KeyShelp);
			AddStringMapping (TermInfoStrings.KeyShome);
			AddStringMapping (TermInfoStrings.KeySleft);
			AddStringMapping (TermInfoStrings.KeySprint);
			AddStringMapping (TermInfoStrings.KeySright);
			AddStringMapping (TermInfoStrings.KeySundo);
			AddStringMapping (TermInfoStrings.KeyF11);
			AddStringMapping (TermInfoStrings.KeyF12);
			AddStringMapping (TermInfoStrings.KeyF13);
			AddStringMapping (TermInfoStrings.KeyF14);
			AddStringMapping (TermInfoStrings.KeyF15);
			AddStringMapping (TermInfoStrings.KeyF16);
			AddStringMapping (TermInfoStrings.KeyF17);
			AddStringMapping (TermInfoStrings.KeyF18);
			AddStringMapping (TermInfoStrings.KeyF19);
			AddStringMapping (TermInfoStrings.KeyF20);
			AddStringMapping (TermInfoStrings.KeyF21);
			AddStringMapping (TermInfoStrings.KeyF22);
			AddStringMapping (TermInfoStrings.KeyF23);
			AddStringMapping (TermInfoStrings.KeyF24);
			rootmap.Sort ();
			initKeys = true;
		}

		void AddStringMapping (TermInfoStrings s)
		{
			byte [] bytes = reader.GetStringBytes (s);
			if (bytes == null)
				return;

			rootmap.AddMapping (s, bytes);
		}
	}

	class ByteMatcher {
		Hashtable map = new Hashtable ();
		Hashtable starts = new Hashtable ();

		public void AddMapping (TermInfoStrings key, byte [] val)
		{
			if (val.Length == 0)
				return;

			map [val] = key;
			starts [(int) val [0]] = true;
		}

		public void Sort ()
		{
		}

		public bool StartsWith (int c)
		{
			return (starts [c] != null);
		}

		public TermInfoStrings Match (byte [] buffer, int offset, int length, out int used)
		{
			foreach (byte [] bytes in map.Keys) {
				for (int i = 0; i < bytes.Length && i < length; i++) {
					if (bytes [i] != buffer [offset + i])
						break;

					if (bytes.Length - 1 == i) {
						used = bytes.Length;
						return (TermInfoStrings) map [bytes];
					}
				}
			}

			used = 0;
			return (TermInfoStrings) (-1);
		}
	}
}
#endif

