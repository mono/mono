//
// System.Xml.XmlDocumentNavigator
//
// Authors:
//   Jason Diamond <jason@injektilo.org>
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Jason Diamond
// (C) 2003 Atsushi Enomoto
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
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Xml
{
	internal class XmlDocumentNavigator : XPathNavigator, IHasXmlNode
	{
		#region Constructors

		internal XmlDocumentNavigator (XmlNode node)
		{
			this.node = node;
			if (node.NodeType == XmlNodeType.Attribute && node.NamespaceURI == XmlNamespaceManager.XmlnsXmlns) {
				nsNode = (XmlAttribute) node; 
				node = nsNode.OwnerElement;
			}
		}

		#endregion

		#region Fields
		private const string Xmlns = "http://www.w3.org/2000/xmlns/";
		private const string XmlnsXML = "http://www.w3.org/XML/1998/namespace";

		private XmlNode node;
		// Current namespace node (ancestor's attribute of current node).
		private XmlAttribute nsNode;
		private ArrayList iteratedNsNames;
		#endregion

		#region Properties

		internal XmlDocument Document {
			get { return node.NodeType == XmlNodeType.Document ? node as XmlDocument : node.OwnerDocument; }
		}

		public override string BaseURI {
			get {
				return node.BaseURI;
			}
		}

		public override bool HasAttributes {
			get {
				if (NsNode != null)
					return false;

				XmlElement el = node as XmlElement;
				if (el == null || !el.HasAttributes)
					return false;

				for (int i = 0; i < node.Attributes.Count; i++)
					if (node.Attributes [i].NamespaceURI != Xmlns)
						return true;
				return false;
			}
		}

		public override bool HasChildren {
			get {
				if (NsNode != null)
					return false;

				XPathNodeType nodeType = NodeType;
				bool canHaveChildren = nodeType == XPathNodeType.Root || nodeType == XPathNodeType.Element;
				return canHaveChildren && GetFirstChild (node) != null;
			}
		}

		public override bool IsEmptyElement {
			get {
				if (NsNode != null)
					return false;

				return node.NodeType == XmlNodeType.Element 
					&& ((XmlElement) node).IsEmpty;
			}
		}

		public XmlAttribute NsNode {
			get { return nsNode; }
			set {
				if (value == null)
					iteratedNsNames = null;
				else
				{
					if (iteratedNsNames == null)
						iteratedNsNames = new ArrayList();
					else
					{
						if (iteratedNsNames.IsReadOnly)
							iteratedNsNames = new ArrayList(iteratedNsNames);
					}
					iteratedNsNames.Add (value.Name);
				}
				nsNode = value;
			}
		}

		public override string LocalName {
			get {
				XmlAttribute nsNode = NsNode;
				if (nsNode != null) {
					if (nsNode == Document.NsNodeXml)
						return "xml";
					else
						return (nsNode.Name == "xmlns") ? String.Empty : nsNode.LocalName;
				}

				XPathNodeType nodeType = NodeType;
				bool canHaveName = 
					nodeType == XPathNodeType.Element || 
					nodeType == XPathNodeType.Attribute || 
					nodeType == XPathNodeType.ProcessingInstruction ||
					nodeType == XPathNodeType.Namespace;
				return canHaveName ? node.LocalName : String.Empty;
			}
		}

		public override string Name {
			get {
				if (NsNode != null)
					return LocalName;

				XPathNodeType nodeType = NodeType;
				bool canHaveName = 
					nodeType == XPathNodeType.Element || 
					nodeType == XPathNodeType.Attribute || 
					nodeType == XPathNodeType.ProcessingInstruction ||
					nodeType == XPathNodeType.Namespace;
				return canHaveName ? node.Name : String.Empty;
			}
		}

		public override string NamespaceURI {
			get { return (NsNode != null) ? String.Empty : node.NamespaceURI; }
		}

		public override XmlNameTable NameTable {
			get { return Document.NameTable; }
		}

		public override XPathNodeType NodeType {
			get {
				if (NsNode != null)
					return XPathNodeType.Namespace;
				XmlNode n = node;
				bool sw = false;
				do {
					switch (n.NodeType) {
					case XmlNodeType.SignificantWhitespace:
						sw = true;
						n = GetNextSibling (n);
						break;
					case XmlNodeType.Whitespace:
						n = GetNextSibling (n);
						break;
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
						return XPathNodeType.Text;
					default:
						n = null;
						break;
					}
				} while (n != null);
				return sw ?
					XPathNodeType.SignificantWhitespace :
					node.XPathNodeType;
			}
		}

		public override string Prefix {
			get { return (NsNode != null) ? String.Empty : node.Prefix; }
		}

#if NET_2_0
		public override IXmlSchemaInfo SchemaInfo {
			get { return NsNode != null ? null : node.SchemaInfo; }
		}

		public override object UnderlyingObject {
			get { return node; }
		}
#endif

		public override string Value {
			get {
				switch (NodeType) {
				case XPathNodeType.Attribute:
				case XPathNodeType.Comment:
				case XPathNodeType.ProcessingInstruction:
					return node.Value;
				case XPathNodeType.Text:
				case XPathNodeType.Whitespace:
				case XPathNodeType.SignificantWhitespace:
					string value = node.Value;
					for (XmlNode n = GetNextSibling (node); n != null; n = GetNextSibling (n)) {
						switch (n.XPathNodeType) {
						case XPathNodeType.Text:
						case XPathNodeType.Whitespace:
						case XPathNodeType.SignificantWhitespace:
							value += n.Value;
							continue;
						}
						break;
					}
					return value;
				case XPathNodeType.Element:
				case XPathNodeType.Root:
					return node.InnerText;
				case XPathNodeType.Namespace:
					return NsNode == Document.NsNodeXml ? XmlnsXML : NsNode.Value;
				}
				return String.Empty;
			}
		}

		public override string XmlLang {
			get {
				return node.XmlLang;
			}
		}

		#endregion

		#region Methods

		private bool CheckNsNameAppearance (string name, string ns)
		{
			if (iteratedNsNames != null && iteratedNsNames.Contains (name))
				return true;
			// default namespace erasure - just add name and never return this node
			if (ns == String.Empty) {
				if (iteratedNsNames == null)
					iteratedNsNames = new ArrayList();
				else
				{
					if (iteratedNsNames.IsReadOnly)
						iteratedNsNames = new ArrayList(iteratedNsNames);
				}
				iteratedNsNames.Add ("xmlns");
				return true;
			}

			return false;
		}

		public override XPathNavigator Clone ()
		{
			XmlDocumentNavigator clone = new XmlDocumentNavigator (node);
			clone.nsNode = nsNode;
			clone.iteratedNsNames = (iteratedNsNames == null || iteratedNsNames.IsReadOnly) ? iteratedNsNames : ArrayList.ReadOnly(iteratedNsNames);
			return clone;
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			if (HasAttributes) {
				XmlElement el = Node as XmlElement;
				return el != null ? el.GetAttribute (localName, namespaceURI) : String.Empty;
			}
			return String.Empty;
		}

		public override string GetNamespace (string name)
		{
			// MSDN says "String.Empty if a matching namespace 
			// node is not found or if the navigator is not 
			// positioned on an element node", but in fact it
			// returns actual namespace for the other nodes.
			return Node.GetNamespaceOfPrefix (name);
		}

		public override bool IsDescendant (XPathNavigator other)
		{
			if (NsNode != null)
				return false;
			XmlDocumentNavigator o = other as XmlDocumentNavigator;
			if (o == null)
				return false;
			XmlNode n =
				o.node.NodeType == XmlNodeType.Attribute ?
				((XmlAttribute) o.node).OwnerElement :
				o.node.ParentNode;
			for (;n != null; n = n.ParentNode)
				if (n == node)
					return true;
			return false;
		}

		public override bool IsSamePosition (XPathNavigator other)
		{
			XmlDocumentNavigator otherDocumentNavigator = other as XmlDocumentNavigator;
			if (otherDocumentNavigator != null)
				return node == otherDocumentNavigator.node
					&& NsNode == otherDocumentNavigator.NsNode;
			return false;
		}

		public override bool MoveTo (XPathNavigator other)
		{
			XmlDocumentNavigator otherDocumentNavigator = other as XmlDocumentNavigator;
			if (otherDocumentNavigator != null) {
				if (Document == otherDocumentNavigator.Document) {
					node = otherDocumentNavigator.node;
					NsNode = otherDocumentNavigator.NsNode;
					return true;
				}
			}
			return false;
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			if (HasAttributes) {
				XmlAttribute attr = node.Attributes [localName, namespaceURI];
				if (attr != null) {
					node = attr;
					NsNode = null;
					return true;
				}
			}
			return false;
		}

#if NET_2_0
#else
		public override bool MoveToFirst ()
		{
			return MoveToFirstImpl ();
		}
#endif

		public override bool MoveToFirstAttribute ()
		{
			if (NodeType == XPathNodeType.Element) {
				XmlElement el = node as XmlElement;
				if (!el.HasAttributes)
					return false;
				for (int i = 0; i < node.Attributes.Count; i++) {
					XmlAttribute attr = node.Attributes [i];
					if (attr.NamespaceURI != Xmlns) {
						node = attr;
						NsNode = null;
						return true;
					}
				}
			}
			return false;
		}

		public override bool MoveToFirstChild ()
		{
			if (HasChildren) {
				XmlNode n = GetFirstChild (node);
				if (n == null)
					return false;
				node = n;
				return true;
			}
			return false;
		}

		public override bool MoveToFirstNamespace (XPathNamespaceScope namespaceScope)
		{
			if (NodeType != XPathNodeType.Element)
				return false;
			XmlElement el = node as XmlElement;
			do {
				if (el.HasAttributes) {
					for (int i = 0; i < el.Attributes.Count; i++) {
						XmlAttribute attr = el.Attributes [i];
						if (attr.NamespaceURI == Xmlns) {
							if (CheckNsNameAppearance (attr.Name, attr.Value))
								continue;
							NsNode = attr;
							return true;
						}
					}
				}
				if (namespaceScope == XPathNamespaceScope.Local)
					return false;
				el = GetParentNode (el) as XmlElement;
			} while (el != null);

			if (namespaceScope == XPathNamespaceScope.All) {
				if (CheckNsNameAppearance (Document.NsNodeXml.Name, Document.NsNodeXml.Value))
					return false;
				NsNode = Document.NsNodeXml;
				return true;
			}
			else
				return false;
		}

		public override bool MoveToId (string id)
		{
			XmlElement eltNew = Document.GetElementById (id);
			if (eltNew == null)
				return false;

			node = eltNew;
			return true;
		}

		public override bool MoveToNamespace (string name)
		{
			if (name == "xml") {
				NsNode = Document.NsNodeXml;
				return true;
			}

			if (NodeType != XPathNodeType.Element)
				return false;

			XmlElement el = node as XmlElement;
			do {
				if (el.HasAttributes) {
					for (int i = 0; i < el.Attributes.Count; i++) {
						XmlAttribute attr = el.Attributes [i];
						if (attr.NamespaceURI == Xmlns && attr.Name == name) {
							NsNode = attr;
							return true;
						}
					}
				}
				el = GetParentNode (node) as XmlElement;
			} while (el != null);
			return false;
		}

		public override bool MoveToNext ()
		{
			if (NsNode != null)
				return false;

			XmlNode n = node;
			if (NodeType == XPathNodeType.Text) {
				do {
					n = GetNextSibling (n);
					if (n == null)
						return false;
					switch (n.NodeType) {
					case XmlNodeType.CDATA:
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Text:
					case XmlNodeType.Whitespace:
						continue;
					default:
						break;
					}
					break;
				} while (true);
			}
			else
				n = GetNextSibling (n);
			if (n == null)
				return false;
			node = n;
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (node == null)
				return false;
			if (NodeType != XPathNodeType.Attribute)
				return false;

			// Find current attribute.
			int pos = 0;
			XmlElement owner = ((XmlAttribute) node).OwnerElement;
			if (owner == null)
				return false;

			int count = owner.Attributes.Count;
			for(; pos < count; pos++)
				if (owner.Attributes [pos] == node)
					break;
			if (pos == count)
				return false;	// Where is current attribute? Maybe removed.

			// Find next attribute.
			for(pos++; pos < count; pos++) {
				if (owner.Attributes [pos].NamespaceURI != Xmlns) {
					node = owner.Attributes [pos];
					NsNode = null;
					return true;
				}
			}
			return false;
		}

		public override bool MoveToNextNamespace (XPathNamespaceScope namespaceScope)
		{
			if (NsNode == Document.NsNodeXml)
				// Current namespace is "xml", so there should be no more namespace nodes.
				return false;

			if (NsNode == null)
				return false;

			// Get current attribute's position.
			int pos = 0;
			XmlElement owner = ((XmlAttribute) NsNode).OwnerElement;
			if (owner == null)
				return false;

			int count = owner.Attributes.Count;
			for(; pos < count; pos++)
				if (owner.Attributes [pos] == NsNode)
					break;
			if (pos == count)
				return false;	// Where is current attribute? Maybe removed.

			// Find next namespace from the same element as current ns node.
			for(pos++; pos < count; pos++) {
				if (owner.Attributes [pos].NamespaceURI == Xmlns) {
					XmlAttribute a = owner.Attributes [pos];
					if (CheckNsNameAppearance (a.Name, a.Value))
						continue;
					NsNode = a;
					return true;
				}
			}

			// If not found more, then find from ancestors.
			// But if scope is Local, then it returns false here.
			if (namespaceScope == XPathNamespaceScope.Local)
				return false;
			owner = GetParentNode (owner) as XmlElement;
			while (owner != null) {
				if (owner.HasAttributes) {
					for (int i = 0; i < owner.Attributes.Count; i++) {
						XmlAttribute attr = owner.Attributes [i];
						if (attr.NamespaceURI == Xmlns) {
							if (CheckNsNameAppearance (attr.Name, attr.Value))
								continue;
							NsNode = attr;
							return true;
						}
					}
				}
				owner = GetParentNode (owner) as XmlElement;
			}

			if (namespaceScope == XPathNamespaceScope.All) {
				if (CheckNsNameAppearance (Document.NsNodeXml.Name, Document.NsNodeXml.Value))
					return false;
				NsNode = Document.NsNodeXml;
				return true;
			}
			return false;
		}

		public override bool MoveToParent ()
		{
			if (NsNode != null) {
				NsNode = null;
				return true;
			}
			else if (node.NodeType == XmlNodeType.Attribute) {
				XmlElement ownerElement = ((XmlAttribute)node).OwnerElement;
				if (ownerElement != null) {
					node = ownerElement;
					NsNode = null;
					return true;
				}
				else
					return false;
			}
			XmlNode n = GetParentNode (node);
			if (n == null)
				return false;
			node = n;
			NsNode = null;
			return true;
		}

		public override bool MoveToPrevious ()
		{
			if (NsNode != null)
				return false;

			XmlNode p = GetPreviousSibling (node);
			if (p == null)
				return false;
			node = p;
			return true;
		}

		public override void MoveToRoot ()
		{
			XmlAttribute attr = node as XmlAttribute;
			XmlNode tmp = attr != null ? attr.OwnerElement : node;
			if (tmp == null)
				return; // i.e. attr has null OwnerElement.
			for (XmlNode tmp2 = GetParentNode (tmp); tmp2 != null; tmp2 = GetParentNode (tmp2))
				tmp = tmp2;
			node = tmp;
			NsNode = null;
		}

		private XmlNode Node { get { return NsNode != null ? NsNode : node; } }

		XmlNode IHasXmlNode.GetNode ()
		{
			return Node;
		}

		private XmlNode GetFirstChild (XmlNode n)
		{
			if (n.FirstChild == null)
				return null;
			switch (n.FirstChild.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return GetNextSibling (n.FirstChild);
			case XmlNodeType.EntityReference:
				foreach (XmlNode c in n.ChildNodes) {
					if (c.NodeType == XmlNodeType.EntityReference) {
						XmlNode ec = GetFirstChild (c);
						if (ec != null)
							return ec;
					}
					else
						return c;
				}
				return null;
			default:
				return n.FirstChild;
			}
		}

		private XmlNode GetLastChild (XmlNode n)
		{
			if (n.LastChild == null)
				return null;
			switch (n.LastChild.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return GetPreviousSibling (n.LastChild);
			case XmlNodeType.EntityReference:
				for (XmlNode c = n.LastChild; c != null; c = c.PreviousSibling) {
					if (c.NodeType == XmlNodeType.EntityReference) {
						XmlNode ec = GetLastChild (c);
						if (ec != null)
							return ec;
					}
					else
						return c;
				}
				return null;
			default:
				return n.LastChild;
			}
		}

		private XmlNode GetPreviousSibling (XmlNode n)
		{
			XmlNode p = n.PreviousSibling;
			if (p != null) {
				switch (p.NodeType) {
				case XmlNodeType.EntityReference:
					XmlNode c = GetLastChild (p);
					if (c != null)
						return c;
					else // empty entity reference etc.
						return GetPreviousSibling (p);
				case XmlNodeType.XmlDeclaration:
				case XmlNodeType.DocumentType:
					return GetPreviousSibling (p);
				default:
					return p;
				}
			} else {
				if (n.ParentNode == null || n.ParentNode.NodeType != XmlNodeType.EntityReference)
					return null;
				return GetPreviousSibling (n.ParentNode);
			}
		}

		private XmlNode GetNextSibling (XmlNode n)
		{
			XmlNode nx = n.NextSibling;
			if (nx != null) {
				switch (nx.NodeType) {
				case XmlNodeType.EntityReference:
					XmlNode c = GetFirstChild (nx);
					if (c != null)
						return c;
					else // empty entity reference etc.
						return GetNextSibling (nx);
				case XmlNodeType.XmlDeclaration:
				case XmlNodeType.DocumentType:
					return GetNextSibling (nx);
				default:
					return n.NextSibling;
				}
			} else {
				if (n.ParentNode == null || n.ParentNode.NodeType != XmlNodeType.EntityReference)
					return null;
				return GetNextSibling (n.ParentNode);
			}
		}

		private XmlNode GetParentNode (XmlNode n)
		{
			if (n.ParentNode == null)
				return null;
			for (XmlNode p = n.ParentNode; p != null; p = p.ParentNode)
				if (p.NodeType != XmlNodeType.EntityReference)
					return p;
			return null;
		}

#if NET_2_0
		public
#else
		internal
#endif
		override string LookupNamespace (string prefix)
		{
			// FIXME: optimize
			return base.LookupNamespace (prefix);
		}

#if NET_2_0
		public
#else
		internal
#endif
		override string LookupPrefix (string namespaceUri)
		{
			// FIXME: optimize
			return base.LookupPrefix (namespaceUri);
		}

#if NET_2_0
		public
#else
		internal
#endif
		override bool MoveToChild (XPathNodeType type)
		{
			// FIXME: optimize
			return base.MoveToChild (type);
		}

#if NET_2_0
		public
#else
		internal
#endif
		override bool MoveToChild (string localName, string namespaceURI)
		{
			// FIXME: optimize
			return base.MoveToChild (localName, namespaceURI);
		}

#if NET_2_0
		public
#else
		internal
#endif
		override bool MoveToNext (string localName, string namespaceURI)
		{
			// FIXME: optimize
			return base.MoveToNext (localName, namespaceURI);
		}

#if NET_2_0
		public
#else
		internal
#endif
		override bool MoveToNext (XPathNodeType type)
		{
			// FIXME: optimize
			return base.MoveToNext (type);
		}

#if NET_2_0
		public
#else
		internal
#endif
		override bool MoveToFollowing (string localName,
			string namespaceURI, XPathNavigator end)
		{
			// FIXME: optimize
			return base.MoveToFollowing (localName, namespaceURI, end);
		}

#if NET_2_0
		public
#else
		internal
#endif
		override bool MoveToFollowing (XPathNodeType type,
			XPathNavigator end)
		{
			// FIXME: optimize
			return base.MoveToFollowing (type, end);
		}
		#endregion
	}
}
