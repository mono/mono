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

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbType_Date : GHTBase
	{
		private string dateColumnName;
		private string dateTableName;

		private OleDbConnection	con;
		private OleDbDataReader	dr	=	null;

		private DateTime TestedDate1;
		private DateTime RetDate;

		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				dateTableName = ConnectedDataProvider.EXTENDED_TYPES_TABLE_NAME;
				switch (ConnectedDataProvider.GetDbType())
				{
					case DataBaseServer.SQLServer:
					case DataBaseServer.Sybase:
						dateColumnName = "T_DATETIME";
						break;
					case DataBaseServer.Oracle:
						dateColumnName = "T_DATE";
						break;
					case DataBaseServer.PostgreSQL:
						dateColumnName = "T_TIMESTAMP";
						break;
					case DataBaseServer.DB2:
						dateColumnName = "T_TIMESTAMP";
						break;
				}
				con = new OleDbConnection(ConnectedDataProvider.ConnectionString);
				con.Open();
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
			OleDbType_Date tc = new OleDbType_Date();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbType_Date");
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
			string rowId = "54416_";

			try
			{
				// clean the test table
				cmd = new OleDbCommand(string.Format("DELETE FROM {0} WHERE ID like '54416_%'", dateTableName));
				cmd.Connection = con;
				cmd.ExecuteNonQuery();
			}
			catch(Exception ex)
			{
				exp = ex;
			}

			#region		---- testing parameterized query with a simple date ---- 
			try
			{
				BeginCase("testing parameterized query with a simple date");
				rowId = "54416_" + TestCaseNumber.ToString();

				cmd = new OleDbCommand(string.Format("insert into {0} (ID, {1}) values ('{2}', ?)", dateTableName, dateColumnName, rowId));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2001, 1, 13);
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				// checking that value returned correctly;
				cmd.CommandText = string.Format("select {0} from {1} where ID='{2}'", dateColumnName, dateTableName, rowId);
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar());

				Compare(TestedDate1,RetDate);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}

				EndCase(exp);
				exp = null;
			}
		
			#endregion

			#region		---- testing parameterized query with a 1753 date min  ---- 
			try
			{
				BeginCase("testing parameterized query with a 1753 date min");
				rowId = "54416_" + TestCaseNumber.ToString();

				con.Open();
				cmd = new OleDbCommand(string.Format("insert into {0} (ID, {1}) values('{2}', ?)", dateTableName, dateColumnName, rowId));
				cmd.Connection = con;
				TestedDate1 = new DateTime(1753, 1, 1); 
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				this.Log(cmd.CommandText);
				cmd.ExecuteNonQuery();

				//' checking that value returned correctly
				cmd.CommandText = string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				this.Log(cmd.CommandText);
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar());
				Compare(TestedDate1.Date, RetDate.Date);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}

				EndCase(exp);
				exp = null;
			}
		
			#endregion

			#region		---- testing parameterized query with a future date ---- 
			try
			{
				BeginCase("testing parameterized query with a future date");
				
				rowId = "54416_" + TestCaseNumber.ToString();
				con.Open();
				cmd = new OleDbCommand(string.Format("insert into {0} (ID, {1}) values('{2}', ?)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13);
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd.CommandText = string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar());
				Compare(TestedDate1, RetDate);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}

				EndCase(exp);
				exp = null;
			}
		
			#endregion

			#region		---- testing parameterized query with a future date ---- 
			try
			{
				BeginCase("testing parameterized query with a future date");

				rowId = "54416_" + TestCaseNumber.ToString();
				con.Open();
				cmd = new OleDbCommand(string.Format("insert into {0} (ID, {1}) values('{2}', ?)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13);
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd.CommandText = string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar());
				Compare(TestedDate1, RetDate);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}

				EndCase(exp);
				exp = null;
			}
		
			#endregion

			#region		---- testing parameterized query with a time part ---- 
			try
			{
				BeginCase("testing parameterized query with a time part");
				
				rowId = "54416_" + TestCaseNumber.ToString();
				con.Open();
				cmd = new OleDbCommand(string.Format("insert into {0} (ID, {1}) values('{2}', ?)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13, 12, 13, 14);
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd.CommandText = string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar());
				Compare(TestedDate1, RetDate);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}

				EndCase(exp);
				exp = null;
			}
		
			#endregion

			#region		---- testing parameterized query with a time part of 00:00 ---- 
			try
			{
				BeginCase("testing parameterized query with a time part of 00:00");

				rowId = "54416_" + TestCaseNumber.ToString();
				con.Open();
				cmd = new OleDbCommand(string.Format("insert into {0} (ID, {1}) values('{2}', ?)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13, 0, 0, 0);
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd.CommandText =string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar());
				Compare(TestedDate1, RetDate);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}

				EndCase(exp);
				exp = null;
			}
		
			#endregion

			#region		---- testing parameterized query with a time part of 23:59:59 ---- 
			try
			{
				BeginCase("testing parameterized query with a time part of 23:59:59");
				
				rowId = "54416_" + TestCaseNumber.ToString();
				con.Open();
				cmd = new OleDbCommand(string.Format("insert into {0} (ID, {1}) values('{2}', ?)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13, 23, 59, 59);
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd.CommandText =string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar());
				Compare(TestedDate1, RetDate);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}
				EndCase(exp);
				exp = null;
			}
		
			#endregion

			#region		---- testing parameterized query with a time part of AM ---- 
			try
			{
				BeginCase("testing parameterized query with a time part of AM");
				string str = string.Empty; //This is an addional test ,passing GH mechnizim 
				rowId = "54416_" + TestCaseNumber.ToString();
				con.Open();
				cmd = new OleDbCommand(string.Format("insert into {0} (ID, {1}) values('{2}', ?)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				//TestedDate1 = new DateTime(2500, 1, 13, 11, 0, 0);
				TestedDate1 = new DateTime(1988,5,31,15,33,44,00);
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				
				//TODO:add also treat for other db
				if (ConnectedDataProvider.GetDbType(con) == MonoTests.System.Data.Utils.DataBaseServer.SQLServer || ConnectedDataProvider.GetDbType(con) == MonoTests.System.Data.Utils.DataBaseServer.Sybase)
				{
					cmd.CommandText = string.Format("select CONVERT(varchar,{0},120) from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
					str = cmd.ExecuteScalar().ToString();
					Compare(TestedDate1, Convert.ToDateTime(str));
				}
				//' checking that value returned correctly
				cmd.CommandText =string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar().ToString());
				Compare(TestedDate1, RetDate);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}

				EndCase(exp);
				exp = null;
			}
		
			#endregion

			#region		---- testing a where clause using a date ---- 
			try
			{
				BeginCase("testing a where clause using a date");

				rowId = "54416_" + TestCaseNumber.ToString();
				con.Open();
				cmd = new OleDbCommand(string.Format("insert into {0} (ID, {1}) values('{2}', ?)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13, 11, 0, 0);
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd = new OleDbCommand(string.Format("select {0} from {1} where ID='{2}' and {0}= ?", dateColumnName, dateTableName, rowId));
				cmd.Connection = con;
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar());
				Compare(TestedDate1, RetDate);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}

				EndCase(exp);
				exp = null;
			}
		
			#endregion

			#region		---- testing a where clause using a rage of dates ---- 
			try
			{
				BeginCase("testing a where clause using a rage of dates");
				
				rowId = "54416_" + TestCaseNumber.ToString();
				con.Open();
				cmd = new OleDbCommand(string.Format("insert into {0} (ID, {1}) values('{2}', ?)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2007, 12, 31, 11, 59, 59);
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd = new OleDbCommand(string.Format("select {0} from {1} where ID='{2}' and {0} >? and {0} <?", dateColumnName, dateTableName, rowId));
				cmd.Connection = con;
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1.AddSeconds(-1);
				cmd.Parameters.Add(new OleDbParameter("date2", OleDbType.DBTimeStamp)).Value = TestedDate1.AddSeconds(+1);
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar());
				Compare(TestedDate1, RetDate);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}
				EndCase(exp);
				exp = null;
			}
		
			#endregion

			#region		---- testing a set statement ---- 
			try
			{
				BeginCase("testing a set statement");

				rowId = "54416_" + TestCaseNumber.ToString();
				con.Open();
				cmd = new OleDbCommand(string.Format("insert into {0} (ID) values('{1}')", dateTableName, rowId));
				cmd.Connection = con;
				cmd.ExecuteNonQuery();
				cmd = new OleDbCommand(string.Format("update {0} set {1} = ? where ID='{2}'", dateTableName, dateColumnName, rowId));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13, 1, 2, 3);
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				cmd.ExecuteNonQuery();

				//' checking that value returned correctly
				cmd = new OleDbCommand(string.Format("select {0} from {1} where ID='{2}' and {0}= ?", dateColumnName, dateTableName, rowId));
				cmd.Connection = con;
				cmd.Parameters.Add(new OleDbParameter("date1", OleDbType.DBTimeStamp)).Value = TestedDate1;
				RetDate = Convert.ToDateTime(cmd.ExecuteScalar());
				Compare(TestedDate1, RetDate);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (dr != null)dr.Close();
				if ( (con != null) && (con.State == ConnectionState.Open) )
				{
					CleanTestRow(rowId);
					con.Close();
				}

				EndCase(exp);
				exp = null;
			}
		
			#endregion
		}

		/// <summary>
		/// Deletes a row from the date table, according to its ID.
		/// </summary>
		/// <param name="rowId">Id of the row to delete.</param>
		private void CleanTestRow(string rowId)
		{
			OleDbCommand deleteCmd = new OleDbCommand(string.Format("DELETE FROM {0} WHERE ID = '{1}'", dateTableName, rowId), con);
			deleteCmd.ExecuteNonQuery();
		}
	}
}