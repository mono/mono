//
// SqlInt64Test.cs - NUnit Test Cases for System.Data.SqlTypes.SqlInt64
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Ville Palo
// (C) 2003 Martin Willemoes Hansen

using NUnit.Framework;
using System;
using System.Data.SqlTypes;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlInt64Test : Assertion {

                // Test constructor
		[Test]
                public void Create()
                {
                        SqlInt64 TestLong = new SqlInt64 (29);
                        AssertEquals ("#A01", (long)29, TestLong.Value);

                        TestLong = new SqlInt64 (-9000);
                        AssertEquals ("#A02", (long)-9000, TestLong.Value);
                 }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        AssertEquals ("#B01", (long)9223372036854775807, SqlInt64.MaxValue.Value);
                        AssertEquals ("#B02", (long)(-9223372036854775808), SqlInt64.MinValue.Value);
                        Assert ("#B03", SqlInt64.Null.IsNull);
                        AssertEquals ("#B04", (long)0, SqlInt64.Zero.Value);
                }

                // Test properties
		[Test]
                public void Properties()
                {
                        SqlInt64 Test5443 = new SqlInt64 (5443);
                        SqlInt64 Test1 = new SqlInt64 (1);

                        Assert ("#C01", SqlInt64.Null.IsNull);
                        AssertEquals ("#C02", (long)5443, Test5443.Value);
                        AssertEquals ("#C03", (long)1, Test1.Value);
                }

                // PUBLIC METHODS

		[Test]
                public void ArithmeticMethods()
                {
                        SqlInt64 Test64 = new SqlInt64 (64);
                        SqlInt64 Test0 = new SqlInt64 (0);
                        SqlInt64 Test164 = new SqlInt64 (164);
                        SqlInt64 TestMax = new SqlInt64 (SqlInt64.MaxValue.Value);

                        // Add()
                        AssertEquals ("#D01", (long)64, SqlInt64.Add (Test64, Test0).Value);
                        AssertEquals ("#D02", (long)228, SqlInt64.Add (Test64, Test164).Value);
                        AssertEquals ("#D03", (long)164, SqlInt64.Add (Test0, Test164).Value);
                        AssertEquals ("#D04", (long)SqlInt64.MaxValue, SqlInt64.Add (TestMax, Test0).Value);

                        try {
                                SqlInt64.Add (TestMax, Test64);
                                Fail ("#D05");
                        } catch (Exception e) {
                                AssertEquals ("#D06", typeof (OverflowException), e.GetType ());
                        }

                        // Divide()
                        AssertEquals ("#D07", (long)2, SqlInt64.Divide (Test164, Test64).Value);
                        AssertEquals ("#D08", (long)0, SqlInt64.Divide (Test64, Test164).Value);

                        try {
                                SqlInt64.Divide(Test64, Test0);
                                Fail ("#D09");
                        } catch(Exception e) {
                                AssertEquals ("#D10", typeof (DivideByZeroException), e.GetType ());
                        }

                        // Mod()
                        AssertEquals ("#D11", (SqlInt64)36, SqlInt64.Mod (Test164, Test64));
                        AssertEquals ("#D12",  (SqlInt64)64, SqlInt64.Mod (Test64, Test164));

                        // Multiply()
                        AssertEquals ("#D13", (long)10496, SqlInt64.Multiply (Test64, Test164).Value);
                        AssertEquals ("#D14", (long)0, SqlInt64.Multiply (Test64, Test0).Value);

                        try {
                                SqlInt64.Multiply (TestMax, Test64);
                                Fail ("#D15");
                        } catch(Exception e) {
                                AssertEquals ("#D16", typeof (OverflowException), e.GetType ());
                        }

                        // Subtract()
                        AssertEquals ("#D17", (long)100, SqlInt64.Subtract (Test164, Test64).Value);

                        try {
                                SqlInt64.Subtract (SqlInt64.MinValue, Test164);
                                Fail ("#D18");
                        } catch(Exception e) {
                                AssertEquals ("#D19", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void BitwiseMethods()
                {
                        long MaxValue = SqlInt64.MaxValue.Value;
                        SqlInt64 TestInt = new SqlInt64 (0);
                        SqlInt64 TestIntMax = new SqlInt64 (MaxValue);
                        SqlInt64 TestInt2 = new SqlInt64 (10922);
                        SqlInt64 TestInt3 = new SqlInt64 (21845);

                        // BitwiseAnd
                        AssertEquals ("#E01", (long)21845, SqlInt64.BitwiseAnd (TestInt3, TestIntMax).Value);
                        AssertEquals ("#E02", (long)0, SqlInt64.BitwiseAnd (TestInt2, TestInt3).Value);
                        AssertEquals ("#E03", (long)10922, SqlInt64.BitwiseAnd (TestInt2, TestIntMax).Value);

                        //BitwiseOr
                        AssertEquals ("#E04", (long)21845, SqlInt64.BitwiseOr (TestInt, TestInt3).Value);
                        AssertEquals ("#E05", (long)MaxValue, SqlInt64.BitwiseOr (TestIntMax, TestInt2).Value);
                }

		[Test]
                public void CompareTo()
                {
                        SqlInt64 TestInt4000 = new SqlInt64 (4000);
                        SqlInt64 TestInt4000II = new SqlInt64 (4000);
                        SqlInt64 TestInt10 = new SqlInt64 (10);
                        SqlInt64 TestInt10000 = new SqlInt64 (10000);
                        SqlString TestString = new SqlString ("This is a test");

                        Assert ("#F01", TestInt4000.CompareTo (TestInt10) > 0);
                        Assert ("#F02", TestInt10.CompareTo (TestInt4000) < 0);
                        Assert ("#F03", TestInt4000II.CompareTo (TestInt4000) == 0);
                        Assert ("#F04", TestInt4000II.CompareTo (SqlInt64.Null) > 0);

                        try {
                                TestInt10.CompareTo (TestString);
                                Fail("#F05");
                        } catch(Exception e) {
                                AssertEquals ("#F06", typeof (ArgumentException), e.GetType ());
                        }
                }

		[Test]
                public void EqualsMethod()
                {
                        SqlInt64 Test0 = new SqlInt64 (0);
                        SqlInt64 Test158 = new SqlInt64 (158);
                        SqlInt64 Test180 = new SqlInt64 (180);
                        SqlInt64 Test180II = new SqlInt64 (180);

                        Assert ("#G01", !Test0.Equals (Test158));
                        Assert ("#G01", !Test158.Equals (Test180));
                        Assert ("#G03", !Test180.Equals (new SqlString ("TEST")));
                        Assert ("#G04", Test180.Equals (Test180II));
                }

		[Test]
                public void StaticEqualsMethod()
                {
                        SqlInt64 Test34 = new SqlInt64 (34);
                        SqlInt64 Test34II = new SqlInt64 (34);
                        SqlInt64 Test15 = new SqlInt64 (15);

                        Assert ("#H01", SqlInt64.Equals (Test34, Test34II).Value);
                        Assert ("#H02", !SqlInt64.Equals (Test34, Test15).Value);
                        Assert ("#H03", !SqlInt64.Equals (Test15, Test34II).Value);
                }

		[Test]
                public void GetHashCodeTest()
                {
                        SqlInt64 Test15 = new SqlInt64 (15);

                        // FIXME: Better way to test HashCode
                        AssertEquals ("#I01", (int)15, Test15.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
                        SqlInt64 Test = new SqlInt64 (84);
                        AssertEquals ("#J01", "System.Data.SqlTypes.SqlInt64", Test.GetType ().ToString ());
                }

		[Test]
                public void Greaters()
                {
                        SqlInt64 Test10 = new SqlInt64 (10);
                        SqlInt64 Test10II = new SqlInt64 (10);
                        SqlInt64 Test110 = new SqlInt64 (110);

                        // GreateThan ()
                        Assert ("#K01", !SqlInt64.GreaterThan (Test10, Test110).Value);
                        Assert ("#K02", SqlInt64.GreaterThan (Test110, Test10).Value);
                        Assert ("#K03", !SqlInt64.GreaterThan (Test10II, Test10).Value);

                        // GreaterTharOrEqual ()
                        Assert ("#K04", !SqlInt64.GreaterThanOrEqual (Test10, Test110).Value);
                        Assert ("#K05", SqlInt64.GreaterThanOrEqual (Test110, Test10).Value);
                        Assert ("#K06", SqlInt64.GreaterThanOrEqual (Test10II, Test10).Value);
                }

		[Test]
                public void Lessers()
                {
                        SqlInt64 Test10 = new SqlInt64 (10);
                        SqlInt64 Test10II = new SqlInt64 (10);
                        SqlInt64 Test110 = new SqlInt64 (110);

                        // LessThan()
                        Assert ("#L01", SqlInt64.LessThan (Test10, Test110).Value);
                        Assert ("#L02", !SqlInt64.LessThan (Test110, Test10).Value);
                        Assert ("#L03", !SqlInt64.LessThan (Test10II, Test10).Value);

                        // LessThanOrEqual ()
                        Assert ("#L04", SqlInt64.LessThanOrEqual (Test10, Test110).Value);
                        Assert ("#L05", !SqlInt64.LessThanOrEqual (Test110, Test10).Value);
                        Assert ("#L06", SqlInt64.LessThanOrEqual (Test10II, Test10).Value);
                        Assert ("#L07", SqlInt64.LessThanOrEqual (Test10II, SqlInt64.Null).IsNull);
                }

		[Test]
                public void NotEquals()
                {
                        SqlInt64 Test12 = new SqlInt64 (12);
                        SqlInt64 Test128 = new SqlInt64 (128);
                        SqlInt64 Test128II = new SqlInt64 (128);

                        Assert ("#M01", SqlInt64.NotEquals (Test12, Test128).Value);
                        Assert ("#M02", SqlInt64.NotEquals (Test128, Test12).Value);
                        Assert ("#M03", SqlInt64.NotEquals (Test128II, Test12).Value);
                        Assert ("#M04", !SqlInt64.NotEquals (Test128II, Test128).Value);
                        Assert ("#M05", !SqlInt64.NotEquals (Test128, Test128II).Value);
                        Assert ("#M06", SqlInt64.NotEquals (SqlInt64.Null, Test128II).IsNull);
                        Assert ("#M07", SqlInt64.NotEquals (SqlInt64.Null, Test128II).IsNull);
                }

		[Test]
                public void OnesComplement()
                {
                        SqlInt64 Test12 = new SqlInt64(12);
                        SqlInt64 Test128 = new SqlInt64(128);

                        AssertEquals ("#N01", (SqlInt64)(-13), SqlInt64.OnesComplement (Test12));
                        AssertEquals ("#N02", (SqlInt64)(-129), SqlInt64.OnesComplement (Test128));
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlInt64.Parse (null);
                                Fail ("#O01");
                        } catch (Exception e) {
                                AssertEquals ("#O02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlInt64.Parse ("not-a-number");
                                Fail ("#O03");
                        } catch (Exception e) {
                                AssertEquals ("#O04", typeof (FormatException), e.GetType ());
                        }

                        try {
                                SqlInt64.Parse ("1000000000000000000000000000");
                                Fail ("#O05");
                        } catch (Exception e) {
                                AssertEquals ("#O06", typeof (OverflowException), e.GetType ());
                        }

                        AssertEquals("#O07", (long)150, SqlInt64.Parse ("150").Value);
                }

		[Test]
                public void Conversions()
                {
                        SqlInt64 Test12 = new SqlInt64 (12);
                        SqlInt64 Test0 = new SqlInt64 (0);
                        SqlInt64 TestNull = SqlInt64.Null;
                        SqlInt64 Test1000 = new SqlInt64 (1000);
                        SqlInt64 Test288 = new SqlInt64(288);

                        // ToSqlBoolean ()
                        Assert ("#P01", Test12.ToSqlBoolean ().Value);
                        Assert ("#P02", !Test0.ToSqlBoolean ().Value);
                        Assert ("#P03", TestNull.ToSqlBoolean ().IsNull);

                        // ToSqlByte ()
                        AssertEquals ("#P04", (byte)12, Test12.ToSqlByte ().Value);
                        AssertEquals ("#P05", (byte)0, Test0.ToSqlByte ().Value);

                        try {
                                SqlByte b = (byte)Test1000.ToSqlByte ();
                                Fail ("#P06");
                        } catch (Exception e) {
                                AssertEquals ("#P07", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlDecimal ()
                        AssertEquals ("#P08", (decimal)12, Test12.ToSqlDecimal ().Value);
                        AssertEquals ("#P09", (decimal)0, Test0.ToSqlDecimal ().Value);
                        AssertEquals ("#P10", (decimal)288, Test288.ToSqlDecimal ().Value);

                        // ToSqlDouble ()
                        AssertEquals ("#P11", (double)12, Test12.ToSqlDouble ().Value);
                        AssertEquals ("#P12", (double)0, Test0.ToSqlDouble ().Value);
                        AssertEquals ("#P13", (double)1000, Test1000.ToSqlDouble ().Value);

                        // ToSqlInt32 ()
                        AssertEquals ("#P14", (int)12, Test12.ToSqlInt32 ().Value);
                        AssertEquals ("#P15", (int)0, Test0.ToSqlInt32 ().Value);
                        AssertEquals ("#P16", (int)288, Test288.ToSqlInt32().Value);

                        // ToSqlInt16 ()
                        AssertEquals ("#P17", (short)12, Test12.ToSqlInt16 ().Value);
                        AssertEquals ("#P18", (short)0, Test0.ToSqlInt16 ().Value);
                        AssertEquals ("#P19", (short)288, Test288.ToSqlInt16 ().Value);

                        // ToSqlMoney ()
                        AssertEquals ("#P20", (decimal)12, Test12.ToSqlMoney ().Value);
                        AssertEquals ("#P21", (decimal)0, Test0.ToSqlMoney ().Value);
                        AssertEquals ("#P22", (decimal)288, Test288.ToSqlMoney ().Value);

                        // ToSqlSingle ()
                        AssertEquals ("#P23", (float)12, Test12.ToSqlSingle ().Value);
                        AssertEquals ("#P24", (float)0, Test0.ToSqlSingle ().Value);
                        AssertEquals ("#P25", (float)288, Test288.ToSqlSingle().Value);

                        // ToSqlString ()
                        AssertEquals ("#P26", "12", Test12.ToSqlString ().Value);
                        AssertEquals ("#P27", "0", Test0.ToSqlString ().Value);
                        AssertEquals ("#P28", "288", Test288.ToSqlString ().Value);

                        // ToString ()
                        AssertEquals ("#P29", "12", Test12.ToString ());
                        AssertEquals ("#P30", "0", Test0.ToString ());
                        AssertEquals ("#P31", "288", Test288.ToString ());
                }

		[Test]
                public void Xor()
                {
                        SqlInt64 Test14 = new SqlInt64 (14);
                        SqlInt64 Test58 = new SqlInt64 (58);
                        SqlInt64 Test130 = new SqlInt64 (130);
                        SqlInt64 TestMax = new SqlInt64 (SqlInt64.MaxValue.Value);
                        SqlInt64 Test0 = new SqlInt64 (0);

                        AssertEquals ("#Q01", (long)52, SqlInt64.Xor (Test14, Test58).Value);
                        AssertEquals ("#Q02", (long)140, SqlInt64.Xor (Test14, Test130).Value);
                        AssertEquals ("#Q03", (long)184, SqlInt64.Xor (Test58, Test130).Value);
                        AssertEquals ("#Q04", (long)0, SqlInt64.Xor (TestMax, TestMax).Value);
                        AssertEquals ("#Q05", TestMax.Value, SqlInt64.Xor (TestMax, Test0).Value);
                }

                // OPERATORS

		[Test]
                public void ArithmeticOperators()
                {
                        SqlInt64 Test24 = new SqlInt64 (24);
                        SqlInt64 Test64 = new SqlInt64 (64);
                        SqlInt64 Test2550 = new SqlInt64 (2550);
                        SqlInt64 Test0 = new SqlInt64 (0);

                        // "+"-operator
                        AssertEquals ("#R01", (SqlInt64)2614,Test2550 + Test64);
                        try {
                                SqlInt64 result = Test64 + SqlInt64.MaxValue;
                                Fail ("#R02");
                        } catch (Exception e) {
                                AssertEquals ("#R03", typeof (OverflowException), e.GetType ());
                        }

                        // "/"-operator
                        AssertEquals ("#R04", (SqlInt64)39, Test2550 / Test64);
                        AssertEquals ("#R05", (SqlInt64)0, Test24 / Test64);

                        try {
                                SqlInt64 result = Test2550 / Test0;
                                Fail ("#R06");
                        } catch (Exception e) {
                                AssertEquals ("#R07", typeof (DivideByZeroException), e.GetType ());
                        }

                        // "*"-operator
                        AssertEquals ("#R08", (SqlInt64)1536, Test64 * Test24);

                        try {
                                SqlInt64 test = (SqlInt64.MaxValue * Test64);
                                Fail ("TestC#2");
                        } catch (Exception e) {
                                AssertEquals ("#R08", typeof (OverflowException), e.GetType ());
                        }

                        // "-"-operator
                        AssertEquals ("#R09", (SqlInt64)2526, Test2550 - Test24);

                        try {
                                SqlInt64 test = SqlInt64.MinValue - Test64;
                                Fail ("#R10");
                        } catch (Exception e) {
                                AssertEquals ("#R11", typeof (OverflowException), e.GetType ());
                        }

                        // "%"-operator
                        AssertEquals ("#R12", (SqlInt64)54, Test2550 % Test64);
                        AssertEquals ("#R13", (SqlInt64)24, Test24 % Test64);
                        AssertEquals ("#R14", (SqlInt64)0, new SqlInt64 (100) % new SqlInt64 (10));
                }

		[Test]
                public void BitwiseOperators()
                {
                        SqlInt64 Test2 = new SqlInt64 (2);
                        SqlInt64 Test4 = new SqlInt64 (4);

                        SqlInt64 Test2550 = new SqlInt64 (2550);

                        // & -operator
                        AssertEquals ("#S01", (SqlInt64)0, Test2 & Test4);
                        AssertEquals ("#S02", (SqlInt64)2, Test2 & Test2550);
                        AssertEquals ("#S03", (SqlInt64)0,  SqlInt64.MaxValue & SqlInt64.MinValue);

                        // | -operator
                        AssertEquals ("#S04", (SqlInt64)6,Test2 | Test4);
                        AssertEquals ("#S05", (SqlInt64)2550, Test2 | Test2550);
                        AssertEquals ("#S06", (SqlInt64)(-1), SqlInt64.MinValue | SqlInt64.MaxValue);

                        //  ^ -operator
                        AssertEquals("#S07", (SqlInt64)2546, (Test2550 ^ Test4));
                        AssertEquals("#S08", (SqlInt64)6, (Test2 ^ Test4));
                }

		[Test]
                public void ThanOrEqualOperators()
                {
                        SqlInt64 Test165 = new SqlInt64 (165);
                        SqlInt64 Test100 = new SqlInt64 (100);
                        SqlInt64 Test100II = new SqlInt64 (100);
                        SqlInt64 Test255 = new SqlInt64 (2550);

                        // == -operator
                        Assert ("#T01", (Test100 == Test100II).Value);
                        Assert ("#T02", !(Test165 == Test100).Value);
                        Assert ("#T03", (Test165 == SqlInt64.Null).IsNull);

                        // != -operator
                        Assert ("#T04", !(Test100 != Test100II).Value);
                        Assert ("#T05", (Test100 != Test255).Value);
                        Assert ("#T06", (Test165 != Test255).Value);
                        Assert ("#T07", (Test165 != SqlInt64.Null).IsNull);

                        // > -operator
                        Assert ("#T08", (Test165 > Test100).Value);
                        Assert ("#T09", !(Test165 > Test255).Value);
                        Assert ("#T10", !(Test100 > Test100II).Value);
                        Assert ("#T11", (Test165 > SqlInt64.Null).IsNull);

                        // >=  -operator
                        Assert ("#T12", !(Test165 >= Test255).Value);
                        Assert ("#T13", (Test255 >= Test165).Value);
                        Assert ("#T14", (Test100 >= Test100II).Value);
                        Assert ("#T15", (Test165 >= SqlInt64.Null).IsNull);

                        // < -operator
                        Assert ("#T16", !(Test165 < Test100).Value);
                        Assert ("#T17", (Test165 < Test255).Value);
                        Assert ("#T18", !(Test100 < Test100II).Value);
                        Assert ("#T19", (Test165 < SqlInt64.Null).IsNull);

                        // <= -operator
                        Assert ("#T20", (Test165 <= Test255).Value);
                        Assert ("#T21", !(Test255 <= Test165).Value);
                        Assert ("#T22", (Test100 <= Test100II).Value);
                        Assert ("#T23", (Test165 <= SqlInt64.Null).IsNull);
                }

		[Test]
                public void OnesComplementOperator()
                {
                        SqlInt64 Test12 = new SqlInt64 (12);
                        SqlInt64 Test128 = new SqlInt64 (128);

                        AssertEquals ("#V01", (SqlInt64)(-13), ~Test12);
                        AssertEquals ("#V02", (SqlInt64)(-129), ~Test128);
                        AssertEquals ("#V03", SqlInt64.Null, ~SqlInt64.Null);
                }

		[Test]
                public void UnaryNegation()
                {
                        SqlInt64 Test = new SqlInt64 (2000);
                        SqlInt64 TestNeg = new SqlInt64 (-3000);

                        SqlInt64 Result = -Test;
                        AssertEquals ("#W01", (long)(-2000), Result.Value);

                        Result = -TestNeg;
                        AssertEquals ("#W02", (long)3000, Result.Value);
                }

		[Test]
                public void SqlBooleanToSqlInt64()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);
                        SqlInt64 Result;

                        Result = (SqlInt64)TestBoolean;

                        AssertEquals ("#X01", (long)1, Result.Value);

                        Result = (SqlInt64)SqlBoolean.Null;
                        Assert ("#X02", Result.IsNull);
                }

		[Test]
                public void SqlDecimalToSqlInt64()
                {
                        SqlDecimal TestDecimal64 = new SqlDecimal (64);
                        SqlDecimal TestDecimal900 = new SqlDecimal (90000);

                        AssertEquals("#Y01", (long)64, ((SqlInt64)TestDecimal64).Value);
                        AssertEquals("#Y02", SqlInt64.Null, ((SqlInt64)SqlDecimal.Null));

                        try {
                                SqlInt64 test = (SqlInt64)SqlDecimal.MaxValue;
                                Fail("#Y03");
                        } catch (Exception e) {
                                AssertEquals("#Y04", typeof(OverflowException), e.GetType());
                        }
                }

		[Test]
                public void SqlDoubleToSqlInt64()
                {
                        SqlDouble TestDouble64 = new SqlDouble (64);
                        SqlDouble TestDouble900 = new SqlDouble (90000);

                        AssertEquals ("#Z01", (long)64, ((SqlInt64)TestDouble64).Value);
                        AssertEquals ("#Z02", SqlInt64.Null, ((SqlInt64)SqlDouble.Null));

                        try {
                                SqlInt64 test = (SqlInt64)SqlDouble.MaxValue;
                                Fail ("#Z03");
                        } catch (Exception e) {
                                AssertEquals("#Z04", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void Sql64IntToInt64()
                {
                        SqlInt64 Test = new SqlInt64 (12);
                        Int64 Result = (Int64)Test;
                        AssertEquals ("#AA01", (long)12, Result);
                }

		[Test]
                public void SqlInt32ToSqlInt64()
                {
                        SqlInt32 Test64 = new SqlInt32 (64);
                        AssertEquals ("#AB01", (long)64, ((SqlInt64)Test64).Value);
                }

		[Test]
                public void SqlInt16ToSqlInt64()
                {
                        SqlInt16 Test64 = new SqlInt16 (64);
                        AssertEquals ("#AC01", (long)64, ((SqlInt64)Test64).Value);
                }

		[Test]
                public void SqlMoneyToSqlInt64()
                {
                        SqlMoney TestMoney64 = new SqlMoney(64);
                        AssertEquals ("#AD01", (long)64, ((SqlInt64)TestMoney64).Value);
                }

		[Test]
                public void SqlSingleToSqlInt64()
                {
                        SqlSingle TestSingle64 = new SqlSingle (64);
                        AssertEquals ("#AE01", (long)64, ((SqlInt64)TestSingle64).Value);
                }

		[Test]
                public void SqlStringToSqlInt64()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");
                        SqlString TestString1000 = new SqlString ("1000000000000000000000");

                        AssertEquals ("#AF01", (long)100, ((SqlInt64)TestString100).Value);

                        try {
                                SqlInt64 test = (SqlInt64)TestString1000;
                                Fail ("#AF02");
                        } catch(Exception e) {
                                AssertEquals ("#AF03", typeof (OverflowException), e.GetType ());
                        }

                        try {
                                SqlInt64 test = (SqlInt64)TestString;
                                Fail ("#AF03");
                        } catch(Exception e) {
                                AssertEquals ("#AF04", typeof (FormatException), e.GetType ());
                        }
                }

		[Test]
                public void ByteToSqlInt64()
                {
                        short TestShort = 14;
                        AssertEquals ("#G01", (long)14, ((SqlInt64)TestShort).Value);
                }
        }
}

