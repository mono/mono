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
using System.Xml;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{

	public class DTMXPathDocument : IXPathNavigable
	{

#region ctor.

		public DTMXPathDocument (XmlNameTable nameTable,
			DTMXPathLinkedNode [] nodes,
			DTMXPathAttributeNode [] attributes,
			DTMXPathNamespaceNode [] namespaces,
			Hashtable idTable)
		{
			this.nameTable = nameTable;
			this.nodes = nodes;
			this.attributes = attributes;
			this.namespaces = namespaces;
			this.idTable = idTable;
		}

#endregion


#region Methods
		public XPathNavigator CreateNavigator ()
		{
			if (root == null) {
				root = new DTMXPathNavigator (this,
					nameTable,
					nodes,
					attributes,
					namespaces,
					idTable);
			}
			return root.Clone ();
		}

#endregion

		XmlNameTable nameTable;

		// Root XPathNavigator.
		DTMXPathNavigator root;

#region Immutable tree fields

		DTMXPathLinkedNode [] nodes = new DTMXPathLinkedNode [0];
		DTMXPathAttributeNode [] attributes = new DTMXPathAttributeNode [0];
		DTMXPathNamespaceNode [] namespaces = new DTMXPathNamespaceNode [0];

		// idTable [string value] -> int nodeId
		readonly Hashtable idTable;

#endregion

	}
}

