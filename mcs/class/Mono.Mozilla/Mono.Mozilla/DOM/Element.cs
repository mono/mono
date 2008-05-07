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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class Element : Node, IElement
	{
		internal nsIDOMElement element {
			get { return node as nsIDOMElement;}
			set { base.node = value as nsIDOMNode; }
		}
		
		public Element(WebBrowser control, nsIDOMElement domElement) : base (control, domElement as nsIDOMNode)
		{
			this.element = domElement;
		}

		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.element = null;
				}
			}
			base.Dispose(disposing);
		}		
		#endregion

		#region Properties
		public virtual IElement AppendChild (IElement child) {
			nsIDOMNode newChild;
			Element elem = (Element) child;
			this.element.appendChild (elem.element, out newChild);
			return new Element (control, newChild as nsIDOMElement);
		}

		public virtual string InnerText
		{
			get
			{
				nsIDOMDocumentRange docRange = ((Document) control.Document).ComObject as nsIDOMDocumentRange;
				nsIDOMRange range;
				docRange.createRange (out range);
				range.selectNodeContents (this.element);
				range.toString (storage);
				return Base.StringGet (storage);
			}
			set
			{
				Base.StringSet (storage, value);
				this.element.setNodeValue (storage);
			}
		}
		
		public virtual string OuterText
		{
			get
			{
				nsIDOMDocumentRange docRange = ((Document) control.Document).ComObject as nsIDOMDocumentRange;
				nsIDOMRange range;
				docRange.createRange (out range);
				nsIDOMNode parent;
				element.getParentNode (out parent);
				range.selectNodeContents (parent);
				range.toString (storage);
				return Base.StringGet (storage);
			}
			set
			{
				Base.StringSet (storage, value);
				nsIDOMNode parent;
				element.getParentNode (out parent);
				parent.setNodeValue (storage);
			}
		}		

		public virtual string InnerHTML	{
			get { return String.Empty; }
			set { }
		}
		
		public virtual string OuterHTML	{
			get { return String.Empty; }
			set {}
		}		

		public IElementCollection All {
			get
			{
				if (!resources.Contains ("All")) {
				
					HTMLElementCollection col = new HTMLElementCollection (control);
					Recurse (col, this.element); 
					resources.Add ("All", col);
				}
				return resources["All"] as IElementCollection;
			}
		}
		
		private void Recurse (HTMLElementCollection col, nsIDOMNode parent) {			
			nsIDOMNodeList children;
			parent.getChildNodes (out children);
			uint count;
			children.getLength (out count);

			for (int i = 0; i < count;i++) {
				nsIDOMNode node;
				children.item ((uint)i, out node);
				ushort type;
				node.getNodeType (out type);
				if (type == (ushort)NodeType.Element) {
					col.Add (new HTMLElement (control, (nsIDOMHTMLElement)node));
					Recurse (col, node);
				}
			}
		}


		public IElementCollection Children {
			get
			{
				if (!resources.Contains ("Children")) {
					nsIDOMNodeList children;
					this.element.getChildNodes (out children);
					resources.Add ("Children", new HTMLElementCollection (control, children));
				}
				return resources["Children"] as IElementCollection;
			}
		}


		public virtual string TagName {
			get {
				element.getTagName (storage);
				return Base.StringGet (storage);
			}
		}

		public virtual bool Disabled {
			get { return false; }
			set { }
				
		}

		public virtual int ClientWidth { 
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				int ret = 0;
				e.getClientWidth (out ret);
				return ret;
			}
		}
		public virtual int ClientHeight	{
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				int ret = 0;
				e.getClientHeight (out ret);
				return ret;
			}
		}
		public virtual int ScrollHeight	{
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				int ret = 0;
				e.getScrollHeight(out ret);
				return ret;
			}
		}
		public virtual int ScrollWidth	{
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				int ret = 0;
				e.getScrollWidth (out ret);
				return ret;
			}
		}
		public virtual int ScrollLeft {
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				int ret = 0;
				e.getScrollLeft (out ret);
				return ret;
			}
			set {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				e.setScrollLeft (value);
			}
		}
		public virtual int ScrollTop {
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				int ret = 0;
				e.getScrollTop (out ret);
				return ret;
			}
			set {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				e.setScrollTop (value);
			}
		}
		public virtual int OffsetHeight {
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				int ret = 0;
				e.getOffsetHeight (out ret);
				return ret;
			}
		}
		public virtual int OffsetWidth {
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				int ret = 0;
				e.getOffsetWidth (out ret);
				return ret;
			}
		}
		public virtual int OffsetLeft {
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				int ret = 0;
				e.getOffsetLeft (out ret);
				return ret;
			}
		}
		public virtual int OffsetTop {
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				int ret = 0;
				e.getOffsetTop (out ret);
				return ret;
			}
		}

		public virtual IElement OffsetParent {
			get {
				nsIDOMNSHTMLElement e = this.element as nsIDOMNSHTMLElement;
				nsIDOMElement ret;
				e.getOffsetParent (out ret);
				if ((ret as nsIDOMHTMLElement) != null)
					return new HTMLElement (this.control, ret as nsIDOMHTMLElement);
				return new Element (this.control, ret);

			}
		}

		#endregion
		
		#region Methods

		public bool Equals (IElement obj) {
			Element doc = (Element) obj;
			return doc.element == this.element;
		}

		public IElementCollection GetElementsByTagName (string name)
		{
			if (!resources.Contains ("GetElementsByTagName" + name)) {
				nsIDOMNodeList nodes;
				this.element.getElementsByTagName (storage, out nodes);
				resources.Add ("GetElementsByTagName" + name, new HTMLElementCollection(control, nodes));
			}
			return resources["GetElementsByTagName" + name] as IElementCollection;
		}
		
		public override int GetHashCode () {
			return this.hashcode;
		}

		public virtual bool HasAttribute (string name)
		{
			bool ret;
			Base.StringSet (storage, name);
			element.hasAttribute (storage, out ret);
			return ret;
		}

		public virtual string GetAttribute (string name) {
			UniString ret = new UniString (String.Empty);
			Base.StringSet (storage, name);
			element.getAttribute (storage, ret.Handle);
			return ret.ToString ();
		}

		public virtual void SetAttribute (string name, string value) {
			UniString strVal = new UniString (value);
			Base.StringSet (storage, name);
			element.setAttribute (storage, strVal.Handle);
		}		
		
		#endregion
		
		internal int Top {
			get {
				int ret;
				((nsIDOMNSHTMLElement)this.element).getOffsetTop (out ret);
				return ret;
			}
		}

		internal int Left {
			get {
				int ret;
				((nsIDOMNSHTMLElement)this.element).getOffsetLeft (out ret);
				return ret;
			}
		}
	
		internal int Width {
			get {
				int ret;
				((nsIDOMNSHTMLElement)this.element).getOffsetWidth (out ret);
				return ret;
			}
		}

		internal int Height {
			get {
				int ret;
				((nsIDOMNSHTMLElement)this.element).getOffsetHeight (out ret);
				return ret;
			}
		}		
	}
}
