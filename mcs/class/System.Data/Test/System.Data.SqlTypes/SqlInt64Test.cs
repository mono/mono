//
// SqlInt64Test.cs - NUnit Test Cases for System.Data.SqlTypes.SqlInt64
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Ville Palo
// (C) 2003 Martin Willemoes Hansen

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Xml;
using System.Data.SqlTypes;
#if TARGET_JVM
using DivideByZeroException = System.ArithmeticException;
#endif

#if NET_2_0
using System.Xml.Serialization;
using System.IO;
#endif

using NUnit.Framework;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
	public class SqlInt64Test
	{
		// Test constructor
		[Test]
		public void Create ()
		{
			SqlInt64 TestLong = new SqlInt64 (29);
			Assert.AreEqual ((long) 29, TestLong.Value, "#A01");

			TestLong = new SqlInt64 (-9000);
			Assert.AreEqual ((long) -9000, TestLong.Value, "#A02");
		}

		// Test public fields
		[Test]
		public void PublicFields ()
		{
			Assert.AreEqual ((long) 9223372036854775807, SqlInt64.MaxValue.Value, "#B01");
			Assert.AreEqual ((long) (-9223372036854775808), SqlInt64.MinValue.Value, "#B02");
			Assert.IsTrue (SqlInt64.Null.IsNull, "#B03");
			Assert.AreEqual ((long) 0, SqlInt64.Zero.Value, "#B04");
		}

		// Test properties
		[Test]
		public void Properties ()
		{
			SqlInt64 Test5443 = new SqlInt64 (5443);
			SqlInt64 Test1 = new SqlInt64 (1);

			Assert.IsTrue (SqlInt64.Null.IsNull, "#C01");
			Assert.AreEqual ((long) 5443, Test5443.Value, "#C02");
			Assert.AreEqual ((long) 1, Test1.Value, "#C03");
		}

		// PUBLIC METHODS

		[Test]
		public void ArithmeticMethods ()
		{
			SqlInt64 Test64 = new SqlInt64 (64);
			SqlInt64 Test0 = new SqlInt64 (0);
			SqlInt64 Test164 = new SqlInt64 (164);
			SqlInt64 TestMax = new SqlInt64 (SqlInt64.MaxValue.Value);

			// Add()
			Assert.AreEqual ((long) 64, SqlInt64.Add (Test64, Test0).Value, "#D01");
			Assert.AreEqual ((long) 228, SqlInt64.Add (Test64, Test164).Value, "#D02");
			Assert.AreEqual ((long) 164, SqlInt64.Add (Test0, Test164).Value, "#D03");
			Assert.AreEqual ((long) SqlInt64.MaxValue, SqlInt64.Add (TestMax, Test0).Value, "#D04");

			try {
				SqlInt64.Add (TestMax, Test64);
				Assert.Fail ("#D05");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D06");
			}

			// Divide()
			Assert.AreEqual ((long) 2, SqlInt64.Divide (Test164, Test64).Value, "#D07");
			Assert.AreEqual ((long) 0, SqlInt64.Divide (Test64, Test164).Value, "#D08");

			try {
				SqlInt64.Divide (Test64, Test0);
				Assert.Fail ("#D09");
			} catch (DivideByZeroException e) {
				Assert.AreEqual (typeof (DivideByZeroException), e.GetType (), "#D10");
			}

			// Mod()
			Assert.AreEqual ((SqlInt64) 36, SqlInt64.Mod (Test164, Test64), "#D11");
			Assert.AreEqual ((SqlInt64) 64, SqlInt64.Mod (Test64, Test164), "#D12");

			// Multiply()
			Assert.AreEqual ((long) 10496, SqlInt64.Multiply (Test64, Test164).Value, "#D13");
			Assert.AreEqual ((long) 0, SqlInt64.Multiply (Test64, Test0).Value, "#D14");

			try {
				SqlInt64.Multiply (TestMax, Test64);
				Assert.Fail ("#D15");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D16");
			}

			// Subtract()
			Assert.AreEqual ((long) 100, SqlInt64.Subtract (Test164, Test64).Value, "#D17");

			try {
				SqlInt64.Subtract (SqlInt64.MinValue, Test164);
				Assert.Fail ("#D18");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D19");
			}

#if NET_2_0
			// Modulus ()
			Assert.AreEqual ((SqlInt64)36, SqlInt64.Modulus (Test164, Test64), "#D20");
			Assert.AreEqual ((SqlInt64)64, SqlInt64.Modulus (Test64, Test164), "#D21");
#endif
		}

		[Test]
		public void BitwiseMethods ()
		{
			long MaxValue = SqlInt64.MaxValue.Value;
			SqlInt64 TestInt = new SqlInt64 (0);
			SqlInt64 TestIntMax = new SqlInt64 (MaxValue);
			SqlInt64 TestInt2 = new SqlInt64 (10922);
			SqlInt64 TestInt3 = new SqlInt64 (21845);

			// BitwiseAnd
			Assert.AreEqual ((long) 21845, SqlInt64.BitwiseAnd (TestInt3, TestIntMax).Value, "#E01");
			Assert.AreEqual ((long) 0, SqlInt64.BitwiseAnd (TestInt2, TestInt3).Value, "#E02");
			Assert.AreEqual ((long) 10922, SqlInt64.BitwiseAnd (TestInt2, TestIntMax).Value, "#E03");

			//BitwiseOr
			Assert.AreEqual ((long) 21845, SqlInt64.BitwiseOr (TestInt, TestInt3).Value, "#E04");
			Assert.AreEqual ((long) MaxValue, SqlInt64.BitwiseOr (TestIntMax, TestInt2).Value, "#E05");
		}

		[Test]
		public void CompareTo ()
		{
			SqlInt64 TestInt4000 = new SqlInt64 (4000);
			SqlInt64 TestInt4000II = new SqlInt64 (4000);
			SqlInt64 TestInt10 = new SqlInt64 (10);
			SqlInt64 TestInt10000 = new SqlInt64 (10000);
			SqlString TestString = new SqlString ("This is a test");

			Assert.IsTrue (TestInt4000.CompareTo (TestInt10) > 0, "#F01");
			Assert.IsTrue (TestInt10.CompareTo (TestInt4000) < 0, "#F02");
			Assert.IsTrue (TestInt4000II.CompareTo (TestInt4000) == 0, "#F03");
			Assert.IsTrue (TestInt4000II.CompareTo (SqlInt64.Null) > 0, "#F04");

			try {
				TestInt10.CompareTo (TestString);
				Assert.Fail ("#F05");
			} catch (ArgumentException e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "#F06");
			}
		}

		[Test]
		public void EqualsMethod ()
		{
			SqlInt64 Test0 = new SqlInt64 (0);
			SqlInt64 Test158 = new SqlInt64 (158);
			SqlInt64 Test180 = new SqlInt64 (180);
			SqlInt64 Test180II = new SqlInt64 (180);

			Assert.IsTrue (!Test0.Equals (Test158), "#G01");
			Assert.IsTrue (!Test158.Equals (Test180), "#G01");
			Assert.IsTrue (!Test180.Equals (new SqlString ("TEST")), "#G03");
			Assert.IsTrue (Test180.Equals (Test180II), "#G04");
		}

		[Test]
		public void StaticEqualsMethod ()
		{
			SqlInt64 Test34 = new SqlInt64 (34);
			SqlInt64 Test34II = new SqlInt64 (34);
			SqlInt64 Test15 = new SqlInt64 (15);

			Assert.IsTrue (SqlInt64.Equals (Test34, Test34II).Value, "#H01");
			Assert.IsTrue (!SqlInt64.Equals (Test34, Test15).Value, "#H02");
			Assert.IsTrue (!SqlInt64.Equals (Test15, Test34II).Value, "#H03");
		}

		[Test]
		public void GetHashCodeTest ()
		{
			SqlInt64 Test15 = new SqlInt64 (15);

			// FIXME: Better way to test HashCode
			Assert.AreEqual ((int) 15, Test15.GetHashCode (), "#I01");
		}

		[Test]
		public void GetTypeTest ()
		{
			SqlInt64 Test = new SqlInt64 (84);
			Assert.AreEqual ("System.Data.SqlTypes.SqlInt64", Test.GetType ().ToString (), "#J01");
		}

		[Test]
		public void Greaters ()
		{
			SqlInt64 Test10 = new SqlInt64 (10);
			SqlInt64 Test10II = new SqlInt64 (10);
			SqlInt64 Test110 = new SqlInt64 (110);

			// GreateThan ()
			Assert.IsTrue (!SqlInt64.GreaterThan (Test10, Test110).Value, "#K01");
			Assert.IsTrue (SqlInt64.GreaterThan (Test110, Test10).Value, "#K02");
			Assert.IsTrue (!SqlInt64.GreaterThan (Test10II, Test10).Value, "#K03");

			// GreaterTharOrEqual ()
			Assert.IsTrue (!SqlInt64.GreaterThanOrEqual (Test10, Test110).Value, "#K04");
			Assert.IsTrue (SqlInt64.GreaterThanOrEqual (Test110, Test10).Value, "#K05");
			Assert.IsTrue (SqlInt64.GreaterThanOrEqual (Test10II, Test10).Value, "#K06");
		}

		[Test]
		public void Lessers ()
		{
			SqlInt64 Test10 = new SqlInt64 (10);
			SqlInt64 Test10II = new SqlInt64 (10);
			SqlInt64 Test110 = new SqlInt64 (110);

			// LessThan()
			Assert.IsTrue (SqlInt64.LessThan (Test10, Test110).Value, "#L01");
			Assert.IsTrue (!SqlInt64.LessThan (Test110, Test10).Value, "#L02");
			Assert.IsTrue (!SqlInt64.LessThan (Test10II, Test10).Value, "#L03");

			// LessThanOrEqual ()
			Assert.IsTrue (SqlInt64.LessThanOrEqual (Test10, Test110).Value, "#L04");
			Assert.IsTrue (!SqlInt64.LessThanOrEqual (Test110, Test10).Value, "#L05");
			Assert.IsTrue (SqlInt64.LessThanOrEqual (Test10II, Test10).Value, "#L06");
			Assert.IsTrue (SqlInt64.LessThanOrEqual (Test10II, SqlInt64.Null).IsNull, "#L07");
		}

		[Test]
		public void NotEquals ()
		{
			SqlInt64 Test12 = new SqlInt64 (12);
			SqlInt64 Test128 = new SqlInt64 (128);
			SqlInt64 Test128II = new SqlInt64 (128);

			Assert.IsTrue (SqlInt64.NotEquals (Test12, Test128).Value, "#M01");
			Assert.IsTrue (SqlInt64.NotEquals (Test128, Test12).Value, "#M02");
			Assert.IsTrue (SqlInt64.NotEquals (Test128II, Test12).Value, "#M03");
			Assert.IsTrue (!SqlInt64.NotEquals (Test128II, Test128).Value, "#M04");
			Assert.IsTrue (!SqlInt64.NotEquals (Test128, Test128II).Value, "#M05");
			Assert.IsTrue (SqlInt64.NotEquals (SqlInt64.Null, Test128II).IsNull, "#M06");
			Assert.IsTrue (SqlInt64.NotEquals (SqlInt64.Null, Test128II).IsNull, "#M07");
		}

		[Test]
		public void OnesComplement ()
		{
			SqlInt64 Test12 = new SqlInt64 (12);
			SqlInt64 Test128 = new SqlInt64 (128);

			Assert.AreEqual ((SqlInt64) (-13), SqlInt64.OnesComplement (Test12), "#N01");
			Assert.AreEqual ((SqlInt64) (-129), SqlInt64.OnesComplement (Test128), "#N02");
		}

		[Test]
		public void Parse ()
		{
			try {
				SqlInt64.Parse (null);
				Assert.Fail ("#O01");
			} catch (ArgumentNullException e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "#O02");
			}

			try {
				SqlInt64.Parse ("not-a-number");
				Assert.Fail ("#O03");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#O04");
			}

			try {
				SqlInt64.Parse ("1000000000000000000000000000");
				Assert.Fail ("#O05");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#O06");
			}

			Assert.AreEqual ((long) 150, SqlInt64.Parse ("150").Value, "#O07");
		}

		[Test]
		public void Conversions ()
		{
			SqlInt64 Test12 = new SqlInt64 (12);
			SqlInt64 Test0 = new SqlInt64 (0);
			SqlInt64 TestNull = SqlInt64.Null;
			SqlInt64 Test1000 = new SqlInt64 (1000);
			SqlInt64 Test288 = new SqlInt64 (288);

			// ToSqlBoolean ()
			Assert.IsTrue (Test12.ToSqlBoolean ().Value, "#P01");
			Assert.IsTrue (!Test0.ToSqlBoolean ().Value, "#P02");
			Assert.IsTrue (TestNull.ToSqlBoolean ().IsNull, "#P03");

			// ToSqlByte ()
			Assert.AreEqual ((byte) 12, Test12.ToSqlByte ().Value, "#P04");
			Assert.AreEqual ((byte) 0, Test0.ToSqlByte ().Value, "#P05");

			try {
				SqlByte b = (byte) Test1000.ToSqlByte ();
				Assert.Fail ("#P06");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#P07");
			}

			// ToSqlDecimal ()
			Assert.AreEqual ((decimal) 12, Test12.ToSqlDecimal ().Value, "#P08");
			Assert.AreEqual ((decimal) 0, Test0.ToSqlDecimal ().Value, "#P09");
			Assert.AreEqual ((decimal) 288, Test288.ToSqlDecimal ().Value, "#P10");

			// ToSqlDouble ()
			Assert.AreEqual ((double) 12, Test12.ToSqlDouble ().Value, "#P11");
			Assert.AreEqual ((double) 0, Test0.ToSqlDouble ().Value, "#P12");
			Assert.AreEqual ((double) 1000, Test1000.ToSqlDouble ().Value, "#P13");

			// ToSqlInt32 ()
			Assert.AreEqual ((int) 12, Test12.ToSqlInt32 ().Value, "#P14");
			Assert.AreEqual ((int) 0, Test0.ToSqlInt32 ().Value, "#P15");
			Assert.AreEqual ((int) 288, Test288.ToSqlInt32 ().Value, "#P16");

			// ToSqlInt16 ()
			Assert.AreEqual ((short) 12, Test12.ToSqlInt16 ().Value, "#P17");
			Assert.AreEqual ((short) 0, Test0.ToSqlInt16 ().Value, "#P18");
			Assert.AreEqual ((short) 288, Test288.ToSqlInt16 ().Value, "#P19");

			// ToSqlMoney ()
			Assert.AreEqual (12.0000M, Test12.ToSqlMoney ().Value, "#P20");
			Assert.AreEqual ((decimal) 0, Test0.ToSqlMoney ().Value, "#P21");
			Assert.AreEqual (288.0000M, Test288.ToSqlMoney ().Value, "#P22");

			// ToSqlSingle ()
			Assert.AreEqual ((float) 12, Test12.ToSqlSingle ().Value, "#P23");
			Assert.AreEqual ((float) 0, Test0.ToSqlSingle ().Value, "#P24");
			Assert.AreEqual ((float) 288, Test288.ToSqlSingle ().Value, "#P25");

			// ToSqlString ()
			Assert.AreEqual ("12", Test12.ToSqlString ().Value, "#P26");
			Assert.AreEqual ("0", Test0.ToSqlString ().Value, "#P27");
			Assert.AreEqual ("288", Test288.ToSqlString ().Value, "#P28");

			// ToString ()
			Assert.AreEqual ("12", Test12.ToString (), "#P29");
			Assert.AreEqual ("0", Test0.ToString (), "#P30");
			Assert.AreEqual ("288", Test288.ToString (), "#P31");
		}

		[Test]
		public void Xor ()
		{
			SqlInt64 Test14 = new SqlInt64 (14);
			SqlInt64 Test58 = new SqlInt64 (58);
			SqlInt64 Test130 = new SqlInt64 (130);
			SqlInt64 TestMax = new SqlInt64 (SqlInt64.MaxValue.Value);
			SqlInt64 Test0 = new SqlInt64 (0);

			Assert.AreEqual ((long) 52, SqlInt64.Xor (Test14, Test58).Value, "#Q01");
			Assert.AreEqual ((long) 140, SqlInt64.Xor (Test14, Test130).Value, "#Q02");
			Assert.AreEqual ((long) 184, SqlInt64.Xor (Test58, Test130).Value, "#Q03");
			Assert.AreEqual ((long) 0, SqlInt64.Xor (TestMax, TestMax).Value, "#Q04");
			Assert.AreEqual (TestMax.Value, SqlInt64.Xor (TestMax, Test0).Value, "#Q05");
		}

		// OPERATORS

		[Test]
		public void ArithmeticOperators ()
		{
			SqlInt64 Test24 = new SqlInt64 (24);
			SqlInt64 Test64 = new SqlInt64 (64);
			SqlInt64 Test2550 = new SqlInt64 (2550);
			SqlInt64 Test0 = new SqlInt64 (0);

			// "+"-operator
			Assert.AreEqual ((SqlInt64) 2614, Test2550 + Test64, "#R01");
			try {
				SqlInt64 result = Test64 + SqlInt64.MaxValue;
				Assert.Fail ("#R02");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#R03");
			}

			// "/"-operator
			Assert.AreEqual ((SqlInt64) 39, Test2550 / Test64, "#R04");
			Assert.AreEqual ((SqlInt64) 0, Test24 / Test64, "#R05");

			try {
				SqlInt64 result = Test2550 / Test0;
				Assert.Fail ("#R06");
			} catch (DivideByZeroException e) {
				Assert.AreEqual (typeof (DivideByZeroException), e.GetType (), "#R07");
			}

			// "*"-operator
			Assert.AreEqual ((SqlInt64) 1536, Test64 * Test24, "#R08");

			try {
				SqlInt64 test = (SqlInt64.MaxValue * Test64);
				Assert.Fail ("TestC#2");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#R08");
			}

			// "-"-operator
			Assert.AreEqual ((SqlInt64) 2526, Test2550 - Test24, "#R09");

			try {
				SqlInt64 test = SqlInt64.MinValue - Test64;
				Assert.Fail ("#R10");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#R11");
			}

			// "%"-operator
			Assert.AreEqual ((SqlInt64) 54, Test2550 % Test64, "#R12");
			Assert.AreEqual ((SqlInt64) 24, Test24 % Test64, "#R13");
			Assert.AreEqual ((SqlInt64) 0, new SqlInt64 (100) % new SqlInt64 (10), "#R14");
		}

		[Test]
		public void BitwiseOperators ()
		{
			SqlInt64 Test2 = new SqlInt64 (2);
			SqlInt64 Test4 = new SqlInt64 (4);

			SqlInt64 Test2550 = new SqlInt64 (2550);

			// & -operator
			Assert.AreEqual ((SqlInt64) 0, Test2 & Test4, "#S01");
			Assert.AreEqual ((SqlInt64) 2, Test2 & Test2550, "#S02");
			Assert.AreEqual ((SqlInt64) 0, SqlInt64.MaxValue & SqlInt64.MinValue, "#S03");

			// | -operator
			Assert.AreEqual ((SqlInt64) 6, Test2 | Test4, "#S04");
			Assert.AreEqual ((SqlInt64) 2550, Test2 | Test2550, "#S05");
			Assert.AreEqual ((SqlInt64) (-1), SqlInt64.MinValue | SqlInt64.MaxValue, "#S06");

			//  ^ -operator
			Assert.AreEqual ((SqlInt64) 2546, (Test2550 ^ Test4), "#S07");
			Assert.AreEqual ((SqlInt64) 6, (Test2 ^ Test4), "#S08");
		}

		[Test]
		public void ThanOrEqualOperators ()
		{
			SqlInt64 Test165 = new SqlInt64 (165);
			SqlInt64 Test100 = new SqlInt64 (100);
			SqlInt64 Test100II = new SqlInt64 (100);
			SqlInt64 Test255 = new SqlInt64 (2550);

			// == -operator
			Assert.IsTrue ((Test100 == Test100II).Value, "#T01");
			Assert.IsTrue (!(Test165 == Test100).Value, "#T02");
			Assert.IsTrue ((Test165 == SqlInt64.Null).IsNull, "#T03");

			// != -operator
			Assert.IsTrue (!(Test100 != Test100II).Value, "#T04");
			Assert.IsTrue ((Test100 != Test255).Value, "#T05");
			Assert.IsTrue ((Test165 != Test255).Value, "#T06");
			Assert.IsTrue ((Test165 != SqlInt64.Null).IsNull, "#T07");

			// > -operator
			Assert.IsTrue ((Test165 > Test100).Value, "#T08");
			Assert.IsTrue (!(Test165 > Test255).Value, "#T09");
			Assert.IsTrue (!(Test100 > Test100II).Value, "#T10");
			Assert.IsTrue ((Test165 > SqlInt64.Null).IsNull, "#T11");

			// >=  -operator
			Assert.IsTrue (!(Test165 >= Test255).Value, "#T12");
			Assert.IsTrue ((Test255 >= Test165).Value, "#T13");
			Assert.IsTrue ((Test100 >= Test100II).Value, "#T14");
			Assert.IsTrue ((Test165 >= SqlInt64.Null).IsNull, "#T15");

			// < -operator
			Assert.IsTrue (!(Test165 < Test100).Value, "#T16");
			Assert.IsTrue ((Test165 < Test255).Value, "#T17");
			Assert.IsTrue (!(Test100 < Test100II).Value, "#T18");
			Assert.IsTrue ((Test165 < SqlInt64.Null).IsNull, "#T19");

			// <= -operator
			Assert.IsTrue ((Test165 <= Test255).Value, "#T20");
			Assert.IsTrue (!(Test255 <= Test165).Value, "#T21");
			Assert.IsTrue ((Test100 <= Test100II).Value, "#T22");
			Assert.IsTrue ((Test165 <= SqlInt64.Null).IsNull, "#T23");
		}

		[Test]
		public void OnesComplementOperator ()
		{
			SqlInt64 Test12 = new SqlInt64 (12);
			SqlInt64 Test128 = new SqlInt64 (128);

			Assert.AreEqual ((SqlInt64) (-13), ~Test12, "#V01");
			Assert.AreEqual ((SqlInt64) (-129), ~Test128, "#V02");
			Assert.AreEqual (SqlInt64.Null, ~SqlInt64.Null, "#V03");
		}

		[Test]
		public void UnaryNegation ()
		{
			SqlInt64 Test = new SqlInt64 (2000);
			SqlInt64 TestNeg = new SqlInt64 (-3000);

			SqlInt64 Result = -Test;
			Assert.AreEqual ((long) (-2000), Result.Value, "#W01");

			Result = -TestNeg;
			Assert.AreEqual ((long) 3000, Result.Value, "#W02");
		}

		[Test]
		public void SqlBooleanToSqlInt64 ()
		{
			SqlBoolean TestBoolean = new SqlBoolean (true);
			SqlInt64 Result;

			Result = (SqlInt64) TestBoolean;

			Assert.AreEqual ((long) 1, Result.Value, "#X01");

			Result = (SqlInt64) SqlBoolean.Null;
			Assert.IsTrue (Result.IsNull, "#X02");
		}

		[Test]
		public void SqlDecimalToSqlInt64 ()
		{
			SqlDecimal TestDecimal64 = new SqlDecimal (64);
			SqlDecimal TestDecimal900 = new SqlDecimal (90000);

			Assert.AreEqual ((long) 64, ((SqlInt64) TestDecimal64).Value, "#Y01");
			Assert.AreEqual (SqlInt64.Null, ((SqlInt64) SqlDecimal.Null), "#Y02");

			try {
				SqlInt64 test = (SqlInt64) SqlDecimal.MaxValue;
				Assert.Fail ("#Y03");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#Y04");
			}
		}

		[Test]
		public void SqlDoubleToSqlInt64 ()
		{
			SqlDouble TestDouble64 = new SqlDouble (64);
			SqlDouble TestDouble900 = new SqlDouble (90000);

			Assert.AreEqual ((long) 64, ((SqlInt64) TestDouble64).Value, "#Z01");
			Assert.AreEqual (SqlInt64.Null, ((SqlInt64) SqlDouble.Null), "#Z02");

			try {
				SqlInt64 test = (SqlInt64) SqlDouble.MaxValue;
				Assert.Fail ("#Z03");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#Z04");
			}
		}

		[Test]
		public void Sql64IntToInt64 ()
		{
			SqlInt64 Test = new SqlInt64 (12);
			Int64 Result = (Int64) Test;
			Assert.AreEqual ((long) 12, Result, "#AA01");
		}

		[Test]
		public void SqlInt32ToSqlInt64 ()
		{
			SqlInt32 Test64 = new SqlInt32 (64);
			Assert.AreEqual ((long) 64, ((SqlInt64) Test64).Value, "#AB01");
		}

		[Test]
		public void SqlInt16ToSqlInt64 ()
		{
			SqlInt16 Test64 = new SqlInt16 (64);
			Assert.AreEqual ((long) 64, ((SqlInt64) Test64).Value, "#AC01");
		}

		[Test]
		public void SqlMoneyToSqlInt64 ()
		{
			SqlMoney TestMoney64 = new SqlMoney (64);
			Assert.AreEqual ((long) 64, ((SqlInt64) TestMoney64).Value, "#AD01");
		}

		[Test]
		public void SqlSingleToSqlInt64 ()
		{
			SqlSingle TestSingle64 = new SqlSingle (64);
			Assert.AreEqual ((long) 64, ((SqlInt64) TestSingle64).Value, "#AE01");
		}

		[Test]
		public void SqlStringToSqlInt64 ()
		{
			SqlString TestString = new SqlString ("Test string");
			SqlString TestString100 = new SqlString ("100");
			SqlString TestString1000 = new SqlString ("1000000000000000000000");

			Assert.AreEqual ((long) 100, ((SqlInt64) TestString100).Value, "#AF01");

			try {
				SqlInt64 test = (SqlInt64) TestString1000;
				Assert.Fail ("#AF02");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#AF03");
			}

			try {
				SqlInt64 test = (SqlInt64) TestString;
				Assert.Fail ("#AF03");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#AF04");
			}
		}

		[Test]
		public void ByteToSqlInt64 ()
		{
			short TestShort = 14;
			Assert.AreEqual ((long) 14, ((SqlInt64) TestShort).Value, "#G01");
		}
#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlInt64.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("long", qualifiedName.Name, "#A01");
		}

		internal void ReadWriteXmlTestInternal (string xml, 
						       long testval, 
						       string unit_test_id)
		{
			SqlInt64 test;
			SqlInt64 test1;
			XmlSerializer ser;
			StringWriter sw;
			XmlTextWriter xw;
			StringReader sr;
			XmlTextReader xr;

			test = new SqlInt64 (testval);
			ser = new XmlSerializer(typeof(SqlInt64));
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			
			ser.Serialize (xw, test);

			// Assert.AreEqual (xml, sw.ToString (), unit_test_id);

			sr = new StringReader (xml);
			xr = new XmlTextReader (sr);
			test1 = (SqlInt64)ser.Deserialize (xr);

			Assert.AreEqual (testval, test1.Value, unit_test_id);
		}

		[Test]
		public void ReadWriteXmlTest ()
		{
			string xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><long>4556</long>";
			string xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><long>-6445</long>";
			string xml3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><long>0x455687AB3E4D56F</long>";
			long lngtest1 = 4556;
			long lngtest2 = -6445;
			long lngtest3 = 0x455687AB3E4D56F;

			ReadWriteXmlTestInternal (xml1, lngtest1, "BA01");
			ReadWriteXmlTestInternal (xml2, lngtest2, "BA02");
		
			try {
				ReadWriteXmlTestInternal (xml3, lngtest3, "BA03");
				Assert.Fail ("BA03");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#BA03");
			}
		}
#endif
	}
}
