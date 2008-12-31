// DataReaderTest.cs - NUnit Test Cases for testing the
// DataReader family of classes
//
// Authors:
//      Sureshkumar T (tsureshkumar@novell.com)
//	Gert Driesen (drieseng@users.sourceforge.net)
// 
// Copyright (c) 2004 Novell Inc., and the individuals listed on the
// ChangeLog entries.
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;

using Mono.Data;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	[Category ("odbc")]
	[Category ("sqlserver")]
	public class DataReaderTest
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

		IDbConnection conn;
		IDbCommand cmd;

		[SetUp]
		public void SetUp ()
		{
			conn = ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();
			cmd = conn.CreateCommand ();
		}

		[TearDown]
		public void TearDown ()
		{
			if (cmd != null)
				cmd.Dispose ();
			ConnectionManager.Singleton.CloseConnection ();
		}

		[Test]
		public void FieldCount ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				Assert.AreEqual (6, reader.FieldCount);
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void FieldCount_Command_Disposed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				cmd.Dispose ();
				Assert.AreEqual (6, reader.FieldCount);
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void FieldCount_Reader_Closed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				reader.Close ();
				try {
					int fieldcount = reader.FieldCount;
					Assert.Fail ("#1:" + fieldcount);
				} catch (InvalidOperationException ex) {
					// Invalid attempt to FieldCount when
					// reader is closed
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetDataTypeName ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();

				switch (ConnectionManager.Singleton.Engine.Type) {
				case EngineType.SQLServer:
					Assert.AreEqual ("int", reader.GetDataTypeName (0), "#1");
					break;
				case EngineType.MySQL:
					Assert.AreEqual ("integer", reader.GetDataTypeName (0), "#1");
					break;
				default:
					Assert.Fail ("Engine type not supported.");
					break;
				}
				Assert.AreEqual ("varchar", reader.GetDataTypeName (1), "#2");
				Assert.AreEqual ("varchar", reader.GetDataTypeName (2), "#3");
				Assert.AreEqual ("datetime", reader.GetDataTypeName (3), "#4");
				Assert.AreEqual ("datetime", reader.GetDataTypeName (4), "#5");
				Assert.AreEqual ("varchar", reader.GetDataTypeName (5), "#6");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetDataTypeName_Index_Invalid ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();

				try {
					reader.GetDataTypeName (-1);
					Assert.Fail ("#1");
				} catch (IndexOutOfRangeException) {
				}

				try {
					reader.GetDataTypeName (6);
					Assert.Fail ("#2");
				} catch (IndexOutOfRangeException) {
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetDataTypeName_Reader_Closed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				reader.Close ();

				try {
					reader.GetDataTypeName (0);
					Assert.Fail ("#1");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetFieldType ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				Assert.AreEqual (typeof (int), reader.GetFieldType (0), "#1");
				Assert.AreEqual (typeof (string), reader.GetFieldType (2), "#2");
				Assert.AreEqual (typeof (DateTime), reader.GetFieldType (4), "#3");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetFieldType_Index_Invalid ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				try {
					reader.GetFieldType (-1);
					Assert.Fail ("#1");
				} catch (IndexOutOfRangeException) {
				}

				try {
					reader.GetFieldType (6);
					Assert.Fail ("#1");
				} catch (IndexOutOfRangeException) {
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetFieldType_Reader_Closed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				reader.Close ();
				try {
					reader.GetFieldType (0);
					Assert.Fail ("#1");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetName ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				Assert.AreEqual ("id", reader.GetName (0), "#1");
				Assert.AreEqual ("fname", reader.GetName (1), "#2");
				Assert.AreEqual ("lname", reader.GetName (2), "#3");
				Assert.AreEqual ("dob", reader.GetName (3), "#4");
				Assert.AreEqual ("doj", reader.GetName (4), "#5");
				Assert.AreEqual ("email", reader.GetName (5), "#6");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetName_Index_Invalid ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				try {
					reader.GetName (-1);
					Assert.Fail ("#1");
				} catch (IndexOutOfRangeException) {
				}

				try {
					reader.GetName (6);
					Assert.Fail ("#2");
				} catch (IndexOutOfRangeException) {
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetName_Reader_Closed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				reader.Close ();
				try {
					reader.GetName (0);
					Assert.Fail ("#1");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetOrdinal ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				Assert.AreEqual (0, reader.GetOrdinal ("id"), "#1");
				Assert.AreEqual (1, reader.GetOrdinal ("fname"), "#2");
				Assert.AreEqual (2, reader.GetOrdinal ("lname"), "#3");
				Assert.AreEqual (3, reader.GetOrdinal ("dob"), "#4");
				Assert.AreEqual (4, reader.GetOrdinal ("doj"), "#5");
				Assert.AreEqual (5, reader.GetOrdinal ("email"), "#6");
				Assert.AreEqual (0, reader.GetOrdinal ("iD"), "#7");
				Assert.AreEqual (5, reader.GetOrdinal ("eMail"), "#8");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetOrdinal_Name_DoesNotExist ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				try {
					reader.GetOrdinal ("non_existing_column");
					Assert.Fail ("#1");
				} catch (IndexOutOfRangeException) {
				}

				try {
					reader.GetOrdinal (string.Empty);
					Assert.Fail ("#2");
				} catch (IndexOutOfRangeException) {
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetOrdinal_Name_Null ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				try {
					reader.GetOrdinal (null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("fieldName", ex.ParamName, "#5");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetOrdinal_Reader_Closed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				reader.Close ();
				try {
					reader.GetOrdinal ("does_not_exist");
					Assert.Fail ("#1");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetSchemaTable_Command_Disposed ()
		{
			if (RunningOnMono && (conn is OdbcConnection))
				Assert.Ignore ("Our statement handle is closed when we dispose the (Odbc)Command");

			IDataReader reader = null;

			try {
				cmd.CommandText = "select id, fname, id + 20 as plustwenty from employee";
				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly);
				cmd.Dispose ();
				DataTable schema = reader.GetSchemaTable ();
				Assert.AreEqual (3, schema.Rows.Count, "RowCount");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetSchemaTable_Reader_Closed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "select id, fname, id + 20 as plustwenty from employee";
				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
				reader.Close ();

				try {
					reader.GetSchemaTable ();
					Assert.Fail ("#1");
				} catch (InvalidOperationException ex) {
					// Invalid attempt to call GetSchemaTable
					// when reader is closed
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetNameTest ()
		{
			cmd.CommandText = "SELECT type_tinyint from numeric_family"; ;
			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.AreEqual ("type_tinyint", reader.GetName (0), "#1");
			}
		}

		[Test]
		public void IsClosed_Command_Disposed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "select id, fname, id + 20 as plustwenty from employee";
				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
				Assert.IsFalse (reader.IsClosed, "#1");
				cmd.Dispose ();
				Assert.IsFalse (reader.IsClosed, "#2");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void IsClosed_Connection_Closed ()
		{
			if (RunningOnMono)
				Assert.Ignore ("We do not mark the corresponding Reader closed when we close a Connection.");

			IDataReader reader = null;

			try {
				cmd.CommandText = "select id, fname, id + 20 as plustwenty from employee";
				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
				Assert.IsFalse (reader.IsClosed, "#1");
				ConnectionManager.Singleton.CloseConnection ();
				Assert.IsTrue (reader.IsClosed, "#2");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void IsClosed_Reader_Closed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "select id, fname, id + 20 as plustwenty from employee";
				reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
				Assert.IsFalse (reader.IsClosed, "#1");
				reader.Close ();
				Assert.IsTrue (reader.IsClosed, "#2");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void NumericTest()
		{
			cmd.CommandText = "select type_numeric1 from numeric_family where id = 1;";

			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read(), "#1");
				object value = reader.GetValue (0);
				Assert.AreEqual (typeof (decimal), value.GetType (), "#2");
				Assert.AreEqual (1000m, value, "#3");
			}
		}

		[Test]
		public void TinyIntTest ()
		{
			cmd.CommandText = "select type_tinyint from numeric_family where id = 1;";
			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read (), "#1");
				object value = reader.GetValue (0);
				Assert.AreEqual (typeof (byte), value.GetType (), "#2");
				Assert.AreEqual (255, value, "#3");
			}
		}
		
		[Test]
		public void GetByteTest () 
		{
			cmd.CommandText = "select type_tinyint from numeric_family where id = 1";
			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read (), "#1");
				byte b = reader.GetByte (0);
				Assert.AreEqual (255, b, "#2");
			}
		}

		[Test]
		public void GetValueBinaryTest ()
		{
			cmd.CommandText = "select type_binary from binary_family where id = 1";
			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read (), "#1");
				object ob = reader.GetValue (0);
				Assert.IsNotNull (ob, "#2");
				Assert.AreEqual (typeof (byte []), ob.GetType (), "#3");
			}
		}
		
		[Test]
		public void GetBytes ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			CommandBehavior behavior;

			behavior = CommandBehavior.SingleResult | CommandBehavior.SequentialAccess;
			using (IDataReader reader = cmd.ExecuteReader (behavior)) {
				Assert.IsTrue (reader.Read (), "#A1");

				// Get By Parts for the column blob
				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				int buffsize = 3;
				int offset = 0;
				long ret = 0;
				long count = 0;
				byte [] val = new byte [totalsize];
				do {
					ret = reader.GetBytes (0, offset, val, offset,
						(int) Math.Min (buffsize, totalsize - count));
					offset += (int) ret;
					count += ret;
				} while (count < totalsize);

				Assert.AreEqual (5, count, "#A2");
				Assert.AreEqual (new byte [] { 0x32, 0x56, 0x00, 0x44, 0x22 }, val, "#A3");
			}

			behavior = CommandBehavior.SingleResult;
			using (IDataReader reader = cmd.ExecuteReader (behavior)) {
				Assert.IsTrue (reader.Read (), "#B1");

				// Get By Parts for the column blob
				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				int buffsize = 3;
				int offset = 0;
				long ret = 0;
				long count = 0;
				byte [] val = new byte [totalsize];
				do {
					ret = reader.GetBytes (0, offset, val, offset,
						(int) Math.Min (buffsize, totalsize - count));
					offset += (int) ret;
					count += ret;
				} while (count < totalsize);

				Assert.AreEqual (5, count, "#B2");
				Assert.AreEqual (new byte [] { 0x32, 0x56, 0x00, 0x44, 0x22 }, val, "#B3");
			}

			behavior = CommandBehavior.SingleResult;
			using (IDataReader reader = cmd.ExecuteReader (behavior)) {
				Assert.IsTrue (reader.Read (), "#D1");

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);

				byte [] val = new byte [totalsize];
				long ret = reader.GetBytes (0, 0L, val, 0, (int) (totalsize * 2));
				Assert.AreEqual (totalsize, ret, "#D2");
				Assert.AreEqual (new byte [] { 0x32, 0x56, 0x00, 0x44, 0x22 }, val, "#D3");
			}

			/* FIXME: dataIndex is currently ignored */
			/*
			behavior = CommandBehavior.SingleResult | CommandBehavior.SequentialAccess;
			using (IDataReader reader = cmd.ExecuteReader (behavior)) {
				Assert.IsTrue (reader.Read (), "#E1");

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);

				int bufferIndex = 3;
				long ret = 0L;
				byte [] val = new byte [totalsize + bufferIndex];
				for (int i = 0; i < val.Length; i++)
					val [i] = 0x0a;
				ret = reader.GetBytes (0, 1L, val, bufferIndex, (int) (totalsize - 2));
				Assert.AreEqual (3, ret, "#E2");
				Assert.AreEqual (new byte [] { 0x0a, 0x0a, 0x0a, 0x56, 0x00, 0x44, 0x0a, 0x0a }, val, "#E3");
				try {
					reader.GetBytes (0, 3L, val, 1, 2);
					Assert.Fail ("#E4");
				} catch (InvalidOperationException ex) {
					// Invalid GetBytes attempt at dataIndex '3'.
					// With CommandBehavior.SequentialAccess, you
					// may only read from dataIndex '4' or greater
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E5");
					Assert.IsNull (ex.InnerException, "#E6");
					Assert.IsNotNull (ex.Message, "#E7");
					Assert.IsTrue (ex.Message.IndexOf ("CommandBehavior.SequentialAccess") != -1, "#E8:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("'" + 3L.ToString (CultureInfo.InvariantCulture) + "'") != -1, "#E9:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("'" + 4L.ToString (CultureInfo.InvariantCulture) + "'") != -1, "#E10:" + ex.Message);
				}

				ret = reader.GetBytes (0, 4L, val, bufferIndex + 4, 2);
				Assert.AreEqual (1, ret, "#E11");
				Assert.AreEqual (new byte [] { 0x0a, 0x0a, 0x0a, 0x56, 0x00, 0x44, 0x0a, 0x22 }, val, "#E12");
			}

			behavior = CommandBehavior.SingleResult;
			using (IDataReader reader = cmd.ExecuteReader (behavior)) {
				Assert.IsTrue (reader.Read (), "#F1");

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);

				int bufferIndex = 3;
				long ret = 0L;
				byte [] val = new byte [totalsize + bufferIndex];
				for (int i = 0; i < val.Length; i++)
					val [i] = 0x0a;
				ret = reader.GetBytes (0, 1L, val, bufferIndex, (int) (totalsize - 2));
				Assert.AreEqual (3, ret, "#F2");
				Assert.AreEqual (new byte [] { 0x0a, 0x0a, 0x0a, 0x56, 0x00, 0x44, 0x0a, 0x0a }, val, "#F3");
				ret = reader.GetBytes (0, 3L, val, 1, 2);
				Assert.AreEqual (2, ret, "#F4");
				Assert.AreEqual (new byte [] { 0x0a, 0x44, 0x22, 0x56, 0x00, 0x44, 0x0a, 0x0a }, val, "#F5");
			}
			*/
		}

		[Test]
		public void GetBytes_Buffer_Null ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id in (1,2,3,4) order by id";

			conn.Close ();
			conn = ConnectionManager.Singleton.Connection;
			ConnectionManager.Singleton.OpenConnection ();

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Console.WriteLine ("A");
				Assert.IsTrue (reader.Read (), "#A1");
				Console.WriteLine ("B");
				Assert.AreEqual (5, reader.GetBytes (0, 0, null, 0, 0), "#A2");
				Console.WriteLine ("C");
				Assert.IsTrue (reader.Read (), "#B1");
				Assert.AreEqual (275, reader.GetBytes (0, 0, null, 0, 0), "#B2");

				Assert.IsTrue (reader.Read (), "#C1");
				Assert.AreEqual (0, reader.GetBytes (0, 0, null, 0, 0), "#C2");

				Assert.IsTrue (reader.Read (), "#D1");
				if (conn is SqlConnection) {
#if NET_2_0
					try {
						reader.GetBytes (0, 0, null, 0, 0);
						Assert.Fail ("#D2");
					} catch (SqlNullValueException ex) {
						// Data is Null. This method or
						// property cannot be called on
						// Null values
						Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "#D3");
						Assert.IsNull (ex.InnerException, "#D4");
						Assert.IsNotNull (ex.Message, "#D5");
					}
#else
					Assert.AreEqual (0, reader.GetBytes (0, 0, null, 0, 0), "#D2");
#endif
				} else {
					Assert.AreEqual (-1, reader.GetBytes (0, 0, null, 0, 0), "#D2");
				}
			}
		}

		[Test]
		public void GetBytes_DataIndex_OffSet ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 2";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize];
				long ret;

				// start reading at index 0
				ret = reader.GetBytes (0, 0, val, 0, 5);
				Assert.AreEqual (5, ret, "#A1");
				for (int i = 0; i < ret; i++)
					Assert.AreEqual (long_bytes [i], val [i], "#A2:" + i);
				Assert.AreEqual (0x00, val [5], "#A3");

				// attempt to read data prior to current pointer
				try {
					reader.GetBytes (0, 4, val, 0, 5);
					Assert.Fail ("#B1");
				} catch (InvalidOperationException ex) {
					// Invalid GetBytes attempt at dataIndex '4'
					// With CommandBehavior.SequentialAccess,
					// you may only read from dataIndex '5'
					// or greater.
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("'4'") != -1, "#B5:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("'5'") != -1, "#B6:" + ex.Message);
				}

				// continue reading at current pointer
				ret = reader.GetBytes (0, 5, val, 5, 6);
				Assert.AreEqual (6, ret, "#C1");
				for (int i = 0; i < 11; i++)
					Assert.AreEqual (long_bytes [i], val [i], "#C2:" + i);
				Assert.AreEqual (0x00, val [11], "#C3");

				// skip 4 characters
				ret = reader.GetBytes (0, 15, val, 13, (val.Length - 13));
				Assert.AreEqual (260, ret, "#D1");
				for (int i = 0; i < 11; i++)
					Assert.AreEqual (long_bytes [i], val [i], "#D2:" + i);
				for (int i = 11; i < 13; i++)
					Assert.AreEqual (0x00, val [i], "#D3:" + i);
				for (int i = 13; i < (totalsize - 4); i++)
					Assert.AreEqual (long_bytes [i + 2], val [i], "#D4:" + i);
			}

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize];
				long ret;

				int offset = (int) totalsize - 5;
				int buffer_offset = 7;

				// start reading at a specific position
				ret = reader.GetBytes (0, offset, val, buffer_offset,
					val.Length - buffer_offset);
				Assert.AreEqual (5, ret, "#E1");
				for (int i = 0; i < buffer_offset; i++)
					Assert.AreEqual (0x00, val [i], "#E2:" + i);
				for (int i = 0; i < ret; i++)
					Assert.AreEqual (long_bytes [offset + i], val [buffer_offset + i], "#E3:" + i);
				for (int i = (buffer_offset + (int) ret); i < val.Length; i++)
					Assert.AreEqual (0x00, val [i], "#E4:" + i);
			}
		}

		[Test]
		public void GetBytes_Reader_Closed ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read (), "#1");
				reader.Close ();

				try {
					reader.GetBytes (0, 0, null, -1, 0);
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
			}
		}

		[Test]
		public void GetBytes_Reader_NoData ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				try {
					reader.GetBytes (0, 0, null, -1, 0);
					Assert.Fail ("#2");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
				}
			}
		}

		[Test]
		public void GetSchemaTableTest_AutoIncrement ()
		{
			cmd.CommandText = "select type_autoincrement from numeric_family";
			cmd.ExecuteNonQuery ();
			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly)) {
				DataTable schemaTable = reader.GetSchemaTable ();
				Assert.IsTrue ((bool) schemaTable.Rows [0]["IsAutoIncrement"], "#1");

				if (!RunningOnMono) {
					/* FIXME: we always set it to false */
					if (schemaTable.Columns.Contains ("IsIdentity"))
						Assert.IsTrue ((bool) schemaTable.Rows [0] ["IsIdentity"], "#2");
				}
			}
		}

		static bool RunningOnMono {
			get {
				return (Type.GetType ("System.MonoType", false) != null);
			}
		}
	}
}
