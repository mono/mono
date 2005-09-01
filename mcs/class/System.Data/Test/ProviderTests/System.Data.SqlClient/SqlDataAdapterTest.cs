//
// SqlDataAdapterTest.cs - NUnit Test Cases for testing the
//                          SqlDataAdapter class
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
	public class SqlDataAdapterTest
	{
		SqlConnection conn;

		[Test]
		/**
		   The below test will not run everytime, since the region id column is unique
		   so change the regionid if you want the test to pass.
		**/	
		public void UpdateTest () {
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				DataTable dt = new DataTable();
				SqlDataAdapter da = null;
				da = new SqlDataAdapter("Select * from employee;", conn);
				SqlCommandBuilder cb = new SqlCommandBuilder (da);
				da.Fill(dt);
				DataRow dr = dt.NewRow();
				dr ["id"] = 6002;
				dr ["fname"] = "boston";
				dr ["dob"] = DateTime.Now.Subtract (new TimeSpan (20*365, 0, 0, 0));
				dr ["doj"] = DateTime.Now;
				dt.Rows.Add(dr);

				da.Update(dt);
			} finally {
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void FillSchemaTest() 
		{
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				string sql = "select * from employee;";
				SqlCommand c = conn.CreateCommand();
				c.CommandText = sql;
				SqlDataReader dr = c.ExecuteReader(CommandBehavior.KeyInfo|CommandBehavior.SchemaOnly);
				DataTable schema = dr.GetSchemaTable();
				DataRowCollection drc = schema.Rows;
				DataRow r = drc[0];
				Assert.AreEqual("id",r["ColumnName"].ToString());
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		/**
		   This needs a errortable created as follows 
		   id uniqueidentifier,name char(10) , with values
		   Guid		name
		   {A12...}	NULL
		   NULL		bbbbbb
		**/
		[Test]
		public void NullGuidTest() 
		{
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				DBHelper.ExecuteNonQuery (conn, "create table #tmp_guid_table ( " +
							  " id uniqueidentifier default newid (), " +
							  " name char (10))");
				DBHelper.ExecuteNonQuery (conn, "insert into #tmp_guid_table (name) values (null)");
				DBHelper.ExecuteNonQuery (conn, "insert into #tmp_guid_table (id, name) values (null, 'bbbb')");
				SqlDataAdapter da = new SqlDataAdapter("select * from #tmp_guid_table", conn);
				DataSet ds = new DataSet();
				da.Fill(ds);
				Assert.AreEqual (1, ds.Tables.Count, "#1");
				Assert.AreEqual (DBNull.Value, ds.Tables [0].Rows [1] ["id"], "#2");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
			// the bug 68804 - is that the fill hangs!
			Assert.AreEqual("Done","Done");
					
		}
 
	
	

	
	}
}
