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
[TestFixture] public class DataSet_CaseSensitive : GHTBase
{
	[Test] public void Main()
	{
		DataSet_CaseSensitive tc = new DataSet_CaseSensitive();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_CaseSensitive");
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
		DataTable dt = new DataTable();

		try
		{
			BeginCase("CaseSensitive - default value (false)");
			Compare(ds.CaseSensitive  ,false );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
        
		ds.CaseSensitive = true;

		try
		{
			BeginCase("CaseSensitive - get");
			Compare(ds.CaseSensitive  ,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//add a datatable to a dataset
		ds.Tables.Add(dt);
		try
		{
			BeginCase("DataTable CaseSensitive from DataSet - true");
			Compare(dt.CaseSensitive  ,true );
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		ds.Tables.Clear();
		ds.CaseSensitive = false;
		dt = new DataTable();
		ds.Tables.Add(dt);

		try
		{
			BeginCase("DataTable CaseSensitive from DataSet - false");
			Compare(dt.CaseSensitive ,false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


		//change DataSet CaseSensitive and check DataTables in it
		ds.Tables.Clear();
		ds.CaseSensitive = false;
		dt = new DataTable();
		ds.Tables.Add(dt);

		try
		{
			BeginCase("Change DataSet CaseSensitive - check Table - true");
			ds.CaseSensitive = true;
			Compare(dt.CaseSensitive ,true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Change DataSet CaseSensitive - check Table - false");
			ds.CaseSensitive = false;
			Compare(dt.CaseSensitive ,false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		
		//Add new table to DataSet with CaseSensitive,check the table case after adding it to DataSet
		ds.Tables.Clear();
		ds.CaseSensitive = true;
		dt = new DataTable();
		dt.CaseSensitive = false;
		ds.Tables.Add(dt);

		try
		{
			BeginCase("DataTable get case sensitive from DataSet - false");
			Compare(dt.CaseSensitive ,false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		ds.Tables.Clear();
		ds.CaseSensitive = false;
		dt = new DataTable();
		dt.CaseSensitive = true;
		ds.Tables.Add(dt);

		try
		{
			BeginCase("DataTable get case sensitive from DataSet - true");
			Compare(dt.CaseSensitive ,true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//Add new table to DataSet and change the DataTable CaseSensitive 
		ds.Tables.Clear();
		ds.CaseSensitive = true;
		dt = new DataTable();
		ds.Tables.Add(dt);

		try
		{
			BeginCase("Add new table to DataSet and change the DataTable CaseSensitive - false");
			dt.CaseSensitive = false;
			Compare(dt.CaseSensitive ,false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		ds.Tables.Clear();
		ds.CaseSensitive = false;
		dt = new DataTable();
		ds.Tables.Add(dt);

		try
		{
			BeginCase("Add new table to DataSet and change the DataTable CaseSensitive - true");
			dt.CaseSensitive = true;
			Compare(dt.CaseSensitive ,true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//Add DataTable to Dataset, Change DataSet CaseSensitive, check DataTable
		ds.Tables.Clear();
		ds.CaseSensitive = true;
		dt = new DataTable();
		dt.CaseSensitive = true;
		ds.Tables.Add(dt);

		try
		{
			BeginCase("Add DataTable to Dataset, Change DataSet CaseSensitive, check DataTable - true");
			ds.CaseSensitive = false;
			Compare(dt.CaseSensitive ,true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}




	}
}
}