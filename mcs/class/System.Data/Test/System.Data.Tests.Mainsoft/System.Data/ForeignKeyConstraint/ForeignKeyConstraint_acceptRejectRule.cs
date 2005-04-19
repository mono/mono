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
[TestFixture]
public class ForeignKeyConstraint_acceptRejectRule : GHTBase
{
	public static void Main()
	{
		ForeignKeyConstraint_acceptRejectRule tc = new ForeignKeyConstraint_acceptRejectRule();
		Exception exp = null;
		try
		{
			tc.BeginTest("ForeignKeyConstraint_acceptRejectRule");
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
			BeginCase("ForeignKeyConstraint_acceptRejectRule");
			ForeignKeyConstraint_acceptRejectRule1();
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
	public void ForeignKeyConstraint_acceptRejectRule1()
	{
		DataSet ds = getNewDataSet();

		ForeignKeyConstraint fc = new ForeignKeyConstraint(ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]);
		fc.AcceptRejectRule= AcceptRejectRule.Cascade;
		ds.Tables[1].Constraints.Add(fc);

		//Update the parent 

		ds.Tables[0].Rows[0]["ParentId"] = 777;
		Compare(ds.Tables[1].Select("ParentId=777").Length > 0 ,true);
		ds.Tables[0].RejectChanges();
		Compare(ds.Tables[1].Select("ParentId=777").Length , 0);
		
	}
	private DataSet getNewDataSet()
	{
		DataSet ds1 = new DataSet();
		ds1.Tables.Add(DataProvider.CreateParentDataTable());
		ds1.Tables.Add(DataProvider.CreateChildDataTable());
	//	ds1.Tables.Add(DataProvider.CreateChildDataTable());
		ds1.Tables[0].PrimaryKey=  new DataColumn[] {ds1.Tables[0].Columns[0]};

		return ds1;
	}

}
}