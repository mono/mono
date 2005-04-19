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
[TestFixture] public class DataSet_GetChanges_D : GHTBase
{
	[Test] public void Main()
	{
		DataSet_GetChanges_D tc = new DataSet_GetChanges_D();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_GetChanges_D");
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
	
		DataSet ds = new DataSet();
		object[] arrAdded,arrDeleted,arrModified,arrUnchanged;
		//object[] arrDetached;
		
		DataRow dr;
		ds.Tables.Add(GHTUtils.DataProvider.CreateParentDataTable());

		try
		{
			BeginCase("GetChanges 1");
			Compare(ds.GetChanges(),null );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//make some changes

// can't check detached
//		dr = ds.Tables[0].Rows[0];
//		arrDetached = dr.ItemArray;
//		dr.Delete();
//		ds.Tables[0].AcceptChanges();

        dr= ds.Tables[0].Rows[1];
		arrDeleted  = dr.ItemArray;
		dr.Delete();
        
		dr = ds.Tables[0].Rows[2];
		dr[1] = "NewValue";
		arrModified = dr.ItemArray;

		dr = ds.Tables[0].Select("","",DataViewRowState.Unchanged)[0];
		arrUnchanged = dr.ItemArray;

		dr = ds.Tables[0].NewRow();
		dr[0] = 1;
		ds.Tables[0].Rows.Add(dr);
		arrAdded = dr.ItemArray;
	
		try
		{
			BeginCase("GetChanges Added");
			Compare(ds.GetChanges(DataRowState.Added).Tables[0].Rows[0].ItemArray ,arrAdded);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
        
        try
		{
			BeginCase("GetChanges Deleted");
			dr = ds.GetChanges(DataRowState.Deleted).Tables[0].Rows[0];
			object[] tmp = new object[] {dr[0,DataRowVersion.Original],dr[1,DataRowVersion.Original],dr[2,DataRowVersion.Original],dr[3,DataRowVersion.Original],dr[4,DataRowVersion.Original],dr[5,DataRowVersion.Original]};
			Compare(tmp,arrDeleted);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

//	can't check it	
//		try
//		{
//			BeginCase("GetChanges Detached");
//			dr = ds.GetChanges(DataRowState.Detached).Tables[0].Rows[0];
//			object[] tmp = new object[] {dr[0,DataRowVersion.Original],dr[1,DataRowVersion.Original],dr[2,DataRowVersion.Original]};
//			Compare(tmp,arrDetached);
//		}
//		catch(Exception ex)	{exp = ex;}
//		finally	{EndCase(exp); exp = null;}
		
		try
		{
			BeginCase("GetChanges Modified");
			Compare(ds.GetChanges(DataRowState.Modified).Tables[0].Rows[0].ItemArray ,arrModified);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("GetChanges Unchanged");
			Compare(ds.GetChanges(DataRowState.Unchanged).Tables[0].Rows[0].ItemArray ,arrUnchanged);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
	}
}
}