// OdbcCommandTest.cs - NUnit Test Cases for testing the
// OdbcCommand class
//
// Authors:
//      Sureshkumar T (TSureshkumar@novell.com)
// 	Umadevi S (sumadevi@novell.com)
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
	public class OdbcCommandTest
	{
		OdbcConnection conn;
		OdbcCommand cmd;

		[SetUp]
		public void SetUp ()
		{
			conn = ConnectionManager.Instance.Odbc.Connection;
			cmd = conn.CreateCommand ();
		}

		[TearDown]
		public void TearDown ()
		{
			if (cmd != null)
				cmd.Dispose ();
			ConnectionManager.Instance.Close ();
		}

		[Test]
		public void PrepareAndExecuteTest ()
		{
			OdbcDataReader reader = null;

			try {
				string tableName = DBHelper.GetRandomName ("PAE", 3);
				try {
					// setup table
					string query = "DROP TABLE " + tableName ;
					DBHelper.ExecuteNonQuery (conn, query);
					query = String.Format ("CREATE TABLE {0} ( id INT, small_id SMALLINT )",
							       tableName);
					DBHelper.ExecuteNonQuery (conn, query);

					query = String.Format ("INSERT INTO {0} values (?, ?)", tableName);
					cmd = conn.CreateCommand ();
					cmd.CommandText = query;
					cmd.Prepare ();

					OdbcParameter param1 = cmd.Parameters.Add ("?", OdbcType.Int);
					OdbcParameter param2 = cmd.Parameters.Add ("?", OdbcType.SmallInt);
					param1.Value = 1;
					param2.Value = 5;
					cmd.ExecuteNonQuery ();

					param1.Value = 2;
					param2.Value = 6;
					cmd.ExecuteNonQuery ();

					cmd.CommandText = "select id, small_id from " + tableName + " order by id asc";
					reader = cmd.ExecuteReader ();

					Assert.IsTrue (reader.Read (), "#A1");
					Assert.AreEqual (1, reader.GetValue (0), "#A2");
					Assert.AreEqual (5, reader.GetValue (1), "#A3");
					Assert.IsTrue (reader.Read (), "#A4");
					Assert.AreEqual (2, reader.GetValue (0), "#A5");
					Assert.AreEqual (6, reader.GetValue (1), "#A6");
					Assert.IsFalse (reader.Read (), "#A7");
					reader.Close ();
					cmd.Dispose ();

					cmd = conn.CreateCommand ();
					cmd.CommandText = "select id, small_id from " + tableName + " order by id asc";

					reader = cmd.ExecuteReader ();
					Assert.IsTrue (reader.Read (), "#B1");
					Assert.AreEqual (1, reader.GetValue (0), "#B2");
					Assert.AreEqual (5, reader.GetValue (1), "#B3");
					Assert.IsTrue (reader.Read (), "#B4");
					Assert.AreEqual (2, reader.GetValue (0), "#B5");
					Assert.AreEqual (6, reader.GetValue (1), "#B6");
					Assert.IsFalse (reader.Read (), "#B7");
				} finally {
					DBHelper.ExecuteNonQuery (conn, "DROP TABLE " + tableName);
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		/// <summary>
		/// Test String parameters to ODBC Command
		/// </summary>
		[Test]
		public void ExecuteStringParameterTest()
		{
			cmd.CommandText = "select count(*) from employee where fname=?;";
			string colvalue = "suresh";
			OdbcParameter param = cmd.Parameters.Add("@un", OdbcType.VarChar);
			param.Value = colvalue;
			int count =  Convert.ToInt32 (cmd.ExecuteScalar ());
			Assert.AreEqual (1, count, "#1 String parameter not passed correctly");
		}

		/// <summary>
		/// Test ExecuteNonQuery
		/// </summary>
		[Test]
		public void ExecuteNonQueryTest ()
		{
			int ret;

			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "select count(*) from employee where id <= ?;";
			cmd.Parameters.Add ("@un", OdbcType.Int).Value = 3;
			ret = cmd.ExecuteNonQuery ();
			switch (ConnectionManager.Instance.Odbc.EngineConfig.Type) {
			case EngineType.SQLServer:
				Assert.AreEqual (-1, ret, "#1");
				break;
			case EngineType.MySQL:
				Assert.AreEqual (1, ret, "#1");
				break;
			default:
				Assert.Fail ("Engine type not supported.");
				break;
			}

			cmd = conn.CreateCommand ();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "select * from employee where id <= ?;";
			cmd.Parameters.Add ("@un", OdbcType.Int).Value = 3;
			ret = cmd.ExecuteNonQuery ();
			switch (ConnectionManager.Instance.Odbc.EngineConfig.Type) {
			case EngineType.SQLServer:
				Assert.AreEqual (-1, ret, "#2");
				break;
			case EngineType.MySQL:
				Assert.AreEqual (3, ret, "#2");
				break;
			default:
				Assert.Fail ("Engine type not supported.");
				break;
			}

			cmd = conn.CreateCommand ();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "select * from employee where id <= 3;";
			ret = cmd.ExecuteNonQuery ();
			switch (ConnectionManager.Instance.Odbc.EngineConfig.Type) {
			case EngineType.SQLServer:
				Assert.AreEqual (-1, ret, "#3");
				break;
			case EngineType.MySQL:
				Assert.AreEqual (3, ret, "#3");
				break;
			default:
				Assert.Fail ("Engine type not supported.");
				break;
			}

			try {
				// insert
				cmd = conn.CreateCommand ();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "insert into employee (id, fname, dob, doj) values " +
					" (6001, 'tttt', '1999-01-22', '2005-02-11');";
				ret = cmd.ExecuteNonQuery ();
				Assert.AreEqual (1, ret, "#4");

				cmd = conn.CreateCommand ();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "insert into employee (id, fname, dob, doj) values " +
					" (?, 'tttt', '1999-01-22', '2005-02-11');";
				cmd.Parameters.Add (new OdbcParameter ("id", OdbcType.Int));
				cmd.Parameters [0].Value = 6002;
				cmd.Prepare ();
				ret = cmd.ExecuteNonQuery ();
				Assert.AreEqual (1, ret, "#5");
			} finally {
				// delete
				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "delete from employee where id > 6000";
				ret = cmd.ExecuteNonQuery ();
				Assert.AreEqual (2, ret, "#6");
			}
		}

		[Test]
		public void ExecuteNonQuery_Query_Invalid ()
		{
			cmd.CommandText = "select id1 from numeric_family"; ;
			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#A1");
			} catch (OdbcException ex) {
				// Invalid column name 'id1'
				Assert.AreEqual (typeof (OdbcException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A5");
			}

			// ensure connection is not closed after error

			int result;

			cmd.CommandText = "INSERT INTO numeric_family (id, type_int) VALUES (6100, 200)";
			result = cmd.ExecuteNonQuery ();
			Assert.AreEqual (1, result, "#B1");

			cmd.CommandText = "DELETE FROM numeric_family WHERE id = 6100";
			result = cmd.ExecuteNonQuery ();
			Assert.AreEqual (1, result, "#B1");
		}

		[Test]
		public void Dispose ()
		{
			OdbcTransaction trans = null;

			try {
				trans = conn.BeginTransaction ();

				cmd.CommandText = "SELECT 'a'";
				cmd.CommandTimeout = 67;
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.DesignTimeVisible = false;
				cmd.Parameters.Add (new OdbcParameter ());
				cmd.Transaction = trans;
				cmd.UpdatedRowSource = UpdateRowSource.OutputParameters;

				cmd.Dispose ();

				Assert.AreEqual (string.Empty, cmd.CommandText, "#1");
				Assert.AreEqual (67, cmd.CommandTimeout, "#2");
				Assert.AreEqual (CommandType.StoredProcedure, cmd.CommandType, "#3");
				Assert.IsNull (cmd.Connection, "#4");
				Assert.IsFalse (cmd.DesignTimeVisible, "#5");
				Assert.IsNotNull (cmd.Parameters, "#6");
				Assert.AreEqual (0, cmd.Parameters.Count, "#7");
				Assert.IsNull (cmd.Transaction, "#8");
				Assert.AreEqual (UpdateRowSource.OutputParameters, cmd.UpdatedRowSource, "#9");
			} finally {
				if (trans != null)
					trans.Rollback ();
			}
		}

		[Test] // bug #341743
		public void Dispose_Connection_Disposed ()
		{
			cmd.CommandText = "SELECT 'a'";
			cmd.ExecuteNonQuery ();

			conn.Dispose ();

			Assert.AreSame (conn, cmd.Connection, "#1");
			cmd.Dispose ();
			Assert.IsNull (cmd.Connection, "#2");
		}
	}
}

#endif