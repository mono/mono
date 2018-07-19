// OdbcCommandTest.cs - NUnit Test Cases for testing the
// OdbcCommand class
//
// Authors:
//      Sureshkumar T (TSureshkumar@novell.com)
//      Gert Driesen (drieseng@users.sourceforge.net)
// 
// Copyright (c) 2005-2008 Novell Inc., and the individuals listed
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

#if !NO_ODBC

using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Globalization;
using System.Text;
using System.Threading;
using NUnit.Framework;


namespace MonoTests.System.Data.Connected.Odbc
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcParameterTest
	{
		private CultureInfo _originalCulture;

		[SetUp]
		public void Setup ()
		{
			_originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("nl-BE");
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = _originalCulture;
		}

		[Test]
		public void IntegerParamTest ()
		{
			string insert_data = "insert into numeric_family (id, type_int) values (6000, ?)";
			string select_data = "select id, type_int from numeric_family where type_int = ? and id = ?";
			string select_by_id = "select id, type_int from numeric_family where id = ?";
			string delete_data = "delete from numeric_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_int", OdbcType.Int);
				param.Value = int.MaxValue;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A2");
				Assert.AreEqual (typeof (int), dr.GetFieldType (1), "#A3");
				Assert.AreEqual (int.MaxValue, dr.GetValue (1), "#A4");
				Assert.IsFalse (dr.Read (), "#A5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_int", OdbcType.Int);
				param.Value = int.MinValue;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B2");
				Assert.AreEqual (typeof (int), dr.GetFieldType (1), "#B3");
				Assert.AreEqual (int.MinValue, dr.GetValue (1), "#B4");
				Assert.IsFalse (dr.Read (), "#B5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_int", OdbcType.Int);
				param.Value = 0;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (3, dr.GetValue (0), "#C2");
				Assert.AreEqual (typeof (int), dr.GetFieldType (1), "#C3");
				Assert.AreEqual (0, dr.GetValue (1), "#C4");
				Assert.IsFalse (dr.Read (), "#C5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (4, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (int), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_int", OdbcType.Int);
				param.Value = int.MaxValue;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (6000, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (int), dr.GetFieldType (1), "#E4");
				Assert.AreEqual (int.MaxValue, dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_int", OdbcType.Int);
				param.Value = int.MinValue;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (6000, dr.GetValue (0), "#F3");
				Assert.AreEqual (typeof (int), dr.GetFieldType (1), "#F4");
				Assert.AreEqual (int.MinValue, dr.GetValue (1), "#F5");
				Assert.IsFalse (dr.Read (), "#F6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_int", OdbcType.Int);
				param.Value = 0;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (6000, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (int), dr.GetFieldType (1), "#G4");
				Assert.AreEqual (0, dr.GetValue (1), "#G5");
				Assert.IsFalse (dr.Read (), "#G6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_int", OdbcType.Int);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (6000, dr.GetValue (0), "#H3");
				Assert.AreEqual (typeof (int), dr.GetFieldType (1), "#H4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#H5");
				Assert.IsFalse (dr.Read (), "#H6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void BigIntParamTest ()
		{
			string insert_data = "insert into numeric_family (id, type_bigint) values (6000, ?)";
			string select_data = "select id, type_bigint from numeric_family where type_bigint = ? and id = ?";
			string select_by_id = "select id, type_bigint from numeric_family where id = ?";
			string delete_data = "delete from numeric_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = 9223372036854775807L;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A3");
				Assert.AreEqual (typeof (long), dr.GetFieldType (1), "#A4");
				Assert.AreEqual (9223372036854775807L, dr.GetValue (1), "#A5");
				Assert.IsFalse (dr.Read (), "#A6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = -9223372036854775808L;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B3");
				Assert.AreEqual (typeof (long), dr.GetFieldType (1), "#B4");
				Assert.AreEqual (-9223372036854775808L, dr.GetValue (1), "#B5");
				Assert.IsFalse (dr.Read (), "#B6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = 0L;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (3, dr.GetValue (0), "#C3");
				Assert.AreEqual (typeof (long), dr.GetFieldType (1), "#C4");
				Assert.AreEqual (0L, dr.GetValue (1), "#C5");
				Assert.IsFalse (dr.Read (), "#C6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (4, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (long), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = 8223372036854775805L;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = 8223372036854775805L;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (6000, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (long), dr.GetFieldType (1), "#E4");
				Assert.AreEqual (8223372036854775805L, dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = -8223372036854775805L;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = -8223372036854775805L;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (6000, dr.GetValue (0), "#F3");
				Assert.AreEqual (typeof (long), dr.GetFieldType (1), "#F4");
				Assert.AreEqual (-8223372036854775805L, dr.GetValue (1), "#F5");
				Assert.IsFalse (dr.Read (), "#F6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = 0L;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = 0;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (6000, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (long), dr.GetFieldType (1), "#G4");
				Assert.AreEqual (0L, dr.GetValue (1), "#G5");
				Assert.IsFalse (dr.Read (), "#G6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_bigint", OdbcType.BigInt);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (6000, dr.GetValue (0), "#H3");
				Assert.AreEqual (typeof (long), dr.GetFieldType (1), "#H4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#H5");
				Assert.IsFalse (dr.Read (), "#H6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void BinaryParameterTest ()
		{
			string insert_data = "insert into binary_family (id, type_binary) values (6000, ?)";
			string select_data = "select id, type_binary from binary_family where type_binary = ? and id = ?";
			string select_by_id = "select id, type_binary from binary_family where id = ?";
			string delete_data = "delete from binary_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				byte [] bytes = new byte [] { 0x35, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00, 0x00 };

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_binary", OdbcType.Binary);
				param.Value = bytes;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#A4");
				Assert.AreEqual (bytes, dr.GetValue (1), "#A5");
				Assert.IsFalse (dr.Read (), "#A6");
				dr.Close ();
				cmd.Dispose ();

				bytes = new byte [] { 0x00, 0x33, 0x34, 0x00,
					0x33, 0x30, 0x35, 0x31 };

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_binary", OdbcType.Binary);
				param.Value = bytes;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#B4");
				Assert.AreEqual (bytes, dr.GetValue (1), "#B5");
				Assert.IsFalse (dr.Read (), "#B6");
				dr.Close ();
				cmd.Dispose ();

				bytes = new byte [8];

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_binary", OdbcType.Binary);
				param.Value = bytes;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (3, dr.GetValue (0), "#C3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#C4");
				Assert.AreEqual (bytes, dr.GetValue (1), "#C5");
				Assert.IsFalse (dr.Read (), "#C6");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int); 
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (4, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				bytes = new byte [0];

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_binary", OdbcType.Binary);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (6000, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#E4");
				Assert.AreEqual (new byte [8], dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				bytes = new byte [] { 0x05 };

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_binary", OdbcType.Binary);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (6000, dr.GetValue (0), "#F3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#F4");
				Assert.AreEqual (new byte [] { 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, dr.GetValue (1), "#F5");
				Assert.IsFalse (dr.Read (), "#F6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				bytes = new byte [] { 0x34, 0x00, 0x32 };

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_binary", OdbcType.Binary);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (6000, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#G4");
				Assert.AreEqual (new byte [] { 0x34, 0x00, 0x32, 0x00, 0x00, 0x00, 0x00, 0x00 }, dr.GetValue (1), "#G5");
				Assert.IsFalse (dr.Read (), "#G6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				bytes = new byte [] { 0x34, 0x00, 0x32, 0x05, 0x07, 0x13 };

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_binary", OdbcType.Binary, 4);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (6000, dr.GetValue (0), "#H3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#H4");
				Assert.AreEqual (new byte [] { 0x34, 0x00, 0x32, 0x05, 0x00, 0x00, 0x00, 0x00 }, dr.GetValue (1), "#H5");
				Assert.IsFalse (dr.Read (), "#H6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_binary", OdbcType.Binary);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#I1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#I2");
				Assert.AreEqual (6000, dr.GetValue (0), "#I3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#I4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#I5");
				Assert.IsFalse (dr.Read (), "#I6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void SmallIntParamTest ()
		{
			string insert_data = "insert into numeric_family (id, type_smallint) values (6000, ?)";
			string select_data = "select id, type_smallint from numeric_family where type_smallint = ? and id = ?";
			string select_by_id = "select id, type_smallint from numeric_family where id = ?";
			string delete_data = "delete from numeric_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_smallint", OdbcType.SmallInt);
				param.Value = short.MaxValue;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A2");
				Assert.AreEqual (typeof (short), dr.GetFieldType (1), "#A3");
				Assert.AreEqual (short.MaxValue, dr.GetValue (1), "#A4");
				Assert.IsFalse (dr.Read (), "#A5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_smallint", OdbcType.SmallInt);
				param.Value = short.MinValue;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B2");
				Assert.AreEqual (typeof (short), dr.GetFieldType (1), "#B3");
				Assert.AreEqual (short.MinValue, dr.GetValue (1), "#B4");
				Assert.IsFalse (dr.Read (), "#B5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_smallint", OdbcType.SmallInt);
				param.Value = (short) 0;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (3, dr.GetValue (0), "#C2");
				Assert.AreEqual (typeof (short), dr.GetFieldType (1), "#C3");
				Assert.AreEqual (0, dr.GetValue (1), "#C4");
				Assert.IsFalse (dr.Read (), "#C5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (4, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (short), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_smallint", OdbcType.SmallInt);
				param.Value = short.MaxValue;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (6000, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (short), dr.GetFieldType (1), "#E4");
				Assert.AreEqual (short.MaxValue, dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_smallint", OdbcType.SmallInt);
				param.Value = short.MinValue;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (6000, dr.GetValue (0), "#F3");
				Assert.AreEqual (typeof (short), dr.GetFieldType (1), "#F4");
				Assert.AreEqual (short.MinValue, dr.GetValue (1), "#F5");
				Assert.IsFalse (dr.Read (), "#F6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_smallint", OdbcType.SmallInt);
				param.Value = 0;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (6000, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (short), dr.GetFieldType (1), "#G4");
				Assert.AreEqual ((short) 0, dr.GetValue (1), "#G5");
				Assert.IsFalse (dr.Read (), "#G6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_smallint", OdbcType.SmallInt);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (6000, dr.GetValue (0), "#H3");
				Assert.AreEqual (typeof (short), dr.GetFieldType (1), "#H4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#H5");
				Assert.IsFalse (dr.Read (), "#H6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void TimestampParameterTest ()
		{
			// the value for the timestamp column is determined by
			// the RDBMS upon insert/update and cannot be specified
			// by the user

			string insert_data = "insert into binary_family (id) values (6000)";
			string select_by_id = "select id, type_timestamp from binary_family where id = ?";
			string delete_data = "delete from binary_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				if (!ConnectionManager.Instance.Odbc.EngineConfig.SupportsTimestamp)
					Assert.Ignore ("Timestamp test does not apply to the current driver (" + conn.Driver + ").");

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				byte [] timestamp;

				cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT @@DBTS";
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (0), "#A2");
				timestamp = (byte []) dr.GetValue (0);
				Assert.IsFalse (dr.Read (), "#A3");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (6000, dr.GetValue (0), "#B3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#B4");
				Assert.AreEqual (timestamp, dr.GetValue (1), "#B5");
				Assert.IsFalse (dr.Read (), "#B6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();

				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void TinyIntParamTest ()
		{
			string insert_data = "insert into numeric_family (id, type_tinyint) values (6000, ?)";
			string select_data = "select id, type_tinyint from numeric_family where type_tinyint = ? and id = ?";
			string select_by_id = "select id, type_tinyint from numeric_family where id = ?";
			string delete_data = "delete from numeric_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_tinyint", OdbcType.TinyInt);
				param.Value = byte.MaxValue;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A2");
				Assert.AreEqual (typeof (byte), dr.GetFieldType (1), "#A3");
				Assert.AreEqual (byte.MaxValue, dr.GetValue (1), "#A4");
				Assert.IsFalse (dr.Read (), "#A5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_tinyint", OdbcType.TinyInt);
				param.Value = byte.MinValue;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B2");
				Assert.AreEqual (typeof (byte), dr.GetFieldType (1), "#B3");
				Assert.AreEqual (byte.MinValue, dr.GetValue (1), "#B4");
				Assert.IsFalse (dr.Read (), "#B5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_tinyint", OdbcType.TinyInt);
				param.Value = 0x00;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (3, dr.GetValue (0), "#C2");
				Assert.AreEqual (typeof (byte), dr.GetFieldType (1), "#C3");
				Assert.AreEqual (0x00, dr.GetValue (1), "#C4");
				Assert.IsFalse (dr.Read (), "#C5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (4, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (byte), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_tinyint", OdbcType.TinyInt);
				param.Value = byte.MaxValue;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (6000, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (byte), dr.GetFieldType (1), "#E4");
				Assert.AreEqual (byte.MaxValue, dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_tinyint", OdbcType.TinyInt);
				param.Value = byte.MinValue;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (6000, dr.GetValue (0), "#F3");
				Assert.AreEqual (typeof (byte), dr.GetFieldType (1), "#F4");
				Assert.AreEqual (byte.MinValue, dr.GetValue (1), "#F5");
				Assert.IsFalse (dr.Read (), "#F6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_tinyint", OdbcType.TinyInt);
				param.Value = 0x00;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (6000, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (byte), dr.GetFieldType (1), "#G4");
				Assert.AreEqual (0x00, dr.GetValue (1), "#G5");
				Assert.IsFalse (dr.Read (), "#G6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_tinyint", OdbcType.TinyInt);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (6000, dr.GetValue (0), "#H3");
				Assert.AreEqual (typeof (byte), dr.GetFieldType (1), "#H4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#H5");
				Assert.IsFalse (dr.Read (), "#H6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void StringParamTest ()
		{
			string query = "select id, fname from employee where fname = ?";
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try {
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = query;

				OdbcParameter param = cmd.Parameters.Add ("fname", OdbcType.VarChar);
				param.Value = "suresh";
				OdbcDataReader dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#1 no data to test");
				Assert.AreEqual (1, (int) dr [0], "#2 value not matching");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}


		[Test]
		public void BitParameterTest ()
		{
			string insert_data = "insert into numeric_family (id, type_bit) values (6000, ?)";
			string select_data = "select id, type_bit from numeric_family where type_bit = ? and id = ?";
			string select_by_id = "select id, type_bit from numeric_family where id = ?";
			string delete_data = "delete from numeric_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_bit", OdbcType.Bit);
				param.Value = true;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A2");
				Assert.AreEqual (typeof (bool), dr.GetFieldType (1), "#A3");
				Assert.AreEqual (true, dr.GetValue (1), "#A4");
				Assert.IsFalse (dr.Read (), "#A5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_bit", OdbcType.Bit);
				param.Value = false;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B2");
				Assert.AreEqual (typeof (bool), dr.GetFieldType (1), "#B3");
				Assert.AreEqual (false, dr.GetValue (1), "#B4");
				Assert.IsFalse (dr.Read (), "#B5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (4, dr.GetValue (0), "#C3");
				Assert.AreEqual (typeof (bool), dr.GetFieldType (1), "#C4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#C5");
				Assert.IsFalse (dr.Read (), "#C6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_bit", OdbcType.Bit);
				param.Value = true;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_bit", OdbcType.Bit);
				param.Value = true;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (6000, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (bool), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (true, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_bit", OdbcType.Bit);
				param.Value = false;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_bit", OdbcType.Bit);
				param.Value = false;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (6000, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (bool), dr.GetFieldType (1), "#E4");
				Assert.AreEqual (false, dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_bit", OdbcType.Bit);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (6000, dr.GetValue (0), "#F3");
				Assert.AreEqual (typeof (bool), dr.GetFieldType (1), "#F4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#F5");
				Assert.IsFalse (dr.Read (), "#F6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void CharParameterTest ()
		{
			string insert_data = "insert into string_family (id, type_char) values (6000, ?)";
			string select_data = "select id, type_char from string_family where type_char = ? and id = ?";
			string select_by_id = "select id, type_char from string_family where id = ?";
			string delete_data = "delete from string_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_char", OdbcType.Char);
				param.Value = "char";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A3");
				Assert.AreEqual (typeof (string), dr.GetFieldType (1), "#A4");
				if (ConnectionManager.Instance.Odbc.EngineConfig.RemovesTrailingSpaces)
					Assert.AreEqual ("char", dr.GetValue (1), "#A5");
				else
					Assert.AreEqual ("char      ", dr.GetValue (1), "#A5");
				Assert.IsFalse (dr.Read (), "#A6");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_char", OdbcType.Char);
				param.Value = "0123456789";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B3");
				Assert.AreEqual (typeof (string), dr.GetFieldType (1), "#B4");
				Assert.AreEqual ("0123456789", dr.GetValue (1), "#B5");
				Assert.IsFalse (dr.Read (), "#B6");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_char", OdbcType.Char);
				param.Value = string.Empty;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (3, dr.GetValue (0), "#C3");
				Assert.AreEqual (typeof (string), dr.GetFieldType (1), "#C4");
				if (ConnectionManager.Instance.Odbc.EngineConfig.RemovesTrailingSpaces)
					Assert.AreEqual (string.Empty, dr.GetValue (1), "#C5");
				else
					Assert.AreEqual ("          ", dr.GetValue (1), "#C5");
				Assert.IsFalse (dr.Read (), "#C6");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (4, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (string), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_char", OdbcType.Char, 3);
				param.Value = "ABCD";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_char", OdbcType.Char, 3);
				param.Value = "ABCE ";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (6000, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (string), dr.GetFieldType (1), "#E4");
				if (ConnectionManager.Instance.Odbc.EngineConfig.RemovesTrailingSpaces)
					Assert.AreEqual ("ABC", dr.GetValue (1), "#E5");
				else
					Assert.AreEqual ("ABC       ", dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_char", OdbcType.Char, 20);
				param.Value = "ABCDEFGHIJ";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_char", OdbcType.Char);
				param.Value = "ABCDEFGHIJ";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (6000, dr.GetValue (0), "#F3");
				Assert.AreEqual (typeof (string), dr.GetFieldType (1), "#F4");
				Assert.AreEqual ("ABCDEFGHIJ", dr.GetValue (1), "#F5");
				Assert.IsFalse (dr.Read (), "#F6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_char", OdbcType.Char, 20);
				param.Value = string.Empty;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_char", OdbcType.Char);
				param.Value = string.Empty;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (6000, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (string), dr.GetFieldType (1), "#G4");
				if (ConnectionManager.Instance.Odbc.EngineConfig.RemovesTrailingSpaces)
					Assert.AreEqual (string.Empty, dr.GetValue (1), "#G5");
				else
					Assert.AreEqual ("          ", dr.GetValue (1), "#G5");
				Assert.IsFalse (dr.Read (), "#G6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_char", OdbcType.Char);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (4, dr.GetValue (0), "#H3");
				Assert.AreEqual (typeof (string), dr.GetFieldType (1), "#H4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#H5");
				Assert.IsFalse (dr.Read (), "#H6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void DecimalParameterTest ()
		{
			string select_data1 = "select id, type_decimal1 from numeric_family where type_decimal1 = ? and id = ?";
			string select_data2 = "select id, type_decimal2 from numeric_family where type_decimal2 = ? and id = ?";
			string select_by_id = "select id, type_decimal1, type_decimal2 from numeric_family where id = ?";
			string insert_data = "insert into numeric_family (id, type_decimal1, type_decimal2) values (6000, ?, ?)";
			string delete_data = "delete from numeric_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_decimal1", OdbcType.Decimal);
				param.Value = 1000.00m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#A4");
				Assert.AreEqual (1000m, dr.GetValue (1), "#A5");
				Assert.IsFalse (dr.Read (), "#A6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_decimal1", OdbcType.Decimal);
				param.Value = -1000.00m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#B4");
				Assert.AreEqual (-1000m, dr.GetValue (1), "#B5");
				Assert.IsFalse (dr.Read (), "#B6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data2;
				param = cmd.Parameters.Add ("type_decimal2", OdbcType.Decimal);
				param.Value = 4456.432m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (1, dr.GetValue (0), "#C3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#C4");
				Assert.AreEqual (4456.432m, dr.GetValue (1), "#C5");
				Assert.IsFalse (dr.Read (), "#C6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data2;
				param = cmd.Parameters.Add ("type_decimal2", OdbcType.Decimal);
				param.Value = -4456.432m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (2, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (-4456.432m, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_decimal1", OdbcType.Decimal);
				param.Value = 0m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (3, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#E4");
				Assert.AreEqual (0m, dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_decimal1", OdbcType.Decimal);
				param.Value = DBNull.Value;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsFalse (dr.Read (), "#F");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (4, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#G4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#G5");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (2), "#G6");
				Assert.AreEqual (DBNull.Value, dr.GetValue (2), "#G7");
				Assert.IsFalse (dr.Read (), "#G8");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_decimal1", OdbcType.Decimal);
				param.Value = -1000.5678m;
				param = cmd.Parameters.Add ("type_decimal2", OdbcType.Decimal);
				param.Value = -1000.5678m;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_decimal1", OdbcType.Decimal);
				param.Value = -1000.5678m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsFalse (dr.Read (), "#H");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_decimal1", OdbcType.Decimal);
				param.Value = -1001;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#I1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#I2");
				Assert.AreEqual (6000, dr.GetValue (0), "#I3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#I4");
				Assert.AreEqual (-1001m, dr.GetValue (1), "#I5");
				Assert.IsFalse (dr.Read (), "#I6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data2;
				param = cmd.Parameters.Add ("type_decimal2", OdbcType.Decimal);
				param.Value = -1000.5678m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsFalse (dr.Read (), "#J");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data2;
				param = cmd.Parameters.Add ("type_decimal2", OdbcType.Decimal);
				param.Value = -1000.568m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#K1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#K2");
				Assert.AreEqual (6000, dr.GetValue (0), "#K3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#K4");
				Assert.AreEqual (-1000.568m, dr.GetValue (1), "#K5");
				Assert.IsFalse (dr.Read (), "#K6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_decimal1", OdbcType.Decimal);
				param.Value = 56789m;
				param.Precision = 7;
				param.Scale = 2;
				param = cmd.Parameters.Add ("type_decimal2", OdbcType.Decimal);
				param.Value = 98765.5678m;
				param.Precision = 10;
				param.Scale = 2;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#L1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#L2");
				Assert.AreEqual (6000, dr.GetValue (0), "#L3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#L4");
				Assert.AreEqual (56789m, dr.GetValue (1), "#L5");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (2), "#L6");
				if (ConnectionManager.Instance.Odbc.EngineConfig.Type == EngineType.MySQL)
					Assert.AreEqual (9876556.780m, dr.GetValue (2), "#L7");
				else
					Assert.AreEqual (98765.570m, dr.GetValue (2), "#L7");
				Assert.IsFalse (dr.Read (), "#L8");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_decimal1", OdbcType.Decimal);
				param.Value = DBNull.Value;
				param = cmd.Parameters.Add ("type_decimal2", OdbcType.Decimal);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#M1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#M2");
				Assert.AreEqual (6000, dr.GetValue (0), "#M3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#M4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#M5");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (2), "#M6");
				Assert.AreEqual (DBNull.Value, dr.GetValue(1), "#M7");
				Assert.AreEqual (DBNull.Value, dr.GetValue (2), "#M8");
				Assert.IsFalse (dr.Read (), "#M9");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (dr != null)
					dr.Close ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}
		
		[Test]
		public void DoubleParameterTest ()
		{
			string insert_data = "insert into numeric_family (id, type_double) values (6000, ?)";
			string select_data = "select id, type_double from numeric_family where type_double = ? and id = ?";
			string select_by_id = "select id, type_double from numeric_family where id = ?";
			string delete_data = "delete from numeric_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = 1.79E+308;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A3");
				Assert.AreEqual (typeof (double), dr.GetFieldType (1), "#A4");
				Assert.AreEqual (1.79E+308, dr.GetValue (1), "#A5");
				Assert.IsFalse (dr.Read (), "#A6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = -1.79E+308;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B3");
				Assert.AreEqual (typeof (double), dr.GetFieldType (1), "#B4");
				Assert.AreEqual (-1.79E+308, dr.GetValue (1), "#B5");
				Assert.IsFalse (dr.Read (), "#B6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = 0d;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (3, dr.GetValue (0), "#C3");
				Assert.AreEqual (typeof (double), dr.GetFieldType (1), "#C4");
				Assert.AreEqual (0d, dr.GetValue (1), "#C5");
				Assert.IsFalse (dr.Read (), "#C6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (4, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (double), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = 1.79E+308;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = 1.79E+308;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (6000, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (double), dr.GetFieldType (1), "#E4");
				Assert.AreEqual (1.79E+308, dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = -1.79E+308;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = -1.79E+308;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (6000, dr.GetValue (0), "#F3");
				Assert.AreEqual (typeof (double), dr.GetFieldType (1), "#F4");
				Assert.AreEqual (-1.79E+308, dr.GetValue (1), "#F5");
				Assert.IsFalse (dr.Read (), "#F6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = "45543,55";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = "45543,55";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (6000, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (double), dr.GetFieldType (1), "#G4");
				Assert.AreEqual (45543.55d, dr.GetValue (1), "#G5");
				Assert.IsFalse (dr.Read (), "#G6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_double", OdbcType.Double);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (6000, dr.GetValue (0), "#H3");
				Assert.AreEqual (typeof (double), dr.GetFieldType (1), "#H4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#H5");
				Assert.IsFalse (dr.Read (), "#H6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void ImageParameterTest ()
		{
			string insert_data = "insert into binary_family (id, type_blob) values (6000, ?)";
			string select_data = "select type_blob from binary_family where id = ?";
			string delete_data = "delete from binary_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				byte [] bytes = new byte [] { 0x32, 0x56, 0x00,
					0x44, 0x22 };

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (bytes, dr.GetValue (0), "#A3");
				Assert.IsFalse (dr.Read (), "#A4");
				dr.Close ();
				cmd.Dispose ();

				bytes = new byte [] { 0x00, 0x66, 0x06, 0x66,
					0x97, 0x00, 0x66, 0x06, 0x66, 0x97, 0x06,
					0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
					0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
					0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
					0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
					0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
					0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
					0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
					0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
					0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
					0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
					0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
					0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
					0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
					0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
					0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
					0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
					0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
					0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
					0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
					0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
					0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
					0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
					0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
					0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
					0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
					0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
					0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
					0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
					0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
					0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
					0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
					0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
					0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
					0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
					0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
					0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
					0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
					0x06, 0x66, 0x06, 0x66, 0x98};

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (bytes, dr.GetValue (0), "#B3");
				Assert.IsFalse (dr.Read (), "#B4");
				dr.Close ();
				cmd.Dispose ();

				bytes = new byte [8];

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (new byte [0], dr.GetValue (0), "#C3");
				Assert.IsFalse (dr.Read (), "#C4");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#D3");
				Assert.IsFalse (dr.Read (), "#D4");
				dr.Close ();
				cmd.Dispose ();

				bytes = new byte [0];

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_blob", OdbcType.Image);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (bytes, dr.GetValue (0), "#E3");
				Assert.IsFalse (dr.Read (), "#E4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				bytes = new byte [] { 0x05 };

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_blob", OdbcType.Image);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (bytes, dr.GetValue (0), "#F3");
				Assert.IsFalse (dr.Read (), "#F4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				bytes = new byte [] { 0x34, 0x00, 0x32 };

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_blob", OdbcType.Image);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (bytes, dr.GetValue (0), "#G3");
				Assert.IsFalse (dr.Read (), "#G4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				bytes = new byte [] { 0x34, 0x00, 0x32, 0x05, 0x07, 0x13 };

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_blob", OdbcType.Image, 4);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (new byte [] { 0x34, 0x00, 0x32, 0x05 }, dr.GetValue (0), "#H3");
				Assert.IsFalse (dr.Read (), "#H4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_blob", OdbcType.Image);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#I1");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (0), "#I2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#I3");
				Assert.IsFalse (dr.Read (), "#I4");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void NCharParameterTest ()
		{
			string insert_data = "insert into string_family (id, type_nchar) values (6000, ?)";
			string select_data = "select type_nchar from string_family where type_nchar = ? and id = ?";
			string select_by_id = "select type_nchar from string_family where id = ?";
			string delete_data = "delete from string_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar);
				param.Value = "nch\u092d\u093er";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#A2");
				if (ConnectionManager.Instance.Odbc.EngineConfig.RemovesTrailingSpaces)
					Assert.AreEqual ("nch\u092d\u093er", dr.GetValue (0), "#A3");
				else
					Assert.AreEqual ("nch\u092d\u093er    ", dr.GetValue (0), "#A3");
				Assert.IsFalse (dr.Read (), "#A4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar);
				param.Value = "0123456789";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#B2");
				Assert.AreEqual ("0123456789", dr.GetValue (0), "#B3");
				Assert.IsFalse (dr.Read (), "#B4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar);
				param.Value = string.Empty;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#C2");
				if (ConnectionManager.Instance.Odbc.EngineConfig.RemovesTrailingSpaces)
					Assert.AreEqual (string.Empty, dr.GetValue (0), "#C3");
				else
					Assert.AreEqual ("          ", dr.GetValue (0), "#C3");
				Assert.IsFalse (dr.Read (), "#C4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#D3");
				Assert.IsFalse (dr.Read (), "#D4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar);
				param.Value = "nchar";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar);
				param.Value = "nchar";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#E2");
				if (ConnectionManager.Instance.Odbc.EngineConfig.RemovesTrailingSpaces)
					Assert.AreEqual ("nchar", dr.GetValue (0), "#E3");
				else
					Assert.AreEqual ("nchar     ", dr.GetValue (0), "#E3");
				Assert.IsFalse (dr.Read (), "#E4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar, 4);
				param.Value = "nch\u0488r";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar, 4);
				param.Value = "nch\u0488r";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#F2");
				if (ConnectionManager.Instance.Odbc.EngineConfig.RemovesTrailingSpaces)
					Assert.AreEqual ("nch\u0488", dr.GetValue (0), "#F3");
				else
					Assert.AreEqual ("nch\u0488      ", dr.GetValue (0), "#F3");
				Assert.IsFalse (dr.Read (), "#F4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar, 8);
				param.Value = "ch\u0488r";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar);
				param.Value = "ch\u0488r";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#G2");
				if (ConnectionManager.Instance.Odbc.EngineConfig.RemovesTrailingSpaces)
					Assert.AreEqual ("ch\u0488r", dr.GetValue (0), "#G3");
				else
					Assert.AreEqual ("ch\u0488r      ", dr.GetValue (0), "#G3");
				Assert.IsFalse (dr.Read (), "#G4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar, 15);
				param.Value = "0123456789";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar);
				param.Value = "0123456789";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#H2");
				Assert.AreEqual ("0123456789", dr.GetValue (0), "#H3");
				Assert.IsFalse (dr.Read (), "#H4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar, 15);
				param.Value = string.Empty;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar);
				param.Value = string.Empty;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#I1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#I2");
				if (ConnectionManager.Instance.Odbc.EngineConfig.RemovesTrailingSpaces)
					Assert.AreEqual (string.Empty, dr.GetValue (0), "#I3");
				else
					Assert.AreEqual ("          ", dr.GetValue (0), "#I3");
				Assert.IsFalse (dr.Read (), "#I4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nchar", OdbcType.NChar);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#J1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#J2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#J3");
				Assert.IsFalse (dr.Read (), "#J4");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();;
			}
		}

		[Test]
		public void NTextParameterTest ()
		{
			string insert_data = "insert into string_family (id, type_ntext) values (6000, ?)";
			string select_by_id = "select type_ntext from string_family where id = ?";
			string delete_data = "delete from string_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#A2");
				Assert.AreEqual ("nt\u092d\u093ext", dr.GetValue (0), "#A3");
				Assert.IsFalse (dr.Read (), "#A4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#B2");
				Assert.AreEqual ("nt\u092d\u093ext ", dr.GetValue (0), "#B3");
				Assert.IsFalse (dr.Read (), "#B4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (string.Empty, dr.GetValue (0), "#C3");
				Assert.IsFalse (dr.Read (), "#C4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#D3");
				Assert.IsFalse (dr.Read (), "#D4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_ntext", OdbcType.NText, 4);
				param.Value = "nt\u0488xt";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#E2");
				Assert.AreEqual ("nt\u0488x", dr.GetValue (0), "#E3");
				Assert.IsFalse (dr.Read (), "#E4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_ntext", OdbcType.NText, 15);
				param.Value = "nt\u0488xt ";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#F2");
				Assert.AreEqual ("nt\u0488xt ", dr.GetValue (0), "#F3");
				Assert.IsFalse (dr.Read (), "#F4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_ntext", OdbcType.NText, 8);
				param.Value = string.Empty;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (string.Empty, dr.GetValue (0), "#G3");
				Assert.IsFalse (dr.Read (), "#G4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_ntext", OdbcType.NText, 8);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#H3");
				Assert.IsFalse (dr.Read (), "#H4");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void TextParameterTest ()
		{
			string insert_data = "insert into string_family (id, type_text) values (6000, ?)";
			string select_by_id = "select type_text from string_family where id = ?";
			string delete_data = "delete from string_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#A2");
				Assert.AreEqual ("text", dr.GetValue (0), "#A3");
				Assert.IsFalse (dr.Read (), "#A4");
				dr.Close ();
				cmd.Dispose ();

				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < 30; i++)
					sb.Append ("longtext ");

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (sb.ToString (), dr.GetValue (0), "#B3");
				Assert.IsFalse (dr.Read (), "#B4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (string.Empty, dr.GetValue (0), "#C3");
				Assert.IsFalse (dr.Read (), "#C4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#D3");
				Assert.IsFalse (dr.Read (), "#D4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_text", OdbcType.Text);
				param.Value = sb.ToString ();
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (sb.ToString (), dr.GetValue (0), "#E3");
				Assert.IsFalse (dr.Read (), "#E4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_text", OdbcType.Text, 2);
				param.Value = "text";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#F2");
				Assert.AreEqual ("te", dr.GetValue (0), "#F3");
				Assert.IsFalse (dr.Read (), "#F4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_text", OdbcType.Text, 8);
				param.Value = string.Empty;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (string.Empty, dr.GetValue (0), "#G3");
				Assert.IsFalse (dr.Read (), "#G4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_text", OdbcType.Text, 8);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#H3");
				Assert.IsFalse (dr.Read (), "#H4");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void NumericParameterTest ()
		{
			string select_data1 = "select id, type_numeric1 from numeric_family where type_numeric1 = ? and id = ?";
			string select_data2 = "select id, type_numeric2 from numeric_family where type_numeric2 = ? and id = ?";
			string select_by_id = "select id, type_numeric1, type_numeric2 from numeric_family where id = ?";
			string insert_data = "insert into numeric_family (id, type_numeric1, type_numeric2) values (6000, ?, ?)";
			string delete_data = "delete from numeric_family where id = 6000";
			
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_numeric1", OdbcType.Numeric);
				param.Value = 1000.00m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#A4");
				Assert.AreEqual (1000m, dr.GetValue (1), "#A5");
				Assert.IsFalse (dr.Read (), "#A6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_numeric1", OdbcType.Numeric);
				param.Value = -1000.00m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#B4");
				Assert.AreEqual (-1000m, dr.GetValue (1), "#B5");
				Assert.IsFalse (dr.Read (), "#B6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data2;
				param = cmd.Parameters.Add ("type_numeric2", OdbcType.Numeric);
				param.Value = 4456.432m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (1, dr.GetValue (0), "#C3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#C4");
				Assert.AreEqual (4456.432m, dr.GetValue (1), "#C5");
				Assert.IsFalse (dr.Read (), "#C6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data2;
				param = cmd.Parameters.Add ("type_numeric2", OdbcType.Numeric);
				param.Value = -4456.432m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (2, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (-4456.432m, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_numeric1", OdbcType.Numeric);
				param.Value = 0m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (3, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#E4");
				Assert.AreEqual (0m, dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_numeric1", OdbcType.Numeric);
				param.Value = DBNull.Value;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsFalse (dr.Read (), "#F");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (4, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#G4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#G5");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (2), "#G6");
				Assert.AreEqual (DBNull.Value, dr.GetValue (2), "#G7");
				Assert.IsFalse (dr.Read (), "#G8");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_numeric1", OdbcType.Numeric);
				param.Value = -1000.5678m;
				param = cmd.Parameters.Add ("type_numeric2", OdbcType.Numeric);
				param.Value = -1000.5678m;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_numeric1", OdbcType.Numeric);
				param.Value = -1000.5678m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsFalse (dr.Read (), "#H");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data1;
				param = cmd.Parameters.Add ("type_numeric1", OdbcType.Numeric);
				param.Value = -1001;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#I1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#I2");
				Assert.AreEqual (6000, dr.GetValue (0), "#I3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#I4");
				Assert.AreEqual (-1001m, dr.GetValue (1), "#I5");
				Assert.IsFalse (dr.Read (), "#I6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data2;
				param = cmd.Parameters.Add ("type_numeric2", OdbcType.Numeric);
				param.Value = -1000.5678m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsFalse (dr.Read (), "#J");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data2;
				param = cmd.Parameters.Add ("type_numeric2", OdbcType.Numeric);
				param.Value = -1000.568m;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#K1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#K2");
				Assert.AreEqual (6000, dr.GetValue (0), "#K3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#K4");
				Assert.AreEqual (-1000.568m, dr.GetValue (1), "#K5");
				Assert.IsFalse (dr.Read (), "#K6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_numeric1", OdbcType.Numeric);
				param.Value = 56789m;
				param.Precision = 7;
				param.Scale = 2;
				param = cmd.Parameters.Add ("type_numeric2", OdbcType.Numeric);
				param.Value = 98765.5678m;
				param.Precision = 10;
				param.Scale = 2;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#L1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#L2");
				Assert.AreEqual (6000, dr.GetValue (0), "#L3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#L4");
				Assert.AreEqual (56789m, dr.GetValue (1), "#L5");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (2), "#L6");
				if (ConnectionManager.Instance.Odbc.EngineConfig.Type == EngineType.MySQL)
					Assert.AreEqual (9876556.780m, dr.GetValue (2), "#L7");
				else
					Assert.AreEqual (98765.570m, dr.GetValue (2), "#L7");
				Assert.IsFalse (dr.Read (), "#L8");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_numeric1", OdbcType.Numeric);
				param.Value = DBNull.Value;
				param = cmd.Parameters.Add ("type_numeric2", OdbcType.Numeric);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#M1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#M2");
				Assert.AreEqual (6000, dr.GetValue (0), "#M3");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (1), "#M4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#M5");
				Assert.AreEqual (typeof (decimal), dr.GetFieldType (2), "#M6");
				Assert.AreEqual (DBNull.Value, dr.GetValue(1), "#M7");
				Assert.AreEqual (DBNull.Value, dr.GetValue (2), "#M7");
				Assert.IsFalse (dr.Read (), "#M8");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (dr != null)
					dr.Close ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void NVarCharParameterTest ()
		{
			string insert_data = "insert into string_family (id, type_nvarchar) values (6000, ?)";
			string select_data = "select type_nvarchar from string_family where type_nvarchar = ? and id = ?";
			string select_by_id = "select type_nvarchar from string_family where id = ?";
			string delete_data = "delete from string_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar);
				param.Value = "nv\u092d\u093e\u0930\u0924r";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#A2");
				Assert.AreEqual ("nv\u092d\u093e\u0930\u0924r", dr.GetValue (0), "#A3");
				Assert.IsFalse (dr.Read (), "#A4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar);
				param.Value = "nv\u092d\u093e\u0930\u0924r ";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#B2");
				Assert.AreEqual ("nv\u092d\u093e\u0930\u0924r ", dr.GetValue (0), "#B3");
				Assert.IsFalse (dr.Read (), "#B4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar);
				param.Value = string.Empty;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (string.Empty, dr.GetValue (0), "#C3");
				Assert.IsFalse (dr.Read (), "#C4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#D3");
				Assert.IsFalse (dr.Read (), "#D4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar);
				param.Value = "nvarchar ";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar);
				param.Value = "nvarchar ";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#E2");
				Assert.AreEqual ("nvarchar ", dr.GetValue (0), "#E3");
				Assert.IsFalse (dr.Read (), "#E4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar, 6);
				param.Value = "nv\u0488rchar";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar, 6);
				param.Value = "nv\u0488rchar";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#F2");
				Assert.AreEqual ("nv\u0488rch", dr.GetValue (0), "#F3");
				Assert.IsFalse (dr.Read (), "#F4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar, 12);
				param.Value = "nvarch\u0488r ";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar, 12);
				param.Value = "nvarch\u0488r ";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#G2");
				Assert.AreEqual ("nvarch\u0488r ", dr.GetValue (0), "#G3");
				Assert.IsFalse (dr.Read (), "#G4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar, 12);
				param.Value = string.Empty;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar, 12);
				param.Value = string.Empty;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (string.Empty, dr.GetValue (0), "#H3");
				Assert.IsFalse (dr.Read (), "#H4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.NVarChar);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#I1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#I2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#I3");
				Assert.IsFalse (dr.Read (), "#I4");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (dr != null)
					dr.Close ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void VarBinaryParameterTest ()
		{
			string insert_data = "insert into binary_family (id, type_varbinary) values (6000, ?)";
			string select_data = "select id, type_varbinary from binary_family where type_varbinary = ? and id = ?";
			string select_by_id = "select id, type_varbinary from binary_family where id = ?";
			string delete_data = "delete from binary_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				byte [] bytes = new byte [] { 0x30, 0x31, 0x32,
					0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
					0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36,
					0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33,
					0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x00,
					0x44, 0x53};

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary);
				param.Value = bytes;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#A4");
				Assert.AreEqual (bytes, dr.GetValue (1), "#A5");
				Assert.IsFalse (dr.Read (), "#A6");
				dr.Close ();
				cmd.Dispose ();

				bytes = new byte [] { 0x00, 0x39, 0x38, 0x37,
					0x36, 0x35, 0x00, 0x33, 0x32, 0x31, 0x30,
					0x31, 0x32, 0x33, 0x34 };

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary);
				param.Value = bytes;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#B4");
				Assert.AreEqual (bytes, dr.GetValue (1), "#B5");
				Assert.IsFalse (dr.Read (), "#B6");
				dr.Close ();
				cmd.Dispose ();

				bytes = new byte [0];

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary);
				param.Value = bytes;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (3, dr.GetValue (0), "#C3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#C4");
				Assert.AreEqual (bytes, dr.GetValue (1), "#C5");
				Assert.IsFalse (dr.Read (), "#C6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (4, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				bytes = new byte [0];

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary);
				param.Value = bytes;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (6000, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#E4");
				Assert.AreEqual (bytes, dr.GetValue (1), "#E5");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				bytes = new byte [] { 0x05 };

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary);
				param.Value = bytes;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (6000, dr.GetValue (0), "#F3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#F4");
				Assert.AreEqual (bytes, dr.GetValue (1), "#F5");
				Assert.IsFalse (dr.Read (), "#F6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				bytes = new byte [] { 0x34, 0x00, 0x32 };

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary);
				param.Value = bytes;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (6000, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#G4");
				Assert.AreEqual (bytes, dr.GetValue (1), "#G5");
				Assert.IsFalse (dr.Read (), "#G6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				bytes = new byte [] { 0x34, 0x00, 0x32, 0x05, 0x07, 0x13 };

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary, 4);
				param.Value = bytes;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary, 4);
				param.Value = bytes;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (6000, dr.GetValue (0), "#H3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#H4");
				Assert.AreEqual (new byte [] { 0x34, 0x00, 0x32, 0x05 }, dr.GetValue (1), "#H5");
				Assert.IsFalse (dr.Read (), "#H6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_varbinary", OdbcType.VarBinary);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#I1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#I2");
				Assert.AreEqual (6000, dr.GetValue (0), "#I3");
				Assert.AreEqual (typeof (byte []), dr.GetFieldType (1), "#I4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#I5");
				Assert.IsFalse (dr.Read (), "#I6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void VarCharParameterTest ()
		{
			string insert_data = "insert into string_family (id, type_varchar) values (6000, ?)";
			string select_data = "select type_varchar from string_family where type_varchar = ? and id = ?";
			string select_by_id = "select type_varchar from string_family where id = ?";
			string delete_data = "delete from string_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varchar", OdbcType.VarChar);
				param.Value = "varchar";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#A2");
				Assert.AreEqual ("varchar", dr.GetValue (0), "#A3");
				Assert.IsFalse (dr.Read (), "#A4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varchar", OdbcType.VarChar);
				param.Value = "varchar ";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#B2");
				Assert.AreEqual ("varchar ", dr.GetValue (0), "#B3");
				Assert.IsFalse (dr.Read (), "#B4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varchar", OdbcType.VarChar);
				param.Value = string.Empty;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (string.Empty, dr.GetValue (0), "#C3");
				Assert.IsFalse (dr.Read (), "#C4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#D3");
				Assert.IsFalse (dr.Read (), "#D4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_varchar", OdbcType.VarChar, 30);
				param.Value = "varchar ";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_varchar", OdbcType.VarChar);
				param.Value = "varchar ";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#E2");
				Assert.AreEqual ("varchar ", dr.GetValue (0), "#E3");
				Assert.IsFalse (dr.Read (), "#E4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_varchar", OdbcType.VarChar, 3);
				param.Value = "vchar";
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.VarChar, 3);
				param.Value = "vcharxzer";
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#F2");
				Assert.AreEqual ("vch", dr.GetValue (0), "#F3");
				Assert.IsFalse (dr.Read (), "#F4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_varchar", OdbcType.VarChar, 3);
				param.Value = string.Empty;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_nvarchar", OdbcType.VarChar, 3);
				param.Value = string.Empty;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (string.Empty, dr.GetValue (0), "#G3");
				Assert.IsFalse (dr.Read (), "#G4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_varchar", OdbcType.VarChar, 5);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (string), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#H3");
				Assert.IsFalse (dr.Read (), "#H4");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (dr != null)
					dr.Close ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void RealParameterTest ()
		{
			string insert_data = "insert into numeric_family (id, type_float) values (6000, ?)";
			string select_data = "select id, type_float from numeric_family where type_float = ? and id = ?";
			string select_by_id = "select id, type_float from numeric_family where id = ?";
			string delete_data = "delete from numeric_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (1, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (float), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (3.39999995E+38f, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();


				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_float", OdbcType.Real);
				param.Value = 3.40E+38;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A2");
				Assert.AreEqual (typeof (float), dr.GetFieldType (1), "#A3");
				Assert.AreEqual (3.40E+38f, (float)dr.GetValue (1), 0.0000001f, "#A4");
				Assert.IsFalse (dr.Read (), "#A5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_float", OdbcType.Real);
				param.Value = -3.40E+38;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 2;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (2, dr.GetValue (0), "#B2");
				Assert.AreEqual (typeof (float), dr.GetFieldType (1), "#B3");
				Assert.AreEqual(-3.40E+38f, (float)dr.GetValue(1), 0.0000001f, "#B4");
				Assert.IsFalse (dr.Read (), "#B5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_float", OdbcType.Real);
				param.Value = 0F;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 3;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (3, dr.GetValue (0), "#C2");
				Assert.AreEqual (typeof (float), dr.GetFieldType (1), "#C3");
				Assert.AreEqual (0F, dr.GetValue (1), "#C4");
				Assert.IsFalse (dr.Read (), "#C5");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (4, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (float), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_float", OdbcType.Real);
				param.Value = 3.40E+38;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#E1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#E2");
				Assert.AreEqual (6000, dr.GetValue (0), "#E3");
				Assert.AreEqual (typeof (float), dr.GetFieldType (1), "#E4");
				Assert.AreEqual(3.40E+38f, (float)dr.GetValue(1), 0.0000001f, "#E4");
				Assert.IsFalse (dr.Read (), "#E6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_float", OdbcType.Real);
				param.Value = -3.40E+38;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#F1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#F2");
				Assert.AreEqual (6000, dr.GetValue (0), "#F3");
				Assert.AreEqual (typeof (float), dr.GetFieldType (1), "#F4");
				Assert.AreEqual (-3.40E+38f, (float)dr.GetValue(1), 0.0000001f, "#F4");
				Assert.IsFalse (dr.Read (), "#F6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_float", OdbcType.Real);
				param.Value = 0F;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#G1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#G2");
				Assert.AreEqual (6000, dr.GetValue (0), "#G3");
				Assert.AreEqual (typeof (float), dr.GetFieldType (1), "#G4");
				Assert.AreEqual (0F, dr.GetValue (1), "#G5");
				Assert.IsFalse (dr.Read (), "#G6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_float", OdbcType.Real);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#H1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#H2");
				Assert.AreEqual (6000, dr.GetValue (0), "#H3");
				Assert.AreEqual (typeof (float), dr.GetFieldType (1), "#H4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#H5");
				Assert.IsFalse (dr.Read (), "#H6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void SmallDateTimeParameterTest ()
		{
			string insert_data = "insert into datetime_family (id, type_smalldatetime) values (6000, ?)";
			string select_data = "select id, type_smalldatetime from datetime_family where type_smalldatetime = ? and id = ?";
			string select_by_id = "select id, type_smalldatetime from datetime_family where id = ?";
			string delete_data = "delete from datetime_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				DateTime date = DateTime.Parse ("2037-12-31 23:59:00");

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_smalldatetime", OdbcType.SmallDateTime);
				param.Value = date;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr.GetValue (0), "#A3");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (1), "#A4");
				Assert.AreEqual (date, dr.GetValue (1), "#A5");
				Assert.IsFalse (dr.Read (), "#A6");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (4, dr.GetValue (0), "#B3");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (1), "#B4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#B5");
				Assert.IsFalse (dr.Read (), "#B6");
				dr.Close ();
				cmd.Dispose ();

				date = new DateTime (1973, 8, 13, 17, 55, 00);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_smalldatetime", OdbcType.SmallDateTime);
				param.Value = date;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_smalldatetime", OdbcType.SmallDateTime);
				param.Value = date;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (6000, dr.GetValue (0), "#C3");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (1), "#C4");
				Assert.AreEqual (date, dr.GetValue (1), "#C5");
				Assert.IsFalse (dr.Read (), "#C6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_smalldatetime", OdbcType.SmallDateTime);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (6000, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();

				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void DateTimeParameterTest ()
		{
			string insert_data = "insert into datetime_family (id, type_datetime) values (6000, ?)";
			string select_data = "select id, type_datetime from datetime_family where type_datetime = ? and id = ?";
			string select_by_id = "select id, type_datetime from datetime_family where id = ?";
			string delete_data = "delete from datetime_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				DateTime date = DateTime.ParseExact ("9999-12-31 23:59:59",
					"yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_datetime", OdbcType.DateTime);
				param.Value = date;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (1, dr [0], "#A3");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (1), "#A4");
				if (ConnectionManager.Instance.Odbc.EngineConfig.SupportsMicroseconds)
					Assert.AreEqual (date, dr [1], "#A5");
				else
					Assert.AreEqual (new DateTime (9999, 12, 31, 23, 59, 59), dr [1], "#A5");
				Assert.IsFalse (dr.Read (), "#A6");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (4, dr.GetValue (0), "#B3");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (1), "#B4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#B5");
				Assert.IsFalse (dr.Read (), "#B6");
				dr.Close ();
				cmd.Dispose ();

				date = new DateTime (1973, 8, 13, 17, 54, 34);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_datetime", OdbcType.DateTime);
				param.Value = date;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_datetime", OdbcType.DateTime);
				param.Value = date;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (6000, dr.GetValue (0), "#C3");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (1), "#C4");
				Assert.AreEqual (new DateTime (1973, 8, 13, 17, 54, 34), dr.GetValue (1), "#C5");
				Assert.IsFalse (dr.Read (), "#C6");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_datetime", OdbcType.DateTime);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (int), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (6000, dr.GetValue (0), "#D3");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (1), "#D4");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D5");
				Assert.IsFalse (dr.Read (), "#D6");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void DateParameterTest ()
		{
			string insert_data = "insert into datetime_family (id, type_date) values (6000, ?)";
			string select_data = "select type_date from datetime_family where type_date = ? and id = 1";
			string select_by_id = "select type_date from datetime_family where id = ?";
			string delete_data = "delete from datetime_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				if (!ConnectionManager.Instance.Odbc.EngineConfig.SupportsDate)
					Assert.Ignore ("Date test does not apply to the current driver (" + conn.Driver + ").");

				DateTime date = new DateTime (9999, 12, 31);

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_date", OdbcType.DateTime);
				param.Value = date;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (date, dr.GetValue (0), "#A3");
				Assert.IsFalse (dr.Read (), "#A4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#B3");
				Assert.IsFalse (dr.Read (), "#B4");
				dr.Close ();
				cmd.Dispose ();

				date = new DateTime (2004, 2, 21, 4, 50, 7);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_date", OdbcType.Date);
				param.Value = date;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (new DateTime (2004, 2, 21), dr.GetValue (0), "#C3");
				Assert.IsFalse (dr.Read (), "#C4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_date", OdbcType.Date);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (DateTime), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#D3");
				Assert.IsFalse (dr.Read (), "#D4");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void TimeParameterTest ()
		{
			string insert_data = "insert into datetime_family (id, type_time) values (6000, ?)";
			string select_data = "select type_time from datetime_family where type_time = ? and id = 1";
			string select_by_id = "select type_time from datetime_family where id = ?";
			string delete_data = "delete from datetime_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				if (!ConnectionManager.Instance.Odbc.EngineConfig.SupportsTime)
					Assert.Ignore ("Time test does not apply to the current driver (" + conn.Driver + ").");

				TimeSpan time = ConnectionManager.Instance.Odbc.EngineConfig.SupportsMicroseconds ?
					new TimeSpan (23, 58, 59, 953) : new TimeSpan (23, 58, 59);

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_time", OdbcType.Time);
				param.Value = time;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (typeof (TimeSpan), dr.GetFieldType (0), "#A2");
				Assert.AreEqual (time, dr.GetValue (0), "#A3");
				Assert.IsFalse (dr.Read (), "#A4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (typeof (TimeSpan), dr.GetFieldType (0), "#B2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#B3");
				Assert.IsFalse (dr.Read (), "#B4");
				dr.Close ();
				cmd.Dispose ();

				time = new TimeSpan (23, 56, 43);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_time", OdbcType.Time);
				param.Value = time;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (typeof (TimeSpan), dr.GetFieldType (0), "#C2");
				Assert.AreEqual (time, dr.GetValue (0), "#C3");
				Assert.IsFalse (dr.Read (), "#C4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_time", OdbcType.Date);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (typeof (TimeSpan), dr.GetFieldType (0), "#D2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (0), "#D3");
				Assert.IsFalse (dr.Read (), "#D4");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();
				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void UniqueIdentifierParameterTest ()
		{
			string insert_data = "insert into string_family (id, type_guid) values (6000, ?)";
			string select_data = "select id, type_guid from string_family where type_guid = ? and id = ?";
			string select_by_id = "select id, type_guid from string_family where id = ?";
			string delete_data = "delete from string_family where id = 6000";

			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;

			IDataReader dr = null;
			OdbcCommand cmd = null;
			OdbcParameter param;

			try {
				if (!ConnectionManager.Instance.Odbc.EngineConfig.SupportsUniqueIdentifier)
					Assert.Ignore ("UniqueIdentifier test does not apply to the current driver (" + conn.Driver + ").");

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_guid", OdbcType.UniqueIdentifier);
				param.Value = new Guid ("d222a130-6383-4d36-ac5e-4e6b2591aabf");
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 1;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (1, dr.GetValue (0), "#A2");
				Assert.AreEqual (new Guid ("d222a130-6383-4d36-ac5e-4e6b2591aabf"), dr.GetValue (1), "#A3");
				Assert.IsFalse (dr.Read (), "#A4");
				dr.Close ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 4;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#B1");
				Assert.AreEqual (4, dr.GetValue (0), "#B2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#B3");
				Assert.IsFalse (dr.Read (), "#B4");
				dr.Close ();
				cmd.Dispose ();

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_guid", OdbcType.UniqueIdentifier);
				param.Value = new Guid ("e222a130-6383-4d36-ac5e-4e6b2591aabe");
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_data;
				param = cmd.Parameters.Add ("type_guid", OdbcType.UniqueIdentifier);
				param.Value = new Guid ("e222a130-6383-4d36-ac5e-4e6b2591aabe");
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#C1");
				Assert.AreEqual (6000, dr.GetValue (0), "#C2");
				Assert.AreEqual (new Guid ("e222a130-6383-4d36-ac5e-4e6b2591aabe"), dr.GetValue (1), "#C3");
				Assert.IsFalse (dr.Read (), "#C4");
				dr.Close ();
				cmd.Dispose ();

				DBHelper.ExecuteNonQuery (conn, delete_data);

				cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandText = insert_data;
				param = cmd.Parameters.Add ("type_guid", OdbcType.UniqueIdentifier);
				param.Value = DBNull.Value;
				cmd.ExecuteNonQuery ();
				cmd.Dispose ();

				cmd = conn.CreateCommand ();
				cmd.CommandText = select_by_id;
				param = cmd.Parameters.Add ("id", OdbcType.Int);
				param.Value = 6000;
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#D1");
				Assert.AreEqual (6000, dr.GetValue (0), "#D2");
				Assert.AreEqual (DBNull.Value, dr.GetValue (1), "#D3");
				Assert.IsFalse (dr.Read (), "#D4");
				dr.Close ();
				cmd.Dispose ();
			} finally {
				if (dr != null)
					dr.Close ();
				if (cmd != null)
					cmd.Dispose ();

				conn.Close ();
				conn.Open ();
				DBHelper.ExecuteNonQuery (conn, delete_data);
				conn.Close ();
			}
		}

		[Test]
		public void DBNullParameterTest()
		{
			OdbcConnection conn = ConnectionManager.Instance.Odbc.Connection;
			try
			{
				OdbcDataAdapter Adaptador = new OdbcDataAdapter ();
				DataSet Lector = new DataSet ();

				Adaptador.SelectCommand = new OdbcCommand ("SELECT ?;", (OdbcConnection) conn);
				Adaptador.SelectCommand.Parameters.AddWithValue("@un", DBNull.Value);
				Adaptador.Fill (Lector);
				Assert.AreEqual (Lector.Tables[0].Rows[0][0], DBNull.Value, "#1 DBNull parameter not passed correctly");
			} finally {
				ConnectionManager.Instance.Odbc.CloseConnection ();
			}
		}

		[Test]
		public void ParameterName ()
		{
			OdbcParameter p = new OdbcParameter ();
			p.ParameterName = "foo1";
			Assert.AreEqual ("foo1", p.ParameterName, "#1");
			p.ParameterName = null;
			Assert.AreEqual (string.Empty, p.ParameterName, "#2");
			p.ParameterName = "foo2";
			Assert.AreEqual ("foo2", p.ParameterName, "#3");
			p.ParameterName = string.Empty;
			Assert.AreEqual (string.Empty, p.ParameterName, "#4");
		}

		[Test]
		public void SourceColumn ()
		{
			OdbcParameter p = new OdbcParameter ();
			p.SourceColumn = "foo1";
			Assert.AreEqual ("foo1", p.SourceColumn, "#1");
			p.SourceColumn = null;
			Assert.AreEqual (string.Empty, p.SourceColumn, "#2");
			p.SourceColumn = "foo2";
			Assert.AreEqual ("foo2", p.SourceColumn, "#3");
			p.SourceColumn = string.Empty;
			Assert.AreEqual (string.Empty, p.SourceColumn, "#4");
		}

		[Test]
		public void DefaultValuesTest ()
		{
			OdbcParameter p;

			p = new OdbcParameter();
			Assert.AreEqual (DbType.String, p.DbType, "#A:DbType");
			Assert.AreEqual (ParameterDirection.Input, p.Direction, "#A:Direction");
			Assert.IsFalse (p.IsNullable, "#A:IsNullable");
			Assert.AreEqual (OdbcType.NVarChar, p.OdbcType, "#A:OdbcType");
			Assert.AreEqual (String.Empty, p.ParameterName, "#A:ParameterName");
			Assert.AreEqual (0, p.Precision, "#A:Precision");
			Assert.AreEqual (0, p.Scale, "#A:Scale");
			Assert.AreEqual (0, p.Size, "#A:Size");
			Assert.AreEqual (String.Empty, p.SourceColumn, "#A:SourceColumn");
			Assert.IsFalse (p.SourceColumnNullMapping, "#A:SourceColumnNullMapping");
			Assert.AreEqual (DataRowVersion.Current, p.SourceVersion, "#A:SourceVersion");
			Assert.IsNull (p.Value, "#A:Value");

			p = new OdbcParameter(null, 2);
			Assert.AreEqual (DbType.String, p.DbType, "#B:DbType");
			Assert.AreEqual (ParameterDirection.Input, p.Direction, "#B:Direction");
			Assert.IsFalse (p.IsNullable, "#B:IsNullable");
			Assert.AreEqual (OdbcType.NVarChar, p.OdbcType, "#B:OdbcType");
			Assert.AreEqual (String.Empty, p.ParameterName, "#B:ParameterName");
			Assert.AreEqual (0, p.Precision, "#B:Precision");
			Assert.AreEqual (0, p.Scale, "#B:Scale");
			Assert.AreEqual (0, p.Size, "#B:Size");
			Assert.AreEqual (String.Empty, p.SourceColumn, "#B:SourceColumn");
			Assert.IsFalse (p.SourceColumnNullMapping, "#B:SourceColumnNullMapping");
			Assert.AreEqual (DataRowVersion.Current, p.SourceVersion, "#B:SourceVersion");
			Assert.AreEqual (2, p.Value, "#B:Value");

			p = new OdbcParameter("foo", 2);
			Assert.AreEqual (DbType.String, p.DbType, "#C:DbType");
			Assert.AreEqual (ParameterDirection.Input, p.Direction, "#C:Direction");
			Assert.IsFalse (p.IsNullable, "#C:IsNullable");
			Assert.AreEqual (OdbcType.NVarChar, p.OdbcType, "#C:OdbcType");
			Assert.AreEqual ("foo", p.ParameterName, "#C:ParameterName");
			Assert.AreEqual (0, p.Precision, "#C:Precision");
			Assert.AreEqual (0, p.Scale, "#C:Scale");
			Assert.AreEqual (0, p.Size, "#C:Size");
			Assert.AreEqual (String.Empty, p.SourceColumn, "#C:SourceColumn");
			Assert.IsFalse (p.SourceColumnNullMapping, "#C:SourceColumnNullMapping");
			Assert.AreEqual (DataRowVersion.Current, p.SourceVersion, "#C:SourceVersion");
			Assert.AreEqual (2, p.Value, "#C:Value");

			p = new OdbcParameter("foo1", OdbcType.Int);
			Assert.AreEqual (DbType.Int32, p.DbType, "#D:DbType");
			Assert.AreEqual (ParameterDirection.Input, p.Direction, "#D:Direction");
			Assert.IsFalse (p.IsNullable, "#D:IsNullable");
			Assert.AreEqual (OdbcType.Int, p.OdbcType, "#D:OdbcType");
			Assert.AreEqual ("foo1", p.ParameterName, "#D:ParameterName");
			Assert.AreEqual (0, p.Precision, "#D:Precision");
			Assert.AreEqual (0, p.Scale, "#D:Scale");
			Assert.AreEqual (0, p.Size, "#D:Size");
			Assert.AreEqual (String.Empty, p.SourceColumn, "#D:SourceColumn");
			Assert.IsFalse (p.SourceColumnNullMapping, "#D:SourceColumnNullMapping");
			Assert.AreEqual (DataRowVersion.Current, p.SourceVersion, "#D:SourceVersion");
			Assert.IsNull (p.Value, "#D:Value");
		}
	}
}

#endif