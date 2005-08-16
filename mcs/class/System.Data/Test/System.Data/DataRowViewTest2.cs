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
using System.ComponentModel;
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class DataRowViewTest2
	{
		[Test] public void BeginEdit()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);

			DataRowView drv = dv[0];

			drv.BeginEdit();
			drv["String1"] = "ChangeValue";

			// check Proposed value
			Assert.AreEqual("ChangeValue" , dt.Rows[0]["String1",DataRowVersion.Proposed] , "DRV1");

			// check Original value
			Assert.AreEqual("1-String1" , dt.Rows[0]["String1",DataRowVersion.Original ] , "DRV2");

			// check IsEdit
			Assert.AreEqual(true, drv.IsEdit , "DRV3");

			// check IsEdit - change another row
			dv[1]["String1"] = "something";
			Assert.AreEqual(true, drv.IsEdit , "DRV4");
		}

		[Test] public void CancelEdit()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);

			DataRowView drv = dv[0];

			drv.BeginEdit();
			drv["String1"] = "ChangeValue";

			// check Proposed value
			Assert.AreEqual("ChangeValue" , dt.Rows[0]["String1",DataRowVersion.Proposed] , "DRV5");

			// check IsEdit
			Assert.AreEqual(true, drv.IsEdit , "DRV6");

			// check Proposed value
			drv.CancelEdit();
			Assert.AreEqual(false, dt.Rows[0].HasVersion(DataRowVersion.Proposed) , "DRV7");

			// check current value
			Assert.AreEqual("1-String1" , dt.Rows[0]["String1",DataRowVersion.Current ] , "DRV8");

			// check IsEdit after cancel edit
			Assert.AreEqual(false, drv.IsEdit , "DRV9");
		}

		[Test] public void CreateChildView_ByDataRelation()
		{
			//create a dataset with two tables, with a DataRelation between them
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataSet ds = new DataSet();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			DataRelation drel = new DataRelation("ParentChild",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(drel);

			//DataView dvChild = null;
			DataView dvParent = new DataView(dtParent);

			DataView dvTmp1 = dvParent[0].CreateChildView(drel);
			DataView dvTmp2 = dvParent[3].CreateChildView(drel);

			// ChildView != null
			Assert.AreEqual(true, dvTmp1!=null, "DRV10");

			// Child view table = ChildTable
			Assert.AreEqual(dtChild , dvTmp1.Table , "DRV11");

			// ChildView1.Table = ChildView2.Table
			Assert.AreEqual(dvTmp2.Table, dvTmp1.Table , "DRV12");

			//the child dataview are different
			// Child DataViews different 
			Assert.AreEqual(false, dvTmp1.Equals(dvTmp2), "DRV13");
		}

		[Test] public void CreateChildView_ByName()
		{
			//create a dataset with two tables, with a DataRelation between them
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataSet ds = new DataSet();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			DataRelation drel = new DataRelation("ParentChild",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(drel);

			//DataView dvChild = null;
			DataView dvParent = new DataView(dtParent);

			DataView dvTmp1 = dvParent[0].CreateChildView("ParentChild");
			DataView dvTmp2 = dvParent[3].CreateChildView("ParentChild");

			// ChildView != null
			Assert.AreEqual(true, dvTmp1!=null, "DRV14");

			// Child view table = ChildTable
			Assert.AreEqual(dtChild , dvTmp1.Table , "DRV15");

			// ChildView1.Table = ChildView2.Table
			Assert.AreEqual(dvTmp2.Table, dvTmp1.Table , "DRV16");

			//the child dataview are different
			// Child DataViews different 
			Assert.AreEqual(false, dvTmp1.Equals(dvTmp2), "DRV17");
		}

		[Test] public void DataView()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);

			DataRowView drv1 = dv[0];
			DataRowView drv2 = dv[4];

			// check DataRowView.DataView 
			Assert.AreEqual(dv, drv1.DataView , "DRV18");

			// compare DataRowView.DataView 
			Assert.AreEqual(drv2.DataView, drv1.DataView , "DRV19");

			//check that the DataRowView still has the same DataView even when the source table changed
			// check that the DataRowView still has the same DataView
			dv.Table = null;

			// Console.WriteLine("*********" + (drv1.DataView == null));
			Assert.AreEqual(true, drv1.DataView == dv , "DRV20");

			//check that the DataRowView has a new DataView
			// check that the DataRowView has a new DataView
			dv = new DataView();
			Assert.AreEqual(false, drv1.DataView.Equals(dv) , "DRV21");
		}

		[Test] public void Delete()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);

			DataRowView drv = dv[0];
			int TableRowsCount = dt.Rows.Count;
			int ViewRowCount = dv.Count;

			// DataView Count
			drv.Delete();
			Assert.AreEqual(dv.Count, ViewRowCount-1, "DRV22");

			//the table count should stay the same until EndEdit is invoked
			// Table Count
			Assert.AreEqual(TableRowsCount, dt.Rows.Count, "DRV23");

			// DataRowState deleted
			Assert.AreEqual(DataRowState.Deleted , drv.Row.RowState , "DRV24");
		}

		[Test] public void EndEdit()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);

			DataRowView drv = dv[0];

			drv.BeginEdit();
			drv["String1"] = "ChangeValue";

			//the row should be stay in edit mode event if changing other rows
			// check IsEdit - change another row
			dv[1]["String1"] = "something";
			Assert.AreEqual(true, drv.IsEdit , "DRV25");

			// check if has Proposed version
			drv.EndEdit();
			Assert.AreEqual(false, dt.Rows[0].HasVersion(DataRowVersion.Proposed) , "DRV26");

			// check Current value
			Assert.AreEqual("ChangeValue" , dt.Rows[0]["String1",DataRowVersion.Current] , "DRV27");

			// check IsEdit
			Assert.AreEqual(false, drv.IsEdit , "DRV28");
		}

		[Test] public void Equals()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);

			DataRowView d1 = dv[0];
			DataRowView d2 = dv[0];

			// DataRowView.Equals
			Assert.AreEqual(d2, d1, "DRV29");
		}

		[Test] public void IsEdit()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);

			DataRowView drv = dv[0];

			// default value
			Assert.AreEqual(false, drv.IsEdit, "DRV30"); 

			// after BeginEdit
			drv.BeginEdit();
			Assert.AreEqual(true, drv.IsEdit, "DRV31"); 

			// after CancelEdit
			drv.CancelEdit();
			Assert.AreEqual(false, drv.IsEdit, "DRV32"); 

			// after BeginEdit again
			drv.BeginEdit();
			Assert.AreEqual(true, drv.IsEdit, "DRV33"); 

			// after EndEdit 
			drv.EndEdit();
			Assert.AreEqual(false, drv.IsEdit, "DRV34"); 
		}

		[Test] public void IsNew()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);

			DataRowView drv = dv[0];

			// existing row
			Assert.AreEqual(false, drv.IsNew , "DRV35"); 

			// add new row
			drv = dv.AddNew();
			Assert.AreEqual(true, drv.IsNew , "DRV36"); 
		}

		[Test] public void Item()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);

			DataRowView drv = dv[0];

			// Item 0
			Assert.AreEqual(dt.Rows[0][0], drv[0], "DRV37"); 

			// Item 4
			Assert.AreEqual(dt.Rows[0][4], drv[4], "DRV38"); 

			// Item -1 - excpetion
			try {
				object o = drv[-1];
				Assert.Fail("DRV39: Indexer Failed to throw IndexOutOfRangeException");
			}
			catch (IndexOutOfRangeException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRV40: Indexer. Wrong exception type. Got:" + exc);
			}
		}

		[Test] public void Item_Property()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);

			DataRowView drv = dv[0];

			// Item 'ParentId'
			Assert.AreEqual(dt.Rows[0]["ParentId"], drv["ParentId"], "DRV41"); 

			// Item 'ParentDateTime'
			Assert.AreEqual(dt.Rows[0]["ParentDateTime"], drv["ParentDateTime"], "DRV42"); 

			// Item invalid - excpetion
			try {
				object o = drv["something"];
				Assert.Fail("DRV43: Indexer Failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRV44: Indexer. Wrong exception type. Got:" + exc);
			}
		}

		[Test] public void Row()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);
			DataRowView drv = null;

			// Compare DataRowView.Row to table row
			drv = dv[3];
			Assert.AreEqual(dt.Rows[3], drv.Row, "DRV45"); 
		}

		[Test] public void RowVersion()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			DataView dv = new DataView(dt);
			dt.Columns[1].DefaultValue = "default";
			DataRowView drv = dv[0];

			dt.Rows.Add(new object[] {99});
			dt.Rows[1].Delete();
			dt.Rows[2].BeginEdit();
			dt.Rows[2][1] = "aaa";

			dv.RowStateFilter=DataViewRowState.CurrentRows ;
			// check Current
			Assert.AreEqual(DataRowVersion.Current, drv.RowVersion, "DRV46");

			dv.RowStateFilter=DataViewRowState.Deleted ;
			// check Original
			Assert.AreEqual(DataRowVersion.Original , drv.RowVersion, "DRV47");
		}
	}
}
