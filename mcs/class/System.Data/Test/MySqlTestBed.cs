//
// MySqlTestBed.cs : This is base class which manages the connections to 
//                    mysql database. This serves as a base class for all
//		      mysql database dependant tests.
//
// To run :
//  * create a test database in mysql server.
//  * create an DNS entry.
//  * update the MySqlTestBed.config with the DNS names and
//    username, password for connection in the configuration key
//    MySql-DSN
//  * compile using following command
//      mcs /r:System.Data.dll,NUnit.Framework.dll /t:library /debug
//      /out:MySqlTestBed.dll MySqlTestBed.cs System.Data.Odbc/*.cs
//  * To run the tests
//      mono /usr/local/bin/nunit-console.exe MySqlTestBed.dll
//
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

using System;
using System.Data;
using System.Data.Odbc;
using System.Collections.Specialized;

namespace MonoTests.System.Data 
{
        public class MySqlOdbcBaseClient  
        {
                #region protected members
                protected string connectionString = null;
                protected OdbcConnection conn = null;
                protected bool isConnAlive = false;
                #endregion

                public MySqlOdbcBaseClient ()
                {
                        //Connection String with DSN.
                        NameValueCollection appSettings = System.Configuration.ConfigurationSettings.AppSettings ;
                        connectionString = appSettings ["MySql-DSN"];
                        conn = new OdbcConnection (connectionString);
                }

                protected void OpenConnection () 
                {
                        conn.ConnectionString = connectionString;
                        conn.Open ();
                        // run tests only if the connection is open,
                        // otherwise make it fail, to setup with correct
                        // database settings
                        if (conn != null && conn.State != ConnectionState.Closed) 
                                isConnAlive = true;
                }

                protected void CloseConnection () 
                {
                        if (conn != null && conn.State != ConnectionState.Closed) {
                                conn.Close ();
                                isConnAlive = false;
                        }
                }

                protected void CreateTestSetup ()
		{
                        if (!isConnAlive)
                                return ;
                        // Create test database & tables
                        // mysql odbc does not supports batch sql statements
                        string createQuery = "DROP TABLE IF EXISTS test;" ;
                        ExecuteQuery (createQuery);
                        createQuery = "CREATE TABLE test (" + 
                                          "pk_tint TINYINT NOT NULL PRIMARY KEY," + 
                                          "col_char CHAR(20)," + 
                                          "col_int INT," + 
                                          "col_blob TINYBLOB," + 
                                          "col_datetime DATETIME," + 
                                          "col_date DATE," + 
                                          "col_time TIME" + 
                                          ");";  
                        ExecuteQuery (createQuery);
                        createQuery = "INSERT INTO test VALUES (1, 'mono test" +
                                      "#1', 255, 127123645917568585638457243856234985, '2004-08-22', '2004-08-22', '12:00:00' );" ; 
                        ExecuteQuery (createQuery);
                        createQuery = "INSERT INTO test VALUES (2, 'mono test" +
                                      "#2', 256, NULL, NULL, NULL, NULL );";
                        ExecuteQuery (createQuery);
                        createQuery = "INSERT INTO test VALUES (3, 'mono test" +
                                      "#3', 257 , 127123645917568585638457243856234985, '2004-08-22', '2004-08-22', '12:00:00');" ; 
                        ExecuteQuery (createQuery);
                        createQuery = "INSERT INTO test VALUES (4, 'mono test" +
                                      "#4', 258 , 127123645917568585638457243856234985, '2004-08-22', '2004-08-22', '12:00:00');" ; 
                        ExecuteQuery (createQuery);
                        createQuery = "INSERT INTO test VALUES (5, 'mono test" +
                                      "#5', 259, 127123645917568585638457243856234985, '2004-08-22', '2004-08-22', '12:00:00' );" ;
                        ExecuteQuery (createQuery);
                }

                private void ExecuteQuery (string query) 
                {
                        OdbcCommand cmd = new OdbcCommand ();
                        cmd.Connection = conn;
                        cmd.CommandText = query;
                        try {
                                int recordsAff = cmd.ExecuteNonQuery ();
                        } catch (Exception e) {
                        }
                }

                protected void CleanTestSetup ()
                {
                        if (!isConnAlive)
                                return;
                        // delete test database 
                        string dropQuery = "DROP table IF EXISTS test";
                        //ExecuteQuery(dropQuery);
                }
        }
}
