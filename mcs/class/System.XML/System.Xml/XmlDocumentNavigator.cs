//
// System.Xml.XmlDocumentNavigator
//
// Author:
//   Jason Diamond <jason@injektilo.org>
//
// (C) 2002 Jason Diamond
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace System.Xml
{
	internal class XmlDocumentNavigator : XPathNavigator
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

		private XmlNode node;
		private XmlDocument document;
		private IEnumerator attributesEnumerator;

		#endregion

		#region Properties

		public override string BaseURI {
			get {
				return node.BaseURI;
			}
		}

		public override bool HasAttributes {
			get {
				if (node.Attributes != null)
					foreach (XmlAttribute attribute in node.Attributes)
						if (attribute.NamespaceURI != "http://www.w3.org/2000/xmlns/")
							return true;
				return false;
			}
		}

		public override bool HasChildren {
			get {
				XPathNodeType nodeType = NodeType;
				bool canHaveChildren = nodeType == XPathNodeType.Root || nodeType == XPathNodeType.Element;
				return canHaveChildren && node.FirstChild != null;
			}
		}

		public override bool IsEmptyElement {
			get {
				return node.NodeType == XmlNodeType.Element && !HasChildren;
			}
		}

		public override string LocalName {
			get {
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
			get {
				return node.NamespaceURI;
			}
		}

		public override XmlNameTable NameTable {
			get {
				return document.NameTable;
			}
		}

		public override XPathNodeType NodeType {
			get {
				return node.XPathNodeType;
			}
		}

		public override string Prefix {
			get {
				return node.Prefix;
			}
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
			return new XmlDocumentNavigator (node);
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			XmlElement el = Node as XmlElement;
			return (el != null) ? el.GetAttribute (localName, namespaceURI) : String.Empty;
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
				return node == otherDocumentNavigator.node;
			return false;
		}

		public override bool MoveTo (XPathNavigator other)
		{
			XmlDocumentNavigator otherDocumentNavigator = other as XmlDocumentNavigator;
			if (otherDocumentNavigator != null) {
				if (document == otherDocumentNavigator.document) {
					node = otherDocumentNavigator.node;
					return true;
				}
			}
			return false;
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			attributesEnumerator = node.Attributes.GetEnumerator ();
			while (attributesEnumerator.MoveNext ()) {
				XmlAttribute attr = attributesEnumerator.Current as XmlAttribute;
				if (attr.LocalName == localName && attr.NamespaceURI == namespaceURI) {
					node = attr;
					return true;
				}
			}
			return false;
		}

		public override bool MoveToFirst ()
		{
			if (node.NodeType != XmlNodeType.Attribute && node.ParentNode != null) {
				node = node.ParentNode.FirstChild;
				return true;
			}
			return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (NodeType == XPathNodeType.Element) {
				attributesEnumerator = node.Attributes.GetEnumerator ();
				return MoveToNextAttribute ();
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

		[MonoTODO]
		public override bool MoveToFirstNamespace (XPathNamespaceScope namespaceScope)
		{
			throw new NotImplementedException ();
		}

		public override bool MoveToId (string id)
		{
			XmlElement eltNew = document.GetElementById (id);
			if (eltNew == null)
				return false;

			node = eltNew;
			return true;
		}

		[MonoTODO]
		public override bool MoveToNamespace (string name)
		{
			throw new NotImplementedException ();
		}

		public override bool MoveToNext ()
		{
			if (node.NextSibling != null) {
				node = node.NextSibling;
				return true;
			}
			return false;
		}

		public override bool MoveToNextAttribute ()
		{
			if (attributesEnumerator != null && attributesEnumerator.MoveNext ()) {
				node = attributesEnumerator.Current as XmlAttribute;
				return true;
			}
			return false;
		}

		[MonoTODO]
		public override bool MoveToNextNamespace (XPathNamespaceScope namespaceScope)
		{
			throw new NotImplementedException ();
		}

		public override bool MoveToParent ()
		{
			if (node.NodeType == XmlNodeType.Attribute) {
				XmlElement ownerElement = ((XmlAttribute)node).OwnerElement;
				if (ownerElement != null) {
					node = ownerElement;
					return true;
				}
			} else if (node.ParentNode != null) {
				node = node.ParentNode;
				return true;
			}
			return false;
		}

		public override bool MoveToPrevious ()
		{
			if (node.PreviousSibling != null) {
				node = node.PreviousSibling;
				return true;
			}
			return false;
		}

		public override void MoveToRoot ()
		{
			node = document;
		}

		internal XmlNode Node { get { return node; } }

		#endregion
	}
}
