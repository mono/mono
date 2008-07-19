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
using System.Data.OracleClient ;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleDataReader_NextResult : ADONetTesterClass 
	{
		OracleConnection con;
		Exception exp = null;

		[SetUp]
		public void SetUp() {
			base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			con = new OracleConnection (MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			con.Open();
		}

		[TearDown]
		public void TearDown() {
			if (con.State == ConnectionState.Open) con.Close();
		}

		public static void Main()
		{
			OracleDataReader_NextResult tc = new OracleDataReader_NextResult();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleDataReader_NextResult");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		public void run()
		{


			base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			con.Open();

			TestMultipleResultSetsWithSP();
			TestMultipleResultSetsWithSQLText();

			if (con.State == ConnectionState.Open) con.Close();

		}

		[Test]
#if !TARGET_JVM
		[Ignore ("JVM test")]
#endif
		public void TestMultipleResultSetsWithSQLText()
		{

			if (ConnectedDataProvider.GetDbType() == DataBaseServer.Oracle)
			{
				this.Log("Multiple result sets by sql text is not tested in oracle.");
				return;
			}

			if (ConnectedDataProvider.GetDbType() == DataBaseServer.DB2)
			{
				this.Log("Multiple result sets using compound statement not supported at DB2.");
				return;
			}

			bool NextResultExists = false;
			OracleDataReader rdr = null;
			OracleCommand cmd;
			int TblResult0=-1;
			int TblResult1=-1;
			int TblResult2=-1;
			try
			{
				BeginCase("Setup: Get expected results.");

				//get excpected results
				GetExcpectedResults(ref TblResult0, ref TblResult1, ref TblResult2);
				this.Pass("Setup: Get expected results ended.");
			}
			catch (Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp);
			}


			string cmdTxt = BuildCommandText();
			cmd = new OracleCommand(cmdTxt, con);
			cmd.CommandType = CommandType.Text;
			rdr = cmd.ExecuteReader();
			// -------------- ResultSet  1 ------------
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check if ResultSet 1 exists");
				Compare(rdr != null, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check if ResultSet 1 contains data");
				NextResultExists = rdr.Read();
				Compare(NextResultExists, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			int i = 1;
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check ResultSet 1 Data");
				while (rdr.Read())
				{
					i++;
				}
				Compare(i, TblResult0);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check ResultSet 1 Schema");
				Compare(rdr.GetSchemaTable().Rows[0].ItemArray.GetValue(0).ToString().ToUpper(), "CUSTOMERID");
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			// -------------- ResultSet  2 ------------
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check if ResultSet 2 exists");
				NextResultExists = rdr.NextResult();
				Compare(NextResultExists, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check if ResultSet 2 contains data");
				NextResultExists = rdr.Read();
				Compare(NextResultExists, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check ResultSet 2 Data");
				i = 1;
				while (rdr.Read())
				{
					i++;
				}
				Compare(i, TblResult1);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check ResultSet 2 Schema");
				Compare(rdr.GetSchemaTable().Rows[0].ItemArray.GetValue(0).ToString().ToUpper(), "CATEGORYID");
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			// -------------- ResultSet  3 ------------
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check if ResultSet 3 exists");
				NextResultExists = rdr.NextResult();
				Compare(NextResultExists, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check if ResultSet 3 contains data");
				NextResultExists = rdr.Read();
				Compare(NextResultExists, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check ResultSet 3 Data");
				i = 1;
				while (rdr.Read())
				{
					i++;
				}
				Compare(i, TblResult2);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check ResultSet 3 Schema");
				Compare(rdr.GetSchemaTable().Rows[0].ItemArray.GetValue(0).ToString().ToUpper(), "REGIONID");
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check that resultset 4 does not exist.");
				NextResultExists = rdr.NextResult();
				Compare(NextResultExists, false);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets sql text) - Check that resultset 4 does not contain data.");
				NextResultExists = rdr.Read();
				Compare(NextResultExists, false);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			if (!rdr.IsClosed)
				rdr.Close();
		}

		[Test]
#if !TARGET_JVM
		[Ignore ("JVM test")]
#endif
		public void TestMultipleResultSetsWithSP()
		{
#if !JAVA
			if (ConnectedDataProvider.GetDbType() == DataBaseServer.Oracle)
			{
				this.Log("Not testing Stored procedures with multiple ref-cursors on Oracle with .NET due to bug in .NET (only the first ref-cursor is retrived).");
				return;
			}

			if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.PostgreSQL)
			{
				// fail to work on .NET OLEDB
				this.Log("Not testing PostgreSQL CommandType.StoredProcedure which return SETOF");
				return;
			}
#endif
			
			bool NextResultExists = false;
			// transaction use was add for PostgreSQL
			OracleTransaction tr = con.BeginTransaction();
			OracleCommand cmd = new OracleCommand("GH_MULTIRECORDSETS", con, tr);
			cmd.Parameters.Add(new OracleParameter("RCT_Employees", OracleType.Cursor)).Direction = ParameterDirection.Output;
			cmd.Parameters.Add(new OracleParameter("RCT_Customers", OracleType.Cursor)).Direction = ParameterDirection.Output;
			cmd.Parameters.Add(new OracleParameter("RCT_Orders", OracleType.Cursor)).Direction = ParameterDirection.Output;
			cmd.CommandType = CommandType.StoredProcedure;
			OracleDataReader rdr = cmd.ExecuteReader();

			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check if ResultSet 1 exists");
				Compare(rdr != null, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check if ResultSet 1 contains data");
				NextResultExists = rdr.Read();
				Compare(NextResultExists, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check ResultSet 1 Data");
				Compare(rdr.GetValue(1).ToString(), "Yavine");
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check ResultSet 1 Schema");
				Compare(rdr.GetSchemaTable().Rows[0].ItemArray.GetValue(0).ToString().ToUpper(), "EMPLOYEEID");
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}

			
			// -------------- ResultSet  2 ------------
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check if ResultSet 2 exists");
				NextResultExists = rdr.NextResult();
				Compare(NextResultExists, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check if ResultSet 2 contains data");
				NextResultExists = rdr.Read();
				Compare(NextResultExists, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check ResultSet 2 Data");
				Compare(rdr.GetValue(1).ToString(), "Morgenstern Gesundkost");
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check ResultSet 2 Schema");
				Compare(rdr.GetSchemaTable().Rows[0].ItemArray.GetValue(0).ToString().ToUpper(), "CUSTOMERID");
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}

			// -------------- ResultSet  3 ------------
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check if ResultSet 3 exists");
				NextResultExists = rdr.NextResult();
				Compare(NextResultExists, true);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check if ResultSet 3 contains data");
				NextResultExists = rdr.Read();
				Compare(NextResultExists, false);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check ResultSet 3 Schema");
				Compare(rdr.GetSchemaTable().Rows[0].ItemArray.GetValue(0).ToString().ToUpper(), "ORDERID");
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check that resultset 4 does not exist.");
				NextResultExists = rdr.NextResult();
				Compare(NextResultExists, false);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}
			try
			{
				exp = null;
				BeginCase("(Multiple Resultsets stored proc.) - Check that resultset 4 does not contain data.");
				NextResultExists = rdr.Read();
				Compare(NextResultExists, false);
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally	
			{
				EndCase(exp);
			}

			//Cleanup:
			if (!rdr.IsClosed)
			{
				rdr.Close();
			}

			// transaction use was add for PostgreSQL
			tr.Commit();

		}


		#region "Private Utilities"
		private string BuildCommandText()
		{
			string beginStatement;
			string endStatement;
			string commandDelimiter;
			string[] commands = new string[] {"select * from Customers", "select * from Categories", "select * from Region"};

			GetDBSpecificSyntax(ConnectedDataProvider.GetDbType(), out beginStatement, out endStatement, out commandDelimiter);

			StringBuilder cmdBuilder = new StringBuilder();
			cmdBuilder.Append(beginStatement);
			cmdBuilder.Append(" ");
			foreach (string statement in commands)
			{
				cmdBuilder.Append(statement);
				cmdBuilder.Append(commandDelimiter);
				cmdBuilder.Append(" ");
			}
			cmdBuilder.Append(endStatement);

			return cmdBuilder.ToString();
		}
		private void GetDBSpecificSyntax(DataBaseServer dbServer, out string beginStatement, out string endStatement, out string commandDelimiter)
		{
			switch (dbServer)
			{
				case DataBaseServer.SQLServer:
					beginStatement = "BEGIN";
					endStatement = "END";
					commandDelimiter = ";";
					break;
				case DataBaseServer.Sybase:
					beginStatement = "BEGIN";
					endStatement = "END";
					commandDelimiter = "\r\n";
					break;
				case DataBaseServer.Oracle:
					beginStatement = "BEGIN";
					endStatement = "END;";
					commandDelimiter = ";";
					break;

				case DataBaseServer.DB2:
					{
						beginStatement = "";
						endStatement = "";
					}
					commandDelimiter = ";";
					break;

				case DataBaseServer.PostgreSQL:
					beginStatement = "";
					endStatement = "";
					commandDelimiter = ";";
					break;

				default:
					this.Fail("Unknown DataBaseServer type");
					throw new ApplicationException("Unknown DataBaseServer type");
			}
		}
		private void GetExcpectedResults(ref int TblResult0, ref int TblResult1, ref int TblResult2)
		{
			// get excpected results
			
			// transaction use was add for PostgreSQL
			OracleTransaction tr = con.BeginTransaction();
			OracleCommand cmd = new OracleCommand("", con,tr);
			cmd.CommandText = "Select count(*) from Customers";
			TblResult0 = Int32.Parse(cmd.ExecuteScalar().ToString());
			cmd.CommandText = "Select count(*) from Categories";
			TblResult1 = Int32.Parse(cmd.ExecuteScalar().ToString());
			cmd.CommandText = "Select count(*) from Region";
			TblResult2 = Int32.Parse(cmd.ExecuteScalar().ToString());
			tr.Commit();
		}

		#endregion
	}
}
