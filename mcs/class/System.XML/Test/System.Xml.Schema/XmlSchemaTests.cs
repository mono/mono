//
// System.Xml.XmlSchema.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaTests : Assertion
	{
		private XmlSchema GetSchema (string path)
		{
			return XmlSchema.Read (new XmlTextReader (path), null);
		}

		private XmlQualifiedName QName (string name, string ns)
		{
			return new XmlQualifiedName (name, ns);
		}

		private void AssertElement (XmlSchemaElement element,
			string name, XmlQualifiedName refName, string id,
			XmlQualifiedName schemaTypeName, XmlSchemaType schemaType)
		{
			AssertNotNull (element);
			AssertEquals (name, element.Name);
			AssertEquals (refName, element.RefName);
			AssertEquals (id, element.Id);
			AssertEquals (schemaTypeName, element.SchemaTypeName);
			AssertEquals (schemaType, element.SchemaType);
		}

		private void AssertElementEx (XmlSchemaElement element,
			XmlSchemaDerivationMethod block, XmlSchemaDerivationMethod final,
			string defaultValue, string fixedValue,
			XmlSchemaForm form, bool isAbstract, bool isNillable,
			XmlQualifiedName substGroup)
		{
			AssertNotNull (element);
			AssertEquals (block, element.Block);
			AssertEquals (final, element.Final);
			AssertEquals (defaultValue, element.DefaultValue);
			AssertEquals (fixedValue, element.FixedValue);
			AssertEquals (form, element.Form);
			AssertEquals (isAbstract, element.IsAbstract);
			AssertEquals (isNillable, element.IsNillable);
			AssertEquals (substGroup, element.SubstitutionGroup);
		}

		private void AssertCompiledComplexType (XmlSchemaComplexType cType,
			XmlQualifiedName name,
			int attributesCount, int attributeUsesCount,
			bool existsAny, Type contentModelType,
			bool hasContentTypeParticle,
			XmlSchemaContentType contentType)
		{
			AssertNotNull (cType);
			AssertEquals (name.Name, cType.Name);
			AssertEquals (name, cType.QualifiedName);
			AssertEquals (attributesCount, cType.Attributes.Count);
			AssertEquals (attributeUsesCount, cType.AttributeUses.Count);
			Assert (existsAny == (cType.AttributeWildcard != null));
			if (contentModelType == null)
				AssertNull (cType.ContentModel);
			else
				AssertEquals (contentModelType, cType.ContentModel.GetType ());
			AssertEquals (contentType, cType.ContentType);
			AssertEquals (hasContentTypeParticle, cType.ContentTypeParticle != null);
		}

		private void AssertCompiledElement (XmlSchemaElement element,
			XmlQualifiedName name, object elementType)
		{
			AssertNotNull (element);
			AssertEquals (name, element.QualifiedName);
			AssertEquals (elementType, element.ElementType);
		}

		[Test]
		public void TestRead ()
		{
			XmlSchema schema = GetSchema ("XmlFiles/xsd/1.xsd");
			AssertEquals (6, schema.Items.Count);

			bool fooValidated = false;
			bool barValidated = false;
			string ns = "urn:bar";

			foreach (XmlSchemaObject obj in schema.Items) {
				XmlSchemaElement element = obj as XmlSchemaElement;
				if (element == null)
					continue;
				if (element.Name == "Foo") {
					AssertElement (element, "Foo", 
						XmlQualifiedName.Empty, null,
						QName ("string", XmlSchema.Namespace), null);
					fooValidated = true;
				}
				if (element.Name == "Bar") {
					AssertElement (element, "Bar",
						XmlQualifiedName.Empty, null, QName ("FugaType", ns), null);
					barValidated = true;
				}
			}
			Assert (fooValidated);
			Assert (barValidated);
		}

		[Test]
		public void TestCompile ()
		{
			XmlSchema schema = GetSchema ("XmlFiles/xsd/1.xsd");
			schema.Compile (null);
			string ns = "urn:bar";

			XmlQualifiedName qname = QName ("HogeType", ns);
			XmlSchemaComplexType cType = schema.SchemaTypes [qname] as XmlSchemaComplexType;
			AssertCompiledComplexType (cType, qname, 0, 0,
				false, null, true, XmlSchemaContentType.ElementOnly);

			qname = QName ("FugaType", ns);
			cType = schema.SchemaTypes [qname] as XmlSchemaComplexType;
			AssertCompiledComplexType (cType, qname, 0, 0,
				false, typeof (XmlSchemaComplexContent),
				true, XmlSchemaContentType.ElementOnly);
			AssertNotNull (cType.BaseSchemaType);

			qname = QName ("Bar", ns);
			XmlSchemaElement element = schema.Elements [qname] as XmlSchemaElement;
			AssertCompiledElement (element, qname, cType);
		}
	}
}
