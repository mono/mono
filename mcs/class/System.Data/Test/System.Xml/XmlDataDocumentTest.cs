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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
        public class XmlDataDocumentTest : DataSetAssertion {

		static string EOL = "\n";

		[SetUp]
                public void GetReady() 
                {
                	Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
                }

		[Test]
		public void NewInstance ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			AssertDataSet ("#1", doc.DataSet, "NewDataSet", 0, 0);
			AssertEquals (false, doc.DataSet.EnforceConstraints);
			XmlElement el = doc.CreateElement ("TEST");
			AssertDataSet ("#2", doc.DataSet, "NewDataSet", 0, 0);
			AssertNull (doc.GetRowFromElement (el));
			doc.AppendChild (el);
			AssertDataSet ("#3", doc.DataSet, "NewDataSet", 0, 0);

			DataSet ds = new DataSet ();
			doc = new XmlDataDocument (ds);
			AssertEquals (true, doc.DataSet.EnforceConstraints);
		}

		[Test]
		public void SimpleLoad ()
		{
			string xml001 = "<root/>";
			XmlDataDocument doc = new XmlDataDocument ();
			DataSet ds = new DataSet ();
			ds.InferXmlSchema (new StringReader (xml001), null);
			doc.LoadXml (xml001);

			string xml002 = "<root><child/></root>";
			doc = new XmlDataDocument ();
			ds = new DataSet ();
			ds.InferXmlSchema (new StringReader (xml002), null);
			doc.LoadXml (xml002);

			string xml003 = "<root><col1>test</col1><col1></col1></root>";
			doc = new XmlDataDocument ();
			ds = new DataSet ();
			ds.InferXmlSchema (new StringReader (xml003), null);
			doc.LoadXml (xml003);

			string xml004 = "<set><tab1><col1>test</col1><col1>test2</col1></tab1><tab2><col2>test3</col2><col2>test4</col2></tab2></set>";
			doc = new XmlDataDocument ();
			ds = new DataSet ();
			ds.InferXmlSchema (new StringReader (xml004), null);
			doc.LoadXml (xml004);
		}

		[Test]
                public void CloneNode ()
                {
                	XmlDataDocument doc = new XmlDataDocument ();
			
                	doc.DataSet.ReadXmlSchema ("Test/System.Xml/region.xsd");
                	doc.Load ("Test/System.Xml/region.xml");
                
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
			doc.DataSet.ReadXmlSchema ("Test/System.Xml/region.xsd");
			doc.Load ("Test/System.Xml/region.xml");

			XmlElement Element = doc.GetElementFromRow (doc.DataSet.Tables [0].Rows [1]);
			Element.FirstChild.InnerText = "64";
			AssertEquals ("test#01", "64", doc.DataSet.Tables [0].Rows [1] [0]);
			
			DataSet Set = new DataSet ();
			Set.ReadXml ("Test/System.Xml/region.xml");
			doc = new XmlDataDocument (Set);
			
			Element = doc.GetElementFromRow (doc.DataSet.Tables [0].Rows [1]);
			AssertNotNull (Element);
			
			try {
				Element.FirstChild.InnerText = "64";
				Fail ("test#02");
			} catch (InvalidOperationException) {
			}
			
			AssertEquals ("test#05", "2", doc.DataSet.Tables [0].Rows [1] [0]);
			
			Set.EnforceConstraints = false;
			Element.FirstChild.InnerText = "64";
			AssertEquals ("test#06", "64", doc.DataSet.Tables [0].Rows [1] [0]);			

		}
		
		[Test]
		public void EditingDataSet ()
		{
			string xml = "<Root><Region><RegionID>1</RegionID><RegionDescription>Eastern\r\n   </RegionDescription></Region><Region><RegionID>2</RegionID><RegionDescription>Western\r\n   </RegionDescription></Region><Region><RegionID>3</RegionID><RegionDescription>Northern\r\n   </RegionDescription></Region><Region><RegionID>4</RegionID><RegionDescription>Southern\r\n   </RegionDescription></Region><MoreData><Column1>12</Column1><Column2>Hi There</Column2></MoreData><MoreData><Column1>12</Column1><Column2>Hi There</Column2></MoreData></Root>";

			XmlReader Reader = new XmlTextReader ("Test/System.Xml/region.xml");
			XmlDataDocument Doc = new XmlDataDocument ();
			Doc.DataSet.ReadXml (Reader);
			StringWriter sw = new StringWriter ();
			XmlTextWriter xw = new XmlTextWriter (sw);
			Doc.DataSet.WriteXml (xw);
			string s = sw.ToString ();
			AssertEquals ("#1", xml, s);
			AssertEquals ("#2", xml, Doc.InnerXml);
			AssertEquals ("test#01", "EndOfFile", Reader.ReadState.ToString ());

			DataSet Set = Doc.DataSet;
			AssertEquals ("test#01.5", "2", Set.Tables [0].Rows [1] [0]);
			Set.Tables [0].Rows [1] [0] = "64";
			AssertEquals ("test#02", "64", Doc.FirstChild.FirstChild.NextSibling.FirstChild.InnerText);
		}
		
		[Test]
		public void CreateElement1 ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema ("Test/System.Xml/region.xsd");
			doc.Load ("Test/System.Xml/region.xml");
			
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
			doc.DataSet.ReadXmlSchema ("Test/System.Xml/region.xsd");
			doc.Load ("Test/System.Xml/region.xml");
			
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
			doc.DataSet.ReadXmlSchema ("Test/System.Xml/region.xsd");
			doc.Load ("Test/System.Xml/region.xml");
			
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
			doc.DataSet.ReadXmlSchema ("Test/System.Xml/region.xsd");
			doc.Load ("Test/System.Xml/region.xml");

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
                        doc.DataSet.ReadXmlSchema("Test/System.Xml/store.xsd"); 
			Console.WriteLine ("books: " + doc.DataSet.Tables.Count);
                        //Load the XML data.
                        doc.Load("Test/System.Xml/2books.xml");
			
                        //Update the price on the first book using the DataSet methods.
                        DataTable books = doc.DataSet.Tables["book"];
			Console.WriteLine ("books: " + doc.DataSet.Tables [0].TableName);
                        books.Rows[0]["price"] = "12.95";  
			
                        //string outstring = "";
                        TextWriter text = new StringWriter ();
			text.NewLine = "\n";
                        doc.Save(text);

                        //str.Read (bytes, 0, (int)str.Length);
                        //String OutString = new String (bytes);
			
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);                	
                        Assert ("#A01", substring.IndexOf ("<?xml version=\"1.0\" encoding=\"utf-16\"?>") == 0);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#A02", substring.IndexOf ("<!--sample XML fragment-->") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#A03", substring.IndexOf ("<bookstore>") != -1);
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#A04", substring.IndexOf ("  <book genre=\"novel\" ISBN=\"10-861003-324\">") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#A05", substring.IndexOf ("    <title>The Handmaid's Tale</title>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("#A06", "    <price>12.95</price>", substring);
                	
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#A07", substring.IndexOf ("  </book>") != -1);
                	
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#A08", substring.IndexOf ("  <book genre=\"novel\" ISBN=\"1-861001-57-5\">") != -1);
                	
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#A09", substring.IndexOf ("    <title>Pride And Prejudice</title>") != -1);
                        
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#A10", substring.IndexOf ("    <price>24.95</price>") != -1);
                        
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
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
                        RegionDS.ReadXmlSchema ("Test/System.Xml/region.xsd");
			AssertEquals ("Was read correct?", 1, RegionDS.Tables.Count);
			XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
                        DataDoc.Load("Test/System.Xml/region.xml" );


                        RegionRow = RegionDS.Tables[0].Rows[0];
			
                        RegionDS.AcceptChanges ();
                        RegionRow["RegionDescription"] = "Reeeeeaalllly Far East!";
			
                        RegionDS.AcceptChanges ();
			
                        TextWriter text = new StringWriter ();
			text.NewLine = "\n";
                        DataDoc.Save (text);
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        
			//AssertEquals ("#B01", "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("#B02", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B03", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B04", substring.IndexOf ("    <RegionID>1</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("#B05", "    <RegionDescription>Reeeeeaalllly Far East!</RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B06", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B07", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B08", substring.IndexOf ("    <RegionID>2</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B09", substring.IndexOf ("    <RegionDescription>Western") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B10", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B11", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B12", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B13", substring.IndexOf ("    <RegionID>3</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B14", substring.IndexOf ("    <RegionDescription>Northern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B15", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B16", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B17", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B18", substring.IndexOf ("    <RegionID>4</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B19", substring.IndexOf ("    <RegionDescription>Southern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B20", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B21", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B22", substring.IndexOf ("  <MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B23", substring.IndexOf ("    <Column1>12</Column1>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B24", substring.IndexOf ("    <Column2>Hi There</Column2>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B25", substring.IndexOf ("  </MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B26", substring.IndexOf ("  <MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B27", substring.IndexOf ("    <Column1>12</Column1>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B28", substring.IndexOf ("    <Column2>Hi There</Column2>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#B29", substring.IndexOf ("  </MoreData>") != -1);
                }
                
		[Test]
                public void Test3()
                {
                	XmlDataDocument DataDoc = new XmlDataDocument ();
                	DataSet dataset = DataDoc.DataSet;
                	dataset.ReadXmlSchema ("Test/System.Xml/region.xsd");
                        DataDoc.Load("Test/System.Xml/region.xml" );

			DataDoc.GetElementsByTagName ("Region") [0].RemoveAll ();

                        TextWriter text = new StringWriter ();
			text.NewLine = "\n";
			dataset.WriteXml (text);
                        //DataDoc.Save (text);
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);

                        Assert ("#C01", substring.IndexOf ("<Root>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("#C02", "  <Region />", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("#C03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("#C04", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// Regardless of NewLine value, original xml contains CR
			// (but in the context of XML spec, it should be normalized)
                        AssertEquals ("#C05", "    <RegionDescription>Western\r", substring);
                        
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#C06", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("#C07", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#C08", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#C09", substring.IndexOf ("    <RegionID>3</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// Regardless of NewLine value, original xml contains CR
			// (but in the context of XML spec, it should be normalized)
                        AssertEquals ("#C10", "    <RegionDescription>Northern\r", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#C11", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#C12", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#C13", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#C14", substring.IndexOf ("    <RegionID>4</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#C15", substring.IndexOf ("    <RegionDescription>Southern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#C16", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#C17", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.Length);
                        Assert ("#C18", substring.IndexOf ("</Root>") != -1);

                }

		[Test]
		public void Test4 ()
		{
                        DataSet RegionDS = new DataSet ();
			
                        RegionDS.ReadXmlSchema ("Test/System.Xml/region.xsd");
                        XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
                        DataDoc.Load("Test/System.Xml/region.xml" );
			AssertEquals (true, RegionDS.EnforceConstraints);
			DataTable table = DataDoc.DataSet.Tables ["Region"];
			DataRow newRow = table.NewRow ();
			newRow [0] = "new row";
			newRow [1] = "new description";
			
			table.Rows.Add (newRow);			
			
                        TextWriter text = new StringWriter ();
			text.NewLine = "\n";
                        DataDoc.Save (text);
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F02", substring.IndexOf ("<Root>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F03", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F04", substring.IndexOf ("    <RegionID>1</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// Regardless of NewLine value, original xml contains CR
			// (but in the context of XML spec, it should be normalized)
                        AssertEquals ("#F05", "    <RegionDescription>Eastern\r", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("#F06", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F07", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F08", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F09", substring.IndexOf ("    <RegionID>2</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F10", substring.IndexOf ("    <RegionDescription>Western") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F11", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F12", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F13", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F14", substring.IndexOf ("    <RegionID>3</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F15", substring.IndexOf ("    <RegionDescription>Northern") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F16", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F17", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F18", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F19", substring.IndexOf ("    <RegionID>4</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// Regardless of NewLine value, original xml contains CR
			// (but in the context of XML spec, it should be normalized)
                        AssertEquals ("#F20", "    <RegionDescription>Southern\r", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F21", substring.IndexOf ("   </RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F22", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F23", substring.IndexOf ("  <MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F24", substring.IndexOf ("    <Column1>12</Column1>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F25", substring.IndexOf ("    <Column2>Hi There</Column2>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F26", substring.IndexOf ("  </MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F27", substring.IndexOf ("  <MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F28", substring.IndexOf ("    <Column1>12</Column1>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F29", substring.IndexOf ("    <Column2>Hi There</Column2>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F30", substring.IndexOf ("  </MoreData>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F31", substring.IndexOf ("  <Region>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F32", substring.IndexOf ("    <RegionID>new row</RegionID>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F33", substring.IndexOf ("    <RegionDescription>new description</RegionDescription>") != -1);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert ("#F34", substring.IndexOf ("  </Region>") != -1);

                        substring = TextString.Substring (0, TextString.Length);
                        Assert ("#F35", substring.IndexOf ("</Root>") != -1);
		}

		[Test]		
		public void Test5 ()
		{
                        DataSet RegionDS = new DataSet ();
			
                        RegionDS.ReadXmlSchema ("Test/System.Xml/region.xsd");
                        XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
                        DataDoc.Load("Test/System.Xml/region.xml" );
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
			
			RegionDS.ReadXmlSchema ("Test/System.Xml/region.xsd");
			XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
			DataDoc.Load("Test/System.Xml/region.xml" );
			DataDoc.DataSet.EnforceConstraints = false;
			
			XmlElement newNode = DataDoc.CreateElement ("Region");
			XmlElement newChildNode = DataDoc.CreateElement ("RegionID");
			
			newChildNode.InnerText = "64";
			XmlElement newChildNode2 = null;
			try {
				newChildNode2 = DataDoc.CreateElement ("something else");
				Fail ("#H01");
			} catch (XmlException) {
			}
			newChildNode2 = DataDoc.CreateElement ("something_else");
			
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
                	doc.DataSet.ReadXmlSchema ("Test/System.Xml/region.xsd");
                	doc.Load ("Test/System.Xml/region.xml");
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
                	doc.DataSet.ReadXmlSchema ("Test/System.Xml/region.xsd");
                	doc.Load ("Test/System.Xml/region.xml");
			XmlElement root = doc.DocumentElement;

        		DataRow row = doc.GetRowFromElement((XmlElement)root.FirstChild);
			
                	AssertEquals ("#E01", "1", row [0]);

                	row = doc.GetRowFromElement((XmlElement)root.ChildNodes [2]);
                	AssertEquals ("#E02", "3", row [0]);
			
                }
        }
}
