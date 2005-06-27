//
// System.Xml.Schema.XmlSchemaDatatype.cs
//
// Authors:
//	Dwivedi, Ajay kumar <Adwiv@Yahoo.com>
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
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

#if NET_2_0
		public abstract XmlTypeCode TypeCode { get; }

		public virtual XmlSchemaDatatypeVariety Variety {
			get {
				return XmlSchemaDatatypeVariety.Atomic;
			}
		}
#endif

		public abstract XmlTokenizedType TokenizedType {  get; }
		public abstract Type ValueType {  get; }

		// Methods
#if NET_2_0
		[MonoTODO]
		public virtual object ChangeType (object value, Type targetType)
		{
			return ChangeType (value, targetType, null);
		}

		[MonoTODO]
		public virtual object ChangeType (object value, Type targetType, IXmlNamespaceResolver nsResolver)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsDerivedFrom (XmlSchemaDatatype datatype)
		{
			throw new NotImplementedException ();
		}
#endif

#if NET_2_0
		public abstract object ParseValue (string s, 
			XmlNameTable nameTable, IXmlNamespaceResolver nsmgr);

		internal abstract ValueType ParseValueType (string s,
			XmlNameTable nameTable, IXmlNamespaceResolver nsmgr);
#else
		public abstract object ParseValue (string s, 
			XmlNameTable nameTable, XmlNamespaceManager nsmgr);

		internal abstract ValueType ParseValueType (string s,
			XmlNameTable nameTable, XmlNamespaceManager nsmgr);
#endif

		static char [] wsChars = new char [] {' ', '\t', '\n', '\r'};

		StringBuilder sb = new StringBuilder ();
		internal string Normalize (string s)
		{
			return Normalize (s, Whitespace);
		}

		internal string Normalize (string s, XsdWhitespaceFacet whitespaceFacet)
		{
			int idx = s.IndexOfAny (wsChars);
			if (idx < 0)
				return s;
			switch (whitespaceFacet) {
			case XsdWhitespaceFacet.Collapse:
				string [] arr = s.Trim ().Split (wsChars);
				for (int i = 0; i < arr.Length; i++) {
					string one = arr [i];
					if (one != "") {
						sb.Append (one);
						sb.Append (" ");
					}
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
				result = sb.ToString ();
				sb.Length = 0;
				return result;
			default:
				return s;
			}
		}

		internal static XmlSchemaDatatype FromName (XmlQualifiedName qname)
		{
			return FromName (qname.Name, qname.Namespace);
		}

		internal static XmlSchemaDatatype FromName (string localName, string ns)
		{
			switch (ns) {
			case XmlSchema.Namespace:
				break;
#if NET_2_0
			case XmlSchema.XdtNamespace:
				switch (localName) {
				case "anyAtomicType":
					return datatypeAnyAtomicType;
				case "untypedAtomic":
					return datatypeUntypedAtomic;
				case "dayTimeDuration":
					return datatypeDayTimeDuration;
				case "yearMonthDuration":
					return datatypeYearMonthDuration;
				}
				return null;
#endif
			default:
				// Maybe invalid name was specified. In such cases, let processors handle them.
				return null;
			}

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

		static readonly XsdAnySimpleType datatypeAnySimpleType = new XsdAnySimpleType ();
		static readonly XsdString datatypeString = new XsdString ();
		static readonly XsdNormalizedString datatypeNormalizedString = new XsdNormalizedString ();
		static readonly XsdToken datatypeToken = new XsdToken ();
		static readonly XsdLanguage datatypeLanguage = new XsdLanguage ();
		static readonly XsdNMToken datatypeNMToken = new XsdNMToken ();
		static readonly XsdNMTokens datatypeNMTokens = new XsdNMTokens ();
		static readonly XsdName datatypeName = new XsdName ();
		static readonly XsdNCName datatypeNCName = new XsdNCName ();
		static readonly XsdID datatypeID = new XsdID ();
		static readonly XsdIDRef datatypeIDRef = new XsdIDRef ();
		static readonly XsdIDRefs datatypeIDRefs = new XsdIDRefs ();
		static readonly XsdEntity datatypeEntity = new XsdEntity ();
		static readonly XsdEntities datatypeEntities = new XsdEntities ();
		static readonly XsdNotation datatypeNotation = new XsdNotation ();
		static readonly XsdDecimal datatypeDecimal = new XsdDecimal ();
		static readonly XsdInteger datatypeInteger = new XsdInteger ();
		static readonly XsdLong datatypeLong = new XsdLong ();
		static readonly XsdInt datatypeInt = new XsdInt ();
		static readonly XsdShort datatypeShort = new XsdShort ();
		static readonly XsdByte datatypeByte = new XsdByte ();
		static readonly XsdNonNegativeInteger datatypeNonNegativeInteger = new XsdNonNegativeInteger ();
		static readonly XsdPositiveInteger datatypePositiveInteger = new XsdPositiveInteger ();
		static readonly XsdUnsignedLong datatypeUnsignedLong = new XsdUnsignedLong ();
		static readonly XsdUnsignedInt datatypeUnsignedInt = new XsdUnsignedInt ();
		static readonly XsdUnsignedShort datatypeUnsignedShort = new XsdUnsignedShort ();
		static readonly XsdUnsignedByte datatypeUnsignedByte = new XsdUnsignedByte ();
		static readonly XsdNonPositiveInteger datatypeNonPositiveInteger = new XsdNonPositiveInteger ();
		static readonly XsdNegativeInteger datatypeNegativeInteger = new XsdNegativeInteger ();
		static readonly XsdFloat datatypeFloat = new XsdFloat ();
		static readonly XsdDouble datatypeDouble = new XsdDouble ();
		static readonly XsdBase64Binary datatypeBase64Binary = new XsdBase64Binary ();
		static readonly XsdBoolean datatypeBoolean = new XsdBoolean ();
		static readonly XsdAnyURI datatypeAnyURI = new XsdAnyURI ();
		static readonly XsdDuration datatypeDuration = new XsdDuration ();
		static readonly XsdDateTime datatypeDateTime = new XsdDateTime ();
		static readonly XsdDate datatypeDate = new XsdDate ();
		static readonly XsdTime datatypeTime = new XsdTime ();
		static readonly XsdHexBinary datatypeHexBinary = new XsdHexBinary ();
		static readonly XsdQName datatypeQName = new XsdQName ();
		static readonly XsdGYearMonth datatypeGYearMonth = new XsdGYearMonth ();
		static readonly XsdGMonthDay datatypeGMonthDay = new XsdGMonthDay ();
		static readonly XsdGYear datatypeGYear = new XsdGYear ();
		static readonly XsdGMonth datatypeGMonth = new XsdGMonth ();
		static readonly XsdGDay datatypeGDay = new XsdGDay ();
#if NET_2_0
		static readonly XdtAnyAtomicType datatypeAnyAtomicType
			= new XdtAnyAtomicType ();
		static readonly XdtUntypedAtomic datatypeUntypedAtomic
			= new XdtUntypedAtomic ();
		static readonly XdtDayTimeDuration datatypeDayTimeDuration
			= new XdtDayTimeDuration ();
		static readonly XdtYearMonthDuration datatypeYearMonthDuration
			= new XdtYearMonthDuration ();
#endif

	}
}
