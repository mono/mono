//
// DataContainerTest.cs - NUnit Test Cases for testing the
//                          DataContainer class
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

using NUnit.Framework;

namespace MonoTests.System.Data.Common
{

  [TestFixture]
  public class DataContainerTest : MSSqlTestClient {
          
          [SetUp]
          public void GetReady () {
                OpenConnection ();
                CreateTestSetup (); // create test database & tables
          }

          [TearDown]
          public void Clean () {
                CleanTestSetup (); // clean test database
                CloseConnection ();
          }

	  private void CreateTestSetup()
	  { 
			if (!isConnAlive)
                                return ;
                        // Create test database & tables
                        string createQuery = "DROP TABLE datetimetest;" ;
                        ExecuteQuery (createQuery);
                        createQuery = "CREATE TABLE datetimetest (" +
                                          "col_char CHAR(20)," +
                                          "col_date DATETIME );";
                        ExecuteQuery (createQuery);
                        createQuery = "INSERT INTO datetimetest VALUES ('one', '4/12/2004 4:59:00');" ;
                        ExecuteQuery (createQuery);
                        createQuery = "INSERT INTO datetimetest VALUES ('two',null);" ;
	                ExecuteQuery (createQuery);
                        createQuery = "INSERT INTO datetimetest (col_char) VALUES ('three');" ;
	                ExecuteQuery (createQuery);

		
	  }

	  private void CleanTestSetup()
	  {  
			if (!isConnAlive)
                                return;
                        // delete test database
                        string dropQuery = "DROP table datetimetest";
                        ExecuteQuery(dropQuery);

	  } 	


          [Test]
          public void DateTimeTest () {
                try {

                                SqlDataAdapter myadapter = new SqlDataAdapter("select * from datetimetest;",conn);

				DataTable dt = new DataTable();
				myadapter.Fill(dt);
				Assertion.AssertEquals ("Row count must be three", 3, dt.Rows.Count );
                        }
                 
		finally { // try/catch is necessary to gracefully close connections
                        CleanTestSetup (); // clean test database
                        CloseConnection ();
                }
          }
    }
}
