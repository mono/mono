//
// System.Xml.XmlNodeReader.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Xml
{
	public class XmlNodeReader : XmlReader
	{
		#region Constructor

		[MonoTODO]
		public XmlNodeReader (XmlNode node)
		{
		}
		
		#endregion

		#region Properties

		[MonoTODO]
		public override int AttributeCount {
			get { return 0; }
		}

		[MonoTODO]
		public override string BaseURI {
			get { return null; }
		}

		[MonoTODO]
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

		[MonoTODO]
		public override bool HasAttributes {
			get { return false; }
		}

		[MonoTODO]
		public override bool HasValue {
			get { return false; }
		}

		[MonoTODO]
		public override bool IsDefault {
			get { return false; }
		}

		[MonoTODO]
		public override bool IsEmptyElement {
			get { return false; }
		}

		[MonoTODO]
		public override string this [int i] {
			get { return null; }
		}

		[MonoTODO]
		public override string this [string name] {
			get { return null; }
		}

		[MonoTODO]
		public override string this [string name, string namespaceURI] {
			get { return null; }
		}

		[MonoTODO]
		public override string LocalName {
			get { return null; }
		}

		[MonoTODO]
		public override string Name {
			get { return null; }
		}

		[MonoTODO]
		public override string NamespaceURI {
			get { return null; }
		}

		[MonoTODO]
		public override XmlNameTable NameTable {
			get { return null; }
		}

		[MonoTODO]
		public override XmlNodeType NodeType {
			get { return 0; }
		}

		[MonoTODO]
		public override string Prefix {
			get { return null; }
		}

		public override char QuoteChar {
			get { return '"'; }
		}

		[MonoTODO]
		public override ReadState ReadState {
			get { return 0; }
		}

		[MonoTODO]
		public override string Value {
			get { return null; }
		}

		[MonoTODO]
		public override string XmlLang {
			get { return null; }
		}

		[MonoTODO]
		public override XmlSpace XmlSpace {
			get { return 0; }
		}
		#endregion

		#region Methods

		[MonoTODO]
		public override void Close ()
		{
		}

		[MonoTODO]
		public override string GetAttribute (int attributeIndex)
		{
			return null;
		}

		[MonoTODO]
		public override string GetAttribute (string name)
		{
			return null;
		}

		[MonoTODO]
		public override string GetAttribute (string name, string namespaceURI)
		{
			return null;
		}

		[MonoTODO]
		public override string LookupNamespace (string prefix)
		{
			return null;
		}

		[MonoTODO]
		public override void MoveToAttribute (int attributeIndex)
		{
		}

		[MonoTODO]
		public override bool MoveToAttribute (string name)
		{
			return false;
		}

		[MonoTODO]
		public override bool MoveToAttribute (string name, string namespaceURI)
		{
			return false;
		}

		[MonoTODO]
		public override bool MoveToElement ()
		{
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

		[MonoTODO]
		public override string ReadInnerXml ()
		{
			return null;
		}

		[MonoTODO]
		public override string ReadOuterXml ()
		{
			return null;
		}

		[MonoTODO]
		public override string ReadString ()
		{
			return null;
		}

		[MonoTODO]
		public override void ResolveEntity ()
		{
		}

		[MonoTODO]
		public override void Skip ()
		{
		}
		#endregion
	}
}
