using System;
using System.Data;
using System.Data.SqlClient;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlCommand_ExecuteReader_ : ADONetTesterClass
	{
		private Exception exp;
		public static void Main()
		{
			SqlCommand_ExecuteReader_ tc = new SqlCommand_ExecuteReader_();
			try
			{
				// Every Test must begin with BeginTest
				tc.BeginTest("SqlCommand_ExecuteReader");
				tc.run();
			}
			catch(Exception ex)
			{
				tc.exp = ex;
			}
			finally
			{
				// Every Test must End with EndTest
				tc.EndTest(tc.exp);
			}
		}


		[Test] 
		public void run()
		{
			// testing only SQLServerr
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer)
			{
				Log("This test is relevant only for MSSQLServer!");
				return;
			}

			DoTestCheckSqlStatementThatDeclaresLocalVariableAndUsesIt();
		}

		public void DoTestCheckSqlStatementThatDeclaresLocalVariableAndUsesIt()
		{
			SqlConnection conn = new SqlConnection(ConnectedDataProvider.ConnectionStringSQLClient);
			SqlDataReader rdr=null;
			try
			{
				BeginCase("Check sql statement that declares a local variable and uses it.");
				SqlCommand cmd = new SqlCommand();
				conn.Open();
				cmd.Connection = conn;

				cmd.CommandText = "declare @var int; select @var=1;";
				cmd.CommandType = CommandType.Text;
				
				rdr = cmd.ExecuteReader();
				Compare(rdr.Read(), false);
			}
			catch (Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp);
				exp = null;
				if (conn != null && conn.State != ConnectionState.Closed)
				{
					conn.Close();
				}
				if (rdr != null && !rdr.IsClosed)
				{
					rdr.Close();
				}
			}
		}

	}
}
