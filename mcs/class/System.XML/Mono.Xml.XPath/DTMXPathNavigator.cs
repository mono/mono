//
// Mono.Xml.XPath.DTMXPathNavigator
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
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
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{
#if OUTSIDE_SYSTEM_XML
	public
#else
	internal
#endif
		class DTMXPathNavigator : XPathNavigator, IXmlLineInfo
	{

		public DTMXPathNavigator (DTMXPathDocument document,
			XmlNameTable nameTable, 
			DTMXPathLinkedNode [] nodes,
			DTMXPathAttributeNode [] attributes,
			DTMXPathNamespaceNode [] namespaces,
			Hashtable idTable)
		{
			this.nodes = nodes;
			this.attributes = attributes;
			this.namespaces = namespaces;
			this.idTable = idTable;
			this.nameTable = nameTable;
			this.MoveToRoot ();
			this.document = document;
		}

		// Copy constructor including position informations.
		public DTMXPathNavigator (DTMXPathNavigator org)
			: this (org.document, org.nameTable,
			org.nodes, org.attributes, org.namespaces,
			org.idTable)
		{
			currentIsNode = org.currentIsNode;
			currentIsAttr = org.currentIsAttr;

			currentNode = org.currentNode;
			currentAttr = org.currentAttr;
			currentNs = org.currentNs;
		}

		XmlNameTable nameTable;

		// Created XPathDocument. This is used to identify the origin of the navigator.
		DTMXPathDocument document;

		DTMXPathLinkedNode [] nodes;// = new DTMXPathLinkedNode [0];
		DTMXPathAttributeNode [] attributes;// = new DTMXPathAttributeNode [0];
		DTMXPathNamespaceNode [] namespaces;// = new DTMXPathNamespaceNode [0];

		// ID table
		Hashtable idTable;

		bool currentIsNode;
		bool currentIsAttr;

		int currentNode;
		int currentAttr;
		int currentNs;

		StringBuilder valueBuilder;

#region Ctor

		internal DTMXPathNavigator (XmlNameTable nt)
		{
			this.nameTable = nt;
		}

#endregion

#region Properties

		public override string BaseURI {
			get { return nodes [currentNode].BaseURI; }
		}

		public override bool HasAttributes {
			get { return currentIsNode ? nodes [currentNode].FirstAttribute != 0 : false; }
		}
		
		public override bool HasChildren {
			get { return currentIsNode ? nodes [currentNode].FirstChild != 0 : false; }
		}

		public override bool IsEmptyElement {
			get { return currentIsNode ? nodes [currentNode].IsEmptyElement : false; }
		}

		int IXmlLineInfo.LineNumber {
			get {
				return currentIsAttr ? attributes [currentAttr].LineNumber :
					nodes [currentNode].LineNumber;
			}
		}

		int IXmlLineInfo.LinePosition {
			get {
				return currentIsAttr ? attributes [currentAttr].LinePosition :
					nodes [currentNode].LinePosition;
			}
		}

		public override string LocalName {
			get {
				if (currentIsNode)
					return nodes [currentNode].LocalName;
				else if (currentIsAttr)
					return attributes [currentAttr].LocalName;
				else
					return namespaces [currentNs].Name;
			}
		}

		// It maybe scarcely used, so I decided to compute it always.
		public override string Name {
			get {
				string prefix;
				string localName;
				if (currentIsNode) {
					prefix = nodes [currentNode].Prefix;
					localName = nodes [currentNode].LocalName;
				} else if (currentIsAttr) {
					prefix = attributes [currentAttr].Prefix;
					localName = attributes [currentAttr].LocalName;
				} else
					return namespaces [currentNs].Name;

				if (prefix != "")
					return prefix + ':' + localName;
				else
					return localName;
			}
		}

		public override string NamespaceURI {
			get {
				if (currentIsNode)
					return nodes [currentNode].NamespaceURI;
				if (currentIsAttr)
					return attributes [currentAttr].NamespaceURI;
				return String.Empty;
			}
		}

		public override XmlNameTable NameTable {
			get { return nameTable; }
		}

		public override XPathNodeType NodeType {
			get {
				if (currentIsNode)
					return nodes [currentNode].NodeType;
				else if (currentIsAttr)
					return XPathNodeType.Attribute;
				else
					return XPathNodeType.Namespace;
			}
		}

		public override string Prefix {
			get {
				if (currentIsNode)
					return nodes [currentNode].Prefix;
				else if (currentIsAttr)
					return attributes [currentAttr].Prefix;
				return String.Empty;
			}
		}

		public override string Value {
			get {
				if (currentIsAttr)
					return attributes [currentAttr].Value;
				else if (!currentIsNode)
					return namespaces [currentNs].Namespace;
				
				switch (nodes [currentNode].NodeType) {
				case XPathNodeType.Comment:
				case XPathNodeType.ProcessingInstruction:
				case XPathNodeType.Text:
				case XPathNodeType.Whitespace:
				case XPathNodeType.SignificantWhitespace:
					return nodes [currentNode].Value;
				}

				// Element - collect all content values
				int iter = nodes [currentNode].FirstChild;
				if (iter == 0)
					return String.Empty;

				if (valueBuilder == null)
					valueBuilder = new StringBuilder ();
				else
					valueBuilder.Length = 0;

				int end = nodes [currentNode].NextSibling;
				if (end == 0) {
					int tmp = currentNode;
					do {
						tmp = nodes [tmp].Parent;
						end = nodes [tmp].NextSibling;
					} while (end == 0 && tmp != 0);
					if (end == 0)
						end = nodes.Length;
				}

				while (iter < end) {
					switch (nodes [iter].NodeType) {
					case XPathNodeType.Text:
					case XPathNodeType.SignificantWhitespace:
					case XPathNodeType.Whitespace:
						valueBuilder.Append (nodes [iter].Value);
						break;
					}
					iter++;
				}
				
				return valueBuilder.ToString ();
			}
		}

		public override string XmlLang {
			get { return nodes [currentNode].XmlLang; }
		}

#endregion

#region Methods

		public override XPathNavigator Clone ()
		{
			return new DTMXPathNavigator (this);
		}

		public override XmlNodeOrder ComparePosition (XPathNavigator nav)
		{
			DTMXPathNavigator another = nav as DTMXPathNavigator;

			if (another == null || another.document != this.document)
				return XmlNodeOrder.Unknown;

			if (currentNode > another.currentNode)
				return XmlNodeOrder.After;
			else if (currentNode < another.currentNode)
				return XmlNodeOrder.Before;

			// another may attr or ns, 
			// and this may be also attr or ns.
			if (another.currentIsAttr) {
				if (this.currentIsAttr) {
					if (currentAttr > another.currentAttr)
						return XmlNodeOrder.After;
					else if (currentAttr < another.currentAttr)
						return XmlNodeOrder.Before;
					else
						return XmlNodeOrder.Same;
				} else
					return XmlNodeOrder.Before;
			} else if (!another.currentIsNode) {
				if (!this.currentIsNode) {
					if (currentNs > another.currentNs)
						return XmlNodeOrder.After;
					else if (currentNs < another.currentNs)
						return XmlNodeOrder.Before;
					else
						return XmlNodeOrder.Same;
				} else
					return XmlNodeOrder.Before;
			} else
				return !another.currentIsNode ? XmlNodeOrder.Before : XmlNodeOrder.Same;
		}

		private int findAttribute (string localName, string namespaceURI)
		{
			if (currentIsNode && nodes [currentNode].NodeType == XPathNodeType.Element) {
				int cur = nodes [currentNode].FirstAttribute;
				while (cur != 0) {
					if (attributes [cur].LocalName == localName && attributes [cur].NamespaceURI == namespaceURI)
						return cur;
					cur = attributes [cur].NextAttribute;
				}
			}
			return 0;
		}

		public override string GetAttribute (string localName,
			string namespaceURI)
		{
			int attr = findAttribute (localName, namespaceURI);
			return (attr != 0) ? attributes [attr].Value : String.Empty;
		}

		public override string GetNamespace (string name)
		{
			if (currentIsNode && nodes [currentNode].NodeType == XPathNodeType.Element) {
				int nsNode = nodes [currentNode].FirstNamespace;
				while (nsNode != 0) {
					if (namespaces [nsNode].Name == name)
						return namespaces [nsNode].Namespace;
					nsNode = namespaces [nsNode].NextNamespace;
				}
			}
			return String.Empty;
		}

		bool IXmlLineInfo.HasLineInfo ()
		{
			return true;
		}

		public override bool IsDescendant (XPathNavigator nav)
		{
			DTMXPathNavigator another = nav as DTMXPathNavigator;

			if (another == null || another.document != this.document)
				return false;

			// Maybe we can improve here more efficiently by
			// comparing node indices.
			if (another.currentNode == currentNode)
				return !another.currentIsNode;
			int tmp = nodes [another.currentNode].Parent;
			while (tmp != 0) {
				if (tmp == currentNode)
					return true;
				tmp = nodes [tmp].Parent;
			}
			return false;
		}

		public override bool IsSamePosition (XPathNavigator other)
		{
			DTMXPathNavigator another = other as DTMXPathNavigator;

			if (another == null || another.document != this.document)
				return false;

			if (this.currentNode != another.currentNode ||
				this.currentIsAttr != another.currentIsAttr ||
				this.currentIsNode != another.currentIsNode)
				return false;

			if (currentIsAttr)
				return this.currentAttr == another.currentAttr;
			else if (!currentIsNode)
				return this.currentNs == another.currentNs;
			return true;
		}

		public override bool MoveTo (XPathNavigator other)
		{
			DTMXPathNavigator another = other as DTMXPathNavigator;

			if (another == null || another.document != this.document)
				return false;

			this.currentNode = another.currentNode;
			this.currentAttr = another.currentAttr;
			this.currentNs = another.currentNs;
			this.currentIsNode = another.currentIsNode;
			this.currentIsAttr = another.currentIsAttr;
			return true;
		}

		public override bool MoveToAttribute (string localName,
			string namespaceURI)
		{
			int attr = findAttribute (localName, namespaceURI);
			if (attr == 0)
				return false;

			currentAttr = attr;
			currentIsAttr = true;
			currentIsNode = false;
			return true;
		}

		public override bool MoveToFirst ()
		{
			if (currentIsAttr)
				return false;

			int cur = nodes [currentNode].PreviousSibling;
			if (cur == 0)
				return false;

			int next = cur;
			while (next != 0) {
				cur = next;
				next = nodes [cur].PreviousSibling;
			}
			currentNode = cur;
			currentIsNode = true;
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (!currentIsNode)
				return false;

			int first = nodes [currentNode].FirstAttribute;
			if (first == 0)
				return false;

			currentAttr = first;
			currentIsAttr = true;
			currentIsNode = false;
			return true;
		}

		public override bool MoveToFirstChild ()
		{
			if (!currentIsNode)
				return false;

			int first = nodes [currentNode].FirstChild;
			if (first == 0)
				return false;

			currentNode = first;
			return true;
		}
		
		private bool moveToSpecifiedNamespace (int cur,
			XPathNamespaceScope namespaceScope)
		{
			if (cur == 0)
				return false;

			if (namespaceScope == XPathNamespaceScope.Local &&
					namespaces [cur].DeclaredElement != currentNode)
				return false;

			if (namespaceScope != XPathNamespaceScope.All
				&& namespaces [cur].Namespace == XmlNamespaces.XML)
				return false;

			if (cur != 0) {
				moveToNamespace (cur);
				return true;
			}
			else
				return false;
		}

		public override bool MoveToFirstNamespace (
			XPathNamespaceScope namespaceScope)
		{
			if (!currentIsNode)
				return false;
			int cur = nodes [currentNode].FirstNamespace;
			return moveToSpecifiedNamespace (cur, namespaceScope);
		}

		// Note that this support is extension to XPathDocument.
		// XPathDocument does not support ID reference.
		public override bool MoveToId (string id)
		{
			if (idTable.ContainsKey (id)) {
				currentNode = (int) idTable [id];
				currentIsNode = true;
				currentIsAttr = false;
				return true;
			}
			else
				return false;
		}

		private void moveToNamespace (int nsNode)
		{
			currentIsNode = currentIsAttr = false;
			currentNs = nsNode;
		}

		public override bool MoveToNamespace (string name)
		{
			int cur = nodes [currentNode].FirstNamespace;
			if (cur == 0)
				return false;

			while (cur != 0) {
				if (namespaces [cur].Name == name) {
					moveToNamespace (cur);
					return true;
				}
				cur = namespaces [cur].NextNamespace;
			}
			return false;
		}

		public override bool MoveToNext ()
		{
			if (currentIsAttr)
				return false;

			int next = nodes [currentNode].NextSibling;
			if (next == 0)
				return false;
			currentNode = next;
			currentIsNode = true;
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (!currentIsAttr)
				return false;

			int next = attributes [currentAttr].NextAttribute;
			if (next == 0)
				return false;
			currentAttr = next;
			return true;
		}

		public override bool MoveToNextNamespace (
			XPathNamespaceScope namespaceScope)
		{
			if (currentIsAttr || currentIsNode)
				return false;

			int cur = namespaces [currentNs].NextNamespace;
			return moveToSpecifiedNamespace (cur, namespaceScope);
		}

		public override bool MoveToParent ()
		{
			if (!currentIsNode) {
				currentIsNode = true;
				currentIsAttr = false;
				return true;
			}

			int parent = nodes [currentNode].Parent;
			if (parent == 0)	// It is root itself.
				return false;

			currentNode = parent;
			return true;
		}

		public override bool MoveToPrevious ()
		{
			if (currentIsAttr)
				return false;

			int previous = nodes [currentNode].PreviousSibling;
			if (previous == 0)
				return false;
			currentNode = previous;
			currentIsNode = true;
			return true;
		}

		public override void MoveToRoot ()
		{
			currentNode = 1;	// root is 1.
			currentIsNode = true;
			currentIsAttr = false;
		}

#endregion
	}

	class XmlNamespaces
	{
		public const string XML = "http://www.w3.org/XML/1998/namespace";
		public const string XMLNS = "http://www.w3.org/2000/xmlns/";
		public const int IndexXML = 2;
		public const int IndexXMLNS = 3;
	}
}
