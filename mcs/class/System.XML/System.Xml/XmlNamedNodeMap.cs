//
// System.Xml.XmlNamedNodeMap
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Duncan Mak  (duncan@ximian.com)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Collections;
using Mono.Xml;

namespace System.Xml
{
	public class XmlNamedNodeMap : IEnumerable
	{
		XmlNode parent;
		ArrayList nodeList;
		bool readOnly = false;

		internal XmlNamedNodeMap (XmlNode parent)
		{
			this.parent = parent;
			nodeList = new ArrayList ();
		}

		public virtual int Count {
			get { return nodeList.Count; }
		}

		public virtual IEnumerator GetEnumerator () 
		{
			return nodeList.GetEnumerator ();
		}

		public virtual XmlNode GetNamedItem (string name)
		{
			for (int i = 0; i < nodeList.Count; i++) {
				XmlNode node = (XmlNode) nodeList [i];
				if (node.Name == name)
					return node;
			}
			return null;
		}

		public virtual XmlNode GetNamedItem (string localName, string namespaceURI)
		{
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
			if (index < 0 || index >= nodeList.Count)
				return null;
			else
				return (XmlNode) nodeList [index];
		}

		public virtual XmlNode RemoveNamedItem (string name)
		{
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
				for (int i = 0; i < nodeList.Count; i++) {
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

				return null;
			} finally {
				if (raiseEvent)
					parent.OwnerDocument.onNodeInserted (node, parent);
			}

		}

		internal ArrayList Nodes { get { return nodeList; } }
	}
}
