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
using System.Data;
using System.IO;
using Mono.Data.Sqlite;

using NUnit.Framework;

namespace MonoTests.Mono.Data.Sqlite
{
        [TestFixture]
        public class SqliteDataReaderTest
        {
                readonly static string _uri = "./test.db";
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
        }
}
