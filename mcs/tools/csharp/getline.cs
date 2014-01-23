//
// getline.cs: A command line editor
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//
// Copyright 2008 Novell, Inc.
//
// Dual-licensed under the terms of the MIT X11 license or the
// Apache License 2.0
//
// USE -define:DEMO to build this as a standalone file and test it
//
// TODO:
//    Enter an error (a = 1);  Notice how the prompt is in the wrong line
//		This is caused by Stderr not being tracked by System.Console.
//    Completion support
//    Why is Thread.Interrupt not working?   Currently I resort to Abort which is too much.
//
// Limitations in System.Console:
//    Console needs SIGWINCH support of some sort
//    Console needs a way of updating its position after things have been written
//    behind its back (P/Invoke puts for example).
//    System.Console needs to get the DELETE character, and report accordingly.
//

using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;

namespace Mono.Terminal {

	public class LineEditor {

		public class Completion {
			public string [] Result;
			public string Prefix;

			public Completion (string prefix, string [] result)
			{
				Prefix = prefix;
				Result = result;
			}
		}
		
		public delegate Completion AutoCompleteHandler (string text, int pos);
		
		//static StreamWriter log;
		
		// The text being edited.
		StringBuilder text;

		// The text as it is rendered (replaces (char)1 with ^A on display for example).
		StringBuilder rendered_text;

		// The prompt specified, and the prompt shown to the user.
		string prompt;
		string shown_prompt;
		
		// The current cursor position, indexes into "text", for an index
		// into rendered_text, use TextToRenderPos
		int cursor;

		// The row where we started displaying data.
		int home_row;

		// The maximum length that has been displayed on the screen
		int max_rendered;

		// If we are done editing, this breaks the interactive loop
		bool done = false;

		// The thread where the Editing started taking place
		Thread edit_thread;

		// Our object that tracks history
		History history;

		// The contents of the kill buffer (cut/paste in Emacs parlance)
		string kill_buffer = "";

		// The string being searched for
		string search;
		string last_search;

		// whether we are searching (-1= reverse; 0 = no; 1 = forward)
		int searching;

		// The position where we found the match.
		int match_at;
		
		// Used to implement the Kill semantics (multiple Alt-Ds accumulate)
		KeyHandler last_handler;
		
		delegate void KeyHandler ();
		
		struct Handler {
			public ConsoleKeyInfo CKI;
			public KeyHandler KeyHandler;

			public Handler (ConsoleKey key, KeyHandler h)
			{
				CKI = new ConsoleKeyInfo ((char) 0, key, false, false, false);
				KeyHandler = h;
			}

			public Handler (char c, KeyHandler h)
			{
				KeyHandler = h;
				// Use the "Zoom" as a flag that we only have a character.
				CKI = new ConsoleKeyInfo (c, ConsoleKey.Zoom, false, false, false);
			}

			public Handler (ConsoleKeyInfo cki, KeyHandler h)
			{
				CKI = cki;
				KeyHandler = h;
			}
			
			public static Handler Control (char c, KeyHandler h)
			{
				return new Handler ((char) (c - 'A' + 1), h);
			}

			public static Handler Alt (char c, ConsoleKey k, KeyHandler h)
			{
				ConsoleKeyInfo cki = new ConsoleKeyInfo ((char) c, k, false, true, false);
				return new Handler (cki, h);
			}
		}

		/// <summary>
		///   Invoked when the user requests auto-completion using the tab character
		/// </summary>
		/// <remarks>
		///    The result is null for no values found, an array with a single
		///    string, in that case the string should be the text to be inserted
		///    for example if the word at pos is "T", the result for a completion
		///    of "ToString" should be "oString", not "ToString".
		///
		///    When there are multiple results, the result should be the full
		///    text
		/// </remarks>
		public AutoCompleteHandler AutoCompleteEvent;
		
		static Handler [] handlers;

		public LineEditor (string name) : this (name, 10) { }
		
		public LineEditor (string name, int histsize)
		{
			handlers = new Handler [] {
				new Handler (ConsoleKey.Home,       CmdHome),
				new Handler (ConsoleKey.End,        CmdEnd),
				new Handler (ConsoleKey.LeftArrow,  CmdLeft),
				new Handler (ConsoleKey.RightArrow, CmdRight),
				new Handler (ConsoleKey.UpArrow,    CmdHistoryPrev),
				new Handler (ConsoleKey.DownArrow,  CmdHistoryNext),
				new Handler (ConsoleKey.Enter,      CmdDone),
				new Handler (ConsoleKey.Backspace,  CmdBackspace),
				new Handler (ConsoleKey.Delete,     CmdDeleteChar),
				new Handler (ConsoleKey.Tab,        CmdTabOrComplete),
				
				// Emacs keys
				Handler.Control ('A', CmdHome),
				Handler.Control ('E', CmdEnd),
				Handler.Control ('B', CmdLeft),
				Handler.Control ('F', CmdRight),
				Handler.Control ('P', CmdHistoryPrev),
				Handler.Control ('N', CmdHistoryNext),
				Handler.Control ('K', CmdKillToEOF),
				Handler.Control ('Y', CmdYank),
				Handler.Control ('D', CmdDeleteChar),
				Handler.Control ('L', CmdRefresh),
				Handler.Control ('R', CmdReverseSearch),
				Handler.Control ('G', delegate {} ),
				Handler.Alt ('B', ConsoleKey.B, CmdBackwardWord),
				Handler.Alt ('F', ConsoleKey.F, CmdForwardWord),
				
				Handler.Alt ('D', ConsoleKey.D, CmdDeleteWord),
				Handler.Alt ((char) 8, ConsoleKey.Backspace, CmdDeleteBackword),
				
				// DEBUG
				//Handler.Control ('T', CmdDebug),

				// quote
				Handler.Control ('Q', delegate { HandleChar (Console.ReadKey (true).KeyChar); })
			};

			rendered_text = new StringBuilder ();
			text = new StringBuilder ();

			history = new History (name, histsize);
			
			//if (File.Exists ("log"))File.Delete ("log");
			//log = File.CreateText ("log"); 
		}

		void CmdDebug ()
		{
			history.Dump ();
			Console.WriteLine ();
			Render ();
		}

		void Render ()
		{
			Console.Write (shown_prompt);
			Console.Write (rendered_text);

			int max = System.Math.Max (rendered_text.Length + shown_prompt.Length, max_rendered);
			
			for (int i = rendered_text.Length + shown_prompt.Length; i < max_rendered; i++)
				Console.Write (' ');
			max_rendered = shown_prompt.Length + rendered_text.Length;

			// Write one more to ensure that we always wrap around properly if we are at the
			// end of a line.
			Console.Write (' ');

			UpdateHomeRow (max);
		}

		void UpdateHomeRow (int screenpos)
		{
			int lines = 1 + (screenpos / Console.WindowWidth);

			home_row = Console.CursorTop - (lines - 1);
			if (home_row < 0)
				home_row = 0;
		}
		

		void RenderFrom (int pos)
		{
			int rpos = TextToRenderPos (pos);
			int i;
			
			for (i = rpos; i < rendered_text.Length; i++)
				Console.Write (rendered_text [i]);

			if ((shown_prompt.Length + rendered_text.Length) > max_rendered)
				max_rendered = shown_prompt.Length + rendered_text.Length;
			else {
				int max_extra = max_rendered - shown_prompt.Length;
				for (; i < max_extra; i++)
					Console.Write (' ');
			}
		}

		void ComputeRendered ()
		{
			rendered_text.Length = 0;

			for (int i = 0; i < text.Length; i++){
				int c = (int) text [i];
				if (c < 26){
					if (c == '\t')
						rendered_text.Append ("    ");
					else {
						rendered_text.Append ('^');
						rendered_text.Append ((char) (c + (int) 'A' - 1));
					}
				} else
					rendered_text.Append ((char)c);
			}
		}

		int TextToRenderPos (int pos)
		{
			int p = 0;

			for (int i = 0; i < pos; i++){
				int c;

				c = (int) text [i];
				
				if (c < 26){
					if (c == 9)
						p += 4;
					else
						p += 2;
				} else
					p++;
			}

			return p;
		}

		int TextToScreenPos (int pos)
		{
			return shown_prompt.Length + TextToRenderPos (pos);
		}
		
		string Prompt {
			get { return prompt; }
			set { prompt = value; }
		}

		int LineCount {
			get {
				return (shown_prompt.Length + rendered_text.Length)/Console.WindowWidth;
			}
		}
		
		void ForceCursor (int newpos)
		{
			cursor = newpos;

			int actual_pos = shown_prompt.Length + TextToRenderPos (cursor);
			int row = home_row + (actual_pos/Console.WindowWidth);
			int col = actual_pos % Console.WindowWidth;

			if (row >= Console.BufferHeight)
				row = Console.BufferHeight-1;
			Console.SetCursorPosition (col, row);
			
			//log.WriteLine ("Going to cursor={0} row={1} col={2} actual={3} prompt={4} ttr={5} old={6}", newpos, row, col, actual_pos, prompt.Length, TextToRenderPos (cursor), cursor);
			//log.Flush ();
		}

		void UpdateCursor (int newpos)
		{
			if (cursor == newpos)
				return;

			ForceCursor (newpos);
		}

		void InsertChar (char c)
		{
			int prev_lines = LineCount;
			text = text.Insert (cursor, c);
			ComputeRendered ();
			if (prev_lines != LineCount){

				Console.SetCursorPosition (0, home_row);
				Render ();
				ForceCursor (++cursor);
			} else {
				RenderFrom (cursor);
				ForceCursor (++cursor);
				UpdateHomeRow (TextToScreenPos (cursor));
			}
		}

		//
		// Commands
		//
		void CmdDone ()
		{
			done = true;
		}

		void CmdTabOrComplete ()
		{
			bool complete = false;

			if (AutoCompleteEvent != null){
				if (TabAtStartCompletes)
					complete = true;
				else {
					for (int i = 0; i < cursor; i++){
						if (!Char.IsWhiteSpace (text [i])){
							complete = true;
							break;
						}
					}
				}

				if (complete){
					Completion completion = AutoCompleteEvent (text.ToString (), cursor);
					string [] completions = completion.Result;
					if (completions == null)
						return;
					
					int ncompletions = completions.Length;
					if (ncompletions == 0)
						return;
					
					if (completions.Length == 1){
						InsertTextAtCursor (completions [0]);
					} else {
						int last = -1;
						
						for (int p = 0; p < completions [0].Length; p++){
							char c = completions [0][p];


							for (int i = 1; i < ncompletions; i++){
								if (completions [i].Length < p)
									goto mismatch;
							
								if (completions [i][p] != c){
									goto mismatch;
								}
							}
							last = p;
						}
					mismatch:
						if (last != -1){
							InsertTextAtCursor (completions [0].Substring (0, last+1));
						}
						Console.WriteLine ();
						foreach (string s in completions){
							Console.Write (completion.Prefix);
							Console.Write (s);
							Console.Write (' ');
						}
						Console.WriteLine ();
						Render ();
						ForceCursor (cursor);
					}
				} else
					HandleChar ('\t');
			} else
				HandleChar ('t');
		}
		
		void CmdHome ()
		{
			UpdateCursor (0);
		}

		void CmdEnd ()
		{
			UpdateCursor (text.Length);
		}
		
		void CmdLeft ()
		{
			if (cursor == 0)
				return;

			UpdateCursor (cursor-1);
		}

		void CmdBackwardWord ()
		{
			int p = WordBackward (cursor);
			if (p == -1)
				return;
			UpdateCursor (p);
		}

		void CmdForwardWord ()
		{
			int p = WordForward (cursor);
			if (p == -1)
				return;
			UpdateCursor (p);
		}

		void CmdRight ()
		{
			if (cursor == text.Length)
				return;

			UpdateCursor (cursor+1);
		}

		void RenderAfter (int p)
		{
			ForceCursor (p);
			RenderFrom (p);
			ForceCursor (cursor);
		}
		
		void CmdBackspace ()
		{
			if (cursor == 0)
				return;

			text.Remove (--cursor, 1);
			ComputeRendered ();
			RenderAfter (cursor);
		}

		void CmdDeleteChar ()
		{
			// If there is no input, this behaves like EOF
			if (text.Length == 0){
				done = true;
				text = null;
				Console.WriteLine ();
				return;
			}
			
			if (cursor == text.Length)
				return;
			text.Remove (cursor, 1);
			ComputeRendered ();
			RenderAfter (cursor);
		}

		int WordForward (int p)
		{
			if (p >= text.Length)
				return -1;

			int i = p;
			if (Char.IsPunctuation (text [p]) || Char.IsSymbol (text [p]) || Char.IsWhiteSpace (text[p])){
				for (; i < text.Length; i++){
					if (Char.IsLetterOrDigit (text [i]))
					    break;
				}
				for (; i < text.Length; i++){
					if (!Char.IsLetterOrDigit (text [i]))
					    break;
				}
			} else {
				for (; i < text.Length; i++){
					if (!Char.IsLetterOrDigit (text [i]))
					    break;
				}
			}
			if (i != p)
				return i;
			return -1;
		}

		int WordBackward (int p)
		{
			if (p == 0)
				return -1;

			int i = p-1;
			if (i == 0)
				return 0;
			
			if (Char.IsPunctuation (text [i]) || Char.IsSymbol (text [i]) || Char.IsWhiteSpace (text[i])){
				for (; i >= 0; i--){
					if (Char.IsLetterOrDigit (text [i]))
						break;
				}
				for (; i >= 0; i--){
					if (!Char.IsLetterOrDigit (text[i]))
						break;
				}
			} else {
				for (; i >= 0; i--){
					if (!Char.IsLetterOrDigit (text [i]))
						break;
				}
			}
			i++;
			
			if (i != p)
				return i;

			return -1;
		}
		
		void CmdDeleteWord ()
		{
			int pos = WordForward (cursor);

			if (pos == -1)
				return;

			string k = text.ToString (cursor, pos-cursor);
			
			if (last_handler == CmdDeleteWord)
				kill_buffer = kill_buffer + k;
			else
				kill_buffer = k;
			
			text.Remove (cursor, pos-cursor);
			ComputeRendered ();
			RenderAfter (cursor);
		}
		
		void CmdDeleteBackword ()
		{
			int pos = WordBackward (cursor);
			if (pos == -1)
				return;

			string k = text.ToString (pos, cursor-pos);
			
			if (last_handler == CmdDeleteBackword)
				kill_buffer = k + kill_buffer;
			else
				kill_buffer = k;
			
			text.Remove (pos, cursor-pos);
			ComputeRendered ();
			RenderAfter (pos);
		}
		
		//
		// Adds the current line to the history if needed
		//
		void HistoryUpdateLine ()
		{
			history.Update (text.ToString ());
		}
		
		void CmdHistoryPrev ()
		{
			if (!history.PreviousAvailable ())
				return;

			HistoryUpdateLine ();
			
			SetText (history.Previous ());
		}

		void CmdHistoryNext ()
		{
			if (!history.NextAvailable())
				return;

			history.Update (text.ToString ());
			SetText (history.Next ());
			
		}

		void CmdKillToEOF ()
		{
			kill_buffer = text.ToString (cursor, text.Length-cursor);
			text.Length = cursor;
			ComputeRendered ();
			RenderAfter (cursor);
		}

		void CmdYank ()
		{
			InsertTextAtCursor (kill_buffer);
		}

		void InsertTextAtCursor (string str)
		{
			int prev_lines = LineCount;
			text.Insert (cursor, str);
			ComputeRendered ();
			if (prev_lines != LineCount){
				Console.SetCursorPosition (0, home_row);
				Render ();
				cursor += str.Length;
				ForceCursor (cursor);
			} else {
				RenderFrom (cursor);
				cursor += str.Length;
				ForceCursor (cursor);
				UpdateHomeRow (TextToScreenPos (cursor));
			}
		}
		
		void SetSearchPrompt (string s)
		{
			SetPrompt ("(reverse-i-search)`" + s + "': ");
		}

		void ReverseSearch ()
		{
			int p;

			if (cursor == text.Length){
				// The cursor is at the end of the string
				
				p = text.ToString ().LastIndexOf (search);
				if (p != -1){
					match_at = p;
					cursor = p;
					ForceCursor (cursor);
					return;
				}
			} else {
				// The cursor is somewhere in the middle of the string
				int start = (cursor == match_at) ? cursor - 1 : cursor;
				if (start != -1){
					p = text.ToString ().LastIndexOf (search, start);
					if (p != -1){
						match_at = p;
						cursor = p;
						ForceCursor (cursor);
						return;
					}
				}
			}

			// Need to search backwards in history
			HistoryUpdateLine ();
			string s = history.SearchBackward (search);
			if (s != null){
				match_at = -1;
				SetText (s);
				ReverseSearch ();
			}
		}
		
		void CmdReverseSearch ()
		{
			if (searching == 0){
				match_at = -1;
				last_search = search;
				searching = -1;
				search = "";
				SetSearchPrompt ("");
			} else {
				if (search == ""){
					if (last_search != "" && last_search != null){
						search = last_search;
						SetSearchPrompt (search);

						ReverseSearch ();
					}
					return;
				}
				ReverseSearch ();
			} 
		}

		void SearchAppend (char c)
		{
			search = search + c;
			SetSearchPrompt (search);

			//
			// If the new typed data still matches the current text, stay here
			//
			if (cursor < text.Length){
				string r = text.ToString (cursor, text.Length - cursor);
				if (r.StartsWith (search))
					return;
			}

			ReverseSearch ();
		}
		
		void CmdRefresh ()
		{
			Console.Clear ();
			max_rendered = 0;
			Render ();
			ForceCursor (cursor);
		}

		void InterruptEdit (object sender, ConsoleCancelEventArgs a)
		{
			// Do not abort our program:
			a.Cancel = true;

			// Interrupt the editor
			edit_thread.Abort();
		}

		void HandleChar (char c)
		{
			if (searching != 0)
				SearchAppend (c);
			else
				InsertChar (c);
		}

		void EditLoop ()
		{
			ConsoleKeyInfo cki;

			while (!done){
				ConsoleModifiers mod;
				
				cki = Console.ReadKey (true);
				if (cki.Key == ConsoleKey.Escape){
					cki = Console.ReadKey (true);

					mod = ConsoleModifiers.Alt;
				} else
					mod = cki.Modifiers;
				
				bool handled = false;

				foreach (Handler handler in handlers){
					ConsoleKeyInfo t = handler.CKI;

					if (t.Key == cki.Key && t.Modifiers == mod){
						handled = true;
						handler.KeyHandler ();
						last_handler = handler.KeyHandler;
						break;
					} else if (t.KeyChar == cki.KeyChar && t.Key == ConsoleKey.Zoom){
						handled = true;
						handler.KeyHandler ();
						last_handler = handler.KeyHandler;
						break;
					}
				}
				if (handled){
					if (searching != 0){
						if (last_handler != CmdReverseSearch){
							searching = 0;
							SetPrompt (prompt);
						}
					}
					continue;
				}

				if (cki.KeyChar != (char) 0)
					HandleChar (cki.KeyChar);
			} 
		}

		void InitText (string initial)
		{
			text = new StringBuilder (initial);
			ComputeRendered ();
			cursor = text.Length;
			Render ();
			ForceCursor (cursor);
		}

		void SetText (string newtext)
		{
			Console.SetCursorPosition (0, home_row);
			InitText (newtext);
		}

		void SetPrompt (string newprompt)
		{
			shown_prompt = newprompt;
			Console.SetCursorPosition (0, home_row);
			Render ();
			ForceCursor (cursor);
		}
		
		public string Edit (string prompt, string initial)
		{
			edit_thread = Thread.CurrentThread;
			searching = 0;
			Console.CancelKeyPress += InterruptEdit;
			
			done = false;
			history.CursorToEnd ();
			max_rendered = 0;
			
			Prompt = prompt;
			shown_prompt = prompt;
			InitText (initial);
			history.Append (initial);

			do {
				try {
					EditLoop ();
				} catch (ThreadAbortException){
					searching = 0;
					Thread.ResetAbort ();
					Console.WriteLine ();
					SetPrompt (prompt);
					SetText ("");
				}
			} while (!done);
			Console.WriteLine ();
			
			Console.CancelKeyPress -= InterruptEdit;

			if (text == null){
				history.Close ();
				return null;
			}

			string result = text.ToString ();
			if (result != "")
				history.Accept (result);
			else
				history.RemoveLast ();

			return result;
		}
		
		public void SaveHistory ()
		{
			if (history != null) {
				history.Close ();
			}
		}

		public bool TabAtStartCompletes { get; set; }
			
		//
		// Emulates the bash-like behavior, where edits done to the
		// history are recorded
		//
		class History {
			string [] history;
			int head, tail;
			int cursor, count;
			string histfile;
			
			public History (string app, int size)
			{
				if (size < 1)
					throw new ArgumentException ("size");

				if (app != null){
					string dir = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
					//Console.WriteLine (dir);
					if (!Directory.Exists (dir)){
						try {
							Directory.CreateDirectory (dir);
						} catch {
							app = null;
						}
					}
					if (app != null)
						histfile = Path.Combine (dir, app) + ".history";
				}
				
				history = new string [size];
				head = tail = cursor = 0;

				if (File.Exists (histfile)){
					using (StreamReader sr = File.OpenText (histfile)){
						string line;
						
						while ((line = sr.ReadLine ()) != null){
							if (line != "")
								Append (line);
						}
					}
				}
			}

			public void Close ()
			{
				if (histfile == null)
					return;

				try {
					using (StreamWriter sw = File.CreateText (histfile)){
						int start = (count == history.Length) ? head : tail;
						for (int i = start; i < start+count; i++){
							int p = i % history.Length;
							sw.WriteLine (history [p]);
						}
					}
				} catch {
					// ignore
				}
			}
			
			//
			// Appends a value to the history
			//
			public void Append (string s)
			{
				//Console.WriteLine ("APPENDING {0} head={1} tail={2}", s, head, tail);
				history [head] = s;
				head = (head+1) % history.Length;
				if (head == tail)
					tail = (tail+1 % history.Length);
				if (count != history.Length)
					count++;
				//Console.WriteLine ("DONE: head={1} tail={2}", s, head, tail);
			}

			//
			// Updates the current cursor location with the string,
			// to support editing of history items.   For the current
			// line to participate, an Append must be done before.
			//
			public void Update (string s)
			{
				history [cursor] = s;
			}

			public void RemoveLast ()
			{
				head = head-1;
				if (head < 0)
					head = history.Length-1;
			}
			
			public void Accept (string s)
			{
				int t = head-1;
				if (t < 0)
					t = history.Length-1;
				
				history [t] = s;
			}
			
			public bool PreviousAvailable ()
			{
				//Console.WriteLine ("h={0} t={1} cursor={2}", head, tail, cursor);
				if (count == 0)
					return false;
				int next = cursor-1;
				if (next < 0)
					next = count-1;

				if (next == head)
					return false;

				return true;
			}

			public bool NextAvailable ()
			{
				if (count == 0)
					return false;
				int next = (cursor + 1) % history.Length;
				if (next == head)
					return false;
				return true;
			}
			
			
			//
			// Returns: a string with the previous line contents, or
			// nul if there is no data in the history to move to.
			//
			public string Previous ()
			{
				if (!PreviousAvailable ())
					return null;

				cursor--;
				if (cursor < 0)
					cursor = history.Length - 1;

				return history [cursor];
			}

			public string Next ()
			{
				if (!NextAvailable ())
					return null;

				cursor = (cursor + 1) % history.Length;
				return history [cursor];
			}

			public void CursorToEnd ()
			{
				if (head == tail)
					return;

				cursor = head;
			}

			public void Dump ()
			{
				Console.WriteLine ("Head={0} Tail={1} Cursor={2} count={3}", head, tail, cursor, count);
				for (int i = 0; i < history.Length;i++){
					Console.WriteLine (" {0} {1}: {2}", i == cursor ? "==>" : "   ", i, history[i]);
				}
				//log.Flush ();
			}

			public string SearchBackward (string term)
			{
				for (int i = 0; i < count; i++){
					int slot = cursor-i-1;
					if (slot < 0)
						slot = history.Length+slot;
					if (slot >= history.Length)
						slot = 0;
					if (history [slot] != null && history [slot].IndexOf (term) != -1){
						cursor = slot;
						return history [slot];
					}
				}

				return null;
			}
			
		}
	}

#if DEMO
	class Demo {
		static void Main ()
		{
			LineEditor le = new LineEditor ("foo");
			string s;
			
			while ((s = le.Edit ("shell> ", "")) != null){
				Console.WriteLine ("----> [{0}]", s);
			}
		}
	}
#endif
}
