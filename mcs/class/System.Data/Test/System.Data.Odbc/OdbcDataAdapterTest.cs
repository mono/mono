//
// OdbcDataAdapterTest.cs - NUnit Test Cases for testing the
//                          OdbcDataAdapter class
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

using NUnit.Framework;

namespace MonoTests.System.Data.Odbc
{

  [TestFixture]
  public class OdbcDataAdapterTest : MySqlOdbcBaseClient {
          
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

          [Test]
          public void FillTest () {
                try {
                        // For this Test, you must create sample table
                        // called test, with a non-zero number of rows
                        // and non-zero number of columns
                        // run the test initialization script mono_test_mysql.sql
                        string tableName = "test";
                        string sql= "select * from " + tableName; 
                        OdbcDataAdapter da = new OdbcDataAdapter (sql, conn);
                        DataSet ds = new DataSet (tableName);
                        da.Fill (ds, tableName);
                        Assertion.AssertEquals ("Table count must not be zero", true, ds.Tables.Count > 0 );
                        Assertion.AssertEquals ("Row count must not be zero", true, ds.Tables [0].Rows.Count > 0 );
                        foreach (DataColumn dc in ds.Tables [0].Columns)
                        Assertion.AssertEquals ("DataSet column names must noot be of size 0", true,
                                        dc.ColumnName.Length > 0);
                        foreach (DataRow dr in ds.Tables [0].Rows) {
                                foreach (DataColumn dc in ds.Tables [0].Columns) 
                                        Assertion.AssertEquals("column values must not be of size 0", true,
                                                dc.ColumnName.Length > 0);
                        }
                } finally { // try/catch is necessary to gracefully close connections
                        CleanTestSetup (); // clean test database
                        CloseConnection ();
                }
          }
    }
}
