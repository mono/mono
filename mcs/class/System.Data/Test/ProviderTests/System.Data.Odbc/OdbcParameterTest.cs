// OdbcCommandTest.cs - NUnit Test Cases for testing the
// OdbcCommand class
//
// Authors:
//      Sureshkumar T (TSureshkumar@novell.com)
// 
// Copyright (c) 2005 Novell Inc., and the individuals listed
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
using System.Globalization;

using Mono.Data;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcParameterTest
	{
		[Test]
		public void IntegerParamTest ()
		{
			string query = "select type_int from numeric_family where id = ?";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (-2147483648, (int) reader [0], "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void BigIntParamTest ()
		{
			string query = "select id, type_bigint from numeric_family where type_bigint = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = (long) (9223372036854775807);
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void SmallIntParamTest ()
		{
			string query = "select id, type_smallint from numeric_family where type_smallint = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_smallint", OdbcType.SmallInt);
				param.Value = 32767;
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void TinyIntParamTest ()
		{
			string query = "select id, type_tinyint from numeric_family where type_tinyint = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_tinyint", OdbcType.TinyInt);
				param.Value = 255;
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}


		[Test]
		public void StringParamTest ()
		{
			string query = "select id, fname from employee where fname = ?";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("fname", OdbcType.VarChar);
				param.Value = "suresh";
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, (int) reader [0], "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}


		[Test]
		public void BitParameterTest ()
		{
			string query = "select id, type_bit from numeric_family where type_bit = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_bit", OdbcType.Bit);
				param.Value = true;
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void CharParameterTest ()
		{
			string query = "select id, type_char from string_family where type_char = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_char", OdbcType.Char);
				param.Value = "char";
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void DecimalParameterTest ()
		{
			string query = "select id, type_decimal from numeric_family where type_decimal = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_decimal", OdbcType.Decimal);
				param.Value = 1000.00m;
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
		
		[Test]
		public void DoubleParameterTest ()
		{
			string query = "select id, type_double from numeric_family where type_double = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = 1.79E+308;
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void ImageParameterTest ()
		{
			string query = "insert into binary_family (id, type_blob) values (6000,?)";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_blob", OdbcType.Image);
				param.Value = new byte [] { 6, 6, 6, 6, 6, 6, 6, 6 };
				cmd.ExecuteNonQuery ();
				cmd.CommandText = "select count (*) from binary_family where id = 6000";
				int count = (int) cmd.ExecuteScalar ();
				Assert.AreEqual (1, count, "#1 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
				ConnectionManager.Singleton.OpenConnection ();
				DBHelper.ExecuteNonQuery (conn, "delete from binary_family where id = 6000");
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void NCharParameterTest ()
		{
			string query = "select id, type_char from string_family where type_char = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_char", OdbcType.NChar);
				param.Value = "char";
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void NTextParameterTest ()
		{
			string query = "insert into string_family (id, type_ntext) values (6000, ?)";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_ntext", OdbcType.NText);
				param.Value = "ntext";
				cmd.ExecuteNonQuery ();
				cmd.CommandText = "select count (*) from string_family where id = 6000";
				Assert.AreEqual (1,(int) cmd.ExecuteScalar (), "#1 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
				ConnectionManager.Singleton.OpenConnection ();
				DBHelper.ExecuteNonQuery (conn, "delete from string_family where id = 6000");
				ConnectionManager.Singleton.CloseConnection ();

			}
		}

		[Test]
		public void TextParameterTest ()
		{
			string query = "insert into string_family (id, type_text) values (6000, ?)";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_text", OdbcType.Text);
				param.Value = "text";
				cmd.ExecuteNonQuery ();
				cmd.CommandText = "select count (*) from string_family where id = 6000";
				Assert.AreEqual (1,(int) cmd.ExecuteScalar (), "#1 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
				ConnectionManager.Singleton.OpenConnection ();
				DBHelper.ExecuteNonQuery (conn, "delete from string_family where id = 6000");
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void NumericParameterTest ()
		{
			string query = "select id, type_numeric from numeric_family where type_numeric = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_numeric", OdbcType.Numeric);
				param.Precision = 0;
				param.Scale = 0;
				param.Value = 1000.00m;
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void NVarCharParameterTest ()
		{
			string query = "select id, type_varchar from string_family where type_varchar = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_varchar", OdbcType.NVarChar);
				param.Value = "varchar";
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]

		public void VarCharParameterTest ()
		{
			string query = "select id, type_varchar from string_family where type_varchar = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_varchar", OdbcType.VarChar);
				param.Value = "varchar";
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void RealParameterTest ()
		{
			string query = "select id, type_float from numeric_family where type_float = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_float", OdbcType.Real);
				param.Value = 3.40E+38;
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void SmallDateTimeParameterTest ()
		{
			string query = "select id, type_smalldatetime from datetime_family where type_smalldatetime = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_smalldatetime", OdbcType.SmallDateTime);
				param.Value = DateTime.Parse ("2079-06-06 23:59:00");
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		public void DateTimeParameterTest ()
		{
			string query = "select id, type_datetime from datetime_family where type_datetime = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_datetime", OdbcType.DateTime);
				param.Value = DateTime.ParseExact ("9999-12-31 23:59:59.997", "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		[Test]
		[Ignore ("Not running on ms.net")]
		public void DateParameterTest ()
		{
			string query = "select id, type_datetime from datetime_family where type_datetime = ? and id = 1";
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("type_datetime", OdbcType.Date);
				param.Value = DateTime.ParseExact ("9999-12-31 23:59:59.997", "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
				OdbcDataReader reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read (), "#1 no data to test");
				Assert.AreEqual (1, Convert.ToInt32 (reader [0]), "#2 value not matching");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
	}
}