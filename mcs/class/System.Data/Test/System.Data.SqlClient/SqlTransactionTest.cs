//
// SqlTransactionTest.cs - NUnit Test Cases for testing the
//                          SqlTransaction class
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
  public class SqlTransactionTest : MSSqlTestClient {
          
          [SetUp]
          public void GetReady () {
                OpenConnection ();
          }

          [TearDown]
          public void Clean () {
		CloseConnection ();
          }

	 [Test]
	  /**
	  The below test expects a table table4 with a bigint column.
	  **/	
          public void TransactionCommitTest () {
			
		using (SqlTransaction transaction = conn.BeginTransaction())
		{
			try
			{
				string sSql = "Insert into Region(RegionID,RegionDescription) Values ('10112', 'NovellBangalore')";
				SqlCommand command = new SqlCommand();
				command.Connection = conn;
				command.CommandText = sSql;
				command.Transaction = transaction;
				command.CommandType = CommandType.Text;
				command.ExecuteNonQuery();
				transaction.Commit();
			}
			catch (System.Exception ex)
			{
				transaction.Rollback();
			}
			finally
			{
				conn.Close();
			}
			
		}
	}

   }
}
