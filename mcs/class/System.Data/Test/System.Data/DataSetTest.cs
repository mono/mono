// MonoTests.System.Data.DataSetTest.cs
//
// Authors:
//   Ville Palo <vi64pa@koti.soon.fi>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) Copyright 2002 Ville Palo
// (C) Copyright 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Xml;
using System.IO;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Threading;

namespace MonoTests.System.Data
{
	[TestFixture]
        public class DataSetTest : Assertion
        {
        
		[SetUp]
                public void GetReady()
                {
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("fi-FI");
                }

		[Test]
		public void ReadXmlSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("System.Data/own_schema.xsd");
			
			AssertEquals ("test#01", 2, ds.Tables.Count);
			DataTable Table = ds.Tables [0];
			AssertEquals ("test#02", "test_table", Table.TableName);
			AssertEquals ("test#03", "", Table.Namespace);
			AssertEquals ("test#04", 2, Table.Columns.Count);
			AssertEquals ("test#05", 0, Table.Rows.Count);
			AssertEquals ("test#06", false, Table.CaseSensitive);
			AssertEquals ("test#07", 1, Table.Constraints.Count);
			AssertEquals ("test#08", "", Table.Prefix);
			
			Constraint cons = Table.Constraints [0];
			AssertEquals ("test#09", "Constraint1", cons.ConstraintName.ToString ());
			AssertEquals ("test#10", "Constraint1", cons.ToString ());
			
			DataColumn column = Table.Columns [0];
			AssertEquals ("test#11", true, column.AllowDBNull);
			AssertEquals ("test#12", false, column.AutoIncrement);
			AssertEquals ("test#13", 0L, column.AutoIncrementSeed);
			AssertEquals ("test#14", 1L, column.AutoIncrementStep);
			AssertEquals ("test#15", "test", column.Caption);
			AssertEquals ("test#16", "Element", column.ColumnMapping.ToString ());
			AssertEquals ("test#17", "first", column.ColumnName);
			AssertEquals ("test#18", "System.String", column.DataType.ToString ());
			AssertEquals ("test#19", "test_default_value", column.DefaultValue.ToString ());
			AssertEquals ("test#20", false, column.DesignMode);
			AssertEquals ("test#21", "", column.Expression);
			AssertEquals ("test#22", 100, column.MaxLength);
			AssertEquals ("test#23", "", column.Namespace);
			AssertEquals ("test#24", 0, column.Ordinal);
			AssertEquals ("test#25", "", column.Prefix);
			AssertEquals ("test#26", false, column.ReadOnly);
			AssertEquals ("test#27", true, column.Unique);
						
			DataColumn column2 = Table.Columns [1];
			AssertEquals ("test#28", true, column2.AllowDBNull);
			AssertEquals ("test#29", false, column2.AutoIncrement);
			AssertEquals ("test#30", 0L, column2.AutoIncrementSeed);
			AssertEquals ("test#31", 1L, column2.AutoIncrementStep);
			AssertEquals ("test#32", "second", column2.Caption);
			AssertEquals ("test#33", "Element", column2.ColumnMapping.ToString ());
			AssertEquals ("test#34", "second", column2.ColumnName);
			AssertEquals ("test#35", "System.Data.SqlTypes.SqlGuid", column2.DataType.ToString ());
			AssertEquals ("test#36", "", column2.DefaultValue.ToString ());
			AssertEquals ("test#37", false, column2.DesignMode);
			AssertEquals ("test#38", "", column2.Expression);
			AssertEquals ("test#39", -1, column2.MaxLength);
			AssertEquals ("test#40", "", column2.Namespace);
			AssertEquals ("test#41", 1, column2.Ordinal);
			AssertEquals ("test#42", "", column2.Prefix);
			AssertEquals ("test#43", false, column2.ReadOnly);
			AssertEquals ("test#44", false, column2.Unique);
			
			DataTable Table2 = ds.Tables [1];
			AssertEquals ("test#45", "second_test_table", Table2.TableName);
			AssertEquals ("test#46", "", Table2.Namespace);
			AssertEquals ("test#47", 1, Table2.Columns.Count);
			AssertEquals ("test#48", 0, Table2.Rows.Count);
			AssertEquals ("test#49", false, Table2.CaseSensitive);
			AssertEquals ("test#50", 1, Table2.Constraints.Count);
			AssertEquals ("test#51", "", Table2.Prefix);
			
			DataColumn column3 = Table2.Columns [0];
			AssertEquals ("test#52", true, column3.AllowDBNull);
			AssertEquals ("test#53", false, column3.AutoIncrement);
			AssertEquals ("test#54", 0L, column3.AutoIncrementSeed);
			AssertEquals ("test#55", 1L, column3.AutoIncrementStep);
			AssertEquals ("test#56", "second_first", column3.Caption);
			AssertEquals ("test#57", "Element", column3.ColumnMapping.ToString ());
			AssertEquals ("test#58", "second_first", column3.ColumnName);
			AssertEquals ("test#59", "System.String", column3.DataType.ToString ());
			AssertEquals ("test#60", "default_value", column3.DefaultValue.ToString ());
			AssertEquals ("test#61", false, column3.DesignMode);
			AssertEquals ("test#62", "", column3.Expression);
			AssertEquals ("test#63", 100, column3.MaxLength);
			AssertEquals ("test#64", "", column3.Namespace);
			AssertEquals ("test#65", 0, column3.Ordinal);
			AssertEquals ("test#66", "", column3.Prefix);
			AssertEquals ("test#67", false, column3.ReadOnly);
			AssertEquals ("test#68", true, column3.Unique);
		}

		[Test]
		public void OwnWriteXmlSchema ()
		{
			DataSet ds = new DataSet ("test_dataset");
			DataTable table = new DataTable ("test_table");
			DataColumn column = new DataColumn ("first", typeof (string));
			column.AllowDBNull = true;
			column.DefaultValue = "test_default_value";			
			column.MaxLength = 100;
			column.Caption = "test";
			column.Unique = true;
			table.Columns.Add (column);

			DataColumn column2 = new DataColumn ("second", typeof (SqlGuid));
			column2.ColumnMapping = MappingType.Element;
			table.Columns.Add (column2);
			ds.Tables.Add (table);
			
			DataTable table2 = new DataTable ("second_test_table");
			DataColumn column3 = new DataColumn ("second_first", typeof (string));
			column3.AllowDBNull = true;
			column3.DefaultValue = "default_value";
			column3.MaxLength = 100;
			column3.Unique = true;
			table2.Columns.Add (column3);
			ds.Tables.Add (table2);

			TextWriter writer = new StringWriter ();
			ds.WriteXmlSchema (writer);
		
			string TextString = writer.ToString ();
			string substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#02", "<xs:schema id=\"test_dataset\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#03", "  <xs:element name=\"test_dataset\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#04", "    <xs:complexType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#05", "      <xs:choice maxOccurs=\"unbounded\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#06", "        <xs:element name=\"test_table\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "          <xs:complexType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#08", "            <xs:sequence>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#09", "              <xs:element name=\"first\" msdata:Caption=\"test\" default=\"test_default_value\" minOccurs=\"0\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#10", "                <xs:simpleType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#11", "                  <xs:restriction base=\"xs:string\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#12", "                    <xs:maxLength value=\"100\" />", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#13", "                  </xs:restriction>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#14", "                </xs:simpleType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#15", "              </xs:element>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#16", "              <xs:element name=\"second\" msdata:DataType=\"System.Data.SqlTypes.SqlGuid, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" type=\"xs:string\" minOccurs=\"0\" />", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#17", "            </xs:sequence>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#18", "          </xs:complexType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#19", "        </xs:element>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#20", "        <xs:element name=\"second_test_table\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#21", "          <xs:complexType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#22", "            <xs:sequence>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#23", "              <xs:element name=\"second_first\" default=\"default_value\" minOccurs=\"0\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#24", "                <xs:simpleType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#25", "                  <xs:restriction base=\"xs:string\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#26", "                    <xs:maxLength value=\"100\" />", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#27", "                  </xs:restriction>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#28", "                </xs:simpleType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#29", "              </xs:element>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#30", "            </xs:sequence>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#31", "          </xs:complexType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#32", "        </xs:element>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#33", "      </xs:choice>", substring);

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#34", "    </xs:complexType>", substring);

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#36", "    <xs:unique name=\"Constraint1\">", substring);

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#37", "      <xs:selector xpath=\".//test_table\" />", substring);

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#38", "      <xs:field xpath=\"first\" />", substring);

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#39", "    </xs:unique>", substring);

			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#40", "    <xs:unique name=\"second_test_table_Constraint1\" msdata:ConstraintName=\"Constraint1\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#41", "      <xs:selector xpath=\".//second_test_table\" />", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#42", "      <xs:field xpath=\"second_first\" />", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#43", "    </xs:unique>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf("\n"));
			TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#44", "  </xs:element>", substring);			
			AssertEquals ("test#45", "</xs:schema>", TextString);
		}
		
		[Test]
		public void ReadWriteXml ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml ("System.Data/region.xml");
			TextWriter writer = new StringWriter ();
			ds.WriteXml (writer);
		
			string TextString = writer.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#01", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#02", "  <Region>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#03", "    <RegionID>1</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#04", "    <RegionDescription>Eastern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#05", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#06", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#07", "  <Region>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#08", "    <RegionID>2</RegionID>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#09", "    <RegionDescription>Western", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#10", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
                        AssertEquals ("test#11", "  </Region>", substring);

                        AssertEquals ("test#11", "</Root>", TextString);
		}

		[Test]
		public void ReadWriteXmlDiffGram ()
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
                        string substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#03", "<NewDataSet /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\" /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\">",substring);

                      	substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#04", "  <Root>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#05", "    <Region diffgr:id=\"Region1\" msdata:rowOrder=\"0\" diffgr:hasChanges=\"inserted\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#06", "      <RegionID>64</RegionID>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "      <RegionDescription>Eastern", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "   </RegionDescription>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#08", "    </Region>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#09", "    <Region diffgr:id=\"Region2\" msdata:rowOrder=\"1\" diffgr:hasChanges=\"inserted\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#10", "      <RegionID>2</RegionID>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#11", "      <RegionDescription>Western", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#12", "   </RegionDescription>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#13", "    </Region>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#14", "  </Root>", substring);
			
			AssertEquals ("test#15", "</diffgr:diffgram>", TextString);
		}

		[Test]
		public void WriteXmlSchema ()
		{
			DataSet ds = new DataSet ();			
			ds.ReadXml ("System.Data/region.xml");
			TextWriter writer = new StringWriter ();
			ds.WriteXmlSchema (writer);
		
			string TextString = writer.ToString ();
		        
		        string substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#02", "<xs:schema id=\"Root\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#03", "  <xs:element name=\"Root\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#04", "    <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#05", "      <xs:choice maxOccurs=\"unbounded\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#06", "        <xs:element name=\"Region\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "          <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#08", "            <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#09", "              <xs:element name=\"RegionID\" type=\"xs:string\" minOccurs=\"0\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#10", "              <xs:element name=\"RegionDescription\" type=\"xs:string\" minOccurs=\"0\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#11", "            </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#12", "          </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#13", "        </xs:element>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#14", "      </xs:choice>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#15", "    </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#16", "  </xs:element>", substring);

			AssertEquals ("test#17", "</xs:schema>", TextString);
		}
		
		[Test]
		public void ReadWriteXmlSchemaIgnoreSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("System.Data/store.xsd");
			ds.ReadXml ("System.Data/region.xml", XmlReadMode.IgnoreSchema);
			TextWriter writer = new StringWriter ();
			
			ds.WriteXmlSchema (writer);
			string TextString = writer.ToString ();
			
		        string substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#02", "<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#03", "  <xs:complexType name=\"bookstoreType\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#04", "    <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#05", "      <xs:element name=\"book\" type=\"bookType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#06", "    </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "  </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#08", "  <xs:complexType name=\"bookType\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#09", "    <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#10", "      <xs:element name=\"title\" type=\"xs:string\" msdata:Ordinal=\"1\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#11", "      <xs:element name=\"price\" type=\"xs:decimal\" msdata:Ordinal=\"2\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#12", "      <xs:element name=\"author\" type=\"authorName\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#13", "    </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#14", "    <xs:attribute name=\"genre\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#15", "  </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#16", "  <xs:complexType name=\"authorName\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#17", "    <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#18", "      <xs:element name=\"first-name\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#19", "      <xs:element name=\"last-name\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#20", "    </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#21", "  </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#22", "  <xs:element name=\"bookstore\" type=\"bookstoreType\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#23", "  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#24", "    <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#25", "      <xs:choice maxOccurs=\"unbounded\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#26", "        <xs:element ref=\"bookstore\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#27", "      </xs:choice>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#28", "    </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#29", "  </xs:element>", substring);

			AssertEquals ("test#30", "</xs:schema>", TextString);
		}
		
		[Test]
		public void ReadWriteXmlSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("System.Data/store.xsd");
			ds.ReadXml ("System.Data/region.xml", XmlReadMode.InferSchema);
			TextWriter writer = new StringWriter ();
			ds.WriteXmlSchema (writer);
		
			string TextString = writer.ToString ();
		        
		        string substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#02", "<xs:schema id=\"Root\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#03", "  <xs:complexType name=\"bookstoreType\">", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#04", "    <xs:sequence>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#05", "      <xs:element name=\"book\" type=\"bookType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#06", "    </xs:sequence>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#07", "  </xs:complexType>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#08", "  <xs:complexType name=\"bookType\">", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#09", "    <xs:sequence>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#10", "      <xs:element name=\"title\" type=\"xs:string\" msdata:Ordinal=\"1\" />", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#11", "      <xs:element name=\"price\" type=\"xs:decimal\" msdata:Ordinal=\"2\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#12", "      <xs:element name=\"author\" type=\"authorName\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#13", "    </xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#14", "    <xs:attribute name=\"genre\" type=\"xs:string\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#15", "  </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#16", "  <xs:complexType name=\"authorName\">", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#17", "    <xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#18", "      <xs:element name=\"first-name\" type=\"xs:string\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#19", "      <xs:element name=\"last-name\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#20", "    </xs:sequence>", substring);
		
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#21", "  </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#22", "  <xs:element name=\"bookstore\" type=\"bookstoreType\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#23", "  <xs:element name=\"Root\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#24", "    <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#25", "      <xs:choice maxOccurs=\"unbounded\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#26", "        <xs:element ref=\"bookstore\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#27", "        <xs:element name=\"Region\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#28", "          <xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#29", "            <xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#30", "              <xs:element name=\"RegionID\" type=\"xs:string\" minOccurs=\"0\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#31", "              <xs:element name=\"RegionDescription\" type=\"xs:string\" minOccurs=\"0\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#32", "            </xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#33", "          </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#34", "        </xs:element>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#35", "      </xs:choice>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#36", "    </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf("\n"));
                        TextString = TextString.Substring (TextString.IndexOf("\n") + 1);
			AssertEquals ("test#37", "  </xs:element>", substring);

			AssertEquals ("test#38", "</xs:schema>", TextString);
		}
        }
}
