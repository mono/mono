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
// Authors:
//	Ravindra (rkumar@novell.com)
//
// TODO:
//   - Drawing dotted text when text is too big to draw on a button
//   - Hilighting a button in flat appearance, when mouse moves over
//   - RightToLeft
//   - DropDown ContextMenu
//
// Copyright (C) Novell Inc., 2004
//
//
// $Revision: 1.1 $
// $Modtime: $
// $Log: ToolBar.cs,v $
// Revision 1.1  2004/08/15 23:13:15  ravindra
// First Implementation of ToolBar control.
//
//
//

// NOT COMPLETE

using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{	
	public class ToolBar : Control
	{
		#region Instance Variables
		private ToolBarAppearance appearance;
		private bool autosize;
		private BorderStyle borderStyle;
		private ToolBarButtonCollection buttons;
		private Size buttonSize;
		private bool divider;
		private bool dropDownArrows;
		private ImageList imageList;
		private ImeMode imeMode;
		private bool showToolTips;
		private ToolBarTextAlign textAlignment;
		private bool wrappable;        // flag to make the toolbar wrappable
		private bool recalculate;      // flag to make sure that we calculate the toolbar before drawing
		private bool redraw;           // flag to force redrawing the control
		#endregion Instance Variables

		#region Events
		public new event EventHandler BackColorChanged;
		public new event EventHandler BackgroundImageChanged;
		public event ToolBarButtonClickEventHandler ButtonClick;
		public event ToolBarButtonClickEventHandler ButtonDropDown;
		public new event EventHandler ForeColorChanged;
		public new event EventHandler ImeModeChanged;
		public new event PaintEventHandler Paint;
		public new event EventHandler RightToLeftChanged;
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

			redraw = true;
			recalculate = true;
		}
		#endregion Constructor

		#region protected Properties
		protected override CreateParams CreateParams 
		{
			get {
				CreateParams createParams = base.CreateParams;
				createParams.ClassName = XplatUI.DefaultClassName;
				createParams.Style = (int) WindowStyles.WS_CHILD | (int) WindowStyles.WS_VISIBLE;

				return createParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get { return ImeMode.Disable; }
		}

		protected override Size DefaultSize {
			get { return new Size (100, 42); }
		}
		#endregion

		#region Public Properties
		public ToolBarAppearance Appearance {
			get { return appearance; }
			set {
				if (value == appearance)
					return;

				appearance = value;
				Redraw (false);
			}
		}
		
		public bool AutoSize {
			get { return autosize; }
			set {
				if (value == autosize)
					return;

				autosize = value;
				Redraw (true);
			}
		}

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

		public BorderStyle BorderStyle {
			get { return borderStyle; }
			set {
				if (value == borderStyle)
					return;

				borderStyle = value;
				Redraw (false);
			}
		}

		public ToolBarButtonCollection Buttons {
			get { return buttons; }
		}

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

		public bool Divider {
			get { return divider; }
			set { divider = value; }
		}

		public override DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; } 
		}

		public bool DropDownArrows {
			get { return dropDownArrows; }
			set {
				if (value == dropDownArrows)
					return;

				dropDownArrows = value;
				Redraw (true);
			}
		}

		public override Color ForeColor {
			get { return foreground_color; }
			set {
				if (value == foreground_color)
					return;

				foreground_color = value;
				Redraw (false);
			}
		}

		public ImageList ImageList {
			get { return imageList; }
			set { imageList = value; }
		}

		public Size ImageSize {
			get {
				if (imageList == null)
					return Size.Empty;

				return imageList.ImageSize;
			}
		}

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

		/* NYI in Control.cs.
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
		*/

		public bool ShowToolTips {
			get { return showToolTips; }
			set { showToolTips = value; }
		}

		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

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

		public ToolBarTextAlign TextAlign {
			get { return textAlignment; }
			set {
				if (value == textAlignment)
					return;

				textAlignment = value;
				Redraw (true);
			}
		}

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
				int imgWidth = this.ImageSize.Width + 2 * ThemeEngine.Current.ToolBarImageGripWidth; 
				// adjustment for the image grip 
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

		internal void Redraw (bool recalculate)
		{
			redraw = true;
			this.recalculate = recalculate;
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

				Invalidate (e.Button.Rectangle);
			}

			if (ButtonClick != null)
				ButtonClick (this, e);
			else
				return;
		}

		protected virtual void OnButtonDropDown (ToolBarButtonClickEventArgs e) 
		{
			//if (e.Button.Style == ToolBarButtonStyle.DropDown)

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
			Refresh ();
		}

		protected override void OnMouseDown (MouseEventArgs me)
   		{
			base.OnMouseDown (me);

			if (! Enabled) return;
			Point hit = new Point (me.X - Location.X, me.Y - Location.Y);
			// draw the pushed button
			foreach (ToolBarButton button in buttons) {
				if (button.Rectangle.Contains (hit) && button.Enabled) {
					button.Pushed = true;
					Redraw (false);
					Invalidate (button.Rectangle);
					break;
				}
			}
		}

		protected override void OnMouseUp (MouseEventArgs me)
		{
			base.OnMouseUp (me);

			if (! Enabled) return;
			Point hit = new Point (me.X - Location.X, me.Y - Location.Y);
			// draw the normal button
			foreach (ToolBarButton button in buttons) {
				if (button.Rectangle.Contains (hit) && button.Enabled) {
					button.Pushed = false;
					Redraw (false);
					Invalidate (button.Rectangle);
					return;
				}
			}
		}

		protected override void OnMouseEnter (EventArgs e)
		{
			base.OnMouseEnter (e);

			if (!Enabled || appearance != ToolBarAppearance.Flat) return;
			// TODO:
			// draw the transparent rectangle with single line border around the flat button
      		}
				
		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);

			if (!Enabled || appearance != ToolBarAppearance.Flat) return;
			// TODO:
			// draw the normal flat button 
    		}

		protected override void OnPaint (PaintEventArgs pe)
		{
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw ();
			pe.Graphics.DrawImage (ImageBuffer, pe.ClipRectangle, pe.ClipRectangle, GraphicsUnit.Pixel);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);

			if (Width <= 0 || Height <= 0 || Visible == false)
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
		private void Draw ()
		{
			if (redraw) {
				StringFormat strFormat = new StringFormat ();
				if (textAlignment == ToolBarTextAlign.Underneath) {
					strFormat.LineAlignment = StringAlignment.Center;
					strFormat.Alignment = StringAlignment.Center;
				}
				else {
					strFormat.LineAlignment = StringAlignment.Center;
					strFormat.Alignment = StringAlignment.Near;
				}

				if (recalculate) {
					CalcToolBar ();
					CreateBuffers (Width, Height);
				}

				ThemeEngine.Current.DrawToolBar (DeviceContext, this, strFormat);
			}
			redraw = false;
			recalculate = false;
		}

		private Size CalcButtonSize ()
		{
			ToolBarButton button;
			String longestText = buttons [0].Text;
			for (int i = 1; i < buttons.Count; i++) {
				if (buttons[i].Text.Length > longestText.Length)
					longestText = buttons[i].Text;
			}

			SizeF sz = this.DeviceContext.MeasureString (longestText, this.Font);
			Size size = new Size ((int) Math.Ceiling (sz.Width), (int) Math.Ceiling (sz.Height));

			if (imageList != null) {
				int imgWidth = this.ImageSize.Width + 2 * ThemeEngine.Current.ToolBarImageGripWidth; 
				// adjustment for the image grip 
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

		private void DumpToolBar (string msg) {
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

		/* Checks for the separators and sets the location of a button and its wrapper flag */
		private void CalcToolBar ()
		{
			int wd = this.Width;             // the amount of space we have for rest of the buttons
			int ht = this.ButtonSize.Height; // all buttons are displayed with the same height
			Point loc = new Point (0, 0);    // the location to place the next button

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
								loc.X = 0;
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
								loc.X = 0;
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
					if (this.Height != (loc.Y + ht))
						this.Height = loc.Y + ht;
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
