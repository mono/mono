//
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
//
// TODO:
//   - Drawing ellipsis when text is too big to draw on a button
//   - RightToLeft
//   - DropDown ContextMenu
//   - Tooltip
//
// Copyright (C) Novell, Inc. 2004 (http://www.novell.com)
//
//
// $Revision: 1.15 $
// $Modtime: $
// $Log: ToolBar.cs,v $
// Revision 1.15  2004/10/06 09:59:05  jordi
// removes warnings from compilation
//
// Revision 1.14  2004/10/05 09:07:07  ravindra
// 	- Removed a private method, Draw ().
// 	- Fixed the ButtonDropDown event handling.
// 	- Fixed MouseMove event handling.
//
// Revision 1.13  2004/10/05 04:56:12  jackson
// Let the base Control handle the buffers, derived classes should not have to CreateBuffers themselves.
//
// Revision 1.12  2004/09/28 18:44:25  pbartok
// - Streamlined Theme interfaces:
//   * Each DrawXXX method for a control now is passed the object for the
//     control to be drawn in order to allow accessing any state the theme
//     might require
//
//   * ControlPaint methods for the theme now have a CP prefix to avoid
//     name clashes with the Draw methods for controls
//
//   * Every control now retrieves it's DefaultSize from the current theme
//
// Revision 1.11  2004/09/16 13:00:19  ravindra
// Invalidate should be done before redrawing.
//
// Revision 1.10  2004/09/09 11:25:03  ravindra
// Make redraw accessible from ToolBarButton.
//
// Revision 1.9  2004/08/25 20:04:40  ravindra
// Added the missing divider code and grip for ToolBar Control.
//
// Revision 1.8  2004/08/25 00:43:13  ravindra
// Fixed wrapping related issues in ToolBar control.
//
// Revision 1.7  2004/08/22 01:20:14  ravindra
// Correcting the formatting mess of VS.NET.
//
// Revision 1.6  2004/08/22 00:49:37  ravindra
// Probably this completes the missing attributes in toolbar control.
//
// Revision 1.5  2004/08/22 00:03:20  ravindra
// Fixed toolbar control signatures.
//
// Revision 1.4  2004/08/21 01:52:08  ravindra
// Improvments in mouse event handling in the ToolBar control.
//
// Revision 1.3  2004/08/17 02:00:54  ravindra
// Added attributes.
//
// Revision 1.2  2004/08/17 00:48:50  ravindra
// Added attributes.
//
// Revision 1.1  2004/08/15 23:13:15  ravindra
// First Implementation of ToolBar control.
//
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
	[Designer ("System.Windows.Forms.Design.ToolBarDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	public class ToolBar : Control
	{
		#region Instance Variables
		internal ToolBarAppearance	appearance;
		internal bool			autosize;
		internal BorderStyle		borderStyle;
		internal ToolBarButtonCollection buttons;
		internal Size			buttonSize;
		internal bool			divider;
		internal bool			dropDownArrows;
		internal ImageList		imageList;
		internal ImeMode		imeMode;
		internal bool			showToolTips;
		internal ToolBarTextAlign	textAlignment;
		internal bool			wrappable;        // flag to make the toolbar wrappable
		internal bool			redraw;           // flag to force redrawing the control
		internal ToolBarButton		currentButton; // the highlighted button
		#endregion Instance Variables

		#region Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged;

		public event ToolBarButtonClickEventHandler ButtonClick;
		public event ToolBarButtonClickEventHandler ButtonDropDown;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftChanged;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged;
		#endregion Events

		#region Constructor
		public ToolBar ()
		{
			appearance = ToolBarAppearance.Normal;
			autosize = true;
			background_color = ThemeEngine.Current.DefaultControlBackColor;
			borderStyle = BorderStyle.None;
			buttons = new ToolBarButtonCollection (this);
			buttonSize = Size.Empty;
			divider = true;
			dropDownArrows = false;
			foreground_color = ThemeEngine.Current.DefaultControlForeColor;
			showToolTips = false;
			textAlignment = ToolBarTextAlign.Underneath;
			wrappable = true;
			dock_style = DockStyle.Top;
			redraw = true;
			
			// event handlers
			this.MouseDown += new MouseEventHandler (ToolBar_MouseDown);
			this.MouseLeave += new EventHandler (ToolBar_MouseLeave);
			this.MouseMove += new MouseEventHandler (ToolBar_MouseMove);
			this.MouseUp += new MouseEventHandler (ToolBar_MouseUp);
			base.Paint += new PaintEventHandler (ToolBar_Paint);
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
				if (BackColorChanged != null)
					BackColorChanged (this, new EventArgs ());
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
				if (BackgroundImageChanged != null)
					BackgroundImageChanged (this, new EventArgs ());
				Redraw (false);
			}
		}

		[DefaultValue (BorderStyle.None)]
		[DispIdAttribute (-504)]
		public BorderStyle BorderStyle {
			get { return borderStyle; }
			set {
				if (value == borderStyle)
					return;

				borderStyle = value;
				Redraw (false);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[MergableProperty (false)]
		public ToolBarButtonCollection Buttons {
			get { return buttons; }
		}

		[Localizable (true)]
		[RefreshProperties (RefreshProperties.All)]
		public Size ButtonSize {
			get {
				if (buttonSize.IsEmpty) {
					if (buttons.Count == 0)
						return new Size (39, 36);
					else
						return CalcButtonSize ();
				}
				return buttonSize;
			}
			set {
				if (buttonSize.Width == value.Width && buttonSize.Height == value.Height)
					return;

				if (value.Width > 0 && value.Height > 0) {
					buttonSize = value;
					Redraw (true);
				}
			}
		}

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

		[DefaultValue (false)]
		[Localizable (true)]
		public bool DropDownArrows {
			get { return dropDownArrows; }
			set {
				if (value == dropDownArrows)
					return;

				dropDownArrows = value;
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
				if (ForeColorChanged != null)
					ForeColorChanged (this, new EventArgs ());
				Redraw (false);
			}
		}

		[DefaultValue (null)]
		public ImageList ImageList {
			get { return imageList; }
			set { imageList = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public Size ImageSize {
			get {
				if (imageList == null)
					return Size.Empty;

				return imageList.ImageSize;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return imeMode; }
			set {
				if (value == imeMode)
					return;

				imeMode = value;
				if (ImeModeChanged != null)
					ImeModeChanged (this, new EventArgs ());
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
				if (RightToLeftChanged != null)
					RightToLeftChanged (this, new EventArgs ());
			}
		}

		[DefaultValue (false)]
		[Localizable (true)]
		public bool ShowToolTips {
			get { return showToolTips; }
			set { showToolTips = value; }
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
				if (TextChanged != null)
					TextChanged (this, new EventArgs ());
			}
		}

		[DefaultValue (ToolBarTextAlign.Underneath)]
		[Localizable (true)]
		public ToolBarTextAlign TextAlign {
			get { return textAlignment; }
			set {
				if (value == textAlignment)
					return;

				textAlignment = value;
				Redraw (true);
			}
		}

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

		#region Internal Methods
		internal Rectangle GetChildBounds (ToolBarButton button)
		{
			if (button.Style == ToolBarButtonStyle.Separator)
				return new Rectangle (button.Location.X, button.Location.Y, 
						      ThemeEngine.Current.ToolBarSeparatorWidth, this.ButtonSize.Height);

			SizeF sz = this.DeviceContext.MeasureString (button.Text, this.Font);
			Size size = new Size ((int) Math.Ceiling (sz.Width), (int) Math.Ceiling (sz.Height));

			if (imageList != null) {
				// adjustment for the image grip 
				int imgWidth = this.ImageSize.Width + 2 * ThemeEngine.Current.ToolBarImageGripWidth; 
				int imgHeight = this.ImageSize.Height + 2 * ThemeEngine.Current.ToolBarImageGripWidth; 

				if (textAlignment == ToolBarTextAlign.Right) {
					size.Width =  imgWidth + size.Width;
					size.Height = (size.Height > imgHeight) ? size.Height : imgHeight;
				}
				else {
					size.Height = imgHeight + size.Height;
					size.Width = (size.Width > imgWidth) ? size.Width : imgWidth;
				}
			}
			if (button.Style == ToolBarButtonStyle.DropDownButton && this.dropDownArrows)
				size.Width += ThemeEngine.Current.ToolBarDropDownWidth;

			return new Rectangle (button.Location, size);
		}
		#endregion Internal Methods

		#region Protected Methods
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (imageList != null)
					imageList.Dispose ();
			}

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
			e.Button.Pressed = false;

			Invalidate (e.Button.Rectangle);
			Redraw (false);

			if (ButtonClick != null)
				ButtonClick (this, e);
			else
				return;
		}

		protected virtual void OnButtonDropDown (ToolBarButtonClickEventArgs e) 
		{
			// if (e.Button.DropDownMenu == null) return;
			// TODO: Display the dropdown menu

			// Reset the flag set on DropDown
			e.Button.dd_pressed = false;

			if (ButtonDropDown != null)
				ButtonDropDown (this, e);
			else
				return;
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

			if (this.Width <= 0 || this.Height <= 0 || this.Visible == false)
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
			if (! this.Enabled) return;

			Point hit = new Point (me.X, me.Y);
			this.Capture = true;

			// draw the pushed button
			foreach (ToolBarButton button in buttons) {
				if (button.Enabled && button.Rectangle.Contains (hit)) {
					// Mark the DropDown rect as pressed.
					// We don't redraw the dropdown rect.
					if (button.Style == ToolBarButtonStyle.DropDownButton) {
						Rectangle ddRect = Rectangle.Empty;
						Rectangle rect = button.Rectangle;
						ddRect.Height = rect.Height;
						ddRect.Width = ThemeEngine.Current.ToolBarDropDownWidth;
						ddRect.X = rect.X + rect.Width - ddRect.Width;
						ddRect.Y = rect.Y;
						if (ddRect.Contains (hit)) {
							button.dd_pressed = true;
							break;
						}
					}
					// If it is not dropdown then we treat it as a normal
					// button press.
					button.Pressed = true;
					Invalidate (button.Rectangle);
					Redraw (false);
					break;
				}
			}
		}

		private void ToolBar_MouseUp (object sender, MouseEventArgs me)
		{
			if (! this.Enabled) return;

			Point hit = new Point (me.X, me.Y);
			this.Capture = false;

			// draw the normal button
			foreach (ToolBarButton button in buttons) {
				if (button.Enabled && button.Rectangle.Contains (hit)) {
					if (button.Style == ToolBarButtonStyle.DropDownButton) {
						Rectangle ddRect = Rectangle.Empty;
						Rectangle rect = button.Rectangle;
						ddRect.Height = rect.Height;
						ddRect.Width = ThemeEngine.Current.ToolBarDropDownWidth;
						ddRect.X = rect.X + rect.Width - ddRect.Width;
						ddRect.Y = rect.Y;
						// Fire a ButtonDropDown event
						if (ddRect.Contains (hit)) {
							if (button.dd_pressed)
								this.OnButtonDropDown (new ToolBarButtonClickEventArgs (button));
							continue;
						}
					}
					// Fire a ButtonClick
					if (button.Pressed)
						this.OnButtonClick (new ToolBarButtonClickEventArgs (button));
				}
				// Clear the button press flags, if any
				else if (button.Pressed) {
					button.Pressed = false;
					Invalidate (button.Rectangle);
					Redraw (false);
				}
			}
		}

		private void ToolBar_MouseLeave (object sender, EventArgs e)
		{
			if (! this.Enabled || appearance != ToolBarAppearance.Flat) return;

			if (currentButton != null && currentButton.Hilight) {
				currentButton.Hilight = false;
				Invalidate (currentButton.Rectangle);
				Redraw (false);
			}
			currentButton = null;
		}

		private void ToolBar_MouseMove (object sender, MouseEventArgs me)
		{
			if (! this.Enabled || appearance != ToolBarAppearance.Flat) return;

			Point hit = new Point (me.X, me.Y);

			if (currentButton != null && currentButton.Rectangle.Contains (hit)) {
				if (currentButton.Hilight || currentButton.Pushed)
					return;
				currentButton.Hilight = true;
				Invalidate (currentButton.Rectangle);
				Redraw (false);
			}
			else {
				foreach (ToolBarButton button in buttons) {
					if (button.Rectangle.Contains (hit) && button.Enabled) {
						currentButton = button;
						if (currentButton.Hilight || currentButton.Pushed)
							continue;
						currentButton.Hilight = true;
						Invalidate (currentButton.Rectangle);
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

		private void ToolBar_Paint (object sender, PaintEventArgs pe)
		{
			if (this.Width <= 0 || this.Height <=  0 || this.Visible == false)
				return;

			if (redraw) {
				ThemeEngine.Current.DrawToolBar (this.DeviceContext, pe.ClipRectangle, this);
				redraw = false;
			}

			// paint on the screen
			pe.Graphics.DrawImage (this.ImageBuffer, pe.ClipRectangle, pe.ClipRectangle, GraphicsUnit.Pixel);

			if (Paint != null)
				Paint (this, pe);
		}

		internal void Redraw (bool recalculate)
		{
			if (recalculate)
				CalcToolBar ();

			redraw = true;
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

			if (imageList != null) {
				// adjustment for the image grip 
				int imgWidth = this.ImageSize.Width + 2 * ThemeEngine.Current.ToolBarImageGripWidth; 
				int imgHeight = this.ImageSize.Height + 2 * ThemeEngine.Current.ToolBarImageGripWidth;

				if (textAlignment == ToolBarTextAlign.Right) {
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

		/* Checks for the separators and sets the location of a button and its wrapper flag */
		private void CalcToolBar ()
		{
			int wd = this.Width;             // the amount of space we have for rest of the buttons
			int ht = this.ButtonSize.Height; // all buttons are displayed with the same height
			Point loc;                       // the location to place the next button, leave the space for border
			loc = new Point (ThemeEngine.Current.ToolBarGripWidth, ThemeEngine.Current.ToolBarGripWidth);

			// clear all the wrappers if toolbar is not wrappable
			if (! wrappable && ! autosize) {
				if (this.Height != this.DefaultSize.Height)
					this.Height = this.DefaultSize.Height;
				foreach (ToolBarButton button in buttons) {
					button.Location = loc;
					button.Wrapper = false;
					loc.X = loc.X + button.Rectangle.Width;
				}
			}
			else if (! wrappable) { // autosizeable
				if (ht != this.Height)
					this.Height = ht;
				foreach (ToolBarButton button in buttons) {
					button.Location = loc;
					button.Wrapper = false;
					loc.X = loc.X + button.Rectangle.Width;
				}
			}
			else { // wrappable
				bool seenSeparator = false;
				int separatorIndex = -1;
				ToolBarButton button;

				for (int i = 0; i < buttons.Count; i++) {
					button = buttons [i];
					if (button.Visible) {
						if (button.Style == ToolBarButtonStyle.Separator) {
							wd -= ThemeEngine.Current.ToolBarSeparatorWidth;
							if (wd > 0) {
								button.Wrapper = false; // clear the old flag in case it was set
								button.Location = loc;
								loc.X = loc.X + ThemeEngine.Current.ToolBarSeparatorWidth;
							}
							else {
								button.Wrapper = true;
								button.Location = loc;
								loc.X = ThemeEngine.Current.ToolBarGripWidth;
								wd = this.Width;
								// we need space to draw horizontal separator
								loc.Y = loc.Y + ThemeEngine.Current.ToolBarSeparatorWidth + ht; 
							}
							seenSeparator = true;
							separatorIndex = i;
						}
						else {
							Rectangle rect = button.Rectangle;
							wd -= rect.Width;
							if (wd > 0) {
								button.Wrapper = false;
								button.Location = loc;
								loc.X = loc.X + rect.Width;
							}
							else if (seenSeparator) { 
								// wrap at the separator and reassign the locations
								i = separatorIndex; // for loop is going to increment it
								buttons [separatorIndex].Wrapper = true;
								seenSeparator = false;
								separatorIndex = -1;
								loc.X = ThemeEngine.Current.ToolBarGripWidth;
								// we need space to draw horizontal separator
								loc.Y = loc.Y + ht + ThemeEngine.Current.ToolBarSeparatorWidth; 
								wd = this.Width;
								continue;
							}
							else {
								button.Wrapper = true;
								wd = this.Width;
								loc.X = 0;
								loc.Y += ht;
								button.Location = loc;
								loc.X = loc.X + rect.Width;
							}
						}
					}
					else // don't consider invisible buttons
						continue;
				}
				/* adjust the control height, if we are autosizeable */
				if (autosize) // wrappable
					if (this.Height != (loc.Y + ht + ThemeEngine.Current.ToolBarGripWidth))
						this.Height = loc.Y + ht + ThemeEngine.Current.ToolBarGripWidth;
			}
		}

		private void DumpToolBar (string msg)
		{
			Console.WriteLine (msg);
			Console.WriteLine ("ToolBar: name: " + this.Text);
			Console.WriteLine ("ToolBar: wd, ht: " + this.Size);
			Console.WriteLine ("ToolBar: img size: " + this.ImageSize);
			Console.WriteLine ("ToolBar: button sz: " + this.buttonSize);
			Console.WriteLine ("ToolBar: textalignment: "+ this.TextAlign);
			Console.WriteLine ("ToolBar: appearance: "+ this.Appearance);
			Console.WriteLine ("ToolBar: wrappable: "+ this.Wrappable);
			Console.WriteLine ("ToolBar: buttons count: " + this.Buttons.Count);

			int i= 0;	
			foreach (ToolBarButton b in buttons) {
				Console.WriteLine ("ToolBar: button [{0}]:",i++);
				b.Dump ();
			}
		}
 		#endregion Private Methods

		#region subclass
		public class ToolBarButtonCollection : IList, ICollection, IEnumerable
		{
			#region instance variables
			private ArrayList buttonsList;
			private ToolBar owner;
			#endregion

			#region constructors
			public ToolBarButtonCollection (ToolBar owner)
			{
				this.owner = owner;
				this.buttonsList = new ArrayList ();
			}
			#endregion

			#region properties
			[Browsable (false)]
			public virtual int Count {
				get { return buttonsList.Count; }
			}

			public virtual bool IsReadOnly {
				get { return buttonsList.IsReadOnly; }
			}

			public virtual ToolBarButton this [int index] {
				get { return (ToolBarButton) buttonsList [index]; }
				set {
					value.SetParent (owner);
					buttonsList [index] = value;
					owner.Redraw (true);
				}
			}

			bool ICollection.IsSynchronized {
				get { return buttonsList.IsSynchronized; }
			}

			object ICollection.SyncRoot {
				get { return buttonsList.SyncRoot; }
			}

			bool IList.IsFixedSize {
				get { return buttonsList.IsFixedSize; }
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
				result = buttonsList.Add (button);
				owner.Redraw (true);
				return result;
			}

			public void AddRange (ToolBarButton [] buttons)
			{
				foreach (ToolBarButton button in buttons)
					Add (button);
			}

			public virtual void Clear ()
			{
				buttonsList.Clear ();
				owner.Redraw (false);
			}

			public bool Contains (ToolBarButton button)
			{
				return buttonsList.Contains (button);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return buttonsList.GetEnumerator ();
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				buttonsList.CopyTo (dest, index);
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
				return buttonsList.IndexOf (button);
			}

			public void Insert (int index, ToolBarButton button)
			{
				buttonsList.Insert (index, button);
				owner.Redraw (true);
			}

			public void Remove (ToolBarButton button)
			{
				buttonsList.Remove (button);
				owner.Redraw (true);
			}

			public virtual void RemoveAt (int index)
			{
				buttonsList.RemoveAt (index);
				owner.Redraw (true);
			}
			#endregion methods
		}
		#endregion subclass
	}
}
