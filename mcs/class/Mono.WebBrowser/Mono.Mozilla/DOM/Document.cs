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
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class Document : Node, IDocument
	{
		internal nsIDOMDocument document;

		public Document (WebBrowser control, nsIDOMHTMLDocument document)
			: base (control, document)
		{
			if (control.platform != control.enginePlatform)
				this.document = nsDOMHTMLDocument.GetProxy (control, document);
			else
				this.document = document;
		}

		public Document (WebBrowser control, nsIDOMDocument document)
			: base (control, document)
		{
			if (control.platform != control.enginePlatform)
				this.document = nsDOMDocument.GetProxy (control, document);
			else
				this.document = document;
		}
		
		#region IDisposable Members
		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.resources.Clear ();
					this.document = null;
				}
			}
			base.Dispose (disposing);
		}
		#endregion

		#region Internal
		internal nsIDOMDocument ComObject
		{
			get { return document; }
		}
		#endregion

		#region IDocument Properties

		public IElement Active {
			get {
				nsIDOMElement element;
				nsIWebBrowserFocus webBrowserFocus = (nsIWebBrowserFocus) (control.navigation.navigation);
				if (webBrowserFocus == null)
					return null;
				webBrowserFocus.getFocusedElement (out element);
				if ((element as nsIDOMHTMLElement) != null)
					return new HTMLElement (control, element as nsIDOMHTMLElement) as IElement;
				return new Element (control, element) as IElement;
			}
		}

		public string ActiveLinkColor {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getALink(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(document is nsIDOMHTMLDocument))
					return;
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setALink(storage);
			}
		}
				
		public IElementCollection Anchors {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return null;
				nsIDOMHTMLCollection ret;
				((nsIDOMHTMLDocument)document).getAnchors (out ret);
				return new HTMLElementCollection(control, (nsIDOMNodeList)ret);
			}

		}

		public IElementCollection Applets {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return null;
				nsIDOMHTMLCollection ret;
				((nsIDOMHTMLDocument)document).getApplets (out ret);
				return new HTMLElementCollection(control, (nsIDOMNodeList)ret);
			}

		}

		public string Background {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getBackground(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(document is nsIDOMHTMLDocument))
					return;				
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setBackground(storage);
			}
		}

		public string BackColor {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getBgColor(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(document is nsIDOMHTMLDocument))
					return;				
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setBgColor(storage);
			}
		}

		public IElement Body {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return null;
				
				if (!resources.Contains ("Body")) {
					nsIDOMHTMLElement element;
					((nsIDOMHTMLDocument)document).getBody (out element);
					nsIDOMHTMLBodyElement b = element as nsIDOMHTMLBodyElement;
					resources.Add ("Body", new HTMLElement (control, b));
				}
				return resources["Body"] as IElement;
			}
		}

		// This is the most ugly code *ever*. TODO: find out why a simple thing as getting the 
		// document encoding is not available in any frozen public interface
		public string Charset {
			get {		
				nsIDOMDocumentView docView = (nsIDOMDocumentView) this.document;
				nsIDOMAbstractView abstractView;
				docView.getDefaultView (out abstractView);

				nsIInterfaceRequestor requestor = (nsIInterfaceRequestor) abstractView;

				IntPtr ret;				
				requestor.getInterface (typeof(nsIDocCharset).GUID, out ret);
				nsIDocCharset charset = (nsIDocCharset) Marshal.GetObjectForIUnknown (ret);

				StringBuilder s = new StringBuilder (30);
				IntPtr r = Marshal.StringToHGlobalUni (s.ToString ());
				charset.getCharset (ref r);

				return Marshal.PtrToStringAnsi (r);
			}
			set {
				nsIDOMDocumentView docView = (nsIDOMDocumentView) this.document;
				nsIDOMAbstractView abstractView;
				docView.getDefaultView (out abstractView);

				nsIInterfaceRequestor requestor = (nsIInterfaceRequestor) abstractView;

				IntPtr ret;
				requestor.getInterface (typeof (nsIDocCharset).GUID, out ret);
				nsIDocCharset charset = (nsIDocCharset) Marshal.GetTypedObjectForIUnknown (ret, typeof (nsIDocCharset));

				charset.setCharset (value);
				control.navigation.Go (this.Url, LoadFlags.CharsetChange);
			}
		}

		public string Cookie {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return String.Empty;
				((nsIDOMHTMLDocument)document).getCookie (storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(document is nsIDOMHTMLDocument))
					return;				
				Base.StringSet (storage, value);
				((nsIDOMHTMLDocument)document).setCookie (storage);
			}
		}
		public string Domain {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return String.Empty;
				((nsIDOMHTMLDocument)document).getDomain (storage);
				return Base.StringGet (storage);
			}
		}
		public IElement DocumentElement {
			get {
				if (!resources.Contains ("DocumentElement")) {
					nsIDOMElement element;
					this.document.getDocumentElement (out element);
					resources.Add ("DocumentElement", new Element (control, element));
				}
				return resources["DocumentElement"] as IElement;
			}
		}
		
		public IDocumentType DocType {
			get {
				nsIDOMDocumentType doctype;
				this.document.getDoctype (out doctype);
				return new DocumentType (this.control, doctype);
			}
		}
		

		public string ForeColor {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getText(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(document is nsIDOMHTMLDocument))
					return;
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setText(storage);
			}
		}

		public IElementCollection Forms {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return null;
				nsIDOMHTMLCollection ret;
				((nsIDOMHTMLDocument)document).getForms (out ret);
				return new HTMLElementCollection(control, (nsIDOMNodeList)ret);
			}

		}

		public IElementCollection Images {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return null;
				nsIDOMHTMLCollection ret;
				((nsIDOMHTMLDocument)document).getImages (out ret);
				return new HTMLElementCollection(control, (nsIDOMNodeList)ret);
			}

		}
		
		public IDOMImplementation Implementation {
			get {
				nsIDOMDOMImplementation implementation;
				document.getImplementation (out implementation);
				return new DOMImplementation (this.control, implementation);
			}
		}

		public string LinkColor {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getLink(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(document is nsIDOMHTMLDocument))
					return;
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setLink(storage);
			}
		}

		public IElementCollection Links {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return null;
				nsIDOMHTMLCollection ret;
				((nsIDOMHTMLDocument)document).getLinks (out ret);
				return new HTMLElementCollection(control, (nsIDOMNodeList)ret);
			}

		}
		
		public IStylesheetList Stylesheets
		{
			get {
				nsIDOMStyleSheetList styleList;
				nsIDOMDocumentStyle docStyle = ((nsIDOMDocumentStyle)this.document);
				docStyle.getStyleSheets (out styleList);
				return new StylesheetList (this.control, styleList);
			}
		}
		
		public string Title {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return String.Empty;
				((nsIDOMHTMLDocument)document).getTitle (storage);
				return Base.StringGet (storage);
			}
			set {
				Base.StringSet (storage, value);
				((nsIDOMHTMLDocument)document).setTitle (storage);
			}
		}

		public string Url {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return String.Empty;
				((nsIDOMHTMLDocument)document).getURL (storage);
				return Base.StringGet (storage);
			}
		}

		public string VisitedLinkColor {
			get {
				if (!(document is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getVLink(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(document is nsIDOMHTMLDocument))
					return;
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)document).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setVLink(storage);
			}
		}
		
		public IWindow Window {
			get {
				nsIDOMDocumentView docView = (nsIDOMDocumentView) this.document;
				nsIDOMAbstractView abstractView;
				docView.getDefaultView (out abstractView);
				nsIInterfaceRequestor requestor = (nsIInterfaceRequestor) abstractView;
				if (requestor == null)
					return null;
				IntPtr ret;				
				requestor.getInterface (typeof(nsIDOMWindow).GUID, out ret);
				nsIDOMWindow window = (nsIDOMWindow) Marshal.GetObjectForIUnknown (ret);
				return new Window (this.control, window);
			}
		}

		#endregion

		#region Public IDocument Methods
		public IAttribute CreateAttribute (string name)
		{
			nsIDOMAttr nsAttribute;
			Base.StringSet (storage, name);
			this.document.createAttribute (storage, out nsAttribute);
			return new Attribute (control, nsAttribute);			
		}

		public IElement CreateElement (string tagName)
		{
			nsIDOMElement nsElement;
			Base.StringSet (storage, tagName);
			this.document.createElement (storage, out nsElement);
			if (document is nsIDOMHTMLDocument)
				return new HTMLElement (control, (nsIDOMHTMLElement)nsElement);
			return new Element (control, nsElement);			
		}

		public IElement GetElementById (string id)
		{
			if (!resources.Contains ("GetElementById" + id)) {
				nsIDOMElement nsElement;
				Base.StringSet (storage, id);
				this.document.getElementById (storage, out nsElement);
				if (nsElement == null)
					return null;
				resources.Add ("GetElementById" + id, new HTMLElement (control, nsElement as nsIDOMHTMLElement));
			}
			return resources["GetElementById" + id] as IElement;
		}

		public IElementCollection GetElementsByTagName (string name)
		{
			if (!resources.Contains ("GetElementsByTagName" + name)) {
				nsIDOMNodeList nodes;
				this.document.getElementsByTagName (storage, out nodes);
				if (nodes == null)
					return null;
				resources.Add ("GetElementsByTagName" + name, new HTMLElementCollection(control, nodes));
			}
			return resources["GetElementsByTagName" + name] as IElementCollection;
		}

		public IElement GetElement (int x, int y)
		{
			nsIDOMNodeList nodes;
			this.document.getChildNodes (out nodes);
			HTMLElementCollection col = new HTMLElementCollection(control, nodes);
			IElement ret = null;
			foreach (Element el in col) {
				if (el.Left <= x && el.Top <= y &&
					el.Left + el.Width >= x && el.Top + el.Height >= y) {
					ret = el;
					break;
				}
			}
			return ret;
		}

		public bool Equals (IDocument obj) {
			Document doc = (Document) obj;
			return doc.document == this.document;
		}
		
		public void Write (string text) {
			if (!(document is nsIDOMHTMLDocument))
				return;
			Base.StringSet (storage, text);
			((nsIDOMHTMLDocument)document).write (storage);
		}

		public string InvokeScript (string script)
		{
			return Base.EvalScript (this.control, script);
		}
		
		#endregion
		
		#region Events
		private System.ComponentModel.EventHandlerList events;
		internal System.ComponentModel.EventHandlerList Events {
			get {
				if (events == null)
					events = new System.ComponentModel.EventHandlerList();

				return events;
			}
		}
		internal static object LoadStoppedEvent = new object ();
		public event EventHandler LoadStopped
		{
			add { Events.AddHandler (LoadStoppedEvent, value); }
			remove { Events.RemoveHandler (LoadStoppedEvent, value); }
		}
		#endregion
		
		
		public override int GetHashCode () {
			return this.document.GetHashCode ();
		}		
	}
}
