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
//#define DTM_CLASS

using System;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{
#if OUTSIDE_SYSTEM_XML
	public
#else
	internal
#endif
#if DTM_CLASS
		class DTMXPathLinkedNode
#else
		struct DTMXPathLinkedNode
#endif
	{
		public int FirstChild;
		public int Parent;
		public int PreviousSibling;
		public int NextSibling;
		public int FirstAttribute;
		public int FirstNamespace;
		public int Depth;
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

#if OUTSIDE_SYSTEM_XML
	public
#else
	internal
#endif
#if DTM_CLASS
		class DTMXPathAttributeNode
#else
		struct DTMXPathAttributeNode
#endif
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

#if OUTSIDE_SYSTEM_XML
	public
#else
	internal
#endif
#if DTM_CLASS
		class DTMXPathNamespaceNode
#else
		struct DTMXPathNamespaceNode
#endif
	{
		public int DeclaredElement;
		public int NextNamespace;
		public string Name;
		public string Namespace;
	}
}
