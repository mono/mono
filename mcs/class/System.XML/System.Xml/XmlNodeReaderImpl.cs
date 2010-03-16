//
// System.Xml.XmlNodeReaderImpl.cs - implements the core part of XmlNodeReader
//
// Author:
//	Atsushi Enomoto  (atsushi@ximian.com)
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

//
// This serves the implementation part of XmlNodeReader except for
// ResolveEntity().
//

using System;
#if NET_2_0
using System.Collections.Generic;
#endif
using System.Xml;
using System.Text;
using Mono.Xml;
#if NET_2_0
using System.Xml.Schema;
#endif

namespace System.Xml
{
#if NET_2_0
	internal class XmlNodeReaderImpl : XmlReader, IHasXmlParserContext, IXmlNamespaceResolver
#else
	internal class XmlNodeReaderImpl : XmlReader, IHasXmlParserContext
#endif
	{
		XmlDocument document;
		XmlNode startNode;
		XmlNode current;
		XmlNode ownerLinkedNode;
		ReadState state = ReadState.Initial;
		int depth;
		bool isEndElement;
		bool ignoreStartNode;

		#region Constructor

		internal XmlNodeReaderImpl (XmlNodeReaderImpl entityContainer)
			: this (entityContainer.current)
		{
		}

		public XmlNodeReaderImpl (XmlNode node)
		{
			startNode = node;
			depth = 0;
			document = startNode.NodeType == XmlNodeType.Document ?
				startNode as XmlDocument : startNode.OwnerDocument;

			switch (node.NodeType) {
			case XmlNodeType.Document:
			case XmlNodeType.DocumentFragment:
			case XmlNodeType.EntityReference:
				ignoreStartNode = true;
				break;
			}

		}
		
		#endregion

		#region Properties

		public override int AttributeCount {
			get {
				if (state != ReadState.Interactive)
					return 0;

				if (isEndElement || current == null)
					return 0;
				XmlNode n = ownerLinkedNode;
				return n.Attributes != null ? n.Attributes.Count : 0;
			}
		}

		public override string BaseURI {
			get {
				if (current == null)
					return startNode.BaseURI;
				return current.BaseURI;
			}
		}

#if NET_2_0
		public override bool CanReadBinaryContent {
			get { return true; }
		}

		public override bool CanReadValueChunk {
			get { return true; }
		}
#else
		internal override bool CanReadBinaryContent {
			get { return true; }
		}

		internal override bool CanReadValueChunk {
			get { return true; }
		}
#endif

		public override bool CanResolveEntity {
			get { return false; }
		}

		public override int Depth {
			get {
				return current == null ? 0 :
					current == ownerLinkedNode ? depth :
					current.NodeType == XmlNodeType.Attribute ? depth + 1 :
					depth + 2;
			}
		}

		public override bool EOF {
			get { return state == ReadState.EndOfFile || state == ReadState.Error; }
		}

		public override bool HasAttributes {
			get {
				if (isEndElement || current == null)
					return false;

				// MS BUG: inconsistent return value between XmlTextReader and XmlNodeReader.
				// As for attribute and its descendants, XmlReader returns element's HasAttributes.
				XmlNode n = ownerLinkedNode;

				if (n.Attributes == null ||
					n.Attributes.Count == 0)
					return false;
				else
					return true;
			}
		}

#if !MOONLIGHT
		public override bool HasValue {
			get {
				if (current == null)
					return false;

				switch (current.NodeType) {
				case XmlNodeType.Element:
				case XmlNodeType.EntityReference:
				case XmlNodeType.Document:
				case XmlNodeType.DocumentFragment:
				case XmlNodeType.Notation:
				case XmlNodeType.EndElement:
				case XmlNodeType.EndEntity:
					return false;
				default:
					return true;
				}
			}
			      
		}
#endif

		public override bool IsDefault {
			get {
				if (current == null)
					return false;

				if (current.NodeType != XmlNodeType.Attribute)
					return false;
				else
				{
					return !((XmlAttribute) current).Specified;
				}
			}
		}

		public override bool IsEmptyElement {
			get {
				if (current == null)
					return false;

				if(current.NodeType == XmlNodeType.Element)
					return ((XmlElement) current).IsEmpty;
				else 
					return false;
			}
		}

#if NET_2_0
#else
		public override string this [int i] {
			get { return GetAttribute (i); }
		}

		public override string this [string name] {
			get { return GetAttribute (name); }
		}

		public override string this [string name, string namespaceURI] {
			get { return GetAttribute (name, namespaceURI); }
		}
#endif

		public override string LocalName {
			get {
				if (current == null)
					return String.Empty;

				switch (current.NodeType) {
				case XmlNodeType.Attribute:
				case XmlNodeType.DocumentType:
				case XmlNodeType.Element:
				case XmlNodeType.EntityReference:
				case XmlNodeType.ProcessingInstruction:
				case XmlNodeType.XmlDeclaration:
					return current.LocalName;
				}

				return String.Empty;
			}
		}

		public override string Name {
			get {
				if (current == null)
					return String.Empty;

				switch (current.NodeType) {
				case XmlNodeType.Attribute:
				case XmlNodeType.DocumentType:
				case XmlNodeType.Element:
				case XmlNodeType.EntityReference:
				case XmlNodeType.ProcessingInstruction:
				case XmlNodeType.XmlDeclaration:
					return current.Name;
				}

				return String.Empty;
			}
		}

		public override string NamespaceURI {
			get {
				if (current == null)
					return String.Empty;

				return current.NamespaceURI;
			}
		}

		public override XmlNameTable NameTable {
			get { return document.NameTable; }
		}

		public override XmlNodeType NodeType {
			get {
				if (current == null)
					return XmlNodeType.None;

				return isEndElement ? XmlNodeType.EndElement : current.NodeType;
			}
		}

		public override string Prefix {
			get { 
				if (current == null)
					return String.Empty;

				return current.Prefix;
			}
		}

#if NET_2_0
#else
		public override char QuoteChar {
			get {
				return '"';
			}
		}
#endif

		public override ReadState ReadState {
			get { return state; }
		}

#if NET_2_0
		public override IXmlSchemaInfo SchemaInfo {
			get { return current != null ? current.SchemaInfo : null; }
		}
#endif

		public override string Value {
			get {
				if (NodeType == XmlNodeType.DocumentType)
					return ((XmlDocumentType) current).InternalSubset;
				else
					return HasValue ? current.Value : String.Empty;
			}
		}

		public override string XmlLang {
			get {
				if (current == null)
					return startNode.XmlLang;

				return current.XmlLang;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				if (current == null)
					return startNode.XmlSpace;

				return current.XmlSpace;
			}
		}
		#endregion

		#region Methods

		public override void Close ()
		{
			current = null;
			state = ReadState.Closed;
		}

		public override string GetAttribute (int attributeIndex)
		{
			if (NodeType == XmlNodeType.XmlDeclaration) {
				XmlDeclaration decl = current as XmlDeclaration;
				if (attributeIndex == 0)
					return decl.Version;
				else if (attributeIndex == 1) {
					if (decl.Encoding != String.Empty)
						return decl.Encoding;
					else if (decl.Standalone != String.Empty)
						return decl.Standalone;
				}
				else if (attributeIndex == 2 &&
						decl.Encoding != String.Empty && decl.Standalone != null)
					return decl.Standalone;
				throw new ArgumentOutOfRangeException ("Index out of range.");
			} else if (NodeType == XmlNodeType.DocumentType) {
				XmlDocumentType doctype = current as XmlDocumentType;
				if (attributeIndex == 0) {
					if (doctype.PublicId != "")
						return doctype.PublicId;
					else if (doctype.SystemId != "")
						return doctype.SystemId;
				} else if (attributeIndex == 1)
					if (doctype.PublicId == "" && doctype.SystemId != "")
						return doctype.SystemId;
				throw new ArgumentOutOfRangeException ("Index out of range.");
			}

			// This is MS.NET bug which returns attributes in spite of EndElement.
			if (isEndElement || current == null)
				return null;

			if (attributeIndex < 0 || attributeIndex > AttributeCount)
				throw new ArgumentOutOfRangeException ("Index out of range.");

			return ownerLinkedNode.Attributes [attributeIndex].Value;
		}

		public override string GetAttribute (string name)
		{
			// This is MS.NET bug which returns attributes in spite of EndElement.
			if (isEndElement || current == null)
				return null;

			if (NodeType == XmlNodeType.XmlDeclaration)
				return GetXmlDeclarationAttribute (name);
			else if (NodeType == XmlNodeType.DocumentType)
				return GetDocumentTypeAttribute (name);

			if (ownerLinkedNode.Attributes == null)
				return null;
			XmlAttribute attr = ownerLinkedNode.Attributes [name];
			if (attr == null)
				return null;
			else
				return attr.Value;
		}

		public override string GetAttribute (string name, string namespaceURI)
		{
			// This is MS.NET bug which returns attributes in spite of EndElement.
			if (isEndElement || current == null)
				return null;

			if (NodeType == XmlNodeType.XmlDeclaration)
				return GetXmlDeclarationAttribute (name);
			else if (NodeType == XmlNodeType.DocumentType)
				return GetDocumentTypeAttribute (name);

			if (ownerLinkedNode.Attributes == null)
				return null;
			XmlAttribute attr = ownerLinkedNode.Attributes [name, namespaceURI];
			if (attr == null)
				return null;	// In fact MS.NET returns null instead of String.Empty.
			else
				return attr.Value;
		}

		private string GetXmlDeclarationAttribute (string name)
		{
			XmlDeclaration decl = current as XmlDeclaration;
			switch (name) {
			case "version":
				return decl.Version;
			case "encoding":
				// This is MS.NET bug that XmlNodeReturns in case of string.empty.
				return decl.Encoding != String.Empty ? decl.Encoding : null;
			case "standalone":
				return decl.Standalone;
			}
			return null;
		}

		private string GetDocumentTypeAttribute (string name)
		{
			XmlDocumentType doctype = current as XmlDocumentType;
			switch (name) {
			case "PUBLIC":
				return doctype.PublicId;
			case "SYSTEM":
				return doctype.SystemId;
			}
			return null;
		}

		XmlParserContext IHasXmlParserContext.ParserContext {
			get {
				return new XmlParserContext (document.NameTable,
					current == null ?
						new XmlNamespaceManager (document.NameTable) :
						current.ConstructNamespaceManager (),
					document.DocumentType != null ? document.DocumentType.DTD : null,
					current == null ? document.BaseURI : current.BaseURI,
					XmlLang, XmlSpace, Encoding.Unicode);
			}
		}

#if NET_2_0
		public IDictionary<string, string> GetNamespacesInScope (XmlNamespaceScope scope)
		{
			IDictionary<string, string> table = new Dictionary<string, string> ();
			XmlNode n = current;
			do {
				if (n.NodeType == XmlNodeType.Document)
					break;
				for (int i = 0; i < current.Attributes.Count; i++) {
					XmlAttribute a = current.Attributes [i];
					if (a.NamespaceURI == XmlNamespaceManager.XmlnsXmlns)
						table.Add (a.Prefix == XmlNamespaceManager.PrefixXmlns ? a.LocalName : String.Empty, a.Value);
				}
				if (scope == XmlNamespaceScope.Local)
					return table;
				n = n.ParentNode;
			} while (n != null);
			if (scope == XmlNamespaceScope.All)
				table.Add (XmlNamespaceManager.PrefixXml, XmlNamespaceManager.XmlnsXml);
			return table;
		}
#endif

		private XmlElement GetCurrentElement ()
		{
			XmlElement el = null;
			switch (current.NodeType) {
			case XmlNodeType.Attribute:
				el = ((XmlAttribute) current).OwnerElement;
				break;
			case XmlNodeType.Element:
				el = (XmlElement) current;
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.EntityReference:
			case XmlNodeType.Comment:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Whitespace:
			case XmlNodeType.ProcessingInstruction:
				el = current.ParentNode as XmlElement;
				break;
			}
			return el;
		}

		public override string LookupNamespace (string prefix)
		{
			if (current == null)
				return null;

			XmlElement el = GetCurrentElement ();

			for (; el != null; el = el.ParentNode as XmlElement) {
				for (int i = 0; i < el.Attributes.Count; i++) {
					XmlAttribute attr = el.Attributes [i];
					if (attr.NamespaceURI != XmlNamespaceManager.XmlnsXmlns)
						continue;
					if (prefix == "") {
						if (attr.Prefix == "")
							return attr.Value;
					}
					else if (attr.LocalName == prefix)
						return attr.Value;
					continue;
				}
			}

			switch (prefix) {
			case XmlNamespaceManager.PrefixXml:
				return XmlNamespaceManager.XmlnsXml;
			case XmlNamespaceManager.PrefixXmlns:
				return XmlNamespaceManager.XmlnsXmlns;
			}
			return null;
		}

#if NET_2_0
		public string LookupPrefix (string ns)
		{
			return LookupPrefix (ns, false);
		}

		public string LookupPrefix (string ns, bool atomizedNames)
		{
			if (current == null)
				return null;

			XmlElement el = GetCurrentElement ();

			for (; el != null; el = el.ParentNode as XmlElement) {
				for (int i = 0; i < el.Attributes.Count; i++) {
					XmlAttribute attr = el.Attributes [i];
					if (atomizedNames) {
						if (!Object.ReferenceEquals (attr.NamespaceURI, XmlNamespaceManager.XmlnsXmlns))
							continue;
						if (Object.ReferenceEquals (attr.Value, ns))
							// xmlns:blah="..." -> LocalName, xmlns="..." -> String.Empty
							return attr.Prefix != String.Empty ? attr.LocalName : String.Empty;
					} else {
						if (attr.NamespaceURI != XmlNamespaceManager.XmlnsXmlns)
							continue;
						if (attr.Value == ns)
							// xmlns:blah="..." -> LocalName, xmlns="..." -> String.Empty
							return attr.Prefix != String.Empty ? attr.LocalName : String.Empty;
					}
				}
			}
			switch (ns) {
			case XmlNamespaceManager.XmlnsXml:
				return XmlNamespaceManager.PrefixXml;
			case XmlNamespaceManager.XmlnsXmlns:
				return XmlNamespaceManager.PrefixXmlns;
			}
			return null;
		}
#endif

		public override void MoveToAttribute (int attributeIndex)
		{
			if (isEndElement || attributeIndex < 0 || attributeIndex > AttributeCount)
				throw new ArgumentOutOfRangeException ();
			
			state = ReadState.Interactive;
			current = ownerLinkedNode.Attributes [attributeIndex];
		}

		public override bool MoveToAttribute (string name)
		{
			if (isEndElement || current == null)
				return false;
			XmlNode tmpCurrent = current;
			if (current.ParentNode.NodeType == XmlNodeType.Attribute)
				current = current.ParentNode;

			if (ownerLinkedNode.Attributes == null)
				return false;
			XmlAttribute attr = ownerLinkedNode.Attributes [name];
			if (attr == null) {
				current = tmpCurrent;
				return false;
			}
			else {
				current = attr;
				return true;
			}
		}

		public override bool MoveToAttribute (string name, string namespaceURI)
		{
			if (isEndElement || current == null)
				return false;

			if (ownerLinkedNode.Attributes == null)
				return false;
			XmlAttribute attr = ownerLinkedNode.Attributes [name, namespaceURI];
			if (attr == null)
				return false;
			else {
				current = attr;
				return true;
			}
		}

		public override bool MoveToElement ()
		{
			if (current == null)
				return false;
			XmlNode n = ownerLinkedNode;
			if (current != n) {
				current = n;
				return true;
			} else 
				return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (current == null)
				return false;

			if (ownerLinkedNode.Attributes == null)
				return false;
			if(ownerLinkedNode.Attributes.Count > 0)
			{
				current = ownerLinkedNode.Attributes [0];
				return true;
			}
			else
				return false;
		}

		public override bool MoveToNextAttribute ()
		{
			if (current == null)
				return false;

			XmlNode anode = current;
			if (current.NodeType != XmlNodeType.Attribute) {
				// then it's either an attribute child or anything on the tree.
				if (current.ParentNode == null ||  // document, or non-tree node
				    current.ParentNode.NodeType != XmlNodeType.Attribute) // not an attr value
					return MoveToFirstAttribute ();
				anode = current.ParentNode;
			}

			{
				XmlAttributeCollection ac = ((XmlAttribute) anode).OwnerElement.Attributes;
				for (int i=0; i<ac.Count-1; i++)
				{
					XmlAttribute attr = ac [i];
					if (attr == anode)
					{
						i++;
						if (i == ac.Count)
							return false;
						current = ac [i];
						return true;
					}
				}
				return false;
			}
		}

		public override bool Read ()
		{
			// FIXME: at some stage inlining might work effectively.
			// if (EOF)
			switch (state) {
			case ReadState.EndOfFile:
			case ReadState.Error:
			case ReadState.Closed:
				return false;
			}

#if NET_2_0
			if (Binary != null)
				Binary.Reset ();
#endif

			bool ret = ReadContent ();
			ownerLinkedNode = current;
			return ret;
		}

		bool ReadContent ()
		{
			if (ReadState == ReadState.Initial) {
				current = startNode;
				state = ReadState.Interactive;
				// when startNode is document or fragment
				if (ignoreStartNode)
					current = startNode.FirstChild;
				if (current == null) {
					state = ReadState.Error;
					return false;
				} else
					return true;
			}

			MoveToElement ();

			// don't step into EntityReference's children. Also
			// avoid re-entering children of already-consumed
			// element (i.e. when it is regarded as EndElement).
			XmlNode firstChild =
				!isEndElement && current.NodeType != XmlNodeType.EntityReference ?
				current.FirstChild : null;
			if (firstChild != null) {
				isEndElement = false;
				current = firstChild;
				depth++;
				return true;
			}

			if (current == startNode) { // Currently it is on the start node.
				if (IsEmptyElement || isEndElement) {
					// The start node is already consumed.
					isEndElement = false;
					current = null;
					state = ReadState.EndOfFile;
					return false;
				} else {
					// The start node is the only element
					// which should be processed. Now it
					// is set as EndElement.
					isEndElement = true;
					return true;
				}
			}
			if (!isEndElement && !IsEmptyElement &&
			    current.NodeType == XmlNodeType.Element) {
				// element, currently not EndElement, and has
				// no child. (such as <foo></foo>, which
				// should become EndElement).
				isEndElement = true;
				return true;
			}

			// If NextSibling is available, move to there.
			XmlNode next = current.NextSibling;
			if (next != null) {
				isEndElement = false;
				current = next;
				return true;
			}

			// Otherwise, parent.
			XmlNode parent = current.ParentNode;
			if (parent == null || parent == startNode && ignoreStartNode) {
				// Parent is not available, or reached to
				// the start node. This reader never sets 
				// startNode as current if it was originally 
				// ignored (e.g. startNode is XmlDocument).
				isEndElement = false;
				current = null;
				state = ReadState.EndOfFile;
				return false;
			} else {
				// Parent was available, so return it as
				// EndElement.
				current = parent;
				depth--;
				isEndElement = true;
				return true;
			}
		}

		public override bool ReadAttributeValue ()
		{
			if (current.NodeType == XmlNodeType.Attribute) {
				if (current.FirstChild == null)
					return false;
				current = current.FirstChild;
				return true;
			} else if (current.ParentNode.NodeType == XmlNodeType.Attribute) {
				if (current.NextSibling == null)
					return false;
				current = current.NextSibling;
				return true;
			} else
				return false;
		}

		public override string ReadString ()
		{
			return base.ReadString ();
		}

#if !MOONLIGHT
		public override void ResolveEntity ()
		{
			throw new NotSupportedException ("Should not happen.");
		}
#endif

		public override void Skip ()
		{
			// Why is this overriden? Such skipping might raise
			// (or ignore) unexpected validation error.
			base.Skip ();
		}
		#endregion
	}
}
