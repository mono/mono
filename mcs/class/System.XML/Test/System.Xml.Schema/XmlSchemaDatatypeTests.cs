//
// System.Xml.XmlSchemaDatatypeTests.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//   Wojciech Kotlarski <wojciech.kotlarski@7digital.com>
//   Andres G. Aragoneses <andres.aragoneses@7digital.com>
//
// (C) 2002 Atsushi Enomoto
// (C) 2012 7digital Media Ltd.
//

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using NUnit.Framework;

using QName = System.Xml.XmlQualifiedName;
using SimpleType = System.Xml.Schema.XmlSchemaSimpleType;
using SimpleRest = System.Xml.Schema.XmlSchemaSimpleTypeRestriction;
using AssertType = NUnit.Framework.Assert;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaDatatypeTests
	{
		private XmlSchema GetSchema (string path)
		{
			return XmlSchema.Read (new XmlTextReader (path), null);
		}

		private XmlQualifiedName QName (string name, string ns)
		{
			return new XmlQualifiedName (name, ns);
		}

		private void AssertDatatype (XmlSchema schema, int index,
			XmlTokenizedType tokenizedType, Type type, string rawValue, object parsedValue)
		{
			XmlSchemaElement element = schema.Items [index] as XmlSchemaElement;
			XmlSchemaDatatype dataType = element.ElementType as XmlSchemaDatatype;
			Assert.AreEqual (tokenizedType, dataType.TokenizedType, "#1");
			Assert.AreEqual (type, dataType.ValueType, "#2");
			Assert.AreEqual (parsedValue, dataType.ParseValue (rawValue, null, null), "#3");
		}

		[Test]
		[Ignore ("The behavior has been inconsistent between versions, so it is not worthy of testing.")]
		// Note that it could also apply to BaseTypeName (since if 
		// it is xs:anyType and BaseType is empty, BaseTypeName
		// should be xs:anyType).
		public void TestAnyType ()
		{
			XmlSchema schema = GetSchema ("Test/XmlFiles/xsd/datatypesTest.xsd");
			schema.Compile (null);
			XmlSchemaElement any = schema.Elements [QName ("e00", "urn:bar")] as XmlSchemaElement;
			XmlSchemaComplexType cType = any.ElementType as XmlSchemaComplexType;
			Assert.AreEqual (typeof (XmlSchemaComplexType), cType.GetType (), "#1");
			Assert.IsNotNull (cType, "#2");
			Assert.AreEqual (XmlQualifiedName.Empty, cType.QualifiedName, "#3");
			Assert.IsNull (cType.BaseSchemaType, "#4");  // In MS.NET 2.0 its null. In 1.1 it is not null.
			Assert.IsNotNull (cType.ContentTypeParticle, "#5");
		}

		[Test]
		public void TestAll ()
		{
			XmlSchema schema = GetSchema ("Test/XmlFiles/xsd/datatypesTest.xsd");
			schema.Compile (null);

			AssertDatatype (schema, 1, XmlTokenizedType.CDATA, typeof (string), " f o o ", " f o o ");
			AssertDatatype (schema, 2, XmlTokenizedType.CDATA, typeof (string), " f o o ", " f o o ");
			// token shouldn't allow " f o o "
			AssertDatatype (schema, 3, XmlTokenizedType.CDATA, typeof (string), "f o o", "f o o");
			// language seems to be checked strictly
			AssertDatatype (schema, 4, XmlTokenizedType.CDATA, typeof (string), "x-foo", "x-foo");

			// NMTOKEN shouldn't allow " f o o "
//			AssertDatatype (schema, 5, XmlTokenizedType.NMTOKEN, typeof (string), "foo", "foo");
//			AssertDatatype (schema, 6, XmlTokenizedType.NMTOKEN, typeof (string []), "f o o", new string [] {"f",  "o",  "o"});
		}

		[Test]
		public void AnyUriRelativePath ()
		{
			XmlValidatingReader vr = new XmlValidatingReader (
				new XmlTextReader (
					// relative path value that contains ':' should be still valid.
					"<root>../copy/myserver/</root>", 
					XmlNodeType.Document, null));
			vr.Schemas.Add (XmlSchema.Read (
				new XmlTextReader ("<xs:schema xmlns:xs='"
					+ XmlSchema.Namespace +
					"'><xs:element name='root' type='xs:anyURI' /></xs:schema>",
					XmlNodeType.Document, null), null));
			vr.Read ();
			vr.Read ();
			vr.Read ();
		}

		[Test]
#if !NET_2_0
		[Category ("NotDotNet")]
#endif
		public void AnyUriRelativePathContainsColon ()
		{
			XmlValidatingReader vr = new XmlValidatingReader (
				new XmlTextReader (
					// relative path value that contains ':' should be still valid.
					"<root>../copy/myserver/c:/foo</root>", 
					XmlNodeType.Document, null));
			vr.Schemas.Add (XmlSchema.Read (
				new XmlTextReader ("<xs:schema xmlns:xs='"
					+ XmlSchema.Namespace +
					"'><xs:element name='root' type='xs:anyURI' /></xs:schema>",
					XmlNodeType.Document, null), null));
			vr.Read ();
			vr.Read ();
			vr.Read ();
		}

		string [] allTypes = new string [] {
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

		XmlSchemaSet allWrappers;

		void SetupSimpleTypeWrappers ()
		{
			XmlSchema schema = new XmlSchema ();
			List<QName> qnames = new List<QName> ();
			foreach (string name in allTypes) {
				SimpleType st = new SimpleType ();
				st.Name = "x-" + name;
				SimpleRest r = new SimpleRest ();
				st.Content = r;
				QName qname = new QName (name, XmlSchema.Namespace);
				r.BaseTypeName = qname;
				qnames.Add (qname);
				schema.Items.Add (st);
			}
			XmlSchemaSet sset = new XmlSchemaSet ();
			sset.Add (schema);
			sset.Compile ();
			allWrappers = sset;
		}

		XmlSchemaDatatype GetDatatype (string name)
		{
			return (allWrappers.GlobalTypes [new QName ("x-" + name,
				String.Empty)] as SimpleType).Datatype;
		}

		string [] GetDerived (string target)
		{
			XmlSchemaDatatype strType = GetDatatype (target);
			List<string> results = new List<string> ();
			foreach (string name in allTypes) {
				if (name == target)
					continue;
				XmlSchemaDatatype deriv = GetDatatype (name);
				if (deriv.IsDerivedFrom (strType))
					results.Add (name);
				else Console.Error.WriteLine (deriv.GetType () + " is not derived from " + strType.GetType ());
			}
			return results.ToArray ();
		}

		[Test]
		public void IsDerivedFrom ()
		{
			SetupSimpleTypeWrappers ();

			// Funky, but XmlSchemaDatatype.IsDerivedFrom() is
			// documented to always return false, but actually
			// matches the same type - which could be guessed that
			// this method is used only to detect user-defined
			// simpleType derivation.
			foreach (string b in allTypes)
				foreach (string d in allTypes)
					AssertType.AreEqual (b == d, GetDatatype (d).IsDerivedFrom (GetDatatype (b)), b);

			AssertType.IsFalse (GetDatatype ("string").IsDerivedFrom (null), "null arg");
		}

		[Test]
		public void ChangeType_StringTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String).Datatype;
			Assert.IsTrue (datatype != null);
			Assert.AreEqual (XmlTypeCode.String, datatype.TypeCode);
			Assert.AreEqual (typeof(string), datatype.ValueType);

			Assert.AreEqual ("test", datatype.ChangeType("test", typeof(string)));
		}

		[Test]
		public void ChangeType_StringToObjectTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String).Datatype;
			Assert.IsTrue (datatype != null);
			Assert.AreEqual (XmlTypeCode.String, datatype.TypeCode);
			Assert.AreEqual (typeof(string), datatype.ValueType);

			Assert.AreEqual ("test", datatype.ChangeType("test", typeof(object)));
		}

		[Test]
		public void ChangeType_IntegerTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Integer).Datatype;
			Assert.IsTrue (datatype != null);
			Assert.AreEqual (XmlTypeCode.Integer, datatype.TypeCode);
			Assert.AreEqual (typeof(decimal), datatype.ValueType);

			Assert.AreEqual (300, datatype.ChangeType("300", typeof(int)));
		}

		[Test]
		public void ChangeType_FromDateTimeTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DateTime).Datatype;
			Assert.IsTrue (datatype != null);
			Assert.AreEqual (XmlTypeCode.DateTime, datatype.TypeCode);
			Assert.AreEqual (typeof(DateTime), datatype.ValueType);

			DateTime date = new DateTime (2012, 06, 27, 0, 0, 0, DateTimeKind.Utc);
			Assert.AreEqual ("2012-06-27T00:00:00Z", datatype.ChangeType(date, typeof(string)));
		}

		[Test]
		public void ChangeType_FromTimeSpanTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DayTimeDuration).Datatype;
			Assert.IsTrue (datatype != null);
			Assert.AreEqual (XmlTypeCode.DayTimeDuration, datatype.TypeCode);
			Assert.AreEqual (typeof(TimeSpan), datatype.ValueType);

			TimeSpan span = new TimeSpan(1, 2, 3);
			Assert.AreEqual ("PT1H2M3S", datatype.ChangeType(span, typeof(string)));
		}

		[Test]
		public void ChangeType_ToDateTimeTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DateTime).Datatype;
			Assert.IsTrue (datatype != null);
			Assert.AreEqual (XmlTypeCode.DateTime, datatype.TypeCode);
			Assert.AreEqual (typeof(DateTime), datatype.ValueType);

			DateTime date = new DateTime (2012, 06, 27, 0, 0, 0, DateTimeKind.Utc);
			Assert.AreEqual (date, datatype.ChangeType("2012-06-27T00:00:00Z", typeof(DateTime)));
		}

		[Test]
		public void ChangeType_ToTimeSpanTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DayTimeDuration).Datatype;
			Assert.IsTrue (datatype != null);
			Assert.AreEqual (XmlTypeCode.DayTimeDuration, datatype.TypeCode);
			Assert.AreEqual (typeof(TimeSpan), datatype.ValueType);

			TimeSpan span = new TimeSpan(1, 2, 3);
			Assert.AreEqual (span, datatype.ChangeType("PT1H2M3S", typeof(TimeSpan)));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ChangeType_NullValueArgumentInFromStringTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Integer).Datatype;
			datatype.ChangeType(null, typeof(string));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ChangeType_NullValueArgumentInToStringTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Integer).Datatype;
			datatype.ChangeType(null, typeof(int));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ChangeType_NullTargetArgumentInFromStringTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Integer).Datatype;
			datatype.ChangeType("100", null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ChangeType_NullNamespaceResolverArgumentInFromStringTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Integer).Datatype;
			datatype.ChangeType("100", typeof(string), null);
		}

		[Test]
		[Category("NotWorking")]
		[ExpectedException (typeof(InvalidCastException))]
		public void InvalidCastExceptionTest()
		{
			XmlSchemaDatatype datatype = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DateTime).Datatype;
			Assert.IsTrue (datatype != null);
			Assert.AreEqual (XmlTypeCode.DateTime, datatype.TypeCode);
			Assert.AreEqual (typeof(DateTime), datatype.ValueType);

			datatype.ChangeType(300, typeof (int));
		}
		
		[Test]
		public void Bug12469 ()
		{
			Dictionary<string, string> validValues = new Dictionary<string, string> {
				{"string", "abc"},

				{"normalizedString", "abc"},
				{"token", "abc"},
				{"language", "en"},
				{"Name", "abc"},
				{"NCName", "abc"},
				{"ID", "abc"},
				{"ENTITY", "abc"},
				{"NMTOKEN", "abc"},

				{"boolean", "true"},
				{"decimal", "1"},
				{"integer", "1"},
				{"nonPositiveInteger", "0"},
				{"negativeInteger", "-1"},
				{"long", "9223372036854775807"},
				{"int", "2147483647"},
				{"short", "32767"},
				{"byte", "127"},
				{"nonNegativeInteger", "0"},
				{"unsignedLong", "18446744073709551615"},
				{"unsignedInt", "4294967295"},
				{"unsignedShort", "65535"},
				{"unsignedByte", "255"},
				{"positiveInteger", "1"},
				{"float", "1.1"},
				{"double", "1.1"},
				{"time", "00:00:00"},
				{"date", "1999-12-31"},
				{"dateTime", "1999-12-31T00:00:00.000"},
				{"duration", "P1Y2M3DT10H30M"},
				{"gYearMonth", "1999-01"},
				{"gYear", "1999"},
				{"gMonthDay", "--12-31"},
				{"gMonth", "--12"},
				{"gDay", "---31"},

				{"base64Binary", "AbCd eFgH IjKl 019+"},
				{"hexBinary", "0123456789ABCDEF"},

				{"anyURI", "https://www.server.com"},
				{"QName", "xml:abc"},
				};

			// FIXME: implement validation
			Dictionary<string, string> invalidValues = new Dictionary<string, string> {
				{"Name", "***"},
				{"NCName", "a::"},
				{"ID", "123"},
				{"ENTITY", "***"},
				{"NMTOKEN", "***"},

				{"boolean", "ABC"},
				{"decimal", "1A"},
				{"integer", "1.5"},
				{"nonPositiveInteger", "5"},
				{"negativeInteger", "10"},
				{"long", "999999999999999999999999999999999999999"},
				{"int", "999999999999999999999999999999999999999"},
				{"short", "32768"},
				{"byte", "128"},
				{"nonNegativeInteger", "-1"},
				{"unsignedLong", "-1"},
				{"unsignedInt", "-1"},
				{"unsignedShort", "-1"},
				{"unsignedByte", "-1"},
				{"positiveInteger", "0"},
				{"float", "1.1x"},
				{"double", "1.1x"},
				{"time", "0"},
				{"date", "1"},
				{"dateTime", "2"},
				{"duration", "P1"},
				{"gYearMonth", "1999"},
				{"gYear", "-1"},
				{"gMonthDay", "-12-31"},
				{"gMonth", "-12"},
				{"gDay", "--31"},

				{"base64Binary", "####"},
				{"hexBinary", "G"},

				// anyURI passes everything (as long as I observed)
				{"QName", "::"},
				};

			const string schemaTemplate = @"
				<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified'> 
					<xs:element name='EL'>
						<xs:complexType>
							<xs:attribute name='attr' type='xs:{0}' use='required' />
						</xs:complexType>
					</xs:element>
				</xs:schema>";

			const string documentTemplate = @"<EL attr='{0}' />";

			foreach (var type in validValues.Keys) {
				try {
					var schema = string.Format (schemaTemplate, type);
					var document = string.Format (documentTemplate, validValues[type]);

					var schemaSet = new XmlSchemaSet ();
					using (var reader = new StringReader (schema))
						schemaSet.Add (XmlSchema.Read (reader, null));
					schemaSet.Compile ();
					var doc = new XmlDocument ();
					using (var reader = new StringReader (document))
						doc.Load (reader);
					doc.Schemas = schemaSet;
					doc.Validate (null);

					// FIXME: implement validation
					/*
					if (!invalidValues.ContainsKey (type))
						continue;
					try {
						doc = new XmlDocument ();
						document = string.Format (documentTemplate, invalidValues [type]);
						using (var reader = new StringReader (document))
							doc.Load (reader);
						doc.Schemas = schemaSet;
						doc.Validate (null);
						Assert.Fail (string.Format ("Failed to invalidate {0} for {1}", document, type));
					} catch (XmlSchemaException) {
					}
					*/
				} catch (Exception) {
					Console.Error.WriteLine (type);
					throw;
				}
			}
		}
	}
}
