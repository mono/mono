using System;
using System.Data;
using System.Data.SqlClient;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlParameter_ctor_SO : GHTBase
	{
		private Exception exp = null;

		public static void Main()
		{
			SqlParameter_ctor_SO tc = new SqlParameter_ctor_SO();
			try
			{
				tc.BeginTest("SqlParameter_ctor_SO");
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
			CreateParamWithTypeBoolTrue();
			CreateParamWithTypeBoolFalse();
		}

		[Test(Description="Create an SqlParameter with value of type bool (true)")]
		public void CreateParamWithTypeBoolTrue()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}
			exp = null;

			try
			{
				BeginCase("Create an SqlParameter with value of type bool (true)");
				SqlParameter p = new SqlParameter("name", true);
				Compare(p.Value.GetType(), typeof(bool));
				Compare(p.DbType, DbType.Boolean);
				Compare(p.SqlDbType, SqlDbType.Bit);
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

		[Test(Description="Create an SqlParameter with value of type bool (false)")]
		public void CreateParamWithTypeBoolFalse()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}
			exp = null;

			try
			{
				BeginCase("Create an SqlParameter with value of type bool (false)");
				SqlParameter p = new SqlParameter("name", false);
				Compare(p.Value.GetType(), typeof(bool));
				Compare(p.DbType, DbType.Boolean);
				Compare(p.SqlDbType, SqlDbType.Bit);
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