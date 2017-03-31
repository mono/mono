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

#if !NO_ODBC

using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Globalization;
using System.Text;
using NUnit.Framework;


namespace MonoTests.System.Data.Connected.Odbc
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcDataReaderTest
	{
		static byte [] long_bytes = new byte [] {
			0x00, 0x66, 0x06, 0x66, 0x97, 0x00, 0x66, 0x06, 0x66,
			0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
			0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
			0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
			0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
			0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
			0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
			0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
			0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
			0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
			0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
			0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
			0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
			0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
			0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
			0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
			0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
			0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
			0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
			0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
			0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
			0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
			0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
			0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
			0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
			0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66,
			0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06,
			0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06, 0x66,
			0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97, 0x06,
			0x66, 0x06, 0x66, 0x97, 0x06, 0x66, 0x06, 0x66, 0x97,
			0x06, 0x66, 0x06, 0x66, 0x98};

		OdbcConnection conn;
		OdbcCommand cmd;

		[SetUp]
		public void SetUp ()
		{
			conn = ConnectionManager.Instance.Odbc.Connection;
			cmd = conn.CreateCommand ();
		}

		[TearDown]
		public void TearDown ()
		{
			if (cmd != null)
				cmd.Dispose ();
			ConnectionManager.Instance.Close ();
		}

		[Test]
		public void GetBytes ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";
			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read (), "#C1");

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);

				byte [] val = new byte [totalsize];
				long ret = reader.GetBytes (0, 0L, val, 0, (int) (totalsize * 2));
				Assert.AreEqual (totalsize, ret, "#C2");
				Assert.AreEqual (new byte [] { 0x32, 0x56, 0x00, 0x44, 0x22 }, val, "#C3");
			}

			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 2";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read (), "#G1");

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);

				byte [] val = new byte [totalsize];
				int offset = 0;
				long ret = 0;
				long count = 0;
				do {
					ret = reader.GetBytes (0, offset, val, offset, 50);
					offset += (int) ret;
					count += ret;
				} while (count < totalsize);

				Assert.AreEqual (long_bytes.Length, count, "#G2");
				Assert.AreEqual (long_bytes, val, "#G3");
			}

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult)) {
				Assert.IsTrue (reader.Read (), "#H1");

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);

				byte [] val = new byte [totalsize];
				int offset = 0;
				long ret = 0;
				long count = 0;
				do {
					ret = reader.GetBytes (0, offset, val, offset, 50);
					offset += (int) ret;
					count += ret;
				} while (count < totalsize);

				Assert.AreEqual (long_bytes.Length, count, "#H2");
				Assert.AreEqual (long_bytes, val, "#H3");
			}
		}

		[Test]
		public void GetBytes_Buffer_TooSmall ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 2";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize - 1];

				long ret = reader.GetBytes (0, 0, val, 0, (int) totalsize);
				Assert.AreEqual (274, ret, "#A1");
				for (int i = 0; i < ret; i++)
					Assert.AreEqual (long_bytes [i], val [i], "#A2:" + i);
				for (long i = ret; i < val.Length; i++)
					Assert.AreEqual (0x00, val [i], "#A3:" + i);
			}

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize];
				int buffer_offset = 1;

				long ret = reader.GetBytes (0, 0, val, buffer_offset, (int) totalsize);
				Assert.AreEqual (274, ret, "#B1");
				Assert.AreEqual (0x00, val [0], "#B2");
				for (int i = 0; i < ret; i++)
					Assert.AreEqual (long_bytes [i], val [i + buffer_offset], "#B2:" + i);
				for (long i = (ret + buffer_offset); i < val.Length; i++)
					Assert.AreEqual (0x00, val [i], "#B3:" + i);
			}

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize];

				long ret = reader.GetBytes (0, 0, val, 0, (int) (totalsize + 1));
				Assert.AreEqual (totalsize, ret, "#C1");
			}

			ConnectionManager.Instance.Odbc.CloseConnection ();
		}

		[Test]
		public void GetBytes_BufferIndex_Negative ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read (), "#1");

				try {
					reader.GetBytes (0, 0, null, -1, 0);
					Assert.Fail ("#2");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
					Assert.AreEqual ("bufferIndex", ex.ParamName, "#6");
				}
			}
		}

		[Test]
		public void GetBytes_DataIndex_Negative ()
		{
			IDbCommand cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read (), "#A1");

				try {
					reader.GetBytes (0, -1L, null, 0, 0);
					Assert.Fail ("#A2");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A3");
					Assert.IsNull (ex.InnerException, "#A4");
					Assert.IsNotNull (ex.Message, "#A5");
					Assert.AreEqual ("dataIndex", ex.ParamName, "#A6");
				}
			}

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult)) {
				Assert.IsTrue (reader.Read (), "#B1");

				try {
					reader.GetBytes (0, -1L, null, 0, 0);
					Assert.Fail ("#B2");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B3");
					Assert.IsNull (ex.InnerException, "#B4");
					Assert.IsNotNull (ex.Message, "#B5");
					Assert.AreEqual ("dataIndex", ex.ParamName, "#B6");
				}
			}
		}

		[Test]
		public void GetBytes_Length_Negative ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read (), "#1");

				try {
					reader.GetBytes (0, 0, null, 0, -1);
					Assert.Fail ("#2");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
					Assert.AreEqual ("length", ex.ParamName, "#6");
				}
			}
		}

		[Test]
		public void GetSchemaTable ()
		{
			IDataReader reader = null;
			DataTable schema;
			DataRow pkRow;

			try {
				cmd.CommandText = "select id, fname, id + 20 as plustwenty from employee";
				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#A:");
				Assert.AreEqual (3, schema.Rows.Count, "#A:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#A:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#A:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#A:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#A:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#A:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#A:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#A:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#A:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#A:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#A:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#A:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#A:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#A:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#A:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#A:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#A:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#A:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#A:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#A:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#A:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#A:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#A:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#A:IsUnique_IsNull");
				Assert.AreEqual (true, pkRow ["IsUnique"], "#A:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#A:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#A:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#A:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#A:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#A:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#A:BaseSchemaName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseCatalogName"), "#A:BaseCatalogName_IsNull");
				Assert.AreEqual (ConnectionManager.Instance.DatabaseName, pkRow ["BaseCatalogName"], "#A:BaseCatalogName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseTableName"), "#A:BaseTableName_IsNull");
				Assert.AreEqual ("employee", pkRow ["BaseTableName"], "#A:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#A:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#A:BaseColumnName_Value");

				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly);
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#B:");
				Assert.AreEqual (3, schema.Rows.Count, "#B:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#B:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#B:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#B:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#B:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#B:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#B:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#B:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#B:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#B:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#B:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#B:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#B:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#B:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#B:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#B:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#B:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#B:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#B:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#B:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#B:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#B:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#B:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#B:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#B:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#B:IsKey_IsNull");
				Assert.AreEqual (false, pkRow ["IsKey"], "#B:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#B:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#B:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#B:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#B:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#B:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#B:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#B:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#B:BaseTableName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseColumnName"), "#B:BaseColumnName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseColumnName"], "#B:BaseColumnName_Value");

				reader = cmd.ExecuteReader (CommandBehavior.KeyInfo);
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#C:");
				Assert.AreEqual (3, schema.Rows.Count, "#C:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#C:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#C:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#C:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#C:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#C:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#C:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#C:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#C:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#C:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#C:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#C:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#C:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#C:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#C:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#C:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#C:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#C:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#C:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#C:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#C:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#C:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#C:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#C:IsUnique_IsNull");
				Assert.AreEqual (true, pkRow ["IsUnique"], "#C:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#C:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#C:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#C:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#C:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#C:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#C:BaseSchemaName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseCatalogName"), "#C:BaseCatalogName_IsNull");
				Assert.AreEqual (ConnectionManager.Instance.DatabaseName, pkRow ["BaseCatalogName"], "#C:BaseCatalogName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseTableName"), "#C:BaseTableName_IsNull");
				Assert.AreEqual ("employee", pkRow ["BaseTableName"], "#C:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#C:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#C:BaseColumnName_Value");

				reader = cmd.ExecuteReader ();
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#D:");
				Assert.AreEqual (3, schema.Rows.Count, "#D:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#D:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#D:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#D:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#D:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#D:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#D:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#D:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#D:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#D:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#D:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#D:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#D:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#D:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#D:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#D:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#D:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#D:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#D:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#D:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#D:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#D:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#D:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#D:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#D:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#D:IsKey_IsNull");
				Assert.AreEqual (false, pkRow ["IsKey"], "#D:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#D:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#D:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#D:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#D:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#D:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#D:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#D:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#D:BaseTableName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseColumnName"), "#D:BaseColumnName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseColumnName"], "#D:BaseColumnName_Value");

				cmd = conn.CreateCommand ();
				cmd.CommandText = "select id, fname, id + 20 as plustwenty from employee";
				cmd.Prepare ();
				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#E:");
				Assert.AreEqual (3, schema.Rows.Count, "#E:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#E:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#E:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#E:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#E:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#E:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#E:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#E:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#E:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#E:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#E:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#E:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#E:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#E:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#E:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#E:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#E:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#E:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#E:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#E:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#E:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#E:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#E:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#E:IsUnique_IsNull");
				Assert.AreEqual (true, pkRow ["IsUnique"], "#E:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#E:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#E:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#E:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#E:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#E:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#E:BaseSchemaName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseCatalogName"), "#E:BaseCatalogName_IsNull");
				Assert.AreEqual (ConnectionManager.Instance.DatabaseName, pkRow ["BaseCatalogName"], "#E:BaseCatalogName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseTableName"), "#E:BaseTableName_IsNull");
				Assert.AreEqual ("employee", pkRow ["BaseTableName"], "#E:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#E:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#E:BaseColumnName_Value");

				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly);
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#F:");
				Assert.AreEqual (3, schema.Rows.Count, "#F:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#F:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#F:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#F:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#F:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#F:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#F:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#F:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#F:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#F:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#F:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#F:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#F:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#F:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#F:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#F:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#F:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#F:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#F:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#F:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#F:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#F:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#F:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#F:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#F:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#F:IsKey_IsNull");
				Assert.AreEqual (false, pkRow ["IsKey"], "#F:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#F:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#F:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#F:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#F:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#F:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#F:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#F:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#F:BaseTableName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseColumnName"), "#F:BaseColumnName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseColumnName"], "#F:BaseColumnName_Value");

				reader = cmd.ExecuteReader (CommandBehavior.KeyInfo);
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#G:");
				Assert.AreEqual (3, schema.Rows.Count, "#G:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#G:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#G:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#G:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#G:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#G:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#G:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#G:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#G:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#G:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#G:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#G:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#G:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#G:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#G:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#G:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#G:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#G:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#G:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#G:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#G:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#G:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#G:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#G:IsUnique_IsNull");
				Assert.AreEqual (true, pkRow ["IsUnique"], "#G:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#G:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#G:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#G:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#G:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#G:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#G:BaseSchemaName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseCatalogName"), "#G:BaseCatalogName_IsNull");
				Assert.AreEqual (ConnectionManager.Instance.DatabaseName, pkRow ["BaseCatalogName"], "#G:BaseCatalogName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseTableName"), "#G:BaseTableName_IsNull");
				Assert.AreEqual ("employee", pkRow ["BaseTableName"], "#G:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#G:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#G:BaseColumnName_Value");

				reader = cmd.ExecuteReader ();
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#H:");
				Assert.AreEqual (3, schema.Rows.Count, "#H:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#H:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#H:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#H:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#H:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#H:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#H:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#H:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#H:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#H:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#H:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#H:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#H:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#H:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#H:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#H:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#H:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#H:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#H:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#H:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#H:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#H:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#H:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#H:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#H:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#H:IsKey_IsNull");
				Assert.AreEqual (false, pkRow ["IsKey"], "#H:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#H:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#H:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#H:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#H:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#H:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#H:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#H:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#H:BaseTableName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseColumnName"), "#H:BaseColumnName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseColumnName"], "#H:BaseColumnName_Value");

				cmd.CommandText = "select id, fname, id + 20 as plustwenty from employee where id = ?";
				IDbDataParameter param = cmd.CreateParameter ();
				cmd.Parameters.Add (param);
				param.DbType = DbType.Int32;
				param.Value = 2;
				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#I:");
				Assert.AreEqual (3, schema.Rows.Count, "#I:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#I:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#I:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#I:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#I:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#I:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#I:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#I:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#I:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#I:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#I:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#I:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#I:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#I:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#I:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#I:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#I:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#I:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#I:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#I:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#I:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#I:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#I:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#I:IsUnique_IsNull");
				Assert.AreEqual (true, pkRow ["IsUnique"], "#I:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#I:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#I:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#I:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#I:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#I:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#I:BaseSchemaName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseCatalogName"), "#I:BaseCatalogName_IsNull");
				Assert.AreEqual (ConnectionManager.Instance.DatabaseName, pkRow ["BaseCatalogName"], "#I:BaseCatalogName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseTableName"), "#I:BaseTableName_IsNull");
				Assert.AreEqual ("employee", pkRow ["BaseTableName"], "#I:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#I:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#I:BaseColumnName_Value");

				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly);
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#J:");
				Assert.AreEqual (3, schema.Rows.Count, "#J:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#J:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#J:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#J:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#J:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#J:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#J:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#J:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#J:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#J:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#J:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#J:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#J:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#J:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#J:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#J:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#J:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#J:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#J:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#J:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#J:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#J:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#J:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#J:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#J:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#J:IsKey_IsNull");
				Assert.AreEqual (false, pkRow ["IsKey"], "#J:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#J:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#J:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#J:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#J:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#J:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#J:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#J:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#J:BaseTableName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseColumnName"), "#J:BaseColumnName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseColumnName"], "#J:BaseColumnName_Value");

				reader = cmd.ExecuteReader (CommandBehavior.KeyInfo);
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#K:");
				Assert.AreEqual (3, schema.Rows.Count, "#K:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#K:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#K:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#K:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#K:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#K:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#K:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#K:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#K:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#K:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#K:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#K:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#K:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#K:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#K:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#K:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#K:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#K:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#K:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#K:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#K:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#K:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#K:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#K:IsUnique_IsNull");
				Assert.AreEqual (true, pkRow ["IsUnique"], "#K:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#K:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#K:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#K:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#K:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#K:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#K:BaseSchemaName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseCatalogName"), "#K:BaseCatalogName_IsNull");
				Assert.AreEqual (ConnectionManager.Instance.DatabaseName, pkRow ["BaseCatalogName"], "#K:BaseCatalogName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseTableName"), "#K:BaseTableName_IsNull");
				Assert.AreEqual ("employee", pkRow ["BaseTableName"], "#K:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#K:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#K:BaseColumnName_Value");

				reader = cmd.ExecuteReader ();
				schema = reader.GetSchemaTable ();
				reader.Close ();

				AssertSchemaTableStructure (schema, "#L:");
				Assert.AreEqual (3, schema.Rows.Count, "#L:RowCount");
				pkRow = schema.Select ("ColumnName = 'id'") [0];
				Assert.IsFalse (pkRow.IsNull ("ColumnName"), "#L:ColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["ColumnName"], "#L:ColumnName_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnOrdinal"), "#L:ColumnOrdinal_IsNull");
				Assert.AreEqual (0, pkRow ["ColumnOrdinal"], "#L:ColumnOrdinal_Value");
				Assert.IsFalse (pkRow.IsNull ("ColumnSize"), "#L:ColumnSize_IsNull");
				Assert.AreEqual (4, pkRow ["ColumnSize"], "#L:ColumnSize_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericPrecision"), "#L:NumericPrecision_IsNull");
				Assert.AreEqual (10, pkRow ["NumericPrecision"], "#L:NumericPrecision_Value");
				Assert.IsFalse (pkRow.IsNull ("NumericScale"), "#L:NumericScale_IsNull");
				Assert.AreEqual (0, pkRow ["NumericScale"], "#L:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#L:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#L:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#L:ProviderType_IsNull");
				Assert.AreEqual (10, pkRow ["ProviderType"], "#L:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#L:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#L:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#L:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#L:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#L:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#L:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#L:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#L:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#L:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#L:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#L:IsKey_IsNull");
				Assert.AreEqual (false, pkRow ["IsKey"], "#L:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#L:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#L:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#L:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#L:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#L:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#L:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#L:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#L:BaseTableName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseColumnName"), "#L:BaseColumnName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseColumnName"], "#L:BaseColumnName_Value");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void OutputParametersTest ()
		{
			// MySQL currently does not support output parameters
			// in its ODBC connector:
			// http://www.paragon-cs.com/mag/issue3.pdf
			if (ConnectionManager.Instance.Odbc.EngineConfig.Type != EngineType.SQLServer)
				Assert.Ignore ("MySQL does not (yet) support output parameters using ODBC.");

			IDataReader reader = null;

			try {
				cmd.CommandText = "{? = CALL sp_get_age (?, ?)}";

				OdbcParameter ret = new OdbcParameter ("ret", OdbcType.Int);
				cmd.Parameters.Add (ret);
				ret.Direction = ParameterDirection.ReturnValue;

				OdbcParameter name = new OdbcParameter ("fname", OdbcType.VarChar);
				cmd.Parameters.Add (name);
				name.Direction = ParameterDirection.Input;
				name.Value = "suresh";

				OdbcParameter age = new OdbcParameter ("age", OdbcType.Int);
				cmd.Parameters.Add (age);
				age.Direction = ParameterDirection.Output;

				reader = cmd.ExecuteReader ();
				reader.Close ();

				/* FIXME: we don't support output/return parameters */
				if (!RunningOnMono) {
					Assert.IsTrue (((int) (age.Value)) > 0, "#1");
					Assert.IsTrue (((int) ret.Value) > 0, "#2");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void LongTextTest ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "Select type_text from string_family where id=2";

				reader = cmd.ExecuteReader ();
				reader.Read ();
				reader.GetValue (0);
			}finally {
				if (reader != null)
					reader.Close ();
			}
		}
		
		[Test]
		public void Bug82135Test ()
		{
			const string drop_table = "drop table odbcnodatatest";

			// cleanup in case of previously failed test
			DBHelper.ExecuteNonQuery (conn, drop_table);

			cmd = new OdbcCommand ("create table odbcnodatatest (ID int not null, Val1 text)",
				conn);
			cmd.ExecuteNonQuery ();
			cmd = new OdbcCommand ("delete from odbcnodatatest", conn);
			Assert.AreEqual (0, cmd.ExecuteNonQuery ());

			// cleanup
			cmd = new OdbcCommand (drop_table, conn);
			cmd.ExecuteNonQuery ();
		}

		[Test]
		public void Bug82560Test ()
		{
			string drop_table = "DROP TABLE odbc_alias_test";

			// cleanup in case of previously failed test
			DBHelper.ExecuteNonQuery (conn, drop_table);

			DoExecuteNonQuery (conn, "CREATE TABLE odbc_alias_test" + 
					   "(ifld INT NOT NULL PRIMARY KEY, sfld VARCHAR(20))");
			DoExecuteNonQuery (conn, "INSERT INTO odbc_alias_test" +
					   "(ifld, sfld) VALUES (1, '1111')");
			DoExecuteScalar (conn, "SELECT A.ifld FROM odbc_alias_test " +
					 "A WHERE A.ifld = 1");
			DoExecuteNonQuery (conn, drop_table);
		}

		[Test]
		public void FindZeroInToStringTest ()
		{
			if (ConnectionManager.Instance.Odbc.EngineConfig.Type != EngineType.MySQL)
				Assert.Ignore ("Only applies to MySQL.");

			IDataReader reader = null;

			try {
				// Create table
				cmd.CommandText = "Create table foo ( bar long varchar )";
				cmd.ExecuteNonQuery();
				cmd.Dispose ();

				// Insert a record into foo
				cmd = conn.CreateCommand ();
				cmd.CommandText = "Insert into foo (bar) values ( '"
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
				cmd.ExecuteNonQuery();
				cmd.Dispose ();

				// Now, get the record back - try and read it two different ways.
				cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT bar FROM foo" ;
				reader = cmd.ExecuteReader ();
				string readAsString = "";
				while (reader.Read ()) {
					readAsString = reader[0].ToString();
				}
				reader.Close();
				cmd.Dispose ();

				// Now, read it using GetBytes
				cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT bar FROM foo";
				reader = cmd.ExecuteReader ();
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
				DBHelper.ExecuteNonQuery (conn, "Drop table foo");
			}
		}

		[Test]
		public void Bug332404Test ()
		{
			if (ConnectionManager.Instance.Odbc.EngineConfig.Type != EngineType.MySQL)
				Assert.Ignore ("Only applies to MySQL.");

			cmd = new OdbcCommand ("DROP TABLE IF EXISTS odbc_test");
			cmd.Connection = conn;
			cmd.ExecuteNonQuery ();

			cmd = new OdbcCommand ("CREATE TABLE odbc_test (id_test INTEGER NOT NULL, payload DECIMAL (14,4) NOT NULL)");
			cmd.Connection = conn;
			cmd.ExecuteNonQuery ();

			cmd = new OdbcCommand ("INSERT INTO odbc_test (id_test, payload) VALUES (1, 1.23456789)");
			cmd.Connection = conn;
			cmd.ExecuteNonQuery ();

			OdbcDataAdapter Adaptador = new OdbcDataAdapter ();

			DataSet Lector = new DataSet ();

			Adaptador.SelectCommand = new OdbcCommand ("SELECT * FROM odbc_test WHERE id_test=1", (OdbcConnection) conn);
			Adaptador.Fill (Lector);
			Assert.AreEqual (Lector.Tables[0].Rows[0]["payload"], 1.2346);
		}

		[Test]
		public void Bug332400Test ()
		{
			if (ConnectionManager.Instance.Odbc.EngineConfig.Type != EngineType.MySQL)
				Assert.Ignore ("Only applies to MySQL.");

			cmd = new OdbcCommand ("DROP TABLE IF EXISTS blob_test");
			cmd.Connection = conn;
			cmd.ExecuteNonQuery ();

			cmd = new OdbcCommand ("CREATE TABLE blob_test (id_test INTEGER NOT NULL, payload LONGBLOB NOT NULL)");
			cmd.Connection = conn;
			cmd.ExecuteNonQuery ();

			cmd = new OdbcCommand ("INSERT INTO blob_test (id_test, payload) VALUES (1, 'test')");
			cmd.Connection = conn;
			cmd.ExecuteNonQuery ();

			OdbcDataAdapter Adaptador = new OdbcDataAdapter();
			DataSet Lector = new DataSet();

			Adaptador.SelectCommand = new OdbcCommand("SELECT * FROM blob_test WHERE id_test=1", (OdbcConnection) conn);
			Adaptador.Fill(Lector);
		}

		[Test]
		public void Bug419224Test () 
		{
			cmd = new OdbcCommand ("DROP TABLE IF EXISTS odbc_test");
			cmd.Connection = conn;
			cmd.ExecuteNonQuery ();

			cmd = new OdbcCommand ("CREATE TABLE odbc_test (id_test INTEGER NOT NULL, payload TINYBLOB NOT NULL)");
			cmd.Connection = conn;
			cmd.ExecuteNonQuery ();

			cmd = new OdbcCommand ("INSERT INTO odbc_test (id_test, payload) VALUES (1, 'test for bug419224')");
			cmd.Connection = conn;
			cmd.ExecuteNonQuery ();

			OdbcDataAdapter adaptador = new OdbcDataAdapter ();
			DataSet lector = new DataSet ();

			adaptador.SelectCommand = new OdbcCommand ("SELECT * FROM odbc_test WHERE id_test=1", (OdbcConnection) conn);
			adaptador.Fill (lector);
			var payload = (byte[])lector.Tables[0].Rows[0]["payload"];
			Assert.AreEqual ("test for bug419224", Encoding.UTF8.GetString(payload));

			OdbcDataReader newRdr = cmd.ExecuteReader();

			// tinyblob column index:
			int TinyblobIdx = 1;

			bool read = newRdr.Read();

			if (read)
			{
					bool ret = newRdr.IsDBNull(TinyblobIdx); 
				Assert.AreEqual (ret, false);
			}
		}

		static void DoExecuteNonQuery (OdbcConnection conn, string sql)
		{
			IDbCommand cmd = new OdbcCommand (sql, conn);
			cmd.ExecuteNonQuery ();
		}

		static void DoExecuteScalar (OdbcConnection conn, string sql)
		{
			IDbCommand cmd = new OdbcCommand (sql, conn);
			cmd.ExecuteScalar ();
		}

		static void AssertSchemaTableStructure (DataTable schemaTable, string prefix)
		{
			object [] [] columns = {
				new object [] { "ColumnName", typeof (string) },
				new object [] { "ColumnOrdinal", typeof (int) },
				new object [] { "ColumnSize", typeof (int) },
				new object [] { "NumericPrecision", typeof (short) },
				new object [] { "NumericScale", typeof (short) },
				new object [] { "DataType", typeof (object) },
				new object [] { "ProviderType", typeof (int) },
				new object [] { "IsLong", typeof (bool) },
				new object [] { "AllowDBNull", typeof (bool) },
				new object [] { "IsReadOnly", typeof (bool) },
				new object [] { "IsRowVersion", typeof (bool) },
				new object [] { "IsUnique", typeof (bool) },
				new object [] { "IsKey", typeof (bool) },
				new object [] { "IsAutoIncrement", typeof (bool) },
				new object [] { "BaseSchemaName", typeof (string) },
				new object [] { "BaseCatalogName", typeof (string) },
				new object [] { "BaseTableName", typeof (string) },
				new object [] { "BaseColumnName", typeof (string) }
				};

			Assert.AreEqual (columns.Length, schemaTable.Columns.Count, prefix);

			for (int i = 0; i < columns.Length; i++) {
				DataColumn col = schemaTable.Columns [i];
				Assert.IsTrue (col.AllowDBNull, prefix + "AllowDBNull (" + i + ")");
				Assert.AreEqual (columns [i] [0], col.ColumnName, prefix + "ColumnName (" + i + ")");
				Assert.AreEqual (columns [i] [1], col.DataType, prefix + "DataType (" + i + ")");
			}
		}

		static bool RunningOnMono {
			get {
				return (Type.GetType ("System.MonoType", false) != null);
			}
		}
	}
}

#endif