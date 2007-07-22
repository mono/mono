//
// SqlTransactionTest.cs - NUnit Test Cases for testing the
//                          SqlTransaction class
// Author:
//      Umadevi S (sumadevi@novell.com)
//	Sureshkumar T (tsureshkumar@novell.com)
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
	[Category ("sqlserver")]
	public class SqlTransactionTest
	{
		[Test]
		public void TransactionCommitTest () 
		{
			SqlConnection conn = new SqlConnection (ConnectionManager.Singleton.ConnectionString);
			try {
				conn.Open ();
				using (SqlTransaction transaction = conn.BeginTransaction())
				{
					string sSql = "Insert into employee (id, fname, dob, doj) Values (6005, 'NovellBangalore', '1989-02-11', '2005-07-22')";
					SqlCommand command = new SqlCommand();
					command.Connection = conn;
					command.CommandText = sSql;
					command.Transaction = transaction;
					command.CommandType = CommandType.Text;
					command.ExecuteNonQuery();
					transaction.Commit();
				}
			} catch (SqlException ex) {
				// SqlException is thrown when running test-case for 1.0 and 2.0 profile
				// Violation of PRIMARY KEY constraint 'PK__employee__5DF5D7ED'.
				// Cannot insert duplicate key in object 'dbo.employee'.
			} finally {
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_person_table");
				if (conn != null && conn.State != ConnectionState.Closed)
					conn.Close ();
			}

		}

	}
}
