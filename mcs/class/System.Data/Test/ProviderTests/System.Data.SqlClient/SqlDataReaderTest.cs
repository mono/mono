//
// SqlDataReaderTest.cs - NUnit Test Cases for testing the
//                          SqlDataReader class
// Author:
//      Umadevi S (sumadevi@novell.com)
//      Kornél Pál <http://www.kornelpal.hu/>
//	Sureshkumar T (tsureshkumar@novell.com)
//	Senganal T (tsenganal@novell.com)
//	Veerapuram Varadhan (vvaradhan@novell.com)
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
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Data.Connected.SqlClient
{
	[TestFixture]
	[Category ("sqlserver")]
	[Category("NotWorking")]
	public class SqlDataReaderTest
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

		SqlConnection conn = null;
		SqlCommand cmd = null;
		SqlDataReader reader = null;
		String query = "Select type_{0},type_{2},convert({1},null) from numeric_family where id=1";
		DataSet sqlDataset = null;
		EngineConfig engine;

		DataTable numericDataTable =null;
		DataTable stringDataTable =null;
		DataTable binaryDataTable =null;
		DataTable datetimeDataTable =null;

		DataRow numericRow = null; 
		DataRow stringRow = null; 
		DataRow binaryRow = null; 
		DataRow datetimeRow = null; 

		[TestFixtureSetUp]
		public void init ()
		{
			conn = new SqlConnection (ConnectionManager.Instance.Sql.ConnectionString);
			cmd = conn.CreateCommand ();
			
			sqlDataset = (new DataProvider()).GetDataSet ();

			numericDataTable = sqlDataset.Tables["numeric_family"];
			stringDataTable = sqlDataset.Tables["string_family"];
			binaryDataTable = sqlDataset.Tables["binary_family"];
			datetimeDataTable = sqlDataset.Tables["datetime_family"];

			numericRow = numericDataTable.Select ("id=1")[0];
			stringRow = stringDataTable.Select ("id=1")[0];
			binaryRow = binaryDataTable.Select ("id=1")[0];
			datetimeRow = datetimeDataTable.Select ("id=1")[0];

		}

		[SetUp]
		public void Setup ()
		{
			conn.Open ();
			engine = ConnectionManager.Instance.Sql.EngineConfig;
		}

		[TearDown]
		public void TearDown ()
		{
			reader?.Close ();
			conn?.Close ();
		}

		[Test]
		public void ReadEmptyNTextFieldTest ()
		{
			try {
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
				ConnectionManager.Instance.Sql.CloseConnection ();
			}
		}

		[Test]
		public void ReadBigIntTest()
		{
			if (ClientVersion <= 7)
				Assert.Ignore ("BigInt data type is not supported.");

			try {
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
				ConnectionManager.Instance.Sql.CloseConnection ();
			}
		}

		// This method just helps in Calling common tests among all the Get* Methods 
		// without replicating code 

		void CallGetMethod (string s, int i)
		{
			switch (s) {
			case "Boolean" : reader.GetBoolean (i) ; break; 
			case "SqlBoolean": reader.GetSqlBoolean (i); break;
			case "Int16" : reader.GetInt16 (i); break;
			case "SqlInt16" : reader.GetSqlInt16 (i); break;
			case "Int32" : reader.GetInt32 (i);break;
			case "SqlInt32" : reader.GetSqlInt32(i);break;
			case "Int64" : reader.GetInt64 (i);break;
			case "SqlInt64" : reader.GetSqlInt64(i); break;
			case "Decimal" : reader.GetDecimal(i);break;
			case "SqlDecimal" : reader.GetSqlDecimal (i);break;
			case "SqlMoney" : reader.GetSqlMoney (i);break;
			case "Float" : reader.GetFloat (i);break;
			case "SqlSingle" : reader.GetSqlSingle(i);break;
			case "Double" : reader.GetDouble (i);break;
			case "SqlDouble" : reader.GetSqlDouble(i);break;
			case "Guid" : reader.GetGuid(i);break;
			case "SqlGuid" : reader.GetSqlGuid(i);break;
			case "String" : reader.GetString(i);break;
			case "SqlString" : reader.GetSqlString(i);break;
			case "Char" : reader.GetChar(i);break;
			case "Byte" : reader.GetByte (i);break;
			case "SqlByte" : reader.GetSqlByte(i); break;
			case "DateTime" : reader.GetDateTime(i); break;
			case "SqlDateTime" : reader.GetSqlDateTime(i); break;
			case "SqlBinary" : reader.GetSqlBinary(i); break;
			default : Console.WriteLine ("OOOOPSSSSSS {0}",s);break;
			}
		}

		// This method just helps in Calling common tests among all the Get* Methods 
		// without replicating code 
		void GetMethodTests (string s)
		{

			try {
				CallGetMethod (s, 1);
				Assert.Fail ("#1[Get"+s+"] InvalidCastException must be thrown");
			} catch (InvalidCastException e) {
				Assert.AreEqual (typeof (InvalidCastException), e.GetType (),
					"#2[Get"+s+"] Incorrect Exception : " + e);
			}
		
			// GetSql* Methods do not throw SqlNullValueException	
			// So, Testimg only for Get* Methods 
			if (!s.StartsWith("Sql")) {
				try {
					CallGetMethod (s, 2);
					Assert.Fail ("#3[Get"+s+"] Exception must be thrown");
				} catch (SqlNullValueException e) {
					Assert.AreEqual (typeof (SqlNullValueException), e.GetType (),
						"#4[Get"+s+"] Incorrect Exception : " + e);
				}
			}

			try {
				CallGetMethod (s, 3);
				Assert.Fail ("#5[Get"+s+"] IndexOutOfRangeException must be thrown");
			} catch (IndexOutOfRangeException e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (),
					"#6[Get"+s+"] Incorrect Exception : " + e);
			}
		}

		[Test]
		public void GetBooleanTest ()
		{
			cmd.CommandText = string.Format (query, "bit", "bit", "int");
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("Boolean");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow ["type_bit"], reader.GetBoolean (0),
						"#2 DataValidation Failed");
			
			// Test for standard exceptions 
			GetMethodTests("SqlBoolean");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_bit"], reader.GetSqlBoolean(0).Value,
				"#4 DataValidation Failed");	
			reader.Close ();
		}

		[Test]
		public void GetByteTest ()
		{
			cmd.CommandText = string.Format (query, "tinyint", "tinyint", "int");
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("Byte");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_tinyint"], reader.GetByte(0),
						"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlByte");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_tinyint"], reader.GetSqlByte(0).Value,
						"#4 DataValidation Failed");
			reader.Close ();
		}

		[Test]
		public void GetInt16Test ()
		{
			cmd.CommandText = string.Format (query, "smallint", "smallint", "int");
			reader = cmd.ExecuteReader();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("Int16");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_smallint"], reader.GetInt16(0),
						"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlInt16");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_smallint"], reader.GetSqlInt16(0).Value,
						"#4 DataValidation Failed");
			reader.Close ();
		}

		[Test]
		public void GetInt32Test ()
		{
			if (ClientVersion == 7)
				cmd.CommandText = string.Format (query, "int", "int", "decimal1");
			else
				cmd.CommandText = string.Format (query, "int", "int", "bigint");

			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("Int32");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_int"], reader.GetInt32(0),
				"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlInt32");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_int"], reader.GetSqlInt32(0).Value,
				"#4 DataValidation Failed");
			reader.Close ();
		}

		[Test]
		public void GetInt64Test ()
		{
			if (ClientVersion == 7)
				Assert.Ignore ("BigInt data type is not supported.");

			cmd.CommandText = string.Format (query, "bigint", "bigint", "int");
			reader = cmd.ExecuteReader ();
			reader.Read ();

			object value;

			// Test for standard exceptions
			GetMethodTests("Int64");

			// Test if data is returned correctly
			value = reader.GetInt64 (0);
			Assert.AreEqual (numericRow ["type_bigint"], value, "#A");

			// Test for standard exceptions
			GetMethodTests("SqlInt64");

			// Test if data is returned correctly
			value = reader.GetSqlInt64 (0);
			Assert.IsNotNull (value, "#B1");
			Assert.AreEqual (typeof (SqlInt64), value.GetType (), "#B2");
			SqlInt64 sqlValue = (SqlInt64) value;
			Assert.IsFalse (sqlValue.IsNull, "#B3");
			Assert.AreEqual (numericRow ["type_bigint"], sqlValue.Value, "#B4");

			value = reader.GetValue (0);
			Assert.IsNotNull (value, "#C1");
			Assert.AreEqual (typeof (long), value.GetType (), "#C2");
			Assert.AreEqual (numericRow ["type_bigint"], value, "#C3");

			reader.Close ();
		}

		[Test]
		public void GetDecimalTest ()
		{
			cmd.CommandText = string.Format (query, "decimal1", "decimal", "int");
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("Decimal");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_decimal1"], reader.GetDecimal(0),
				"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlDecimal");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_decimal1"], reader.GetSqlDecimal(0).Value,
				"#4 DataValidation Failed");
			reader.Close ();
		}

		//#613087, #620860 Test
		[Test]
		public void GetDecimalOfInt64Test_Max ()
		{
			string crTable = "CREATE TABLE #613087 (decimalint64 decimal(20,0))";
			//string drTable = "drop table #613087";
			
			cmd.CommandText = crTable;
			cmd.CommandType = CommandType.Text;
			cmd.ExecuteNonQuery ();
			
			cmd.CommandText = "INSERT INTO #613087 VALUES (@decimalint64)";
			SqlParameter param = new SqlParameter ();
			param.ParameterName = "@decimalint64";
			param.Value = new SqlDecimal ((long)Int64.MaxValue);
			cmd.Parameters.Add (param);
			cmd.ExecuteNonQuery ();
			
			cmd.Parameters.Clear ();
			cmd.CommandText = "Select * from #613087";
			reader = cmd.ExecuteReader();
			reader.Read ();
			Assert.AreEqual (param.Value, reader.GetSqlDecimal (0), "SqlDecimalFromInt64_Max Test failed");
		}
		
		//#613087, #620860 Test
		[Test]
		public void GetDecimalOfInt64Test_Min ()
		{
			string crTable = "CREATE TABLE #613087 (decimalint64 decimal(20,0))";
			//string drTable = "drop table #613087";
			
			cmd.CommandText = crTable;
			cmd.CommandType = CommandType.Text;
			cmd.ExecuteNonQuery ();
			
			cmd.CommandText = "INSERT INTO #613087 VALUES (@decimalint64)";
			SqlParameter param = new SqlParameter ();
			param.ParameterName = "@decimalint64";
			param.Value = new SqlDecimal ((long)Int64.MinValue);
			cmd.Parameters.Add (param);
			cmd.ExecuteNonQuery ();
			
			cmd.Parameters.Clear ();
			cmd.CommandText = "Select * from #613087";
			reader = cmd.ExecuteReader();
			reader.Read ();
			Assert.AreEqual (param.Value, reader.GetSqlDecimal (0), "SqlDecimalFromInt64_Min Test failed");
		}

		//#613087, #620860 Test
		[Test]
		public void GetDecimalOfInt64Test_Any ()
		{
			string crTable = "CREATE TABLE #613087 (decimalint64 decimal(20,0))";
			//string drTable = "drop table #613087";
			
			cmd.CommandText = crTable;
			cmd.CommandType = CommandType.Text;
			cmd.ExecuteNonQuery ();
			
			cmd.CommandText = "INSERT INTO #613087 VALUES (@decimalint64)";
			SqlParameter param = new SqlParameter ();
			param.ParameterName = "@decimalint64";
			param.DbType = DbType.Decimal;
			param.Value = ulong.MaxValue;
			cmd.Parameters.Add (param);
			cmd.ExecuteNonQuery ();
			
			cmd.Parameters.Clear ();
			cmd.CommandText = "Select * from #613087";
			reader = cmd.ExecuteReader();
			reader.Read ();
			Assert.AreEqual (param.Value, (ulong)reader.GetSqlDecimal (0).Value, "SqlDecimalFromInt64_Any Test failed");
		}

		[Test]
		public void GetSqlMoneyTest ()
		{
			cmd.CommandText = string.Format (query, "money", "money", "int");
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("SqlMoney");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_money"], reader.GetSqlMoney(0).Value,
				"#2 DataValidation Failed");
			reader.Close ();
		}

		[Test]
		public void GetFloatTest ()
		{
			cmd.CommandText = "select type_float,type_double,convert(real,null)";
			cmd.CommandText += "from numeric_family where id=1"; 
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("Float");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_float"], reader.GetFloat(0),
				"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlSingle");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_float"], reader.GetSqlSingle(0).Value,
				"#2 DataValidation Failed");
			reader.Close ();
		} 

		[Test]
		public void GetDoubleTest ()
		{
			cmd.CommandText = "select type_double,type_float,convert(float,null)";
			cmd.CommandText += " from numeric_family where id=1"; 
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("Double");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_double"], reader.GetDouble(0),
				"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlDouble");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_double"], reader.GetSqlDouble(0).Value,
				"#4 DataValidation Failed");
			reader.Close ();
		}

		[Test]
		public void GetBytesTest ()
		{
			cmd.CommandText = "Select type_text,type_ntext,convert(text,null) ";
			cmd.CommandText += "from string_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();
			try {
				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				Assert.AreEqual (4, totalsize, "#1");
			} finally {
				reader.Close ();
			}
			
			byte[] asciiArray = (new ASCIIEncoding ()).GetBytes ("text");
			byte[] unicodeArray = (new UnicodeEncoding ()).GetBytes ("nt\u092d\u093ext");
			byte[] buffer = null ;
			long size = 0; 

			reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess);
			reader.Read ();
			size = reader.GetBytes (0,0,null,0,0);
			Assert.AreEqual (asciiArray.Length, size, "#3 Data Incorrect");

			buffer = new byte[size];
			size = reader.GetBytes (0,0,buffer,0,(int)size);
			for (int i=0;i<size; i++)
				Assert.AreEqual (asciiArray[i], buffer[i], "#4 Data Incorrect");

			size = reader.GetBytes (1, 0, null, 0, 0);
			Assert.AreEqual (unicodeArray.Length, size, "#5 Data Incorrect");
			buffer = new byte[size];
			size = reader.GetBytes (1,0,buffer,0,(int)size);
			for (int i=0;i<size; i++)
				Assert.AreEqual (unicodeArray[i], buffer[i], "#6 Data Incorrect");
			
			// Test if msdotnet behavior s followed when null value 
			// is read using GetBytes 
			try {
				reader.GetBytes (2, 0, null, 0, 0);
				Assert.Fail ("#7");
			} catch (SqlNullValueException) {
			}

			try {
				reader.GetBytes (2, 0, buffer, 0, 10);
				Assert.Fail ("#8");
			} catch (SqlNullValueException) {
			}
			reader.Close ();
			// do i need to test for image/binary values also ??? 
		}

		[Test]
		public void GetBytes_BufferIndex_Negative ()
		{
			var conn = ConnectionManager.Instance.Sql.Connection;

			try {
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
					Assert.IsTrue (reader.Read (), "#1");

					long size = reader.GetBytes (0, 0, null, -1, 0);
					Assert.AreEqual (5, size);
				}
			} finally {
				ConnectionManager.Instance.Sql.CloseConnection ();
			}
		}

		[Test]
		public void GetBytes_DataIndex_Negative ()
		{
			var conn = ConnectionManager.Instance.Sql.Connection;

			try {
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
					Assert.IsTrue (reader.Read ());

					long totalsize = reader.GetBytes (0, -1L, null, 0, 0);
					Assert.AreEqual (5, totalsize, "#A");
				}

				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
					Assert.IsTrue (reader.Read ());

					byte [] val = new byte [5];
					try {
						reader.GetBytes (0, -1L, val, 0, 3);
						Assert.Fail ("#B1");
					} catch (InvalidOperationException ex) {
						// Invalid GetBytes attempt at dataIndex '-1'
						// With CommandBehavior.SequentialAccess,
						// you may only read from dataIndex '0'
						// or greater.
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
						Assert.IsNull (ex.InnerException, "#B3");
						Assert.IsNotNull (ex.Message, "#B4");
						Assert.IsTrue (ex.Message.IndexOf ("dataIndex") != -1, "#B5:" + ex.Message);
					}
				}

				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult)) {
					Assert.IsTrue (reader.Read ());

					try {
						reader.GetBytes (0, -1L, null, 0, 0);
						Assert.Fail ("#C1");
					} catch (InvalidOperationException ex) {
						// Invalid value for argument 'dataIndex'.
						// The value must be greater than or equal to 0
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
						Assert.IsNull (ex.InnerException, "#C3");
						Assert.IsNotNull (ex.Message, "#C4");
						Assert.IsTrue (ex.Message.IndexOf ("'dataIndex'") != -1, "#C5:" + ex.Message);
					}

					byte [] val = new byte [3];
					try {
						reader.GetBytes (0, -1L, val, 0, 3);
						Assert.Fail ("#D1");
					} catch (InvalidOperationException ex) {
						// Invalid value for argument 'dataIndex'.
						// The value must be greater than or equal to 0
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
						Assert.IsNull (ex.InnerException, "#D3");
						Assert.IsNotNull (ex.Message, "#D4");
						Assert.IsTrue (ex.Message.IndexOf ("'dataIndex'") != -1, "#D5:" + ex.Message);
					}
				}

				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult)) {
					Assert.IsTrue (reader.Read ());

					byte [] val = new byte [5];
					try {
						reader.GetBytes (0, -1L, val, 0, 3);
						Assert.Fail ("#E1");
					} catch (InvalidOperationException ex) {
						// Invalid value for argument 'dataIndex'.
						// The value must be greater than or equal to 0
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
						Assert.IsNull (ex.InnerException, "#E3");
						Assert.IsNotNull (ex.Message, "#E4");
						Assert.IsTrue (ex.Message.IndexOf ("'dataIndex'") != -1, "#E5:" + ex.Message);
					}
				}

				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
					Assert.IsTrue (reader.Read ());

					long totalsize = reader.GetBytes (0, -1L, null, 5, 8);
					Assert.AreEqual (5, totalsize, "#F");
				}

				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult)) {
					Assert.IsTrue (reader.Read ());

					try {
						reader.GetBytes (0, -1L, null, 4, 8);
						Assert.Fail ("#G1");
					} catch (InvalidOperationException ex) {
						// Invalid value for argument 'dataIndex'.
						// The value must be greater than or equal to 0
						Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#G2");
						Assert.IsNull (ex.InnerException, "#G3");
						Assert.IsNotNull (ex.Message, "#G4");
						Assert.IsTrue (ex.Message.IndexOf ("'dataIndex'") != -1, "#G5:" + ex.Message);
					}
				}
			} finally {
				ConnectionManager.Instance.Sql.CloseConnection ();
			}
		}

		[Test]
		public void GetBytes_Length_Negative ()
		{
			var conn = ConnectionManager.Instance.Sql.Connection;

			try {
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
					Assert.IsTrue (reader.Read (), "#A1");

					long size = reader.GetBytes (0, 0, null, 0, -1);
					Assert.AreEqual (5, size, "#A2");
				}

				using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult)) {
					Assert.IsTrue (reader.Read (), "#B1");

					long size = reader.GetBytes (0, 0, null, 0, -1);
					Assert.AreEqual (5, size, "#B2");
				}
			} finally {
				ConnectionManager.Instance.Sql.CloseConnection ();
			}
		}

		[Test]
		public void GetBytes_Buffer_TooSmall ()
		{
			cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 2";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize -1];

				try {
					reader.GetBytes (0, 0, val, 0, (int) totalsize);
					Assert.Fail ("#A1");
				} catch (IndexOutOfRangeException ex) {
					// Buffer offset '0' plus the bytes available
					// '275' is greater than the length of the
					// passed in buffer
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#A5:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("'" + totalsize + "'") != -1, "#A6:" + ex.Message);
				}
			}

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize];

				try {
					reader.GetBytes (0, 0, val, 1, (int) totalsize);
					Assert.Fail ("#B1");
				} catch (IndexOutOfRangeException ex) {
					// Buffer offset '1' plus the bytes available
					// '275' is greater than the length of the
					// passed in buffer
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("'1'") != -1, "#B5:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("'" + totalsize + "'") != -1, "#B6:" + ex.Message);
				}
			}

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				long totalsize = reader.GetBytes (0, 0, null, 0, 0);
				byte [] val = new byte [totalsize];

				try {
					reader.GetBytes (0, 0, val, 0, (int) (totalsize + 1));
					Assert.Fail ("#C1");
				} catch (IndexOutOfRangeException ex) {
					// Buffer offset '0' plus the bytes available
					// '277' is greater than the length of the
					// passed in buffer
					Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#C2");
					Assert.IsNull (ex.InnerException, "#C3");
					Assert.IsNotNull (ex.Message, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("'0'") != -1, "#C5:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("'" + (totalsize + 1) + "'") != -1, "#C6:" + ex.Message);
				}
			}
		}

		[Test]
		public void GetBytes ()
		{
			cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 2";

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
		public void GetBytes_Type_Binary ()
		{
			cmd.CommandText = "Select type_binary, type_varbinary, " +
				"type_blob from binary_family where id = 1";
			reader = cmd.ExecuteReader ();
			reader.Read ();
			byte[] binary = (byte[])reader.GetValue (0);
			byte[] varbinary = (byte[])reader.GetValue (1);
			byte[] image = (byte[])reader.GetValue (2);
			reader.Close ();

			reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess);
			reader.Read ();
			int len = 0;
			byte[] arr ;
			len = (int)reader.GetBytes (0,0,null,0,0);
			Assert.AreEqual (binary.Length, len, "#1");
			arr = new byte [len];
			reader.GetBytes (0,0,arr,0,len);
			for (int i=0; i<len; ++i)
				Assert.AreEqual (binary[i], arr[i], "#2");


			len = (int)reader.GetBytes (1,0,null,0,0);
			Assert.AreEqual (varbinary.Length, len, "#1");
			arr = new byte [len];
			reader.GetBytes (1,0,arr,0,len);
			for (int i=0; i<len; ++i)
				Assert.AreEqual (varbinary[i], arr[i], "#2");

			len = (int)reader.GetBytes (2,0,null,0,0);
			Assert.AreEqual (image.Length, len, "#1");
			arr = new byte [len];
			reader.GetBytes (2,0,arr,0,len);
			for (int i=0; i<len; ++i)
				Assert.AreEqual (image[i], arr[i], "#2");

			reader.Close ();

			cmd.CommandText = "Select type_binary,type_varbinary,type_blob ";
			cmd.CommandText += "from binary_family where id=1";
		
			reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess);
			reader.Read ();
		
			len  = (int)reader.GetBytes (0,0,null,0,0);
			arr = new byte [100];
			for (int i=0; i<len; ++i) {
				Assert.AreEqual (len-i, reader.GetBytes (0, i, null, 0, 0), "#1_"+i);
				Assert.AreEqual (1, reader.GetBytes (0, i, arr, 0, 1), "#2_"+i);
				Assert.AreEqual (binary [i], arr [0], "#3_"+i);
			}
			Assert.AreEqual (0, reader.GetBytes (0, len+10, null, 0, 0));
			reader.Close ();
		}

		[Test]
		public void GetBytes_Type_DateTime ()
		{
			cmd.CommandText = "SELECT type_datetime FROM datetime_family where id = 1";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				try {
					reader.GetBytes (0, 0, null, 0, 0);
					Assert.Fail ("#A1");
				} catch (InvalidCastException ex) {
					// Invalid attempt to GetBytes on column
					// 'type_datetime'.
					// The GetBytes function can only be used
					// on columns of type Text, NText, or Image
					Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.IsTrue (ex.Message.IndexOf ("'type_datetime'") != -1, "#A5:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("GetBytes") != -1, "#A6:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("Text") != -1, "#A7:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("NText") != -1, "#A8:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("Image") != -1, "#A9:" + ex.Message);
				}
			}

			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read ());

				try {
					reader.GetBytes (0, 0, null, 0, 0);
					Assert.Fail ("#B1");
				} catch (InvalidCastException ex) {
					// Invalid attempt to GetBytes on column
					// 'type_datetime'.
					// The GetBytes function can only be used
					// on columns of type Text, NText, or Image
					Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsTrue (ex.Message.IndexOf ("'type_datetime'") != -1, "#B5:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("GetBytes") != -1, "#B6:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("Text") != -1, "#B7:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("NText") != -1, "#B8:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("Image") != -1, "#B9:" + ex.Message);
				}
			}

			cmd.CommandText = "SELECT type_datetime FROM datetime_family where id = 4";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				try {
					reader.GetBytes (0, 0, null, 0, 0);
					Assert.Fail ("#C1");
				} catch (InvalidCastException ex) {
					// Invalid attempt to GetBytes on column
					// 'type_datetime'.
					// The GetBytes function can only be used
					// on columns of type Text, NText, or Image
					Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#C2");
					Assert.IsNull (ex.InnerException, "#C3");
					Assert.IsNotNull (ex.Message, "#C4");
					Assert.IsTrue (ex.Message.IndexOf ("'type_datetime'") != -1, "#C5:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("GetBytes") != -1, "#C6:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("Text") != -1, "#C7:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("NText") != -1, "#C8:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("Image") != -1, "#C9:" + ex.Message);
				}
			}

			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read ());

				try {
					reader.GetBytes (0, 0, null, 0, 0);
					Assert.Fail ("#D1");
				} catch (InvalidCastException ex) {
					// Invalid attempt to GetBytes on column
					// 'type_datetime'.
					// The GetBytes function can only be used
					// on columns of type Text, NText, or Image
					Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#D2");
					Assert.IsNull (ex.InnerException, "#D3");
					Assert.IsNotNull (ex.Message, "#D4");
					Assert.IsTrue (ex.Message.IndexOf ("'type_datetime'") != -1, "#D5:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("GetBytes") != -1, "#D6:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("Text") != -1, "#D7:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("NText") != -1, "#D8:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("Image") != -1, "#D9:" + ex.Message);
				}
			}
		}

		[Test]
		public void GetBytes_Type_Text ()
		{
			long len;
			byte [] buffer;
			byte [] expected;

			cmd.CommandText = "SELECT type_text FROM string_family order by id asc";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				expected = new byte [] { 0x74, 0x65, 0x78,
					0x74 };

				Assert.IsTrue (reader.Read (), "#A1");
				len = reader.GetBytes (0, 0, null, 0, 0);
				Assert.AreEqual (4, len, "#A2");
				buffer = new byte [len];
				len = reader.GetBytes (0, 0, buffer, 0, (int) len);
				Assert.AreEqual (4, len, "#A3");
				Assert.AreEqual (expected, buffer, "#A4");

				expected = new byte [] { 0x00, 0x00, 0x6f, 0x6e,
					0x67, 0x00 };

				Assert.IsTrue (reader.Read (), "#B1");
				len = reader.GetBytes (0, 0, null, 0, 0);
				Assert.AreEqual (270, len, "#B2");
				buffer = new byte [6];
				len = reader.GetBytes (0, 1, buffer, 2, 3);
				Assert.AreEqual (3, len, "#B3");
				Assert.AreEqual (expected, buffer, "#B4");

				expected = new byte [0];

				Assert.IsTrue (reader.Read (), "#C1");
				len = reader.GetBytes (0, 0, null, 0, 0);
				Assert.AreEqual (0, len, "#C2");
				buffer = new byte [len];
				len = reader.GetBytes (0, 0, buffer, 0, 0);
				Assert.AreEqual (0, len, "#C3");
				Assert.AreEqual (expected, buffer, "#C4");

				Assert.IsTrue (reader.Read (), "#D1");
				try {
					reader.GetBytes (0, 0, null, 0, 0);
					Assert.Fail ("#D2");
				} catch (SqlNullValueException) {
				}
			}

			using (IDataReader reader = cmd.ExecuteReader ()) {
				expected = new byte [] { 0x74, 0x65, 0x78,
					0x74 };

				Assert.IsTrue (reader.Read (), "#E1");
				len = reader.GetBytes (0, 0, null, 0, 0);
				Assert.AreEqual (4, len, "#E2");
				buffer = new byte [len];
				len = reader.GetBytes (0, 0, buffer, 0, (int) len);
				Assert.AreEqual (4, len, "#E3");
				Assert.AreEqual (expected, buffer, "#E4");

				expected = new byte [] { 0x00, 0x00, 0x6f, 0x6e,
					0x67, 0x00 };

				Assert.IsTrue (reader.Read (), "#F1");
				len = reader.GetBytes (0, 0, null, 0, 0);
				Assert.AreEqual (270, len, "#F2");
				buffer = new byte [6];
				len = reader.GetBytes (0, 1, buffer, 2, 3);
				Assert.AreEqual (3, len, "#F3");
				Assert.AreEqual (expected, buffer, "#F4");

				expected = new byte [0];

				Assert.IsTrue (reader.Read (), "#G1");
				len = reader.GetBytes (0, 0, null, 0, 0);
				Assert.AreEqual (0, len, "#G2");
				buffer = new byte [len];
				len = reader.GetBytes (0, 0, buffer, 0, 0);
				Assert.AreEqual (0, len, "#G3");
				Assert.AreEqual (expected, buffer, "#G4");

				Assert.IsTrue (reader.Read (), "#H1");
				try {
					reader.GetBytes (0, 0, new byte [0], 0, 0);
					Assert.Fail ("#H2");
				} catch (NullReferenceException) {
				}
				try {
					reader.GetBytes (0, 0, null, 0, 0);
					Assert.Fail ("#H3");
				} catch (NullReferenceException) {
				}
			}
		}

		[Test]
		public void GetChar ()
		{
			cmd.CommandText = "SELECT type_char FROM string_family where id = 1";

			using (IDataReader reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess)) {
				Assert.IsTrue (reader.Read ());

				try {
					reader.GetChar (0);
					Assert.Fail ("#A1");
				} catch (NotSupportedException ex) {
					Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.AreEqual ((new NotSupportedException ()).Message, ex.Message, "#A5");
				}
			}

			using (IDataReader reader = cmd.ExecuteReader ()) {
				Assert.IsTrue (reader.Read ());

				try {
					reader.GetChar (0);
					Assert.Fail ("#B1");
				} catch (NotSupportedException ex) {
					Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.AreEqual ((new NotSupportedException ()).Message, ex.Message, "#B5");
				}
			}
		}

		[Test]
		public void GetChars ()
		{
			cmd.CommandText = "Select type_char, type_varchar,type_text, type_ntext ";
			cmd.CommandText += "from string_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();
			string charstring = reader.GetString (0);
			//string ncharstring = reader.GetString (1);
			string varcharstring = reader.GetString (1);
			//string nvarcharstring = reader.GetString (2);
			string textstring = reader.GetString (2);
			string ntextstring = reader.GetString (3);
			reader.Close ();
			
			reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess);
			reader.Read ();
			int len = 0;
			char[] arr; 

			len = (int) reader.GetChars (0, 0, null, 0, 0);
			Assert.AreEqual (charstring.Length, len, "#1");
			arr = new char [len];
			reader.GetChars (0, 0, arr, 0, len);
			Assert.AreEqual (0, charstring.CompareTo (new String (arr)), "#2");

			len = (int)reader.GetChars (1,0,null,0,0);
			Assert.AreEqual (varcharstring.Length, len, "#3");
			arr = new char [len];
			reader.GetChars (1, 0,arr,0,len);
			Assert.AreEqual (0, varcharstring.CompareTo (new String (arr)), "#4");

			len = (int)reader.GetChars (2,0,null,0,0);
			Assert.AreEqual (textstring.Length, len, "#5");
			arr = new char [len];
			reader.GetChars (2,0,arr,0,len);
			Assert.AreEqual (0, textstring.CompareTo (new String (arr)), "#6");

			len = (int)reader.GetChars (3,0,null,0,0);
			Assert.AreEqual (ntextstring.Length, len, "#7");
			arr = new char [len];
			reader.GetChars (3,0,arr,0,len);
			Assert.AreEqual (0, ntextstring.CompareTo (new String (arr)), "#8");

			reader.Close ();

			reader = cmd.ExecuteReader (CommandBehavior.SequentialAccess);
			reader.Read ();
			
			len  = (int)reader.GetChars (0,0,null,0,0);
			arr = new char [10];
			for (int i = 0; i < len; ++i) {
				Assert.AreEqual (len, reader.GetChars (0, i, null, 0, 0), "#9_" + i);
				Assert.AreEqual (1, reader.GetChars (0, i, arr, 0, 1), "#10_" + i);
				Assert.AreEqual (charstring [i], arr [0], "#11_" + i);
			}
			Assert.AreEqual (10, reader.GetChars (0, len + 10, null, 0, 0));

			reader.Close ();
		}

		[Test]
		public void GetStringTest ()
		{
			cmd.CommandText = "Select type_varchar,10,convert(varchar,null)";
			cmd.CommandText += "from string_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("String");

			// Test if data is returned correctly
			Assert.AreEqual (stringRow["type_varchar"], reader.GetString(0),
				"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlString");

			// Test if data is returned correctly
			Assert.AreEqual (stringRow["type_varchar"], reader.GetSqlString(0).Value,
				"#4 DataValidation Failed");
			reader.Close();
		}

		[Test]
		public void GetSqlBinaryTest ()
		{
			cmd.CommandText = "Select type_binary ,10 ,convert(binary,null)";
			cmd.CommandText += "from binary_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 	
			GetMethodTests ("SqlBinary");

			// Test if data is returned correctly
			Assert.AreEqual (binaryRow["type_binary"], reader.GetSqlBinary(0).Value,
				"#2 DataValidation Failed");
			reader.Close ();
		}

		[Test]
		public void GetGuidTest ()
		{
			cmd.CommandText = "Select type_guid,id,convert(uniqueidentifier,null)";
			cmd.CommandText += "from string_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();

			// Test for standard exceptions 
			GetMethodTests("Guid");

			// Test if data is returned correctly
			Assert.AreEqual (stringRow["type_guid"], reader.GetGuid(0),
				"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlGuid");

			// Test if data is returned correctly
			Assert.AreEqual (stringRow["type_guid"], reader.GetSqlGuid(0).Value,
				"#4 DataValidation Failed");
			reader.Close ();
		}

		[Test]
		public void GetDateTimeTest ()
		{
			cmd.CommandText = "Select type_datetime,10,convert(datetime,null)";
			cmd.CommandText += "from datetime_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();

			// Test for standard exceptions 
			GetMethodTests("DateTime");

			// Test if data is returned correctly
			Assert.AreEqual (datetimeRow["type_datetime"], reader.GetDateTime(0),
				"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlDateTime");

			// Test if data is returned correctly
			Assert.AreEqual (datetimeRow["type_datetime"], reader.GetSqlDateTime(0).Value,
				"#2 DataValidation Failed");
			reader.Close ();
		}

		[Test]
		[Ignore ("Not Supported by msdotnet")]
		public void GetCharTest ()
		{
			cmd.CommandText = "Select type_char,type_guid,convert(char,null)"; 
			cmd.CommandText += "from string_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions
			GetMethodTests ("Char");
			reader.Close ();
		}

		[Test]
		public void GetValueTest ()
		{
			cmd.CommandText = "Select id, null from numeric_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();

			object obj = null; 
			obj = reader.GetValue (0);
			Assert.AreEqual ((byte)1, obj, "#1 Shud return the value of id");
			obj = reader.GetValue (1);
			Assert.AreEqual (DBNull.Value, obj, "#2 shud return DBNull");
			reader.Close ();
		}

		[Test]
		public void GetValuesTest ()
		{
			cmd.CommandText = "Select 10,20,30 from numeric_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();
			object[] arr = null;
			int count = 0; 

			arr = new object[1];
			count = reader.GetValues (arr);
			Assert.AreEqual (10, (int)arr[0], "#1 Only first object shud be copied");
			Assert.AreEqual (1, count, "#1 return value shud equal objects copied");

			arr = new object[3];
			count = reader.GetValues (arr);
			Assert.AreEqual (3, count, "#2 return value shud equal objects copied");

			arr = new object [5];
			count = reader.GetValues (arr);
			Assert.AreEqual (3, count, "#3 return value shud equal objects copied");
			Assert.IsNull (arr[3], "#4 Only 3 objects shud be copied");

			reader.Close ();
		}

		[Test]
		public void GetValues_Values_Null ()
		{
			cmd.CommandText = "SELECT type_blob FROM binary_family where id = 1";

			using (IDataReader rdr = cmd.ExecuteReader ()) {
				Assert.IsTrue (rdr.Read ());

				try {
					rdr.GetValues ((object []) null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("values", ex.ParamName, "#5");
				}
			}
		}

		[Test]
		public void GetSqlValue ()
		{
			cmd.CommandText = "Select id, type_tinyint, null from numeric_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();

			Assert.AreEqual ((byte) 255, ((SqlByte) reader.GetSqlValue (1)).Value, "#1");
			//Assert.AreEqual (DBNull.Value, reader.GetSqlValue(2), "#2");

			reader.Close ();
		}

		[Test]
		public void GetSqlValue_Index_Invalid ()
		{
			cmd.CommandText = "Select id, type_tinyint, null from numeric_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();

			try {
				reader.GetSqlValue (-1);
				Assert.Fail ("#A1");
			} catch (IndexOutOfRangeException ex) {
				// Index was outside the bounds of the array
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				reader.GetSqlValue (3);
				Assert.Fail ("#B1");
			} catch (IndexOutOfRangeException ex) {
				// Index was outside the bounds of the array
				Assert.AreEqual (typeof (IndexOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void GetSqlValue_Reader_Closed ()
		{
			SqlCommand cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT * FROM employee";
			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				rdr.Read ();
				rdr.Close ();
				try {
					rdr.GetSqlValue (-1);
					Assert.Fail ("#1");
				} catch (InvalidOperationException ex) {
					// Invalid attempt to call MetaData
					// when reader is closed
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			}
		}

		[Test]
		public void GetSqlValue_Reader_NoData ()
		{
			SqlCommand cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT * FROM employee where id = 6666";
			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				try {
					rdr.GetSqlValue (-1);
					Assert.Fail ("#A1");
				} catch (InvalidOperationException ex) {
					// Invalid attempt to read when no data
					// is present
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				Assert.IsFalse (rdr.Read (), "#B");

				try {
					rdr.GetSqlValue (-1);
					Assert.Fail ("#C1");
				} catch (InvalidOperationException ex) {
					// Invalid attempt to read when no data
					// is present
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
					Assert.IsNull (ex.InnerException, "#C3");
					Assert.IsNotNull (ex.Message, "#C4");
				}
			}
		}

		[Test]
		public void GetSqlValues ()
		{
			cmd.CommandText = "Select 10,20,30 from numeric_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();
			object[] arr = null;
			int count = 0; 

			arr = new object[1];
			count = reader.GetSqlValues (arr);
			// Something is wrong with types ... gotta figure it out 
			//Assert.AreEqual (10, arr[0], "#1 Only first object shud be copied");
			Assert.AreEqual (1, count, "#1 return value shud equal objects copied");

			arr = new object[3];
			count = reader.GetSqlValues (arr);
			Assert.AreEqual (3, count, "#2 return value shud equal objects copied");

			arr = new object[5];
			count = reader.GetSqlValues (arr);
			Assert.AreEqual (3, count, "#3 return value shud equal objects copied");
			Assert.IsNull (arr[3], "#4 Only 3 objects shud be copied");

			reader.Close ();
		}

		[Test]
		public void GetSqlValues_Reader_Closed ()
		{
			SqlCommand cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT * FROM employee";
			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				rdr.Read ();
				rdr.Close ();
				try {
					rdr.GetSqlValues (null);
					Assert.Fail ("#1");
				} catch (InvalidOperationException ex) {
					// Invalid attempt to call MetaData
					// when reader is closed
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			}
		}

		[Test]
		public void GetSqlValues_Reader_NoData ()
		{
			SqlCommand cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT * FROM employee where id = 6666";
			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				try {
					rdr.GetSqlValues (null);
					Assert.Fail ("#A1");
				} catch (InvalidOperationException ex) {
					// Invalid attempt to read when no data
					// is present
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}

				Assert.IsFalse (rdr.Read (), "#B");

				try {
					rdr.GetSqlValues (null);
					Assert.Fail ("#C1");
				} catch (InvalidOperationException ex) {
					// Invalid attempt to read when no data
					// is present
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
					Assert.IsNull (ex.InnerException, "#C3");
					Assert.IsNotNull (ex.Message, "#C4");
				}
			}
		}

		[Test]
		public void GetSqlValues_Values_Null ()
		{
			SqlCommand cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT * FROM employee";
			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				rdr.Read ();
				try {
					rdr.GetSqlValues (null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("values", ex.ParamName, "#5");
				}
			}
		}

		[Test]
		public void HasRows ()
		{
			SqlCommand cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT id FROM employee WHERE id in (1, 2) ORDER BY id ASC";
			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				Assert.IsTrue (rdr.HasRows, "#A1");
				try {
					rdr.GetValue (0);
					Assert.Fail ("#A2");
				} catch (InvalidOperationException) {
				}
				Assert.IsTrue (rdr.HasRows, "#A3");
				Assert.IsTrue (rdr.Read (), "#A4");
				Assert.AreEqual (1, rdr.GetValue (0), "#A5");
				Assert.IsTrue (rdr.HasRows, "#A6");
				Assert.AreEqual (1, rdr.GetValue (0), "#A7");
				Assert.IsTrue (rdr.Read (), "#A8");
				Assert.AreEqual (2, rdr.GetValue (0), "#A9");
				Assert.IsTrue (rdr.HasRows, "#A10");
				Assert.IsFalse (rdr.Read (), "#A11");
				Assert.IsTrue (rdr.HasRows, "#A12");
				Assert.IsFalse (rdr.NextResult (), "#A13");
				Assert.IsFalse (rdr.HasRows, "#A14");
			}

			cmd.CommandText = "SELECT id FROM employee WHERE id = 666";
			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				Assert.IsFalse (rdr.HasRows, "#B1");
				Assert.IsFalse (rdr.Read (), "#B2");
			}

			cmd.CommandText = "SELECT id FROM employee WHERE id = 666; SELECT 3";
			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				Assert.IsFalse (rdr.HasRows, "#C1");
				Assert.IsFalse (rdr.Read (), "#C2");
				Assert.IsFalse (rdr.HasRows, "#C3");
				Assert.IsTrue (rdr.NextResult (), "#C4");
				Assert.IsTrue (rdr.HasRows, "#C5");
				try {
					rdr.GetValue (0);
					Assert.Fail ("#C6");
				} catch (InvalidOperationException) {
				}
				Assert.IsTrue (rdr.Read (), "#C7");
				Assert.AreEqual (3, rdr.GetValue (0), "#C8");
				Assert.IsTrue (rdr.HasRows, "#C9");
				Assert.AreEqual (3, rdr.GetValue (0), "#C10");
				Assert.IsFalse (rdr.Read (), "#C11");
				Assert.IsTrue (rdr.HasRows, "#C12");
				try {
					rdr.GetValue (0);
					Assert.Fail ("#C13");
				} catch (InvalidOperationException) {
				}
				Assert.IsFalse (rdr.NextResult (), "#C14");
				Assert.IsFalse (rdr.HasRows, "#C15");
				Assert.IsFalse (rdr.Read (), "#C16");
				Assert.IsFalse (rdr.HasRows, "#C17");
			}

			cmd.CommandText = "SELECT id FROM employee WHERE id = 1; SELECT 3";
			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.SingleResult)) {
				Assert.IsTrue (rdr.HasRows, "#D1");
				Assert.IsTrue (rdr.Read (), "#D2");
				Assert.IsTrue (rdr.HasRows, "#D3");
				Assert.IsFalse (rdr.NextResult (), "#D4");
				Assert.IsTrue (rdr.HasRows, "#D5");
				Assert.IsFalse (rdr.Read (), "#D6");
				Assert.IsTrue(rdr.HasRows, "#D7");
			}

			cmd.CommandText = "SELECT id FROM employee WHERE id = 666; SELECT 3";
			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.SingleResult)) {
				Assert.IsFalse (rdr.HasRows, "#E1");
				Assert.IsFalse (rdr.Read (), "#E2");
				Assert.IsFalse (rdr.HasRows, "#E3");
				Assert.IsFalse (rdr.NextResult (), "#E4");
				Assert.IsTrue (rdr.HasRows, "#E5");
				Assert.IsFalse (rdr.Read (), "#E6");
				Assert.IsTrue (rdr.HasRows, "#E7");
			}

			cmd.CommandText = "SELECT id FROM employee WHERE id = 1; SELECT 3";
			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.SchemaOnly)) {
				Assert.IsFalse (rdr.HasRows, "#F1");
				try {
					rdr.GetValue (0);
					Assert.Fail ("#F2");
				} catch (InvalidOperationException) {
				}
				Assert.IsFalse (rdr.Read (), "#F3");
				try {
					rdr.GetValue (0);
					Assert.Fail ("#F4");
				} catch (InvalidOperationException) {
				}
				Assert.IsFalse (rdr.HasRows, "#F5");
				try {
					rdr.GetValue (0);
					Assert.Fail ("#F6");
				} catch (InvalidOperationException) {
				}
				Assert.IsTrue (rdr.NextResult (), "#F7");
				try {
					rdr.GetValue (0);
					Assert.Fail ("#F8");
				} catch (InvalidOperationException) {
				}
				Assert.IsFalse (rdr.HasRows, "#F9");
				Assert.IsFalse (rdr.Read (), "#F10");
				Assert.IsFalse (rdr.HasRows, "#F11");
				Assert.IsFalse (rdr.NextResult (), "#F12");
				Assert.IsFalse (rdr.HasRows, "#F13");
				Assert.IsFalse (rdr.Read (), "#F14");
				Assert.IsFalse (rdr.HasRows, "#F15");
			}

			cmd.CommandText = "SELECT id FROM employee WHERE id = 666; SELECT 3";
			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.SchemaOnly)) {
				Assert.IsFalse (rdr.HasRows, "#G1");
				Assert.IsFalse (rdr.Read (), "#G2");
				Assert.IsFalse (rdr.HasRows, "#G3");
				Assert.IsTrue (rdr.NextResult (), "#G4");
				Assert.IsFalse (rdr.HasRows, "#G5");
				Assert.IsFalse (rdr.Read (), "#G6");
				Assert.IsFalse (rdr.HasRows, "#G7");
				Assert.IsFalse (rdr.NextResult (), "#G8");
				Assert.IsFalse (rdr.HasRows, "#G9");
				Assert.IsFalse (rdr.Read (), "#G10");
				Assert.IsFalse (rdr.HasRows, "#G11");
			}
		}

		[Test]
		public void HasRows_Reader_Closed ()
		{
			SqlCommand cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT id FROM employee WHERE id in (1, 2) ORDER BY id ASC";
			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				rdr.Close ();
				try {
					bool hasRows = rdr.HasRows;
					Assert.Fail ("#A1:" + hasRows);
				} catch (InvalidOperationException ex) {
					// Invalid attempt to call MetaData
					// when reader is closed
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}
			}

			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				Assert.IsTrue (rdr.Read (), "#B1");
				rdr.Close ();
				try {
					bool hasRows = rdr.HasRows;
					Assert.Fail ("#B2:" + hasRows);
				} catch (InvalidOperationException ex) {
					// Invalid attempt to call MetaData
					// when reader is closed
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B3");
					Assert.IsNull (ex.InnerException, "#B4");
					Assert.IsNotNull (ex.Message, "#B5");
				}
			}
		}

		[Test]
		public void isDBNullTest ()
		{
			cmd.CommandText = "select id , null from numeric_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();

			Assert.IsFalse (reader.IsDBNull (0), "#1");
			Assert.IsTrue (reader.IsDBNull (1) , "#2");

			try {
				reader.IsDBNull (10);
				Assert.Fail ("#1 Invalid Argument");
			} catch (IndexOutOfRangeException e) {
				Assert.AreEqual (typeof(IndexOutOfRangeException), e.GetType(),
					"#1 Incorrect Exception : " + e); 
			}
		}

		[Test]
		public void ReadTest ()
		{
			cmd.CommandText = "select id, type_bit from numeric_family where id=1" ;
			reader = cmd.ExecuteReader ();
			Assert.IsTrue (reader.Read () , "#1");
			Assert.IsFalse (reader.Read (), "#2");
			reader.Close ();

			try {
				reader.Read ();
				Assert.Fail ("#3 Exception shud be thrown : Reader is closed");
			} catch (InvalidOperationException e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType (),
					"#4 Incorrect Exception : " + e);
			}
		}

		[Test]
		public void NextResult ()
		{
			cmd.CommandText = "Select id from numeric_family where id=1";
			reader = cmd.ExecuteReader ();
			Assert.IsFalse (reader.NextResult (), "#1");
			reader.Close ();

			cmd.CommandText = "select id from numeric_family where id=1;";
			cmd.CommandText += "select type_bit from numeric_family where id=2;";
			reader = cmd.ExecuteReader ();
			Assert.IsTrue (reader.NextResult (), "#B1");
			Assert.IsTrue (reader.Read (), "#B2");
			Assert.IsFalse (reader.NextResult (), "#B3");
			try {
				reader.GetValue (0);
				Assert.Fail ("#B3");
			} catch (InvalidOperationException) {
			}
			Assert.IsFalse (reader.Read (), "#B4");
			try {
				reader.GetValue (0);
				Assert.Fail ("#B5");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void NextResult_Reader_Close ()
		{
			SqlCommand cmd = conn.CreateCommand ();
			cmd.CommandText = "SELECT * FROM employee";
			using (SqlDataReader rdr = cmd.ExecuteReader ()) {
				rdr.Read ();
				rdr.Close ();
				try {
					rdr.NextResult ();
					Assert.Fail ("#1");
				} catch (InvalidOperationException ex) {
					// Invalid attempt to NextResult when
					// reader is closed
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			}
		}

		[Test]
		public void GetNameTest ()
		{
			cmd.CommandText = "Select id,10 as gen,20 from numeric_family where id=1";
			reader = cmd.ExecuteReader ();

			Assert.AreEqual ("id" , reader.GetName(0) , "#1");
			Assert.AreEqual ("gen" , reader.GetName(1) , "#2");

			try {
				reader.GetName (3);
				Assert.Fail ("#4 Exception shud be thrown");
			} catch (IndexOutOfRangeException e) {
				Assert.AreEqual (typeof(IndexOutOfRangeException), e.GetType(),
					"#5 Incorrect Exception : " + e);
			}
		}

		[Test]
		public void GetOrdinalTest ()
		{
			//what is kana-width insensitive ????? 
			cmd.CommandText = "Select id,10 as gen,20 from numeric_family where id=1";
			reader = cmd.ExecuteReader ();

			Assert.AreEqual (0, reader.GetOrdinal ("id"), "#1");
			Assert.AreEqual (0, reader.GetOrdinal ("ID"), "#2");
			Assert.AreEqual (1, reader.GetOrdinal ("gen"), "#3");
			// Would expect column1,columnn2 etc for unnamed columns,
			// but msdotnet return empty string for unnamed columns
			Assert.AreEqual (2, reader.GetOrdinal (""), "#4");

			try {
				reader.GetOrdinal ("invalidname");
			} catch (IndexOutOfRangeException e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException),
					e.GetType(), "#4 Incorrect Exception : " + e);
			}
		}

		[Test]
		public void GetSchemaTable ()
		{
			var conn = ConnectionManager.Instance.Sql.Connection;

			IDbCommand cmd = null;
			IDataReader reader = null;
			DataTable schema;
			DataRow pkRow;

			try {
				cmd = conn.CreateCommand ();
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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#A:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#A:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#A:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#A:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#A:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#A:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#A:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#A:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#A:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#A:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#A:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#A:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#A:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#A:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#A:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#A:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#A:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#A:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#A:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#A:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#A:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#A:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#A:BaseCatalogName_Value");
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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#B:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#B:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#B:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#B:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#B:ProviderType_Value");
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
				Assert.IsTrue (pkRow.IsNull ("IsKey"), "#B:IsKey_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["IsKey"], "#B:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#B:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#B:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#B:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#B:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#B:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#B:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#B:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#B:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#B:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#B:BaseColumnName_Value");

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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#C:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#C:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#C:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#C:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#C:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#C:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#C:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#C:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#C:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#C:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#C:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#C:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#C:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#C:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#C:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#C:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#C:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#C:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#C:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#C:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#C:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#C:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#C:BaseCatalogName_Value");
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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#D:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#D:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#D:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#D:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#D:ProviderType_Value");
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
				Assert.IsTrue (pkRow.IsNull ("IsKey"), "#D:IsKey_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["IsKey"], "#D:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#D:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#D:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#D:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#D:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#D:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#D:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#D:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#D:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#D:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#D:BaseColumnName_Value");

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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#E:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#E:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#E:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#E:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#E:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#E:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#E:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#E:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#E:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#E:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#E:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#E:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#E:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#E:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#E:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#E:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#E:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#E:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#E:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#E:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#E:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#E:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#E:BaseCatalogName_Value");
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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#F:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#F:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#F:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#F:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#F:ProviderType_Value");
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
				Assert.IsTrue (pkRow.IsNull ("IsKey"), "#F:IsKey_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["IsKey"], "#F:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#F:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#F:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#F:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#F:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#F:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#F:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#F:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#F:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#F:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#F:BaseColumnName_Value");

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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#G:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#G:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#G:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#G:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#G:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#G:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#G:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#G:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#G:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#G:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#G:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#G:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#G:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#G:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#G:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#G:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#G:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#G:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#G:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#G:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#G:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#G:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#G:BaseCatalogName_Value");
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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#H:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#H:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#H:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#H:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#H:ProviderType_Value");
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
				Assert.IsTrue (pkRow.IsNull ("IsKey"), "#H:IsKey_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["IsKey"], "#H:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#H:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#H:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#H:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#H:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#H:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#H:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#H:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#H:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#H:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#H:BaseColumnName_Value");

				cmd.CommandText = "select id, fname, id + 20 as plustwenty from employee where id = @id";
				IDbDataParameter param = cmd.CreateParameter ();
				param.ParameterName = "@id";
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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#I:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#I:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#I:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#I:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#I:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#I:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#I:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#I:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#I:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#I:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#I:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#I:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#I:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#I:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#I:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#I:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#I:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#I:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#I:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#I:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#I:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#I:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#I:BaseCatalogName_Value");
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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#J:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#J:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#J:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#J:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#J:ProviderType_Value");
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
				Assert.IsTrue (pkRow.IsNull ("IsKey"), "#J:IsKey_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["IsKey"], "#J:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#J:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#J:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#J:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#J:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#J:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#J:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#J:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#J:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#J:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#J:BaseColumnName_Value");

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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#K:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#K:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#K:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#K:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#K:ProviderType_Value");
				Assert.IsFalse (pkRow.IsNull ("IsLong"), "#K:IsLong_IsNull");
				Assert.AreEqual (false, pkRow ["IsLong"], "#K:IsLong_Value");
				Assert.IsFalse (pkRow.IsNull ("AllowDBNull"), "#K:AllowDBNull_IsNull");
				Assert.AreEqual (false, pkRow ["AllowDBNull"], "#K:AllowDBNull_Value");
				Assert.IsFalse (pkRow.IsNull ("IsReadOnly"), "#K:IsReadOnly_IsNull");
				Assert.AreEqual (false, pkRow ["IsReadOnly"], "#K:IsReadOnly_Value");
				Assert.IsFalse (pkRow.IsNull ("IsRowVersion"), "#K:IsRowVersion_IsNull");
				Assert.AreEqual (false, pkRow ["IsRowVersion"], "#K:IsRowVersion_Value");
				Assert.IsFalse (pkRow.IsNull ("IsUnique"), "#K:IsUnique_IsNull");
				Assert.AreEqual (false, pkRow ["IsUnique"], "#K:IsUnique_Value");
				Assert.IsFalse (pkRow.IsNull ("IsKey"), "#K:IsKey_IsNull");
				Assert.AreEqual (true, pkRow ["IsKey"], "#K:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#K:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#K:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#K:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#K:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#K:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#K:BaseCatalogName_Value");
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
				Assert.AreEqual (255, pkRow ["NumericScale"], "#L:NumericScale_Value");
				Assert.IsFalse (pkRow.IsNull ("DataType"), "#L:DataType_IsNull");
				Assert.AreEqual (typeof (int), pkRow ["DataType"], "#L:DataType_Value");
				Assert.IsFalse (pkRow.IsNull ("ProviderType"), "#L:ProviderType_IsNull");
				Assert.AreEqual (8, pkRow ["ProviderType"], "#L:ProviderType_Value");
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
				Assert.IsTrue (pkRow.IsNull ("IsKey"), "#L:IsKey_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["IsKey"], "#L:IsKey_Value");
				Assert.IsFalse (pkRow.IsNull ("IsAutoIncrement"), "#L:IsAutoIncrement_IsNull");
				Assert.AreEqual (false, pkRow ["IsAutoIncrement"], "#L:IsAutoIncrement_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseSchemaName"), "#L:BaseSchemaName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseSchemaName"], "#L:BaseSchemaName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseCatalogName"), "#L:BaseCatalogName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseCatalogName"], "#L:BaseCatalogName_Value");
				Assert.IsTrue (pkRow.IsNull ("BaseTableName"), "#L:BaseTableName_IsNull");
				Assert.AreEqual (DBNull.Value, pkRow ["BaseTableName"], "#L:BaseTableName_Value");
				Assert.IsFalse (pkRow.IsNull ("BaseColumnName"), "#L:BaseColumnName_IsNull");
				Assert.AreEqual ("id", pkRow ["BaseColumnName"], "#L:BaseColumnName_Value");
			} finally {
				if (cmd != null)
					cmd.Dispose ();
				if (reader != null)
					reader.Close ();
				ConnectionManager.Instance.Sql.CloseConnection ();
			}
		}

		[Test]
		public void GetSchemaTableTest ()
		{
			cmd.CommandText = "Select type_decimal1 as decimal,id,10 ";
			cmd.CommandText += "from numeric_family where id=1";
			reader = cmd.ExecuteReader (CommandBehavior.KeyInfo);
			DataTable schemaTable  = reader.GetSchemaTable ();
			DataRow row0 = schemaTable.Rows[0]; 
			DataRow row1 = schemaTable.Rows[1]; 
		
			Assert.AreEqual ("decimal", row0["ColumnName"], "#1");
			Assert.AreEqual ("", schemaTable.Rows[2]["ColumnName"], "#2");

			Assert.AreEqual (0, row0["ColumnOrdinal"], "#2");
			Assert.AreEqual (17, row0["ColumnSize"], "#3");
			if (ClientVersion == 7)
				Assert.AreEqual (28, row0["NumericPrecision"], "#4"); 
			else
				Assert.AreEqual (38, row0 ["NumericPrecision"], "#4"); 
			Assert.AreEqual (0, row0["NumericScale"], "#5");

			Assert.AreEqual (false, row0["IsUnique"], "#6"); 
			// msdotnet returns IsUnique as false for Primary key
			// even though table consists of a single Primary Key
			//Assert.AreEqual (true, row1["IsUnique"], "#7"); 
			Assert.AreEqual (false, row0["IsKey"], "#8"); 
			Assert.AreEqual (true, row1["IsKey"], "#9"); 

			//Assert.AreEqual ("servername", row0["BaseServerName"], "#10");
			//Assert.AreEqual ("monotest", row0["BaseCatalogName"], "#11");  
			Assert.AreEqual ("type_decimal1", row0["BaseColumnName"], "#12");
			//Assert.IsNull(row0["BaseSchemaName"], "#13");
			Assert.AreEqual ("numeric_family", row0["BaseTableName"], "#14");
			Assert.AreEqual (typeof (Decimal), row0["DataType"], "#15"); 
			Assert.AreEqual (true, row0["AllowDBNull"], "#16");
			Assert.AreEqual (false, row1["AllowDBNull"], "#17");
			//Assert.IsNull(row0["ProviderType"], "#18");
			Assert.AreEqual (true, row0["IsAliased"], "#19");
			Assert.AreEqual (false, row1["IsAliased"], "#20");

			Assert.AreEqual (false, row0["IsExpression"], "#21"); 
			Assert.AreEqual (false, row0["IsIdentity"], "#22"); 
			Assert.AreEqual (false, row0["IsAutoIncrement"], "#23");
			Assert.AreEqual (false, row0["IsRowVersion"], "#24"); 
			Assert.AreEqual (false, row0["IsHidden"], "#25"); 
			Assert.AreEqual (false, row0["IsLong"], "#26"); 
			Assert.AreEqual (false, row0["IsReadOnly"], "#27"); 
			Assert.AreEqual (true, schemaTable.Rows[2]["IsReadOnly"], "#27"); 

			// Test Exception is thrown when reader is closed
			reader.Close ();
			try {
				reader.GetSchemaTable ();
				Assert.Fail ("#28 Exception shud be thrown" );
			} catch (InvalidOperationException e) {
				Assert.AreEqual (typeof (InvalidOperationException), e.GetType(),
					"#29 Incorrect Exception");
			}
		}

		[Test]
		public void GetDataTypeNameTest ()
		{
			cmd.CommandText = "Select id, type_tinyint, 10,null from numeric_family where id=1";
			reader = cmd.ExecuteReader ();

			Assert.AreEqual ("tinyint", reader.GetDataTypeName(1), "#1");
			Assert.AreEqual ("int", reader.GetDataTypeName(2), "#2");
			//need check on windows 
			Assert.AreEqual ("int", reader.GetDataTypeName(3), "#3");
			try {
				reader.GetDataTypeName (10);
				Assert.Fail ("#4 Exception shud be thrown");
			} catch (IndexOutOfRangeException e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (),
					"#5 Incorrect Exception : " + e);
			}
		}

		[Test]
		public void GetFieldType_BigInt ()
		{
			if (ClientVersion <= 7)
				Assert.Ignore ("BigInt data type is not supported.");

			cmd.CommandText = "SELECT type_bigint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (long), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Binary ()
		{
			cmd.CommandText = "SELECT type_binary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Bit ()
		{
			cmd.CommandText = "SELECT type_bit FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (bool), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Char ()
		{
			cmd.CommandText = "SELECT type_char FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Date ()
		{
			// TODO
		}

		[Test]
		public void GetFieldType_DateTime ()
		{
			cmd.CommandText = "SELECT type_datetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (DateTime), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Decimal ()
		{
			cmd.CommandText = "SELECT type_decimal1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (decimal), rdr.GetFieldType (0));
			}

			cmd.CommandText = "SELECT type_decimal2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (decimal), rdr.GetFieldType (0));
			}

			cmd.CommandText = "SELECT type_numeric1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (decimal), rdr.GetFieldType (0));
			}

			cmd.CommandText = "SELECT type_numeric2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (decimal), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Float ()
		{
			cmd.CommandText = "SELECT type_double FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (double), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Image ()
		{
			cmd.CommandText = "SELECT type_tinyblob FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Int ()
		{
			cmd.CommandText = "SELECT type_int FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (int), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Money ()
		{
			cmd.CommandText = "SELECT type_money FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (decimal), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_NChar ()
		{
			cmd.CommandText = "SELECT type_nchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_NText ()
		{
			cmd.CommandText = "SELECT type_ntext FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_NVarChar ()
		{
			cmd.CommandText = "SELECT type_nvarchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Real ()
		{
			cmd.CommandText = "SELECT type_float FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (float), rdr.GetFieldType (0));
			}

		}

		[Test]
		public void GetFieldType_SmallDateTime ()
		{
			cmd.CommandText = "SELECT type_smalldatetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (DateTime), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_SmallInt ()
		{
			cmd.CommandText = "SELECT type_smallint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (short), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_SmallMoney ()
		{
			cmd.CommandText = "SELECT type_smallmoney FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (decimal), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Text ()
		{
			cmd.CommandText = "SELECT type_text FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Time ()
		{
			// TODO
		}

		[Test]
		public void GetFieldType_Timestamp ()
		{
			cmd.CommandText = "SELECT type_timestamp FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_TinyInt ()
		{
			cmd.CommandText = "SELECT type_tinyint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (byte), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_Udt ()
		{
			// TODO
		}

		[Test]
		public void GetFieldType_UniqueIdentifier ()
		{
			cmd.CommandText = "SELECT type_guid FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (Guid), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_VarBinary ()
		{
			cmd.CommandText = "SELECT type_varbinary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (byte []), rdr.GetFieldType (0));
			}
		}

		[Test]
		public void GetFieldType_VarChar ()
		{
			cmd.CommandText = "SELECT type_varchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				Assert.AreEqual (typeof (string), rdr.GetFieldType (0));
			}
		}

		// Need to populate the data from a config file
		// Will be replaced later 
		void validateData(string sqlQuery, DataTable table)
		{
			string fmt = "#TAB[{0}] ROW[{1}] COL[{2}] Data Mismatch";

			int noOfColumns = table.Columns.Count ;
			int i=0;

			cmd.CommandText = sqlQuery ;
			reader = cmd.ExecuteReader ();

			while (reader.Read ()){
				for (int j=1; j< noOfColumns ; ++j) {
					Assert.AreEqual (table.Rows[i][j], reader[j],
						String.Format (fmt, table.TableName, i+1, j));
				}
				
				i++;
			}
			reader.Close ();
		}

		[Test]
		public void NumericDataValidation ()
		{
			validateData ("select * from numeric_family order by id ASC",
				numericDataTable);
		}
		
		[Test]
		public void StringDataValidation ()
		{
			validateData ("select * from string_family order by id ASC",
				stringDataTable);
		}

		[Test]
		public void BinaryDataValidation ()
		{
			validateData ("select * from binary_family order by id ASC",
				binaryDataTable);
		}

		[Test]
		public void DatetimeDataValidation ()
		{
			validateData ("select * from datetime_family order by id ASC",
				datetimeDataTable);
		}

		string connectionString = ConnectionManager.Instance.Sql.ConnectionString;

		//FIXME : Add more test cases
		[Test]
		public void GetProviderSpecificFieldTypeTest ()
		{
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee";
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					Assert.AreEqual (6, rdr.FieldCount, "#A1");
					Assert.AreEqual(typeof(SqlInt32), rdr.GetProviderSpecificFieldType (0), "#A2");
					Assert.AreEqual(typeof(SqlString), rdr.GetProviderSpecificFieldType (1), "#A3");
					Assert.AreEqual(typeof(SqlString), rdr.GetProviderSpecificFieldType (2), "#A4");
					Assert.AreEqual(typeof(SqlDateTime), rdr.GetProviderSpecificFieldType (3), "#A5");
					Assert.AreEqual(typeof(SqlDateTime), rdr.GetProviderSpecificFieldType (4), "#A6");
					Assert.AreEqual(typeof(SqlString), rdr.GetProviderSpecificFieldType (5), "#A7");
				}
				cmd.CommandText = "SELECT * FROM numeric_family";
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					Assert.AreEqual (15, rdr.FieldCount, "#B1");
					Assert.AreEqual(typeof (SqlInt32), rdr.GetProviderSpecificFieldType(0), "#B2");
					Assert.AreEqual(typeof (SqlBoolean), rdr.GetProviderSpecificFieldType(1), "#B3");
					Assert.AreEqual(typeof (SqlByte), rdr.GetProviderSpecificFieldType(2), "#B4");
					Assert.AreEqual(typeof (SqlInt16), rdr.GetProviderSpecificFieldType(3), "#B5");
					Assert.AreEqual(typeof (SqlInt32), rdr.GetProviderSpecificFieldType(4), "#B6");
					Assert.AreEqual(typeof (SqlInt64), rdr.GetProviderSpecificFieldType(5), "#B7");
					Assert.AreEqual(typeof (SqlDecimal), rdr.GetProviderSpecificFieldType(6), "#B8");
					Assert.AreEqual(typeof (SqlDecimal), rdr.GetProviderSpecificFieldType(7), "#B9");
					Assert.AreEqual(typeof (SqlDecimal), rdr.GetProviderSpecificFieldType(8), "#B10");
					Assert.AreEqual(typeof (SqlDecimal), rdr.GetProviderSpecificFieldType(9), "#B11");
					Assert.AreEqual(typeof (SqlMoney), rdr.GetProviderSpecificFieldType(10), "#B12");
					Assert.AreEqual(typeof (SqlMoney), rdr.GetProviderSpecificFieldType(11), "#B13");
					Assert.AreEqual(typeof (SqlSingle), rdr.GetProviderSpecificFieldType(12), "#B14");
					Assert.AreEqual(typeof (SqlDouble), rdr.GetProviderSpecificFieldType(13), "#B15");
					Assert.AreEqual(typeof (SqlInt32), rdr.GetProviderSpecificFieldType(14), "#B16");
				}
			}
		}

		//FIXME : Add more test cases
		[Test]
		public void GetProviderSpecificValueTest ()
		{
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee";
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					Assert.AreEqual (6, rdr.FieldCount, "#1");
					Assert.AreEqual((SqlInt32)1,rdr.GetProviderSpecificValue(0), "#2");
					Assert.AreEqual((SqlString)"suresh",rdr.GetProviderSpecificValue(1), "#3");
					Assert.AreEqual((SqlDateTime) new DateTime (1978, 8, 22), rdr.GetProviderSpecificValue(3), "#4");
				}
			}
		}

		[Test]
		public void GetProviderSpecificValueLowerBoundaryTest ()
		{
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee";
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					try {
						object value = rdr.GetProviderSpecificValue (-1);
						Assert.Fail ("#1:" + value);
					} catch (IndexOutOfRangeException) {
						Assert.AreEqual((SqlInt32) 1, rdr.GetProviderSpecificValue (0), "#2");
						Assert.AreEqual((SqlString) "suresh", rdr.GetProviderSpecificValue(1), "#3");
						Assert.AreEqual((SqlDateTime) new DateTime (1978, 8, 22), rdr.GetProviderSpecificValue (3), "#4");
					}
				}
			}
		}

		[Test]
		public void GetProviderSpecificValueUpperBoundaryTest ()
		{
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee";
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					try {
						object value = rdr.GetProviderSpecificValue (rdr.FieldCount);
						Assert.Fail ("#1:" + value);
					} catch (IndexOutOfRangeException) {
						Assert.AreEqual((SqlInt32) 1, rdr.GetProviderSpecificValue (0), "#2");
						Assert.AreEqual((SqlString) "suresh", rdr.GetProviderSpecificValue (1), "#3");
						Assert.AreEqual((SqlDateTime) new DateTime (1978, 8, 22), rdr.GetProviderSpecificValue (3), "#4");
					}
				}
			}
		}

		//FIXME : Add more test cases
		[Test]
		public void GetProviderSpecificValuesTest ()
		{
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee";
				Object [] psValues = new Object[6];
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					int count = rdr.GetProviderSpecificValues (psValues);
					Assert.AreEqual (6, count, "#1");
					Assert.AreEqual ((SqlInt32) 1, psValues [0], "#2");
					Assert.AreEqual ((SqlString) "suresh", psValues [1], "#3");
					Assert.AreEqual ((SqlString) "kumar", psValues [2], "#4");
					Assert.AreEqual ((SqlDateTime) new DateTime (1978, 8, 22), psValues [3], "#5");
					Assert.AreEqual ((SqlDateTime) new DateTime (2001, 03, 12), psValues [4], "#6");
					Assert.AreEqual ((SqlString) "suresh@gmail.com", psValues [5], "#7");
				}
			}
		}

		[Test]
		public void GetProviderSpecificValues_Values_Null ()
		{
			cmd.CommandText = "SELECT * FROM employee";
			reader = cmd.ExecuteReader ();
			reader.Read ();

			try {
				reader.GetProviderSpecificValues (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("values", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetProviderSpecificValuesSmallArrayTest ()
		{
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee";
				Object [] psValues = new Object[2];
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					int count = rdr.GetProviderSpecificValues (psValues);
					Assert.AreEqual (2, count, "#1");
					Assert.AreEqual ((SqlInt32) 1, psValues [0], "#2");
					Assert.AreEqual ((SqlString) "suresh", psValues [1], "#3");
				}
			}
		}

		[Test]
		public void GetProviderSpecificValuesLargeArrayTest ()
		{
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee";
				Object [] psValues = new Object[10];
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					int count = rdr.GetProviderSpecificValues (psValues);
					Assert.AreEqual (6, count, "#1");
					Assert.AreEqual ((SqlInt32) 1, psValues [0], "#2");
					Assert.AreEqual ((SqlString) "suresh", psValues [1], "#3");
					Assert.AreEqual ((SqlString) "kumar", psValues [2], "#4");
					Assert.AreEqual ((SqlDateTime) new DateTime (1978, 8, 22), psValues [3], "#5");
					Assert.AreEqual ((SqlDateTime) new DateTime (2001, 03, 12), psValues [4], "#6");
					Assert.AreEqual ((SqlString) "suresh@gmail.com", psValues [5], "#7");
					Assert.IsNull (psValues [6], "#8");
					Assert.IsNull (psValues [7], "#9");
					Assert.IsNull (psValues [8], "#10");
					Assert.IsNull (psValues [9], "#11");
				}
			}
		}

		[Test]
		public void GetSqlBytesTest ()
		{
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM binary_family where id=1";
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					Assert.AreEqual (8, rdr.FieldCount);
					
					SqlBytes sb = rdr.GetSqlBytes (1);
					Assert.AreEqual (8, sb.Length, "#A1");
					Assert.AreEqual (53, sb [0], "#A2");
					Assert.AreEqual (0, sb [1], "#A3");
					Assert.AreEqual (0, sb [2], "#A4");
					Assert.AreEqual (0, sb [3], "#A5");
					Assert.AreEqual (0, sb [4], "#A6");
					Assert.AreEqual (0, sb [5], "#A7");
					Assert.AreEqual (0, sb [6], "#A8");
					Assert.AreEqual (0, sb [7], "#A9");

					sb = rdr.GetSqlBytes (2);
					Assert.AreEqual (typeof (SqlBinary), rdr.GetSqlValue (2).GetType (), "B1#");
					Assert.AreEqual (33, sb.Length, "#B2");
					Assert.AreEqual (48, sb [0], "#B3");
					Assert.AreEqual (49, sb [1], "#B4");
					Assert.AreEqual (50, sb [2], "#B5");
					Assert.AreEqual (53, sb [15], "#B6");
					Assert.AreEqual (57, sb [29], "#B7");
				}
			}
			
		}


		static void AssertSchemaTableStructure (DataTable schemaTable, string prefix)
		{
			object [] [] columns = {
				new object [] { "ColumnName", typeof (string) },
				new object [] { "ColumnOrdinal", typeof (int) },
				new object [] { "ColumnSize", typeof (int) },
				new object [] { "NumericPrecision", typeof (short) },
				new object [] { "NumericScale", typeof (short) },
				new object [] { "IsUnique", typeof (bool) },
				new object [] { "IsKey", typeof (bool) },
				new object [] { "BaseServerName", typeof (string) },
				new object [] { "BaseCatalogName", typeof (string) },
				new object [] { "BaseColumnName", typeof (string) },
				new object [] { "BaseSchemaName", typeof (string) },
				new object [] { "BaseTableName", typeof (string) },
				new object [] { "DataType", typeof (Type) },
				new object [] { "AllowDBNull", typeof (bool) },
				new object [] { "ProviderType", typeof (int) },
				new object [] { "IsAliased", typeof (bool) },
				new object [] { "IsExpression", typeof (bool) },
				new object [] { "IsIdentity", typeof (bool) },
				new object [] { "IsAutoIncrement", typeof (bool) },
				new object [] { "IsRowVersion", typeof (bool) },
				new object [] { "IsHidden", typeof (bool) },
				new object [] { "IsLong", typeof (bool) },
				new object [] { "IsReadOnly", typeof (bool) },
				new object [] { "ProviderSpecificDataType", typeof (Type) },
				new object [] { "DataTypeName", typeof (string) },
				new object [] { "XmlSchemaCollectionDatabase", typeof (string) },
				new object [] { "XmlSchemaCollectionOwningSchema", typeof (string) },
				new object [] { "XmlSchemaCollectionName", typeof (string) },
				new object [] { "UdtAssemblyQualifiedName", typeof (string) },
				new object [] { "NonVersionedProviderType", typeof (int) },
				new object [] { "IsColumnSet", typeof (bool) }
				};

			Assert.AreEqual (columns.Length, schemaTable.Columns.Count, prefix);

			for (int i = 0; i < columns.Length; i++) {
				DataColumn col = schemaTable.Columns [i];
				Assert.IsTrue (col.AllowDBNull, prefix + "AllowDBNull (" + i + ")");
				Assert.AreEqual (columns [i] [0], col.ColumnName, prefix + "ColumnName (" + i + ")");
				Assert.AreEqual (columns [i] [1], col.DataType, prefix + "DataType (" + i + ")");
			}
		}

		int ClientVersion {
			get {
				return (engine.ClientVersion);
			}
		}
	}

	[TestFixture]
	[Category ("sqlserver")]
	[Category("NotWorking")]
	public class SqlDataReaderSchemaTest
	{
		SqlConnection conn;
		SqlCommand cmd;
		EngineConfig engine;

		[SetUp]
		public void SetUp ()
		{
			conn = ConnectionManager.Instance.Sql.Connection;
			cmd = conn.CreateCommand ();
			engine = ConnectionManager.Instance.Sql.EngineConfig;
		}

		[TearDown]
		public void TearDown ()
		{
			if (cmd != null)
				cmd.Dispose ();
			ConnectionManager.Instance.Close ();
		}

		[Test]
		public void ColumnSize_BigInt ()
		{
			if (ClientVersion <= 7)
				Assert.Ignore ("BigInt data type is not supported.");

			cmd.CommandText = "SELECT type_bigint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				// we only support TDS 7.0, which returns bigint data values as decimal(19,0)
				if (ClientVersion > 7)
					Assert.AreEqual (8, row ["ColumnSize"], "Value");
				else
					Assert.AreEqual (17, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Binary ()
		{
			cmd.CommandText = "SELECT type_binary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (8, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Bit ()
		{
			cmd.CommandText = "SELECT type_bit FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (1, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Char ()
		{
			cmd.CommandText = "SELECT type_char FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (10, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Date ()
		{
			// TODO
		}

		[Test]
		public void ColumnSize_DateTime ()
		{
			cmd.CommandText = "SELECT type_datetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (8, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Decimal ()
		{
			cmd.CommandText = "SELECT type_decimal1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "#A:IsNull");
				Assert.AreEqual (17, row ["ColumnSize"], "#A:Value");
			}

			cmd.CommandText = "SELECT type_decimal2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "#B:IsNull");
				Assert.AreEqual (17, row ["ColumnSize"], "#B:Value");
			}

			cmd.CommandText = "SELECT type_numeric1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "#C:IsNull");
				Assert.AreEqual (17, row ["ColumnSize"], "#C:Value");
			}

			cmd.CommandText = "SELECT type_numeric2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "#D:IsNull");
				Assert.AreEqual (17, row ["ColumnSize"], "#D:Value");
			}
		}

		[Test]
		public void ColumnSize_Float ()
		{
			cmd.CommandText = "SELECT type_double FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (8, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Image ()
		{
			cmd.CommandText = "SELECT type_tinyblob FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (int.MaxValue, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Int ()
		{
			cmd.CommandText = "SELECT type_int FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (4, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Money ()
		{
			cmd.CommandText = "SELECT type_money FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (8, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_NChar ()
		{
			cmd.CommandText = "SELECT type_nchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (10, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_NText ()
		{
			cmd.CommandText = "SELECT type_ntext FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (1073741823, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_NVarChar ()
		{
			cmd.CommandText = "SELECT type_nvarchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (10, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Real ()
		{
			cmd.CommandText = "SELECT type_float FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (4, row ["ColumnSize"], "Value");
			}

		}

		[Test]
		public void ColumnSize_SmallDateTime ()
		{
			cmd.CommandText = "SELECT type_smalldatetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (4, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_SmallInt ()
		{
			cmd.CommandText = "SELECT type_smallint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (2, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_SmallMoney ()
		{
			cmd.CommandText = "SELECT type_smallmoney FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (4, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Text ()
		{
			cmd.CommandText = "SELECT type_text FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (2147483647, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Time ()
		{
			// TODO
		}

		[Test]
		public void ColumnSize_Timestamp ()
		{
			cmd.CommandText = "SELECT type_timestamp FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (8, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_TinyInt ()
		{
			cmd.CommandText = "SELECT type_tinyint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (1, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_Udt ()
		{
			// TODO
		}

		[Test]
		public void ColumnSize_UniqueIdentifier ()
		{
			cmd.CommandText = "SELECT type_guid FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (16, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_VarBinary ()
		{
			cmd.CommandText = "SELECT type_varbinary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (255, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void ColumnSize_VarChar ()
		{
			cmd.CommandText = "SELECT type_varchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ColumnSize"), "IsNull");
				Assert.AreEqual (10, row ["ColumnSize"], "Value");
			}
		}

		[Test]
		public void DataType_BigInt ()
		{
			if (ClientVersion <= 7)
				Assert.Ignore ("BigInt data type is not supported.");

			cmd.CommandText = "SELECT type_bigint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (long), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Binary ()
		{
			cmd.CommandText = "SELECT type_binary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (byte []), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Bit ()
		{
			cmd.CommandText = "SELECT type_bit FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (bool), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Char ()
		{
			cmd.CommandText = "SELECT type_char FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (string), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Date ()
		{
			// TODO
		}

		[Test]
		public void DataType_DateTime ()
		{
			cmd.CommandText = "SELECT type_datetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (DateTime), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Decimal ()
		{
			cmd.CommandText = "SELECT type_decimal1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "#A:IsNull");
				Assert.AreEqual (typeof (decimal), row ["DataType"], "#A:Value");
			}

			cmd.CommandText = "SELECT type_decimal2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "#B:IsNull");
				Assert.AreEqual (typeof (decimal), row ["DataType"], "#B:Value");
			}

			cmd.CommandText = "SELECT type_numeric1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "#C:IsNull");
				Assert.AreEqual (typeof (decimal), row ["DataType"], "#C:Value");
			}

			cmd.CommandText = "SELECT type_numeric2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "#D:IsNull");
				Assert.AreEqual (typeof (decimal), row ["DataType"], "#D:Value");
			}
		}

		[Test]
		public void DataType_Float ()
		{
			cmd.CommandText = "SELECT type_double FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (double), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Image ()
		{
			cmd.CommandText = "SELECT type_tinyblob FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (byte []), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Int ()
		{
			cmd.CommandText = "SELECT type_int FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (int), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Money ()
		{
			cmd.CommandText = "SELECT type_money FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (decimal), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_NChar ()
		{
			cmd.CommandText = "SELECT type_nchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (string), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_NText ()
		{
			cmd.CommandText = "SELECT type_ntext FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (string), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_NVarChar ()
		{
			cmd.CommandText = "SELECT type_nvarchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (string), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Real ()
		{
			cmd.CommandText = "SELECT type_float FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (float), row ["DataType"], "Value");
			}

		}

		[Test]
		public void DataType_SmallDateTime ()
		{
			cmd.CommandText = "SELECT type_smalldatetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (DateTime), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_SmallInt ()
		{
			cmd.CommandText = "SELECT type_smallint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (short), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_SmallMoney ()
		{
			cmd.CommandText = "SELECT type_smallmoney FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (decimal), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Text ()
		{
			cmd.CommandText = "SELECT type_text FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (string), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Time ()
		{
			// TODO
		}

		[Test]
		public void DataType_Timestamp ()
		{
			cmd.CommandText = "SELECT type_timestamp FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (byte []), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_TinyInt ()
		{
			cmd.CommandText = "SELECT type_tinyint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (byte), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_Udt ()
		{
			// TODO
		}

		[Test]
		public void DataType_UniqueIdentifier ()
		{
			cmd.CommandText = "SELECT type_guid FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (Guid), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_VarBinary ()
		{
			cmd.CommandText = "SELECT type_varbinary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (byte []), row ["DataType"], "Value");
			}
		}

		[Test]
		public void DataType_VarChar ()
		{
			cmd.CommandText = "SELECT type_varchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("DataType"), "IsNull");
				Assert.AreEqual (typeof (string), row ["DataType"], "Value");
			}
		}

		[Test]
		public void IsLong_BigInt ()
		{
			if (ClientVersion <= 7)
				Assert.Ignore ("BigInt data type is not supported.");

			cmd.CommandText = "SELECT type_bigint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Binary ()
		{
			cmd.CommandText = "SELECT type_binary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Bit ()
		{
			cmd.CommandText = "SELECT type_bit FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Char ()
		{
			cmd.CommandText = "SELECT type_char FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Date ()
		{
			// TODO
		}

		[Test]
		public void IsLong_DateTime ()
		{
			cmd.CommandText = "SELECT type_datetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Decimal ()
		{
			cmd.CommandText = "SELECT type_decimal1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "#A:IsNull");
				Assert.AreEqual (false, row ["IsLong"], "#A:Value");
			}

			cmd.CommandText = "SELECT type_decimal2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "#B:IsNull");
				Assert.AreEqual (false, row ["IsLong"], "#B:Value");
			}

			cmd.CommandText = "SELECT type_numeric1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "#C:IsNull");
				Assert.AreEqual (false, row ["IsLong"], "#C:Value");
			}

			cmd.CommandText = "SELECT type_numeric2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "#D:IsNull");
				Assert.AreEqual (false, row ["IsLong"], "#D:Value");
			}
		}

		[Test]
		public void IsLong_Float ()
		{
			cmd.CommandText = "SELECT type_double FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Image ()
		{
			cmd.CommandText = "SELECT type_tinyblob FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (true, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Int ()
		{
			cmd.CommandText = "SELECT type_int FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Money ()
		{
			cmd.CommandText = "SELECT type_money FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_NChar ()
		{
			cmd.CommandText = "SELECT type_nchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_NText ()
		{
			cmd.CommandText = "SELECT type_ntext FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (true, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_NVarChar ()
		{
			cmd.CommandText = "SELECT type_nvarchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Real ()
		{
			cmd.CommandText = "SELECT type_float FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}

		}

		[Test]
		public void IsLong_SmallDateTime ()
		{
			cmd.CommandText = "SELECT type_smalldatetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_SmallInt ()
		{
			cmd.CommandText = "SELECT type_smallint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_SmallMoney ()
		{
			cmd.CommandText = "SELECT type_smallmoney FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Text ()
		{
			cmd.CommandText = "SELECT type_text FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (true, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Time ()
		{
			// TODO
		}

		[Test]
		public void IsLong_Timestamp ()
		{
			cmd.CommandText = "SELECT type_timestamp FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_TinyInt ()
		{
			cmd.CommandText = "SELECT type_tinyint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_Udt ()
		{
			// TODO
		}

		[Test]
		public void IsLong_UniqueIdentifier ()
		{
			cmd.CommandText = "SELECT type_guid FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_VarBinary ()
		{
			cmd.CommandText = "SELECT type_varbinary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void IsLong_VarChar ()
		{
			cmd.CommandText = "SELECT type_varchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("IsLong"), "IsNull");
				Assert.AreEqual (false, row ["IsLong"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_BigInt ()
		{
			if (ClientVersion <= 7)
				Assert.Ignore ("BigInt data type is not supported.");

			cmd.CommandText = "SELECT type_bigint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (19, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Binary ()
		{
			cmd.CommandText = "SELECT type_binary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Bit ()
		{
			cmd.CommandText = "SELECT type_bit FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Char ()
		{
			cmd.CommandText = "SELECT type_char FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Date ()
		{
			// TODO
		}

		[Test]
		public void NumericPrecision_DateTime ()
		{
			cmd.CommandText = "SELECT type_datetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (23, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Decimal ()
		{
			cmd.CommandText = "SELECT type_decimal1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "#A:IsNull");
				if (ClientVersion == 7)
					Assert.AreEqual (28, row ["NumericPrecision"], "#A:Value");
				else
					Assert.AreEqual (38, row ["NumericPrecision"], "#A:Value");
			}

			cmd.CommandText = "SELECT type_decimal2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "#B:IsNull");
				Assert.AreEqual (10, row ["NumericPrecision"], "#B:Value");
			}

			cmd.CommandText = "SELECT type_numeric1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "#C:IsNull");
				if (ClientVersion == 7)
					Assert.AreEqual (28, row ["NumericPrecision"], "#C:Value");
				else
					Assert.AreEqual (38, row ["NumericPrecision"], "#C:Value");
			}

			cmd.CommandText = "SELECT type_numeric2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "#D:IsNull");
				Assert.AreEqual (10, row ["NumericPrecision"], "#D:Value");
			}
		}

		[Test]
		public void NumericPrecision_Float ()
		{
			cmd.CommandText = "SELECT type_double FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (15, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Image ()
		{
			cmd.CommandText = "SELECT type_tinyblob FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Int ()
		{
			cmd.CommandText = "SELECT type_int FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (10, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Money ()
		{
			cmd.CommandText = "SELECT type_money FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (19, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_NChar ()
		{
			cmd.CommandText = "SELECT type_nchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_NText ()
		{
			cmd.CommandText = "SELECT type_ntext FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_NVarChar ()
		{
			cmd.CommandText = "SELECT type_nvarchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Real ()
		{
			cmd.CommandText = "SELECT type_float FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (7, row ["NumericPrecision"], "Value");
			}

		}

		[Test]
		public void NumericPrecision_SmallDateTime ()
		{
			cmd.CommandText = "SELECT type_smalldatetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (16, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_SmallInt ()
		{
			cmd.CommandText = "SELECT type_smallint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (5, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_SmallMoney ()
		{
			cmd.CommandText = "SELECT type_smallmoney FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (10, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Text ()
		{
			cmd.CommandText = "SELECT type_text FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Time ()
		{
			// TODO
		}

		[Test]
		public void NumericPrecision_Timestamp ()
		{
			cmd.CommandText = "SELECT type_timestamp FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_TinyInt ()
		{
			cmd.CommandText = "SELECT type_tinyint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (3, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Udt ()
		{
			// TODO
		}

		[Test]
		public void NumericPrecision_UniqueIdentifier ()
		{
			cmd.CommandText = "SELECT type_guid FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_VarBinary ()
		{
			cmd.CommandText = "SELECT type_varbinary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_VarChar ()
		{
			cmd.CommandText = "SELECT type_varchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericPrecision"), "IsNull");
				Assert.AreEqual (255, row ["NumericPrecision"], "Value");
			}
		}

		[Test]
		public void NumericPrecision_Variant ()
		{
			// TODO
		}

		[Test]
		public void NumericPrecision_Xml ()
		{
			// TODO
		}

		[Test]
		public void NumericScale_BigInt ()
		{
			if (ClientVersion <= 7)
				Assert.Ignore ("BigInt data type is not supported.");

			cmd.CommandText = "SELECT type_bigint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Binary ()
		{
			cmd.CommandText = "SELECT type_binary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Bit ()
		{
			cmd.CommandText = "SELECT type_bit FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Char ()
		{
			cmd.CommandText = "SELECT type_char FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Date ()
		{
			// TODO
		}

		[Test]
		public void NumericScale_DateTime ()
		{
			cmd.CommandText = "SELECT type_datetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (3, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Decimal ()
		{
			cmd.CommandText = "SELECT type_decimal1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "#A:IsNull");
				Assert.AreEqual (0, row ["NumericScale"], "#A:Value");
			}

			cmd.CommandText = "SELECT type_decimal2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "#B:IsNull");
				Assert.AreEqual (3, row ["NumericScale"], "#B:Value");
			}

			cmd.CommandText = "SELECT type_numeric1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "#C:IsNull");
				Assert.AreEqual (0, row ["NumericScale"], "#C:Value");
			}

			cmd.CommandText = "SELECT type_numeric2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "#D:IsNull");
				Assert.AreEqual (3, row ["NumericScale"], "#D:Value");
			}
		}

		[Test]
		public void NumericScale_Float ()
		{
			cmd.CommandText = "SELECT type_double FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Image ()
		{
			cmd.CommandText = "SELECT type_tinyblob FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Int ()
		{
			cmd.CommandText = "SELECT type_int FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Money ()
		{
			cmd.CommandText = "SELECT type_money FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_NChar ()
		{
			cmd.CommandText = "SELECT type_nchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_NText ()
		{
			cmd.CommandText = "SELECT type_ntext FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_NVarChar ()
		{
			cmd.CommandText = "SELECT type_nvarchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Real ()
		{
			cmd.CommandText = "SELECT type_float FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}

		}

		[Test]
		public void NumericScale_SmallDateTime ()
		{
			cmd.CommandText = "SELECT type_smalldatetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (0, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_SmallInt ()
		{
			cmd.CommandText = "SELECT type_smallint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_SmallMoney ()
		{
			cmd.CommandText = "SELECT type_smallmoney FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Text ()
		{
			cmd.CommandText = "SELECT type_text FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Time ()
		{
			// TODO
		}

		[Test]
		public void NumericScale_Timestamp ()
		{
			cmd.CommandText = "SELECT type_timestamp FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_TinyInt ()
		{
			cmd.CommandText = "SELECT type_tinyint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Udt ()
		{
			// TODO
		}

		[Test]
		public void NumericScale_UniqueIdentifier ()
		{
			cmd.CommandText = "SELECT type_guid FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_VarBinary ()
		{
			cmd.CommandText = "SELECT type_varbinary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_VarChar ()
		{
			cmd.CommandText = "SELECT type_varchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("NumericScale"), "IsNull");
				Assert.AreEqual (255, row ["NumericScale"], "Value");
			}
		}

		[Test]
		public void NumericScale_Variant ()
		{
			// TODO
		}

		[Test]
		public void NumericScale_Xml ()
		{
			// TODO
		}

		[Test]
		public void ProviderType_BigInt ()
		{
			if (ClientVersion <= 7)
				Assert.Ignore ("BigInt data type is not supported.");

			cmd.CommandText = "SELECT type_bigint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (0, row ["ProviderType"], "Value");
			} 
		}

		[Test]
		public void ProviderType_Binary ()
		{
			cmd.CommandText = "SELECT type_binary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (1, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Bit ()
		{
			cmd.CommandText = "SELECT type_bit FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (2, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Char ()
		{
			cmd.CommandText = "SELECT type_char FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (3, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Date ()
		{
			// TODO
		}

		[Test]
		public void ProviderType_DateTime ()
		{
			cmd.CommandText = "SELECT type_datetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (4, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Decimal ()
		{
			cmd.CommandText = "SELECT type_decimal1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "#A:IsNull");
				Assert.AreEqual (5, row ["ProviderType"], "#A:Value");
			}

			cmd.CommandText = "SELECT type_decimal2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "#B:IsNull");
				Assert.AreEqual (5, row ["ProviderType"], "#B:Value");
			}

			cmd.CommandText = "SELECT type_numeric1 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "#C:IsNull");
				Assert.AreEqual (5, row ["ProviderType"], "#C:Value");
			}

			cmd.CommandText = "SELECT type_numeric2 FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "#D:IsNull");
				Assert.AreEqual (5, row ["ProviderType"], "#D:Value");
			}
		}

		[Test]
		public void ProviderType_Float ()
		{
			cmd.CommandText = "SELECT type_double FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (6, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Image ()
		{
			cmd.CommandText = "SELECT type_tinyblob FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (7, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Int ()
		{
			cmd.CommandText = "SELECT type_int FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (8, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Money ()
		{
			cmd.CommandText = "SELECT type_money FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (9, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_NChar ()
		{
			cmd.CommandText = "SELECT type_nchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (10, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_NText ()
		{
			cmd.CommandText = "SELECT type_ntext FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (11, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_NVarChar ()
		{
			cmd.CommandText = "SELECT type_nvarchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (12, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Real ()
		{
			cmd.CommandText = "SELECT type_float FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (13, row ["ProviderType"], "Value");
			}

		}

		[Test]
		public void ProviderType_SmallDateTime ()
		{
			cmd.CommandText = "SELECT type_smalldatetime FROM datetime_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (15, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_SmallInt ()
		{
			cmd.CommandText = "SELECT type_smallint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (16, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_SmallMoney ()
		{
			cmd.CommandText = "SELECT type_smallmoney FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (17, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Text ()
		{
			cmd.CommandText = "SELECT type_text FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (18, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Time ()
		{
			// TODO
		}

		[Test]
		public void ProviderType_Timestamp ()
		{
			cmd.CommandText = "SELECT type_timestamp FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				// we currently consider timestamp as binary (due to TDS 7.0?)
				Assert.AreEqual (19, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_TinyInt ()
		{
			cmd.CommandText = "SELECT type_tinyint FROM numeric_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (20, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Udt ()
		{
			// TODO
		}

		[Test]
		public void ProviderType_UniqueIdentifier ()
		{
			cmd.CommandText = "SELECT type_guid FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (14, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_VarBinary ()
		{
			cmd.CommandText = "SELECT type_varbinary FROM binary_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (21, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_VarChar ()
		{
			cmd.CommandText = "SELECT type_varchar FROM string_family WHERE id = 1";

			using (SqlDataReader rdr = cmd.ExecuteReader (CommandBehavior.KeyInfo)) {
				DataTable schemaTable = rdr.GetSchemaTable ();
				DataRow row = schemaTable.Rows [0];
				Assert.IsFalse (row.IsNull ("ProviderType"), "IsNull");
				Assert.AreEqual (22, row ["ProviderType"], "Value");
			}
		}

		[Test]
		public void ProviderType_Variant ()
		{
			// TODO
		}

		[Test]
		public void ProviderType_Xml ()
		{
			// TODO
		}

		private int ClientVersion {
			get {
				return (engine.ClientVersion);
			}
		}
	}
}
