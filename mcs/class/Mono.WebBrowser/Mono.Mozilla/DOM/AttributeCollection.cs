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

using System;
using System.Collections;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;


namespace Mono.Mozilla.DOM
{
	internal class AttributeCollection : NodeList, IAttributeCollection
	{
		protected new nsIDOMNamedNodeMap unmanagedNodes;

		public AttributeCollection (WebBrowser control, nsIDOMNamedNodeMap nodeMap)
			: base (control, true)
		{
			if (control.platform != control.enginePlatform)
				unmanagedNodes = nsDOMNamedNodeMap.GetProxy (control, nodeMap);
			else
				unmanagedNodes = nodeMap;
		}
		
		public AttributeCollection (WebBrowser control) : base (control) 
		{
		}
		
		internal override void Load ()
		{
			if (unmanagedNodes == null) return;
			Clear ();
			uint count;
			unmanagedNodes.getLength (out count);
			nodeCount = (int) count;
			nodes = new Node[count];
			for (int i = 0; i < count; i++) {
				nsIDOMNode node;
				unmanagedNodes.item ((uint) i, out node);
				nodes[i] = new Attribute (control, node as nsIDOMAttr);
			}
		}

		public override int Count {
			get {
				if (unmanagedNodes != null && nodes == null)
					Load ();
				return nodeCount; 
			}
		}

		#region IList members
		public new IAttribute this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");
				return nodes[index] as IAttribute;
			}
			set { }
		}

		public IAttribute this[string name]
		{
			get
			{
				for (int i = 0; i < nodes.Length; i++) {
					if (((IAttribute) nodes[i]).Name.Equals (name))
						return nodes[i] as IAttribute;
				}
				return null;
			}
		}

		public bool Exists (string name)
		{
			if (unmanagedNodes == null) return false;
			Base.StringSet (storage, name);
			nsIDOMNode ret;
			unmanagedNodes.getNamedItem (storage, out ret);
			return ret != null;
		}
		#endregion
		
		public override int GetHashCode () {
			if (unmanagedNodes == null) return base.GetHashCode ();
			return this.unmanagedNodes.GetHashCode ();
		}
	}
}
