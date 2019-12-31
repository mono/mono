//
// System.ConsoleDriver
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005,2006 Novell, Inc (http://www.novell.com)
// Copyright (c) Microsoft.
// Copyright 2014 Xamarin Inc
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
// This code contains the ParameterizedStrings implementation from .NET's
// Core System.Console:
// https://github.com/dotnet/corefx
// src/System.Console/src/System/ConsolePal.Unix.cs
//
#if MONO_FEATURE_CONSOLE

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
		readonly static string [] locations = { "/usr/share/terminfo", "/etc/terminfo", "/usr/lib/terminfo", "/lib/terminfo" };

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
		string setfgcolor;
		string setbgcolor;
		int maxColors;
		bool noGetPosition;
		Hashtable keymap;
		ByteMatcher rootmap;
		int rl_startx = -1, rl_starty = -1;
		byte [] control_characters; // Indexed by ControlCharacters.XXXXXX
#if DEBUG
		StreamWriter logger;
#endif

		static string TryTermInfoDir (string dir, string term )
		{
			string path = String.Format ("{0}/{1:x}/{2}", dir, (int)(term [0]), term);
			if (File.Exists (path))
				return path;
				
			path = Path.Combine (dir, term.Substring (0, 1), term);
			if (File.Exists (path))
				return path;
			return null;
		}

		static string SearchTerminfo (string term)
		{
			if (term == null || term == String.Empty)
				return null;

			string path;
			string terminfo = Environment.GetEnvironmentVariable ("TERMINFO");
			if (terminfo != null && Directory.Exists (terminfo)){
				path = TryTermInfoDir (terminfo, term);
				if (path != null)
					return path;
			}
				    
			foreach (string dir in locations) {
				if (!Directory.Exists (dir))
					continue;

				path = TryTermInfoDir (dir, term);
				if (path != null)
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

			string filename = SearchTerminfo (term);
			if (filename != null)
				reader = new TermInfoReader (term, filename);
			else {
				// fallbacks
				if (term == "xterm") {
					reader = new TermInfoReader (term, KnownTerminals.xterm);
				} else if (term == "linux") {
					reader = new TermInfoReader (term, KnownTerminals.linux);
				}
			}

			if (reader == null)
				reader = new TermInfoReader (term, KnownTerminals.ansi);

			if (!(Console.stdout is CStreamWriter)) {
				// Application set its own stdout, we need a reference to the real stdout
				stdout = new CStreamWriter (Console.OpenStandardOutput (0), Console.OutputEncoding, false);
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
				
				try {
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
					setfgcolor = reader.Get (TermInfoStrings.SetAForeground);
					setbgcolor = reader.Get (TermInfoStrings.SetABackground);
					maxColors = reader.Get (TermInfoNumbers.MaxColors);
					maxColors = Math.Max (Math.Min (maxColors, 16), 1);
					
					string resetColors = (origColors == null) ? origPair : origColors;
					if (resetColors != null)
						endString += resetColors;
					
					unsafe {
						if (!ConsoleDriver.TtySetup (keypadXmit, endString, out control_characters, out native_terminal_size)){
							control_characters = new byte [17];
							native_terminal_size = null;
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

				} finally {
					inited = true;
				}

			}
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

		/// <summary>
		/// The values of the ConsoleColor enums unfortunately don't map to the 
		/// corresponding ANSI values.  We need to do the mapping manually.
		/// See http://en.wikipedia.org/wiki/ANSI_escape_code#Colors
		/// </summary>
		private static readonly int[] _consoleColorToAnsiCode = new int[]
		{
			// Dark/Normal colors
			0, // Black,
			4, // DarkBlue,
			2, // DarkGreen,
			6, // DarkCyan,
			1, // DarkRed,
			5, // DarkMagenta,
			3, // DarkYellow,
			7, // Gray,
	
			// Bright colors
			8,  // DarkGray,
			12, // Blue,
			10, // Green,
			14, // Cyan,
			9,  // Red,
			13, // Magenta,
			11, // Yellow,
			15  // White
		};

		void ChangeColor (string format, ConsoleColor color)
		{
			if (String.IsNullOrEmpty (format))
				// the terminal doesn't support colors
				return;

			int ccValue = (int)color;
			if ((ccValue & ~0xF) != 0)
				throw new ArgumentException("Invalid Console Color");

			int ansiCode = _consoleColorToAnsiCode[ccValue] % maxColors;

			WriteConsole (ParameterizedStrings.Evaluate (format, ansiCode));
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
				ChangeColor (setbgcolor, value);
				bgcolor = value;
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
				ChangeColor (setfgcolor, value);
				fgcolor = value;
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

				CheckWindowDimensions ();
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

				CheckWindowDimensions ();
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
			return ReadUntilConditionInternal (true);
 		}

		public string ReadToEnd ()
 		{
			return ReadUntilConditionInternal (false);
 		}

		private string ReadUntilConditionInternal (bool haltOnNewLine)
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

			bool treatAsEnterKey;

			do {
				key = ReadKeyInternal (out fresh);
				echo = echo || fresh;
				c = key.KeyChar;
				// EOF -> Ctrl-D (EOT) pressed.
				if (c == eof && c != 0 && builder.Length == 0)
					return null;

				treatAsEnterKey = haltOnNewLine && (key.Key == ConsoleKey.Enter);

				if (!treatAsEnterKey) {
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
			} while (!treatAsEnterKey);

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

			WriteConsole (ParameterizedStrings.Evaluate (cursorAddress, top, left));
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

		void InitKeys ()
		{
			if (initKeys)
				return;

			CreateKeyMap ();
			rootmap = new ByteMatcher ();

			//
			// The keys that we know about and use
			//
			var UsedKeys = new [] {
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

	/// <summary>Provides support for evaluating parameterized terminfo database format strings.</summary>
	internal static class ParameterizedStrings
	{
                /// <summary>A cached stack to use to avoid allocating a new stack object for every evaluation.</summary>
                [ThreadStatic]
                private static LowLevelStack _cachedStack;

                /// <summary>Evaluates a terminfo formatting string, using the supplied arguments.</summary>
                /// <param name="format">The format string.</param>
                /// <param name="args">The arguments to the format string.</param>
                /// <returns>The formatted string.</returns>
                public static string Evaluate(string format, params FormatParam[] args)
		{
			if (format == null)
				throw new ArgumentNullException("format");
			if (args == null)
				throw new ArgumentNullException("args");

			// Initialize the stack to use for processing.
			LowLevelStack stack = _cachedStack;
			if (stack == null)
				_cachedStack = stack = new LowLevelStack();
			else
				stack.Clear();

			// "dynamic" and "static" variables are much less often used (the "dynamic" and "static"
			// terminology appears to just refer to two different collections rather than to any semantic
			// meaning).  As such, we'll only initialize them if we really need them.
			FormatParam[] dynamicVars = null, staticVars = null;
			
			int pos = 0;
			return EvaluateInternal(format, ref pos, args, stack, ref dynamicVars, ref staticVars);
			
			// EvaluateInternal may throw IndexOutOfRangeException and InvalidOperationException
			// if the format string is malformed or if it's inconsistent with the parameters provided.
		}
		
                /// <summary>Evaluates a terminfo formatting string, using the supplied arguments and processing data structures.</summary>
                /// <param name="format">The format string.</param>
                /// <param name="pos">The position in <paramref name="format"/> to start processing.</param>
                /// <param name="args">The arguments to the format string.</param>
                /// <param name="stack">The stack to use as the format string is evaluated.</param>
                /// <param name="dynamicVars">A lazily-initialized collection of variables.</param>
                /// <param name="staticVars">A lazily-initialized collection of variables.</param>
                /// <returns>
                /// The formatted string; this may be empty if the evaluation didn't yield any output.
                /// The evaluation stack will have a 1 at the top if all processing was completed at invoked level
                /// of recursion, and a 0 at the top if we're still inside of a conditional that requires more processing.
                /// </returns>
                private static string EvaluateInternal(
			string format, ref int pos, FormatParam[] args, LowLevelStack stack,
			ref FormatParam[] dynamicVars, ref FormatParam[] staticVars)
		{
			// Create a StringBuilder to store the output of this processing.  We use the format's length as an 
			// approximation of an upper-bound for how large the output will be, though with parameter processing,
			// this is just an estimate, sometimes way over, sometimes under.
			StringBuilder output = new StringBuilder(format.Length);

			// Format strings support conditionals, including the equivalent of "if ... then ..." and
			// "if ... then ... else ...", as well as "if ... then ... else ... then ..."
			// and so on, where an else clause can not only be evaluated for string output but also
			// as a conditional used to determine whether to evaluate a subsequent then clause.
			// We use recursion to process these subsequent parts, and we track whether we're processing
			// at the same level of the initial if clause (or whether we're nested).
			bool sawIfConditional = false;

			// Process each character in the format string, starting from the position passed in.
			for (; pos < format.Length; pos++){
				// '%' is the escape character for a special sequence to be evaluated.
				// Anything else just gets pushed to output.
				if (format[pos] != '%') {
					output.Append(format[pos]);
					continue;
				}
				// We have a special parameter sequence to process.  Now we need
				// to look at what comes after the '%'.
				++pos;
				switch (format[pos]) {
				// Output appending operations
				case '%': // Output the escaped '%'
					output.Append('%');
					break;
				case 'c': // Pop the stack and output it as a char
					output.Append((char)stack.Pop().Int32);
					break;
				case 's': // Pop the stack and output it as a string
					output.Append(stack.Pop().String);
					break;
				case 'd': // Pop the stack and output it as an integer
					output.Append(stack.Pop().Int32);
					break;
				case 'o':
				case 'X':
				case 'x':
				case ':':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					// printf strings of the format "%[[:]flags][width[.precision]][doxXs]" are allowed
					// (with a ':' used in front of flags to help differentiate from binary operations, as flags can
					// include '-' and '+').  While above we've special-cased common usage (e.g. %d, %s),
					// for more complicated expressions we delegate to printf.
					int printfEnd = pos;
					for (; printfEnd < format.Length; printfEnd++) // find the end of the printf format string
					{
						char ec = format[printfEnd];
						if (ec == 'd' || ec == 'o' || ec == 'x' || ec == 'X' || ec == 's')
						{
							break;
						}
					}
					if (printfEnd >= format.Length)
						throw new InvalidOperationException("Terminfo database contains invalid values");
					string printfFormat = format.Substring(pos - 1, printfEnd - pos + 2); // extract the format string
					if (printfFormat.Length > 1 && printfFormat[1] == ':')
						printfFormat = printfFormat.Remove(1, 1);
					output.Append(FormatPrintF(printfFormat, stack.Pop().Object)); // do the printf formatting and append its output
					break;

					// Stack pushing operations
				case 'p': // Push the specified parameter (1-based) onto the stack
					pos++;
					stack.Push(args[format[pos] - '1']);
					break;
				case 'l': // Pop a string and push its length
					stack.Push(stack.Pop().String.Length);
					break;
				case '{': // Push integer literal, enclosed between braces
					pos++;
					int intLit = 0;
					while (format[pos] != '}')
					{
						intLit = (intLit * 10) + (format[pos] - '0');
						pos++;
					}
					stack.Push(intLit);
					break;
				case '\'': // Push literal character, enclosed between single quotes
					stack.Push((int)format[pos + 1]);
					pos += 2;
					break;

					// Storing and retrieving "static" and "dynamic" variables
				case 'P': // Pop a value and store it into either static or dynamic variables based on whether the a-z variable is capitalized
					pos++;
					int setIndex;
					FormatParam[] targetVars = GetDynamicOrStaticVariables(format[pos], ref dynamicVars, ref staticVars, out setIndex);
					targetVars[setIndex] = stack.Pop();
					break;
				case 'g': // Push a static or dynamic variable; which is based on whether the a-z variable is capitalized
					pos++;
					int getIndex;
					FormatParam[] sourceVars = GetDynamicOrStaticVariables(format[pos], ref dynamicVars, ref staticVars, out getIndex);
					stack.Push(sourceVars[getIndex]);
					break;

					// Binary operations
				case '+':
				case '-':
				case '*':
				case '/':
				case 'm':
				case '^': // arithmetic
				case '&':
				case '|':                                         // bitwise
				case '=':
				case '>':
				case '<':                               // comparison
				case 'A':
				case 'O':                                         // logical
					int second = stack.Pop().Int32; // it's a stack... the second value was pushed last
					int first = stack.Pop().Int32;
					int res;
					switch (format[pos]) {
					case '+':
						res = first + second;
						break;
					case '-':
						res = first - second;
						break;
					case '*':
						res = first * second;
						break;
					case '/':
						res = first / second;
						break;
					case 'm':
						res = first % second;
						break;
					case '^':
						res = first ^ second;
						break;
					case '&':
						res = first & second;
						break;
					case '|':
						res = first | second;
						break;
					case '=':
						res = AsInt(first == second);
						break;
					case '>':
						res = AsInt(first > second);
						break;
					case '<':
						res = AsInt(first < second);
						break;
					case 'A':
						res = AsInt(AsBool(first) && AsBool(second));
						break;
					case 'O':
						res = AsInt(AsBool(first) || AsBool(second));
						break;
					default:
						res = 0;
						break;
					}
					stack.Push(res);
					break;

					// Unary operations
				case '!':
				case '~':
					int value = stack.Pop().Int32;
					stack.Push(
						format[pos] == '!' ? AsInt(!AsBool(value)) :
						~value);
					break;

					// Augment first two parameters by 1
				case 'i':
					args[0] = 1 + args[0].Int32;
					args[1] = 1 + args[1].Int32;
					break;

					// Conditional of the form %? if-part %t then-part %e else-part %;
					// The "%e else-part" is optional.
				case '?':
					sawIfConditional = true;
					break;
				case 't':
					// We hit the end of the if-part and are about to start the then-part.
					// The if-part left its result on the stack; pop and evaluate.
					bool conditionalResult = AsBool(stack.Pop().Int32);

					// Regardless of whether it's true, run the then-part to get past it.
					// If the conditional was true, output the then results.
					pos++;
					string thenResult = EvaluateInternal(format, ref pos, args, stack, ref dynamicVars, ref staticVars);
					if (conditionalResult)
					{
						output.Append(thenResult);
					}

					// We're past the then; the top of the stack should now be a Boolean
					// indicating whether this conditional has more to be processed (an else clause).
					if (!AsBool(stack.Pop().Int32))
					{
						// Process the else clause, and if the conditional was false, output the else results.
						pos++;
						string elseResult = EvaluateInternal(format, ref pos, args, stack, ref dynamicVars, ref staticVars);
						if (!conditionalResult)
						{
							output.Append(elseResult);
						}
						// Now we should be done (any subsequent elseif logic will have bene handled in the recursive call).
						if (!AsBool(stack.Pop().Int32))
						{
							throw new InvalidOperationException("Terminfo database contains invalid values");
						}
					}

					// If we're in a nested processing, return to our parent.
					if (!sawIfConditional)
					{
						stack.Push(1);
						return output.ToString();
					}
					// Otherwise, we're done processing the conditional in its entirety.
					sawIfConditional = false;
					break;
				case 'e':
				case ';':
					// Let our caller know why we're exiting, whether due to the end of the conditional or an else branch.
					stack.Push(AsInt(format[pos] == ';'));
					return output.ToString();

					// Anything else is an error
				default:
					throw new InvalidOperationException("Terminfo database contains invalid values");
				}
			}
			stack.Push(1);
			return output.ToString();
		}
		
                /// <summary>Converts an Int32 to a Boolean, with 0 meaning false and all non-zero values meaning true.</summary>
                /// <param name="i">The integer value to convert.</param>
                /// <returns>true if the integer was non-zero; otherwise, false.</returns>
                static bool AsBool(Int32 i) { return i != 0; }

                /// <summary>Converts a Boolean to an Int32, with true meaning 1 and false meaning 0.</summary>
                /// <param name="b">The Boolean value to convert.</param>
                /// <returns>1 if the Boolean is true; otherwise, 0.</returns>
                static int AsInt(bool b) { return b ? 1 : 0; }

		static string StringFromAsciiBytes(byte[] buffer, int offset, int length)
		{
			// Special-case for empty strings
			if (length == 0)
				return string.Empty;

			// new string(sbyte*, ...) doesn't exist in the targeted reference assembly,
			// so we first copy to an array of chars, and then create a string from that.
			char[] chars = new char[length];
			for (int i = 0, j = offset; i < length; i++, j++)
				chars[i] = (char)buffer[j];
			return new string(chars);
		}

		[DllImport("libc")]
		static extern unsafe int snprintf(byte* str, IntPtr size, string format, string arg1);

		[DllImport("libc")]
		static extern unsafe int snprintf(byte* str, IntPtr size, string format, int arg1);
		
                /// <summary>Formats an argument into a printf-style format string.</summary>
                /// <param name="format">The printf-style format string.</param>
                /// <param name="arg">The argument to format.  This must be an Int32 or a String.</param>
                /// <returns>The formatted string.</returns>
                static unsafe string FormatPrintF(string format, object arg)
                {
			// Determine how much space is needed to store the formatted string.
			string stringArg = arg as string;
			int neededLength = stringArg != null ?
				snprintf(null, IntPtr.Zero, format, stringArg) :
				snprintf(null, IntPtr.Zero, format, (int)arg);
			if (neededLength == 0)
				return string.Empty;
			if (neededLength < 0)
				throw new InvalidOperationException("The printf operation failed");
			
			// Allocate the needed space, format into it, and return the data as a string.
			byte[] bytes = new byte[neededLength + 1]; // extra byte for the null terminator
			fixed (byte* ptr = bytes){
				int length = stringArg != null ?
					snprintf(ptr, (IntPtr)bytes.Length, format, stringArg) :
					snprintf(ptr, (IntPtr)bytes.Length, format, (int)arg);
				if (length != neededLength)
				{
					throw new InvalidOperationException("Invalid printf operation");
				}
			}
			return StringFromAsciiBytes(bytes, 0, neededLength);
                }
		
                /// <summary>Gets the lazily-initialized dynamic or static variables collection, based on the supplied variable name.</summary>
                /// <param name="c">The name of the variable.</param>
                /// <param name="dynamicVars">The lazily-initialized dynamic variables collection.</param>
                /// <param name="staticVars">The lazily-initialized static variables collection.</param>
                /// <param name="index">The index to use to index into the variables.</param>
                /// <returns>The variables collection.</returns>
                private static FormatParam[] GetDynamicOrStaticVariables(
			char c, ref FormatParam[] dynamicVars, ref FormatParam[] staticVars, out int index)
                {
			if (c >= 'A' && c <= 'Z'){
				index = c - 'A';
				return staticVars ?? (staticVars = new FormatParam[26]); // one slot for each letter of alphabet
			} else if (c >= 'a' && c <= 'z') {
				index = c - 'a';
				return dynamicVars ?? (dynamicVars = new FormatParam[26]); // one slot for each letter of alphabet
			}
			else throw new InvalidOperationException("Terminfo database contains invalid values");
                }

                /// <summary>
                /// Represents a parameter to a terminfo formatting string.
                /// It is a discriminated union of either an integer or a string, 
                /// with characters represented as integers.
                /// </summary>
                public struct FormatParam
                {
			/// <summary>The integer stored in the parameter.</summary>
			private readonly int _int32;
			/// <summary>The string stored in the parameter.</summary>
			private readonly string _string; // null means an Int32 is stored
			
			/// <summary>Initializes the parameter with an integer value.</summary>
			/// <param name="value">The value to be stored in the parameter.</param>
			public FormatParam(Int32 value) : this(value, null) { }
			
			/// <summary>Initializes the parameter with a string value.</summary>
			/// <param name="value">The value to be stored in the parameter.</param>
			public FormatParam(String value) : this(0, value ?? string.Empty) { }
			
			/// <summary>Initializes the parameter.</summary>
			/// <param name="intValue">The integer value.</param>
			/// <param name="stringValue">The string value.</param>
			private FormatParam(Int32 intValue, String stringValue)
			{
				_int32 = intValue;
				_string = stringValue;
			}
			
			/// <summary>Implicit converts an integer into a parameter.</summary>
			public static implicit operator FormatParam(int value)
			{
				return new FormatParam(value);
			}
			
			/// <summary>Implicit converts a string into a parameter.</summary>
			public static implicit operator FormatParam(string value)
			{
				return new FormatParam(value);
			}
			
			/// <summary>Gets the integer value of the parameter. If a string was stored, 0 is returned.</summary>
			public int Int32 { get { return _int32; } }
			
			/// <summary>Gets the string value of the parameter.  If an Int32 or a null String were stored, an empty string is returned.</summary>
			public string String { get { return _string ?? string.Empty; } }
			
			/// <summary>Gets the string or the integer value as an object.</summary>
			public object Object { get { return _string ?? (object)_int32; } }
                }
		
                /// <summary>Provides a basic stack data structure.</summary>
                /// <typeparam name="T">Specifies the type of data in the stack.</typeparam>
                private sealed class LowLevelStack
		{
			private const int DefaultSize = 4;
			private FormatParam[] _arr;
			private int _count;
			
			public LowLevelStack() { _arr = new FormatParam[DefaultSize]; }
			
			public FormatParam Pop()
			{
				if (_count == 0)
					throw new InvalidOperationException("Terminfo: Invalid Stack");

				var item = _arr[--_count];
				_arr[_count] = default(FormatParam);
				return item;
			}
			
			public void Push(FormatParam item)
			{
				if (_arr.Length == _count){
					var newArr = new FormatParam[_arr.Length * 2];
					Array.Copy(_arr, 0, newArr, 0, _arr.Length);
					_arr = newArr;
				}
				_arr[_count++] = item;
			}
			
			public void Clear()
			{
				Array.Clear(_arr, 0, _count);
				_count = 0;
			}
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

