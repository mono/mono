// SqliteDataReaderTest.cs - NUnit Test Cases for SqliteDataReader
//
// Authors:
//   Sureshkumar T <tsureshkumar@novell.com>
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Mono.Data.Sqlite;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.Mono.Data.Sqlite
{
	[TestFixture]
	public class SqliteDataReaderTest
	{
		readonly static string _uri = TestResourceHelper.GetFullPathOfResource("Test/test.db");
		readonly static string _connectionString = "URI=file://" + _uri + ", version=3";
		SqliteConnection _conn = new SqliteConnection ();

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			if (! File.Exists (_uri) || new FileInfo (_uri).Length == 0) {
				// ignore all tests
				Assert.Ignore ("#000 ignoring all fixtures. No database present");
			}
		}

		[Test]
		public void EmptyStatementThrowsAgrumentException ()
		{
			_conn.ConnectionString = _connectionString;
			SqliteDataReader reader = null;
			using (_conn) {
				_conn.Open ();
				SqliteCommand cmd = (SqliteCommand) _conn.CreateCommand ();
				cmd.CommandText = string.Empty;
				try {
					reader = cmd.ExecuteReader ();
					Assert.Fail ();
				} catch (ArgumentException ex){
					// expected this one
				} catch (Exception ex) {
					Assert.Fail ();
				}
			}
		}

		[Test]
		public void NullStatementThrowsAgrumentException ()
		{
			_conn.ConnectionString = _connectionString;
			SqliteDataReader reader = null;
			using (_conn) {
				_conn.Open ();
				SqliteCommand cmd = (SqliteCommand) _conn.CreateCommand ();
				cmd.CommandText = null;
				try {
					reader = cmd.ExecuteReader ();
					Assert.Fail ();
				} catch (ArgumentException ex){
					// expected this one
				} catch (Exception ex) {
					Assert.Fail ();
				}
			}
		}

		[Test]
		public void WhitespaceOnlyStatementThrowsAgrumentException ()
		{
			_conn.ConnectionString = _connectionString;
			SqliteDataReader reader = null;
			using (_conn) {
				_conn.Open ();
				SqliteCommand cmd = (SqliteCommand) _conn.CreateCommand ();
				cmd.CommandText = "   ";
				try {
					reader = cmd.ExecuteReader ();
					Assert.Fail ();
				} catch (ArgumentException ex){
					// expected this one
				} catch (Exception ex) {
					Assert.Fail ();
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void GetSchemaTableTest ()
		{
			_conn.ConnectionString = _connectionString;
			SqliteDataReader reader = null;
			using (_conn) {
				_conn.Open ();
				SqliteCommand cmd = (SqliteCommand) _conn.CreateCommand ();
				cmd.CommandText = "select * from test";
				reader = cmd.ExecuteReader ();
				try {
					DataTable dt = reader.GetSchemaTable ();
					Assert.IsNotNull (dt, "#GS1 should return valid table");
					Assert.IsTrue (dt.Rows.Count > 0, "#GS2 should return with rows ;-)");
				} finally {
					if (reader != null && !reader.IsClosed)
						reader.Close ();
					_conn.Close ();
				}
			}
		}

		[Test]
		public void TypeOfNullInResultTest ()
		{
			_conn.ConnectionString = _connectionString;
			SqliteDataReader reader = null;
			using (_conn) {
				_conn.Open ();
				SqliteCommand cmd = (SqliteCommand) _conn.CreateCommand ();
				cmd.CommandText = "select null from test";
				reader = cmd.ExecuteReader ();
				try {
					Assert.IsTrue (reader.Read());
					Assert.IsNotNull (reader.GetFieldType (0));
				} finally {
					if (reader != null && !reader.IsClosed)
						reader.Close ();
					_conn.Close ();
				}
			}
		}
		
		[Test]
		public void TimestampTest ()
		{
			_conn.ConnectionString = _connectionString;
			using (_conn) {
				_conn.Open ();
				var cmd = (SqliteCommand) _conn.CreateCommand ();
				cmd.CommandText = @"CREATE TABLE IF NOT EXISTS TestNullableDateTime (nullable TIMESTAMP NULL, dummy int); INSERT INTO TestNullableDateTime (nullable, dummy) VALUES (124123, 2);";
				cmd.ExecuteNonQuery ();
			
				var query = "SELECT * FROM TestNullableDateTime;";
				cmd = (SqliteCommand) _conn.CreateCommand ();
				cmd.CommandText = query;
				cmd.CommandType = CommandType.Text;
			
				using (var reader = cmd.ExecuteReader ()) {
					try {
						var dt = reader ["nullable"];
						Assert.Fail ("Expected: FormatException");
					} catch (FormatException ex) {
						// expected this one
					}
				}
			}
			
			_conn.ConnectionString = _connectionString + ",DateTimeFormat=UnixEpoch";
			using (_conn) {
				_conn.Open ();
				var cmd = (SqliteCommand) _conn.CreateCommand ();
				cmd.CommandText = @"CREATE TABLE IF NOT EXISTS TestNullableDateTime (nullable TIMESTAMP NULL, dummy int); INSERT INTO TestNullableDateTime (nullable, dummy) VALUES (124123, 2);";
				cmd.ExecuteNonQuery ();
			
				var query = "SELECT * FROM TestNullableDateTime;";
				cmd = (SqliteCommand) _conn.CreateCommand ();
				cmd.CommandText = query;
				cmd.CommandType = CommandType.Text;
			
				using (var reader = cmd.ExecuteReader ()) {
					// this should succeed now
					var dt = reader ["nullable"];
				}
			}
		}
		
		[Test]
		public void CloseConnectionTest ()
		{
			// When this test fails it may confuse nunit a bit, causing it to show strange
			// exceptions, since it leaks file handles (and nunit tries to open files,
			// which it doesn't expect to fail).
			
			// For the same reason a lot of other tests will fail when this one fails.
			
			_conn.ConnectionString = _connectionString;
			using (_conn) {
				_conn.Open ();
				using (var cmd = (SqliteCommand) _conn.CreateCommand ()) {
					cmd.CommandText = @"CREATE TABLE IF NOT EXISTS TestNullableDateTime (nullable TIMESTAMP NULL, dummy int); INSERT INTO TestNullableDateTime (nullable, dummy) VALUES (124123, 2);";
					cmd.ExecuteNonQuery ();
				}
			}

			for (int i = 0; i < 1000; i++) {
				_conn.ConnectionString = _connectionString;
				using (_conn) {
					_conn.Open ();
					using (var cmd = (SqliteCommand) _conn.CreateCommand ()) {
						cmd.CommandText = "SELECT * FROM TestNullableDateTime;";
						cmd.CommandType = CommandType.Text;
					
						using (var reader = cmd.ExecuteReader (CommandBehavior.CloseConnection)) {
							reader.Read ();
						}
					}
				}
			}
		}

		void AddParameter (global::System.Data.Common.DbCommand cm, string name, object value)
		{
			var param = cm.CreateParameter ();
			param.ParameterName = ":" + name;
			param.Value = value;
			cm.Parameters.Add (param);
		}

		class D
		{
			public string Sql;
			public Type Expected;
			public object Value;
		}

		[Test]
		public void TestDataTypes ()
		{
			SqliteParameter param;
			
			var data = new List<D> ()
			{
				new D () { Sql = "DATETIME",            Expected = typeof (DateTime),   Value = DateTime.Now              },
				new D () { Sql = "GUIDBLOB NOT NULL",   Expected = typeof (Guid),       Value = new byte [] { 3, 14, 15 } },
				new D () { Sql = "BOOLEAN",             Expected = typeof (bool),       Value = true                      },
				new D () { Sql = "INT32",               Expected = typeof (long),       Value = 1                         },
				new D () { Sql = "INT32 NOT NULL",      Expected = typeof (long),       Value = 2                         },
				new D () { Sql = "UINT1",               Expected = typeof (long),       Value = 3                         },
				
				// these map to the INTEGER affinity
				new D () { Sql = "INT",                 Expected = typeof (int),       Value = 4  },
				new D () { Sql = "INTEGER",             Expected = typeof (long),      Value = 5 },
				new D () { Sql = "TINYINT",             Expected = typeof (byte),      Value = 6 },
				new D () { Sql = "SMALLINT",            Expected = typeof (short),     Value = 7 },
				new D () { Sql = "MEDIUMINT",           Expected = typeof (long),      Value = 8 },
				new D () { Sql = "BIGINT",              Expected = typeof (long),      Value = 9 },
				new D () { Sql = "UNSIGNED BIG INT",    Expected = typeof (long),      Value = 0 },
				new D () { Sql = "INT2",                Expected = typeof (long),      Value = 1 },
				new D () { Sql = "INT4",                Expected = typeof (long),      Value = 2 },
				
				// these map to the TEXT affinity
				new D () { Sql = "CHARACTER(20)",          Expected = typeof (string),       Value = "a" },
				new D () { Sql = "VARCHAR(255)",           Expected = typeof (string),       Value = "b" },
				new D () { Sql = "VARYING CHARACTER(255)", Expected = typeof (string),       Value = "c" },
				new D () { Sql = "NCHAR(55)",              Expected = typeof (string),       Value = "d" },
				new D () { Sql = "NATIVE CHARACTER(70)",   Expected = typeof (string),       Value = "e" },
				new D () { Sql = "NVARCHAR(100)",          Expected = typeof (string),       Value = "f" },
				new D () { Sql = "TEXT",                   Expected = typeof (string),       Value = "g" },
				new D () { Sql = "CLOB",                   Expected = typeof (string),       Value = "h" },
				
				// these map to the NONE affinity
				new D () { Sql = "BLOB",           Expected = typeof (byte[]),       Value = new byte [] { 3, 14, 15 } },
				new D () { Sql = "",               Expected = typeof (object),       Value = null                      },
				
				// these map to the REAL affinity
				new D () { Sql = "REAL",               Expected = typeof (float),        Value = 3.2 },
				new D () { Sql = "DOUBLE",             Expected = typeof (double),       Value = 4.2 },
				new D () { Sql = "DOUBLE PRECISION",   Expected = typeof (double),       Value = 5.2 },
				new D () { Sql = "FLOAT",              Expected = typeof (double),       Value = 6.2 },
				
				// these map to the NUMERIC affinity
				new D () { Sql = "NUMERIC",            Expected = typeof (decimal),        Value = 3.2          },
				new D () { Sql = "DECIMAL(10,5)",      Expected = typeof (decimal),        Value = null         },
				new D () { Sql = "BOOLEAN",            Expected = typeof (bool),           Value = true         },
				new D () { Sql = "DATE",               Expected = typeof (DateTime),       Value = DateTime.Now },
				new D () { Sql = "DATETIME",           Expected = typeof (DateTime),       Value = DateTime.Now },
				
				
			};
			
			_conn.ConnectionString = _connectionString;
			using (_conn) {
				_conn.Open();
				
				using (var cm = _conn.CreateCommand ()) {
					var sql = new StringBuilder ();
					var args = new StringBuilder ();
					var vals = new StringBuilder ();
					sql.AppendLine ("DROP TABLE TEST;");
					sql.Append ("CREATE TABLE TEST (");
					
					bool comma = false;
					for (int i = 0; i < data.Count; i++) {
						if (comma) {
							sql.Append (", ");
							args.Append (", ");
							vals.Append (", ");
						}
						comma = true;
						sql.AppendFormat ("F{0} {1}", i, data [i].Sql);
						args.AppendFormat ("F{0}", i);
						vals.AppendFormat (":F{0}", i);
						AddParameter (cm, "F" + i.ToString (), data [i].Value);
					}
					sql.AppendLine (");");
					sql.Append ("INSERT INTO TEST (");
					sql.Append (args.ToString ());
					sql.Append (") VALUES (");
					sql.Append (vals.ToString ());
					sql.Append (");");
		
					cm.CommandText = sql.ToString ();

					cm.ExecuteNonQuery ();
				}
				
				using (var cm = _conn.CreateCommand ()) {
					cm.CommandText = "SELECT * FROM TEST";
					using (var dr = cm.ExecuteReader ()) {
						dr.Read ();
						
						for (int i = 0; i < data.Count; i++) {
							string tn = data [i].Sql.Replace (" NOT NULL", "");
							int index = dr.GetOrdinal ("F" + i.ToString ());
							Assert.AreEqual (tn, dr.GetDataTypeName (index), "F" + i.ToString () + " (" + data [i].Sql + ")");
							Assert.AreEqual (data [i].Expected.FullName, dr.GetFieldType (index).ToString (), "F" + i.ToString () + " (" + data [i].Sql + ")");
						}
					}
				}
			}
		}
	}
}
