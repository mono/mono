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
		
		// --- Constructors ---
		public Control ()
		{
			CreateParams cp = new CreateParams ();
			window = new ControlNativeWindow (this);
			
			cp.Caption = "";
			cp.ClassName = "mono_wine_class";
			cp.X = 10;
			cp.Y = 10;
			cp.Width = 50;
			cp.Height = 50;
			cp.ClassStyle = 0;
			cp.ExStyle = 0;
			cp.Param = 0;
			cp.Param = 0;
			cp.Style = (int) Win32.WS_OVERLAPPEDWINDOW;

			window.CreateHandle (cp);

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
			throw new NotImplementedException ();
			//this.text=text;
		}
		
		[MonoTODO]
		public Control (Control parent, string text) : this (text) 
		{
			// FIXME: set parent
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Control (string text, int left, int top, 
				int width, int height) : this(text) 
		{
			// FIXME: set size and location
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Control (Control parent,string text,int left, int top,
				int width,int height) : this (parent,text) 
		{
			// FIXME: set size and location
			throw new NotImplementedException ();
		}
		
		// --- Properties ---
		// Properties only supporting .NET framework, not stubbed out:
		//  - protected bool RenderRightToLeft {get;}
		//  - public IWindowTarget WindowTarget {get; set;}
		//[MonoTODO]
		// AccessibleObject not ready
		//public AccessibleObject AccessibilityObject {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		
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
				//return backColor;
				throw new NotImplementedException ();
			}
			set {
				//backColor=value;
				throw new NotImplementedException ();
			}
		}
		
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
		
		[MonoTODO]
		public int Bottom {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public Rectangle Bounds {
			// CHECKME:
			get {
				//return bounds;
				throw new NotImplementedException ();
			}
			set {
				//bounds=value;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool CanFocus {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool CanSelect {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool Capture {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
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
		
		[MonoTODO]
		public Rectangle ClientRectangle {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public Size ClientSize {
			get {
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
		
		[MonoTODO]
		public bool ContainsFocus {
			get {
				throw new NotImplementedException ();
			}
		}
		
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
		
		[MonoTODO]
		public bool Created {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		protected virtual CreateParams CreateParams {
			get {
				throw new NotImplementedException ();
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
		
		[MonoTODO]
		// waiting for BindingContext
		//public ControlBindingsCollection DataBindings {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		
		public static Color DefaultBackColor {
			get {
				//return SystemColors.Control;
				throw new NotImplementedException ();
			}
		}

		//[MonoTODO]
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
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public virtual Rectangle DisplayRectangle {
			get {
				throw new NotImplementedException ();
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

		[MonoTODO]
		public virtual bool Enabled {
			// CHECKME:
			get {
				//return enabled;
				throw new NotImplementedException ();
			}
			set {
				//enabled=value;
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public virtual bool Focused {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
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
		
		[MonoTODO]
		public bool IsDisposed {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool IsHandleCreated {
			get {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static Point MousePosition {
			get {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			set {
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
		
		[MonoTODO]
		// Region class not ready
		//public Region Region {
		//	// CHECKME:
		//	get {
		//		//return region;
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		//region=value;
		//		throw new NotImplementedException ();
		//	}
		//}
		
		[MonoTODO]
		protected bool ResizeRedraw {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public int Right {
			get {
				throw new NotImplementedException ();
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
		
		[MonoTODO]
		public virtual string Text {
			// CHECKME:
			get {
				//return text;
				throw new NotImplementedException ();
			}
			set {
				//text=value;
				throw new NotImplementedException ();
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
		
		[MonoTODO]
		public bool Visible {
			// CHECKME:
			get {
				//return visible;
				throw new NotImplementedException ();
			}
			set {
				//visible=value;
				throw new NotImplementedException ();
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

		[MonoTODO]
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
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		protected virtual void DefWndProc (ref Message m)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		protected virtual void DestroyHandle ()
		{
			throw new NotImplementedException ();
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
	
		[MonoTODO]
		public bool Focus () 
		{
			throw new NotImplementedException ();
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
	
		[MonoTODO]
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
		
		[MonoTODO]
		public void Hide ()
 		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void InitLayout () 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Invalidate () 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Invalidate (bool invalidateChildren) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Invalidate (Rectangle rc) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		// Region class not ready
		//public void Invalidate(Region region) 
		//{
		//	throw new NotImplementedException ();
		//}
		
		[MonoTODO]
		public void Invalidate (Rectangle rc, bool invalidateChildren) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		// Region not ready
		//public void Invalidate(Region region,bool invalidateChildren) 
		//{
		//	throw new NotImplementedException ();
		//}
		
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
		[MonoTODO]
		protected virtual void OnBackColorChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnBackgroundImageChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnBindingContextChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnCausesValidationChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		// Region not ready
		//protected virtual void OnChangeUICues(UICuesEventArgs e) 
		//{
		//	throw new NotImplementedException ();
		//}
		
		[MonoTODO]
		protected virtual void OnClick (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnContextMenuChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnControlAdded (ControlEventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnControlRemoved (ControlEventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnCreateControl ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnCursorChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnDockChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnDoubleClick (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnDragDrop (DragEventArgs drgevent)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnDragEnter (DragEventArgs drgevent)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnDragLeave (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnDragOver (DragEventArgs drgevent)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnEnabledChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnEnter (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnFontChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnForeColorChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnGiveFeedback (
			GiveFeedbackEventArgs gfbevent)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnGotFocus (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnHandleCreated (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnHandleDestroyed (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnHelpRequested (HelpEventArgs hevent) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnImeModeChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnInvalidated (InvalidateEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnKeyDown (KeyEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnKeyPress (KeyPressEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnKeyUp (KeyEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnLayout (LayoutEventArgs levent) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnLeave (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnLocationChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnLostFocus (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnMouseDown (MouseEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnMouseEnter (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnMouseHover (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnMouseLeave (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnMouseMove (MouseEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnMouseUp (MouseEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnMouseWheel (MouseEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnMove (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnNotifyMessage (Message m) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnPaint (PaintEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnPaintBackground (
			PaintEventArgs pevent) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentBackColorChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentBackgroundImageChanged (
			EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentBindingContextChanged(
			EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentEnabledChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentFontChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentForeColorChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentRightToLeftChanged (
			EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentVisibleChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnQueryContinueDrag (
			QueryContinueDragEventArgs qcdevent) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnResize (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnRightToLeftChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnSizeChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnStyleChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnSystemColorsChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnTabIndexChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnTabStopChanged (EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnTextChanged (EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnTextAlignChanged (EventArgs e) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnValidated (EventArgs e) 
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
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
		
		[MonoTODO]
		public virtual void Refresh () 
		{
			throw new NotImplementedException ();
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
		
		[MonoTODO]
		protected void SetTopLevel (bool value)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void SetVisibleCore (bool value)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Show () 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SuspendLayout () 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Update () 
		{
			throw new NotImplementedException ();
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
		
		[MonoTODO]
		protected virtual void WndProc(ref Message m) 
		{

		}
		
		/// --- Control: events ---
		[MonoTODO]
		public event EventHandler BackColorChanged;// {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
		
		[MonoTODO]
		public event EventHandler BackgroundImageChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler BindingContextChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler CausesValidationChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event UICuesEventHandler ChangeUICues {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler Click {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler ContextMenuChanged;
		
		[MonoTODO]
		public event ControlEventHandler ControlAdded	{
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event ControlEventHandler ControlRemoved	{
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler CursorChanged	{
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler DockChanged	{
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler DoubleClick {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event DragEventHandler DragDrop {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event DragEventHandler DragEnter {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler DragLeave {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event DragEventHandler DragOver {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler EnabledChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler Enter {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler FontChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler ForeColorChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event GiveFeedbackEventHandler GiveFeedback {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler GotFocus {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler HandleCreated {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler HandleDestroyed {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event HelpEventHandler HelpRequested {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler ImeModeChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event InvalidateEventHandler Invalidated {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event KeyEventHandler KeyDown {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event KeyPressEventHandler KeyPress {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event KeyEventHandler KeyUp {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event LayoutEventHandler Layout {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler Leave {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler LocationChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler LostFocus {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event MouseEventHandler MouseDown {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler MouseEnter {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler MouseHover {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler MouseLeave {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event MouseEventHandler MouseMove {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event MouseEventHandler MouseUp {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event MouseEventHandler MouseWheel {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler Move {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event PaintEventHandler Paint {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler ParentChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event QueryContinueDragEventHandler QueryContinueDrag {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler Resize {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler RightToLeftChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler SizeChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler StyleChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler SystemColorsChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler TabIndexChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler TabStopChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler TextChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler Validated {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
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
		
		[MonoTODO]
		public event EventHandler VisibleChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		/// --- IWin32Window properties
		[MonoTODO]
		public IntPtr Handle {
			get { throw new NotImplementedException (); }
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
