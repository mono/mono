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
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests_System.Data
{
	[TestFixture] public class DataTableCollectionTest2
	{
		private int counter = 0;

		[Test] public void Add()
		{
			// Adding computed column to a data set
			DataSet ds = new DataSet();
			ds.Tables.Add(new DataTable("Table"));
			ds.Tables[0].Columns.Add(new DataColumn("EmployeeNo", typeof(string)));
			ds.Tables[0].Rows.Add(new object[] {"Maciek"});
			ds.Tables[0].Columns.Add("ComputedColumn", typeof(object), "EmployeeNo");

			Assert.AreEqual("EmployeeNo", ds.Tables[0].Columns["ComputedColumn"].Expression, "DTC1");
		}

		[Test]
		public void AddTwoTables()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add();
			Assert.AreEqual("Table1", ds.Tables[0].TableName , "DTC2");
			//Assert.AreEqual(ds.Tables[0].TableName,"Table1");
			ds.Tables.Add();
			Assert.AreEqual("Table2", ds.Tables[1].TableName , "DTC3");
			//Assert.AreEqual(ds.Tables[1].TableName,"Table2");
		}

		[Test]
		public void AddRange()
		{
			DataSet ds = new DataSet();

			DataTable[] arr = new DataTable[2];

			arr[0] = new DataTable("NewTable1");
			arr[1] = new DataTable("NewTable2");

			ds.Tables.AddRange(arr);
			Assert.AreEqual("NewTable1", ds.Tables[0].TableName, "DTC4");
			Assert.AreEqual("NewTable2", ds.Tables[1].TableName, "DTC5");
		}
		[Test]
		public void AddRange_NullValue()
		{
			DataSet ds = new DataSet();
			ds.Tables.AddRange(null);
		}

		[Test]
		public void AddRange_ArrayWithNull()
		{
			DataSet ds = new DataSet();
			DataTable[] arr = new DataTable[2];
			arr[0] = new DataTable("NewTable1");
			arr[1] = (DataTable)null ;
			ds.Tables.AddRange(arr);
			Assert.AreEqual("NewTable1", ds.Tables[0].TableName, "DTC6");
			Assert.AreEqual(1, ds.Tables.Count, "DTC7");
		}

		[Test]
		public void CanRemove()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add();
			Assert.AreEqual(true, ds.Tables.CanRemove(ds.Tables[0]), "DTC8"); 
		}

		[Test]
		public void CanRemove_NullValue()
		{
			DataSet ds = new DataSet();
			Assert.AreEqual(false, ds.Tables.CanRemove(null), "DTC9");
		}

		[Test]
		public void CanRemove_TableDoesntBelong()
		{
			DataSet ds = new DataSet();
			DataSet ds1 = new DataSet();
			ds1.Tables.Add();
			Assert.AreEqual(false, ds.Tables.CanRemove(ds1.Tables[0]), "DTC10");
		}

		[Test]
		public void CanRemove_PartOfRelation()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add(DataProvider.CreateParentDataTable());
			ds.Tables.Add(DataProvider.CreateChildDataTable());

			ds.Relations.Add("rel",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"],false);

			Assert.AreEqual(false, ds.Tables.CanRemove(ds.Tables[0]), "DTC11");
			Assert.AreEqual(false, ds.Tables.CanRemove(ds.Tables[1]), "DTC12");
		}
		[Test]
		public void CanRemove_PartOfConstraint()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();
			Assert.AreEqual(false, ds.Tables.CanRemove(ds.Tables[0]), "DTC13"); //Unique 
			Assert.AreEqual(false, ds.Tables.CanRemove(ds.Tables[1]), "DTC14"); //Foreign 
		}

		[Test]
		public void CollectionChanged()
		{
			counter = 0;
			DataSet ds = new DataSet();
			ds.Tables.CollectionChanged+=new System.ComponentModel.CollectionChangeEventHandler(Tables_CollectionChanged);
			ds.Tables.Add();
			ds.Tables.Add();
			Assert.AreEqual(2, counter, "DTC15");

			ds.Tables.Remove(ds.Tables[0]);
			ds.Tables.Remove(ds.Tables[0]);
			Assert.AreEqual(4, counter, "DTC16");
		}

		private void Tables_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
		{
			counter++;
		}

		[Test]
		public void CollectionChanging()
		{
			counter = 0;
			DataSet ds = new DataSet();
			ds.Tables.CollectionChanging+=new System.ComponentModel.CollectionChangeEventHandler(Tables_CollectionChanging);
			ds.Tables.Add();
			ds.Tables.Add();
			Assert.AreEqual(2, counter, "DTC17");

			ds.Tables.Remove(ds.Tables[0]);
			ds.Tables.Remove(ds.Tables[0]);
			Assert.AreEqual(4, counter, "DTC18");
		}

		private void Tables_CollectionChanging(object sender, System.ComponentModel.CollectionChangeEventArgs e)
		{
			counter++;
		}

		[Test]
		public void Contains()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add("NewTable1");
			ds.Tables.Add("NewTable2");

			Assert.AreEqual(true, ds.Tables.Contains("NewTable1"), "DTC19");
			Assert.AreEqual(true, ds.Tables.Contains("NewTable2"), "DTC20");
			Assert.AreEqual(false, ds.Tables.Contains("NewTable3"), "DTC21");

			ds.Tables["NewTable1"].TableName = "Tbl1";
			Assert.AreEqual(false, ds.Tables.Contains("NewTable1"), "DTC22");
			Assert.AreEqual(true, ds.Tables.Contains("Tbl1"), "DTC23");
		}

		[Test]
		public void CopyTo()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add();
			ds.Tables.Add();
			DataTable[] arr = new DataTable[2];
			ds.Tables.CopyTo(arr,0);
			Assert.AreEqual("Table1", ((DataTable)arr[0]).TableName, "DTC24");
			Assert.AreEqual("Table2", ((DataTable)arr[1]).TableName, "DTC25");
		}

		[Test]
		public void Count()
		{
			DataSet ds = new DataSet();
			Assert.AreEqual(0, ds.Tables.Count, "DTC26");

			ds.Tables.Add();
			Assert.AreEqual(1, ds.Tables.Count, "DTC27");

			ds.Tables.Add();
			Assert.AreEqual(2, ds.Tables.Count, "DTC28");

			ds.Tables.Remove("Table1");
			Assert.AreEqual(1, ds.Tables.Count, "DTC29");

			ds.Tables.Remove("Table2");
			Assert.AreEqual(0, ds.Tables.Count, "DTC30");
		}

		[Test]
		public void GetEnumerator()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add();
			ds.Tables.Add();
			int count=0;

			System.Collections.IEnumerator myEnumerator = ds.Tables.GetEnumerator();

			while (myEnumerator.MoveNext())
			{
				Assert.AreEqual("Table",  ((DataTable) myEnumerator.Current).TableName.Substring(0,5), "DTC31");
				count++;
			}
			Assert.AreEqual(2, count, "DTC32");
		}
		public void IndexOf_ByDataTable()
		{
			DataSet ds = new DataSet();
			DataTable dt = new DataTable("NewTable1");
			DataTable dt1 = new DataTable("NewTable2");
			ds.Tables.AddRange(new DataTable[] {dt,dt1});

			Assert.AreEqual(0, ds.Tables.IndexOf(dt), "DTC33");
			Assert.AreEqual(1, ds.Tables.IndexOf(dt1), "DTC34");

			ds.Tables.IndexOf((DataTable)null);

			DataTable dt2 = new DataTable("NewTable2");

			Assert.AreEqual(-1, ds.Tables.IndexOf(dt2), "DTC35");
		}

		public void IndexOf_ByName()
		{
			DataSet ds = new DataSet();
			DataTable dt = new DataTable("NewTable1");
			DataTable dt1 = new DataTable("NewTable2");
			ds.Tables.AddRange(new DataTable[] {dt,dt1});

			Assert.AreEqual(0, ds.Tables.IndexOf("NewTable1"), "DTC36");
			Assert.AreEqual(1, ds.Tables.IndexOf("NewTable2"), "DTC37");

			ds.Tables.IndexOf((string)null);

			Assert.AreEqual(-1, ds.Tables.IndexOf("NewTable3"), "DTC38");
		}

		[Test]
		public void Item()
		{
			DataSet ds = new DataSet();
			DataTable dt = new DataTable("NewTable1");
			DataTable dt1 = new DataTable("NewTable2");
			ds.Tables.AddRange(new DataTable[] {dt,dt1});

			Assert.AreEqual(dt, ds.Tables[0], "DTC39");
			Assert.AreEqual(dt1, ds.Tables[1], "DTC40");
			Assert.AreEqual(dt, ds.Tables["NewTable1"], "DTC41");
			Assert.AreEqual(dt1, ds.Tables["NewTable2"], "DTC42");
		}

		[Test]
		public void DataTableCollection_Add_D1()
		{
			DataSet ds = new DataSet();
			DataTable dt = new DataTable("NewTable1");
			ds.Tables.Add(dt);
			Assert.AreEqual("NewTable1",ds.Tables[0].TableName,"DTC43");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DataTableCollection_Add_D2()
		{
			DataSet ds = new DataSet();

			ds.Tables.Add((DataTable)null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DataTableCollection_Add_D3()
		{
			DataSet ds = new DataSet();
			DataSet ds1 = new DataSet();
			ds1.Tables.Add();

			ds.Tables.Add(ds1.Tables[0]);
		}

		[Test]
		[ExpectedException(typeof(DuplicateNameException))]
		public void DataTableCollection_Add_D4()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add();

			DataTable dt = new DataTable("Table1");
			ds.Tables.Add(dt);
		}

		[Test]
		public void DataTableCollection_Add_S1()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add("NewTable1");
			Assert.AreEqual("NewTable1",ds.Tables[0].TableName,"DTC44");
			ds.Tables.Add("NewTable2");
			Assert.AreEqual("NewTable2",ds.Tables[1].TableName,"DTC45");
		}

		[Test]
		[ExpectedException(typeof(DuplicateNameException))]
		public void DataTableCollection_Add_S2()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add("NewTable1");

			ds.Tables.Add("NewTable1");
		}

		[Test]
		public void DataTableCollection_Clear1()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add();
			ds.Tables.Add();
			ds.Tables.Clear();
			Assert.AreEqual(0,ds.Tables.Count,"DTC46");

		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void DataTableCollection_Clear2()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add();
			ds.Tables.Add();
			ds.Tables.Clear();

			ds.Tables[0].TableName = "Error";
		}

		[Test]
		public void DataTableCollection_Remove_D1()
		{
			DataSet ds = new DataSet();
			DataTable dt = new DataTable("NewTable1");
			DataTable dt1 = new DataTable("NewTable2");
			ds.Tables.AddRange(new DataTable[] {dt,dt1});

			ds.Tables.Remove(dt);
			Assert.AreEqual(1,ds.Tables.Count,"DTC47");
			Assert.AreEqual(dt1,ds.Tables[0],"DTC48");
			ds.Tables.Remove(dt1);
			Assert.AreEqual(0,ds.Tables.Count,"DTC49");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DataTableCollection_Remove_D2()
		{
			DataSet ds = new DataSet();
			DataTable dt = new DataTable("NewTable1");

			ds.Tables.Remove(dt);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DataTableCollection_Remove_D3()
		{
			DataSet ds = new DataSet();

			ds.Tables.Remove((DataTable)null);
		}

		[Test]
		public void DataTableCollection_Remove_S1()
		{
			DataSet ds = new DataSet();
			DataTable dt = new DataTable("NewTable1");
			DataTable dt1 = new DataTable("NewTable2");
			ds.Tables.AddRange(new DataTable[] {dt,dt1});

			ds.Tables.Remove("NewTable1");
			Assert.AreEqual(1,ds.Tables.Count,"DTC50");
			Assert.AreEqual(dt1,ds.Tables[0],"DTC51");
			ds.Tables.Remove("NewTable2");
			Assert.AreEqual(0,ds.Tables.Count,"DTC52");	
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DataTableCollection_Remove_S2()
		{
			DataSet ds = new DataSet();

			ds.Tables.Remove("NewTable2");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DataTableCollection_Remove_S3()
		{
			DataSet ds = new DataSet();

			ds.Tables.Remove((string)null);
		}

		[Test]
		public void DataTableCollection_RemoveAt_I1()
		{
			DataSet ds = new DataSet();
			DataTable dt = new DataTable("NewTable1");
			DataTable dt1 = new DataTable("NewTable2");
			ds.Tables.AddRange(new DataTable[] {dt,dt1});

			ds.Tables.RemoveAt(1);
			Assert.AreEqual(dt,ds.Tables[0],"DTC53");
			ds.Tables.RemoveAt(0);
			Assert.AreEqual(0,ds.Tables.Count,"DTC54");
		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void DataTableCollection_RemoveAt_I2()
		{
			DataSet ds = new DataSet();

			ds.Tables.RemoveAt(-1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DataTableCollection_RemoveAt_I3()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();

			ds.Tables.RemoveAt(0); //Parent table
		}

#if NET_2_0
		[Test]
		public void AddTable_DiffNamespaceTest ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add ("table", "namespace1");
			ds.Tables.Add ("table", "namespace2");
			Assert.AreEqual (2, ds.Tables.Count, "#1");

			try {
				ds.Tables.Add ("table", "namespace1");
				Assert.Fail ("#2");
			} catch (DuplicateNameException e) { }

			ds.Tables.Add ("table");
			try {
				ds.Tables.Add ("table", null);
				Assert.Fail ("#4");
			} catch (DuplicateNameException e) { }
		}

		[Test]
		public void Contains_DiffNamespaceTest ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add ("table");
			Assert.IsTrue (ds.Tables.Contains ("table"), "#1");

			ds.Tables.Add ("table", "namespace1");
			ds.Tables.Add ("table", "namespace2");

			// Should fail if it cannot be resolved to a single table
			Assert.IsFalse (ds.Tables.Contains ("table"));

			try {
				ds.Tables.Contains ("table", null);
				Assert.Fail ("#2");
			} catch (ArgumentNullException e) { }

			Assert.IsTrue (ds.Tables.Contains ("table", "namespace1"), "#4");
			Assert.IsFalse (ds.Tables.Contains ("table", "namespace3"), "#5");
		}

		[Test]
		public void IndexOf_DiffNamespaceTest ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add ("table");
			Assert.AreEqual (0, ds.Tables.IndexOf ("table"), "#1");
			ds.Tables.Add ("table", "namespace1");
			ds.Tables.Add ("table", "namespace2");
			Assert.AreEqual (-1, ds.Tables.IndexOf ("table"), "#2");
			Assert.AreEqual (2, ds.Tables.IndexOf ("table", "namespace2"), "#3");
			Assert.AreEqual (1, ds.Tables.IndexOf ("table", "namespace1"), "#4");
		}

		[Test]
		public void Remove_DiffNamespaceTest ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add ("table");
			ds.Tables.Add ("table", "namespace1");
			ds.Tables.Add ("table", "namespace2");

			try {
				ds.Tables.Remove ("table");
				Assert.Fail ("#1");
			} catch (ArgumentException e) { }

			ds.Tables.Remove ("table", "namespace2");
			Assert.AreEqual (2, ds.Tables.Count, "#3");
			Assert.AreEqual ("namespace1", ds.Tables [1].Namespace, "#4");

			try {
				ds.Tables.Remove ("table", "namespace2");
				Assert.Fail ("#5");
			} catch (ArgumentException e) { }
		}
#endif
	}
}
