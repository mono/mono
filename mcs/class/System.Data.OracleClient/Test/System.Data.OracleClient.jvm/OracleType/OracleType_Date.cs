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
using System.Data.OracleClient ;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleType_Date : GHTBase
	{
		private string dateColumnName;
		private string dateTableName;

		private OracleConnection	con;
		private OracleDataReader	dr	=	null;

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

				dateColumnName = "T_DATE";

				con = new OracleConnection(ConnectedDataProvider.ConnectionString);
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
			OracleType_Date tc = new OracleType_Date();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleType_Date");
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
			OracleCommand cmd = new OracleCommand();
			string rowId = "54416_";

			try
			{
				// clean the test table
				cmd = new OracleCommand(string.Format("DELETE FROM {0} WHERE ID like '54416_%'", dateTableName));
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

				cmd = new OracleCommand(string.Format("insert into {0} (ID, {1}) values ('{2}', :date1)", dateTableName, dateColumnName, rowId));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2001, 1, 13);
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				// checking that value returned correctly;
				cmd.CommandText = string.Format("select {0} from {1} where ID='{2}'", dateColumnName, dateTableName, rowId);
				cmd.Parameters.Clear();
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
				cmd = new OracleCommand(string.Format("insert into {0} (ID, {1}) values('{2}', :date1)", dateTableName, dateColumnName, rowId));
				cmd.Connection = con;
				TestedDate1 = new DateTime(1753, 1, 1); 
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				this.Log(cmd.CommandText);
				cmd.ExecuteNonQuery();

				//' checking that value returned correctly
				cmd.CommandText = string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				this.Log(cmd.CommandText);
				cmd.Parameters.Clear();
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
				cmd = new OracleCommand(string.Format("insert into {0} (ID, {1}) values('{2}', :date1)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13);
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd.CommandText = string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				cmd.Parameters.Clear();
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
				cmd = new OracleCommand(string.Format("insert into {0} (ID, {1}) values('{2}', :date1)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13);
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd.CommandText = string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				cmd.Parameters.Clear();
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
				cmd = new OracleCommand(string.Format("insert into {0} (ID, {1}) values('{2}', :date1)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13, 12, 13, 14);
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd.CommandText = string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				cmd.Parameters.Clear();
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
				cmd = new OracleCommand(string.Format("insert into {0} (ID, {1}) values('{2}', :date1)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13, 0, 0, 0);
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd.CommandText =string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				cmd.Parameters.Clear();
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
				cmd = new OracleCommand(string.Format("insert into {0} (ID, {1}) values('{2}', :date1)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13, 23, 59, 59);
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd.CommandText =string.Format("select {0} from {1}  where ID='{2}'", dateColumnName, dateTableName, rowId);
				cmd.Parameters.Clear();
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
				cmd = new OracleCommand(string.Format("insert into {0} (ID, {1}) values('{2}', :date1)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				//TestedDate1 = new DateTime(2500, 1, 13, 11, 0, 0);
				TestedDate1 = new DateTime(1988,5,31,15,33,44,00);
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				cmd.Parameters.Clear();
				
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
				cmd = new OracleCommand(string.Format("insert into {0} (ID, {1}) values('{2}', :date1)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13, 11, 0, 0);
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd = new OracleCommand(string.Format("select {0} from {1} where ID='{2}' and {0}= :date1", dateColumnName, dateTableName, rowId));
				cmd.Connection = con;
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
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
				cmd = new OracleCommand(string.Format("insert into {0} (ID, {1}) values('{2}', :date1)", dateTableName, dateColumnName, rowId ));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2007, 12, 31, 11, 59, 59);
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				cmd.ExecuteNonQuery();
				//' checking that value returned correctly
				cmd = new OracleCommand(string.Format("select {0} from {1} where ID='{2}' and {0} >:date1 and {0} <:date2", dateColumnName, dateTableName, rowId));
				cmd.Connection = con;
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1.AddSeconds(-1);
				cmd.Parameters.Add(new OracleParameter("date2", OracleType.DateTime)).Value = TestedDate1.AddSeconds(+1);
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
				cmd = new OracleCommand(string.Format("insert into {0} (ID) values('{1}')", dateTableName, rowId));
				cmd.Connection = con;
				cmd.ExecuteNonQuery();
				cmd = new OracleCommand(string.Format("update {0} set {1} = :date1 where ID='{2}'", dateTableName, dateColumnName, rowId));
				cmd.Connection = con;
				TestedDate1 = new DateTime(2500, 1, 13, 1, 2, 3);
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
				cmd.ExecuteNonQuery();

				//' checking that value returned correctly
				cmd = new OracleCommand(string.Format("select {0} from {1} where ID='{2}' and {0}= :date1", dateColumnName, dateTableName, rowId));
				cmd.Connection = con;
				cmd.Parameters.Add(new OracleParameter("date1", OracleType.DateTime)).Value = TestedDate1;
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
			OracleCommand deleteCmd = new OracleCommand(string.Format("DELETE FROM {0} WHERE ID = '{1}'", dateTableName, rowId), con);
			deleteCmd.ExecuteNonQuery();
		}
	}
}