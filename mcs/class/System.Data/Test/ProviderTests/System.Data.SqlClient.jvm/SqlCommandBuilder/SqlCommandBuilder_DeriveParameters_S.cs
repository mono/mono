using System;
using System.Data;
using System.Data.SqlClient;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlCommandBuilder_DeriveParameters_S : GHTBase
	{
		SqlConnection con;
		SqlCommand cmd;
		public static void Main()
		{
			SqlCommandBuilder_DeriveParameters_S tc = new SqlCommandBuilder_DeriveParameters_S();
			Exception exp = null;
			try
			{
				// Every Test must begin with BeginTest
				tc.BeginTest("DeriveParameters");
				tc.run();
			}
			catch(Exception ex)
			{
				exp=ex;
			}
			finally
			{
				// Every Test must End with EndTest
				tc.EndTest(exp);
			}
			
			
		}

		[SetUp]
		public void setUp()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			if (con == null)
			{
				con = new SqlConnection(ConnectedDataProvider.ConnectionStringSQLClient);
				con.Open();
			}
		}

		[TearDown]
		public void tearDown()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			if (con.State == ConnectionState.Open)
			{
				con.Close();
			}

		}

		public void run()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer)
			{
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			setUp();
			test();
			tearDown();

			
		}

		[Test]
		public void test()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer)
			{
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			Exception exp = null;
			BeginCase("Checking with sp that doesn't exsits ");
			try
			{
				cmd = new SqlCommand("NotExists",con);
				cmd.CommandType = CommandType.StoredProcedure;
				SqlCommandBuilder.DeriveParameters(cmd);
			} 
			catch(InvalidOperationException ex)
			{
				ExpectedExceptionCaught(ex);
				exp = ex;
			}
			finally
			{
				if (exp == null)
				{
					ExpectedExceptionNotCaught("InvalidOperationException");
				}
				EndCase(null);
				exp = null;
			}

		}

		//Activate This Construntor to log All To Standard output
		//public TestClass():base(true){}

		//Activate this constructor to log Failures to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, false){}

		//Activate this constructor to log All to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, true){}

		//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

	}
}