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

#if NET_2_0

#undef debug

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	[MonoTODO ("Needs Implementation")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    [ComVisible(true)]
    public abstract class WebBrowserBase : Control
	{
		protected bool documentReady;

		#region Public Properties
		[MonoTODO ("Stub, not implemented")]
		[Browsable (false)] 
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Object ActiveXInstance {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO ("Stub, not implemented")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public override bool AllowDrop {
			get { return false; }
			set { throw new NotSupportedException (); }
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
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Cursor Cursor {
			get { return null; }
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool Enabled {
			get { return true; }
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Font Font {
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable (false)]
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
		[Localizable (true)]
		public new virtual RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set { base.RightToLeft = value; }
		}

		public override ISite Site {
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return String.Empty; }
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool UseWaitCursor {
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		#endregion

		#region Protected Properties
		protected override Size DefaultSize {
			get { return new Size (100, 100); }
		}
		#endregion

		#region Public Methods
		public new void DrawToBitmap (Bitmap bitmap, Rectangle targetBounds) 
		{
			throw new NotImplementedException ();
		}

		public new Control GetChildAtPoint (Point point)
		{
			return base.GetChildAtPoint (point);
		}

		public override bool  PreProcessMessage(ref Message msg)
		{
 			 return base.PreProcessMessage(ref msg);
		}

		#endregion

		#region Protected Virtual Methods
		protected virtual void AttachInterfaces (object nativeActiveXObject) 
		{
			throw new NotImplementedException ();
		}

		protected virtual void CreateSink ()
		{
			throw new NotImplementedException ();
		}

		protected virtual WebBrowserSiteBase CreateWebBrowserSiteBase ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void DetachInterfaces ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void DetachSink ()
		{
			throw new NotImplementedException ();
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
			get	{ return webHost; }
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
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
			webHost.Alert += new Mono.WebBrowser.AlertEventHandler (OnWebHostAlert);
			webHost.Completed += new EventHandler (OnWebHostCompleted);
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


		protected override void OnResize (EventArgs e)
		{

			base.OnResize (e);

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

		internal abstract bool OnNewWindowInternal ();
		internal abstract void OnWebHostCompleted (object sender, EventArgs e);
		#endregion
	
	}
}

#endif
