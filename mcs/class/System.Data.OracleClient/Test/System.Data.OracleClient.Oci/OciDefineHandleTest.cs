//
// OracleParameterTest.cs -
//      NUnit Test Cases for OciDefineHandle
//
// Author:
//      Leszek Ciesielski  <skolima@gmail.com>
//
// Copyright (C) 2006 Forcom (http://www.forcom.com.pl/)
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

using NUnit.Framework;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Data.OracleClient;
using System.Data;
using System;

namespace MonoTests.System.Data.OracleClient {

        [TestFixture]
        public class OciDefineHandleTest {

                String connection_string;
                OracleConnection connection;
                OracleCommand command;

                // test string
                string test_value = "  sim\u4FF5ply\u65E5 tri\u672Cm te\u4F00st      ";
                string test_value2 = "  simply \u672Ctrim\u4F00 test in\u65E5 query  \u4FF5    ";

                [TestFixtureSetUp]
                public void FixtureSetUp ()
                {
                        connection_string = Environment.GetEnvironmentVariable ("MONO_TESTS_ORACLE_CONNECTION_STRING");
                        if(connection_string == null)
                                Assert.Ignore ("Please consult README.tests.");
                }

                [SetUp]
                public void SetUp ()
                {
                        connection = new OracleConnection (connection_string);
                        connection.Open ();
                        using (command = connection.CreateCommand ()) {
                                // create the tables
                                command.CommandText =
                                                "create table utf8test (id number(10), text nvarchar2(64), text2 nvarchar2(64) )";
                                command.ExecuteNonQuery ();
                        }
                }

                [TearDown]
                public void TearDown ()
                {
                        using (command = connection.CreateCommand ()) {
                                command.CommandText = "drop table utf8test";
                                command.ExecuteNonQuery ();
                        }

                        connection.Close ();
                        connection.Dispose ();
                }

                [Test] // regression for bug #79004
                public void TrimsUnicodeStringsTest ()
                {
                        using (command = connection.CreateCommand ()) { // reusing command from SetUp causes parameter names mismatch

                                // insert test values
                                command.CommandText =
                                                "insert into utf8test (id,text,text2) values (:id,:txt,'" + test_value2 + "')";
                                command.Parameters.Add (new OracleParameter ("ID", OracleType.Int32));
                                command.Parameters.Add( new OracleParameter ("TXT", OracleType.NVarChar));
                                command.Parameters ["ID"].Value = 101;
                                command.Parameters ["TXT"].Value = test_value;
                                command.ExecuteNonQuery ();

                                // read test values
                                command.CommandText =
                                                "select text,text2 from utf8test where id = 101";
                                command.Parameters.Clear ();
                                using (OracleDataReader reader = command.ExecuteReader ()) {
                                        if (reader.Read ()) {
                                                Assert.AreEqual (test_value, reader.GetString (0), "Passed through bind value mismatched");
                                                Assert.AreEqual (test_value2, reader.GetString (1), "Directly passed value mismatched");
                                        }
                                        else {
                                                Assert.Fail ("Expected records not found.");
                                        }
                                }
                        }
                }
        }
}
