//
// System.Xml.Schema.XmlSchemaDatatype.cs
//
// Author:
//	Atsushi Enomoto
//
// (C)2003 Atsushi Enomoto
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Globalization;
using System.Security.Cryptography;

#if NET_2_0
using NSResolver = System.Xml.IXmlNamespaceResolver;
#else
using NSResolver = System.Xml.XmlNamespaceManager;
#endif


namespace Mono.Xml.Schema
{
	internal enum XsdWhitespaceFacet
	{
		Preserve,
		Replace,
		Collapse
	}

	internal enum XsdOrderedFacet
	{
		False,
		Partial,
		Total
	}

	internal enum XsdOrdering 
	{
		LessThan = -1,
		Equal = 0,
		GreaterThan = 1,
		Indeterminate = 2
	}
	
	internal class XsdAnySimpleType : XmlSchemaDatatype
	{
		static XsdAnySimpleType instance;
		static XsdAnySimpleType ()
		{
			instance = new XsdAnySimpleType ();
		}

		public static XsdAnySimpleType Instance {
			get { return instance; }
		}

		protected XsdAnySimpleType ()
		{
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.AnyAtomicType; }
		}
#endif

		static readonly char [] whitespaceArray = new char [] {' '};

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
			get {
				if (XmlSchemaUtil.StrictMsCompliant)
					return typeof (string);
				else
					return typeof (object);
			}
		}

		public override XmlTokenizedType TokenizedType {
			get {
				return XmlTokenizedType.None;
			}
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return Normalize (s);
		}

		internal override ValueType ParseValueType (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return new StringValueType (Normalize (s));
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
	
		internal virtual int Length(string s) {
			return s.Length;
		}

		
		// anySimpleType allows any facet
		internal virtual XmlSchemaFacet.Facet AllowedFacets {
			get { return XmlSchemaFacet.AllFacets ;}
		}

		/* Matches facets allowed on boolean type
		 */
		internal static readonly XmlSchemaFacet.Facet booleanAllowedFacets = 
					 XmlSchemaFacet.Facet.pattern | XmlSchemaFacet.Facet.whiteSpace;

		/* Matches facets allowed on decimal type. 
		 */
		internal static readonly XmlSchemaFacet.Facet decimalAllowedFacets = 
							XmlSchemaFacet.Facet.pattern | XmlSchemaFacet.Facet.enumeration | 
							XmlSchemaFacet.Facet.whiteSpace | XmlSchemaFacet.Facet.maxInclusive | 
							XmlSchemaFacet.Facet.minInclusive | XmlSchemaFacet.Facet.maxExclusive | 
							XmlSchemaFacet.Facet.minExclusive | XmlSchemaFacet.Facet.fractionDigits | 
							XmlSchemaFacet.Facet.totalDigits ;

		/* Matches facets allowed on float, double, duration, dateTime, time, date,
		 * gYearMonth, gYear, gMonthDay, gMonth, and gDay types
		 */

		internal static readonly XmlSchemaFacet.Facet durationAllowedFacets = 
							XmlSchemaFacet.Facet.pattern | XmlSchemaFacet.Facet.enumeration | 
							XmlSchemaFacet.Facet.whiteSpace | XmlSchemaFacet.Facet.maxInclusive |
							XmlSchemaFacet.Facet.minInclusive | XmlSchemaFacet.Facet.maxExclusive |
							XmlSchemaFacet.Facet.minExclusive ;

		/* Matches facet allowed on string, hexBinary, base64Binary,
		 * anyURI, QName and NOTATION types 
		 *
		 * Also used on list types
		 */

		internal static readonly XmlSchemaFacet.Facet stringAllowedFacets = 
						 XmlSchemaFacet.Facet.length | XmlSchemaFacet.Facet.minLength |
						 XmlSchemaFacet.Facet.maxLength | XmlSchemaFacet.Facet.pattern | 
						 XmlSchemaFacet.Facet.enumeration | XmlSchemaFacet.Facet.whiteSpace; 
	}

#if NET_2_0
	internal class XdtAnyAtomicType : XsdAnySimpleType
	{
		internal XdtAnyAtomicType ()
		{
		}

		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.AnyAtomicType; }
		}
	}

	internal class XdtUntypedAtomic : XdtAnyAtomicType
	{
		internal XdtUntypedAtomic ()
		{
		}

		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.UntypedAtomic; }
		}
	}
#endif

	// xs:string
	internal class XsdString : XsdAnySimpleType
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

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.String; }
		}
#endif

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

	}

	// xs:normalizedString
	internal class XsdNormalizedString : XsdString
	{
		internal XsdNormalizedString ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Replace;
		}

		public override XmlTokenizedType TokenizedType {

			get { return XmlTokenizedType.CDATA; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.NormalizedString; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:token
	internal class XsdToken : XsdNormalizedString
	{
		internal XsdToken ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Token; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs:language
	internal class XsdLanguage : XsdToken
	{
		internal XsdLanguage ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Language; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string); }
		}

		// ParseValue () method is as same as that of xs:string
	}

	// xs;NMTOKEN
	internal class XsdNMToken : XsdToken
	{
		internal XsdNMToken ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.NMTOKEN; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.NmToken; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			if (!XmlChar.IsNmToken (s))
				throw new ArgumentException ("'" + s + "' is an invalid NMTOKEN.");
			return s;
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return new StringValueType (ParseValue (s, nameTable, nsmgr) as string);
		}
	}

	// xs:NMTOKENS
	internal class XsdNMTokens : XsdNMToken
	{
		internal XsdNMTokens ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.NMTOKENS; }
		}

#if NET_2_0
		[MonoTODO]
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Item; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string []); }
		}

		public override object ParseValue (string value, XmlNameTable nt, NSResolver nsmgr)
		{
			return GetValidatedArray (value, nt);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return new StringArrayValueType (GetValidatedArray (s, nameTable));
		}

		private string [] GetValidatedArray (string value, XmlNameTable nt)
		{
			string [] nmtokens = ParseListValue (value, nt);
			for (int i = 0; i < nmtokens.Length; i++)
				if (!XmlChar.IsNmToken (nmtokens [i]))
					throw new ArgumentException ("Invalid name token.");
			return nmtokens;
		}
	}

	// xs:Name
	internal class XsdName : XsdToken
	{
		internal XsdName ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.CDATA; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Name; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			if (!XmlChar.IsName (s))
				throw new ArgumentException ("'" + s + "' is an invalid name.");
			return s;
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return new StringValueType (ParseValue (s, nameTable, nsmgr) as string);
		}
	}

	// xs:NCName
	internal class XsdNCName : XsdName
	{
		internal XsdNCName ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.NCName; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.NCName; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			if (!XmlChar.IsNCName (s))
				throw new ArgumentException ("'" + s + "' is an invalid NCName.");
			return s;
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return new StringValueType (ParseValue (s, nameTable, nsmgr) as string);
		}
	}

	// xs:ID
	internal class XsdID : XsdName
	{
		internal XsdID ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.ID; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Id; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string); }
		}

		public override object ParseValue (string s, XmlNameTable nt, NSResolver nsmgr)
		{
			if (!XmlChar.IsNCName (s))
				throw new ArgumentException ("'" + s + "' is an invalid NCName.");
			return s;
		}
	}

	// xs:IDREF
	internal class XsdIDRef : XsdName
	{
		internal XsdIDRef ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.IDREF; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Idref; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string); }
		}

		public override object ParseValue (string s, XmlNameTable nt, NSResolver nsmgr)
		{
			if (!XmlChar.IsNCName (s))
				throw new ArgumentException ("'" + s + "' is an invalid NCName.");
			return s;
		}
	}

	// xs:IDREFS
	internal class XsdIDRefs : XsdName
	{
		internal XsdIDRefs ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.IDREFS; }
		}

#if NET_2_0
		[MonoTODO]
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Item; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string []); }
		}

		public override object ParseValue (string value, XmlNameTable nt, NSResolver nsmgr)
		{
			return GetValidatedArray (value, nt);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return new StringArrayValueType (GetValidatedArray (s, nameTable));
		}

		private string [] GetValidatedArray (string value, XmlNameTable nt)
		{
			string [] idrefs = ParseListValue (value, nt);
			for (int i = 0; i < idrefs.Length; i++)
				XmlConvert.VerifyNCName (idrefs [i]);
			return idrefs;
		}
	}

	// xs:ENTITY
	internal class XsdEntity : XsdName
	{
		internal XsdEntity ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.ENTITY; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Entity; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string); }
		}


	}


	// xs:ENTITIES
	internal class XsdEntities : XsdName
	{
		internal XsdEntities ()
		{
		}

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.ENTITIES; }
		}

#if NET_2_0
		[MonoTODO]
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Item; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string []); }
		}

		public override object ParseValue (string value, XmlNameTable nt, NSResolver nsmgr)
		{
			return GetValidatedArray (value, nt);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return new StringArrayValueType (GetValidatedArray (s, nameTable));
		}

		private string [] GetValidatedArray (string value, XmlNameTable nt)
		{
			string [] entities = ParseListValue (value, nt);
			for (int i = 0; i < entities.Length; i++)
				if (!XmlChar.IsName (entities [i]))
					throw new ArgumentException ("Invalid entitiy name.");
			return entities;
		}
	}

	// xs:NOTATION
	internal class XsdNotation : XsdAnySimpleType
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

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Notation; }
		}
#endif

		public override Type ValueType {
			get { return typeof (string); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
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

	}

	// xs:decimal
	internal class XsdDecimal : XsdAnySimpleType
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

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Decimal; }
		}
#endif

		public override Type ValueType {
			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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

	}

	// xs:integer
	internal class XsdInteger : XsdDecimal
	{
		public XsdInteger ()
		{
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Integer; }
		}
#endif

		// Here it may be bigger than int's (or long's) MaxValue.
		public override Type ValueType {
			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			decimal d = XmlConvert.ToDecimal (Normalize (s));
			if (Decimal.Floor (d) != d)
				throw new FormatException ("Integer contains point number.");
			return d;
		}
	 
		
	
	}

	// xs:Long
	internal class XsdLong : XsdInteger
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Long; }
		}
#endif

		public override Type ValueType {
			get { return typeof (long); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdInt : XsdLong
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Int; }
		}
#endif

		public override Type ValueType {
			get { return typeof (int); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdShort : XsdInt
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Short; }
		}
#endif

		public override Type ValueType {
			get { return typeof (short); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdByte : XsdShort
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Byte; }
		}
#endif

		public override Type ValueType {
			get { return typeof (sbyte); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
//	[CLSCompliant (false)]
	internal class XsdNonNegativeInteger : XsdInteger
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.NonNegativeInteger; }
		}
#endif

		public override Type ValueType {
			get { return typeof (decimal); }
		}

//		[CLSCompliant (false)]
		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return XmlConvert.ToDecimal (Normalize (s));
		}
	}

	// xs:unsignedLong
//	[CLSCompliant (false)]
	internal class XsdUnsignedLong : XsdNonNegativeInteger
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.UnsignedLong; }
		}
#endif

		public override Type ValueType {
			get { return typeof (ulong); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
//	[CLSCompliant (false)]
	internal class XsdUnsignedInt : XsdUnsignedLong
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.UnsignedInt; }
		}
#endif

		public override Type ValueType {
			get { return typeof (uint); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
//	[CLSCompliant (false)]
	internal class XsdUnsignedShort : XsdUnsignedInt
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.UnsignedShort; }
		}
#endif

		public override Type ValueType {
			get { return typeof (ushort); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
//	[CLSCompliant (false)]
	internal class XsdUnsignedByte : XsdUnsignedShort
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.UnsignedByte; }
		}
#endif

		public override Type ValueType {
			get { return typeof (byte); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
//	[CLSCompliant (false)]
	internal class XsdPositiveInteger : XsdNonNegativeInteger
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.PositiveInteger; }
		}
#endif

		// It returns decimal, instead of int or long.
		// Maybe MS developers thought about big integer...
		public override Type ValueType {
			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return XmlConvert.ToDecimal (Normalize (s));
		}
	}

	// xs:nonPositiveInteger
	internal class XsdNonPositiveInteger : XsdInteger
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.NonPositiveInteger; }
		}
#endif

		public override Type ValueType {
			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return XmlConvert.ToDecimal (Normalize (s));
		}
	}

	// xs:negativeInteger
	internal class XsdNegativeInteger : XsdNonPositiveInteger
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.NegativeInteger; }
		}
#endif

		public override Type ValueType {

			get { return typeof (decimal); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return XmlConvert.ToDecimal (Normalize (s));
		}
	}

	// xs:float
	internal class XsdFloat : XsdAnySimpleType
	{
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Float; }
		}
#endif

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
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdDouble : XsdAnySimpleType
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

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Double; }
		}
#endif

		public override Type ValueType {
			get { return typeof (double); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdBase64Binary : XsdString
	{
		internal XsdBase64Binary ()
		{
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Base64Binary; }
		}
#endif

		public override Type ValueType {
			get { return typeof (byte[]); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
		        // If it isnt ASCII it isnt valid base64 data
			byte[] inArr = new System.Text.ASCIIEncoding().GetBytes(s);
			FromBase64Transform t = new FromBase64Transform();
			return t.TransformFinalBlock(inArr, 0, inArr.Length);
		}


		internal override int Length(string s) {
			int length = 0;
			int pad = 0;
			int end = s.Length;
			for (int i = 0; i < end; i++) {
			  char c = s[i];
				if (!Char.IsWhiteSpace(c)) {
					if (isData(c))
						length ++;
					else if (isPad(c)) 
						pad++;					
					else 
					  return -1;   // Invalid characters
				}	
			}
			if (pad > 2) 
			  return -1; // Max 2 padding at the end.
			if (pad > 0) 
			  pad = 3-pad;				
			
		  return ((length/4)*3)+pad;
		}	

/* TODO: Use the Base64Table and similar code when it makes it 
 * out of System.Security.Cryptography (currently internal so I 
 * don't think we can use it).
 * 
 */
			private static string ALPHABET =
				"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

			private static byte[] decodeTable;
			static XsdBase64Binary ()
			{
				int len = ALPHABET.Length;

				decodeTable = new byte [1 + (int)'z'];

				for (int i=0; i < decodeTable.Length; i++) {
					decodeTable [i] = Byte.MaxValue;
				}

				for (int i=0; i < len; i++) {
					char ch = ALPHABET [i];
					decodeTable [(int)ch] = (byte) i;
				}
			}
	
		protected static bool isPad(char octect) {
			return (octect == '=');
		}
											
		protected static bool isData(char octect) {
			return ((octect <= 'z') && (decodeTable[octect] != Byte.MaxValue));
		}


		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return new StringValueType (ParseValue (s, nameTable, nsmgr) as string);
		}
	}

	
	// xs:hexBinary
	internal class XsdHexBinary : XsdAnySimpleType
	{
		internal XsdHexBinary ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}

		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return stringAllowedFacets; } 
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.HexBinary; }
		}
#endif

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.None; }
		}

		public override Type ValueType {
			get { return typeof (byte []); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return XmlConvert.FromBinHexString (Normalize (s));
		}
		
		internal override int Length(string s) {
		  return s.Length / 2 + s.Length % 2 ;   // Not sure if odd lengths are even allowed
    }

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return new StringValueType (ParseValue (s, nameTable, nsmgr) as string);
		}

		// Fundamental Facets ... no need to override
	}

	// xs:QName
	internal class XsdQName : XsdName
	{
		internal XsdQName ()
		{
		}

		// Fundamental facets are the same as anySimpleType.

		public override XmlTokenizedType TokenizedType {
			get { return XmlTokenizedType.QName; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.QName; }
		}
#endif

		public override Type ValueType {
			get { return typeof (XmlQualifiedName); }
		}

		// ParseValue () method is as same as that of xs:string
		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			if (nameTable == null)
				throw new ArgumentNullException ("name table");
			if (nsmgr == null)
				throw new ArgumentNullException ("namespace manager");
			XmlQualifiedName name = XmlQualifiedName.Parse (s, nsmgr, true);
			nameTable.Add (name.Name);
			nameTable.Add (name.Namespace);
			return name;
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return new QNameValueType (ParseValue (s, nameTable, nsmgr) as XmlQualifiedName);
		}
	}

	// xs:boolean
	internal class XsdBoolean : XsdAnySimpleType
	{
		internal XsdBoolean ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}

		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return booleanAllowedFacets; } 
		}

		public override XmlTokenizedType TokenizedType {
			get {
				if (XmlSchemaUtil.StrictMsCompliant)
					return XmlTokenizedType.None;
				else
					return XmlTokenizedType.CDATA;
			}
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Boolean; }
		}
#endif

		public override Type ValueType {
			get { return typeof (bool); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdAnyURI : XsdString
	{
		public override XmlTokenizedType TokenizedType {

			get { return XmlTokenizedType.CDATA; }
		}

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.AnyUri; }
		}
#endif

		public override Type ValueType {
			get { return typeof (Uri); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return new XmlSchemaUri (Normalize (s));
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return new UriValueType ((XmlSchemaUri) ParseValue (s, nameTable, nsmgr));
		}
	}

	internal class XmlSchemaUri : Uri
	{
		public string value;

		static bool HasValidScheme (string src)
		{
			int idx = src.IndexOf (':');
			if (idx < 0)
				return false;
			for (int i = 0; i < idx; i++) {
				switch (src [i]) {
				case '+':
				case '-':
				case '.':
					continue;
				default:
					if (Char.IsLetterOrDigit (src [i]))
						continue;
					return false;
				}
			}
			return true;
		}

		// MS BUG: Some strings that contain ':' might result in 
		// exception (MS.NET looks implemented as such).
		public XmlSchemaUri (string src)
			: this (src, HasValidScheme (src))
		{
		}

		private XmlSchemaUri (string src, bool formal)
			: base (formal ? src : "anyuri:" + src, !formal)
		{
			value = src;
		}

		public static bool operator == (XmlSchemaUri v1, XmlSchemaUri v2)
		{
			return v1.value == v2.value;
		}

		public static bool operator != (XmlSchemaUri v1, XmlSchemaUri v2)
		{
			return v1.value != v2.value;
		}

		public override bool Equals (object obj)
		{
			if (obj is XmlSchemaUri)
				return (XmlSchemaUri) obj == this;
			else
				return false;
		}

		public override int GetHashCode () 
		{
			return value.GetHashCode ();
		}

		public override string ToString ()
		{
			return value;
		}
	}

	// xs:duration
	internal class XsdDuration : XsdAnySimpleType
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

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Duration; }
		}
#endif

		public override Type ValueType {
			get { return typeof (TimeSpan); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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

#if NET_2_0
	// xdt:dayTimeDuration
	internal class XdtDayTimeDuration : XsdDuration
	{
		internal XdtDayTimeDuration ()
		{
		}

		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.DayTimeDuration; }
		}

		public override Type ValueType {
			get { return typeof (TimeSpan); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return XmlConvert.ToTimeSpan (Normalize (s));
		}

		// FIXME: Fundamental Facets
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

	// xdt:yearMonthDuration
	internal class XdtYearMonthDuration : XsdDuration
	{
		internal XdtYearMonthDuration ()
		{
		}

		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.YearMonthDuration; }
		}

		public override Type ValueType {
			get { return typeof (TimeSpan); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			return XmlConvert.ToTimeSpan (Normalize (s));
		}

		// FIXME: Fundamental Facets
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
#endif

	// xs:dateTime
	internal class XsdDateTime : XsdAnySimpleType
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

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.DateTime; }
		}
#endif

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdDate : XsdAnySimpleType
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

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Date; }
		}
#endif

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdTime : XsdAnySimpleType
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

#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.Time; }
		}
#endif

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdGYearMonth : XsdAnySimpleType
	{
		internal XsdGYearMonth ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
    
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.GYearMonth; }
		}
#endif

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdGMonthDay : XsdAnySimpleType
	{
		internal XsdGMonthDay ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
    
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.GMonthDay; }
		}
#endif

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdGYear : XsdAnySimpleType
	{
		internal XsdGYear ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
		
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.GYear; }
		}
#endif

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		// LAMESPEC: XML Schema Datatypes allows leading '-' to identify B.C. years,
		// but CLR DateTime does not allow such expression.
		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
	internal class XsdGMonth : XsdAnySimpleType
	{
		internal XsdGMonth ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
		
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.GMonth; }
		}
#endif

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
		{
			DateTime ret;
			return DateTime.TryParseExact (Normalize(s), "--MM", null, DateTimeStyles.RoundtripKind, out ret) ? ret : DateTime.ParseExact (Normalize(s), "--MM--", null);
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
	internal class XsdGDay : XsdAnySimpleType
	{
		internal XsdGDay ()
		{
			this.WhitespaceValue = XsdWhitespaceFacet.Collapse;
		}
		
		internal override XmlSchemaFacet.Facet AllowedFacets {
			get { return durationAllowedFacets; } 
		}
		
#if NET_2_0
		public override XmlTypeCode TypeCode {
			get { return XmlTypeCode.GDay; }
		}
#endif

		public override Type ValueType {
			get { return typeof (DateTime); }
		}

		public override object ParseValue (string s,
			XmlNameTable nameTable, NSResolver nsmgr)
		{
			return ParseValueType (s, nameTable, nsmgr);
		}

		internal override ValueType ParseValueType (string s, XmlNameTable nameTable, NSResolver nsmgr) 
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
