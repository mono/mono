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
        public class XmlDataDocumentTest : Assertion {


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
			
                	AssertEquals ("#I01", 0, doc2.ChildNodes.Count);
                	AssertEquals ("#I02", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", doc2.DataSet.GetXmlSchema ().Substring (0, 39));
                	
                	doc2 = (XmlDataDocument)doc.CloneNode (true);
                	
                	AssertEquals ("#I03", 2, doc2.ChildNodes.Count);
                	AssertEquals ("#I04", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", doc2.DataSet.GetXmlSchema ().Substring (0, 39));
                	
                	doc.DataSet.Tables [0].Rows [0][0] = "64";
              
                	AssertEquals ("#I05", "1", doc2.DataSet.Tables [0].Rows [0][0].ToString ());
                }

		[Test]
		public void EditingXmlTree ()
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
		
		[Test]
		public void EditingDataSet ()
		{
			XmlReader Reader = new XmlTextReader ("System.Xml/region.xml");
			XmlDataDocument Doc = new XmlDataDocument ();
			Doc.DataSet.ReadXml (Reader);
			AssertEquals ("test#01", "EndOfFile", Reader.ReadState.ToString ());

			DataSet Set = Doc.DataSet;
			Set.Tables [0].Rows [1] [0] = "64";
			AssertEquals ("test#02", "64", Doc.FirstChild.FirstChild.NextSibling.FirstChild.InnerText);
		}
		
		[Test]
		public void CreateElement1 ()
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
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("test#05", substring.IndexOf ("<Root>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("test#06", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("test#07", substring.IndexOf ("    <RegionID>1</RegionID>") != -1);

			for (int i = 0; i < 26; i++) {
	                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
        	                TextString = TextString.Substring (TextString.IndexOf("\n") + 1);				
			}
			
                        substring = TextString.Substring (0, TextString.Length);                        
                        Assert ("test#07", substring.IndexOf ("</Root>") != -1);			
		}
	
		[Test]
		public void CreateElement2 ()
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

		[Test]
		public void CreateElement3 ()
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
		
		[Test]
		public void Navigator ()
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
                        books.Rows[0]["price"] = "12.95";  
			
                        //string outstring = "";
                        TextWriter text = new StringWriter ();
                        doc.Save(text);

                        //str.Read (bytes, 0, (int)str.Length);
                        //String OutString = new String (bytes);
			
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);                	
                        Assert ("#A01", substring.IndexOf ("<?xml version=\"1.0\" encoding=\"utf-16\"?>") == 0);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#A02", substring.IndexOf ("<!--sample XML fragment-->") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#A03", substring.IndexOf ("<bookstore>") != -1);
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#A04", substring.IndexOf ("  <book genre=\"novel\" ISBN=\"10-861003-324\">") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#A05", substring.IndexOf ("    <title>The Handmaid's Tale</title>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#A06", substring.IndexOf ("    <price>12.95</price>") != -1);
                	
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#A07", substring.IndexOf ("  </book>") != -1);
                	
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#A08", substring.IndexOf ("  <book genre=\"novel\" ISBN=\"1-861001-57-5\">") != -1);
                	
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#A09", substring.IndexOf ("    <title>Pride And Prejudice</title>") != -1);
                        
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#A10", substring.IndexOf ("    <price>24.95</price>") != -1);
                        
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#A11", substring.IndexOf ("  </book>") != -1);
                        
                        substring = TextString;
                        Assert ("#A12", substring.IndexOf ("</bookstore>") != -1);
			
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
                        Assert ("#B02", substring.IndexOf ("<Root>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B03", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B04", substring.IndexOf ("    <RegionID>1</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B05", substring.IndexOf ("    <RegionDescription>Reeeeeaalllly Far East!</RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B06", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B07", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B08", substring.IndexOf ("    <RegionID>2</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B09", substring.IndexOf ("    <RegionDescription>Western") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B10", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B11", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B12", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B13", substring.IndexOf ("    <RegionID>3</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B14", substring.IndexOf ("    <RegionDescription>Northern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B15", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B16", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B17", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B18", substring.IndexOf ("    <RegionID>4</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B19", substring.IndexOf ("    <RegionDescription>Southern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B20", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B21", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B22", substring.IndexOf ("  <MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B23", substring.IndexOf ("    <Column1>12</Column1>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B24", substring.IndexOf ("    <Column2>Hi There</Column2>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B25", substring.IndexOf ("  </MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B26", substring.IndexOf ("  <MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B27", substring.IndexOf ("    <Column1>12</Column1>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B28", substring.IndexOf ("    <Column2>Hi There</Column2>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#B29", substring.IndexOf ("  </MoreData>") != -1);
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

                        Assert ("#C01", substring.IndexOf ("<Root>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C02", substring.IndexOf ("  <Region />") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C03", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C04", substring.IndexOf ("    <RegionID>2</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n")- 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C05", substring.IndexOf ("    <RegionDescription>Western") != -1);
                        
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C06", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C07", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C08", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C09", substring.IndexOf ("    <RegionID>3</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C10", substring.IndexOf ("    <RegionDescription>Northern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C11", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C12", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C13", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C14", substring.IndexOf ("    <RegionID>4</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C15", substring.IndexOf ("    <RegionDescription>Southern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C16", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#C17", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.Length);
                        Assert ("#C18", substring.IndexOf ("</Root>") != -1);

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

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F02", substring.IndexOf ("<Root>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F03", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F04", substring.IndexOf ("    <RegionID>1</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F05", substring.IndexOf ("    <RegionDescription>Eastern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F06", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F07", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F08", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F09", substring.IndexOf ("    <RegionID>2</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F10", substring.IndexOf ("    <RegionDescription>Western") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F11", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F12", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F13", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F14", substring.IndexOf ("    <RegionID>3</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F15", substring.IndexOf ("    <RegionDescription>Northern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F16", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F17", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F18", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F19", substring.IndexOf ("    <RegionID>4</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F20", substring.IndexOf ("    <RegionDescription>Southern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F21", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F22", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F23", substring.IndexOf ("  <MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F24", substring.IndexOf ("    <Column1>12</Column1>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F25", substring.IndexOf ("    <Column2>Hi There</Column2>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F26", substring.IndexOf ("  </MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F27", substring.IndexOf ("  <MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F28", substring.IndexOf ("    <Column1>12</Column1>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F29", substring.IndexOf ("    <Column2>Hi There</Column2>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F30", substring.IndexOf ("  </MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F31", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F32", substring.IndexOf ("    <RegionID>new row</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F33", substring.IndexOf ("    <RegionDescription>new description</RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#F34", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.Length);
                        Assert ("#F35", substring.IndexOf ("</Root>") != -1);
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
                        Assert ("#G04", substring.IndexOf ("  <Region>") != -1);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#G05", substring.IndexOf ("    <RegionID>64</RegionID>") != -1);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#G06", substring.IndexOf ("    <RegionDescription>test node</RegionDescription>") != -1);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#G07", substring.IndexOf ("  </Region>") != -1);
			
                        substring = TextString.Substring (0, TextString.Length);
                        Assert ("#G08", substring.IndexOf ("</Root>") != -1);

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
                        
			Assert ("#H03", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#H04", substring.IndexOf ("    <RegionID>64</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") );
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        Assert ("#H05", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.Length);
                        Assert ("#H06", substring.IndexOf ("</Root>") != -1);
			
			
		}

		[Test]
                public void GetElementFromRow ()
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
                
		[Test]
                public void GetRowFromElement ()
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
