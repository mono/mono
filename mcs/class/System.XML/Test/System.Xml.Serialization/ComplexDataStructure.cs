//
// ComplexDataStructure.cs
//
// Author:
//	Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2004 Novell, Inc.
//
//
using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections;
using System.ComponentModel; 
using NUnit.Framework;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class ComplexDataStructure
	{
		[Test]
		[NUnit.Framework.Category("NotDotNet")] // FDBK50639 
#if TARGET_JVM
		[Ignore ("JVM returns fields in different order")]
#endif
		public void WriteLiteral ()
		{
			Test data = BuildTestObject ();

			XmlSerializer ss = new XmlSerializer (GetLiteralTypeMapping ());
			XmlSerializerNamespaces nams = new XmlSerializerNamespaces ();
			StringWriter sw = new StringWriter();
			ss.Serialize (sw,data,nams);
			string serialized = sw.ToString ();
			serialized = XmlSerializerTests.Infoset (serialized);

			StreamReader sr = new StreamReader ("Test/XmlFiles/literal-data.xml");
			string expected = sr.ReadToEnd ();
			sr.Close ();
			
			expected = XmlSerializerTests.Infoset (expected);
			Assert.AreEqual (expected, serialized);
		}
		
		[Test]
		[NUnit.Framework.Category ("NotDotNet")] // MS.NET results in compilation error (probably it generates bogus source.)
		public void ReadLiteral ()
		{
			XmlSerializer ss = new XmlSerializer (GetLiteralTypeMapping ());
			XmlSerializerNamespaces nams = new XmlSerializerNamespaces ();
			
			StreamReader sr = new StreamReader ("Test/XmlFiles/literal-data.xml");
			Test data = (Test) ss.Deserialize (sr);
			sr.Close ();
			
			CheckObjectContent (BuildTestObject(), data);
		}
		
		XmlTypeMapping GetLiteralTypeMapping ()
		{
			XmlRootAttribute root = new XmlRootAttribute("rootroot");
			Type[] types = new Type[] {typeof(UknTestPart), typeof(AnotherTestPart), typeof(DblStringContainer) };
			XmlReflectionImporter ri = new XmlReflectionImporter ();
			foreach (Type t in types) ri.IncludeType (t);
			return ri.ImportTypeMapping (typeof(Test), root);
		}

		XmlTypeMapping GetEncodedTypeMapping ()
		{
			SoapReflectionImporter sri = new SoapReflectionImporter ();
			sri.IncludeType (typeof(UknTestPart));
			sri.IncludeType (typeof(AnotherTestPart));
			sri.IncludeType (typeof(DblStringContainer));
			return sri.ImportTypeMapping (typeof(Test));
		}
		
		Test BuildTestObject ()
		{
			XmlDocument doc = new XmlDocument();

			Test t = new UknTestPart();
			t.a = 1;
			t.b = "hola";
			t.bbis = t.b;
			t.c = 44;
			t.parts = new TestPart[3];
			t.parts[0] = new TestPart();
			t.parts[0].name = "un";
			t.parts[0].bval = true;
			t.parts[1] = new TestPart();
			t.parts[1].name = "dos";
			t.parts[1].bval = false;
			t.parts[2] = t.parts[0];
			t.part = t.parts[1];
			t.strings = new string[] { "un", "dos", null, "tres" };
			t.ushorts = new ushort[] { 1,2,3 };
			t.ta = new TB();
			t.ta.extraTextNodes = new XmlNode[] { doc.CreateTextNode ("AA"), doc.CreateTextNode ("BB") };

			t.tam2 = new TA[][][]
					{
						new TA[][] { new TA[] {new TA(), new TA()}, new TA[] {new TA(), new TA()}},
						new TA[][] { new TA[] {new TB(), new TA()}, new TA[] {new TB(), new TA()}},
						new TA[][] { new TA[] {new TA(), new TB()}, new TA[] {new TA(), new TA()}} 
					};

			t.tam3 = t.tam2;
			t.flatParts = t.parts;

			t.flatParts2 = new TA[] {new TA(), new TB(), null, new TB()};

			t.anot = new AnotherTestPart ();
			((AnotherTestPart)t.anot).lo = 1234567890;

			t.ob = t.parts[1];
			t.ob2 = t.parts[1];

			XmlElement e1 = doc.CreateElement ("explicitElement");
			XmlElement e2 = doc.CreateElement ("subElement");
			e2.SetAttribute ("unAtrib","val");
			doc.AppendChild (e1);
			e1.AppendChild (e2);

			t.oneElem = e1;
			t.oneElem2 = e1;
			t.someElems = new XmlNode[3];
			t.someElems[0] = e1;
			t.someElems[1] = null;
			t.someElems[2] = e2;

			t.extraElems = new XmlElement[1];
			t.extraElems[0] = doc.CreateElement ("extra1");
			t.extraElems[0].SetAttribute ("val","1");

			t.extraElems23 = new XmlElement[2];
			t.extraElems23[0] = doc.CreateElement ("extra2");
			t.extraElems23[0].SetAttribute ("val","2");
			t.extraElems23[1] = doc.CreateElement ("extra3");
			t.extraElems23[1].SetAttribute ("val","3");

			t.extraElemsRest = doc.CreateElement ("extra4");
			t.extraElemsRest.SetAttribute ("val","4");

			t.uktester = new UnknownAttributeTester();
			t.uktester.aa = "hihi";

			t.uktester.extraAtts = new XmlAttribute[3];
			t.uktester.extraAtts[0] = doc.CreateAttribute ("extraAtt1");
			t.uktester.extraAtts[0].Value = "val1";
			t.uktester.extraAtts[1] = doc.CreateAttribute ("extraAtt2");
			t.uktester.extraAtts[1].Value = "val2";
			t.uktester.extraAtts[2] = doc.CreateAttribute ("extraAtt3");
			t.uktester.extraAtts[2].Value = "val3";

			t.ob3 = 12345;
			t.ob4 = (float)54321.12;

			t.op1 = option.AA;
			t.opArray = new option[] { option.CC, option.BB, option.AA };
			t.ukOpt = option.DD;
			t.opAtt = option.BB;

			t.byteArray = new byte[] { 11,33,55,222 };
			t.byteByteArray = new byte[][] { t.byteArray, t.byteArray };

			t.ttList = new ArrayList();
			t.ttList.Add ("two");
			t.ttList.Add ("strings");
			//			t.extraText = "Additional text";

			t.RoList = new ArrayList ();
			t.RoList.Add (t.parts[0]);
			t.RoList.Add (t.parts[1]);

/*			t.struc = new OneStruct();
			t.struc.aa = 776655;
			t.struc.cc = "this is a struct";
*/
			t.multiList = new ArrayList[2];
			t.multiList[0] = new ArrayList ();
			t.multiList[0].Add (22);
			t.multiList[0].Add (33);
			t.multiList[1] = new ArrayList ();
			t.multiList[1].Add (888);
			t.multiList[1].Add (999);

			// XmlSerializer does not deserialize default values explicitly.
			//t.defElem = "theDefValue";
			//t.defAttr = "theDefValue";

			t.special = new CustomHashtable ();
			t.special.Add ("one","1");
			t.special.Add ("two","2");
			t.special.Add ("three","3");

			t.attqname = new XmlQualifiedName ("thename","thenamespace");

			DblStringContainer dbc = new DblStringContainer ();
			dbc.doublestring = new string [][] { null, new string[] {"hello"} };
			AnotherTestPart at = new AnotherTestPart ();
			at.lo = 567;
			dbc.at = at;

			DblStringContainerAnm dbca = new DblStringContainerAnm ();
			dbca.at = dbc;
			t.dbscontainer = dbca;
			
			return t;
		}
		
		void CheckObjectContent (Test exp, Test t)
		{
			Assert.AreEqual (exp.a, t.a, "t.a");
			Assert.AreEqual (exp.b, t.b, "t.b");
			Assert.AreEqual (exp.bbis, t.bbis, "t.bbis");
			Assert.AreEqual (exp.c, t.c, "t.c");
			
			Assert.IsNotNull (t.parts, "t.parts");
			CheckParts ("t.parts", exp.parts, t.parts);
			
			TestPart.AssertEquals ("t.part", exp.part, t.part);
			
			AssertionHelper.AssertEqualsArray ("t.strings", exp.strings, t.strings);
			AssertionHelper.AssertEqualsArray ("t.ushorts", exp.ushorts, t.ushorts);
			
			TA.AssertEquals ("t.ta", exp.ta, t.ta);

			Assert.IsNotNull (t.tam2, "t.tam2");
			CheckTaArray ("t.tam2", exp.tam2, t.tam2);
			
			Assert.IsNotNull (t.tam3, "t.tam3");
			CheckTaArray ("t.tam3", exp.tam3, t.tam3);
			
			Assert.IsNotNull (t.flatParts, "t.flatParts");
			CheckParts ("t.flatParts", exp.flatParts, t.flatParts);

			// Null element is ignored
			Assert.IsNotNull (t.flatParts2, "t.flatParts2");
			Assert.AreEqual (3, t.flatParts2.Length, "t.flatParts2.Length");
			TA.AssertEquals ("t.flatParts2 0", exp.flatParts2[0], t.flatParts2[0]);
			TA.AssertEquals ("t.flatParts2 1", exp.flatParts2[1], t.flatParts2[1]);
			TA.AssertEquals ("t.flatParts2 2", exp.flatParts2[3], t.flatParts2[2]);
			
			Assert.IsNotNull (t.anot, "t.anot");
			Assert.AreEqual (((AnotherTestPart)exp.anot).lo, ((AnotherTestPart)t.anot).lo, "t.anot.lo");

			TestPart.AssertEquals ("t.ob", exp.ob as TestPart, t.ob as TestPart);
			TestPart.AssertEquals ("t.ob2", exp.ob2 as TestPart, t.ob2 as TestPart);
			
			AssertionHelper.AssertEqualsXml ("t.oneElem", exp.oneElem, t.oneElem);
			AssertionHelper.AssertEqualsXml ("t.oneElem2", exp.oneElem2, t.oneElem2);
			
			// One of the elements was null and it is ignored
			Assert.IsNotNull (t.someElems, "t.someElems");
			Assert.AreEqual (2, t.someElems.Length, "t.someElems.Length");
			AssertionHelper.AssertEqualsXml ("t.someElems[0]", exp.someElems[0], t.someElems[0]);
			AssertionHelper.AssertEqualsXml ("t.someElems[1]", exp.someElems[2], t.someElems[1]);

			Assert.IsNotNull (t.extraElems, "t.extraElems");
			Assert.AreEqual (exp.extraElems.Length, t.extraElems.Length, "t.extraElems.Length");
			for (int n=0; n<exp.extraElems.Length; n++)
				AssertionHelper.AssertEqualsXml ("t.extraElems[" + n + "]", exp.extraElems[n], t.extraElems[n]);
			
			Assert.IsNotNull (t.extraElems23, "t.extraElems23");
			Assert.AreEqual (exp.extraElems23.Length, t.extraElems23.Length, "t.extraElems23.Length");
			for (int n=0; n<t.extraElems23.Length; n++)
				AssertionHelper.AssertEqualsXml ("t.extraElems23[" + n + "]", exp.extraElems23[n], t.extraElems23[n]);
			
			AssertionHelper.AssertEqualsXml ("t.extraElemsRest", exp.extraElemsRest, t.extraElemsRest);
			
			UnknownAttributeTester.AssertEquals ("t.uktester", exp.uktester, t.uktester);

			Assert.AreEqual (exp.ob3, t.ob3, "t.ob3");
			Assert.AreEqual (exp.ob4, t.ob4, "t.ob4");
			Assert.AreEqual (exp.op1, t.op1, "t.op1");
			
			AssertionHelper.AssertEqualsArray ("t.opArray", exp.opArray, t.opArray);

			Assert.AreEqual (exp.ukOpt, t.ukOpt, "t.ukOpt");
			Assert.AreEqual (exp.opAtt, t.opAtt, "t.opAtt");

			AssertionHelper.AssertEqualsArray ("t.byteArray", exp.byteArray, t.byteArray);
			AssertionHelper.AssertEqualsArray ("t.byteByteArray", exp.byteByteArray, t.byteByteArray);
			
			Assert.IsNotNull (t.ttList, "t.ttList");
			AssertionHelper.AssertEqualsArray ("t.ttList", exp.ttList.ToArray(), t.ttList.ToArray());
			
			Assert.IsNotNull (t.RoList, "t.RoList");
			Assert.AreEqual (exp.RoList.Count, t.RoList.Count, "t.RoList.Count");
			for (int n=0; n<exp.RoList.Count; n++)
				TestPart.AssertEquals ("t.RoList " + n, (TestPart)exp.RoList[n], (TestPart)t.RoList[n]);
			
			Assert.AreEqual (exp.struc.aa, t.struc.aa, "t.struc.aa");
			Assert.AreSame (exp.struc.cc, t.struc.cc, "t.struc.cc");

			Assert.IsNotNull (t.multiList, "t.multiList");
			Assert.AreEqual (exp.multiList.Length, t.multiList.Length, "t.multiList.Count");
			for (int n=0; n<exp.multiList.Length; n++)
				AssertionHelper.AssertEqualsArray ("t.multiList " + n, exp.multiList[n].ToArray(), t.multiList[n].ToArray());
			
			Assert.AreEqual (exp.defElem, t.defElem, "t.defElem");
			Assert.AreEqual (exp.defAttr, t.defAttr, "t.defAttr");

			CustomHashtable.AssertEquals ("t.special", exp.special, t.special);

			Assert.AreEqual (exp.attqname, t.attqname, "t.attqname");

			Assert.IsNotNull (t.dbscontainer, "t.dbscontainer");
			DblStringContainer tdbca = t.dbscontainer.at as DblStringContainer;
			DblStringContainer expdbca = exp.dbscontainer.at as DblStringContainer;
			Assert.IsNotNull (tdbca, "t.dbscontainer.at");
			
			Assert.IsNotNull (tdbca, "t.dbscontainer.dbca");
			AssertionHelper.AssertEqualsArray ("t.dbscontainer.at.doublestring", expdbca.doublestring, tdbca.doublestring);
			
			AnotherTestPart tat = tdbca.at as AnotherTestPart;
			AnotherTestPart expat = expdbca.at as AnotherTestPart;
			Assert.IsNotNull (tat, "t.dbscontainer.dbca.at");
			Assert.AreEqual (expat.lo, tat.lo, "t.dbscontainer.dbca.at.lo");
		}
		
		void CheckParts (string id, TestPart[] exp, TestPart[] parts)
		{
			AssertionHelper.AssertType (id, exp, parts);
			Assert.AreEqual (exp.Length, parts.Length, id + " Len");
			for (int n=0; n<exp.Length; n++)
				TestPart.AssertEquals (id + "[" + n + "]", exp[n], parts[n]);
		}
		
		void CheckTaArray (string id, TA[][][] exp, TA[][][] arr)
		{
			AssertionHelper.AssertType (id, exp, arr);
			Assert.AreEqual (exp.Length, arr.Length, id + " Len");
			for (int n=0; n<exp.Length; n++)
			{
				TA[][] tar = arr[n];
				TA[][] expar = exp[n];
				
				AssertionHelper.AssertType (id, expar, tar);
				Assert.AreEqual (expar.Length, tar.Length);
				
				for (int m=0; m<expar.Length; m++)
				{
					TA[] tar2 = tar[m];
					TA[] expar2 = expar[m];
					
					AssertionHelper.AssertType (id, expar2, tar2);
					Assert.AreEqual (expar2.Length, tar2.Length, id);
					
					for (int i=0; i<expar2.Length; i++)
						TA.AssertEquals (id + "[" + n + "][" + m + "][" + i + "]", expar2[i], tar2[i]);
				}
			}
		}
	}
	
	public class AssertionHelper
	{
		public static bool AssertType (string id, object exp, object ob)
		{
			if (exp == null) {
				Assert.IsNull (ob, id);
				return false;
			}
			else {
				Assert.IsNotNull (ob, id);
				Assert.AreEqual (exp.GetType(), ob.GetType(), id + " type");
				return true;
			}
		}
		
		public static void AssertEqualsXml (string id, XmlNode exp, XmlNode ob)
		{
			if (!AssertType (id, exp, ob)) return;
			Assert.AreEqual (XmlSerializerTests.Infoset (exp), XmlSerializerTests.Infoset (ob), id);
		}
		
		public static void AssertEqualsArray (string id, Array exp, Array ob)
		{
			if (!AssertType (id, exp, ob)) return;
			Assert.AreEqual (exp.GetLength(0), ob.GetLength(0), id + " Length");
			for (int n=0; n<exp.GetLength(0); n++) {
				object it = exp.GetValue(n);
				if (it is Array) 
					AssertEqualsArray (id + "[" + n + "]", it as Array, ob.GetValue(n) as Array);
				else
					Assert.AreEqual (it, ob.GetValue(n), id + "[" + n + "]");
			}
		}
	}
	
	[XmlType(TypeName="")]
	[XmlRoot(ElementName="aaaaaa",Namespace="")]
	[XmlInclude(typeof(UknTestPart))]
	[SoapInclude(typeof(UknTestPart))]
	public class Test
	{
		//		public option op;elem.SchemaTypeName
		public object anot;

		[SoapElement(ElementName="suba")]
		[XmlElement(ElementName="suba",Namespace="kk")]
		public int a;

		public string b;
		public string bbis;

		[SoapAttribute]
		[XmlAttribute (Namespace="attribns")]
		public byte c;

		[XmlElement(Namespace="oo")]
		public TestPart part;

		public TA ta;

		public TestPart[] parts;

		[SoapElement(ElementName="multita")]
		[XmlArray(ElementName="multita")]
			//		[XmlArrayItem(ElementName="itema",NestingLevel=1)]
		[XmlArrayItem(ElementName="itema",Type=typeof(TA),NestingLevel=1)]
		[XmlArrayItem(ElementName="itemb",Type=typeof(TB),NestingLevel=1)]
		public TA[][] tam = new TA[][] { new TA[] {new TA(), new TB()}, new TA[] {new TA(), new TA()}};

//		[SoapElement(ElementName="multita2")]
		[SoapIgnore]
		[XmlArray(ElementName="multita2")]
		[XmlArrayItem(ElementName="data1",NestingLevel=0)]

		[XmlArrayItem(ElementName="data2",NestingLevel=1,Namespace="da2")]
		[XmlArrayItem(ElementName="data3a",Type=typeof(TA),NestingLevel=2,Namespace="da3")]
		[XmlArrayItem(ElementName="data3b",Type=typeof(TB),NestingLevel=2,Namespace="da3")]
		public TA[][][] tam2;

		[SoapIgnore]
		public TA[][][] tam3;

		[SoapElement(IsNullable=true)]
		[XmlElement(IsNullable=true)]
		public string mayBeNull;

		public string[] strings;

		[XmlArray(Namespace="arrayNamespace")]
		public ushort[] ushorts;

		[XmlElement]
		public TestPart[] flatParts;

		[SoapElement (ElementName="flatTAs")]
		[XmlElement (ElementName="flatTAs")]
		public TA[] flatParts2;

		public object ob;

		[XmlElement (Namespace="uimp")]
		public object ob2;

		public object ob3;
		public object ob4;

		[SoapIgnore]
		public XmlElement oneElem;

		[SoapIgnore]
		[XmlElement (ElementName="unElement", Namespace="elemns")]
		public XmlElement oneElem2;

		[SoapIgnore]
		[XmlElement (ElementName="unsElements", Namespace="elemns")]
		public XmlNode[] someElems;

		[SoapIgnore]
		[XmlAnyElement ("extra1")]
		public XmlElement[] extraElems;

		[SoapIgnore]
		[XmlAnyElement ("extra2")]
		[XmlAnyElement ("extra3")]
		[XmlAnyElement ("extra3","nnn")]
		public XmlElement[] extraElems23;

		[SoapIgnore]
		[XmlAnyElement]
		public XmlElement extraElemsRest;

		public UnknownAttributeTester uktester;

		public option op1;
		public option[] opArray;
		public object ukOpt;

		[XmlAttribute]
		[SoapIgnore]
		public option opAtt;


		public byte[] byteArray;
		[SoapIgnore]
		public byte[][] byteByteArray;

		[XmlElement(Type=typeof(string))]
		[XmlElement(ElementName="kk",Type=typeof(int))]
		public object[] tt = new object[] { "aa",22 };

		public ArrayList ttList;

		ArrayList roList;
		public ArrayList RoList
		{
			get { return roList; }
			set { roList = value; }
		}

		[SoapIgnore]
//		[XmlIgnore]
		public ArrayList[] multiList;

		[SoapIgnore]
		[XmlIgnore]
		public OneStruct struc;

		[DefaultValue("theDefValue")]
		public string defElem;

		[XmlAttribute]
		[DefaultValue("theDefValue")]
		public string defAttr;

		[XmlText (Type=typeof(string))]
		[XmlElement (Type=typeof(int))]
		public object[] xmltext = new object[] {"aa",33,"bb",776};

		[SoapIgnore]
		public CustomHashtable special;

		[XmlAttribute]
		public XmlQualifiedName attqname;

		[XmlAttribute]
		public DateTime[] arrayAttribute;

		[XmlArray (Namespace="mm")]
		public string[][] dummyStringArray = new string[][] {null,null};
		
		[XmlElement (Namespace="mm")]
		public DblStringContainerAnm dbscontainer;
	}

	public class DblStringContainerAnm
	{
		public object at;
	}

	[XmlType(Namespace="mm")]
	public class DblStringContainer
	{
		[XmlArrayItem (NestingLevel=1, IsNullable=true)]
		public string [][] doublestring;
		public object at;
	}

	[SoapType(TypeName="TC")]
	[XmlType(TypeName="TC")]
	[XmlInclude(typeof(TB))]
	[SoapInclude(typeof(TB))]
	public class TA
	{
		public int xx = 1;

		[XmlText]
		[SoapIgnore]
		public XmlNode[] extraTextNodes;
		
		public static void AssertEquals (string id, TA expected, TA ob)
		{
			if (!AssertionHelper.AssertType (id, expected, ob)) return;
			Assert.AreEqual (expected.xx, ob.xx, id + " xx");
			// TODO: extraTextNodes
		}
	}

	public class TB: TA
	{
		public int yy = 2;
		
		public static void AssertEquals (string id, TB expected, TB ob)
		{
			if (!AssertionHelper.AssertType (id, expected, ob)) return;
			Assert.AreEqual (expected.yy, ob.yy, id + " yy");
			TA.AssertEquals (id + " base", expected, ob);
		}
	}

	public class UnknownAttributeTester

	{
		[SoapAttribute]
		[XmlAttribute]
		public string aa;
	
		[SoapIgnore]
		[XmlAnyAttribute]
		public XmlAttribute[] extraAtts;

		//		[XmlText(Type=typeof(XmlNode))]
		//		public XmlNode extraText;
		
		public static void AssertEquals (string id, UnknownAttributeTester exp, UnknownAttributeTester ob)
		{
			if (!AssertionHelper.AssertType (id, exp, ob)) return;
			Assert.AreEqual (exp.aa, ob.aa, id + " aa");
			
			if (!AssertionHelper.AssertType (id + " extraAtts", exp.extraAtts, ob.extraAtts)) return;
			
			int p = 0;
			for (int n=0; n<ob.extraAtts.Length; n++)
			{
				XmlAttribute at = ob.extraAtts [n];
				if (at.NamespaceURI == "http://www.w3.org/2000/xmlns/") continue;
				Assert.IsTrue (p < exp.extraAtts.Length, id + " extraAtts length");
				AssertionHelper.AssertEqualsXml (id + ".extraAtts " + n, exp.extraAtts [p], ob.extraAtts[n]);
				p++;
			}
		}
	}

	[SoapType(TypeName="UnTestPart", Namespace="mm")]
	[XmlType(TypeName="UnTestPart", Namespace="mm")]
	public class TestPart
	{
		public string name;
		public bool bval;
		
		public static void AssertEquals (string id, TestPart expected, TestPart ob)
		{
			if (!AssertionHelper.AssertType (id, expected, ob)) return;
			Assert.AreEqual (expected.name, ob.name, id + " name");
			Assert.AreEqual (expected.bval, ob.bval, id + " bval");
		}
	}

	[SoapType(Namespace="mm")]
	[XmlType(Namespace="mm")]
	public class AnotherTestPart
	{
		[XmlText]
		public long lo;
	}

	public class UknTestPart : Test
	{
		[XmlElement(IsNullable=true)]
		public string uname;
		public bool uval;
	}

	public struct OneStruct
	{
		public int aa;
		public string cc;
	}

//	[XmlType(Namespace="enum_namespace")]
	public enum option 
	{ 
		AA, 
		[SoapEnum(Name="xmlBB")]
		[XmlEnum(Name="xmlBB")]
		BB, 
		CC, 
		DD 
	}

	public class CustomHashtable : IXmlSerializable
	{
		Hashtable data = new Hashtable ();

		public void Add (string key, string value)
		{
			data.Add (key, value);
		}
		
		public static void AssertEquals (string id, CustomHashtable exp, CustomHashtable ob)
		{
			if (!AssertionHelper.AssertType (id, exp, ob)) return;
			if (!AssertionHelper.AssertType (id, exp.data, ob.data)) return;
			
			Assert.AreEqual (exp.data.Count, ob.data.Count, id + " data Count");
			
			foreach (DictionaryEntry entry in exp.data)
				Assert.AreEqual (entry.Value, ob.data[entry.Key]);
		}
		
		public void ReadXml (XmlReader reader)
		{
			// Read the element enclosing the object
			reader.ReadStartElement();
			reader.MoveToContent ();

			// Reads the "data" element
			reader.ReadStartElement();
			reader.MoveToContent ();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string key = reader.LocalName;
					data [key] = reader.ReadElementString ();
				}
				else
					reader.Skip ();
				reader.MoveToContent ();
			}

			reader.ReadEndElement ();
		}

		public void WriteXml (XmlWriter writer)
		{
			writer.WriteStartElement ("data");
			foreach (DictionaryEntry entry in data) 
			{
				writer.WriteElementString ((string)entry.Key, (string)entry.Value);
			}
			writer.WriteEndElement ();
		}

		public XmlSchema GetSchema ()
		{
			XmlSchema s = new XmlSchema ();
			s.TargetNamespace = "http://www.go-mono.org/schemas";
			s.Id = "monoschema";
			XmlSchemaElement e = new XmlSchemaElement ();
			e.Name = "data";
			s.Items.Add (e);
			XmlSchemaComplexType cs = new XmlSchemaComplexType ();
			XmlSchemaSequence seq = new XmlSchemaSequence ();
			XmlSchemaAny any = new XmlSchemaAny ();
			any.MinOccurs = 0;
			any.MaxOccurs = decimal.MaxValue;
			seq.Items.Add (any);
			cs.Particle = seq;
			e.SchemaType = cs;
			return s;
		}
	}
}
