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
#if DAAB
using Microsoft.ApplicationBlocks;
#endif

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbCommand_ExecuteReader : ADONetTesterClass
	{
		OleDbConnection	con;
		OleDbCommand cmd;
		
		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				cmd = new OleDbCommand("", con);
				con.Open();
				this.Pass("Setup.");
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
			OleDbCommand_ExecuteReader tc = new OleDbCommand_ExecuteReader();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbCommand_ExecuteReader");
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
			bool RecordsExists = false;
			OleDbDataReader rdr =null;

//			testBug3965();
//			TestMultipleResultsets();
//			TestCompoundVariable();

			cmd.CommandText = "Select FirstName,City From Employees";
			if (con.State != ConnectionState.Open)
			{
				con.Open();
			}

			try
			{
				BeginCase("check reader is null");
				rdr = cmd.ExecuteReader();
				Compare(rdr==null, false);
			} 
			catch(Exception ex){exp = ex;}
			finally
			{
				if (rdr != null) rdr.Close();
				EndCase(exp);
				exp = null;
			}

			try
			{
				BeginCase("check reader.read");
				rdr = cmd.ExecuteReader();
				RecordsExists = rdr.Read();
				Compare(RecordsExists ,true);
			} 
			catch(Exception ex){exp = ex;}
			finally
			{
				if (rdr != null) rdr.Close();
				EndCase(exp); 
				exp = null;
			}

			try
			{
				BeginCase("execute reader again ");
				rdr = cmd.ExecuteReader();
				Compare(rdr==null, false);
			} 
			catch(Exception ex){exp = ex;}
			finally
			{
				if (rdr != null) rdr.Close();
				EndCase(exp); 
				exp = null;
			}

			try
			{
				BeginCase("Test compound SQL statement");
				//Build a compund SQL command.
				string[] sqlStatements = new string[] {
														  "INSERT INTO Categories (CategoryName, Description) VALUES('__TEST_RECORD__', 'Inserted')",
														  "UPDATE Categories  SET Description='Updated' WHERE CategoryName='__TEST_RECORD__'",
														  "DELETE FROM Categories WHERE CategoryName='__TEST_RECORD__'" ,
				};
				cmd.CommandText = CreateCompundSqlStatement(sqlStatements, ConnectedDataProvider.GetDbType());
				rdr = cmd.ExecuteReader();
				Compare(rdr.Read(), false);
			} 
			catch(Exception ex){exp = ex;}
			finally
			{
				if (rdr != null) rdr.Close();
				EndCase(exp); 
				exp = null;
			}


			if (ConnectedDataProvider.GetDbType() != DataBaseServer.Oracle)
			{
				try
				{
					BeginCase("Check that in a compound SQL statement, resultsets are returned only for SELECT statements. (bug #3358)");
					//prepare db:
					OleDbCommand prepare = new OleDbCommand("DELETE FROM Categories WHERE CategoryName='__TEST_RECORD__'", con);
					prepare.ExecuteNonQuery();


					//Test body
					int resultSetCount ;

					//Build a compund SQL command that contains only one select statement.
					string[] sqlStatements = new string[] {
															  "INSERT INTO Categories (CategoryName, Description) VALUES('__TEST_RECORD__', 'Inserted')",
															  "UPDATE Categories  SET Description='Updated' WHERE CategoryName='__TEST_RECORD__'",
															  "DELETE FROM Categories WHERE CategoryName='__TEST_RECORD__'" ,
															  "SELECT * FROM Categories "
														  };
					string insertCmdTxt = CreateCompundSqlStatement(sqlStatements, ConnectedDataProvider.GetDbType());
					//this.Log(insertCmdTxt);
					OleDbCommand InsertCmd = new OleDbCommand(insertCmdTxt, con);
					rdr = InsertCmd.ExecuteReader();

					//Count the number of result sets.
					resultSetCount = 0;
					do
					{
						resultSetCount++;
					}while (rdr.NextResult());

					//Test that there is only one result set.
					Compare(resultSetCount, 1);
				} 
				catch(Exception ex){exp = ex;}
				finally
				{
					if (rdr != null) rdr.Close();
					//cleanup db:
					OleDbCommand cleanup = new OleDbCommand("DELETE FROM Categories WHERE CategoryName='__TEST_RECORD__'", con);
					cleanup.ExecuteNonQuery();
					EndCase(exp); 
					exp = null;
				}
			}

			if (ConnectedDataProvider.GetDbType() == DataBaseServer.Oracle)
			{
				try
				{
					BeginCase("Use out refcursor implicitly to get resultset from stored-procedure in command type text.");
					cmd.CommandText = "{ call ghsp_types_simple_1 (null, null, 1.234, null, null, null, null)}";
					cmd.CommandType = CommandType.Text;
					rdr = cmd.ExecuteReader();
					Compare(rdr.HasRows, true);
				} 
				catch(Exception ex){exp = ex;}
				finally
				{
					if (rdr != null) rdr.Close();
					EndCase(exp); 
					exp = null;
				}

				try
				{
					BeginCase("Use out refcursor implicitly to get resultset from stored-procedure in CommandType.StoredProcedure.");
					cmd.CommandText = "GHSP_TYPES_SIMPLE_1";
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("", 123456);
					cmd.Parameters.Add("", "asdasd");
					cmd.Parameters.Add("", 1.234);
					cmd.Parameters.Add("", "asdasd");
					cmd.Parameters.Add("", "asdasd");
					cmd.Parameters.Add("", "asdasd");
					cmd.Parameters.Add("", "asdasd");
					rdr = cmd.ExecuteReader();
					Compare(rdr.HasRows, true);
				}
				catch(Exception ex){exp = ex;}
				finally
				{
					if (rdr != null) rdr.Close();
					EndCase(exp); 
					exp = null;
				}
			}
		}


		//Create the compund sql statement according to the dbserver.
		private string CreateCompundSqlStatement(string[] sqlStatements, DataBaseServer dbServer)
		{
			string beginStatement;
			string endStatement;
			string commandDelimiter;

			GetDBSpecificSyntax(dbServer, out beginStatement, out endStatement, out commandDelimiter);

			StringBuilder cmdBuilder = new StringBuilder();
			cmdBuilder.Append(beginStatement);
			cmdBuilder.Append(" ");
			foreach (string statement in sqlStatements)
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
				case DataBaseServer.PostgreSQL:
					beginStatement = "";
					endStatement = "";
					commandDelimiter = ";";
					break;

				case DataBaseServer.Sybase:
					beginStatement = "BEGIN";
					endStatement = "END";
					commandDelimiter = "";
					break;
				case DataBaseServer.Oracle:
					beginStatement = "BEGIN";
					endStatement = "END;";
					commandDelimiter = ";";
					break;

				case DataBaseServer.DB2:
					beginStatement = "BEGIN ATOMIC";
					endStatement = "END";
					commandDelimiter = ";";
					break;

				default:
					this.Fail("Unknown DataBaseServer type");
					throw new ApplicationException("Unknown DataBaseServer type");
			}
		}

		[Test]
		public void testBug3965()
		{
			// testing only SQLServerr
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				Log("This test is relevant only for MSSQLServer!");
				return;
			}
			Exception exp=null;
			BeginCase("Test for bug #3965");
			OleDbConnection con = new OleDbConnection("Provider=SQLOLEDB.1;Data Source=TESTDIR;User ID=ghuser;Password=[PLACEHOLDER];Persist Security Info=True;Initial Catalog=default_grasshopper_db;Packet Size=4096;Connect Timeout=60");
			con.Open();
			string text = "SELECT     td.TESTCYCL.TC_TEST_ID, td.TESTCYCL.TC_STATUS, td.BUG.BG_STATUS, td.BUG.BG_BUG_ID, td.BUG.BG_USER_03, td.BUG.BG_USER_09,";
			text+=  " td.BUG.BG_USER_10, td.BUG.BG_SUMMARY,BG_DETECTION_VERSION";
			text+= " FROM         td.TESTCYCL INNER JOIN";
			text+= " td.TEST ON td.TESTCYCL.TC_TEST_ID = td.TEST.TS_TEST_ID INNER JOIN";
			text+= " td.ALL_LISTS ON td.TEST.TS_SUBJECT = td.ALL_LISTS.AL_ITEM_ID RIGHT OUTER JOIN";
			text+= " td.BUG ON td.TEST.TS_TEST_ID = td.BUG.BG_TEST_REFERENCE";
			OleDbCommand cmd = new OleDbCommand(text,con);
			try 
			{		
				cmd.ExecuteReader();
				Compare(true,true);
				
			}
			catch (Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp);
			}
		}


		[Test]
		public void TestMultipleResultsets()
		{
#if !JAVA
			if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.Oracle)
			{
				//In .NET there is a bug when calling a SP with multiple REFCURSORS, the workaround is to use OracleClient and not OleDb.
				//In GH we are not bug complient in this issue, because there is no workaround (We do not support the OracleClient namespace.
			    this.Log("Not testing multi result set Oracle on .NET");
				return;
			}

			if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.PostgreSQL)
			{
				// fail to work on .NET OLEDB
				//reader = Microsoft.ApplicationBlocks.Data.PostgresOleDbHelper.ADOExecuteReader(cmd1);
				this.Log("Not testing PostgreSQL CommandType.StoredProcedure which return SETOF");
				return;
			}
#endif

			Exception exp = null;
			BeginCase("Test multi result set from stored procedure");

				
			OleDbDataReader reader = null;
			OleDbTransaction tr = null;

			try
			{
				//Check SP with the structre : insert Select + update Select + delete Select
				if (con.State != ConnectionState.Open)
				{
					con.Open();
				}
					
				// transaction use was add for PostgreSQL
				tr = con.BeginTransaction();
				OleDbCommand cmd1 = new OleDbCommand("GHSP_TYPES_SIMPLE_4", con, tr);
				cmd1.CommandType = CommandType.StoredProcedure;

				OleDbParameter param = new OleDbParameter();
				param.ParameterName = "ID1";
				param.Value = string.Format("13268_{0}", this.TestCaseNumber);
				param.OleDbType = OleDbType.VarWChar;
				cmd1.Parameters.Add(param);

				reader = cmd1.ExecuteReader();
				
				//Count the number of result sets.
				int resultSetCount = 0;
				//Count the number of the records
				int recordCounter=0;

				do
				{
					//this.Log(string.Format("resultSetCount:{0}",resultSetCount));
				while (reader.Read())
				{
					recordCounter++;
				}
					//this.Log(string.Format("recordCounter:{0}",recordCounter));
					if (resultSetCount != 2)
					{
						Compare(recordCounter,1); //Insert + update 
					}
					else
					{
						Compare(recordCounter,0); //Delete 
					}

					recordCounter=0;
					resultSetCount++;
				}while (reader.NextResult());

				Compare(resultSetCount,3);
			}
			catch (Exception ex)
			{
				exp=ex;
			}
			finally
			{
				if (reader != null) reader.Close();
				tr.Commit();
				con.Close();
				EndCase(exp);
			}
		}
		[Test]
		public void TestCompoundVariable()
		{
			OleDbDataReader rdr = null;
			if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.PostgreSQL)
			{
				this.Log("not testing PostgreSQL");
				return;
			}

			Exception exp = null;
			try
			{
				BeginCase("Check sql statement that declares a local variable and uses it.");

				if (con.State != ConnectionState.Open)
				{
					con.Open();
				}

				string sqlTxt = "";
				switch (ConnectedDataProvider.GetDbType(cmd.Connection))
				{
					case DataBaseServer.SQLServer:
						sqlTxt = "declare @var int; select @var=1;";
						break;
					case DataBaseServer.Sybase:
						sqlTxt = "declare @var int select @var=1";
						break;
					case DataBaseServer.Oracle:
						sqlTxt = "declare var int;begin var:=1;end;";
						break;
					case DataBaseServer.DB2:
						sqlTxt = "begin atomic declare var integer; set var = 1; end";
						break;
					case DataBaseServer.PostgreSQL:
						// we don't know how the heck to do this in PostgreSQL
						sqlTxt = "";
						break;
					default:
						throw new ApplicationException(string.Format("GHT: Unknown DataBaseServer '{0}'", ConnectedDataProvider.GetDbType(cmd.Connection)));
				}
				cmd.CommandText = sqlTxt;
				rdr = cmd.ExecuteReader();
				Compare(rdr.Read(), false);
			} 
			catch(Exception ex){exp = ex;}
			finally
			{
				if (rdr != null) rdr.Close();
				EndCase(exp); 
				exp = null;
			}
		}
	}

}
