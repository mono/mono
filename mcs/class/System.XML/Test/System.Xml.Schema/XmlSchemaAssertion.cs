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
	public class XmlSchemaAssertion
	{
		public static XmlSchema GetSchema (string path)
		{
			XmlTextReader reader = new XmlTextReader (path);
			XmlSchema schema = XmlSchema.Read (reader, null);
			reader.Close ();
			return schema;
		}

		public static XmlQualifiedName QName (string name, string ns)
		{
			return new XmlQualifiedName (name, ns);
		}

		public static void AssertElement (XmlSchemaElement element,
			string name, XmlQualifiedName refName, string id,
			XmlQualifiedName schemaTypeName, XmlSchemaType schemaType)
		{
			Assert.IsNotNull (element);
			Assert.AreEqual (name, element.Name);
			Assert.AreEqual (refName, element.RefName);
			Assert.AreEqual (id, element.Id);
			Assert.AreEqual (schemaTypeName, element.SchemaTypeName);
			Assert.AreEqual (schemaType, element.SchemaType);
		}

		public static void AssertElementEx (XmlSchemaElement element,
			XmlSchemaDerivationMethod block, XmlSchemaDerivationMethod final,
			string defaultValue, string fixedValue,
			XmlSchemaForm form, bool isAbstract, bool isNillable,
			XmlQualifiedName substGroup)
		{
			Assert.IsNotNull (element);
			Assert.AreEqual (block, element.Block);
			Assert.AreEqual (final, element.Final);
			Assert.AreEqual (defaultValue, element.DefaultValue);
			Assert.AreEqual (fixedValue, element.FixedValue);
			Assert.AreEqual (form, element.Form);
			Assert.AreEqual (isAbstract, element.IsAbstract);
			Assert.AreEqual (isNillable, element.IsNillable);
			Assert.AreEqual (substGroup, element.SubstitutionGroup);
		}

		public static void AssertCompiledComplexType (XmlSchemaComplexType cType,
			XmlQualifiedName name,
			int attributesCount, int attributeUsesCount,
			bool existsAny, Type contentModelType,
			bool hasContentTypeParticle,
			XmlSchemaContentType contentType)
		{
			Assert.IsNotNull (cType);
			Assert.AreEqual (name.Name, cType.Name);
			Assert.AreEqual (name, cType.QualifiedName);
			Assert.AreEqual (attributesCount, cType.Attributes.Count);
			Assert.AreEqual (attributeUsesCount, cType.AttributeUses.Count);
			Assert.IsTrue (existsAny == (cType.AttributeWildcard != null));
			if (contentModelType == null)
				Assert.IsNull (cType.ContentModel);
			else
				Assert.AreEqual (contentModelType, cType.ContentModel.GetType ());
			Assert.AreEqual (hasContentTypeParticle, cType.ContentTypeParticle != null);
			Assert.AreEqual (contentType, cType.ContentType);
		}

		public static void AssertCompiledComplexContentExtension (XmlSchemaComplexContentExtension xccx,
			int attributeCount, bool hasAnyAttribute, XmlQualifiedName baseTypeName)
		{
			Assert.IsNotNull (xccx);
			Assert.AreEqual (attributeCount, xccx.Attributes.Count);
			Assert.AreEqual (hasAnyAttribute, xccx.AnyAttribute != null);
			Assert.AreEqual (baseTypeName, xccx.BaseTypeName);
			Assert.IsNotNull (xccx.Particle);
		}

		public static void AssertCompiledElement (XmlSchemaElement element,
			XmlQualifiedName name, object elementType)
		{
			Assert.IsNotNull (element);
			Assert.AreEqual (name, element.QualifiedName);
			Assert.AreEqual (elementType, element.ElementType);
		}

	}
}
