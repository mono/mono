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
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;


namespace Mono.Mozilla.DOM
{
	internal class HTMLElementCollection : NodeList, IElementCollection
	{
		public HTMLElementCollection (WebBrowser control, nsIDOMNodeList nodeList) : base (control, nodeList)
		{
		}
		
		
		public HTMLElementCollection (WebBrowser control) : base (control)
		{
		}
		
		internal override void Load ()
		{
			Clear ();
			uint count;
			unmanagedNodes.getLength (out count);
			Node[] tmpnodes = new Node[count];
			for (int i = 0; i < count;i++) {
				nsIDOMNode node;
				unmanagedNodes.item ((uint)i, out node);
				ushort type;
				node.getNodeType (out type);
				if (type == (ushort)NodeType.Element)
					tmpnodes[nodeCount++] = new HTMLElement (control, (nsIDOMHTMLElement)node);
			}
			nodes = new Node[nodeCount];
			Array.Copy (tmpnodes, nodes, nodeCount);
		}
		
		#region IList members
		public new IElement this [int index] {
			get {
				if (index < 0 || index >= nodeCount)
					throw new ArgumentOutOfRangeException ("index");
				return nodes [index] as IElement;
			}
			set {
				if (index < 0 || index >= nodeCount)
					throw new ArgumentOutOfRangeException ("index");
				nodes [index] = value as INode;
			}
		}
		
		#endregion
	}
}
