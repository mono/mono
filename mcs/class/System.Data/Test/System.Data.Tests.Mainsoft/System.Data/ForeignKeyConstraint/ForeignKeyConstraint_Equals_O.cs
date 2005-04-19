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
[TestFixture] public class ForeignKeyConstraint_Equals_O : GHTBase
{
	[Test] public void Main()
	{
		ForeignKeyConstraint_Equals_O tc = new ForeignKeyConstraint_Equals_O();
		Exception exp = null;
		try
		{
			tc.BeginTest("ForeignKeyConstraint_Equals_O");
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
		DataTable dtParent = GHTUtils.DataProvider.CreateParentDataTable();
		DataTable dtChild = GHTUtils.DataProvider.CreateChildDataTable();
		ds.Tables.Add(dtParent);
		ds.Tables.Add(dtChild);
		dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns[0]};
		ds.EnforceConstraints = true;

		ForeignKeyConstraint fc1,fc2;
		fc1 = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

		fc2 = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[1]);
		try
		{
			BeginCase("different columnn");
			Compare(fc1.Equals(fc2),false);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		//Two System.Data.ForeignKeyConstraint are equal if they constrain the same columns.
		try
		{
			BeginCase("same column");
			fc2 = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);
			Compare(fc1.Equals(fc2),true);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}
	}
}
}