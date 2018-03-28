//
// SqlTransactionTest.cs - NUnit Test Cases for testing the
//                          SqlTransaction class
// Author:
//      Umadevi S (sumadevi@novell.com)
//	Sureshkumar T (tsureshkumar@novell.com)
//
// Copyright (c) 2004 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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
using System.Data.Common;
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.Connected.SqlClient
{
	[TestFixture]
	[Category ("sqlserver")]
	public class SqlTransactionTest
	{
		SqlConnection conn;
		SqlTransaction trans;
		String connectionString;
		EngineConfig engine;

		[SetUp]
		public void SetUp ()
		{
			connectionString = ConnectionManager.Instance.Sql.ConnectionString;
			engine = ConnectionManager.Instance.Sql.EngineConfig;
		}

		[TearDown]
		public void TearDown ()
		{
			if (conn != null)
				conn.Dispose ();
			if (trans != null)
				trans.Dispose ();
		}

		[Test]
		[Category("NotWorking")]
		public void Commit ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			SqlConnection connA = null;
			SqlConnection connB = null;

			try {
				connA = new SqlConnection (connectionString);
				connA.Open ();

				connB = new SqlConnection (connectionString);
				connB.Open ();

				using (trans = connA.BeginTransaction ()) {
				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, connA, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", connA, trans);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#A1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#A2");
						Assert.IsFalse (reader.Read (), "#A3");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", connB);
					cmd.CommandTimeout = 2;
					try {
						cmd.ExecuteReader ();
						Assert.Fail ("#B1");
					} catch (SqlException ex) {
						// Timeout expired.  The timeout period
						// elapsed prior to completion of the
						// operation or the server is not responding
						Assert.AreEqual (typeof (SqlException), ex.GetType (), "#B2");
						Assert.AreEqual ((byte) 11, ex.Class, "#B3");
						Assert.IsNotNull (ex.Message, "#B5");
						Assert.AreEqual (-2, ex.Number, "#B6");
						Assert.AreEqual ((byte) 0, ex.State, "#B7");
					}

					trans.Commit ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", connB);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#C1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#C2");
						Assert.IsFalse (reader.Read (), "#C3");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", connA);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#D1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#D2");
						Assert.IsFalse (reader.Read (), "#D3");
					}
				}
			} finally {
				if (connA != null)
					connA.Close ();
				if (connB != null)
					connB.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Commit_Connection_Closed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				SqlCommand cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				conn.Close ();

				try {
					trans.Commit ();
					Assert.Fail ("#A1");
				} catch (InvalidOperationException ex) {
					// This SqlTransaction has completed; it is no
					// longer usable
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#B");
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Commit_Reader_Open ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			SqlCommand cmd;

			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();

			try {
				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = new SqlCommand ("select @@version", conn, trans);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					try {
						trans.Commit ();
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// There is already an open DataReader
						// associated with this Command which
						// must be closed first
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn, trans);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsTrue (reader.Read (), "#B1");
				}

				trans.Dispose ();
				conn.Close ();

				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#C1");
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Commit_Transaction_Committed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Commit ();

					try {
						trans.Commit ();
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#B1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#B2");
						Assert.IsFalse (reader.Read (), "#B3");
					}

					conn.Close ();
					conn = new SqlConnection (connectionString);
					conn.Open ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#C1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#C2");
						Assert.IsFalse (reader.Read (), "#C3");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Commit_Transaction_Disposed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Dispose ();

					try {
						trans.Commit ();
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#B1");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Commit_Transaction_Rolledback ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Rollback ();

					try {
						trans.Commit ();
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#B1");
					}
				}

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Save ("SAVE1");

					sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6667, 'BangaloreNovell', '1999-03-10', '2006-08-23')";
					cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Rollback ("SAVE1");
					trans.Commit ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#D1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#D2");
						Assert.IsFalse (reader.Read (), "#D3");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6667", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#E1");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Connection_Transaction_Committed ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();
			trans.Commit ();

			Assert.IsNull (trans.Connection);
		}

		[Test]
		public void Connection_Transaction_Disposed ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();
			trans.Dispose ();

			Assert.IsNull (trans.Connection);
		}

		[Test]
		public void Connection_Transaction_Open ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();

			Assert.AreSame (conn, trans.Connection);
		}

		[Test]
		public void Connection_Transaction_Rolledback ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();
			try {
				trans.Rollback ();
				Assert.IsNull (trans.Connection);
			} finally {
				trans.Dispose ();
			}

			trans = conn.BeginTransaction ();
			trans.Save ("SAVE1");
			try {
				trans.Rollback ("SAVE1");
				Assert.AreSame (conn, trans.Connection);
			} finally {
				trans.Dispose ();
			}
		}

		[Test]
		public void Connection_Transaction_Saved ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();
			trans.Save ("SAVE1");

			Assert.AreSame (conn, trans.Connection);
		}

		[Test]
		public void Dispose ()
		{
			string sql;
			SqlCommand cmd = null;

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE1");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6667, 'BangaloreNovell', '1999-03-10', '2006-08-23')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Dispose ();
				trans.Dispose ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#1");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6667", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#2");
				}
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Dispose_Connection_Closed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				SqlCommand cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				conn.Close ();

				trans.Dispose ();

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read ());
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Dispose_Reader_Open ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				SqlCommand cmd = new SqlCommand ("select * from employee", conn, trans);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					try {
						trans.Dispose ();
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// There is already an open DataReader
						// associated with this Connection
						// which must be closed first
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					try {
						trans.Dispose ();
						Assert.Fail ("#B1");
					} catch (InvalidOperationException ex) {
						// There is already an open DataReader
						// associated with this Connection
						// which must be closed first
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
						Assert.IsNull (ex.InnerException, "#B3");
						Assert.IsNotNull (ex.Message, "#B4");
					}
				}

				trans.Dispose ();
			} finally {
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();
			}
		}

		[Test]
		public void IsolationLevel_Reader_Open ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();

			try {
				SqlCommand cmd = new SqlCommand ("select @@version", conn, trans);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.AreEqual (IsolationLevel.ReadCommitted, trans.IsolationLevel);
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
			}
		}

		[Test]
		public void IsolationLevel_Transaction_Committed ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();
			trans.Commit ();

			try {
				IsolationLevel iso = trans.IsolationLevel;
				Assert.Fail ("#1:" + iso);
			} catch (InvalidOperationException ex) {
				// This SqlTransaction has completed; it is no
				// longer usable
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void IsolationLevel_Transaction_Disposed ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();
			trans.Dispose ();

			try {
				IsolationLevel iso = trans.IsolationLevel;
				Assert.Fail ("#1:" + iso);
			} catch (InvalidOperationException ex) {
				// This SqlTransaction has completed; it is no
				// longer usable
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void IsolationLevel_Transaction_Open ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();
			Assert.AreEqual (IsolationLevel.ReadCommitted, trans.IsolationLevel);
		}

		[Test]
		public void IsolationLevel_Transaction_Rolledback ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();
			trans.Rollback ();

			try {
				IsolationLevel iso = trans.IsolationLevel;
				Assert.Fail ("#A1:" + iso);
			} catch (InvalidOperationException ex) {
				// This SqlTransaction has completed; it is no
				// longer usable
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			} finally {
				trans.Dispose ();
			}

			trans = conn.BeginTransaction ();
			trans.Save ("SAVE1");
			trans.Rollback ("SAVE1");

			Assert.AreEqual (IsolationLevel.ReadCommitted, trans.IsolationLevel, "#B1");
		}

		[Test]
		public void IsolationLevel_Transaction_Saved ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();
			trans.Save ("SAVE1");
			Assert.AreEqual (IsolationLevel.ReadCommitted, trans.IsolationLevel);
		}

		[Test] // Rollback ()
		public void Rollback1 ()
		{
			string sql;
			SqlCommand cmd = null;
			SqlConnection connA = null;
			SqlConnection connB = null;

			try {
				connA = new SqlConnection (connectionString);
				connA.Open ();

				connB = new SqlConnection (connectionString);
				connB.Open ();

				using (trans = connA.BeginTransaction ()) {
					sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					cmd = new SqlCommand (sql, connA, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", connA, trans);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#A1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#A2");
						Assert.IsFalse (reader.Read (), "#A3");
					}

					trans.Rollback ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", connA);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#B1");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", connB);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#C1");
					}
				}
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (connA != null)
					connA.Close ();
				if (connB != null)
					connB.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE1");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6667, 'BangaloreNovell', '1999-03-10', '2006-08-23')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE2");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6668, 'Novell', '1997-04-07', '2003-06-25')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Rollback ();
				conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#D1");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6667", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#E1");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6668", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#F1");
				}
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();
				trans.Rollback ();
			} finally {
				if (trans != null)
					trans.Dispose ();
			}
		}

		[Test] // Rollback ()
		public void Rollback1_Connection_Closed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				SqlCommand cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				conn.Close ();

				try {
					trans.Rollback ();
					Assert.Fail ("#A1");
				} catch (InvalidOperationException ex) {
					// This SqlTransaction has completed; it is no
					// longer usable
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#B1");
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test] // Rollback ()
		public void Rollback1_Reader_Open ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			SqlCommand cmd;

			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();

			try {
				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = new SqlCommand ("select @@version", conn, trans);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					try {
						trans.Rollback ();
						Assert.Fail ("#1");
					} catch (InvalidOperationException ex) {
						// There is already an open DataReader
						// associated with this Command which
						// must be closed first
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
					}
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
			}
		}

		[Test] // Rollback ()
		public void Rollback1_Transaction_Committed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Commit ();

					try {
						trans.Rollback ();
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#B1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#B2");
						Assert.IsFalse (reader.Read (), "#B3");
					}

					conn.Close ();
					conn = new SqlConnection (connectionString);
					conn.Open ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#C1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#C2");
						Assert.IsFalse (reader.Read (), "#C3");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test] // Rollback ()
		public void Rollback1_Transaction_Disposed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Dispose ();
					trans.Rollback ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#B1");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test] // Rollback ()
		public void Rollback1_Transaction_Rolledback ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Rollback ();

					try {
						trans.Rollback ();
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#B1");
					}
				}

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Save ("SAVE1");

					sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6667, 'BangaloreNovell', '1999-03-10', '2006-08-23')";
					cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Rollback ("SAVE1");

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn, trans);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#C1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#C2");
						Assert.IsFalse (reader.Read (), "#C3");
					}

					trans.Rollback ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#D1");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test] // Rollback (String)
		public void Rollback2 ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			string sql;
			SqlCommand cmd = null;

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE1");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6667, 'BangaloreNovell', '1999-03-10', '2006-08-23')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE2");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6668, 'Novell', '1997-04-07', '2003-06-25')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE3");

				trans.Rollback ("SAVE1");
				trans.Commit ();
				conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsTrue (reader.Read (), "#A1");
					Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#A2");
					Assert.IsFalse (reader.Read (), "#A3");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6667", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#B1");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6668", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#C1");
				}
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE1");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6667, 'BangaloreNovell', '1999-03-10', '2006-08-23')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE2");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6668, 'Novell', '1997-04-07', '2003-06-25')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE1");

				trans.Rollback ("SAVE1");
				trans.Commit ();
				conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsTrue (reader.Read (), "#D1");
					Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#D2");
					Assert.IsFalse (reader.Read (), "#D3");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6667", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsTrue (reader.Read (), "#E1");
					Assert.AreEqual ("BangaloreNovell", reader.GetString (0), "#E2");
					Assert.IsFalse (reader.Read (), "#E3");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6668", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsTrue (reader.Read (), "#F1");
					Assert.AreEqual ("Novell", reader.GetString (0), "#F2");
					Assert.IsFalse (reader.Read (), "#F3");
				}
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test] // Rollback (String)
		public void Rollback2_Connection_Closed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				SqlCommand cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				conn.Close ();

				try {
					trans.Rollback ("SAVE1");
					Assert.Fail ("#A1");
				} catch (InvalidOperationException ex) {
					// This SqlTransaction has completed; it is no
					// longer usable
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#B");
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test] // Rollback (String)
		public void Rollback2_Reader_Open ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			SqlCommand cmd;

			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();

			try {
				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE1");

				cmd = new SqlCommand ("select @@version", conn, trans);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					try {
						trans.Rollback ("SAVE1");
						Assert.Fail ("#1");
					} catch (InvalidOperationException ex) {
						// There is already an open DataReader
						// associated with this Command which
						// must be closed first
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
					}
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
			}
		}

		[Test] // Rollback (String)
		public void Rollback2_Transaction_Committed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Commit ();

					try {
						trans.Rollback ("SAVE1");
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#B1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#B2");
						Assert.IsFalse (reader.Read (), "#B3");
					}

					conn.Close ();
					conn = new SqlConnection (connectionString);
					conn.Open ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#C1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#C2");
						Assert.IsFalse (reader.Read (), "#C3");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test] // Rollback (String)
		public void Rollback2_Transaction_Disposed ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Dispose ();

					try {
						trans.Rollback ("SAVE1");
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#B1");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test] // Rollback (String)
		public void Rollback2_Transaction_Rolledback ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Rollback ();

					try {
						trans.Rollback ("SAVE1");
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#B1");
					}
				}

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Save ("SAVE1");
					trans.Rollback ("SAVE1");

					try {
						trans.Rollback ("SAVE1");
						Assert.Fail ("#C1");
					} catch (SqlException ex) {
						// Cannot roll back SAVE1. No transaction
						// or savepoint of that name was found
						Assert.AreEqual (typeof (SqlException), ex.GetType (), "#C2");
						Assert.AreEqual ((byte) 16, ex.Class, "#C3");
						Assert.IsNull (ex.InnerException, "#C4");
						Assert.IsNotNull (ex.Message, "#C5");
						Assert.IsTrue (ex.Message.IndexOf ("SAVE1") != -1, "#C6");
						Assert.AreEqual (6401, ex.Number, "#C7");
						Assert.AreEqual ((byte) 1, ex.State, "#C8");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn, trans);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#D1");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test] // Rollback (String)
		public void Rollback2_TransactionName_DoesNotExist ()
		{
			conn = new SqlConnection (connectionString);
			conn.Open ();

			using (trans = conn.BeginTransaction ()) {
				try {
					trans.Rollback ("SAVE1");
					Assert.Fail ("#1");
				} catch (SqlException ex) {
					// Cannot roll back SAVE1. No transaction
					// or savepoint of that name was found
					Assert.AreEqual (typeof (SqlException), ex.GetType (), "#2");
					Assert.AreEqual ((byte) 16, ex.Class, "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
					Assert.IsTrue (ex.Message.IndexOf ("SAVE1") != -1, "#6");
					Assert.AreEqual (6401, ex.Number, "#7");
					if (ClientVersion == 7)
						Assert.AreEqual ((byte) 2, ex.State, "#8");
					else
						Assert.AreEqual ((byte) 1, ex.State, "#8");
				}

				trans.Commit ();
			}
		}

		[Test] // Rollback (String)
		public void Rollback2_TransactionName_Empty ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			conn = new SqlConnection (connectionString);
			conn.Open ();

			using (trans = conn.BeginTransaction ()) {
				try {
					trans.Rollback (string.Empty);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// Invalid transaction or invalid name
					// for a point at which to save within
					// the transaction
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}

				trans.Commit ();
			}
		}

		[Test] // Rollback (String)
		public void Rollback2_TransactionName_Null ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			conn = new SqlConnection (connectionString);
			conn.Open ();

			using (trans = conn.BeginTransaction ()) {
				try {
					trans.Rollback ((string) null);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// Invalid transaction or invalid name
					// for a point at which to save within
					// the transaction
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}

				trans.Commit ();
			}
		}

		[Test]
		public void Save_Connection_Closed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				SqlCommand cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				conn.Close ();

				try {
					trans.Save ("SAVE1");
					Assert.Fail ("#A1");
				} catch (InvalidOperationException ex) {
					// This SqlTransaction has completed; it is no
					// longer usable
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#B1");
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Save ()
		{
			string sql;
			SqlCommand cmd = null;

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE1");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6667, 'BangaloreNovell', '1999-03-10', '2006-08-23')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE2");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6668, 'Novell', '1997-04-07', '2003-06-25')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#A1");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6667", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#B1");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6668", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#C1");
				}
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE1");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6667, 'BangaloreNovell', '1999-03-10', '2006-08-23')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Save ("SAVE2");

				sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6668, 'Novell', '1997-04-07', '2003-06-25')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				trans.Commit ();

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsTrue (reader.Read (), "#D1");
					Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#D2");
					Assert.IsFalse (reader.Read (), "#D3");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6667", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsTrue (reader.Read (), "#E1");
					Assert.AreEqual ("BangaloreNovell", reader.GetString (0), "#E2");
					Assert.IsFalse (reader.Read (), "#E3");
				}

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6668", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsTrue (reader.Read (), "#F1");
					Assert.AreEqual ("Novell", reader.GetString (0), "#F2");
					Assert.IsFalse (reader.Read (), "#F3");
				}
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Save_Reader_Open ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			SqlCommand cmd;

			conn = new SqlConnection (connectionString);
			conn.Open ();

			trans = conn.BeginTransaction ();

			try {
				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = new SqlCommand ("select @@version", conn, trans);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					try {
						trans.Save ("SAVE1");
						Assert.Fail ("#1");
					} catch (InvalidOperationException ex) {
						// There is already an open DataReader
						// associated with this Command which
						// must be closed first
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
						Assert.IsNull (ex.InnerException, "#3");
						Assert.IsNotNull (ex.Message, "#4");
					}
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
			}
		}

		[Test]
		public void Save_Transaction_Committed ()
		{
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Commit ();

					try {
						trans.Save ("SAVE1");
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#B1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#B2");
						Assert.IsFalse (reader.Read (), "#B3");
					}

					conn.Close ();
					conn = new SqlConnection (connectionString);
					conn.Open ();

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#C1");
						Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#C2");
						Assert.IsFalse (reader.Read (), "#C3");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Save_Transaction_Disposed ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Dispose ();

					try {
						trans.Save ("SAVE1");
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#B1");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Save_Transaction_Rolledback ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Rollback ();

					try {
						trans.Save ("SAVE1");
						Assert.Fail ("#A1");
					} catch (InvalidOperationException ex) {
						// This SqlTransaction has completed; it is no
						// longer usable
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
						Assert.IsNull (ex.InnerException, "#A3");
						Assert.IsNotNull (ex.Message, "#A4");
					}

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsFalse (reader.Read (), "#B1");
					}
				}

				using (trans = conn.BeginTransaction ()) {
					string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand cmd = new SqlCommand (sql, conn, trans);
					cmd.ExecuteNonQuery ();
					cmd.Dispose ();

					trans.Save ("SAVE1");
					trans.Rollback ("SAVE1");
					trans.Save ("SAVE1");

					cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn, trans);
					using (SqlDataReader reader = cmd.ExecuteReader ()) {
						Assert.IsTrue (reader.Read (), "#D1");
					}
				}
			} finally {
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		public void Save_TransactionName_Empty ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				SqlCommand cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				try {
					trans.Save (string.Empty);
					Assert.Fail ("#A1");
				} catch (ArgumentException ex) {
					// Invalid transaction or invalid name
					// for a point at which to save within
					// the transaction
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.IsNull (ex.ParamName, "#A5");
				}

				trans.Rollback ();
				conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsFalse (reader.Read (), "#B1");
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		[Ignore("Deesn't work on mono. TODO:Fix")]
		public void Save_TransactionName_Null ()
		{
			if (RunningOnMono)
				Assert.Ignore ("NotWorking");

			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();

				trans = conn.BeginTransaction ();

				string sql = "INSERT INTO employee (id, fname, dob, doj) VALUES (6666, 'NovellBangalore', '1989-02-11', '2005-07-22')";
				SqlCommand cmd = new SqlCommand (sql, conn, trans);
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				try {
					trans.Save ((string) null);
					Assert.Fail ("#A1");
				} catch (ArgumentException ex) {
					// Invalid transaction or invalid name
					// for a point at which to save within
					// the transaction
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.IsNull (ex.ParamName, "#A5");
				}

				trans.Commit ();
				conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();

				cmd = new SqlCommand ("SELECT fname FROM employee WHERE id=6666", conn);
				using (SqlDataReader reader = cmd.ExecuteReader ()) {
					Assert.IsTrue (reader.Read (), "#B1");
					Assert.AreEqual ("NovellBangalore", reader.GetString (0), "#B2");
					Assert.IsFalse (reader.Read (), "#B3");
				}
			} finally {
				if (trans != null)
					trans.Dispose ();
				if (conn != null)
					conn.Close ();

				conn = new SqlConnection (connectionString);
				conn.Open ();
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		int ClientVersion {
			get {
				return (engine.ClientVersion);
			}
		}

		static bool RunningOnMono {
			get
			{
				return (Type.GetType ("System.MonoType", false) != null);
			}
		}
	}
}
