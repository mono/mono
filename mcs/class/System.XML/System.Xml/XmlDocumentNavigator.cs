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
using System.Xml.XPath;

namespace System.Xml
{
	internal class XmlDocumentNavigator : XPathNavigator, IHasXmlNode
	{
		#region Constructors

		internal XmlDocumentNavigator (XmlNode node)
			: this (node, null)
		{
			nsNodeXml = document.CreateAttribute ("xmlns", "xml", Xmlns);
			nsNodeXml.Value = XmlnsXML;
			if (node.NodeType == XmlNodeType.Attribute && node.NamespaceURI == XmlNamespaceManager.XmlnsXmlns) {
				nsNode = (XmlAttribute) node; 
				node = nsNode.OwnerElement;
			}
		}

		private XmlDocumentNavigator (XmlNode node, XmlAttribute nsNodeXml)
		{
			this.node = node;
			this.document = node.NodeType == XmlNodeType.Document ?
				node as XmlDocument : node.OwnerDocument;
			this.nsNodeXml = nsNodeXml;
		}

		#endregion

		#region Fields
		private const string Xmlns = "http://www.w3.org/2000/xmlns/";
		private const string XmlnsXML = "http://www.w3.org/XML/1998/namespace";

		private XmlAttribute nsNodeXml;
		private XmlNode node;
		private XmlDocument document;
		// Current namespace node (ancestor's attribute of current node).
		private XmlAttribute nsNode;
		private ArrayList iteratedNsNames = new ArrayList ();
		#endregion

		#region Properties

		public override string BaseURI {
			get {
				return node.BaseURI;
			}
		}

		public override bool HasAttributes {
			get {
				if (NsNode != null)
					return false;

				if (node.Attributes != null)
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
				return canHaveChildren && node.FirstChild != null;
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
					iteratedNsNames.Clear ();
				else
					iteratedNsNames.Add (value.Name);
				nsNode = value;
			}
		}

		public override string LocalName {
			get {
				XmlAttribute nsNode = NsNode;
				if (nsNode != null) {
					if (nsNode == nsNodeXml)
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
			get {
				return document.NameTable;
			}
		}

		public override XPathNodeType NodeType {
			get { return (NsNode != null) ? XPathNodeType.Namespace : node.XPathNodeType; }
		}

		public override string Prefix {
			get { return (NsNode != null) ? String.Empty : node.Prefix; }
		}

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
					for (XmlNode n = node.NextSibling; n != null; n = n.NextSibling) {
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
					return NsNode == nsNodeXml ? XmlnsXML : NsNode.Value;
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
			if (iteratedNsNames.Contains (name))
				return true;
			// default namespace erasure - just add name and never return this node
			if (ns == String.Empty) {
				iteratedNsNames.Add ("xmlns");
				return true;
			}

			return false;
		}

		public override XPathNavigator Clone ()
		{
			XmlDocumentNavigator clone = new XmlDocumentNavigator (node, nsNodeXml);
			clone.nsNode = nsNode;
			clone.iteratedNsNames = (ArrayList) iteratedNsNames.Clone ();
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
					&& NsNode == otherDocumentNavigator.NsNode;
			return false;
		}

		public override bool MoveTo (XPathNavigator other)
		{
			XmlDocumentNavigator otherDocumentNavigator = other as XmlDocumentNavigator;
			if (otherDocumentNavigator != null) {
				if (document == otherDocumentNavigator.document) {
					node = otherDocumentNavigator.node;
					NsNode = otherDocumentNavigator.NsNode;
					return true;
				}
			}
			return false;
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			if (node.Attributes != null) {
				for (int i = 0; i < node.Attributes.Count; i++) {
					XmlAttribute attr = node.Attributes [i];
					if (attr.LocalName == localName
						&& attr.NamespaceURI == namespaceURI) {
						node = attr;
						NsNode = null;
						return true;
					}
				}
			}
			return false;
		}

		public override bool MoveToFirst ()
		{
			if (NsNode == null && node.NodeType != XmlNodeType.Attribute && node.ParentNode != null) {
				if (!MoveToParent ())
					return false;
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
				} else {
					XmlNode n2 = null;
					do {
						n2 = node.FirstChild;
						if (node.NodeType != XmlNodeType.EntityReference)
							break;
						n2 = node.NextSibling;
					} while (n2 != null);
					if (n2 == null)
						return false;
					node = n2;
				}
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
					for (int i = 0; i < el.Attributes.Count; i++) {
						XmlAttribute attr = el.Attributes [i];
						if (attr.NamespaceURI == Xmlns) {
							if (CheckNsNameAppearance (attr.Name, attr.Value))
								continue;
							NsNode = attr;
							return true;
						}
					}
					if (namespaceScope == XPathNamespaceScope.Local)
						return false;
					el = el.ParentNode as XmlElement;
				} while (el != null);
			}

			if (namespaceScope == XPathNamespaceScope.All) {
				if (CheckNsNameAppearance (nsNodeXml.Name, nsNodeXml.Value))
					return false;
				NsNode = nsNodeXml;
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
				NsNode = nsNodeXml;
				return true;
			}

			if (NodeType != XPathNodeType.Element)
				return false;

			XmlElement el = node as XmlElement;
			if (node.Attributes != null) {
				do {
					for (int i = 0; i < el.Attributes.Count; i++) {
						XmlAttribute attr = el.Attributes [i];
						if (attr.NamespaceURI == Xmlns && attr.Name == name) {
							NsNode = attr;
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
			if (NsNode != null)
				return false;

			XmlNode n = node;
			if (NodeType == XPathNodeType.Text) {
				do {
					n = n.NextSibling;
					if (n == null)
						return false;
					switch (n.NodeType) {
					case XmlNodeType.CDATA:
					case XmlNodeType.EntityReference:
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Text:
					case XmlNodeType.Whitespace:
						continue;
					default:
						break;
					}
					break;
				} while (true);
			} else {
				n = n.NextSibling;
				if (n == null)
					return false;
			}

			if (n.ParentNode != null && n.ParentNode.NodeType == XmlNodeType.Document) {
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
			} else {
				while (n != null) {
					if (n.NodeType != XmlNodeType.EntityReference)
						break;
					n = n.NextSibling;
				}
				if (n != null)
					node = n;
				else
					return false;
			}
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
			if (NsNode == nsNodeXml)
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
			owner = owner.ParentNode as XmlElement;
			while (owner != null) {
				for (int i = 0; i < owner.Attributes.Count; i++) {
					XmlAttribute attr = owner.Attributes [i];
					if (attr.NamespaceURI == Xmlns) {
						if (CheckNsNameAppearance (attr.Name, attr.Value))
							continue;
						NsNode = attr;
						return true;
					}
				}
				owner = owner.ParentNode as XmlElement;
			}

			if (namespaceScope == XPathNamespaceScope.All) {
				if (CheckNsNameAppearance (nsNodeXml.Name, nsNodeXml.Value))
					return false;
				NsNode = nsNodeXml;
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
			} else if (node.ParentNode != null) {
				node = node.ParentNode;
				NsNode = null;
				return true;
			}
			return false;
		}

		public override bool MoveToPrevious ()
		{
			if (NsNode != null)
				return false;

			if (node.PreviousSibling != null) {
				if (node.ParentNode != null && node.ParentNode.NodeType == XmlNodeType.Document) {
					XmlNode n = node.PreviousSibling;
					while (n != null) {
						switch (n.NodeType) {
						case XmlNodeType.DocumentType:
						case XmlNodeType.XmlDeclaration:
							n = n.PreviousSibling;
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
					node = node.PreviousSibling;
				
				return true;
			}
			else
				return false;
		}

		public override void MoveToRoot ()
		{
			XmlAttribute attr = node as XmlAttribute;
			XmlNode tmp = attr != null ? attr.OwnerElement : node;
			while (tmp.ParentNode != null)
				tmp = tmp.ParentNode;
			node = tmp;
			NsNode = null;
		}

		private XmlNode Node { get { return NsNode != null ? NsNode : node; } }

		XmlNode IHasXmlNode.GetNode ()
		{
			return Node;
		}

		#endregion
	}
}
