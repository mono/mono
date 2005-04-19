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
[TestFixture] public class DataRow_GetColumnsInError : GHTBase
{
	[Test] public void Main()
	{
		DataRow_GetColumnsInError tc = new DataRow_GetColumnsInError();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataRow_GetColumnsInError");
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

		string sColErr = "Error!";
		DataColumn[] dcArr;
		DataTable dt = new DataTable("myTable"); 
		//init some columns
		dt.Columns.Add(new DataColumn());
		dt.Columns.Add(new DataColumn());
		dt.Columns.Add(new DataColumn());
		dt.Columns.Add(new DataColumn());
		dt.Columns.Add(new DataColumn());

		//init some rows
		dt.Rows.Add(new object[] {});
		dt.Rows.Add(new object[] {});
		dt.Rows.Add(new object[] {});

		DataRow dr = dt.Rows[1];

		dcArr = dr.GetColumnsInError();
        
		try
		{
			BeginCase("GetColumnsInError 1");
			Compare( dcArr.Length , 0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		

		dr.SetColumnError(0,sColErr);
		dr.SetColumnError(2,sColErr);
		dr.SetColumnError(4,sColErr);
		
		dcArr = dr.GetColumnsInError();
        
		try
		{
			BeginCase("GetColumnsInError 2");
			Compare(dcArr.Length , 3);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		

		//check that the right columns taken
		try
		{
			BeginCase("GetColumnsInError 3");
			Compare(dcArr[0],dt.Columns[0]);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("GetColumnsInError 4");
			Compare(dcArr[1],dt.Columns[2]);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("GetColumnsInError 5");
			Compare(dcArr[2],dt.Columns[4]);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

	}
}
}