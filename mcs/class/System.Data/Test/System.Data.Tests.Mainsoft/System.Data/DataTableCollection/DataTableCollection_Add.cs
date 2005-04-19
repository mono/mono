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
class DataTableCollection_Add : GHTBase
{
	public static void Main()
	{
		DataTableCollection_Add tc = new DataTableCollection_Add();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataTableCollection_Add");
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

	
	public void run()
	{
		Exception exp = null;
		try
		{
			BeginCase("DataTableCollection_Add");
			DataTableCollection_Add1();
			/*BeginCase("Adding computed column to a data set");
			DataSet ds = new DataSet();
			ds.Tables.Add(new DataTable("Table"));
			ds.Tables[0].Columns.Add(new DataColumn("EmployeeNo", typeof(string)));
			ds.Tables[0].Rows.Add(new object[] {"Maciek"});
			ds.Tables[0].Columns.Add("ComputedColumn", typeof(object), "EmployeeNo");

			Compare(ds.Tables[0].Columns["ComputedColumn"].Expression, "EmployeeNo");*/
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
	public void DataTableCollection_Add1()
	{
		DataSet ds = new DataSet();
		ds.Tables.Add();
		Compare(ds.Tables[0].TableName ,"Table1");
		//Assert.AreEqual(ds.Tables[0].TableName,"Table1");
		ds.Tables.Add();
		Compare(ds.Tables[1].TableName ,"Table2");
		//Assert.AreEqual(ds.Tables[1].TableName,"Table2");



	}
}
}