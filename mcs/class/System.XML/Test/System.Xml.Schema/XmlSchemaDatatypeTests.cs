//
// System.Xml.XmlSchemaDatatypeTests.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaDatatypeTests : Assertion
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
			AssertEquals (tokenizedType, dataType.TokenizedType);
			AssertEquals (type, dataType.ValueType);
			AssertEquals (parsedValue, dataType.ParseValue (rawValue, null, null));
		}

		[Test]
		public void TestAnyType ()
		{
			XmlSchema schema = GetSchema ("Test/XmlFiles/xsd/datatypesTest.xsd");
			schema.Compile (null);
			XmlSchemaElement any = schema.Elements [QName ("e00", "urn:bar")] as XmlSchemaElement;
			XmlSchemaComplexType cType = any.ElementType as XmlSchemaComplexType;
			AssertEquals (typeof (XmlSchemaComplexType), cType.GetType ());
			AssertNotNull (cType);
			AssertEquals (XmlQualifiedName.Empty, cType.QualifiedName);
			AssertNull (cType.BaseSchemaType);
			// In MS.NET its type is "XmlSchemaParticle.EmptyParticle"
			AssertNotNull (cType.ContentTypeParticle);
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

	}
}
