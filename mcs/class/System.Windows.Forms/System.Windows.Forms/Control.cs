    //
    // System.Windows.Forms.Control.cs
    //
    // Author:
    //   stubbed out by Jaak Simm (jaaksimm@firm.ee)
    //	Dennis Hayes (dennish@rayetk.com)
    //   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
    //
    // (C) Ximian, Inc., 2002
    //
    
    using System.ComponentModel;
    using System.Drawing;
    using System.Collections;
    
    namespace System.Windows.Forms {
    
    	/// <summary>
    	/// Defines the base class for controls, which are components with 
    	/// visual representation.
    	/// </summary>
    	
    	public class Control : Component , ISynchronizeInvoke, IWin32Window {
    

    		// Helper NativeWindow class to dispatch messages back
    		// to the Control class
    		protected class ControlNativeWindow : NativeWindow {
    
    			private Control control;
    
    			public ControlNativeWindow (Control control) : base() {
    				this.control = control;
    			}
    
    			protected override void WndProc (ref Message m) {
    				base.WndProc (ref m);
    
     				control.WndProc (ref m);
    			}
    		}
    		
    		// FIXME: not sure if dervied classes should have access
    		protected ControlNativeWindow window;
    		private ControlCollection childControls;
    		private Control parent;
    
    		// private fields
    		// it seems these are stored in case the window is not created,
    		// corresponding properties (below) need to check if window
    		// is created or not and react accordingly
    		string accessibleDefaultActionDescription;
    		string accessibleDescription;
    		string accessibleName;
    		AccessibleRole accessibleRole;
    		bool allowDrop;
    		AnchorStyles anchor;
    		Color backColor;
    		Image backgroundImage;
    		//BindingContext bindingContext;
    		Rectangle bounds;
    		bool causesValidation;
    		//ContextMenu contextMenu;
    		DockStyle dock;
    		bool enabled;
    		Font font;
    		Color foreColor;
    		ImeMode imeMode;
    		bool isAccessible;
    		// Point location;  // using bounds to store location
    		string name;
    		Region region;
    		RightToLeft rightToLeft;
    		bool tabStop;
    		string text;
    		bool visible;
    
    		// --- Constructors ---
    
	 		//Compact Framework //only Control()
    		public Control ()
    		{
    			CreateControlsInstance ();
    
    			accessibleDefaultActionDescription = null;
    			accessibleDescription = null;
    			accessibleName = null;
    			accessibleRole = AccessibleRole.Default;
    			allowDrop = false;
    			anchor = AnchorStyles.Top | AnchorStyles.Left;
    			//backColor = Control.DefaultBackColor;
    			backgroundImage = null;
    			bounds = new Rectangle();
    			// bindingContext = null;
    			causesValidation = true;
    			// contextMenu = null;
    			dock = DockStyle.None;
    			enabled = true;
    			// font = Control.DefaultFont;
    			// foreColor = Control.DefaultForeColor;
    			imeMode = ImeMode.Inherit;
    			isAccessible = false;
    			// location = new Point (0,0); should be from OS
    			name = "";
    			region = null;
    			rightToLeft = RightToLeft.Inherit;
    			tabStop = false;
    			text = "";
    			visible = true;
    			parent = null;
    			CreateHandle();//sets window handle. FIXME: No it does not
    		}
    		
    		// according to docs, the constructors do not create 
    		// the (HWND) window
    		public Control (string text) : this() 
    		{
    			Text = text;
    			// SetWindowTextA (Handle, text);
    		}
    		
    		public Control (Control parent, string text) : this (text) 
    		{
    			Parent = parent;
    			// SetParent (Handle, parent.Handle);
    		}
    		
    		public Control (string text, int left, int top, 
    				int width, int height) : this(text) 
    		{
    			Left = left;
    			Top = top;
    			Width = width;
    			Height = height;
    			//SetWindowPos (Handle, (IntPtr) 0, left, top,
    			//	    width, height, 0);
    		}
    		
    		public Control (Control parent,string text,int left, int top,
    				int width,int height) : this (parent, text)
    		{
    			Left = left;
    			Top = top;
    			Width = width;
    			Height = height;
    			// SetWindowPos (Handle, (IntPtr) 0, left, top,
    			//		    width, height, 0);
    		}
    
    		// for internal use only, create a control class
    		// for an existing, created HWND
     		private Control (IntPtr existingHandle)
     		{
     			window = (ControlNativeWindow) NativeWindow.FromHandle (
     				existingHandle);
     		}
    
    		// --- Properties ---
    		// Properties only supporting .NET framework, not stubbed out:
    		//  - protected bool RenderRightToLeft {get;}
    		//  - public IWindowTarget WindowTarget {get; set;}
    		//[MonoTODO]
    		//public AccessibleObject AccessibilityObject {
    		//	get {
    		//		throw new NotImplementedException ();
    		//	}
    		//}
    
    		public string AccessibleDefaultActionDescription {
    			get {
    				return accessibleDefaultActionDescription;
    			}
    			set {
    				accessibleDefaultActionDescription = value;
    			}
    		}
    		
    		public string AccessibleDescription {
    			get {
    				return accessibleDescription;
    			}
    			set {
    				accessibleDescription=value;
    			}
    		}
    		
    		public string AccessibleName {
    			get {
    				return accessibleName;
    			}
    			set {
    				accessibleName=value;
    			}
    		}
    		
    		public AccessibleRole AccessibleRole {
    			get {
    				return accessibleRole;
    			}
    			set {
    				accessibleRole=value;
    			}
    		}
    		
    		public virtual bool AllowDrop {
    			get {
    				return allowDrop;
    			}
    			set {
    				allowDrop=value;
    			}
    		}
    	
    		public virtual AnchorStyles Anchor {
    			get {
    				return anchor;
    			}
    			set {
    				anchor=value;
    			}
    		}
    		
	  		//Compact Framework
    		public virtual Color BackColor {
    			get {
    				if (IsHandleCreated) {
    					IntPtr dc = Win32.GetDC (Handle);
    					uint bgColor = Win32.GetBkColor (dc);
    					Win32.ReleaseDC (Handle, dc);
    					int r = (int) (bgColor & 0xFF);
    					int g = (int) ((bgColor >> 8) & 0xFF);
    					int b = (int) ((bgColor >> 16) & 0xFF);
    					return Color.FromArgb (r, g, b);
    				}  else return backColor;
    			}
    			set {
    				backColor = value;
    				if (IsHandleCreated) {
    					IntPtr dc = Win32.GetDC (Handle);
    					Win32.SetBkColor (dc, (uint) value.ToArgb());
    					Win32.ReleaseDC (Handle, dc);
    				}
    			}
    		}
    		
    		public virtual Image BackgroundImage {
    			get {
    				return backgroundImage;
    			}
    			set {
    				backgroundImage = value;
    				// FIXME: force redraw
    			}
    		}
    
    		// waiting for BindingContext
    		//public virtual BindingContext BindingContext {
    		//	get {
    		//		//return bindingContext;
    		//		throw new NotImplementedException ();
    		//	}
    		//	set {
    		//		//bindingContext=value;
    		//		throw new NotImplementedException ();
    		//	}
    		//}
    		
	 		//Compact Framework
    		public int Bottom {
    			get {
    				return Top + Height;
    			}
    		}
    		
 			//Compact Framework
    		public Rectangle Bounds {
    			get {
    				if (IsHandleCreated) {
    					RECT rect = new RECT();
    					Win32.GetWindowRect (Handle, ref rect);
    					return new Rectangle ((int) rect.left, 
    							      (int) rect.top,
    							      (int) rect.right, 
    							      (int) rect.bottom);
    				} else return bounds;
    			}
    			set {
    				if (IsHandleCreated)
    					Win32.SetWindowPos (
    						//Handle, (IntPtr) 0, value.X, value.Y,
							Handle, SetWindowPosZOrder.HWND_TOPMOST, value.X, value.Y,
							value.Width, value.Height, 0);
    				else bounds = value;
    			}
    		}
    		
    		public bool CanFocus {
    			get {
    				if (IsHandleCreated && Visible && Enabled)
    					return true;
    				return false;
    			}
    		}
    		
    		[MonoTODO]
    		public bool CanSelect {
    			get {
    // 				if (ControlStyles.Selectable &&
    // 				    isContainedInAnotherControl &&
    // 				    parentIsVisiable && isVisialbe &&
    // 				    parentIsEnabled && isEnabled) {
    // 					return true;
    // 				}
    // 				return false;
    
    				throw new NotImplementedException ();
    			}
    		}
    		
	 		//Compact Framework
    		public bool Capture {
    			get {
    				if (IsHandleCreated) {
    					IntPtr captured = Win32.GetCapture ();
    					if (Handle == captured) 
    						return true;
    				}
    				return false;
    			}
    			set {
    				if (IsHandleCreated) {
    					if (value)
    						Win32.SetCapture (Handle);
    					else {
    						IntPtr captured = Win32.GetCapture ();
    
    						// if this window is in capture state
    						// release it
    						if (Handle == captured)
    							Win32.ReleaseCapture ();
    					}
    				}
    			}
    		}
    		
    		public bool CausesValidation {
    			get {
    				return causesValidation;
    			}
    			set {
    				causesValidation=value;
    			}
    		}
    		
	 		//Compact Framework
    		public Rectangle ClientRectangle {
    			get {
    				if (IsHandleCreated) {
    					RECT rect = new RECT();
    					Win32.GetClientRect (Handle, ref rect);
    					return new Rectangle ((int) rect.left, 
    							      (int) rect.top,
    							      (int) rect.right, 
    							      (int) rect.bottom);
    				}
    
    				// FIXME: is the correct return value for
    				// window who's handle is not created
    				return new Rectangle (0, 0, 0, 0);
    			}
    		}
    		
	 		//Compact Framework
			[MonoTODO]
			public Size ClientSize {
				get {
					if (IsHandleCreated) {
						RECT rect = new RECT();
						Win32.GetClientRect (Handle, ref rect);
						return new Size (
							(int) rect.right, 
							(int) rect.bottom);
					}
					// FIXME: is the correct return value for
					// window who's handle is not created
					return new Size (0, 0);
				}
				set {
					//How do set this???
					throw new NotImplementedException ();
				}
			}
    		
    		[MonoTODO]
    		public string CompanyName {
    			get {
					//Better than throwing an execption
    				return "Company Name";
    			}
    		}
    		
    		public bool ContainsFocus {
    			get {
    				if (IsHandleCreated) {
    					IntPtr focusedWindow = Win32.GetFocus();
    					if (focusedWindow == Handle)
    						return true;
    				}
    				return false;
    			}
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public virtual ContextMenu ContextMenu {
    			get {
    				//return contextMenu;
    				throw new NotImplementedException ();
    			}
    			set {
    				//contextMenu=value;
    				throw new NotImplementedException ();
    			}
    		}
    		
    		public ControlCollection Controls {
    			get { return childControls; }
    		}
    		
    		public bool Created {
    			get { 
    				if (Handle != (IntPtr) 0)
    					return true;
    				return false;
    			}
    		}
    		
    		protected virtual CreateParams CreateParams {
    			get {
  					CreateParams createParams = new CreateParams ();
  					createParams.Caption = Text;
  					createParams.ClassName = "mono_native_window";
  					createParams.X = Left;
  					createParams.Y = Top;
  					createParams.Width = Width;
  					createParams.Height = Height;
  					createParams.ClassStyle = 0;
  					createParams.ExStyle = 0;
  					createParams.Param = 0;
  				
  					if (parent != null)
  						createParams.Parent = parent.Handle;
  					else 
  						createParams.Parent = (IntPtr) 0;
	  
  					createParams.Style = (int) WindowStyles.WS_OVERLAPPEDWINDOW;
	  
    				return createParams;
    			}
    		}
    		
    		[MonoTODO]
    		public virtual Cursor Cursor {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
    				throw new NotImplementedException ();
    			}
    		}
    		
	  		//Compact Framework
    		[MonoTODO]
    		// waiting for BindingContext; should be stubbed now
    		public ControlBindingsCollection DataBindings {
    			get {
    				throw new NotImplementedException ();
    			}
    		}
    		
    		public static Color DefaultBackColor {
    			get {
    				// FIXME: use GetSystemMetrics?
    				//return SystemColors.Control;
    				throw new NotImplementedException ();
    			}
    		}
    
    		//[MonoTODO]
    		// FIXME: use GetSystemMetrics?
     		//public static Font DefaultFont {
    			// FIXME: get current system font from GenericSansSerif
    			//        call ArgumentException not called
    		//	get {
    		//		throw new NotImplementedException ();
    				//return (FontFamily.GenericSansSerif); 
    		//	}
    		//}
    		
    		public static Color DefaultForeColor {
    			get {
    				return SystemColors.ControlText;
    			}
    		}
    		
    		protected virtual ImeMode DefaultImeMode {
    			get {
    				return ImeMode.Inherit;
    			}
    		}
    		
    		protected virtual Size DefaultSize {
    			get {
					//Default label size, this should be correct.
    				return new Size(100,23);
    			}
    		}
    		
    		public virtual Rectangle DisplayRectangle {
    			get {
    				return ClientRectangle;
    			}
    		}
    		
    		[MonoTODO]
    		public bool Disposing {
    			get {
    				throw new NotImplementedException ();
    			}
    		}
    		
    		public virtual DockStyle Dock {
    			get {
    				return dock;
    			}
    			set {
    				dock=value;
    			}
    		}
    
	  		//Compact Framework
    		public virtual bool Enabled {
    			get {
    				return Win32.IsWindowEnabled (Handle);
    			}
    			set {
    				Win32.EnableWindow (Handle, value);
    			}
    		}
    		
  			//Compact Framework
    		public virtual bool Focused {
    			get {
    				return ContainsFocus;
    			}
    		}
    		
  			//Compact Framework
    		public virtual Font Font {
				get {
					throw new NotImplementedException ();
					//uint ReturnValue = SendMessage(Handle, WM_GETFONT, IntPtr.Zero, IntPtr.Zero);
					//if(ReturnValue != 0)throw new Exception("Could not get Font",null);
					//return Handle;
				}
    			set {
					throw new NotImplementedException ();
					//uint ReturnValue = SendMessage(Handle, WM_SETFONT, value, (IntPtr)1 );
					//if(ReturnValue != 0)throw new Exception("Could not set Font",null);
				}
    		}
    		
    		[MonoTODO]
    		protected int FontHeight {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
    				throw new NotImplementedException ();
    			}
    		}
    		
  			//Compact Framework
    		public virtual Color ForeColor {
    			get {
    				return foreColor;
    			}
    			set {
    				foreColor = value;
    			}
    		}
    		
    		public bool HasChildren {
    			get {
    				if (childControls.Count >0) 
    					return true;
    				return false;
    			}
    		}
    		
  			//Compact Framework
    		public int Height {
    			get {
    				if (IsHandleCreated) {
    					// FIXME: GetWindowPos
    				}
    				return bounds.Height;
    			}
    			set {
    				bounds.Height = value;
    				if (IsHandleCreated) {
    					// FIXME: SetWindowPos
    				}
    			}
    		}
    		
    		public ImeMode ImeMode {
    			// CHECKME:
    			get {
    				return imeMode;
    			}
    			set {
    				imeMode=value;
    			}
    		}
    		
    		public bool IsAccessible {
    			// CHECKME:
    			get {
    				return isAccessible;
    			} // default is false
    			set {
    				isAccessible=value;
    			}
    		}
    		
    		public bool IsDisposed {
    			get {
    				if (Handle == (IntPtr) 0)
    					return true;
    				return false;
    			}
    		}
    		
    		public bool IsHandleCreated {
    			get {
    				if (Handle != (IntPtr) 0)
    					return true;
    				return false;
    			}
    		}
    		
  			//Compact Framework
			public int Left {
				get {
					if (IsHandleCreated) {
						// FIXME: GetWindowPos
						return 0;
					} else return bounds.X;
				}
				set {
					bounds.X = value;
 
					if (IsHandleCreated) {
						// FIXME: SetWindowPos
					}
				}
			}
 		
 			//Compact Framework
			public Point Location {
				// CHECKME:
				get {
					return new Point (Top, Left);
				}
				set {
					bounds.X = value.X;
					bounds.Y = value.Y;
    
					if (IsHandleCreated) {
						// FIXME: SetWindowPos
					}
    
				}
			}
    		
    		[MonoTODO]
    		public static Keys ModifierKeys {
    			get {
    				throw new NotImplementedException ();
    			}
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public static MouseButtons MouseButtons {
    			get {
    				// FIXME: use GetAsycKeyState?
    				throw new NotImplementedException ();
    			}
    		}
    		
 			//Compact Framework
    		public static Point MousePosition {
    			get {
    				POINT point = new POINT();
    				Win32.GetCursorPos (ref point);
    				return new Point ( (int) point.x, (int) point.y);
    			}
    		}
    		
    		public string Name {
    			// CHECKME:
    			get {
    				return name;
    			}
    			set {
    				name = value;
    			}
    		}
    		
 			//Compact Framework
    		public Control Parent {
    			get {
    				return parent;
    				//IntPtr parent = GetParent (Handle);
    				//return FromHandle (parent);
    			}
    			set {
    				Console.WriteLine ("setting parent");
    				parent = value;
    
    				Console.WriteLine ("add ourself to the parents control");
    				// add ourself to the parents control
    				parent.Controls.Add (this);
    
    				Console.WriteLine ("SetParent");
    				if (IsHandleCreated) {
    					Console.WriteLine ("Handle created");
    					Win32.SetParent (Handle, value.Handle);
    				}
    			}
    		}
    		
    		[MonoTODO]
    		public string ProductName {
    			get {
    				throw new NotImplementedException ();
    			}
    		}
    		
    		[MonoTODO]
    		public string ProductVersion {
    			get {
    				throw new NotImplementedException ();
    			}
    		}
    		
    		[MonoTODO]
    		public bool RecreatingHandle {
    			get {
    				throw new NotImplementedException ();
    			}
    		}
    		
    		public Region Region {
    			// CHECKME:
    			get {
    				return region;
    			}
    			set {
    				region=value;
    			}
    		}
    		
    		[MonoTODO]
    		protected bool ResizeRedraw {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
    				throw new NotImplementedException ();
    			}
    		}
    		
 			//Compact Framework
    		public int Right {
    			get {
    				return Left + Width;
    			}
    		}
    		
    		[MonoTODO]
    		public virtual RightToLeft RightToLeft {
    			// CHECKME:
    			get {
    				return rightToLeft;
    			}
    			set {
    				rightToLeft=value;
    			}
    		}
    		
    		[MonoTODO]
    		protected virtual bool ShowFocusCues {
    			get {
    				throw new NotImplementedException ();
    			}
    		}
    		
    		[MonoTODO]
    		protected bool ShowKeyboardCues {
    			get {
    				throw new NotImplementedException ();
    			}
    		}
    		
    		[MonoTODO]
    		public override ISite Site {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
    				throw new NotImplementedException ();
    			}
    		}
    		
  			//Compact Framework
    		public Size Size {
				//FIXME: should we return client size or someother size???
    			get {
					RECT WindowRectangle;
					WindowRectangle = new RECT();
					if(!Win32.GetWindowRect(Handle,ref WindowRectangle)){
						//throw new Exception("couild not retreve Control Size");
					}
					return new Size(WindowRectangle.right - WindowRectangle.left,
						WindowRectangle.bottom - WindowRectangle.top);
    			}
    			set {
    				Win32.SetWindowPos(Handle, 0, 0, 0, this.Size.Width, this.Size.Height,
						SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOMOVE | 
						SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE);
    			}
    		}
    		
    		[MonoTODO]
    		public int TabIndex {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
    				throw new NotImplementedException ();
    			}
    		}
    		
    		public bool TabStop {
    			// CHECKME:
    			get {
    				return tabStop;
    			}
    			set {
    				tabStop = value;
    			}
    		}
    		
    		[MonoTODO]
    		public object Tag {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
    				throw new NotImplementedException ();
    			}
    		}
    		
 			//Compact Framework
    		public virtual string Text {
    			get {
    				if (IsHandleCreated) {
    					String text = "";
    					int length = Win32.GetWindowTextLengthA (Handle);
    					Win32.GetWindowTextA (Handle, ref text, length);
    					return text;
    				} 
					else{
						return text;
					}
    			}
    			set {
    				text = value;
    
    				if (IsHandleCreated)
    					Win32.SetWindowTextA (Handle, value);
    			}
    		}
    		
 			//Compact Framework
    		public int Top {
    			get {
    				if (IsHandleCreated) {
    					// FIXME: GetWindowPos
    					return 0;
    				} else return bounds.Top;
 			}
 			set {
 				bounds.Y = value;
 
 				if (IsHandleCreated) {
 					// FIXME: SetWindowPos
 				}
 			}
 		}
 		
    		[MonoTODO]
    		public Control TopLevelControl {
    			get {
    				throw new NotImplementedException ();
    			}
    		}
    
  			//Compact Framework
    		public bool Visible {
    			get {
    				throw new NotImplementedException ();
    			}
    			set {
    				if (value)
    					Win32.ShowWindow (
    						Handle, ShowWindowStyles.SW_SHOW);
    				else
    					Win32.ShowWindow (
    						Handle, ShowWindowStyles.SW_HIDE);
    			}
    		}
    		
 			//Compact Framework
    		public int Width {
    			get {
    				if (IsHandleCreated) {
    					// FIXME: GetWindowPos
    				}
    				return bounds.Width;
    			}
    			set {
    				bounds.Width = value;
    				if (IsHandleCreated) {
    					// FIXME: SetWindowPos
    				}
    			}
    		}
    		
    		/// --- methods ---
    		/// internal .NET framework supporting methods, not stubbed out:
    		/// - protected virtual void NotifyInvalidate(Rectangle invalidatedArea)
    		/// - protected void RaiseDragEvent(object key,DragEventArgs e);
    		/// - protected void RaiseKeyEvent(object key,KeyEventArgs e);
    		/// - protected void RaiseMouseEvent(object key,MouseEventArgs e);
    		/// - protected void RaisePaintEvent(object key,PaintEventArgs e);
    		/// - protected void ResetMouseEventArgs();
    		
    		[MonoTODO]
    		protected void AccessibilityNotifyClients (
    			AccessibleEvents accEvent,int childID) 
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public void BringToFront () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		public bool Contains (Control ctl) 
    		{
    			return childControls.Contains (ctl);
    		}
    		
    		public void CreateControl () 
    		{
    			CreateHandle ();
    		}
    
    		[MonoTODO]
    		protected virtual AccessibleObject CreateAccessibilityInstance() {
    			throw new NotImplementedException ();
    		}
    		
    		protected virtual ControlCollection CreateControlsInstance ()
    		{
    			childControls = new ControlCollection (this);
    			return childControls;
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public Graphics CreateGraphics () 
    		{
    			throw new NotImplementedException ();
    		}
    	
    		protected virtual void CreateHandle ()
    		{
    			window = new ControlNativeWindow (this);
    			
  			window.CreateHandle (CreateParams);
    		}
    	
    		protected virtual void DefWndProc (ref Message m)
    		{
    			window.DefWndProc(ref m);
    		}
    		
    		protected virtual void DestroyHandle ()
    		{
    			window.DestroyHandle ();
    		}
    	
    		[MonoTODO]
    		protected override void Dispose (bool disposing) 
    		{
    			throw new NotImplementedException ();
    		}
    	
    		[MonoTODO]
    		public DragDropEffects DoDragDrop (
    			object data, DragDropEffects allowedEffects)
    		{
    			throw new NotImplementedException ();
    		}
    	
    		//public object EndInvoke(IAsyncResult asyncResult):
    		//look under ISynchronizeInvoke methods
    	
    		[MonoTODO]
    		public Form FindForm () 
    		{
    			throw new NotImplementedException ();
    		}
    	
 			//Compact Framework
    		public bool Focus () 
    		{
    			if (Win32.SetFocus (Handle) != (IntPtr) 0)
    				return true;
    			return false;
    		}
    	
    		[MonoTODO]
    		public static Control FromChildHandle (IntPtr handle) 
    		{
    			throw new NotImplementedException ();
    		}
    	
    		public static Control FromHandle (IntPtr handle) 
    		{
    			Control control = new Control (handle);
    			return control;
    		}
    	
    		[MonoTODO]
    		public Control GetChildAtPoint (Point pt) 
    		{
    			throw new NotImplementedException ();
    		}
    	
    		// [MonoTODO]
    		//public IContainerControl GetContainerControl () 
    		//{
    		//	throw new NotImplementedException ();
    		//}
    		
    		[MonoTODO]
    		public Control GetNextControl (Control ctl, bool forward) 
    		{
    			throw new NotImplementedException ();
    		}
    	
    		[MonoTODO]
    		protected bool GetStyle (ControlStyles flag) 
    		{
    			throw new NotImplementedException ();
    		}
    	
    		[MonoTODO]
    		protected bool GetTopLevel () 
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		public void Hide ()
     		{
    			if (IsHandleCreated)
    				Win32.ShowWindow (Handle, ShowWindowStyles.SW_HIDE);
    		}
    		
    		[MonoTODO]
    		protected virtual void InitLayout () 
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		public void Invalidate () 
    		{
    			if (IsHandleCreated) {
					//FIXME: 
    				//RECT rect = (RECT) null;
    				//InvalidateRect (Handle, ref rect, true);
    			}
    		}
    		
    		[MonoTODO]
    		public void Invalidate (bool invalidateChildren) 
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		public void Invalidate (Rectangle rc) 
    		{
    			if (IsHandleCreated) {
    				RECT rect = new RECT();
    				rect.left = rc.Left;
    				rect.top = rc.Top;
    				rect.right = rc.Right;
    				rect.bottom = rc.Bottom;
    				Win32.InvalidateRect (Handle, ref rect, true);
    			}
    		}
    		
    		//[MonoTODO]
    		public void Invalidate(Region region) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void Invalidate (Rectangle rc, bool invalidateChildren) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		//[MonoTODO]
    		public void Invalidate(Region region,bool invalidateChildren) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected void InvokeGotFocus (Control toInvoke, EventArgs e) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected void InvokeLostFocus (Control toInvoke, EventArgs e) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected void InvokeOnClick (Control toInvoke, EventArgs e) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected void InvokePaint (Control c, PaintEventArgs e) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected void InvokePaintBackground (
    			Control c,PaintEventArgs e) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual bool IsInputChar (char charCode)
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual bool IsInputKey (Keys keyData) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public static bool IsMnemonic (char charCode,string text)
    		{
    			throw new NotImplementedException ();
    		}
    		
    		// methods used with events:
    		protected virtual void OnBackColorChanged (EventArgs e)
    		{
    			if (BackColorChanged != null)
    				BackColorChanged (this, e);
    		}
    		
    		protected virtual void OnBackgroundImageChanged (EventArgs e)
    		{
    			if (BackgroundImageChanged != null) 
    				BackgroundImageChanged (this, e);
    		}
    		
    		protected virtual void OnBindingContextChanged (EventArgs e)
    		{
    			if (BindingContextChanged != null)
    				BindingContextChanged (this, e);
    		}
    		
    		protected virtual void OnCausesValidationChanged (EventArgs e)
    		{
    			if (CausesValidationChanged != null)
    				CausesValidationChanged (this, e);
    		}
    		
    		protected virtual void OnChangeUICues(UICuesEventArgs e) 
    		{
    			if (ChangeUICues != null)
    				ChangeUICues (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnClick (EventArgs e)
    		{
    			if (Click != null)
    				Click (this, e);
    		}
    		
    
    		protected virtual void OnContextMenuChanged (EventArgs e)
    		{
    			if (ContextMenuChanged != null)
    				ContextMenuChanged (this, e);
    		}
    		
    		protected virtual void OnControlAdded (ControlEventArgs e)
    		{
    			if (ControlAdded != null)
    				ControlAdded (this, e);
    		}
    		
    		protected virtual void OnControlRemoved (ControlEventArgs e)
    		{
    			if (ControlRemoved != null)
    				ControlRemoved (this, e);
    		}
    		
    		protected virtual void OnCreateControl ()
    		{
    			
    		}
    		
    		protected virtual void OnCursorChanged (EventArgs e)
    		{
    			if (CursorChanged != null)
    				CursorChanged (this, e);
    		}
    		
    		protected virtual void OnDockChanged (EventArgs e)
    		{
    			if (DockChanged != null)
    				DockChanged (this, e);
    		}
    		
    		protected virtual void OnDoubleClick (EventArgs e)
    		{
    			if (DoubleClick != null)
    				DoubleClick (this, e);
    		}
    		
    		protected virtual void OnDragDrop (DragEventArgs e)
    		{
    			if (DragDrop != null)
    				DragDrop (this, e);
    		}
    		
    		protected virtual void OnDragEnter (DragEventArgs e)
    		{
    			if (DragEnter != null)
    				DragEnter (this, e);
    		}
    		
    		protected virtual void OnDragLeave (EventArgs e)
    		{
    			if (DragLeave != null)
    				DragLeave (this, e);
    		}
    		
    		protected virtual void OnDragOver (DragEventArgs e)
    		{
    			if (DragOver != null)
    				DragOver (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnEnabledChanged (EventArgs e)
    		{
    			if (EnabledChanged != null)
    				EnabledChanged (this, e);
    		}
    		
    		protected virtual void OnEnter (EventArgs e)
    		{
    			if (Enter != null)
    				Enter (this, e);
    		}
    		
    		protected virtual void OnFontChanged (EventArgs e)
    		{
    			if (FontChanged != null)
    				FontChanged (this, e);
    		}
    		
    		protected virtual void OnForeColorChanged (EventArgs e) 
    		{
    			if (ForeColorChanged != null)
    				ForeColorChanged (this, e);
    		}
    		
    		protected virtual void OnGiveFeedback (GiveFeedbackEventArgs e)
    		{
    			if (GiveFeedback != null)
    				GiveFeedback (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnGotFocus (EventArgs e) 
    		{
    			if (GotFocus != null)
    				GotFocus (this, e);
    		}
    		
    		protected virtual void OnHandleCreated (EventArgs e) 
    		{
    			Console.WriteLine ("OnHandleCreated");
    
    			if (HandleCreated != null)
    				HandleCreated (this, e);
    
    			// create all child windows
    			IEnumerator cw = childControls.GetEnumerator();
    
    			while (cw.MoveNext()) {
    				Console.WriteLine ("Adding Control");
    				Control control = (Control) cw.Current;
    				control.CreateControl ();
    				control.Show ();
    			}
    		}
    		
    		protected virtual void OnHandleDestroyed (EventArgs e) 
    		{
    			if (HandleDestroyed != null)
    				HandleDestroyed (this, e);
    		}
    		
    		protected virtual void OnHelpRequested (HelpEventArgs e) 
    		{
    			if (HelpRequested != null)
    				HelpRequested (this, e);
    		}
    		
    		protected virtual void OnImeModeChanged (EventArgs e) 
    		{
    			if (ImeModeChanged != null)
    				ImeModeChanged (this, e);
    		}
    		
    		protected virtual void OnInvalidated (InvalidateEventArgs e) 
    		{
    			if (Invalidated != null)
    				Invalidated (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnKeyDown (KeyEventArgs e) 
    		{
    			if (KeyDown != null)
    				KeyDown (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnKeyPress (KeyPressEventArgs e) 
    		{
    			if (KeyPress != null)
    				KeyPress (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnKeyUp (KeyEventArgs e) 
    		{
    			if (KeyUp != null)
    				KeyUp (this, e);
    
    		}
    		
    		protected virtual void OnLayout (LayoutEventArgs e) 
    		{
    			if (Layout != null)
    				Layout (this, e);
    		}
    		
    		protected virtual void OnLeave (EventArgs e) 
    		{
    			if (Leave != null)
    				Leave (this, e);
    		}
    		
    		protected virtual void OnLocationChanged (EventArgs e) 
    		{
    			if (LocationChanged != null)
    				LocationChanged (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnLostFocus (EventArgs e) 
    		{
    			if (LostFocus != null)
    				LostFocus (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnMouseDown (MouseEventArgs e) 
    		{
    			if (MouseDown != null)
    				MouseDown (this, e);
    		}
    		
    		protected virtual void OnMouseEnter (EventArgs e) 
    		{
    			if (MouseEnter != null)
    				MouseEnter (this, e);
    		}
    
    		protected virtual void OnMouseHover (EventArgs e) 
    		{
    			if (MouseHover != null)
    				MouseHover (this, e);
    		}
    		
    		protected virtual void OnMouseLeave (EventArgs e) 
    		{
    			if (MouseLeave != null)
    				MouseLeave (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnMouseMove (MouseEventArgs e) 
    		{
    			if (MouseMove != null)
    				MouseMove (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnMouseUp (MouseEventArgs e) 
    		{
    			if (MouseUp != null)
    				MouseUp (this, e);
    		}
    		
    		protected virtual void OnMouseWheel (MouseEventArgs e) 
    		{
    			if (MouseWheel != null)
    				MouseWheel (this, e);
    		}
    		
    		protected virtual void OnMove (EventArgs e) 
    		{
    			if (Move != null)
    				Move (this, e);
    		}
    		
    		protected virtual void OnNotifyMessage (Message m) 
    		{
    
    		}
    		
 			//Compact Framework
    		protected virtual void OnPaint (PaintEventArgs e) 
    		{
    			if (Paint != null)
    				Paint (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnPaintBackground (PaintEventArgs e) 
    		{
    
    		}
    		
    		protected virtual void OnParentBackColorChanged (EventArgs e) 
    		{
    			if (BackColorChanged != null)
    				BackColorChanged (this, e);
    		}
    		
    		protected virtual void OnParentBackgroundImageChanged (
    			EventArgs e) 
    		{
    			if (BackgroundImageChanged != null)
    				BackgroundImageChanged (this, e);
    		}
    		
    		protected virtual void OnParentBindingContextChanged (
    			EventArgs e) 
    		{
    			if (BindingContextChanged != null)
    				BindingContextChanged (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnParentChanged (EventArgs e) 
    		{
    			if (ParentChanged != null)
    				ParentChanged (this, e);
    		}
    		
    		protected virtual void OnParentEnabledChanged (EventArgs e) 
    		{
    			if (EnabledChanged != null)
    				EnabledChanged (this, e);
    		}
    		
    		protected virtual void OnParentFontChanged (EventArgs e) 
    		{
    			if (FontChanged != null)
    				FontChanged (this, e);
    		}
    		
    		protected virtual void OnParentForeColorChanged (EventArgs e) 
    		{
    			if (ForeColorChanged != null)
    				ForeColorChanged (this, e);
    		}
    		
    		protected virtual void OnParentRightToLeftChanged (
    			EventArgs e) 
    		{
    			if (RightToLeftChanged != null)
    				RightToLeftChanged (this, e);
    		}
    		
    		protected virtual void OnParentVisibleChanged (EventArgs e) 
    		{
    			if (VisibleChanged != null)
    				VisibleChanged (this, e);
    		}
    		
    		protected virtual void OnQueryContinueDrag (
    			QueryContinueDragEventArgs e) 
    		{
    			if (QueryContinueDrag != null)
    				QueryContinueDrag (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnResize (EventArgs e) 
    		{
    			if (Resize != null)
    				Resize (this, e);
    		}
    		
    		protected virtual void OnRightToLeftChanged (EventArgs e) 
    		{
    			if (RightToLeftChanged != null)
    				RightToLeftChanged (this, e);
    		}
    		
    		protected virtual void OnSizeChanged (EventArgs e) 
    		{
    			if (SizeChanged != null)
    				SizeChanged (this, e);
    		}
    		
    		protected virtual void OnStyleChanged (EventArgs e) 
    		{
    			if (StyleChanged != null)
    				StyleChanged (this, e);
    		}
    		
    		protected virtual void OnSystemColorsChanged (EventArgs e) 
    		{
    			if (SystemColorsChanged != null)
    				SystemColorsChanged (this, e);
    		}
    		
    		protected virtual void OnTabIndexChanged (EventArgs e) 
    		{
    			if (TabIndexChanged != null)
    				TabIndexChanged (this, e);
    		}
    		
    		protected virtual void OnTabStopChanged (EventArgs e) 
    		{
    			if (TabStopChanged != null)
    				TabStopChanged (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnTextChanged (EventArgs e) 
    		{
    			if (TextChanged != null)
    				TextChanged (this, e);
    		}
    
    		//[MonoTODO] // this doesn't seem to be documented
    // 		protected virtual void OnTextAlignChanged (EventArgs e) {
    // 			TextAlignChanged (this, e);
    // 		}
    		
    		protected virtual void OnValidated (EventArgs e) 
    		{
    			if (Validated != null)
    				Validated (this, e);
    		}
    		
    		//[MonoTODO]
    		// CancelEventArgs not ready
    		//protected virtual void OnValidating(CancelEventArgs e) 
    		//{
    		//	throw new NotImplementedException ();
    		//}
    		
    		[MonoTODO]
    		protected virtual void OnVisibleChanged (EventArgs e) 
    		{
    			if (VisibleChanged != null)
    				VisibleChanged (this, e);
    		}
    		// --- end of methods for events ---
    		
    		
    		[MonoTODO]
    		public void PerformLayout () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void PerformLayout (Control affectedControl,
    					   string affectedProperty) 
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public Point PointToClient (Point p) 
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public Point PointToScreen (Point p) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public virtual bool PreProcessMessage (ref Message msg) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessCmdKey (ref Message msg,
    						      Keys keyData) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessDialogChar (char charCode) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessDialogKey (Keys keyData) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessKeyEventArgs (ref Message m) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected internal virtual bool ProcessKeyMessage (
    			ref Message m) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessKeyPreview (ref Message m) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessMnemonic (char charCode) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		// used when properties/values of Control 
    		// are big enough to warrant recreating the HWND
    		protected void RecreateHandle() 
    		{
    			CreateHandle ();
    
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public Rectangle RectangleToClient (Rectangle r) 
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public Rectangle RectangleToScreen (Rectangle r) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected static bool ReflectMessage (IntPtr hWnd,
    						      ref Message m) 
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		public virtual void Refresh () 
    		{
    			//RECT rect = (RECT) null;
    			//InvalidateRect (Handle, ref rect, true);
    			Win32.UpdateWindow (Handle);
    		}
    		
    		[MonoTODO]
    		public virtual void ResetBackColor () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void ResetBindings () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public virtual void ResetFont () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public virtual void ResetForeColor () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void ResetImeMode () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public virtual void ResetRightToLeft () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public virtual void ResetText () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void ResumeLayout () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void ResumeLayout (bool performLayout) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected ContentAlignment RtlTranslateAlignment (
    			ContentAlignment align) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected HorizontalAlignment RtlTranslateAlignment (
    			HorizontalAlignment align) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected LeftRightAlignment RtlTranslateAlignment (
    			LeftRightAlignment align) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected ContentAlignment RtlTranslateContent (
    			ContentAlignment align) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected HorizontalAlignment RtlTranslateHorizontal (
    			HorizontalAlignment align) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected LeftRightAlignment RtlTranslateLeftRight (
    			LeftRightAlignment align) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void Scale (float ratio) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void Scale (float dx,float dy) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual void ScaleCore (float dx, float dy) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void Select () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual void Select (bool directed,bool forward) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public bool SelectNextControl (Control ctl, bool forward, 
    					       bool tabStopOnly, 
    					       bool nested, bool wrap)
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public void SendToBack () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void SetBounds (int x, int y, int width, int height) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public void SetBounds (int x, int y, int width, int height,
    				       BoundsSpecified specified) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual void SetBoundsCore (
    			int x, int y, int width, int height,
    			BoundsSpecified specified) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected virtual void SetClientSizeCore (int x, int y)
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected void SetStyle (ControlStyles flag, bool value) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		protected void SetTopLevel (bool value)
    		{
    			if (value)
    				// FIXME: verify on whether this is supposed
    				// to activate/deactive the window
    				Win32.SetWindowPos (Handle, 
						SetWindowPosZOrder.HWND_NOTOPMOST,
						0, 0, 0, 0, 0);
    			else
    				// FIXME: this does not make sense but
    				// the docs say the window is hidden
    				Win32.ShowWindow (Handle, ShowWindowStyles.SW_HIDE);
    		}
    		
    		[MonoTODO]
    		protected virtual void SetVisibleCore (bool value)
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		public void Show () 
    		{
    			Win32.ShowWindow (Handle, ShowWindowStyles.SW_SHOW);
    		}
    		
    		[MonoTODO]
    		public void SuspendLayout () 
    		{
    			throw new NotImplementedException ();
    		}
    		
 			//Compact Framework
    		public void Update () 
    		{
    			Win32.UpdateWindow (Handle);
    		}
    		
    		[MonoTODO]
    		protected void UpdateBounds () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected void UpdateBounds (int x, int y, int width, int height) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected void UpdateBounds (
    			int x, int y, int width, int height, int clientWidth,
    			int clientHeight)
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected void UpdateStyles () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		protected void UpdateZOrder () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		// WndProc - calls appriate On... function for the give
    		// message
    		//
    		// These On... functions do not appear to be called by
    		// WndProc:
    		//
    		// background color/image handled by WinForms
    		// OnBackColorChanged
    		// OnBackgroundImageChanged
    		// OnForeColorChanged
    		// OnPaintBackground
    		//
    		// controls are added/removed by WinForms
    		// OnControlAdded
    		// OnControlRemoved
    		// OnCreateControl
    		//
    		// OnBindingContextChanged
    		// OnCausesValidationChanged
    		// OnChangeUICues
    		// OnContextMenuChanged
    		// OnRightToLeftChanged
    		// OnGiveFeedback
    		// OnLayout
    		// OnDockChanged
    		// OnCursorChanged
    		// OnTextAlignChanged
    		// OnValidated
    		// OnValidating
    		// OnTabIndexChanged
    		// OnTabStopChanged
    		// OnLocationChanged
    		//
    		// FIXME: may be one of the WM_IME_ messages
    		// OnImeModeChanged 
    		//
    		// InvalidateRect is called by no Invalidate message exists
    		// OnInvalidated
    		//
    		// these messages ARE not called by WNDPROC according to docs
    		// OnParentBackColorChanged 
    		// OnParentBackgroundImageChanged
    		// OnParentBindingContextChanged
    		// OnParentChanged
    		// OnParentEnabledChanged
    		// OnParentFontChanged
    		// OnParentForeColorChanged
    		// OnParentRightToLeftChanged
    		// OnParentVisibleChanged
    		//
    		protected virtual void WndProc(ref Message m) 
    		{
    			EventArgs eventArgs = new EventArgs ();
    			// FIXME: paintEventArgs is not being created properly
				// FIXME: Graphics does not have a public constructor, you must get one from .NET
    			//PaintEventArgs paintEventArgs = new PaintEventArgs (
    			//	new Graphics(), new Rectangle());
    
    			switch (m.Msg) {
    
    			case Msg.WM_CREATE:
    				Console.WriteLine ("WM_CREATE");
    				OnHandleCreated (eventArgs);
    				break;
    			case Msg.WM_LBUTTONDBLCLK:
    				OnDoubleClick (eventArgs);
    				break;
    				// OnDragDrop
    				// OnDragEnter
    				// OnDragLeave
    				// OnDragOver
    				// OnQueryContinueDrag
    			case Msg.WM_ENABLE:
    				OnEnabledChanged (eventArgs);
    				break;
    			case Msg.WM_SETFOCUS:
    				OnEnter (eventArgs);
    				OnGotFocus (eventArgs);
    				break;
    			case Msg.WM_FONTCHANGE:
    				OnFontChanged (eventArgs);
    				break;
    			case Msg.WM_DESTROY:
    				OnHandleDestroyed (eventArgs);
    				break;
    			case Msg.WM_HELP:
    				// FIXME:
    				//OnHelpRequested (eventArgs);
    				break;
    			case Msg.WM_KEYDOWN:
    				// FIXME:
    				// OnKeyDown (eventArgs);
    				break;
    			case Msg.WM_CHAR:
    				// FIXME:
    				// OnKeyPress (eventArgs);
    				break;
    			case Msg.WM_KEYUP:
    				// FIXME:
    				// OnKeyUp (eventArgs);
    				break;
    			case Msg.WM_KILLFOCUS:
    				OnLeave (eventArgs);
    				OnLostFocus (eventArgs);
    				break;
    			case Msg.WM_LBUTTONDOWN:
    				// FIXME:
    				// OnMouseDown (eventArgs);
    				break;
    			case Msg.WM_MOUSEACTIVATE:
    				OnMouseEnter (eventArgs);
    				break;
    			case Msg.WM_MOUSEHOVER: // called by TrackMouseEvent
    				OnMouseHover (eventArgs);
    				break;
    			case Msg.WM_MOUSELEAVE: // called by TrackMouseEvent
    				OnMouseLeave (eventArgs);
    				break;
    			case Msg.WM_MOUSEMOVE:
    				// FIXME:
    				// OnMouseMove (eventArgs);
    				break;
    			case Msg.WM_LBUTTONUP:
    				// FIXME:
    				// OnMouseUp (eventArgs);
    				break;
    			case Msg.WM_MOUSEWHEEL:
    				// FIXME:
    				// OnMouseWheel (eventArgs);
    				break;
    			case Msg.WM_MOVE:
    				OnMove (eventArgs);
    				break;
    			case Msg.WM_NOTIFY:
    				// FIXME: get NM_CLICKED msg from pnmh
    				// OnClick (eventArgs);
    				// OnNotifyMessage (eventArgs);
    			case Msg.WM_PAINT:
    				//OnPaint (paintEventArgs);
    				break;
    			case Msg.WM_SIZE:
    				OnResize (eventArgs);
    				OnSizeChanged (eventArgs);
    				break;
    			case Msg.WM_STYLECHANGED:
    				OnStyleChanged (eventArgs);
    				break;
    			case Msg.WM_SYSCOLORCHANGE:
    				OnSystemColorsChanged (eventArgs);
    				break;
    			case Msg.WM_SETTEXT:
    				OnTextChanged (eventArgs);
    				break;
    			case Msg.WM_SHOWWINDOW:
    				OnVisibleChanged (eventArgs);
    				break;
    // 			default:
    // 				DefWndProc (ref m);
    // 				break;
    			}
    		}
    
    		/// --- Control: events ---
    		public event EventHandler BackColorChanged;
    		public event EventHandler BackgroundImageChanged;
    		public event EventHandler BindingContextChanged;
    		public event EventHandler CausesValidationChanged;
    		public event UICuesEventHandler ChangeUICues;
    		
 			//Compact Framework
    		public event EventHandler Click;
     		
    		public event EventHandler ContextMenuChanged;
    		public event ControlEventHandler ControlAdded;
    		public event ControlEventHandler ControlRemoved;
    		public event EventHandler CursorChanged;
    		public event EventHandler DockChanged;
    		public event EventHandler DoubleClick;
    		public event DragEventHandler DragDrop;
    		public event DragEventHandler DragEnter;
    		public event EventHandler DragLeave;
    		public event DragEventHandler DragOver;

			//Compact Framework
    		public event EventHandler EnabledChanged;
    		
    		public event EventHandler Enter;
    		public event EventHandler FontChanged;
    		public event EventHandler ForeColorChanged;
    		public event GiveFeedbackEventHandler GiveFeedback;
   		
 			//Compact Framework
    		public event EventHandler GotFocus;
    		
    		public event EventHandler HandleCreated;
    		public event EventHandler HandleDestroyed;
    		public event HelpEventHandler HelpRequested;
    		public event EventHandler ImeModeChanged;
    		public event InvalidateEventHandler Invalidated;
    		
 			//Compact Framework
    		public event KeyEventHandler KeyDown;
    		
 			//Compact Framework
    		public event KeyPressEventHandler KeyPress;
    		
 			//Compact Framework
    		public event KeyEventHandler KeyUp;
    		
    		public event LayoutEventHandler Layout;
    		public event EventHandler Leave;
    		public event EventHandler LocationChanged;
    		
 			//Compact Framework
    		public event EventHandler LostFocus;

			//Compact Framework
    		public event MouseEventHandler MouseDown;
    		
    		public event EventHandler MouseEnter;
    		public event EventHandler MouseHover;
    		public event EventHandler MouseLeave;
    		
 			//Compact Framework
    		public event MouseEventHandler MouseMove;
    		
 			//Compact Framework
    		public event MouseEventHandler MouseUp;
    		
    		public event MouseEventHandler MouseWheel;
    		public event EventHandler Move;
    		
 			//Compact Framework
    		public event PaintEventHandler Paint;
    		
 			//Compact Framework
    		public event EventHandler ParentChanged;
    		
    		public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp;
    		public event QueryContinueDragEventHandler QueryContinueDrag;
    		
 			//Compact Framework
    		public event EventHandler Resize;
    		
    		public event EventHandler RightToLeftChanged;
    		public event EventHandler SizeChanged;
    		public event EventHandler StyleChanged;
    		public event EventHandler SystemColorsChanged;
    		public event EventHandler TabIndexChanged;
    		public event EventHandler TabStopChanged;
    		
 			//Compact Framework
    		public event EventHandler TextChanged;
    		
    		public event EventHandler Validated;
    		//[MonoTODO]
    		// CancelEventHandler not yet defined
    		//public event CancelEventHandler Validating {
    		
    		public event EventHandler VisibleChanged;
    		
    		/// --- IWin32Window properties
    		public IntPtr Handle {
    			get { 
    				if (window != null) 
    					return window.Handle; 
    				return (IntPtr) 0;
    			}
    		}
    		
    		/// --- ISynchronizeInvoke properties ---
    		[MonoTODO]
    		public bool InvokeRequired {
    			get { throw new NotImplementedException (); }
    		}
    		
    		/// --- ISynchronizeInvoke methods ---
    		[MonoTODO]
    		public IAsyncResult BeginInvoke (Delegate method) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public IAsyncResult BeginInvoke (Delegate method,
    						 object[] args) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		[MonoTODO]
    		public object EndInvoke (IAsyncResult asyncResult) 
    		{
    			throw new NotImplementedException ();
    		}
    		
  		//Compact Framework
    		[MonoTODO]
    		public object Invoke (Delegate method) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		//[MonoTODO]
    		public object Invoke (Delegate method, object[] args) 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		/// sub-class: Control.ControlAccessibleObject
    		/// <summary>
    		/// Provides information about a control that can be used by an accessibility application.
    		/// </summary>
    		public class ControlAccessibleObject : AccessibleObject {
    			// AccessibleObject not ready to be base class
    			/// --- ControlAccessibleObject.constructor ---
    			[MonoTODO]
    			public ControlAccessibleObject (Control ownerControl) 
    			{
    				throw new NotImplementedException ();
    			}
    			
    			
    			/// --- ControlAccessibleObject Properties ---
    			[MonoTODO]
     			public override string DefaultAction {
     				get { throw new NotImplementedException (); }
     			}
    			
    			[MonoTODO]
    			public override string Description {
     				get { throw new NotImplementedException (); }
     			}
    			
    			[MonoTODO]
    			public IntPtr Handle {
    				get { throw new NotImplementedException (); }
    				set { throw new NotImplementedException (); }
    			}
    			
    			[MonoTODO]
     			public override string Help {
     				get { throw new NotImplementedException (); }
     			}
    			
    			[MonoTODO]
     			public override string KeyboardShortcut {
     				get { throw new NotImplementedException (); }
     			}
    			
    			[MonoTODO]
     			public override string Name {
     				get { throw new NotImplementedException (); }
     				set { throw new NotImplementedException (); }
     			}
    			
    			[MonoTODO]
    			public Control Owner {
    				get { throw new NotImplementedException (); }
    			}
    			
    			[MonoTODO]
     			public override AccessibleRole Role {
     				get { throw new NotImplementedException (); }
     			}
    			
    			
    			/// --- ControlAccessibleObject Methods ---
    			[MonoTODO]
     			public override int GetHelpTopic(out string fileName) 
     			{
     				throw new NotImplementedException ();
     			}
    			
    			[MonoTODO]
    			public void NotifyClients (AccessibleEvents accEvent) 
    			{
    				throw new NotImplementedException ();
    			}
    			
    			[MonoTODO]
    			public void NotifyClients (AccessibleEvents accEvent,
    						   int childID) 
    			{
    				throw new NotImplementedException ();
    			}
    			
    			[MonoTODO]
    			public override string ToString ()
    			{
    				throw new NotImplementedException ();
    			}
    		}
    		
    		/// sub-class: Control.ControlCollection
    		/// <summary>
    		/// Represents a collection of Control objects
    		/// </summary>
    		public class ControlCollection : IList, ICollection, IEnumerable, ICloneable {
    
    			private ArrayList collection = new ArrayList ();
    			private Control owner;
    
    			/// --- ControlCollection.constructor ---
    			public ControlCollection (Control owner) 
    			{
    				this.owner = owner;
    			}
    		
    			/// --- ControlCollection Properties ---
    			public int Count {
    				get { return collection.Count; }
    			}
    		
    			public bool IsReadOnly {
    				get { return collection.IsReadOnly; }
    			}
    			
    			public virtual Control this [int index] {
    				get { return (Control) collection[index]; }
    			}
    		
    			public virtual void Add (Control value) 
    			{
    				collection.Add (value);
    			}
    			
    			public virtual void AddRange (Control[] controls) 
    			{
    				collection.AddRange (controls);
    			}
    			
    			public virtual void Clear () 
    			{
    				collection.Clear ();
    			}
    		
    			public bool Contains (Control control) 
    			{
    				return collection.Contains (control);
    			}
    			
    			public void CopyTo (Array dest,int index) 
    			{
    				collection.CopyTo (dest, index);
    			}
    			
    			[MonoTODO]
    			public override bool Equals (object other) 
    			{
    				throw new NotImplementedException ();
    			}
    			
    			//inherited
    			//public static bool Equals(object o1, object o2) {
    			//	throw new NotImplementedException ();
    			//}
    
    			[MonoTODO]
    			public int GetChildIndex (Control child)
    			{
    				throw new NotImplementedException ();
    			}
    			
    			public IEnumerator GetEnumerator () 
    			{
    				return collection.GetEnumerator ();
    			}
    			
    			[MonoTODO]
    			public override int GetHashCode () 
    			{
    				throw new NotImplementedException ();
    			}
    			
    			public int IndexOf (Control control) 
    			{
    				return collection.IndexOf (control);
    			}
    			
    			public virtual void Remove (Control value) 
    			{
    				collection.Remove (value);
    			}
    			
    			public void RemoveAt (int index) 
    			{
    				collection.RemoveAt (index);
    			}
    			
    			[MonoTODO]
    			public void SetChildIndex (Control child,int newIndex) 
    			{
    				throw new NotImplementedException ();
    			}
    			
    			/// --- ControlCollection.IClonable methods ---
    			[MonoTODO]
    			object ICloneable.Clone ()
    			{
    				throw new NotImplementedException ();
    			}
    			
    			/// --- ControlCollection.IList properties ---
    			bool IList.IsFixedSize {
    				get { return collection.IsFixedSize; }
    			}
    
    			object IList.this [int index] {
    				get { return collection[index]; }
    				set { collection[index] = value; }
    			}
    
    			object ICollection.SyncRoot {
    				get { return collection.SyncRoot; }
    			}
    	
    			bool ICollection.IsSynchronized {
    				get { return collection.IsSynchronized; }
    			}
    			
    			/// --- ControlCollection.IList methods ---
    			int IList.Add (object control) 
    			{
    				return collection.Add (control);
    			}
    		
    			bool IList.Contains (object control) 
    			{
    				return collection.Contains (control);
    			}
    		
    			int IList.IndexOf (object control) 
    			{
    				return collection.IndexOf (control);
    			}
    		
    			void IList.Insert (int index,object value) 
    			{
    				collection.Insert (index, value);
    			}
    		
    			void IList.Remove (object control) 
    			{
    				collection.Remove (control);
    			}
    		}  // --- end of Control.ControlCollection ---
    	}
    }
