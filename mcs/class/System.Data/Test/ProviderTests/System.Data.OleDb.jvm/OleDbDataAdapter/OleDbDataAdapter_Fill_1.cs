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
using System.Data.OleDb ;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbDataAdapter_Fill_1: ADONetTesterClass 
	{
		// transaction use was add for PostgreSQL
		OleDbTransaction tr;

		OleDbConnection con;
		OleDbCommand cmd;

		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				
				con.Open();
				// transaction use was add for PostgreSQL
				tr = con.BeginTransaction();
				
				cmd = new OleDbCommand("", con, tr);
				// prepare data
				base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				Compare("Setup" ,"Setup");
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		[TearDown]
		public void TearDown()
		{
			// transaction use was add for PostgreSQL
			tr.Commit();
			if (con != null)
			{
				if (con.State == ConnectionState.Open) con.Close();
			}
		}

		public static void Main()
		{
			OleDbDataAdapter_Fill_1 tc = new OleDbDataAdapter_Fill_1();
			Exception exp = null;
			try
			{
				// Every Test must begin with BeginTest
				tc.BeginTest("OleDbDataAdapter_Fill_1");
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
				// Every Test must End with EndTest
				tc.EndTest(exp);
			}
		}

		[Test]
		public void run()
		{
			Exception exp = null;
#if !JAVA
			if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.Oracle)
			{
				StringBuilder messageBuilder = new StringBuilder();
				messageBuilder.Append("Test \"OleDbDataAdapter_Fill_1\" Skipped when running in .NET against Oracle database:\n");
				messageBuilder.Append("In .NET there is a bug when calling a SP with multiple REFCURSORS from oracle server, the workaround is to use OracleClient and not OleDb.\n");
				messageBuilder.Append("In GH we are not bug complient in this issue, because there is no workaround - We do not support the OracleClient namespace.");
				messageBuilder.Append(" (The java run is not skipped).");
				Log(messageBuilder.ToString());
				return;
			}

			if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.PostgreSQL)
			{
				// fail to work on .NET OLEDB
				this.Log("Not testing PostgreSQL CommandType.StoredProcedure which return SETOF");
				return;
			}
#endif

			cmd.CommandText = "GH_MULTIRECORDSETS";
			cmd.CommandType = CommandType.StoredProcedure;
			OleDbDataAdapter da = new OleDbDataAdapter(cmd);
			DataSet ds = new DataSet();

			//execute the fill command
			da.Fill(ds);

			try
			{
				BeginCase("Check table count");
				Compare(ds.Tables.Count ,3);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check table 0 rows count");
				Compare(ds.Tables[0].Rows.Count ,2);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check table 0 Columns count");
				Compare(ds.Tables[0].Columns.Count ,2);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check table 1 rows count");
				Compare(ds.Tables[1].Rows.Count ,2);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check table 1 Columns count");
				Compare(ds.Tables[1].Columns.Count ,3);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check table 2 rows count");
				Compare(ds.Tables[2].Rows.Count ,0);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Check table 2 Columns count");
				Compare(ds.Tables[2].Columns.Count ,4);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

		}
	}
}