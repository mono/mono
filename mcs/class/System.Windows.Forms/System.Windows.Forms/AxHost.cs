////
//// System.Windows.Forms.AxHost
////
//// Author:
////   stubbed out by Jaak Simm (jaaksimm@firm.ee)
////
//// (C) Ximian, Inc., 2002
////
//
//using System;
//using System.ComponentModel;
//using System.Drawing;
using System.Runtime.Serialization;

namespace System.Windows.Forms
{
	/// <summary>
	/// Wraps ActiveX controls and exposes them as fully featured Windows Forms controls.
	///
	/// ToDo note:
	///  - Nothing is implemented
	/// </summary>
	
	[MonoTODO]
	public abstract class AxHost : Control, ISupportInitialize,ICustomTypeDescriptor
	{
		/// --- Constructors ---
		/// Class AxHost does not have a constructor for non-internal purposes.
		/// Thus, no constructor is stubbed out.
		/// Here are the two AxHost constructors for supporting .NET Framework infrastructure:
		/// - AxHost(String clsid);
		/// - AxHost(string clsid,int flags);
		
		
		
		
		/// --- public Properties ---
		/// Properties supporting .NET framework, only. Not stubbed out:
		///  - public bool EditMode {get;}
//		[MonoTODO]
//		public override Color BackColor {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public override Image BackgroundImage {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public ContainerControl ContainingControl {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public override ContextMenu ContextMenu {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		protected override CreateParams CreateParams {
//			get { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public override Cursor Cursor {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		protected override Size DefaultSize {
//			get { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public virtual bool Enabled {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public override Font Font {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public override Color ForeColor {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public bool HasAboutBox {
//			get { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public AxHost.State OcxState {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public virtual bool RightToLeft {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		ISite Site {
//			set { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		public override string Text {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		
//		
//		
//		
//		/// --- methods ---
//		/// internal .NET framework supporting methods, not stubbed out:
//		///  - protected virtual void CreateSink();
//		///  - protected virtual void DetachSink();
//		///  - public void DoVerb(int verb);
//		///  - protected static Color GetColorFromOleColor(uint color);
//		///  - protected static Font GetFontFromIFont(object font);
//		///  - protected static Font GetFontFromIFontDisp(object font);
//		///  - protected static object GetIFontDispFromFont(Font font);
//		///  - protected static object GetIFontFromFont(Font font);
//		///  - protected static object GetIPictureDispFromPicture(Image image);
//		///  - protected static object GetIPictureFromCursor(Cursor cursor);
//		///  - protected static object GetIPictureFromPicture(Image image);
//		///  - protected static double GetOADateFromTime(DateTime time);
//		///  - protected static uint GetOleColorFromColor(Color color);
//		///  - protected static Image GetPictureFromIPicture(object picture);
//		///  - protected static Image GetPictureFromIPictureDisp(object picture);
//		///  - protected static DateTime GetTimeFromOADate(double date);
//		///  - public void InvokeEditMode();
//		///  - public void MakeDirty();
//		///  - protected bool PropsValid();
//		///  - protected void RaiseOnMouseDown(short button,short shift,int x,int y);
//		///  - protected void RaiseOnMouseDown(short button,short shift,float x,float y);
//		///  - protected void RaiseOnMouseDown(object o1,object o2,object o3,object o4);
//		///  - protected void RaiseOnMouseMove(short button,short shift,int x,int y);
//		///  - protected void RaiseOnMouseMove(short button,short shift,float x,float y);
//		///  - protected void RaiseOnMouseMove(object o1,object o2,object o3,object o4);
//		///  - protected void RaiseOnMouseUp(short button,short shift,int x,int y);
//		///  - protected void RaiseOnMouseUp(short button,short shift,float x,float y);
//		///  - protected void RaiseOnMouseUp(object o1,object o2,object o3,object o4);
//		
//		[MonoTODO]
//		protected virtual void AttachInterfaces() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public void BeginInit() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void CreateHandle() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void DestroyHandle() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void Dispose(bool disposing) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public virtual void EndInit() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public object GetOcx() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public bool HasPropertyPages() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override bool IsInputChar(char charCode) {
//			throw new NotImplementedException ();
//		}
//		
//		/// --- methods used with events ---
//		[MonoTODO]
//		protected override void OnBackColorChanged(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnFontChanged(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnForeColorChanged(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnHandleCreated(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnLostFocus(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		/// --- END OF: methods used with events ---
//		
//		[MonoTODO]
//		public override bool PreProcessMessage(ref Message msg) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override bool ProcessMnemonic(char charCode) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected void SetAboutBoxDelegate(AxHost.AboutBoxDelegate d) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void SetBoundsCore(int x,int y,int width,int height,BoundsSpecified specified) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void SetVisibleCore(bool value) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public void ShowAboutBox() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public void ShowPropertyPages() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public void ShowPropertyPages(Control control) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void WndProc(ref Message m) {
//			throw new NotImplementedException ();
//		}
//		
//		
//		
//		
//		
//		/// --- events ---
//
//		[MonoTODO]
//		public new event EventHandler BackColorChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//		
//
//		[MonoTODO]
//		public new event EventHandler BackgroundImageChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler BindingContextChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event UICuesEventHandler ChangeUICues {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler Click {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler ContextMenuChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler CursorChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler DoubleClick {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event DragEventHandler DragDrop {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event DragEventHandler DragEnter {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler DragLeave {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event DragEventHandler DragOver {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler EnabledChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler FontChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler ForeColorChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event GiveFeedbackEventHandler GiveFeedback {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event HelpEventHandler HelpRequested {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler ImeModeChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event KeyEventHandler KeyDown {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event KeyPressEventHandler KeyPress {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event KeyEventHandler KeyUp {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event LayoutEventHandler Layout {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event MouseEventHandler MouseDown {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler MouseEnter {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler MouseHover {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler MouseLeave {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event MouseEventHandler MouseMove {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event MouseEventHandler MouseUp {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event MouseEventHandler MouseWheel {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event PaintEventHandler Paint {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event QueryContinueDragEventHandler QueryContinueDrag {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler RightToLeftChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler StyleChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler TabIndexChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler TabStopChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//
//		[MonoTODO]
//		public new event EventHandler TextChanged {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//		
//		
//		
//		
//		
//		/// --- public delegates ---
//		[Serializable]
//		protected delegate void AboutBoxDelegate();
//		
//		
//		
//		
//		/// --- ICustomTypeDescriptor methods ---
//		/// Note: all of them are supporting .NET framework, but have to be stubbed out for the interface
//		
//		[MonoTODO]
//		AttributeCollection ICustomTypeDescriptor.GetAttributes() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		string ICustomTypeDescriptor.GetClassName() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		string ICustomTypeDescriptor.GetComponentName() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		TypeConverter ICustomTypeDescriptor.GetConverter() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
//			throw new NotImplementedException ();
//		}
//		
//		
		/// sub-class: AxHost.InvalidActiveXStateException
		/// <summary>
		/// The exception that is thrown when the ActiveX control is referenced while in an invalid state.
		/// </summary>
		[MonoTODO]
		public class InvalidActiveXStateException : Exception
		{
			/// --- methods ---
//			[MonoTODO]
//			public override string ToString() {
//				throw new NotImplementedException ();
//			}
		}
		
		
		/// sub-class: AxHost.State
		/// <summary>
		/// Encapsulates the persisted state of an ActiveX control.
		///
		/// Note: the class does not contain any documented methods, just only those supporting .NET framework
		/// </summary>
		[MonoTODO]
		[Serializable]
		public class State : ISerializable {
			
			/// The classes only constructor is supporting .NET framework, and thus not stubbed out:
			/// - [Serializable] public AxHost.State(Stream ms,int storageType,bool manualUpdate,string licKey);
			
			/// --- Methods ---
			//[Serializable]
			void ISerializable.GetObjectData(SerializationInfo si,StreamingContext context) {
				throw new NotImplementedException ();
			}
		}
	}
}
