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
	public class XmlReflectionImporterTests
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
			Assertion.AssertEquals(tm.ElementName, "int");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Int32");
			Assertion.AssertEquals(tm.TypeFullName, "System.Int32");
		}

		[Test]
		public void TestIntArrayTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(int[]));
			Assertion.AssertEquals(tm.ElementName, "ArrayOfInt");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Int32[]");
			Assertion.AssertEquals(tm.TypeFullName, "System.Int32[]");
		}

		[Test]
		public void TestStringTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(string));
			Assertion.AssertEquals(tm.ElementName, "string");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "String");
			Assertion.AssertEquals(tm.TypeFullName, "System.String");
		}

		[Test]
		public void TestObjectTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(object));
			Assertion.AssertEquals(tm.ElementName, "anyType");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Object");
			Assertion.AssertEquals(tm.TypeFullName, "System.Object");
		}

		[Test]
		public void TestByteTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(byte));
			Assertion.AssertEquals(tm.ElementName, "unsignedByte");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Byte");
			Assertion.AssertEquals(tm.TypeFullName, "System.Byte");
		}

		[Test]
		public void TestByteArrayTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(byte[]));
			Assertion.AssertEquals(tm.ElementName, "base64Binary");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Byte[]");
			Assertion.AssertEquals(tm.TypeFullName, "System.Byte[]");
		}

		[Test]
		public void TestBoolTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(bool));
			Assertion.AssertEquals(tm.ElementName, "boolean");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Boolean");
			Assertion.AssertEquals(tm.TypeFullName, "System.Boolean");
		}

		[Test]
		public void TestShortTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(short));
			Assertion.AssertEquals(tm.ElementName, "short");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Int16");
			Assertion.AssertEquals(tm.TypeFullName, "System.Int16");
		}

		[Test]
		public void TestUnsignedShortTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(ushort));
			Assertion.AssertEquals(tm.ElementName, "unsignedShort");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "UInt16");
			Assertion.AssertEquals(tm.TypeFullName, "System.UInt16");
		}
		
		[Test]
		public void TestUIntTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(uint));
			Assertion.AssertEquals(tm.ElementName, "unsignedInt");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "UInt32");
			Assertion.AssertEquals(tm.TypeFullName, "System.UInt32");
		}
		
		[Test]
		public void TestLongTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(long));
			Assertion.AssertEquals(tm.ElementName, "long");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Int64");
			Assertion.AssertEquals(tm.TypeFullName, "System.Int64");
		}
		
		[Test]
		public void TestULongTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(ulong));
			Assertion.AssertEquals(tm.ElementName, "unsignedLong");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "UInt64");
			Assertion.AssertEquals(tm.TypeFullName, "System.UInt64");
		}
		
		[Test]
		public void TestFloatTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(float));
			Assertion.AssertEquals(tm.ElementName, "float");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Single");
			Assertion.AssertEquals(tm.TypeFullName, "System.Single");
		}
		
		[Test]
		public void TestDoubleTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(double));
			Assertion.AssertEquals(tm.ElementName, "double");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Double");
			Assertion.AssertEquals(tm.TypeFullName, "System.Double");
		}
		
		[Test]
		public void TestDateTimeTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(DateTime));
			Assertion.AssertEquals(tm.ElementName, "dateTime");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "DateTime");
			Assertion.AssertEquals(tm.TypeFullName, "System.DateTime");
		}
		
		[Test]
		public void TestGuidTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(Guid));
			Assertion.AssertEquals(tm.ElementName, "guid");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Guid");
			Assertion.AssertEquals(tm.TypeFullName, "System.Guid");
		}
		
		[Test]
		public void TestDecimalTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(decimal));
			Assertion.AssertEquals(tm.ElementName, "decimal");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Decimal");
			Assertion.AssertEquals(tm.TypeFullName, "System.Decimal");
		}
		
		[Test]
		public void TestXmlQualifiedNameTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(XmlQualifiedName));
			Assertion.AssertEquals(tm.ElementName, "QName");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "XmlQualifiedName");
			Assertion.AssertEquals(tm.TypeFullName, "System.Xml.XmlQualifiedName");
		}
		
		[Test]
		public void TestSByteTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(sbyte));
			Assertion.AssertEquals(tm.ElementName, "byte");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "SByte");
			Assertion.AssertEquals(tm.TypeFullName, "System.SByte");
		}
		

		[Test]
		public void TestCharTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(char));
			Assertion.AssertEquals(tm.ElementName, "char");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "Char");
			Assertion.AssertEquals(tm.TypeFullName, "System.Char");
		}

		[Test]
		public void TestNullTypeMapping()
		{
			try
			{
				XmlTypeMapping tm = Map(null);
				Assertion.Fail("Should not be able to map a null type");
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
				Assertion.Fail("Should not be able to this type");
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
			Assertion.AssertEquals(tm.ElementName, "");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "");
			Assertion.AssertEquals(tm.TypeFullName, "System.");
		}
		*/
	
		[Test]
		public void TestIntTypeMappingWithDefaultNamespaces()
		{
			XmlTypeMapping tm = Map(typeof(int), SomeNamespace);
			Assertion.AssertEquals(tm.ElementName, "int");
			Assertion.AssertEquals(tm.Namespace, SomeNamespace);
			Assertion.AssertEquals(tm.TypeName, "Int32");
			Assertion.AssertEquals(tm.TypeFullName, "System.Int32");
		}

		[Test]
		public void TestValidClassTypeMapping()
		{
			Type type = typeof(SimpleClass);
			XmlAttributes attrs = new  XmlAttributes();
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			overrides.Add(typeof(SimpleClass), attrs);
			
			XmlTypeMapping tm = Map(type, overrides);
			Assertion.AssertEquals(tm.ElementName, "SimpleClass");
			Assertion.AssertEquals(tm.Namespace, "");
			Assertion.AssertEquals(tm.TypeName, "SimpleClass");
			Assertion.AssertEquals(tm.TypeFullName, "MonoTests.System.Xml.TestClasses.SimpleClass");
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
				Assertion.Fail("Should not be able to fetch an empty XmlMembersMapping");
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

			Assertion.Equals(mm.Count, 1);

			XmlMemberMapping smm = mm[0];
			Assertion.AssertEquals(smm.Any, false);
			Assertion.AssertEquals(smm.ElementName, "something");
			Assertion.AssertEquals(smm.MemberName, "something");
			Assertion.AssertEquals(smm.Namespace, null);
			Assertion.AssertEquals(smm.TypeFullName, "System.String");
			Assertion.AssertEquals(smm.TypeName, "string");
			Assertion.AssertEquals(smm.TypeNamespace, null);

			
			rm = new XmlReflectionMember();
			rm.IsReturnValue = false;
			rm.MemberName = "nothing";
			rm.MemberType = typeof(string);
			members = new XmlReflectionMember[1];
			members[0] = rm;

			mm = MembersMap(type, overrides, members, false);
			Assertion.Equals(mm.Count, 0);
		}

		[Test]
		public void TestIntTypeMappingWithXmlRootAttribute()
		{
			const string TheNamespace = "another:urn";
			XmlRootAttribute root = new XmlRootAttribute("price");
			root.Namespace = TheNamespace;
			
			XmlTypeMapping tm = Map(typeof(int), root);
			Assertion.AssertEquals(tm.ElementName, "price");
			Assertion.AssertEquals(tm.Namespace, TheNamespace);
			Assertion.AssertEquals(tm.TypeName, "Int32");
			Assertion.AssertEquals(tm.TypeFullName, "System.Int32");
		}

	}
}

