//
// System.Xml.XmlValidatingReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Text;
using System.Xml.Schema;

namespace System.Xml {
	public class XmlValidatingReader : XmlReader, IXmlLineInfo {

		#region Fields

		EntityHandling entityHandling;
		bool namespaces;
		XmlReader reader;
		ValidationType validationType;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public XmlValidatingReader (XmlReader reader)
			: base ()
		{
			if (!(reader is XmlTextReader))
				throw new ArgumentException ();

			this.reader = reader;
			entityHandling = EntityHandling.ExpandEntities;
			namespaces = true;
			validationType = ValidationType.Auto;
		}

		[MonoTODO]
		public XmlValidatingReader (Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (new XmlTextReader (xmlFragment))
		{
		}

		public XmlValidatingReader (string xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (new XmlTextReader (xmlFragment))
		{
		}

		#endregion // Constructors

		#region Properties

		public override int AttributeCount {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string BaseURI {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override bool CanResolveEntity {
			get { return true; }
		}

		public override int Depth { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public Encoding Encoding {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public EntityHandling EntityHandling {
			get { return entityHandling; }
			set { entityHandling = value; }
		}

		public override bool EOF { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override bool HasValue { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override bool IsDefault {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override bool IsEmptyElement { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string this [int i] { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string this [string name] { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string this [string localName, string namespaceName] { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		int IXmlLineInfo.LineNumber {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		int IXmlLineInfo.LinePosition {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string LocalName { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string Name {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public bool Namespaces {
			get { return namespaces; }
			set { namespaces = value; }
		}

		public override string NamespaceURI { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override XmlNameTable NameTable { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override XmlNodeType NodeType { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string Prefix { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override char QuoteChar { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public XmlReader Reader {
			get { return reader; }
		}

		public override ReadState ReadState { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public XmlSchemaCollection Schemas {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public object SchemaType {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public ValidationType ValidationType {
			get { return validationType; }
			[MonoTODO ("Need to check for exception.")]
			set { validationType = value; }
		}

		public override string Value {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public override string XmlLang {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		XmlResolver XmlResolver {
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		public override XmlSpace XmlSpace {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void Close ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetAttribute (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetAttribute (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetAttribute (string localName, string namespaceName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IXmlLineInfo.HasLineInfo ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string LookupNamespace (string prefix)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void MoveToAttribute (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool MoveToAttribute (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool MoveToAttribute (string localName, string namespaceName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool MoveToElement ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool MoveToFirstAttribute ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool MoveToNextAttribute ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Read ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool ReadAttributeValue ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ReadInnerXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ReadOuterXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ReadString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void ResolveEntity ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

		#region Events and Delegates

		public event ValidationEventHandler ValidationEventHandler;

		#endregion // Events and Delegates
	}
}
