//
// System.Xml.XmlValidatingReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// Copyright (C) Tim Coleman, 2002
// (C)2003 Atsushi Enomoto
//

using System.IO;
using System.Text;
using System.Xml.Schema;
using Mono.Xml;

namespace System.Xml {
	public class XmlValidatingReader : XmlReader, IXmlLineInfo {

		#region Fields

		EntityHandling entityHandling;
		XmlReader sourceReader;
		XmlTextReader xmlTextReader;
		XmlReader validatingReader;
		XmlResolver resolver;
		ValidationType validationType;
		XmlSchemaCollection schemas;
		DTDValidatingReader dtdReader;
		IHasXmlSchemaInfo schemaInfo;

		#endregion // Fields

		#region Constructors

		public XmlValidatingReader (XmlReader reader)
		{
			this.sourceReader = reader;
			this.xmlTextReader = reader as XmlTextReader;
			entityHandling = EntityHandling.ExpandEntities;
			validationType = ValidationType.Auto;
			schemas = new XmlSchemaCollection ();
		}

		public XmlValidatingReader (Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (new XmlTextReader (xmlFragment, fragType, context))
		{
		}

		public XmlValidatingReader (string xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (new XmlTextReader (xmlFragment, fragType, context))
		{
		}

		#endregion // Constructors

		#region Properties

		public override int AttributeCount {
			get { return validatingReader == null ? 0 : validatingReader.AttributeCount; }
		}

		public override string BaseURI {
			get { return validatingReader == null ? sourceReader.BaseURI : validatingReader.BaseURI; }
		}

		// This property for this class always return true.
		public override bool CanResolveEntity {
			get { return true; }
		}

		public override int Depth { 
			get { return validatingReader == null ? 0 : validatingReader.Depth; }
		}

		public Encoding Encoding {
			get {
				if (xmlTextReader != null)
					return xmlTextReader.Encoding;
				else
					throw new NotSupportedException ("Encoding is supported only for XmlTextReader.");
			}
		}

		public EntityHandling EntityHandling {
			get { return entityHandling; }
			set { entityHandling = value; }
		}

		public override bool EOF { 
			get { return validatingReader == null ? false : validatingReader.EOF; }
		}

		public override bool HasValue { 
			get { return validatingReader == null ? false : validatingReader.HasValue; }
		}

		public override bool IsDefault {
			get { return validatingReader == null ? false : validatingReader.IsDefault; }
		}

		public override bool IsEmptyElement { 
			get { return validatingReader == null ? false : validatingReader.IsEmptyElement; }
		}

		public override string this [int i] { 
			get {
				if (validatingReader == null)
					throw new IndexOutOfRangeException ("Reader is not started.");
				return validatingReader [i];
			}
		}

		public override string this [string name] { 
			get { return validatingReader == null ? null : validatingReader [name]; }
		}

		public override string this [string localName, string namespaceName] { 
			get { return validatingReader == null ? null : validatingReader [localName, namespaceName]; }
		}

		int IXmlLineInfo.LineNumber {
			get {
				IXmlLineInfo info = validatingReader as IXmlLineInfo;
				return info != null ? info.LineNumber : 0;
			}
		}

		int IXmlLineInfo.LinePosition {
			get {
				IXmlLineInfo info = validatingReader as IXmlLineInfo;
				return info != null ? info.LinePosition : 0;
			}
		}

		public override string LocalName { 
			get {
				if (validatingReader == null)
					return String.Empty;
				else if (Namespaces)
					return validatingReader.LocalName;
				else
					return validatingReader.Name;
			}
		}

		public override string Name {
			get { return validatingReader == null ? String.Empty : validatingReader.Name; }
		}

		public bool Namespaces {
			get {
				if (xmlTextReader != null)
					return xmlTextReader.Namespaces;
				else
					return true;
			}
			set {
				if (ReadState != ReadState.Initial)
					throw new InvalidOperationException ("Namespaces have to be set before reading.");

				if (xmlTextReader != null)
					xmlTextReader.Namespaces = value;
				else
					throw new NotSupportedException ("Property 'Namespaces' is supported only for XmlTextReader.");
			}
		}

		public override string NamespaceURI { 
			get {
				if (validatingReader == null)
					return String.Empty;
				else if (Namespaces)
					return validatingReader.NamespaceURI;
				else
					return String.Empty;
			}
		}

		public override XmlNameTable NameTable { 
			get { return validatingReader == null ? sourceReader.NameTable : validatingReader.NameTable; }
		}

		public override XmlNodeType NodeType { 
			get { return validatingReader == null ? XmlNodeType.None : validatingReader.NodeType; }
		}

		public override string Prefix {
			[MonoTODO]
			get { return validatingReader == null ? String.Empty : validatingReader.Prefix; }
		}

		public override char QuoteChar { 
			get { return validatingReader == null ? sourceReader.QuoteChar : validatingReader.QuoteChar; }
		}

		public XmlReader Reader {
			get { return sourceReader; }
		}

		public override ReadState ReadState { 
			[MonoTODO]
			get {
				if (validatingReader == null)
					return ReadState.Initial;
				return validatingReader.ReadState; 
			}
		}

		public XmlSchemaCollection Schemas {
			get { return schemas; }
		}

		public object SchemaType {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ValidationType ValidationType {
			get { return validationType; }
			set {
				if (ReadState != ReadState.Initial)
					throw new InvalidOperationException ("ValidationType cannot be set after the first call to Read method.");
				switch (validationType) {
				case ValidationType.Auto:
				case ValidationType.DTD:
				case ValidationType.None:
					validationType = value; 
					break;
				case ValidationType.Schema:
				case ValidationType.XDR:
					throw new NotImplementedException ();
				}
			}
		}

		public override string Value {
			get { return validatingReader == null ? String.Empty : validatingReader.Value; }
		}

		public override string XmlLang {
			get { return validatingReader == null ? String.Empty : validatingReader.XmlLang; }
		}

		public XmlResolver XmlResolver {
			[MonoTODO]
			set {
				resolver = value;
//				XmlSchemaValidatingReader xsvr = validatingReader as XmlSchemaValidatingReader;
//				if (xsvr != null)
//					xsvr.XmlResolver = value;
				DTDValidatingReader dvr = validatingReader as DTDValidatingReader;
				if (dvr != null)
					dvr.XmlResolver = value;
			}
		}

		public override XmlSpace XmlSpace {
			[MonoTODO]
			get { return validatingReader == null ? XmlSpace.None : validatingReader.XmlSpace; }
		}

		#endregion // Properties

		#region Methods

		public override void Close ()
		{
			if (validatingReader == null)
				sourceReader.Close ();
			else
				validatingReader.Close ();
		}

		public override string GetAttribute (int i)
		{
			return this [i];
		}

		public override string GetAttribute (string name)
		{
			return this [name];
		}

		public override string GetAttribute (string localName, string namespaceName)
		{
			return this [localName, namespaceName];
		}

		internal XmlParserContext GetInternalParserContext ()
		{
			return dtdReader != null ?
				dtdReader.ParserContext : null;
		}

		bool IXmlLineInfo.HasLineInfo ()
		{
			IXmlLineInfo info = validatingReader as IXmlLineInfo;
			return info != null ? info.HasLineInfo () : false;
		}

		public override string LookupNamespace (string prefix)
		{
			if (validatingReader != null)
				return sourceReader.LookupNamespace (prefix);
			else
				return validatingReader.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int i)
		{
			if (validatingReader == null)
				throw new IndexOutOfRangeException ("Reader is not started.");
			else
				validatingReader.MoveToAttribute (i);
		}

		public override bool MoveToAttribute (string name)
		{
			if (validatingReader == null)
				return false;
			return validatingReader.MoveToAttribute (name);
		}

		public override bool MoveToAttribute (string localName, string namespaceName)
		{
			if (validatingReader == null)
				return false;
			return validatingReader.MoveToAttribute (localName, namespaceName);
		}

		public override bool MoveToElement ()
		{
			if (validatingReader == null)
				return false;
			return validatingReader.MoveToElement ();
		}

		public override bool MoveToFirstAttribute ()
		{
			if (validatingReader == null)
				return false;
			return validatingReader.MoveToFirstAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			if (validatingReader == null)
				return false;
			return validatingReader.MoveToNextAttribute ();
		}

		[MonoTODO]
		public override bool Read ()
		{
			if (ReadState == ReadState.Initial) {
				switch (ValidationType) {
				case ValidationType.Auto:
				case ValidationType.None:
					if (schemas.Count > 0)
						goto case ValidationType.Schema;
					else
						goto case ValidationType.DTD;
				case ValidationType.DTD:
					validatingReader = dtdReader = new DTDValidatingReader (sourceReader, this);
					break;
				case ValidationType.Schema:
//					dtdReader = new DTDValidatingReader (sourceReader, this);
//					validatingReader = new XmlSchemaValidatingReader (dtdReader, this);
//					break;
				case ValidationType.XDR:
					throw new NotImplementedException ();
				}
			}
			return validatingReader.Read ();
		}

		public override bool ReadAttributeValue ()
		{
			if (validatingReader == null)
				return false;
			return validatingReader.ReadAttributeValue ();
		}

#if NET_1_0
		// LAMESPEC: MS.NET 1.0 has critical bug here.
		// After calling these methods, validation does no more work!
		[MonoTODO]
		public override string ReadInnerXml ()
		{
			if (validatingReader == null)
				return "";
			return validatingReader.ReadInnerXml ();
		}

		[MonoTODO]
		public override string ReadOuterXml ()
		{
			if (validatingReader == null)
				return "";
			return validatingReader.ReadOuterXml ();
		}
#endif

		[MonoTODO]
#if NET_1_0
		public override string ReadString ()
		{
			return base.ReadStringInternal ();
		}
#else
		public override string ReadString ()
		{
			return base.ReadString ();
		}
#endif

		[MonoTODO]
		public object ReadTypedValue ()
		{
			if (dtdReader == null)
				return null;
			XmlSchemaDatatype dt = schemaInfo.SchemaType as XmlSchemaDatatype;
			if (dt == null)
				return null;
			switch (NodeType) {
			case XmlNodeType.Element:
				return dt.ParseValue (ReadString (), NameTable, dtdReader.ParserContext.NamespaceManager);
			case XmlNodeType.Attribute:
				return dt.ParseValue (Value, NameTable, dtdReader.ParserContext.NamespaceManager);
			}
			return null;
		}

		public override void ResolveEntity ()
		{
			validatingReader.ResolveEntity ();
		}

		// It should be "protected" as usual "event model"
		// methods are, but validation event is not exposed,
		// so it is no other way to make it "internal".
		internal void OnValidationEvent (object o, ValidationEventArgs e)
		{
			if (ValidationEventHandler != null)
				ValidationEventHandler (o, e);
			else if (ValidationType != ValidationType.None)
				throw e.Exception;
		}
		#endregion // Methods

		#region Events and Delegates

		public event ValidationEventHandler ValidationEventHandler;

		#endregion // Events and Delegates
	}
}
