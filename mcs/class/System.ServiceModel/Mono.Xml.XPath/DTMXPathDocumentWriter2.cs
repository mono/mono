//
// Mono.Xml.XPath.DTMXPathDocumentWriter2
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
		class DTMXPathDocumentWriter2 : XmlWriter
	{
		public DTMXPathDocumentWriter2 (XmlNameTable nt, int defaultCapacity)
		{
			nameTable = nt == null ? new NameTable () : nt;
			nodeCapacity = defaultCapacity;
			attributeCapacity = nodeCapacity;
			nsCapacity = 10;
			idTable = new Hashtable ();

			nodes = new DTMXPathLinkedNode2 [nodeCapacity];
			attributes = new DTMXPathAttributeNode2 [attributeCapacity];
			namespaces = new DTMXPathNamespaceNode2 [nsCapacity];

			atomicStringPool = new string [20];
			nonAtomicStringPool = new string [20];

			Init ();
		}
		
		XmlNameTable nameTable;
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
		int [] parentStack = new int [10];
		int parentStackIndex = 0;

		// for attribute processing; should be reset per each element.
		bool hasAttributes;
		bool hasLocalNs;
		int attrIndexAtStart;
		int nsIndexAtStart;

		int lastNsInScope;

		// They are only used in Writer
		int prevSibling;
		WriteState state;
		bool openNamespace;
		bool isClosed;

		public DTMXPathDocument2 CreateDocument ()
		{
			if (!isClosed)
				Close ();
			return new DTMXPathDocument2 (nameTable,
				nodes,
				attributes,
				namespaces,
				atomicStringPool,
				nonAtomicStringPool,
				idTable
			);
		}

		public void Init ()
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
			AddNode (0, 0, 0, XPathNodeType.All, "", false, "", "", "", "", "", 0, 0, 0);
			nodeIndex++;
			AddAttribute (0, "", "", "", "", 0, 0);
			AddNsNode (0, "", "", 0);
			nsIndex++;
			AddNsNode (1, "xml", XmlNamespaces.XML, 0);

			// add root.
			AddNode (0, 0, 0, XPathNodeType.Root, "", false, "", "", "", "", "", 1, 0, 0);

			this.nodeIndex = 1;
			this.lastNsInScope = 1;
			parentStack [0] = nodeIndex;

			state = WriteState.Content;
		}

		private int GetParentIndex ()
		{
			return parentStack [parentStackIndex];
		}

		private int GetPreviousSiblingIndex ()
		{
			int parent = parentStack [parentStackIndex];
			if (parent == nodeIndex)
				return 0;
			int prevSibling = nodeIndex;
			while (nodes [prevSibling].Parent != parent)
				prevSibling = nodes [prevSibling].Parent;
			return prevSibling;
		}

		private void UpdateTreeForAddition ()
		{
			int parent = GetParentIndex ();
			prevSibling = GetPreviousSiblingIndex ();

			nodeIndex++;

			if (prevSibling != 0)
				nodes [prevSibling].NextSibling = nodeIndex;
			if (parent == nodeIndex - 1)
				nodes [parent].FirstChild = nodeIndex;
		}

		private void CloseStartElement ()
		{
			if (attrIndexAtStart != attributeIndex)
				nodes [nodeIndex].FirstAttribute = attrIndexAtStart + 1;
			if (nsIndexAtStart != nsIndex) {
				nodes [nodeIndex].FirstNamespace = nsIndex;
				lastNsInScope = nsIndex;
			}

			parentStackIndex++;
			if (parentStack.Length == parentStackIndex) {
				int [] tmp = new int [parentStackIndex * 2];
				Array.Copy (parentStack, tmp, parentStackIndex);
				parentStack = tmp;
			}
			parentStack [parentStackIndex] = nodeIndex;

			state = WriteState.Content;
		}

		#region Adding Nodes

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
		public void AddNode (int parent, int firstAttribute, int previousSibling, XPathNodeType nodeType, string baseUri, bool isEmptyElement, string localName, string ns, string prefix, string value, string xmlLang, int namespaceNode, int lineNumber, int linePosition)
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
			nodes [nodeIndex].BaseURI = AtomicIndex (baseUri);
			nodes [nodeIndex].IsEmptyElement = isEmptyElement;
			nodes [nodeIndex].LocalName = AtomicIndex (localName);
			nodes [nodeIndex].NamespaceURI = AtomicIndex (ns);
			nodes [nodeIndex].Prefix = AtomicIndex (prefix);
			nodes [nodeIndex].Value = NonAtomicIndex (value);
			nodes [nodeIndex].XmlLang = AtomicIndex (xmlLang);
			nodes [nodeIndex].FirstNamespace = namespaceNode;
			nodes [nodeIndex].LineNumber = lineNumber;
			nodes [nodeIndex].LinePosition = linePosition;
		}

		// Followings are skipped: nextAttribute,
		public void AddAttribute (int ownerElement, string localName, string ns, string prefix, string value, int lineNumber, int linePosition)
		{
			if (attributes.Length < attributeIndex + 1) {
				attributeCapacity *= 4;
				SetAttributeArrayLength (attributeCapacity);
			}

#if DTM_CLASS
			attributes [attributeIndex] = new DTMXPathAttributeNode2 ();
#endif
			attributes [attributeIndex].OwnerElement = ownerElement;
			attributes [attributeIndex].LocalName = AtomicIndex (localName);
			attributes [attributeIndex].NamespaceURI = AtomicIndex (ns);
			attributes [attributeIndex].Prefix = AtomicIndex (prefix);
			attributes [attributeIndex].Value = NonAtomicIndex (value);
			attributes [attributeIndex].LineNumber = lineNumber;
			attributes [attributeIndex].LinePosition = linePosition;
		}

		// Followings are skipped: nextNsNode (may be next attribute in the same element, or ancestors' nsNode)
		public void AddNsNode (int declaredElement, string name, string ns, int nextNs)
		{
			if (namespaces.Length < nsIndex + 1) {
				nsCapacity *= 4;
				SetNsArrayLength (nsCapacity);
			}

#if DTM_CLASS
			namespaces [nsIndex] = new DTMXPathNamespaceNode2 ();
#endif
			namespaces [nsIndex].DeclaredElement = declaredElement;
			namespaces [nsIndex].Name = AtomicIndex (name);
			namespaces [nsIndex].Namespace = AtomicIndex (ns);
			namespaces [nsIndex].NextNamespace = nextNs;
		}
		#endregion

		#region XmlWriter methods
		// They are not supported
		public override string XmlLang { get { return null; } }
		public override XmlSpace XmlSpace { get { return XmlSpace.None; } }

		public override WriteState WriteState { get { return state; } }

		public override void Close ()
		{
			// Fixup arrays
			SetNodeArrayLength (nodeIndex + 1);
			SetAttributeArrayLength (attributeIndex + 1);
			SetNsArrayLength (nsIndex + 1);

			string [] newArr = new string [atomicIndex];
			Array.Copy (atomicStringPool, newArr, atomicIndex);
			atomicStringPool = newArr;

			newArr = new string [nonAtomicIndex];
			Array.Copy (nonAtomicStringPool, newArr, nonAtomicIndex);
			nonAtomicStringPool = newArr;

			isClosed = true;
		}

		public override void Flush ()
		{
			// do nothing
		}

		public override string LookupPrefix (string ns)
		{
			int tmp = nsIndex;
			while (tmp != 0) {
				if (atomicStringPool [namespaces [tmp].Namespace] == ns)
					return atomicStringPool [namespaces [tmp].Name];
				tmp = namespaces [tmp].NextNamespace;
			}
			return null;
		}

		public override void WriteCData (string data)
		{
			AddTextNode (data);
		}

		private void AddTextNode (string data)
		{
			switch (state) {
			case WriteState.Element:
				CloseStartElement ();
				break;
			case WriteState.Content:
				break;
			default:
				throw new InvalidOperationException ("Invalid document state for CDATA section: " + state);
			}

			// When text after text, just add the value, and return.
			if (nodes [nodeIndex].Parent == parentStack [parentStackIndex]) {
				switch (nodes [nodeIndex].NodeType) {
				case XPathNodeType.Text:
				case XPathNodeType.SignificantWhitespace:
					string value = nonAtomicStringPool [nodes [nodeIndex].Value] + data;
					nodes [nodeIndex].Value = NonAtomicIndex (value);
					if (IsWhitespace (value))
						nodes [nodeIndex].NodeType = XPathNodeType.SignificantWhitespace;
					else
						nodes [nodeIndex].NodeType = XPathNodeType.Text;
					return;
				}
			}

			int parent = GetParentIndex ();
			UpdateTreeForAddition ();

			AddNode (parent,
				0, // attribute
				prevSibling,
				XPathNodeType.Text,
				null,
				false,
				String.Empty,
				String.Empty,
				String.Empty,
				data,
				null,
				0, // nsIndex
				0, // line info
				0);
		}

		private void CheckTopLevelNode ()
		{
			switch (state) {
			case WriteState.Element:
				CloseStartElement ();
				break;
			case WriteState.Content:
			case WriteState.Prolog:
			case WriteState.Start:
				break;
			default:
				throw new InvalidOperationException ("Invalid document state for CDATA section: " + state);
			}
		}

		public override void WriteComment (string data)
		{
			CheckTopLevelNode ();

			int parent = GetParentIndex ();
			UpdateTreeForAddition ();

			AddNode (parent,
				0, // attribute
				prevSibling,
				XPathNodeType.Comment,
				null,
				false,
				String.Empty,
				String.Empty,
				String.Empty,
				data,
				null,
				0, // nsIndex
				0, // line info
				0);
		}

		public override void WriteProcessingInstruction (string name, string data)
		{
			CheckTopLevelNode ();

			int parent = GetParentIndex ();
			UpdateTreeForAddition ();

			AddNode (parent,
				0, // attribute
				prevSibling,
				XPathNodeType.ProcessingInstruction,
				null,
				false,
				name,
				String.Empty,
				String.Empty,
				data,
				null,
				0, // nsIndex
				0, // line info
				0);
		}

		public override void WriteWhitespace (string data)
		{
			CheckTopLevelNode ();

			int parent = GetParentIndex ();
			UpdateTreeForAddition ();

			AddNode (parent,
				0, // attribute
				prevSibling,
				XPathNodeType.Whitespace,
				null,
				false,
				String.Empty,
				String.Empty,
				String.Empty,
				data,
				null,
				0, // nsIndex
				0, // line info
				0);
		}

		public override void WriteStartDocument ()
		{
			// do nothing
		}

		public override void WriteStartDocument (bool standalone)
		{
			// do nothing
		}

		public override void WriteEndDocument ()
		{
			// do nothing
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			if (ns == null)
				ns = String.Empty;
			else if (prefix == null && ns.Length > 0)
				prefix = LookupPrefix (ns);
			if (prefix == null)
				prefix = String.Empty;

			switch (state) {
			case WriteState.Element:
				CloseStartElement ();
				break;
			case WriteState.Start:
			case WriteState.Prolog:
			case WriteState.Content:
				break;
			default:
				throw new InvalidOperationException ("Invalid document state for writing element: " + state);
			}

			int parent = GetParentIndex ();
			UpdateTreeForAddition ();

			WriteStartElement (parent, prevSibling, prefix, localName, ns);
			state = WriteState.Element;
		}

		private void WriteStartElement (int parent, int previousSibling, string prefix, string localName, string ns)
		{
			PrepareStartElement (previousSibling);

			AddNode (parent,
				0, // dummy:firstAttribute
				previousSibling,
				XPathNodeType.Element,
				null,
				false,
				localName,
				ns,
				prefix,
				"",	// Element has no internal value.
				null,
				lastNsInScope,
				0,
				0);
		}

		private void PrepareStartElement (int previousSibling)
		{
			hasAttributes = false;
			hasLocalNs = false;
			attrIndexAtStart = attributeIndex;
			nsIndexAtStart = nsIndex;

			while (namespaces [lastNsInScope].DeclaredElement == previousSibling) {
				lastNsInScope = namespaces [lastNsInScope].NextNamespace;
			}
		}

		public override void WriteEndElement ()
		{
			WriteEndElement (false);
		}

		public override void WriteFullEndElement ()
		{
			WriteEndElement (true);
		}

		private void WriteEndElement (bool full)
		{
			switch (state) {
			case WriteState.Element:
				CloseStartElement ();
				break;
			case WriteState.Content:
				break;
			default:
				throw new InvalidOperationException ("Invalid state for writing EndElement: " + state);
			}
			parentStackIndex--;
			if (nodes [nodeIndex].NodeType == XPathNodeType.Element) {
				if (!full)
					nodes [nodeIndex].IsEmptyElement = true;
			}
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			if (ns == null)
				ns = String.Empty;

			if (state != WriteState.Element)
				throw new InvalidOperationException ("Invalid document state for attribute: " + state);

			state = WriteState.Attribute;
			if (ns == XmlNamespaces.XMLNS)
				ProcessNamespace ((prefix == null || prefix == String.Empty) ? "" : localName, String.Empty); // dummy: Value should be completed
			else
				ProcessAttribute (prefix, localName, ns, String.Empty); // dummy: Value should be completed
		}

		private void ProcessNamespace (string prefix, string ns)
		{
			int nextTmp = hasLocalNs ? nsIndex : nodes [nodeIndex].FirstNamespace;

			nsIndex++;

			this.AddNsNode (nodeIndex,
				prefix,
				ns,
				nextTmp);
			hasLocalNs = true;
			openNamespace = true;
		}

		private void ProcessAttribute (string prefix, string localName, string ns, string value)
		{
			if (prefix == null && ns.Length > 0)
				prefix = LookupPrefix (ns);
			attributeIndex ++;

			this.AddAttribute (nodeIndex,
				localName,
				ns, 
				prefix != null ? prefix : String.Empty, 
				value,
				0,
				0);
			if (hasAttributes)
				attributes [attributeIndex - 1].NextAttribute = attributeIndex;
			else
				hasAttributes = true;
		}

		public override void WriteEndAttribute ()
		{
			if (state != WriteState.Attribute)
				throw new InvalidOperationException ();

			openNamespace = false;
			state = WriteState.Element;
		}

		public override void WriteString (string text)
		{
			if (WriteState == WriteState.Attribute) {
				if (openNamespace) {
					string value = atomicStringPool [namespaces [nsIndex].Namespace] + text;
					namespaces [nsIndex].Namespace = AtomicIndex (value);
				} else {
					string value = nonAtomicStringPool [attributes [attributeIndex].Value] + text;
					attributes [attributeIndex].Value = NonAtomicIndex (value);
				}
			}
			else
				AddTextNode (text);
		}

		// Well, they cannot be supported, but actually used to
		// disable-output-escaping = "true"
		public override void WriteRaw (string data)
		{
			WriteString (data);
		}

		public override void WriteRaw (char [] data, int start, int len)
		{
			WriteString (new string (data, start, len));
		}

		public override void WriteName (string name)
		{
			WriteString (name);
		}

		public override void WriteNmToken (string name)
		{
			WriteString (name);
		}

		public override void WriteBase64 (byte [] buffer, int index, int count)
		{
			WriteString (Convert.ToBase64String (buffer, index, count));
		}

		public override void WriteBinHex (byte [] buffer, int index, int count)
		{
			throw new NotSupportedException ();
		}

		public override void WriteChars (char [] buffer, int index, int count)
		{
			WriteString (new string (buffer, index, count));
		}

		public override void WriteCharEntity (char c)
		{
			WriteString (c.ToString ());
		}

		public override void WriteDocType (string name, string pub, string sys, string intSubset)
		{
			throw new NotSupportedException ();
		}

		public override void WriteEntityRef (string name)
		{
			throw new NotSupportedException ();
		}

		public override void WriteQualifiedName (string localName, string ns)
		{
			string prefix = (ns == null || ns.Length == 0) ? null : LookupPrefix (ns);
			if (prefix != null && prefix.Length > 0)
				WriteString (prefix + ":" + localName);
			else
				WriteString (localName);
		}

		public override void WriteSurrogateCharEntity (char high, char low)
		{
			WriteString (high.ToString ());
			WriteString (low.ToString ());
		}

		private bool IsWhitespace (string data)
		{
			for (int i = 0; i < data.Length; i++) {
				switch (data [i]) {
				case ' ':
				case '\r':
				case '\n':
				case '\t':
					continue;
				default:
					return false;
				}
			}
			return true;
		}
		#endregion
	}
}
