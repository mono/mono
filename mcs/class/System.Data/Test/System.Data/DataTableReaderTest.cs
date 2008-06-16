
// DataTableReaderTest.cs - NUnit Test Cases for testing the DataTableReader
//
// Authors:
//   Sureshkumar T <tsureshkumar@novell.com>
// 
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

#if NET_2_0

using System;
using System.Data;
using System.Data.Common;
using System.Collections;

using NUnit.Framework;

namespace MonoTests.System.Data
{
        [TestFixture]
        public class DataTableReaderTest
        {
                DataTable dt;

                [SetUp]
                public void Setup ()
                {
                        dt = new DataTable ("test");
                        dt.Columns.Add ("id", typeof (int));
                        dt.Columns.Add ("name", typeof (string));
                        dt.PrimaryKey = new DataColumn [] { dt.Columns ["id"] };

                        dt.Rows.Add (new object [] { 1, "mono 1" });
                        dt.Rows.Add (new object [] { 2, "mono 2" });
                        dt.Rows.Add (new object [] { 3, "mono 3" });

                        dt.AcceptChanges ();

                }

                #region Positive Tests
                [Test]
                public void CtorTest ()
                {
                        dt.Rows [1].Delete ();
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                
                                int i = 0;
                                while (reader.Read ())
                                        i++;
                                reader.Close ();

                                Assert.AreEqual (2, i, "no. of rows iterated is wrong");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                [ExpectedException (typeof (InvalidOperationException))]
                public void RowInAccessibleTest ()
                {

                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read ();
                                reader.Read (); // 2nd row
                                dt.Rows [1].Delete ();
                                string value = reader [1].ToString ();
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void IgnoreDeletedRowsDynamicTest ()
                {

                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                dt.Rows [1].Delete ();
                                reader.Read (); // it should be 3rd row
                                string value = reader [0].ToString ();
                                Assert.AreEqual ("3", value, "#1 reader should have moved to 3rd row");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void SeeTheModifiedTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                dt.Rows [1] ["name"] = "mono changed";
                                reader.Read ();
                                string value = reader [1].ToString ();
                                Assert.AreEqual ("mono changed", value, "#2 reader should have moved to 3rd row");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void SchemaTest ()
                {
                        DataTable another = new DataTable ("another");
                        another.Columns.Add ("x", typeof (string));

                        another.Rows.Add (new object [] {"test 1" });
                        another.Rows.Add (new object [] {"test 2" });
                        another.Rows.Add (new object [] {"test 3" });

                        DataTableReader reader = new DataTableReader (new DataTable [] { dt, another });
                        try {
                                DataTable schema = reader.GetSchemaTable ();

                                Assert.AreEqual (dt.Columns.Count, schema.Rows.Count, "#1 should be same");
                                Assert.AreEqual (dt.Columns [1].DataType.ToString (), schema.Rows [1] ["DataType"].ToString (), "#2 data type should match");

                                reader.NextResult (); //schema should change here
                                schema = reader.GetSchemaTable ();

                                Assert.AreEqual (another.Columns.Count, schema.Rows.Count, "#3 should be same");
                                Assert.AreEqual (another.Columns [0].DataType.ToString (), schema.Rows [0] ["DataType"].ToString (), "#4 data type should match");
                        
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void MultipleResultSetsTest ()
                {
                        DataTable dt1 = new DataTable ("test2");
                        dt1.Columns.Add ("x", typeof (string));
                        dt1.Rows.Add (new object [] {"test"} );
                        dt1.Rows.Add (new object [] {"test1"} );
                        dt1.AcceptChanges ();
                        
                        DataTable [] collection = new DataTable [] { dt, dt1 } ; 
                        
                        DataTableReader reader = new DataTableReader (collection);
                        try {
                                int i = 0;
                                do {
                                        while (reader.Read ())
                                                i++;
                                } while (reader.NextResult ());
                                                
                                Assert.AreEqual (5, i, "#1 rows should be of both the tables");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void GetTest ()
                {
                        dt.Columns.Add ("nullint", typeof (int));
                        dt.Rows [0] ["nullint"] = 333;

                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read ();
                        
                                int ordinal = reader.GetOrdinal ("nullint");
                                // Get by name
                                Assert.AreEqual (1, (int) reader ["id"], "#1 should be able to get by name");
                                Assert.AreEqual (333, reader.GetInt32 (ordinal), "#2 should get int32");
                                Assert.AreEqual ("System.Int32", reader.GetDataTypeName (ordinal), "#3 data type should match");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                [ExpectedException (typeof (InvalidOperationException))]
                public void CloseTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                int i = 0;
                                while (reader.Read () && i < 1)
                                        i++;
                                reader.Close ();
                                reader.Read ();
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void GetOrdinalTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                Assert.AreEqual (1, reader.GetOrdinal ("name"), "#1 get ordinal should work even" +
                                                 " without calling Read");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }
                #endregion // Positive Tests

                
                #region Negative Tests
                [Test]
                public void NoRowsTest ()
                {
                        dt.Rows.Clear ();
                        dt.AcceptChanges ();
                        
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                        
                                Assert.IsFalse (reader.Read (), "#1 there are no rows");
                                Assert.IsFalse (reader.NextResult (), "#2 there are no further resultsets");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }
                
                [Test]
                [ExpectedException (typeof (ArgumentException))]
                public void NoTablesTest ()
                {
                        DataTableReader reader = new DataTableReader (new DataTable [] {});
                        try {
                                reader.Read ();
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

		[Test]
                [ExpectedException (typeof (InvalidOperationException))]
		public void ReadAfterClosedTest ()
		{
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read ();
                                reader.Close ();
                                reader.Read ();
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
		}	

		[Test]
                [ExpectedException (typeof (InvalidOperationException))]
		public void AccessAfterClosedTest ()
		{
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read ();
                                reader.Close ();
                                int i = (int) reader [0];
                                i++; // to supress warning
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
		}

                [Test]
                [ExpectedException (typeof (InvalidOperationException))]
		public void AccessBeforeReadTest ()
		{
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                int i = (int) reader [0];
                                i++; // to supress warning
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
		}

                [Test]
                [ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InvalidIndexTest ()
		{
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read ();
                                int i = (int) reader [90]; // kidding, ;-)
                                i++; // to supress warning
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
		}

                [Test]
		public void DontSeeTheEarlierRowsTest ()
		{
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row

                                // insert a row at position 0
                                DataRow r = dt.NewRow ();
                                r [0] = 0;
                                r [1] = "adhi bagavan";
                                dt.Rows.InsertAt (r, 0);
                        
                                Assert.AreEqual (2, (int) reader.GetInt32 (0), "#1 should not alter the position");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
		}

                [Test]
                public void AddBeforePointTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row
                                DataRow r = dt.NewRow ();
                                r [0] = 0;
                                r [1] = "adhi bagavan";
                                dt.Rows.InsertAt (r, 0);
                                dt.Rows.Add (new object [] { 4, "mono 4"}); // should not affect the counter
                                Assert.AreEqual (2, (int) reader [0], "#1 should not affect the current position");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void AddAtPointTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row
                                DataRow r = dt.NewRow ();
                                r [0] = 0;
                                r [1] = "same point";
                                dt.Rows.InsertAt (r, 1);
                                dt.Rows.Add (new object [] { 4, "mono 4"}); // should not affect the counter
                                Assert.AreEqual (2, (int) reader [0], "#1 should not affect the current position");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void DeletePreviousAndAcceptChangesTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row
                                dt.Rows [0].Delete ();
                                dt.AcceptChanges ();
                                Assert.AreEqual (2, (int) reader [0], "#1 should not affect the current position");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }

                }

                [Test]
                public void DeleteCurrentAndAcceptChangesTest2 ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row
                                dt.Rows [1].Delete (); // delete row, where reader points to
                                dt.AcceptChanges (); // accept the action
                                Assert.AreEqual (1, (int) reader [0], "#1 should point to the first row");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                [ExpectedException (typeof (InvalidOperationException))]
                public void DeleteFirstCurrentAndAcceptChangesTest ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                dt.Rows [0].Delete (); // delete row, where reader points to
                                dt.AcceptChanges (); // accept the action
                                Assert.AreEqual (2, (int) reader [0], "#1 should point to the first row");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void DeleteLastAndAcceptChangesTest2 ()
                {
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read (); // second row
                                reader.Read (); // third row
                                dt.Rows [2].Delete (); // delete row, where reader points to
                                dt.AcceptChanges (); // accept the action
                                Assert.AreEqual (2, (int) reader [0], "#1 should point to the first row");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }

                [Test]
                public void ClearTest ()
                {
                        DataTableReader reader = null;
                        try {
                                reader = new DataTableReader (dt);
                                reader.Read (); // first row
                                reader.Read (); // second row
                                dt.Clear ();
                                try {
                                        int i = (int) reader [0];
                                        i++; // supress warning
                                        Assert.Fail("#1 should have thrown RowNotInTableException");
                                } catch (RowNotInTableException) {}

                                // clear and add test
                                reader.Close ();
                                reader = new DataTableReader (dt);
                                reader.Read (); // first row
                                reader.Read (); // second row
                                dt.Clear ();
                                dt.Rows.Add (new object [] {8, "mono 8"});
                                dt.AcceptChanges ();
                                bool success = reader.Read ();
                                Assert.IsFalse (success, "#2 is always invalid");

                                // clear when reader is not read yet
                                reader.Close ();
                                reader = new DataTableReader (dt);
                                dt.Clear ();
                                dt.Rows.Add (new object [] {8, "mono 8"});
                                dt.AcceptChanges ();
                                success = reader.Read ();
                                Assert.IsTrue (success, "#3 should add");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                        
                }

                [Test]
                public void MultipleDeleteTest ()
                {
                        dt.Rows.Add (new object [] {4, "mono 4"});
                        dt.Rows.Add (new object [] {5, "mono 5"});
                        dt.Rows.Add (new object [] {6, "mono 6"});
                        dt.Rows.Add (new object [] {7, "mono 7"});
                        dt.Rows.Add (new object [] {8, "mono 8"});
                        dt.AcceptChanges ();
                        
                        DataTableReader reader = new DataTableReader (dt);
                        try {
                                reader.Read (); // first row
                                reader.Read ();
                                reader.Read ();
                                reader.Read ();
                                reader.Read ();

                                dt.Rows [3].Delete ();
                                dt.Rows [1].Delete ();
                                dt.Rows [2].Delete ();
                                dt.Rows [0].Delete ();
                                dt.Rows [6].Delete ();
                                dt.AcceptChanges ();

                                Assert.AreEqual (5, (int) reader [0], "#1 should keep pointing to 5");
                        } finally {
                                if (reader != null && !reader.IsClosed)
                                        reader.Close ();
                        }
                }
                #endregion // Negative Tests
		
		[Test]
		public void TestSchemaTable()
		{
			DataSet ds = new DataSet();
			DataTable testTable = new DataTable ("TestTable1");
			DataTable testTable1 = new DataTable ();
			
			testTable.Namespace = "TableNamespace";
			
			testTable1.Columns.Add ("col1", typeof(int));
			testTable1.Columns.Add ("col2", typeof(int));
			ds.Tables.Add (testTable);
			ds.Tables.Add (testTable1);
			
			//create a col for standard datatype
			
			testTable.Columns.Add ("col_string");
			testTable.Columns.Add ("col_string_fixed");
			testTable.Columns ["col_string_fixed"].MaxLength = 10;
			testTable.Columns.Add ("col_int", typeof(int));
			testTable.Columns.Add ("col_decimal", typeof(decimal));
			testTable.Columns.Add ("col_datetime", typeof(DateTime));
			testTable.Columns.Add ("col_float", typeof (float));
			
			// Check for col constraints/properties
			testTable.Columns.Add ("col_readonly").ReadOnly = true;
			
			testTable.Columns.Add ("col_autoincrement", typeof(Int64)).AutoIncrement = true;
			testTable.Columns ["col_autoincrement"].AutoIncrementStep = 5;
			testTable.Columns ["col_autoincrement"].AutoIncrementSeed = 10;
			
			testTable.Columns.Add ("col_pk");
			testTable.PrimaryKey = new DataColumn[] {testTable.Columns ["col_pk"]};
			
			testTable.Columns.Add ("col_unique");
			testTable.Columns ["col_unique"].Unique = true;
			
			testTable.Columns.Add ("col_defaultvalue");
			testTable.Columns ["col_defaultvalue"].DefaultValue = "DefaultValue";
			
			testTable.Columns.Add ("col_expression_local", typeof(int));
			testTable.Columns ["col_expression_local"].Expression = "col_int*5";
			
			ds.Relations.Add ("rel", new DataColumn[] {testTable1.Columns ["col1"]}, 
					new DataColumn[] {testTable.Columns ["col_int"]}, false);
			testTable.Columns.Add ("col_expression_ext");
			testTable.Columns ["col_expression_ext"].Expression = "parent.col2";
			
			testTable.Columns.Add ("col_namespace");
			testTable.Columns ["col_namespace"].Namespace = "ColumnNamespace";
			
			testTable.Columns.Add ("col_mapping");
			testTable.Columns ["col_mapping"].ColumnMapping = MappingType.Attribute;
			
			DataTable schemaTable = testTable.CreateDataReader ().GetSchemaTable ();
			
			Assert.AreEqual (25, schemaTable.Columns.Count, "#1");
			Assert.AreEqual (testTable.Columns.Count, schemaTable.Rows.Count, "#2");
			
			//True for all rows
			for (int i = 0; i < schemaTable.Rows.Count; ++i) {
				Assert.AreEqual (testTable.TableName, schemaTable.Rows [i]["BaseTableName"], i+"_#3");
				Assert.AreEqual (ds.DataSetName, schemaTable.Rows [i]["BaseCatalogName"], i+"_#4");
				Assert.AreEqual (DBNull.Value, schemaTable.Rows [i]["BaseSchemaName"], i+"_#5");
				Assert.AreEqual (schemaTable.Rows [i]["BaseColumnName"], schemaTable.Rows [i]["ColumnName"], i+"_#6");
				Assert.IsFalse ((bool)schemaTable.Rows [i]["IsRowVersion"], i+"_#7");
			}
			
			Assert.AreEqual ("col_string", schemaTable.Rows [0]["ColumnName"], "#8");
			Assert.AreEqual (typeof(string), schemaTable.Rows [0]["DataType"], "#9");
			Assert.AreEqual (-1, schemaTable.Rows [0]["ColumnSize"], "#10");
			Assert.AreEqual (0, schemaTable.Rows [0]["ColumnOrdinal"], "#11");
			// ms.net contradicts documented behavior
			Assert.IsFalse ((bool)schemaTable.Rows [0]["IsLong"], "#12");
			
			Assert.AreEqual ("col_string_fixed", schemaTable.Rows [1]["ColumnName"], "#13");
			Assert.AreEqual (typeof(string), schemaTable.Rows [1]["DataType"], "#14");
			Assert.AreEqual (10, schemaTable.Rows [1]["ColumnSize"], "#15");
			Assert.AreEqual (1, schemaTable.Rows [1]["ColumnOrdinal"], "#16");
			Assert.IsFalse ((bool)schemaTable.Rows [1]["IsLong"], "#17");
			
			Assert.AreEqual ("col_int", schemaTable.Rows [2]["ColumnName"], "#18");
			Assert.AreEqual (typeof(int), schemaTable.Rows [2]["DataType"], "#19");
			Assert.AreEqual (DBNull.Value, schemaTable.Rows [2]["NumericPrecision"], "#20");
			Assert.AreEqual (DBNull.Value, schemaTable.Rows [2]["NumericScale"], "#21");
			Assert.AreEqual (-1, schemaTable.Rows [2]["ColumnSize"], "#22");
			Assert.AreEqual (2, schemaTable.Rows [2]["ColumnOrdinal"], "#23");
			
			Assert.AreEqual ("col_decimal", schemaTable.Rows [3]["ColumnName"], "#24");
			Assert.AreEqual (typeof(decimal), schemaTable.Rows [3]["DataType"], "#25");
			// When are the Precision and Scale Values set ? 
			Assert.AreEqual (DBNull.Value, schemaTable.Rows [3]["NumericPrecision"], "#26");
			Assert.AreEqual (DBNull.Value, schemaTable.Rows [3]["NumericScale"], "#27");
			Assert.AreEqual (-1, schemaTable.Rows [3]["ColumnSize"], "#28");
			Assert.AreEqual (3, schemaTable.Rows [3]["ColumnOrdinal"], "#29");
			
			Assert.AreEqual ("col_datetime", schemaTable.Rows [4]["ColumnName"], "#30");
			Assert.AreEqual (typeof(DateTime), schemaTable.Rows [4]["DataType"], "#31");
			Assert.AreEqual (4, schemaTable.Rows [4]["ColumnOrdinal"], "#32");
			
			Assert.AreEqual ("col_float", schemaTable.Rows [5]["ColumnName"], "#33");
			Assert.AreEqual (typeof(float), schemaTable.Rows [5]["DataType"], "#34");
			Assert.AreEqual (5, schemaTable.Rows [5]["ColumnOrdinal"], "#35");
			Assert.AreEqual (DBNull.Value, schemaTable.Rows [5]["NumericPrecision"], "#36");
			Assert.AreEqual (DBNull.Value, schemaTable.Rows [5]["NumericScale"], "#37");
			Assert.AreEqual (-1, schemaTable.Rows [5]["ColumnSize"], "#38");
			
			Assert.AreEqual ("col_readonly", schemaTable.Rows [6]["ColumnName"], "#39");
			Assert.IsTrue ((bool)schemaTable.Rows [6]["IsReadOnly"], "#40");
			
			Assert.AreEqual ("col_autoincrement", schemaTable.Rows [7]["ColumnName"], "#9");
			Assert.IsTrue ((bool)schemaTable.Rows [7]["IsAutoIncrement"], "#41");
			Assert.AreEqual (10, schemaTable.Rows [7]["AutoIncrementSeed"], "#42");
			Assert.AreEqual (5, schemaTable.Rows [7]["AutoIncrementStep"], "#43");
			Assert.IsFalse ((bool)schemaTable.Rows [7]["IsReadOnly"], "#44");
			
			Assert.AreEqual ("col_pk", schemaTable.Rows [8]["ColumnName"], "#45");
			Assert.IsTrue ((bool)schemaTable.Rows [8]["IsKey"], "#46");
			Assert.IsTrue ((bool)schemaTable.Rows [8]["IsUnique"], "#47");
			
			Assert.AreEqual ("col_unique", schemaTable.Rows [9]["ColumnName"], "#48");
			Assert.IsTrue ((bool)schemaTable.Rows [9]["IsUnique"], "#49");
			
			Assert.AreEqual ("col_defaultvalue", schemaTable.Rows [10]["ColumnName"], "#50");
			Assert.AreEqual ("DefaultValue", schemaTable.Rows [10]["DefaultValue"], "#51");
			
			Assert.AreEqual ("col_expression_local", schemaTable.Rows [11]["ColumnName"], "#52");
			Assert.AreEqual ("col_int*5", schemaTable.Rows [11]["Expression"], "#53");
			Assert.IsTrue ((bool)schemaTable.Rows [11]["IsReadOnly"], "#54");
			
			// if expression depends on a external col, then set Expression as null..
			Assert.AreEqual ("col_expression_ext", schemaTable.Rows [12]["ColumnName"], "#55");
			Assert.AreEqual (DBNull.Value, schemaTable.Rows [12]["Expression"], "#56");
			Assert.IsTrue ((bool)schemaTable.Rows [12]["IsReadOnly"], "#57");
			
			Assert.AreEqual ("col_namespace", schemaTable.Rows [13]["ColumnName"], "#58");
			Assert.AreEqual ("TableNamespace", schemaTable.Rows [13]["BaseTableNamespace"], "#59");
			Assert.AreEqual ("TableNamespace", schemaTable.Rows [12]["BaseColumnNamespace"], "#60");
			Assert.AreEqual ("ColumnNamespace", schemaTable.Rows [13]["BaseColumnNamespace"], "#61");
			
			Assert.AreEqual ("col_mapping", schemaTable.Rows [14]["ColumnName"], "#62");
			Assert.AreEqual (MappingType.Element, (MappingType)schemaTable.Rows [13]["ColumnMapping"], "#63");
			Assert.AreEqual (MappingType.Attribute, (MappingType)schemaTable.Rows [14]["ColumnMapping"], "#64");
		}

		[Test]
		public void TestExceptionIfSchemaChanges ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1");
			DataTableReader rdr = table.CreateDataReader ();
			Assert.AreEqual (1, rdr.GetSchemaTable().Rows.Count, "#1");

			table.Columns [0].ColumnName = "newcol1";
			try {
				rdr.GetSchemaTable ();
				Assert.Fail ("#0");
			} catch (InvalidOperationException e) {
				// Never premise English.
				//Assert.AreEqual ("Schema of current DataTable '" + table.TableName + 
				//		"' in DataTableReader has changed, DataTableReader is invalid.", e.Message, "#1");
			}
			
			rdr = table.CreateDataReader ();
			rdr.GetSchemaTable (); //no exception
			table.Columns.Add ("col2");
			try {
				rdr.GetSchemaTable ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException e) {
				// Never premise English.
				//Assert.AreEqual ("Schema of current DataTable '" + table.TableName + 
				//		"' in DataTableReader has changed, DataTableReader is invalid.", e.Message, "#1");
			}
		}
	
		[Test]
		public void EnumeratorTest ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof(int));
			table.Rows.Add (new object[] {0});
			table.Rows.Add (new object[] {1});
			
			DataTableReader rdr = table.CreateDataReader ();
			IEnumerator enmr = rdr.GetEnumerator ();
			
			table.Rows.Add (new object[] {2});
			table.Rows.RemoveAt (0);
						
			//Test if the Enumerator is stable
			int i = 1;
			while (enmr.MoveNext ()) {
				DbDataRecord rec = (DbDataRecord)enmr.Current;
				Assert.AreEqual (i, rec.GetInt32 (0), "#2_" + i);
				i++;
			}
		}
		
		[Test]
		public void GetCharsTest()
		{
			dt.Columns.Add ("col2", typeof (char[]));
			
			dt.Rows.Clear ();
			dt.Rows.Add (new object[] {1, "string", "string".ToCharArray()}); 
			dt.Rows.Add (new object[] {2, "string1", null}); 
			DataTableReader rdr = dt.CreateDataReader ();
			
			rdr.Read ();
			
			try {
				rdr.GetChars (1, 0, null, 0, 10);
				Assert.Fail ("#1");
			} catch (InvalidCastException e) {
				// Never premise English.
				//Assert.AreEqual ("Unable to cast object of type 'System.String'" +
				//	" to type 'System.Char[]'.", e.Message, "#1");
			}
			char[] char_arr = null;
			long len = 0;
			
			len =  rdr.GetChars (2, 0, null, 0, 0);
			Assert.AreEqual(6, len, "#2");
			
			char_arr = new char [len];
			len = rdr.GetChars (2, 0, char_arr, 0, 0);
			Assert.AreEqual(0, len, "#3");
			
			len = rdr.GetChars (2, 0, null, 0, 0);
			char_arr = new char [len+2];
			len = rdr.GetChars (2, 0, char_arr, 2, 100);
			Assert.AreEqual (6, len, "#4");
			char[] val = (char[])rdr.GetValue (2);
			for (int i = 0; i < len; ++i)
				Assert.AreEqual (val[i], char_arr[i+2], "#5_"+i);
		}
		
		[Test]
		public void GetProviderSpecificTests()
		{
			DataTableReader rdr = dt.CreateDataReader ();
			while (rdr.Read ()) {
				object[] values = new object [rdr.FieldCount];
				object[] pvalues = new object [rdr.FieldCount];
				rdr.GetValues (values);
				rdr.GetProviderSpecificValues (pvalues);
				
				for (int i = 0; i < rdr.FieldCount; ++i) {
					Assert.AreEqual(values [i], pvalues [i], "#1");
					Assert.AreEqual(rdr.GetValue (i), rdr.GetProviderSpecificValue (i), "#2");
					Assert.AreEqual(rdr.GetFieldType (i), rdr.GetProviderSpecificFieldType (i), "#3");
				}
			}
		}
		
		[Test]
		public void GetNameTest()
		{
			DataTableReader rdr = dt.CreateDataReader();
			for (int i = 0; i < dt.Columns.Count; ++i)
				Assert.AreEqual(dt.Columns[i].ColumnName, rdr.GetName(i), "#1_" + i);
		}
        }
}

#endif // NET_2_0
