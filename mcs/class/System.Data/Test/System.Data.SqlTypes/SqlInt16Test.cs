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

using NUnit.Framework;
using System;
using System.Data.SqlTypes;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlInt16Test : Assertion {

                // Test constructor
		[Test]
                public void Create()
                {
                        SqlInt16 TestShort = new SqlInt16 (29);
                        AssertEquals ("Test#1", (short)29, TestShort.Value);

                        TestShort = new SqlInt16 (-9000);
                        AssertEquals ("Test#2", (short)-9000, TestShort.Value);
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        AssertEquals ("Test#1", (SqlInt16)32767, SqlInt16.MaxValue);
                        AssertEquals ("Test#2", (SqlInt16)(-32768), SqlInt16.MinValue);
                        Assert ("Test#3", SqlInt16.Null.IsNull);
                        AssertEquals ("Test#4", (short)0, SqlInt16.Zero.Value);
                }

                // Test properties
		[Test]
                public void Properties()
                {
                        SqlInt16 Test5443 = new SqlInt16 (5443);
                        SqlInt16 Test1 = new SqlInt16 (1);
                        Assert ("Test#1", SqlInt16.Null.IsNull);
                        AssertEquals ("Test#2", (short)5443, Test5443.Value);
                        AssertEquals ("Test#3", (short)1, Test1.Value);
                }

                // PUBLIC METHODS

		[Test]
                public void ArithmeticMethods()
                {
                        SqlInt16 Test64 = new SqlInt16 (64);
                        SqlInt16 Test0 = new SqlInt16 (0);
                        SqlInt16 Test164 = new SqlInt16 (164);
                        SqlInt16 TestMax = new SqlInt16 (SqlInt16.MaxValue.Value);

                        // Add()
                        AssertEquals ("Test#1", (short)64, SqlInt16.Add (Test64, Test0).Value);
                        AssertEquals ("Test#2", (short)228, SqlInt16.Add (Test64, Test164).Value);
                        AssertEquals ("Test#3", (short)164, SqlInt16.Add (Test0, Test164).Value);
                        AssertEquals ("Test#4", (short)SqlInt16.MaxValue, SqlInt16.Add (TestMax, Test0).Value);

                        try {
                                SqlInt16.Add (TestMax, Test64);
                                Fail ("Test#5");
                        } catch (Exception e) {
                                AssertEquals ("Test#6", typeof (OverflowException), e.GetType ());
                        }

                        // Divide()
                        AssertEquals ("Test#7", (short)2, SqlInt16.Divide (Test164, Test64).Value);
                        AssertEquals ("Test#8", (short)0, SqlInt16.Divide (Test64, Test164).Value);
                        try {
                                SqlInt16.Divide(Test64, Test0);
                                Fail ("Test#9");
                        } catch(Exception e) {
                                AssertEquals ("Test#10", typeof (DivideByZeroException), e.GetType ());
                        }

                        // Mod()
                        AssertEquals ("Test#11", (SqlInt16)36, SqlInt16.Mod (Test164, Test64));
                        AssertEquals ("Test#12",  (SqlInt16)64, SqlInt16.Mod (Test64, Test164));

                        // Multiply()
                        AssertEquals ("Test#13", (short)10496, SqlInt16.Multiply (Test64, Test164).Value);
                        AssertEquals ("Test#14", (short)0, SqlInt16.Multiply (Test64, Test0).Value);

                        try {
                                SqlInt16.Multiply (TestMax, Test64);
                                Fail ("Test#15");
                        } catch(Exception e) {
                                AssertEquals ("Test#16", typeof (OverflowException), e.GetType ());
                        }

                        // Subtract()
                        AssertEquals ("Test#17", (short)100, SqlInt16.Subtract (Test164, Test64).Value);

                        try {
                                SqlInt16.Subtract (SqlInt16.MinValue, Test164);
                                Fail("Test#18");
                        } catch(Exception e) {
                                AssertEquals ("Test#19", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void BitwiseMethods()
                {
                        short MaxValue = SqlInt16.MaxValue.Value;
                        SqlInt16 TestInt = new SqlInt16 (0);
                        SqlInt16 TestIntMax = new SqlInt16 (MaxValue);
                        SqlInt16 TestInt2 = new SqlInt16 (10922);
                        SqlInt16 TestInt3 = new SqlInt16 (21845);

                        // BitwiseAnd
                        AssertEquals ("Test#1", (short)21845, SqlInt16.BitwiseAnd (TestInt3, TestIntMax).Value);
                        AssertEquals ("Test#2", (short)0, SqlInt16.BitwiseAnd (TestInt2, TestInt3).Value);
                        AssertEquals ("Test#3", (short)10922, SqlInt16.BitwiseAnd (TestInt2, TestIntMax).Value);

                        //BitwiseOr
                        AssertEquals ("Test#4", (short)MaxValue, SqlInt16.BitwiseOr (TestInt2, TestInt3).Value);
                        AssertEquals ("Test#5", (short)21845, SqlInt16.BitwiseOr (TestInt, TestInt3).Value);
                        AssertEquals ("Test#6", (short)MaxValue, SqlInt16.BitwiseOr (TestIntMax, TestInt2).Value);
                }

		[Test]
                public void CompareTo()
                {
                        SqlInt16 TestInt4000 = new SqlInt16 (4000);
                        SqlInt16 TestInt4000II = new SqlInt16 (4000);
                        SqlInt16 TestInt10 = new SqlInt16 (10);
                        SqlInt16 TestInt10000 = new SqlInt16 (10000);
                        SqlString TestString = new SqlString ("This is a test");

                        Assert ("Test#1", TestInt4000.CompareTo (TestInt10) > 0);
                        Assert ("Test#2", TestInt10.CompareTo (TestInt4000) < 0);
                        Assert ("Test#3", TestInt4000II.CompareTo (TestInt4000) == 0);
                        Assert ("Test#4", TestInt4000II.CompareTo (SqlInt16.Null) > 0);

                        try {
                                TestInt10.CompareTo (TestString);
                                Fail ("Test#5");
                        } catch(Exception e) {
                                AssertEquals ("Test#6", typeof (ArgumentException), e.GetType ());
                        }
                }

		[Test]
                public void EqualsMethod()
                {
                        SqlInt16 Test0 = new SqlInt16 (0);
                        SqlInt16 Test158 = new SqlInt16 (158);
                        SqlInt16 Test180 = new SqlInt16 (180);
                        SqlInt16 Test180II = new SqlInt16 (180);

                        Assert ("Test#1", !Test0.Equals (Test158));
                        Assert ("Test#2", !Test158.Equals (Test180));
                        Assert ("Test#3", !Test180.Equals (new SqlString ("TEST")));
                        Assert ("Test#4", Test180.Equals (Test180II));
                }

		[Test]
                public void StaticEqualsMethod()
                {
                        SqlInt16 Test34 = new SqlInt16 (34);
                        SqlInt16 Test34II = new SqlInt16 (34);
                        SqlInt16 Test15 = new SqlInt16 (15);

                        Assert ("Test#1", SqlInt16.Equals (Test34, Test34II).Value);
                        Assert ("Test#2", !SqlInt16.Equals (Test34, Test15).Value);
                        Assert ("Test#3", !SqlInt16.Equals (Test15, Test34II).Value);
                }

		[Test]
                public void GetHashCodeTest()
                {
                        SqlInt16 Test15 = new SqlInt16 (15);

                        // FIXME: Better way to test GetHashCode()-methods
                        AssertEquals ("Test#1", Test15.GetHashCode (), Test15.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
                        SqlInt16 Test = new SqlInt16 (84);
                        AssertEquals ("Test#1", "System.Data.SqlTypes.SqlInt16", Test.GetType ().ToString ());
                }

		[Test]
                public void Greaters()
                {
                        SqlInt16 Test10 = new SqlInt16 (10);
                        SqlInt16 Test10II = new SqlInt16 (10);
                        SqlInt16 Test110 = new SqlInt16 (110);

                        // GreateThan ()
                        Assert ("Test#1", !SqlInt16.GreaterThan (Test10, Test110).Value);
                        Assert ("Test#2", SqlInt16.GreaterThan (Test110, Test10).Value);
                        Assert ("Test#3", !SqlInt16.GreaterThan (Test10II, Test10).Value);

                        // GreaterTharOrEqual ()
                        Assert ("Test#4", !SqlInt16.GreaterThanOrEqual (Test10, Test110).Value);
                        Assert ("Test#5", SqlInt16.GreaterThanOrEqual (Test110, Test10).Value);
                        Assert ("Test#6", SqlInt16.GreaterThanOrEqual (Test10II, Test10).Value);
                }

		[Test]
                public void Lessers()
                {
                        SqlInt16 Test10 = new SqlInt16 (10);
                        SqlInt16 Test10II = new SqlInt16 (10);
                        SqlInt16 Test110 = new SqlInt16 (110);

                        // LessThan()
                        Assert ("Test#1", SqlInt16.LessThan (Test10, Test110).Value);
                        Assert ("Test#2", !SqlInt16.LessThan (Test110, Test10).Value);
                        Assert ("Test#3", !SqlInt16.LessThan (Test10II, Test10).Value);

                        // LessThanOrEqual ()
                        Assert ("Test#4", SqlInt16.LessThanOrEqual (Test10, Test110).Value);
                        Assert ("Test#5", !SqlInt16.LessThanOrEqual (Test110, Test10).Value);
                        Assert ("Test#6", SqlInt16.LessThanOrEqual (Test10II, Test10).Value);
                        Assert ("Test#7", SqlInt16.LessThanOrEqual (Test10II, SqlInt16.Null).IsNull);
                }

		[Test]
                public void NotEquals()
                {
                        SqlInt16 Test12 = new SqlInt16 (12);
                        SqlInt16 Test128 = new SqlInt16 (128);
                        SqlInt16 Test128II = new SqlInt16 (128);

                        Assert ("Test#1", SqlInt16.NotEquals (Test12, Test128).Value);
                        Assert ("Test#2", SqlInt16.NotEquals (Test128, Test12).Value);
                        Assert ("Test#3", SqlInt16.NotEquals (Test128II, Test12).Value);
                        Assert ("Test#4", !SqlInt16.NotEquals (Test128II, Test128).Value);
                        Assert ("Test#5", !SqlInt16.NotEquals (Test128, Test128II).Value);
                        Assert ("Test#6", SqlInt16.NotEquals (SqlInt16.Null, Test128II).IsNull);
                        Assert ("Test#7", SqlInt16.NotEquals (SqlInt16.Null, Test128II).IsNull);
                }

		[Test]
                public void OnesComplement()
                {
                        SqlInt16 Test12 = new SqlInt16(12);
                        SqlInt16 Test128 = new SqlInt16(128);

                        AssertEquals ("Test#1", (SqlInt16)(-13), SqlInt16.OnesComplement (Test12));
                        AssertEquals ("Test#2", (SqlInt16)(-129), SqlInt16.OnesComplement (Test128));
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlInt16.Parse (null);
                                Fail ("Test#1");
                        } catch (Exception e) {
                                AssertEquals ("Test#2", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlInt16.Parse ("not-a-number");
                                Fail ("Test#3");
                        } catch (Exception e) {
                                AssertEquals ("Test#4", typeof (FormatException), e.GetType ());
                        }

                        try {
                                int OverInt = (int)SqlInt16.MaxValue + 1;
                                SqlInt16.Parse (OverInt.ToString ());
                                Fail ("Test#5");
                        } catch (Exception e) {
                                AssertEquals ("Test#6", typeof (OverflowException), e.GetType ());
                        }

                        AssertEquals("Test#7", (short)150, SqlInt16.Parse ("150").Value);
                }

		[Test]
                public void Conversions()
                {
                        SqlInt16 Test12 = new SqlInt16 (12);
                        SqlInt16 Test0 = new SqlInt16 (0);
                        SqlInt16 TestNull = SqlInt16.Null;
                        SqlInt16 Test1000 = new SqlInt16 (1000);
                        SqlInt16 Test288 = new SqlInt16(288);

                        // ToSqlBoolean ()
                        Assert ("TestA#1", Test12.ToSqlBoolean ().Value);
                        Assert ("TestA#2", !Test0.ToSqlBoolean ().Value);
                        Assert ("TestA#3", TestNull.ToSqlBoolean ().IsNull);

                        // ToSqlByte ()
                        AssertEquals ("TestB#1", (byte)12, Test12.ToSqlByte ().Value);
                        AssertEquals ("TestB#2", (byte)0, Test0.ToSqlByte ().Value);

                        try {
                                SqlByte b = (byte)Test1000.ToSqlByte ();
                                Fail ("TestB#4");
                        } catch (Exception e) {
                                AssertEquals ("TestB#5", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlDecimal ()
                        AssertEquals ("TestC#1", (decimal)12, Test12.ToSqlDecimal ().Value);
                        AssertEquals ("TestC#2", (decimal)0, Test0.ToSqlDecimal ().Value);
                        AssertEquals ("TestC#3", (decimal)288, Test288.ToSqlDecimal ().Value);

                        // ToSqlDouble ()
                        AssertEquals ("TestD#1", (double)12, Test12.ToSqlDouble ().Value);
                        AssertEquals ("TestD#2", (double)0, Test0.ToSqlDouble ().Value);
                        AssertEquals ("TestD#3", (double)1000, Test1000.ToSqlDouble ().Value);

                        // ToSqlInt32 ()
                        AssertEquals ("TestE#1", (int)12, Test12.ToSqlInt32 ().Value);
                        AssertEquals ("TestE#2", (int)0, Test0.ToSqlInt32 ().Value);
                        AssertEquals ("TestE#3", (int)288, Test288.ToSqlInt32().Value);

                        // ToSqlInt64 ()
                        AssertEquals ("TestF#1", (long)12, Test12.ToSqlInt64 ().Value);
                        AssertEquals ("TestF#2", (long)0, Test0.ToSqlInt64 ().Value);
                        AssertEquals ("TestF#3", (long)288, Test288.ToSqlInt64 ().Value);

                        // ToSqlMoney ()
                        AssertEquals ("TestG#1", (decimal)12, Test12.ToSqlMoney ().Value);
                        AssertEquals ("TestG#2", (decimal)0, Test0.ToSqlMoney ().Value);
                        AssertEquals ("TestG#3", (decimal)288, Test288.ToSqlMoney ().Value);

                        // ToSqlSingle ()
                        AssertEquals ("TestH#1", (float)12, Test12.ToSqlSingle ().Value);
                        AssertEquals ("TestH#2", (float)0, Test0.ToSqlSingle ().Value);
                        AssertEquals ("TestH#3", (float)288, Test288.ToSqlSingle().Value);

                        // ToSqlString ()
                        AssertEquals ("TestI#1", "12", Test12.ToSqlString ().Value);
                        AssertEquals ("TestI#2", "0", Test0.ToSqlString ().Value);
                        AssertEquals ("TestI#3", "288", Test288.ToSqlString ().Value);

                        // ToString ()
                        AssertEquals ("TestJ#1", "12", Test12.ToString ());
                        AssertEquals ("TestJ#2", "0", Test0.ToString ());
                        AssertEquals ("TestJ#3", "288", Test288.ToString ());
                }

		[Test]
                public void Xor()
                {
                        SqlInt16 Test14 = new SqlInt16 (14);
                        SqlInt16 Test58 = new SqlInt16 (58);
                        SqlInt16 Test130 = new SqlInt16 (130);
                        SqlInt16 TestMax = new SqlInt16 (SqlInt16.MaxValue.Value);
                        SqlInt16 Test0 = new SqlInt16 (0);

                        AssertEquals ("Test#1", (short)52, SqlInt16.Xor (Test14, Test58).Value);
                        AssertEquals ("Test#2", (short)140, SqlInt16.Xor (Test14, Test130).Value);
                        AssertEquals ("Test#3", (short)184, SqlInt16.Xor (Test58, Test130).Value);
                        AssertEquals ("Test#4", (short)0, SqlInt16.Xor (TestMax, TestMax).Value);
                        AssertEquals ("Test#5", TestMax.Value, SqlInt16.Xor (TestMax, Test0).Value);
                }

                // OPERATORS

		[Test]
                public void ArithmeticOperators()
                {
                        SqlInt16 Test24 = new SqlInt16 (24);
                        SqlInt16 Test64 = new SqlInt16 (64);
                        SqlInt16 Test2550 = new SqlInt16 (2550);
                        SqlInt16 Test0 = new SqlInt16 (0);

                        // "+"-operator
                        AssertEquals ("TestA#1", (SqlInt16)2614,Test2550 + Test64);
                        try {
                                SqlInt16 result = Test64 + SqlInt16.MaxValue;
                                Fail ("TestA#2");
                        } catch (Exception e) {
                                AssertEquals ("TestA#3", typeof (OverflowException), e.GetType ());
                        }

                        // "/"-operator
                        AssertEquals ("TestB#1", (SqlInt16)39, Test2550 / Test64);
                        AssertEquals ("TestB#2", (SqlInt16)0, Test24 / Test64);

                        try {
                                SqlInt16 result = Test2550 / Test0;
                                Fail ("TestB#3");
                        } catch (Exception e) {
                                AssertEquals ("TestB#4", typeof (DivideByZeroException), e.GetType ());
                        }

                        // "*"-operator
                        AssertEquals ("TestC#1", (SqlInt16)1536, Test64 * Test24);

                        try {
                                SqlInt16 test = (SqlInt16.MaxValue * Test64);
                                Fail ("TestC#2");
                        } catch (Exception e) {
                                AssertEquals ("TestC#3", typeof (OverflowException), e.GetType ());
                        }

                        // "-"-operator
                        AssertEquals ("TestD#1", (SqlInt16)2526, Test2550 - Test24);

                        try {
                                SqlInt16 test = SqlInt16.MinValue - Test64;
                                Fail ("TestD#2");
                        } catch (Exception e) {
                                AssertEquals ("OverflowException", typeof (OverflowException), e.GetType ());
                        }

                        // "%"-operator
                        AssertEquals ("TestE#1", (SqlInt16)54, Test2550 % Test64);
                        AssertEquals ("TestE#2", (SqlInt16)24, Test24 % Test64);
                        AssertEquals ("TestE#1", (SqlInt16)0, new SqlInt16 (100) % new SqlInt16 (10));
                }

		[Test]
                public void BitwiseOperators()
                {
                        SqlInt16 Test2 = new SqlInt16 (2);
                        SqlInt16 Test4 = new SqlInt16 (4);
                        SqlInt16 Test2550 = new SqlInt16 (2550);

                        // & -operator
                        AssertEquals ("TestA#1", (SqlInt16)0, Test2 & Test4);
                        AssertEquals ("TestA#2", (SqlInt16)2, Test2 & Test2550);
                        AssertEquals ("TestA#3", (SqlInt16)0,  SqlInt16.MaxValue & SqlInt16.MinValue);

                        // | -operator
                        AssertEquals ("TestB#1", (SqlInt16)6,Test2 | Test4);
                        AssertEquals ("TestB#2", (SqlInt16)2550, Test2 | Test2550);
                        AssertEquals ("TestB#3", (SqlInt16)(-1), SqlInt16.MinValue | SqlInt16.MaxValue);

                        //  ^ -operator
                        AssertEquals("TestC#1", (SqlInt16)2546, (Test2550 ^ Test4));
                        AssertEquals("TestC#2", (SqlInt16)6, (Test2 ^ Test4));
                }

		[Test]
                public void ThanOrEqualOperators()
                {
                        SqlInt16 Test165 = new SqlInt16 (165);
                        SqlInt16 Test100 = new SqlInt16 (100);
                        SqlInt16 Test100II = new SqlInt16 (100);
                        SqlInt16 Test255 = new SqlInt16 (2550);

                        // == -operator
                        Assert ("TestA#1", (Test100 == Test100II).Value);
                        Assert ("TestA#2", !(Test165 == Test100).Value);
                        Assert ("TestA#3", (Test165 == SqlInt16.Null).IsNull);

                        // != -operator
                        Assert ("TestB#1", !(Test100 != Test100II).Value);
                        Assert ("TestB#2", (Test100 != Test255).Value);
                        Assert ("TestB#3", (Test165 != Test255).Value);
                        Assert ("TestB#4", (Test165 != SqlInt16.Null).IsNull);

                        // > -operator
                        Assert ("TestC#1", (Test165 > Test100).Value);
                        Assert ("TestC#2", !(Test165 > Test255).Value);
                        Assert ("TestC#3", !(Test100 > Test100II).Value);
                        Assert ("TestC#4", (Test165 > SqlInt16.Null).IsNull);

                        // >=  -operator
                        Assert ("TestD#1", !(Test165 >= Test255).Value);
                        Assert ("TestD#2", (Test255 >= Test165).Value);
                        Assert ("TestD#3", (Test100 >= Test100II).Value);
                        Assert ("TestD#4", (Test165 >= SqlInt16.Null).IsNull);

                        // < -operator
                        Assert ("TestE#1", !(Test165 < Test100).Value);
                        Assert ("TestE#2", (Test165 < Test255).Value);
                        Assert ("TestE#3", !(Test100 < Test100II).Value);
                        Assert ("TestE#4", (Test165 < SqlInt16.Null).IsNull);

                        // <= -operator
                        Assert ("TestF#1", (Test165 <= Test255).Value);
                        Assert ("TestF#2", !(Test255 <= Test165).Value);
                        Assert ("TestF#3", (Test100 <= Test100II).Value);
                        Assert ("TestF#4", (Test165 <= SqlInt16.Null).IsNull);
                }

		[Test]
                public void OnesComplementOperator()
                {
                        SqlInt16 Test12 = new SqlInt16 (12);
                        SqlInt16 Test128 = new SqlInt16 (128);

                        AssertEquals ("Test#1", (SqlInt16)(-13), ~Test12);
                        AssertEquals ("Test#2", (SqlInt16)(-129), ~Test128);
                        AssertEquals ("Test#3", SqlInt16.Null, ~SqlInt16.Null);
                }

		[Test]
                public void UnaryNegation()
                {
                        SqlInt16 Test = new SqlInt16 (2000);
                        SqlInt16 TestNeg = new SqlInt16 (-3000);

                        SqlInt16 Result = -Test;
                        AssertEquals ("Test#1", (short)(-2000), Result.Value);

                        Result = -TestNeg;
                        AssertEquals ("Test#2", (short)3000, Result.Value);
                }

		[Test]
                public void SqlBooleanToSqlInt16()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);
                        SqlInt16 Result;

                        Result = (SqlInt16)TestBoolean;

                        AssertEquals ("Test#1", (short)1, Result.Value);

                        Result = (SqlInt16)SqlBoolean.Null;
                        Assert ("Test#2", Result.IsNull);
                }

		[Test]
                public void SqlDecimalToSqlInt16()
                {
                        SqlDecimal TestDecimal64 = new SqlDecimal (64);
                        SqlDecimal TestDecimal900 = new SqlDecimal (90000);

                        AssertEquals ("Test#1", (short)64, ((SqlInt16)TestDecimal64).Value);
                        AssertEquals ("Test#2", SqlInt16.Null, ((SqlInt16)SqlDecimal.Null));

                        try {
                                SqlInt16 test = (SqlInt16)TestDecimal900;
                                Fail ("Test#3");
                        } catch (Exception e) {
                                AssertEquals("Test#4", typeof(OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void SqlDoubleToSqlInt16()
                {
                        SqlDouble TestDouble64 = new SqlDouble (64);
                        SqlDouble TestDouble900 = new SqlDouble (90000);

                        AssertEquals ("Test#1", (short)64, ((SqlInt16)TestDouble64).Value);
                        AssertEquals ("Test#2", SqlInt16.Null, ((SqlInt16)SqlDouble.Null));

                        try {
                                SqlInt16 test = (SqlInt16)TestDouble900;
                                Fail ("Test#3");
                        } catch (Exception e) {
                                AssertEquals("Test#4", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void SqlIntToInt16()
                {
                        SqlInt16 Test = new SqlInt16(12);
                        Int16 Result = (Int16)Test;
                        AssertEquals("Test#1", (short)12, Result);
                }

		[Test]
                public void SqlInt32ToSqlInt16()
                {
                        SqlInt32 Test64 = new SqlInt32 (64);
                        SqlInt32 Test900 = new SqlInt32 (90000);

                        AssertEquals ("Test#1", (short)64, ((SqlInt16)Test64).Value);

                        try {
                                SqlInt16 test = (SqlInt16)Test900;
                                Fail ("Test#2");
                        } catch (Exception e) {
                                AssertEquals ("Test#3", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void SqlInt64ToSqlInt16()
                {
                        SqlInt64 Test64 = new SqlInt64 (64);
                        SqlInt64 Test900 = new SqlInt64 (90000);

                        AssertEquals ("Test#1", (short)64, ((SqlInt16)Test64).Value);

                        try {
                                SqlInt16 test = (SqlInt16)Test900;
                                Fail ("Test#2");
                        } catch (Exception e) {
                                AssertEquals("Test#3", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void SqlMoneyToSqlInt16()
                {
                        SqlMoney TestMoney64 = new SqlMoney(64);
                        SqlMoney TestMoney900 = new SqlMoney(90000);

                        AssertEquals ("Test#1", (short)64, ((SqlInt16)TestMoney64).Value);

                        try {
                                SqlInt16 test = (SqlInt16)TestMoney900;
                                Fail ("Test#2");
                        } catch (Exception e) {
                                AssertEquals("test#3", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void SqlSingleToSqlInt16()
                {
                        SqlSingle TestSingle64 = new SqlSingle(64);
                        SqlSingle TestSingle900 = new SqlSingle(90000);

                        AssertEquals("Test#1", (short)64, ((SqlInt16)TestSingle64).Value);

                        try {
                                SqlInt16 test = (SqlInt16)TestSingle900;
                                Fail ("Test#2");
                        } catch (Exception e) {
                                AssertEquals ("Test#3", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void SqlStringToSqlInt16()
                {
                        SqlString TestString = new SqlString("Test string");
                        SqlString TestString100 = new SqlString("100");
                        SqlString TestString1000 = new SqlString("100000");

                        AssertEquals ("Test#1", (short)100, ((SqlInt16)TestString100).Value);

                        try {
                                SqlInt16 test = (SqlInt16)TestString1000;
                                Fail ("Test#2");
                        } catch(Exception e) {
                                AssertEquals ("Test#3", typeof (OverflowException), e.GetType ());
                        }

                        try {
                                SqlInt16 test = (SqlInt16)TestString;
                                Fail ("Test#3");
                        } catch(Exception e) {
                                AssertEquals ("Test#4", typeof (FormatException), e.GetType ());
                        }
                }

		[Test]
                public void ByteToSqlInt16()
                {
                        short TestShort = 14;
                        AssertEquals ("Test#1", (short)14, ((SqlInt16)TestShort).Value);
                }
        }
}

