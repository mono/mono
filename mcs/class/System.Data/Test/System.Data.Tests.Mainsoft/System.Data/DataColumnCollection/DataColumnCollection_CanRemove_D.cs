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
[TestFixture] public class DataColumnCollection_CanRemove_D : GHTBase
{
	[Test] public void Main()
	{
		DataColumnCollection_CanRemove_D tc = new DataColumnCollection_CanRemove_D();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataColumnCollection_CanRemove_D");
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
		try
		{
			BeginCase("DataColumnCollection_CanRemove_D");
			DataColumnCollection_CanRemove_D1();
		} 
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
			exp = null;
		}

		try
		{
			BeginCase("DataColumnCollection_CanRemove_D");
			DataColumnCollection_CanRemove_D2();
		} 
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
			exp = null;
		}

		try
		{
			BeginCase("DataColumnCollection_CanRemove_D");
			DataColumnCollection_CanRemove_D3();
		} 
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
			exp = null;
		}

		try
		{
			BeginCase("DataColumnCollection_CanRemove_D");
			DataColumnCollection_CanRemove_D4();
		} 
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			EndCase(exp);
			exp = null;
		}
	}
	private void DataColumnCollection_CanRemove_D1()
	{
		DataTable dt = GHTUtils.DataProvider.CreateUniqueConstraint();
		DataColumn dummyCol = new DataColumn();
		Compare(dt.Columns.CanRemove(null),false); //Cannot remove null column 
		Compare(dt.Columns.CanRemove(dummyCol),false); //Don't belong to this table
		Compare(dt.Columns.CanRemove(dt.Columns[0]),false); //It belongs to unique constraint
		Compare(dt.Columns.CanRemove(dt.Columns[1]),true);

		
	}
	private void DataColumnCollection_CanRemove_D2()
	{
		DataSet ds = DataProvider.CreateForigenConstraint();

		Compare(ds.Tables["child"].Columns.CanRemove(ds.Tables["child"].Columns["parentId"]),false);//Forigen
		Compare(ds.Tables["parent"].Columns.CanRemove(ds.Tables["child"].Columns["parentId"]),false);//Parent
	}
	private void DataColumnCollection_CanRemove_D3()
	{
		
		DataSet ds = new DataSet();

		ds.Tables.Add("table1"); 
		ds.Tables.Add("table2");
		ds.Tables["table1"].Columns.Add("col1");
		ds.Tables["table2"].Columns.Add("col1");
	
		ds.Tables[1].ParentRelations.Add("name1",ds.Tables[0].Columns["col1"],ds.Tables[1].Columns["col1"],false);

		Compare(ds.Tables[1].Columns.CanRemove(ds.Tables[1].Columns["col1"]),false); //Part of a parent 
		Compare(ds.Tables[0].Columns.CanRemove(ds.Tables[0].Columns["col1"]),false); //Part of a child 

	}

	private void DataColumnCollection_CanRemove_D4()
	{
		DataTable dt = new DataTable();
		dt.Columns.Add("col1",typeof(string));
		dt.Columns.Add("col2",typeof(string),"sum(col1)");
			
		Compare(dt.Columns.CanRemove(dt.Columns["col1"]),false); //Col1 is a part of expression

	}
}
}