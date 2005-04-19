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
[TestFixture] public class DataRow_Item : GHTBase
{
	[Test] public void Main()
	{
		DataRow_Item tc = new DataRow_Item();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataRow_Item");
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
		// init table with columns
		DataTable myTable = new DataTable("myTable"); 
		
		myTable.Columns.Add(new DataColumn("Id",typeof(int)));
		myTable.Columns.Add(new DataColumn("Name",typeof(string)));
		DataColumn dc = myTable.Columns[0];
		
		myTable.Rows.Add(new object[] {1,"Ofer"});
		myTable.Rows.Add(new object[] {2,"Ofer"});
		
		myTable.AcceptChanges();
		
		DataRow myRow = myTable.Rows[0];
			

		//Start checking
        
		try
		{
			BeginCase("Item - index");
			Compare((int)myRow[0] , 1 );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		try
		{
			BeginCase("Item - string");
			Compare( (int)myRow["Id"] , 1 );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
		
		try
		{
			BeginCase("Item - Column");
			Compare( (int)myRow[dc] , 1 );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
	
		try
		{
			BeginCase("Item - index,Current");
			Compare( (int)myRow[0,DataRowVersion.Current ] , 1 );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
	
		try
		{
			BeginCase("Item - string,Current");
			Compare( (int)myRow["Id",DataRowVersion.Current] , 1 );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
	
		try
		{
			BeginCase("Item - columnn,Current");
			Compare( (int)myRow[dc,DataRowVersion.Current] , 1 );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

	//	testMore();
	
	}

	/*public void testMore()
	{
		try
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Rows[0].BeginEdit();
			dt.Rows[0][0] = 10;
			dt.Rows[0].EndEdit();
			dt.AcceptChanges();
			
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}*/
}
}