// MonoTests.System.Data.DataSetTest.cs
//
// Authors:
//   Ville Palo <vi64pa@koti.soon.fi>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) Copyright 2002 Ville Palo
// (C) Copyright 2003 Martin Willemoes Hansen

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
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Threading;
using System.Text;

namespace MonoTests.System.Data
{
	[TestFixture]
        public class DataSetTest : DataSetAssertion
        {
        	string EOL = Environment.NewLine;

		[SetUp]
                public void GetReady()
                {
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("fi-FI");
                }

		[Test]
		public void Properties ()
		{
			DataSet ds = new DataSet ();
			AssertEquals ("default namespace", String.Empty, ds.Namespace);
			ds.Namespace = null; // setting null == setting ""
			AssertEquals ("after setting null to namespace", String.Empty, ds.Namespace);

			AssertEquals ("default prefix", String.Empty, ds.Prefix);
			ds.Prefix = null; // setting null == setting ""
			AssertEquals ("after setting null to prefix", String.Empty, ds.Prefix);
		}

		[Test]
		public void ReadXmlSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("Test/System.Data/own_schema.xsd");
			
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

			string TextString = GetNormalizedSchema (writer.ToString ());
//			string TextString = writer.ToString ();

			string substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#02", "<xs:schema id=\"test_dataset\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);
			AssertEquals ("test#02", "<xs:schema id=\"test_dataset\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#03", "  <xs:element name=\"test_dataset\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);
			AssertEquals ("test#03", "  <xs:element msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\" name=\"test_dataset\">", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#04", "    <xs:complexType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#05", "      <xs:choice maxOccurs=\"unbounded\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#06", "        <xs:element name=\"test_table\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#07", "          <xs:complexType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#08", "            <xs:sequence>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#09", "              <xs:element name=\"first\" msdata:Caption=\"test\" default=\"test_default_value\" minOccurs=\"0\">", substring);
			AssertEquals ("test#09", "              <xs:element default=\"test_default_value\" minOccurs=\"0\" msdata:Caption=\"test\" name=\"first\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#10", "                <xs:simpleType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#11", "                  <xs:restriction base=\"xs:string\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#12", "                    <xs:maxLength value=\"100\" />", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#13", "                  </xs:restriction>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#14", "                </xs:simpleType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#15", "              </xs:element>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#16", "              <xs:element name=\"second\" msdata:DataType=\"System.Data.SqlTypes.SqlGuid, System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" type=\"xs:string\" minOccurs=\"0\" />", substring);
#if NET_2_0
			AssertEquals ("test#16", "              <xs:element minOccurs=\"0\" msdata:DataType=\"System.Data.SqlTypes.SqlGuid, System.Data, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" name=\"second\" type=\"xs:string\" />", substring);
#else
			AssertEquals ("test#16", "              <xs:element minOccurs=\"0\" msdata:DataType=\"System.Data.SqlTypes.SqlGuid, System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" name=\"second\" type=\"xs:string\" />", substring);
#endif
			if (substring.IndexOf ("<xs:element") < 0)
				Fail ("test#16: " + substring);
			if (substring.IndexOf ("name=\"second\"") < 0)
				Fail ("test#16: " + substring);
			if (substring.IndexOf ("msdata:DataType=\"System.Data.SqlTypes.SqlGuid, System.Data, Version=") < 0)
				Fail ("test#16: " + substring);
			if (substring.IndexOf ("type=\"xs:string\"") < 0)
				Fail ("test#16: " + substring);
			if (substring.IndexOf ("minOccurs=\"0\"") < 0)
				Fail ("test#16: " + substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#17", "            </xs:sequence>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#18", "          </xs:complexType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#19", "        </xs:element>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#20", "        <xs:element name=\"second_test_table\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#21", "          <xs:complexType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#22", "            <xs:sequence>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#23", "              <xs:element name=\"second_first\" default=\"default_value\" minOccurs=\"0\">", substring);
			AssertEquals ("test#23", "              <xs:element default=\"default_value\" minOccurs=\"0\" name=\"second_first\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#24", "                <xs:simpleType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#25", "                  <xs:restriction base=\"xs:string\">", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#26", "                    <xs:maxLength value=\"100\" />", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#27", "                  </xs:restriction>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#28", "                </xs:simpleType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#29", "              </xs:element>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#30", "            </xs:sequence>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#31", "          </xs:complexType>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#32", "        </xs:element>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#33", "      </xs:choice>", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#34", "    </xs:complexType>", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#36", "    <xs:unique name=\"Constraint1\">", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#37", "      <xs:selector xpath=\".//test_table\" />", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#38", "      <xs:field xpath=\"first\" />", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#39", "    </xs:unique>", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#40", "    <xs:unique name=\"second_test_table_Constraint1\" msdata:ConstraintName=\"Constraint1\">", substring);
			AssertEquals ("test#40", "    <xs:unique msdata:ConstraintName=\"Constraint1\" name=\"second_test_table_Constraint1\">", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#41", "      <xs:selector xpath=\".//second_test_table\" />", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#42", "      <xs:field xpath=\"second_first\" />", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#43", "    </xs:unique>", substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#44", "  </xs:element>", substring);			
			AssertEquals ("test#45", "</xs:schema>", TextString);
		}
		
		[Test]
		public void ReadWriteXml ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml ("Test/System.Data/region.xml");
			TextWriter writer = new StringWriter ();
			ds.WriteXml (writer);
		
			string TextString = writer.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("test#01", "<Root>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("test#02", "  <Region>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("test#03", "    <RegionID>1</RegionID>", substring);
			// Here the end of line is text markup "\n"
                        substring = TextString.Substring (0, TextString.IndexOf('\n'));
                        TextString = TextString.Substring (TextString.IndexOf('\n') + 1);
                        AssertEquals ("test#04", "    <RegionDescription>Eastern", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("test#05", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("test#06", "  </Region>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("test#07", "  <Region>", substring);
			
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("test#08", "    <RegionID>2</RegionID>", substring);

			// Here the end of line is text markup "\n"
                        substring = TextString.Substring (0, TextString.IndexOf('\n'));
                        TextString = TextString.Substring (TextString.IndexOf('\n') + 1);
                        AssertEquals ("test#09", "    <RegionDescription>Western", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("test#10", "   </RegionDescription>", substring);

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        AssertEquals ("test#11", "  </Region>", substring);

                        AssertEquals ("test#11", "</Root>", TextString);
		}

		[Test]
		public void ReadWriteXmlDiffGram ()
		{
			DataSet ds = new DataSet ();
			// It is not a diffgram, so no data loading should be done.
			ds.ReadXml ("Test/System.Data/region.xml", XmlReadMode.DiffGram);
			TextWriter writer = new StringWriter ();
			ds.WriteXml (writer);
		
			string TextString = writer.ToString ();
                        AssertEquals ("test#01", "<NewDataSet />", TextString);

			ds.WriteXml (writer, XmlWriteMode.DiffGram);
			TextString = writer.ToString ();
			
			AssertEquals ("test#02", "<NewDataSet /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\" />", TextString);

			
			ds = new DataSet ();
			ds.ReadXml ("Test/System.Data/region.xml");
			DataTable table = ds.Tables ["Region"];
			table.Rows [0] [0] = "64";
			ds.ReadXml ("Test/System.Data/region.xml", XmlReadMode.DiffGram);
			ds.WriteXml (writer, XmlWriteMode.DiffGram);
			
			TextString = writer.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#03", "<NewDataSet /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\" /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\">",substring);

                      	substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#04", "  <Root>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#05", "    <Region diffgr:id=\"Region1\" msdata:rowOrder=\"0\" diffgr:hasChanges=\"inserted\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#06", "      <RegionID>64</RegionID>", substring);

			// not EOL but literal '\n'
		        substring = TextString.Substring (0, TextString.IndexOf('\n'));
                        TextString = TextString.Substring (TextString.IndexOf('\n') + 1);
			AssertEquals ("test#07", "      <RegionDescription>Eastern", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#07", "   </RegionDescription>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#08", "    </Region>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#09", "    <Region diffgr:id=\"Region2\" msdata:rowOrder=\"1\" diffgr:hasChanges=\"inserted\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#10", "      <RegionID>2</RegionID>", substring);

			// not EOL but literal '\n'
		        substring = TextString.Substring (0, TextString.IndexOf('\n'));
                        TextString = TextString.Substring (TextString.IndexOf('\n') + 1);
			AssertEquals ("test#11", "      <RegionDescription>Western", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#12", "   </RegionDescription>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#13", "    </Region>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#14", "  </Root>", substring);
			
			AssertEquals ("test#15", "</diffgr:diffgram>", TextString);
		}

		[Test]
		public void WriteXmlSchema ()
		{
			DataSet ds = new DataSet ();			
			ds.ReadXml ("Test/System.Data/region.xml");
			TextWriter writer = new StringWriter ();
			ds.WriteXmlSchema (writer);
		

			string TextString = GetNormalizedSchema (writer.ToString ());
//			string TextString = writer.ToString ();
		        
		        string substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#02", "<xs:schema id=\"Root\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);
			AssertEquals ("test#02", "<xs:schema id=\"Root\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#03", "  <xs:element name=\"Root\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);
			AssertEquals ("test#03", "  <xs:element msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\" name=\"Root\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#04", "    <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#05", "      <xs:choice maxOccurs=\"unbounded\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#06", "        <xs:element name=\"Region\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#07", "          <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#08", "            <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#09", "              <xs:element name=\"RegionID\" type=\"xs:string\" minOccurs=\"0\" />", substring);
			AssertEquals ("test#09", "              <xs:element minOccurs=\"0\" name=\"RegionID\" type=\"xs:string\" />", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#10", "              <xs:element name=\"RegionDescription\" type=\"xs:string\" minOccurs=\"0\" />", substring);
			AssertEquals ("test#10", "              <xs:element minOccurs=\"0\" name=\"RegionDescription\" type=\"xs:string\" />", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#11", "            </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#12", "          </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#13", "        </xs:element>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#14", "      </xs:choice>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#15", "    </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#16", "  </xs:element>", substring);

			AssertEquals ("test#17", "</xs:schema>", TextString);
		}
		
		[Test]
		[Ignore ("MS behavior is far from consistent to be regarded as a reference implementation.")]
		// MS ReadXmlSchema() is too inconsistent to regard as a 
		// reference implementation. To find the reason why, try to
		// read store2.xsd and store4.xsd, write and compare for each
		// DataSet property.
		public void ReadWriteXmlSchemaIgnoreSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("Test/System.Data/store.xsd");
			AssertDataSet ("read DataSet", ds, "NewDataSet", 3, 2);
			AssertDataTable ("read bookstore table", ds.Tables [0], "bookstore", 1, 0, 0, 1, 1, 1);
			AssertDataTable ("read book table", ds.Tables [1], "book", 5, 0, 1, 1, 2, 1);
			ds.ReadXml ("Test/System.Data/region.xml", XmlReadMode.IgnoreSchema);
			TextWriter writer = new StringWriter ();
			
			ds.WriteXmlSchema (writer);
			string TextString = GetNormalizedSchema (writer.ToString ());
//			string TextString = writer.ToString ();
			

		        string substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#02", "<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);
			AssertEquals ("test#02", "<xs:schema id=\"NewDataSet\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">", substring);

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#03", "  <xs:complexType name=\"bookstoreType\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#04", "    <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#05", "      <xs:element name=\"book\" type=\"bookType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);
			AssertEquals ("test#05", "      <xs:element maxOccurs=\"unbounded\" minOccurs=\"0\" name=\"book\" type=\"bookType\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#06", "    </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#07", "  </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#08", "  <xs:complexType name=\"bookType\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#09", "    <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#10", "      <xs:element name=\"title\" type=\"xs:string\" msdata:Ordinal=\"1\" />", substring);
			
			AssertEquals ("test#10", "      <xs:element msdata:Ordinal=\"1\" name=\"title\" type=\"xs:string\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#11", "      <xs:element name=\"price\" type=\"xs:decimal\" msdata:Ordinal=\"2\" />", substring);
			AssertEquals ("test#11", "      <xs:element msdata:Ordinal=\"2\" name=\"price\" type=\"xs:decimal\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#12", "      <xs:element name=\"author\" type=\"authorName\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);
			AssertEquals ("test#12", "      <xs:element maxOccurs=\"unbounded\" minOccurs=\"0\" name=\"author\" type=\"authorName\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#13", "    </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#14", "    <xs:attribute name=\"genre\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#15", "  </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#16", "  <xs:complexType name=\"authorName\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#17", "    <xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#18", "      <xs:element name=\"first-name\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#19", "      <xs:element name=\"last-name\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#20", "    </xs:sequence>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#21", "  </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#22", "  <xs:element name=\"bookstore\" type=\"bookstoreType\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#23", "  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);
			AssertEquals ("test#23", "  <xs:element msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\" name=\"NewDataSet\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#24", "    <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#25", "      <xs:choice maxOccurs=\"unbounded\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#26", "        <xs:element ref=\"bookstore\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#27", "      </xs:choice>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#28", "    </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#29", "  </xs:element>", substring);

			AssertEquals ("test#30", "</xs:schema>", TextString);
		}
		
		[Test]
		[Ignore ("MS behavior is far from consistent to be regarded as a reference implementation.")]
		// See comments on ReadWriteXmlSchemaIgnoreSchema().
		public void ReadWriteXmlSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("Test/System.Data/store.xsd");
			// check dataset properties before testing write
			AssertDataSet ("ds", ds, "NewDataSet", 3, 2);
			AssertDataTable ("tab1", ds.Tables [0], "bookstore", 1, 0, 0, 1, 1, 1);
			AssertDataTable ("tab2", ds.Tables [1], "book", 5, 0, 1, 1, 2, 1);
			AssertDataTable ("tab3", ds.Tables [2], "author", 3, 0, 1, 0, 1, 0);
			// FIXME: currently order is not compatible. Use name as index
			AssertDataRelation ("rel1", ds.Relations ["book_author"], "book_author", true, new string [] {"book_Id"}, new string [] {"book_Id"}, true, true);
			AssertDataRelation ("rel2", ds.Relations ["bookstore_book"], "bookstore_book", true, new string [] {"bookstore_Id"}, new string [] {"bookstore_Id"}, true, true);

			ds.ReadXml ("Test/System.Data/region.xml", XmlReadMode.InferSchema);

			TextWriter writer = new StringWriter ();
			ds.WriteXmlSchema (writer);
		
			string TextString = GetNormalizedSchema (writer.ToString ());
//			string TextString = writer.ToString ();
		        
		        string substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#01", "<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#02", "<xs:schema id=\"Root\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring);
			AssertEquals ("test#02", "<xs:schema id=\"Root\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#03", "  <xs:complexType name=\"bookstoreType\">", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#04", "    <xs:sequence>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#05", "      <xs:element name=\"book\" type=\"bookType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);
			AssertEquals ("test#05", "      <xs:element maxOccurs=\"unbounded\" minOccurs=\"0\" name=\"book\" type=\"bookType\" />", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#06", "    </xs:sequence>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#07", "  </xs:complexType>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#08", "  <xs:complexType name=\"bookType\">", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#09", "    <xs:sequence>", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#10", "      <xs:element name=\"title\" type=\"xs:string\" msdata:Ordinal=\"1\" />", substring);
			AssertEquals ("test#10", "      <xs:element msdata:Ordinal=\"1\" name=\"title\" type=\"xs:string\" />", substring);
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#11", "      <xs:element name=\"price\" type=\"xs:decimal\" msdata:Ordinal=\"2\" />", substring);
			AssertEquals ("test#11", "      <xs:element msdata:Ordinal=\"2\" name=\"price\" type=\"xs:decimal\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#12", "      <xs:element name=\"author\" type=\"authorName\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring);
			AssertEquals ("test#12", "      <xs:element maxOccurs=\"unbounded\" minOccurs=\"0\" name=\"author\" type=\"authorName\" />", substring);
	
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#13", "    </xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#14", "    <xs:attribute name=\"genre\" type=\"xs:string\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#15", "  </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#16", "  <xs:complexType name=\"authorName\">", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#17", "    <xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#18", "      <xs:element name=\"first-name\" type=\"xs:string\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#19", "      <xs:element name=\"last-name\" type=\"xs:string\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#20", "    </xs:sequence>", substring);
		
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#21", "  </xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#22", "  <xs:element name=\"bookstore\" type=\"bookstoreType\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#23", "  <xs:element name=\"Root\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring);
			AssertEquals ("test#23", "  <xs:element msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\" name=\"Root\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#24", "    <xs:complexType>", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#25", "      <xs:choice maxOccurs=\"unbounded\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#26", "        <xs:element ref=\"bookstore\" />", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#27", "        <xs:element name=\"Region\">", substring);

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#28", "          <xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#29", "            <xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#30", "              <xs:element name=\"RegionID\" type=\"xs:string\" minOccurs=\"0\" />", substring);
			AssertEquals ("test#30", "              <xs:element minOccurs=\"0\" name=\"RegionID\" type=\"xs:string\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			AssertEquals ("test#31", "              <xs:element name=\"RegionDescription\" type=\"xs:string\" minOccurs=\"0\" />", substring);
			AssertEquals ("test#31", "              <xs:element minOccurs=\"0\" name=\"RegionDescription\" type=\"xs:string\" />", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#32", "            </xs:sequence>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#33", "          </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#34", "        </xs:element>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#35", "      </xs:choice>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#36", "    </xs:complexType>", substring);
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			AssertEquals ("test#37", "  </xs:element>", substring);

			AssertEquals ("test#38", "</xs:schema>", TextString);
		}

		[Test]
		public void WriteDifferentNamespaceSchema ()
		{
			string schema = @"<?xml version='1.0' encoding='utf-16'?>
<xs:schema id='NewDataSet' targetNamespace='urn:bar' xmlns:mstns='urn:bar' xmlns='urn:bar' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' attributeFormDefault='qualified' elementFormDefault='qualified' xmlns:app1='urn:baz' xmlns:app2='urn:foo'>
  <!--ATTENTION: This schema contains references to other imported schemas-->
  <xs:import namespace='urn:baz' schemaLocation='_app1.xsd' />
  <xs:import namespace='urn:foo' schemaLocation='_app2.xsd' />
  <xs:element name='NewDataSet' msdata:IsDataSet='true' msdata:Locale='fi-FI'>
    <xs:complexType>
      <xs:choice maxOccurs='unbounded'>
        <xs:element ref='app2:NS1Table' />
        <xs:element name='NS2Table'>
          <xs:complexType>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet();
			DataTable dt = new DataTable ();
			dt.TableName = "NS1Table";
			dt.Namespace = "urn:foo";
			dt.Columns.Add ("column1");
			dt.Columns.Add ("column2");
			dt.Columns [1].Namespace = "urn:baz";
			ds.Tables.Add (dt);
			DataTable dt2 = new DataTable ();
			dt2.TableName = "NS2Table";
			dt2.Namespace = "urn:bar";
			ds.Tables.Add (dt2);
			ds.Namespace = "urn:bar";
			StringWriter sw = new StringWriter ();
			XmlTextWriter xw = new XmlTextWriter (sw);
			xw.Formatting = Formatting.Indented;
			xw.QuoteChar = '\'';
			ds.WriteXmlSchema (xw);

			string result = sw.ToString ();
			AssertEquals (schema, result.Replace ("\r\n", "\n"));
		}

		[Test]
		public void IgnoreColumnEmptyNamespace ()
		{
			DataColumn col = new DataColumn ("TEST");
			col.Namespace = "urn:foo";
			DataSet ds = new DataSet ("DS");
			ds.Namespace = "urn:foo";
			DataTable dt = new DataTable ("tab");
			ds.Tables.Add (dt);
			dt.Columns.Add (col);
			dt.Rows.Add (new object [] {"test"});
			StringWriter sw = new StringWriter ();
			ds.WriteXml (new XmlTextWriter (sw));
			string xml = @"<DS xmlns=""urn:foo""><tab><TEST>test</TEST></tab></DS>";
			AssertEquals (xml, sw.ToString ());
		}

		[Test]
		public void SerializeDataSet ()
		{
			// see GetReady() for current culture

			string xml = "<?xml version='1.0' encoding='utf-16'?><DataSet><xs:schema id='DS' xmlns='' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'><xs:element name='DS' msdata:IsDataSet='true' msdata:Locale='fi-FI'><xs:complexType><xs:choice maxOccurs='unbounded' /></xs:complexType></xs:element></xs:schema><diffgr:diffgram xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' xmlns:diffgr='urn:schemas-microsoft-com:xml-diffgram-v1' /></DataSet>";
			DataSet ds = new DataSet ();
			ds.DataSetName = "DS";
			XmlSerializer ser = new XmlSerializer (typeof (DataSet));
			StringWriter sw = new StringWriter ();
			XmlTextWriter xw = new XmlTextWriter (sw);
			xw.QuoteChar = '\'';
			ser.Serialize (xw, ds);

			string result = sw.ToString ();
			AssertEquals (xml, result.Replace ("\r\n", "\n"));
		}

		// bug #70961
		[Test]
		public void SerializeDataSet2 ()
		{
			DataSet quota = new DataSet ("Quota");

			// Dimension
			DataTable dt = new DataTable ("Dimension");
			quota.Tables.Add (dt);

			dt.Columns.Add ("Number", typeof(int));
			dt.Columns ["Number"].AllowDBNull = false;
			dt.Columns ["Number"].ColumnMapping = MappingType.Attribute;

			dt.Columns.Add ("Title", typeof(string));
			dt.Columns ["Title"].AllowDBNull = false;
			dt.Columns ["Title"].ColumnMapping = 
			MappingType.Attribute;

			dt.Rows.Add (new object [] {0, "Hospitals"});
			dt.Rows.Add (new object [] {1, "Doctors"});

			dt.Constraints.Add ("PK_Dimension", dt.Columns ["Number"], true);

			quota.AcceptChanges ();

			XmlSerializer ser = new XmlSerializer (quota.GetType ());

			StringWriter sw = new StringWriter ();
		        ser.Serialize (sw, quota);

			DataSet ds = (DataSet) ser.Deserialize (new StringReader (sw.ToString ()));
		}

		// bug #68007
		public void SerializeDataSet3 ()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?><DataSet><xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata""><xs:element name=""Example"" msdata:IsDataSet=""true""><xs:complexType><xs:choice maxOccurs=""unbounded""><xs:element name=""Packages""><xs:complexType><xs:attribute name=""ID"" type=""xs:int"" use=""required"" /><xs:attribute name=""ShipDate"" type=""xs:dateTime"" /><xs:attribute name=""Message"" type=""xs:string"" /><xs:attribute name=""Handlers"" type=""xs:int"" /></xs:complexType></xs:element></xs:choice></xs:complexType></xs:element></xs:schema><diffgr:diffgram xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:diffgr=""urn:schemas-microsoft-com:xml-diffgram-v1""><Example><Packages diffgr:id=""Packages1"" msdata:rowOrder=""0"" ID=""0"" ShipDate=""2004-10-11T17:46:18.6962302-05:00"" Message=""Received with no breakage!"" Handlers=""3"" /><Packages diffgr:id=""Packages2"" msdata:rowOrder=""1"" ID=""1"" /></Example></diffgr:diffgram></DataSet>";

			DataSet ds = new DataSet ("Example");

			// Add a DataTable
			DataTable dt = new DataTable ("Packages");
			ds.Tables.Add (dt);

			// Add an ID DataColumn w/ ColumnMapping = MappingType.Attribute
			dt.Columns.Add (new DataColumn ("ID", typeof(int), "", 
				MappingType.Attribute));
			dt.Columns ["ID"].AllowDBNull = false;

			// Add a nullable DataColumn w/ ColumnMapping = MappingType.Attribute
			dt.Columns.Add (new DataColumn ("ShipDate",
				typeof (DateTime), "", MappingType.Attribute));
			dt.Columns ["ShipDate"].AllowDBNull = true;

			// Add a nullable DataColumn w/ ColumnMapping = MappingType.Attribute
			dt.Columns.Add (new DataColumn ("Message",
				typeof (string), "", MappingType.Attribute));
			dt.Columns ["Message"].AllowDBNull = true;

			// Add a nullable DataColumn w/ ColumnMapping = MappingType.Attribute
			dt.Columns.Add (new DataColumn ("Handlers",
				typeof (int), "", MappingType.Attribute));
			dt.Columns ["Handlers"].AllowDBNull = true;

			// Add a non-null value row
			DataRow newRow = dt.NewRow();
			newRow ["ID"] = 0;
			newRow ["ShipDate"] = DateTime.Now;
			newRow ["Message"] = "Received with no breakage!";
			newRow ["Handlers"] = 3;
			dt.Rows.Add (newRow);

			// Add a null value row
			newRow = dt.NewRow ();
			newRow ["ID"] = 1;
			newRow ["ShipDate"] = DBNull.Value;
			newRow ["Message"] = DBNull.Value;
			newRow ["Handlers"] = DBNull.Value;
			dt.Rows.Add (newRow);

			ds.AcceptChanges ();

			XmlSerializer ser = new XmlSerializer (ds.GetType());
			StringWriter sw = new StringWriter ();
			ser.Serialize (sw, ds);

			string result = sw.ToString ();

			AssertEquals (xml, result);
		}

		[Test]
		public void DeserializeDataSet ()
		{
			string xml = @"<DataSet>
  <diffgr:diffgram xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' xmlns:diffgr='urn:schemas-microsoft-com:xml-diffgram-v1'>
    <Quota>
      <Dimension diffgr:id='Dimension1' msdata:rowOrder='0' Number='0' Title='Hospitals' />
      <Dimension diffgr:id='Dimension2' msdata:rowOrder='1' Number='1' Title='Doctors' />
    </Quota>
  </diffgr:diffgram>
</DataSet>";
			XmlSerializer ser = new XmlSerializer (typeof (DataSet));
			ser.Deserialize (new XmlTextReader (
				xml, XmlNodeType.Document, null));
		}

		/* To be added
		[Test]
		public void WriteDiffReadAutoWriteSchema ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add ("Table1");
			ds.Tables.Add ("Table2");
			ds.Tables [0].Columns.Add ("Column1_1");
			ds.Tables [0].Columns.Add ("Column1_2");
			ds.Tables [0].Columns.Add ("Column1_3");
			ds.Tables [1].Columns.Add ("Column2_1");
			ds.Tables [1].Columns.Add ("Column2_2");
			ds.Tables [1].Columns.Add ("Column2_3");
			ds.Tables [0].Rows.Add (new object [] {"ppp", "www", "xxx"});

			// save as diffgram
			StringWriter sw = new StringWriter ();
			ds.WriteXml (sw, XmlWriteMode.DiffGram);
			string xml = sw.ToString ();
			string result = new StreamReader ("Test/System.Data/DataSetReadXmlTest1.xml", Encoding.ASCII).ReadToEnd ();
			AssertEquals ("#01", result, xml);

			// load diffgram above
			ds.ReadXml (new StringReader (sw.ToString ()));
			sw = new StringWriter ();
			ds.WriteXml (sw, XmlWriteMode.WriteSchema);
			xml = sw.ToString ();
			result = new StreamReader ("Test/System.Data/DataSetReadXmlTest2.xml", Encoding.ASCII).ReadToEnd ();
			AssertEquals ("#02", result, xml);
		}
		*/

		[Test]
                public void CloneCopy ()
                {
                        DataTable table = new DataTable ("pTable");                         DataTable table1 = new DataTable ("cTable");                         DataSet set = new DataSet ();
                                                                                                    
                        set.Tables.Add (table);
                        set.Tables.Add (table1);                         DataColumn col = new DataColumn ();
                        col.ColumnName = "Id";
                        col.DataType = Type.GetType ("System.Int32");
                        table.Columns.Add (col);
                        UniqueConstraint uc = new UniqueConstraint ("UK1", table.Columns[0] );
                        table.Constraints.Add (uc);
                                                                                                    
                        col = new DataColumn ();
                        col.ColumnName = "Name";
                        col.DataType = Type.GetType ("System.String");
                        table.Columns.Add (col);
                                                                                                    
                        col = new DataColumn ();
                        col.ColumnName = "Id";
                        col.DataType = Type.GetType ("System.Int32");
                        table1.Columns.Add (col);
                                                                                                    
                        col = new DataColumn ();
                        col.ColumnName = "Name";
                        col.DataType = Type.GetType ("System.String");
		        table1.Columns.Add (col);
			  ForeignKeyConstraint fc = new ForeignKeyConstraint ("FK1", table.Columns[0], table1.Columns[0] );
                        table1.Constraints.Add (fc);
                                                                                                    
                                                                                                    
                        DataRow row = table.NewRow ();
                                                                                                    
                        row ["Id"] = 147;
                        row ["name"] = "Row1";
                        row.RowError = "Error#1";
                        table.Rows.Add (row);
                                                                                                    
                        row = table1.NewRow ();
                        row ["Id"] = 147;
                        row ["Name"] = "Row1";
                        table1.Rows.Add (row);
                                                                                                    
                        //Setting properties of DataSet
                        set.CaseSensitive = true;
                        set.DataSetName = "My DataSet";
                        set.EnforceConstraints = false;
                        set.Namespace = "Namespace#1";
                        set.Prefix = "Prefix:1";
                        DataRelation dr = new DataRelation ("DR", table.Columns [0],table1.Columns [0]);
                        set.Relations.Add (dr);
                        set.ExtendedProperties.Add ("TimeStamp", DateTime.Now);
                        CultureInfo cultureInfo = new CultureInfo( "ar-SA" );
                        set.Locale = cultureInfo;
                                                                                                    
                        //Testing Copy ()
                        DataSet copySet = set.Copy ();
                        AssertEquals ("#A01", set.CaseSensitive, copySet.CaseSensitive);
			AssertEquals ("#A02", set.DataSetName, copySet.DataSetName);
                        AssertEquals ("#A03", set.EnforceConstraints, copySet.EnforceConstraints);
                        AssertEquals ("#A04", set.HasErrors, copySet.HasErrors);
                        AssertEquals ("#A05", set.Namespace, copySet.Namespace);
                        AssertEquals ("#A06", set.Prefix, copySet.Prefix);
                        AssertEquals ("#A07", set.Relations.Count, copySet.Relations.Count);
                        AssertEquals ("#A08", set.Tables.Count, copySet.Tables.Count);
                        AssertEquals ("#A09", set.ExtendedProperties ["TimeStamp"], copySet.ExtendedProperties ["TimeStamp"]);
                        for (int i = 0;i < copySet.Tables.Count; i++) {
                                AssertEquals ("#A10", set.Tables [i].Rows.Count, copySet.Tables [i].Rows.Count);
                                AssertEquals ("#A11", set.Tables [i].Columns.Count, copySet.Tables [i].Columns.Count);
                        }
                        //Testing Clone ()
                        copySet = set.Clone ();
                        AssertEquals ("#A12", set.CaseSensitive, copySet.CaseSensitive);
                        AssertEquals ("#A13", set.DataSetName, copySet.DataSetName);
                        AssertEquals ("#A14", set.EnforceConstraints, copySet.EnforceConstraints);
                        AssertEquals ("#A15", false, copySet.HasErrors);
                        AssertEquals ("#A16", set.Namespace, copySet.Namespace);
                        AssertEquals ("#A17", set.Prefix, copySet.Prefix);
                        AssertEquals ("#A18", set.Relations.Count, copySet.Relations.Count);
                        AssertEquals ("#A19", set.Tables.Count, copySet.Tables.Count);
                        AssertEquals ("#A20", set.ExtendedProperties ["TimeStamp"], copySet.ExtendedProperties ["TimeStamp"]);
                        for (int i = 0;i < copySet.Tables.Count; i++) {
                                AssertEquals ("#A21", 0, copySet.Tables [i].Rows.Count);
                                AssertEquals ("#A22", set.Tables [i].Columns.Count, copySet.Tables [i].Columns.Count);
                        }
		}

		[Test]
		public void CloneCopy2 ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("Test/System.Data/store.xsd");
			ds.Clone ();
		}

		[Test]
		public void WriteNestedTableXml ()
		{
			string xml = @"<NewDataSet>
  <tab1>
    <ident>1</ident>
    <name>hoge</name>
    <tab2>
      <timestamp>2004-05-05</timestamp>
    </tab2>
  </tab1>
  <tab1>
    <ident>2</ident>
    <name>fuga</name>
    <tab2>
      <timestamp>2004-05-06</timestamp>
    </tab2>
  </tab1>
</NewDataSet>";
			DataSet ds = new DataSet ();
			DataTable dt = new DataTable ("tab1");
			dt.Columns.Add ("ident");
			dt.Columns.Add ("name");
			dt.Rows.Add (new object [] {"1", "hoge"});
			dt.Rows.Add (new object [] {"2", "fuga"});
			DataTable dt2 = new DataTable ("tab2");
			dt2.Columns.Add ("idref");
			dt2.Columns [0].ColumnMapping = MappingType.Hidden;
			dt2.Columns.Add ("timestamp");
			dt2.Rows.Add (new object [] {"1", "2004-05-05"});
			dt2.Rows.Add (new object [] {"2", "2004-05-06"});
			ds.Tables.Add (dt);
			ds.Tables.Add (dt2);
			DataRelation rel = new DataRelation ("rel", dt.Columns [0], dt2.Columns [0]);
			rel.Nested = true;
			ds.Relations.Add (rel);
			StringWriter sw = new StringWriter ();
			ds.WriteXml (sw);
			AssertEquals (xml, sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		public void WriteXmlToStream ()
		{
			string xml = "<set><table1><col1>sample text</col1><col2/></table1><table2 attr='value'><col3>sample text 2</col3></table2></set>";
			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (xml));
			MemoryStream ms = new MemoryStream ();
			ds.WriteXml (ms);
			MemoryStream ms2 = new MemoryStream (ms.ToArray ());
			StreamReader sr = new StreamReader (ms2, Encoding.UTF8);
			string result = @"<set>
  <table1>
    <col1>sample text</col1>
    <col2 />
  </table1>
  <table2 attr=""value"">
    <col3>sample text 2</col3>
  </table2>
</set>";
			AssertEquals (result, sr.ReadToEnd ().Replace ("\r\n", "\n"));
		}

		[Test]
		public void WtiteXmlEncodedXml ()
		{
			string xml = @"<an_x0020_example_x0020_dataset.>
  <WOW_x0021__x0020_that_x0027_s_x0020_nasty...>
    <URL_x0020_is_x0020_http_x003A__x002F__x002F_www.go-mono.com>content string.</URL_x0020_is_x0020_http_x003A__x002F__x002F_www.go-mono.com>
  </WOW_x0021__x0020_that_x0027_s_x0020_nasty...>
</an_x0020_example_x0020_dataset.>";
			DataSet ds = new DataSet ("an example dataset.");
			ds.Tables.Add (new DataTable ("WOW! that's nasty..."));
			ds.Tables [0].Columns.Add ("URL is http://www.go-mono.com");
			ds.Tables [0].Rows.Add (new object [] {"content string."});
			StringWriter sw = new StringWriter ();
			ds.WriteXml (sw);
			AssertEquals (xml, sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		public void ReadWriteXml2 ()
		{
			string xml = "<FullTextResponse><Domains><AvailResponse info='y' name='novell-ximian-group' /><AvailResponse info='n' name='ximian' /></Domains></FullTextResponse>";
			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (xml));
			AssertDataSet ("ds", ds, "FullTextResponse", 2, 1);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt1", dt, "Domains", 1, 1, 0, 1, 1, 1);
			dt = ds.Tables [1];
			AssertDataTable ("dt2", dt, "AvailResponse", 3, 2, 1, 0, 1, 0);
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			ds.WriteXml (xtw);
			AssertEquals (xml, sw.ToString ());
		}

		// bug #53959.
		[Test]
		public void ReadWriteXml3 ()
		{
			string input = @"<FullTextResponse>
  <Domains>
    <AvailResponse info='y' name='novell-ximian-group' />
    <AvailResponse info='n' name='ximian' />
  </Domains>
</FullTextResponse>";
			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (input));

			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.Formatting = Formatting.Indented;
			xtw.QuoteChar = '\'';
			ds.WriteXml (xtw);
			xtw.Flush ();
			AssertEquals (input, sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test] // bug #60469
		public void WriteXmlSchema2 ()
		{
			string xml = @"<myDataSet xmlns='NetFrameWork'><myTable><id>0</id><item>item 0</item></myTable><myTable><id>1</id><item>item 1</item></myTable><myTable><id>2</id><item>item 2</item></myTable><myTable><id>3</id><item>item 3</item></myTable><myTable><id>4</id><item>item 4</item></myTable><myTable><id>5</id><item>item 5</item></myTable><myTable><id>6</id><item>item 6</item></myTable><myTable><id>7</id><item>item 7</item></myTable><myTable><id>8</id><item>item 8</item></myTable><myTable><id>9</id><item>item 9</item></myTable></myDataSet>";
			string schema = @"<?xml version='1.0' encoding='utf-16'?>
<xs:schema id='myDataSet' targetNamespace='NetFrameWork' xmlns:mstns='NetFrameWork' xmlns='NetFrameWork' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' attributeFormDefault='qualified' elementFormDefault='qualified'>
  <xs:element name='myDataSet' msdata:IsDataSet='true' msdata:Locale='fi-FI'>
    <xs:complexType>
      <xs:choice maxOccurs='unbounded'>
        <xs:element name='myTable'>
          <xs:complexType>
            <xs:sequence>
              <xs:element name='id' msdata:AutoIncrement='true' type='xs:int' minOccurs='0' />
              <xs:element name='item' type='xs:string' minOccurs='0' />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			DataSet OriginalDataSet = new DataSet ("myDataSet"); 
			OriginalDataSet.Namespace= "NetFrameWork"; 
			DataTable myTable = new DataTable ("myTable"); 
			DataColumn c1 = new DataColumn ("id", typeof (int)); 
			c1.AutoIncrement = true;
			DataColumn c2 = new DataColumn ("item"); 
			myTable.Columns.Add (c1);
			myTable.Columns.Add (c2);
			OriginalDataSet.Tables.Add (myTable);
			// Add ten rows.
			DataRow newRow;
			for(int i = 0; i < 10; i++) {
				newRow = myTable.NewRow ();
				newRow ["item"] = "item " + i;
				myTable.Rows.Add (newRow);
			} 
			OriginalDataSet.AcceptChanges ();

			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			OriginalDataSet.WriteXml (xtw);
			string result = sw.ToString ();

			AssertEquals (xml, result);

			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
			xtw.Formatting = Formatting.Indented;
			OriginalDataSet.WriteXmlSchema (xtw);
			result = sw.ToString ();

			result = result.Replace ("\r\n", "\n").Replace ('"', '\'');
			AssertEquals (schema, result);
		}

		// bug #66366
		[Test]
		public void WriteXmlSchema3 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""ExampleDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""ExampleDataSet"" msdata:IsDataSet=""true"" msdata:Locale=""fi-FI"">
    <xs:complexType>
      <xs:choice maxOccurs=""unbounded"">
        <xs:element name=""ExampleDataTable"">
          <xs:complexType>
            <xs:attribute name=""PrimaryKeyColumn"" type=""xs:int"" use=""required"" />
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
    <xs:unique name=""PK_ExampleDataTable"" msdata:PrimaryKey=""true"">
      <xs:selector xpath="".//ExampleDataTable"" />
      <xs:field xpath=""@PrimaryKeyColumn"" />
    </xs:unique>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet ("ExampleDataSet");

			ds.Tables.Add (new DataTable ("ExampleDataTable"));
			ds.Tables ["ExampleDataTable"].Columns.Add (
				new DataColumn ("PrimaryKeyColumn", typeof(int), "", MappingType.Attribute));
			ds.Tables ["ExampleDataTable"].Columns ["PrimaryKeyColumn"].AllowDBNull = false;

			ds.Tables ["ExampleDataTable"].Constraints.Add (
				"PK_ExampleDataTable", 
				ds.Tables ["ExampleDataTable"].Columns ["PrimaryKeyColumn"],
				true);

			ds.AcceptChanges ();
			StringWriter sw = new StringWriter ();
			ds.WriteXmlSchema (sw);

			string result = sw.ToString ();

			AssertEquals (xmlschema, result.Replace ("\r\n", "\n"));
		}

		// bug #67792.
		[Test]
		public void WriteXmlSchema4 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:Locale=""fi-FI"">
    <xs:complexType>
      <xs:choice maxOccurs=""unbounded"">
        <xs:element name=""MyType"">
          <xs:complexType>
            <xs:attribute name=""ID"" type=""xs:int"" use=""required"" />
            <xs:attribute name=""Desc"" type=""xs:string"" />
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet ("Example");

			// Add MyType DataTable
			DataTable dt = new DataTable ("MyType");
			ds.Tables.Add (dt);

			dt.Columns.Add (new DataColumn ("ID", typeof(int), "",
				MappingType.Attribute));
			dt.Columns ["ID"].AllowDBNull = false;

			dt.Columns.Add (new DataColumn ("Desc", typeof
				(string), "", MappingType.Attribute));

			ds.AcceptChanges ();

			StringWriter sw = new StringWriter ();
			ds.WriteXmlSchema (sw);

			string result = sw.ToString ();

			AssertEquals (xmlschema, result.Replace ("\r\n", "\n"));
		}

		// bug # 68432
		[Test]
		public void WriteXmlSchema5 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:Locale=""fi-FI"">
    <xs:complexType>
      <xs:choice maxOccurs=""unbounded"">
        <xs:element name=""StandAlone"">
          <xs:complexType>
            <xs:attribute name=""ID"" type=""xs:int"" use=""required"" />
            <xs:attribute name=""Desc"" type=""xs:string"" use=""required"" />
          </xs:complexType>
        </xs:element>
        <xs:element name=""Dimension"">
          <xs:complexType>
            <xs:attribute name=""Number"" msdata:ReadOnly=""true"" type=""xs:int"" use=""required"" />
            <xs:attribute name=""Title"" type=""xs:string"" use=""required"" />
          </xs:complexType>
        </xs:element>
        <xs:element name=""Element"">
          <xs:complexType>
            <xs:attribute name=""Dimension"" msdata:ReadOnly=""true"" type=""xs:int"" use=""required"" />
            <xs:attribute name=""Number"" msdata:ReadOnly=""true"" type=""xs:int"" use=""required"" />
            <xs:attribute name=""Title"" type=""xs:string"" use=""required"" />
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
    <xs:unique name=""PK_Dimension"" msdata:PrimaryKey=""true"">
      <xs:selector xpath="".//Dimension"" />
      <xs:field xpath=""@Number"" />
    </xs:unique>
    <xs:unique name=""PK_Element"" msdata:PrimaryKey=""true"">
      <xs:selector xpath="".//Element"" />
      <xs:field xpath=""@Dimension"" />
      <xs:field xpath=""@Number"" />
    </xs:unique>
    <xs:keyref name=""FK_Element_To_Dimension"" refer=""PK_Dimension"">
      <xs:selector xpath="".//Element"" />
      <xs:field xpath=""@Dimension"" />
    </xs:keyref>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet("Example");

			// Add a DataTable with no ReadOnly columns
			DataTable dt1 = new DataTable ("StandAlone");
			ds.Tables.Add (dt1);

			// Add a ReadOnly column
			dt1.Columns.Add (new DataColumn ("ID", typeof(int), "", 
				MappingType.Attribute));
			dt1.Columns ["ID"].AllowDBNull = false;

			dt1.Columns.Add (new DataColumn ("Desc", typeof
				(string), "", MappingType.Attribute));
			dt1.Columns ["Desc"].AllowDBNull = false;

			// Add related DataTables with ReadOnly columns
			DataTable dt2 = new DataTable ("Dimension");
			ds.Tables.Add (dt2);
			dt2.Columns.Add (new DataColumn ("Number", typeof
				(int), "", MappingType.Attribute));
			dt2.Columns ["Number"].AllowDBNull = false;
			dt2.Columns ["Number"].ReadOnly = true;

			dt2.Columns.Add (new DataColumn ("Title", typeof
				(string), "", MappingType.Attribute));
			dt2.Columns ["Title"].AllowDBNull = false;

			dt2.Constraints.Add ("PK_Dimension", dt2.Columns ["Number"], true);

			DataTable dt3 = new DataTable ("Element");
			ds.Tables.Add(dt3);
			
			dt3.Columns.Add (new DataColumn ("Dimension", typeof
				(int), "", MappingType.Attribute));
			dt3.Columns ["Dimension"].AllowDBNull = false;
			dt3.Columns ["Dimension"].ReadOnly = true;

			dt3.Columns.Add (new DataColumn ("Number", typeof
				(int), "", MappingType.Attribute));
			dt3.Columns ["Number"].AllowDBNull = false;
			dt3.Columns ["Number"].ReadOnly = true;

			dt3.Columns.Add (new DataColumn ("Title", typeof
				(string), "", MappingType.Attribute));
			dt3.Columns ["Title"].AllowDBNull = false;

			dt3.Constraints.Add ("PK_Element", new DataColumn[] { 
				dt3.Columns ["Dimension"],
				dt3.Columns ["Number"] }, true);

			ds.Relations.Add ("FK_Element_To_Dimension",
				dt2.Columns ["Number"], dt3.Columns["Dimension"]);

			ds.AcceptChanges ();

			StringWriter sw = new StringWriter ();
			ds.WriteXmlSchema (sw);

			string result = sw.ToString ();

			AssertEquals (xmlschema, result.Replace ("\r\n", "\n"));
		}

		// bug #67793
		[Test]
		public void WriteXmlSchema6 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:Locale=""fi-FI"">
    <xs:complexType>
      <xs:choice maxOccurs=""unbounded"">
        <xs:element name=""MyType"">
          <xs:complexType>
            <xs:attribute name=""Desc"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                  <xs:maxLength value=""32"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:attribute>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet("Example");

			// Add MyType DataTable
			ds.Tables.Add ("MyType");

			ds.Tables ["MyType"].Columns.Add (new DataColumn(
				"Desc", typeof (string), "", MappingType.Attribute));
			ds.Tables ["MyType"].Columns ["Desc"].MaxLength = 32;

			ds.AcceptChanges ();

			StringWriter sw = new StringWriter ();
			ds.WriteXmlSchema (sw);

			string result = sw.ToString ();

			AssertEquals (xmlschema, result.Replace ("\r\n", "\n"));
		}

		// bug #68008
		[Test]
		public void WriteXmlSchema7 ()
		{
			DataSet ds = new DataSet ();
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			ds.Tables.Add (dt);
			dt.Rows.Add (new object [] {"foo", "bar"});
			StringWriter sw = new StringWriter ();
			ds.WriteXmlSchema (sw);
			Assert (sw.ToString ().IndexOf ("xmlns=\"\"") > 0);
		}

		// bug #61233
		[Test]
		public void WriteXmlExtendedProperties ()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""NewDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:msprop=""urn:schemas-microsoft-com:xml-msprop"">
  <xs:element name=""NewDataSet"" msdata:IsDataSet=""true"" msdata:Locale=""fi-FI"" msprop:version=""version 2.1"">
    <xs:complexType>
      <xs:choice maxOccurs=""unbounded"">
        <xs:element name=""Foo"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""col1"" type=""xs:string"" minOccurs=""0"" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			DataSet ds = new DataSet ();
			ds.ExtendedProperties ["version"] = "version 2.1";
			DataTable dt = new DataTable ("Foo");
			dt.Columns.Add ("col1");
			dt.Rows.Add (new object [] {"foo"});
			ds.Tables.Add (dt);

			StringWriter sw = new StringWriter ();
			ds.WriteXmlSchema (sw);

			string result = sw.ToString ();

			AssertEquals (xml, result.Replace ("\r\n", "\n"));
		}

		[Test]
		public void WriteXmlModeSchema ()
		{
			// This is the MS output of WriteXmlSchema().

			string xml = @"<Example>
  <xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:Locale=""fi-FI"">
      <xs:complexType>
        <xs:choice maxOccurs=""unbounded"">
          <xs:element name=""Dimension"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""Number"" type=""xs:int"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
          <xs:element name=""Element"">
            <xs:complexType>
              <xs:sequence>
                <xs:element name=""Dimension"" type=""xs:int"" />
                <xs:element name=""Number"" type=""xs:int"" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:choice>
      </xs:complexType>
      <xs:unique name=""PK_Dimension"" msdata:PrimaryKey=""true"">
        <xs:selector xpath="".//Dimension"" />
        <xs:field xpath=""Number"" />
      </xs:unique>
      <xs:unique name=""PK_Element"" msdata:PrimaryKey=""true"">
        <xs:selector xpath="".//Element"" />
        <xs:field xpath=""Dimension"" />
        <xs:field xpath=""Number"" />
      </xs:unique>
      <xs:keyref name=""FK_Element_To_Dimension"" refer=""PK_Dimension"">
        <xs:selector xpath="".//Element"" />
        <xs:field xpath=""Dimension"" />
      </xs:keyref>
    </xs:element>
  </xs:schema>
  <Dimension>
    <Number>0</Number>
  </Dimension>
  <Dimension>
    <Number>1</Number>
  </Dimension>
  <Element>
    <Dimension>0</Dimension>
    <Number>0</Number>
  </Element>
  <Element>
    <Dimension>0</Dimension>
    <Number>1</Number>
  </Element>
  <Element>
    <Dimension>0</Dimension>
    <Number>2</Number>
  </Element>
  <Element>
    <Dimension>0</Dimension>
    <Number>3</Number>
  </Element>
  <Element>
    <Dimension>1</Dimension>
    <Number>0</Number>
  </Element>
  <Element>
    <Dimension>1</Dimension>
    <Number>1</Number>
  </Element>
</Example>";
			DataSet ds = new DataSet("Example");

			// Dimension DataTable
			DataTable dt1 = new DataTable ("Dimension");
			ds.Tables.Add (dt1);

			dt1.Columns.Add (new DataColumn ("Number", typeof (int)));
			dt1.Columns ["Number"].AllowDBNull = false;

			dt1.Constraints.Add ("PK_Dimension", dt1.Columns ["Number"], true);

			// Element DataTable
			DataTable dt2 = new DataTable ("Element");
			ds.Tables.Add (dt2);

			dt2.Columns.Add (new DataColumn ("Dimension", typeof (int)));
			dt2.Columns ["Dimension"].AllowDBNull = false;

			dt2.Columns.Add (new DataColumn ("Number", typeof (int)));
			dt2.Columns ["Number"].AllowDBNull = false;

			dt2.Constraints.Add ("PK_Element", new DataColumn[] {
				dt2.Columns ["Dimension"],
				dt2.Columns ["Number"] },
				true);
			
			// Add DataRelations
			ds.Relations.Add ("FK_Element_To_Dimension",
				dt1.Columns ["Number"],
				dt2.Columns ["Dimension"], true);

			// Add 2 Dimensions
			for (int i = 0; i < 2; i++) {
				DataRow newRow = dt1.NewRow ();
				newRow ["Number"] = i;
				dt1.Rows.Add (newRow);
			}

			// Dimension 0 => 4 Elements
			for (int i = 0; i < 4; i++) {
				DataRow newRow = dt2.NewRow();
				newRow ["Dimension"] = 0;
				newRow ["Number"] = i;
				dt2.Rows.Add (newRow);
			}

			// Dimension 1 => 2 Elements
			for (int i = 0; i < 2; i++) {
				DataRow newRow = dt2.NewRow();
				newRow ["Dimension"] = 1;
				newRow ["Number"] = i;
				dt2.Rows.Add (newRow);
			}

			ds.AcceptChanges ();

			StringWriter sw = new StringWriter ();
			ds.WriteXml(sw, XmlWriteMode.WriteSchema);

			string result = sw.ToString ();

			AssertEquals (xml, result.Replace ("\r\n", "\n"));
		}

		[Test]
		public void DeserializeModifiedDataSet ()
                {
                        // Serialization begins
                        DataSet prevDs = new DataSet ();
			DataTable dt = prevDs.Tables.Add ();
                        dt.Columns.Add(new DataColumn("Id", typeof(string)));
                                                                                                                             
                        DataRow dr = dt.NewRow();
                        dr [0] = "a";
                        dt.Rows.Add (dr);
                        prevDs.AcceptChanges ();
                        dr = prevDs.Tables[0].Rows[0];
                        dr [0] = "b";
                                                                                                                             
			XmlSerializer serializer = new XmlSerializer (typeof (DataSet));
			StringWriter sw = new StringWriter ();
			XmlTextWriter xw = new XmlTextWriter (sw);
			xw.QuoteChar = '\'';
			serializer.Serialize (xw, prevDs);

                        // Deserialization begins
			StringReader sr = new StringReader (sw.ToString ());
			XmlTextReader reader = new XmlTextReader (sr);
			XmlSerializer serializer1 = new XmlSerializer (typeof (DataSet));
			DataSet ds = serializer1.Deserialize (reader) as DataSet;
                        AssertEquals ("deserialization after modification does not give original values",
                                prevDs.Tables[0].Rows [0][0,DataRowVersion.Original].ToString (), 
				ds.Tables[0].Rows [0][0,DataRowVersion.Original].ToString ());
                        AssertEquals ("deserialization after modification oes not give current values",
                                prevDs.Tables[0].Rows [0][0,DataRowVersion.Current].ToString (), 
				ds.Tables[0].Rows [0][0,DataRowVersion.Current].ToString ());
                }

                /// <summary>
                /// Test for testing DataSet.Clear method with foriegn key relations
                /// This is expected to clear all the related datatable rows also
                /// </summary>
                [Test]
                public void DataSetClearTest ()
                {
                        DataSet ds = new DataSet ();
                        DataTable parent = ds.Tables.Add ("Parent");
                        DataTable child = ds.Tables.Add ("Child");
                        
                        parent.Columns.Add ("id", typeof (int));
                        child.Columns.Add ("ref_id", typeof(int));
                        
                        child.Constraints.Add (new ForeignKeyConstraint ("fk_constraint", parent.Columns [0], child.Columns [0]));
                        
                        DataRow dr = parent.NewRow ();
                        dr [0] = 1;
                        parent.Rows.Add (dr);
                        dr.AcceptChanges ();
                        
                        dr = child.NewRow ();
                        dr [0] = 1;
                        child.Rows.Add (dr);
                        dr.AcceptChanges ();
                        
                        try {
                                ds.Clear (); // this should clear all the rows in parent & child tables
                        } catch (Exception e) {
                                throw (new Exception ("Exception should not have been thrown at Clear method" + e.ToString ()));
                        }
                        Assertion.AssertEquals ("parent table rows should not exist!", 0, parent.Rows.Count);
                        Assertion.AssertEquals ("child table rows should not exist!", 0, child.Rows.Count);
                }

		[Test]
		public void CloneSubClassTest()
		{
			MyDataSet ds1 = new MyDataSet();
                        MyDataSet ds = (MyDataSet)(ds1.Clone());
                     	AssertEquals("A#01",2,MyDataSet.count);
		}
        }


	 public  class MyDataSet:DataSet {

	     public static int count = 0;
                                                                                                    
             public MyDataSet() {

                    count++;
             }
                                                                                                    
         }
	

}
