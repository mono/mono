//
// Mono.Xml.XPath.DTMXPathDocumentWriter
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
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
		class DTMXPathDocumentWriter : XmlWriter
	{
		public DTMXPathDocumentWriter (XmlNameTable nt, int defaultCapacity)
		{
			nameTable = nt == null ? new NameTable () : nt;
			nodeCapacity = defaultCapacity;
			attributeCapacity = nodeCapacity;
			idTable = new Hashtable ();

			nodes = new DTMXPathLinkedNode [nodeCapacity];
			attributes = new DTMXPathAttributeNode [attributeCapacity];
			namespaces = new DTMXPathNamespaceNode [0];

			Init ();
		}
		
		XmlNameTable nameTable;
		int nodeCapacity = 200;
		int attributeCapacity = 200;
		int nsCapacity = 10;

		// Linked Node
		DTMXPathLinkedNode [] nodes;

		// Attribute
		DTMXPathAttributeNode [] attributes;

		// NamespaceNode
		DTMXPathNamespaceNode [] namespaces;

		// idTable [string value] -> int nodeId
		Hashtable idTable;

		int nodeIndex;
		int attributeIndex;
		int nsIndex;
		int parentForFirstChild;

		// for attribute processing; should be reset per each element.
		int firstAttributeIndex;
		int lastNsIndexInCurrent;
		int attrIndexAtStart;
		int nsIndexAtStart;

		int prevSibling;
		int lastNsInScope;

		// They are only used in Writer
		int writerDepth;
		WriteState state;
		bool openNamespace;

		public DTMXPathDocument CreateDocument ()
		{
			return new DTMXPathDocument (nameTable,
				nodes,
				attributes,
				namespaces,
				idTable
			);
		}

		public void Init ()
		{
			// index 0 is dummy. No node (including Root) is assigned to this index
			// So that we can easily compare index != 0 instead of index < 0.
			// (Difference between jnz or jbe in 80x86.)
			AddNode (0, 0, 0, 0, XPathNodeType.All, "", false, "", "", "", "", "", 0, 0, 0);
			nodeIndex++;
			AddAttribute (0, null, null, null, null, null, 0, 0);
			AddNsNode (0, null, null, 0);
			nsIndex++;
			AddNsNode (1, "xml", XmlNamespaces.XML, 0);

			// add root.
			AddNode (0, 0, 0, -1, XPathNodeType.Root, null, false, "", "", "", "", "", 1, 0, 0);

			this.nodeIndex = 1;
			this.lastNsInScope = 1;
			this.parentForFirstChild = nodeIndex;

			state = WriteState.Content;
		}

		private int GetParentIndex ()
		{
			if (parentForFirstChild >= 0)
				return parentForFirstChild;

			int parent = nodeIndex;
			if (nodes [nodeIndex].Depth >= writerDepth) {
				// if not, then current node is parent.
				while (writerDepth <= nodes [parent].Depth)
					parent = nodes [parent].Parent;
			}
			return parent;
		}

		private int GetPreviousSiblingIndex ()
		{
			int prevSibling = nodeIndex;
			if (parentForFirstChild >= 0)
				prevSibling = 0;
			else
				while (nodes [prevSibling].Depth != writerDepth)
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
			if (parentForFirstChild >= 0)
				nodes [parent].FirstChild = nodeIndex;

			parentForFirstChild = -1;
		}

		private void CloseStartElement ()
		{
			if (attrIndexAtStart != attributeIndex)
				nodes [nodeIndex].FirstAttribute = attrIndexAtStart + 1;
			if (nsIndexAtStart != nsIndex) {
				nodes [nodeIndex].FirstNamespace = nsIndex;
				lastNsInScope = nsIndex;
			}

			if (!nodes [nodeIndex].IsEmptyElement)
				parentForFirstChild = nodeIndex;

			state = WriteState.Content;
			writerDepth++;
		}

		#region Adding Nodes
		private void SetNodeArrayLength (int size)
		{
			DTMXPathLinkedNode [] newArr = new DTMXPathLinkedNode [size];
			Array.Copy (nodes, newArr, System.Math.Min (size, nodes.Length));
			nodes = newArr;
		}

		private void SetAttributeArrayLength (int size)
		{
			DTMXPathAttributeNode [] newArr = 
				new DTMXPathAttributeNode [size];
			Array.Copy (attributes, newArr, System.Math.Min (size, attributes.Length));
			attributes = newArr;
		}

		private void SetNsArrayLength (int size)
		{
			DTMXPathNamespaceNode [] newArr =
				new DTMXPathNamespaceNode [size];
			Array.Copy (namespaces, newArr, System.Math.Min (size, namespaces.Length));
			namespaces = newArr;
		}

		// Here followings are skipped: firstChild, nextSibling, 
		public void AddNode (int parent, int firstAttribute, int previousSibling, int depth, XPathNodeType nodeType, string baseUri, bool isEmptyElement, string localName, string ns, string prefix, string value, string xmlLang, int namespaceNode, int lineNumber, int linePosition)
		{
			if (nodes.Length < nodeIndex + 1) {
				nodeCapacity *= 4;
				SetNodeArrayLength (nodeCapacity);
			}

#if DTM_CLASS
			nodes [nodeIndex] = new DTMXPathLinkedNode ();
#endif
			nodes [nodeIndex].FirstChild = 0;		// dummy
			nodes [nodeIndex].Parent = parent;
			nodes [nodeIndex].FirstAttribute = firstAttribute;
			nodes [nodeIndex].PreviousSibling = previousSibling;
			nodes [nodeIndex].NextSibling = 0;	// dummy
			nodes [nodeIndex].Depth = depth;
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
		public void AddAttribute (int ownerElement, string localName, string ns, string prefix, string value, object schemaType, int lineNumber, int linePosition)
		{
			if (attributes.Length < attributeIndex + 1) {
				attributeCapacity *= 4;
				SetAttributeArrayLength (attributeCapacity);
			}

#if DTM_CLASS
			attributes [attributeIndex] = new DTMXPathAttributeNode ();
#endif
			attributes [attributeIndex].OwnerElement = ownerElement;
			attributes [attributeIndex].LocalName = localName;
			attributes [attributeIndex].NamespaceURI = ns;
			attributes [attributeIndex].Prefix = prefix;
			attributes [attributeIndex].Value = value;
			attributes [attributeIndex].SchemaType = schemaType;
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
			namespaces [nsIndex] = new DTMXPathNamespaceNode ();
#endif
			namespaces [nsIndex].DeclaredElement = declaredElement;
			namespaces [nsIndex].Name = name;
			namespaces [nsIndex].Namespace = ns;
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
		}

		public override void Flush ()
		{
			// do nothing
		}

		public override string LookupPrefix (string ns)
		{
			int tmp = nsIndex;
			while (tmp != 0) {
				if (namespaces [tmp].Namespace == ns)
					return namespaces [tmp].Name;
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
			if (nodes [nodeIndex].Depth == writerDepth) {
				switch (nodes [nodeIndex].NodeType) {
				case XPathNodeType.Text:
				case XPathNodeType.SignificantWhitespace:
					nodes [nodeIndex].Value += data;
					if (IsWhitespace (data))
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
				writerDepth,
				XPathNodeType.Text,
				null,
				false,
				null,
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
				writerDepth,
				XPathNodeType.Comment,
				null,
				false,
				null,
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
				writerDepth,
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
				writerDepth,
				XPathNodeType.Whitespace,
				null,
				false,
				null,
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
				writerDepth,
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
			firstAttributeIndex = 0;
			lastNsIndexInCurrent = 0;
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
			parentForFirstChild = -1;
			if (nodes [nodeIndex].NodeType == XPathNodeType.Element) {
				if (!full)
					nodes [nodeIndex].IsEmptyElement = true;
			}

			writerDepth--;
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
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
			nsIndex++;

			int nextTmp = lastNsIndexInCurrent == 0 ? nodes [nodeIndex].FirstNamespace : lastNsIndexInCurrent;

			this.AddNsNode (nodeIndex,
				prefix,
				ns,
				nextTmp);
			lastNsIndexInCurrent = nsIndex;
			openNamespace = true;
		}

		private void ProcessAttribute (string prefix, string localName, string ns, string value)
		{
			attributeIndex ++;

			this.AddAttribute (nodeIndex,
				localName,
				ns, 
				prefix != null ? prefix : String.Empty, 
				value,
				null,
				0,
				0);
			if (firstAttributeIndex == 0)
				firstAttributeIndex = attributeIndex;
			else
				attributes [attributeIndex - 1].NextAttribute = attributeIndex;
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
				if (openNamespace)
					namespaces [nsIndex].Namespace += text;
				else
					attributes [attributeIndex].Value += text;
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
			throw new NotSupportedException ();
		}

		public override void WriteBinHex (byte [] buffer, int index, int count)
		{
			throw new NotSupportedException ();
		}

		public override void WriteChars (char [] buffer, int index, int count)
		{
			throw new NotSupportedException ();
		}

		public override void WriteCharEntity (char c)
		{
			throw new NotSupportedException ();
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
			throw new NotSupportedException ();
		}

		public override void WriteSurrogateCharEntity (char high, char low)
		{
			throw new NotSupportedException ();
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
