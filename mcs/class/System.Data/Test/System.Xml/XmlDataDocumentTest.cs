//
// XmlDataDocumentTestTest.cs - NUnit Test Cases for  XmlDataDocument
//
// Ville Palo (vi64pa@koti.soon.fi)
//
// (C) Ville Palo 2002
// 

using NUnit.Framework;
using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Threading;
using System.Globalization;

namespace MonoTests.System.Data.Xml
{
        public class XmlDataDocumentTest : TestCase {


                public XmlDataDocumentTest() : base ("System.Xml.XmlDataDocument") {}
                public XmlDataDocumentTest(string name) : base(name) {}

                protected override void TearDown() {}

                protected override void SetUp() 
                {
                	Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
                }

                public static ITest Suite {
                        get {
                                return new TestSuite(typeof(XmlDataDocumentTest));
                        }
                }

                public void TestCloneNode ()
                {
                	XmlDataDocument doc = new XmlDataDocument ();
			
                	doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
                	doc.Load ("System.Xml/region.xml");
                
                	XmlDataDocument doc2 = (XmlDataDocument)doc.CloneNode (false);
			
                	AssertEquals ("#I01", 0, doc2.ChildNodes.Count);
                	AssertEquals ("#I02", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", doc2.DataSet.GetXmlSchema ().Substring (0, 39));
                	
                	doc2 = (XmlDataDocument)doc.CloneNode (true);
                	
                	AssertEquals ("#I03", 2, doc2.ChildNodes.Count);
                	AssertEquals ("#I04", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", doc2.DataSet.GetXmlSchema ().Substring (0, 39));
                	
                	doc.DataSet.Tables [0].Rows [0][0] = "64";
              
                	AssertEquals ("#I05", "1", doc2.DataSet.Tables [0].Rows [0][0].ToString ());
                }

		public void TestEditingXmlTree ()
		{	
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
			doc.Load ("System.Xml/region.xml");

			XmlElement Element = doc.GetElementFromRow (doc.DataSet.Tables [0].Rows [1]);
			Element.FirstChild.InnerText = "64";
			AssertEquals ("test#01", "64", doc.DataSet.Tables [0].Rows [1] [0]);
			
			DataSet Set = new DataSet ();
			Set.ReadXml ("System.Xml/region.xml");
			doc = new XmlDataDocument (Set);
			
			Element = doc.GetElementFromRow (doc.DataSet.Tables [0].Rows [1]);
			
			try {
				Element.FirstChild.InnerText = "64";
				Fail ("test#02");
			} catch (Exception e) {
				AssertEquals ("test#03", typeof (InvalidOperationException), e.GetType ());
				AssertEquals ("test#04", "Please set DataSet.EnforceConstraints == false before trying to edit XmlDataDocument using XML operations.", e.Message);
			}
			
			AssertEquals ("test#05", "2", doc.DataSet.Tables [0].Rows [1] [0]);
			
			Set.EnforceConstraints = false;
			Element.FirstChild.InnerText = "64";
			AssertEquals ("test#06", "64", doc.DataSet.Tables [0].Rows [1] [0]);			

		}
		
		public void TestEditingDataSet ()
		{
			XmlReader Reader = new XmlTextReader ("System.Xml/region.xml");
			XmlDataDocument Doc = new XmlDataDocument ();
			Doc.DataSet.ReadXml (Reader);
			AssertEquals ("test#01", "Interactive", Reader.ReadState.ToString ());

			DataSet Set = Doc.DataSet;
			Set.Tables [0].Rows [1] [0] = "64";
			AssertEquals ("test#02", "64", Doc.FirstChild.FirstChild.NextSibling.FirstChild.InnerText);
		}
		
		public void TestCreateElement1 ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
			doc.Load ("System.Xml/region.xml");
			
			XmlElement Element = doc.CreateElement ("prefix", "localname", "namespaceURI"); 			
			AssertEquals ("test#01", "prefix", Element.Prefix);
			AssertEquals ("test#02", "localname", Element.LocalName);
			AssertEquals ("test#03", "namespaceURI", Element.NamespaceURI);
			doc.ImportNode (Element, false);
			
                        TextWriter text = new StringWriter ();

                        doc.Save(text);

			string substring = "";
			string TextString = text.ToString ();

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        //AssertEquals ("test#04", "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#05", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#06", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#07", "    <RegionID>1</RegionID>", substring);

			for (int i = 0; i < 26; i++) {
	                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
        	                TextString = TextString.Substring (TextString.IndexOf("\n") + 1);				
			}
			
                        substring = TextString.Substring (0, TextString.Length);                        
                        AssertEquals ("test#07", "</Root>", substring);			
		}
	
		public void TestCreateElement2 ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
			doc.Load ("System.Xml/region.xml");
			
			XmlElement Element = doc.CreateElement ("ElementName"); 			
			AssertEquals ("test#01", "", Element.Prefix);
			AssertEquals ("test#02", "ElementName", Element.LocalName);
			AssertEquals ("test#03", "", Element.NamespaceURI);
			
			Element = doc.CreateElement ("prefix:ElementName");
			AssertEquals ("test#04", "prefix", Element.Prefix);
			AssertEquals ("test#05", "ElementName", Element.LocalName);
			AssertEquals ("test#06", "", Element.NamespaceURI);
		}

		public void TestCreateElement3 ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
			doc.Load ("System.Xml/region.xml");
			
			XmlElement Element = doc.CreateElement ("ElementName", "namespace"); 			
			AssertEquals ("test#01", "", Element.Prefix);
			AssertEquals ("test#02", "ElementName", Element.LocalName);
			AssertEquals ("test#03", "namespace", Element.NamespaceURI);
			
			Element = doc.CreateElement ("prefix:ElementName", "namespace");
			AssertEquals ("test#04", "prefix", Element.Prefix);
			AssertEquals ("test#05", "ElementName", Element.LocalName);
			AssertEquals ("test#06", "namespace", Element.NamespaceURI);
		}
		
		public void TestNavigator ()
		{	
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
			doc.Load ("System.Xml/region.xml");

			XPathNavigator Nav = doc.CreateNavigator ();
			
			Nav.MoveToRoot ();
			Nav.MoveToFirstChild ();

			AssertEquals ("test#01", "Root", Nav.Name.ToString ());
			AssertEquals ("test#02", "", Nav.NamespaceURI.ToString ());
			AssertEquals ("test#03", "False", Nav.IsEmptyElement.ToString ());
			AssertEquals ("test#04", "Element", Nav.NodeType.ToString ());
			AssertEquals ("test#05", "", Nav.Prefix);
			
			Nav.MoveToFirstChild ();
			Nav.MoveToNext ();
			AssertEquals ("test#06", "Region", Nav.Name.ToString ());
			
			AssertEquals ("test#07", "2Western", Nav.Value.Substring(0, Nav.Value.IndexOf ("\n") - 1));
			Nav.MoveToFirstChild ();
			AssertEquals ("test#08", "2", Nav.Value);
			Nav.MoveToRoot ();
			AssertEquals ("test#09", "Root", Nav.NodeType.ToString ());
			
		}

                // Test constructor
                public void Test1()
                {

                             //Create an XmlDataDocument.
                        XmlDataDocument doc = new XmlDataDocument();

                        //Load the schema file.
                        doc.DataSet.ReadXmlSchema("System.Xml/store.xsd"); 
			Console.WriteLine ("books: " + doc.DataSet.Tables.Count);
                        //Load the XML data.
                        doc.Load("System.Xml/2books.xml");
			
                        //Update the price on the first book using the DataSet methods.
                        DataTable books = doc.DataSet.Tables["book"];
			Console.WriteLine ("books: " + doc.DataSet.Tables [0].TableName);
                        books.Rows[0]["price"] = "12,95";  
			
                        //string outstring = "";
                        TextWriter text = new StringWriter ();
                        doc.Save(text);

                        //str.Read (bytes, 0, (int)str.Length);
                        //String OutString = new String (bytes);
			
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A02", "<!--sample XML fragment-->", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A03", "<bookstore>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A04", "  <book genre=\"novel\" ISBN=\"10-861003-324\">", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A05", "    <title>The Handmaid's Tale</title>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A06", "    <price>12.95</price>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A07", "  </book>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A08", "  <book genre=\"novel\" ISBN=\"1-861001-57-5\">", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A09", "    <title>Pride And Prejudice</title>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A10", "    <price>24.95</price>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#A11", "  </book>", substring);
                        substring = TextString;
                        AssertEquals ("#A12", "</bookstore>", substring);
			
                }

                // Test public fields
                public void Test2()
                {
                        DataSet RegionDS = new DataSet ();
                        DataRow RegionRow;
                        RegionDS.ReadXmlSchema ("System.Xml/region.xsd");
                        XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
                        DataDoc.Load("System.Xml/region.xml" );


                        RegionRow = RegionDS.Tables[0].Rows[0];
			
                        RegionDS.AcceptChanges ();
			Console.WriteLine ("***");
                        RegionRow["RegionDescription"] = "Reeeeeaalllly Far East!";
			
                        RegionDS.AcceptChanges ();
			
                        TextWriter text = new StringWriter ();

                        DataDoc.Save (text);
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        
			//AssertEquals ("#B01", "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B02", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B04", "    <RegionID>1</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B05", "    <RegionDescription>Reeeeeaalllly Far East!</RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B06", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B07", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B08", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B09", "    <RegionDescription>Western", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B10", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B11", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B12", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B13", "    <RegionID>3</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B14", "    <RegionDescription>Northern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B15", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B16", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B17", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B18", "    <RegionID>4</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B19", "    <RegionDescription>Southern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B20", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B21", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B22", "  <MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B23", "    <Column1>12</Column1>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B24", "    <Column2>Hi There</Column2>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B25", "  </MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B26", "  <MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B27", "    <Column1>12</Column1>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B28", "    <Column2>Hi There</Column2>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B29", "  </MoreData>", substring);
                }
                
                public void Test3()
                {
                	XmlDataDocument DataDoc = new XmlDataDocument ();
                	DataSet dataset = DataDoc.DataSet;
                	dataset.ReadXmlSchema ("System.Xml/region.xsd");
                        DataDoc.Load("System.Xml/region.xml" );

			DataDoc.GetElementsByTagName ("Region") [0].RemoveAll ();

                        TextWriter text = new StringWriter ();
			dataset.WriteXml (text);
                        //DataDoc.Save (text);
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);

                        AssertEquals ("#C01", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C02", "  <Region />", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C04", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n")- 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C05", "    <RegionDescription>Western", substring);
                        
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C06", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C07", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C08", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C09", "    <RegionID>3</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C10", "    <RegionDescription>Northern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C11", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C12", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C13", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C14", "    <RegionID>4</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C15", "    <RegionDescription>Southern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C16", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C17", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.Length);
                        AssertEquals ("#C18", "</Root>", substring);

                }

                
		public void Test4 ()
		{
                        DataSet RegionDS = new DataSet ();
			
                        RegionDS.ReadXmlSchema ("System.Xml/region.xsd");
                        XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
                        DataDoc.Load("System.Xml/region.xml" );
			DataTable table = DataDoc.DataSet.Tables ["Region"];
			DataRow newRow = table.NewRow ();
			newRow [0] = "new row";
			newRow [1] = "new description";
			
			table.Rows.Add (newRow);			
			
                        TextWriter text = new StringWriter ();

                        DataDoc.Save (text);
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);

                        //AssertEquals ("#F01", "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F02", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F04", "    <RegionID>1</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F05", "    <RegionDescription>Eastern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F06", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F07", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F08", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F09", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F10", "    <RegionDescription>Western", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F11", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F12", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F13", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F14", "    <RegionID>3</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F15", "    <RegionDescription>Northern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F16", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F17", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F18", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F19", "    <RegionID>4</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F20", "    <RegionDescription>Southern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F21", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F22", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F23", "  <MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F24", "    <Column1>12</Column1>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F25", "    <Column2>Hi There</Column2>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F26", "  </MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F27", "  <MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F28", "    <Column1>12</Column1>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F29", "    <Column2>Hi There</Column2>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F30", "  </MoreData>", substring);


                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F31", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F32", "    <RegionID>new row</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F33", "    <RegionDescription>new description</RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#F34", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.Length);
                        AssertEquals ("#F35", "</Root>", substring);
		}
		
		public void Test5 ()
		{
                        DataSet RegionDS = new DataSet ();
			
                        RegionDS.ReadXmlSchema ("System.Xml/region.xsd");
                        XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
                        DataDoc.Load("System.Xml/region.xml" );
			try {
				DataDoc.DocumentElement.AppendChild (DataDoc.DocumentElement.FirstChild);
				Fail ("#G01");
			} catch (Exception e) {
				AssertEquals ("#G02", typeof (InvalidOperationException), e.GetType ());
				AssertEquals ("#G03", "Please set DataSet.EnforceConstraints == false before trying to edit " +
					              "XmlDataDocument using XML operations.", e.Message);
				DataDoc.DataSet.EnforceConstraints = false;
			}
			XmlElement newNode = DataDoc.CreateElement ("Region");
			XmlElement newChildNode = DataDoc.CreateElement ("RegionID");
			newChildNode.InnerText = "64";
			XmlElement newChildNode2 = DataDoc.CreateElement ("RegionDescription");
			newChildNode2.InnerText = "test node";
			newNode.AppendChild (newChildNode);
			newNode.AppendChild (newChildNode2);
			DataDoc.DocumentElement.AppendChild (newNode);
                        TextWriter text = new StringWriter ();
			
                        //DataDoc.Save (text);
			DataDoc.DataSet.WriteXml(text);
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			
			for (int i = 0; i < 21; i++) {
				substring = TextString.Substring (0, TextString.IndexOf("\n"));
				TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			}
                        AssertEquals ("#G04", "  <Region>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#G05", "    <RegionID>64</RegionID>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#G06", "    <RegionDescription>test node</RegionDescription>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#G07", "  </Region>", substring);
			
                        substring = TextString.Substring (0, TextString.Length);
                        AssertEquals ("#G08", "</Root>", substring);

		}
		
		public void Test6 ()
		{
			DataSet RegionDS = new DataSet ();
			
			RegionDS.ReadXmlSchema ("System.Xml/region.xsd");
			XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
			DataDoc.Load("System.Xml/region.xml" );
			DataDoc.DataSet.EnforceConstraints = false;
			
			XmlElement newNode = DataDoc.CreateElement ("Region");
			XmlElement newChildNode = DataDoc.CreateElement ("RegionID");
			
			newChildNode.InnerText = "64";
			XmlElement newChildNode2 = null;
			try {
				newChildNode2 = DataDoc.CreateElement ("something else");
				Fail ("#H01");
			} catch (Exception e) {
				AssertEquals ("#H02", typeof (XmlException), e.GetType ());
				newChildNode2 = DataDoc.CreateElement ("something_else");
			}
			
			newChildNode2.InnerText = "test node";
			
			newNode.AppendChild (newChildNode);
			newNode.AppendChild (newChildNode2);
			DataDoc.DocumentElement.AppendChild (newNode);
			
                        TextWriter text = new StringWriter ();
			
                        //DataDoc.Save (text);
			DataDoc.DataSet.WriteXml(text);
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			
			for (int i = 0; i < 21; i++) {
				substring = TextString.Substring (0, TextString.IndexOf("\n"));
				TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			}
                        
			AssertEquals ("#H03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#H04", "    <RegionID>64</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") );
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#H05", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.Length);
                        AssertEquals ("#H06", "</Root>", substring);
			
			
		}

                public void TestGetElementFromRow ()
                {
                	XmlDataDocument doc = new XmlDataDocument ();
                	doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
                	doc.Load ("System.Xml/region.xml");
                	DataTable table = doc.DataSet.Tables ["Region"];
                	
                	XmlElement element = doc.GetElementFromRow (table.Rows [2]);
                	AssertEquals ("#D01", "Region", element.Name);
                	AssertEquals ("#D02", "3", element ["RegionID"].InnerText);
                	
                	try {
                		element = doc.GetElementFromRow (table.Rows [4]);
                		Fail ("#D03");
                	} catch (Exception e) {
                		AssertEquals ("#D04", typeof (IndexOutOfRangeException), e.GetType ());
                		AssertEquals ("#D05", "There is no row at position 4.", e.Message);
                	}
                }
                
                public void TestGetRowFromElement ()
                {
                	XmlDataDocument doc = new XmlDataDocument ();
                	doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
                	doc.Load ("System.Xml/region.xml");
			XmlElement root = doc.DocumentElement;

        		DataRow row = doc.GetRowFromElement((XmlElement)root.FirstChild);
			
                	AssertEquals ("#E01", "1", row [0]);

                	row = doc.GetRowFromElement((XmlElement)root.ChildNodes [2]);
                	AssertEquals ("#E02", "3", row [0]);
			
                }
        }
}
