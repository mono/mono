//
// System.Xml.Serialization.XmlReflectionImporterTests
//
// Author:
//   Erik LeBel (eriklebel@yahoo.ca)
//
// (C) 2003 Erik LeBel
// 
// FIXME test some of these with Xml Attributes attached to some members: 
// do the names get carried over to Element for XmlAttributeAttribute and XmlElementAttribute?
// 

using System;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	// debugging class
	internal class Debug
	{
		public static void Print(XmlTypeMapping tm)
		{
			Console.WriteLine("/XmlTypeMapping:");
			Console.WriteLine("ElementName: {0} ", tm.ElementName);
			Console.WriteLine("Namespace: {0} ", tm.Namespace);
			Console.WriteLine("TypeName: {0} ", tm.TypeName);
			Console.WriteLine("FullTypeName: {0} ", tm.TypeFullName);
		}

		public static void Print(XmlMemberMapping mm)
		{
			Console.WriteLine("/XmlMemberMapping:");
			Console.WriteLine("Any: {0} ", mm.Any);
			Console.WriteLine("ElementName: {0} ", mm.ElementName);
			Console.WriteLine("MemberName: {0} ", mm.MemberName);
			Console.WriteLine("Namespace: {0} ", mm.Namespace);
			Console.WriteLine("TypeFullName: {0} ", mm.TypeFullName);
			Console.WriteLine("TypeName: {0} ", mm.TypeName);
			Console.WriteLine("TypeNamespace: {0} ", mm.TypeNamespace);
		}
	}

	[TestFixture]
	public class XmlReflectionImporterTests : Assertion
	{
		private const string SomeNamespace = "some:urn";
		private const string AnotherNamespace = "another:urn";

		// these Map methods re-create the XmlReflectionImporter at every call.

		private XmlTypeMapping Map(Type t)
		{
			XmlReflectionImporter ri = new XmlReflectionImporter();
			XmlTypeMapping tm = ri.ImportTypeMapping(t);
			//Debug.Print(tm);

			return tm;
		}

		private XmlTypeMapping Map(Type t, XmlRootAttribute root)
		{
			XmlReflectionImporter ri = new XmlReflectionImporter();
			XmlTypeMapping tm = ri.ImportTypeMapping(t, root);

			return tm;
		}

		private XmlTypeMapping Map(Type t, string ns)
		{
			XmlReflectionImporter ri = new XmlReflectionImporter(ns);
			XmlTypeMapping tm = ri.ImportTypeMapping(t);
			//Debug.Print(tm);

			return tm;
		}

		private XmlTypeMapping Map(Type t, XmlAttributeOverrides overrides)
		{
			XmlReflectionImporter ri = new XmlReflectionImporter(overrides);
			XmlTypeMapping tm = ri.ImportTypeMapping(t);
			//Debug.Print(tm);

			return tm;
		}

		private XmlMembersMapping MembersMap(Type t, XmlAttributeOverrides overrides, 
			XmlReflectionMember [] members, bool inContainer)
		{
			XmlReflectionImporter ri = new XmlReflectionImporter(overrides);
			XmlMembersMapping mm = ri.ImportMembersMapping(null, null, members, inContainer);
			
			return mm;
		}
		
		[Test]
		public void TestIntTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(int));
			AssertEquals("int", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Int32", tm.TypeName);
			AssertEquals("System.Int32", tm.TypeFullName);
		}

		[Test]
		public void TestIntArrayTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(int[]));
			AssertEquals("ArrayOfInt", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Int32[]", tm.TypeName);
			AssertEquals("System.Int32[]", tm.TypeFullName);
		}

		[Test]
		public void TestStringTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(string));
			AssertEquals("string", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("String", tm.TypeName);
			AssertEquals("System.String", tm.TypeFullName);
		}

		[Test]
		public void TestObjectTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(object));
			AssertEquals("anyType", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Object", tm.TypeName);
			AssertEquals("System.Object", tm.TypeFullName);
		}

		[Test]
		public void TestByteTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(byte));
			AssertEquals("unsignedByte", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Byte", tm.TypeName);
			AssertEquals("System.Byte", tm.TypeFullName);
		}

		[Test]
		public void TestByteArrayTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(byte[]));
			AssertEquals("base64Binary", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Byte[]", tm.TypeName);
			AssertEquals("System.Byte[]", tm.TypeFullName);
		}

		[Test]
		public void TestBoolTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(bool));
			AssertEquals("boolean", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Boolean", tm.TypeName);
			AssertEquals("System.Boolean", tm.TypeFullName);
		}

		[Test]
		public void TestShortTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(short));
			AssertEquals("short", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Int16", tm.TypeName);
			AssertEquals("System.Int16", tm.TypeFullName);
		}

		[Test]
		public void TestUnsignedShortTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(ushort));
			AssertEquals("unsignedShort", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("UInt16", tm.TypeName);
			AssertEquals("System.UInt16", tm.TypeFullName);
		}
		
		[Test]
		public void TestUIntTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(uint));
			AssertEquals("unsignedInt", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("UInt32", tm.TypeName);
			AssertEquals("System.UInt32", tm.TypeFullName);
		}
		
		[Test]
		public void TestLongTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(long));
			AssertEquals("long", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Int64", tm.TypeName);
			AssertEquals("System.Int64", tm.TypeFullName);
		}
		
		[Test]
		public void TestULongTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(ulong));
			AssertEquals("unsignedLong", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("UInt64", tm.TypeName);
			AssertEquals("System.UInt64", tm.TypeFullName);
		}
		
		[Test]
		public void TestFloatTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(float));
			AssertEquals("float", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Single", tm.TypeName);
			AssertEquals("System.Single", tm.TypeFullName);
		}
		
		[Test]
		public void TestDoubleTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(double));
			AssertEquals("double", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Double", tm.TypeName);
			AssertEquals("System.Double", tm.TypeFullName);
		}
		
		[Test]
		public void TestDateTimeTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(DateTime));
			AssertEquals("dateTime", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("DateTime", tm.TypeName);
			AssertEquals("System.DateTime", tm.TypeFullName);
		}
		
		[Test]
		public void TestGuidTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(Guid));
			AssertEquals("guid", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Guid", tm.TypeName);
			AssertEquals("System.Guid", tm.TypeFullName);
		}
		
		[Test]
		public void TestDecimalTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(decimal));
			AssertEquals("decimal", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Decimal", tm.TypeName);
			AssertEquals("System.Decimal", tm.TypeFullName);
		}
		
		[Test]
		public void TestXmlQualifiedNameTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(XmlQualifiedName));
			AssertEquals("QName", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("XmlQualifiedName", tm.TypeName);
			AssertEquals("System.Xml.XmlQualifiedName", tm.TypeFullName);
		}
		
		[Test]
		public void TestSByteTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(sbyte));
			AssertEquals("byte", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("SByte", tm.TypeName);
			AssertEquals("System.SByte", tm.TypeFullName);
		}
		

		[Test]
		public void TestCharTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(char));
			AssertEquals("char", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("Char", tm.TypeName);
			AssertEquals("System.Char", tm.TypeFullName);
		}

		[Test]
		public void TestNullTypeMapping()
		{
			try
			{
				XmlTypeMapping tm = Map(null);
				Fail("Should not be able to map a null type");
			}
			catch (Exception)
			{
			}
		}

	    
		[Test]
		public void TestInvalidClassTypeMapping()
		{
			try
			{
				// this can use any class
				XmlTypeMapping tm = Map(typeof(SimpleClass));
				Fail("Should not be able to this type");
			}
			catch (Exception)
			{
			}
		}

		/*
		[Test]
		public void TestTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof());
			AssertEquals(tm.ElementName, "");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "");
			AssertEquals(tm.TypeFullName, "System.");
		}
		*/
	
		[Test]
		public void TestIntTypeMappingWithDefaultNamespaces()
		{
			XmlTypeMapping tm = Map(typeof(int), SomeNamespace);
			AssertEquals("int", tm.ElementName);
			AssertEquals(SomeNamespace, tm.Namespace);
			AssertEquals("Int32", tm.TypeName);
			AssertEquals("System.Int32", tm.TypeFullName);
		}

		[Test]
		public void TestValidClassTypeMapping()
		{
			Type type = typeof(SimpleClass);
			XmlAttributes attrs = new  XmlAttributes();
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			overrides.Add(typeof(SimpleClass), attrs);
			
			XmlTypeMapping tm = Map(type, overrides);
			AssertEquals("SimpleClass", tm.ElementName);
			AssertEquals("", tm.Namespace);
			AssertEquals("SimpleClass", tm.TypeName);
			AssertEquals("MonoTests.System.Xml.TestClasses.SimpleClass", tm.TypeFullName);
		}

		
		[Test]
		public void TestImportMembersMapping()
		{
			Type type = typeof(SimpleClass);
			XmlAttributes attrs = new  XmlAttributes();
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			overrides.Add(typeof(SimpleClass), attrs);

			XmlReflectionMember[] members = new XmlReflectionMember[0];
			XmlMembersMapping mm;
			try
			{
				mm = MembersMap(type, overrides, members, true);
				Fail("Should not be able to fetch an empty XmlMembersMapping");
			}
			catch (Exception)
			{
			}
			
			XmlReflectionMember rm = new XmlReflectionMember();
			rm.IsReturnValue = false;
			rm.MemberName = "something";
			rm.MemberType = typeof(string);
			members = new XmlReflectionMember[1];
			members[0] = rm;

			mm = MembersMap(type, overrides, members, false);

			Equals(mm.Count, 1);

			XmlMemberMapping smm = mm[0];
			AssertEquals(false, smm.Any);
			AssertEquals("something", smm.ElementName);
			AssertEquals("something", smm.MemberName);
			AssertEquals(null, smm.Namespace);
			AssertEquals("System.String", smm.TypeFullName);
			AssertEquals("string", smm.TypeName);
			AssertEquals(null, smm.TypeNamespace);

			
			rm = new XmlReflectionMember();
			rm.IsReturnValue = false;
			rm.MemberName = "nothing";
			rm.MemberType = typeof(string);
			members = new XmlReflectionMember[1];
			members[0] = rm;

			mm = MembersMap(type, overrides, members, false);
			Equals(mm.Count, 0);
		}

		[Test]
		public void TestIntTypeMappingWithXmlRootAttribute()
		{
			const string TheNamespace = "another:urn";
			XmlRootAttribute root = new XmlRootAttribute("price");
			root.Namespace = TheNamespace;
			
			XmlTypeMapping tm = Map(typeof(int), root);
			AssertEquals("price", tm.ElementName);
			AssertEquals(TheNamespace, tm.Namespace);
			AssertEquals("Int32", tm.TypeName);
			AssertEquals("System.Int32", tm.TypeFullName);
		}

	}
}

