using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlParameter_Direction : GHTBase
	{
		private Exception exp;
		public static void Main()
		{
			SqlParameter_Direction tc = new SqlParameter_Direction();
			tc.exp = null;
			tc.TestSetup();
			try
			{
				// Every Test must begin with BeginTest
				tc.BeginTest("SqlParameter_Direction");
				tc.run();
			}
			catch(Exception ex)
			{
				tc.exp = ex;
			}
			finally
			{
				tc.EndTest(tc.exp);
				tc.TestTearDown();
			}
		}
 
		public void run()
		{
			TestBug4703();
		}

		[TestFixtureSetUp]
		public void TestSetup()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			BeginCase("Test Setup");
			SqlConnection con = new SqlConnection(ConnectedDataProvider.ConnectionStringSQLClient);
			StringBuilder createTestSpBuilder = new StringBuilder();
			createTestSpBuilder.Append("CREATE PROCEDURE dbo.GHSP_DateTimeOutputTest");
			createTestSpBuilder.Append("(");
			createTestSpBuilder.Append("	@LastRefresh datetime OUTPUT");
			createTestSpBuilder.Append(")");
			createTestSpBuilder.Append("AS ");
			createTestSpBuilder.Append("SET @LastRefresh = GETDATE() ");
			createTestSpBuilder.Append("RETURN");
			SqlCommand createTestSpCmd = null;
			try
			{
				createTestSpCmd = new SqlCommand(createTestSpBuilder.ToString(), con);
				con.Open();
				createTestSpCmd.ExecuteNonQuery();
				Pass("Test setup completed successfuly.");
			}
			catch (Exception ex)
			{
				Fail("Test setup failed");
				exp = ex;
			}
			finally
			{
				EndCase(exp);
				if (con != null && con.State != ConnectionState.Closed)
				{
					con.Close();
				}
			}
		}

		[TestFixtureTearDown()]
		public void TestTearDown()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			BeginCase("Test Teardown");
			SqlConnection con = new SqlConnection(ConnectedDataProvider.ConnectionStringSQLClient);
			StringBuilder createTestSpBuilder = new StringBuilder();
			string dropTestSpSql = "DROP PROCEDURE dbo.GHSP_DateTimeOutputTest";
			SqlCommand dropTestSpCmd = null;
			try
			{
				dropTestSpCmd = new SqlCommand(dropTestSpSql, con);
				con.Open();
				dropTestSpCmd.ExecuteNonQuery();
				Pass("Test teardown completed successfuly.");
			}
			catch (Exception ex)
			{
				Fail("Test teardown failed");
				exp = ex;
			}
			finally
			{
				EndCase(exp);
				if (con != null && con.State != ConnectionState.Closed)
				{
					con.Close();
				}
			}
		}
		
		[Test]
		public void TestBug4703()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			try
			{
				BeginCase("Test Bug 4703 - DateTime output parameter of stored procedure contains incorrect time ( always 12:00 AM )");
				string strConnection = ConnectedDataProvider.ConnectionStringSQLClient;
				SqlConnection conn = new SqlConnection(strConnection);
				conn.Open();
				SqlCommand command = conn.CreateCommand();
				SqlParameter param = null;

				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "GHSP_DateTimeOutputTest";

				param = command.CreateParameter();
				param.ParameterName="@LastRefresh";
				param.DbType = DbType.DateTime;
				param.Direction = ParameterDirection.InputOutput;
				DateTime testValue = DateTime.Now;
				param.Value = testValue;

				command.Parameters.Add( param );
				Compare(param.Value, testValue);
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