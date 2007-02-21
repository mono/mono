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
		public void GetDataTypeNameTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			OdbcDataReader reader;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				Assert.AreEqual ("integer", reader.GetDataTypeName (0), "#1 Column id");
				Assert.AreEqual ("varchar", reader.GetDataTypeName (2), "#2 Column lname");
				Assert.AreEqual ("datetime", reader.GetDataTypeName (4), "#3 Column doj");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void GetDataTypeNameIndexOutOfRangeExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			OdbcDataReader reader;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				string tmp = reader.GetDataTypeName (6);
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
			OdbcDataReader reader;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				Assert.AreEqual (0, reader.GetOrdinal ("id"), "#1 First column = id");
				Assert.AreEqual (1, reader.GetOrdinal ("fname"), "#2 Second column = fname");
				Assert.AreEqual (2, reader.GetOrdinal ("lname"), "#3 Third column = lname");
				Assert.AreEqual (3, reader.GetOrdinal ("dob"), "#4 Fourth column = dob");
				Assert.AreEqual (4, reader.GetOrdinal ("doj"), "#5 Fifth column = doj");
				Assert.AreEqual (5, reader.GetOrdinal ("email"), "#6 Sixth column = email");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void GetOrdinalIndexOutOfRangeExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			OdbcDataReader reader;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				int ord = reader.GetOrdinal ("non_existing_column");
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
			OdbcDataReader reader;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				Assert.AreEqual ("System.Int32", reader.GetFieldType (0).ToString (), "#1 Integer");
				Assert.AreEqual ("System.String", reader.GetFieldType (2).ToString (), "#2 Varchar");
				Assert.AreEqual ("System.DateTime", reader.GetFieldType (4).ToString (), "#3 DateTime");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void GetFieldTypeIndexOutOfRangeExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			OdbcDataReader reader;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				String tmp = reader.GetFieldType (6).ToString ();
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
			OdbcDataReader reader;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				Assert.AreEqual ("id", reader.GetName (0), "#1 First column = id");
				Assert.AreEqual ("fname", reader.GetName (1), "#2 Second column = fname");
				Assert.AreEqual ("lname", reader.GetName (2), "#3 Third column = lname");
				Assert.AreEqual ("dob", reader.GetName (3), "#4 Fourth column = dob");
				Assert.AreEqual ("doj", reader.GetName (4), "#5 Fifth column = doj");
				Assert.AreEqual ("email", reader.GetName (5), "#6 Sixth column = email");
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void GetNameIndexOutOfRangeExceptionTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			OdbcDataReader reader;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = OdbcCmd.ExecuteReader ();
				String tmp = reader.reader.GetName (6);
			} finally {
				if (reader != null)
					reader.Close ();
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
	}
}
