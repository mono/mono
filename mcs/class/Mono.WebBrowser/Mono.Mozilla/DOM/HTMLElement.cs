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
using io=System.IO;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class HTMLElement : Element, IElement
	{
		protected nsIDOMHTMLElement node {
			get { return base.node as nsIDOMHTMLElement; }
			set { base.node = value as nsIDOMElement; }
		}

		public HTMLElement (WebBrowser control, nsIDOMHTMLElement domHtmlElement) : base (control, domHtmlElement as nsIDOMElement)
		{
			this.node = domHtmlElement;
		}

		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.node = null;
				}
			}
			base.Dispose(disposing);
		}		
		#endregion

		#region IElement Members
		public new string InnerHTML
		{
			get {
				nsIDOMNSHTMLElement nsElem = this.node as nsIDOMNSHTMLElement;
				if (nsElem == null)
					return null;
				nsElem.getInnerHTML (storage);
				return Base.StringGet (storage);
			}
			set {
				nsIDOMNSHTMLElement nsElem = this.node as nsIDOMNSHTMLElement;
				if (nsElem == null)
					return;
				Base.StringSet (storage, value);
				nsElem.setInnerHTML (storage);
			}
		}

		public override string OuterHTML
		{
			// bad emulation of outerHTML since gecko doesn't support it :P
			get {
				try {
					control.DocEncoder.Flags = DocumentEncoderFlags.OutputRaw;
					if (this.Equals (Owner.DocumentElement))
						return control.DocEncoder.EncodeToString ((Document) Owner);
					return control.DocEncoder.EncodeToString (this);
				} catch {
					string tag = this.TagName;
					string str = "<" + tag;
					foreach (IAttribute att in this.Attributes) {
						str += " " + att.Name + "=\"" + att.Value + "\"";
					}
					nsIDOMNSHTMLElement nsElem = this.node as nsIDOMNSHTMLElement;
					nsElem.getInnerHTML (storage);
					str += ">" + Base.StringGet (storage) + "</" + tag + ">";
					return str;
				}
			}
			set {
				nsIDOMDocumentRange docRange = ((Document) control.Document).XPComObject as nsIDOMDocumentRange;
				nsIDOMRange range;
				docRange.createRange (out range);
				range.setStartBefore (this.node);
				nsIDOMNSRange nsRange = range as nsIDOMNSRange;
				Base.StringSet (storage, value);
				nsIDOMDocumentFragment fragment;
				nsRange.createContextualFragment (storage, out fragment);
				nsIDOMNode parent;
				this.node.getParentNode (out parent);
				parent = nsDOMNode.GetProxy (this.control, parent);
				nsIDOMNode newNode;
				parent.replaceChild (fragment as nsIDOMNode, this.node as nsIDOMNode, out newNode);
				this.node = newNode as Mono.Mozilla.nsIDOMHTMLElement;
			}
		}

		public override io.Stream ContentStream {
			get {
				try {
					control.DocEncoder.Flags = DocumentEncoderFlags.OutputRaw;
					if (this.Equals (Owner.DocumentElement))
						return control.DocEncoder.EncodeToStream ((Document) Owner);
					return control.DocEncoder.EncodeToStream (this);
				} catch {
					string tag = this.TagName;
					string str = "<" + tag;
					foreach (IAttribute att in this.Attributes) {
						str += " " + att.Name + "=\"" + att.Value + "\"";
					}
					nsIDOMNSHTMLElement nsElem = this.node as nsIDOMNSHTMLElement;
					nsElem.getInnerHTML (storage);
					str += ">" + Base.StringGet (storage) + "</" + tag + ">";
					byte[] bytes = System.Text.ASCIIEncoding.UTF8.GetBytes (str);
					return new io.MemoryStream (bytes);
				}
			}
		}

		
		public override bool Disabled
		{			
			get {
				if (this.HasAttribute ("disabled")) {
					string dis = this.GetAttribute ("disabled");
					return bool.Parse (dis);
				}
				return false;
			}
			set {
				if (this.HasAttribute ("disabled")) {
					this.SetAttribute ("disabled", value.ToString ());
				}
			}
		}

		public override int TabIndex {
			get { 
				int tabIndex;
				((nsIDOMNSHTMLElement)this.node).getTabIndex (out tabIndex);
				return tabIndex;
			}
			set { 
				((nsIDOMNSHTMLElement)this.node).setTabIndex (value);
			}
		}

		public override int GetHashCode () {
			return this.hashcode;
		}
		#endregion
	}
}
