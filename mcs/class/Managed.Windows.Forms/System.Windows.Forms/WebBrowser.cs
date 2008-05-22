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

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace System.Windows.Forms
{
	[DefaultProperty ("Url")]
	[DefaultEvent ("DocumentCompleted")]
	[Docking (DockingBehavior.AutoDock)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	[Designer("System.Windows.Forms.Design.WebBrowserDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class WebBrowser : WebBrowserBase
	{
		private bool navigated; // flag indicating that at least one page has been loaded (besides the initial about:blank)
		private bool allowNavigation; // if this is true, and navigated is also true, no other navigation is allowed
		
		private bool allowWebBrowserDrop;
		private bool isWebBrowserContextMenuEnabled;
		private object objectForScripting;
		private bool scriptErrorsSuppressed;
		private bool scrollBarsEnabled;
		private string statusText;
		private bool webBrowserShortcutsEnabled;

		private HtmlDocument document;

		#region Public Properties

		[DefaultValue(true)]
		public bool AllowNavigation {
			get { return allowNavigation; }
			set { allowNavigation = value; }
		}

		[MonoTODO ("Stub, not implemented")]
		[DefaultValue (true)]
		public bool AllowWebBrowserDrop {
			get { return allowWebBrowserDrop; }
			set { allowWebBrowserDrop = value; }
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public bool CanGoBack {
			get { return this.WebHost.Navigation.CanGoBack; }
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public bool CanGoForward {
			get { return this.WebHost.Navigation.CanGoForward; }
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public HtmlDocument Document {
			get {
				if (document == null && documentReady)
					document = new HtmlDocument (this.WebHost);
				return document; 
			}
		}

		[MonoTODO ("Stub, not implemented")]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public Stream DocumentStream {
			get { return null; }
			set { 
				if (this.allowNavigation && this.navigated)
					return;
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public string DocumentText {
			get { 
				if (!this.navigated)
					return String.Empty; 
				return ((Mono.WebBrowser.DOM.IElement)WebHost.Document.FirstChild).OuterHTML;
			}
			set { 
				((Mono.WebBrowser.DOM.IElement)WebHost.Document.FirstChild).OuterHTML = value;
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public string DocumentTitle {
			get { return document.Title; }
			private set { document.Title = value; }
		}

		[MonoTODO ("Stub, not implemented")]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public string DocumentType {
			get { return String.Empty; }
		}

		[MonoTODO ("Stub, not implemented")]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public WebBrowserEncryptionLevel EncryptionLevel {
			get { return WebBrowserEncryptionLevel.Unknown; }
		}

		[MonoTODO ("Stub, not implemented")]
		public override bool Focused {
			get { return base.Focused; }
		}

		[MonoTODO ("Stub, not implemented")]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public bool IsBusy {
			get { return false; }
		}

		[MonoTODO ("Stub, not implemented")]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public bool IsOffline {
			get { return true; }
		}

		[MonoTODO ("Stub, not implemented")]
		[DefaultValue(true)]
		public bool IsWebBrowserContextMenuEnabled {
			get { return isWebBrowserContextMenuEnabled; }
			set { isWebBrowserContextMenuEnabled = value; }
		}

		[MonoTODO ("Stub, not implemented")]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public object ObjectForScripting {
			get { return objectForScripting; }
			set { objectForScripting = value; }
		}

		[MonoTODO ("Stub, not implemented")]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public WebBrowserReadyState ReadyState {
			get { return WebBrowserReadyState.Uninitialized; }
		}

		[MonoTODO ("Stub, not implemented")]
		[DefaultValue(false)]
		public bool ScriptErrorsSuppressed {
			get { return scriptErrorsSuppressed; }
			set { scriptErrorsSuppressed = value; }
		}

		[MonoTODO ("Stub, not implemented")]
		[DefaultValue(true)]
		public bool ScrollBarsEnabled {
			get { return scrollBarsEnabled; }
			set { scrollBarsEnabled = value; }
		}

		[MonoTODO ("Stub, not implemented")]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public virtual string StatusText {
			get { return statusText; }
		}

		[BindableAttribute(true)] 
		[DefaultValue(null)]
		[TypeConverter(typeof(WebBrowserUriTypeConverter))]
		public Uri Url {
			get { return new Uri(WebHost.Document.Url); }
			set { this.Navigate (value); }
		}

		[MonoTODO ("Stub, not implemented")]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public Version Version {
			get { return null; }
		}

		[MonoTODO ("Stub, not implemented")]
		[DefaultValue(true)]
		public bool WebBrowserShortcutsEnabled {
			get { return webBrowserShortcutsEnabled; }
			set { webBrowserShortcutsEnabled = value; }
		}
		
		protected override Size DefaultSize {
			get { return base.DefaultSize; }
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}
		
		#endregion

		[MonoTODO ("Stub, not implemented")]
		public WebBrowser ()
		{
		}

		#region Public Methods

		public bool GoBack ()
		{
			documentReady = false;
			document = null;
			return WebHost.Navigation.Back ();
		}

		public bool GoForward ()
		{
			documentReady = false;
			document = null;
			return WebHost.Navigation.Forward ();
		}

		public void GoHome ()
		{
			documentReady = false;
			document = null;
			WebHost.Navigation.Home ();
		}

		public void Navigate (string urlString)
		{
			documentReady = false;
			document = null;
			WebHost.Navigation.Go (urlString);
		}

		public void Navigate (Uri url)
		{
			documentReady = false;
			document = null;
			WebHost.Navigation.Go (url.ToString ());
		}

		public void Navigate (string urlString, bool newWindow)
		{
			documentReady = false;
			document = null;
			WebHost.Navigation.Go (urlString);
		}

		public void Navigate (string urlString, string targetFrameName)
		{
			documentReady = false;
			document = null;
			WebHost.Navigation.Go (urlString);
		}

		public void Navigate (Uri url, bool newWindow)
		{
			documentReady = false;
			document = null;
			WebHost.Navigation.Go (url.ToString ());
		}

		public void Navigate (Uri url, string targetFrameName)
		{
			documentReady = false;
			document = null;
			WebHost.Navigation.Go (url.ToString ());
		}

		public void Navigate (string urlString, string targetFrameName, byte[] postData, string additionalHeaders)
		{
			documentReady = false;
			document = null;
			WebHost.Navigation.Go (urlString);
		}

		public void Navigate (Uri url, string targetFrameName, byte[] postData, string additionalHeaders)
		{
			documentReady = false;
			document = null;
			WebHost.Navigation.Go (url.ToString ());
		}

		public override void Refresh ()
		{
			Refresh (WebBrowserRefreshOption.IfExpired);
		}

		public void Refresh (WebBrowserRefreshOption opt)
		{
			documentReady = false;
			document = null;
			switch (opt) {
				case WebBrowserRefreshOption.Normal:
					WebHost.Navigation.Reload (Mono.WebBrowser.ReloadOption.Proxy);
					break;
				case WebBrowserRefreshOption.IfExpired:
					WebHost.Navigation.Reload (Mono.WebBrowser.ReloadOption.None);
					break;
				case WebBrowserRefreshOption.Completely:
					WebHost.Navigation.Reload (Mono.WebBrowser.ReloadOption.Full);
					break;
			}
		}

		public void Stop ()
		{
			WebHost.Navigation.Stop ();
		}

		public void GoSearch ()
		{
			throw new NotImplementedException ();
		}

		public void Print ()
		{
			throw new NotImplementedException ();
		}

		public void ShowPageSetupDialog ()
		{
			throw new NotImplementedException ();
		}

		public void ShowPrintDialog()
		{
			throw new NotImplementedException ();
		}
		
		public void ShowPrintPreviewDialog()
		{
			throw new NotImplementedException ();
		}
		
		public void ShowPropertiesDialog()
		{
			throw new NotImplementedException ();
		}
		
		public void ShowSaveAsDialog()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Protected Overridden Methods

		[MonoTODO ("Stub, not implemented")]
		protected override void AttachInterfaces (object nativeActiveXObject)
		{
			base.AttachInterfaces (nativeActiveXObject);
		}

		[MonoTODO ("Stub, not implemented")]
		protected override void CreateSink ()
		{
			base.CreateSink ();
		}

		[MonoTODO ("Stub, not implemented")]
		protected override WebBrowserSiteBase CreateWebBrowserSiteBase ()
		{
			return base.CreateWebBrowserSiteBase ();
		}

		[MonoTODO ("Stub, not implemented")]
		protected override void DetachInterfaces ()
		{
			base.DetachInterfaces ();
		}

		[MonoTODO ("Stub, not implemented")]
		protected override void DetachSink ()
		{
			base.DetachSink ();
		}

		[MonoTODO ("Stub, not implemented")]
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		[MonoTODO ("Stub, not implemented")]
		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}

		#endregion

		#region OnXXX methods

		protected virtual void OnCanGoBackChanged(EventArgs e)
		{
			if (CanGoBackChanged != null)
				CanGoBackChanged (this, e);
		}

		protected virtual void OnCanGoForwardChanged(EventArgs e)
		{			
			if (CanGoForwardChanged != null)
				CanGoForwardChanged (this, e);
		}

		protected virtual void OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e)
		{
			Console.Error.WriteLine (Environment.StackTrace);
			if (DocumentCompleted != null)
				DocumentCompleted (this, e);
		}

		protected virtual void OnDocumentTitleChanged(EventArgs e)
		{
			if (DocumentTitleChanged != null)
				DocumentTitleChanged (this, e);
		}

		protected virtual void OnEncryptionLevelChanged(EventArgs e)
		{
			if (EncryptionLevelChanged != null)
				EncryptionLevelChanged (this, e);
		}

		protected virtual void OnFileDownload(EventArgs e)
		{
			if (FileDownload != null)
				FileDownload (this, e);
		}

		protected virtual void OnNavigated(WebBrowserNavigatedEventArgs e)
		{
			if (Navigated != null)
				Navigated (this, e);
		}

		protected virtual void OnNavigating(WebBrowserNavigatingEventArgs e)
		{
			if (Navigating != null)
				Navigating (this, e);
		}

		protected virtual void OnNewWindow(CancelEventArgs e)
		{
			if (NewWindow != null)
				NewWindow (this, e);
		}

		protected virtual void OnProgressChanged(WebBrowserProgressChangedEventArgs e)
		{
			if (ProgressChanged != null)
				ProgressChanged (this, e);
		}

		protected virtual void OnStatusTextChanged(EventArgs e)
		{
			if (StatusTextChanged != null)
				StatusTextChanged (this, e);
		}

		#endregion

		#region Events	
		[BrowsableAttribute(false)]
		public event EventHandler CanGoBackChanged;

		[BrowsableAttribute(false)]
		public event EventHandler CanGoForwardChanged;

		public event WebBrowserDocumentCompletedEventHandler DocumentCompleted;

		[BrowsableAttribute(false)]
		public event EventHandler DocumentTitleChanged;

		[BrowsableAttribute(false)]
		public event EventHandler EncryptionLevelChanged;

		public event EventHandler FileDownload;

		public event WebBrowserNavigatedEventHandler Navigated;

		public event WebBrowserNavigatingEventHandler Navigating;

		public event CancelEventHandler NewWindow;
		
		public event WebBrowserProgressChangedEventHandler ProgressChanged;

		[BrowsableAttribute(false)]
		public event EventHandler StatusTextChanged;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler PaddingChanged;
		#endregion

		#region Internal

		internal override bool OnNewWindowInternal ()
		{
			CancelEventArgs c = new CancelEventArgs ();
			OnNewWindow (c);
			return c.Cancel;
		}

		internal override void OnWebHostLoadFinished (object sender, Mono.WebBrowser.LoadFinishedEventArgs e)
		{
			documentReady = true;
			if (!this.navigated) {
				this.navigated = true;
				return;
			}
			
			WebBrowserDocumentCompletedEventArgs n = new WebBrowserDocumentCompletedEventArgs (new Uri (e.Uri));
			OnDocumentCompleted (n);
		}
		
		internal override void OnWebHostLoadStarted (object sender, Mono.WebBrowser.LoadStartedEventArgs e)
		{
			documentReady = false;
			document = null;
			if (!this.navigated)
				return;
			WebBrowserNavigatingEventArgs n = new WebBrowserNavigatingEventArgs (new Uri (e.Uri), e.FrameName);
			OnNavigating (n);
		}

		internal override void OnWebHostLoadCommited (object sender, Mono.WebBrowser.LoadCommitedEventArgs e)
		{
			if (!this.navigated)
				return;
			WebBrowserNavigatedEventArgs n = new WebBrowserNavigatedEventArgs (new Uri (e.Uri));
			OnNavigated (n);
		}
		internal override void OnWebHostProgressChanged (object sender, Mono.WebBrowser.ProgressChangedEventArgs e)
		{
			if (!this.navigated)
				return;
			WebBrowserProgressChangedEventArgs n = new WebBrowserProgressChangedEventArgs (e.Progress, e.MaxProgress);
			OnProgressChanged (n);
		}
		#endregion

		[MonoTODO ("Stub, not implemented")]
		[ComVisible (false)]
		protected class WebBrowserSite : WebBrowserSiteBase
		{
			[MonoTODO ("Stub, not implemented")]
			public WebBrowserSite (WebBrowser host)
				: base ()
			{
			}
		}
	}

	internal class WebBrowserUriTypeConverter : UriTypeConverter
	{
	}
}

#endif
