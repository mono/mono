using System;
using System.Data;
using System.Data.SqlClient;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlParameter_set_DbType_D : GHTBase
	{
		private Exception exp;
		public static void Main()
		{
			SqlParameter_set_DbType_D tc = new SqlParameter_set_DbType_D();
			tc.exp = null;
			try
			{
				// Every Test must begin with BeginTest
				tc.BeginTest("SqlParameter_set_DbType_D");
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
 
		public void run()
		{
			TestBug4689();
		}

		[Test]
		public void TestBug4689()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			try
			{
				// Every Sub Test must begin with BeginCase
				BeginCase("Test Bug 4689 - Exception when adding System.Data.DbType.Date parameter");
				SqlCommand command = new SqlCommand();
				SqlParameter param = command.CreateParameter();
				param.ParameterName = "@EffectiveDate";
				param.DbType = DbType.Date;
				param.Value = DateTime.Now.Date;
				command.Parameters.Add(param);
				Pass("Addition of parameter didn't throw exception.");
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				// Every Sub Test must end with EndCase
				EndCase(exp);
				exp = null;
			}
		}

	}
}