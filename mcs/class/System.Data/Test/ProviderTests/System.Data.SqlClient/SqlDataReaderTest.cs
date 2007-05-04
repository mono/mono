//
// SqlDataReaderTest.cs - NUnit Test Cases for testing the
//                          SqlDataReader class
// Author:
//      Umadevi S (sumadevi@novell.com)
//      Kornél Pál <http://www.kornelpal.hu/>
//	Sureshkumar T (tsureshkumar@novell.com)
//	Senganal T (tsenganal@novell.com)
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
using System.Text;
using System.Data.SqlTypes;
using System.Data.Common;
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{

	[TestFixture]
	[Category ("sqlserver")]
	public class SqlDataReaderTest 
	{
		SqlConnection conn = null; 
		SqlCommand cmd = null;
		SqlDataReader reader = null; 
		String query = "Select type_{0},type_{1},convert({0},null) from numeric_family where id=1";
		DataSet sqlDataset = null; 

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
			conn = new SqlConnection (ConnectionManager.Singleton.ConnectionString);
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
		}
		[TearDown]
		public void TearDown ()
		{
			if (reader != null)
				reader.Close ();

			conn.Close ();
		}
		
		[Test]
		public void ReadEmptyNTextFieldTest () {
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
				ConnectionManager.Singleton.CloseConnection ();
			}
		}		

		[Test]
		public void ReadBingIntTest() 
		{
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
				ConnectionManager.Singleton.CloseConnection ();
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
			}catch (AssertionException e) {
				throw e;		
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(),
					"#2[Get"+s+"] Incorrect Exception : " + e);
			}
		
			// GetSql* Methods do not throw SqlNullValueException	
			// So, Testimg only for Get* Methods 
			if (!s.StartsWith("Sql")) {
				try {
					CallGetMethod (s, 2);
					Assert.Fail ("#3[Get"+s+"] Exception must be thrown");	
				}catch (AssertionException e) {
					throw e;
				}catch (Exception e){
					Assert.AreEqual (typeof(SqlNullValueException),e.GetType(),
						"#4[Get"+s+"] Incorrect Exception : " + e);
				}
			}

			try {
				CallGetMethod (s, 3);
				Assert.Fail ("#5[Get"+s+"] IndexOutOfRangeException must be thrown");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e){
				Assert.AreEqual (typeof(IndexOutOfRangeException), e.GetType(),
					"#6[Get"+s+"] Incorrect Exception : " + e);
			}
		}

		[Test]
		public void GetBooleanTest ()
		{
			cmd.CommandText = string.Format (query, "bit", "int");
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("Boolean");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_bit"], reader.GetBoolean(0),
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
			cmd.CommandText = string.Format (query, "tinyint", "int");
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
			cmd.CommandText = string.Format (query, "smallint", "int");
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
			cmd.CommandText = string.Format (query, "int", "bigint");
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
			cmd.CommandText = string.Format (query, "bigint", "int");
			reader = cmd.ExecuteReader ();
			reader.Read ();

			// Test for standard exceptions 
			GetMethodTests("Int64");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_bigint"], reader.GetInt64(0),
				"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlInt64");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_bigint"], reader.GetSqlInt64(0).Value,
				"#4 DataValidation Failed");
			reader.Close ();
		}

		[Test]
		public void GetDecimalTest ()
		{
			cmd.CommandText = string.Format (query, "decimal", "int");
			reader = cmd.ExecuteReader ();
			reader.Read ();
			// Test for standard exceptions 
			GetMethodTests("Decimal");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_decimal"], reader.GetDecimal(0),
				"#2 DataValidation Failed");

			// Test for standard exceptions 
			GetMethodTests("SqlDecimal");

			// Test if data is returned correctly
			Assert.AreEqual (numericRow["type_decimal"], reader.GetSqlDecimal(0).Value,
				"#4 DataValidation Failed");
			reader.Close ();
		}

		[Test]
		public void GetSqlMoneyTest ()
		{
			cmd.CommandText = string.Format (query, "money", "int");
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
				reader.GetBytes (0,0,null,0,0);	
				Assert.Fail ("#1 GetBytes shud be used only wth Sequential Access");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType (),
					"#2 Incorrect Exception : " + e);
			}
			reader.Close ();
			
			byte[] asciiArray = (new ASCIIEncoding ()).GetBytes ("text");
			byte[] unicodeArray = (new UnicodeEncoding ()).GetBytes ("ntext");
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

			size = reader.GetBytes (1,0,null,0,0);
			Assert.AreEqual (unicodeArray.Length, size, "#5 Data Incorrect");
			buffer = new byte[size];
			size = reader.GetBytes (1,0,buffer,0,(int)size);
			for (int i=0;i<size; i++)
				Assert.AreEqual (unicodeArray[i], buffer[i], "#6 Data Incorrect");
			
			// Test if msdotnet behavior s followed when null value 
			// is read using GetBytes 
			Assert.AreEqual (0, reader.GetBytes (2,0,null,0,0), "#7");
			reader.GetBytes (2,0,buffer,0,10);

			reader.Close ();
			// do i need to test for image/binary values also ??? 
		}

		[Test]
		public void GetBytes_Binary ()
		{
			cmd.CommandText = "Select type_binary,type_varbinary,type_blob ";
			cmd.CommandText += "from binary_family where id=1";
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

			len = (int)reader.GetChars (0,0,null,0,0);
			Assert.AreEqual (charstring.Length, len, "#1");
			arr = new char [len];
			reader.GetChars (0,0,arr,0,len);
			Assert.AreEqual (0, charstring.CompareTo (new String (arr)), "#2");

			len = (int)reader.GetChars (1,0,null,0,0);
			Assert.AreEqual (varcharstring.Length, len, "#3");
			arr = new char [len];
			reader.GetChars (1,0,arr,0,len);
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
			for (int i=0; i<len; ++i) {
				Assert.AreEqual (len-i, reader.GetChars (0, i, null, 0, 0), "#9_"+i);
				Assert.AreEqual (1, reader.GetChars (0, i, arr, 0, 1), "#10_"+i);
				Assert.AreEqual (charstring [i], arr [0], "#11_"+i);
			}
			Assert.AreEqual (0, reader.GetChars (0, len+10, null, 0, 0));

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
		public void GetSqlValueTest ()
		{
			cmd.CommandText = "Select id, type_tinyint, null from numeric_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();

			Assert.AreEqual ((byte)255, ((SqlByte) reader.GetSqlValue(1)).Value, "#1");
			//Assert.AreEqual (DBNull.Value, reader.GetSqlValue(2), "#2");

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
		public void GetSqlValuesTest ()
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
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
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
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType (),
					"#4 Incorrect Exception : " + e);
			}
		}

		[Test]
		public void NextResultTest ()
		{
			cmd.CommandText = "Select id from numeric_family where id=1";
			reader = cmd.ExecuteReader ();
			Assert.IsFalse (reader.NextResult (), "#1");
			reader.Close ();

			cmd.CommandText = "select id from numeric_family where id=1;";
			cmd.CommandText += "select type_bit from numeric_family where id=2;";
			reader = cmd.ExecuteReader ();
			Assert.IsTrue (reader.NextResult (), "#2");
			Assert.IsFalse (reader.NextResult (), "#3");
			reader.Close ();

			try {
				reader.NextResult ();
				Assert.Fail ("#4 Exception shud be thrown : Reader is closed");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType (),
					"#5 Incorrect Exception : " + e);
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
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
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
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException),
					e.GetType(), "#4 Incorrect Exception : " + e);
			}
		}

		[Test]
		public void GetSchemaTableTest ()
		{
			cmd.CommandText = "Select type_decimal as decimal,id,10 ";
			cmd.CommandText += "from numeric_family where id=1";
			reader = cmd.ExecuteReader (CommandBehavior.KeyInfo);
			DataTable schemaTable  = reader.GetSchemaTable ();
			DataRow row0 = schemaTable.Rows[0]; 
			DataRow row1 = schemaTable.Rows[1]; 
		
			Assert.AreEqual ("decimal", row0["ColumnName"], "#1");
			Assert.AreEqual ("", schemaTable.Rows[2]["ColumnName"], "#2");

			Assert.AreEqual (0, row0["ColumnOrdinal"], "#2");
			Assert.AreEqual (17, row0["ColumnSize"], "#3");
			Assert.AreEqual (38, row0["NumericPrecision"], "#4"); 
			Assert.AreEqual (0, row0["NumericScale"], "#5");

			Assert.AreEqual (false, row0["IsUnique"], "#6"); 
			// msdotnet returns IsUnique as false for Primary key
			// even though table consists of a single Primary Key
			//Assert.AreEqual (true, row1["IsUnique"], "#7"); 
			Assert.AreEqual (false, row0["IsKey"], "#8"); 
			Assert.AreEqual (true, row1["IsKey"], "#9"); 

			//Assert.AreEqual ("servername", row0["BaseServerName"], "#10");
			//Assert.AreEqual ("monotest", row0["BaseCatalogName"], "#11");  
			Assert.AreEqual ("type_decimal", row0["BaseColumnName"], "#12");
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
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(),
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
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(IndexOutOfRangeException), e.GetType(),
					"#5 Incorrect Exception : " + e);
			}
		}

		[Test]
		public void GetFieldTypeTest ()
		{
			cmd.CommandText = "Select id , type_tinyint, 10 , null from numeric_family where id=1";
			reader = cmd.ExecuteReader ();

			Assert.AreEqual ("tinyint", reader.GetDataTypeName(1), "#1");
			Assert.AreEqual ("int", reader.GetDataTypeName(2) , "#2");
			Assert.AreEqual ("int", reader.GetDataTypeName(3), "#3");
			try {
				reader.GetDataTypeName (10);
				Assert.Fail ("#4 Exception shud be thrown");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(IndexOutOfRangeException), e.GetType(),
					"#5 Incorrect Exception : " + e);
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
				for (int j=1; j< noOfColumns ; ++j)
					Assert.AreEqual (table.Rows[i][j], reader[j],
						String.Format (fmt, table.TableName, i+1, j));
				
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

#if NET_2_0
		string connectionString = ConnectionManager.Singleton.ConnectionString;

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
					Assert.AreEqual (6, rdr.FieldCount, "#1");
					Assert.AreEqual(typeof(SqlInt32),rdr.GetProviderSpecificFieldType(0),"#2 The column at index 0 must have FieldType as SqlString");
					Assert.AreEqual(typeof(SqlString),rdr.GetProviderSpecificFieldType(1),"#3 The column at index 1 must have FieldType as SqlString");
					Assert.AreEqual(typeof(SqlDateTime),rdr.GetProviderSpecificFieldType(3),"#4 The column at index 3 must have FieldType as SqlString");
				}
				cmd.CommandText = "SELECT * FROM numeric_family";
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					Assert.AreEqual (12, rdr.FieldCount, "#5");
					Assert.AreEqual(typeof(SqlBoolean),rdr.GetProviderSpecificFieldType(1),"#6 The column at index 0 must have FieldType as SqlString");
					Assert.AreEqual(typeof(SqlByte),rdr.GetProviderSpecificFieldType(2),"#7 The column at index 1 must have FieldType as SqlString");
					Assert.AreEqual(typeof(SqlDecimal),rdr.GetProviderSpecificFieldType(6),"#8 The column at index 3 must have FieldType as SqlString");
					Assert.AreEqual(typeof(SqlMoney),rdr.GetProviderSpecificFieldType(8),"#9 The column at index 3 must have FieldType as SqlString");
					Assert.AreEqual(typeof(SqlSingle),rdr.GetProviderSpecificFieldType(10),"#10 The column at index 3 must have FieldType as SqlString");
					Assert.AreEqual(typeof(SqlDouble),rdr.GetProviderSpecificFieldType(11),"#11 The column at index 3 must have FieldType as SqlString");
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
					Assert.AreEqual((SqlInt32)1,rdr.GetProviderSpecificValue(0),"#2 The column at index 0 must have FieldType as SqlString");
					Assert.AreEqual((SqlString)"suresh",rdr.GetProviderSpecificValue(1),"#3 The column at index 1 must have FieldType as SqlString");
					Assert.AreEqual((SqlDateTime)DateTime.Parse("8/22/1978"),rdr.GetProviderSpecificValue(3),"#4 The column at index 3 must have FieldType as SqlString");
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
					Assert.AreEqual (6, rdr.FieldCount, "#1");
					try {
						Assert.AreEqual((SqlInt32)1,rdr.GetProviderSpecificValue(-1),"#2 The column at index 0 must have FieldType as SqlString");
					} catch (IndexOutOfRangeException) {
						Assert.AreEqual((SqlInt32)1,rdr.GetProviderSpecificValue(0),"#2 The column at index 0 must have FieldType as SqlString");
						Assert.AreEqual((SqlString)"suresh",rdr.GetProviderSpecificValue(1),"#3 The column at index 1 must have FieldType as SqlString");
						Assert.AreEqual((SqlDateTime)DateTime.Parse("8/22/1978"),rdr.GetProviderSpecificValue(3),"#4 The column at index 3 must have FieldType as SqlString");
						return;
					}
					Assert.Fail ("Expected exception IndexOutOfRangeException was not thrown");
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
					Assert.AreEqual (6, rdr.FieldCount, "#1");
					try {
						Assert.AreEqual((SqlInt32)1,rdr.GetProviderSpecificValue(rdr.FieldCount),"#2 The column at index 0 must have FieldType as SqlString");
					} catch (IndexOutOfRangeException) {
						Assert.AreEqual((SqlInt32)1,rdr.GetProviderSpecificValue(0),"#2 The column at index 0 must have FieldType as SqlString");
						Assert.AreEqual((SqlString)"suresh",rdr.GetProviderSpecificValue(1),"#3 The column at index 1 must have FieldType as SqlString");
						Assert.AreEqual((SqlDateTime)DateTime.Parse("8/22/1978"),rdr.GetProviderSpecificValue(3),"#4 The column at index 3 must have FieldType as SqlString");
						return;
					}
					Assert.Fail ("Expected exception IndexOutOfRangeException was not thrown");
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
					rdr.GetProviderSpecificValues (psValues);
					Assert.AreEqual ((SqlInt32)1, psValues[0], "#");
					Assert.AreEqual ((SqlString)"suresh", psValues[1], "#");
					Assert.AreEqual ((SqlDateTime)DateTime.Parse("8/22/1978"), psValues[3], "#");
				}
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
					rdr.GetProviderSpecificValues (psValues);
					Assert.AreEqual ((SqlInt32)1, psValues[0], "#");
					Assert.AreEqual ((SqlString)"suresh", psValues[1], "#");
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
					rdr.GetProviderSpecificValues (psValues);
					Assert.AreEqual ((SqlInt32)1, psValues[0], "#");
					Assert.AreEqual ((SqlString)"suresh", psValues[1], "#");
					Assert.AreEqual ((SqlDateTime)DateTime.Parse("8/22/1978"), psValues[3], "#");
					Assert.AreEqual (null, psValues[6], "#");
					Assert.AreEqual (null, psValues[9], "#");
				}
			}
		}

		[Test]
		public void GetProviderSpecificValuesNullTest ()
		{
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM employee";
				Object [] psValues = new Object[2];
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					try {
						rdr.GetProviderSpecificValues (null);
					} catch (NullReferenceException) {
						Assert.AreEqual (null, psValues[0], "#");
						Assert.AreEqual (null, psValues[1], "#");
						return;
					}
					Assert.Fail ("Expected exception NullReferenceException was not thrown");
				}
			}
		}

		[Test]
		public void GetSqlBytesTest ()
		{
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT * FROM binary_family";
				using (SqlDataReader rdr = cmd.ExecuteReader ()) {
					rdr.Read();
					Assert.AreEqual (7, rdr.FieldCount, "#1");
					
					SqlBytes sb = rdr.GetSqlBytes (1);
					long byteCount = sb.Length;
					Assert.AreEqual (1, byteCount, "#");
					Assert.AreEqual (53, sb[0], "#");

					sb = rdr.GetSqlBytes (2);
					Assert.AreEqual (typeof(SqlBinary), rdr.GetSqlValue(2).GetType(), "#");
					byteCount = sb.Length;
					Assert.AreEqual (30, byteCount, "#");
					Assert.AreEqual (48, sb[0], "#");
					Assert.AreEqual (49, sb[1], "#");
					Assert.AreEqual (50, sb[2], "#");
					Assert.AreEqual (53, sb[15], "#");
					Assert.AreEqual (57, sb[29], "#");					
				}
			}
			
		}

#endif // NET_2_0

	}
}
