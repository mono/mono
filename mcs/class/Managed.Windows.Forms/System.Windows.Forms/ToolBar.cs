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
//
// TODO:
//   - Tooltip
//
// Copyright (C) 2004-2006  Novell, Inc. (http://www.novell.com)
//


// NOT COMPLETE

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{	
	[DefaultEvent ("ButtonClick")]
	[DefaultProperty ("Buttons")]
	[Designer ("System.Windows.Forms.Design.ToolBarDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ToolBar : Control
	{
		#region Instance Variables
		bool size_specified = false;
		ToolBarButton current_button;
		#endregion Instance Variables

		#region Events
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

		public event ToolBarButtonClickEventHandler ButtonClick;
		public event ToolBarButtonClickEventHandler ButtonDropDown;

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
			dock_style = DockStyle.Top;
			
			GotFocus += new EventHandler (FocusChanged);
			LostFocus += new EventHandler (FocusChanged);
			MouseDown += new MouseEventHandler (ToolBar_MouseDown);
			MouseHover += new EventHandler (ToolBar_MouseHover);
			MouseLeave += new EventHandler (ToolBar_MouseLeave);
			MouseMove += new MouseEventHandler (ToolBar_MouseMove);
			MouseUp += new MouseEventHandler (ToolBar_MouseUp);

			SetStyle (ControlStyles.UserPaint, false);
			SetStyle (ControlStyles.FixedHeight, true);
		}
		#endregion Constructor

		#region protected Properties
		protected override CreateParams CreateParams 
		{
			get { return base.CreateParams; }
		}

		protected override ImeMode DefaultImeMode {
			get { return ImeMode.Disable; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.ToolBarDefaultSize; }
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

		[DefaultValue (true)]
		[Localizable (true)]
		public bool AutoSize {
			get { return autosize; }
			set {
				if (value == autosize)
					return;

				autosize = value;
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
			get { return background_image; }
			set {
				if (value == background_image)
					return;

				background_image = value;
				OnBackgroundImageChanged (EventArgs.Empty);
				Redraw (false);
			}
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
				if (button_size.IsEmpty) {
					if (buttons.Count == 0)
						return new Size (24, 22);
					Size result = CalcButtonSize ();
					if (result.IsEmpty)
						return new Size (24, 22);
					else
						return result;
				}
				return button_size;
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
			set { base.Dock = value; } 
		}

		bool drop_down_arrows = false;

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

		ImeMode ime_mode;

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

		bool show_tooltips = false;

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
			get { return text; } 
			set {
				if (value == text)
					return;

				text = value;
				Redraw (true);
				OnTextChanged (EventArgs.Empty);
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
				return string.Format ("System.Windows.Forms.ToolBar, Button.Count: 0");
			else
				return string.Format ("System.Windows.Forms.ToolBar, Button.Count: {0}, Buttons[0]: {1}",
						      count, this.Buttons [0].ToString ());
		}
		#endregion Public Methods

		#region Protected Methods
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				ImageList = null;

			base.Dispose (disposing);
		}

		protected virtual void OnButtonClick (ToolBarButtonClickEventArgs e)
		{
			if (e.Button.Style == ToolBarButtonStyle.ToggleButton) {
				if (! e.Button.Pushed)
					e.Button.Pushed = true;
				else
					e.Button.Pushed = false;
			}
			e.Button.pressed = false;

			e.Button.InvalidateBorder ();

			if (ButtonClick != null)
				ButtonClick (this, e);
		}

		protected virtual void OnButtonDropDown (ToolBarButtonClickEventArgs e) 
		{
			if (ButtonDropDown != null)
				ButtonDropDown (this, e);

			if (e.Button.DropDownMenu == null)
				return;

			Point loc = new Point (e.Button.Rectangle.X + 1, e.Button.Rectangle.Bottom + 1);
			((ContextMenu) e.Button.DropDownMenu).Show (this, loc);

			e.Button.dd_pressed = false;
			Invalidate (e.Button.Rectangle);
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

			if (Width <= 0 || Height <= 0 || !Visible)
				return;

			Redraw (true, background_image != null);
		}

		bool height_specified = false;
		int requested_height = -1;

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			if ((specified & BoundsSpecified.Height) != 0) {
				requested_height = height;
				height_specified = true;
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
				if (HandleKeyDown (key_data))
					return true;
			} 
			return base.InternalPreProcessMessage (ref msg);
		}
			
		#endregion Protected Methods

		#region Private Methods
		private void FocusChanged (object sender, EventArgs args)
		{
			if (Appearance != ToolBarAppearance.Flat || Buttons.Count == 0)
				return;

			ToolBarButton prelit = null;
			foreach (ToolBarButton b in Buttons)
				if (b.Hilight) {
					prelit = b;
					break;
				}

			if (Focused && prelit == null)
				foreach (ToolBarButton btn in Buttons) {
					if (btn.Enabled) {
						btn.Hilight = true;
						break;
					}
				}
			else if (prelit != null)
				prelit.Hilight = false;
		}

		private bool HandleKeyDown (Keys key_data)
		{
			if (Appearance != ToolBarAppearance.Flat || Buttons.Count == 0)
				return false;

			switch (key_data) {
			case Keys.Left:
			case Keys.Up:
				HighlightButton (-1);
				return true;
			case Keys.Right:
			case Keys.Down:
				HighlightButton (1);
				return true;
			default:
				return false;
			}
		}

		void HighlightButton (int offset)
		{
			ArrayList enabled = new ArrayList ();
			int count = 0;
			int start = -1;
			ToolBarButton curr_button = null;
			foreach (ToolBarButton btn in Buttons) {
				if (btn.Hilight) {
					start = count;
					curr_button = btn;
				}

				if (btn.Enabled) {
					enabled.Add (btn);
					count++;
				}
			}

			int next = (start + offset) % count;
			if (next < 0)
				next = count - 1;

			if (next == start)
				return;

			if (curr_button != null)
				curr_button.Hilight = false;
			(enabled [next] as ToolBarButton).Hilight = true;
		}

		private void ToolBar_MouseDown (object sender, MouseEventArgs me)
		{
			if (!Enabled) 
				return;

			Point loc = new Point (me.X, me.Y);

			if (ButtonAtPoint (loc) == null)
				return;
			
			// Hide tooltip when left mouse button 
			if ((tip_window != null) && (tip_window.Visible) && ((me.Button & MouseButtons.Left) == MouseButtons.Left)) {
				TipDownTimer.Stop ();
				tip_window.Hide ();
			}
			
			// draw the pushed button
			foreach (ToolBarButton button in buttons) {
				if (button.Enabled && button.Rectangle.Contains (loc)) {
					// Mark the DropDown rect as pressed.
					// We don't redraw the dropdown rect.
					if (button.Style == ToolBarButtonStyle.DropDownButton) {
						Rectangle rect = button.Rectangle;
						rect.Width = ThemeEngine.Current.ToolBarDropDownWidth;
						rect.X = button.Rectangle.Right - rect.Width;
						if (rect.Contains (loc)) {
							if (button.DropDownMenu != null) {
								button.dd_pressed = true;
								Invalidate (rect);
							}
							break;
						}
					} else if ((me.Button & MouseButtons.Left) == MouseButtons.Left) {
						button.pressed = true;
						button.inside = true;
						button.InvalidateBorder ();
						break;
					}
				}
			}
		}

		private void ToolBar_MouseUp (object sender, MouseEventArgs me)
		{
			if (!Enabled) 
				return;

			Point loc = new Point (me.X, me.Y);

			// draw the normal button
			// Make a copy in case the list is modified during enumeration
			ArrayList buttons = new ArrayList (this.buttons);
			foreach (ToolBarButton button in buttons) {
				if (button.Enabled && button.Rectangle.Contains (loc)) {
					if (button.Style == ToolBarButtonStyle.DropDownButton) {
						Rectangle ddRect = button.Rectangle;
						ddRect.Width = ThemeEngine.Current.ToolBarDropDownWidth;
						ddRect.X = button.Rectangle.Right - ddRect.Width;
						if (ddRect.Contains (loc)) {
							if (button.dd_pressed)
								OnButtonDropDown (new ToolBarButtonClickEventArgs (button));
							continue;
						}
					}
					// Fire a ButtonClick
					if ((button.pressed) && ((me.Button & MouseButtons.Left) == MouseButtons.Left))
						OnButtonClick (new ToolBarButtonClickEventArgs (button));
				} else if (button.pressed) {
					button.pressed = false;
					button.InvalidateBorder ();
				}
			}
		}

		private ToolBarButton ButtonAtPoint (Point pt)
		{
			foreach (ToolBarButton button in buttons)
				if (button.Rectangle.Contains (pt)) 
					return button;

			return null;
		}

		ToolTip.ToolTipWindow tip_window = null;
		Timer tipdown_timer = null;

		private void PopDownTip (object o, EventArgs args)
		{
			tip_window.Hide ();
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
				tip_window = new ToolTip.ToolTipWindow ();

			ToolBarButton btn = ButtonAtPoint (PointToClient (Control.MousePosition));
			current_button = btn;

			if (btn == null || btn.ToolTipText.Length == 0)
				return;

			tip_window.Present (this, btn.ToolTipText);
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

			if (!Enabled || current_button == null) 
				return;

			current_button.Hilight = false;
			current_button = null;
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
				foreach (ToolBarButton button in buttons) {
					if (button.pressed &&
					    (button.inside != button.Rectangle.Contains (loc))) {
						button.inside = button.Rectangle.Contains (loc);
						button.Hilight = false;
						break;
					}
				}
				return;
			} 

			if (current_button != null && current_button.Rectangle.Contains (loc)) {
				if (appearance == ToolBarAppearance.Flat) {
					if (current_button.Hilight || current_button.Pushed || !current_button.Enabled)
						return;
					current_button.Hilight = true;
				}
			} else {
				if (tip_window != null) {
					if (tip_window.Visible) {
						tip_window.Hide ();
						TipDownTimer.Stop ();
					}
					current_button = ButtonAtPoint (loc);
					if (current_button != null && current_button.ToolTipText.Length > 0) {
						tip_window.Present (this, current_button.ToolTipText);
						TipDownTimer.Start ();
					}
				}

				if (appearance == ToolBarAppearance.Flat) {
					foreach (ToolBarButton button in buttons) {
						if (button.Rectangle.Contains (loc) && button.Enabled) {
							current_button = button;
							if (current_button.Hilight || current_button.Pushed)
								continue;
							current_button.Hilight = true;
						}
						else if (button.Hilight) {
							button.Hilight = false;
						}
					}
				}
			}
		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{
			ThemeEngine.Current.DrawToolBar (pevent.Graphics, pevent.ClipRectangle, this);
		}

		internal void Redraw (bool recalculate)
		{
			Redraw (recalculate, true);
		}

		internal void Redraw (bool recalculate, bool force)
		{
			bool invalidate = true;
			if (recalculate) {
				invalidate = Layout ();
			}

			if (force || invalidate)
				Invalidate ();
		}

		internal bool SizeSpecified {
			get { return size_specified; }
		}

		const int text_padding = 3;

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
				SizeF sz = DeviceContext.MeasureString (longest_text, Font);
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
		Size AdjustedButtonSize {
			get {
				Size size = ButtonSize;
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

		bool Layout ()
		{
			bool changed = false;
			Theme theme = ThemeEngine.Current;
			int x = theme.ToolBarGripWidth;
			int y = theme.ToolBarGripWidth;

			Size button_size = AdjustedButtonSize;

			int ht = button_size.Height + theme.ToolBarGripWidth;

			if (Wrappable && Parent != null) {
				int separator_index = -1;

				for (int i = 0; i < buttons.Count; i++) {
					ToolBarButton button = buttons [i];

					if (!button.Visible)
						continue;

					if (size_specified && (button.Style != ToolBarButtonStyle.Separator)) {
						if (button.Layout (button_size))
							changed = true;
					}
					else {
						if (button.Layout ())
							changed = true;
					}

					bool is_separator = button.Style == ToolBarButtonStyle.Separator;

					if (x + button.Rectangle.Width < Width || is_separator) {
						if (button.Location.X != x || button.Location.Y != y)
							changed = true;
						button.Location = new Point (x, y);
						x += button.Rectangle.Width;
						if (is_separator)
							separator_index = i;
					} else if (separator_index > 0) { 
						i = separator_index;
						separator_index = -1;
						x = theme.ToolBarGripWidth;
						y += ht; 
					} else {
						x = theme.ToolBarGripWidth;
						y += ht; 
						if (button.Location.X != x || button.Location.Y != y)
							changed = true;
						button.Location = new Point (x, y);
						x += button.Rectangle.Width;
					}
				}
				if (AutoSize)
					Height = y + ht;
			} else {
				if (AutoSize)
					Height = ht;
				else if (!height_specified)
					Height = DefaultSize.Height;
				foreach (ToolBarButton button in buttons) {
					if (size_specified) {
						if (button.Layout (button_size))
							changed = true;
					}
					else {
						if (button.Layout ())
							changed = true;
					}
					if (button.Location.X != x || button.Location.Y != y)
						changed = true;
					button.Location = new Point (x, y);
					x += button.Rectangle.Width;
				}
			}

			return changed;
		}
 		#endregion Private Methods

		#region subclass
		public class ToolBarButtonCollection : IList, ICollection, IEnumerable
		{
			#region instance variables
			private ArrayList list;
			private ToolBar owner;
			private bool redraw;
			#endregion

			#region constructors
			public ToolBarButtonCollection (ToolBar owner)
			{
				this.owner = owner;
				list = new ArrayList ();
				redraw = true;
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
					value.SetParent (owner);
					list [index] = value;
					owner.Redraw (true);
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
			}

			public bool Contains (ToolBarButton button)
			{
				return list.Contains (button);
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

			public void Insert (int index, ToolBarButton button)
			{
				list.Insert (index, button);
				owner.Redraw (true);
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
			}
			#endregion methods
		}
		#endregion subclass
	}
}
