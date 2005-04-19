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

#region old
/*
using System;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.ComponentModel;

using GHTUtils;
using GHTUtils.Base;

using NUnit.Framework;

namespace tests.system_data_dll.System_Data
{
[TestFixture] public class DataRelationCollection_Add_SDD : GHTBase
{
	OleDbConnection con;

	public void SetUp()
	{
		con = new OleDbConnection(GHTUtils.DataProvider.ConnectionString);
		con.Open();
	}

	public void TearDown()
	{
		if (con != null)
		{
			if (con.State == ConnectionState.Open) con.Close();
		}
	}

	[Test] public void Main()
	{
		DataRelationCollection_Add_SDD tc = new DataRelationCollection_Add_SDD();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataRelationCollection_Add_SDD");
			tc.SetUp();
			tc.run();
			tc.TearDown();
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
			BeginCase("DataRelation.Add(name,DataColumn parentColumn,DataColumn childColumn");
			DataSet contactsDataSet1 = new DataSet();
			string strbuff = null;

			OleDbCommand oleDbSelectCommand1 = new OleDbCommand();
			OleDbDataAdapter cmdCustomers= new OleDbDataAdapter("Select CustomerID,CompanyName,ContactName,ContactTitle,Phone,Address from Customers where CustomerID like 'A%'", con);
			OleDbDataAdapter cmdOrders = new OleDbDataAdapter("Select CustomerID,OrderID,OrderDate,ShippedDate,ShipAddress,Freight from Orders where CustomerID like 'A%'", con);
			cmdCustomers.Fill(contactsDataSet1, "Customers");
			cmdOrders.Fill(contactsDataSet1, "Orders");

			contactsDataSet1.Relations.Add("CustomerID",
					contactsDataSet1.Tables["Customers"].Columns["CustomerID"],
					contactsDataSet1.Tables["Orders"].Columns["CustomerID"]);

			//' Iterate through the master and child tables
			foreach (DataRow customerRow in contactsDataSet1.Tables["Customers"].Rows)
			{
				foreach (DataRow orderRow in customerRow.GetChildRows("CustomerID"))
				{
					object item = orderRow.ItemArray.GetValue(0);
					strbuff = strbuff + item.ToString().Substring(1,1);
				}
			}

			Compare(strbuff,"LLLLLLNNNNNNNNNNNRRRRRRRRRRRRR");
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
}
}
*/
#endregion

using System;
using System.Data;

using GHTUtils;
using GHTUtils.Base;

namespace tests.system_data_dll.System_Data
{
	[TestFixture] public class DataRelationCollection_Add_SDD : GHTBase
	{
		[Test] public void Main()
		{
			DataRelationCollection_Add_SDD tc = new DataRelationCollection_Add_SDD();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataRelationCollection_Add_DD");
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

		/// <summary>
		/// All the test are in the overload Add(dataRelation)
		/// </summary>
		public void run()
		{
			Exception exp = null;
			try
			{
				BeginCase("DataRelationCollection_Add_SDD");
				DataRelationCollection_Add_SDD1(); 
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

		private void DataRelationCollection_Add_SDD1()
		{
			DataSet ds = getDataSet();
			ds.Relations.Add("rel1",ds.Tables[0].Columns["ParentId"],ds.Tables[1].Columns["ParentId"]);

			Compare(ds.Relations.Count,1);
		
			Compare(ds.Tables[0].ChildRelations.Count,1); //When adding a relation,it's also added on the tables
			Compare(ds.Tables[1].ParentRelations.Count,1);

			Compare(ds.Tables[0].Constraints[0].GetType(),typeof(UniqueConstraint));
			Compare(ds.Tables[1].Constraints[0].GetType(),typeof(ForeignKeyConstraint)); 

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
