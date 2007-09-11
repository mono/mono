//
// OdbcDataReaderTest.cs - NUnit Test Cases for testing the
// OdbcDataReader class
//
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
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using Mono.Data;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcDataReaderTest
	{
		[Test]
		public void OutputParametersTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "call {? = sp_get_age (?,?)}";

				OdbcParameter ret = (OdbcParameter) new OdbcParameter ("ret", OdbcType.Int);
				cmd.Parameters.Add (ret);
				ret.Direction = ParameterDirection.ReturnValue;

				OdbcParameter name = (OdbcParameter) new OdbcParameter ("name", OdbcType.VarChar);
				cmd.Parameters.Add (name);
				name.Direction = ParameterDirection.Input;
				name.Value = "suresh";

				OdbcParameter age = (OdbcParameter) new OdbcParameter ("age", OdbcType.Int);
				cmd.Parameters.Add (age);
				name.Direction = ParameterDirection.Output;

				IDataReader reader = cmd.ExecuteReader ();
				reader.Close ();
				Assert.AreEqual (true, ((int) ret.Value) > 0, "#1");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
		[Test]
		public void LongTextTest ()
		{
			IDbConnection conn = new OdbcConnection (
						ConnectionManager.Singleton.ConnectionString);
			IDataReader rdr = null; 
			try {
				conn.Open ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "Select type_text"; 
				cmd.CommandText += " from string_family where id=3";

				rdr = cmd.ExecuteReader ();
				rdr.Read ();
				rdr.GetValue (0);
			}finally {
				if (rdr != null)
					rdr.Close ();
				conn.Close ();
			}
		}
		
		[Test]
		public void Bug82135Test ()
		{
			IDbConnection conn = new OdbcConnection (
						ConnectionManager.Singleton.ConnectionString);
			try {
				conn.Open ();
				OdbcCommand cmd = new OdbcCommand ("create table odbcnodatatest (ID int not null, Val1 text)",
								   (OdbcConnection) conn);
				cmd.ExecuteNonQuery ();
				cmd = new OdbcCommand ("delete from odbcnodatatest", (OdbcConnection) conn);
				Assert.AreEqual (0, cmd.ExecuteNonQuery ());
				cmd = new OdbcCommand ("drop table odbcnodatatest", (OdbcConnection) conn);
				cmd.ExecuteNonQuery ();
			}finally {
				conn.Close ();
			}
		}
		
		private static void DoExecuteNonQuery (OdbcConnection conn, string sql) {
			OdbcCommand cmd = new OdbcCommand (sql, conn);
			cmd.ExecuteNonQuery();
		}
  
		private static void DoExecuteScalar(OdbcConnection conn, string sql) {
			OdbcCommand cmd = new OdbcCommand (sql, conn);
			cmd.ExecuteScalar();
		}

		[Test]
		public void Bug82560Test ()
		{
			IDbConnection conn = new OdbcConnection (
						ConnectionManager.Singleton.ConnectionString);
			try {
				conn.Open ();
				DoExecuteNonQuery ((OdbcConnection) conn, "CREATE TABLE odbc_alias_test" + 
						   "(ifld INT NOT NULL PRIMARY KEY, sfld VARCHAR(20))");
				DoExecuteNonQuery ((OdbcConnection) conn, "INSERT INTO odbc_alias_test" +
						   "(ifld, sfld) VALUES (1, '1111')");
				DoExecuteScalar ((OdbcConnection) conn, "SELECT A.ifld FROM odbc_alias_test " +
						 "A WHERE A.ifld = 1");
				DoExecuteNonQuery ((OdbcConnection) conn, "DROP TABLE odbc_alias_test");
			}finally {
				conn.Close ();
			}
		}

		[Test]
		public void FindZeroInToStringTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			IDataReader reader = null;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandType = CommandType.Text;
				OdbcCmd.CommandText = "Drop table foo";
				try {
					OdbcCmd.ExecuteNonQuery ();
				} catch (OdbcException e) {
					Assert.Fail ("Exception thrown: " + e.Message);
				}
				// Create table
				OdbcCmd.CommandText = "Create table foo ( bar long varchar )";
				OdbcCmd.ExecuteNonQuery();

				// Insert a record into foo
				OdbcCmd.CommandText = "Insert into foo (bar) values ( '"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "This string has more than 255 characters"
				  + "' )";
				OdbcCmd.ExecuteNonQuery();
    
				// Now, get the record back - try and read it two different ways.
				OdbcCmd.CommandText = "SELECT bar FROM foo" ;
  
				reader = OdbcCmd.ExecuteReader ();
				string readAsString = "";
				while (reader.Read ()) {
					readAsString = reader[0].ToString();
				}
				reader.Close();
				// Now, read it using GetBytes
				reader = OdbcCmd.ExecuteReader ();
				byte[] buffer = new byte [2048];
				long total = 0;
				while (reader.Read ()) {
					total = reader.GetBytes (0, 0, buffer, 0, 2048);
				}
				reader.Close();
				// Convert bytes read to string - look for binary zero - there is none (OK)
				string readAsBytes = Encoding.Default.GetString (buffer, 0, (int) total);
				Assert.AreEqual  (readAsBytes, readAsString, "#1 ReadAsString is not same as ReadAsBytes");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
#if NET_2_0 
		[Test]
		public void GetDataTypeNameTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			IDataReader reader = null;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				Assert.AreEqual ("integer", reader.GetDataTypeName (0), "#1 GetDataTypeName should return integer not Int");
				Assert.AreEqual ("varchar", reader.GetDataTypeName (2), "#2 GetDataTypeName should return varchar not VarChar");
				Assert.AreEqual ("datetime", reader.GetDataTypeName (4), "#3 GetDataTypeName should return datetime not DateTime");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetDataTypeNameIndexOutOfRangeExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			IDataReader reader = null;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				try {
					/*string tmp = */reader.GetDataTypeName (6);
				} catch (IndexOutOfRangeException) {
					return;
				} Assert.Fail ("Expected Exception IndexOutOfRangeException not thrown");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetOrdinalTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			IDataReader reader = null;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				Assert.AreEqual (0, reader.GetOrdinal ("id"), "#1 First column should be id");
				Assert.AreEqual (1, reader.GetOrdinal ("fname"), "#2 Second column should fname");
				Assert.AreEqual (2, reader.GetOrdinal ("lname"), "#3 Third column should lname");
				Assert.AreEqual (3, reader.GetOrdinal ("dob"), "#4 Fourth column should dob");
				Assert.AreEqual (4, reader.GetOrdinal ("doj"), "#5 Fifth column should doj");
				Assert.AreEqual (5, reader.GetOrdinal ("email"), "#6 Sixth column should email");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetOrdinalIndexOutOfRangeExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			IDataReader reader = null;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				try {
					/*int ord = */reader.GetOrdinal ("non_existing_column");
				} catch (IndexOutOfRangeException){
					return;
				} Assert.Fail("Expected Exception IndexOutOfRangeException not thrown");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}


		[Test]
		public void GetFieldTypeTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			IDataReader reader = null;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				Assert.AreEqual (typeof(int), reader.GetFieldType (0), "#1 Field type is not Integer");
				Assert.AreEqual (typeof(string), reader.GetFieldType (2), "#2 Field type is not Varchar");
				Assert.AreEqual (typeof(DateTime), reader.GetFieldType (4), "#3 Field type is not DateTime");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetFieldTypeIndexOutOfRangeExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			IDataReader reader = null;;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				try {
					/*String tmp = */reader.GetFieldType (6).ToString ();
				} catch (IndexOutOfRangeException){
					return;
				} Assert.Fail("Expected Exception IndexOutOfRangeException not thrown");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetNameTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			IDataReader reader = null;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				Assert.AreEqual ("id", reader.GetName (0), "#1 First column is not id");
				Assert.AreEqual ("fname", reader.GetName (1), "#2 Second column is not fname");
				Assert.AreEqual ("lname", reader.GetName (2), "#3 Third column is not lname");
				Assert.AreEqual ("dob", reader.GetName (3), "#4 Fourth column is not dob");
				Assert.AreEqual ("doj", reader.GetName (4), "#5 Fifth column is not doj");
				Assert.AreEqual ("email", reader.GetName (5), "#6 Sixth column is not email");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void GetNameIndexOutOfRangeExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			IDataReader reader = null;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand OdbcCmd = conn.CreateCommand ();
				OdbcCmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				try {
					/*String tmp = */reader.GetName (6);
				} catch (IndexOutOfRangeException){
					return;
				} Assert.Fail("Expected Exception IndexOutOfRangeException not thrown");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
#endif
	}
}
