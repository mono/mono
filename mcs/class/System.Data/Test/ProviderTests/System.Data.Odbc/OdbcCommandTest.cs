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
	public class OdbcCommandTest
	{
		[Test]
		public void PrepareAndExecuteTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				
				string tableName = DBHelper.GetRandomName ("PAE", 3);
				try {
					// setup table
					string query = "DROP TABLE " + tableName ;
					DBHelper.ExecuteNonQuery (conn, query);
					query = String.Format ("CREATE TABLE {0} ( id INT, small_id SMALLINT )",
							       tableName);
					DBHelper.ExecuteNonQuery (conn, query);

					query = String.Format ("INSERT INTO {0} values (?, ?)", tableName);
					OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
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
					
					cmd.CommandText = "select * from " + tableName;
					cmd.Parameters.Clear ();
					
					OdbcDataReader reader = cmd.ExecuteReader ();
					int count = 0;
					while (reader.Read ()){
						count++;
					}
					reader.Close ();
					Assert.AreEqual (2, count, "#1");
				} finally {
					DBHelper.ExecuteNonQuery (conn, "DROP TABLE " + tableName);
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		/// <summary>
                /// Test String parameters to ODBC Command
                /// </summary>
                [Test]
                public void ExecuteStringParameterTest()
                {
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand dbcmd = (OdbcCommand) conn.CreateCommand ();
				dbcmd.CommandType = CommandType.Text;
				dbcmd.CommandText = "select count(*) from employee where fname=?;";
				string colvalue = "suresh";
				OdbcParameter param = dbcmd.Parameters.Add("@un", OdbcType.VarChar);
				param.Value = colvalue;
				int count =  Convert.ToInt32 (dbcmd.ExecuteScalar ());
				Assert.AreEqual (1, count, "#1 String parameter not passed correctly");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void bug341743 ()
		{
			OdbcConnection conn = (OdbcConnection) ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			OdbcCommand cmd = null;
			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT 'a'";
				cmd.ExecuteNonQuery ();

				conn.Dispose ();

				Assert.AreSame (conn, cmd.Connection, "#1");
				cmd.Dispose ();
				Assert.IsNull (cmd.Connection, "#2");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		/// <summary>
                /// Test ExecuteNonQuery
                /// </summary>
                [Test]
                public void ExecuteNonQueryTest ()
                {
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand dbcmd = (OdbcCommand) conn.CreateCommand ();
				dbcmd.CommandType = CommandType.Text;
				dbcmd.CommandText = "select count(*) from employee where id <= ?;";
				int value = 3;
				dbcmd.Parameters.Add("@un", OdbcType.Int).Value = value;
				int ret = dbcmd.ExecuteNonQuery();
				Assert.AreEqual (-1, ret,  "#1 ExecuteNonQuery not working");

				try {
					// insert
					dbcmd = (OdbcCommand) conn.CreateCommand ();
					dbcmd.CommandType = CommandType.Text;
					dbcmd.CommandText = "insert into employee (id, fname, dob, doj) values " +
						" (6001, 'tttt', '1999-01-22', '2005-02-11');";
					ret = dbcmd.ExecuteNonQuery();
					Assert.AreEqual (1, ret, "#2 ExecuteNonQuery not working");
				} finally {
					// delete
					dbcmd = (OdbcCommand) conn.CreateCommand ();
					dbcmd.CommandType = CommandType.Text;
					dbcmd.CommandText = "delete from employee where id > 6000";
					ret = dbcmd.ExecuteNonQuery();
					Assert.AreEqual (true, ret > 0, "#3 ExecuteNonQuery for deletion not working");
				}
				
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

	}
}
