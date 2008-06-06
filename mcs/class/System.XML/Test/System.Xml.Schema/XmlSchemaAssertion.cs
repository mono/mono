//
// System.Xml.XmlSchemaAssertion.cs
//
// Author:
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Atsushi Enomoto
//

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaAssertion : Assertion
	{
		protected XmlSchema GetSchema (string path)
		{
			XmlTextReader reader = new XmlTextReader (path);
			XmlSchema schema = XmlSchema.Read (reader, null);
			reader.Close ();
			return schema;
		}

		protected XmlQualifiedName QName (string name, string ns)
		{
			return new XmlQualifiedName (name, ns);
		}

		protected void AssertElement (XmlSchemaElement element,
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

		protected void AssertElementEx (XmlSchemaElement element,
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

		protected void AssertCompiledComplexType (XmlSchemaComplexType cType,
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

		protected void AssertCompiledComplexContentExtension (XmlSchemaComplexContentExtension xccx,
			int attributeCount, bool hasAnyAttribute, XmlQualifiedName baseTypeName)
		{
			AssertNotNull (xccx);
			AssertEquals (attributeCount, xccx.Attributes.Count);
			AssertEquals (hasAnyAttribute, xccx.AnyAttribute != null);
			AssertEquals (baseTypeName, xccx.BaseTypeName);
			AssertNotNull (xccx.Particle);
		}

		protected void AssertCompiledElement (XmlSchemaElement element,
			XmlQualifiedName name, object elementType)
		{
			AssertNotNull (element);
			AssertEquals (name, element.QualifiedName);
			AssertEquals (elementType, element.ElementType);
		}

	}
}
