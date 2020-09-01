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
//	Karl Scowen	<contact@scowencomputers.co.nz>
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using RTF=System.Windows.Forms.RTF;

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

	[Flags]
	internal enum FormatSpecified {
		None,

		BackColor = 2,
		Font = 4,
		Color = 8,
		TextPosition = 16,
		CharOffset = 32,
		Visibility = 64,

		All = 126
	}

	internal enum TextPositioning {
		Normal,
		Superscript,
		Subscript
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

	internal enum LineEnding {
		Wrap = 1,    // line wraps to the next line
		Limp = 2,    // \r
		Hard = 4,    // \r\n
		Soft = 8,    // \r\r\n
		Rich = 16,    // \n

		None = 0
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
		private bool		enable_links;

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
		internal HorizontalAlignment alignment;
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
		internal int		offset_x;
		internal int		offset_y;		// Never assigned to except in constructor, and a property that is also never assigned to?
		internal int		viewport_width;
		internal int		viewport_height;

		internal int		document_x;		// Width of the document
		internal int		document_y;		// Height of the document

		internal int		crlf_size;		// 1 or 2, depending on whether we use \r\n or just \n

		internal TextBoxBase	owner;			// Who's owning us?
		static internal int	caret_width = 1;
		static internal int	caret_shift = 1;

		internal int left_margin = 2;  // A left margin for all lines
		internal int top_margin = 2;
		internal int right_margin = 2;

		internal float	dpi;
		#endregion	// Local Variables

		#region Constructors
		internal Document (TextBoxBase owner)
		{
			lines = 0;

			this.owner = owner;

			password_char = "";
			calc_pass = false;
			recalc_pending = false;

			// Tree related stuff
			sentinel = new Line (this, LineEnding.None);
			sentinel.color = LineColor.Black;

			document = sentinel;

			// We always have a blank line
			owner.HandleCreated += new EventHandler(owner_HandleCreated);
			owner.VisibleChanged += new EventHandler(owner_VisibleChanged);

			Add (1, String.Empty, owner.Font, owner.ForeColor, LineEnding.None);

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

			offset_x = 0;
			offset_y = 0;

			crlf_size = 2;

			// Default selection is empty

			document_id = random.Next();

			string_format.Trimming = StringTrimming.None;
			string_format.FormatFlags = StringFormatFlags.DisplayFormatControl;

			UpdateMargins ();
		}
		#endregion

		#region Internal Properties

		internal float Dpi {
			get {
				if (dpi > 0)
					return dpi;
				return TextRenderer.GetDpi ().Height;
			}
		}

		internal Line Root {
			get {
				return document;
			}

			set {
				document = value;
			}
		}

		// UIA: Method used via reflection in TextRangeProvider
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
				return new Point((int)caret.tag.Line.widths[caret.pos] + caret.line.X, caret.line.Y);
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

		/// <summary>
		///  Whether text is scanned for links
		/// </summary>
		internal bool EnableLinks {
			get { return enable_links; }
			set { enable_links = value; }
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
				var lastLine = GetLine (lines);
				return char_count - LineEndingLength (lastLine.ending);
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

		internal int OffsetX
		{
			get
			{
				return offset_x;
			}

			set
			{
				offset_x = value;
			}
		}

		internal int OffsetY
		{
			get
			{
				return offset_y;
			}

			set
			{
				offset_y = value;
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

		internal void UpdateMargins ()
		{
			switch (owner.actual_border_style) {
				case BorderStyle.None:
					left_margin = 0;
					top_margin = 0;
					right_margin = 1;
					break;
				case BorderStyle.FixedSingle:
					left_margin = 2;
					top_margin = 2;
					right_margin = 3;
					break;
				case BorderStyle.Fixed3D:
					left_margin = 1;
					top_margin = 1;
					right_margin = 2;
					break;
			}
		}

		internal void SuspendRecalc ()
		{
			if (recalc_suspended == 0) {
				recalc_start = int.MaxValue;
				recalc_end = int.MinValue;
			}
			
			recalc_suspended++;
		}

		internal void ResumeRecalc (bool immediate_update)
		{
			if (recalc_suspended > 0)
				recalc_suspended--;

			if (recalc_suspended == 0 && (immediate_update || recalc_pending) && !(recalc_start == int.MaxValue && recalc_end == int.MinValue)) {
				if (owner.IsHandleCreated) {
					using (var graphics = owner.CreateGraphics ())
						RecalculateDocument (graphics, recalc_start, recalc_end, recalc_optimize);
					recalc_pending = false;
				}
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

			Console.Write("Line {0} [# {1}], Y: {2}, ending style: {3},  Text: '{4}'",
					line.line_no, line.GetHashCode(), line.Y, line.ending,
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
					Console.Write("{0} <{1}>-<{2}>", count++, tag.Start, tag.End
							/*line.text.ToString (tag.start - 1, tag.length)*/);
					length += tag.Length;

					if (tag.Line != line) {
						Console.Write("BAD line link");
						throw new Exception("Bad line link in tree");
					}
					tag = tag.Next;
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
			bool old_selection_visible = selection_visible;
			selection_visible = value;

			// cursor and selection are enemies, we can't have both in the same room at the same time
			if (owner.IsHandleCreated && !owner.show_caret_w_selection)
				XplatUI.CaretVisible (owner.Handle, !selection_visible);
			if (UIASelectionChanged != null && (selection_visible || old_selection_visible))
				UIASelectionChanged (this, EventArgs.Empty);
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
			foreach (var line in TransverseLines(this.lines, line_no)) {
				line.line_no++;
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

			var prev_line_pos = new Point(line.X, line.Y);
			var prev_line_bottom = line.Y + line.Height;
			var prev_pos_width = line.widths[pos];
			float prev_pos1_width = (line.widths.Length > pos + 1) ? line.widths[pos + 1] : prev_pos_width;

			// Optimize invalidation based on Line alignment
			bool height_changed;
			using (var graphics = owner.CreateGraphics())
				height_changed = RecalculateDocument(graphics, line.line_no, line.line_no, true);
			var x = Math.Min(prev_line_pos.X, line.X);
			var y = Math.Min(prev_line_pos.Y, line.Y);
			var h = Math.Max(prev_line_bottom, line.Y + line.Height) - y;
			if (height_changed) {
				// Lineheight changed, invalidate the rest of the document
				if ((y - viewport_y) >=0 ) {
					// We formatted something that's in view, only draw parts of the screen
					owner.Invalidate(new Rectangle(
						offset_x, 
						y - viewport_y + offset_y, 
						viewport_width, 
						owner.Height - (y - viewport_y)));
				} else {
					// The tag was above the visible area, draw everything
					owner.Invalidate();
				}
			} else {
				switch(line.alignment) {
					case HorizontalAlignment.Left: {
						owner.Invalidate(new Rectangle(
							x + ((int)Math.Max(line.widths[pos], prev_pos_width) - viewport_x - 1) + offset_x, 
							y - viewport_y + offset_y, 
							viewport_width, 
							h + 1));
						break;
					}

					case HorizontalAlignment.Center: {
						owner.Invalidate(new Rectangle(
							x + offset_x, 
							y - viewport_y + offset_y, 
							viewport_width, 
							h + 1));
						break;
					}

					case HorizontalAlignment.Right: {
						owner.Invalidate(new Rectangle(
							x + offset_x, 
							y - viewport_y + offset_y, 
							(int)Math.Max(line.widths[pos + ((line.widths.Length > pos + 1) ? 1 : 0)], prev_pos1_width) - viewport_x + line.X, 
							h + 1));
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
				recalc_end = Math.Max (recalc_end, line.line_no + line_count);
				recalc_optimize = true;
				recalc_pending = true;
				return;
			}

			int start_line_top = line.Y;			

			Line end_line = GetLine (line.line_no + line_count);
			if (end_line == null)
				end_line = GetLine (lines);

			if (end_line == null)
				return;
			
			int end_line_bottom = end_line.Y + end_line.height;
			
			bool height_changed;
			using (var graphics = owner.CreateGraphics())
				height_changed = RecalculateDocument(graphics, line.line_no, line.line_no + line_count, true);
			if (height_changed) {
				// Lineheight changed, invalidate the rest of the document
				if ((line.Y - viewport_y) >=0 ) {
					// We formatted something that's in view, only draw parts of the screen
					owner.Invalidate(new Rectangle(
						offset_x, 
						line.Y - viewport_y + offset_y, 
						viewport_width, 
						owner.Height - (line.Y - viewport_y)));
				} else {
					// The tag was above the visible area, draw everything
					owner.Invalidate();
				}
			} else {
				int x = 0 - viewport_x + offset_x;
				int w = viewport_width;
				int y = Math.Min (start_line_top - viewport_y, line.Y - viewport_y) + offset_y;
				int h = Math.Max (end_line_bottom - y, end_line.Y + end_line.height - y);

				owner.Invalidate (new Rectangle (x, y, w, h));
			}
		}

		/// <summary>
		///  Scans the next paragraph for http:/ ftp:/ www. https:/ etc and marks the tags
		///  as links.
		/// </summary>
		/// <param name="start_line">The line to start on</param>
		/// <param name="link_changed">marks as true if something is changed</param>
		private void ScanForLinks (Line start_line, ref bool link_changed)
		{
			Line current_line = start_line;
			StringBuilder line_no_breaks = new StringBuilder ();
			StringBuilder line_link_record = new StringBuilder ();
			ArrayList cumulative_length_list = new ArrayList ();
			bool update_caret_tag = false;

			cumulative_length_list.Add (0);

			while (current_line != null) {
				line_no_breaks.Append (current_line.text);

				if (link_changed == false)
					current_line.LinkRecord (line_link_record);

				current_line.ClearLinks ();

				cumulative_length_list.Add (line_no_breaks.Length);

				if (current_line.ending == LineEnding.Wrap)
					current_line = GetLine (current_line.LineNo + 1);
				else
					break;
			}

			// search for protocols.. make sure www. is first!
			string [] search_terms = new string [] { "www.", "http:/", "ftp:/", "https:/" };
			int search_found = 0;
			int index_found = 0;
			string line_no_breaks_string = line_no_breaks.ToString ();
			int line_no_breaks_index = 0;
			int link_end = 0;

			while (true) {
				if (line_no_breaks_index >= line_no_breaks_string.Length)
					break;

				index_found = FirstIndexOfAny (line_no_breaks_string, search_terms, line_no_breaks_index, out search_found);

				//no links found on this line
				if (index_found == -1)
					break;

				if (search_found == 0) {
					// if we are at the end of the line to analyse and the end of the line
					// is "www." then there are no links here
					if (line_no_breaks_string.Length == index_found + search_terms [0].Length)
						break;

					// if after www. we don't have a letter a digit or a @ or - or /
					// then it is not a web address, we should continue searching
					if (char.IsLetterOrDigit (line_no_breaks_string [index_found + search_terms [0].Length]) == false &&
						"@/~".IndexOf (line_no_breaks_string [index_found + search_terms [0].Length].ToString ()) == -1) {
						line_no_breaks_index = index_found + search_terms [0].Length;
						continue;
					}
				}

				link_end = line_no_breaks_string.Length - 1;
				line_no_breaks_index = line_no_breaks_string.Length;

				// we've found a link, we just need to find where it ends now
				for (int i = index_found + search_terms [search_found].Length; i < line_no_breaks_string.Length; i++) {
					if (line_no_breaks_string [i - 1] == '.') {
						if (char.IsLetterOrDigit (line_no_breaks_string [i]) == false &&
							"@/~".IndexOf (line_no_breaks_string [i].ToString ()) == -1) {
							link_end = i - 1;
							line_no_breaks_index = i;
							break;
						}
					} else {
						if (char.IsLetterOrDigit (line_no_breaks_string [i]) == false &&
							"@-/:~.?=_&".IndexOf (line_no_breaks_string [i].ToString ()) == -1) {
							link_end = i - 1;
							line_no_breaks_index = i;
							break;
						}
					}
				}

				string link_text = line_no_breaks_string.Substring (index_found, link_end - index_found + 1);
				int current_cumulative = 0;

				// we've found a link - index_found -> link_end
				// now we just make all the tags as containing link and
				// point them to the text for the whole link

				current_line = start_line;

				//find the line we start on
				for (current_cumulative = 1; current_cumulative < cumulative_length_list.Count; current_cumulative++)
					if ((int)cumulative_length_list [current_cumulative] > index_found)
						break;

				current_line = GetLine (start_line.LineNo + current_cumulative - 1);

				// find the tag we start on
				LineTag current_tag = current_line.FindTag (index_found - (int)cumulative_length_list [current_cumulative - 1] + 1);

				if (current_tag.Start != (index_found - (int)cumulative_length_list [current_cumulative - 1]) + 1) {
					if (current_tag == CaretTag)
						update_caret_tag = true;

					current_tag = current_tag.Break ((index_found - (int)cumulative_length_list [current_cumulative - 1]) + 1);
				}

				// set the tag
				current_tag.IsLink = true;
				current_tag.LinkText = link_text;

				//go through each character
				// find the tag we are in
				// skip the number of characters in the tag
				for (int i = 1; i < link_text.Length; i++) {
					// on to a new word-wrapped line
					if ((int)cumulative_length_list [current_cumulative] <= index_found + i) {

						current_line = GetLine (start_line.LineNo + current_cumulative++);
						current_tag = current_line.FindTag (index_found + i - (int)cumulative_length_list [current_cumulative - 1] + 1);

						current_tag.IsLink = true;
						current_tag.LinkText = link_text;

						continue;
					}

					if (current_tag.End < index_found + 1 + i - (int)cumulative_length_list [current_cumulative - 1]) {
						// skip empty tags in the middle of the URL
						do {
							current_tag = current_tag.Next;
						} while (current_tag.Length == 0);

						current_tag.IsLink = true;
						current_tag.LinkText = link_text;
					}
				}

				//if there are characters left in the tag after the link
				// split the tag
				// make the second part a non link
				if (current_tag.End > (index_found + link_text.Length + 1) - (int)cumulative_length_list [current_cumulative - 1]) {
					if (current_tag == CaretTag)
						update_caret_tag = true;

					current_tag.Break ((index_found + link_text.Length + 1) - (int)cumulative_length_list [current_cumulative - 1]);
				}
			}

			if (update_caret_tag) {
				CaretTag = LineTag.FindTag (CaretLine, CaretPosition);
				link_changed = true;
			} else {
				if (link_changed == false) {
					current_line = start_line;
					StringBuilder new_link_record = new StringBuilder ();

					while (current_line != null) {
						current_line.LinkRecord (new_link_record);

						if (current_line.ending == LineEnding.Wrap)
							current_line = GetLine (current_line.LineNo + 1);
						else
							break;
					}

					if (new_link_record.Equals (line_link_record) == false)
						link_changed = true;
				}
			}
		}

		private int FirstIndexOfAny (string haystack, string [] needles, int start_index, out int term_found)
		{
			term_found = -1;
			int best_index = -1;

			for (int i = 0; i < needles.Length; i++) {
				int index = haystack.IndexOf (needles [i], start_index,	StringComparison.InvariantCultureIgnoreCase);

				if (index > -1) {
					if (term_found > -1) {
						if (index < best_index) {
							best_index = index;
							term_found = i;
						}
					} else {
						best_index = index;
						term_found = i;
					}
				}
			}

			return best_index;
		}



		private void InvalidateLinks (Rectangle clip)
		{
			for (int i = (owner.list_links.Count - 1); i >= 0; i--) {
				TextBoxBase.LinkRectangle link = (TextBoxBase.LinkRectangle) owner.list_links [i];

				if (clip.IntersectsWith (link.LinkAreaRectangle))
					owner.list_links.RemoveAt (i);
			}
		}
		#endregion	// Private Methods

		#region Internal Methods

		internal void ScanForLinks (int start, int end, ref bool link_changed)
		{
			Line line = null;
			LineEnding lastending = LineEnding.Rich;

			// make sure we start scanning at the real begining of the line
			while (true) {
				if (start != 1 && GetLine (start - 1).ending == LineEnding.Wrap)
					start--;
				else
					break;
			}

			for (int i = start; i <= end && i <= lines; i++) {
				line = GetLine (i);

				if (lastending != LineEnding.Wrap)
					ScanForLinks (line, ref link_changed);

				lastending = line.ending;

				if (lastending == LineEnding.Wrap && (i + 1) <= end)
					end++;
			}
		}

		// Clear the document and reset state
		internal void Empty() {

			document = sentinel;
			lines = 0;

			// We always have a blank line
			Add (1, String.Empty, owner.Font, owner.ForeColor, LineEnding.None);
			
			if (owner.IsHandleCreated)
				using (var graphics = owner.CreateGraphics())
					this.RecalculateDocument(graphics);
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
			caret.tag = line.FindTag (pos);
			if (pos == caret.tag.Start - 1 && caret.tag.Length != 0 && caret.tag.Previous != null)
				caret.tag = caret.tag.Previous;
			// When we're at a tag boundary we want the cursor in the previous (left) tag
			// whereas FindTag(pos) gets the next (right) tag. LineTag.Start is 1-based.

			if (pos > line.TextLengthWithoutEnding())
				pos = line.TextLengthWithoutEnding();
			// We don't want the caret after the line ending.

			MoveCaretToTextTag ();

			caret.line = line;
			caret.pos = pos;

			if (owner.IsHandleCreated) {
				if (owner.Focused) {
					if (caret.height != caret.tag.DrawnHeight) {
						caret.height = caret.tag.DrawnHeight;
						XplatUI.CreateCaret (owner.Handle, caret_width, caret.height);
					}
					SetCaretPos();
				}

				if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
			}
		}

		internal void PositionCaret(int x, int y) {
			if (!owner.IsHandleCreated) {
				return;
			}

			caret.tag = FindCursor(x, y, out caret.pos);

			if (caret.pos > caret.tag.Line.TextLengthWithoutEnding())
				caret.pos = caret.tag.Line.TextLengthWithoutEnding();
			// Don't allow the caret to be positioned after the line ending.
			// This was happening with the up and down arrows due to how FindCursor works.
			
			MoveCaretToTextTag ();
			
			caret.line = caret.tag.Line;
			caret.height = caret.tag.DrawnHeight;

			if (owner.ShowSelection && (!selection_visible || owner.show_caret_w_selection)) {
				XplatUI.CreateCaret (owner.Handle, caret_width, caret.height);
				SetCaretPos();
			}

			if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
		}

		internal void CaretHasFocus() {
			if ((caret.tag != null) && owner.IsHandleCreated) {
				XplatUI.CreateCaret(owner.Handle, caret_width, caret.height);
				SetCaretPos();

				DisplayCaret ();
			}

			if (owner.IsHandleCreated && SelectionLength () > 0) {
				InvalidateSelectionArea ();
			}
		}

		internal void CaretLostFocus() {
			if (!owner.IsHandleCreated) {
				return;
			}
			XplatUI.DestroyCaret(owner.Handle);
		}

		internal void AlignCaret ()
		{
			AlignCaret (true);
		}

		internal void AlignCaret(bool changeCaretTag) {
			if (!owner.IsHandleCreated) {
				return;
			}

			if (changeCaretTag) {
				caret.tag = LineTag.FindTag (caret.line, caret.pos);

				MoveCaretToTextTag ();
			}

			// if the caret has had SelectionFont changed to a
			// different height, we reflect changes unless the new
			// font is larger than the line (line recalculations
			// ignore empty tags) in which case we make it equal
			// the line height and then when text is entered
			if (caret.tag.DrawnHeight > caret.tag.Line.TextHeight) {
				caret.height = caret.line.TextHeight;
			} else {
				caret.height = caret.tag.DrawnHeight;
			}

			if (owner.Focused) {
				XplatUI.CreateCaret(owner.Handle, caret_width, caret.height);
				SetCaretPos();
				DisplayCaret ();
			}

			if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
		}

		internal void UpdateCaret() {
			if (!owner.IsHandleCreated || caret.tag == null) {
				return;
			}

			MoveCaretToTextTag ();

			if (caret.tag.DrawnHeight != caret.height) {
				caret.height = caret.tag.DrawnHeight;
				if (owner.Focused) {
					XplatUI.CreateCaret(owner.Handle, caret_width, caret.height);
				}
			}

			if (owner.Focused) {
				SetCaretPos();
				DisplayCaret ();
			}
			
			if (CaretMoved != null) CaretMoved(this, EventArgs.Empty);
		}

		void SetCaretPos ()
		{
			XplatUI.SetCaretPos (owner.Handle,
				(int)Math.Min(offset_x + caret.tag.Line.widths[caret.pos] + caret.line.X - viewport_x,
					viewport_width - caret.tag.Line.right_indent - caret_width), // Limit X, because whitespace can be outside this.
				(int)(offset_y + caret.line.Y + caret.line.SpacingBefore + caret.tag.OffsetY -
				caret.tag.CharOffset + caret.tag.Shift - viewport_y + caret_shift));
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

		
		internal void MoveCaretToTextTag ()
		{
			if (caret.tag == null || caret.tag.IsTextTag)
				return;

			

			if (caret.pos < caret.tag.Start) {
				caret.tag = caret.tag.Previous;
			} else {
				caret.tag = caret.tag.Next;
			}
		}

		internal void MoveCaret(CaretDirection direction) {
			// FIXME should we use IsWordSeparator to detect whitespace, instead 
			// of looking for actual spaces in the Word move cases?

			Line currentLine = caret.line;
			int currentPos = caret.pos;
			LineTag currentTag = caret.tag;

			bool nowrap = false;
			switch(direction) {
				case CaretDirection.CharForwardNoWrap:
					nowrap = true;
					goto case CaretDirection.CharForward;
				case CaretDirection.CharForward: {
					caret.pos++;
					if (caret.pos > caret.line.TextLengthWithoutEnding ()) {
						if (!nowrap) {
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
						if ((caret.tag.Start - 1 + caret.tag.Length) < caret.pos) {
							caret.tag = caret.tag.Next;
						}
					}
					UpdateCaret();
					break;
				}

				case CaretDirection.CharBackNoWrap:
					nowrap = true;
					goto case CaretDirection.CharBack;
				case CaretDirection.CharBack: {
					if (caret.pos > 0) {
						caret.pos--;
						
						if (caret.tag.Start > caret.pos && caret.tag.Previous != null) {
							caret.tag = caret.tag.Previous;
						}
					} else {
						if (caret.line.line_no > 1 && !nowrap) {
							caret.line = GetLine(caret.line.line_no - 1);
							caret.pos = caret.line.TextLengthWithoutEnding ();
							caret.tag = LineTag.FindTag(caret.line, caret.pos);
						}
					}
					UpdateCaret();
					break;
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
					break;
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
					break;
				}

				case CaretDirection.LineUp: {
					if (caret.line.line_no > 1) {
						int	pixel;

						pixel = (int)caret.line.widths[caret.pos] + caret.line.align_shift;
						PositionCaret(pixel, GetLine(caret.line.line_no - 1).Y);

						DisplayCaret ();
					}
					break;
				}

				case CaretDirection.LineDown: {
					if (caret.line.line_no < lines) {
						int	pixel;

						pixel = (int)caret.line.widths[caret.pos] + caret.line.align_shift;
						PositionCaret(pixel, GetLine(caret.line.line_no + 1).Y);

						DisplayCaret ();
					}
					break;
				}

				case CaretDirection.Home: {
					if (caret.pos > 0) {
						caret.pos = 0;
						caret.tag = caret.line.tags;
						UpdateCaret();
					}
					break;
				}

				case CaretDirection.End: {
					if (caret.pos < caret.line.TextLengthWithoutEnding ()) {
						caret.pos = caret.line.TextLengthWithoutEnding ();
						caret.tag = LineTag.FindTag(caret.line, caret.pos);
						UpdateCaret();
					}
					break;
				}

				case CaretDirection.PgUp: {

					if (caret.line.line_no == 1 && owner.richtext) {
						owner.vscroll.Value = 0;
						Line line = GetLine (1);
						PositionCaret (line, 0);
						break;
					}

					int y_offset = caret.line.Y + caret.line.height - 1 - viewport_y;
					int expected_y = viewport_y - viewport_height;
					int index;
					LineTag top = FindCursor ((int) caret.line.widths [caret.pos] + caret.line.align_shift,
							expected_y, out index);

					owner.vscroll.Value = Math.Min (top.Line.Y, owner.vscroll.Maximum - viewport_height);
					PositionCaret ((int) caret.line.widths [caret.pos] + caret.line.align_shift,
						(expected_y >= 0) ? y_offset + viewport_y : 0);

					break;
				}

				case CaretDirection.PgDn: {

					if (caret.line.line_no == lines && owner.richtext) {
						owner.vscroll.Value = owner.vscroll.Maximum - viewport_height + 1;
						Line line = GetLine (lines);
						PositionCaret (line, line.TextLengthWithoutEnding());
						break;
					}

					int y_offset = caret.line.Y - viewport_y;
					int expected_y = viewport_y + viewport_height;
					int index;
					LineTag top = FindCursor ((int) caret.line.widths [caret.pos] + caret.line.align_shift,
							expected_y, out index);

					owner.vscroll.Value = Math.Min (top.Line.Y, owner.vscroll.Maximum - viewport_height);
					PositionCaret ((int) caret.line.widths [caret.pos] + caret.line.align_shift,
						(expected_y <= document_y - viewport_height) ? y_offset + viewport_y : document_y);
					
					break;
				}

				case CaretDirection.CtrlPgUp: {
					PositionCaret(0, viewport_y);
					DisplayCaret ();
					break;
				}

				case CaretDirection.CtrlPgDn: {
					Line	line;
					LineTag	tag;
					int	index;

					tag = FindCursor (0, viewport_y + viewport_height, out index);
					if (tag.Line.line_no > 1) {
						line = GetLine(tag.Line.line_no - 1);
					} else {
						line = tag.Line;
					}
					PositionCaret(line, line.Text.Length);
					DisplayCaret ();
					break;
				}

				case CaretDirection.CtrlHome: {
					caret.line = GetLine(1);
					caret.pos = 0;
					caret.tag = caret.line.tags;

					UpdateCaret();
					break;
				}

				case CaretDirection.CtrlEnd: {
					caret.line = GetLine(lines);
					caret.pos = caret.line.TextLengthWithoutEnding ();
					caret.tag = LineTag.FindTag(caret.line, caret.pos);

					UpdateCaret();
					break;
				}

				case CaretDirection.SelectionStart: {
					caret.line = selection_start.line;
					caret.pos = selection_start.pos;
					caret.tag = selection_start.tag;

					UpdateCaret();
					break;
				}

				case CaretDirection.SelectionEnd: {
					caret.line = selection_end.line;
					caret.pos = selection_end.pos;
					caret.tag = selection_end.tag;

					UpdateCaret();
					break;
				}
			}

			if ((caret.pos != currentPos || caret.line != currentLine) && currentTag.Length == 0) {
				// Remove the empty tag it was previously on.
				if (currentTag.Previous != null) {
					currentTag.Previous.Next = currentTag.Next;
				} else if (currentTag.Next != null) {
					// update line.tags, but don't set it to null!
					currentLine.tags = currentTag.Next;
				}
				if (currentTag.Next != null)
					currentTag.Next.Previous = currentTag.Previous;
			}
		}

		internal void DumpDoc ()
		{
			Console.WriteLine ("<doc lines='{0}' width='{1}' height='{2}' ownerwidth='{3}' ownerheight='{4}'>",
				lines, document_x, document_y, owner.Width, owner.Height);
			for (int i = 1; i <= lines ; i++) {
				Line line = GetLine (i);
				Console.WriteLine ("<line no='{0}' ending='{1}' x='{2}' y='{3}' width='{4}' height='{5}' indent='{6}' hanging-indent='{7}' right-indent='{8}' spacing-before='{9}' spacing-after='{10}'>",
					line.line_no, line.ending, line.X, line.Y, line.Width, line.Height, line.Indent, line.HangingIndent, line.RightIndent, line.SpacingBefore, line.SpacingAfter);

				LineTag tag = line.tags;
				while (tag != null) {
					Console.Write ("\t<tag type='{0}' span='{1}->{2}' font='{3}' color='{4}' position='{5}' " +
						"charoffset='{6}' x='{7}' width='{8}' height='{11}' ascent='{9}' descent='{10}'>",
						tag.GetType (), tag.Start, tag.Length, tag.Font, tag.Color, tag.TextPosition,
						tag.CharOffset, tag.X, tag.Width, tag.Ascent, tag.Descent, tag.MaxHeight());
					Console.Write (tag.Text ());
					Console.WriteLine ("</tag>");
					tag = tag.Next;
				}
				Console.WriteLine ("</line>");
			}
			Console.WriteLine ("</doc>");
		}

		// UIA: Used via reflection by TextProviderBehavior
		internal void GetVisibleLineIndexes (Rectangle clip, out int start, out int end)
		{
			if (multiline) {
				/* Expand the region slightly to be sure to
				 * paint the full extent of the line of text.
				 * See bug 464464.
				 */
				start = GetLineByPixel(clip.Top + viewport_y - offset_y - 1, false).line_no;
				end = GetLineByPixel(clip.Bottom + viewport_y - offset_y + 1, false).line_no;
			} else {
				start = GetLineByPixel(clip.Left + viewport_x - offset_x, false).line_no;
				end = GetLineByPixel(clip.Right + viewport_x - offset_x, false).line_no;
			}
		}

		internal void Draw (Graphics g, Rectangle clip)
		{
			Line line;		// Current line being drawn
			LineTag	tag;		// Current tag being drawn
			int start;		// First line to draw
			int end;		// Last line to draw
			StringBuilder text;	// String representing the current line
			int line_no;
			Color tag_color;
			Color tag_backcolor;
			Color current_color;
			Color current_backcolor;

			// First, figure out from what line to what line we need to draw
			GetVisibleLineIndexes (clip, out start, out end);

			// remove links in the list (used for mouse down events) that are within the clip area.
			InvalidateLinks (clip);

			///
			/// We draw the single border ourself
			///
			if (owner.actual_border_style == BorderStyle.FixedSingle) {
				ControlPaint.DrawBorder (g, owner.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
			}

			/// Make sure that we aren't drawing one more line then we need to
			line = GetLine (end - 1);
			if (line != null && clip.Bottom == offset_y + line.Y + line.height - viewport_y)
				end--;

			line_no = start;

			#if Debug
				DateTime	n = DateTime.Now;
				Console.WriteLine ("Started drawing: {0}s {1}ms", n.Second, n.Millisecond);
				Console.WriteLine ("CLIP:  {0}", clip);
				Console.WriteLine ("S: {0}", GetLine (start).text);
				Console.WriteLine ("E: {0}", GetLine (end).text);
			#endif

			// Non multiline selection can be handled outside of the loop
			if (!multiline && selection_visible && owner.ShowSelection) {
				g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorHighlight),
						offset_x + selection_start.line.widths [selection_start.pos] +
						selection_start.line.X - viewport_x, 
				        offset_y + selection_start.line.Y,
						(selection_end.line.X + selection_end.line.widths [selection_end.pos]) -
						(selection_start.line.X + selection_start.line.widths [selection_start.pos]), 
				        selection_start.line.height);
			}

			while (line_no <= end) {
				line = GetLine (line_no);
				float line_y = line.Y - viewport_y + offset_y + line.SpacingBefore;
				
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
					} else if (multiline) {
						// lets draw some selection baby!!  (non multiline selection is drawn outside the loop)
						g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorHighlight),
							offset_x + line.widths [line_selection_start - 1] + line.X - viewport_x, line_y - line.SpacingBefore,
							line.widths [line_selection_end - 1] - line.widths [line_selection_start - 1], line.height);
					}
				}

				while (tag != null) {

					// Skip empty tags
					if (tag.Length == 0 || !tag.Visible) {
						tag = tag.Next;
						continue;
					}

					if (((tag.X + tag.Width) < (clip.Left + viewport_x - offset_x)) ||
					     (tag.X > (clip.Right + viewport_x - offset_x))) {
						// Don't draw a tag that is horizontally outside the visible region.
						tag = tag.Next;
						continue;
					}

					tag_color = tag.ColorToDisplay;
					tag_backcolor = tag.BackColor;

					if (!owner.Enabled) {
						Color a = tag.Color;
						Color b = ThemeEngine.Current.ColorWindowText;

						if ((a.R == b.R) && (a.G == b.G) && (a.B == b.B))
							tag_color = ThemeEngine.Current.ColorGrayText;

					} 

					int tag_pos = tag.Start;
					while (tag_pos < tag.Start + tag.Length) {
						int old_tag_pos = tag_pos;

						if (tag_pos >= line_selection_start && tag_pos < line_selection_end) {
							current_color = ThemeEngine.Current.ColorHighlightText;
							tag_pos = Math.Min (tag.End, line_selection_end);
							current_backcolor = Color.Empty;
						} else if (tag_pos < line_selection_start) {
							current_color = tag_color;
							tag_pos = Math.Min (tag.End, line_selection_start);
							current_backcolor = tag_backcolor;
						} else {
							current_color = tag_color;
							tag_pos = tag.End;
							current_backcolor = tag_backcolor;
						}

						if (current_backcolor != Color.Empty && current_backcolor != owner.BackColor) {
							g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (current_backcolor),
							    offset_x + line.widths [old_tag_pos - 1] + line.X - viewport_x,
							    line_y - line.SpacingBefore,
							    line.widths [Math.Min (tag.Start + tag.Length, tag_pos) - 1] - line.widths [old_tag_pos - 1],
							    line.height);
						}

						Rectangle text_size;

						tag.Draw (g, current_color,
								offset_x + line.X - viewport_x,
								line_y + tag.Shift - tag.CharOffset,
								old_tag_pos - 1, Math.Min (tag.Start + tag.Length, tag_pos) - 1,
								text.ToString (), out text_size, tag.IsLink);

						if (tag.IsLink) {
							TextBoxBase.LinkRectangle link = new TextBoxBase.LinkRectangle (text_size);
							link.LinkTag = tag;
							owner.list_links.Add (link);
						}
					}
					tag = tag.Next;
				}

				line.DrawEnding (g, line_y);
				line_no++;
			}
		}

		private int GetLineEnding (string line, int start, out LineEnding ending)
		{
			int res;
			int rich_index;

			if (start >= line.Length) {
				ending = LineEnding.Wrap;
				return -1;
			}
			
			res = line.IndexOf ('\r', start);
			rich_index = line.IndexOf ('\n', start);
			
			// Handle the case where we find both of them, and the \n is before the \r
			if (res != -1 && rich_index != -1)
				if (rich_index < res) {
					ending = LineEnding.Rich;
					return rich_index;				
				}
			
			if (res != -1) {
				if (res + 2 < line.Length && line [res + 1] == '\r' && line [res + 2] == '\n') {
					ending = LineEnding.Soft;
					return res;
				}
				if (res + 1 < line.Length && line [res + 1] == '\n') {
					ending = LineEnding.Hard;
					return res;
				}
				ending = LineEnding.Limp;
				return res;
			}

			if (rich_index != -1) {
				ending = LineEnding.Rich;
				return rich_index;
			}

			ending = LineEnding.Wrap;
			return line.Length;
		}

		// Get the line ending, but only of the types specified
		private int GetLineEnding (string line, int start, out LineEnding ending, LineEnding type)
		{
			int index = start;
			int last_length = 0;

			do {
				index = GetLineEnding (line, index + last_length, out ending);
				last_length = LineEndingLength (ending);
			} while 
				((ending & type) != ending && index != -1);
			
			return index == -1 ? line.Length : index;
		}
		
		internal int LineEndingLength (LineEnding ending)
		{
			switch (ending) {
				case LineEnding.Limp:
				case LineEnding.Rich:
					return 1;
				case LineEnding.Hard:
					return 2;
				case LineEnding.Soft:
					return 3;
			}

			return 0;
		}

		internal string LineEndingToString (LineEnding ending)
		{
			switch (ending) {
				case LineEnding.Limp:
					return "\r";
				case LineEnding.Hard:
					return "\r\n";
				case LineEnding.Soft:
					return "\r\r\n";
				case LineEnding.Rich:
					return "\n";
			}
			
			return string.Empty;
		}

		internal LineEnding StringToLineEnding (string ending)
		{
			switch (ending) {
				case "\r":
					return LineEnding.Limp;
				case "\r\n":
					return LineEnding.Hard;
				case "\r\r\n":
					return LineEnding.Soft;
				case "\n":
					return LineEnding.Rich;
				default:
					return LineEnding.None;
			}
		}
		
		internal void Insert (Line line, int pos, bool update_caret, string s)
		{
			LineTag tag = line.FindTag(pos);
			if (tag.Length != 0) {
				if (tag.Start == pos + 1) { // pos is zero-based, tag.Start and tag.End are one-based.
					// Check for empty tags before this one at the same position
					var t = tag.Previous;
					while (t != null && t.End == pos + 1) {
						if (t.Length == 0) {
							tag = t;
							break;
						}
						t = t.Previous;
					}
				} // There will never be empty tags after this one, because FindTag gets the last tag at the position.
			}
			Insert (line, pos, update_caret, s, tag);
		}

		// Insert text at the given position; use formatting at insertion point for inserted text
		internal void Insert (Line line, int pos, bool update_caret, string s, LineTag tag)
		{
			int break_index;
			int base_line;
			int old_line_count;
			int count = 1;
			LineEnding ending;
			Line split_line;
			
			// Don't recalculate while we mess around
			SuspendRecalc ();
			
			base_line = line.line_no;
			old_line_count = lines;

			// Discard chars after any possible -unlikely- end of file
			int eof_index = s.IndexOf ('\0');
			if (eof_index != -1)
				s = s.Substring (0, eof_index);

			break_index = GetLineEnding (s, 0, out ending, LineEnding.Hard | LineEnding.Rich);

			// There are no line feeds in our text to be pasted
			if (break_index == s.Length) {
				line.InsertString (pos, s, tag);
				CharCount += s.Length;
			} else {
				// Add up to the first line feed to our current position
				line.InsertString (pos, s.Substring (0, break_index + LineEndingLength (ending)), tag);
				CharCount += break_index + LineEndingLength (ending);
				
				// Split the rest of the original line to a new line
				Split (line, pos + (break_index + LineEndingLength (ending)));
				line.ending = ending;
				break_index += LineEndingLength (ending);
				split_line = GetLine (line.line_no + 1);
				
				// Insert brand new lines for any more line feeds in the inserted string
				while (true) {
					int next_break = GetLineEnding (s, break_index, out ending, LineEnding.Hard | LineEnding.Rich);
					
					if (next_break == s.Length)
						break;
						
					string line_text = s.Substring (break_index, next_break - break_index +
							LineEndingLength (ending));

					Add (base_line + count, line_text, line.alignment, tag.Font, tag.Color, ending);

					Line last = GetLine (base_line + count);
					last.ending = ending;

					count++;
					break_index = next_break + LineEndingLength (ending);
				}

				// Add the remainder of the insert text to the split
				// part of the original line
				CharCount += s.Length - break_index;
				split_line.InsertString (0, s.Substring (break_index));
			}
			
			// Allow the document to recalculate things
			ResumeRecalc (false);

			UpdateView (line, lines - old_line_count + 1, pos);

			// Move the caret to the end of the inserted text if requested
			if (update_caret) {
				Line l = GetLine (line.line_no + lines - old_line_count);
				PositionCaret (l, l.text.Length);
				DisplayCaret ();
			}
		}

		// Inserts a string at the given position
		internal void InsertString (Line line, int pos, string s)
		{
			// Update our character count
			CharCount += s.Length;

			// Insert the text into the Line
			line.InsertString (pos, s);
		}

		// Inserts a character at the current caret position
		internal void InsertCharAtCaret (char ch, bool move_caret)
		{
			caret.line.InsertString (caret.pos, ch.ToString(), caret.tag);

			// Update our character count
			CharCount++;
			
			undo.RecordTyping (caret.line, caret.pos, ch);

			UpdateView (caret.line, caret.pos);
			
			if (move_caret) {
				caret.pos++;
				UpdateCaret ();
				SetSelectionToCaret (true);
			}
		}
		
		internal void InsertPicture (Line line, int pos, RTF.Picture picture)
		{
			//LineTag next_tag;
			LineTag tag;
			int len;

			len = 1;

			// Just a place holder basically
			line.text.Insert (pos, "I");

			PictureTag picture_tag = new PictureTag (line, pos + 1, picture);

			tag = LineTag.FindTag (line, pos);
			picture_tag.CopyFormattingFrom (tag);
			/*next_tag = */tag.Break (pos + 1);
			picture_tag.Previous = tag;
			picture_tag.Next = tag.Next;
			tag.Next = picture_tag;

			//
			// Picture tags need to be surrounded by text tags
			//
			if (picture_tag.Next == null) {
				picture_tag.Next = new LineTag (line, pos + 1);
				picture_tag.Next.CopyFormattingFrom (tag);
				picture_tag.Next.Previous = picture_tag;
			}

			tag = picture_tag.Next;
			while (tag != null) {
				tag.Start += len;
				tag = tag.Next;
			}

			line.Grow (len);
			line.recalc = true;

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
				DeleteChars (start.line, pos, end.pos - pos);
			} else {

				// Delete first and last lines
				DeleteChars (start.line, start.pos, start.line.text.Length - start.pos);
				DeleteChars (end.line, 0, end.pos);

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
		public void DeleteChars (Line line, int pos, int count)
		{
			// Reduce our character count
			CharCount -= count;
			
			line.DeleteCharacters (pos, count);

			if (pos >= line.TextLengthWithoutEnding ()) {
				LineEnding ending = line.ending;
				GetLineEnding (line.text.ToString (), 0, out ending);
				
				if (ending != line.ending) {
					line.ending = ending;

					if (!multiline) {
						UpdateView (line, lines, pos);
						owner.Invalidate ();
						return;
					}
				}
			}
			if (!multiline) {
				UpdateView (line, lines, pos);
				owner.Invalidate ();
			} else {
				if (line.line_no > 1) {
					// If the previous line is wrapped, we update that too in case the wrap point has changed.
					var l = GetLine(line.line_no - 1);
					if (l != null && (l.ending == LineEnding.None || l.ending == LineEnding.Wrap))
						line = l;
				}
				UpdateView (line, pos);
			}
		}

		// Deletes a character at or after the given position (depending on forward); it will not delete past line limits
		public void DeleteChar (Line line, int pos, bool forward)
		{
			if ((pos == 0 && forward == false) || (pos == line.text.Length && forward == true))
				return;
			
			undo.BeginUserAction ("Delete");

			if (forward) {
				undo.RecordDeleteString (line, pos, line, pos + 1);
				DeleteChars (line, pos, 1);
			} else {
				undo.RecordDeleteString (line, pos - 1, line, pos);
				DeleteChars (line, pos - 1, 1);
			}

			undo.EndUserAction ();
		}

		// Combine two lines
		internal void Combine(int FirstLine, int SecondLine) {
			Combine(GetLine(FirstLine), GetLine(SecondLine));
		}

		internal void Combine(Line first, Line second) {
			LineTag	last;
			int	shift;

			// strip the ending off of the first lines text
			first.text.Length = first.text.Length - LineEndingLength (first.ending);

			// Combine the two tag chains into one
			last = first.tags;

			// Maintain the line ending style
			first.ending = second.ending;

			while (last.Next != null) {
				last = last.Next;
			}

			// need to get the shift before setting the next tag since that effects length
			shift = last.Start + last.Length - 1;
			last.Next = second.tags;
			last.Next.Previous = last;

			// Fix up references within the chain
			last = last.Next;
			while (last != null) {
				last.Line = first;
				last.Start += shift;
				last = last.Next;
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
			Split(line, tag, pos);
		}

		internal void Split(Line line, int pos) {
			LineTag	tag;

			tag = LineTag.FindTag(line, pos);
			Split(line, tag, pos);
		}

		///<summary>Split line at given tag and position into two lines</summary>
		///if more space becomes available on previous line
		internal void Split(Line line, LineTag tag, int pos) {
			LineTag	new_tag;
			Line	new_line;
			bool	move_caret;
			bool	move_sel_start;
			bool	move_sel_end;

			move_caret = false;
			move_sel_start = false;
			move_sel_end = false;

#if DEBUG
			SanityCheck();

			if (tag.End < pos)
				throw new Exception ("Split called with the wrong tag");
#endif

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
				Add (line.line_no + 1, String.Empty, line.alignment, tag.Font, tag.Color, tag.BackColor, tag.TextPosition,
				     tag.CharOffset, line.Indent, line.HangingIndent, line.RightIndent, line.spacing_before, line.spacing_after,
					 line.line_spacing, line.line_spacing_multiple, line.tab_stops, tag.Visible, line.ending);

				new_line = GetLine (line.line_no + 1);
				
				if (move_caret) {
					caret.line = new_line;
					caret.tag = new_line.tags;
					caret.pos = 0;

					if (selection_visible == false) {
						SetSelectionToCaret (true);
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

#if DEBUG
				SanityCheck ();
#endif
				return;
			}

			// We need to move the rest of the text into the new line
			Add (line.line_no + 1, line.text.ToString (pos, line.text.Length - pos), line.alignment, tag.Font, tag.Color,
			     tag.BackColor, tag.TextPosition, tag.CharOffset, line.Indent, line.HangingIndent, line.RightIndent,
				 line.spacing_before, line.spacing_after, line.line_spacing, line.line_spacing_multiple, line.tab_stops,
				 tag.Visible, line.ending);

			// Now transfer our tags from this line to the next
			new_line = GetLine(line.line_no + 1);

			line.recalc = true;
			new_line.recalc = true;

			//make sure that if we are at the end of a tag, we start on the begining
			//of a new one, if one exists... Stops us creating an empty tag and
			//make the operation easier.
			if (tag.Next != null && (tag.Next.Start - 1) == pos)
				tag = tag.Next;

			if ((tag.Start - 1) == pos) {
				int	shift;

				// We can simply break the chain and move the tag into the next line

				// if the tag we are moving is the first, create an empty tag
				// for the line we are leaving behind
				if (tag == line.tags) {
					new_tag = new LineTag(line, 1);
					new_tag.CopyFormattingFrom (tag);
					line.tags = new_tag;
				}

				if (tag.Previous != null) {
					tag.Previous.Next = null;
				}
				new_line.tags = tag;
				tag.Previous = null;
				tag.Line = new_line;

				// Walk the list and correct the start location of the tags we just bumped into the next line
				shift = tag.Start - 1;

				new_tag = tag;
				while (new_tag != null) {
					new_tag.Start -= shift;
					new_tag.Line = new_line;
					new_tag = new_tag.Next;
				}
			} else {
				int	shift;

				new_tag = new LineTag (new_line, 1);			
				new_tag.Next = tag.Next;
				new_tag.CopyFormattingFrom (tag);
				new_line.tags = new_tag;
				if (new_tag.Next != null) {
					new_tag.Next.Previous = new_tag;
				}
				tag.Next = null;

				shift = pos;
				new_tag = new_tag.Next;
				while (new_tag != null) {
					new_tag.Start -= shift;
					new_tag.Line = new_line;
					new_tag = new_tag.Next;

				}
			}

			if (move_caret) {
				caret.line = new_line;
				caret.pos = caret.pos - pos;
				caret.tag = caret.line.FindTag(caret.pos);

				if (selection_visible == false) {
					SetSelectionToCaret (true);
					move_sel_start = false;
					move_sel_end = false;
				}
			}

			if (move_sel_start) {
				selection_start.line = new_line;
				selection_start.pos = selection_start.pos - pos;
				if  (selection_start.Equals(selection_end))
					selection_start.tag = new_line.FindTag(selection_start.pos);
				else
					selection_start.tag = new_line.FindTag (selection_start.pos + 1);
			}

			if (move_sel_end) {
				selection_end.line = new_line;
				selection_end.pos = selection_end.pos - pos;
				selection_end.tag = new_line.FindTag(selection_end.pos);
			}

			CharCount -= line.text.Length - pos;
			line.text.Remove(pos, line.text.Length - pos);
#if DEBUG
			SanityCheck ();
#endif
		}

#if DEBUG
		private void SanityCheck () {
			for (int i = 1; i < lines; i++) {
				LineTag tag = GetLine (i).tags;

				if (tag.Start != 1)
					throw new Exception ("Line doesn't start at the begining");

				int start = 1;
				tag = tag.Next;

				while (tag != null) {
					if (tag.Start == start)
						throw new Exception ("Empty tag!");

					if (tag.Start < start)
						throw new Exception ("Insane!!");

					start = tag.Start;
					tag = tag.Next;
				}
			}
		}
#endif

		// Adds a line of text, with given font.
		// Bumps any line at that line number that already exists down
		internal void Add (int LineNo, string Text, Font font, Color color, LineEnding ending)
		{
			Add (LineNo, Text, alignment, font, color, ending);
		}

		internal void Add (int LineNo, string Text, HorizontalAlignment align, Font font, Color color, LineEnding ending)
		{
			Add (LineNo, Text, align, font, color, Color.Empty, TextPositioning.Normal,
				0, 0, 0, 0, 0, 0, 0, false, new TabStopCollection(), true, ending);
		}

		internal void Add (int LineNo, string Text, HorizontalAlignment align, Font font, Color color, Color back_color,
		                   TextPositioning text_position, float char_offset, float left_indent, float hanging_indent,
		                   float right_indent, float spacing_before, float spacing_after, float line_spacing,
		                   bool line_spacing_multiple, TabStopCollection tab_stops, bool visible, LineEnding ending)
		{
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

			CharCount += Text.Length;

			add = new Line (this, LineNo, Text, align, font, color, back_color, text_position, char_offset, left_indent,
				hanging_indent, right_indent, spacing_before, spacing_after, line_spacing, line_spacing_multiple, tab_stops, visible, ending);

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

		private void Delete (int LineNo)
		{
			Line	line;

			if (LineNo > lines)
				return;

			line = GetLine (LineNo);

			CharCount -= line.text.Length;

			DecrementLines (LineNo + 1);
			Delete (line);
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
				line1.ending = line3.ending;
				line1.space = line3.space;
				line1.tags = line3.tags;
				line1.text = line3.text;
				line1.widths = line3.widths;
				line1.offset = line3.offset;

				tag = line1.tags;
				while (tag != null) {
					tag.Line = line1;
					tag = tag.Next;
				}
			}

			if (line3.color == LineColor.Black)
				RebalanceAfterDelete(line2);

			this.lines--;
		}

		// Invalidates the start line until the end of the viewstate
		internal void InvalidateLinesAfter (Line start) {
			owner.Invalidate (new Rectangle (0, start.Y - viewport_y, viewport_width, viewport_height - start.Y));
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
								(int)l1.widths[p1] + l1.X - viewport_x, 
								l1.Y - viewport_y, 
								(int)l1.widths[p2], 
								l1.height
								)
						);
				#endif

				owner.Invalidate(new Rectangle (
					offset_x + (int)l1.widths[p1] + l1.X - viewport_x, 
					offset_y + l1.Y - viewport_y,
					endpoint - (int) l1.widths [p1] + 1, 
					l1.height));
				return;
			}

			#if Debug
				Console.WriteLine("Invaliding from {0}:{1} to {2}:{3} Start  => x={4}, y={5}, {6}x{7}", l1.line_no, p1, l2.line_no, p2, (int)l1.widths[p1] + l1.X - viewport_x, l1.Y - viewport_y, viewport_width, l1.height);
				Console.WriteLine ("invalidate start line:  {0}  position:  {1}", l1.text, p1);
			#endif

			// Three invalidates:
			// First line from start
			owner.Invalidate(new Rectangle(
				offset_x + (int)l1.widths[p1] + l1.X - viewport_x, 
				offset_y + l1.Y - viewport_y, 
				viewport_width, 
				l1.height));

			
			// lines inbetween
			if ((l1.line_no + 1) < l2.line_no) {
				int	y;

				y = GetLine(l1.line_no + 1).Y;
				owner.Invalidate(new Rectangle(
					offset_x, 
					offset_y + y - viewport_y, 
					viewport_width, 
					l2.Y - y));

				#if Debug
					Console.WriteLine("Invaliding from {0}:{1} to {2}:{3} Middle => x={4}, y={5}, {6}x{7}", l1.line_no, p1, l2.line_no, p2, 0, y - viewport_y, viewport_width, l2.Y - y);
				#endif
			}
			

			// Last line to end
			owner.Invalidate(new Rectangle(
				offset_x + (int)l2.widths[0] + l2.X - viewport_x, 
				offset_y + l2.Y - viewport_y, 
				(int)l2.widths[p2] + 1, 
				l2.height));

			#if Debug
				Console.WriteLine("Invaliding from {0}:{1} to {2}:{3} End    => x={4}, y={5}, {6}x{7}", l1.line_no, p1, l2.line_no, p2, (int)l2.widths[0] + l2.X - viewport_x, l2.Y - viewport_y, (int)l2.widths[p2] + 1, l2.height);
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
							selection_start.tag = selection_anchor.line.FindTag(selection_anchor.height + 1);

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
							selection_start.tag = caret.line.FindTag(start_pos + 1);
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
							selection_start.tag = selection_anchor.line.FindTag(selection_anchor.height + 1);

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
						selection_start.tag = caret.line.FindTag(start_pos + 1);
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

		internal void SetSelectionStart(Line start, int start_pos, bool invalidate) {
			// Invalidate from the previous to the new start pos
			if (invalidate)
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

			if (invalidate)
				Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
		}

		internal void SetSelectionStart(int character_index, bool invalidate) {
			Line	line;
			LineTag	tag;
			int	pos;

			if (character_index < 0) {
				return;
			}

			CharIndexToLineTag(character_index, out line, out tag, out pos);
			SetSelectionStart(line, pos, invalidate);
		}

		internal void SetSelectionEnd(Line end, int end_pos, bool invalidate) {

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
				if (invalidate)
					Invalidate(selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);
			} else {
				SetSelectionVisible (false);
				// ?? Do I need to invalidate here, tests seem to work without it, but I don't think they should :-s
			}
		}

		internal void SetSelectionEnd(int character_index, bool invalidate) {
			Line	line;
			LineTag	tag;
			int	pos;

			if (character_index < 0) {
				return;
			}

			CharIndexToLineTag(character_index, out line, out tag, out pos);
			SetSelectionEnd(line, pos, invalidate);
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

			if (selection_start.line == selection_end.line) {
				return selection_start.line.text.ToString (selection_start.pos, selection_end.pos - selection_start.pos);
			} else {
				StringBuilder	sb;
				int		i;
				int		start;
				int		end;

				sb = new StringBuilder();
				start = selection_start.line.line_no;
				end = selection_end.line.line_no;

				sb.Append(selection_start.line.text.ToString(selection_start.pos, selection_start.line.text.Length - selection_start.pos));

				if ((start + 1) < end) {
					for (i = start + 1; i < end; i++) {
						sb.Append(GetLine(i).text.ToString());
					}
				}

				sb.Append(selection_end.line.text.ToString(0, selection_end.pos));

				return sb.ToString();
			}
		}

		internal void ReplaceSelection(string s, bool select_new) {
			int		i;

			int selection_start_pos = LineTagToCharIndex (selection_start.line, selection_start.pos);
			SuspendRecalc ();

			var formatTag = selection_start.tag;

			// Delete any selected text
			if ((selection_start.pos != selection_end.pos) || (selection_start.line != selection_end.line)) {
				if (selection_start.line == selection_end.line) {
					undo.RecordDeleteString (selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);

					DeleteChars (selection_start.line, selection_start.pos, selection_end.pos - selection_start.pos);

					// The tag might have been removed, we need to recalc it
					selection_start.tag = selection_start.line.FindTag(selection_start.pos + 1);
				} else {
					int		start;
					int		end;

					start = selection_start.line.line_no;
					end = selection_end.line.line_no;

					undo.RecordDeleteString (selection_start.line, selection_start.pos, selection_end.line, selection_end.pos);

					InvalidateLinesAfter(selection_start.line);

					// Delete first line
					DeleteChars (selection_start.line, selection_start.pos, selection_start.line.text.Length - selection_start.pos);
					selection_start.line.recalc = true;

					// Delete last line
					DeleteChars(selection_end.line, 0, selection_end.pos);

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

			if (!String.IsNullOrEmpty(s)) {
				int old_line_count = lines;
				Insert(selection_start.line, selection_start.pos, false, s);
				Line end_line;
				int end_pos;
				if (lines == old_line_count) {
					end_line = selection_start.line;
					end_pos = selection_start.pos + s.Length + 1;
				} else {
					end_line = GetLine(selection_start.line.line_no + lines - old_line_count);
					end_pos = end_line.text.Length;
				}
				FormatText(selection_start.line, selection_start.pos + 1, end_line, end_pos, formatTag); // 0-base to 1-base...
				undo.RecordInsertString(selection_start.line, selection_start.pos, s);
			}



			Line begin_update_line = selection_start.line;
			int begin_update_pos = selection_start.pos;
			
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

			ResumeRecalc (false);
			PositionCaret (selection_start.line, selection_start.pos);

			if (begin_update_line.line_no > selection_start.line.line_no) {
				begin_update_line = selection_start.line;
				begin_update_pos = selection_start.pos;
			}
			UpdateView (begin_update_line, selection_end.line.line_no - begin_update_line.line_no, begin_update_pos);
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
				chars += line.text.Length;

				if (index < chars) {
					// we found the line
					tag = line.tags;

					while (tag != null) {
						if (index < (start + tag.Start + tag.Length - 1)) {
							line_out = line;
							tag_out = LineTag.GetFinalTag (tag);
							pos = index - start;
							return;
						}
						if (tag.Next == null) {
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
						tag = tag.Next;
					}
				}
			}

			line_out = GetLine(lines);
			tag = line_out.tags;
			while (tag.Next != null) {
				tag = tag.Next;
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
				length += GetLine(i).text.Length;
			}

			length += pos;

			return length;
		}

		internal int SelectionLength() {
			if ((selection_start.pos == selection_end.pos) && (selection_start.line == selection_end.line)) {
				return 0;
			}

			if (selection_start.line == selection_end.line) {
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
						length += line.text.Length + LineEndingLength (line.ending);
					}
				}

				return length;
			}

			
		}


		// UIA: Method used via reflection in TextRangeProvider

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

		internal IEnumerable<Line> TransverseLines (int start, int end)
		{
			Line l, c, r, prev = null;
			bool r2l = start > end;
			int number;
			if (r2l) {
				// swap start and end so that start is less than end
				int s = start;
				start = end;
				end = s;
			}
			c = document;
			while (c != null && c != sentinel) {
				l = c.left;
				r = c.right;

				if (((r2l && c.line_no < end) || (!r2l && c.line_no > start)) && (r2l ? r : l) != sentinel && (r2l ? r : l) != prev) {
					// There's no point going further this way if we're just finding lines we don't want!
					c = r2l ? r : l;
					continue;
				}

				number = c.line_no;
				if (number >= start && number <= end)
					yield return c;

				if ((r2l && number <= start) || (!r2l && number >= end))
					yield break; // We're done here, no need to look further.

				if ((r2l ? l : r) != sentinel) {
					c = r2l ? l : r;
				} else {
					// If we're on the first-side node, the parent is finished with too (continues up tree), otherwise we're only done with the current node.
					// We don't want to come back to first-side nodes we've already done when we do the new parent.
					// The highest node we discard is going to be a first-side node, because we're discarding all second-side nodes we run into.
					// The exception is when we run off the top, which is just fine too, because we're done.
					// But the highest node we discard is already visited, so that is the one we're not allowed back to -- anything higher needs visiting still.
					prev = c;
					c = c.parent;
					// The xor inverts the condition when we're going right-to-left, and therefore trims non-right (i.e. left) branches.
					// With both forwards and reverse transversal, prev will be given the first-side node.
					while (c != null && c.parent != null && c.right == prev ^ r2l) {
						prev = c;
						c = c.parent;
					}
					// And prev is now the previous first-side node, unless we happen to have none remaining!
				}
			}
		}

		/// <summary>Retrieve the previous tag; walks line boundaries</summary>
		internal LineTag PreviousTag(LineTag tag) {
			Line l; 

			if (tag.Previous != null) {
				return tag.Previous;
			}

			// Next line 
			if (tag.Line.line_no == 1) {
				return null;
			}

			l = GetLine(tag.Line.line_no - 1);
			if (l != null) {
				LineTag t;

				t = l.tags;
				while (t.Next != null) {
					t = t.Next;
				}
				return t;
			}

			return null;
		}

		/// <summary>Retrieve the next tag; walks line boundaries</summary>
		internal LineTag NextTag(LineTag tag) {
			Line l;

			if (tag.Next != null) {
				return tag.Next;
			}

			// Next line
			l = GetLine(tag.Line.line_no + 1);
			if (l != null) {
				return l.tags;
			}

			return null;
		}

		internal Line ParagraphStart(Line line) {
			Line lastline = line;
			do {
				if (line.line_no <= 1)
					break;

				line = lastline;
				lastline = GetLine (line.line_no - 1);
			} while (lastline.ending == LineEnding.Wrap);

			return line;
		}       

		internal Line ParagraphEnd(Line line) {
			Line    l;
   
			while (line.ending == LineEnding.Wrap) {
				l = GetLine(line.line_no + 1);
				if ((l == null) || (l.ending != LineEnding.Wrap)) {
					break;
				}
				line = l;
			}
			return line;
		}

		/// <summary>Give it a pixel offset coordinate and it returns the Line covering that are (offset
		/// is either X or Y depending on if we are multiline
		/// </summary>
		internal Line GetLineByPixel (int offset, bool exact)
		{
			Line	line = document;
			Line	last = null;

			if (multiline) {
				while (line != sentinel) {
					last = line;
					if ((offset >= line.Y) && (offset < (line.Y+line.height))) {
						return line;
					} else if (offset < line.Y) {
						line = line.left;
					} else {
						line = line.right;
					}
				}
			} else {
				while (line != sentinel) {
					last = line;
					if ((offset >= line.X) && (offset < (line.X + line.Width)))
						return line;
					else if (offset < line.X)
						line = line.left;
					else
						line = line.right;
				}
			}

			if (exact) {
				return null;
			}
			return last;
		}

		// UIA: Method used via reflection in TextProviderBehavior

		// Give it x/y pixel coordinates and it returns the Tag at that position
		internal LineTag FindCursor (int x, int y, out int index)
		{
			Line line;

			x -= offset_x;
			y -= offset_y;

			line = GetLineByPixel (multiline ? y : x, false);

			LineTag tag = line.GetTag (x);
				
			if (tag.Length == 0 && tag.Start == 1)
				index = 0;
			else
				index = tag.GetCharIndex (x - line.align_shift);
			
			return tag;
		}

		public void FormatText (Line start_line, int start_pos, Line end_line, int end_pos, LineTag copyFrom) {
			FormatText(start_line, start_pos, end_line, end_pos, copyFrom.Font, copyFrom.Color,
				copyFrom.BackColor, copyFrom.TextPosition, copyFrom.CharOffset, copyFrom.Visible, FormatSpecified.All);
		}

		internal void FormatText (Line start_line, int start_pos, Line end_line, int end_pos, Font font,
		                          Color color, Color back_color, FormatSpecified specified)
		{
			FormatText (start_line, start_pos, end_line, end_pos, font, color, back_color,
				TextPositioning.Normal, 0, true, specified);
		}

		/// <summary>Format area of document in specified font and color</summary>
		/// <param name="start_pos">1-based start position on start_line</param>
		/// <param name="end_pos">1-based end position on end_line </param>
		internal void FormatText (Line start_line, int start_pos, Line end_line, int end_pos, Font font,
		                          Color color, Color back_color, TextPositioning text_position, float char_offset,
		                          bool visible, FormatSpecified specified)
		{
			Line    l;

			// First, format the first line
			if (start_line != end_line) {
				// First line
				LineTag.FormatText(start_line, start_pos, start_line.text.Length - start_pos + 1, font, color,
					back_color, text_position, char_offset, visible, specified);

				// Format last line
				LineTag.FormatText(end_line, 1, end_pos - 1, font, color, back_color, text_position, char_offset, visible, specified);

				// Now all the lines inbetween
				for (int i = start_line.line_no + 1; i < end_line.line_no; i++) {
					l = GetLine(i);
					LineTag.FormatText(l, 1, l.text.Length, font, color, back_color, text_position, char_offset, visible, specified);
				}
			} else {
				// Special case, single line
				LineTag.FormatText(start_line, start_pos, end_pos - start_pos, font, color, back_color,
					text_position, char_offset, visible, specified);
				
				if ((end_pos - start_pos) == 0 && CaretTag.Length != 0)
					CaretTag = CaretTag.Next;
			}
		}

		internal void RecalculateAlignments ()
		{
			Line	line;
			int	line_no;

			line_no = 1;



			while (line_no <= lines) {
				line = GetLine(line_no);

				if (line != null) {
					line.CalculateAlignment();
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
			int	offset;
			int	new_width;
			bool	changed;
			int	shift;
			bool width_changed;
			bool height_changed;

			if (recalc_suspended > 0) {
				recalc_pending = true;
				recalc_start = Math.Min (recalc_start, start);
				recalc_end = Math.Max (recalc_end, end);
				recalc_optimize = optimize;
				return false;
			}

			// Fixup the positions, they can go kinda nuts
			// (this is suspend and resume recalc - they set them to 1 and max)
			start = Math.Max (start, 1);
			end = Math.Min (end, lines);

			offset = GetLine(start).offset;
			line_no = start;
			new_width = 0;
			shift = this.lines;
			width_changed = false;
			height_changed = false;
			if (!optimize) {
				changed = true;		// We always return true if we run non-optimized
			} else {
				changed = false;
			}

			while (line_no <= (end + this.lines - shift)) {
				line = GetLine(line_no++);
				line.offset = offset;

				// if we are not calculating a password
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
					new_width = (int)Math.Ceiling(line.widths[line.text.Length]);
				}

				line.CalculateAlignment ();

				if (multiline)
					offset += line.height;
				else
					offset += (int) line.widths [line.text.Length];

				if (line_no > lines) {
					break;
				}
			}

			if (document_x != new_width) {
				document_x = new_width;
				width_changed = true;
			}

			line = GetLine(lines);

			if (document_y != line.Y + line.height) {
				document_y = line.Y + line.height;
				height_changed = true;
			}

			if (height_changed || width_changed) {
				SizeChanged?.Invoke (this, new SizeChangedEventArgs(height_changed));
			}

			RecalculateAlignments ();

			// scan for links and tell us if its all
			// changed, so we can update everything
			if (EnableLinks)
				ScanForLinks (start, end, ref changed);

			UpdateCaret();
			return changed;
		}

		internal int Size() {
			return lines;
		}

		private void owner_HandleCreated(object sender, EventArgs e) {
			using (var graphics = owner.CreateGraphics()) {
				dpi = (graphics.DpiX + graphics.DpiY) / 2;
				RecalculateDocument(graphics);
			}
			AlignCaret();
		}

		private void owner_VisibleChanged(object sender, EventArgs e) {
			if (owner.Visible && owner.IsHandleCreated) {
				using (var graphics = owner.CreateGraphics())
					RecalculateDocument(graphics);
			}
		}

		internal static bool IsWordSeparator (char ch)
		{
			switch (ch) {
			case ' ':
			case '\t':
			case '(':
			case ')':
			case '\r':
			case '\n':
				return true;
			default:
				return false;
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
						if (prev_line.ending == LineEnding.Wrap) {
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
					if (line.ending != LineEnding.Wrap || line.line_no == lines - 1) {
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
				while (mark.tag.Next != null) {
					mark.tag = mark.tag.Next;
				}
				mark.pos = mark.line.text.Length;
			}
		}
		#endregion	// Internal Methods

		#region Events
		internal class SizeChangedEventArgs : EventArgs {
			public bool HeightChanged { get; }

			public SizeChangedEventArgs (bool HeightChanged)
			{
				this.HeightChanged = HeightChanged;
			}
		}

		internal event EventHandler CaretMoved;
		internal event EventHandler<SizeChangedEventArgs> SizeChanged;
		internal event EventHandler LengthChanged;
		internal event EventHandler UIASelectionChanged;
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

	internal class PictureTag : LineTag {

		internal RTF.Picture picture;

		internal PictureTag (Line line, int start, RTF.Picture picture) : base (line, start)
		{
			this.picture = picture;
		}

		public override bool IsTextTag {
			get { return false; }
		}

		public override SizeF SizeOfPosition (Graphics dc, int pos)
		{
			if (Visible)
				return picture.Size;
			else
				return SizeF.Empty;
		}

		internal override int MaxHeight ()
		{
			return (int) (picture.Height + 0.5F);
		}

		public override void Draw (Graphics dc, Color color, float xoff, float y, int start, int end)
		{
			if (Visible)
				picture.DrawImage (dc, (xoff + Line.widths [start]), y, false);
		}

		public override void Draw (Graphics dc, Color color, float xoff, float y, int start, int end, string text)
		{
			Draw (dc, color, xoff, y, start, end);
		}

		public override void Draw (Graphics dc, Color color, float xoff, float y, int start, int end,
		                           string text, out Rectangle measuredText, bool measureText)
		{
			Draw (dc, color, xoff, y, start, end);
			if (measureText && Visible) {
				measuredText = new Rectangle (Point.Round (new PointF (xoff + Line.widths [start], y)), Size.Round (picture.Size));
			} else {
				measuredText = new Rectangle ();
			}
		}

		public override string Text ()
		{
			return "I";
		}
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

		//private int		caret_line;
		//private int		caret_pos;

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

		internal bool Undo ()
		{
			Action action;
			bool user_action_finished = false;

			if (undo_actions.Count == 0)
				return false;

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

			return true;
		}

		internal bool Redo ()
		{
			Action action;
			bool user_action_finished = false;

			if (redo_actions.Count == 0)
				return false;

			locked = true;
			do {
				Line start;
				int start_index;

				action = (Action) redo_actions.Pop ();
				undo_actions.Push (action);

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

			return true;
		}
		#endregion	// Internal Methods

		#region Private Methods

		public void BeginUserAction (string name)
		{
			if (locked)
				return;

			// Nuke the redo queue
			redo_actions.Clear ();

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

			// Nuke the redo queue
			redo_actions.Clear ();

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

			// Nuke the redo queue
			redo_actions.Clear ();

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

			// Nuke the redo queue
			redo_actions.Clear ();

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
		public Line Duplicate(Line start_line, int start_pos, Line end_line, int end_pos)
		{
			Line	ret;
			Line	line;
			Line	current;
			LineTag	tag;
			LineTag	current_tag;
			int	start;
			int	end;
			int	tag_start;

			line = new Line (start_line.document, start_line.ending);
			ret = line;

			for (int i = start_line.line_no; i <= end_line.line_no; i++) {
				current = document.GetLine(i);

				if (start_line.line_no == i) {
					start = start_pos;
				} else {
					start = 0;
				}

				if (end_line.line_no == i) {
					end = end_pos;
				} else {
					end = current.text.Length;
				}

				if (end_pos == 0)
					continue;

				// Text for the tag
				line.text = new StringBuilder (current.text.ToString (start, end - start));

				// Copy tags from start to start+length onto new line
				current_tag = current.FindTag (start + 1);
				while ((current_tag != null) && (current_tag.Start <= end)) {
					if ((current_tag.Start <= start) && (start < (current_tag.Start + current_tag.Length))) {
						// start tag is within this tag
						tag_start = start;
					} else {
						tag_start = current_tag.Start;
					}

					tag = new LineTag(line, tag_start - start + 1);
					tag.CopyFormattingFrom (current_tag);

					current_tag = current_tag.Next;

					// Add the new tag to the line
					if (line.tags == null) {
						line.tags = tag;
					} else {
						LineTag tail;
						tail = line.tags;

						while (tail.Next != null) {
							tail = tail.Next;
						}
						tail.Next = tag;
						tag.Previous = tail;
					}
				}

				if ((i + 1) <= end_line.line_no) {
					line.ending = current.ending;

					// Chain them (we use right/left as next/previous)
					line.right = new Line (start_line.document, start_line.ending);
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

				while (tag.Next != null) {
					tag = tag.Next;
				}

				offset = tag.Start + tag.Length - 1;

				tag.Next = insert.tags;
				line.text.Insert(offset, insert.text.ToString());

				// Adjust start locations
				tag = tag.Next;
				while (tag != null) {
					tag.Start += offset;
					tag.Line = line;
					tag = tag.Next;
				}
				// Put it back together
				document.Combine(line.line_no, line.line_no + 1);

				if (select) {
					document.SetSelectionStart (line, pos, false);
					document.SetSelectionEnd (line, pos + insert.text.Length, false);
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
					document.Split(line.line_no, pos);
					//Insert our tags at the end of the line
					tag = line.tags;

					
					if (tag != null && tag.Length != 0) {
						while (tag.Next != null) {
							tag = tag.Next;
						}
						offset = tag.Start + tag.Length - 1;
						tag.Next = current.tags;
						tag.Next.Previous = tag;

						tag = tag.Next;

					} else {
						offset = 0;
						line.tags = current.tags;
						line.tags.Previous = null;
						tag = line.tags;
					}

					line.ending = current.ending;
				} else {
					document.Split(line.line_no, 0);
					offset = 0;
					line.tags = current.tags;
					line.tags.Previous = null;
					line.ending = current.ending;
					tag = line.tags;
				}

				// Adjust start locations and line pointers
				while (tag != null) {
					tag.Start += offset - 1;
					tag.Line = line;
					tag = tag.Next;
				}

				line.text.Insert(offset, current.text.ToString());
				line.Grow(line.text.Length);

				line.recalc = true;
				line = document.GetLine(line.line_no + 1);

				// FIXME? Test undo of line-boundaries
				if ((current.right == null) && (current.tags.Length != 0)) {
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
