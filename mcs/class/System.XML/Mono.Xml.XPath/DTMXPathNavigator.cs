//
// Mono.Xml.XPath.DTMXPathNavigator
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{
	public class DTMXPathNavigator : XPathNavigator
	{

#region Copy of XPathDocument
		public DTMXPathNavigator (DTMXPathDocument document, XmlNameTable nameTable, int [] firstChild__, int [] parent__, int [] firstAttribute__, int [] previousSibling__, int [] nextSibling__, int [] depth__, int [] position__, XPathNodeType [] nodeType__, string [] baseUri__, bool [] isEmptyElement__, string [] localName__, string [] namespaceUri__, string [] prefix__, string [] value__, string [] xmlLang__, int [] namespaceNode__, object [] schemaType__, int [] ownerElement__, int [] nextAttribute__, string [] attrLocalName__, string [] attrPrefix__, string [] attrNsUri__, string [] attrValue__, object [] attrSchemaType__, int [] nsDeclaredElement__, int [] nextNsNode__, string [] nsNodeName__, string [] nsNodeUri__)
		{
			firstChild_ = firstChild__;
			parent_ = parent__;
			firstAttribute_ = firstAttribute__;
			previousSibling_ = previousSibling__;
			nextSibling_ = nextSibling__;
			depth_ = depth__;
			position_ = position__;
			nodeType_ = nodeType__;
			baseUri_ = baseUri__;
			isEmptyElement_ = isEmptyElement__;
			localName_ = localName__;
			namespaceUri_ = namespaceUri__;
			prefix_ = prefix__;
			value_ = value__;
			xmlLang_ = xmlLang__;
			namespaceNode_ = namespaceNode__;
			schemaType_ = schemaType__;

			// Attribute
			ownerElement_ = ownerElement__;
			nextAttribute_ = nextAttribute__;
			attrLocalName_ = attrLocalName__;
			attrPrefix_ = attrPrefix__;
			attrNsUri_ = attrNsUri__;
			attrValue_ = attrValue__;
			attrSchemaType_ = attrSchemaType__;

			// NamespaceNode
			nsDeclaredElement_ = nsDeclaredElement__;
			nextNsNode_ = nextNsNode__;
			nsNodeName_ = nsNodeName__;
			nsNodeUri_ = nsNodeUri__;

			this.nameTable = nameTable;
			this.MoveToRoot ();
			this.document = document;
		}

		// Copy constructor including position informations.
		public DTMXPathNavigator (DTMXPathNavigator org)
			: this (org.document, org.nameTable, org.firstChild_, org.parent_, org.firstAttribute_, org.previousSibling_, org.nextSibling_, org.depth_, org.position_, org.nodeType_, org.baseUri_, org.isEmptyElement_, org.localName_, org.namespaceUri_, org.prefix_, org.value_, org.xmlLang_, org.namespaceNode_, org.schemaType_, org.ownerElement_, org.nextAttribute_, org.attrLocalName_, org.attrPrefix_, org.attrNsUri_, org.attrValue_, org.attrSchemaType_, org.nsDeclaredElement_, org.nextNsNode_, org.nsNodeName_, org.nsNodeUri_)
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

		// Tree Node
		int [] firstChild_;
		int [] parent_;
		int [] firstAttribute_;
		int [] previousSibling_;
		int [] nextSibling_;
		int [] depth_;
		int [] position_;
		XPathNodeType [] nodeType_;
		string [] baseUri_;
		bool [] isEmptyElement_;	// MS lamespec that represents whether the original element is <foo/> or <foo></foo>.
		string [] localName_;
		string [] namespaceUri_;
		string [] prefix_;
		string [] value_;
		string [] xmlLang_;
		int [] namespaceNode_;
		object [] schemaType_;	// for XPath 2.0

		// Attribute
		int [] ownerElement_;
		int [] nextAttribute_;
		string [] attrLocalName_;
		string [] attrPrefix_;
		string [] attrNsUri_;
		string [] attrValue_;
		object [] attrSchemaType_;	// for XPath 2.0

		// NamespaceNode
		int [] nsDeclaredElement_;	// the Element that declares NS.
		int [] nextNsNode_;		// "next" = "ancestor".
		string [] nsNodeName_;		// NS prefix.
		string [] nsNodeUri_;		// NS uri.

		// ID-Key (considered xsd:keyref for XPath 2.0)
		Hashtable keyRefTable;	// [string key-name] -> idTable
					// idTable [string value] -> int nodeId
					// keyname="" for ID
#endregion

		bool currentIsNode;
		bool currentIsAttr;

		int currentNode;
		int currentAttr;
		int currentNs;

		StringBuilder valueBuilder = new StringBuilder ();

#region Ctor

		internal DTMXPathNavigator (XmlNameTable nt)
		{
			this.nameTable = nt;
		}

#endregion

#region Properties

		public override string BaseURI {
			get { return baseUri_ [currentNode]; }
		}

		public override bool HasAttributes {
			get { return currentIsNode ? firstAttribute_ [currentNode] != 0 : false; }
		}
		
		public override bool HasChildren {
			get { return currentIsNode ? firstChild_ [currentNode] != 0 : false; }
		}

		public override bool IsEmptyElement {
			get { return currentIsNode ? isEmptyElement_ [currentNode] : false; }
		}

		public override string LocalName {
			get { return currentIsNode ? localName_ [currentNode] : currentIsAttr ? attrLocalName_ [currentAttr] : nsNodeName_ [currentNs]; }
		}

		// It maybe scarcely used, so I decided to compute it.
		public override string Name {
			get {
				string prefix;
				string localName;
				if (currentIsNode) {
					prefix = prefix_ [currentNode];
					localName = localName_ [currentNode];
				} else if (currentIsAttr) {
					prefix = attrPrefix_ [currentAttr];
					localName = attrLocalName_ [currentAttr];
				} else
					return nsNodeName_ [currentNs];
				return prefix != "" ? String.Format ("{0}:{1}", prefix, localName) : localName;
			}
		}

		public override string NamespaceURI {
			get {
				if (currentIsNode)
					return namespaceUri_ [currentNode];
				if (currentIsAttr)
					return attrNsUri_ [currentAttr];
				return String.Empty;
			}
		}

		public override XmlNameTable NameTable {
			get { return nameTable; }
		}

		public override XPathNodeType NodeType {
			get {
				return currentIsNode ? nodeType_ [currentNode] 
				  : currentIsAttr ? XPathNodeType.Attribute
				  : XPathNodeType.Namespace;
			}
		}

		public override string Prefix {
			get {
				if (currentIsNode)
					return prefix_ [currentNode];
				else if (currentIsAttr)
					return attrPrefix_ [currentAttr];
				return String.Empty;
			}
		}

		public override string Value {
			get {
				if (currentIsAttr)
					return attrValue_ [currentAttr];
				else if (!currentIsNode)
					return nsNodeUri_ [currentNs];
				
				switch (nodeType_ [currentNode]) {
				case XPathNodeType.Comment:
				case XPathNodeType.ProcessingInstruction:
				case XPathNodeType.Text:
				case XPathNodeType.Whitespace:
				case XPathNodeType.SignificantWhitespace:
					return value_ [currentNode];
				}
				int iter = firstChild_ [currentNode];
				while (iter != 0 && iter < depth_.Length && depth_ [iter] > depth_ [currentNode]) {
					switch (nodeType_ [iter]) {
					case XPathNodeType.Comment:
					case XPathNodeType.ProcessingInstruction:
						break;
					default:
						valueBuilder.Append (value_ [iter]);
						break;
					}
					iter++;
				}
				string result = valueBuilder.ToString ();
				valueBuilder.Length = 0;
				return result;
			}
		}

		public override string XmlLang {
			get { return xmlLang_ [currentNode]; }
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

			int result = this.currentNode.CompareTo (another.currentNode);
			if (result != 0)
				return result > 0 ? XmlNodeOrder.After : XmlNodeOrder.Before;

			// another may attr or ns, 
			// and this may be also attr or ns.
			if (another.currentIsAttr) {
				if (this.currentIsAttr) {
					int resultAttr = this.currentAttr.CompareTo (another.currentAttr);
					return result == 0 ? XmlNodeOrder.Same : result > 0 ? XmlNodeOrder.After : XmlNodeOrder.Before;
				} else
					return XmlNodeOrder.Before;
			} else if (!another.currentIsNode) {
				if (!this.currentIsNode) {
					int resultNs = this.currentNs.CompareTo (another.currentNs);
					return result == 0 ? XmlNodeOrder.Same : result > 0 ? XmlNodeOrder.After : XmlNodeOrder.Before;
				} else
					return XmlNodeOrder.Before;
			} else
				return !another.currentIsNode ? XmlNodeOrder.Before : XmlNodeOrder.Same;
		}

		private int findAttribute (string localName, string namespaceURI)
		{
			if (currentIsNode && nodeType_ [currentNode] == XPathNodeType.Element) {
				int cur = firstAttribute_ [currentNode];
				while (cur != 0) {
					if (attrLocalName_ [cur] == localName && attrNsUri_ [cur] == namespaceURI)
						return cur;
					cur = nextAttribute_ [cur];
				}
			}
			return 0;
		}

		public override string GetAttribute (string localName,
			string namespaceURI)
		{
			int attr = findAttribute (localName, namespaceURI);
			return (attr != 0) ? attrValue_ [attr] : String.Empty;
		}

		public override string GetNamespace (string name)
		{
			if (currentIsNode && nodeType_ [currentNode] == XPathNodeType.Element) {
				int nsNode = namespaceNode_ [currentNode];
				while (nsNode != 0) {
					if (nsNodeName_ [nsNode] == name)
						return nsNodeUri_ [nsNode];
					nsNode = nextNsNode_ [nsNode];
				}
			}
			return String.Empty;
		}

		public override bool IsDescendant (XPathNavigator nav)
		{
			DTMXPathNavigator another = nav as DTMXPathNavigator;

			if (another == null || another.document != this.document)
				return false;

			if (ComparePosition (another) != XmlNodeOrder.After)
				return false;

			int end = nextSibling_ [currentNode];
			if (end == 0)
				end = nextSibling_.Length;
			return another.currentNode < end;
		}

		public override bool IsSamePosition (XPathNavigator other)
		{
			return ComparePosition (other) == XmlNodeOrder.Same;
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

			int cur = previousSibling_ [currentNode];
			if (cur == 0)
				return false;

			int next = cur;
			while (next != 0) {
				cur = next;
				next = previousSibling_ [cur];
			}
			currentNode = cur;
			currentIsNode = true;
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (!currentIsNode)
				return false;

			int first = firstAttribute_ [currentNode];
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

			int first = firstChild_ [currentNode];
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
					nsDeclaredElement_ [cur] != currentNode)
				return false;

			/*
			while (cur != 0) {
				if (namespaceScope != XPathNamespaceScope.All
					&& nsNodeUri_ [cur] == XmlNamespaces.XML) {
					cur = nextNsNode_ [cur];
					continue;
				} else
					break;
			}
			*/
			if (namespaceScope != XPathNamespaceScope.All
				&& nsNodeUri_ [cur] == XmlNamespaces.XML)
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
			int cur = namespaceNode_ [currentNode];
			return moveToSpecifiedNamespace (cur, namespaceScope);
		}

		public override bool MoveToId (string id)
		{
			return MoveToKeyRef ("", id);
		}

		// This is extension for XPath 2.0
		public virtual bool MoveToKeyRef (string key, string value)
		{
			Hashtable idTable = keyRefTable [key] as Hashtable;
			if (idTable == null)
				return false;
			if (!idTable.ContainsKey (key))
				return false;

			int target = (int) idTable [key];
			currentNode = target;
			currentIsNode = true;
			currentIsAttr = false;
			return true;
		}

		private void moveToNamespace (int nsNode)
		{
			currentIsNode = currentIsAttr = false;
			currentNs = nsNode;
		}

		public override bool MoveToNamespace (string name)
		{
			int cur = namespaceNode_ [currentNode];
			if (cur == 0)
				return false;

			while (cur != 0) {
				if (nsNodeName_ [cur] == name) {
					moveToNamespace (cur);
					return true;
				}
			}
			return false;
		}

		public override bool MoveToNext ()
		{
			if (currentIsAttr)
				return false;

			int next = nextSibling_ [currentNode];
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

			int next = nextAttribute_ [currentAttr];
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

			int cur = nextNsNode_ [currentNs];
			return moveToSpecifiedNamespace (cur, namespaceScope);
		}

		public override bool MoveToParent ()
		{
			if (!currentIsNode) {
				currentIsNode = true;
				currentIsAttr = false;
				return true;
			}

			int parent = parent_ [currentNode];
			if (parent == 0)	// It is root itself.
				return false;

			currentNode = parent;
			currentIsNode = true;
			currentIsAttr = false;
			return true;
		}

		public override bool MoveToPrevious ()
		{
			if (currentIsAttr)
				return false;

			int previous = previousSibling_ [currentNode];
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

		public string Debug {
			get {
				StringBuilder sb = new StringBuilder ();

				for (int i=0; i<this.nsDeclaredElement_.Length; i++) {
					sb.AppendFormat ("{0}: {1},{2} {3}/{4}\n", i,
						this.nsDeclaredElement_ [i], this.nextNsNode_ [i],
						this.nsNodeName_ [i], this.nsNodeUri_ [i]);
				}

				for (int i=0; i<this.localName_.Length; i++) {
					sb.AppendFormat ("{0}: {1}:{2} {3} {4} {5} {6} {7}\n", new object [] {i, this.prefix_ [i], this.localName_ [i], this.namespaceUri_ [i], this.namespaceNode_ [i], this.firstAttribute_ [i], this.firstChild_ [i], this.parent_ [i]});
				}

				for (int i=0; i<this.attrLocalName_.Length; i++) {
					sb.AppendFormat ("{0}: {1}:{2} {3} {4}\n", i, this.attrPrefix_ [i], 
						this.attrLocalName_ [i], this.attrNsUri_ [i], this.nextAttribute_ [i]);
				}

				return sb.ToString ();
			}
		}

	}

	public class XmlNamespaces
	{
		public const string XML = "http://www.w3.org/XML/1998/namespace";
		public const string XMLNS = "http://www.w3.org/2000/xmlns/";
	}
}
