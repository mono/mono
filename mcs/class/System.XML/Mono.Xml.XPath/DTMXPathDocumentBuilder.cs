//
// Mono.Xml.XPath.DTMXPathDocumentBuilder
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Mono.Xml.XPath
{

	public class DTMXPathDocumentBuilder
	{
		public DTMXPathDocumentBuilder (string url)
			: this (url, XmlSpace.None, 400)
		{
		}

		public DTMXPathDocumentBuilder (string url, XmlSpace space)
			: this (url, space, 400)
		{
		}

		public DTMXPathDocumentBuilder (string url, XmlSpace space, int defaultCapacity)
			: this (new XmlTextReader (url), space, defaultCapacity)
		{
		}

		public DTMXPathDocumentBuilder (XmlReader reader)
			: this (reader, XmlSpace.None, 400)
		{
		}

		public DTMXPathDocumentBuilder (XmlReader reader, XmlSpace space)
			: this (reader, space, 400)
		{
		}

		public DTMXPathDocumentBuilder (XmlReader reader, XmlSpace space, int defaultCapacity)
		{
			this.xmlReader = reader;
			this.validatingReader = reader as XmlValidatingReader;
			lineInfo = reader as IXmlLineInfo;
			this.xmlSpace = space;
			this.nameTable = reader.NameTable;
			nodeCapacity = nodeCapacity;
			attributeCapacity = nodeCapacity * 2;
			Compile ();
		}
		
		XmlReader xmlReader;
		XmlValidatingReader validatingReader;
		XmlSpace xmlSpace;
		XmlNameTable nameTable;
		IXmlLineInfo lineInfo;
		int nodeCapacity = 400;
		int attributeCapacity = 800;
		int nsCapacity = 10;

		// Linked Node
		DTMXPathLinkedNode [] nodes = new DTMXPathLinkedNode [0];

		// Attribute
		DTMXPathAttributeNode [] attributes = new DTMXPathAttributeNode [0];

		// NamespaceNode
		DTMXPathNamespaceNode [] namespaces = new DTMXPathNamespaceNode [0];

		// idTable [string value] -> int nodeId
		Hashtable idTable;

		int nodeIndex;
		int attributeIndex;
		int nsIndex;
		bool requireFirstChildFill;

		int prevSibling;
		int position;
		bool skipRead = false;

		public DTMXPathDocument CreateDocument ()
		{
			return new DTMXPathDocument (nameTable,
				nodes,
				attributes,
				namespaces,
				idTable
			);
		}

		public void Compile ()
		{
			idTable = new Hashtable ();

			// index 0 is dummy. No node (including Root) is assigned to this index
			// So that we can easily compare index != 0 instead of index < 0.
			// (Difference between jnz or jbe in 80x86.)
			AddNode (0, 0, 0, 0, 0, 0, XPathNodeType.All, "", false, "", "", "", "", "", 0, 0, 0);
			nodeIndex++;
			AddAttribute (0, null, null, null, null, null, 0, 0);
//			attributes [0].NextAttribute = 0;
			AddNsNode (0, null, null);
			nsIndex++;
//			nextNsNode_ [0] = 0;
			AddNsNode (1, "xml", XmlNamespaces.XML);
//			nextNsNode_ [1] = 0;

			// add root.
			AddNode (0, 0, 0, 0, -1, 0, XPathNodeType.Root, xmlReader.BaseURI, false, "", "", "", "", "", 1, 0, 0);

			this.nodeIndex = 1;
			this.requireFirstChildFill = true;

			while (!xmlReader.EOF)
				Read ();
			SetNodeArrayLength (nodeIndex + 1);
			SetAttributeArrayLength (attributeIndex + 1);
			SetNsArrayLength (nsIndex + 1);

			xmlReader = null;	// It is no more required.
		}

		public void Read ()
		{
			if (!skipRead)
				if (!xmlReader.Read ())
					return;
			skipRead = false;
			int parent = nodeIndex;

			if (nodes [nodeIndex].Depth >= xmlReader.Depth) {	// not ">=" ? But == worked when with ArrayList...
				// if not, then current node is parent.
				while (xmlReader.Depth <= nodes [parent].Depth)
					parent = nodes [parent].Parent;
			}

			prevSibling = nodeIndex;
			position = 0;
			switch (xmlReader.NodeType) {
			case XmlNodeType.Element:
			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Comment:
			case XmlNodeType.Text:
			case XmlNodeType.ProcessingInstruction:
				if (requireFirstChildFill)
					prevSibling = 0;
				else
					while (nodes [prevSibling].Depth != xmlReader.Depth)
						prevSibling = nodes [prevSibling].Parent;
				if (prevSibling != 0)
					position = nodes [prevSibling].Position + 1;

				nodeIndex++;

				if (prevSibling != 0)
					nodes [prevSibling].NextSibling = nodeIndex;
				if (requireFirstChildFill)
					nodes [parent].FirstChild = nodeIndex;
				break;
			case XmlNodeType.Whitespace:
				if (xmlSpace == XmlSpace.Preserve)
					goto case XmlNodeType.Text;
				else
					goto default;
			case XmlNodeType.EndElement:
				requireFirstChildFill = false;
				return;
			default:
				// No operations. Doctype, EntityReference, 
				return;
			}

			requireFirstChildFill = false;	// Might be changed in ProcessElement().

			string value = null;
			XPathNodeType nodeType = xmlReader.NodeType == XmlNodeType.Whitespace ?
				XPathNodeType.Whitespace : XPathNodeType.Text;

			switch (xmlReader.NodeType) {
			case XmlNodeType.Element:
				ProcessElement (parent, prevSibling, position);
				break;
			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Text:
			case XmlNodeType.Whitespace:
				if (value == null)
					skipRead = true;
				AddNode (parent,
					0,
					attributeIndex,
					prevSibling,
					xmlReader.Depth,
					position,
					nodeType,
					xmlReader.BaseURI,
					xmlReader.IsEmptyElement,
					xmlReader.LocalName,	// for PI
					xmlReader.NamespaceURI,	// for PI
					xmlReader.Prefix,
					value,
					xmlReader.XmlLang,
					nsIndex,
					lineInfo != null ? lineInfo.LineNumber : 0,
					lineInfo != null ? lineInfo.LinePosition : 0);
				// this code is tricky, but after ReadString() invokation,
				// xmlReader is moved to next node!!
				if (value == null)
					nodes [nodeIndex].Value = xmlReader.ReadString ();
				break;
			case XmlNodeType.Comment:
				value = xmlReader.Value;
				nodeType = XPathNodeType.Comment;
				goto case XmlNodeType.Text;
			case XmlNodeType.ProcessingInstruction:
				value = xmlReader.Value;
				nodeType = XPathNodeType.ProcessingInstruction;
				goto case XmlNodeType.Text;
			}
		}

		private void ProcessElement (int parent, int previousSibling, int position)
		{
			int firstAttributeIndex = 0;
			int lastNsIndexInCurrent = 0;


			// process namespaces and attributes.
			if (xmlReader.MoveToFirstAttribute ()) {
				do {
					if (xmlReader.NamespaceURI == XmlNamespaces.XMLNS) {
						// add namespace node.
						nsIndex++;

						int nextTmp = lastNsIndexInCurrent == 0 ? nodes [parent].FirstNamespace : lastNsIndexInCurrent;

						this.AddNsNode (nodeIndex,
							(xmlReader.Prefix == null || xmlReader.Prefix == String.Empty) ?
								"" : xmlReader.LocalName,
							xmlReader.Value);
						namespaces [nsIndex].NextNamespace = nextTmp;
//						if (lastNsIndexInCurrent == 0)
//							nodes [nodeIndex].FirstNamespace = nsIndex;
						lastNsIndexInCurrent = nsIndex;
					} else {
						// add attribute node.
						attributeIndex ++;
						this.AddAttribute (nodeIndex,
							xmlReader.LocalName,
							xmlReader.NamespaceURI, 
							xmlReader.Prefix != null ? xmlReader.Prefix : String.Empty, 
							xmlReader.Value,
							null, 
							lineInfo != null ? lineInfo.LineNumber : 0,
							lineInfo != null ? lineInfo.LinePosition : 0);
						if (firstAttributeIndex == 0)
							firstAttributeIndex = attributeIndex;
						else
							attributes [attributeIndex - 1].NextAttribute = attributeIndex;
						// dummy for "current" attribute.
						attributes [attributeIndex].NextAttribute = 0;

						// Identity infoset
						if (validatingReader != null) {
							XmlSchemaDatatype dt = validatingReader.SchemaType as XmlSchemaDatatype;
							if (dt == null) {
								XmlSchemaType xsType = validatingReader.SchemaType as XmlSchemaType;
								if (xsType != null)
									dt = xsType.Datatype;
							}
							if (dt != null && dt.TokenizedType == XmlTokenizedType.ID)
								idTable.Add (xmlReader.Value, nodeIndex);
						}
					}
				} while (xmlReader.MoveToNextAttribute ());
				xmlReader.MoveToElement ();
			}

			AddNode (parent,
				firstAttributeIndex,
				attributeIndex,
				previousSibling,
				xmlReader.Depth,
				position,
				XPathNodeType.Element,
				xmlReader.BaseURI,
				xmlReader.IsEmptyElement,
				xmlReader.LocalName,
				xmlReader.NamespaceURI,
				xmlReader.Prefix,
				"",	// Element has no internal value.
				xmlReader.XmlLang,
				nsIndex,
				lineInfo != null ? lineInfo.LineNumber : 0,
				lineInfo != null ? lineInfo.LinePosition : 0);
			if (!xmlReader.IsEmptyElement)
				requireFirstChildFill = true;
		}

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
		public void AddNode (int parent, int firstAttribute, int attributeEnd, int previousSibling, int depth, int position, XPathNodeType nodeType, string baseUri, bool isEmptyElement, string localName, string ns, string prefix, string value, string xmlLang, int namespaceNode, int lineNumber, int linePosition)
		{
			if (nodes.Length < nodeIndex + 1) {
//				if (nodes.Length >= nodeCapacity) {
					nodeCapacity *= 2;
					SetNodeArrayLength (nodeCapacity);
//				}
			}

			DTMXPathLinkedNode curNode = nodes [nodeIndex];// = new DTMXPathLinkedNode ();
			nodes [nodeIndex].FirstChild = 0;		// dummy
			nodes [nodeIndex].Parent = parent;
			nodes [nodeIndex].FirstAttribute = firstAttribute;
			nodes [nodeIndex].PreviousSibling = previousSibling;
			nodes [nodeIndex].NextSibling = 0;	// dummy
			nodes [nodeIndex].Depth = depth;
			nodes [nodeIndex].Position = position;
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
//				if (attributes.Length >= attributeCapacity) {
					attributeCapacity *= 2;
					SetAttributeArrayLength (attributeCapacity);
//				}
			}

			DTMXPathAttributeNode attr = attributes [attributeIndex];// = new DTMXPathAttributeNode ();
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
		public void AddNsNode (int declaredElement, string name, string ns)
		{
			if (namespaces.Length < nsIndex + 1) {
//				if (namespaces.Length >= nsCapacity) {
					nsCapacity *= 2;
					SetNsArrayLength (nsCapacity);
//				}
			}

			DTMXPathNamespaceNode nsNode = namespaces [nsIndex];// = new DTMXPathNamespaceNode ();
			namespaces [nsIndex].DeclaredElement = declaredElement;
			namespaces [nsIndex].Name = name;
			namespaces [nsIndex].Namespace = ns;
		}
	}
}

