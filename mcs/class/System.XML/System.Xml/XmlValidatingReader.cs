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
		XmlReader validatingReader;
		XmlResolver resolver;
		ValidationType validationType;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public XmlValidatingReader (XmlReader reader)
			: base ()
		{
			this.sourceReader = reader;
			entityHandling = EntityHandling.ExpandEntities;
			validationType = ValidationType.Auto;
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
			[MonoTODO]
			get { return validatingReader == null ? 0 : validatingReader.AttributeCount; }
		}

		public override string BaseURI {
			[MonoTODO]
			get { return validatingReader == null ? sourceReader.BaseURI : validatingReader.BaseURI; }
		}

		public override bool CanResolveEntity {
			get { return validatingReader == null ? false : validatingReader.CanResolveEntity; }
		}

		public override int Depth { 
			[MonoTODO]
			get { return validatingReader == null ? 0 : validatingReader.Depth; }
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
			get { return validatingReader == null ? false : validatingReader.EOF; }
		}

		public override bool HasValue { 
			[MonoTODO]
			get { return validatingReader == null ? false : validatingReader.HasValue; }
		}

		public override bool IsDefault {
			[MonoTODO]
			get { return validatingReader == null ? false : validatingReader.IsDefault; }
		}

		public override bool IsEmptyElement { 
			[MonoTODO]
			get { return validatingReader == null ? false : validatingReader.IsEmptyElement; }
		}

		public override string this [int i] { 
			[MonoTODO]
			get { return validatingReader [i]; }
		}

		public override string this [string name] { 
			[MonoTODO]
			get { return validatingReader == null ? String.Empty : validatingReader [name]; }
		}

		public override string this [string localName, string namespaceName] { 
			[MonoTODO]
			get { return validatingReader == null ? String.Empty : validatingReader [localName, namespaceName]; }
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
			[MonoTODO]
			get { return validatingReader == null ? String.Empty : validatingReader.LocalName; }
		}

		public override string Name {
			[MonoTODO]
			get { return validatingReader == null ? String.Empty : validatingReader.Name; }
		}

		[MonoTODO]
		public bool Namespaces {
			get {
				XmlTextReader xtr = sourceReader as XmlTextReader;
				if (xtr != null)
					return xtr.Namespaces;
				else
					throw new NotImplementedException ();
			}
			set {
				XmlTextReader xtr = sourceReader as XmlTextReader;
				if (xtr != null)
					xtr.Namespaces = value;
				else
					throw new NotImplementedException ();
			}
		}

		public override string NamespaceURI { 
			[MonoTODO]
			get { return validatingReader == null ? String.Empty : validatingReader.NamespaceURI; }
		}

		public override XmlNameTable NameTable { 
			[MonoTODO]
			get { return validatingReader == null ? null : validatingReader.NameTable; }
		}

		public override XmlNodeType NodeType { 
			[MonoTODO]
			get { return validatingReader == null ? XmlNodeType.None : validatingReader.NodeType; }
		}

		public override string Prefix { 
			[MonoTODO]
			get { return validatingReader == null ? String.Empty : validatingReader.Prefix; }
		}

		public override char QuoteChar { 
			[MonoTODO]
			get { return validatingReader == null ? '"' : validatingReader.QuoteChar; }
		}

		[MonoTODO ("confirm which reader should be returned.")]
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
			[MonoTODO]
			get { throw new NotImplementedException (); }
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
			[MonoTODO]
			get { return validatingReader == null ? String.Empty : validatingReader.Value; }
		}

		public override string XmlLang {
			[MonoTODO]
			get { return validatingReader == null ? String.Empty : validatingReader.XmlLang; }
		}

		public XmlResolver XmlResolver {
			[MonoTODO]
			set {
				resolver = value;
				DTDValidatingReader dvr = validatingReader as DTDValidatingReader;
				if (dvr != null)
					dvr.XmlResolver = value;
//				XmlSchemaValidatingReader xsvr = validatingReader as XmlSchemaValidatingReader;
//				if (xsvr != null)
//					xsvr.XmlResolver = value;
			}
		}

		public override XmlSpace XmlSpace {
			[MonoTODO]
			get { return validatingReader == null ? XmlSpace.None : validatingReader.XmlSpace; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void Close ()
		{
			validatingReader.Close ();
		}

		[MonoTODO]
		public override string GetAttribute (int i)
		{
			return validatingReader.GetAttribute (i);
		}

		[MonoTODO]
		public override string GetAttribute (string name)
		{
			return validatingReader.GetAttribute (name);
		}

		[MonoTODO]
		public override string GetAttribute (string localName, string namespaceName)
		{
			return validatingReader.GetAttribute (localName, namespaceName);
		}

		bool IXmlLineInfo.HasLineInfo ()
		{
			IXmlLineInfo info = validatingReader as IXmlLineInfo;
			return info != null ? info.HasLineInfo () : false;
		}

		[MonoTODO]
		public override string LookupNamespace (string prefix)
		{
			return validatingReader.LookupNamespace (prefix);
		}

		[MonoTODO]
		public override void MoveToAttribute (int i)
		{
			validatingReader.MoveToAttribute (i);
		}

		[MonoTODO]
		public override bool MoveToAttribute (string name)
		{
			return validatingReader.MoveToAttribute (name);
		}

		[MonoTODO]
		public override bool MoveToAttribute (string localName, string namespaceName)
		{
			return validatingReader.MoveToAttribute (localName, namespaceName);
		}

		[MonoTODO]
		public override bool MoveToElement ()
		{
			return validatingReader.MoveToElement ();
		}

		[MonoTODO]
		public override bool MoveToFirstAttribute ()
		{
			return validatingReader.MoveToFirstAttribute ();
		}

		[MonoTODO]
		public override bool MoveToNextAttribute ()
		{
			return validatingReader.MoveToNextAttribute ();
		}

		[MonoTODO]
		public override bool Read ()
		{
			if (ReadState == ReadState.Initial) {
				switch (ValidationType) {
				case ValidationType.Auto:
				case ValidationType.None:
					validatingReader = // new XmlSchemaValidatingReader (
						new DTDValidatingReader (sourceReader, this);
					break;
				case ValidationType.DTD:
					validatingReader = new DTDValidatingReader (sourceReader, this);
					break;
				case ValidationType.Schema:
//					validatingReader = new XmlSchemaValidatingReader (sourceReader, this);
//					break;
				case ValidationType.XDR:
					throw new NotImplementedException ();
				}
			}
			return validatingReader.Read ();
		}

		[MonoTODO]
		public override bool ReadAttributeValue ()
		{
			return validatingReader.ReadAttributeValue ();
		}

#if NET_1_0
		[MonoTODO]
		public override string ReadInnerXml ()
		{
			return validatingReader.ReadInnerXml ();
		}

		[MonoTODO]
		public override string ReadOuterXml ()
		{
			return validatingReader.ReadOuterXml ();
		}
#endif

		[MonoTODO]
		public override string ReadString ()
		{
			return validatingReader.ReadString ();
		}

		[MonoTODO]
		public object ReadTypedValue ()
		{
			throw new NotImplementedException ();
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
