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
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

// NOT COMPLETE
#define Debug


using System.Drawing;
using System.Drawing.Text;

namespace System.Windows.Forms {
	public abstract class TextBoxBase : Control {
		#region Local Variables
		internal bool		accepts_tab;
		internal bool		auto_size;
		internal BorderStyle	border_style;
		internal bool		undo;
		internal bool		hide_selection;
		internal int		max_length;
		internal bool		modified;
		internal bool		multiline;
		internal int		preferred_height;
		internal bool		read_only;
		internal bool		word_wrap;
		internal Document	document;
		internal LineTag	caret_tag;		// tag our cursor is in
		internal int		caret_pos;		// position on the line our cursor is in (can be 0 = beginning of line)
		internal int		viewport_x;		// left visible pixel
		internal int		viewport_y;		// top visible pixel

		#if Debug
		internal static bool	draw_lines = true;
		#endif

		#endregion	// Local Variables

		#region Private Constructor
		// Constructor will go when complete, only for testing - pdb
		public TextBoxBase() {
			accepts_tab = false;
			auto_size = true;
			border_style = BorderStyle.Fixed3D;
			undo = false;
			hide_selection = true;
			max_length = 32767;
			modified = false;
			multiline = false;
			preferred_height = 10;
			read_only = false;
			word_wrap = true;
			document = new Document(this);
			this.MouseDown += new MouseEventHandler(TextBoxBase_MouseDown);
			this.MouseUp += new MouseEventHandler(TextBoxBase_MouseUp);

			//SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);
		}
		#endregion	// Private Constructor

		#region Public Instance Properties
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

		public virtual bool AutoSize {
			get {
				return auto_size;
			}

			set {
				if (value != auto_size) {
					auto_size = value;
					OnAutoSizeChanged(EventArgs.Empty);
				}
			}
		}

		public override System.Drawing.Color BackColor {
			get {
				return base.BackColor;
			}
			set {
				base.BackColor = value;
			}
		}

		public override System.Drawing.Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
			}
		}

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

		public bool CanUndo {
			get {
				return undo;
			}
		}

		public override System.Drawing.Color ForeColor {
			get {
				return base.ForeColor;
			}
			set {
				base.ForeColor = value;
			}
		}

		public bool HideSelection {
			get {
				return hide_selection;
			}

			set {
				if (value != hide_selection) {
					hide_selection = value;
					OnHideSelectionChanged(EventArgs.Empty);
				}
			}
		}

		public string[] Lines {
			get {
				// FIXME
				return null;
			}

			set {
				// FIXME
			}
		}

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

		public virtual bool Multiline {
			get {
				return multiline;
			}

			set {
				if (value != multiline) {
					multiline = value;
					OnMultilineChanged(EventArgs.Empty);
				}
			}
		}

		public int PreferredHeight {
			get {
				return preferred_height;
			}
		}

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

		public virtual string SelectedText {
			get {
				// FIXME
				return string.Empty;
			}

			set {
				// FIXME
			}
		}

		public virtual int SelectionLength {
			get {
				// FIXME
				return 0;
			}

			set {
				// FIXME
			}
		}

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
			// FIXME
			throw new NotImplementedException();
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
			// FIXME
			throw new NotImplementedException();
		}

		public void SelectAll() {
		}

		public override string ToString() {
			// FIXME
			return base.ToString ();
		}

		public void Undo() {
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void CreateHandle() {
			base.CreateHandle ();
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
			return base.ProcessDialogKey (keyData);
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void WndProc(ref Message m) {
			switch ((Msg)m.Msg) {
				case Msg.WM_PAINT: {
					PaintEventArgs	paint_event;

#if !__MonoCS__
XplatUIWin32.Win32SetFocus(Handle);
#endif
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

				case Msg.WM_KEYDOWN: {
					switch ((Keys)(m.WParam.ToInt32())) {
						case Keys.Left: {
							if ((Control.ModifierKeys & Keys.Control) != 0) {
								document.MoveCaret(CaretDirection.WordBack);
							} else {
								document.MoveCaret(CaretDirection.CharBack);
							}
							return;
						}

						case Keys.Right: {
							if ((Control.ModifierKeys & Keys.Control) != 0) {
								document.MoveCaret(CaretDirection.WordForward);
							} else {
								document.MoveCaret(CaretDirection.CharForward);
							}
							return;
						}

						case Keys.Up: {
							document.MoveCaret(CaretDirection.LineUp);
							return;
						}

						case Keys.Down: {
							document.DumpTree(document.Root, true);
							document.MoveCaret(CaretDirection.LineDown);
							return;
						}

						case Keys.Home: {
							if ((Control.ModifierKeys & Keys.Control) != 0) {
								document.MoveCaret(CaretDirection.CtrlHome);
							} else {
								document.MoveCaret(CaretDirection.Home);
							}
							return;
						}

						case Keys.End: {
							if ((Control.ModifierKeys & Keys.Control) != 0) {
								document.MoveCaret(CaretDirection.CtrlEnd);
							} else {
								document.MoveCaret(CaretDirection.End);
							}
							return;
						}

						case Keys.Enter: {
							document.Split(document.CaretLine, document.CaretTag, document.CaretPosition);
							document.UpdateView(document.CaretLine, 2, 0);
							//document.RecalculateDocument(CreateGraphics(), document.CaretLine.line_no, document.CaretLine.line_no+1, true);
							document.MoveCaret(CaretDirection.CharForward);
							return;
						}

						case Keys.Back: {
							// delete only deletes on the line, doesn't do the combine
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
								}
							} else {
								document.DeleteChar(document.CaretTag, document.CaretPosition, false);
								document.MoveCaret(CaretDirection.CharBack);
							}
							return;
						}

						case Keys.Delete: {
							// delete only deletes on the line, doesn't do the combine
							if (document.CaretPosition == document.CaretLine.text.Length) {
								if (document.CaretLine.LineNo < document.Lines) {
									Line	line;

									line = document.GetLine(document.CaretLine.LineNo + 1);
									document.Combine(document.CaretLine, line);
									document.UpdateView(line, 1, 0);
									// Caret doesn't move
								}
							} else {
								document.DeleteChar(document.CaretTag, document.CaretPosition, true);
							}
							return;
						}
					}
					return;
				}

				case Msg.WM_CHAR: {
Console.WriteLine("Got char, inserting at cursor");
					if (m.WParam.ToInt32() >= 32) {	// FIXME, tabs should probably go through
						document.InsertCharAtCaret((char)m.WParam, true);
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
		public event EventHandler	BorderStyleChanged;
		public event EventHandler	Click;
		public event EventHandler	HideSelectionChanged;
		public event EventHandler	ModifiedChanged;
		public event EventHandler	MultilineChanged;
		public event PaintEventHandler	Paint;
		public event EventHandler	ReadOnlyChanged;
		#endregion	// Events

		#region Private Methods
		public Document Document {
			get {
				return document;
			}

			set {
				document = value;
			}
		}

static int current;

		private void PaintControl(PaintEventArgs pevent) {
Console.WriteLine("Received expose: {0}", pevent.ClipRectangle);
			// Fill background
			pevent.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(BackColor), pevent.ClipRectangle);
			pevent.Graphics.TextRenderingHint=TextRenderingHint.AntiAlias;

			// Draw the viewable document
			document.Draw(pevent.Graphics, pevent.ClipRectangle, ThemeEngine.Current.ResPool.GetSolidBrush(ForeColor));

#if Debug
			int		start;
			int		end;
			Line		line;
			int		line_no;
			LineTag		tag;
			Pen		p;

			p = new Pen(Color.Red, 1);

			// First, figure out from what line to what line we need to draw
			start = document.GetLineByPixel(pevent.ClipRectangle.Top - viewport_y, false).line_no;
			end = document.GetLineByPixel(pevent.ClipRectangle.Bottom - viewport_y, false).line_no;

			Console.WriteLine("Starting drawing on line '{0}'", document.GetLine(start));
			Console.WriteLine("Ending drawing on line '{0}'", document.GetLine(end));

			line_no = start;
			while (line_no <= end) {
				line = document.GetLine(line_no);

				if (draw_lines) {
					for (int i = 0; i < line.text.Length; i++) {
						pevent.Graphics.DrawLine(p, (int)line.widths[i] - viewport_x, line.Y - viewport_y, (int)line.widths[i] - viewport_x, line.Y + line.height  - viewport_y);
					}
				}
#endif

				line_no++;
			}
		}

		private void TextBoxBase_MouseDown(object sender, MouseEventArgs e) {
			LineTag	tag;
			Line	line;
			int	pos;

if (e.Button == MouseButtons.Middle) {
			document.PositionCaret(e.X, e.Y);
			return;
}

if (e.Button == MouseButtons.Right) {
#if Debug
	draw_lines = !draw_lines;
#endif
	((Control)sender).Invalidate();
	return;
}
			tag = document.FindTag(e.X, e.Y, out pos, false);

bool recalc_line = false;
Console.WriteLine("Click found tag {0}, character {1}", tag, pos);
line = tag.line;
switch(current) {
 case 4: recalc_line = LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, new Font("impact", 20, FontStyle.Bold, GraphicsUnit.Pixel)); break;
 case 1: recalc_line = LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, new Font("arial unicode ms", 24, FontStyle.Italic, GraphicsUnit.Pixel)); break;
 case 2: recalc_line = LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, new Font("arial", 10, FontStyle.Regular, GraphicsUnit.Pixel)); break;
 case 3: recalc_line = LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, new Font("times roman", 16, FontStyle.Underline, GraphicsUnit.Pixel)); break;
 case 0: recalc_line = LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, new Font("times roman", 64, FontStyle.Italic | FontStyle.Bold, GraphicsUnit.Pixel)); break;
 case 5: recalc_line = LineTag.FormatText(tag.line, pos, (pos+10)<line.Text.Length ? 10 : line.Text.Length - pos+1, ((TextBoxBase)sender).Font); break;
}
current++;
if (current==6) {
 current=0;
}

// Update/Recalculate what we see
document.UpdateView(line, 0);

// Make sure our caret is properly positioned and sized
document.AlignCaret();
		}

		private void TextBoxBase_MouseUp(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Middle) {
				this.caret_tag = document.FindCursor(e.X, e.Y, out caret_pos);
				XplatUI.CreateCaret(this.Handle, 2, this.caret_tag.height);
				XplatUI.SetCaretPos(this.Handle, (int)caret_tag.line.widths[caret_pos], caret_tag.line.Y + caret_tag.line.height - this.caret_tag.height);
				XplatUI.CaretVisible(this.Handle, true);
				return;
			}
		}
		#endregion	// Private Methods
	}
}
