    //
    // System.Windows.Forms.Control.cs
    //
    // Author:
    //   stubbed out by Jaak Simm (jaaksimm@firm.ee)
    //	Dennis Hayes (dennish@rayetk.com)
    //   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
    //	 Alexandre Pigolkine (pigolkine@gmx.de)
    //	Aleksey Ryabchuk (ryabchuk@yahoo.com)
    //
    // (C) Ximian, Inc., 2002
    //
    
    using System.ComponentModel;
    using System.Drawing;
    using System.Collections;
	using System.Threading;
	using System.Text;
    using System.Runtime.InteropServices;
    using System.Collections.Specialized;

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
					//Console.WriteLine ("Control WndProc Message HWnd {0}, Msg {1}", m.HWnd, m.Msg);
					// Do not call default WndProc here
					// let the control decide what to do
    				// base.WndProc (ref m);
       				control.WndProc (ref m);
    			}
    		}
    		
    		// FIXME: not sure if dervied classes should have access
    		protected ControlNativeWindow window;
    		private ControlCollection childControls;
    		private Control parent;
    		static private Hashtable controlsCollection = new Hashtable ();
		static private NativeWindow parkingWindow = null;
		private static bool classRegistered = false;

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
    		protected Color backColor;
    		Image backgroundImage;
    		//BindingContext bindingContext;
    		Rectangle bounds;
		Rectangle oldBounds;

    		bool causesValidation;
    		//ContextMenu contextMenu;
    		DockStyle dock;
    		bool enabled;
    		Font font;
    		protected Color foreColor;
    		ImeMode imeMode;
    		bool isAccessible;
    		// Point location;  // using bounds to store location
    		string name;
    		Region region;
    		RightToLeft rightToLeft;
    		bool tabStop;
    		protected string text;
    		protected bool visible;
    		protected ControlStyles controlStyles_;
		Cursor  cursor;

		int clientWidth;
		int clientHeight;

		BitVector32 statuses;
		private static readonly int LAYOUT_SUSPENDED = BitVector32.CreateMask();
		private static readonly int LAYOUT_PENDING   = BitVector32.CreateMask( LAYOUT_SUSPENDED );
		private static readonly int DISPOSED	     = BitVector32.CreateMask( LAYOUT_PENDING );
		private static readonly int RECREATING_HANDLE= BitVector32.CreateMask( DISPOSED );
		private static readonly int CREATED          = BitVector32.CreateMask( RECREATING_HANDLE );

			object tag;
			protected bool mouseIsInside_;

			// BeginInvoke() etc. helpers
			static int InvokeMessage = Win32.RegisterWindowMessage("mono_control_invoke_helper");

			// CHECKME: This variable is used to determine whether current thread
			// was used to create Control Handle. It take some space but saves a call
			// to unmanaged code in ISynchronizeInvoke.IsInvokeRequired.
			private int CreatorThreadId_ = 0; 

			private Queue InvokeQueue_ = new Queue();

			internal class ControlInvokeHelper : IAsyncResult {
				private Delegate Method_ = null;
				private object[] MethodArgs_ = null;
				private object MethodResult_ = null;
				private ManualResetEvent AsyncWaitHandle_ = new ManualResetEvent(false);
				private bool CompletedSynchronously_ = false;
				private bool IsCompleted_ = false;

				public ControlInvokeHelper( Delegate method, object[] args) {
					Method_ = method;
					MethodArgs_ = args;
				}

				// IAsyncResult interface 
				object IAsyncResult.AsyncState { 
					get {
						if( MethodArgs_ != null && MethodArgs_.Length != 0) {
							return MethodArgs_[MethodArgs_.Length - 1];
						}
						return null;
					}
				}

				WaitHandle IAsyncResult.AsyncWaitHandle { 
					get {
						return AsyncWaitHandle_;
					}
				}

				bool IAsyncResult.CompletedSynchronously {
					get {
						return CompletedSynchronously_;
					}
				}

				bool IAsyncResult.IsCompleted { 
					get {
						return IsCompleted_;
					}
				}

				internal bool CompletedSynchronously {
					set {
						CompletedSynchronously_ = value;
					}
				}

				internal object MethodResult {
					get {
						return MethodResult_;
					}
				}

				internal void ExecuteMethod() {
					object result = Method_.DynamicInvoke(MethodArgs_);
					lock(this) {
						MethodResult_ = result;
						IsCompleted_ = true;
					}
					AsyncWaitHandle_.Set();
				}
			}

    		// --- Constructors ---
    
	 		//Compact Framework //only Control()
    		public Control ()
    		{
    			childControls = CreateControlsInstance ();

			statuses = new BitVector32(); 
			
    			accessibleDefaultActionDescription = null;
    			accessibleDescription = null;
    			accessibleName = null;
    			accessibleRole = AccessibleRole.Default;
    			allowDrop = false;
    			anchor = AnchorStyles.Top | AnchorStyles.Left;
    			backColor = Control.DefaultBackColor;
    			backgroundImage = null;
    			bounds = new Rectangle();
			oldBounds = new Rectangle();
    			// bindingContext = null;
    			causesValidation = true;
    			// contextMenu = null;
    			dock = DockStyle.None;
    			enabled = true;
    			// font = Control.DefaultFont;
    			foreColor = Control.DefaultForeColor;
    			imeMode = ImeMode.Inherit;
    			isAccessible = false;
    			// location = new Point (0,0); should be from OS
    			name = "";
    			region = null;
    			rightToLeft = RightToLeft.Inherit;
    			tabStop = true;
    			text = "";
    			visible = true;
    			parent = null;
			cursor = null;

			oldBounds.Width  = bounds.Width = DefaultSize.Width;
			oldBounds.Height = bounds.Height= DefaultSize.Height;
			clientWidth	 = DefaultSize.Width;
			clientHeight	 = DefaultSize.Height;

			mouseIsInside_ = false;
    			controlStyles_ = ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick;
				// Do not create Handle here, only in CreateHandle
    			// CreateHandle();//sets window handle. FIXME: No it does not
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
				if ( anchor != value ) {
    					anchor = value;
					if ( anchor != ( AnchorStyles.Left | AnchorStyles.Top ) )
						Dock = DockStyle.None;
				}
    			}
    		}
    		
	  		//Compact Framework
    		public virtual Color BackColor {
    			get {
					return backColor;
    			}
    			set {
					if( backColor != value) {
						backColor = value;
						OnBackColorChanged(new EventArgs());
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
					Invalidate();
    			}
    		}
    
    		public virtual BindingContext BindingContext {
    			get {
    				//return bindingContext;
    				throw new NotImplementedException ();
    			}
    			set {
    				//bindingContext=value;
    				throw new NotImplementedException ();
    			}
    		}
    		
	 		//Compact Framework
    		public int Bottom {
    			get {
    				return Top + Height;
    			}
    		}
    		
 			//Compact Framework
    		public Rectangle Bounds {
    			get {
    				return bounds;
    			}
    			set {
				SetBounds(value.Left, value.Top, value.Width, value.Height);
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
				return new Rectangle ( 0, 0, ClientSize.Width, ClientSize.Height );
 
    			}
    		}
    		
	 		//Compact Framework
			[MonoTODO]
			public Size ClientSize {
				get {
					return new Size ( clientWidth, clientHeight);
				}
				set {
					SetClientSizeCore ( value.Width, value.Height );
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
						if (Focused) return true;
						foreach (Control ctr in Controls) {
							if (ctr.ContainsFocus) return true;
						}
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
    			get { return statuses[ CREATED ]; }
    		}
    		
    		protected virtual CreateParams CreateParams {
    			get {
  					CreateParams createParams = new CreateParams ();
  					createParams.Caption = Text;
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
  						createParams.Parent = ParkingWindowHandle;
	  
  					createParams.Style = (int) WindowStyles.WS_OVERLAPPED;
					if( visible) {
						createParams.Style |= (int) WindowStyles.WS_VISIBLE;
					}
	  
    				return createParams;
    			}
    		}
    		
			internal protected IntPtr ControlRealWndProc = IntPtr.Zero;
			internal protected bool SubClassWndProc_ = false;

			// This function lets Windows or/and default Windows control process message
			// Classes have to call it if they do not handle message in WndProc or
			// default handling is needed.
			protected void CallControlWndProc( ref Message msg) {
				if( ControlRealWndProc != IntPtr.Zero) {
					msg.Result = (IntPtr)Win32.CallWindowProc(ControlRealWndProc, msg.HWnd, (int)msg.Msg, msg.WParam.ToInt32(), msg.LParam.ToInt32());
				}
				else {
					DefWndProc(ref msg);
				}
			}

			// Subclass only native Windows controls. Those classes have to set SubClassWndProc_ to true in contructor
			private void SubclassWindow() {
				if( IsHandleCreated && SubClassWndProc_) {
					ControlRealWndProc = Win32.SetWindowLong( Handle, GetWindowLongFlag.GWL_WNDPROC, NativeWindow.GetWindowProc());
				}
			}

			private void UnsubclassWindow() {
				if( IsHandleCreated) {
					Win32.SetWindowLong( Handle, GetWindowLongFlag.GWL_WNDPROC, ControlRealWndProc.ToInt32());
				}
			}

			protected virtual void OnWmCommand (ref Message m) {
				if( m.LParam.ToInt32() != 0) {
					if( m.LParam != Handle) {
						// Control notification
						System.Console.WriteLine("Control notification Code {0} Id = Hwnd {1}", m.HiWordWParam, m.LParam.ToInt32());
						Control.ReflectMessage(m.LParam, ref m);
					}
					else {
						// Unhandled Control reflection
						// Derived class didn't handle WM_COMMAND or called base.WndProc in WM_COMMAND handler
						// CHECKME: Shall we notify user in debug build, throw an exception or just ignore this case ?
					}
				}
			}

    		[MonoTODO]
    		public virtual Cursor Cursor {
    			get { 
				if ( cursor == null )
					return Cursors.Default;
				return cursor; 
			}
    			set { cursor = value;}
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
    				return SystemColors.Control;
    				//throw new NotImplementedException ();
    			}
    		}
    
    		//[MonoTODO]
    		// FIXME: use GetSystemMetrics?
     		public static Font DefaultFont {
    			// FIXME: get current system font from GenericSansSerif
    			//        call ArgumentException not called
    			get {
    		//		throw new NotImplementedException ();
    		//		return (FontFamily.GenericSansSerif);
					return Font.FromHfont(Win32.GetStockObject(GSO_.DEFAULT_GUI_FONT));
    			}
    		}
    		
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
				if ( dock != value ) {
    					dock = value;
					if ( dock != DockStyle.None )
						 Anchor = ( AnchorStyles.Left | AnchorStyles.Top );
					OnDockChanged ( EventArgs.Empty );					
				}
    			}
    		}
    
	  		//Compact Framework
    		public virtual bool Enabled {
    			get {
					return enabled;
    				//return Win32.IsWindowEnabled (Handle);
    			}
    			set {
					if( enabled != value) {
						Win32.EnableWindow (Handle, value);
						enabled = value;
						// FIXME: Disable/enable all children here
						Invalidate();
					}
    			}
    		}
    		
  			//Compact Framework
    		public virtual bool Focused {
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
    		public virtual Font Font {
				get {
					Font result = font;
					if( result == null) {
						if( Parent != null) {
							result = Parent.Font;
						}
						if( result == null) {
							result = Control.DefaultFont;
						}
					}
					return result;
				}
    			set {
					font = value;
					if( IsHandleCreated) {
						Win32.SendMessage(Handle, Msg.WM_SETFONT, Font.ToHfont().ToInt32(), 1);
					}
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
    				return bounds.Height;
    			}
    			set {
				SetBounds(bounds.X, bounds.Y, bounds.Width, value, BoundsSpecified.Height);
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
    			get {	return statuses [ DISPOSED ]; }
    		}
    		
    		public bool IsHandleCreated {
    			get { return window != null && window.Handle != IntPtr.Zero; }
    		}
    		
  			//Compact Framework
			public int Left {
				get {
					return bounds.X;
				}
				set {
					SetBounds(value, bounds.Y, bounds.Width, bounds.Height, BoundsSpecified.X);
				}
			}
 		
 			//Compact Framework
			public Point Location {
				// CHECKME:
				get {
					return new Point (Top, Left);
				}
				set {
					SetBounds(value.X, value.Y, bounds.Width, bounds.Height, BoundsSpecified.Location);
				}
			}
    		
    		public static Keys ModifierKeys {
    			get {
				Keys keys = Keys.None;

				if ( ( Win32.GetKeyState( (int) VirtualKeys.VK_SHIFT ) & 0x8000 ) == 0x8000 )
					 keys |= Keys.Shift;
				if ( ( Win32.GetKeyState( (int) VirtualKeys.VK_MENU ) & 0x8000 )  == 0x8000 )
					keys |= Keys.Alt;
				if ( ( Win32.GetKeyState( (int) VirtualKeys.VK_CONTROL) & 0x8000) == 0x8000 )
					 keys |= Keys.Control;

				return keys;
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
					if( parent != value) {
						Console.WriteLine ("setting parent");
						parent = value;
	    
						Console.WriteLine ("add ourself to the parents control");
						// add ourself to the parents control
						if ( !parent.Controls.Contains ( this ) )
							parent.Controls.Add (this);

						// FIXME: Is this logic correct ?
						if( BackColor == DefaultBackColor) {
							if( parent.BackColor != BackColor)
								OnParentBackColorChanged(new EventArgs());
						}

						Console.WriteLine ("SetParent");
						if (IsHandleCreated) {
							Console.WriteLine ("Handle created");
							Win32.SetParent (Handle, value.Handle);
						}
						/*
						else if( parent.IsHandleCreated){
							// CHECKME: Now control is responsible for creating his window
							// when added to Form, may be things must be reversed.
							CreateControl();
						}
						*/			
					}
    			}
    		}
    		
		protected static IntPtr ParkingWindowHandle {
			get {
				if ( parkingWindow == null )
					parkingWindow = new NativeWindow ( );

				if ( parkingWindow.Handle == IntPtr.Zero ) {
					CreateParams pars = new CreateParams ( );
					pars.ClassName = "mono_native_window";
					pars.Style = (int) WindowStyles.WS_OVERLAPPED;
					parkingWindow.CreateHandle ( pars );
				}

				return parkingWindow.Handle;
			}
		}

		protected static void RegisterDefaultWindowClass ( )
		{
			if ( !classRegistered ) {
				WNDCLASS wndClass = new WNDCLASS();
 
				wndClass.style = (int) (CS_.CS_OWNDC);
				wndClass.lpfnWndProc = NativeWindow.GetWindowProc();
				wndClass.cbClsExtra = 0;
				wndClass.cbWndExtra = 0;
				wndClass.hInstance = (IntPtr)0;
				wndClass.hIcon = (IntPtr)0;
				wndClass.hCursor = Win32.LoadCursor( (IntPtr)0, LC_.IDC_ARROW);
				wndClass.hbrBackground = (IntPtr)((int)GetSysColorIndex.COLOR_BTNFACE + 1);
				wndClass.lpszMenuName = "";
				wndClass.lpszClassName = Win32.DEFAULT_WINDOW_CLASS;
    
				if (Win32.RegisterClass(ref wndClass) != 0) 
					classRegistered = true; 
			}		
		}

    		[MonoTODO]
    		public string ProductName {
    			get {
					//FIXME:
    				return "Product Name";
    			}
    		}
    		
    		[MonoTODO]
    		public string ProductVersion {
    			get {
					//FIXME:
					return "Product Version";
    			}
    		}
    		
    		[MonoTODO]
    		public bool RecreatingHandle {
    			get {
    				return statuses [ RECREATING_HANDLE ] ;
    			}
    		}
    		
    		public Region Region {
    			// CHECKME:
    			get {
    				return region;
    			}
    			set {
    				region = value;
    			}
    		}
    		
    		protected bool ResizeRedraw {
    			get {	return GetStyle ( ControlStyles.ResizeRedraw );	}
    			set {	SetStyle ( ControlStyles.ResizeRedraw, value ); }
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
    			get {
				return new Size(Width, Height);
    			}
    			set {
				SetBounds(bounds.X, bounds.Y, value.Width, value.Height, BoundsSpecified.Size);
			}
    		}

    		internal int tabindex;//for debug/test only. remove
    		[MonoTODO]
    		public int TabIndex {
    			get {
    				return tabindex;
    			}
    			set {
    				tabindex = value;
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
    				return tag;
    			}
    			set {
    				tag = value;
    			}
    		}
    		
 			//Compact Framework
    		public virtual string Text {
    			get {
					// CHECKME: if we really need to provide back current text of real window
					// or just our copy in text member.
    				if ( IsHandleCreated ) {
					int len = Win32.GetWindowTextLengthA (Handle);
					// FIXME: len is doubled due to some strange behaviour.(of GetWindowText function ?)
					// instead of 10 characters we can get only 9, even if sb.Capacity is 10.
					StringBuilder sb = new StringBuilder(len * 2 /*Win32.GetWindowTextLengthA (Handle)*/);
    					Win32.GetWindowText (Handle, sb, sb.Capacity);
    					text = sb.ToString();
    				} 
				return text;
    			}
    			set {
				if ( text != value ) {
    					text = value;
	    
    					if (IsHandleCreated)
    						Win32.SetWindowTextA (Handle, value);

					OnTextChanged ( EventArgs.Empty );
				}
    			}
    		}
    		
 			//Compact Framework
    		public int Top {
    			get {
				return bounds.Top;
 			}
 			set {
				SetBounds(bounds.X, value, bounds.Width, bounds.Height, BoundsSpecified.Y);
			}
 		}
 		
    		[MonoTODO]
    		public Control TopLevelControl {
    			get {
    				throw new NotImplementedException ();
    			}
    		}
    
    		public bool Visible {
    			get {	return visible;	}
    			set {
				SetVisibleCore ( value );
    			}
    		}
    		
 			//Compact Framework
    		public int Width {
    			get {
    				return bounds.Width;
    			}
    			set {
				SetBounds(bounds.X, bounds.Y, value, bounds.Height, BoundsSpecified.Width);
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
    		
    		[MonoTODO]
    		public void BringToFront () 
    		{
			if ( IsHandleCreated )
				Win32.SetWindowPos ( Handle, SetWindowPosZOrder.HWND_TOP, 0, 0, 0, 0, 
					SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE );
		}
    		
    		public bool Contains (Control ctl) 
    		{
    			return childControls.Contains (ctl);
    		}
    		
    		public void CreateControl () 
    		{
			if ( !Created )	{
    				CreateHandle ();
				OnCreateControl();
				statuses [ CREATED ] = true;
			}
		}
    
    		[MonoTODO]
    		protected virtual AccessibleObject CreateAccessibilityInstance() {
    			throw new NotImplementedException ();
    		}
    		
    		protected virtual ControlCollection CreateControlsInstance ()
    		{
    			return new ControlCollection (this);
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public Graphics CreateGraphics () 
    		{
				return Graphics.FromHwnd(Handle);
    		}
    	
    		protected virtual void CreateHandle ()
    		{
			if( IsDisposed )
				throw new ObjectDisposedException ( Name );

			if( IsHandleCreated ) 
				return;

			if( window == null)
				window = new ControlNativeWindow (this);

			CreateParams createParams = CreateParams;
			if( !Enabled) {
				createParams.Style |= (int)WindowStyles.WS_DISABLED;
			}
			window.CreateHandle (createParams);

			if( window.Handle != IntPtr.Zero) {
				if( !controlsCollection.Contains( window.Handle ) )
					controlsCollection.Add( window.Handle, this );

				SubclassWindow();

				CreatorThreadId_ = Win32.GetCurrentThreadId();

				OnHandleCreated ( EventArgs.Empty );
			}
    		}
    	
    		protected virtual void DefWndProc (ref Message m)
    		{
    			window.DefWndProc(ref m);
    		}
    		
    		protected virtual void DestroyHandle ()
    		{
			if ( IsHandleCreated ) {
				if( Handle != IntPtr.Zero) {
					controlsCollection.Remove(Handle);
				}
				if( window != null) {
					window.DestroyHandle ();
				}
			}
		}
    	
    		protected override void Dispose (bool disposing) 
    		{
			statuses [ DISPOSED ] = true;
				//FIXME: 
    			base.Dispose(disposing);
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
    			Control control  = null;
    			IntPtr 	controlHwnd = handle;
				while( controlHwnd != IntPtr.Zero) {
		 			control  = controlsCollection[controlHwnd] as Control;
		 			if( control != null) break;
		 			controlHwnd = Win32.GetParent(controlHwnd);
				}
				return control;    			
    		}
    	
    		public static Control FromHandle (IntPtr handle) 
    		{
				// FIXME: Here we have to check, whether control already exists
    			//Control control = new Control (handle);
	 			Control control  = controlsCollection[handle] as Control;
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

		internal Control getNextFocusedControlCore ( Control parent, Control ctl, bool forward )
		{
			while ( parent.Parent != null )
				parent = parent.Parent;

			Control next = parent.GetNextControl ( ctl, forward );
			while ( next != null ) {
				if ( next.TabStop && next.CanFocus )
					return next;
				next = parent.GetNextControl ( next, forward );
			}
			return null;
		}

		internal Control getNextFocusedControl ( Control parent, bool forward )
		{
			Control next = getNextFocusedControlCore ( parent, FocusedControl, forward );
			if ( next == null )
				next = getNextFocusedControlCore ( parent, null, forward );
			return next;
		}
    		
    		[MonoTODO]
    		public Control GetNextControl ( Control ctl, bool forward ) 
    		{
			Control next = null;

			if ( ctl == null ) 
				next = Controls.GetFirstControl ( forward );
			else {
				if ( forward )
					next = getNextControlForward ( ctl );
				else
					next = getNextControlBackward ( ctl );
			}
			return next;
		}

    		private Control getNextControlForward ( Control ctl ) 
		{
			if ( ctl.Controls.Count != 0 )
				return ctl.Controls.GetFirstControl ( true );

			Control parent = ctl.Parent;
			if ( parent != null ) {
				while ( parent != null ) {
					Control next = parent.Controls.GetNextControl ( ctl, true );
					if ( next != null )
						return next;
					ctl = parent;
					parent = parent.Parent;
				}
				return null;
			}
			else
				return Controls.GetFirstControl ( true );
		}

    		private Control getNextControlBackward ( Control ctl ) 
		{
			Control parent = ctl.Parent;
			if ( parent != null ) {
				Control next = parent.Controls.GetNextControl ( ctl, false );
				if ( next != null ) {
					if ( next.Controls.Count > 0 )
						return next.Controls.GetFirstControl ( false );
					else
						return next;
				}
				return parent;
			}
			else
				return Controls.GetFirstControl ( false );
		}

    		[MonoTODO]
    		protected bool GetStyle (ControlStyles flag) 
    		{
    			return (controlStyles_ & flag) != 0;
    		}
    	
    		[MonoTODO]
    		protected bool GetTopLevel () 
    		{
    			throw new NotImplementedException ();
    		}
    		
    		public void Hide ()
     		{
			Visible = false;
    		}
    		
    		[MonoTODO]
    		protected virtual void InitLayout () 
    		{
				//FIXME:
    		}
    		
 			//Compact Framework
    		public void Invalidate () 
    		{
    			if (IsHandleCreated) {
					Win32.InvalidateRect(Handle, IntPtr.Zero, 1);
    			}
    		}
    		
    		public void Invalidate ( bool invalidateChildren ) 
    		{
			Invalidate ( ) ;
			if ( invalidateChildren ) {
				foreach ( Control child in Controls )
					child.Invalidate ( invalidateChildren );
			}
		}
    		
		// tries to find appropriate owner for modal form
		internal static Control getOwnerWindow ( Control skip )
		{
			// temporary solution
			IEnumerator cw = controlsCollection.GetEnumerator ( );

			while ( cw.MoveNext ( ) ) {
				Control c = ( (DictionaryEntry) cw.Current).Value as Control;
				if ( c != null && c != skip ) {
					IntPtr parent = Win32.GetParent ( c.Handle );
					IntPtr owner  = Win32.GetWindow ( c.Handle, (uint) GetWindowConstants.GW_OWNER );
					if ( parent == IntPtr.Zero && owner == IntPtr.Zero )
						return c;
				}
			}
			return null;
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
    		
    		[MonoTODO]
    		public void Invalidate(Region region) 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		public void Invalidate (Rectangle rc, bool invalidateChildren) 
    		{
				//FIXME:
    		}
    		
    		[MonoTODO]
    		public void Invalidate(Region region,bool invalidateChildren) 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		protected void InvokeGotFocus (Control toInvoke, EventArgs e) 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		protected void InvokeLostFocus (Control toInvoke, EventArgs e) 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		protected void InvokeOnClick (Control toInvoke, EventArgs e) 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		protected void InvokePaint (Control c, PaintEventArgs e) 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		protected void InvokePaintBackground (
    			Control c,PaintEventArgs e) 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		protected virtual bool IsInputChar (char charCode)
    		{
    			return true;
    		}
    		
    		[MonoTODO]
    		protected virtual bool IsInputKey (Keys keyData) 
    		{
			return false;
    		}
    		
    		public static bool IsMnemonic (char charCode, string text)
    		{
    			if ( text == null )
				return false;
			return text.IndexOf ( "&" + charCode ) > 0;

    		}
    		
    		// methods used with events:
    		protected virtual void OnBackColorChanged (EventArgs e)
    		{
    			if (BackColorChanged != null)
    				BackColorChanged (this, e);

				foreach( Control ctl in Controls) {
					ctl.OnParentBackColorChanged(e);
				}
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
			if ( Created ) {
				e.Control.CreateControl ( );
			}
			e.Control.Visible = Visible;

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
				//FIXME:
    			// create all child windows
    			IEnumerator cw = childControls.GetEnumerator();
    
    			while (cw.MoveNext()) {
    				Console.WriteLine ("Adding Control");
    				Control control = (Control) cw.Current;
    				control.CreateControl ();
    				control.Show ();
    			}
			}
    		
    		protected virtual void OnCursorChanged (EventArgs e)
    		{
    			if (CursorChanged != null)
    				CursorChanged (this, e);
    		}
    		
    		protected virtual void OnDockChanged (EventArgs e)
    		{
			PerformLayout ( this, "Dock" );
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

				//if( font != null) {
				//	Win32.SendMessage( Handle, Msg.WM_SETFONT, font.ToHfont().ToInt32(), 0);
				//}
				Win32.SendMessage( Handle, Msg.WM_SETFONT, Font.ToHfont().ToInt32(), 0);
				Win32.SetWindowText( Handle, text);

    			if (HandleCreated != null)
    				HandleCreated (this, e);
    
    		}
    		
    		protected virtual void OnHandleDestroyed (EventArgs e) 
    		{
				if( Handle != IntPtr.Zero) {
					controlsCollection.Remove(Handle);
				}
				
    			if (HandleDestroyed != null) {
    				HandleDestroyed (this, e);
    			}
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
			DoDockAndAnchorLayout ( e );
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
				//System.Console.WriteLine("OnMouseEnter");
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
				//System.Console.WriteLine("OnMouseLeave");

				mouseIsInside_ = false;
    			if (MouseLeave != null)
    				MouseLeave (this, e);
    		}
    		
 			//Compact Framework
    		protected virtual void OnMouseMove (MouseEventArgs e) 
    		{
				// If enter and mouse pressed - do not process
				if( ((e.Button & MouseButtons.Left) != 0) && !mouseIsInside_) return;

				if( !mouseIsInside_) {
					TRACKMOUSEEVENT tme = new TRACKMOUSEEVENT();
					tme.cbSize = 16;
					tme.hWnd = Handle;
					tme.dwFlags = (int)TrackerEventFlags.TME_LEAVE;
					tme.dwHoverTime = 0;

					bool result = Win32.TrackMouseEvent(ref tme);
					if( !result) {
						System.Console.WriteLine("{0}",Win32.FormatMessage(Win32.GetLastError()));
					}
				}

				POINT pt = new POINT();
				pt.x = e.X;
				pt.y = e.Y;
				Win32.ClientToScreen(Handle, ref pt);
				IntPtr wndUnderMouse = Win32.WindowFromPoint(pt);

				if( wndUnderMouse != Handle) {
					// we are outside of the window
					if( mouseIsInside_) {
						OnMouseLeave(new EventArgs());
						mouseIsInside_ = false;
					}
				}
				else {
					if( !mouseIsInside_) {
						mouseIsInside_ = true;
						OnMouseEnter(new EventArgs());
					}
				}
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
				//FIXME:
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
                if( GetStyle(ControlStyles.UserPaint)) {
                    Brush br = new SolidBrush(BackColor);
                    e.Graphics.FillRectangle(br, e.ClipRectangle);
                    br.Dispose();
                }
            }
    		
    		protected virtual void OnParentBackColorChanged (EventArgs e) 
    		{
				BackColor = Parent.BackColor;
				// FIXME: setting BackColor fires the BackColorChanged event,
				// so we do not need to call this here
				/*
    			if (BackColorChanged != null)
    				BackColorChanged (this, e);
				*/					
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

			PerformLayout ( this, "Bounds" );
    		}
    		
    		protected virtual void OnRightToLeftChanged (EventArgs e) 
    		{
    			if (RightToLeftChanged != null)
    				RightToLeftChanged (this, e);
    		}
    		
    		protected virtual void OnSizeChanged (EventArgs e) 
    		{
			OnResize ( e );
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

			PerformLayout ( );
    		}
    		// --- end of methods for events ---
    		
    		
    		[MonoTODO]
    		public void PerformLayout () 
    		{
			PerformLayout ( null, null );
		}
    		
    		[MonoTODO]
    		public void PerformLayout (Control affectedControl,
    					   string affectedProperty) 
    		{
			if ( !statuses [ LAYOUT_SUSPENDED ] )
				OnLayout ( new LayoutEventArgs ( affectedControl, affectedProperty ) );
			else
				statuses [ LAYOUT_PENDING ] = true;
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
			if ( msg.Msg == Msg.WM_KEYDOWN ) {
				Keys keyData = (Keys)msg.WParam.ToInt32( );
    				if ( !ProcessCmdKey ( ref msg, keyData ) ) {
					if ( IsInputKey ( keyData ) )
						return false;

					return ProcessDialogKey ( keyData );
				}
				return true;
			}
			else if ( msg.Msg == Msg.WM_CHAR ) {
				if ( IsInputChar ( (char) msg.WParam ) )
					return false;

				return ProcessDialogChar ( (char) msg.WParam );
			}

			return false;
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessCmdKey (ref Message msg,
    						      Keys keyData) 
    		{
			// do something with context menu

    			if ( Parent != null )
				return Parent.ProcessCmdKey ( ref msg, keyData );
			return false;
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessDialogChar (char charCode) 
    		{
			if ( Parent != null )
				return Parent.ProcessDialogChar ( charCode );
    			return false;
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessDialogKey (Keys keyData) 
    		{
			if ( Parent != null )
				return Parent.ProcessDialogKey ( keyData );
    			return false;
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessKeyEventArgs (ref Message m) 
    		{
			bool handled = false;

			switch ( m.Msg ) {
			case Msg.WM_KEYDOWN:
				KeyEventArgs args_down = new KeyEventArgs ( (Keys)m.WParam.ToInt32() );
				OnKeyDown ( args_down );
				handled = args_down.Handled;
			break;			
			case Msg.WM_CHAR:
				KeyPressEventArgs args_press = new KeyPressEventArgs ( (char) m.WParam );
				OnKeyPress ( args_press );
				handled = args_press.Handled;
			break;
			case Msg.WM_KEYUP:
				KeyEventArgs args_up = new KeyEventArgs ( (Keys)m.WParam.ToInt32() );
				OnKeyUp ( args_up );
				handled = args_up.Handled;
			break;
			}
			
			return handled;
    		}
    		
    		[MonoTODO]
    		protected internal virtual bool ProcessKeyMessage ( ref Message m) 
    		{
			if ( Parent != null ) {
				if ( !Parent.ProcessKeyPreview ( ref m ) )
					return ProcessKeyEventArgs ( ref m );
			}
			return false;
    		}
    		
    		[MonoTODO]
    		protected virtual bool ProcessKeyPreview (ref Message m) 
    		{
    			if ( Parent != null )
				return Parent.ProcessKeyPreview ( ref m );
			return false;
    		}
    		
		// This default implementation of the ProcessMnemonic method simply
		// returns false to indicate that the control has no mnemonic.
    		protected virtual bool ProcessMnemonic (char charCode) 
    		{
    			return false;
    		}
    		
    		// used when properties/values of Control 
    		// are big enough to warrant recreating the HWND
    		protected void RecreateHandle() 
    		{
			statuses [ RECREATING_HANDLE ] = true;
			if( IsHandleCreated) {
				DestroyHandle ();
				CreateHandle ();

				UpdateZOrder ( );

				IEnumerator cw = childControls.GetEnumerator();
				while ( cw.MoveNext() )
					(( Control )cw.Current).RecreateHandle ( );

			}
			statuses [ RECREATING_HANDLE ] = false;
		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public Rectangle RectangleToClient (Rectangle r) 
    		{
				// FIXME: What to return if Handle is not created yet ?
				RECT rect = new RECT();
				rect.left = r.Left;
				rect.top = r.Top;
				rect.right = r.Right;
				rect.bottom = r.Bottom;
				Win32.ScreenToClient(Handle,ref rect);
				return new Rectangle( rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    		}
    		
 			//Compact Framework
    		[MonoTODO]
    		public Rectangle RectangleToScreen (Rectangle r) 
    		{
				// FIXME: What to return if Handle is not created yet ?
				RECT rect = new RECT();
				rect.left = r.Left;
				rect.top = r.Top;
				rect.right = r.Right;
				rect.bottom = r.Bottom;
				Win32.ClientToScreen(Handle,ref rect);
				return new Rectangle( rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
			}
    		
    		[MonoTODO]
    		protected static bool ReflectMessage (IntPtr hWnd, ref Message m) {
				bool result = false;
				Control cntrl = Control.FromHandle( hWnd);
				if( cntrl != null) {
					cntrl.WndProc(ref m);
					result = true;
				}
				return result;
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
				//FIXME:
			}
    		
    		[MonoTODO]
    		public void ResetBindings () 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		public virtual void ResetFont () 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		public virtual void ResetForeColor () 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		public void ResetImeMode () 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		public virtual void ResetRightToLeft () 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		public virtual void ResetText () 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		public void ResumeLayout () 
    		{
			statuses [ LAYOUT_SUSPENDED ] = false;
			if ( statuses [ LAYOUT_PENDING ] ) {
				PerformLayout ( );
				statuses [ LAYOUT_PENDING ] = false;
			}
		}
    		
    		[MonoTODO]
    		public void ResumeLayout (bool performLayout) 
    		{
			statuses [ LAYOUT_SUSPENDED ] = false;
			if ( performLayout && statuses [ LAYOUT_PENDING ] ) {
				PerformLayout ( );
				statuses [ LAYOUT_PENDING ] = false;
			}
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
			Scale ( ratio, ratio );
		}
    		
    		[MonoTODO]
    		public void Scale (float dx,float dy) 
    		{
			SuspendLayout();
			ScaleCore ( dx, dy );
			IEnumerator cw = childControls.GetEnumerator();
			while ( cw.MoveNext() ) {
				Control control = (Control) cw.Current;
				control.Scale ( dx, dy );
			}
			ResumeLayout();
		}
    		
    		[MonoTODO]
    		protected virtual void ScaleCore (float dx, float dy) 
    		{
			Location   = new Point ( (int) (Left * dx), (int) (Top * dy) );
			ClientSize = new Size  ((int) ( ClientSize.Width * dx ),
						(int) ( ClientSize.Height * dy) );
		}
    		
    		[MonoTODO]
    		public void Select () 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		protected virtual void Select (bool directed,bool forward) 
    		{
				//FIXME:
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
				//FIXME:
    		}
    		
    		[MonoTODO]
    		public void SetBounds (int x, int y, int width, int height) 
    		{
			SetBounds(x, y, width, height, BoundsSpecified.All);
		}
    		
    		[MonoTODO]
    		public void SetBounds (int x, int y, int width, int height, BoundsSpecified specified) 
    		{
			SetBoundsCore( x, y, width, height, specified);
		}
    		
    		[MonoTODO]
    		protected virtual void SetBoundsCore ( int x, int y, int width, int height, BoundsSpecified specified) 
    		{
			if( (specified & BoundsSpecified.X) == 0)      x = Left;
			if( (specified & BoundsSpecified.Y) == 0)      y = Top;
			if( (specified & BoundsSpecified.Width) == 0)  width = Width;
			if( (specified & BoundsSpecified.Height) == 0) height = Height;

			if( IsHandleCreated ) {
				SetWindowPosFlags flags = SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_DRAWFRAME;
				Win32.SetWindowPos( Handle, SetWindowPosZOrder.HWND_NOTOPMOST, x, y, width, height, flags);
			}

			UpdateBounds( x, y, width, height );
		}
    		
		protected virtual bool MenuPresent {
			get { return false; }
		}

    		[MonoTODO]
    		protected virtual void SetClientSizeCore (int x, int y)
    		{
			RECT rc   = new RECT();
			rc.right  = x;
			rc.bottom = y;
			
			CreateParams pars = CreateParams;		
			Win32.AdjustWindowRectEx ( ref rc, pars.Style, MenuPresent ? 1 : 0, pars.ExStyle );

			Size = new Size( rc.right - rc.left, rc.bottom - rc.top );
		}
    		
    		[MonoTODO]
    		protected void SetStyle (ControlStyles flag, bool value) 
    		{
    			if( value) {
    				controlStyles_ |= flag;
    			}
    			else {
    				controlStyles_ &= ~flag;
    			}
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
    		protected virtual void SetVisibleCore ( bool value )
    		{
			bool visibleChanged = ( visible != value );

			visible = value;

			if ( visibleChanged )
				OnVisibleChanged ( EventArgs.Empty );

			if ( IsHandleCreated ) {
				SetWindowPosFlags flags = value ? SetWindowPosFlags.SWP_SHOWWINDOW : SetWindowPosFlags.SWP_HIDEWINDOW;
				flags |= SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE;
				Win32.SetWindowPos( Handle, 0, 0, 0, 0, 0, flags );
			}

			foreach ( Control c in Controls )
				c.Visible = value ;
		}
    		
    		public void Show () 
    		{
			Visible = true;
    		}
    		
    		public void SuspendLayout () 
    		{
			statuses [ LAYOUT_SUSPENDED ] = true;
		}
    		
 			//Compact Framework
    		public void Update () 
    		{
    			Win32.UpdateWindow (Handle);
    		}
    		
    		[MonoTODO]
    		protected void UpdateBounds () 
    		{	// update control bounds with current size and position

			// currently, this function is called in responce to
			// window events, so I assume that all window handles
			// are created
			RECT rect = new RECT ( );
			Win32.GetWindowRect ( Handle, ref rect );

			IntPtr parent = Win32.GetParent ( Handle );
			if ( parent != IntPtr.Zero ) {
				Win32.ScreenToClient( parent, ref rect );
			}

			UpdateBounds ( rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top );
		}
    		
    		[MonoTODO]
    		protected void UpdateBounds (int x, int y, int width, int height) 
    		{
			int clWidth = width;
			int clHeight = height;

			CreateParams pars = CreateParams;
			// this check should be removed when all controls will use base
			// implementation of CreateParams
			if ( pars != null ) {
				RECT rc   = new RECT();
				rc.right  = width;
				rc.bottom = height;
				
				Win32.AdjustWindowRectEx ( ref rc, pars.Style, MenuPresent ? 1 : 0, pars.ExStyle );

				clWidth  -= ( ( rc.right - rc.left ) - clWidth );
				clHeight -= ( ( rc.bottom - rc.top ) - clHeight);
			}
			
			UpdateBounds ( x , y, width, height, clWidth, clHeight );
		}
    		
    		[MonoTODO]
    		protected void UpdateBounds (
    			int x, int y, int width, int height, int clientWidth,
    			int clientHeight)
		{
			oldBounds.X = bounds.X;
			oldBounds.Y = bounds.Y;
			oldBounds.Width = bounds.Width;
			oldBounds.Height = bounds.Height;

			bool bLocationChanged = ( bounds.X != x ) || ( bounds.Y != y );
			bounds.X = x;
			bounds.Y = y;
			
			bool bSizeChanged = ( bounds.Width  != width ) || ( bounds.Height != height );
			bounds.Width  = width;
			bounds.Height = height;

			this.clientWidth   = clientWidth;
			this.clientHeight  = clientHeight;

			if ( bLocationChanged )
				OnLocationChanged ( EventArgs.Empty );
			if ( bSizeChanged )
				OnSizeChanged ( EventArgs.Empty );
		}
    		
    		[MonoTODO]
    		protected void UpdateStyles () 
    		{
				//FIXME:
			}
    		
    		[MonoTODO]
    		protected void UpdateZOrder () 
    		{
			if ( !IsHandleCreated || Parent == null )
				return;

			int position = Parent.Controls.GetChildIndex ( this , false );
			switch ( position ) {
			case  0:
				BringToFront();
			break;
				// not in collection for some reason
			case -1:
			break;
			default:
				Control prev = Parent.Controls [ position - 1 ];
				if ( prev.IsHandleCreated )
					Win32.SetWindowPos( Handle, prev.Handle, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE );
			break;
			}
		}
    		

			internal MouseEventArgs Msg2MouseEventArgs( ref Message msg) {
				MouseButtons mb = MouseButtons.None;
				KeyStatusFlags keyIndicator = (KeyStatusFlags)msg.WParam.ToInt32();
				if( (keyIndicator & KeyStatusFlags.MK_LBUTTON) != 0) {
					mb |= MouseButtons.Left;
				}
				if( (keyIndicator & KeyStatusFlags.MK_RBUTTON) != 0) {
					mb |= MouseButtons.Right;
				}
				if( (keyIndicator & KeyStatusFlags.MK_MBUTTON) != 0) {
					mb |= MouseButtons.Middle;
				}
				if( (keyIndicator & KeyStatusFlags.MK_XBUTTON1) != 0) {
					mb |= MouseButtons.XButton1;
				}
				if( (keyIndicator & KeyStatusFlags.MK_XBUTTON2) != 0) {
					mb |= MouseButtons.XButton2;
				}

				return new MouseEventArgs( mb, (mb != MouseButtons.None) ? 1: 0, msg.LoWordLParam, msg.HiWordLParam, 0);
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

				if( (uint)m.Msg == Control.InvokeMessage) {
					ControlInvokeHelper helper = null;
					lock( InvokeQueue_.SyncRoot) {
						if( InvokeQueue_.Count > 0) {
							helper = (ControlInvokeHelper)InvokeQueue_.Dequeue();
						}
					}
					if( helper != null) {
						helper.ExecuteMethod();
					}
					return;
				}
				else if( m.Msg == Msg.WM_COMMAND) {
					// Notification
					m.Result = (IntPtr)1;
					OnWmCommand (ref m);
					if( m.Result != IntPtr.Zero) {
						CallControlWndProc (ref m);
					}
					return;
				}

    			switch (m.Msg) {
       			case Msg.WM_CREATE:
    				Console.WriteLine ("WM_CREATE");
    				OnHandleCreated (eventArgs);
    				break;
    			case Msg.WM_LBUTTONDBLCLK:
    				OnDoubleClick (eventArgs);
					CallControlWndProc(ref m);
    				break;
    				// OnDragDrop
    				// OnDragEnter
    				// OnDragLeave
    				// OnDragOver
    				// OnQueryContinueDrag
    			case Msg.WM_ENABLE:
    				OnEnabledChanged (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_SETFOCUS:
    				OnEnter (eventArgs);
    				OnGotFocus (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_FONTCHANGE:
    				OnFontChanged (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_DESTROY:
    				OnHandleDestroyed (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_HELP:
    				// FIXME:
    				//OnHelpRequested (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_KEYDOWN:
				if ( !ProcessKeyMessage ( ref m ) )
					CallControlWndProc( ref m );
			break;
    			case Msg.WM_CHAR:
				if ( !ProcessKeyMessage ( ref m ) )
					CallControlWndProc( ref m );
			break;
    			case Msg.WM_KEYUP:
				if ( !ProcessKeyMessage ( ref m ) )
					CallControlWndProc( ref m );
			break;
    			case Msg.WM_KILLFOCUS:
    				OnLeave (eventArgs);
    				OnLostFocus (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_MOUSEACTIVATE:
    				//OnMouseEnter (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_MOUSEHOVER: // called by TrackMouseEvent
    				OnMouseHover (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_MOUSELEAVE: // called by TrackMouseEvent
    				OnMouseLeave (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_MOUSEMOVE:
    				// FIXME:
    				OnMouseMove (Msg2MouseEventArgs(ref m));
					CallControlWndProc(ref m);
					break;
				case Msg.WM_LBUTTONDOWN:
					// FIXME:
					//OnMouseDown (eventArgs);
					CallControlWndProc(ref m);
					break;
				case Msg.WM_LBUTTONUP:
    				// FIXME:
    				//OnMouseUp (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_MOUSEWHEEL:
    				// FIXME:
    				//OnMouseWheel (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_MOVE:
    				OnMove (eventArgs);
				UpdateBounds ( );
				CallControlWndProc(ref m);
				break;
				case Msg.WM_NOTIFY:
					NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure ( m.LParam,
									typeof ( NMHDR ) );
					if( !Control.ReflectMessage( nmhdr.hwndFrom, ref m )) 
						CallControlWndProc(ref m);
					
					// FIXME: get NM_CLICKED msg from pnmh
					// OnClick (eventArgs);
					//OnNotifyMessage (eventArgs);
					break;
    			case Msg.WM_ERASEBKGND:
    				if( GetStyle(ControlStyles.UserPaint)){
	    				if( !GetStyle(ControlStyles.AllPaintingInWmPaint)) {
							PaintEventArgs eraseEventArgs = new PaintEventArgs( Graphics.FromHdc(m.WParam), new Rectangle(new Point(0,0),Size));
		    				OnPaintBackground(eraseEventArgs);
							eraseEventArgs.Dispose();
	    				}
	    				m.Result = (IntPtr)1;
    				}
    				else {
						CallControlWndProc(ref m);
    				}
    				break;
				case Msg.WM_PAINT:
					if( !GetStyle(ControlStyles.UserPaint)) {
						CallControlWndProc(ref m);
					}
					else {
						PAINTSTRUCT	ps = new PAINTSTRUCT();
						IntPtr hdc = Win32.BeginPaint( Handle, ref ps);
						Rectangle rc = new Rectangle();
						rc.X = ps.rcPaint.left;
						rc.Y = ps.rcPaint.top;
						rc.Width = ps.rcPaint.right - ps.rcPaint.left;
						rc.Height = ps.rcPaint.bottom - ps.rcPaint.top;
						PaintEventArgs paintEventArgs = new PaintEventArgs( Graphics.FromHdc(hdc), rc);
	    				if( GetStyle(ControlStyles.AllPaintingInWmPaint)) {
	    					OnPaintBackground(paintEventArgs);
	    				}
						OnPaint (paintEventArgs);
						paintEventArgs.Dispose();
						Win32.EndPaint(Handle, ref ps);
					}
					break;
    			case Msg.WM_SIZE:
    				if( GetStyle(ControlStyles.ResizeRedraw)) {
    					Invalidate();		
    				}
				CallControlWndProc(ref m);
				break;
    			case Msg.WM_WINDOWPOSCHANGED:
    				UpdateBounds ( );
    				CallControlWndProc(ref m);
    			break;
    			case Msg.WM_STYLECHANGED:
    				OnStyleChanged (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_SYSCOLORCHANGE:
    				OnSystemColorsChanged (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_SETTEXT:
    				//OnTextChanged (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_SETFONT:
    				//OnTextChanged (eventArgs);
					CallControlWndProc(ref m);
					break;
    			case Msg.WM_SHOWWINDOW:
    				OnVisibleChanged (eventArgs);
					CallControlWndProc(ref m);
					break;
				case Msg.WM_CTLCOLORLISTBOX:
					Win32.SetTextColor( m.WParam, Win32.RGB(ForeColor));
					//Win32.SetBkColor( m.WParam, 0x00FF00);
					//m.Result = Win32.GetStockObject(GSO_.LTGRAY_BRUSH);
					break;
				case Msg.WM_MEASUREITEM:
					ReflectMessage( m.WParam, ref m);
					break;
				case Msg.WM_DRAWITEM:
					Control.ReflectMessage( m.WParam, ref m);
					break;
				case Msg.WM_HSCROLL:
				case Msg.WM_VSCROLL:
					if(!Control.ReflectMessage( m.LParam, ref m )) {
    					CallControlWndProc(ref m);
					}
					break;
				case Msg.WM_SETCURSOR:
					if ( cursor != null && cursor.Handle != IntPtr.Zero ) {
						Win32.SetCursor ( cursor.Handle );
						m.Result = (IntPtr)1;
					} else
						CallControlWndProc( ref m );
				break;
				default:
					CallControlWndProc(ref m);
/*
					if( ControlRealWndProc != IntPtr.Zero) {
						CallControlWndProc(ref m);
					}
					else {
						DefWndProc (ref m);
					}
*/					
     				break;
    			}
    		}

		private void DoAnchor(Control ctrl) {
			int deltaWidth = Bounds.Width - oldBounds.Width;
			int deltaHeight = Bounds.Height - oldBounds.Height;
			int halfDeltaWidth = deltaWidth / 2;
			int halfDeltaHeight = deltaHeight / 2;
			Rectangle controlBounds = ctrl.Bounds;
			if ((ctrl.Anchor & AnchorStyles.Left) == 0) {
				controlBounds.X += halfDeltaWidth;
			}
			if ((ctrl.Anchor & AnchorStyles.Top) == 0) {
				controlBounds.Y += halfDeltaHeight;
			}
			if ((ctrl.Anchor & AnchorStyles.Right) == AnchorStyles.Right) {
				if ((ctrl.Anchor & AnchorStyles.Left) == AnchorStyles.Left) {
					controlBounds.Width += deltaWidth;
				}
				else {
					controlBounds.X += halfDeltaWidth;
				}
			}
			if ((ctrl.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom) {
				if ((ctrl.Anchor & AnchorStyles.Top) == AnchorStyles.Top) {
					controlBounds.Height += deltaHeight;
				}
				else {
					controlBounds.Y += halfDeltaHeight;
				}
			}
			ctrl.Bounds = controlBounds;
		}

		private void DoDockAndAnchorLayout ( LayoutEventArgs e ) {
			Rectangle area = DisplayRectangle;
			
			for ( int i = childControls.Count - 1; i >= 0; i-- ) {
				Control control = childControls[i];
				
				switch ( control.Dock ) {
				case DockStyle.Bottom:
					control.SetBounds ( area.Left, area.Bottom - control.Height,
								area.Width, control.Height );
					area.Height -= control.Height;
				break;
				case DockStyle.Top:
					control.SetBounds ( area.Left, area.Y, area.Width, control.Height );
					area.Y += control.Height;
					area.Height -= control.Height;
				break;
				case DockStyle.Right:
					control.SetBounds ( area.Right - control.Width,	area.Top,
								control.Width, area.Height );
					area.Width -= control.Width;
				break;
				case DockStyle.Left:
					control.SetBounds ( area.Left, area.Y, control.Width, area.Height );
					area.X += control.Width;
					area.Width -= control.Width;
				break;
				case DockStyle.None:
					DoAnchor(control);
				break;
				}
			}

			for ( int i = childControls.Count - 1; i >= 0; i-- ) {
				Control control = childControls[i];
				
				if ( control.Dock == DockStyle.Fill ) {
					control.SetBounds ( area.X, area.Y, area.Width, area.Height );
				}	
			}
		}
    		
		internal static Control FocusedControl {
			get {
				IEnumerator cw = controlsCollection.GetEnumerator ( );

				while ( cw.MoveNext ( ) ) {
					Control c = ( (DictionaryEntry) cw.Current).Value as Control;

					if ( c.Focused ) return c;
				}

				return null;
			}
		}

		internal Control getParentForm ( )
		{
			Control parent = this.Parent;
			while ( parent != null ) {
				if ( parent is Form ) 
					return parent;
				parent = parent.Parent;
			}
			return null;
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
				// If the handle has not yet been created,
				// referencing this property will force the
				// handle to be created. ( MSDN )

    				if ( !IsHandleCreated ) 
					CreateHandle ( );

    				return window.Handle;
    			}
    		}
    		
    		/// --- ISynchronizeInvoke properties ---
    		[MonoTODO]
    		public bool InvokeRequired {
    			get { 
					return CreatorThreadId_ != Win32.GetCurrentThreadId(); 
				}
    		}
    		
			private IAsyncResult DoInvoke( Delegate method, object[] args) {
				IAsyncResult result = null;
				ControlInvokeHelper helper = new ControlInvokeHelper(method, args);
				if( InvokeRequired) {
					lock( this) {
						lock( InvokeQueue_.SyncRoot) {
							InvokeQueue_.Enqueue(helper);
						}
						Win32.PostMessage(Handle, Control.InvokeMessage, 0, 0);
						result = helper;
					}
				}
				else {
					helper.CompletedSynchronously = true;
					helper.ExecuteMethod();
					result = helper;
				}
				return result;
			}

    		/// --- ISynchronizeInvoke methods ---
    		[MonoTODO]
    		public IAsyncResult BeginInvoke (Delegate method) 
    		{
				return DoInvoke( method, null);
    		}
    		
    		[MonoTODO]
    		public IAsyncResult BeginInvoke (Delegate method, object[] args) 
    		{
				return DoInvoke( method, args);
			}
    		
    		[MonoTODO]
    		public object EndInvoke (IAsyncResult asyncResult) 
    		{
				object result = null;
				ControlInvokeHelper helper = asyncResult as ControlInvokeHelper;
				if( helper != null) {
					if( !asyncResult.CompletedSynchronously) {
						asyncResult.AsyncWaitHandle.WaitOne();
					}
					result = helper.MethodResult;
				}
				return result;
			}
    		
  		//Compact Framework
    		[MonoTODO]
    		public object Invoke (Delegate method) 
    		{
				return Invoke( method, null);
			}
    		
    		//[MonoTODO]
    		public object Invoke (Delegate method, object[] args) 
    		{
				IAsyncResult result = BeginInvoke(method, args);
				return EndInvoke(result);
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
     				get {
						//FIXME:
						return base.DefaultAction;
					}
     			}
    			
    			[MonoTODO]
    			public override string Description {
     				get {
						//FIXME:
						return base.Description;
					}
     			}
    			
    			[MonoTODO]
    			public IntPtr Handle {
    				get {
						throw new NotImplementedException ();
					}
    				set {
						//FIXME:
					}
    			}
    			
    			[MonoTODO]
     			public override string Help {
     				get {
						//FIXME:
						return base.Help;
					}
     			}
    			
    			[MonoTODO]
     			public override string KeyboardShortcut {
     				get {
						//FIXME:
						return base.KeyboardShortcut;
					}
     			}
    			
    			[MonoTODO]
     			public override string Name {
     				get {
						//FIXME:
						return base.Name;
					}
     				set {
						//FIXME:
						base.Name = value;
					}
     			}
    			
    			[MonoTODO]
    			public Control Owner {
    				get { 
						throw new NotImplementedException ();
					}
    			}
    			
    			[MonoTODO]
     			public override AccessibleRole Role {
     				get {
						//FIXME:
						return base.Role;
					}
     			}
    			
    			/// --- ControlAccessibleObject Methods ---
    			[MonoTODO]
     			public override int GetHelpTopic(out string fileName) 
     			{
					//FIXME:
					return base.GetHelpTopic(out fileName);
				}
    			
    			[MonoTODO]
    			public void NotifyClients (AccessibleEvents accEvent) 
    			{
					//FIXME:
    			}
    			
    			[MonoTODO]
    			public void NotifyClients (AccessibleEvents accEvent,
    						   int childID) 
    			{
					//FIXME:
				}
    			
    			[MonoTODO]
    			public override string ToString ()
    			{
					//FIXME:
					return base.ToString();
			}
    		}
    		
    		/// sub-class: Control.ControlCollection
    		/// <summary>
    		/// Represents a collection of Control objects
    		/// </summary>
    		public class ControlCollection : IList, ICollection, IEnumerable, ICloneable {
    
			class ControlComparer : IComparer {

				int IComparer.Compare( object x, object y )
				{
					int tx = ( ( Control )x ).TabIndex;
					int ty = ( ( Control )y ).TabIndex;

					if ( tx > ty )
						return 1;
					else if ( tx < ty )
						return -1;
					else
						return 0;
				}
			}

    			private ArrayList collection = new ArrayList ();
    			protected Control owner;
    
    			/// --- ControlCollection.constructor ---
    			public ControlCollection (Control owner) 
    			{
    				this.owner = owner;
    			}
    		
    			/// --- ControlCollection Properties ---
    			public int Count {
    				get {
						return collection.Count;
					}
    			}
    		
    			public bool IsReadOnly {
    				get {
						return collection.IsReadOnly;
					}
    			}
    			
    			public virtual Control this [int index] {
    				get {
						return (Control) collection[index];
					}
    			}
    		
    			public virtual void Add (Control value) 
    			{
				if( !Contains(value)) {
					collection.Add (value);
					value.Parent = owner;
					owner.OnControlAdded ( new ControlEventArgs ( value ) );
				}
			}
    			
    			public virtual void AddRange (Control[] controls) 
    			{
					for(int i = 0; i < controls.Length; i++) {
						Add(controls[i]);
					}
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
    			public override bool Equals (object obj) 
    			{
					//FIXME:
					return base.Equals(obj);
			}

			public int GetChildIndex ( Control child )
			{
				return GetChildIndex ( child, true );
			}

			public int GetChildIndex ( Control child, bool throwException )
			{
				int index = collection.IndexOf ( child );
				if ( index == -1 && throwException )
					throw new ArgumentException( "'child' is not a child control of this parent.");
				return index;
			}

    			public IEnumerator GetEnumerator () 
    			{
    				return collection.GetEnumerator ();
    			}
    			
    			[MonoTODO]
    			public override int GetHashCode () 
    			{
				//FIXME:
				return base.GetHashCode();
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
				Remove ( this [ index ] );
				// have to give a chance to handle this situation in derived class
    				//collection.RemoveAt (index);
    			}
    			
    			public void SetChildIndex ( Control child, int newIndex ) 
    			{
				int oldIndex = collection.IndexOf ( child );
				if ( oldIndex == -1 )
					throw new ArgumentException( "'child' is not a child control of this parent.");

				if ( oldIndex != newIndex ) {
					collection.Remove ( child );

					if ( newIndex >= collection.Count )
						collection.Add ( child );
					else
						collection.Insert ( newIndex, child );
				}
    			}

			internal Control GetFirstControl ( bool direction )
			{
				if ( collection.Count == 0 )
					return null;

				ArrayList copy = collection.Clone ( ) as ArrayList;
				copy.Sort ( new ControlComparer ( ) );
				
				if ( direction )
					return copy [0] as Control;
				else {
					Control last = copy[ collection.Count - 1 ] as Control;
					if ( last.Controls.Count == 0 )
						return last;
					else
						return last.Controls.GetFirstControl ( false );
				}
			}


			internal Control GetNextControl ( Control ctl, bool forward )
			{
				if ( collection.Count == 0 )
					return null;

				ArrayList copy = collection.Clone ( ) as ArrayList;
				copy.Sort ( new ControlComparer (  ) );

				int index = copy.IndexOf ( ctl ) + ( forward ? 1 : -1 );

				if ( ( forward && index  < copy.Count ) || ( !forward && index >= 0 ) )
					return copy[index] as Control;

				return null;
			}

    			/// --- ControlCollection.IClonable methods ---
    			[MonoTODO]
    			object ICloneable.Clone ()
    			{
    				throw new NotImplementedException ();
    			}
    			
    			/// --- ControlCollection.IList properties ---
    			bool IList.IsFixedSize {
    				get {
						return collection.IsFixedSize;
					}
    			}
    
    			object IList.this [int index] {
    				get {
						return collection[index];
					}
    				set {
						collection[index] = value;
					}
    			}
    
    			object ICollection.SyncRoot {
    				get {
						return collection.SyncRoot;
					}
    			}
    	
    			bool ICollection.IsSynchronized {
    				get {
						return collection.IsSynchronized;
					}
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