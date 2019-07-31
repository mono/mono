//
// System.Xml.DeserializationTests
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//	Hagit Yidov <hagity@mainsoft.com>
//	Andres G. Aragoneses <andres.aragoneses@7digital.com>
//
// (C) 2003 Atsushi Enomoto
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
// (C) 2012 7digital Media Ltd (http://www.7digital.com)
//
//
using System;
using System.Globalization;
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
		public string[] ArrayText;
	}

	[TestFixture]
	public class DeserializationTests
	{
		const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";
		const string XmlSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		const string WsdlTypesNamespace = "http://microsoft.com/wsdl/types/";
		const string ANamespace = "some:urn";
		const string AnotherNamespace = "another:urn";

		object result;

		private object Deserialize (Type t, string xml)
		{
			StringReader sr = new StringReader (xml);
			XmlReader xr = new XmlTextReader (sr);
			return Deserialize (t, xr);
		}

		private object Deserialize (Type t, string xml, string defaultNamespace)
		{
			StringReader sr = new StringReader (xml);
			XmlReader xr = new XmlTextReader (sr);
			return Deserialize (t, xr, defaultNamespace);
		}

		private object Deserialize (Type t, string xml, XmlAttributeOverrides ao)
		{
			StringReader sr = new StringReader (xml);
			XmlReader xr = new XmlTextReader (sr);
			return Deserialize (t, xr, ao);
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

		private object Deserialize (Type t, XmlReader xr, string defaultNamespace)
		{
			XmlSerializer ser = new XmlSerializer (t, defaultNamespace);
			result = ser.Deserialize (xr);
			return result;
		}

		private object Deserialize (Type t, XmlReader xr, XmlAttributeOverrides ao)
		{
			XmlSerializer ser = new XmlSerializer (t, ao);
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
			Assert.AreEqual (typeof (Sample), result.GetType ());
			Sample sample = result as Sample;
			Assert.AreEqual ("Test.", sample.Text);
		}

		[Test]
		public void DeserializeInt ()
		{
			Deserialize (typeof (int), "<int>10</int>");
			Assert.AreEqual (typeof (int), result.GetType ());
			Assert.AreEqual (10, result);
		}

		[Test]
		public void DeserializeSimpleArray ()
		{
			Deserialize (typeof (Sample), "<Sample><ArrayText><string>Test1</string><string>Test2</string></ArrayText></Sample>");
			Assert.AreEqual (typeof (Sample), result.GetType ());
			Sample sample = result as Sample;
			Assert.AreEqual ("Test1", sample.ArrayText[0]);
			Assert.AreEqual ("Test2", sample.ArrayText[1]);
		}

		[Test]
		public void DeserializeEmptyEnum ()
		{
			Field f = Deserialize (typeof (Field), "<field modifiers=\"\" />") as Field;
			Assert.AreEqual (MapModifiers.Public, f.Modifiers);
		}

		[Test]
		public void DeserializePrivateCollection ()
		{
			MemoryStream ms = new MemoryStream ();
			Container c = new Container ();
			c.Items.Add (1);

			XmlSerializer serializer = new XmlSerializer (typeof (Container));
			serializer.Serialize (ms, c);

			ms.Position = 0;
			c = (Container) serializer.Deserialize (ms);
			Assert.AreEqual (1, c.Items[0]);
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DeserializeEmptyPrivateCollection ()
		{
			MemoryStream ms = new MemoryStream ();
			Container2 c = new Container2 (true);
			c.Items.Add (1);

			XmlSerializer serializer = new XmlSerializer (typeof (Container2));
			serializer.Serialize (ms, c);

			ms.Position = 0;
			c = (Container2) serializer.Deserialize (ms);
		}

		[Test]
		[Category ("MobileNotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DeserializeArrayReferences ()
		{
			string s = "<Sample xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";
			s += "<ArrayText xmlns:n3=\"http://schemas.xmlsoap.org/soap/encoding/\" xsi:type=\"n3:Array\" n3:arrayType=\"xsd:string[2]\">";
			s += "<item href=\"#id-606830706\"></item>";
			s += "<item xsi:type=\"xsd:string\">Hola</item>";
			s += "</ArrayText>";
			s += "<string id=\"id-606830706\" xsi:type=\"xsd:string\">Adeu</string>";
			s += "</Sample>";
			DeserializeEncoded (typeof (Sample), s);
		}


		[Test]
		public void TestDeserializeXmlNodeArray ()
		{
			object ob = Deserialize (typeof (object), "<anyType at=\"1\"><elem1/><elem2/></anyType>");
			Assert.IsTrue (ob is XmlNode[], "Is node array");

			XmlNode[] nods = (XmlNode[]) ob;
			Assert.AreEqual (3, nods.Length, "lengh");
			Assert.IsTrue (nods[0] is XmlAttribute, "#1");
			Assert.AreEqual ("at", ((XmlAttribute) nods[0]).LocalName, "#2");
			Assert.AreEqual ("1", ((XmlAttribute) nods[0]).Value, "#3");
			Assert.IsTrue (nods[1] is XmlElement, "#4");
			Assert.AreEqual ("elem1", ((XmlElement) nods[1]).LocalName, "#5");
			Assert.IsTrue (nods[2] is XmlElement, "#6");
			Assert.AreEqual ("elem2", ((XmlElement) nods[2]).LocalName, "#7");
		}

		[Test]
		public void TestDeserializeXmlElement ()
		{
			object ob = Deserialize (typeof (XmlElement), "<elem/>");
			Assert.IsTrue (ob is XmlElement, "#1");
			Assert.AreEqual ("elem", ((XmlElement) ob).LocalName, "#2");
		}

		[Test]
		public void TestDeserializeXmlCDataSection ()
		{
			CDataContainer c = (CDataContainer) Deserialize (typeof (CDataContainer), "<CDataContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><cdata><![CDATA[data section contents]]></cdata></CDataContainer>");
			Assert.IsNotNull (c.cdata, "#1");
			Assert.AreEqual ("data section contents", c.cdata.Value, "#2");
		}

		[Test]
		public void TestDeserializeXmlNode ()
		{
			NodeContainer c = (NodeContainer) Deserialize (typeof (NodeContainer), "<NodeContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><node>text</node></NodeContainer>");
			Assert.IsTrue (c.node is XmlText, "#1");
			Assert.AreEqual ("text", c.node.Value, "#2");
		}

		[Test]
		public void TestDeserializeChoices ()
		{
			Choices ch = (Choices) Deserialize (typeof (Choices), "<Choices><ChoiceZero>choice text</ChoiceZero></Choices>");
			Assert.AreEqual ("choice text", ch.MyChoice, "#A1");
			Assert.AreEqual (ItemChoiceType.ChoiceZero, ch.ItemType, "#A2");

			ch = (Choices) Deserialize (typeof (Choices), "<Choices><ChoiceOne>choice text</ChoiceOne></Choices>");
			Assert.AreEqual ("choice text", ch.MyChoice, "#B1");
			Assert.AreEqual (ItemChoiceType.StrangeOne, ch.ItemType, "#B2");

			ch = (Choices) Deserialize (typeof (Choices), "<Choices><ChoiceTwo>choice text</ChoiceTwo></Choices>");
			Assert.AreEqual ("choice text", ch.MyChoice, "#C1");
			Assert.AreEqual (ItemChoiceType.ChoiceTwo, ch.ItemType, "#C2");
		}

		[Test]
		public void TestDeserializeNamesWithSpaces ()
		{
			TestSpace ts = (TestSpace) Deserialize (typeof (TestSpace), "<Type_x0020_with_x0020_space xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Attribute_x0020_with_x0020_space='5'><Element_x0020_with_x0020_space>4</Element_x0020_with_x0020_space></Type_x0020_with_x0020_space>");
			Assert.AreEqual (4, ts.elem, "#1");
			Assert.AreEqual (5, ts.attr, "#2");
		}

		[Test]
		public void TestDeserializeDefaults ()
		{
			ListDefaults d2 = (ListDefaults) Deserialize (typeof (ListDefaults), "<root/>");

			Assert.IsNotNull (d2.list2, "#A1");
			Assert.IsNull (d2.list4, "#A3");
			Assert.IsNotNull (d2.list5, "#A4");
			Assert.IsNotNull (d2.ed, "#A5");
			Assert.IsNotNull (d2.str, "#A6");

			d2 = (ListDefaults) Deserialize (typeof (ListDefaults), "<root></root>");

			Assert.IsNotNull (d2.list2, "#B1");
			Assert.IsNull (d2.list4, "#B3");
			Assert.IsNotNull (d2.list5, "#B4");
			Assert.IsNotNull (d2.ed, "#B5");
			Assert.IsNotNull (d2.str, "#B6");
		}

		[Test]
		public void TestDeserializeChoiceArray ()
		{
			CompositeValueType v = (CompositeValueType) Deserialize (typeof (CompositeValueType), "<?xml version=\"1.0\" encoding=\"utf-16\"?><CompositeValueType xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><In>1</In><Es>2</Es></CompositeValueType>");
			Assert.IsNotNull (v.Items, "#1");
			Assert.IsNotNull (v.ItemsElementName, "#2");
			Assert.AreEqual (2, v.Items.Length, "#3");
			Assert.AreEqual (2, v.ItemsElementName.Length, "#4");
			Assert.AreEqual (1, v.Items[0], "#5");
			Assert.AreEqual (2, v.Items[1], "#6");
			Assert.AreEqual (ItemsChoiceType.In, v.ItemsElementName[0], "#7");
			Assert.AreEqual (ItemsChoiceType.Es, v.ItemsElementName[1], "#8");
		}

		#region GenericsDeseralizationTests

		[Test]
		public void TestDeserializeGenSimpleClassString ()
		{
			Deserialize (typeof (GenSimpleClass<string>), "<GenSimpleClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />");
			Assert.AreEqual (typeof (GenSimpleClass<string>), result.GetType ());
			Deserialize (typeof (GenSimpleClass<string>), "<GenSimpleClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></GenSimpleClassOfString>");
			GenSimpleClass<string> simple = result as GenSimpleClass<string>;
			Assert.AreEqual ("hello", simple.something);
		}

		[Test]
		public void TestDeserializeGenSimpleClassBool ()
		{
			Deserialize (typeof (GenSimpleClass<bool>), "<GenSimpleClassOfBoolean xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>false</something></GenSimpleClassOfBoolean>");
			Assert.AreEqual (typeof (GenSimpleClass<bool>), result.GetType ());
			Deserialize (typeof (GenSimpleClass<bool>), "<GenSimpleClassOfBoolean xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>true</something></GenSimpleClassOfBoolean>");
			GenSimpleClass<bool> simple = result as GenSimpleClass<bool>;
			Assert.AreEqual (true, simple.something);
		}

		[Test]
		public void TestDeserializeGenSimpleStructInt ()
		{
			Deserialize (typeof (GenSimpleStruct<int>), "<GenSimpleStructOfInt32 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>0</something></GenSimpleStructOfInt32>");
			Assert.AreEqual (typeof (GenSimpleStruct<int>), result.GetType ());
			Deserialize (typeof (GenSimpleStruct<int>), "<GenSimpleStructOfInt32 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>123</something></GenSimpleStructOfInt32>");
			GenSimpleStruct<int> simple = new GenSimpleStruct<int> (0);
			if (result != null)
				simple = (GenSimpleStruct<int>) result;
			Assert.AreEqual (123, simple.something);
		}

		[Test]
		public void TestDeserializeGenListClassString ()
		{
			Deserialize (typeof (GenListClass<string>), "<GenListClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist></somelist></GenListClassOfString>");
			Assert.AreEqual (typeof (GenListClass<string>), result.GetType ());
			Deserialize (typeof (GenListClass<string>), "<GenListClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist><string>Value1</string><string>Value2</string></somelist></GenListClassOfString>");
			GenListClass<string> genlist = result as GenListClass<string>;
			Assert.AreEqual ("Value1", genlist.somelist[0]);
			Assert.AreEqual ("Value2", genlist.somelist[1]);
		}

		[Test]
		public void TestDeserializeGenListClassFloat ()
		{
			Deserialize (typeof (GenListClass<float>), "<GenListClassOfSingle xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist></somelist></GenListClassOfSingle>");
			Assert.AreEqual (typeof (GenListClass<float>), result.GetType ());
			Deserialize (typeof (GenListClass<float>), "<GenListClassOfSingle xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist><float>1</float><float>2.2</float></somelist></GenListClassOfSingle>");
			GenListClass<float> genlist = result as GenListClass<float>;
			Assert.AreEqual (1, genlist.somelist[0]);
			Assert.AreEqual (2.2F, genlist.somelist[1]);
		}

		[Test]
		public void TestDeserializeGenListClassList ()
		{
			Deserialize (typeof (GenListClass<GenListClass<int>>), "<GenListClassOfGenListClassOfInt32 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist></somelist></GenListClassOfGenListClassOfInt32>");
			Assert.AreEqual (typeof (GenListClass<GenListClass<int>>), result.GetType ());
			Deserialize (typeof (GenListClass<GenListClass<int>>), "<GenListClassOfGenListClassOfInt32 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist><GenListClassOfInt32><somelist><int>1</int><int>2</int></somelist></GenListClassOfInt32><GenListClassOfInt32><somelist><int>10</int><int>20</int></somelist></GenListClassOfInt32></somelist></GenListClassOfGenListClassOfInt32>");
			GenListClass<GenListClass<int>> genlist = result as GenListClass<GenListClass<int>>;
			Assert.AreEqual (1, genlist.somelist[0].somelist[0]);
			Assert.AreEqual (2, genlist.somelist[0].somelist[1]);
			Assert.AreEqual (10, genlist.somelist[1].somelist[0]);
			Assert.AreEqual (20, genlist.somelist[1].somelist[1]);
		}

		[Test]
		public void TestDeserializeGenListClassArray ()
		{
			Deserialize (typeof (GenListClass<GenArrayClass<char>>), "<GenListClassOfGenArrayClassOfChar xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist></somelist></GenListClassOfGenArrayClassOfChar>");
			Assert.AreEqual (typeof (GenListClass<GenArrayClass<char>>), result.GetType ());
			Deserialize (typeof (GenListClass<GenArrayClass<char>>), "<GenListClassOfGenArrayClassOfChar xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist><GenArrayClassOfChar><arr><char>97</char><char>98</char><char>0</char></arr></GenArrayClassOfChar><GenArrayClassOfChar><arr><char>100</char><char>101</char><char>102</char></arr></GenArrayClassOfChar></somelist></GenListClassOfGenArrayClassOfChar>");
			GenListClass<GenArrayClass<char>> genlist = result as GenListClass<GenArrayClass<char>>;
			Assert.AreEqual ('a', genlist.somelist[0].arr[0]);
			Assert.AreEqual ('b', genlist.somelist[0].arr[1]);
			Assert.AreEqual ('d', genlist.somelist[1].arr[0]);
			Assert.AreEqual ('e', genlist.somelist[1].arr[1]);
			Assert.AreEqual ('f', genlist.somelist[1].arr[2]);
		}

		[Test]
		public void TestDeserializeGenTwoClassCharDouble ()
		{
			Deserialize (typeof (GenTwoClass<char, double>), "<GenTwoClassOfCharDouble xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something1>0</something1><something2>0</something2></GenTwoClassOfCharDouble>");
			Assert.AreEqual (typeof (GenTwoClass<char, double>), result.GetType ());
			Deserialize (typeof (GenTwoClass<char, double>), "<GenTwoClassOfCharDouble xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something1>97</something1><something2>2.2</something2></GenTwoClassOfCharDouble>");
			GenTwoClass<char, double> gentwo = result as GenTwoClass<char, double>;
			Assert.AreEqual ('a', gentwo.something1);
			Assert.AreEqual (2.2, gentwo.something2);
		}

		[Test]
		public void TestDeserializeGenDerivedClassDecimalShort ()
		{
			Deserialize (typeof (GenDerivedClass<decimal, short>), "<GenDerivedClassOfDecimalInt16 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something2>0</something2><another1>0</another1><another2>0</another2></GenDerivedClassOfDecimalInt16>");
			Assert.AreEqual (typeof (GenDerivedClass<decimal, short>), result.GetType ());
			Deserialize (typeof (GenDerivedClass<decimal, short>), "<GenDerivedClassOfDecimalInt16 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something1>Value1</something1><something2>1</something2><another1>1.1</another1><another2>-22</another2></GenDerivedClassOfDecimalInt16>");
			GenDerivedClass<decimal, short> derived = result as GenDerivedClass<decimal, short>;
			Assert.AreEqual ("Value1", derived.something1);
			Assert.AreEqual (1, derived.something2);
			Assert.AreEqual (1.1M, derived.another1);
			Assert.AreEqual (-22, derived.another2);
		}

		[Test]
		public void TestDeserializeGenDerivedSecondClassByteUlong ()
		{
			Deserialize (typeof (GenDerived2Class<byte, ulong>), "<GenDerived2ClassOfByteUInt64 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something1>0</something1><something2>0</something2><another1>0</another1><another2>0</another2></GenDerived2ClassOfByteUInt64>");
			Assert.AreEqual (typeof (GenDerived2Class<byte, ulong>), result.GetType ());
			Deserialize (typeof (GenDerived2Class<byte, ulong>), "<GenDerived2ClassOfByteUInt64 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something1>1</something1><something2>222</something2><another1>111</another1><another2>222222</another2></GenDerived2ClassOfByteUInt64>");
			GenDerived2Class<byte, ulong> derived2 = result as GenDerived2Class<byte, ulong>;
			Assert.AreEqual (1, derived2.something1);
			Assert.AreEqual (222, derived2.something2);
			Assert.AreEqual (111, derived2.another1);
			Assert.AreEqual (222222, derived2.another2);
		}

		[Test]
		public void TestDeserializeGenNestedClass ()
		{
			Deserialize (typeof (GenNestedClass<string, int>.InnerClass<bool>), "<InnerClassOfStringInt32Boolean xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><inner>0</inner><something>false</something></InnerClassOfStringInt32Boolean>");
			Assert.AreEqual (typeof (GenNestedClass<string, int>.InnerClass<bool>), result.GetType ());
			Deserialize (typeof (GenNestedClass<string, int>.InnerClass<bool>), "<InnerClassOfStringInt32Boolean xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><inner>5</inner><something>true</something></InnerClassOfStringInt32Boolean>");
			GenNestedClass<string, int>.InnerClass<bool> nested = result as GenNestedClass<string, int>.InnerClass<bool>;
			Assert.AreEqual (5, nested.inner);
			Assert.AreEqual (true, nested.something);
		}

		[Test]
		public void TestDeserializeGenListClassListNested ()
		{
			Deserialize (typeof (GenListClass<GenListClass<GenNestedClass<int, int>.InnerClass<string>>>),
				"<GenListClassOfGenListClassOfInnerClassOfInt32Int32String xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist></somelist></GenListClassOfGenListClassOfInnerClassOfInt32Int32String>");
			Assert.AreEqual (typeof (GenListClass<GenListClass<GenNestedClass<int, int>.InnerClass<string>>>), result.GetType ());
			Deserialize (typeof (GenListClass<GenListClass<GenNestedClass<int, int>.InnerClass<string>>>),
				"<GenListClassOfGenListClassOfInnerClassOfInt32Int32String xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><somelist><GenListClassOfInnerClassOfInt32Int32String><somelist><InnerClassOfInt32Int32String><inner>1</inner><something>ONE</something></InnerClassOfInt32Int32String><InnerClassOfInt32Int32String><inner>2</inner><something>TWO</something></InnerClassOfInt32Int32String></somelist></GenListClassOfInnerClassOfInt32Int32String><GenListClassOfInnerClassOfInt32Int32String><somelist><InnerClassOfInt32Int32String><inner>30</inner><something>THIRTY</something></InnerClassOfInt32Int32String></somelist></GenListClassOfInnerClassOfInt32Int32String></somelist></GenListClassOfGenListClassOfInnerClassOfInt32Int32String>");
			GenListClass<GenListClass<GenNestedClass<int, int>.InnerClass<string>>> genlist =
				result as GenListClass<GenListClass<GenNestedClass<int, int>.InnerClass<string>>>;
			Assert.AreEqual (1, genlist.somelist[0].somelist[0].inner);
			Assert.AreEqual ("ONE", genlist.somelist[0].somelist[0].something);
			Assert.AreEqual (2, genlist.somelist[0].somelist[1].inner);
			Assert.AreEqual ("TWO", genlist.somelist[0].somelist[1].something);
			Assert.AreEqual (30, genlist.somelist[1].somelist[0].inner);
			Assert.AreEqual ("THIRTY", genlist.somelist[1].somelist[0].something);
		}

		public enum Myenum { one, two, three, four, five, six };
		[Test]
		public void TestDeserializeGenArrayClassEnum ()
		{
			Deserialize (typeof (GenArrayClass<Myenum>), "<GenArrayClassOfMyenum xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><arr><Myenum>one</Myenum><Myenum>one</Myenum><Myenum>one</Myenum></arr></GenArrayClassOfMyenum>");
			Assert.AreEqual (typeof (GenArrayClass<Myenum>), result.GetType ());
			Deserialize (typeof (GenArrayClass<Myenum>), "<GenArrayClassOfMyenum xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><arr><Myenum>one</Myenum><Myenum>three</Myenum><Myenum>five</Myenum></arr></GenArrayClassOfMyenum>");
			GenArrayClass<Myenum> genarr = result as GenArrayClass<Myenum>;
			Assert.AreEqual (Myenum.one, genarr.arr[0]);
			Assert.AreEqual (Myenum.three, genarr.arr[1]);
			Assert.AreEqual (Myenum.five, genarr.arr[2]);
		}

		[Test]
		public void TestDeserializeGenArrayClassStruct ()
		{
			Deserialize (typeof (GenArrayClass<GenSimpleStruct<uint>>), "<GenArrayClassOfGenSimpleStructOfUInt32 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><arr><GenSimpleStructOfUInt32>0</GenSimpleStructOfUInt32><GenSimpleStructOfUInt32>0</GenSimpleStructOfUInt32><GenSimpleStructOfUInt32>0</GenSimpleStructOfUInt32></arr></GenArrayClassOfGenSimpleStructOfUInt32>");
			Assert.AreEqual (typeof (GenArrayClass<GenSimpleStruct<uint>>), result.GetType ());
			Deserialize (typeof (GenArrayClass<GenSimpleStruct<uint>>), "<GenArrayClassOfGenSimpleStructOfUInt32 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><arr><GenSimpleStructOfUInt32><something>111</something></GenSimpleStructOfUInt32><GenSimpleStructOfUInt32><something>222</something></GenSimpleStructOfUInt32><GenSimpleStructOfUInt32><something>333</something></GenSimpleStructOfUInt32></arr></GenArrayClassOfGenSimpleStructOfUInt32>");
			GenArrayClass<GenSimpleStruct<uint>> genarr = result as GenArrayClass<GenSimpleStruct<uint>>;
			Assert.AreEqual (111, genarr.arr[0].something);
			Assert.AreEqual (222, genarr.arr[1].something);
			Assert.AreEqual (333, genarr.arr[2].something);
		}

		[Test]
		public void TestDeserializeGenArrayClassList ()
		{
			Deserialize (typeof (GenArrayClass<GenListClass<string>>), "<GenArrayClassOfGenListClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><arr><GenListClassOfString>0</GenListClassOfString><GenListClassOfString>0</GenListClassOfString><GenListClassOfString>0</GenListClassOfString></arr></GenArrayClassOfGenListClassOfString>");
			Assert.AreEqual (typeof (GenArrayClass<GenListClass<string>>), result.GetType ());
			Deserialize (typeof (GenArrayClass<GenListClass<string>>), "<GenArrayClassOfGenListClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><arr><GenListClassOfString><somelist><string>list1-val1</string><string>list1-val2</string></somelist></GenListClassOfString><GenListClassOfString><somelist><string>list2-val1</string><string>list2-val2</string><string>list2-val3</string><string>list2-val4</string></somelist></GenListClassOfString><GenListClassOfString><somelist><string>list3val</string></somelist></GenListClassOfString></arr></GenArrayClassOfGenListClassOfString>");
			GenArrayClass<GenListClass<string>> genarr = result as GenArrayClass<GenListClass<string>>;
			Assert.AreEqual ("list1-val1", genarr.arr[0].somelist[0]);
			Assert.AreEqual ("list1-val2", genarr.arr[0].somelist[1]);
			Assert.AreEqual ("list2-val1", genarr.arr[1].somelist[0]);
			Assert.AreEqual ("list2-val2", genarr.arr[1].somelist[1]);
			Assert.AreEqual ("list2-val3", genarr.arr[1].somelist[2]);
			Assert.AreEqual ("list2-val4", genarr.arr[1].somelist[3]);
			Assert.AreEqual ("list3val", genarr.arr[2].somelist[0]);
			// The code below checks for DotNet bug (see corresponding test in XmlSerializerTests).
			Deserialize (typeof (GenArrayClass<GenListClass<string>>), "<GenArrayClassOfGenListClassOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><arr><GenListClassOfString><somelist><string>list1-val1</string><string>list1-val2</string><string>list3val</string></somelist></GenListClassOfString><GenListClassOfString><somelist><string>list2-val1</string><string>list2-val2</string><string>list2-val3</string><string>list2-val4</string></somelist></GenListClassOfString><GenListClassOfString><somelist></somelist></GenListClassOfString></arr></GenArrayClassOfGenListClassOfString>");
			GenArrayClass<GenListClass<string>> genarr2 = result as GenArrayClass<GenListClass<string>>;
			Assert.AreEqual ("list1-val1", genarr2.arr[0].somelist[0]);
			Assert.AreEqual ("list1-val2", genarr2.arr[0].somelist[1]);
			/**/
			Assert.AreEqual ("list3val", genarr2.arr[0].somelist[2]);
			Assert.AreEqual ("list2-val1", genarr2.arr[1].somelist[0]);
			Assert.AreEqual ("list2-val2", genarr2.arr[1].somelist[1]);
			Assert.AreEqual ("list2-val3", genarr2.arr[1].somelist[2]);
			Assert.AreEqual ("list2-val4", genarr2.arr[1].somelist[3]);
			//Assert.AreEqual ("list3val", genarr2.arr[2].somelist[0]);
		}

		[Test]
		public void TestDeserializeGenComplexStruct ()
		{
			Deserialize (typeof (GenComplexStruct<int, string>), "<GenComplexStructOfInt32String xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>0</something><simpleclass><something>0</something></simpleclass><simplestruct><something>0</something></simplestruct><listclass><somelist></somelist></listclass><arrayclass><arr><int>0</int><int>0</int><int>0</int></arr></arrayclass><twoclass><something1>0</something1></twoclass><derivedclass><something2>0</something2><another1>0</another1></derivedclass><derived2><something1>0</something1><another1>0</another1></derived2><nestedouter><outer>0</outer></nestedouter><nestedinner><something>0</something></nestedinner></GenComplexStructOfInt32String>");
			Assert.AreEqual (typeof (GenComplexStruct<int, string>), result.GetType ());
			Deserialize (typeof (GenComplexStruct<int, string>), "<GenComplexStructOfInt32String xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>123</something><simpleclass><something>456</something></simpleclass><simplestruct><something>789</something></simplestruct><listclass><somelist><int>100</int><int>200</int></somelist></listclass><arrayclass><arr><int>11</int><int>22</int><int>33</int></arr></arrayclass><twoclass><something1>10</something1><something2>Ten</something2></twoclass><derivedclass><something1>two</something1><something2>2</something2><another1>1</another1><another2>one</another2></derivedclass><derived2><something1>4</something1><something2>four</something2><another1>3</another1><another2>three</another2></derived2><nestedouter><outer>5</outer></nestedouter><nestedinner><inner>six</inner><something>6</something></nestedinner></GenComplexStructOfInt32String>");
			GenComplexStruct<int, string> complex = new GenComplexStruct<int, string> (0);
			if (result != null)
				complex = (GenComplexStruct<int, string>) result;
			Assert.AreEqual (123, complex.something);
			Assert.AreEqual (456, complex.simpleclass.something);
			Assert.AreEqual (789, complex.simplestruct.something);
			Assert.AreEqual (100, complex.listclass.somelist[0]);
			Assert.AreEqual (200, complex.listclass.somelist[1]);
			Assert.AreEqual (11, complex.arrayclass.arr[0]);
			Assert.AreEqual (22, complex.arrayclass.arr[1]);
			Assert.AreEqual (33, complex.arrayclass.arr[2]);
			Assert.AreEqual (10, complex.twoclass.something1);
			Assert.AreEqual ("Ten", complex.twoclass.something2);
			Assert.AreEqual (1, complex.derivedclass.another1);
			Assert.AreEqual ("one", complex.derivedclass.another2);
			Assert.AreEqual ("two", complex.derivedclass.something1);
			Assert.AreEqual (2, complex.derivedclass.something2);
			Assert.AreEqual (3, complex.derived2.another1);
			Assert.AreEqual ("three", complex.derived2.another2);
			Assert.AreEqual (4, complex.derived2.something1);
			Assert.AreEqual ("four", complex.derived2.something2);
			Assert.AreEqual (5, complex.nestedouter.outer);
			Assert.AreEqual ("six", complex.nestedinner.inner);
			Assert.AreEqual (6, complex.nestedinner.something);
		}

		#endregion //GenericsDeseralizationTests

		[Test]
		public void TestDeserializeCollection ()
		{
			string s0 = "";
			s0 += "	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s0 += "		<Entity Name='node1'/>";
			s0 += "		<Entity Name='node2'/>";
			s0 += "	</ArrayOfEntity>";

			EntityCollection col = (EntityCollection) Deserialize (typeof (EntityCollection), s0);
			Assert.IsNotNull (col, "#1");
			Assert.AreEqual (2, col.Count, "#2");
			Assert.IsNull (col[0].Parent, "#3");
			Assert.IsNull (col[1].Parent, "#4");
		}

		[Test]
		public void TestDeserializeEmptyCollection ()
		{
			string s1 = "";
			s1 += "	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";

			EntityCollection col = (EntityCollection) Deserialize (typeof (EntityCollection), s1);
			Assert.IsNotNull (col, "#A1");
			Assert.AreEqual (0, col.Count, "#A2");

			string s1_1 = "";
			s1_1 += "	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s1_1 += "	</ArrayOfEntity>";

			col = (EntityCollection) Deserialize (typeof (EntityCollection), s1_1);
			Assert.IsNotNull (col, "#B1");
			Assert.AreEqual (0, col.Count, "#B2");
		}

		[Test]
		public void TestDeserializeNilCollectionIsNotNull ()
		{
			string s2 = "";
			s2 += "	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true'/>";

			EntityCollection col = (EntityCollection) Deserialize (typeof (EntityCollection), s2);
			Assert.IsNotNull (col, "#1");
			Assert.AreEqual (0, col.Count, "#2");
		}

		[Test]
		public void TestDeserializeObjectCollections ()
		{
			string s3 = "";
			s3 += "<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3 += "	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3 += "		<Entity Name='node1'/>";
			s3 += "		<Entity Name='node2'/>";
			s3 += "	</Collection1>";
			s3 += "	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3 += "		<Entity Name='node1'/>";
			s3 += "		<Entity Name='node2'/>";
			s3 += "	</Collection2>";
			s3 += "	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3 += "		<Entity Name='node1'/>";
			s3 += "		<Entity Name='node2'/>";
			s3 += "	</Collection3>";
			s3 += "	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3 += "		<Entity Name='node1'/>";
			s3 += "		<Entity Name='node2'/>";
			s3 += "	</Collection4>";
			s3 += "</Container>";

			EntityContainer cont = (EntityContainer) Deserialize (typeof (EntityContainer), s3);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNotNull (cont.Collection1, "#B1");
			Assert.AreEqual (2, cont.Collection1.Count, "#B2");
			Assert.AreEqual ("assigned", cont.Collection1.Container, "#B3");
			Assert.AreEqual ("assigned", cont.Collection1[0].Parent, "#B4");
			Assert.AreEqual ("assigned", cont.Collection1[1].Parent, "#B5");

			Assert.IsNotNull (cont.Collection2, "#C1");
			Assert.AreEqual (2, cont.Collection2.Count, "#C2");
			Assert.AreEqual ("assigned", cont.Collection2.Container, "#C3");
			Assert.AreEqual ("assigned", cont.Collection2[0].Parent, "#C4");
			Assert.AreEqual ("assigned", cont.Collection2[1].Parent, "#C5");

			Assert.IsNotNull (cont.Collection3, "#D1");
			Assert.AreEqual (2, cont.Collection3.Count, "#D2");
			Assert.AreEqual ("root", cont.Collection3.Container, "#D3");
			Assert.AreEqual ("root", cont.Collection3[0].Parent, "#D4");
			Assert.AreEqual ("root", cont.Collection3[1].Parent, "#D5");

			Assert.IsNotNull (cont.Collection4, "#E1");
			Assert.AreEqual (2, cont.Collection4.Count, "#E2");
			Assert.AreEqual ("root", cont.Collection4.Container, "#E3");
			Assert.AreEqual ("root", cont.Collection4[0].Parent, "#E4");
			Assert.AreEqual ("root", cont.Collection4[1].Parent, "#E5");
		}

		[Test]
		public void TestDeserializeEmptyObjectCollections ()
		{
			string s4 = "";
			s4 += "<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s4 += "</Container>";

			EntityContainer cont = (EntityContainer) Deserialize (typeof (EntityContainer), s4);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNotNull (cont.Collection1, "#B1");
			Assert.AreEqual (0, cont.Collection1.Count, "#B2");
			Assert.AreEqual ("assigned", cont.Collection1.Container, "#B3");

			Assert.IsNotNull (cont.Collection2, "#C1");
			Assert.AreEqual (0, cont.Collection2.Count, "#C2");
			Assert.AreEqual ("assigned", cont.Collection2.Container, "#C3");

			Assert.IsNotNull (cont.Collection3, "#D1");
			Assert.AreEqual (0, cont.Collection3.Count, "#D2");
			Assert.AreEqual ("root", cont.Collection3.Container, "#D3");

			Assert.IsNotNull (cont.Collection4, "#E1");
			Assert.AreEqual (0, cont.Collection4.Count, "#E2");
			Assert.AreEqual ("root", cont.Collection4.Container, "#E3");
		}

		[Test]
		public void TestDeserializeObjectNilCollectionsAreNotNull ()
		{
			string s5 = "";
			s5 += "<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s5 += "	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5 += "	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5 += "	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5 += "	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5 += "</Container>";

			EntityContainer cont = (EntityContainer) Deserialize (typeof (EntityContainer), s5);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNotNull (cont.Collection1, "#B1");
			Assert.AreEqual (0, cont.Collection1.Count, "#B2");
			Assert.AreEqual ("assigned", cont.Collection1.Container, "#B3");

			Assert.IsNotNull (cont.Collection2, "#C1");
			Assert.AreEqual (0, cont.Collection2.Count, "#C2");
			Assert.AreEqual ("assigned", cont.Collection2.Container, "#C3");

			Assert.IsNotNull (cont.Collection3, "#D1");
			Assert.AreEqual (0, cont.Collection3.Count, "#D2");
			Assert.AreEqual ("root", cont.Collection3.Container, "#D3");

			Assert.IsNotNull (cont.Collection4, "#E1");
			Assert.AreEqual (0, cont.Collection4.Count, "#E2");
			Assert.AreEqual ("root", cont.Collection4.Container, "#E3");
		}

		[Test]
		public void TestDeserializeObjectEmptyCollections ()
		{
			string s6 = "";
			s6 += "<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s6 += "	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6 += "	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6 += "	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6 += "	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6 += "</Container>";

			EntityContainer cont = (EntityContainer) Deserialize (typeof (EntityContainer), s6);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNotNull (cont.Collection1, "#B1");
			Assert.AreEqual (0, cont.Collection1.Count, "#B2");
			Assert.AreEqual ("assigned", cont.Collection1.Container, "#B3");

			Assert.IsNotNull (cont.Collection2, "#C1");
			Assert.AreEqual (0, cont.Collection2.Count, "#C2");
			Assert.AreEqual ("assigned", cont.Collection2.Container, "#C3");

			Assert.IsNotNull (cont.Collection3, "#D1");
			Assert.AreEqual (0, cont.Collection3.Count, "#D2");
			Assert.AreEqual ("root", cont.Collection3.Container, "#D3");

			Assert.IsNotNull (cont.Collection4, "#E1");
			Assert.AreEqual (0, cont.Collection4.Count, "#E2");
			Assert.AreEqual ("root", cont.Collection4.Container, "#E3");
		}

		[Test]
		public void TestDeserializeObjectEmptyArrays ()
		{
			string s6 = "";
			s6 += "<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s6 += "	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6 += "	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6 += "	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6 += "	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6 += "</Container>";

			ArrayEntityContainer cont = (ArrayEntityContainer) Deserialize (typeof (ArrayEntityContainer), s6);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNotNull (cont.Collection1, "#B1");
			Assert.AreEqual (0, cont.Collection1.Length, "#B2");

			Assert.IsNotNull (cont.Collection2, "#C1");
			Assert.AreEqual (0, cont.Collection2.Length, "#C2");

			Assert.IsNotNull (cont.Collection3, "#D1");
			Assert.AreEqual (0, cont.Collection3.Length, "#D2");

			Assert.IsNotNull (cont.Collection4, "#E1");
			Assert.AreEqual (0, cont.Collection4.Length, "#E2");
		}

		[Test]
		public void TestDeserializeEmptyObjectArrays ()
		{
			string s4 = "";
			s4 += "<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s4 += "</Container>";

			ArrayEntityContainer cont = (ArrayEntityContainer) Deserialize (typeof (ArrayEntityContainer), s4);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNull (cont.Collection1, "#B1");
			Assert.IsNull (cont.Collection2, "#B2");

			Assert.IsNotNull (cont.Collection3, "#C1");
			Assert.AreEqual (0, cont.Collection3.Length, "#C2");

			Assert.IsNotNull (cont.Collection4, "#D1");
			Assert.AreEqual (0, cont.Collection4.Length, "#D2");
		}

		[Test]
		public void TestDeserializeObjectNilArrays ()
		{
			string s5 = "";
			s5 += "<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s5 += "	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5 += "	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5 += "	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5 += "	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5 += "</Container>";

			ArrayEntityContainer cont = (ArrayEntityContainer) Deserialize (typeof (ArrayEntityContainer), s5);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNull (cont.Collection1, "#B1");
			Assert.IsNull (cont.Collection2, "#B2");
			Assert.IsNull (cont.Collection3, "#B3");

			Assert.IsNotNull (cont.Collection4, "#C1");
			Assert.AreEqual (0, cont.Collection4.Length, "#C2");
		}

		[Test]
		public void TestDeserializeEmptyArray ()
		{
			string s1 = "";
			s1 += "<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";

			Entity[] col = (Entity[]) Deserialize (typeof (Entity[]), s1);
			Assert.IsNotNull (col, "#A1");
			Assert.AreEqual (0, col.Length, "#A2");

			string s1_1 = "";
			s1_1 += "	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s1_1 += "	</ArrayOfEntity>";

			col = (Entity[]) Deserialize (typeof (Entity[]), s1_1);
			Assert.IsNotNull (col, "#B1");
			Assert.AreEqual (0, col.Length, "#B2");
		}

		[Test]
		public void TestDeserializeNilArray ()
		{
			string s2 = "";
			s2 += "<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true'/>";

			Entity[] col = (Entity[]) Deserialize (typeof (Entity[]), s2);
			Assert.IsNull (col, "#1");
		}

		[Test]
		public void TestDeserializeObjectWithReadonlyCollection ()
		{
			string s3 = "";
			s3 += "<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3 += "	<Collection1>";
			s3 += "		<Entity Name='node1'/>";
			s3 += "		<Entity Name='node2'/>";
			s3 += "	</Collection1>";
			s3 += "</Container>";

			ObjectWithReadonlyCollection cont = (ObjectWithReadonlyCollection) Deserialize (typeof (ObjectWithReadonlyCollection), s3);
			Assert.IsNotNull (cont, "#1");
			Assert.IsNotNull (cont.Collection1, "#2");
			Assert.AreEqual (2, cont.Collection1.Count, "#3");
			Assert.AreEqual ("root", cont.Collection1.Container, "#4");
			Assert.AreEqual ("root", cont.Collection1[0].Parent, "#5");
			Assert.AreEqual ("root", cont.Collection1[1].Parent, "#6");
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void TestDeserializeObjectWithReadonlyNulCollection ()
		{
			string s3 = "";
			s3 += "<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3 += "	<Collection1>";
			s3 += "		<Entity Name='node1'/>";
			s3 += "		<Entity Name='node2'/>";
			s3 += "	</Collection1>";
			s3 += "</Container>";

			var obj = (ObjectWithReadonlyNulCollection) Deserialize (typeof (ObjectWithReadonlyNulCollection), s3);
			Assert.IsNull (obj.Collection1);
		}

		[Test]
		public void TestDeserializeObjectWithReadonlyArray ()
		{
			string s3 = "";
			s3 += "<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3 += "	<Collection1>";
			s3 += "		<Entity Name='node1'/>";
			s3 += "		<Entity Name='node2'/>";
			s3 += "	</Collection1>";
			s3 += "</Container>";

			ObjectWithReadonlyArray cont = (ObjectWithReadonlyArray) Deserialize (typeof (ObjectWithReadonlyArray), s3);
			Assert.IsNotNull (cont, "#1");
			Assert.IsNotNull (cont.Collection1, "#2");
			Assert.AreEqual (0, cont.Collection1.Length, "#3");
		}

		[Test]
		public void TestDeserialize_EnumDefaultValue ()
		{
			EnumDefaultValue e;

			e = (EnumDefaultValue) Deserialize (typeof (EnumDefaultValue), "<EnumDefaultValue />");
			Assert.AreEqual (0, (int) e, "#1");

			e = (EnumDefaultValue) Deserialize (typeof (EnumDefaultValue), "<EnumDefaultValue> e3</EnumDefaultValue>");
			Assert.AreEqual (EnumDefaultValue.e3, e, "#2");

			e = (EnumDefaultValue) Deserialize (typeof (EnumDefaultValue), "<EnumDefaultValue>e1 e2</EnumDefaultValue>");
			Assert.AreEqual (EnumDefaultValue.e3, e, "#3");

			e = (EnumDefaultValue) Deserialize (typeof (EnumDefaultValue), "<EnumDefaultValue>  e1   e2 </EnumDefaultValue>");
			Assert.AreEqual (EnumDefaultValue.e1 | EnumDefaultValue.e2, e, "#4");
		}

		[Test]
		public void TestDeserialize_EnumDefaultValueNF ()
		{
			EnumDefaultValueNF e;

			e = (EnumDefaultValueNF) Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF>e3</EnumDefaultValueNF>");
			Assert.AreEqual (EnumDefaultValueNF.e3, e, "#A1");

			try {
				Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF />");
				Assert.Fail ("#B1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.InnerException.Message, "#B5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("''") != -1, "#B6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).Name) != -1, "#B7");
			}

			try {
				Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF>e1 e3</EnumDefaultValueNF>");
				Assert.Fail ("#C1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.InnerException.Message, "#C5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'e1 e3'") != -1, "#C6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).Name) != -1, "#C7");
			}

			try {
				Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF> e3</EnumDefaultValueNF>");
				Assert.Fail ("#D1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.InnerException, "#D3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D4");
				Assert.IsNotNull (ex.InnerException.Message, "#D5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("' e3'") != -1, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).Name) != -1, "#D7");
			}

			try {
				Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF> </EnumDefaultValueNF>");
				Assert.Fail ("#E1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.InnerException, "#E3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#E4");
				Assert.IsNotNull (ex.InnerException.Message, "#E5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("' '") != -1, "#E6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).Name) != -1, "#E7");
			}

			try {
				Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF>1</EnumDefaultValueNF>");
				Assert.Fail ("#F1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
				Assert.IsNotNull (ex.InnerException, "#F3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#F4");
				Assert.IsNotNull (ex.InnerException.Message, "#F5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'1'") != -1, "#F6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).Name) != -1, "#F7");
			}
		}

		[Test]
		public void TestDeserialize_Field ()
		{
			Field f = null;

			f = (Field) Deserialize (typeof (Field),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='' flag2='' flag4='' modifiers='public' modifiers2='public' modifiers4='public' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace));
			Assert.AreEqual ((FlagEnum) 0, f.Flags1, "#A1");
			Assert.AreEqual ((FlagEnum) 0, f.Flags2, "#A2");
			Assert.AreEqual ((FlagEnum) 0, f.Flags3, "#A3");
			Assert.AreEqual ((FlagEnum) 0, f.Flags4, "#A4");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers, "#A5");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers2, "#A6");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers3, "#A7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#A8");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers5, "#A9");
			Assert.IsNull (f.Names, "#A10");
			Assert.IsNull (f.Street, "#A11");

			f = (Field) Deserialize (typeof (Field),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag3='two' flag4='' modifiers='protected' modifiers2='public' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace));
			Assert.AreEqual ((FlagEnum) 0, f.Flags1, "#B1");
			Assert.AreEqual ((FlagEnum) 0, f.Flags2, "#B2");
			Assert.AreEqual (FlagEnum.e2, f.Flags3, "#B3");
			Assert.AreEqual ((FlagEnum) 0, f.Flags4, "#B4");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers, "#B5");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers2, "#B6");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers3, "#B7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#B8");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers5, "#B9");
			Assert.IsNull (f.Names, "#B10");
			Assert.IsNull (f.Street, "#B11");

			f = (Field) Deserialize (typeof (Field),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='two' flag2='two' flag4='' modifiers='public' modifiers2='protected' modifiers3='protected' modifiers4='public' modifiers5='protected' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace));
			Assert.AreEqual (FlagEnum.e2, f.Flags1, "#C1");
			Assert.AreEqual (FlagEnum.e2, f.Flags2, "#C2");
			Assert.AreEqual ((FlagEnum) 0, f.Flags3, "#C3");
			Assert.AreEqual ((FlagEnum) 0, f.Flags4, "#C4");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers, "#C5");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers2, "#C6");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers3, "#C7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#C8");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers5, "#C9");
			Assert.IsNull (f.Names, "#C10");
			Assert.IsNull (f.Street, "#C11");

			try {
				f = (Field) Deserialize (typeof (Field),
					string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
					"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag2='444' />",
					XmlSchemaNamespace, XmlSchemaInstanceNamespace));
				Assert.Fail ("#D1");
			}
			catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsNotNull (ex.InnerException, "#D4");

				// '444' is not a valid value for MonoTests.System.Xml.TestClasses.FlagEnum
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D5");
				Assert.IsNotNull (ex.InnerException.Message, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'444'") != -1, "#D7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#D8");
				Assert.IsNull (ex.InnerException.InnerException, "#D9");
			}

			try {
				f = (Field) Deserialize (typeof (Field),
					string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
					"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag2='Garbage' />",
					XmlSchemaNamespace, XmlSchemaInstanceNamespace));
				Assert.Fail ("#E1");
			}
			catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsNotNull (ex.InnerException, "#E4");

				// 'Garbage' is not a valid value for MonoTests.System.Xml.TestClasses.FlagEnum
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#E5");
				Assert.IsNotNull (ex.InnerException.Message, "#E6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'Garbage'") != -1, "#E7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#E8");
				Assert.IsNull (ex.InnerException.InnerException, "#E9");
			}

			try {
				f = (Field) Deserialize (typeof (Field),
					string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
					"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag2='{2}' />",
					XmlSchemaNamespace, XmlSchemaInstanceNamespace, ((int) FlagEnum.e2).ToString (CultureInfo.InvariantCulture)));
				Assert.Fail ("#F1");
			}
			catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
				Assert.IsNotNull (ex.Message, "#F3");
				Assert.IsNotNull (ex.InnerException, "#F4");

				// '2' is not a valid value for MonoTests.System.Xml.TestClasses.FlagEnum
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#F5");
				Assert.IsNotNull (ex.InnerException.Message, "#F6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'2'") != -1, "#F7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#F8");
				Assert.IsNull (ex.InnerException.InnerException, "#F9");
			}
		}

		[Test]
		[Category ("NotWorking")] // MS.NET results in compilation error (probably it generates bogus source.)
		public void TestDeserialize_Field_Encoded ()
		{
			Field_Encoded f = null;

			f = (Field_Encoded) DeserializeEncoded (typeof (Field_Encoded),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag1='' flag2='' flag4='' modifiers='PuBlIc' modifiers2='PuBlIc' modifiers4='PuBlIc' xmlns:q1='{2}' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace));
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags1, "#A1");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags2, "#A2");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags3, "#A3");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags4, "#A4");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers, "#A5");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers2, "#A6");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers3, "#A7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#A8");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers5, "#A9");
			Assert.IsNull (f.Names, "#A10");
			Assert.IsNull (f.Street, "#A11");

			f = (Field_Encoded) DeserializeEncoded (typeof (Field_Encoded),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag3='two' flag4='' modifiers='Protected' modifiers2='PuBlIc' xmlns:q1='{2}' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace));
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags1, "#B1");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags2, "#B2");
			Assert.AreEqual (FlagEnum_Encoded.e2, f.Flags3, "#B3");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags4, "#B4");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers, "#B5");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers2, "#B6");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers3, "#B7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#B8");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers5, "#B9");
			Assert.IsNull (f.Names, "#B10");
			Assert.IsNull (f.Street, "#B11");

			f = (Field_Encoded) DeserializeEncoded (typeof (Field_Encoded),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag1='two' flag2='two' flag4='' modifiers='PuBlIc' modifiers2='Protected' modifiers3='Protected' modifiers4='PuBlIc' modifiers5='Protected' xmlns:q1='{2}' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace));
			Assert.AreEqual (FlagEnum_Encoded.e2, f.Flags1, "#C1");
			Assert.AreEqual (FlagEnum_Encoded.e2, f.Flags2, "#C2");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags3, "#C3");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags4, "#C4");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers, "#C5");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers2, "#C6");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers3, "#C7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#C8");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers5, "#C9");
			Assert.IsNull (f.Names, "#C10");
			Assert.IsNull (f.Street, "#C11");

			try {
				f = (Field_Encoded) DeserializeEncoded (typeof (Field_Encoded),
					string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
					"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag2='444' flag3='555' flag4='' modifiers='666' modifiers2='777' modifiers4='888' modifiers5='999' xmlns:q1='{2}' />",
					XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace));
				Assert.Fail ("#D1");
			}
			catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsNotNull (ex.InnerException, "#D4");

				// '444' is not a valid value for MonoTests.System.Xml.TestClasses.FlagEnum_Encoded
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D5");
				Assert.IsNotNull (ex.InnerException.Message, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'444'") != -1, "#D7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum_Encoded).FullName) != -1, "#D8");
				Assert.IsNull (ex.InnerException.InnerException, "#D9");
			}
		}

		[Test]
		public void TestDeserialize_FlagEnum ()
		{
			FlagEnum e;

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum />");
			Assert.AreEqual (0, (int) e, "#A1");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum>one</FlagEnum>");
			Assert.AreEqual (FlagEnum.e1, e, "#A2");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum>one\u200atwo</FlagEnum>");
			Assert.AreEqual (FlagEnum.e1 | FlagEnum.e2, e, "#A3");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum>one two four</FlagEnum>");
			Assert.AreEqual (FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4, e, "#A4");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum> two  four </FlagEnum>");
			Assert.AreEqual (FlagEnum.e2 | FlagEnum.e4, e, "#A5");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum>two four two</FlagEnum>");
			Assert.AreEqual (FlagEnum.e2 | FlagEnum.e4, e, "#A6");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum>two four two\tone\u2002four\rtwo one</FlagEnum>");
			Assert.AreEqual (FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4, e, "#A7");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum></FlagEnum>");
			Assert.AreEqual (0, (int) e, "#A8");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum> </FlagEnum>");
			Assert.AreEqual (0, (int) e, "#A9");

			try {
				Deserialize (typeof (FlagEnum), "<FlagEnum>1</FlagEnum>");
				Assert.Fail ("#B1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.InnerException.Message, "#B5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'1'") != -1, "#B6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#B7");
			}

			try {
				Deserialize (typeof (FlagEnum), "<FlagEnum>one,two</FlagEnum>");
				Assert.Fail ("#C1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.InnerException.Message, "#C5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'one,two'") != -1, "#C6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#C7");
			}

			try {
				Deserialize (typeof (FlagEnum), "<FlagEnum>one something</FlagEnum>");
				Assert.Fail ("#D1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.InnerException, "#D3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D4");
				Assert.IsNotNull (ex.InnerException.Message, "#D5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'something'") != -1, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#D7");
			}
		}

		[Test]
		public void TestDeserialize_Group ()
		{
			string xml = string.Format (CultureInfo.InvariantCulture,
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
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance");

			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			Group group = (Group) DeserializeEncoded (typeof (Group), xtr);

			Assert.AreEqual (new DateTime (2002, 5, 2), group.Today, "#A1");
			Assert.AreEqual (".NET", group.GroupName, "#A2");
			Assert.AreEqual (new byte[] { 0x64, 0x32 }, group.GroupNumber, "#A3");
			Assert.AreEqual (GroupType.A, group.Grouptype, "#A4");
			Assert.AreEqual ("10000", group.PostitiveInt, "#A5");
			Assert.IsFalse (group.IgnoreThis, "#A6");
			Assert.IsNotNull (group.MyVehicle, "#A7");
			Assert.AreEqual (typeof (Car), group.MyVehicle.GetType (), "#A8");
			Assert.AreEqual ("1234566", group.MyVehicle.licenseNumber, "#A9");
			Assert.AreEqual (new DateTime (1, 1, 1), group.MyVehicle.makeDate, "#A10");
			Assert.IsNull (group.MyVehicle.weight, "#A11");

			xml = string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' CreationDate='2002-05-02' GroupNumber='ZDI=' id='id1'>" +
				"<PosInt xsi:type='xsd:nonNegativeInteger'>10000</PosInt>" +
				"<Grouptype xsi:type='GroupType'>Large</Grouptype>" +
				"<MyVehicle href='#id2' />" +
				"</Group>" +
				"<Car xmlns:d2p1='{1}' id='id2' d2p1:type='Car'>" +
				"<weight xmlns:q2='{0}' d2p1:type='q2:string'>450</weight>" +
				"</Car>" +
				"</Wrapper>",
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance");

			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			group = (Group) DeserializeEncoded (typeof (Group), xtr);

			Assert.AreEqual (new DateTime (2002, 5, 2), group.Today, "#B1");
			Assert.IsNull (group.GroupName, "#B2");
			Assert.AreEqual (new byte[] { 0x64, 0x32 }, group.GroupNumber, "#B3");
			Assert.AreEqual (GroupType.B, group.Grouptype, "#B4");
			Assert.AreEqual ("10000", group.PostitiveInt, "#B5");
			Assert.IsFalse (group.IgnoreThis, "#B6");
			Assert.IsNotNull (group.MyVehicle, "#B7");
			Assert.AreEqual (typeof (Car), group.MyVehicle.GetType (), "#B8");
			Assert.IsNull (group.MyVehicle.licenseNumber, "#B9");
			Assert.AreEqual (DateTime.MinValue, group.MyVehicle.makeDate, "#B10");
			Assert.AreEqual ("450", group.MyVehicle.weight, "#B11");

			xml = string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' />" +
				"</Wrapper>",
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance");

			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			group = (Group) DeserializeEncoded (typeof (Group), xtr);

			Assert.AreEqual (DateTime.MinValue, group.Today, "#C1");
			Assert.IsNull (group.GroupName, "#C2");
			Assert.AreEqual (null, group.GroupNumber, "#C3");
			Assert.AreEqual (GroupType.A, group.Grouptype, "#C4");
			Assert.IsNull (group.PostitiveInt, "#C5");
			Assert.IsFalse (group.IgnoreThis, "#C6");
			Assert.IsNull (group.MyVehicle, "#C7");

			xml = string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1'>" +
				"<Grouptype xsi:type='GroupType'>666</Grouptype>" +
				"</Group>" +
				"</Wrapper>",
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance");

			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			try {
				group = (Group) DeserializeEncoded (typeof (Group), xtr);
				Assert.Fail ("#D1");
			}
			catch (InvalidOperationException ex) {
				// There is an error in XML document (1, 174)
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsNotNull (ex.InnerException, "#D4");

				// '666' is not a valid value for GroupType
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D5");
				Assert.IsNotNull (ex.InnerException.Message, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'666'") != -1, "#D7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (GroupType).Name) != -1, "#D8");
				Assert.IsNull (ex.InnerException.InnerException, "#D9");
			}

			xml = string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1'>" +
				"<Grouptype xsi:type='GroupType'>Garbage</Grouptype>" +
				"</Group>" +
				"</Wrapper>",
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance");

			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			try {
				group = (Group) DeserializeEncoded (typeof (Group), xtr);
				Assert.Fail ("#E1");
			}
			catch (InvalidOperationException ex) {
				// There is an error in XML document (1, 178)
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsNotNull (ex.InnerException, "#E4");

				// 'Garbage' is not a valid value for GroupType
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#E5");
				Assert.IsNotNull (ex.InnerException.Message, "#E6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'Garbage'") != -1, "#E7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (GroupType).Name) != -1, "#E8");
				Assert.IsNull (ex.InnerException.InnerException, "#E9");
			}

			xml = string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1'>" +
				"<Grouptype xsi:type='GroupType'>{2}</Grouptype>" +
				"</Group>" +
				"</Wrapper>",
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance",
				((int) GroupType.B).ToString (CultureInfo.InvariantCulture));

			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			try {
				group = (Group) DeserializeEncoded (typeof (Group), xtr);
				Assert.Fail ("#F1");
			}
			catch (InvalidOperationException ex) {
				// There is an error in XML document (1, 172)
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
				Assert.IsNotNull (ex.Message, "#F3");
				Assert.IsNotNull (ex.InnerException, "#F4");

				// '1' is not a valid value for GroupType
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#F5");
				Assert.IsNotNull (ex.InnerException.Message, "#F6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'1'") != -1, "#F7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (GroupType).Name) != -1, "#F8");
				Assert.IsNull (ex.InnerException.InnerException, "#F9");
			}
		}

		[Test]
		public void TestDeserialize_ZeroFlagEnum ()
		{
			ZeroFlagEnum e;

			e = (ZeroFlagEnum) Deserialize (typeof (ZeroFlagEnum), "<ZeroFlagEnum />");
			Assert.AreEqual (ZeroFlagEnum.e0, e, "#A1");
			e = (ZeroFlagEnum) Deserialize (typeof (ZeroFlagEnum), "<ZeroFlagEnum></ZeroFlagEnum>");
			Assert.AreEqual (ZeroFlagEnum.e0, e, "#A2");

			try {
				Deserialize (typeof (ZeroFlagEnum), "<ZeroFlagEnum>four</ZeroFlagEnum>");
				Assert.Fail ("#B1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.InnerException.Message, "#B5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'four'") != -1, "#B6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (ZeroFlagEnum).FullName) != -1, "#B7");
			}

			try {
				Deserialize (typeof (ZeroFlagEnum), "<ZeroFlagEnum> o&lt;n&gt;e  four </ZeroFlagEnum>");
				Assert.Fail ("#C1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.InnerException.Message, "#C5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'four'") != -1, "#C6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (ZeroFlagEnum).FullName) != -1, "#C7");
			}

			try {
				Deserialize (typeof (ZeroFlagEnum), "<ZeroFlagEnum>four o&lt;n&gt;e</ZeroFlagEnum>");
				Assert.Fail ("#D1");
			}
			catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.InnerException, "#D3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D4");
				Assert.IsNotNull (ex.InnerException.Message, "#D5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'four'") != -1, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (ZeroFlagEnum).FullName) != -1, "#D7");
			}
		}

		[Test]
		public void TestDeserialize_PrimitiveTypesContainer ()
		{
			Deserialize (typeof (PrimitiveTypesContainer), string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<PrimitiveTypesContainer xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns='{2}'>" +
				"<Number xsi:type='xsd:int'>2004</Number>" +
				"<Name xsi:type='xsd:string'>some name</Name>" +
				"<Index xsi:type='xsd:unsignedByte'>56</Index>" +
				"<Password xsi:type='xsd:base64Binary'>8w8=</Password>" +
				"<PathSeparatorCharacter xmlns:q1='{3}' xsi:type='q1:char'>47</PathSeparatorCharacter>" +
				"</PrimitiveTypesContainer>", XmlSchemaNamespace,
				XmlSchemaInstanceNamespace, ANamespace, WsdlTypesNamespace));
			Assert.AreEqual (typeof (PrimitiveTypesContainer), result.GetType (), "#A1");

			PrimitiveTypesContainer deserialized = (PrimitiveTypesContainer) result;
			Assert.AreEqual (2004, deserialized.Number, "#A2");
			Assert.AreEqual ("some name", deserialized.Name, "#A3");
			Assert.AreEqual ((byte) 56, deserialized.Index, "#A4");
			Assert.AreEqual (new byte[] { 243, 15 }, deserialized.Password, "#A5");
			Assert.AreEqual ('/', deserialized.PathSeparatorCharacter, "#A6");

			DeserializeEncoded (typeof (PrimitiveTypesContainer), string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:PrimitiveTypesContainer xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' xmlns:q1='{2}'>" +
				"<Number xsi:type='xsd:int'>2004</Number>" +
				"<Name xsi:type='xsd:string'>some name</Name>" +
				"<Index xsi:type='xsd:unsignedByte'>56</Index>" +
				"<Password xsi:type='xsd:base64Binary'>8w8=</Password>" +
				"<PathSeparatorCharacter xmlns:q1='{3}' xsi:type='q1:char'>47</PathSeparatorCharacter>" +
				"</q1:PrimitiveTypesContainer>", XmlSchemaNamespace,
				XmlSchemaInstanceNamespace, AnotherNamespace, WsdlTypesNamespace));
			Assert.AreEqual (typeof (PrimitiveTypesContainer), result.GetType (), "#B1");

			deserialized = (PrimitiveTypesContainer) result;
			Assert.AreEqual (2004, deserialized.Number, "#B2");
			Assert.AreEqual ("some name", deserialized.Name, "#B3");
			Assert.AreEqual ((byte) 56, deserialized.Index, "#B4");
			Assert.AreEqual (new byte[] { 243, 15 }, deserialized.Password, "#B5");
			Assert.AreEqual ('/', deserialized.PathSeparatorCharacter, "#B6");
		}

		[Test] // bug #378696
		public void DoNotFillDefaultValue ()
		{
			XmlSerializer xs = new XmlSerializer (typeof (DefaultDateTimeContainer));
			DefaultDateTimeContainer o = (DefaultDateTimeContainer) xs.Deserialize (new StringReader ("<DefaultDateTimeContainer xmlns='urn:foo' />"));
			// do not fill DefaultValue / do not bork at generating code.
			Assert.AreEqual (DateTime.MinValue, o.FancyDateTime, "#1");
			Assert.AreEqual (0, o.Numeric, "#2");
		}
		
		[Test] // bug bxc 4367
		public void SpecifiedXmlIgnoreTest ()
		{
			XmlReflectionMember [] out_members = new XmlReflectionMember [2];
			XmlReflectionMember m;
			
			m = new XmlReflectionMember ();
			m.IsReturnValue = false;
			m.MemberName = "HasPermissionsForUserResult";
			m.MemberType = typeof (bool);
			m.SoapAttributes = new SoapAttributes ();
			m.XmlAttributes = new XmlAttributes ();
			out_members [0] = m;
			
			m = new XmlReflectionMember ();
			m.IsReturnValue = false;
			m.MemberName = "HasPermissionsForUserResultSpecified";
			m.MemberType = typeof (bool);
			m.SoapAttributes = new SoapAttributes ();
			m.XmlAttributes = new XmlAttributes ();
			m.XmlAttributes.XmlIgnore = true;
			out_members [1] = m;
			
			XmlReflectionImporter xmlImporter = new XmlReflectionImporter ();
			XmlMembersMapping OutputMembersMapping = xmlImporter.ImportMembersMapping ("HasPermissionsForUserResponse", "http://tempuri.org", out_members, true);
			XmlSerializer xmlSerializer = XmlSerializer.FromMappings (new XmlMapping [] { OutputMembersMapping }) [0];
			
			Assert.AreEqual (2, OutputMembersMapping.Count, "#count");
			
			string msg = @"
			<HasPermissionsForUserResponse xmlns=""http://tempuri.org/"">
				<HasPermissionsForUserResult>true</HasPermissionsForUserResult>
			</HasPermissionsForUserResponse>
			";
			
			object res = xmlSerializer.Deserialize (new StringReader (msg));
			Assert.AreEqual (typeof (object[]), res.GetType (), "type");
			Assert.AreEqual (2, ((object[]) res).Length, "length");
		}
		
		[Test]
		public void InvalidNullableTypeTest ()
		{
			XmlReflectionMember [] out_members = new XmlReflectionMember [1];
			XmlReflectionMember m;
			
			m = new XmlReflectionMember ();
			m.IsReturnValue = false;
			m.MemberName = "HasPermissionsForUserResultSpecified";
			m.MemberType = typeof (bool);
			m.SoapAttributes = new SoapAttributes ();
			m.XmlAttributes = new XmlAttributes ();
			m.XmlAttributes.XmlIgnore = true;
			m.XmlAttributes.XmlElements.Add (new XmlElementAttribute () { IsNullable = true });
			out_members [0] = m;
			
			XmlReflectionImporter xmlImporter = new XmlReflectionImporter ();
			
			try {
				xmlImporter.ImportMembersMapping ("HasPermissionsForUserResponse", "http://tempuri.org", out_members, true);
				Assert.Fail ("Expected InvalidOperationException");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void NotExactDateParse ()
		{
			XmlSerializer xs = new XmlSerializer (typeof (NotExactDateParseClass));
			NotExactDateParseClass o = (NotExactDateParseClass) xs.Deserialize (new StringReader ("<NotExactDateParseClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><SomeDate xsi:type=\"xsd:date\">2012-02-05-09:00</SomeDate></NotExactDateParseClass>"));
			Assert.AreEqual (new DateTime (2012,2,5,9,0,0,DateTimeKind.Utc), o.SomeDate.ToUniversalTime ());
		}

		[Test]
		public void TimeWithUtc ()
		{
			XmlSerializer xs = new XmlSerializer (typeof (UtcTimeClass));
			var o = (UtcTimeClass) xs.Deserialize (new StringReader ("<UtcTimeClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><DateTimeValue>12:34:56.0Z</DateTimeValue></UtcTimeClass>"));
			Assert.AreEqual (new DateTime (1,1,1,12,34,56,DateTimeKind.Utc), o.DateTimeValue);
		}

		public class Foo
		{
			public DateTime? Baz { get; set; }
		}

		[Test]
		public void CanDeserializeXsiNil()
		{
			var reader = new StringReader(
@"<Foo xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<Baz xsi:nil=""true"" />
</Foo>");

			using (var xmlReader = new XmlTextReader(reader))
			{
				var serializer = new XmlSerializer(typeof(Foo));
				var foo = (Foo)serializer.Deserialize(xmlReader);
				Assert.IsNull(foo.Baz);
			}
		}

		public class Bar
		{
			[XmlElement("baz")]
			public DateTime? Baz { get; set; }
		}

		[Test]
		public void CanDeserializeXsiNilToAPropertyWithXmlElementAttrib()
		{
			var reader = new StringReader(
@"<Bar xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<baz xsi:nil=""true"" />
</Bar>");

			using (var xmlReader = new XmlTextReader(reader))
			{
				var serializer = new XmlSerializer(typeof(Bar));
				var bar = (Bar)serializer.Deserialize(xmlReader);
				Assert.IsNull(bar.Baz);
			}
		}

		public class FooBar
		{
			[XmlElement("baz", IsNullable = true)]
			public DateTime? Baz { get; set; }
		}

		[Test]
		public void CanDeserializeXsiNilToAPropertyWithXmlElementAttribAndIsNullableTrue()
		{
			var reader = new StringReader(
@"<FooBar xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<baz xsi:nil=""true"" />
</FooBar>");

			using (var xmlReader = new XmlTextReader(reader))
			{
				var serializer = new XmlSerializer(typeof(FooBar));
				var foobar = (FooBar)serializer.Deserialize(xmlReader);
				Assert.IsNull(foobar.Baz);
			}
		}

		[Test] // bug #8468
		public void TestUseSubclassDefaultNamespace ()
		{
			XmlSerializer xs = new XmlSerializer (typeof (Bug8468Subclass));
			string msg = "<Test xmlns=\"http://test-namespace\"><Base>BaseValue</Base><Mid>MidValue</Mid></Test>";
			var res1 = (Bug8468Subclass)xs.Deserialize (new StringReader (msg));
			Assert.IsNotNull (res1);
			Assert.AreEqual ("BaseValue", res1.Base);
			Assert.AreEqual ("MidValue", res1.Mid);

			xs = new XmlSerializer (typeof (Bug8468SubclassNoNamespace), "http://test-namespace");
			var res2 = (Bug8468SubclassNoNamespace)xs.Deserialize (new StringReader (msg));
			Assert.IsNotNull (res2);
			Assert.AreEqual ("BaseValue", res2.Base);
			Assert.AreEqual ("MidValue", res2.Mid);

			xs = new XmlSerializer (typeof (Bug8468SubclassV2));
			var res3 = (Bug8468SubclassV2)xs.Deserialize (new StringReader (msg));
			Assert.IsNotNull (res3);
			Assert.IsNull (res3.Base);
			Assert.AreEqual ("MidValue", res3.Mid);

			xs = new XmlSerializer (typeof (Bug8468SubclassNoNamespaceV2), "http://test-namespace");
			var res4 = (Bug8468SubclassNoNamespaceV2)xs.Deserialize (new StringReader (msg));
			Assert.IsNotNull (res4);
			Assert.IsNull (res4.Base);
			Assert.AreEqual ("MidValue", res4.Mid);

			msg = "<Test xmlns=\"http://test-namespace\"><Base xmlns=\"\">BaseValue</Base><Mid>MidValue</Mid></Test>";

			xs = new XmlSerializer (typeof (Bug8468SubclassV2));
			var res5 = (Bug8468SubclassV2)xs.Deserialize (new StringReader (msg));
			Assert.IsNotNull (res5);
			Assert.AreEqual ("BaseValue", res5.Base);
			Assert.AreEqual ("MidValue", res5.Mid);

			xs = new XmlSerializer (typeof (Bug8468SubclassNoNamespaceV2), "http://test-namespace");
			var res6 = (Bug8468SubclassNoNamespaceV2)xs.Deserialize (new StringReader (msg));
			Assert.IsNotNull (res6);
			Assert.AreEqual ("BaseValue", res6.Base);
			Assert.AreEqual ("MidValue", res6.Mid);	
		}
		
		[Test] // bug #9193
		public void TestOrderedMapWithFlatList ()
		{
			var d = (Bug9193Class) Deserialize (typeof(Bug9193Class), "<Test><Data>One</Data><Data>Two</Data><Data>Three</Data><Extra>a</Extra><Extra>b</Extra></Test>");
			Assert.IsNotNull (d);
			Assert.AreEqual (3, d.Data.Length);
			Assert.AreEqual ("One", d.Data[0]);
			Assert.AreEqual ("Two", d.Data[1]);
			Assert.AreEqual ("Three", d.Data[2]);

			Assert.AreEqual (2, d.Extra.Length);
			Assert.AreEqual ("a", d.Extra[0]);
			Assert.AreEqual ("b", d.Extra[1]);
		}

		[Test] // PR #11194
		public void TestDerrivedClassProperty ()
		{
			var data = "<layout width=\".25\" height =\".25\" x=\"0\" y=\"0\" id=\"portrait\" class=\"render\"><background color=\"white\"><div x=\".03\" y=\".05\" width=\".94\" height =\".9\" layout=\"proportional_rows\" paddingX=\".01\" paddingY=\".02\"><background color=\"black\"></background><background color=\"gray\" padding=\".1\"><label color=\"white\" font=\"emulogic.ttf\" fontSize=\"15\">Test UI</label></background><br brSize=\"1\" /><background class=\"back,Lab\" color=\"green\"><label class=\"Lab\" color=\"white\" font=\"emulogic.ttf\" fontSize=\"15\">GREEN</label></background><background color=\"red\"><label class=\"Lab\" color=\"white\" font=\"emulogic.ttf\" fontSize=\"15\">RED</label></background><background color=\"blue\"><label class=\"Lab\" color=\"white\" font=\"emulogic.ttf\" fontSize=\"15\">TLUE</label></background><background color=\"blue\"><label class=\"Lab\" color=\"white\" font=\"emulogic.ttf\" fontSize=\"15\">BLUE</label></background></div></background></layout>";

			XmlAttributeOverrides overrides = PR11194.GenerateLayoutOverrides<PR11194ChildLo>();

			var result = Deserialize(typeof(PR11194ChildLo), data, overrides) as PR11194ChildLo;

			PR11194Parent2 child;
			Assert.IsNotNull(result, "#11194_1");
			Assert.AreEqual(1, result.children.Count, "#11194_2");
			child = result.children[0] as PR11194Parent2;
			Assert.IsNotNull(child, "#11194_3");
			Assert.AreEqual(1, child.children.Count, "#11194_4");
			child = child.children[0] as PR11194Parent2;
			Assert.IsNotNull(child, "#11194_5");
			Assert.AreEqual(7, child.children.Count, "#11194_6");
			child = child.children[1] as PR11194Parent2;
			Assert.IsNotNull(child, "#11194_7");
			Assert.AreEqual(1, child.children.Count, "#11194_8");
			Assert.AreEqual("PR11194ChildL", child.children[0].GetType().Name, "#11194_9");
		}
	}
}
