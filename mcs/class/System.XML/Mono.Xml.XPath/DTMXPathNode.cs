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
