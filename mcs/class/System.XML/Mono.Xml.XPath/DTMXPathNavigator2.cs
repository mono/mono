//
// Mono.Xml.XPath.DTMXPathNavigator2
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
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
		class DTMXPathNavigator2 : XPathNavigator, IXmlLineInfo
	{

		public DTMXPathNavigator2 (DTMXPathDocument2 document)
		{
			this.MoveToRoot ();
			this.document = document;
		}

		// Copy constructor including position informations.
		public DTMXPathNavigator2 (DTMXPathNavigator2 org)
		{
			document = org.document;
			currentIsNode = org.currentIsNode;
			currentIsAttr = org.currentIsAttr;

			currentNode = org.currentNode;
			currentAttr = org.currentAttr;
			currentNs = org.currentNs;
		}

		XmlNameTable nameTable {
			get { return document.NameTable; }
		}

		// Created XPathDocument. This is used to identify the origin of the navigator.
		DTMXPathDocument2 document;

		DTMXPathLinkedNode2 [] nodes {
			get { return document.Nodes; }
		}
		DTMXPathAttributeNode2 [] attributes {
			get { return document.Attributes; }
		}
		DTMXPathNamespaceNode2 [] namespaces {
			get { return document.Namespaces; }
		}
		string [] atomicStringPool {
			get { return document.AtomicStringPool; }
		}
		string [] nonAtomicStringPool {
			get { return document.NonAtomicStringPool; }
		}

		// ID table
		Hashtable idTable {
			get { return document.IdTable; }
		}

		bool currentIsNode;
		bool currentIsAttr;

		int currentNode;
		int currentAttr;
		int currentNs;

#region Properties

		public override string BaseURI {
			get { return atomicStringPool [nodes [currentNode].BaseURI]; }
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
					currentIsNode ? nodes [currentNode].LineNumber :
					namespaces [currentNs].LineNumber;
			}
		}

		int IXmlLineInfo.LinePosition {
			get {
				return currentIsAttr ? attributes [currentAttr].LinePosition :
					currentIsNode ? nodes [currentNode].LinePosition :
					namespaces [currentNs].LinePosition;
			}
		}

		public override string LocalName {
			get {
				if (currentIsNode)
					return atomicStringPool [nodes [currentNode].LocalName];
				else if (currentIsAttr)
					return atomicStringPool [attributes [currentAttr].LocalName];
				else
					return atomicStringPool [namespaces [currentNs].Name];
			}
		}

		// It maybe scarcely used, so I decided to compute it always.
		public override string Name {
			get {
				string prefix;
				string localName;
				if (currentIsNode) {
					prefix = atomicStringPool [nodes [currentNode].Prefix];
					localName = atomicStringPool [nodes [currentNode].LocalName];
				} else if (currentIsAttr) {
					prefix = atomicStringPool [attributes [currentAttr].Prefix];
					localName = atomicStringPool [attributes [currentAttr].LocalName];
				} else
					return atomicStringPool [namespaces [currentNs].Name];

				if (prefix != "")
					return prefix + ':' + localName;
				else
					return localName;
			}
		}

		public override string NamespaceURI {
			get {
				if (currentIsNode)
					return atomicStringPool [nodes [currentNode].NamespaceURI];
				if (currentIsAttr)
					return atomicStringPool [attributes [currentAttr].NamespaceURI];
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
					return atomicStringPool [nodes [currentNode].Prefix];
				else if (currentIsAttr)
					return atomicStringPool [attributes [currentAttr].Prefix];
				return String.Empty;
			}
		}

		public override string Value {
			get {
				if (currentIsAttr)
					return nonAtomicStringPool [attributes [currentAttr].Value];
				else if (!currentIsNode)
					return atomicStringPool [namespaces [currentNs].Namespace];
				
				switch (nodes [currentNode].NodeType) {
				case XPathNodeType.Comment:
				case XPathNodeType.ProcessingInstruction:
				case XPathNodeType.Text:
				case XPathNodeType.Whitespace:
				case XPathNodeType.SignificantWhitespace:
					return nonAtomicStringPool [nodes [currentNode].Value];
				}

				// Element - collect all content values
				int iter = nodes [currentNode].FirstChild;
				if (iter == 0)
					return String.Empty;

				StringBuilder builder = null;
				BuildValue (iter, ref builder);
				return builder == null ? String.Empty : builder.ToString ();
			}
		}

		void BuildValue (int iter, ref StringBuilder valueBuilder)
		{
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
					if (valueBuilder == null)
						valueBuilder = new StringBuilder ();
					valueBuilder.Append (nonAtomicStringPool [nodes [iter].Value]);
					break;
				}
				iter++;
			}
		}

		public override string XmlLang {
			get { return atomicStringPool [nodes [currentNode].XmlLang]; }
		}

#endregion

#region Methods

		public override XPathNavigator Clone ()
		{
			return new DTMXPathNavigator2 (this);
		}

		public override XmlNodeOrder ComparePosition (XPathNavigator nav)
		{
			DTMXPathNavigator2 another = nav as DTMXPathNavigator2;

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
					if (atomicStringPool [attributes [cur].LocalName] == localName && atomicStringPool [attributes [cur].NamespaceURI] == namespaceURI)
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
			return (attr != 0) ? nonAtomicStringPool [attributes [attr].Value] : String.Empty;
		}

		public override string GetNamespace (string name)
		{
			if (currentIsNode && nodes [currentNode].NodeType == XPathNodeType.Element) {
				int nsNode = nodes [currentNode].FirstNamespace;
				while (nsNode != 0) {
					if (atomicStringPool [namespaces [nsNode].Name] == name)
						return atomicStringPool [namespaces [nsNode].Namespace];
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
			DTMXPathNavigator2 another = nav as DTMXPathNavigator2;

			if (another == null || another.document != this.document)
				return false;

			if (another.currentNode == currentNode)
				return !another.currentIsNode;
			int tmp = nodes [another.currentNode].Parent;

			// ancestors must appear in prior on the node list.
			if (tmp < currentNode)
				return false;

			while (tmp != 0) {
				if (tmp == currentNode)
					return true;
				tmp = nodes [tmp].Parent;
			}
			return false;
		}

		public override bool IsSamePosition (XPathNavigator other)
		{
			DTMXPathNavigator2 another = other as DTMXPathNavigator2;

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
			DTMXPathNavigator2 another = other as DTMXPathNavigator2;

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

			cur = nodes [cur].Parent;
			cur = nodes [cur].FirstChild;
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
				&& namespaces [cur].Namespace == XmlNamespaces.IndexXML)
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
				if (atomicStringPool [namespaces [cur].Name] == name) {
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
}
