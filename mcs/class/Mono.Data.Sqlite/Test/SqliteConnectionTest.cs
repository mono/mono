// SqliteConnectionTest.cs - NUnit Test Cases for SqliteConnection
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
using Mono.Data.Sqlite;

using NUnit.Framework;

namespace MonoTests.Mono.Data.Sqlite
{
        [TestFixture]
        public class SqliteConnectionTest
        {
                readonly static string _uri = "test.db";
                readonly static string _connectionString = "URI=file://" + _uri + ", version=3";
                SqliteConnection _conn = new SqliteConnection ();

#if NET_2_0
                [Test]
                [ExpectedException (typeof (ArgumentNullException))]
                public void ConnectionStringTest_Null ()
                {
                        _conn.ConnectionString = null;
                }

                [Test]
                [ExpectedException (typeof (InvalidOperationException))]
                public void ConnectionStringTest_MustBeClosed ()
                {
                        _conn.ConnectionString = _connectionString;
                        try {
                    		_conn.Open ();
                    		_conn.ConnectionString = _connectionString;
                    	} finally {
                    		_conn.Close ();
                    	}
                }

#else
                [Test]
                [ExpectedException (typeof (InvalidOperationException))]
                public void ConnectionStringTest_Empty ()
                {
                        _conn.ConnectionString = "";
                }

                [Test]
                [ExpectedException (typeof (InvalidOperationException))]
                public void ConnectionStringTest_NoURI ()
                {
                        _conn.ConnectionString = "version=3";
                }

		// In 2.0 _conn.Database always returns "main"
                [Test]
                public void ConnectionStringTest_IgnoreSpacesAndTrim ()
                {
                        _conn.ConnectionString = "URI=file://xyz      , ,,, ,, version=3";
                        Assert.AreEqual ("xyz", _conn.Database, "#1 file path is wrong");
                }
#endif
				// behavior has changed, I guess
                //[Test]
                [Ignore ("opening a connection should not create db! though, leave for now")]
                public void OpenTest ()
                {
                        try {
                                _conn.ConnectionString = _connectionString;
                                _conn.Open ();
                                Assert.AreEqual (ConnectionState.Open, _conn.State, "#1 not opened");
                                _conn.Close ();

                                // negative test: try opening a non-existent file
                                _conn.ConnectionString = "URI=file://abcdefgh.db, version=3";
                                try {
                                        _conn.Open ();
                                        Assert.Fail ("#1 should have failed on opening a non-existent db");
                                } catch (ArgumentException e) {Console.WriteLine (e);}
                                
                        } finally {
                                if (_conn != null && _conn.State != ConnectionState.Closed)
                                        _conn.Close ();
                        }
                }

        }
}
