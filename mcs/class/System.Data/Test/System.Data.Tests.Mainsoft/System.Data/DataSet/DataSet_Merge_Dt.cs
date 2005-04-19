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
[TestFixture] public class DataSet_Merge_Dt : GHTBase
{
	[Test] public void Main()
	{
		DataSet_Merge_Dt tc = new DataSet_Merge_Dt();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_Merge_Dt");
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
		//create datatable
		DataTable dt = GHTUtils.DataProvider.CreateParentDataTable();
		dt.TableName = "Table1";
		//add a copy of the datatable to the dataset
		ds.Tables.Add(dt.Copy());

		dt.TableName = "Table2";
		//add primary key
		dt.PrimaryKey = new DataColumn[] {dt.Columns[0]};
		ds.Tables.Add(dt.Copy());
		//now the dataset hase two tables
		
				
		//create target dataset (copy of source dataset)
		DataSet dsTarget = ds.Copy();

		dt = ds.Tables["Table2"];
		//update existing row
		dt.Select("ParentId=1")[0][1] = "OldValue1";
		//add new row
		object[] arrAddedRow = new object[] {99,"NewValue1","NewValue2",new DateTime(0),0.5,true};
		dt.Rows.Add(arrAddedRow);
		//delete existing rows
		foreach (DataRow dr in dt.Select("ParentId=2"))
		{
			dr.Delete();
		}

		try
		{
			BeginCase("Merge - changed values");
			dsTarget.Merge(dt);
			Compare(dsTarget.Tables["Table2"].Select("ParentId=1")[0][1] , "OldValue1");
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
        
		try
		{
			BeginCase("Merge - added values");
			Compare(dsTarget.Tables["Table2"].Select("ParentId=99")[0].ItemArray  , arrAddedRow);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge - deleted row");
			Compare(dsTarget.Tables["Table2"].Select("ParentId=2").Length ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


		//test case added due to a reported bug from infogate
		//when merging a DataTable with TableName=null, GH throw null reference exception.
		ds = new DataSet();
		dt = new DataTable();
		dt.Columns.Add("Col1");
		dt.Rows.Add(new object[] {1});

		try
		{
			BeginCase("Merge - add a table with no name");
			ds.Merge(dt);
			Compare(ds.Tables.Count,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


		try
		{
			BeginCase("Merge - add a table with no name - check Rows.Count");
			Compare(ds.Tables[0].Rows.Count ,dt.Rows.Count );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		
	
	}
}
}