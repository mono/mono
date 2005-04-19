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

using GHTUtils;
using GHTUtils.Base;

namespace tests.system_data_dll.System_Data
{
[TestFixture] public class DataRow_SetParentRow_D : GHTBase
{
	[Test] public void Main()
	{
		DataRow_SetParentRow_D tc = new DataRow_SetParentRow_D();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataRow_SetParentRow_D");
			tc.run();
		}
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			tc.EndTest(exp);
		}
	}

	//Activate This Construntor to log All To Standard output
	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}


	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

	public void run()
	{
		Exception exp = null;

		DataRow drParent,drChild;
		DataRow drArrExcepted,drArrResult;
		DataTable dtChild,dtParent;
		DataSet ds = new DataSet();
		//Create tables
		dtChild = GHTUtils.DataProvider.CreateChildDataTable();
		dtParent= GHTUtils.DataProvider.CreateParentDataTable(); 
		//Add tables to dataset
		ds.Tables.Add(dtChild);
		ds.Tables.Add(dtParent);
		//Add Relation
		DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
		ds.Relations.Add(dRel);

		drParent = dtParent.Rows[0];
		drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

		drChild.SetParentRow(drParent);
				
		//Get Excepted result
		drArrExcepted = drParent;
		//Get Result DataRowVersion.Current
		drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Current);
				
		try
		{
			BeginCase("SetParentRow");
			base.Compare( drArrResult, drArrExcepted );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
	}
	public void testMore()
	{
		DataSet ds = DataProvider.CreateForigenConstraint();
		DataRow drParent = ds.Tables[0].Rows[0];
		try
		{
			//DataRow[] drArray =  ds.Tables[1].Rows[0].GetParentRows(ds.Tables[1].ParentRelations[0]);
			ds.Tables[1].Rows[0].SetParentRow(drParent);
			
		}
		catch (Exception ex)
		{
			throw ex;
		}
		
	}
	public void test()
	{
		BeginCase("test SetParentRow");
		DataTable parent = DataProvider.CreateParentDataTable();
		DataTable child = DataProvider.CreateChildDataTable();
		DataRow dr = parent.Rows[0];
		dr.Delete();
		parent.AcceptChanges();

		try
		{
			child.Rows[0].SetParentRow(dr);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public void checkForLoops()
	{
		DataSet ds = new DataSet();
		//Create tables
		DataTable  dtChild = GHTUtils.DataProvider.CreateChildDataTable();
		DataTable dtParent= GHTUtils.DataProvider.CreateParentDataTable(); 
		//Add tables to dataset
		ds.Tables.Add(dtChild);
		ds.Tables.Add(dtParent);

		dtChild.Rows.Clear();
		dtParent.Rows.Clear();

		dtParent.ChildRelations.Add(dtParent.Columns[0],dtChild.Columns[0]);
		try
		{
			dtChild.ChildRelations.Add(dtChild.Columns[0],dtParent.Columns[0]);
		}
		catch (Exception ex)
		{
			throw ex;
		}

		dtChild.Rows[0].SetParentRow(dtParent.Rows[0]);
		dtParent.Rows[0].SetParentRow(dtChild.Rows[0]);

	

	}

	public void checkForLoopsAdvenced()
	{
		
		//Create tables
		DataTable  dtChild = new DataTable();
		dtChild.Columns.Add("Col1",typeof(int));
		dtChild.Columns.Add("Col2",typeof(int));


		try
		{
			DataRelation drl = new DataRelation("drl1",dtChild.Columns[0],dtChild.Columns[1]);
			dtChild.ChildRelations.Add(drl);
		}
		catch (Exception ex)
		{
			throw ex;
		}
		dtChild.Rows[0].SetParentRow(dtChild.Rows[1]);
		dtChild.Rows[1].SetParentRow(dtChild.Rows[0]);

	

	}
}
}