//
// System.Xml.Schema.XmlSchemaDatatype.cs
//
// Authors:
//	Dwivedi, Ajay kumar <Adwiv@Yahoo.com>
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
using System;
using System.Collections;
using System.Text;
using System.Xml;
using Mono.Xml.Schema;

namespace System.Xml.Schema
{
	public abstract class XmlSchemaDatatype
	{
		protected XmlSchemaDatatype()
		{
		}
		
		internal XsdWhitespaceFacet WhitespaceValue =
			XsdWhitespaceFacet.Preserve;

		// Common Facets
		internal virtual XsdWhitespaceFacet Whitespace {
			get { return WhitespaceValue; }
		}

		public abstract XmlTokenizedType TokenizedType {  get; }
		public abstract Type ValueType {  get; }

		// Methods
		public abstract object ParseValue (string s, 
			XmlNameTable nameTable, XmlNamespaceManager nsmgr);

		internal abstract ValueType ParseValueType (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr);

		static char [] wsChars = new char [] {' ', '\t', '\n', '\r'};

		StringBuilder sb = new StringBuilder ();
		internal string Normalize (string s)
		{
			return Normalize (s, Whitespace);
		}

		internal string Normalize (string s, XsdWhitespaceFacet whitespaceFacet)
		{
			switch (whitespaceFacet) {
			case XsdWhitespaceFacet.Collapse:
				string [] arr = s.Trim ().Split (wsChars);
				foreach (string one in arr)
					if (one != "") {
						sb.Append (one);
						sb.Append (" ");
					}
				string result = sb.ToString ();
				sb.Length = 0;
				return result.Trim ();
			case XsdWhitespaceFacet.Replace:
				sb.Length = 0;
				sb.Append (s);
				for (int i = 0; i < sb.Length; i++)
					switch (sb [i]) {
					case '\r':
					case '\n':
					case '\t':
						sb [i] = ' ';
						break;
					}
				/*
				sb.Replace ('\r', ' ');
				sb.Replace ('\n', ' ');
				sb.Replace ('\t', ' ');
				*/
				result = sb.ToString ();
				sb.Length = 0;
				return result;
			default:
				return s;
			}
		}

		internal static XmlSchemaDatatype FromName (XmlQualifiedName qname)
		{
			if (qname.Namespace != XmlSchema.Namespace)
				throw new InvalidOperationException ("Namespace " + XmlSchema.Namespace + " is required.");
			return FromName (qname.Name);
		}

		internal static XmlSchemaDatatype FromName (string localName)
		{
			switch (localName) {
			case "anySimpleType":
				return datatypeAnySimpleType;
			case "string":
				return datatypeString;
			case "normalizedString":
				return datatypeNormalizedString;
			case "token":
				return datatypeToken;
			case "language":
				return datatypeLanguage;
			case "NMTOKEN":
				return datatypeNMToken;
			case "NMTOKENS":
				return datatypeNMTokens;
			case "Name":
				return datatypeName;
			case "NCName":
				return datatypeNCName;
			case "ID":
				return datatypeID;
			case "IDREF":
				return datatypeIDRef;
			case "IDREFS":
				return datatypeIDRefs;
			case "ENTITY":
				return datatypeEntity;
			case "ENTITIES":
				return datatypeEntities;
			case "NOTATION":
				return datatypeNotation;
			case "decimal":
				return datatypeDecimal;
			case "integer":
				return datatypeInteger;
			case "long":
				return datatypeLong;
			case "int":
				return datatypeInt;
			case "short":
				return datatypeShort;
			case "byte":
				return datatypeByte;
			case "nonPositiveInteger":
				return datatypeNonPositiveInteger;
			case "negativeInteger":
				return datatypeNegativeInteger;
			case "nonNegativeInteger":
				return datatypeNonNegativeInteger;
			case "unsignedLong":
				return datatypeUnsignedLong;
			case "unsignedInt":
				return datatypeUnsignedInt;
			case "unsignedShort":
				return datatypeUnsignedShort;
			case "unsignedByte":
				return datatypeUnsignedByte;
			case "positiveInteger":
				return datatypePositiveInteger;
			case "float":
				return datatypeFloat;
			case "double":
				return datatypeDouble;
			case "base64Binary":
				return datatypeBase64Binary;
			case "boolean":
				return datatypeBoolean;
			case "anyURI":
				return datatypeAnyURI;
			case "duration":
				return datatypeDuration;
			case "dateTime":
				return datatypeDateTime;
			case "date":
				return datatypeDate;
			case "time":
				return datatypeTime;
			case "hexBinary":
				return datatypeHexBinary;
			case "QName":
				return datatypeQName;
			case "gYearMonth":
				return datatypeGYearMonth;
			case "gMonthDay":
				return datatypeGMonthDay;
			case "gYear":
				return datatypeGYear;
			case "gMonth":
				return datatypeGMonth;
			case "gDay":
				return datatypeGDay;
			default:
				// Maybe invalid name was specified. In such cases, let processors handle them.
				return null;
			}
		}

		private static XsdAnySimpleType datatypeAnySimpleType = new XsdAnySimpleType ();
		private static XsdString datatypeString = new XsdString ();
		private static XsdNormalizedString datatypeNormalizedString = new XsdNormalizedString ();
		private static XsdToken datatypeToken = new XsdToken ();
		private static XsdLanguage datatypeLanguage = new XsdLanguage ();
		private static XsdNMToken datatypeNMToken = new XsdNMToken ();
		private static XsdNMTokens datatypeNMTokens = new XsdNMTokens ();
		private static XsdName datatypeName = new XsdName ();
		private static XsdNCName datatypeNCName = new XsdNCName ();
		private static XsdID datatypeID = new XsdID ();
		private static XsdIDRef datatypeIDRef = new XsdIDRef ();
		private static XsdIDRefs datatypeIDRefs = new XsdIDRefs ();
		private static XsdEntity datatypeEntity = new XsdEntity ();
		private static XsdEntities datatypeEntities = new XsdEntities ();
		private static XsdNotation datatypeNotation = new XsdNotation ();
		private static XsdDecimal datatypeDecimal = new XsdDecimal ();
		private static XsdInteger datatypeInteger = new XsdInteger ();
		private static XsdLong datatypeLong = new XsdLong ();
		private static XsdInt datatypeInt = new XsdInt ();
		private static XsdShort datatypeShort = new XsdShort ();
		private static XsdByte datatypeByte = new XsdByte ();
		private static XsdNonNegativeInteger datatypeNonNegativeInteger = new XsdNonNegativeInteger ();
		private static XsdPositiveInteger datatypePositiveInteger = new XsdPositiveInteger ();
		private static XsdUnsignedLong datatypeUnsignedLong = new XsdUnsignedLong ();
		private static XsdUnsignedInt datatypeUnsignedInt = new XsdUnsignedInt ();
		private static XsdUnsignedShort datatypeUnsignedShort = new XsdUnsignedShort ();
		private static XsdUnsignedByte datatypeUnsignedByte = new XsdUnsignedByte ();
		private static XsdNonPositiveInteger datatypeNonPositiveInteger = new XsdNonPositiveInteger ();
		private static XsdNegativeInteger datatypeNegativeInteger = new XsdNegativeInteger ();
		private static XsdFloat datatypeFloat = new XsdFloat ();
		private static XsdDouble datatypeDouble = new XsdDouble ();
		private static XsdBase64Binary datatypeBase64Binary = new XsdBase64Binary ();
		private static XsdBoolean datatypeBoolean = new XsdBoolean ();
		private static XsdAnyURI datatypeAnyURI = new XsdAnyURI ();
		private static XsdDuration datatypeDuration = new XsdDuration ();
		private static XsdDateTime datatypeDateTime = new XsdDateTime ();
		private static XsdDate datatypeDate = new XsdDate ();
		private static XsdTime datatypeTime = new XsdTime ();
		private static XsdHexBinary datatypeHexBinary = new XsdHexBinary ();
		private static XsdQName datatypeQName = new XsdQName ();
		private static XsdGYearMonth datatypeGYearMonth = new XsdGYearMonth ();
		private static XsdGMonthDay datatypeGMonthDay = new XsdGMonthDay ();
		private static XsdGYear datatypeGYear = new XsdGYear ();
		private static XsdGMonth datatypeGMonth = new XsdGMonth ();
		private static XsdGDay datatypeGDay = new XsdGDay ();
	}
}
