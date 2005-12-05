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
		
		[Test]
		public void TestDeserializeNamesWithSpaces ()
		{
			TestSpace ts = (TestSpace) Deserialize (typeof(TestSpace), "<Type_x0020_with_x0020_space xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Attribute_x0020_with_x0020_space='5'><Element_x0020_with_x0020_space>4</Element_x0020_with_x0020_space></Type_x0020_with_x0020_space>");
			Assertion.AssertEquals ("#1", 4, ts.elem);
			Assertion.AssertEquals ("#2", 5, ts.attr);
		}
		
		[Test]
		public void TestDeserializeDefaults ()
		{
			ListDefaults d2 = (ListDefaults) Deserialize (typeof(ListDefaults), "<root/>");
	        
	        Assertion.AssertNotNull ("list2", d2.list2);
	        Assertion.AssertNull ("list3", d2.list3);
	        Assertion.AssertNull ("list4", d2.list4);
	        Assertion.AssertNotNull ("list5", d2.list5);
	        Assertion.AssertNotNull ("ed", d2.ed);
	        Assertion.AssertNotNull ("str", d2.str);
	        
			d2 = (ListDefaults) Deserialize (typeof(ListDefaults), "<root></root>");
	        
	        Assertion.AssertNotNull ("2 list2", d2.list2);
	        Assertion.AssertNull ("2 list3", d2.list3);
	        Assertion.AssertNull ("2 list4", d2.list4);
	        Assertion.AssertNotNull ("2 list5", d2.list5);
	        Assertion.AssertNotNull ("2 ed", d2.ed);
	        Assertion.AssertNotNull ("2 str", d2.str);
		}
		
		[Test]
		public void TestDeserializeChoiceArray ()
		{
			CompositeValueType v = (CompositeValueType) Deserialize (typeof(CompositeValueType), "<?xml version=\"1.0\" encoding=\"utf-16\"?><CompositeValueType xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><In>1</In><Es>2</Es></CompositeValueType>");
	        Assertion.AssertNotNull ("v.Items", v.Items);
	        Assertion.AssertNotNull ("v.ItemsElementName", v.ItemsElementName);
			Assertion.AssertEquals ("v.Items.Length", 2, v.Items.Length);
			Assertion.AssertEquals ("v.ItemsElementName.Length", 2, v.ItemsElementName.Length);
			Assertion.AssertEquals ("v.Items[0]", 1, v.Items[0]);
			Assertion.AssertEquals ("v.Items[1]", 2, v.Items[1]);
			Assertion.AssertEquals ("v.ItemsElementName[0]", ItemsChoiceType.In, v.ItemsElementName[0]);
			Assertion.AssertEquals ("v.ItemsElementName[1]", ItemsChoiceType.Es, v.ItemsElementName[1]);
		}
		
		[Test]
		public void TestDeserializeCollection ()
		{
			string s0 = "";
			s0+="	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s0+="		<Entity Name='node1'/>";
			s0+="		<Entity Name='node2'/>";
			s0+="	</ArrayOfEntity>";
			
			EntityCollection col = (EntityCollection) Deserialize (typeof(EntityCollection), s0);
	        Assertion.AssertNotNull ("col", col);
			Assertion.AssertEquals ("col.Count", 2, col.Count);
	        Assertion.AssertNull ("col[0]", col[0].Parent);
	        Assertion.AssertNull ("col[1]", col[1].Parent);
		}
		
		[Test]
		public void TestDeserializeEmptyCollection ()
		{
			string s1 = "";
			s1+="	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			
			EntityCollection col = (EntityCollection) Deserialize (typeof(EntityCollection), s1);
	        Assertion.AssertNotNull ("col", col);
			Assertion.AssertEquals ("col.Count", 0, col.Count);
			
			string s1_1 = "";
			s1_1+="	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s1_1+="	</ArrayOfEntity>";
			
			col = (EntityCollection) Deserialize (typeof(EntityCollection), s1_1);
	        Assertion.AssertNotNull ("col", col);
			Assertion.AssertEquals ("col.Count", 0, col.Count);
		}
		
		[Test]
		public void TestDeserializeNilCollectionIsNotNull ()
		{
			string s2 = "";
			s2+="	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true'/>";
			
			EntityCollection col = (EntityCollection) Deserialize (typeof(EntityCollection), s2);
	        Assertion.AssertNotNull ("col", col);
			Assertion.AssertEquals ("col.Count", 0, col.Count);
		}
		
		[Test]
		public void TestDeserializeObjectCollections ()
		{
			string s3 = "";
			s3+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection1>";
			s3+="	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection2>";
			s3+="	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection3>";
			s3+="	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection4>";
			s3+="</Container>";
			
			EntityContainer cont = (EntityContainer) Deserialize (typeof(EntityContainer), s3);
	        Assertion.AssertNotNull ("cont", cont);
	        
	        Assertion.AssertNotNull ("cont.Collection1", cont.Collection1);
			Assertion.AssertEquals ("cont.Collection1", 2, cont.Collection1.Count);
			Assertion.AssertEquals ("cont.Collection1.Container", "assigned", cont.Collection1.Container);
			Assertion.AssertEquals ("cont.Collection1[0].Parent", "assigned", cont.Collection1[0].Parent);
			Assertion.AssertEquals ("cont.Collection1[1].Parent", "assigned", cont.Collection1[1].Parent);
			
	        Assertion.AssertNotNull ("cont.Collection2", cont.Collection2);
			Assertion.AssertEquals ("cont.Collection2", 2, cont.Collection2.Count);
			Assertion.AssertEquals ("cont.Collection2.Container", "assigned", cont.Collection2.Container);
			Assertion.AssertEquals ("cont.Collection2[0].Parent", "assigned", cont.Collection2[0].Parent);
			Assertion.AssertEquals ("cont.Collection2[1].Parent", "assigned", cont.Collection2[1].Parent);
			
	        Assertion.AssertNotNull ("cont.Collection3", cont.Collection3);
			Assertion.AssertEquals ("cont.Collection3", 2, cont.Collection3.Count);
			Assertion.AssertEquals ("cont.Collection3.Container", "root", cont.Collection3.Container);
			Assertion.AssertEquals ("cont.Collection3[0].Parent", "root", cont.Collection3[0].Parent);
			Assertion.AssertEquals ("cont.Collection3[1].Parent", "root", cont.Collection3[1].Parent);
			
	        Assertion.AssertNotNull ("cont.Collection4", cont.Collection4);
			Assertion.AssertEquals ("cont.Collection4", 2, cont.Collection4.Count);
			Assertion.AssertEquals ("cont.Collection4.Container", "root", cont.Collection4.Container);
			Assertion.AssertEquals ("cont.Collection4[0].Parent", "root", cont.Collection4[0].Parent);
			Assertion.AssertEquals ("cont.Collection4[1].Parent", "root", cont.Collection4[1].Parent);
		}
		
		[Test]
		public void TestDeserializeEmptyObjectCollections ()
		{
			string s4 = "";
			s4+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s4+="</Container>";
			
			EntityContainer cont = (EntityContainer) Deserialize (typeof(EntityContainer), s4);
	        Assertion.AssertNotNull ("cont", cont);
	        
	        Assertion.AssertNotNull ("cont.Collection1", cont.Collection1);
			Assertion.AssertEquals ("cont.Collection1", 0, cont.Collection1.Count);
			Assertion.AssertEquals ("cont.Collection1.Container", "assigned", cont.Collection1.Container);
			
	        Assertion.AssertNotNull ("cont.Collection2", cont.Collection2);
			Assertion.AssertEquals ("cont.Collection2", 0, cont.Collection2.Count);
			Assertion.AssertEquals ("cont.Collection2.Container", "assigned", cont.Collection2.Container);
			
	        Assertion.AssertNotNull ("cont.Collection3", cont.Collection3);
			Assertion.AssertEquals ("cont.Collection3", 0, cont.Collection3.Count);
			Assertion.AssertEquals ("cont.Collection3.Container", "root", cont.Collection3.Container);
			
	        Assertion.AssertNotNull ("cont.Collection4", cont.Collection4);
			Assertion.AssertEquals ("cont.Collection4", 0, cont.Collection4.Count);
			Assertion.AssertEquals ("cont.Collection4.Container", "root", cont.Collection4.Container);
		}
		
		[Test]
		public void TestDeserializeObjectNilCollectionsAreNotNull ()
		{
			string s5 = "";
			s5+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s5+="	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="</Container>";
			
			EntityContainer cont = (EntityContainer) Deserialize (typeof(EntityContainer), s5);
	        Assertion.AssertNotNull ("cont", cont);
	        
	        Assertion.AssertNotNull ("cont.Collection1", cont.Collection1);
			Assertion.AssertEquals ("cont.Collection1", 0, cont.Collection1.Count);
			Assertion.AssertEquals ("cont.Collection1.Container", "assigned", cont.Collection1.Container);
			
	        Assertion.AssertNotNull ("cont.Collection2", cont.Collection2);
			Assertion.AssertEquals ("cont.Collection2", 0, cont.Collection2.Count);
			Assertion.AssertEquals ("cont.Collection2.Container", "assigned", cont.Collection2.Container);
			
	        Assertion.AssertNotNull ("cont.Collection3", cont.Collection3);
			Assertion.AssertEquals ("cont.Collection3", 0, cont.Collection3.Count);
			Assertion.AssertEquals ("cont.Collection3.Container", "root", cont.Collection3.Container);
			
	        Assertion.AssertNotNull ("cont.Collection4", cont.Collection4);
			Assertion.AssertEquals ("cont.Collection4", 0, cont.Collection4.Count);
			Assertion.AssertEquals ("cont.Collection4.Container", "root", cont.Collection4.Container);
		}
		
		[Test]
		public void TestDeserializeObjectEmptyCollections ()
		{
			string s6 = "";
			s6+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s6+="	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="</Container>";
			
			EntityContainer cont = (EntityContainer) Deserialize (typeof(EntityContainer), s6);
	        Assertion.AssertNotNull ("cont", cont);
	        
	        Assertion.AssertNotNull ("cont.Collection1", cont.Collection1);
			Assertion.AssertEquals ("cont.Collection1", 0, cont.Collection1.Count);
			Assertion.AssertEquals ("cont.Collection1.Container", "assigned", cont.Collection1.Container);
			
	        Assertion.AssertNotNull ("cont.Collection2", cont.Collection2);
			Assertion.AssertEquals ("cont.Collection2", 0, cont.Collection2.Count);
			Assertion.AssertEquals ("cont.Collection2.Container", "assigned", cont.Collection2.Container);
			
	        Assertion.AssertNotNull ("cont.Collection3", cont.Collection3);
			Assertion.AssertEquals ("cont.Collection3", 0, cont.Collection3.Count);
			Assertion.AssertEquals ("cont.Collection3.Container", "root", cont.Collection3.Container);
			
	        Assertion.AssertNotNull ("cont.Collection4", cont.Collection4);
			Assertion.AssertEquals ("cont.Collection4", 0, cont.Collection4.Count);
			Assertion.AssertEquals ("cont.Collection4.Container", "root", cont.Collection4.Container);
		}
		
		[Test]
		public void TestDeserializeObjectEmptyArrays ()
		{
			string s6 = "";
			s6+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s6+="	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="</Container>";
			
			ArrayEntityContainer cont = (ArrayEntityContainer) Deserialize (typeof(ArrayEntityContainer), s6);
	        Assertion.AssertNotNull ("cont", cont);
	        
	        Assertion.AssertNotNull ("cont.Collection1", cont.Collection1);
			Assertion.AssertEquals ("cont.Collection1.Length", 0, cont.Collection1.Length);
			
	        Assertion.AssertNotNull ("cont.Collection2", cont.Collection2);
			Assertion.AssertEquals ("cont.Collection2.Length", 0, cont.Collection2.Length);
			
	        Assertion.AssertNotNull ("cont.Collection3", cont.Collection3);
			Assertion.AssertEquals ("cont.Collection3.Length", 0, cont.Collection3.Length);
			
	        Assertion.AssertNotNull ("cont.Collection4", cont.Collection4);
			Assertion.AssertEquals ("cont.Collection4.Length", 0, cont.Collection4.Length);
		}
		
		[Test]
		public void TestDeserializeEmptyObjectArrays ()
		{
			string s4 = "";
			s4+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s4+="</Container>";
			
			ArrayEntityContainer cont = (ArrayEntityContainer) Deserialize (typeof(ArrayEntityContainer), s4);
	        Assertion.AssertNotNull ("cont", cont);
	        
	        Assertion.AssertNull ("cont.Collection1", cont.Collection1);
	        Assertion.AssertNull ("cont.Collection2", cont.Collection2);
			
	        Assertion.AssertNotNull ("cont.Collection3", cont.Collection3);
			Assertion.AssertEquals ("cont.Collection3.Length", 0, cont.Collection3.Length);
			
	        Assertion.AssertNotNull ("cont.Collection4", cont.Collection4);
			Assertion.AssertEquals ("cont.Collection4.Length", 0, cont.Collection4.Length);
		}
		
		[Test]
		public void TestDeserializeObjectNilArrays ()
		{
			string s5 = "";
			s5+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s5+="	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="</Container>";
			
			ArrayEntityContainer cont = (ArrayEntityContainer) Deserialize (typeof(ArrayEntityContainer), s5);
	        Assertion.AssertNotNull ("cont", cont);
	        
	        Assertion.AssertNull ("cont.Collection1", cont.Collection1);
	        Assertion.AssertNull ("cont.Collection2", cont.Collection2);
	        Assertion.AssertNull ("cont.Collection3", cont.Collection3);
			
	        Assertion.AssertNotNull ("cont.Collection4", cont.Collection4);
			Assertion.AssertEquals ("cont.Collection4.Length", 0, cont.Collection4.Length);
		}
		
		[Test]
		public void TestDeserializeEmptyArray ()
		{
			string s1 = "";
			s1+="<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			
			Entity[] col = (Entity[]) Deserialize (typeof(Entity[]), s1);
	        Assertion.AssertNotNull ("col", col);
			Assertion.AssertEquals ("col.Length", 0, col.Length);
			
			string s1_1 = "";
			s1_1+="	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s1_1+="	</ArrayOfEntity>";
			
			col = (Entity[]) Deserialize (typeof(Entity[]), s1_1);
	        Assertion.AssertNotNull ("col", col);
			Assertion.AssertEquals ("col.Length", 0, col.Length);
		}
		
		[Test]
		public void TestDeserializeNilArray ()
		{
			string s2 = "";
			s2 += "<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true'/>";
			
			Entity[] col = (Entity[]) Deserialize (typeof(Entity[]), s2);
	        Assertion.AssertNull ("col", col);
		}
		
		[Test]
		public void TestDeserializeObjectWithReadonlyCollection ()
		{
			string s3 = "";
			s3+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="	<Collection1>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection1>";
			s3+="</Container>";
			
			ObjectWithReadonlyCollection cont = (ObjectWithReadonlyCollection) Deserialize (typeof(ObjectWithReadonlyCollection), s3);
	        Assertion.AssertNotNull ("cont", cont);
	        Assertion.AssertNotNull ("cont.Collection1", cont.Collection1);
			Assertion.AssertEquals ("cont.Collection1.Count", 2, cont.Collection1.Count);
			Assertion.AssertEquals ("cont.Collection1.Container", "root", cont.Collection1.Container);
			Assertion.AssertEquals ("cont.Collection1[0].Parent", "root", cont.Collection1[0].Parent);
			Assertion.AssertEquals ("cont.Collection1[1].Parent", "root", cont.Collection1[1].Parent);
		}
		
		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void TestDeserializeObjectWithReadonlyNulCollection ()
		{
			string s3 = "";
			s3+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="	<Collection1>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection1>";
			s3+="</Container>";
			
			Deserialize (typeof(ObjectWithReadonlyNulCollection), s3);
		}
		
		[Test]
		public void TestDeserializeObjectWithReadonlyArray ()
		{
			string s3 = "";
			s3+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="	<Collection1>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection1>";
			s3+="</Container>";
			
			ObjectWithReadonlyArray cont = (ObjectWithReadonlyArray) Deserialize (typeof(ObjectWithReadonlyArray), s3);
	        Assertion.AssertNotNull ("cont", cont);
	        Assertion.AssertNotNull ("cont.Collection1", cont.Collection1);
			Assertion.AssertEquals ("cont.Collection1.Length", 0, cont.Collection1.Length);
		}
	}
}
