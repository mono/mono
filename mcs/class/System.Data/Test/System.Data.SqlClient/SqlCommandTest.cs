//
// SqlCommandTest.cs - NUnit Test Cases for testing the
//                          SqlCommand class
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

namespace MonoTests.System.Data.SqlClient
{

  [TestFixture]
  public class SqlCommandTest : MSSqlTestClient {
          
          [SetUp]
          public void GetReady () {
                OpenConnection ();
          }

          [TearDown]
          public void Clean () {
		CloseConnection ();
          }

	  /**
	  This is required to be run only once, call this from the GetReady.	
	  **/
	  private void setup(){
		string createquery = "CREATE PROCEDURE sp_insert @TestPar1 varchar(50),@BirthDate datetime as insert into Employees(LastName,FirstName) VALUES('SSS','uuuu') ";
		SqlCommand cmd = new SqlCommand();
		cmd.Connection = conn;
		cmd.CommandText = createquery;
		int ret =cmd.ExecuteNonQuery();
	  }
		

	 [Test]
	  /**
	  The below test expects the stored procedure sp_insert in the database.
	  **/	
          public void ExecuteNonQueryTest () {
		try    {
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = "sp_insert";
			cmd.CommandType = CommandType.StoredProcedure;
			Object TestPar = System.DBNull.Value;
			cmd.Parameters.Add("@TestPar1",SqlDbType.Int);
			cmd.Parameters["@TestPar1"].Value = TestPar;
			cmd.Parameters.Add("@BirthDate",DateTime.Now);
			Assert.AreEqual(-1,cmd.ExecuteNonQuery());
                }
		catch  (Exception e) {
			Assert.Fail("A#01 Got an exception");
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
				
		}
                 
		finally { // try/catch is necessary to gracefully close connections
                       
                        CloseConnection ();
                }
          }



	
	
    }
}
