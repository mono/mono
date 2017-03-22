// 
// Copyright (c) 2006 Mainsoft Co.
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
using System.Text;
using System.Data;
using System.Data.OleDb;

using MonoTests.System.Data.Utils;


using NUnit.Framework;
namespace MonoTests.System.Data.OleDb
{
[TestFixture]
public class OleDbDataAdapter_FillSchema_DsSt : ADONetTesterClass
{
	private Exception exp;
	private string connectionString = ConnectedDataProvider.ConnectionString;

	public static void Main()
	{
		OleDbDataAdapter_FillSchema_DsSt tc = new OleDbDataAdapter_FillSchema_DsSt();
		Exception l_exp = null;
		try
		{
			// Every Test must begin with BeginTest
			tc.BeginTest("OleDbDataAdapter_FillSchema_DsSt");
			tc.run();
		}
		catch(Exception ex)
		{
			l_exp = ex;
		}
		finally
		{
			// Every Test must End with EndTest
			tc.EndTest(l_exp);
		}
	}

	public void run()
	{
		TestLongSqlExpression();
	}

	//Test case for bug #4708
	[Test(Description="Test case for bug #4708")]
	public void TestLongSqlExpression()
	{
		BeginCase("Long SQL string cause java.lang.StackOverflowError (Test case for bug #4708)");

		StringBuilder querySb = new StringBuilder();
		querySb.Append("SELECT ");
		querySb.Append("c.CustomerID as ci1, c.CustomerID as ci2, c.CustomerID as ci3, c.CustomerID as ci4, ");
		querySb.Append("c.CompanyName as cn1, c.CompanyName as cn2, c.CompanyName as cn3, c.CompanyName as cn4, ");
		querySb.Append("c.ContactName as cntn1, c.ContactName as cntn2, c.ContactName as cntn3, c.ContactName as cntn4, ");
		querySb.Append("c.ContactTitle as ct1, c.ContactTitle as ct2, c.ContactTitle as ct3, c.ContactTitle as ct4, ");
		querySb.Append("c.Address as ad1, c.Address as ad2, c.Address as ad3, c.Address as ad4, ");
		querySb.Append("c.City as ct1, c.City as ct2, c.City as ct3, c.City as ct4, ");
		querySb.Append("c.Region as rg1, c.Region as rg2, c.Region as rg3, c.Region as rg4, ");
		querySb.Append("c.PostalCode as pc1, c.PostalCode as pc2, c.PostalCode as pc3, c.PostalCode as pc4, ");
		querySb.Append("c.Country as co1, c.Country as co2, c.Country as co3, c.Country as co4, ");
		querySb.Append("c.Phone as ph1, c.Phone as ph2, c.Phone as ph3, c.Phone as ph4, ");
		querySb.Append("c.Fax as fx1, c.Fax as fx2, c.Fax as fx3, c.Fax as fx4 ");
		querySb.Append("FROM Customers c");
		OleDbDataAdapter adapter = null;
		DataSet schemaDs = new DataSet();
		try
		{
			using(adapter = new OleDbDataAdapter(querySb.ToString(), this.connectionString))
			{
				adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
				adapter.FillSchema(schemaDs, SchemaType.Source);
				Compare(schemaDs.Tables.Count, 1);
			}
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