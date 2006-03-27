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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Jonathan Gilbert	<logic@deltaq.org>
//
// Integration into MWF:
//	Peter Bartok		<pbartok@novell.com>
//

// COMPLETE

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	[Designer("System.Windows.Forms.Design.UpDownBaseDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class UpDownBase : System.Windows.Forms.ContainerControl {
		#region UpDownSpinner Sub-class
		internal sealed class UpDownSpinner : Control {
			#region	Local Variables
			private const int	InitialRepeatDelay = 50;
			private UpDownBase	owner;
			private Timer		tmrRepeat;
			private Rectangle	top_button_rect;
			private Rectangle	bottom_button_rect;
			private int		mouse_pressed;
			private int		mouse_x;
			private int		mouse_y;
			private int		repeat_delay;
			private int		repeat_counter;
			#endregion	// Local Variables

			#region Constructors
			public UpDownSpinner(UpDownBase owner) {
				this.owner = owner;

				mouse_pressed = 0;

				this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				this.SetStyle(ControlStyles.DoubleBuffer, true);
				this.SetStyle(ControlStyles.Opaque, true);
				this.SetStyle(ControlStyles.ResizeRedraw, true);
				this.SetStyle(ControlStyles.UserPaint, true);
				this.SetStyle(ControlStyles.Selectable, false);

				tmrRepeat = new Timer();

				tmrRepeat.Enabled = false;
				tmrRepeat.Interval = 10;
				tmrRepeat.Tick += new EventHandler(tmrRepeat_Tick);

				compute_rects();
			}
			#endregion	// Constructors

			#region Private & Internal Methods
			private void compute_rects() {
				int top_button_height;
				int bottom_button_height;

				top_button_height = ClientSize.Height / 2;
				bottom_button_height = ClientSize.Height - top_button_height;

				top_button_rect = new Rectangle(0, 0, ClientSize.Width, top_button_height);
				bottom_button_rect = new Rectangle(0, top_button_height, ClientSize.Width, bottom_button_height);
			}

			private void redraw(Graphics graphics) {
				ButtonState top_button_state;
				ButtonState bottom_button_state;

				top_button_state = bottom_button_state = ButtonState.Normal;

				if (mouse_pressed != 0) {
					if ((mouse_pressed == 1) && top_button_rect.Contains(mouse_x, mouse_y)) {
						top_button_state = ButtonState.Pushed;
					}

					if ((mouse_pressed == 2) && bottom_button_rect.Contains(mouse_x, mouse_y)) {
						bottom_button_state = ButtonState.Pushed;
					}
				}

				ControlPaint.DrawScrollButton(graphics, top_button_rect, ScrollButton.Up, top_button_state);
				ControlPaint.DrawScrollButton(graphics, bottom_button_rect, ScrollButton.Down, bottom_button_state);
			}

			private void tmrRepeat_Tick(object sender, EventArgs e) {
				if (repeat_delay > 1) {
					repeat_counter++;

					if (repeat_counter < repeat_delay) {
						return;
					}

					repeat_counter = 0;
					repeat_delay = (repeat_delay * 3 / 4);
				}

				if (mouse_pressed == 0) {
					tmrRepeat.Enabled = false;
				}

				if ((mouse_pressed == 1) && top_button_rect.Contains(mouse_x, mouse_y)) {
					owner.UpButton();
				}

				if ((mouse_pressed == 2) && bottom_button_rect.Contains(mouse_x, mouse_y)) {
					owner.DownButton();
				}
			}
			#endregion	// Private & Internal Methods

			#region Protected Instance Methods
			protected override void OnMouseDown(MouseEventArgs e) {
				this.Select(owner.txtView);

				if (e.Button != MouseButtons.Left) {
					return;
				}

				if (top_button_rect.Contains(e.X, e.Y)) {
					mouse_pressed = 1;
					owner.UpButton();
				} else if (bottom_button_rect.Contains(e.X, e.Y)) {
					mouse_pressed = 2;
					owner.DownButton();
				}

				mouse_x = e.X;
				mouse_y = e.Y;
				Capture = true;

				tmrRepeat.Enabled = true;
				repeat_counter = 0;
				repeat_delay = InitialRepeatDelay;

				using (Graphics g = CreateGraphics()) {
					redraw(g);
				}
			}

			protected override void OnMouseMove(MouseEventArgs e) {
				ButtonState before, after;

				before = ButtonState.Normal;
				if ((mouse_pressed == 1) && top_button_rect.Contains(mouse_x, mouse_y))
					before = ButtonState.Pushed;
				if ((mouse_pressed == 2) && bottom_button_rect.Contains(mouse_x, mouse_y))
					before = ButtonState.Pushed;

				mouse_x = e.X;
				mouse_y = e.Y;

				after = ButtonState.Normal;
				if ((mouse_pressed == 1) && top_button_rect.Contains(mouse_x, mouse_y))
					after = ButtonState.Pushed;
				if ((mouse_pressed == 2) && bottom_button_rect.Contains(mouse_x, mouse_y))
					after = ButtonState.Pushed;

				if (before != after) {
					if (after == ButtonState.Pushed) {
						tmrRepeat.Enabled = true;
						repeat_counter = 0;
						repeat_delay = InitialRepeatDelay;

						// fire off one right now too for good luck
						if (mouse_pressed == 1)
							owner.UpButton();
						if (mouse_pressed == 2)
							owner.DownButton();
					}
					else
						tmrRepeat.Enabled = false;

					using (Graphics g = CreateGraphics()) {
						redraw(g);
					}
				}
			}

			protected override void OnMouseUp(MouseEventArgs e) {
				mouse_pressed = 0;
				Capture = false;

				using (Graphics g = CreateGraphics()) {
					redraw(g);
				}
			}

			protected override void OnMouseWheel(MouseEventArgs e) {
				if (e.Delta > 0)
					owner.UpButton();
				else if (e.Delta < 0)
					owner.DownButton();
			}

			protected override void OnPaint(PaintEventArgs e) {
				redraw(e.Graphics);
			}

			protected override void OnResize(EventArgs e) {
				base.OnResize(e);
				compute_rects();
			}
			#endregion	// Protected Instance Methods
		}
		#endregion	// UpDownSpinner Sub-class

		#region Local Variables
		internal TextBox		txtView;
		private UpDownSpinner		spnSpinner;
		private bool			_InterceptArrowKeys = true;
		private LeftRightAlignment	_UpDownAlign;
		private bool			changing_text;
		private bool			user_edit;
		#endregion	// Local Variables

		#region Public Constructors
		public UpDownBase() {
			_UpDownAlign = LeftRightAlignment.Right;
			border_style = BorderStyle.Fixed3D;

			spnSpinner = new UpDownSpinner(this);

			txtView = new FixedSizeTextBox();
			txtView.ModifiedChanged += new EventHandler(OnChanged);
			txtView.AcceptsReturn = true;
			txtView.AutoSize = false;
			txtView.BorderStyle = BorderStyle.None;
			txtView.Location = new System.Drawing.Point(17, 17);
			txtView.TabIndex = TabIndex;

			SuspendLayout ();
			Controls.AddImplicit (txtView);
			Controls.AddImplicit (spnSpinner);
			ResumeLayout ();

			this.ActiveControl = txtView;

			Height = PreferredHeight;
			base.BackColor = txtView.BackColor;

			GotFocus += new EventHandler (GotFocusHandler);
			TabIndexChanged += new EventHandler (TabIndexChangedHandler);
			
			txtView.MouseWheel += new MouseEventHandler(txtView_MouseWheel);
			txtView.KeyDown += new KeyEventHandler(OnTextBoxKeyDown);
			txtView.KeyPress += new KeyPressEventHandler(OnTextBoxKeyPress);
			txtView.LostFocus += new EventHandler(OnTextBoxLostFocus);
			txtView.Resize += new EventHandler(OnTextBoxResize);
			txtView.TextChanged += new EventHandler(OnTextBoxTextChanged);

			txtView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.Paint +=new PaintEventHandler(UpDownBase_Paint);

			SetStyle(ControlStyles.FixedHeight, true);

			UpdateEditText();
		}
		#endregion

		#region Private Methods
		void reseat_controls() {
			int text_displacement = 0;

			int spinner_width = 16;
			//int spinner_width = ClientSize.Height;

			if (_UpDownAlign == LeftRightAlignment.Left) {
				spnSpinner.Bounds = new Rectangle(0, 0, spinner_width, ClientSize.Height);
				text_displacement = spnSpinner.Width;

				spnSpinner.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			} else {
				spnSpinner.Bounds = new Rectangle(ClientSize.Width - spinner_width, 0, spinner_width, ClientSize.Height);

				spnSpinner.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			}
			
			txtView.Bounds = new Rectangle(text_displacement, 0, ClientSize.Width - spinner_width, Height);
		}

		private void txtView_MouseWheel(object sender, MouseEventArgs e) {
			if (e.Delta > 0) {
				UpButton();
			} else if (e.Delta < 0) {
				DownButton();
			}
		}

		private void GotFocusHandler (object sender, EventArgs e)
		{
			txtView.Focus ();
		}

		private void TabIndexChangedHandler (object sender, EventArgs e)
		{
			txtView.TabIndex = TabIndex;
		}

		private void UpDownBase_Paint(object sender, PaintEventArgs e) {
			e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(BackColor), ClientRectangle);
		}
		#endregion	// Private Methods

		#region Public Instance Properties
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AutoScroll {
			get {
				return base.AutoScroll;
			}

			set {
				base.AutoScroll = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Size AutoScrollMargin {
			get {
				return base.AutoScrollMargin;
			}

			set {
				base.AutoScrollMargin = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Size AutoScrollMinSize {
			get {
				return base.AutoScrollMinSize;
			}

			set {
				base.AutoScrollMinSize = value;
			}
		}

		public override Color BackColor {
			get {
				return base.BackColor;
			}

			set {
				base.BackColor = value;
				txtView.BackColor = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
				txtView.BackgroundImage = value;
			}
		}


		[DefaultValue(BorderStyle.Fixed3D)]
		[DispId(-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle = value; }
		}

		public override ContextMenu ContextMenu {
			get {
				return base.ContextMenu;
			}
			set {
				base.ContextMenu = value;
				txtView.ContextMenu = value;
				spnSpinner.ContextMenu = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DockPaddingEdges DockPadding {
			get {
				return base.DockPadding;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool Focused {
			get {
				return txtView.Focused;
			}
		}

		public override Color ForeColor {
			get {
				return base.ForeColor;
			}
			set {
				base.ForeColor = value;
				txtView.ForeColor = value;
			}
		}

		[DefaultValue(true)]
		public bool InterceptArrowKeys {
			get {
				return _InterceptArrowKeys;
			}
			set {
				_InterceptArrowKeys = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public int PreferredHeight {
			get {
				// For some reason, the TextBox's PreferredHeight does not
				// change when the Font property is assigned. Without a
				// border, it will always be Font.Height anyway.
				//int text_box_preferred_height = (txtView != null) ? txtView.PreferredHeight : Font.Height;
				int text_box_preferred_height = Font.Height;

				switch (border_style) {
					case BorderStyle.FixedSingle:
					case BorderStyle.Fixed3D:
						text_box_preferred_height += 3; // magic number? :-)

						return text_box_preferred_height + 4;

					case BorderStyle.None:
					default:
						return text_box_preferred_height;
				}
			}
		}

		[DefaultValue(false)]
		public bool ReadOnly {
			get {
				return txtView.ReadOnly;
			}
			set {
				txtView.ReadOnly = value;
			}
		}

		[Localizable(true)]
		public override string Text {
			get {
				return txtView.Text;
			}
			set {
				bool suppress_validation = changing_text;

				txtView.Text = value;

				if (!suppress_validation)
					ValidateEditText();
			}
		}

		[DefaultValue(HorizontalAlignment.Left)]
		[Localizable(true)]
		public HorizontalAlignment TextAlign {
			get {
				return txtView.TextAlign;
			}
			set{
				txtView.TextAlign = value;
			}
		}

		[DefaultValue(LeftRightAlignment.Right)]
		[Localizable(true)]
		public LeftRightAlignment UpDownAlign {
			get {
				return _UpDownAlign;
			}
			set {
				_UpDownAlign = value;

				reseat_controls();
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected bool ChangingText {
			get {
				return changing_text;
			}
			set {
				changing_text = value;
			}
		}

		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size(120, this.PreferredHeight);
			}
		}

		protected bool UserEdit {
			get {
				return user_edit;
			}
			set {
				user_edit = value;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public abstract void DownButton();
		public void Select(int start, int length) {
			txtView.Select(start, length);
		}

		public abstract void UpButton();
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void Dispose(bool disposing) {
			if (disposing) {
				txtView.Dispose();
				txtView = null;

				spnSpinner.Dispose();
				spnSpinner = null;
			}
			base.Dispose (disposing);
		}

		[MonoTODO]
		protected virtual void OnChanged(object source, EventArgs e) {
			// FIXME
		}

		protected override void OnFontChanged(EventArgs e) {
			txtView.Font = this.Font;
			Height = PreferredHeight;
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnLayout(LayoutEventArgs e) {
			base.OnLayout(e);
		}

		protected override void OnMouseWheel(MouseEventArgs e) {
			// prevent this event from firing twice for the same mouse action!
			if (GetChildAtPoint(new Point(e.X, e.Y)) == null)
				txtView_MouseWheel(null, e);
		}

		protected virtual void OnTextBoxKeyDown(object source, KeyEventArgs e) {
			if (_InterceptArrowKeys) {
				if ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down)) {
					e.Handled = true;

					if (e.KeyCode == Keys.Up)
						UpButton();
					if (e.KeyCode == Keys.Down)
						DownButton();
				}
			}

			OnKeyDown(e);
		}

		protected virtual void OnTextBoxKeyPress(object source, KeyPressEventArgs e) {
			if (e.KeyChar == '\r') {
				e.Handled = true;
				ValidateEditText();
			}
			OnKeyPress(e);
		}

		protected virtual void OnTextBoxLostFocus(object source, EventArgs e) {
			if (user_edit) {
				ValidateEditText();
			}
		}

		protected virtual void OnTextBoxResize(object source, EventArgs e) {
			// compute the new height, taking the border into account
			Height = PreferredHeight;

			// let anchoring reposition the controls
		}

		protected virtual void OnTextBoxTextChanged(object source, EventArgs e) {
			if (changing_text) {
				ChangingText = false;
			} else {
				UserEdit = true;
			}

			OnTextChanged(e);
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			base.SetBoundsCore(x, y, width, height, specified);

			if ((specified & BoundsSpecified.Size) != BoundsSpecified.None) {
				reseat_controls();
			}
		}

		protected abstract void UpdateEditText();

		protected virtual void ValidateEditText() {
			// to be overridden by subclassers
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
		}
		#endregion	// Protected Instance Methods

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MouseEnter;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MouseHover;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MouseLeave;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseMove;
		#endregion	// Events
	}
}
