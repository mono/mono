//
// System.Xml.XmlSerializerTests
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//
//
// NOTES:
//  Where possible, these tests avoid testing the order of
//  an object's members serialization. Mono and .NET do not
//  reflect members in the same order.
//
//  Only serializations tests so far, no deserialization.
//
// FIXME
//  test XmlArrayAttribute
//  test XmlArrayItemAttribute
//  test serialization of decimal type
//  test serialization of Guid type
//  test XmlNode serialization with and without modifying attributes.
//  test deserialization
//  FIXMEs found in this file

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlSerializerTests
	{
		const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";
		const string XmlSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		const string WsdlTypesNamespace = "http://microsoft.com/wsdl/types/";
		const string ANamespace = "some:urn";
		const string AnotherNamespace = "another:urn";

		StringWriter sw;
		XmlTextWriter xtw;
		XmlSerializer xs;

		private void SetUpWriter()
		{
			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			xtw.Formatting = Formatting.None;
		}
		
		private string WriterText 
		{
			get
			{
				string val = sw.GetStringBuilder().ToString();
				int offset = val.IndexOf('>') + 1;
				val = val.Substring(offset);
				return Infoset(val);
			}
		}

		private void Serialize(object o)
		{
			SetUpWriter();
			xs = new XmlSerializer(o.GetType());
			xs.Serialize(xtw, o);
		}
		
		private void Serialize(object o, Type type)
		{
			SetUpWriter();
			xs = new XmlSerializer(type);
			xs.Serialize(xtw, o);
		}

		private void Serialize(object o, XmlSerializerNamespaces ns)
		{
			SetUpWriter();
			xs = new XmlSerializer(o.GetType());
			xs.Serialize(xtw, o, ns);
		}

		private void Serialize(object o, XmlAttributeOverrides ao)
		{
			SetUpWriter();
			xs = new XmlSerializer(o.GetType(), ao);
			xs.Serialize(xtw, o);
		}
		
		private void Serialize(object o, XmlRootAttribute root)
		{
			SetUpWriter();
			xs = new XmlSerializer(o.GetType(), root);
			xs.Serialize(xtw, o);
		}

		private void Serialize (object o, XmlTypeMapping typeMapping)
		{
			SetUpWriter ();
			xs = new XmlSerializer (typeMapping);
			xs.Serialize (xtw, o);
		}

		private void SerializeEncoded (object o)
		{
			SerializeEncoded (o, o.GetType ());
		}

		private void SerializeEncoded (object o, Type type)
		{
			XmlTypeMapping mapping = CreateSoapMapping (type);
			SetUpWriter ();
			xs = new XmlSerializer (mapping);
			xs.Serialize (xtw, o);
		}

		// test constructors
#if USE_VERSION_1_1	// It doesn't pass on MS.NET 1.1.
		[Test]
		public void TestConstructor()
		{
			XmlSerializer ser = new XmlSerializer (null, "");
		}
#else
#endif

		// test basic types ////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeInt()
		{
			Serialize(10);
			Assert.AreEqual (Infoset("<int>10</int>"), WriterText);
		}

		[Test]
		public void TestSerializeBool()
		{
			Serialize(true);
			Assert.AreEqual (Infoset ("<boolean>true</boolean>"), WriterText);
			
			Serialize(false);
			Assert.AreEqual (Infoset ("<boolean>false</boolean>"), WriterText);
		}
		
		[Test]
		public void TestSerializeString()
		{
			Serialize("hello");
			Assert.AreEqual (Infoset ("<string>hello</string>"), WriterText);
		}

		[Test]
		public void TestSerializeEmptyString()
		{
			Serialize(String.Empty);
			Assert.AreEqual (Infoset ("<string />"), WriterText);
		}
		
		[Test]
		public void TestSerializeNullObject()
		{
			Serialize(null, typeof(object));
			Assert.AreEqual (Infoset ("<anyType xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />"), WriterText);
		}

		[Test]
		[Ignore ("The generated XML is not exact but it is equivalent")]
		public void TestSerializeNullString()
		{
			Serialize(null, typeof(string));
			Assert.AreEqual (Infoset ("<string xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />"), WriterText);
		}
			
		[Test]
		public void TestSerializeIntArray()
		{
			Serialize(new int[] {1, 2, 3, 4});
			Assert.AreEqual (Infoset ("<ArrayOfInt xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><int>1</int><int>2</int><int>3</int><int>4</int></ArrayOfInt>"), WriterText);
		}
		
		[Test]
		public void TestSerializeEmptyArray()
		{
			Serialize(new int[] {});
			Assert.AreEqual (Infoset ("<ArrayOfInt xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
		}
		
		[Test]
		public void TestSerializeChar()
		{
			Serialize('A');
			Assert.AreEqual (Infoset ("<char>65</char>"), WriterText);
			
			Serialize('\0');
			Assert.AreEqual (Infoset ("<char>0</char>"), WriterText);
			
			Serialize('\n');
			Assert.AreEqual (Infoset ("<char>10</char>"), WriterText);
			
			Serialize('\uFF01');
			Assert.AreEqual (Infoset ("<char>65281</char>"), WriterText);
		}
		
		[Test]
		public void TestSerializeFloat()
		{
			Serialize(10.78);
			Assert.AreEqual (Infoset ("<double>10.78</double>"), WriterText);
			
			Serialize(-1e8);
			Assert.AreEqual (Infoset ("<double>-100000000</double>"), WriterText);
			
			// FIXME test INF and other boundary conditions that may exist with floats
		}

		[Test]
		[Category ("NotDotNet")] // bug #77501, will fail on .NET until we output name of constant
		public void TestSerializeEnumeration_FromValue ()
		{
			Serialize ((int) SimpleEnumeration.SECOND, typeof (SimpleEnumeration));
			Assert.AreEqual (
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration>1</SimpleEnumeration>",
				sw.ToString ());
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializeEnumeration_FromValue_Encoded ()
		{
			SerializeEncoded ((int) SimpleEnumeration.SECOND, typeof (SimpleEnumeration));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>SECOND</SimpleEnumeration>",
				XmlSchemaInstanceNamespace), sw.ToString ());
		}

		[Test]
		public void TestSerializeEnumeration()
		{
			Serialize(SimpleEnumeration.FIRST);
			Assert.AreEqual (Infoset ("<SimpleEnumeration>FIRST</SimpleEnumeration>"), WriterText, "#1");
			
			Serialize(SimpleEnumeration.SECOND);
			Assert.AreEqual (Infoset ("<SimpleEnumeration>SECOND</SimpleEnumeration>"), WriterText, "#2");
		}

		[Test]
		public void TestSerializeEnumeration_Encoded()
		{
			SerializeEncoded (SimpleEnumeration.FIRST);
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>FIRST</SimpleEnumeration>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#B1");

			SerializeEncoded (SimpleEnumeration.SECOND);
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>SECOND</SimpleEnumeration>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#B2");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializeEnumDefaultValue ()
		{
			Serialize (new EnumDefaultValue ());
			Assert.AreEqual (Infoset ("<EnumDefaultValue />"), WriterText, "#1");

			Serialize (new SimpleEnumeration ());
			Assert.AreEqual (Infoset ("<SimpleEnumeration>FIRST</SimpleEnumeration>"), WriterText, "#2");

			Serialize (3, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#3");

			Serialize (EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#4");

			Serialize (EnumDefaultValue.e1 | EnumDefaultValue.e2, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#5");

			Serialize (EnumDefaultValue.e1 | EnumDefaultValue.e2 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#6");

			Serialize (EnumDefaultValue.e1 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#7");

			Serialize (EnumDefaultValue.e2 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#8");

			Serialize (3, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>one two</FlagEnum>"), WriterText, "#9");

			Serialize (5, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>one four</FlagEnum>"), WriterText, "#10");

			Serialize (FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>four</FlagEnum>"), WriterText, "#11");

			Serialize (FlagEnum.e1 | FlagEnum.e2, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>one two</FlagEnum>"), WriterText, "#12");

			Serialize (FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>one two four</FlagEnum>"), WriterText, "#13");

			Serialize (FlagEnum.e1 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>one four</FlagEnum>"), WriterText, "#14");

			Serialize (FlagEnum.e2 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>two four</FlagEnum>"), WriterText, "#15");

			Serialize (3, typeof (EnumDefaultValueNF));
			Assert.AreEqual (Infoset ("<EnumDefaultValueNF>e3</EnumDefaultValueNF>"), WriterText, "#16");

			Serialize (EnumDefaultValueNF.e2, typeof (EnumDefaultValueNF));
			Assert.AreEqual (Infoset ("<EnumDefaultValueNF>e2</EnumDefaultValueNF>"), WriterText, "#17");

			Serialize (2, typeof (ZeroFlagEnum));
			Assert.AreEqual (Infoset ("<ZeroFlagEnum>tns:t&lt;w&gt;o</ZeroFlagEnum>"), WriterText, "#18");

			Serialize (new ZeroFlagEnum ()); // enum actually has a field with value 0
			Assert.AreEqual (Infoset ("<ZeroFlagEnum>zero</ZeroFlagEnum>"), WriterText, "#19");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializeEnumDefaultValue_Encoded ()
		{
			SerializeEncoded (new EnumDefaultValue ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}' />",
				XmlSchemaInstanceNamespace), sw.ToString (), "#1");

			SerializeEncoded (new SimpleEnumeration ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>FIRST</SimpleEnumeration>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#2");

			SerializeEncoded (3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#3");

			SerializeEncoded (EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#4");

			SerializeEncoded (EnumDefaultValue.e1 | EnumDefaultValue.e2, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#5");

			SerializeEncoded (EnumDefaultValue.e1 | EnumDefaultValue.e2 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#6");

			SerializeEncoded (EnumDefaultValue.e1 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#7");

			SerializeEncoded (EnumDefaultValue.e2 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#8");

			SerializeEncoded (3, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e2</FlagEnum>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#9");

			SerializeEncoded (5, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e4</FlagEnum>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#10");

			SerializeEncoded (FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e4</FlagEnum>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#11");

			SerializeEncoded (FlagEnum.e1 | FlagEnum.e2, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e2</FlagEnum>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#12");

			SerializeEncoded (FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e2 e4</FlagEnum>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#13");

			SerializeEncoded (FlagEnum.e1 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e4</FlagEnum>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#14");

			SerializeEncoded (FlagEnum.e2 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e2 e4</FlagEnum>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#15");

			SerializeEncoded (3, typeof (EnumDefaultValueNF));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValueNF d1p1:type='EnumDefaultValueNF' xmlns:d1p1='{0}'>e3</EnumDefaultValueNF>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#16");

			SerializeEncoded (EnumDefaultValueNF.e2, typeof (EnumDefaultValueNF));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValueNF d1p1:type='EnumDefaultValueNF' xmlns:d1p1='{0}'>e2</EnumDefaultValueNF>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#17");

			SerializeEncoded (2, typeof (ZeroFlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<ZeroFlagEnum d1p1:type='ZeroFlagEnum' xmlns:d1p1='{0}'>e2</ZeroFlagEnum>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#18");

			SerializeEncoded (new ZeroFlagEnum ()); // enum actually has a field with value 0
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<ZeroFlagEnum d1p1:type='ZeroFlagEnum' xmlns:d1p1='{0}'>e0</ZeroFlagEnum>",
				XmlSchemaInstanceNamespace), sw.ToString (), "#19");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializeEnumDefaultValue_InvalidValue1 ()
		{
			try {
				Serialize ("b", typeof (EnumDefaultValue));
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#A2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#A3");
			}

			try {
				Serialize ("e1", typeof (EnumDefaultValue));
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#B2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#B3");
			}

			try {
				Serialize ("e1,e2", typeof (EnumDefaultValue));
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#C2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#C3");
			}

			try {
				Serialize (string.Empty, typeof (EnumDefaultValue));
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#D2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#D3");
			}

			try {
				Serialize ("1", typeof (EnumDefaultValue));
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#E2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#E3");
			}

			try {
				Serialize ("0", typeof (EnumDefaultValue));
				Assert.Fail ("#F1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#F2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#F3");
			}

			try {
				Serialize (new SimpleClass (), typeof (EnumDefaultValue));
				Assert.Fail ("#G1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#G2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#G3");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializeEnumDefaultValue_InvalidValue2 ()
		{
#if NET_2_0
			try {
				Serialize (5, typeof (EnumDefaultValue));
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'5'") != -1, "#6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValue).FullName) != -1, "#7");
			}
#else
			Serialize (5, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>5</EnumDefaultValue>"), WriterText);
#endif
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializeEnumDefaultValueNF_InvalidValue1 ()
		{
#if NET_2_0
			try {
				Serialize (new EnumDefaultValueNF ());
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'0'") != -1, "#6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).FullName) != -1, "#7");
			}
#else
			Serialize (new EnumDefaultValueNF ());
			Assert.AreEqual (Infoset ("<EnumDefaultValueNF>0</EnumDefaultValueNF>"), WriterText);
#endif
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializeEnumDefaultValueNF_InvalidValue2 ()
		{
#if NET_2_0
			try {
				Serialize (15, typeof (EnumDefaultValueNF));
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'15'") != -1, "#6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).FullName) != -1, "#7");
			}
#else
			Serialize (15, typeof (EnumDefaultValueNF));
			Assert.AreEqual (Infoset ("<EnumDefaultValueNF>15</EnumDefaultValueNF>"), WriterText);
#endif
		}
 
		[Test]
		[Category ("NotWorking")]
		public void TestSerializeEnumDefaultValueNF_InvalidValue3 ()
		{
			try {
				Serialize ("b", typeof (EnumDefaultValueNF));
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#A2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#A3");
			}

			try {
				Serialize ("e2", typeof (EnumDefaultValueNF));
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#B2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#B3");
			}

			try {
				Serialize (string.Empty, typeof (EnumDefaultValueNF));
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#C2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#C3");
			}

			try {
				Serialize ("1", typeof (EnumDefaultValueNF));
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#D2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#D3");
			}

			try {
				Serialize ("0", typeof (EnumDefaultValueNF));
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#E2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#E3");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializeZeroFlagEnum_InvalidValue ()
		{
#if NET_2_0
			try {
				Serialize (4, typeof (ZeroFlagEnum)); // corresponding enum field is marked XmlIgnore
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'4'") != -1, "#6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (ZeroFlagEnum).FullName) != -1, "#7");
			}
#else
			Serialize (4, typeof (ZeroFlagEnum)); // corresponding enum field is marked XmlIgnore
			Assert.AreEqual (Infoset ("<ZeroFlagEnum>4</ZeroFlagEnum>"), WriterText);
#endif
		}

		[Test]
		public void TestSerializeQualifiedName()
		{
			Serialize(new XmlQualifiedName("me", "home.urn"));
			Assert.AreEqual (Infoset ("<QName xmlns:q1='home.urn'>q1:me</QName>"), WriterText);
		}
		
		[Test]
		public void TestSerializeBytes()
		{
			Serialize((byte)0xAB);
			Assert.AreEqual (Infoset ("<unsignedByte>171</unsignedByte>"), WriterText);
			
			Serialize((byte)15);
			Assert.AreEqual (Infoset ("<unsignedByte>15</unsignedByte>"), WriterText);
		}
		
		[Test]
		public void TestSerializeByteArrays()
		{
			Serialize(new byte[] {});
			Assert.AreEqual (Infoset ("<base64Binary />"), WriterText);
			
			Serialize(new byte[] {0xAB, 0xCD});
			Assert.AreEqual (Infoset ("<base64Binary>q80=</base64Binary>"), WriterText);
		}
		
		[Test]
		public void TestSerializeDateTime()
		{
			DateTime d = new DateTime();
			Serialize(d);
			
			TimeZone tz = TimeZone.CurrentTimeZone;
			TimeSpan off = tz.GetUtcOffset (d);
			string sp = string.Format ("{0}{1:00}:{2:00}", off.Ticks >= 0 ? "+" : "", off.Hours, off.Minutes);
			Assert.AreEqual (Infoset ("<dateTime>0001-01-01T00:00:00.0000000" + sp + "</dateTime>"), WriterText);
		}

		/*
		FIXME
		 - decimal
		 - Guid
		 - XmlNode objects
		
		[Test]
		public void TestSerialize()
		{
			Serialize();
			Assert.AreEqual (WriterText, "");
		}
		*/

		// test basic class serialization /////////////////////////////////////		
		[Test]
		public void TestSerializeSimpleClass()
		{
			SimpleClass simple = new SimpleClass();
			Serialize(simple);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
			
			simple.something = "hello";
			
			Serialize(simple);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>"), WriterText);
		}
		
		[Test]
		public void TestSerializeStringCollection()
		{
			StringCollection strings = new StringCollection();
			Serialize(strings);
			Assert.AreEqual (Infoset ("<ArrayOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
			
			strings.Add("hello");
			strings.Add("goodbye");
			Serialize(strings);
			Assert.AreEqual (Infoset ("<ArrayOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><string>hello</string><string>goodbye</string></ArrayOfString>"), WriterText);
		}
		
		[Test]
		public void TestSerializePlainContainer()
		{
			StringCollectionContainer container = new StringCollectionContainer();
			Serialize(container);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages /></StringCollectionContainer>"), WriterText);
			
			container.Messages.Add("hello");
			container.Messages.Add("goodbye");
			Serialize(container);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages><string>hello</string><string>goodbye</string></Messages></StringCollectionContainer>"), WriterText);
		}

		[Test]
		public void TestSerializeArrayContainer()
		{
			ArrayContainer container = new ArrayContainer();
			Serialize(container);
			Assert.AreEqual (Infoset ("<ArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
			
			container.items = new object[] {10, 20};
			Serialize(container);
			Assert.AreEqual (Infoset ("<ArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><anyType xsi:type='xsd:int'>10</anyType><anyType xsi:type='xsd:int'>20</anyType></items></ArrayContainer>"), WriterText);
			
			container.items = new object[] {10, "hello"};
			Serialize(container);
			Assert.AreEqual (Infoset ("<ArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><anyType xsi:type='xsd:int'>10</anyType><anyType xsi:type='xsd:string'>hello</anyType></items></ArrayContainer>"), WriterText);
		}
		
		[Test]
		public void TestSerializeClassArrayContainer()
		{
			ClassArrayContainer container = new ClassArrayContainer();
			Serialize(container);
			Assert.AreEqual (Infoset ("<ClassArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
			
			SimpleClass simple1 = new SimpleClass();
			simple1.something = "hello";
			SimpleClass simple2 = new SimpleClass();
			simple2.something = "hello";
			container.items = new SimpleClass[2];
			container.items[0] = simple1;
			container.items[1] = simple2;
			Serialize(container);
			Assert.AreEqual (Infoset ("<ClassArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><SimpleClass><something>hello</something></SimpleClass><SimpleClass><something>hello</something></SimpleClass></items></ClassArrayContainer>"), WriterText);
		}
		
		// test basic attributes ///////////////////////////////////////////////
		[Test]
		public void TestSerializeSimpleClassWithXmlAttributes()
		{
			SimpleClassWithXmlAttributes simple = new SimpleClassWithXmlAttributes();
			Serialize(simple);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			simple.something = "hello";
			Serialize(simple);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' member='hello' />"), WriterText);
		}
		
		// test overrides ///////////////////////////////////////////////////////
		[Test]
		public void TestSerializeSimpleClassWithOverrides()
		{
			// Also tests XmlIgnore
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			
			XmlAttributes attr = new XmlAttributes();
			attr.XmlIgnore = true;
			overrides.Add(typeof(SimpleClassWithXmlAttributes), "something", attr);
			
			SimpleClassWithXmlAttributes simple = new SimpleClassWithXmlAttributes();
			simple.something = "hello";
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
		}

		[Test]
		public void TestSerializeSchema ()
		{
			XmlSchema schema = new XmlSchema ();
			schema.Items.Add (new XmlSchemaAttribute ());
			schema.Items.Add (new XmlSchemaAttributeGroup ());
			schema.Items.Add (new XmlSchemaComplexType ());
			schema.Items.Add (new XmlSchemaNotation ());
			schema.Items.Add (new XmlSchemaSimpleType ());
			schema.Items.Add (new XmlSchemaGroup ());
			schema.Items.Add (new XmlSchemaElement ());

			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			xtw.Formatting = Formatting.Indented;
			XmlSerializer xs = new XmlSerializer (schema.GetType ());
			xs.Serialize (xtw, schema);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>{0}" +
				"<xsd:schema xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>{0}" +
				"  <xsd:attribute />{0}" +
				"  <xsd:attributeGroup />{0}" +
				"  <xsd:complexType />{0}" +
				"  <xsd:notation />{0}" +
				"  <xsd:simpleType />{0}" +
				"  <xsd:group />{0}" +
				"  <xsd:element />{0}" +
				"</xsd:schema>", Environment.NewLine), sw.ToString ());
		}

		// test xmlText //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlTextAttribute()
		{
			SimpleClass simple = new SimpleClass();
			simple.something = "hello";
			
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes attr = new XmlAttributes();
			overrides.Add(typeof(SimpleClass), "something", attr);
			
			attr.XmlText = new XmlTextAttribute();
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>hello</SimpleClass>"), WriterText, "#1");
			
			attr.XmlText = new XmlTextAttribute(typeof(string));
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>hello</SimpleClass>"), WriterText, "#2");
			
			try {
				attr.XmlText = new XmlTextAttribute(typeof(byte[]));
				Serialize(simple, overrides);
				Assert.Fail("XmlText.Type does not match the type it serializes: this should have failed");
			} catch (InvalidOperationException ex) {
				// there was an error reflecting type 'MonoTests.System.Xml.TestClasses.SimpleClass'.
				Assert.IsNotNull (ex.Message, "#A1");
				Assert.IsTrue (ex.Message.IndexOf (typeof (SimpleClass).FullName) != -1, "#A2");

				// there was an error reflecting field 'something'.
				Assert.IsNotNull (ex.InnerException, "#A3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#A4");
				Assert.IsNotNull (ex.InnerException.Message, "#A5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("something") != -1, "#A6");

				// the type for XmlText may not be specified for primitive types.
				Assert.IsNotNull (ex.InnerException.InnerException, "#A7");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#A8");
				Assert.IsNotNull (ex.InnerException.Message, "#A9");

				Assert.IsNull (ex.InnerException.InnerException.InnerException, "#A10");
			} catch (InvalidCastException ex) {
				// FIXME: we should report InvalidOperationException
			}
			
			try {
				attr.XmlText = new XmlTextAttribute();
				attr.XmlText.DataType = "sometype";
				Serialize(simple, overrides);
				Assert.Fail("XmlText.DataType does not match the type it serializes: this should have failed");
			} catch (InvalidOperationException ex) {
				// there was an error reflecting type 'MonoTests.System.Xml.TestClasses.SimpleClass'.
				Assert.IsNotNull (ex.Message, "#B1");
				Assert.IsTrue (ex.Message.IndexOf (typeof (SimpleClass).FullName) != -1, "#B2");

				// there was an error reflecting field 'something'.
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.InnerException.Message, "#B5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("something") != -1, "#B6");

				// there was an error reflecting type 'System.String'.
				Assert.IsNotNull (ex.InnerException.InnerException, "#B7");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.InnerException.GetType (), "#B8");
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#B9");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf (typeof (string).FullName) != -1, "#B10");

				// Value 'sometype' cannot be used for the XmlElementAttribute.DataType property. 
				// The datatype 'http://www.w3.org/2001/XMLSchema:sometype' is missing.
				Assert.IsNotNull (ex.InnerException.InnerException.InnerException, "#B11");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.InnerException.InnerException.GetType (), "#B12");
				Assert.IsNotNull (ex.InnerException.InnerException.InnerException.Message, "#B13");
				Assert.IsTrue (ex.InnerException.InnerException.InnerException.Message.IndexOf ("http://www.w3.org/2001/XMLSchema:sometype") != -1, "#B14");
			} catch (NotSupportedException ex) {
				// FIXME: we should report InvalidOperationException
			}
		}
		
		// test xmlRoot //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlRootAttribute()
		{
			// constructor override & element name
			XmlRootAttribute root = new XmlRootAttribute();
			root.ElementName = "renamed";
			
			SimpleClassWithXmlAttributes simpleWithAttributes = new SimpleClassWithXmlAttributes();
			Serialize(simpleWithAttributes, root);
			Assert.AreEqual (Infoset ("<renamed xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
			
			SimpleClass simple = null;
			root.IsNullable = false;
			try {
				Serialize(simple, root);
				Assert.Fail("Cannot serialize null object if XmlRoot's IsNullable == false");
			} catch (NullReferenceException) {
			}
			
			root.IsNullable = true;
			try {
				Serialize(simple, root);
				Assert.Fail("Cannot serialize null object if XmlRoot's IsNullable == true");
			} catch (NullReferenceException) {
			}
			
			simple = new SimpleClass();
			root.ElementName = null;
			root.Namespace = "some.urn";
			Serialize(simple, root);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='some.urn' />"), WriterText);
		}
		
		[Test]
		public void TestSerializeXmlRootAttributeOnMember()
		{			
			// nested root
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes childAttr = new XmlAttributes();
			childAttr.XmlRoot = new XmlRootAttribute("simple");
			overrides.Add(typeof(SimpleClass), childAttr);
			
			XmlAttributes attr = new XmlAttributes();
			attr.XmlRoot = new XmlRootAttribute("simple");
			overrides.Add(typeof(ClassArrayContainer), attr);
			
			ClassArrayContainer container = new ClassArrayContainer();
			container.items = new SimpleClass[1];
			container.items[0] = new SimpleClass();
			Serialize(container, overrides);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><SimpleClass /></items></simple>"), WriterText);
			
			// FIXME test data type
		}
		
		// test XmlAttribute /////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlAttributeAttribute()
		{	
			// null
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes attr = new XmlAttributes();
			attr.XmlAttribute = new XmlAttributeAttribute();
			overrides.Add(typeof(SimpleClass), "something", attr);
			
			SimpleClass simple = new SimpleClass();;
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#1");
			
			// regular
			simple.something = "hello";
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' something='hello' />"), WriterText, "#2");
			
			// AttributeName
			attr.XmlAttribute.AttributeName = "somethingelse";
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' somethingelse='hello' />"), WriterText, "#3");
			
			// Type
			// FIXME this should work, shouldnt it?
			// attr.XmlAttribute.Type = typeof(string);
			// Serialize(simple, overrides);
			// Assert(WriterText.EndsWith(" something='hello' />"));
			
			// Namespace
			attr.XmlAttribute.Namespace = "some:urn";
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' d1p1:somethingelse='hello' xmlns:d1p1='some:urn' />"), WriterText, "#4");
			
			// FIXME DataType
			// FIXME XmlSchemaForm Form
			
			// FIXME write XmlQualifiedName as attribute
		}
		
		// test XmlElement ///////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlElementAttribute()
		{
			
			
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes attr = new XmlAttributes();
			XmlElementAttribute element = new XmlElementAttribute();
			attr.XmlElements.Add(element);
			overrides.Add(typeof(SimpleClass), "something", attr);
			
			// null
			SimpleClass simple = new SimpleClass();;
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#1");
			
			// not null
			simple.something = "hello";
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>"), WriterText, "#2");
			
			//ElementName
			element.ElementName = "saying";
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><saying>hello</saying></SimpleClass>"), WriterText, "#3");
			
			//IsNullable
			element.IsNullable = false;
			simple.something = null;
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#4");
			
			element.IsNullable = true;
			simple.something = null;
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><saying xsi:nil='true' /></SimpleClass>"), WriterText, "#5");
			
			//Namespace
			element.ElementName = null;
			element.IsNullable = false;
			element.Namespace = "some:urn";
			simple.something = "hello";
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something xmlns='some:urn'>hello</something></SimpleClass>"), WriterText, "#6");
			
			//FIXME DataType
			//FIXME Form
			//FIXME Type
		}
		
		// test XmlElementAttribute with arrays and collections //////////////////
		[Test]
		public void TestSerializeCollectionWithXmlElementAttribute()
		{
			// the rule is:
			// if no type is specified or the specified type 
			//    matches the contents of the collection, 
			//    serialize each element in an element named after the member.
			// if the type does not match, or matches the collection itself,
			//    create a base wrapping element for the member, and then
			//    wrap each collection item in its own wrapping element based on type.
			
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes attr = new XmlAttributes();
			XmlElementAttribute element = new XmlElementAttribute();
			attr.XmlElements.Add(element);
			overrides.Add(typeof(StringCollectionContainer), "Messages", attr);
			
			// empty collection & no type info in XmlElementAttribute
			StringCollectionContainer container = new StringCollectionContainer();
			Serialize(container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
			
			// non-empty collection & no type info in XmlElementAttribute
			container.Messages.Add("hello");
			Serialize(container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages>hello</Messages></StringCollectionContainer>"), WriterText);
			
			// non-empty collection & only type info in XmlElementAttribute
			element.Type = typeof(StringCollection);
			Serialize(container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages><string>hello</string></Messages></StringCollectionContainer>"), WriterText);
			
			// non-empty collection & only type info in XmlElementAttribute
			element.Type = typeof(string);
			Serialize(container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages>hello</Messages></StringCollectionContainer>"), WriterText);
			
			// two elements
			container.Messages.Add("goodbye");
			element.Type = null;
			Serialize(container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages>hello</Messages><Messages>goodbye</Messages></StringCollectionContainer>"), WriterText);
		}
		
		// test DefaultValue /////////////////////////////////////////////////////
		[Test]
		public void TestSerializeDefaultValueAttribute()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			
			XmlAttributes attr = new XmlAttributes();
			string defaultValueInstance = "nothing";
			attr.XmlDefaultValue = defaultValueInstance;
			overrides.Add(typeof(SimpleClass), "something", attr);
			
			// use the default
			SimpleClass simple = new SimpleClass();
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
			
			// same value as default
			simple.something = defaultValueInstance;
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
			
			// some other value
			simple.something = "hello";
			Serialize(simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>"), WriterText);
		}
		
		// test XmlEnum //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlEnumAttribute()
		{
			Serialize(XmlSchemaForm.Qualified);
			Assert.AreEqual (Infoset ("<XmlSchemaForm>qualified</XmlSchemaForm>"), WriterText, "#1");
			
			Serialize(XmlSchemaForm.Unqualified);
			Assert.AreEqual (Infoset ("<XmlSchemaForm>unqualified</XmlSchemaForm>"), WriterText, "#2");
		}
		
		[Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void TestSerializeXmlEnumAttribute_IgnoredValue ()
		{
			// technically XmlSchemaForm.None has an XmlIgnore attribute,
			// but it is not being serialized as a member.

#if NET_2_0
			try {
				Serialize (XmlSchemaForm.None);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'0'") != -1, "#6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (XmlSchemaForm).FullName) != -1, "#7");
			}
#else
			Serialize (XmlSchemaForm.None);
			Assert.AreEqual (Infoset ("<XmlSchemaForm>0</XmlSchemaForm>"), WriterText);
#endif
		}

		[Test]
		public void TestSerializeXmlNodeArray ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new XmlNode [] { doc.CreateAttribute("at"), doc.CreateElement("elem1"), doc.CreateElement("elem2") }, typeof(object));
			Assert.AreEqual (Infoset ("<anyType at=\"\"><elem1/><elem2/></anyType>"), WriterText);
		}
		
		[Test]
		public void TestSerializeXmlElement ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (doc.CreateElement("elem"), typeof(XmlElement));
			Assert.AreEqual (Infoset ("<elem/>"), WriterText);
		}
		
		[Test]
		public void TestSerializeXmlElementSubclass ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new MyElem (doc), typeof(XmlElement));
			Assert.AreEqual (Infoset ("<myelem aa=\"1\"/>"), WriterText);
			
			Serialize (new MyElem (doc), typeof(MyElem));
			Assert.AreEqual (Infoset ("<myelem aa=\"1\"/>"), WriterText);
		}
		
		[Test]
		public void TestSerializeXmlCDataSection ()
		{
			XmlDocument doc = new XmlDocument ();
			CDataContainer c = new CDataContainer ();
			c.cdata = doc.CreateCDataSection("data section contents");
			Serialize (c);
			Assert.AreEqual (Infoset ("<CDataContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><cdata><![CDATA[data section contents]]></cdata></CDataContainer>"), WriterText);
		}
		
		[Test]
		public void TestSerializeXmlNode ()
		{
			XmlDocument doc = new XmlDocument ();
			NodeContainer c = new NodeContainer ();
			c.node = doc.CreateTextNode("text");
			Serialize (c);
			Assert.AreEqual (Infoset ("<NodeContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><node>text</node></NodeContainer>"), WriterText);
		}
		
		[Test]
		public void TestSerializeChoice ()
		{
			Choices ch = new Choices ();
			ch.MyChoice = "choice text";
			ch.ItemType = ItemChoiceType.ChoiceZero;
			Serialize (ch);
			Assert.AreEqual (Infoset ("<Choices xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><ChoiceZero>choice text</ChoiceZero></Choices>"), WriterText);
			ch.ItemType = ItemChoiceType.StrangeOne;
			Serialize (ch);
			Assert.AreEqual (Infoset ("<Choices xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><ChoiceOne>choice text</ChoiceOne></Choices>"), WriterText);
			ch.ItemType = ItemChoiceType.ChoiceTwo;
			Serialize (ch);
			Assert.AreEqual (Infoset ("<Choices xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><ChoiceTwo>choice text</ChoiceTwo></Choices>"), WriterText);
		}
		
		[Test]
		public void TestSerializeNamesWithSpaces ()
		{
			TestSpace ts = new TestSpace();
			ts.elem = 4;
			ts.attr = 5;
			Serialize (ts);
			Assert.AreEqual (Infoset ("<Type_x0020_with_x0020_space xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Attribute_x0020_with_x0020_space='5'><Element_x0020_with_x0020_space>4</Element_x0020_with_x0020_space></Type_x0020_with_x0020_space>"), WriterText);
		}
		
		[Test]
		public void TestSerializeReadOnlyProps ()
		{
			ReadOnlyProperties ts = new ReadOnlyProperties();
			Serialize (ts);
			Assert.AreEqual (Infoset ("<ReadOnlyProperties xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
		}
		
		[Test]
		public void TestSerializeIList()
		{
			clsPerson k = new clsPerson();
			k.EmailAccounts = new ArrayList();
			k.EmailAccounts.Add("a");
			k.EmailAccounts.Add("b");
			Serialize (k);
			Assert.AreEqual (Infoset ("<clsPerson xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><EmailAccounts><anyType xsi:type=\"xsd:string\">a</anyType><anyType xsi:type=\"xsd:string\">b</anyType></EmailAccounts></clsPerson>"), WriterText);
		}
		
		[Test]
		public void TestSerializeArrayEnc ()
		{
			SoapReflectionImporter imp = new SoapReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (typeof(ArrayClass));
			XmlSerializer ser = new XmlSerializer (map);
			StringWriter sw = new StringWriter ();
			XmlTextWriter tw = new XmlTextWriter (sw);
			tw.WriteStartElement ("aa");
			ser.Serialize (tw, new ArrayClass ());
			tw.WriteEndElement ();
		}
		
		[Test]
		public void TestIncludeType()
		{
			// Test for bug #76049
			XmlReflectionImporter imp = new XmlReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (typeof(object));
			imp.IncludeType (typeof(TestSpace));
			XmlSerializer ser = new XmlSerializer (map);
			ser.Serialize (new StringWriter (), new TestSpace ());
		}
		
		[Test]
		public void TestSerializeChoiceArray()
		{
			CompositeValueType v = new CompositeValueType ();
			v.Init ();
			Serialize (v);
			Assert.AreEqual (Infoset ("<?xml version=\"1.0\" encoding=\"utf-16\"?><CompositeValueType xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><In>1</In><Es>2</Es></CompositeValueType>"), WriterText);
		}
		
		[Test]
		public void TestArrayAttributeWithDataType ()
		{
			Serialize (new ArrayAttributeWithType ());
			string res = "<ArrayAttributeWithType xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ";
			res += "at='a b' bin1='AQI= AQI=' bin2='AQI=' />";
			Assert.AreEqual (Infoset (res), WriterText);
		}
		
		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void TestArrayAttributeWithWrongDataType ()
		{
			Serialize (new ArrayAttributeWithWrongType ());
		}
		
		[Test]
		public void TestSubclassElementType ()
		{
			SubclassTestContainer c = new SubclassTestContainer ();
			c.data = new SubclassTestSub ();
			Serialize (c);
			
			string res = "<SubclassTestContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			res += "<a xsi:type=\"SubclassTestSub\"/></SubclassTestContainer>";
			Assert.AreEqual (Infoset (res), WriterText);
		}
		
		// Helper methods
				
		}
		
		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void TestArrayAttributeWithWrongDataType ()
		{
			Serialize (new ArrayAttributeWithWrongType ());
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializePrimitiveTypesContainer ()
		{
			Serialize (new PrimitiveTypesContainer ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
#if NET_2_0
				"<PrimitiveTypesContainer xmlns:xsi='{1}' xmlns:xsd='{0}' xmlns='some:urn'>" +
#else
				"<PrimitiveTypesContainer xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns='some:urn'>" +
#endif
				"<Number>2004</Number>" +
				"<Name>some name</Name>" +
				"<Index>56</Index>" +
				"<Password>8w8=</Password>" +
				"<PathSeparatorCharacter>47</PathSeparatorCharacter>" +
				"</PrimitiveTypesContainer>", XmlSchemaNamespace, 
				XmlSchemaInstanceNamespace), sw.ToString (), "#1");

			SerializeEncoded (new PrimitiveTypesContainer ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
#if NET_2_0
				"<q1:PrimitiveTypesContainer xmlns:xsi='{1}' xmlns:xsd='{0}' id='id1' xmlns:q1='{2}'>" +
#else
				"<q1:PrimitiveTypesContainer xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' xmlns:q1='{2}'>" +
#endif
				"<Number xsi:type='xsd:int'>2004</Number>" +
				"<Name xsi:type='xsd:string'>some name</Name>" +
				"<Index xsi:type='xsd:unsignedByte'>56</Index>" +
				"<Password xsi:type='xsd:base64Binary'>8w8=</Password>" +
				"<PathSeparatorCharacter xmlns:q2='{3}' xsi:type='q2:char'>47</PathSeparatorCharacter>" +
				"</q1:PrimitiveTypesContainer>", XmlSchemaNamespace, 
				XmlSchemaInstanceNamespace, AnotherNamespace, WsdlTypesNamespace), 
				sw.ToString (), "#2");
		}

		// Helper methods
				
		public static string Infoset (string sx)
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (sx);
			StringBuilder sb = new StringBuilder ();
			GetInfoset (doc.DocumentElement, sb);
			return sb.ToString ();
		}
		
		public static string Infoset (XmlNode nod)
		{
			StringBuilder sb = new StringBuilder ();
			GetInfoset (nod, sb);
			return sb.ToString ();
		}
		
		static void GetInfoset (XmlNode nod, StringBuilder sb)
		{
			switch (nod.NodeType)
			{
				case XmlNodeType.Attribute:
					if (nod.LocalName == "xmlns" && nod.NamespaceURI == "http://www.w3.org/2000/xmlns/") return;
					sb.Append (" " + nod.NamespaceURI + ":" + nod.LocalName + "='" + nod.Value + "'");
					break;
					
				case XmlNodeType.Element:
					XmlElement elem = (XmlElement) nod;
					sb.Append ("<" + elem.NamespaceURI + ":" + elem.LocalName);
					
					ArrayList ats = new ArrayList ();
					foreach (XmlAttribute at in elem.Attributes)
						ats.Add (at.LocalName + " " + at.NamespaceURI);
						
					ats.Sort ();
						
					foreach (string name in ats)
					{
						string[] nn = name.Split (' ');
						GetInfoset (elem.Attributes[nn[0],nn[1]], sb);
					}
						
					sb.Append (">");
					foreach (XmlNode cn in elem.ChildNodes)
						GetInfoset (cn, sb);
					sb.Append ("</>");
					break;
					
				default:
					sb.Append (nod.OuterXml);
					break;
			}
		}

		static XmlTypeMapping CreateSoapMapping (Type type)
		{
			SoapReflectionImporter importer = new SoapReflectionImporter ();
			return importer.ImportTypeMapping (type);
		}
	}
}
