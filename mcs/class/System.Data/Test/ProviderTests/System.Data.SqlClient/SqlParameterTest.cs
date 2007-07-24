//
// SqlParameterTest.cs - NUnit Test Cases for testing the
//                          SqlParameter class
// Author:
//      Senganal T (tsenganal@novell.com)
//      Amit Biswas (amit@amitbiswas.com)
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

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	[Category ("sqlserver")]
	//FIXME : Add more testcases
	public class SqlParameterTest
	{
		//testcase for #77410
		[Test]
		public void ParameterNullTest ()
		{
			SqlParameter param = new SqlParameter ("param", SqlDbType.Decimal);
			Assert.AreEqual (0, param.Scale, "#1");
			param.Value = DBNull.Value;
			Assert.AreEqual (0, param.Scale, "#2");

			param = new SqlParameter ("param", SqlDbType.Int);
			Assert.AreEqual (0, param.Scale, "#3");
			param.Value = DBNull.Value;
			Assert.AreEqual (0, param.Scale, "#4");
		}

		[Test]
		public void ParameterType ()
		{
			// If Type is not set, then type is inferred from the value
			// assigned. The Type should be inferred everytime Value is assigned
			// If value is null or DBNull, then the current Type should be reset to NVarChar.
			SqlParameter param = new SqlParameter ();
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#1");
			param.Value = DBNull.Value;
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#2");
			param.Value = 1;
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, "#3");
			param.Value = DBNull.Value;
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#4");
			param.Value = null;
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#5");

			//If Type is set, then the Type should not inferred from the value 
			//assigned.
			SqlParameter param1 = new SqlParameter ();
			param1.DbType = DbType.String; 
			Assert.AreEqual (SqlDbType.NVarChar, param1.SqlDbType, "#6");
			param1.Value = 1;
			Assert.AreEqual (SqlDbType.NVarChar, param1.SqlDbType, "#7");

			SqlParameter param2 = new SqlParameter ();
			param2.SqlDbType = SqlDbType.NVarChar;
			Assert.AreEqual (SqlDbType.NVarChar, param2.SqlDbType, "#8");
			param2.Value = 1;
			Assert.AreEqual (SqlDbType.NVarChar, param2.SqlDbType, "#9");
		}

		[Test]
		public void SqlDbTypeTest ()
		{
			SqlParameter p1 = new SqlParameter ();
			Assert.AreEqual (null, p1.Value, "#1 Value of the parameter must be null by default");
			Assert.AreEqual (SqlDbType.NVarChar, p1.SqlDbType, "#2 parameters without any value or null value must have SqlDbtype as NVarChar");
			Assert.AreEqual (DbType.String, p1.DbType, "#3 parameters without any value must have DbType as NVarChar");
			
			SqlParameter p2 = new SqlParameter ("#4 p2Name", (Object)null);
			Assert.AreEqual (null, p2.Value, "#5 Value of the parameter must be null by default");
			Assert.AreEqual (SqlDbType.NVarChar, p2.SqlDbType, "#6 parameters without any value or null value must have SqlDbtype as NVarChar");
			Assert.AreEqual (DbType.String, p2.DbType, "#7parameters without any value or null value must have DbType as String");
			
			p2.Value = Convert.ToInt32(42);
			Assert.AreEqual (42, p2.Value, "#8 Value of the parameter must be 42");
			Assert.AreEqual (SqlDbType.Int, p2.SqlDbType, "#9 parameter must have SqlDbtype as Int");
			Assert.AreEqual (DbType.Int32, p2.DbType, "#10 parameter must have Dbtype as Int32");

			p2.Value = DBNull.Value;
			Assert.AreEqual (DBNull.Value, p2.Value, "#11 Value of the parameter must be DBNull.Value");
			Assert.AreEqual (SqlDbType.NVarChar, p2.SqlDbType, "#12 parameters without any value or null value must have SqlDbtype as NVarChar");
			Assert.AreEqual (DbType.String, p2.DbType, "#13 parameters without any value or null value must have DbType as String");

		}

		//testcase for #82170
		[Test]
		public void ParameterSizeTest ()
		{
			SqlConnection conn = new SqlConnection (ConnectionManager.Singleton.ConnectionString);
			conn.Open ();
			string longstring = new String('x', 20480);
			SqlCommand cmd;
			SqlParameter prm;
			cmd = new SqlCommand ("create table #text1 (ID int not null, Val1 ntext)", conn);
			cmd.ExecuteNonQuery ();
			cmd.CommandText = "INSERT INTO #text1(ID,Val1) VALUES (@ID,@Val1)";
			prm = new SqlParameter ();
			prm.ParameterName = "@ID";
			prm.Value = 1;
			cmd.Parameters.Add (prm);

			prm = new SqlParameter ();
			prm.ParameterName = "@Val1";
			prm.Value = longstring;
			prm.SqlDbType = SqlDbType.NText; // Comment and enjoy the truncation
			cmd.Parameters.Add (prm);
			cmd.ExecuteNonQuery ();
			cmd = new SqlCommand ("select datalength(Val1) from #text1", conn);
			Assert.AreEqual (20480 * 2, cmd.ExecuteScalar (), "#1");

			cmd.CommandText = "INSERT INTO #text1(ID,Val1) VALUES (@ID,@Val1)";
			prm = new SqlParameter ();
			prm.ParameterName = "@ID";
			prm.Value = 1;
			cmd.Parameters.Add (prm);

			prm = new SqlParameter ();
			prm.ParameterName = "@Val1";
			prm.Value = longstring;
			//prm.SqlDbType = SqlDbType.NText;
			cmd.Parameters.Add (prm);
			cmd.ExecuteNonQuery ();
			cmd = new SqlCommand ("select datalength(Val1) from #text1", conn);
			Assert.AreEqual (20480 * 2, cmd.ExecuteScalar (), "#2");

			cmd.CommandText = "INSERT INTO #text1(ID,Val1) VALUES (@ID,@Val1)";
			prm = new SqlParameter ();
			prm.ParameterName = "@ID";
			prm.Value = 1;
			cmd.Parameters.Add (prm);

			prm = new SqlParameter ();
			prm.ParameterName = "@Val1";
			prm.Value = longstring;
			prm.SqlDbType = SqlDbType.VarChar;
			cmd.Parameters.Add (prm);
			cmd.ExecuteNonQuery ();
			cmd = new SqlCommand ("select datalength(Val1) from #text1", conn);
			Assert.AreEqual (20480 * 2, cmd.ExecuteScalar (), "#3");
			cmd = new SqlCommand ("drop table #text1", conn);
			cmd.ExecuteNonQuery ();
			conn.Close ();
		}
#if NET_2_0
		[Test]
		public void ResetSqlDbTypeTest ()
		{
			//Parameter with an assigned value but no SqlDbType specified
			SqlParameter p1 = new SqlParameter ("foo", 42);
			Assert.AreEqual (42, p1.Value, "#1");
			Assert.AreEqual (DbType.Int32, p1.DbType, "#2");
			Assert.AreEqual (SqlDbType.Int, p1.SqlDbType, "#3");

			p1.ResetSqlDbType ();
			Assert.AreEqual (DbType.Int32, p1.DbType, "#4 The parameter with value 42 must have DbType as Int32");
			Assert.AreEqual (SqlDbType.Int, p1.SqlDbType, "#5 The parameter with value 42 must have SqlDbType as Int");

			p1.SqlDbType = SqlDbType.DateTime; //assigning a SqlDbType
			Assert.AreEqual (DbType.DateTime, p1.DbType, "#6");
			Assert.AreEqual (SqlDbType.DateTime, p1.SqlDbType, "#7");
			p1.ResetSqlDbType (); //Resetting SqlDbType
			Assert.AreEqual (DbType.Int32, p1.DbType, "#8 Resetting SqlDbType must infer the type from the value");
			Assert.AreEqual (SqlDbType.Int, p1.SqlDbType, "#9 Resetting SqlDbType must infer the type from the value");

			//Parameter with an assigned SqlDbType but no specified value
			SqlParameter p2 = new SqlParameter ("foo", SqlDbType.Int);
			Assert.AreEqual (null, p2.Value, "#10");
			Assert.AreEqual (DbType.Int32, p2.DbType, "#11");
			Assert.AreEqual (SqlDbType.Int, p2.SqlDbType, "#12");

			//Although a SqlDbType is specified, calling ResetSqlDbType resets 
			//the SqlDbType and DbType properties to default values
			p2.ResetSqlDbType ();
			Assert.AreEqual (DbType.String, p2.DbType, "#13 Resetting SqlDbType must infer the type from the value");
			Assert.AreEqual (SqlDbType.NVarChar, p2.SqlDbType, "#14 Resetting SqlDbType must infer the type from the value");

			p2.SqlDbType = SqlDbType.DateTime; //assigning a SqlDbType
			Assert.AreEqual (DbType.DateTime, p2.DbType, "#15");
			Assert.AreEqual (SqlDbType.DateTime, p2.SqlDbType, "#16");
			p2.ResetSqlDbType (); //Resetting SqlDbType
			Assert.AreEqual (DbType.String, p2.DbType, "#17 Resetting SqlDbType must infer the type from the value");
			Assert.AreEqual (SqlDbType.NVarChar, p2.SqlDbType, "#18 Resetting SqlDbType must infer the type from the value");
		}

		[Test]
		public void ResetDbTypeTest ()
		{
			//Parameter with an assigned value but no DbType specified
			SqlParameter p1 = new SqlParameter ("foo", 42);
			Assert.AreEqual (42, p1.Value, "#1");
			Assert.AreEqual (DbType.Int32, p1.DbType, "#2");
			Assert.AreEqual (SqlDbType.Int, p1.SqlDbType, "#3");

			p1.ResetDbType ();
			Assert.AreEqual (DbType.Int32, p1.DbType, "#4 The parameter with value 42 must have DbType as Int32");
			Assert.AreEqual (SqlDbType.Int, p1.SqlDbType, "#5 The parameter with value 42 must have SqlDbType as Int32");

			p1.DbType = DbType.DateTime; //assigning a DbType
			Assert.AreEqual (DbType.DateTime, p1.DbType, "#6");
			Assert.AreEqual (SqlDbType.DateTime, p1.SqlDbType, "#7");
			p1.ResetDbType (); //Resetting DbType
			Assert.AreEqual (DbType.Int32, p1.DbType, "#8 Resetting DbType must infer the type from the value");
			Assert.AreEqual (SqlDbType.Int, p1.SqlDbType, "#9 Resetting DbType must infer the type from the value");

			//Parameter with an assigned SqlDbType but no specified value
			SqlParameter p2 = new SqlParameter ("foo", SqlDbType.Int);
			Assert.AreEqual (null, p2.Value, "#10");
			Assert.AreEqual (DbType.Int32, p2.DbType, "#11");
			Assert.AreEqual (SqlDbType.Int, p2.SqlDbType, "#12");

			//Although a SqlDbType is specified, calling ResetDbType resets 
			//the SqlDbType and DbType properties to default values
			p2.ResetDbType (); 
			Assert.AreEqual (DbType.String, p2.DbType, "#13 Resetting DbType must infer the type from the value");
			Assert.AreEqual (SqlDbType.NVarChar, p2.SqlDbType, "#14 Resetting DbType must infer the type from the value");

			p2.DbType = DbType.DateTime; //assigning a SqlDbType
			Assert.AreEqual (DbType.DateTime, p2.DbType, "#15");
			Assert.AreEqual (SqlDbType.DateTime, p2.SqlDbType, "#16");
			p2.ResetDbType (); //Resetting DbType
			Assert.AreEqual (DbType.String, p2.DbType, "#17 Resetting DbType must infer the type from the value");
			Assert.AreEqual (SqlDbType.NVarChar, p2.SqlDbType, "#18 Resetting DbType must infer the type from the value");
		}

		[Test]
		public void XmlSchemaTest ()
		{
			SqlParameter p1 = new SqlParameter ();
			
			//Testing default values
			Assert.AreEqual (String.Empty, p1.XmlSchemaCollectionDatabase,
					 "#1 Default value for XmlSchemaCollectionDatabase is an empty string");
			Assert.AreEqual (String.Empty, p1.XmlSchemaCollectionName,
					 "#2 Default value for XmlSchemaCollectionName is an empty string");
			Assert.AreEqual (String.Empty, p1.XmlSchemaCollectionOwningSchema,
					 "#3 Default value for XmlSchemaCollectionOwningSchema is an empty string");

			//Changing one property should not affect the remaining two properties
			p1.XmlSchemaCollectionDatabase = "database";
			Assert.AreEqual ("database", p1.XmlSchemaCollectionDatabase,
					 "#4 Default value for XmlSchemaCollectionDatabase is an empty string");
			Assert.AreEqual (String.Empty, p1.XmlSchemaCollectionName,
					 "#5 Default value for XmlSchemaCollectionName is an empty string");
			Assert.AreEqual (String.Empty, p1.XmlSchemaCollectionOwningSchema,
					 "#6 Default value for XmlSchemaCollectionOwningSchema is an empty string");

			p1.XmlSchemaCollectionName = "name";
			Assert.AreEqual ("database", p1.XmlSchemaCollectionDatabase,
					 "#7 Default value for XmlSchemaCollectionDatabase is an empty string");
			Assert.AreEqual ("name", p1.XmlSchemaCollectionName,
					 "#8 Default value for XmlSchemaCollectionName is an empty string");
			Assert.AreEqual (String.Empty, p1.XmlSchemaCollectionOwningSchema,
					 "#9 Default value for XmlSchemaCollectionOwningSchema is an empty string");

			p1.XmlSchemaCollectionOwningSchema = "schema";
			Assert.AreEqual ("database", p1.XmlSchemaCollectionDatabase,
					 "#10 Default value for XmlSchemaCollectionDatabase is an empty string");
			Assert.AreEqual ("name", p1.XmlSchemaCollectionName,
					 "#11 Default value for XmlSchemaCollectionName is an empty string");
			Assert.AreEqual ("schema", p1.XmlSchemaCollectionOwningSchema,
					 "#12 Default value for XmlSchemaCollectionOwningSchema is an empty string");

			//assigning null value stores default empty string
			p1.XmlSchemaCollectionDatabase = null;
			Assert.AreEqual (String.Empty, p1.XmlSchemaCollectionDatabase,
					 "#13 Default value for XmlSchemaCollectionDatabase is an empty string");
			Assert.AreEqual ("name", p1.XmlSchemaCollectionName,
					 "#14 Default value for XmlSchemaCollectionName is an empty string");
			Assert.AreEqual ("schema", p1.XmlSchemaCollectionOwningSchema,
					 "#15 Default value for XmlSchemaCollectionOwningSchema is an empty string");

			p1.XmlSchemaCollectionName = "";
			Assert.AreEqual (String.Empty, p1.XmlSchemaCollectionDatabase,
					 "#16 Default value for XmlSchemaCollectionDatabase is an empty string");
			Assert.AreEqual ("", p1.XmlSchemaCollectionName,
					 "#17 Default value for XmlSchemaCollectionName is an empty string");
			Assert.AreEqual ("schema", p1.XmlSchemaCollectionOwningSchema,
					 "#18 Default value for XmlSchemaCollectionOwningSchema is an empty string");

			//values are not trimmed
			p1.XmlSchemaCollectionOwningSchema = "  a  ";
			Assert.AreEqual (String.Empty, p1.XmlSchemaCollectionDatabase,
					 "#19 Default value for XmlSchemaCollectionDatabase is an empty string");
			Assert.AreEqual ("", p1.XmlSchemaCollectionName,
					 "#20 Default value for XmlSchemaCollectionName is an empty string");
			Assert.AreEqual ("  a  ", p1.XmlSchemaCollectionOwningSchema,
					 "#21 Default value for XmlSchemaCollectionOwningSchema is an empty string");
		}

		[Test]
		public void SourceColumnNullMappingTest ()
		{
			SqlParameter p1 = new SqlParameter ();
			Assert.AreEqual (false, p1.SourceColumnNullMapping, "#1 SourceColumnNullMapping should be false by default");
			p1.SourceColumnNullMapping = true;
			Assert.AreEqual (true, p1.SourceColumnNullMapping, "#2 SourceColumnNullMapping should be false by default");
		}

		[Test]
		public void ctor7Test ()
		{
			SqlParameter p1 = new SqlParameter ("p1Name", SqlDbType.VarChar, 20,
							    ParameterDirection.InputOutput, 0, 0,
							    "srcCol", DataRowVersion.Original, false,
							    "foo", "database", "schema", "name");
			Assert.AreEqual (DbType.AnsiString, p1.DbType, "#");
			Assert.AreEqual (ParameterDirection.InputOutput, p1.Direction, "#");
			Assert.AreEqual (false, p1.IsNullable, "#");
			//Assert.AreEqual (999, p1.LocaleId, "#");
			Assert.AreEqual ("p1Name", p1.ParameterName, "#");
			Assert.AreEqual (0, p1.Precision, "#");
			Assert.AreEqual (0, p1.Scale, "#");
			Assert.AreEqual (20, p1.Size, "#");
			Assert.AreEqual ("srcCol", p1.SourceColumn, "#");
			Assert.AreEqual (false, p1.SourceColumnNullMapping, "#");
			Assert.AreEqual (DataRowVersion.Original, p1.SourceVersion, "#");
			Assert.AreEqual (SqlDbType.VarChar, p1.SqlDbType, "#");
			//Assert.AreEqual (3210, p1.SqlValue, "#");
			Assert.AreEqual ("foo", p1.Value, "#");
			Assert.AreEqual ("database", p1.XmlSchemaCollectionDatabase, "#");
			Assert.AreEqual ("name", p1.XmlSchemaCollectionName, "#");
			Assert.AreEqual ("schema", p1.XmlSchemaCollectionOwningSchema, "#");
		}

		[Test]
		public void CompareInfoTest ()
		{
			SqlParameter parameter = new SqlParameter ();
			Assert.AreEqual (SqlCompareOptions.None, parameter.CompareInfo, "#1 Default value should be System.Data.SqlTypes.SqlCompareOptions.None");

			parameter.CompareInfo = SqlCompareOptions.IgnoreNonSpace;
			Assert.AreEqual (SqlCompareOptions.IgnoreNonSpace, parameter.CompareInfo, "#2 It should return CompareOptions.IgnoreSpace after setting this value for the property");
		}
	
		[Test]
		public void LocaleIdTest ()
		{
			SqlParameter parameter = new SqlParameter ();
			Assert.AreEqual (0, parameter.LocaleId, "#1 Default value for the property should be 0");

			parameter.LocaleId = 15;
			Assert.AreEqual(15, parameter.LocaleId, "#2");
		}

		[Test]
		public void SqlValue ()
		{
			SqlParameter parameter = new SqlParameter ();
			Assert.AreEqual (null, parameter.SqlValue, "#1 Default value for the property should be Null");

			parameter.SqlValue = SqlDbType.Char.ToString ();
			Assert.AreEqual ("Char", parameter.SqlValue, "#1 The value for the property should be Char after setting SqlDbType to Char");
		}
#endif

	}
}
