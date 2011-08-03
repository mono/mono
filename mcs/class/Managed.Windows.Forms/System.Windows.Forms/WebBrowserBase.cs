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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita	<avidigal@novell.com>

#undef debug

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using Mono.WebBrowser;

namespace System.Windows.Forms
{
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	[DefaultProperty ("Name")]
	[DefaultEvent ("Enter")]
	[Designer("System.Windows.Forms.Design.AxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class WebBrowserBase : Control
	{
		internal bool documentReady;
		private bool suppressDialogs;
		internal bool SuppressDialogs {
			get { return suppressDialogs; }
			set { 
				suppressDialogs = value;
				webHost.Alert -= new Mono.WebBrowser.AlertEventHandler (OnWebHostAlert);
				if (!suppressDialogs)
					webHost.Alert += new Mono.WebBrowser.AlertEventHandler (OnWebHostAlert);				
			}
		}
		
		protected string status;

		#region Public Properties

		[Browsable (false)] 
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Object ActiveXInstance {
			get { throw new NotSupportedException ("Retrieving a reference to an activex interface is not supported. Sorry."); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool AllowDrop {
			get { return base.AllowDrop; }
			set { base.AllowDrop = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Color BackColor {
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Cursor Cursor {
			get { return base.Cursor; }
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool Enabled {
			get { return base.Enabled; }
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Font Font {
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Localizable (false)]
		public new virtual RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set { base.RightToLeft = value; }
		}

		public override ISite Site {
			set { base.Site = value; }
		}

		[Bindable (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return String.Empty; }
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool UseWaitCursor {
			get { return base.UseWaitCursor; }
			set { throw new NotSupportedException (); }
		}

		#endregion

		#region Protected Properties

		protected override Size DefaultSize {
			get { return new Size (100, 100); }
		}

		#endregion

		#region Public Methods

		[EditorBrowsable (EditorBrowsableState.Never)]
		public new void DrawToBitmap (Bitmap bitmap, Rectangle targetBounds)
		{
			base.DrawToBitmap (bitmap, targetBounds);
		}

		public override bool  PreProcessMessage(ref Message msg)
		{
 			 return base.PreProcessMessage(ref msg);
		}

		#endregion

		#region Protected Virtual Methods

		protected virtual void AttachInterfaces (object nativeActiveXObject)
		{
			throw new NotSupportedException ("Retrieving a reference to an activex interface is not supported. Sorry.");
		}

		protected virtual void CreateSink ()
		{
			throw new NotSupportedException ("Retrieving a reference to an activex interface is not supported. Sorry.");
		}

		protected virtual WebBrowserSiteBase CreateWebBrowserSiteBase ()
		{
			throw new NotSupportedException ("Retrieving a reference to an activex interface is not supported. Sorry.");
		}

		protected virtual void DetachInterfaces ()
		{
			throw new NotSupportedException ("Retrieving a reference to an activex interface is not supported. Sorry.");
		}

		protected virtual void DetachSink ()
		{
			throw new NotSupportedException ("Retrieving a reference to an activex interface is not supported. Sorry.");
		}

		#endregion

		#region Protected Overriden Methods

		protected override void Dispose (bool disposing)
		{
			WebHost.Shutdown ();
			base.Dispose (disposing);
		}

		protected override bool IsInputChar (char charCode)
		{
			return base.IsInputChar (charCode);
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged (e);
		}

		protected override void OnGotFocus (EventArgs e)
		{
#if debug
			Console.Error.WriteLine ("WebBrowserBase: OnGotFocus");
#endif
			base.OnGotFocus (e);
//			WebHost.FocusIn (Mono.WebBrowser.FocusOption.FocusFirstElement);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnLostFocus (EventArgs e)
		{
#if debug
			Console.Error.WriteLine ("WebBrowserBase: OnLostFocus");
#endif
			base.OnLostFocus (e);
			WebHost.FocusOut ();
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}
		
		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged (e);
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);
			if (Visible && !Disposing && !IsDisposed && state == State.Loaded) {
				state = State.Active;
				webHost.Activate ();
			} else if (!Visible && state == State.Active) {
				state = State.Loaded;
				webHost.Deactivate ();
			}
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			return base.ProcessMnemonic (charCode);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}
		
		#endregion

		#region Internal Properties

		enum State
		{
			Unloaded,
			Loaded,
			Active
		}
		private State state;

		private Mono.WebBrowser.IWebBrowser webHost;
		internal Mono.WebBrowser.IWebBrowser WebHost {
			get { return webHost; }
		}

		internal override void SetBoundsCoreInternal (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCoreInternal (x, y, width, height, specified);
			this.webHost.Resize (width, height);
		}

		#endregion

		#region Internal Methods

		internal WebBrowserBase ()
		{
			webHost = Mono.WebBrowser.Manager.GetNewInstance ();
			bool loaded = webHost.Load (this.Handle, this.Width, this.Height);
			if (!loaded)
				return;
				
			state = State.Loaded;

			webHost.MouseClick += new Mono.WebBrowser.DOM.NodeEventHandler (OnWebHostMouseClick);
			webHost.Focus += new EventHandler (OnWebHostFocus);
			webHost.CreateNewWindow += new Mono.WebBrowser.CreateNewWindowEventHandler (OnWebHostCreateNewWindow);
			webHost.LoadStarted += new LoadStartedEventHandler (OnWebHostLoadStarted);
			webHost.LoadCommited += new LoadCommitedEventHandler (OnWebHostLoadCommited);
			webHost.ProgressChanged += new Mono.WebBrowser.ProgressChangedEventHandler (OnWebHostProgressChanged);
			webHost.LoadFinished += new LoadFinishedEventHandler (OnWebHostLoadFinished);
			
			if (!suppressDialogs)
				webHost.Alert += new Mono.WebBrowser.AlertEventHandler (OnWebHostAlert);

			webHost.StatusChanged += new StatusChangedEventHandler (OnWebHostStatusChanged);
			
			webHost.SecurityChanged += new SecurityChangedEventHandler (OnWebHostSecurityChanged);
			webHost.ContextMenuShown += new ContextMenuEventHandler (OnWebHostContextMenuShown);
		}

		void OnWebHostAlert (object sender, Mono.WebBrowser.AlertEventArgs e)
		{
			switch (e.Type) {
				case Mono.WebBrowser.DialogType.Alert:
					MessageBox.Show (e.Text, e.Title);
					break;
				case Mono.WebBrowser.DialogType.AlertCheck:
					WebBrowserDialogs.AlertCheck form1 = new WebBrowserDialogs.AlertCheck (e.Title, e.Text, e.CheckMessage, e.CheckState);
					form1.Show ();
					e.CheckState = form1.Checked;
					e.BoolReturn = true;
					break;
				case Mono.WebBrowser.DialogType.Confirm:
					DialogResult r1 = MessageBox.Show (e.Text, e.Title, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
					e.BoolReturn = (r1 == DialogResult.OK);
					break;
				case Mono.WebBrowser.DialogType.ConfirmCheck:
					WebBrowserDialogs.ConfirmCheck form2 = new WebBrowserDialogs.ConfirmCheck (e.Title, e.Text, e.CheckMessage, e.CheckState);
					DialogResult r2 = form2.Show ();
					e.CheckState = form2.Checked;
					e.BoolReturn = (r2 == DialogResult.OK);
					break;
				case Mono.WebBrowser.DialogType.ConfirmEx:
					MessageBox.Show (e.Text, e.Title);
					break;
				case Mono.WebBrowser.DialogType.Prompt:
					WebBrowserDialogs.Prompt form4 = new WebBrowserDialogs.Prompt (e.Title, e.Text, e.Text2);
					DialogResult r4 = form4.Show ();
					e.StringReturn = form4.Text;
					e.BoolReturn = (r4 == DialogResult.OK);
					break;
				case Mono.WebBrowser.DialogType.PromptPassword:
					MessageBox.Show (e.Text, e.Title);
					break;
				case Mono.WebBrowser.DialogType.PromptUsernamePassword:
					MessageBox.Show (e.Text, e.Title);
					break;
				case Mono.WebBrowser.DialogType.Select:
					MessageBox.Show (e.Text, e.Title);
					break;
			}
			
		}

		#region Events raised by the embedded web browser

		bool OnWebHostCreateNewWindow (object sender, Mono.WebBrowser.CreateNewWindowEventArgs e)
		{
			return OnNewWindowInternal ();
		}

		internal override void OnResizeInternal (EventArgs e)
		{
			base.OnResizeInternal (e);

			if (state == State.Active)
				this.webHost.Resize (this.Width, this.Height);
		}

		private void OnWebHostMouseClick (object sender, EventArgs e)
		{
			//MessageBox.Show ("clicked");
		}

		/// <summary>
		/// Event raised from the embedded webbrowser control, saying that it has received focus
		/// (via a mouse click, for instance).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnWebHostFocus (object sender, EventArgs e)
		{
#if debug
			Console.Error.WriteLine ("WebBrowserBase: OnWebHostFocus");
#endif
			this.Focus ();
		}
		
		#endregion

		internal virtual bool OnNewWindowInternal ()
		{
			return false;
		}

		internal virtual void OnWebHostLoadStarted (object sender, LoadStartedEventArgs e)
		{
		}

		internal virtual void OnWebHostLoadCommited (object sender, LoadCommitedEventArgs e)
		{
		}
		internal virtual void OnWebHostProgressChanged (object sender, Mono.WebBrowser.ProgressChangedEventArgs e)
		{
		}
		internal virtual void OnWebHostLoadFinished (object sender, LoadFinishedEventArgs e)
		{
		}
		
		internal virtual void OnWebHostSecurityChanged (object sender, SecurityChangedEventArgs e)
		{
		}
		
		internal virtual void OnWebHostContextMenuShown (object sender, ContextMenuEventArgs e) {
		}

		internal virtual void OnWebHostStatusChanged (object sender, StatusChangedEventArgs e) {
		}

		#endregion

		#region Events
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add { throw new NotSupportedException ("Invalid event handler for BackColorChanged"); }
			remove { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { throw new NotSupportedException ("Invalid event handler for BackgroundImageChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { throw new NotSupportedException ("Invalid event handler for BackgroundImageLayoutChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BindingContextChanged {
			add { throw new NotSupportedException ("Invalid event handler for BindingContextChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event UICuesEventHandler ChangeUICues {
			add { throw new NotSupportedException ("Invalid event handler for ChangeUICues"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Click {
			add { throw new NotSupportedException ("Invalid event handler for Click"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler CursorChanged {
			add { throw new NotSupportedException ("Invalid event handler for CursorChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add { throw new NotSupportedException ("Invalid event handler for DoubleClick"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event DragEventHandler DragDrop {
			add { throw new NotSupportedException ("Invalid event handler for DragDrop"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event DragEventHandler DragEnter {
			add { throw new NotSupportedException ("Invalid event handler for DragEnter"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DragLeave {
			add { throw new NotSupportedException ("Invalid event handler for DragLeave"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event DragEventHandler DragOver {
			add { throw new NotSupportedException ("Invalid event handler for DragOver"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler EnabledChanged {
			add { throw new NotSupportedException ("Invalid event handler for EnabledChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Enter {
			add { throw new NotSupportedException ("Invalid event handler for Enter"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler FontChanged {
			add { throw new NotSupportedException ("Invalid event handler for FontChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { throw new NotSupportedException ("Invalid event handler for ForeColorChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event GiveFeedbackEventHandler GiveFeedback {
			add { throw new NotSupportedException ("Invalid event handler for GiveFeedback"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event HelpEventHandler HelpRequested {
			add { throw new NotSupportedException ("Invalid event handler for HelpRequested"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { throw new NotSupportedException ("Invalid event handler for ImeModeChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown {
			add { throw new NotSupportedException ("Invalid event handler for KeyDown"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress {
			add { throw new NotSupportedException ("Invalid event handler for KeyPress"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp {
			add { throw new NotSupportedException ("Invalid event handler for KeyUp"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event LayoutEventHandler Layout {
			add { throw new NotSupportedException ("Invalid event handler for Layout"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Leave {
			add { throw new NotSupportedException ("Invalid event handler for Leave"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseCaptureChanged {
			add { throw new NotSupportedException ("Invalid event handler for MouseCaptureChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseClick {
			add { throw new NotSupportedException ("Invalid event handler for MouseClick"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDoubleClick {
			add { throw new NotSupportedException ("Invalid event handler for MouseDoubleClick"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDown {
			add { throw new NotSupportedException ("Invalid event handler for MouseDown"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseEnter {
			add { throw new NotSupportedException ("Invalid event handler for MouseEnter"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseHover {
			add { throw new NotSupportedException ("Invalid event handler for MouseHover"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseLeave {
			add { throw new NotSupportedException ("Invalid event handler for MouseLeave"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseMove {
			add { throw new NotSupportedException ("Invalid event handler for MouseMove"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseUp {
			add { throw new NotSupportedException ("Invalid event handler for MouseUp"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseWheel {
			add { throw new NotSupportedException ("Invalid event handler for MouseWheel"); }
			remove { }
		}
		
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { throw new NotSupportedException ("Invalid event handler for Paint"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp {
			add { throw new NotSupportedException ("Invalid event handler for QueryAccessibilityHelp"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event QueryContinueDragEventHandler QueryContinueDrag {
			add { throw new NotSupportedException ("Invalid event handler for QueryContinueDrag"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftChanged {
			add { throw new NotSupportedException ("Invalid event handler for RightToLeftChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler StyleChanged {
			add { throw new NotSupportedException ("Invalid event handler for StyleChanged"); }
			remove { }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { throw new NotSupportedException ("Invalid event handler for TextChanged"); }
			remove { }
		}

		#endregion
	}
}
