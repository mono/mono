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
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Peter Bartok, pbartok@novell.com
//
//
// $Revision: 1.15 $
// $Modtime: $
// $Log: Label.cs,v $
// Revision 1.15  2004/09/28 18:44:25  pbartok
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
// Revision 1.14  2004/09/07 09:40:15  jordi
// LinkLabel fixes, methods, multiple links
//
// Revision 1.13  2004/09/04 17:10:18  jordi
// Refresh when font changed
//
// Revision 1.12  2004/09/01 15:10:10  jordi
// fixes method signatures, new methods, events, fixes autosize
//
// Revision 1.11  2004/08/21 22:30:53  pbartok
// - Signature fixes
//
// Revision 1.10  2004/08/21 22:21:13  pbartok
// - Signature fixes
//
// Revision 1.9  2004/08/11 18:54:11  pbartok
// - Forcing redraw on resize
//
// Revision 1.8  2004/08/10 15:24:35  jackson
// Let Control handle buffering.
//
// Revision 1.7  2004/08/08 19:47:41  jordi
// add cvs header info
//
//
// INCOMPLETE

using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.ComponentModel;

namespace System.Windows.Forms
{
    	public class Label : Control
    	{
    		private BorderStyle border_style;
    		private bool autosize;
    		private Image image;
    		private bool render_transparent;
    		private FlatStyle flat_style;
    		private int preferred_height;
    		private int preferred_width;
    		private bool use_mnemonic;
    		private int image_index = -1;
    		private ImageList image_list;
		internal ContentAlignment image_align;
		internal StringFormat string_format;
    		internal ContentAlignment text_align;    		
    		static SizeF req_witdthsize = new SizeF (0,0);

    		#region Events
    		public event EventHandler AutoSizeChanged;
    		public new event EventHandler BackgroundImageChanged;
    		public new event EventHandler ImeModeChanged;    		
		public new event KeyEventHandler KeyDown;		
		public new event KeyPressEventHandler KeyPress;		
		public new event KeyEventHandler KeyUp;
		public new event EventHandler TabStopChanged;
    		public event EventHandler TextAlignChanged;
		#endregion

    		public Label ()
    		{
			// Defaults in the Spec
			autosize = false;
			border_style = BorderStyle.None;
			string_format = new StringFormat();
			TextAlign = ContentAlignment.TopLeft;
			image = null;
			UseMnemonic = true;
			image_list = null;
			image_align = ContentAlignment.MiddleCenter;
			SetUseMnemonic (UseMnemonic);

			BackColor = ThemeEngine.Current.ColorButtonFace;
			ForeColor = ThemeEngine.Current.ColorWindowText;

			CalcPreferredHeight ();
			CalcPreferredWidth ();

			AutoSizeChanged = null;
    			TextAlignChanged = null;

			SetStyle (ControlStyles.ResizeRedraw, true);

			Resize += new EventHandler (OnResizeLB);
			HandleCreated += new EventHandler (OnHandleCreatedLB);
		}

		#region Public Properties

    		public virtual bool AutoSize {
    			get { return autosize; }
    			set {
    				if (autosize == value)
    					return;

    				autosize = value;
    				CalcAutoSize ();
				Refresh ();

    				if (AutoSizeChanged != null)
    					AutoSizeChanged (this, new EventArgs ());
    			}
    		}

    		public override Image BackgroundImage {
    			get {
    				return base.BackgroundImage;
    			}
    			set {
    				if (base.BackgroundImage == value)
					return;

				if (BackgroundImageChanged != null)
					BackgroundImageChanged (this, EventArgs.Empty);

				base.BackgroundImage = value;
				Refresh ();
    			}
    		}

    		public virtual BorderStyle BorderStyle {
    			get {
    				return border_style;
    			}
    			set {
				if (!Enum.IsDefined (typeof (BorderStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for BorderStyle", value));

				if (border_style == value)
					return;

    				border_style = value;
				Refresh ();
    			}
    		}

    		protected override CreateParams CreateParams {
    			get { return base.CreateParams;}
    		}

    		protected override ImeMode DefaultImeMode {
    			get { return ImeMode.Disable;}
    		}

    		protected override Size DefaultSize {
    			get {return ThemeEngine.Current.LabelDefaultSize;}
    		}

    		public FlatStyle FlatStyle {
    			get {
    				return flat_style;
    			}
    			set {
				if (!Enum.IsDefined (typeof (FlatStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for FlatStyle", value));

    				if (flat_style == value)
					return;

    				flat_style = value;
				Refresh ();
    			}
    		}

    		public Image Image {
    			get {
    				return image;
    			}
    			set {
    				if (image == value)
					return;

    				image = value;
				Refresh ();
    			}
    		}

    		public ContentAlignment ImageAlign {
    			get {
    				return image_align;
    			}
    			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for ContentAlignment", value));

    				if (image_align == value)
    					return;

    				image_align = value;
    				Refresh ();
    			}
    		}

    		public int ImageIndex {
    			get { return image_index;}
    			set {

				if (value < 0 || value>= image_list.Images.Count)
					throw new ArgumentException();

    				if (image_index == value)
					return;

				image_index = value;

				if (ImageList != null && image_index !=-1)
					Image = null;

    				Refresh ();
			}
    		}

    		public ImageList ImageList {
    			get { return image_list;}
    			set {
    				if (image_list == value)
					return;

				if (ImageList != null && image_index !=-1)
					Image = null;

    				Refresh ();
			}
    		}

    		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set {
				if (value == ImeMode)
					return;
				base.ImeMode = value;
				if (ImeModeChanged != null)
					ImeModeChanged (this, EventArgs.Empty);
			}
		}

    		public virtual int PreferredHeight {
    			get { return preferred_height; }
    		}

    		public virtual int PreferredWidth {
    			get {return preferred_width; }
    		}

    		protected virtual bool RenderTransparent {
			get { return render_transparent; }
			set { render_transparent = value;}
		}
    		
		public new bool TabStop  {
    			get { return base.TabStop; }
    			set {
				if (value == base.TabStop)
					return;

				base.TabStop = value;
				if (TabStopChanged != null)
					TabStopChanged (this, EventArgs.Empty);
			}
    		}

    		public virtual ContentAlignment TextAlign {
    			get { return text_align; }

    			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for ContentAlignment", value));

    				if (text_align != value) {

	    				text_align = value;

	    				switch (value) {

	    				case ContentAlignment.BottomLeft:
	    					string_format.LineAlignment = StringAlignment.Far;
	    					string_format.Alignment = StringAlignment.Near;
	    					break;
	    				case ContentAlignment.BottomCenter:
	    					string_format.LineAlignment = StringAlignment.Far;
	    					string_format.Alignment = StringAlignment.Center;
	    					break;
	    				case ContentAlignment.BottomRight:
	    					string_format.LineAlignment = StringAlignment.Far;
						string_format.Alignment = StringAlignment.Far;
						break;
					case ContentAlignment.TopLeft:
						string_format.LineAlignment = StringAlignment.Near;
						string_format.Alignment = StringAlignment.Near;
						break;
					case ContentAlignment.TopCenter:
						string_format.LineAlignment = StringAlignment.Near;
						string_format.Alignment = StringAlignment.Center;
						break;
					case ContentAlignment.TopRight:
						string_format.LineAlignment = StringAlignment.Near;
						string_format.Alignment = StringAlignment.Far;
						break;
					case ContentAlignment.MiddleLeft:
						string_format.LineAlignment = StringAlignment.Center;
						string_format.Alignment = StringAlignment.Near;
						break;
	    				case ContentAlignment.MiddleRight:
	    					string_format.LineAlignment = StringAlignment.Center;
	    					string_format.Alignment = StringAlignment.Far;
	    					break;
	    				case ContentAlignment.MiddleCenter:
	    					string_format.LineAlignment = StringAlignment.Center;
	    					string_format.Alignment = StringAlignment.Center;
						break;
					default:
						break;
					}

					if (TextAlignChanged != null)
    						TextAlignChanged (this, new EventArgs ());

    					Refresh();
				}
			}
    		}

    		public bool UseMnemonic {
    			get { return use_mnemonic; }
   			set {
    				if (use_mnemonic != value) {
					use_mnemonic = value;
					SetUseMnemonic (use_mnemonic);
	    				Refresh ();
    				}
    			}
    		}

    		#endregion


		#region Public Methods

    		protected Rectangle CalcImageRenderBounds (Image image, Rectangle area, ContentAlignment img_align)
    		{
    			Rectangle rcImageClip = area;
			rcImageClip.Inflate (-2,-2);

			int X = area.X;
			int Y = area.Y;

			if (img_align == ContentAlignment.TopCenter ||
				img_align == ContentAlignment.MiddleCenter ||
				img_align == ContentAlignment.BottomCenter) {
				X += (area.Width - image.Width) / 2;
			}
			else if (img_align == ContentAlignment.TopRight ||
				img_align == ContentAlignment.MiddleRight||
				img_align == ContentAlignment.BottomRight) {
				X += (area.Width - image.Width);
			}

			if( img_align == ContentAlignment.BottomCenter ||
				img_align == ContentAlignment.BottomLeft ||
				img_align == ContentAlignment.BottomRight) {
				Y += area.Height - image.Height;
			}
			else if(img_align == ContentAlignment.MiddleCenter ||
					img_align == ContentAlignment.MiddleLeft ||
					img_align == ContentAlignment.MiddleRight) {
				Y += (area.Height - image.Height) / 2;
			}

			rcImageClip.X = X;
			rcImageClip.Y = Y;
			rcImageClip.Width = image.Width;
			rcImageClip.Height = image.Height;

			return rcImageClip;
    		}

    		
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return base.CreateAccessibilityInstance ();
		}

    		protected override void Dispose(bool disposing)
		{
			base.Dispose (disposing);
		}

    		protected void DrawImage (Graphics g, Image image, Rectangle area, ContentAlignment img_align)
    		{
 			if (image == null || g == null)
				return;

			Rectangle rcImageClip = CalcImageRenderBounds (image, area, img_align);

			if (Enabled)
				g.DrawImage (image, rcImageClip.X, rcImageClip.Y, rcImageClip.Width, rcImageClip.Height);
			else
				ControlPaint.DrawImageDisabled (g, image, rcImageClip.X, rcImageClip.Y, BackColor);
		}

    		protected virtual void OnAutoSizeChanged (EventArgs e)
		{
    			if (AutoSizeChanged != null)
    				AutoSizeChanged (this, e);
    		}

    		protected override void OnEnabledChanged (EventArgs e)
    		{
			base.OnEnabledChanged (e);
    		}

    		protected override void OnFontChanged (EventArgs e)
    		{
			base.OnFontChanged (e);
			CalcPreferredHeight ();
			Refresh ();
    		}


    		protected override void OnPaint (PaintEventArgs pevent)
    		{
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw ();
			// TODO: Imagelist
			pevent.Graphics.DrawImage (ImageBuffer, 0, 0);

		}

    		protected override void OnParentChanged (EventArgs e)
    		{
    			base.OnParentChanged (e);
    		}

    		protected virtual void OnTextAlignChanged (EventArgs e)
    		{
    			if (TextAlignChanged != null)
    				TextAlignChanged (this, e);
    		}

    		protected override void OnTextChanged (EventArgs e)
    		{
			base.OnTextChanged (e);
			CalcPreferredWidth ();
			Refresh ();
    		}

    		protected override void OnVisibleChanged (EventArgs e)
    		{
    			base.OnVisibleChanged (e);
    		}

    		protected override bool ProcessMnemonic (char charCode)
    		{
    			return base.ProcessMnemonic (charCode);
    		}

    		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
    		{
    			base.SetBoundsCore (x, y, width, height, specified);
    		}

    		public override string ToString()
    		{
    			return base.ToString();
    		}

    		protected override void WndProc(ref Message m)
    		{
			switch ((Msg) m.Msg) {
				case Msg.WM_DRAWITEM: {
					m.Result = (IntPtr)1;
				}
					break;
				default:
					base.WndProc (ref m);
					break;
			}
    		}

    		#endregion Public Methods

    		#region Private Methods

		private void CalcAutoSize ()
    		{
    			if (IsHandleCreated == false)
    				return;

    			CalcPreferredWidth ();
    			CalcPreferredHeight ();

    		 	Width =  PreferredWidth;
    		 	Height =  PreferredHeight;

    		 	Invalidate ();
    		}

    		private void CalcPreferredHeight ()
		{
			preferred_height = Font.Height;

			switch (border_style) {
			case BorderStyle.None:
				preferred_height += 3;
				break;
			case BorderStyle.FixedSingle:
			case BorderStyle.Fixed3D:
				preferred_height += 6;
				break;
			default:
				break;
			}

		}

    		private void CalcPreferredWidth ()
		{
			SizeF size;
    		 	size = DeviceContext.MeasureString (Text, Font, req_witdthsize, string_format);
    		 	preferred_width = (int) size.Width + 3;
		}

    		internal void Draw ()
		{			
			ThemeEngine.Current.DrawLabel(DeviceContext, ClientRectangle, this);

			DrawImage (DeviceContext, Image, ClientRectangle, image_align);
		}

    		private void OnHandleCreatedLB (Object o, EventArgs e)
		{
			CreateBuffers (Width, Height);

			if (autosize)
				CalcAutoSize ();
		}

		private void OnResizeLB (object o, EventArgs e)
    		{
    			if (Width <= 0 || Height <= 0)
    				return;

			CreateBuffers (Width, Height);
    		}


		private void SetUseMnemonic (bool use)
    		{
			if (use)
				string_format.HotkeyPrefix = HotkeyPrefix.Show;
			else
				string_format.HotkeyPrefix = HotkeyPrefix.None;
    		}

		#endregion Private Methods

    	}
    }
