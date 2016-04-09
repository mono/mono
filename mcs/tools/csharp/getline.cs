//
// getline.cs: A command line editor
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//
// Copyright 2008 Novell, Inc.
// Copyright 2016 Xamarin Inc
//
// Completion wanted:
//
//   * Enable bash-like completion window the window as an option for non-GUI people?
//
//   * Continue completing when Backspace is used?
//
//   * Should we keep the auto-complete on "."?
//
//   * Completion produces an error if the value is not resolvable, we should hide those errors
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
// Bug:
//   About 8 lines missing, type "Con<TAB>" and not enough lines are inserted at the bottom.
// 
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

		// null does nothing, "csharp" uses some heuristics that make sense for C#
		public string HeuristicsMode;
		
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

		// If we have a popup completion, this is not null and holds the state.
		CompletionState current_completion;

		// If this is set, it contains an escape sequence to reset the Unix colors to the ones that were used on startup
		static byte [] unix_reset_colors;

		// This contains a raw stream pointing to stdout, used to bypass the TermInfoDriver
		static Stream unix_raw_output;
		
		delegate void KeyHandler ();
		
		struct Handler {
			public ConsoleKeyInfo CKI;
			public KeyHandler KeyHandler;
			public bool ResetCompletion;
			
			public Handler (ConsoleKey key, KeyHandler h, bool resetCompletion = true)
			{
				CKI = new ConsoleKeyInfo ((char) 0, key, false, false, false);
				KeyHandler = h;
				ResetCompletion = resetCompletion;
			}

			public Handler (char c, KeyHandler h, bool resetCompletion = true)
			{
				KeyHandler = h;
				// Use the "Zoom" as a flag that we only have a character.
				CKI = new ConsoleKeyInfo (c, ConsoleKey.Zoom, false, false, false);
				ResetCompletion = resetCompletion;
			}

			public Handler (ConsoleKeyInfo cki, KeyHandler h, bool resetCompletion = true)
			{
				CKI = cki;
				KeyHandler = h;
				ResetCompletion = resetCompletion;
			}
			
			public static Handler Control (char c, KeyHandler h, bool resetCompletion = true)
			{
				return new Handler ((char) (c - 'A' + 1), h, resetCompletion);
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
				new Handler (ConsoleKey.UpArrow,    CmdUp, resetCompletion: false),
				new Handler (ConsoleKey.DownArrow,  CmdDown, resetCompletion: false),
				new Handler (ConsoleKey.Enter,      CmdDone, resetCompletion: false),
				new Handler (ConsoleKey.Backspace,  CmdBackspace, resetCompletion: false),
				new Handler (ConsoleKey.Delete,     CmdDeleteChar),
				new Handler (ConsoleKey.Tab,        CmdTabOrComplete, resetCompletion: false),
				
				// Emacs keys
				Handler.Control ('A', CmdHome),
				Handler.Control ('E', CmdEnd),
				Handler.Control ('B', CmdLeft),
				Handler.Control ('F', CmdRight),
				Handler.Control ('P', CmdUp, resetCompletion: false),
				Handler.Control ('N', CmdDown, resetCompletion: false),
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

			GetUnixConsoleReset ();
			//if (File.Exists ("log"))File.Delete ("log");
			//log = File.CreateText ("log"); 
		}

		// On Unix, there is a "default" color which is not represented by any colors in
		// ConsoleColor and it is not possible to set is by setting the ForegroundColor or
		// BackgroundColor properties, so we have to use the terminfo driver in Mono to
		// fetch these values

		void GetUnixConsoleReset ()
		{
			//
			// On Unix, we want to be able to reset the color for the pop-up completion
			//
			int p = (int) Environment.OSVersion.Platform;
			var is_unix = (p == 4) || (p == 128);
			if (!is_unix)
				return;

			// Sole purpose of this call is to initialize the Terminfo driver
			var x = Console.CursorLeft;
			
			try {
				var terminfo_driver = Type.GetType ("System.ConsoleDriver")?.GetField ("driver", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue (null);
				if (terminfo_driver == null)
					return;

				var unix_reset_colors_str = (terminfo_driver?.GetType ()?.GetField ("origPair", BindingFlags.Instance | BindingFlags.NonPublic))?.GetValue (terminfo_driver) as string;
				
				if (unix_reset_colors_str != null)
					unix_reset_colors = Encoding.UTF8.GetBytes ((string)unix_reset_colors_str);
				unix_raw_output = Console.OpenStandardOutput ();
			} catch (Exception e){
				Console.WriteLine ("Error: " + e);
			}
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

		static void SaveExcursion (Action code)
		{
			var saved_col = Console.CursorLeft;
			var saved_row = Console.CursorTop;
			var saved_fore = Console.ForegroundColor;
			var saved_back = Console.BackgroundColor;
			
			code ();
			
			Console.CursorLeft = saved_col;
			Console.CursorTop = saved_row;
			if (unix_reset_colors != null){
				unix_raw_output.Write (unix_reset_colors, 0, unix_reset_colors.Length);
			} else {
				Console.ForegroundColor = saved_fore;
				Console.BackgroundColor = saved_back;
			}
		}
		
		class CompletionState {
			public string Prefix;
			public string [] Completions;
			public int Col, Row, Width, Height;
			int selected_item, top_item;

			public CompletionState (int col, int row, int width, int height)
			{
				Col = col;
				Row = row;
				Width = width;
				Height = height;

				if (Col < 0)
					throw new ArgumentException ("Cannot be less than zero" + Col, "Col");
				if (Row < 0)
					throw new ArgumentException ("Cannot be less than zero", "Row");
				if (Width < 1)
					throw new ArgumentException ("Cannot be less than one", "Width");
				if (Height < 1)
					throw new ArgumentException ("Cannot be less than one", "Height");
				
			}
			
			void DrawSelection ()
			{
				for (int r = 0; r < Height; r++){
					int item_idx = top_item + r;
					bool selected = (item_idx == selected_item);
					
					Console.ForegroundColor = selected ? ConsoleColor.Black : ConsoleColor.Gray;
					Console.BackgroundColor = selected ? ConsoleColor.Cyan : ConsoleColor.Blue;

					var item = Prefix + Completions [item_idx];
					if (item.Length > Width)
						item = item.Substring (0, Width);

					Console.CursorLeft = Col;
					Console.CursorTop = Row + r;
					Console.Write (item);
					for (int space = item.Length; space <= Width; space++)
						Console.Write (" ");
				}
			}

			public string Current {
				get {
					return Completions [selected_item];
				}
			}
			
			public void Show ()
			{
				SaveExcursion (DrawSelection);
			}

			public void SelectNext ()
			{
				if (selected_item+1 < Completions.Length){
					selected_item++;
					if (selected_item - top_item >= Height)
						top_item++;
					SaveExcursion (DrawSelection);
				}
			}

			public void SelectPrevious ()
			{
				if (selected_item > 0){
					selected_item--;
					if (selected_item < top_item)
						top_item = selected_item;
					SaveExcursion (DrawSelection);
				}
			}

			void Clear ()
			{
				for (int r = 0; r < Height; r++){
					Console.CursorLeft = Col;
					Console.CursorTop = Row + r;
					for (int space = 0; space <= Width; space++)
						Console.Write (" ");
				}
			}
			
			public void Remove ()
			{
				SaveExcursion (Clear);
			}
		}

		void ShowCompletions (string prefix, string [] completions)
		{
			// Ensure we have space, determine window size
			int window_height = System.Math.Min (completions.Length, Console.WindowHeight/5);
			int target_line = Console.WindowHeight-window_height-1;
			if (Console.CursorTop > target_line){
				var saved_left = Console.CursorLeft;
				var delta = Console.CursorTop-target_line;
				Console.CursorLeft = 0;
				Console.CursorTop = Console.WindowHeight-1;
				for (int i = 0; i < delta+1; i++){
					for (int c = Console.WindowWidth; c > 0; c--)
						Console.Write (" "); // To debug use ("{0}", i%10);
				}
				Console.CursorTop = target_line;
				Console.CursorLeft = 0;
				Render ();
			}

			const int MaxWidth = 50;
			int window_width = 12;
			int plen = prefix.Length;
			foreach (var s in completions)
				window_width = System.Math.Max (plen + s.Length, window_width);
			window_width = System.Math.Min (window_width, MaxWidth);

			if (current_completion == null){
				int left = Console.CursorLeft-prefix.Length;
				
				if (left + window_width + 1 >= Console.WindowWidth)
					left = Console.WindowWidth-window_width-1;
				
				current_completion = new CompletionState (left, Console.CursorTop+1, window_width, window_height) {
					Prefix = prefix,
					Completions = completions,
				};
			} else {
				current_completion.Prefix = prefix;
				current_completion.Completions = completions;
			}
			current_completion.Show ();
			Console.CursorLeft = 0;
		}

		void HideCompletions ()
		{
			if (current_completion == null)
				return;
			current_completion.Remove ();
			current_completion = null;
		}

		//
		// Triggers the completion engine, if insertBestMatch is true, then this will
		// insert the best match found, this behaves like the shell "tab" which will
		// complete as much as possible given the options.
		//
		void Complete ()
		{
			Completion completion = AutoCompleteEvent (text.ToString (), cursor);
			string [] completions = completion.Result;
			if (completions == null){
				HideCompletions ();
				return;
			}
					
			int ncompletions = completions.Length;
			if (ncompletions == 0){
				HideCompletions ();
				return;
			}
					
			if (completions.Length == 1){
				InsertTextAtCursor (completions [0]);
				HideCompletions ();
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
				var prefix = completion.Prefix;
				if (last != -1){
					InsertTextAtCursor (completions [0].Substring (0, last+1));

					// Adjust the completions to skip the common prefix
					prefix += completions [0].Substring (0, last+1);
					for (int i = 0; i < completions.Length; i++)
						completions [i] = completions [i].Substring (last+1);
				}
				ShowCompletions (prefix, completions);
				Render ();
				ForceCursor (cursor);
			}
		}

		//
		// When the user has triggered a completion window, this will try to update
		// the contents of it.   The completion window is assumed to be hidden at this
		// point
		// 
		void UpdateCompletionWindow ()
		{
			if (current_completion != null)
				throw new Exception ("This method should only be called if the window has been hidden");
			
			Completion completion = AutoCompleteEvent (text.ToString (), cursor);
			string [] completions = completion.Result;
			if (completions == null)
				return;
					
			int ncompletions = completions.Length;
			if (ncompletions == 0)
				return;
			
			ShowCompletions (completion.Prefix, completion.Result);
			Render ();
			ForceCursor (cursor);
		}
		
		
		//
		// Commands
		//
		void CmdDone ()
		{
			if (current_completion != null){
				InsertTextAtCursor (current_completion.Current);
				HideCompletions ();
				return;
			}
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

				if (complete)
					Complete ();
				else
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

			bool completing = current_completion != null;
			HideCompletions ();
			
			text.Remove (--cursor, 1);
			ComputeRendered ();
			RenderAfter (cursor);
			if (completing)
				UpdateCompletionWindow ();
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

		void CmdUp ()
		{
			if (current_completion == null)
				CmdHistoryPrev ();
			else
				current_completion.SelectPrevious ();
		}

		void CmdDown ()
		{
			if (current_completion == null)
				CmdHistoryNext ();
			else
				current_completion.SelectNext ();
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

		//
		// Implements heuristics to show the completion window based on the mode
		//
		bool HeuristicAutoComplete (bool wasCompleting, char insertedChar)
		{
			if (HeuristicsMode == "csharp"){
				// csharp heuristics
				if (wasCompleting){
					if (insertedChar == ' '){
						return false;
					}
					return true;
				} 
				// If we were not completing, determine if we want to now
				if (insertedChar == '.'){
					// Avoid completing for numbers "1.2" for example
					if (cursor > 1 && Char.IsDigit (text[cursor-2])){
						for (int p = cursor-3; p >= 0; p--){
							char c = text[p];
							if (Char.IsDigit (c))
								continue;
							if (c == '_')
								return true;
							if (Char.IsLetter (c) || Char.IsPunctuation (c) || Char.IsSymbol (c) || Char.IsControl (c))
								return true;
						}
						return false;
					}
					return true;
				}
			}
			return false;
		}
		
		void HandleChar (char c)
		{
			if (searching != 0)
				SearchAppend (c);
			else {
				bool completing = current_completion != null;
				HideCompletions ();

				InsertChar (c);
				if (HeuristicAutoComplete (completing, c))
					UpdateCompletionWindow ();
			}
		}

		void EditLoop ()
		{
			ConsoleKeyInfo cki;

			while (!done){
				ConsoleModifiers mod;
				
				cki = Console.ReadKey (true);
				if (cki.Key == ConsoleKey.Escape){
					if (current_completion != null){
						HideCompletions ();
						continue;
					} else {
						cki = Console.ReadKey (true);
						
						mod = ConsoleModifiers.Alt;
					}
				} else
					mod = cki.Modifiers;
				
				bool handled = false;

				foreach (Handler handler in handlers){
					ConsoleKeyInfo t = handler.CKI;

					if (t.Key == cki.Key && t.Modifiers == mod){
						handled = true;
						if (handler.ResetCompletion)
							HideCompletions ();
						handler.KeyHandler ();
						last_handler = handler.KeyHandler;
						break;
					} else if (t.KeyChar == cki.KeyChar && t.Key == ConsoleKey.Zoom){
						handled = true;
						if (handler.ResetCompletion)
							HideCompletions ();

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

				if (cki.KeyChar != (char) 0){
					HandleChar (cki.KeyChar);
				}
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
			LineEditor le = new LineEditor ("foo") {
				HeuristicsMode = "csharp"
			};
			le.AutoCompleteEvent += delegate (string a, int pos){
				string prefix = "";
				var completions = new string [] { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };
				return new Mono.Terminal.LineEditor.Completion (prefix, completions);
			};
			
			string s;
			
			while ((s = le.Edit ("shell> ", "")) != null){
				Console.WriteLine ("----> [{0}]", s);
			}
		}
	}
#endif
}
