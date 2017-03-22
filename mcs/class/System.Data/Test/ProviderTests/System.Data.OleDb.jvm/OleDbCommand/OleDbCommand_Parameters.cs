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

using MonoTests.System.Data.Utils.Data;

using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbCommand_Parameters : ADONetTesterClass 
	{
		//Used to test GUID.
		private const string TEST_GUID_STRING = "239A3C5E-8D41-11D1-B675-00C04FA3C554";

		private Exception exp;
		public static void Main()
		{
			OleDbCommand_Parameters tc = new OleDbCommand_Parameters();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbCommand_Parameters");
				tc.run();
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

			#region Simple Tests
			//string str="";
			string sql;
			OleDbConnection con = null;

			sql = "UPDATE Employees SET Region = ?, TitleOfCourtesy = ? WHERE EmployeeID=1";
			con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			
			//not testing with DB2 provider
			if (ConnectedDataProvider.GetDbType(con) != DataBaseServer.DB2) 
			{

				OleDbCommand cmd = new OleDbCommand(sql, con);
				cmd.Parameters.Add(new OleDbParameter("Region", OleDbType.VarWChar));
				cmd.Parameters.Add(new OleDbParameter("TitleOfCourtesy", OleDbType.VarWChar));

				con.Open();
				cmd.Parameters["Region"].Value = "WA";
				cmd.Parameters["TitleOfCourtesy"].Value = "Mr";

				//return the number of rows affected
				int i = cmd.ExecuteNonQuery();

				try
				{
					BeginCase("Check row count");
					Compare(i, 1);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}
			}

			if (con.State == ConnectionState.Open) con.Close();
			#endregion
			#region Test Parameter Types
			#region General
			TypesSubTests(ConnectedDataProvider.GetSimpleDbTypesParameters());
			TypesSubTests(ConnectedDataProvider.GetExtendedDbTypesParameters());
			#endregion
			#endregion
		}

		private void TypesSubTests(DbTypeParametersCollection typesToTest)
		{
			DbTypeParametersCollection currentlyTested = new DbTypeParametersCollection(typesToTest.TableName);
			int affectedRows;
			string rowId = string.Empty;

			foreach (DbTypeParameter currentParamType in typesToTest)
			{
				try
				{
					BeginCase("Test INSERT with parameter of type: " + currentParamType.DbTypeName);
					rowId = string.Format("13282_{0}", this.TestCaseNumber);
					currentlyTested.Clear();
					currentlyTested.Add(currentParamType);
					affectedRows = currentlyTested.ExecuteInsert(rowId);
					Compare(affectedRows, 1);
				} 
				catch(Exception ex)
				{
					exp = ex;
				}
				finally
				{
					currentlyTested.ExecuteDelete(rowId);
					EndCase(exp);
					exp = null;
				}
			}
		}

		//Test insertion of a GUID parameter on MSSQLServer.
		[Test]
		public void DoTestGUIDOnMSSQLServer()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer)
				return;
			DbTypeParametersCollection guidRow = new DbTypeParametersCollection(ConnectedDataProvider.SPECIFIC_TYPES_TABLE_NAME);
			guidRow.Add("UNIQUEIDENTIFIER", new Guid(TEST_GUID_STRING));
			TypesSubTests(guidRow);
		}
		//Test problems specific to the TIME type on DB2.
		[Test]
		public void DoTestTimeOnDB2()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.DB2)
				return;

			OleDbConnection conn = new OleDbConnection(ConnectedDataProvider.ConnectionString);
			string rowId = string .Empty;

			try
			{
				BeginCase("Test INSERT to TIME column using only TimeOfDate part of DateTime");
				rowId = "13282_" + this.TestCaseNumber.ToString();
				OleDbCommand cmd = new OleDbCommand();
				conn.Open();
				cmd.Connection = conn;
				cmd.CommandText = string.Format("INSERT INTO {0} (ID, T_TIME) VALUES ('{1}', ?)",ConnectedDataProvider.EXTENDED_TYPES_TABLE_NAME ,rowId);
				cmd.Parameters.Add("time", DateTime.Now.TimeOfDay);
				int affectedRows;
				affectedRows = cmd.ExecuteNonQuery();
				Compare(affectedRows, 1);
			}
			catch (Exception ex)
			{
				exp = ex;
			}
			finally
			{
				DbTypeParametersCollection.ExecuteDelete(ConnectedDataProvider.EXTENDED_TYPES_TABLE_NAME, rowId);
				conn.Close();
				EndCase(exp);
				exp = null;
			}

		}

		[Test]
#if JAVA
		[Category("NotWorking")]
#endif
		public void DoTestTimeOnDB2_bug3391()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.DB2)
				return;

			OleDbConnection conn = new OleDbConnection(ConnectedDataProvider.ConnectionString);
			string rowId = string .Empty;

			try {
				BeginCase("Test INSERT to TIME column using all of the DateTime");
				rowId = "13282_" + this.TestCaseNumber.ToString();
				OleDbCommand cmd = new OleDbCommand();
				conn.Open();
				cmd.Connection = conn;
				cmd.CommandText = string.Format("INSERT INTO {0} (ID, T_TIME) VALUES ('{1}', ?)",ConnectedDataProvider.EXTENDED_TYPES_TABLE_NAME ,rowId);
				cmd.Parameters.Add("time", DateTime.Now);
				try {
					cmd.ExecuteNonQuery();
					ExpectedExceptionNotCaught("System.OleDbException");
				}
				catch (OleDbException ex) {
					ExpectedExceptionCaught(ex);
				}
			}
			catch (Exception ex) {
				exp = ex;
			}
			finally {
				DbTypeParametersCollection.ExecuteDelete(ConnectedDataProvider.EXTENDED_TYPES_TABLE_NAME, rowId);
				conn.Close();
				EndCase(exp);
				exp = null;
			}
		}
	}
}
