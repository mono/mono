// SqlCommandBuilderTest.cs - NUnit Test Cases for testing the
// SqlCommandBuilder class
//
// Authors:
//      Sureshkumar T (tsureshkumar@novell.com)
// 
// Copyright (c) 2004 Novell Inc., and the individuals listed on the
// ChangeLog entries.
//
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Mono.Data;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	[Category ("sqlserver")]
	public class SqlCommandBuilderTest
	{
		[Test]
		public void GetInsertCommandTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				string selectQuery = "select id, fname from employee where id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, (SqlConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count, "#1 atleast one table should be filled");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				SqlCommand cmd = cb.GetInsertCommand ();
				Assert.AreEqual ("INSERT INTO employee (id, fname) VALUES (@p1, @p2)",
						cmd.CommandText, "#2");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetInsertCommandTestWithExpression ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				string selectQuery = "select id, fname, id+1 as next_id from employee where id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, (SqlConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count, "#1 atleast one table should be filled");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				SqlCommand cmd = cb.GetInsertCommand ();
				Assert.AreEqual ("INSERT INTO employee (id, fname) VALUES (@p1, @p2)",
						cmd.CommandText, "#2");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetUpdateCommandTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				string selectQuery = "select id, fname, id+1 as next_id from employee where id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, (SqlConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count, "#1 atleast one table should be filled");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				SqlCommand cmd = cb.GetUpdateCommand ();
				Assert.AreEqual ("UPDATE employee SET id = @p1, fname = @p2 WHERE ((id = @p3) AND ((@p4 = 1 AND fname IS NULL) OR (fname = @p5)))",
						cmd.CommandText, "#2");
				Assert.AreEqual (5, cmd.Parameters.Count, "#3");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}

		}

		[Test]
		public void GetUpdateCommandTest_CheckNonUpdatableColumns ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "create table #tmp_table (id int primary key , counter int identity(1,1), value varchar(10))";
				cmd.ExecuteNonQuery ();

				string selectQuery = "select id, counter, value, id+1 as next_id from #tmp_table";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, (SqlConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds);
				Assert.AreEqual (1, ds.Tables.Count, "#1"); 
				Assert.AreEqual (4, ds.Tables [0].Columns.Count, "#2");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				SqlCommand updateCmd = cb.GetUpdateCommand ();
				Assert.AreEqual ("UPDATE #tmp_table SET id = @p1, value = @p2 WHERE ((id = @p3) AND ((@p4 = 1 AND " +
							"counter IS NULL) OR (counter = @p5)) AND ((@p6 = 1 AND value IS NULL) OR (value = @p7)))",
						updateCmd.CommandText, "#3");
				Assert.AreEqual (7, updateCmd.Parameters.Count, "#3");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
		[Test]
		[ExpectedException (typeof (DBConcurrencyException))]
		public void GetUpdateCommandDBConcurrencyExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				string selectQuery = "select id, fname from employee where id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, (SqlConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count, "#1 atleast one table should be filled");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				DataRow [] rows = ds.Tables [0].Select ("id=1");
				rows [0] [0] = 6660; // non existent 
				ds.Tables [0].AcceptChanges (); // moves 6660 to original value
				rows [0] [0] = 1; // moves 6660 as search key into db table
				da.Update (rows);
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetDeleteCommandTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				string selectQuery = "select id, fname, id+1 as next_id from employee where id = 1";
				SqlDataAdapter da = new SqlDataAdapter (selectQuery, (SqlConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count, "#1 atleast one table should be filled");

				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				SqlCommand cmd = cb.GetDeleteCommand ();
				Assert.AreEqual ("DELETE FROM employee WHERE ((id = @p1) AND ((@p2 = 1 AND fname IS NULL) OR (fname = @p3)))",
						cmd.CommandText, "#2");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void DefaultPropertiesTest ()
		{
			SqlCommandBuilder cb = new SqlCommandBuilder ();
#if NET_1_1 || NET_1_0 || ONLY_1_1
			Assert.AreEqual (ConflictOption.CompareAllSearchableValues, cb.ConflictDetection);
#endif // NET_1_1 || NET_1_0 || ONLY_1_1
			Assert.AreEqual ("", cb.QuotePrefix, "#5");
			Assert.AreEqual ("", cb.QuoteSuffix, "#6");
#if NET_2_0				
			Assert.AreEqual (".", cb.CatalogSeparator, "#2");
			Assert.AreEqual ("", cb.DecimalSeparator, "#3");
			Assert.AreEqual (".", cb.SchemaSeparator, "#4");
			Assert.AreEqual (CatalogLocation.Start, cb.CatalogLocation, "#1");
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				conn.Open ();
				cb = new SqlCommandBuilder ();
				Assert.AreEqual ("\"monotest\"", cb.QuoteIdentifier ("monotest", (SqlConnection) conn), "#7");
				Assert.AreEqual ("monotest", cb.UnquoteIdentifier ("\"monotest\"", (SqlConnection) conn), "#8");
				conn.Close ();
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
			// FIXME: test SetAllValues
#endif // NET_2_0
		}

		// FIXME:  Add tests for examining RowError
		// FIXME: Add test for ContinueUpdateOnError property
		
		[Test]
		public void CheckParameters_BuiltCommand ()
		{
			using (IDbConnection conn = ConnectionManager.Singleton.Connection) {
				SqlDataAdapter adapter = new SqlDataAdapter ("select id,type_varchar from string_family", (SqlConnection)conn);
				SqlCommandBuilder cb = new SqlCommandBuilder(adapter);
				DataSet ds = new DataSet ();
				adapter.Fill(ds);

				Assert.AreEqual (2, cb.GetInsertCommand().Parameters.Count, "#1");

				DataRow row_rsInput = ds.Tables[0].NewRow();
				row_rsInput["id"] = 100;
				row_rsInput["type_varchar"] = "ttt";
				ds.Tables[0].Rows.Add(row_rsInput);

				Assert.AreEqual (2, cb.GetInsertCommand().Parameters.Count, "#2");

				row_rsInput = ds.Tables[0].NewRow();
				row_rsInput["id"] = 101;
				row_rsInput["type_varchar"] = "ttt";
				ds.Tables[0].Rows.Add(row_rsInput);

				Assert.AreEqual (2, cb.GetInsertCommand().Parameters.Count, "#3");
			}
		}
	}
}
