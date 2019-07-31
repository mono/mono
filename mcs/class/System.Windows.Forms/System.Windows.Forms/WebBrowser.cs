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


using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Reflection;
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
		bool allowNavigation; // if this is true, no other navigation is allowed
		
		bool allowWebBrowserDrop = true;
		bool isWebBrowserContextMenuEnabled;
		object objectForScripting;
		bool webBrowserShortcutsEnabled;
		bool scrollbarsEnabled = true;
		
		WebBrowserReadyState readyState;

		HtmlDocument document;
		
		WebBrowserEncryptionLevel securityLevel;

		Stream data;
		bool isStreamSet;

		string url;

		#region Public Properties

		[DefaultValue(true)]
		public bool AllowNavigation {
			get { return allowNavigation; }
			set { allowNavigation = value; }
		}

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
					document = new HtmlDocument (this, this.WebHost);
				return document; 
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public Stream DocumentStream {
			get {
				if (WebHost.Document == null || WebHost.Document.DocumentElement == null)
					return null;

				return null; //WebHost.Document.DocumentElement.ContentStream;
			}
			set { 
				if (this.allowNavigation)
					return;

				this.Url = new Uri ("about:blank");

				data = value;
				isStreamSet = true;
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public string DocumentText {
			get { 
				if (WebHost.Document == null || WebHost.Document.DocumentElement == null)
					return String.Empty;
				return WebHost.Document.DocumentElement.OuterHTML;
			}
			set {
				if (WebHost.Document != null && WebHost.Document.DocumentElement != null)
					WebHost.Document.DocumentElement.OuterHTML = value;
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public string DocumentTitle {
			get {
				if (document != null)
					return document.Title;
				return String.Empty;
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public string DocumentType {
			get {
				if (document != null)
					return document.DocType;
				return String.Empty;
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public WebBrowserEncryptionLevel EncryptionLevel {
			get { return securityLevel; }
		}

		public override bool Focused {
			get { return base.Focused; }
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public bool IsBusy {
			get { return !documentReady; }
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public bool IsOffline {
			get { return WebHost.Offline; }
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

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public WebBrowserReadyState ReadyState {
			get { return readyState; }
		}

		[DefaultValue(false)]
		public bool ScriptErrorsSuppressed {
			get { return SuppressDialogs; }
			set { SuppressDialogs = value; }
		}
		
		[DefaultValue(true)]
		public bool ScrollBarsEnabled {
			get { return scrollbarsEnabled; }
			set {
				scrollbarsEnabled = value;
				if (document != null)
					SetScrollbars ();
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public virtual string StatusText {
			get { return base.status; }
		}

		[BindableAttribute(true)] 
		[DefaultValue(null)]
		[TypeConverter(typeof(WebBrowserUriTypeConverter))]
		public Uri Url {
			get {
				if (url != null)
					return new Uri (url);
				if (WebHost.Document != null && WebHost.Document.Url != null)
					return new Uri (WebHost.Document.Url);
				return null;
			}
			set {
				url = null;
				this.Navigate (value); 
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public Version Version {
			get { 
				Assembly ass = WebHost.GetType().Assembly;
				return ass.GetName().Version;
			}
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

		[MonoTODO ("WebBrowser control is only supported on Linux/Windows. No support for OSX.")]
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
			string url = "http://www.example.com";
			try {
				Microsoft.Win32.RegistryKey reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey (@"Software\Microsoft\Internet Explorer\Main\Search Page");
				if (reg != null) {
					object searchUrl = reg.GetValue ("Default_Search_URL");
					if (searchUrl != null && searchUrl is string) {
						Uri uri;
						if (System.Uri.TryCreate (searchUrl as string, UriKind.Absolute, out uri))
							url = uri.ToString ();
					}
				}
			} catch {
			}
			Navigate (url);
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

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

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

#pragma warning disable 0067
		[MonoTODO]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler PaddingChanged;
#pragma warning restore 0067

		#endregion

		#region Internal

		internal override bool OnNewWindowInternal ()
		{
			CancelEventArgs c = new CancelEventArgs ();
			OnNewWindow (c);
			return c.Cancel;
		}
		
		internal override void OnWebHostLoadStarted (object sender, Mono.WebBrowser.LoadStartedEventArgs e)
		{
			documentReady = false;
			document = null;
			readyState = WebBrowserReadyState.Loading;
			WebBrowserNavigatingEventArgs n = new WebBrowserNavigatingEventArgs (new Uri (e.Uri), e.FrameName);
			OnNavigating (n);
		}

		internal override void OnWebHostLoadCommited (object sender, Mono.WebBrowser.LoadCommitedEventArgs e)
		{
			readyState = WebBrowserReadyState.Loaded;
			url = e.Uri;
			SetScrollbars ();
			WebBrowserNavigatedEventArgs n = new WebBrowserNavigatedEventArgs (new Uri (e.Uri));
			OnNavigated (n);
		}
		internal override void OnWebHostProgressChanged (object sender, Mono.WebBrowser.ProgressChangedEventArgs e)
		{
			readyState = WebBrowserReadyState.Interactive;
			WebBrowserProgressChangedEventArgs n = new WebBrowserProgressChangedEventArgs (e.Progress, e.MaxProgress);
			OnProgressChanged (n);
		}

		internal override void OnWebHostLoadFinished (object sender, Mono.WebBrowser.LoadFinishedEventArgs e)
		{
			url = null;
			documentReady = true;
			readyState = WebBrowserReadyState.Complete;
			if (isStreamSet) {
				byte[] buffer = new byte [data.Length];
				long len = data.Length;
				int count = 0;
				data.Position = 0;
				do {
					count = data.Read (buffer, (int) data.Position, (int) (len - data.Position));
				} while (count > 0);
				WebHost.Render (buffer);
				data = null;
				isStreamSet = false;
			}
			SetScrollbars ();
			WebBrowserDocumentCompletedEventArgs n = new WebBrowserDocumentCompletedEventArgs (new Uri (e.Uri));
			OnDocumentCompleted (n);
		}
		
		internal override void OnWebHostSecurityChanged (object sender, Mono.WebBrowser.SecurityChangedEventArgs e)
		{
			switch (e.State) {
				case Mono.WebBrowser.SecurityLevel.Insecure:
					securityLevel = WebBrowserEncryptionLevel.Insecure;
				break;
				case Mono.WebBrowser.SecurityLevel.Mixed:
					securityLevel = WebBrowserEncryptionLevel.Mixed;
				break;
				case Mono.WebBrowser.SecurityLevel.Secure:
					securityLevel = WebBrowserEncryptionLevel.Bit56;
				break;
			}
		}
		
		internal override void OnWebHostContextMenuShown (object sender, Mono.WebBrowser.ContextMenuEventArgs e) {
			if (!isWebBrowserContextMenuEnabled)
				return;
					
            ContextMenu menu = new ContextMenu();
                        
			MenuItem item = new MenuItem("Back", delegate { 
				GoBack(); 
			});
			item.Enabled = this.CanGoBack;
			menu.MenuItems.Add (item);
			
			item = new MenuItem("Forward", delegate { 
				GoForward(); 
			});
			item.Enabled = this.CanGoForward;
			menu.MenuItems.Add (item);
			
			item = new MenuItem("Refresh", delegate { 
				Refresh (); 
			});
			menu.MenuItems.Add (item);
            
            menu.MenuItems.Add (new MenuItem ("-"));
            
            menu.Show(this, PointToClient(MousePosition));
		}

		internal override void OnWebHostStatusChanged (object sender, Mono.WebBrowser.StatusChangedEventArgs e) {
			base.status = e.Message;
			OnStatusTextChanged (null);
		}
		
		#endregion


		void SetScrollbars () {
			//if (!scrollbarsEnabled)
			//        WebHost.ExecuteScript ("document.body.style.overflow='hidden';");
			//else
			//        WebHost.ExecuteScript ("document.body.style.overflow='auto';");
		}

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
