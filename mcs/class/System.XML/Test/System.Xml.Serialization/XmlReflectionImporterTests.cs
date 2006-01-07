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
using System.Xml;
using System.Xml.Schema;
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
		[Category ("NotWorking")]
		public void TestIntTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map(typeof(int[]));
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

			tm = Map (typeof (int[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfInt", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfInt32", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("Int32[][][]", tm.TypeName, "#C3");
#endif
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
		[Category ("NotWorking")]
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

			tm = Map (typeof (string[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfString", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfString", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("String[][][]", tm.TypeName, "#C3");
#endif
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
		[Category ("NotWorking")]
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

			tm = Map (typeof (object[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfObject", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("Object[][][]", tm.TypeName, "#C3");
#endif
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
		[Category ("NotWorking")]
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
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfByte", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("Byte[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.Byte[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (byte[][][]));
			Assert.AreEqual ("ArrayOfArrayOfBase64Binary", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfByte", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("Byte[][][]", tm.TypeName, "#C3");
#endif
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
		[Category ("NotWorking")]
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

			tm = Map (typeof (DateTime[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfDateTime", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfDateTime", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("DateTime[][][]", tm.TypeName, "#C3");
#endif
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
		[Category ("NotWorking")]
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

			tm = Map (typeof (Guid[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfGuid", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfGuid", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("Guid[][][]", tm.TypeName, "#C3");
#endif
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
		[Category ("NotWorking")]
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

			tm = Map (typeof (char[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfChar", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfChar", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("Char[][][]", tm.TypeName, "#C3");
#endif
			Assert.AreEqual ("System.Char[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
		public void TestXmlSerializableTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (Employee));
			Assert.AreEqual ("Employee", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("Employee", tm.TypeName, "#3");
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.Employee", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestXmlSerializableTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (Employee[]));
			Assert.AreEqual ("ArrayOfEmployee", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfEmployee", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("Employee[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.Employee[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (Employee[][]));
			Assert.AreEqual ("ArrayOfArrayOfEmployee", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfEmployee", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("Employee[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.Employee[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (Employee[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfEmployee", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfEmployee", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("Employee[][][]", tm.TypeName, "#C3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.Employee[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
		public void TestStructTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (TimeSpan));
			Assert.AreEqual ("TimeSpan", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("TimeSpan", tm.TypeName, "#3");
			Assert.AreEqual ("System.TimeSpan", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestStructTypeMapping_Array ()
		{
			XmlTypeMapping tm = Map (typeof (TimeSpan[]));
			Assert.AreEqual ("ArrayOfTimeSpan", tm.ElementName, "#A1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#A2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfTimeSpan", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("TimeSpan[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("System.TimeSpan[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (TimeSpan[][]));
			Assert.AreEqual ("ArrayOfArrayOfTimeSpan", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfTimeSpan", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("TimeSpan[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("System.TimeSpan[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (TimeSpan[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfTimeSpan", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfTimeSpan", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("TimeSpan[][][]", tm.TypeName, "#C3");
#endif
			Assert.AreEqual ("System.TimeSpan[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestEnumTypeMapping ()
		{
			XmlTypeMapping tm = Map (typeof (AttributeTargets));
			Assert.AreEqual ("AttributeTargets", tm.ElementName, "#1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#2");
			Assert.AreEqual ("AttributeTargets", tm.TypeName, "#3");
			Assert.AreEqual ("System.AttributeTargets", tm.TypeFullName, "#4");
		}

		[Test]
		[Category ("NotWorking")]
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

			tm = Map (typeof (AttributeTargets[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAttributeTargets", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAttributeTargets", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("AttributeTargets[][][]", tm.TypeName, "#C3");
#endif
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
		[Category ("NotWorking")]
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

			tm = Map (typeof (SimpleClass[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClass", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("SimpleClass[][][]", tm.TypeName, "#C3");
#endif
			Assert.AreEqual ("MonoTests.System.Xml.TestClasses.SimpleClass[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
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
#if NET_2_0
			Assert.AreEqual ("ArrayOfSimpleClassEnumerable", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("SimpleClassEnumerable[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerable[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassEnumerable[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassEnumerable", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("SimpleClassEnumerable[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerable[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (SimpleClassEnumerable[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClassEnumerable", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("SimpleClassEnumerable[][][]", tm.TypeName, "#C3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerable[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
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
#if NET_2_0
			Assert.AreEqual ("ArrayOfObjectEnumerable", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("ObjectEnumerable[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectEnumerable[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (ObjectEnumerable[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfObjectEnumerable", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("ObjectEnumerable[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectEnumerable[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (ObjectEnumerable[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfObjectEnumerable", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("ObjectEnumerable[][][]", tm.TypeName, "#C3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectEnumerable[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_IEnumarable_Object_NoMatchingAddMethod ()
		{
			Map (typeof (ObjectEnumerableNoMatchingAddMethod));
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_IEnumarable_Object_NoMatchingAddMethod_Array ()
		{
			Map (typeof (ObjectEnumerableNoMatchingAddMethod[]));
		}

		[Test]
		[Category ("NotWorking")]
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
#if NET_2_0
			Assert.AreEqual ("ArrayOfSimpleClassEnumerablePrivateCurrent", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("SimpleClassEnumerablePrivateCurrent[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateCurrent[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassEnumerablePrivateCurrent[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassEnumerablePrivateCurrent", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("SimpleClassEnumerablePrivateCurrent[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateCurrent[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (SimpleClassEnumerablePrivateCurrent[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClassEnumerablePrivateCurrent", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("SimpleClassEnumerablePrivateCurrent[][][]", tm.TypeName, "#C3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateCurrent[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")] // results in NullReferenceException in .NET 1.1 (SP1)
#endif
		[Category ("NotWorking")]
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
#if NET_2_0
			Assert.AreEqual ("ArrayOfSimpleClassEnumerablePrivateGetEnumerator", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("SimpleClassEnumerablePrivateGetEnumerator[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateGetEnumerator[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassEnumerablePrivateGetEnumerator[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassEnumerablePrivateGetEnumerator", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("SimpleClassEnumerablePrivateGetEnumerator[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateGetEnumerator[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (SimpleClassEnumerablePrivateGetEnumerator[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClassEnumerablePrivateGetEnumerator", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("SimpleClassEnumerablePrivateGetEnumerator[][][]", tm.TypeName, "#C3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassEnumerablePrivateGetEnumerator[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_Object_NoMatchingAddMethod ()
		{
			Map (typeof (ObjectCollectionNoMatchingAddMethod));
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_Object_NoMatchingAddMethod_Array ()
		{
			Map (typeof (ObjectCollectionNoMatchingAddMethod[]));
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_SimpleClass_NoMatchingAddMethod ()
		{
			Map (typeof (SimpleClassCollectionNoMatchingAddMethod));
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_SimpleClass_NoMatchingAddMethod_Array ()
		{
			Map (typeof (SimpleClassCollectionNoMatchingAddMethod[]));
		}

		[Test]
		[Category ("NotWorking")]
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
#if NET_2_0
			Assert.AreEqual ("ArrayOfSimpleClassCollection", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("SimpleClassCollection[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassCollection[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (SimpleClassCollection[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfSimpleClassCollection", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("SimpleClassCollection[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassCollection[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (SimpleClassCollection[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfSimpleClass", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfSimpleClassCollection", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("SimpleClassCollection[][][]", tm.TypeName, "#C3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.SimpleClassCollection[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
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
#if NET_2_0
			Assert.AreEqual ("ArrayOfObjectCollection", tm.TypeName, "#A3");
#else
			Assert.AreEqual ("ObjectCollection[]", tm.TypeName, "#A3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectCollection[]", tm.TypeFullName, "#A4");

			tm = Map (typeof (ObjectCollection[][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#B1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#B2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfObjectCollection", tm.TypeName, "#B3");
#else
			Assert.AreEqual ("ObjectCollection[][]", tm.TypeName, "#B3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectCollection[][]", tm.TypeFullName, "#B4");

			tm = Map (typeof (ObjectCollection[][][]));
			Assert.AreEqual ("ArrayOfArrayOfArrayOfArrayOfAnyType", tm.ElementName, "#C1");
			Assert.AreEqual (string.Empty, tm.Namespace, "#C2");
#if NET_2_0
			Assert.AreEqual ("ArrayOfArrayOfArrayOfObjectCollection", tm.TypeName, "#C3");
#else
			Assert.AreEqual ("ObjectCollection[][][]", tm.TypeName, "#C3");
#endif
			Assert.AreEqual ("MonoTests.System.XmlSerialization.XmlReflectionImporterTests.ObjectCollection[][][]", tm.TypeFullName, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_Object_NoIntIndexer ()
		{
			Map (typeof (ObjectCollectionNoIntIndexer));
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_Object_NoIntIndexer_Array ()
		{
			Map (typeof (ObjectCollectionNoIntIndexer[]));
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_SimpleClass_NoIntIndexer ()
		{
			Map (typeof (SimpleClassCollectionNoIntIndexer));
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TypeMapping_ICollection_SimpleClass_NoIntIndexer_Array ()
		{
			Map (typeof (SimpleClassCollectionNoIntIndexer[]));
		}

		[Test]
		[Category ("NotWorking")]
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
			Assert.IsNull (smm.TypeNamespace, "#7");

			
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
	}
}

