//
// MSSqlTestBed.cs : This is base class which manages the connections to 
//                    MSSql database. This serves as a base class for all
//		      MSSql database dependant tests.
//
// To run :
//  
//  * compile using following command
//      mcs /r:System.Data.dll,nunit.framework.dll /t:library /debug
//      /out:MSSqlTestBed.dll MSSqlTestBed.cs System.Data.Common/*.cs
//  * To run the tests
//      mono /usr/local/bin/nunit-console.exe MSSqlTestBed.dll
//
// Author:
//      Umadevi S (sumadevi@novell.com)
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
using System.Data.SqlClient;
using System.Collections.Specialized;

namespace MonoTests.System.Data 
{
        public class MSSqlTestClient  
        {
                #region protected members
                protected string connectionString = null;
                protected SqlConnection conn = null;
                protected bool isConnAlive = false;
                #endregion

                public MSSqlTestClient ()
                {
				connectionString =
                                        "Server=164.99.168.131;" +
                                        "Database=Northwind;" +
                                        "User ID=sa;" +
                                        "Password=[PLACEHOLDER]";
                                conn = new SqlConnection(connectionString);
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

		internal void ExecuteQuery (string query)
                {
                        SqlCommand cmd = new SqlCommand ();
                        cmd.Connection = conn;
                        cmd.CommandText = query;
                        try {
                                int recordsAff = cmd.ExecuteNonQuery ();
                        } catch (Exception e) {
				Console.WriteLine("exception");
				Console.WriteLine(e.StackTrace);
                        }
                }
	

        }
}
