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
using System.Globalization;

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

	public enum XsdOrdering 
	{
		LessThan = -1,
		Equal = 0,
		GreaterThan = 1,
		Indeterminate = 2
	}
	
	public class XsdAnySimpleType : XmlSchemaDatatype
	{
		static XsdAnySimpleType instance;
		static XsdAnySimpleType ()
		{
			instance = new XsdAnySimpleType ();
		}

		public static XsdAnySimpleType Instance {
			get { return instance; }
		}

		readonly char [] whitespaceArray = new char [] {' '};

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

		public virtual XsdOrderedFacet Ordered { 
			get { return XsdOrderedFacet.False; }
		}

		public override Type ValueType {
			get { return typeof (object); }
		}

		public override XmlTokenizedType TokenizedType {
			get {
				return XmlTokenizedType.None;
			}
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return Normalize (s);
		}

		internal string [] ParseListValue (string s, XmlNameTable nameTable)
		{
			return this.Normalize (s, XsdWhitespaceFacet.Collapse).Split (whitespaceArray);
		}
		
	

			// Can you even use XsdAnySimpleType in a schema?
			// -> Yes. See E1-22 of http://www.w3.org/2001/05/xmlschema-errata#Errata1 (somewhat paranoid ;-)
		
		internal bool AllowsFacet(XmlSchemaFacet xsf) {
			return (AllowedFacets & xsf.ThisFacet)!=0;
		}



		internal virtual XsdOrdering Compare(object x, object y) {
			return XsdOrdering.Indeterminate;
			}
		
		// anySimpleType allows any facet
		internal virtual XmlSchemaFacet.Facet AllowedFacets {
			get { return XmlSchemaFacet.AllFacets ;}
		}

		/* Matches facets allowed on boolean type
		 */
		protected const XmlSchemaFacet.Facet booleanAllowedFacets = 
					 XmlSchemaFacet.Facet.pattern | XmlSchemaFacet.Facet.whiteSpace;

		/* Matches facets allowed on decimal type. 
		 */
		protected const XmlSchemaFacet.Facet decimalAllowedFacets = 
							XmlSchemaFacet.Facet.pattern | XmlSchemaFacet.Facet.enumeration | 
							XmlSchemaFacet.Facet.whiteSpace | XmlSchemaFacet.Facet.maxInclusive | 
							XmlSchemaFacet.Facet.minInclusive | XmlSchemaFacet.Facet.maxExclusive | 
							XmlSchemaFacet.Facet.minExclusive | XmlSchemaFacet.Facet.fractionDigits | 
							XmlSchemaFacet.Facet.totalDigits ;

		/* Matches facets allowed on float, double, duration, dateTime, time, date,
		 * gYearMonth, gYear, gMonthDay, gMonth, and gDay types
		 */

		protected const XmlSchemaFacet.Facet durationAllowedFacets = 
							XmlSchemaFacet.Facet.pattern | XmlSchemaFacet.Facet.enumeration | 
							XmlSchemaFacet.Facet.whiteSpace | XmlSchemaFacet.Facet.maxInclusive |
							XmlSchemaFacet.Facet.minInclusive | XmlSchemaFacet.Facet.maxExclusive |
							XmlSchemaFacet.Facet.minExclusive ;

		/* Matches facet allowed on string, hexBinary, base64Binary,
		 * anyURI, QName and NOTATION types 
		 *
		 * Also used on list types
		 */

		protected const XmlSchemaFacet.Facet stringAllowedFacets = 
						 XmlSchemaFacet.Facet.length | XmlSchemaFacet.Facet.minLength |
						 XmlSchemaFacet.Facet.maxLength | XmlSchemaFacet.Facet.pattern | 
						 XmlSchemaFacet.Facet.enumeration | XmlSchemaFacet.Facet.whiteSpace; 
	}

	// xs:string
	public class XsdString : XsdAnySimpleType
	{
		internal XsdString ()
		{
		}

		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return stringAllowedFacets; } 
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (string); }
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

		/* Freeze their use now.
		// Constraining Facets
		public bool HasLengthFacet;
		public bool HasMaxLengthFacet;
		public bool HasMinLengthFacet;
		public int Length;
		public int MaxLength;
		public int MinLength;
		public string Pattern;
		public ICollection Enumeration;
		*/
	
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

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			if (!XmlChar.IsNmToken (s))
				throw new ArgumentException ("'" + s + "' is an invalid NMTOKEN.");
			return s;
		}

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

		public override object ParseValue (string value, XmlNameTable nt, XmlNamespaceManager nsmgr)
		{
			return ParseListValue (value, nt);
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

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			if (!XmlChar.IsName (s))
				throw new ArgumentException ("'" + s + "' is an invalid name.");
			return s;
		}
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

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			if (!XmlChar.IsNCName (s))
				throw new ArgumentException ("'" + s + "' is an invalid NCName.");
			return s;
		}

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

		public override object ParseValue (string value, XmlNameTable nt, XmlNamespaceManager nsmgr)
		{
			return ParseListValue (value, nt);
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

		public override object ParseValue (string value, XmlNameTable nt, XmlNamespaceManager nsmgr)
		{
			return ParseListValue (value, nt);
		}
	}

	// xs:NOTATION
	public class XsdNotation : XsdAnySimpleType
	{
		internal XsdNotation ()
		{
		}

		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return stringAllowedFacets; } 
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

	// xs:decimal
	public class XsdDecimal : XsdAnySimpleType
	{
		internal XsdDecimal ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}

		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return decimalAllowedFacets; } 
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.None; }
		}

		public override Type ValueType {
			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToDecimal (this.Normalize (s));
		}

	 
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is Decimal) && (y is Decimal)) {
				int ret = Decimal.Compare((Decimal)x, (Decimal)y);
				if (ret < 0) { 
					return XsdOrdering.LessThan;
				}
				else if (ret > 0) {
					return XsdOrdering.GreaterThan;
				}
				else {
					return XsdOrdering.Equal;
				}
			}
			return XsdOrdering.Indeterminate;
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
		public XsdInteger ()
		{
		}

		// Here it may be bigger than int's (or long's) MaxValue.
		public override Type ValueType {
			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			decimal d = XmlConvert.ToDecimal (Normalize (s));
			if (Decimal.Floor (d) != d)
				throw new FormatException ("Integer contains point number.");
			return d;
		}
	 
		
	
	}

	// xs:Long
	public class XsdLong : XsdInteger
	{
		public override Type ValueType {
			get { return typeof (long); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToInt64 (Normalize (s));
		}
		
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is long) && (y is long)) {
				if ((long)x==(long)y) {
					return XsdOrdering.Equal;
				}
				else if ((long)x<(long)y) {
					return XsdOrdering.LessThan;
				}
				else {
					return XsdOrdering.GreaterThan;
				}
			}
			return XsdOrdering.Indeterminate;
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
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is int) && (y is int)) {
				if ((int)x==(int)y) {
					return XsdOrdering.Equal;
				}
				else if ((int)x<(int)y) {
					return XsdOrdering.LessThan;
				}
				else {
					return XsdOrdering.GreaterThan;
				}
			}
			return XsdOrdering.Indeterminate;
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
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is short) && (y is short)) {
				if ((short)x==(short)y) {
					return XsdOrdering.Equal;
				}
				else if ((short)x<(short)y) {
					return XsdOrdering.LessThan;
				}
				else {
					return XsdOrdering.GreaterThan;
				}
			}
			return XsdOrdering.Indeterminate;
		}
	}

	// xs:Byte
	public class XsdByte : XsdShort
	{
		public override Type ValueType {
			get { return typeof (sbyte); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToSByte (Normalize (s));
		}
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is sbyte) && (y is sbyte)) {
				if ((sbyte)x==(sbyte)y) {
					return XsdOrdering.Equal;
				}
				else if ((sbyte)x<(sbyte)y) {
					return XsdOrdering.LessThan;
				}
				else {
					return XsdOrdering.GreaterThan;
				}
			}
			return XsdOrdering.Indeterminate;
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
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is ulong) && (y is ulong)) {
				if ((ulong)x==(ulong)y) {
					return XsdOrdering.Equal;
				}
				else if ((ulong)x<(ulong)y) {
					return XsdOrdering.LessThan;
				}
				else {
					return XsdOrdering.GreaterThan;
				}
			}
			return XsdOrdering.Indeterminate;
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
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is uint) && (y is uint)) {
				if ((uint)x==(uint)y) {
					return XsdOrdering.Equal;
				}
				else if ((uint)x<(uint)y) {
					return XsdOrdering.LessThan;
				}
				else {
					return XsdOrdering.GreaterThan;
				}
			}
			return XsdOrdering.Indeterminate;
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
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is ushort) && (y is ushort)) {
				if ((ushort)x==(ushort)y) {
					return XsdOrdering.Equal;
				}
				else if ((ushort)x<(ushort)y) {
					return XsdOrdering.LessThan;
				}
				else {
					return XsdOrdering.GreaterThan;
				}
			}
			return XsdOrdering.Indeterminate;
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
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is byte) && (y is byte)) {
				if ((byte)x==(byte)y) {
					return XsdOrdering.Equal;
				}
				else if ((byte)x<(byte)y) {
					return XsdOrdering.LessThan;
				}
				else {
					return XsdOrdering.GreaterThan;
				}
			}
			return XsdOrdering.Indeterminate;
		}
	}


	// xs:positiveInteger
	[CLSCompliant (false)]
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
	public class XsdFloat : XsdAnySimpleType
	{
		internal XsdFloat ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
    
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
		// Fundamental Facets
		public override bool Bounded {
			get { return true; }
		}
		public override bool Finite {
			get { return true; }
		}
		public override bool Numeric {
			get { return true; }
		}
		public override XsdOrderedFacet Ordered {
			get { return XsdOrderedFacet.Total; }
		}

		public override Type ValueType {
			get { return typeof (float); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToSingle (Normalize (s));
		}
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is float) && (y is float)) {
				if ((float)x==(float)y) {
					return XsdOrdering.Equal;
				}
				else if ((float)x<(float)y) {
					return XsdOrdering.LessThan;
				}
				else {
					return XsdOrdering.GreaterThan;
				}
		}
			return XsdOrdering.Indeterminate;
		}
		
	}

	// xs:double
	public class XsdDouble : XsdAnySimpleType
	{
		internal XsdDouble ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
    
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
		// Fundamental Facets
		public override bool Bounded {
			get { return true; }
		}
		public override bool Finite {
			get { return true; }
		}
		public override bool Numeric {
			get { return true; }
		}
		public override XsdOrderedFacet Ordered {
			get { return XsdOrderedFacet.Total; }
		}

		public override Type ValueType {
			get { return typeof (double); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToDouble (Normalize (s));
		}
		internal override XsdOrdering Compare(object x, object y) {
			if ((x is double) && (y is double)) {
				if ((double)x==(double)y) {
					return XsdOrdering.Equal;
				}
				else if ((double)x<(double)y) {
					return XsdOrdering.LessThan;
		}
				else {
					return XsdOrdering.GreaterThan;
				}
			}
			return XsdOrdering.Indeterminate;
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
			return Convert.FromBase64String (Normalize (s));
		}
	}

	// xs:hexBinary
	public class XsdHexBinary : XsdAnySimpleType
	{
		internal XsdHexBinary ()
		{
      this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}

		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return stringAllowedFacets; } 
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.None; }
		}

		public override Type ValueType {
			get { return typeof (byte []); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.FromBinHexString (Normalize (s));
		}
		

		// Fundamental Facets ... no need to override
	}

	// xs:QName
	public class XsdQName : XsdName
	{
		internal XsdQName ()
		{
		}

		// Fundamental facets are the same as anySimpleType.

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.QName; }
		}

		public override Type ValueType {
			get { return typeof (XmlQualifiedName); }
		}

		// ParseValue () method is as same as that of xs:string
		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			if (nameTable == null)
				throw new ArgumentNullException ("name table");
			if (nsmgr == null)
				throw new ArgumentNullException ("namespace manager");
			int colonAt = s.IndexOf (':');
			string localName = 
				nameTable.Add (colonAt < 0 ? s : s.Substring (colonAt + 1));
			return new XmlQualifiedName (localName, nsmgr.LookupNamespace (
				colonAt < 0 ? "" : s.Substring (0, colonAt - 1)));
		}
		
	}

	// xs:boolean
	public class XsdBoolean : XsdAnySimpleType
	{
		internal XsdBoolean ()
		{
      this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}

		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return booleanAllowedFacets; } 
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

	// xs:anyURI
	public class XsdAnyURI : XsdString
	{
		public override XmlTokenizedType TokenizedType {

			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (Uri); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return new Uri (Normalize (s));
		}
	}
	
	// xs:duration
	public class XsdDuration : XsdAnySimpleType
	{
		internal XsdDuration ()
		{
      this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}

		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (TimeSpan); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToTimeSpan (Normalize (s));
		}

		internal override XsdOrdering Compare(object x, object y) {
			/* FIXME: This is really simple so far 
			 *
			 * In fact in order to do this correctly in XmlSchema, we cannot use TimeSpan as the base type
			 * Though it turns out that MS .NET is a little broken in this regard too. Not doing comparisons 
			 * correctly.
			 */
			if ((x is TimeSpan) && (y is TimeSpan)) {
				int ret = TimeSpan.Compare((TimeSpan)x, (TimeSpan)y);
				if (ret < 0) { 
					return XsdOrdering.LessThan;
				}
				else if (ret > 0) {
					return XsdOrdering.GreaterThan;
				}
				else {
					return XsdOrdering.Equal;
				}
			}
			return XsdOrdering.Indeterminate;
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
			get { return XsdOrderedFacet.Partial; }
	 
		}
	}

	// xs:dateTime
	public class XsdDateTime : XsdAnySimpleType
	{
		internal XsdDateTime ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}

		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return XmlConvert.ToDateTime (Normalize (s));
		}

		internal override XsdOrdering Compare(object x, object y) {
			/* Really simple so far */
			if ((x is DateTime) && (y is DateTime)) {
				int ret = DateTime.Compare((DateTime)x, (DateTime)y);
				if (ret < 0) { 
					return XsdOrdering.LessThan;
				}
				else if (ret > 0) {
					return XsdOrdering.GreaterThan;
				}
				else {
					return XsdOrdering.Equal;
				}
			}
			return XsdOrdering.Indeterminate;
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
			get { return XsdOrderedFacet.Partial; }
		}
		
	}

	// xs:date
	public class XsdDate : XsdAnySimpleType
	{
		internal XsdDate ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
    
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return DateTime.ParseExact (Normalize (s), "yyyy-MM-dd", null);
		}

		internal override XsdOrdering Compare(object x, object y) {
			/* Really simple so far */
			if ((x is DateTime) && (y is DateTime)) {
				int ret = DateTime.Compare((DateTime)x, (DateTime)y);
				if (ret < 0) { 
					return XsdOrdering.LessThan;
				}
				else if (ret > 0) {
					return XsdOrdering.GreaterThan;
				}
				else {
					return XsdOrdering.Equal;
				}
			}
			return XsdOrdering.Indeterminate;
		}
		// Fundamental Facets ... no need to override except for Ordered.
		public override XsdOrderedFacet Ordered {
			get { return XsdOrderedFacet.Partial; }
		}
		
	}

	// xs:time
	public class XsdTime : XsdAnySimpleType
	{
		static string [] timeFormats = new string [] {
			  // copied from XmlConvert.
			  "HH:mm:ss",
			  "HH:mm:ss.f",
			  "HH:mm:ss.ff",
			  "HH:mm:ss.fff",
			  "HH:mm:ss.ffff",
			  "HH:mm:ss.fffff",
			  "HH:mm:ss.ffffff",
			  "HH:mm:ss.fffffff",
			  "HH:mm:sszzz",
			  "HH:mm:ss.fzzz",
			  "HH:mm:ss.ffzzz",
			  "HH:mm:ss.fffzzz",
			  "HH:mm:ss.ffffzzz",
			  "HH:mm:ss.fffffzzz",
			  "HH:mm:ss.ffffffzzz",
			  "HH:mm:ss.fffffffzzz",
			  "HH:mm:ssZ",
			  "HH:mm:ss.fZ",
			  "HH:mm:ss.ffZ",
			  "HH:mm:ss.fffZ",
			  "HH:mm:ss.ffffZ",
			  "HH:mm:ss.fffffZ",
			  "HH:mm:ss.ffffffZ",
			  "HH:mm:ss.fffffffZ"};

		internal XsdTime ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}

		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return DateTime.ParseExact (Normalize (s), timeFormats, null, DateTimeStyles.None);
		}

		internal override XsdOrdering Compare(object x, object y) {
			/* Really simple so far */
			if ((x is DateTime) && (y is DateTime)) {
				int ret = DateTime.Compare((DateTime)x, (DateTime)y);
				if (ret < 0) { 
					return XsdOrdering.LessThan;
				}
				else if (ret > 0) {
					return XsdOrdering.GreaterThan;
				}
				else {
					return XsdOrdering.Equal;
				}
			}
			return XsdOrdering.Indeterminate;
		}
		// Fundamental Facets ... no need to override except for Ordered.
		public override XsdOrderedFacet Ordered {
			get { return XsdOrderedFacet.Partial; }
		}
		
	}

	// xs:gYearMonth
	public class XsdGYearMonth : XsdAnySimpleType
	{
		internal XsdGYearMonth ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
    
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return DateTime.ParseExact (Normalize (s), "yyyy-MM", null);
		}
		
		internal override XsdOrdering Compare(object x, object y) {
			/* Really simple so far */
			if ((x is DateTime) && (y is DateTime)) {
				int ret = DateTime.Compare((DateTime)x, (DateTime)y);
				if (ret < 0) { 
					return XsdOrdering.LessThan;
				}
				else if (ret > 0) {
					return XsdOrdering.GreaterThan;
				}
				else {
					return XsdOrdering.Equal;
				}
			}
			return XsdOrdering.Indeterminate;
		}
	}

	// xs:gMonthDay
	public class XsdGMonthDay : XsdAnySimpleType
	{
		internal XsdGMonthDay ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
    
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return DateTime.ParseExact (Normalize (s), "--MM-dd", null);
		}
		
		internal override XsdOrdering Compare(object x, object y) {
			/* Really simple so far */
			if ((x is DateTime) && (y is DateTime)) {
				int ret = DateTime.Compare((DateTime)x, (DateTime)y);
				if (ret < 0) { 
					return XsdOrdering.LessThan;
				}
				else if (ret > 0) {
					return XsdOrdering.GreaterThan;
				}
				else {
					return XsdOrdering.Equal;
				}
			}
			return XsdOrdering.Indeterminate;
		}
	}

	// xs:gYear
	public class XsdGYear : XsdAnySimpleType
	{
		internal XsdGYear ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
		
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
    public override Type ValueType {
			get { return typeof (DateTime); }
		}

		// LAMESPEC: XML Schema Datatypes allows leading '-' to identify B.C. years,
		// but CLR DateTime does not allow such expression.
		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return DateTime.ParseExact (Normalize(s), "yyyy", null);
		}
		
		internal override XsdOrdering Compare(object x, object y) {
			/* Really simple so far */
			if ((x is DateTime) && (y is DateTime)) {
				int ret = DateTime.Compare((DateTime)x, (DateTime)y);
				if (ret < 0) { 
					return XsdOrdering.LessThan;
				}
				else if (ret > 0) {
					return XsdOrdering.GreaterThan;
				}
				else {
					return XsdOrdering.Equal;
				}
			}
			return XsdOrdering.Indeterminate;
		}
	}

	// xs:gMonth
	public class XsdGMonth : XsdAnySimpleType
	{
		internal XsdGMonth ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
		
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return DateTime.ParseExact (Normalize(s), "--MM--", null);
		}
		
		internal override XsdOrdering Compare(object x, object y) {
			/* Really simple so far */
			if ((x is DateTime) && (y is DateTime)) {
				int ret = DateTime.Compare((DateTime)x, (DateTime)y);
				if (ret < 0) { 
					return XsdOrdering.LessThan;
				}
				else if (ret > 0) {
					return XsdOrdering.GreaterThan;
				}
				else {
					return XsdOrdering.Equal;
				}
			}
			return XsdOrdering.Indeterminate;
		}
	}

	// xs:gDay
	public class XsdGDay : XsdAnySimpleType
	{
		internal XsdGDay ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
		
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr)
		{
			return DateTime.ParseExact (Normalize(s), "---dd", null);
		}
		
		internal override XsdOrdering Compare(object x, object y) {
			/* Really simple so far */
			if ((x is DateTime) && (y is DateTime)) {
				int ret = DateTime.Compare((DateTime)x, (DateTime)y);
				if (ret < 0) { 
					return XsdOrdering.LessThan;
				}
				else if (ret > 0) {
					return XsdOrdering.GreaterThan;
				}
				else {
					return XsdOrdering.Equal;
				}
			}
			return XsdOrdering.Indeterminate;
		}
	}

}
