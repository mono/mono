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
[TestFixture] public class DataSet_Merge_DsB : GHTBase
{
	[Test] public void Main()
	{
		DataSet_Merge_DsB tc = new DataSet_Merge_DsB();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_Merge_DsB");
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
	
		//create source dataset
		DataSet ds = new DataSet();
		DataTable dt = GHTUtils.DataProvider.CreateParentDataTable();
		dt.TableName = "Table1";
		ds.Tables.Add(dt.Copy());
		dt.TableName = "Table2";
		//add primary key
		dt.PrimaryKey = new DataColumn[] {dt.Columns[0]};
		ds.Tables.Add(dt.Copy());
				
		//create target dataset (copy of source dataset)
		DataSet dsTarget1 = ds.Copy();
		DataSet dsTarget2 = ds.Copy();
		int iTable1RowsCount = dsTarget1.Tables["Table1"].Rows.Count;


		//update existing row
		string oldValue = ds.Tables["Table2"].Select("ParentId=1")[0][1].ToString();
		ds.Tables["Table2"].Select("ParentId=1")[0][1] = "NewValue";
		//add new row
		object[] arrAddedRow = new object[] {99,"NewValue1","NewValue2",new DateTime(0),0.5,true};
		ds.Tables["Table2"].Rows.Add(arrAddedRow);
		//delete existing rows
		int iDeleteLength = dsTarget1.Tables["Table2"].Select("ParentId=2").Length;
		foreach (DataRow dr in ds.Tables["Table2"].Select("ParentId=2"))
		{
			dr.Delete();
		}

		#region "Merge(ds,true)"
		//only new added rows are merged (preserveChanges = true)
		dsTarget1.Merge(ds,true);
		try
		{
			BeginCase("Merge - changed values");
			Compare(dsTarget1.Tables["Table2"].Select("ParentId=1")[0][1] , oldValue);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
        
		try
		{
			BeginCase("Merge - added values");
			Compare(dsTarget1.Tables["Table2"].Select("ParentId=99")[0].ItemArray  , arrAddedRow);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge - deleted row");
			Compare(dsTarget1.Tables["Table2"].Select("ParentId=2").Length ,iDeleteLength);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		#endregion

		#region "Merge(ds,false)"
		//all changes are merged (preserveChanges = false)
		dsTarget2.Merge(ds,false);
		try
		{
			BeginCase("Merge - changed values");
			Compare(dsTarget2.Tables["Table2"].Select("ParentId=1")[0][1] , "NewValue");
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
        
		try
		{
			BeginCase("Merge - added values");
			Compare(dsTarget2.Tables["Table2"].Select("ParentId=99")[0].ItemArray  , arrAddedRow);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge - deleted row");
			Compare(dsTarget2.Tables["Table2"].Select("ParentId=2").Length ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		#endregion
	}
}
}