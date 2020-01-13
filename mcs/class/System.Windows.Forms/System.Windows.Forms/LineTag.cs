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

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Text;
using System.Text;

namespace System.Windows.Forms
{
	internal class LineTag
	{
		#region	Local Variables
		// Formatting
		private Font		font;		// System.Drawing.Font object for this tag
		private Color		color;		// The font color for this tag
		private Color		back_color;	// In 2.0 tags can have background colours.
		private Font		link_font;	// Cached font used for link if IsLink
		private bool		is_link;	// Whether this tag is a link
		private string		link_text;	// The full link text e.g. this might be 
							// word-wrapped to "w" but this would be
							// "www.example.com"
		private bool		visible;
		private TextPositioning	text_position;	// Normal / superscript / subscript
		private Font		small_font;			// Cached font for superscript / subscript
		private float		char_offset;		// Shift the text baseline up or down

		// Payload; text
		private int		start;		// start, in chars; index into Line.text
							// 1 based!!

		// Drawing support
		private int		height;		// Height in pixels of the text this tag describes
		private int		ascent;		// Ascent of the font for this tag
		private int		descent;	// Descent of the font for this tag
		private int		shift;		// Shift down for this tag, to stay on baseline.
							// Measured from top of line to top of tag.

		// Administrative
		private Line		line;		// The line we're on
		private LineTag		next;		// Next tag on the same line
		private LineTag		previous;	// Previous tag on the same line
		#endregion

		#region Constructors
		public LineTag (Line line, int start)
		{
			this.line = line;
			Start = start;
			link_font = null;
			is_link = false;
			link_text = null;
			visible = true;
		}
		#endregion	// Constructors

		#region Public Properties
		public int Ascent {
			get { return ascent; }
		}
		
		public Color BackColor {
			get { return back_color; }
			set { back_color = value; }
		}

		public Color ColorToDisplay {
			get {
				if (IsLink == true)
					return Color.Blue;

				return color;
			}
		}

		public Color Color {
			get { return color; }
			set { color = value; }
		}
		
		public int Descent {
			get { return descent; }
		}

		public int End {
			get { return start + Length; }
		}

		public Font FontToDisplay {
			get {
				if (IsLink) {
					if (link_font == null)
						link_font = new Font (font.FontFamily, font.Size, font.Style | FontStyle.Underline);

					return link_font;
				}

				if (TextPosition != TextPositioning.Normal) {
					if (small_font == null)
						small_font = new Font (font.FontFamily, font.Size * 0.583F, font.Style);

					if (IsLink)
						return new Font (small_font, font.Style | FontStyle.Underline);
					else
						return small_font;
				}

				return font;
			}
		}

		public Font Font {
			get { return font; }
			set { 
				if (font != value) {
					link_font = null;
					small_font = null;
					font = value;
	
					height = Font.Height;
					XplatUI.GetFontMetrics (Hwnd.GraphicsContext, Font, out ascent, out descent);
					float scale_factor = font.GetHeight () / font.FontFamily.GetLineSpacing (font.Style);
					ascent = (int) Math.Ceiling (ascent * scale_factor);
					descent = (int) Math.Ceiling (descent * scale_factor);
					line.recalc = true;
				}
			}
		}

		public TextPositioning TextPosition {
			get { return text_position; }
			set { text_position = value; }
		}

		public float CharOffset {
			get { return char_offset; }
			set { char_offset = value; }
		}

		public int Height {
			get { return height; }
			set { height = value; }
		}

		public int DrawnHeight {
			get {
				if (text_position != TextPositioning.Normal)
					return (int) (height * 0.583F);

				return height;
			}
		}

		public virtual bool IsTextTag {
			get { return true; }
		}

		public int Length {
			get {
				int res = 0;
				if (next != null)
					res = next.start - start;
				else
					res = line.text.Length - (start - 1);

				return res > 0 ? res : 0;
			}
		}

		public Line Line {
			get { return line; }
			set { line = value; }
		}

		public LineTag Next {
			get { return next; }
			set { next = value; }
		}

		public LineTag Previous {
			get { return previous; }
			set { previous = value; }
		}

		public int Shift {
			get { return shift; }
			set { shift = value; }
		}

		public int Start {
			get { return start; }
			set {
#if DEBUG
				if (value <= 0)
					throw new Exception("Start of tag must be 1 or higher!");

				if (this.Previous != null) {
					if  (this.Previous.Start == value)
						System.Console.Write("Creating empty tag");
					if  (this.Previous.Start > value)
						throw new Exception("New tag makes an insane tag");
				}
#endif
				start = value;
			}
		}

		public int TextEnd {
			get { return start + TextLength; }
		}

		public int TextLength {
			get {
				int res = 0;
				if (next != null)
					res = next.start - start;
				else
					res = line.TextLengthWithoutEnding () - (start - 1);

				return res > 0 ? res : 0;
			}
		}

		public bool Visible {
			get { return visible; }
			set { visible = value; }
		}

		public float Width {
			get {
				if (Length == 0 || !visible)
					return 0;
				return line.widths [start + Length - 1] - (start != 0 ? line.widths [start - 1] : 0);
			}
		}

		public float X {
			get {
				if (start == 0)
					return line.X;
				return line.X + line.widths [start - 1];
			}
		}

		public int OffsetY {
			get {
				if (text_position == TextPositioning.Subscript)
					return (int) (height * 0.45F);

				return 0;
			}
		}

		public bool IsLink {
			get { return is_link; }
			set { is_link = value; }
		}

		public string LinkText {
			get { return link_text; }
			set { link_text = value; }
		}
		#endregion
		
		#region Public Methods
		///<summary>Break a tag into two with identical attributes; pos is 1-based; returns tag starting at &gt;pos&lt; or null if end-of-line</summary>
		public LineTag Break (int pos)
		{
			LineTag	new_tag;

#if DEBUG
			// Sanity
			if (pos < this.Start)
				throw new Exception ("Breaking at a negative point");
#endif

#if DEBUG
			if (pos > End)
				throw new Exception ("Breaking past the end of a line");
#endif

			new_tag = new LineTag(line, pos);
			new_tag.CopyFormattingFrom (this);

			new_tag.Next = this.next;
			this.Next = new_tag;
			new_tag.previous = this;

			if (new_tag.next != null)
				new_tag.next.previous = new_tag;

			return new_tag;
		}

		/// <summary>Combines 'this' tag with 'other' tag</summary>
		public bool Combine (LineTag other)
		{
			if (!this.Equals (other))
				return false;

			this.Next = other.next;
			
			if (this.next != null)
				this.next.previous = this;

			return true;
		}

		public void CopyFormattingFrom (LineTag other)
		{
			Font = other.font;
			color = other.color;
			back_color = other.back_color;
			TextPosition = other.text_position;
			CharOffset = other.CharOffset;
			Visible = other.Visible;
		}

		public void Delete ()
		{
			// If we are the only tag, we can't be deleted
			if (previous == null && next == null)
				return;
				
			// If we are the last tag, deletion is easy
			if (next == null) {
				previous.next = null;
				return;
			}
			
			// Easy cases gone, little tougher, delete ourself
			// Update links, and start
			next.previous = null;
			
			LineTag loop = next;
			
			while (loop != null) {
				loop.Start -= Length;
				loop = loop.next;			
			}
			
			return;
		}
		
		public virtual void Draw (Graphics dc, Color color, float x, float y, int start, int end)
		{
			if (text_position == TextPositioning.Subscript)
				y += OffsetY;
			TextBoxTextRenderer.DrawText (dc, line.text.ToString (start, end).Replace ("\r", string.Empty), FontToDisplay, color, x, y, false);
		}
		
		public virtual void Draw (Graphics dc, Color color, float xoff, float y, int start, int end, string text)
		{
			Rectangle measured_text;
			Draw (dc, color, xoff, y, start, end, text, out measured_text, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="drawStart">0 based start index</param>
		public virtual void Draw (Graphics dc, Color color, float xoff, float y, int drawStart, int drawEnd,
					  string text, out Rectangle measuredText, bool measureText)
		{
			if (!visible) {
				measuredText = new Rectangle ();
				return;
			}

			if (text_position == TextPositioning.Subscript)
				y += OffsetY;

			if (measureText) {
				int xstart = (int)line.widths [drawStart] + (int)xoff;
				int xend = (int)line.widths [drawEnd] - (int)line.widths [drawStart];
				int ystart = (int)y;
				int yend = (int)TextBoxTextRenderer.MeasureText (dc, Text (), FontToDisplay).Height;

				measuredText = new Rectangle (xstart, ystart, xend, yend);
			} else {
				measuredText = new Rectangle ();
			}

			while (drawStart < drawEnd) {
				int tab_index = text.IndexOf ("\t", drawStart);
				
				if (tab_index == -1 || tab_index > drawEnd)
					tab_index = drawEnd;

				TextBoxTextRenderer.DrawText (dc, text.Substring (drawStart, tab_index - drawStart).Replace ("\r", string.Empty), FontToDisplay, color, xoff + line.widths [drawStart], y, false);

				// non multilines get the unknown char 
				if (!line.document.multiline && tab_index != drawEnd)
					TextBoxTextRenderer.DrawText (dc, "\u0013", FontToDisplay, color, xoff + line.widths [tab_index], y, true);

				drawStart = tab_index + 1;
			}
		}

		/// <summary>Checks if 'this' tag describes the same formatting options as 'obj'</summary>
		public override bool Equals (object obj)
		{
			LineTag other;

			if (obj == null)
				return false;

			if (!(obj is LineTag))
				return false;

			if (obj == this)
				return true;

			other = (LineTag)obj;

			if (other.IsTextTag != IsTextTag)
				return false;

			if (this.IsLink != other.IsLink)
				return false;

			if (this.LinkText != other.LinkText)
				return false;

			if (this.TextPosition != other.TextPosition)
				return false;

			if (this.CharOffset != other.CharOffset)
				return false;

			if (this.Visible != other.Visible)
				return false;

			if (this.font.Equals (other.font) && this.color.Equals (other.color) && this.back_color.Equals (other.back_color))
				return true;

			return false;
		}

		/// <summary>Finds the tag that describes the character at position 'pos' (0 based) on 'line'</summary>
		public static LineTag FindTag (Line line, int pos)
		{
			LineTag tag = line.tags;

			// Beginning of line is a bit special
			if (pos == 0)
				return tag;	// Not sure if we should get the final tag here

			while (tag != null) {
				// [H  e][l][l  o  _  W][o  r]  Text
				// [1  2][3][4  5  6  7][8  9]  Start
				//     3  4           8     10  End
				// 0 1  2  3  4  5  6  7  8  9   Pos
				if ((tag.start <= pos) && (pos < tag.End))
					return GetFinalTag (tag);

				tag = tag.next;
			}

			return null;
		}

		public static bool FormatText (Line line, int formatStart, int length, Font font, Color color, Color backColor, FormatSpecified specified)
		{
			return FormatText (line, formatStart, length, font, color, backColor, TextPositioning.Normal, 0, true, specified);
		}

		/// <summary>Applies 'font' and 'brush' to characters starting at 'start' for 'length' chars; 
		/// Removes any previous tags overlapping the same area; 
		/// returns true if lineheight has changed</summary>
		/// <param name="formatStart">1-based character position on line</param>
		public static bool FormatText (Line line, int formatStart, int length, Font font, Color color, Color backColor,
		                               TextPositioning text_position, float char_offset, bool visible, FormatSpecified specified)
		{
			LineTag tag;
			LineTag start_tag;
			LineTag end_tag;
			int end;
			bool retval = false;		// Assume line-height doesn't change

			// Too simple?
			if (((FormatSpecified.Font & specified) == FormatSpecified.Font) && font.Height != line.TextHeight)
				retval = true;

			line.recalc = true;		// This forces recalculation of the line in RecalculateDocument

			// A little sanity, not sure if it's needed, might be able to remove for speed
			if (length > line.text.Length)
				length = line.text.Length;

			tag = line.tags;
			end = formatStart + length;

			// Common special case
			if ((formatStart == 1) && (length == tag.Length)) {
				SetFormat (tag, font, color, backColor, text_position, char_offset, visible, specified);
				return retval;
			}

			// empty selection style at begining of line means
			// we only need one new tag
			if  (formatStart == 1 && length == 0) {
				line.tags.Break (1);
				SetFormat (line.tags, font, color, backColor, text_position, char_offset, visible, specified);
				return retval;
			}

			start_tag = FindTag (line, formatStart - 1);

			// we are at an empty tag already!
			// e.g. [Tag 0 - "He"][Tag 1 = 0 length][Tag 2 "llo world"]
			// Find Tag will return tag 0 at position 3, but we should just
			// use the empty tag after..
			if (start_tag.End == formatStart && length == 0 && start_tag.Next != null && start_tag.Next.Length == 0) {
				SetFormat (start_tag.Next, font, color, backColor, text_position, char_offset, visible, specified);
				return retval;
			}

			// if we are at the end of a tag, we want to move to the next tag
			while (start_tag.End == formatStart && start_tag.Next != null)
				start_tag = start_tag.Next;

			if (start_tag.Start == formatStart && start_tag.Length == length) {
				SetFormat (start_tag, font, color, backColor, text_position, char_offset, visible, specified);
				return retval;
			}

			// Break the tag if needed -- we don't need to break for the start if we're starting at its start.
			if (start_tag.Start != formatStart)
				tag = start_tag.Break (formatStart);
			else
				tag = start_tag;

			// empty selection style at end of line - its the only situation
			// where the rest of the tag would be empty, since we moved to the
			// begining of next non empty tag
			if (tag.Length == 0) {
				SetFormat (tag, font, color, backColor, text_position, char_offset, visible, specified);
				return retval;
			}

			// empty - so we just create another tag for
			// after our new (now) empty one..
			if (length == 0) {
				tag.Break (formatStart);
				SetFormat (tag, font, color, backColor, text_position, char_offset, visible, specified);
				return retval;
			}

			bool atEnd = false;
			while (tag != null && tag.End <= end) {
				SetFormat (tag, font, color, backColor, text_position, char_offset, visible, specified);
				atEnd |= tag.End == end;
				tag = tag.next;
			}

			// did the last tag conveniently fit?
			if (atEnd || (tag != null && tag.End == end))
				return retval;

			// Now do the last tag
			end_tag = FindTag (line, end-1);

			if (end_tag != null) {
				end_tag.Break (end);
				SetFormat (end_tag, font, color, backColor, text_position, char_offset, visible, specified);
			}

			return retval;
		}

		// Gets the character at the x-coordinate.  Index is based from the
		// line, not the start of the tag.
		// returns 0 based index (0 means before character at 1, 1 means at character 1)
		public int GetCharIndex (int x)
		{
			int low = start;
			int high = low + Length;
			int length_no_ending = line.TextLengthWithoutEnding ();
			float char_mid;

			if (Length == 0)
				return low-1;

			if (length_no_ending == 0)
				return 0;

			if (x < line.widths [low]) {
				char_mid = (line.widths [1] + line.widths [0]) / 2;
				if (low == 1 && x >= char_mid)
					return low;
				return low - 1;
			}

			if (x > line.widths[length_no_ending])
				return length_no_ending;
				
			while (low < high - 1) {
				int mid = (high + low) / 2;
				float width = line.widths[mid];

				if (width < x)
					low = mid;
				else
					high = mid;
			}

			char_mid = (line.widths [high] + line.widths [low]) / 2;

			if (x >= char_mid)
				return high;
			else
				return low;	
		}
		
		// There can be multiple tags at the same position, we want to make
		// sure we are using the very last tag at the given position
		// Empty tags are necessary if style is set at a position with
		// no length.
		public static LineTag GetFinalTag (LineTag tag)
		{
			LineTag res = tag;

			while (res.Length == 0 && res.next != null && res.next.Length == 0)
				res = res.next;

			return res;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		internal virtual int MaxHeight ()
		{
			return font.Height;
		}

		private static void SetFormat (LineTag tag, Font font, Color color, Color back_color, FormatSpecified specified)
		{
			SetFormat (tag, font, color, back_color, TextPositioning.Normal, 0, true, specified);
		}

		private static void SetFormat (LineTag tag, Font font, Color color, Color back_color, TextPositioning text_position,
		                               float char_offset, bool visible, FormatSpecified specified)
		{
			if ((FormatSpecified.Font & specified) == FormatSpecified.Font) {
				tag.Font = font;
			}
			if ((FormatSpecified.Color & specified) == FormatSpecified.Color)
				tag.color = color;
			if ((FormatSpecified.BackColor & specified) == FormatSpecified.BackColor) {
				tag.back_color = back_color;
			}
			if ((FormatSpecified.TextPosition & specified) == FormatSpecified.TextPosition)
				tag.TextPosition = text_position;
			if ((FormatSpecified.CharOffset & specified) == FormatSpecified.CharOffset)
				tag.CharOffset = char_offset;
			if ((FormatSpecified.Visibility & specified) == FormatSpecified.Visibility)
				tag.Visible = visible;
			// Console.WriteLine ("setting format:   {0}  {1}   new color {2}", color.Color, specified, tag.color.Color);
		}

		public virtual SizeF SizeOfPosition (Graphics dc, int pos)
		{
			if ((pos >= line.TextLengthWithoutEnding () && line.document.multiline) || !visible)
				return SizeF.Empty;

			string text = line.text.ToString (pos, 1);
			switch ((int)text [0]) {
			case '\t':
				if (!line.document.multiline)
					goto case 10;
				SizeF res = TextBoxTextRenderer.MeasureText (dc, " ", FontToDisplay); // This way we get the height, not that it is ever used...
				float left = line.widths [pos];
				float right = -1;
				TabStopCollection stops = line.tab_stops;
				float tabPos;
				for (int i = 0; i < stops.Count; i++) {
					tabPos = stops [i].Position;
					if (tabPos >= left) {
						if (tabPos <= line.document.viewport_width - line.RightIndent)
							break; // Can't use tabs that are past the end of the line.

						right = stops [i].CalculateRight (line, pos);
						break;
					}
				}
				if (right < 0) {
					float maxWidth = dc.DpiX / 2; // tab stops are 1/2"
					right = (float)(Math.Floor (left / maxWidth) + 1) * maxWidth;
				}
				res.Width = right - left;
				return res;
			case 10:
			case 13:
				return TextBoxTextRenderer.MeasureText (dc, "\u000D", FontToDisplay);
			}

			return TextBoxTextRenderer.MeasureText (dc, text, FontToDisplay);
		}

		public virtual string Text ()
		{
			return line.text.ToString (start - 1, Length);
		}

		public override string ToString ()
		{
			if (Length > 0)
				return string.Format ("{0} Tag starts at index: {1}, length: {2}, text: {3}, font: {4}", GetType (), start, Length, Text (), font.ToString ());
				
			return string.Format ("Zero Length tag at index: {0}", start);
		}
		#endregion	// Internal Methods
	}
}
