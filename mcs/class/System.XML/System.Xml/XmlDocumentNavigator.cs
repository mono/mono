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

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace System.Xml
{
	internal class XmlDocumentNavigator : XPathNavigator, IHasXmlNode
	{
		#region Constructors

		internal XmlDocumentNavigator(XmlNode node)
		{
			this.node = node;
			this.document = node.NodeType == XmlNodeType.Document ?
				node as XmlDocument : node.OwnerDocument;
		}

		#endregion

		#region Fields
		private const string Xmlns = "http://www.w3.org/2000/xmlns/";
		private const string XmlnsXML = "http://www.w3.org/XML/1998/namespace";

		private XmlNode node;
		private XmlDocument document;
		// Current namespace node (ancestor's attribute of current node).
		private XmlNode nsNode;

		#endregion

		#region Properties

		public override string BaseURI {
			get {
				return node.BaseURI;
			}
		}

		public override bool HasAttributes {
			get {
				if (nsNode != null)
					return false;

				if (node.Attributes != null)
					foreach (XmlAttribute attribute in node.Attributes)
						if (attribute.NamespaceURI != Xmlns)
							return true;
				return false;
			}
		}

		public override bool HasChildren {
			get {
				if (nsNode != null)
					return false;

				XPathNodeType nodeType = NodeType;
				bool canHaveChildren = nodeType == XPathNodeType.Root || nodeType == XPathNodeType.Element;
				return canHaveChildren && node.FirstChild != null;
			}
		}

		public override bool IsEmptyElement {
			get {
				if (nsNode != null)
					return false;

				return node.NodeType == XmlNodeType.Element 
					&& ((XmlElement) node).IsEmpty;
			}
		}

		public override string LocalName {
			get {
				if (nsNode != null) {
					if (nsNode == document)
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
				if (nsNode != null)
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
			get { return (nsNode != null) ? String.Empty : node.NamespaceURI; }
		}

		public override XmlNameTable NameTable {
			get {
				return document.NameTable;
			}
		}

		public override XPathNodeType NodeType {
			get { return (nsNode != null) ? XPathNodeType.Namespace : node.XPathNodeType; }
		}

		public override string Prefix {
			get { return (nsNode != null) ? String.Empty : node.Prefix; }
		}

		public override string Value {
			get {
				switch (NodeType) {
				case XPathNodeType.Attribute:
				case XPathNodeType.Comment:
				case XPathNodeType.ProcessingInstruction:
				case XPathNodeType.Text:
				case XPathNodeType.Whitespace:
				case XPathNodeType.SignificantWhitespace:
					return node.Value;
				case XPathNodeType.Element:
				case XPathNodeType.Root:
					return node.InnerText;
				case XPathNodeType.Namespace:
					return nsNode == document ? XmlnsXML : nsNode.Value;
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

		public override XPathNavigator Clone ()
		{
			XmlDocumentNavigator clone = new XmlDocumentNavigator (node);
			clone.nsNode = nsNode;
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
		
		public override bool IsSamePosition (XPathNavigator other)
		{
			XmlDocumentNavigator otherDocumentNavigator = other as XmlDocumentNavigator;
			if (otherDocumentNavigator != null)
				return node == otherDocumentNavigator.node
					&& nsNode == otherDocumentNavigator.nsNode;
			return false;
		}

		public override bool MoveTo (XPathNavigator other)
		{
			XmlDocumentNavigator otherDocumentNavigator = other as XmlDocumentNavigator;
			if (otherDocumentNavigator != null) {
				if (document == otherDocumentNavigator.document) {
					node = otherDocumentNavigator.node;
					nsNode = otherDocumentNavigator.nsNode;
					return true;
				}
			}
			return false;
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			if (node.Attributes != null) {
				foreach (XmlAttribute attr in node.Attributes) {
					if (attr.LocalName == localName
						&& attr.NamespaceURI == namespaceURI) {
						node = attr;
						nsNode = null;
						return true;
					}
				}
			}
			return false;
		}

		public override bool MoveToFirst ()
		{
			if (nsNode == null && node.NodeType != XmlNodeType.Attribute && node.ParentNode != null) {
				MoveToParent ();
				// Follow these 2 steps so that we can skip 
				// some types of nodes .
				MoveToFirstChild ();
				return true;
			}
			return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (node.Attributes == null)
				return false;
			if (NodeType == XPathNodeType.Element) {
				foreach (XmlAttribute attr in node.Attributes) {
					if (attr.NamespaceURI != Xmlns) {
						node = attr;
						nsNode = null;
						return true;
					}
				}
			}
			return false;
		}

		public override bool MoveToFirstChild ()
		{
			if (HasChildren) {
				if (node == document) {
					XmlNode n = node.FirstChild;
					if (n == null)
						return false;
					bool loop = true;
					do {
						switch (n.NodeType) {
						case XmlNodeType.XmlDeclaration:
						case XmlNodeType.DocumentType:
							n = n.NextSibling;
							if (n == null)
								return false;
							break;
						default:
							loop = false;
							break;
						}
					} while (loop);
					node = n;
				}
				else
					node = node.FirstChild;
				return true;
			}
			return false;
		}

		public override bool MoveToFirstNamespace (XPathNamespaceScope namespaceScope)
		{
			if (NodeType != XPathNodeType.Element)
				return false;

			XmlElement el = node as XmlElement;
			if (node.Attributes != null) {
				do {
					foreach (XmlAttribute attr in el.Attributes) {
						if (attr.NamespaceURI == Xmlns) {
							nsNode = attr;
							return true;
						}
					}
					if (namespaceScope == XPathNamespaceScope.Local)
						return false;
					el = node.ParentNode as XmlElement;
				} while (el != null);
			}

			if (namespaceScope == XPathNamespaceScope.All) {
				nsNode = document;
				return true;
			}
			else
				return false;
		}

		public override bool MoveToId (string id)
		{
			XmlElement eltNew = document.GetElementById (id);
			if (eltNew == null)
				return false;

			node = eltNew;
			return true;
		}

		public override bool MoveToNamespace (string name)
		{
			if (name == "xml") {
				nsNode = document;
				return true;
			}

			if (NodeType != XPathNodeType.Element)
				return false;

			XmlElement el = node as XmlElement;
			if (node.Attributes != null) {
				do {
					foreach (XmlAttribute attr in el.Attributes) {
						if (attr.NamespaceURI == Xmlns && Name == name) {
							nsNode = attr;
							return true;
						}
					}
					el = node.ParentNode as XmlElement;
				} while (el != null);
			}
			return false;
		}

		public override bool MoveToNext ()
		{
			if (nsNode != null)
				return false;

			if (node.NextSibling != null) {
				if (node.ParentNode != null && node.ParentNode.NodeType == XmlNodeType.Document) {
					XmlNode n = node.NextSibling;
					while (n != null) {
						switch (n.NodeType) {
						case XmlNodeType.DocumentType:
						case XmlNodeType.XmlDeclaration:
							n = n.NextSibling;
							continue;
						}
						break;
					}
					if (n != null)
						node = n;
					else
						return false;
				}
				else
					node = node.NextSibling;
				return true;
			}
			else
				return false;
		}

		public override bool MoveToNextAttribute ()
		{
			if (NodeType != XPathNodeType.Attribute)
				return false;

			// Find current attribute.
			int pos = 0;
			XmlElement owner = ((XmlAttribute) node).OwnerElement;
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
					nsNode = null;
					return true;
				}
			}
			return false;
		}

		public override bool MoveToNextNamespace (XPathNamespaceScope namespaceScope)
		{
			if (nsNode == document)
				// Current namespace is "xml", so there should be no more namespace nodes.
				return false;

			if (nsNode == null)
				return false;

			// Get current attribute's position.
			int pos = 0;
			XmlElement owner = ((XmlAttribute) nsNode).OwnerElement;
			int count = owner.Attributes.Count;
			for(; pos < count; pos++)
				if (owner.Attributes [pos] == nsNode)
					break;
			if (pos == count)
				return false;	// Where is current attribute? Maybe removed.

			// Find next namespace from the same element as current ns node.
			for(pos++; pos < count; pos++) {
				if (owner.Attributes [pos].NamespaceURI == Xmlns) {
					nsNode = owner.Attributes [pos];
					return true;
				}
			}

			// If not found more, then find from ancestors.
			// But if scope is Local, then it returns false here.
			if (namespaceScope == XPathNamespaceScope.Local)
				return false;
			owner = owner.ParentNode as XmlElement;
			while (owner != null) {
				foreach (XmlAttribute attr in owner.Attributes) {
					if (attr.NamespaceURI == Xmlns) {
						nsNode = attr;
						return true;
					}
				}
				owner = owner.ParentNode as XmlElement;
			}

			if (namespaceScope == XPathNamespaceScope.All) {
				nsNode = document;
				return true;
			}
			else
				return false;
		}

		public override bool MoveToParent ()
		{
			if (nsNode != null) {
				nsNode = null;
				return true;
			}
			else if (node.NodeType == XmlNodeType.Attribute) {
				XmlElement ownerElement = ((XmlAttribute)node).OwnerElement;
				if (ownerElement != null) {
					node = ownerElement;
					nsNode = null;
					return true;
				}
			} else if (node.ParentNode != null) {
				node = node.ParentNode;
				nsNode = null;
				return true;
			}
			return false;
		}

		public override bool MoveToPrevious ()
		{
			if (nsNode != null)
				return false;

			if (node.PreviousSibling != null) {
				node = node.PreviousSibling;
				return true;
			}
			return false;
		}

		public override void MoveToRoot ()
		{
			node = document;
			nsNode = null;
		}

		internal XmlNode Node { get { return node; } }

                XmlNode IHasXmlNode.GetNode ()
                {
                        return node;
                }

		#endregion
	}
}
