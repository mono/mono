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
using System.Data.OleDb;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbCommandBuilder_DeriveParameters_O : GHTBase
	{
		OleDbConnection	con;
		OleDbCommand cmd;
		Exception exp = null;

		[SetUp]
		public void SetUp()
		{
			exp=null;
			BeginCase("Setup");
			try
			{
				con = new OleDbConnection(ConnectedDataProvider.ConnectionString);
				con.Open();
				cmd = new OleDbCommand("", con);
				Compare("Setup", "Setup");
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		[TearDown]
		public void TearDown()
		{
			if (con != null)
			{
				if (con.State == ConnectionState.Open) con.Close();
			}
		}

		public static void Main()
		{
			OleDbCommandBuilder_DeriveParameters_O tc = new OleDbCommandBuilder_DeriveParameters_O();
			try
			{
				tc.BeginTest("OleDbCommandBuilder_DeriveParameters_O");
				tc.SetUp();
				tc.run();
				tc.TearDown();
			}
			catch(Exception ex)
			{
				tc.exp = ex;
			}
			finally
			{
				tc.EndTest(tc.exp);
			}
		}


		public void run()
		{
            RetrieveParameters();
		}

		[Test]
		public void RetrieveParameters()
		{
			exp = null;

			try
			{
				BeginCase("retrieve parameters");
				
				if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.DB2)
				{
					this.Skip("Not Implemented on DB2.");
					return;
				}

				switch (ConnectedDataProvider.GetDbType(con))
				{
//					case MonoTests.Utils.DataBaseServer.PostgreSQL:
//						cmd = new OleDbCommand("GH_MULTIRECORDSETS('a','b','c')", con);
//						break;
					default:
						cmd = new OleDbCommand("GH_MultiRecordSets", con);
						break;
				}
				
				cmd.CommandType = CommandType.StoredProcedure;
				OleDbCommandBuilder.DeriveParameters(cmd);

				switch (ConnectedDataProvider.GetDbType(con))
				{
					case DataBaseServer.SQLServer:
					case DataBaseServer.Sybase:
						Compare(cmd.Parameters.Count, 1);
						Compare(cmd.Parameters[0].Direction, ParameterDirection.ReturnValue);
						Compare(cmd.Parameters[0].ParameterName, "RETURN_VALUE");
						break;
				
					case DataBaseServer.Oracle:
						Compare(cmd.Parameters.Count, 3);
						Compare(cmd.Parameters[0].Direction, ParameterDirection.Output);
						Compare(cmd.Parameters[1].Direction, ParameterDirection.Output);
						Compare(cmd.Parameters[2].Direction, ParameterDirection.Output);
						Compare(cmd.Parameters[0].ParameterName, "RCT_EMPLOYEES");
						break;

					case DataBaseServer.PostgreSQL:
						Compare(cmd.Parameters.Count, 1);
						Compare(cmd.Parameters[0].Direction, ParameterDirection.ReturnValue);
						Compare(cmd.Parameters[0].ParameterName, "returnValue");
						break;

					default:
						throw new ApplicationException(string.Format("GHT: Test not implemented for DB type {0}", ConnectedDataProvider.GetDbType(con)));
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
