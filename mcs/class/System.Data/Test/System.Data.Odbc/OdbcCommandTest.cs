//
// OdbcCommandTest.cs - NUnit Test Cases for testing the
// OdbcCommand class
//
// Authors:
//      Sureshkumar T (TSureshkumar@novell.com)
// 	Umadevi S (sumadevi@novell.com)
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
	public class OdbcCommandTest : MySqlOdbcBaseClient 
	{
		
		[SetUp]
		public void GetReady () {
			OpenConnection ();
			CreateTestSetup (); // create database & test tables
		}

		[TearDown]
		public void Clean () {
			CleanTestSetup (); // clean test database;
			CloseConnection ();
		}

		/// <summary>
		/// Test Execute Scalar Method
		/// </summary>
		[Test]
		public void ExecuteScalarTest () 
		{
			OdbcCommand cmd = conn.CreateCommand ();
			string query = "select count(*) from test order by col_int;";
			cmd.CommandText = query;
			object objCount = cmd.ExecuteScalar ();
			Assertion.AssertEquals( "ExecuteScalar does not return int type", 5, Convert.ToInt32(objCount));
		}

		 /// <summary>
                /// Test String parameters to ODBC Command
                /// </summary>
                [Test]
                public void ExecuteStringParameterTest()
                {
                                                                                                    
                        OdbcCommand dbcmd = new OdbcCommand();
                        dbcmd.Connection = conn;
                        dbcmd.CommandType = CommandType.Text;
                        dbcmd.CommandText = "select count(*) from test where col_char=?;";
                        string colvalue = "mono test#1";
                        dbcmd.Parameters.Add("@un",colvalue);
                        Object  obj = dbcmd.ExecuteScalar();
                        Assertion.AssertEquals( "String parameter not passed correctly",1, Convert.ToInt32(obj));
                                                                                                    
                                                                                                 
                }

		/// <summary>
                /// Test ExecuteNonQuery
                /// </summary>
                [Test]
                public void ExecuteNonQueryTest ()
                {
                                                                                                    
                        OdbcCommand dbcmd = new OdbcCommand();
                        dbcmd.Connection = conn;
                        dbcmd.CommandType = CommandType.Text;
                        dbcmd.CommandText = "select count(*) from test where col_char=?;";
                        string colvalue = "mono test";
                        dbcmd.Parameters.Add("@un",colvalue);
                        int ret = dbcmd.ExecuteNonQuery();
                        Assertion.AssertEquals( "ExecuteNonQuery not working",-1, ret);
                        dbcmd = new OdbcCommand();
                        dbcmd.Connection = conn;
                        dbcmd.CommandType = CommandType.Text;
                        dbcmd.CommandText = "delete from test where (col_int >257);";
                        ret = dbcmd.ExecuteNonQuery();
                        Assertion.AssertEquals("ExecuteNonQuery not working", 2, ret);
                                                                                                    
                 }

	}
}
