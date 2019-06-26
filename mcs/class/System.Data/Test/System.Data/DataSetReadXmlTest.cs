//
// DataSetReadXmlTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataSetReadXmlTest : DataSetAssertion
	{
		const string xml1 = "";
		const string xml2 = "<root/>";
		const string xml3 = "<root></root>";
		const string xml4 = "<root>   </root>";
		const string xml5 = "<root>test</root>";
		const string xml6 = "<root><test>1</test></root>";
		const string xml7 = "<root><test>1</test><test2>a</test2></root>";
		const string xml8 = "<dataset><table><col1>foo</col1><col2>bar</col2></table></dataset>";
		const string xml29 = @"<PersonalSite><License Name='Sum Wang' Email='sumwang@somewhere.net' Mode='Trial' StartDate='01/01/2004' Serial='aaa' /></PersonalSite>";

		const string diff1 = @"<diffgr:diffgram xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' xmlns:diffgr='urn:schemas-microsoft-com:xml-diffgram-v1'>
  <NewDataSet>
    <Table1 diffgr:id='Table11' msdata:rowOrder='0' diffgr:hasChanges='inserted'>
      <Column1_1>ppp</Column1_1>
      <Column1_2>www</Column1_2>
      <Column1_3>xxx</Column1_3>
    </Table1>
  </NewDataSet>
</diffgr:diffgram>";
		const string diff2 = diff1 + xml8;

		const string schema1 = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	<xs:element name='Root'>
		<xs:complexType>
			<xs:sequence>
				<xs:element name='Child' type='xs:string' />
			</xs:sequence>
		</xs:complexType>
	</xs:element>
</xs:schema>";
		const string schema2 = schema1 + xml8;

		[Test]
		public void ReadSimpleAuto ()
		{
			DataSet ds;

			// empty XML
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.Auto, XmlReadMode.Auto,
				"NewDataSet", 0);

			// simple element
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"root", 0);

			// simple element2
			ds = new DataSet ();
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"root", 0);

			// whitespace in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"root", 0);

			// text in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"root", 0);

			// simple table pattern:
			// root becomes a table and test becomes a column.
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable ("xml6", ds.Tables [0], "root", 1, 1, 0, 0, 0, 0);

			// simple table with 2 columns:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable ("xml7", ds.Tables [0], "root", 2, 1, 0, 0, 0, 0);

			// simple dataset with 1 table:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"dataset", 1);
			AssertDataTable ("xml8", ds.Tables [0], "table", 2, 1, 0, 0, 0, 0);
		}

		[Test]
		public void ReadSimpleDiffgram ()
		{
			DataSet ds;

			// empty XML
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// simple element
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// simple element2
			ds = new DataSet ();
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// whitespace in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// text in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// simple table pattern:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// simple table with 2 columns:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// simple dataset with 1 table:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);
		}

		[Test]
		public void ReadSimpleFragment ()
		{
			DataSet ds;

			// empty XML
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// simple element
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// simple element2
			ds = new DataSet ();
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// whitespace in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// text in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// simple table pattern:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// simple table with 2 columns:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// simple dataset with 1 table:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);
		}

		[Test]
		public void ReadSimpleIgnoreSchema ()
		{
			DataSet ds;

			// empty XML
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// simple element
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// simple element2
			ds = new DataSet ();
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// whitespace in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// text in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// simple table pattern:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// simple table with 2 columns:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// simple dataset with 1 table:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);
		}

		[Test]
		public void ReadSimpleInferSchema ()
		{
			DataSet ds;

			// empty XML
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// simple element
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"root", 0);

			// simple element2
			ds = new DataSet ();
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"root", 0);

			// whitespace in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"root", 0);

			// text in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"root", 0);

			// simple table pattern:
			// root becomes a table and test becomes a column.
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable ("xml6", ds.Tables [0], "root", 1, 1, 0, 0, 0, 0);

			// simple table with 2 columns:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable ("xml7", ds.Tables [0], "root", 2, 1, 0, 0, 0, 0);

			// simple dataset with 1 table:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"dataset", 1);
			AssertDataTable ("xml8", ds.Tables [0], "table", 2, 1, 0, 0, 0, 0);
		}

		[Test]
		public void ReadSimpleReadSchema ()
		{
			DataSet ds;

			// empty XML
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// simple element
			ds = new DataSet ();
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// simple element2
			ds = new DataSet ();
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// whitespace in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// text in simple element
			ds = new DataSet ();
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// simple table pattern:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// simple table with 2 columns:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// simple dataset with 1 table:
			ds = new DataSet ();
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);
		}

		[Test]
		public void TestSimpleDiffXmlAll ()
		{
			DataSet ds;

			// ignored
			ds = new DataSet ();
			AssertReadXml (ds, "Fragment", diff1,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			ds = new DataSet ();
			AssertReadXml (ds, "IgnoreSchema", diff1,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			ds = new DataSet ();
			AssertReadXml (ds, "InferSchema", diff1,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			ds = new DataSet ();
			AssertReadXml (ds, "ReadSchema", diff1,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// Auto, DiffGram ... treated as DiffGram
			ds = new DataSet ();
			AssertReadXml (ds, "Auto", diff1,
				XmlReadMode.Auto, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			ds = new DataSet ();
			AssertReadXml (ds, "DiffGram", diff1,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);
		}

		[Test]
		public void TestSimpleDiffPlusContentAll ()
		{
			DataSet ds;

			// Fragment ... skipped
			ds = new DataSet ();
			AssertReadXml (ds, "Fragment", diff2,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// others ... kept 
			ds = new DataSet ();
			AssertReadXml (ds, "IgnoreSchema", diff2,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0, ReadState.Interactive);

			ds = new DataSet ();
			AssertReadXml (ds, "InferSchema", diff2,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0, ReadState.Interactive);

			ds = new DataSet ();
			AssertReadXml (ds, "ReadSchema", diff2,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0, ReadState.Interactive);

			// Auto, DiffGram ... treated as DiffGram
			ds = new DataSet ();
			AssertReadXml (ds, "Auto", diff2,
				XmlReadMode.Auto, XmlReadMode.DiffGram,
				"NewDataSet", 0, ReadState.Interactive);

			ds = new DataSet ();
			AssertReadXml (ds, "DiffGram", diff2,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0, ReadState.Interactive);
		}

		[Test]
		public void TestSimpleSchemaXmlAll ()
		{
			DataSet ds;

			// ignored
			ds = new DataSet ();
			AssertReadXml (ds, "IgnoreSchema", schema1,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			ds = new DataSet ();
			AssertReadXml (ds, "InferSchema", schema1,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// misc ... consume schema
			ds = new DataSet ();
			AssertReadXml (ds, "Fragment", schema1,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 1);
			AssertDataTable ("fragment", ds.Tables [0], "Root", 1, 0, 0, 0, 0, 0);

			ds = new DataSet ();
			AssertReadXml (ds, "ReadSchema", schema1,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 1);
			AssertDataTable ("readschema", ds.Tables [0], "Root", 1, 0, 0, 0, 0, 0);

			ds = new DataSet ();
			AssertReadXml (ds, "Auto", schema1,
				XmlReadMode.Auto, XmlReadMode.ReadSchema,
				"NewDataSet", 1);
			AssertDataTable ("auto", ds.Tables [0], "Root", 1, 0, 0, 0, 0, 0);

			ds = new DataSet ();
			AssertReadXml (ds, "DiffGram", schema1,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 1);
		}

		[Test]
		public void TestSimpleSchemaPlusContentAll ()
		{
			DataSet ds;

			// ignored
			ds = new DataSet ();
			AssertReadXml (ds, "IgnoreSchema", schema2,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0, ReadState.Interactive);

			ds = new DataSet ();
			AssertReadXml (ds, "InferSchema", schema2,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0, ReadState.Interactive);

			// Fragment ... consumed both
			ds = new DataSet ();
			AssertReadXml (ds, "Fragment", schema2,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 1);
			AssertDataTable ("fragment", ds.Tables [0], "Root", 1, 0, 0, 0, 0, 0);

			// rest ... treated as schema
			ds = new DataSet ();
			AssertReadXml (ds, "Auto", schema2,
				XmlReadMode.Auto, XmlReadMode.ReadSchema,
				"NewDataSet", 1, ReadState.Interactive);
			AssertDataTable ("auto", ds.Tables [0], "Root", 1, 0, 0, 0, 0, 0);

			ds = new DataSet ();
			AssertReadXml (ds, "DiffGram", schema2,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 1, ReadState.Interactive);
			AssertDataTable ("diffgram", ds.Tables [0], "Root", 1, 0, 0, 0, 0, 0);

			ds = new DataSet ();
			AssertReadXml (ds, "ReadSchema", schema2,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 1, ReadState.Interactive);
		}

		[Test]
		public void SequentialRead1 ()
		{
			// simple element -> simple table
			DataSet ds = new DataSet ();

			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"root", 0);

			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable ("seq1", ds.Tables [0], "root", 1, 1, 0, 0, 0, 0);
		}

		[Test]
		public void SequentialRead2 ()
		{
			// simple element -> simple dataset
			DataSet ds = new DataSet ();

			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"root", 0);

			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable ("#1", ds.Tables [0], "root", 2, 1, 0, 0, 0, 0);

			// simple table -> simple dataset
			ds = new DataSet ();

			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable ("#2", ds.Tables [0], "root", 1, 1, 0, 0, 0, 0);

			// Return value became IgnoreSchema, since there is
			// already schema information in the dataset.
			// Columns are kept 1 as old table holds.
			// Rows are up to 2 because of accumulative read.
			AssertReadXml (ds, "SimpleTable2-2", xml7,
				XmlReadMode.Auto, XmlReadMode.IgnoreSchema,
				"NewDataSet", 1);
			AssertDataTable ("#3", ds.Tables [0], "root", 1, 2, 0, 0, 0, 0);

		}

		[Test] // based on bug case
		public void ReadComplexElementDocument ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (xml29));
		}

		[Test]
		public void IgnoreSchemaShouldFillData ()
		{
			// no such dataset
			string xml1 = "<set><tab><col>test</col></tab></set>";
			// no wrapper element
			string xml2 = "<tab><col>test</col></tab>";
			// no such table
			string xml3 = "<tar><col>test</col></tar>";
			DataSet ds = new DataSet ();
			DataTable dt = new DataTable ("tab");
			ds.Tables.Add (dt);
			dt.Columns.Add ("col");
			ds.ReadXml (new StringReader (xml1), XmlReadMode.IgnoreSchema);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			Assert.AreEqual (1, dt.Rows.Count, "wrapper element");
			dt.Clear ();

			ds.ReadXml (new StringReader (xml2), XmlReadMode.IgnoreSchema);
			Assert.AreEqual (1, dt.Rows.Count, "no wrapper element");
			dt.Clear ();

			ds.ReadXml (new StringReader (xml3), XmlReadMode.IgnoreSchema);
			Assert.AreEqual (0, dt.Rows.Count, "no such table");
		}

		// bug #60118
		[Test]
		public void NameConflictDSAndTable ()
		{
			string xml = @"<PriceListDetails> 
	<PriceListList>    
		<Id>1</Id>
	</PriceListList>
	<PriceListDetails> 
		<Id>1</Id>
		<Status>0</Status>
	</PriceListDetails>
</PriceListDetails>";

			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (xml));
			Assert.IsNotNull (ds.Tables ["PriceListDetails"]);
		}

		[Test] // bug #80045
		public void ColumnOrder ()
		{
			string xml = "<?xml version=\"1.0\" standalone=\"yes\"?>" +
				"<NewDataSet>" +
				"  <Table>" +
				"    <Name>Miguel</Name>" +
				"    <FirstName>de Icaza</FirstName>" +
				"    <Income>4000</Income>" +
				"  </Table>" +
				"  <Table>" +
				"    <Name>25</Name>" +
				"    <FirstName>250</FirstName>" +
				"    <Address>Belgium</Address>" +
				"    <Income>5000</Income>" +
				"</Table>" +
				"</NewDataSet>";

			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (xml));
			Assert.AreEqual (1, ds.Tables.Count, "#1");
			Assert.AreEqual ("Table", ds.Tables [0].TableName, "#2");
			Assert.AreEqual (4, ds.Tables [0].Columns.Count, "#3");
			Assert.AreEqual ("Name", ds.Tables [0].Columns [0].ColumnName, "#4a");
			Assert.AreEqual (0, ds.Tables [0].Columns [0].Ordinal, "#4b");
			Assert.AreEqual ("FirstName", ds.Tables [0].Columns [1].ColumnName, "#5a");
			Assert.AreEqual (1, ds.Tables [0].Columns [1].Ordinal, "#5b");
			Assert.AreEqual ("Address", ds.Tables [0].Columns [2].ColumnName, "#6a");
			Assert.AreEqual (2, ds.Tables [0].Columns [2].Ordinal, "#6b");
			Assert.AreEqual ("Income", ds.Tables [0].Columns [3].ColumnName, "#7a");
			Assert.AreEqual (3, ds.Tables [0].Columns [3].Ordinal, "#7b");
		}

		[Test] // bug #80048
		public void XmlSpace ()
		{
			string xml = "<?xml version=\"1.0\" standalone=\"yes\"?>" +
				"<NewDataSet>" +
				"  <Table>" +
				"    <Name>Miguel</Name>" +
				"    <FirstName xml:space=\"preserve\"> de Icaza</FirstName>" +
				"    <Income>4000</Income>" +
				"  </Table>" +
				"  <Table>" +
				"    <Name>Chris</Name>" +
				"    <FirstName xml:space=\"preserve\">Toshok </FirstName>" +
				"    <Income>3000</Income>" +
				"  </Table>" +
				"</NewDataSet>";

 			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (xml));
			Assert.AreEqual (1, ds.Tables.Count, "#1");
			Assert.AreEqual ("Table", ds.Tables [0].TableName, "#2");
			Assert.AreEqual (3, ds.Tables [0].Columns.Count, "#3");
			Assert.AreEqual ("Name", ds.Tables [0].Columns [0].ColumnName, "#4a");
			Assert.AreEqual (0, ds.Tables [0].Columns [0].Ordinal, "#4b");
			Assert.AreEqual ("FirstName", ds.Tables [0].Columns [1].ColumnName, "#5a");
			Assert.AreEqual (1, ds.Tables [0].Columns [1].Ordinal, "#5b");
			Assert.AreEqual ("Income", ds.Tables [0].Columns [2].ColumnName, "#6a");
			Assert.AreEqual (2, ds.Tables [0].Columns [2].Ordinal, "#6b");
		}

		public void TestSameParentChildName ()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><resource type=\"parent\">" +
			       	     "<resource type=\"child\" /></resource>";
 			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (xml));

			AssertReadXml (ds, "SameNameParentChild", xml,
				XmlReadMode.Auto, XmlReadMode.IgnoreSchema,
				"NewDataSet", 1);
		}

		public void TestSameColumnName ()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><resource resource_Id_0=\"parent\">" +
			       	     "<resource resource_Id_0=\"child\" /></resource>";
 			DataSet ds = new DataSet ();
			ds.ReadXml (new StringReader (xml));

			AssertReadXml (ds, "SameColumnName", xml,
				XmlReadMode.Auto, XmlReadMode.IgnoreSchema,
				"NewDataSet", 1);
		}
		
		[Test]
		public void DataSetExtendedPropertiesTest()
		{
			DataSet dataSet1 = new DataSet();
			dataSet1.ExtendedProperties.Add("DS1", "extended0");
			DataTable table = new DataTable("TABLE1");
			table.ExtendedProperties.Add("T1", "extended1");
			table.Columns.Add("C1", typeof(int));
			table.Columns.Add("C2", typeof(string));
			table.Columns[1].MaxLength = 20;
			table.Columns[0].ExtendedProperties.Add("C1Ext1", "extended2");
			table.Columns[1].ExtendedProperties.Add("C2Ext1", "extended3");
			dataSet1.Tables.Add(table);
			table.LoadDataRow(new object[]{1, "One"}, false);
			table.LoadDataRow(new object[]{2, "Two"}, false);
			string file = Path.Combine (Path.GetTempPath (), "schemas-test.xml");
			try {
				dataSet1.WriteXml (file, XmlWriteMode.WriteSchema);
			}
			catch (Exception ex) {
				Assert.Fail ("DSExtPropTest failed: WriteXml failed with : "+ex.Message);
			} finally {
				File.Delete (file);
			}
			
			DataSet dataSet2 = new DataSet();
			dataSet2.ReadXml(TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/b582732.xml"), XmlReadMode.ReadSchema);
			Assert.AreEqual (dataSet1.ExtendedProperties["DS1"], dataSet2.ExtendedProperties["DS1"],
			                 "DSExtProp#1: DS extended properties mismatch");
						
			Assert.AreEqual (dataSet1.Tables[0].ExtendedProperties["T1"], dataSet2.Tables[0].ExtendedProperties["T1"],
			                 "DSExtProp#2: DS Table extended properties mismatch");
			Assert.AreEqual (dataSet1.Tables[0].Columns[0].ExtendedProperties["C1Ext1"], 
			                 dataSet2.Tables[0].Columns[0].ExtendedProperties["C1Ext1"],
			                 "DSExtProp#3: DS Table Column 1 extended properties mismatch");
			Assert.AreEqual (dataSet1.Tables[0].Columns[1].ExtendedProperties["C2Ext1"], 
			                 dataSet2.Tables[0].Columns[1].ExtendedProperties["C2Ext1"],
			                 "DSExtProp#4: DS Table Column 2 extended properties mismatch");
		}
	}
}
