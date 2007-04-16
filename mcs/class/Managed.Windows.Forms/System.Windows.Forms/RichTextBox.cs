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
//
//

// #define DEBUG

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using RTF=System.Windows.Forms.RTF;

namespace System.Windows.Forms {
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[Docking (DockingBehavior.Ask)]
	[ComVisible (true)]
#endif
	public class RichTextBox : TextBoxBase {
		#region Local Variables
		internal bool		auto_word_select;
		internal int		bullet_indent;
		internal bool		detect_urls;
		internal int		margin_right;
		internal float		zoom;

		private RTF.TextMap	rtf_text_map;
		private int		rtf_skip_width;
		private int		rtf_skip_count;
		private StringBuilder	rtf_line;
		private SolidBrush	rtf_color;
		private RTF.Font	rtf_rtffont;
		private int		rtf_rtffont_size;
		private FontStyle	rtf_rtfstyle;
		private HorizontalAlignment rtf_rtfalign;
		private int		rtf_cursor_x;
		private int		rtf_cursor_y;
		private int		rtf_chars;
		private int rtf_par_line_left_indent;
		#endregion	// Local Variables

		#region Public Constructors
		public RichTextBox() {
			accepts_return = true;
			auto_word_select = false;
			bullet_indent = 0;
			detect_urls = true;
			max_length = Int32.MaxValue;
			margin_right = 0;
			zoom = 1;
			base.Multiline = true;
			document.CRLFSize = 1;
			shortcuts_enabled = true;

			scrollbars = RichTextBoxScrollBars.Both;
			alignment = HorizontalAlignment.Left;
			LostFocus += new EventHandler(RichTextBox_LostFocus);
			GotFocus += new EventHandler(RichTextBox_GotFocus);
			BackColor = ThemeEngine.Current.ColorWindow;
#if NET_2_0
			backcolor_set = false;
#endif
			ForeColor = ThemeEngine.Current.ColorWindowText;

			base.HScrolled += new EventHandler(RichTextBox_HScrolled);
			base.VScrolled += new EventHandler(RichTextBox_VScrolled);

#if NET_2_0
			SetStyle (ControlStyles.StandardDoubleClick, false);
#endif
		}
		#endregion	// Public Constructors

		#region Private & Internal Methods
		internal override Color ChangeBackColor (Color backColor)
		{
			if (backColor == Color.Empty) {
#if NET_2_0
				backcolor_set = false;
				if (!ReadOnly) {
					backColor = SystemColors.Window;
				}
#else
				backColor = SystemColors.Window;
#endif
			}
			return backColor;
		}

		private void RichTextBox_LostFocus(object sender, EventArgs e) {
			Invalidate();
		}

		private void RichTextBox_GotFocus(object sender, EventArgs e) {
			Invalidate();
		}
		#endregion	// Private & Internal Methods

		#region Public Instance Properties
#if NET_2_0
		[Browsable (false)]
#endif
		public override bool AllowDrop {
			get {
				return base.AllowDrop;
			}

			set {
				base.AllowDrop = value;
			}
		}

		[DefaultValue(false)]
#if NET_2_0
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
#else
		[Localizable(true)]
#endif
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
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
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
					document.FormatText(start, 1, end, end.text.Length + 1, base.Font, null, null, FormatSpecified.Font);
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
				return base.Multiline;
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
				return document.undo.RedoActionName;
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
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		[RefreshProperties (RefreshProperties.All)]
#else
		[DefaultValue("")]
#endif
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

				document.Empty();
				data = new MemoryStream(Encoding.ASCII.GetBytes(value), false);

				InsertRTFFromStream(data, 0, 1);

				data.Close();

				Invalidate();
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

				scrollbars = value;
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

				if (document.selection_visible) {
					document.ReplaceSelection("", false);
				}

				sel_start = document.LineTagToCharIndex(document.selection_start.line, document.selection_start.pos);

				data = new MemoryStream(Encoding.ASCII.GetBytes(value), false);
				InsertRTFFromStream(data, document.selection_start.pos, document.selection_start.line.line_no, out x, out y, out chars);
				data.Close();

				document.CharIndexToLineTag(sel_start + chars + (y - document.selection_start.line.line_no) * 2, out line, out tag, out sel_start);
				document.SetSelection(line, sel_start);
				document.PositionCaret(line, sel_start);
				document.DisplayCaret();
				ScrollToCaret();
				OnTextChanged(EventArgs.Empty);
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

		[Browsable(false)]
		[DefaultValue(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO]
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
		[MonoTODO]
		public int SelectionCharOffset {
			get {
				return 0;
			}

			set {
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
				end = document.selection_start.line.FindTag (document.selection_end.pos);
				color = start.color.Color;

				tag = start;
				while (tag != null && tag != end) {

					if (!color.Equals (tag.color.Color))
						return Color.Empty;

					tag = document.NextTag (tag);
				}

				return color;
			}

			set {
				int sel_start;
				int sel_end;

				sel_start = document.LineTagToCharIndex(document.selection_start.line, document.selection_start.pos);
				sel_end = document.LineTagToCharIndex(document.selection_end.line, document.selection_end.pos);

				document.FormatText (document.selection_start.line, document.selection_start.pos + 1,
						document.selection_end.line, document.selection_end.pos + 1, null,
						new SolidBrush (value),	null, FormatSpecified.Color);

				document.CharIndexToLineTag(sel_start, out document.selection_start.line, out document.selection_start.tag, out document.selection_start.pos);
				document.CharIndexToLineTag(sel_end, out document.selection_end.line, out document.selection_end.tag, out document.selection_end.pos);

				document.UpdateView(document.selection_start.line, 0);
				document.AlignCaret();
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
				end = document.selection_start.line.FindTag (document.selection_end.pos);
				font = start.font;

				if (selection_length > 1) {
					tag = start;
					while (tag != null && tag != end) {

						if (!font.Equals(tag.font))
							return null;

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
						null, null, FormatSpecified.Font);

				document.CharIndexToLineTag(sel_start, out document.selection_start.line, out document.selection_start.tag, out document.selection_start.pos);
				document.CharIndexToLineTag(sel_end, out document.selection_end.line, out document.selection_end.tag, out document.selection_end.pos);

				document.UpdateView(document.selection_start.line, 0);
				document.AlignCaret();

			}
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO]
		public int SelectionHangingIndent {
			get {
				return 0;
			}

			set {
			}
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO]
		public int SelectionIndent {
			get {
				return 0;
			}

			set {
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
		[MonoTODO]
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
		[MonoTODO]
		public int SelectionRightIndent {
			get {
				return 0;
			}

			set {
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonoTODO]
		public int[] SelectionTabs {
			get {
				return new int[0];
			}

			set {
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
		[MonoTODO]
		public bool ShowSelectionMargin {
			get {
				return false;
			}

			set {
			}
		}

		[Localizable(true)]
#if NET_2_0
		[RefreshProperties (RefreshProperties.All)]
#endif
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

		public char GetCharFromPosition(Point pt) {
			LineTag	tag;
			int	pos;

			PointToTagPos(pt, out tag, out pos);

			if (pos >= tag.line.text.Length) {
				return '\n';
			}

			return tag.line.text[pos];
			
		}

		public int GetCharIndexFromPosition(Point pt) {
			LineTag	tag;
			int	pos;

			PointToTagPos(pt, out tag, out pos);

			return document.LineTagToCharIndex(tag.line, pos);
		}

		public int GetLineFromCharIndex(int index) {
			Line	line;
			LineTag	tag;
			int	pos;

			document.CharIndexToLineTag(index, out line, out tag, out pos);

			return line.LineNo - 1;
		}

		public Point GetPositionFromCharIndex(int index) {
			Line	line;
			LineTag	tag;
			int	pos;

			document.CharIndexToLineTag(index, out line, out tag, out pos);

			return new Point((int)line.widths[pos] + 1, line.Y + 1);
		}

		public void LoadFile(System.IO.Stream data, RichTextBoxStreamType fileType) {
			document.Empty();

			
			// FIXME - ignoring unicode
			if (fileType == RichTextBoxStreamType.PlainText) {
				StringBuilder   sb;
				int             count;
				byte[]          buffer;

				try {
					sb = new StringBuilder((int)data.Length);
					buffer = new byte[1024];
				} catch {
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

			InsertRTFFromStream(data, 0, 1);

			document.PositionCaret (document.GetLine (1), 0);
			document.SetSelectionToCaret (true);
			ScrollToCaret ();
		}

		[MonoTODO("Make smarter RTF detection?")]
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


			try {
				data = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024);

				LoadFile(data, fileType);
			}
#if !DEBUG
			catch {
				throw new IOException("Could not open file " + path);
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
			document.undo.Redo ();
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
						bytes = encoding.GetBytes(document.GetLine(i).text.ToString() + Environment.NewLine);
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

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual object CreateRichEditOleCallback() {
			throw new NotImplementedException();
		}

		protected override void OnBackColorChanged(EventArgs e) {
			base.OnBackColorChanged (e);
		}

		protected virtual void OnContentsResized(ContentsResizedEventArgs e) {
			ContentsResizedEventHandler eh = (ContentsResizedEventHandler)(Events [ContentsResizedEvent]);
			if (eh != null)
				eh (this, e);
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

		protected virtual void OnHScroll(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [HScrollEvent]);
			if (eh != null)
				eh (this, e);
		}

		[MonoTODO("Determine when to call this")]
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

		protected override void OnSystemColorsChanged(EventArgs e) {
			base.OnSystemColorsChanged (e);
		}

		protected override void OnTextChanged(EventArgs e) {
			base.OnTextChanged (e);
		}

		protected virtual void OnVScroll(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [VScrollEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
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

		public event ContentsResizedEventHandler ContentsResized {
			add { Events.AddHandler (ContentsResizedEvent, value); }
			remove { Events.RemoveHandler (ContentsResizedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add { base.DoubleClick += value; }
			remove { base.DoubleClick -= value; }
		}

		[Browsable(false)]
#if !NET_2_0
		[EditorBrowsable(EditorBrowsableState.Never)]
#endif
		public new event DragEventHandler DragDrop {
			add { base.DragDrop += value; }
			remove { base.DragDrop -= value; }
		}

		[Browsable(false)]
#if !NET_2_0
		[EditorBrowsable(EditorBrowsableState.Never)]
#endif
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

		private void HandleControl(RTF.RTF rtf) {
			switch(rtf.Major) {
				case RTF.Major.Unicode: {
					switch(rtf.Minor) {
						case RTF.Minor.UnicodeCharBytes: {
							rtf_skip_width = rtf.Param;
							break;
						}

						case RTF.Minor.UnicodeChar: {
							rtf_skip_count += rtf_skip_width;
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
						Line line = document.GetLine (rtf_cursor_y);
						document.InsertPicture (line, 0, rtf.Picture);
						rtf_cursor_x++;

						FlushText (rtf, true);
						rtf.Picture = null;
					}
					break;

				case RTF.Major.CharAttr: {
					switch(rtf.Minor) {
						case RTF.Minor.ForeColor: {
							System.Windows.Forms.RTF.Color	color;

							color = System.Windows.Forms.RTF.Color.GetColor(rtf, rtf.Param);
							
							if (color != null) {
								FlushText(rtf, false);
								if (color.Red == -1 && color.Green == -1 && color.Blue == -1) {
									this.rtf_color = new SolidBrush(ForeColor);
								} else {
									this.rtf_color = new SolidBrush(Color.FromArgb(color.Red, color.Green, color.Blue));
								}
								FlushText (rtf, false);
							}
							break;
						}

						case RTF.Minor.FontSize: {
							FlushText(rtf, false);
							this.rtf_rtffont_size = rtf.Param / 2;
							break;
						}

						case RTF.Minor.FontNum: {
							System.Windows.Forms.RTF.Font	font;

							font = System.Windows.Forms.RTF.Font.GetFont(rtf, rtf.Param);
							if (font != null) {
								FlushText(rtf, false);
								this.rtf_rtffont = font;
							}
							break;
						}

						case RTF.Minor.Plain: {
							FlushText(rtf, false);
							rtf_rtfstyle = FontStyle.Regular;
							break;
						}

						case RTF.Minor.Bold: {
							FlushText(rtf, false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_rtfstyle |= FontStyle.Bold;
							} else {
								rtf_rtfstyle &= ~FontStyle.Bold;
							}
							break;
						}

						case RTF.Minor.Italic: {
							FlushText(rtf, false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_rtfstyle |= FontStyle.Italic;
							} else {
								rtf_rtfstyle &= ~FontStyle.Italic;
							}
							break;
						}

						case RTF.Minor.StrikeThru: {
							FlushText(rtf, false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_rtfstyle |= FontStyle.Strikeout;
							} else {
								rtf_rtfstyle &= ~FontStyle.Strikeout;
							}
							break;
						}

						case RTF.Minor.Underline: {
							FlushText(rtf, false);
							if (rtf.Param == RTF.RTF.NoParam) {
								rtf_rtfstyle |= FontStyle.Underline;
							} else {
								rtf_rtfstyle = rtf_rtfstyle & ~FontStyle.Underline;
							}
							break;
						}

						case RTF.Minor.NoUnderline: {
							FlushText(rtf, false);
							rtf_rtfstyle &= ~FontStyle.Underline;
							break;
						}
					}
					break;
				}

			case RTF.Major.ParAttr: {
				switch (rtf.Minor) {
				case RTF.Minor.LeftIndent:
					rtf_par_line_left_indent = (int) (((float) rtf.Param / 1440.0F) * CreateGraphics ().DpiX + 0.5F);
					break;
				}
				break;
			}
				
				case RTF.Major.SpecialChar: {
					//Console.Write("[Got SpecialChar control {0}]", rtf.Minor);
					SpecialChar(rtf);
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
					FlushText(rtf, true);
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
/*
				case RTF.Minor.LQuote: {
					Console.Write("\u2018");
					break;
				}

				case RTF.Minor.RQuote: {
					Console.Write("\u2019");
					break;
				}

				case RTF.Minor.LDblQuote: {
					Console.Write("\u201C");
					break;
				}

				case RTF.Minor.RDblQuote: {
					Console.Write("\u201D");
					break;
				}
*/
				default: {
//					Console.WriteLine ("skipped special char:   {0}", rtf.Minor);
//					rtf.SkipGroup();
					break;
				}
			}
		}

		private void HandleText(RTF.RTF rtf) {
			if (rtf_skip_count > 0) {
				rtf_skip_count--;
				return;
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
			rtf_line.Append (rtf.EncodedText);
		}

		private void FlushText(RTF.RTF rtf, bool newline) {
			int		length;
			Font		font;

			length = rtf_line.Length;
			if (!newline && (length == 0)) {
				return;
			}

			if (rtf_rtffont == null) {
				// First font in table is default
				rtf_rtffont = System.Windows.Forms.RTF.Font.GetFont(rtf, 0);
			}

			font = new Font(rtf_rtffont.Name, rtf_rtffont_size, rtf_rtfstyle);

			if (rtf_color == null) {
				System.Windows.Forms.RTF.Color color;

				// First color in table is default
				color = System.Windows.Forms.RTF.Color.GetColor(rtf, 0);

				if ((color == null) || (color.Red == -1 && color.Green == -1 && color.Blue == -1)) {
					rtf_color = new SolidBrush(ForeColor);
				} else {
					rtf_color = new SolidBrush(Color.FromArgb(color.Red, color.Green, color.Blue));
				}
				
			}

			rtf_chars += rtf_line.Length;

			if (rtf_cursor_x == 0) {
				document.Add(rtf_cursor_y, rtf_line.ToString(), rtf_rtfalign, font, rtf_color, LineEnding.Wrap);
				if (rtf_par_line_left_indent != 0) {
					Line line = document.GetLine (rtf_cursor_y);
					line.indent = rtf_par_line_left_indent;
				}
			} else {
				Line	line;

				line = document.GetLine(rtf_cursor_y);
				line.indent = rtf_par_line_left_indent;
				if (rtf_line.Length > 0) {
					document.InsertString(line, rtf_cursor_x, rtf_line.ToString());
					document.FormatText(line, rtf_cursor_x + 1, line, rtf_cursor_x + 1 + length, font, rtf_color, null, FormatSpecified.Font | FormatSpecified.Color); // FormatText is 1-based
				}
				if (newline) {
					document.Split(line, rtf_cursor_x + length);
				}
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

			rtf_skip_width = 0;
			rtf_skip_count = 0;
			rtf_line = new StringBuilder();
			rtf_color = null;
			rtf_rtffont_size = (int)this.Font.Size;
			rtf_rtfalign = HorizontalAlignment.Left;
			rtf_rtfstyle = FontStyle.Regular;
			rtf_rtffont = null;
			rtf_cursor_x = cursor_x;
			rtf_cursor_y = cursor_y;
			rtf_chars = 0;
			rtf.DefaultFont(this.Font.Name);

			rtf_text_map = new RTF.TextMap();
			RTF.TextMap.SetupStandardTable(rtf_text_map.Table);

			document.SuspendRecalc ();

			try {
				rtf.Read();	// That's it
				FlushText(rtf, false);

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

			document.RecalculateDocument(CreateGraphicsInternal(), cursor_y, document.Lines, false);
			document.ResumeRecalc (true);

			document.Invalidate (document.GetLine(cursor_y), 0, document.GetLine(document.Lines), -1);
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

			if ((prev_font == null) || (prev_font.Size != font.Size)) {
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

		[MonoTODO("Emit unicode and other special characters properly")]
		private void EmitRTFText(StringBuilder rtf, string text) {
			rtf.Append(text);
		}

		// start_pos and end_pos are 0-based
		private StringBuilder GenerateRTF(Line start_line, int start_pos, Line end_line, int end_pos) {
			StringBuilder	sb;
			ArrayList	fonts;
			ArrayList	colors;
			Color		color;
			Font		font;
			Line		line;
			LineTag		tag;
			int		pos;
			int		line_no;
			int		line_len;
			int		i;
			int		length;

			sb = new StringBuilder();
			fonts = new ArrayList(10);
			colors = new ArrayList(10);

			// Two runs, first we parse to determine tables;
			// and unlike most of our processing here we work on tags

			line = start_line;
			line_no = start_line.line_no;
			pos = start_pos;

			// Add default font and color; to optimize document content we don't
			// use this.Font and this.ForeColor but the font/color from the first tag
			tag = LineTag.FindTag(start_line, pos);
			font = tag.font;
			color = ((SolidBrush)tag.color).Color;
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
					if (tag.font.Name != font.Name) {
						font = tag.font;
						if (!fonts.Contains(font.Name)) {
							fonts.Add(font.Name);
						}
					}

					if (((SolidBrush)tag.color).Color != color) {
						color = ((SolidBrush)tag.color).Color;
						if (!colors.Contains(color)) {
							colors.Add(color);
						}
					}

					pos = tag.start + tag.length - 1;
					tag = tag.next;
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
			sb.Append("\\deflang1033\n");	// FIXME - always 1033?

			// Emit the font table
			sb.Append("{\\fonttbl");
			for (i = 0; i < fonts.Count; i++) {
				sb.Append(String.Format("{{\\f{0}", i));	// {Font 
				sb.Append("\\fnil");			// Family
				sb.Append("\\fcharset0 ");		// Charset ANSI<space>
				sb.Append((string)fonts[i]);		// Font name
				sb.Append(";}");			// }
			}
			sb.Append("}\n");

			// Emit the color table (if needed)
			if ((colors.Count > 1) || ((((Color)colors[0]).R != this.ForeColor.R) || (((Color)colors[0]).G != this.ForeColor.G) || (((Color)colors[0]).B != this.ForeColor.B))) {
				sb.Append("{\\colortbl ");			// Header and NO! default color
				for (i = 0; i < colors.Count; i++) {
					sb.Append(String.Format("\\red{0}", ((Color)colors[i]).R));
					sb.Append(String.Format("\\green{0}", ((Color)colors[i]).G));
					sb.Append(String.Format("\\blue{0}", ((Color)colors[i]).B));
					sb.Append(";");
				}
				sb.Append("}\n");
			}

			sb.Append("{\\*\\generator Mono RichTextBox;}");
			// Emit initial paragraph settings
			tag = LineTag.FindTag(start_line, start_pos);
			sb.Append("\\pard");	// Reset to default paragraph properties
			EmitRTFFontProperties(sb, -1, fonts.IndexOf(tag.font.Name), null, tag.font);	// Font properties
			sb.Append(" ");		// Space separator

			font = tag.font;
			color = (Color)colors[0];
			line = start_line;
			line_no = start_line.line_no;
			pos = start_pos;

			while (line_no <= end_line.line_no) {
				line = document.GetLine(line_no);
				tag = LineTag.FindTag(line, pos);

				if (line_no != end_line.line_no) {
					line_len = line.text.Length;
				} else {
					line_len = end_pos;
				}

				while (pos < line_len) {
					length = sb.Length;

					if (tag.font != font) {
						EmitRTFFontProperties(sb, fonts.IndexOf(font.Name), fonts.IndexOf(tag.font.Name), font, tag.font);
						font = tag.font;
					}

					if (((SolidBrush)tag.color).Color != color) {
						color = ((SolidBrush)tag.color).Color;
						sb.Append(String.Format("\\cf{0}", colors.IndexOf(color)));
					}
					if (length != sb.Length) {
						sb.Append(" ");	// Emit space to separate keywords from text
					}

					// Emit the string itself
					if (line_no != end_line.line_no) {
						EmitRTFText(sb, tag.line.text.ToString(pos, tag.start + tag.length - pos - 1));
					} else {
						if (end_pos < (tag.start + tag.length - 1)) {
							// Emit partial tag only, end_pos is inside this tag
							EmitRTFText(sb, tag.line.text.ToString(pos, end_pos - pos));
						} else {
							EmitRTFText(sb, tag.line.text.ToString(pos, tag.start + tag.length - pos - 1));
						}
					}

					pos = tag.start + tag.length - 1;
					tag = tag.next;
				}
				if (pos >= line.text.Length) {
					if (line.ending != LineEnding.Wrap) {
						sb.Append("\\par\n");
					}
				}
				pos = 0;
				line_no++;
			}

			sb.Append("}\n");

			return sb;
		}
		#endregion	// Private Methods
	}
}
