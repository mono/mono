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
		internal int			height;			// Total height of the line, including TotalParagraphSpacing and LineSpacing
		private int				textHeight;		// Height of the line without spacing.
		internal int			ascent;			// Ascent of the line (highest distance above the baseline, including character offset)
		internal HorizontalAlignment	alignment;		// Alignment of the line
		internal int			align_shift;		// Pixel shift caused by the alignment
		internal float			indent;			// Left indent for the first line
		internal float			hanging_indent;		// Hanging indent (difference between first line indent and other lines)
		internal float			right_indent;		// Right indent for all lines
		internal LineEnding		ending;
		internal float			spacing_before;
		internal float			spacing_after;
		internal float			line_spacing;
		internal bool			line_spacing_multiple;
		internal TabStopCollection		tab_stops;		// Custom tabstops for this paragraph.

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
			tab_stops = new TabStopCollection();

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
			tab_stops = new TabStopCollection();

			widths = new float[space + 1];

			
			tags = new LineTag(this, 1);
			tags.Font = font;
			tags.Color = color;
		}

		internal Line (Document document, int LineNo, string Text, HorizontalAlignment align, Font font, Color color,
		               Color back_color, TextPositioning text_position, float char_offset, float left_indent, float hanging_indent,
					   float right_indent, float spacing_before, float spacing_after, float line_spacing, bool line_spacing_multiple,
					   TabStopCollection tab_stops, bool visible, LineEnding ending) : this(document, ending)
		{
			space = Text.Length > DEFAULT_TEXT_LEN ? Text.Length+1 : DEFAULT_TEXT_LEN;

			text = new StringBuilder (Text, space);
			line_no = LineNo;
			this.ending = ending;
			alignment = align;
			indent = left_indent;
			HangingIndent = hanging_indent;
			this.right_indent = right_indent;
			this.spacing_before = spacing_before;
			this.spacing_after = spacing_after;
			this.tab_stops = tab_stops;
			this.line_spacing = line_spacing;
			this.line_spacing_multiple = line_spacing_multiple;

			widths = new float[space + 1];


			tags = new LineTag(this, 1);
			tags.Font = font;
			tags.Color = color;
			tags.BackColor = back_color;
			tags.TextPosition = text_position;
			tags.CharOffset = char_offset;
			tags.Visible = visible;
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

		internal float HangingIndent {
			get { return hanging_indent; }
			set {
				this.hanging_indent = value;
				recalc = true;
			}
		}

		// UIA: Method used via reflection in TextRangeProvider
		internal int Height {
			get { return height; }
			set { height = value; }
		}

		internal int TextHeight {
			get {
				return textHeight;
			}
		}

		internal TabStopCollection TabStops {
			get { return tab_stops; }
			set { tab_stops = value; }
		}

		internal float TotalParagraphSpacing {
			get {
				return SpacingBefore + SpacingAfter;
			}
		}

		internal float LineSpacing {
			get {
				if (textHeight == 0) {
					throw new InvalidOperationException("Can't get LineSpacing when the line height isn't calculated!");
				}
				if (line_spacing < 0) {
					return -line_spacing;
				} else if (line_spacing_multiple) {
					return line_spacing * textHeight * 6f / document.Dpi;
				} else {
					return Math.Max(line_spacing, textHeight);
				}
			}
		}

		internal float SpacingBefore {
			get {
				bool has_spacing = true;
				if (line_no > 1) {
					Line previous_line = document.GetLine(line_no - 1);
					if (previous_line != null && (previous_line.ending == LineEnding.Wrap || previous_line.ending == LineEnding.None))
						has_spacing = false;
				}
				if (has_spacing)
					return spacing_before;
				else 
					return 0;
			}
		}

		internal float SpacingAfter {
			get {
				if (ending == LineEnding.Wrap)
					return 0;
				else
					return spacing_after;
			}
		}

		internal float Indent {
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

		internal float RightIndent {
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
			tag = FindTag (pos);

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
				
				// Update the start of each tag
				while ((tag.Next != null) && (left > 0)) {
					tag = tag.Next;
					// Cache tag.Length as is will be indireclty modified
					// by changes to tag.Start
					int tag_length = tag.Length;
					tag.Start -= count - left;

					if (tag_length > left) {
						left = 0;
					} else {
						left -= tag_length;
					}

				}
			} else {
				// We got off easy, same tag

				if (tag.Length == 0)
					streamline = true;
			}

			LineTag walk = tag;

			// Adjust the start point of any tags following
			if (tag != null) {
				tag = tag.Next;
				while (tag != null) {
					tag.Start -= count;
					tag = tag.Next;
				}
			}

			// Delete empty orphaned tags at the end. Do this after adjusting their starts, otherwise we might delete tags that acutally do have content.
			while (walk != null && walk.Next != null && walk.Next.Length == 0) {
				LineTag t = walk;
				walk.Next = walk.Next.Next;
				if (walk.Next != null)
					walk.Next.Previous = t;
				walk = walk.Next;
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
				if (((tag.Start - 1) <= pos) && (pos < (tag.Start + tag.Length - 1)))
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

			// Check that tag is still in use in the line. If not, then we choose the last tag at that position.
			LineTag t = tags;
			while (t != null) {
				if (((t.Start - 1) <= pos) && (pos < (t.End - 1) || (pos == t.End - 1 && t.Length == 0))) {
					// found the location
					bool foundTag = false;
					while (pos < (t.Start + t.Length - 1)) {
						if (t == tag) {
							foundTag = true;
							break;
						}
						if (t.Next == null)
							break;
						t = t.Next;
					}
					if (!foundTag) {
						if (pos < (t.Start + t.Length - 1)) {
							tag = t.Previous;
						} else {
							tag = t;
						}
					}
					break;
				}
				t = t.Next;
			}

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
			LineTag tag;
			int pos;
			int len;
			Font currentFont;
			int currentFontStart;
			SizeF size;
			float w;
			int prev_offset;
			bool retval;
			bool wrapped;
			bool first_in_para;
			Line line;
			int wrap_pos;
			int prev_wrap_pos;
			int prev_height;
			int prev_ascent;
			float prev_spacing_before;
			int max_above_baseline;
			int max_below_baseline;
			int total_ascent;
			int total_descent;
			TabStop lastTab;
			int lastTabPos;
			char c;
			bool handleKerning;
			float right_indent;

			pos = 0;
			len = this.text.Length;
			currentFont = tags.FontToDisplay;
			currentFontStart = 0;
			tag = this.tags;
			prev_offset = this.offset;	// For drawing optimization calculations
			prev_height = this.height;
			prev_ascent = this.ascent;
			prev_spacing_before = this.SpacingBefore;
			max_above_baseline = 0;
			max_below_baseline = 0;
			total_ascent = 0;
			total_descent = 0;
			lastTab = null;
			lastTabPos = 0;
			this.height = 0;		// Reset line height
			this.ascent = 0;		// Reset the ascent for the line
			tag.Shift = 0;			// Reset shift (which should be stored as pixels, not as points)
			right_indent = Math.Max(this.right_indent, 0); // Ignore any negative right indent.

			if (line_no > 0) {
				line = doc.GetLine (LineNo - 1);
				first_in_para = line != null && line.ending != LineEnding.Wrap;
			} else {
				first_in_para = true;
			}

			if (first_in_para)
				widths [0] = indent;
			else
				widths [0] = indent + hanging_indent;

			if (widths [0] < 0)
				widths [0] = 0; // Don't allow a negative indent to take the line to a negative position.

			widths [0] += document.left_margin;

			this.recalc = false;
			retval = false;
			wrapped = false;

			wrap_pos = 0;
			prev_wrap_pos = 0;

			handleKerning = kerning_fonts.ContainsKey (currentFont.GetHashCode ());

			while (pos < len) {
				while (tag.Length == 0) {	// We should always have tags after a tag.length==0 unless len==0
					//tag.Ascent = 0;
					tag.Shift = (tag.Line.ascent - tag.Ascent); // / 72;
					tag = tag.Next;
					if (tag.Length != 0 && tag.FontToDisplay != currentFont) {
						CheckKerning (g, currentFont, currentFontStart, pos - currentFontStart);
						currentFont = tag.FontToDisplay;
						currentFontStart = pos;
						handleKerning = kerning_fonts.ContainsKey (currentFont.GetHashCode ());
					}
				}

				c = text [pos];

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
					size = TextBoxTextRenderer.MeasureText (g, text.ToString (currentFontStart, pos + 1 - currentFontStart), currentFont);
					newWidth = widths [currentFontStart] + size.Width;
				}
				else if (c != '\t') {
					size = tag.SizeOfPosition (g, pos);
					w = size.Width;
					newWidth = widths[pos] + w;
				} else {
					CheckKerning (g, currentFont, currentFontStart, pos - currentFontStart);
					currentFontStart = pos + 1; // Don't try handling the tab along with kerned text.

					if (lastTab != null) {
						ProcessLastTab (lastTab, lastTabPos, pos);
						lastTab = null;
					}

					float l = widths [pos];
					w = -1;
					for (int i = 0; i < tab_stops.Count; i++) {
						if (tab_stops [i].Position > l) {
							lastTab = tab_stops [i];
							lastTabPos = pos;
							w = lastTab.GetInitialWidth (this, pos);
							break;
						}
					}

					if (w < 0) {
						w = tag.SizeOfPosition (g, pos).Width;
					}

					newWidth = widths [pos] + w;
				}

				if (doc.Wrap) {
					// FIXME: Technically there are multiple no-break spaces, not just the main one.
					if (wrap_pos <= pos && ((Char.IsWhiteSpace (c) && c != '\u00A0') || c == '-' || c == '\u2013' || c == '\u2014')) {
						// Primarily break on dashes or whitespace other than a no-break space.
						prev_wrap_pos = wrap_pos;
						if (c == '\t') {
							wrap_pos = pos; // Wrap before tabs for some reason.
						} else {
							wrap_pos = pos + 1;
						}
					}

					if (newWidth > (doc.viewport_width - this.right_indent)) {
						LineTag split_tag = null;
						if (wrap_pos > 0) {
							// Make sure to set the last width of the line before wrapping
							widths [pos + 1] = newWidth;

							if (Char.IsWhiteSpace (c)) {
								if (wrap_pos > pos) {
									while (wrap_pos < text.Length && Char.IsWhiteSpace (text [wrap_pos]) && text [wrap_pos] != '\t') {
										// Leave whitespace other than tabs on the end of this line.
										wrap_pos++;
									}
									pos++;
									wrapped = true;
									// don't try pulling more into this line, but keep looping to deal with the rest of the widths and tags that will be left on the line
								} else {
									// At the wrap position, so split the line. c is a tab.
									split_tag = tag;
								}
							} else  {
								if (wrap_pos > pos && pos > 0) {
									// We're at a dash (otherwise we'd be above), but don't have room to fit it in.
									// Wrap at the previous wrap point if possible.
									wrap_pos = prev_wrap_pos > 0 ? prev_wrap_pos : pos;
								}
								split_tag = tag;
								pos = wrap_pos;
							}
						} else if (pos > 0) {
							// No suitable wrap position was found so break right in the middle of a word

							// Make sure to set the last width of the line before wrapping
							widths [pos + 1] = newWidth;

							split_tag = tag;
						} // Else don't wrap -- pos == 0, so we'd infinite loop adding blank lines before this.

						if (split_tag != null) {
							if (lastTab != null) {
								ProcessLastTab (lastTab, lastTabPos, pos);
								lastTab = null;
							}

							while (pos < split_tag.Start)
								split_tag = split_tag.Previous;
							// We have to pass Split the correct tag, and that can change if pos
							// is set somewhere before the tag change (e.g. by wrap_pos).

							doc.Split (this, split_tag, pos);
							ending = LineEnding.Wrap;
							len = this.text.Length;

							retval = true;
							wrapped = true;
						}
					}
				}

				// Contract all wrapped lines that follow back into our line
				if (!wrapped) {
					pos++;

					widths[pos] = newWidth;

					if (pos == len) {
						line = doc.GetLine (this.line_no + 1);
						do {
							if ((line != null) && (ending == LineEnding.Wrap || ending == LineEnding.None) &&
								(widths[pos] < (doc.viewport_width - this.right_indent) || line.text.Length == 0)) {
								// Pull the two lines together
								// Only do this if the line isn't already full, or the next line is empty.
								var h = this.height; // Back up h, because Combine sets it to zero.
								doc.Combine (this, line);
								this.height = h; // And restore it. There's no point starting at the start again.
								// Document.Combine() called Line.Streamline(), so it is possible tag points a tag that got removed.
								tag = FindTag (pos - 1); // So make sure we've got the correct tag.
								len = this.text.Length;
								line = doc.GetLine (this.line_no + 1);
								retval = true;
							}
						} while ((ending == LineEnding.Wrap || ending == LineEnding.None) && line != null && line.text.Length == 0);
						// If the next line is empty, do it again (if possible).
						// The amount of room on this line doesn't matter when there's no text being added...
					}
				}

				if (pos == (tag.Start - 1 + tag.Length)) {
					// We just found the end of our current tag
					tag.Height = tag.MaxHeight ();

					/* line.ascent is the highest point above the baseline.
					 * total_ascent will equal the maximum distance of the tag above the baseline.
					 * total_descent is needed to calculate the line height.
					 * tag.Shift does not include tag.CharOffset, because Shift puts the tag
					 * on the baseline, while CharOffset moves the baseline.
					 * However, we move the normal baseline when CharOffset is trying to push
					 * stuff off the top.
					 */
					total_ascent = tag.Ascent + (int)tag.CharOffset;
					total_descent = tag.Descent - (int)tag.CharOffset; // gets bigger as CharOffset gets smaller
					if (total_ascent > max_above_baseline) {
						int moveBy = total_ascent - max_above_baseline;
						max_above_baseline = total_ascent;

						LineTag t = tags;
						while (t != null && t != tag) {
							t.Shift += moveBy;
							t = t.Next;
						}

						tag.Shift = (int)tag.CharOffset;
						this.ascent = max_above_baseline;
					} else {
						tag.Shift = (this.ascent - tag.Ascent);
					}

					if (total_descent > max_below_baseline)
						max_below_baseline = total_descent;

					if (this.height < max_above_baseline + max_below_baseline + tag.Height - tag.Ascent - tag.Descent)
						this.height = max_above_baseline + max_below_baseline + tag.Height - tag.Ascent - tag.Descent;

					tag = tag.Next;
					if (tag != null) {
						if (tag.Length != 0 && tag.FontToDisplay != currentFont) {
							CheckKerning (g, currentFont, currentFontStart, pos - currentFontStart);
							currentFont = tag.FontToDisplay;
							currentFontStart = pos;
							handleKerning = kerning_fonts.ContainsKey (currentFont.GetHashCode ());
						}
						tag.Shift = 0;
						// We can't just wrap on tag boundaries -- e.g. if the first letter of the word has a different colour / font.
					}
				}
			}

			if (pos != currentFontStart) {
				CheckKerning (g, currentFont, currentFontStart, pos - currentFontStart);
			}

			if (lastTab != null) {
				ProcessLastTab (lastTab, lastTabPos, pos);
				lastTab = null;
			}

			while (tag != null) {
				tag.Shift = (tag.Line.ascent - tag.Ascent); // / 72;
				tag = tag.Next;
			}

			if (this.height == 0) {
				this.height = tags.Font.Height;
				tags.Height = this.height;
				tags.Shift = 0;
			}

			this.textHeight = this.height;
			this.height = (int)(this.LineSpacing + this.TotalParagraphSpacing);

			if (prev_offset != offset || prev_height != this.height || prev_ascent != this.ascent ||
				Math.Abs (prev_spacing_before - this.SpacingBefore) > document.Dpi / 1440f)
				retval = true;

			return retval;
		}

		private void ProcessLastTab (TabStop tab, int tab_pos, int pos)
		{
			float prevTabRight = widths[tab_pos + 1];
			float tabRight = tab.CalculateRight (this, tab_pos);
			float change = tabRight - prevTabRight;

			for (int i = tab_pos + 1; i <= pos; i++) {
				widths[i] += change;
			}
		}

		private void CheckKerning (Graphics g, Font font, int start, int length)
		{
			if (length > 1) {
				if (!kerning_fonts.ContainsKey (font.GetHashCode ())) {
					// Check whether kerning takes place for this string and font.
					var partText = text.ToString(start, length);
					var realSize = TextBoxTextRenderer.MeasureText(g, partText, font);
					float realWidth = realSize.Width + widths[start + 1];
					// MeasureText ignores trailing whitespace, so we will too at this point.
					int textLength = partText.TrimEnd().Length;
					float sumWidth = widths[textLength + start + 1];
					if (realWidth != sumWidth)
					{
						kerning_fonts.Add(font.GetHashCode (), true);
						// Using a slightly incorrect width this time around isn't that bad. All that happens
						// is that the cursor is a pixel or two off until the next character is typed.  It's
						// the accumulation of pixel after pixel that causes display problems.
					}
				}
			}
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

			if (this.textHeight != (int)tag.Font.Height)
				ret = true;
			else
				ret = false;

			this.textHeight = (int)tag.Font.Height;
			tag.Height = this.textHeight;
			this.height = (int)(this.LineSpacing + this.TotalParagraphSpacing);

			this.ascent = tag.Ascent;

			while (pos < len) {
				pos++;
				widths[pos] = widths[pos - 1] + w;
			}

			return ret;
		}

		internal void CalculateAlignment ()
		{
			var alignmentWidth = document.ViewPortWidth - document.left_margin - document.right_margin;
			var alignmentLineWidth = GetAlignmentLineWidth ();

			switch (alignment) {
			case HorizontalAlignment.Left:
				align_shift = 0;
				break;
			case HorizontalAlignment.Center:
				align_shift = (alignmentWidth - alignmentLineWidth) / 2;
				break;
			case HorizontalAlignment.Right:
				align_shift = alignmentWidth - alignmentLineWidth;
				break;
			}

			align_shift = Math.Max (align_shift, 0); // Don't allow negative shifts.
		}

		private int GetAlignmentLineWidth ()
		{
			int last = text.Length - 1;
			if (last < 0)
				return 0;

			char c = text [last];
			while (last > 0 && Char.IsWhiteSpace (c) && c != '\t' && c != '\u00A0') {
				c = text [--last];
			}
			// widths[0] has both the left margin and the left indents.
			// Remove the margin (it is part of the viewport) and add the right indents (part of the line width, for alignment purposes).
			return (int)(widths [last + 1] - document.left_margin + Math.Max (right_indent, 0));
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
				// Take out 0 length tags unless it's the last tag in the document.
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
