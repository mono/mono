//
// Mono.Xml.XPath.DTMXPathDocument
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{

	public class DTMXPathDocument : IXPathNavigable
	{

#region ctor.

		public DTMXPathDocument (XmlNameTable nameTable,
			int [] firstChild__,
			int [] parent__,
			int [] firstAttribute__,
			int [] previousSibling__,
			int [] nextSibling__,
			int [] depth__,
			int [] position__,
			XPathNodeType [] nodeType__,
			string [] baseUri__,
			bool [] isEmptyElement__,
			string [] localName__,
			string [] namespaceUri__,
			string [] prefix__,
			string [] value__,
			string [] xmlLang__,
			int [] namespaceNode__,
			object [] schemaType__,
			int [] ownerElement__,
			int [] nextAttribute__,
			string [] attrLocalName__,
			string [] attrPrefix__,
			string [] attrNsUri__,
			string [] attrValue__,
			object [] attrSchemaType__,
			int [] nsDeclaredElement__,
			int [] nextNsNode__,
			string [] nsNodeName__,
			string [] nsNodeUri__,
			Hashtable idTable__)
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

			idTable_ = idTable__;

			this.nameTable = nameTable;
		}

#endregion


#region Methods
		public XPathNavigator CreateNavigator ()
		{
			if (root == null) {
				root = new DTMXPathNavigator (this, nameTable, firstChild_, parent_, firstAttribute_, previousSibling_, nextSibling_, depth_, position_, nodeType_, baseUri_, isEmptyElement_, localName_, namespaceUri_, prefix_, value_, xmlLang_, namespaceNode_, schemaType_, ownerElement_, nextAttribute_, attrLocalName_, attrPrefix_, attrNsUri_, attrValue_, attrSchemaType_, nsDeclaredElement_, nextNsNode_, nsNodeName_, nsNodeUri_, idTable_);
				return root;
			} else
				return root.Clone ();
		}

#endregion

		XmlNameTable nameTable;

		// Root XPathNavigator.
		DTMXPathNavigator root;

#region Immutable tree fields

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
		bool [] isEmptyElement_;	// rotten MS spec that represents whether the original element is <foo/> or <foo></foo>.
		string [] localName_;
		string [] namespaceUri_;
		string [] prefix_;
		string [] value_;
		string [] xmlLang_;
		int [] namespaceNode_;
		object [] schemaType_;		// for XPath 2.0

		// Attribute
		int [] ownerElement_;
		int [] nextAttribute_;
		string [] attrLocalName_;
		string [] attrPrefix_;
		string [] attrNsUri_;
		string [] attrValue_;
		object [] attrSchemaType_;	// for XPath 2.0

		// NamespaceNode
		int [] nsDeclaredElement_;	// the Element that declares NS, or Root.
		int [] nextNsNode_;		// "next" is "ancestor" or previous xmlns attr.
		string [] nsNodeName_;		// NS prefix.
		string [] nsNodeUri_;		// NS uri.

		// idTable [string value] -> int nodeId
		readonly Hashtable idTable_;

#endregion

	}
}

