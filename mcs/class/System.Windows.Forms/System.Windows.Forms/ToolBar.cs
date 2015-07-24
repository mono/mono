// System.Windows.Forms.ToolBar.cs
//
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
// Author:
//	Ravindra (rkumar@novell.com)
//	Mike Kestner <mkestner@novell.com>
//	Everaldo Canuto <ecanuto@novell.com>
//
// Copyright (C) 2004-2006  Novell, Inc. (http://www.novell.com)
//

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[DefaultEvent ("ButtonClick")]
	[DefaultProperty ("Buttons")]
	[Designer ("System.Windows.Forms.Design.ToolBarDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ToolBar : Control
	{
		#region Instance Variables
		private bool size_specified = false;
		private ToolBarItem current_item;
		internal ToolBarItem[] items;
		internal Size default_size;
		#endregion Instance Variables

		#region Events
		static object ButtonClickEvent = new object ();
		static object ButtonDropDownEvent = new object ();

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add { base.BackColorChanged += value; }
			remove { base.BackColorChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
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

		public event ToolBarButtonClickEventHandler ButtonClick {
			add { Events.AddHandler (ButtonClickEvent, value); }
			remove {Events.RemoveHandler (ButtonClickEvent, value); }
		}

		public event ToolBarButtonClickEventHandler ButtonDropDown {
			add { Events.AddHandler (ButtonDropDownEvent, value); }
			remove {Events.RemoveHandler (ButtonDropDownEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftChanged {
			add { base.RightToLeftChanged += value; }
			remove { base.RightToLeftChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion Events

		#region Constructor
		public ToolBar ()
		{
			background_color = ThemeEngine.Current.DefaultControlBackColor;
			foreground_color = ThemeEngine.Current.DefaultControlForeColor;
			buttons = new ToolBarButtonCollection (this);
			Dock = DockStyle.Top;
			
			GotFocus += new EventHandler (FocusChanged);
			LostFocus += new EventHandler (FocusChanged);
			MouseDown += new MouseEventHandler (ToolBar_MouseDown);
			MouseHover += new EventHandler (ToolBar_MouseHover);
			MouseLeave += new EventHandler (ToolBar_MouseLeave);
			MouseMove += new MouseEventHandler (ToolBar_MouseMove);
			MouseUp += new MouseEventHandler (ToolBar_MouseUp);
			BackgroundImageChanged += new EventHandler (ToolBar_BackgroundImageChanged);

			TabStop = false;
			
			SetStyle (ControlStyles.UserPaint, false);
			SetStyle (ControlStyles.FixedHeight, true);
			SetStyle (ControlStyles.FixedWidth, false);
		}
		#endregion Constructor

		#region protected Properties
		protected override CreateParams CreateParams {
			get { 
				CreateParams create_params = base.CreateParams;
				
				if (appearance == ToolBarAppearance.Flat) {
					create_params.Style |= (int) ToolBarStyles.TBSTYLE_FLAT;
				}
				
				return create_params;
			}
		}

		protected override ImeMode DefaultImeMode {
			get { return ImeMode.Disable; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.ToolBarDefaultSize; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override bool DoubleBuffered {
			get { return base.DoubleBuffered; }
			set { base.DoubleBuffered = value; }
		}
		#endregion

		ToolBarAppearance appearance = ToolBarAppearance.Normal;

		#region Public Properties
		[DefaultValue (ToolBarAppearance.Normal)]
		[Localizable (true)]
		public ToolBarAppearance Appearance {
			get { return appearance; }
			set {
				if (value == appearance)
					return;

				appearance = value;
				Redraw (true);
			}
		}

		bool autosize = true;

		[Browsable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DefaultValue (true)]
		[Localizable (true)]
		public override bool AutoSize {
			get { return autosize; }
			set {
				if (value == autosize)
					return;

				autosize = value;
				
				if (IsHandleCreated)
					Redraw (true);
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color BackColor {
			get { return background_color; }
			set {
				if (value == background_color)
					return;

				background_color = value;
				OnBackColorChanged (EventArgs.Empty);
				Redraw (false);
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[DefaultValue (BorderStyle.None)]
		[DispIdAttribute (-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle = value; }
		}

		ToolBarButtonCollection buttons;

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[MergableProperty (false)]
		public ToolBarButtonCollection Buttons {
			get { return buttons; }
		}

		Size button_size;

		[Localizable (true)]
		[RefreshProperties (RefreshProperties.All)]
		public Size ButtonSize {
			get {
				if (!button_size.IsEmpty)
					return button_size; 
				
				if (buttons.Count == 0)
					return new Size (39, 36);
					
				Size result = CalcButtonSize ();
				if (result.IsEmpty)
					return new Size (24, 22);
				else
					return result;
			}
			set {
				size_specified = value != Size.Empty;
				if (button_size == value)
					return;

				button_size = value;
				Redraw (true);
			}
		}

		bool divider = true;

		[DefaultValue (true)]
		public bool Divider {
			get { return divider; }
			set {
				if (value == divider)
					return;

				divider = value;
				Redraw (false);
			}
		}

		[DefaultValue (DockStyle.Top)]
		[Localizable (true)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set {
				if (base.Dock == value) {
					// Call base anyways so layout_type gets set correctly
					if (value != DockStyle.None)
						base.Dock = value;
					return;
				}
					
				if (Vertical) {
					SetStyle (ControlStyles.FixedWidth, AutoSize);
					SetStyle (ControlStyles.FixedHeight, false);
				} else {
					SetStyle (ControlStyles.FixedHeight, AutoSize);
					SetStyle (ControlStyles.FixedWidth, false);
				}
				
				LayoutToolBar ();
				
				base.Dock = value;
			}
		}

		bool drop_down_arrows = true;

		[DefaultValue (false)]
		[Localizable (true)]
		public bool DropDownArrows {
			get { return drop_down_arrows; }
			set {
				if (value == drop_down_arrows)
					return;

				drop_down_arrows = value;
				Redraw (true);
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color ForeColor {
			get { return foreground_color; }
			set {
				if (value == foreground_color)
					return;

				foreground_color = value;
				OnForeColorChanged (EventArgs.Empty);
				Redraw (false);
			}
		}

		ImageList image_list;

		[DefaultValue (null)]
		public ImageList ImageList {
			get { return image_list; }
			set { 
				if (image_list == value)
					return;
				image_list = value;
				Redraw (true);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public Size ImageSize {
			get {
				if (ImageList == null)
					return Size.Empty;

				return ImageList.ImageSize;
			}
		}

		// XXX this should probably go away and it should call
		// into Control.ImeMode instead.
		ImeMode ime_mode = ImeMode.Disable;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return ime_mode; }
			set {
				if (value == ime_mode)
					return;

				ime_mode = value;
				OnImeModeChanged (EventArgs.Empty);
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set {
				if (value == base.RightToLeft)
					return;

				base.RightToLeft = value;
				OnRightToLeftChanged (EventArgs.Empty);
			}
		}

		// Default value is "false" but after make a test in .NET we get "true" result as default.  
		bool show_tooltips = true;

		[DefaultValue (false)]
		[Localizable (true)]
		public bool ShowToolTips {
			get { return show_tooltips; }
			set { show_tooltips = value; }
		}

		[DefaultValue (false)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Bindable (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; } 
			set {
				if (value == base.Text)
					return;

				base.Text = value;
				Redraw (true);
			}
		}

		ToolBarTextAlign text_alignment = ToolBarTextAlign.Underneath;

		[DefaultValue (ToolBarTextAlign.Underneath)]
		[Localizable (true)]
		public ToolBarTextAlign TextAlign {
			get { return text_alignment; }
			set {
				if (value == text_alignment)
					return;

				text_alignment = value;
				Redraw (true);
			}
		}

		bool wrappable = true;

		[DefaultValue (true)]
		[Localizable (true)]
		public bool Wrappable {
			get { return wrappable; }
			set {
				if (value == wrappable)
					return;

				wrappable = value;
				Redraw (true);
			}
		}
		#endregion Public Properties

		#region Public Methods
		public override string ToString ()
		{
			int count = this.Buttons.Count;

			if (count == 0)
				return string.Format ("System.Windows.Forms.ToolBar, Buttons.Count: 0");
			else
				return string.Format ("System.Windows.Forms.ToolBar, Buttons.Count: {0}, Buttons[0]: {1}",
						      count, this.Buttons [0].ToString ());
		}
		#endregion Public Methods

		#region Protected Methods
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
			default_size = CalcButtonSize ();
			
			// In win32 the recalculate size only happens for not flat style
			if (appearance != ToolBarAppearance.Flat)
				Redraw (true);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				ImageList = null;

			base.Dispose (disposing);
		}

		private ToolBarButton button_for_focus = null;
		
		internal void UIAPerformClick (ToolBarButton button)
		{
			ToolBarItem previous_item = current_item;
			current_item = null;
			
			foreach (ToolBarItem item in items)
				if (item.Button == button) {
					current_item = item;
					break;
				}

			try {
				if (current_item == null)
					throw new ArgumentException ("button", "The button specified is not part of this toolbar");
				PerformButtonClick (new ToolBarButtonClickEventArgs (button));
			} finally {
				current_item = previous_item;
			}
		}
		
		void PerformButtonClick (ToolBarButtonClickEventArgs e)
		{
			// Only change pushed for ToogleButton
			if (e.Button.Style == ToolBarButtonStyle.ToggleButton) {
				if (! e.Button.Pushed)
					e.Button.Pushed = true;
				else
					e.Button.Pushed = false;
			}
			
			current_item.Pressed = false;
			current_item.Invalidate ();
			
			button_for_focus = current_item.Button;
			button_for_focus.UIAHasFocus = true;

			OnButtonClick (e);
		}

		protected virtual void OnButtonClick (ToolBarButtonClickEventArgs e)
		{			
			ToolBarButtonClickEventHandler eh = (ToolBarButtonClickEventHandler)(Events [ButtonClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnButtonDropDown (ToolBarButtonClickEventArgs e) 
		{
			ToolBarButtonClickEventHandler eh = (ToolBarButtonClickEventHandler)(Events [ButtonDropDownEvent]);
			if (eh != null)
				eh (this, e);

			if (e.Button.DropDownMenu == null)
				return;

			ShowDropDownMenu (current_item);
		}

		internal void ShowDropDownMenu (ToolBarItem item)
		{
			Point loc = new Point (item.Rectangle.X + 1, item.Rectangle.Bottom + 1);
			((ContextMenu) item.Button.DropDownMenu).Show (this, loc);

			item.DDPressed = false;
			item.Hilight = false;
			item.Invalidate ();
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			Redraw (true);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			LayoutToolBar ();
		}

		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			specified &= ~BoundsSpecified.Height;
			
			base.ScaleControl (factor, specified);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void ScaleCore (float dx, float dy)
		{
			dy = 1.0f;
			
			base.ScaleCore (dx, dy);
		}

		private int requested_size = -1;

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (Vertical) {
				if (!AutoSize && (requested_size != width) && ((specified & BoundsSpecified.Width) != BoundsSpecified.None)) 
					requested_size = width;
			} else {
				if (!AutoSize && (requested_size != height) && ((specified & BoundsSpecified.Height) != BoundsSpecified.None)) 
					requested_size = height;
			}
			
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}

		internal override bool InternalPreProcessMessage (ref Message msg)
		{
			if (msg.Msg == (int)Msg.WM_KEYDOWN) {
				Keys key_data = (Keys)msg.WParam.ToInt32();
				if (HandleKeyDown (ref msg, key_data))
					return true;
			} 
			return base.InternalPreProcessMessage (ref msg);
		}
			
		#endregion Protected Methods

		#region Private Methods
		internal int CurrentItem {
			get {
				return Array.IndexOf (items, current_item);
			}
			set {
				if (current_item != null)
					current_item.Hilight = false;

				current_item = value == -1 ? null : items [value];

				if (current_item != null)
					current_item.Hilight = true;
			}

		}

		private void FocusChanged (object sender, EventArgs args)
		{
			if (!Focused && button_for_focus != null)
				button_for_focus.UIAHasFocus = false;
			button_for_focus = null;
			
			if (Appearance != ToolBarAppearance.Flat || Buttons.Count == 0)
				return;

			ToolBarItem prelit = null;
			foreach (ToolBarItem item in items) {
				if (item.Hilight) {
					prelit = item;
					break;
				}
			}

			if (Focused && prelit == null) {
				foreach (ToolBarItem item in items) {
					if (item.Button.Enabled) {
						item.Hilight = true;
						break;
					}
				}
			} else if (prelit != null) {
				prelit.Hilight = false;
			}
		}

		private bool HandleKeyDown (ref Message msg, Keys key_data)
		{
			if (Appearance != ToolBarAppearance.Flat || Buttons.Count == 0)
				return false;

			// Handle the key as needed if the current item is a dropdownbutton.
			if (HandleKeyOnDropDown (ref msg, key_data))
				return true;

			switch (key_data) {
				case Keys.Left:
				case Keys.Up:
					HighlightButton (-1);
					return true;
				case Keys.Right:
				case Keys.Down:
					HighlightButton (1);
					return true;
				case Keys.Enter:
				case Keys.Space:
					if (current_item != null) {
						OnButtonClick (new ToolBarButtonClickEventArgs (current_item.Button));
						return true;
					}
					break;
			}

			return false;
		}

		bool HandleKeyOnDropDown (ref Message msg, Keys key_data)
		{
			if (current_item == null || current_item.Button.Style != ToolBarButtonStyle.DropDownButton ||
					current_item.Button.DropDownMenu == null)
				return false;

			Menu dropdown_menu = current_item.Button.DropDownMenu;

			if (dropdown_menu.Tracker.active) {
				dropdown_menu.ProcessCmdKey (ref msg, key_data);
				return true; // always true if the menu is active
			}

			if (key_data == Keys.Up || key_data == Keys.Down) {
				current_item.DDPressed = true;
				current_item.Invalidate ();
				OnButtonDropDown (new ToolBarButtonClickEventArgs (current_item.Button));
				return true;
			}

			return false;
		}

		void HighlightButton (int offset)
		{
			ArrayList enabled = new ArrayList ();
			int count = 0;
			int start = -1;
			ToolBarItem curr_item = null;
			foreach (ToolBarItem item in items) {
				if (item.Hilight) {
					start = count;
					curr_item = item;
				}

				if (item.Button.Enabled) {
					enabled.Add (item);
					count++;
				}
			}

			int next = (start + offset) % count;
			if (next < 0)
				next = count - 1;

			if (next == start)
				return;

			if (curr_item != null)
				curr_item.Hilight = false;

			current_item = enabled [next] as ToolBarItem;
			current_item.Hilight = true;
		}

		private void ToolBar_BackgroundImageChanged (object sender, EventArgs args)
		{
			Redraw (false, true);
		}

		private void ToolBar_MouseDown (object sender, MouseEventArgs me)
		{
			if ((!Enabled) || ((me.Button & MouseButtons.Left) == 0))
				return;

			Point loc = new Point (me.X, me.Y);

			if (ItemAtPoint (loc) == null)
				return;
			
			// Hide tooltip when left mouse button 
			if ((tip_window != null) && (tip_window.Visible) && ((me.Button & MouseButtons.Left) == MouseButtons.Left)) {
				TipDownTimer.Stop ();
				tip_window.Hide (this);
			}
			
			// draw the pushed button
			foreach (ToolBarItem item in items) {
				if (item.Button.Enabled && item.Rectangle.Contains (loc)) {
					// Mark the DropDown rect as pressed.
					// We don't redraw the dropdown rect.
					if (item.Button.Style == ToolBarButtonStyle.DropDownButton) {
						Rectangle rect = item.Rectangle;
						if (DropDownArrows) {
							rect.Width = ThemeEngine.Current.ToolBarDropDownWidth;
							rect.X = item.Rectangle.Right - rect.Width;
						}
						
						if (rect.Contains (loc)) {
							if (item.Button.DropDownMenu != null) {
								item.DDPressed = true;
								Invalidate (rect);
							}
							break;
						}
					}
					item.Pressed = true;
					item.Inside = true;
					item.Invalidate ();
					break;
				}
			}
		}

		private void ToolBar_MouseUp (object sender, MouseEventArgs me)
		{
			if ((!Enabled) || ((me.Button & MouseButtons.Left) == 0))
				return;

			Point loc = new Point (me.X, me.Y);

			// draw the normal button
			// Make a copy in case the list is modified during enumeration
			ArrayList items = new ArrayList (this.items);
			foreach (ToolBarItem item in items) {
				if (item.Button.Enabled && item.Rectangle.Contains (loc)) {
					if (item.Button.Style == ToolBarButtonStyle.DropDownButton) {
						Rectangle ddRect = item.Rectangle;
						ddRect.Width = ThemeEngine.Current.ToolBarDropDownWidth;
						ddRect.X = item.Rectangle.Right - ddRect.Width;
						if (ddRect.Contains (loc)) {
							current_item = item;
							if (item.DDPressed)
								OnButtonDropDown (new ToolBarButtonClickEventArgs (item.Button));
							continue;
						}
					}
					// Fire a ButtonClick
					current_item = item;
					if ((item.Pressed) && ((me.Button & MouseButtons.Left) == MouseButtons.Left))
						PerformButtonClick (new ToolBarButtonClickEventArgs (item.Button));
				} else if (item.Pressed) {
					item.Pressed = false;
					item.Invalidate ();
				}
			}
		}

		private ToolBarItem ItemAtPoint (Point pt)
		{
			foreach (ToolBarItem item in items)
				if (item.Rectangle.Contains (pt)) 
					return item;

			return null;
		}

		ToolTip tip_window = null;
		Timer tipdown_timer = null;

		private void PopDownTip (object o, EventArgs args)
		{
			tip_window.Hide (this);
		}

		private Timer TipDownTimer {
			get {
				if (tipdown_timer == null) {
					tipdown_timer = new Timer ();
					tipdown_timer.Enabled = false;
					tipdown_timer.Interval = 5000;
					tipdown_timer.Tick += new EventHandler (PopDownTip);
				}
				return tipdown_timer;
			}
		}

		private void ToolBar_MouseHover (object sender, EventArgs e)
		{
			if (Capture)
				return;

			if (tip_window == null)
				tip_window = new ToolTip ();

			ToolBarItem item = ItemAtPoint (PointToClient (Control.MousePosition));
			current_item = item;

			if (item == null || item.Button.ToolTipText.Length == 0)
				return;

			tip_window.Present (this, item.Button.ToolTipText);
			TipDownTimer.Start ();
		}

		private void ToolBar_MouseLeave (object sender, EventArgs e)
		{
			if (tipdown_timer != null)
				tipdown_timer.Dispose ();
			tipdown_timer = null;
			if (tip_window != null)
				tip_window.Dispose ();
			tip_window = null;

			if (!Enabled || current_item == null) 
				return;

			current_item.Hilight = false;
			current_item = null;
		}

		private void ToolBar_MouseMove (object sender, MouseEventArgs me)
		{
			if (!Enabled) 
				return;

			if (tip_window != null && tip_window.Visible) {
				TipDownTimer.Stop ();
				TipDownTimer.Start ();
			}

			Point loc = new Point (me.X, me.Y);

			if (Capture) {
				// If the button was pressed and we leave, release the 
				// button press and vice versa
				foreach (ToolBarItem item in items) {
					if (item.Pressed &&
					    (item.Inside != item.Rectangle.Contains (loc))) {
						item.Inside = item.Rectangle.Contains (loc);
						item.Hilight = false;
						break;
					}
				}
				return;
			} 

			if (current_item != null && current_item.Rectangle.Contains (loc)) {
				if (ThemeEngine.Current.ToolBarHasHotElementStyles (this)) {
					if (current_item.Hilight || (!ThemeEngine.Current.ToolBarHasHotCheckedElementStyles && current_item.Button.Pushed) || !current_item.Button.Enabled)
						return;
					current_item.Hilight = true;
				}
			} else {
				if (tip_window != null) {
					if (tip_window.Visible) {
						tip_window.Hide (this);
						TipDownTimer.Stop ();
					}
					current_item = ItemAtPoint (loc);
					if (current_item != null && current_item.Button.ToolTipText.Length > 0) {
						tip_window.Present (this, current_item.Button.ToolTipText);
						TipDownTimer.Start ();
					}
				}

				if (ThemeEngine.Current.ToolBarHasHotElementStyles (this)) {
					foreach (ToolBarItem item in items) {
						if (item.Rectangle.Contains (loc) && item.Button.Enabled) {
							current_item = item;
							if (current_item.Hilight || (!ThemeEngine.Current.ToolBarHasHotCheckedElementStyles && current_item.Button.Pushed))
								continue;
							current_item.Hilight = true;
						}
						else if (item.Hilight) {
							item.Hilight = false;
						}
					}
				}
			}
		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{
			if (GetStyle (ControlStyles.UserPaint))
				return;
				
			ThemeEngine.Current.DrawToolBar (pevent.Graphics, pevent.ClipRectangle, this);
			
			// Toolbars do not raise OnPaint unless UserPaint is set
			pevent.Handled = true;
		}

		internal void Redraw (bool recalculate)
		{
			Redraw (recalculate, true);
		}

		internal void Redraw (bool recalculate, bool force)
		{
			bool invalidate = true;
			
			if (recalculate)
				invalidate = LayoutToolBar ();

			if (force || invalidate)
				Invalidate ();
		}

		internal bool SizeSpecified {
			get { return size_specified; }
		}
		
		internal bool Vertical {
			get { return (Dock == DockStyle.Left) || (Dock == DockStyle.Right); }
		}

		internal const int text_padding = 3;

		private Size CalcButtonSize ()
		{
			if (Buttons.Count == 0)
				return Size.Empty;

			string longest_text = Buttons [0].Text;
			for (int i = 1; i < Buttons.Count; i++) {
				if (Buttons[i].Text.Length > longest_text.Length)
					longest_text = Buttons[i].Text;
			}

			Size size = Size.Empty;
			if (longest_text != null && longest_text.Length > 0) {
				SizeF sz = TextRenderer.MeasureString (longest_text, Font);
				if (sz != SizeF.Empty)
					size = new Size ((int) Math.Ceiling (sz.Width) + 2 * text_padding, (int) Math.Ceiling (sz.Height));
			}

			Size img_size = ImageList == null ? new Size (16, 16) : ImageSize;

			Theme theme = ThemeEngine.Current;
			int imgWidth = img_size.Width + 2 * theme.ToolBarImageGripWidth; 
			int imgHeight = img_size.Height + 2 * theme.ToolBarImageGripWidth;

			if (text_alignment == ToolBarTextAlign.Right) {
				size.Width = imgWidth + size.Width;
				size.Height = (size.Height > imgHeight) ? size.Height : imgHeight;
			} else {
				size.Height = imgHeight + size.Height;
				size.Width = (size.Width > imgWidth) ? size.Width : imgWidth;
			}

			size.Width += theme.ToolBarImageGripWidth;
			size.Height += theme.ToolBarImageGripWidth;
			return size;
		}

		// Flat toolbars disregard specified sizes.  Normal toolbars grow the
		// button size to be at least large enough to show the image.
		private Size AdjustedButtonSize {
			get {
				Size size;

				if (default_size.IsEmpty || Appearance == ToolBarAppearance.Normal) 
					size = ButtonSize;
				else
					size = default_size;
				
				if (size_specified) {
					if (Appearance == ToolBarAppearance.Flat)
						size = CalcButtonSize ();
					else {
						int grip = ThemeEngine.Current.ToolBarImageGripWidth;
						if (size.Width < ImageSize.Width + 2 * grip )
							size.Width = ImageSize.Width + 2 * grip;
						if (size.Height < ImageSize.Height + 2 * grip)
							size.Height = ImageSize.Height + 2 * grip;
					}
				}
				return size;
			}
		}

		private bool LayoutToolBar ()
		{
			bool changed = false;
			Theme theme = ThemeEngine.Current;
			int x = theme.ToolBarGripWidth;
			int y = theme.ToolBarGripWidth;

			Size adjusted_size = AdjustedButtonSize;
			
			int calculated_size = (Vertical ? adjusted_size.Width : adjusted_size.Height) + theme.ToolBarGripWidth;
			
			int separator_index = -1;

			items = new ToolBarItem [buttons.Count];
			
			for (int i = 0; i < buttons.Count; i++) {
				ToolBarButton button = buttons [i];
				
				ToolBarItem item = new ToolBarItem (button);
				items [i] = item;

				if (!button.Visible)
					continue;

				if (size_specified && (button.Style != ToolBarButtonStyle.Separator))
					changed = item.Layout (adjusted_size);
				else
					changed = item.Layout (Vertical, calculated_size);
				
				bool is_separator = button.Style == ToolBarButtonStyle.Separator;
				
				if (Vertical) {
					if (y + item.Rectangle.Height < Height || is_separator || !Wrappable) {
						if (item.Location.X != x || item.Location.Y != y)
							changed = true;
						item.Location = new Point (x, y);
						y += item.Rectangle.Height;
						if (is_separator)
							separator_index = i;
					} else if (separator_index > 0) {
						i = separator_index;
						separator_index = -1;
						y = theme.ToolBarGripWidth;
						x += calculated_size; 
					} else {
						y = theme.ToolBarGripWidth;
						x += calculated_size; 
						if (item.Location.X != x || item.Location.Y != y)
							changed = true;
						item.Location = new Point (x, y);
						y += item.Rectangle.Height;
					}
				} else {
					if (x + item.Rectangle.Width < Width || is_separator || !Wrappable) {
						if (item.Location.X != x || item.Location.Y != y)
							changed = true;
						item.Location = new Point (x, y);
						x += item.Rectangle.Width;
						if (is_separator)
							separator_index = i;
					} else if (separator_index > 0) {
						i = separator_index;
						separator_index = -1;
						x = theme.ToolBarGripWidth;
						y += calculated_size; 
					} else {
						x = theme.ToolBarGripWidth;
						y += calculated_size; 
						if (item.Location.X != x || item.Location.Y != y)
							changed = true;
						item.Location = new Point (x, y);
						x += item.Rectangle.Width;
					}
				}
			}
			
			if (Parent == null)
				return changed;
			
			if (Wrappable)
				calculated_size += Vertical ? x : y;
			
			if (IsHandleCreated) {
				if (Vertical)
					Width = calculated_size;
				else
					Height = calculated_size; 
			}
			
			return changed;
		}
 		#endregion Private Methods

		#region subclass
		public class ToolBarButtonCollection : IList, ICollection, IEnumerable
		{
			#region instance variables
			private ArrayList list; // ToolBarButton list
			private ToolBar owner;  // ToolBar associated to Collection
			private bool redraw;    // Flag if needs to redraw after add/remove operations
			#endregion

			#region UIA Framework Events
			static object UIACollectionChangedEvent = new object ();
			
			internal event CollectionChangeEventHandler UIACollectionChanged {
				add { owner.Events.AddHandler (UIACollectionChangedEvent, value); }
				remove { owner.Events.RemoveHandler (UIACollectionChangedEvent, value); }
			}

			internal void OnUIACollectionChanged (CollectionChangeEventArgs e)
			{
				CollectionChangeEventHandler eh
					= (CollectionChangeEventHandler) owner.Events [UIACollectionChangedEvent];
				if (eh != null)
					eh (owner, e);
			}
			#endregion

			#region constructors
			public ToolBarButtonCollection (ToolBar owner)
			{
				this.list   = new ArrayList ();
				this.owner  = owner;
				this.redraw = true;
			}
			#endregion

			#region properties
			[Browsable (false)]
			public int Count {
				get { return list.Count; }
			}

			public bool IsReadOnly {
				get { return list.IsReadOnly; }
			}

			public virtual ToolBarButton this [int index] {
				get { return (ToolBarButton) list [index]; }
				set {
					// UIA Framework Event: Button Removed
					OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, index));

					value.SetParent (owner);
					list [index] = value;
					owner.Redraw (true);

				// UIA Framework Event: Button Added
				OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, index));
				}
			}

			public virtual ToolBarButton this[string key] {
				get {
					if (string.IsNullOrEmpty (key))
						return null;
						
					foreach (ToolBarButton b in list)
						if (string.Compare (b.Name, key, true) == 0)
							return b;
							
					return null;
				}
			}

			bool ICollection.IsSynchronized {
				get { return list.IsSynchronized; }
			}

			object ICollection.SyncRoot {
				get { return list.SyncRoot; }
			}

			bool IList.IsFixedSize {
				get { return list.IsFixedSize; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set {
					if (! (value is ToolBarButton))
						throw new ArgumentException("Not of type ToolBarButton", "value");
					this [index] = (ToolBarButton) value;
				}
			}
			#endregion

			#region methods
			public int Add (string text)
			{
				ToolBarButton button = new ToolBarButton (text);
				return this.Add (button);
			}

			public int Add (ToolBarButton button)
			{
				int result;
				button.SetParent (owner);
				result = list.Add (button);
				if (redraw)
					owner.Redraw (true);

				// UIA Framework Event: Button Added
				OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, result));

				return result;
			}

			public void AddRange (ToolBarButton [] buttons)
			{
				try {
					redraw = false;
					foreach (ToolBarButton button in buttons)
						Add (button);
				}
				finally {
					redraw = true;
					owner.Redraw (true);
				}
			}

			public void Clear ()
			{
				list.Clear ();
				owner.Redraw (false);

				// UIA Framework Event: Button Cleared
				OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, -1));
			}

			public bool Contains (ToolBarButton button)
			{
				return list.Contains (button);
			}

			public virtual bool ContainsKey (string key)
			{
				return !(this[key] == null);
			}

			public IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			int IList.Add (object button)
			{
				if (! (button is ToolBarButton)) {
					throw new ArgumentException("Not of type ToolBarButton", "button");
				}

				return this.Add ((ToolBarButton) button);
			}

			bool IList.Contains (object button)
			{
				if (! (button is ToolBarButton)) {
					throw new ArgumentException("Not of type ToolBarButton", "button");
				}

				return this.Contains ((ToolBarButton) button);
			}

			int IList.IndexOf (object button)
			{
				if (! (button is ToolBarButton)) {
					throw new ArgumentException("Not of type ToolBarButton", "button");
				}

				return this.IndexOf ((ToolBarButton) button);
			}

			void IList.Insert (int index, object button)
			{
				if (! (button is ToolBarButton)) {
					throw new ArgumentException("Not of type ToolBarButton", "button");
				}

				this.Insert (index, (ToolBarButton) button);
			}

			void IList.Remove (object button)
			{
				if (! (button is ToolBarButton)) {
					throw new ArgumentException("Not of type ToolBarButton", "button");
				}

				this.Remove ((ToolBarButton) button);
			}

			public int IndexOf (ToolBarButton button)
			{
				return list.IndexOf (button);
			}

			public virtual int IndexOfKey (string key)
			{
				return IndexOf (this[key]);
			}

			public void Insert (int index, ToolBarButton button)
			{
				list.Insert (index, button);
				owner.Redraw (true);

				// UIA Framework Event: Button Added
				OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, index));
			}

			public void Remove (ToolBarButton button)
			{
				list.Remove (button);
				owner.Redraw (true);
			}

			public void RemoveAt (int index)
			{
				list.RemoveAt (index);
				owner.Redraw (true);

				// UIA Framework Event: Button Removed
				OnUIACollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, index));
			}

			public virtual void RemoveByKey (string key)
			{
				Remove (this[key]);
			}
			#endregion methods
		}
		
		#endregion subclass
	}
	
	
	// Because same button can be added to toolbar multiple times, we need to maintain
	// a list of button information for each positions. 
	internal class ToolBarItem : Component
	{
		#region Instance variables
		
		private ToolBar       toolbar;    // Parent toolbar
		private ToolBarButton button;     // Associated toolBar button 
		private Rectangle     bounds;     // Toolbar button bounds
		private Rectangle     image_rect; // Image button bounds
		private Rectangle     text_rect;  // Text button bounds

		private bool dd_pressed = false;  // to check for a mouse down on dropdown rect
		private bool inside     = false;  // to handle the mouse move event with mouse pressed
		private bool hilight    = false;  // to hilight buttons in flat style
		private bool pressed    = false;  // this is to check for mouse down on a button
		
		#endregion
		
		#region Constructors
		
		public ToolBarItem (ToolBarButton button)
		{
			this.toolbar = button.Parent;
			this.button  = button;
		}
		
		#endregion Constructors
	
		#region Properties

		public ToolBarButton Button {
			get { return this.button; }
		}
		
		public Rectangle Rectangle {
			get { 
				if (!button.Visible || toolbar == null)
					return Rectangle.Empty;

				if (button.Style == ToolBarButtonStyle.DropDownButton && toolbar.DropDownArrows) {
					Rectangle result = bounds;
					result.Width += ThemeEngine.Current.ToolBarDropDownWidth;
					return result;
				}
				 
				return bounds;
			}
			set { this.bounds = value; }
		}

		public Point Location {
			get { return bounds.Location; }
			set { bounds.Location = value; }
		}

		public Rectangle ImageRectangle {
			get {
				Rectangle result = image_rect;
				result.X += bounds.X;
				result.Y += bounds.Y;
				return result; 
			}
		}
		
		public Rectangle TextRectangle {
			get { 
				Rectangle result = text_rect;
				result.X += bounds.X;
				result.Y += bounds.Y;
				return result; 
			}
		}

		private Size TextSize {
			get {
				StringFormat text_format = new StringFormat ();
				text_format.HotkeyPrefix = HotkeyPrefix.Hide;

				SizeF sz = TextRenderer.MeasureString (button.Text, toolbar.Font, SizeF.Empty, text_format);
				if (sz == SizeF.Empty)
					return Size.Empty;
				return new Size ((int) Math.Ceiling (sz.Width) + 2 * ToolBar.text_padding, (int) Math.Ceiling (sz.Height));
			}
		}
		
		public bool Pressed {
			get { return (pressed && inside); }
			set { pressed = value; }
		}

		public bool DDPressed {
			get { return dd_pressed; }
			set { dd_pressed = value; }
		}

		public bool Inside {
			get { return inside; }
			set { inside = value; }
		}

		public bool Hilight {
			get { return hilight; }
			set {
				if (hilight == value)
					return;

				hilight = value;
				Invalidate ();
			}
		}	
		
		#endregion Properties

		#region Methods
		
		public Size CalculateSize ()
		{
			Theme theme = ThemeEngine.Current;

			int ht = toolbar.ButtonSize.Height + 2 * theme.ToolBarGripWidth;

			if (button.Style == ToolBarButtonStyle.Separator)
				return new Size (theme.ToolBarSeparatorWidth, ht);

			Size size;
			if (TextSize.IsEmpty && (button.Image == null))
				size = toolbar.default_size;
			else
				size = TextSize;
			
			Size image_size = (toolbar.ImageSize == Size.Empty) ? new Size (16, 16) : toolbar.ImageSize;

			int image_width = image_size.Width + 2 * theme.ToolBarImageGripWidth; 
			int image_height = image_size.Height + 2 * theme.ToolBarImageGripWidth; 

			if (toolbar.TextAlign == ToolBarTextAlign.Right) {
				size.Width =  image_width + size.Width;
				size.Height = (size.Height > image_height) ? size.Height : image_height;
			} else {
				size.Height = image_height + size.Height;
				size.Width = (size.Width > image_width) ? size.Width : image_width;
			}

			size.Width += theme.ToolBarGripWidth;
			size.Height += theme.ToolBarGripWidth;
			return size;
		}

		
		public bool Layout (bool vertical, int calculated_size)
		{
			if (toolbar == null || !button.Visible)
				return false;

			Size psize = toolbar.ButtonSize;
			Size size = psize;
			if ((!toolbar.SizeSpecified) || (button.Style == ToolBarButtonStyle.Separator)) {
				size = CalculateSize ();

				if (size.Width == 0 || size.Height == 0)
					size = psize;

				if (vertical)
					size.Width = calculated_size;
				else
					size.Height = calculated_size;
			}
			return Layout (size);
		}

		public bool Layout (Size size)
		{
			if (toolbar == null || !button.Visible)
				return false;

			bounds.Size = size;

			Size image_size = (toolbar.ImageSize == Size.Empty) ? new Size (16, 16) : toolbar.ImageSize;
			int grip = ThemeEngine.Current.ToolBarImageGripWidth;

			Rectangle new_image_rect, new_text_rect;
			
			if (toolbar.TextAlign == ToolBarTextAlign.Underneath) {
				new_image_rect = new Rectangle ((bounds.Size.Width - image_size.Width) / 2 - grip, 0, image_size.Width + 2 + grip, image_size.Height + 2 * grip);
				new_text_rect = new Rectangle (0, new_image_rect.Height, bounds.Size.Width, bounds.Size.Height - new_image_rect.Height - 2 * grip);
			} else {
				new_image_rect = new Rectangle (0, 0, image_size.Width + 2 * grip, image_size.Height + 2 * grip);
				new_text_rect = new Rectangle (new_image_rect.Width, 0, bounds.Size.Width - new_image_rect.Width, bounds.Size.Height - 2 * grip);
			}

			bool changed = false;

			if (new_image_rect != image_rect || new_text_rect != text_rect)
				changed = true;

			image_rect = new_image_rect;
			text_rect = new_text_rect;
			
			return changed;
		}
		
		public void Invalidate ()
		{
			if (toolbar != null)
				toolbar.Invalidate (Rectangle);
		}

		#endregion Methods
	}
}
