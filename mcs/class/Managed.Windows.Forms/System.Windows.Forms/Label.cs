//
// System.Windows.Forms.Label.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//	Dennis Hayes (dennish@raytek.com)
//   WineLib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) 2002/3 Ximian, Inc
//

namespace System.Windows.Forms {
    	using System.ComponentModel;
    	using System.Drawing;
    	using System.Drawing.Text;
	
    	public class Label : Control {
    		Image background_image;
    		BorderStyle border_style;
    		bool autoSize;
    		Image image;
    		ContentAlignment image_align;
		StringFormat	string_format;
//    		ImeMode default_ime_mode;
    		bool render_transparent;
    		FlatStyle flat_style;
    		int preferred_height;
    		int preferred_width;
    		bool tab_stop;
    		ContentAlignment text_align;
    		bool use_mnemonic;
    
    		public Label () : base ()
    		{
			// Defaults in the Spec
			autoSize = false;
			border_style = BorderStyle.None;
//			base.TabStop = false;
			text_align = ContentAlignment.TopLeft;
//			SetStyle (ControlStyles.Selectable, false);
//			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			string_format = new StringFormat();
			string_format.HotkeyPrefix=HotkeyPrefix.Show;
    		}

#region Properties
    		public virtual bool AutoSize {
    			get {
    				return autoSize;
    			}
    			set {
    				autoSize = value;
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
				if (border_style == value)
					return;
				
    				border_style = value;
				RecreateHandle ();
    			}
    		}
    
    
    		public FlatStyle FlatStyle {
    			get {
    				return flat_style;
    			}
    			set {
    				flat_style = value;
				Refresh ();
    			}
    		}
    
    		public Image Image {
    			get {
    				return image;
    			}
    			set {
    				image = value;
				Refresh ();
    			}
    		}
    
    		public ContentAlignment ImageAlign {
    			get {
    				return image_align;
    			}
    			set {
    				image_align = value;
    			}
    		}
    
    
    		[MonoTODO]
    		public int ImageIndex {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
					//FIXME:
			}
    		}
#if nodef    
    		[MonoTODO]
    		public ImageList ImageList {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
					//FIXME:
    			}
    		}
#endif
    
    		protected override ImeMode DefaultImeMode {
    			get {
    				return ImeMode.Disable;
    			}
    		}
    
    		public virtual int PreferredHeight {
    			get {
    				return preferred_height;
    			}
    		}
    
    		public virtual int PreferredWidth {
    			get {
    				return preferred_width;
    			}
    		}
    
    		public new bool TabStop {
    			get {
    				return tab_stop;
    			}
    			set {
    				tab_stop = value;
    			}
    		}
    
 		//Compact Framework
    		public virtual ContentAlignment TextAlign {
    			get {
    				return text_align;
    			}

    			set {
    				text_align = value;

				// Calculate vertical alignment
				if ((value == ContentAlignment.BottomLeft) || (value == ContentAlignment.BottomCenter) || (value == ContentAlignment.BottomRight)) {
					string_format.LineAlignment=StringAlignment.Far;
				} else  if ((value == ContentAlignment.TopLeft) || (value == ContentAlignment.TopCenter) || (value == ContentAlignment.TopRight)) {
					string_format.LineAlignment=StringAlignment.Near;
				} else {
					string_format.LineAlignment=StringAlignment.Center;
				}

				// Calculate horizontal alignment
				if ((value == ContentAlignment.TopLeft) || (value == ContentAlignment.MiddleLeft) || (value == ContentAlignment.BottomLeft)) {
					string_format.Alignment=StringAlignment.Near;
				} else  if ((value == ContentAlignment.TopRight) || (value == ContentAlignment.MiddleRight) || (value == ContentAlignment.BottomRight)) {
					string_format.LineAlignment=StringAlignment.Far;
				} else {
					string_format.LineAlignment=StringAlignment.Center;
				}
			}
    		}
    
    		public bool UseMnemonic {
    			get {
    				return use_mnemonic;
    			}
    			set {
    				use_mnemonic = value;
    			}
    		}
		
    		protected override CreateParams CreateParams {
    			get {
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = XplatUI.DefaultClassName;

				int bs = 0;
#if notyet
				if (border_style == BorderStyle.FixedSingle)
					bs |= (int) WindowStyles.WS_BORDER;
				else if (border_style == BorderStyle.Fixed3D)
					bs |= (int) WindowStyles.WS_BORDER | (int) SS_Static_Control_Types.SS_SUNKEN;
					
				createParams.Style = (int) (
					(int)WindowStyles.WS_CHILD | 
					(int)WindowStyles.WS_VISIBLE | 
					(int)SS_Static_Control_Types.SS_LEFT |
					(int)WindowStyles.WS_CLIPCHILDREN |
					(int)WindowStyles.WS_CLIPSIBLINGS |
					(int)SS_Static_Control_Types.SS_OWNERDRAW |
					bs);
#else
				createParams.Style = (int)WindowStyles.WS_CHILD | (int)WindowStyles.WS_VISIBLE |(int)WindowStyles.WS_CLIPCHILDREN | (int)WindowStyles.WS_CLIPSIBLINGS;
#endif


				return createParams;
    			}
    		}
    
    		protected override Size DefaultSize {
    			get {
    				return new Size(100,23);//Correct value
    			}
    		}
    
			protected virtual bool RenderTransparent {
				get {
					return render_transparent;
				}
				set {
					//FIXME:
				}
			}
#if nodef
    		protected override ImeMode DefaultImeMode {
    			get {
				//FIXME:
				return default_ime_mode;
    			}
    		}
#endif
    
#endregion

#region Methods

    		public override string ToString()
    		{
			//FIXME: add name of lable, as well as text. would adding base.ToString work?
    			return "Label: " + base.Text;
    		}

    		[MonoTODO]
			protected override void Dispose(bool disposing){
				base.Dispose(disposing);
			}

    		[MonoTODO]
    		protected Rectangle CalcImageRenderBounds (
    			Image image, Rectangle r, ContentAlignment align)
    		{
    			throw new NotImplementedException ();
    		}
    
      		[MonoTODO]
      		protected  override AccessibleObject CreateAccessibilityInstance()
      		{
				//FIXME:
				return base.CreateAccessibilityInstance();
      		}

    		[MonoTODO]
    		protected  void DrawImage (Graphics g, Image image, 
    					   Rectangle r, ContentAlignment align)
    		{
				//FIXME:
			}
    
    		protected virtual void OnAutoSizeChanged (EventArgs e)
		{
    			if (AutoSizeChanged != null) AutoSizeChanged (this, e);
    		}
    
    		protected override void OnEnabledChanged (EventArgs e)
    		{
				//FIXME:
				base.OnEnabledChanged (e);
    		}
    
    		protected override void OnFontChanged (EventArgs e)
    		{
				//FIXME:
				base.OnFontChanged (e);
    		}
    
    		protected override void OnPaint (PaintEventArgs e)
    		{
				Rectangle paintBounds = ClientRectangle;
				Bitmap bmp = new Bitmap( paintBounds.Width, paintBounds.Height,e.Graphics);
				Graphics paintOn = Graphics.FromImage(bmp);
			
				Color controlColor = BackColor; //SystemColors.Control;
				Color textColor = ForeColor; // SystemColors.ControlText;
				if (BackColor == System.Drawing.Color.Red) {
					Color t = System.Drawing.Color.Red;
				}
			
				Rectangle rc = paintBounds;
				Rectangle rcImageClip = paintBounds;
				rcImageClip.Inflate(-2,-2);

				SolidBrush sb = new SolidBrush( controlColor);
				paintOn.FillRectangle(sb, rc);
				sb.Dispose();
				
				// Do not place Text and Images on the borders 
				paintOn.Clip = new Region(rcImageClip);
				if(Image != null) {
					int X = rc.X;
					int Y = rc.Y;

					if( ImageAlign == ContentAlignment.TopCenter ||
						ImageAlign == ContentAlignment.MiddleCenter ||
						ImageAlign == ContentAlignment.BottomCenter) {
						X += (rc.Width - Image.Width) / 2;
					}
					else if(ImageAlign == ContentAlignment.TopRight ||
						ImageAlign == ContentAlignment.MiddleRight||
						ImageAlign == ContentAlignment.BottomRight) {
						X += (rc.Width - Image.Width);
					}

					if( ImageAlign == ContentAlignment.BottomCenter ||
						ImageAlign == ContentAlignment.BottomLeft ||
						ImageAlign == ContentAlignment.BottomRight) {
						Y += rc.Height - Image.Height;
					}
					else if(ImageAlign == ContentAlignment.MiddleCenter ||
							ImageAlign == ContentAlignment.MiddleLeft ||
							ImageAlign == ContentAlignment.MiddleRight) {
						Y += (rc.Height - Image.Height) / 2;
					}
					paintOn.DrawImage(Image, X, Y, Image.Width, Image.Height);
				}

				if (Enabled) {
					SolidBrush  brush;

					brush=new SolidBrush(textColor);
					paintOn.DrawString(Text, Font, brush, rc, string_format);
					brush.Dispose();
				}
				else {
					ControlPaint.DrawStringDisabled(paintOn, Text, Font, textColor, rc, string_format);
				}

				e.Graphics.DrawImage(bmp, 0, 0, paintBounds.Width, paintBounds.Height);
				paintOn.Dispose ();
				bmp.Dispose();
			}
    
  			//Compact Framework
    		protected override void OnParentChanged (EventArgs e)
    		{
    			base.OnParentChanged (e);
    		}
    
    		protected virtual void OnTextAlignChanged (EventArgs e) {
    			if (TextAlignChanged != null) TextAlignChanged (this, e);
    		}
    
 			//Compact Framework
    		protected override void OnTextChanged (EventArgs e) {
				base.OnTextChanged (e);
				Invalidate ();
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
    
//    		[MonoTODO]
//    		protected new ContentAlignment RtlTranslateAlignment (
//    			ContentAlignment alignment)
//    		{
//    			throw new NotImplementedException ();
//    		}
//    
//    		[MonoTODO]
//    		protected new HorizontalAlignment RtlTranslateAlignment (
//    			HorizontalAlignment alignment)
//    		{
//    			throw new NotImplementedException ();
//    		}
//    		
//    		[MonoTODO]
//    		protected new LeftRightAlignment RtlTranslateAlignment (
//    			LeftRightAlignment align)
//    		{
//    			throw new NotImplementedException ();
//    		}
//    
//    		[MonoTODO]
//    		protected new virtual void Select (bool directed, bool forward)
//    		{
//				//FIXME:
//			}

#if nodef    
    		protected override void SetBoundsCore (
    			int x, int y, int width, int height,
    			BoundsSpecified specified)
    		{
    			base.SetBoundsCore (x, y, width, height, specified);
    		}
#endif
    
//    		protected new void UpdateBounds()
//    		{
//    			base.UpdateBounds ();
//    		}
//    
//    		protected new void UpdateBounds (int x, int y,
//    					     int width, int height)
//    		{
//    			base.UpdateBounds (x, y, width, height);
//    		}
//    
//    
//    		protected new void UpdateBounds (int x, int y, int width,
//						 int height, int clientWidth,
//						 int clientHeight)
//		{
//    			base.UpdateBounds (x, y, width, height, clientWidth, clientHeight);
//		}

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

#region Events
    		public event EventHandler AutoSizeChanged;
   
    		public event EventHandler TextAlignChanged;
#endregion
		
    	}
    }
