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

		[MonoTODO]
		internal XmlDocumentNavigator(XmlNode node)
		{
			this.node = node;
		}

		#endregion

		#region Fields

		private XmlNode node;
		private IEnumerator attributesEnumerator;

		#endregion

		#region Properties

		[MonoTODO]
		public override string BaseURI {
			get {
				throw new NotImplementedException ();
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

		[MonoTODO]
		public override XmlNameTable NameTable {
			get {
				throw new NotImplementedException ();
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

		[MonoTODO]
		public override string XmlLang {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region Methods

		public override XPathNavigator Clone ()
		{
			return new XmlDocumentNavigator (node);
		}

		[MonoTODO]
		public override string GetAttribute (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetNamespace (string name)
		{
			throw new NotImplementedException ();
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
				if (node.OwnerDocument == otherDocumentNavigator.node.OwnerDocument) {
					node = otherDocumentNavigator.node;
					return true;
				}
			}
			return false;
		}

		[MonoTODO]
		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			throw new NotImplementedException ();
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
			XmlDocument doc;
			
			if (node.NodeType == XmlNodeType.Document)
				doc = (XmlDocument) node;
			else
				doc = node.OwnerDocument;

			XmlElement eltNew = doc.GetElementById (id);
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
			if (node.NodeType != XmlNodeType.Document)
				node = node.OwnerDocument;
		}

		internal XmlNode Node { get { return node; } }

		#endregion
	}
}
