// MonoTests.System.Data.DataSetTest.cs
//
// Authors:
//   Ville Palo <vi64pa@koti.soon.fi>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//   Atsushi Enomoto <atsushi@ximian.com>
//   Hagit Yidov <hagity@mainsoft.com>
//
// (C) Copyright 2002 Ville Palo
// (C) Copyright 2003 Martin Willemoes Hansen
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
// Copyright 2011 Xamarin Inc.
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
using System.Xml.Schema;
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
		CultureInfo currentCultureBackup;

		[SetUp]
		public void Setup () {
			currentCultureBackup = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("fi-FI");
		}

		//[SetUp]
		//public void GetReady()
		//{
		//        currentCultureBackup = Thread.CurrentThread.CurrentCulture;
		//        Thread.CurrentThread.CurrentCulture = new CultureInfo ("fi-FI");
		//}

		[TearDown]
		public void Teardown ()
		{
			Thread.CurrentThread.CurrentCulture = currentCultureBackup;
		}

		[Test]
		public void Properties ()
		{
			DataSet ds = new DataSet ();
			Assert.AreEqual (String.Empty, ds.Namespace, "default namespace");
			ds.Namespace = null; // setting null == setting ""
			Assert.AreEqual (String.Empty, ds.Namespace, "after setting null to namespace");

			Assert.AreEqual (String.Empty, ds.Prefix, "default prefix");
			ds.Prefix = null; // setting null == setting ""
			Assert.AreEqual (String.Empty, ds.Prefix, "after setting null to prefix");
		}

		[Test]
		public void ReadXmlSchema ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema ("Test/System.Data/own_schema.xsd");
			
			Assert.AreEqual (2, ds.Tables.Count, "test#01");
			DataTable Table = ds.Tables [0];
			Assert.AreEqual ("test_table", Table.TableName, "test#02");
			Assert.AreEqual ("", Table.Namespace, "test#03");
			Assert.AreEqual (2, Table.Columns.Count, "test#04");
			Assert.AreEqual (0, Table.Rows.Count, "test#05");
			Assert.IsFalse (Table.CaseSensitive, "test#06");
			Assert.AreEqual (1, Table.Constraints.Count, "test#07");
			Assert.AreEqual ("", Table.Prefix, "test#08");
			
			Constraint cons = Table.Constraints [0];
			Assert.AreEqual ("Constraint1", cons.ConstraintName.ToString (), "test#09");
			Assert.AreEqual ("Constraint1", cons.ToString (), "test#10");
			
			DataColumn column = Table.Columns [0];
			Assert.IsTrue (column.AllowDBNull, "test#11");
			Assert.IsFalse (column.AutoIncrement, "test#12");
			Assert.AreEqual (0L, column.AutoIncrementSeed, "test#13");
			Assert.AreEqual (1L, column.AutoIncrementStep, "test#14");
			Assert.AreEqual ("test", column.Caption, "test#15");
			Assert.AreEqual ("Element", column.ColumnMapping.ToString (), "test#16");
			Assert.AreEqual ("first", column.ColumnName, "test#17");
			Assert.AreEqual ("System.String", column.DataType.ToString (), "test#18");
			Assert.AreEqual ("test_default_value", column.DefaultValue.ToString (), "test#19");
			Assert.IsFalse (column.DesignMode, "test#20");
			Assert.AreEqual ("", column.Expression, "test#21");
			Assert.AreEqual (100, column.MaxLength, "test#22");
			Assert.AreEqual ("", column.Namespace, "test#23");
			Assert.AreEqual (0, column.Ordinal, "test#24");
			Assert.AreEqual ("", column.Prefix, "test#25");
			Assert.IsFalse (column.ReadOnly, "test#26");
			Assert.IsTrue (column.Unique, "test#27");
						
			DataColumn column2 = Table.Columns [1];
			Assert.IsTrue (column2.AllowDBNull, "test#28");
			Assert.IsFalse (column2.AutoIncrement, "test#29");
			Assert.AreEqual (0L, column2.AutoIncrementSeed, "test#30");
			Assert.AreEqual (1L, column2.AutoIncrementStep, "test#31");
			Assert.AreEqual ("second", column2.Caption, "test#32");
			Assert.AreEqual ("Element", column2.ColumnMapping.ToString (), "test#33");
			Assert.AreEqual ("second", column2.ColumnName, "test#34");
			Assert.AreEqual ("System.Data.SqlTypes.SqlGuid", column2.DataType.ToString (), "test#35");
#if NET_2_0
			Assert.AreEqual (SqlGuid.Null, column2.DefaultValue, "test#36");
#else
			Assert.AreEqual (DBNull.Value, column2.DefaultValue, "test#36");
#endif
			Assert.IsFalse (column2.DesignMode, "test#37");
			Assert.AreEqual ("", column2.Expression, "test#38");
			Assert.AreEqual (-1, column2.MaxLength, "test#39");
			Assert.AreEqual ("", column2.Namespace, "test#40");
			Assert.AreEqual (1, column2.Ordinal, "test#41");
			Assert.AreEqual ("", column2.Prefix, "test#42");
			Assert.IsFalse (column2.ReadOnly, "test#43");
			Assert.IsFalse (column2.Unique, "test#44");
			
			DataTable Table2 = ds.Tables [1];
			Assert.AreEqual ("second_test_table", Table2.TableName, "test#45");
			Assert.AreEqual ("", Table2.Namespace, "test#46");
			Assert.AreEqual (1, Table2.Columns.Count, "test#47");
			Assert.AreEqual (0, Table2.Rows.Count, "test#48");
			Assert.IsFalse (Table2.CaseSensitive, "test#49");
			Assert.AreEqual (1, Table2.Constraints.Count, "test#50");
			Assert.AreEqual ("", Table2.Prefix, "test#51");
			
			DataColumn column3 = Table2.Columns [0];
			Assert.IsTrue (column3.AllowDBNull, "test#52");
			Assert.IsFalse (column3.AutoIncrement, "test#53");
			Assert.AreEqual (0L, column3.AutoIncrementSeed, "test#54");
			Assert.AreEqual (1L, column3.AutoIncrementStep, "test#55");
			Assert.AreEqual ("second_first", column3.Caption, "test#56");
			Assert.AreEqual ("Element", column3.ColumnMapping.ToString (), "test#57");
			Assert.AreEqual ("second_first", column3.ColumnName, "test#58");
			Assert.AreEqual ("System.String", column3.DataType.ToString (), "test#59");
			Assert.AreEqual ("default_value", column3.DefaultValue.ToString (), "test#60");
			Assert.IsFalse (column3.DesignMode, "test#61");
			Assert.AreEqual ("", column3.Expression, "test#62");
			Assert.AreEqual (100, column3.MaxLength, "test#63");
			Assert.AreEqual ("", column3.Namespace, "test#64");
			Assert.AreEqual (0, column3.Ordinal, "test#65");
			Assert.AreEqual ("", column3.Prefix, "test#66");
			Assert.IsFalse (column3.ReadOnly, "test#67");
			Assert.IsTrue (column3.Unique, "test#68");
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
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring, "test#01");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("<xs:schema id=\"test_dataset\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring, "test#02");
			Assert.AreEqual ("<xs:schema id=\"test_dataset\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">", substring, "test#02");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("  <xs:element name=\"test_dataset\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring, "test#03");
#if !NET_2_0
			Assert.AreEqual ("  <xs:element msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\" name=\"test_dataset\">", substring, "test#03");
#else
			Assert.AreEqual ("  <xs:element msdata:IsDataSet=\"true\" msdata:UseCurrentLocale=\"true\" name=\"test_dataset\">", substring, "test#03");
#endif

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:complexType>", substring, "test#04");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:choice maxOccurs=\"unbounded\" minOccurs=\"0\">", substring, "test#05");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element name=\"test_table\">", substring, "test#06");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          <xs:complexType>", substring, "test#07");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            <xs:sequence>", substring, "test#08");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("              <xs:element name=\"first\" msdata:Caption=\"test\" default=\"test_default_value\" minOccurs=\"0\">", substring, "test#09");
			Assert.AreEqual ("              <xs:element default=\"test_default_value\" minOccurs=\"0\" msdata:Caption=\"test\" name=\"first\">", substring, "test#09");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("                <xs:simpleType>", substring, "test#10");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("                  <xs:restriction base=\"xs:string\">", substring, "test#11");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("                    <xs:maxLength value=\"100\" />", substring, "test#12");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("                  </xs:restriction>", substring, "test#13");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("                </xs:simpleType>", substring, "test#14");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("              </xs:element>", substring, "test#15");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
#if NET_4_0
			Assert.AreEqual ("              <xs:element minOccurs=\"0\" msdata:DataType=\"System.Data.SqlTypes.SqlGuid, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" name=\"second\" type=\"xs:string\" />", substring, "test#16");
#elif NET_2_0
			Assert.AreEqual ("              <xs:element minOccurs=\"0\" msdata:DataType=\"System.Data.SqlTypes.SqlGuid, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" name=\"second\" type=\"xs:string\" />", substring, "test#16");
#else
			#error "Unknown profile"
#endif
			if (substring.IndexOf ("<xs:element") < 0)
				Assert.Fail ("test#16: " + substring);
			if (substring.IndexOf ("name=\"second\"") < 0)
				Assert.Fail ("test#16: " + substring);
			if (substring.IndexOf ("msdata:DataType=\"System.Data.SqlTypes.SqlGuid, System.Data, Version=") < 0)
				Assert.Fail ("test#16: " + substring);
			if (substring.IndexOf ("type=\"xs:string\"") < 0)
				Assert.Fail ("test#16: " + substring);
			if (substring.IndexOf ("minOccurs=\"0\"") < 0)
				Assert.Fail ("test#16: " + substring);
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            </xs:sequence>", substring, "test#17");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          </xs:complexType>", substring, "test#18");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        </xs:element>", substring, "test#19");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element name=\"second_test_table\">", substring, "test#20");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          <xs:complexType>", substring, "test#21");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            <xs:sequence>", substring, "test#22");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("              <xs:element name=\"second_first\" default=\"default_value\" minOccurs=\"0\">", substring, "test#23");
			Assert.AreEqual ("              <xs:element default=\"default_value\" minOccurs=\"0\" name=\"second_first\">", substring, "test#23");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("                <xs:simpleType>", substring, "test#24");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("                  <xs:restriction base=\"xs:string\">", substring, "test#25");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("                    <xs:maxLength value=\"100\" />", substring, "test#26");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("                  </xs:restriction>", substring, "test#27");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("                </xs:simpleType>", substring, "test#28");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("              </xs:element>", substring, "test#29");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            </xs:sequence>", substring, "test#30");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          </xs:complexType>", substring, "test#31");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        </xs:element>", substring, "test#32");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      </xs:choice>", substring, "test#33");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:complexType>", substring, "test#34");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:unique name=\"Constraint1\">", substring, "test#36");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:selector xpath=\".//test_table\" />", substring, "test#37");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:field xpath=\"first\" />", substring, "test#38");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:unique>", substring, "test#39");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("    <xs:unique name=\"second_test_table_Constraint1\" msdata:ConstraintName=\"Constraint1\">", substring, "test#40");
			Assert.AreEqual ("    <xs:unique msdata:ConstraintName=\"Constraint1\" name=\"second_test_table_Constraint1\">", substring, "test#40");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:selector xpath=\".//second_test_table\" />", substring, "test#41");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:field xpath=\"second_first\" />", substring, "test#42");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:unique>", substring, "test#43");
			
			substring = TextString.Substring (0, TextString.IndexOf(EOL));
			TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:element>", substring, "test#44");			
			Assert.AreEqual ("</xs:schema>", TextString, "test#45");
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
                        Assert.AreEqual ("<Root>", substring, "test#01");

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert.AreEqual ("  <Region>", substring, "test#02");
			
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert.AreEqual ("    <RegionID>1</RegionID>", substring, "test#03");
			// Here the end of line is text markup "\n"
                        substring = TextString.Substring (0, TextString.IndexOf('\n'));
                        TextString = TextString.Substring (TextString.IndexOf('\n') + 1);
                        Assert.AreEqual ("    <RegionDescription>Eastern", substring, "test#04");

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert.AreEqual ("   </RegionDescription>", substring, "test#05");

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert.AreEqual ("  </Region>", substring, "test#06");

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert.AreEqual ("  <Region>", substring, "test#07");
			
                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert.AreEqual ("    <RegionID>2</RegionID>", substring, "test#08");

			// Here the end of line is text markup "\n"
                        substring = TextString.Substring (0, TextString.IndexOf('\n'));
                        TextString = TextString.Substring (TextString.IndexOf('\n') + 1);
                        Assert.AreEqual ("    <RegionDescription>Western", substring, "test#09");

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert.AreEqual ("   </RegionDescription>", substring, "test#10");

                        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
                        Assert.AreEqual ("  </Region>", substring, "test#11");

                        Assert.AreEqual ("</Root>", TextString, "test#11");
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
                        Assert.AreEqual ("<NewDataSet />", TextString, "test#01");

			ds.WriteXml (writer, XmlWriteMode.DiffGram);
			TextString = writer.ToString ();
			
			Assert.AreEqual ("<NewDataSet /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\" />", TextString, "test#02");

			
			ds = new DataSet ();
			ds.ReadXml ("Test/System.Data/region.xml");
			DataTable table = ds.Tables ["Region"];
			table.Rows [0] [0] = "64";
			ds.ReadXml ("Test/System.Data/region.xml", XmlReadMode.DiffGram);
			ds.WriteXml (writer, XmlWriteMode.DiffGram);
			
			TextString = writer.ToString ();
                        string substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("<NewDataSet /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\" /><diffgr:diffgram xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:diffgr=\"urn:schemas-microsoft-com:xml-diffgram-v1\">", substring, "test#03");

                      	substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <Root>", substring, "test#04");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <Region diffgr:id=\"Region1\" msdata:rowOrder=\"0\" diffgr:hasChanges=\"inserted\">", substring, "test#05");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <RegionID>64</RegionID>", substring, "test#06");

			// not EOL but literal '\n'
		        substring = TextString.Substring (0, TextString.IndexOf('\n'));
                        TextString = TextString.Substring (TextString.IndexOf('\n') + 1);
			Assert.AreEqual ("      <RegionDescription>Eastern", substring, "test#07");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("   </RegionDescription>", substring, "test#07");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </Region>", substring, "test#08");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <Region diffgr:id=\"Region2\" msdata:rowOrder=\"1\" diffgr:hasChanges=\"inserted\">", substring, "test#09");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <RegionID>2</RegionID>", substring, "test#10");

			// not EOL but literal '\n'
		        substring = TextString.Substring (0, TextString.IndexOf('\n'));
                        TextString = TextString.Substring (TextString.IndexOf('\n') + 1);
			Assert.AreEqual ("      <RegionDescription>Western", substring, "test#11");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("   </RegionDescription>", substring, "test#12");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </Region>", substring, "test#13");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </Root>", substring, "test#14");
			
			Assert.AreEqual ("</diffgr:diffgram>", TextString, "test#15");
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
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring, "test#01");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("<xs:schema id=\"Root\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring, "test#02");
			Assert.AreEqual ("<xs:schema id=\"Root\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">", substring, "test#02");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:element msdata:IsDataSet=\"true\" msdata:Locale=\"en-US\" name=\"Root\">", substring, "test#03");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:complexType>", substring, "test#04");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:choice maxOccurs=\"unbounded\" minOccurs=\"0\">", substring, "test#05");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element name=\"Region\">", substring, "test#06");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          <xs:complexType>", substring, "test#07");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            <xs:sequence>", substring, "test#08");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("              <xs:element name=\"RegionID\" type=\"xs:string\" minOccurs=\"0\" />", substring, "test#09");
			Assert.AreEqual ("              <xs:element minOccurs=\"0\" name=\"RegionID\" type=\"xs:string\" />", substring, "test#09");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("              <xs:element name=\"RegionDescription\" type=\"xs:string\" minOccurs=\"0\" />", substring, "test#10");
			Assert.AreEqual ("              <xs:element minOccurs=\"0\" name=\"RegionDescription\" type=\"xs:string\" />", substring, "test#10");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            </xs:sequence>", substring, "test#11");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          </xs:complexType>", substring, "test#12");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        </xs:element>", substring, "test#13");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      </xs:choice>", substring, "test#14");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:complexType>", substring, "test#15");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:element>", substring, "test#16");

			Assert.AreEqual ("</xs:schema>", TextString, "test#17");
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
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring, "test#01");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("<xs:schema id=\"NewDataSet\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring, "test#02");
			Assert.AreEqual ("<xs:schema id=\"NewDataSet\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">", substring, "test#02");

			substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:complexType name=\"bookstoreType\">", substring, "test#03");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:sequence>", substring, "test#04");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("      <xs:element name=\"book\" type=\"bookType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring, "test#05");
			Assert.AreEqual ("      <xs:element maxOccurs=\"unbounded\" minOccurs=\"0\" name=\"book\" type=\"bookType\" />", substring, "test#05");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:sequence>", substring, "test#06");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:complexType>", substring, "test#07");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:complexType name=\"bookType\">", substring, "test#08");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:sequence>", substring, "test#09");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("      <xs:element name=\"title\" type=\"xs:string\" msdata:Ordinal=\"1\" />", substring, "test#10");
			
			Assert.AreEqual ("      <xs:element msdata:Ordinal=\"1\" name=\"title\" type=\"xs:string\" />", substring, "test#10");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("      <xs:element name=\"price\" type=\"xs:decimal\" msdata:Ordinal=\"2\" />", substring, "test#11");
			Assert.AreEqual ("      <xs:element msdata:Ordinal=\"2\" name=\"price\" type=\"xs:decimal\" />", substring, "test#11");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("      <xs:element name=\"author\" type=\"authorName\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring, "test#12");
			Assert.AreEqual ("      <xs:element maxOccurs=\"unbounded\" minOccurs=\"0\" name=\"author\" type=\"authorName\" />", substring, "test#12");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:sequence>", substring, "test#13");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:attribute name=\"genre\" type=\"xs:string\" />", substring, "test#14");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:complexType>", substring, "test#15");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:complexType name=\"authorName\">", substring, "test#16");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:sequence>", substring, "test#17");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:element name=\"first-name\" type=\"xs:string\" />", substring, "test#18");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:element name=\"last-name\" type=\"xs:string\" />", substring, "test#19");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:sequence>", substring, "test#20");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:complexType>", substring, "test#21");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:element name=\"bookstore\" type=\"bookstoreType\" />", substring, "test#22");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("  <xs:element name=\"NewDataSet\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring, "test#23");
			Assert.AreEqual ("  <xs:element msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\" name=\"NewDataSet\">", substring, "test#23");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:complexType>", substring, "test#24");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:choice maxOccurs=\"unbounded\" minOccurs=\"0\">", substring, "test#25");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element ref=\"bookstore\" />", substring, "test#26");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      </xs:choice>", substring, "test#27");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:complexType>", substring, "test#28");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:element>", substring, "test#29");

			Assert.AreEqual ("</xs:schema>", TextString, "test#30");
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
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", substring, "test#01");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("<xs:schema id=\"Root\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">", substring, "test#02");
			Assert.AreEqual ("<xs:schema id=\"Root\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">", substring, "test#02");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:complexType name=\"bookstoreType\">", substring, "test#03");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:sequence>", substring, "test#04");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("      <xs:element name=\"book\" type=\"bookType\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring, "test#05");
			Assert.AreEqual ("      <xs:element maxOccurs=\"unbounded\" minOccurs=\"0\" name=\"book\" type=\"bookType\" />", substring, "test#05");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:sequence>", substring, "test#06");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:complexType>", substring, "test#07");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:complexType name=\"bookType\">", substring, "test#08");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:sequence>", substring, "test#09");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("      <xs:element name=\"title\" type=\"xs:string\" msdata:Ordinal=\"1\" />", substring, "test#10");
			Assert.AreEqual ("      <xs:element msdata:Ordinal=\"1\" name=\"title\" type=\"xs:string\" />", substring, "test#10");
		        
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("      <xs:element name=\"price\" type=\"xs:decimal\" msdata:Ordinal=\"2\" />", substring, "test#11");
			Assert.AreEqual ("      <xs:element msdata:Ordinal=\"2\" name=\"price\" type=\"xs:decimal\" />", substring, "test#11");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("      <xs:element name=\"author\" type=\"authorName\" minOccurs=\"0\" maxOccurs=\"unbounded\" />", substring, "test#12");
			Assert.AreEqual ("      <xs:element maxOccurs=\"unbounded\" minOccurs=\"0\" name=\"author\" type=\"authorName\" />", substring, "test#12");
	
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:sequence>", substring, "test#13");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:attribute name=\"genre\" type=\"xs:string\" />", substring, "test#14");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:complexType>", substring, "test#15");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:complexType name=\"authorName\">", substring, "test#16");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:sequence>", substring, "test#17");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:element name=\"first-name\" type=\"xs:string\" />", substring, "test#18");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:element name=\"last-name\" type=\"xs:string\" />", substring, "test#19");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:sequence>", substring, "test#20");
		
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:complexType>", substring, "test#21");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  <xs:element name=\"bookstore\" type=\"bookstoreType\" />", substring, "test#22");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("  <xs:element name=\"Root\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\">", substring, "test#23");
			Assert.AreEqual ("  <xs:element msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\" name=\"Root\">", substring, "test#23");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    <xs:complexType>", substring, "test#24");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      <xs:choice maxOccurs=\"unbounded\" minOccurs=\"0\">", substring, "test#25");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element ref=\"bookstore\" />", substring, "test#26");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        <xs:element name=\"Region\">", substring, "test#27");

		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          <xs:complexType>", substring, "test#28");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            <xs:sequence>", substring, "test#29");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("              <xs:element name=\"RegionID\" type=\"xs:string\" minOccurs=\"0\" />", substring, "test#30");
			Assert.AreEqual ("              <xs:element minOccurs=\"0\" name=\"RegionID\" type=\"xs:string\" />", substring, "test#30");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			// This is original DataSet.WriteXmlSchema() output
//			Assert.AreEqual ("              <xs:element name=\"RegionDescription\" type=\"xs:string\" minOccurs=\"0\" />", substring, "test#31");
			Assert.AreEqual ("              <xs:element minOccurs=\"0\" name=\"RegionDescription\" type=\"xs:string\" />", substring, "test#31");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("            </xs:sequence>", substring, "test#32");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("          </xs:complexType>", substring, "test#33");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("        </xs:element>", substring, "test#34");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("      </xs:choice>", substring, "test#35");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("    </xs:complexType>", substring, "test#36");
			
		        substring = TextString.Substring (0, TextString.IndexOf(EOL));
                        TextString = TextString.Substring (TextString.IndexOf(EOL) + EOL.Length);
			Assert.AreEqual ("  </xs:element>", substring, "test#37");

			Assert.AreEqual ("</xs:schema>", TextString, "test#38");
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteDifferentNamespaceSchema ()
		{
			string schema = @"<?xml version='1.0' encoding='utf-16'?>
<xs:schema id='NewDataSet' targetNamespace='urn:bar' xmlns:mstns='urn:bar' xmlns='urn:bar' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' attributeFormDefault='qualified' elementFormDefault='qualified' xmlns:app1='urn:baz' xmlns:app2='urn:foo'>
  <!--ATTENTION: This schema contains references to other imported schemas-->
  <xs:import namespace='urn:baz' schemaLocation='_app1.xsd' />
  <xs:import namespace='urn:foo' schemaLocation='_app2.xsd' />
  <xs:element name='NewDataSet' msdata:IsDataSet='true' msdata:Locale='fi-FI'>
    <xs:complexType>
      <xs:choice minOccurs='0' maxOccurs='unbounded'>
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
			Assert.AreEqual (result.Replace ("\r\n", "\n"), schema.Replace ("\r\n", "\n"));
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
			Assert.AreEqual (xml, sw.ToString ());
		}

		[Test]
		public void SerializeDataSet ()
		{
			// see GetReady() for current culture

			string xml = "<?xml version='1.0' encoding='utf-16'?><DataSet><xs:schema id='DS' xmlns='' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'><xs:element name='DS' msdata:IsDataSet='true' " + 
#if !NET_2_0
			  "msdata:Locale='fi-FI'"
#else
			  "msdata:UseCurrentLocale='true'"
#endif
			  + "><xs:complexType><xs:choice minOccurs='0' maxOccurs='unbounded' /></xs:complexType></xs:element></xs:schema><diffgr:diffgram xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' xmlns:diffgr='urn:schemas-microsoft-com:xml-diffgram-v1' /></DataSet>";
			DataSet ds = new DataSet ();
			ds.DataSetName = "DS";
			XmlSerializer ser = new XmlSerializer (typeof (DataSet));
			StringWriter sw = new StringWriter ();
			XmlTextWriter xw = new XmlTextWriter (sw);
			xw.QuoteChar = '\'';
			ser.Serialize (xw, ds);

			string result = sw.ToString ();
			Assert.AreEqual (result.Replace ("\r\n", "\n"), xml.Replace ("\r\n", "\n"));
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
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?><DataSet><xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata""><xs:element name=""Example"" msdata:IsDataSet=""true""><xs:complexType><xs:choice maxOccurs=""unbounded"" minOccurs=""0""><xs:element name=""Packages""><xs:complexType><xs:attribute name=""ID"" type=""xs:int"" use=""required"" /><xs:attribute name=""ShipDate"" type=""xs:dateTime"" /><xs:attribute name=""Message"" type=""xs:string"" /><xs:attribute name=""Handlers"" type=""xs:int"" /></xs:complexType></xs:element></xs:choice></xs:complexType></xs:element></xs:schema><diffgr:diffgram xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:diffgr=""urn:schemas-microsoft-com:xml-diffgram-v1""><Example><Packages diffgr:id=""Packages1"" msdata:rowOrder=""0"" ID=""0"" ShipDate=""2004-10-11T17:46:18.6962302-05:00"" Message=""Received with no breakage!"" Handlers=""3"" /><Packages diffgr:id=""Packages2"" msdata:rowOrder=""1"" ID=""1"" /></Example></diffgr:diffgram></DataSet>";

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

			Assert.AreEqual (xml, result);
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
			Assert.AreEqual (result, xml, "#01");

			// load diffgram above
			ds.ReadXml (new StringReader (sw.ToString ()));
			sw = new StringWriter ();
			ds.WriteXml (sw, XmlWriteMode.WriteSchema);
			xml = sw.ToString ();
			result = new StreamReader ("Test/System.Data/DataSetReadXmlTest2.xml", Encoding.ASCII).ReadToEnd ();
			Assert.AreEqual (result, xml, "#02");
		}
		*/

		[Test]
                public void CloneCopy ()
                {
                        DataTable table = new DataTable ("pTable");
			DataTable table1 = new DataTable ("cTable");
			DataSet set = new DataSet ();

                        set.Tables.Add (table);
                        set.Tables.Add (table1);

			DataColumn col = new DataColumn ();
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

			// Set column to RO as commonly used by auto-increment fields.
			// ds.Copy() has to omit the RO check when cloning DataRows 
			table.Columns["Id"].ReadOnly = true;
			
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
                        Assert.AreEqual (set.CaseSensitive, copySet.CaseSensitive, "#A01");
			Assert.AreEqual (set.DataSetName, copySet.DataSetName, "#A02");
                        Assert.AreEqual (set.EnforceConstraints, copySet.EnforceConstraints, "#A03");
                        Assert.AreEqual (set.HasErrors, copySet.HasErrors, "#A04");
                        Assert.AreEqual (set.Namespace, copySet.Namespace, "#A05");
                        Assert.AreEqual (set.Prefix, copySet.Prefix, "#A06");
                        Assert.AreEqual (set.Relations.Count, copySet.Relations.Count, "#A07");
                        Assert.AreEqual (set.Tables.Count, copySet.Tables.Count, "#A08");
                        Assert.AreEqual (set.ExtendedProperties ["TimeStamp"], copySet.ExtendedProperties ["TimeStamp"], "#A09");
                        for (int i = 0;i < copySet.Tables.Count; i++) {
                                Assert.AreEqual (set.Tables [i].Rows.Count, copySet.Tables [i].Rows.Count, "#A10");
                                Assert.AreEqual (set.Tables [i].Columns.Count, copySet.Tables [i].Columns.Count, "#A11");
                        }
                        //Testing Clone ()
                        copySet = set.Clone ();
                        Assert.AreEqual (set.CaseSensitive, copySet.CaseSensitive, "#A12");
                        Assert.AreEqual (set.DataSetName, copySet.DataSetName, "#A13");
                        Assert.AreEqual (set.EnforceConstraints, copySet.EnforceConstraints, "#A14");
                        Assert.IsFalse (copySet.HasErrors, "#A15");
                        Assert.AreEqual (set.Namespace, copySet.Namespace, "#A16");
                        Assert.AreEqual (set.Prefix, copySet.Prefix, "#A17");
                        Assert.AreEqual (set.Relations.Count, copySet.Relations.Count, "#A18");
                        Assert.AreEqual (set.Tables.Count, copySet.Tables.Count, "#A19");
                        Assert.AreEqual (set.ExtendedProperties ["TimeStamp"], copySet.ExtendedProperties ["TimeStamp"], "#A20");
                        for (int i = 0;i < copySet.Tables.Count; i++) {
                                Assert.AreEqual (0, copySet.Tables [i].Rows.Count, "#A21");
                                Assert.AreEqual (set.Tables [i].Columns.Count, copySet.Tables [i].Columns.Count, "#A22");
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
		public void CloneCopy_TestForeignKeyConstraints ()
		{
			DataTable dirTable = new DataTable("Directories");

			DataColumn dir_UID = new DataColumn("UID", typeof(int));
			dir_UID.Unique = true;
			dir_UID.AllowDBNull = false;

			dirTable.Columns.Add(dir_UID);

			// Build a simple Files table
			DataTable fileTable = new DataTable("Files");

			DataColumn file_DirID = new DataColumn("DirectoryID", typeof(int));
			file_DirID.Unique = false;
			file_DirID.AllowDBNull = false;

			fileTable.Columns.Add(file_DirID);

			// Build the DataSet
			DataSet ds = new DataSet("TestDataset");
			ds.Tables.Add(dirTable);
			ds.Tables.Add(fileTable);

			// Add a foreign key constraint
			DataColumn[] parentColumns = new DataColumn[1];
			parentColumns[0] = ds.Tables["Directories"].Columns["UID"];

			DataColumn[] childColumns = new DataColumn[1];
			childColumns[0] = ds.Tables["Files"].Columns["DirectoryID"];

			ForeignKeyConstraint fk = new ForeignKeyConstraint("FK_Test", parentColumns, childColumns);
			ds.Tables["Files"].Constraints.Add(fk);		
			ds.EnforceConstraints = true;

			Assert.AreEqual (1, ds.Tables["Directories"].Constraints.Count, "#1");
			Assert.AreEqual (1, ds.Tables["Files"].Constraints.Count, "#2");

			// check clone works fine
			DataSet cloned_ds = ds.Clone ();
			Assert.AreEqual (1, cloned_ds.Tables["Directories"].Constraints.Count, "#3");
			Assert.AreEqual (1, cloned_ds.Tables["Files"].Constraints.Count, "#4");

			ForeignKeyConstraint clonedFk =  (ForeignKeyConstraint)cloned_ds.Tables["Files"].Constraints[0];
			Assert.AreEqual ("FK_Test", clonedFk.ConstraintName, "#5");
			Assert.AreEqual (1, clonedFk.Columns.Length, "#6");
			Assert.AreEqual ("DirectoryID", clonedFk.Columns[0].ColumnName, "#7");

			UniqueConstraint clonedUc = (UniqueConstraint)cloned_ds.Tables ["Directories"].Constraints[0];
			UniqueConstraint origUc = (UniqueConstraint)ds.Tables ["Directories"].Constraints[0];
			Assert.AreEqual (origUc.ConstraintName, clonedUc.ConstraintName, "#8");
			Assert.AreEqual (1, clonedUc.Columns.Length, "#9");
			Assert.AreEqual ("UID", clonedUc.Columns[0].ColumnName, "#10");

			// check copy works fine
			DataSet copy_ds = ds.Copy ();
			Assert.AreEqual (1, copy_ds.Tables["Directories"].Constraints.Count, "#11");
			Assert.AreEqual (1, copy_ds.Tables["Files"].Constraints.Count, "#12");

			ForeignKeyConstraint copyFk =  (ForeignKeyConstraint)copy_ds.Tables["Files"].Constraints[0];
			Assert.AreEqual ("FK_Test", copyFk.ConstraintName, "#13");
			Assert.AreEqual (1, copyFk.Columns.Length, "#14");
			Assert.AreEqual ("DirectoryID", copyFk.Columns[0].ColumnName, "#15");

			UniqueConstraint copyUc = (UniqueConstraint)copy_ds.Tables ["Directories"].Constraints[0];
			origUc = (UniqueConstraint)ds.Tables ["Directories"].Constraints[0];
			Assert.AreEqual (origUc.ConstraintName, copyUc.ConstraintName, "#16");
			Assert.AreEqual (1, copyUc.Columns.Length, "#17");
			Assert.AreEqual ("UID", copyUc.Columns[0].ColumnName, "#18");
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
			Assert.AreEqual (sw.ToString ().Replace ("\r\n", "\n"), xml.Replace ("\r\n", "\n"));
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
			Assert.AreEqual (sr.ReadToEnd ().Replace ("\r\n", "\n"), result.Replace ("\r\n", "\n"));
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
			Assert.AreEqual (sw.ToString ().Replace ("\r\n", "\n"), xml.Replace ("\r\n", "\n"));
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
			Assert.AreEqual (xml, sw.ToString ());
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
			Assert.AreEqual (input.Replace ("\r\n", "\n"), sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test] // bug #60469
		public void WriteXmlSchema2 ()
		{
			string xml = @"<myDataSet xmlns='NetFrameWork'><myTable><id>0</id><item>item 0</item></myTable><myTable><id>1</id><item>item 1</item></myTable><myTable><id>2</id><item>item 2</item></myTable><myTable><id>3</id><item>item 3</item></myTable><myTable><id>4</id><item>item 4</item></myTable><myTable><id>5</id><item>item 5</item></myTable><myTable><id>6</id><item>item 6</item></myTable><myTable><id>7</id><item>item 7</item></myTable><myTable><id>8</id><item>item 8</item></myTable><myTable><id>9</id><item>item 9</item></myTable></myDataSet>";
			string schema = @"<?xml version='1.0' encoding='utf-16'?>
<xs:schema id='myDataSet' targetNamespace='NetFrameWork' xmlns:mstns='NetFrameWork' xmlns='NetFrameWork' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' attributeFormDefault='qualified' elementFormDefault='qualified'>
  <xs:element name='myDataSet' msdata:IsDataSet='true' " +
#if NET_2_0
			"msdata:UseCurrentLocale='true'"
#else
			"msdata:Locale='fi-FI'"
#endif
			+ @">
    <xs:complexType>
      <xs:choice minOccurs='0' maxOccurs='unbounded'>
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

			Assert.AreEqual (xml, result);

			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
			xtw.Formatting = Formatting.Indented;
			OriginalDataSet.WriteXmlSchema (xtw);
			result = sw.ToString ();

			result = result.Replace ("\r\n", "\n").Replace ('"', '\'');
			Assert.AreEqual (schema.Replace ("\r\n", "\n"), result);
		}

		// bug #66366
		[Test]
		public void WriteXmlSchema3 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""ExampleDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
  <xs:element name=""ExampleDataSet"" msdata:IsDataSet=""true"" ";
#if NET_2_0
			xmlschema = xmlschema + "msdata:UseCurrentLocale=\"true\"";
#else
			xmlschema = xmlschema + "msdata:Locale=\"fi-FI\"";
#endif
			xmlschema = xmlschema + @">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
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

			Assert.AreEqual (result.Replace ("\r\n", "\n"), xmlschema.Replace ("\r\n", "\n"));
		}

		// bug #67792.
		[Test]
		public void WriteXmlSchema4 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
";
#if NET_2_0
			xmlschema = xmlschema + "  <xs:element name=\"Example\" msdata:IsDataSet=\"true\" msdata:UseCurrentLocale=\"true\"";
#else
			xmlschema = xmlschema + "  <xs:element name=\"Example\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\"";
#endif
			xmlschema = xmlschema + @">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
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

			Assert.AreEqual (result.Replace ("\r\n", "\n"), xmlschema.Replace ("\r\n", "\n"));
		}

		// bug # 68432
		[Test]
		public void WriteXmlSchema5 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
"+
#if NET_2_0
"  <xs:element name=\"Example\" msdata:IsDataSet=\"true\" msdata:UseCurrentLocale=\"true\""
#else
"  <xs:element name=\"Example\" msdata:IsDataSet=\"true\" msdata:Locale=\"fi-FI\""
#endif
			  + @">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
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

			Assert.AreEqual (result.Replace ("\r\n", "\n"), xmlschema.Replace ("\r\n", "\n"));
		}

		// bug #67793
		[Test]
		public void WriteXmlSchema6 ()
		{
			string xmlschema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
"+
#if NET_2_0
			  @"  <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:UseCurrentLocale=""true"""
#else
			  @"  <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:Locale=""fi-FI"""
#endif
			  + @">
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
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

			Assert.AreEqual (result.Replace ("\r\n", "\n"), xmlschema.Replace ("\r\n", "\n"));
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
			Assert.IsTrue (sw.ToString ().IndexOf ("xmlns=\"\"") > 0);
		}

		// bug #61233
		[Test]
		public void WriteXmlExtendedProperties ()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema id=""NewDataSet"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:msprop=""urn:schemas-microsoft-com:xml-msprop"">
" +
#if NET_2_0
@"  <xs:element name=""NewDataSet"" msdata:IsDataSet=""true"" msdata:UseCurrentLocale=""true"" msprop:version=""version 2.1"">"
#else
@"  <xs:element name=""NewDataSet"" msdata:IsDataSet=""true"" msdata:Locale=""fi-FI"" msprop:version=""version 2.1"">"
#endif
			  + @"
    <xs:complexType>
      <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
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

			Assert.AreEqual (result.Replace ("\r\n", "\n"), xml.Replace ("\r\n", "\n"));
		}

		[Test]
		public void WriteXmlModeSchema ()
		{
			// This is the MS output of WriteXmlSchema().

			string xml = @"<Example>
  <xs:schema id=""Example"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
" +
#if NET_2_0
@"    <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:UseCurrentLocale=""true"">"
#else
@"    <xs:element name=""Example"" msdata:IsDataSet=""true"" msdata:Locale=""fi-FI"">"
#endif
			  + @"
      <xs:complexType>
        <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
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

			Assert.AreEqual (result.Replace ("\r\n", "\n"), xml.Replace ("\r\n", "\n"));
		}

		[Test]
		public void WriteXmlModeSchema1 () {
			string SerializedDataTable =
@"<rdData>
  <MyDataTable CustomerID='VINET' CompanyName='Vins et alcools Chevalier' ContactName='Paul Henriot' />
</rdData>";
			string expected =
@"<rdData>
  <xs:schema id=""rdData"" xmlns="""" xmlns:xs=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xs:element name=""rdData"" msdata:IsDataSet=""true"" " +
			  @"msdata:Locale=""en-US"">" +
@"
      <xs:complexType>
        <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
          <xs:element name=""MyDataTable"">
            <xs:complexType>
              <xs:attribute name=""CustomerID"" type=""xs:string"" />
              <xs:attribute name=""CompanyName"" type=""xs:string"" />
              <xs:attribute name=""ContactName"" type=""xs:string"" />
            </xs:complexType>
          </xs:element>
        </xs:choice>
      </xs:complexType>
    </xs:element>
  </xs:schema>
  <MyDataTable CustomerID=""VINET"" CompanyName=""Vins et alcools Chevalier"" ContactName=""Paul Henriot"" />
</rdData>";
			DataSet set;
			set = new DataSet ();
			set.ReadXml (new StringReader (SerializedDataTable));

			StringWriter w = new StringWriter ();
			set.WriteXml (w, XmlWriteMode.WriteSchema);
			string result = w.ToString ();
			Assert.AreEqual (expected.Replace ("\r", ""), result.Replace ("\r", ""));
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
                        Assert.AreEqual (
                                prevDs.Tables[0].Rows [0][0,DataRowVersion.Original].ToString (), 
				ds.Tables[0].Rows [0][0,DataRowVersion.Original].ToString (),
				"deserialization after modification does not give original values");
                        Assert.AreEqual (
                                prevDs.Tables[0].Rows [0][0,DataRowVersion.Current].ToString (), 
				ds.Tables[0].Rows [0][0,DataRowVersion.Current].ToString (),
				"deserialization after modification oes not give current values");
                }

		[Test]
		public void Bug420862 ()
		{
			DataSet ds = new DataSet ("d");
			DataTable dt = ds.Tables.Add ("t");
			dt.Columns.Add ("c", typeof (ushort));

			XmlSchema xs = XmlSchema.Read (new StringReader (ds.GetXmlSchema ()), null);
			xs.Compile (null);

			// follow the nesting of the schema in the foreach
			foreach (XmlSchemaElement d in xs.Items) {
				Assert.AreEqual ("d", d.Name);
				XmlSchemaChoice dsc = (XmlSchemaChoice) ((XmlSchemaComplexType) d.SchemaType).Particle;
				foreach (XmlSchemaElement t in dsc.Items) {
					Assert.AreEqual ("t", t.Name);
					XmlSchemaSequence tss = (XmlSchemaSequence) ((XmlSchemaComplexType) t.SchemaType).Particle;
					foreach (XmlSchemaElement c in tss.Items) {
						Assert.AreEqual ("c", c.Name);
						Assert.AreEqual ("unsignedShort", c.SchemaTypeName.Name);
						return;
					}
				}
			}
			Assert.Fail ();
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
                        Assert.AreEqual (0, parent.Rows.Count, "parent table rows should not exist!");
                        Assert.AreEqual (0, child.Rows.Count, "child table rows should not exist!");
                }

		[Test]
		public void CloneSubClassTest()
		{
			MyDataSet ds1 = new MyDataSet();
                        MyDataSet ds = (MyDataSet)(ds1.Clone());
                     	Assert.AreEqual (2, MyDataSet.count, "A#01");
		}

		#region DataSet.GetChanges Tests
		public void GetChanges_Relations_DifferentRowStatesTest ()
		{
			DataSet ds = new DataSet ("ds");
			DataTable parent = ds.Tables.Add ("parent");
			DataTable child = ds.Tables.Add ("child");
			
			parent.Columns.Add ("id", typeof (int));
			parent.Columns.Add ("name", typeof (string));
			

			child.Columns.Add ("id", typeof (int));
			child.Columns.Add ("parent", typeof (int));
			child.Columns.Add ("name", typeof (string));

			parent.Rows.Add (new object [] { 1, "mono parent 1" } );
			parent.Rows.Add (new object [] { 2, "mono parent 2" } );
			parent.Rows.Add (new object [] { 3, "mono parent 3" } );
			parent.Rows.Add (new object [] { 4, "mono parent 4" } );
			parent.AcceptChanges ();

			child.Rows.Add (new object [] { 1, 1, "mono child 1" } );
			child.Rows.Add (new object [] { 2, 2, "mono child 2" } );
			child.Rows.Add (new object [] { 3, 3, "mono child 3" } );
			child.AcceptChanges ();

			DataRelation relation = ds.Relations.Add ("parent_child", 
								  parent.Columns ["id"],
								  child.Columns ["parent"]);
			
			// modify the parent and get changes
			child.Rows [1]["parent"] = 4;
			DataSet changes = ds.GetChanges ();
			DataRow row = changes.Tables ["parent"].Rows[0];
			Assert.AreEqual ((int) parent.Rows [3][0], (int) row [0], "#RT1");
			Assert.AreEqual (1, changes.Tables ["parent"].Rows.Count, "#RT2 only get parent row with current version");
			ds.RejectChanges ();

			// delete a child row and get changes.
			child.Rows [0].Delete ();
			changes = ds.GetChanges ();
			
			Assert.AreEqual (changes.Tables.Count, 2, "#RT3 Should import parent table as well");
			Assert.AreEqual (1, changes.Tables ["parent"].Rows.Count, "#RT4 only get parent row with original version");
			Assert.AreEqual (1, (int) changes.Tables ["parent"].Rows [0][0], "#RT5 parent row based on original version");
		}
		#endregion // DataSet.GetChanges Tests

		[Test]
		public void RuleTest ()
		{
			DataSet ds = new DataSet ("testds");
			DataTable parent = ds.Tables.Add ("parent");
			DataTable child = ds.Tables.Add ("child");
			
			parent.Columns.Add ("id", typeof (int));
			parent.Columns.Add ("name", typeof (string));
			parent.PrimaryKey = new DataColumn [] {parent.Columns ["id"]} ;

			child.Columns.Add ("id", typeof (int));
			child.Columns.Add ("parent", typeof (int));
			child.Columns.Add ("name", typeof (string));
			child.PrimaryKey = new DataColumn [] {child.Columns ["id"]} ;

			DataRelation relation = ds.Relations.Add ("parent_child", 
								  parent.Columns ["id"],
								  child.Columns ["parent"]);

			parent.Rows.Add (new object [] {1, "mono test 1"});
			parent.Rows.Add (new object [] {2, "mono test 2"});
			parent.Rows.Add (new object [] {3, "mono test 3"});
			
			child.Rows.Add (new object [] {1, 1, "mono child test 1"});
			child.Rows.Add (new object [] {2, 2, "mono child test 2"});
			child.Rows.Add (new object [] {3, 3, "mono child test 3"});
			
			ds.AcceptChanges ();
			
			parent.Rows [0] ["name"] = "mono changed test 1";
			
			Assert.AreEqual (DataRowState.Unchanged, parent.Rows [0].GetChildRows (relation) [0].RowState,
					 "#RT1 child should not be modified");

			ds.RejectChanges ();
			parent.Rows [0] ["id"] = "4";

			DataRow childRow =  parent.Rows [0].GetChildRows (relation) [0];
			Assert.AreEqual (DataRowState.Modified, childRow.RowState, "#RT2 child should be modified");
			Assert.AreEqual (4, (int) childRow ["parent"], "#RT3 child should point to modified row");
		}

		[Test] // from bug #76480
		public void WriteXmlEscapeName ()
		{
			// create dataset
			DataSet data = new DataSet();

			DataTable mainTable = data.Tables.Add ("main");
			DataColumn mainkey = mainTable.Columns.Add ("mainkey", typeof(Guid));
			mainTable.Columns.Add ("col.2<hi/>", typeof (string));
			mainTable.Columns.Add ("#col3", typeof (string));

			// populate data
			mainTable.Rows.Add (new object [] { Guid.NewGuid (), "hi there", "my friend" } );
			mainTable.Rows.Add (new object [] { Guid.NewGuid (), "what is", "your name" } );
			mainTable.Rows.Add (new object [] { Guid.NewGuid (), "I have", "a bean" } );

			// write xml
			StringWriter writer = new StringWriter ();
			data.WriteXml (writer, XmlWriteMode.WriteSchema);
			string xml = writer.ToString ();
			Assert.IsTrue (xml.IndexOf ("name=\"col.2_x003C_hi_x002F__x003E_\"") > 0, "#1");
			Assert.IsTrue (xml.IndexOf ("name=\"_x0023_col3\"") > 0, "#2");
			Assert.IsTrue (xml.IndexOf ("<col.2_x003C_hi_x002F__x003E_>hi there</col.2_x003C_hi_x002F__x003E_>") > 0, "#3");

			// read xml
			DataSet data2 = new DataSet();
			data2.ReadXml (new StringReader (
				writer.GetStringBuilder ().ToString ()));
		}

#if NET_2_0

		// it is basically a test for XmlSerializer, but I need it
		// here to not add dependency on sys.data.dll in sys.xml test.
		[Test]
		public void ReflectTypedDataSet ()
		{
			XmlReflectionImporter imp = new XmlReflectionImporter ();
			// it used to cause "missing GetDataSetSchema" error.
			imp.ImportTypeMapping (typeof (MonkeyDataSet));
		}

		#region DataSet.CreateDataReader Tests and DataSet.Load Tests

		private DataSet ds;
		private DataTable dt1, dt2;

		private void localSetup () {
			ds = new DataSet ("test");
			dt1 = new DataTable ("test1");
			dt1.Columns.Add ("id1", typeof (int));
			dt1.Columns.Add ("name1", typeof (string));
			//dt1.PrimaryKey = new DataColumn[] { dt1.Columns["id"] };
			dt1.Rows.Add (new object[] { 1, "mono 1" });
			dt1.Rows.Add (new object[] { 2, "mono 2" });
			dt1.Rows.Add (new object[] { 3, "mono 3" });
			dt1.AcceptChanges ();
			dt2 = new DataTable ("test2");
			dt2.Columns.Add ("id2", typeof (int));
			dt2.Columns.Add ("name2", typeof (string));
			dt2.Columns.Add ("name3", typeof (string));
			//dt2.PrimaryKey = new DataColumn[] { dt2.Columns["id"] };
			dt2.Rows.Add (new object[] { 4, "mono 4", "four" });
			dt2.Rows.Add (new object[] { 5, "mono 5", "five" });
			dt2.Rows.Add (new object[] { 6, "mono 6", "six" });
			dt2.AcceptChanges ();
			ds.Tables.Add (dt1);
			ds.Tables.Add (dt2);
			ds.AcceptChanges ();
		}

		[Test]
		public void CreateDataReader1 () {
			// For First CreateDataReader Overload
			localSetup ();
			DataTableReader dtr = ds.CreateDataReader ();
			Assert.IsTrue (dtr.HasRows, "HasRows");
			int ti = 0;
			do {
				Assert.AreEqual (ds.Tables[ti].Columns.Count, dtr.FieldCount, "CountCols-" + ti);
				int ri = 0;
				while (dtr.Read ()) {
					for (int i = 0; i < dtr.FieldCount; i++)
						Assert.AreEqual (ds.Tables[ti].Rows[ri][i], dtr[i], "RowData-"+ti+"-"+ri+"-"+i);
					ri++;
				}
				ti++;
			} while (dtr.NextResult ());
		}

		[Test]
		public void CreateDataReader2 () {
			// For Second CreateDataReader Overload -
			// compare to ds.Tables
			localSetup ();
			DataTableReader dtr = ds.CreateDataReader (dt1, dt2);
			Assert.IsTrue (dtr.HasRows, "HasRows");
			int ti = 0;
			do {
			        Assert.AreEqual (ds.Tables[ti].Columns.Count, dtr.FieldCount, "CountCols-" + ti);
			        int ri = 0;
			        while (dtr.Read ()) {
			                for (int i = 0; i < dtr.FieldCount; i++)
			                        Assert.AreEqual (ds.Tables[ti].Rows[ri][i], dtr[i], "RowData-" + ti + "-" + ri + "-" + i);
			                ri++;
			        }
			        ti++;
			} while (dtr.NextResult ());
		}

		[Test]
		public void CreateDataReader3 () {
			// For Second CreateDataReader Overload -
			// compare to dt1 and dt2
			localSetup ();
			ds.Tables.Clear ();
			DataTableReader dtr = ds.CreateDataReader (dt1, dt2);
			Assert.IsTrue (dtr.HasRows, "HasRows");
			string name = "dt1";
			DataTable dtn = dt1;
			do {
			        Assert.AreEqual (dtn.Columns.Count, dtr.FieldCount, "CountCols-" + name);
			        int ri = 0;
			        while (dtr.Read ()) {
			                for (int i = 0; i < dtr.FieldCount; i++)
			                        Assert.AreEqual (dtn.Rows[ri][i], dtr[i], "RowData-" + name + "-" + ri + "-" + i);
			                ri++;
			        }
				if (dtn == dt1) {
					dtn = dt2;
					name = "dt2";
				} else {
					dtn = null;
					name = null;
				}
			} while (dtr.NextResult ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateDataReaderNoTable () {
			DataSet dsr = new DataSet ();
			DataTableReader dtr = dsr.CreateDataReader ();
		}

		internal struct fillErrorStruct {
			internal string error;
			internal string tableName;
			internal int rowKey;
			internal bool contFlag;
			internal void init (string tbl, int row, bool cont, string err) {
				tableName = tbl;
				rowKey = row;
				contFlag = cont;
				error = err;
			}
		}
		private fillErrorStruct[] fillErr = new fillErrorStruct[3];
		private int fillErrCounter;
		private void fillErrorHandler (object sender, FillErrorEventArgs e) {
			e.Continue = fillErr[fillErrCounter].contFlag;
			Assert.AreEqual (fillErr[fillErrCounter].tableName, e.DataTable.TableName, "fillErr-T");
			Assert.AreEqual (fillErr[fillErrCounter].contFlag, e.Continue, "fillErr-C");
			fillErrCounter++;
		}

		[Test]
		public void Load_Basic () {
			localSetup ();
			DataSet dsLoad = new DataSet ("LoadBasic");
			DataTable table1 = new DataTable ();
			dsLoad.Tables.Add (table1);
			DataTable table2 = new DataTable ();
			dsLoad.Tables.Add (table2);
			DataTableReader dtr = ds.CreateDataReader ();
			dsLoad.Load (dtr, LoadOption.OverwriteChanges, table1, table2);
			CompareTables (dsLoad);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Load_TableUnknown () {
			localSetup ();
			DataSet dsLoad = new DataSet ("LoadTableUnknown");
			DataTable table1 = new DataTable ();
			dsLoad.Tables.Add (table1);
			DataTable table2 = new DataTable ();
			// table2 is not added to dsLoad [dsLoad.Tables.Add (table2);]
			DataTableReader dtr = ds.CreateDataReader ();
			dsLoad.Load (dtr, LoadOption.OverwriteChanges, table1, table2);
		}

		[Test]
		public void Load_TableConflictT () {
			fillErrCounter = 0;
			fillErr[0].init ("Table1", 1, true,
				"Input string was not in a correct format.Couldn't store <mono 1> in name1 Column.  Expected type is Double.");
			fillErr[1].init ("Table1", 2, true,
				"Input string was not in a correct format.Couldn't store <mono 2> in name1 Column.  Expected type is Double.");
			fillErr[2].init ("Table1", 3, true,
				"Input string was not in a correct format.Couldn't store <mono 3> in name1 Column.  Expected type is Double.");
			localSetup ();
			DataSet dsLoad = new DataSet ("LoadTableConflict");
			DataTable table1 = new DataTable ();
			table1.Columns.Add ("name1", typeof (double));
			dsLoad.Tables.Add (table1);
			DataTable table2 = new DataTable ();
			dsLoad.Tables.Add (table2);
			DataTableReader dtr = ds.CreateDataReader ();
			dsLoad.Load (dtr, LoadOption.PreserveChanges,
				     fillErrorHandler, table1, table2);
		}
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Load_TableConflictF () {
			fillErrCounter = 0;
			fillErr[0].init ("Table1", 1, false,
				"Input string was not in a correct format.Couldn't store <mono 1> in name1 Column.  Expected type is Double.");
			localSetup ();
			DataSet dsLoad = new DataSet ("LoadTableConflict");
			DataTable table1 = new DataTable ();
			table1.Columns.Add ("name1", typeof (double));
			dsLoad.Tables.Add (table1);
			DataTable table2 = new DataTable ();
			dsLoad.Tables.Add (table2);
			DataTableReader dtr = ds.CreateDataReader ();
			dsLoad.Load (dtr, LoadOption.Upsert,
				     fillErrorHandler, table1, table2);
		}

		[Test]
		public void Load_StringsAsc () {
			localSetup ();
			DataSet dsLoad = new DataSet ("LoadStrings");
			DataTable table1 = new DataTable ("First");
			dsLoad.Tables.Add (table1);
			DataTable table2 = new DataTable ("Second");
			dsLoad.Tables.Add (table2);
			DataTableReader dtr = ds.CreateDataReader ();
			dsLoad.Load (dtr, LoadOption.OverwriteChanges, "First", "Second");
			CompareTables (dsLoad);
		}

		[Test]
		public void Load_StringsDesc () {
			localSetup ();
			DataSet dsLoad = new DataSet ("LoadStrings");
			DataTable table1 = new DataTable ("First");
			dsLoad.Tables.Add (table1);
			DataTable table2 = new DataTable ("Second");
			dsLoad.Tables.Add (table2);
			DataTableReader dtr = ds.CreateDataReader ();
			dsLoad.Load (dtr, LoadOption.PreserveChanges, "Second", "First");
			Assert.AreEqual (2, dsLoad.Tables.Count, "Tables");
			Assert.AreEqual (3, dsLoad.Tables[0].Rows.Count, "T1-Rows");
			Assert.AreEqual (3, dsLoad.Tables[0].Columns.Count, "T1-Columns");
			Assert.AreEqual (3, dsLoad.Tables[1].Rows.Count, "T2-Rows");
			Assert.AreEqual (2, dsLoad.Tables[1].Columns.Count, "T2-Columns");
		}

		[Test]
		public void Load_StringsNew () {
			localSetup ();
			DataSet dsLoad = new DataSet ("LoadStrings");
			DataTable table1 = new DataTable ("First");
			dsLoad.Tables.Add (table1);
			DataTable table2 = new DataTable ("Second");
			dsLoad.Tables.Add (table2);
			DataTableReader dtr = ds.CreateDataReader ();
			dsLoad.Load (dtr, LoadOption.Upsert, "Third", "Fourth");
			Assert.AreEqual (4, dsLoad.Tables.Count, "Tables");
			Assert.AreEqual ("First", dsLoad.Tables[0].TableName, "T1-Name");
			Assert.AreEqual (0, dsLoad.Tables[0].Rows.Count, "T1-Rows");
			Assert.AreEqual (0, dsLoad.Tables[0].Columns.Count, "T1-Columns");
			Assert.AreEqual ("Second", dsLoad.Tables[1].TableName, "T2-Name");
			Assert.AreEqual (0, dsLoad.Tables[1].Rows.Count, "T2-Rows");
			Assert.AreEqual (0, dsLoad.Tables[1].Columns.Count, "T2-Columns");
			Assert.AreEqual ("Third", dsLoad.Tables[2].TableName, "T3-Name");
			Assert.AreEqual (3, dsLoad.Tables[2].Rows.Count, "T3-Rows");
			Assert.AreEqual (2, dsLoad.Tables[2].Columns.Count, "T3-Columns");
			Assert.AreEqual ("Fourth", dsLoad.Tables[3].TableName, "T4-Name");
			Assert.AreEqual (3, dsLoad.Tables[3].Rows.Count, "T4-Rows");
			Assert.AreEqual (3, dsLoad.Tables[3].Columns.Count, "T4-Columns");
		}

		[Test]
		public void Load_StringsNewMerge () {
			localSetup ();
			DataSet dsLoad = new DataSet ("LoadStrings");
			DataTable table1 = new DataTable ("First");
			table1.Columns.Add ("col1", typeof (string));
			table1.Rows.Add (new object[] { "T1Row1" });
			dsLoad.Tables.Add (table1);
			DataTable table2 = new DataTable ("Second");
			table2.Columns.Add ("col2", typeof (string));
			table2.Rows.Add (new object[] { "T2Row1" });
			table2.Rows.Add (new object[] { "T2Row2" });
			dsLoad.Tables.Add (table2);
			DataTableReader dtr = ds.CreateDataReader ();
			dsLoad.Load (dtr, LoadOption.OverwriteChanges, "Third", "First");
			Assert.AreEqual (3, dsLoad.Tables.Count, "Tables");
			Assert.AreEqual ("First", dsLoad.Tables[0].TableName, "T1-Name");
			Assert.AreEqual (4, dsLoad.Tables[0].Rows.Count, "T1-Rows");
			Assert.AreEqual (4, dsLoad.Tables[0].Columns.Count, "T1-Columns");
			Assert.AreEqual ("Second", dsLoad.Tables[1].TableName, "T2-Name");
			Assert.AreEqual (2, dsLoad.Tables[1].Rows.Count, "T2-Rows");
			Assert.AreEqual (1, dsLoad.Tables[1].Columns.Count, "T2-Columns");
			Assert.AreEqual ("Third", dsLoad.Tables[2].TableName, "T3-Name");
			Assert.AreEqual (3, dsLoad.Tables[2].Rows.Count, "T3-Rows");
			Assert.AreEqual (2, dsLoad.Tables[2].Columns.Count, "T3-Columns");
		}

		[Test]
		public void ReadDiff ()
		{
			DataSet dsTest = new DataSet ("MonoTouchTest");
			var dt = new DataTable ("123");
			dt.Columns.Add (new DataColumn ("Value1"));
			dt.Columns.Add (new DataColumn ("Value2"));
			dsTest.Tables.Add (dt);
			dsTest.ReadXml (new StringReader (@"
<diffgr:diffgram
   xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'
   xmlns:diffgr='urn:schemas-microsoft-com:xml-diffgram-v1'>
  <MonoTouchTest>
    <_x0031_23 diffgr:id='1231' msdata:rowOrder='0'>
      <Value1>Row1Value1</Value1>
      <Value2>Row1Value2</Value2>
    </_x0031_23>
  </MonoTouchTest>
</diffgr:diffgram>
"));
			Assert.AreEqual ("123", dsTest.Tables [0].TableName, "#1");
			Assert.AreEqual (1, dsTest.Tables [0].Rows.Count, "#2");
		}

		private void CompareTables (DataSet dsLoad) {
			Assert.AreEqual (ds.Tables.Count, dsLoad.Tables.Count, "NumTables");
			for (int tc = 0; tc < dsLoad.Tables.Count; tc++) {
				Assert.AreEqual (ds.Tables[tc].Columns.Count, dsLoad.Tables[tc].Columns.Count, "Table" + tc + "-NumCols");
				Assert.AreEqual (ds.Tables[tc].Rows.Count, dsLoad.Tables[tc].Rows.Count, "Table" + tc + "-NumRows");
				for (int cc = 0; cc < dsLoad.Tables[tc].Columns.Count; cc++) {
					Assert.AreEqual (ds.Tables[tc].Columns[cc].ColumnName,
							 dsLoad.Tables[tc].Columns[cc].ColumnName,
							 "Table" + tc + "-" + "Col" + cc + "-Name");
				}
				for (int rc = 0; rc < dsLoad.Tables[tc].Rows.Count; rc++) {
					for (int cc = 0; cc < dsLoad.Tables[tc].Columns.Count; cc++) {
						Assert.AreEqual (ds.Tables[tc].Rows[rc].ItemArray[cc],
								 dsLoad.Tables[tc].Rows[rc].ItemArray[cc],
								 "Table" + tc + "-Row" + rc + "-Col" + cc + "-Data");
					}
				}
			}
		}

		#endregion // DataSet.CreateDataReader Tests and DataSet.Load Tests
#endif

	}

	 public  class MyDataSet:DataSet {

	     public static int count = 0;
                                                                                                    
             public MyDataSet() {

                    count++;
             }
                                                                                                    
         }
	

}
