// MonoTests.System.Data.DataSetTest.cs
//
// Author:
//   Ville Palo <vi64pa@koti.soon.fi>
//
// (C) Copyright 2002 Ville Palo
//

using NUnit.Framework;
using System;
using System.Xml;
using System.IO;
using System.Data;
using System.Globalization;
using System.Threading;

namespace MonoTests.System.Data
{

        public class DataSetTest : TestCase 
        {
        
                public DataSetTest() : base ("MonoTests.System.Data.DataSetTest") {}
                public DataSetTest(string name) : base(name) {}

                protected override void SetUp()
                {
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("fi-FI");
                }

                protected override void TearDown() {}

                public static ITest Suite 
                {
                        get 
                        { 
                                return new TestSuite(typeof(DataSetTest)); 
                        }
                }


		public void TestReadWriteXml ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml ("System.Data/region.xml");
			TextWriter writer = new StringWriter ();
			ds.WriteXml (writer);
		
			string TextString = writer.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#01", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#02", "  <Region>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#03", "    <RegionID>1</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#04", "    <RegionDescription>Eastern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#05", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#06", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#07", "  <Region>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#08", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#09", "    <RegionDescription>Western", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#10", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#11", "  </Region>", substring);

                        AssertEquals ("test#11", "</Root>", TextString);
		}

		public void TestReadWriteXmlDiffGram ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml ("System.Data/region.xml", XmlReadMode.DiffGram);
			TextWriter writer = new StringWriter ();
			ds.WriteXml (writer);
		
			string TextString = writer.ToString ();
                        AssertEquals ("test#01", "<NewDataSet />", TextString);

			ds.WriteXml (writer, XmlWriteMode.DiffGram);
			TextString = writer.ToString ();
			
			AssertEquals ("test#02", "<NewDataSet /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\" />", TextString);

			
			ds = new DataSet ();
			ds.ReadXml ("System.Data/region.xml");
			DataTable table = ds.Tables ["Region"];
			table.Rows [0] [0] = "64";
			ds.ReadXml ("System.Data/region.xml", XmlReadMode.DiffGram);
			ds.WriteXml (writer, XmlWriteMode.DiffGram);
			
			TextString = writer.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#03", "<NewDataSet /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\" /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\">",substring);

                      	substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#04", "  <Root>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#05", "    <Region diffgr:id=\"Region1\" msdata:rowOrder=\"0\" diffgr:hasChanges=\"inserted\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#06", "      <RegionID>64</RegionID>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "      <RegionDescription>Eastern", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "   </RegionDescription>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#08", "    </Region>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#09", "    <Region diffgr:id=\"Region2\" msdata:rowOrder=\"1\" diffgr:hasChanges=\"inserted\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#10", "      <RegionID>2</RegionID>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#11", "      <RegionDescription>Western", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#12", "   </RegionDescription>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#13", "    </Region>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#14", "  </Root>", substring);
			
			AssertEquals ("test#15", "</diffgr:diffgram>", TextString);
		}

		public void TestWriteXmlSchema ()
		{
			DataSet ds = new DataSet ();			
			ds.ReadXml ("System.Data/region.xml");
			TextWriter writer = new StringWriter ();
			ds.WriteXmlSchema (writer);
		
			string TextString = writer.ToString ();
		        
		        string substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#02", "<xs:schema id=\"Root\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#03", "  <xs:element name=\"Root\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#04", "    <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#05", "      <xs:choice maxOccurs=\"unbounded\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#06", "        <xs:element name=\"Region\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "          <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#08", "            <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#09", "              <xs:element name=\"RegionID\" type=\"xs:string\" minOccurs=\"0\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#10", "              <xs:element name=\"RegionDescription\" type=\"xs:string\" minOccurs=\"0\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#11", "            </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#12", "          </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#13", "        </xs:element>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#14", "      </xs:choice>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#15", "    </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#16", "  </xs:element>", substring);

			AssertEquals ("test#17", "</xs:schema>", TextString);
		}
		
		public void TestReadWriteXmlSchemaIgnoreSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("System.Data/store.xsd");
			ds.ReadXml ("System.Data/region.xml", XmlReadMode.IgnoreSchema);
			TextWriter writer = new StringWriter ();
			
			ds.WriteXmlSchema (writer);
			string TextString = writer.ToString ();
			
		        string substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#02", "<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#03", "  <xs:complexType name=\"bookstoreType\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#04", "    <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#05", "      <xs:element name=\"book\" type=\"bookType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#06", "    </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "  </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#08", "  <xs:complexType name=\"bookType\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#09", "    <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#10", "      <xs:element name=\"title\" type=\"xs:string\" msdata:Ordinal=\"1\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#11", "      <xs:element name=\"price\" type=\"xs:decimal\" msdata:Ordinal=\"2\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#12", "      <xs:element name=\"author\" type=\"authorName\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#13", "    </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#14", "    <xs:attribute name=\"genre\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#15", "  </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#16", "  <xs:complexType name=\"authorName\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#17", "    <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#18", "      <xs:element name=\"first-name\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#19", "      <xs:element name=\"last-name\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#20", "    </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#21", "  </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#22", "  <xs:element name=\"bookstore\" type=\"bookstoreType\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#23", "  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#24", "    <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#25", "      <xs:choice maxOccurs=\"unbounded\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#26", "        <xs:element ref=\"bookstore\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#27", "      </xs:choice>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#28", "    </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#29", "  </xs:element>", substring);

			AssertEquals ("test#30", "</xs:schema>", TextString);
		}
		
		public void TestReadWriteXmlSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("System.Data/store.xsd");
			ds.ReadXml ("System.Data/region.xml", XmlReadMode.InferSchema);
			TextWriter writer = new StringWriter ();
			ds.WriteXmlSchema (writer);
		
			string TextString = writer.ToString ();
		        
		        string substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#02", "<xs:schema id=\"Root\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#03", "  <xs:complexType name=\"bookstoreType\">", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#04", "    <xs:sequence>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#05", "      <xs:element name=\"book\" type=\"bookType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#06", "    </xs:sequence>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "  </xs:complexType>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#08", "  <xs:complexType name=\"bookType\">", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#09", "    <xs:sequence>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#10", "      <xs:element name=\"title\" type=\"xs:string\" msdata:Ordinal=\"1\" />", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#11", "      <xs:element name=\"price\" type=\"xs:decimal\" msdata:Ordinal=\"2\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#12", "      <xs:element name=\"author\" type=\"authorName\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#13", "    </xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#14", "    <xs:attribute name=\"genre\" type=\"xs:string\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#15", "  </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#16", "  <xs:complexType name=\"authorName\">", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#17", "    <xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#18", "      <xs:element name=\"first-name\" type=\"xs:string\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#19", "      <xs:element name=\"last-name\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#20", "    </xs:sequence>", substring);
		
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#21", "  </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#22", "  <xs:element name=\"bookstore\" type=\"bookstoreType\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#23", "  <xs:element name=\"Root\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#24", "    <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#25", "      <xs:choice maxOccurs=\"unbounded\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#26", "        <xs:element ref=\"bookstore\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#27", "        <xs:element name=\"Region\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#28", "          <xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#29", "            <xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#30", "              <xs:element name=\"RegionID\" type=\"xs:string\" minOccurs=\"0\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#31", "              <xs:element name=\"RegionDescription\" type=\"xs:string\" minOccurs=\"0\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#32", "            </xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#33", "          </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#34", "        </xs:element>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#35", "      </xs:choice>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#36", "    </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n") - 1);
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#37", "  </xs:element>", substring);

			AssertEquals ("test#38", "</xs:schema>", TextString);
		}
        }
}
