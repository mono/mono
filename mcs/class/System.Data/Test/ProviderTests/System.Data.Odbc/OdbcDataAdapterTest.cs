// OdbcDataAdapterTest.cs - NUnit Test Cases for testing the
//                          OdbcDataAdapter class
// Author:
//      Sureshkumar T (TSureshkumar@novell.com)
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
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Connected.Odbc
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcDataAdapterTest
	{
		[Test]
		public void FillTest ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try
			{
				// For this Test, you must create sample table
				// called person-age, with a non-zero number of rows
				// and non-zero number of columns
				// run the test initialization script mono_test_mysql.sql
				string tableName = "employee";
				string sql= "select * from " + tableName; 
				OdbcDataAdapter da = new OdbcDataAdapter (sql, (OdbcConnection) conn);
				DataSet ds = new DataSet (tableName);
				da.Fill (ds, tableName);
				Assert.AreEqual (true, 
						 ds.Tables.Count > 0,
						 "#1 Table count must not be zero");
				Assert.AreEqual (true, 
						 ds.Tables [0].Rows.Count > 0,
						 "#2 Row count must not be zero");
				foreach (DataColumn dc in ds.Tables [0].Columns)
					Assert.AreEqual (true,
							 dc.ColumnName.Length > 0,
							 "#3 DataSet column names must noot be of size 0");
				foreach (DataRow dr in ds.Tables [0].Rows) {
					foreach (DataColumn dc in ds.Tables [0].Columns) 
						Assert.AreEqual (true,
								 dc.ColumnName.Length > 0,
								 "#4 column values must not be of size 0");
				}
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		[Ignore]
		public void InsertUtf8Test ()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try
			{
				DoExecuteNonQuery ((OdbcConnection) conn,
						   "CREATE TABLE odbc_ins_utf8_test(ival int not null, sval varchar(20))");
				Assert.AreEqual (DoExecuteNonQuery ((OdbcConnection) conn,
								    "INSERT INTO odbc_ins_utf8_test(ival, sval) VALUES (1, 'English')"),
						 1);
				Assert.AreEqual (DoExecuteNonQuery ((OdbcConnection) conn,
								    "INSERT INTO odbc_ins_utf8_test(ival, sval) VALUES (2, 'Français')"),
						 1);
				Assert.AreEqual (DoExecuteNonQuery ((OdbcConnection) conn,
								    "INSERT INTO odbc_ins_utf8_test(ival, sval) VALUES (3, 'Español')"),
						 1);
				Assert.AreEqual (DoExecuteScalar   ((OdbcConnection) conn,
								    "SELECT COUNT(*) FROM odbc_ins_utf8_test WHERE sval " +
								    "IN('English', 'Français', 'Español')"),
						 3);
			} finally {
				DoExecuteNonQuery ((OdbcConnection) conn, "DROP TABLE odbc_ins_utf8_test");
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		private int DoExecuteNonQuery (OdbcConnection conn, string sql)
		{
			OdbcCommand cmd = new OdbcCommand (sql, conn);
			return cmd.ExecuteNonQuery ();
		}

		private int DoExecuteScalar (OdbcConnection conn, string sql)
		{
			OdbcCommand cmd = new OdbcCommand (sql, conn);
			object value = cmd.ExecuteScalar ();
			return (int) Convert.ChangeType (value, typeof (int));
		}
	}
}

#endif