//
// XmlSchemaValidator.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2004 Novell Inc,
//

#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

using QName = System.Xml.XmlQualifiedName;
using Form = System.Xml.Schema.XmlSchemaForm;
using Use = System.Xml.Schema.XmlSchemaUse;
using ContentType = System.Xml.Schema.XmlSchemaContentType;
using Validity = System.Xml.Schema.XmlSchemaValidity;
using ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags;
using SOMList = System.Xml.Schema.XmlSchemaObjectCollection;
using SOMObject = System.Xml.Schema.XmlSchemaObject;
using Element = System.Xml.Schema.XmlSchemaElement;
using Attr = System.Xml.Schema.XmlSchemaAttribute;
using AttrGroup = System.Xml.Schema.XmlSchemaAttributeGroup;
using AttrGroupRef = System.Xml.Schema.XmlSchemaAttributeGroupRef;
using SimpleType = System.Xml.Schema.XmlSchemaSimpleType;
using ComplexType = System.Xml.Schema.XmlSchemaComplexType;
using SimpleModel = System.Xml.Schema.XmlSchemaSimpleContent;
using SimpleExt = System.Xml.Schema.XmlSchemaSimpleContentExtension;
using SimpleRst = System.Xml.Schema.XmlSchemaSimpleContentRestriction;
using ComplexModel = System.Xml.Schema.XmlSchemaComplexContent;
using ComplexExt = System.Xml.Schema.XmlSchemaComplexContentExtension;
using ComplexRst = System.Xml.Schema.XmlSchemaComplexContentRestriction;
using SimpleTypeRst = System.Xml.Schema.XmlSchemaSimpleTypeRestriction;
using SimpleList = System.Xml.Schema.XmlSchemaSimpleTypeList;
using SimpleUnion = System.Xml.Schema.XmlSchemaSimpleTypeUnion;
using SchemaFacet = System.Xml.Schema.XmlSchemaFacet;
using LengthFacet = System.Xml.Schema.XmlSchemaLengthFacet;
using MinLengthFacet = System.Xml.Schema.XmlSchemaMinLengthFacet;
using Particle = System.Xml.Schema.XmlSchemaParticle;
using Sequence = System.Xml.Schema.XmlSchemaSequence;
using Choice = System.Xml.Schema.XmlSchemaChoice;


namespace System.Xml.Schema
{
	public class XmlSchemaValidator
	{
		public XmlSchemaValidator (
			XmlNameTable nameTable,
			XmlSchemaSet schemas,
			IXmlNamespaceResolver nsResolver,
			ValidationFlags options)
		{
			this.nameTable = nameTable;
			this.schemas = schemas;
			this.nsResolver = nsResolver;
			this.options = options;
		}

		#region Fields

		// XmlReader/XPathNavigator themselves
		object nominalEventSender;
		IXmlLineInfo lineInfo;
		IXmlNamespaceResolver nsResolver;

		// These fields will be from XmlReaderSettings or 
		// XPathNavigator.CheckValidity(). BTW, I think we could
		// implement XPathNavigator.CheckValidity() with
		// XsdValidatingReader.
		XmlNameTable nameTable;
		XmlSchemaSet schemas;
		XmlResolver xmlResolver;

		// "partialValidationType". but not sure how it will be used.
		SOMObject startType;

		// Below are maybe from XmlReaderSettings, but XPathNavigator
		// does not have it.
		ValidationFlags options;

		SOMObject currentType;

		#endregion

		#region Properties

		// Settable Properties

		// IMHO It should just be an event that fires another event.
		public event ValidationEventHandler ValidationEventHandler;

		public object ValidationEventSender {
			get { return nominalEventSender; }
			set { nominalEventSender = value; }
		}

		// (kinda) Construction Properties

		public IXmlLineInfo LineInfoProvider {
			get { return lineInfo; }
			set { lineInfo = value; }
		}

		public XmlResolver XmlResolver {
			set { xmlResolver = value; }
		}

		[MonoTODO]
		public string SourceUri {
			get { throw new NotImplementedException (); }
		}
		#endregion

		#region Methods

		// State Monitor

		[MonoTODO]
		public XmlSchemaAttribute [] GetExpectedAttributes ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlSchemaParticle [] GetExpectedParticles ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetUnspecifiedDefaultAttributes (ArrayList list)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object FindID (string name)
		{
			throw new NotImplementedException ();
		}

		// State Controller

		public void Init ()
		{
			Init (null);
		}

		public void Init (SOMObject startType)
		{
			this.startType = startType;
		}

		// It must be called at the end of the validation (to check
		// identity constraints etc.).
		[MonoTODO]
		public void EndValidation ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SkipToEndElement (XmlSchemaInfo info)
		{
			throw new NotImplementedException ();
		}

		// I guess this weird XmlValueGetter is for such case that
		// value might not be required (and thus it improves 
		// performance in some cases. Doh.

		// AttDeriv
		[MonoTODO]
		public object ValidateAttribute (
			string localName,
			string ns,
			XmlValueGetter attributeValue,
			XmlSchemaInfo info)
		{
			throw new NotImplementedException ();
		}

		// StartTagOpenDeriv
		[MonoTODO]
		public void ValidateElement (
			string localName,
			string ns,
			XmlSchemaInfo info) // How is it used?
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object ValidateEndElement (XmlSchemaInfo scheaInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object ValidateEndElement (XmlSchemaInfo scheaInfo,
			object var)
		{
			throw new NotImplementedException ();
		}

		// StartTagCloseDeriv
		[MonoTODO]
		public void ValidateEndOfAttributes ()
		{
			throw new NotImplementedException ();
		}

		// TextDeriv ... without text. Maybe typed check is done by
		// ValidateAtomicValue().
		[MonoTODO]
		public void ValidateText (XmlValueGetter getter)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ValidateWhitespace (XmlValueGetter getter)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

#endif
