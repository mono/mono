//
// System.Xml.DeserializationTests
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
//
//
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	public class Sample
	{
		public string Text;
		public string [] ArrayText;
	}

	[TestFixture]
	public class DeserializationTests
	{
		object result;

		private object Deserialize (Type t, string xml)
		{
			StringReader sr = new StringReader (xml);
			XmlReader xr = new XmlTextReader (sr);
			return Deserialize (t, xr);
		}

		private object DeserializeEncoded (Type t, string xml)
		{
			StringReader sr = new StringReader (xml);
			XmlReader xr = new XmlTextReader (sr);
			return DeserializeEncoded (t, xr);
		}

		private object Deserialize (Type t, XmlReader xr)
		{
			XmlSerializer ser = new XmlSerializer (t);
			result = ser.Deserialize (xr);
			return result;
		}

		private object DeserializeEncoded (Type t, XmlReader xr)
		{
			SoapReflectionImporter im = new SoapReflectionImporter ();
			XmlTypeMapping tm = im.ImportTypeMapping (t);
			XmlSerializer ser = new XmlSerializer (tm);
			result = ser.Deserialize (xr);
			return result;
		}

		[Test]
		public void SimpleDeserialize ()
		{
			Deserialize (typeof (Sample), "<Sample><Text>Test.</Text></Sample>");
			Assertion.AssertEquals (typeof (Sample), result.GetType ());
			Sample sample = result as Sample;
			Assertion.AssertEquals ("Test.", sample.Text);
		}

		[Test]
		public void DeserializeInt ()
		{
			Deserialize (typeof (int), "<int>10</int>");
			Assertion.AssertEquals (typeof (int), result.GetType ());
			Assertion.AssertEquals (10, result);
		}

		[Test]
		public void DeserializeSimpleArray ()
		{
			Deserialize (typeof (Sample), "<Sample><ArrayText><string>Test1</string><string>Test2</string></ArrayText></Sample>");
			Assertion.AssertEquals (typeof (Sample), result.GetType ());
			Sample sample = result as Sample;
			Assertion.AssertEquals ("Test1", sample.ArrayText [0]);
			Assertion.AssertEquals ("Test2", sample.ArrayText [1]);
		}

		[Test]
		public void DeserializeEmptyEnum ()
		{
			Field f = Deserialize (typeof (Field), "<field modifiers=\"\" />") as Field;
			Assertion.AssertEquals (MapModifiers.Public, f.Modifiers);
		}
		
		[Test]
		public void DeserializePrivateCollection ()
		{
			MemoryStream ms = new MemoryStream ();
			Container c = new Container();
			c.Items.Add(1);
			
			XmlSerializer serializer = new XmlSerializer(typeof(Container));
			serializer.Serialize(ms, c);
			
			ms.Position = 0;
			c = (Container) serializer.Deserialize (ms);
			Assertion.AssertEquals (1, c.Items[0]);
		}
		
		[Test]
		[Category("NotDotNet")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DeserializeEmptyPrivateCollection ()
		{
			MemoryStream ms = new MemoryStream ();
			Container2 c = new Container2(true);
			c.Items.Add(1);
			
			XmlSerializer serializer = new XmlSerializer(typeof(Container2));
			serializer.Serialize(ms, c);
			
			ms.Position = 0;
			c = (Container2) serializer.Deserialize (ms);
		}
		
		[Test]
		[Category("NotDotNet")]
		public void DeserializeArrayReferences ()
		{
			string s = "<Sample xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";
			s += "<ArrayText xmlns:n3=\"http://schemas.xmlsoap.org/soap/encoding/\" xsi:type=\"n3:Array\" n3:arrayType=\"xsd:string[2]\">";
			s += "<item href=\"#id-606830706\"></item>";
			s += "<item xsi:type=\"xsd:string\">Hola</item>";
			s += "</ArrayText>";
			s += "<string id=\"id-606830706\" xsi:type=\"xsd:string\">Adeu</string>";
			s += "</Sample>";
			DeserializeEncoded (typeof(Sample), s);
		}
		
		
		[Test]
		public void TestDeserializeXmlNodeArray ()
		{
			object ob = Deserialize (typeof(object), "<anyType at=\"1\"><elem1/><elem2/></anyType>");
			Assertion.Assert ("Is node array", ob is XmlNode[]);
			
			XmlNode[] nods = (XmlNode[]) ob; 
			Assertion.AssertEquals ("lengh", 3, nods.Length);
			Assertion.Assert ("#1", nods[0] is XmlAttribute);
			Assertion.AssertEquals ("#2", "at", ((XmlAttribute)nods[0]).LocalName);
			Assertion.AssertEquals ("#3", "1", ((XmlAttribute)nods[0]).Value);
			Assertion.Assert ("#4", nods[1] is XmlElement);
			Assertion.AssertEquals ("#5", "elem1", ((XmlElement)nods[1]).LocalName);
			Assertion.Assert ("#6", nods[2] is XmlElement);
			Assertion.AssertEquals ("#7", "elem2", ((XmlElement)nods[2]).LocalName);
		}
		
		[Test]
		public void TestDeserializeXmlElement ()
		{
			object ob = Deserialize (typeof(XmlElement), "<elem/>");
			Assertion.Assert ("#1", ob is XmlElement);
			Assertion.AssertEquals ("#2", "elem", ((XmlElement)ob).LocalName);
		}
		
		[Test]
		public void TestDeserializeXmlCDataSection ()
		{
			CDataContainer c = (CDataContainer) Deserialize (typeof(CDataContainer), "<CDataContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><cdata><![CDATA[data section contents]]></cdata></CDataContainer>");
			Assertion.AssertNotNull ("#1", c.cdata);
			Assertion.AssertEquals ("#2", "data section contents", c.cdata.Value);
		}
		
		[Test]
		public void TestDeserializeXmlNode ()
		{
			NodeContainer c = (NodeContainer) Deserialize (typeof(NodeContainer), "<NodeContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><node>text</node></NodeContainer>");
			Assertion.Assert ("#1", c.node is XmlText);
			Assertion.AssertEquals ("#2", "text", c.node.Value);
		}
		
		[Test]
		public void TestDeserializeChoices ()
		{
			Choices ch = (Choices) Deserialize (typeof(Choices), "<Choices><ChoiceZero>choice text</ChoiceZero></Choices>");
			Assertion.AssertEquals ("#1", "choice text", ch.MyChoice);
			Assertion.AssertEquals ("#2", ItemChoiceType.ChoiceZero, ch.ItemType);
			
			ch = (Choices) Deserialize (typeof(Choices), "<Choices><ChoiceOne>choice text</ChoiceOne></Choices>");
			Assertion.AssertEquals ("#1", "choice text", ch.MyChoice);
			Assertion.AssertEquals ("#2", ItemChoiceType.StrangeOne, ch.ItemType);
			
			ch = (Choices) Deserialize (typeof(Choices), "<Choices><ChoiceTwo>choice text</ChoiceTwo></Choices>");
			Assertion.AssertEquals ("#1", "choice text", ch.MyChoice);
			Assertion.AssertEquals ("#2", ItemChoiceType.ChoiceTwo, ch.ItemType);
		}
	}
}
