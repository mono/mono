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
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class HTMLElement : Element, IElement
	{
		private nsIDOMHTMLElement element;

		public HTMLElement (WebBrowser control, nsIDOMHTMLElement domHtmlElement) : base (control, domHtmlElement as nsIDOMElement)
		{
			if (control.platform != control.enginePlatform)
				this.element = nsDOMHTMLElement.GetProxy (control, domHtmlElement);
			else
				this.element = domHtmlElement;
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

		#region IElement Members
		public override IElement AppendChild (IElement child) {
			nsIDOMNode newChild;
			HTMLElement elem = (HTMLElement) child;
			this.element.appendChild (elem.element, out newChild);
			return new HTMLElement (control, newChild as nsIDOMHTMLElement);
		}
		
		public override IElement InsertBefore (INode child, INode refChild) {
			nsIDOMNode newChild;
			Node elem = (Node) child;
			Node reference = (Node) refChild;
			this.element.insertBefore (elem.node, reference.node, out newChild);
			return new HTMLElement (control, newChild as nsIDOMHTMLElement);
		}		

		public new string InnerHTML
		{
			get {
				nsIDOMNSHTMLElement nsElem = this.element as nsIDOMNSHTMLElement;
				nsElem.getInnerHTML (storage);
				return Base.StringGet (storage);
			}
		}

		public override int GetHashCode () {
			return this.hashcode;
		}
		#endregion
	}
}
