//
// System.Xml.XmlNodeReader.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;
using System.Xml;

namespace System.Xml
{
	public class XmlNodeReader : XmlReader
	{
		#region Constructor

		XmlNode current;
		ReadState state = ReadState.Initial;

		public XmlNodeReader (XmlNode node)
		{
			current = node;
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

		[MonoTODO]
		public override int Depth {
			get { return 0; }
		}

		[MonoTODO]
		public override bool EOF {
			get { return false; }
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

		[MonoTODO]
		public override bool IsDefault {
			get {
				if (current.NodeType != XmlNodeType.Attribute)
					return false;
				else
					return false;
			}
		}

		public override bool IsEmptyElement {
			get {
				if (current.NodeType == XmlNodeType.Entity &&
				    ((XmlEntity) current).Value.EndsWith ("/>"))
					return true;
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

		[MonoTODO]
		public override XmlNameTable NameTable {
			get { return null; }
		}

		public override XmlNodeType NodeType {
			get {
				return current.NodeType;
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
			get { return current.Value; }
		}

		[MonoTODO]
		public override string XmlLang {
			get { return String.Empty; }
		}

		[MonoTODO]
		public override XmlSpace XmlSpace {
			get { return 0; }
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

		[MonoTODO]
		public override string LookupNamespace (string prefix)
		{
			return null;
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

		[MonoTODO]
		public override bool MoveToFirstAttribute ()
		{
			return false;
		}

		[MonoTODO]
		public override bool MoveToNextAttribute ()
		{
			if (current.NodeType != XmlNodeType.Attribute)
				return MoveToFirstAttribute ();
			else
				return false;
		}

		[MonoTODO]
		public override bool Read ()
		{
			return false;
		}

		[MonoTODO]
		public override bool ReadAttributeValue ()
		{
			return false;
		}

		public override string ReadInnerXml ()
		{
			if (current.NodeType != XmlNodeType.Attribute &&
			    current.NodeType != XmlNodeType.Element)
				return String.Empty;
			else
				return current.InnerXml;
		}

		public override string ReadOuterXml ()
		{
			if (current.NodeType != XmlNodeType.Attribute &&
			    current.NodeType != XmlNodeType.Element)
				return String.Empty;
			else
				return current.OuterXml;
		}

		[MonoTODO]
		public override string ReadString ()
		{
			return null;
		}

		[MonoTODO]
		public override void ResolveEntity ()
		{
			if (current.NodeType != XmlNodeType.EntityReference)
				throw new InvalidOperationException ("The current node is not an Entity Reference");
		}

		[MonoTODO]
		public override void Skip ()
		{
		}
		#endregion
	}
}
