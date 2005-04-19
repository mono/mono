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
[TestFixture] public class DataSet_Merge_Dr : GHTBase
{
	[Test] public void Main()
	{
		DataSet_Merge_Dr tc = new DataSet_Merge_Dr();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_Merge_Dr");
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
	
		DataTable dt = GHTUtils.DataProvider.CreateParentDataTable();
		dt.TableName = "Table1";
		dt.PrimaryKey = new DataColumn[] {dt.Columns[0]};

		//create target dataset (copy of source dataset)
		DataSet dsTarget = new DataSet();
		dsTarget.Tables.Add(dt.Copy());

		DataRow[] drArr = new DataRow[3];
		//Update row
		string OldValue = dt.Select("ParentId=1")[0][1].ToString();
		drArr[0] = dt.Select("ParentId=1")[0];
		drArr[0][1]	= "NewValue";
		//delete rows
		drArr[1] = dt.Select("ParentId=2")[0];
		drArr[1].Delete();
		//add row
		drArr[2] = dt.NewRow();
		drArr[2].ItemArray = new object[] {99 ,"NewRowValue1", "NewRowValue2"};
		dt.Rows.Add(drArr[2]);

		dsTarget.Merge(drArr,false,MissingSchemaAction.Ignore );

		try
		{
			BeginCase("Merge update row");
			Compare(dsTarget.Tables["Table1"].Select("ParentId=1")[0][1] , "NewValue");
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
        
		try
		{
			BeginCase("Merge added row");
			Compare(dsTarget.Tables["Table1"].Select("ParentId=99").Length ,1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Merge deleted row");
			Compare(dsTarget.Tables["Table1"].Select("ParentId=2").Length , 0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		
	}
}
}