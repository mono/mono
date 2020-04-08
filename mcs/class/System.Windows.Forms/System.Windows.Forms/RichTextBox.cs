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
// Copyright (c) 2005-2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	<pbartok@novell.com>
//	Karl Scowen	<contact@scowencomputers.co.nz>
//
//

// #define DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using RTF=System.Windows.Forms.RTF;

namespace System.Windows.Forms {
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[Docking (DockingBehavior.Ask)]
	[ComVisible (true)]
	[Designer ("System.Windows.Forms.Design.RichTextBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class RichTextBox : TextBoxBase {
		#region Local Variables
		internal bool		auto_word_select;
		internal int		bullet_indent;
		internal bool		detect_urls;
		private bool		reuse_line;	// Sometimes we are loading text with already available lines
		internal int		margin_right;
		internal float		zoom;
		private StringBuilder	rtf_line;

		private RtfSectionStyle rtf_style;	// Replaces individual style
							// properties so we can revert
		private Stack		rtf_section_stack;

		bool		fire_contents_resized;
		Size		existing_contents_size;

		private int rtf_skip_count;
		private int		rtf_cursor_x;
		private int		rtf_cursor_y;
		private int		rtf_chars;

		private bool		enable_auto_drag_drop;
		private RichTextBoxLanguageOptions language_option;
		private bool		rich_text_shortcuts_enabled;

		#endregion	// Local Variables

		#region Public Constructors
		public RichTextBox() {
			accepts_return = true;
			auto_size = false;
			auto_word_select = false;
			bullet_indent = 0;
			base.MaxLength = Int32.MaxValue;
			margin_right = 0;
			zoom = 1;
			base.Multiline = true;
			document.CRLFSize = 1;
			shortcuts_enabled = true;
			base.EnableLinks = true;
			richtext = true;
			
			rtf_style = new RtfSectionStyle ();
			rtf_section_stack = null;

			fire_contents_resized = true;

			scrollbars = RichTextBoxScrollBars.Both;
			alignment = HorizontalAlignment.Left;
			document.SizeChanged += new EventHandler<Document.SizeChangedEventArgs> (ContentSizeChanged);
			LostFocus += new EventHandler(RichTextBox_LostFocus);
			GotFocus += new EventHandler(RichTextBox_GotFocus);
			BackColor = ThemeEngine.Current.ColorWindow;
			backcolor_set = false;
			language_option = RichTextBoxLanguageOptions.AutoFontSizeAdjust;
			rich_text_shortcuts_enabled = true;
			ForeColor = ThemeEngine.Current.ColorWindowText;

			base.HScrolled += new EventHandler(RichTextBox_HScrolled);
			base.VScrolled += new EventHandler(RichTextBox_VScrolled);

			SetStyle (ControlStyles.StandardDoubleClick, false);
		}
		#endregion	// Public Constructors

		#region Private & Internal Methods

		internal override void HandleLinkClicked (LinkRectangle link)
		{
			OnLinkClicked (new LinkClickedEventArgs (link.LinkTag.LinkText));
		}

		internal override Color ChangeBackColor (Color backColor)
		{
			if (backColor == Color.Empty) {
				backcolor_set = false;
				if (!ReadOnly) {
					backColor = SystemColors.Window;
				}
			}
			return backColor;
		}

		internal override void RaiseSelectionChanged()
		{
			OnSelectionChanged (EventArgs.Empty);
		}
		
		private void RichTextBox_LostFocus(object sender, EventArgs e) {
			Invalidate();
		}

		private void RichTextBox_GotFocus(object sender, EventArgs e) {
			Invalidate();
		}

		private void ContentSizeChanged (object sender, Document.SizeChangedEventArgs e)
		{
			ContentSizeChanged ();
		}

		private void ContentSizeChanged ()
		{
			if (fire_contents_resized && (existing_contents_size.IsEmpty ||
					existing_contents_size.Height != document.Height || existing_contents_size.Width != document.Width)) {
				int width;
				int height = document.Height + document.top_margin * 2 + Height - ClientRectangle.Height;

				if (Multiline) {
					width = Width; // yes, this is the insanity that is the traditional .Net implementation...
				} else {
					width = document.Width + document.left_margin + document.right_margin + Width - ClientRectangle.Width;
				}

				ContentsResizedEventArgs args = new ContentsResizedEventArgs (new Rectangle (Left, Top, width, height));
				OnContentsResized (args);
			}
		}
		#endregion	// Private & Internal Methods

		#region Public Instance Properties
		[Browsable (false)]
		public override bool AllowDrop {
			get {
				return base.AllowDrop;
			}

			set {
				base.AllowDrop = value;
			}
		}

		[DefaultValue(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override bool AutoSize {
			get {
				return auto_size;
			}

			set {
				base.AutoSize = value;
			}
		}

		[MonoTODO ("Value not respected, always true")]
		[DefaultValue(false)]
		public bool AutoWordSelection {
			get { return auto_word_select; }
			set { auto_word_select = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override System.Drawing.Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[DefaultValue(0)]
		[Localizable(true)]
		public int BulletIndent {
			get {
				return bullet_indent;
			}

			set {
				bullet_indent = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanRedo {
			get {
				return document.undo.CanRedo;
			}
		}

		[DefaultValue(true)]
		public bool DetectUrls {
			get { return base.EnableLinks; }
			set { base.EnableLinks = value; }
		}

		[MonoTODO ("Stub, does nothing")]
		[DefaultValue (false)]
		public bool EnableAutoDragDrop {
			get { return enable_auto_drag_drop; }
			set { enable_auto_drag_drop = value; }
		}

		public override Font Font {
			get {
				return base.Font;
			}

			set {
				if (font != value) {
					Line	start;
					Line	end;

					if (auto_size) {
						if (PreferredHeight != Height) {
							Height = PreferredHeight;
						}
					}

					base.Font = value;

					// Font changes always set the whole doc to that font
					start = document.GetLine(1);
					end = document.GetLine(document.Lines);
					document.FormatText(start, 1, end, end.text.Length + 1, base.Font, Color.Empty, Color.Empty, FormatSpecified.Font);
				}
			}
		}

		public override Color ForeColor {
			get {
				return base.ForeColor;
			}

			set {
				base.ForeColor = value;
			}
		}

		[MonoTODO ("Stub, does nothing")]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public RichTextBoxLanguageOptions LanguageOption {
			get { return language_option; }
			set { language_option = value; }
		}

		[DefaultValue(Int32.MaxValue)]
		public override int MaxLength {
			get { return base.MaxLength; }
			set { base.MaxLength = value; }
		}

		[DefaultValue(true)]
		public override bool Multiline {
			get {
				return base.Multiline;
			}

			set {
				base.Multiline = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string RedoActionName {
			get {
				return document.undo.RedoActionName;
			}
		}

		[MonoTODO ("Stub, does nothing")]
		[Browsable (false)]
		[DefaultValue (true)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool RichTextShortcutsEnabled {
			get { return rich_text_shortcuts_enabled; }
			set { rich_text_shortcuts_enabled = value; }
		}

		[DefaultValue(0)]
		[Localizable(true)]
		[MonoTODO ("Stub, does nothing")]
		[MonoInternalNote ("Teach TextControl.RecalculateLine to consider the right margin as well")]
		public int RightMargin {
			get {
				return margin_right;
			}

			set {
				margin_right = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[RefreshProperties (RefreshProperties.All)]
		public string Rtf {
			get {
				Line		start_line;
				Line		end_line;

				start_line = document.GetLine(1);
				end_line = document.GetLine(document.Lines);
				return GenerateRTF(start_line, 0, end_line, end_line.text.Length).ToString();
			}

			set {
				MemoryStream	data;

				fire_contents_resized = false;
				existing_contents_size = new Size (document.Width, document.Height);

				document.Empty();
				data = new MemoryStream(Encoding.ASCII.GetBytes(value), false);

				InsertRTFFromStream(data, 0, 1);

				Line line = document.GetLine (1);
				document.SetSelection (line, 0);
				document.PositionCaret (line, 0);

				data.Close();

				Invalidate();

				fire_contents_resized = true;
				ContentSizeChanged ();
				existing_contents_size = Size.Empty;
			}
		}

		[DefaultValue(RichTextBoxScrollBars.Both)]
		[Localizable(true)]
		public RichTextBoxScrollBars ScrollBars {
			get {
				return scrollbars;
			}

			set {
				if (!Enum.IsDefined (typeof (RichTextBoxScrollBars), value))
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (RichTextBoxScrollBars));

				if (value != scrollbars) {
					scrollbars = value;
					CalculateDocument ();
				}
			}
		}

		[Browsable(false)]
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string SelectedRtf {
			get {
				return GenerateRTF(document.selection_start.line, document.selection_start.pos, document.selection_end.line, document.selection_end.pos).ToString();
			}

			set {				
				MemoryStream	data;
				int		x;
				int		y;
				int		sel_start;
				int		chars;
				Line		line;
				LineTag		tag;

				fire_contents_resized = false;
				existing_contents_size = new Size (document.Width, document.Height);
				document.SuspendRecalc ();
				if (document.selection_visible) {
					document.ReplaceSelection("", false);
				}

				sel_start = document.LineTagToCharIndex(document.selection_start.line, document.selection_start.pos);

				data = new MemoryStream(Encoding.ASCII.GetBytes(value), false);
				int cursor_x = document.selection_start.pos;
				int cursor_y = document.selection_start.line.line_no;

				// The RFT parser by default, when finds our x cursor in 0, it thinks if needs to
				// add a new line; but in *this* scenario the line is already created, so force it to reuse it.
				// Hackish, but works without touching the heart of the buggy parser.
				if (cursor_x == 0)
					reuse_line = true;

				InsertRTFFromStream(data, cursor_x, cursor_y, out x, out y, out chars);
				data.Close();

				int nl_length = document.LineEndingLength (XplatUI.RunningOnUnix ? LineEnding.Rich : LineEnding.Hard);
				document.CharIndexToLineTag(sel_start + chars + (y - document.selection_start.line.line_no) * nl_length, 
						out line, out tag, out sel_start);
				if (sel_start > line.text.Length)
					sel_start = line.text.Length; //zero-based, but we want to go after the last character rather than before.

				document.SetSelection(line, sel_start);
				document.PositionCaret(line, sel_start);
				document.ResumeRecalc(true);
				document.DisplayCaret();
				ScrollToCaret();
				OnTextChanged(EventArgs.Empty);

				fire_contents_resized = true;
				ContentSizeChanged();
				existing_contents_size = Size.Empty;
			}
		}

		[Browsable(false)]
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string SelectedText {
			get {
				return base.SelectedText;
			}

			set {
				// TextBox/TextBoxBase don't set Modified in this same property
				Modified = true;
				base.SelectedText = value;
			}
		}

		[Browsable(false)]
		[DefaultValue(HorizontalAlignment.Left)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public HorizontalAlignment SelectionAlignment {
			get {
				HorizontalAlignment	align;
				Line			start;
				Line			end;
				Line			line;

				start = document.ParagraphStart(document.selection_start.line);
				align = start.alignment;

				end = document.ParagraphEnd(document.selection_end.line);

				line = start;

				while (true) {
					if (line.alignment != align) {
						return HorizontalAlignment.Left;
					}

					if (line == end) {
						break;
					}
					line = document.GetLine(line.line_no + 1);
				}

				return align;
			}

			set {
				Line			start;
				Line			end;
				Line			line;

				start = document.ParagraphStart(document.selection_start.line);

				end = document.ParagraphEnd(document.selection_end.line);

				line = start;

				while (true) {
					line.alignment = value;

					if (line == end) {
						break;
					}
					line = document.GetLine(line.line_no + 1);
				}
				this.CalculateDocument();
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Color SelectionBackColor {
			get {
				Color		back_colour;
				LineTag	start;
				LineTag	end;
				LineTag	tag;

				start = document.selection_start.line.FindTag (document.selection_start.pos);

				if (SelectionLength > 0) {
					end = document.selection_start.line.FindTag (document.selection_end.pos - 1);
				} else {
					end = start;
				}

				back_colour = start.BackColor;

				tag = start;
				while (tag != null) {
					if (back_colour != tag.BackColor)
						return Color.Empty;

					if (tag == end)
						break;

					tag = document.NextTag (tag);
				}

				return back_colour;
			}
			set {
				int		sel_start;
				int		sel_end;

				sel_start = document.LineTagToCharIndex (document.selection_start.line, document.selection_start.pos);
				sel_end = document.LineTagToCharIndex (document.selection_end.line, document.selection_end.pos);

				document.FormatText (document.selection_start.line, document.selection_start.pos + 1,
					document.selection_end.line, document.selection_end.pos + 1, null,
					Color.Empty, value, FormatSpecified.BackColor);

				document.CharIndexToLineTag (sel_start, out document.selection_start.line, out document.selection_start.tag, out document.selection_start.pos);
				document.CharIndexToLineTag (sel_end, out document.selection_end.line, out document.selection_end.tag, out document.selection_end.pos);

				document.UpdateView (document.selection_start.line, 0);
				//Re-Align the caret in case its changed size or position
				Document.AlignCaret (false);
			}
		}

		[Browsable(false)]
		[DefaultValue(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO ("Stub, does nothing")]
		public bool SelectionBullet {
			get {
				return false;
			}

			set {
			}
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionCharOffset {
			get {
				float		char_offset;
				LineTag	start;
				LineTag	end;
				LineTag	tag;

				start = document.selection_start.line.FindTag (document.selection_start.pos);

				if (SelectionLength > 0) {
					end = document.selection_start.line.FindTag (document.selection_end.pos - 1);
				} else {
					end = start;
				}

				char_offset = start.CharOffset;

				tag = start;
				while (tag != null) {
					if (Math.Abs(char_offset - tag.CharOffset) > 0.01)
						return 0;

					if (tag == end)
						break;

					tag = document.NextTag (tag);
				}

				return (int)char_offset;
			}

			set {
				int		sel_start;
				int		sel_end;

				sel_start = document.LineTagToCharIndex (document.selection_start.line, document.selection_start.pos);
				sel_end = document.LineTagToCharIndex (document.selection_end.line, document.selection_end.pos);

				document.FormatText (document.selection_start.line, document.selection_start.pos + 1,
					document.selection_end.line, document.selection_end.pos + 1, null,
					Color.Empty, Color.Empty, TextPositioning.Normal, value, true, FormatSpecified.CharOffset);

				document.CharIndexToLineTag (sel_start, out document.selection_start.line, out document.selection_start.tag, out document.selection_start.pos);
				document.CharIndexToLineTag (sel_end, out document.selection_end.line, out document.selection_end.tag, out document.selection_end.pos);

				document.UpdateView (document.selection_start.line, 0);
				//Re-Align the caret in case its changed size or position -- probably not necessary
				Document.AlignCaret (false);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color SelectionColor {
			get {
				Color	color;
				LineTag	start;
				LineTag	end;
				LineTag	tag;

				start = document.selection_start.line.FindTag (document.selection_start.pos);

				if (SelectionLength > 0) {
					end = document.selection_start.line.FindTag (document.selection_end.pos - 1);
				} else {
					end = start;
				}

				color = start.Color;

				tag = start;
				while (tag != null) {

					if (!color.Equals (tag.Color))
						return Color.Empty;

					if (tag == end)
						break;

					tag = document.NextTag (tag);
				}

				return color;
			}

			set {
				if (value == Color.Empty)
					value = DefaultForeColor;
					
				int sel_start;
				int sel_end;

				sel_start = document.LineTagToCharIndex(document.selection_start.line, document.selection_start.pos);
				sel_end = document.LineTagToCharIndex(document.selection_end.line, document.selection_end.pos);

				document.FormatText (document.selection_start.line, document.selection_start.pos + 1,
						document.selection_end.line, document.selection_end.pos + 1, null,
						value, Color.Empty, FormatSpecified.Color);

				document.CharIndexToLineTag(sel_start, out document.selection_start.line, out document.selection_start.tag, out document.selection_start.pos);
				document.CharIndexToLineTag(sel_end, out document.selection_end.line, out document.selection_end.tag, out document.selection_end.pos);

				document.UpdateView(document.selection_start.line, 0);

				//Re-Align the caret in case its changed size or position
				//probably not necessary here
				document.AlignCaret(false);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Font SelectionFont {
			get {
				Font	font;
				LineTag	start;
				LineTag	end;
				LineTag	tag;

				start = document.selection_start.line.FindTag (document.selection_start.pos);
				if (SelectionLength > 0) {
					end = document.selection_start.line.FindTag (document.selection_end.pos - 1);
				} else {
					end = start;
				}

				font = start.Font;

				if (SelectionLength > 1) {
					tag = start;
					while (tag != null) {

						if (!font.Equals(tag.Font))
							return null;

						if (tag == end)
							break;

						tag = document.NextTag (tag);
					}
				}

				return font;
			}

			set {
				int		sel_start;
				int		sel_end;

				sel_start = document.LineTagToCharIndex(document.selection_start.line, document.selection_start.pos);
				sel_end = document.LineTagToCharIndex(document.selection_end.line, document.selection_end.pos);

				document.FormatText (document.selection_start.line, document.selection_start.pos + 1,
						document.selection_end.line, document.selection_end.pos + 1, value,
						Color.Empty, Color.Empty, FormatSpecified.Font);

				document.CharIndexToLineTag(sel_start, out document.selection_start.line, out document.selection_start.tag, out document.selection_start.pos);
				document.CharIndexToLineTag(sel_end, out document.selection_end.line, out document.selection_end.tag, out document.selection_end.pos);

				document.UpdateView(document.selection_start.line, 0);
				//Re-Align the caret in case its changed size or position
				Document.AlignCaret (false);

			}
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionHangingIndent {
			get {
				int				indent;
				Line			start;
				Line			end;
				Line			line;

				start = document.ParagraphStart (document.selection_start.line);
				indent = (int)start.hanging_indent;

				end = document.ParagraphEnd (document.selection_end.line);

				line = start;

				while (true) {
					if ((int)line.hanging_indent != indent) {
						return 0;
					}

					if (line == end) {
						break;
					}
					line = document.GetLine (line.line_no + 1);
				}

				return indent;
			}

			set {
				Line			start;
				Line			end;
				Line			line;

				start = document.ParagraphStart (document.selection_start.line);
				end = document.ParagraphEnd (document.selection_end.line);

				line = start;

				while (true) {
					line.HangingIndent = value;

					if (line == end) {
						break;
					}
					line = document.GetLine (line.line_no + 1);
				}
				this.CalculateDocument ();
			}
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionIndent {
			get {
				int				indent;
				Line			start;
				Line			end;
				Line			line;

				start = document.ParagraphStart (document.selection_start.line);
				indent = (int)start.indent;

				end = document.ParagraphEnd (document.selection_end.line);

				line = start;

				while (true) {
					if ((int)line.indent != indent) {
						return 0;
					}

					if (line == end) {
						break;
					}
					line = document.GetLine (line.line_no + 1);
				}

				return indent;
			}

			set {
				Line			start;
				Line			end;
				Line			line;

				start = document.ParagraphStart (document.selection_start.line);
				end = document.ParagraphEnd (document.selection_end.line);

				line = start;

				while (true) {
					line.Indent = value;

					if (line == end) {
						break;
					}
					line = document.GetLine (line.line_no + 1);
				}
				this.CalculateDocument ();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override int SelectionLength {
			get {
				return base.SelectionLength;
			}

			set {
				base.SelectionLength = value;
			}
		}

		[Browsable(false)]
		[DefaultValue(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO ("Stub, does nothing")]
		public bool SelectionProtected {
			get {
				return false;
			}

			set {
			}
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionRightIndent {
			get {
				int				indent;
				Line			start;
				Line			end;
				Line			line;

				start = document.ParagraphStart (document.selection_start.line);
				indent = (int)start.right_indent;

				end = document.ParagraphEnd (document.selection_end.line);

				line = start;

				while (true) {
					if ((int)line.right_indent != indent) {
						return 0;
					}

					if (line == end) {
						break;
					}
					line = document.GetLine (line.line_no + 1);
				}

				return indent;
			}

			set {
				Line			start;
				Line			end;
				Line			line;

				start = document.ParagraphStart (document.selection_start.line);
				end = document.ParagraphEnd (document.selection_end.line);

				line = start;

				while (true) {
					line.RightIndent = value;

					if (line == end) {
						break;
					}
					line = document.GetLine (line.line_no + 1);
				}
				this.CalculateDocument ();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int[] SelectionTabs {
			get {
				TabStopCollection		tabs;
				Line			start;
				Line			end;
				Line			line;

				start = document.ParagraphStart (document.selection_start.line);
				tabs = start.TabStops;

				end = document.ParagraphEnd (document.selection_end.line);

				line = start;

				while (true) {
					if (!line.TabStops.Equals (tabs))
						return new int[0];

					if (line == end)
						break;
					line = document.GetLine (line.line_no + 1);
				}

				return tabs.ToPosArray ();
			}

			set {
				Line			start;
				Line			end;
				Line			line;

				start = document.ParagraphStart (document.selection_start.line);
				end = document.ParagraphEnd (document.selection_end.line);

				line = start;

				while (true) {
					line.TabStops.Clear ();
					foreach (int val in value)
						line.TabStops.Add (new LeftTabStop (val));

					if (line == end) {
						break;
					}
					line = document.GetLine (line.line_no + 1);
				}
				this.CalculateDocument ();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public RichTextBoxSelectionTypes SelectionType {
			get {
				if (document.selection_start == document.selection_end) {
					return RichTextBoxSelectionTypes.Empty;
				}

				// Lazy, but works
				if (SelectedText.Length > 1) {
					return RichTextBoxSelectionTypes.MultiChar | RichTextBoxSelectionTypes.Text;
				}

				return RichTextBoxSelectionTypes.Text;
			}
		}

		[DefaultValue(false)]
		[MonoTODO ("Stub, does nothing")]
		public bool ShowSelectionMargin {
			get {
				return false;
			}

			set {
			}
		}

		[Localizable(true)]
		[RefreshProperties (RefreshProperties.All)]
		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}

		[Browsable(false)]
		public override int TextLength {
			get {
				return base.TextLength;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string UndoActionName {
			get {
				return document.undo.UndoActionName;
			}
		}

		[Localizable(true)]
		[DefaultValue(1)]
		public float ZoomFactor {
			get {
				return zoom;
			}

			set {
				zoom = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size(100, 96);
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public bool CanPaste(DataFormats.Format clipFormat) {
			if ((clipFormat.Name == DataFormats.Rtf) ||
				(clipFormat.Name == DataFormats.Text) ||
				(clipFormat.Name == DataFormats.UnicodeText)) {
					return true;
			}
			return false;
		}

		public int Find(char[] characterSet) {
			return Find(characterSet, -1, -1);
		}

		public int Find(char[] characterSet, int start) {
			return Find(characterSet, start, -1);
		}

		public int Find(char[] characterSet, int start, int end) {
			Document.Marker	start_mark;
			Document.Marker end_mark;
			Document.Marker result;

			if (start == -1) {
				document.GetMarker(out start_mark, true);
			} else {
				Line line;
				LineTag tag;
				int pos;

				start_mark = new Document.Marker();

				document.CharIndexToLineTag(start, out line, out tag, out pos);
				start_mark.line = line;
				start_mark.tag = tag;
				start_mark.pos = pos;
			}

			if (end == -1) {
				document.GetMarker(out end_mark, false);
			} else {
				Line line;
				LineTag tag;
				int pos;

				end_mark = new Document.Marker();

				document.CharIndexToLineTag(end, out line, out tag, out pos);
				end_mark.line = line;
				end_mark.tag = tag;
				end_mark.pos = pos;
			}

			if (document.FindChars(characterSet, start_mark, end_mark, out result)) {
				return document.LineTagToCharIndex(result.line, result.pos);
			}

			return -1;
		}

		public int Find(string str) {
			return Find(str, -1, -1, RichTextBoxFinds.None);
		}

		public int Find(string str, int start, int end, RichTextBoxFinds options) {
			Document.Marker	start_mark;
			Document.Marker end_mark;
			Document.Marker result;

			if (start == -1) {
				document.GetMarker(out start_mark, true);
			} else {
				Line line;
				LineTag tag;
				int pos;

				start_mark = new Document.Marker();

				document.CharIndexToLineTag(start, out line, out tag, out pos);

				start_mark.line = line;
				start_mark.tag = tag;
				start_mark.pos = pos;
			}

			if (end == -1) {
				document.GetMarker(out end_mark, false);
			} else {
				Line line;
				LineTag tag;
				int pos;

				end_mark = new Document.Marker();

				document.CharIndexToLineTag(end, out line, out tag, out pos);

				end_mark.line = line;
				end_mark.tag = tag;
				end_mark.pos = pos;
			}

			if (document.Find(str, start_mark, end_mark, out result, options)) {
				return document.LineTagToCharIndex(result.line, result.pos);
			}

			return -1;
		}

		public int Find(string str, int start, RichTextBoxFinds options) {
			return Find(str, start, -1, options);
		}

		public int Find(string str, RichTextBoxFinds options) {
			return Find(str, -1, -1, options);
		}

		
		internal override char GetCharFromPositionInternal (Point p)
		{
			LineTag tag;
			int pos;

			PointToTagPos (p, out tag, out pos);

			if (pos >= tag.Line.text.Length)
				return '\n';

			return tag.Line.text[pos];
		}

		public override int GetCharIndexFromPosition(Point pt) {
			LineTag	tag;
			int	pos;

			PointToTagPos(pt, out tag, out pos);

			return document.LineTagToCharIndex(tag.Line, pos);
		}

		public override int GetLineFromCharIndex(int index) {
			Line	line;
			LineTag	tag;
			int	pos;

			document.CharIndexToLineTag(index, out line, out tag, out pos);

			return line.LineNo - 1;
		}

		public override Point GetPositionFromCharIndex(int index) {
			Line	line;
			LineTag	tag;
			int	pos;

			document.CharIndexToLineTag(index, out line, out tag, out pos);
			return new Point(line.X + (int)line.widths[pos] + document.OffsetX - document.ViewPortX, 
					 line.Y + document.OffsetY - document.ViewPortY);
		}

		public void LoadFile(System.IO.Stream data, RichTextBoxStreamType fileType) {
			document.Empty();

			
			// FIXME - ignoring unicode
			if (fileType == RichTextBoxStreamType.PlainText) {
				StringBuilder sb;
				char[] buffer;

				try {
					sb = new StringBuilder ((int) data.Length);
					buffer = new char [1024];
				} catch {
					throw new IOException("Not enough memory to load document");
				}

				StreamReader sr = new StreamReader (data, Encoding.Default, true);
				int charsRead = sr.Read (buffer, 0, buffer.Length);
				while (charsRead > 0) {
					sb.Append (buffer, 0, charsRead);
					charsRead = sr.Read (buffer, 0, buffer.Length);
				}

				// Remove the EOF converted to an extra EOL by the StreamReader
				if (sb.Length > 0 && sb [sb.Length - 1] == '\n')
					sb.Remove (sb.Length - 1, 1);

				base.Text = sb.ToString();
				return;
			}

			InsertRTFFromStream(data, 0, 1);

			document.PositionCaret (document.GetLine (1), 0);
			document.SetSelectionToCaret (true);
			ScrollToCaret ();
		}

		public void LoadFile(string path) {
			LoadFile (path, RichTextBoxStreamType.RichText);
		}

		public void LoadFile(string path, RichTextBoxStreamType fileType) {
			FileStream	data;

			data = null;


			try {
				data = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024);

				LoadFile(data, fileType);
			}
#if !DEBUG
			catch (Exception ex) {
				throw new IOException("Could not open file " + path, ex);
			}
#endif
			finally {
				if (data != null) {
					data.Close();
				}
			}
		}

		public void Paste(DataFormats.Format clipFormat) {
			base.Paste(Clipboard.GetDataObject(), clipFormat, false);
		}

		public void Redo()
		{
			if (document.undo.Redo ())
				OnTextChanged (EventArgs.Empty);
		}

		public void SaveFile(Stream data, RichTextBoxStreamType fileType) {
			Encoding	encoding;
			int		i;
			Byte[]		bytes;


			if (fileType == RichTextBoxStreamType.UnicodePlainText) {
				encoding = Encoding.Unicode;
			} else {
				encoding = Encoding.ASCII;
			}

			switch(fileType) {
				case RichTextBoxStreamType.PlainText: 
				case RichTextBoxStreamType.TextTextOleObjs: 
				case RichTextBoxStreamType.UnicodePlainText: {
					if (!Multiline) {
						bytes = encoding.GetBytes(document.Root.text.ToString());
						data.Write(bytes, 0, bytes.Length);
						return;
					}

					for (i = 1; i < document.Lines; i++) {
						// Normalize the new lines to the system ones
						string line_text = document.GetLine (i).TextWithoutEnding () + Environment.NewLine;
						bytes = encoding.GetBytes(line_text);
						data.Write(bytes, 0, bytes.Length);
					}
					bytes = encoding.GetBytes(document.GetLine(document.Lines).text.ToString());
					data.Write(bytes, 0, bytes.Length);
					return;
				}
			}

			// If we're here we're saving RTF
			Line		start_line;
			Line		end_line;
			StringBuilder	rtf;
			int		current;
			int		total;

			start_line = document.GetLine(1);
			end_line = document.GetLine(document.Lines);
			rtf = GenerateRTF(start_line, 0, end_line, end_line.text.Length);
			total = rtf.Length;
			bytes = new Byte[4096];

			// Let's chunk it so we don't use up all memory...
			for (i = 0; i < total; i += 1024) {
				if ((i + 1024) < total) {
					current = encoding.GetBytes(rtf.ToString(i, 1024), 0, 1024, bytes, 0);
				} else {
					current = total - i;
					current = encoding.GetBytes(rtf.ToString(i, current), 0, current, bytes, 0);
				}
				data.Write(bytes, 0, current);
			}
		}

		public void SaveFile(string path) {
			if (path.EndsWith(".rtf")) {
				SaveFile(path, RichTextBoxStreamType.RichText);
			} else {
				SaveFile(path, RichTextBoxStreamType.PlainText);
			}
		}

		public void SaveFile(string path, RichTextBoxStreamType fileType) {
			FileStream	data;

			data = null;

//			try {
				data = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1024, false);
				SaveFile(data, fileType);
//			}

//			catch {
//				throw new IOException("Could not write document to file " + path);
//			}

//			finally {
				if (data != null) {
					data.Close();
				}
//			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public new void DrawToBitmap (Bitmap bitmap, Rectangle targetBounds)
		{
			using (Graphics dc = Graphics.FromImage (bitmap))
				Draw (dc, targetBounds);
		}

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual object CreateRichEditOleCallback()
		{
			throw new NotImplementedException ();
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected virtual void OnContentsResized (ContentsResizedEventArgs e)
		{
			ContentsResizedEventHandler eh = (ContentsResizedEventHandler)(Events [ContentsResizedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnContextMenuChanged (EventArgs e)
		{
			base.OnContextMenuChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected virtual void OnHScroll(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [HScrollEvent]);
			if (eh != null)
				eh (this, e);
		}

		[MonoTODO ("Stub, never called")]
		protected virtual void OnImeChange(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ImeChangeEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnLinkClicked(LinkClickedEventArgs e) {
			LinkClickedEventHandler eh = (LinkClickedEventHandler)(Events [LinkClickedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnProtected(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ProtectedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnRightToLeftChanged(EventArgs e) {
			base.OnRightToLeftChanged (e);
		}

		protected virtual void OnSelectionChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [SelectionChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnVScroll(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [VScrollEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
		}

		protected override bool ProcessCmdKey (ref Message m, Keys keyData)
		{
			return base.ProcessCmdKey (ref m, keyData);
		}
		#endregion	// Protected Instance Methods

		#region Events
		static object ContentsResizedEvent = new object ();
		static object HScrollEvent = new object ();
		static object ImeChangeEvent = new object ();
		static object LinkClickedEvent = new object ();
		static object ProtectedEvent = new object ();
		static object SelectionChangedEvent = new object ();
		static object VScrollEvent = new object ();

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		public event ContentsResizedEventHandler ContentsResized {
			add { Events.AddHandler (ContentsResizedEvent, value); }
			remove { Events.RemoveHandler (ContentsResizedEvent, value); }
		}

		[Browsable(false)]
		public new event DragEventHandler DragDrop {
			add { base.DragDrop += value; }
			remove { base.DragDrop -= value; }
		}

		[Browsable(false)]
		public new event DragEventHandler DragEnter {
			add { base.DragEnter += value; }
			remove { base.DragEnter -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler DragLeave {
			add { base.DragLeave += value; }
			remove { base.DragLeave -= value; }
		}


		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event DragEventHandler DragOver {
			add { base.DragOver += value; }
			remove { base.DragOver -= value; }
		}


		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event GiveFeedbackEventHandler GiveFeedback {
			add { base.GiveFeedback += value; }
			remove { base.GiveFeedback -= value; }
		}

		public event EventHandler HScroll {
			add { Events.AddHandler (HScrollEvent, value); }
			remove { Events.RemoveHandler (HScrollEvent, value); }
		}

		public event EventHandler ImeChange {
			add { Events.AddHandler (ImeChangeEvent, value); }
			remove { Events.RemoveHandler (ImeChangeEvent, value); }
		}

		public event LinkClickedEventHandler LinkClicked {
			add { Events.AddHandler (LinkClickedEvent, value); }
			remove { Events.RemoveHandler (LinkClickedEvent, value); }
		}

		public event EventHandler Protected {
			add { Events.AddHandler (ProtectedEvent, value); }
			remove { Events.RemoveHandler (ProtectedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event QueryContinueDragEventHandler QueryContinueDrag {
			add { base.QueryContinueDrag += value; }
			remove { base.QueryContinueDrag -= value; }
		}

		[MonoTODO ("Event never raised")]
		public event EventHandler SelectionChanged {
			add { Events.AddHandler (SelectionChangedEvent, value); }
			remove { Events.RemoveHandler (SelectionChangedEvent, value); }
		}

		public event EventHandler VScroll {
			add { Events.AddHandler (VScrollEvent, value); }
			remove { Events.RemoveHandler (VScrollEvent, value); }
		}
		#endregion	// Events

		#region Private Methods

		internal override void SelectWord ()
		{
			document.ExpandSelection(CaretSelection.Word, false);
		}

		private class RtfSectionStyle : ICloneable {
			internal Color rtf_color;
			internal Color rtf_back_color;
			internal RTF.Font rtf_rtffont;
			internal float rtf_rtffont_size;
			internal FontStyle rtf_rtfstyle;
			internal HorizontalAlignment rtf_par_align;
			internal float rtf_par_line_left_indent;
			internal float rtf_par_first_line_indent;
			internal float rtf_par_line_right_indent;
			internal bool rtf_visible;
			internal int rtf_skip_width;
			internal float rtf_par_spacing_after;
			internal float rtf_par_spacing_before;
			internal float rtf_par_line_spacing;
			internal bool rtf_par_line_spacing_multiple;
			internal TextPositioning rtf_text_position;
			internal float rtf_char_offset;
			internal TabStop rtf_par_next_tab_stop = null;
			internal TabStopCollection rtf_par_tab_stops = new TabStopCollection ();

			public object Clone ()
			{
				RtfSectionStyle new_style = new RtfSectionStyle ();

				new_style.rtf_color = rtf_color;
				new_style.rtf_back_color = rtf_back_color;
				new_style.rtf_par_line_left_indent = rtf_par_line_left_indent;
				new_style.rtf_par_first_line_indent = rtf_par_first_line_indent;
				new_style.rtf_par_line_right_indent = rtf_par_line_right_indent;
				new_style.rtf_par_align = rtf_par_align;
				new_style.rtf_rtffont = rtf_rtffont;
				new_style.rtf_rtffont_size = rtf_rtffont_size;
				new_style.rtf_rtfstyle = rtf_rtfstyle;
				new_style.rtf_visible = rtf_visible;
				new_style.rtf_skip_width = rtf_skip_width;
				new_style.rtf_par_spacing_after = rtf_par_spacing_after;
				new_style.rtf_par_spacing_before = rtf_par_spacing_before;
				new_style.rtf_par_line_spacing = rtf_par_line_spacing;
				new_style.rtf_par_line_spacing_multiple = rtf_par_line_spacing_multiple;
				new_style.rtf_text_position = rtf_text_position;
				new_style.rtf_char_offset = rtf_char_offset;
				new_style.rtf_par_tab_stops = rtf_par_tab_stops.Clone ();

				return new_style;
			}
		}

		// To allow us to keep track of the sections and revert formatting
		// as we go in and out of sections of the document.
		private void HandleGroup (RTF.RTF rtf)
		{
			//start group - save the current formatting on to a stack
			//end group - go back to the formatting at the current group
			if (rtf_section_stack == null) {
				rtf_section_stack = new Stack ();
			}

			if (rtf.Major == RTF.Major.BeginGroup) {
				rtf_section_stack.Push (rtf_style.Clone ());
				//spec specifies resetting unicode ignore at begin group as an attempt at error
				//recovery.
				rtf_skip_count = 0;
			} else if (rtf.Major == RTF.Major.EndGroup) {
				if (rtf_section_stack.Count > 0) {
					FlushText (rtf, false);

					rtf_style = (RtfSectionStyle) rtf_section_stack.Pop ();
				}
			}
		}

		[MonoInternalNote ("Add QuadJust support for justified alignment")]
		private void HandleControl(RTF.RTF rtf) {
			switch(rtf.Major) {
				case RTF.Major.Unicode: {
					switch(rtf.Minor) {
						case RTF.Minor.UnicodeCharBytes: {
							rtf_style.rtf_skip_width = rtf.Param;
							break;
						}

						case RTF.Minor.UnicodeChar: {
							FlushText (rtf, false);
							rtf_skip_count += rtf_style.rtf_skip_width;
							rtf_line.Append((char)rtf.Param);
							break;
						}
					}
					break;
				}

				case RTF.Major.Destination: {
//					Console.Write("[Got Destination control {0}]", rtf.Minor);
					rtf.SkipGroup();
					break;
				}

				case RTF.Major.PictAttr:
					if (rtf.Picture != null && rtf.Picture.IsValid ()) {
						FlushText (rtf, false, true);
						Line line = document.GetLine (rtf_cursor_y);
						document.InsertPicture (line, rtf_cursor_x++, rtf.Picture);
						rtf.Picture = null;
					}
					break;

				case RTF.Major.CharAttr: {
					switch(rtf.Minor) {
						case RTF.Minor.ForeColor: {
							System.Windows.Forms.RTF.Color	color;

							color = System.Windows.Forms.RTF.Color.GetColor (rtf, rtf.Param);

							if (color != null) {
								FlushText (rtf, false);
								if (color.Red == -1 && color.Green == -1 && color.Blue == -1) {
									this.rtf_style.rtf_color = ForeColor;
								} else {
									this.rtf_style.rtf_color = Color.FromArgb (color.Red, color.Green, color.Blue);
								}
								FlushText (rtf, false);
							}
							break;
						}

						case RTF.Minor.BackColor: {
							System.Windows.Forms.RTF.Color	color;

							color = System.Windows.Forms.RTF.Color.GetColor(rtf, rtf.Param);

							if (color != null) {
								FlushText(rtf, false);
								if (color.Red == -1 && color.Green == -1 && color.Blue == -1) {
									this.rtf_style.rtf_back_color = BackColor;
								} else {
									this.rtf_style.rtf_back_color = Color.FromArgb(color.Red, color.Green, color.Blue);
								}
								FlushText (rtf, false);
							}
							break;
						}

						case RTF.Minor.FontSize: {
							FlushText(rtf, false);
							this.rtf_style.rtf_rtffont_size = rtf.Param / 2f;
							break;
						}

						case RTF.Minor.FontNum: {
							System.Windows.Forms.RTF.Font	font;

							font = System.Windows.Forms.RTF.Font.GetFont(rtf, rtf.Param);
							if (font != null) {
								FlushText(rtf, false);
								this.rtf_style.rtf_rtffont = font;
							}
							break;
						}

						case RTF.Minor.Plain: {
							FlushText(rtf, false);
							rtf_style.rtf_rtfstyle = FontStyle.Regular;
							break;
						}

						case RTF.Minor.Bold: {
							FlushText(rtf, false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_style.rtf_rtfstyle |= FontStyle.Bold;
							} else {
								rtf_style.rtf_rtfstyle &= ~FontStyle.Bold;
							}
							break;
						}

						case RTF.Minor.Italic: {
							FlushText(rtf, false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_style.rtf_rtfstyle |= FontStyle.Italic;
							} else {
								rtf_style.rtf_rtfstyle &= ~FontStyle.Italic;
							}
							break;
						}

						case RTF.Minor.StrikeThru: {
							FlushText(rtf, false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_style.rtf_rtfstyle |= FontStyle.Strikeout;
							} else {
								rtf_style.rtf_rtfstyle &= ~FontStyle.Strikeout;
							}
							break;
						}

						case RTF.Minor.Underline: {
							FlushText(rtf, false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_style.rtf_rtfstyle |= FontStyle.Underline;
							} else {
								rtf_style.rtf_rtfstyle = rtf_style.rtf_rtfstyle & ~FontStyle.Underline;
							}
							break;
						}

						case RTF.Minor.Invisible: {
							FlushText (rtf, false);
							rtf_style.rtf_visible = (rtf.Param != RTF.RTF.NoParam);
							break;
						}

						case RTF.Minor.NoUnderline: {
							FlushText(rtf, false);
							rtf_style.rtf_rtfstyle &= ~FontStyle.Underline;
							break;
						}

						case RTF.Minor.SuperScrShrink: {
							FlushText (rtf, false);
							rtf_style.rtf_text_position = TextPositioning.Superscript;
							break;
						}

						case RTF.Minor.SubScrShrink: {
							FlushText (rtf, false);
							rtf_style.rtf_text_position = TextPositioning.Subscript;
							break;
						}

						case RTF.Minor.NoSuperSub: {
							FlushText (rtf, false);
							rtf_style.rtf_text_position = TextPositioning.Normal;
							break;
						}

						case RTF.Minor.SuperScript: {
							FlushText (rtf, false);
							rtf_style.rtf_char_offset = ((float) rtf.Param / 144.0F) * document.Dpi;
							break;
						}

						case RTF.Minor.SubScript: {
							FlushText (rtf, false);
							rtf_style.rtf_char_offset = -((float) rtf.Param / 144.0F) * document.Dpi;
							break;
						}
					}
					break;
				}

			case RTF.Major.ParAttr: {
				switch (rtf.Minor) {

				case RTF.Minor.ParDef:
					FlushText (rtf, false);
					rtf_style.rtf_par_line_left_indent = 0;
					rtf_style.rtf_par_first_line_indent = 0;
					rtf_style.rtf_par_line_right_indent = 0;
					rtf_style.rtf_par_spacing_after = 0;
					rtf_style.rtf_par_spacing_before = 0;
					rtf_style.rtf_par_line_spacing = 0;
					rtf_style.rtf_par_line_spacing_multiple = false;
					rtf_style.rtf_par_align = HorizontalAlignment.Left;
					rtf_style.rtf_par_next_tab_stop = null;
					rtf_style.rtf_par_tab_stops.Clear ();
					break;

				case RTF.Minor.TabLeft:
					rtf_style.rtf_par_next_tab_stop = new LeftTabStop ();
					break;

				case RTF.Minor.TabCenter:
					rtf_style.rtf_par_next_tab_stop = new CentredTabStop ();
					break;

				case RTF.Minor.TabRight:
					rtf_style.rtf_par_next_tab_stop = new RightTabStop ();
					break;

				case RTF.Minor.TabDecimal:
					rtf_style.rtf_par_next_tab_stop = new DecimalTabStop ();
					break;

				case RTF.Minor.TabPos:
					float tabPos = ((float)rtf.Param / 1440.0F) * document.Dpi;
					if (rtf_style.rtf_par_next_tab_stop != null) {
						rtf_style.rtf_par_next_tab_stop.Position = tabPos;
						rtf_style.rtf_par_tab_stops.Add (rtf_style.rtf_par_next_tab_stop);
						rtf_style.rtf_par_next_tab_stop = null;
					} else {
						rtf_style.rtf_par_tab_stops.Add (new LeftTabStop (tabPos));
					}
					break;

				case RTF.Minor.LeftIndent:
					rtf_style.rtf_par_line_left_indent = ((float) rtf.Param / 1440.0F) * document.Dpi;
					break;

				case RTF.Minor.FirstIndent:
					rtf_style.rtf_par_first_line_indent = ((float) rtf.Param / 1440.0F) * document.Dpi;
					break;

				case RTF.Minor.RightIndent:
					rtf_style.rtf_par_line_right_indent = ((float) rtf.Param / 1440.0F) * document.Dpi;
					break;

				case RTF.Minor.QuadCenter:
					FlushText (rtf, false);
					rtf_style.rtf_par_align = HorizontalAlignment.Center;
					break;

				case RTF.Minor.QuadJust:
					FlushText (rtf, false);
					rtf_style.rtf_par_align = HorizontalAlignment.Left;
					break;

				case RTF.Minor.QuadLeft:
					FlushText (rtf, false);
					rtf_style.rtf_par_align = HorizontalAlignment.Left;
					break;

				case RTF.Minor.QuadRight:
					FlushText (rtf, false);
					rtf_style.rtf_par_align = HorizontalAlignment.Right;
					break;

				case RTF.Minor.SpaceAfter:
					rtf_style.rtf_par_spacing_after = ((float) rtf.Param / 1440.0F) * document.Dpi;
					break;

				case RTF.Minor.SpaceBefore:
					rtf_style.rtf_par_spacing_before = ((float) rtf.Param / 1440.0F) * document.Dpi;
					break;

				case RTF.Minor.SpaceBetween:
					rtf_style.rtf_par_line_spacing = ((float) rtf.Param / 1440.0F) * document.Dpi;
					break;

				case RTF.Minor.SpaceMultiply:
					rtf_style.rtf_par_line_spacing_multiple = (rtf.Param == 1);
					break;
				}
				break;
			}

			case RTF.Major.SpecialChar: {
					//Console.Write("[Got SpecialChar control {0}]", rtf.Minor);
					SpecialChar (rtf);
					break;
				}
			}
		}

		private void SpecialChar(RTF.RTF rtf) {
			switch(rtf.Minor) {
				case RTF.Minor.Page:
				case RTF.Minor.Sect:
				case RTF.Minor.Row:
				case RTF.Minor.Line:
				case RTF.Minor.Par: {
					if (Multiline)
						FlushText (rtf, true);
					break;
				}

				case RTF.Minor.Cell: {
					Console.Write(" ");
					break;
				}

				case RTF.Minor.NoBrkSpace: {
					Console.Write(" ");
					break;
				}

				case RTF.Minor.Tab: {
					rtf_line.Append ("\t");
//					FlushText (rtf, false);
					break;
				}

				case RTF.Minor.NoReqHyphen:
				case RTF.Minor.NoBrkHyphen: {
					rtf_line.Append ("-");
//					FlushText (rtf, false);
					break;
				}

				case RTF.Minor.Bullet: {
					Console.WriteLine("*");
					break;
				}

				case RTF.Minor.WidowCtrl:
					break;

				case RTF.Minor.EmDash: {
				rtf_line.Append ("\u2014");
					break;
				}

				case RTF.Minor.EnDash: {
					rtf_line.Append ("\u2013");
					break;
				}

				case RTF.Minor.LQuote: {
					rtf_line.Append ("\u2018");
					break;
				}

				case RTF.Minor.RQuote: {
					rtf_line.Append ("\u2019");
					break;
				}

				case RTF.Minor.LDblQuote: {
					rtf_line.Append ("\u201C");
					break;
				}

				case RTF.Minor.RDblQuote: {
					rtf_line.Append ("\u201D");
					break;
				}

				default: {
//					Console.WriteLine ("skipped special char:   {0}", rtf.Minor);
//					rtf.SkipGroup();
					break;
				}
			}
		}

		private void HandleText(RTF.RTF rtf) {
			string str = rtf.EncodedText;

			//todo - simplistically skips characters, should skip bytes?
			if (rtf_skip_count > 0 && str.Length > 0) {
				int iToRemove = Math.Min (rtf_skip_count, str.Length);

				str = str.Substring (iToRemove);
				rtf_skip_count-=iToRemove;
			}

			/*
			if ((RTF.StandardCharCode)rtf.Minor != RTF.StandardCharCode.nothing) {
				rtf_line.Append(rtf_text_map[(RTF.StandardCharCode)rtf.Minor]);
			} else {
				if ((int)rtf.Major > 31 && (int)rtf.Major < 128) {
					rtf_line.Append((char)rtf.Major);
				} else {
					//rtf_line.Append((char)rtf.Major);
					Console.Write("[Literal:0x{0:X2}]", (int)rtf.Major);
				}
			}
			*/

			rtf_line.Append (str);
		}

		private void FlushText(RTF.RTF rtf, bool newline) {
			FlushText (rtf, newline, false);
		}

		private void FlushText(RTF.RTF rtf, bool newline, bool force) {
			int		length;
			float hanging_indent;
			float left_indent;
			Font		font;

			length = rtf_line.Length;
			if (!newline && (length == 0) && !force) {
				return;
			}

			if (rtf_style.rtf_rtffont == null) {
				// First font in table is default
				rtf_style.rtf_rtffont = System.Windows.Forms.RTF.Font.GetFont (rtf, 0);
			}

			font = new Font (rtf_style.rtf_rtffont.Name, rtf_style.rtf_rtffont_size, rtf_style.rtf_rtfstyle);
			if (font.Name != rtf_style.rtf_rtffont.Name && !string.IsNullOrEmpty (rtf_style.rtf_rtffont.AltName))
				font = new Font (rtf_style.rtf_rtffont.AltName, rtf_style.rtf_rtffont_size, rtf_style.rtf_rtfstyle);

			hanging_indent = -rtf_style.rtf_par_first_line_indent;
			left_indent = rtf_style.rtf_par_line_left_indent - hanging_indent;
			
			if (rtf_style.rtf_color == Color.Empty) {
				System.Windows.Forms.RTF.Color color;

				// First color in table is default
				color = System.Windows.Forms.RTF.Color.GetColor (rtf, 0);

				if ((color == null) || (color.Red == -1 && color.Green == -1 && color.Blue == -1)) {
					rtf_style.rtf_color = ForeColor;
				} else {
					rtf_style.rtf_color = Color.FromArgb (color.Red, color.Green, color.Blue);
				}

			}

			rtf_chars += rtf_line.Length;

			// Try to re-use if we are told so - this usually happens when we are inserting a flow of rtf text
			// with an already alive line.
			if (rtf_cursor_x == 0 && !reuse_line) {
				if (newline && rtf_line.ToString ().EndsWith (Environment.NewLine) == false)
					rtf_line.Append (Environment.NewLine);

				document.Add (rtf_cursor_y, rtf_line.ToString (), rtf_style.rtf_par_align, font, rtf_style.rtf_color,
					rtf_style.rtf_back_color, rtf_style.rtf_text_position, rtf_style.rtf_char_offset, left_indent, hanging_indent,
					rtf_style.rtf_par_line_right_indent, rtf_style.rtf_par_spacing_before, rtf_style.rtf_par_spacing_after,
					rtf_style.rtf_par_line_spacing, rtf_style.rtf_par_line_spacing_multiple,
					rtf_style.rtf_par_tab_stops.Clone() , rtf_style.rtf_visible,
					newline ? LineEnding.Rich : LineEnding.None);
			} else {
				Line line = document.GetLine (rtf_cursor_y);

				if (newline) {
					if (rtf_cursor_x < line.text.Length)
						document.Split(line, rtf_cursor_x);
					line.ending = LineEnding.Rich;
				}

				line.indent = left_indent;
				line.HangingIndent = hanging_indent;
				line.right_indent = rtf_style.rtf_par_line_right_indent;
				line.spacing_after = rtf_style.rtf_par_spacing_after;
				line.spacing_before = rtf_style.rtf_par_spacing_before;
				line.line_spacing = rtf_style.rtf_par_line_spacing;
				line.line_spacing_multiple = rtf_style.rtf_par_line_spacing_multiple;
				line.alignment = rtf_style.rtf_par_align;

				if (rtf_line.Length > 0) {
					document.InsertString (line, rtf_cursor_x, rtf_line.ToString ());
					document.FormatText (line, rtf_cursor_x + 1, line, rtf_cursor_x + 1 + length,
					    font, rtf_style.rtf_color, rtf_style.rtf_back_color, rtf_style.rtf_text_position, rtf_style.rtf_char_offset,
					    rtf_style.rtf_visible, FormatSpecified.Font | FormatSpecified.Color | FormatSpecified.BackColor |
					    FormatSpecified.TextPosition | FormatSpecified.CharOffset | FormatSpecified.Visibility);
				}

				if (newline && line.Text.EndsWith (Environment.NewLine) == false)
					line.Text += Environment.NewLine;

				reuse_line = false; // sanity assignment - in this case we have already re-used one line.
			}

			if (newline) {
				rtf_cursor_x = 0;
				rtf_cursor_y++;
			} else {
				rtf_cursor_x += length;
			}
			rtf_line.Length = 0;	// Empty line
		}

		private void InsertRTFFromStream(Stream data, int cursor_x, int cursor_y) {
			int	x;
			int	y;
			int	chars;

			InsertRTFFromStream(data, cursor_x, cursor_y, out x, out y, out chars);
		}

		private void InsertRTFFromStream(Stream data, int cursor_x, int cursor_y, out int to_x, out int to_y, out int chars) {
			RTF.RTF		rtf;

			rtf = new RTF.RTF(data);

			// Prepare
			rtf.ClassCallback[RTF.TokenClass.Text] = new RTF.ClassDelegate(HandleText);
			rtf.ClassCallback[RTF.TokenClass.Control] = new RTF.ClassDelegate(HandleControl);
			rtf.ClassCallback[RTF.TokenClass.Group] = new RTF.ClassDelegate(HandleGroup);

			rtf_skip_count = 0;
			rtf_line = new StringBuilder();
			rtf_style.rtf_color = Color.Empty;
			rtf_style.rtf_back_color = Color.Empty;
			rtf_style.rtf_rtffont_size = (int)this.Font.Size;
			rtf_style.rtf_par_align = HorizontalAlignment.Left;
			rtf_style.rtf_rtfstyle = FontStyle.Regular;
			rtf_style.rtf_text_position = TextPositioning.Normal;
			rtf_style.rtf_par_spacing_after = 0;
			rtf_style.rtf_par_spacing_before = 0;
			rtf_style.rtf_par_line_spacing = 0;
			rtf_style.rtf_par_line_spacing_multiple = false;
			rtf_style.rtf_par_line_left_indent = 0;
			rtf_style.rtf_par_first_line_indent = 0;
			rtf_style.rtf_par_line_right_indent = 0;
			rtf_style.rtf_par_tab_stops.Clear ();
			rtf_style.rtf_char_offset = 0;
			rtf_style.rtf_rtffont = null;
			rtf_style.rtf_visible = true;
			rtf_style.rtf_skip_width = 1;
			rtf_cursor_x = cursor_x;
			rtf_cursor_y = cursor_y;
			rtf_chars = 0;
			rtf.DefaultFont(this.Font.Name);

			document.SuspendRecalc ();

			try {
				rtf.Read();	// That's it
				FlushText(rtf, false);

				if (document.Lines > 1) {
					Line last_line = document.GetLine (document.Lines);
					if (last_line.text.Length == 0) {
						document.Delete (last_line);
					}
				}
			}


			catch (RTF.RTFException e) {
#if DEBUG
				throw e;
#endif
				// Seems to be plain text or broken RTF
				Console.WriteLine("RTF Parsing failure: {0}", e.Message);
			}                     

			to_x = rtf_cursor_x;
			to_y = rtf_cursor_y;
			chars = rtf_chars;

			// clear the section stack if it was used
			if (rtf_section_stack != null)
				rtf_section_stack.Clear();

			if (IsHandleCreated) {
				CalculateScrollBars ();
				using (var graphics = CreateGraphics())
					document.RecalculateDocument(graphics, cursor_y, document.Lines, false);
				document.ResumeRecalc (true);
				document.InvalidateLinesAfter(document.GetLine(cursor_y));
			} else {
				document.ResumeRecalc (false);
			}
		}

		private void RichTextBox_HScrolled(object sender, EventArgs e) {
			OnHScroll(e);
		}

		private void RichTextBox_VScrolled(object sender, EventArgs e) {
			OnVScroll(e);
		}

		private void PointToTagPos(Point pt, out LineTag tag, out int pos) {
			Point p;

			p = pt;

			if (p.X >= document.ViewPortWidth) {
				p.X = document.ViewPortWidth - 1;
			} else if (p.X < 0) {
				p.X = 0;
			}

			if (p.Y >= document.ViewPortHeight) {
				p.Y = document.ViewPortHeight - 1;
			} else if (p.Y < 0) {
				p.Y = 0;
			}

			tag = document.FindCursor(p.X + document.ViewPortX, p.Y + document.ViewPortY, out pos);
		}

		private void EmitRTFFontProperties(StringBuilder rtf, int prev_index, int font_index, Font prev_font, Font font) {
			if (prev_index != font_index) {
				rtf.Append(String.Format("\\f{0}", font_index));	// Font table entry
			}

			if ((prev_font == null) || (Math.Abs (prev_font.Size - font.Size) > 0.01)) {
				rtf.Append(String.Format("\\fs{0}", (int)(font.Size * 2)));		// Font size
			}

			if ((prev_font == null) || (font.Bold != prev_font.Bold)) {
				if (font.Bold) {
					rtf.Append("\\b");
				} else {
					if (prev_font != null) {
						rtf.Append("\\b0");
					}
				}
			}

			if ((prev_font == null) || (font.Italic != prev_font.Italic)) {
				if (font.Italic) {
					rtf.Append("\\i");
				} else {
					if (prev_font != null) {
						rtf.Append("\\i0");
					}
				}
			}

			if ((prev_font == null) || (font.Strikeout != prev_font.Strikeout)) {
				if (font.Strikeout) {
					rtf.Append("\\strike");
				} else {
					if (prev_font != null) {
						rtf.Append("\\strike0");
					}
				}
			}

			if ((prev_font == null) || (font.Underline != prev_font.Underline)) {
				if (font.Underline) {
					rtf.Append("\\ul");
				} else {
					if (prev_font != null) {
						rtf.Append("\\ul0");
					}
				}
			}
		}

		static readonly char [] ReservedRTFChars = new char [] { '\\', '{', '}' };

		private void EmitRTFText(StringBuilder rtf, string text) {
			int start = rtf.Length;
			int count = text.Length;

			// This method emits user text *only*, so it's safe to escape any reserved rtf chars
			// Escape '\' first, since it is used later to escape the other chars
			if (text.IndexOfAny (ReservedRTFChars) > -1) {
				StringBuilder sb = new StringBuilder(text); // Would it be better to just use text = text.Replace for this?
				sb.Replace ("\\", "\\\\");
				sb.Replace ("{", "\\{");
				sb.Replace ("}", "\\}");
				text = sb.ToString ();
			}

			// Then actually emit the text, and also escape any Unicode
			EmitEscapedUnicode (rtf, text);
		}

		private void EmitEscapedUnicode (StringBuilder sb, string text)
		{
			int pos;
			int start = 0;

			while ((pos = IndexOfNonAscii (text, start)) > -1) {
				sb.Append (text, start, pos - start);

				short n = (short)text [pos];
				sb.Append ("\\u");
				sb.Append (n.ToString ());
				sb.Append ("?");

				start = pos + 1;
			}

			// Append remaining (maybe all) the text value.
			if (start < text.Length)
				sb.Append (text, start, text.Length - start);
		}

		// MS seems to be escaping values larger than 0x80
		private int IndexOfNonAscii (string text, int startIndex)
		{
			for (int i = startIndex; i < text.Length; i++) {
				int n = (int)text [i];
				if (n < 0 || n >= 0x80)
					return i;
			}

			return -1;
		}

		static char[] GetHexChars (byte[] bytes, int length)
		{
			if (length > bytes.Length)
				throw new ArgumentOutOfRangeException ("length");

			var chars = new char [length * 2];
			int n;
			for (int i = 0; i < length; i++) {
				n = bytes [i] >> 4;
				chars [i * 2] = (char)('A' - 10 + n + (((n - 10) >> 31) & ('0' - 55)));
				n = bytes [i] & 0x0F;
				chars [i * 2 + 1] = (char)('A' - 10 + n + (((n - 10) >> 31) & ('0' - 55)));
			}
			return chars;
		}

		void EmitRtfPicture (PictureTag picture, StringBuilder sb)
		{
			if (!picture.picture.IsValid ()) {
				return;
			}

			int width = (int)((float)picture.picture.Width / document.Dpi * 1440f);
			int height = (int)((float)picture.picture.Height / document.Dpi * 1440f);
			string type = "";
			switch (picture.picture.ImageType) {
			case RTF.Minor.WinMetafile:
				type = "wmetafile1"; // The number should actually vary, but I don't see how it is used here at all.
				break;
			case RTF.Minor.EnhancedMetafile:
				type = "emfblip";
				break;
			case RTF.Minor.PngBlip:
				type = "pngblip";
				break;
			case RTF.Minor.JpegBlip:
				type = "jpegblip";
				break;
			}
			sb.AppendFormat ("{{\\pict\\{0}\\picwgoal{1}\\pichgoal{2} ", type, width, height);

			var data = picture.picture.Data;
			data.Position = 0;
			if (sb.Capacity - sb.Length < data.Length) {
				sb.Capacity += (int)data.Length * 2;
			}
			var buffer = new byte [39];
			int length;
			while ((length = data.Read (buffer, 0, buffer.Length)) > 0) {
				sb.AppendLine ().Append (GetHexChars (buffer, length));
			}
			sb.Append ("}");
		}

		void EmitTabStops (StringBuilder sb, TabStopCollection tabs)
		{
			foreach (var tab in tabs) {
				if (tab is DecimalTabStop) {
					sb.Append ("\\tqdec");
				} else if (tab is CentredTabStop) {
					sb.Append ("\\tqc");
				} else if (tab is RightTabStop) {
					sb.Append ("\\tqr");
				}
				sb.Append ("\\tx");
				sb.Append (Int (tab.Position / document.Dpi * 1440f));
			}
		}

		void EmitPard (StringBuilder sb, ArrayList fonts, Line line, LineTag tag, TabStopCollection tabs, float ppt)
		{
			var first_line_indent = -line.HangingIndent;
			var left_indent = line.Indent - first_line_indent;
			var right_indent = line.RightIndent;

			sb.Append ("\\pard");
			// Reset to default paragraph properties
			switch (line.alignment) {
			case HorizontalAlignment.Left:
				sb.Append ("\\ql");
				break;
			case HorizontalAlignment.Center:
				sb.Append ("\\qc");
				break;
			case HorizontalAlignment.Right:
				sb.Append ("\\qr");
				break;
			}
			if (Math.Abs (line.spacing_after) > ppt) {
				sb.Append ("\\sa");
				sb.Append (Int (line.spacing_after / ppt));
			}
			if (Math.Abs (line.spacing_before) > ppt) {
				sb.Append ("\\sb");
				sb.Append (Int (line.spacing_before / ppt));
			}
			if (Math.Abs (line.line_spacing) > ppt) {
				sb.Append ("\\sl");
				sb.Append (Int (line.line_spacing / ppt));
				sb.Append ("\\slmult");
				sb.Append (line.line_spacing_multiple ? "1" : "0");
			}
			if (Math.Abs (left_indent) > ppt) {
				sb.Append ("\\li");
				sb.Append (Int (left_indent / ppt));
			}
			if (Math.Abs (first_line_indent) > ppt) {
				sb.Append ("\\fi");
				sb.Append (Int (first_line_indent / ppt));
			}
			if (Math.Abs (right_indent) > ppt) {
				sb.Append ("\\ri");
				sb.Append (Int (right_indent / ppt));
			}
			if (tabs.Count > 0) {
				EmitTabStops (sb, tabs);
			}
		}

		static void LoadParaSettings (Line line, out HorizontalAlignment line_alignment, out float spacing_after, out float spacing_before, out float line_spacing,
		                              out bool line_spacing_multiple, out float left_indent, out float prev_left_indent, out float first_line_indent,
									  out float prev_first_line_indent, out float right_indent, out TabStopCollection tabs)
		{
			spacing_after = line.spacing_after;
			spacing_before = line.spacing_before;
			line_spacing = line.line_spacing;
			line_spacing_multiple = line.line_spacing_multiple;
			line_alignment = line.alignment;
			first_line_indent = -line.HangingIndent;
			left_indent = line.Indent - first_line_indent;
			prev_first_line_indent = first_line_indent;
			prev_left_indent = left_indent;
			right_indent = line.RightIndent;
			tabs = line.TabStops;
		}

		// start_pos and end_pos are 0-based
		private StringBuilder GenerateRTF(Line start_line, int start_pos, Line end_line, int end_pos) {
			StringBuilder	sb;
			ArrayList	fonts;
			ArrayList	colors;
			Color		color;
			Color		back_color;
			Font		font;
			Line		line;
			LineTag		tag;
			TextPositioning text_position;
			HorizontalAlignment line_alignment;
			float		spacing_after;
			float		spacing_before;
			float		line_spacing;
			bool		line_spacing_multiple;
			float		left_indent;
			float		prev_left_indent;
			float		first_line_indent;
			float		prev_first_line_indent;
			float		right_indent;
			TabStopCollection tabs;
			TabStopCollection tabDiff;
			bool emit_defaults;
			float		char_offset;
			bool	visible;
			int		pos;
			int		line_no;
			int		line_len;
			int		i, j;
			int		length;
			float 	ppt; // pixels per twip

			ppt = document.Dpi / 1440f; // 1 twip = 1/20 point, 1 point = 1/72 inch, thus 1440 twips = 1 inch.
			emit_defaults = false;

			sb = new StringBuilder();
			fonts = new ArrayList(10);
			colors = new ArrayList(10);
			tabDiff = new TabStopCollection ();

			// Two runs, first we parse to determine tables;
			// and unlike most of our processing here we work on tags

			line = start_line;
			line_no = start_line.line_no;
			pos = start_pos;

			// Add default font and color; to optimize document content we don't
			// use this.Font and this.ForeColor but the font/color from the first tag
			tag = LineTag.FindTag(start_line, pos);
			font = tag.Font;
			color = tag.Color;
			back_color = Color.Empty;
			fonts.Add(font.Name);
			colors.Add(color);

			while (line_no <= end_line.line_no) {
				line = document.GetLine(line_no);
				tag = LineTag.FindTag(line, pos);

				if (line_no != end_line.line_no) {
					line_len = line.text.Length;
				} else {
					line_len = end_pos;
				}

				while (pos < line_len) {
					if (tag.Font.Name != font.Name) {
						font = tag.Font;
						if (!fonts.Contains(font.Name)) {
							fonts.Add(font.Name);
						}
					}

					if (tag.Color != color) {
						color = tag.Color;
						if (color != Color.Empty && !colors.Contains (color)) {
							colors.Add(color);
						}
					}

					if (tag.BackColor != back_color) {
						back_color = tag.BackColor;
						if (back_color != Color.Empty && !colors.Contains (back_color)) {
							colors.Add (back_color);
						}
					}

					pos = tag.Start + tag.Length - 1;
					tag = tag.Next;
				}
				pos = 0;
				line_no++;
			}

			// We have the tables, emit the header
			sb.Append("{\\rtf1\\ansi");
			sb.Append("\\ansicpg1252");	// FIXME - is this correct?

			// Default Font
			sb.Append(String.Format("\\deff{0}", fonts.IndexOf(this.Font.Name)));

			// Default Language 
			sb.Append("\\deflang1033" + Environment.NewLine);	// FIXME - always 1033?

			// Emit the font table
			sb.Append("{\\fonttbl");
			for (i = 0; i < fonts.Count; i++) {
				sb.Append(String.Format("{{\\f{0}", i));	// {Font 
				sb.Append("\\fnil");			// Family
				sb.Append("\\fcharset0 ");		// Charset ANSI<space>
				sb.Append((string)fonts[i]);		// Font name
				sb.Append(";}");			// }
			}
			sb.Append("}");
			sb.Append(Environment.NewLine);

			// Emit the color table (if needed)
			if ((colors.Count > 1) || ((((Color)colors[0]).R != this.ForeColor.R) || (((Color)colors[0]).G != this.ForeColor.G) || (((Color)colors[0]).B != this.ForeColor.B))) {
				sb.Append("{\\colortbl;");			// Header and default color (default is needed)
				for (i = 0; i < colors.Count; i++) {
					sb.Append(String.Format("\\red{0}", ((Color)colors[i]).R));
					sb.Append(String.Format("\\green{0}", ((Color)colors[i]).G));
					sb.Append(String.Format("\\blue{0}", ((Color)colors[i]).B));
					sb.Append(";");
				}
				sb.Append("}");
				sb.Append(Environment.NewLine);
			}

			sb.Append("{\\*\\generator Mono RichTextBox;}");

			tag = LineTag.FindTag (start_line, start_pos);
			font = tag.Font;
			color = Color.Empty;
			back_color = Color.Empty;
			text_position = TextPositioning.Normal;
			char_offset = 0;
			visible = true;
			line = start_line;
			line_no = start_line.line_no;
			pos = start_pos;
			LoadParaSettings (line, out line_alignment, out spacing_after, out spacing_before, out line_spacing,
				out line_spacing_multiple, out left_indent, out prev_left_indent, out first_line_indent,
				out prev_first_line_indent, out right_indent, out tabs);

			EmitPard (sb, fonts, line, tag, tabs, ppt);
			EmitRTFFontProperties (sb, -1, fonts.IndexOf (tag.Font.Name), null, tag.Font); // Font properties
			sb.Append(" "); // Space separator

			while (line_no <= end_line.line_no) {
				line = document.GetLine(line_no);
				tag = LineTag.FindTag(line, pos);

				if (line_no != end_line.line_no) {
					line_len = line.text.Length;
				} else {
					line_len = end_pos;
				}

				i = 0;
				j = 0;
				tabDiff.Clear ();
				emit_defaults = line.TabStops.Count < tabs.Count; // If there are less tabs on the new line, we've got to start over.
				while (!emit_defaults && i < tabs.Count && j < line.TabStops.Count) {
					if (tabs [i].Equals (line.TabStops [j])) {
						i++;
						j++;
					} else if (tabs [i].Position - ppt > line.TabStops [j].Position) {
						// The current tabstop is after the new line's one, so we need to add one in between.
						tabDiff.Add (line.TabStops [j]);
						j++;
					} else {
						// Either the tabs are at the same position and are of different types, or this line is missing at least one.
						// This in turn means we must start the line with a \pard, and re-emit all other paragraph properties.
						emit_defaults = true;
					}
				}
				if (i < tabs.Count) // We didn't reach the end of the existing tabstops, so the rest have to be removed.
					emit_defaults = true;
				while (!emit_defaults && j < line.TabStops.Count) { // Any new ones have to be added on the end too.
					tabDiff.Add (line.TabStops[j]);
					j++;
				}
				tabs = line.TabStops;

				if (!emit_defaults) {
					length = sb.Length;
					if (line.Alignment != line_alignment) {
						line_alignment = line.Alignment;
						switch (line_alignment) {
						case HorizontalAlignment.Left:
							sb.Append("\\ql");
							break;
						case HorizontalAlignment.Center:
							sb.Append("\\qc");
							break;
						case HorizontalAlignment.Right:
							sb.Append("\\qr");
							break;
						}
					}

					if (Math.Abs(line.spacing_after - spacing_after) > ppt) {
						spacing_after = line.spacing_after;
						sb.Append("\\sa");
						sb.Append(Int(spacing_after / ppt));
					}

					if (Math.Abs(line.spacing_before - spacing_before) > ppt) {
						spacing_before = line.spacing_before;
						sb.Append("\\sb");
						sb.Append(Int(spacing_before / ppt));
					}

					if (Math.Abs(line.line_spacing - line_spacing) > ppt) {
						line_spacing = line.line_spacing;
						sb.Append("\\sl");
						sb.Append(Int(line.line_spacing / ppt));
					}

					if (line.line_spacing_multiple != line_spacing_multiple) {
						line_spacing_multiple = line.line_spacing_multiple;
						sb.Append("\\slmult");
						sb.Append(line.line_spacing_multiple ? "1" : "0");
					}

					first_line_indent = -line.HangingIndent;
					left_indent = line.Indent - first_line_indent;

					if (Math.Abs(prev_left_indent - left_indent) > ppt) {
						prev_left_indent = left_indent;
						sb.Append("\\li");
						sb.Append(Int(left_indent / ppt));
					}

					if (Math.Abs(prev_first_line_indent - first_line_indent) > ppt) {
						prev_first_line_indent = first_line_indent;
						sb.Append("\\fi");
						sb.Append(Int(first_line_indent / ppt));
					}

					if (Math.Abs(line.right_indent - right_indent) > ppt) {
						right_indent = line.right_indent;
						sb.Append("\\ri");
						sb.Append(Int(right_indent / ppt));
					}

					if (tabDiff.Count > 0) {
						EmitTabStops(sb, tabDiff);
					}

					if (length != sb.Length) {
						sb.Append(" ");
					}
				} else {
					EmitPard (sb, fonts, line, tag, tabs, ppt);
					sb.Append(" ");
					LoadParaSettings (line, out line_alignment, out spacing_after, out spacing_before, out line_spacing,
						out line_spacing_multiple, out left_indent, out prev_left_indent, out first_line_indent,
						out prev_first_line_indent, out right_indent, out tabs);
				}

				while (pos < line_len && tag != null) {
					length = sb.Length;

					if (tag.Font != font) {
						EmitRTFFontProperties(sb, fonts.IndexOf(font.Name), fonts.IndexOf(tag.Font.Name), font, tag.Font);
						font = tag.Font;
					}

					if (tag.Color != color) {
						color = tag.Color;
						if (color != Color.Empty)
							sb.Append(String.Format("\\cf{0}", colors.IndexOf(color) + 1));
						else
							sb.Append("\\cf0");
					}

					if (tag.BackColor != back_color) {
						back_color = tag.BackColor;
						if (back_color != Color.Empty)
							sb.Append(String.Format("\\cb{0}", colors.IndexOf(back_color) + 1));
						else
							sb.Append("\\cb0");
					}

					if (tag.TextPosition != text_position) {
						if (text_position != TextPositioning.Normal && tag.TextPosition != TextPositioning.Normal)
							sb.Append("\\nosupersub");
						// Technically it is possible to have subscripts in superscript and vise versa. But that's not what we've got.
						text_position = tag.TextPosition;
						switch (tag.TextPosition) {
						case TextPositioning.Normal: 
							sb.Append("\\nosupersub");
							break;
						case TextPositioning.Subscript:
							sb.Append("\\sub");
							break;
						case TextPositioning.Superscript:
							sb.Append("\\super");
							break;
						}
					}

					if (tag.CharOffset != char_offset) {
						char_offset = tag.CharOffset;
						if (char_offset >= 0) {
							sb.Append("\\up");
							sb.Append(Int((char_offset / document.Dpi) * 144));
						} else {
							sb.Append("\\dn");
							sb.Append(-Int((char_offset / document.Dpi) * 144));
						}
					}

					if (tag.Visible != visible) {
						visible = tag.Visible;
						if (visible)
							sb.Append("\\v0");
						else
							sb.Append("\\v");
					}

					if (length != sb.Length) {
						sb.Append(" ");	// Emit space to separate keywords from text
					}

					// Emit the string itself
					if (tag is PictureTag) {
						EmitRtfPicture((PictureTag)tag, sb);
					} else if (line_no != end_line.line_no) {
						EmitRTFText(sb, tag.Line.text.ToString(pos, tag.Start + tag.Length - pos - 1));
					} else {
						if (end_pos < (tag.Start + tag.Length - 1)) {
							// Emit partial tag only, end_pos is inside this tag
							EmitRTFText(sb, tag.Line.text.ToString(pos, end_pos - pos));
						} else {
							EmitRTFText(sb, tag.Line.text.ToString(pos, tag.Start + tag.Length - pos - 1));
						}
					}

					pos = tag.Start + tag.Length - 1;
					do {
						tag = tag.Next;
					} while (tag != null && tag.IsTextTag && tag.Length == 0);
				}
				if (pos >= line.text.Length) {
					if (line.ending != LineEnding.Wrap) {
						// pos is incremented by the tag length, so it can be after where we want to finish.
						// If we're on the last line we don't want to output \par when we're stopping before the end of the line.
						if (!(line_no == end_line.line_no && pos > end_pos))
							sb.Append("\\par");
						sb.Append(Environment.NewLine);
					}
				}
				pos = 0;
				line_no++;
			}

			sb.Append("}");
			sb.Append(Environment.NewLine);

			return sb;
		}

		int Int (float f)
		{
			return (int)(f + 0.5f);
		}
		#endregion	// Private Methods
	}
}
