//
// SqlEditor.cs - writen in C# using GTK#
//
// Authors:
//     Daniel Morgan <danmorg@sc.rr.com>
//     Rodrigo Moya <rodrigo@gnome-db.org>
//
// (c)copyright 2002 Daniel Morgan
// (c)copyright 2002 Rodrigo Moya
//
// SqlEditorSharp is based on the gnome-db-sql-editor.c in libgnomedb.
// SqlEditorSharp falls under the GPL license and is included
// in SQL# For GTK#.  SQL# For GTK# is a database query tool for Mono.
//

namespace SqlEditorSharp 
{
	using System;
	using Gtk;
	using Gdk;
	using GLib;
	using System.Collections;
	using System.IO;
	using System.Text;
	using System.Runtime.InteropServices;
	using System.Diagnostics;
	using Mono.Data.SqlSharp.Gui.GtkSharp;

	/// <summary> SqlEditor Class</summary>
	/// <remarks>
	/// </remarks>
	public class SqlEditorSharp : Gtk.VBox 
	{
		// Fields

		// text tags for TextTagTable in TextBuffer
		private TextTag freecomment_tag;
		private TextTag linecomment_tag; 
		private TextTag singlequotedconstant_tag;
		private TextTag sql_tag;
		private TextTag normaltext_tag;

		// determine if something has changed beyond a line
		// updating one line is faster than the whole buffer
		//private int line_last_changed;
		//private int last_freecomment_count;

		// settings
		private bool use_hi_lighting;
		private string family;

		// widgets
		private ScrolledWindow scroll;
		private TextView sqlTextView;
		private TextBuffer sqlTextBuffer;
		private EditorTab tab = null;

		// Constructors

		public SqlEditorSharp() : base(false, 4) {		 
			scroll = new ScrolledWindow (
				new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0), 
				new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0));
			scroll.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			scroll.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			scroll.ShadowType = Gtk.ShadowType.In;
			this.PackStart (scroll, true, true, 0);

			// default font famly for SQL editor
			family = "courier";

			// other default settings
			use_hi_lighting = false;

			// create text tag table
			TextTagTable textTagTable = new TextTagTable ();

			// anything else is normaltext
			normaltext_tag = new TextTag ("normaltext");
			normaltext_tag.Family = family;
			normaltext_tag.Foreground = "black";
			normaltext_tag.Style = Pango.Style.Normal;
			textTagTable.Add (normaltext_tag);
       
			// SQL Keywords - SELECT FROM WHERE, etc
			sql_tag = new TextTag ("sql");
			sql_tag.Family = family;
			sql_tag.Foreground = "blue";
			sql_tag.Style = Pango.Style.Normal;
			textTagTable.Add (sql_tag);

			// c like free comment - used within a SQL statement
			freecomment_tag = new TextTag ("freecomment");
			freecomment_tag.Family = family;
			freecomment_tag.Foreground = "darkgreen";
			freecomment_tag.Style = Pango.Style.Italic;
			textTagTable.Add (freecomment_tag);

			// c++ like line comment, but using two hyphens
			linecomment_tag = new TextTag ("linecomment");
			linecomment_tag.Family = family;
			linecomment_tag.Foreground = "darkgreen";
			linecomment_tag.Style = Pango.Style.Italic;
			textTagTable.Add (linecomment_tag);

			/* single quoted constant - WHERE COL1 = 'ABC' */
			singlequotedconstant_tag = new TextTag ("singlequotedconstant");
			singlequotedconstant_tag.Family = family;
			singlequotedconstant_tag.Foreground = "red";
			singlequotedconstant_tag.Style = Pango.Style.Normal;
			textTagTable.Add (singlequotedconstant_tag);

			// create TextBuffer and TextView
			sqlTextBuffer = new TextBuffer (textTagTable);
			sqlTextView = new TextView (sqlTextBuffer);

			// allow it to be edited
			sqlTextView.Editable = true;

			//line_last_changed = -1;
			//last_freecomment_count = -1;

			// attach OnTextChanged callback function
			// to "changed" signal so we can do something
			// when the text has changed in the buffer
			sqlTextBuffer.Changed += new EventHandler (OnTextChanged);

			// add the TextView to the ScrolledWindow
			scroll.Add (sqlTextView);					
		}

		// Public Properties

		public TextBuffer Buffer {
			get {
				return sqlTextBuffer;
			}
		}

		public TextView View {
			get {
				return sqlTextView;
			}
		}

		public EditorTab Tab {
			get {
				return tab;
			}
			set {
				tab = value;
			}

		}

		public bool UseSyntaxHiLighting {
			get {
				return use_hi_lighting;
			}

			set {
				use_hi_lighting = value;
			}
		}

		// Private Methods

		void OnTextChanged (object o, EventArgs args) 
		{
			if(tab != null)
				tab.label.Text = tab.basefilename + " *";

			SqlSharpGtk.DebugWriteLine ("[[[[[ Syntax Hi-Light Text BEGIN ]]]]]");

			if (use_hi_lighting == true) {
				SyntaxHiLightText ();
			}
			
			SqlSharpGtk.DebugWriteLine ("[[[[[ Syntax Hi-Light Text END   ]]]]]\n");
		}

		void SyntaxHiLightText () 
		{
			TextIter start_iter, end_iter, 
				iter, insert_iter;
			TextIter match_start1, match_end1, 
				match_start2, match_end2;
			int char_count = 0;
			int hyphen = 0, single_quotes = 0;
			string text = String.Empty;
			int i = 0, start_con = 0, end_con = 0;
			//int line = 0;
			//int freecomment_count = 0;
			int start_word = -1;
			TextMark insert_mark;
			char ch = ' ';

			insert_mark = sqlTextBuffer.InsertMark;
			sqlTextBuffer.GetIterAtMark (out insert_iter, insert_mark);
			//line = insert_iter.Line;
			
			/* get the starting and ending text iterators */
			sqlTextBuffer.GetIterAtOffset (out start_iter, 0);
			char_count = sqlTextBuffer.CharCount;
			sqlTextBuffer.GetIterAtOffset (out end_iter, char_count);
			
			SqlSharpGtk.DebugWriteLine ("char_count: " + char_count);
			
			/* since line is not same - redo all */
			//if (line != line_last_changed) {
			/* remove all previously applied tags */
			sqlTextBuffer.RemoveAllTags (start_iter, end_iter);

			/* apply the entire buffer to the normaltext tag */
			sqlTextBuffer.ApplyTag (normaltext_tag, start_iter, end_iter);
			//}
			//else { /* just worry about current insertion line */
			//	/* get start iter */
			//	if (insert_iter.StartsLine () == true) {
			//		start_iter = insert_iter;
			//	}
			//	else {
			//		start_iter = insert_iter;
			//		start_iter.LineOffset = 0;
			//	}
			//	/* get end iter */	
			//	end_iter.ForwardToLineEnd ();
			//	char_count = start_iter.CharsInLine;
			//	
			//	/* remove all previously applied tags */
			//	sqlTextBuffer.RemoveAllTags (start_iter, end_iter);
			//	
			//	/* apply the entire buffer to the normaltext tag */
			//	sqlTextBuffer.ApplyTag (normaltext_tag,
			//		start_iter, end_iter);
			//
			//	/* get the starting and ending text iterators */
			//	sqlTextBuffer.GetIterAtOffset (out start_iter, 0);
			//	char_count = sqlTextBuffer.CharCount;
			//	sqlTextBuffer.GetIterAtOffset (out end_iter, char_count);
			//}

			/*  ------------------------------------
			 *  Free Comments (sort of like c style) 
			 *  ------------------------------------
			 *  except in SQL, a c like comment occurs within
			 *  a SQL statement
			 */ 
			match_start1 = start_iter; // dummy
			match_end1 = end_iter; // dummy
			match_start2 = start_iter; // dummy
			match_end2 = end_iter; // dummy

			while (start_iter.IsEnd == false) {
				// FIXME: match_start1, match_end1, end_iter
				//        need to be set to have ref in front
				//        Problem with TextIter's ForwardSearch()
				//        in GTK# (not GTK+)
				if (start_iter.ForwardSearch (
					"/*",
					TextSearchFlags.TextOnly,
					out match_start1, 
					out match_end1,
					end_iter) == true) {

					/* beginning of free comment found */ 
					//freecomment_count++;
					// FIXME: fix match_start2, match_end2, end_iter
					//        with ref if front
					if (match_end1.ForwardSearch (
						"*/",
						TextSearchFlags.TextOnly, 
						out match_start2,
						out match_end2,
						end_iter) == true) {

						// ending of free comment found, 
						// now hi-light comment
						sqlTextBuffer.ApplyTag (
							freecomment_tag,
							match_start1,
							match_end2);
						match_end2.ForwardChars (1);
						start_iter = match_end2;
					}
					else {
						// if no end found, 
						// hi-light to the end, 
						// to let the user know 
						// the ending asterisk slash is missing 
						ApplyTag (
							freecomment_tag,
							normaltext_tag,
							match_start1,
							end_iter);
						break;
					}
				}
				else
					break;
			}

			/* if free comments is different than last time,
			 * invalidate line_last_changed - causes 
			 * a complete redo (instead hi-lighting just the current line -
			 * do the whole buffer)
			 * THIS IS JUST AN ATTEMPT FOR SPEED
			 */
			//if (freecomment_count != last_freecomment_count) {
			//	line_last_changed = -1;
			//}

			/*********************************************************************
			 * See if the following needs hi-lighting:
			 * - Line Comments (sort of like C++ slash slash comments 
			 *   but uses hypen hyphen and it is based at the beginning of a line)
			 * - Single-Quoted Constants ( WHERE COL1 = 'ABC' )
			 * - SQL keywords (SELECT, FROM, WHERE, UPDATE, etc)
			 *********************************************************************/
			//if (line != line_last_changed) {
			sqlTextBuffer.GetIterAtOffset (out start_iter, 0);
			//}
			//else {
			//	if (insert_iter.StartsLine () == true) {
			//		start_iter = insert_iter;
			//	}
			//	else {
			//		start_iter = insert_iter;
			//		start_iter.LineOffset = 0;
			//	}
			//}

			// get starting and ending iters 
			// and character count of line
			char_count = sqlTextBuffer.CharCount;
			sqlTextBuffer.GetIterAtOffset (out end_iter, char_count);
			
			// for each line, look for:
			// line comments, constants, and keywoards 
			do {	
				iter = start_iter;
				iter.ForwardToLineEnd ();
				text = sqlTextBuffer.GetText (
					start_iter, iter, false);

				// look for line comment
				char_count = start_iter.CharsInLine;
				hyphen = 0; 
				for (i = 0; i < char_count - 1; i++) {
					switch (text[i]) {
					case '-':
						if (hyphen == 1) {
							hyphen = 2;
							// line comment found
							i = char_count;

							ApplyTag (
								linecomment_tag, 
								normaltext_tag,
								start_iter, 
								iter);
						}
						else {
							hyphen = 1;
						}
						break;
					case ' ':
						// continue
						break;
					default:
						// this line is not line commented
						i = char_count; // break out of for loop
						break;
					}
				}
				// if not line commented, 
				// look for singled quoted constants 
				// and keywords
				if (hyphen < 2) {
					if (start_iter.IsEnd == true)
						break; // break out of for loop

					start_word = -1;
					single_quotes = 0;

					LookForSingleQuotesAndWords (
						ref start_iter, 
						text, char_count,
						ref start_word, 
						ref single_quotes, 
						ref start_con, 
						ref end_con);
				}
				
			} while (start_iter.ForwardLine () == true);
	

			// POOR ATTEMPTS AT SPEED - last_freecomment_count 
			// and line_last_changed 
			//
			//last_freecomment_count = freecomment_count;
			//line_last_changed = line;
		}

		void LookForSingleQuotesAndWords (ref TextIter start_iter, 
					string text, int char_count,
					ref int start_word, ref int single_quotes, 
					ref int start_con, ref int end_con) 
		{
			TextIter match_start1, match_end1;
			int i;
			char ch;

			for (i = 0; i < char_count; i++) {
				match_start1 = start_iter;
				match_end1 = start_iter;
						
				if (match_end1.IsEnd == true)
					break;

				if (CharHasTag (start_iter, 
					freecomment_tag, i) 
					== false) {
							
					if (single_quotes == 0 && 
						start_word == -1) {

						ch = text[i];
						if (ch == '\'') {
							single_quotes = 1;
							start_con = i + 1;
						}
						else if (Char.IsLetter (ch)) {
							start_word = i;
						}
						else {
							// continue
						}
					}
					else if (single_quotes == 1) {
						ch = text[i];
						switch (ch) {
						case '\'':
							// single quoted constant
							end_con = i;
		
							// get starting and
							// ending of constant 
							// excluding quotes
							ApplyTagOffsets (
								start_iter,
								start_con, i,
								singlequotedconstant_tag,
								normaltext_tag);

							single_quotes = 0;
							break;
						default:
							break;
						}
					}
					else if (start_word != -1) {
						ch = text[i];
						// is character alphabetic, numeric, or '_'
						if (Char.IsLetterOrDigit (ch) || 
							ch == '_') {

							// continue 
						}
						else {
							// using start_word 
							// and i offsets, 
							// get word
							if (IsTextSQL (text, start_word, i)) {
								// word is a SQL keyword, 
								// hi-light word
								ApplyTagOffsets (
									start_iter,
									start_word, i,
									sql_tag,
									normaltext_tag);
							}
							start_word = -1;
							switch (text[i]) {
							case '\'':
								single_quotes = 1;
								start_con = i + 1;
								break;
							default:
								break;
							}
						}
					}
				} 
			}
			if( start_word != -1) {
				if (IsTextSQL (text, start_word, i)) {
					// word is a SQL keyword, 
					// hi-light word
					ApplyTagOffsets(
						start_iter,
						start_word, i,
						sql_tag,
						normaltext_tag);
				}
			}
		}

		void ApplyTag ( TextTag apply_tag, TextTag remove_tag,
				TextIter start_iter, TextIter end_iter ) 
		{
#if DEBUG
			DebugText(start_iter, end_iter, "ApplyTag() " + 
				"remove: " + remove_tag.Name + 
				" apply: " + apply_tag.Name);
#endif // DEBUG

			sqlTextBuffer.RemoveTag (
				remove_tag, 
				start_iter, end_iter);

			sqlTextBuffer.ApplyTag (
				apply_tag,
				start_iter, end_iter);
		}

		void ApplyTagOffsets (TextIter start_iter, 
				int start_offset, int end_offset, 
				TextTag apply_tag,
				TextTag remove_tag) 
		{
			TextIter begin_iter, end_iter;
	
			begin_iter = start_iter;
			end_iter = start_iter;

			begin_iter.LineOffset = start_offset;
			end_iter.LineOffset = end_offset;
	
#if DEBUG		
			DebugText(start_iter, end_iter, "ApplyTagOffsets() " + 
				"remove: " + remove_tag.Name + 
				" apply: " + apply_tag.Name +
				" start: " + start_offset.ToString() +
				" end: " + end_offset.ToString());
#endif

			sqlTextBuffer.RemoveTag (remove_tag,
				begin_iter, end_iter);

			sqlTextBuffer.ApplyTag (apply_tag,
				begin_iter, end_iter);
		}

		/* is word a SQL keyword? */
		bool IsTextSQL (string text, int begin, int end) 
		{
			string keyword = "";

			int i;
			int text_len;
			if(text.Equals(String.Empty))
				return false;

			if(begin < 0)
				return false;

			if(end < 2)
				return false;

			if(begin >= end)
				return false;

			text_len = end - begin;
			if(text_len < 1)
				return false;

#if DEBUG
			SqlSharpGtk.DebugWriteLine("IsTextSQL - " +
				"begin: " + begin.ToString() +
				" end: " + end.ToString() +
				" text_len: " + text_len);
			SqlSharpGtk.DebugWriteLine("[TEXT BEGIN]");
			SqlSharpGtk.DebugWriteLine(text);
			SqlSharpGtk.DebugWriteLine("[TEXT END  ]");
#endif // DEBUG

			for (i = 0; sql_keywords[i] != String.Empty; i++) {
				if(text_len == sql_keywords[i].Length) {
			
					SqlSharpGtk.DebugWriteLine(
						"Test length: " + text_len + 
						" keyword: " + keyword);
					
					try {
						keyword = text.Substring (begin, text_len);
					}
					catch(ArgumentOutOfRangeException a) {
						Console.WriteLine ("Internal Error: SqlSharpGtk: text.Substring() ArgumentOutOfRange");
					}

					keyword = keyword.ToUpper();

					if(keyword.Equals (sql_keywords [i]))
						return true;
				}
			}

			return false;
		}

		// does the character at offset in the GtkTextIter has
		// this text tag applied?
		bool CharHasTag(TextIter iter, 
				TextTag tag, int char_offset_in_line) 
		{

			TextIter offset_iter;

			offset_iter = iter;
			offset_iter.LineOffset = char_offset_in_line;
		
			return offset_iter.HasTag (tag);
		}

		public void Clear() 
		{
			TextIter start, end;
			start = sqlTextBuffer.StartIter;
			end = sqlTextBuffer.EndIter;
			sqlTextBuffer.Delete(start,end);
		}

		public void LoadFromFile(string inFilename) 
		{
			StreamReader sr = new StreamReader(inFilename);
			Clear();
			string NextLine;
			string line;
			
			while((NextLine = sr.ReadLine()) != null) {
				line = NextLine + "\n";
				sqlTextBuffer.Insert (sqlTextBuffer.EndIter, line);
			}
			sr.Close();
		}

		public void SaveToFile(string outFilename) 
		{
			TextIter start_iter, iter;
			string text;
			StreamWriter sw = null;
			
			sw = new StreamWriter(outFilename);			
			sqlTextBuffer.GetIterAtOffset (out iter, 0);
			start_iter = iter;
			while (iter.ForwardLine()) {
				text = sqlTextBuffer.GetText(start_iter, iter, false);
				sw.Write(text);
				start_iter = iter;
			}
			text = sqlTextBuffer.GetText(start_iter, iter, false);
			sw.Write(text);
			sw.Close();
			sw = null;
		}

		void DebugText (TextIter iter_start, TextIter iter_end,
				string debugMessage) 
		{

#if DEBUG
			string text = sqlTextBuffer.GetText (
				iter_start, iter_end, false);
			string msg = 
				"[DEBUG-TEXT]: " + 
				debugMessage +
				" (" +
				text +
				")";
			SqlSharpGtk.DebugWriteLine(msg);
#endif // DEBUG
		}

		static readonly string[] sql_keywords = 
			new string[] {
					     "DELETE",
					     "FROM",
					     "SELECT",
					     "UPDATE",
					     "SET",
					     "INSERT",
					     "INTO",
					     "VALUES",
					     "WHERE",
					     "COUNT",
					     "SUM",
					     "MAX",
					     "MIN",
					     "AVG",
					     "DROP",
					     "ALTER",
					     "CREATE",
					     "VIEW",
					     "TABLE",
					     "AS",
					     "AND",
					     "OR",
					     "ORDER",
					     "GROUP",
					     "BY",
					     "HAVING",
					     "IS",
					     "NULL",
					     "NOT",
					     "COMMIT",
					     "ROLLBACK",
					     "EXISTS",
					     "IN",
					     "LIKE",
					     "GRANT",
					     "REVOKE",
					     "ON",
					     "TO",
					     String.Empty
				     };
	}
}
