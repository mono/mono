//
// XmlDataDocumentTestTest.cs - NUnit Test Cases for  XmlDataDocument
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Ville Palo
// (C) 2003 Martin Willemoes Hansen
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
	[TestFixture]
        public class XmlDataDocumentTest {


		[SetUp]
                public void GetReady() 
                {
                	Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
                }

		[Test]
                public void CloneNode ()
                {
                	XmlDataDocument doc = new XmlDataDocument ();
			
                	doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
                	doc.Load ("System.Xml/region.xml");
                
                	XmlDataDocument doc2 = (XmlDataDocument)doc.CloneNode (false);
			
                	Assertion.AssertEquals ("#I01", 0, doc2.ChildNodes.Count);
                	Assertion.AssertEquals ("#I02", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", doc2.DataSet.GetXmlSchema ().Substring (0, 39));
                	
                	doc2 = (XmlDataDocument)doc.CloneNode (true);
                	
                	Assertion.AssertEquals ("#I03", 2, doc2.ChildNodes.Count);
                	Assertion.AssertEquals ("#I04", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", doc2.DataSet.GetXmlSchema ().Substring (0, 39));
                	
                	doc.DataSet.Tables [0].Rows [0][0] = "64";
              
                	Assertion.AssertEquals ("#I05", "1", doc2.DataSet.Tables [0].Rows [0][0].ToString ());
                }

		[Test]
		public void EditingXmlTree ()
		{	
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
			doc.Load ("System.Xml/region.xml");

			XmlElement Element = doc.GetElementFromRow (doc.DataSet.Tables [0].Rows [1]);
			Element.FirstChild.InnerText = "64";
			Assertion.AssertEquals ("test#01", "64", doc.DataSet.Tables [0].Rows [1] [0]);
			
			DataSet Set = new DataSet ();
			Set.ReadXml ("System.Xml/region.xml");
			doc = new XmlDataDocument (Set);
			
			Element = doc.GetElementFromRow (doc.DataSet.Tables [0].Rows [1]);
			
			try {
				Element.FirstChild.InnerText = "64";
				Assertion.Fail ("test#02");
			} catch (Exception e) {
				Assertion.AssertEquals ("test#03", typeof (InvalidOperationException), e.GetType ());
				Assertion.AssertEquals ("test#04", "Please set DataSet.EnforceConstraints == false before trying to edit XmlDataDocument using XML operations.", e.Message);
			}
			
			Assertion.AssertEquals ("test#05", "2", doc.DataSet.Tables [0].Rows [1] [0]);
			
			Set.EnforceConstraints = false;
			Element.FirstChild.InnerText = "64";
			Assertion.AssertEquals ("test#06", "64", doc.DataSet.Tables [0].Rows [1] [0]);			

		}
		
		[Test]
		public void EditingDataSet ()
		{
			XmlReader Reader = new XmlTextReader ("System.Xml/region.xml");
			XmlDataDocument Doc = new XmlDataDocument ();
			Doc.DataSet.ReadXml (Reader);
			Assertion.AssertEquals ("test#01", "Interactive", Reader.ReadState.ToString ());

			DataSet Set = Doc.DataSet;
			Set.Tables [0].Rows [1] [0] = "64";
			Assertion.AssertEquals ("test#02", "64", Doc.FirstChild.FirstChild.NextSibling.FirstChild.InnerText);
		}
		
		[Test]
		public void CreateElement1 ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
			doc.Load ("System.Xml/region.xml");
			
			XmlElement Element = doc.CreateElement ("prefix", "localname", "namespaceURI"); 			
			Assertion.AssertEquals ("test#01", "prefix", Element.Prefix);
			Assertion.AssertEquals ("test#02", "localname", Element.LocalName);
			Assertion.AssertEquals ("test#03", "namespaceURI", Element.NamespaceURI);
			doc.ImportNode (Element, false);
			
                        TextWriter text = new StringWriter ();

                        doc.Save(text);

			string substring = "";
			string TextString = text.ToString ();

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        //Assertion.AssertEquals ("test#04", "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("test#05", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("test#06", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("test#07", "    <RegionID>1</RegionID>", substring);

			for (int i = 0; i < 26; i++) {
	                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
        	                TextString = TextString.Substring (TextString.IndexOf("\n") + 1);				
			}
			
                        substring = TextString.Substring (0, TextString.Length);                        
                        Assertion.AssertEquals ("test#07", "</Root>", substring);			
		}
	
		[Test]
		public void CreateElement2 ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
			doc.Load ("System.Xml/region.xml");
			
			XmlElement Element = doc.CreateElement ("ElementName"); 			
			Assertion.AssertEquals ("test#01", "", Element.Prefix);
			Assertion.AssertEquals ("test#02", "ElementName", Element.LocalName);
			Assertion.AssertEquals ("test#03", "", Element.NamespaceURI);
			
			Element = doc.CreateElement ("prefix:ElementName");
			Assertion.AssertEquals ("test#04", "prefix", Element.Prefix);
			Assertion.AssertEquals ("test#05", "ElementName", Element.LocalName);
			Assertion.AssertEquals ("test#06", "", Element.NamespaceURI);
		}

		[Test]
		public void CreateElement3 ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
			doc.Load ("System.Xml/region.xml");
			
			XmlElement Element = doc.CreateElement ("ElementName", "namespace"); 			
			Assertion.AssertEquals ("test#01", "", Element.Prefix);
			Assertion.AssertEquals ("test#02", "ElementName", Element.LocalName);
			Assertion.AssertEquals ("test#03", "namespace", Element.NamespaceURI);
			
			Element = doc.CreateElement ("prefix:ElementName", "namespace");
			Assertion.AssertEquals ("test#04", "prefix", Element.Prefix);
			Assertion.AssertEquals ("test#05", "ElementName", Element.LocalName);
			Assertion.AssertEquals ("test#06", "namespace", Element.NamespaceURI);
		}
		
		[Test]
		public void Navigator ()
		{	
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
			doc.Load ("System.Xml/region.xml");

			XPathNavigator Nav = doc.CreateNavigator ();
			
			Nav.MoveToRoot ();
			Nav.MoveToFirstChild ();

			Assertion.AssertEquals ("test#01", "Root", Nav.Name.ToString ());
			Assertion.AssertEquals ("test#02", "", Nav.NamespaceURI.ToString ());
			Assertion.AssertEquals ("test#03", "False", Nav.IsEmptyElement.ToString ());
			Assertion.AssertEquals ("test#04", "Element", Nav.NodeType.ToString ());
			Assertion.AssertEquals ("test#05", "", Nav.Prefix);
			
			Nav.MoveToFirstChild ();
			Nav.MoveToNext ();
			Assertion.AssertEquals ("test#06", "Region", Nav.Name.ToString ());
			
			Assertion.AssertEquals ("test#07", "2Western", Nav.Value.Substring(0, Nav.Value.IndexOf ("\n") - 1));
			Nav.MoveToFirstChild ();
			Assertion.AssertEquals ("test#08", "2", Nav.Value);
			Nav.MoveToRoot ();
			Assertion.AssertEquals ("test#09", "Root", Nav.NodeType.ToString ());
			
		}

                // Test constructor
		[Test]
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
                        Assertion.AssertEquals ("#A01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#A02", "<!--sample XML fragment-->", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#A03", "<bookstore>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#A04", "  <book genre=\"novel\" ISBN=\"10-861003-324\">", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#A05", "    <title>The Handmaid's Tale</title>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#A06", "    <price>12.95</price>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#A07", "  </book>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#A08", "  <book genre=\"novel\" ISBN=\"1-861001-57-5\">", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#A09", "    <title>Pride And Prejudice</title>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#A10", "    <price>24.95</price>", substring);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#A11", "  </book>", substring);
                        substring = TextString;
                        Assertion.AssertEquals ("#A12", "</bookstore>", substring);
			
                }

                // Test public fields
		[Test]
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
                        
			//Assertion.AssertEquals ("#B01", "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B02", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B04", "    <RegionID>1</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B05", "    <RegionDescription>Reeeeeaalllly Far East!</RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B06", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B07", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B08", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B09", "    <RegionDescription>Western", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B10", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B11", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B12", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B13", "    <RegionID>3</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B14", "    <RegionDescription>Northern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B15", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B16", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B17", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B18", "    <RegionID>4</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B19", "    <RegionDescription>Southern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B20", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B21", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B22", "  <MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B23", "    <Column1>12</Column1>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B24", "    <Column2>Hi There</Column2>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B25", "  </MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B26", "  <MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B27", "    <Column1>12</Column1>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B28", "    <Column2>Hi There</Column2>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#B29", "  </MoreData>", substring);
                }
                
		[Test]
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

                        Assertion.AssertEquals ("#C01", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C02", "  <Region />", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C04", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n")- 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C05", "    <RegionDescription>Western", substring);
                        
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C06", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C07", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C08", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C09", "    <RegionID>3</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C10", "    <RegionDescription>Northern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C11", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C12", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C13", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C14", "    <RegionID>4</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C15", "    <RegionDescription>Southern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C16", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#C17", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.Length);
                        Assertion.AssertEquals ("#C18", "</Root>", substring);

                }

		[Test]
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

                        //Assertion.AssertEquals ("#F01", "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F02", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F04", "    <RegionID>1</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F05", "    <RegionDescription>Eastern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F06", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F07", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F08", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F09", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F10", "    <RegionDescription>Western", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F11", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F12", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F13", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F14", "    <RegionID>3</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F15", "    <RegionDescription>Northern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F16", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F17", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F18", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F19", "    <RegionID>4</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F20", "    <RegionDescription>Southern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F21", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F22", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F23", "  <MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F24", "    <Column1>12</Column1>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F25", "    <Column2>Hi There</Column2>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F26", "  </MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F27", "  <MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F28", "    <Column1>12</Column1>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F29", "    <Column2>Hi There</Column2>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F30", "  </MoreData>", substring);


                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F31", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F32", "    <RegionID>new row</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F33", "    <RegionDescription>new description</RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#F34", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.Length);
                        Assertion.AssertEquals ("#F35", "</Root>", substring);
		}

		[Test]		
		public void Test5 ()
		{
                        DataSet RegionDS = new DataSet ();
			
                        RegionDS.ReadXmlSchema ("System.Xml/region.xsd");
                        XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
                        DataDoc.Load("System.Xml/region.xml" );
			try {
				DataDoc.DocumentElement.AppendChild (DataDoc.DocumentElement.FirstChild);
				Assertion.Fail ("#G01");
			} catch (Exception e) {
				Assertion.AssertEquals ("#G02", typeof (InvalidOperationException), e.GetType ());
				Assertion.AssertEquals ("#G03", "Please set DataSet.EnforceConstraints == false before trying to edit " +
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
                        Assertion.AssertEquals ("#G04", "  <Region>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#G05", "    <RegionID>64</RegionID>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#G06", "    <RegionDescription>test node</RegionDescription>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#G07", "  </Region>", substring);
			
                        substring = TextString.Substring (0, TextString.Length);
                        Assertion.AssertEquals ("#G08", "</Root>", substring);

		}
		
		[Test]
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
				Assertion.Fail ("#H01");
			} catch (Exception e) {
				Assertion.AssertEquals ("#H02", typeof (XmlException), e.GetType ());
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
                        
			Assertion.AssertEquals ("#H03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#H04", "    <RegionID>64</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") );
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assertion.AssertEquals ("#H05", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.Length);
                        Assertion.AssertEquals ("#H06", "</Root>", substring);
			
			
		}

		[Test]
                public void GetElementFromRow ()
                {
                	XmlDataDocument doc = new XmlDataDocument ();
                	doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
                	doc.Load ("System.Xml/region.xml");
                	DataTable table = doc.DataSet.Tables ["Region"];
                	
                	XmlElement element = doc.GetElementFromRow (table.Rows [2]);
                	Assertion.AssertEquals ("#D01", "Region", element.Name);
                	Assertion.AssertEquals ("#D02", "3", element ["RegionID"].InnerText);
                	
                	try {
                		element = doc.GetElementFromRow (table.Rows [4]);
                		Assertion.Fail ("#D03");
                	} catch (Exception e) {
                		Assertion.AssertEquals ("#D04", typeof (IndexOutOfRangeException), e.GetType ());
                		Assertion.AssertEquals ("#D05", "There is no row at position 4.", e.Message);
                	}
                }
                
		[Test]
                public void GetRowFromElement ()
                {
                	XmlDataDocument doc = new XmlDataDocument ();
                	doc.DataSet.ReadXmlSchema ("System.Xml/region.xsd");
                	doc.Load ("System.Xml/region.xml");
			XmlElement root = doc.DocumentElement;

        		DataRow row = doc.GetRowFromElement((XmlElement)root.FirstChild);
			
                	Assertion.AssertEquals ("#E01", "1", row [0]);

                	row = doc.GetRowFromElement((XmlElement)root.ChildNodes [2]);
                	Assertion.AssertEquals ("#E02", "3", row [0]);
			
                }
        }
}
