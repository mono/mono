using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlConnection_BeginTransaction_S : ADONetTesterClass
	{
		SqlConnection con;

		[SetUp]
		public void SetUp()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			Exception exp = null;
			BeginCase("Setup");
			try
			{
				con = new SqlConnection(ConnectedDataProvider.ConnectionStringSQLClient);
				con.Open();
				Compare("Setup", "Setup");
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		[TearDown]
		public void TearDown()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			if (con != null)
			{
				if (con.State == ConnectionState.Open) con.Close();
			}
		}

		public static void Main()
		{
			SqlConnection_BeginTransaction_S tc = new SqlConnection_BeginTransaction_S();
			Exception exp = null;
			try
			{
				tc.BeginTest("SqlConnection_BeginTransaction_S");

				//testing only on SQLServer
				if (ConnectedDataProvider.GetDbType(ConnectedDataProvider.ConnectionStringSQLClient) != DataBaseServer.SQLServer) return ; 

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

		[Test] 
		public void run()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			Exception exp = null;

			#region		---- Bug 2716 - MSSQL - SqlCommand.Transaction ---- 
			// testing only SQLServerr
			if (ConnectedDataProvider.GetDbType(con.ConnectionString) != DataBaseServer.SQLServer)
			{
				try
				{
					BeginCase("Bug 2716 - MSSQL - SqlCommand.Transaction");
					SqlCommand comm = new SqlCommand("SELECT * FROM Customers",con);

					SqlTransaction trans = con.BeginTransaction("transaction");
					comm.Transaction = trans;

					con.Close();
					Compare(con.State,ConnectionState.Closed);
				} 
				catch(Exception ex)
				{
					exp = ex;
				}
				finally
				{
					if (con != null)
					{if (con.State == ConnectionState.Open) con.Close();}

					EndCase(exp);
					exp = null;
				}

			}
			#endregion

		}
	}
}