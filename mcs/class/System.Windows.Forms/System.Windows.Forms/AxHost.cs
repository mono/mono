//
// System.Windows.Forms.AxHost
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002/3
//
//

//
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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace System.Windows.Forms {

	/// <summary>
	/// Wraps ActiveX controls and exposes them as fully featured Windows Forms controls.
	/// </summary>
	
	[MonoTODO]
	public abstract class AxHost : Control, ISupportInitialize, ICustomTypeDescriptor {

		/// --- Constructors ---
		/// Class AxHost does not have a constructor for non-internal purposes.
		/// Thus, no constructor is stubbed out.
		/// Here are the two AxHost constructors for supporting .NET Framework infrastructure:
		protected AxHost(string clsid){
		}

		protected AxHost(string clsid,int flags){
		}
		
		/// --- public Properties ---
		/// Properties supporting .NET framework, only. Not stubbed out:
		public bool EditMode {
			get {
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public override Color BackColor {
			get {
				//FIXME:
				return base.BackColor;
			}
			set {
				//FIXME:
				base.BackColor = value;
			}
		}
		
		[MonoTODO]
		public override Image BackgroundImage {
			get {
				//FIXME:
				return base.BackgroundImage;
			}
			set {
				//FIXME:
				base.BackgroundImage = value;
			}
		}
		
		[MonoTODO]
		public ContainerControl ContainingControl {
			get {
				throw new NotImplementedException (); 
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public override ContextMenu ContextMenu {
			get {
				//FIXME:
				return base.ContextMenu;
			}
			set {
				//FIXME:
				base.ContextMenu = value;
			}
		}
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				//FIXME:
				return base.CreateParams;
			}
		}
		
		[MonoTODO]
		public override Cursor Cursor {
			get {
				//FIXME:
				return base.Cursor;
			}
			set {
				//FIXME:
				base.Cursor = value;
			}
		}
		
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				//FIXME:
				return base.DefaultSize;
			}
		}
		
		[MonoTODO]
		public new virtual bool Enabled {
			get {
				//FIXME:
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public override Font Font {
			get {
				//FIXME:
				return base.Font;
			}
			set {
				//FIXME:
				base.Font = value;
			}
		}
		
		[MonoTODO]
		public override Color ForeColor {
			get { 
				//FIXME:
				return base.ForeColor; 
			}
			set {
				//FIXME:
				base.ForeColor = value; 
			}
		}
		
		[MonoTODO]
		public bool HasAboutBox {
			get { 
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public AxHost.State OcxState {
			get {
				//FIXME:
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		//FIXME
		public new virtual bool RightToLeft {
			get {
				throw new NotImplementedException (); 
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public override ISite Site {
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public override string Text {
			get {
				//FIXME:
				return base.Text; 
			}
			set {
				//FIXME:
				base.Text = value;
			}
		}
		
		/// --- methods ---
		/// internal .NET framework supporting methods, not stubbed out:
		/// 

		[MonoTODO]
		protected virtual void CreateSink(){
		}

		[MonoTODO]
		protected virtual void DetachSink(){
		}

		[MonoTODO]
		public void DoVerb(int verb){
		}

		[MonoTODO]
		[CLSCompliant(false)]
		protected static Color GetColorFromOleColor(uint color){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static Font GetFontFromIFont(object font){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static Font GetFontFromIFontDisp(object font){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static object GetIFontDispFromFont(Font font){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static object GetIFontFromFont(Font font){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static object GetIPictureDispFromPicture(Image image){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static object GetIPictureFromCursor(Cursor cursor){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static object GetIPictureFromPicture(Image image){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static double GetOADateFromTime(DateTime time){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		[CLSCompliant(false)]
		protected static uint GetOleColorFromColor(Color color){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static Image GetPictureFromIPicture(object picture){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static Image GetPictureFromIPictureDisp(object picture){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		protected static DateTime GetTimeFromOADate(double date){
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public void InvokeEditMode(){
		}

		[MonoTODO]
		public void MakeDirty(){
		}

		protected bool PropsValid(){
			throw new NotImplementedException (); 
		}

		protected void RaiseOnMouseDown(short button,short shift,int x,int y){
		}

		protected void RaiseOnMouseDown(short button,short shift,float x,float y){
		}

		protected void RaiseOnMouseDown(object o1,object o2,object o3,object o4){
		}

		protected void RaiseOnMouseMove(short button,short shift,int x,int y){
		}

		protected void RaiseOnMouseMove(short button,short shift,float x,float y){
		}

		protected void RaiseOnMouseMove(object o1,object o2,object o3,object o4){
		}

		protected void RaiseOnMouseUp(short button,short shift,int x,int y){
		}

		protected void RaiseOnMouseUp(short button,short shift,float x,float y){
		}

		protected void RaiseOnMouseUp(object o1,object o2,object o3,object o4){
		}
		
		[MonoTODO]
		protected virtual void AttachInterfaces() {
			//FIXME:
		}
		
		[MonoTODO]
		public void BeginInit() {
			//FIXME:
		}
		
		[MonoTODO]
		protected override void CreateHandle() {
			//FIXME:
			base.CreateHandle();
		}
		
		[MonoTODO]
		protected override void DestroyHandle() {
			//FIXME:
			base.DestroyHandle();
		}
		
		[MonoTODO]
		protected override void Dispose(bool disposing) {
			//FIXME:
			base.Dispose(disposing);
		}
		
		[MonoTODO]
		public virtual void EndInit() {
			//FIXME:
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
			//FIXME:
			return base.IsInputChar(charCode);
		}
		
		/// --- methods used with events ---
		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e) {
			//FIXME:
			base.OnBackColorChanged(e);
		}
		
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) {
			//FIXME:
			base.OnFontChanged(e);
		}
		
		[MonoTODO]
		protected override void OnForeColorChanged(EventArgs e) {
			//FIXME:
			base.OnForeColorChanged(e);
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) {
			//FIXME:
			base.OnHandleCreated(e);
		}

		[MonoTODO]
		protected virtual void OnInPlaceActive() {
			//FIXME:
			
		}
		
		[MonoTODO]
		protected override void OnLostFocus(EventArgs e) {
			//FIXME:
			base.OnLostFocus(e);
		}
		/// --- END OF: methods used with events ---
		
		[MonoTODO]
		public override bool PreProcessMessage(ref Message msg) {
			//FIXME:
			return base.PreProcessMessage(ref msg);
		}
		
		[MonoTODO]
		protected override bool ProcessMnemonic(char charCode) {
			//FIXME:
			return base.ProcessMnemonic(charCode);
		}

		[MonoTODO]
		protected override bool ProcessDialogKey(Keys keyData) { // .NET V1.1 Beta
			//FIXME:
			return base.ProcessDialogKey(keyData);
		}
		
		[MonoTODO]
		protected void SetAboutBoxDelegate(AxHost.AboutBoxDelegate d) {
			//FIXME:
		}
		
		[MonoTODO]
		protected override void SetBoundsCore(int x,int y,int width,int height,BoundsSpecified specified) {
			//FIXME:
			base.SetBoundsCore(x,y,width,height,specified);
		}
		
		[MonoTODO]
		protected override void SetVisibleCore(bool value) {
			//FIXME:
			base.SetVisibleCore(value);
		}
		
		[MonoTODO]
		public void ShowAboutBox() {
			//FIXME:
		}
		
		[MonoTODO]
		public void ShowPropertyPages() {
			//FIXME:
		}
		
		[MonoTODO]
		public void ShowPropertyPages(Control control) {
			//FIXME:
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) {
			//FIXME:
			base.WndProc(ref m);
		}
		
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
			//FIXME:
			return "";
		}
		
		[MonoTODO]
		string ICustomTypeDescriptor.GetComponentName() 
		{
			//FIXME:
			return "";
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

		//protected bool RenderRightToLeft{
		//}

		public enum ActiveXInvokeKind {
			MethodInvoke = 0,
			PropertyGet = 1,
			PropertySet = 2
		}
		
		[MonoTODO]
			public class AxComponentEditor {// add ref to swf.desing : WindowsFormsComponentEditor {

		}
		
		[MonoTODO]
			public class ConnectionPointCookie {
		}
		
		public class StateConverter : System.ComponentModel.TypeConverter {
		}

		[AttributeUsage(AttributeTargets.Class)]
		public sealed class ClsidAttribute : Attribute {
			string clsid;

			public ClsidAttribute (string clsid)
			{
				this.clsid = clsid;
			}

			public string Value {
				get {
					return clsid;
				}
			}
		}

		[AttributeUsage(AttributeTargets.Assembly)]
			public sealed class TypeLibraryTimeStampAttribute : Attribute{
		}


	}
}
