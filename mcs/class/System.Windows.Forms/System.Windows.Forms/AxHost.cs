//
// System.Windows.Forms.AxHost
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
//
//
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace System.Windows.Forms {

	/// <summary>
	/// Wraps ActiveX controls and exposes them as fully featured Windows Forms controls.
	///
	/// ToDo note:
	///  - Nothing is implemented
	/// </summary>
	
	[MonoTODO]
	public abstract class AxHost : Control, ISupportInitialize, ICustomTypeDescriptor {

		/// --- Constructors ---
		/// Class AxHost does not have a constructor for non-internal purposes.
		/// Thus, no constructor is stubbed out.
		/// Here are the two AxHost constructors for supporting .NET Framework infrastructure:
		/// - AxHost(String clsid);
		/// - AxHost(string clsid,int flags);
		
		
		
		
		/// --- public Properties ---
		/// Properties supporting .NET framework, only. Not stubbed out:
		///  - public bool EditMode {get;}
		[MonoTODO]
		public override Color BackColor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Image BackgroundImage {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public ContainerControl ContainingControl {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override ContextMenu ContextMenu {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Cursor Cursor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		protected override Size DefaultSize {
			get { throw new NotImplementedException (); }
		}
		
		//[MonoTODO]
		public override bool Enabled {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Font Font {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Color ForeColor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public bool HasAboutBox {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public AxHost.State OcxState {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		//		[MonoTODO]
		//		//FIXME
		//		public virtual bool RightToLeft {
		//			get { throw new NotImplementedException (); }
		//			set { throw new NotImplementedException (); }
		//		}
		
		//		[MonoTODO]
		//		//FIXME
		//		ISite Site {
		//			set { throw new NotImplementedException (); }
		//		}
		
		[MonoTODO]
		public override string Text {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		
		
		
		
		/// --- methods ---
		/// internal .NET framework supporting methods, not stubbed out:
		///  - protected virtual void CreateSink();
		///  - protected virtual void DetachSink();
		///  - public void DoVerb(int verb);
		///  - protected static Color GetColorFromOleColor(uint color);
		///  - protected static Font GetFontFromIFont(object font);
		///  - protected static Font GetFontFromIFontDisp(object font);
		///  - protected static object GetIFontDispFromFont(Font font);
		///  - protected static object GetIFontFromFont(Font font);
		///  - protected static object GetIPictureDispFromPicture(Image image);
		///  - protected static object GetIPictureFromCursor(Cursor cursor);
		///  - protected static object GetIPictureFromPicture(Image image);
		///  - protected static double GetOADateFromTime(DateTime time);
		///  - protected static uint GetOleColorFromColor(Color color);
		///  - protected static Image GetPictureFromIPicture(object picture);
		///  - protected static Image GetPictureFromIPictureDisp(object picture);
		///  - protected static DateTime GetTimeFromOADate(double date);
		///  - public void InvokeEditMode();
		///  - public void MakeDirty();
		///  - protected bool PropsValid();
		///  - protected void RaiseOnMouseDown(short button,short shift,int x,int y);
		///  - protected void RaiseOnMouseDown(short button,short shift,float x,float y);
		///  - protected void RaiseOnMouseDown(object o1,object o2,object o3,object o4);
		///  - protected void RaiseOnMouseMove(short button,short shift,int x,int y);
		///  - protected void RaiseOnMouseMove(short button,short shift,float x,float y);
		///  - protected void RaiseOnMouseMove(object o1,object o2,object o3,object o4);
		///  - protected void RaiseOnMouseUp(short button,short shift,int x,int y);
		///  - protected void RaiseOnMouseUp(short button,short shift,float x,float y);
		///  - protected void RaiseOnMouseUp(object o1,object o2,object o3,object o4);
		
		[MonoTODO]
		protected virtual void AttachInterfaces() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void BeginInit() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void CreateHandle() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void DestroyHandle() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void Dispose(bool disposing) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void EndInit() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public object GetOcx() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool HasPropertyPages() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool IsInputChar(char charCode) {
			throw new NotImplementedException ();
		}
		
		/// --- methods used with events ---
		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnForeColorChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnLostFocus(EventArgs e) {
			throw new NotImplementedException ();
		}
		/// --- END OF: methods used with events ---
		
		[MonoTODO]
		public override bool PreProcessMessage(ref Message msg) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool ProcessMnemonic(char charCode) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool ProcessDialogKey(Keys keyData) { // .NET V1.1 Beta
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void SetAboutBoxDelegate(AxHost.AboutBoxDelegate d) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void SetBoundsCore(int x,int y,int width,int height,BoundsSpecified specified) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void SetVisibleCore(bool value) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ShowAboutBox() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ShowPropertyPages() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ShowPropertyPages(Control control) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) {
			throw new NotImplementedException ();
		}
		
		/// --- events ---

		//[MonoToDo]
		//public event EventHandler BackColorChanged;
		//public new event EventHandler BackgroundImageChanged;
		//public new event EventHandler BindingContextChanged;
		//public new event UICuesEventHandler ChangeUICues;
		//public new event EventHandler Click;
		//public new event EventHandler ContextMenuChanged;
		//public new event EventHandler CursorChanged;
		//public new event EventHandler DoubleClick;
		//public new event DragEventHandler DragDrop;
		//public new event DragEventHandler DragEnter;
		//public new event EventHandler DragLeave;
		//public new event DragEventHandler DragOver;
		//public new event EventHandler EnabledChanged;
		//public new event EventHandler FontChanged;
		//public new event EventHandler ForeColorChanged;
		//public new event GiveFeedbackEventHandler GiveFeedback;
		//public new event HelpEventHandler HelpRequested;
		//public new event EventHandler ImeModeChanged;
		//public new event KeyEventHandler KeyDown;
		//public new event KeyPressEventHandler KeyPress;
		//public new event KeyEventHandler KeyUp;
		//public new event LayoutEventHandler Layout;
		//public new event MouseEventHandler MouseDown;
		//public new event EventHandler MouseEnter;
		//public new event EventHandler MouseHover;
		//public new event EventHandler MouseLeave;
		//public new event MouseEventHandler MouseMove;
		//public new event MouseEventHandler MouseUp;
		//public new event MouseEventHandler MouseWheel;
		//public new event PaintEventHandler Paint;
		//public new event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp;
		//public new event QueryContinueDragEventHandler QueryContinueDrag;
		//public new event EventHandler RightToLeftChanged;
		//public new event EventHandler StyleChanged;
		//public new event EventHandler TabIndexChanged;
		//public new event EventHandler TabStopChanged;
		//public new event EventHandler TextChanged;
		
		/// --- public delegates ---
		//[Serializable]
		protected delegate void AboutBoxDelegate();
		
		/// --- ICustomTypeDescriptor methods ---
		/// Note: all of them are supporting .NET framework, but have to be stubbed out for the interface
		
		[MonoTODO]
		AttributeCollection ICustomTypeDescriptor.GetAttributes() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		string ICustomTypeDescriptor.GetClassName() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		string ICustomTypeDescriptor.GetComponentName() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		TypeConverter ICustomTypeDescriptor.GetConverter() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		object ICustomTypeDescriptor.GetEditor(Type editorBaseType) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) 
		{
			throw new NotImplementedException ();
		}
		
		
		/// sub-class: AxHost.InvalidActiveXStateException
		/// <summary>
		/// The exception that is thrown when the ActiveX control is referenced while in an invalid state.
		/// </summary>
		[MonoTODO]
		public class InvalidActiveXStateException : Exception {

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
			void ISerializable.GetObjectData(SerializationInfo si,StreamingContext context) 
			{
				throw new NotImplementedException ();
			}
		}
	}
}
