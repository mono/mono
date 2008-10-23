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

		internal nsIDOMDocument node {
			get { return base.node as nsIDOMDocument;}
			set { base.node = value as nsIDOMDocument; }
		}
		
		public Document (WebBrowser control, nsIDOMHTMLDocument document)
			: base (control, document)
		{
			if (control.platform != control.enginePlatform)
				this.node = nsDOMHTMLDocument.GetProxy (control, document);
			else
				this.node = document;
		}

		public Document (WebBrowser control, nsIDOMDocument document)
			: base (control, document)
		{
			if (control.platform != control.enginePlatform)
				this.node = nsDOMDocument.GetProxy (control, document);
			else
				this.node = document;
		}
		
		#region IDisposable Members
		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.resources.Clear ();
					this.node = null;
				}
			}
			base.Dispose (disposing);
		}
		#endregion

		#region Internal
		internal new nsIDOMDocument XPComObject
		{
			get { return this.node; }
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
				return (IElement)GetTypedNode (element);
//				if ((element as nsIDOMHTMLElement) != null)
//					return new HTMLElement (control, element as nsIDOMHTMLElement) as IElement;
//				return new Element (control, element) as IElement;
			}
		}

		public string ActiveLinkColor {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getALink(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(node is nsIDOMHTMLDocument))
					return;
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setALink(storage);
			}
		}
				
		public IElementCollection Anchors {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return null;
				nsIDOMHTMLCollection ret;
				((nsIDOMHTMLDocument)node).getAnchors (out ret);
				return new HTMLElementCollection(control, (nsIDOMNodeList)ret);
			}

		}

		public IElementCollection Applets {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return null;
				nsIDOMHTMLCollection ret;
				((nsIDOMHTMLDocument)node).getApplets (out ret);
				return new HTMLElementCollection(control, (nsIDOMNodeList)ret);
			}

		}

		public string Background {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getBackground(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(node is nsIDOMHTMLDocument))
					return;				
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setBackground(storage);
			}
		}

		public string BackColor {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getBgColor(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(node is nsIDOMHTMLDocument))
					return;				
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setBgColor(storage);
			}
		}

		public IElement Body {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return null;
				
				if (!resources.Contains ("Body")) {
					nsIDOMHTMLElement element;
					((nsIDOMHTMLDocument)node).getBody (out element);
					resources.Add ("Body", GetTypedNode (element));
				}
				return resources["Body"] as IElement;
			}
		}

		// This is the most ugly code *ever*. TODO: find out why a simple thing as getting the 
		// document encoding is not available in any frozen public interface
		public string Charset {
			get {		
				nsIDOMDocumentView docView = (nsIDOMDocumentView) this.node;
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
				nsIDOMDocumentView docView = (nsIDOMDocumentView) this.node;
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
				if (!(node is nsIDOMHTMLDocument))
					return String.Empty;
				((nsIDOMHTMLDocument)node).getCookie (storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(node is nsIDOMHTMLDocument))
					return;				
				Base.StringSet (storage, value);
				((nsIDOMHTMLDocument)node).setCookie (storage);
			}
		}
		public string Domain {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return String.Empty;
				((nsIDOMHTMLDocument)node).getDomain (storage);
				return Base.StringGet (storage);
			}
		}
		public IElement DocumentElement {
			get {
				if (!resources.Contains ("DocumentElement")) {
					nsIDOMElement element;
					this.node.getDocumentElement (out element);
					resources.Add ("DocumentElement", GetTypedNode (element));
				}
				return resources["DocumentElement"] as IElement;
			}
		}
		
		public IDocumentType DocType {
			get {
				nsIDOMDocumentType doctype;
				this.node.getDoctype (out doctype);
				return new DocumentType (this.control, doctype);
			}
		}
		

		public string ForeColor {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getText(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(node is nsIDOMHTMLDocument))
					return;
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setText(storage);
			}
		}

		public IElementCollection Forms {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return null;
				nsIDOMHTMLCollection ret;
				((nsIDOMHTMLDocument)node).getForms (out ret);
				return new HTMLElementCollection(control, (nsIDOMNodeList)ret);
			}

		}

		public IElementCollection Images {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return null;
				nsIDOMHTMLCollection ret;
				((nsIDOMHTMLDocument)node).getImages (out ret);
				return new HTMLElementCollection(control, (nsIDOMNodeList)ret);
			}

		}
		
		public IDOMImplementation Implementation {
			get {
				nsIDOMDOMImplementation implementation;
				node.getImplementation (out implementation);
				return new DOMImplementation (this.control, implementation);
			}
		}

		public string LinkColor {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getLink(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(node is nsIDOMHTMLDocument))
					return;
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setLink(storage);
			}
		}

		public IElementCollection Links {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return null;
				nsIDOMHTMLCollection ret;
				((nsIDOMHTMLDocument)node).getLinks (out ret);
				return new HTMLElementCollection(control, (nsIDOMNodeList)ret);
			}

		}
		
		public IStylesheetList Stylesheets
		{
			get {
				nsIDOMStyleSheetList styleList;
				nsIDOMDocumentStyle docStyle = ((nsIDOMDocumentStyle)this.node);
				docStyle.getStyleSheets (out styleList);
				return new StylesheetList (this.control, styleList);
			}
		}
		
		public string Title {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return String.Empty;
				((nsIDOMHTMLDocument)node).getTitle (storage);
				return Base.StringGet (storage);
			}
			set {
				Base.StringSet (storage, value);
				((nsIDOMHTMLDocument)node).setTitle (storage);
			}
		}

		public string Url {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return String.Empty;
				((nsIDOMHTMLDocument)node).getURL (storage);
				return Base.StringGet (storage);
			}
		}

		public string VisitedLinkColor {
			get {
				if (!(node is nsIDOMHTMLDocument))
					return String.Empty;
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).getVLink(storage);
				return Base.StringGet (storage);
			}
			set {
				if (!(node is nsIDOMHTMLDocument))
					return;
				Base.StringSet (storage, value);
				nsIDOMHTMLElement element;
				((nsIDOMHTMLDocument)node).getBody (out element);
				((nsIDOMHTMLBodyElement)element).setVLink(storage);
			}
		}
		
		public IWindow Window {
			get {
				nsIDOMDocumentView docView = (nsIDOMDocumentView) this.node;
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
			this.node.createAttribute (storage, out nsAttribute);
			return new Attribute (control, nsAttribute);			
		}

		public IElement CreateElement (string tagName)
		{
			nsIDOMElement nsElement;
			Base.StringSet (storage, tagName);
			this.node.createElement (storage, out nsElement);
			if (node is nsIDOMHTMLDocument)
				return new HTMLElement (control, (nsIDOMHTMLElement)nsElement);
			return new Element (control, nsElement);			
		}

		public IElement GetElementById (string id)
		{
			if (!resources.Contains ("GetElementById" + id)) {
				nsIDOMElement nsElement;
				Base.StringSet (storage, id);
				this.node.getElementById (storage, out nsElement);
				if (nsElement == null)
					return null;
				resources.Add ("GetElementById" + id, GetTypedNode (nsElement));
			}
			return resources["GetElementById" + id] as IElement;
		}

		public IElementCollection GetElementsByTagName (string name)
		{
			if (!resources.Contains ("GetElementsByTagName" + name)) {
				nsIDOMNodeList nodes;
				this.node.getElementsByTagName (storage, out nodes);
				if (nodes == null)
					return null;
				resources.Add ("GetElementsByTagName" + name, new HTMLElementCollection(control, nodes));
			}
			return resources["GetElementsByTagName" + name] as IElementCollection;
		}

		public IElement GetElement (int x, int y)
		{
			nsIDOMNodeList nodes;
			this.node.getChildNodes (out nodes);
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
		
		public void Write (string text) {
			if (!(node is nsIDOMHTMLDocument))
				return;
			Base.StringSet (storage, text);
			((nsIDOMHTMLDocument)node).write (storage);
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
			return this.node.GetHashCode ();
		}		
	}
}
