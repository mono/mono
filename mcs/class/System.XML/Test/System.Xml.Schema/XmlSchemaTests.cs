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
			AssertEquals (hasContentTypeParticle, cType.ContentTypeParticle != null);
			AssertEquals (contentType, cType.ContentType);
		}

		private void AssertCompiledComplexContentExtension (XmlSchemaComplexContentExtension xccx,
			int attributeCount, bool hasAnyAttribute, XmlQualifiedName baseTypeName)
		{
			AssertNotNull (xccx);
			AssertEquals (attributeCount, xccx.Attributes.Count);
			AssertEquals (hasAnyAttribute, xccx.AnyAttribute != null);
			AssertEquals (baseTypeName, xccx.BaseTypeName);
			AssertNotNull (xccx.Particle);
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
			XmlQualifiedName qname;
			XmlSchemaComplexContentExtension xccx;
			XmlSchemaComplexType cType;
			XmlSchemaSequence seq;

			XmlSchema schema = GetSchema ("XmlFiles/xsd/1.xsd");
			schema.Compile (null);
			string ns = "urn:bar";

			// HogeType
			qname = QName ("HogeType", ns);
			cType = schema.SchemaTypes [qname] as XmlSchemaComplexType;
			AssertNotNull (cType);
			AssertNull (cType.ContentModel);
			AssertCompiledComplexType (cType, qname, 0, 0,
				false, null, true, XmlSchemaContentType.ElementOnly);
			seq = cType.ContentTypeParticle as XmlSchemaSequence;
			AssertNotNull (seq);
			AssertEquals (2, seq.Items.Count);
			XmlSchemaElement refFoo = seq.Items [0] as XmlSchemaElement;
			AssertCompiledElement (refFoo, QName ("Foo", ns),
				((XmlSchemaElement) schema.Elements [QName ("Foo", ns)]).ElementType);

			// FugaType
			qname = QName ("FugaType", ns);
			cType = schema.SchemaTypes [qname] as XmlSchemaComplexType;
			AssertNotNull (cType);
			xccx = cType.ContentModel.Content as XmlSchemaComplexContentExtension;
			AssertCompiledComplexContentExtension (
				xccx, 0, false, QName ("HogeType", ns));

			AssertCompiledComplexType (cType, qname, 0, 0,
				false, typeof (XmlSchemaComplexContent),
				true, XmlSchemaContentType.ElementOnly);
			AssertNotNull (cType.BaseSchemaType);

			seq = xccx.Particle as XmlSchemaSequence;
			AssertNotNull (seq);
			AssertEquals (1, seq.Items.Count);
			XmlSchemaElement refBaz = seq.Items [0] as XmlSchemaElement;
			AssertNotNull (refBaz);
			AssertCompiledElement (refBaz, QName ("Baz", ""),
				((XmlSchemaElement) schema.Elements [QName ("Foo", ns)]).ElementType);

			qname = QName ("Bar", ns);
			XmlSchemaElement element = schema.Elements [qname] as XmlSchemaElement;
			AssertCompiledElement (element, qname, cType);
		}
	}
}
