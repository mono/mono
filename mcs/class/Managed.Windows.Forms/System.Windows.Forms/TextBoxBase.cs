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
#undef Debug
#undef DebugClick

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
		internal char			password_char;
		internal bool			read_only;
		internal bool			word_wrap;
		internal Document		document;
		internal LineTag		caret_tag;		// tag our cursor is in
		internal int			caret_pos;		// position on the line our cursor is in (can be 0 = beginning of line)
		internal ImplicitHScrollBar	hscroll;
		internal ImplicitVScrollBar	vscroll;
		internal RichTextBoxScrollBars	scrollbars;
		internal bool			richtext;
		internal int			requested_height;
		internal int			canvas_width;
		internal int			canvas_height;
		internal int			track_width = 20;
		internal DateTime		click_last;
		internal CaretSelection		click_mode;
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
			password_char = '\0';
			read_only = false;
			word_wrap = true;
			richtext = false;
			document = new Document(this);
			document.WidthChanged += new EventHandler(document_WidthChanged);
			document.HeightChanged += new EventHandler(document_HeightChanged);
			//document.CaretMoved += new EventHandler(CaretMoved);
			document.Wrap = false;
			requested_height = -1;
			click_last = DateTime.Now;
			click_mode = CaretSelection.Position;

			MouseDown += new MouseEventHandler(TextBoxBase_MouseDown);
			MouseUp += new MouseEventHandler(TextBoxBase_MouseUp);
			MouseMove += new MouseEventHandler(TextBoxBase_MouseMove);
			SizeChanged += new EventHandler(TextBoxBase_SizeChanged);
			FontChanged += new EventHandler(TextBoxBase_FontOrColorChanged);
			ForeColorChanged += new EventHandler(TextBoxBase_FontOrColorChanged);
			MouseWheel += new MouseEventHandler(TextBoxBase_MouseWheel);
			
			scrollbars = RichTextBoxScrollBars.None;

			hscroll = new ImplicitHScrollBar();
			hscroll.ValueChanged += new EventHandler(hscroll_ValueChanged);
			hscroll.control_style &= ~ControlStyles.Selectable;
			hscroll.Enabled = false;
			hscroll.Visible = false;

			vscroll = new ImplicitVScrollBar();
			vscroll.ValueChanged += new EventHandler(vscroll_ValueChanged);
			vscroll.control_style &= ~ControlStyles.Selectable;
			vscroll.Enabled = false;
			vscroll.Visible = false;

			SuspendLayout ();
			this.Controls.AddImplicit (hscroll);
			this.Controls.AddImplicit (vscroll);
			ResumeLayout ();
			
			//SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.UserPaint | ControlStyles.StandardClick, false);

			canvas_width = ClientSize.Width;
			canvas_height = ClientSize.Height;

			CalculateScrollBars();
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
						if (PreferredHeight != ClientSize.Height) {
							ClientSize = new Size(ClientSize.Width, PreferredHeight);
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
			get { return InternalBorderStyle; }
			set { 
				InternalBorderStyle = value; 
				OnBorderStyleChanged(EventArgs.Empty);
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
				CalculateDocument();
				OnTextChanged(EventArgs.Empty);
			}
		}

		[DefaultValue(32767)]
		[Localizable(true)]
		public virtual int MaxLength {
			get {
				if (max_length == 2147483646) {	// We don't distinguish between single and multi-line limits
					return 0;
				}
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

				if (multiline) {
					document.Wrap = word_wrap;
					document.PasswordChar = "";

				} else {
					document.Wrap = false;
					if (this.password_char != '\0') {
						document.PasswordChar = password_char.ToString();
					} else {
						document.PasswordChar = "";
					}
				}
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
				if (!read_only) {
					document.ReplaceSelection(CaseAdjust(value));
					OnTextChanged(EventArgs.Empty);
				}
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
					sb.Append(document.GetLine(document.Lines).text.ToString());
					return sb.ToString();
				}
			}

			set {
				if (value == base.Text) {
					return;
				}

				if (value != null) {
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
						CalculateDocument();
						line = document.GetLine(1);
						document.SetSelectionStart(line, 0);
						document.SetSelectionEnd(line, value.Length);
						document.PositionCaret(line, value.Length);
					}
				}
				base.Text = value;
				// Not needed, base.Text already fires it
				// OnTextChanged(EventArgs.Empty);
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
					total += document.GetLine(i).text.Length;

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
					if (multiline) {
						word_wrap = value;
						document.Wrap = value;
					}
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
				return new Size(100, 20);
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void AppendText(string text) {
			if (multiline) {
				// Grab the formatting for the last element
				document.MoveCaret(CaretDirection.CtrlEnd);
				document.Insert(document.CaretLine, document.CaretPosition, text);

				CalculateDocument();
				document.MoveCaret(CaretDirection.CtrlEnd);
			} else {
				document.MoveCaret(CaretDirection.CtrlEnd);
				document.InsertStringAtCaret(text, true);
				Invalidate();
			}
			OnTextChanged(EventArgs.Empty);
		}

		public void Clear() {
			Text = null;
		}

		public void ClearUndo() {
			document.undo.Clear();
		}

		public void Copy() {
			DataObject	o;

			o = new DataObject(DataFormats.Text, SelectedText);
			if (this is RichTextBox) {
				o.SetData(DataFormats.Rtf, ((RichTextBox)this).SelectedRtf);
			}
			Clipboard.SetDataObject(o);
		}

		public void Cut() {
			DataObject	o;

			o = new DataObject(DataFormats.Text, SelectedText);
			if (this is RichTextBox) {
				o.SetData(DataFormats.Rtf, ((RichTextBox)this).SelectedRtf);
			}
			Clipboard.SetDataObject(o);
			document.ReplaceSelection("");
		}

		public void Paste() {
			Paste(null, false);
		}

		public void ScrollToCaret() {
			this.CaretMoved(this, EventArgs.Empty);
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
			return Text;
		}

		public void Undo() {
			document.undo.Undo();
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void CreateHandle() {
			base.CreateHandle ();
		}

		protected override bool IsInputKey(Keys keyData) {
			if ((keyData & Keys.Alt) != 0) {
				return base.IsInputKey(keyData);
			}

			switch (keyData & Keys.KeyCode) {
				case Keys.Enter: {
					if (multiline && accepts_return) {
						return true;
					}
					return false;
				}

				case Keys.Tab: {
					if (accepts_tab && multiline) {
						if ((keyData & Keys.Control) == 0) {
							return true;
						}
					}
					return false;
				}

				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.Home:
				case Keys.End: {
					return true;
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
				if (PreferredHeight != ClientSize.Height) {
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
			return base.ProcessDialogKey(keyData);
		}

		private bool ProcessKey(Keys keyData) {
			bool control;
			bool shift;

			control = (Control.ModifierKeys & Keys.Control) != 0;
			shift = (Control.ModifierKeys & Keys.Shift) != 0;

			switch (keyData & Keys.KeyCode) {
				case Keys.X: {	// Cut (Ctrl-X)
					if (control) {
						Cut();
						return true;
					}
					return false;
				}

				case Keys.C: {	// Copy (Ctrl-C)
					if (control) {
						Copy();
						return true;
					}
					return false;
				}

				case Keys.V: {	// Paste (Ctrl-V)
					if (control) {
						return Paste(null, true);
					}
					return false;
				}

				case Keys.Z: {	// Undo (Ctrl-Z)
					if (control) {
						Undo();
						return true;
					}
					return false;
				}

				case Keys.Left: {
					if (control) {
						document.MoveCaret(CaretDirection.WordBack);
					} else {
						if (!document.selection_visible || shift) {
							document.MoveCaret(CaretDirection.CharBack);
						} else {
							document.MoveCaret(CaretDirection.SelectionStart);
						}
					}

					if (!shift) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Right: {
					if (control) {
						document.MoveCaret(CaretDirection.WordForward);
					} else {
						if (!document.selection_visible || shift) {
							document.MoveCaret(CaretDirection.CharForward);
						} else {
							document.MoveCaret(CaretDirection.SelectionEnd);
						}
					}
					if (!shift) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Up: {
					if (control) {
						if (document.CaretPosition == 0) {
							document.MoveCaret(CaretDirection.LineUp);
						} else {
							document.MoveCaret(CaretDirection.Home);
						}
					} else {
						document.MoveCaret(CaretDirection.LineUp);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Down: {
					if (control) {
						if (document.CaretPosition == document.CaretLine.Text.Length) {
							document.MoveCaret(CaretDirection.LineDown);
						} else {
							document.MoveCaret(CaretDirection.End);
						}
					} else {
						document.MoveCaret(CaretDirection.LineDown);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Home: {
					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlHome);
					} else {
						document.MoveCaret(CaretDirection.Home);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.End: {
					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlEnd);
					} else {
						document.MoveCaret(CaretDirection.End);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Enter: {
					// ignoring accepts_return, fixes bug #76355
					if (!read_only && multiline && (accepts_return || (FindForm() != null && FindForm().AcceptButton == null) || ((Control.ModifierKeys & Keys.Control) != 0))) {
						Line	line;

						if (document.selection_visible) {
							document.ReplaceSelection("");
						}
						document.SetSelectionToCaret(true);

						line = document.CaretLine;

						document.Split(document.CaretLine, document.CaretTag, document.CaretPosition, false);
						OnTextChanged(EventArgs.Empty);
						document.UpdateView(line, 2, 0);
						document.MoveCaret(CaretDirection.CharForward);
						CaretMoved(this, null);
						return true;
					}
					break;
				}

				case Keys.Tab: {
					if (!read_only && accepts_tab && multiline) {
						document.InsertChar(document.CaretLine, document.CaretPosition, '\t');
						if (document.selection_visible) {
							document.ReplaceSelection("");
						}
						document.SetSelectionToCaret(true);

						OnTextChanged(EventArgs.Empty);
						CaretMoved(this, null);
						return true;
					}
					break;
				}


				case Keys.Back: {
					if (read_only) {
						break;
					}

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
							//document.MoveCaret(CaretDirection.CharForward);
							document.UpdateCaret();
							OnTextChanged(EventArgs.Empty);
						}
					} else {
						if (!control || document.CaretPosition == 0) {
							document.DeleteChar(document.CaretTag, document.CaretPosition, false);
							document.MoveCaret(CaretDirection.CharBack);
						} else {
							int start_pos;

							start_pos = document.CaretPosition - 1;

							while ((start_pos > 0) && !Document.IsWordSeparator(document.CaretLine.Text[start_pos - 1])) {
								start_pos--;
							}
							document.DeleteChars(document.CaretTag, start_pos, document.CaretPosition - start_pos);
							document.PositionCaret(document.CaretLine, start_pos);
						}
						document.UpdateCaret();
						OnTextChanged(EventArgs.Empty);
					}
					CaretMoved(this, null);
					return true;
				}

				case Keys.Insert: {
					if (shift) {
						Paste(null, true);
						return true;
					}

					if (control) {
						Copy();
						return true;
					}

					// FIXME - need overwrite/insert toggle?
					return false;
				}

				case Keys.Delete: {
					if (shift) {
						Cut();
						return true;
					}

					if (read_only) {
						break;
					}

					if (document.selection_visible) {
						document.ReplaceSelection("");
					} else {
						// DeleteChar only deletes on the line, doesn't do the combine
						if (document.CaretPosition == document.CaretLine.Text.Length) {
							if (document.CaretLine.LineNo < document.Lines) {
								Line	line;

								line = document.GetLine(document.CaretLine.LineNo + 1);
								document.Combine(document.CaretLine, line);
								document.UpdateView(document.CaretLine, 2, 0);

								#if not_Debug
								Line	check_first;
								Line	check_second;

								check_first = document.GetLine(document.CaretLine.LineNo);
								check_second = document.GetLine(check_first.line_no + 1);

								Console.WriteLine("Post-UpdateView: Y of first line: {0}, second line: {1}", check_first.Y, check_second.Y);
								#endif

								// Caret doesn't move
							}
						} else {
							if (!control) {
								document.DeleteChar(document.CaretTag, document.CaretPosition, true);
							} else {
								int end_pos;

								end_pos = document.CaretPosition;

								while ((end_pos < document.CaretLine.Text.Length) && !Document.IsWordSeparator(document.CaretLine.Text[end_pos])) {
									end_pos++;
								}

								if (end_pos < document.CaretLine.Text.Length) {
									end_pos++;
								}
								document.DeleteChars(document.CaretTag, document.CaretPosition, end_pos - document.CaretPosition);
							}
						}
					}

					OnTextChanged(EventArgs.Empty);
					document.AlignCaret();
					document.UpdateCaret();
					CaretMoved(this, null);
					return true;
				}
			}
			return false;
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			// Make sure we don't get sized bigger than we want to be
			if (!richtext) {
				if (!multiline) {
					if (height != PreferredHeight) {
						requested_height = height;
						height = PreferredHeight;
						specified |= BoundsSpecified.Height;
					}
				}
			}

			document.ViewPortWidth = width;
			document.ViewPortHeight = height;

			CalculateDocument();

			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void WndProc(ref Message m) {
			switch ((Msg)m.Msg) {
				case Msg.WM_PAINT: {
					PaintEventArgs	paint_event;

					paint_event = XplatUI.PaintEventStart(Handle, true);
					
					PaintControl(paint_event);
					XplatUI.PaintEventEnd(Handle, true);
					DefWndProc(ref m);
					return;
				}

				case Msg.WM_SETFOCUS: {
					// Set caret
					document.CaretHasFocus();
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_KILLFOCUS: {
					// Kill caret
					document.CaretLostFocus();
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_KEYDOWN: {
					base.WndProc(ref m);
					ProcessKey((Keys)m.WParam.ToInt32() | XplatUI.State.ModifierKeys);
					return;
				}

				case Msg.WM_CHAR: {
					// Ctrl-Backspace generates a real char, whack it
					if (m.WParam.ToInt32() == 127) {
						base.WndProc(ref m);
						return;
					}

					base.WndProc(ref m);

					if (!read_only && (m.WParam.ToInt32() >= 32)) {	// FIXME, tabs should probably go through
						if (document.selection_visible) {
							document.ReplaceSelection("");
						}

						switch (character_casing) {
							case CharacterCasing.Normal: {
								if (document.Length < max_length) {
									document.InsertCharAtCaret((char)m.WParam, true);
									OnTextChanged(EventArgs.Empty);
									CaretMoved(this, null);
								} else {
									XplatUI.AudibleAlert();
								}
								return;
							}

							case CharacterCasing.Lower: {
								if (document.Length < max_length) {
									document.InsertCharAtCaret(Char.ToLower((char)m.WParam), true);
									OnTextChanged(EventArgs.Empty);
									CaretMoved(this, null);
								} else {
									XplatUI.AudibleAlert();
								}
								return;
							}

							case CharacterCasing.Upper: {
								if (document.Length < max_length) {
									document.InsertCharAtCaret(Char.ToUpper((char)m.WParam), true);
									OnTextChanged(EventArgs.Empty);
									CaretMoved(this, null);
								} else {
									XplatUI.AudibleAlert();
								}
								return;
							}
						}
					}
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

		internal event EventHandler	HScrolled;
		internal event EventHandler	VScrolled;
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

		#if Debug
		static int current;
		#endif

		private void PaintControl(PaintEventArgs pevent) {
			// Fill background
			pevent.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(BackColor), pevent.ClipRectangle);
			pevent.Graphics.TextRenderingHint=TextRenderingHint.AntiAlias;

			// Draw the viewable document
			document.Draw(pevent.Graphics, pevent.ClipRectangle);

			Rectangle	rect = ClientRectangle;
			rect.Width--;
			rect.Height--;
			//pevent.Graphics.DrawRectangle(ThemeEngine.Current.ResPool.GetPen(ThemeEngine.Current.ColorControlDark), rect);

			#if Debug
				int		start;
				int		end;
				Line		line;
				int		line_no;
				Pen		p;

				p = new Pen(Color.Red, 1);

				// First, figure out from what line to what line we need to draw
				start = document.GetLineByPixel(pevent.ClipRectangle.Top - document.ViewPortY, false).line_no;
				end = document.GetLineByPixel(pevent.ClipRectangle.Bottom - document.ViewPortY, false).line_no;

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
			if (e.Button == MouseButtons.Left) {
				TimeSpan interval;

				interval = DateTime.Now - click_last;
				document.PositionCaret(e.X + document.ViewPortX, e.Y + document.ViewPortY);
				this.Capture = true;
				
				// Handle place caret/select word/select line behaviour
				if (e.Clicks == 1) {
					if (SystemInformation.DoubleClickTime < interval.TotalMilliseconds) {
						#if DebugClick
							Console.WriteLine("Single Click Invalidating from char {0} to char {1} ({2})", document.selection_start.pos, document.selection_end.pos, document.selection_start.line.text.ToString(document.selection_start.pos, document.selection_end.pos - document.selection_start.pos));
						#endif
						document.SetSelectionToCaret(true);
						click_mode = CaretSelection.Position;
					} else {
						#if DebugClick
							Console.WriteLine("Tripple Click Selecting line");
						#endif
						document.ExpandSelection(CaretSelection.Line, false);
						click_mode = CaretSelection.Line;
					}
				} else {
					// We select the line if the word is already selected, and vice versa
					if (click_mode != CaretSelection.Word) {
						if (click_mode == CaretSelection.Line) {
							document.Invalidate(document.selection_start.line, 0, document.selection_start.line, document.selection_start.line.text.Length);
						}
						click_mode = CaretSelection.Word;
						document.ExpandSelection(CaretSelection.Word, false);	// Setting initial selection
					} else {
						click_mode = CaretSelection.Line;
						document.ExpandSelection(CaretSelection.Line, false);	// Setting initial selection
					}
				}

				// Reset
				click_last = DateTime.Now;
				return;
			}

			#if Debug
				LineTag	tag;
				Line	line;
				int	pos;

				if (e.Button == MouseButtons.Right) {
					draw_lines = !draw_lines;
					this.Invalidate();
					Console.WriteLine("SelectedText: {0}, length {1}", this.SelectedText, this.SelectionLength);
					Console.WriteLine("Selection start: {0}", this.SelectionStart);

					this.SelectionStart = 10;
					this.SelectionLength = 5;

					return;
				}

				tag = document.FindTag(e.X + document.ViewPortX, e.Y + document.ViewPortY, out pos, false);

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
			if (e.Button == MouseButtons.Left) {
				document.PositionCaret(e.X + document.ViewPortX, e.Y + document.ViewPortY);
				if (click_mode == CaretSelection.Position) {
					document.SetSelectionToCaret(false);
					document.DisplayCaret();
				} else {
					document.ExpandSelection(click_mode, true);
				}
				return;
			}
		}

		private void TextBoxBase_SizeChanged(object sender, EventArgs e) {
			canvas_width = ClientSize.Width;
			canvas_height = ClientSize.Height;

			// We always move them, they just might not be displayed
			hscroll.Bounds = new Rectangle (ClientRectangle.Left, ClientRectangle.Bottom - hscroll.Height, ClientSize.Width - (vscroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0), hscroll.Height);
			vscroll.Bounds = new Rectangle (ClientRectangle.Right - vscroll.Width, ClientRectangle.Top, vscroll.Width, ClientSize.Height - (hscroll.Visible ? SystemInformation.HorizontalScrollBarHeight : 0));
			
		}

		private void TextBoxBase_MouseWheel(object sender, MouseEventArgs e) {
			Line	line;
			int	line_no;
			int	target;

			if (!vscroll.Enabled) {
				return;
			}

			if (e.Delta < 0) {
				line_no = document.GetLineByPixel(document.ViewPortY, false).line_no + SystemInformation.MouseWheelScrollLines;
				if (line_no > document.Lines) {
					line_no = document.Lines;
				}
			} else {
				line_no = document.GetLineByPixel(document.ViewPortY, false).line_no - SystemInformation.MouseWheelScrollLines;
				if (line_no < 1) {
					line_no = 1;
				}
			}

			line = document.GetLine(line_no);
			if (line.Y < vscroll.Maximum) {
				vscroll.Value = line.Y;
			} else {
				vscroll.Value = vscroll.Maximum;
			}
		}

		internal void CalculateDocument() {
			if (!IsHandleCreated) {
				return;
			}
			document.RecalculateDocument(CreateGraphics());
			CalculateScrollBars();
//blah Console.WriteLine("TextBox.cs(1175) Invalidate called in CalculateDocument");
			Invalidate();	// FIXME - do we need this?
		}

		internal void CalculateScrollBars() {
			// FIXME - need separate calculations for center and right alignment
			// No scrollbars for a single line
			if (document.Width >= ClientSize.Width) {
				hscroll.Enabled = true;
				hscroll.Minimum = 0;
				hscroll.Maximum = document.Width - ClientSize.Width;
			} else {
				hscroll.Maximum = document.ViewPortWidth;
				hscroll.Enabled = false;
			}

			if (document.Height >= ClientSize.Height) {
				vscroll.Enabled = true;
				vscroll.Minimum = 0;
				vscroll.Maximum = document.Height - ClientSize.Height;
			} else {
				vscroll.Maximum = document.ViewPortHeight;
				vscroll.Enabled = false;
			}


			if (!multiline) {
				return;
			}

			if (!WordWrap) {
				if ((scrollbars & RichTextBoxScrollBars.Horizontal) != 0) {
					if (((scrollbars & RichTextBoxScrollBars.ForcedHorizontal) != 0) || hscroll.Enabled) {
						hscroll.Visible = true;
					}
				}
			}

			if ((scrollbars & RichTextBoxScrollBars.Vertical) != 0) {
				if (((scrollbars & RichTextBoxScrollBars.ForcedVertical) != 0) || vscroll.Enabled) {
					vscroll.Visible = true;
				}
			}

			if (hscroll.Visible) {
				vscroll.Maximum += hscroll.Height;
				canvas_height = ClientSize.Height - hscroll.Height;
			}

			if (vscroll.Visible) {
				hscroll.Maximum += vscroll.Width * 2;
				canvas_width = ClientSize.Width - vscroll.Width * 2;
			}

			TextBoxBase_SizeChanged(this, EventArgs.Empty);
		}

		private void document_WidthChanged(object sender, EventArgs e) {
			CalculateScrollBars();
		}

		private void document_HeightChanged(object sender, EventArgs e) {
			CalculateScrollBars();
		}

		private void hscroll_ValueChanged(object sender, EventArgs e) {
			XplatUI.ScrollWindow(this.Handle, document.ViewPortX-this.hscroll.Value, 0, false);
			document.ViewPortX = this.hscroll.Value;
			document.UpdateCaret();

			if (HScrolled != null) {
				HScrolled(this, EventArgs.Empty);
			}
		}

		private void vscroll_ValueChanged(object sender, EventArgs e) {
			XplatUI.ScrollWindow(this.Handle, 0, document.ViewPortY-this.vscroll.Value, false);
			document.ViewPortY = this.vscroll.Value;
			document.UpdateCaret();

			if (VScrolled != null) {
				VScrolled(this, EventArgs.Empty);
			}
		}

		private void TextBoxBase_MouseMove(object sender, MouseEventArgs e) {
			// FIXME - handle auto-scrolling if mouse is to the right/left of the window
			if (Capture) {
				document.PositionCaret(e.X + document.ViewPortX, e.Y + document.ViewPortY);
				if (click_mode == CaretSelection.Position) {
					document.SetSelectionToCaret(false);
					document.DisplayCaret();
				} else {
					document.ExpandSelection(click_mode, true);
				}
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

		/// <summary>Ensure the caret is always visible</summary>
		internal void CaretMoved(object sender, EventArgs e) {
			Point	pos;
			int	height;
			
			pos = document.Caret;
			//Console.WriteLine("Caret now at {0} (Thumb: {1}x{2}, Canvas: {3}x{4}, Document {5}x{6})", pos, hscroll.Value, vscroll.Value, canvas_width, canvas_height, document.Width, document.Height);

			// Handle horizontal scrolling
			if (document.CaretLine.alignment == HorizontalAlignment.Left) {
				if (pos.X < (document.ViewPortX + track_width)) {
					do {
						if ((hscroll.Value - track_width) >= hscroll.Minimum) {
							hscroll.Value -= track_width;
						} else {
							hscroll.Value = hscroll.Minimum;
						}
					} while (hscroll.Value > pos.X);
				}

				if ((pos.X > (this.canvas_width + document.ViewPortX - track_width)) && (hscroll.Value != hscroll.Maximum)) {
					do {
						if ((hscroll.Value + track_width) <= hscroll.Maximum) {
							hscroll.Value += track_width;
						} else {
							hscroll.Value = hscroll.Maximum;
						}
					} while (pos.X > (hscroll.Value + this.canvas_width));
				}
			} else if (document.CaretLine.alignment == HorizontalAlignment.Right) {
				if (pos.X < document.ViewPortX) {
					if (pos.X > hscroll.Minimum) {
						hscroll.Value = pos.X;
					} else {
						hscroll.Value = hscroll.Minimum;
					}
				}

				if ((pos.X > (this.canvas_width + document.ViewPortX)) && (hscroll.Value != hscroll.Maximum)) {
					hscroll.Value = hscroll.Maximum;
				}
			} else {
			}

			if (!multiline) {
				return;
			}

			// Handle vertical scrolling
			height = document.CaretLine.Height;

			if (pos.Y < document.ViewPortY) {
				vscroll.Value = pos.Y;
			}

			if ((pos.Y + height) > (document.ViewPortY + canvas_height)) {
				vscroll.Value = pos.Y - canvas_height + height;
			}
		}

		internal bool Paste(DataFormats.Format format, bool obey_length) {
			IDataObject	clip;
			string		s;

			clip = Clipboard.GetDataObject();
			if (clip == null)
				return false;
			
			if (format == null) {
				if ((this is RichTextBox) && clip.GetDataPresent(DataFormats.Rtf)) {
					format = DataFormats.GetFormat(DataFormats.Rtf);
				} else if (clip.GetDataPresent(DataFormats.UnicodeText)) {
					format = DataFormats.GetFormat(DataFormats.UnicodeText);
				} else if (clip.GetDataPresent(DataFormats.Text)) {
					format = DataFormats.GetFormat(DataFormats.Text);
				} else {
					return false;
				}
			} else {
				if ((format.Name == DataFormats.Rtf) && !(this is RichTextBox)) {
					return false;
				}

				if (!clip.GetDataPresent(format.Name)) {
					return false;
				}
			}

			if (format.Name == DataFormats.Rtf) {
				((RichTextBox)this).SelectedRtf = (string)clip.GetData(DataFormats.Rtf);
				return true;
			} else if (format.Name == DataFormats.UnicodeText) {
				s = (string)clip.GetData(DataFormats.UnicodeText);
			} else if (format.Name == DataFormats.Text) {
				s = (string)clip.GetData(DataFormats.Text);
			} else {
				return false;
			}

			if (!obey_length) {
				this.SelectedText = s;
			} else {
				if ((s.Length + document.Length) < max_length) {
					this.SelectedText = s;
				} else if (document.Length < max_length) {
					this.SelectedText = s.Substring(0, max_length - document.Length);
				}
			}

			return true;
		}
		#endregion	// Private Methods
	}
}
