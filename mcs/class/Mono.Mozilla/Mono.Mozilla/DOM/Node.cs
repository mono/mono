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
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class Node: DOMObject, INode
	{
		private nsIDOMNode node;
		
		public Node (WebBrowser control, nsIDOMNode domNode) : base (control)
		{
			if (!(domNode is nsIDOMHTMLDocument) && control.platform != control.enginePlatform)
				this.node = nsDOMNode.GetProxy (control, domNode);
			else
				this.node = domNode;
		}

		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.resources.Clear ();
					this.node = null;
				}
			}
			base.Dispose(disposing);
		}		
		#endregion

		#region INode Members

		public virtual IAttributeCollection Attributes
		{
			get
			{
				if (!resources.Contains ("Attributes")) {
					nsIDOMNamedNodeMap attributes;
					this.node.getAttributes (out attributes);
					resources.Add ("Attributes", new AttributeCollection (control, attributes));
				}
				return resources["Attributes"] as IAttributeCollection;
			}
		}

		public virtual INodeList ChildNodes {
			get {
				if (!resources.Contains ("ChildNodes")) {
					nsIDOMNodeList children;
					this.node.getChildNodes (out children);
					resources.Add ("ChildNodes", new NodeList (control, children));
				}
				return resources["ChildNodes"] as INodeList;
			}
		}


		public virtual INode FirstChild {
			get {
				if (!resources.Contains ("FirstChild")) {
					nsIDOMNode child;
					this.node.getFirstChild (out child);
					resources.Add ("FirstChild", new Node (control, child));
				}
				return resources["FirstChild"] as INode;
			}
		}

		public virtual string LocalName {
			get {
				this.node.getLocalName (storage);
				return Base.StringGet (storage);				
			}
		}

		public virtual Mono.WebBrowser.DOM.NodeType Type {
			get {
				ushort type;
				this.node.getNodeType (out type);
				return (Mono.WebBrowser.DOM.NodeType) Enum.ToObject (typeof (Mono.WebBrowser.DOM.NodeType), type);
			}
		}

		public virtual string Value {
			get
			{
				this.node.getNodeValue (storage);
				return Base.StringGet (storage);
			}
		}
		
		#endregion
	}
}
