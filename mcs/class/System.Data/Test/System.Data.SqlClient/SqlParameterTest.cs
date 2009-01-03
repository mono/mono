//
// SqlParameterTest.cs - NUnit Test Cases for testing the
//                          SqlParameter class
// Author:
//	Senganal T (tsenganal@novell.com)
//	Amit Biswas (amit@amitbiswas.com)
//	Gert Driesen (drieseng@users.sourceforge.net)
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
	public class SqlParameterTest
	{
#if NET_2_0
		[Test] // .ctor (String, SqlDbType, Int32, ParameterDirection, Byte, Byte, String, DataRowVersion, Boolean, Object, String, String, String)
		public void Constructor7 ()
		{
			SqlParameter p1 = new SqlParameter ("p1Name", SqlDbType.VarChar, 20,
							    ParameterDirection.InputOutput, 0, 0,
							    "srcCol", DataRowVersion.Original, false,
							    "foo", "database", "schema", "name");
			Assert.AreEqual (DbType.AnsiString, p1.DbType, "DbType");
			Assert.AreEqual (ParameterDirection.InputOutput, p1.Direction, "Direction");
			Assert.AreEqual (false, p1.IsNullable, "IsNullable");
			//Assert.AreEqual (999, p1.LocaleId, "#");
			Assert.AreEqual ("p1Name", p1.ParameterName, "ParameterName");
			Assert.AreEqual (0, p1.Precision, "Precision");
			Assert.AreEqual (0, p1.Scale, "Scale");
			Assert.AreEqual (20, p1.Size, "Size");
			Assert.AreEqual ("srcCol", p1.SourceColumn, "SourceColumn");
			Assert.AreEqual (false, p1.SourceColumnNullMapping, "SourceColumnNullMapping");
			Assert.AreEqual (DataRowVersion.Original, p1.SourceVersion, "SourceVersion");
			Assert.AreEqual (SqlDbType.VarChar, p1.SqlDbType, "SqlDbType");
			//Assert.AreEqual (3210, p1.SqlValue, "#");
			Assert.AreEqual ("foo", p1.Value, "Value");
			Assert.AreEqual ("database", p1.XmlSchemaCollectionDatabase, "XmlSchemaCollectionDatabase");
			Assert.AreEqual ("name", p1.XmlSchemaCollectionName, "XmlSchemaCollectionName");
			Assert.AreEqual ("schema", p1.XmlSchemaCollectionOwningSchema, "XmlSchemaCollectionOwningSchema");
		}

		[Test]
		public void CompareInfo ()
		{
			SqlParameter parameter = new SqlParameter ();
			Assert.AreEqual (SqlCompareOptions.None, parameter.CompareInfo, "#1");
			parameter.CompareInfo = SqlCompareOptions.IgnoreNonSpace;
			Assert.AreEqual (SqlCompareOptions.IgnoreNonSpace, parameter.CompareInfo, "#2");
		}
#endif

		[Test]
		public void InferType_Byte ()
		{
			Byte value = 0x0a;

			SqlParameter param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.TinyInt, param.SqlDbType, "#1");
			Assert.AreEqual (DbType.Byte, param.DbType, "#2");
		}

		[Test]
		public void InferType_ByteArray ()
		{
			Byte [] value = new Byte [] { 0x0a, 0x0d };

			SqlParameter param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.VarBinary, param.SqlDbType, "#1");
			Assert.AreEqual (DbType.Binary, param.DbType, "#2");
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void InferType_Char ()
		{
			Char value = Char.MaxValue;

			SqlParameter param = new SqlParameter ();
#if NET_2_0
			param.Value = value;
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#1");
			Assert.AreEqual (DbType.String, param.DbType, "#2");
#else
			try {
				param.Value = value;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The parameter data type of Char is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
#endif
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void InferType_CharArray ()
		{
			Char [] value = new Char [] { 'A', 'X' };

			SqlParameter param = new SqlParameter ();
#if NET_2_0
			param.Value = value;
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#1");
			Assert.AreEqual (DbType.String, param.DbType, "#2");
#else
			try {
				param.Value = value;
				Assert.Fail ("#1");
			} catch (FormatException) {
				// appears to be bug in .NET 1.1 while constructing
				// exception message
			} catch (ArgumentException ex) {
				// The parameter data type of Char[] is invalid
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
#endif
		}

		[Test]
		public void InferType_DateTime ()
		{
			DateTime value;
			SqlParameter param;

			value = DateTime.Now;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.DateTime, param.SqlDbType, "#A1");
			Assert.AreEqual (DbType.DateTime, param.DbType, "#A2");

			value = DateTime.Now;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.DateTime, param.SqlDbType, "#B1");
			Assert.AreEqual (DbType.DateTime, param.DbType, "#B2");

			value = new DateTime (1973, 8, 13);
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.DateTime, param.SqlDbType, "#C1");
			Assert.AreEqual (DbType.DateTime, param.DbType, "#C2");
		}

		[Test]
		public void InferType_Decimal ()
		{
			Decimal value;
			SqlParameter param;
			
			value = Decimal.MaxValue;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Decimal, param.SqlDbType, "#A1");
			Assert.AreEqual (DbType.Decimal, param.DbType, "#A2");

			value = Decimal.MinValue;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Decimal, param.SqlDbType, "#B1");
			Assert.AreEqual (DbType.Decimal, param.DbType, "#B2");

			value = 214748.364m;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Decimal, param.SqlDbType, "#C1");
			Assert.AreEqual (DbType.Decimal, param.DbType, "#C2");
		}

		[Test]
		public void InferType_Double ()
		{
			Double value;
			SqlParameter param;
			
			value = Double.MaxValue;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Float, param.SqlDbType, "#A1");
			Assert.AreEqual (DbType.Double, param.DbType, "#A2");

			value = Double.MinValue;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Float, param.SqlDbType, "#B1");
			Assert.AreEqual (DbType.Double, param.DbType, "#B2");

			value = 0d;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Float, param.SqlDbType, "#C1");
			Assert.AreEqual (DbType.Double, param.DbType, "#C2");
		}

		[Test]
		public void InferType_Enum ()
		{
			SqlParameter param;

			param = new SqlParameter ();
			param.Value = ByteEnum.A;
			Assert.AreEqual (SqlDbType.TinyInt, param.SqlDbType, "#A1");
			Assert.AreEqual (DbType.Byte, param.DbType, "#A2");

			param = new SqlParameter ();
			param.Value = Int64Enum.A;
			Assert.AreEqual (SqlDbType.BigInt, param.SqlDbType, "#B1");
			Assert.AreEqual (DbType.Int64, param.DbType, "#B2");
		}

		[Test]
		public void InferType_Guid ()
		{
			Guid value = Guid.NewGuid ();

			SqlParameter param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.UniqueIdentifier, param.SqlDbType, "#1");
			Assert.AreEqual (DbType.Guid, param.DbType, "#2");
		}

		[Test]
		public void InferType_Int16 ()
		{
			Int16 value;
			SqlParameter param;
			
			value = Int16.MaxValue;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.SmallInt, param.SqlDbType, "#A1");
			Assert.AreEqual (DbType.Int16, param.DbType, "#A2");

			value = Int16.MinValue;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.SmallInt, param.SqlDbType, "#B1");
			Assert.AreEqual (DbType.Int16, param.DbType, "#B2");

			value = (Int16) 0;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.SmallInt, param.SqlDbType, "#C1");
			Assert.AreEqual (DbType.Int16, param.DbType, "#C2");
		}

		[Test]
		public void InferType_Int32 ()
		{
			Int32 value;
			SqlParameter param;
			
			value = Int32.MaxValue;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, "#A1");
			Assert.AreEqual (DbType.Int32, param.DbType, "#A2");

			value = Int32.MinValue;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, "#B1");
			Assert.AreEqual (DbType.Int32, param.DbType, "#B2");

			value = 0;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, "#C1");
			Assert.AreEqual (DbType.Int32, param.DbType, "#C2");
		}

		[Test]
		public void InferType_Int64 ()
		{
			Int64 value;
			SqlParameter param;
			
			value = Int64.MaxValue;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.BigInt, param.SqlDbType, "#A1");
			Assert.AreEqual (DbType.Int64, param.DbType, "#A2");

			value = Int64.MinValue;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.BigInt, param.SqlDbType, "#B1");
			Assert.AreEqual (DbType.Int64, param.DbType, "#B2");

			value = 0L;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.BigInt, param.SqlDbType, "#C1");
			Assert.AreEqual (DbType.Int64, param.DbType, "#C2");
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void InferType_Invalid ()
		{
			object [] notsupported = new object [] {
				UInt16.MaxValue,
				UInt32.MaxValue,
				UInt64.MaxValue,
				SByte.MaxValue,
				new SqlParameter ()
			};

			SqlParameter param = new SqlParameter ();

			for (int i = 0; i < notsupported.Length; i++) {
#if NET_2_0
				param.Value = notsupported [i];
				try {
					SqlDbType type = param.SqlDbType;
					Assert.Fail ("#A1:" + i + " (" + type + ")");
				} catch (ArgumentException ex) {
					// The parameter data type of ... is invalid
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.IsNull (ex.ParamName, "#A5");
				}

				try {
					DbType type = param.DbType;
					Assert.Fail ("#B1:" + i + " (" + type + ")");
				} catch (ArgumentException ex) {
					// The parameter data type of ... is invalid
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
					Assert.IsNull (ex.ParamName, "#B5");
				}
#else
				try {
					param.Value = notsupported [i];
					Assert.Fail ("#A1:" + i);
				} catch (FormatException) {
					// appears to be bug in .NET 1.1 while
					// constructing exception message
				} catch (ArgumentException ex) {
					// The parameter data type of ... is invalid
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
					Assert.IsNull (ex.ParamName, "#A5");
				}
#endif
			}
		}

		[Test]
		public void InferType_Object ()
		{
			Object value = new Object ();

			SqlParameter param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Variant, param.SqlDbType, "#1");
			Assert.AreEqual (DbType.Object, param.DbType, "#2");
		}

		[Test]
		public void InferType_Single ()
		{
			Single value = Single.MaxValue;

			SqlParameter param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Real, param.SqlDbType, "#1");
			Assert.AreEqual (DbType.Single, param.DbType, "#2");
		}

		[Test]
		public void InferType_String ()
		{
			String value = "some text";

			SqlParameter param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#1");
			Assert.AreEqual (DbType.String, param.DbType, "#2");
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void InferType_TimeSpan ()
		{
			TimeSpan value = new TimeSpan (4, 6, 23);

			SqlParameter param = new SqlParameter ();
#if NET_2_0
			param.Value = value;
			Assert.AreEqual (SqlDbType.Time, param.SqlDbType, "#1");
			Assert.AreEqual (DbType.Time, param.DbType, "#2");
#else
			try {
				param.Value = value;
				Assert.Fail ("#1");
			} catch (FormatException) {
				// appears to be bug in .NET 1.1 while constructing
				// exception message
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
#endif
		}

#if NET_2_0
		[Test]
		public void LocaleId ()
		{
			SqlParameter parameter = new SqlParameter ();
			Assert.AreEqual (0, parameter.LocaleId, "#1");
			parameter.LocaleId = 15;
			Assert.AreEqual(15, parameter.LocaleId, "#2");
		}
#endif

		[Test] // bug #320196
		public void ParameterNullTest ()
		{
			SqlParameter param = new SqlParameter ("param", SqlDbType.Decimal);
			Assert.AreEqual (0, param.Scale, "#A1");
			param.Value = DBNull.Value;
			Assert.AreEqual (0, param.Scale, "#A2");

			param = new SqlParameter ("param", SqlDbType.Int);
			Assert.AreEqual (0, param.Scale, "#B1");
			param.Value = DBNull.Value;
			Assert.AreEqual (0, param.Scale, "#B2");
		}

		[Test]
#if ONLY_1_1
		[Category ("NotWorking")]
#endif
		public void ParameterType ()
		{
			// If Type is not set, then type is inferred from the value
			// assigned. The Type should be inferred everytime Value is assigned
			// If value is null or DBNull, then the current Type should be reset to NVarChar.
			SqlParameter param = new SqlParameter ();
			Assert.AreEqual (DbType.String, param.DbType, "#A1");
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#A2");
			param.Value = DBNull.Value;
			Assert.AreEqual (DbType.String, param.DbType, "#A3");
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#A4");
			param.Value = 1;
			Assert.AreEqual (DbType.Int32, param.DbType, "#A5");
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, "#A6");
			param.Value = DBNull.Value;
#if NET_2_0
			Assert.AreEqual (DbType.String, param.DbType, "#A7");
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#A8");
#else
			Assert.AreEqual (DbType.Int32, param.DbType, "#A7");
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, "#A8");
#endif
			param.Value = null;
#if NET_2_0
			Assert.AreEqual (DbType.String, param.DbType, "#A9");
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#A10");
#else
			Assert.AreEqual (DbType.Int32, param.DbType, "#A9");
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, "#A10");
#endif
			param.Value = DateTime.Now;
			Assert.AreEqual (DbType.DateTime, param.DbType, "#A11");
			Assert.AreEqual (SqlDbType.DateTime, param.SqlDbType, "#A12");
			param.Value = null;
#if NET_2_0
			Assert.AreEqual (DbType.String, param.DbType, "#A13");
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#A14");
#else
			Assert.AreEqual (DbType.DateTime, param.DbType, "#A13");
			Assert.AreEqual (SqlDbType.DateTime, param.SqlDbType, "#A14");
#endif

			// If DbType is set, then the SqlDbType should not be
			// inferred from the value assigned.
			SqlParameter param1 = new SqlParameter ();
			param1.DbType = DbType.String;
			Assert.AreEqual (SqlDbType.NVarChar, param1.SqlDbType, "#B1");
			param1.Value = 1;
			Assert.AreEqual (SqlDbType.NVarChar, param1.SqlDbType, "#B2");

			// If SqlDbType is set, then the DbType should not be
			// inferred from the value assigned.
			SqlParameter param2 = new SqlParameter ();
			param2.SqlDbType = SqlDbType.NVarChar;
			Assert.AreEqual (SqlDbType.NVarChar, param2.SqlDbType, "#C1");
			param2.Value = 1;
			Assert.AreEqual (SqlDbType.NVarChar, param2.SqlDbType, "#C2");
		}

		[Test]
		public void InferType_Boolean ()
		{
			Boolean value;
			SqlParameter param;

			value = false;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Bit, param.SqlDbType, "#A1");
			Assert.AreEqual (DbType.Boolean, param.DbType, "#A2");

			value = true;
			param = new SqlParameter ();
			param.Value = value;
			Assert.AreEqual (SqlDbType.Bit, param.SqlDbType, "#B1");
			Assert.AreEqual (DbType.Boolean, param.DbType, "#B2");
		}

#if NET_2_0
		[Test]
		public void ResetDbType ()
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
		public void ResetSqlDbType ()
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
		public void SourceColumnNullMapping ()
		{
			SqlParameter p1 = new SqlParameter ();
			Assert.IsFalse (p1.SourceColumnNullMapping, "#1");
			p1.SourceColumnNullMapping = true;
			Assert.IsTrue (p1.SourceColumnNullMapping, "#2");
			p1.SourceColumnNullMapping = false;
			Assert.IsFalse (p1.SourceColumnNullMapping, "#3");
		}
#endif

		[Test]
#if ONLY_1_1
		[Category ("NotWorking")]
#endif
		public void SqlDbTypeTest ()
		{
			SqlParameter p1 = new SqlParameter ();
			Assert.AreEqual (null, p1.Value, "#1");
			Assert.AreEqual (SqlDbType.NVarChar, p1.SqlDbType, "#2");
			Assert.AreEqual (DbType.String, p1.DbType, "#3");
			
			SqlParameter p2 = new SqlParameter ("#4 p2Name", (Object)null);
			Assert.AreEqual (null, p2.Value, "#5");
			Assert.AreEqual (SqlDbType.NVarChar, p2.SqlDbType, "#6");
			Assert.AreEqual (DbType.String, p2.DbType, "#7");
			
			p2.Value = Convert.ToInt32(42);
			Assert.AreEqual (42, p2.Value, "#8");
			Assert.AreEqual (SqlDbType.Int, p2.SqlDbType, "#9");
			Assert.AreEqual (DbType.Int32, p2.DbType, "#10");

			p2.Value = DBNull.Value;
			Assert.AreEqual (DBNull.Value, p2.Value, "#11");
#if NET_2_0
			Assert.AreEqual (SqlDbType.NVarChar, p2.SqlDbType, "#12");
			Assert.AreEqual (DbType.String, p2.DbType, "#13");
#else
			Assert.AreEqual (SqlDbType.Int, p2.SqlDbType, "#12");
			Assert.AreEqual (DbType.Int32, p2.DbType, "#13");
#endif
		}

#if NET_2_0
		[Test]
		[Category ("NotWorking")]
		public void SqlValue ()
		{
			SqlParameter parameter = new SqlParameter ();
			Assert.IsNull (parameter.SqlValue, "#A1");

			parameter.SqlValue = "Char";
			Assert.AreEqual (SqlDbType.NVarChar, parameter.SqlDbType, "#B:SqlDbType");
			Assert.IsNotNull (parameter.SqlValue, "#B:SqlValue1");
			Assert.AreEqual (typeof (SqlString), parameter.SqlValue.GetType (), "#B:SqlValue2");
			Assert.AreEqual ("Char", ((SqlString) parameter.SqlValue).Value, "#B:SqlValue3");
			Assert.AreEqual ("Char", parameter.Value, "#B:Value");

			parameter.SqlValue = 10;
			Assert.AreEqual (SqlDbType.Int, parameter.SqlDbType, "#C:SqlDbType");
			Assert.IsNotNull (parameter.SqlValue, "#C:SqlValue1");
			Assert.AreEqual (typeof (SqlInt32), parameter.SqlValue.GetType (), "#C:SqlValue2");
			Assert.AreEqual (10, ((SqlInt32) parameter.SqlValue).Value, "#C:SqlValue3");
			Assert.AreEqual (10, parameter.Value, "#C:Value");
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
#endif

		private enum ByteEnum : byte
		{
			A = 0x0a,
			B = 0x0d
		}

		private enum Int64Enum : long
		{
			A = long.MinValue,
			B = long.MaxValue
		}
	}
}
