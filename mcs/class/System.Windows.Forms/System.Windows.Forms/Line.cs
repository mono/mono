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
		static int			DEFAULT_TEXT_LEN = 0;	// 
		internal bool			recalc;			// Line changed

		private static Hashtable kerning_fonts = new Hashtable ();		// record which fonts use kerning
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

		internal Line (Document document, int LineNo, string Text, Font font, Color color, LineEnding ending) : this (document, ending)
		{
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder (Text, space);
			line_no = LineNo;
			this.ending = ending;

			widths = new float[space + 1];

			
			tags = new LineTag(this, 1);
			tags.Font = font;
			tags.Color = color;
		}

		internal Line (Document document, int LineNo, string Text, HorizontalAlignment align, Font font, Color color, LineEnding ending) : this(document, ending)
		{
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder (Text, space);
			line_no = LineNo;
			this.ending = ending;
			alignment = align;

			widths = new float[space + 1];

			
			tags = new LineTag(this, 1);
			tags.Font = font;
			tags.Color = color;
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

		// UIA: Method used via reflection in TextRangeProvider
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
			
		// UIA: Method used via reflection in TextRangeProvider
		internal int Width {
			get {
				int res = (int) widths [text.Length];
				return res;
			}
		}

		internal string Text {
			get { return text.ToString(); }
			set {
				int prev_length = text.Length;
				text = new StringBuilder(value, value.Length > DEFAULT_TEXT_LEN ? value.Length + 1 : DEFAULT_TEXT_LEN);

				if (text.Length > prev_length)
					Grow (text.Length - prev_length);
			}
		}
		
		// UIA: Method used via reflection in TextRangeProvider
		internal int X {
			get {
				if (document.multiline)
					return align_shift;
				return offset + align_shift;
			}
		}

		// UIA: Method used via reflection in TextRangeProvider
		internal int Y {
			get {
				if (!document.multiline)
					return document.top_margin;
				return document.top_margin + offset;
			}
		}
		#endregion	// Internal Properties

		#region Internal Methods

		/// <summary>
		///  Builds a simple code to record which tags are links and how many tags
		///  used to compare lines before and after to see if the scan for links
		///  process has changed anything.
		/// </summary>
		internal void LinkRecord (StringBuilder linkRecord)
		{
			LineTag tag = tags;

			while (tag != null) {
				if (tag.IsLink)
					linkRecord.Append ("L");
				else
					linkRecord.Append ("N");

				tag = tag.Next;
			}
		}

		/// <summary>
		///  Clears all link properties from tags
		/// </summary>
		internal void ClearLinks ()
		{
			LineTag tag = tags;

			while (tag != null) {
				tag.IsLink = false;
				tag = tag.Next;
			}
		}

		public void DeleteCharacters(int pos, int count)
		{
			LineTag tag;
			bool streamline = false;
			
			// Can't delete more than the line has
			if (pos >= text.Length)
				return;

			// Find the first tag that we are deleting from
			tag = FindTag (pos + 1);

			// Remove the characters from the line
			text.Remove (pos, count);

			if (tag == null)
				return;

			// Check if we're crossing tag boundaries
			if ((pos + count) > (tag.Start + tag.Length - 1)) {
				int left;

				// We have to delete cross tag boundaries
				streamline = true;
				left = count;

				left -= tag.Start + tag.Length - pos - 1;
				tag = tag.Next;
				
				// Update the start of each tag
				while ((tag != null) && (left > 0)) {
					// Cache tag.Length as is will be indireclty modified
					// by changes to tag.Start
					int tag_length = tag.Length;
					tag.Start -= count - left;

					if (tag_length > left) {
						left = 0;
					} else {
						left -= tag_length;
						tag = tag.Next;
					}

				}
			} else {
				// We got off easy, same tag

				if (tag.Length == 0)
					streamline = true;
			}

			// Delete empty orphaned tags at the end
			LineTag walk = tag;
			while (walk != null && walk.Next != null && walk.Next.Length == 0) {
				LineTag t = walk;
				walk.Next = walk.Next.Next;
				if (walk.Next != null)
					walk.Next.Previous = t;
				walk = walk.Next;
			}

			// Adjust the start point of any tags following
			if (tag != null) {
				tag = tag.Next;
				while (tag != null) {
					tag.Start -= count;
					tag = tag.Next;
				}
			}

			recalc = true;

			if (streamline)
				Streamline (document.Lines);
		}
		
		// This doesn't do exactly what you would think, it just pulls off the \n part of the ending
		internal void DrawEnding (Graphics dc, float y)
		{
			if (document.multiline)
				return;
			LineTag last = tags;
			while (last.Next != null)
				last = last.Next;

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

			TextBoxTextRenderer.DrawText (dc, end_str, last.Font, last.Color, X + widths [TextLengthWithoutEnding ()] - document.viewport_x + document.OffsetX, y, true);
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
				if (((tag.Start - 1) <= pos) && (pos <= (tag.Start + tag.Length - 1)))
					return LineTag.GetFinalTag (tag);

				tag = tag.Next;
			}
			
			return null;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		// Get the tag that contains this x coordinate
		public LineTag GetTag (int x)
		{
			LineTag tag = tags;
			
			// Coord is to the left of the first character
			if (x < tag.X)
				return LineTag.GetFinalTag (tag);
			
			// All we have is a linked-list of tags, so we have
			// to do a linear search.  But there shouldn't be
			// too many tags per line in general.
			while (true) {
				if (x >= tag.X && x < (tag.X + tag.Width))
					return tag;
					
				if (tag.Next != null)
					tag = tag.Next;
				else
					return LineTag.GetFinalTag (tag);			
			}
		}
					
		// Make sure we always have enoughs space in text and widths
		internal void Grow (int minimum)
		{
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
		public void InsertString (int pos, string s)
		{
			InsertString (pos, s, FindTag (pos));
		}

		// Inserts a string at the given position
		public void InsertString (int pos, string s, LineTag tag)
		{
			int len = s.Length;

			// Insert the text into the StringBuilder
			text.Insert (pos, s);

			// Update the start position of every tag after this one
			tag = tag.Next;

			while (tag != null) {
				tag.Start += len;
				tag = tag.Next;
			}

			// Make sure we have room in the widths array
			Grow (len);

			// This line needs to be recalculated
			recalc = true;
		}

		/// <summary>
		/// Go through all tags on a line and recalculate all size-related values;
		/// returns true if lineheight changed
		/// </summary>
		internal bool RecalculateLine (Graphics g, Document doc)
		{
			return RecalculateLine (g, doc, kerning_fonts.ContainsKey (tags.Font.GetHashCode ()));
		}

		private bool RecalculateLine (Graphics g, Document doc, bool handleKerning)
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
			int prev_height;
			int prev_ascent;
			float add_width;

			pos = 0;
			len = this.text.Length;
			tag = this.tags;
			prev_offset = this.offset;	// For drawing optimization calculations
			prev_height = this.height;
			prev_ascent = this.ascent;
			this.height = 0;		// Reset line height
			this.ascent = 0;		// Reset the ascent for the line
			tag.Shift = 0;			// Reset shift (which should be stored as pixels, not as points)

			if (ending == LineEnding.Wrap)
				widths[0] = document.left_margin + hanging_indent;
			else
				widths[0] = document.left_margin + indent;

			this.recalc = false;
			retval = false;
			wrapped = false;

			wrap_pos = 0;
			add_width = 0;

			while (pos < len) {

				while (tag.Length == 0) {	// We should always have tags after a tag.length==0 unless len==0
					//tag.Ascent = 0;
					tag.Shift = (tag.Line.ascent - tag.Ascent) / 72;
					tag = tag.Next;
				}

				// kerning is a problem.  The original code in this method assumed that the
				// width of a string equals the sum of the widths of its characters.  This is
				// not true when kerning takes place during the display process.  Since it's
				// impossible to find out easily whether a font does kerning, and with which
				// characters, we just detect that kerning must have happened and use a slower
				// (but accurate) measurement for those fonts henceforth.  Without handling
				// kerning, many fonts for English become unreadable during typing for many
				// input strings, and text in many other languages is even worse trying to
				// type in TextBoxes.
				// See https://bugzilla.xamarin.com/show_bug.cgi?id=26478 for details.
				float newWidth;
				if (handleKerning && !Char.IsWhiteSpace(text[pos]))
				{
					// MeasureText doesn't measure trailing spaces, so we do the best we can for those
					// in the else branch.
					// It doesn't measure /t characters either, we need to add it manually with add_width.
					size = TextBoxTextRenderer.MeasureText (g, text.ToString (0, pos + 1), tag.Font);
					newWidth = widths[0] + size.Width + add_width;
				}
				else
				{
					size = tag.SizeOfPosition (g, pos);
					w = size.Width;
					newWidth = widths[pos] + w;
					if (text[pos] == '\t') add_width += w;
				}

				if (Char.IsWhiteSpace (text[pos]))
					wrap_pos = pos + 1;

				if (doc.wrap) {
					if ((wrap_pos > 0) && (wrap_pos != len) && (newWidth + 5) > (doc.viewport_width - this.right_indent)) {
						// Make sure to set the last width of the line before wrapping
						widths[pos + 1] = newWidth;

						pos = wrap_pos;
						len = text.Length;
						doc.Split (this, tag, pos);
						ending = LineEnding.Wrap;
						len = this.text.Length;

						retval = true;
						wrapped = true;
					} else if (pos > 1 && newWidth > (doc.viewport_width - this.right_indent)) {
						// No suitable wrap position was found so break right in the middle of a word

						// Make sure to set the last width of the line before wrapping
						widths[pos + 1] = newWidth;

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

					widths[pos] = newWidth;

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

				if (pos == (tag.Start - 1 + tag.Length)) {
					// We just found the end of our current tag
					tag.Height = tag.MaxHeight ();

					// Check if we're the tallest on the line (so far)
					if (tag.Height > this.height)
						this.height = tag.Height;	// Yep; make sure the line knows

					if (tag.Ascent > this.ascent) {
						LineTag t;

						// We have a tag that has a taller ascent than the line;
						t = tags;
						while (t != null && t != tag) {
							t.Shift = (tag.Ascent - t.Ascent) / 72;
							t = t.Next;
						}

						// Save on our line
						this.ascent = tag.Ascent;
					} else {
						tag.Shift = (this.ascent - tag.Ascent) / 72;
					}

					tag = tag.Next;
					if (tag != null) {
						tag.Shift = 0;
						wrap_pos = pos;
					}
				}
			}

			var fullText = text.ToString();
			if (!handleKerning && fullText.Length > 1 && !wrapped)
			{
				// Check whether kerning takes place for this string and font.
				var realSize = TextBoxTextRenderer.MeasureText(g, fullText, tags.Font);
				float realWidth = realSize.Width + widths[0];
				// MeasureText ignores trailing whitespace, so we will too at this point.
				int length = fullText.TrimEnd().Length;
				float sumWidth = widths[length];
				if (realWidth != sumWidth)
				{
					kerning_fonts.Add(tags.Font.GetHashCode (), true);
					// Using a slightly incorrect width this time around isn't that bad. All that happens
					// is that the cursor is a pixel or two off until the next character is typed.  It's
					// the accumulation of pixel after pixel that causes display problems.
				}
			}

			while (tag != null) {	
				tag.Shift = (tag.Line.ascent - tag.Ascent) / 72;
				tag = tag.Next;
			}

			if (this.height == 0) {
				this.height = tags.Font.Height;
				tags.Height = this.height;
				tags.Shift = 0;
			}

			if (prev_offset != offset || prev_height != this.height || prev_ascent != this.ascent)
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

			pos = 0;
			len = this.text.Length;
			tag = this.tags;
			ascent = 0;
			tag.Shift = 0;

			this.recalc = false;
			widths[0] = document.left_margin + indent;

			w = TextBoxTextRenderer.MeasureText (g, doc.password_char, tags.Font).Width;

			if (this.height != (int)tag.Font.Height)
				ret = true;
			else
				ret = false;

			this.height = (int)tag.Font.Height;
			tag.Height = this.height;

			this.ascent = tag.Ascent;

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
			next = current.Next;

			//
			// Catch what the loop below wont; eliminate 0 length 
			// tags, but only if there are other tags after us
			// We only eliminate text tags if there is another text tag
			// after it.  Otherwise we wind up trying to type on picture tags
			//
			while ((current.Length == 0) && (next != null) && (next.IsTextTag)) {
				tags = next;
				tags.Previous = null;
				current = next;
				next = current.Next;
			}


			if (next == null)
				return;

			while (next != null) {
				// Take out 0 length tags unless it's the last tag in the document
				if (current.IsTextTag && next.Length == 0 && next.IsTextTag) {
					if ((next.Next != null) || (line_no != lines)) {
						current.Next = next.Next;
						if (current.Next != null) {
							current.Next.Previous = current;
						}
						next = current.Next;
						continue;
					}
				}
				
				if (current.Combine (next)) {
					next = current.Next;
					continue;
				}

				current = current.Next;
				next = current.Next;
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
