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
	
    	public class Label : Control {
    		Image background_image;
    		BorderStyle border_style;
    		bool autoSize;
    		Image image;
    		ContentAlignment image_align;
    		ImeMode default_ime_mode;
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
			base.TabStop = false;
			
			SubClassWndProc_ = true;
			SetStyle (ControlStyles.Selectable, false);
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
    
    		[MonoTODO]
    		public ImageList ImageList {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
					//FIXME:
    			}
    		}
    
    		[MonoTODO]
    		public new ImeMode ImeMode {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
					//FIXME:
				}
    		}
    
    		public int PreferredHeight {
    			get {
    				return preferred_height;
    			}
    		}
    
    		public int PreferredWidth {
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

				createParams.ClassName = "Static";

				int bs = 0;
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
					bs);

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
    
    		protected override ImeMode DefaultImeMode {
    			get {
				//FIXME:
				return default_ime_mode;
    			}
    		}
    
#endregion

#region Methods

    		public override string ToString()
    		{
			//FIXME: add name of lable, as well as text. would adding base.ToString work?
    			return "Label: " + base.Text;
    		}

    		[MonoTODO]
    		protected  Rectangle CalcImageRenderBounds (
    			Image image, Rectangle rect, ContentAlignment align)
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
    		protected  void DrawImage (Graphics g, Image img, 
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
				//FIXME:
				base.OnPaint(e);
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
				//FIXME:
				base.OnTextChanged (e);
    		}
    
    		protected override void OnVisibleChanged (EventArgs e)
    		{
    			base.OnVisibleChanged (e);
    		}
    
    		protected override bool ProcessMnemonic(char charCode)
    		{
    			return base.ProcessMnemonic (charCode);
    		}
    
    		[MonoTODO]
    		protected new ContentAlignment RtlTranslateAlignment (
    			ContentAlignment alignment)
    		{
    			throw new NotImplementedException ();
    		}
    
    		[MonoTODO]
    		protected new HorizontalAlignment RtlTranslateAlignment (
    			HorizontalAlignment alignment)
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected new LeftRightAlignment RtlTranslateAlignment (
    			LeftRightAlignment align)
    		{
    			throw new NotImplementedException ();
    		}
    
    		[MonoTODO]
    		protected new virtual void Select (bool directed, bool forward)
    		{
				//FIXME:
			}
    
    		protected override void SetBoundsCore (
    			int x, int y, int width, int height,
    			BoundsSpecified specified)
    		{
    			base.SetBoundsCore (x, y, width, height, specified);
    		}
    
    		protected new void UpdateBounds()
    		{
    			base.UpdateBounds ();
    		}
    
    		protected new void UpdateBounds (int x, int y,
    					     int width, int height)
    		{
    			base.UpdateBounds (x, y, width, height);
    		}
    
    
    		protected new void UpdateBounds (int x, int y, int width,
						 int height, int clientWidth,
						 int clientHeight)
		{
    			base.UpdateBounds (x, y, width, height, clientWidth, clientHeight);
		}
    
    		protected override void WndProc(ref Message m)
    		{
    			base.WndProc (ref m);
    		}
#endregion

#region Events
    		public event EventHandler AutoSizeChanged;
   
    		public event EventHandler TextAlignChanged;
#endregion
		
    	}
    }
