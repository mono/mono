    //
    // System.Windows.Forms.Label.cs
    //
    // Author:
    //   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
    //	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
    //	Dennis Hayes (dennish@raytek.com)
    //   WineLib implementation started by John Sohn (jsohn@columbus.rr.com)
    //
    // (C) 2002 Ximian, Inc
    //
    
    namespace System.Windows.Forms {
    	using System.ComponentModel;
    	using System.Drawing;
    
    	// <summary>
    	//
    	// </summary>
    	
    	public class Label : Control {
    
    		Image backgroundImage;
    		BorderStyle borderStyle;
    		bool autoSize;
    		Image image;
    		ContentAlignment imageAlign;
    		ImeMode defaultImeMode;
    		bool renderTransparent;
    		FlatStyle flatStyle;
    		int preferredHeight;
    		int preferredWidth;
    		bool tabStop;
    		ContentAlignment textAlign;
    		bool useMnemonic;
    
    		//
    		//  --- Constructor
    		//
    		public Label () : base ()
    		{
				// Defaults in the Spec
				autoSize = false;
				borderStyle = BorderStyle.None;
	
				//Defaults not in the spec
				Image backgroundImage;
				Image image;
				ContentAlignment imageAlign;
				ImeMode defaultImeMode;
				bool renderTransparent;
				FlatStyle flatStyle;
				int preferredHeight;
				int preferredWidth;
				bool tabStop;
				ContentAlignment textAlign;
				bool useMnemonic;

				SubClassWndProc_ = true;
    		}
    		
    		//
    		//  --- Public Properties
    		//
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
    				return backgroundImage;
    			}
    			set {
    				backgroundImage = value;
    				// FIXME: force redraw
    			}
    		}
    
    		public virtual BorderStyle BorderStyle {
    			get {
    				return borderStyle;
    			}
    			set {
    				borderStyle = value;
    			}
    		}
    
    
    		public FlatStyle FlatStyle {
    			get {
    				return flatStyle;
    			}
    			set {
    				flatStyle = value;
    			}
    		}
    
    		public Image Image {
    			get {
    				return image;
    			}
    			set {
    				image = value;
    			}
    		}
    
    		public ContentAlignment ImageAlign {
    			get {
    				return imageAlign;
    			}
    			set {
    				imageAlign = value;
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
    				return preferredHeight;
    			}
    		}
    
    		public int PreferredWidth {
    			get {
    				return preferredWidth;
    			}
    		}
    
    		public new bool TabStop {
    			get {
    				return tabStop;
    			}
    			set {
    				tabStop = value;
    			}
    		}
    
 		//Compact Framework
    		public virtual ContentAlignment TextAlign {
    			get {
    				return textAlign;
    			}
    			set {
    				textAlign = value;
    			}
    		}
    
    		public bool UseMnemonic {
    			get {
    				return useMnemonic;
    			}
    			set {
    				useMnemonic = value;
    			}
    		}
    
    		//
    		//  --- Protected Properties
    		//
    
    		protected override CreateParams CreateParams {
    			get {
					if( Parent != null) {
						CreateParams createParams = new CreateParams ();

						if(window == null) {
							window = new ControlNativeWindow (this);
						}
		 
						createParams.Caption = Text;
						createParams.ClassName = "Static";
						createParams.X = Left;
						createParams.Y = Top;
						createParams.Width = Width;
						createParams.Height = Height;
						createParams.ClassStyle = 0;
						createParams.ExStyle = 0;
						createParams.Param = 0;
						createParams.Parent = Parent.Handle;
						createParams.Style = (int) (
							(int)WindowStyles.WS_CHILD | 
							(int)WindowStyles.WS_VISIBLE | 
							(int)SS_Static_Control_Types.SS_LEFT );
						window.CreateHandle (createParams);
						return createParams;
					}
					return null;
    			}
    		}
    
    		protected override Size DefaultSize {
    			get {
    				return new Size(100,23);//Correct value
    			}
    		}
    
    		protected virtual bool RenderTransparent {
    			get {
    				return renderTransparent;
    			}
    			set {
					//FIXME:
				}
    		}
    
    		protected override ImeMode DefaultImeMode {
    			get {
					//FIXME:
					return defaultImeMode;
    			}
    		}
    
    		//
    		//  --- Public Methods
    		//

    		public new void Select()
    		{
				//FIXME:
				base.Select ();
    		}
    
  		//Compact Framework
    		public override string ToString()
    		{
				//FIXME: add name of lable, as well as text. would adding base.ToString work?
    			return "Label: " + base.Text;
    		}
    
    		//
    		//  --- Public Events
    		// 
    		public event EventHandler AutoSizeChanged;
   
    		public event EventHandler TextAlignChanged;
    
    		//
    		//  --- Protected Methods
    		//
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
    
    		protected virtual void OnAutoSizeChanged (EventArgs e) {
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
    			base.UpdateBounds (x, y, width, height, clientWidth, 
    					   clientHeight);
    		}
    
    		protected override void WndProc(ref Message m)
    		{
    			base.WndProc (ref m);
    		}
    	}
    }
