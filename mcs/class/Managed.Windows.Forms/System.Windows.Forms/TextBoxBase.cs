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
#define Debug

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DefaultEvent("TextChanged")]
	[Designer("System.Windows.Forms.Design.TextBoxBaseDesigner, " + Consts.AssemblySystem_Design)]
	public abstract class TextBoxBase : Control {
		#region Local Variables
		internal HorizontalAlignment	alignment;
		internal bool			accepts_tab;
		internal bool			accepts_return;
		internal bool			auto_size;
		internal CharacterCasing	character_casing;
		internal bool			undo;
		internal bool			hide_selection;
		internal int			max_length;
		internal bool			modified;
		internal bool			multiline;
		internal bool			read_only;
		internal bool			word_wrap;
		internal Document		document;
		internal LineTag		caret_tag;		// tag our cursor is in
		internal int			caret_pos;		// position on the line our cursor is in (can be 0 = beginning of line)
		internal int			viewport_x;		// left visible pixel
		internal int			viewport_y;		// top visible pixel
		internal HScrollBar		hscroll;
		internal VScrollBar		vscroll;
		internal ScrollBars		scrollbars;
		internal bool			grabbed;
		internal bool			richtext;
		internal int			requested_height;

		#if Debug
		internal static bool	draw_lines = false;
		#endif

		#endregion	// Local Variables

		#region Internal Constructor
		// Constructor will go when complete, only for testing - pdb
		internal TextBoxBase() {
			alignment = HorizontalAlignment.Left;
			accepts_return = false;
			accepts_tab = false;
			auto_size = true;
			border_style = BorderStyle.Fixed3D;
			character_casing = CharacterCasing.Normal;
			undo = false;
			hide_selection = true;
			max_length = 32767;
			modified = false;
			multiline = false;
			read_only = false;
			word_wrap = true;
			richtext = false;
			document = new Document(this);
			requested_height = -1;

			MouseDown += new MouseEventHandler(TextBoxBase_MouseDown);
			MouseUp += new MouseEventHandler(TextBoxBase_MouseUp);
			MouseMove += new MouseEventHandler(TextBoxBase_MouseMove);
			SizeChanged += new EventHandler(TextBoxBase_SizeChanged);
			FontChanged += new EventHandler(TextBoxBase_FontOrColorChanged);
			ForeColorChanged += new EventHandler(TextBoxBase_FontOrColorChanged);
			
			scrollbars = ScrollBars.None;

			hscroll = new HScrollBar();
			hscroll.ValueChanged +=new EventHandler(hscroll_ValueChanged);
			hscroll.Enabled = true;
			hscroll.Visible = false;

			vscroll = new VScrollBar();
			vscroll.Visible = false;

			this.Controls.Add(hscroll);
			this.Controls.Add(vscroll);

			//SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
		}
		#endregion	// Internal Constructor

		#region Private and Internal Methods
		internal string CaseAdjust(string s) {
			if (character_casing == CharacterCasing.Normal) {
				return s;
			}
			if (character_casing == CharacterCasing.Lower) {
				return s.ToLower();
			} else {
				return s.ToUpper();
			}
		}
		#endregion	// Private and Internal Methods

		#region Public Instance Properties
		[DefaultValue(false)]
		public bool AcceptsTab {
			get {
				return accepts_tab;
			}

			set {
				if (value != accepts_tab) {
					accepts_tab = value;
					OnAcceptsTabChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public virtual bool AutoSize {
			get {
				return auto_size;
			}

			set {
				if (value != auto_size) {
					auto_size = value;
					if (auto_size) {
						if (PreferredHeight != Height) {
							Height = PreferredHeight;
						}
					}
					OnAutoSizeChanged(EventArgs.Empty);
				}
			}
		}

		[DispId(-501)]
		public override System.Drawing.Color BackColor {
			get {
				return base.BackColor;
			}
			set {
				base.BackColor = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override System.Drawing.Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
			}
		}

		[DefaultValue(BorderStyle.Fixed3D)]
		[DispId(-504)]
		public BorderStyle BorderStyle {
			get {
				return border_style;
			}

			set {
				if (value != border_style) {
					border_style = value;
					OnBorderStyleChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanUndo {
			get {
				return undo;
			}
		}

		[DispId(-513)]
		public override System.Drawing.Color ForeColor {
			get {
				return base.ForeColor;
			}
			set {
				base.ForeColor = value;
			}
		}

		[DefaultValue(true)]
		public bool HideSelection {
			get {
				return hide_selection;
			}

			set {
				if (value != hide_selection) {
					hide_selection = value;
					OnHideSelectionChanged(EventArgs.Empty);
				}
				if (hide_selection) {
					document.selection_visible = false;
				} else {
					document.selection_visible = true;
				}
				document.InvalidateSelectionArea();

			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Editor("System.Windows.Forms.Design.StringArrayEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		public string[] Lines {
			get {
				string[]	lines;
				int		i;
				int		l;

				l = document.Lines;
				lines = new string[l];

				for (i = 1; i <= l; i++) {
					lines[i - 1] = document.GetLine(i).text.ToString();
				}

				return lines;
			}

			set {
				int	i;
				int	l;
				Brush	brush;

				document.Empty();

				l = value.Length;
				brush = ThemeEngine.Current.ResPool.GetSolidBrush(this.ForeColor);

				for (i = 0; i < l; i++) {
					document.Add(i+1, CaseAdjust(value[i]), alignment, Font, brush);
				}
				document.RecalculateDocument(CreateGraphics());
				OnTextChanged(EventArgs.Empty);
			}
		}

		[DefaultValue(32767)]
		[Localizable(true)]
		public virtual int MaxLength {
			get {
				return max_length;
			}

			set {
				if (value != max_length) {
					max_length = value;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Modified {
			get {
				return modified;
			}

			set {
				if (value != modified) {
					modified = value;
					OnModifiedChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(false)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.All)]
		public virtual bool Multiline {
			get {
				return multiline;
			}

			set {
				if (value != multiline) {
					multiline = value;

					// Make sure we update our size; the user may have already set the size before going to multiline
					if (multiline && requested_height != -1) {
						Height = requested_height;
						requested_height = -1;
					}

					OnMultilineChanged(EventArgs.Empty);
				}

				document.multiline = multiline;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public int PreferredHeight {
			get {
				return this.Font.Height + 7;	// FIXME - consider border style as well
			}
		}

		[DefaultValue(false)]
		public bool ReadOnly {
			get {
				return read_only;
			}

			set {
				if (value != read_only) {
					read_only = value;
					OnReadOnlyChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual string SelectedText {
			get {
				return document.GetSelection();
			}

			set {
				document.ReplaceSelection(CaseAdjust(value));
				OnTextChanged(EventArgs.Empty);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual int SelectionLength {
			get {
				return document.SelectionLength();
			}

			set {
				if (value != 0) {
					int	start;
					Line	line;
					LineTag	tag;
					int	pos;

					start = document.LineTagToCharIndex(document.selection_start.line, document.selection_start.pos);

					document.CharIndexToLineTag(start + value, out line, out tag, out pos);
					document.SetSelectionEnd(line, pos);
					document.PositionCaret(line, pos);
				} else {
					document.SetSelectionEnd(document.selection_start.line, document.selection_start.pos);
					document.PositionCaret(document.selection_start.line, document.selection_start.pos);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionStart {
			get {
				int index;

				index = document.LineTagToCharIndex(document.selection_start.line, document.selection_start.pos);

				return index;
			}

			set {
				Line	line;
				LineTag	tag;
				int	pos;

				document.CharIndexToLineTag(value, out line, out tag, out pos);
				document.SetSelectionStart(line, pos);
			}
		}

		[Localizable(true)]
		public override string Text {
			get {
				if (document == null || document.Root == null || document.Root.text == null) {
					return string.Empty;
				}

				if (!multiline) {
					return document.Root.text.ToString();
				} else {
					StringBuilder	sb;
					int		i;

					sb = new StringBuilder();

					for (i = 1; i < document.Lines; i++) {
						sb.Append(document.GetLine(i).text.ToString() + Environment.NewLine);
					}

					return sb.ToString();
				}
			}

			set {
				Line	line;

				if (multiline) {
					string[]	lines;

					lines = value.Split(new char[] {'\n'});
					for (int i = 0; i < lines.Length; i++) {
						if (lines[i].EndsWith("\r")) {
							lines[i] = lines[i].Substring(0, lines[i].Length - 1);
						}
					}
					this.Lines = lines;

					line = document.GetLine(1);
					document.SetSelectionStart(line, 0);

					line = document.GetLine(document.Lines);
					document.SetSelectionEnd(line, line.text.Length);
					document.PositionCaret(line, line.text.Length);
				} else {
					document.Clear();
					document.Add(1, CaseAdjust(value), alignment, Font, ThemeEngine.Current.ResPool.GetSolidBrush(ForeColor));
					document.RecalculateDocument(CreateGraphics());
					line = document.GetLine(1);
					document.SetSelectionStart(line, 0);
					document.SetSelectionEnd(line, value.Length);
					document.PositionCaret(line, value.Length);
				}
				base.Text = value;
				OnTextChanged(EventArgs.Empty);
			}
		}

		[Browsable(false)]
		public virtual int TextLength {
			get {
				if (document == null || document.Root == null || document.Root.text == null) {
					return 0;
				}

				if (!multiline) {
					return document.Root.text.Length;
				} else {
					int	total;
					int	i;

					total = 0;
					for (i = 1; i < document.Lines; i++) {
						total += document.GetLine(i).text.Length + Environment.NewLine.Length;
					}

					return total;
				}
			}
		}

		[DefaultValue(true)]
		[Localizable(true)]
		public bool WordWrap {
			get {
				return word_wrap;
			}

			set {
				if (value != word_wrap) {
					word_wrap = value;
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override System.Drawing.Size DefaultSize {
			get {
				return base.DefaultSize;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void AppendText(string text) {

			if (multiline) {
				string[]	lines;
				int		linecount;

				// Break the string into separate lines
				lines = text.Split(new char[] {'\n'});
				linecount = lines.Length;
				for (int i = 0; i < linecount; i++) {
					if (lines[i].EndsWith("\r")) {
						lines[i] = lines[i].Substring(0, lines[i].Length - 1);
					}
				}

				// Grab the formatting for the last element
				document.MoveCaret(CaretDirection.CtrlEnd);

				// Insert the first line
				document.InsertString(document.CaretLine, document.CaretPosition, lines[0]);

				for (int i = 1; i < linecount; i++) {
					document.Add(document.CaretLine.LineNo+i, CaseAdjust(lines[i]), alignment, document.CaretTag.font, document.CaretTag.color);
				}

				document.RecalculateDocument(CreateGraphics());
				document.MoveCaret(CaretDirection.CtrlEnd);
				Invalidate();
			} else {
				document.MoveCaret(CaretDirection.CtrlEnd);
				document.InsertStringAtCaret(text, true);
				Invalidate();
			}
			OnTextChanged(EventArgs.Empty);
		}

		public void Clear() {
			// FIXME
			throw new NotImplementedException();
		}

		public void ClearUndo() {
			// FIXME
			throw new NotImplementedException();
		}

		public void Copy() {
			// FIXME
			throw new NotImplementedException();
		}

		public void Cut() {
			// FIXME
			throw new NotImplementedException();
		}

		public void Paste() {
			// FIXME
			throw new NotImplementedException();
		}

		public void ScrollToCaret() {
			// FIXME
			throw new NotImplementedException();
		}

		public void Select(int start, int length) {
			SelectionStart = start;
			SelectionLength = length;
		}


		public void SelectAll() {
			Line	last;

			last = document.GetLine(document.Lines);
			document.SetSelectionStart(document.GetLine(1), 0);
			document.SetSelectionEnd(last, last.text.Length);
		}

		public override string ToString() {
			StringBuilder	sb;
			int		i;
			int		end;

			if (document == null) {
				return String.Empty;
			}

			sb = new StringBuilder();

			end = document.Lines;

			for (i = 1; i < end; i++) {
				sb.Append(document.GetLine(i).text.ToString() + "\n");
			}

			return sb.ToString();
		}

		public void Undo() {
			return;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void CreateHandle() {
			base.CreateHandle ();
		}

		protected override bool IsInputKey(Keys keyData) {
			switch (keyData) {
				case Keys.Enter: {
					if (multiline && (accepts_return || ((keyData & Keys.Control) != 0))) {
						return true;
					}
					return false;
				}

				case Keys.Tab: {
					if (accepts_tab) {
						return true;
					}
					return false;
				}
			}
			return false;
		}


		protected virtual void OnAcceptsTabChanged(EventArgs e) {
			if (AcceptsTabChanged != null) {
				AcceptsTabChanged(this, e);
			}
		}

		protected virtual void OnAutoSizeChanged(EventArgs e) {
			if (AutoSizeChanged != null) {
				AutoSizeChanged(this, e);
			}
		}

		protected virtual void OnBorderStyleChanged(EventArgs e) {
			if (BorderStyleChanged != null) {
				BorderStyleChanged(this, e);
			}
		}

		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged (e);

			if (auto_size) {
				if (PreferredHeight != Height) {
					Height = PreferredHeight;
				}
			}
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed(EventArgs e) {
			base.OnHandleDestroyed (e);
		}

		protected virtual void OnHideSelectionChanged(EventArgs e) {
			if (HideSelectionChanged != null) {
				HideSelectionChanged(this, e);
			}
		}

		protected virtual void OnModifiedChanged(EventArgs e) {
			if (ModifiedChanged != null) {
				ModifiedChanged(this, e);
			}
		}

		protected virtual void OnMultilineChanged(EventArgs e) {
			if (MultilineChanged != null) {
				MultilineChanged(this, e);
			}
		}

		protected virtual void OnReadOnlyChanged(EventArgs e) {
			if (ReadOnlyChanged != null) {
				ReadOnlyChanged(this, e);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData) {
			switch (keyData & Keys.KeyCode) {
				case Keys.Left: {
					document.SetSelectionToCaret(true);

					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.WordBack);
					} else {
						document.MoveCaret(CaretDirection.CharBack);
					}
					return true;
				}

				case Keys.Right: {
					document.SetSelectionToCaret(true);

					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.WordForward);
					} else {
						document.MoveCaret(CaretDirection.CharForward);
					}
					return true;
				}

				case Keys.Up: {
					document.SetSelectionToCaret(true);
					document.MoveCaret(CaretDirection.LineUp);
					return true;
				}

				case Keys.Down: {
					document.SetSelectionToCaret(true);
					document.MoveCaret(CaretDirection.LineDown);
					return true;
				}

				case Keys.Home: {
					document.SetSelectionToCaret(true);

					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlHome);
					} else {
						document.MoveCaret(CaretDirection.Home);
					}
					return true;
				}

				case Keys.End: {
					document.SetSelectionToCaret(true);

					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlEnd);
					} else {
						document.MoveCaret(CaretDirection.End);
					}
					return true;
				}

				case Keys.Enter: {
					if (multiline && (accepts_return || ((Control.ModifierKeys & Keys.Control) != 0))) {
						if (document.selection_visible) {
							document.ReplaceSelection("");
						}
						document.SetSelectionToCaret(true);

						document.Split(document.CaretLine, document.CaretTag, document.CaretPosition);
						OnTextChanged(EventArgs.Empty);
						document.UpdateView(document.CaretLine, 2, 0);
						document.MoveCaret(CaretDirection.CharForward);
						return true;
					}
					break;
				}

				case Keys.Tab: {
					if (accepts_tab) {
						document.InsertChar(document.CaretLine, document.CaretPosition, '\t');
						if (document.selection_visible) {
							document.ReplaceSelection("");
						}
						document.SetSelectionToCaret(true);

						OnTextChanged(EventArgs.Empty);
						return true;
					}
					break;
				}


				case Keys.Back: {
					// delete only deletes on the line, doesn't do the combine
					if (document.selection_visible) {
						document.ReplaceSelection("");
					}
					document.SetSelectionToCaret(true);
					if (document.CaretPosition == 0) {
						if (document.CaretLine.LineNo > 1) {
							Line	line;
							int	new_caret_pos;

							line = document.GetLine(document.CaretLine.LineNo - 1);
							new_caret_pos = line.text.Length;

							document.Combine(line, document.CaretLine);
							document.UpdateView(line, 1, 0);
							document.PositionCaret(line, new_caret_pos);
							document.UpdateCaret();
							OnTextChanged(EventArgs.Empty);
						}
					} else {
						document.DeleteChar(document.CaretTag, document.CaretPosition, false);
						document.MoveCaret(CaretDirection.CharBack);
						OnTextChanged(EventArgs.Empty);
					}
					return true;
				}

				case Keys.Delete: {
					// delete only deletes on the line, doesn't do the combine
					if (document.CaretPosition == document.CaretLine.text.Length) {
						if (document.CaretLine.LineNo < document.Lines) {
							Line	line;

							line = document.GetLine(document.CaretLine.LineNo + 1);
							document.Combine(document.CaretLine, line);
							document.UpdateView(document.CaretLine, 2, 0);
							OnTextChanged(EventArgs.Empty);

#if Debug
							Line	check_first;
							Line	check_second;

							check_first = document.GetLine(document.CaretLine.LineNo);
							check_second = document.GetLine(check_first.line_no + 1);

							Console.WriteLine("Post-UpdateView: Y of first line: {0}, second line: {1}", check_first.Y, check_second.Y);
#endif

							// Caret doesn't move
						}
					} else {
						document.DeleteChar(document.CaretTag, document.CaretPosition, true);
						OnTextChanged(EventArgs.Empty);
					}
					return true;
				}
			}
			return base.ProcessDialogKey (keyData);
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			// Make sure we don't get sized bigger than we want to be
			if (!richtext) {
				if (!multiline) {
					if (height > PreferredHeight) {
						requested_height = height;
						height = PreferredHeight;
						specified |= BoundsSpecified.Height;
					}
				}
			}

			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void WndProc(ref Message m) {
			switch ((Msg)m.Msg) {
				case Msg.WM_PAINT: {
					PaintEventArgs	paint_event;

					paint_event = XplatUI.PaintEventStart(Handle);
					
					PaintControl(paint_event);
					XplatUI.PaintEventEnd(Handle);
					DefWndProc(ref m);
					return;
				}

				case Msg.WM_SETFOCUS: {
					// Set caret
					document.CaretHasFocus();
Console.WriteLine("Creating caret");
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_KILLFOCUS: {
					// Kill caret
					document.CaretLostFocus();
Console.WriteLine("Destroying caret");
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_CHAR: {
					if (ProcessKeyEventArgs(ref m)) {
						return;
					}

					if (PreProcessMessage(ref m)) {
						return;
					}

					if (ProcessKeyMessage(ref m)) {
						return;
					}

					if (m.WParam.ToInt32() >= 32) {	// FIXME, tabs should probably go through
						if (document.selection_visible) {
							document.ReplaceSelection("");
						}

						switch (character_casing) {
							case CharacterCasing.Normal: {
								document.InsertCharAtCaret((char)m.WParam, true);
								OnTextChanged(EventArgs.Empty);
								return;
							}

							case CharacterCasing.Lower: {
								document.InsertCharAtCaret(Char.ToLower((char)m.WParam), true);
								OnTextChanged(EventArgs.Empty);
								return;
							}

							case CharacterCasing.Upper: {
								document.InsertCharAtCaret(Char.ToUpper((char)m.WParam), true);
								OnTextChanged(EventArgs.Empty);
								return;
							}
						}
					}
					DefWndProc(ref m);
					return;
				}

				default: {
					base.WndProc(ref m);
					return;
				}
			}
		}

		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler	AcceptsTabChanged;
		public event EventHandler	AutoSizeChanged;
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}
		public event EventHandler	BorderStyleChanged;
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public event EventHandler	Click;
		public event EventHandler	HideSelectionChanged;
		public event EventHandler	ModifiedChanged;
		public event EventHandler	MultilineChanged;
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event PaintEventHandler	Paint;
		public event EventHandler	ReadOnlyChanged;
		#endregion	// Events

		#region Private Methods
		internal Document Document {
			get {
				return document;
			}

			set {
				document = value;
			}
		}

static int current;

		private void PaintControl(PaintEventArgs pevent) {
			// Fill background
			pevent.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(BackColor), pevent.ClipRectangle);
			//pevent.Graphics.TextRenderingHint=TextRenderingHint.AntiAlias;

			// Draw the viewable document
			document.Draw(pevent.Graphics, pevent.ClipRectangle);

			Rectangle	rect = ClientRectangle;
			rect.Width--;
			rect.Height--;
			pevent.Graphics.DrawRectangle(ThemeEngine.Current.ResPool.GetPen(ThemeEngine.Current.ColorButtonShadow), rect);

			// Set the scrollbar
			switch (scrollbars) {
				case ScrollBars.Both: {
					break;
				}
				case ScrollBars.Vertical: {
					break;
				}
				case ScrollBars.Horizontal: {
					hscroll.Minimum = 0;
					hscroll.Maximum = document.Width - this.Width;
					break;
				}
			}

			#if Debug
				int		start;
				int		end;
				Line		line;
				int		line_no;
				Pen		p;

				p = new Pen(Color.Red, 1);

				// First, figure out from what line to what line we need to draw
				start = document.GetLineByPixel(pevent.ClipRectangle.Top - viewport_y, false).line_no;
				end = document.GetLineByPixel(pevent.ClipRectangle.Bottom - viewport_y, false).line_no;

				//Console.WriteLine("Starting drawing on line '{0}'", document.GetLine(start));
				//Console.WriteLine("Ending drawing on line '{0}'", document.GetLine(end));

				line_no = start;
				while (line_no <= end) {
					line = document.GetLine(line_no);

					if (draw_lines) {
						for (int i = 0; i < line.text.Length; i++) {
							pevent.Graphics.DrawLine(p, (int)line.widths[i] - document.ViewPortX, line.Y - document.ViewPortY, (int)line.widths[i] - document.ViewPortX, line.Y + line.height  - document.ViewPortY);
						}
					}

					line_no++;
				}
			#endif
		}

		private void TextBoxBase_MouseDown(object sender, MouseEventArgs e) {
			LineTag	tag;
			Line	line;
			int	pos;

			if (e.Button == MouseButtons.Left) {
				document.PositionCaret(e.X, e.Y);
				document.SetSelectionToCaret(true);
				this.grabbed = true;
				this.Capture = true;
				return;
			}

			#if Debug
				if (e.Button == MouseButtons.Right) {
					draw_lines = !draw_lines;
					this.Invalidate();
					Console.WriteLine("SelectedText: {0}, length {1}", this.SelectedText, this.SelectionLength);
					Console.WriteLine("Selection start: {0}", this.SelectionStart);

					this.SelectionStart = 10;
					this.SelectionLength = 5;

					return;
				}

				tag = document.FindTag(e.X, e.Y, out pos, false);

				Console.WriteLine("Click found tag {0}, character {1}", tag, pos);
				line = tag.line;
				switch(current) {
					case 4: LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, new Font("impact", 20, FontStyle.Bold, GraphicsUnit.Pixel), ThemeEngine.Current.ResPool.GetSolidBrush(Color.Red)); break;
					case 1: LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, new Font("arial unicode ms", 24, FontStyle.Italic, GraphicsUnit.Pixel), ThemeEngine.Current.ResPool.GetSolidBrush(Color.DarkGoldenrod)); break;
					case 2: LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, new Font("arial", 10, FontStyle.Regular, GraphicsUnit.Pixel), ThemeEngine.Current.ResPool.GetSolidBrush(Color.Aquamarine)); break;
					case 3: LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, new Font("times roman", 16, FontStyle.Underline, GraphicsUnit.Pixel), ThemeEngine.Current.ResPool.GetSolidBrush(Color.Turquoise)); break;
					case 0: LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, new Font("times roman", 64, FontStyle.Italic | FontStyle.Bold, GraphicsUnit.Pixel), ThemeEngine.Current.ResPool.GetSolidBrush(Color.LightSeaGreen)); break;
					case 5: LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, ((TextBoxBase)sender).Font, ThemeEngine.Current.ResPool.GetSolidBrush(ForeColor)); break;
				}
				current++;
				if (current==6) {
					current=0;
				}

				// Update/Recalculate what we see
				document.UpdateView(line, 0);

				// Make sure our caret is properly positioned and sized
				document.AlignCaret();
			#endif
		}

		private void TextBoxBase_MouseUp(object sender, MouseEventArgs e) {
			this.Capture = false;
			this.grabbed = false;
			if (e.Button == MouseButtons.Left) {
				document.PositionCaret(e.X + viewport_x, e.Y + viewport_y);
				document.SetSelectionToCaret(false);
				document.DisplayCaret();
				return;
			}
		}
		#endregion	// Private Methods


		private void TextBoxBase_SizeChanged(object sender, EventArgs e) {

			// First, check which scrollbars we need
			
			hscroll.Bounds = new Rectangle (ClientRectangle.Left, ClientRectangle.Bottom - hscroll.Height, Width, hscroll.Height);
			
		}

		private void hscroll_ValueChanged(object sender, EventArgs e) {
			XplatUI.ScrollWindow(this.Handle, document.ViewPortX-this.hscroll.Value, 0, false);
			document.ViewPortX = this.hscroll.Value;
			document.UpdateCaret();
			Console.WriteLine("Dude scrolled");
		}

		private void TextBoxBase_MouseMove(object sender, MouseEventArgs e) {
			// FIXME - handle auto-scrolling if mouse is to the right/left of the window
			if (grabbed) {
				document.PositionCaret(e.X + viewport_x, e.Y + viewport_y);
				document.SetSelectionToCaret(false);
				document.DisplayCaret();
			}
		}
									      
		private void TextBoxBase_FontOrColorChanged(object sender, EventArgs e) {
			if (!richtext) {
				Line	line;

				// Font changes apply to the whole document
				for (int i = 1; i <= document.Lines; i++) {
					line = document.GetLine(i);
					LineTag.FormatText(line, 1, line.text.Length, Font, ThemeEngine.Current.ResPool.GetSolidBrush(ForeColor));
					document.UpdateView(line, 0);
				}
				// Make sure the caret height is matching the new font height
				document.AlignCaret();
			}
		}
	}
}
