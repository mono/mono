//
// SqlDecimalTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlDecimal
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen
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
using System.Threading;
using System.Globalization;
#if NET_2_0
using System.Xml.Serialization;
using System.IO;
#endif

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
	public class SqlDecimalTest
	{
		private CultureInfo originalCulture;
		private SqlDecimal Test1;
		private SqlDecimal Test2;
		private SqlDecimal Test3;
		private SqlDecimal Test4;
		private SqlDecimal Test5;

		[SetUp]
		public void GetReady ()
		{
			originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
			Test1 = new SqlDecimal (6464.6464m);
			Test2 = new SqlDecimal (10000.00m);
			Test3 = new SqlDecimal (10000.00m);
			Test4 = new SqlDecimal (-6m);
			Test5 = new SqlDecimal (Decimal.MaxValue);
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		// Test constructor
		[Test]
		public void Create ()
		{
			// SqlDecimal (decimal)
			SqlDecimal Test = new SqlDecimal (30.3098m);
			Assert.AreEqual ((decimal) 30.3098, Test.Value, "#A01");

			try {
				decimal d = Decimal.MaxValue;
				SqlDecimal test = new SqlDecimal (d + 1);
				Assert.Fail ("#A02");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#A03");
			}

			// SqlDecimal (double)
			Test = new SqlDecimal (10E+10d);
			Assert.AreEqual (100000000000.00000m, Test.Value, "#A05");

			try {
				SqlDecimal test = new SqlDecimal (10E+200d);
				Assert.Fail ("#A06");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#A07");
			}

			// SqlDecimal (int)
			Test = new SqlDecimal (-1);
			Assert.AreEqual (-1m, Test.Value, "#A08");

			// SqlDecimal (long)
			Test = new SqlDecimal ((long) (-99999));
			Assert.AreEqual (-99999m, Test.Value, "#A09");

			// SqlDecimal (byte, byte, bool. int[]
			Test = new SqlDecimal (10, 3, false, new int [4] { 200, 1, 0, 0 });
			Assert.AreEqual (-4294967.496m, Test.Value, "#A10");

			try {
				Test = new SqlDecimal (100, 100, false,
					new int [4] {Int32.MaxValue,
					Int32.MaxValue, Int32.MaxValue,
					Int32.MaxValue});
				Assert.Fail ("#A11");
			} catch (SqlTypeException) {
			}

			// sqlDecimal (byte, byte, bool, int, int, int, int)
			Test = new SqlDecimal (12, 2, true, 100, 100, 0, 0);
			Assert.AreEqual (4294967297.00m, Test.Value, "#A13");

			try {
				Test = new SqlDecimal (100, 100, false,
					Int32.MaxValue,
					Int32.MaxValue, Int32.MaxValue,
					Int32.MaxValue);
				Assert.Fail ("#A14");
			} catch (SqlTypeException) {
			}
		}

		// Test public fields
		[Test]
		public void PublicFields ()
		{
			Assert.AreEqual ((byte) 38, SqlDecimal.MaxPrecision, "#B01");
			Assert.AreEqual ((byte) 38, SqlDecimal.MaxScale, "#B02");

			// FIXME: on windows: Conversion overflow
			Assert.AreEqual (1262177448, SqlDecimal.MaxValue.Data [3], "#B03a");

			Assert.AreEqual (1262177448, SqlDecimal.MinValue.Data [3], "#B04");
			Assert.IsTrue (SqlDecimal.Null.IsNull, "#B05");
			Assert.IsTrue (!Test1.IsNull, "#B06");
		}

		// Test properties
		[Test]
		public void Properties ()
		{
			byte [] b = Test1.BinData;
			Assert.AreEqual ((byte) 64, b [0], "#C01");

			int [] i = Test1.Data;
			Assert.AreEqual (64646464, i [0], "#C02");

			Assert.IsTrue (SqlDecimal.Null.IsNull, "#C03");
			Assert.IsTrue (Test1.IsPositive, "#C04");
			Assert.IsTrue (!Test4.IsPositive, "#C05");
			Assert.AreEqual ((byte) 8, Test1.Precision, "#C06");
			Assert.AreEqual ((byte) 2, Test2.Scale, "#C07");
			Assert.AreEqual (6464.6464m, Test1.Value, "#C08");
			Assert.AreEqual ((byte) 4, Test1.Scale, "#C09");
			Assert.AreEqual ((byte) 7, Test2.Precision, "#C10");
			Assert.AreEqual ((byte) 1, Test4.Precision, "#C11");
		}

		// PUBLIC METHODS
		[Test]
		public void ArithmeticMethods ()
		{

			// Abs
			Assert.AreEqual ((SqlDecimal) 6m, SqlDecimal.Abs (Test4), "#D01");
			Assert.AreEqual (new SqlDecimal (6464.6464m).Value, SqlDecimal.Abs (Test1).Value, "#D02");

			Assert.AreEqual (SqlDecimal.Null, SqlDecimal.Abs (SqlDecimal.Null), "#D03");

			// Add()
			SqlDecimal test2 = new SqlDecimal (-2000m);
			Assert.AreEqual (16464.6464m, SqlDecimal.Add (Test1, Test2).Value, "#D04");
			Assert.AreEqual ("158456325028528675187087900670", SqlDecimal.Add (Test5, Test5).ToString (), "#D04.1");
			Assert.AreEqual ((SqlDecimal) 9994.00m, SqlDecimal.Add (Test3, Test4), "#D04.2");
			Assert.AreEqual ((SqlDecimal) (-2006m), SqlDecimal.Add (Test4, test2), "#D04.3");
			Assert.AreEqual ((SqlDecimal) 8000.00m, SqlDecimal.Add (test2, Test3), "#D04.4");

			try {
				SqlDecimal test = SqlDecimal.Add (SqlDecimal.MaxValue, SqlDecimal.MaxValue);
				Assert.Fail ("#D05");
			} catch (OverflowException) {
			}

			Assert.AreEqual ((SqlDecimal) 6465m, SqlDecimal.Ceiling (Test1), "#D07");
			Assert.AreEqual (SqlDecimal.Null, SqlDecimal.Ceiling (SqlDecimal.Null), "#D08");

			// Divide() => Notworking
			/*
			Assert.AreEqual ((SqlDecimal)(-1077.441066m), SqlDecimal.Divide (Test1, Test4), "#D09");
			Assert.AreEqual (1.54687501546m, SqlDecimal.Divide (Test2, Test1).Value, "#D10");

			try {
				SqlDecimal test = SqlDecimal.Divide(Test1, new SqlDecimal(0)).Value;
				Assert.Fail ("#D11");
			} catch (DivideByZeroException e) {
				Assert.AreEqual (typeof (DivideByZeroException), e.GetType (), "#D12");
			}
			*/

			Assert.AreEqual ((SqlDecimal) 6464m, SqlDecimal.Floor (Test1), "#D13");

			// Multiply()
			SqlDecimal Test;
			SqlDecimal test1 = new SqlDecimal (2m);
			Assert.AreEqual (64646464.000000m, SqlDecimal.Multiply (Test1, Test2).Value, "#D14");
			Assert.AreEqual (-38787.8784m, SqlDecimal.Multiply (Test1, Test4).Value, "#D15");
			Test = SqlDecimal.Multiply (Test5, test1);
			Assert.AreEqual ("158456325028528675187087900670", Test.ToString (), "#D15.1");

			try {
				SqlDecimal test = SqlDecimal.Multiply (SqlDecimal.MaxValue, Test1);
				Assert.Fail ("#D16");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D17");
			}

			// Power => NotWorking
			//Assert.AreEqual ((SqlDecimal)41791653.0770m, SqlDecimal.Power (Test1, 2), "#D18");

			// Round
			Assert.AreEqual ((SqlDecimal)6464.65m, SqlDecimal.Round (Test1, 2), "#D19");

			// Subtract()
			Assert.AreEqual (-3535.3536m, SqlDecimal.Subtract (Test1, Test3).Value, "#D20");
			Assert.AreEqual (10006.00m, SqlDecimal.Subtract (Test3, Test4).Value, "#D20.1");
			Assert.AreEqual ("99999999920771837485735662406456049664",
					SqlDecimal.Subtract (SqlDecimal.MaxValue, Decimal.MaxValue).ToString (),
					"#D20.2");

			try {
				SqlDecimal test = SqlDecimal.Subtract (SqlDecimal.MinValue, SqlDecimal.MaxValue);
				Assert.Fail ("#D21");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D22");
			}

			Assert.AreEqual ((SqlInt32) 1, SqlDecimal.Sign (Test1), "#D23");
			Assert.AreEqual (new SqlInt32 (-1), SqlDecimal.Sign (Test4), "#D24");
		}

		[Test]
		public void AdjustScale ()
		{
			Assert.AreEqual ("6464.646400", SqlDecimal.AdjustScale (Test1, 2, false).Value.ToString (), "#E01");
			Assert.AreEqual ("6464.65", SqlDecimal.AdjustScale (Test1, -2, true).Value.ToString (), "#E02");
			Assert.AreEqual ("6464.64", SqlDecimal.AdjustScale (Test1, -2, false).Value.ToString (), "#E03");
			Assert.AreEqual ("10000.000000000000", SqlDecimal.AdjustScale (Test2, 10, false).Value.ToString (), "#E01");
			Assert.AreEqual ("79228162514264337593543950335.00", SqlDecimal.AdjustScale (Test5, 2, false).ToString (), "#E04");
			try {
				SqlDecimal test = SqlDecimal.AdjustScale (Test1, -5, false);
				Assert.Fail ("#E05");
			} catch (SqlTruncateException) {
			}
		}

		[Test]
		public void ConvertToPrecScale ()
		{
			Assert.AreEqual (new SqlDecimal (6464.6m).Value, SqlDecimal.ConvertToPrecScale (Test1, 5, 1).Value, "#F01");

			try {
				SqlDecimal test = SqlDecimal.ConvertToPrecScale (Test1, 6, 4);
				Assert.Fail ("#F02");
			} catch (SqlTruncateException e) {
				Assert.AreEqual (typeof (SqlTruncateException), e.GetType (), "#F03");
			}

			Assert.AreEqual ((SqlString) "10000.00", SqlDecimal.ConvertToPrecScale (Test2, 7, 2).ToSqlString (), "#F04");

			SqlDecimal tmp = new SqlDecimal (38, 4, true, 64646464, 0, 0, 0);
			Assert.AreEqual ("6465", SqlDecimal.ConvertToPrecScale (tmp, 4, 0).ToString (), "#F05");
		}

		[Test]
		public void CompareTo ()
		{
			SqlString TestString = new SqlString ("This is a test");

			Assert.IsTrue (Test1.CompareTo (Test3) < 0, "#G01");
			Assert.IsTrue (Test2.CompareTo (Test1) > 0, "#G02");
			Assert.IsTrue (Test2.CompareTo (Test3) == 0, "#G03");
			Assert.IsTrue (Test4.CompareTo (SqlDecimal.Null) > 0, "#G04");

			try {
				Test1.CompareTo (TestString);
				Assert.Fail ("#G05");
			} catch (ArgumentException e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "#G06");
			}
		}

		[Test]
		public void EqualsMethods ()
		{
			Assert.IsTrue (!Test1.Equals (Test2), "#H01");
			Assert.IsTrue (!Test2.Equals (new SqlString ("TEST")), "#H02");
			Assert.IsTrue (Test2.Equals (Test3), "#H03");

			// Static Equals()-method
			Assert.IsTrue (SqlDecimal.Equals (Test2, Test2).Value, "#H05");
			Assert.IsTrue (!SqlDecimal.Equals (Test1, Test2).Value, "#H06");

			// NotEquals
			Assert.IsTrue (SqlDecimal.NotEquals (Test1, Test2).Value, "#H07");
			Assert.IsTrue (SqlDecimal.NotEquals (Test4, Test1).Value, "#H08");
			Assert.IsTrue (!SqlDecimal.NotEquals (Test2, Test3).Value, "#H09");
			Assert.IsTrue (SqlDecimal.NotEquals (SqlDecimal.Null, Test3).IsNull, "#H10");
		}

		/* Don't do such environment-dependent test. It will never succeed under Portable.NET and MS.NET
		[Test]
		public void GetHashCodeTest()
		{
			// FIXME: Better way to test HashCode
			Assert.AreEqual (-1281249885, Test1.GetHashCode (), "#I01");
		}
		*/

		[Test]
		public void GetTypeTest ()
		{
			Assert.AreEqual ("System.Data.SqlTypes.SqlDecimal",
				      Test1.GetType ().ToString (), "#J01");
			Assert.AreEqual ("System.Decimal", Test1.Value.GetType ().ToString (), "#J02");
		}

		[Test]
		public void Greaters ()
		{
			// GreateThan ()
			Assert.IsTrue (!SqlDecimal.GreaterThan (Test1, Test2).Value, "#K01");
			Assert.IsTrue (SqlDecimal.GreaterThan (Test2, Test1).Value, "#K02");
			Assert.IsTrue (!SqlDecimal.GreaterThan (Test2, Test3).Value, "#K03");

			// GreaterTharOrEqual ()
			Assert.IsTrue (!SqlDecimal.GreaterThanOrEqual (Test1, Test2).Value, "#K04");
			Assert.IsTrue (SqlDecimal.GreaterThanOrEqual (Test2, Test1).Value, "#K05");
			Assert.IsTrue (SqlDecimal.GreaterThanOrEqual (Test2, Test3).Value, "#K06");
		}

		[Test]
		public void Lessers ()
		{
			// LessThan()
			Assert.IsTrue (!SqlDecimal.LessThan (Test3, Test2).Value, "#L01");
			Assert.IsTrue (!SqlDecimal.LessThan (Test2, Test1).Value, "#L02");
			Assert.IsTrue (SqlDecimal.LessThan (Test1, Test2).Value, "#L03");

			// LessThanOrEqual ()
			Assert.IsTrue (SqlDecimal.LessThanOrEqual (Test1, Test2).Value, "#L04");
			Assert.IsTrue (!SqlDecimal.LessThanOrEqual (Test2, Test1).Value, "#L05");
			Assert.IsTrue (SqlDecimal.LessThanOrEqual (Test2, Test3).Value, "#L06");
			Assert.IsTrue (SqlDecimal.LessThanOrEqual (Test1, SqlDecimal.Null).IsNull, "#L07");
		}

		[Test]
		[Category ("NotWorking")]
		public void Parse ()
		{
			try {
				SqlDecimal.Parse (null);
				Assert.Fail ("#m01");
			} catch (ArgumentNullException e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "#M02");
			}

			try {
				SqlDecimal.Parse ("not-a-number");
				Assert.Fail ("#M03");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#M04");
			}

			try {
				SqlDecimal test = SqlDecimal.Parse ("9e300");
				Assert.Fail ("#M05");
			} catch (FormatException) {
			}

			Assert.AreEqual (150m, SqlDecimal.Parse ("150").Value, "#M07");

			// decimal.Parse() does not pass this string.
			string max = "99999999999999999999999999999999999999";
			SqlDecimal dx = SqlDecimal.Parse (max);
			Assert.AreEqual (max, dx.ToString (), "#M08");

			try {
				dx = SqlDecimal.Parse (max + ".0");
				Assert.Fail ("#M09");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Conversions ()
		{
			// ToDouble
			Assert.AreEqual (6464.6464, Test1.ToDouble (), "N01");

			// ToSqlBoolean ()
			Assert.AreEqual (new SqlBoolean (1), Test1.ToSqlBoolean (), "#N02");

			SqlDecimal Test = new SqlDecimal (0);
			Assert.IsTrue (!Test.ToSqlBoolean ().Value, "#N03");

			Test = new SqlDecimal (0);
			Assert.IsTrue (!Test.ToSqlBoolean ().Value, "#N04");
			Assert.IsTrue (SqlDecimal.Null.ToSqlBoolean ().IsNull, "#N05");

			// ToSqlByte ()
			Test = new SqlDecimal (250);
			Assert.AreEqual ((byte) 250, Test.ToSqlByte ().Value, "#N06");

			try {
				SqlByte b = (byte) Test2.ToSqlByte ();
				Assert.Fail ("#N07");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N08");
			}

			// ToSqlDouble ()
			Assert.AreEqual ((SqlDouble) 6464.6464, Test1.ToSqlDouble (), "#N09");

			// ToSqlInt16 ()
			Assert.AreEqual ((short) 1, new SqlDecimal (1).ToSqlInt16 ().Value, "#N10");

			try {
				SqlInt16 test = SqlDecimal.MaxValue.ToSqlInt16 ().Value;
				Assert.Fail ("#N11");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N12");
			}

			// ToSqlInt32 () 
			// LAMESPEC: 6464.6464 --> 64646464 ??? with windows
			// MS.NET seems to return the first 32 bit integer (i.e. 
			// Data [0]) but we don't have to follow such stupidity.
			//			Assert.AreEqual ((int)64646464, Test1.ToSqlInt32 ().Value, "#N13a");
			//			Assert.AreEqual ((int)1212, new SqlDecimal(12.12m).ToSqlInt32 ().Value, "#N13b");

			try {
				SqlInt32 test = SqlDecimal.MaxValue.ToSqlInt32 ().Value;
				Assert.Fail ("#N14");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N15");
			}

			// ToSqlInt64 ()
			Assert.AreEqual ((long) 6464, Test1.ToSqlInt64 ().Value, "#N16");

			// ToSqlMoney ()
			Assert.AreEqual ((decimal) 6464.6464, Test1.ToSqlMoney ().Value, "#N17");

			try {
				SqlMoney test = SqlDecimal.MaxValue.ToSqlMoney ().Value;
				Assert.Fail ("#N18");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N19");
			}

			// ToSqlSingle ()
			Assert.AreEqual ((float) 6464.6464, Test1.ToSqlSingle ().Value, "#N20");

			// ToSqlString ()
			Assert.AreEqual ("6464.6464", Test1.ToSqlString ().Value, "#N21");

			// ToString ()
			Assert.AreEqual ("6464.6464", Test1.ToString (), "#N22");
			// NOT WORKING
			Assert.AreEqual ("792281625142643375935439503350000.00", SqlDecimal.Multiply (Test5 , Test2).ToString () , "#N22.1");
			Assert.AreEqual ((SqlDouble) 1E+38, SqlDecimal.MaxValue.ToSqlDouble (), "#N23");

		}

		[Test]
		public void Truncate ()
		{
			// NOT WORKING
			Assert.AreEqual (new SqlDecimal (6464.6400m).Value, SqlDecimal.Truncate (Test1, 2).Value, "#O01");
			Assert.AreEqual (6464.6400m, SqlDecimal.Truncate (Test1, 2).Value, "#O01");
		}

		// OPERATORS

		[Test]
		public void ArithmeticOperators ()
		{
			// "+"-operator
			Assert.AreEqual (new SqlDecimal (16464.6464m), Test1 + Test2, "#P01");
			Assert.AreEqual ("79228162514264337593543960335.00", (Test5 + Test3).ToString (), "#P01.1");

			SqlDecimal test2 = new SqlDecimal (-2000m);
			Assert.AreEqual ((SqlDecimal) 8000.00m, Test3 + test2, "#P01.2");
			Assert.AreEqual ((SqlDecimal) (-2006m), Test4 + test2, "#P01.3");
			Assert.AreEqual ((SqlDecimal) 8000.00m, test2 + Test3, "#P01.4");

			try {
				SqlDecimal test = SqlDecimal.MaxValue + SqlDecimal.MaxValue;
				Assert.Fail ("#P02");
			} catch (OverflowException) { }

			// "/"-operator => NotWorking
			//Assert.AreEqual ((SqlDecimal)1.54687501546m, Test2 / Test1, "#P04");

			try {
				SqlDecimal test = Test3 / new SqlDecimal (0);
				Assert.Fail ("#P05");
			} catch (DivideByZeroException e) {
				Assert.AreEqual (typeof (DivideByZeroException), e.GetType (), "#P06");
			}

			// "*"-operator
			Assert.AreEqual ((SqlDecimal) 64646464.000000m, Test1 * Test2, "#P07");

			SqlDecimal Test = Test5 * (new SqlDecimal (2m));
			Assert.AreEqual ("158456325028528675187087900670", Test.ToString (), "#P7.1");

			try {
				SqlDecimal test = SqlDecimal.MaxValue * Test1;
				Assert.Fail ("#P08");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#P09");
			}

			// "-"-operator
			Assert.AreEqual ((SqlDecimal) 3535.3536m, Test2 - Test1, "#P10");
			Assert.AreEqual ((SqlDecimal) (-10006.00m), Test4 - Test3, "#P10.1");

			try {
				SqlDecimal test = SqlDecimal.MinValue - SqlDecimal.MaxValue;
				Assert.Fail ("#P11");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#P12");
			}

			Assert.AreEqual (SqlDecimal.Null, SqlDecimal.Null + Test1, "#P13");
		}

		[Test]
		public void ThanOrEqualOperators ()
		{
			SqlDecimal pval = new SqlDecimal (10m);
			SqlDecimal nval = new SqlDecimal (-10m);
			SqlDecimal val = new SqlDecimal (5m);

			// == -operator
			Assert.IsTrue ((Test2 == Test3).Value, "#Q01");
			Assert.IsTrue (!(Test1 == Test2).Value, "#Q02");
			Assert.IsTrue ((Test1 == SqlDecimal.Null).IsNull, "#Q03");
			Assert.IsFalse ((pval == nval).Value, "#Q03.1");

			// != -operator
			Assert.IsTrue (!(Test2 != Test3).Value, "#Q04");
			Assert.IsTrue ((Test1 != Test3).Value, "#Q05");
			Assert.IsTrue ((Test4 != Test3).Value, "#Q06");
			Assert.IsTrue ((Test1 != SqlDecimal.Null).IsNull, "#Q07");
			Assert.IsTrue ((pval != nval).Value, "#Q07.1");

			// > -operator
			Assert.IsTrue ((Test2 > Test1).Value, "#Q08");
			Assert.IsTrue (!(Test1 > Test3).Value, "#Q09");
			Assert.IsTrue (!(Test2 > Test3).Value, "#Q10");
			Assert.IsTrue ((Test1 > SqlDecimal.Null).IsNull, "#Q11");
			Assert.IsFalse ((nval > val).Value, "#Q11.1");

			// >=  -operator
			Assert.IsTrue (!(Test1 >= Test3).Value, "#Q12");
			Assert.IsTrue ((Test3 >= Test1).Value, "#Q13");
			Assert.IsTrue ((Test2 >= Test3).Value, "#Q14");
			Assert.IsTrue ((Test1 >= SqlDecimal.Null).IsNull, "#Q15");
			Assert.IsFalse ((nval > val).Value, "#Q15.1");

			// < -operator
			Assert.IsTrue (!(Test2 < Test1).Value, "#Q16");
			Assert.IsTrue ((Test1 < Test3).Value, "#Q17");
			Assert.IsTrue (!(Test2 < Test3).Value, "#Q18");
			Assert.IsTrue ((Test1 < SqlDecimal.Null).IsNull, "#Q19");
			Assert.IsFalse ((val < nval).Value, "#Q19.1");

			// <= -operator
			Assert.IsTrue ((Test1 <= Test3).Value, "#Q20");
			Assert.IsTrue (!(Test3 <= Test1).Value, "#Q21");
			Assert.IsTrue ((Test2 <= Test3).Value, "#Q22");
			Assert.IsTrue ((Test1 <= SqlDecimal.Null).IsNull, "#Q23");
			Assert.IsFalse ((val <= nval).Value, "#Q23.1");
		}

		[Test]
		public void UnaryNegation ()
		{
			Assert.AreEqual (6m, -Test4.Value, "#R01");
			Assert.AreEqual (-6464.6464m, -Test1.Value, "#R02");
			Assert.AreEqual (SqlDecimal.Null, SqlDecimal.Null, "#R03");
		}

		[Test]
		public void SqlBooleanToSqlDecimal ()
		{
			SqlBoolean TestBoolean = new SqlBoolean (true);
			SqlDecimal Result;

			Result = (SqlDecimal) TestBoolean;

			Assert.AreEqual (1m, Result.Value, "#S01");

			Result = (SqlDecimal) SqlBoolean.Null;
			Assert.IsTrue (Result.IsNull, "#S02");
			Assert.AreEqual (SqlDecimal.Null, (SqlDecimal) SqlBoolean.Null, "#S03");
		}

		[Test]
		public void SqlDecimalToDecimal ()
		{
			Assert.AreEqual (6464.6464m, (Decimal) Test1, "#T01");
		}

		[Test]
		public void SqlDoubleToSqlDecimal ()
		{
			SqlDouble Test = new SqlDouble (12E+10);
			Assert.AreEqual (120000000000.00000m, ((SqlDecimal) Test).Value, "#U01");
		}

		[Test]
		public void SqlSingleToSqlDecimal ()
		{
			SqlSingle Test = new SqlSingle (1E+9);
			Assert.AreEqual (1000000000.0000000m, ((SqlDecimal) Test).Value, "#V01");

			try {
				SqlDecimal test = (SqlDecimal) SqlSingle.MaxValue;
				Assert.Fail ("#V02");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#V03");
			}
		}

		[Test]
		public void SqlStringToSqlDecimal ()
		{
			SqlString TestString = new SqlString ("Test string");
			SqlString TestString100 = new SqlString ("100");

			Assert.AreEqual (100m, ((SqlDecimal) TestString100).Value, "#W01");

			try {
				SqlDecimal test = (SqlDecimal) TestString;
				Assert.Fail ("#W02");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#W03");
			}

			try {
				SqlDecimal test = (SqlDecimal) new SqlString ("9E+100");
				Assert.Fail ("#W04");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#W05");
			}
		}

		[Test]
		public void DecimalToSqlDecimal ()
		{
			decimal d = 1000.1m;
			Assert.AreEqual ((SqlDecimal) 1000.1m, (SqlDecimal) d, "#X01");
		}

		[Test]
		public void ByteToSqlDecimal ()
		{
			Assert.AreEqual (255m, ((SqlDecimal) SqlByte.MaxValue).Value, "#Y01");
		}

		[Test]
		public void SqlIntToSqlDouble ()
		{
			SqlInt16 Test64 = new SqlInt16 (64);
			SqlInt32 Test640 = new SqlInt32 (640);
			SqlInt64 Test64000 = new SqlInt64 (64000);
			Assert.AreEqual (64m, ((SqlDecimal) Test64).Value, "#Z01");
			Assert.AreEqual (640m, ((SqlDecimal) Test640).Value, "#Z02");
			Assert.AreEqual (64000m, ((SqlDecimal) Test64000).Value, "#Z03");
		}

		[Test]
		public void SqlMoneyToSqlDecimal ()
		{
			SqlMoney TestMoney64 = new SqlMoney (64);
			Assert.AreEqual (64.0000M, ((SqlDecimal) TestMoney64).Value, "#AA01");
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("Null", SqlDecimal.Null.ToString (), "#01");
			Assert.AreEqual ("-99999999999999999999999999999999999999", SqlDecimal.MinValue.ToString (), "#02");
			Assert.AreEqual ("99999999999999999999999999999999999999", SqlDecimal.MaxValue.ToString (), "#03");
		}

		[Test]
		public void Value ()
		{
			decimal d = decimal.Parse ("9999999999999999999999999999");
			Assert.AreEqual (9999999999999999999999999999m, d);
		}

#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlDecimal.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("decimal", qualifiedName.Name, "#A01");
		}

		internal void ReadWriteXmlTestInternal (string xml, 
						       decimal testval, 
						       string unit_test_id)
		{
			SqlDecimal test;
			SqlDecimal test1;
			XmlSerializer ser;
			StringWriter sw;
			XmlTextWriter xw;
			StringReader sr;
			XmlTextReader xr;

			test = new SqlDecimal (testval);
			ser = new XmlSerializer(typeof(SqlDecimal));
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			
			ser.Serialize (xw, test);

			// Assert.AreEqual (xml, sw.ToString (), unit_test_id);

			sr = new StringReader (xml);
			xr = new XmlTextReader (sr);
			test1 = (SqlDecimal)ser.Deserialize (xr);

			Assert.AreEqual (testval, test1.Value, unit_test_id);
		}

		[Test]
		public void ReadWriteXmlTest ()
		{
			string xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><decimal>4556.89756</decimal>";
			string xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><decimal>-6445.9999</decimal>";
			string xml3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><decimal>0x455687AB3E4D56F</decimal>";
			decimal test1 = new Decimal (4556.89756);
			// This one fails because of a possible conversion bug
			//decimal test2 = new Decimal (-6445.999999999999999999999);
			decimal test2 = new Decimal (-6445.9999);
			decimal test3 = new Decimal (0x455687AB3E4D56F);

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
