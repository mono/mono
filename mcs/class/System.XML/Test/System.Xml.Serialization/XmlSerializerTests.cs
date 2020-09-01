//
// System.Xml.XmlSerializerTests
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//   Hagit Yidov <hagity@mainsoft.com>
//
// (C) 2003 Erik LeBel
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Data;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections.Generic;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlSerializerTests
	{
		const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		const string WsdlTypesNamespace = "http://microsoft.com/wsdl/types/";
		const string ANamespace = "some:urn";
		const string AnotherNamespace = "another:urn";

		StringWriter sw;
		XmlTextWriter xtw;
		XmlSerializer xs;

		private void SetUpWriter ()
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
				string val = sw.GetStringBuilder ().ToString ();
				int offset = val.IndexOf ('>') + 1;
				val = val.Substring (offset);
				return Infoset (val);
			}
		}

		private void Serialize (object o)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType ());
			xs.Serialize (xtw, o);
		}

		private void Serialize (object o, Type type)
		{
			SetUpWriter ();
			xs = new XmlSerializer (type);
			xs.Serialize (xtw, o);
		}

		private void Serialize (object o, XmlSerializerNamespaces ns)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType ());
			xs.Serialize (xtw, o, ns);
		}

		private void Serialize (object o, XmlAttributeOverrides ao)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType (), ao);
			xs.Serialize (xtw, o);
		}

		private void Serialize (object o, XmlAttributeOverrides ao, string defaultNamespace)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType (), ao, Type.EmptyTypes,
				(XmlRootAttribute) null, defaultNamespace);
			xs.Serialize (xtw, o);
		}

		private void Serialize (object o, XmlRootAttribute root)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType (), root);
			xs.Serialize (xtw, o);
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

		private void SerializeEncoded (object o, SoapAttributeOverrides ao)
		{
			XmlTypeMapping mapping = CreateSoapMapping (o.GetType (), ao);
			SetUpWriter ();
			xs = new XmlSerializer (mapping);
			xs.Serialize (xtw, o);
		}

		private void SerializeEncoded (object o, SoapAttributeOverrides ao, string defaultNamespace)
		{
			XmlTypeMapping mapping = CreateSoapMapping (o.GetType (), ao, defaultNamespace);
			SetUpWriter ();
			xs = new XmlSerializer (mapping);
			xs.Serialize (xtw, o);
		}

		private void SerializeEncoded (object o, Type type)
		{
			XmlTypeMapping mapping = CreateSoapMapping (type);
			SetUpWriter ();
			xs = new XmlSerializer (mapping);
			xs.Serialize (xtw, o);
		}

		private void SerializeEncoded (XmlTextWriter xtw, object o, Type type)
		{
			XmlTypeMapping mapping = CreateSoapMapping (type);
			xs = new XmlSerializer (mapping);
			xs.Serialize (xtw, o);
		}

		// test constructors
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestConstructor()
		{
			XmlSerializer ser = new XmlSerializer (null, "");
		}

		// test basic types ////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeInt ()
		{
			Serialize (10);
			Assert.AreEqual (Infoset ("<int>10</int>"), WriterText);
		}

		[Test]
		public void TestSerializeBool ()
		{
			Serialize (true);
			Assert.AreEqual (Infoset ("<boolean>true</boolean>"), WriterText);

			Serialize (false);
			Assert.AreEqual (Infoset ("<boolean>false</boolean>"), WriterText);
		}

		[Test]
		public void TestSerializeString ()
		{
			Serialize ("hello");
			Assert.AreEqual (Infoset ("<string>hello</string>"), WriterText);
		}

		[Test]
		public void TestSerializeEmptyString ()
		{
			Serialize (String.Empty);
			Assert.AreEqual (Infoset ("<string />"), WriterText);
		}

		[Test]
		public void TestSerializeNullObject ()
		{
			Serialize (null, typeof (object));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<anyType xmlns:xsd='{0}' xmlns:xsi='{1}' xsi:nil='true' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText);
		}

		[Test]
		[Ignore ("The generated XML is not exact but it is equivalent")]
		public void TestSerializeNullString ()
		{
			Serialize (null, typeof (string));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<string xmlns:xsd='{0}' xmlns:xsi='{1}' xsi:nil='true' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText);
		}

		[Test]
		public void TestSerializeIntArray ()
		{
			Serialize (new int[] { 1, 2, 3, 4 });
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<ArrayOfInt xmlns:xsd='{0}' xmlns:xsi='{1}'><int>1</int><int>2</int><int>3</int><int>4</int></ArrayOfInt>",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText);
		}

		[Test]
		public void TestSerializeEmptyArray ()
		{
			Serialize (new int[] { });
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<ArrayOfInt xmlns:xsd='{0}' xmlns:xsi='{1}' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText);
		}

		[Test]
		public void TestSerializeChar ()
		{
			Serialize ('A');
			Assert.AreEqual (Infoset ("<char>65</char>"), WriterText);

			Serialize ('\0');
			Assert.AreEqual (Infoset ("<char>0</char>"), WriterText);

			Serialize ('\n');
			Assert.AreEqual (Infoset ("<char>10</char>"), WriterText);

			Serialize ('\uFF01');
			Assert.AreEqual (Infoset ("<char>65281</char>"), WriterText);
		}

		[Test]
		public void TestSerializeFloat ()
		{
			Serialize (10.78);
			Assert.AreEqual (Infoset ("<double>10.78</double>"), WriterText);

			Serialize (-1e8);
			Assert.AreEqual (Infoset ("<double>-100000000</double>"), WriterText);

			// FIXME test INF and other boundary conditions that may exist with floats
		}

		[Test]
		public void TestSerializeEnumeration_FromValue ()
		{
			Serialize ((int) SimpleEnumeration.SECOND, typeof (SimpleEnumeration));
			Assert.AreEqual (
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration>SECOND</SimpleEnumeration>",
				sw.ToString ());
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void TestSerializeEnumeration_FromValue_Encoded ()
		{
			SerializeEncoded ((int) SimpleEnumeration.SECOND, typeof (SimpleEnumeration));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>SECOND</SimpleEnumeration>",
				XmlSchema.InstanceNamespace), sw.ToString ());
		}

		[Test]
		public void TestSerializeEnumeration ()
		{
			Serialize (SimpleEnumeration.FIRST);
			Assert.AreEqual (Infoset ("<SimpleEnumeration>FIRST</SimpleEnumeration>"), WriterText, "#1");

			Serialize (SimpleEnumeration.SECOND);
			Assert.AreEqual (Infoset ("<SimpleEnumeration>SECOND</SimpleEnumeration>"), WriterText, "#2");
		}

		[Test]
		public void TestSerializeEnumeration_Encoded ()
		{
			SerializeEncoded (SimpleEnumeration.FIRST);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>FIRST</SimpleEnumeration>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#B1");

			SerializeEncoded (SimpleEnumeration.SECOND);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>SECOND</SimpleEnumeration>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#B2");
		}

		[Test]
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
		[Category ("MobileNotWorking")]
		public void TestSerializeEnumDefaultValue_Encoded ()
		{
			SerializeEncoded (new EnumDefaultValue ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}' />",
				XmlSchema.InstanceNamespace), sw.ToString (), "#1");

			SerializeEncoded (new SimpleEnumeration ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>FIRST</SimpleEnumeration>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#2");

			SerializeEncoded (3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#3");

			SerializeEncoded (EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#4");

			SerializeEncoded (EnumDefaultValue.e1 | EnumDefaultValue.e2, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#5");

			SerializeEncoded (EnumDefaultValue.e1 | EnumDefaultValue.e2 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#6");

			SerializeEncoded (EnumDefaultValue.e1 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#7");

			SerializeEncoded (EnumDefaultValue.e2 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#8");

			SerializeEncoded (3, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e2</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#9");

			SerializeEncoded (5, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e4</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#10");

			SerializeEncoded (FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e4</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#11");

			SerializeEncoded (FlagEnum.e1 | FlagEnum.e2, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e2</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#12");

			SerializeEncoded (FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e2 e4</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#13");

			SerializeEncoded (FlagEnum.e1 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e4</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#14");

			SerializeEncoded (FlagEnum.e2 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e2 e4</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#15");

			SerializeEncoded (3, typeof (EnumDefaultValueNF));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValueNF d1p1:type='EnumDefaultValueNF' xmlns:d1p1='{0}'>e3</EnumDefaultValueNF>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#16");

			SerializeEncoded (EnumDefaultValueNF.e2, typeof (EnumDefaultValueNF));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValueNF d1p1:type='EnumDefaultValueNF' xmlns:d1p1='{0}'>e2</EnumDefaultValueNF>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#17");

			SerializeEncoded (2, typeof (ZeroFlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<ZeroFlagEnum d1p1:type='ZeroFlagEnum' xmlns:d1p1='{0}'>e2</ZeroFlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#18");

			SerializeEncoded (new ZeroFlagEnum ()); // enum actually has a field with value 0
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<ZeroFlagEnum d1p1:type='ZeroFlagEnum' xmlns:d1p1='{0}'>e0</ZeroFlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#19");
		}

		[Test]
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
		public void TestSerializeEnumDefaultValue_InvalidValue2 ()
		{
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
		}

		[Test]
		public void TestSerializeEnumDefaultValueNF_InvalidValue1 ()
		{
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
		}

		[Test]
		public void TestSerializeEnumDefaultValueNF_InvalidValue2 ()
		{
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
		}

		[Test]
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
		public void TestSerializeField ()
		{
			Field f = new Field ();
			Serialize (f, typeof (Field));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='' flag2='' flag3=''" +
				" flag4='' modifiers='public' modifiers2='public' modifiers4='public' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText, "#A");

			f.Flags1 = FlagEnum.e1;
			f.Flags2 = FlagEnum.e1;
			f.Flags3 = FlagEnum.e2;
			f.Modifiers = MapModifiers.Protected;
			f.Modifiers2 = MapModifiers.Public;
			f.Modifiers3 = MapModifiers.Public;
			f.Modifiers4 = MapModifiers.Protected;
			f.Modifiers5 = MapModifiers.Public;
			Serialize (f, typeof (Field));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag3='two' flag4=''" +
				" modifiers='protected' modifiers2='public' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText, "#B");

			f.Flags1 = (FlagEnum) 1;
			f.Flags1 = FlagEnum.e2;
			f.Flags2 = FlagEnum.e2;
			f.Flags3 = FlagEnum.e1 | FlagEnum.e2;
			f.Modifiers = MapModifiers.Public;
			f.Modifiers2 = MapModifiers.Protected;
			f.Modifiers3 = MapModifiers.Protected;
			f.Modifiers4 = MapModifiers.Public;
			f.Modifiers5 = MapModifiers.Protected;
			Serialize (f, typeof (Field));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='two' flag2='two'" +
				" flag4='' modifiers='public' modifiers2='protected'" +
				" modifiers3='protected' modifiers4='public'" +
				" modifiers5='protected' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText, "#C");

			f.Flags1 = FlagEnum.e1 | FlagEnum.e2;
			f.Flags2 = FlagEnum.e2;
			f.Flags3 = FlagEnum.e4;
			f.Flags4 = FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4;
			f.Modifiers3 = MapModifiers.Public;
			f.Modifiers4 = MapModifiers.Protected;
			f.Modifiers5 = MapModifiers.Public;
			f.Names = new string[] { "a", "b" };
			Serialize (f, typeof (Field));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='one two' flag2='two'" +
				" flag3='four' flag4='one two four' modifiers='public'" +
				" modifiers2='protected' names='a b' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText, "#D");

			f.Flags2 = (FlagEnum) 444;
			f.Flags3 = (FlagEnum) 555;
			f.Modifiers = (MapModifiers) 666;
			f.Modifiers2 = (MapModifiers) 777;
			f.Modifiers3 = (MapModifiers) 0;
			f.Modifiers4 = (MapModifiers) 888;
			f.Modifiers5 = (MapModifiers) 999;
			try {
				Serialize (f, typeof (Field));
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsNotNull (ex.InnerException, "#E4");

				// Instance validation error: '444' is not a valid value for
				// MonoTests.System.Xml.TestClasses.FlagEnum
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#E5");
				Assert.IsNotNull (ex.InnerException.Message, "#E6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'444'") != -1, "#E7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#E8");
				Assert.IsNull (ex.InnerException.InnerException, "#E9");
			}
		}

		[Test]
		[Category ("NotWorking")] // MS bug
		public void TestSerializeField_Encoded ()
		{
			Field_Encoded f = new Field_Encoded ();
			SerializeEncoded (f, typeof (Field_Encoded));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag1=''" +
				" flag2='' flag3='' flag4='' modifiers='PuBlIc'" +
				" modifiers2='PuBlIc' modifiers4='PuBlIc' xmlns:q1='some:urn' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace),
				sw.GetStringBuilder ().ToString (), "#A");

			f.Flags1 = FlagEnum_Encoded.e1;
			f.Flags2 = FlagEnum_Encoded.e1;
			f.Flags3 = FlagEnum_Encoded.e2;
			f.Modifiers = MapModifiers.Protected;
			f.Modifiers2 = MapModifiers.Public;
			f.Modifiers3 = MapModifiers.Public;
			f.Modifiers4 = MapModifiers.Protected;
			f.Modifiers5 = MapModifiers.Public;
			SerializeEncoded (f, typeof (Field_Encoded));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag3='two'" +
				" flag4='' modifiers='Protected' modifiers2='PuBlIc'" +
				" xmlns:q1='some:urn' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace),
				sw.GetStringBuilder ().ToString (), "#B");

			f.Flags1 = FlagEnum_Encoded.e2;
			f.Flags2 = FlagEnum_Encoded.e2;
			f.Flags3 = FlagEnum_Encoded.e1 | FlagEnum_Encoded.e2;
			f.Modifiers = MapModifiers.Public;
			f.Modifiers2 = MapModifiers.Protected;
			f.Modifiers3 = MapModifiers.Protected;
			f.Modifiers4 = MapModifiers.Public;
			f.Modifiers5 = MapModifiers.Protected;
			SerializeEncoded (f, typeof (Field_Encoded));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag1='two'" +
				" flag2='two' flag4='' modifiers='PuBlIc' modifiers2='Protected'" +
				" modifiers3='Protected' modifiers4='PuBlIc' modifiers5='Protected'" +
				" xmlns:q1='some:urn' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace),
				sw.GetStringBuilder ().ToString (), "#C");

			f.Flags1 = (FlagEnum_Encoded) 1;
			f.Flags2 = (FlagEnum_Encoded) 444;
			f.Flags3 = (FlagEnum_Encoded) 555;
			f.Modifiers = (MapModifiers) 666;
			f.Modifiers2 = (MapModifiers) 777;
			f.Modifiers3 = (MapModifiers) 0;
			f.Modifiers4 = (MapModifiers) 888;
			f.Modifiers5 = (MapModifiers) 999;
			try {
			SerializeEncoded (f, typeof (Field_Encoded));
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsNotNull (ex.InnerException, "#D4");

				// Instance validation error: '444' is not a valid value for
				// MonoTests.System.Xml.TestClasses.FlagEnum_Encoded
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D5");
				Assert.IsNotNull (ex.InnerException.Message, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'444'") != -1, "#D7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum_Encoded).FullName) != -1, "#D8");
				Assert.IsNull (ex.InnerException.InnerException, "#D9");
			}
		}

		[Test]
		public void TestSerializeGroup ()
		{
			Group myGroup = new Group ();
			myGroup.GroupName = ".NET";

			Byte[] hexByte = new Byte[] { 0x64, 0x32 };
			myGroup.GroupNumber = hexByte;

			DateTime myDate = new DateTime (2002, 5, 2);
			myGroup.Today = myDate;
			myGroup.PostitiveInt = "10000";
			myGroup.IgnoreThis = true;
			Car thisCar = (Car) myGroup.myCar ("1234566");
			myGroup.MyVehicle = thisCar;

			SetUpWriter ();
			xtw.WriteStartDocument (true);
			xtw.WriteStartElement ("Wrapper");
			SerializeEncoded (xtw, myGroup, typeof (Group));
			xtw.WriteEndElement ();
			xtw.Close ();

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns:d2p1='http://www.cpandl.com' CreationDate='2002-05-02' d2p1:GroupName='.NET' GroupNumber='ZDI=' id='id1'>" +
				"<PosInt xsi:type='xsd:nonNegativeInteger'>10000</PosInt>" +
				"<Grouptype xsi:type='GroupType'>Small</Grouptype>" +
				"<MyVehicle href='#id2' />" +
				"</Group>" +
				"<Car xmlns:d2p1='{1}' id='id2' d2p1:type='Car'>" +
				"<licenseNumber xmlns:q1='{0}' d2p1:type='q1:string'>1234566</licenseNumber>" +
				"<makeDate xmlns:q2='{0}' d2p1:type='q2:date'>0001-01-01</makeDate>" +
				"</Car>" +
				"</Wrapper>",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#1");

			myGroup.GroupName = null;
			myGroup.Grouptype = GroupType.B;
			myGroup.MyVehicle.licenseNumber = null;
			myGroup.MyVehicle.weight = "450";

			SetUpWriter ();
			xtw.WriteStartDocument (true);
			xtw.WriteStartElement ("Wrapper");
			SerializeEncoded (xtw, myGroup, typeof (Group));
			xtw.WriteEndElement ();
			xtw.Close ();

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' CreationDate='2002-05-02' GroupNumber='ZDI=' id='id1'>" +
				"<PosInt xsi:type='xsd:nonNegativeInteger'>10000</PosInt>" +
				"<Grouptype xsi:type='GroupType'>Large</Grouptype>" +
				"<MyVehicle href='#id2' />" +
				"</Group>" +
				"<Car xmlns:d2p1='{1}' id='id2' d2p1:type='Car'>" +
				"<makeDate xmlns:q1='{0}' d2p1:type='q1:date'>0001-01-01</makeDate>" +
				"<weight xmlns:q2='{0}' d2p1:type='q2:string'>450</weight>" +
				"</Car>" +
				"</Wrapper>",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#2");
		}

		[Test]
		public void TestSerializeZeroFlagEnum_InvalidValue ()
		{
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
		}

		[Test]
		public void TestSerializeQualifiedName ()
		{
			Serialize (new XmlQualifiedName ("me", "home.urn"));
			Assert.AreEqual (Infoset ("<QName xmlns:q1='home.urn'>q1:me</QName>"), WriterText);
		}

		[Test]
		public void TestSerializeBytes ()
		{
			Serialize ((byte) 0xAB);
			Assert.AreEqual (Infoset ("<unsignedByte>171</unsignedByte>"), WriterText);

			Serialize ((byte) 15);
			Assert.AreEqual (Infoset ("<unsignedByte>15</unsignedByte>"), WriterText);
		}

		[Test]
		public void TestSerializeByteArrays ()
		{
			Serialize (new byte[] { });
			Assert.AreEqual (Infoset ("<base64Binary />"), WriterText);

			Serialize (new byte[] { 0xAB, 0xCD });
			Assert.AreEqual (Infoset ("<base64Binary>q80=</base64Binary>"), WriterText);
		}

		[Test]
		public void TestSerializeDateTime ()
		{
			DateTime d = new DateTime ();
			Serialize (d);

			TimeZone tz = TimeZone.CurrentTimeZone;
			TimeSpan off = tz.GetUtcOffset (d);
			string sp = string.Format ("{0}{1:00}:{2:00}", off.Ticks >= 0 ? "+" : "", off.Hours, off.Minutes);
			Assert.AreEqual (Infoset ("<dateTime>0001-01-01T00:00:00</dateTime>"), WriterText);
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
		public void TestSerializeSimpleClass ()
		{
			SimpleClass simple = new SimpleClass ();
			Serialize (simple);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			simple.something = "hello";

			Serialize (simple);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>"), WriterText);
		}

		[Test]
		public void TestSerializeStringCollection ()
		{
			StringCollection strings = new StringCollection ();
			Serialize (strings);
			Assert.AreEqual (Infoset ("<ArrayOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			strings.Add ("hello");
			strings.Add ("goodbye");
			Serialize (strings);
			Assert.AreEqual (Infoset ("<ArrayOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><string>hello</string><string>goodbye</string></ArrayOfString>"), WriterText);
		}

		[Test]
		public void TestSerializeOptionalValueTypeContainer ()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr;
			OptionalValueTypeContainer optionalValue = new OptionalValueTypeContainer ();

			Serialize (optionalValue);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<optionalValue xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns='{2}' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace, AnotherNamespace),
				sw.ToString (), "#1");

			attr = new XmlAttributes ();

			// remove the DefaultValue attribute on the Flags member
			overrides.Add (typeof (OptionalValueTypeContainer), "Flags", attr);
			// remove the DefaultValue attribute on the Attributes member
			overrides.Add (typeof (OptionalValueTypeContainer), "Attributes", attr);

			Serialize (optionalValue, overrides);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<optionalValue xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns='{2}'>" +
				"<Attributes xmlns='{3}'>one four</Attributes>" +
				"</optionalValue>", XmlSchema.Namespace, XmlSchema.InstanceNamespace,
				AnotherNamespace, ANamespace), sw.ToString (), "#2");

			optionalValue.FlagsSpecified = true;
			Serialize (optionalValue, overrides);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<optionalValue xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns='{2}'>" +
				"<Attributes xmlns='{3}'>one four</Attributes>" +
				"<Flags xmlns='{3}'>one</Flags>" +
				"</optionalValue>",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace, AnotherNamespace,
				ANamespace), sw.ToString (), "#3");
		}
		
		[Test]
		public void TestRoundTripSerializeOptionalValueTypeContainer ()
		{
			var source = new OptionalValueTypeContainer ();
			source.IsEmpty = true;
			source.IsEmptySpecified = true;
			var ser = new XmlSerializer (typeof (OptionalValueTypeContainer));
			string xml;
			using (var t = new StringWriter ()) {
				ser.Serialize (t, source);
				xml = t.ToString();
			}
			using (var s = new StringReader (xml)) {
				var obj = (OptionalValueTypeContainer) ser.Deserialize(s);
				Assert.AreEqual (source.IsEmpty, obj.IsEmpty, "#1");
				Assert.AreEqual (source.IsEmptySpecified, obj.IsEmptySpecified, "#2");
			}
		}
		
		[Test]
		public void TestSerializePlainContainer ()
		{
			StringCollectionContainer container = new StringCollectionContainer ();
			Serialize (container);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages /></StringCollectionContainer>"), WriterText);

			container.Messages.Add ("hello");
			container.Messages.Add ("goodbye");
			Serialize (container);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages><string>hello</string><string>goodbye</string></Messages></StringCollectionContainer>"), WriterText);
		}

		[Test]
		public void TestSerializeArrayContainer ()
		{
			ArrayContainer container = new ArrayContainer ();
			Serialize (container);
			Assert.AreEqual (Infoset ("<ArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			container.items = new object[] { 10, 20 };
			Serialize (container);
			Assert.AreEqual (Infoset ("<ArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><anyType xsi:type='xsd:int'>10</anyType><anyType xsi:type='xsd:int'>20</anyType></items></ArrayContainer>"), WriterText);

			container.items = new object[] { 10, "hello" };
			Serialize (container);
			Assert.AreEqual (Infoset ("<ArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><anyType xsi:type='xsd:int'>10</anyType><anyType xsi:type='xsd:string'>hello</anyType></items></ArrayContainer>"), WriterText);
		}

		[Test]
		public void TestSerializeClassArrayContainer ()
		{
			ClassArrayContainer container = new ClassArrayContainer ();
			Serialize (container);
			Assert.AreEqual (Infoset ("<ClassArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			SimpleClass simple1 = new SimpleClass ();
			simple1.something = "hello";
			SimpleClass simple2 = new SimpleClass ();
			simple2.something = "hello";
			container.items = new SimpleClass[2];
			container.items[0] = simple1;
			container.items[1] = simple2;
			Serialize (container);
			Assert.AreEqual (Infoset ("<ClassArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><SimpleClass><something>hello</something></SimpleClass><SimpleClass><something>hello</something></SimpleClass></items></ClassArrayContainer>"), WriterText);
		}

		// test basic attributes ///////////////////////////////////////////////
		[Test]
		public void TestSerializeSimpleClassWithXmlAttributes ()
		{
			SimpleClassWithXmlAttributes simple = new SimpleClassWithXmlAttributes ();
			Serialize (simple);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			simple.something = "hello";
			Serialize (simple);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' member='hello' />"), WriterText);
		}

		// test overrides ///////////////////////////////////////////////////////
		[Test]
		public void TestSerializeSimpleClassWithOverrides ()
		{
			// Also tests XmlIgnore
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();

			XmlAttributes attr = new XmlAttributes ();
			attr.XmlIgnore = true;
			overrides.Add (typeof (SimpleClassWithXmlAttributes), "something", attr);

			SimpleClassWithXmlAttributes simple = new SimpleClassWithXmlAttributes ();
			simple.something = "hello";
			Serialize (simple, overrides);
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
		public void TestSerializeXmlTextAttribute ()
		{
			SimpleClass simple = new SimpleClass ();
			simple.something = "hello";

			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			overrides.Add (typeof (SimpleClass), "something", attr);

			attr.XmlText = new XmlTextAttribute ();
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>hello</SimpleClass>"), WriterText, "#1");

			attr.XmlText = new XmlTextAttribute (typeof (string));
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>hello</SimpleClass>"), WriterText, "#2");

			try {
				attr.XmlText = new XmlTextAttribute (typeof (byte[]));
				Serialize (simple, overrides);
				Assert.Fail ("#A1: XmlText.Type does not match the type it serializes: this should have failed");
			} catch (InvalidOperationException ex) {
				// there was an error reflecting type 'MonoTests.System.Xml.TestClasses.SimpleClass'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (SimpleClass).FullName) != -1, "#A4");
				Assert.IsNotNull (ex.InnerException, "#A5");

				// there was an error reflecting field 'something'.
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#A6");
				Assert.IsNotNull (ex.InnerException.Message, "#A7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("something") != -1, "#A8");
				Assert.IsNotNull (ex.InnerException.InnerException, "#A9");

				// the type for XmlText may not be specified for primitive types.
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.InnerException.GetType (), "#A10");
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#A11");
				Assert.IsNull (ex.InnerException.InnerException.InnerException, "#A12");
			}

			try {
				attr.XmlText = new XmlTextAttribute ();
				attr.XmlText.DataType = "sometype";
				Serialize (simple, overrides);
				Assert.Fail ("#B1: XmlText.DataType does not match the type it serializes: this should have failed");
			} catch (InvalidOperationException ex) {
				// There was an error reflecting type 'MonoTests.System.Xml.TestClasses.SimpleClass'.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (SimpleClass).FullName) != -1, "#B4");
				Assert.IsNotNull (ex.InnerException, "#B5");

				// There was an error reflecting field 'something'.
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#B6");
				Assert.IsNotNull (ex.InnerException.Message, "#B7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("something") != -1, "#B8");
				Assert.IsNotNull (ex.InnerException.InnerException, "#B9");

				//FIXME
				/*
				// There was an error reflecting type 'System.String'.
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.InnerException.GetType (), "#B10");
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#B11");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf (typeof (string).FullName) != -1, "#B12");
				Assert.IsNotNull (ex.InnerException.InnerException.InnerException, "#B13");

				// Value 'sometype' cannot be used for the XmlElementAttribute.DataType property. 
				// The datatype 'http://www.w3.org/2001/XMLSchema:sometype' is missing.
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.InnerException.InnerException.GetType (), "#B14");
				Assert.IsNotNull (ex.InnerException.InnerException.InnerException.Message, "#B15");
				Assert.IsTrue (ex.InnerException.InnerException.InnerException.Message.IndexOf ("http://www.w3.org/2001/XMLSchema:sometype") != -1, "#B16");
				Assert.IsNull (ex.InnerException.InnerException.InnerException.InnerException, "#B17");
				*/
			}
		}

		// test xmlRoot //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlRootAttribute ()
		{
			// constructor override & element name
			XmlRootAttribute root = new XmlRootAttribute ();
			root.ElementName = "renamed";

			SimpleClassWithXmlAttributes simpleWithAttributes = new SimpleClassWithXmlAttributes ();
			Serialize (simpleWithAttributes, root);
			Assert.AreEqual (Infoset ("<renamed xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			SimpleClass simple = null;
			root.IsNullable = false;
			try {
				Serialize (simple, root);
				Assert.Fail ("Cannot serialize null object if XmlRoot's IsNullable == false");
			} catch (NullReferenceException) {
			}

			root.IsNullable = true;
			try {
				Serialize (simple, root);
				Assert.Fail ("Cannot serialize null object if XmlRoot's IsNullable == true");
			} catch (NullReferenceException) {
			}

			simple = new SimpleClass ();
			root.ElementName = null;
			root.Namespace = "some.urn";
			Serialize (simple, root);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='some.urn' />"), WriterText);
		}

		[Test]
		public void TestSerializeXmlRootAttributeOnMember ()
		{
			// nested root
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes childAttr = new XmlAttributes ();
			childAttr.XmlRoot = new XmlRootAttribute ("simple");
			overrides.Add (typeof (SimpleClass), childAttr);

			XmlAttributes attr = new XmlAttributes ();
			attr.XmlRoot = new XmlRootAttribute ("simple");
			overrides.Add (typeof (ClassArrayContainer), attr);

			ClassArrayContainer container = new ClassArrayContainer ();
			container.items = new SimpleClass[1];
			container.items[0] = new SimpleClass ();
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><SimpleClass /></items></simple>"), WriterText);

			// FIXME test data type
		}

		// test XmlAttribute /////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlAttributeAttribute ()
		{
			// null
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			attr.XmlAttribute = new XmlAttributeAttribute ();
			overrides.Add (typeof (SimpleClass), "something", attr);

			SimpleClass simple = new SimpleClass (); ;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#1");

			// regular
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' something='hello' />"), WriterText, "#2");

			// AttributeName
			attr.XmlAttribute.AttributeName = "somethingelse";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' somethingelse='hello' />"), WriterText, "#3");

			// Type
			// FIXME this should work, shouldnt it?
			// attr.XmlAttribute.Type = typeof(string);
			// Serialize(simple, overrides);
			// Assert(WriterText.EndsWith(" something='hello' />"));

			// Namespace
			attr.XmlAttribute.Namespace = "some:urn";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' d1p1:somethingelse='hello' xmlns:d1p1='some:urn' />"), WriterText, "#4");

			// FIXME DataType
			// FIXME XmlSchemaForm Form

			// FIXME write XmlQualifiedName as attribute
		}

		// test XmlElement ///////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlElementAttribute ()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			XmlElementAttribute element = new XmlElementAttribute ();
			attr.XmlElements.Add (element);
			overrides.Add (typeof (SimpleClass), "something", attr);

			// null
			SimpleClass simple = new SimpleClass (); ;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#1");

			// not null
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>"), WriterText, "#2");

			//ElementName
			element.ElementName = "saying";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><saying>hello</saying></SimpleClass>"), WriterText, "#3");

			//IsNullable
			element.IsNullable = false;
			simple.something = null;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#4");

			element.IsNullable = true;
			simple.something = null;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><saying xsi:nil='true' /></SimpleClass>"), WriterText, "#5");

			//Namespace
			element.ElementName = null;
			element.IsNullable = false;
			element.Namespace = "some:urn";
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something xmlns='some:urn'>hello</something></SimpleClass>"), WriterText, "#6");

			//FIXME DataType
			//FIXME Form
			//FIXME Type
		}

		// test XmlElementAttribute with arrays and collections //////////////////
		[Test]
		public void TestSerializeCollectionWithXmlElementAttribute ()
		{
			// the rule is:
			// if no type is specified or the specified type 
			//    matches the contents of the collection, 
			//    serialize each element in an element named after the member.
			// if the type does not match, or matches the collection itself,
			//    create a base wrapping element for the member, and then
			//    wrap each collection item in its own wrapping element based on type.

			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			XmlElementAttribute element = new XmlElementAttribute ();
			attr.XmlElements.Add (element);
			overrides.Add (typeof (StringCollectionContainer), "Messages", attr);

			// empty collection & no type info in XmlElementAttribute
			StringCollectionContainer container = new StringCollectionContainer ();
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#1");

			// non-empty collection & no type info in XmlElementAttribute
			container.Messages.Add ("hello");
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages>hello</Messages></StringCollectionContainer>"), WriterText, "#2");

			// non-empty collection & only type info in XmlElementAttribute
			element.Type = typeof (StringCollection);
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages><string>hello</string></Messages></StringCollectionContainer>"), WriterText, "#3");

			// non-empty collection & only type info in XmlElementAttribute
			element.Type = typeof (string);
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages>hello</Messages></StringCollectionContainer>"), WriterText, "#4");

			// two elements
			container.Messages.Add ("goodbye");
			element.Type = null;
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages>hello</Messages><Messages>goodbye</Messages></StringCollectionContainer>"), WriterText, "#5");
		}

		// test DefaultValue /////////////////////////////////////////////////////
		[Test]
		public void TestSerializeDefaultValueAttribute ()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();

			XmlAttributes attr = new XmlAttributes ();
			string defaultValueInstance = "nothing";
			attr.XmlDefaultValue = defaultValueInstance;
			overrides.Add (typeof (SimpleClass), "something", attr);

			// use the default
			SimpleClass simple = new SimpleClass ();
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#A1");

			// same value as default
			simple.something = defaultValueInstance;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#A2");

			// some other value
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>"), WriterText, "#A3");

			overrides = new XmlAttributeOverrides ();
			attr = new XmlAttributes ();
			attr.XmlAttribute = new XmlAttributeAttribute ();
			attr.XmlDefaultValue = defaultValueInstance;
			overrides.Add (typeof (SimpleClass), "something", attr);

			// use the default
			simple = new SimpleClass ();
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#B1");

			// same value as default
			simple.something = defaultValueInstance;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#B2");

			// some other value
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass something='hello' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#B3");

			overrides = new XmlAttributeOverrides ();
			attr = new XmlAttributes ();
			attr.XmlAttribute = new XmlAttributeAttribute ("flagenc");
			overrides.Add (typeof (TestDefault), "flagencoded", attr);

			// use the default
			TestDefault testDefault = new TestDefault ();
			Serialize (testDefault);
			Assert.AreEqual (Infoset ("<testDefault xmlns='urn:myNS' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#C1");

			// use the default with overrides
			Serialize (testDefault, overrides);
			Assert.AreEqual (Infoset ("<testDefault flagenc='e1 e4' xmlns='urn:myNS' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#C2");

			overrides = new XmlAttributeOverrides ();
			attr = new XmlAttributes ();
			attr.XmlAttribute = new XmlAttributeAttribute ("flagenc");
			attr.XmlDefaultValue = (FlagEnum_Encoded.e1 | FlagEnum_Encoded.e4); // add default again
			overrides.Add (typeof (TestDefault), "flagencoded", attr);

			// use the default with overrides
			Serialize (testDefault, overrides);
			Assert.AreEqual (Infoset ("<testDefault xmlns='urn:myNS' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#C3");

			// use the default with overrides and default namspace
			Serialize (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset ("<testDefault xmlns='urn:myNS' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#C4");

			// non-default values
			testDefault.strDefault = "Some Text";
			testDefault.boolT = false;
			testDefault.boolF = true;
			testDefault.decimalval = 20m;
			testDefault.flag = FlagEnum.e2;
			testDefault.flagencoded = FlagEnum_Encoded.e2 | FlagEnum_Encoded.e1;
			Serialize (testDefault);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<testDefault xmlns='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault>Some Text</strDefault>" +
				"    <boolT>false</boolT>" +
				"    <boolF>true</boolF>" +
				"    <decimalval>20</decimalval>" +
				"    <flag>two</flag>" +
				"    <flagencoded>e1 e2</flagencoded>" +
				"</testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C5");

			Serialize (testDefault, overrides);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<testDefault flagenc='e1 e2' xmlns='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault>Some Text</strDefault>" +
				"    <boolT>false</boolT>" +
				"    <boolF>true</boolF>" +
				"    <decimalval>20</decimalval>" +
				"    <flag>two</flag>" +
				"</testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C6");

			Serialize (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<testDefault flagenc='e1 e2' xmlns='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault>Some Text</strDefault>" +
				"    <boolT>false</boolT>" +
				"    <boolF>true</boolF>" +
				"    <decimalval>20</decimalval>" +
				"    <flag>two</flag>" +
				"</testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C7");

			attr = new XmlAttributes ();
			XmlTypeAttribute xmlType = new XmlTypeAttribute ("flagenum");
			xmlType.Namespace = "yetanother:urn";
			attr.XmlType = xmlType;
			overrides.Add (typeof (FlagEnum_Encoded), attr);

			Serialize (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<testDefault flagenc='e1 e2' xmlns='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault>Some Text</strDefault>" +
				"    <boolT>false</boolT>" +
				"    <boolF>true</boolF>" +
				"    <decimalval>20</decimalval>" +
				"    <flag>two</flag>" +
				"</testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C8");

			attr = new XmlAttributes ();
			attr.XmlType = new XmlTypeAttribute ("testDefault");
			overrides.Add (typeof (TestDefault), attr);

			Serialize (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<testDefault flagenc='e1 e2' xmlns='{2}' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault>Some Text</strDefault>" +
				"    <boolT>false</boolT>" +
				"    <boolF>true</boolF>" +
				"    <decimalval>20</decimalval>" +
				"    <flag>two</flag>" +
				"</testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace,
				AnotherNamespace)), WriterText, "#C9");
		}

		[Test]
		public void TestSerializeDefaultValueAttribute_Encoded ()
		{
			SoapAttributeOverrides overrides = new SoapAttributeOverrides ();
			SoapAttributes attr = new SoapAttributes ();
			attr.SoapAttribute = new SoapAttributeAttribute ();
			string defaultValueInstance = "nothing";
			attr.SoapDefaultValue = defaultValueInstance;
			overrides.Add (typeof (SimpleClass), "something", attr);

			// use the default
			SimpleClass simple = new SimpleClass ();
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#A1");

			// same value as default
			simple.something = defaultValueInstance;
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#A2");

			// some other value
			simple.something = "hello";
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' something='hello' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#A3");

			attr.SoapAttribute = null;
			attr.SoapElement = new SoapElementAttribute ();

			// use the default
			simple = new SimpleClass ();
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#B1");

			// same value as default
			simple.something = defaultValueInstance;
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something xsi:type='xsd:string'>nothing</something></SimpleClass>"), WriterText, "#B2");

			// some other value
			simple.something = "hello";
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something xsi:type='xsd:string'>hello</something></SimpleClass>"), WriterText, "#B3");

			overrides = new SoapAttributeOverrides ();
			attr = new SoapAttributes ();
			attr.SoapElement = new SoapElementAttribute ("flagenc");
			overrides.Add (typeof (TestDefault), "flagencoded", attr);

			// use the default (from MS KB325691)
			TestDefault testDefault = new TestDefault ();
			SerializeEncoded (testDefault);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Default Value</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>true</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>false</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>10</decimalval>" +
				"    <flag xsi:type='FlagEnum'>e1 e4</flag>" +
				"    <flagencoded xsi:type='flagenum'>one four</flagencoded>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C1");

			SerializeEncoded (testDefault, overrides);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Default Value</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>true</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>false</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>10</decimalval>" +
				"    <flag xsi:type='FlagEnum'>e1 e4</flag>" +
				"    <flagenc xsi:type='flagenum'>one four</flagenc>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C2");

			SerializeEncoded (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Default Value</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>true</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>false</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>10</decimalval>" +
				"    <flag xmlns:q2='{2}' xsi:type='q2:FlagEnum'>e1 e4</flag>" +
				"    <flagenc xmlns:q3='{2}' xsi:type='q3:flagenum'>one four</flagenc>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace,
				AnotherNamespace)), WriterText, "#C3");

			// non-default values
			testDefault.strDefault = "Some Text";
			testDefault.boolT = false;
			testDefault.boolF = true;
			testDefault.decimalval = 20m;
			testDefault.flag = FlagEnum.e2;
			testDefault.flagencoded = FlagEnum_Encoded.e2 | FlagEnum_Encoded.e1;
			SerializeEncoded (testDefault);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Some Text</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>false</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>true</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>20</decimalval>" +
				"    <flag xsi:type='FlagEnum'>e2</flag>" +
				"    <flagencoded xsi:type='flagenum'>one two</flagencoded>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C4");

			SerializeEncoded (testDefault, overrides);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Some Text</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>false</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>true</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>20</decimalval>" +
				"    <flag xsi:type='FlagEnum'>e2</flag>" +
				"    <flagenc xsi:type='flagenum'>one two</flagenc>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C5");

			attr = new SoapAttributes ();
			attr.SoapType = new SoapTypeAttribute ("flagenum", "yetanother:urn");
			overrides.Add (typeof (FlagEnum_Encoded), attr);

			SerializeEncoded (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Some Text</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>false</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>true</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>20</decimalval>" +
				"    <flag xmlns:q2='{2}' xsi:type='q2:FlagEnum'>e2</flag>" +
				"    <flagenc xmlns:q3='yetanother:urn' xsi:type='q3:flagenum'>one two</flagenc>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace,
				AnotherNamespace)), WriterText, "#C6");

			attr = new SoapAttributes ();
			attr.SoapType = new SoapTypeAttribute ("testDefault");
			overrides.Add (typeof (TestDefault), attr);

			SerializeEncoded (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='{2}' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Some Text</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>false</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>true</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>20</decimalval>" +
				"    <flag xsi:type='q1:FlagEnum'>e2</flag>" +
				"    <flagenc xmlns:q2='yetanother:urn' xsi:type='q2:flagenum'>one two</flagenc>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace,
				AnotherNamespace)), WriterText, "#C7");
		}

		// test XmlEnum //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlEnumAttribute ()
		{
			Serialize (XmlSchemaForm.Qualified);
			Assert.AreEqual (Infoset ("<XmlSchemaForm>qualified</XmlSchemaForm>"), WriterText, "#1");

			Serialize (XmlSchemaForm.Unqualified);
			Assert.AreEqual (Infoset ("<XmlSchemaForm>unqualified</XmlSchemaForm>"), WriterText, "#2");
		}

		[Test]
		public void TestSerializeXmlEnumAttribute_IgnoredValue ()
		{
			// technically XmlSchemaForm.None has an XmlIgnore attribute,
			// but it is not being serialized as a member.

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
		}

		[Test]
		public void TestSerializeXmlNodeArray ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new XmlNode[] { doc.CreateAttribute ("at"), doc.CreateElement ("elem1"), doc.CreateElement ("elem2") }, typeof (object));
			Assert.AreEqual (Infoset ("<anyType at=\"\"><elem1/><elem2/></anyType>"), WriterText);
		}

		[Test]
		public void TestSerializeXmlNodeArray2 ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new XmlNode[] { doc.CreateElement ("elem1"), doc.CreateElement ("elem2") }, typeof (XmlNode []));
			Assert.AreEqual (Infoset (String.Format ("<ArrayOfXmlNode xmlns:xsd='{0}' xmlns:xsi='{1}'><XmlNode><elem1/></XmlNode><XmlNode><elem2/></XmlNode></ArrayOfXmlNode>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("MobileNotWorking")]
		public void TestSerializeXmlNodeArrayIncludesAttribute ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new XmlNode[] { doc.CreateAttribute ("at"), doc.CreateElement ("elem1"), doc.CreateElement ("elem2") }, typeof (XmlNode []));
		}

		[Test]
		public void TestSerializeXmlElementArray ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new XmlElement[] { doc.CreateElement ("elem1"), doc.CreateElement ("elem2") }, typeof (object));
			Assert.AreEqual (Infoset ("<anyType><elem1/><elem2/></anyType>"), WriterText);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // List<XmlNode> is not supported
		public void TestSerializeGenericListOfNode ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new List<XmlNode> (new XmlNode [] { doc.CreateAttribute ("at"), doc.CreateElement ("elem1"), doc.CreateElement ("elem2") }), typeof (object));
			Assert.AreEqual (Infoset ("<anyType at=\"\"><elem1/><elem2/></anyType>"), WriterText);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // List<XmlElement> is not supported
		public void TestSerializeGenericListOfElement ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new List<XmlElement> (new XmlElement [] { doc.CreateElement ("elem1"), doc.CreateElement ("elem2") }), typeof (object));
			Assert.AreEqual (Infoset ("<anyType><elem1/><elem2/></anyType>"), WriterText);
		}
		[Test]
		public void TestSerializeXmlDocument ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (@"<?xml version=""1.0"" encoding=""utf-8"" ?><root/>");
			Serialize (doc, typeof (XmlDocument));
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><root />",
				sw.GetStringBuilder ().ToString ());
		}

		[Test]
		public void TestSerializeXmlElement ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (doc.CreateElement ("elem"), typeof (XmlElement));
			Assert.AreEqual (Infoset ("<elem/>"), WriterText);
		}

		[Test]
		public void TestSerializeXmlElementSubclass ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new MyElem (doc), typeof (XmlElement));
			Assert.AreEqual (Infoset ("<myelem aa=\"1\"/>"), WriterText, "#1");

			Serialize (new MyElem (doc), typeof (MyElem));
			Assert.AreEqual (Infoset ("<myelem aa=\"1\"/>"), WriterText, "#2");
		}

		[Test]
		public void TestSerializeXmlCDataSection ()
		{
			XmlDocument doc = new XmlDocument ();
			CDataContainer c = new CDataContainer ();
			c.cdata = doc.CreateCDataSection ("data section contents");
			Serialize (c);
			Assert.AreEqual (Infoset ("<CDataContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><cdata><![CDATA[data section contents]]></cdata></CDataContainer>"), WriterText);
		}

		[Test]
		public void TestSerializeXmlNode ()
		{
			XmlDocument doc = new XmlDocument ();
			NodeContainer c = new NodeContainer ();
			c.node = doc.CreateTextNode ("text");
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
			Assert.AreEqual (Infoset ("<Choices xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><ChoiceZero>choice text</ChoiceZero></Choices>"), WriterText, "#1");
			ch.ItemType = ItemChoiceType.StrangeOne;
			Serialize (ch);
			Assert.AreEqual (Infoset ("<Choices xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><ChoiceOne>choice text</ChoiceOne></Choices>"), WriterText, "#2");
			ch.ItemType = ItemChoiceType.ChoiceTwo;
			Serialize (ch);
			Assert.AreEqual (Infoset ("<Choices xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><ChoiceTwo>choice text</ChoiceTwo></Choices>"), WriterText, "#3");
		}

		[Test]
		public void TestSerializeNamesWithSpaces ()
		{
			TestSpace ts = new TestSpace ();
			ts.elem = 4;
			ts.attr = 5;
			Serialize (ts);
			Assert.AreEqual (Infoset ("<Type_x0020_with_x0020_space xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Attribute_x0020_with_x0020_space='5'><Element_x0020_with_x0020_space>4</Element_x0020_with_x0020_space></Type_x0020_with_x0020_space>"), WriterText);
		}

		[Test]
		public void TestSerializeReadOnlyProps ()
		{
			ReadOnlyProperties ts = new ReadOnlyProperties ();
			Serialize (ts);
			Assert.AreEqual (Infoset ("<ReadOnlyProperties xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
		}

		[Test]
		public void TestSerializeReadOnlyListProp ()
		{
			ReadOnlyListProperty ts = new ReadOnlyListProperty ();
			Serialize (ts);
			Assert.AreEqual (Infoset ("<ReadOnlyListProperty xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><StrList><string>listString1</string><string>listString2</string></StrList></ReadOnlyListProperty>"), WriterText);
		}


		[Test]
		public void TestSerializeIList ()
		{
			clsPerson k = new clsPerson ();
			k.EmailAccounts = new ArrayList ();
			k.EmailAccounts.Add ("a");
			k.EmailAccounts.Add ("b");
			Serialize (k);
			Assert.AreEqual (Infoset ("<clsPerson xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><EmailAccounts><anyType xsi:type=\"xsd:string\">a</anyType><anyType xsi:type=\"xsd:string\">b</anyType></EmailAccounts></clsPerson>"), WriterText);
		}

		[Test]
		public void TestSerializeArrayEnc ()
		{
			SoapReflectionImporter imp = new SoapReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (typeof (ArrayClass));
			XmlSerializer ser = new XmlSerializer (map);
			StringWriter sw = new StringWriter ();
			XmlTextWriter tw = new XmlTextWriter (sw);
			tw.WriteStartElement ("aa");
			ser.Serialize (tw, new ArrayClass ());
			tw.WriteEndElement ();
		}

		[Test] // bug #76049
		public void TestIncludeType ()
		{
			XmlReflectionImporter imp = new XmlReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (typeof (object));
			imp.IncludeType (typeof (TestSpace));
			XmlSerializer ser = new XmlSerializer (map);
			ser.Serialize (new StringWriter (), new TestSpace ());
		}

		[Test]
		public void TestSerializeChoiceArray ()
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
		public void TestSubclassElementType ()
		{
			SubclassTestContainer c = new SubclassTestContainer ();
			c.data = new SubclassTestSub ();
			Serialize (c);

			string res = "<SubclassTestContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			res += "<a xsi:type=\"SubclassTestSub\"/></SubclassTestContainer>";
			Assert.AreEqual (Infoset (res), WriterText);
		}

		[Test] // Covers #36829
		public void TestSubclassElementList ()
		{
			var o = new SubclassTestList () { Items = new List<object> () { new SubclassTestSub () } };
			Serialize (o);

			string res = "<SubclassTestList xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			res += "<b xsi:type=\"SubclassTestSub\"/></SubclassTestList>";
			Assert.AreEqual (Infoset (res), WriterText);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
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
				"<PrimitiveTypesContainer xmlns:xsi='{1}' xmlns:xsd='{0}' xmlns='some:urn'>" +
				"<Number>2004</Number>" +
				"<Name>some name</Name>" +
				"<Index>56</Index>" +
				"<Password>8w8=</Password>" +
				"<PathSeparatorCharacter>47</PathSeparatorCharacter>" +
				"</PrimitiveTypesContainer>", XmlSchema.Namespace,
				XmlSchema.InstanceNamespace), sw.ToString (), "#1");

			SerializeEncoded (new PrimitiveTypesContainer ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:PrimitiveTypesContainer xmlns:xsi='{1}' xmlns:xsd='{0}' id='id1' xmlns:q1='{2}'>" +
				"<Number xsi:type='xsd:int'>2004</Number>" +
				"<Name xsi:type='xsd:string'>some name</Name>" +
				"<Index xsi:type='xsd:unsignedByte'>56</Index>" +
				"<Password xsi:type='xsd:base64Binary'>8w8=</Password>" +
				"<PathSeparatorCharacter xmlns:q2='{3}' xsi:type='q2:char'>47</PathSeparatorCharacter>" +
				"</q1:PrimitiveTypesContainer>", XmlSchema.Namespace,
				XmlSchema.InstanceNamespace, AnotherNamespace, WsdlTypesNamespace),
				sw.ToString (), "#2");
		}

		[Test]
		public void TestSchemaForm ()
		{
			TestSchemaForm1 t1 = new TestSchemaForm1 ();
			t1.p1 = new PrintTypeResponse ();
			t1.p1.Init ();
			t1.p2 = new PrintTypeResponse ();
			t1.p2.Init ();

			TestSchemaForm2 t2 = new TestSchemaForm2 ();
			t2.p1 = new PrintTypeResponse ();
			t2.p1.Init ();
			t2.p2 = new PrintTypeResponse ();
			t2.p2.Init ();

			Serialize (t1);
			string res = "";
			res += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
			res += "<TestSchemaForm1 xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";
			res += "  <p1>";
			res += "    <result>";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p1>";
			res += "  <p2 xmlns=\"urn:oo\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p2>";
			res += "</TestSchemaForm1>";
			Assert.AreEqual (Infoset (res), WriterText);

			Serialize (t2);
			res = "";
			res += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
			res += "<TestSchemaForm2 xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";
			res += "  <p1 xmlns=\"urn:testForm\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p1>";
			res += "  <p2 xmlns=\"urn:oo\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p2>";
			res += "</TestSchemaForm2>";
			Assert.AreEqual (Infoset (res), WriterText);

			XmlReflectionImporter imp = new XmlReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (typeof (TestSchemaForm1), "urn:extra");
			Serialize (t1, map);
			res = "";
			res += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
			res += "<TestSchemaForm1 xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:extra\">";
			res += "  <p1>";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p1>";
			res += "  <p2 xmlns=\"urn:oo\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p2>";
			res += "</TestSchemaForm1>";
			Assert.AreEqual (Infoset (res), WriterText);

			imp = new XmlReflectionImporter ();
			map = imp.ImportTypeMapping (typeof (TestSchemaForm2), "urn:extra");
			Serialize (t2, map);
			res = "";
			res += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
			res += "<TestSchemaForm2 xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:extra\">";
			res += "  <p1 xmlns=\"urn:testForm\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p1>";
			res += "  <p2 xmlns=\"urn:oo\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p2>";
			res += "</TestSchemaForm2>";
			Assert.AreEqual (Infoset (res), WriterText);
		}

		[Test] // bug #78536
		public void CDataTextNodes ()
		{
			XmlSerializer ser = new XmlSerializer (typeof (CDataTextNodesType));
			ser.UnknownNode += new XmlNodeEventHandler (CDataTextNodes_BadNode);
			string xml = @"<CDataTextNodesType>
  <foo><![CDATA[
(?<filename>^([A-Z]:)?[^\(]+)\((?<line>\d+),(?<column>\d+)\):
\s((?<warning>warning)|(?<error>error))\s[^:]+:(?<message>.+$)|
(?<error>(fatal\s)?error)[^:]+:(?<message>.+$)
	]]></foo>
</CDataTextNodesType>";
			ser.Deserialize (new XmlTextReader (xml, XmlNodeType.Document, null));
		}

#if !MOBILE
		[Test]
		public void GenerateSerializerGenerics ()
		{
			XmlReflectionImporter imp = new XmlReflectionImporter ();
			Type type = typeof (List<int>);
			XmlSerializer.GenerateSerializer (
				new Type [] {type},
				new XmlTypeMapping [] {imp.ImportTypeMapping (type)});
		}
#endif

		[Test]
		public void Nullable ()
		{
			XmlSerializer ser = new XmlSerializer (typeof (int?));
			int? nullableType = 5;
			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
			ser.Serialize (xtw, nullableType);
			xtw.Close ();
			string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?><int>5</int>";
			Assert.AreEqual (Infoset (expected), WriterText);
			int? i = (int?) ser.Deserialize (new StringReader (sw.ToString ()));
			Assert.AreEqual (5, i);
		}

		[Test]
		public void NullableEnums ()
		{
			WithNulls w = new WithNulls ();
			XmlSerializer ser = new XmlSerializer (typeof(WithNulls));
			StringWriter tw = new StringWriter ();
			ser.Serialize (tw, w);

			string expected = "<?xml version='1.0' encoding='utf-16'?>" +
				"<WithNulls xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>" +
					"<nint xsi:nil='true' />" +
					"<nenum xsi:nil='true' />" +
					"<ndate xsi:nil='true' />" +
					"</WithNulls>";
			
			Assert.AreEqual (Infoset (expected), Infoset (tw.ToString ()));
			
			StringReader sr = new StringReader (tw.ToString ());
			w = (WithNulls) ser.Deserialize (sr);
			
			Assert.IsFalse (w.nint.HasValue);
			Assert.IsFalse (w.nenum.HasValue);
			Assert.IsFalse (w.ndate.HasValue);
			
			DateTime t = new DateTime (2008,4,1);
			w.nint = 4;
			w.ndate = t;
			w.nenum = TestEnumWithNulls.bb;
			
			tw = new StringWriter ();
			ser.Serialize (tw, w);
			
			expected = "<?xml version='1.0' encoding='utf-16'?>" +
				"<WithNulls xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>" +
					"<nint>4</nint>" +
					"<nenum>bb</nenum>" +
					"<ndate>2008-04-01T00:00:00</ndate>" +
					"</WithNulls>";
			
			Assert.AreEqual (Infoset (expected), Infoset (tw.ToString ()));
			
			sr = new StringReader (tw.ToString ());
			w = (WithNulls) ser.Deserialize (sr);
			
			Assert.IsTrue (w.nint.HasValue);
			Assert.IsTrue (w.nenum.HasValue);
			Assert.IsTrue (w.ndate.HasValue);
			Assert.AreEqual (4, w.nint.Value);
			Assert.AreEqual (TestEnumWithNulls.bb, w.nenum.Value);
			Assert.AreEqual (t, w.ndate.Value);
		}

		[Test]
		public void SerializeBase64Binary()
		{
			XmlSerializer ser = new XmlSerializer (typeof (Base64Binary));
			sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			ser.Serialize (xtw, new Base64Binary ());
			xtw.Close ();
			string expected = @"<?xml version=""1.0"" encoding=""utf-16""?><Base64Binary xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Data=""AQID"" />";
			Assert.AreEqual (Infoset (expected), WriterText);
			Base64Binary h = (Base64Binary) ser.Deserialize (new StringReader (sw.ToString ()));
			Assert.AreEqual (new byte [] {1, 2, 3}, h.Data);
		}

		[Test] // bug #79989, #79990
		public void SerializeHexBinary ()
		{
			XmlSerializer ser = new XmlSerializer (typeof (HexBinary));
			sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			ser.Serialize (xtw, new HexBinary ());
			xtw.Close ();
			string expected = @"<?xml version=""1.0"" encoding=""utf-16""?><HexBinary xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Data=""010203"" />";
			Assert.AreEqual (Infoset (expected), WriterText);
			HexBinary h = (HexBinary) ser.Deserialize (new StringReader (sw.ToString ()));
			Assert.AreEqual (new byte[] { 1, 2, 3 }, h.Data);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void XmlArrayAttributeOnInt ()
		{
			new XmlSerializer (typeof (XmlArrayOnInt));
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void XmlArrayAttributeUnqualifiedWithNamespace ()
		{
			new XmlSerializer (typeof (XmlArrayUnqualifiedWithNamespace));
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void XmlArrayItemAttributeUnqualifiedWithNamespace ()
		{
			new XmlSerializer (typeof (XmlArrayItemUnqualifiedWithNamespace));
		}

		[Test] // bug #78042
		public void XmlArrayAttributeOnArray ()
		{
			XmlSerializer ser = new XmlSerializer (typeof (XmlArrayOnArray));
			sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			ser.Serialize (xtw, new XmlArrayOnArray ());
			xtw.Close ();
			string expected = @"<?xml version=""1.0"" encoding=""utf-16""?><XmlArrayOnArray xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:foo""><Sane xmlns=""""><string xmlns=""urn:foo"">foo</string><string xmlns=""urn:foo"">bar</string></Sane><Mids xmlns=""""><ArrayItemInXmlArray xmlns=""urn:foo""><Whee xmlns=""""><string xmlns=""urn:gyabo"">foo</string><string xmlns=""urn:gyabo"">bar</string></Whee></ArrayItemInXmlArray></Mids></XmlArrayOnArray>";
			Assert.AreEqual (Infoset (expected), WriterText);
		}

		[Test]
		public void XmlArrayAttributeOnCollection ()
		{
			XmlSerializer ser = new XmlSerializer (typeof (XmlArrayOnArrayList));
			XmlArrayOnArrayList inst = new XmlArrayOnArrayList ();
			inst.Sane.Add ("abc");
			inst.Sane.Add (1);
			sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			ser.Serialize (xtw, inst);
			xtw.Close ();
			string expected = @"<?xml version=""1.0"" encoding=""utf-16""?><XmlArrayOnArrayList xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:foo""><Sane xmlns=""""><anyType xsi:type=""xsd:string"" xmlns=""urn:foo"">abc</anyType><anyType xsi:type=""xsd:int"" xmlns=""urn:foo"">1</anyType></Sane></XmlArrayOnArrayList>";
			Assert.AreEqual (Infoset (expected), WriterText);
		}

		[Test] // bug #338705
		public void SerializeTimeSpan ()
		{
			// TimeSpan itself is not for duration. Hence it is just regarded as one of custom types.
			XmlSerializer ser = new XmlSerializer (typeof (TimeSpan));
			ser.Serialize (TextWriter.Null, TimeSpan.Zero);
		}

		[Test]
		public void SerializeDurationToString ()
		{
			XmlSerializer ser = new XmlSerializer (typeof (TimeSpanContainer1));
			ser.Serialize (TextWriter.Null, new TimeSpanContainer1 ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SerializeDurationToTimeSpan ()
		{
			XmlSerializer ser = new XmlSerializer (typeof (TimeSpanContainer2));
			ser.Serialize (TextWriter.Null, new TimeSpanContainer2 ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SerializeInvalidDataType ()
		{
			XmlSerializer ser = new XmlSerializer (typeof (InvalidTypeContainer));
			ser.Serialize (TextWriter.Null, new InvalidTypeContainer ());
		}

		[Test]
		public void SerializeErrorneousIXmlSerializable ()
		{
			Serialize (new ErrorneousGetSchema ());
			Assert.AreEqual ("<:ErrorneousGetSchema></>", Infoset (sw.ToString ()));
		}

		[Test]
		public void DateTimeRoundtrip ()
		{
			// bug #337729
			XmlSerializer ser = new XmlSerializer (typeof (DateTime));
			StringWriter sw = new StringWriter ();
			ser.Serialize (sw, DateTime.UtcNow);
			DateTime d = (DateTime) ser.Deserialize (new StringReader (sw.ToString ()));
			Assert.AreEqual (DateTimeKind.Utc, d.Kind);
		}

		[Test]
		public void SupportIXmlSerializableImplicitlyConvertible ()
		{
			XmlAttributes attrs = new XmlAttributes ();
			XmlElementAttribute attr = new XmlElementAttribute ();
			attr.ElementName = "XmlSerializable";
			attr.Type = typeof (XmlSerializableImplicitConvertible.XmlSerializable);
			attrs.XmlElements.Add (attr);
			XmlAttributeOverrides attrOverrides = new
			XmlAttributeOverrides ();
			attrOverrides.Add (typeof (XmlSerializableImplicitConvertible), "B", attrs);

			XmlSerializableImplicitConvertible x = new XmlSerializableImplicitConvertible ();
			new XmlSerializer (typeof (XmlSerializableImplicitConvertible), attrOverrides).Serialize (TextWriter.Null, x);
		}

		[Test] // bug #566370
		public void SerializeEnumWithCSharpKeyword ()
		{
			var ser = new XmlSerializer (typeof (DoxCompoundKind));
			for (int i = 0; i < 100; i++) // test serialization code generator
				ser.Serialize (Console.Out, DoxCompoundKind.@class);
		}

		public enum DoxCompoundKind
		{
			[XmlEnum("class")]
			@class,
			[XmlEnum("struct")]
			@struct,
			union,
			[XmlEnum("interface")]
			@interface,
			protocol,
			category,
			exception,
			file,
			[XmlEnum("namespace")]
			@namespace,
			group,
			page,
			example,
			dir
		}

		#region GenericsSeralizationTests

		[Test]
		public void TestSerializeGenSimpleClassString ()
		{
			GenSimpleClass<string> simple = new GenSimpleClass<string> ();
			Serialize (simple);
			Assert.AreEqual (Infoset ("<GenSimpleClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			simple.something = "hello";

			Serialize (simple);
			Assert.AreEqual (Infoset ("<GenSimpleClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></GenSimpleClassOfString>"), WriterText);
		}

		[Test]
		public void TestSerializeGenSimpleClassBool ()
		{
			GenSimpleClass<bool> simple = new GenSimpleClass<bool> ();
			Serialize (simple);
			Assert.AreEqual (Infoset ("<GenSimpleClassOfBoolean xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>false</something></GenSimpleClassOfBoolean>"), WriterText);

			simple.something = true;

			Serialize (simple);
			Assert.AreEqual (Infoset ("<GenSimpleClassOfBoolean xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>true</something></GenSimpleClassOfBoolean>"), WriterText);
		}

		[Test]
		public void TestSerializeGenSimpleStructInt ()
		{
			GenSimpleStruct<int> simple = new GenSimpleStruct<int> (0);
			Serialize (simple);
			Assert.AreEqual (Infoset ("<GenSimpleStructOfInt32 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>0</something></GenSimpleStructOfInt32>"), WriterText);

			simple.something = 123;

			Serialize (simple);
			Assert.AreEqual (Infoset ("<GenSimpleStructOfInt32 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>123</something></GenSimpleStructOfInt32>"), WriterText);
		}

		[Test]
		public void TestSerializeGenListClassString ()
		{
			GenListClass<string> genlist = new GenListClass<string> ();
			Serialize (genlist);
			Assert.AreEqual (Infoset ("<GenListClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist></somelist></GenListClassOfString>"), WriterText);

			genlist.somelist.Add ("Value1");
			genlist.somelist.Add ("Value2");

			Serialize (genlist);
			Assert.AreEqual (Infoset ("<GenListClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist><string>Value1</string><string>Value2</string></somelist></GenListClassOfString>"), WriterText);
		}

		[Test]
		public void TestSerializeGenListClassFloat ()
		{
			GenListClass<float> genlist = new GenListClass<float> ();
			Serialize (genlist);
			Assert.AreEqual (Infoset ("<GenListClassOfSingle xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist></somelist></GenListClassOfSingle>"), WriterText);

			genlist.somelist.Add (1);
			genlist.somelist.Add (2.2F);

			Serialize (genlist);
			Assert.AreEqual (Infoset ("<GenListClassOfSingle xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist><float>1</float><float>2.2</float></somelist></GenListClassOfSingle>"), WriterText);
		}

		[Test]
		public void TestSerializeGenListClassList ()
		{
			GenListClass<GenListClass<int>> genlist = new GenListClass<GenListClass<int>> ();
			Serialize (genlist);
			Assert.AreEqual (Infoset ("<GenListClassOfGenListClassOfInt32 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist></somelist></GenListClassOfGenListClassOfInt32>"), WriterText);

			GenListClass<int> inlist1 = new GenListClass<int> ();
			inlist1.somelist.Add (1);
			inlist1.somelist.Add (2);
			GenListClass<int> inlist2 = new GenListClass<int> ();
			inlist2.somelist.Add (10);
			inlist2.somelist.Add (20);
			genlist.somelist.Add (inlist1);
			genlist.somelist.Add (inlist2);

			Serialize (genlist);
			Assert.AreEqual (Infoset ("<GenListClassOfGenListClassOfInt32 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist><GenListClassOfInt32><somelist><int>1</int><int>2</int></somelist></GenListClassOfInt32><GenListClassOfInt32><somelist><int>10</int><int>20</int></somelist></GenListClassOfInt32></somelist></GenListClassOfGenListClassOfInt32>"), WriterText);
		}

		[Test]
		public void TestSerializeGenListClassArray ()
		{
			GenListClass<GenArrayClass<char>> genlist = new GenListClass<GenArrayClass<char>> ();
			Serialize (genlist);
			Assert.AreEqual (Infoset ("<GenListClassOfGenArrayClassOfChar xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist></somelist></GenListClassOfGenArrayClassOfChar>"), WriterText);

			GenArrayClass<char> genarr1 = new GenArrayClass<char> ();
			genarr1.arr[0] = 'a';
			genarr1.arr[1] = 'b';
			genlist.somelist.Add (genarr1);
			GenArrayClass<char> genarr2 = new GenArrayClass<char> ();
			genarr2.arr[0] = 'd';
			genarr2.arr[1] = 'e';
			genarr2.arr[2] = 'f';
			genlist.somelist.Add (genarr2);

			Serialize (genlist);
			Assert.AreEqual (Infoset ("<GenListClassOfGenArrayClassOfChar xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist><GenArrayClassOfChar><arr><char>97</char><char>98</char><char>0</char></arr></GenArrayClassOfChar><GenArrayClassOfChar><arr><char>100</char><char>101</char><char>102</char></arr></GenArrayClassOfChar></somelist></GenListClassOfGenArrayClassOfChar>"), WriterText);
		}

		[Test]
		public void TestSerializeGenTwoClassCharDouble ()
		{
			GenTwoClass<char, double> gentwo = new GenTwoClass<char, double> ();
			Serialize (gentwo);
			Assert.AreEqual (Infoset ("<GenTwoClassOfCharDouble xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something1>0</something1><something2>0</something2></GenTwoClassOfCharDouble>"), WriterText);

			gentwo.something1 = 'a';
			gentwo.something2 = 2.2;

			Serialize (gentwo);
			Assert.AreEqual (Infoset ("<GenTwoClassOfCharDouble xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something1>97</something1><something2>2.2</something2></GenTwoClassOfCharDouble>"), WriterText);
		}

		[Test]
		public void TestSerializeGenDerivedClassDecimalShort ()
		{
			GenDerivedClass<decimal, short> derived = new GenDerivedClass<decimal, short> ();
			Serialize (derived);
			Assert.AreEqual (Infoset ("<GenDerivedClassOfDecimalInt16 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something2>0</something2><another1>0</another1><another2>0</another2></GenDerivedClassOfDecimalInt16>"), WriterText);

			derived.something1 = "Value1";
			derived.something2 = 1;
			derived.another1 = 1.1M;
			derived.another2 = -22;

			Serialize (derived);
			Assert.AreEqual (Infoset ("<GenDerivedClassOfDecimalInt16 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something1>Value1</something1><something2>1</something2><another1>1.1</another1><another2>-22</another2></GenDerivedClassOfDecimalInt16>"), WriterText);
		}

		[Test]
		public void TestSerializeGenDerivedSecondClassByteUlong ()
		{
			GenDerived2Class<byte, ulong> derived2 = new GenDerived2Class<byte, ulong> ();
			Serialize (derived2);
			Assert.AreEqual (Infoset ("<GenDerived2ClassOfByteUInt64 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something1>0</something1><something2>0</something2><another1>0</another1><another2>0</another2></GenDerived2ClassOfByteUInt64>"), WriterText);

			derived2.something1 = 1;
			derived2.something2 = 222;
			derived2.another1 = 111;
			derived2.another2 = 222222;

			Serialize (derived2);
			Assert.AreEqual (Infoset ("<GenDerived2ClassOfByteUInt64 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something1>1</something1><something2>222</something2><another1>111</another1><another2>222222</another2></GenDerived2ClassOfByteUInt64>"), WriterText);
		}

		[Test]
		public void TestSerializeGenNestedClass ()
		{
			GenNestedClass<string, int>.InnerClass<bool> nested =
				new GenNestedClass<string, int>.InnerClass<bool> ();
			Serialize (nested);
			Assert.AreEqual (Infoset ("<InnerClassOfStringInt32Boolean xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><inner>0</inner><something>false</something></InnerClassOfStringInt32Boolean>"), WriterText);

			nested.inner = 5;
			nested.something = true;

			Serialize (nested);
			Assert.AreEqual (Infoset ("<InnerClassOfStringInt32Boolean xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><inner>5</inner><something>true</something></InnerClassOfStringInt32Boolean>"), WriterText);
		}

		[Test]
		public void TestSerializeGenListClassListNested ()
		{
			GenListClass<GenListClass<GenNestedClass<int, int>.InnerClass<string>>> genlist =
				new GenListClass<GenListClass<GenNestedClass<int, int>.InnerClass<string>>> ();
			Serialize (genlist);
			Assert.AreEqual (Infoset ("<GenListClassOfGenListClassOfInnerClassOfInt32Int32String xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist></somelist></GenListClassOfGenListClassOfInnerClassOfInt32Int32String>"), WriterText);

			GenListClass<GenNestedClass<int, int>.InnerClass<string>> inlist1 =
				new GenListClass<GenNestedClass<int, int>.InnerClass<string>> ();
			GenNestedClass<int, int>.InnerClass<string> inval1 = new GenNestedClass<int, int>.InnerClass<string> ();
			inval1.inner = 1;
			inval1.something = "ONE";
			inlist1.somelist.Add (inval1);
			GenNestedClass<int, int>.InnerClass<string> inval2 = new GenNestedClass<int, int>.InnerClass<string> ();
			inval2.inner = 2;
			inval2.something = "TWO";
			inlist1.somelist.Add (inval2);
			GenListClass<GenNestedClass<int, int>.InnerClass<string>> inlist2 =
				new GenListClass<GenNestedClass<int, int>.InnerClass<string>> ();
			GenNestedClass<int, int>.InnerClass<string> inval3 = new GenNestedClass<int, int>.InnerClass<string> ();
			inval3.inner = 30;
			inval3.something = "THIRTY";
			inlist2.somelist.Add (inval3);
			genlist.somelist.Add (inlist1);
			genlist.somelist.Add (inlist2);

			Serialize (genlist);
			Assert.AreEqual (Infoset ("<GenListClassOfGenListClassOfInnerClassOfInt32Int32String xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist><GenListClassOfInnerClassOfInt32Int32String><somelist><InnerClassOfInt32Int32String><inner>1</inner><something>ONE</something></InnerClassOfInt32Int32String><InnerClassOfInt32Int32String><inner>2</inner><something>TWO</something></InnerClassOfInt32Int32String></somelist></GenListClassOfInnerClassOfInt32Int32String><GenListClassOfInnerClassOfInt32Int32String><somelist><InnerClassOfInt32Int32String><inner>30</inner><something>THIRTY</something></InnerClassOfInt32Int32String></somelist></GenListClassOfInnerClassOfInt32Int32String></somelist></GenListClassOfGenListClassOfInnerClassOfInt32Int32String>"), WriterText);
		}

		public enum Myenum { one, two, three, four, five, six };
		[Test]
		public void TestSerializeGenArrayClassEnum ()
		{
			GenArrayClass<Myenum> genarr = new GenArrayClass<Myenum> ();
			Serialize (genarr);
			Assert.AreEqual (Infoset ("<GenArrayClassOfMyenum xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><arr><Myenum>one</Myenum><Myenum>one</Myenum><Myenum>one</Myenum></arr></GenArrayClassOfMyenum>"), WriterText);

			genarr.arr[0] = Myenum.one;
			genarr.arr[1] = Myenum.three;
			genarr.arr[2] = Myenum.five;

			Serialize (genarr);
			Assert.AreEqual (Infoset ("<GenArrayClassOfMyenum xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><arr><Myenum>one</Myenum><Myenum>three</Myenum><Myenum>five</Myenum></arr></GenArrayClassOfMyenum>"), WriterText);
		}

		[Test]
		public void TestSerializeGenArrayStruct ()
		{
			GenArrayClass<GenSimpleStruct<uint>> genarr = new GenArrayClass<GenSimpleStruct<uint>> ();
			Serialize (genarr);
			Assert.AreEqual ("<:GenArrayClassOfGenSimpleStructOfUInt32 http://www.w3.org/2000/xmlns/:xsd='http://www.w3.org/2001/XMLSchema' http://www.w3.org/2000/xmlns/:xsi='http://www.w3.org/2001/XMLSchema-instance'><:arr><:GenSimpleStructOfUInt32><:something>0</></><:GenSimpleStructOfUInt32><:something>0</></><:GenSimpleStructOfUInt32><:something>0</></></></>", WriterText);

			GenSimpleStruct<uint> genstruct = new GenSimpleStruct<uint> ();
			genstruct.something = 111;
			genarr.arr[0] = genstruct;
			genstruct.something = 222;
			genarr.arr[1] = genstruct;
			genstruct.something = 333;
			genarr.arr[2] = genstruct;

			Serialize (genarr);
			Assert.AreEqual ("<:GenArrayClassOfGenSimpleStructOfUInt32 http://www.w3.org/2000/xmlns/:xsd='http://www.w3.org/2001/XMLSchema' http://www.w3.org/2000/xmlns/:xsi='http://www.w3.org/2001/XMLSchema-instance'><:arr><:GenSimpleStructOfUInt32><:something>111</></><:GenSimpleStructOfUInt32><:something>222</></><:GenSimpleStructOfUInt32><:something>333</></></></>", WriterText);
		}

		[Test]
		public void TestSerializeGenArrayList ()
		{
			GenArrayClass<GenListClass<string>> genarr = new GenArrayClass<GenListClass<string>> ();
			Serialize (genarr);
			Assert.AreEqual ("<:GenArrayClassOfGenListClassOfString http://www.w3.org/2000/xmlns/:xsd='http://www.w3.org/2001/XMLSchema' http://www.w3.org/2000/xmlns/:xsi='http://www.w3.org/2001/XMLSchema-instance'><:arr><:GenListClassOfString http://www.w3.org/2001/XMLSchema-instance:nil='true'></><:GenListClassOfString http://www.w3.org/2001/XMLSchema-instance:nil='true'></><:GenListClassOfString http://www.w3.org/2001/XMLSchema-instance:nil='true'></></></>", WriterText);

			GenListClass<string> genlist1 = new GenListClass<string> ();
			genlist1.somelist.Add ("list1-val1");
			genlist1.somelist.Add ("list1-val2");
			genarr.arr[0] = genlist1;
			GenListClass<string> genlist2 = new GenListClass<string> ();
			genlist2.somelist.Add ("list2-val1");
			genlist2.somelist.Add ("list2-val2");
			genlist2.somelist.Add ("list2-val3");
			genlist2.somelist.Add ("list2-val4");
			genarr.arr[1] = genlist2;
			GenListClass<string> genlist3 = new GenListClass<string> ();
			genlist3.somelist.Add ("list3val");
			genarr.arr[2] = genlist3;

			Serialize (genarr);
			Assert.AreEqual ("<:GenArrayClassOfGenListClassOfString http://www.w3.org/2000/xmlns/:xsd='http://www.w3.org/2001/XMLSchema' http://www.w3.org/2000/xmlns/:xsi='http://www.w3.org/2001/XMLSchema-instance'><:arr><:GenListClassOfString><:somelist><:string>list1-val1</><:string>list1-val2</></></><:GenListClassOfString><:somelist><:string>list2-val1</><:string>list2-val2</><:string>list2-val3</><:string>list2-val4</></></><:GenListClassOfString><:somelist><:string>list3val</></></></></>", WriterText);
		}

		[Test]
		public void TestSerializeGenComplexStruct ()
		{
			GenComplexStruct<int, string> complex = new GenComplexStruct<int, string> (0);
			Serialize (complex);
			Assert.AreEqual ("<:GenComplexStructOfInt32String http://www.w3.org/2000/xmlns/:xsd='http://www.w3.org/2001/XMLSchema' http://www.w3.org/2000/xmlns/:xsi='http://www.w3.org/2001/XMLSchema-instance'><:something>0</><:simpleclass><:something>0</></><:simplestruct><:something>0</></><:listclass><:somelist></></><:arrayclass><:arr><:int>0</><:int>0</><:int>0</></></><:twoclass><:something1>0</></><:derivedclass><:something2>0</><:another1>0</></><:derived2><:something1>0</><:another1>0</></><:nestedouter><:outer>0</></><:nestedinner><:something>0</></></>", WriterText);

			complex.something = 123;
			complex.simpleclass.something = 456;
			complex.simplestruct.something = 789;
			GenListClass<int> genlist = new GenListClass<int> ();
			genlist.somelist.Add (100);
			genlist.somelist.Add (200);
			complex.listclass = genlist;
			GenArrayClass<int> genarr = new GenArrayClass<int> ();
			genarr.arr[0] = 11;
			genarr.arr[1] = 22;
			genarr.arr[2] = 33;
			complex.arrayclass = genarr;
			complex.twoclass.something1 = 10;
			complex.twoclass.something2 = "Ten";
			complex.derivedclass.another1 = 1;
			complex.derivedclass.another2 = "one";
			complex.derivedclass.something1 = "two";
			complex.derivedclass.something2 = 2;
			complex.derived2.another1 = 3;
			complex.derived2.another2 = "three";
			complex.derived2.something1 = 4;
			complex.derived2.something2 = "four";
			complex.nestedouter.outer = 5;
			complex.nestedinner.inner = "six";
			complex.nestedinner.something = 6;

			Serialize (complex);
			Assert.AreEqual ("<:GenComplexStructOfInt32String http://www.w3.org/2000/xmlns/:xsd='http://www.w3.org/2001/XMLSchema' http://www.w3.org/2000/xmlns/:xsi='http://www.w3.org/2001/XMLSchema-instance'><:something>123</><:simpleclass><:something>456</></><:simplestruct><:something>789</></><:listclass><:somelist><:int>100</><:int>200</></></><:arrayclass><:arr><:int>11</><:int>22</><:int>33</></></><:twoclass><:something1>10</><:something2>Ten</></><:derivedclass><:something1>two</><:something2>2</><:another1>1</><:another2>one</></><:derived2><:something1>4</><:something2>four</><:another1>3</><:another2>three</></><:nestedouter><:outer>5</></><:nestedinner><:inner>six</><:something>6</></></>", WriterText);
		}

		[Test]
		public void TestSerializeStreamPreserveUTFChars () {
			string foo = "BÄR";
			XmlSerializer serializer = new XmlSerializer (typeof (string));

			MemoryStream stream = new MemoryStream ();

			serializer.Serialize (stream, foo);
			stream.Position = 0;
			foo = (string) serializer.Deserialize (stream);
			Assert.AreEqual("BÄR", foo);
		}

		[Test] // bug #80759
		public void HasNullableField ()
		{
			Bug80759 foo = new Bug80759 ();
			foo.Test = "BAR";
			foo.NullableInt = 10;

			XmlSerializer serializer = new XmlSerializer (typeof (Bug80759));

			MemoryStream stream = new MemoryStream ();

			serializer.Serialize (stream, foo);
			stream.Position = 0;
			foo = (Bug80759) serializer.Deserialize (stream);
		}

		[Test] // bug #80759, with fieldSpecified.
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void HasFieldSpecifiedButIrrelevant ()
		{
			Bug80759_2 foo = new Bug80759_2 ();
			foo.Test = "BAR";
			foo.NullableInt = 10;

			XmlSerializer serializer = new XmlSerializer (typeof (Bug80759_2));

			MemoryStream stream = new MemoryStream ();

			serializer.Serialize (stream, foo);
			stream.Position = 0;
			foo = (Bug80759_2) serializer.Deserialize (stream);
		}

		[Test]
		public void HasNullableField2 ()
		{
			Bug80759 foo = new Bug80759 ();
			foo.Test = "BAR";
			foo.NullableInt = 10;

			XmlSerializer serializer = new XmlSerializer (typeof (Bug80759));

			MemoryStream stream = new MemoryStream ();

			serializer.Serialize (stream, foo);
			stream.Position = 0;
			foo = (Bug80759) serializer.Deserialize (stream);

			Assert.AreEqual ("BAR", foo.Test, "#1");
			Assert.AreEqual (10, foo.NullableInt, "#2");

			foo.NullableInt = null;
			stream = new MemoryStream ();
			serializer.Serialize (stream, foo);
			stream.Position = 0;
			foo = (Bug80759) serializer.Deserialize (stream);

			Assert.AreEqual ("BAR", foo.Test, "#3");
			Assert.IsNull (foo.NullableInt, "#4");
		}

		[Test]
		public void SupportPrivateCtorOnly ()
		{
			XmlSerializer xs =
				new XmlSerializer (typeof (PrivateCtorOnly));
			StringWriter sw = new StringWriter ();
			xs.Serialize (sw, PrivateCtorOnly.Instance);
			xs.Deserialize (new StringReader (sw.ToString ()));
		}

		[Test]
		public void XmlSchemaProviderQNameBecomesRootName ()
		{
			xs = new XmlSerializer (typeof (XmlSchemaProviderQNameBecomesRootNameType));
			Serialize (new XmlSchemaProviderQNameBecomesRootNameType ());
			Assert.AreEqual (Infoset ("<foo />"), WriterText);
			xs.Deserialize (new StringReader ("<foo/>"));
		}

		[Test]
		public void XmlSchemaProviderQNameBecomesRootName2 ()
		{
			string xml = "<XmlSchemaProviderQNameBecomesRootNameType2 xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Foo><foo /></Foo></XmlSchemaProviderQNameBecomesRootNameType2>";
			xs = new XmlSerializer (typeof (XmlSchemaProviderQNameBecomesRootNameType2));
			Serialize (new XmlSchemaProviderQNameBecomesRootNameType2 ());
			Assert.AreEqual (Infoset (xml), WriterText);
			xs.Deserialize (new StringReader (xml));
		}

		[Test]
		public void XmlAnyElementForObjects () // bug #553032
		{
			new XmlSerializer (typeof (XmlAnyElementForObjectsType));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void XmlAnyElementForObjects2 () // bug #553032-2
		{
			new XmlSerializer (typeof (XmlAnyElementForObjectsType)).Serialize (TextWriter.Null, new XmlAnyElementForObjectsType ());
		}


		public class Bug2893 {
			public Bug2893 ()
			{			
				Contents = new XmlDataDocument();
			}
			
			[XmlAnyElement("Contents")]
			public XmlNode Contents;
		}

		// Bug Xamarin #2893
		[Test]
		public void XmlAnyElementForXmlNode ()
		{
			var obj = new Bug2893 ();
			XmlSerializer mySerializer = new XmlSerializer(typeof(Bug2893));
			XmlWriterSettings settings = new XmlWriterSettings();

			var xsn = new XmlSerializerNamespaces();
			xsn.Add(string.Empty, string.Empty);

			byte[] buffer = new byte[2048];
			var ms = new MemoryStream(buffer);
			using (var xw = XmlWriter.Create(ms, settings))
			{
				mySerializer.Serialize(xw, obj, xsn);
				xw.Flush();
			}

			mySerializer.Serialize(ms, obj);
		}

		[Test]
		public void XmlRootOverridesSchemaProviderQName ()
		{
			var obj = new XmlRootOverridesSchemaProviderQNameType ();

			XmlSerializer xs = new XmlSerializer (obj.GetType ());

			var sw = new StringWriter ();
			using (XmlWriter xw = XmlWriter.Create (sw))
				xs.Serialize (xw, obj);
			Assert.IsTrue (sw.ToString ().IndexOf ("foo") > 0, "#1");
		}

		public class AnotherArrayListType
		{
			[XmlAttribute]
			public string one = "aaa";
			[XmlAttribute]
			public string another = "bbb";
		}

		public class DerivedArrayListType : AnotherArrayListType
		{

		}

		public class ClassWithArrayList
		{
			[XmlElement (Type = typeof(int), ElementName = "int_elem")]
			[XmlElement (Type = typeof(string), ElementName = "string_elem")]
			[XmlElement (Type = typeof(AnotherArrayListType), ElementName = "another_elem")]
			[XmlElement (Type = typeof(DerivedArrayListType), ElementName = "derived_elem")]
			public ArrayList list;
		}

		public class ClassWithArray
		{
			[XmlElement (Type = typeof(int), ElementName = "int_elem")]
			[XmlElement (Type = typeof(string), ElementName = "string_elem")]
			[XmlElement (Type = typeof(AnotherArrayListType), ElementName = "another_elem")]
			[XmlElement (Type = typeof(DerivedArrayListType), ElementName = "derived_elem")]
			public object[] list;

		}

		[Test]
		public void MultipleXmlElementAttributesOnArrayList()
		{
			var test = new ClassWithArrayList();

			test.list = new ArrayList();
			test.list.Add(3);
			test.list.Add("apepe");
			test.list.Add(new AnotherArrayListType());
			test.list.Add(new DerivedArrayListType());

			Serialize(test);
			var expected_text = "<:ClassWithArrayList http://www.w3.org/2000/xmlns/:xsd='http://www.w3.org/2001/XMLSchema' http://www.w3.org/2000/xmlns/:xsi='http://www.w3.org/2001/XMLSchema-instance'><:int_elem>3</><:string_elem>apepe</><:another_elem :another='bbb' :one='aaa'></><:derived_elem :another='bbb' :one='aaa'></></>";

			Assert.AreEqual(WriterText, expected_text, WriterText);
		}

		[Test]
		public void MultipleXmlElementAttributesOnArray()
		{
			var test = new ClassWithArray();

			test.list = new object[] { 3, "apepe", new AnotherArrayListType(), new DerivedArrayListType() };

			Serialize(test);
			var expected_text = "<:ClassWithArray http://www.w3.org/2000/xmlns/:xsd='http://www.w3.org/2001/XMLSchema' http://www.w3.org/2000/xmlns/:xsi='http://www.w3.org/2001/XMLSchema-instance'><:int_elem>3</><:string_elem>apepe</><:another_elem :another='bbb' :one='aaa'></><:derived_elem :another='bbb' :one='aaa'></></>";

			Assert.AreEqual(WriterText, expected_text, WriterText);
		}


		#endregion //GenericsSeralizationTests
		#region XmlInclude on abstract class tests (Bug #18558)
		[Test]
		public void TestSerializeIntermediateType ()
		{
			string expectedXml = "<ContainerTypeForTest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><XmlIntermediateType intermediate=\"false\"/></ContainerTypeForTest>";
			var obj = new ContainerTypeForTest();
			obj.MemberToUseInclude = new IntermediateTypeForTest ();
			Serialize (obj);
			Assert.AreEqual (Infoset (expectedXml), WriterText, "Serialized Output : " + WriterText);
		}

		[Test]
		public void TestSerializeSecondType ()
		{
			string expectedXml = "<ContainerTypeForTest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><XmlSecondType intermediate=\"false\"/></ContainerTypeForTest>";
			var obj = new ContainerTypeForTest();
			obj.MemberToUseInclude = new SecondDerivedTypeForTest ();
			Serialize (obj);
			Assert.AreEqual (Infoset (expectedXml), WriterText, "Serialized Output : " + WriterText);
		}
		#endregion
		public class XmlArrayOnInt
		{
			[XmlArray]
			public int Bogus;
		}

		public class XmlArrayUnqualifiedWithNamespace
		{
			[XmlArray (Namespace = "", Form = XmlSchemaForm.Unqualified)]
			public ArrayList Sane = new ArrayList ();
		}

		public class XmlArrayItemUnqualifiedWithNamespace
		{
			[XmlArrayItem ("foo", Namespace = "", Form = XmlSchemaForm.Unqualified)]
			public ArrayList Sane = new ArrayList ();
		}

		[XmlRoot (Namespace = "urn:foo")]
		public class XmlArrayOnArrayList
		{
			[XmlArray (Form = XmlSchemaForm.Unqualified)]
			public ArrayList Sane = new ArrayList ();
		}

		[XmlRoot (Namespace = "urn:foo")]
		public class XmlArrayOnArray
		{
			[XmlArray (Form = XmlSchemaForm.Unqualified)]
			public string[] Sane = new string[] { "foo", "bar" };

			[XmlArray (Form = XmlSchemaForm.Unqualified)]
			public ArrayItemInXmlArray[] Mids =
				new ArrayItemInXmlArray[] { new ArrayItemInXmlArray () };
		}

		[XmlType (Namespace = "urn:gyabo")]
		public class ArrayItemInXmlArray
		{
			[XmlArray (Form = XmlSchemaForm.Unqualified)]
			public string[] Whee = new string[] { "foo", "bar" };
		}

		[XmlRoot ("Base64Binary")]
		public class Base64Binary
		{
			[XmlAttribute (DataType = "base64Binary")]
			public byte [] Data = new byte [] {1, 2, 3};
		}

		[XmlRoot ("HexBinary")]
		public class HexBinary
		{
			[XmlAttribute (DataType = "hexBinary")]
			public byte[] Data = new byte[] { 1, 2, 3 };
		}

		[XmlRoot ("PrivateCtorOnly")]
		public class PrivateCtorOnly
		{
			public static PrivateCtorOnly Instance = new PrivateCtorOnly ();
			private PrivateCtorOnly ()
			{
			}
		}

		public class CDataTextNodesType
		{
			public CDataTextNodesInternal foo;
		}

		public class CDataTextNodesInternal
		{
			[XmlText]
			public string Value;
		}

		public class InvalidTypeContainer
		{
			[XmlElement (DataType = "invalid")]
			public string InvalidTypeItem = "aaa";
		}

		public class TimeSpanContainer1
		{
			[XmlElement (DataType = "duration")]
			public string StringDuration = "aaa";
		}

		public class TimeSpanContainer2
		{
			[XmlElement (DataType = "duration")]
			public TimeSpan StringDuration = TimeSpan.FromSeconds (1);
		}

		public class Bug80759
		{
			public string Test;
			public int? NullableInt;
		}

		public class Bug80759_2
		{
			public string Test;
			public int? NullableInt;

			[XmlIgnore]
			public bool NullableIntSpecified {
				get { return NullableInt.HasValue; }
			}
		}

		[XmlSchemaProvider ("GetXsdType")]
		public class XmlSchemaProviderQNameBecomesRootNameType : IXmlSerializable
		{
			public XmlSchema GetSchema ()
			{
				return null;
			}

			public void ReadXml (XmlReader reader)
			{
				reader.Skip ();
			}

			public void WriteXml (XmlWriter writer)
			{
			}

			public static XmlQualifiedName GetXsdType (XmlSchemaSet xss)
			{
				if (xss.Count == 0) {
					XmlSchema xs = new XmlSchema ();
					XmlSchemaComplexType ct = new XmlSchemaComplexType ();
					ct.Name = "foo";
					xs.Items.Add (ct);
					xss.Add (xs);
				}
				return new XmlQualifiedName ("foo");
			}
		}

		public class XmlSchemaProviderQNameBecomesRootNameType2
		{
		        [XmlArrayItem (typeof (XmlSchemaProviderQNameBecomesRootNameType))]
		        public object [] Foo = new object [] {new XmlSchemaProviderQNameBecomesRootNameType ()};
		}

		public class XmlAnyElementForObjectsType
		{
			[XmlAnyElement]
			public object [] arr = new object [] {3,4,5};
		}

		[XmlRoot ("foo")]
		[XmlSchemaProvider ("GetSchema")]
		public class XmlRootOverridesSchemaProviderQNameType : IXmlSerializable
		{
			public static XmlQualifiedName GetSchema (XmlSchemaSet xss)
			{
				var xs = new XmlSchema ();
				var xse = new XmlSchemaComplexType () { Name = "bar" };
				xs.Items.Add (xse);
				xss.Add (xs);
				return new XmlQualifiedName ("bar");
			}

			XmlSchema IXmlSerializable.GetSchema ()
			{
				return null;
			}

			void IXmlSerializable.ReadXml (XmlReader reader)
			{
			}
			void IXmlSerializable.WriteXml (XmlWriter writer)
			{
			}
		}


		void CDataTextNodes_BadNode (object s, XmlNodeEventArgs e)
		{
			Assert.Fail ();
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
			switch (nod.NodeType) {
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

				foreach (string name in ats) {
					string[] nn = name.Split (' ');
					GetInfoset (elem.Attributes[nn[0], nn[1]], sb);
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

		static XmlTypeMapping CreateSoapMapping (Type type, SoapAttributeOverrides ao)
		{
			SoapReflectionImporter importer = new SoapReflectionImporter (ao);
			return importer.ImportTypeMapping (type);
		}

		static XmlTypeMapping CreateSoapMapping (Type type, SoapAttributeOverrides ao, string defaultNamespace)
		{
			SoapReflectionImporter importer = new SoapReflectionImporter (ao, defaultNamespace);
			return importer.ImportTypeMapping (type);
		}

		[XmlSchemaProvider (null, IsAny = true)]
		public class AnySchemaProviderClass : IXmlSerializable {

			public string Text;

			void IXmlSerializable.WriteXml (XmlWriter writer)
			{
				writer.WriteElementString ("text", Text);
			}

			void IXmlSerializable.ReadXml (XmlReader reader)
			{
				Text = reader.ReadElementString ("text");
			}

			XmlSchema IXmlSerializable.GetSchema ()
			{
				return null;
			}
		}

		[Test]
		public void SerializeAnySchemaProvider ()
		{
			string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				Environment.NewLine + "<text>test</text>";

			var ser = new XmlSerializer (typeof (AnySchemaProviderClass));

			var obj = new AnySchemaProviderClass {
				Text = "test",
			};

			using (var t = new StringWriter ()) {
				ser.Serialize (t, obj);
				Assert.AreEqual (expected, t.ToString ());
			}
		}

		[Test]
		public void DeserializeAnySchemaProvider ()
		{
			string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				Environment.NewLine + "<text>test</text>";

			var ser = new XmlSerializer (typeof (AnySchemaProviderClass));

			using (var t = new StringReader (expected)) {
				var obj = (AnySchemaProviderClass) ser.Deserialize (t);
				Assert.AreEqual ("test", obj.Text);
			}
		}

		public class SubNoParameterlessConstructor : NoParameterlessConstructor
		{
			public SubNoParameterlessConstructor ()
				: base ("")
			{
			}
		}

		public class NoParameterlessConstructor
		{
			[XmlElement ("Text")]
			public string Text;

			public NoParameterlessConstructor (string parameter)
			{
			}
		}

		[Test]
		public void BaseClassWithoutParameterlessConstructor ()
		{
			var ser = new XmlSerializer (typeof (SubNoParameterlessConstructor));

			var obj = new SubNoParameterlessConstructor {
				Text = "test",
			};

			using (var w = new StringWriter ()) {
				ser.Serialize (w, obj);
				using (var r = new StringReader ( w.ToString ())) {
					var desObj = (SubNoParameterlessConstructor) ser.Deserialize (r);
					Assert.AreEqual (obj.Text, desObj.Text);
				}
			}
		}

		public class ClassWithXmlAnyElement
		{
			[XmlAnyElement ("Contents")]
			public XmlNode Contents;
		}

		[Test] // bug #3211
		public void TestClassWithXmlAnyElement ()
		{
			var d = new XmlDocument ();
			var e = d.CreateElement ("Contents");
			e.AppendChild (d.CreateElement ("SomeElement"));

			var c = new ClassWithXmlAnyElement {
				Contents = e,
			};

			var ser = new XmlSerializer (typeof (ClassWithXmlAnyElement));
			using (var sw = new StringWriter ())
				ser.Serialize (sw, c);
		}

		[Test]
		public void ClassWithImplicitlyConvertibleElement ()
		{
			var ser = new XmlSerializer (typeof (ObjectWithElementRequiringImplicitCast));

			var obj = new ObjectWithElementRequiringImplicitCast ("test");

			using (var w = new StringWriter ()) {
				ser.Serialize (w, obj);
				using (var r = new StringReader ( w.ToString ())) {
					var desObj = (ObjectWithElementRequiringImplicitCast) ser.Deserialize (r);
					Assert.AreEqual (obj.Object.Text, desObj.Object.Text);
				}
			}
		}

		public class ClassWithOptionalMethods
		{
			private readonly bool shouldSerializeX;
			private readonly bool xSpecified;

			[XmlAttribute]
			public int X { get; set; }

			public bool ShouldSerializeX () { return shouldSerializeX; }

			public bool XSpecified
			{
				get { return xSpecified; }
			}

			public ClassWithOptionalMethods ()
			{
			}

			public ClassWithOptionalMethods (int x, bool shouldSerializeX, bool xSpecified)
			{
				this.X = x;
				this.shouldSerializeX = shouldSerializeX;
				this.xSpecified = xSpecified;
			}
		}

		[Test]
		public void OptionalMethods ()
		{
			var ser = new XmlSerializer (typeof (ClassWithOptionalMethods));

			var expectedValueWithoutX = Infoset ("<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
				"<ClassWithOptionalMethods xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />");

			var expectedValueWithX = Infoset ("<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
				"<ClassWithOptionalMethods xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" X=\"11\" />");

			using (var t = new StringWriter ()) {
				var obj = new ClassWithOptionalMethods (11, false, false);
				ser.Serialize (t, obj);
				Assert.AreEqual (expectedValueWithoutX, Infoset (t.ToString ()));
			}

			using (var t = new StringWriter ()) {
				var obj = new ClassWithOptionalMethods (11, true, false);
				ser.Serialize (t, obj);
				Assert.AreEqual (expectedValueWithoutX, Infoset (t.ToString ()));
			}

			using (var t = new StringWriter ()) {
				var obj = new ClassWithOptionalMethods (11, false, true);
				ser.Serialize (t, obj);
				Assert.AreEqual (expectedValueWithoutX, Infoset (t.ToString ()));
			}

			using (var t = new StringWriter ()) {
				var obj = new ClassWithOptionalMethods (11, true, true);
				ser.Serialize (t, obj);
				Assert.AreEqual (expectedValueWithX, Infoset (t.ToString ()));
			}
		}

		public class ClassWithShouldSerializeGeneric
		{
			[XmlAttribute]
			public int X { get; set; }

			public bool ShouldSerializeX<T> () { return false; }
		}

		[Test]
		[Category("NotWorking")]
		public void ShouldSerializeGeneric ()
		{
			var ser = new XmlSerializer (typeof (ClassWithShouldSerializeGeneric));

			var expectedValueWithX = Infoset ("<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
				"<ClassWithShouldSerializeGeneric xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" X=\"11\" />");

			using (var t = new StringWriter ()) {
				var obj = new ClassWithShouldSerializeGeneric { X = 11 };
				ser.Serialize (t, obj);
				Assert.AreEqual (expectedValueWithX, Infoset (t.ToString ()));
			}
		}

		[Test]
		public void NullableArrayItems ()
		{
			var ser = new XmlSerializer (typeof (ObjectWithNullableArrayItems));

			var obj = new ObjectWithNullableArrayItems ();
			obj.Elements = new List <SimpleClass> ();
			obj.Elements.Add (new SimpleClass { something = "Hello" });
			obj.Elements.Add (null);
			obj.Elements.Add (new SimpleClass { something = "World" });

			using (var w = new StringWriter ()) {
				ser.Serialize (w, obj);
				using (var r = new StringReader ( w.ToString ())) {
					var desObj = (ObjectWithNullableArrayItems) ser.Deserialize (r);
					Assert.IsNull (desObj.Elements [1]);
				}
			}
		}

		[Test]
		public void NonNullableArrayItems ()
		{
			var ser = new XmlSerializer (typeof (ObjectWithNonNullableArrayItems));

			var obj = new ObjectWithNonNullableArrayItems ();
			obj.Elements = new List <SimpleClass> ();
			obj.Elements.Add (new SimpleClass { something = "Hello" });
			obj.Elements.Add (null);
			obj.Elements.Add (new SimpleClass { something = "World" });

			using (var w = new StringWriter ()) {
				ser.Serialize (w, obj);
				using (var r = new StringReader ( w.ToString ())) {
					var desObj = (ObjectWithNonNullableArrayItems) ser.Deserialize (r);
					Assert.IsNotNull (desObj.Elements [1]);
				}
			}
		}

		[Test]
		public void NotSpecifiedNullableArrayItems ()
		{
			var ser = new XmlSerializer (typeof (ObjectWithNotSpecifiedNullableArrayItems));

			var obj = new ObjectWithNotSpecifiedNullableArrayItems ();
			obj.Elements = new List <SimpleClass> ();
			obj.Elements.Add (new SimpleClass { something = "Hello" });
			obj.Elements.Add (null);
			obj.Elements.Add (new SimpleClass { something = "World" });

			using (var w = new StringWriter ()) {
				ser.Serialize (w, obj);
				using (var r = new StringReader ( w.ToString ())) {
					var desObj = (ObjectWithNotSpecifiedNullableArrayItems) ser.Deserialize (r);
					Assert.IsNull (desObj.Elements [1]);
				}
			}
		}

		private static void TestClassWithDefaultTextNotNullAux (string value, string expected)
		{
			var obj = new ClassWithDefaultTextNotNull (value);
			var ser = new XmlSerializer (typeof (ClassWithDefaultTextNotNull));

			using (var mstream = new MemoryStream ())
			using (var writer = new XmlTextWriter (mstream, Encoding.ASCII)) {
				ser.Serialize (writer, obj);

				mstream.Seek (0, SeekOrigin.Begin);
				using (var reader = new XmlTextReader (mstream)) {
					var result = (ClassWithDefaultTextNotNull) ser.Deserialize (reader);
					Assert.AreEqual (expected, result.Value);
				}
			}
		}

		[Test]
		public void TestClassWithDefaultTextNotNull ()
		{
			TestClassWithDefaultTextNotNullAux ("my_text", "my_text");
			TestClassWithDefaultTextNotNullAux ("", ClassWithDefaultTextNotNull.DefaultValue);
			TestClassWithDefaultTextNotNullAux (null, ClassWithDefaultTextNotNull.DefaultValue);
		}
	}

	// Test generated serialization code.
	public class XmlSerializerGeneratorTests : XmlSerializerTests {

		private FieldInfo backgroundGeneration;
		private FieldInfo generationThreshold;
		private FieldInfo generatorFallback;

		private bool backgroundGenerationOld;
		private int generationThresholdOld;
		private bool generatorFallbackOld;

		[SetUp]
		public void SetUp ()
		{
			// Make sure XmlSerializer static constructor is called
			XmlSerializer.FromTypes (new Type [] {});

			const BindingFlags binding = BindingFlags.Static | BindingFlags.NonPublic;
			backgroundGeneration = typeof (XmlSerializer).GetField ("backgroundGeneration", binding);
			generationThreshold = typeof (XmlSerializer).GetField ("generationThreshold", binding);
			generatorFallback = typeof (XmlSerializer).GetField ("generatorFallback", binding);

			if (backgroundGeneration == null)
				Assert.Ignore ("Unable to access field backgroundGeneration");
			if (generationThreshold == null)
				Assert.Ignore ("Unable to access field generationThreshold");
			if (generatorFallback == null)
				Assert.Ignore ("Unable to access field generatorFallback");

			backgroundGenerationOld = (bool) backgroundGeneration.GetValue (null);
			generationThresholdOld = (int) generationThreshold.GetValue (null);
			generatorFallbackOld = (bool) generatorFallback.GetValue (null);

			backgroundGeneration.SetValue (null, false);
			generationThreshold.SetValue (null, 0);
			generatorFallback.SetValue (null, false);
		}

		[TearDown]
		public void TearDown ()
		{
			if (backgroundGeneration == null || generationThreshold == null || generatorFallback == null)
				return;

			backgroundGeneration.SetValue (null, backgroundGenerationOld);
			generationThreshold.SetValue (null, generationThresholdOld);
			generatorFallback.SetValue (null, generatorFallbackOld);
		}
	}

#region XmlInclude on abstract class test classes

	[XmlType]
	public class ContainerTypeForTest
	{
		[XmlElement ("XmlSecondType", typeof (SecondDerivedTypeForTest))]
		[XmlElement ("XmlIntermediateType", typeof (IntermediateTypeForTest))]
		[XmlElement ("XmlFirstType", typeof (FirstDerivedTypeForTest))]
		public AbstractTypeForTest MemberToUseInclude { get; set; }
	}

	[XmlInclude (typeof (SecondDerivedTypeForTest))]
	[XmlInclude (typeof (IntermediateTypeForTest))]
	[XmlInclude (typeof (FirstDerivedTypeForTest))]
	public abstract class AbstractTypeForTest
	{
	}

	public class IntermediateTypeForTest : AbstractTypeForTest
	{
		[XmlAttribute (AttributeName = "intermediate")]
		public bool IntermediateMember { get; set; }
	}

	public class FirstDerivedTypeForTest : AbstractTypeForTest
	{
		public string FirstMember { get; set; }
	}

	public class SecondDerivedTypeForTest : IntermediateTypeForTest
	{
		public string SecondMember { get; set; }
	}
#endregion
}
