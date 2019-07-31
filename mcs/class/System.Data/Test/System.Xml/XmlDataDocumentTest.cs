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

using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Data.Xml
{
	[TestFixture]
	public class XmlDataDocumentTest : DataSetAssertion {

		static string EOL = "\n";

		private CultureInfo originalCulture;

		[SetUp]
		public void SetUp ()
		{
			originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		[Test]
		public void NewInstance ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			AssertDataSet ("#1", doc.DataSet, "NewDataSet", 0, 0);
			Assert.IsFalse (doc.DataSet.EnforceConstraints);
			XmlElement el = doc.CreateElement ("TEST");
			AssertDataSet ("#2", doc.DataSet, "NewDataSet", 0, 0);
			Assert.IsNull (doc.GetRowFromElement (el));
			doc.AppendChild (el);
			AssertDataSet ("#3", doc.DataSet, "NewDataSet", 0, 0);

			DataSet ds = new DataSet ();
			doc = new XmlDataDocument (ds);
			Assert.IsTrue (doc.DataSet.EnforceConstraints);
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
			
                	doc.DataSet.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
                	doc.Load (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));
                
                	XmlDataDocument doc2 = (XmlDataDocument)doc.CloneNode (false);
			
                	Assert.AreEqual (0, doc2.ChildNodes.Count, "#I01");
                	Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", doc2.DataSet.GetXmlSchema ().Substring (0, 39), "#I02");
                	
                	doc2 = (XmlDataDocument)doc.CloneNode (true);
                	
                	Assert.AreEqual (2, doc2.ChildNodes.Count, "#I03");
                	Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", doc2.DataSet.GetXmlSchema ().Substring (0, 39), "#I04");
                	
                	doc.DataSet.Tables [0].Rows [0][0] = "64";
              
                	Assert.AreEqual ("1", doc2.DataSet.Tables [0].Rows [0][0].ToString (), "#I05");
                }

		[Test]
		public void EditingXmlTree ()
		{	
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			doc.Load (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));

			XmlElement Element = doc.GetElementFromRow (doc.DataSet.Tables [0].Rows [1]);
			Element.FirstChild.InnerText = "64";
			Assert.AreEqual ("64", doc.DataSet.Tables [0].Rows [1] [0], "test#01");
			
			DataSet Set = new DataSet ();
			Set.ReadXml (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));
			doc = new XmlDataDocument (Set);
			
			Element = doc.GetElementFromRow (doc.DataSet.Tables [0].Rows [1]);
			Assert.IsNotNull (Element);
			
			try {
				Element.FirstChild.InnerText = "64";
				Assert.Fail ("test#02");
			} catch (InvalidOperationException) {
			}
			
			Assert.AreEqual ("2", doc.DataSet.Tables [0].Rows [1] [0], "test#05");
			
			Set.EnforceConstraints = false;
			Element.FirstChild.InnerText = "64";
			Assert.AreEqual ("64", doc.DataSet.Tables [0].Rows [1] [0], "test#06");
		}
		
		[Test]
		public void EditingDataSet ()
		{
			string xml = "<Root><Region><RegionID>1</RegionID><RegionDescription>Eastern" + Environment.NewLine + "   </RegionDescription></Region><Region><RegionID>2</RegionID><RegionDescription>Western" + Environment.NewLine + "   </RegionDescription></Region><Region><RegionID>3</RegionID><RegionDescription>Northern" + Environment.NewLine + "   </RegionDescription></Region><Region><RegionID>4</RegionID><RegionDescription>Southern" + Environment.NewLine + "   </RegionDescription></Region><MoreData><Column1>12</Column1><Column2>Hi There</Column2></MoreData><MoreData><Column1>12</Column1><Column2>Hi There</Column2></MoreData></Root>";

			XmlReader Reader = new XmlTextReader (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));
			XmlDataDocument Doc = new XmlDataDocument ();
			Doc.DataSet.ReadXml (Reader);
			StringWriter sw = new StringWriter ();
			XmlTextWriter xw = new XmlTextWriter (sw);
			Doc.DataSet.WriteXml (xw);
			string s = sw.ToString ().Replace ("\r", "").Replace ("\n", Environment.NewLine);
			Assert.AreEqual (xml, s, "#1");
			Assert.AreEqual (xml, Doc.InnerXml.Replace ("\r", "").Replace ("\n", Environment.NewLine), "#2");
			Assert.AreEqual ("EndOfFile", Reader.ReadState.ToString (), "test#01");

			DataSet Set = Doc.DataSet;
			Assert.AreEqual ("2", Set.Tables [0].Rows [1] [0], "test#01.5");
			Set.Tables [0].Rows [1] [0] = "64";
			Assert.AreEqual ("64", Doc.FirstChild.FirstChild.NextSibling.FirstChild.InnerText, "test#02");
		}
		
		[Test]
		public void CreateElement1 ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			doc.Load (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));
			
			XmlElement Element = doc.CreateElement ("prefix", "localname", "namespaceURI");
			Assert.AreEqual ("prefix", Element.Prefix, "test#01");
			Assert.AreEqual ("localname", Element.LocalName, "test#02");
			Assert.AreEqual ("namespaceURI", Element.NamespaceURI, "test#03");
			doc.ImportNode (Element, false);
			
			TextWriter text = new StringWriter ();
			doc.Save(text);

			string substring = string.Empty;
			string TextString = text.ToString ();

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			Assert.IsTrue (substring.IndexOf ("<Root>") != -1, "test#05");

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "test#06");

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>1</RegionID>") != -1, "test#07");

			for (int i = 0; i < 26; i++) {
				substring = TextString.Substring (0, TextString.IndexOf("\n"));
				TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			}

			substring = TextString.Substring (0, TextString.Length);
			Assert.IsTrue (substring.IndexOf ("</Root>") != -1, "test#08");
		}

		[Test]
		public void CreateElement2 ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			doc.Load (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));

			XmlElement Element = doc.CreateElement ("ElementName");
			Assert.AreEqual (string.Empty, Element.Prefix, "test#01");
			Assert.AreEqual ("ElementName", Element.LocalName, "test#02");
			Assert.AreEqual (string.Empty, Element.NamespaceURI, "test#03");

			Element = doc.CreateElement ("prefix:ElementName");
			Assert.AreEqual ("prefix", Element.Prefix, "test#04");
			Assert.AreEqual ("ElementName", Element.LocalName, "test#05");
			Assert.AreEqual (string.Empty, Element.NamespaceURI, "test#06");
		}

		[Test]
		public void CreateElement3 ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			doc.Load (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));
			
			XmlElement Element = doc.CreateElement ("ElementName", "namespace");
			Assert.AreEqual (string.Empty, Element.Prefix, "test#01");
			Assert.AreEqual ("ElementName", Element.LocalName, "test#02");
			Assert.AreEqual ("namespace", Element.NamespaceURI, "test#03");
			
			Element = doc.CreateElement ("prefix:ElementName", "namespace");
			Assert.AreEqual ("prefix", Element.Prefix, "test#04");
			Assert.AreEqual ("ElementName", Element.LocalName, "test#05");
			Assert.AreEqual ("namespace", Element.NamespaceURI, "test#06");
		}

		[Test]
		public void Navigator ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			doc.Load (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));

			XPathNavigator Nav = doc.CreateNavigator ();
			
			Nav.MoveToRoot ();
			Nav.MoveToFirstChild ();

			Assert.AreEqual ("Root", Nav.Name.ToString (), "test#01");
			Assert.AreEqual (string.Empty, Nav.NamespaceURI.ToString (), "test#02");
			Assert.AreEqual ("False", Nav.IsEmptyElement.ToString (), "test#03");
			Assert.AreEqual ("Element", Nav.NodeType.ToString (), "test#04");
			Assert.AreEqual (string.Empty, Nav.Prefix, "test#05");
			
			Nav.MoveToFirstChild ();
			Nav.MoveToNext ();
			Assert.AreEqual ("Region", Nav.Name.ToString (), "test#06");
			
			Assert.AreEqual ("2Western", Nav.Value.Substring(0, Nav.Value.IndexOf (EOL)), "test#07");
			Nav.MoveToFirstChild ();
			Assert.AreEqual ("2", Nav.Value, "test#08");
			Nav.MoveToRoot ();
			Assert.AreEqual ("Root", Nav.NodeType.ToString (), "test#09");
		}

		// Test constructor
		[Test]
		public void Test1()
		{
			//Create an XmlDataDocument.
			XmlDataDocument doc = new XmlDataDocument();

			//Load the schema file.
			doc.DataSet.ReadXmlSchema(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/store.xsd")); 
			//Load the XML data.
			doc.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/2books.xml"));
			
			//Update the price on the first book using the DataSet methods.
			DataTable books = doc.DataSet.Tables["book"];
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
			Assert.IsTrue (substring.IndexOf ("<?xml version=\"1.0\" encoding=\"utf-16\"?>") == 0, "#A01");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("<!--sample XML fragment-->") != -1, "#A02");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("<bookstore>") != -1, "#A03");
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <book genre=\"novel\" ISBN=\"10-861003-324\">") != -1, "#A04");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <title>The Handmaid's Tale</title>") != -1, "#A05");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <price>12.95</price>", substring, "#A06");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </book>") != -1, "#A07");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <book genre=\"novel\" ISBN=\"1-861001-57-5\">") != -1, "#A08");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <title>Pride And Prejudice</title>") != -1, "#A09");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <price>24.95</price>") != -1, "#A10");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </book>") != -1, "#A11");

			substring = TextString;
			Assert.IsTrue (substring.IndexOf ("</bookstore>") != -1, "#A12");
		}

		// Test public fields
		[Test]
		public void Test2()
		{
			DataSet RegionDS = new DataSet ();
			DataRow RegionRow;
			RegionDS.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			Assert.AreEqual (1, RegionDS.Tables.Count, "Was read correct?");
			XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
			DataDoc.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml") );

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

			//Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", substring, "#B01");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("<Root>", substring, "#B02");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#B03");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>1</RegionID>") != -1, "#B04");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <RegionDescription>Reeeeeaalllly Far East!</RegionDescription>", substring, "#B05");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#B06");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#B07");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>2</RegionID>") != -1, "#B08");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionDescription>Western") != -1, "#B09");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("   </RegionDescription>") != -1, "#B10");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#B11");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#B12");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>3</RegionID>") != -1, "#B13");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionDescription>Northern") != -1, "#B14");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("   </RegionDescription>") != -1, "#B15");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#B16");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#B17");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>4</RegionID>") != -1, "#B18");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionDescription>Southern") != -1, "#B19");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("   </RegionDescription>") != -1, "#B20");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#B21");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <MoreData>") != -1, "#B22");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <Column1>12</Column1>") != -1, "#B23");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <Column2>Hi There</Column2>") != -1, "#B24");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </MoreData>") != -1, "#B25");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <MoreData>") != -1, "#B26");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <Column1>12</Column1>") != -1, "#B27");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <Column2>Hi There</Column2>") != -1, "#B28");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </MoreData>") != -1, "#B29");
		}

		[Test]
		public void Test3()
		{
			XmlDataDocument DataDoc = new XmlDataDocument ();
			DataSet dataset = DataDoc.DataSet;
			dataset.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			DataDoc.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml") );

			DataDoc.GetElementsByTagName ("Region") [0].RemoveAll ();

			TextWriter text = new StringWriter ();
			text.NewLine = "\n";
			dataset.WriteXml (text);
			//DataDoc.Save (text);
			string TextString = text.ToString ();
			string substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);

			Assert.IsTrue (substring.IndexOf ("<Root>") != -1, "#C01");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <Region />", substring, "#C02");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <Region>", substring, "#C03");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <RegionID>2</RegionID>", substring, "#C04");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// Regardless of NewLine value, original xml contains CR
			// (but in the context of XML spec, it should be normalized)
			Assert.AreEqual ("    <RegionDescription>Western", substring, "#C05");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("   </RegionDescription>") != -1, "#C06");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </Region>", substring, "#C07");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#C08");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>3</RegionID>") != -1, "#C09");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// Regardless of NewLine value, original xml contains CR
			// (but in the context of XML spec, it should be normalized)
			Assert.AreEqual ("    <RegionDescription>Northern", substring, "#C10");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("   </RegionDescription>") != -1, "#C11");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#C12");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#C13");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>4</RegionID>") != -1, "#C14");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionDescription>Southern") != -1, "#C15");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("   </RegionDescription>") != -1, "#C16");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#C17");

			substring = TextString.Substring (0, TextString.Length);
			Assert.IsTrue (substring.IndexOf ("</Root>") != -1, "#C18");
		}

		[Test]
		public void Test4 ()
		{
			DataSet RegionDS = new DataSet ();

			RegionDS.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
			DataDoc.Load (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));
			Assert.IsTrue (RegionDS.EnforceConstraints);
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
			Assert.IsTrue (substring.IndexOf ("<Root>") != -1, "#F02");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#F03");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>1</RegionID>") != -1, "#F04");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// Regardless of NewLine value, original xml contains CR
			// (but in the context of XML spec, it should be normalized)
			Assert.AreEqual ("    <RegionDescription>Eastern", substring, "#F05");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("   </RegionDescription>", substring, "#F06");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#F07");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#F08");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>2</RegionID>") != -1, "#F09");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionDescription>Western") != -1, "#F10");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("   </RegionDescription>") != -1, "#F11");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#F12");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#F13");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>3</RegionID>") != -1, "#F14");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionDescription>Northern") != -1, "#F15");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("   </RegionDescription>") != -1, "#F16");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#F17");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#F18");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>4</RegionID>") != -1, "#F19");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// Regardless of NewLine value, original xml contains CR
			// (but in the context of XML spec, it should be normalized)
			Assert.AreEqual ("    <RegionDescription>Southern", substring, "#F20");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("   </RegionDescription>") != -1, "#F21");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#F22");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <MoreData>") != -1, "#F23");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <Column1>12</Column1>") != -1, "#F24");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <Column2>Hi There</Column2>") != -1, "#F25");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </MoreData>") != -1, "#F26");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <MoreData>") != -1, "#F27");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <Column1>12</Column1>") != -1, "#F28");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <Column2>Hi There</Column2>") != -1, "#F29");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </MoreData>") != -1, "#F30");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#F31");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>new row</RegionID>") != -1, "#F32");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("    <RegionDescription>new description</RegionDescription>") != -1, "#F33");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#F34");

			substring = TextString.Substring (0, TextString.Length);
			Assert.IsTrue (substring.IndexOf ("</Root>") != -1, "#F35");
		}

		[Test]
		public void Test5 ()
		{
			DataSet RegionDS = new DataSet ();

			RegionDS.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
			DataDoc.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml") );
			try {
				DataDoc.DocumentElement.AppendChild (DataDoc.DocumentElement.FirstChild);
				Assert.Fail ("#G01");
			} catch (InvalidOperationException e) {
				Assert.AreEqual (typeof (InvalidOperationException), e.GetType (), "#G02");
				Assert.AreEqual ("Please set DataSet.EnforceConstraints == false before trying to edit " +
					              "XmlDataDocument using XML operations.", e.Message, "#G03");
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
			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#G04");

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>64</RegionID>") != -1, "#G05");

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			Assert.IsTrue (substring.IndexOf ("    <RegionDescription>test node</RegionDescription>") != -1, "#G06");

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#G07");

			substring = TextString.Substring (0, TextString.Length);
			Assert.IsTrue (substring.IndexOf ("</Root>") != -1, "#G08");
		}

		[Test]
		public void Test6 ()
		{
			DataSet RegionDS = new DataSet ();

			RegionDS.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			XmlDataDocument DataDoc = new XmlDataDocument (RegionDS);
			DataDoc.Load(TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml") );
			DataDoc.DataSet.EnforceConstraints = false;

			XmlElement newNode = DataDoc.CreateElement ("Region");
			XmlElement newChildNode = DataDoc.CreateElement ("RegionID");

			newChildNode.InnerText = "64";
			XmlElement newChildNode2 = null;
			try {
				newChildNode2 = DataDoc.CreateElement ("something else");
				Assert.Fail ("#H01");
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

			Assert.IsTrue (substring.IndexOf ("  <Region>") != -1, "#H03");

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			Assert.IsTrue (substring.IndexOf ("    <RegionID>64</RegionID>") != -1, "#H04");

			substring = TextString.Substring (0, TextString.IndexOf("\n") );
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			Assert.IsTrue (substring.IndexOf ("  </Region>") != -1, "#H05");

			substring = TextString.Substring (0, TextString.Length);
			Assert.IsTrue (substring.IndexOf ("</Root>") != -1, "#H06");
		}

		[Test]
		public void GetElementFromRow ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			doc.Load (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));
			DataTable table = doc.DataSet.Tables ["Region"];

			XmlElement element = doc.GetElementFromRow (table.Rows [2]);
			Assert.AreEqual ("Region", element.Name, "#D01");
			Assert.AreEqual ("3", element ["RegionID"].InnerText, "#D02");

			try {
				element = doc.GetElementFromRow (table.Rows [4]);
				Assert.Fail ("#D03");
			} catch (IndexOutOfRangeException e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (), "#D04");
				Assert.AreEqual ("There is no row at position 4.", e.Message, "#D05");
			}
		}

		[Test]
		public void GetRowFromElement ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xsd"));
			doc.Load (TestResourceHelper.GetFullPathOfResource ("Test/System.Xml/region.xml"));
			XmlElement root = doc.DocumentElement;

			DataRow row = doc.GetRowFromElement((XmlElement)root.FirstChild);

			Assert.AreEqual ("1", row [0], "#E01");

			row = doc.GetRowFromElement((XmlElement)root.ChildNodes [2]);
			Assert.AreEqual ("3", row [0], "#E02");
		}
	}
}
