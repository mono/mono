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
			
			MouseDown += new MouseEventHandler (ToolBar_MouseDown);
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
				Redraw (false);
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
						return new Size (39, 36);
					else
						return CalcButtonSize ();
				}
				return button_size;
			}
			set {
				size_specified = true;
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
			set { image_list = value; }
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

			Invalidate (e.Button.Rectangle);
			Redraw (false);

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

			Redraw (true);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}

		#endregion Protected Methods

		#region Private Methods
		private void ToolBar_MouseDown (object sender, MouseEventArgs me)
		{
			if (!Enabled) 
				return;

			Point loc = new Point (me.X, me.Y);

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
					}
					button.pressed = true;
					button.inside = true;
					Invalidate (button.Rectangle);
					break;
				}
			}
		}

		private void ToolBar_MouseUp (object sender, MouseEventArgs me)
		{
			if (!Enabled) 
				return;

			Point loc = new Point (me.X, me.Y);

			// draw the normal button
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
					if (button.pressed)
						OnButtonClick (new ToolBarButtonClickEventArgs (button));
				} else if (button.pressed) {
					button.pressed = false;
					Invalidate (button.Rectangle);
				}
			}
		}

		private void ToolBar_MouseLeave (object sender, EventArgs e)
		{
			if (!Enabled || appearance != ToolBarAppearance.Flat || current_button == null) 
				return;

			if (current_button.Hilight) {
				current_button.Hilight = false;
				Invalidate (current_button.Rectangle);
				Redraw (false);
			}
			current_button = null;
		}

		private void ToolBar_MouseMove (object sender, MouseEventArgs me)
		{
			if (!Enabled) 
				return;

			Point loc = new Point (me.X, me.Y);

			if (this.Capture) {
				// If the button was pressed and we leave, release the 
				// button press and vice versa
				foreach (ToolBarButton button in buttons) {
					if (button.pressed &&
					    (button.inside != button.Rectangle.Contains (loc))) {
						button.inside = button.Rectangle.Contains (loc);
						button.Hilight = false;
						Invalidate (button.Rectangle);
						Redraw (false);
						break;
					}
				}
			}
			// following is only for flat style toolbar
			else if (appearance == ToolBarAppearance.Flat) {
				if (current_button != null && current_button.Rectangle.Contains (loc)) {
					if (current_button.Hilight || current_button.Pushed)
						return;
					current_button.Hilight = true;
					Invalidate (current_button.Rectangle);
					Redraw (false);
				}
				else {
					foreach (ToolBarButton button in buttons) {
						if (button.Rectangle.Contains (loc) && button.Enabled) {
							current_button = button;
							if (current_button.Hilight || current_button.Pushed)
								continue;
							current_button.Hilight = true;
							Invalidate (current_button.Rectangle);
							Redraw (false);
						}
						else if (button.Hilight) {
							button.Hilight = false;
							Invalidate (button.Rectangle);
							Redraw (false);
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
			if (recalculate)
				Layout ();

			Refresh ();
		}

		private Size CalcButtonSize ()
		{
			String longestText = buttons [0].Text;
			for (int i = 1; i < buttons.Count; i++) {
				if (buttons[i].Text.Length > longestText.Length)
					longestText = buttons[i].Text;
			}

			SizeF sz = this.DeviceContext.MeasureString (longestText, this.Font);
			Size size = new Size ((int) Math.Ceiling (sz.Width), (int) Math.Ceiling (sz.Height));

			if (ImageList != null) {
				// adjustment for the image grip 
				int imgWidth = this.ImageSize.Width + 2 * ThemeEngine.Current.ToolBarImageGripWidth; 
				int imgHeight = this.ImageSize.Height + 2 * ThemeEngine.Current.ToolBarImageGripWidth;

				if (text_alignment == ToolBarTextAlign.Right) {
					size.Width = imgWidth + size.Width;
					size.Height = (size.Height > imgHeight) ? size.Height : imgHeight;
				}
				else {
					size.Height = imgHeight + size.Height;
					size.Width = (size.Width > imgWidth) ? size.Width : imgWidth;
				}
			}
			return size;
		}

		void Layout ()
		{
			Theme theme = ThemeEngine.Current;
			int ht = ButtonSize.Height + theme.ToolBarGripWidth;
			int x = theme.ToolBarGripWidth;
			int y = theme.ToolBarGripWidth;

			if (Wrappable) {
				int separator_index = -1;

				for (int i = 0; i < buttons.Count; i++) {
					ToolBarButton button = buttons [i];

					if (!button.Visible)
						continue;

					if (size_specified)
						button.Layout (ButtonSize);
					else
						button.Layout ();

					bool is_separator = button.Style == ToolBarButtonStyle.Separator;

					if (x + button.Rectangle.Width < Width || is_separator) {
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
						button.Location = new Point (x, y);
						x += button.Rectangle.Width;
					}
				}
				if (AutoSize)
					Height = y + ht;
			} else {
				if (AutoSize)
					Height = ht;
				else
					Height = DefaultSize.Height;
				foreach (ToolBarButton button in buttons) {
					if (size_specified)
						button.Layout (ButtonSize);
					else
						button.Layout ();
					button.Location = new Point (x, y);
					x += button.Rectangle.Width;
				}
			}
		}
 		#endregion Private Methods

		#region subclass
		public class ToolBarButtonCollection : IList, ICollection, IEnumerable
		{
			#region instance variables
			private ArrayList list;
			private ToolBar owner;
			#endregion

			#region constructors
			public ToolBarButtonCollection (ToolBar owner)
			{
				this.owner = owner;
				list = new ArrayList ();
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
				owner.Redraw (true);
				return result;
			}

			public void AddRange (ToolBarButton [] buttons)
			{
				foreach (ToolBarButton button in buttons)
					Add (button);
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
