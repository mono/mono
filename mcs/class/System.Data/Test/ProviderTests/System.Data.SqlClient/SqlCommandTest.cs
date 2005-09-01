//
// SqlCommandTest.cs - NUnit Test Cases for testing the
//                          SqlCommand class
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
	public class SqlCommandTest 
	{

		public SqlConnection conn;

		[Test]
		public void ExecuteNonQueryTempProcedureTest () {
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				// create temp sp here, should normally be created in Setup of test 
				// case, but cannot be done right now because of ug #68978
				DBHelper.ExecuteNonQuery (conn, CREATE_TMP_SP_TEMP_INSERT_PERSON);
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText = "#sp_temp_insert_employee";
				cmd.CommandType = CommandType.StoredProcedure;
				Object TestPar = "test";
				cmd.Parameters.Add("@fname", SqlDbType.VarChar);
				cmd.Parameters ["@fname"].Value = TestPar;
				Assert.AreEqual(-1,cmd.ExecuteNonQuery());
			} finally {
				DBHelper.ExecuteNonQuery (conn, DROP_TMP_SP_TEMP_INSERT_PERSON);
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_person_table");
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		/**
		 * Verifies whether an enum value is converted to a numeric value when
		 * used as value for a numeric parameter (bug #66630)
		 */
		[Test]
		public void EnumParameterTest() {
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				// create temp sp here, should normally be created in Setup of test 
				// case, but cannot be done right now because of ug #68978
				DBHelper.ExecuteNonQuery (conn, "CREATE PROCEDURE #Bug66630 (" 
							  + "@Status smallint = 7"
							  + ")"
							  + "AS" + Environment.NewLine
							  + "BEGIN" + Environment.NewLine
							  + "SELECT CAST(5 AS int), @Status" + Environment.NewLine
							  + "END");
				
				SqlCommand cmd = new SqlCommand("#Bug66630", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Status", SqlDbType.Int).Value = Status.Error;

				using (SqlDataReader dr = cmd.ExecuteReader()) {
					// one record should be returned
					Assert.IsTrue(dr.Read(), "EnumParameterTest#1");
					// we should get two field in the result
					Assert.AreEqual(2, dr.FieldCount, "EnumParameterTest#2");
					// field 1
					Assert.AreEqual("int", dr.GetDataTypeName(0), "EnumParameterTest#3");
					Assert.AreEqual(5, dr.GetInt32(0), "EnumParameterTest#4");
					// field 2
					Assert.AreEqual("smallint", dr.GetDataTypeName(1), "EnumParameterTest#5");
					Assert.AreEqual((short) Status.Error, dr.GetInt16(1), "EnumParameterTest#6");
					// only one record should be returned
					Assert.IsFalse(dr.Read(), "EnumParameterTest#7");
				}
			} finally {
				DBHelper.ExecuteNonQuery (conn, "if exists (select name from sysobjects " +
							  " where name like '#temp_Bug66630' and type like 'P') " +
							  " drop procedure #temp_Bug66630; ");
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		/**
		 * The below test does not need a connection but since the setup opens 
		 * the connection i will need to close it
		 */
		[Test]
		public void CloneTest() {
			ConnectionManager.Singleton.OpenConnection ();
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = null;
			cmd.CommandText = "sp_insert";
			cmd.CommandType = CommandType.StoredProcedure;
			Object TestPar = DBNull.Value;
			cmd.Parameters.Add("@TestPar1", SqlDbType.Int);
			cmd.Parameters["@TestPar1"].Value = TestPar;
			cmd.Parameters.Add("@BirthDate", DateTime.Now);
			cmd.DesignTimeVisible = true;
			cmd.CommandTimeout = 100;
			Object clone1 = ((ICloneable)(cmd)).Clone();
			SqlCommand cmd1 = (SqlCommand) clone1;
			Assert.AreEqual(2, cmd1.Parameters.Count);
			Assert.AreEqual(100, cmd1.CommandTimeout);
			cmd1.Parameters.Add("@test", DateTime.Now);
			// to check that it is deep copy and not a shallow copy of the
			// parameter collection
			Assert.AreEqual(3, cmd1.Parameters.Count);
			Assert.AreEqual(2, cmd.Parameters.Count);
		}

		private enum Status { 
			OK = 0,
			Error = 3
		}

		private readonly string CREATE_TMP_SP_TEMP_INSERT_PERSON = ("create procedure #sp_temp_insert_employee ( " + Environment.NewLine + 
									    "@fname varchar (20), " + Environment.NewLine + 
									    "as " + Environment.NewLine + 
									    "begin" + Environment.NewLine + 
									    "declare @id int;" + Environment.NewLine + 
									    "select @id = max (id) from employee;" + Environment.NewLine + 
									    "set @id = @id + 6000 + 1;" + Environment.NewLine + 
									    "insert into employee (id, fname, dob, doj) values (@id, @fname, '1980-02-11', getdate ());" + Environment.NewLine + 
									    "return @id;" + Environment.NewLine + 
									    "end");

		private readonly string DROP_TMP_SP_TEMP_INSERT_PERSON = ("if exists (select name from sysobjects where " + Environment.NewLine + 
									  "name = '#sp_temp_insert_employee' and type = 'P') " + Environment.NewLine + 
									  "drop procedure #sp_temp_insert_employee; ");
	}
}
