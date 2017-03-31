// DataReaderTest.cs - NUnit Test Cases for testing the
// DataReader family of classes
//
// Authors:
//      Sureshkumar T (tsureshkumar@novell.com)
//	Gert Driesen (drieseng@users.sourceforge.net)
//	Veerapuram Varadhan  (vvaradhan@novell.com)
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
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Data.Connected
{
	[TestFixture]
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
			conn = ConnectionManager.Instance.Sql.Connection;
			cmd = conn.CreateCommand ();
		}

		[TearDown]
		public void TearDown ()
		{
			cmd?.Dispose ();
			ConnectionManager.Instance.Close();
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
		public void GetChars_Index_Invalid ()
		{
			//Console.WriteLine ("In GetChars_Index_Invalid - first_executereader");
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				Assert.IsTrue (rdr.Read ());

				try {
					rdr.GetChars (-1, 0, (char []) null, 0, 0);
					Assert.Fail ("#A1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				try {
					rdr.GetChars (1, 0, (char []) null, 0, 0);
					Assert.Fail ("#B1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
				}
			}

			//Console.WriteLine ("In GetChars_Index_Invalid - second_executereader");
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (rdr.Read ());

				try {
					rdr.GetChars (-1, 0, (char []) null, 0, 0);
					Assert.Fail ("#C1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#C2");
					Assert.IsNull (ex.InnerException, "#C3");
					Assert.IsNotNull (ex.Message, "#C4");
				}

				try {
					rdr.GetChars (1, 0, (char []) null, 0, 0);
					Assert.Fail ("#D1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#D2");
					Assert.IsNull (ex.InnerException, "#D3");
					Assert.IsNotNull (ex.Message, "#D4");
				}
			}
		}

		[Test]
		public void GetChars_Reader_Closed ()
		{
			//Console.WriteLine ("GetChars_Reader_Closed - first_executereader");
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				Assert.IsTrue (rdr.Read ());
				rdr.Close ();

				try {
					rdr.GetChars (-1, 0, (char []) null, 0, 0);
					Assert.Fail ("#A1");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}
			}

			//Console.WriteLine ("GetChars_Reader_Closed - second_executereader");
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (rdr.Read ());
				rdr.Close ();

				try {
					rdr.GetChars (-1, 0, (char []) null, 0, 0);
					Assert.Fail ("#B1");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
				}
			}
		}

		[Test]
		[Category("NotWorking")]
		public void GetChars_Reader_NoData ()
		{
			//Console.WriteLine ("GetChars_Reader_NoData - first_executereader");
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 666";

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				try {
					rdr.GetChars (-1, 0, (char []) null, 0, 0);
					Assert.Fail ("#A1");
				} catch (IndexOutOfRangeException ex) {
					// No data exists for the row/column
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				Assert.IsFalse (rdr.Read (), "#B");

				try {
					rdr.GetChars (-1, 0, (char []) null, 0, 0);
					Assert.Fail ("#C1");
				} catch (IndexOutOfRangeException ex) {
					// No data exists for the row/column
					Assert.IsNull (ex.InnerException, "#C3");
					Assert.IsNotNull (ex.Message, "#C4");
				}
			}
		}

		[Test]
		public void GetDataTypeName ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();

				switch (ConnectionManager.Instance.Sql.EngineConfig.Type) {
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
					Assert.Fail ("#A1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				try {
					reader.GetDataTypeName (6);
					Assert.Fail ("#B1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
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
					Assert.Fail ("#A1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				try {
					reader.GetFieldType (6);
					Assert.Fail ("#B1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
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
					Assert.Fail ("#A1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				try {
					reader.GetName (6);
					Assert.Fail ("#B1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
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
				Assert.AreEqual (3, reader.GetOrdinal ("doB"), "#4");
				Assert.AreEqual (4, reader.GetOrdinal ("doj"), "#5");
				Assert.AreEqual (5, reader.GetOrdinal ("EmaiL"), "#6");
				Assert.AreEqual (0, reader.GetOrdinal ("iD"), "#7");
				Assert.AreEqual (5, reader.GetOrdinal ("eMail"), "#8");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test]
		public void GetOrdinal_Name_NotFound ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				try {
					reader.GetOrdinal ("non_existing_column");
					Assert.Fail ("#A1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				try {
					reader.GetOrdinal (string.Empty);
					Assert.Fail ("#B1");
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
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
			cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				rdr.Close ();

				try {
					rdr.GetOrdinal (null);
					Assert.Fail ("#A1");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}
			}
		}

		[Test]
		public void GetOrdinal_Reader_NoData ()
		{
			cmd.CommandText = "SELECT * FROM employee WHERE id = 666";

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				Assert.AreEqual (0, rdr.GetOrdinal ("id"), "#A1");
				Assert.AreEqual (5, rdr.GetOrdinal ("eMail"), "#A2");

				Assert.IsFalse (rdr.Read (), "#B");

				Assert.AreEqual (2, rdr.GetOrdinal ("lname"), "#C1");
				Assert.AreEqual (3, rdr.GetOrdinal ("dob"), "#C2");
			}
		}

		[Test]
		public void GetSchemaTable_Command_Disposed ()
		{
			if (RunningOnMono)
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

		[Test] // this [Int32]
		public void Indexer1 ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read ());
				Assert.AreEqual (1, reader [0], "#0");
				Assert.AreEqual ("suresh", reader [1], "#1");
				Assert.AreEqual ("kumar", reader [2], "#2");
				Assert.AreEqual (new DateTime (1978, 8, 22), reader [3], "#3");
				Assert.AreEqual (new DateTime (2001, 3, 12), reader [4], "#4");
				Assert.AreEqual ("suresh@gmail.com", reader [5], "#5");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test] // this [Int32]
		public void Indexer1_Reader_Closed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "select fname from employee";
				reader = cmd.ExecuteReader ();
				reader.Read ();
				reader.Close ();

				try {
					object value = reader [0];
					Assert.Fail ("#A1:" + value);
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				reader = cmd.ExecuteReader ();
				reader.Close ();

				try {
					object value = reader [0];
					Assert.Fail ("#B1:" + value);
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test] // this [Int32]
		public void Indexer1_Reader_NoData ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "select fname from employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				try {
					object value = reader [0];
					Assert.Fail ("#A1:" + value);
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				Assert.IsTrue (reader.Read ());
				Assert.IsFalse (reader.Read ());

				try {
					object value = reader [0];
					Assert.Fail ("#B1:" + value);
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test] // this [Int32]
		public void Indexer1_Value_Invalid ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "select fname from employee";
				reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read ());
				try {
					object value = reader [-1];
					Assert.Fail ("#A1:" + value);
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				try {
					object value = reader [1];
					Assert.Fail ("#B1:" + value);
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test] // this [String]
		public void Indexer2 ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "SELECT * FROM employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read ());
				Assert.AreEqual (1, reader ["id"], "id");
				Assert.AreEqual ("suresh", reader ["fname"], "fname");
				Assert.AreEqual ("kumar", reader ["lname"], "lname");
				Assert.AreEqual (new DateTime (1978, 8, 22), reader ["doB"], "doB");
				Assert.AreEqual (new DateTime (2001, 3, 12), reader ["doj"], "doj");
				Assert.AreEqual ("suresh@gmail.com", reader ["EmaiL"], "EmaiL");
				Assert.AreEqual (1, reader ["iD"], "iD");
				Assert.AreEqual ("suresh@gmail.com", reader ["eMail"], "eMail");
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test] // this [String]
		public void Indexer2_Reader_Closed ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "select fname from employee";
				reader = cmd.ExecuteReader ();
				reader.Read ();
				reader.Close ();

				try {
					object value = reader ["fname"];
					Assert.Fail ("#A1:" + value);
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				reader = cmd.ExecuteReader ();
				reader.Close ();

				try {
					object value = reader ["fname"];
					Assert.Fail ("#B1:" + value);
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test] // this [String]
		public void Indexer2_Reader_NoData ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "select fname from employee WHERE lname='kumar'";
				reader = cmd.ExecuteReader ();

				try {
					object value = reader ["fname"];
					Assert.Fail ("#A1:" + value);
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				Assert.IsTrue (reader.Read ());
				Assert.IsFalse (reader.Read ());

				try {
					object value = reader ["fname"];
					Assert.Fail ("#B1:" + value);
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test] // this [String]
		public void Indexer2_Value_NotFound ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "select fname from employee";
				reader = cmd.ExecuteReader ();
				Assert.IsTrue (reader.Read ());
				try {
					object value = reader ["address"];
					Assert.Fail ("#A1:" + value);
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				try {
					object value = reader [string.Empty];
					Assert.Fail ("#B1:" + value);
				} catch (IndexOutOfRangeException ex) {
					// Index was outside the bounds of the array
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
				}
			} finally {
				if (reader != null)
					reader.Close ();
			}
		}

		[Test] // this [String]
		public void Indexer2_Value_Null ()
		{
			IDataReader reader = null;

			try {
				cmd.CommandText = "select fname from employee";
				reader = cmd.ExecuteReader ();
				try {
					object value = reader [(string) null];
					Assert.Fail ("#1:" + value);
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
				ConnectionManager.Instance.Sql.CloseConnection ();
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
		public void GetValue_Reader_Closed ()
		{
			//Console.WriteLine ("GetValue_Reader_Closed - first_executereader");
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read ());
				reader.Close ();

				try {
					reader.GetValue (-1);
					Assert.Fail ("#1");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			}
		}

		[Test]
		public void GetValue_Reader_NoData ()
		{
			//Console.WriteLine ("GetValue_Reader_NoData - first_executereader");
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 666";

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				try {
					rdr.GetValue (-1);
					Assert.Fail ("#A1");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				Assert.IsFalse (rdr.Read (), "#B");

				try {
					rdr.GetValue (-1);
					Assert.Fail ("#C1");
				} catch (InvalidOperationException ex) {
					// No data exists for the row/column
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
					Assert.IsNull (ex.InnerException, "#C3");
					Assert.IsNotNull (ex.Message, "#C4");
				}
			}
		}

		[Test]
		public void GetValue_Type_Binary ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_binary from binary_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = new byte [] { 0x35, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00 };

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#A5");

				expected = new byte [] { 0x00, 0x33, 0x34, 0x00,
					0x33, 0x30, 0x35, 0x31 };

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#B5");

				expected = new byte [8];

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (DBNull.Value, value, "#D4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#D5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = new byte [] { 0x35, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00 };

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#E5");

				expected = new byte [] { 0x00, 0x33, 0x34, 0x00,
					0x33, 0x30, 0x35, 0x31 };

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#F5");

				expected = new byte [8];

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetValue_Type_Image ()
		{
			object value;
			object expected;

			//Console.WriteLine ("GetValue_Type_Image - first_executereader");
			cmd.CommandText = "select type_blob from binary_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = new byte [] { 0x32, 0x56, 0x00,
					0x44, 0x22 };

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#A5");

				expected = long_bytes;

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#B5");

				expected = new byte [0];

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (DBNull.Value, value, "#D4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#D5");
			}

			//Console.WriteLine ("GetChars_Reader_Closed - second_executereader");
			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = new byte [] { 0x32, 0x56, 0x00,
					0x44, 0x22 };

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#E5");

				expected = long_bytes;

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#F5");

				expected = new byte [0];

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetValue_Type_Integer ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_int from numeric_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = int.MaxValue;

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (int), rdr.GetFieldType (0), "#A5");

				expected = int.MinValue;

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (int), rdr.GetFieldType (0), "#B5");

				expected = 0;

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (int), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (DBNull.Value, value, "#D4");
				Assert.AreEqual (typeof (int), rdr.GetFieldType (0), "#D5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = int.MaxValue;

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (int), rdr.GetFieldType (0), "#E5");

				expected = int.MinValue;

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (int), rdr.GetFieldType (0), "#F5");

				expected = 0;

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (int), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (int), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetValue_Type_NText ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_ntext from string_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = "nt\u092d\u093ext";

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#A5");

				expected = "nt\u092d\u093ext ";

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#B5");

				expected = string.Empty;

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (expected, value, "#D4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#D5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = "nt\u092d\u093ext";

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#E5");

				expected = "nt\u092d\u093ext ";

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#F5");

				expected = string.Empty;

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetValue_Type_NVarChar ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_nvarchar from string_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = "nv\u092d\u093e\u0930\u0924r";

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#A5");

				expected = "nv\u092d\u093e\u0930\u0924r ";

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#B5");

				expected = string.Empty;

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (expected, value, "#D4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#D5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = "nv\u092d\u093e\u0930\u0924r";

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#E5");

				expected = "nv\u092d\u093e\u0930\u0924r ";

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#F5");

				expected = string.Empty;

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetValue_Type_Real ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_float from numeric_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = 3.40E+38F;

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (float), rdr.GetFieldType (0), "#A5");

				expected = -3.40E+38F;

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (float), rdr.GetFieldType (0), "#B5");

				expected = 0F;

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (float), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (expected, value, "#D4");
				Assert.AreEqual (typeof (float), rdr.GetFieldType (0), "#D5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = 3.40E+38F;

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (float), rdr.GetFieldType (0), "#E5");

				expected = -3.40E+38F;

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (float), rdr.GetFieldType (0), "#F5");

				expected = 0F;

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (float), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (float), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetValue_Type_SmallDateTime ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_smalldatetime from datetime_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = DateTime.Parse ("2037-12-31 23:59:00");

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (DateTime), rdr.GetFieldType (0), "#A5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (DateTime), rdr.GetFieldType (0), "#B5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = DateTime.Parse ("2037-12-31 23:59:00");

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (DateTime), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (expected, value, "#D4");
				Assert.AreEqual (typeof (DateTime), rdr.GetFieldType (0), "#D5");
			}
		}

		[Test]
		public void GetValue_Type_SmallInt ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_smallint from numeric_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = short.MaxValue;

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (short), rdr.GetFieldType (0), "#A5");

				expected = short.MinValue;

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (short), rdr.GetFieldType (0), "#B5");

				expected = (short) 0;

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (short), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (expected, value, "#D4");
				Assert.AreEqual (typeof (short), rdr.GetFieldType (0), "#D5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = short.MaxValue;

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (short), rdr.GetFieldType (0), "#E5");

				expected = short.MinValue;

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (short), rdr.GetFieldType (0), "#F5");

				expected = (short) 0;

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (short), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (short), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetValue_Type_Text ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_text from string_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = "text";

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#A5");

				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < 30; i++)
					sb.Append ("longtext ");
				expected = sb.ToString ();

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#B5");

				expected = string.Empty;

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (expected, value, "#D4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#D5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = "text";

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#E5");

				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < 30; i++)
					sb.Append ("longtext ");
				expected = sb.ToString ();

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#F5");

				expected = string.Empty;

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetValue_Type_TinyInt ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_tinyint from numeric_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = byte.MaxValue;

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (byte), rdr.GetFieldType (0), "#A5");

				expected = byte.MinValue;

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (byte), rdr.GetFieldType (0), "#B5");

				expected = (byte) 0x00;

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (byte), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (expected, value, "#D4");
				Assert.AreEqual (typeof (byte), rdr.GetFieldType (0), "#D5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = byte.MaxValue;

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (byte), rdr.GetFieldType (0), "#E5");

				expected = byte.MinValue;

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (byte), rdr.GetFieldType (0), "#F5");

				expected = (byte) 0x00;

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (byte), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (byte), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetValue_Type_VarBinary ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_varbinary from binary_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = new byte [] { 0x30, 0x31, 0x32, 0x33,
					0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30,
					0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
					0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34,
					0x35, 0x36, 0x37, 0x38, 0x39, 0x00, 0x44,
					0x53};

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#A5");

				expected = new byte [] { 0x00, 0x39, 0x38, 0x37,
					0x36, 0x35, 0x00, 0x33, 0x32, 0x31, 0x30,
					0x31, 0x32, 0x33, 0x34 };

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#B5");

				expected = new byte [0];

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (DBNull.Value, value, "#D4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#D5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = new byte [] { 0x30, 0x31, 0x32, 0x33,
					0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30,
					0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
					0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34,
					0x35, 0x36, 0x37, 0x38, 0x39, 0x00, 0x44,
					0x53};

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#E5");

				expected = new byte [] { 0x00, 0x39, 0x38, 0x37,
					0x36, 0x35, 0x00, 0x33, 0x32, 0x31, 0x30,
					0x31, 0x32, 0x33, 0x34 };

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#F5");

				expected = new byte [0];

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetValue_Type_VarChar ()
		{
			object value;
			object expected;

			cmd.CommandText = "select type_varchar from string_family order by id asc";
			using (IDataReader rdr = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = "varchar";

				Assert.IsTrue (rdr.Read (), "#A1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#A2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#A3");
				Assert.AreEqual (expected, value, "#A4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#A5");

				expected = "varchar ";

				Assert.IsTrue (rdr.Read (), "#B1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#B2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#B3");
				Assert.AreEqual (expected, value, "#B4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#B5");

				expected = string.Empty;

				Assert.IsTrue (rdr.Read (), "#C1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#C2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#C3");
				Assert.AreEqual (expected, value, "#C4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#C5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#D1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#D2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#D3");
				Assert.AreEqual (expected, value, "#D4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#D5");
			}

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				expected = "varchar";

				Assert.IsTrue (rdr.Read (), "#E1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#E2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#E3");
				Assert.AreEqual (expected, value, "#E4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#E5");

				expected = "varchar ";

				Assert.IsTrue (rdr.Read (), "#F1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#F2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#F3");
				Assert.AreEqual (expected, value, "#F4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#F5");

				expected = string.Empty;

				Assert.IsTrue (rdr.Read (), "#G1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#G2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#G3");
				Assert.AreEqual (expected, value, "#G4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#G5");

				expected = DBNull.Value;

				Assert.IsTrue (rdr.Read (), "#H1");
				value = rdr.GetValue (0);
				Assert.IsNotNull (value, "#H2");
				Assert.AreEqual (expected.GetType (), value.GetType (), "#H3");
				Assert.AreEqual (expected, value, "#H4");
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0), "#H5");
			}
		}

		[Test]
		public void GetBytes ()
		{
			//Console.WriteLine ("GetBytes - first_executereader");
			byte [] expected = new byte [] { 0x32, 0x56, 0x00, 0x44, 0x22 };
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read (), "#A1");

				// Get By Parts for the column blob
				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				int buffsize = 3;
				int offset = 0;
				long ret = 0;
				long count = 0;
				byte [] val = new byte [totalsize];
				//Console.WriteLine ("GetBytes:: totalsize={0}", totalsize);
				do {
					ret = reader.GetBytes (0, offset, val, offset,
						(int) Math.Min (buffsize, totalsize - count));
					offset += (int) ret;
					count += ret;
				} while (count < totalsize);

				Assert.AreEqual (expected.Length, count, "#A2");
				Assert.AreEqual (expected, val, "#A3");
			}

			//Console.WriteLine ("GetBytes - second_executereader");
			using (IDataReader reader = cmd.ExecuteReader ()) {
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

				Assert.AreEqual (expected.Length, count, "#B2");
				Assert.AreEqual (expected, val, "#B3");
			}

			//Console.WriteLine ("GetBytes - third_executereader");
			// buffer size > (buffer offset + length) > remaining data
			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize + 5];
				int buffer_offset = 3;

				long ret = reader.GetBytes (0, 0, val, buffer_offset, (int) totalsize);
				Assert.AreEqual (expected.Length, ret, "#C1");
				for (int i = 0; i < buffer_offset; i++)
					Assert.AreEqual (0x00, val [i], "#C2:" + i);
				for (int i = 0; i < totalsize; i++)
					Assert.AreEqual (expected [i], val [buffer_offset + i], "#C3:" + i);
			}

			//Console.WriteLine ("GetBytes - fourth_executereader");
			// buffer size > (buffer offset + length) > remaining data
			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize + 5];
				int buffer_offset = 3;

				long ret = reader.GetBytes (0, 0, val, buffer_offset, (int) totalsize);
				Assert.AreEqual (expected.Length, ret, "#D1");
				for (int i = 0; i < buffer_offset; i++)
					Assert.AreEqual (0x00, val [i], "#D2:" + i);
				for (int i = 0; i < totalsize; i++)
					Assert.AreEqual (expected [i], val [buffer_offset + i], "#D3:" + i);
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

			//Console.WriteLine ("GetBytes - fifth_executereader");
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
			//Console.WriteLine ("GetBytes_Buffer_Null- first_executereader");
			cmd.CommandText = "SELECT type_blob FROM binary_family where id in (1,2,3,4) order by id";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read (), "#A1");
				Assert.AreEqual (5, reader.GetBytes (0, 0, null, 0, 0), "#A2");

				Assert.IsTrue (reader.Read (), "#B1");
				Assert.AreEqual (275, reader.GetBytes (0, 0, null, 0, 0), "#B2");

				Assert.IsTrue (reader.Read (), "#C1");
				Assert.AreEqual (0, reader.GetBytes (0, 0, null, 0, 0), "#C2");

				Assert.IsTrue (reader.Read (), "#D1");
				if (conn is SqlConnection) {
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
				} else {
					Assert.AreEqual (-1, reader.GetBytes (0, 0, null, 0, 0), "#D2");
				}
			}

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read (), "#E1");
				Assert.AreEqual (5, reader.GetBytes (0, 5, null, 3, 8), "#E2");

				Assert.IsTrue (reader.Read (), "#F1");
				Assert.AreEqual (275, reader.GetBytes (0, 5, null, 3, 4), "#F2");

				Assert.IsTrue (reader.Read (), "#G1");
				Assert.AreEqual (0, reader.GetBytes (0, 5, null, 3, 4), "#G2");

				Assert.IsTrue (reader.Read (), "#H1");
				if (conn is SqlConnection) {
					try {
						reader.GetBytes (0, 5, null, 3, 4);
						Assert.Fail ("#H2");
					} catch (SqlNullValueException ex) {
						// Data is Null. This method or
						// property cannot be called on
						// Null values
						Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "#H3");
						Assert.IsNull (ex.InnerException, "#H4");
						Assert.IsNotNull (ex.Message, "#H5");
					}
				} else {
					Assert.AreEqual (-1, reader.GetBytes (0, 5, null, 3, 4), "#H2");
				}
			}

			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read (), "#I1");
				Assert.AreEqual (5, reader.GetBytes (0, 0, null, 0, 0), "#I2");

				Assert.IsTrue (reader.Read (), "#J1");
				Assert.AreEqual (275, reader.GetBytes (0, 0, null, 0, 0), "#J2");

				Assert.IsTrue (reader.Read (), "#K1");
				Assert.AreEqual (0, reader.GetBytes (0, 0, null, 0, 0), "#K2");

				Assert.IsTrue (reader.Read (), "#L1");
				if (conn is SqlConnection) {
					try {
						reader.GetBytes (0, 0, null, 0, 0);
						Assert.Fail ("#L2");
					} catch (SqlNullValueException ex) {
						// Data is Null. This method or
						// property cannot be called on
						// Null values
						Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "#L3");
						Assert.IsNull (ex.InnerException, "#L4");
						Assert.IsNotNull (ex.Message, "#L5");
					}
				} else {
					if (RunningOnMono)
						Assert.AreEqual (-1, reader.GetBytes (0, 0, null, 0, 0), "#L2");
					else {
						try {
							reader.GetBytes (0, 0, null, 0, 0);
							Assert.Fail ("#L2");
						} catch (InvalidCastException) {
							// Unable to cast object of type
							// 'System.DBNull' to type 'System.Byte[]'
						}
					}
				}
			}

			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read (), "#M1");
				Assert.AreEqual (5, reader.GetBytes (0, 5, null, 3, 8), "#M2");

				Assert.IsTrue (reader.Read (), "#N1");
				Assert.AreEqual (275, reader.GetBytes (0, 5, null, 3, 4), "#N2");

				Assert.IsTrue (reader.Read (), "#O1");
				Assert.AreEqual (0, reader.GetBytes (0, 5, null, 3, 4), "#O2");

				Assert.IsTrue (reader.Read (), "#P1");
				if (conn is SqlConnection) {
					try {
						reader.GetBytes (0, 5, null, 3, 4);
						Assert.Fail ("#P2");
					} catch (SqlNullValueException ex) {
						// Data is Null. This method or
						// property cannot be called on
						// Null values
						Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "#P3");
						Assert.IsNull (ex.InnerException, "#P4");
						Assert.IsNotNull (ex.Message, "#P5");
					}
				} else {
					if (RunningOnMono)
						Assert.AreEqual (-1, reader.GetBytes (0, 0, null, 0, 0), "#L2");
					else {
						try {
							reader.GetBytes (0, 0, null, 0, 0);
							Assert.Fail ("#L2");
						} catch (InvalidCastException) {
							// Unable to cast object of type
							// 'System.DBNull' to type 'System.Byte[]'
						}
					}
				}
			}
		}

		[Test]
		[Category("NotWorking")]
		public void GetBytes_DataIndex_Overflow ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 2";

			//Console.WriteLine ("GetBytes_DataIndex_Overflow - first_executereader");
			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize * 2];
				long ret;

				// dataIndex > total size, length = 0
				ret = reader.GetBytes (0, totalsize + 5, val, 0, 0);
				Assert.AreEqual (0, ret, "#C1");
				// dataIndex > total size, length < total size
				ret = reader.GetBytes (0, totalsize + 5, val, 0, 5);
				Assert.AreEqual (0, ret, "#C2");
				// dataIndex > total size, length > total size
				ret = reader.GetBytes (0, totalsize + 5, val, 0, (int) (totalsize + 5));
				Assert.AreEqual (0, ret, "#C3");
			}

			//Console.WriteLine ("GetBytes_DataIndex_Overflow - second_executereader");
			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize * 2];
				long ret;

				// dataIndex > total size, length = 0
				ret = reader.GetBytes (0, totalsize + 5, val, 0, 0);
				Assert.AreEqual (0, ret, "#B1");
				// dataIndex > total size, length < total size
				ret = reader.GetBytes (0, totalsize + 5, val, 0, 5);
				Assert.AreEqual (0, ret, "#B2");
				// dataIndex > total size, length > total size
				ret = reader.GetBytes (0, totalsize + 5, val, 0, (int) (totalsize + 5));
				Assert.AreEqual (0, ret, "#B3");
			}
		}

		[Test]
		public void GetBytes_DataIndex_OffSet ()
		{
			//Console.WriteLine ("GetBytes_DataIndex_Offset - first_executereader");
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

			//Console.WriteLine ("GetBytes_DataIndex_Offset - second_executereader");
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
			//Console.WriteLine ("GetBytes_Reader_Closed - first_executereader");
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

#if DONT_RUN
		[Test]
		public void GetBytes_Reader_NoData ()
		{
			//Console.WriteLine ("GetBytes_Reader_NoData - first_executereader");
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
#endif 
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

		[Test]
		[Category("NotWorking")]
		public void GetValues_Reader_Closed ()
		{
			//Console.WriteLine ("GetValues_Reader_Closed - first_executereader");
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				Assert.IsTrue (rdr.Read ());
				rdr.Close ();

				try {
					rdr.GetValues ((object []) null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					// No data exists for the row/column
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			}
		}

		[Test]
		[Category("NotWorking")]
		public void GetValues_Reader_NoData ()
		{
			//Console.WriteLine ("GetValues_Reader_NoData - first_executereader");			
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 666";

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				try {
					rdr.GetValues ((object []) null);
					Assert.Fail ("#A1");
				} catch (ArgumentNullException ex) {
					// No data exists for the row/column
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				Assert.IsFalse (rdr.Read (), "#B");

				try {
					rdr.GetValues ((object []) null);
					Assert.Fail ("#C1");
				} catch (ArgumentNullException ex) {
					// No data exists for the row/column
					Assert.IsNull (ex.InnerException, "#C3");
					Assert.IsNotNull (ex.Message, "#C4");
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
