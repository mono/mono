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
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class DataRowCollectionTest2
	{
		[Test] public void CopyTo()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataRow[] arr = new DataRow[dt.Rows.Count];
			dt.Rows.CopyTo(arr,0);
			Assert.AreEqual(dt.Rows.Count, arr.Length, "DRWC1");

			int index=0;
			foreach (DataRow dr in dt.Rows)
			{
				Assert.AreEqual(dr, arr[index], "DRWC2");
				index++;
			}
		}

		[Test] public void Count()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			Assert.AreEqual(6, dt.Rows.Count, "DRWC3");
			dt.Rows.Remove(dt.Rows[0]);
			Assert.AreEqual(5, dt.Rows.Count, "DRWC4");
			dt.Rows.Add(new object[] {1,"1-String1","1-String2",new DateTime(2005,1,1,0,0,0,0),1.534,true});
			Assert.AreEqual(6, dt.Rows.Count, "DRWC5");
		}

		[Test] public void GetEnumerator()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			IEnumerator myEnumerator = dt.Rows.GetEnumerator();
			int index=0;
			while (myEnumerator.MoveNext())
			{
				Assert.AreEqual(dt.Rows[index], (DataRow)myEnumerator.Current, "DRWC6");
				index++;
			}
			Assert.AreEqual(index, dt.Rows.Count, "DRWC7");
		}

		[Test] public void RemoveAt_ByIndex()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			int counter = dt.Rows.Count;
			dt.PrimaryKey=  new DataColumn[] {dt.Columns[0]};
			dt.Rows.RemoveAt(3);
			Assert.AreEqual(counter-1, dt.Rows.Count, "DRWC8");
			Assert.AreEqual(null, dt.Rows.Find(4), "DRWC9");
		}

		[Test] public void Remove_ByDataRow()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			int counter = dt.Rows.Count;
			dt.PrimaryKey=  new DataColumn[] {dt.Columns[0]};
			Assert.AreEqual(dt.Rows[0], dt.Rows.Find(1), "DRWC10");
			dt.Rows.Remove(dt.Rows[0]);
			Assert.AreEqual(counter-1, dt.Rows.Count, "DRWC11");
			Assert.AreEqual(null, dt.Rows.Find(1), "DRWC12");
		}

		[Test]
		public void DataRowCollection_Add_D1()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Rows.Clear();
			DataRow dr = dt.NewRow();
			dr["ParentId"] = 10;
			dr["String1"] = "string1";
			dr["String2"] = string.Empty;
			dr["ParentDateTime"] = new DateTime(2004,12,15);
			dr["ParentDouble"] = 3.14;
			dr["ParentBool"] = false;

			dt.Rows.Add(dr);

			Assert.AreEqual(1,dt.Rows.Count,"RDWC13");
			Assert.AreEqual(dr,dt.Rows[0],"DRWC14");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DataRowCollection_Add_D2()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Rows.Add(dt.Rows[0]);			
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DataRowCollection_Add_D3()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Rows.Add((DataRow)null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DataRowCollection_Add_D4()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataTable dt1 = DataProvider.CreateParentDataTable();

			dt.Rows.Add(dt1.Rows[0]);
		}

		[Test]
		public void DataRowCollection_Add_O1()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Rows.Clear();
			dt.Rows.Add(new object[] {1,"1-String1","1-String2",new DateTime(2005,1,1,0,0,0,0),1.534,true});
			Assert.AreEqual(1,dt.Rows.Count,"DRWC15");
			Assert.AreEqual(1,dt.Rows[0]["ParentId"],"DRWC16");
			Assert.AreEqual("1-String1",dt.Rows[0]["String1"],"DRWC17");
			Assert.AreEqual("1-String2",dt.Rows[0]["String2"],"DRWC18");
			Assert.AreEqual(new DateTime(2005,1,1,0,0,0,0),dt.Rows[0]["ParentDateTime"],"DRWC19");
			Assert.AreEqual(1.534,dt.Rows[0]["ParentDouble"],"DRWC20");
			Assert.AreEqual(true,dt.Rows[0]["ParentBool"],"DRWC21");

		}

		[Test]
		public void DataRowCollection_Add_O2()
		{		
			DataTable dt = DataProvider.CreateParentDataTable();
			int count = dt.Rows.Count;
			dt.Rows.Add(new object[] {8,"1-String1","1-String2",new DateTime(2005,1,1,0,0,0,0),1.534});
			Assert.AreEqual(count+1,dt.Rows.Count,"DRWC14");
		}

//		[Test]
//		[ExpectedException(typeof(ArgumentException))]
//		public void DataRowCollection_Add_O3()
//		{
//			DataTable dt = DataProvider.CreateParentDataTable();
//			dt.Rows.Add(new object[] {8,"1-String1","1-String2",new DateTime(2005,1,1,0,0,0,0),1.534});			
//		}

		[Test]
		[ExpectedException(typeof(NullReferenceException))]
		public void DataRowCollection_Add_O4()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Rows.Add((Object[])null);
		}

		[Test]
		public void FindByKey ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.PrimaryKey = new DataColumn[] {table.Columns [0]};

			table.Rows.Add (new object[] {1});
			table.Rows.Add (new object[] {2});
			table.Rows.Add (new object[] {3});
			table.AcceptChanges ();

			Assert.IsNotNull (table.Rows.Find (new object[] {1}), "#1");

			table.Rows[0].Delete ();
			Assert.IsNull (table.Rows.Find (new object[] {1}), "#2");

			table.RejectChanges ();
			Assert.IsNotNull (table.Rows.Find (new object[] {1}), "#3");
		}

		[Test]
		public void FindByKey_VerifyOrder ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.PrimaryKey = new DataColumn[] {table.Columns [0]};

			table.Rows.Add (new object[] {1});
			table.Rows.Add (new object[] {2});
			table.Rows.Add (new object[] {1000});
			table.AcceptChanges ();

			table.Rows [1][0] = 100;
			Assert.IsNotNull (table.Rows.Find (100), "#1");

			table.Rows [2][0] = 999;
			Assert.IsNotNull (table.Rows.Find (999), "#2");
			Assert.IsNotNull (table.Rows.Find (100), "#3");
		}

		[Test]
		public void FindByKey_DuringDataLoad ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.PrimaryKey = new DataColumn[] {table.Columns [0]};

			table.Rows.Add (new object[] {1});
			table.Rows.Add (new object[] {2});
			table.AcceptChanges ();

			table.BeginLoadData ();
			table.LoadDataRow (new object[] {1000}, false);
			Assert.IsNotNull (table.Rows.Find (1), "#1");
			Assert.IsNotNull (table.Rows.Find (1000), "#2");
			table.EndLoadData ();
			Assert.IsNotNull (table.Rows.Find (1000), "#3");
		}

		[Test]
		public void DataRowCollection_Clear1()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			int count = dt.Rows.Count;
			Assert.AreEqual(count !=0,true);
			dt.Rows.Clear();
			Assert.AreEqual(0,dt.Rows.Count,"DRWC15");
		}

		[Test]
		[ExpectedException(typeof(InvalidConstraintException))]
		public void DataRowCollection_Clear2()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();

			ds.Tables[0].Rows.Clear(); //Try to clear the parent table			
		}

		[Test]
		public void DataRowCollection_Contains_O1()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.PrimaryKey=  new DataColumn[] {dt.Columns[0]};
			
			Assert.AreEqual(true,dt.Rows.Contains(1),"DRWC16");
			Assert.AreEqual(false,dt.Rows.Contains(10),"DRWC17");
		}

		[Test]
		[ExpectedException(typeof(MissingPrimaryKeyException))]
		public void DataRowCollection_Contains_O2()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			Assert.AreEqual(false,dt.Rows.Contains(1),"DRWC18");
		}

		[Test]
		public void DataRowCollection_Contains_O3()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.PrimaryKey= new DataColumn[] {dt.Columns[0],dt.Columns[1]};

			//Prepare values array
			object[] arr = new object[2];
			arr[0] = 1;
			arr[1] = "1-String1";

			Assert.AreEqual(true,dt.Rows.Contains( (object[])arr),"DRWC19");

			arr[0] = 8;

			Assert.AreEqual(false,dt.Rows.Contains( (object[])arr),"DRWC20");

		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DataRowCollection_Contains_O4()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.PrimaryKey= new DataColumn[] {dt.Columns[0],dt.Columns[1]};

			//Prepare values array
			object[] arr = new object[1];
			arr[0] = 1;

			Assert.AreEqual(false,dt.Rows.Contains((object[]) arr),"DRWC21");
		}

		[Test]
		public void DataRowCollection_Find_O1()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.PrimaryKey=  new DataColumn[] {dt.Columns[0]};
			
			Assert.AreEqual(dt.Rows[0],dt.Rows.Find(1),"DRWC22");
			Assert.AreEqual(null,dt.Rows.Find(10),"DRWC23");
		}

		[Test]
		[ExpectedException(typeof(MissingPrimaryKeyException))]
		public void DataRowCollection_Find_O2()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			Assert.AreEqual(null,dt.Rows.Find(1),"DRWC24");
		}

		[Test]
		public void DataRowCollection_Find_O3()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.PrimaryKey= new DataColumn[] {dt.Columns[0],dt.Columns[1]};

			//Prepare values array
			object[] arr = new object[2];
			arr[0] = 2;
			arr[1] = "2-String1";

			Assert.AreEqual(dt.Rows[1],dt.Rows.Find( (object[])arr),"DRWC25");

			arr[0] = 8;

			Assert.AreEqual(null,dt.Rows.Find( (object[])arr),"DRWC26");

		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void DataRowCollection_Find_O4()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.PrimaryKey= new DataColumn[] {dt.Columns[0],dt.Columns[1]};

			//Prepare values array
			object[] arr = new object[1];
			arr[0] = 1;
			
			Assert.AreEqual(null,dt.Rows.Find((object[]) arr),"DRWC27");
		}

		[Test]
		public void DataRowCollection_InsertAt_DI1()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataRow dr =  GetNewDataRow(dt);
			dt.Rows.InsertAt(dr,0);

			Assert.AreEqual(dr,dt.Rows[0],"DRWC28"); //Begin
		}

		[Test]
		public void DataRowCollection_InsertAt_DI2()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataRow dr =  GetNewDataRow(dt);
			dt.Rows.InsertAt(dr,3);

			Assert.AreEqual(dr,dt.Rows[3],"DRWC29"); //Middle
		}

		[Test]
		public void DataRowCollection_InsertAt_DI3()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataRow dr =  GetNewDataRow(dt);
			dt.Rows.InsertAt(dr,300);

			Assert.AreEqual(dr,dt.Rows[dt.Rows.Count-1],"DRWC30"); //End
		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void DataRowCollection_InsertAt_DI4()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataRow dr =  GetNewDataRow(dt);

			dt.Rows.InsertAt(dr,-1);
		}

		private DataRow  GetNewDataRow(DataTable dt)
		{
			DataRow dr = dt.NewRow();
			dr["ParentId"] = 10;
			dr["String1"] = "string1";
			dr["String2"] = string.Empty;
			dr["ParentDateTime"] = new DateTime(2004,12,15);
			dr["ParentDouble"] = 3.14;
			dr["ParentBool"] = false;
			return dr;
		}

		[Test]
		public void DataRowCollection_Item1()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			int index=0;

			foreach (DataRow dr in dt.Rows)
			{
				Assert.AreEqual(dr,dt.Rows[index],"DRWC31");
				index++;
			}
		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void DataRowCollection_Item2()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			DataRow dr =  dt.Rows[-1];
		}
	}
}
