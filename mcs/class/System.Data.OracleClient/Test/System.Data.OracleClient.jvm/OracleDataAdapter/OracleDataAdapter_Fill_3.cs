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
using System.Data;
using System.Data.OracleClient;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleDataAdapter_Fill_3: ADONetTesterClass
	{
		public static void Main()
		{
			OracleDataAdapter_Fill_3 tc = new OracleDataAdapter_Fill_3();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleDataAdapter_Fill_3");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			Exception exp = null;

			//in DB2 when trying to fill an empty table - no table is loaded
			OracleConnection conn = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			OracleDataAdapter oleDBda = new OracleDataAdapter();
			oleDBda.SelectCommand = new OracleCommand("Select * from GH_EMPTYTABLE",conn);
			
			DataSet ds = new DataSet();
			oleDBda.Fill(ds);
        
			try
			{
				BeginCase("Table count - fill with SP");
				Compare(ds.Tables.Count ,1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//add for bug #2508 - OLEDBDataAdapter.Fill fills only the 1st result set, reported from an evaluation
			if (ConnectedDataProvider.GetDbType(oleDBda.SelectCommand.Connection) == DataBaseServer.SQLServer)
				//multiple commands can not be done with Oracle or DB2
			{


				//get excpected results
				if (oleDBda.SelectCommand.Connection.State != ConnectionState.Open)
				{
					oleDBda.SelectCommand.Connection.Open();
				}
				OracleCommand cmd = new OracleCommand("",oleDBda.SelectCommand.Connection);
				cmd.CommandText = "Select count(*) from Customers";
				int TblResult0 = (int)cmd.ExecuteScalar();
				cmd.CommandText = "Select count(*) from Categories";
				int TblResult1 = (int)cmd.ExecuteScalar();
				cmd.CommandText = "Select count(*) from Region";
				int TblResult2 = (int)cmd.ExecuteScalar();
				if (oleDBda.SelectCommand.Connection.State != ConnectionState.Closed)
				{
					oleDBda.SelectCommand.Connection.Close();
				}


				oleDBda.SelectCommand.CommandText = "Select * from Customers; " +
					"Select * from Categories; " +
					"Select * from Region";
				ds = new DataSet();
				oleDBda.Fill(ds);

				try
				{
					BeginCase("Table count - Fill with query");
					Compare(ds.Tables.Count ,3 );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}

				try
				{
					BeginCase("Table 0 rows count");
					Compare(ds.Tables[0].Rows.Count ,TblResult0 );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}

				try
				{
					BeginCase("Table 1 rows count");
					Compare(ds.Tables[1].Rows.Count ,TblResult1 );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}

				try
				{
					BeginCase("Table 2 rows count");
					Compare(ds.Tables[2].Rows.Count ,TblResult2 );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}

			}

 
		}
	}
}