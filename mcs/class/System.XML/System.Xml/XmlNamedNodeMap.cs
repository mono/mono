//
// System.Xml.XmlNamedNodeMap
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Duncan Mak  (duncan@ximian.com)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

//
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

using System;
using System.Collections;
using Mono.Xml;

namespace System.Xml
{
	public class XmlNamedNodeMap : IEnumerable
	{
		static readonly IEnumerator emptyEnumerator = new XmlNode [0].GetEnumerator ();

		XmlNode parent;
		ArrayList nodeList;
		bool readOnly = false;

		internal XmlNamedNodeMap (XmlNode parent)
		{
			this.parent = parent;
		}

		private ArrayList NodeList {
			get {
				if (nodeList == null)
					nodeList = new ArrayList ();
				return nodeList;
			}
		}

		public virtual int Count {
			get { return nodeList == null ? 0 : nodeList.Count; }
		}

		public virtual IEnumerator GetEnumerator () 
		{
			if (nodeList == null)
				return emptyEnumerator;
			return nodeList.GetEnumerator ();
		}

		public virtual XmlNode GetNamedItem (string name)
		{
			if (nodeList == null)
				return null;

			for (int i = 0; i < nodeList.Count; i++) {
				XmlNode node = (XmlNode) nodeList [i];
				if (node.Name == name)
					return node;
			}
			return null;
		}

		public virtual XmlNode GetNamedItem (string localName, string namespaceURI)
		{
			if (nodeList == null)
				return null;

			for (int i = 0; i < nodeList.Count; i++) {
				XmlNode node = (XmlNode) nodeList [i];
				if ((node.LocalName == localName)
				    && (node.NamespaceURI == namespaceURI))
					return node;
			}

			return null;
		}
		
		public virtual XmlNode Item (int index)
		{
			if (nodeList == null || index < 0 || index >= nodeList.Count)
				return null;
			else
				return (XmlNode) nodeList [index];
		}

		public virtual XmlNode RemoveNamedItem (string name)
		{
			if (nodeList == null)
				return null;

			for (int i = 0; i < nodeList.Count; i++) {
				XmlNode node = (XmlNode) nodeList [i];
				if (node.Name == name) {
					if (node.IsReadOnly)
						throw new InvalidOperationException ("Cannot remove. This node is read only: " + name);
					nodeList.Remove (node);
					// Since XmlAttributeCollection does not override
					// it while attribute have to keep it in the
					// collection, it adds to the collection immediately.
					XmlAttribute attr = node as XmlAttribute;
					if (attr != null) {
						DTDAttributeDefinition def = attr.GetAttributeDefinition ();
						if (def != null && def.DefaultValue != null) {
							XmlAttribute newAttr = attr.OwnerDocument.CreateAttribute (attr.Prefix, attr.LocalName, attr.NamespaceURI, true, false);
							newAttr.Value = def.DefaultValue;
							newAttr.SetDefault ();
							attr.OwnerElement.SetAttributeNode (newAttr);
						}
					}
					return node;
				}
			}
			return null;
		}

		public virtual XmlNode RemoveNamedItem (string localName, string namespaceURI)
		{
			if (nodeList == null)
				return null;

			for (int i = 0; i < nodeList.Count; i++) {
				XmlNode node = (XmlNode) nodeList [i];
				if ((node.LocalName == localName)
				    && (node.NamespaceURI == namespaceURI)) {
					nodeList.Remove (node);
					return node;
				}
			}
			return null;
		}

		public virtual XmlNode SetNamedItem (XmlNode node)
		{
			return SetNamedItem (node, -1, true);
		}

		internal XmlNode SetNamedItem (XmlNode node, bool raiseEvent)
		{
			return SetNamedItem (node, -1, raiseEvent);
		}

		internal XmlNode SetNamedItem (XmlNode node, int pos, bool raiseEvent)
		{
			if (readOnly || (node.OwnerDocument != parent.OwnerDocument))
				throw new ArgumentException ("Cannot add to NodeMap.");

			if (raiseEvent)
				parent.OwnerDocument.onNodeInserting (node, parent);

			try {
				for (int i = 0; i < NodeList.Count; i++) {
					XmlNode x = (XmlNode) nodeList [i];
					if(x.LocalName == node.LocalName && x.NamespaceURI == node.NamespaceURI) {
						nodeList.Remove (x);
						if (pos < 0)
							nodeList.Add (node);
						else
							nodeList.Insert (pos, node);
						return x;
					}
				}
			
				if(pos < 0)
					nodeList.Add (node);
				else
					nodeList.Insert (pos, node);

				// LAMESPEC: It should return null here, but
				// it just returns the input node.
				return node;
			} finally {
				if (raiseEvent)
					parent.OwnerDocument.onNodeInserted (node, parent);
			}

		}

		internal ArrayList Nodes { get { return NodeList; } }
	}
}
