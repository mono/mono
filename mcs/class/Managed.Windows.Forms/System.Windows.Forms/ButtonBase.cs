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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
// $Log: ButtonBase.cs,v $
// Revision 1.10  2004/10/05 04:56:11  jackson
// Let the base Control handle the buffers, derived classes should not have to CreateBuffers themselves.
//
// Revision 1.9  2004/09/28 18:44:25  pbartok
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
// Revision 1.8  2004/09/02 22:24:35  pbartok
// - Fixed selection of text color
// - Fixed handling of resize event; now properly recreates double buffering
//   bitmap
// - Added missing assignment of TextAlignment
// - Added proper default for TextAlignment
//
// Revision 1.7  2004/09/01 02:07:37  pbartok
// - Enabled display of strings
//
// Revision 1.6  2004/09/01 01:55:20  pbartok
// - Removed the rather odd split between 'needs redraw' and redrawing
// - Now handles the events that require regeneration (ambient properties and
//   size)
//
// Revision 1.5  2004/08/31 18:49:58  pbartok
// - Removed debug
// - Minor fixes
//
// Revision 1.4  2004/08/30 20:42:10  pbartok
// - Made Redraw() and Redraw() virtual
// - Improved mouse up/down/move logic to properly track buttons
//
// Revision 1.3  2004/08/23 23:27:44  pbartok
// - Finishing touches. Works now, just needs some optimizations.
//
// Revision 1.2  2004/08/21 21:57:41  pbartok
// - Added loads of debug output for development
// - Fixed typo in method name
//
// Revision 1.1  2004/08/15 21:31:10  pbartok
// - First (mostly) working version
//
//
//

// NOT COMPLETE

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	public abstract class ButtonBase : Control {
		#region Local Variables
		internal FlatStyle		flat_style;
		internal int			image_index;
		internal Image			image;
		internal ImageList		image_list;
		internal ContentAlignment	image_alignment;
		internal ContentAlignment	text_alignment;
		private ImeMode			ime_mode;
		private bool			is_default;
		internal bool			has_focus;
		internal bool			is_pressed;
		internal bool			is_entered;
		internal StringFormat		text_format;
		#endregion	// Local Variables

		#region Private Properties and Methods
		internal ButtonState ButtonState {
			get {
				ButtonState	ret = ButtonState.Normal;

				if (Enabled) {
					// Popup style is only followed as long as the mouse isn't "in" the control
					if (is_entered) {
						if (flat_style == FlatStyle.Flat) {
							ret |= ButtonState.Flat;
						}
					} else {
						if (flat_style == FlatStyle.Flat || flat_style == FlatStyle.Popup) {
							ret |= ButtonState.Flat;
						}
					}

					if (is_entered && is_pressed) {
						ret |= ButtonState.Pushed;
					}
				} else {
					ret |= ButtonState.Inactive;
					if ((flat_style == FlatStyle.Flat) || (flat_style == FlatStyle.Popup)) {
						ret |= ButtonState.Flat;
					}
				}
				return ret;
			}
		}

		[MonoTODO("Make the FillRectangle use a global brush instead of creating one every time")]
		internal virtual void Redraw() {
			ThemeEngine.Current.DrawButtonBase(this.DeviceContext, this.ClientRectangle, this);
			Refresh();
		}

		private void RedrawEvent(object sender, System.EventArgs e) {
			Redraw();
		}

		#endregion	// Private Properties and Methods

		#region Public Constructors
		protected ButtonBase() : base() {
			flat_style	= FlatStyle.Standard;
			image_index	= -1;
			image		= null;
			image_list	= null;
			image_alignment	= ContentAlignment.MiddleCenter;
			text_alignment	= ContentAlignment.MiddleCenter;
			ime_mode	= ImeMode.Inherit;
			is_default	= false;
			is_entered	= false;
			is_pressed	= false;
			has_focus	= false;
			text_format	= new StringFormat();
			text_format.Alignment = StringAlignment.Center;
			text_format.LineAlignment = StringAlignment.Center;

			TextChanged+=new System.EventHandler(RedrawEvent);
			ForeColorChanged+=new EventHandler(RedrawEvent);
			BackColorChanged+=new System.EventHandler(RedrawEvent);
			FontChanged+=new EventHandler(RedrawEvent);
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public FlatStyle FlatStyle {
			get {
				return flat_style;
			}

			set { 
				flat_style = value; 
				Redraw();
			}
		}
		
		public Image Image {
			get {
				return image;
			}

			set { 
				image = value;
				Redraw();
			}
		}

		public ContentAlignment ImageAlign {
			get {
				return image_alignment;
			}

			set {
				image_alignment=value;
				Redraw();
			}
		}

		public int ImageIndex {
			get {
				if (image_list==null) {
					return -1;
				}
				return image_index;
			}

			set {
				image_index=value;
				Redraw();
			}
		}

		public ImageList ImageList {
			get {
				return image_list;
			}

			set {
				if (image_list != null) {
					image_list.Dispose();
				}

				image_list = value;
				if (value != null) {
					if (image != null) {
						image=null;
					}
					if (image_list.Images.Count >= image_index) {
						image_index=image_list.Images.Count-1;
					}
				}
				Redraw();
			}
		}

		public ImeMode ImeMode {
			get {
				return ime_mode;
			}

			set {
				ime_mode = value;
			}
		}

		public virtual ContentAlignment TextAlign {
			get {
				return text_alignment;
			}

			set {
				if (text_alignment != value) {
					text_alignment = value;
					switch(text_alignment) {
						case ContentAlignment.TopLeft: {
							text_format.Alignment=StringAlignment.Near;
							text_format.LineAlignment=StringAlignment.Near;
							break;
						}

						case ContentAlignment.TopCenter: {
							text_format.Alignment=StringAlignment.Center;
							text_format.LineAlignment=StringAlignment.Near;
							break;
						}

						case ContentAlignment.TopRight: {
							text_format.Alignment=StringAlignment.Far;
							text_format.LineAlignment=StringAlignment.Near;
							break;
						}

						case ContentAlignment.MiddleLeft: {
							text_format.Alignment=StringAlignment.Near;
							text_format.LineAlignment=StringAlignment.Center;
							break;
						}

						case ContentAlignment.MiddleCenter: {
							text_format.Alignment=StringAlignment.Center;
							text_format.LineAlignment=StringAlignment.Center;
							break;
						}

						case ContentAlignment.MiddleRight: {
							text_format.Alignment=StringAlignment.Far;
							text_format.LineAlignment=StringAlignment.Center;
							break;
						}

						case ContentAlignment.BottomLeft: {
							text_format.Alignment=StringAlignment.Near;
							text_format.LineAlignment=StringAlignment.Far;
							break;
						}

						case ContentAlignment.BottomCenter: {
							text_format.Alignment=StringAlignment.Center;
							text_format.LineAlignment=StringAlignment.Far;
							break;
						}

						case ContentAlignment.BottomRight: {
							text_format.Alignment=StringAlignment.Far;
							text_format.LineAlignment=StringAlignment.Far;
							break;
						}
					}	
					Redraw();
				}
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get { 
				CreateParams	cp;

				cp=base.CreateParams;

				cp.Style=(int)WindowStyles.WS_VISIBLE | (int)WindowStyles.WS_CHILD;

				SetStyle(ControlStyles.UserPaint, true);
				SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				return cp;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {
				return ImeMode.Inherit;
			}
		}

		protected override Size DefaultSize {
			get {
				return ThemeEngine.Current.ButtonBaseDefaultSize;
			}
		}

		protected bool IsDefault {
			get {
				return is_default;
			}

			set {
				if (is_default != value) {
					is_default = true;
					Redraw();
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		[MonoTODO("Finish setting properties of the AccessibleObject")]
		protected override AccessibleObject CreateAccessibilityInstance() {
			AccessibleObject ao;
			ao=base.CreateAccessibilityInstance();
			ao.description="Button";

			return ao;
		}

		protected override void Dispose(bool Disposing) {
			base.Dispose(Disposing);
		}

		protected override void OnEnabledChanged(EventArgs e) {
			Redraw();
			base.OnEnabledChanged(e);
		}

		protected override void OnGotFocus(EventArgs e) {
			has_focus=true;
			Redraw();
			base.OnGotFocus(e);
		}

		protected override void OnKeyDown(KeyEventArgs kevent) {
			if (is_enabled && (kevent.KeyData == Keys.Enter || kevent.KeyData == Keys.Space)) {
				OnClick(EventArgs.Empty);
				kevent.Handled=true;
			}
			base.OnKeyDown(kevent);
		}

		protected override void OnKeyUp(KeyEventArgs kevent) {
			base.OnKeyUp(kevent);
		}

		protected override void OnLostFocus(EventArgs e) {
			has_focus=false;
			Redraw();
			base.OnLostFocus(e);
		}

		protected override void OnMouseDown(MouseEventArgs mevent) {
			if (is_enabled && (mevent.Button == MouseButtons.Left)) {
				is_pressed = true;
				this.Capture = true;
				Redraw();
			}

			base.OnMouseDown(mevent);
		}

		protected override void OnMouseEnter(EventArgs e) {
			is_entered=true;
			if ((this.flat_style == FlatStyle.Flat) || (this.flat_style == FlatStyle.Popup)) {
				Redraw();
			}
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e) {
			is_entered=false;
			if ((this.flat_style == FlatStyle.Flat) || (this.flat_style == FlatStyle.Popup)) {
				Redraw();
			}
			base.OnMouseLeave(e);
		}

		protected override void OnMouseMove(MouseEventArgs mevent) {
			bool	inside = false;
			bool	redraw = false;

			if (mevent.X>=0 && mevent.Y>=0 && mevent.X<this.client_size.Width && mevent.Y<=this.client_size.Height) {
				inside = true;
			}

			// If the button was pressed and we leave, release the button press and vice versa
			if (this.Capture && (inside != is_pressed)) {
				is_pressed = inside;
				redraw = true;
			}

			if (is_entered != inside) {
				is_entered = inside;
				redraw = true;
			}

			if (redraw) {
				Redraw();
			}

			base.OnMouseMove(mevent);
		}

		protected override void OnMouseUp(MouseEventArgs mevent) {
			if (this.Capture && mevent.Button == MouseButtons.Left) {
				this.Capture = false;
				if (is_pressed) {
					is_pressed = false;
					Redraw();
				}

				if (mevent.X>=0 && mevent.Y>=0 && mevent.X<this.client_size.Width && mevent.Y<=this.client_size.Height) {
					OnClick(EventArgs.Empty);
				}
			}
			base.OnMouseUp(mevent);
		}

		protected override void OnPaint(PaintEventArgs pevent) {
			pevent.Graphics.DrawImage(this.ImageBuffer, pevent.ClipRectangle, pevent.ClipRectangle, GraphicsUnit.Pixel);
			base.OnPaint(pevent);
		}

		protected override void OnParentChanged(EventArgs e) {
			base.OnParentChanged(e);
		}

		protected override void OnTextChanged(EventArgs e) {
			Redraw();
			base.OnTextChanged(e);
		}

		protected override void OnVisibleChanged(EventArgs e) {
			if (!Visible) {
				is_pressed = false;
				has_focus = false;
				is_entered = false;
			}
			base.OnVisibleChanged(e);
		}

		protected void ResetFlagsandPaint() {
			// Nothing to do; MS internal
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
		}
		#endregion	// Public Instance Properties
	}
}
