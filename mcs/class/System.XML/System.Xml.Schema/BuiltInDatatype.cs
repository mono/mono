//
// System.Xml.Schema.XmlSchemaDatatype.cs
//
// Author:
//	Atsushi Enomoto
//
// (C)2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml.Schema
{
	public enum XsdWhitespaceFacet
	{
		Preserve,
		Replace,
		Collapse
	}

	// xs:string
	public class XsdString : XmlSchemaDatatype
	{
		internal XsdString ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return Normalize (s);
		}

		// Fundamental Facets
		public virtual bool Bounded {
			get { return false; }
		}
		public virtual bool Finite {
			get { return false; }
		}
		public virtual bool Numeric {
			get { return false; }
		}
		public virtual bool Ordered {
			get { return false; }
		}

		// Constraining Facets
		public bool HasLengthFacet;
		public bool HasMaxLengthFacet;
		public bool HasMinLengthFacet;
		public int Length;
		public int MaxLength;
		public int MinLength;
		public string Pattern;
		public ICollection Enumeration;
	}

	// xs:normalizedString
	public class XsdNormalizedString : XsdString
	{
		internal XsdNormalizedString ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Replace;
		}

		public override XmlTokenizedType TokenizedType {

			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:token
	public class XsdToken : XsdNormalizedString
	{
		internal XsdToken ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:language
	public class XsdLanguage : XsdToken
	{
		internal XsdLanguage ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs;NMTOKEN
	public class XsdNMToken : XsdToken
	{
		internal XsdNMToken ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.NMTOKEN; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:NMTOKENS
	public class XsdNMTokens : XsdNMToken
	{
		internal XsdNMTokens ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.NMTOKENS; }
		}

		public override Type ValueType {
			get { return typeof (string []); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:Name
	public class XsdName : XsdToken
	{
		internal XsdName ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:NCName
	public class XsdNCName : XsdName
	{
		internal XsdNCName ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.NCName; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:ID
	public class XsdID : XsdName
	{
		internal XsdID ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.ID; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:IDREF
	public class XsdIDRef : XsdName
	{
		internal XsdIDRef ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.IDREF; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:IDREFS
	public class XsdIDRefs : XsdName
	{
		internal XsdIDRefs ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.IDREFS; }
		}

		public override Type ValueType {
			get { return typeof (string []); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:ENTITY
	public class XsdEntity : XsdName
	{
		internal XsdEntity ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.ENTITY; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}


	// xs:ENTITIES
	public class XsdEntities : XsdName
	{
		internal XsdEntities ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.ENTITIES; }
		}

		public override Type ValueType {
			get { return typeof (string []); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:NOTATION
	public class XsdNotation : XmlSchemaDatatype
	{
		internal XsdNotation ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.NOTATION; }
		}

		public override Type ValueType {
			get { return typeof (string); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			throw new NotImplementedException ();
		}

		// Fundamental Facets
		public virtual bool Bounded {
			get { return false; }
		}
		public virtual bool Finite {
			get { return false; }
		}
		public virtual bool Numeric {
			get { return false; }
		}
		public virtual bool Ordered {
			get { return false; }
		}

		// Constraining Facets
		public bool HasLengthFacet;
		public bool HasMaxLengthFacet;
		public bool HasMinLengthFacet;
		public int Length;
		public int MaxLength;
		public int MinLength;
		public string Pattern;
		public ICollection Enumeration;
	}

	// xs:unsignedByte
	public class XsdUnsignedByte : XmlSchemaDatatype
	{
		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (byte); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToByte(Normalize (s));
		}
	}

}
