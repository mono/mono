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
#if !NET_2_1

//
// Defining this writes the output to console.log
//#define DEBUG

using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
namespace System {
	class TermInfoDriver : IConsoleDriver {

		// This points to a variable that is updated by unmanage code on window size changes.
		unsafe static int *native_terminal_size;

		// The current size that we believe we have
		static int terminal_size;
		
		//static uint flag = 0xdeadbeef;
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
		StreamReader stdin;
		CStreamWriter stdout;

		int windowWidth;
		int windowHeight;
		//int windowTop;
		//int windowLeft;
		int bufferHeight;
		int bufferWidth;

		char [] buffer;
		int readpos;
		int writepos;
		string keypadXmit, keypadLocal;
		bool controlCAsInput;
		bool inited;
		object initLock = new object ();
		bool initKeys;
		string origPair;
		string origColors;
		string cursorAddress;
		ConsoleColor fgcolor = ConsoleColor.White;
		ConsoleColor bgcolor = ConsoleColor.Black;
		bool color16 = false; // terminal supports 16 colors
		string setlfgcolor;
		string setlbgcolor;
		string setfgcolor;
		string setbgcolor;
		bool noGetPosition;
		Hashtable keymap;
		ByteMatcher rootmap;
		bool home_1_1; // if true, we have to add 1 to x and y when using cursorAddress
		int rl_startx = -1, rl_starty = -1;
		byte [] control_characters; // Indexed by ControlCharacters.XXXXXX
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

		void WriteConsole (string str)
		{
			if (str == null)
				return;
			
			stdout.InternalWriteString (str);
		}

		public TermInfoDriver ()
			: this (Environment.GetEnvironmentVariable ("TERM"))
		{
		}

		public TermInfoDriver (string term)
		{
#if DEBUG
			File.Delete ("console.log");
			logger = new StreamWriter (File.OpenWrite ("console.log"));
#endif
			this.term = term;

			if (term == "xterm") {
				reader = new TermInfoReader (term, KnownTerminals.xterm);
				color16 = true;
			} else if (term == "linux") {
				reader = new TermInfoReader (term, KnownTerminals.linux);
				color16 = true;
			} else {
				string filename = SearchTerminfo (term);
				if (filename != null)
					reader = new TermInfoReader (term, filename);
			}

			if (reader == null)
				reader = new TermInfoReader (term, KnownTerminals.ansi);

			if (!(Console.stdout is CStreamWriter)) {
				// Application set its own stdout, we need a reference to the real stdout
				stdout = new CStreamWriter (Console.OpenStandardOutput (0), Console.OutputEncoding);
				((StreamWriter) stdout).AutoFlush = true;
			} else {
				stdout = (CStreamWriter) Console.stdout;
			}
		}

		public bool Initialized {
			get { return inited; }
		}

		public void Init ()
		{
			if (inited)
				return;

			lock (initLock){
				if (inited)
					return;
				inited = true;
				
				/* This should not happen any more, since it is checked for in Console */
				if (!ConsoleDriver.IsConsole)
					throw new IOException ("Not a tty.");
				
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
				setfgcolor = MangleParameters (reader.Get (TermInfoStrings.SetAForeground));
				setbgcolor = MangleParameters (reader.Get (TermInfoStrings.SetABackground));
				// lighter fg colours are 90 -> 97 rather than 30 -> 37
				setlfgcolor = color16 ? setfgcolor.Replace ("[3", "[9") : setfgcolor;
				// lighter bg colours are 100 -> 107 rather than 40 -> 47
				setlbgcolor = color16 ? setbgcolor.Replace ("[4", "[10") : setbgcolor;
				string resetColors = (origColors == null) ? origPair : origColors;
				if (resetColors != null)
					endString += resetColors;
				
				unsafe {
					if (!ConsoleDriver.TtySetup (keypadXmit, endString, out control_characters, out native_terminal_size)){
						control_characters = new byte [17];
						native_terminal_size = -1;
						//throw new IOException ("Error initializing terminal.");
					}
				}
				
				stdin = new StreamReader (Console.OpenStandardInput (0), Console.InputEncoding);
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

		static int TranslateColor (ConsoleColor desired, out bool light)
		{
			switch (desired) {
			// Dark colours
			case ConsoleColor.Black:
				light = false;
				return 0;
			case ConsoleColor.DarkRed:
				light = false;
				return 1;
			case ConsoleColor.DarkGreen:
				light = false;
				return 2;
			case ConsoleColor.DarkYellow:
				light = false;
				return 3;
			case ConsoleColor.DarkBlue:
				light = false;
				return 4;
			case ConsoleColor.DarkMagenta:
				light = false;
				return 5;
			case ConsoleColor.DarkCyan:
				light = false;
				return 6;
			case ConsoleColor.Gray:
				light = false;
				return 7;
			// Light colours
			case ConsoleColor.DarkGray:
				light = true;
				return 0;
			case ConsoleColor.Red:
				light = true;
				return 1;
			case ConsoleColor.Green:
				light = true;
				return 2;
			case ConsoleColor.Yellow:
				light = true;
				return 3;
			case ConsoleColor.Blue:
				light = true;
				return 4;
			case ConsoleColor.Magenta:
				light = true;
				return 5;
			case ConsoleColor.Cyan:
				light = true;
				return 6;
			case ConsoleColor.White:
				light = true;
				return 7;
			}

			light = false;

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
				for (int i = 0; i < n; i++){
					IncrementX ();
				}
				WriteConsole ("\t");
				break;
			case ConsoleKey.Clear:
				WriteConsole (clear);
				cursorLeft = 0;
				cursorTop = 0;
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
			WriteSpecialKey (CreateKeyInfoFromInt (c, false));
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
			return IsSpecialKey (CreateKeyInfoFromInt (c, false));
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

				bool light;
				int colour = TranslateColor (value, out light);

				if (light)
					WriteConsole (String.Format (setlbgcolor, colour));
				else
					WriteConsole (String.Format (setbgcolor, colour));
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

				bool light;
				int colour = TranslateColor (value, out light);

				if (light)
					WriteConsole (String.Format (setlfgcolor, colour));
				else
					WriteConsole (String.Format (setfgcolor, colour));
			}
		}

		void GetCursorPosition ()
		{
			int row = 0, col = 0;
			int b;

			// First, get any data in the input buffer.  Merely reduces the likelyhood of getting an error
			int inqueue = ConsoleDriver.InternalKeyAvailable (0);
			while (inqueue-- > 0){
				b = stdin.Read ();
				AddToBuffer (b);
			}

			// Then try to probe for the cursor coordinates
			WriteConsole ("\x1b[6n");
			if (ConsoleDriver.InternalKeyAvailable (1000) <= 0) {
				noGetPosition = true;
				return;
			}

			b = stdin.Read ();
			while (b != '\x1b') {
				AddToBuffer (b);
				if (ConsoleDriver.InternalKeyAvailable (100) <= 0)
					return;
				b = stdin.Read ();
			}

			b = stdin.Read ();
			if (b != '[') {
				AddToBuffer ('\x1b');
				AddToBuffer (b);
				return;
			}

			b = stdin.Read ();
			if (b != ';') {
				row = b - '0';
				b = stdin.Read ();
				while ((b >= '0') && (b <= '9')) {
					row = row * 10 + b - '0';
					b = stdin.Read ();
				}
				// Row/col is 0 based
				row --;
			}

			b = stdin.Read ();
			if (b != 'R') {
				col = b - '0';
				b = stdin.Read ();
				while ((b >= '0') && (b <= '9')) {
					col = col * 10 + b - '0';
					b = stdin.Read ();
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
				return false;
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

				return false;
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

		//
		// Requries that caller calls Init () if not !inited.
		//
		unsafe void CheckWindowDimensions ()
		{
			if (native_terminal_size == null || terminal_size == *native_terminal_size)
				return;

			if (*native_terminal_size == -1){
				int c = reader.Get (TermInfoNumbers.Columns);
				if (c != 0)
					windowWidth = c;
				
				c = reader.Get (TermInfoNumbers.Lines);
				if (c != 0)
					windowHeight = c;
			} else {
				terminal_size = *native_terminal_size;
				windowWidth = terminal_size >> 16;
				windowHeight = terminal_size & 0xffff;
			}
			bufferHeight = windowHeight;
			bufferWidth = windowWidth;
		}

		
		public int WindowHeight {
			get {
				if (!inited) {
					Init ();
				}

				CheckWindowDimensions ();
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

				//CheckWindowDimensions ();
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

				//CheckWindowDimensions ();
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

				CheckWindowDimensions ();
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
			cursorLeft = 0;
			cursorTop = 0;
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
				buffer = new char [1024];
			} else if (writepos >= buffer.Length) {
				char [] newbuf = new char [buffer.Length * 2];
				Buffer.BlockCopy (buffer, 0, newbuf, 0, buffer.Length);
				buffer = newbuf;
			}

			buffer [writepos++] = (char) b;
		}

		void AdjustBuffer ()
		{
			if (readpos >= writepos) {
				readpos = writepos = 0;
			}
		}

		ConsoleKeyInfo CreateKeyInfoFromInt (int n, bool alt)
		{
			char c = (char) n;
			ConsoleKey key = (ConsoleKey)n;
			bool shift = false;
			bool ctrl = false;

			switch (n){
			case 10:
				key = ConsoleKey.Enter;
				break;
			case 0x20:
				key = ConsoleKey.Spacebar;
				break;
			case 45:
				key = ConsoleKey.Subtract;
				break;
			case 43:
				key = ConsoleKey.Add;
				break;
			case 47:
				key = ConsoleKey.Divide;
				break;
			case 42:
				key = ConsoleKey.Multiply;
				break;
			case 8: case 9: case 12: case 13: case 19:
				/* Values in ConsoleKey */
				break;
			case 27:
				key = ConsoleKey.Escape;
				break;
				
			default:
				if (n >= 1 && n <= 26) {
					// For Ctrl-a to Ctrl-z.
					ctrl = true;
					key = ConsoleKey.A + n - 1;
				} else if (n >= 'a' && n <= 'z') {
					key = ConsoleKey.A - 'a' + n;
				} else if (n >= 'A' && n <= 'Z') {
					shift = true;
				} else if (n >= '0' && n <= '9') {
				} else
					key = 0;
				break;
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
				return CreateKeyInfoFromInt (next, false);
			}

			int used;
			TermInfoStrings str = rootmap.Match (buffer, readpos, writepos - readpos, out used);
			if ((int) str == -1){
				// Escape sequences: alt keys are sent as ESC-key
				if (buffer [readpos] == 27 && (writepos - readpos) >= 2){
					readpos += 2;
					AdjustBuffer ();
					if (buffer [readpos+1] == 127)
						return new ConsoleKeyInfo ((char)8, ConsoleKey.Backspace, false, true, false);
					return CreateKeyInfoFromInt (buffer [readpos+1], true);
				} else
					return null;
			}

			ConsoleKeyInfo key;
			if (keymap [str] != null) {
				key = (ConsoleKeyInfo) keymap [str];
			} else {
				readpos++;
				AdjustBuffer ();
				return CreateKeyInfoFromInt (next, false);
			}

			readpos += used;
			AdjustBuffer ();
			return key;
		}

		ConsoleKeyInfo ReadKeyInternal (out bool fresh)
		{
			if (!inited)
				Init ();

			InitKeys ();

			object o;

			if ((o = GetKeyFromBuffer (true)) == null) {
				do {
					if (ConsoleDriver.InternalKeyAvailable (150) > 0) {
						do {
							AddToBuffer (stdin.Read ());
						} while (ConsoleDriver.InternalKeyAvailable (0) > 0);
					} else if (stdin.DataAvailable ()) {
						do {
							AddToBuffer (stdin.Read ());
						} while (stdin.DataAvailable ());
					} else {
						if ((o = GetKeyFromBuffer (false)) != null)
							break;

						AddToBuffer (stdin.Read ());
					}
					
					o = GetKeyFromBuffer (true);
				} while (o == null);

				// freshly read character
				fresh = true;
			} else {
				// this char was pre-buffered (e.g. not fresh)
				fresh = false;
			}

			return (ConsoleKeyInfo) o;
		}

#region Input echoing optimization
		bool InputPending ()
		{
			// check if we've got pending input we can read immediately
			return readpos < writepos || stdin.DataAvailable ();
		}

		char [] echobuf = null;
		int echon = 0;

		// Queues a character to be echo'd back to the console
		void QueueEcho (char c)
		{
			if (echobuf == null)
				echobuf = new char [1024];

			echobuf[echon++] = c;

			if (echon == echobuf.Length || !InputPending ()) {
				// blit our echo buffer to the console
				stdout.InternalWriteChars (echobuf, echon);
				echon = 0;
			}
		}

		// Queues a key to be echo'd back to the console
		void Echo (ConsoleKeyInfo key)
		{
			if (!IsSpecialKey (key)) {
				QueueEcho (key.KeyChar);
				return;
			}

			// flush pending echo's
			EchoFlush ();

			WriteSpecialKey (key);
		}

		// Flush the pending echo queue
		void EchoFlush ()
		{
			if (echon == 0)
				return;

			// flush our echo buffer to the console
			stdout.InternalWriteChars (echobuf, echon);
			echon = 0;
		}
#endregion

		public int Read ([In, Out] char [] dest, int index, int count)
		{
			bool fresh, echo = false;
			StringBuilder sbuf;
			ConsoleKeyInfo key;
			int BoL = 0;  // Beginning-of-Line marker (can't backspace beyond this)
			object o;
			char c;

			sbuf = new StringBuilder ();

			// consume buffered keys first (do not echo, these have already been echo'd)
			while (true) {
				if ((o = GetKeyFromBuffer (true)) == null)
					break;

				key = (ConsoleKeyInfo) o;
				c = key.KeyChar;

				if (key.Key != ConsoleKey.Backspace) {
					if (key.Key == ConsoleKey.Enter)
						BoL = sbuf.Length;

					sbuf.Append (c);
				} else if (sbuf.Length > BoL) {
					sbuf.Length--;
				}
			}

			// continue reading until Enter is hit
			rl_startx = cursorLeft;
			rl_starty = cursorTop;

			do {
				key = ReadKeyInternal (out fresh);
				echo = echo || fresh;
				c = key.KeyChar;

				if (key.Key != ConsoleKey.Backspace) {
					if (key.Key == ConsoleKey.Enter)
						BoL = sbuf.Length;

					sbuf.Append (c);
				} else if (sbuf.Length > BoL) {
					sbuf.Length--;
				} else {
					continue;
				}

				// echo fresh keys back to the console
				if (echo)
					Echo (key);
			} while (key.Key != ConsoleKey.Enter);

			EchoFlush ();

			rl_startx = -1;
			rl_starty = -1;

			// copy up to count chars into dest
			int nread = 0;
			while (count > 0 && nread < sbuf.Length) {
				dest[index + nread] = sbuf[nread];
				nread++;
				count--;
			}

			// put the rest back into our key buffer
			for (int i = nread; i < sbuf.Length; i++)
				AddToBuffer (sbuf[i]);

			return nread;
		}

		public ConsoleKeyInfo ReadKey (bool intercept)
		{
			bool fresh;

			ConsoleKeyInfo key = ReadKeyInternal (out fresh);

			if (!intercept && fresh) {
				// echo the fresh key back to the console
				Echo (key);
				EchoFlush ();
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
			bool fresh, echo = false;
			ConsoleKeyInfo key;
			char c;

			rl_startx = cursorLeft;
			rl_starty = cursorTop;
			char eof = (char) control_characters [ControlCharacters.EOF];

			do {
				key = ReadKeyInternal (out fresh);
				echo = echo || fresh;
				c = key.KeyChar;
				// EOF -> Ctrl-D (EOT) pressed.
				if (c == eof && c != 0 && builder.Length == 0)
					return null;

				if (key.Key != ConsoleKey.Enter) {
					if (key.Key != ConsoleKey.Backspace) {
						builder.Append (c);
					} else if (builder.Length > 0) {
						builder.Length--;
					} else {
						// skips over echoing the key to the console
						continue;
					}
				}

				// echo fresh keys back to the console
				if (echo)
					Echo (key);
			} while (key.Key != ConsoleKey.Enter);

			EchoFlush ();

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

			CheckWindowDimensions ();
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
			// These were previously missing:
			keymap [TermInfoStrings.KeyDc] = new ConsoleKeyInfo ('\0', ConsoleKey.Delete, false, false, false);
			keymap [TermInfoStrings.KeyIc] = new ConsoleKeyInfo ('\0', ConsoleKey.Insert, false, false, false);
		}

		//
		// The keys that we know about and use
		//
		static TermInfoStrings [] UsedKeys = {
			TermInfoStrings.KeyBackspace,
			TermInfoStrings.KeyClear,
			TermInfoStrings.KeyDown,
			TermInfoStrings.KeyF1,
			TermInfoStrings.KeyF10,
			TermInfoStrings.KeyF2,
			TermInfoStrings.KeyF3,
			TermInfoStrings.KeyF4,
			TermInfoStrings.KeyF5,
			TermInfoStrings.KeyF6,
			TermInfoStrings.KeyF7,
			TermInfoStrings.KeyF8,
			TermInfoStrings.KeyF9,
			TermInfoStrings.KeyHome,
			TermInfoStrings.KeyLeft,
			TermInfoStrings.KeyLl,
			TermInfoStrings.KeyNpage,
			TermInfoStrings.KeyPpage,
			TermInfoStrings.KeyRight,
			TermInfoStrings.KeySf,
			TermInfoStrings.KeySr,
			TermInfoStrings.KeyUp,
			TermInfoStrings.KeyA1,
			TermInfoStrings.KeyA3,
			TermInfoStrings.KeyB2,
			TermInfoStrings.KeyC1,
			TermInfoStrings.KeyC3,
			TermInfoStrings.KeyBtab,
			TermInfoStrings.KeyBeg,
			TermInfoStrings.KeyCopy,
			TermInfoStrings.KeyEnd,
			TermInfoStrings.KeyEnter,
			TermInfoStrings.KeyHelp,
			TermInfoStrings.KeyPrint,
			TermInfoStrings.KeyUndo,
			TermInfoStrings.KeySbeg,
			TermInfoStrings.KeyScopy,
			TermInfoStrings.KeySdc,
			TermInfoStrings.KeyShelp,
			TermInfoStrings.KeyShome,
			TermInfoStrings.KeySleft,
			TermInfoStrings.KeySprint,
			TermInfoStrings.KeySright,
			TermInfoStrings.KeySundo,
			TermInfoStrings.KeyF11,
			TermInfoStrings.KeyF12,
			TermInfoStrings.KeyF13,
			TermInfoStrings.KeyF14,
			TermInfoStrings.KeyF15,
			TermInfoStrings.KeyF16,
			TermInfoStrings.KeyF17,
			TermInfoStrings.KeyF18,
			TermInfoStrings.KeyF19,
			TermInfoStrings.KeyF20,
			TermInfoStrings.KeyF21,
			TermInfoStrings.KeyF22,
			TermInfoStrings.KeyF23,
			TermInfoStrings.KeyF24,

			// These were missing
			TermInfoStrings.KeyDc,
			TermInfoStrings.KeyIc
		};
		
		void InitKeys ()
		{
			if (initKeys)
				return;

			CreateKeyMap ();
			rootmap = new ByteMatcher ();

			foreach (TermInfoStrings tis in UsedKeys)
				AddStringMapping (tis);
			
			rootmap.AddMapping (TermInfoStrings.KeyBackspace, new byte [] { control_characters [ControlCharacters.Erase] });
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

		public TermInfoStrings Match (char [] buffer, int offset, int length, out int used)
		{
			foreach (byte [] bytes in map.Keys) {
				for (int i = 0; i < bytes.Length && i < length; i++) {
					if ((char) bytes [i] != buffer [offset + i])
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

