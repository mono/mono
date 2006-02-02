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
using System.Data.OleDb ;

using MonoTests.System.Data.Utils;


using NUnit.Framework;
#if DAAB
using Microsoft.ApplicationBlocks.Data;
#endif

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbCommand_CommandType : ADONetTesterClass
	{
		OleDbConnection	con;
		// transaction is must on PostgreSQL
		OleDbTransaction tr;
		OleDbCommand cmd;
		OleDbDataReader dr = null;
		DataBaseServer dbServerType;

		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				con.Open();
				tr = con.BeginTransaction();
				cmd = new OleDbCommand("", con, tr);
				dbServerType = ConnectedDataProvider.GetDbType(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				Assert.AreEqual("Setup", "Setup");
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
			OleDbCommand_CommandType tc = new OleDbCommand_CommandType();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbCommand_CommandType");
				tc.SetUp();
				tc.run();
				tc.TearDown();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			Exception exp = null;
		
			OleDbCommand cmd = new OleDbCommand();
			try
			{
				BeginCase("CommandType - default");
				Assert.AreEqual(cmd.CommandType , CommandType.Text );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("CommandType - TableDirect");
				cmd.CommandType = CommandType.TableDirect; 
				Assert.AreEqual(cmd.CommandType , CommandType.TableDirect);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("CommandType - Text");
				cmd.CommandType = CommandType.Text  ; 
				Assert.AreEqual(cmd.CommandType , CommandType.Text);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}



			#region		---- CommandType.Text using Parameters.Add ---- 
			try
			{
				BeginCase("CommandType.Text using Parameters.Add");

				cmd = new OleDbCommand();
				cmd.Connection = con;
				cmd.Transaction = tr;
				cmd.CommandType = CommandType.Text;
				switch (dbServerType)
				{
					case DataBaseServer.PostgreSQL:
						cmd.CommandText = "SELECT * FROM GH_REFCURSOR3(?)";
						break;
					default:
						cmd.CommandText = "{call GH_REFCURSOR3(?)}";
						break;
				}

				OleDbParameter param1 = cmd.Parameters.Add("IN_LASTNAME", OleDbType.VarChar,20);
				param1.Direction = ParameterDirection.Input;
				param1.Value = "Yavine"; 
#if DAAB
#if !JAVA
				if ((dbServerType == DataBaseServer.PostgreSQL))
				{
					dr = PostgresOleDbHelper.OLEDB4ODBCExecuteReader(cmd,true);
				}
				else
#endif
#endif
				{
					dr = cmd.ExecuteReader();
				}

				if (dr.HasRows)
				{
					dr.Read();
					Assert.AreEqual(dr.GetValue(0).ToString(),"1");
					Assert.AreEqual(dr.GetString(1),"Yavine");
				}
				else
					Assert.AreEqual("error","HasRows=0");

			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if (con != null)
				{if (con.State == ConnectionState.Open) con.Close();}

				EndCase(exp);
				exp = null;
			}
			#endregion
		
			CommandTypeSP_Manual_InOutParameters();

			#region		---- ORACLE CommandType.StoredProcedure using DeriveParameters ---- 
			if (ConnectedDataProvider.GetDbType(con) == MonoTests.System.Data.Utils.DataBaseServer.Oracle)
			{
				try
				{
					BeginCase("ORACLE CommandType.StoredProcedure using DeriveParameters");

					con.Open();
					cmd = new OleDbCommand();
					cmd.Connection = con;
					cmd.Transaction = tr;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "GH_REFCURSOR3";

					OleDbCommandBuilder.DeriveParameters(cmd);
					cmd.Parameters[0].Value = "Yavine"; 
					cmd.Parameters.RemoveAt(1); // the ORACLE DAAB trick is to remove the out parameter

					dr = cmd.ExecuteReader();
					if (dr.HasRows)
					{
						dr.Read();
						Assert.AreEqual(dr.GetValue(0).ToString(),"1");
						Assert.AreEqual(dr.GetString(1),"Yavine");
					}
					else
						Assert.AreEqual("error","HasRows=0");

				} 
				catch(Exception ex)
				{
					exp = ex;
				}
				finally
				{
					if (dr != null)dr.Close();
					if (con != null)
					{if (con.State == ConnectionState.Open) con.Close();}

					EndCase(exp);
					exp = null;
				}
			}		
			#endregion

			#region CommandType.StoredProcedure in order to repreduce bug 4003
			if (ConnectedDataProvider.GetDbType(con) == MonoTests.System.Data.Utils.DataBaseServer.SQLServer)
			{
				exp = null;
				try
				{
					if (con.State == ConnectionState.Closed) con.Open();
					BeginCase("Bug 4003");
					OleDbCommand cmd4003 = new OleDbCommand("[mainsoft].[GH_DUMMY]",con);
					cmd4003.CommandType = CommandType.StoredProcedure;
					cmd4003.Parameters.Add("@EmployeeIDPrm","1");
					cmd4003.ExecuteReader();
					
				}
				catch (Exception ex)
				{
					exp=ex;
				}
				finally
				{
					if (con.State == ConnectionState.Open) con.Close();
					EndCase(exp);
				}

			}

			#endregion
		}

		#region		---- CommandType.StoredProcedure manual in out parameters ---- 
		public void CommandTypeSP_Manual_InOutParameters()
		{
			Exception exp = null;
			try
			{
				BeginCase("CommandType.StoredProcedure manual in out parameters");

				if (ConnectedDataProvider.GetDbType(con) == MonoTests.System.Data.Utils.DataBaseServer.PostgreSQL)
				{
					this.Log("CommandType.StoredProcedure manual in out parameters is not tested in oracle.");
					return;
				}

				con.Open();
				cmd = new OleDbCommand();
				cmd.Connection = con;

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "GH_INOUT1";

				//RETURN_VALUE for SQLServer
				if (ConnectedDataProvider.GetDbType(con) == MonoTests.System.Data.Utils.DataBaseServer.SQLServer)
				{
					OleDbParameter param0 = cmd.Parameters.Add("@RETURN_VALUE", OleDbType.Integer);
					param0.Direction = ParameterDirection.ReturnValue;
				}

				OleDbParameter param1 = cmd.Parameters.Add("@INPARAM", OleDbType.VarChar,20);
				param1.Direction = ParameterDirection.Input;
				param1.Value = Convert.ToString("dummy"); 
	
				OleDbParameter param2 = cmd.Parameters.Add("@OUTPARAM", OleDbType.Integer);//VarNumeric);
				param2.Direction = ParameterDirection.Output;

				int ret = cmd.ExecuteNonQuery();
				int intReturn;
				intReturn = Convert.ToInt32(cmd.Parameters["@OUTPARAM"].Value);
				Assert.AreEqual(intReturn,100);

			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if (con != null)
				{if (con.State == ConnectionState.Open) con.Close();}

				EndCase(exp);
				exp = null;
			}
		}
		#endregion
	}

}