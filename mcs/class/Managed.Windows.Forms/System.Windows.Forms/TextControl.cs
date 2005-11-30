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
// Copyright (c) 2004-2005 Novell, Inc. (http://www.novell.com)
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
// - Implement tag types for hotlinks, images, etc.
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

	internal enum CaretDirection {
		CharForward,	// Move a char to the right
		CharBack,	// Move a char to the left
		LineUp,		// Move a line up
		LineDown,	// Move a line down
		Home,		// Move to the beginning of the line
		End,		// Move to the end of the line
		PgUp,		// Move one page up
		PgDn,		// Move one page down
		CtrlHome,	// Move to the beginning of the document
		CtrlEnd,	// Move to the end of the document
		WordBack,	// Move to the beginning of the previous word (or beginning of line)
		WordForward,	// Move to the beginning of the next word (or end of line)
		SelectionStart,	// Move to the beginning of the current selection
		SelectionEnd	// Move to the end of the current selection
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


		// Stuff that's important for the tree
		internal Line			parent;			// Our parent line
		internal Line			left;			// Line with smaller line number
		internal Line			right;			// Line with higher line number
		internal LineColor		color;			// We're doing a black/red tree. this is the node color
		internal int			DEFAULT_TEXT_LEN;	// 
		internal static StringFormat	string_format;		// For calculating widths/heights
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

			if (string_format == null) {
				string_format = new StringFormat(StringFormat.GenericTypographic);
				string_format.Trimming = StringTrimming.None;
				string_format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
			}
		}

		internal Line(int LineNo, string Text, Font font, Brush color) : this() {
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder(Text, space);
			line_no = LineNo;

			widths = new float[space + 1];
			tags = new LineTag(this, 1, text.Length);
			tags.font = font;
			tags.color = color;
		}

		internal Line(int LineNo, string Text, HorizontalAlignment align, Font font, Brush color) : this() {
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder(Text, space);
			line_no = LineNo;
			alignment = align;

			widths = new float[space + 1];
			tags = new LineTag(this, 1, text.Length);
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

		internal void Streamline() {
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
				// Take out 0 length tags
				if (next.length == 0) {
					current.next = next.next;
					if (current.next != null) {
						current.next.previous = current;
					}
					next = current.next;
					continue;
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
					return tag;
				}
				tag = tag.next;
			}
			return null;
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
			float	wrap_width;

			pos = 0;
			len = this.text.Length;
			tag = this.tags;
			prev_height = this.height;	// For drawing optimization calculations
			this.height = 0;		// Reset line height
			this.ascent = 0;		// Reset the ascent for the line
			tag.shift = 0;
			tag.width = 0;
			widths[0] = 0;
			this.recalc = false;
			retval = false;
			wrapped = false;

			wrap_pos = 0;
			wrap_width = 0;

			while (pos < len) {
				size = g.MeasureString(this.text.ToString(pos, 1), tag.font, 10000, string_format);

				while (tag.length == 0) {	// We should always have tags after a tag.length==0 unless len==0
					tag.width = 0;
					tag.ascent = 0;
					if (tag.previous != null) {
						tag.X = tag.previous.X;
					} else {
						tag.X = 0;
					}
					tag = tag.next;
					tag.width = 0;
					tag.shift = 0;
				}

				w = size.Width;

				if (Char.IsWhiteSpace(text[pos])) {
					wrap_pos = pos + 1;
					wrap_width = tag.width + w;
				}

				if (doc.wrap) {
					if ((widths[pos] + w) + 27 > doc.viewport_width) {
						pos = wrap_pos;
						tag.width = wrap_width;
						doc.Split(this, tag, pos, true);
						len = this.text.Length;
						retval = true;
						wrapped = true;
					}
				}

				// Contract all soft lines that follow back into our line
				if (!wrapped) {
					tag.width += w;

					pos++;

					widths[pos] = widths[pos-1] + w;

					if (pos == len) {
						line = doc.GetLine(this.line_no + 1);
						if ((line != null) && (line.soft_break)) {
							// Pull the previous line back into this one
							doc.Combine(this.line_no, this.line_no + 1);
							len = this.text.Length;
							retval = true;
						}
					}
				}

				if (pos == (tag.start-1 + tag.length)) {
					// We just found the end of our current tag
					tag.height = (int)tag.font.Height;

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
						while (t != tag) {
							t.shift = tag.ascent - t.ascent;
							t = t.next;
						}

						// Save on our line
						this.ascent = tag.ascent;
					} else {
						tag.shift = this.ascent - tag.ascent;
					}

					// Update our horizontal starting pixel position
					if (tag.previous == null) {
						tag.X = 0;
					} else {
						tag.X = tag.previous.X + (int)tag.previous.width;
					}

					tag = tag.next;
					if (tag != null) {
						tag.width = 0;
						tag.shift = 0;
						wrap_pos = pos;
						wrap_width = tag.width;
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
		private Line		last_found;
		private int		document_id;
		private Random		random = new Random();

		internal bool		multiline;
		internal bool		wrap;

		internal UndoClass	undo;

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

		internal Control	owner;			// Who's owning us?
		#endregion	// Local Variables

		#region Constructors
		internal Document(Control owner) {
			lines = 0;

			this.owner = owner;

			multiline = true;

			// Tree related stuff
			sentinel = new Line();
			sentinel.color = LineColor.Black;

			document = sentinel;
			last_found = sentinel;

			// We always have a blank line
			owner.HandleCreated += new EventHandler(owner_HandleCreated);
			Add(1, "", owner.Font, ThemeEngine.Current.ResPool.GetSolidBrush(owner.ForeColor));
			lines=1;

			undo = new UndoClass(this);

			selection_visible = false;
			selection_start.line = this.document;
			selection_start.pos = 0;
			selection_start.tag = selection_start.line.tags;
			selection_end.line = this.document;
			selection_end.pos = 0;
			selection_end.tag = selection_end.line.tags;

			viewport_x = 0;
			viewport_y = -2;

			crlf_size = 2;

			// Default selection is empty

			document_id = random.Next();
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
		}

		internal int CRLFSize {
			get {
				return crlf_size;
			}

			set {
				crlf_size = value;
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
				viewport_width = value - 4;
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
		// For debugging
		internal int DumpTree(Line line, bool with_tags) {
			int	total;

			total = 1;

			Console.Write("Line {0}, Y: {1} Text {2}", line.line_no, line.Y, line.text != null ? line.text.ToString() : "undefined");

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
					Console.Write("{0} <{1}>-<{2}> ", count++, tag.start, tag.length);
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

			if (RecalculateDocument(owner.CreateGraphics(), line.line_no, line.line_no, true)) {
				// Lineheight changed, invalidate the rest of the document
				if ((line.Y - viewport_y) >=0 ) {
					// We formatted something that's in view, only draw parts of the screen
					owner.Invalidate(new Rectangle(0, line.Y - viewport_y, viewport_width, owner.Height - line.Y - viewport_y));
				} else {
					// The tag was above the visible area, draw everything
					owner.Invalidate();
				}
			} else {
				owner.Invalidate(new Rectangle((int)line.widths[pos] - viewport_x - 1, line.Y - viewport_y, viewport_width, line.height));
			}
		}


		// Update display from line, down line_count lines; pos is unused, but required for the signature
		internal void UpdateView(Line line, int line_count, int pos) {
			if (RecalculateDocument(owner.CreateGraphics(), line.line_no, line.line_no + line_count - 1, true)) {
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
			last_found = sentinel;
			lines = 0;

			// We always have a blank line
			Add(1, "", owner.Font, ThemeEngine.Current.ResPool.GetSolidBrush(owner.ForeColor));
			this.RecalculateDocument(owner.CreateGraphics());
			PositionCaret(0, 0);

			selection_visible = false;
			selection_start.line = this.document;
			selection_start.pos = 0;
			selection_start.tag = selection_start.line.tags;
			selection_end.line = this.document;
			selection_end.pos = 0;
			selection_end.tag = selection_end.line.tags;

			viewport_x = 0;
			viewport_y = 0;

			document_x = 0;
			document_y = 0;
		}

		internal void PositionCaret(Line line, int pos) {
			undo.RecordCursor();

			caret.tag = line.FindTag(pos);
			caret.line = line;
			caret.pos = pos;
			caret.height = caret.tag.height;

			XplatUI.DestroyCaret(owner.Handle);
			XplatUI.CreateCaret(owner.Handle, 2, caret.height);
			XplatUI.SetCaretPos(owner.Handle, (int)caret.tag.line.widths[caret.pos] + caret.line.align_shift - viewport_x, caret.line.Y + caret.tag.shift - viewport_y);

			if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
		}

		internal void PositionCaret(int x, int y) {
			undo.RecordCursor();

			caret.tag = FindCursor(x, y, out caret.pos);
			caret.line = caret.tag.line;
			caret.height = caret.tag.height;

			XplatUI.DestroyCaret(owner.Handle);
			XplatUI.CreateCaret(owner.Handle, 2, caret.height);
			XplatUI.SetCaretPos(owner.Handle, (int)caret.tag.line.widths[caret.pos] + caret.line.align_shift - viewport_x, caret.line.Y + caret.tag.shift - viewport_y);

			if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
		}

		internal void CaretHasFocus() {
			if ((caret.tag != null) && (!selection_visible)) {
				XplatUI.CreateCaret(owner.Handle, 2, caret.height);
				XplatUI.SetCaretPos(owner.Handle, (int)caret.tag.line.widths[caret.pos] + caret.line.align_shift - viewport_x, caret.line.Y + caret.tag.shift - viewport_y);
				XplatUI.CaretVisible(owner.Handle, true);
			}
		}

		internal void CaretLostFocus() {
			XplatUI.DestroyCaret(owner.Handle);
		}

		internal void AlignCaret() {
			if (!owner.IsHandleCreated) {
				return;
			}

			undo.RecordCursor();

			caret.tag = LineTag.FindTag(caret.line, caret.pos);
			caret.height = caret.tag.height;

			XplatUI.CreateCaret(owner.Handle, 2, caret.height);
			XplatUI.SetCaretPos(owner.Handle, (int)caret.tag.line.widths[caret.pos] + caret.line.align_shift - viewport_x, caret.line.Y + caret.tag.shift - viewport_y);
			XplatUI.CaretVisible(owner.Handle, true);

			if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
		}

		internal void UpdateCaret() {
			undo.RecordCursor();

			if (caret.tag.height != caret.height) {
				caret.height = caret.tag.height;
				XplatUI.CreateCaret(owner.Handle, 2, caret.height);
			}
			XplatUI.SetCaretPos(owner.Handle, (int)caret.tag.line.widths[caret.pos] + caret.line.align_shift - viewport_x, caret.line.Y + caret.tag.shift - viewport_y);
			XplatUI.CaretVisible(owner.Handle, true);

			if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
		}

		internal void DisplayCaret() {
			XplatUI.CaretVisible(owner.Handle, true);
		}

		internal void HideCaret() {
			XplatUI.CaretVisible(owner.Handle, false);
		}

		internal void MoveCaret(CaretDirection direction) {
			// FIXME should we use IsWordSeparator to detect whitespace, instead 
			// of looking for actual spaces in the Word move cases?
			switch(direction) {
				case CaretDirection.CharForward: {
					caret.pos++;
					if (caret.pos > caret.line.text.Length) {
						if (multiline) {
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

				case CaretDirection.CharBack: {
					if (caret.pos > 0) {
						// caret.pos--; // folded into the if below
						if (--caret.pos > 0) {
							if (caret.tag.start > caret.pos) {
								caret.tag = caret.tag.previous;
							}
						}
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
						XplatUI.CaretVisible(owner.Handle, true);
					}
					return;
				}

				case CaretDirection.LineDown: {
					if (caret.line.line_no < lines) {
						int	pixel;

						pixel = (int)caret.line.widths[caret.pos];
						PositionCaret(pixel, GetLine(caret.line.line_no + 1).Y);
						XplatUI.CaretVisible(owner.Handle, true);
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
					return;
				}

				case CaretDirection.PgDn: {
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
					caret.pos = 0;
					caret.tag = caret.line.tags;

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

		// Draw the document
		internal void Draw(Graphics g, Rectangle clip) {
			Line	line;		// Current line being drawn
			LineTag	tag;		// Current tag being drawn
			int	start;		// First line to draw
			int	end;		// Last line to draw
			//string	s;		// String representing the current line
			int	line_no;	//
			Brush	hilight;
			Brush	hilight_text;

			// First, figure out from what line to what line we need to draw
			start = GetLineByPixel(clip.Top + viewport_y, false).line_no;
			end = GetLineByPixel(clip.Bottom + viewport_y, false).line_no;
//Console.WriteLine("Starting drawing at line {0}, ending at line {1} (clip-bottom:{2})", start, end, clip.Bottom);

			// Now draw our elements; try to only draw those that are visible
			line_no = start;

			#if Debug
				DateTime	n = DateTime.Now;
				Console.WriteLine("Started drawing: {0}s {1}ms", n.Second, n.Millisecond);
			#endif

			hilight = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorHighlight);
			hilight_text = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorHighlightText);

			while (line_no <= end) {
				line = GetLine(line_no);
				tag = line.tags;
				//s = line.text.ToString();
				while (tag != null) {
					if (tag.length == 0) {
						tag = tag.next;
						continue;
					}

					if (((tag.X + tag.width) > (clip.Left - viewport_x)) || (tag.X < (clip.Right - viewport_x))) {
						// Check for selection
						if ((!selection_visible) || (!owner.has_focus) || (line_no < selection_start.line.line_no) || (line_no > selection_end.line.line_no)) {
							// regular drawing, no selection to deal with
							//g.DrawString(s.Substring(tag.start-1, tag.length), tag.font, tag.color, tag.X + line.align_shift - viewport_x, line.Y + tag.shift  - viewport_y, StringFormat.GenericTypographic);
							g.DrawString(line.text.ToString(tag.start-1, tag.length), tag.font, tag.color, tag.X + line.align_shift - viewport_x, line.Y + tag.shift  - viewport_y, StringFormat.GenericTypographic);
						} else {
							// we might have to draw our selection
							if ((line_no != selection_start.line.line_no) && (line_no != selection_end.line.line_no)) {
								// Special case, whole line is selected, draw this tag selected
								g.FillRectangle(
									hilight,					// Brush 
									tag.X + line.align_shift - viewport_x,		// X
									line.Y + tag.shift - viewport_y,		// Y
									line.widths[tag.start + tag.length - 1],	// width
									tag.height					// Height
								);

								g.DrawString(
									//s.Substring(tag.start-1, tag.length),		// String
									line.text.ToString(tag.start-1, tag.length),	// String
									tag.font,					// Font
									hilight_text,					// Brush
									tag.X + line.align_shift - viewport_x,		// X
									line.Y + tag.shift  - viewport_y,		// Y
									StringFormat.GenericTypographic);
							} else {
								bool	highlight;
								bool	partial;

								highlight = false;
								partial = false;

								// One or more, but not all tags on the line are selected
								if ((selection_start.tag == tag) && (selection_end.tag == tag)) {
									// Single tag selected, draw "normalSELECTEDnormal"
									partial = true;
									// First, the regular part
									g.DrawString(
										//s.Substring(tag.start - 1, selection_start.pos - tag.start + 1),	// String
										line.text.ToString(tag.start - 1, selection_start.pos - tag.start + 1),	// String
										tag.font,								// Font
										tag.color,								// Brush
										tag.X + line.align_shift - viewport_x,					// X
										line.Y + tag.shift  - viewport_y,					// Y
										StringFormat.GenericTypographic);

									// Now the highlight
									g.FillRectangle(
										hilight,								// Brush
										line.widths[selection_start.pos] + line.align_shift,			// X
										line.Y + tag.shift - viewport_y,					// Y
										line.widths[selection_end.pos] - line.widths[selection_start.pos],	// Width
										tag.height);								// Height

									g.DrawString(
										//s.Substring(selection_start.pos, selection_end.pos - selection_start.pos), // String
										line.text.ToString(selection_start.pos, selection_end.pos - selection_start.pos), // String
										tag.font,								// Font
										hilight_text,								// Brush
										line.widths[selection_start.pos] + line.align_shift - viewport_x,	// X
										line.Y + tag.shift - viewport_y,					// Y
										StringFormat.GenericTypographic);

									// And back to the regular
									g.DrawString(
										//s.Substring(selection_end.pos, tag.start + tag.length - selection_end.pos - 1), 	// String
										line.text.ToString(selection_end.pos, tag.start + tag.length - selection_end.pos - 1), 	// String
										tag.font,								// Font
										tag.color,								// Brush
										line.widths[selection_end.pos] + line.align_shift - viewport_x, 	// X
										line.Y + tag.shift - viewport_y,					// Y
										StringFormat.GenericTypographic);

								} else if (selection_start.tag == tag) {
									partial = true;

									// The highlighted part
									g.FillRectangle(
										hilight, 
										line.widths[selection_start.pos] + line.align_shift, 
										line.Y + tag.shift - viewport_y, 
										line.widths[tag.start + tag.length - 1] - line.widths[selection_start.pos], 
										tag.height);

									g.DrawString(
										//s.Substring(selection_start.pos, tag.start + tag.length - selection_start.pos - 1), 	// String
										line.text.ToString(selection_start.pos, tag.start + tag.length - selection_start.pos - 1), 	// String
										tag.font,							    	// Font
										hilight_text,							    	// Brush
										line.widths[selection_start.pos] + line.align_shift - viewport_x,    	// X
										line.Y + tag.shift - viewport_y,				    	// Y
										StringFormat.GenericTypographic);

									// The regular part
									g.DrawString(
										//s.Substring(tag.start - 1, selection_start.pos - tag.start + 1),  	// String
										line.text.ToString(tag.start - 1, selection_start.pos - tag.start + 1), // String
										tag.font,							  	// Font
										tag.color,							  	// Brush
										tag.X + line.align_shift - viewport_x,				  	// X
										line.Y + tag.shift  - viewport_y,				  	// Y
										StringFormat.GenericTypographic);
								} else if (selection_end.tag == tag) {
									partial = true;

									// The highlighted part
									g.FillRectangle(
										hilight, 
										tag.X + line.align_shift - viewport_x, 
										line.Y + tag.shift - viewport_y, 
										line.widths[selection_end.pos] - line.widths[tag.start - 1], 
										tag.height);

									g.DrawString(
										//s.Substring(tag.start - 1, selection_end.pos - tag.start + 1),	 // String
										line.text.ToString(tag.start - 1, selection_end.pos - tag.start + 1),	 // String
										tag.font,							 	// Font
										hilight_text,							 	// Brush
										tag.X + line.align_shift - viewport_x,				 	// X
										line.Y + tag.shift  - viewport_y,				 	// Y
										StringFormat.GenericTypographic);

									// The regular part
									g.DrawString(
										//s.Substring(selection_end.pos, tag.start + tag.length - selection_end.pos - 1),	  	// String
										line.text.ToString(selection_end.pos, tag.start + tag.length - selection_end.pos - 1),	  	// String
										tag.font,							  	// Font
										tag.color,							  	// Brush
										line.widths[selection_end.pos] + line.align_shift - viewport_x,	  	// X
										line.Y + tag.shift - viewport_y,				  	// Y
										StringFormat.GenericTypographic);
								} else {
									// no partially selected tags here, simple checks...
									if (selection_start.line == line) {
										int begin;
										int stop;

										begin = tag.start - 1;
										stop = tag.start + tag.length - 1;
										if (selection_end.line == line) {
											if ((begin >= selection_start.pos) && (stop < selection_end.pos)) {
												highlight = true;
											}
										} else {
											if (stop > selection_start.pos) {
												highlight = true;
											}
										}
									} else if (selection_end.line == line) {
										if ((tag.start - 1) < selection_end.pos) {
											highlight = true;
										}
									}
								}

								if (!partial) {
									if (highlight) {
										g.FillRectangle(
											hilight, 
											tag.X + line.align_shift - viewport_x, 
											line.Y + tag.shift  - viewport_y, 
											line.widths[tag.start + tag.length - 1] - line.widths[tag.start - 1],
											tag.height);

										g.DrawString(
											//s.Substring(tag.start-1, tag.length),		  	// String
											line.text.ToString(tag.start-1, tag.length),		// String
											tag.font,					  	// Font
											hilight_text,					  	// Brush
											tag.X + line.align_shift - viewport_x,		  	// X
											line.Y + tag.shift  - viewport_y,			// Y
											StringFormat.GenericTypographic);
									} else {
										g.DrawString(
											//s.Substring(tag.start-1, tag.length),		       	// String
											line.text.ToString(tag.start-1, tag.length),		// String
											tag.font,					       	// Font
											tag.color,					       	// Brush
											tag.X + line.align_shift - viewport_x,		       	// X
											line.Y + tag.shift  - viewport_y,			// Y
											StringFormat.GenericTypographic);
									}
								}
							}

						}
					}

					tag = tag.next;
				}

				line_no++;
			}
			#if Debug
				n = DateTime.Now;
				Console.WriteLine("Finished drawing: {0}s {1}ms", n.Second, n.Millisecond);
			#endif

		}

		internal void Insert(Line line, int pos, string s) {
			Insert(line, pos, false, s);
		}

		// Insert multi-line text at the given position; use formatting at insertion point for inserted text
		internal void Insert(Line line, int pos, bool update_caret, string s) {
			int		i;
			int		base_line;
			string[]	ins;
			int		insert_lines;
			LineTag		tag;

			// The formatting at the insertion point is used for the inserted text
			tag = LineTag.FindTag(line, pos);

			base_line = line.line_no;

			ins = s.Split(new char[] {'\n'});

			for (int j = 0; j < ins.Length; j++) {
				if (ins[j].EndsWith("\r")) {
					ins[j] = ins[j].Substring(0, ins[j].Length - 1);
				}
			}

			insert_lines = ins.Length;

			// Bump the text at insertion point a line down if we're inserting more than one line
			if (insert_lines > 1) {
				Split(line, pos);
				// Remainder of start line is now in base_line + 1
			}

			// Insert the first line
			InsertString(line, pos, ins[0]);

			if (insert_lines > 1) {
				for (i = 1; i < insert_lines; i++) {
					Add(base_line + i, ins[i], line.alignment, tag.font, tag.color);
				}
				if (!s.EndsWith("\n")) {
					this.Combine(base_line + insert_lines - 1, base_line + insert_lines);
				}
			}

			UpdateView(line, insert_lines + 1, pos);

			if (update_caret) {
				// Move caret to the end of the inserted text
				if (insert_lines > 1) {
					PositionCaret(GetLine(line.line_no + insert_lines - 1), ins[ins.Length - 1].Length);
				} else {
					PositionCaret(line, pos + ins[0].Length);
				}
				XplatUI.CaretVisible(owner.Handle, true);
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

			line = tag.line;
			line.text.Insert(pos, s);
			tag.length += len;

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
			LineTag	tag;
			int	len;

			len = s.Length;

			caret.line.text.Insert(caret.pos, s);
			caret.tag.length += len;
			
			if (caret.tag.next != null) {
				tag = caret.tag.next;
				while (tag != null) {
					tag.start += len;
					tag = tag.next;
				}
			}
			caret.line.Grow(len);
			caret.line.recalc = true;

			UpdateView(caret.line, caret.pos);
			if (move_caret) {
				caret.pos += len;
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

			line = tag.line;
			line.text.Insert(pos, ch);
			tag.length++;

			tag = tag.next;
			while (tag != null) {
				tag.start++;
				tag = tag.next;
			}
			line.Grow(1);
			line.recalc = true;

			UpdateView(line, pos);
		}

		// Inserts a character at the current caret position
		internal void InsertCharAtCaret(char ch, bool move_caret) {
			LineTag	tag;

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

			UpdateView(caret.line, caret.pos);
			if (move_caret) {
				caret.pos++;
				UpdateCaret();
			}
		}

		// Deletes n characters at the given position; it will not delete past line limits
		// pos is 0-based
		internal void DeleteChars(LineTag tag, int pos, int count) {
			Line	line;
			bool	streamline;


			streamline = false;
			line = tag.line;

			if (pos == line.text.Length) {
				return;
			}

			line.text.Remove(pos, count);

			// Make sure the tag points to the right spot
			while ((tag != null) && (tag.start + tag.length - 1) <= pos) {
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
				tag.length -= tag.start + tag.length - pos - 1;

				tag = tag.next;
				while ((tag != null) && (left > 0)) {
					tag.start -= count - left;
					if (tag.length > left) {
						tag.length -= left;
						left = 0;
					} else {
						left -= tag.length;
						tag.length = 0;
	
						tag = tag.next;
					}
				}
			} else {
				// We got off easy, same tag

				tag.length -= count;

				if (tag.length == 0) {
					streamline = true;
				}
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
				line.Streamline();
			}

			UpdateView(line, pos);
		}

		// Deletes a character at or after the given position (depending on forward); it will not delete past line limits
		internal void DeleteChar(LineTag tag, int pos, bool forward) {
			Line	line;
			bool	streamline;

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

				tag.length--;

				if (tag.length == 0) {
					streamline = true;
				}
			} else {
				pos--;
				line.text.Remove(pos, 1);
				if (pos >= (tag.start - 1)) {
					tag.length--;
					if (tag.length == 0) {
						streamline = true;
					}
				} else if (tag.previous != null) {
					tag.previous.length--;
					if (tag.previous.length == 0) {
						streamline = true;
					}
				}
			}

			tag = tag.next;
			while (tag != null) {
				tag.start--;
				tag = tag.next;
			}
			line.recalc = true;
			if (streamline) {
				line.Streamline();
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

			while (last.next != null) {
				last = last.next;
			}

			last.next = second.tags;
			last.next.previous = last;

			shift = last.start + last.length - 1;

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
			first.Streamline();

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
			if (soft && (caret.line == line) && (caret.pos > pos)) {
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

				if (soft) {
					if (move_caret) {
						caret.line = new_line;
						caret.line.soft_break = true;
						caret.tag = new_line.tags;
						caret.pos = 0;
					} else {
						new_line.soft_break = true;
					}
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
			Add(line.line_no + 1, line.text.ToString(pos, line.text.Length - pos), line.alignment, tag.font, tag.color);

			// Now transfer our tags from this line to the next
			new_line = GetLine(line.line_no + 1);
			line.recalc = true;
			new_line.recalc = true;

			if ((tag.start - 1) == pos) {
				int	shift;

				// We can simply break the chain and move the tag into the next line
				if (tag == line.tags) {
					new_tag = new LineTag(line, 1, 0);
					new_tag.font = tag.font;
					new_tag.color = tag.color;
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

				new_tag = new LineTag(new_line, 1, tag.start - 1 + tag.length - pos);
				new_tag.next = tag.next;
				new_tag.font = tag.font;
				new_tag.color = tag.color;
				new_line.tags = new_tag;
				if (new_tag.next != null) {
					new_tag.next.previous = new_tag;
				}
				tag.next = null;
				tag.length = pos - tag.start + 1;

				shift = pos;
				new_tag = new_tag.next;
				while (new_tag != null) {
					new_tag.start -= shift;
					new_tag.line = new_line;
					new_tag = new_tag.next;

				}
			}

			if (soft) {
				if (move_caret) {
					caret.line = new_line;
					caret.pos = caret.pos - pos;
					caret.tag = caret.line.FindTag(caret.pos);
				}
				new_line.soft_break = true;
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

			line.text.Remove(pos, line.text.Length - pos);
		}

		// Adds a line of text, with given font.
		// Bumps any line at that line number that already exists down
		internal void Add(int LineNo, string Text, Font font, Brush color) {
			Add(LineNo, Text, HorizontalAlignment.Left, font, color);
		}

		internal void Add(int LineNo, string Text, HorizontalAlignment align, Font font, Brush color) {
			Line	add;
			Line	line;
			int	line_no;

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

				line1.ascent = line3.ascent;
				line1.height = line3.height;
				line1.line_no = line3.line_no;
				line1.recalc = line3.recalc;
				line1.space = line3.space;
				line1.tags = line3.tags;
				line1.text = line3.text;
				line1.widths = line3.widths;
				line1.Y = line3.Y;
				line1.soft_break = line3.soft_break;

				tag = line1.tags;
				while (tag != null) {
					tag.line = line1;
					tag = tag.next;
				}
			}

			if (line3.color == LineColor.Black)
				RebalanceAfterDelete(line2);

			this.lines--;

			last_found = sentinel;
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

				#if Debug
					Console.WriteLine("Invaliding from {0}:{1} to {2}:{3}", l1.line_no, p1, l2.line_no, p2);
				#endif

				owner.Invalidate(
					new Rectangle(
						(int)l1.widths[p1] + l1.align_shift - viewport_x, 
						l1.Y - viewport_y, 
						(int)l2.widths[p2] - (int)l1.widths[p1] + 1, 
						l1.height
					)
				);
				return;
			}

			#if Debug
				Console.WriteLine("Invaliding from {0}:{1} to {2}:{3} Start  => x={4}, y={5}, {6}x{7}", l1.line_no, p1, l2.line_no, p2, (int)l1.widths[p1] + l1.align_shift - viewport_x, l1.Y - viewport_y, viewport_width, l1.height);
			#endif

			// Three invalidates:
			// First line from start
			owner.Invalidate(new Rectangle((int)l1.widths[p1] + l1.align_shift - viewport_x, l1.Y - viewport_y, viewport_width, l1.height));

			// lines inbetween
			if ((l1.line_no + 1) < l2.line_no) {
				int	y;

				y = GetLine(l1.line_no + 1).Y;
				owner.Invalidate(new Rectangle(0, y - viewport_y, viewport_width, GetLine(l2.line_no).Y - y - viewport_y));

				#if Debug
					Console.WriteLine("Invaliding from {0}:{1} to {2}:{3} Middle => x={4}, y={5}, {6}x{7}", l1.line_no, p1, l2.line_no, p2, 0, y - viewport_y, viewport_width, GetLine(l2.line_no).Y - y - viewport_y);
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

			if (selection_start == selection_end) {
				selection_visible = false;
			} else {
				selection_visible = true;
			}
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

			if (selection_start == selection_end) {
				selection_visible = false;
			} else {
				selection_visible = true;
			}
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
				selection_visible = false;
			} else {
				selection_visible = true;

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
				selection_visible = true;

				// This could be calculated better
				Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
			}

		}

		internal void SetSelectionEnd(Line end, int end_pos) {
			if ((end.line_no < selection_anchor.line.line_no) || ((end == selection_anchor.line) && (end_pos <= selection_anchor.pos))) {
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
				selection_visible = true;
				Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
			}
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
			selection_visible = false;
		}

		internal void InvalidateSelectionArea() {
			// FIXME - the only place that calls this right now should really calculate the redraw itself; if done this function can go
			// Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
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

		internal void ReplaceSelection(string s) {
			int		i;
			int		base_line;
			string[]	ins;
			int		insert_lines;

			base_line = selection_start.line.line_no;

			// First, delete any selected text
			if ((selection_start.pos != selection_end.pos) || (selection_start.line != selection_end.line)) {
				if (!multiline || (selection_start.line == selection_end.line)) {
					undo.RecordDeleteChars(selection_start.line, selection_start.pos + 1, selection_end.pos - selection_start.pos);

					DeleteChars(selection_start.tag, selection_start.pos, selection_end.pos - selection_start.pos);

					// The tag might have been removed, we need to recalc it
					selection_start.tag = selection_start.line.FindTag(selection_start.pos);
				} else {
					int		start;
					int		end;

					start = selection_start.line.line_no;
					end = selection_end.line.line_no;

					undo.RecordDelete(selection_start.line, selection_start.pos + 1, selection_end.line, selection_end.pos);

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

			Insert(selection_start.line, selection_start.pos, true, s);

			selection_end.line = selection_start.line;
			selection_end.pos = selection_start.pos;
			selection_end.tag = selection_start.tag;

			selection_visible = false;
		}

		internal void CharIndexToLineTag(int index, out Line line_out, out LineTag tag_out, out int pos) {
			Line	line;
			LineTag	tag;
			int	i;
			int	chars;
			int	start;

			chars = 0;

			for (i = 1; i < lines; i++) {
				line = GetLine(i);

				start = chars;
				chars += line.text.Length + crlf_size;

				if (index <= chars) {
					// we found the line
					tag = line.tags;

					while (tag != null) {
						if (index < (start + tag.start + tag.length)) {
							line_out = line;
							tag_out = tag;
							pos = index - start;
							return;
						}
						if (tag.next == null) {
							Line	next_line;

							next_line = GetLine(line.line_no + 1);

							if (next_line != null) {
								line_out = next_line;
								tag_out = next_line.tags;
								pos = 0;
								return;
							} else {
								line_out = line;
								tag_out = tag;
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
				length += GetLine(i).text.Length + crlf_size;
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
						length += GetLine(i).text.Length + crlf_size;
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
							return tag;
						}
					}
					index=end;
					return tag;
				}
				if (tag.next != null) {
					tag = tag.next;
				} else {
					if (exact) {
						index = 0;
						return null;
					}

					index = line.text.Length;
					return tag;
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
			x += line.align_shift;

			while (true) {
				if (x >= tag.X && x < (tag.X+tag.width)) {
					int	end;

					end = tag.start + tag.length - 1;

					for (int pos = tag.start-1; pos < end; pos++) {
						// When clicking on a character, we position the cursor to whatever edge
						// of the character the click was closer
						if (x < (line.widths[pos] + ((line.widths[pos+1]-line.widths[pos])/2))) {
							index = pos;
							return tag;
						}
					}
					index=end;
					return tag;
				}
				if (tag.next != null) {
					tag = tag.next;
				} else {
					index = line.text.Length;
					return tag;
				}
			}
		}

		/// <summary>Format area of document in specified font and color</summary>
		/// <param name="start_pos">1-based start position on start_line</param>
		/// <param name="end_pos">1-based end position on end_line </param>
		internal void FormatText(Line start_line, int start_pos, Line end_line, int end_pos, Font font, Brush color) {
			Line    l;

			// First, format the first line
			if (start_line != end_line) {
				// First line
				LineTag.FormatText(start_line, start_pos, start_line.text.Length - start_pos + 1, font, color);

				// Format last line
				LineTag.FormatText(end_line, 1, end_pos, font, color);

				// Now all the lines inbetween
				for (int i = start_line.line_no + 1; i < end_line.line_no; i++) {
					l = GetLine(i);
					LineTag.FormatText(l, 1, l.text.Length, font, color);
				}
			} else {
				// Special case, single line
				LineTag.FormatText(start_line, start_pos, end_pos - start_pos, font, color);
			}
		}

		internal void RecalculateAlignments() {
			Line	line;
			int	line_no;

			line_no = 1;

			while (line_no <= lines) {
				line = GetLine(line_no);

				if (line != null && line.alignment != HorizontalAlignment.Left) {
					if (line.alignment == HorizontalAlignment.Center) {
						line.align_shift = (viewport_width - (int)line.widths[line.text.Length]) / 2;
					} else {
						line.align_shift = viewport_width - (int)line.widths[line.text.Length];
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

				if (line.widths[line.text.Length] > new_width) {
					new_width = (int)line.widths[line.text.Length];
				}

				// Calculate alignment
				if (line.alignment != HorizontalAlignment.Left) {
					if (line.alignment == HorizontalAlignment.Center) {
						line.align_shift = (viewport_width - (int)line.widths[line.text.Length]) / 2;
					} else {
						line.align_shift = viewport_width - (int)line.widths[line.text.Length];
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

			return changed;
		}

		internal int Size() {
			return lines;
		}

		private void owner_HandleCreated(object sender, EventArgs e) {
			this.RecalculateDocument(owner.CreateGraphics());
			PositionCaret(0, 0);
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

	internal class LineTag {
		#region	Local Variables;
		// Payload; formatting
		internal Font		font;		// System.Drawing.Font object for this tag
		internal Brush		color;		// System.Drawing.Brush object

		// Payload; text
		internal int		start;		// start, in chars; index into Line.text
		internal int		length;		// length, in chars
		internal bool		r_to_l;		// Which way is the font

		// Drawing support
		internal int		height;		// Height in pixels of the text this tag describes
		internal int		X;		// X location of the text this tag describes
		internal float		width;		// Width in pixels of the text this tag describes
		internal int		ascent;		// Ascent of the font for this tag
		internal int		shift;		// Shift down for this tag, to stay on baseline

		// Administrative
		internal Line		line;		// The line we're on
		internal LineTag	next;		// Next tag on the same line
		internal LineTag	previous;	// Previous tag on the same line
		#endregion;

		#region Constructors
		internal LineTag(Line line, int start, int length) {
			this.line = line;
			this.start = start;
			this.length = length;
			this.X = 0;
			this.width = 0;
		}
		#endregion	// Constructors

		#region Internal Methods
		/// <summary>Applies 'font' to characters starting at 'start' for 'length' chars; 
		/// Removes any previous tags overlapping the same area; 
		/// returns true if lineheight has changed</summary>
		/// <param name="start">1-based character position on line</param>
		internal static bool FormatText(Line line, int start, int length, Font font, Brush color) {
			LineTag	tag;
			LineTag	start_tag;
			int	end;
			bool	retval = false;		// Assume line-height doesn't change

			// Too simple?
			if (font.Height != line.height) {
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
				tag.font = font;
				tag.color = color;
				return retval;
			}

			start_tag = FindTag(line, start);

			tag = new LineTag(line, start, length);
			tag.font = font;
			tag.color = color;

			if (start == 1) {
				line.tags = tag;
			}

			if (start_tag.start == start) {
				tag.next = start_tag;
				tag.previous = start_tag.previous;
				if (start_tag.previous != null) {
					start_tag.previous.next = tag;
				}
				start_tag.previous = tag;
			} else {
				// Insert ourselves 'in the middle'
				if ((start_tag.next != null) && (start_tag.next.start < end)) {
					tag.next = start_tag.next;
				} else {
					tag.next = new LineTag(line, start_tag.start, start_tag.length);
					tag.next.font = start_tag.font;
					tag.next.color = start_tag.color;

					if (start_tag.next != null) {
						tag.next.next = start_tag.next;
						tag.next.next.previous = tag.next;
					}
				}
				tag.next.previous = tag;

				start_tag.length = start - start_tag.start;

				tag.previous = start_tag;
				start_tag.next = tag;
#if nope
				if (tag.next.start > (tag.start + tag.length)) {
					tag.next.length  += tag.next.start - (tag.start + tag.length);
					tag.next.start = tag.start + tag.length;
				}
#endif
			}

			// Elimination loop
			tag = tag.next;
			while ((tag != null) && (tag.start < end)) {
				if ((tag.start + tag.length) <= end) {
					// remove the tag
					tag.previous.next = tag.next;
					if (tag.next != null) {
						tag.next.previous = tag.previous;
					}
					tag = tag.previous;
				} else {
					// Adjust the length of the tag
					tag.length = (tag.start + tag.length) - end;
					tag.start = end;
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
				return tag;
			}

			while (tag != null) {
				if ((tag.start <= pos) && (pos < (tag.start+tag.length))) {
					return tag;
				}

				tag = tag.next;
			}

			return null;
		}

		/// <summary>Combines 'this' tag with 'other' tag</summary>
		internal bool Combine(LineTag other) {
			if (!this.Equals(other)) {
				return false;
			}

			this.width += other.width;
			this.length += other.length;
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
				this.previous.length += this.length;
				this.previous.width = -1;
				this.previous.next = this.next;
				this.next.previous = this.previous;
			} else {
				this.next.start = 1;
				this.next.length += this.length;
				this.next.width = -1;
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
			return "Tag starts at index " + this.start + "length " + this.length + " text: " + this.line.Text.Substring(this.start-1, this.length) + "Font " + this.font.ToString();
		}

		#endregion	// Internal Methods
	}

	internal class UndoClass {
		internal enum ActionType {
			InsertChar,
			InsertString,
			DeleteChar,
			DeleteChars,
			CursorMove,
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
		#endregion	// Local Variables

		#region Constructors
		internal UndoClass(Document doc) {
			document = doc;
			undo_actions = new Stack(50);
			redo_actions = new Stack(50);
		}
		#endregion	// Constructors

		#region Properties
		[MonoTODO("Change this to be configurable")]
		internal int UndoLevels {
			get {
				return undo_actions.Count;
			}
		}

		[MonoTODO("Change this to be configurable")]
		internal int RedoLevels {
			get {
				return redo_actions.Count;
			}
		}

		[MonoTODO("Come up with good naming and localization")]
		internal string UndoName {
			get {
				Action action;

				action = (Action)undo_actions.Peek();
				switch(action.type) {
					case ActionType.InsertChar: {
						Locale.GetText("Insert character");
						break;
					}

					case ActionType.DeleteChar: {
						Locale.GetText("Delete character");
						break;
					}

					case ActionType.InsertString: {
						Locale.GetText("Insert string");
						break;
					}

					case ActionType.DeleteChars: {
						Locale.GetText("Delete string");
						break;
					}

					case ActionType.CursorMove: {
						Locale.GetText("Cursor move");
						break;
					}
				}
				return null;
			}
		}

		internal string RedoName() {
			return null;
		}
		#endregion	// Properties

		#region Internal Methods
		internal void Clear() {
			undo_actions.Clear();
			redo_actions.Clear();
		}

		internal void Undo() {
			Action action;

			if (undo_actions.Count == 0) {
				return;
			}

			action = (Action)undo_actions.Pop();

			// Put onto redo stack
			redo_actions.Push(action);

			// Do the thing
			switch(action.type) {
				case ActionType.InsertChar: {
					break;
				}

				case ActionType.DeleteChars: {
					this.Insert(document.GetLine(action.line_no), action.pos, (Line)action.data);
					break;
				}

				case ActionType.CursorMove: {
					document.caret.line = document.GetLine(action.line_no);
					document.caret.tag = document.caret.line.FindTag(action.pos);
					document.caret.pos = action.pos;
					document.caret.height = document.caret.tag.height;

					XplatUI.DestroyCaret(document.owner.Handle);
					XplatUI.CreateCaret(document.owner.Handle, 2, document.caret.height);
					XplatUI.SetCaretPos(document.owner.Handle, (int)document.caret.tag.line.widths[document.caret.pos] + document.caret.line.align_shift - document.viewport_x, document.caret.line.Y + document.caret.tag.shift - document.viewport_y);
					XplatUI.CaretVisible(document.owner.Handle, true);

					// FIXME - enable call
					//if (document.CaretMoved != null) document.CaretMoved(this, EventArgs.Empty);
					break;
				}
			}
		}

		internal void Redo() {
			if (redo_actions.Count == 0) {
				return;
			}
		}
		#endregion	// Internal Methods

		#region Private Methods
		// pos = 1-based
		public void RecordDeleteChars(Line line, int pos, int length) {
			RecordDelete(line, pos, line, pos + length - 1);
		}

		// start_pos, end_pos = 1 based
		public void RecordDelete(Line start_line, int start_pos, Line end_line, int end_pos) {
			Line	l;
			Action	a;

			l = Duplicate(start_line, start_pos, end_line, end_pos);

			a = new Action();
			a.type = ActionType.DeleteChars;
			a.data = l;
			a.line_no = start_line.line_no;
			a.pos = start_pos - 1;

			undo_actions.Push(a);
		}

		public void RecordCursor() {
			if (document.caret.line == null) {
				return;
			}

			RecordCursor(document.caret.line, document.caret.pos);
		}

		public void RecordCursor(Line line, int pos) {
			Action a;

			if ((line.line_no == caret_line) && (pos == caret_pos)) {
				return;
			}

			caret_line = line.line_no;
			caret_pos = pos;

			a = new Action();
			a.type = ActionType.CursorMove;
			a.line_no = line.line_no;
			a.pos = pos;

			undo_actions.Push(a);
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
			int	tag_length;

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
				line.text = new StringBuilder(current.text.ToString(start - 1, end - start + 1));

				// Copy tags from start to start+length onto new line
				current_tag = current.FindTag(start - 1);
				while ((current_tag != null) && (current_tag.start < end)) {
					if ((current_tag.start <= start) && (start < (current_tag.start + current_tag.length))) {
						// start tag is within this tag
						tag_start = start;
					} else {
						tag_start = current_tag.start;
					}

					if (end < (current_tag.start + current_tag.length)) {
						tag_length = end - tag_start + 1;
					} else {
						tag_length = current_tag.start + current_tag.length - tag_start;
					}
					tag = new LineTag(line, tag_start - start + 1, tag_length);
					tag.color = current_tag.color;
					tag.font = current_tag.font;

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
		internal void Insert(Line line, int pos, Line insert) {
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
