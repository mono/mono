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
using System.IO;

namespace MonoTests.System.Data.Xml
{
        public class XmlDataDocumentTest : TestCase {


                public XmlDataDocumentTest() : base ("System.Xml.XmlDataDocument") {}
                public XmlDataDocumentTest(string name) : base(name) {}

                protected override void TearDown() {}

                protected override void SetUp() 
                {
                }

                public static ITest Suite {
                        get {
                                return new TestSuite(typeof(XmlDataDocumentTest));
                        }
                }

                // Test constructor
                public void Test1()
                {

                             //Create an XmlDataDocument.
                        XmlDataDocument doc = new XmlDataDocument();

                        //Load the schema file.
                        doc.DataSet.ReadXmlSchema("System.Xml/store.xsd"); 

                        //Load the XML data.
                        doc.Load("System.Xml/2books.xml");

                        //Update the price on the first book using the DataSet methods.
                        DataTable books = doc.DataSet.Tables["book"];
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
                        RegionRow["RegionDescription"] = "Reeeeeaalllly Far East!";
                        RegionDS.AcceptChanges ();

                        TextWriter text = new StringWriter ();

                        DataDoc.Save (text);
                        string TextString = text.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        
                        AssertEquals ("#B01", "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B02", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B04", "    <RegionID>1</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B05", "    <RegionDescription>Reeeeeaalllly Far East!</RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B06", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B07", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B08", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B09", "    <RegionDescription>Western", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B10", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B11", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B12", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B13", "    <RegionID>3</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B14", "    <RegionDescription>Northern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B15", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B16", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B17", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B18", "    <RegionID>4</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B19", "    <RegionDescription>Southern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B20", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B21", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B22", "  <MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B23", "    <Column1>12</Column1>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B24", "    <Column2>Hi There</Column2>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B25", "  </MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B26", "  <MoreData>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B27", "    <Column1>12</Column1>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#B28", "    <Column2>Hi There</Column2>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
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
                        string substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);

                        AssertEquals ("#C01", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C02", "  <Region />", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C03", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C04", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C05", "    <RegionDescription>Western", substring);
                        
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C06", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C07", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C08", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C09", "    <RegionID>3</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C10", "    <RegionDescription>Northern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C11", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C12", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C13", "  <Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C14", "    <RegionID>4</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C15", "    <RegionDescription>Southern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C16", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C17", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.Length);
                        //TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("#C18", "</Root>", substring);

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
			Console.WriteLine ("rootti " + root.FirstChild);
        		DataRow row = doc.GetRowFromElement((XmlElement)root.FirstChild);
			
                	AssertEquals ("#E01", "1", row [0]);

                	row = doc.GetRowFromElement((XmlElement)root.ChildNodes [2]);
                	AssertEquals ("#E02", "3", row [0]);
			
                }
        }
}
