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

namespace MonoTests.System.Xml.Serialization
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
			AssertEquals(tm.ElementName, "int");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Int32");
			AssertEquals(tm.TypeFullName, "System.Int32");
		}

		[Test]
		public void TestIntArrayTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(int[]));
			AssertEquals(tm.ElementName, "ArrayOfInt");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Int32[]");
			AssertEquals(tm.TypeFullName, "System.Int32[]");
		}

		[Test]
		public void TestStringTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(string));
			AssertEquals(tm.ElementName, "string");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "String");
			AssertEquals(tm.TypeFullName, "System.String");
		}

		[Test]
		public void TestObjectTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(object));
			AssertEquals(tm.ElementName, "anyType");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Object");
			AssertEquals(tm.TypeFullName, "System.Object");
		}

		[Test]
		public void TestByteTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(byte));
			AssertEquals(tm.ElementName, "unsignedByte");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Byte");
			AssertEquals(tm.TypeFullName, "System.Byte");
		}

		[Test]
		public void TestByteArrayTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(byte[]));
			AssertEquals(tm.ElementName, "base64Binary");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Byte[]");
			AssertEquals(tm.TypeFullName, "System.Byte[]");
		}

		[Test]
		public void TestBoolTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(bool));
			AssertEquals(tm.ElementName, "boolean");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Boolean");
			AssertEquals(tm.TypeFullName, "System.Boolean");
		}

		[Test]
		public void TestShortTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(short));
			AssertEquals(tm.ElementName, "short");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Int16");
			AssertEquals(tm.TypeFullName, "System.Int16");
		}

		[Test]
		public void TestUnsignedShortTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(ushort));
			AssertEquals(tm.ElementName, "unsignedShort");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "UInt16");
			AssertEquals(tm.TypeFullName, "System.UInt16");
		}
		
		[Test]
		public void TestUIntTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(uint));
			AssertEquals(tm.ElementName, "unsignedInt");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "UInt32");
			AssertEquals(tm.TypeFullName, "System.UInt32");
		}
		
		[Test]
		public void TestLongTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(long));
			AssertEquals(tm.ElementName, "long");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Int64");
			AssertEquals(tm.TypeFullName, "System.Int64");
		}
		
		[Test]
		public void TestULongTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(ulong));
			AssertEquals(tm.ElementName, "unsignedLong");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "UInt64");
			AssertEquals(tm.TypeFullName, "System.UInt64");
		}
		
		[Test]
		public void TestFloatTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(float));
			AssertEquals(tm.ElementName, "float");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Single");
			AssertEquals(tm.TypeFullName, "System.Single");
		}
		
		[Test]
		public void TestDoubleTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(double));
			AssertEquals(tm.ElementName, "double");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Double");
			AssertEquals(tm.TypeFullName, "System.Double");
		}
		
		[Test]
		public void TestDateTimeTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(DateTime));
			AssertEquals(tm.ElementName, "dateTime");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "DateTime");
			AssertEquals(tm.TypeFullName, "System.DateTime");
		}
		
		[Test]
		public void TestGuidTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(Guid));
			AssertEquals(tm.ElementName, "guid");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Guid");
			AssertEquals(tm.TypeFullName, "System.Guid");
		}
		
		[Test]
		public void TestDecimalTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(decimal));
			AssertEquals(tm.ElementName, "decimal");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Decimal");
			AssertEquals(tm.TypeFullName, "System.Decimal");
		}
		
		[Test]
		public void TestXmlQualifiedNameTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(XmlQualifiedName));
			AssertEquals(tm.ElementName, "QName");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "XmlQualifiedName");
			AssertEquals(tm.TypeFullName, "System.Xml.XmlQualifiedName");
		}
		
		[Test]
		public void TestSByteTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(sbyte));
			AssertEquals(tm.ElementName, "byte");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "SByte");
			AssertEquals(tm.TypeFullName, "System.SByte");
		}
		

		[Test]
		public void TestCharTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(char));
			AssertEquals(tm.ElementName, "char");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "Char");
			AssertEquals(tm.TypeFullName, "System.Char");
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
			AssertEquals(tm.ElementName, "int");
			AssertEquals(tm.Namespace, SomeNamespace);
			AssertEquals(tm.TypeName, "Int32");
			AssertEquals(tm.TypeFullName, "System.Int32");
		}

		[Test]
		public void TestValidClassTypeMapping()
		{
			Type type = typeof(SimpleClass);
			XmlAttributes attrs = new  XmlAttributes();
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			overrides.Add(typeof(SimpleClass), attrs);
			
			XmlTypeMapping tm = Map(type, overrides);
			AssertEquals(tm.ElementName, "SimpleClass");
			AssertEquals(tm.Namespace, "");
			AssertEquals(tm.TypeName, "SimpleClass");
			AssertEquals(tm.TypeFullName, "MonoTests.System.Xml.TestClasses.SimpleClass");
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
			AssertEquals(smm.Any, false);
			AssertEquals(smm.ElementName, "something");
			AssertEquals(smm.MemberName, "something");
			AssertEquals(smm.Namespace, null);
			AssertEquals(smm.TypeFullName, "System.String");
			AssertEquals(smm.TypeName, "string");
			AssertEquals(smm.TypeNamespace, null);

			
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
			AssertEquals(tm.ElementName, "price");
			AssertEquals(tm.Namespace, TheNamespace);
			AssertEquals(tm.TypeName, "Int32");
			AssertEquals(tm.TypeFullName, "System.Int32");
		}

	}
}

