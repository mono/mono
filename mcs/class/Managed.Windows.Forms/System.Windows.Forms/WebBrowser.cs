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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita	<avidigal@novell.com>

#if NET_2_0

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace System.Windows.Forms
{
	[MonoTODO ("Needs Implementation")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
    [ComVisible(true)]
    [Designer("System.Windows.Forms.Design.WebBrowserBaseDesigner, " + Consts.AssemblySystem_Design, 
		"System.ComponentModel.Design.IDesigner")]
    public class WebBrowser : WebBrowserBase
    {
		private bool allowNavigation;
		private bool allowWebBrowserDrop;
		private bool canGoBack;
		private bool canGoForward;
		private bool isWebBrowserContextMenuEnabled;
		private object objectForScripting;
		private bool scriptErrorsSuppressed;
		private bool scrollBarsEnabled;
		private string statusText;
		private bool webBrowserShortcutsEnabled;

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

		[DefaultValue (true)]
		public bool CanGoBack {
			get { return canGoBack; }
		}

		[DefaultValue (true)]
		public bool CanGoForward {
			get { return canGoForward; }
		}

		public HtmlDocument Document {
			get { throw new NotImplementedException (); }
		}

		public Stream DocumentStream {
			get { return null; }
			set { throw new NotSupportedException (); }
		}
		
		public string DocumentText {
			get { return String.Empty; }
			set { throw new NotSupportedException (); }
		}

		public string DocumentTitle {
			get { return String.Empty; }
			set { throw new NotSupportedException (); }
		}

		public string DocumentType {
			get { return String.Empty; }
			set { throw new NotSupportedException (); }
		}

		public WebBrowserEncryptionLevel EncryptionLevel {
			get { return WebBrowserEncryptionLevel.Unknown; }
		}

		public override bool Focused {
			get { return base.Focused; }
		}

		public bool IsBusy {
			get { return false; }
		}

		public bool IsOffline {
			get { return true; }
		}

		public bool IsWebBrowserContextMenuEnabled { 
			get { return isWebBrowserContextMenuEnabled; } 
			set { isWebBrowserContextMenuEnabled = value; } 
		}

		public object ObjectForScripting {
			get { return objectForScripting; }
			set { objectForScripting = value; }
		}

		public WebBrowserReadyState ReadyState { 
			get { return WebBrowserReadyState.Uninitialized; }
		}

		public bool ScriptErrorsSuppressed {
			get { return scriptErrorsSuppressed; }
			set { scriptErrorsSuppressed = value; }
		}

		public bool ScrollBarsEnabled {
			get { return scrollBarsEnabled; }
			set { scrollBarsEnabled = value; }
		}

		public virtual string StatusText {
			get { return statusText; }
			set { statusText = value; }
		}

		[BindableAttribute(true)] 
		public Uri Url {
			get { return null; }
			set { throw new NotSupportedException (); }
		}
		
		public Version Version {
			get { return null; }
		}

		public bool WebBrowserShortcutsEnabled {
			get { return webBrowserShortcutsEnabled; }
			set { webBrowserShortcutsEnabled = value; }
		}

		#endregion

		public WebBrowser ()
		{
		}

		#region Public Methods

		public bool GoBack ()
		{
			throw new NotImplementedException ();
		}

		public bool GoForward ()
		{
			throw new NotImplementedException ();
		}

		public void GoHome ()
		{
			throw new NotImplementedException ();
		}

		public void GoSearch ()
		{
			throw new NotImplementedException ();
		}

		public void Navigate (string urlString)
		{
			throw new NotImplementedException ();
		}

		public void Navigate (Uri url)
		{
			throw new NotImplementedException ();
		}

		public void Navigate (string urlString, bool newWindow)
		{
			throw new NotImplementedException ();
		}

		public void Navigate (string urlString, string targetFrameName)
		{
			throw new NotImplementedException ();
		}

		public void Navigate (Uri url, bool newWindow)
		{
			throw new NotImplementedException ();
		}

		public void Navigate (Uri url, string targetFrameName)
		{
			throw new NotImplementedException ();
		}

		public void Navigate (string urlString, string targetFrameName, byte[] postData, string additionalHeaders)
		{
			throw new NotImplementedException ();
		}

		public void Navigate (Uri url, string targetFrameName, byte[] postData, string additionalHeaders)
		{
			throw new NotImplementedException ();
		}

		public void Print ()
		{
			throw new NotImplementedException ();
		}

		public override void Refresh ()
		{
			base.Refresh ();
		}

		public void Refresh (WebBrowserRefreshOption opt)
		{
			throw new NotImplementedException ();
		}

		public void ShowPageSetupDialog()
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

		public void Stop()
		{
			throw new NotImplementedException ();
		}

		#endregion


		#region Protected Overridden Methods
		protected override void AttachInterfaces (object nativeActiveXObject)
		{
			base.AttachInterfaces (nativeActiveXObject);
		}

		protected override void CreateSink ()
		{
			base.CreateSink ();
		}

		protected override WebBrowserSiteBase CreateWebBrowserSiteBase ()
		{
			return base.CreateWebBrowserSiteBase ();
		}

		protected override void DetachInterfaces ()
		{
			base.DetachInterfaces ();
		}

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
			EventHandler eh = (EventHandler)(Events [CanGoBackChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCanGoForwardChanged(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [CanGoForwardChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e)
		{
			WebBrowserDocumentCompletedEventHandler eh = (WebBrowserDocumentCompletedEventHandler)(Events [DocumentCompletedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDocumentTitleChanged(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DocumentTitleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnEncryptionLevelChanged(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [EncryptionLevelChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnFileDownload(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [FileDownloadEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnNavigated(WebBrowserNavigatedEventArgs e)
		{
			WebBrowserNavigatedEventHandler eh = (WebBrowserNavigatedEventHandler)(Events [NavigatedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnNavigating(WebBrowserNavigatingEventArgs e)
		{
			WebBrowserNavigatingEventHandler eh = (WebBrowserNavigatingEventHandler)(Events [NavigatingEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnNewWindow(CancelEventArgs e)
		{
			CancelEventHandler eh = (CancelEventHandler)(Events [NewWindowEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnProgressChanged(WebBrowserProgressChangedEventArgs e)
		{
			WebBrowserProgressChangedEventHandler eh = (WebBrowserProgressChangedEventHandler)(Events [ProgressChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnStatusTextChanged(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [StatusTextChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
		#endregion

		#region Events
		static object CanGoBackChangedEvent = new object ();
		static object CanGoForwardChangedEvent = new object ();
		static object DocumentCompletedEvent = new object ();
		static object DocumentTitleChangedEvent = new object ();
		static object EncryptionLevelChangedEvent = new object ();
		static object FileDownloadEvent = new object ();
		static object NavigatedEvent = new object ();
		static object NavigatingEvent = new object ();
		static object NewWindowEvent = new object ();
		static object ProgressChangedEvent = new object ();
		static object StatusTextChangedEvent = new object ();
		
		public event EventHandler CanGoBackChanged {
			add { Events.AddHandler (CanGoBackChangedEvent, value); }
			remove { Events.RemoveHandler (CanGoBackChangedEvent, value); }
		}

		public event EventHandler CanGoForwardChanged {
			add { Events.AddHandler (CanGoForwardChangedEvent, value); }
			remove { Events.RemoveHandler (CanGoForwardChangedEvent, value); }
		}

		public event WebBrowserDocumentCompletedEventHandler DocumentCompleted {
			add { Events.AddHandler (DocumentCompletedEvent, value); }
			remove { Events.RemoveHandler (DocumentCompletedEvent, value); }
		}

		public event EventHandler DocumentTitleChanged {
			add { Events.AddHandler (DocumentTitleChangedEvent, value); }
			remove { Events.RemoveHandler (DocumentTitleChangedEvent, value); }
		}

		public event EventHandler EncryptionLevelChanged {
			add { Events.AddHandler (EncryptionLevelChangedEvent, value); }
			remove { Events.RemoveHandler (EncryptionLevelChangedEvent, value); }
		}

		public event EventHandler FileDownload {
			add { Events.AddHandler (FileDownloadEvent, value); }
			remove { Events.RemoveHandler (FileDownloadEvent, value); }
		}

		public event WebBrowserNavigatedEventHandler Navigated {
			add { Events.AddHandler (NavigatedEvent, value); }
			remove { Events.RemoveHandler (NavigatedEvent, value); }
		}

		public event WebBrowserNavigatingEventHandler Navigating {
			add { Events.AddHandler (NavigatingEvent, value); }
			remove { Events.RemoveHandler (NavigatingEvent, value); }
		}

		public event CancelEventHandler NewWindow {
			add { Events.AddHandler (NewWindowEvent, value); }
			remove { Events.RemoveHandler (NewWindowEvent, value); }
		}
		
		public event WebBrowserProgressChangedEventHandler ProgressChanged {
			add { Events.AddHandler (ProgressChangedEvent, value); }
			remove { Events.RemoveHandler (ProgressChangedEvent, value); }
		}

		public event EventHandler StatusTextChanged {
			add { Events.AddHandler (StatusTextChangedEvent, value); }
			remove { Events.RemoveHandler (StatusTextChangedEvent, value); }
		}
		#endregion


		protected class WebBrowserSite : WebBrowserSiteBase
		{
			public WebBrowserSite (WebBrowser host)
				: base ()
			{
			}

		}
    }
}

#endif