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

using System;
using System.Data;
using NUnit.Framework;
using GHTUtils;
using GHTUtils.Base;

namespace tests.system_data_dll.System_Data
{
[TestFixture] public class DataTableCollection_CanRemove_D : GHTBase
{
	public static void Main()
	{
		DataTableCollection_CanRemove_D tc = new DataTableCollection_CanRemove_D();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataTableCollection_CanRemove_D");
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
			BeginCase("DataTableCollection_CanRemove_D");
			DataTableCollection_CanRemove_D1();
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
			BeginCase("DataTableCollection_CanRemove_D");
			DataTableCollection_CanRemove_D2();
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
			BeginCase("DataTableCollection_CanRemove_D");
			DataTableCollection_CanRemove_D3();
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
			BeginCase("DataTableCollection_CanRemove_D");
			DataTableCollection_CanRemove_D4();
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
			BeginCase("DataTableCollection_CanRemove_D");
			DataTableCollection_CanRemove_D5();
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
	[Test]
	public void DataTableCollection_CanRemove_D1()
	{
		DataSet ds = new DataSet();
		ds.Tables.Add();
		Compare(ds.Tables.CanRemove(ds.Tables[0]),true); 
	}

	[Test]
	public void DataTableCollection_CanRemove_D2()
	{
		DataSet ds = new DataSet();
		Compare(ds.Tables.CanRemove(null),false);
	}

	[Test]
	public void DataTableCollection_CanRemove_D3()
	{
		DataSet ds = new DataSet();
		DataSet ds1 = new DataSet();
		ds1.Tables.Add();
		Compare(ds.Tables.CanRemove(ds1.Tables[0]),false);
	}

	[Test]
	public void DataTableCollection_CanRemove_D4()
	{
		DataSet ds = new DataSet();
		ds.Tables.Add(DataProvider.CreateParentDataTable());
		ds.Tables.Add(DataProvider.CreateChildDataTable());

		ds.Relations.Add("rel",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"],false);

		Compare(ds.Tables.CanRemove(ds.Tables[0]),false);
		Compare(ds.Tables.CanRemove(ds.Tables[1]),false);
	}
	[Test]
	public void DataTableCollection_CanRemove_D5()
	{
		DataSet ds = DataProvider.CreateForigenConstraint();
		Compare(ds.Tables.CanRemove(ds.Tables[0]),false); //Unique 
		Compare(ds.Tables.CanRemove(ds.Tables[1]),false); //Foreign 

	}

}
}
