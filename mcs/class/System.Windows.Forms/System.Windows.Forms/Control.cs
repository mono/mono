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
	///
	/// ToDo note:
	///  - no methods are implemented
	/// </summary>
	
	[MonoTODO]
	public class Control : Component , ISynchronizeInvoke, IWin32Window {

		// Helper NativeWindow class to dispatch messages back
		// to the Control class
		protected class ControlNativeWindow : NativeWindow {

			private Control control;

			public ControlNativeWindow (Control control) : base() {
				this.control = control;
			}

			protected override void WndProc (ref Message m) {
				control.WndProc (ref m);
			}
		}
		
		protected ControlNativeWindow window;

		// private fields
		//Acually many of these need to be gotten or sent to the OS, 
		//and not stored here.
		//string accessibleDefaultActionDescription;
		//string accessibleDescription;
		//string accessibleName;
		//AccessibleRole accessibleRole;
		//bool allowDrop;
		//AnchorStyles anchor;
		//Color backColor;
		//Image backgroundImage;
		//BindingContext bindingContext;
		//Rectangle bounds;
		//bool causesValidation;
		//ContextMenu contextMenu;
		//DockStyle dock;
		//bool enabled;
		//Font font;
		//Color foreColor;
		//ImeMode imeMode;
		//bool isAccessible;
		//Point location;
		//string name;
		//Region region;
		//RightToLeft rightToLeft;
		//bool tabStop;
		//string text;
		//bool visible;
		CreateParams createParams = new CreateParams ();

		// --- Constructors ---
		public Control ()
		{
			CreateHandle ();
			//Acually many of these need to be gotten or sent to 
			//the OS, and not stored here.
			//accessibleDefaultActionDescription=null;
			//accessibleDescription=null;
		//accessibleName=null;
			//accessibleRole=AccessibleRole.Default;
			//allowDrop=false;
			//anchor=AnchorStyles.Top|AnchorStyles.Left;
			//backColor=Control.DefaultBackColor;
			//backgroundImage=null;
			//bounds=new Rectangle();
			//bindingContext=null;
			//causesValidation=true;
			//contextMenu=null;
			//dock=DockStyle.None;
			//enabled=true;
			//font=Control.DefaultFont;
			//foreColor=Control.DefaultForeColor;
			//imeMode=ImeMode.Inherit;
			//isAccessible=false;
			//location=new Point (0,0);
			//name="";
			//region=null;
			//rightToLeft=RightToLeft.Inherit;
			//tabStop=false;
			//text="";
			//visible=true;
		}
		
		public Control (string text) : this() 
		{
			Win32.SetWindowTextA (Handle, text);
		}
		
		public Control (Control parent, string text) : this (text) 
		{
			Win32.SetParent (Handle, parent.Handle);
		}
		
		public Control (string text, int left, int top, 
				int width, int height) : this(text) 
		{
			Win32.SetWindowPos (Handle, (IntPtr) 0, left, top,
					    width, height, 0);
		}
		
		public Control (Control parent,string text,int left, int top,
				int width,int height) : this (parent, text)
		{
			Win32.SetWindowPos (Handle, (IntPtr) 0, left, top,
					    width, height, 0);
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

		[MonoTODO]
		public string AccessibleDefaultActionDescription {
			get {
				//return accessibleDefaultActionDescription;
				throw new NotImplementedException ();
			}
			set {
				//accessibleDefaultActionDescription=value;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string AccessibleDescription {
			get {
				//return accessibleDescription;
				throw new NotImplementedException ();
			}
			set {
				//accessibleDescription=value;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string AccessibleName {
			get {
				//return accessibleName;
				throw new NotImplementedException ();
			}
			set {
				//accessibleName=value;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public AccessibleRole AccessibleRole {
			get {
				//return accessibleRole;
				throw new NotImplementedException ();
			}
			set {
				//accessibleRole=value;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public virtual bool AllowDrop {
			get {
				//return allowDrop;
				throw new NotImplementedException ();
			}
			set {
				//allowDrop=value;
				throw new NotImplementedException ();
			}
		}
	
		[MonoTODO]
		public virtual AnchorStyles Anchor {
			get {
				//return anchor;
				throw new NotImplementedException ();
			}
			set {
				//anchor=value;
				throw new NotImplementedException ();
			}
		}
		
		public virtual Color BackColor {
			get {
				IntPtr dc = Win32.GetDC (Handle);
				uint bgColor = Win32.GetBkColor (dc);
				Win32.ReleaseDC (Handle, dc);

				int r = (int) (bgColor & 0xFF);
				int g = (int) ((bgColor >> 8) & 0xFF);
				int b = (int) ((bgColor >> 16) & 0xFF);

				return Color.FromArgb (r, g, b);
			}
			set {
				IntPtr dc = Win32.GetDC (Handle);
				Win32.SetBkColor (dc, (uint) value.ToArgb());
				Win32.ReleaseDC (Handle, dc);
			}
		}
		
		[MonoTODO]
		public virtual Image BackgroundImage {
			get {
				//return backgroundImage;
				throw new NotImplementedException ();
			}
			set {
				//backgroundImage=value;
				throw new NotImplementedException ();
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
		
		public int Bottom {
			get {
				return Top + Height;
			}
		}
		
		public Rectangle Bounds {
			get {
				Win32.RECT rect = new Win32.RECT();
				Win32.GetWindowRect (Handle, ref rect);
				return new Rectangle ((int) rect.left, 
						      (int) rect.top,
						      (int) rect.right, 
						      (int) rect.bottom);
			}
			set {
				Win32.SetWindowPos ( 
					Handle, (IntPtr) 0, value.X, value.Y,
					value.Width, value.Height, 0);
			}
		}
		
		public bool CanFocus {
			get {
				if (Handle != (IntPtr) 0 && Visible && Enabled)
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
		
		public bool Capture {
			get {
				IntPtr captured = Win32.GetCapture ();
				if (Handle == captured) 
					return true;
				return false;
			}
			set {
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
		
		[MonoTODO]
		public bool CausesValidation {
			get {
				//return causesValidation;
				throw new NotImplementedException ();
			}
			set {
				//causesValidation=value;
				throw new NotImplementedException ();
			}
		}
		
		public Rectangle ClientRectangle {
			get {
				Win32.RECT rect = new Win32.RECT();
				Win32.GetClientRect (Handle, ref rect);
				return new Rectangle ((int) rect.left, 
						      (int) rect.top,
						      (int) rect.right, 
						      (int) rect.bottom);
			}
		}
		
		[MonoTODO]
		public Size ClientSize {
			get {
				// FIXME: use GetSystemMetrics and/or
				// GetClientRect here?
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string CompanyName {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public bool ContainsFocus {
			get {
				IntPtr focusedWindow = Win32.GetFocus();
				if (focusedWindow == Handle)
					return true;
				return false;
			}
		}
		
		//[MonoTODO]
		//public virtual ContextMenu ContextMenu {
		//	get {
		//		//return contextMenu;
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		//contextMenu=value;
		//		throw new NotImplementedException ();
		//	}
		//}
		
		[MonoTODO]
		public ControlCollection Controls {
			get {
				throw new NotImplementedException ();
			}
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
		
		//[MonoTODO]
		// waiting for BindingContext
		//public ControlBindingsCollection DataBindings {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		
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
				// FIXME: use GetSystemMetrics?
				//return SystemColors.ControlText;
				throw new NotImplementedException ();
			}
		}
		
		protected virtual ImeMode DefaultImeMode {
			get {
				//return ImeMode.Inherit;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		protected virtual Size DefaultSize {
			get {
				// FIXME: use GetSystemMetrics?
				throw new NotImplementedException ();
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
		
		[MonoTODO]
		public virtual DockStyle Dock {
			// CHECKME:
			get {
				//return dock;
				throw new NotImplementedException ();
			}
			set {
				//dock=value;
				throw new NotImplementedException ();
			}
		}

		public virtual bool Enabled {
			get {
				return Win32.IsWindowEnabled (Handle);
			}
			set {
				Win32.EnableWindow (Handle, value);
			}
		}
		
		public virtual bool Focused {
			get {
				return ContainsFocus;
			}
		}
		
		// [MonoTODO]
		//public virtual Font Font {
			// CHECKME:
		//	get {
				//return font;
		//		throw new NotImplementedException ();
		//	}
		//	set {
				//font=value;
		//		throw new NotImplementedException ();
		//	}
		//}
		
		[MonoTODO]
		protected int FontHeight {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public virtual Color ForeColor {
			// CHECKME:
			get {
				//return foreColor;
				throw new NotImplementedException ();
			}
			set {
				//foreColor=value;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool HasChildren {
			// FIXME: use EnumChildWindows here?
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public int Height {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public ImeMode ImeMode {
			// CHECKME:
			get {
				//return imeMode;
				throw new NotImplementedException ();
			}
			set {
				//imeMode=value;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool IsAccessible {
			// CHECKME:
			get {
				//return isAccessible;
				throw new NotImplementedException ();
			} // default is false
			set {
				//isAccessible=value;
				throw new NotImplementedException ();
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
		
		[MonoTODO]
		public int Left {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public Point Location {
			// CHECKME:
			get {
				//return location;
				throw new NotImplementedException ();
			}
			set {
				//location=value;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Keys ModifierKeys {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static MouseButtons MouseButtons {
			get {
				// FIXME: use GetAsycKeyState?
				throw new NotImplementedException ();
			}
		}
		
		public static Point MousePosition {
			get {
				Win32.POINT point = new Win32.POINT();
				Win32.GetCursorPos (ref point);
				return new Point ( (int) point.x, (int) point.y);
			}
		}
		
		[MonoTODO]
		public string Name {
			// CHECKME:
			get {
				//return name;
				throw new NotImplementedException ();
			}
			set {
				//name=value;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public Control Parent {
			get {
				// Need to get HWND from Control class
				// Win32.GetParent (Handle);
				throw new NotImplementedException ();
			}
			set {
				// Need to get HWND from Control class
				// Win32.SetParent (value);
				throw new NotImplementedException ();
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
		
		// [MonoTODO]
		public Region Region {
			// CHECKME:
			get {
				//return region;
				throw new NotImplementedException ();
			}
			set {
				//region=value;
				throw new NotImplementedException ();
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
		
		public int Right {
			get {
				return Left + Width;
			}
		}
		
		[MonoTODO]
		public virtual RightToLeft RightToLeft {
			// CHECKME:
			get {
				//return rightToLeft;
				throw new NotImplementedException ();
			}
			set {
				//rightToLeft=value;
				throw new NotImplementedException ();
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
		
		[MonoTODO]
		public Size Size {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
		
		[MonoTODO]
		public bool TabStop {
			// CHECKME:
			get {
				//return tabStop;
				throw new NotImplementedException ();
			}
			set {
				//tabStop=value;
				throw new NotImplementedException ();
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
		
		public virtual string Text {
			get {
				String text = "";
				int length = Win32.GetWindowTextLengthA (Handle);
				Win32.GetWindowTextA (Handle, ref text, length);
				return text;
			}
			set {
				Win32.SetWindowTextA (Handle, value);
			}
		}
		
		[MonoTODO]
		public int Top {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public Control TopLevelControl {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool Visible {
			get {
				throw new NotImplementedException ();
			}
			set {
				if (value)
					Win32.ShowWindow (
						Handle, Win32.SW_SHOW);
				else
					Win32.ShowWindow (
						Handle, Win32.SW_HIDE);
			}
		}
		
		[MonoTODO]
		public int Width {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool Contains (Control ctl) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void CreateControl () 
		{
			throw new NotImplementedException ();
		}

		//[MonoTODO]
		// AccessibleObject not ready
		//protected virtual AccessibleObject CreateAccessibilityInstance() {
		//	throw new NotImplementedException ();
		//}
		
		[MonoTODO]
		protected virtual ControlCollection CreateControlsInstance ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Graphics CreateGraphics () 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		protected virtual void CreateHandle ()
		{
			// FIXME: should window creation happen here
			// (in the constructor)?
			window = new ControlNativeWindow (this);
			
			createParams.Caption = "";
			createParams.ClassName = "mono_native_window";
			createParams.X = 10;
			createParams.Y = 10;
			createParams.Width = 50;
			createParams.Height = 50;
			createParams.ClassStyle = 0;
			createParams.ExStyle = 0;
			createParams.Param = 0;
			createParams.Param = 0;
			createParams.Style = (int) Win32.WS_OVERLAPPEDWINDOW;

			window.CreateHandle (createParams);
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
	
		[MonoTODO]
		public static Control FromHandle (IntPtr handle) 
		{
			throw new NotImplementedException ();
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
		
		public void Hide ()
 		{
			Win32.ShowWindow (Handle, Win32.SW_HIDE);
		}
		
		[MonoTODO]
		protected virtual void InitLayout () 
		{
			throw new NotImplementedException ();
		}
		
		public void Invalidate () 
		{
			Win32.RECT rect = (Win32.RECT) null;
			Win32.InvalidateRect (Handle, ref rect, true);
		}
		
		[MonoTODO]
		public void Invalidate (bool invalidateChildren) 
		{
			throw new NotImplementedException ();
		}
		
		public void Invalidate (Rectangle rc) 
		{
			Win32.RECT rect = new Win32.RECT();
			rect.left = rc.Left;
			rect.top = rc.Top;
			rect.right = rc.Right;
			rect.bottom = rc.Bottom;
			Win32.InvalidateRect (Handle, ref rect, true);
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
		
		protected virtual void OnGotFocus (EventArgs e) 
		{
			if (GotFocus != null)
				GotFocus (this, e);
		}
		
		protected virtual void OnHandleCreated (EventArgs e) 
		{
			if (HandleCreated != null)
				HandleCreated (this, e);
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
		
		protected virtual void OnKeyDown (KeyEventArgs e) 
		{
			if (KeyDown != null)
				KeyDown (this, e);
		}
		
		protected virtual void OnKeyPress (KeyPressEventArgs e) 
		{
			if (KeyPress != null)
				KeyPress (this, e);
		}
		
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
		
		protected virtual void OnLostFocus (EventArgs e) 
		{
			if (LostFocus != null)
				LostFocus (this, e);
		}
		
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
		
		protected virtual void OnMouseMove (MouseEventArgs e) 
		{
			if (MouseMove != null)
				MouseMove (this, e);
		}
		
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
		
		protected virtual void OnPaint (PaintEventArgs e) 
		{
			if (Paint != null)
				Paint (this, e);
		}
		
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
		
		protected virtual void OnTextChanged (EventArgs e) 
		{
			if (TextChanged != null)
				TextChanged (this, e);
		}

		[MonoTODO] // this doesn't seem to be documented
// 		protected virtual void OnTextAlignChanged (EventArgs e) {
// 			TextAlignChanged (this, e);
// 		}
		
		protected virtual void OnValidated (EventArgs e) 
		{
			if (Validated != null)
				Validated (this, e);
		}
		
		[MonoTODO]
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
		
		[MonoTODO]
		public Point PointToClient (Point p) 
		{
			throw new NotImplementedException ();
		}
		
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
		
		[MonoTODO]
		protected void RecreateHandle() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Rectangle RectangleToClient (Rectangle r) 
		{
			throw new NotImplementedException ();
		}
		
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
		
		public virtual void Refresh () 
		{
			Win32.RECT rect = (Win32.RECT) null;
			Win32.InvalidateRect (Handle, ref rect, true);
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
						    (IntPtr) Win32.HWND_NOTOPMOST,
						    0, 0, 0, 0, 0);
			else
				// FIXME: this does not make sense but
				// the docs say the window is hidden
				Win32.ShowWindow (Handle, Win32.SW_HIDE);
		}
		
		[MonoTODO]
		protected virtual void SetVisibleCore (bool value)
		{
			throw new NotImplementedException ();
		}
		
		public void Show () 
		{
			Win32.ShowWindow (Handle, Win32.SW_SHOW);
		}
		
		[MonoTODO]
		public void SuspendLayout () 
		{
			throw new NotImplementedException ();
		}
		
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
			PaintEventArgs paintEventArgs = new PaintEventArgs (new Graphics(), new Rectangle());

			switch (m.Msg) {

			case Win32.WM_CREATE:
				OnHandleCreated (eventArgs);
				break;
			case Win32.WM_LBUTTONDBLCLK:
				OnDoubleClick (eventArgs);
				break;
				// OnDragDrop
				// OnDragEnter
				// OnDragLeave
				// OnDragOver
				// OnQueryContinueDrag
			case Win32.WM_ENABLE:
				OnEnabledChanged (eventArgs);
				break;
			case Win32.WM_SETFOCUS:
				OnEnter (eventArgs);
				OnGotFocus (eventArgs);
				break;
			case Win32.WM_FONTCHANGE:
				OnFontChanged (eventArgs);
				break;
			case Win32.WM_DESTROY:
				OnHandleDestroyed (eventArgs);
				break;
			case Win32.WM_HELP:
				// FIXME:
				//OnHelpRequested (eventArgs);
				break;
			case Win32.WM_KEYDOWN:
				// FIXME:
				// OnKeyDown (eventArgs);
				break;
			case Win32.WM_CHAR:
				// FIXME:
				// OnKeyPress (eventArgs);
				break;
			case Win32.WM_KEYUP:
				// FIXME:
				// OnKeyUp (eventArgs);
				break;
			case Win32.WM_KILLFOCUS:
				OnLeave (eventArgs);
				OnLostFocus (eventArgs);
				break;
			case Win32.WM_LBUTTONDOWN:
				// FIXME:
				// OnMouseDown (eventArgs);
				break;
			case Win32.WM_MOUSEACTIVATE:
				OnMouseEnter (eventArgs);
				break;
			case Win32.WM_MOUSEHOVER: // called by TrackMouseEvent
				OnMouseHover (eventArgs);
				break;
			case Win32.WM_MOUSELEAVE: // called by TrackMouseEvent
				OnMouseLeave (eventArgs);
				break;
			case Win32.WM_MOUSEMOVE:
				// FIXME:
				// OnMouseMove (eventArgs);
				break;
			case Win32.WM_LBUTTONUP:
				// FIXME:
				// OnMouseUp (eventArgs);
				break;
			case Win32.WM_MOUSEWHEEL:
				// FIXME:
				// OnMouseWheel (eventArgs);
				break;
			case Win32.WM_MOVE:
				OnMove (eventArgs);
				break;
			case Win32.WM_NOTIFY:
				// FIXME: get NM_CLICKED msg from pnmh
				// OnClick (eventArgs);
				// OnNotifyMessage (eventArgs);
			case Win32.WM_PAINT:
				OnPaint (paintEventArgs);
				break;
			case Win32.WM_SIZE:
				OnResize (eventArgs);
				OnSizeChanged (eventArgs);
				break;
			case Win32.WM_STYLECHANGED:
				OnStyleChanged (eventArgs);
				break;
			case Win32.WM_SYSCOLORCHANGE:
				OnSystemColorsChanged (eventArgs);
				break;
			case Win32.WM_SETTEXT:
				OnTextChanged (eventArgs);
				break;
			case Win32.WM_SHOWWINDOW:
				OnVisibleChanged (eventArgs);
				break;
			default:
				DefWndProc (ref m);
				break;
			}
		}

		/// --- Control: events ---
		public event EventHandler BackColorChanged;// {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
		
		public event EventHandler BackgroundImageChanged; //{
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler BindingContextChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler CausesValidationChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event UICuesEventHandler ChangeUICues; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler Click; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler ContextMenuChanged;
		
		public event ControlEventHandler ControlAdded;	// {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event ControlEventHandler ControlRemoved; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler CursorChanged;	// {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler DockChanged;	// {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler DoubleClick; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event DragEventHandler DragDrop; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event DragEventHandler DragEnter; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler DragLeave; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event DragEventHandler DragOver; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler EnabledChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler Enter; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler FontChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler ForeColorChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event GiveFeedbackEventHandler GiveFeedback; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler GotFocus; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler HandleCreated; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler HandleDestroyed; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event HelpEventHandler HelpRequested; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler ImeModeChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event InvalidateEventHandler Invalidated; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event KeyEventHandler KeyDown; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event KeyPressEventHandler KeyPress; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event KeyEventHandler KeyUp; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event LayoutEventHandler Layout; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler Leave; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler LocationChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler LostFocus; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event MouseEventHandler MouseDown; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler MouseEnter; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler MouseHover; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler MouseLeave; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event MouseEventHandler MouseMove; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event MouseEventHandler MouseUp; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event MouseEventHandler MouseWheel; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler Move; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event PaintEventHandler Paint; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler ParentChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event QueryContinueDragEventHandler QueryContinueDrag; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler Resize; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler RightToLeftChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler SizeChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler StyleChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler SystemColorsChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler TabIndexChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler TabStopChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler TextChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler Validated; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		[MonoTODO]
		// CancelEventHandler not yet defined
		//public event CancelEventHandler Validating {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}
		
		public event EventHandler VisibleChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		/// --- IWin32Window properties
		public IntPtr Handle {
			get { return window.Handle; }
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
		
		[MonoTODO]
		public object Invoke (Delegate method) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public object Invoke (Delegate method,object[] args) 
		{
			throw new NotImplementedException ();
		}
		
		/// sub-class: Control.ControlAccessibleObject
		/// <summary>
		/// Provides information about a control that can be used by an accessibility application.
		/// </summary>
		public class ControlAccessibleObject /*: AccessibleObject*/ {
			// AccessibleObject not ready to be base class
			/// --- ControlAccessibleObject.constructor ---
			[MonoTODO]
			public ControlAccessibleObject (Control ownerControl) 
			{
				throw new NotImplementedException ();
			}
			
			
			/// --- ControlAccessibleObject Properties ---
			[MonoTODO]
// 			public override string DefaultAction {
// 				get { throw new NotImplementedException (); }
// 			}
			
			[MonoTODO]
// 			public override string Description {
// 				get { throw new NotImplementedException (); }
// 			}
			
			[MonoTODO]
			public IntPtr Handle {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
// 			public override string Help {
// 				get { throw new NotImplementedException (); }
// 			}
			
			[MonoTODO]
// 			public override string KeyboardShortcut {
// 				get { throw new NotImplementedException (); }
// 			}
			
			[MonoTODO]
// 			public override string Name {
// 				get { throw new NotImplementedException (); }
// 				set { throw new NotImplementedException (); }
// 			}
			
			[MonoTODO]
			public Control Owner {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
// 			public override AccessibleRole Role {
// 				get { throw new NotImplementedException (); }
// 			}
			
			
			/// --- ControlAccessibleObject Methods ---
			[MonoTODO]
// 			public override int GetHelpTopic(out string fileName) 
// 			{
// 				throw new NotImplementedException ();
// 			}
			
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
			
			/// --- ControlCollection.constructor ---
			[MonoTODO]
			public ControlCollection (Control owner) 
			{
				throw new NotImplementedException ();
			}
		
			/// --- ControlCollection Properties ---
			[MonoTODO]
			public int Count {
				get { throw new NotImplementedException (); }
			}
		
			[MonoTODO]
			public bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public virtual Control this [int index] {
				get { throw new NotImplementedException (); }
			}
		
		
			/// --- ControlCollection Methods ---
			/// .NET framework supporting internal methods, not stubbed out:
			/// - object ICloneable.Clone();
			/// Note: IList methods stubbed out, otherwise does not compile
			
			[MonoTODO]
			public virtual void Add (Control value) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual void AddRange (Control[] controls) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual void Clear () 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			public bool Contains (Control control) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void CopyTo (Array dest,int index) 
			{
				throw new NotImplementedException ();
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
			
			[MonoTODO]
			public int GetChildIndex (Control child,
						  bool throwException) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public IEnumerator GetEnumerator () 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public override int GetHashCode () 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public int IndexOf (Control control) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual void Remove (Control value) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void RemoveAt (int index) 
			{
				throw new NotImplementedException ();
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
			[MonoTODO]
			bool IList.IsFixedSize {
				get { throw new NotImplementedException (); }
			}

			[MonoTODO]			
			object IList.this [int index] {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}

			[MonoTODO]				
			object ICollection.SyncRoot {
				get { throw new NotImplementedException (); }
			}
	
			[MonoTODO]				
			bool ICollection.IsSynchronized {
				get { throw new NotImplementedException (); }
			}
			
			/// --- ControlCollection.IList methods ---
			[MonoTODO]
			int IList.Add (object control) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			bool IList.Contains (object control) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			int IList.IndexOf (object control) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			void IList.Insert (int index,object value) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			void IList.Remove (object control) 
			{
				throw new NotImplementedException ();
			}
		}  // --- end of Control.ControlCollection ---
	}
}
