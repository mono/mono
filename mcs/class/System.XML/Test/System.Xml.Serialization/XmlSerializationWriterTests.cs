//
// MonoTests.System.Xml.Serialization.XmlSerializationWriterTests
//
// Author: Erik LeBel <eriklebel@yahoo.ca>
//
//  (C) Erik LeBel 2003
//  
// FIXME add tests for callbacks
// FIXME add tests for writes that generate namespaces
// FIXME add test that write XmlNode objects
// 

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Xml.Serialization
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
		protected void Reset()
		{
			sw = new StringWriter ();
			writer = new XmlTextWriter (sw);
			writer.QuoteChar = '\'';
			writer.Formatting = Formatting.None;
			Writer = writer;
		}

		protected string Content
		{
			get
			{ 
				string val = sw.GetStringBuilder().ToString();
//				Console.WriteLine(val);
				return val;
			}
		}
	}
	
	// this class tests the methods of the XmlSerializationWriter that
	// can be executed out of order.
	[TestFixture]
	public class XmlSerializationWriterSimpleTests : XmlSerializarionWriterTester
	{
		const string ANamespace = "some:urn";

		// These TestFrom* methods indirectly test the functionality of XmlCustomFormatter

		[Test]
		public void TestFromByteArrayBase64()
		{
			// FIXME
			// This should work according to Mono's API, but .NET's FromByteArrayBase64 
			// returns a byte array.
			// 
			//string val = this.FromByteArrayBase64(new byte [] {143, 144, 1, 0});
			//Assertion.AssertEquals(FromByteArrayBase64(null), "");
			
			//val = FromByteArrayBase64(null);
			//try/catch or AssertEruals?
		}

		[Test]
		public void TestFromByteArrayHex()
		{
			byte [] vals = {143, 144, 1, 0};
			Assertion.AssertEquals("8F900100", FromByteArrayHex(vals));
			Assertion.AssertEquals(null, FromByteArrayHex(null));
		}

		[Test]
		public void TestFromChar()
		{
			Assertion.AssertEquals("97", FromChar('a'));
			Assertion.AssertEquals("0", FromChar('\0'));
			Assertion.AssertEquals("10", FromChar('\n'));
			Assertion.AssertEquals("65281", FromChar('\uFF01'));
		}

		[Test]
		public void TestFromDate()
		{
			DateTime d = new DateTime();
			Assertion.AssertEquals("0001-01-01", FromDate(d));
		}

		[Test]
		public void TestFromDateTime()
		{
			DateTime d = new DateTime();
			Assertion.AssertEquals("0001-01-01T00:00:00.0000000", FromDateTime(d).Substring (0, 27));
		}

		[Test]
		public void TestFromEnum()
		{
			long[] ids = {1, 2, 3, 4};
			string[] values = {"one", "two", "three"};
			
			Assertion.AssertEquals("one", FromEnum(1, values, ids));
			Assertion.AssertEquals("", FromEnum(0, values, ids));

			try
			{
				string dummy = FromEnum(4, values, ids);
				Assertion.Fail("This should fail with an array-out-of-bunds error");
			}
			catch (Exception)
			{
			}
		}

		[Test]
		public void TestFromTime()
		{
			DateTime d = new DateTime();
			// Don't include time zone.
			Assertion.AssertEquals("00:00:00.0000000", FromTime(d).Substring (0, 16));
		}

		[Test]
		public void TestFromXmlName()
		{
			Assertion.AssertEquals("Hello", FromXmlName("Hello"));
			Assertion.AssertEquals("go_x0020_dogs_x0020_go", FromXmlName("go dogs go"));
			Assertion.AssertEquals("what_x0027_s_x0020_up", FromXmlName("what's up"));
			Assertion.AssertEquals("_x0031_23go", FromXmlName("123go"));
			Assertion.AssertEquals("Hello_x0020_what_x0027_s.up", FromXmlName("Hello what's.up"));
		}

		[Test]
		public void TestFromXmlNCName()
		{
			Assertion.AssertEquals("Hello", FromXmlNCName("Hello"));
			Assertion.AssertEquals("go_x0020_dogs_x0020_go", FromXmlNCName("go dogs go"));
			Assertion.AssertEquals("what_x0027_s_x0020_up", FromXmlNCName("what's up"));
			Assertion.AssertEquals("_x0031_23go", FromXmlNCName("123go"));
			Assertion.AssertEquals("Hello_x0020_what_x0027_s.up", FromXmlNCName("Hello what's.up"));
		}

		[Test]
		public void TestFromXmlNmToken()
		{
			Assertion.AssertEquals("Hello", FromXmlNmToken("Hello"));
			Assertion.AssertEquals("go_x0020_dogs_x0020_go", FromXmlNmToken("go dogs go"));
			Assertion.AssertEquals("what_x0027_s_x0020_up", FromXmlNmToken("what's up"));
			Assertion.AssertEquals("123go", FromXmlNmToken("123go"));
			Assertion.AssertEquals("Hello_x0020_what_x0027_s.up", FromXmlNmToken("Hello what's.up"));
		}

		[Test]
		public void TestFromXmlNmTokens()
		{
			Assertion.AssertEquals("Hello go dogs_go 123go what_x0027_s.up", FromXmlNmTokens("Hello go dogs_go 123go what's.up"));
		}

		[Test]
		public void TestWriteAttribute()
		{
			WriteStartElement("x");
			WriteAttribute("a", "b");
			WriteEndElement();
			Assertion.AssertEquals("<x a='b' />", Content);

			Reset();
			WriteStartElement("x");
			WriteAttribute("a", new byte[] {1, 2, 3});
			WriteEndElement();
			Assertion.AssertEquals("<x a='AQID' />", Content);

			Reset();
			WriteStartElement("x");
			WriteAttribute("a", "<b");
			WriteEndElement();
			Assertion.AssertEquals("<x a='&lt;b' />", Content);

			Reset();
			WriteStartElement("x");
			string typedPlaceholder = null;
			WriteAttribute("a", typedPlaceholder);
			WriteEndElement();
			Assertion.AssertEquals("<x />", Content);

			Reset();
			WriteStartElement("x");
			WriteAttribute("a", "\"");
			WriteEndElement();
			Assertion.AssertEquals("<x a='\"' />", Content);

			Reset();
			WriteStartElement("x");
			WriteAttribute("a", "b\nc");
			WriteEndElement();
			Assertion.AssertEquals("<x a='b&#xA;c' />", Content);

			Reset();
			WriteStartElement("x");
			WriteAttribute("a", ANamespace, "b");
			WriteEndElement();
			Assertion.AssertEquals("<x d1p1:a='b' xmlns:d1p1='some:urn' />", Content);


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
			Assertion.AssertEquals("<x>a</x>", Content);

			Reset();
			WriteElementString("x", "<a");
			Assertion.AssertEquals("<x>&lt;a</x>", Content);
		}

		[Test]
		public void TestWriteElementStringRaw()
		{
			byte [] placeHolderArray = null;
			WriteElementStringRaw("x", placeHolderArray);
			Assertion.AssertEquals("", Content);

			Reset();
			WriteElementStringRaw("x", new byte[] {0, 2, 4});
			Assertion.AssertEquals("<x>AAIE</x>", Content);

			Reset();
			WriteElementStringRaw("x", new byte[] {});
			Assertion.AssertEquals("<x />", Content);

			// Note to reader, the output is not valid xml
			Reset();
			WriteElementStringRaw("x", "a > 13 && a < 19");
			Assertion.AssertEquals("<x>a > 13 && a < 19</x>", Content);
		}

		[Test]
		public void TestWriteEmptyTag()
		{
			WriteEmptyTag("x");
			Assertion.AssertEquals("<x />", Content);
		}

		[Test]
		public void TestWriteNamespaceDeclarations()
		{
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

			WriteStartElement("x");
			WriteNamespaceDeclarations(ns);
			WriteEndElement();
			Assertion.AssertEquals("<x />", Content);

			Reset();
			ns.Add("mypref", ANamespace);
			WriteStartElement("x");
			WriteNamespaceDeclarations(ns);
			WriteEndElement();
			Assertion.AssertEquals("<x xmlns:mypref='some:urn' />", Content);

			Reset();
			ns.Add("ns2", "another:urn");
			WriteStartElement("x");
			WriteNamespaceDeclarations(ns);
			WriteEndElement();
			Assertion.AssertEquals("<x xmlns:ns2='another:urn' xmlns:mypref='some:urn' />", Content);

			Reset();
			ns.Add("ns3", "ya:urn");
			WriteStartElement("x");
			WriteNamespaceDeclarations(ns);
			WriteEndElement();
			Assertion.AssertEquals("<x xmlns:ns3='ya:urn' xmlns:ns2='another:urn' xmlns:mypref='some:urn' />", Content);
		}

		[Test]
		public void TestWriteNullableStringLiteral()
		{
			WriteNullableStringLiteral("x", null, null);
			Assertion.AssertEquals("<x d1p1:nil='true' xmlns:d1p1='http://www.w3.org/2001/XMLSchema-instance' />", Content);

			Reset();
			WriteNullableStringLiteral("x", null, "");
			Assertion.AssertEquals("<x />", Content);
			
			Reset();
			WriteNullableStringLiteral("x", null, "a<b\'c");
			Assertion.AssertEquals("<x>a&lt;b\'c</x>", Content);

			Reset();
			WriteNullableStringLiteral("x", ANamespace, "b");
			Assertion.AssertEquals("<x xmlns='some:urn'>b</x>", Content);
		}

		[Test]
		public void TestWriteNullableStringLiteralRaw()
		{
			WriteNullableStringLiteralRaw("x", null, new byte[] {1, 2, 244});
			Assertion.AssertEquals("<x>AQL0</x>", Content);
		}

		[Test]
		public void TestWriteNullTagEncoded()
		{
			WriteNullTagEncoded("x");
			Assertion.AssertEquals("<x d1p1:nil='true' xmlns:d1p1='http://www.w3.org/2001/XMLSchema-instance' />", Content);
		}

		[Test]
		public void TestWriteNullTagLiteral()
		{
			WriteNullTagLiteral("x");
			Assertion.AssertEquals("<x d1p1:nil='true' xmlns:d1p1='http://www.w3.org/2001/XMLSchema-instance' />", Content);
		}

		[Test]
		public void TestWriteSerializable()
		{
			// FIXME
			//Assertion.AssertEquals(, "");
		}

		public void TestWriteStartDocument()
		{
			Assertion.AssertEquals("", Content);
			
			WriteStartDocument();
			Assertion.AssertEquals("<?xml version='1.0' encoding='utf-16'?>", Content);
		}

		[Test]
		public void TestWriteStartElement()
		{
			WriteStartElement("x");
			WriteEndElement();
			Assertion.AssertEquals("<x />", Content);

			Reset();
			WriteStartElement("x");
			WriteValue("a");
			WriteEndElement();
			Assertion.AssertEquals("<x>a</x>", Content);

			Reset();
			WriteStartElement("x");
			WriteStartElement("y", "z");
			WriteEndElement();
			WriteEndElement();
			Assertion.AssertEquals("<x><y xmlns='z' /></x>", Content);

			Reset();
			WriteStartElement("x");
			WriteStartElement("y", "z", true);
			WriteEndElement();
			WriteEndElement();
			Assertion.AssertEquals("<x><q1:y xmlns:q1='z' /></x>", Content);
		}
		
		public void TestWriteTypedPrimitive()
		{
			// as long as WriteTypePrimitive's last argument is false, this is OK here.
			WriteTypedPrimitive("x", ANamespace, "hello", false);
			Assertion.AssertEquals("<x xmlns='some:urn'>hello</x>", Content);

			Reset();
			WriteTypedPrimitive("x", ANamespace, 10, false);
			Assertion.AssertEquals("<x xmlns='some:urn'>10</x>", Content);

			try
			{
				WriteTypedPrimitive("x", ANamespace, null, false);
				Assertion.Fail("Should not be able to write a null primitive");
			}
			catch (Exception)
			{
			}
		}

		public void TestWriteValue()
		{
			WriteValue("");
			Assertion.AssertEquals("", Content);

			Reset();
			WriteValue("hello");
			Assertion.AssertEquals("hello", Content);

			Reset();
			string v = null;
			WriteValue(v);
			Assertion.AssertEquals("", Content);

			Reset();
			WriteValue(new byte[] {13, 8, 99});
			Assertion.AssertEquals("DQhj", Content);
		}

		public void TestWriteXmlAttribute()
		{
			// FIXME
			// XmlNode related
		}

		public void TestWriteXsiType()
		{
			WriteStartElement("x");
			WriteXsiType("pref", null);
			WriteEndElement();
			Assertion.AssertEquals("<x d1p1:type='pref' xmlns:d1p1='http://www.w3.org/2001/XMLSchema-instance' />", Content);
		}
	}
}
