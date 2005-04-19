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
[TestFixture] public class DataRelationCollection_Add_SDDB : GHTBase
{
	[Test] public void Main()
	{
		DataRelationCollection_Add_SDDB tc = new DataRelationCollection_Add_SDDB();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataRelationCollection_Add_SDDB");
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
			BeginCase("DataRelationCollection_Add_SDDB");
			DataRelationCollection_Add_SDDB1();
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
			BeginCase("DataRelationCollection_Add_SDDB");
			DataRelationCollection_Add_SDDB2();
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
	private void DataRelationCollection_Add_SDDB1()
	{
		DataSet ds = getDataSet();
		ds.Relations.Add("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"],true);

		Compare(ds.Relations.Count,1);
		
		Compare(ds.Tables[0].ChildRelations.Count,1); //When adding a relation,it's also added on the tables
		Compare(ds.Tables[1].ParentRelations.Count,1);

		Compare(ds.Tables[0].Constraints[0].GetType(),typeof(UniqueConstraint));
		Compare(ds.Tables[1].Constraints[0].GetType(),typeof(ForeignKeyConstraint)); 

		Compare(ds.Relations[0].RelationName,"rel1");
		Compare(ds.Tables[0].ChildRelations[0].RelationName,"rel1");
		Compare(ds.Tables[1].ParentRelations[0].RelationName,"rel1");
		
	}
	private void DataRelationCollection_Add_SDDB2()
	{
		DataSet ds = getDataSet();
		ds.Relations.Add("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"],false);

		Compare(ds.Relations.Count,1);
		
		Compare(ds.Tables[0].ChildRelations.Count,1); //When adding a relation,it's also added on the tables
		Compare(ds.Tables[1].ParentRelations.Count,1);

		Compare(ds.Tables[0].Constraints.Count,0);
		Compare(ds.Tables[1].Constraints.Count,0); 

		Compare(ds.Relations[0].RelationName,"rel1");
		Compare(ds.Tables[0].ChildRelations[0].RelationName,"rel1");
		Compare(ds.Tables[1].ParentRelations[0].RelationName,"rel1");

	}

	private DataSet getDataSet()
	{
		DataSet ds = new DataSet();
		DataTable dt1 = DataProvider.CreateParentDataTable();
		DataTable dt2 = DataProvider.CreateChildDataTable();

		ds.Tables.Add(dt1);
		ds.Tables.Add(dt2);
		return ds;
	}

}
}