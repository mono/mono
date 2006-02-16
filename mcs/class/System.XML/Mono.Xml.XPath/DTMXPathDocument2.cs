//
// Mono.Xml.XPath.DTMXPathDocument2
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
using System.Xml;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{
#if OUTSIDE_SYSTEM_XML
	public
#else
	internal
#endif
		class DTMXPathDocument2 : IXPathNavigable
	{

		public DTMXPathDocument2 (XmlNameTable nameTable,
			DTMXPathLinkedNode2 [] nodes,
			DTMXPathAttributeNode2 [] attributes,
			DTMXPathNamespaceNode2 [] namespaces,
			string [] atomicStringPool,
			string [] nonAtomicStringPool,
			Hashtable idTable)
		{
			this.Nodes = nodes;
			this.Attributes = attributes;
			this.Namespaces = namespaces;
			this.AtomicStringPool = atomicStringPool;
			this.NonAtomicStringPool = nonAtomicStringPool;
			this.IdTable = idTable;
			this.NameTable = nameTable;

			root = new DTMXPathNavigator2 (this);
		}

		public XPathNavigator CreateNavigator ()
		{
			return root.Clone ();
		}

		readonly XPathNavigator root;

		internal readonly XmlNameTable NameTable;

		internal readonly DTMXPathLinkedNode2 [] Nodes;
		internal readonly DTMXPathAttributeNode2 [] Attributes;
		internal readonly DTMXPathNamespaceNode2 [] Namespaces;

		// String pool
		internal readonly string [] AtomicStringPool;
		internal readonly string [] NonAtomicStringPool;

		// ID table
		internal readonly Hashtable IdTable;
	}
}

