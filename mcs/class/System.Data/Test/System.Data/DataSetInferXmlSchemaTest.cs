//
// DataSetInferXmlSchemaTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataSetInferXmlSchemaTest : DataSetAssertion
	{
		string xml1 = "<root/>";
		string xml2 = "<root attr='value' />";
		string xml3 = "<root attr='value' attr2='2' />";
		string xml4 = "<root>simple.txt</root>";
		string xml5 = "<root><child/></root>";
		string xml6 = "<root><col1>sample</col1></root>";
		string xml7 = @"<root>
<col1>column 1 test</col1>
<col2>column2test</col2>
<col3>3get</col3>
</root>";
		string xml8 = @"<set>
<tab>
<col1>1</col1>
<col2>1</col2>
<col3>1</col3>
</tab>
</set>";
		string xml9 = @"<el1 attr1='val1' attrA='valA'>
  <el2 attr2='val2' attrB='valB'>
    <el3 attr3='val3' attrC='valC'>3</el3>
      <column2>1</column2>
      <column3>1</column3>
    <el4 attr4='val4' attrD='valD'>4</el4>
  </el2>
</el1>";
		string xml10 = "<root>Here is a <b>mixed</b> content.</root>";
		string xml11 = @"<root xml:space='preserve'>
   <child_after_significant_space />
</root>";
		// This is useless ... since xml:space becomes a DataColumn here.
//		string xml12 = "<root xml:space='preserve'>     </root>";
		// The result is silly under MS.NET. It never ignores comment, so
		// They differ:
		//   1) <root>simple string.</root>
		//   2) <root>simple <!-- comment -->string.</root>
		// The same applies to PI.
//		string xml13 = "<root><tab><col>test <!-- out --> comment</col></tab></root>";
		string xml14 = "<p:root xmlns:p='urn:foo'>test string</p:root>";
		string xml15 = @"<root>
<table1>
        <col1_1>test1</col1_1>
        <col1_2>test2</col1_2>
</table1>
<table2>
        <col2_1>test1</col2_1>
        <col2_2>test2</col2_2>
</table2>
</root>";
		string xml16 = @"<root>
<table>
        <foo>
                <tableFooChild1>1</tableFooChild1>
                <tableFooChild2>2</tableFooChild2>
        </foo>
        <bar />
</table>
<foo></foo>
<bar />
</root>";
		string xml17 = @"<root xmlns='urn:foo' />";
		string xml18 = @"<set>
<table>
<col>
        <another_col />
</col>
<col>simple text here.</col>
</table>
</set>";
		string xml19 =@"<set>
<table>
<col>simple text</col><!-- ignored -->
<col>
        <another_col />
</col>
</table>
</set>";
		string xml20 = @"<set>
<table>
<col>
        <another_col />
</col>
<col attr='value' />
</table>
</set>";
		string xml21 = @"<set>
<table>
<col data='value'>
        <data />
</col>
</table>
</set>";

		private DataSet GetDataSet (string xml, string [] nss)
		{
			DataSet ds = new DataSet ();
			ds.InferXmlSchema (new XmlTextReader (xml, XmlNodeType.Document, null), nss);
			return ds;
		}

		[Test]
		public void NullFileName ()
		{
			DataSet ds = new DataSet ();
			ds.InferXmlSchema ((XmlReader) null, null);
			AssertDataSet ("null", ds, "NewDataSet", 0);
		}

		[Test]
		public void SingleElement ()
		{
			DataSet ds = GetDataSet (xml1, null);
			AssertDataSet ("xml1", ds, "root", 0, 0);

			ds = GetDataSet (xml4, null);
			AssertDataSet ("xml4", ds, "root", 0, 0);

			// namespaces
			ds = GetDataSet (xml14, null);
			AssertDataSet ("xml14", ds, "root", 0, 0);
			AssertEquals ("p", ds.Prefix);
			AssertEquals ("urn:foo", ds.Namespace);

			ds = GetDataSet (xml17, null);
			AssertDataSet ("xml17", ds, "root", 0, 0);
			AssertEquals ("urn:foo", ds.Namespace);
		}

		[Test]
		public void SingleElementWithAttribute ()
		{
			DataSet ds = GetDataSet (xml2, null);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 1, 0);
			AssertDataColumn ("col", dt.Columns [0], "attr", true, false, 0, 1, "attr", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
		}

		[Test]
		public void SingleElementWithTwoAttribute ()
		{
			DataSet ds = GetDataSet (xml3, null);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 2, 0);
			AssertDataColumn ("col", dt.Columns [0], "attr", true, false, 0, 1, "attr", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("col", dt.Columns [1], "attr2", true, false, 0, 1, "attr2", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
		}

		[Test]
		public void SingleChild ()
		{
			DataSet ds = GetDataSet (xml5, null);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 1, 0);
			AssertDataColumn ("col", dt.Columns [0], "child", true, false, 0, 1, "child", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);

			ds = GetDataSet (xml6, null);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 1, 0);
			AssertDataColumn ("col", dt.Columns [0], "col1", true, false, 0, 1, "col1", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
		}

		[Test]
		public void SimpleElementTable ()
		{
			DataSet ds = GetDataSet (xml7, null);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 3, 0);
			AssertDataColumn ("col", dt.Columns [0], "col1", true, false, 0, 1, "col1", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("col2", dt.Columns [1], "col2", true, false, 0, 1, "col2", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
			AssertDataColumn ("col3", dt.Columns [2], "col3", true, false, 0, 1, "col3", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 2, String.Empty, false, false);
		}

		[Test]
		public void SimpleDataSet ()
		{
			DataSet ds = GetDataSet (xml8, null);
			AssertDataSet ("ds", ds, "set", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "tab", 3, 0);
			AssertDataColumn ("col", dt.Columns [0], "col1", true, false, 0, 1, "col1", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("col2", dt.Columns [1], "col2", true, false, 0, 1, "col2", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
			AssertDataColumn ("col3", dt.Columns [2], "col3", true, false, 0, 1, "col3", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 2, String.Empty, false, false);
		}

		[Test]
		public void ComplexElementAttributeTable1 ()
		{
			// FIXME: Also test ReadXml (, XmlReadMode.InferSchema) and
			// make sure that ReadXml() stores DataRow to el1 (and maybe to others)
			DataSet ds = GetDataSet (xml9, null);
			AssertDataSet ("ds", ds, "NewDataSet", 4, 0);
			DataTable dt = ds.Tables [0];

			AssertDataTable ("dt", dt, "el1", 3, 0);
			AssertDataColumn ("el1_Id", dt.Columns [0], "el1_Id", false, true, 0, 1, "el1_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, true);
			AssertDataColumn ("el1_attr1", dt.Columns [1], "attr1", true, false, 0, 1, "attr1", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
			AssertDataColumn ("el1_attrA", dt.Columns [2], "attrA", true, false, 0, 1, "attrA", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 2, String.Empty, false, false);

			dt = ds.Tables [1];
			AssertDataTable ("dt", dt, "el2", 6, 0);
			AssertDataColumn ("el2_Id", dt.Columns [0], "el2_Id", false, true, 0, 1, "el2_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, true);
			AssertDataColumn ("el2_col2", dt.Columns [1], "column2", true, false, 0, 1, "column2", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
			AssertDataColumn ("el2_col3", dt.Columns [2], "column3", true, false, 0, 1, "column3", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 2, String.Empty, false, false);
			AssertDataColumn ("el2_attr2", dt.Columns [3], "attr2", true, false, 0, 1, "attr2", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 3, String.Empty, false, false);
			AssertDataColumn ("el2_attrB", dt.Columns [4], "attrB", true, false, 0, 1, "attrB", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 4, String.Empty, false, false);
			AssertDataColumn ("el2_el1Id", dt.Columns [5], "el1_Id", true, false, 0, 1, "el1_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 5, String.Empty, false, false);

			dt = ds.Tables [2];
			AssertDataTable ("dt", dt, "el3", 4, 0);
			AssertDataColumn ("el3_attr3", dt.Columns [0], "attr3", true, false, 0, 1, "attr3", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("el3_attrC", dt.Columns [1], "attrC", true, false, 0, 1, "attrC", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
			AssertDataColumn ("el3_Text", dt.Columns [2], "el3_Text", true, false, 0, 1, "el3_Text", MappingType.SimpleContent, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 2, String.Empty, false, false);
			AssertDataColumn ("el3_el2Id", dt.Columns [3], "el2_Id", true, false, 0, 1, "el2_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 3, String.Empty, false, false);

			dt = ds.Tables [3];
			AssertDataTable ("dt", dt, "el4", 4, 0);
			AssertDataColumn ("el3_attr4", dt.Columns [0], "attr4", true, false, 0, 1, "attr4", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("el4_attrD", dt.Columns [1], "attrD", true, false, 0, 1, "attrD", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
			AssertDataColumn ("el4_Text", dt.Columns [2], "el4_Text", true, false, 0, 1, "el4_Text", MappingType.SimpleContent, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 2, String.Empty, false, false);
			AssertDataColumn ("el4_el2Id", dt.Columns [4], "el2_Id", true, false, 0, 1, "el2_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 3, String.Empty, false, false);
		}

		[Test]
		public void MixedContent ()
		{
			// Note that text part is ignored.

			DataSet ds = GetDataSet (xml10, null);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 1, 0);
			AssertDataColumn ("col", dt.Columns [0], "b", true, false, 0, 1, "b", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
		}

		[Test]
		public void SignificantWhitespaceIgnored ()
		{
			// Note that 1) significant whitespace is ignored, and 
			// 2) xml:space is treated as column (and also note namespaces).
			DataSet ds = GetDataSet (xml11, null);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 2, 0);
			AssertDataColumn ("element", dt.Columns [0], "child_after_significant_space", true, false, 0, 1, "child_after_significant_space", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("xml:space", dt.Columns [1], "space", true, false, 0, 1, "space", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, "http://www.w3.org/XML/1998/namespace", 1, "xml", false, false);
		}

		[Test]
		public void SignificantWhitespaceIgnored2 ()
		{
			// To make sure, create pure significant whitespace element
			// using XmlNodeReader (that does not have xml:space attribute
			// column).
			DataSet ds = new DataSet ();
			XmlDocument doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("root"));
			doc.DocumentElement.AppendChild (doc.CreateSignificantWhitespace
			("      \n\n"));
			XmlReader xr = new XmlNodeReader (doc);
			ds.InferXmlSchema (xr, null);
			AssertDataSet ("pure_whitespace", ds, "root", 0);
		}

		[Test]
		public void TwoElementTable ()
		{
			// FIXME: Also test ReadXml (, XmlReadMode.InferSchema) and
			// make sure that ReadXml() stores DataRow to el1 (and maybe to others)
			DataSet ds = GetDataSet (xml15, null);
			AssertDataSet ("ds", ds, "root", 2, 0);

			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "table1", 2, 0);
			AssertDataColumn ("col1_1", dt.Columns [0], "col1_1", true, false, 0, 1, "col1_1", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("col1_2", dt.Columns [1], "col1_2", true, false, 0, 1, "col1_2", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);

			dt = ds.Tables [1];
			AssertDataTable ("dt", dt, "table2", 2, 0);
			AssertDataColumn ("col2_1", dt.Columns [0], "col2_1", true, false, 0, 1, "col2_1", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("col2_2", dt.Columns [1], "col2_2", true, false, 0, 1, "col2_2", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		// The same table cannot be the child table in two nested relations.
		public void ComplexElementTable1 ()
		{
			// TODO: Also test ReadXml (, XmlReadMode.InferSchema) and
			// make sure that ReadXml() stores DataRow to el1 (and maybe to others)
			DataSet ds = GetDataSet (xml16, null);
/*
			AssertDataSet ("ds", ds, "NewDataSet", 4, 0);

			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 2, 0);
			AssertDataColumn ("table#1_id", dt.Columns [0], "root_Id", false, true, 0, 1, "root_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, true);
			AssertDataColumn ("table#1_bar", dt.Columns [1], "bar", true, false, 0, 1, "bar", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);

			dt = ds.Tables [1];
			AssertDataTable ("dt2", dt, "table", 3, 0);
			AssertDataColumn ("table#2_id", dt.Columns [0], "table_Id", false, true, 0, 1, "table_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, true);
			AssertDataColumn ("table#2_bar", dt.Columns [1], "bar", true, false, 0, 1, "bar", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
			AssertDataColumn ("table#2_refid", dt.Columns [0], "root_Id", true, false, 0, 1, "root_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 2, String.Empty, false, false);

			dt = ds.Tables [2];
			AssertDataTable ("dt3", dt, "foo", 3, 0);
			AssertDataColumn ("table#3_col1", dt.Columns [0], "tableFooChild1", true, false, 0, 1, "tableFooChild1", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("table#3_col2", dt.Columns [0], "tableFooChild2", true, false, 0, 1, "tableFooChild2", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
			AssertDataColumn ("table#3_refid", dt.Columns [0], "table_Id", true, false, 0, 1, "table_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 2, String.Empty, false, false);
*/
		}

		[Test]
		public void TwoElementTable3 ()
		{
			DataSet ds = GetDataSet (xml18, null);
			AssertDataSet ("ds", ds, "set", 2, 1);

			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "table", 1, 0);
			AssertDataColumn ("table_Id", dt.Columns [0], "table_Id", false, true, 0, 1, "table_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, true);

			dt = ds.Tables [1];
			AssertDataTable ("dt", dt, "col", 2, 0);
			AssertDataColumn ("another_col", dt.Columns [0], "another_col", true, false, 0, 1, "another_col", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("table_refId", dt.Columns [1], "table_Id", true, false, 0, 1, "table_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);

			DataRelation dr = ds.Relations [0];
			AssertDataRelation ("rel", dr, "table_col", new string [] {"table_Id"}, new string [] {"table_Id"}, true, true);
			AssertUniqueConstraint ("uniq", dr.ParentKeyConstraint, "Constraint1", true, new string [] {"table_Id"});
			AssertForeignKeyConstraint ("fkey", dr.ChildKeyConstraint, "table_col", AcceptRejectRule.None, Rule.Cascade, Rule.Cascade, new string [] {"table_Id"}, new string [] {"table_Id"});
		}

		[Test]
		public void ConflictColumnTable ()
		{
			DataSet ds = GetDataSet (xml19, null);
			AssertDataSet ("ds", ds, "set", 2, 1);

			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "table", 1, 0);
			AssertDataColumn ("table_Id", dt.Columns [0], "table_Id", false, true, 0, 1, "table_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, true);

			dt = ds.Tables [1];
			AssertDataTable ("dt", dt, "col", 2, 0);
			AssertDataColumn ("table_refId", dt.Columns [1], "table_Id", true, false, 0, 1, "table_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("another_col", dt.Columns [0], "another_col", true, false, 0, 1, "another_col", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);

			DataRelation dr = ds.Relations [0];
			AssertDataRelation ("rel", dr, "table_col", new string [] {"table_Id"}, new string [] {"table_Id"}, true, true);
			AssertUniqueConstraint ("uniq", dr.ParentKeyConstraint, "Constraint1", true, new string [] {"table_Id"});
			AssertForeignKeyConstraint ("fkey", dr.ChildKeyConstraint, "table_col", AcceptRejectRule.None, Rule.Cascade, Rule.Cascade, new string [] {"table_Id"}, new string [] {"table_Id"});
		}

		[Test]
		public void ConflictColumnTableAttribute ()
		{
			// Conflicts between a column and a table, additionally an attribute.
			DataSet ds = GetDataSet (xml20, null);
			AssertDataSet ("ds", ds, "set", 2, 1);

			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "table", 1, 0);
			AssertDataColumn ("table_Id", dt.Columns [0], "table_Id", false, true, 0, 1, "table_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, true);

			dt = ds.Tables [1];
			AssertDataTable ("dt", dt, "col", 3, 0);
			AssertDataColumn ("another_col", dt.Columns [0], "another_col", true, false, 0, 1, "another_col", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("table_refId", dt.Columns [1], "table_Id", true, false, 0, 1, "table_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
			AssertDataColumn ("attr", dt.Columns [2], "attr", true, false, 0, 1, "attr", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 2, String.Empty, false, false);

			DataRelation dr = ds.Relations [0];
			AssertDataRelation ("rel", dr, "table_col", new string [] {"table_Id"}, new string [] {"table_Id"}, true, true);
			AssertUniqueConstraint ("uniq", dr.ParentKeyConstraint, "Constraint1", true, new string [] {"table_Id"});
			AssertForeignKeyConstraint ("fkey", dr.ChildKeyConstraint, "table_col", AcceptRejectRule.None, Rule.Cascade, Rule.Cascade, new string [] {"table_Id"}, new string [] {"table_Id"});
		}

		[Test]
		[ExpectedException (typeof (DataException))]
		public void ConflictAttributeDataTable ()
		{
			// attribute "data" becomes DataTable, and when column "data"
			// appears, it cannot be DataColumn, since the name is 
			// already allocated for DataTable.
			DataSet ds = GetDataSet (xml21, null);
		}
	}
}
