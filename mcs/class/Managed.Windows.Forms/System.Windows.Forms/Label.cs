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
// TODO:
//		- The AutoSize functionality needs missing control functions to be implemented
//		- Draw BorderStyle and FlatStyle
//
// Based on work by:
//	Daniel Carrera, dcarrera@math.toronto.edu (stubbed out)
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
    		private Image background_image;
    		private BorderStyle border_style;
    		private bool autoSize;
    		private Image image;    		
    		private bool render_transparent;
    		private FlatStyle flat_style;
    		private int preferred_height;
    		private int preferred_width;
    		private bool tab_stop;    		
    		private bool use_mnemonic;
    		private int image_index = -1;
    		private ImageList image_list;
    		protected Bitmap bmp_mem;
		protected Graphics dc_mem;
		protected Rectangle paint_area;
		protected ContentAlignment image_align;
		protected StringFormat string_format;    		
    		protected ContentAlignment text_align;

    		#region Events
    		public event EventHandler AutoSizeChanged;
    		public event EventHandler TextAlignChanged;
		#endregion

    		public Label () : base ()
    		{
			// Defaults in the Spec
			autoSize = false;
			border_style = BorderStyle.None;			
			string_format = new StringFormat();
			TextAlign = ContentAlignment.TopLeft;
			image = null;
			bmp_mem = null;
			dc_mem = null;
			UseMnemonic = true;		
			image_list = null;
			paint_area = new Rectangle ();
			image_align = ContentAlignment.MiddleCenter;
			set_usemnemonic (UseMnemonic);

			BackColor = Color.FromArgb (255, 236, 233, 216);
			ForeColor = Color.Black;

			CalcPreferredHeight ();
			CalcPreferredWidth ();

			AutoSizeChanged = null;
    			TextAlignChanged = null;
    		}

		#region Public Properties

    		public virtual bool AutoSize {
    			get { return autoSize; }
    			set {
    				if (autoSize == value)
    					return;
    					
    				autoSize = value;    				
    				CalcAutoSize ();
				Refresh ();

    				if (AutoSizeChanged != null)
    					AutoSizeChanged (this, new EventArgs ());
    			}
    		}

    		public override Image BackgroundImage {
    			get {
    				return background_image;
    			}
    			set {
    				background_image = value;
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

		private void CalcPreferredHeight ()
		{
			if (font == null) { 
				preferred_height = 0;
				return;
			}

			preferred_height = Font.Height;

			if (border_style == BorderStyle.None)
				preferred_height += 3;
		}
    		
    		public virtual int PreferredHeight {
    			get { return preferred_height; }
    		}

		private void CalcPreferredWidth ()
		{
			if (font == null) { 
				preferred_width = 0;
				return;
			}

			SizeF size;    			
    		 	size = dc_mem.MeasureString (Text, Font, new SizeF (paint_area.Width,
    		 		paint_area.Height), string_format);
    		 	
    		 	preferred_width = Size.Width;
		}

    		public virtual int PreferredWidth {
    			get {return preferred_width; }
    		}

    		public bool TabStop {
    			get { return tab_stop; }
    			set { tab_stop = value; }
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
					set_usemnemonic (use_mnemonic);
	    				Refresh ();
    				}
    			}
    		}

    		#endregion

    		protected override CreateParams CreateParams {
    			get {
				CreateParams createParams = base.CreateParams;
				createParams.ClassName = XplatUI.DefaultClassName;
				createParams.Style = (int)WindowStyles.WS_CHILD | (int)WindowStyles.WS_VISIBLE |(int)WindowStyles.WS_CLIPCHILDREN | (int)WindowStyles.WS_CLIPSIBLINGS;
				return createParams;
    			}
    		}

    		protected override Size DefaultSize {
    			get {return new Size (100,23);}
    		}

		protected virtual bool RenderTransparent {
			get { return render_transparent; }
			set { render_transparent = value;}
		}

    		protected override ImeMode DefaultImeMode {
    			get { return ImeMode.Disable;}    			
    		}

		#region  Methods		
		

    		public override string ToString()
    		{
			//FIXME: add name of lable, as well as text. would adding base.ToString work?
    			return "Label: " + base.Text;
    		}
    		
		protected override void Dispose(bool disposing)
		{			
			base.Dispose (disposing);
		}

    		
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

      		
      		protected  override AccessibleObject CreateAccessibilityInstance()
      		{				
			return base.CreateAccessibilityInstance();
      		}

    		
    		protected  void DrawImage (Graphics g, Image image,   Rectangle area, ContentAlignment img_align)
    		{
 			if (image == null || g == null)
				return;
				
			Rectangle rcImageClip = CalcImageRenderBounds (image, area, img_align);		
			g.DrawImage (image, rcImageClip.X, rcImageClip.Y, rcImageClip.Width, rcImageClip.Height);
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
			Console.WriteLine ("OnFontChanged");
			base.OnFontChanged (e);
			CalcPreferredHeight ();
    		}
    		
    		private void CalcAutoSize ()
    		{
    			if (IsHandleCreated == false)
    				return;    			   	
    			
    			SizeF size;    			
    		 	size = dc_mem.MeasureString (Text, Font, new SizeF (paint_area.Width,
    		 		paint_area.Height), string_format);
    		 	
    		 	Width = Size.Width;
    		 	Height = Size.Height;
    		 	
    		 	Invalidate ();
    		 	
    		 	Console.WriteLine ("CalcAutoSize () after " + Size);
    		}

    		protected virtual void draw ()
		{
			ThemeEngine.Current.DrawLabel (dc_mem, paint_area, BorderStyle, Text, 
				ForeColor, BackColor, Font, string_format, Enabled);
				
			DrawImage (dc_mem, Image, paint_area, image_align);
		}
		
		private void set_usemnemonic (bool use)
    		{    			
			if (use)
				string_format.HotkeyPrefix = HotkeyPrefix.Show;
			else
				string_format.HotkeyPrefix = HotkeyPrefix.None;
    		}


    		protected override void OnPaint (PaintEventArgs pevent)
    		{
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			/* Copies memory drawing buffer to screen*/
			UpdateArea ();
			draw();
			pevent.Graphics.DrawImage (bmp_mem, 0, 0);

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

    		protected override bool ProcessMnemonic(char charCode)
    		{
    			return base.ProcessMnemonic (charCode);
    		}

    		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated(e);
			//Console.WriteLine ("OnHandleCreated");

			UpdateArea ();

			bmp_mem = new Bitmap (Width, Height, PixelFormat.Format32bppArgb);
			dc_mem = Graphics.FromImage (bmp_mem);
			
			if (AutoSize)
				CalcAutoSize ();
		}

		private void UpdateArea ()
		{			
			paint_area.X = 	paint_area.Y = 0;
			paint_area.Width = Width;
			paint_area.Height = Height;			
		}


		protected override void OnResize (EventArgs e)
    		{
    			//Console.WriteLine ("OnResize");
    			base.OnResize (e);

    			if (Width <= 0 || Height <= 0)
    				return;

    			UpdateArea ();

			/* Area for double buffering */
			bmp_mem = new Bitmap (Width, Height, PixelFormat.Format32bppArgb);
			dc_mem = Graphics.FromImage (bmp_mem);
    		}



#if nodef
    		protected override void SetBoundsCore (
    			int x, int y, int width, int height,
    			BoundsSpecified specified)
    		{
    			base.SetBoundsCore (x, y, width, height, specified);
    		}
#endif


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

#endregion

    	}
    }
