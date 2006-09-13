//
// OracleParameterTest.cs -
//      NUnit Test Cases for OracleParameter
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
        public class OracleParameterTest {

                String connection_string;
                OracleConnection connection;
                OracleCommand command;

                // test string
                string test_value = "  simply trim test      ";
                string test_value2 = "  simply trim test in query      ";

                [TestFixtureSetUp]
                public void FixtureSetUp ()
                {
                        connection_string = ConfigurationSettings.AppSettings.Get ("OracleConnectionString");
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
                                                "create table oratest (id number(10), text varchar2(64), text2 varchar2(64) )";
                                command.ExecuteNonQuery ();

                                command.CommandText =
                                                "create table culture_test (id number(10), value1 float, value2 number(20,10))";
                                command.ExecuteNonQuery ();
                        }
                }

                [TearDown]
                public void TearDown ()
                {
                        using (command = connection.CreateCommand ()) {
                                command.CommandText = "drop table oratest";
                                command.ExecuteNonQuery ();
                                command.CommandText = "drop table culture_test";
                                command.ExecuteNonQuery ();
                        }

                        connection.Close ();
                        connection.Dispose ();
                }

                [Test] // regression for bug #78509
                public void TrimsTrailingSpacesTest ()
                {
                        using (command = connection.CreateCommand ()) { // reusing command from SetUp causes parameter names mismatch

                                // insert test values
                                command.CommandText =
                                                "insert into oratest (id,text,text2) values (:id,:txt,'" + test_value2 + "')";
                                command.Parameters.Add (new OracleParameter ("ID", OracleType.Int32));
                                command.Parameters.Add( new OracleParameter ("TXT", OracleType.VarChar));
                                command.Parameters ["ID"].Value = 100;
                                command.Parameters ["TXT"].Value = test_value;
                                command.ExecuteNonQuery ();

                                // read test values
                                command.CommandText =
                                                "select text,text2 from oratest where id = 100";
                                command.Parameters.Clear ();
                                using (OracleDataReader reader = command.ExecuteReader ()) {
                                        if (reader.Read ()) {
                                                Assert.AreEqual (test_value2, reader.GetString (1), "Directly passed value mismatched");
                                                Assert.AreEqual (test_value, reader.GetString (0), "Passed through bind value mismatched");
                                        }
                                        else {
                                                Assert.Fail ("Expected records not found.");
                                        }
                                }
                        }
                }

                [Test] // regression for bug #79284
                public void CultureSensitiveNumbersTest ()
                {
                        CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

                        Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-GB", false);
                        CultureSensitiveNumbersInsertTest (1);
                        CultureSensitiveNumbersSelectTest (1);

                        Thread.CurrentThread.CurrentCulture = new CultureInfo ("pl-PL", false);
                        CultureSensitiveNumbersInsertTest (2);
                        CultureSensitiveNumbersSelectTest (2);

                        Thread.CurrentThread.CurrentCulture = new CultureInfo ("ja-JP", false);
                        CultureSensitiveNumbersInsertTest (3);
                        CultureSensitiveNumbersSelectTest (3);

                        Thread.CurrentThread.CurrentCulture = currentCulture;
                }

                // regression for bug #79284
                protected void CultureSensitiveNumbersInsertTest (int id)
                {
                        using (command = connection.CreateCommand ()) { // reusing command from SetUp causes parameter names mismatch
                                // insert test values
                                command.CommandText =
                                                "insert into culture_test (id,value1,value2) values (:id,:value1,:value2)";
                                command.Parameters.Add (new OracleParameter ("ID", OracleType.Int32));
                                command.Parameters.Add( new OracleParameter ("VALUE1", OracleType.Float));
                                command.Parameters.Add( new OracleParameter ("VALUE2", OracleType.Double));
                                command.Parameters ["ID"].Value = id;
                                command.Parameters ["VALUE1"].Value = 2346.2342f;
                                command.Parameters ["VALUE2"].Value = 4567456.23412m;
                                try {
                                        command.ExecuteNonQuery ();
                                }
                                catch (OracleException e) {
                                        if (e.Code == 1722)
                                                Assert.Fail("Culture incompatibility error while inserting [" + id + ']');
                                        else throw;
                                }
                        }
                }

                // regression for bug #79284
                protected void CultureSensitiveNumbersSelectTest (int id)
                {
                        using (command = connection.CreateCommand ()) { // reusing command from SetUp causes parameter names mismatch
                                // read test values
                                command.CommandText =
                                                "select value1,value2 from culture_test where id = " + id;
                                command.Parameters.Clear ();
                                try {
                                        using (OracleDataReader reader = command.ExecuteReader ()) {
                                                if (reader.Read ()) {
                                                        Assert.AreEqual (2346.2342f,reader.GetFloat(0),
                                                                "Float value improperly stored [" + id + ']');
                                                        Assert.AreEqual (4567456.23412m, reader.GetDecimal (1),
                                                                "Decimal value improperly stored [" + id + ']');
                                                }
                                                else {
                                                        Assert.Fail ("Expected records not found [" + id + ']');
                                                }
                                        }
                                }
                                catch (FormatException) {
                                        Assert.Fail("Culture incompatibility error while reading [" + id + ']');
                                }
                        }
                }
        }
}
