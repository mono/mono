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
		#region Constructor

		XmlNode startNode;
		XmlNode current;
		ReadState state = ReadState.Initial;
		int depth;
		bool isEndElement;
		bool isEndEntity;
		bool nextIsEndElement;	// used for ReadString()
		bool alreadyRead;

		public XmlNodeReader (XmlNode node)
		{
			startNode = node;
			if (node.NodeType != XmlNodeType.Document
				&& node.NodeType != XmlNodeType.DocumentFragment)
				alreadyRead = true;
		}
		
		#endregion

		#region Properties

		public override int AttributeCount {
			get {
				if (current == null)
					return 0;

				return ((ICollection) current.Attributes).Count;
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
				if (current == null)
					return false;

				if (current.Attributes == null)
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

		[MonoTODO("test it.")]
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
				if (current == null)
					return null;

				if (i < 0 || i > AttributeCount)
					throw new ArgumentOutOfRangeException ("i is out of range.");
				
				return current.Attributes [i].Value;
			}
		}

		public override string this [string name] {
			get {
				if (current == null)
					return null;

				string ret =  current.Attributes [name].Value;
				
				if (ret == null)
					return String.Empty;
				else
					return ret;
			}
		}

		public override string this [string name, string namespaceURI] {
			get {
				if (current == null)
					return null;

				string ret =  current.Attributes [name, namespaceURI].Value;
				
				if (ret == null)
					return String.Empty;
				else
					return ret;
			}
		}

		public override string LocalName {
			get {
				if (current == null)
					return String.Empty;

				if (current is XmlCharacterData)
					return String.Empty;
				else
					return current.LocalName;
			}
		}

		public override string Name {
			get {
				if (current == null)
					return String.Empty;

				return current.Name;
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
			get {
				XmlDocument doc = 
					current.NodeType == XmlNodeType.Document ?
					current as XmlDocument : current.OwnerDocument;
				return doc.NameTable;
			}
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

		// FIXME: Its performance is not good.
		public override string LookupNamespace (string prefix)
		{
			XmlNamespaceManager nsmgr = current.ConstructNamespaceManager();
			return nsmgr.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int attributeIndex)
		{
			if (attributeIndex < 0 || attributeIndex > AttributeCount)
				throw new ArgumentOutOfRangeException ();
			
			state = ReadState.Interactive;
			current = current.Attributes [attributeIndex];
		}

		public override bool MoveToAttribute (string name)
		{
			if (GetAttribute (name) == null)
				return false;
			else {
				current = current.Attributes [name];
				return true;
			}
		}

		public override bool MoveToAttribute (string name, string namespaceURI)
		{
			if (GetAttribute (name, namespaceURI) == null)
				return false;
			else {
				current = current.Attributes [name, namespaceURI];
				return true;
			}
		}

		private void MoveToEndElement ()
		{
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
			if(current.Attributes.Count > 0)
			{
				current = current.Attributes [0];
				return true;
			}
			else
				return false;
		}

		public override bool MoveToNextAttribute ()
		{
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
				MoveToEndElement ();
			} else if (alreadyRead) {
				alreadyRead = false;
				return current != null;
			}
			if (current.NextSibling != null) {
				isEndElement = false;
				current = current.NextSibling;
			} else {
				MoveToEndElement ();
			}
			return current != null;
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

			if (isEndElement) {
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

			// hmm... here may be unnecessary codes. plz check anyone ;)
			if (!isEndElement && current.FirstChild != null) {
				isEndElement = false;
				current = current.FirstChild;
				depth++;
			} else if (depth == 0) {
				state = ReadState.EndOfFile;
				return false;
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

		[MonoTODO("Need to move to next content.")]
		// Its traversal behavior is almost same as Read().
		public override string ReadInnerXml ()
		{
			if (ReadState == ReadState.Initial) {
				state = ReadState.Error;
				return String.Empty;	// heh
			}
		
			if (current.NodeType != XmlNodeType.Attribute &&
			    current.NodeType != XmlNodeType.Element)
				return String.Empty;
			else
				return current.InnerXml;
		}

		[MonoTODO("Need to move to next content.")]
		// Its traversal behavior is almost same as Read().
		public override string ReadOuterXml ()
		{
			if (NodeType == XmlNodeType.EndElement)
				return String.Empty;

			if (current.NodeType != XmlNodeType.Attribute &&
			    current.NodeType != XmlNodeType.Element)
				return String.Empty;
			else
				return current.OuterXml;
		}

		public override string ReadString ()
		{
			if (NodeType == XmlNodeType.EndElement)
				return String.Empty;

			XmlNode original = current;
			StringBuilder builder = new StringBuilder();
			if (NodeType == XmlNodeType.Element) {
				foreach (XmlNode child in current.ChildNodes) {
					if (child is XmlCharacterData && !(child is XmlComment))
						builder.Append (child.Value);
					else {
						depth++;
						current = child;
						break;
					}
				}
				alreadyRead = true;
				if (current == original) {
					nextIsEndElement = true;
					Read ();
				}
			} else {
				do {
					builder.Append (current.Value);
					if (current.NextSibling == null) {
						nextIsEndElement = true;
						break;
					} else if (current.NextSibling.NodeType == XmlNodeType.Comment)
						break;
					else
						current = current.NextSibling;
				} while (true);
				alreadyRead = true;
				if (current.NextSibling == null) {
					nextIsEndElement = true;
					Read ();
				}
			}
			return builder.ToString ();
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
			MoveToElement ();
			if(current.ChildNodes.Count > 0)
				MoveToNextSibling ();
			else
				Read ();
		}
		#endregion
	}
}
