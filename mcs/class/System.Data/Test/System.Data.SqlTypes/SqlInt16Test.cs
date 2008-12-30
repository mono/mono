//
// SqlInt16Test.cs - NUnit Test Cases for System.Data.SqlTypes.SqlInt16
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Ville Palo
// (C) 2003 Martin Willemoes Hansen
// 
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

using NUnit.Framework;
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

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
	public class SqlInt16Test
	{
		// Test constructor
		[Test]
		public void Create ()
		{
			SqlInt16 TestShort = new SqlInt16 (29);
			Assert.AreEqual ((short) 29, TestShort.Value, "Test#1");

			TestShort = new SqlInt16 (-9000);
			Assert.AreEqual ((short) -9000, TestShort.Value, "Test#2");
		}

		// Test public fields
		[Test]
		public void PublicFields ()
		{
			Assert.AreEqual ((SqlInt16) 32767, SqlInt16.MaxValue, "Test#1");
			Assert.AreEqual ((SqlInt16) (-32768), SqlInt16.MinValue, "Test#2");
			Assert.IsTrue (SqlInt16.Null.IsNull, "Test#3");
			Assert.AreEqual ((short) 0, SqlInt16.Zero.Value, "Test#4");
		}

		// Test properties
		[Test]
		public void Properties ()
		{
			SqlInt16 Test5443 = new SqlInt16 (5443);
			SqlInt16 Test1 = new SqlInt16 (1);
			Assert.IsTrue (SqlInt16.Null.IsNull, "Test#1");
			Assert.AreEqual ((short) 5443, Test5443.Value, "Test#2");
			Assert.AreEqual ((short) 1, Test1.Value, "Test#3");
		}

		// PUBLIC METHODS

		[Test]
		public void ArithmeticMethods ()
		{
			SqlInt16 Test64 = new SqlInt16 (64);
			SqlInt16 Test0 = new SqlInt16 (0);
			SqlInt16 Test164 = new SqlInt16 (164);
			SqlInt16 TestMax = new SqlInt16 (SqlInt16.MaxValue.Value);

			// Add()
			Assert.AreEqual ((short) 64, SqlInt16.Add (Test64, Test0).Value, "Test#1");
			Assert.AreEqual ((short) 228, SqlInt16.Add (Test64, Test164).Value, "Test#2");
			Assert.AreEqual ((short) 164, SqlInt16.Add (Test0, Test164).Value, "Test#3");
			Assert.AreEqual ((short) SqlInt16.MaxValue, SqlInt16.Add (TestMax, Test0).Value, "Test#4");

			try {
				SqlInt16.Add (TestMax, Test64);
				Assert.Fail ("Test#5");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "Test#6");
			}

			// Divide()
			Assert.AreEqual ((short) 2, SqlInt16.Divide (Test164, Test64).Value, "Test#7");
			Assert.AreEqual ((short) 0, SqlInt16.Divide (Test64, Test164).Value, "Test#8");
			try {
				SqlInt16.Divide (Test64, Test0);
				Assert.Fail ("Test#9");
			} catch (DivideByZeroException e) {
				Assert.AreEqual (typeof (DivideByZeroException), e.GetType (), "Test#10");
			}

			// Mod()
			Assert.AreEqual ((SqlInt16) 36, SqlInt16.Mod (Test164, Test64), "Test#11");
			Assert.AreEqual ((SqlInt16) 64, SqlInt16.Mod (Test64, Test164), "Test#12");

			// Multiply()
			Assert.AreEqual ((short) 10496, SqlInt16.Multiply (Test64, Test164).Value, "Test#13");
			Assert.AreEqual ((short) 0, SqlInt16.Multiply (Test64, Test0).Value, "Test#14");

			try {
				SqlInt16.Multiply (TestMax, Test64);
				Assert.Fail ("Test#15");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "Test#16");
			}

			// Subtract()
			Assert.AreEqual ((short) 100, SqlInt16.Subtract (Test164, Test64).Value, "Test#17");

			try {
				SqlInt16.Subtract (SqlInt16.MinValue, Test164);
				Assert.Fail ("Test#18");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "Test#19");
			}

#if NET_2_0
			// Modulus ()
			Assert.AreEqual ((SqlInt16)36, SqlInt16.Modulus (Test164, Test64), "Test#20");
			Assert.AreEqual ((SqlInt16)64, SqlInt16.Modulus (Test64, Test164), "Test#21");
#endif
		}

		[Test]
		public void BitwiseMethods ()
		{
			short MaxValue = SqlInt16.MaxValue.Value;
			SqlInt16 TestInt = new SqlInt16 (0);
			SqlInt16 TestIntMax = new SqlInt16 (MaxValue);
			SqlInt16 TestInt2 = new SqlInt16 (10922);
			SqlInt16 TestInt3 = new SqlInt16 (21845);

			// BitwiseAnd
			Assert.AreEqual ((short) 21845, SqlInt16.BitwiseAnd (TestInt3, TestIntMax).Value, "Test#1");
			Assert.AreEqual ((short) 0, SqlInt16.BitwiseAnd (TestInt2, TestInt3).Value, "Test#2");
			Assert.AreEqual ((short) 10922, SqlInt16.BitwiseAnd (TestInt2, TestIntMax).Value, "Test#3");

			//BitwiseOr
			Assert.AreEqual ((short) MaxValue, SqlInt16.BitwiseOr (TestInt2, TestInt3).Value, "Test#4");
			Assert.AreEqual ((short) 21845, SqlInt16.BitwiseOr (TestInt, TestInt3).Value, "Test#5");
			Assert.AreEqual ((short) MaxValue, SqlInt16.BitwiseOr (TestIntMax, TestInt2).Value, "Test#6");
		}

		[Test]
		public void CompareTo ()
		{
			SqlInt16 TestInt4000 = new SqlInt16 (4000);
			SqlInt16 TestInt4000II = new SqlInt16 (4000);
			SqlInt16 TestInt10 = new SqlInt16 (10);
			SqlInt16 TestInt10000 = new SqlInt16 (10000);
			SqlString TestString = new SqlString ("This is a test");

			Assert.IsTrue (TestInt4000.CompareTo (TestInt10) > 0, "Test#1");
			Assert.IsTrue (TestInt10.CompareTo (TestInt4000) < 0, "Test#2");
			Assert.IsTrue (TestInt4000II.CompareTo (TestInt4000) == 0, "Test#3");
			Assert.IsTrue (TestInt4000II.CompareTo (SqlInt16.Null) > 0, "Test#4");

			try {
				TestInt10.CompareTo (TestString);
				Assert.Fail ("Test#5");
			} catch (ArgumentException e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "Test#6");
			}
		}

		[Test]
		public void EqualsMethod ()
		{
			SqlInt16 Test0 = new SqlInt16 (0);
			SqlInt16 Test158 = new SqlInt16 (158);
			SqlInt16 Test180 = new SqlInt16 (180);
			SqlInt16 Test180II = new SqlInt16 (180);

			Assert.IsTrue (!Test0.Equals (Test158), "Test#1");
			Assert.IsTrue (!Test158.Equals (Test180), "Test#2");
			Assert.IsTrue (!Test180.Equals (new SqlString ("TEST")), "Test#3");
			Assert.IsTrue (Test180.Equals (Test180II), "Test#4");
		}

		[Test]
		public void StaticEqualsMethod ()
		{
			SqlInt16 Test34 = new SqlInt16 (34);
			SqlInt16 Test34II = new SqlInt16 (34);
			SqlInt16 Test15 = new SqlInt16 (15);

			Assert.IsTrue (SqlInt16.Equals (Test34, Test34II).Value, "Test#1");
			Assert.IsTrue (!SqlInt16.Equals (Test34, Test15).Value, "Test#2");
			Assert.IsTrue (!SqlInt16.Equals (Test15, Test34II).Value, "Test#3");
		}

		[Test]
		public void GetHashCodeTest ()
		{
			SqlInt16 Test15 = new SqlInt16 (15);

			// FIXME: Better way to test GetHashCode()-methods
			Assert.AreEqual (Test15.GetHashCode (), Test15.GetHashCode (), "Test#1");
		}

		[Test]
		public void GetTypeTest ()
		{
			SqlInt16 Test = new SqlInt16 (84);
			Assert.AreEqual ("System.Data.SqlTypes.SqlInt16", Test.GetType ().ToString (), "Test#1");
		}

		[Test]
		public void Greaters ()
		{
			SqlInt16 Test10 = new SqlInt16 (10);
			SqlInt16 Test10II = new SqlInt16 (10);
			SqlInt16 Test110 = new SqlInt16 (110);

			// GreateThan ()
			Assert.IsTrue (!SqlInt16.GreaterThan (Test10, Test110).Value, "Test#1");
			Assert.IsTrue (SqlInt16.GreaterThan (Test110, Test10).Value, "Test#2");
			Assert.IsTrue (!SqlInt16.GreaterThan (Test10II, Test10).Value, "Test#3");

			// GreaterTharOrEqual ()
			Assert.IsTrue (!SqlInt16.GreaterThanOrEqual (Test10, Test110).Value, "Test#4");
			Assert.IsTrue (SqlInt16.GreaterThanOrEqual (Test110, Test10).Value, "Test#5");
			Assert.IsTrue (SqlInt16.GreaterThanOrEqual (Test10II, Test10).Value, "Test#6");
		}

		[Test]
		public void Lessers ()
		{
			SqlInt16 Test10 = new SqlInt16 (10);
			SqlInt16 Test10II = new SqlInt16 (10);
			SqlInt16 Test110 = new SqlInt16 (110);

			// LessThan()
			Assert.IsTrue (SqlInt16.LessThan (Test10, Test110).Value, "Test#1");
			Assert.IsTrue (!SqlInt16.LessThan (Test110, Test10).Value, "Test#2");
			Assert.IsTrue (!SqlInt16.LessThan (Test10II, Test10).Value, "Test#3");

			// LessThanOrEqual ()
			Assert.IsTrue (SqlInt16.LessThanOrEqual (Test10, Test110).Value, "Test#4");
			Assert.IsTrue (!SqlInt16.LessThanOrEqual (Test110, Test10).Value, "Test#5");
			Assert.IsTrue (SqlInt16.LessThanOrEqual (Test10II, Test10).Value, "Test#6");
			Assert.IsTrue (SqlInt16.LessThanOrEqual (Test10II, SqlInt16.Null).IsNull, "Test#7");
		}

		[Test]
		public void NotEquals ()
		{
			SqlInt16 Test12 = new SqlInt16 (12);
			SqlInt16 Test128 = new SqlInt16 (128);
			SqlInt16 Test128II = new SqlInt16 (128);

			Assert.IsTrue (SqlInt16.NotEquals (Test12, Test128).Value, "Test#1");
			Assert.IsTrue (SqlInt16.NotEquals (Test128, Test12).Value, "Test#2");
			Assert.IsTrue (SqlInt16.NotEquals (Test128II, Test12).Value, "Test#3");
			Assert.IsTrue (!SqlInt16.NotEquals (Test128II, Test128).Value, "Test#4");
			Assert.IsTrue (!SqlInt16.NotEquals (Test128, Test128II).Value, "Test#5");
			Assert.IsTrue (SqlInt16.NotEquals (SqlInt16.Null, Test128II).IsNull, "Test#6");
			Assert.IsTrue (SqlInt16.NotEquals (SqlInt16.Null, Test128II).IsNull, "Test#7");
		}

		[Test]
		public void OnesComplement ()
		{
			SqlInt16 Test12 = new SqlInt16 (12);
			SqlInt16 Test128 = new SqlInt16 (128);

			Assert.AreEqual ((SqlInt16) (-13), SqlInt16.OnesComplement (Test12), "Test#1");
			Assert.AreEqual ((SqlInt16) (-129), SqlInt16.OnesComplement (Test128), "Test#2");
		}

		[Test]
		public void Parse ()
		{
			try {
				SqlInt16.Parse (null);
				Assert.Fail ("Test#1");
			} catch (ArgumentNullException e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "Test#2");
			}

			try {
				SqlInt16.Parse ("not-a-number");
				Assert.Fail ("Test#3");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "Test#4");
			}

			try {
				int OverInt = (int) SqlInt16.MaxValue + 1;
				SqlInt16.Parse (OverInt.ToString ());
				Assert.Fail ("Test#5");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "Test#6");
			}

			Assert.AreEqual ((short) 150, SqlInt16.Parse ("150").Value, "Test#7");
		}

		[Test]
		public void Conversions ()
		{
			SqlInt16 Test12 = new SqlInt16 (12);
			SqlInt16 Test0 = new SqlInt16 (0);
			SqlInt16 TestNull = SqlInt16.Null;
			SqlInt16 Test1000 = new SqlInt16 (1000);
			SqlInt16 Test288 = new SqlInt16 (288);

			// ToSqlBoolean ()
			Assert.IsTrue (Test12.ToSqlBoolean ().Value, "TestA#1");
			Assert.IsTrue (!Test0.ToSqlBoolean ().Value, "TestA#2");
			Assert.IsTrue (TestNull.ToSqlBoolean ().IsNull, "TestA#3");

			// ToSqlByte ()
			Assert.AreEqual ((byte) 12, Test12.ToSqlByte ().Value, "TestB#1");
			Assert.AreEqual ((byte) 0, Test0.ToSqlByte ().Value, "TestB#2");

			try {
				SqlByte b = (byte) Test1000.ToSqlByte ();
				Assert.Fail ("TestB#4");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "TestB#5");
			}

			// ToSqlDecimal ()
			Assert.AreEqual ((decimal) 12, Test12.ToSqlDecimal ().Value, "TestC#1");
			Assert.AreEqual ((decimal) 0, Test0.ToSqlDecimal ().Value, "TestC#2");
			Assert.AreEqual ((decimal) 288, Test288.ToSqlDecimal ().Value, "TestC#3");

			// ToSqlDouble ()
			Assert.AreEqual ((double) 12, Test12.ToSqlDouble ().Value, "TestD#1");
			Assert.AreEqual ((double) 0, Test0.ToSqlDouble ().Value, "TestD#2");
			Assert.AreEqual ((double) 1000, Test1000.ToSqlDouble ().Value, "TestD#3");

			// ToSqlInt32 ()
			Assert.AreEqual ((int) 12, Test12.ToSqlInt32 ().Value, "TestE#1");
			Assert.AreEqual ((int) 0, Test0.ToSqlInt32 ().Value, "TestE#2");
			Assert.AreEqual ((int) 288, Test288.ToSqlInt32 ().Value, "TestE#3");

			// ToSqlInt64 ()
			Assert.AreEqual ((long) 12, Test12.ToSqlInt64 ().Value, "TestF#1");
			Assert.AreEqual ((long) 0, Test0.ToSqlInt64 ().Value, "TestF#2");
			Assert.AreEqual ((long) 288, Test288.ToSqlInt64 ().Value, "TestF#3");

			// ToSqlMoney ()
			Assert.AreEqual (12.0000M, Test12.ToSqlMoney ().Value, "TestG#1");
			Assert.AreEqual ((decimal) 0, Test0.ToSqlMoney ().Value, "TestG#2");
			Assert.AreEqual (288.0000M, Test288.ToSqlMoney ().Value, "TestG#3");

			// ToSqlSingle ()
			Assert.AreEqual ((float) 12, Test12.ToSqlSingle ().Value, "TestH#1");
			Assert.AreEqual ((float) 0, Test0.ToSqlSingle ().Value, "TestH#2");
			Assert.AreEqual ((float) 288, Test288.ToSqlSingle ().Value, "TestH#3");

			// ToSqlString ()
			Assert.AreEqual ("12", Test12.ToSqlString ().Value, "TestI#1");
			Assert.AreEqual ("0", Test0.ToSqlString ().Value, "TestI#2");
			Assert.AreEqual ("288", Test288.ToSqlString ().Value, "TestI#3");

			// ToString ()
			Assert.AreEqual ("12", Test12.ToString (), "TestJ#1");
			Assert.AreEqual ("0", Test0.ToString (), "TestJ#2");
			Assert.AreEqual ("288", Test288.ToString (), "TestJ#3");
		}

		[Test]
		public void Xor ()
		{
			SqlInt16 Test14 = new SqlInt16 (14);
			SqlInt16 Test58 = new SqlInt16 (58);
			SqlInt16 Test130 = new SqlInt16 (130);
			SqlInt16 TestMax = new SqlInt16 (SqlInt16.MaxValue.Value);
			SqlInt16 Test0 = new SqlInt16 (0);

			Assert.AreEqual ((short) 52, SqlInt16.Xor (Test14, Test58).Value, "Test#1");
			Assert.AreEqual ((short) 140, SqlInt16.Xor (Test14, Test130).Value, "Test#2");
			Assert.AreEqual ((short) 184, SqlInt16.Xor (Test58, Test130).Value, "Test#3");
			Assert.AreEqual ((short) 0, SqlInt16.Xor (TestMax, TestMax).Value, "Test#4");
			Assert.AreEqual (TestMax.Value, SqlInt16.Xor (TestMax, Test0).Value, "Test#5");
		}

		// OPERATORS

		[Test]
		public void ArithmeticOperators ()
		{
			SqlInt16 Test24 = new SqlInt16 (24);
			SqlInt16 Test64 = new SqlInt16 (64);
			SqlInt16 Test2550 = new SqlInt16 (2550);
			SqlInt16 Test0 = new SqlInt16 (0);

			// "+"-operator
			Assert.AreEqual ((SqlInt16) 2614, Test2550 + Test64, "TestA#1");
			try {
				SqlInt16 result = Test64 + SqlInt16.MaxValue;
				Assert.Fail ("TestA#2");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "TestA#3");
			}

			// "/"-operator
			Assert.AreEqual ((SqlInt16) 39, Test2550 / Test64, "TestB#1");
			Assert.AreEqual ((SqlInt16) 0, Test24 / Test64, "TestB#2");

			try {
				SqlInt16 result = Test2550 / Test0;
				Assert.Fail ("TestB#3");
			} catch (DivideByZeroException e) {
				Assert.AreEqual (typeof (DivideByZeroException), e.GetType (), "TestB#4");
			}

			// "*"-operator
			Assert.AreEqual ((SqlInt16) 1536, Test64 * Test24, "TestC#1");

			try {
				SqlInt16 test = (SqlInt16.MaxValue * Test64);
				Assert.Fail ("TestC#2");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "TestC#3");
			}

			// "-"-operator
			Assert.AreEqual ((SqlInt16) 2526, Test2550 - Test24, "TestD#1");

			try {
				SqlInt16 test = SqlInt16.MinValue - Test64;
				Assert.Fail ("TestD#2");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "OverflowException");
			}

			// "%"-operator
			Assert.AreEqual ((SqlInt16) 54, Test2550 % Test64, "TestE#1");
			Assert.AreEqual ((SqlInt16) 24, Test24 % Test64, "TestE#2");
			Assert.AreEqual ((SqlInt16) 0, new SqlInt16 (100) % new SqlInt16 (10), "TestE#1");
		}

		[Test]
		public void BitwiseOperators ()
		{
			SqlInt16 Test2 = new SqlInt16 (2);
			SqlInt16 Test4 = new SqlInt16 (4);
			SqlInt16 Test2550 = new SqlInt16 (2550);

			// & -operator
			Assert.AreEqual ((SqlInt16) 0, Test2 & Test4, "TestA#1");
			Assert.AreEqual ((SqlInt16) 2, Test2 & Test2550, "TestA#2");
			Assert.AreEqual ((SqlInt16) 0, SqlInt16.MaxValue & SqlInt16.MinValue, "TestA#3");

			// | -operator
			Assert.AreEqual ((SqlInt16) 6, Test2 | Test4, "TestB#1");
			Assert.AreEqual ((SqlInt16) 2550, Test2 | Test2550, "TestB#2");
			Assert.AreEqual ((SqlInt16) (-1), SqlInt16.MinValue | SqlInt16.MaxValue, "TestB#3");

			//  ^ -operator
			Assert.AreEqual ((SqlInt16) 2546, (Test2550 ^ Test4), "TestC#1");
			Assert.AreEqual ((SqlInt16) 6, (Test2 ^ Test4), "TestC#2");
		}

		[Test]
		public void ThanOrEqualOperators ()
		{
			SqlInt16 Test165 = new SqlInt16 (165);
			SqlInt16 Test100 = new SqlInt16 (100);
			SqlInt16 Test100II = new SqlInt16 (100);
			SqlInt16 Test255 = new SqlInt16 (2550);

			// == -operator
			Assert.IsTrue ((Test100 == Test100II).Value, "TestA#1");
			Assert.IsTrue (!(Test165 == Test100).Value, "TestA#2");
			Assert.IsTrue ((Test165 == SqlInt16.Null).IsNull, "TestA#3");

			// != -operator
			Assert.IsTrue (!(Test100 != Test100II).Value, "TestB#1");
			Assert.IsTrue ((Test100 != Test255).Value, "TestB#2");
			Assert.IsTrue ((Test165 != Test255).Value, "TestB#3");
			Assert.IsTrue ((Test165 != SqlInt16.Null).IsNull, "TestB#4");

			// > -operator
			Assert.IsTrue ((Test165 > Test100).Value, "TestC#1");
			Assert.IsTrue (!(Test165 > Test255).Value, "TestC#2");
			Assert.IsTrue (!(Test100 > Test100II).Value, "TestC#3");
			Assert.IsTrue ((Test165 > SqlInt16.Null).IsNull, "TestC#4");

			// >=  -operator
			Assert.IsTrue (!(Test165 >= Test255).Value, "TestD#1");
			Assert.IsTrue ((Test255 >= Test165).Value, "TestD#2");
			Assert.IsTrue ((Test100 >= Test100II).Value, "TestD#3");
			Assert.IsTrue ((Test165 >= SqlInt16.Null).IsNull, "TestD#4");

			// < -operator
			Assert.IsTrue (!(Test165 < Test100).Value, "TestE#1");
			Assert.IsTrue ((Test165 < Test255).Value, "TestE#2");
			Assert.IsTrue (!(Test100 < Test100II).Value, "TestE#3");
			Assert.IsTrue ((Test165 < SqlInt16.Null).IsNull, "TestE#4");

			// <= -operator
			Assert.IsTrue ((Test165 <= Test255).Value, "TestF#1");
			Assert.IsTrue (!(Test255 <= Test165).Value, "TestF#2");
			Assert.IsTrue ((Test100 <= Test100II).Value, "TestF#3");
			Assert.IsTrue ((Test165 <= SqlInt16.Null).IsNull, "TestF#4");
		}

		[Test]
		public void OnesComplementOperator ()
		{
			SqlInt16 Test12 = new SqlInt16 (12);
			SqlInt16 Test128 = new SqlInt16 (128);

			Assert.AreEqual ((SqlInt16) (-13), ~Test12, "Test#1");
			Assert.AreEqual ((SqlInt16) (-129), ~Test128, "Test#2");
			Assert.AreEqual (SqlInt16.Null, ~SqlInt16.Null, "Test#3");
		}

		[Test]
		public void UnaryNegation ()
		{
			SqlInt16 Test = new SqlInt16 (2000);
			SqlInt16 TestNeg = new SqlInt16 (-3000);

			SqlInt16 Result = -Test;
			Assert.AreEqual ((short) (-2000), Result.Value, "Test#1");

			Result = -TestNeg;
			Assert.AreEqual ((short) 3000, Result.Value, "Test#2");
		}

		[Test]
		public void SqlBooleanToSqlInt16 ()
		{
			SqlBoolean TestBoolean = new SqlBoolean (true);
			SqlInt16 Result;

			Result = (SqlInt16) TestBoolean;

			Assert.AreEqual ((short) 1, Result.Value, "Test#1");

			Result = (SqlInt16) SqlBoolean.Null;
			Assert.IsTrue (Result.IsNull, "Test#2");
		}

		[Test]
		public void SqlDecimalToSqlInt16 ()
		{
			SqlDecimal TestDecimal64 = new SqlDecimal (64);
			SqlDecimal TestDecimal900 = new SqlDecimal (90000);

			Assert.AreEqual ((short) 64, ((SqlInt16) TestDecimal64).Value, "Test#1");
			Assert.AreEqual (SqlInt16.Null, ((SqlInt16) SqlDecimal.Null), "Test#2");

			try {
				SqlInt16 test = (SqlInt16) TestDecimal900;
				Assert.Fail ("Test#3");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "Test#4");
			}
		}

		[Test]
		public void SqlDoubleToSqlInt16 ()
		{
			SqlDouble TestDouble64 = new SqlDouble (64);
			SqlDouble TestDouble900 = new SqlDouble (90000);

			Assert.AreEqual ((short) 64, ((SqlInt16) TestDouble64).Value, "Test#1");
			Assert.AreEqual (SqlInt16.Null, ((SqlInt16) SqlDouble.Null), "Test#2");

			try {
				SqlInt16 test = (SqlInt16) TestDouble900;
				Assert.Fail ("Test#3");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "Test#4");
			}
		}

		[Test]
		public void SqlIntToInt16 ()
		{
			SqlInt16 Test = new SqlInt16 (12);
			Int16 Result = (Int16) Test;
			Assert.AreEqual ((short) 12, Result, "Test#1");
		}

		[Test]
		public void SqlInt32ToSqlInt16 ()
		{
			SqlInt32 Test64 = new SqlInt32 (64);
			SqlInt32 Test900 = new SqlInt32 (90000);

			Assert.AreEqual ((short) 64, ((SqlInt16) Test64).Value, "Test#1");

			try {
				SqlInt16 test = (SqlInt16) Test900;
				Assert.Fail ("Test#2");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "Test#3");
			}
		}

		[Test]
		public void SqlInt64ToSqlInt16 ()
		{
			SqlInt64 Test64 = new SqlInt64 (64);
			SqlInt64 Test900 = new SqlInt64 (90000);

			Assert.AreEqual ((short) 64, ((SqlInt16) Test64).Value, "Test#1");

			try {
				SqlInt16 test = (SqlInt16) Test900;
				Assert.Fail ("Test#2");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "Test#3");
			}
		}

		[Test]
		public void SqlMoneyToSqlInt16 ()
		{
			SqlMoney TestMoney64 = new SqlMoney (64);
			SqlMoney TestMoney900 = new SqlMoney (90000);

			Assert.AreEqual ((short) 64, ((SqlInt16) TestMoney64).Value, "Test#1");

			try {
				SqlInt16 test = (SqlInt16) TestMoney900;
				Assert.Fail ("Test#2");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "test#3");
			}
		}

		[Test]
		public void SqlSingleToSqlInt16 ()
		{
			SqlSingle TestSingle64 = new SqlSingle (64);
			SqlSingle TestSingle900 = new SqlSingle (90000);

			Assert.AreEqual ((short) 64, ((SqlInt16) TestSingle64).Value, "Test#1");

			try {
				SqlInt16 test = (SqlInt16) TestSingle900;
				Assert.Fail ("Test#2");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "Test#3");
			}
		}

		[Test]
		public void SqlStringToSqlInt16 ()
		{
			SqlString TestString = new SqlString ("Test string");
			SqlString TestString100 = new SqlString ("100");
			SqlString TestString1000 = new SqlString ("100000");

			Assert.AreEqual ((short) 100, ((SqlInt16) TestString100).Value, "Test#1");

			try {
				SqlInt16 test = (SqlInt16) TestString1000;
				Assert.Fail ("Test#2");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "Test#3");
			}

			try {
				SqlInt16 test = (SqlInt16) TestString;
				Assert.Fail ("Test#3");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "Test#4");
			}
		}

		[Test]
		public void ByteToSqlInt16 ()
		{
			short TestShort = 14;
			Assert.AreEqual ((short) 14, ((SqlInt16) TestShort).Value, "Test#1");
		}
#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlInt16.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("short", qualifiedName.Name, "#A01");
		}

		internal void ReadWriteXmlTestInternal (string xml, 
						       short testval, 
						       string unit_test_id)
		{
			SqlInt16 test;
			SqlInt16 test1;
			XmlSerializer ser;
			StringWriter sw;
			XmlTextWriter xw;
			StringReader sr;
			XmlTextReader xr;

			test = new SqlInt16 (testval);
			ser = new XmlSerializer(typeof(SqlInt16));
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			
			ser.Serialize (xw, test);

			// Assert.AreEqual (xml, sw.ToString (), unit_test_id);

			sr = new StringReader (xml);
			xr = new XmlTextReader (sr);
			test1 = (SqlInt16)ser.Deserialize (xr);

			Assert.AreEqual (testval, test1.Value, unit_test_id);
		}

		[Test]
		public void ReadWriteXmlTest ()
		{
			string xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><short>4556</short>";
			string xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><short>-6445</short>";
			string xml3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><short>0x455687AB3E4D56F</short>";
			short test1 = 4556;
			short test2 = -6445;
			short test3 = 0x4F56;

			ReadWriteXmlTestInternal (xml1, test1, "BA01");
			ReadWriteXmlTestInternal (xml2, test2, "BA02");
		
			try {
				ReadWriteXmlTestInternal (xml3, test3, "BA03");
				Assert.Fail ("BA03");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#BA03");
			}
		}
#endif
	}
}
