//
// Mono.Xml.XPath.DTMXPathNode.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//
// These classes represent each node of DTMXPathNavigator.
//

using System;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{
	public struct DTMXPathLinkedNode
	{
		public int FirstChild;
		public int Parent;
		public int PreviousSibling;
		public int NextSibling;
		public int FirstAttribute;
		public int FirstNamespace;
		public int Depth;
		public int Position;
		public XPathNodeType NodeType;
		public string BaseURI;
		public bool IsEmptyElement;
		public string LocalName;
		public string NamespaceURI;
		public string Prefix;
		public string Value;
		public string XmlLang;
		public int LineNumber;
		public int LinePosition;
	}

	public struct DTMXPathAttributeNode
	{
		public int OwnerElement;
		public int NextAttribute;
		public string LocalName;
		public string NamespaceURI;
		public string Prefix;
		public string Value;
		public object SchemaType;
		public int LineNumber;
		public int LinePosition;
	}

	public struct DTMXPathNamespaceNode
	{
		public int DeclaredElement;
		public int NextNamespace;
		public string Name;
		public string Namespace;
	}
}
