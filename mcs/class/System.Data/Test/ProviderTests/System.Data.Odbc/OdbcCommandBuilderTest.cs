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

#if !NO_ODBC

using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Connected.Odbc
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcCommandBuilderTest
	{
		[Test]
		public void GetInsertCommandTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			OdbcCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb;
				
				cb = new OdbcCommandBuilder (da);
				cmd = cb.GetInsertCommand ();
				Assert.AreEqual ("INSERT INTO employee (id, lname) VALUES (?, ?)",
						cmd.CommandText, "#A1");
				Assert.AreSame (conn, cmd.Connection, "#A2");
				AssertInsertParameters (cmd, "#A3:");

				cb = new OdbcCommandBuilder (da);
				cb.QuotePrefix = "\"";

				cmd = cb.GetInsertCommand ();
				Assert.AreEqual ("INSERT INTO \"employee (\"id, \"lname) VALUES (?, ?)",
						cmd.CommandText, "#B1");
				Assert.AreSame (conn, cmd.Connection, "#B2");
				AssertInsertParameters (cmd, "#B3:");

				cb = new OdbcCommandBuilder (da);
				cb.QuoteSuffix = "´";

				cmd = cb.GetInsertCommand ();
				Assert.AreEqual ("INSERT INTO employee´ (id´, lname´) VALUES (?, ?)",
						cmd.CommandText, "#C1");
				Assert.AreSame (conn, cmd.Connection, "#C2");
				AssertInsertParameters (cmd, "#C3:");

				cb = new OdbcCommandBuilder (da);
				cb.QuotePrefix = "\"";
				cb.QuoteSuffix = "´";

				cmd = cb.GetInsertCommand ();
				Assert.AreEqual ("INSERT INTO \"employee´ (\"id´, \"lname´) VALUES (?, ?)",
						cmd.CommandText, "#D1");
				Assert.AreSame (conn, cmd.Connection, "#D2");
				AssertInsertParameters (cmd, "#D3:");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void GetInsertCommandTestWithExpression ()
		{
			if (ConnectionManager.Instance.Odbc.EngineConfig.Type == EngineType.MySQL)
				Assert.Ignore ("Schema info from MySQL is incomplete");

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			OdbcCommand cmd = null;

			try {
				string selectQuery = "select id, lname, id+1 as next_id from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				cmd = cb.GetInsertCommand ();
				Assert.AreEqual ("INSERT INTO employee (id, lname) VALUES (?, ?)",
						cmd.CommandText, "#1");
				Assert.AreSame (conn, cmd.Connection, "#2");
				AssertInsertParameters (cmd, "#3:");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void GetUpdateCommandTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			OdbcCommand cmd = null;

			try {
				string selectQuery = "select id, lname, id+1 as next_id from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				cmd = cb.GetUpdateCommand ();
				Assert.AreEqual ("UPDATE employee SET id = ?, lname = ? WHERE ((id = ?) AND ((? = 1 AND lname IS NULL) OR (lname = ?)))",
						cmd.CommandText, "#A1");
				Assert.AreSame (conn, cmd.Connection, "#A2");
				AssertUpdateParameters (cmd, "#A3:");

				cb = new OdbcCommandBuilder (da);
				cb.QuotePrefix = "\"";

				cmd = cb.GetUpdateCommand ();
				Assert.AreEqual ("UPDATE \"employee SET \"id = ?, \"lname = ? WHERE ((\"id = ?) AND ((? = 1 AND \"lname IS NULL) OR (\"lname = ?)))",
						cmd.CommandText, "#B1");
				Assert.AreSame (conn, cmd.Connection, "#B2");
				AssertUpdateParameters (cmd, "#B3:");

				cb = new OdbcCommandBuilder (da);
				cb.QuoteSuffix = "´";

				cmd = cb.GetUpdateCommand ();
				Assert.AreEqual ("UPDATE employee´ SET id´ = ?, lname´ = ? WHERE ((id´ = ?) AND ((? = 1 AND lname´ IS NULL) OR (lname´ = ?)))",
						cmd.CommandText, "#C1");
				Assert.AreSame (conn, cmd.Connection, "#C2");
				AssertUpdateParameters (cmd, "#C3:");

				cb = new OdbcCommandBuilder (da);
				cb.QuotePrefix = "\"";
				cb.QuoteSuffix = "´";

				cmd = cb.GetUpdateCommand ();
				Assert.AreEqual ("UPDATE \"employee´ SET \"id´ = ?, \"lname´ = ? WHERE ((\"id´ = ?) AND ((? = 1 AND \"lname´ IS NULL) OR (\"lname´ = ?)))",
						cmd.CommandText, "#D1");
				Assert.AreSame (conn, cmd.Connection, "#D2");
				AssertUpdateParameters (cmd, "#D3:");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Ignore ("FIXME: Auto SQL generation during Update requires a valid SelectCommand")]
		public void GetUpdateCommandDBConcurrencyExceptionTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try {
				string selectQuery = "select id, lname from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				Assert.IsNotNull (cb);

				DataRow [] rows = ds.Tables [0].Select ("id=1");
				rows [0] [0] = 6660; // non existent 
				ds.Tables [0].AcceptChanges (); // moves 6660 to original value
				rows [0] [0] = 1; // moves 6660 as search key into db table

				try {
					da.Update (rows);
					Assert.Fail ("#1");
				} catch (DBConcurrencyException ex) {
					// Concurrency violation: the UpdateCommand
					// affected 0 records
					Assert.AreEqual (typeof (DBConcurrencyException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreSame (rows [0], ex.Row, "#5");
					Assert.AreEqual (1, ex.RowCount, "#6");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Category("NotWorking")]
		public void GetInsertCommandTest_option_true ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try {
				string selectQuery = "select id, lname from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				OdbcCommand cmd = cb.GetInsertCommand (true);
				Assert.AreEqual ("INSERT INTO employee (id, lname) VALUES (?, ?)",
						cmd.CommandText, "#1");
				Assert.AreSame (conn, cmd.Connection, "#2");
				AssertInsertParameters (cmd, "#3:");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void GetInsertCommandTest_option_false ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try {
				string selectQuery = "select id, lname from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				OdbcCommand cmd = cb.GetInsertCommand (false);
				Assert.AreEqual ("INSERT INTO employee (id, lname) VALUES (?, ?)",
						cmd.CommandText, "#1");
				Assert.AreSame (conn, cmd.Connection, "#2");
				AssertInsertParameters (cmd, "#3:");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Category("NotWorking")]
		public void GetUpdateCommandTest_option_true ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try {
				string selectQuery = "select id, lname, id+1 as next_id from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				OdbcCommand cmd = cb.GetUpdateCommand (true);
				Assert.AreEqual ("UPDATE employee SET id = ?, lname = ? WHERE ((id = ?) AND ((? = 1 AND lname IS NULL) OR (lname = ?)))",
						cmd.CommandText, "#1");
				Assert.AreSame (conn, cmd.Connection, "#2");
				AssertUpdateParameters (cmd, "#3:");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void GetUpdateCommandTest_option_false ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try {
				string selectQuery = "select id, lname, id+1 as next_id from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				OdbcCommand cmd = cb.GetUpdateCommand (false);
				Assert.AreEqual ("UPDATE employee SET id = ?, lname = ? WHERE ((id = ?) AND ((? = 1 AND lname IS NULL) OR (lname = ?)))",
						cmd.CommandText, "#1");
				Assert.AreSame (conn, cmd.Connection, "#2");
				AssertUpdateParameters (cmd, "#3:");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Category("NotWorking")]
		public void GetDeleteCommandTest_option_true ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try {
				string selectQuery = "select id, lname, id+1 as next_id from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				OdbcCommand cmd = cb.GetDeleteCommand (true);
				Assert.AreEqual ("DELETE FROM employee WHERE ((id = ?) AND ((? = 1 AND lname IS NULL) OR (lname = ?)))",
						cmd.CommandText, "#1");
				Assert.AreSame (conn, cmd.Connection, "#2");
				AssertDeleteParameters (cmd, "#3:");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void GetDeleteCommandTest_option_false ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try {
				string selectQuery = "select id, lname, id+1 as next_id from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				OdbcCommand cmd = cb.GetDeleteCommand (false);
				Assert.AreEqual ("DELETE FROM employee WHERE ((id = ?) AND ((? = 1 AND lname IS NULL) OR (lname = ?)))",
						cmd.CommandText, "#1");
				Assert.AreSame (conn, cmd.Connection, "#2");
				AssertDeleteParameters (cmd, "#3:");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void GetDeleteCommandTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			OdbcCommand cmd = null;

			try {
				string selectQuery = "select id, lname, id+1 as next_id from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");
				Assert.AreEqual (1, ds.Tables.Count);

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				cmd = cb.GetDeleteCommand ();

				Assert.AreEqual ("DELETE FROM employee WHERE ((id = ?) AND ((? = 1 AND lname IS NULL) OR (lname = ?)))",
						cmd.CommandText, "#A1");
				Assert.AreSame (conn, cmd.Connection, "#A2");
				AssertDeleteParameters (cmd, "#A3:");

				cb = new OdbcCommandBuilder (da);
				cb.QuotePrefix = "\"";

				cmd = cb.GetDeleteCommand ();
				Assert.AreEqual ("DELETE FROM \"employee WHERE ((\"id = ?) AND ((? = 1 AND \"lname IS NULL) OR (\"lname = ?)))",
						cmd.CommandText, "#B1");
				Assert.AreSame (conn, cmd.Connection, "#B2");
				AssertDeleteParameters (cmd, "#B3:");

				cb = new OdbcCommandBuilder (da);
				cb.QuoteSuffix = "´";

				cmd = cb.GetDeleteCommand ();
				Assert.AreEqual ("DELETE FROM employee´ WHERE ((id´ = ?) AND ((? = 1 AND lname´ IS NULL) OR (lname´ = ?)))",
						cmd.CommandText, "#C1");
				Assert.AreSame (conn, cmd.Connection, "#C2");
				AssertDeleteParameters (cmd, "#C3:");

				cb = new OdbcCommandBuilder (da);
				cb.QuotePrefix = "\"";
				cb.QuoteSuffix = "´";

				cmd = cb.GetDeleteCommand ();
				Assert.AreEqual ("DELETE FROM \"employee´ WHERE ((\"id´ = ?) AND ((? = 1 AND \"lname´ IS NULL) OR (\"lname´ = ?)))",
						cmd.CommandText, "#D1");
				Assert.AreSame (conn, cmd.Connection, "#D2");
				AssertDeleteParameters (cmd, "#D3:");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Ignore ("QuoteSuffix and QuotePrefix are now in DbCommandBuilder, while commands are in implementation classes. Result: we cannot perform this check until we refactor this.")]
		public void QuotePrefix_DeleteCommand_Generated ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			OdbcCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				cmd = cb.GetDeleteCommand ();
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#1");
				try {
					cb.QuotePrefix = "";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#6");
				cb.RefreshSchema ();
				cb.QuotePrefix = "";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Ignore ("QuoteSuffix and QuotePrefix are now in DbCommandBuilder, while commands are in implementation classes. Result: we cannot perform this check until we refactor this.")]
		public void QuotePrefix_InsertCommand_Generated ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			OdbcCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				cmd = cb.GetInsertCommand ();
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#1");
				try {
					cb.QuotePrefix = "";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#6");
				cb.RefreshSchema ();
				cb.QuotePrefix = "";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Ignore ("QuoteSuffix and QuotePrefix are now in DbCommandBuilder, while commands are in implementation classes. Result: we cannot perform this check until we refactor this.")]
		public void QuotePrefix_UpdateCommand_Generated ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			OdbcCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				cmd = cb.GetUpdateCommand ();
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#1");
				try {
					cb.QuotePrefix = "";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#6");
				cb.RefreshSchema ();
				cb.QuotePrefix = "";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Ignore ("QuoteSuffix and QuotePrefix are now in DbCommandBuilder, while commands are in implementation classes. Result: we cannot perform this check until we refactor this.")]
		public void QuoteSuffix_DeleteCommand_Generated ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			OdbcCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				cmd = cb.GetDeleteCommand ();
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#1");
				try {
					cb.QuoteSuffix = "";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#6");
				cb.RefreshSchema ();
				cb.QuoteSuffix = "";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Ignore ("QuoteSuffix and QuotePrefix are now in DbCommandBuilder, while commands are in implementation classes. Result: we cannot perform this check until we refactor this.")]
		public void QuoteSuffix_InsertCommand_Generated ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			OdbcCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				cmd = cb.GetInsertCommand ();
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#1");
				try {
					cb.QuoteSuffix = "";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#6");
				cb.RefreshSchema ();
				cb.QuoteSuffix = "";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Ignore ("QuoteSuffix and QuotePrefix are now in DbCommandBuilder, while commands are in implementation classes. Result: we cannot perform this check until we refactor this.")]
		public void QuoteSuffix_UpdateCommand_Generated ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			OdbcCommand cmd = null;

			try {
				string selectQuery = "select id, lname from employee where id = 3";
				OdbcDataAdapter da = new OdbcDataAdapter (selectQuery, conn);
				DataSet ds = new DataSet ();
				da.Fill (ds, "IntTest");

				OdbcCommandBuilder cb = new OdbcCommandBuilder (da);
				cmd = cb.GetUpdateCommand ();
				Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#1");
				try {
					cb.QuoteSuffix = "";
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// The QuotePrefix and QuoteSuffix properties
					// cannot be changed once an Insert, Update, or
					// Delete command has been generated
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
				Assert.AreEqual (string.Empty, cb.QuotePrefix, "#6");
				cb.RefreshSchema ();
				cb.QuoteSuffix = "";
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test] // QuoteIdentifier (String, OdbcConnection)
		[Category("NotWorking")] //needs https://github.com/dotnet/corefx/pull/22499
		public void QuoteIdentifier2 ()
		{
			OdbcCommandBuilder cb;
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			string quote_char = ConnectionManager.Instance.Odbc.EngineConfig.QuoteCharacter;

			try {
				cb = new OdbcCommandBuilder ();
				Assert.AreEqual (quote_char + "mono" + quote_char, cb.QuoteIdentifier ("mono", conn), "#A1");
				Assert.AreEqual (quote_char + "Z" + quote_char, cb.QuoteIdentifier ("Z", conn), "#A2");
				Assert.AreEqual (quote_char + "abc" + quote_char, cb.QuoteIdentifier ("abc", conn), "#A3");
				Assert.AreEqual (quote_char + quote_char, cb.QuoteIdentifier (string.Empty, conn), "#A4");
				Assert.AreEqual (quote_char + " " + quote_char, cb.QuoteIdentifier (" ", conn), "#A5");
				Assert.AreEqual (quote_char + "\r" + quote_char, cb.QuoteIdentifier ("\r", conn), "#A6");
				cb.QuoteSuffix = "def";
				Assert.AreEqual (quote_char + "mono" + quote_char, cb.QuoteIdentifier ("mono", conn), "#A7");
				Assert.AreEqual (quote_char + "Z" + quote_char, cb.QuoteIdentifier ("Z", conn), "#A8");
				Assert.AreEqual (quote_char + "abc" + quote_char, cb.QuoteIdentifier ("abc", conn), "#A9");
				Assert.AreEqual (quote_char + quote_char, cb.QuoteIdentifier (string.Empty, conn), "#A10");
				Assert.AreEqual (quote_char + " " + quote_char, cb.QuoteIdentifier (" ", conn), "#A11");
				Assert.AreEqual (quote_char + "\r" + quote_char, cb.QuoteIdentifier ("\r", conn), "#A12");

				cb = new OdbcCommandBuilder ();
				cb.QuotePrefix = "abc";
				Assert.AreEqual ("abcmono", cb.QuoteIdentifier ("mono", conn), "#B1");
				Assert.AreEqual ("abcZ", cb.QuoteIdentifier ("Z", conn), "#B2");
				Assert.AreEqual ("abcabc", cb.QuoteIdentifier ("abc", conn), "#B3");
				Assert.AreEqual ("abc", cb.QuoteIdentifier (string.Empty, conn), "#B4");
				Assert.AreEqual ("abc ", cb.QuoteIdentifier (" ", conn), "#B5");
				Assert.AreEqual ("abc\r", cb.QuoteIdentifier ("\r", conn), "#B6");
				cb.QuoteSuffix = "def";
				Assert.AreEqual ("abcmonodef", cb.QuoteIdentifier ("mono", conn), "#B7");
				Assert.AreEqual ("abcZdef", cb.QuoteIdentifier ("Z", conn), "#B8");
				Assert.AreEqual ("abcabcdef", cb.QuoteIdentifier ("abc", conn), "#B9");
				Assert.AreEqual ("abcdef", cb.QuoteIdentifier (string.Empty, conn), "#B10");
				Assert.AreEqual ("abc def", cb.QuoteIdentifier (" ", conn), "#B11");
				Assert.AreEqual ("abc\rdef", cb.QuoteIdentifier ("\r", conn), "#B12");

				cb.QuotePrefix = string.Empty;

				cb = new OdbcCommandBuilder ();
				cb.QuotePrefix = "X";
				Assert.AreEqual ("Xmono", cb.QuoteIdentifier ("mono", conn), "#D1");
				Assert.AreEqual ("XZ", cb.QuoteIdentifier ("Z", conn), "#D2");
				Assert.AreEqual ("XX", cb.QuoteIdentifier ("X", conn), "#D3");
				Assert.AreEqual ("X", cb.QuoteIdentifier (string.Empty, conn), "#D4");
				Assert.AreEqual ("X ", cb.QuoteIdentifier (" ", conn), "#D5");
				Assert.AreEqual ("X\r", cb.QuoteIdentifier ("\r", conn), "#D6");
				cb.QuoteSuffix = " ";
				Assert.AreEqual ("Xmono ", cb.QuoteIdentifier ("mono", conn), "#D7");
				Assert.AreEqual ("XZ ", cb.QuoteIdentifier ("Z", conn), "#D8");
				Assert.AreEqual ("XX ", cb.QuoteIdentifier ("X", conn), "#D9");
				Assert.AreEqual ("X ", cb.QuoteIdentifier (string.Empty, conn), "#D10");
				Assert.AreEqual ("X   ", cb.QuoteIdentifier (" ", conn), "#D11");
				Assert.AreEqual ("X\r ", cb.QuoteIdentifier ("\r", conn), "#D12");

				cb = new OdbcCommandBuilder ();
				cb.QuotePrefix = " ";
				Assert.AreEqual ("mono", cb.QuoteIdentifier ("mono", conn), "#E1");
				Assert.AreEqual ("Z", cb.QuoteIdentifier ("Z", conn), "#E2");
				Assert.AreEqual ("abc", cb.QuoteIdentifier ("abc", conn), "#E3");
				Assert.AreEqual (string.Empty, cb.QuoteIdentifier (string.Empty, conn), "#E4");
				Assert.AreEqual (" ", cb.QuoteIdentifier (" ", conn), "#E5");
				Assert.AreEqual ("\r", cb.QuoteIdentifier ("\r", conn), "#E6");
				cb.QuoteSuffix = "def";
				Assert.AreEqual ("mono", cb.QuoteIdentifier ("mono", conn), "#E7");
				Assert.AreEqual ("Z", cb.QuoteIdentifier ("Z", conn), "#E8");
				Assert.AreEqual ("abc", cb.QuoteIdentifier ("abc", conn), "#E9");
				Assert.AreEqual (string.Empty, cb.QuoteIdentifier (string.Empty, conn), "#E10");
				Assert.AreEqual (" ", cb.QuoteIdentifier (" ", conn), "#E11");
				Assert.AreEqual ("\r", cb.QuoteIdentifier ("\r", conn), "#E12");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		void AssertInsertParameters (OdbcCommand cmd, string prefix)
		{
			Assert.AreEqual (2, cmd.Parameters.Count, prefix + "Count");
			Assert.AreEqual (DbType.Int32, cmd.Parameters [0].DbType, prefix + "DbType (0)");
			Assert.AreEqual ("p1", cmd.Parameters [0].ParameterName, prefix + "ParameterName (0)");
			Assert.AreEqual ("id", cmd.Parameters [0].SourceColumn, prefix + "SourceColumn (0)");
			Assert.IsNull (cmd.Parameters [0].Value, prefix + "Value (0)");
			Assert.AreEqual (DbType.String, cmd.Parameters [1].DbType, prefix + "DbType (1)");
			Assert.AreEqual ("p2", cmd.Parameters [1].ParameterName, prefix + "ParameterName (1)");
			Assert.AreEqual ("lname", cmd.Parameters [1].SourceColumn, prefix + "SourceColumn (1)");
			Assert.IsNull (cmd.Parameters [1].Value, prefix + "Value (1)");
		}

		void AssertUpdateParameters (OdbcCommand cmd, string prefix)
		{
			Assert.AreEqual (5, cmd.Parameters.Count, prefix + "Count");
			Assert.AreEqual (DbType.Int32, cmd.Parameters [0].DbType, prefix + "DbType (0)");
			Assert.AreEqual ("p1", cmd.Parameters [0].ParameterName, prefix + "ParameterName (0)");
			Assert.AreEqual ("id", cmd.Parameters [0].SourceColumn, prefix + "SourceColumn (0)");
			Assert.IsNull (cmd.Parameters [0].Value, prefix + "Value (0)");
			Assert.AreEqual (DbType.String, cmd.Parameters [1].DbType, prefix + "DbType (1)");
			Assert.AreEqual ("p2", cmd.Parameters [1].ParameterName, prefix + "ParameterName (1)");
			Assert.AreEqual ("lname", cmd.Parameters [1].SourceColumn, prefix + "SourceColumn (1)");
			Assert.IsNull (cmd.Parameters [1].Value, prefix + "Value (1)");
			Assert.AreEqual (DbType.Int32, cmd.Parameters [2].DbType, prefix + "DbType (2)");
			Assert.AreEqual ("p3", cmd.Parameters [2].ParameterName, prefix + "ParameterName (2)");
			Assert.AreEqual ("id", cmd.Parameters [2].SourceColumn, prefix + "SourceColumn (2)");
			Assert.IsNull (cmd.Parameters [2].Value, prefix + "Value (2)");
			Assert.AreEqual (DbType.Int32, cmd.Parameters [3].DbType, prefix + "DbType (3)");
			Assert.AreEqual ("p4", cmd.Parameters [3].ParameterName, prefix + "ParameterName (3)");
			Assert.AreEqual ("lname", cmd.Parameters [3].SourceColumn, prefix + "SourceColumn (3)");
			Assert.AreEqual (1, cmd.Parameters [3].Value, prefix + "Value (3)");
			Assert.AreEqual (DbType.String, cmd.Parameters [4].DbType, prefix + "DbType (4)");
			Assert.AreEqual ("p5", cmd.Parameters [4].ParameterName, prefix + "ParameterName (4)");
			Assert.AreEqual ("lname", cmd.Parameters [4].SourceColumn, prefix + "SourceColumn (4)");
			Assert.IsNull (cmd.Parameters [4].Value, prefix + "Value (4)");
		}

		void AssertDeleteParameters (OdbcCommand cmd, string prefix)
		{
			Assert.AreEqual (3, cmd.Parameters.Count, prefix + "Count");
			Assert.AreEqual (DbType.Int32, cmd.Parameters [0].DbType, prefix + "DbType (0)");
			Assert.AreEqual ("p1", cmd.Parameters [0].ParameterName, prefix + "ParameterName (0)");
			Assert.AreEqual ("id", cmd.Parameters [0].SourceColumn, prefix + "SourceColumn (0)");
			Assert.IsNull (cmd.Parameters [0].Value, prefix + "Value (0)");
			Assert.AreEqual (DbType.Int32, cmd.Parameters [1].DbType, prefix + "DbType (1)");
			Assert.AreEqual ("p2", cmd.Parameters [1].ParameterName, prefix + "ParameterName (1)");
			Assert.AreEqual ("lname", cmd.Parameters [1].SourceColumn, prefix + "SourceColumn (1)");
			Assert.AreEqual (1, cmd.Parameters [1].Value, prefix + "Value (1)");
			Assert.AreEqual (DbType.String, cmd.Parameters [2].DbType, prefix + "DbType (2)");
			Assert.AreEqual ("p3", cmd.Parameters [2].ParameterName, prefix + "ParameterName (2)");
			Assert.AreEqual ("lname", cmd.Parameters [2].SourceColumn, prefix + "SourceColumn (2)");
			Assert.IsNull (cmd.Parameters [2].Value, prefix + "Value (2)");
		}

		// FIXME: test SetAllValues
		// FIXME:  Add tests for examining RowError
		// FIXME: Add test for ContinueUpdateOnError property
	}
}

#endif