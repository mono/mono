// DataReaderTest.cs - NUnit Test Cases for testing the
// DataReader family of classes
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
using Mono.Data;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	[Category ("odbc"), Category ("sqlserver")]
	public class DataReaderTest
	{
		[Test]
		public void GetSchemaTableTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "select id, fname, id + 20 as plustwenty from employee";
				IDataReader reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
				DataTable schema = reader.GetSchemaTable ();
				reader.Close ();
				Assert.AreEqual (3, schema.Rows.Count, "#1");

				// Primary Key Test
				DataRow pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.AreEqual (false, pkRow.IsNull ("IsKey"), "#2");
				Assert.AreEqual (true, (bool) pkRow ["IsKey"], "#3");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		
		[Test]
		public void GetNameTest () 
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand dbcmd = conn.CreateCommand ();
				string sql = "SELECT  type_tinyint from numeric_family";
				dbcmd.CommandText = sql;
				using (IDataReader reader = dbcmd.ExecuteReader ()) {
					Assert.AreEqual ("type_tinyint", reader.GetName (0), "#1 GetName failes");
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void NumericTest()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand dbCommand = conn.CreateCommand ();
				dbCommand.CommandText = "select type_numeric from numeric_family where id = 1;";
				using(IDataReader reader = dbCommand.ExecuteReader ()) {
					if (reader.Read()) {
						object value = reader.GetValue(0);
						Assert.AreEqual (typeof (decimal), value.GetType(), "#1 wrong type");
						Assert.AreEqual ( (decimal) 1000, value, "#2 value is wrong");   
					} else
						Assert.Fail ("#3 does not have test data");
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void TinyIntTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand dbCommand = conn.CreateCommand ();
				dbCommand.CommandText = "select type_tinyint from numeric_family where id = 1;";
				using(IDataReader reader = dbCommand.ExecuteReader ()) {
					if (reader.Read()) {
						object value = reader.GetValue (0);
						Assert.AreEqual (typeof (byte), value.GetType(), "#1 wrong type");
						Assert.AreEqual (255, value, "#2 value is wrong");   
					} else
						Assert.Fail ("#3 does not have test data");
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
		
		[Test]
		public void GetByteTest () 
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				string query = "select type_tinyint from numeric_family where id = 1";
				cmd.CommandText = query;
				using (IDataReader reader = cmd.ExecuteReader ()) {
					if (reader.Read ()) {
						byte b = reader.GetByte (0); 
						Assert.AreEqual (255, b, "GetByte returns wrong result");
					} else // This should not happen while testing
						Assert.Fail ("test table does not have a test data!");
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}			
		}

		[Test]
		public void GetValueBinaryTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				string sql = "select type_binary from binary_family where id = 1";
				cmd.CommandText = sql;
				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
					if (reader.Read ()) {
						object ob = reader.GetValue (0);
						Assert.AreEqual (typeof (byte []), ob.GetType (), "#1 Type of binary column is wrong!");
					} else
						Assert.Fail ("#2 test data not available");
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}      
		}
		
		[Test]
		public void GetBytesNullBufferTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				// 4th row is null
				string sql = "SELECT type_blob FROM binary_family where id = 1 or id = 4 order by id";
				cmd.CommandText = sql;
				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
					if (reader.Read ()) {
						Assert.AreEqual (8, reader.GetBytes (0, 0, null, 0, 0), 
								 "#1 getbytes should return length of column");
					} else
						Assert.Fail ("#2 No test data");
					// for null value, length in bytes should return 0
					if (reader.Read ()) 
						Assert.AreEqual (0, reader.GetBytes (0, 0, null, 0, 0), 
								 "#3 on null value, it should return -1");
					else
						Assert.Fail ("#4 No test data");
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetBytesTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				string sql = "SELECT type_blob FROM binary_family where id = 1";
				cmd.CommandText = sql;
				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
					if (reader.Read ()) {
						// Get By Parts for the column blob
						long totalsize = reader.GetBytes (0, 0, null, 0, 0);
						int buffsize = 5;
						int start = 0;
						int offset = 0;
						long ret = 0;
						long count = 0;
						byte [] val = new byte [totalsize];
						do {
							ret = reader.GetBytes (0, offset, val, offset, buffsize);
							offset += (int) ret;
							count += ret;
						} while (count < totalsize);
						Assert.AreEqual (8, count, 
								 "#1 The assembled value length does not match");
					} else
						Assert.Fail ("#2 no test data");
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetSchemaTableTest_AutoIncrement ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "create table #tmp_table (id int identity(1,1))";
				cmd.ExecuteNonQuery ();
				cmd.CommandText = "select * from #tmp_table";
				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly)) {
					DataTable schemaTable = reader.GetSchemaTable ();
					Assert.IsTrue ((bool)schemaTable.Rows [0]["IsAutoIncrement"], "#1");
					if (schemaTable.Columns.Contains ("IsIdentity"))
						Assert.IsTrue ((bool)schemaTable.Rows [0]["IsIdentity"], "#2");
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
	}
}
