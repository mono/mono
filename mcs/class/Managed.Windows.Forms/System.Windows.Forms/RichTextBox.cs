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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	<pbartok@novell.com>
//
//

// NOT COMPLETE

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using RTF=System.Windows.Forms.RTF;

namespace System.Windows.Forms {
	public class RichTextBox : TextBoxBase {
		#region Local Variables
		internal bool		auto_word_select;
		internal int		bullet_indent;
		internal bool		can_redo;
		internal bool		detect_urls;
		internal string		redo_action_name;
		internal int		margin_right;
		internal string		undo_action_name;
		internal float		zoom;

		private RTF.TextMap	rtf_text_map;
		private int		rtf_skip_width;
		private int		rtf_skip_count;
		private StringBuilder	rtf_line;
		private Font		rtf_font;
		private SolidBrush	rtf_color;
		private RTF.Font	rtf_rtffont;
		private int		rtf_rtffont_size;
		private FontStyle	rtf_rtfstyle;
		private HorizontalAlignment rtf_rtfalign;
		private int		rtf_cursor_x;
		private int		rtf_cursor_y;
		#endregion	// Local Variables

		#region Public Constructors
		public RichTextBox() {
			accepts_return = true;
			auto_word_select = false;
			bullet_indent = 0;
			can_redo = false;
			detect_urls = true;
			max_length = Int32.MaxValue;
			redo_action_name = string.Empty;
			margin_right = 0;
			undo_action_name = string.Empty;
			zoom = 1;
			base.Multiline = true;
			document.CRLFSize = 1;

			scrollbars = RichTextBoxScrollBars.Both;
			alignment = HorizontalAlignment.Left;
			this.LostFocus += new EventHandler(RichTextBox_LostFocus);
			this.GotFocus += new EventHandler(RichTextBox_GotFocus);
			this.BackColor = ThemeEngine.Current.ColorWindow;
			this.ForeColor = ThemeEngine.Current.ColorWindowText;

			Console.WriteLine("A friendly request: Do not log a bug about debug messages being emitted when\n" +
				"using RichTextBox. It's not yet finished, it will spew debug information, and\n" +
				"it may not work the way you like it just yet. Some methods also are also not yet\n" + 
				"implemented. And we're also aware that text gets bolder with every change.");
			Console.WriteLine("To quote Sean Gilkes: Patience is a virtue, waiting doesn't hurt you :-)");
		}
		#endregion	// Public Constructors

		#region Private & Internal Methods
		private void RichTextBox_LostFocus(object sender, EventArgs e) {
			has_focus = false;
			Invalidate();
		}

		private void RichTextBox_GotFocus(object sender, EventArgs e) {
			has_focus = true;
			Invalidate();
		}
		#endregion	// Private & Internal Methods

		#region Public Instance Properties
		public override bool AllowDrop {
			get {
				return base.AllowDrop;
			}

			set {
				base.AllowDrop = value;
			}
		}

		[DefaultValue(false)]
		public override bool AutoSize {
			get {
				return auto_size;
			}

			set {
				base.AutoSize = value;
			}
		}

		[DefaultValue(false)]
		public bool AutoWordSelection {
			get {
				return auto_word_select;
			}

			set {
				auto_word_select = true;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override System.Drawing.Image BackgroundImage {
			get {
				return background_image;
			}

			set {
				base.BackgroundImage = value;
			}
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
				return can_redo;
			}
		}

		[DefaultValue(true)]
		public bool DetectUrls {
			get {
				return detect_urls;
			}

			set {
				detect_urls = true;
			}
		}

		public override Font Font {
			get {
				return base.Font;
			}

			set {
				if (font != value) {
					if (auto_size) {
						if (PreferredHeight != Height) {
							Height = PreferredHeight;
						}
					}

					base.Font = value;
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

		[DefaultValue(Int32.MaxValue)]
		public override int MaxLength {
			get {
				return base.max_length;
			}

			set {
				base.max_length = value;
			}
		}

		[DefaultValue(true)]
		public override bool Multiline {
			get {
				return multiline;
			}

			set {
				base.Multiline = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO]
		public string RedoActionName {
			get {
				return redo_action_name;
			}
		}

		[DefaultValue(0)]
		[Localizable(true)]
		[MonoTODO("Teach TextControl.RecalculateLine to consider the right margin as well")]
		public int RightMargin {
			get {
				return margin_right;
			}

			set {
				margin_right = value;
			}
		}

		[Browsable(false)]
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO("finish and plug in the rtf parser/generator")]
		public string Rtf {
			get {
				// FIXME
				return null;
			}

			set {
				// FIXME
			}
		}

		[DefaultValue(RichTextBoxScrollBars.Both)]
		[Localizable(true)]
		public RichTextBoxScrollBars ScrollBars {
			get {
				return scrollbars;
			}

			set {
				scrollbars = value;
			}
		}

		[MonoTODO("finish and plug in rtf parser/generator")]
		public string SelectedRtf {
			get {
				// FIXME
				return null;
			}

			set {
				// FIXME
			}
		}

		public override string SelectedText {
			get {
				return base.SelectedText;
			}

			set {
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


		public Font SelectionFont {
			get {
				Font	font;
				LineTag	start;
				LineTag	end;
				LineTag	tag;

				start = document.selection_start.tag;
				end = document.selection_end.tag;
				font = document.selection_start.tag.font;

				tag = start;
				while (true) {
					if (!font.Equals(tag.font)) {
						return null;
					}

					if (tag == end) {
						break;
					}

					tag = document.NextTag(tag);

					if (tag == null) {
						break;
					}
				}

				return font;
			}

			set {
				int	sel_start;
				int	sel_end;

				sel_start = document.LineTagToCharIndex(document.selection_start.line, document.selection_start.pos);
				sel_end = document.LineTagToCharIndex(document.selection_end.line, document.selection_end.pos);

				document.FormatText(document.selection_start.line, document.selection_start.pos + 1, document.selection_end.line, document.selection_end.pos, value, document.selection_start.tag.color);

				document.CharIndexToLineTag(sel_start, out document.selection_start.line, out document.selection_start.tag, out document.selection_start.pos);
				document.CharIndexToLineTag(sel_end, out document.selection_end.line, out document.selection_end.tag, out document.selection_end.pos);

				document.UpdateView(document.selection_start.line, 0);
				document.AlignCaret();
				
			}
		}

		[Localizable(true)]
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
				return undo_action_name;
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
		public void LoadFile(System.IO.Stream data, RichTextBoxStreamType fileType) {
			RTF.RTF rtf;	// Not 'using SWF.RTF' to avoid ambiguities with font and color

			document.Empty();

			// FIXME - ignoring unicode
			if (fileType == RichTextBoxStreamType.PlainText) {
				StringBuilder   sb;
				int             count;
				byte[]          buffer;

				try {
					sb = new StringBuilder((int)data.Length);
					buffer = new byte[1024];
				}

				catch {
					throw new IOException("Not enough memory to load document");
				}

				count = 0;
				while (count < data.Length) {
					count += data.Read(buffer, count, 1024);
					sb.Append(buffer);
				}
				base.Text = sb.ToString();
				return;
			}


			rtf = new RTF.RTF(data);

			// Prepare
			rtf.ClassCallback[RTF.TokenClass.Text] = new RTF.ClassDelegate(HandleText);
			rtf.ClassCallback[RTF.TokenClass.Control] = new RTF.ClassDelegate(HandleControl);

			rtf_skip_width = 0;
			rtf_skip_count = 0;
			rtf_line = new StringBuilder();
			rtf_font = Font;
			rtf_color = new SolidBrush(ForeColor);
			rtf_rtffont_size = this.Font.Height;
			rtf_rtfalign = HorizontalAlignment.Left;
			rtf_rtffont = null;
			rtf_cursor_x = 0;
			rtf_cursor_y = 1;

			rtf_text_map = new RTF.TextMap();
			RTF.TextMap.SetupStandardTable(rtf_text_map.Table);

			rtf.Read();	// That's it
			document.RecalculateDocument(CreateGraphics());
		}

		public void LoadFile(string path) {
			if (path.EndsWith(".rtf")) {
				LoadFile(path, RichTextBoxStreamType.RichText);
			} else {
				LoadFile(path, RichTextBoxStreamType.PlainText);
			}
		}

		public void LoadFile(string path, RichTextBoxStreamType fileType) {
			FileStream	data;

			data = null;

//			try {
				data = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
				LoadFile(data, fileType);
//			}

//			catch {
//				throw new IOException("Could not open file " + path);
//			}

//			finally {
				if (data != null) {
					data.Close();
//				}
			}
		}

		public void SaveFile(Stream data, RichTextBoxStreamType fileType) {
			#if later
			Encoding	encoding;

			if (fileType == RichTextBoxStreamType.UnicodePlainText) {
				encoding = Encoding.Unicode;
			} else {
				encoding = Encoding.ASCII;
			}
			#endif
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

			try {
				data = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1024, false);
				SaveFile(data, fileType);
			}

			catch {
				throw new IOException("Could not write document to file " + path);
			}

			finally {
				if (data != null) {
					data.Close();
				}
			}
		}

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void OnBackColorChanged(EventArgs e) {
			base.OnBackColorChanged (e);
		}

		protected override void OnContextMenuChanged(EventArgs e) {
			base.OnContextMenuChanged (e);
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed(EventArgs e) {
			base.OnHandleDestroyed (e);
		}

		protected override void OnRightToLeftChanged(EventArgs e) {
			base.OnRightToLeftChanged (e);
		}

		protected override void OnSystemColorsChanged(EventArgs e) {
			base.OnSystemColorsChanged (e);
		}

		protected override void OnTextChanged(EventArgs e) {
			base.OnTextChanged (e);
		}

		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
		}
		#endregion	// Protected Instance Methods

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler			BackgroundImageChanged;

		public event ContentsResizedEventHandler	ContentsResized;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler			DoubleClick;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event DragEventHandler			DragDrop;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event DragEventHandler			DragEnter;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler			DragLeave;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event DragEventHandler			DragOver;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event GiveFeedbackEventHandler		GiveFeedback;

		public event EventHandler			HScroll;
		public event EventHandler			ImeChange;
		public event LinkClickedEventHandler		LinkClicked;
		public event EventHandler			Protected;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event QueryContinueDragEventHandler	QueryContinueDrag;
		public event EventHandler			SelectionChanged;
		public event EventHandler			VScroll;
		#endregion	// Events

		#region Private Methods
		void HandleControl(RTF.RTF rtf) {
			switch(rtf.Major) {
				case RTF.Major.Unicode: {
					switch(rtf.Minor) {
						case Minor.UnicodeCharBytes: {
							rtf_skip_width = rtf.Param;
							break;
						}

						case Minor.UnicodeChar: {
							rtf_skip_count += rtf_skip_width;
							rtf_line.Append((char)rtf.Param);
							break;
						}
					}
					break;
				}

				case RTF.Major.Destination: {
					Console.Write("[Got Destination control {0}]", rtf.Minor);
					rtf.SkipGroup();
					break;
				}

				case RTF.Major.CharAttr: {
					switch(rtf.Minor) {
						case Minor.ForeColor: {
							System.Windows.Forms.RTF.Color	color;

							color = System.Windows.Forms.RTF.Color.GetColor(rtf, rtf.Param);
							if (color != null) {
								FlushText(false);
								if (color.Red == -1 && color.Green == -1 && color.Blue == -1) {
									this.rtf_color = new SolidBrush(ForeColor);
								} else {
									this.rtf_color = new SolidBrush(Color.FromArgb(color.Red, color.Green, color.Blue));
								}
							}
							break;
						}

						case Minor.FontSize: {
							this.rtf_rtffont_size = rtf.Param / 2;
							break;
						}

						case Minor.FontNum: {
							System.Windows.Forms.RTF.Font	font;

							font = System.Windows.Forms.RTF.Font.GetFont(rtf, rtf.Param);
							if (font != null) {
								FlushText(false);
								this.rtf_rtffont = font;
							}
							break;
						}

						case Minor.Plain: {
							FlushText(false);
							rtf_rtfstyle = FontStyle.Regular;
							break;
						}

						case Minor.Bold: {
							FlushText(false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_rtfstyle |= FontStyle.Bold;
							} else {
								rtf_rtfstyle &= ~FontStyle.Bold;
							}
							break;
						}

						case Minor.Italic: {
							FlushText(false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_rtfstyle |= FontStyle.Italic;
							} else {
								rtf_rtfstyle &= ~FontStyle.Italic;
							}
							break;
						}

						case Minor.StrikeThru: {
							FlushText(false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_rtfstyle |= FontStyle.Strikeout;
							} else {
								rtf_rtfstyle &= ~FontStyle.Strikeout;
							}
							break;
						}

						case Minor.Underline: {
							FlushText(false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_rtfstyle |= FontStyle.Underline;
							} else {
								rtf_rtfstyle &= ~FontStyle.Underline;
							}
							break;
						}

						case Minor.NoUnderline: {
							FlushText(false);
							rtf_rtfstyle &= ~FontStyle.Underline;
							break;
						}
					}
					break;
				}

				case RTF.Major.SpecialChar: {
					Console.Write("[Got SpecialChar control {0}]", rtf.Minor);
					SpecialChar(rtf);
					break;
				}
			}
		}

		void SpecialChar(RTF.RTF rtf) {
			switch(rtf.Minor) {
				case Minor.Page:
				case Minor.Sect:
				case Minor.Row:
				case Minor.Line:
				case Minor.Par: {
					FlushText(true);
					break;
				}

				case Minor.Cell: {
					Console.Write(" ");
					break;
				}

				case Minor.NoBrkSpace: {
					Console.Write(" ");
					break;
				}

				case Minor.Tab: {
					Console.Write("\t");
					break;
				}

				case Minor.NoBrkHyphen: {
					Console.Write("-");
					break;
				}

				case Minor.Bullet: {
					Console.Write("*");
					break;
				}

				case Minor.EmDash: {
					Console.Write("—");
					break;
				}

				case Minor.EnDash: {
					Console.Write("–");
					break;
				}

				case Minor.LQuote: {
					Console.Write("‘");
					break;
				}

				case Minor.RQuote: {
					Console.Write("’");
					break;
				}

				case Minor.LDblQuote: {
					Console.Write("“");
					break;
				}

				case Minor.RDblQuote: {
					Console.Write("”");
					break;
				}

				default: {
					rtf.SkipGroup();
					break;
				}
			}
		}


		void HandleText(RTF.RTF rtf) {
			if (rtf_skip_count > 0) {
				rtf_skip_count--;
				return;
			}

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
		}

		void FlushText(bool newline) {
			int		length;
			Font		font;

			length = rtf_line.Length;
			if (length == 0) {
				return;
			}

			if (rtf_rtffont != null) {
				font = new Font(rtf_rtffont.Name, rtf_rtffont_size, rtf_rtfstyle);
			} else {
				font = this.Font;
			}

			if (rtf_cursor_x == 0) {
				document.Add(rtf_cursor_y, rtf_line.ToString(), rtf_rtfalign, font, rtf_color);
			} else {
				Line	line;

				line = document.GetLine(rtf_cursor_y);
				document.InsertString(line, rtf_cursor_x, rtf_line.ToString());
				document.FormatText(line, rtf_cursor_x, line, rtf_cursor_x + length, font, rtf_color);
			}

			if (newline) {
				rtf_cursor_x = 0;
				rtf_cursor_y++;
			} else {
				rtf_cursor_x += length;
			}
			rtf_line.Length = 0;	// Empty line
		}
		#endregion	// Private Methods
	}
}
