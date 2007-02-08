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
// Copyright (c) 2004-2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

// NOT COMPLETE

// There's still plenty of things missing, I've got most of it planned, just hadn't had
// the time to write it all yet.
// Stuff missing (in no particular order):
// - Align text after RecalculateLine
// - Implement tag types for hotlinks, etc.
// - Implement CaretPgUp/PgDown

// NOTE:
// selection_start.pos and selection_end.pos are 0-based
// selection_start.pos = first selected char
// selection_end.pos = first NOT-selected char
//
// FormatText methods are 1-based (as are all tags, LineTag.Start is 1 for 
// the first character on a line; the reason is that 0 is the position 
// *before* the first character on a line


#undef Debug

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Text;
using System.Text;

namespace System.Windows.Forms {
	internal enum LineColor {
		Red	= 0,
		Black	= 1
	}

	internal enum CaretSelection {
		Position,	// Selection=Caret
		Word,		// Selection=Word under caret
		Line		// Selection=Line under caret
	}

	internal class FontDefinition {
		internal String		face;
		internal int		size;
		internal FontStyle	add_style;
		internal FontStyle	remove_style;
		internal Color		color;
		internal Font		font_obj;
	}

	[Flags]
	internal enum FormatSpecified {
		None,

		BackColor = 2,
		Font = 4,
		Color = 8,
	}

	internal enum CaretDirection {
		CharForward,	// Move a char to the right
		CharBack,	// Move a char to the left
		LineUp,		// Move a line up
		LineDown,	// Move a line down
		Home,		// Move to the beginning of the line
		End,		// Move to the end of the line
		PgUp,		// Move one page up
		PgDn,		// Move one page down
		CtrlPgUp,	// Move caret to the first visible char in the viewport
		CtrlPgDn,	// Move caret to the last visible char in the viewport
		CtrlHome,	// Move to the beginning of the document
		CtrlEnd,	// Move to the end of the document
		WordBack,	// Move to the beginning of the previous word (or beginning of line)
		WordForward,	// Move to the beginning of the next word (or end of line)
		SelectionStart,	// Move to the beginning of the current selection
		SelectionEnd,	// Move to the end of the current selection
		CharForwardNoWrap,   // Move a char forward, but don't wrap onto the next line
		CharBackNoWrap      // Move a char backward, but don't wrap onto the previous line
	}

	// Being cloneable should allow for nice line and document copies...
	internal class Line : ICloneable, IComparable {
		#region	Local Variables
		// Stuff that matters for our line
		internal StringBuilder		text;			// Characters for the line
		internal float[]		widths;			// Width of each character; always one larger than text.Length
		internal int			space;			// Number of elements in text and widths
		internal int			line_no;		// Line number
		internal LineTag		tags;			// Tags describing the text
		internal int			Y;			// Baseline
		internal int			height;			// Height of the line (height of tallest tag)
		internal int			ascent;			// Ascent of the line (ascent of the tallest tag)
		internal HorizontalAlignment	alignment;		// Alignment of the line
		internal int			align_shift;		// Pixel shift caused by the alignment
		internal bool			soft_break;		// Tag is 'broken soft' and continuation from previous line
		internal int			indent;			// Left indent for the first line
		internal int			hanging_indent;		// Hanging indent (left indent for all but the first line)
		internal int			right_indent;		// Right indent for all lines
		internal bool carriage_return;


		// Stuff that's important for the tree
		internal Line			parent;			// Our parent line
		internal Line			left;			// Line with smaller line number
		internal Line			right;			// Line with higher line number
		internal LineColor		color;			// We're doing a black/red tree. this is the node color
		internal int			DEFAULT_TEXT_LEN;	// 
		internal bool			recalc;			// Line changed
		#endregion	// Local Variables

		#region Constructors
		internal Line() {
			color = LineColor.Red;
			left = null;
			right = null;
			parent = null;
			text = null;
			recalc = true;
			soft_break = false;
			alignment = HorizontalAlignment.Left;
		}

		internal Line(int LineNo, string Text, Font font, SolidBrush color) : this() {
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder(Text, space);
			line_no = LineNo;

			widths = new float[space + 1];
			tags = new LineTag(this, 1);
			tags.font = font;
			tags.color = color;
		}

		internal Line(int LineNo, string Text, HorizontalAlignment align, Font font, SolidBrush color) : this() {
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder(Text, space);
			line_no = LineNo;
			alignment = align;

			widths = new float[space + 1];
			tags = new LineTag(this, 1);
			tags.font = font;
			tags.color = color;
		}

		internal Line(int LineNo, string Text, LineTag tag) : this() {
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder(Text, space);
			line_no = LineNo;

			widths = new float[space + 1];
			tags = tag;
		}

		#endregion	// Constructors

		#region Internal Properties
		internal int Indent {
			get {
				return indent;
			}

			set {
				indent = value;
				recalc = true;
			}
		}

		internal int HangingIndent {
			get {
				return hanging_indent;
			}

			set {
				hanging_indent = value;
				recalc = true;
			}
		}

		internal int RightIndent {
			get {
				return right_indent;
			}

			set {
				right_indent = value;
				recalc = true;
			}
		}
			

		internal int Height {
			get {
				return height;
			}

			set {
				height = value;
			}
		}

		internal int LineNo {
			get {
				return line_no;
			}

			set {
				line_no = value;
			}
		}

		internal string Text {
			get {
				return text.ToString();
			}

			set {
				text = new StringBuilder(value, value.Length > DEFAULT_TEXT_LEN ? value.Length : DEFAULT_TEXT_LEN);
			}
		}

		internal HorizontalAlignment Alignment {
			get {
				return alignment;
			}

			set {
				if (alignment != value) {
					alignment = value;
					recalc = true;
				}
			}
		}
#if no
		internal StringBuilder Text {
			get {
				return text;
			}

			set {
				text = value;
			}
		}
#endif
		#endregion	// Internal Properties

		#region Internal Methods
		// Make sure we always have enoughs space in text and widths
		internal void Grow(int minimum) {
			int	length;
			float[]	new_widths;

			length = text.Length;

			if ((length + minimum) > space) {
				// We need to grow; double the size

				if ((length + minimum) > (space * 2)) {
					new_widths = new float[length + minimum * 2 + 1];
					space = length + minimum * 2;
				} else {				
					new_widths = new float[space * 2 + 1];
					space *= 2;
				}
				widths.CopyTo(new_widths, 0);

				widths = new_widths;
			}
		}

		internal void Streamline(int lines) {
			LineTag	current;
			LineTag	next;

			current = this.tags;
			next = current.next;

			// Catch what the loop below wont; eliminate 0 length 
			// tags, but only if there are other tags after us
			while ((current.length == 0) && (next != null)) {
				tags = next;
				tags.previous = null;
				current = next;
				next = current.next;
			}

			if (next == null) {
				return;
			}

			while (next != null) {
				// Take out 0 length tags unless it's the last tag in the document
				if (next.length == 0) {
					if ((next.next != null) || (line_no != lines)) {
						current.next = next.next;
						if (current.next != null) {
							current.next.previous = current;
						}
						next = current.next;
						continue;
					}
				}
				if (current.Combine(next)) {
					next = current.next;
					continue;
				}

				current = current.next;
				next = current.next;
			}
		}

		/// <summary> Find the tag on a line based on the character position, pos is 0-based</summary>
		internal LineTag FindTag(int pos) {
			LineTag tag;

			if (pos == 0) {
				return tags;
			}

			tag = this.tags;

			if (pos >= text.Length) {
				pos = text.Length - 1;
			}

			while (tag != null) {
				if (((tag.start - 1) <= pos) && (pos < (tag.start + tag.length - 1))) {
					return LineTag.GetFinalTag (tag);
				}
				tag = tag.next;
			}
			return null;
		}

		/// <summary>
		/// Recalculate a single line using the same char for every character in the line
		/// </summary>
		
		internal bool RecalculatePasswordLine(Graphics g, Document doc) {
			LineTag	tag;
			int	pos;
			int	len;
			float	w;
			bool	ret;
			int	descent;

			pos = 0;
			len = this.text.Length;
			tag = this.tags;
			ascent = 0;
			tag.shift = 0;

			this.recalc = false;
			widths[0] = indent;

			w = g.MeasureString(doc.password_char, tags.font, 10000, Document.string_format).Width;

			if (this.height != (int)tag.font.Height) {
				ret = true;
			} else {
				ret = false;
			}

			this.height = (int)tag.font.Height;
			tag.height = this.height;

			XplatUI.GetFontMetrics(g, tag.font, out tag.ascent, out descent);
			this.ascent = tag.ascent;

			while (pos < len) {
				pos++;
				widths[pos] = widths[pos-1] + w;
			}

			return ret;
		}

		/// <summary>
		/// Go through all tags on a line and recalculate all size-related values;
		/// returns true if lineheight changed
		/// </summary>
		internal bool RecalculateLine(Graphics g, Document doc) {
			LineTag	tag;
			int	pos;
			int	len;
			SizeF	size;
			float	w;
			int	prev_height;
			bool	retval;
			bool	wrapped;
			Line	line;
			int	wrap_pos;

			pos = 0;
			len = this.text.Length;
			tag = this.tags;
			prev_height = this.height;	// For drawing optimization calculations
			this.height = 0;		// Reset line height
			this.ascent = 0;		// Reset the ascent for the line
			tag.shift = 0;

			if (this.soft_break) {
				widths[0] = hanging_indent;
			} else {
				widths[0] = indent;
			}

			this.recalc = false;
			retval = false;
			wrapped = false;

			wrap_pos = 0;

			while (pos < len) {

				while (tag.length == 0) {	// We should always have tags after a tag.length==0 unless len==0
					tag.ascent = 0;
					tag = tag.next;
					tag.shift = 0;
				}

				size = tag.SizeOfPosition (g, pos);
				w = size.Width;

				if (Char.IsWhiteSpace(text[pos])) {
					wrap_pos = pos + 1;
				}

				if (doc.wrap) {
					if ((wrap_pos > 0) && (wrap_pos != len) && (widths[pos] + w) + 5 > (doc.viewport_width - this.right_indent)) {
						// Make sure to set the last width of the line before wrapping
						widths [pos + 1] = widths [pos] + w;

						pos = wrap_pos;
						len = text.Length;
						doc.Split(this, tag, pos, this.soft_break);
						this.soft_break = true;
						len = this.text.Length;
						
						retval = true;
						wrapped = true;
					}  else if (pos > 1 && (widths[pos] + w) > (doc.viewport_width - this.right_indent)) {
						// No suitable wrap position was found so break right in the middle of a word

						// Make sure to set the last width of the line before wrapping
						widths [pos + 1] = widths [pos] + w;

						doc.Split(this, tag, pos, this.soft_break);
						this.soft_break = true;
						len = this.text.Length;
						retval = true;
						wrapped = true;
					}
				}

				// Contract all soft lines that follow back into our line
				if (!wrapped) {
					pos++;

					widths[pos] = widths[pos-1] + w;

					if (pos == len) {
						line = doc.GetLine(this.line_no + 1);
						if ((line != null) && soft_break) {
							// Pull the two lines together
							doc.Combine(this.line_no, this.line_no + 1);
							len = this.text.Length;
							retval = true;
						}
					}
				}

				if (pos == (tag.start-1 + tag.length)) {
					// We just found the end of our current tag
					tag.height = tag.MaxHeight ();

					// Check if we're the tallest on the line (so far)
					if (tag.height > this.height) {
						this.height = tag.height;		// Yep; make sure the line knows
					}

					if (tag.ascent == 0) {
						int	descent;

						XplatUI.GetFontMetrics(g, tag.font, out tag.ascent, out descent);
					}

					if (tag.ascent > this.ascent) {
						LineTag		t;

						// We have a tag that has a taller ascent than the line;
						t = tags;
						while (t != null && t != tag) {
							t.shift = tag.ascent - t.ascent;
							t = t.next;
						}

						// Save on our line
						this.ascent = tag.ascent;
					} else {
						tag.shift = this.ascent - tag.ascent;
					}

					tag = tag.next;
					if (tag != null) {
						tag.shift = 0;
						wrap_pos = pos;
					}
				}
			}

			if (this.height == 0) {
				this.height = tags.font.Height;
				tag.height = this.height;
			}

			if (prev_height != this.height) {
				retval = true;
			}
			return retval;
		}
		#endregion	// Internal Methods

		#region Administrative
		public int CompareTo(object obj) {
			if (obj == null) {
				return 1;
			}

			if (! (obj is Line)) {
				throw new ArgumentException("Object is not of type Line", "obj");
			}

			if (line_no < ((Line)obj).line_no) {
				return -1;
			} else if (line_no > ((Line)obj).line_no) {
				return 1;
			} else {
				return 0;
			}
		}

		public object Clone() {
			Line	clone;

			clone = new Line();

			clone.text = text;

			if (left != null) {
				clone.left = (Line)left.Clone();
			}

			if (left != null) {
				clone.left = (Line)left.Clone();
			}

			return clone;
		}

		internal object CloneLine() {
			Line	clone;

			clone = new Line();

			clone.text = text;

			return clone;
		}

		public override bool Equals(object obj) {
			if (obj == null) {
				return false;
			}

			if (!(obj is Line)) {
				return false;
			}

			if (obj == this) {
				return true;
			}

			if (line_no == ((Line)obj).line_no) {
				return true;
			}

			return false;
		}

		public override int GetHashCode() {
			return base.GetHashCode ();
		}

		public override string ToString() {
			return "Line " + line_no;
		}

		#endregion	// Administrative
	}

	internal class Document : ICloneable, IEnumerable {
		#region Structures
		// FIXME - go through code and check for places where
		// we do explicit comparisons instead of using the compare overloads
		internal struct Marker {
			internal Line		line;
			internal LineTag	tag;
			internal int		pos;
			internal int		height;

			public static bool operator<(Marker lhs, Marker rhs) {
				if (lhs.line.line_no < rhs.line.line_no) {
					return true;
				}

				if (lhs.line.line_no == rhs.line.line_no) {
					if (lhs.pos < rhs.pos) {
						return true;
					}
				}
				return false;
			}

			public static bool operator>(Marker lhs, Marker rhs) {
				if (lhs.line.line_no > rhs.line.line_no) {
					return true;
				}

				if (lhs.line.line_no == rhs.line.line_no) {
					if (lhs.pos > rhs.pos) {
						return true;
					}
				}
				return false;
			}

			public static bool operator==(Marker lhs, Marker rhs) {
				if ((lhs.line.line_no == rhs.line.line_no) && (lhs.pos == rhs.pos)) {
					return true;
				}
				return false;
			}

			public static bool operator!=(Marker lhs, Marker rhs) {
				if ((lhs.line.line_no != rhs.line.line_no) || (lhs.pos != rhs.pos)) {
					return true;
				}
				return false;
			}

			public void Combine(Line move_to_line, int move_to_line_length) {
				line = move_to_line;
				pos += move_to_line_length;
				tag = LineTag.FindTag(line, pos);
			}

			// This is for future use, right now Document.Split does it by hand, with some added shortcut logic
			public void Split(Line move_to_line, int split_at) {
				line = move_to_line;
				pos -= split_at;
				tag = LineTag.FindTag(line, pos);
			}

			public override bool Equals(object obj) {
				   return this==(Marker)obj;
			}

			public override int GetHashCode() {
				return base.GetHashCode ();
			}

			public override string ToString() {
				return "Marker Line " + line + ", Position " + pos;
			}

		}
		#endregion Structures

		#region Local Variables
		private Line		document;
		private int		lines;
		private Line		sentinel;
		private int		document_id;
		private Random		random = new Random();
		internal string		password_char;
		private StringBuilder	password_cache;
		private bool		calc_pass;
		private int		char_count;

		// For calculating widths/heights
		public static readonly StringFormat string_format = new StringFormat (StringFormat.GenericTypographic);

		private int 		recalc_suspended;
		private bool		recalc_pending;
		private int		recalc_start = 1;   // This starts at one, since lines are 1 based
		private int		recalc_end;
		private bool		recalc_optimize;

		private int             update_suspended;
		private bool update_pending;
		private int update_start = 1;

		internal bool		multiline;
		internal bool		wrap;

		internal UndoManager	undo;

		internal Marker		caret;
		internal Marker		selection_start;
		internal Marker		selection_end;
		internal bool		selection_visible;
		internal Marker		selection_anchor;
		internal Marker		selection_prev;
		internal bool		selection_end_anchor;

		internal int		viewport_x;
		internal int		viewport_y;		// The visible area of the document
		internal int		viewport_width;
		internal int		viewport_height;

		internal int		document_x;		// Width of the document
		internal int		document_y;		// Height of the document

		internal Rectangle	invalid;

		internal int		crlf_size;		// 1 or 2, depending on whether we use \r\n or just \n

		internal TextBoxBase	owner;			// Who's owning us?
		static internal int	caret_width = 1;
		static internal int	caret_shift = 1;
		#endregion	// Local Variables

		#region Constructors
		internal Document(TextBoxBase owner) {
			lines = 0;

			this.owner = owner;

			multiline = true;
			password_char = "";
			calc_pass = false;
			recalc_pending = false;

			// Tree related stuff
			sentinel = new Line();
			sentinel.color = LineColor.Black;

			document = sentinel;

			// We always have a blank line
			owner.HandleCreated += new EventHandler(owner_HandleCreated);
			owner.VisibleChanged += new EventHandler(owner_VisibleChanged);

			Add(1, "", owner.Font, ThemeEngine.Current.ResPool.GetSolidBrush(owner.ForeColor));
			Line l = GetLine (1);
			l.soft_break = true;

			undo = new UndoManager (this);

			selection_visible = false;
			selection_start.line = this.document;
			selection_start.pos = 0;
			selection_start.tag = selection_start.line.tags;
			selection_end.line = this.document;
			selection_end.pos = 0;
			selection_end.tag = selection_end.line.tags;
			selection_anchor.line = this.document;
			selection_anchor.pos = 0;
			selection_anchor.tag = selection_anchor.line.tags;
			caret.line = this.document;
			caret.pos = 0;
			caret.tag = caret.line.tags;

			viewport_x = 0;
			viewport_y = 0;

			crlf_size = 2;

			// Default selection is empty

			document_id = random.Next();

			string_format.Trimming = StringTrimming.None;
			string_format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
		}
		#endregion

		#region Internal Properties
		internal Line Root {
			get {
				return document;
			}

			set {
				document = value;
			}
		}

		internal int Lines {
			get {
				return lines;
			}
		}

		internal Line CaretLine {
			get {
				return caret.line;
			}
		}

		internal int CaretPosition {
			get {
				return caret.pos;
			}
		}

		internal Point Caret {
			get {
				return new Point((int)caret.tag.line.widths[caret.pos] + caret.line.align_shift, caret.line.Y);
			}
		}

		internal LineTag CaretTag {
			get {
				return caret.tag;
			}

			set {
				caret.tag = value;
			}
		}

		internal int CRLFSize {
			get {
				return crlf_size;
			}

			set {
				crlf_size = value;
			}
		}

		internal string PasswordChar {
			get {
				return password_char;
			}

			set {
				password_char = value;
				PasswordCache.Length = 0;
				if ((password_char.Length != 0) && (password_char[0] != '\0')) {
					calc_pass = true;
				} else {
					calc_pass = false;
				}
			}
		}

		private StringBuilder PasswordCache {
			get { 
				if (password_cache == null) 
					  password_cache = new StringBuilder(); 
				return password_cache;
			}
		}

		internal int ViewPortX {
			get {
				return viewport_x;
			}

			set {
				viewport_x = value;
			}
		}

		internal int Length {
			get {
				return char_count + lines - 1;	// Add \n for each line but the last
			}
		}

		private int CharCount {
			get {
				return char_count;
			}

			set {
				char_count = value;

				if (LengthChanged != null) {
					LengthChanged(this, EventArgs.Empty);
				}
			}
		}

		internal int ViewPortY {
			get {
				return viewport_y;
			}

			set {
				viewport_y = value;
			}
		}

		internal int ViewPortWidth {
			get {
				return viewport_width;
			}

			set {
				viewport_width = value;
			}
		}

		internal int ViewPortHeight {
			get {
				return viewport_height;
			}

			set {
				viewport_height = value;
			}
		}


		internal int Width {
			get {
				return this.document_x;
			}
		}

		internal int Height {
			get {
				return this.document_y;
			}
		}

		internal bool SelectionVisible {
			get {
				return selection_visible;
			}
		}

		internal bool Wrap {
			get {
				return wrap;
			}

			set {
				wrap = value;
			}
		}

		#endregion	// Internal Properties

		#region Private Methods

		internal void SuspendRecalc ()
		{
			recalc_suspended++;
		}

		internal void ResumeRecalc (bool immediate_update)
		{
			if (recalc_suspended > 0)
				recalc_suspended--;

			if (immediate_update && recalc_suspended == 0 && recalc_pending) {
				RecalculateDocument (owner.CreateGraphicsInternal(), recalc_start, recalc_end, recalc_optimize);
				recalc_pending = false;
			}
		}

		internal void SuspendUpdate ()
		{
			update_suspended++;
		}

		internal void ResumeUpdate (bool immediate_update)
		{
			if (update_suspended > 0)
				update_suspended--;

			if (immediate_update && update_suspended == 0 && update_pending) {
				UpdateView (GetLine (update_start), 0);
				update_pending = false;
			}
		}

		// For debugging
		internal int DumpTree(Line line, bool with_tags) {
			int	total;

			total = 1;

			Console.Write("Line {0} [# {1}], Y: {2}, soft: {3},  Text: '{4}'",
					line.line_no, line.GetHashCode(), line.Y, line.soft_break,
					line.text != null ? line.text.ToString() : "undefined");

			if (line.left == sentinel) {
				Console.Write(", left = sentinel");
			} else if (line.left == null) {
				Console.Write(", left = NULL");
			}

			if (line.right == sentinel) {
				Console.Write(", right = sentinel");
			} else if (line.right == null) {
				Console.Write(", right = NULL");
			}

			Console.WriteLine("");

			if (with_tags) {
				LineTag	tag;
				int	count;
				int	length;

				tag = line.tags;
				count = 1;
				length = 0;
				Console.Write("   Tags: ");
				while (tag != null) {
					Console.Write("{0} <{1}>-<{2}>", count++, tag.start, tag.end
							/*line.text.ToString (tag.start - 1, tag.length)*/);
					length += tag.length;

					if (tag.line != line) {
						Console.Write("BAD line link");
						throw new Exception("Bad line link in tree");
					}
					tag = tag.next;
					if (tag != null) {
						Console.Write(", ");
					}
				}
				if (length > line.text.Length) {
					throw new Exception(String.Format("Length of tags more than length of text on line (expected {0} calculated {1})", line.text.Length, length));
				} else if (length < line.text.Length) {
					throw new Exception(String.Format("Length of tags less than length of text on line (expected {0} calculated {1})", line.text.Length, length));
				}
				Console.WriteLine("");
			}
			if (line.left != null) {
				if (line.left != sentinel) {
					total += DumpTree(line.left, with_tags);
				}
			} else {
				if (line != sentinel) {
					throw new Exception("Left should not be NULL");
				}
			}

			if (line.right != null) {
				if (line.right != sentinel) {
					total += DumpTree(line.right, with_tags);
				}
			} else {
				if (line != sentinel) {
					throw new Exception("Right should not be NULL");
				}
			}

			for (int i = 1; i <= this.lines; i++) {
				if (GetLine(i) == null) {
					throw new Exception(String.Format("Hole in line order, missing {0}", i));
				}
			}

			if (line == this.Root) {
				if (total < this.lines) {
					throw new Exception(String.Format("Not enough nodes in tree, found {0}, expected {1}", total, this.lines));
				} else if (total > this.lines) {
					throw new Exception(String.Format("Too many nodes in tree, found {0}, expected {1}", total, this.lines));
				}
			}

			return total;
		}

		private void SetSelectionVisible (bool value)
		{
			selection_visible = value;

			// cursor and selection are enemies, we can't have both in the same room at the same time
			if (owner.IsHandleCreated && !owner.show_caret_w_selection)
				XplatUI.CaretVisible (owner.Handle, !selection_visible);
		}

		private void DecrementLines(int line_no) {
			int	current;

			current = line_no;
			while (current <= lines) {
				GetLine(current).line_no--;
				current++;
			}
			return;
		}

		private void IncrementLines(int line_no) {
			int	current;

			current = this.lines;
			while (current >= line_no) {
				GetLine(current).line_no++;
				current--;
			}
			return;
		}

		private void RebalanceAfterAdd(Line line1) {
			Line	line2;

			while ((line1 != document) && (line1.parent.color == LineColor.Red)) {
				if (line1.parent == line1.parent.parent.left) {
					line2 = line1.parent.parent.right;

					if ((line2 != null) && (line2.color == LineColor.Red)) {
						line1.parent.color = LineColor.Black;
						line2.color = LineColor.Black;
						line1.parent.parent.color = LineColor.Red;
						line1 = line1.parent.parent;
					} else {
						if (line1 == line1.parent.right) {
							line1 = line1.parent;
							RotateLeft(line1);
						}

						line1.parent.color = LineColor.Black;
						line1.parent.parent.color = LineColor.Red;

						RotateRight(line1.parent.parent);
					}
				} else {
					line2 = line1.parent.parent.left;

					if ((line2 != null) && (line2.color == LineColor.Red)) {
						line1.parent.color = LineColor.Black;
						line2.color = LineColor.Black;
						line1.parent.parent.color = LineColor.Red;
						line1 = line1.parent.parent;
					} else {
						if (line1 == line1.parent.left) {
							line1 = line1.parent;
							RotateRight(line1);
						}

						line1.parent.color = LineColor.Black;
						line1.parent.parent.color = LineColor.Red;
						RotateLeft(line1.parent.parent);
					}
				}
			}
			document.color = LineColor.Black;
		}

		private void RebalanceAfterDelete(Line line1) {
			Line line2;

			while ((line1 != document) && (line1.color == LineColor.Black)) {
				if (line1 == line1.parent.left) {
					line2 = line1.parent.right;
					if (line2.color == LineColor.Red) { 
						line2.color = LineColor.Black;
						line1.parent.color = LineColor.Red;
						RotateLeft(line1.parent);
						line2 = line1.parent.right;
					}
					if ((line2.left.color == LineColor.Black) && (line2.right.color == LineColor.Black)) { 
						line2.color = LineColor.Red;
						line1 = line1.parent;
					} else {
						if (line2.right.color == LineColor.Black) {
							line2.left.color = LineColor.Black;
							line2.color = LineColor.Red;
							RotateRight(line2);
							line2 = line1.parent.right;
						}
						line2.color = line1.parent.color;
						line1.parent.color = LineColor.Black;
						line2.right.color = LineColor.Black;
						RotateLeft(line1.parent);
						line1 = document;
					}
				} else { 
					line2 = line1.parent.left;
					if (line2.color == LineColor.Red) {
						line2.color = LineColor.Black;
						line1.parent.color = LineColor.Red;
						RotateRight(line1.parent);
						line2 = line1.parent.left;
					}
					if ((line2.right.color == LineColor.Black) && (line2.left.color == LineColor.Black)) {
						line2.color = LineColor.Red;
						line1 = line1.parent;
					} else {
						if (line2.left.color == LineColor.Black) {
							line2.right.color = LineColor.Black;
							line2.color = LineColor.Red;
							RotateLeft(line2);
							line2 = line1.parent.left;
						}
						line2.color = line1.parent.color;
						line1.parent.color = LineColor.Black;
						line2.left.color = LineColor.Black;
						RotateRight(line1.parent);
						line1 = document;
					}
				}
			}
			line1.color = LineColor.Black;
		}

		private void RotateLeft(Line line1) {
			Line	line2 = line1.right;

			line1.right = line2.left;

			if (line2.left != sentinel) {
				line2.left.parent = line1;
			}

			if (line2 != sentinel) {
				line2.parent = line1.parent;
			}

			if (line1.parent != null) {
				if (line1 == line1.parent.left) {
					line1.parent.left = line2;
				} else {
					line1.parent.right = line2;
				}
			} else {
				document = line2;
			}

			line2.left = line1;
			if (line1 != sentinel) {
				line1.parent = line2;
			}
		}

		private void RotateRight(Line line1) {
			Line line2 = line1.left;

			line1.left = line2.right;

			if (line2.right != sentinel) {
				line2.right.parent = line1;
			}

			if (line2 != sentinel) {
				line2.parent = line1.parent;
			}

			if (line1.parent != null) {
				if (line1 == line1.parent.right) {
					line1.parent.right = line2;
				} else {
					line1.parent.left = line2;
				}
			} else {
				document = line2;
			}

			line2.right = line1;
			if (line1 != sentinel) {
				line1.parent = line2;
			}
		}        


		internal void UpdateView(Line line, int pos) {
			if (!owner.IsHandleCreated) {
				return;
			}

			if (update_suspended > 0) {
				update_start = Math.Min (update_start, line.line_no);
				// update_end = Math.Max (update_end, line.line_no);
				// recalc_optimize = true;
				update_pending = true;
				return;
			}

			// Optimize invalidation based on Line alignment
			if (RecalculateDocument(owner.CreateGraphicsInternal(), line.line_no, line.line_no, true)) {
				// Lineheight changed, invalidate the rest of the document
				if ((line.Y - viewport_y) >=0 ) {
					// We formatted something that's in view, only draw parts of the screen
					owner.Invalidate(new Rectangle(0, line.Y - viewport_y, viewport_width, owner.Height - line.Y - viewport_y));
				} else {
					// The tag was above the visible area, draw everything
					owner.Invalidate();
				}
			} else {
				switch(line.alignment) {
					case HorizontalAlignment.Left: {
						owner.Invalidate(new Rectangle((int)line.widths[pos] - viewport_x - 1, line.Y - viewport_y, viewport_width, line.height + 1));
						break;
					}

					case HorizontalAlignment.Center: {
						owner.Invalidate(new Rectangle(0, line.Y - viewport_y, viewport_width, line.height + 1));
						break;
					}

					case HorizontalAlignment.Right: {
						owner.Invalidate(new Rectangle(0, line.Y - viewport_y, (int)line.widths[pos + 1] - viewport_x + line.align_shift, line.height + 1));
						break;
					}
				}
			}
		}


		// Update display from line, down line_count lines; pos is unused, but required for the signature
		internal void UpdateView(Line line, int line_count, int pos) {
			if (!owner.IsHandleCreated) {
				return;
			}

			if (recalc_suspended > 0) {
				recalc_start = Math.Min (recalc_start, line.line_no);
				recalc_end = Math.Max (recalc_end, line.line_no + line_count - 1);
				recalc_optimize = true;
				recalc_pending = true;
				return;
			}

			if (RecalculateDocument(owner.CreateGraphicsInternal(), line.line_no, line.line_no + line_count - 1, true)) {
				// Lineheight changed, invalidate the rest of the document
				if ((line.Y - viewport_y) >=0 ) {
					// We formatted something that's in view, only draw parts of the screen
//blah Console.WriteLine("TextControl.cs(981) Invalidate called in UpdateView(line, line_count, pos)");
					owner.Invalidate(new Rectangle(0, line.Y - viewport_y, viewport_width, owner.Height - line.Y - viewport_y));
				} else {
					// The tag was above the visible area, draw everything
//blah Console.WriteLine("TextControl.cs(985) Invalidate called in UpdateView(line, line_count, pos)");
					owner.Invalidate();
				}
			} else {
				Line	end_line;

				end_line = GetLine(line.line_no + line_count -1);
				if (end_line == null) {
					end_line = line;
				}

//blah Console.WriteLine("TextControl.cs(996) Invalidate called in UpdateView(line, line_count, pos)");
				owner.Invalidate(new Rectangle(0 - viewport_x, line.Y - viewport_y, (int)line.widths[line.text.Length], end_line.Y + end_line.height));
			}
		}
		#endregion	// Private Methods

		#region Internal Methods
		// Clear the document and reset state
		internal void Empty() {

			document = sentinel;
			lines = 0;

			// We always have a blank line
			Add(1, "", owner.Font, ThemeEngine.Current.ResPool.GetSolidBrush(owner.ForeColor));
			Line l = GetLine (1);
			l.soft_break = true;
			
			this.RecalculateDocument(owner.CreateGraphicsInternal());
			PositionCaret(0, 0);

			SetSelectionVisible (false);

			selection_start.line = this.document;
			selection_start.pos = 0;
			selection_start.tag = selection_start.line.tags;
			selection_end.line = this.document;
			selection_end.pos = 0;
			selection_end.tag = selection_end.line.tags;
			char_count = 0;

			viewport_x = 0;
			viewport_y = 0;

			document_x = 0;
			document_y = 0;

			if (owner.IsHandleCreated)
				owner.Invalidate ();
		}

		internal void PositionCaret(Line line, int pos) {
			caret.tag = line.FindTag(pos);
			caret.line = line;
			caret.pos = pos;

			if (owner.IsHandleCreated) {
				if (owner.Focused) {
					if (caret.height != caret.tag.height)
						XplatUI.CreateCaret (owner.Handle, caret_width, caret.height);
					XplatUI.SetCaretPos(owner.Handle, (int)caret.tag.line.widths[caret.pos] + caret.line.align_shift - viewport_x, caret.line.Y + caret.tag.shift - viewport_y + caret_shift);
				}

				if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
			}

			// We set this at the end because we use the heights to determine whether or
			// not we need to recreate the caret
			caret.height = caret.tag.height;

		}

		internal void PositionCaret(int x, int y) {
			if (!owner.IsHandleCreated) {
				return;
			}

			caret.tag = FindCursor(x, y, out caret.pos);
			caret.line = caret.tag.line;
			caret.height = caret.tag.height;

			if (owner.Focused) {
				XplatUI.CreateCaret (owner.Handle, caret_width, caret.height);
				XplatUI.SetCaretPos(owner.Handle, (int)caret.tag.line.widths[caret.pos] + caret.line.align_shift - viewport_x, caret.line.Y + caret.tag.shift - viewport_y + caret_shift);
			}

			if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
		}

		internal void CaretHasFocus() {
			if ((caret.tag != null) && owner.IsHandleCreated) {
				XplatUI.CreateCaret(owner.Handle, caret_width, caret.height);
				XplatUI.SetCaretPos(owner.Handle, (int)caret.tag.line.widths[caret.pos] + caret.line.align_shift - viewport_x, caret.line.Y + caret.tag.shift - viewport_y + caret_shift);

				DisplayCaret ();
			}

			if (owner.IsHandleCreated && selection_visible) {
				InvalidateSelectionArea ();
			}
		}

		internal void CaretLostFocus() {
			if (!owner.IsHandleCreated) {
				return;
			}
			XplatUI.DestroyCaret(owner.Handle);
		}

		internal void AlignCaret() {
			if (!owner.IsHandleCreated) {
				return;
			}

			caret.tag = LineTag.FindTag(caret.line, caret.pos);
			caret.height = caret.tag.height;

			if (owner.Focused) {
				XplatUI.CreateCaret(owner.Handle, caret_width, caret.height);
				XplatUI.SetCaretPos(owner.Handle, (int)caret.tag.line.widths[caret.pos] + caret.line.align_shift - viewport_x, caret.line.Y + caret.tag.shift - viewport_y + caret_shift);
				DisplayCaret ();
			}

			if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
		}

		internal void UpdateCaret() {
			if (!owner.IsHandleCreated || caret.tag == null) {
				return;
			}

			if (caret.tag.height != caret.height) {
				caret.height = caret.tag.height;
				if (owner.Focused) {
					XplatUI.CreateCaret(owner.Handle, caret_width, caret.height);
				}
			}

			XplatUI.SetCaretPos(owner.Handle, (int)caret.tag.line.widths[caret.pos] + caret.line.align_shift - viewport_x, caret.line.Y + caret.tag.shift - viewport_y + caret_shift);

			DisplayCaret ();

			if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
		}

		internal void DisplayCaret() {
			if (!owner.IsHandleCreated) {
				return;
			}

			if (owner.ShowSelection && (!selection_visible || owner.show_caret_w_selection)) {
				XplatUI.CaretVisible(owner.Handle, true);
			}
		}

		internal void HideCaret() {
			if (!owner.IsHandleCreated) {
				return;
			}

			if (owner.Focused) {
				XplatUI.CaretVisible(owner.Handle, false);
			}
		}

		internal void MoveCaret(CaretDirection direction) {
			// FIXME should we use IsWordSeparator to detect whitespace, instead 
			// of looking for actual spaces in the Word move cases?

			bool nowrap = false;
			switch(direction) {
				case CaretDirection.CharForwardNoWrap:
					nowrap = true;
					goto case CaretDirection.CharForward;
				case CaretDirection.CharForward: {
					caret.pos++;
					if (caret.pos > caret.line.text.Length) {
						if (multiline && !nowrap) {
							// Go into next line
							if (caret.line.line_no < this.lines) {
								caret.line = GetLine(caret.line.line_no+1);
								caret.pos = 0;
								caret.tag = caret.line.tags;
							} else {
								caret.pos--;
							}
						} else {
							// Single line; we stay where we are
							caret.pos--;
						}
					} else {
						if ((caret.tag.start - 1 + caret.tag.length) < caret.pos) {
							caret.tag = caret.tag.next;
						}
					}
					UpdateCaret();
					return;
				}

				case CaretDirection.CharBackNoWrap:
					nowrap = true;
					goto case CaretDirection.CharBack;
				case CaretDirection.CharBack: {
					if (caret.pos > 0) {
						// caret.pos--; // folded into the if below
						if (--caret.pos > 0) {
							if (caret.tag.start > caret.pos) {
								caret.tag = caret.tag.previous;
							}
						}
					} else {
						if (caret.line.line_no > 1 && !nowrap) {
							caret.line = GetLine(caret.line.line_no - 1);
							caret.pos = caret.line.text.Length;
							caret.tag = LineTag.FindTag(caret.line, caret.pos);
						}
					}
					UpdateCaret();
					return;
				}

				case CaretDirection.WordForward: {
					int len;

					len = caret.line.text.Length;
					if (caret.pos < len) {
						while ((caret.pos < len) && (caret.line.text[caret.pos] != ' ')) {
							caret.pos++;
						}
						if (caret.pos < len) {
							// Skip any whitespace
							while ((caret.pos < len) && (caret.line.text[caret.pos] == ' ')) {
								caret.pos++;
							}
						}
						caret.tag = LineTag.FindTag(caret.line, caret.pos);
					} else {
						if (caret.line.line_no < this.lines) {
							caret.line = GetLine(caret.line.line_no + 1);
							caret.pos = 0;
							caret.tag = caret.line.tags;
						}
					}
					UpdateCaret();
					return;
				}

				case CaretDirection.WordBack: {
					if (caret.pos > 0) {
						caret.pos--;

						while ((caret.pos > 0) && (caret.line.text[caret.pos] == ' ')) {
							caret.pos--;
						}

						while ((caret.pos > 0) && (caret.line.text[caret.pos] != ' ')) {
							caret.pos--;
						}

						if (caret.line.text.ToString(caret.pos, 1) == " ") {
							if (caret.pos != 0) {
								caret.pos++;
							} else {
								caret.line = GetLine(caret.line.line_no - 1);
								caret.pos = caret.line.text.Length;
							}
						}
						caret.tag = LineTag.FindTag(caret.line, caret.pos);
					} else {
						if (caret.line.line_no > 1) {
							caret.line = GetLine(caret.line.line_no - 1);
							caret.pos = caret.line.text.Length;
							caret.tag = LineTag.FindTag(caret.line, caret.pos);
						}
					}
					UpdateCaret();
					return;
				}

				case CaretDirection.LineUp: {
					if (caret.line.line_no > 1) {
						int	pixel;

						pixel = (int)caret.line.widths[caret.pos];
						PositionCaret(pixel, GetLine(caret.line.line_no - 1).Y);

						DisplayCaret ();
					}
					return;
				}

				case CaretDirection.LineDown: {
					if (caret.line.line_no < lines) {
						int	pixel;

						pixel = (int)caret.line.widths[caret.pos];
						PositionCaret(pixel, GetLine(caret.line.line_no + 1).Y);

						DisplayCaret ();
					}
					return;
				}

				case CaretDirection.Home: {
					if (caret.pos > 0) {
						caret.pos = 0;
						caret.tag = caret.line.tags;
						UpdateCaret();
					}
					return;
				}

				case CaretDirection.End: {
					if (caret.pos < caret.line.text.Length) {
						caret.pos = caret.line.text.Length;
						caret.tag = LineTag.FindTag(caret.line, caret.pos);
						UpdateCaret();
					}
					return;
				}

				case CaretDirection.PgUp: {

					int new_y, y_offset;

					if (viewport_y == 0) {

						// This should probably be handled elsewhere
						if (!(owner is RichTextBox)) {
							// Page down doesn't do anything in a regular TextBox
							// if the bottom of the document
							// is already visible, the page and the caret stay still
							return;
						}

						// We're just placing the caret at the end of the document, no scrolling needed
						owner.vscroll.Value = 0;
						Line line = GetLine (1);
						PositionCaret (line, 0);
					}

					y_offset = caret.line.Y - viewport_y;
					new_y = caret.line.Y - viewport_height;

					owner.vscroll.Value = Math.Max (new_y, 0);
					PositionCaret ((int)caret.line.widths[caret.pos], y_offset + viewport_y);
					return;
				}

				case CaretDirection.PgDn: {
					int new_y, y_offset;

					if ((viewport_y + viewport_height) > document_y) {

						// This should probably be handled elsewhere
						if (!(owner is RichTextBox)) {
							// Page up doesn't do anything in a regular TextBox
							// if the bottom of the document
							// is already visible, the page and the caret stay still
							return;
						}

						// We're just placing the caret at the end of the document, no scrolling needed
						owner.vscroll.Value = owner.vscroll.Maximum - viewport_height + 1;
						Line line = GetLine (lines);
						PositionCaret (line, line.Text.Length);
					}

					y_offset = caret.line.Y - viewport_y;
					new_y = caret.line.Y + viewport_height;
					
					owner.vscroll.Value = Math.Min (new_y, owner.vscroll.Maximum - viewport_height + 1);
					PositionCaret ((int)caret.line.widths[caret.pos], y_offset + viewport_y);
					
					return;
				}

				case CaretDirection.CtrlPgUp: {
					PositionCaret(0, viewport_y);
					DisplayCaret ();
					return;
				}

				case CaretDirection.CtrlPgDn: {
					Line	line;
					LineTag	tag;
					int	index;

					tag = FindTag(0, viewport_y + viewport_height, out index, false);
					if (tag.line.line_no > 1) {
						line = GetLine(tag.line.line_no - 1);
					} else {
						line = tag.line;
					}
					PositionCaret(line, line.Text.Length);
					DisplayCaret ();
					return;
				}

				case CaretDirection.CtrlHome: {
					caret.line = GetLine(1);
					caret.pos = 0;
					caret.tag = caret.line.tags;

					UpdateCaret();
					return;
				}

				case CaretDirection.CtrlEnd: {
					caret.line = GetLine(lines);
					caret.pos = caret.line.text.Length;
					caret.tag = LineTag.FindTag(caret.line, caret.pos);

					UpdateCaret();
					return;
				}

				case CaretDirection.SelectionStart: {
					caret.line = selection_start.line;
					caret.pos = selection_start.pos;
					caret.tag = selection_start.tag;

					UpdateCaret();
					return;
				}

				case CaretDirection.SelectionEnd: {
					caret.line = selection_end.line;
					caret.pos = selection_end.pos;
					caret.tag = selection_end.tag;

					UpdateCaret();
					return;
				}
			}
		}

		internal void DumpDoc ()
		{
			Console.WriteLine ("<doc>");
			for (int i = 1; i < lines; i++) {
				Line line = GetLine (i);
				Console.WriteLine ("<line no='{0}'>", line.line_no);

				LineTag tag = line.tags;
				while (tag != null) {
					Console.Write ("\t<tag color='{0}'>", tag.color.Color);
					Console.Write (tag.Text ());
					Console.WriteLine ("\t</tag>");
					tag = tag.next;
				}
				Console.WriteLine ("</line>");
			}
			Console.WriteLine ("</doc>");
		}

		internal void Draw (Graphics g, Rectangle clip)
		{
			Line line;		// Current line being drawn
			LineTag	tag;		// Current tag being drawn
			int start;		// First line to draw
			int end;		// Last line to draw
			StringBuilder text;	// String representing the current line
			int line_no;
			Brush tag_brush;
			Brush current_brush;
			Brush disabled_brush;
			Brush hilight;
			Brush hilight_text;

			// First, figure out from what line to what line we need to draw
			start = GetLineByPixel(clip.Top + viewport_y, false).line_no;
			end = GetLineByPixel(clip.Bottom + viewport_y, false).line_no;

			/// Make sure that we aren't drawing one more line then we need to
			line = GetLine (end - 1);
			if (line != null && clip.Bottom == line.Y + line.height + viewport_y)
				end--;			

			line_no = start;

			#if Debug
				DateTime	n = DateTime.Now;
				Console.WriteLine ("Started drawing: {0}s {1}ms", n.Second, n.Millisecond);
				Console.WriteLine ("CLIP:  {0}", clip);
				Console.WriteLine ("S: {0}", GetLine (start).text);
				Console.WriteLine ("E: {0}", GetLine (end).text);
			#endif

			disabled_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorGrayText);
			hilight = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorHighlight);
			hilight_text = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorHighlightText);

			while (line_no <= end) {
				line = GetLine (line_no);
				float line_y = line.Y - viewport_y;
				
				tag = line.tags;
				if (!calc_pass) {
					text = line.text;
				} else {
					if (PasswordCache.Length < line.text.Length)
						PasswordCache.Append(Char.Parse(password_char), line.text.Length - PasswordCache.Length);
					else if (PasswordCache.Length > line.text.Length)
						PasswordCache.Remove(line.text.Length, PasswordCache.Length - line.text.Length);
					text = PasswordCache;
				}

				int line_selection_start = text.Length + 1;
				int line_selection_end = text.Length + 1;
				if (selection_visible && owner.ShowSelection &&
						(line_no >= selection_start.line.line_no) &&
						(line_no <= selection_end.line.line_no)) {

					if (line_no == selection_start.line.line_no)
						line_selection_start = selection_start.pos + 1;
					else
						line_selection_start = 1;

					if (line_no == selection_end.line.line_no)
						line_selection_end = selection_end.pos + 1;
					else
						line_selection_end = text.Length + 1;

					if (line_selection_end == line_selection_start) {
						// There isn't really selection
						line_selection_start = text.Length + 1;
						line_selection_end = line_selection_start;
					} else {
						// lets draw some selection baby!!

						g.FillRectangle (hilight,
								line.widths [line_selection_start - 1] + line.align_shift - viewport_x, 
								line_y, line.widths [line_selection_end - 1] - line.widths [line_selection_start - 1], 
								line.height);
					}
				}

				current_brush = line.tags.color;
				while (tag != null) {

					// Skip empty tags
					if (tag.length == 0) {
						tag = tag.next;
						continue;
					}

					if (((tag.X + tag.width) < (clip.Left - viewport_x)) && (tag.X > (clip.Right - viewport_x))) {
						tag = tag.next;
						continue;
					}

					if (tag.back_color != null) {
						g.FillRectangle (tag.back_color, tag.X + line.align_shift - viewport_x,
								line_y + tag.shift, tag.width, line.height);
					}

					tag_brush = tag.color;
					current_brush = tag_brush;
					
					if (!owner.is_enabled) {
						Color a = ((SolidBrush) tag.color).Color;
						Color b = ThemeEngine.Current.ColorWindowText;

						if ((a.R == b.R) && (a.G == b.G) && (a.B == b.B)) {
							tag_brush = disabled_brush;
						}
					}

					int tag_pos = tag.start;
					current_brush = tag_brush;
					while (tag_pos < tag.start + tag.length) {
						int old_tag_pos = tag_pos;

						if (tag_pos >= line_selection_start && tag_pos < line_selection_end) {
							current_brush = hilight_text;
							tag_pos = Math.Min (tag.end, line_selection_end);
						} else if (tag_pos < line_selection_start) {
							current_brush = tag.color;
							tag_pos = Math.Min (tag.end, line_selection_start);
						} else {
							current_brush = tag.color;
							tag_pos = tag.end;
						}

						tag.Draw (g, current_brush,
								line.widths [old_tag_pos - 1] + line.align_shift - viewport_x,
								line_y + tag.shift,
								old_tag_pos - 1, Math.Min (tag.length, tag_pos - old_tag_pos),
								text.ToString() );
					}
					tag = tag.next;
				}
				line_no++;
			}
		}

		private void InsertLineString (Line line, int pos, string s)
		{
			bool carriage_return = false;

			if (s.EndsWith ("\r")) {
				s = s.Substring (0, s.Length - 1);
				carriage_return = true;
			}

			InsertString (line, pos, s);

			if (carriage_return) {
				Line l = GetLine (line.line_no);
				l.carriage_return = true;
			}
		}

		// Insert multi-line text at the given position; use formatting at insertion point for inserted text
		internal void Insert(Line line, int pos, bool update_caret, string s) {
			int break_index;
			int base_line;
			int old_line_count;
			int count = 1;
			LineTag tag = LineTag.FindTag (line, pos);
			
			SuspendRecalc ();
			
			base_line = line.line_no;
			old_line_count = lines;

			break_index = s.IndexOf ('\n');

			// Bump the text at insertion point a line down if we're inserting more than one line
			if (break_index > -1) {
				Split(line, pos);
				line.soft_break = false;
				// Remainder of start line is now in base_line + 1
			}

			if (break_index == -1)
				break_index = s.Length;

			InsertLineString (line, pos, s.Substring (0, break_index));
			break_index++;

			while (break_index < s.Length) {
				bool soft = false;
				int next_break = s.IndexOf ('\n', break_index);
				int adjusted_next_break;
				bool carriage_return = false;

				if (next_break == -1) {
					next_break = s.Length;
					soft = true;
				}

				adjusted_next_break = next_break;
				if (s [next_break - 1] == '\r') {
					adjusted_next_break--;
					carriage_return = true;
				}

				string line_text = s.Substring (break_index, adjusted_next_break - break_index);
				Add (base_line + count, line_text, line.alignment, tag.font, tag.color);

				if (carriage_return) {
					Line last = GetLine (base_line + count);
					last.carriage_return = true;

					if (soft)
						last.soft_break = true;
				} else if (soft) {
					Line last = GetLine (base_line + count);
					last.soft_break = true;
				}

				count++;
				break_index = next_break + 1;
			}

			ResumeRecalc (true);

			UpdateView(line, lines - old_line_count + 1, pos);

			if (update_caret) {
				// Move caret to the end of the inserted text
				Line l = GetLine (line.line_no + lines - old_line_count);
				PositionCaret(l, l.text.Length);
				DisplayCaret ();
			}
		}

		// Inserts a character at the given position
		internal void InsertString(Line line, int pos, string s) {
			InsertString(line.FindTag(pos), pos, s);
		}

		// Inserts a string at the given position
		internal void InsertString(LineTag tag, int pos, string s) {
			Line	line;
			int	len;

			len = s.Length;

			CharCount += len;

			line = tag.line;
			line.text.Insert(pos, s);

			tag = tag.next;
			while (tag != null) {
				tag.start += len;
				tag = tag.next;
			}
			line.Grow(len);
			line.recalc = true;

			UpdateView(line, pos);
		}

		// Inserts a string at the caret position
		internal void InsertStringAtCaret(string s, bool move_caret) {

			InsertString (caret.tag, caret.pos, s);

			UpdateView(caret.line, caret.pos);
			if (move_caret) {
				caret.pos += s.Length;
				UpdateCaret();
			}
		}



		// Inserts a character at the given position
		internal void InsertChar(Line line, int pos, char ch) {
			InsertChar(line.FindTag(pos), pos, ch);
		}

		// Inserts a character at the given position
		internal void InsertChar(LineTag tag, int pos, char ch) {
			Line	line;

			CharCount++;

			line = tag.line;
			line.text.Insert(pos, ch);
			//	tag.length++;

			tag = tag.next;
			while (tag != null) {
				tag.start++;
				tag = tag.next;
			}
			line.Grow(1);
			line.recalc = true;

			undo.RecordTyping (line, pos, ch);
			UpdateView(line, pos);
		}

		// Inserts a character at the current caret position
		internal void InsertCharAtCaret(char ch, bool move_caret) {
			/*
			LineTag	tag;

			CharCount++;

			caret.line.text.Insert(caret.pos, ch);
			caret.tag.length++;
			
			if (caret.tag.next != null) {
				tag = caret.tag.next;
				while (tag != null) {
					tag.start++;
					tag = tag.next;
				}
			}
			caret.line.Grow(1);
			caret.line.recalc = true;
			*/
			InsertChar (caret.tag, caret.pos, ch);

			UpdateView(caret.line, caret.pos);
			if (move_caret) {
				caret.pos++;
				UpdateCaret();
				SetSelectionToCaret(true);
			}

		}
		
		internal void InsertImage (LineTag tag, int pos, Image image)
		{
			Line line;
			int len;

			len = 1;

			line = tag.line;
			line.text.Insert (pos, "I");

			LineTag next_tag = tag.Break (pos);
			ImageTag image_tag = new ImageTag (line, pos, image);
			image_tag.CopyFormattingFrom (tag);
			image_tag.next = next_tag;
			image_tag.previous = tag;
			tag.next = image_tag;
			
			tag = image_tag.next;
			while (tag != null) {
				tag.start += len;
				tag = tag.next;
			}

			line.Grow (len);
			line.recalc = true;

			DumpDoc ();
			UpdateView (line, pos);
		}

		internal void DeleteMultiline (Line start_line, int pos, int length)
		{
			Marker start = new Marker ();
			Marker end = new Marker ();
			int start_index = LineTagToCharIndex (start_line, pos);

			start.line = start_line;
			start.pos = pos;
			start.tag = LineTag.FindTag (start_line, pos);

			CharIndexToLineTag (start_index + length, out end.line,
					out end.tag, out end.pos);

			SuspendUpdate ();

			if (start.line == end.line) {
				DeleteChars (start.tag, pos, end.pos - pos);
			} else {

				// Delete first and last lines
				DeleteChars (start.tag, start.pos, start.line.text.Length - start.pos);
				DeleteChars (end.line.tags, 0, end.pos);

				int current = start.line.line_no + 1;
				if (current < end.line.line_no) {
					for (int i = end.line.line_no - 1; i >= current; i--) {
						Delete (i);
					}
				}

				// BIG FAT WARNING - selection_end.line might be stale due 
				// to the above Delete() call. DONT USE IT before hitting the end of this method!

				// Join start and end
				Combine (start.line.line_no, current);
			}

			ResumeUpdate (true);
		}

		
		// Deletes n characters at the given position; it will not delete past line limits
		// pos is 0-based
		internal void DeleteChars(LineTag tag, int pos, int count) {
			Line	line;
			bool	streamline;

			streamline = false;
			line = tag.line;

			CharCount -= count;

			if (pos == line.text.Length) {
				return;
			}

			line.text.Remove(pos, count);

			// Make sure the tag points to the right spot
			while ((tag != null) && (tag.end) < pos) {
				tag = tag.next;
			}

			if (tag == null) {
				return;
			}

			// Check if we're crossing tag boundaries
			if ((pos + count) > (tag.start + tag.length - 1)) {
				int	left;

				// We have to delete cross tag boundaries
				streamline = true;
				left = count;

				left -= tag.start + tag.length - pos - 1;

				tag = tag.next;
				while ((tag != null) && (left > 0)) {
					tag.start -= count - left;

					if (tag.length > left) {
						left = 0;
					} else {
						left -= tag.length;
						tag = tag.next;
					}

				}
			} else {
				// We got off easy, same tag

				if (tag.length == 0) {
					streamline = true;
				}
			}

			// Delete empty orphaned tags at the end
			LineTag walk = tag;
			while (walk != null && walk.next != null && walk.next.length == 0) {
				LineTag t = walk;
				walk.next = walk.next.next;
				if (walk.next != null)
					walk.next.previous = t;
				walk = walk.next;
			}

			// Adjust the start point of any tags following
			if (tag != null) {
				tag = tag.next;
				while (tag != null) {
					tag.start -= count;
					tag = tag.next;
				}
			}

			line.recalc = true;
			if (streamline) {
				line.Streamline(lines);
			}

			UpdateView(line, pos);
		}

		// Deletes a character at or after the given position (depending on forward); it will not delete past line limits
		internal void DeleteChar(LineTag tag, int pos, bool forward) {
			Line	line;
			bool	streamline;

			CharCount--;

			streamline = false;
			line = tag.line;

			if ((pos == 0 && forward == false) || (pos == line.text.Length && forward == true)) {
				return;
			}


			if (forward) {
				line.text.Remove(pos, 1);

				while ((tag != null) && (tag.start + tag.length - 1) <= pos) {
					tag = tag.next;
				}

				if (tag == null) {
					return;
				}

				//	tag.length--;

				if (tag.length == 0) {
					streamline = true;
				}
			} else {
				pos--;
				line.text.Remove(pos, 1);
				if (pos >= (tag.start - 1)) {
					//		tag.length--;
					if (tag.length == 0) {
						streamline = true;
					}
				} else if (tag.previous != null) {
					//		tag.previous.length--;
					if (tag.previous.length == 0) {
						streamline = true;
					}
				}
			}

			// Delete empty orphaned tags at the end
			LineTag walk = tag;
			while (walk != null && walk.next != null && walk.next.length == 0) {
				LineTag t = walk;
				walk.next = walk.next.next;
				if (walk.next != null)
					walk.next.previous = t;
				walk = walk.next;
			}

			tag = tag.next;
			while (tag != null) {
				tag.start--;
				tag = tag.next;
			}
			line.recalc = true;
			if (streamline) {
				line.Streamline(lines);
			}

			UpdateView(line, pos);
		}

		// Combine two lines
		internal void Combine(int FirstLine, int SecondLine) {
			Combine(GetLine(FirstLine), GetLine(SecondLine));
		}

		internal void Combine(Line first, Line second) {
			LineTag	last;
			int	shift;

			// Combine the two tag chains into one
			last = first.tags;

			// Maintain the line ending style
			first.soft_break = second.soft_break;

			while (last.next != null) {
				last = last.next;
			}

			// need to get the shift before setting the next tag since that effects length
			shift = last.start + last.length - 1;
			last.next = second.tags;
			last.next.previous = last;

			// Fix up references within the chain
			last = last.next;
			while (last != null) {
				last.line = first;
				last.start += shift;
				last = last.next;
			}

			// Combine both lines' strings
			first.text.Insert(first.text.Length, second.text.ToString());
			first.Grow(first.text.Length);

			// Remove the reference to our (now combined) tags from the doomed line
			second.tags = null;

			// Renumber lines
			DecrementLines(first.line_no + 2);	// first.line_no + 1 will be deleted, so we need to start renumbering one later

			// Mop up
			first.recalc = true;
			first.height = 0;	// This forces RecalcDocument/UpdateView to redraw from this line on
			first.Streamline(lines);

			// Update Caret, Selection, etc
			if (caret.line == second) {
				caret.Combine(first, shift);
			}
			if (selection_anchor.line == second) {
				selection_anchor.Combine(first, shift);
			}
			if (selection_start.line == second) {
				selection_start.Combine(first, shift);
			}
			if (selection_end.line == second) {
				selection_end.Combine(first, shift);
			}

			#if Debug
				Line	check_first;
				Line	check_second;

				check_first = GetLine(first.line_no);
				check_second = GetLine(check_first.line_no + 1);

				Console.WriteLine("Pre-delete: Y of first line: {0}, second line: {1}", check_first.Y, check_second.Y);
			#endif

			this.Delete(second);

			#if Debug
				check_first = GetLine(first.line_no);
				check_second = GetLine(check_first.line_no + 1);

				Console.WriteLine("Post-delete Y of first line: {0}, second line: {1}", check_first.Y, check_second.Y);
			#endif
		}

		// Split the line at the position into two
		internal void Split(int LineNo, int pos) {
			Line	line;
			LineTag	tag;

			line = GetLine(LineNo);
			tag = LineTag.FindTag(line, pos);
			Split(line, tag, pos, false);
		}

		internal void Split(Line line, int pos) {
			LineTag	tag;

			tag = LineTag.FindTag(line, pos);
			Split(line, tag, pos, false);
		}

		///<summary>Split line at given tag and position into two lines</summary>
		///<param name="soft">True if the split should be marked as 'soft', indicating that it can be contracted 
		///if more space becomes available on previous line</param>
		internal void Split(Line line, LineTag tag, int pos, bool soft) {
			LineTag	new_tag;
			Line	new_line;
			bool	move_caret;
			bool	move_sel_start;
			bool	move_sel_end;

			move_caret = false;
			move_sel_start = false;
			move_sel_end = false;

			// Adjust selection and cursors
			if (caret.line == line && caret.pos >= pos) {
				move_caret = true;
			}
			if (selection_start.line == line && selection_start.pos > pos) {
				move_sel_start = true;
			}

			if (selection_end.line == line && selection_end.pos > pos) {
				move_sel_end = true;
			}

			// cover the easy case first
			if (pos == line.text.Length) {
				Add(line.line_no + 1, "", line.alignment, tag.font, tag.color);

				new_line = GetLine(line.line_no + 1);

				line.carriage_return = false;
				new_line.carriage_return = line.carriage_return;
				new_line.soft_break = soft;
				
				if (move_caret) {
					caret.line = new_line;
					caret.tag = new_line.tags;
					caret.pos = 0;
				}

				if (move_sel_start) {
					selection_start.line = new_line;
					selection_start.pos = 0;
					selection_start.tag = new_line.tags;
				}

				if (move_sel_end) {
					selection_end.line = new_line;
					selection_end.pos = 0;
					selection_end.tag = new_line.tags;
				}
				return;
			}

			// We need to move the rest of the text into the new line
			Add (line.line_no + 1, line.text.ToString (pos, line.text.Length - pos), line.alignment, tag.font, tag.color);

			// Now transfer our tags from this line to the next
			new_line = GetLine(line.line_no + 1);

			line.carriage_return = false;
			new_line.carriage_return = line.carriage_return;
			new_line.soft_break = soft;

			line.recalc = true;
			new_line.recalc = true;

			if ((tag.start - 1) == pos) {
				int	shift;

				// We can simply break the chain and move the tag into the next line
				if (tag == line.tags) {
					new_tag = new LineTag(line, 1);
					new_tag.CopyFormattingFrom (tag);
					line.tags = new_tag;
				}

				if (tag.previous != null) {
					tag.previous.next = null;
				}
				new_line.tags = tag;
				tag.previous = null;
				tag.line = new_line;

				// Walk the list and correct the start location of the tags we just bumped into the next line
				shift = tag.start - 1;

				new_tag = tag;
				while (new_tag != null) {
					new_tag.start -= shift;
					new_tag.line = new_line;
					new_tag = new_tag.next;
				}
			} else {
				int	shift;

				new_tag = new LineTag (new_line, 1);			
				new_tag.next = tag.next;
				new_tag.CopyFormattingFrom (tag);
				new_line.tags = new_tag;
				if (new_tag.next != null) {
					new_tag.next.previous = new_tag;
				}
				tag.next = null;

				shift = pos;
				new_tag = new_tag.next;
				while (new_tag != null) {
					new_tag.start -= shift;
					new_tag.line = new_line;
					new_tag = new_tag.next;

				}
			}

			if (move_caret) {
				caret.line = new_line;
				caret.pos = caret.pos - pos;
				caret.tag = caret.line.FindTag(caret.pos);
			}

			if (move_sel_start) {
				selection_start.line = new_line;
				selection_start.pos = selection_start.pos - pos;
				selection_start.tag = new_line.FindTag(selection_start.pos);
			}

			if (move_sel_end) {
				selection_end.line = new_line;
				selection_end.pos = selection_end.pos - pos;
				selection_end.tag = new_line.FindTag(selection_end.pos);
			}

			CharCount -= line.text.Length - pos;
			line.text.Remove(pos, line.text.Length - pos);
		}

		// Adds a line of text, with given font.
		// Bumps any line at that line number that already exists down
		internal void Add(int LineNo, string Text, Font font, SolidBrush color) {
			Add(LineNo, Text, HorizontalAlignment.Left, font, color);
		}

		internal void Add(int LineNo, string Text, HorizontalAlignment align, Font font, SolidBrush color) {
			Line	add;
			Line	line;
			int	line_no;

			CharCount += Text.Length;

			if (LineNo<1 || Text == null) {
				if (LineNo<1) {
					throw new ArgumentNullException("LineNo", "Line numbers must be positive");
				} else {
					throw new ArgumentNullException("Text", "Cannot insert NULL line");
				}
			}

			add = new Line(LineNo, Text, align, font, color);

			line = document;
			while (line != sentinel) {
				add.parent = line;
				line_no = line.line_no;

				if (LineNo > line_no) {
					line = line.right;
				} else if (LineNo < line_no) {
					line = line.left;
				} else {
					// Bump existing line numbers; walk all nodes to the right of this one and increment line_no
					IncrementLines(line.line_no);
					line = line.left;
				}
			}

			add.left = sentinel;
			add.right = sentinel;

			if (add.parent != null) {
				if (LineNo > add.parent.line_no) {
					add.parent.right = add;
				} else {
					add.parent.left = add;
				}
			} else {
				// Root node
				document = add;
			}

			RebalanceAfterAdd(add);

			lines++;
		}

		internal virtual void Clear() {
			lines = 0;
			CharCount = 0;
			document = sentinel;
		}

		public virtual object Clone() {
			Document clone;

			clone = new Document(null);

			clone.lines = this.lines;
			clone.document = (Line)document.Clone();

			return clone;
		}

		internal void Delete(int LineNo) {
			Line	line;

			if (LineNo>lines) {
				return;
			}

			line = GetLine(LineNo);

			CharCount -= line.text.Length;

			DecrementLines(LineNo + 1);
			Delete(line);
		}

		internal void Delete(Line line1) {
			Line	line2;// = new Line();
			Line	line3;

			if ((line1.left == sentinel) || (line1.right == sentinel)) {
				line3 = line1;
			} else {
				line3 = line1.right;
				while (line3.left != sentinel) {
					line3 = line3.left;
				}
			}

			if (line3.left != sentinel) {
				line2 = line3.left;
			} else {
				line2 = line3.right;
			}

			line2.parent = line3.parent;
			if (line3.parent != null) {
				if(line3 == line3.parent.left) {
					line3.parent.left = line2;
				} else {
					line3.parent.right = line2;
				}
			} else {
				document = line2;
			}

			if (line3 != line1) {
				LineTag	tag;

				if (selection_start.line == line3) {
					selection_start.line = line1;
				}

				if (selection_end.line == line3) {
					selection_end.line = line1;
				}

				if (selection_anchor.line == line3) {
					selection_anchor.line = line1;
				}

				if (caret.line == line3) {
					caret.line = line1;
				}


				line1.alignment = line3.alignment;
				line1.ascent = line3.ascent;
				line1.hanging_indent = line3.hanging_indent;
				line1.height = line3.height;
				line1.indent = line3.indent;
				line1.line_no = line3.line_no;
				line1.recalc = line3.recalc;
				line1.right_indent = line3.right_indent;
				line1.soft_break = line3.soft_break;
				line1.space = line3.space;
				line1.tags = line3.tags;
				line1.text = line3.text;
				line1.widths = line3.widths;
				line1.Y = line3.Y;

				tag = line1.tags;
				while (tag != null) {
					tag.line = line1;
					tag = tag.next;
				}
			}

			if (line3.color == LineColor.Black)
				RebalanceAfterDelete(line2);

			this.lines--;
		}

		// Invalidate a section of the document to trigger redraw
		internal void Invalidate(Line start, int start_pos, Line end, int end_pos) {
			Line	l1;
			Line	l2;
			int	p1;
			int	p2;

			if ((start == end) && (start_pos == end_pos)) {
				return;
			}

			if (end_pos == -1) {
				end_pos = end.text.Length;
			}
	
			// figure out what's before what so the logic below is straightforward
			if (start.line_no < end.line_no) {
				l1 = start;
				p1 = start_pos;

				l2 = end;
				p2 = end_pos;
			} else if (start.line_no > end.line_no) {
				l1 = end;
				p1 = end_pos;

				l2 = start;
				p2 = start_pos;
			} else {
				if (start_pos < end_pos) {
					l1 = start;
					p1 = start_pos;

					l2 = end;
					p2 = end_pos;
				} else {
					l1 = end;
					p1 = end_pos;

					l2 = start;
					p2 = start_pos;
				}

				int endpoint = (int) l1.widths [p2];
				if (p2 == l1.text.Length + 1) {
					endpoint = (int) viewport_width;
				}

				#if Debug
					Console.WriteLine("Invaliding backwards from {0}:{1} to {2}:{3}   {4}",
							l1.line_no, p1, l2.line_no, p2,
							new Rectangle(
								(int)l1.widths[p1] + l1.align_shift - viewport_x, 
								l1.Y - viewport_y, 
								(int)l1.widths[p2], 
								l1.height
								)
						);
				#endif

				owner.Invalidate(new Rectangle (
					(int)l1.widths[p1] + l1.align_shift - viewport_x, 
					l1.Y - viewport_y, 
					endpoint - (int)l1.widths[p1] + 1, 
					l1.height));
				return;
			}

			#if Debug
				Console.WriteLine("Invaliding from {0}:{1} to {2}:{3} Start  => x={4}, y={5}, {6}x{7}", l1.line_no, p1, l2.line_no, p2, (int)l1.widths[p1] + l1.align_shift - viewport_x, l1.Y - viewport_y, viewport_width, l1.height);
				Console.WriteLine ("invalidate start line:  {0}  position:  {1}", l1.text, p1);
			#endif

			// Three invalidates:
			// First line from start
			owner.Invalidate(new Rectangle((int)l1.widths[p1] + l1.align_shift - viewport_x, l1.Y - viewport_y, viewport_width, l1.height));

			
			// lines inbetween
			if ((l1.line_no + 1) < l2.line_no) {
				int	y;

				y = GetLine(l1.line_no + 1).Y;
				owner.Invalidate(new Rectangle(0, y - viewport_y, viewport_width, l2.Y - y));

				#if Debug
					Console.WriteLine("Invaliding from {0}:{1} to {2}:{3} Middle => x={4}, y={5}, {6}x{7}", l1.line_no, p1, l2.line_no, p2, 0, y - viewport_y, viewport_width, l2.Y - y);
				#endif
			}
			

			// Last line to end
			owner.Invalidate(new Rectangle((int)l2.widths[0] + l2.align_shift - viewport_x, l2.Y - viewport_y, (int)l2.widths[p2] + 1, l2.height));
			#if Debug
				Console.WriteLine("Invaliding from {0}:{1} to {2}:{3} End    => x={4}, y={5}, {6}x{7}", l1.line_no, p1, l2.line_no, p2, (int)l2.widths[0] + l2.align_shift - viewport_x, l2.Y - viewport_y, (int)l2.widths[p2] + 1, l2.height);

			#endif
		}

		/// <summary>Select text around caret</summary>
		internal void ExpandSelection(CaretSelection mode, bool to_caret) {
			if (to_caret) {
				// We're expanding the selection to the caret position
				switch(mode) {
					case CaretSelection.Line: {
						// Invalidate the selection delta
						if (caret > selection_prev) {
							Invalidate(selection_prev.line, 0, caret.line, caret.line.text.Length);
						} else {
							Invalidate(selection_prev.line, selection_prev.line.text.Length, caret.line, 0);
						}

						if (caret.line.line_no <= selection_anchor.line.line_no) {
							selection_start.line = caret.line;
							selection_start.tag = caret.line.tags;
							selection_start.pos = 0;

							selection_end.line = selection_anchor.line;
							selection_end.tag = selection_anchor.tag;
							selection_end.pos = selection_anchor.pos;

							selection_end_anchor = true;
						} else {
							selection_start.line = selection_anchor.line;
							selection_start.pos = selection_anchor.height;
							selection_start.tag = selection_anchor.line.FindTag(selection_anchor.height);

							selection_end.line = caret.line;
							selection_end.tag = caret.line.tags;
							selection_end.pos = caret.line.text.Length;

							selection_end_anchor = false;
						}
						selection_prev.line = caret.line;
						selection_prev.tag = caret.tag;
						selection_prev.pos = caret.pos;

						break;
					}

					case CaretSelection.Word: {
						int	start_pos;
						int	end_pos;

						start_pos = FindWordSeparator(caret.line, caret.pos, false);
						end_pos = FindWordSeparator(caret.line, caret.pos, true);

						
						// Invalidate the selection delta
						if (caret > selection_prev) {
							Invalidate(selection_prev.line, selection_prev.pos, caret.line, end_pos);
						} else {
							Invalidate(selection_prev.line, selection_prev.pos, caret.line, start_pos);
						}
						if (caret < selection_anchor) {
							selection_start.line = caret.line;
							selection_start.tag = caret.line.FindTag(start_pos);
							selection_start.pos = start_pos;

							selection_end.line = selection_anchor.line;
							selection_end.tag = selection_anchor.tag;
							selection_end.pos = selection_anchor.pos;

							selection_prev.line = caret.line;
							selection_prev.tag = caret.tag;
							selection_prev.pos = start_pos;

							selection_end_anchor = true;
						} else {
							selection_start.line = selection_anchor.line;
							selection_start.pos = selection_anchor.height;
							selection_start.tag = selection_anchor.line.FindTag(selection_anchor.height);

							selection_end.line = caret.line;
							selection_end.tag = caret.line.FindTag(end_pos);
							selection_end.pos = end_pos;

							selection_prev.line = caret.line;
							selection_prev.tag = caret.tag;
							selection_prev.pos = end_pos;

							selection_end_anchor = false;
						}
						break;
					}

					case CaretSelection.Position: {
						SetSelectionToCaret(false);
						return;
					}
				}
			} else {
				// We're setting the selection 'around' the caret position
				switch(mode) {
					case CaretSelection.Line: {
						this.Invalidate(caret.line, 0, caret.line, caret.line.text.Length);

						selection_start.line = caret.line;
						selection_start.tag = caret.line.tags;
						selection_start.pos = 0;

						selection_end.line = caret.line;
						selection_end.pos = caret.line.text.Length;
						selection_end.tag = caret.line.FindTag(selection_end.pos);

						selection_anchor.line = selection_end.line;
						selection_anchor.tag = selection_end.tag;
						selection_anchor.pos = selection_end.pos;
						selection_anchor.height = 0;

						selection_prev.line = caret.line;
						selection_prev.tag = caret.tag;
						selection_prev.pos = caret.pos;

						this.selection_end_anchor = true;

						break;
					}

					case CaretSelection.Word: {
						int	start_pos;
						int	end_pos;

						start_pos = FindWordSeparator(caret.line, caret.pos, false);
						end_pos = FindWordSeparator(caret.line, caret.pos, true);

						this.Invalidate(selection_start.line, start_pos, caret.line, end_pos);

						selection_start.line = caret.line;
						selection_start.tag = caret.line.FindTag(start_pos);
						selection_start.pos = start_pos;

						selection_end.line = caret.line;
						selection_end.tag = caret.line.FindTag(end_pos);
						selection_end.pos = end_pos;

						selection_anchor.line = selection_end.line;
						selection_anchor.tag = selection_end.tag;
						selection_anchor.pos = selection_end.pos;
						selection_anchor.height = start_pos;

						selection_prev.line = caret.line;
						selection_prev.tag = caret.tag;
						selection_prev.pos = caret.pos;

						this.selection_end_anchor = true;

						break;
					}
				}
			}

			SetSelectionVisible (!(selection_start == selection_end));
		}

		internal void SetSelectionToCaret(bool start) {
			if (start) {
				// Invalidate old selection; selection is being reset to empty
				this.Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);

				selection_start.line = caret.line;
				selection_start.tag = caret.tag;
				selection_start.pos = caret.pos;

				// start always also selects end
				selection_end.line = caret.line;
				selection_end.tag = caret.tag;
				selection_end.pos = caret.pos;

				selection_anchor.line = caret.line;
				selection_anchor.tag = caret.tag;
				selection_anchor.pos = caret.pos;
			} else {
				// Invalidate from previous end to caret (aka new end)
				if (selection_end_anchor) {
					if (selection_start != caret) {
						this.Invalidate(selection_start.line, selection_start.pos, caret.line, caret.pos);
					}
				} else {
					if (selection_end != caret) {
						this.Invalidate(selection_end.line, selection_end.pos, caret.line, caret.pos);
					}
				}

				if (caret < selection_anchor) {
					selection_start.line = caret.line;
					selection_start.tag = caret.tag;
					selection_start.pos = caret.pos;

					selection_end.line = selection_anchor.line;
					selection_end.tag = selection_anchor.tag;
					selection_end.pos = selection_anchor.pos;

					selection_end_anchor = true;
				} else {
					selection_start.line = selection_anchor.line;
					selection_start.tag = selection_anchor.tag;
					selection_start.pos = selection_anchor.pos;

					selection_end.line = caret.line;
					selection_end.tag = caret.tag;
					selection_end.pos = caret.pos;

					selection_end_anchor = false;
				}
			}

			SetSelectionVisible (!(selection_start == selection_end));
		}

		internal void SetSelection(Line start, int start_pos, Line end, int end_pos) {
			if (selection_visible) {
				Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
			}

			if ((end.line_no < start.line_no) || ((end == start) && (end_pos <= start_pos))) {
				selection_start.line = end;
				selection_start.tag = LineTag.FindTag(end, end_pos);
				selection_start.pos = end_pos;

				selection_end.line = start;
				selection_end.tag = LineTag.FindTag(start, start_pos);
				selection_end.pos = start_pos;

				selection_end_anchor = true;
			} else {
				selection_start.line = start;
				selection_start.tag = LineTag.FindTag(start, start_pos);
				selection_start.pos = start_pos;

				selection_end.line = end;
				selection_end.tag = LineTag.FindTag(end, end_pos);
				selection_end.pos = end_pos;

				selection_end_anchor = false;
			}

			selection_anchor.line = start;
			selection_anchor.tag = selection_start.tag;
			selection_anchor.pos = start_pos;

			if (((start == end) && (start_pos == end_pos)) || start == null || end == null) {
				SetSelectionVisible (false);
			} else {
				SetSelectionVisible (true);
				Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
			}
		}

		internal void SetSelectionStart(Line start, int start_pos) {
			// Invalidate from the previous to the new start pos
			Invalidate(selection_start.line, selection_start.pos, start, start_pos);

			selection_start.line = start;
			selection_start.pos = start_pos;
			selection_start.tag = LineTag.FindTag(start, start_pos);

			selection_anchor.line = start;
			selection_anchor.pos = start_pos;
			selection_anchor.tag = selection_start.tag;

			selection_end_anchor = false;

			
			if ((selection_end.line != selection_start.line) || (selection_end.pos != selection_start.pos)) {
				SetSelectionVisible (true);
			} else {
				SetSelectionVisible (false);
			}

			Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
		}

		internal void SetSelectionStart(int character_index) {
			Line	line;
			LineTag	tag;
			int	pos;

			if (character_index < 0) {
				return;
			}

			CharIndexToLineTag(character_index, out line, out tag, out pos);
			SetSelectionStart(line, pos);
		}

		internal void SetSelectionEnd(Line end, int end_pos) {

			if (end == selection_end.line && end_pos == selection_start.pos) {
				selection_anchor.line = selection_start.line;
				selection_anchor.tag = selection_start.tag;
				selection_anchor.pos = selection_start.pos;

				selection_end.line = selection_start.line;
				selection_end.tag = selection_start.tag;
				selection_end.pos = selection_start.pos;

				selection_end_anchor = false;
			} else if ((end.line_no < selection_anchor.line.line_no) || ((end == selection_anchor.line) && (end_pos <= selection_anchor.pos))) {
				selection_start.line = end;
				selection_start.tag = LineTag.FindTag(end, end_pos);
				selection_start.pos = end_pos;

				selection_end.line = selection_anchor.line;
				selection_end.tag = selection_anchor.tag;
				selection_end.pos = selection_anchor.pos;

				selection_end_anchor = true;
			} else {
				selection_start.line = selection_anchor.line;
				selection_start.tag = selection_anchor.tag;
				selection_start.pos = selection_anchor.pos;

				selection_end.line = end;
				selection_end.tag = LineTag.FindTag(end, end_pos);
				selection_end.pos = end_pos;

				selection_end_anchor = false;
			}

			if ((selection_end.line != selection_start.line) || (selection_end.pos != selection_start.pos)) {
				SetSelectionVisible (true);
				Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
			} else {
				SetSelectionVisible (false);
				// ?? Do I need to invalidate here, tests seem to work without it, but I don't think they should :-s
			}
		}

		internal void SetSelectionEnd(int character_index) {
			Line	line;
			LineTag	tag;
			int	pos;

			if (character_index < 0) {
				return;
			}

			CharIndexToLineTag(character_index, out line, out tag, out pos);
			SetSelectionEnd(line, pos);
		}

		internal void SetSelection(Line start, int start_pos) {
			if (selection_visible) {
				Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
			}

			selection_start.line = start;
			selection_start.pos = start_pos;
			selection_start.tag = LineTag.FindTag(start, start_pos);

			selection_end.line = start;
			selection_end.tag = selection_start.tag;
			selection_end.pos = start_pos;

			selection_anchor.line = start;
			selection_anchor.tag = selection_start.tag;
			selection_anchor.pos = start_pos;

			selection_end_anchor = false;
			SetSelectionVisible (false);
		}

		internal void InvalidateSelectionArea() {
			Invalidate (selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
		}

		// Return the current selection, as string
		internal string GetSelection() {
			// We return String.Empty if there is no selection
			if ((selection_start.pos == selection_end.pos) && (selection_start.line == selection_end.line)) {
				return string.Empty;
			}

			if (!multiline || (selection_start.line == selection_end.line)) {
				return selection_start.line.text.ToString(selection_start.pos, selection_end.pos - selection_start.pos);
			} else {
				StringBuilder	sb;
				int		i;
				int		start;
				int		end;

				sb = new StringBuilder();
				start = selection_start.line.line_no;
				end = selection_end.line.line_no;

				sb.Append(selection_start.line.text.ToString(selection_start.pos, selection_start.line.text.Length - selection_start.pos) + Environment.NewLine);

				if ((start + 1) < end) {
					for (i = start + 1; i < end; i++) {
						sb.Append(GetLine(i).text.ToString() + Environment.NewLine);
					}
				}

				sb.Append(selection_end.line.text.ToString(0, selection_end.pos));

				return sb.ToString();
			}
		}

		internal void ReplaceSelection(string s, bool select_new) {
			int		i;

			int selection_pos_on_line = selection_start.pos;
			int selection_start_pos = LineTagToCharIndex (selection_start.line, selection_start.pos);
			SuspendRecalc ();

			// First, delete any selected text
			if ((selection_start.pos != selection_end.pos) || (selection_start.line != selection_end.line)) {
				if (!multiline || (selection_start.line == selection_end.line)) {
					undo.RecordDeleteString (selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);

					DeleteChars(selection_start.tag, selection_start.pos, selection_end.pos - selection_start.pos);

					// The tag might have been removed, we need to recalc it
					selection_start.tag = selection_start.line.FindTag(selection_start.pos);
				} else {
					int		start;
					int		end;

					start = selection_start.line.line_no;
					end = selection_end.line.line_no;

					undo.RecordDeleteString (selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);

					// Delete first line
					DeleteChars(selection_start.tag, selection_start.pos, selection_start.line.text.Length - selection_start.pos);

					// Delete last line
					DeleteChars(selection_end.line.tags, 0, selection_end.pos);

					start++;
					if (start < end) {
						for (i = end - 1; i >= start; i--) {
							Delete(i);
						}
					}

					// BIG FAT WARNING - selection_end.line might be stale due 
					// to the above Delete() call. DONT USE IT before hitting the end of this method!

					// Join start and end
					Combine(selection_start.line.line_no, start);
				}
			}


			Insert(selection_start.line, selection_start.pos, false, s);
			undo.RecordInsertString (selection_start.line, selection_start.pos, s);
			ResumeRecalc (false);

			if (!select_new) {
				CharIndexToLineTag(selection_start_pos + s.Length, out selection_start.line,
						out selection_start.tag, out selection_start.pos);

				selection_end.line = selection_start.line;
				selection_end.pos = selection_start.pos;
				selection_end.tag = selection_start.tag;
				selection_anchor.line = selection_start.line;
				selection_anchor.pos = selection_start.pos;
				selection_anchor.tag = selection_start.tag;

				SetSelectionVisible (false);
			} else {
				CharIndexToLineTag(selection_start_pos, out selection_start.line,
						out selection_start.tag, out selection_start.pos);

				CharIndexToLineTag(selection_start_pos + s.Length, out selection_end.line,
						out selection_end.tag, out selection_end.pos);

				selection_anchor.line = selection_start.line;
				selection_anchor.pos = selection_start.pos;
				selection_anchor.tag = selection_start.tag;

				SetSelectionVisible (true);
			}

			PositionCaret (selection_start.line, selection_start.pos);
			UpdateView (selection_start.line, selection_pos_on_line);
		}

		internal void CharIndexToLineTag(int index, out Line line_out, out LineTag tag_out, out int pos) {
			Line	line;
			LineTag	tag;
			int	i;
			int	chars;
			int	start;

			chars = 0;

			for (i = 1; i <= lines; i++) {
				line = GetLine(i);

				start = chars;
				chars += line.text.Length + (line.soft_break ? 0 : crlf_size);

				if (index <= chars) {
					// we found the line
					tag = line.tags;

					while (tag != null) {
						if (index < (start + tag.start + tag.length)) {
							line_out = line;
							tag_out = LineTag.GetFinalTag (tag);
							pos = index - start;
							return;
						}
						if (tag.next == null) {
							Line	next_line;

							next_line = GetLine(line.line_no + 1);

							if (next_line != null) {
								line_out = next_line;
								tag_out = LineTag.GetFinalTag (next_line.tags);
								pos = 0;
								return;
							} else {
								line_out = line;
								tag_out = LineTag.GetFinalTag (tag);
								pos = line_out.text.Length;
								return;
							}
						}
						tag = tag.next;
					}
				}
			}

			line_out = GetLine(lines);
			tag = line_out.tags;
			while (tag.next != null) {
				tag = tag.next;
			}
			tag_out = tag;
			pos = line_out.text.Length;
		}

		internal int LineTagToCharIndex(Line line, int pos) {
			int	i;
			int	length;

			// Count first and last line
			length = 0;

			// Count the lines in the middle

			for (i = 1; i < line.line_no; i++) {
				length += GetLine(i).text.Length + (line.soft_break ? 0 : crlf_size);
			}

			length += pos;

			return length;
		}

		internal int SelectionLength() {
			if ((selection_start.pos == selection_end.pos) && (selection_start.line == selection_end.line)) {
				return 0;
			}

			if (!multiline || (selection_start.line == selection_end.line)) {
				return selection_end.pos - selection_start.pos;
			} else {
				int	i;
				int	start;
				int	end;
				int	length;

				// Count first and last line
				length = selection_start.line.text.Length - selection_start.pos + selection_end.pos + crlf_size;

				// Count the lines in the middle
				start = selection_start.line.line_no + 1;
				end = selection_end.line.line_no;

				if (start < end) {
					for (i = start; i < end; i++) {
						Line line = GetLine (i);
						length += line.text.Length + (line.soft_break ? 0 : crlf_size);
					}
				}

				return length;
			}

			
		}


		/// <summary>Give it a Line number and it returns the Line object at with that line number</summary>
		internal Line GetLine(int LineNo) {
			Line	line = document;

			while (line != sentinel) {
				if (LineNo == line.line_no) {
					return line;
				} else if (LineNo < line.line_no) {
					line = line.left;
				} else {
					line = line.right;
				}
			}

			return null;
		}

		/// <summary>Retrieve the previous tag; walks line boundaries</summary>
		internal LineTag PreviousTag(LineTag tag) {
			Line l; 

			if (tag.previous != null) {
				return tag.previous;
			}

			// Next line 
			if (tag.line.line_no == 1) {
				return null;
			}

			l = GetLine(tag.line.line_no - 1);
			if (l != null) {
				LineTag t;

				t = l.tags;
				while (t.next != null) {
					t = t.next;
				}
				return t;
			}

			return null;
		}

		/// <summary>Retrieve the next tag; walks line boundaries</summary>
		internal LineTag NextTag(LineTag tag) {
			Line l;

			if (tag.next != null) {
				return tag.next;
			}

			// Next line
			l = GetLine(tag.line.line_no + 1);
			if (l != null) {
				return l.tags;
			}

			return null;
		}

		internal Line ParagraphStart(Line line) {
			while (line.soft_break) {
				line = GetLine(line.line_no - 1);
			}
			return line;
		}       

		internal Line ParagraphEnd(Line line) {
			Line    l;
   
			while (line.soft_break) {
				l = GetLine(line.line_no + 1);
				if ((l == null) || (!l.soft_break)) {
					break;
				}
				line = l;
			}
			return line;
		}

		/// <summary>Give it a Y pixel coordinate and it returns the Line covering that Y coordinate</summary>
		internal Line GetLineByPixel(int y, bool exact) {
			Line	line = document;
			Line	last = null;

			while (line != sentinel) {
				last = line;
				if ((y >= line.Y) && (y < (line.Y+line.height))) {
					return line;
				} else if (y < line.Y) {
					line = line.left;
				} else {
					line = line.right;
				}
			}

			if (exact) {
				return null;
			}
			return last;
		}

		// Give it x/y pixel coordinates and it returns the Tag at that position; optionally the char position is returned in index
		internal LineTag FindTag(int x, int y, out int index, bool exact) {
			Line	line;
			LineTag	tag;

			line = GetLineByPixel(y, exact);
			if (line == null) {
				index = 0;
				return null;
			}
			tag = line.tags;

			// Alignment adjustment
			x += line.align_shift;

			while (true) {
				if (x >= tag.X && x < (tag.X+tag.width)) {
					int	end;

					end = tag.start + tag.length - 1;

					for (int pos = tag.start; pos < end; pos++) {
						if (x < line.widths[pos]) {
							index = pos;
							return LineTag.GetFinalTag (tag);
						}
					}
					index=end;
					return LineTag.GetFinalTag (tag);
				}
				if (tag.next != null) {
					tag = tag.next;
				} else {
					if (exact) {
						index = 0;
						return null;
					}

					index = line.text.Length;
					return LineTag.GetFinalTag (tag);
				}
			}
		}

		// Give it x/y pixel coordinates and it returns the Tag at that position; optionally the char position is returned in index
		internal LineTag FindCursor(int x, int y, out int index) {
			Line	line;
			LineTag	tag;

			line = GetLineByPixel(y, false);
			tag = line.tags;

			// Adjust for alignment
			x -= line.align_shift;

			while (true) {
				if (x >= tag.X && x < (tag.X+tag.width)) {
					int	end;

					end = tag.end;

					for (int pos = tag.start-1; pos < end; pos++) {
						// When clicking on a character, we position the cursor to whatever edge
						// of the character the click was closer
						if (x < (line.widths[pos] + ((line.widths[pos+1]-line.widths[pos])/2))) {
							index = pos;
							return tag;
						}
					}
					index=end;
					return LineTag.GetFinalTag (tag);
				}
				if (tag.next != null) {
					tag = tag.next;
				} else {
					index = line.text.Length;
					return LineTag.GetFinalTag (tag);
				}
			}
		}

		/// <summary>Format area of document in specified font and color</summary>
		/// <param name="start_pos">1-based start position on start_line</param>
		/// <param name="end_pos">1-based end position on end_line </param>
		internal void FormatText (Line start_line, int start_pos, Line end_line, int end_pos, Font font,
				SolidBrush color, SolidBrush back_color, FormatSpecified specified)
		{
			Line    l;

			// First, format the first line
			if (start_line != end_line) {
				// First line
				LineTag.FormatText(start_line, start_pos, start_line.text.Length - start_pos + 1, font, color, back_color, specified);

				// Format last line
				LineTag.FormatText(end_line, 1, end_pos, font, color, back_color, specified);

				// Now all the lines inbetween
				for (int i = start_line.line_no + 1; i < end_line.line_no; i++) {
					l = GetLine(i);
					LineTag.FormatText(l, 1, l.text.Length, font, color, back_color, specified);
				}
			} else {
				// Special case, single line
				LineTag.FormatText(start_line, start_pos, end_pos - start_pos, font, color, back_color, specified);
			}
		}

		/// <summary>Re-format areas of the document in specified font and color</summary>
		/// <param name="start_pos">1-based start position on start_line</param>
		/// <param name="end_pos">1-based end position on end_line </param>
		/// <param name="font">Font specifying attributes</param>
		/// <param name="color">Color (or NULL) to apply</param>
		/// <param name="apply">Attributes from font and color to apply</param>
		internal void FormatText(Line start_line, int start_pos, Line end_line, int end_pos, FontDefinition attributes) {
			Line    l;

			// First, format the first line
			if (start_line != end_line) {
				// First line
				LineTag.FormatText(start_line, start_pos, start_line.text.Length - start_pos + 1, attributes);

				// Format last line
				LineTag.FormatText(end_line, 1, end_pos - 1, attributes);

				// Now all the lines inbetween
				for (int i = start_line.line_no + 1; i < end_line.line_no; i++) {
					l = GetLine(i);
					LineTag.FormatText(l, 1, l.text.Length, attributes);
				}
			} else {
				// Special case, single line
				LineTag.FormatText(start_line, start_pos, end_pos - start_pos, attributes);
			}
		}

		internal void RecalculateAlignments() {
			Line	line;
			int	line_no;

			line_no = 1;

			while (line_no <= lines) {
				line = GetLine(line_no);

				if (line != null) {
					switch (line.alignment) {
					case HorizontalAlignment.Left:
						line.align_shift = 0;
						break;
					case HorizontalAlignment.Center:
 						line.align_shift = (viewport_width - (int)line.widths[line.text.Length]) / 2;
						break;
					case HorizontalAlignment.Right:
 						line.align_shift = viewport_width - (int)line.widths[line.text.Length];
						break;
					}
				}

				line_no++;
			}
			return;
		}

		/// <summary>Calculate formatting for the whole document</summary>
		internal bool RecalculateDocument(Graphics g) {
			return RecalculateDocument(g, 1, this.lines, false);
		}

		/// <summary>Calculate formatting starting at a certain line</summary>
		internal bool RecalculateDocument(Graphics g, int start) {
			return RecalculateDocument(g, start, this.lines, false);
		}

		/// <summary>Calculate formatting within two given line numbers</summary>
		internal bool RecalculateDocument(Graphics g, int start, int end) {
			return RecalculateDocument(g, start, end, false);
		}

		/// <summary>With optimize on, returns true if line heights changed</summary>
		internal bool RecalculateDocument(Graphics g, int start, int end, bool optimize) {
			Line	line;
			int	line_no;
			int	Y;
			int	new_width;
			bool	changed;
			int	shift;

			if (recalc_suspended > 0) {
				recalc_pending = true;
				recalc_start = Math.Min (recalc_start, start);
				recalc_end = Math.Max (recalc_end, end);
				recalc_optimize = optimize;
				return false;
			}

			// Fixup the positions, they can go kinda nuts
			start = Math.Max (start, 1);
			end = Math.Min (end, lines);

			Y = GetLine(start).Y;
			line_no = start;
			new_width = 0;
			shift = this.lines;
			if (!optimize) {
				changed = true;		// We always return true if we run non-optimized
			} else {
				changed = false;
			}

			while (line_no <= (end + this.lines - shift)) {
				line = GetLine(line_no++);
				line.Y = Y;

				if (!calc_pass) {
					if (!optimize) {
						line.RecalculateLine(g, this);
					} else {
						if (line.recalc && line.RecalculateLine(g, this)) {
							changed = true;
							// If the height changed, all subsequent lines change
							end = this.lines;
							shift = this.lines;
						}
					}
				} else {
					if (!optimize) {
						line.RecalculatePasswordLine(g, this);
					} else {
						if (line.recalc && line.RecalculatePasswordLine(g, this)) {
							changed = true;
							// If the height changed, all subsequent lines change
							end = this.lines;
							shift = this.lines;
						}
					}
				}

				if (line.widths[line.text.Length] > new_width) {
					new_width = (int)line.widths[line.text.Length];
				}

				// Calculate alignment
				if (line.alignment != HorizontalAlignment.Left) {
					if (line.alignment == HorizontalAlignment.Center) {
						line.align_shift = (viewport_width - (int)line.widths[line.text.Length]) / 2;
					} else {
						line.align_shift = viewport_width - (int)line.widths[line.text.Length] - 1;
					}
				}

				Y += line.height;

				if (line_no > lines) {
					break;
				}
			}

			if (document_x != new_width) {
				document_x = new_width;
				if (WidthChanged != null) {
					WidthChanged(this, null);
				}
			}

			RecalculateAlignments();

			line = GetLine(lines);

			if (document_y != line.Y + line.height) {
				document_y = line.Y + line.height;
				if (HeightChanged != null) {
					HeightChanged(this, null);
				}
			}
			UpdateCaret();
			return changed;
		}

		internal int Size() {
			return lines;
		}

		private void owner_HandleCreated(object sender, EventArgs e) {
			RecalculateDocument(owner.CreateGraphicsInternal());
			AlignCaret();
		}

		private void owner_VisibleChanged(object sender, EventArgs e) {
			if (owner.Visible) {
				RecalculateDocument(owner.CreateGraphicsInternal());
			}
		}

		internal static bool IsWordSeparator(char ch) {
			switch(ch) {
				case ' ':
				case '\t':
				case '(':
				case ')': {
					return true;
				}

				default: {
					return false;
				}
			}
		}
		internal int FindWordSeparator(Line line, int pos, bool forward) {
			int len;

			len = line.text.Length;

			if (forward) {
				for (int i = pos + 1; i < len; i++) {
					if (IsWordSeparator(line.Text[i])) {
						return i + 1;
					}
				}
				return len;
			} else {
				for (int i = pos - 1; i > 0; i--) {
					if (IsWordSeparator(line.Text[i - 1])) {
						return i;
					}
				}
				return 0;
			}
		}

		/* Search document for text */
		internal bool FindChars(char[] chars, Marker start, Marker end, out Marker result) {
			Line	line;
			int	line_no;
			int	pos;
			int	line_len;

			// Search for occurence of any char in the chars array
			result = new Marker();

			line = start.line;
			line_no = start.line.line_no;
			pos = start.pos;
			while (line_no <= end.line.line_no) {
				line_len = line.text.Length;
				while (pos < line_len) {
					for (int i = 0; i < chars.Length; i++) {
						if (line.text[pos] == chars[i]) {
							// Special case
							if ((line.line_no == end.line.line_no) && (pos >= end.pos)) {
								return false;
							}

							result.line = line;
							result.pos = pos;
							return true;
						}
					}
					pos++;
				}

				pos = 0;
				line_no++;
				line = GetLine(line_no);
			}

			return false;
		}

		// This version does not build one big string for searching, instead it handles 
		// line-boundaries, which is faster and less memory intensive
		// FIXME - Depending on culture stuff we might have to create a big string and use culturespecific 
		// search stuff and change it to accept and return positions instead of Markers (which would match 
		// RichTextBox behaviour better but would be inconsistent with the rest of TextControl)
		internal bool Find(string search, Marker start, Marker end, out Marker result, RichTextBoxFinds options) {
			Marker	last;
			string	search_string;
			Line	line;
			int	line_no;
			int	pos;
			int	line_len;
			int	current;
			bool	word;
			bool	word_option;
			bool	ignore_case;
			bool	reverse;
			char	c;

			result = new Marker();
			word_option = ((options & RichTextBoxFinds.WholeWord) != 0);
			ignore_case = ((options & RichTextBoxFinds.MatchCase) == 0);
			reverse = ((options & RichTextBoxFinds.Reverse) != 0);

			line = start.line;
			line_no = start.line.line_no;
			pos = start.pos;
			current = 0;

			// Prep our search string, lowercasing it if we do case-independent matching
			if (ignore_case) {
				StringBuilder	sb;
				sb = new StringBuilder(search);
				for (int i = 0; i < sb.Length; i++) {
					sb[i] = Char.ToLower(sb[i]);
				}
				search_string = sb.ToString();
			} else {
				search_string = search;
			}

			// We need to check if the character before our start position is a wordbreak
			if (word_option) {
				if (line_no == 1) {
					if ((pos == 0) || (IsWordSeparator(line.text[pos - 1]))) {
						word = true;
					} else {
						word = false;
					}
				} else {
					if (pos > 0) {
						if (IsWordSeparator(line.text[pos - 1])) {
							word = true;
						} else {
							word = false;
						}
					} else {
						// Need to check the end of the previous line
						Line	prev_line;

						prev_line = GetLine(line_no - 1);
						if (prev_line.soft_break) {
							if (IsWordSeparator(prev_line.text[prev_line.text.Length - 1])) {
								word = true;
							} else {
								word = false;
							}
						} else {
							word = true;
						}
					}
				}
			} else {
				word = false;
			}

			// To avoid duplication of this loop with reverse logic, we search
			// through the document, remembering the last match and when returning
			// report that last remembered match

			last = new Marker();
			last.height = -1;	// Abused - we use it to track change

			while (line_no <= end.line.line_no) {
				if (line_no != end.line.line_no) {
					line_len = line.text.Length;
				} else {
					line_len = end.pos;
				}

				while (pos < line_len) {
					if (word_option && (current == search_string.Length)) {
						if (IsWordSeparator(line.text[pos])) {
							if (!reverse) {
								goto FindFound;
							} else {
								last = result;
								current = 0;
							}
						} else {
							current = 0;
						}
					}

					if (ignore_case) {
						c = Char.ToLower(line.text[pos]);
					} else {
						c = line.text[pos];
					}

					if (c == search_string[current]) {
						if (current == 0) {
							result.line = line;
							result.pos = pos;
						}
						if (!word_option || (word_option && (word || (current > 0)))) {
							current++;
						}

						if (!word_option && (current == search_string.Length)) {
							if (!reverse) {
								goto FindFound;
							} else {
								last = result;
								current = 0;
							}
						}
					} else {
						current = 0;
					}
					pos++;

					if (!word_option) {
						continue;
					}

					if (IsWordSeparator(c)) {
						word = true;
					} else {
						word = false;
					}
				}

				if (word_option) {
					// Mark that we just saw a word boundary
					if (!line.soft_break) {
						word = true;
					}

					if (current == search_string.Length) {
						if (word) {
							if (!reverse) {
								goto FindFound;
							} else {
								last = result;
								current = 0;
							}
						} else {
							current = 0;
						}
					}
				}

				pos = 0;
				line_no++;
				line = GetLine(line_no);
			}

			if (reverse) {
				if (last.height != -1) {
					result = last;
					return true;
				}
			}

			return false;

			FindFound:
			if (!reverse) {
//				if ((line.line_no == end.line.line_no) && (pos >= end.pos)) {
//					return false;
//				}
				return true;
			}

			result = last;
			return true;

		}

		/* Marker stuff */
		internal void GetMarker(out Marker mark, bool start) {
			mark = new Marker();

			if (start) {
				mark.line = GetLine(1);
				mark.tag = mark.line.tags;
				mark.pos = 0;
			} else {
				mark.line = GetLine(lines);
				mark.tag = mark.line.tags;
				while (mark.tag.next != null) {
					mark.tag = mark.tag.next;
				}
				mark.pos = mark.line.text.Length;
			}
		}
		#endregion	// Internal Methods

		#region Events
		internal event EventHandler CaretMoved;
		internal event EventHandler WidthChanged;
		internal event EventHandler HeightChanged;
		internal event EventHandler LengthChanged;
		#endregion	// Events

		#region Administrative
		public IEnumerator GetEnumerator() {
			// FIXME
			return null;
		}

		public override bool Equals(object obj) {
			if (obj == null) {
				return false;
			}

			if (!(obj is Document)) {
				return false;
			}

			if (obj == this) {
				return true;
			}

			if (ToString().Equals(((Document)obj).ToString())) {
				return true;
			}

			return false;
		}

		public override int GetHashCode() {
			return document_id;
		}

		public override string ToString() {
			return "document " + this.document_id;
		}
		#endregion	// Administrative
	}

	internal class ImageTag : LineTag {

		internal Image image;

		internal ImageTag (Line line, int start, Image image) : base (line, start)
		{
			this.image = image;
		}

		internal override SizeF SizeOfPosition (Graphics dc, int pos)
		{
			return image.Size;
		}

		internal override int MaxHeight ()
		{
			return image.Height;
		}

		internal override void Draw (Graphics dc, Brush brush, float x, float y, int start, int end)
		{
			dc.DrawImage (image, x, y);
		}
	}

	internal class LineTag {
		#region	Local Variables;
		// Payload; formatting
		internal Font		font;		// System.Drawing.Font object for this tag
		internal SolidBrush	color;		// The font color for this tag

		// In 2.0 tags can have background colours.  I'm not going to #ifdef
		// at this level though since I want to reduce code paths
		internal SolidBrush back_color;  

		// Payload; text
		internal int		start;		// start, in chars; index into Line.text
		internal bool		r_to_l;		// Which way is the font

		// Drawing support
		internal int		height;		// Height in pixels of the text this tag describes

		internal int		ascent;		// Ascent of the font for this tag
		internal int		shift;		// Shift down for this tag, to stay on baseline

		// Administrative
		internal Line		line;		// The line we're on
		internal LineTag	next;		// Next tag on the same line
		internal LineTag	previous;	// Previous tag on the same line
		#endregion;

		#region Constructors
		internal LineTag(Line line, int start) {
			this.line = line;
			this.start = start;
		}
		#endregion	// Constructors

		#region Internal Methods

		public float X {
			get {
				if (start == 0)
					return line.indent;
				return line.widths [start - 1];
			}
		}

		public int end {
			get { return start + length; }
		}

		public float width {
			get {
				if (length == 0)
					return 0;
				return line.widths [start + length - 1] - line.widths [start - 1];
			}
		}

		public int length {
			get {
				int res = 0;
				if (next != null)
					res = next.start - start;
				else
					res = line.text.Length - (start - 1);

				return res > 0 ? res : 0;
			}
		}

		internal virtual SizeF SizeOfPosition (Graphics dc, int pos)
		{
			return dc.MeasureString (line.text.ToString (pos, 1), font, 10000, Document.string_format);
		}

		internal virtual int MaxHeight ()
		{
			return font.Height;
		}

		internal virtual void Draw (Graphics dc, Brush brush, float x, float y, int start, int end)
		{
			dc.DrawString (line.text.ToString (start, end), font, brush, x, y, StringFormat.GenericTypographic);
		}

		internal virtual void Draw (Graphics dc, Brush brush, float x, float y, int start, int end, string text) {
			dc.DrawString (text.Substring (start, end), font, brush, x, y, StringFormat.GenericTypographic);
		}

		///<summary>Break a tag into two with identical attributes; pos is 1-based; returns tag starting at &gt;pos&lt; or null if end-of-line</summary>
		internal LineTag Break(int pos) {

			LineTag	new_tag;

			// Sanity
			if (pos == this.start) {
				return this;
			} else if (pos >= (start + length)) {
				return null;
			}

			new_tag = new LineTag(line, pos);
			new_tag.CopyFormattingFrom (this);

			new_tag.next = this.next;
			this.next = new_tag;
			new_tag.previous = this;

			if (new_tag.next != null) {
				new_tag.next.previous = new_tag;
			}

			return new_tag;
		}

		public string Text ()
		{
			return line.text.ToString (start - 1, length);
		}

		public void CopyFormattingFrom (LineTag other)
		{
			height = other.height;
			font = other.font;
			color = other.color;
			back_color = other.back_color;
		}

		///<summary>Create new font and brush from existing font and given new attributes. Returns true if fontheight changes</summary>
		internal static bool GenerateTextFormat(Font font_from, SolidBrush color_from, FontDefinition attributes, out Font new_font, out SolidBrush new_color) {
			float		size;
			string		face;
			FontStyle	style;
			GraphicsUnit	unit;

			if (attributes.font_obj == null) {
				size = font_from.SizeInPoints;
				unit = font_from.Unit;
				face = font_from.Name;
				style = font_from.Style;

				if (attributes.face != null) {
					face = attributes.face;
				}
				
				if (attributes.size != 0) {
					size = attributes.size;
				}

				style |= attributes.add_style;
				style &= ~attributes.remove_style;

				// Create new font
				new_font = new Font(face, size, style, unit);
			} else {
				new_font = attributes.font_obj;
			}

			// Create 'new' color brush
			if (attributes.color != Color.Empty) {
				new_color = new SolidBrush(attributes.color);
			} else {
				new_color = color_from;
			}

			if (new_font.Height == font_from.Height) {
				return false;
			}
			return true;
		}

		/// <summary>Applies 'font' and 'brush' to characters starting at 'start' for 'length' chars; 
		/// Removes any previous tags overlapping the same area; 
		/// returns true if lineheight has changed</summary>
		/// <param name="start">1-based character position on line</param>
		internal static bool FormatText(Line line, int start, int length, Font font, SolidBrush color, SolidBrush back_color, FormatSpecified specified)
		{
			LineTag	tag;
			LineTag	start_tag;
			LineTag end_tag;
			int	end;
			bool	retval = false;		// Assume line-height doesn't change

			// Too simple?
			if (((FormatSpecified.Font & specified) == FormatSpecified.Font) && font.Height != line.height) {
				retval = true;
			}
			line.recalc = true;		// This forces recalculation of the line in RecalculateDocument

			// A little sanity, not sure if it's needed, might be able to remove for speed
			if (length > line.text.Length) {
				length = line.text.Length;
			}

			tag = line.tags;
			end = start + length;

			// Common special case
			if ((start == 1) && (length == tag.length)) {
				tag.ascent = 0;
				SetFormat (tag, font, color, back_color, specified);
				return retval;
			}

			start_tag = FindTag (line, start);

			tag = start_tag.Break (start);

			while (tag != null && tag.end <= end) {
				SetFormat (tag, font, color, back_color, specified);
				tag = tag.next;
			}

			if (end != line.text.Length) {
				/// Now do the last tag
				end_tag = FindTag (line, end);

				if (end_tag != null) {
					end_tag.Break (end);
					SetFormat (end_tag, font, color, back_color, specified);
				}
			}

			return retval;
		}

		private static void SetFormat (LineTag tag, Font font, SolidBrush color, SolidBrush back_color, FormatSpecified specified)
		{
			if ((FormatSpecified.Font & specified) == FormatSpecified.Font)
				tag.font = font;
			if ((FormatSpecified.Color & specified) == FormatSpecified.Color)
				tag.color = color;
			if ((FormatSpecified.BackColor & specified) == FormatSpecified.BackColor) {
				tag.back_color = back_color;
			}
			// Console.WriteLine ("setting format:   {0}  {1}   new color {2}", color.Color, specified, tag.color.Color);
		}

		/// <summary>Applies font attributes specified to characters starting at 'start' for 'length' chars; 
		/// Breaks tags at start and end point, keeping middle tags with altered attributes.
		/// Returns true if lineheight has changed</summary>
		/// <param name="start">1-based character position on line</param>
		internal static bool FormatText(Line line, int start, int length, FontDefinition attributes) {
			LineTag	tag;
			LineTag	start_tag;
			LineTag	end_tag;
			bool	retval = false;		// Assume line-height doesn't change

			line.recalc = true;		// This forces recalculation of the line in RecalculateDocument

			// A little sanity, not sure if it's needed, might be able to remove for speed
			if (length > line.text.Length) {
				length = line.text.Length;
			}

			tag = line.tags;

			// Common special case
			if ((start == 1) && (length == tag.length)) {
				tag.ascent = 0;
				GenerateTextFormat(tag.font, tag.color, attributes, out tag.font, out tag.color);
				return retval;
			}

			start_tag = FindTag(line, start);
			
			if (start_tag == null) {
				if (length == 0) {
					// We are 'starting' after all valid tags; create a new tag with the right attributes
					start_tag = FindTag(line, line.text.Length - 1);
					start_tag.next = new LineTag(line, line.text.Length + 1);
					start_tag.next.CopyFormattingFrom (start_tag);
					start_tag.next.previous = start_tag;
					start_tag = start_tag.next;
				} else {
					throw new Exception(String.Format("Could not find start_tag in document at line {0} position {1}", line.line_no, start));
				}
			} else {
				start_tag = start_tag.Break(start);
			}

			end_tag = FindTag(line, start + length);
			if (end_tag != null) {
				end_tag = end_tag.Break(start + length);
			}

			// start_tag or end_tag might be null; we're cool with that
			// we now walk from start_tag to end_tag, applying new attributes
			tag = start_tag;
			while ((tag != null) && tag != end_tag) {
				if (LineTag.GenerateTextFormat(tag.font, tag.color, attributes, out tag.font, out tag.color)) {
					retval = true;
				}
				tag = tag.next;
			}
			return retval;
		}


		/// <summary>Finds the tag that describes the character at position 'pos' on 'line'</summary>
		internal static LineTag FindTag(Line line, int pos) {
			LineTag tag = line.tags;

			// Beginning of line is a bit special
			if (pos == 0) {
				// Not sure if we should get the final tag here
				return tag;
			}

			while (tag != null) {
				if ((tag.start <= pos) && (pos <= tag.end)) {
					return GetFinalTag (tag);
				}

				tag = tag.next;
			}

			return null;
		}

		// There can be multiple tags at the same position, we want to make
		// sure we are using the very last tag at the given position
		internal static LineTag GetFinalTag (LineTag tag)
		{
			LineTag res = tag;

			while (res.next != null && res.next.length == 0)
				res = res.next;
			return res;
		}

		/// <summary>Combines 'this' tag with 'other' tag</summary>
		internal bool Combine(LineTag other) {
			if (!this.Equals(other)) {
				return false;
			}

			this.next = other.next;
			if (this.next != null) {
				this.next.previous = this;
			}

			return true;
		}


		/// <summary>Remove 'this' tag ; to be called when formatting is to be removed</summary>
		internal bool Remove() {
			if ((this.start == 1) && (this.next == null)) {
				// We cannot remove the only tag
				return false;
			}
			if (this.start != 1) {
				this.previous.next = this.next;
				this.next.previous = this.previous;
			} else {
				this.next.start = 1;
				this.line.tags = this.next;
				this.next.previous = null;
			}
			return true;
		}


		/// <summary>Checks if 'this' tag describes the same formatting options as 'obj'</summary>
		public override bool Equals(object obj) {
			LineTag	other;

			if (obj == null) {
				return false;
			}

			if (!(obj is LineTag)) {
				return false;
			}

			if (obj == this) {
				return true;
			}

			other = (LineTag)obj;

			if (this.font.Equals(other.font) && this.color.Equals(other.color)) {	// FIXME add checking for things like link or type later
				return true;
			}

			return false;
		}

		public override int GetHashCode() {
			return base.GetHashCode ();
		}

		public override string ToString() {
			if (length > 0)
				return "Tag starts at index " + this.start + " length " + this.length + " text: " + Text () + "Font " + this.font.ToString();
			return "Zero Lengthed tag at index " + this.start;
		}

		#endregion	// Internal Methods
	}

	internal class UndoManager {

		internal enum ActionType {

			Typing,

			// This is basically just cut & paste
			InsertString,
			DeleteString,

			UserActionBegin,
			UserActionEnd
		}

		internal class Action {
			internal ActionType	type;
			internal int		line_no;
			internal int		pos;
			internal object		data;
		}

		#region Local Variables
		private Document	document;
		private Stack		undo_actions;
		private Stack		redo_actions;

		private int		caret_line;
		private int		caret_pos;

		// When performing an action, we lock the queue, so that the action can't be undone
		private bool locked;
		#endregion	// Local Variables

		#region Constructors
		internal UndoManager (Document document)
		{
			this.document = document;
			undo_actions = new Stack (50);
			redo_actions = new Stack (50);
		}
		#endregion	// Constructors

		#region Properties
		internal bool CanUndo {
			get { return undo_actions.Count > 0; }
		}

		internal bool CanRedo {
			get { return redo_actions.Count > 0; }
		}

		internal string UndoActionName {
			get {
				foreach (Action action in undo_actions) {
					if (action.type == ActionType.UserActionBegin)
						return (string) action.data;
					if (action.type == ActionType.Typing)
						return Locale.GetText ("Typing");
				}
				return String.Empty;
			}
		}

		internal string RedoActionName {
			get {
				foreach (Action action in redo_actions) {
					if (action.type == ActionType.UserActionBegin)
						return (string) action.data;
					if (action.type == ActionType.Typing)
						return Locale.GetText ("Typing");
				}
				return String.Empty;
			}
		}
		#endregion	// Properties

		#region Internal Methods
		internal void Clear ()
		{
			undo_actions.Clear();
			redo_actions.Clear();
		}

		internal void Undo ()
		{
			Action action;
			bool user_action_finished = false;

			if (undo_actions.Count == 0)
				return;

			// Nuke the redo queue
			redo_actions.Clear ();

			locked = true;
			do {
				Line start;
				action = (Action) undo_actions.Pop ();

				// Put onto redo stack
				redo_actions.Push(action);

				// Do the thing
				switch(action.type) {

				case ActionType.UserActionBegin:
					user_action_finished = true;
					break;

				case ActionType.UserActionEnd:
					// noop
					break;

				case ActionType.InsertString:
					start = document.GetLine (action.line_no);
					document.SuspendUpdate ();
					document.DeleteMultiline (start, action.pos, ((string) action.data).Length + 1);
					document.PositionCaret (start, action.pos);
					document.SetSelectionToCaret (true);
					document.ResumeUpdate (true);
					break;

				case ActionType.Typing:
					start = document.GetLine (action.line_no);
					document.SuspendUpdate ();
					document.DeleteMultiline (start, action.pos, ((StringBuilder) action.data).Length);
					document.PositionCaret (start, action.pos);
					document.SetSelectionToCaret (true);
					document.ResumeUpdate (true);

					// This is an open ended operation, so only a single typing operation can be undone at once
					user_action_finished = true;
					break;

				case ActionType.DeleteString:
					start = document.GetLine (action.line_no);
					document.SuspendUpdate ();
					Insert (start, action.pos, (Line) action.data, true);
					document.ResumeUpdate (true);
					break;
				}
			} while (!user_action_finished && undo_actions.Count > 0);

			locked = false;
		}

		internal void Redo ()
		{
			Action action;
			bool user_action_finished = false;

			if (redo_actions.Count == 0)
				return;

			// You can't undo anything after redoing
			undo_actions.Clear ();

			locked = true;
			do {
				Line start;
				int start_index;

				action = (Action) redo_actions.Pop ();

				switch (action.type) {

				case ActionType.UserActionBegin:
					//  Noop
					break;

				case ActionType.UserActionEnd:
					user_action_finished = true;
					break;

				case ActionType.InsertString:
					start = document.GetLine (action.line_no);
					document.SuspendUpdate ();
					start_index = document.LineTagToCharIndex (start, action.pos);
					document.InsertString (start, action.pos, (string) action.data);
					document.CharIndexToLineTag (start_index + ((string) action.data).Length,
							out document.caret.line, out document.caret.tag,
							out document.caret.pos);
					document.UpdateCaret ();
					document.SetSelectionToCaret (true);
					document.ResumeUpdate (true);
					break;

				case ActionType.Typing:
					start = document.GetLine (action.line_no);
					document.SuspendUpdate ();
					start_index = document.LineTagToCharIndex (start, action.pos);
					document.InsertString (start, action.pos, ((StringBuilder) action.data).ToString ());
					document.CharIndexToLineTag (start_index + ((StringBuilder) action.data).Length,
							out document.caret.line, out document.caret.tag,
							out document.caret.pos);
					document.UpdateCaret ();
					document.SetSelectionToCaret (true);
					document.ResumeUpdate (true);

					// This is an open ended operation, so only a single typing operation can be undone at once
					user_action_finished = true;
					break;

				case ActionType.DeleteString:
					start = document.GetLine (action.line_no);
					document.SuspendUpdate ();
					document.DeleteMultiline (start, action.pos, ((Line) action.data).text.Length);
					document.PositionCaret (start, action.pos);
					document.SetSelectionToCaret (true);
					document.ResumeUpdate (true);

					break;
				}
			} while (!user_action_finished && redo_actions.Count > 0);

			locked = false;
		}
		#endregion	// Internal Methods

		#region Private Methods

		public void BeginUserAction (string name)
		{
			if (locked)
				return;

			Action ua = new Action ();
			ua.type = ActionType.UserActionBegin;
			ua.data = name;

			undo_actions.Push (ua);
		}

		public void EndUserAction ()
		{
			if (locked)
				return;

			Action ua = new Action ();
			ua.type = ActionType.UserActionEnd;

			undo_actions.Push (ua);
		}

		// start_pos, end_pos = 1 based
		public void RecordDeleteString (Line start_line, int start_pos, Line end_line, int end_pos)
		{
			if (locked)
				return;

			Action	a = new Action ();

			// We cant simply store the string, because then formatting would be lost
			a.type = ActionType.DeleteString;
			a.line_no = start_line.line_no;
			a.pos = start_pos;
			a.data = Duplicate (start_line, start_pos, end_line, end_pos);

			undo_actions.Push(a);
		}

		public void RecordInsertString (Line line, int pos, string str)
		{
			if (locked || str.Length == 0)
				return;

			Action a = new Action ();

			a.type = ActionType.InsertString;
			a.data = str;
			a.line_no = line.line_no;
			a.pos = pos;

			undo_actions.Push (a);
		}

		public void RecordTyping (Line line, int pos, char ch)
		{
			if (locked)
				return;

			Action a = null;

			if (undo_actions.Count > 0)
				a = (Action) undo_actions.Peek ();

			if (a == null || a.type != ActionType.Typing) {
				a = new Action ();
				a.type = ActionType.Typing;
				a.data = new StringBuilder ();
				a.line_no = line.line_no;
				a.pos = pos;

				undo_actions.Push (a);
			}

			StringBuilder data = (StringBuilder) a.data;
			data.Append (ch);
		}

		// start_pos = 1-based
		// end_pos = 1-based
		public Line Duplicate(Line start_line, int start_pos, Line end_line, int end_pos) {
			Line	ret;
			Line	line;
			Line	current;
			LineTag	tag;
			LineTag	current_tag;
			int	start;
			int	end;
			int	tag_start;

			line = new Line();
			ret = line;

			for (int i = start_line.line_no; i <= end_line.line_no; i++) {
				current = document.GetLine(i);

				if (start_line.line_no == i) {
					start = start_pos;
				} else {
					start = 1;
				}

				if (end_line.line_no == i) {
					end = end_pos;
				} else {
					end = current.text.Length;
				}

				// Text for the tag
				line.text = new StringBuilder (current.text.ToString (start, end - start));

				// Copy tags from start to start+length onto new line
				current_tag = current.FindTag (start);
				while ((current_tag != null) && (current_tag.start < end)) {
					if ((current_tag.start <= start) && (start < (current_tag.start + current_tag.length))) {
						// start tag is within this tag
						tag_start = start;
					} else {
						tag_start = current_tag.start;
					}

					tag = new LineTag(line, tag_start - start + 1);
					tag.CopyFormattingFrom (current_tag);

					current_tag = current_tag.next;

					// Add the new tag to the line
					if (line.tags == null) {
						line.tags = tag;
					} else {
						LineTag tail;
						tail = line.tags;

						while (tail.next != null) {
							tail = tail.next;
						}
						tail.next = tag;
						tag.previous = tail;
					}
				}

				if ((i + 1) <= end_line.line_no) {
					line.soft_break = current.soft_break;

					// Chain them (we use right/left as next/previous)
					line.right = new Line();
					line.right.left = line;
					line = line.right;
				}
			}

			return ret;
		}

		// Insert multi-line text at the given position; use formatting at insertion point for inserted text
		internal void Insert(Line line, int pos, Line insert, bool select)
		{
			Line	current;
			LineTag	tag;
			int	offset;
			int	lines;
			Line	first;

			// Handle special case first
			if (insert.right == null) {

				// Single line insert
				document.Split(line, pos);

				if (insert.tags == null) {
					return;	// Blank line
				}

				//Insert our tags at the end
				tag = line.tags;

				while (tag.next != null) {
					tag = tag.next;
				}

				offset = tag.start + tag.length - 1;

				tag.next = insert.tags;
				line.text.Insert(offset, insert.text.ToString());

				// Adjust start locations
				tag = tag.next;
				while (tag != null) {
					tag.start += offset;
					tag.line = line;
					tag = tag.next;
				}
				// Put it back together
				document.Combine(line.line_no, line.line_no + 1);

				if (select) {
					document.SetSelectionStart (line, pos);
					document.SetSelectionEnd (line, pos + insert.text.Length);
				}

				document.UpdateView(line, pos);
				return;
			}

			first = line;
			lines = 1;
			current = insert;

			while (current != null) {
				if (current == insert) {
					// Inserting the first line we split the line (and make space)
					document.Split(line, pos);
					//Insert our tags at the end of the line
					tag = line.tags;

					if (tag != null) {
						while (tag.next != null) {
							tag = tag.next;
						}
						offset = tag.start + tag.length - 1;
						tag.next = current.tags;
						tag.next.previous = tag;

						tag = tag.next;

					} else {
						offset = 0;
						line.tags = current.tags;
						line.tags.previous = null;
						tag = line.tags;
					}
				} else {
					document.Split(line.line_no, 0);
					offset = 0;
					line.tags = current.tags;
					line.tags.previous = null;
					tag = line.tags;
				}
				// Adjust start locations and line pointers
				while (tag != null) {
					tag.start += offset;
					tag.line = line;
					tag = tag.next;
				}

				line.text.Insert(offset, current.text.ToString());
				line.Grow(line.text.Length);

				line.recalc = true;
				line = document.GetLine(line.line_no + 1);

				// FIXME? Test undo of line-boundaries
				if ((current.right == null) && (current.tags.length != 0)) {
					document.Combine(line.line_no - 1, line.line_no);
				}
				current = current.right;
				lines++;

			}

			// Recalculate our document
			document.UpdateView(first, lines, pos);
			return;
		}		
		#endregion	// Private Methods
	}
}
