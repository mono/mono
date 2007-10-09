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

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Text;
using System.Text;

namespace System.Windows.Forms
{
	internal class Line : ICloneable, IComparable
	{
		#region	Local Variables

		internal Document document;
		// Stuff that matters for our line
		internal StringBuilder		text;			// Characters for the line
		internal float[]		widths;			// Width of each character; always one larger than text.Length
		internal int			space;			// Number of elements in text and widths
		internal int			line_no;		// Line number
		internal LineTag		tags;			// Tags describing the text
		internal int			offset;			// Baseline can be on the X or Y axis depending if we are in multiline mode or not
		internal int			height;			// Height of the line (height of tallest tag)
		internal int			ascent;			// Ascent of the line (ascent of the tallest tag)
		internal HorizontalAlignment	alignment;		// Alignment of the line
		internal int			align_shift;		// Pixel shift caused by the alignment
		internal int			indent;			// Left indent for the first line
		internal int			hanging_indent;		// Hanging indent (left indent for all but the first line)
		internal int			right_indent;		// Right indent for all lines
		internal LineEnding		ending;

		// Stuff that's important for the tree
		internal Line			parent;			// Our parent line
		internal Line			left;			// Line with smaller line number
		internal Line			right;			// Line with higher line number
		internal LineColor		color;			// We're doing a black/red tree. this is the node color
		internal int			DEFAULT_TEXT_LEN;	// 
		internal bool			recalc;			// Line changed
		#endregion	// Local Variables

		#region Constructors
		internal Line (Document document, LineEnding ending)
		{
			this.document = document; 
			color = LineColor.Red;
			left = null;
			right = null;
			parent = null;
			text = null;
			recalc = true;
			alignment = document.alignment;

			this.ending = ending;
		}

		internal Line (Document document, int LineNo, string Text, Font font, SolidBrush color, LineEnding ending) : this (document, ending)
		{
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder (Text, space);
			line_no = LineNo;
			this.ending = ending;

			widths = new float[space + 1];

			
			tags = new LineTag(this, 1);
			tags.font = font;
			tags.color = color;				
		}

		internal Line (Document document, int LineNo, string Text, HorizontalAlignment align, Font font, SolidBrush color, LineEnding ending) : this(document, ending)
		{
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder (Text, space);
			line_no = LineNo;
			this.ending = ending;
			alignment = align;

			widths = new float[space + 1];

			
			tags = new LineTag(this, 1);
			tags.font = font;
			tags.color = color;
		}

		internal Line (Document document, int LineNo, string Text, LineTag tag, LineEnding ending) : this(document, ending)
		{
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder (Text, space);
			this.ending = ending;
			line_no = LineNo;

			widths = new float[space + 1];
			tags = tag;
		}

		#endregion	// Constructors

		#region Internal Properties
		internal HorizontalAlignment Alignment {
			get { return alignment; }
			set {
				if (alignment != value) {
					alignment = value;
					recalc = true;
				}
			}
		}

		internal int HangingIndent {
			get { return hanging_indent; }
			set {
				hanging_indent = value;
				recalc = true;
			}
		}

		internal int Height {
			get { return height; }
			set { height = value; }
		}

		internal int Indent {
			get { return indent; }
			set { 
				indent = value;
				recalc = true;
			}
		}

		internal int LineNo {
			get { return line_no; }
			set { line_no = value; }
		}

		internal int RightIndent {
			get { return right_indent; }
			set { 
				right_indent = value;
				recalc = true;
			}
		}
			
		internal int Width {
			get {
				int res = (int) widths [text.Length];
				return res;
			}
		}

		internal string Text {
			get { return text.ToString(); }
			set { 
				text = new StringBuilder(value, value.Length > DEFAULT_TEXT_LEN ? value.Length : DEFAULT_TEXT_LEN);
			}
		}
		
		internal int X {
			get {
				if (document.multiline)
					return align_shift;
				return offset + align_shift;
			}
		}

		internal int Y {
			get {
				if (!document.multiline)
					return document.top_margin;
				return document.top_margin + offset;
			}
		}
		#endregion	// Internal Properties

		#region Internal Methods
		// This doesn't do exactly what you would think, it just pulls off the \n part of the ending
		internal void DrawEnding (Graphics dc, float y)
		{
			if (document.multiline)
				return;
			LineTag last = tags;
			while (last.next != null)
				last = last.next;

			string end_str = null;
			switch (document.LineEndingLength (ending)) {
			case 0:
				return;
			case 1:
				end_str = "\u0013";
				break;
			case 2:
				end_str = "\u0013\u0013";
				break;
			case 3:
				end_str = "\u0013\u0013\u0013";
				break;
			}

			TextBoxTextRenderer.DrawText (dc, end_str, last.font, last.color, X + widths [TextLengthWithoutEnding ()] - document.viewport_x, y, true);
		}

		/// <summary> Find the tag on a line based on the character position, pos is 0-based</summary>
		internal LineTag FindTag (int pos)
		{
			LineTag tag;

			if (pos == 0)
				return tags;

			tag = this.tags;

			if (pos >= text.Length)
				pos = text.Length - 1;

			while (tag != null) {
				if (((tag.start - 1) <= pos) && (pos < (tag.start + tag.Length - 1)))
					return LineTag.GetFinalTag (tag);

				tag = tag.next;
			}
			
			return null;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		// Make sure we always have enoughs space in text and widths
		internal void Grow (int minimum) {
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
				widths.CopyTo (new_widths, 0);

				widths = new_widths;
			}
		}

		/// <summary>
		/// Go through all tags on a line and recalculate all size-related values;
		/// returns true if lineheight changed
		/// </summary>
		internal bool RecalculateLine (Graphics g, Document doc)
		{
			LineTag tag;
			int pos;
			int len;
			SizeF size;
			float w;
			int prev_offset;
			bool retval;
			bool wrapped;
			Line line;
			int wrap_pos;

			pos = 0;
			len = this.text.Length;
			tag = this.tags;
			prev_offset = this.offset;	// For drawing optimization calculations
			this.height = 0;		// Reset line height
			this.ascent = 0;		// Reset the ascent for the line
			tag.shift = 0;

			if (ending == LineEnding.Wrap)
				widths[0] = document.left_margin + hanging_indent;
			else
				widths[0] = document.left_margin + indent;

			this.recalc = false;
			retval = false;
			wrapped = false;

			wrap_pos = 0;

			while (pos < len) {
				while (tag.Length == 0) {	// We should always have tags after a tag.length==0 unless len==0
					tag.ascent = 0;
					tag.shift = 0;
					tag = tag.next;
				}

				size = tag.SizeOfPosition (g, pos);
				w = size.Width;

				if (Char.IsWhiteSpace (text[pos]))
					wrap_pos = pos + 1;

				if (doc.wrap) {
					if ((wrap_pos > 0) && (wrap_pos != len) && (widths[pos] + w) + 5 > (doc.viewport_width - this.right_indent)) {
						// Make sure to set the last width of the line before wrapping
						widths[pos + 1] = widths[pos] + w;

						pos = wrap_pos;
						len = text.Length;
						doc.Split (this, tag, pos);
						ending = LineEnding.Wrap;
						len = this.text.Length;

						retval = true;
						wrapped = true;
					} else if (pos > 1 && (widths[pos] + w) > (doc.viewport_width - this.right_indent)) {
						// No suitable wrap position was found so break right in the middle of a word

						// Make sure to set the last width of the line before wrapping
						widths[pos + 1] = widths[pos] + w;

						doc.Split (this, tag, pos);
						ending = LineEnding.Wrap;
						len = this.text.Length;
						retval = true;
						wrapped = true;
					}
				}

				// Contract all wrapped lines that follow back into our line
				if (!wrapped) {
					pos++;

					widths[pos] = widths[pos - 1] + w;

					if (pos == len) {
						line = doc.GetLine (this.line_no + 1);
						if ((line != null) && (ending == LineEnding.Wrap || ending == LineEnding.None)) {
							// Pull the two lines together
							doc.Combine (this.line_no, this.line_no + 1);
							len = this.text.Length;
							retval = true;
						}
					}
				}

				if (pos == (tag.start - 1 + tag.Length)) {
					// We just found the end of our current tag
					tag.height = tag.MaxHeight ();

					// Check if we're the tallest on the line (so far)
					if (tag.height > this.height)
						this.height = tag.height;	// Yep; make sure the line knows

					if (tag.ascent == 0) {
						int descent;

						XplatUI.GetFontMetrics (g, tag.font, out tag.ascent, out descent);
					}

					if (tag.ascent > this.ascent) {
						LineTag t;

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

			if (prev_offset != offset)
				retval = true;

			return retval;
		}

		/// <summary>
		/// Recalculate a single line using the same char for every character in the line
		/// </summary>
		internal bool RecalculatePasswordLine (Graphics g, Document doc)
		{
			LineTag tag;
			int pos;
			int len;
			float w;
			bool ret;
			int descent;

			pos = 0;
			len = this.text.Length;
			tag = this.tags;
			ascent = 0;
			tag.shift = 0;

			this.recalc = false;
			widths[0] = document.left_margin + indent;

			w = TextBoxTextRenderer.MeasureText (g, doc.password_char, tags.font).Width;

			if (this.height != (int)tag.font.Height)
				ret = true;
			else
				ret = false;

			this.height = (int)tag.font.Height;
			tag.height = this.height;

			XplatUI.GetFontMetrics (g, tag.font, out tag.ascent, out descent);
			this.ascent = tag.ascent;

			while (pos < len) {
				pos++;
				widths[pos] = widths[pos - 1] + w;
			}

			return ret;
		}
		
		internal void Streamline (int lines)
		{
			LineTag current;
			LineTag next;

			current = this.tags;
			next = current.next;

			//
			// Catch what the loop below wont; eliminate 0 length 
			// tags, but only if there are other tags after us
			// We only eliminate text tags if there is another text tag
			// after it.  Otherwise we wind up trying to type on picture tags
			//
			while ((current.Length == 0) && (next != null) && (next.IsTextTag)) {
				tags = next;
				tags.previous = null;
				current = next;
				next = current.next;
			}


			if (next == null)
				return;

			while (next != null) {
				// Take out 0 length tags unless it's the last tag in the document
				if (current.IsTextTag && next.Length == 0 && next.IsTextTag) {
					if ((next.next != null) || (line_no != lines)) {
						current.next = next.next;
						if (current.next != null) {
							current.next.previous = current;
						}
						next = current.next;
						continue;
					}
				}
				
				if (current.Combine (next)) {
					next = current.next;
					continue;
				}

				current = current.next;
				next = current.next;
			}
		}

		internal int TextLengthWithoutEnding ()
		{
			return text.Length - document.LineEndingLength (ending);
		}

		internal string TextWithoutEnding ()
		{
			return text.ToString (0, text.Length - document.LineEndingLength (ending));
		}
		#endregion	// Internal Methods

		#region Administrative
		public object Clone ()
		{
			Line	clone;

			clone = new Line (document, ending);

			clone.text = text;

			if (left != null)
				clone.left = (Line)left.Clone();

			if (left != null)
				clone.left = (Line)left.Clone();

			return clone;
		}

		internal object CloneLine ()
		{
			Line	clone;

			clone = new Line (document, ending);

			clone.text = text;

			return clone;
		}

		public int CompareTo (object obj)
		{
			if (obj == null)
				return 1;

			if (! (obj is Line))
				throw new ArgumentException("Object is not of type Line", "obj");

			if (line_no < ((Line)obj).line_no)
				return -1;
			else if (line_no > ((Line)obj).line_no)
				return 1;
			else
				return 0;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;

			if (!(obj is Line))
				return false;

			if (obj == this)
				return true;

			if (line_no == ((Line)obj).line_no)
				return true;

			return false;
		}

		public override string ToString()
		{
			return string.Format ("Line {0}", line_no);
		}
		#endregion	// Administrative
	}
}
