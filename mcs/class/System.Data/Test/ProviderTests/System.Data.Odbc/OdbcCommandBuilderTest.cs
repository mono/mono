// OdbcCommandBuilderTest.cs - NUnit Test Cases for testing the
// OdbcCommandBuilder Test.
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
using System.Data.Odbc;
using Mono.Data;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcCommandBuilderTest
	{
		[Test]
		public void GetInsertCommandTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				string selectQuery = "select id, lname from employee where id = 1";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, (OdbcConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count, "#1 atleast one table should be filled");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				OdbcCommand cmd = cb.GetInsertCommand ();
				Assert.AreEqual ("INSERT INTO employee (id, lname) VALUES (?, ?)",
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
				string selectQuery = "select id, lname, id+1 as next_id from employee where id = 1";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, (OdbcConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count, "#1 atleast one table should be filled");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				OdbcCommand cmd = cb.GetInsertCommand ();
				Assert.AreEqual ("INSERT INTO employee (id, lname) VALUES (?, ?)",
						cmd.CommandText, "#2");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetUpdateCommandTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			using (conn) {
				string selectQuery = "select id, lname, id+1 as next_id from employee where id = 1";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, (OdbcConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count, "#1 atleast one table should be filled");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				OdbcCommand cmd = cb.GetUpdateCommand ();
				Assert.AreEqual ("UPDATE employee SET id = ?, lname = ? WHERE ((id = ?) AND ((? = 1 AND lname IS NULL) OR (lname = ?)))",
						cmd.CommandText, "#2");
				Assert.AreEqual (5, cmd.Parameters.Count, "#3");
			}
		}

		[Test]
		[ExpectedException (typeof (DBConcurrencyException))]
		public void GetUpdateCommandDBConcurrencyExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				string selectQuery = "select id, lname from employee where id = 1";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, (OdbcConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count, "#1 atleast one table should be filled");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
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
				string selectQuery = "select id, lname, id+1 as next_id from employee where id = 1";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, (OdbcConnection) conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count, "#1 atleast one table should be filled");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				OdbcCommand cmd = cb.GetDeleteCommand ();
				Assert.AreEqual ("DELETE FROM employee WHERE ((id = ?) AND ((? = 1 AND lname IS NULL) OR (lname = ?)))",
						cmd.CommandText, "#2");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void DefaultPropertiesTest ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
#if NET_1_0
			Assert.AreEqual (ConflictOption.CompareAllSearchableValues, cb.ConflictDetection);
			Assert.AreEqual ("", cb.QuotePrefix, "#5");
			Assert.AreEqual ("", cb.QuoteSuffix, "#6");
#endif
#if NET_2_0				
			Assert.AreEqual (".", cb.CatalogSeparator, "#2");
			//Assert.AreEqual ("", cb.DecimalSeparator, "#3");
			Assert.AreEqual (".", cb.SchemaSeparator, "#4");
			Assert.AreEqual (CatalogLocation.Start, cb.CatalogLocation, "#1");
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				conn.Open ();
				cb = new OdbcCommandBuilder ();
				Assert.AreEqual ("\"monotest\"", cb.QuoteIdentifier ("monotest"), "#7");
				Assert.AreEqual ("monotest", cb.UnquoteIdentifier ("\"monotest\""), "#8");
				conn.Close ();
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
			// FIXME: test SetAllValues
#endif // NET_2_0
		}

		// FIXME:  Add tests for examining RowError
		// FIXME: Add test for ContinueUpdateOnError property

	}
}
