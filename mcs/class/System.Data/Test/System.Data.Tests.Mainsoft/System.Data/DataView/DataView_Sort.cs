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
[TestFixture] public class DataView_Sort : GHTBase
{
	[Test] public void Main()
	{
		DataView_Sort tc = new DataView_Sort();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataView_Sort");
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
		DataRow[] drArrTable;

		//create the source datatable
		DataTable dt = GHTUtils.DataProvider.CreateChildDataTable();

		//create the dataview for the table
		DataView dv = new DataView(dt);


		dv.Sort = "ParentId";
		drArrTable = dt.Select("","ParentId");
		try
		{
			BeginCase("sort = ParentId");
			Compare(CompareSortedRows(dv,drArrTable),true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		dv.Sort = "ChildId";
		drArrTable = dt.Select("","ChildId");
		try
		{
			BeginCase("sort = ChildId");
			Compare(CompareSortedRows(dv,drArrTable),true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		dv.Sort = "ParentId Desc, ChildId";
		drArrTable = dt.Select("","ParentId Desc, ChildId");
		try
		{
			BeginCase("sort = ParentId Desc, ChildId");
			Compare(CompareSortedRows(dv,drArrTable),true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		dv.Sort = "ChildId Asc, ParentId";
		drArrTable = dt.Select("","ChildId Asc, ParentId");
		try
		{
			BeginCase("sort = ChildId Asc, ParentId");
			Compare(CompareSortedRows(dv,drArrTable),true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		dv.Sort = "ChildId Asc, ChildId Desc";
		drArrTable = dt.Select("","ChildId Asc, ChildId Desc");
		try
		{
			BeginCase("sort = ChildId Asc, ChildId Desc");
			Compare(CompareSortedRows(dv,drArrTable),true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("IndexOutOfRangeException - 1");
			try
			{
				dv.Sort = "something";
			}
			catch (IndexOutOfRangeException ex)
			{
				exp=ex;
			}
			Compare(exp.GetType().FullName,typeof(IndexOutOfRangeException).FullName); 
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("IndexOutOfRangeException - 2");
			try
			{
				dv.Sort = "ColumnId Desc Asc";
			}
			catch (IndexOutOfRangeException ex)
			{
				exp=ex;
			}
			Compare(exp.GetType().FullName,typeof(IndexOutOfRangeException).FullName);
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			BeginCase("IndexOutOfRangeException - 3");
			try
			{
				dv.Sort = "ColumnId blabla";
			}
			catch (IndexOutOfRangeException ex)
			{
				exp=ex;
			}
			Compare(exp.GetType().FullName,typeof(IndexOutOfRangeException).FullName); 
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

	}
	private bool CompareSortedRows(DataView dv, DataRow[] drTable)
	{
		if (dv.Count != drTable.Length) throw new Exception("DataRows[] length are different");

		//comparing the rows by using columns ParentId and ChildId
		for (int i=0; i<dv.Count ; i++)
		{
			if (	dv[i].Row["ParentId"].ToString() != drTable[i]["ParentId"].ToString() 
				&& 
				dv[i].Row["ChildId"].ToString() != drTable[i]["ChildId"].ToString())
				return false;
		}
		return true;
	}
}
}