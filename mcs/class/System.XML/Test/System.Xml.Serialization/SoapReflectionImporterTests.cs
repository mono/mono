//
// System.Xml.Serialization.SoapReflectionImporterTests
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005 Novell
// 

using System;
using System.Collections;
using DefaultValueAttribute = System.ComponentModel.DefaultValueAttribute;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class SoapReflectionImporterTests
	{
		private const string SomeNamespace = "some:urn";
		private const string AnotherNamespace = "another:urn";
		private const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";
		private const string WsdlTypesNamespace = "http://microsoft.com/wsdl/types/";

		// these Map methods re-create the SoapReflectionImporter at every call.

		private XmlTypeMapping Map (Type t)
		{
			SoapReflectionImporter ri = new SoapReflectionImporter ();
			XmlTypeMapping tm = ri.ImportTypeMapping (t);

			return tm;
		}

		private XmlTypeMapping Map (Type t, string ns)
		{
			SoapReflectionImporter ri = new SoapReflectionImporter (ns);
			XmlTypeMapping tm = ri.ImportTypeMapping (t);

			return tm;
		}

		private XmlTypeMapping Map (Type t, SoapAttributeOverrides overrides)
		{
			SoapReflectionImporter ri = new SoapReflectionImporter(overrides);
			XmlTypeMapping tm = ri.ImportTypeMapping(t);

			return tm;
		}

		private XmlMembersMapping MembersMap (Type t, SoapAttributeOverrides overrides, 
			XmlReflectionMember [] members, bool inContainer, bool writeAccessors)
		{
			SoapReflectionImporter ri = new SoapReflectionImporter (overrides);
			XmlMembersMapping mm = ri.ImportMembersMapping (null, null, members, 
				inContainer, writeAccessors);
			
			return mm;
		}
		
		[Test]
		public void TestIntTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (int));
			Assert.AreEqual ("int", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Int32", tm.TypeName, "#3");
			Assert.AreEqual ("System.Int32", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestIntTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (int), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (int[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (int[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestIntTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (int[]));
			Assert.AreEqual ("ArrayOfInt", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfInt32", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("Int32[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("System.Int32[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (int[][]));
			Assert.AreEqual ("ArrayOfArrayOfInt", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfInt32", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("Int32[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.Int32[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		public void TestStringTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (string));
			Assert.AreEqual ("string", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("String", tm.TypeName, "#3");
			Assert.AreEqual ("System.String", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestStringTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (string), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (string[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (string[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestStringTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (string[]));
			Assert.AreEqual ("ArrayOfString", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfString", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("String[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("System.String[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (string[][]));
			Assert.AreEqual ("ArrayOfArrayOfString", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfString", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("String[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.String[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		public void TestObjectTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (object));
			Assert.AreEqual ("anyType", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Object", tm.TypeName, "#3");
			Assert.AreEqual ("System.Object", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestObjectTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (object), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (object[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (object[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestObjectTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (object[]));
			Assert.AreEqual ("ArrayOfAnyType", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfObject", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("Object[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("System.Object[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (object[][]));
			Assert.AreEqual ("ArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfObject", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("Object[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.Object[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		public void TestByteTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (byte));
			Assert.AreEqual ("unsignedByte", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Byte", tm.TypeName, "#3");
			Assert.AreEqual ("System.Byte", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestByteTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (byte), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (byte[]), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");

			tm = Map (typeof (byte[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestByteTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (byte[]));
			Assert.AreEqual ("base64Binary", tm.ElementName, "#A1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#A2");
			Assert.AreEqual ("Byte[]", tm.TypeName, "#A3");
			Assert.AreEqual ("System.Byte[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (byte[][]));
			Assert.AreEqual ("ArrayOfBase64Binary", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfByte", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("Byte[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.Byte[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		public void TestBoolTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (bool));
			Assert.AreEqual ("boolean", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Boolean", tm.TypeName, "#3");
			Assert.AreEqual ("System.Boolean", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestBoolTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (bool), "http://somenamespace");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (bool[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (bool[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		public void TestShortTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (short));
			Assert.AreEqual ("short", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Int16", tm.TypeName, "#3");
			Assert.AreEqual ("System.Int16", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestShortTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (short), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (short[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (short[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		public void TestUnsignedShortTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (ushort));
			Assert.AreEqual ("unsignedShort", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("UInt16", tm.TypeName, "#3");
			Assert.AreEqual ("System.UInt16", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestUnsignedShortTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (ushort), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (ushort[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (ushort[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		public void TestUIntTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (uint));
			Assert.AreEqual ("unsignedInt", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("UInt32", tm.TypeName, "#3");
			Assert.AreEqual ("System.UInt32", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestUIntTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (uint), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (uint[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (uint[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		public void TestLongTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (long));
			Assert.AreEqual ("long", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Int64", tm.TypeName, "#3");
			Assert.AreEqual ("System.Int64", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestLongTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (long), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (long[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (long[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		public void TestULongTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (ulong));
			Assert.AreEqual ("unsignedLong", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("UInt64", tm.TypeName, "#3");
			Assert.AreEqual ("System.UInt64", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestULongTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (ulong), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (ulong[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (ulong[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		public void TestFloatTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (float));
			Assert.AreEqual ("float", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Single", tm.TypeName, "#3");
			Assert.AreEqual ("System.Single", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestFloatTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (float), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (float[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (float[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		public void TestDoubleTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (double));
			Assert.AreEqual ("double", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Double", tm.TypeName, "#3");
			Assert.AreEqual ("System.Double", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestDoubleTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (double), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (double[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (double[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		public void TestDateTimeTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (DateTime));
			Assert.AreEqual ("dateTime", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("DateTime", tm.TypeName, "#3");
			Assert.AreEqual ("System.DateTime", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestDateTimeTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (DateTime), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (DateTime[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (DateTime[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestDateTimeTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (DateTime[]));
			Assert.AreEqual ("ArrayOfDateTime", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfDateTime", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("DateTime[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("System.DateTime[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (DateTime[][]));
			Assert.AreEqual ("ArrayOfArrayOfDateTime", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfDateTime", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("DateTime[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.DateTime[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		public void TestGuidTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (Guid));
			Assert.AreEqual ("guid", tm.ElementName, "#1");
			Assert.AreEqual (WsdlTypesNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Guid", tm.TypeName, "#3");
			Assert.AreEqual ("System.Guid", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestGuidTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (Guid), SomeNamespace);
			Assert.AreEqual (WsdlTypesNamespace, tm.Namespace, "#1");

			tm = Map (typeof (Guid[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (Guid[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestGuidTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (Guid[]));
			Assert.AreEqual ("ArrayOfGuid", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfGuid", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("Guid[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("System.Guid[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (Guid[][]));
			Assert.AreEqual ("ArrayOfArrayOfGuid", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfGuid", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("Guid[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.Guid[][]", tm.TypeFullName, "#B4");
		}
		
		[Test]
		public void TestDecimalTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (decimal));
			Assert.AreEqual ("decimal", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Decimal", tm.TypeName, "#3");
			Assert.AreEqual ("System.Decimal", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestDecimalTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (decimal), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (decimal[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (decimal[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		public void TestXmlQualifiedNameTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (XmlQualifiedName));
			Assert.AreEqual ("QName", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("XmlQualifiedName", tm.TypeName, "#3");
			Assert.AreEqual ("System.Xml.XmlQualifiedName", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestXmlQualifiedNameTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (XmlQualifiedName), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (XmlQualifiedName[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (XmlQualifiedName[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestXmlQualifiedNameTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (XmlQualifiedName[]));
			Assert.AreEqual ("ArrayOfQName", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfXmlQualifiedName", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("XmlQualifiedName[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("System.Xml.XmlQualifiedName[]", tm.TypeFullName, "#A4");

			tm  = Map (typeof (XmlQualifiedName[][]));
			Assert.AreEqual ("ArrayOfArrayOfQName", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfXmlQualifiedName", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("XmlQualifiedName[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.Xml.XmlQualifiedName[][]", tm.TypeFullName, "#B4");
		}
		
		[Test]
		public void TestSByteTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (sbyte));
			Assert.AreEqual ("byte", tm.ElementName, "#1");
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("SByte", tm.TypeName, "#3");
			Assert.AreEqual ("System.SByte", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestSByteTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (sbyte), SomeNamespace);
			Assert.AreEqual (XmlSchemaNamespace, tm.Namespace, "#1");

			tm = Map (typeof (sbyte[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (sbyte[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestSByteTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (sbyte[]));
			Assert.AreEqual ("ArrayOfByte", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfSByte", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("SByte[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("System.SByte[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (sbyte[][]));
			Assert.AreEqual ("ArrayOfArrayOfByte", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfSByte", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("SByte[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.SByte[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		public void TestCharTypeMapping()
		{
			XmlTypeMapping tm = Map (typeof (char));
			Assert.AreEqual ("char", tm.ElementName, "#1");
			Assert.AreEqual (WsdlTypesNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Char", tm.TypeName, "#3");
			Assert.AreEqual ("System.Char", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestCharTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (char), SomeNamespace);
			Assert.AreEqual (WsdlTypesNamespace, tm.Namespace, "#1");

			tm = Map (typeof (char[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (char[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestCharTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (char[]));
			Assert.AreEqual ("ArrayOfChar", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfChar", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("Char[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("System.Char[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (char[][]));
			Assert.AreEqual ("ArrayOfArrayOfChar", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfChar", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("Char[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.Char[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		public void TestXmlNodeTypeMapping ()
		{
			try {
				Map (typeof (XmlNode));
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNode may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlNode[]));
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNode may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlNode[][]));
				Assert.Fail ("#3");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNode may not be serialized with SOAP-encoded messages.
			}
		}

		[Test]
		public void TestXmlNodeTypeMapping_DefaultNamespace ()
		{
			try {
				Map (typeof (XmlNode), SomeNamespace);
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNode may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlNode[]), SomeNamespace);
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNode may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlNode[][]), SomeNamespace);
				Assert.Fail ("#3");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNode may not be serialized with SOAP-encoded messages.
			}
		}

		[Test]
		public void TestXmlElementTypeMapping ()
		{
			try {
				Map (typeof (XmlElement));
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlElement may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlElement[]));
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlElement may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlElement[][]));
				Assert.Fail ("#3");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlElement may not be serialized with SOAP-encoded messages.
			}
		}

		[Test]
		public void TestXmlElementTypeMapping_DefaultNamespace ()
		{
			try {
				Map (typeof (XmlElement), SomeNamespace);
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlElement may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlElement[]), SomeNamespace);
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlElement may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlElement[][]), SomeNamespace);
				Assert.Fail ("#3");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlElement may not be serialized with SOAP-encoded messages.
			}
		}

		[Test]
		public void TestXmlNotationTypeMapping ()
		{
			try {
				Map (typeof (XmlNotation));
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNotation may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlNotation[]));
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNotation may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlNotation[][]));
				Assert.Fail ("#3");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNotation may not be serialized with SOAP-encoded messages.
			}
		}

		[Test]
		public void TestXmlNotationTypeMapping_DefaultNamespace ()
		{
			try {
				Map (typeof (XmlNotation), SomeNamespace);
				Assert.Fail ("#1");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNotation may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlNotation[]), SomeNamespace);
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNotation may not be serialized with SOAP-encoded messages.
			}

			try {
				Map (typeof (XmlNotation[][]), SomeNamespace);
				Assert.Fail ("#3");
			} catch (NotSupportedException) {
				// The type System.Xml.XmlNotation may not be serialized with SOAP-encoded messages.
			}
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestXmlSerializableTypeMapping ()
		{
			Map (typeof (Employee));
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestClassTypeMapping_NestedStruct ()
		{
			Map (typeof (NestedStruct));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestNullTypeMapping()
		{
			Map(null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestNullTypeMapping_DefaultNamespace ()
		{
			Map (null, SomeNamespace);
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestStructTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (TimeSpan));
			Assert.AreEqual ("TimeSpan", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("TimeSpan", tm.TypeName, "#3");
			Assert.AreEqual ("System.TimeSpan", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestStructTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (TimeSpan), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#1");

			try {
				tm = Map (typeof (TimeSpan[]), SomeNamespace);
				Assert.Fail ("#2");
			} catch (NotSupportedException) {
			}

			try {
				tm = Map (typeof (TimeSpan[][]), SomeNamespace);
				Assert.Fail ("#3");
			} catch (NotSupportedException) {
			}
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		[ExpectedException (typeof (NotSupportedException))] // Arrays of structs are not supported with encoded SOAP.
		public void TestStructTypeMapping_Array ()
		{
			Map (typeof (TimeSpan[]));
		}

		[Test]
		public void TestEnumTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (AttributeTargets));
			Assert.AreEqual ("AttributeTargets", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("AttributeTargets", tm.TypeName, "#3");
			Assert.AreEqual ("System.AttributeTargets", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestEnumTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (AttributeTargets), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#1");

			tm = Map (typeof (AttributeTargets[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (AttributeTargets[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestEnumTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (AttributeTargets[]));
			Assert.AreEqual ("ArrayOfAttributeTargets", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfAttributeTargets", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("AttributeTargets[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("System.AttributeTargets[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (AttributeTargets[][]));
			Assert.AreEqual ("ArrayOfArrayOfAttributeTargets", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfAttributeTargets", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("AttributeTargets[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.AttributeTargets[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		public void TestClassTypeMapping()
		{
			XmlTypeMapping tm = Map (typeof (SimpleClass));
			Assert.AreEqual ("SimpleClass", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("SimpleClass", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.SimpleClass", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestClassTypeMapping_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (SimpleClass), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#1");

			tm = Map (typeof (SimpleClass[]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");

			tm = Map (typeof (SimpleClass[][]), SomeNamespace);
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestClassTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (SimpleClass[]));
			Assert.AreEqual ("ArrayOfSimpleClass", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfSimpleClass", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("SimpleClass[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.SimpleClass[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClass[][]));
			Assert.AreEqual ("ArrayOfArrayOfSimpleClass", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfSimpleClass", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("SimpleClass[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.SimpleClass[][]", tm.TypeFullName, "#B4");
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")] // wrong error message is reported in .NET 1.1
#endif
		public void TypeMapping_Attribute_ComplexType ()
		{
			SoapAttributes attrs = new SoapAttributes (typeof (Field_Encoded).GetMember ("Names")[0]);
			attrs.SoapAttribute = new SoapAttributeAttribute (); // SoapAttribute cannot be used to encode complex types
			SoapAttributeOverrides overrides = new SoapAttributeOverrides ();
			overrides.Add (typeof (Field_Encoded), "Names", attrs);

			try {
				Map (typeof (Field_Encoded), overrides);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Cannot serialize member 'Names' of type System.String[].
				// SoapAttribute cannot be used to encode complex types.
				Assert.IsNotNull (ex.Message, "#A2");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string[]).FullName) != -1, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("SoapAttribute") != -1, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
			}
		}

		[Test]
		public void TypeMapping_Field ()
		{
			XmlTypeMapping tm = Map (typeof (Field));
			Assert.AreEqual ("Field", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Field", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.Field", tm.TypeFullName, "#4");
		}

		[Test]
		public void TypeMapping_Field_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (Field), SomeNamespace);
			Assert.AreEqual ("Field", tm.ElementName, "#1");
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Field", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.Field", tm.TypeFullName, "#4");
		}

		[Test]
		public void TypeMapping_Field_Encoded ()
		{
			XmlTypeMapping tm = Map (typeof (Field_Encoded));
			Assert.AreEqual ("field", tm.ElementName, "#1");
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Field_Encoded", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.Field_Encoded", tm.TypeFullName, "#4");
		}

		[Test]
		public void TypeMapping_Field_Encoded_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (Field_Encoded), AnotherNamespace);
			Assert.AreEqual ("field", tm.ElementName, "#1");
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Field_Encoded", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.Field_Encoded", tm.TypeFullName, "#4");
		}

		[Test]
		public void TypeMapping_FlagEnum ()
		{
			XmlTypeMapping tm = Map (typeof (FlagEnum));
			Assert.AreEqual ("FlagEnum", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("FlagEnum", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.FlagEnum", tm.TypeFullName, "#4");
		}

		[Test]
		public void TypeMapping_FlagEnum_DefaultNamespace ()
		{
			XmlTypeMapping tm = Map (typeof (FlagEnum), SomeNamespace);
			Assert.AreEqual ("FlagEnum", tm.ElementName, "#1");
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("FlagEnum", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.FlagEnum", tm.TypeFullName, "#4");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeMapping_IDictionary ()
		{
			// The type MonoTests.System.Xml.TestClasses.DictionaryWithIndexer 
			// is not supported because it implements IDictionary.
			Map (typeof (DictionaryWithIndexer));
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TypeMapping_IEnumerable_SimpleClass ()
		{
			XmlTypeMapping tm = Map (typeof (SimpleClassEnumerable));
			Assert.AreEqual ("ArrayOfSimpleClass", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("SimpleClassEnumerable", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassEnumerable", tm.TypeFullName, "#4");

			tm = Map (typeof (SimpleClassEnumerable[]));
			Assert.AreEqual ("ArrayOfArrayOfSimpleClass", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfSimpleClassEnumerable", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("SimpleClassEnumerable[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassEnumerable[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassEnumerable[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassEnumerable", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("SimpleClassEnumerable[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassEnumerable[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TypeMapping_IEnumerable_Object ()
		{
			XmlTypeMapping tm = Map (typeof (ObjectEnumerable));
			Assert.AreEqual ("ArrayOfAnyType", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("ObjectEnumerable", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.ObjectEnumerable", tm.TypeFullName, "#4");

			tm = Map (typeof (ObjectEnumerable[]));
			Assert.AreEqual ("ArrayOfArrayOfAnyType", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfObjectEnumerable", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("ObjectEnumerable[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.ObjectEnumerable[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (ObjectEnumerable[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfObjectEnumerable", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("ObjectEnumerable[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.ObjectEnumerable[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_IEnumerable_Object_NoMatchingAddMethod ()
		{
			Map (typeof (ObjectEnumerableNoMatchingAddMethod));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_IEnumerable_Object_NoMatchingAddMethod_Array ()
		{
			Map (typeof (ObjectEnumerableNoMatchingAddMethod[]));
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TypeMapping_IEnumerable_SimpleClass_PrivateCurrent ()
		{
			XmlTypeMapping tm = Map (typeof (SimpleClassEnumerablePrivateCurrent));
			Assert.AreEqual ("ArrayOfAnyType", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("SimpleClassEnumerablePrivateCurrent", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassEnumerablePrivateCurrent", tm.TypeFullName, "#4");

			tm = Map (typeof (SimpleClassEnumerablePrivateCurrent[]));
			Assert.AreEqual ("ArrayOfArrayOfAnyType", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfSimpleClassEnumerablePrivateCurrent", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("SimpleClassEnumerablePrivateCurrent[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassEnumerablePrivateCurrent[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassEnumerablePrivateCurrent[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassEnumerablePrivateCurrent", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("SimpleClassEnumerablePrivateCurrent[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassEnumerablePrivateCurrent[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
#if ONLY_1_1
		[Category ("NotDotNet")] // results in NullReferenceException in .NET 1.1 (SP1)
#endif
		public void TypeMapping_IEnumerable_SimpleClass_PrivateGetEnumerator ()
		{
			XmlTypeMapping tm = Map (typeof (SimpleClassEnumerablePrivateGetEnumerator));
			Assert.AreEqual ("ArrayOfAnyType", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("SimpleClassEnumerablePrivateGetEnumerator", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassEnumerablePrivateGetEnumerator", tm.TypeFullName, "#4");

			tm = Map (typeof (SimpleClassEnumerablePrivateGetEnumerator[]));
			Assert.AreEqual ("ArrayOfArrayOfAnyType", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfSimpleClassEnumerablePrivateGetEnumerator", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("SimpleClassEnumerablePrivateGetEnumerator[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassEnumerablePrivateGetEnumerator[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassEnumerablePrivateGetEnumerator[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassEnumerablePrivateGetEnumerator", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("SimpleClassEnumerablePrivateGetEnumerator[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassEnumerablePrivateGetEnumerator[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_Object_NoMatchingAddMethod ()
		{
			Map (typeof (ObjectCollectionNoMatchingAddMethod));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_Object_NoMatchingAddMethod_Array ()
		{
			Map (typeof (ObjectCollectionNoMatchingAddMethod[]));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_SimpleClass_NoMatchingAddMethod ()
		{
			Map (typeof (SimpleClassCollectionNoMatchingAddMethod));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_SimpleClass_NoMatchingAddMethod_Array ()
		{
			Map (typeof (SimpleClassCollectionNoMatchingAddMethod[]));
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TypeMapping_ICollection_SimpleClass ()
		{
			XmlTypeMapping tm = Map (typeof (SimpleClassCollection));
			Assert.AreEqual ("ArrayOfSimpleClass", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("SimpleClassCollection", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassCollection", tm.TypeFullName, "#4");

			tm = Map (typeof (SimpleClassCollection[]));
			Assert.AreEqual ("ArrayOfArrayOfSimpleClass", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfSimpleClassCollection", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("SimpleClassCollection[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassCollection[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassCollection[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassCollection", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("SimpleClassCollection[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.SimpleClassCollection[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TypeMapping_ICollection_Object ()
		{
			XmlTypeMapping tm = Map (typeof (ObjectCollection));
			Assert.AreEqual ("ArrayOfAnyType", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("ObjectCollection", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.ObjectCollection", tm.TypeFullName, "#4");

			tm = Map (typeof (ObjectCollection[]));
			Assert.AreEqual ("ArrayOfArrayOfAnyType", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfObjectCollection", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("ObjectCollection[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.ObjectCollection[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (ObjectCollection[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfObjectCollection", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("ObjectCollection[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.SoapReflectionImporterTests.ObjectCollection[][]", tm.TypeFullName, "#B4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_Object_NoIntIndexer ()
		{
			Map (typeof (ObjectCollectionNoIntIndexer));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_Object_NoIntIndexer_Array ()
		{
			Map (typeof (ObjectCollectionNoIntIndexer[]));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_SimpleClass_NoIntIndexer ()
		{
			Map (typeof (SimpleClassCollectionNoIntIndexer));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_SimpleClass_NoIntIndexer_Array ()
		{
			Map (typeof (SimpleClassCollectionNoIntIndexer[]));
		}

		[Test]
		public void TypeMapping_InvalidDefault ()
		{
			SoapAttributes attrs = new SoapAttributes (typeof (Field_Encoded).GetMember ("Modifiers")[0]);
			attrs.SoapDefaultValue = 2; // not a defined enum value
			SoapAttributeOverrides overrides = new SoapAttributeOverrides ();
			overrides.Add (typeof (Field_Encoded), "Modifiers", attrs);

			try {
				Map (typeof (Field_Encoded), overrides);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Enum System.Int32 cannot be converted to MonoTests.System.Xml.TestClasses.MapModifiers
				Assert.IsNotNull (ex.Message, "#A2");
				Assert.IsTrue (ex.Message.IndexOf (typeof (int).FullName) != -1, "#A3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (MapModifiers).FullName) != -1, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
			}

			attrs.SoapDefaultValue = "2"; // not of the same type as the underlying enum type (System.Int32)

			try {
				Map (typeof (Field_Encoded), overrides);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Enum System.String cannot be converted to MonoTests.System.Xml.TestClasses.MapModifiers
				Assert.IsNotNull (ex.Message, "#B2");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).FullName) != -1, "#B3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (MapModifiers).FullName) != -1, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
			}

			attrs.SoapDefaultValue = EnumDefaultValueNF.e2; // other enum type

			try {
				Map (typeof (Field_Encoded), overrides);
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// Enum MonoTests.System.Xml.TestClasses.EnumDefaultValueNF cannot be converted to MonoTests.System.Xml.TestClasses.MapModifiers
				Assert.IsNotNull (ex.Message, "#C2");
				Assert.IsTrue (ex.Message.IndexOf (typeof (EnumDefaultValueNF).FullName) != -1, "#C3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (MapModifiers).FullName) != -1, "#C4");
				Assert.IsNull (ex.InnerException, "#C5");
			}

			attrs.SoapDefaultValue = (MapModifiers) 20; // non-existing enum value

			try {
				Map (typeof (Field_Encoded), overrides);
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// Value '20' cannot be converted to MonoTests.System.Xml.TestClasses.MapModifiers
				Assert.IsNotNull (ex.Message, "#D2");
				Assert.IsTrue (ex.Message.IndexOf ("'20'") != -1, "#D3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (MapModifiers).FullName) != -1, "#D4");
				Assert.IsNull (ex.InnerException, "#D5");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void TestImportMembersMapping()
		{
			Type type = typeof (SimpleClass);
			SoapAttributes attrs = new SoapAttributes ();
			SoapAttributeOverrides overrides = new SoapAttributeOverrides ();
			overrides.Add (typeof (SimpleClass), attrs);

			XmlReflectionMember[] members = new XmlReflectionMember[0];
			XmlMembersMapping mm;
			try
			{
				mm = MembersMap(type, overrides, members, true, true);
				Assert.Fail("Should not be able to fetch an empty XmlMembersMapping");
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

			mm = MembersMap(type, overrides, members, false, false);

			Equals(mm.Count, 1);

			XmlMemberMapping smm = mm[0];
			Assert.IsFalse (smm.Any, "#1");
			Assert.AreEqual ("something", smm.ElementName, "#2");
			Assert.AreEqual ("something", smm.MemberName, "#3");
			Assert.IsNull (smm.Namespace, "#4");
			Assert.AreEqual ("System.String", smm.TypeFullName, "#5");
			Assert.AreEqual ("string", smm.TypeName, "#6");
			Assert.AreEqual (XmlSchemaNamespace, smm.TypeNamespace, "#7");

			
			rm = new XmlReflectionMember();
			rm.IsReturnValue = false;
			rm.MemberName = "nothing";
			rm.MemberType = typeof(string);
			members = new XmlReflectionMember[1];
			members[0] = rm;

			mm = MembersMap(type, overrides, members, false, false);
			Assert.AreEqual (1 , mm.Count, "#8");
		}

		public class Employee : IXmlSerializable
		{
			private string _firstName;
			private string _lastName;
			private string _address;

			public XmlSchema GetSchema ()
			{
				return null;
			}

			public void WriteXml (XmlWriter writer)
			{
				writer.WriteStartElement ("employee", "urn:devx-com");
				writer.WriteAttributeString ("firstName", _firstName);
				writer.WriteAttributeString ("lastName", _lastName);
				writer.WriteAttributeString ("address", _address);
				writer.WriteEndElement ();
			}

			public void ReadXml (XmlReader reader)
			{
				XmlNodeType type = reader.MoveToContent ();
				if (type == XmlNodeType.Element && reader.LocalName == "employee") {
					_firstName = reader["firstName"];
					_lastName = reader["lastName"];
					_address = reader["address"];
				}
			}
		}

		public class NestedStruct
		{
			public TimeSpan Period = TimeSpan.MaxValue;
		}

		public class ObjectEnumerable : IEnumerable
		{
			public void Add (int value)
			{
			}

			public void Add (object value)
			{
			}

			public IEnumerator GetEnumerator ()
			{
				return new ArrayList ().GetEnumerator ();
			}
		}

		public class SimpleClassEnumerable : IEnumerable
		{
			public void Add (int value)
			{
			}

			public void Add (object value)
			{
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public SimpleClassEnumerator GetEnumerator ()
			{
				return new SimpleClassEnumerator (new ArrayList ());
			}
		}

		public class SimpleClassEnumerablePrivateGetEnumerator : IEnumerable
		{
			public void Add (object value)
			{
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new ArrayList ().GetEnumerator ();
			}
		}

		public class SimpleClassEnumerablePrivateCurrent : IEnumerable
		{
			public void Add (object value)
			{
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public NoCurrentEnumerator GetEnumerator ()
			{
				return new NoCurrentEnumerator (new ArrayList ());
			}
		}

		// GetEnumerator().Current returns object, but there's no corresponding
		// Add (System.Object) method
		public class ObjectEnumerableNoMatchingAddMethod : IEnumerable
		{
			public void Add (int value)
			{
			}

			public IEnumerator GetEnumerator ()
			{
				return new ArrayList ().GetEnumerator ();
			}
		}

		// GetEnumerator().Current returns SimpleClass, but there's no 
		// corresponding Add (SimpleClass) method
		public class SimpleClassCollectionNoMatchingAddMethod : ICollection
		{
			public SimpleClass this[int index] {
				get {
					return (SimpleClass) _list[index];
				}
			}

			public int Count {
				get { return _list.Count; }
			}

			public bool IsSynchronized {
				get { return _list.IsSynchronized; }
			}

			public object SyncRoot {
				get { return _list.SyncRoot; }
			}

			public void CopyTo (Array array, int index)
			{
				_list.CopyTo (array, index);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator ();
			}

			public SimpleClassEnumerator GetEnumerator ()
			{
				return new SimpleClassEnumerator (_list);
			}

			private ArrayList _list = new ArrayList ();
		}

		// GetEnumerator().Current returns object, but there's no corresponding
		// Add (System.Object) method
		public class ObjectCollectionNoMatchingAddMethod : ICollection
		{
			public object this[int index] {
				get {
					return _list[index];
				}
			}

			public int Count {
				get { return _list.Count; }
			}

			public bool IsSynchronized {
				get { return _list.IsSynchronized; }
			}

			public object SyncRoot {
				get { return _list.SyncRoot; }
			}

			public void CopyTo (Array array, int index)
			{
				_list.CopyTo (array, index);
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public IEnumerator GetEnumerator ()
			{
				return _list.GetEnumerator ();
			}

			private ArrayList _list = new ArrayList ();
		}

		// Does not have int indexer.
		public class SimpleClassCollectionNoIntIndexer : ICollection
		{
			public SimpleClass this[string name] {
				get {
					return new SimpleClass ();
				}
			}

			public int Count {
				get { return _list.Count; }
			}

			public bool IsSynchronized {
				get { return _list.IsSynchronized; }
			}

			public object SyncRoot {
				get { return _list.SyncRoot; }
			}

			public void CopyTo (Array array, int index)
			{
				_list.CopyTo (array, index);
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public SimpleClassEnumerator GetEnumerator ()
			{
				return new SimpleClassEnumerator (_list);
			}

			public void Add (SimpleClass value)
			{
				_list.Add (value);
			}

			private ArrayList _list = new ArrayList ();
		}

		// Does not have int indexer.
		public class ObjectCollectionNoIntIndexer : ICollection
		{
			public object this[string name] {
				get {
					return new SimpleClass ();
				}
			}

			public int Count {
				get { return _list.Count; }
			}

			public bool IsSynchronized {
				get { return _list.IsSynchronized; }
			}

			public object SyncRoot {
				get { return _list.SyncRoot; }
			}

			public void CopyTo (Array array, int index)
			{
				_list.CopyTo (array, index);
			}

			public IEnumerator GetEnumerator ()
			{
				return _list.GetEnumerator ();
			}

			public void Add (object value)
			{
				_list.Add (value);
			}

			private ArrayList _list = new ArrayList ();
		}

		public class SimpleClassCollection : ICollection
		{
			public SimpleClass this[int index] {
				get {
					return (SimpleClass) _list[index];
				}
			}

			public int Count {
				get { return _list.Count; }
			}

			public bool IsSynchronized {
				get { return _list.IsSynchronized; }
			}

			public object SyncRoot {
				get { return _list.SyncRoot; }
			}

			public void CopyTo (Array array, int index)
			{
				_list.CopyTo (array, index);
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public SimpleClassEnumerator GetEnumerator ()
			{
				return new SimpleClassEnumerator (_list);
			}

			public void Add (SimpleClass value)
			{
				_list.Add (value);
			}

			private ArrayList _list = new ArrayList ();
		}

		public class ObjectCollection : ICollection
		{
			public object this[int name] {
				get {
					return new SimpleClass ();
				}
			}

			public int Count
			{
				get { return _list.Count; }
			}

			public bool IsSynchronized
			{
				get { return _list.IsSynchronized; }
			}

			public object SyncRoot
			{
				get { return _list.SyncRoot; }
			}

			public void CopyTo (Array array, int index)
			{
				_list.CopyTo (array, index);
			}

			public IEnumerator GetEnumerator ()
			{
				return _list.GetEnumerator ();
			}

			public void Add (object value)
			{
				_list.Add (value);
			}

			private ArrayList _list = new ArrayList ();
		}

		public class SimpleClassEnumerator : IEnumerator
		{
			internal SimpleClassEnumerator (ArrayList arguments)
			{
				IEnumerable temp = (IEnumerable) (arguments);
				_baseEnumerator = temp.GetEnumerator ();
			}
			public SimpleClass Current
			{
				get { return (SimpleClass) _baseEnumerator.Current; }
			}

			object IEnumerator.Current
			{
				get { return _baseEnumerator.Current; }
			}

			public bool MoveNext ()
			{
				return _baseEnumerator.MoveNext ();
			}

			bool IEnumerator.MoveNext ()
			{
				return _baseEnumerator.MoveNext ();
			}

			public void Reset ()
			{
				_baseEnumerator.Reset ();
			}

			void IEnumerator.Reset ()
			{
				_baseEnumerator.Reset ();
			}

			private IEnumerator _baseEnumerator;
		}

		public class NoCurrentEnumerator : IEnumerator
		{
			internal NoCurrentEnumerator (ArrayList arguments)
			{
				IEnumerable temp = (IEnumerable) (arguments);
				_baseEnumerator = temp.GetEnumerator ();
			}

			object IEnumerator.Current
			{
				get { return _baseEnumerator.Current; }
			}

			public bool MoveNext ()
			{
				return _baseEnumerator.MoveNext ();
			}

			bool IEnumerator.MoveNext ()
			{
				return _baseEnumerator.MoveNext ();
			}

			public void Reset ()
			{
				_baseEnumerator.Reset ();
			}

			void IEnumerator.Reset ()
			{
				_baseEnumerator.Reset ();
			}

			private IEnumerator _baseEnumerator;
		}
	}
}
