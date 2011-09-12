//
// Mono.Xml.XPath.DTMXPathDocumentBuilder2
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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
using System.IO;
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
	class DTMXPathDocumentBuilder2
	{
		public DTMXPathDocumentBuilder2 (string url)
			: this (url, XmlSpace.None, 200)
		{
		}

		public DTMXPathDocumentBuilder2 (string url, XmlSpace space)
			: this (url, space, 200)
		{
		}

		public DTMXPathDocumentBuilder2 (string url, XmlSpace space, int defaultCapacity)
		{
			XmlReader r = null;
			try {
				r = new XmlTextReader (url);
				Init (r, space, defaultCapacity);
			} finally {
				if (r != null)
					r.Close ();
			}
		}

		public DTMXPathDocumentBuilder2 (XmlReader reader)
			: this (reader, XmlSpace.None, 200)
		{
		}

		public DTMXPathDocumentBuilder2 (XmlReader reader, XmlSpace space)
			: this (reader, space, 200)
		{
		}

		public DTMXPathDocumentBuilder2 (XmlReader reader, XmlSpace space, int defaultCapacity)
		{
			Init (reader, space, defaultCapacity);
		}

		private void Init (XmlReader reader, XmlSpace space, int defaultCapacity)
		{
			xmlReader = reader;
			this.validatingReader = reader as XmlValidatingReader;
			lineInfo = reader as IXmlLineInfo;
			this.xmlSpace = space;
			this.nameTable = reader.NameTable;
			nodeCapacity = defaultCapacity;
			attributeCapacity = nodeCapacity;
			nsCapacity = 10;
			idTable = new Hashtable ();

			nodes = new DTMXPathLinkedNode2 [nodeCapacity];
			attributes = new DTMXPathAttributeNode2 [attributeCapacity];
			namespaces = new DTMXPathNamespaceNode2 [nsCapacity];
			atomicStringPool = new string [20];
			nonAtomicStringPool = new string [20];

			Compile ();
		}
		
		XmlReader xmlReader;
		XmlValidatingReader validatingReader;
		XmlSpace xmlSpace;
		XmlNameTable nameTable;
		IXmlLineInfo lineInfo;
		int nodeCapacity;
		int attributeCapacity;
		int nsCapacity;

		// Linked Node
		DTMXPathLinkedNode2 [] nodes;

		// Attribute
		DTMXPathAttributeNode2 [] attributes;

		// NamespaceNode
		DTMXPathNamespaceNode2 [] namespaces;

		// String pool
		string [] atomicStringPool;
		int atomicIndex;
		string [] nonAtomicStringPool;
		int nonAtomicIndex;

		// idTable [string value] -> int nodeId
		Hashtable idTable;

		int nodeIndex;
		int attributeIndex;
		int nsIndex;

		// for attribute processing; should be reset per each element.
		bool hasAttributes;
		bool hasLocalNs;
		int attrIndexAtStart;
		int nsIndexAtStart;

		int lastNsInScope;
		bool skipRead = false;

		int [] parentStack = new int [10];
		int parentStackIndex = 0;

		public DTMXPathDocument2 CreateDocument ()
		{
			return new DTMXPathDocument2 (nameTable,
				nodes,
				attributes,
				namespaces,
				atomicStringPool,
				nonAtomicStringPool,
				idTable
			);
		}

		public void Compile ()
		{
			// string pool index 0 to 3 are fixed.
			atomicStringPool [0] = nonAtomicStringPool [0] = "";
			atomicStringPool [1] = nonAtomicStringPool [1] = null;
			atomicStringPool [2] = nonAtomicStringPool [2] = XmlNamespaces.XML;
			atomicStringPool [3] = nonAtomicStringPool [3] = XmlNamespaces.XMLNS;
			atomicIndex = nonAtomicIndex = 4;

			// index 0 is dummy. No node (including Root) is assigned to this index
			// So that we can easily compare index != 0 instead of index < 0.
			// (Difference between jnz or jbe in 80x86.)
			AddNode (0, 0, 0, XPathNodeType.All, 0, false, 0, 0, 0, 0, 0, 0, 0, 0);
			nodeIndex++;
			AddAttribute (0, 0, 0, 0, 0, 0, 0);
			AddNsNode (0, 0, 0, 0);
			nsIndex++;
			AddNsNode (1, AtomicIndex ("xml"), AtomicIndex (XmlNamespaces.XML), 0);

			// add root.
			AddNode (0, 0, 0, XPathNodeType.Root, AtomicIndex (xmlReader.BaseURI), false, 0, 0, 0, 0, 0, 1, 0, 0);

			this.nodeIndex = 1;
			this.lastNsInScope = 1;
			parentStack [0] = nodeIndex;

			if (xmlReader.ReadState == ReadState.Initial)
				xmlReader.Read ();
			int startDepth = xmlReader.Depth;
			do {
				Read ();
			} while (skipRead || xmlReader.Read () && xmlReader.Depth >= startDepth);
			SetNodeArrayLength (nodeIndex + 1);
			SetAttributeArrayLength (attributeIndex + 1);
			SetNsArrayLength (nsIndex + 1);

			string [] newArr = new string [atomicIndex];
			Array.Copy (atomicStringPool, newArr, atomicIndex);
			atomicStringPool = newArr;

			newArr = new string [nonAtomicIndex];
			Array.Copy (nonAtomicStringPool, newArr, nonAtomicIndex);
			nonAtomicStringPool = newArr;

			xmlReader = null;	// It is no more required.
		}

		public void Read ()
		{
			skipRead = false;

			int parent = parentStack [parentStackIndex];
			int prevSibling = nodeIndex;

			switch (xmlReader.NodeType) {
			case XmlNodeType.Element:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Comment:
			case XmlNodeType.Text:
			case XmlNodeType.ProcessingInstruction:
				break;
			case XmlNodeType.EndElement:
				int endedNode = parentStack [parentStackIndex];
				AdjustLastNsInScope (endedNode);
				parentStackIndex--;
				return;
			default:
				// No operations. Doctype, EntityReference, 
				return;
			}

			string value = null;
			XPathNodeType nodeType = XPathNodeType.Root; // dummy
			bool containsCDATA = false;

			switch (xmlReader.NodeType) {
			case XmlNodeType.Element:
				nodeType = XPathNodeType.Element;
				goto case XmlNodeType.None;
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Whitespace:
			case XmlNodeType.CDATA:
			case XmlNodeType.Text:
				skipRead = true;
				do {
					switch (xmlReader.NodeType) {
					case XmlNodeType.Whitespace:
						switch (nodeType) {
						case XPathNodeType.Root:
							nodeType = XPathNodeType.Whitespace;
							break;
						}
						// whitespaces after CDATA are ignored
						if (!containsCDATA)
							value += xmlReader.Value;
						continue;
					case XmlNodeType.SignificantWhitespace:
						if (nodeType == XPathNodeType.Root ||
						    nodeType == XPathNodeType.Whitespace)
							nodeType = XPathNodeType.SignificantWhitespace;
						value += xmlReader.Value;
						continue;
					case XmlNodeType.CDATA:
						// whitespaces before CDATA are ignored
						if (nodeType != XPathNodeType.Text)
							value = String.Empty;
						containsCDATA = true;
						goto case XmlNodeType.Text;
					case XmlNodeType.Text:
						nodeType = XPathNodeType.Text;
						value += xmlReader.Value;
						continue;
					}
					break;
				} while (xmlReader.Read ());
				goto case XmlNodeType.None;
			case XmlNodeType.None:
				if (nodeType == XPathNodeType.Root ||
				    nodeType == XPathNodeType.Whitespace && xmlSpace != XmlSpace.Preserve)
					return; // do not process as a node.

				// prepare a slot for new node.
				if (parent == nodeIndex)
					prevSibling = 0;
				else
					while (nodes [prevSibling].Parent != parent)
						prevSibling = nodes [prevSibling].Parent;

				nodeIndex++;

				if (prevSibling != 0)
					nodes [prevSibling].NextSibling = nodeIndex;
				if (parentStack [parentStackIndex] == nodeIndex - 1)
					nodes [parent].FirstChild = nodeIndex;

				// append new node.
				if (nodeType == XPathNodeType.Element) {
					ProcessElement (parent, prevSibling);
					break;
				}
				AddNode (parent,
					0,
					prevSibling,
					nodeType,
					AtomicIndex (xmlReader.BaseURI),
					xmlReader.IsEmptyElement,
					skipRead ? 0 : AtomicIndex (xmlReader.LocalName),	// for PI
					skipRead ? 0 : AtomicIndex (xmlReader.NamespaceURI),	// for PI
					AtomicIndex (xmlReader.Prefix),
					value == null ? 0 : NonAtomicIndex (value),
					AtomicIndex (xmlReader.XmlLang),
					nsIndex,
					lineInfo != null ? lineInfo.LineNumber : 0,
					lineInfo != null ? lineInfo.LinePosition : 0);
				break;
			case XmlNodeType.Comment:
				value = xmlReader.Value;
				nodeType = XPathNodeType.Comment;
				goto case XmlNodeType.None;
			case XmlNodeType.ProcessingInstruction:
				value = xmlReader.Value;
				nodeType = XPathNodeType.ProcessingInstruction;
				goto case XmlNodeType.None;
			}
		}

		private void ProcessElement (int parent, int previousSibling)
		{
			WriteStartElement (parent, previousSibling);

			// process namespaces and attributes.
			if (xmlReader.MoveToFirstAttribute ()) {
				do {
					string prefix = xmlReader.Prefix;
					string ns = xmlReader.NamespaceURI;
					if (ns == XmlNamespaces.XMLNS)
						ProcessNamespace ((prefix == null || prefix == String.Empty) ? "" : xmlReader.LocalName, xmlReader.Value);
					else
						ProcessAttribute (prefix, xmlReader.LocalName, ns, xmlReader.Value);

				} while (xmlReader.MoveToNextAttribute ());
				xmlReader.MoveToElement ();
			}

			CloseStartElement ();
		}

		private void PrepareStartElement (int previousSibling)
		{
			hasAttributes = false;
			hasLocalNs = false;
			attrIndexAtStart = attributeIndex;
			nsIndexAtStart = nsIndex;
			AdjustLastNsInScope (previousSibling);
		}

		private void AdjustLastNsInScope (int target)
		{
			while (namespaces [lastNsInScope].DeclaredElement == target) {
				lastNsInScope = namespaces [lastNsInScope].NextNamespace;
			}
		}

		private void WriteStartElement (int parent, int previousSibling)
		{
			PrepareStartElement (previousSibling);

			AddNode (parent,
				0, // dummy:firstAttribute
				previousSibling,
				XPathNodeType.Element,
				AtomicIndex (xmlReader.BaseURI),
				xmlReader.IsEmptyElement,
				AtomicIndex (xmlReader.LocalName),
				AtomicIndex (xmlReader.NamespaceURI),
				AtomicIndex (xmlReader.Prefix),
				0,	// Element has no internal value.
				AtomicIndex (xmlReader.XmlLang),
				lastNsInScope,
				lineInfo != null ? lineInfo.LineNumber : 0,
				lineInfo != null ? lineInfo.LinePosition : 0);

		}

		private void CloseStartElement ()
		{
			if (attrIndexAtStart != attributeIndex)
				nodes [nodeIndex].FirstAttribute = attrIndexAtStart + 1;
			if (nsIndexAtStart != nsIndex) {
				nodes [nodeIndex].FirstNamespace = nsIndex;
				if (!xmlReader.IsEmptyElement)
					lastNsInScope = nsIndex;
			}

			if (!nodes [nodeIndex].IsEmptyElement) {
				parentStackIndex++;
				if (parentStack.Length == parentStackIndex) {
					int [] tmp = new int [parentStackIndex * 2];
					Array.Copy (parentStack, tmp, parentStackIndex);
					parentStack = tmp;
				}
				parentStack [parentStackIndex] = nodeIndex;
			}
		}

		private void ProcessNamespace (string prefix, string ns)
		{
			int nextTmp = hasLocalNs ?
				nsIndex : nodes [nodeIndex].FirstNamespace;

			nsIndex++;

			this.AddNsNode (nodeIndex,
				AtomicIndex (prefix),
				AtomicIndex (ns),
				nextTmp);
			hasLocalNs = true;
		}

		private void ProcessAttribute (string prefix, string localName, string ns, string value)
		{
			attributeIndex ++;

			this.AddAttribute (nodeIndex,
				AtomicIndex (localName),
				AtomicIndex (ns), 
				prefix != null ? AtomicIndex (prefix) : 0, 
				NonAtomicIndex (value),
				lineInfo != null ? lineInfo.LineNumber : 0,
				lineInfo != null ? lineInfo.LinePosition : 0);
			if (hasAttributes)
				attributes [attributeIndex - 1].NextAttribute = attributeIndex;
			else
				hasAttributes = true;

			// Identity infoset
			if (validatingReader != null) {
				XmlSchemaDatatype dt = validatingReader.SchemaType as XmlSchemaDatatype;
				if (dt == null) {
					XmlSchemaType xsType = validatingReader.SchemaType as XmlSchemaType;
					if (xsType != null)
						dt = xsType.Datatype;
				}
				if (dt != null && dt.TokenizedType == XmlTokenizedType.ID)
					idTable.Add (value, nodeIndex);
			}
		}

		private int AtomicIndex (string s)
		{
			if (s == "")
				return 0;
			if (s == null)
				return 1;
			int i = 2;
			for (; i < atomicIndex; i++)
				if (Object.ReferenceEquals (s, atomicStringPool [i]))
					return i;

			if (atomicIndex == atomicStringPool.Length) {
				string [] newArr = new string [atomicIndex * 2];
				Array.Copy (atomicStringPool, newArr, atomicIndex);
				atomicStringPool = newArr;
			}
			atomicStringPool [atomicIndex] = s;
			return atomicIndex++;
		}

		private int NonAtomicIndex (string s)
		{
			if (s == "")
				return 0;
			if (s == null)
				return 1;
			int i = 2;

			// Here we don't compare all the entries (sometimes it
			// goes extremely slow).
			int max = nonAtomicIndex < 100 ? nonAtomicIndex : 100;
			for (; i < max; i++)
				if (s == nonAtomicStringPool [i])
					return i;

			if (nonAtomicIndex == nonAtomicStringPool.Length) {
				string [] newArr = new string [nonAtomicIndex * 2];
				Array.Copy (nonAtomicStringPool, newArr, nonAtomicIndex);
				nonAtomicStringPool = newArr;
			}
			nonAtomicStringPool [nonAtomicIndex] = s;
			return nonAtomicIndex++;
		}

		private void SetNodeArrayLength (int size)
		{
			DTMXPathLinkedNode2 [] newArr = new DTMXPathLinkedNode2 [size];
			Array.Copy (nodes, newArr, System.Math.Min (size, nodes.Length));
			nodes = newArr;
		}

		private void SetAttributeArrayLength (int size)
		{
			DTMXPathAttributeNode2 [] newArr = 
				new DTMXPathAttributeNode2 [size];
			Array.Copy (attributes, newArr, System.Math.Min (size, attributes.Length));
			attributes = newArr;
		}

		private void SetNsArrayLength (int size)
		{
			DTMXPathNamespaceNode2 [] newArr =
				new DTMXPathNamespaceNode2 [size];
			Array.Copy (namespaces, newArr, System.Math.Min (size, namespaces.Length));
			namespaces = newArr;
		}

		// Here followings are skipped: firstChild, nextSibling, 
		public void AddNode (int parent, int firstAttribute, int previousSibling, XPathNodeType nodeType, int baseUri, bool isEmptyElement, int localName, int ns, int prefix, int value, int xmlLang, int namespaceNode, int lineNumber, int linePosition)
		{
			if (nodes.Length < nodeIndex + 1) {
				nodeCapacity *= 4;
				SetNodeArrayLength (nodeCapacity);
			}

#if DTM_CLASS
			nodes [nodeIndex] = new DTMXPathLinkedNode2 ();
#endif
			nodes [nodeIndex].FirstChild = 0;		// dummy
			nodes [nodeIndex].Parent = parent;
			nodes [nodeIndex].FirstAttribute = firstAttribute;
			nodes [nodeIndex].PreviousSibling = previousSibling;
			nodes [nodeIndex].NextSibling = 0;	// dummy
			nodes [nodeIndex].NodeType = nodeType;
			nodes [nodeIndex].BaseURI = baseUri;
			nodes [nodeIndex].IsEmptyElement = isEmptyElement;
			nodes [nodeIndex].LocalName = localName;
			nodes [nodeIndex].NamespaceURI = ns;
			nodes [nodeIndex].Prefix = prefix;
			nodes [nodeIndex].Value = value;
			nodes [nodeIndex].XmlLang = xmlLang;
			nodes [nodeIndex].FirstNamespace = namespaceNode;
			nodes [nodeIndex].LineNumber = lineNumber;
			nodes [nodeIndex].LinePosition = linePosition;
		}

		// Followings are skipped: nextAttribute,
		public void AddAttribute (int ownerElement, int localName, int ns, int prefix, int value, int lineNumber, int linePosition)
		{
			if (attributes.Length < attributeIndex + 1) {
				attributeCapacity *= 4;
				SetAttributeArrayLength (attributeCapacity);
			}

#if DTM_CLASS
			attributes [attributeIndex] = new DTMXPathAttributeNode2 ();
#endif
			attributes [attributeIndex].OwnerElement = ownerElement;
			attributes [attributeIndex].LocalName = localName;
			attributes [attributeIndex].NamespaceURI = ns;
			attributes [attributeIndex].Prefix = prefix;
			attributes [attributeIndex].Value = value;
			attributes [attributeIndex].LineNumber = lineNumber;
			attributes [attributeIndex].LinePosition = linePosition;
		}

		// Followings are skipped: nextNsNode (may be next attribute in the same element, or ancestors' nsNode)
		public void AddNsNode (int declaredElement, int name, int ns, int nextNs)
		{
			if (namespaces.Length < nsIndex + 1) {
				nsCapacity *= 4;
				SetNsArrayLength (nsCapacity);
			}

#if DTM_CLASS
			namespaces [nsIndex] = new DTMXPathNamespaceNode2 ();
#endif
			namespaces [nsIndex].DeclaredElement = declaredElement;
			namespaces [nsIndex].Name = name;
			namespaces [nsIndex].Namespace = ns;
			namespaces [nsIndex].NextNamespace = nextNs;
			if (lineInfo != null && lineInfo.HasLineInfo ()) {
				namespaces [nsIndex].LineNumber = lineInfo.LineNumber;
				namespaces [nsIndex].LinePosition = lineInfo.LinePosition;
			}
		}
	}
}

