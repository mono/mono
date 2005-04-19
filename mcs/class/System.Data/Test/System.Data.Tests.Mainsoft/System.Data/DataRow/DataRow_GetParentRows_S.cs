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
[TestFixture] public class DataRow_GetParentRows_S : GHTBase
{
	[Test] public void Main()
	{
		DataRow_GetParentRows_S tc = new DataRow_GetParentRows_S();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataRow_GetParentRows_S");
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

		
		DataRow dr;
		DataRow[] drArrExcepted,drArrResult;
		DataTable dtChild,dtParent;
		DataSet ds = new DataSet();

		//Create tables
		dtChild = GHTUtils.DataProvider.CreateChildDataTable();
		dtParent = GHTUtils.DataProvider.CreateParentDataTable(); 

		//Add tables to dataset
		ds.Tables.Add(dtChild);
		ds.Tables.Add(dtParent);
		dr = dtParent.Rows[0];
				
		//Duplicate several rows in order to create Many to Many relation
		dtParent.ImportRow(dr); 
		dtParent.ImportRow(dr); 
		dtParent.ImportRow(dr); 

		//Add Relation
		DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"],false);
		ds.Relations.Add(dRel);
		//Get Excepted result
		drArrExcepted = dtParent.Select("ParentId=" + dr["ParentId"]);
		dr = dtChild.Select("ParentId=" + dr["ParentId"])[0];
		//Get Result
		drArrResult = dr.GetParentRows("Parent-Child");
		
		try
		{
			BeginCase("GetParentRows_S");
			base.Compare( drArrResult, drArrExcepted);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
	}
}
}