//
// System.Windows.Forms.Form
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms {

	public class Form : ContainerControl  {

		string caption;

		public Form () : base ()
		{
		}
		
		static Form ()
		{

		}
		
		//  --- Public Properties
		//
		[MonoTODO]
		public IButtonControl AcceptButton {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static Form ActiveForm {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Form ActiveMdiChild {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool AutoScale {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual Size AutoScaleBaseSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override bool AutoScroll {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override Color BackColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public IButtonControl CancelButton {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public new Size ClientSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool ControlBox {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Rectangle DesktopBounds {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Point DesktopLocation {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public DialogResult DialogResult {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public FormBorderStyle FormBorderStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool HelpButton {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		// Icon class not yet stubbed/implemented
		//public Icon Icon {
		//	 get {
		//		 throw new NotImplementedException ();
		//	 }
		//	 set {
		//		 throw new NotImplementedException ();
		//	 }
		//}

		[MonoTODO]
		public bool IsMidiChild {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool IsMidiContainer {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool KeyPreview {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool MaximizeBox {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Size MaximumSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Form[] MdiChildren {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Form MdiParent {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		//public MainMenu Menu {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}

		[MonoTODO]
		//public MainMenu MergedMenu {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		[MonoTODO]
		public bool MinimizeBox {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Size MinimumSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool Modal {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public double Opacity {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Form[] OwnedForms {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Form Owner {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool ShowInTaskbar {
			get {
				throw new NotImplementedException ();
			}
			set {
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
		public SizeGripStyle SizeGripStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public FormStartPosition StartPosition {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool TopLevel {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool TopMost {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Color TransparencyKey {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public FormWindowState WindowState {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		
		//  --- Public Methods
		public void Activate ()
		{
			Win32.SetActiveWindow (window.Handle);
		}

		[MonoTODO]
		public void AddOwnedForm (Form ownedForm)
		{
			throw new NotImplementedException ();
		}

		public void Close ()
		{
			Win32.CloseWindow (window.Handle);
		}

		//inherited
		//public void Dispose ()
		//{
		//		throw new NotImplementedException ();
		//}
		//public static bool Equals (object o1, object o2)
		//{
		//		throw new NotImplementedException ();
		//}		 [MonoTODO]

		public override bool Equals (object o)
		{
			throw new NotImplementedException ();
		}


		[MonoTODO]
		public override int GetHashCode () {
			//FIXME add our proprities
			return base.GetHashCode ();
		}

		[MonoTODO]
		// Font class not implemented or stubbed
 		//public static SizeF GetAutoScaleSize(Font font)
 		//{
 		//	throw new NotImplementedException ();
 		//}

		//public void Invalidate()
		//{
		//		throw new NotImplementedException ();
		//}

		//public object Invoke()
		//{
		//		throw new NotImplementedException ();
		//}

		[MonoTODO]
		public void LayoutMdi (MdiLayout value)
		{
			throw new NotImplementedException ();
		}

		//public void PerformLayout()
		//{
		//		throw new NotImplementedException ();
		//}

		[MonoTODO]
		public void RemoveOwnedForm (Form ownedForm)
		{
			throw new NotImplementedException ();
		}

		//	public void ResumeLayout()
		//	{
		//		throw new NotImplementedException ();
		//	}
		//
		//	public void Scale(float f)
		//	{
		//		throw new NotImplementedException ();
		//	}
		//
		//	public void Select()
		//	{
		//		throw new NotImplementedException ();
		//	}
		//
		//	public void SetBounds(int x, int y, int width, int height)
		//	{
		//		throw new NotImplementedException ();
		//	}

		public void SetDesktopLocation (int x, int y)
		{
			Win32.SetWindowPos ((IntPtr) window.Handle, (IntPtr) 0, 
					    x, y, 0, 0, 
					    (int) (Win32.SWP_NOSIZE | 
					    Win32.SWP_NOZORDER));
		}

		public new void Show ()
		{
			Win32.ShowWindow (window.Handle, (int) Win32.SW_SHOW);
		}

		[MonoTODO]
		public DialogResult ShowDialog ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		//  --- Public Events
		
		public event EventHandler Activated; //{
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}
		
		public event EventHandler Closed;
		 
		// CancelEventHandler not yet implemented/stubbed
		//public event CancelEventHandler Closing;
		
		public event EventHandler Deactivate; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}

		public event InputLanguageChangedEventHandler InputLanguageChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}

		[MonoTODO]
		public event InputLanguageChangingEventHandler InputLanguageChanging; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}

		[MonoTODO]
		public event EventHandler  Load; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}

		[MonoTODO]
		public event EventHandler  MaximizedBoundsChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}

		[MonoTODO]
		public event EventHandler MaximumSizeChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}

		[MonoTODO]
		public event EventHandler  MdiChildActivate; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}

		[MonoTODO]
		public event EventHandler  MenuComplete; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}

		[MonoTODO]
		public event EventHandler  MenuStart; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}

		[MonoTODO]
		public event EventHandler  MinimumSizedChanged; // {
// 			add {
// 				throw new NotImplementedException ();
// 			}
// 			remove {
// 				throw new NotImplementedException ();
// 			}
// 		}

		
		//  --- Protected Properties
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		[MonoTODO]
		protected override ImeMode DefaultImeMode {
			get {
				throw new NotImplementedException ();
			}
		}

		//[MonoTODO]
		////FIXME
		//protected override Size DefaultSize {
		//}

		[MonoTODO]
		protected Rectangle MaximizedBounds {
 			get {
 				throw new NotImplementedException ();
 			}
 			set {
 				throw new NotImplementedException ();
 			}
 		}

		
		//  --- Protected Methods
		
		[MonoTODO]
		protected override void AdjustFormScrollbars (bool displayScrollbars)
		{
			throw new NotImplementedException ();
		}

		//[MonoTODO]
		//protected override ControlCollection CreateControlsInstance()
		//{
		//		throw new NotImplementedException ();
		//}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override void DefWndProc (ref Message m)
		{
			window.DefWndProc (ref m);
		}

		//protected override void Dispose(bool disposing)
		//{
		//		throw new NotImplementedException ();
		//}

		protected virtual void OnClosed (EventArgs e)
		{
			if (Closed != null)
				Closed (this, e);
		}

		[MonoTODO]
		// CancelEventArgs not yet stubbed/implemented
		//protected virtual void  OnClosing(CancelEventArgs e)
		//{
		//		throw new NotImplementedException ();
		//}

		protected override void OnCreateControl ()
		{
			
		}

		protected override void OnFontChanged (EventArgs e)
		{

		}

		protected override void OnHandleCreated (EventArgs e)
		{

		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
		}

		protected virtual void OnInputLanguageChanged (InputLanguageChangedEventArgs e)
		{
			if (InputLanguageChanged != null)
				InputLanguageChanged (this, e);
		}

		protected virtual void OnInputLanguagedChanging (InputLanguageChangingEventArgs e)
		{
			if (InputLanguageChanging != null)
				InputLanguageChanging (this, e);
		}

		protected virtual void OnLoad (EventArgs e)
		{
			if (Load != null)
				Load (this, e);
		}

		protected virtual void OnMaximizedBoundsChanged (EventArgs e)
		{
			if (MaximizedBoundsChanged != null)
				MaximizedBoundsChanged (this, e);
		}

		protected virtual void OnMaximumSizeChanged (EventArgs e)
		{
			if (MaximumSizeChanged != null)
				MaximumSizeChanged (this, e);
		}

		protected virtual void OnMdiChildActivate (EventArgs e)
		{
			if (MdiChildActivate != null)
				MdiChildActivate (this, e);
		}

		protected virtual void OnMenuComplete (EventArgs e)
		{
			if (MenuComplete != null)
				MenuComplete (this, e);
		}

		protected virtual void OnMenuStart (EventArgs e)
		{
			if (MenuStart != null)
				MenuStart (this, e);
		}

		protected virtual void OnMinimumSizeChanged (EventArgs e)
		{

		}

		protected override void  OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
		}

		protected override void  OnResize (EventArgs e)
		{

		}

		protected override void  OnStyleChanged (EventArgs e)
		{
			base.OnStyleChanged (e);
		}

		protected override void  OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);
		}

		protected override void  OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);
		}

		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			return base.ProcessCmdKey (ref msg, keyData);
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			return base.ProcessDialogKey (keyData);
		}

		protected override bool ProcessKeyPreview (ref Message m)
		{
			return base.ProcessKeyPreview (ref m);
		}

		protected override bool ProcessTabKey (bool forward)
		{
			return base.ProcessTabKey (forward);
		}

		protected override void ScaleCore (float x, float y)
		{
			base.ScaleCore (x, y);
		}

		//public void Select(bool b1, bool b2)
		//{
		//		throw new NotImplementedException ();
		//}

		protected override void SetBoundsCore (
			int x, int y,  int width, int height,  
			BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void SetClientSizeCore (int x, int y)
		{
			base.SetClientSizeCore (x, y);
		}

		protected override void SetVisibleCore (bool value)
		{
			base.SetVisibleCore (value);
		}

		//protected void UpdateBounds()
		//{
		//		throw new NotImplementedException ();
		//}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);

			switch (m.Msg) {
			case Win32.WM_CLOSE:
				EventArgs closeArgs = new EventArgs();
				OnClosed (closeArgs);
				break;
				//case ?:
				//OnCreateControl()
				//break;
			case Win32.WM_FONTCHANGE:
				EventArgs fontChangedArgs = new EventArgs();
				OnFontChanged (fontChangedArgs);
				break;
			case Win32.WM_CREATE:
				EventArgs handleCreatedArgs = new EventArgs(); 
				OnHandleCreated (handleCreatedArgs);
				break;
			case Win32.WM_DESTROY:
				EventArgs destroyArgs = new EventArgs();
				OnHandleDestroyed (destroyArgs);
				break;
			case Win32.WM_INPUTLANGCHANGE:
				//InputLanguageChangedEventArgs ilChangedArgs =
				//	new InputLanguageChangedEventArgs();
				//OnInputLanguageChanged (ilChangedArgs);
				break;
			case Win32.WM_INPUTLANGCHANGEREQUEST:
				//InputLanguageChangingEventArgs ilChangingArgs =
				//	new InputLanguageChangingEventArgs();
				//OnInputLanguagedChanging (ilChangingArgs);
				break;
				/*
				  case Win32.WM_SHOWWINDOW:
				  EventArgs e;
				  OnLoad (e);
				  break;
				*/
				// case ?:
				// OnMaximizedBoundsChanged(EventArgs e)
				// break;
				// case ?:
				// OnMaximumSizedChanged(EventArgs e)
				//break;
			case Win32.WM_MDIACTIVATE:
				EventArgs mdiActivateArgs = new EventArgs();
				OnMdiChildActivate (mdiActivateArgs);
				break;
			case Win32.WM_EXITMENULOOP:
				EventArgs menuCompleteArgs = new EventArgs();
				OnMenuComplete (menuCompleteArgs);
				break;
			case Win32.WM_ENTERMENULOOP:
				EventArgs enterMenuLoopArgs = new EventArgs();
				OnMenuStart (enterMenuLoopArgs);
				break;
				// case ?:
				// OnMinimumSizeChanged(EventArgs e)
				// break;
			case Win32.WM_PAINT:
				//PaintEventArgs paintArgs = new PaintEventArgs();
				//OnPaint (paintArgs);
				break;
			case Win32.WM_SIZE:
				EventArgs resizeArgs = new EventArgs();
				OnResize (resizeArgs);
				break;
				//case ?:
				//OnStyleChanged(EventArgs e)
				//break;
			case Win32.WM_SETTEXT:
				EventArgs textChangedArgs = new EventArgs();
				OnTextChanged (textChangedArgs);
				break;
			case Win32.WM_SHOWWINDOW:
				EventArgs visibleChangedArgs = new EventArgs();
				OnVisibleChanged (visibleChangedArgs);
				break;
			}
		}

		//sub class
		//System.Windows.Forms.Form.ControlCollection.cs
		//
		//Author:
		//  stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//
		// (C) 2002 Ximian, Inc
		//
		//
		// <summary>
		//	This is only a template.  Nothing is implemented yet.
		//
		// </summary>
		// TODO: implement support classes and derive from 
		// proper classes
		public /*new(remove error)*/ class  ControlCollection /*: System.Windows.Forms.Control.ControlCollection, ICollection*/ {

			//  --- Constructor
			[MonoTODO]
			// base class not defined (yet!)
			public ControlCollection (Form owner) /*: base(owner)*/ {
				throw new NotImplementedException ();
			}
		
			//  --- Public Methods

			[MonoTODO]
			// TODO: see what causes this compile error
			//public override void Add(Control value) {
			//	throw new NotImplementedException ();
			//}

			[MonoTODO]
			public override bool Equals (object o) {
				throw new NotImplementedException ();
			}

			//public static bool Equals(object o1, object o2) {
			//	throw new NotImplementedException ();
			//}

			[MonoTODO]
			public override int GetHashCode () {
				//FIXME add our proprities
				return base.GetHashCode ();
			}

			// Inherited
			//public override int GetChildIndex(Control c) {
			//	throw new NotImplementedException ();
			//}

			//[MonoTODO]
			// Control not yet usuable due to dependencies
			//public override void Remove(Control value) {
			//	throw new NotImplementedException ();
			//}
		} // end of Subclass
	}
}
