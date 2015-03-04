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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	<pbartok@novell.com>
//
//

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Windows.Forms {
	[MonoTODO("Possibly implement this for Win32; find a way for Linux and Mac")]
	[DefaultEvent("Enter")]
	[Designer("System.Windows.Forms.Design.AxHostDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public abstract class AxHost : Control, ISupportInitialize, ICustomTypeDescriptor {
		#region AxHost Subclasses
		#region AxHost.ActiveXInvokeKind Enum
		public enum ActiveXInvokeKind {
			MethodInvoke	= 0,
			PropertyGet	= 1,
			PropertySet	= 2
		}
		#endregion	// AxHost.ActiveXInvokeKind Enum

		#region AxHost.AxComponentEditor Class
		[ComVisible (false)]
		public class AxComponentEditor : System.Windows.Forms.Design.WindowsFormsComponentEditor {
			public AxComponentEditor ()
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			public override bool EditComponent (ITypeDescriptorContext context, object obj, IWin32Window parent)
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		#endregion	// AxHost.AxComponentEditor Class

		#region AxHost.ClsidAttribute
		[AttributeUsage(AttributeTargets.Class,Inherited=false)]
		public sealed class ClsidAttribute : Attribute {
			string clsid;

			public ClsidAttribute (string clsid)
			{
				this.clsid = clsid;
			}

			public string Value {
				get { return clsid; }
			}
		}
		#endregion AxHost.ClsidAttribute
		
		#region AxHost.ConnectionPointCookie
		public class ConnectionPointCookie {
			public ConnectionPointCookie (object source, object sink, Type eventInterface)
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			public void Disconnect ()
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			~ConnectionPointCookie ()
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		#endregion	// AxHost.ConnectionPointCookie
		
		#region AxHost.InvalidActiveXStateException  Class
		public class InvalidActiveXStateException : Exception {
			private string mName;
			private ActiveXInvokeKind mKind;

			public InvalidActiveXStateException ()
			{

			}

			public InvalidActiveXStateException (string name, ActiveXInvokeKind kind)
			{
				mName = name;
				mKind = kind;
			}

			public override string ToString ()
			{
				if(mKind == ActiveXInvokeKind.MethodInvoke)
					return "Invoke:" + mName;
				else if(mKind == ActiveXInvokeKind.PropertyGet)
					return "PropertyGet:" + mName;
				else if(mKind == ActiveXInvokeKind.PropertySet)
					return "PropertySet:" + mName;

				return base.ToString();
			}
		}
		#endregion	// AxHost.InvalidActiveXStateException  Class
			
		#region AxHost.State Class
		[Serializable]
		[TypeConverter("System.ComponentModel.TypeConverter, " + Consts.AssemblySystem)]
		public class State : ISerializable {
			public State (Stream ms, int storageType, bool manualUpdate, string licKey)
			{
				//throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			protected State (SerializationInfo info, StreamingContext context)
			{
			}

			void ISerializable.GetObjectData (SerializationInfo si,StreamingContext context)
			{
				//throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		#endregion	// AxHost.State Class

		#region AxHost.TypeLibraryTimeStampAttribute Class
		[AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
		public sealed class TypeLibraryTimeStampAttribute : Attribute {
			public TypeLibraryTimeStampAttribute (string timestamp)
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			public DateTime Value {
				get {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}
			}
		}
		#endregion	// AxHost.TypeLibraryTimeStampAttribute Class

		#region AxHost.StateConverter Class
		public class StateConverter : System.ComponentModel.TypeConverter {
			public StateConverter ()
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
			{
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		#endregion	// AxHost.StateConverter Class
		#endregion	// AxHost Subclasses

		//private int flags;
		private Guid clsid;
		private object instance;
		private AboutBoxDelegate aboutDelegate = null;
		private AxHost.State ocxState = null;
		static bool runningOnWindows;

		#region Protected Constructors

		protected AxHost (string clsid) : this(clsid, 0)
		{

		}

		protected AxHost (string clsid, int flags)
		{
			this.clsid = new Guid(clsid);
			//this.flags = flags;
			this.instance = null;

			PlatformID pid = Environment.OSVersion.Platform;
			runningOnWindows = ((int) pid != 128 && (int) pid != 4 && (int) pid != 6);
		}
		#endregion	// Public Instance Properties

		#region Public Instance Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color BackColor {
			get {
				return base.BackColor;
			}
			set {
				base.BackColor = value;
			}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}

			set {
				base.BackgroundImage = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ImageLayout BackgroundImageLayout {
			get {
				return base.BackgroundImageLayout;
			}

			set {
				base.BackgroundImageLayout = value;
			}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ContainerControl ContainingControl {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ContextMenu ContextMenu {
			get {
				return base.ContextMenu;
			}

			set {
				base.ContextMenu = value;
			}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Cursor Cursor {
			get {
				return base.Cursor;
			}

			set {
				base.Cursor = value;
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool EditMode {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new virtual bool Enabled {
			get {
				return base.Enabled;
			}

			set {
				base.Enabled = value;
			}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Font Font {
			get {
				return base.Font;
			}

			set {
				base.Font = value;
			}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color ForeColor {
			get { 
				return base.ForeColor;
			}

			set {
				base.ForeColor = value;
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool HasAboutBox {
			get {
				return aboutDelegate != null;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { 
				return base.ImeMode;
			}
			set { 
				base.ImeMode = value;
			}
		}
		
		[Browsable (false)]
		[DefaultValue (null)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[RefreshProperties (RefreshProperties.All)]
		public AxHost.State OcxState {
			get {
				return ocxState;
			}

			set {
				if (ocxState == value || value == null)
				{
					return;
				}
				this.ocxState = value;
			}
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Localizable (true)]
		public new virtual bool RightToLeft {
			get {
				return base.RightToLeft == System.Windows.Forms.RightToLeft.Yes;
			}

			set {
				base.RightToLeft = (value ? System.Windows.Forms.RightToLeft.Yes : 
                                                       System.Windows.Forms.RightToLeft.No);
			}
		}
		
		public override ISite Site {
			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}
		#endregion	// Protected Constructors
		
		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}
		
		protected override Size DefaultSize {
			get {
				return new Size (75, 23);
			}
		}
		#endregion	// Protected Instance Properties

		#region Protected Static Methods
		[CLSCompliant(false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static Color GetColorFromOleColor (uint color)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static Font GetFontFromIFont (object font)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static Font GetFontFromIFontDisp (object font)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static object GetIFontDispFromFont (Font font)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static object GetIFontFromFont (Font font)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static object GetIPictureDispFromPicture (Image image)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static object GetIPictureFromCursor (Cursor cursor)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static object GetIPictureFromPicture (Image image)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static double GetOADateFromTime (DateTime time)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[CLSCompliant(false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static uint GetOleColorFromColor (Color color)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static Image GetPictureFromIPicture (object picture)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static Image GetPictureFromIPictureDisp (object picture)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected static DateTime GetTimeFromOADate (double date)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}
		#endregion	// Protected Static Methods

		#region Public Instance Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[MonoTODO]
		public void BeginInit ()
		{
		}
		
		public void DoVerb (int verb)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[MonoTODO]
		public void EndInit ()
		{
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public object GetOcx ()
		{
			return instance;
		}
		
		public bool HasPropertyPages ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void InvokeEditMode ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void MakeDirty ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		public override bool PreProcessMessage (ref Message msg)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		public void ShowAboutBox ()
		{
			if (aboutDelegate != null)
				this.aboutDelegate();
		}
		
		public void ShowPropertyPages ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		public void ShowPropertyPages (Control control)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual void AttachInterfaces ()
		{

		}
		
		protected override void CreateHandle ()
		{
			if(IsRunningOnWindows && !base.IsHandleCreated) {
				GetActiveXInstance ();
				AttachInterfaces ();

				base.CreateHandle ();
			} else {
				throw new NotSupportedException ();
			}
		}

		private void GetActiveXInstance()
		{
			if (this.instance == null) {
				object obj;
				CoCreateInstance (ref clsid, null, 1, ref IID_IUnknown, out obj);
				this.instance = obj;
			}
		}

		protected virtual object CreateInstanceCore (Guid clsid)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}


		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void CreateSink ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void DestroyHandle ()
		{
			base.DestroyHandle();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void DetachSink ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		protected override void Dispose (bool disposing)
		{
			if(disposing) {
				if(this.instance != null)
					Marshal.ReleaseComObject (this.instance);
				this.instance = null;
			}
			base.Dispose(disposing);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public new void DrawToBitmap (Bitmap bitmap, Rectangle targetBounds)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected new virtual Rectangle GetScaledBounds (Rectangle bounds, SizeF factor, BoundsSpecified specified)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		protected override bool IsInputChar (char charCode)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged(e);
		}
		
		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged(e);
		}
		
		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged(e);
		}
		
		protected override void OnHandleCreated (EventArgs e)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		protected virtual void OnInPlaceActive ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnLostFocus (EventArgs e)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override bool ProcessDialogKey (Keys keyData)
		{
			return base.ProcessDialogKey(keyData);
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected bool PropsValid ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseDown (short button, short shift, int x, int y)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseDown (short button, short shift, float x, float y)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseDown (object o1, object o2, object o3, object o4)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseMove (short button, short shift, int x, int y)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseMove (short button, short shift, float x, float y)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseMove (object o1, object o2, object o3, object o4)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseUp (short button, short shift, int x, int y)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseUp (short button, short shift, float x, float y)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseUp (object o1, object o2, object o3, object o4)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		protected void SetAboutBoxDelegate (AxHost.AboutBoxDelegate d)
		{
			this.aboutDelegate = d;
		}
		
		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore(x, y, width, height, specified);
		}
		
		protected override void SetVisibleCore (bool value)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void WndProc (ref Message m)
		{
			this.DefWndProc(ref m);
		}
		#endregion	// Protected Instance Methods

		#region Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add { throw new NotSupportedException("BackColorChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { throw new NotSupportedException("BackgroundImageChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BindingContextChanged {
			add { throw new NotSupportedException("BackgroundImageChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event UICuesEventHandler ChangeUICues {
			add { throw new NotSupportedException("ChangeUICues"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Click {
			add { throw new NotSupportedException("Click"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ContextMenuChanged {
			add { throw new NotSupportedException("ContextMenuChanged"); }
			remove { }
		}
	
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler CursorChanged {
			add { throw new NotSupportedException("CursorChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add { throw new NotSupportedException("DoubleClick"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event DragEventHandler DragDrop {
			add { throw new NotSupportedException("DragDrop"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event DragEventHandler DragEnter {
			add { throw new NotSupportedException("DragEnter"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DragLeave {
			add { throw new NotSupportedException("DragLeave"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event DragEventHandler DragOver {
			add { throw new NotSupportedException("DragOver"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler EnabledChanged {
			add { throw new NotSupportedException("EnabledChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler FontChanged {
			add { throw new NotSupportedException("FontChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { throw new NotSupportedException("ForeColorChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event GiveFeedbackEventHandler GiveFeedback {
			add { throw new NotSupportedException("GiveFeedback"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event HelpEventHandler HelpRequested {
			add { throw new NotSupportedException("HelpRequested"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { throw new NotSupportedException("ImeModeChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown {
			add { throw new NotSupportedException("KeyDown"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress {
			add { throw new NotSupportedException("KeyPress"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp {
			add { throw new NotSupportedException("KeyUp"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event LayoutEventHandler Layout {
			add { throw new NotSupportedException("Layout"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDown {
			add { throw new NotSupportedException("MouseDown"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseEnter {
			add { throw new NotSupportedException("MouseEnter"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseHover {
			add { throw new NotSupportedException("MouseHover"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseLeave {
			add { throw new NotSupportedException("MouseLeave"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseMove {
			add { throw new NotSupportedException("MouseMove"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseUp {
			add { throw new NotSupportedException("MouseUp"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseWheel {
			add { throw new NotSupportedException("MouseWheel"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { throw new NotSupportedException("Paint"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp {
			add { throw new NotSupportedException("QueryAccessibilityHelp"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event QueryContinueDragEventHandler QueryContinueDrag {
			add { throw new NotSupportedException("QueryContinueDrag"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftChanged {
			add { throw new NotSupportedException("RightToLeftChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler StyleChanged {
			add { throw new NotSupportedException("StyleChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { throw new NotSupportedException("BackgroundImageChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseClick {
			add { throw new NotSupportedException("BackgroundImMouseClickageChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseDoubleClick {
			add { throw new NotSupportedException("MouseDoubleClick"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { throw new NotSupportedException("TextChanged"); }
			remove { }
		}
		#endregion	// Events

		#region Delegates
		protected delegate void AboutBoxDelegate ();
		#endregion	// Delegates

		#region	Interfaces
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		string ICustomTypeDescriptor.GetClassName ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		string ICustomTypeDescriptor.GetComponentName ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute[] attributes)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute[] attributes)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		#endregion	// Interfaces

		internal static bool IsRunningOnWindows {
                        get { return runningOnWindows; }
                }

		static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

		[DllImport("ole32.dll")]
		static extern int CoCreateInstance (
			[In] ref Guid rclsid,
			[In, MarshalAs (UnmanagedType.IUnknown)] object pUnkOuter,
			[In] uint dwClsContext,
			[In] ref Guid riid,
			[Out, MarshalAs (UnmanagedType.Interface)] out object ppv);

	}
}
