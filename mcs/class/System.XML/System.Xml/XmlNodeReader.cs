//
// System.Xml.XmlNodeReader.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//	Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C) Ximian, Inc.
// (C) Atsushi Enomoto
//

using System;
using System.Collections;
using System.Xml;
using System.Text;

namespace System.Xml
{
	public class XmlNodeReader : XmlReader
	{
		XmlDocument document;
		XmlNode startNode;
		XmlNode current;
		ReadState state = ReadState.Initial;
		int depth;
		bool isEndElement;
		bool isEndEntity;
		bool nextIsEndElement;	// used for ReadString()
		bool alreadyRead;
		StringBuilder valueBuilder = new StringBuilder ();
		XmlNamespaceManager defaultNsmgr;

		private XmlNode ownerElement {
			get {
				return (current.NodeType == XmlNodeType.Attribute) ? ((XmlAttribute)current).OwnerElement : current;
			}
		}

		#region Constructor

		public XmlNodeReader (XmlNode node)
		{
			startNode = node;
			document = startNode.NodeType == XmlNodeType.Document ?
				startNode as XmlDocument : startNode.OwnerDocument;

			if (node.NodeType != XmlNodeType.Document
				&& node.NodeType != XmlNodeType.DocumentFragment)
				alreadyRead = true;
			defaultNsmgr = new XmlNamespaceManager (this.NameTable);

		}
		
		#endregion

		#region Properties

		public override int AttributeCount {
			get {
				if (isEndElement || current == null || current.Attributes == null)
					return 0;
				return ownerElement.Attributes.Count;
			}
		}

		public override string BaseURI {
			get { 
				if (current == null)
					return String.Empty;
				return current.BaseURI;
			}
		}

		[MonoTODO("wait for XML resolver")]
		public override bool CanResolveEntity {
			get {
				throw new NotImplementedException ();
			}
		}

		public override int Depth {
			get { return depth; }
		}

		public override bool EOF {
			get {
				return this.ReadState == ReadState.EndOfFile 
				|| this.ReadState == ReadState.Error;
			}
		}

		public override bool HasAttributes {
			get {
				if (isEndElement || current == null)
					return false;

				if (current.Attributes == null ||
					current.Attributes.Count == 0)
					return false;
				else
					return true;
			}
		}

		public override bool HasValue {
			get {
				if (current == null)
					return false;

				if (current.NodeType == XmlNodeType.Element ||
				    current.NodeType == XmlNodeType.EntityReference ||
				    current.NodeType == XmlNodeType.Document ||
				    current.NodeType == XmlNodeType.DocumentFragment ||
				    current.NodeType == XmlNodeType.Notation ||
				    current.NodeType == XmlNodeType.EndElement ||
				    current.NodeType == XmlNodeType.EndEntity)
					return false;
				else
					return true;
			}
			      
		}

		[MonoTODO("waiting for DTD implementation")]
		public override bool IsDefault {
			get {
				if (current == null)
					return false;

				if (current.NodeType != XmlNodeType.Attribute)
					return false;
				else
				{
					return ((XmlAttribute) current).isDefault;
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

		public override string this [int i] {
			get {
				// This is MS.NET bug which returns attributes in spite of EndElement.
				if (isEndElement || current == null)
					return null;

				if (NodeType == XmlNodeType.XmlDeclaration) {
					XmlDeclaration decl = current as XmlDeclaration;
					switch (i) {
					case 0:
						return decl.Version;
					case 1:
						if (decl.Encoding != String.Empty)
							return decl.Encoding;
						else if (decl.Standalone != String.Empty)
							return decl.Standalone;
						else
							throw new ArgumentOutOfRangeException ("Index out of range.");
					case 2:
						if (decl.Encoding != String.Empty && decl.Standalone != null)
							return decl.Standalone;
						else
							throw new ArgumentOutOfRangeException ("Index out of range.");
					}
				}

				if (i < 0 || i > AttributeCount)
					throw new ArgumentOutOfRangeException ("Index out of range.");

				return ownerElement.Attributes [i].Value;
			}
		}

		private string GetXmlDeclarationAttribute (string name)
		{
			XmlDeclaration decl = current as XmlDeclaration;
			switch (name) {
			case "version":
				return decl.Version;
			case "encoding":
				return decl.Encoding;
			case "standalone":
				return decl.Standalone;
			}
			return null;
		}

		public override string this [string name] {
			get {
				// This is MS.NET bug which returns attributes in spite of EndElement.
				if (isEndElement || current == null)
					return null;

				if (NodeType == XmlNodeType.XmlDeclaration)
					return GetXmlDeclarationAttribute (name);

				XmlAttribute attr = ownerElement.Attributes [name];
				if (attr == null)
					return String.Empty;
				else
					return attr.Value;
			}
		}

		public override string this [string name, string namespaceURI] {
			get {
				// This is MS.NET bug which returns attributes in spite of EndElement.
				if (isEndElement || current == null)
					return null;

				if (NodeType == XmlNodeType.XmlDeclaration)
					return GetXmlDeclarationAttribute (name);

				XmlAttribute attr = ownerElement.Attributes [name, namespaceURI];
				if (attr == null)
					return String.Empty;
				else
					return attr.Value;
			}
		}

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

		public override char QuoteChar {
			get { return '"'; }
		}

		public override ReadState ReadState {
			get { return state; }
		}

		public override string Value {
			get {
				return HasValue ? current.Value : String.Empty;
			}
		}

		public override string XmlLang {
			get {
				if (current == null)
					return String.Empty;

				return current.XmlLang;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				if (current == null)
					return XmlSpace.None;

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
			return this [attributeIndex];
		}

		public override string GetAttribute (string name)
		{
			return this [name];
		}

		public override string GetAttribute (string name, string namespaceURI)
		{
			return this [name, namespaceURI];
		}

		public override string LookupNamespace (string prefix)
		{
			if (current == null)
				return null;

			XmlAttribute curAttr = current as XmlAttribute;
			XmlNode target = curAttr != null ? curAttr.OwnerElement : current;

			if (prefix == "") {
				do {
					XmlAttribute attr = target.Attributes ["xmlns"];
					if (attr != null)
						return attr.Value;
					target = target.ParentNode;
				} while (target.NodeType != XmlNodeType.Document);
			} else {
				string name = "xmlns:" + prefix;
				do {
					XmlAttribute attr = target.Attributes [name];
					if (attr != null)
						return attr.Value;
					target = target.ParentNode;
				} while (target.NodeType != XmlNodeType.Document);
			}
			return defaultNsmgr.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int attributeIndex)
		{
			if (isEndElement || attributeIndex < 0 || attributeIndex > AttributeCount)
				throw new ArgumentOutOfRangeException ();
			
			state = ReadState.Interactive;
			current = ownerElement.Attributes [attributeIndex];
		}

		public override bool MoveToAttribute (string name)
		{
			if (isEndElement || current == null)
				return false;

			XmlAttribute attr = ownerElement.Attributes [name];
			if (attr == null)
				return false;
			else {
				current = attr;
				return true;
			}
		}

		public override bool MoveToAttribute (string name, string namespaceURI)
		{
			if (isEndElement || current == null)
				return false;

			XmlAttribute attr = ownerElement.Attributes [name, namespaceURI];
			if (attr == null)
				return false;
			else {
				current = attr;
				return true;
			}
		}

		private void MoveToParentElement ()
		{
			// This is buggy. It is not only the case when EndElement = true.
			isEndElement = true;
			depth--;
			current = current.ParentNode;
		}

		public override bool MoveToElement ()
		{
			if (current == null)
				return false;
			if (current.NodeType == XmlNodeType.Attribute) {
				current = ((XmlAttribute) current).OwnerElement;
				return true;
			} else 
				return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (current == null)
				return false;

			if(ownerElement.Attributes.Count > 0)
			{
				current = ownerElement.Attributes [0];
				return true;
			}
			else
				return false;
		}

		public override bool MoveToNextAttribute ()
		{
			if (current == null)
				return false;

			if (current.NodeType != XmlNodeType.Attribute)
				return MoveToFirstAttribute ();
			else
			{
				XmlAttributeCollection ac = ((XmlAttribute) current).OwnerElement.Attributes;
				for (int i=0; i<ac.Count-1; i++)
				{
					XmlAttribute attr = ac [i];
					if (attr == current)
					{
						current = ac [i+1];
						return true;
					}
				}
				return false;
			}
		}

		private bool MoveToNextSibling ()
		{
			if (nextIsEndElement) {
				// nextIsEndElement is set only by ReadString.
				nextIsEndElement = false;
				MoveToParentElement ();
			} else if (alreadyRead) {
				alreadyRead = false;
				return current != null;
			}
			if (current.NextSibling != null) {
				isEndElement = false;
				current = current.NextSibling;
			} else {
				MoveToParentElement ();
			}
			if (current == null) {
				state = ReadState.EndOfFile;
				return false;
			}
			else
				return true;
		}

		[MonoTODO("Entity handling is not supported.")]
		public override bool Read ()
		{
			if (EOF)
				return false;

			if (ReadState == ReadState.Initial) {
				current = startNode;
				state = ReadState.Interactive;
				// when startNode is document or fragment
				if (!alreadyRead)
					current = startNode.FirstChild;
				else
					alreadyRead = false;
				if (current == null) {
					state = ReadState.Error;
					return false;
				} else
					return true;
			}

			MoveToElement ();
			isEndEntity = false;

			if (IsEmptyElement || isEndElement) {
				// Then go up and move to next.
				// If no more nodes, then set EOF.
				isEndElement = false;
				if (current.ParentNode == null
					|| current.ParentNode.NodeType == XmlNodeType.Document
					|| current.ParentNode.NodeType == XmlNodeType.DocumentFragment) {
					current = null;
					state = ReadState.EndOfFile;
					return false;
				} else if (current.NextSibling == null) {
					depth--;
					current = current.ParentNode;
					isEndElement = true;
					return true;
				} else {
					current = current.NextSibling;
					return true;
				}

			} else if (nextIsEndElement) {
				// nextIsEndElement is set only by ReadString.
				nextIsEndElement = false;
				isEndElement = true;
				return current != null;

			} else if (alreadyRead) {
				alreadyRead = false;
				return current != null;
			}

			if (!isEndElement && current.FirstChild != null) {
				isEndElement = false;
				current = current.FirstChild;
				depth++;
			} else if (current.NodeType == XmlNodeType.Element) {
				isEndElement = true;
				if (current.FirstChild != null)
					depth--;
			} else
				MoveToNextSibling ();

			return current != null;
		}

		public override bool ReadAttributeValue ()
		{
			if (current.NodeType == XmlNodeType.Attribute) {
				current = current.FirstChild;
				return current != null;
			} else if (current.ParentNode.NodeType == XmlNodeType.Attribute) {
				current = current.NextSibling;
				return current != null;
			} else
				return false;
		}

		// Its traversal behavior is almost same as Read().
		public override string ReadInnerXml ()
		{
			XmlNode initial = current;
			// Almost copied from XmlTextReader.
			switch (NodeType) {
			case XmlNodeType.Attribute:
				return Value;
			case XmlNodeType.Element:
				if (IsEmptyElement)
					return String.Empty;

				int startDepth = depth;

				bool loop = true;
				do {
					Read ();
					if (NodeType ==XmlNodeType.None)
						throw new XmlException ("unexpected end of xml.");
					else if (NodeType == XmlNodeType.EndElement && depth == startDepth) {
						loop = false;
						Read ();
					}
				} while (loop);
				return initial.InnerXml;
			case XmlNodeType.None:
				return String.Empty;
			default:
				Read ();
				return String.Empty;
			}
		}

		[MonoTODO("Need to move to next content.")]
		// Its traversal behavior is almost same as Read().
		public override string ReadOuterXml ()
		{
			if (NodeType == XmlNodeType.EndElement)
				return String.Empty;
/*
			if (current.NodeType != XmlNodeType.Attribute &&
			    current.NodeType != XmlNodeType.Element)
				return String.Empty;
			else
				return current.OuterXml;
*/
			XmlNode initial = current;

			switch (NodeType) {
			case XmlNodeType.Attribute:
				return current.OuterXml;
			case XmlNodeType.Element:
				if (NodeType == XmlNodeType.Element && !IsEmptyElement)
					ReadInnerXml ();
				else
					Read ();
				return initial.OuterXml;
			case XmlNodeType.None:
				return String.Empty;
			default:
				Read ();
				return String.Empty;
			}
		}

		public override string ReadString ()
		{
			return ReadStringInternal ();
		}

		[MonoTODO]
		public override void ResolveEntity ()
		{
			throw new NotImplementedException ();
//			if (current.NodeType != XmlNodeType.EntityReference)
//				throw new InvalidOperationException ("The current node is not an Entity Reference");
		}

		[MonoTODO("test it.")]
		public override void Skip ()
		{
			// Why is this overriden? Such skipping might raise
			// (or ignore) unexpected validation error.
			base.Skip ();
		}
		#endregion
	}
}
