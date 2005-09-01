//
// SqlDataReaderTest.cs - NUnit Test Cases for testing the
//                          SqlDataReader class
// Author:
//      Umadevi S (sumadevi@novell.com)
//      Kornél Pál <http://www.kornelpal.hu/>
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
	public class SqlDataReaderTest 
	{
		SqlConnection conn;
		
		[Test]
		public void ReadEmptyNTextFieldTest () {
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				DBHelper.ExecuteNonQuery (conn, "create table #tmp_monotest (name ntext)");
				DBHelper.ExecuteNonQuery (conn, "insert into #tmp_monotest values ('')");
				
				SqlCommand cmd = (SqlCommand) conn.CreateCommand ();
				cmd.CommandText = "select * from #tmp_monotest";
				SqlDataReader dr = cmd.ExecuteReader ();
				if (dr.Read()) {
					Assert.AreEqual("System.String",dr["NAME"].GetType().FullName);
				}
				Assert.AreEqual (false, dr.Read (), "#2");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}		

		[Test]
		public void ReadBingIntTest() 
		{
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				string query = "SELECT CAST(548967465189498 AS bigint) AS Value";
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText = query;
				SqlDataReader r = cmd.ExecuteReader();
				using (r) {
					Assert.AreEqual (true, r.Read(), "#1");
					long id = r.GetInt64(0);
					Assert.AreEqual(548967465189498, id, "#2");
					id = r.GetSqlInt64(0).Value;
					Assert.AreEqual(548967465189498, id, "#3");
				}
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
	}
}
