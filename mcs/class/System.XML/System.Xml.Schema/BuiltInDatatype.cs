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

	public enum XsdOrderedFacet
	{
		False,
		Partial,
		Total
	}

	public abstract class XsdAnySimpleType : XmlSchemaDatatype
	{
		// Fundamental Facets
		public abstract bool Bounded { get; }

		public abstract bool Finite { get; }

		public abstract bool Numeric { get; }

		public abstract XsdOrderedFacet Ordered { get; }

	}

	// xs:string
	public class XsdString : XsdAnySimpleType
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
		public override bool Bounded {
			get { return false; }
		}
		public override bool Finite {
			get { return false; }
		}
		public override bool Numeric {
			get { return false; }
		}
		public override XsdOrderedFacet Ordered {
			get { return XsdOrderedFacet.False; }
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
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
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

		readonly char [] whitespaceArray = new char [] {' '};
		public override object ParseValue (string value, XmlNameTable nt, XmlNamespaceManager nsmgr)
		{
			return this.Normalize (value).Split (whitespaceArray);
		}
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

		readonly char [] whitespaceArray = new char [] {' '};
		public override object ParseValue (string value, XmlNameTable nt, XmlNamespaceManager nsmgr)
		{
			return this.Normalize (value).Split (whitespaceArray);
		}
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

		readonly char [] whitespaceArray = new char [] {' '};
		public override object ParseValue (string value, XmlNameTable nt, XmlNamespaceManager nsmgr)
		{
			return this.Normalize (value).Split (whitespaceArray);
		}
	}

	// xs:NOTATION
	public class XsdNotation : XsdAnySimpleType
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
		public override bool Bounded {
			get { return false; }
		}
		public override bool Finite {
			get { return false; }
		}
		public override bool Numeric {
			get { return false; }
		}
		public override XsdOrderedFacet Ordered {
			get { return XsdOrderedFacet.False; }
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

	// xs:decimal
	public class XsdDecimal : XsdAnySimpleType
	{
		internal XsdDecimal ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToDecimal (this.Normalize (s));
		}

		// Fundamental Facets
		public override bool Bounded {
			get { return false; }
		}
		public override bool Finite {
			get { return false; }
		}
		public override bool Numeric {
			get { return true; }
		}
		public override XsdOrderedFacet Ordered {
			get { return XsdOrderedFacet.Total; }
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

	// xs:integer
	public class XsdInteger : XsdDecimal
	{
		// Here it may be bigger than int's (or long's) MaxValue.
		public override Type ValueType {
			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToDecimal (Normalize (s));
		}
	}

	// xs:Long
	public class XsdLong : XsdNonNegativeInteger
	{
		public override Type ValueType {
			get { return typeof (long); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToInt64 (Normalize (s));
		}
	}

	// xs:Int
	public class XsdInt : XsdLong
	{
		public override Type ValueType {
			get { return typeof (int); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToInt32 (Normalize (s));
		}
	}


	// xs:Short
	public class XsdShort : XsdInt
	{
		public override Type ValueType {
			get { return typeof (short); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToInt16 (Normalize (s));
		}
	}

	// xs:Byte
	public class XsdByte : XsdShort
	{
		public override Type ValueType {
			get { return typeof (byte); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToByte (Normalize (s));
		}
	}

	// xs:nonNegativeInteger
	[CLSCompliant (false)]
	public class XsdNonNegativeInteger : XsdInteger
	{
		public override Type ValueType {
			get { return typeof (decimal); }
		}

		[CLSCompliant (false)]
		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToDecimal (Normalize (s));
		}
	}

	// xs:unsignedLong
	[CLSCompliant (false)]
	public class XsdUnsignedLong : XsdNonNegativeInteger
	{
		public override Type ValueType {
			get { return typeof (ulong); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToUInt64 (Normalize (s));
		}
	}

	// xs:unsignedInt
	[CLSCompliant (false)]
	public class XsdUnsignedInt : XsdUnsignedLong
	{
		public override Type ValueType {
			get { return typeof (uint); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToUInt32 (Normalize (s));
		}
	}


	// xs:unsignedShort
	[CLSCompliant (false)]
	public class XsdUnsignedShort : XsdUnsignedInt
	{
		public override Type ValueType {
			get { return typeof (ushort); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToUInt16 (Normalize (s));
		}
	}

	// xs:unsignedByte
	[CLSCompliant (false)]
	public class XsdUnsignedByte : XsdUnsignedShort
	{
		public override Type ValueType {
			get { return typeof (byte); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToByte(Normalize (s));
		}
	}

	// xs:positiveInteger
	public class XsdPositiveInteger : XsdNonNegativeInteger
	{
		// It returns decimal, instead of int or long.
		// Maybe MS developers thought about big integer...
		public override Type ValueType {
			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToDecimal (Normalize (s));
		}
	}

	// xs:nonPositiveInteger
	public class XsdNonPositiveInteger : XsdInteger
	{
		public override Type ValueType {
			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToDecimal (Normalize (s));
		}
	}

	// xs:negativeInteger
	public class XsdNegativeInteger : XsdNonPositiveInteger
	{
		public override Type ValueType {

			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToDecimal (Normalize (s));
		}
	}

	// xs:float
	public class XsdFloat : XsdDecimal
	{
		public override Type ValueType {
			get { return typeof (float); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToSingle (Normalize (s));
		}
	}


	// xs:base64Binary
	public class XsdBase64Binary : XsdString
	{
		internal XsdBase64Binary ()
		{
		}

		public override Type ValueType {
			get { return typeof (byte[]); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return Convert.FromBase64String (s);
		}
	}

	// xs:boolean
	public class XsdBoolean : XsdAnySimpleType
	{
		internal XsdBoolean ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (bool); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToBoolean (this.Normalize (s));
		}

		// Fundamental Facets
		public override bool Bounded {
			get { return false; }
		}
		public override bool Finite {
			get { return true; }
		}
		public override bool Numeric {
			get { return false; }
		}
		public override XsdOrderedFacet Ordered {
			get { return XsdOrderedFacet.Total; }
		}
	}
}
