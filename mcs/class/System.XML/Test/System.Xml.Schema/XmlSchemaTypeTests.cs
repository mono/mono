//
// System.Xml.XmlSchemaSetTests.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
//

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

using QName = System.Xml.XmlQualifiedName;
using SimpleType = System.Xml.Schema.XmlSchemaSimpleType;
using SimpleRest = System.Xml.Schema.XmlSchemaSimpleTypeRestriction;
using AssertType = NUnit.Framework.Assert;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaTypeTests
	{
#if NET_2_0
		string [] all_types = new string [] {
			"string", "boolean", "float", "double", "decimal", 
			"duration", "dateTime", "time", "date", "gYearMonth", 
			"gYear", "gMonthDay", "gDay", "gMonth", "hexBinary", 
			"base64Binary", "anyURI", "QName", "NOTATION", 
			"normalizedString", "token", "language", "IDREFS",
			"ENTITIES", "NMTOKEN", "NMTOKENS", "Name", "NCName",
			"ID", "IDREF", "ENTITY", "integer",
			"nonPositiveInteger", "negativeInteger", "long",
			"int", "short", "byte", "nonNegativeInteger",
			"unsignedLong", "unsignedInt", "unsignedShort",
			"unsignedByte", "positiveInteger"
			};

		XmlTypeCode [] type_codes = new XmlTypeCode [] {
			XmlTypeCode.String,
			XmlTypeCode.Boolean,
			XmlTypeCode.Float,
			XmlTypeCode.Double,
			XmlTypeCode.Decimal,
			XmlTypeCode.Duration,
			XmlTypeCode.DateTime,
			XmlTypeCode.Time,
			XmlTypeCode.Date,
			XmlTypeCode.GYearMonth,
			XmlTypeCode.GYear,
			XmlTypeCode.GMonthDay,
			XmlTypeCode.GDay,
			XmlTypeCode.GMonth,
			XmlTypeCode.HexBinary,
			XmlTypeCode.Base64Binary,
			XmlTypeCode.AnyUri,
			XmlTypeCode.QName,
			XmlTypeCode.Notation,
			XmlTypeCode.NormalizedString,
			XmlTypeCode.Token,
			XmlTypeCode.Language,
			XmlTypeCode.Idref, // IDREFS (LAMESPEC)
			XmlTypeCode.Entity, // ENTITIES (LAMESPEC)
			XmlTypeCode.NmToken,
			XmlTypeCode.NmToken, // NMTOKENS (LAMESPEC)
			XmlTypeCode.Name,
			XmlTypeCode.NCName,
			XmlTypeCode.Id,
			XmlTypeCode.Idref,
			XmlTypeCode.Entity,
			XmlTypeCode.Integer,
			XmlTypeCode.NonPositiveInteger,
			XmlTypeCode.NegativeInteger,
			XmlTypeCode.Long,
			XmlTypeCode.Int,
			XmlTypeCode.Short,
			XmlTypeCode.Byte,
			XmlTypeCode.NonNegativeInteger,
			XmlTypeCode.UnsignedLong,
			XmlTypeCode.UnsignedInt,
			XmlTypeCode.UnsignedShort,
			XmlTypeCode.UnsignedByte,
			XmlTypeCode.PositiveInteger};

		[Test]
		public void TypeCode ()
		{
			for (int i = 0; i < all_types.Length; i++) {
				string name = all_types [i];
				QName qname = new QName (name, XmlSchema.Namespace);
				Assert.AreEqual (type_codes [i],
					XmlSchemaType.GetBuiltInSimpleType (qname).TypeCode, name);
			}
		}

#endif
	}
}
