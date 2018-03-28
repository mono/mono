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
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;
using System.Collections.Generic;

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
	public class XmlReflectionImporterTests
	{
		private const string SomeNamespace = "some:urn";
		private const string AnotherNamespace = "another:urn";
		private const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

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

		private XmlTypeMapping Map (Type t, string ns, XmlRootAttribute root)
		{
			XmlReflectionImporter ri = new XmlReflectionImporter (ns);
			XmlTypeMapping tm = ri.ImportTypeMapping (t, root);

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
			Assert.AreEqual ("int", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Int32", tm.TypeName, "#3");
			Assert.AreEqual ("System.Int32", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestIntTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map(typeof(int[]));
			Assert.AreEqual ("ArrayOfInt", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfInt32", tm.TypeName, "#A3");
			Assert.AreEqual ("System.Int32[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (int[][]));
			Assert.AreEqual ("ArrayOfArrayOfInt", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfInt32", tm.TypeName, "#B3");
			Assert.AreEqual ("System.Int32[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (int[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfInt", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfInt32", tm.TypeName, "#C3");
			Assert.AreEqual ("System.Int32[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		public void TestStringTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(string));
			Assert.AreEqual ("string", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("String", tm.TypeName, "#3");
			Assert.AreEqual ("System.String", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestStringTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (string[]));
			Assert.AreEqual ("ArrayOfString", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfString", tm.TypeName, "#A3");
			Assert.AreEqual ("System.String[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (string[][]));
			Assert.AreEqual ("ArrayOfArrayOfString", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfString", tm.TypeName, "#B3");
			Assert.AreEqual ("System.String[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (string[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfString", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfString", tm.TypeName, "#C3");
			Assert.AreEqual ("System.String[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		public void TestObjectTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(object));
			Assert.AreEqual ("anyType", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Object", tm.TypeName, "#3");
			Assert.AreEqual ("System.Object", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestObjectTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (object[]));
			Assert.AreEqual ("ArrayOfAnyType", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfObject", tm.TypeName, "#A3");
			Assert.AreEqual ("System.Object[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (object[][]));
			Assert.AreEqual ("ArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfObject", tm.TypeName, "#B3");
			Assert.AreEqual ("System.Object[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (object[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfObject", tm.TypeName, "#C3");
			Assert.AreEqual ("System.Object[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		public void TestByteTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(byte));
			Assert.AreEqual ("unsignedByte", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Byte", tm.TypeName, "#3");
			Assert.AreEqual ("System.Byte", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestByteTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map(typeof(byte[]));
			Assert.AreEqual ("base64Binary", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("Byte[]", tm.TypeName, "#A3");
			Assert.AreEqual ("System.Byte[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (byte[][]));
			Assert.AreEqual ("ArrayOfBase64Binary", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfByte", tm.TypeName, "#B3");
			Assert.AreEqual ("System.Byte[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (byte[][][]));
			Assert.AreEqual ("ArrayOfArrayOfBase64Binary", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfByte", tm.TypeName, "#C3");
			Assert.AreEqual ("System.Byte[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		public void TestBoolTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(bool));
			Assert.AreEqual ("boolean", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Boolean", tm.TypeName, "#3");
			Assert.AreEqual ("System.Boolean", tm.TypeFullName, "#4");
		}

		[Test]
		public void TestShortTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(short));
			Assert.AreEqual ("short", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Int16", tm.TypeName, "#3");
			Assert.AreEqual ("System.Int16", tm.TypeFullName, "#4");
		}

		[Test]
		public void TestUnsignedShortTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(ushort));
			Assert.AreEqual ("unsignedShort", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("UInt16", tm.TypeName, "#3");
			Assert.AreEqual ("System.UInt16", tm.TypeFullName, "#4");
		}
		
		[Test]
		public void TestUIntTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(uint));
			Assert.AreEqual ("unsignedInt", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("UInt32", tm.TypeName, "#3");
			Assert.AreEqual ("System.UInt32", tm.TypeFullName, "#4");
		}
		
		[Test]
		public void TestLongTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(long));
			Assert.AreEqual ("long", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Int64", tm.TypeName, "#3");
			Assert.AreEqual ("System.Int64", tm.TypeFullName, "#4");
		}
		
		[Test]
		public void TestULongTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(ulong));
			Assert.AreEqual ("unsignedLong", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("UInt64", tm.TypeName, "#3");
			Assert.AreEqual ("System.UInt64", tm.TypeFullName, "#4");
		}
		
		[Test]
		public void TestFloatTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(float));
			Assert.AreEqual ("float", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Single", tm.TypeName, "#3");
			Assert.AreEqual ("System.Single", tm.TypeFullName, "#4");
		}
		
		[Test]
		public void TestDoubleTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(double));
			Assert.AreEqual ("double", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Double", tm.TypeName, "#3");
			Assert.AreEqual ("System.Double", tm.TypeFullName, "#4");
		}
		
		[Test]
		public void TestDateTimeTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(DateTime));
			Assert.AreEqual ("dateTime", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("DateTime", tm.TypeName, "#3");
			Assert.AreEqual ("System.DateTime", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestDateTimeTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (DateTime[]));
			Assert.AreEqual ("ArrayOfDateTime", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfDateTime", tm.TypeName, "#A3");
			Assert.AreEqual ("System.DateTime[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (DateTime[][]));
			Assert.AreEqual ("ArrayOfArrayOfDateTime", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfDateTime", tm.TypeName, "#B3");
			Assert.AreEqual ("System.DateTime[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (DateTime[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfDateTime", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfDateTime", tm.TypeName, "#C3");
			Assert.AreEqual ("System.DateTime[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		public void TestGuidTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(Guid));
			Assert.AreEqual ("guid", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Guid", tm.TypeName, "#3");
			Assert.AreEqual ("System.Guid", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestGuidTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (Guid[]));
			Assert.AreEqual ("ArrayOfGuid", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfGuid", tm.TypeName, "#A3");
			Assert.AreEqual ("System.Guid[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (Guid[][]));
			Assert.AreEqual ("ArrayOfArrayOfGuid", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfGuid", tm.TypeName, "#B3");
			Assert.AreEqual ("System.Guid[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (Guid[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfGuid", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfGuid", tm.TypeName, "#C3");
			Assert.AreEqual ("System.Guid[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		public void TestDecimalTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(decimal));
			Assert.AreEqual ("decimal", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Decimal", tm.TypeName, "#3");
			Assert.AreEqual ("System.Decimal", tm.TypeFullName, "#4");
		}
		
		[Test]
		public void TestXmlQualifiedNameTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(XmlQualifiedName));
			Assert.AreEqual ("QName", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("XmlQualifiedName", tm.TypeName, "#3");
			Assert.AreEqual ("System.Xml.XmlQualifiedName", tm.TypeFullName, "#4");
		}
		
		[Test]
		public void TestSByteTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(sbyte));
			Assert.AreEqual ("byte", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("SByte", tm.TypeName, "#3");
			Assert.AreEqual ("System.SByte", tm.TypeFullName, "#4");
		}
		

		[Test]
		public void TestCharTypeMapping()
		{
			XmlTypeMapping tm = Map(typeof(char));
			Assert.AreEqual ("char", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Char", tm.TypeName, "#3");
			Assert.AreEqual ("System.Char", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestCharTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (char[]));
			Assert.AreEqual ("ArrayOfChar", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfChar", tm.TypeName, "#A3");
			Assert.AreEqual ("System.Char[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (char[][]));
			Assert.AreEqual ("ArrayOfArrayOfChar", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfChar", tm.TypeName, "#B3");
			Assert.AreEqual ("System.Char[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (char[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfChar", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfChar", tm.TypeName, "#C3");
			Assert.AreEqual ("System.Char[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestXmlNodeTypeMapping ()
		{
			Type type = typeof (XmlNode);

			XmlTypeMapping tm = Map (type);
			Assert.AreEqual (string.Empty, tm.ElementName, "#A1");
			Assert.IsNull (tm.Namespace, "#A2");
			Assert.AreEqual ("XmlNode", tm.TypeName, "#A3");
			Assert.AreEqual ("System.Xml.XmlNode", tm.TypeFullName, "#A4");

			tm = Map (type, AnotherNamespace);
			Assert.AreEqual (string.Empty, tm.ElementName, "#B1");
			Assert.IsNull (tm.Namespace, "#B2");
			Assert.AreEqual ("XmlNode", tm.TypeName, "#B3");
			Assert.AreEqual ("System.Xml.XmlNode", tm.TypeFullName, "#B4");

			XmlRootAttribute root = new XmlRootAttribute ("somename");
			root.Namespace = SomeNamespace;
			tm = Map (type, root);
			Assert.AreEqual ("somename", tm.ElementName, "#C1");
			Assert.IsNull (tm.Namespace, "#C2");
			Assert.AreEqual ("XmlNode", tm.TypeName, "#C3");
			Assert.AreEqual ("System.Xml.XmlNode", tm.TypeFullName, "#C4");

			tm = Map (type, AnotherNamespace, root);
			Assert.AreEqual ("somename", tm.ElementName, "#D1");
			Assert.IsNull (tm.Namespace, "#D2");
			Assert.AreEqual ("XmlNode", tm.TypeName, "#D3");
			Assert.AreEqual ("System.Xml.XmlNode", tm.TypeFullName, "#D4");

			root.Namespace = null;
			tm = Map (type, root);
			Assert.AreEqual ("somename", tm.ElementName, "#E1");
			Assert.IsNull (tm.Namespace, "#E2");
			Assert.AreEqual ("XmlNode", tm.TypeName, "#E3");
			Assert.AreEqual ("System.Xml.XmlNode", tm.TypeFullName, "#E4");

			tm = Map (type, AnotherNamespace, root);
			Assert.AreEqual ("somename", tm.ElementName, "#F1");
			Assert.IsNull (tm.Namespace, "#F2");
			Assert.AreEqual ("XmlNode", tm.TypeName, "#F3");
			Assert.AreEqual ("System.Xml.XmlNode", tm.TypeFullName, "#F4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestXmlElementTypeMapping ()
		{
			Type type = typeof (XmlElement);

			XmlTypeMapping tm = Map (type);
			Assert.AreEqual (string.Empty, tm.ElementName, "#1");
			Assert.IsNull (tm.Namespace, "#2");
			Assert.AreEqual ("XmlElement", tm.TypeName, "#3");
			Assert.AreEqual ("System.Xml.XmlElement", tm.TypeFullName, "#4");

			tm = Map (type, AnotherNamespace);
			Assert.AreEqual (string.Empty, tm.ElementName, "#B1");
			Assert.IsNull (tm.Namespace, "#B2");
			Assert.AreEqual ("XmlElement", tm.TypeName, "#B3");
			Assert.AreEqual ("System.Xml.XmlElement", tm.TypeFullName, "#B4");

			XmlRootAttribute root = new XmlRootAttribute ("somename");
			root.Namespace = SomeNamespace;
			tm = Map (type, root);
			Assert.AreEqual ("somename", tm.ElementName, "#C1");
			Assert.IsNull (tm.Namespace, "#C2");
			Assert.AreEqual ("XmlElement", tm.TypeName, "#C3");
			Assert.AreEqual ("System.Xml.XmlElement", tm.TypeFullName, "#C4");

			tm = Map (type, AnotherNamespace, root);
			Assert.AreEqual ("somename", tm.ElementName, "#D1");
			Assert.IsNull (tm.Namespace, "#D2");
			Assert.AreEqual ("XmlElement", tm.TypeName, "#D3");
			Assert.AreEqual ("System.Xml.XmlElement", tm.TypeFullName, "#D4");

			root.Namespace = null;
			tm = Map (type, root);
			Assert.AreEqual ("somename", tm.ElementName, "#E1");
			Assert.IsNull (tm.Namespace, "#E2");
			Assert.AreEqual ("XmlElement", tm.TypeName, "#E3");
			Assert.AreEqual ("System.Xml.XmlElement", tm.TypeFullName, "#E4");

			tm = Map (type, AnotherNamespace, root);
			Assert.AreEqual ("somename", tm.ElementName, "#F1");
			Assert.IsNull (tm.Namespace, "#F2");
			Assert.AreEqual ("XmlElement", tm.TypeName, "#F3");
			Assert.AreEqual ("System.Xml.XmlElement", tm.TypeFullName, "#F4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestXmlNotationTypeMapping ()
		{
			Type type = typeof (XmlNotation);

			XmlTypeMapping tm = Map (type);
			Assert.AreEqual (string.Empty, tm.ElementName, "#1");
			Assert.IsNull (tm.Namespace, "#2");
			Assert.AreEqual ("XmlNotation", tm.TypeName, "#3");
			Assert.AreEqual ("System.Xml.XmlNotation", tm.TypeFullName, "#4");

			tm = Map (type, AnotherNamespace);
			Assert.AreEqual (string.Empty, tm.ElementName, "#B1");
			Assert.IsNull (tm.Namespace, "#B2");
			Assert.AreEqual ("XmlNotation", tm.TypeName, "#B3");
			Assert.AreEqual ("System.Xml.XmlNotation", tm.TypeFullName, "#B4");

			XmlRootAttribute root = new XmlRootAttribute ("somename");
			root.Namespace = SomeNamespace;
			tm = Map (type, root);
			Assert.AreEqual ("somename", tm.ElementName, "#C1");
			Assert.IsNull (tm.Namespace, "#C2");
			Assert.AreEqual ("XmlNotation", tm.TypeName, "#C3");
			Assert.AreEqual ("System.Xml.XmlNotation", tm.TypeFullName, "#C4");

			tm = Map (type, AnotherNamespace, root);
			Assert.AreEqual ("somename", tm.ElementName, "#D1");
			Assert.IsNull (tm.Namespace, "#D2");
			Assert.AreEqual ("XmlNotation", tm.TypeName, "#D3");
			Assert.AreEqual ("System.Xml.XmlNotation", tm.TypeFullName, "#D4");

			root.Namespace = null;
			tm = Map (type, root);
			Assert.AreEqual ("somename", tm.ElementName, "#E1");
			Assert.IsNull (tm.Namespace, "#E2");
			Assert.AreEqual ("XmlNotation", tm.TypeName, "#E3");
			Assert.AreEqual ("System.Xml.XmlNotation", tm.TypeFullName, "#E4");

			tm = Map (type, AnotherNamespace, root);
			Assert.AreEqual ("somename", tm.ElementName, "#F1");
			Assert.IsNull (tm.Namespace, "#F2");
			Assert.AreEqual ("XmlNotation", tm.TypeName, "#F3");
			Assert.AreEqual ("System.Xml.XmlNotation", tm.TypeFullName, "#F4");
		}

		[Test]
		public void TestXmlSerializableTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (Employee));
			Assert.AreEqual ("Employee", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Employee", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.Employee", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TestXmlSerializableTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (Employee[]));
			Assert.AreEqual ("ArrayOfEmployee", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfEmployee", tm.TypeName, "#A3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.Employee[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (Employee[][]));
			Assert.AreEqual ("ArrayOfArrayOfEmployee", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfEmployee", tm.TypeName, "#B3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.Employee[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (Employee[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfEmployee", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfEmployee", tm.TypeName, "#C3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.Employee[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		public void TestClassTypeMapping_NestedStruct ()
		{
			XmlTypeMapping tm = Map (typeof (NestedStruct));
			Assert.AreEqual ("NestedStruct", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("NestedStruct", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.NestedStruct", tm.TypeFullName, "#4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestNullTypeMapping()
		{
			Map(null);
		}

		[Test]
		public void TestIntTypeMappingWithDefaultNamespaces()
		{
			XmlTypeMapping tm = Map(typeof(int), SomeNamespace);
			Assert.AreEqual ("int", tm.ElementName, "#1");
			Assert.AreEqual (SomeNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Int32", tm.TypeName, "#3");
			Assert.AreEqual ("System.Int32", tm.TypeFullName, "#4");
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
		public void TestStructTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (TimeSpan[]));
			Assert.AreEqual ("ArrayOfTimeSpan", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfTimeSpan", tm.TypeName, "#A3");
			Assert.AreEqual ("System.TimeSpan[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (TimeSpan[][]));
			Assert.AreEqual ("ArrayOfArrayOfTimeSpan", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfTimeSpan", tm.TypeName, "#B3");
			Assert.AreEqual ("System.TimeSpan[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (TimeSpan[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfTimeSpan", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfTimeSpan", tm.TypeName, "#C3");
			Assert.AreEqual ("System.TimeSpan[][][]", tm.TypeFullName, "#C4");
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
		public void TestEnumTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (AttributeTargets[]));
			Assert.AreEqual ("ArrayOfAttributeTargets", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfAttributeTargets", tm.TypeName, "#A3");
			Assert.AreEqual ("System.AttributeTargets[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (AttributeTargets[][]));
			Assert.AreEqual ("ArrayOfArrayOfAttributeTargets", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfAttributeTargets", tm.TypeName, "#B3");
			Assert.AreEqual ("System.AttributeTargets[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (AttributeTargets[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAttributeTargets", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAttributeTargets", tm.TypeName, "#C3");
			Assert.AreEqual ("System.AttributeTargets[][][]", tm.TypeFullName, "#C4");
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
		public void TestClassTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (SimpleClass[]));
			Assert.AreEqual ("ArrayOfSimpleClass", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfSimpleClass", tm.TypeName, "#A3");
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.SimpleClass[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClass[][]));
			Assert.AreEqual ("ArrayOfArrayOfSimpleClass", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfSimpleClass", tm.TypeName, "#B3");
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.SimpleClass[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (SimpleClass[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClass", tm.TypeName, "#C3");
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.SimpleClass[][][]", tm.TypeFullName, "#C4");
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
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerable", tm.TypeFullName, "#4");

			tm = Map (typeof (SimpleClassEnumerable[]));
			Assert.AreEqual ("ArrayOfArrayOfSimpleClass", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfSimpleClassEnumerable", tm.TypeName, "#A3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerable[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassEnumerable[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassEnumerable", tm.TypeName, "#B3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerable[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (SimpleClassEnumerable[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClassEnumerable", tm.TypeName, "#C3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerable[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TypeMapping_IEnumerable_Object ()
		{
			XmlTypeMapping tm = Map (typeof (ObjectEnumerable));
			Assert.AreEqual ("ArrayOfAnyType", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("ObjectEnumerable", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectEnumerable", tm.TypeFullName, "#4");

			tm = Map (typeof (ObjectEnumerable[]));
			Assert.AreEqual ("ArrayOfArrayOfAnyType", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfObjectEnumerable", tm.TypeName, "#A3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectEnumerable[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (ObjectEnumerable[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfObjectEnumerable", tm.TypeName, "#B3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectEnumerable[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (ObjectEnumerable[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfObjectEnumerable", tm.TypeName, "#C3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectEnumerable[][][]", tm.TypeFullName, "#C4");
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
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateCurrent", tm.TypeFullName, "#4");

			tm = Map (typeof (SimpleClassEnumerablePrivateCurrent[]));
			Assert.AreEqual ("ArrayOfArrayOfAnyType", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfSimpleClassEnumerablePrivateCurrent", tm.TypeName, "#A3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateCurrent[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassEnumerablePrivateCurrent[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassEnumerablePrivateCurrent", tm.TypeName, "#B3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateCurrent[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (SimpleClassEnumerablePrivateCurrent[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClassEnumerablePrivateCurrent", tm.TypeName, "#C3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateCurrent[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TypeMapping_IEnumerable_SimpleClass_PrivateGetEnumerator ()
		{
			XmlTypeMapping tm = Map (typeof (SimpleClassEnumerablePrivateGetEnumerator));
			Assert.AreEqual ("ArrayOfAnyType", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("SimpleClassEnumerablePrivateGetEnumerator", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateGetEnumerator", tm.TypeFullName, "#4");

			tm = Map (typeof (SimpleClassEnumerablePrivateGetEnumerator[]));
			Assert.AreEqual ("ArrayOfArrayOfAnyType", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfSimpleClassEnumerablePrivateGetEnumerator", tm.TypeName, "#A3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateGetEnumerator[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassEnumerablePrivateGetEnumerator[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassEnumerablePrivateGetEnumerator", tm.TypeName, "#B3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateGetEnumerator[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (SimpleClassEnumerablePrivateGetEnumerator[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClassEnumerablePrivateGetEnumerator", tm.TypeName, "#C3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateGetEnumerator[][][]", tm.TypeFullName, "#C4");
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
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassCollection", tm.TypeFullName, "#4");

			tm = Map (typeof (SimpleClassCollection[]));
			Assert.AreEqual ("ArrayOfArrayOfSimpleClass", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfSimpleClassCollection", tm.TypeName, "#A3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassCollection[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassCollection[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassCollection", tm.TypeName, "#B3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassCollection[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (SimpleClassCollection[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClassCollection", tm.TypeName, "#C3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassCollection[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
		public void TypeMapping_ICollection_Object ()
		{
			XmlTypeMapping tm = Map (typeof (ObjectCollection));
			Assert.AreEqual ("ArrayOfAnyType", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("ObjectCollection", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectCollection", tm.TypeFullName, "#4");

			tm = Map (typeof (ObjectCollection[]));
			Assert.AreEqual ("ArrayOfArrayOfAnyType", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
			Assert.AreEqual ("ArrayOfObjectCollection", tm.TypeName, "#A3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectCollection[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (ObjectCollection[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
			Assert.AreEqual ("ArrayOfArrayOfObjectCollection", tm.TypeName, "#B3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectCollection[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (ObjectCollection[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
			Assert.AreEqual ("ArrayOfArrayOfArrayOfObjectCollection", tm.TypeName, "#C3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectCollection[][][]", tm.TypeFullName, "#C4");
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
			XmlAttributes attrs = new XmlAttributes (typeof (Field).GetMember ("Modifiers") [0]);
			attrs.XmlDefaultValue = 2; // not a defined enum value
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			overrides.Add (typeof (Field), "Modifiers", attrs);

			try {
				Map (typeof (Field), overrides);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// There was an error reflecting type MonoTests.System.Xml.TestClasses.Field
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (Field).FullName) != -1, "#A4");
				Assert.IsNotNull (ex.InnerException, "#A5");

				// There was an error reflecting field 'Modifiers'
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#A6");
				Assert.IsNotNull (ex.InnerException.Message, "#A7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'Modifiers'") != -1, "#A8");
				Assert.IsNotNull (ex.InnerException.InnerException, "#A9");

				// Value '2' cannot be converted to System.Int32
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.InnerException.GetType (), "#A10");
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#A11");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf ("'2'") != -1, "#A12");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf (typeof (int).FullName) != -1, "#A13");
				Assert.IsNull (ex.InnerException.InnerException.InnerException, "#A14");
			}

			attrs.XmlDefaultValue = "2"; // not of the same type as the underlying enum type (System.Int32)

			try {
				Map (typeof (Field), overrides);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// There was an error reflecting type MonoTests.System.Xml.TestClasses.Field
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (Field).FullName) != -1, "#B4");
				Assert.IsNotNull (ex.InnerException, "#B5");

				// There was an error reflecting field 'Modifiers'
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#B6");
				Assert.IsNotNull (ex.InnerException.Message, "#B7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'Modifiers'") != -1, "#B8");
				Assert.IsNotNull (ex.InnerException.InnerException, "#B9");

				// Enum underlying type and the object must be same type or object.
				// Type passed in was 'System.String'; the enum underlying type was
				// 'System.Int32'.
				Assert.AreEqual (typeof (ArgumentException), ex.InnerException.InnerException.GetType (), "#B10");
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#B11");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf (typeof (string).FullName) != -1, "#B12");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf (typeof (int).FullName) != -1, "#B13");
				Assert.IsNull (ex.InnerException.InnerException.InnerException, "#B14");
			}

			attrs.XmlDefaultValue = EnumDefaultValueNF.e2; // other enum type

			try {
				Map (typeof (Field), overrides);
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// There was an error reflecting type MonoTests.System.Xml.TestClasses.Field
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (Field).FullName) != -1, "#C4");
				Assert.IsNotNull (ex.InnerException, "#C5");

				// There was an error reflecting field 'Modifiers'
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#C6");
				Assert.IsNotNull (ex.InnerException.Message, "#C7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'Modifiers'") != -1, "#C8");
				Assert.IsNotNull (ex.InnerException.InnerException, "#C9");

				// Object must be the same type as the enum. The type passed in
				// was MonoTests.System.Xml.TestClasses.EnumDefaultValueNF; the
				// enum type was MonoTests.System.Xml.TestClasses.MapModifiers
				Assert.AreEqual (typeof (ArgumentException), ex.InnerException.InnerException.GetType (), "#C10");
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#C11");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).FullName) != -1, "#C12");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf (typeof (MapModifiers).FullName) != -1, "#C13");
				Assert.IsNull (ex.InnerException.InnerException.InnerException, "#C14");
			}

			attrs.XmlDefaultValue = (MapModifiers) 20; // non-existing enum value

			try {
				Map (typeof (Field), overrides);
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// There was an error reflecting type MonoTests.System.Xml.TestClasses.Field
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (Field).FullName) != -1, "#D4");
				Assert.IsNotNull (ex.InnerException, "#D5");

				// There was an error reflecting field 'Modifiers'
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D6");
				Assert.IsNotNull (ex.InnerException.Message, "#D7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'Modifiers'") != -1, "#D8");
				Assert.IsNotNull (ex.InnerException.InnerException, "#D9");

				// Value '20' cannot be converted to MonoTests.System.Xml.TestClasses.MapModifiers
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.InnerException.GetType (), "#D10");
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#D11");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf ("'20'") != -1, "#D12");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf (typeof (MapModifiers).FullName) != -1, "#D13");
				Assert.IsNull (ex.InnerException.InnerException.InnerException, "#D14");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TypeMapping_Null ()
		{
			Map ((Type) null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TypeMapping_Void ()
		{
			Map (typeof (void));
		}

		[Test]
		public void TypeMapping_WrongChoices ()
		{
			try {
				Map (typeof (WrongChoices));
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// There was an error reflecting type 'MonoTests.System.Xml.TestClasses.WrongChoices'
				Assert.IsNotNull (ex.Message, "#2");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (WrongChoices).FullName + "'") != -1, "#3");
				Assert.IsNotNull (ex.InnerException, "#4");

				// There was an error reflecting field 'MyChoice'
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'MyChoice'") != -1, "#6");
				Assert.IsNotNull (ex.InnerException.InnerException, "#7");

				// Type MonoTests.System.Xml.TestClasses.ItemChoiceType is missing 
				// enumeration value 'StrangeOne' for element 'StrangeOne' from
				// namespace ''.
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#8");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf (typeof (ItemChoiceType).FullName) != -1, "#9");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf ("'StrangeOne'") != -1, "#10");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf ("''") != -1, "#11");
			}
		}

		[Test] // bug #77591
		public void TypeMapping_XmlText_PrimitiveTypes ()
		{
			XmlAttributeOverrides overrides = null;
			XmlAttributes attrs = null;

			overrides = new XmlAttributeOverrides ();
			attrs = new  XmlAttributes ();
			attrs.XmlText = new XmlTextAttribute (typeof (int));
			overrides.Add (typeof (Field), "Modifiers", attrs);

			try {
				Map (typeof (Field), overrides);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// There was an error reflecting type 'MonoTests.System.Xml.TestClasses.Field'
				Assert.IsNotNull (ex.Message, "#A2");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (Field).FullName + "'") != -1, "#A3");
				Assert.IsNotNull (ex.InnerException, "#A4");

				// There was an error reflecting field 'Modifiers'
				Assert.IsNotNull (ex.InnerException.Message, "#A5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'Modifiers'") != -1, "#A6");
				Assert.IsNotNull (ex.InnerException.InnerException, "#A7");

				// The type for XmlText may not be specified for primitive types
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#A8");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf ("XmlText") != -1, "#A9");
			}

			overrides = new XmlAttributeOverrides ();
			attrs = new XmlAttributes ();
			attrs.XmlText = new XmlTextAttribute (typeof (int));
			overrides.Add (typeof (Field), "Street", attrs);

			try {
				Map (typeof (Field), overrides);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// There was an error reflecting type 'MonoTests.System.Xml.TestClasses.Field'
				Assert.IsNotNull (ex.Message, "#B2");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (Field).FullName + "'") != -1, "#B3");
				Assert.IsNotNull (ex.InnerException, "#B4");

				// There was an error reflecting field 'Street'
				Assert.IsNotNull (ex.InnerException.Message, "#B5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'Street'") != -1, "#B6");
				Assert.IsNotNull (ex.InnerException.InnerException, "#B7");

				// The type for XmlText may not be specified for primitive types
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#B8");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf ("XmlText") != -1, "#B9");
			}

			overrides = new XmlAttributeOverrides ();
			attrs = new XmlAttributes ();
			attrs.XmlText = new XmlTextAttribute (typeof (MapModifiers));
			overrides.Add (typeof (Field), "Modifiers", attrs);
			Map (typeof (Field), overrides);

			overrides = new XmlAttributeOverrides ();
			attrs = new XmlAttributes ();
			attrs.XmlText = new XmlTextAttribute (typeof (string));
			overrides.Add (typeof (Field), "Street", attrs);
			Map (typeof (Field), overrides);
		}

		[Test]
		[Category ("NotWorking")] // mark it NotWorking until fixes have landed in svn
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

			mm = MembersMap(type, overrides, members, false);

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

			mm = MembersMap(type, overrides, members, false);
			Assert.AreEqual (1, mm.Count, "#8");
		}

		[Test]
		public void TestIntTypeMappingWithXmlRootAttribute()
		{
			const string TheNamespace = "another:urn";
			XmlRootAttribute root = new XmlRootAttribute("price");
			root.Namespace = TheNamespace;
			
			XmlTypeMapping tm = Map(typeof(int), root);
			Assert.AreEqual ("price", tm.ElementName, "#1");
			Assert.AreEqual (TheNamespace, tm.Namespace, "#2");
			Assert.AreEqual ("Int32", tm.TypeName, "#3");
			Assert.AreEqual ("System.Int32", tm.TypeFullName, "#4");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestSerializeWrongChoice ()
		{
			new XmlSerializer (typeof(WrongChoices));
		}

		[Test]
		public void XmlArrayOnByteArray ()
		{
			new XmlSerializer (typeof (XmlArrayOnByteArrayType));
		}


		[Test]
		public void ImportNullableInt ()
		{
			XmlReflectionImporter imp = new XmlReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (typeof (int?));
			XmlSchemas schemas = new XmlSchemas ();
			XmlSchemaExporter exp = new XmlSchemaExporter (schemas);
			exp.ExportTypeMapping (map);
			XmlSchema schema = schemas [0];
			XmlSchemaElement el = schema.Items [0] as XmlSchemaElement;
			Assert.AreEqual ("int", el.Name, "#1");
			Assert.AreEqual (new XmlQualifiedName ("int", XmlSchema.Namespace), el.SchemaTypeName, "#2");
			Assert.AreEqual (true, el.IsNillable, "#3");
		}

		[Test]
		public void ImportNullableContainer ()
		{
			new XmlSerializer (typeof (NullableContainer));
		}

		[Test]
		public void ImportNullableContainer2 ()
		{
			XmlReflectionImporter imp = new XmlReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (typeof (NullableContainer2));
			XmlSchemas schemas = new XmlSchemas ();
			XmlSchemaExporter exp = new XmlSchemaExporter (schemas);
			exp.ExportTypeMapping (map);

			XmlSchema schema = schemas [0];
			XmlSchemaComplexType el = schema.Items [1] as XmlSchemaComplexType;

			XmlSchemaSequence s = el.Particle as XmlSchemaSequence;
			XmlSchemaElement el2 = s.Items [0] as XmlSchemaElement;
			Assert.IsTrue (el2.IsNillable);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ImportGenericTypeDefinition ()
		{
			new XmlSerializer (typeof (List<int>).GetGenericTypeDefinition ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void XmlSchemaProviderMissingMethod ()
		{
			new XmlSerializer (typeof (XmlSchemaProviderMissingMethodType));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void XmlSchemaProviderMethodNonStatic ()
		{
			new XmlSerializer (typeof (XmlSchemaProviderNonStaticType));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void XmlSchemaProviderMethodIncorrectReturn ()
		{
			new XmlSerializer (typeof (XmlSchemaProviderIncorrectReturnType));
		}

		[Test]
		public void XmlSchemaProviderAndDefaultNamespace ()
		{
			XmlTypeMapping tm = new XmlReflectionImporter ("urn:bar").ImportTypeMapping (typeof (XmlSchemaProviderAndDefaultNamespaceType));
			Assert.AreEqual ("foo", tm.ElementName, "#1");
			Assert.AreEqual ("foo", tm.XsdTypeName, "#2");
			Assert.AreEqual ("urn:bar", tm.Namespace, "#3");
			Assert.AreEqual ("urn:foo", tm.XsdTypeNamespace);
		}

		[Test]
		public void ImportGenericICollectionWrapped ()
		{
			new XmlSerializer (typeof (MyCollection));
		}

		[Test]
		public void Bug704813Type ()
		{
			var xs = new XmlSerializer (typeof (Bug704813Type));
			xs.Serialize (TextWriter.Null, new Bug704813Type ());
		}

		[Test]
		public void Bug708178Type()
		{
			string file = Path.Combine (Path.GetTempPath (), "Bug708178Type.xml");
			XmlSerializer xmlSerializer = new XmlSerializer (typeof(Bug708178Type));
			Bug708178Type bugType = new Bug708178Type ();
			bugType.Foo.Add ("test");
			Assert.AreEqual (1, bugType.Foo.Count);
		 
			//xml Serialize
			TextWriter WriteFileStream = new StreamWriter (file, false);
			xmlSerializer.Serialize (WriteFileStream, bugType);
			WriteFileStream.Close ();
		 
			//xml Deserialize
			FileStream ReadFileStream = new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read);
			Bug708178Type bugTypeReload = (Bug708178Type)xmlSerializer.Deserialize (ReadFileStream);
		 
			//should have deserialized the relationship
			Assert.AreEqual(1, bugTypeReload.Foo.Count);
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
			public SimpleClass this[int index]
			{
				get
				{
					return (SimpleClass) _list[index];
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

			IEnumerator IEnumerable.GetEnumerator ()
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
			public object this[int index]
			{
				get
				{
					return _list[index];
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
			public SimpleClass this[string name]
			{
				get
				{
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
			public object this[string name]
			{
				get
				{
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

		public class SimpleClassCollection : ICollection
		{
			public SimpleClass this[int index]
			{
				get
				{
					return (SimpleClass) _list[index];
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
			public object this[int name]
			{
				get
				{
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

		public class XmlArrayOnByteArrayType
		{
			[XmlArray]
			[XmlArrayItem ("Byte", IsNullable =false)]
			public byte [] Args;
		}

		public class NullableContainer
		{
			[XmlElement (IsNullable = true)]
			public int? NilInt;
		}

		public class NullableContainer2
		{
			int? value;

			public int? NullableInt {
				get { return value; }
				set { this.value = value; }
			}
		}

		[XmlSchemaProvider ("GetXsdType")]
		public class XmlSchemaProviderMissingMethodType : IXmlSerializable
		{
			public void ReadXml (XmlReader reader)
			{
			}

			public void WriteXml (XmlWriter writer)
			{
			}

			public XmlSchema GetSchema ()
			{
				return null;
			}
		}

		[XmlSchemaProvider ("GetXsdType")]
		public class XmlSchemaProviderNonStaticType : IXmlSerializable
		{
			public void ReadXml (XmlReader reader)
			{
			}

			public void WriteXml (XmlWriter writer)
			{
			}

			public XmlSchema GetSchema ()
			{
				return null;
			}

			public object GetXsdType ()
			{
				return null;
			}
		}

		[XmlSchemaProvider ("GetXsdType")]
		public class XmlSchemaProviderIncorrectReturnType : IXmlSerializable
		{
			public void ReadXml (XmlReader reader)
			{
			}

			public void WriteXml (XmlWriter writer)
			{
			}

			public XmlSchema GetSchema ()
			{
				return null;
			}

			public static object GetXsdType ()
			{
				return null;
			}
		}

		[XmlSchemaProvider ("GetXsd")]
		public class XmlSchemaProviderAndDefaultNamespaceType : IXmlSerializable
		{
			public static XmlQualifiedName GetXsd (XmlSchemaSet xss)
			{
				XmlSchema xs = new XmlSchema ();
				xs.TargetNamespace = "urn:foo";
				XmlSchemaComplexType ct = new XmlSchemaComplexType ();
				ct.Name = "foo";
				xs.Items.Add (ct);
				xss.Add (xs);
				return new XmlQualifiedName ("foo", "urn:foo");
			}

			public void WriteXml (XmlWriter write)
			{
			}

			public void ReadXml (XmlReader reader)
			{
			}

			public XmlSchema GetSchema ()
			{
				return null;
			}
		}

		public class MyCollection : ICollection<string>
		{
			public int Count { get { return 0; } }

			public bool IsReadOnly { get { return false; } }

			public void Add (string s)
			{
			}

			public void Clear ()
			{
			}

			public bool Contains (string item)
			{
				return false;
			}

			public void CopyTo (string [] array, int arrayIndex)
			{
			}

			public IEnumerator<string> GetEnumerator ()
			{
				throw new Exception ();
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public bool Remove (string item)
			{
				return false;
			}
		}

		public class Bug594490Class
		{
			[XmlAttribute ("xml:lang")]
			public string GroupName;
		}

		[Test]
		public void Bug594490_SerializationOfXmlLangAttribute ()
		{
			var serializer = new XmlSerializer (typeof(Bug594490Class));

			using (var writer = new StringWriter ()) {
				var obj = new Bug594490Class ();

				obj.GroupName = "hello world";

				serializer.Serialize (writer, obj);
				writer.Close ();

				Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>" + Environment.NewLine +
				                 @"<Bug594490Class xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xml:lang=""hello world"" />",
					writer.ToString (),
					"Novell bug #594490 (https://bugzilla.novell.com/show_bug.cgi?id=594490) not fixed.");
			}
		}

		/*
		 * The following code was generated from Microsoft's xsd.exe with the /classes switch.
		 * It only includes the relevent details but was based on the following namespaces:
		 *   urn:oasis:names:tc:SAML:2.0:protocol
		 *   urn:oasis:names:tc:SAML:2.0:assertion
		 *   http://www.w3.org/2000/09/xmldsig#
		 *   http://www.w3.org/2001/04/xmlenc
		 */

		[XmlTypeAttribute (Namespace = "urn:oasis:names:tc:SAML:2.0:protocol")]
		[XmlRootAttribute ("RequestedAuthnContext", Namespace = "urn:oasis:names:tc:SAML:2.0:protocol", IsNullable = false)]
		public class RequestedAuthnContext
		{
			string[] items;
			ItemsChoice7[] itemsElementName;

			[XmlElementAttribute ("AuthnContextClassRef", typeof (string), Namespace = "urn:oasis:names:tc:SAML:2.0:assertion", DataType = "anyURI")]
			[XmlElementAttribute ("AuthnContextDeclRef", typeof (string), Namespace = "urn:oasis:names:tc:SAML:2.0:assertion", DataType = "anyURI")]
			[XmlChoiceIdentifierAttribute ("ItemsElementName")]
			public string[] Items {
				get { return this.items; }
				set { this.items = value; }
			}

			[XmlElementAttribute ("ItemsElementName")]
			[XmlIgnoreAttribute ()]
			public ItemsChoice7[] ItemsElementName {
				get { return this.itemsElementName; }
				set { this.itemsElementName = value; }
			}
		}

		[XmlTypeAttribute (Namespace = "urn:oasis:names:tc:SAML:2.0:protocol", IncludeInSchema = false)]
		public enum ItemsChoice7 {
			[XmlEnumAttribute ("urn:oasis:names:tc:SAML:2.0:assertion:AuthnContextClassRef")]
			AuthnContextClassRef,
			[XmlEnumAttribute ("urn:oasis:names:tc:SAML:2.0:assertion:AuthnContextDeclRef")]
			AuthnContextDeclRef,
		}
		// End snippet from xsd.exe
	
		[Test]
		public void FullyQualifiedName_XmlEnumAttribute ()
		{
			var serializer = new XmlSerializer (typeof (RequestedAuthnContext)); 
		}
	}
}

