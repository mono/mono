//
// Mono.Xml.Schema.XsdValidatingReader.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using System.Xml.Schema;


namespace Mono.Xml
{
	public class XsdValidatingReader : XmlReader, IXmlLineInfo, IHasXmlSchemaInfo //, IHasXmlParserContext
	{
		XmlReader reader;
		XmlValidatingReader xvReader;
		bool laxElementValidation = true;
		bool reportNoValidationError;
		XmlSchemaCollection schemas = new XmlSchemaCollection ();
		bool namespaces = true;

		XsdValidationState currentValidationState;
//		XsdAttributeValidationStateCollection attributeValidationStates;
		object elementXsiType;
		StringBuilder storedCharacters = new StringBuilder ();

		// [int Depth] -> XsdAutomata.
		// Some of them might be missing (See the spec section 5.3).
		Hashtable automataTable = new Hashtable ();

		StringCollection defaultAttributes = new StringCollection ();
		int currentDefaultAttribute;

		// Property Cache.
		int attributeCount;

#region .ctor
		public XsdValidatingReader (XmlReader reader)
			: this (reader, null)
		{
		}
		
		public XsdValidatingReader (XmlReader reader, XmlReader validatingReader)
		{
			this.reader = reader;
			xvReader = validatingReader as XmlValidatingReader;
			if (xvReader != null) {
				if (xvReader.ValidationType == ValidationType.None)
					reportNoValidationError = true;
			}
		}
#endregion

// Non-overrides

		public bool Namespaces {
			get { return namespaces; }
			set { namespaces = value; }
		}

		public XmlReader Reader {
			get { return reader; }
		}

		// This should be changed before the first Read() call.
		public XmlSchemaCollection Schemas {
			get { return schemas; }
		}

		public object SchemaType {
			get {
				if (ReadState != ReadState.Interactive)
					return null;

				switch (NodeType) {
				case XmlNodeType.Element:
					if (elementXsiType != null)
						return elementXsiType;
					else if (currentValidationState != null)
						return currentValidationState.Element.ElementType;
					else
						return null;
				case XmlNodeType.Attribute:
					throw new NotImplementedException ();
				default:
					return null;
				}
			}
		}

		// This property is never used in Mono.
		public ValidationType ValidationType {
			get {
				if (reportNoValidationError)
					return ValidationType.None;
				else
					return ValidationType.Schema;
			}
		}

		public XmlResolver XmlResolver {
			set { throw new NotImplementedException (); }
		}

		// TODO: provide XmlNamespaceManager to ParseValue() if possible
		public object ReadTypedValue ()
		{
			switch (NodeType) {
			case XmlNodeType.Element:
				XmlSchemaDatatype xsDatatype = currentValidationState.Datatype;
				if (xsDatatype != null)
					return xsDatatype.ParseValue (ReadString (), NameTable, null);
				else
					return null;
			case XmlNodeType.Attribute:
				throw new NotImplementedException ();
//				xsDatatype = attributeValidationStates [LocalName, NamespaceURI].Datatype;
//				if (xsDatatype != null)
//					return xsDatatype.ParseValue (Value, NameTable, null);
//				else
//					return null;
			default:
				return null;
			}
		}

		public ValidationEventHandler ValidationEventHandler;

// Overrided Properties

		public override int AttributeCount {
			get {
				if (NodeType == XmlNodeType.Element)
					return attributeCount;
				else if (IsDefault)
					return 0;
				else
					return reader.AttributeCount;
			}
		}

		public override string BaseURI {
			get { return reader.BaseURI; }
		}

		// If this class is used to implement XmlValidatingReader,
		// it should be left to DTDValidatingReader. In other cases,
		// it depends on the reader's ability.
		public override bool CanResolveEntity {
			get { return reader.CanResolveEntity; }
		}

		public override int Depth {
			get { return reader.Depth; }
		}

		public override bool EOF {
			get { return reader.EOF; }
		}

		public override bool HasValue {
			get { throw new NotImplementedException (); }
		}

		public override bool IsDefault {
			// TODO: handle default node
			get { return false; }
		}

		public override bool IsEmptyElement {
			// TODO: consider default attributes
			get { return reader.IsEmptyElement; }
		}

		public override string this [int i] {
			get { return GetAttribute (i); }
		}

		public override string this [string name] {
			get { return GetAttribute (name); }
		}

		public override string this [string localName, string ns] {
			get { return GetAttribute (localName, ns); }
		}

		int IXmlLineInfo.LineNumber {
			get { throw new NotImplementedException (); }
		}

		int IXmlLineInfo.LinePosition {
			get { throw new NotImplementedException (); }
		}

		public override string LocalName {
			// TODO: handle default node
			get { return reader.LocalName; }
		}

		public override string Name {
			// TODO: handle default node
			get { return reader.Name; }
		}

		public override string NamespaceURI {
			// TODO: handle default node
			get { return reader.NamespaceURI; }
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}

		public override XmlNodeType NodeType {
			// TODO: handle default node
			get { return reader.NodeType; }
		}

		public override string Prefix {
			// TODO: handle default node
			get { return reader.Prefix; }
		}

		public override char QuoteChar {
			get { return reader.QuoteChar; }
		}

		public override ReadState ReadState {
			get { return reader.ReadState; }
		}

		public override string Value {
			// TODO: handle default node
			get { return reader.Value; }
		}

		public override string XmlLang {
			get { throw new NotImplementedException (); }
		}

		public override XmlSpace XmlSpace {
			get { throw new NotImplementedException (); }
		}

		private void HandleError (string error)
		{
			if (reportNoValidationError)
				return;

			ValidationEventArgs e = new ValidationEventArgs (
				new XmlSchemaException (error, null), error, XmlSeverityType.Error);

			if (this.ValidationEventHandler != null)
				this.ValidationEventHandler (this, e);
			else
#if NON_MONO_ENV
				this.xvReader.OnValidationEvent (this, e);
#else
				throw e.Exception;
#endif
		}

		private void ValidateStartElementParticle ()
		{
			if (schemas.Count == 0)	// No validation is performed.
				return;

			// Creating element automata, if current does not exist.
			if (currentValidationState == null) {
				XmlSchemaElement root = null;
				foreach (XmlSchema target in schemas) {
					XmlSchema matches = target.Schemas [reader.NamespaceURI];
					if (matches != null) {
						root = target.Elements [new XmlQualifiedName (reader.LocalName, reader.NamespaceURI)] as XmlSchemaElement;
						if (root != null) {
							XsdValidationStateFactory factory = new XsdValidationStateFactory ();
							currentValidationState = factory.Create (root);
						}
						else
							HandleError ("Invalid start element. Element declaration for " + reader.LocalName + " is missing.");
						break;
					}
				}
				if (root == null && reader.NamespaceURI != String.Empty)
					HandleError ("Invalid start element. Element declaration for " + reader.LocalName + " is missing.");
			}

			if (currentValidationState == null)
				return;		// no validation.

			if (!currentValidationState.EvaluateStartElement (reader.LocalName, reader.NamespaceURI))
				HandleError ("Invalid start element: " + reader.LocalName);

			automataTable [reader.Depth] = currentValidationState;
			XmlSchemaElement el = currentValidationState.Element;
			XmlSchemaComplexType ctype = el.ElementType as XmlSchemaComplexType;
			if (ctype != null && ctype.ContentTypeParticle != null) {
				currentValidationState = currentValidationState.Factory.Create (ctype.ContentTypeParticle);
			}

			// TODO: Attribute validation
		}

		private void ValidateEndElementParticle ()
		{
			if (currentValidationState != null) {
				if (!currentValidationState.EvaluateEndElement ()) {
					HandleError ("Invalid end element: " + reader.Name);
				}
			}
			currentValidationState = automataTable [reader.Depth] as XsdValidationState;
			automataTable [reader.Depth + 1] = null;
		}

		private void ValidateCharacters ()
		{
			// TODO: value context validation here.
		}

// Overrided Methods

		public override void Close ()
		{
			reader.Close ();
		}

		// MonoTODO
		public override string GetAttribute (int i)
		{
			if (reader.AttributeCount > i)
				reader.GetAttribute (i);
//			else if (defaultAttributes.Count)
			throw new NotImplementedException ();
		}

		// MonoTODO
		public override string GetAttribute (string name)
		{
			return reader.GetAttribute (name);
		}

		// MonoTODO
		public override string GetAttribute (string localName, string ns)
		{
			return reader.GetAttribute (localName, ns);
		}

		// When it is default attribute, does it works?
		bool IXmlLineInfo.HasLineInfo ()
		{
			throw new NotImplementedException ();
		}

		// MonoTODO
		public override string LookupNamespace (string prefix)
		{
			return reader.LookupNamespace (prefix);
		}

		// MonoTODO
		public override void MoveToAttribute (int i)
		{
			reader.MoveToAttribute (i);
		}

		// MonoTODO
		public override bool MoveToAttribute (string name)
		{
			return reader.MoveToAttribute (name);
		}

		// MonoTODO
		public override bool MoveToAttribute (string localName, string ns)
		{
			return reader.MoveToAttribute (localName);
		}

		public override bool MoveToElement ()
		{
			// TODO: handle default node
			return reader.MoveToElement ();
		}

		public override bool MoveToFirstAttribute ()
		{
			// TODO: handle default node
			return reader.MoveToFirstAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			// TODO: handle default node
			return reader.MoveToNextAttribute ();
		}

		public override bool Read ()
		{
			bool result = reader.Read ();

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				ValidateStartElementParticle ();
				// TODO: validate xsi:nil, create xsi:type, and so on.
				// TODO: validate attributes

				if (reader.IsEmptyElement)
					goto case XmlNodeType.EndElement;
				break;

			case XmlNodeType.EndElement:
				ValidateEndElementParticle ();
				// TODO: validate content data type.
				break;

			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Text:
				ValidateCharacters ();
				break;
			}

			return result;
		}

		public override bool ReadAttributeValue ()
		{
			// TODO: handle default node
			return reader.ReadAttributeValue ();
		}

#if NET_1_0
		public override string ReadInnerXml ()
		{
			// MS.NET 1.0 has a serious bug here. It skips validation.
			return reader.ReadInnerXml ();
		}

		public override string ReadOuterXml ()
		{
			// MS.NET 1.0 has a serious bug here. It skips validation.
			return reader.ReadOuterXml ();
		}
#endif

		// XmlReader.ReadString() should call derived this.Read().
		public override string ReadString ()
		{
#if NET_1_0
			return reader.ReadString ();
#else
			return base.ReadString ();
#endif
		}

		// This class itself does not have this feature.
		public override void ResolveEntity ()
		{
			reader.ResolveEntity ();
		}
	}

}
