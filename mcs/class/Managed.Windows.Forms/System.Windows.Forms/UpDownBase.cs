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

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[Designer("System.Windows.Forms.Design.UpDownBaseDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class UpDownBase : ContainerControl {
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
			bool top_button_entered;
			bool bottom_button_entered;
			#endregion	// Local Variables

			#region Constructors
			public UpDownSpinner(UpDownBase owner)
			{
				this.owner = owner;

				mouse_pressed = 0;

				SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				SetStyle(ControlStyles.DoubleBuffer, true);
				SetStyle(ControlStyles.Opaque, true);
				SetStyle(ControlStyles.ResizeRedraw, true);
				SetStyle(ControlStyles.UserPaint, true);
				SetStyle(ControlStyles.FixedHeight, true);
				SetStyle(ControlStyles.Selectable, false);

				tmrRepeat = new Timer();

				tmrRepeat.Enabled = false;
				tmrRepeat.Interval = 10;
				tmrRepeat.Tick += new EventHandler(tmrRepeat_Tick);

				compute_rects();
			}
			#endregion	// Constructors

			#region Private & Internal Methods
			private void compute_rects ()
			{
				int top_button_height;
				int bottom_button_height;

				top_button_height = ClientSize.Height / 2;
				bottom_button_height = ClientSize.Height - top_button_height;

				top_button_rect = new Rectangle(0, 0, ClientSize.Width, top_button_height);
				bottom_button_rect = new Rectangle(0, top_button_height, ClientSize.Width, bottom_button_height);
			}

			private void redraw (Graphics graphics)
			{
				VisualStyles.PushButtonState top_button_state = VisualStyles.PushButtonState.Normal;
				VisualStyles.PushButtonState bottom_button_state = VisualStyles.PushButtonState.Normal;

				if (owner.Enabled) {
					if (mouse_pressed != 0) {
						if (mouse_pressed == 1 && top_button_rect.Contains(mouse_x, mouse_y))
							top_button_state = VisualStyles.PushButtonState.Pressed;

						if (mouse_pressed == 2 && bottom_button_rect.Contains(mouse_x, mouse_y))
							bottom_button_state = VisualStyles.PushButtonState.Pressed;
					} else {
						if (top_button_entered)
							top_button_state = VisualStyles.PushButtonState.Hot;
						if (bottom_button_entered)
							bottom_button_state = VisualStyles.PushButtonState.Hot;
					}
				} else {
					top_button_state = VisualStyles.PushButtonState.Disabled;
					bottom_button_state = VisualStyles.PushButtonState.Disabled;
				}
				ThemeEngine.Current.UpDownBaseDrawButton (graphics, top_button_rect, true, top_button_state);
				ThemeEngine.Current.UpDownBaseDrawButton (graphics, bottom_button_rect, false, bottom_button_state);
			}

			private void tmrRepeat_Tick (object sender, EventArgs e)
			{
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
			protected override void OnMouseDown (MouseEventArgs e)
			{
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

				Refresh ();
			}

			protected override void OnMouseMove (MouseEventArgs e)
			{
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

				bool new_top_button_entered = top_button_rect.Contains (e.Location);
				bool new_bottom_button_entered = bottom_button_rect.Contains (e.Location);
				
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

					top_button_entered = new_top_button_entered;
					bottom_button_entered = new_bottom_button_entered;

					Refresh ();
				} else {
					if (ThemeEngine.Current.UpDownBaseHasHotButtonStyle) {
						Region area_to_invalidate = new Region ();
						bool dirty = false;
						area_to_invalidate.MakeEmpty ();
						if (top_button_entered != new_top_button_entered) {
							top_button_entered = new_top_button_entered;
							area_to_invalidate.Union (top_button_rect);
							dirty = true;
						}
						if (bottom_button_entered != new_bottom_button_entered) {
							bottom_button_entered = new_bottom_button_entered;
							area_to_invalidate.Union (bottom_button_rect);
							dirty = true;
						}
						if (dirty)
							Invalidate (area_to_invalidate);
						area_to_invalidate.Dispose ();
					} else {
						top_button_entered = new_top_button_entered;
						bottom_button_entered = new_bottom_button_entered;
					}
				}
			}

			protected override void OnMouseUp(MouseEventArgs e)
			{
				mouse_pressed = 0;
				Capture = false;

				Refresh ();
			}

			protected override void OnMouseWheel(MouseEventArgs e)
			{
				if (e.Delta > 0)
					owner.UpButton();
				else if (e.Delta < 0)
					owner.DownButton();
			}

			protected override void OnMouseLeave (EventArgs e)
			{
				if (top_button_entered) {
					top_button_entered = false;
					if (ThemeEngine.Current.UpDownBaseHasHotButtonStyle)
						Invalidate (top_button_rect);
				}
				if (bottom_button_entered) {
					bottom_button_entered = false;
					if (ThemeEngine.Current.UpDownBaseHasHotButtonStyle)
						Invalidate (bottom_button_rect);
				}
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				redraw(e.Graphics);
			}

			protected override void OnResize(EventArgs e)
			{
				base.OnResize(e);
				compute_rects();
			}
			#endregion	// Protected Instance Methods
		}
		#endregion	// UpDownSpinner Sub-class

		internal class UpDownTextBox : TextBox {

			private UpDownBase owner;

			public UpDownTextBox (UpDownBase owner)
			{
				this.owner = owner;

				SetStyle (ControlStyles.FixedWidth, false);
				SetStyle (ControlStyles.Selectable, false);
			}


			// The following can be shown to be present by
			// adding events to both the UpDown and the
			// internal textbox.  the textbox doesn't
			// generate any, and the updown generates them
			// all instead.
			protected override void OnGotFocus (EventArgs e)
			{
				ShowSelection = true;
				owner.OnGotFocus (e);
				// doesn't chain up
			}

			protected override void OnLostFocus (EventArgs e)
			{
				ShowSelection = false;
				owner.OnLostFocus (e);
				// doesn't chain up
			}

			protected override void OnMouseDown (MouseEventArgs e)
			{
				// XXX look into whether or not the
				// mouse event args are altered in
				// some way.

				owner.OnMouseDown (e);
				base.OnMouseDown (e);
			}

			protected override void OnMouseUp (MouseEventArgs e)
			{
				// XXX look into whether or not the
				// mouse event args are altered in
				// some way.

				owner.OnMouseUp (e);
				base.OnMouseUp (e);
			}

			// XXX there are likely more events that forward up to the UpDown
		}

		#region Local Variables
		internal UpDownTextBox		txtView;
		private UpDownSpinner		spnSpinner;
		private bool			_InterceptArrowKeys = true;
		private LeftRightAlignment	_UpDownAlign;
		private bool			changing_text;
		private bool			user_edit;
		#endregion	// Local Variables

		#region Public Constructors
		public UpDownBase()
		{
			_UpDownAlign = LeftRightAlignment.Right;
			InternalBorderStyle = BorderStyle.Fixed3D;

			spnSpinner = new UpDownSpinner(this);

			txtView = new UpDownTextBox (this);
			txtView.ModifiedChanged += new EventHandler(OnChanged);
			txtView.AcceptsReturn = true;
			txtView.AutoSize = false;
			txtView.BorderStyle = BorderStyle.None;
			txtView.Location = new System.Drawing.Point(17, 17);
			txtView.TabIndex = TabIndex;

			spnSpinner.Width = 16;
			spnSpinner.Dock = DockStyle.Right;
			
			txtView.Dock = DockStyle.Fill;
			
			SuspendLayout ();
			Controls.Add (spnSpinner);
			Controls.Add (txtView);	
			ResumeLayout ();

			Height = PreferredHeight;
			base.BackColor = txtView.BackColor;

			TabIndexChanged += new EventHandler (TabIndexChangedHandler);
			
			txtView.KeyDown += new KeyEventHandler(OnTextBoxKeyDown);
			txtView.KeyPress += new KeyPressEventHandler(OnTextBoxKeyPress);
//			txtView.LostFocus += new EventHandler(OnTextBoxLostFocus);
			txtView.Resize += new EventHandler(OnTextBoxResize);
			txtView.TextChanged += new EventHandler(OnTextBoxTextChanged);

			// So the child controls don't get auto selected when the updown is selected
			auto_select_child = false;
			SetStyle(ControlStyles.FixedHeight, true);
			SetStyle(ControlStyles.Selectable, true);
			SetStyle (ControlStyles.Opaque | ControlStyles.ResizeRedraw, true);
			SetStyle (ControlStyles.StandardClick | ControlStyles.UseTextForAccessibility, false);
		}
		#endregion

		#region UIA Framework Events
		static object UIAUpButtonClickEvent = new object ();

		internal event EventHandler UIAUpButtonClick {
			add { Events.AddHandler (UIAUpButtonClickEvent, value); }
			remove { Events.RemoveHandler (UIAUpButtonClickEvent, value); }
		}

		internal void OnUIAUpButtonClick (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIAUpButtonClickEvent];
			if (eh != null)
				eh (this, e);
		}

		static object UIADownButtonClickEvent = new object ();

		internal event EventHandler UIADownButtonClick {
			add { Events.AddHandler (UIADownButtonClickEvent, value); }
			remove { Events.RemoveHandler (UIADownButtonClickEvent, value); }
		}

		internal void OnUIADownButtonClick (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIADownButtonClickEvent];
			if (eh != null)
				eh (this, e);
		}
		#endregion

		#region Private Methods
		private void TabIndexChangedHandler (object sender, EventArgs e)
		{
			txtView.TabIndex = TabIndex;
		}

		internal override void OnPaintInternal (PaintEventArgs e)
		{
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
		public new Size AutoScrollMargin {
			get { return base.AutoScrollMargin; }
			set { base.AutoScrollMargin = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Size AutoScrollMinSize {
			get { return base.AutoScrollMinSize; }
			set { base.AutoScrollMinSize = value; }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
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

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
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

		public override ContextMenuStrip ContextMenuStrip {
			get { return base.ContextMenuStrip; }
			set {
				base.ContextMenuStrip = value;
				txtView.ContextMenuStrip = value;
				spnSpinner.ContextMenuStrip = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new DockPaddingEdges DockPadding {
			get { return base.DockPadding; }
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

		public override Size MaximumSize {
			get { return base.MaximumSize; }
			set { base.MaximumSize = new Size (value.Width, 0); }
		}
		
		public override Size MinimumSize {
			get { return base.MinimumSize; }
			set { base.MinimumSize = new Size (value.Width, 0); }
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
				if (txtView != null) {
					return txtView.Text;
				}
				return "";
			}
			set {
				txtView.Text = value;
				if (this.UserEdit)
					ValidateEditText();

				txtView.SelectionLength = 0;
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
				if (_UpDownAlign != value) {
					_UpDownAlign = value;
					
					if (value == LeftRightAlignment.Left)
						spnSpinner.Dock = DockStyle.Left;
					else
						spnSpinner.Dock = DockStyle.Right;
				}
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
		public abstract void DownButton ();
		public void Select(int start, int length)
		{
			txtView.Select(start, length);
		}

		public abstract void UpButton ();
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual void OnChanged (object source, EventArgs e)
		{
		}

		protected override void OnFontChanged (EventArgs e)
		{
			txtView.Font = this.Font;
			Height = PreferredHeight;
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			base.OnLayout(e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);
		}

		protected override void OnMouseUp (MouseEventArgs mevent)
		{
			base.OnMouseUp (mevent);
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			if (e.Delta > 0)
				UpButton();
			else if (e.Delta < 0)
				DownButton();
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
		}

		protected virtual void OnTextBoxKeyDown (object source, KeyEventArgs e)
		{
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

		protected virtual void OnTextBoxKeyPress (object source, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r') {
				e.Handled = true;
				ValidateEditText();
			}
			OnKeyPress(e);
		}

		protected virtual void OnTextBoxLostFocus (object source, EventArgs e)
		{
			if (UserEdit) {
				ValidateEditText();
			}
		}

		protected virtual void OnTextBoxResize (object source, EventArgs e)
		{
			// compute the new height, taking the border into account
			Height = PreferredHeight;

			// let anchoring reposition the controls
		}

		protected virtual void OnTextBoxTextChanged (object source, EventArgs e)
		{
			if (changing_text)
				ChangingText = false;
			else
				UserEdit = true;

			OnTextChanged(e);
		}

		internal override void SetBoundsCoreInternal(int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCoreInternal (x, y, width, Math.Min (width, PreferredHeight), specified);
		}

		protected abstract void UpdateEditText ();

		protected virtual void ValidateEditText ()
		{
			// to be overridden by subclassers
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void WndProc (ref Message m)
		{
			switch((Msg) m.Msg) {
			case Msg.WM_KEYUP:
			case Msg.WM_KEYDOWN:
			case Msg.WM_CHAR:
				XplatUI.SendMessage (txtView.Handle, (Msg) m.Msg, m.WParam, m.LParam);
				break;
			case Msg.WM_SETFOCUS:
				ActiveControl = txtView;
				break;
			case Msg.WM_KILLFOCUS:
				ActiveControl = null;
				break;
			default:
				base.WndProc (ref m);
				break;
			}
		}
		#endregion	// Protected Instance Methods

		#region Events
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

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

		[Browsable (false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MouseEnter {
			add { base.MouseEnter += value; }
			remove { base.MouseEnter -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MouseHover {
			add { base.MouseHover += value; }
			remove { base.MouseHover -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler MouseLeave {
			add { base.MouseLeave += value; }
			remove { base.MouseLeave -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseMove {
			add { base.MouseMove += value; }
			remove { base.MouseMove -= value; }
		}
		#endregion	// Events
	}
}
