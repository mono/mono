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
			: this (url, XmlSpace.None, false)
		{
		}

		public DTMXPathDocumentBuilder (string url, XmlSpace space)
			: this (url, space, false)
		{
		}

		public DTMXPathDocumentBuilder (string url, XmlSpace space, bool supportID)
			: this (new XmlTextReader (url), space, supportID)
		{
		}

		public DTMXPathDocumentBuilder (XmlReader reader)
			: this (reader, XmlSpace.None, false)
		{
		}

		public DTMXPathDocumentBuilder (XmlReader reader, XmlSpace space)
			: this (reader, space, false)
		{
		}

		public DTMXPathDocumentBuilder (XmlReader reader, XmlSpace space, bool supportID)
		{
			this.xmlReader = reader;
			if (supportID)
				this.validatingReader = reader as XmlValidatingReader;
			this.xmlSpace = xmlSpace;
			this.nameTable = reader.NameTable;
			Compile ();
		}
		
		bool supportID;
		XmlReader xmlReader;
		XmlValidatingReader validatingReader;
		XmlSpace xmlSpace;
		XmlNameTable nameTable;
		int defaultCapacity = 100;
		public int DefaultCapacity {
			get { return defaultCapacity; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ();
				defaultCapacity = value;
			}
		}

#region Tree node info collections.
		// Tree Node
		int [] firstChild_ = new int [0];
		int [] parent_ = new int [0];
		int [] firstAttribute_ = new int [0];
		int [] previousSibling_ = new int [0];
		int [] nextSibling_ = new int [0];
		int [] depth_ = new int [0];
		int [] position_ = new int [0];
		XPathNodeType [] nodeType_ = new XPathNodeType [0];
		string [] baseUri_ = new string [0];
		bool [] isEmptyElement_ = new bool [0];
		string [] localName_ = new string [0];
		string [] namespaceUri_ = new string [0];
		string [] prefix_ = new string [0];
		string [] value_ = new string [0];
		string [] xmlLang_ = new string [0];
		int [] namespaceNode_ = new int [0];
		object [] schemaType_ = new object [0];

		// Attribute
		int [] ownerElement_ = new int [0];
		int [] nextAttribute_ = new int [0];
		string [] attrLocalName_ = new string [0];
		string [] attrPrefix_ = new string [0];
		string [] attrNsUri_ = new string [0];
		string [] attrValue_ = new string [0];
		object [] attrSchemaType_ = new object [0];

		// NamespaceNode
		int [] nsDeclaredElement_ = new int [100];
		int [] nextNsNode_ = new int [100];
		string [] nsNodeName_ = new string [100];
		string [] nsNodeUri_ = new string [100];

		// idTable [string value] -> int nodeId
		Hashtable idTable_;
#endregion

		int nodeIndex;
		int attributeIndex;
		int nsIndex;
		bool requireFirstChildFill;

		public DTMXPathDocument CreateDocument ()
		{
			return new DTMXPathDocument (nameTable,
				firstChild_,
				parent_,
				firstAttribute_,
				previousSibling_,
				nextSibling_,
				depth_,
				position_,
				nodeType_,
				baseUri_,
				isEmptyElement_,
				localName_,
				namespaceUri_,
				prefix_,
				value_,
				xmlLang_,
				namespaceNode_,
				schemaType_,

				// Attribute
				ownerElement_,
				nextAttribute_,
				attrLocalName_,
				attrPrefix_,
				attrNsUri_,
				attrValue_,
				attrSchemaType_,

				// NamespaceNode
				nsDeclaredElement_,
				nextNsNode_,
				nsNodeName_,
				nsNodeUri_,
				idTable_
			);
		}

		public void Compile ()
		{
			idTable_ = new Hashtable ();

			// index 0 is dummy. No node (including Root) is assigned to this index
			// So that we can easily compare index != 0 instead of index < 0.
			// (Difference between jnz or jbe in 80x86.)
			AddNode (0, 0, 0, 0, 0, 0, XPathNodeType.All, "", false, "", "", "", "", "", 0, null);
			nodeIndex++;
			AddAttribute (0, null, null, null, null, null);
			nextAttribute_ [0] = 0;
			AddNsNode (0, null, null);
			nsIndex++;
			nextNsNode_ [0] = 0;
			AddNsNode (1, "xml", XmlNamespaces.XML);
			nextNsNode_ [1] = 0;

			// add root.
			AddNode (0, 0, 0, 0, -1, 0, XPathNodeType.Root, xmlReader.BaseURI, false, "", "", "", "", "", 1, null);

			this.nodeIndex = 1;
			this.requireFirstChildFill = true;

			while (!xmlReader.EOF)
				Read ();
			SetNodeArraysLength (nodeIndex + 1);
			SetAttributeArraysLength (attributeIndex + 1);
			SetNsArraysLength (nsIndex + 1);

			xmlReader = null;	// It is no more required.
		}

		int prevSibling;
		int position;
		bool skipRead = false;

		public void Read ()
		{
			if (!skipRead)
				if (!xmlReader.Read ())
					return;
			skipRead = false;
			int parent = nodeIndex;

			if (depth_ [nodeIndex] >= xmlReader.Depth) {	// not ">=" ? But == worked when with ArrayList...
				// if not, then current node is parent.
				while (xmlReader.Depth <= depth_ [parent])
					parent = parent_ [parent];
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
					while (depth_ [prevSibling] != xmlReader.Depth)
						prevSibling = parent_ [prevSibling];
				if (prevSibling != 0)
					position = position_ [prevSibling] + 1;

				nodeIndex++;

				if (prevSibling != 0)
					nextSibling_ [prevSibling] = nodeIndex;
				if (requireFirstChildFill)
					firstChild_ [parent] = nodeIndex;
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
			XPathNodeType nodeType = XPathNodeType.Text;

			switch (xmlReader.NodeType) {
			case XmlNodeType.Element:
				ProcessElement (parent, prevSibling, position);
				break;
			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Text:
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
					null);	// schemaType
				// this code is tricky, but after ReadString() invokation,
				// xmlReader is moved to next node!!
				if (value == null)
					value_ [nodeIndex] = xmlReader.ReadString ();
				break;
			case XmlNodeType.Comment:
				value = xmlReader.Value;
				nodeType = XPathNodeType.Comment;
				goto case XmlNodeType.Text;
			case XmlNodeType.Whitespace:
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

						nextNsNode_ [nsIndex] = lastNsIndexInCurrent == 0 ? namespaceNode_ [parent] : lastNsIndexInCurrent;

						if (lastNsIndexInCurrent == 0)
							namespaceNode_ [nodeIndex] = nsIndex;
						this.AddNsNode (nodeIndex,
							(xmlReader.Prefix == null || xmlReader.Prefix == String.Empty) ?
								"" : xmlReader.LocalName,
							xmlReader.Value);
						lastNsIndexInCurrent = nsIndex;
					} else {
						// add attribute node.
						attributeIndex ++;
						this.AddAttribute (nodeIndex, xmlReader.LocalName, xmlReader.NamespaceURI, xmlReader.Prefix != null ? xmlReader.Prefix : String.Empty, xmlReader.Value, null);
						if (firstAttributeIndex == 0)
							firstAttributeIndex = attributeIndex;
						else
							nextAttribute_ [attributeIndex - 1] = attributeIndex;
						// dummy for "current" attribute.
						nextAttribute_ [attributeIndex] = 0;

						// Identity infoset
						if (validatingReader != null) {
							XmlSchemaDatatype dt = validatingReader.SchemaType as XmlSchemaDatatype;
							if (dt == null) {
								XmlSchemaType xsType = validatingReader.SchemaType as XmlSchemaType;
								dt = xsType.Datatype;
							}
							if (dt != null && dt.TokenizedType == XmlTokenizedType.ID)
								idTable_.Add (xmlReader.Value, nodeIndex);
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
				null);	// schemaType
			if (!xmlReader.IsEmptyElement)
				requireFirstChildFill = true;
		}

		private void SetObjectArrayLength (ref object [] a, int length)
		{
			object [] arr = new object [length];
			Array.Copy (a, arr, System.Math.Min (a.Length, length));
			a = arr;
		}

		private void SetBoolArrayLength (ref bool [] a, int length)
		{
			bool [] bArr = new bool [length];
			Array.Copy (a, bArr, System.Math.Min (a.Length, length));
			a = bArr;
		}

		private void SetXPathNodeTypeArrayLength (ref XPathNodeType [] a, int length)
		{
			XPathNodeType [] arr = new XPathNodeType [length];
			Array.Copy (a, arr, System.Math.Min (a.Length, length));
			a = arr;
		}

		private void SetIntArrayLength (ref int [] a, int length)
		{
			int [] intArr = new int [length];
			Array.Copy (a, intArr, System.Math.Min (a.Length, length));
			a = intArr;
		}

		private void SetStringArrayLength (ref string [] a, int length)
		{
			string [] strArr = new string [length];
			Array.Copy (a, strArr, System.Math.Min (a.Length, length));
			a = strArr;
		}

		private void SetNodeArraysLength (int size)
		{
			SetIntArrayLength (ref firstChild_, size);
			SetIntArrayLength (ref parent_, size);
			SetIntArrayLength (ref firstAttribute_, size);
			SetIntArrayLength (ref previousSibling_, size);
			SetIntArrayLength (ref nextSibling_, size);
			SetIntArrayLength (ref depth_, size);
			SetIntArrayLength (ref position_, size);
			SetXPathNodeTypeArrayLength (ref nodeType_, size);
			SetStringArrayLength (ref baseUri_, size);
			SetBoolArrayLength (ref isEmptyElement_, size);
			SetStringArrayLength (ref localName_, size);
			SetStringArrayLength (ref namespaceUri_, size);
			SetStringArrayLength (ref prefix_, size);
			SetStringArrayLength (ref value_, size);
			SetStringArrayLength (ref xmlLang_, size);
			SetIntArrayLength (ref namespaceNode_, size);
			SetObjectArrayLength (ref schemaType_, size);
		}

		private void SetAttributeArraysLength (int size)
		{
			SetIntArrayLength (ref ownerElement_, size);
			SetIntArrayLength (ref nextAttribute_, size);
			SetStringArrayLength (ref attrLocalName_, size);
			SetStringArrayLength (ref attrPrefix_, size);
			SetStringArrayLength (ref attrNsUri_, size);
			SetStringArrayLength (ref attrValue_, size);
			SetObjectArrayLength (ref attrSchemaType_, size);
		}

		private void SetNsArraysLength (int size)
		{
			SetIntArrayLength (ref nsDeclaredElement_, size);
			SetIntArrayLength (ref nextNsNode_, size);
			SetStringArrayLength (ref nsNodeName_, size);
			SetStringArrayLength (ref nsNodeUri_, size);
		}

		// Here followings are skipped: firstChild, nextSibling, 
		public void AddNode (int parent, int firstAttribute, int attributeEnd, int previousSibling, int depth, int position, XPathNodeType nodeType, string baseUri, bool isEmptyElement, string localName, string ns, string prefix, string value, string xmlLang, int namespaceNode, object schemaType)
		{
			if (firstChild_.Length < nodeIndex + 1) {
				if (firstChild_.Length >= defaultCapacity)
					defaultCapacity *= 2;
				SetNodeArraysLength (defaultCapacity);
			}

			firstChild_ [nodeIndex] = 0;		// dummy
			parent_ [nodeIndex] = parent;
			firstAttribute_ [nodeIndex] = firstAttribute;
			previousSibling_ [nodeIndex] = previousSibling;
			nextSibling_ [nodeIndex] = 0;	// dummy
			depth_ [nodeIndex] = depth;
			position_ [nodeIndex] = position;
			nodeType_ [nodeIndex] = nodeType;
			baseUri_ [nodeIndex] = baseUri;
			isEmptyElement_ [nodeIndex] = isEmptyElement;
			localName_ [nodeIndex] = localName;
			namespaceUri_ [nodeIndex] = ns;
			prefix_ [nodeIndex] = prefix;
			value_ [nodeIndex] = value;
			xmlLang_ [nodeIndex] = xmlLang;
			namespaceNode_ [nodeIndex] = namespaceNode;
			schemaType_ [nodeIndex] = schemaType;
		}

		// Followings are skipped: nextAttribute,
		public void AddAttribute (int ownerElement, string localName, string ns, string prefix, string value, object schemaType)
		{
			if (ownerElement_.Length < attributeIndex + 1) {
				if (ownerElement_.Length >= defaultCapacity)
					defaultCapacity *= 2;
				SetAttributeArraysLength (defaultCapacity);
			}

			ownerElement_ [attributeIndex] = ownerElement;
			attrLocalName_ [attributeIndex] = localName;
			attrNsUri_ [attributeIndex] = ns;
			attrPrefix_ [attributeIndex] = prefix;
			attrValue_ [attributeIndex] = value;
			attrSchemaType_ [attributeIndex] = schemaType;
		}

		// Followings are skipped: nextNsNode (may be next attribute in the same element, or ancestors' nsNode)
		public void AddNsNode (int declaredElement, string name, string ns)
		{
			if (nsDeclaredElement_.Length < nsIndex + 1) {
				if (nsDeclaredElement_.Length >= defaultCapacity)
					defaultCapacity *= 2;
				SetNsArraysLength (defaultCapacity);
			}

			nsDeclaredElement_ [nsIndex] = declaredElement;
			nsNodeName_ [nsIndex] = name;
			nsNodeUri_ [nsIndex] = ns;
		}
	}
}

