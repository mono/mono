//
// MonoTests.System.Xml.Serialization.XmlSerializationWriterTests
//
// Author: Erik LeBel <eriklebel@yahoo.ca>
//
//  (C) Erik LeBel 2003
// Copyright 2003-2011 Novell 
// Copyright 2011 Xamarin Inc
//  
// FIXME add tests for callbacks
// FIXME add tests for writes that generate namespaces
// FIXME add test that write XmlNode objects
// 

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	// base, common implementation of XmlSerializationWriter test harness.
	// the reason for this is that all auto generated namespace prefixes 
	// of the form q# are modified by any Write* that defines a new namespace.
	// The result of this is that even though we redefine the string results
	// to exclude previous tests, the q#s will change depending on number of
	// namespace declarations were made prior to the perticual test. This 
	// means that if the [Test] methods are called out of sequence, they 
	// all start to fail. For this reason, tests that define and verify 
	// temporary namespaces should be stored in a seperate class which protects
	// itself from accidental pre-definitions.
	public class XmlSerializarionWriterTester : XmlSerializationWriter
	{
		// appease the compiler
		protected override void InitCallbacks ()
		{
		}

		StringWriter sw;
		XmlTextWriter writer;
		
		[SetUp]
		public void Reset()
		{
			sw = new StringWriter ();
			writer = new XmlTextWriter (sw);
			writer.QuoteChar = '\'';
			writer.Formatting = Formatting.None;
			Writer = writer;
		}

		public string Content
		{
			get
			{ 
				string val = sw.GetStringBuilder().ToString();
				return val;
			}
		}

		public void ExecuteWritePotentiallyReferencingElement (string name, string ns, object o, Type ambientType, bool suppressReference, bool isNullable)
		{
			WritePotentiallyReferencingElement (name, ns, o, ambientType, suppressReference, isNullable);
		}

		public void ExecuteWriteTypedPrimitive (string name, string ns, object o, bool xsiType)
		{
			WriteTypedPrimitive (name, ns, o, xsiType);
		}
	}
	
	// this class tests the methods of the XmlSerializationWriter that
	// can be executed out of order.
	[TestFixture]
	public class XmlSerializationWriterSimpleTests : XmlSerializarionWriterTester
	{
		const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";
		const string XmlSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		const string WsdlTypesNamespace = "http://microsoft.com/wsdl/types/";
		const string ANamespace = "some:urn";
		const string AnotherNamespace = "another:urn";

		// These TestFrom* methods indirectly test the functionality of XmlCustomFormatter

		[Test]
		public void TestFromByteArrayBase64()
		{
			// FIXME
			// This should work according to Mono's API, but .NET's FromByteArrayBase64 
			// returns a byte array.
			// 
			//string val = this.FromByteArrayBase64(new byte [] {143, 144, 1, 0});
			//Assert.AreEqual (FromByteArrayBase64(null), "");
			
			//val = FromByteArrayBase64(null);
			//try/catch or AssertEruals?
		}

		[Test]
		public void TestFromByteArrayHex()
		{
			byte [] vals = {143, 144, 1, 0};
			Assert.AreEqual ("8F900100", FromByteArrayHex(vals));
			Assert.IsNull (FromByteArrayHex (null));
		}

		[Test]
		public void TestFromChar()
		{
			Assert.AreEqual ("97", FromChar ('a'));
			Assert.AreEqual ("0", FromChar ('\0'));
			Assert.AreEqual ("10", FromChar ('\n'));
			Assert.AreEqual ("65281", FromChar ('\uFF01'));
		}

		[Test]
		public void TestFromDate()
		{
			DateTime d = new DateTime();
			Assert.AreEqual ("0001-01-01", FromDate (d));
		}

		[Test]
		public void TestFromDateTime()
		{
			DateTime d = new DateTime();
#if NET_2_0
			Assert.AreEqual ("0001-01-01T00:00:00", FromDateTime (d));
#else
			Assert.AreEqual ("0001-01-01T00:00:00.0000000", FromDateTime (d).Substring (0, 27));
#endif
		}

		[Test] // bug #77500
		public void TestFromEnum()
		{
			long[] ids = {1, 2, 3, 4};
			string[] values = {"one", "two", "three"};

			Assert.AreEqual ("one", FromEnum (1, values, ids), "#1");
			Assert.AreEqual (string.Empty, FromEnum (0, values, ids), "#2");
			Assert.AreEqual ("one two", FromEnum (3, values, ids), "#3");

			try {
				string dummy = FromEnum(4, values, ids);
				Assert.Fail("#4");
			} catch (IndexOutOfRangeException) {
			}

			string[] correctValues = {"one", "two", "three", "four"};
			Assert.AreEqual ("four", FromEnum (4, correctValues, ids), "#5");
			Assert.AreEqual ("one four", FromEnum (5, correctValues, ids), "#6");
			Assert.AreEqual ("two four", FromEnum (6, correctValues, ids), "#7");
			Assert.AreEqual ("one two three four", FromEnum (7, correctValues, ids), "#8");

			string[] flagValues = {"one", "two", "four", "eight"};
			long[] flagIDs = {1, 2, 4, 8};
			Assert.AreEqual (string.Empty, FromEnum (0, flagValues, flagIDs), "#9");
			Assert.AreEqual ("two", FromEnum (2, flagValues, flagIDs), "#10");
			Assert.AreEqual ("four", FromEnum (4, flagValues, flagIDs), "#1");
			Assert.AreEqual ("one four", FromEnum (5, flagValues, flagIDs), "#12");
			Assert.AreEqual ("two four", FromEnum (6, flagValues, flagIDs), "#13");
			Assert.AreEqual ("one two four", FromEnum (7, flagValues, flagIDs), "#14");
			Assert.AreEqual ("eight", FromEnum (8, flagValues, flagIDs), "#15");
			Assert.AreEqual ("one four eight", FromEnum (13, flagValues, flagIDs), "#16");

			string[] unorderedValues = {"one", "four", "two", "zero"};
			long[] unorderedIDs = {1, 4, 2, 0};

			Assert.AreEqual (string.Empty, FromEnum (0, unorderedValues, unorderedIDs), "#17");
			Assert.AreEqual ("two", FromEnum (2, unorderedValues, unorderedIDs), "#18");
			Assert.AreEqual ("four", FromEnum (4, unorderedValues, unorderedIDs), "#19");
			Assert.AreEqual ("one four", FromEnum (5, unorderedValues, unorderedIDs), "#20");
			Assert.AreEqual ("four two", FromEnum (6, unorderedValues, unorderedIDs), "#21");
			Assert.AreEqual ("one four two", FromEnum (7, unorderedValues, unorderedIDs), "#22");

			string[] zeroValues = {"zero", "ten"};
			long[] zeroIDs = {0, 10};

			Assert.AreEqual ("zero", FromEnum (0, zeroValues, zeroIDs), "#9");
			Assert.AreEqual ("ten", FromEnum (10, zeroValues, zeroIDs), "#9");

			string[] reverseZeroValues = {"", "zero"};
			long[] reverseZeroIDs = {4, 0};
			Assert.AreEqual (string.Empty, FromEnum (0, reverseZeroValues, reverseZeroIDs), "#9");
			Assert.AreEqual ("zero", FromEnum (4, reverseZeroValues, reverseZeroIDs), "#9");

			string[] emptyValues = { "zero" };
			long[] emptyIDs = {0};
			Assert.AreEqual ("zero", FromEnum (0, emptyValues, emptyIDs), "#9");
		}

		[Test]
		public void TestFromEnum_InvalidValue ()
		{
			long[] ids = {1, 2, 3, 4};
			string[] values = {"one", "two", "three", "four"};

#if NET_2_0
			try {
				FromEnum (8, values, ids);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("'8'") != -1, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
			}
#else
			Assert.AreEqual ("8", FromEnum (8, values, ids), "#A6");
#endif

#if NET_2_0
			try {
				FromEnum (8, values, ids, "Some.Type.Name");
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'8'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("Some.Type.Name") != -1, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}
#endif
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestFromEnum_Null_Values ()
		{
			long[] ids = { 1, 2, 3, 4 };
			string[] values = { "one", "two", "three", "four" };

			FromEnum (1, (string[]) null, ids);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestFromEnum_Null_IDs ()
		{
			string[] values = { "one", "two", "three", "four" };

			FromEnum (1, values, (long[]) null);
		}

		[Test]
		public void TestFromTime()
		{
			DateTime d = new DateTime();
			// Don't include time zone.
			Assert.AreEqual ("00:00:00.0000000", FromTime (d).Substring (0, 16));
		}

		[Test]
		public void TestFromXmlName()
		{
			Assert.AreEqual ("Hello", FromXmlName ("Hello"));
			Assert.AreEqual ("go_x0020_dogs_x0020_go", FromXmlName ("go dogs go"));
			Assert.AreEqual ("what_x0027_s_x0020_up", FromXmlName ("what's up"));
			Assert.AreEqual ("_x0031_23go", FromXmlName ("123go"));
			Assert.AreEqual ("Hello_x0020_what_x0027_s.up", FromXmlName ("Hello what's.up"));
		}

		[Test]
		public void TestFromXmlNCName()
		{
			Assert.AreEqual ("Hello", FromXmlNCName ("Hello"));
			Assert.AreEqual ("go_x0020_dogs_x0020_go", FromXmlNCName ("go dogs go"));
			Assert.AreEqual ("what_x0027_s_x0020_up", FromXmlNCName ("what's up"));
			Assert.AreEqual ("_x0031_23go", FromXmlNCName ("123go"));
			Assert.AreEqual ("Hello_x0020_what_x0027_s.up", FromXmlNCName ("Hello what's.up"));
		}

		[Test]
		public void TestFromXmlNmToken()
		{
			Assert.AreEqual ("Hello", FromXmlNmToken ("Hello"));
			Assert.AreEqual ("go_x0020_dogs_x0020_go", FromXmlNmToken ("go dogs go"));
			Assert.AreEqual ("what_x0027_s_x0020_up", FromXmlNmToken ("what's up"));
			Assert.AreEqual ("123go", FromXmlNmToken ("123go"));
			Assert.AreEqual ("Hello_x0020_what_x0027_s.up", FromXmlNmToken ("Hello what's.up"));
		}

		[Test]
		public void TestFromXmlNmTokens()
		{
			Assert.AreEqual ("Hello go dogs_go 123go what_x0027_s.up", FromXmlNmTokens ("Hello go dogs_go 123go what's.up"));
		}

		[Test]
		public void TestWriteAttribute()
		{
			WriteStartElement("x");
			WriteAttribute("a", "b");
			WriteEndElement();
			Assert.AreEqual ("<x a='b' />", Content);

			Reset();
			WriteStartElement("x");
			WriteAttribute("a", new byte[] {1, 2, 3});
			WriteEndElement();
			Assert.AreEqual ("<x a='AQID' />", Content);

			Reset();
			WriteStartElement("x");
			WriteAttribute("a", "<b");
			WriteEndElement();
			Assert.AreEqual ("<x a='&lt;b' />", Content);

			Reset();
			WriteStartElement("x");
			string typedPlaceholder = null;
			WriteAttribute("a", typedPlaceholder);
			WriteEndElement();
			Assert.AreEqual ("<x />", Content);

			Reset();
			WriteStartElement("x");
			WriteAttribute("a", "\"");
			WriteEndElement();
			Assert.AreEqual ("<x a='\"' />", Content);

			Reset();
			WriteStartElement("x");
			WriteAttribute("a", "b\nc");
			WriteEndElement();
			Assert.AreEqual ("<x a='b&#xA;c' />", Content);

			Reset();
			WriteStartElement("x");
			WriteAttribute("a", ANamespace, "b");
			WriteEndElement();
			Assert.AreEqual ("<x d1p1:a='b' xmlns:d1p1='some:urn' />", Content);
		}

		[Test]
		public void TestWriteElementEncoded()
		{
			// FIXME
			// XmlNode related
		}

		[Test]
		public void TestWriteElementLiteral()
		{
			// FIXME
			// XmlNode related
		}

		[Test]
		public void TestWriteElementString()
		{
			WriteElementString("x", "a");
			Assert.AreEqual ("<x>a</x>", Content);

			Reset();
			WriteElementString("x", "<a");
			Assert.AreEqual ("<x>&lt;a</x>", Content);
		}

		[Test]
		public void TestWriteElementStringRaw()
		{
			byte [] placeHolderArray = null;
			WriteElementStringRaw("x", placeHolderArray);
			Assert.AreEqual ("", Content);

			Reset();
			WriteElementStringRaw("x", new byte[] {0, 2, 4});
			Assert.AreEqual ("<x>AAIE</x>", Content);

			Reset();
			WriteElementStringRaw("x", new byte[] {});
			Assert.AreEqual ("<x />", Content);

			// Note to reader, the output is not valid xml
			Reset();
			WriteElementStringRaw("x", "a > 13 && a < 19");
			Assert.AreEqual ("<x>a > 13 && a < 19</x>", Content);
		}

		[Test]
		public void TestWriteEmptyTag()
		{
			WriteEmptyTag("x");
			Assert.AreEqual ("<x />", Content);
		}

		[Test]
		public void TestWriteNamespaceDeclarations()
		{
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

			WriteStartElement("x");
			WriteNamespaceDeclarations(ns);
			WriteEndElement();
			Assert.AreEqual ("<x />", Content);

			Reset();
			ns.Add("mypref", ANamespace);
			WriteStartElement("x");
			WriteNamespaceDeclarations(ns);
			WriteEndElement();
			Assert.AreEqual (XmlSerializerTests.Infoset("<x xmlns:mypref='some:urn' />"), XmlSerializerTests.Infoset(Content));

			Reset();
			ns.Add("ns2", "another:urn");
			WriteStartElement("x");
			WriteNamespaceDeclarations(ns);
			WriteEndElement();
			Assert.AreEqual (XmlSerializerTests.Infoset("<x xmlns:ns2='another:urn' xmlns:mypref='some:urn' />"), XmlSerializerTests.Infoset(Content));

			Reset();
			ns.Add("ns3", "ya:urn");
			WriteStartElement("x");
			WriteNamespaceDeclarations(ns);
			WriteEndElement();
			Assert.AreEqual (XmlSerializerTests.Infoset("<x xmlns:ns3='ya:urn' xmlns:ns2='another:urn' xmlns:mypref='some:urn' />"), XmlSerializerTests.Infoset(Content));
		}

		[Test]
		public void TestWriteNullableStringLiteral()
		{
			WriteNullableStringLiteral("x", null, null);
			Assert.AreEqual (XmlSerializerTests.Infoset("<x d1p1:nil='true' xmlns:d1p1='http://www.w3.org/2001/XMLSchema-instance' />"), XmlSerializerTests.Infoset(Content));

			Reset();
			WriteNullableStringLiteral("x", null, "");
			Assert.AreEqual ("<x />", Content);
			
			Reset();
			WriteNullableStringLiteral("x", null, "a<b\'c");
			Assert.AreEqual ("<x>a&lt;b\'c</x>", Content);

			Reset();
			WriteNullableStringLiteral("x", ANamespace, "b");
			Assert.AreEqual ("<x xmlns='some:urn'>b</x>", Content);
		}

		[Test]
		public void TestWriteNullableStringLiteralRaw()
		{
			WriteNullableStringLiteralRaw("x", null, new byte[] {1, 2, 244});
			Assert.AreEqual ("<x>AQL0</x>", Content);
		}

		[Test]
		public void TestWriteNullTagEncoded()
		{
			WriteNullTagEncoded("x");
			Assert.AreEqual (XmlSerializerTests.Infoset("<x d1p1:nil='true' xmlns:d1p1='http://www.w3.org/2001/XMLSchema-instance' />"), XmlSerializerTests.Infoset(Content));
		}

		[Test]
		public void TestWriteNullTagLiteral()
		{
			WriteNullTagLiteral("x");
			Assert.AreEqual (XmlSerializerTests.Infoset("<x d1p1:nil='true' xmlns:d1p1='http://www.w3.org/2001/XMLSchema-instance' />"), XmlSerializerTests.Infoset(Content));
		}

		[Test]
		[Ignore ("Additional namespace prefixes are added")]
		public void TestWritePotentiallyReferencingElement ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWritePotentiallyReferencingElement ("x", ANamespace, EnumDefaultValue.e1, typeof (EnumDefaultValue), true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>1</x>", ANamespace), xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWritePotentiallyReferencingElement ("x", ANamespace, (int) 1, typeof (EnumDefaultValue), true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:int' xmlns:d1p1='{1}' xmlns='{2}'>1</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace),
				xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWritePotentiallyReferencingElement ("x", ANamespace, "something", typeof (string), true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>something</x>", ANamespace), xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWritePotentiallyReferencingElement ("x", ANamespace, "something", null, true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:string' xmlns:d1p1='{1}' xmlns='{2}'>something</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace),
				xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWritePotentiallyReferencingElement ("x", ANamespace, new string[] { "A", "B" }, typeof (string[]), true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<q3:Array id='id1' xmlns:q4='{0}' q3:arrayType='q4:string[2]' xmlns:q3='{1}'>" +
				"<Item>A</Item>" +
				"<Item>B</Item>" +
				"</q3:Array>", XmlSchemaNamespace, SoapEncodingNamespace), xsw.Content, "#5");
		}

		[Test]
		public void TestWriteSerializable()
		{
			// FIXME
			//Assert.AreEqual (, "");
		}

		[Test]
		public void TestWriteStartDocument()
		{
			Assert.AreEqual ("", Content);
			
			WriteStartDocument();
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?>", Content);
		}

		[Test]
		public void TestWriteStartElement()
		{
			WriteStartElement("x");
			WriteEndElement();
			Assert.AreEqual ("<x />", Content);

			Reset();
			WriteStartElement("x");
			WriteValue("a");
			WriteEndElement();
			Assert.AreEqual ("<x>a</x>", Content);

			Reset();
			WriteStartElement("x");
			WriteStartElement("y", "z");
			WriteEndElement();
			WriteEndElement();
			Assert.AreEqual ("<x><y xmlns='z' /></x>", Content);

			Reset();
			WriteStartElement("x");
			WriteStartElement("y", "z", true);
			WriteEndElement();
			WriteEndElement();
			Assert.AreEqual ("<x><q1:y xmlns:q1='z' /></x>", Content);
		}

		[Test]
		public void TestWriteTypedPrimitive_Base64Binary ()
		{
			byte[] byteArray = new byte[] { 255, 20, 10, 5, 0, 7 };
			string expected = "/xQKBQAH";

			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, byteArray, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>{1}</x>", ANamespace, expected),
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, byteArray, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x>{0}</x>", expected), xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, byteArray, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x>{0}</x>", expected), xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, byteArray, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>{1}</x>", XmlSchemaNamespace, expected),
				xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, byteArray, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>{1}</x>", XmlSchemaInstanceNamespace, expected),
				xsw.Content, "#5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, byteArray, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns='{0}'>{1}</>", ANamespace, expected), xsw.Content, "#6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, byteArray, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<base64Binary xmlns='{0}'>{1}</base64Binary>",
				XmlSchemaNamespace, expected), xsw.Content, "#7");
		}

		[Test]
		public void TestWriteTypedPrimitive_Base64Binary_XsiType ()
		{
			byte[] byteArray = new byte[] { 255, 20, 10, 5, 0, 7 };
			string expected = "/xQKBQAH";

			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, byteArray, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:base64Binary' xmlns:d1p1='{1}'>{2}</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, expected), 
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, byteArray, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:base64Binary' xmlns:d1p1='{1}'>{2}</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, expected),
				xsw.Content, "#2");
		}

		[Test]
		public void TestWriteTypedPrimitive_Boolean ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>true</x>", ANamespace), xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, false, false);
			Assert.AreEqual ("<x>false</x>", xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, true, false);
			Assert.AreEqual ("<x>true</x>", xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>false</x>", XmlSchemaNamespace), xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>true</x>", XmlSchemaInstanceNamespace),
				xsw.Content, "#5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns='{0}'>false</>", ANamespace), xsw.Content, "#6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<boolean xmlns='{0}'>true</boolean>", XmlSchemaNamespace),
				xsw.Content, "#7");
		}

		[Test]
		public void TestWriteTypedPrimitive_Boolean_XsiType ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:boolean' xmlns:d1p1='{1}'>true</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:boolean' xmlns:d1p1='{1}'>false</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#2");
		}

		[Test]
		public void TestWriteTypedPrimitive_Char ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, 'c', false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>99</x>", ANamespace), xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, 'a', false);
			Assert.AreEqual ("<x>97</x>", xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, 'b', false);
			Assert.AreEqual ("<x>98</x>", xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, 'd', false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>100</x>", XmlSchemaNamespace), xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, 'e', false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>101</x>", XmlSchemaInstanceNamespace),
				xsw.Content, "#5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, ' ', false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns='{0}'>32</>", ANamespace), xsw.Content, "#6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, '0', false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<char xmlns='{0}'>48</char>", WsdlTypesNamespace),
				xsw.Content, "#7");
		}

		[Test]
		public void TestWriteTypedPrimitive_Char_XsiType ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, 'c', true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:char' xmlns:d1p1='{1}'>99</x>",
				WsdlTypesNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, 'a', true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:char' xmlns:d1p1='{1}'>97</x>",
				WsdlTypesNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#2");
		}

		[Test]
		public void TestWriteTypedPrimitive_DateTime ()
		{
			DateTime dateTime = new DateTime (1973, 08, 13);

			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, dateTime, false);
			// FIXME: This is a bad test case. The following switch
			// should be applied to the entire test.
#if NET_2_0
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>1973-08-13T00:00:00</x>", ANamespace),
				xsw.Content, "#1");
#else
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>{1}</x>", ANamespace, FromDateTime (dateTime)),
				xsw.Content, "#1");
#endif
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, dateTime, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x>{0}</x>", FromDateTime (dateTime)), xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, dateTime, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x>{0}</x>", FromDateTime (dateTime)), xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, dateTime, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>{1}</x>", XmlSchemaNamespace, 
				FromDateTime (dateTime)), xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, dateTime, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>{1}</x>", XmlSchemaInstanceNamespace,
				FromDateTime (dateTime)), xsw.Content, "#5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, dateTime, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns='{0}'>{1}</>", ANamespace, FromDateTime (dateTime)),
				xsw.Content, "#6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, dateTime, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<dateTime xmlns='{0}'>{1}</dateTime>", XmlSchemaNamespace,
				FromDateTime (dateTime)), xsw.Content, "#7");
		}

		// FIXME: This is a bad test case.
		// See TestWriteTypedPrimitive_DateTime.
		[Test]
		public void TestWriteTypedPrimitive_DateTime_XsiType ()
		{
			DateTime dateTime = new DateTime (1973, 08, 13);

			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, dateTime, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:dateTime' xmlns:d1p1='{1}'>{2}</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, 
				FromDateTime (dateTime)), xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, dateTime, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:dateTime' xmlns:d1p1='{1}'>{2}</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace,
				FromDateTime (dateTime)), xsw.Content, "#2");
		}

		[Test]
		[Category ("NotWorking")] // enum name is output instead of integral value
		public void TestWriteTypedPrimitive_Enum ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, EnumDefaultValue.e1, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>1</x>", ANamespace), xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, EnumDefaultValue.e2, false);
			Assert.AreEqual ("<x>2</x>", xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, EnumDefaultValue.e3, false);
			Assert.AreEqual ("<x>3</x>", xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, EnumDefaultValue.e1, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>1</x>", XmlSchemaNamespace), xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, EnumDefaultValue.e2, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>2</x>", XmlSchemaInstanceNamespace),
				xsw.Content, "#5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, EnumDefaultValue.e3, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns='{0}'>3</>", ANamespace), xsw.Content, "#6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, EnumDefaultValue.e2, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<int xmlns='{0}'>2</int>", XmlSchemaNamespace),
				xsw.Content, "#7");
		}

		[Test]
		[Category ("NotWorking")] // InvalidOperationException is thrown
		public void TestWriteTypedPrimitive_Enum_XsiType ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, EnumDefaultValue.e1, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:int' xmlns:d1p1='{1}'>1</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, EnumDefaultValue.e2, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:int' xmlns:d1p1='{1}'>2</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#2");
		}

		[Test]
		public void TestWriteTypedPrimitive_Guid ()
		{
			Guid guid = new Guid ("CA761232-ED42-11CE-BACD-00AA0057B223");
			string expectedGuid = "ca761232-ed42-11ce-bacd-00aa0057b223";

			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, guid, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>{1}</x>", ANamespace, expectedGuid), 
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, guid, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x>{0}</x>", expectedGuid), xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, guid, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x>{0}</x>", expectedGuid), xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, guid, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>{1}</x>", XmlSchemaNamespace, expectedGuid),
				xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, guid, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>{1}</x>", XmlSchemaInstanceNamespace, expectedGuid),
				xsw.Content, "#5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, guid, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns='{0}'>{1}</>", ANamespace, expectedGuid), 
				xsw.Content, "#6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, guid, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<guid xmlns='{0}'>{1}</guid>", WsdlTypesNamespace, 
				expectedGuid), xsw.Content, "#7");
		}

		[Test]
		public void TestWriteTypedPrimitive_Guid_XsiType ()
		{
			Guid guid = new Guid ("CA761232-ED42-11CE-BACD-00AA0057B223");
			string expectedGuid = "ca761232-ed42-11ce-bacd-00aa0057b223";

			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, guid, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:guid' xmlns:d1p1='{1}'>{2}</x>",
				WsdlTypesNamespace, XmlSchemaInstanceNamespace, expectedGuid),
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, guid, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:guid' xmlns:d1p1='{1}'>{2}</x>",
				WsdlTypesNamespace, XmlSchemaInstanceNamespace, expectedGuid),
				xsw.Content, "#2");
		}

		[Test]
		public void TestWriteTypedPrimitive_Int ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, 76665, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>76665</x>", ANamespace), xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, -5656, false);
			Assert.AreEqual ("<x>-5656</x>", xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, 0, false);
			Assert.AreEqual ("<x>0</x>", xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, 534, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>534</x>", XmlSchemaNamespace), xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, -6756, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>-6756</x>", XmlSchemaInstanceNamespace),
				xsw.Content, "#5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, 434, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns='{0}'>434</>", ANamespace), xsw.Content, "#6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, 434, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<int xmlns='{0}'>434</int>", XmlSchemaNamespace),
				xsw.Content, "#7");
		}

		[Test]
		public void TestWriteTypedPrimitive_Int_XsiType ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, -6756, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:int' xmlns:d1p1='{1}'>-6756</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, 434, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:int' xmlns:d1p1='{1}'>434</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#2");
		}

		[Test]
		public void TestWriteTypedPrimitive_String ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, "hello", false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>hello</x>", ANamespace), xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, "hello", false);
			Assert.AreEqual ("<x>hello</x>", xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, "hello", false);
			Assert.AreEqual ("<x>hello</x>", xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, "hello", false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>hello</x>", XmlSchemaNamespace),
				xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, "hello", false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>hello</x>", XmlSchemaInstanceNamespace),
				xsw.Content, "#5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, string.Empty, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}' />", ANamespace), xsw.Content, "#6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, "<\"te'st\">", false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns='{0}'>&lt;\"te'st\"&gt;</>", ANamespace),
				xsw.Content, "#7");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, "hello", false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<string xmlns='{0}'>hello</string>", XmlSchemaNamespace),
				xsw.Content, "#8");
		}

		[Test]
		public void TestWriteTypedPrimitive_String_XsiType ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, "hello", true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:string' xmlns:d1p1='{1}'>hello</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace), 
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, "hello", true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:string' xmlns:d1p1='{1}'>hello</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#2");
		}

		[Test]
		public void TestWriteTypedPrimitive_String_XsiType_Namespace ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, "hello", true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:string' xmlns:d1p1='{1}' xmlns='{2}'>hello</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace),
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, "hello", true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x d1p1:type='string' xmlns:d1p1='{0}' xmlns='{1}'>hello</x>",
				XmlSchemaInstanceNamespace, XmlSchemaNamespace),
				xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, "hello", true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:string' xmlns:d1p1='{1}' xmlns='{1}'>hello</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace), xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, string.Empty, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q3='{0}' d1p1:type='q3:string' xmlns:d1p1='{1}' xmlns='{2}' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace),
				xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, "<\"te'st\">", true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns:q4='{0}' d1p1:type='q4:string' xmlns:d1p1='{1}' xmlns='{2}'>&lt;\"te'st\"&gt;</>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace),
				xsw.Content, "#5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, "hello", true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<string d1p1:type='string' xmlns:d1p1='{0}' xmlns='{1}'>hello</string>",
				XmlSchemaInstanceNamespace, XmlSchemaNamespace), 
				xsw.Content, "#6");
		}

		[Test]
		public void TestWriteTypedPrimitive_UnsignedByte ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, (byte) 5, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>5</x>", ANamespace), xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, (byte) 125, false);
			Assert.AreEqual ("<x>125</x>", xsw.Content, "#2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, (byte) 0, false);
			Assert.AreEqual ("<x>0</x>", xsw.Content, "#3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, (byte) 255, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>255</x>", XmlSchemaNamespace), xsw.Content, "#4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, (byte) 128, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>128</x>", XmlSchemaInstanceNamespace),
				xsw.Content, "#5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, (byte) 1, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns='{0}'>1</>", ANamespace), xsw.Content, "#6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, (byte) 99, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<unsignedByte xmlns='{0}'>99</unsignedByte>",
				XmlSchemaNamespace), xsw.Content, "#7");
		}

		[Test]
		public void TestWriteTypedPrimitive_UnsignedByte_XsiType ()
		{
			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, (byte) 5, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' d1p1:type='q1:unsignedByte' xmlns:d1p1='{1}'>5</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, (byte) 99, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:unsignedByte' xmlns:d1p1='{1}'>99</x>",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace),
				xsw.Content, "#2");
		}

		[Test]
		public void TestWriteTypedPrimitive_XmlQualifiedName ()
		{
			XmlQualifiedName qname = new XmlQualifiedName ("something", AnotherNamespace);

			XmlSerializarionWriterTester xsw = new XmlSerializarionWriterTester ();
			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q1='{0}' xmlns='{1}'>q1:something</x>", 
				AnotherNamespace, ANamespace), xsw.Content, "#A1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}'>q2:something</x>",
				AnotherNamespace), xsw.Content, "#A2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q3='{0}'>q3:something</x>", AnotherNamespace), 
				xsw.Content, "#A3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q4='{0}' xmlns='{1}'>q4:something</x>", AnotherNamespace, 
				XmlSchemaNamespace), xsw.Content, "#A4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q5='{0}' xmlns='{1}'>q5:something</x>", AnotherNamespace, 
				XmlSchemaInstanceNamespace), xsw.Content, "#A5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns:q6='{0}' xmlns='{1}'>q6:something</>", AnotherNamespace,
				ANamespace), xsw.Content, "#A6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<QName xmlns:q7='{0}' xmlns='{1}'>q7:something</QName>",
				AnotherNamespace, XmlSchemaNamespace), xsw.Content, "#A7");

			xsw.Reset ();

			qname = new XmlQualifiedName ("else");

			xsw.ExecuteWriteTypedPrimitive ("x", ANamespace, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>else</x>", ANamespace), xsw.Content, "#B1");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", string.Empty, qname, false);
			Assert.AreEqual ("<x>else</x>", xsw.Content, "#B2");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", null, qname, false);
			Assert.AreEqual ("<x>else</x>", xsw.Content, "#B3");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaNamespace, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>else</x>", XmlSchemaNamespace), xsw.Content, "#B4");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive ("x", XmlSchemaInstanceNamespace, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}'>else</x>", XmlSchemaInstanceNamespace), 
				xsw.Content, "#B5");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (string.Empty, ANamespace, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"< xmlns='{0}'>else</>", ANamespace), xsw.Content, "#B6");

			xsw.Reset ();

			xsw.ExecuteWriteTypedPrimitive (null, ANamespace, qname, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<QName xmlns='{0}'>else</QName>", XmlSchemaNamespace), 
				xsw.Content, "#B7");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestWriteTypedPrimitive_Null_Value()
		{
			WriteTypedPrimitive("x", ANamespace, null, false);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestWriteTypedPrimitive_NonPrimitive ()
		{
			// The type System.Version was not expected. Use the XmlInclude
			// or SoapInclude attribute to specify types that are not known
			// statically.
			WriteTypedPrimitive ("x", ANamespace, new Version (), false);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestWriteTypedPrimitive_XmlNode ()
		{
			WriteTypedPrimitive ("x", ANamespace, new XmlDocument ().CreateElement ("foo"), false);
		}

		[Test]
		public void TestWriteValue()
		{
			WriteValue("");
			Assert.AreEqual ("", Content);

			Reset();
			WriteValue("hello");
			Assert.AreEqual ("hello", Content);

			Reset();
			string v = null;
			WriteValue(v);
			Assert.AreEqual ("", Content);

			Reset();
			WriteValue(new byte[] {13, 8, 99});
			Assert.AreEqual ("DQhj", Content);
		}

		public void TestWriteXmlAttribute()
		{
			// FIXME
			// XmlNode related
		}

		[Test]
		public void TestWriteXsiType()
		{
			WriteStartElement("x");
			WriteXsiType("pref", null);
			WriteEndElement();
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"<x d1p1:type='pref' xmlns:d1p1='{0}' />", XmlSchemaInstanceNamespace),
				Content, "#1");

			Reset ();

			WriteStartElement ("x");
			WriteXsiType ("int", XmlSchemaNamespace);
			WriteEndElement ();
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"<x xmlns:q2='{0}' d1p1:type='q2:int' xmlns:d1p1='{1}' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace),
				Content, "#2");

			Reset ();

			WriteStartElement ("x");
			WriteXsiType ("int", ANamespace);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q3='{0}' d1p1:type='q3:int' xmlns:d1p1='{1}' />",
				ANamespace, XmlSchemaInstanceNamespace), Content, "#3");

			Reset ();

			WriteStartElement ("x");
			WriteXsiType ("int", XmlSchemaInstanceNamespace);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q4='{0}' q4:type='q4:int' />",
				XmlSchemaInstanceNamespace), Content, "#4");

			Reset ();

			WriteStartElement ("x");
			WriteXsiType ("int", string.Empty);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x d1p1:type='int' xmlns:d1p1='{0}' />", XmlSchemaInstanceNamespace),
				Content, "#5");

			Reset ();

			WriteStartElement ("x");
			WriteXsiType (string.Empty, null);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x d1p1:type='' xmlns:d1p1='{0}' />", XmlSchemaInstanceNamespace),
				Content, "#6");

			Reset ();

			WriteStartElement ("x");
			WriteXsiType (null, null);
			WriteEndElement ();
			Assert.AreEqual ("<x />", Content, "#7");
		}

		[Test]
		public void TestWriteXsiType_Namespace ()
		{
			WriteStartElement ("x", ANamespace);
			WriteXsiType ("pref", null);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x d1p1:type='pref' xmlns:d1p1='{0}' xmlns='{1}' />", 
				XmlSchemaInstanceNamespace, ANamespace), Content, "#1");

			Reset ();

			WriteStartElement ("x", ANamespace);
			WriteXsiType ("int", XmlSchemaNamespace);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q5='{0}' d1p1:type='q5:int' xmlns:d1p1='{1}' xmlns='{2}' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace),
				Content, "#2");

			Reset ();

			WriteStartElement ("x", ANamespace);
			WriteXsiType ("int", ANamespace);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x d1p1:type='int' xmlns:d1p1='{1}' xmlns='{2}' />",
				ANamespace, XmlSchemaInstanceNamespace, ANamespace), 
				Content, "#3");

			Reset ();

			WriteStartElement ("x", ANamespace);
			WriteXsiType ("int", XmlSchemaInstanceNamespace);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns:q6='{0}' q6:type='q6:int' xmlns='{1}' />",
				XmlSchemaInstanceNamespace, ANamespace), Content, "#4");

			Reset ();

			WriteStartElement ("x", ANamespace);
			WriteXsiType ("int", string.Empty);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x d1p1:type='int' xmlns:d1p1='{0}' xmlns='{1}' />",
				XmlSchemaInstanceNamespace, ANamespace), Content, "#5");

			Reset ();

			WriteStartElement ("x", ANamespace);
			WriteXsiType (string.Empty, null);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x d1p1:type='' xmlns:d1p1='{0}' xmlns='{1}' />",
				XmlSchemaInstanceNamespace, ANamespace), Content, "#6");

			Reset ();

			WriteStartElement ("x", ANamespace);
			WriteXsiType (null, null);
			WriteEndElement ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<x xmlns='{0}' />", ANamespace), Content, "#7");
		}


#if NET_2_0
		[Test]
		public void TestFromEnum_Null_TypeName ()
		{
			string[] values = { "one", "two", "three", "four" };
			long[] ids = { 1, 2, 3, 4 };

			Assert.AreEqual ("one", FromEnum (1, values, ids, (string) null));
		}

		[Test]
		public void TestCreateInvalidEnumValueException ()
		{
			Exception ex = CreateInvalidEnumValueException("AnInvalidValue", "SomeType");
			Assert.IsNotNull (ex, "#1");
			Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
			Assert.IsNotNull (ex.Message, "#3");
			Assert.IsTrue (ex.Message.IndexOf ("AnInvalidValue") != -1, "#4");
			Assert.IsTrue (ex.Message.IndexOf ("SomeType") != -1, "#5");
		}
#endif

		[Test]
		public void WriteCharacter ()
		{
			// mostly from bug #673019
			var SerializerObj = new XmlSerializer (typeof (ToBeSerialized));
			StringWriter writer = new StringWriter ();
			SerializerObj.Serialize (writer, new ToBeSerialized ());
			Assert.IsTrue (writer.ToString ().IndexOf ("<character>39</character>") > 0, "#1");
		}

		[Serializable]
		public class ToBeSerialized
		{
			[DefaultValue ('a')]
			public char character = '\'';
		}

		[Test]
		public void TestNullableDatesAndTimes ()
		{
			DateTime dt = new DateTime (2012, 1, 3, 10, 0, 0, 0);
			
			var d = new NullableDatesAndTimes () {
				MyTime = dt,
				MyTimeNullable = dt,
				MyDate = dt,
				MyDateNullable = dt
			};
			
			XmlSerializer ser = new XmlSerializer (d.GetType ());
			StringWriter sw = new StringWriter ();
			ser.Serialize (sw, d);
			string str = sw.ToString ();

			Assert.IsTrue (str.IndexOf ("<MyTime>10:00:00</MyTime>") != -1, "Time");
			Assert.IsTrue (str.IndexOf ("<MyTimeNullable>10:00:00</MyTimeNullable>") != -1, "Nullable Time");
			Assert.IsTrue (str.IndexOf ("<MyDate>2012-01-03</MyDate>") != -1, "Date");
			Assert.IsTrue (str.IndexOf ("<MyDateNullable>2012-01-03</MyDateNullable>") != -1, "Nullable Datwe");
		}
		
		
	}
}
