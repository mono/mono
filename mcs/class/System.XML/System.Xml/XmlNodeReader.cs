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

		XmlNode current;
		ReadState state = ReadState.Initial;
		int depth;
		bool isEndElement;
		bool isEndEntity;
		bool nextIsEndElement;
		bool alreadyRead;

		public XmlNodeReader (XmlNode node)
		{
			current = node;
			if (node.NodeType != XmlNodeType.Document
				&& node.NodeType != XmlNodeType.DocumentFragment)
				alreadyRead = true;
		}
		
		#endregion

		#region Properties

		public override int AttributeCount {
			get {
				return ((ICollection) current.Attributes).Count;
			}
		}

		public override string BaseURI {
			get { return current.BaseURI; }
		}

		public override bool CanResolveEntity {
			get { return false; }
		}

		public override int Depth {
			get { return depth; }
		}

		public override bool EOF {
			get { return this.ReadState == ReadState.EndOfFile; }
		}

		public override bool HasAttributes {
			get {
				if (current.Attributes == null)
					return false;
				else
					return true;
			}
		}

		public override bool HasValue {
			get {
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
				if(current.NodeType == XmlNodeType.Element)
					return ((XmlElement) current).IsEmpty;
				else 
					return false;
			}
		}

		public override string this [int i] {
			get {
				if (i < 0 || i > AttributeCount)
					throw new ArgumentOutOfRangeException ("i is out of range.");
				
				return current.Attributes [i].Value;
			}
		}

		public override string this [string name] {
			get {
				string ret =  current.Attributes [name].Value;
				
				if (ret == null)
					return String.Empty;
				else
					return ret;
			}
		}

		public override string this [string name, string namespaceURI] {
			get {
				string ret =  current.Attributes [name].Value;
				
				if (ret == null)
					return String.Empty;
				else
					return ret;
			}
		}

		public override string LocalName {
			get {
				if (current is XmlCharacterData)
					return String.Empty;
				else
					return current.LocalName;
			}
		}

		public override string Name {
			get { return current.Name; }
		}

		public override string NamespaceURI {
			get {
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
				return isEndElement ? XmlNodeType.EndElement : current.NodeType;
			}
		}

		public override string Prefix {
			get { 
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
			get { return current.XmlLang; }
		}

		public override XmlSpace XmlSpace {
			get { return current.XmlSpace; }
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
			else
				return true;
		}

		public override bool MoveToAttribute (string name, string namespaceURI)
		{
			if (GetAttribute (name, namespaceURI) == null)
				return false;
			else
				return true;
		}

		public override bool MoveToElement ()
		{
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

		[MonoTODO("Entity handling is not supported.")]
		public override bool Read ()
		{
			state = ReadState.Interactive;

			isEndEntity = false;

			if (current.NodeType == XmlNodeType.Attribute)
				current = ((XmlAttribute) current).OwnerElement;

			if (nextIsEndElement) {
				nextIsEndElement = false;
				isEndElement = true;
			} else if (alreadyRead) {
				alreadyRead = false;
				return current != null;
			}

			if (!isEndElement && current.FirstChild != null) {
				isEndElement = false;
				current = current.FirstChild;
				depth++;
			} else if (depth == 0) {
				state = ReadState.EndOfFile;
				return false;
			} else if (current.NextSibling != null) {
				isEndElement = false;
				current = current.NextSibling;
			} else {
				isEndElement = true;
				depth--;
				current = current.ParentNode;
			}
			return current != null;
		}

		public override bool ReadAttributeValue ()
		{
			if (current is XmlAttribute) {
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
			if (current.NodeType != XmlNodeType.Attribute &&
			    current.NodeType != XmlNodeType.Element)
				return String.Empty;
			else
				return current.OuterXml;
		}

		[MonoTODO("test it.")]
		public override string ReadString ()
		{
			XmlNode original = current;
			StringBuilder builder = new StringBuilder();
			foreach (XmlNode child in current.ChildNodes)
			{
				if (child is XmlCharacterData)
					builder.Append (child.Value);
				else {
					depth++;
					current = child;
					break;
				}
			}
			alreadyRead = true;
			if (current == original)
				nextIsEndElement = true;
			return builder.ToString ();
		}

		[MonoTODO]
		public override void ResolveEntity ()
		{
			if (current.NodeType != XmlNodeType.EntityReference)
				throw new InvalidOperationException ("The current node is not an Entity Reference");
		}

		[MonoTODO("test it.")]
		public override void Skip ()
		{
			if (current.NodeType == XmlNodeType.Attribute)
				current = ((XmlAttribute) current).OwnerElement.NextSibling;
			else 
			{
				if(current.ChildNodes.Count > 0) {
					current = current.FirstChild;
					depth++;
				} else if (current.NextSibling != null) {
					current = current.NextSibling;
				} else if (current.NodeType == XmlNodeType.Attribute) {
					current = current.ParentNode;
				} else {
					depth--;
				}
			}
		}
		#endregion
	}
}
