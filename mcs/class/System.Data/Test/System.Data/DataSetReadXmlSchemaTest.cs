//
// DataSetReadXmlSchemaTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

using System;
using System.IO;
using System.Data;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataSetReadXmlSchemaTest : DataSetAssertion
	{
		private DataSet CreateTestSet ()
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
			ds.Relations.Add ("Rel1", ds.Tables [0].Columns [2], ds.Tables [1].Columns [0]);
			return ds;
		}

		[Test]
		public void ReadTest1 ()
		{
			DataSet ds = CreateTestSet ();

			StringWriter sw = new StringWriter ();
			ds.WriteXmlSchema (sw);

			string schema = sw.ToString ();

			ds = new DataSet ();
			ds.ReadXmlSchema (new XmlTextReader (schema, XmlNodeType.Document, null));

			AssertDataSet ("dataset", ds, "NewDataSet", 2);
			AssertDataTable ("tbl1", ds.Tables [0], "Table1", 3, 0);
			AssertDataTable ("tbl2", ds.Tables [1], "Table2", 3, 0);

			DataRelation rel = ds.Relations [0];
			AssertDataRelation ("rel", rel, "Rel1",
				new string [] {"Column1_3"},
				new string [] {"Column2_1"}, true, true);
			AssertUniqueConstraint ("uc", rel.ParentKeyConstraint, 
				"Constraint1", false, new string [] {"Column1_3"});
			AssertForeignKeyConstraint ("fk", rel.ChildKeyConstraint, "Rel1",
				AcceptRejectRule.None, Rule.Cascade, Rule.Cascade,
				new string [] {"Column2_1"}, 
				new string [] {"Column1_3"});
		}
	}
}
