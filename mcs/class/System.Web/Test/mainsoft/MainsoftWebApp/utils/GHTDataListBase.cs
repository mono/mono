//
// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Vladimir Krasnov <vladimirk@mainsoft.com>
//   
// 
// Copyright (c) 2002-2005 Mainsoft Corporation.
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
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using GHTWebControls;
using System.Drawing;
using System.Data;

namespace GHTTests
{

	/// <summary>
	/// Summary description for GHTDataListBase.
	/// </summary>
	public class GHTDataListBase:GHTBaseWeb
	{
		BaseDataList mActiveDataList;
		public GHTDataListBase():base()
		{
				
		}
		#region Tests
		protected static DataTable GHTGetSampleDataSource()
		{
			DataTable SampleDT;

			SampleDT = new DataTable("Sample");
			DataColumn col = new DataColumn("colA",typeof(System.String));
			SampleDT.Columns.Add (col);
			col = new DataColumn("colB",typeof(System.Int32));
			SampleDT.Columns.Add (col);
			col = new DataColumn("colC",typeof(System.DateTime));
			SampleDT.Columns.Add (col);
			SampleDT.Rows.Add(new object[]{"row 1",11111,"01/01/2003"});
			SampleDT.Rows.Add(new object[]{"row 22222",2,"02/02/2004"});
			//					SampleDT.Rows.Add(new object[]{"row 1",1});
			//					SampleDT.Rows.Add(new object[]{"row 2",2});

			//				}
			return SampleDT;
		}
		protected static DataSet GHTGetSampleDataSourceDS()
		{

			DataSet SampleDS = new DataSet("SampleDS");
			SampleDS.Tables.Add( GHTGetSampleDataSource());
			return SampleDS;

		}
		protected static string[] GHTGetSampleDataSourceArray()
		{
			return new string[]{"A","B","C"};

		}
		protected static Hashtable GHTGetSampleDataSourceCol()
		{
			Hashtable col = new Hashtable();
			col.Add ("Key A","Value A");
			col.Add ("Key B","Value B");
			col.Add ("Key C","Value C");
			return col;	
		}
		#endregion
		protected void GHTBuildUnboundSampleDataList(BaseDataList ctl)
		{
			DataList lst = (DataList)ctl;
			lst.ItemTemplate = new MyItemTemplate();
		}			  
		protected void GHTBuildSampleDataList(BaseDataList ctl)
		{
			DataList lst = (DataList)ctl;
			lst.ItemTemplate = new MyItemTemplate();
			lst.DataSource = GHTGetSampleDataSource();
			lst.DataBind();
		}			  
		protected void GHTBuildUnboundSampleDataGrid(BaseDataList ctl)
		{
			DataGrid grid = (DataGrid)ctl;
			BoundColumn col = new BoundColumn();
			col.DataField = "colA";
			grid.Columns.Add(col);
			col = new BoundColumn();
			col.DataField = "colB";
			grid.Columns.Add(col);
			col = new BoundColumn();
			col.DataField = "colC";
			grid.Columns.Add(col);
			grid.AutoGenerateColumns =false;

		}
		protected void GHTBuildSampleDataGrid(BaseDataList ctl)
		{
			DataGrid grid = (DataGrid)ctl;
			BoundColumn col = new BoundColumn();
			col.DataField = "colA";
			grid.Columns.Add(col);

			col = new BoundColumn();
			col.DataField = "colB";
			grid.Columns.Add(col);

			col = new BoundColumn();
			col.DataField = "colC";
			grid.Columns.Add(col);

			grid.AutoGenerateColumns =false;
			col = new BoundColumn();
			grid.DataSource = GHTGetSampleDataSource();
			grid.DataBind();

		}
		private class MyItemTemplate : ITemplate
		{

			public void InstantiateIn(Control container ) 
			{
				TextBox ctl1 = new  TextBox();
				ctl1.ID = "MyTextBox1";
				ctl1.DataBinding +=new EventHandler(BindColA);
				container.Controls.Add(ctl1);
				TextBox ctl2 = new  TextBox();
				ctl2.ID = "MyTextBox2";
				ctl2.DataBinding +=new EventHandler(BindColB);
				container.Controls.Add(ctl2);
			}
			private void BindColA(Object sender ,EventArgs e )
			{
				TextBox ctl = (TextBox)sender;
				DataListItem DLI = (DataListItem)ctl.NamingContainer;
				DataRowView drv = (DataRowView)DLI.DataItem;
				ctl.Text = drv["colA"].ToString();
			}
			private void BindColB(Object sender ,EventArgs e )
			{
				TextBox ctl = (TextBox)sender;
				DataListItem DLI = (DataListItem)ctl.NamingContainer;
				DataRowView drv = (DataRowView)DLI.DataItem;
				ctl.Text = drv["colB"].ToString();
			}
		}
		private class DataSourceClass
		{
			private string mcolA;
			private string mcolB;
			private string mcolC;
			public DataSourceClass(string colA, string colB, string colC)
			{
				mcolA = colA;
				mcolB = colB;
				mcolC = colC;
			}
			internal string colA
			{
				get {return mcolA;}
			}
			internal string colB
			{
				get {return mcolB;}
			}
			internal string colC
			{
				get {return mcolC;}
			}
		}

		#region Private Methods
		// helper utility to create a new sub test
		private void GHTDataListSubTestBegin(Type ctrlType, string description)
		{
			mActiveDataList = (BaseDataList)GHTElementClone(ctrlType);
			GHTSubTestBegin(description);
			GHTActiveSubTest.Controls.Add(mActiveDataList);
		}
		#endregion
	}
	public class GHTDataListSampleClass
	{
		public string colA;
		public int colB;
		public DateTime colC;
		public GHTDataListSampleClass(string lcolA, int lcolB, DateTime lcolC)
		{
			colA = lcolA;
			colB = lcolB;
			colC = lcolC;
		}
		//			public string colA
		//			{
		//				get{return mcolA;}
		//				set {mcolA = value;}
		//			}
		//			public int colB
		//			{
		//				get{return mcolB;}
		//				set {mcolB = value;}
		//			}
		//			public DateTime colC
		//			{
		//				get{return mcolC;}
		//				set {mcolC = value;}
		//			}



	}
}