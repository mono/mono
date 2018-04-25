//
// OracleParameterTest.cs -
//      NUnit Test Cases for OracleLob
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
using System.IO;
using System;

namespace MonoTests.System.Data.OracleClient {

        [TestFixture]
        public class OracleLobTest {

                String connection_string;
                OracleConnection connection;
                OracleCommand command;

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
                                                "create table lob_test (id number(10), lobo blob)";
                                command.ExecuteNonQuery ();
                        }
                }

                [TearDown]
                public void TearDown ()
                {
                        using (command = connection.CreateCommand ()) {
                                command.CommandText = "drop table lob_test";
                                command.ExecuteNonQuery ();
                        }

                        connection.Close ();
                        connection.Dispose ();
                }

                [Test] // regression for bug #78898
                public void PositionIs0BasedTest ()
                {
                        using (command = connection.CreateCommand ()) { // reusing command from SetUp causes parameter names mismatch
                                // insert test values
                                command.CommandText =
                                                "insert into lob_test (id, lobo) values (11, '00000000000000000000000000000')";
                                command.ExecuteNonQuery ();

                                // select for writing test values
                                command.CommandText =
                                                "select lobo from lob_test where id = 11 for update";

                                using (OracleDataReader reader = command.ExecuteReader ()) {
                                        if (reader.Read ()) {
                                                OracleLob lob = reader.GetOracleLob (0);
                                                Assert.AreEqual (0, lob.Position, "Lob index is not 0 - based.");
                                                lob.Seek (1, SeekOrigin.Current);
                                                Assert.AreEqual (1, lob.Position, "Lob seek placed position wrongly.");
                                                lob.Seek (0, SeekOrigin.End);
                                                Assert.AreEqual (lob.Length , lob.Position, "Lob end is too far away.");
                                                TrySeek (lob, -lob.Length, SeekOrigin.End, 0, 1);
                                                TrySeek (lob, -lob.Length + 5, SeekOrigin.End, 5, 2);
                                                try {
                                                        lob.Seek (5, SeekOrigin.End);
                                                        Assert.Fail ("Illegal seek succeeded.");
                                                }
                                                catch (ArgumentOutOfRangeException) { // exception is required
                                                }
                                                lob.Seek (lob.Length - 5, SeekOrigin.Begin);
                                                Assert.AreEqual (lob.Length - 5 , lob.Position, "Lob position has unexpected value.");
                                                lob.Seek (0, SeekOrigin.Begin);
                                                TryRead (lob, 10, 1);
                                                lob.Seek (5, SeekOrigin.Begin);
                                                lob.Position = 0;
                                                TryRead (lob, 10, 2);
                                                lob.Position = lob.Length;
                                                TryRead (lob, 0, 3);
                                                lob.Seek (-1, SeekOrigin.Current);
                                                TryRead (lob, 1, 4);
                                        }
                                        else {
                                                Assert.Fail ("Expected records not found.");
                                        }
                                }
                        }
                }

                void TrySeek(OracleLob lob, long offset, SeekOrigin start, long expected, int id)
                {
                        try {
                                lob.Seek (offset, start);
                                Assert.AreEqual (expected, lob.Position, "Lob position was unexpected [" + id + ']');
                        }
                        catch (ArgumentOutOfRangeException) {
                                Assert.Fail ("Unable to perform a legal seek [" + id + ']');
                        }
                }

                void TryRead(OracleLob lob, long expectedCount, int id)
                {
                        try {
                                long numberRead = lob.Read (new byte [10], 0, 10);
                                Assert.AreEqual (expectedCount, numberRead, "Wrong number of bytes read [" + id + ']');
                        }
                        catch (OracleException e) {
                                if (e.Code == 24801)
                                        Assert.Fail ("Unable to perform a legal read [" + id + ']');
                                else throw;
                        }
                }
        }
}
