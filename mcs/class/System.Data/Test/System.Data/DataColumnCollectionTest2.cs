// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
//
// Copyright (c) 2004 Mainsoft Co.
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
using System.ComponentModel;
using System.Data;
using MonoTests.System.Data.Utils;
using System.Collections;

namespace MonoTests.System.Data
{
	[TestFixture] public class DataColumnCollectionTest2
	{
		private int counter = 0;

		[Test] public void Add()
		{
			DataColumn dc = null;
			DataTable dt = new DataTable();

			//----------------------------- check default --------------------
			dc = dt.Columns.Add();
			// Add column 1
			Assert.AreEqual("Column1", dc.ColumnName, "DCC1");

			// Add column 2
			dc = dt.Columns.Add();
			Assert.AreEqual("Column2", dc.ColumnName, "DCC2");

			dc = dt.Columns.Add();
			// Add column 3
			Assert.AreEqual("Column3", dc.ColumnName, "DCC3");

			dc = dt.Columns.Add();
			// Add column 4
			Assert.AreEqual("Column4", dc.ColumnName, "DCC4");

			dc = dt.Columns.Add();
			// Add column 5
			Assert.AreEqual("Column5", dc.ColumnName, "DCC5");
			Assert.AreEqual(5, dt.Columns.Count, "DCC6");

			//----------------------------- check Add/Remove from begining --------------------
			dt = initTable();

			dt.Columns.Remove(dt.Columns[0]);
			dt.Columns.Remove(dt.Columns[0]);
			dt.Columns.Remove(dt.Columns[0]);

			// check column 4 - remove - from begining
			Assert.AreEqual("Column4", dt.Columns[0].ColumnName, "DCC7");

			// check column 5 - remove - from begining
			Assert.AreEqual("Column5", dt.Columns[1].ColumnName , "DCC8");
			Assert.AreEqual(2, dt.Columns.Count, "DCC9");

			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();

			// check column 0 - Add new  - from begining
			Assert.AreEqual("Column4", dt.Columns[0].ColumnName , "DCC10");

			// check column 1 - Add new - from begining
			Assert.AreEqual("Column5", dt.Columns[1].ColumnName , "DCC11");

			// check column 2 - Add new - from begining
			Assert.AreEqual("Column6", dt.Columns[2].ColumnName , "DCC12");

			// check column 3 - Add new - from begining
			Assert.AreEqual("Column7", dt.Columns[3].ColumnName , "DCC13");

			// check column 4 - Add new - from begining
			Assert.AreEqual("Column8", dt.Columns[4].ColumnName , "DCC14");

			// check column 5 - Add new - from begining
			Assert.AreEqual("Column9", dt.Columns[5].ColumnName , "DCC15");

			//----------------------------- check Add/Remove from middle --------------------

			dt = initTable();

			dt.Columns.Remove(dt.Columns[2]);
			dt.Columns.Remove(dt.Columns[2]);
			dt.Columns.Remove(dt.Columns[2]);

			// check column 0 - remove - from Middle
			Assert.AreEqual("Column1", dt.Columns[0].ColumnName, "DCC16");

			// check column 1 - remove - from Middle
			Assert.AreEqual("Column2", dt.Columns[1].ColumnName , "DCC17");

			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();

			// check column 0 - Add new  - from Middle
			Assert.AreEqual("Column1", dt.Columns[0].ColumnName , "DCC18");

			// check column 1 - Add new - from Middle
			Assert.AreEqual("Column2", dt.Columns[1].ColumnName , "DCC19");

			// check column 2 - Add new - from Middle
			Assert.AreEqual("Column3", dt.Columns[2].ColumnName , "DCC20");

			// check column 3 - Add new - from Middle
			Assert.AreEqual("Column4", dt.Columns[3].ColumnName , "DCC21");

			// check column 4 - Add new - from Middle
			Assert.AreEqual("Column5", dt.Columns[4].ColumnName , "DCC22");

			// check column 5 - Add new - from Middle
			Assert.AreEqual("Column6", dt.Columns[5].ColumnName , "DCC23");

			//----------------------------- check Add/Remove from end --------------------

			dt = initTable();

			dt.Columns.Remove(dt.Columns[4]);
			dt.Columns.Remove(dt.Columns[3]);
			dt.Columns.Remove(dt.Columns[2]);

			// check column 0 - remove - from end
			Assert.AreEqual("Column1", dt.Columns[0].ColumnName, "DCC24");

			// check column 1 - remove - from end
			Assert.AreEqual("Column2", dt.Columns[1].ColumnName , "DCC25");

			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();

			// check column 0 - Add new  - from end
			Assert.AreEqual("Column1", dt.Columns[0].ColumnName , "DCC26");

			// check column 1 - Add new - from end
			Assert.AreEqual("Column2", dt.Columns[1].ColumnName , "DCC27");

			// check column 2 - Add new - from end
			Assert.AreEqual("Column3", dt.Columns[2].ColumnName , "DCC28");

			// check column 3 - Add new - from end
			Assert.AreEqual("Column4", dt.Columns[3].ColumnName , "DCC29");

			// check column 4 - Add new - from end
			Assert.AreEqual("Column5", dt.Columns[4].ColumnName , "DCC30");

			// check column 5 - Add new - from end
			Assert.AreEqual("Column6", dt.Columns[5].ColumnName , "DCC31");
		}

		private DataTable initTable()
		{
			DataTable dt = new DataTable();
			for (int i=0; i<5; i++)
			{
				dt.Columns.Add();
			}
			return dt;
	   }

		[Test] public void TestAdd_ByTableName()
		{
			//this test is from boris

			DataSet ds = new DataSet();
			DataTable dt = new DataTable();
			ds.Tables.Add(dt);

			// add one column
			dt.Columns.Add("id1",typeof(int));

			// DataColumnCollection add
			Assert.AreEqual(1, dt.Columns.Count , "DCC32");

			// add row
			DataRow dr = dt.NewRow();
			dt.Rows.Add(dr);

			// remove column
			dt.Columns.Remove("id1");

			// DataColumnCollection remove
			Assert.AreEqual(0, dt.Columns.Count , "DCC33");

			//row is still there

			// now add column
			dt.Columns.Add("id2",typeof(int));

			// DataColumnCollection add again
			Assert.AreEqual(1, dt.Columns.Count , "DCC34");
		}

		[Test] public void TestCanRemove_ByDataColumn()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			DataColumn dummyCol = new DataColumn();
			Assert.AreEqual(false, dt.Columns.CanRemove(null), "DCC35"); //Cannot remove null column
			Assert.AreEqual(false, dt.Columns.CanRemove(dummyCol), "DCC36"); //Don't belong to this table
			Assert.AreEqual(false, dt.Columns.CanRemove(dt.Columns[0]), "DCC37"); //It belongs to unique constraint
			Assert.AreEqual(true, dt.Columns.CanRemove(dt.Columns[1]), "DCC38");
		}
		[Test] public void TestCanRemove_ForigenConstraint()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();

			Assert.AreEqual(false, ds.Tables["child"].Columns.CanRemove(ds.Tables["child"].Columns["parentId"]), "DCC39");//Forigen
			Assert.AreEqual(false, ds.Tables["parent"].Columns.CanRemove(ds.Tables["child"].Columns["parentId"]), "DCC40");//Parent
		}
		[Test] public void TestCanRemove_ParentRelations()
		{
			DataSet ds = new DataSet();

			ds.Tables.Add("table1");
			ds.Tables.Add("table2");
			ds.Tables["table1"].Columns.Add("col1");
			ds.Tables["table2"].Columns.Add("col1");

			ds.Tables[1].ParentRelations.Add("name1",ds.Tables[0].Columns["col1"],ds.Tables[1].Columns["col1"],false);

			Assert.AreEqual(false, ds.Tables[1].Columns.CanRemove(ds.Tables[1].Columns["col1"]), "DCC41"); //Part of a parent
			Assert.AreEqual(false, ds.Tables[0].Columns.CanRemove(ds.Tables[0].Columns["col1"]), "DCC42"); //Part of a child
		}

		[Test] public void TestCanRemove_Expression()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("col1",typeof(string));
			dt.Columns.Add("col2",typeof(string),"sum(col1)");

			Assert.AreEqual(false, dt.Columns.CanRemove(dt.Columns["col1"]), "DCC43"); //Col1 is a part of expression
		}

		[Test] public void TestAdd_CollectionChanged()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			dt.Columns.CollectionChanged+=new CollectionChangeEventHandler(Columns_CollectionChanged);
			counter = 0;
			DataColumn c = dt.Columns.Add("tempCol");

			Assert.AreEqual(1, counter, "DCC44.1");
			Assert.AreEqual (c, change_element, "DCC44.2");
		}

		[Test] public void TestRemove_CollectionChanged()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			dt.Columns.CollectionChanged+=new CollectionChangeEventHandler(Columns_CollectionChanged);
			DataColumn c = dt.Columns.Add("tempCol");
			counter = 0;
			dt.Columns.Remove("tempCol");

			Assert.AreEqual (1, counter, "DCC44.3");
			Assert.AreEqual (c, change_element, "DCC44.4");
		}

		[Test] public void TestSetName_CollectionChanged()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			dt.Columns.CollectionChanged+=new CollectionChangeEventHandler(Columns_CollectionChanged);
			dt.Columns.Add("tempCol");
			counter = 0;
			dt.Columns[0].ColumnName = "tempCol2";

			Assert.AreEqual(0, counter, "DCC44.5");
		}

		object change_element;
		private void Columns_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			counter++;
			change_element = e.Element;
		}

		[Test] public void TestContains_ByColumnName()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			Assert.AreEqual(true, dt.Columns.Contains("ParentId"), "DCC45");
			Assert.AreEqual(true, dt.Columns.Contains("String1"), "DCC46");
			Assert.AreEqual(true, dt.Columns.Contains("ParentBool"), "DCC47");

			Assert.AreEqual(false, dt.Columns.Contains("ParentId1"), "DCC48");
			dt.Columns.Remove("ParentId");
			Assert.AreEqual(false, dt.Columns.Contains("ParentId"), "DCC49");

			dt.Columns["String1"].ColumnName = "Temp1";

			Assert.AreEqual(false, dt.Columns.Contains("String1"), "DCC50");
			Assert.AreEqual(true, dt.Columns.Contains("Temp1"), "DCC51");
		}
		public void NotReadyTestContains_S2() // FIXME: fails in MS
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			Assert.AreEqual(false, dt.Columns.Contains(null), "DCC52");
		}


		[Test] public void Count()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			Assert.AreEqual(6, dt.Columns.Count, "DCC55");

			dt.Columns.Add("temp1");
			Assert.AreEqual(7, dt.Columns.Count, "DCC56");

			dt.Columns.Remove("temp1");
			Assert.AreEqual(6, dt.Columns.Count, "DCC57");

			dt.Columns.Remove("ParentId");
			Assert.AreEqual(5, dt.Columns.Count, "DCC58");
		}

		[Test] public void TestIndexOf_ByDataColumn()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			for (int i=0;i<dt.Columns.Count;i++)
			{
				Assert.AreEqual(i, dt.Columns.IndexOf(dt.Columns[i]), "DCC59");
			}

			DataColumn col = new DataColumn();

			Assert.AreEqual(-1, dt.Columns.IndexOf(col), "DCC60");

			Assert.AreEqual(-1, dt.Columns.IndexOf((DataColumn)null), "DCC61");
		}

		[Test]
		public void TestIndexOf_ByColumnName()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			for (int i=0;i<dt.Columns.Count;i++)
			{
				Assert.AreEqual(i, dt.Columns.IndexOf(dt.Columns[i].ColumnName), "DCC62");
			}

			DataColumn col = new DataColumn();

			Assert.AreEqual(-1, dt.Columns.IndexOf("temp1"), "DCC63");

			Assert.AreEqual(-1, dt.Columns.IndexOf((string)null), "DCC64");
		}

		[Test] public void TestRemove_ByDataColumn()
		{
			//prepare a DataSet with DataTable to be checked
			DataTable dtSource = new DataTable();
			dtSource.Columns.Add("Col_0", typeof(int));
			dtSource.Columns.Add("Col_1", typeof(int));
			dtSource.Columns.Add("Col_2", typeof(int));
			dtSource.Rows.Add(new object[] {0,1,2});

			DataTable dt = null;

			//------Check Remove first column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[0]);
			// Remove first column - check column count
			Assert.AreEqual(2, dt.Columns.Count , "DCC65");

			// Remove first column - check column removed
			Assert.AreEqual(false, dt.Columns.Contains("Col_0"), "DCC66");

			// Remove first column - check column 0 data
			Assert.AreEqual(1, dt.Rows[0][0], "DCC67");

			// Remove first column - check column 1 data
			Assert.AreEqual(2, dt.Rows[0][1], "DCC68");

			//------Check Remove middle column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[1]);
			// Remove middle column - check column count
			Assert.AreEqual(2, dt.Columns.Count , "DCC69");

			// Remove middle column - check column removed
			Assert.AreEqual(false, dt.Columns.Contains("Col_1"), "DCC70");

			// Remove middle column - check column 0 data
			Assert.AreEqual(0, dt.Rows[0][0], "DCC71");

			// Remove middle column - check column 1 data
			Assert.AreEqual(2, dt.Rows[0][1], "DCC72");

			//------Check Remove last column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[2]);
			// Remove last column - check column count
			Assert.AreEqual(2, dt.Columns.Count , "DCC73");

			// Remove last column - check column removed
			Assert.AreEqual(false, dt.Columns.Contains("Col_2"), "DCC74");

			// Remove last column - check column 0 data
			Assert.AreEqual(0, dt.Rows[0][0], "DCC75");

			// Remove last column - check column 1 data
			Assert.AreEqual(1, dt.Rows[0][1], "DCC76");

			//------Check Remove column exception---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);
			// Check Remove column exception - Column name not exists
			try {
				DataColumn dc = new DataColumn();
				dt.Columns.Remove(dc);
				Assert.Fail("DCC77: Remove failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DCC78: Remove. Wrong exception type. Got:" + exc);
			}
		}

		[Test] 
		public void Add_DataColumn1()
		{
			DataTable dt = new DataTable();
			DataColumn col = new DataColumn("col1",Type.GetType("System.String"));
			dt.Columns.Add(col);
			Assert.AreEqual(1,dt.Columns.Count,"dccadc1#1");
			Assert.AreEqual("col1",dt.Columns[0].ColumnName,"dccadc1#2");
			Assert.AreEqual("System.String",dt.Columns[0].DataType.ToString(),"dccadc1#3");			
		}

		[Test] 
		public void Add_DataColumn2()
		{
			DataTable dt = new DataTable();
			DataColumn col = new DataColumn("col1",Type.GetType("System.String"));
			dt.Columns.Add(col);
			try
			{
				dt.Columns.Add(col); 
				Assert.Fail("dccadc2#1: Add failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccadc2#2: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test] 
		public void Add_DataColumn3()
		{
			DataTable dt = new DataTable();
			DataColumn col = new DataColumn("col1",Type.GetType("System.String"));
			dt.Columns.Add(col);
			try
			{
				DataColumn col1 = new DataColumn("col1",Type.GetType("System.String"));
				dt.Columns.Add(col1);
				Assert.Fail("dccadc3#1: Add failed to throw DuplicateNameExcpeion");
			}
			catch (DuplicateNameException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccadc3#2: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void Add_String1()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("col1");
			Assert.AreEqual(1,dt.Columns.Count,"dccas1#1");
			Assert.AreEqual("col1",dt.Columns[0].ColumnName,"dccas1#2");

		}

		[Test]
		public void Add_String2()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("col1");
			try
			{
				dt.Columns.Add("col1");
				Assert.Fail("dccas2#1: Add failed to throw DuplicateNameExcpeion");
			}
			catch (DuplicateNameException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccas2#2: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void AddRange_DataColumn1()
		{
			DataTable dt = new DataTable();
			dt.Columns.AddRange(GetDataColumArray());
			Assert.AreEqual(2,dt.Columns.Count,"dccardc1#1");
			Assert.AreEqual("col1",dt.Columns[0].ColumnName,"dccardc1#2");
			Assert.AreEqual("col2",dt.Columns[1].ColumnName,"dccardc1#3");
			Assert.AreEqual(typeof(int),dt.Columns[0].DataType,"dccardc1#4");
			Assert.AreEqual(typeof(string),dt.Columns[1].DataType,"dccardc1#5");			
		}

		[Test]
		public void AddRange_DataColumn2()
		{
			DataTable dt = new DataTable();
			try
			{
				dt.Columns.AddRange(GetBadDataColumArray());
				Assert.Fail("dccardc2#1: AddRange failed to throw DuplicateNameExcpeion");
			}
			catch (DuplicateNameException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccardc2#2: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void DataColumnCollection_AddRange_DataColumn3()
		{
			DataTable dt = new DataTable();
			dt.Columns.AddRange(null);
		}

		private DataColumn[] GetDataColumArray()
		{
			DataColumn[] arr = new DataColumn[2];

			arr[0] = new DataColumn("col1",typeof(int));
			arr[1] = new DataColumn("col2",typeof(string));
			
			return arr;
		}

		private DataColumn[] GetBadDataColumArray()
		{
			DataColumn[] arr = new DataColumn[2];

			arr[0] = new DataColumn("col1",typeof(int));
			arr[1] = new DataColumn("col1",typeof(string));
			
			return arr;
		}

		[Test]
		public void Clear1()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Columns.Clear();
			Assert.AreEqual(0,dt.Columns.Count,"dccc1#1");
		}

		[Test]
		public void Clear2()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();

			try
			{
				ds.Tables[0].Columns.Clear();
				Assert.Fail("dccc2#1: Clear failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccc2#2: Clear. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void Clear3()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();
			ds.Tables[1].Constraints.RemoveAt(0);
			ds.Tables[0].Constraints.RemoveAt(0);
			ds.Tables[0].Columns.Clear();
			ds.Tables[1].Columns.Clear();
			Assert.AreEqual(0,ds.Tables[0].Columns.Count,"dccc3#1");
			Assert.AreEqual(0,ds.Tables[1].Columns.Count,"dccc3#2");
		}

		[Test]
		public void GetEnumerator()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			
			int counter=0;
			IEnumerator myEnumerator = dt.Columns.GetEnumerator();
			while (myEnumerator.MoveNext())
			{
				counter++;
			}
			Assert.AreEqual(6,counter,"dccge#1");

			try
			{
				DataColumn col = (DataColumn)myEnumerator.Current;
				Assert.Fail("dccc2#1: GetEnumerator failed to throw InvalidOperationException");
			}
			catch (InvalidOperationException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccc2#2: GetEnumerator. Wrong exception type. Got:" + exc);
			}
		}

		[Test] // this [Int32]
		public void Indexer1 ()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataColumn col;

			col = dt.Columns [5];
			Assert.IsNotNull (col, "#A1");
			Assert.AreEqual ("ParentBool", col.ColumnName, "#A2");

			col = dt.Columns [0];
			Assert.IsNotNull (col, "#B1");
			Assert.AreEqual ("ParentId", col.ColumnName, "#B2");

			col = dt.Columns [3];
			Assert.IsNotNull (col, "#C1");
			Assert.AreEqual ("ParentDateTime", col.ColumnName, "#C2");
		}

		[Test] // this [Int32]
		public void Indexer1_Index_Negative ()
		{
			DataTable dt = DataProvider.CreateParentDataTable ();

			try {
				DataColumn column = dt.Columns [-1];
				Assert.Fail ("#1:" + column);
			} catch (IndexOutOfRangeException ex) {
				// Cannot find column -1
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // this [Int32]
		public void Indexer1_Index_Overflow ()
		{
			DataTable dt = DataProvider.CreateParentDataTable ();

			try {
				DataColumn column = dt.Columns [6];
				Assert.Fail ("#1:" + column);
			} catch (IndexOutOfRangeException ex) {
				// Cannot find column 6
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // this [String]
		public void Indexer2 ()
		{
			DataTable dt = DataProvider.CreateParentDataTable ();
			DataColumnCollection cols = dt.Columns;
			DataColumn col;

			col = cols ["ParentId"];
			Assert.IsNotNull (col, "#A1");
			Assert.AreEqual ("ParentId", col.ColumnName, "#A2");

			col = cols ["parentiD"];
			Assert.IsNotNull (col, "#B1");
			Assert.AreEqual ("ParentId", col.ColumnName, "#B2");

			col = cols ["DoesNotExist"];
			Assert.IsNull (col, "#C");
		}

		[Test] // this [String]
		public void Indexer2_Name_Empty ()
		{
			DataTable dt = new DataTable ();
			DataColumnCollection cols = dt.Columns;

			cols.Add (string.Empty, typeof (int));
			cols.Add ((string) null, typeof (bool));

			DataColumn column = cols [string.Empty];
			Assert.IsNull (column);
		}

		[Test] // this [String]
		public void Indexer2_Name_Null ()
		{
			DataTable dt = DataProvider.CreateParentDataTable ();

			try {
				DataColumn column = dt.Columns [(string) null];
				Assert.Fail ("#1:" + column);
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
#if NET_2_0
				Assert.AreEqual ("name", ex.ParamName, "#5");
#else
				Assert.AreEqual ("key", ex.ParamName, "#5");
#endif
			}
		}

		[Test]
		public void Remove()
		{
			//prepare a DataSet with DataTable to be checked
			DataTable dtSource = new DataTable();
			dtSource.Columns.Add("Col_0", typeof(int)); 
			dtSource.Columns.Add("Col_1", typeof(int)); 
			dtSource.Columns.Add("Col_2", typeof(int)); 
			dtSource.Rows.Add(new object[] {0,1,2}); 

			DataTable dt = null;

			//------Check Remove first column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[0].ColumnName); 
			Assert.AreEqual(2,dt.Columns.Count , "dccr#1");
			Assert.AreEqual(false,dt.Columns.Contains("Col_0"),"dccr#2");
			Assert.AreEqual(1,dt.Rows[0][0],"dccr#3");
			Assert.AreEqual(2,dt.Rows[0][1],"dccr#4");



			//------Check Remove middle column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[1].ColumnName); 
			Assert.AreEqual(2,dt.Columns.Count , "dccr#5");
			Assert.AreEqual(false,dt.Columns.Contains("Col_1"),"dccr#6");
			Assert.AreEqual(0,dt.Rows[0][0],"dccr#7");
			Assert.AreEqual(2,dt.Rows[0][1],"dccr#8");


			//------Check Remove last column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[2].ColumnName); 

			Assert.AreEqual(2,dt.Columns.Count , "dccr#9");
			Assert.AreEqual(false,dt.Columns.Contains("Col_2"),"dccr#10");
			Assert.AreEqual(0,dt.Rows[0][0],"dccr#11");
			Assert.AreEqual(1,dt.Rows[0][1],"dccr#12");


			//------Check Remove column exception---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			try
			{
				dt.Columns.Remove("NotExist"); 
				Assert.Fail("dccr#13: Remove failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccr#14: Remove. Wrong exception type. Got:" + exc);
			}

			dt.Columns.Clear();

			try
			{
				dt.Columns.Remove("Col_0"); 
				Assert.Fail("dccr#15: Remove failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccr#16: Remove. Wrong exception type. Got:" + exc);
			}
		}

		private bool eventOccured = false;

		[Test]
		public void RemoveAt_Integer()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Columns.CollectionChanged+=new CollectionChangeEventHandler(Columns_CollectionChanged1);
			int originalColumnCount = dt.Columns.Count;
			dt.Columns.RemoveAt(0);
			Assert.AreEqual(originalColumnCount-1,dt.Columns.Count,"dccrai#1"); 
			Assert.AreEqual(true,eventOccured,"dccrai#2");

			try
			{
				dt.Columns.RemoveAt(-1);
				Assert.Fail("dccrai#3: RemoveAt failed to throw IndexOutOfRangeException");
			}
			catch (IndexOutOfRangeException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("dccrai#4: RemoveAt. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void Test_Indexes ()
		{
			DataTable dt = new DataTable ();
			DataColumn dc = new DataColumn("A");
			dt.Columns.Add (dc);

			dc = new DataColumn("B");
			dt.Columns.Add (dc);

			dc = new DataColumn("C");
			dt.Columns.Add (dc);

			for(int i=0; i < 10; i++) {
				DataRow dr = dt.NewRow ();
				dr ["A"] = i;
				dr ["B"] = i + 1;
				dr ["C"] = i + 2;
				dt.Rows.Add (dr);
			}

			DataRow[] rows = dt.Select ("A=5");
			Assert.AreEqual (1, rows.Length);

			dt.Columns.Remove ("A");

			dc = new DataColumn ("A");
			dc.DefaultValue = 5;

			dt.Columns.Add (dc);

			rows = dt.Select ("A=5");
			Assert.AreEqual (10, rows.Length);
		}

		private void Columns_CollectionChanged1(object sender, CollectionChangeEventArgs e)
		{
			eventOccured = true;
		}
	}
}
