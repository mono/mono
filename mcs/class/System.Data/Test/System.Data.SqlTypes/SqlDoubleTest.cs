//
// SqlDoubleTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlDouble
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
        public class SqlDoubleTest {

                // Test constructor
		[Test]
                public void Create()
                {
                        SqlDouble Test= new SqlDouble ((double)34.87);
                        Assertion.AssertEquals ("#A01", 34.87D, Test.Value);

                        Test = new SqlDouble (-9000.6543);
                        Assertion.AssertEquals ("#A02", -9000.6543D, Test.Value);
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        Assertion.AssertEquals ("#B01", 1.7976931348623157e+308, SqlDouble.MaxValue.Value);
                        Assertion.AssertEquals ("#B02", -1.7976931348623157e+308, SqlDouble.MinValue.Value);
                        Assertion.Assert ("#B03", SqlDouble.Null.IsNull);
                        Assertion.AssertEquals ("#B04", 0d, SqlDouble.Zero.Value);
                }

                // Test properties
		[Test]
                public void Properties()
                {
                        SqlDouble Test5443 = new SqlDouble (5443e12);
                        SqlDouble Test1 = new SqlDouble (1);

                        Assertion.Assert ("#C01", SqlDouble.Null.IsNull);
                        Assertion.AssertEquals ("#C02", 5443e12, Test5443.Value);
                        Assertion.AssertEquals ("#C03", (double)1, Test1.Value);
                }

                // PUBLIC METHODS

		[Test]
                public void ArithmeticMethods()
                {
                        SqlDouble Test0 = new SqlDouble (0);
                        SqlDouble Test1 = new SqlDouble (15E+108);
                        SqlDouble Test2 = new SqlDouble (-65E+64);
                        SqlDouble Test3 = new SqlDouble (5E+64);
                        SqlDouble Test4 = new SqlDouble (5E+108);
                        SqlDouble TestMax = new SqlDouble (SqlDouble.MaxValue.Value);

                        // Add()
                        Assertion.AssertEquals ("#D01A", 15E+108, SqlDouble.Add (Test1, Test0).Value);
                        Assertion.AssertEquals ("#D02A", 1.5E+109, SqlDouble.Add (Test1, Test2).Value);

                        try {
                                SqlDouble test = SqlDouble.Add (SqlDouble.MaxValue, SqlDouble.MaxValue);
                                Assertion.Fail ("#D03A");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#D04A", typeof (OverflowException), e.GetType ());
                        }
                        
                        // Divide()
                        Assertion.AssertEquals ("#D01B", (SqlDouble)3, SqlDouble.Divide (Test1, Test4));
                        Assertion.AssertEquals ("#D02B", -13d, SqlDouble.Divide (Test2, Test3).Value);

                        try {
                                SqlDouble test = SqlDouble.Divide(Test1, Test0).Value;
                                Assertion.Fail ("#D03B");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#D04B", typeof (DivideByZeroException), e.GetType ());
                        }

                        // Multiply()
                        Assertion.AssertEquals ("#D01D", (double)(75E+216), SqlDouble.Multiply (Test1, Test4).Value);
                        Assertion.AssertEquals ("#D02D", (double)0, SqlDouble.Multiply (Test1, Test0).Value);

                        try {
                                SqlDouble test = SqlDouble.Multiply (TestMax, Test1);
                                Assertion.Fail ("#D03D");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#D04D", typeof (OverflowException), e.GetType ());
                        }
                                

                        // Subtract()
                        Assertion.AssertEquals ("#D01F", (double)1.5E+109, SqlDouble.Subtract (Test1, Test3).Value);

                        try {
                                SqlDouble test = SqlDouble.Subtract(SqlDouble.MinValue, SqlDouble.MaxValue);
                                Assertion.Fail ("D02F");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#D03F", typeof (OverflowException), e.GetType ());
                        }                                
                }

		[Test]
                public void CompareTo()
                {
                        SqlDouble Test1 = new SqlDouble (4e64);
                        SqlDouble Test11 = new SqlDouble (4e64);
                        SqlDouble Test2 = new SqlDouble (-9e34);
                        SqlDouble Test3 = new SqlDouble (10000);
                        SqlString TestString = new SqlString ("This is a test");

                        Assertion.Assert ("#E01", Test1.CompareTo (Test3) > 0);
                        Assertion.Assert ("#E02", Test2.CompareTo (Test3) < 0);
                        Assertion.Assert ("#E03", Test1.CompareTo (Test11) == 0);
                        Assertion.Assert ("#E04", Test11.CompareTo (SqlDouble.Null) > 0);

                        try {
                                Test1.CompareTo (TestString);
                                Assertion.Fail("#E05");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#E06", typeof (ArgumentException), e.GetType ());
                        }
                }

		[Test]
                public void EqualsMethods()
                {
                        SqlDouble Test0 = new SqlDouble (0);
                        SqlDouble Test1 = new SqlDouble (1.58e30);
                        SqlDouble Test2 = new SqlDouble (1.8e180);
                        SqlDouble Test22 = new SqlDouble (1.8e180);

                        Assertion.Assert ("#F01", !Test0.Equals (Test1));
                        Assertion.Assert ("#F02", !Test1.Equals (Test2));
                        Assertion.Assert ("#F03", !Test2.Equals (new SqlString ("TEST")));
                        Assertion.Assert ("#F04", Test2.Equals (Test22));

                        // Static Equals()-method
                        Assertion.Assert ("#F05", SqlDouble.Equals (Test2, Test22).Value);
                        Assertion.Assert ("#F06", !SqlDouble.Equals (Test1, Test2).Value);
                }

		[Test]
                public void GetHashCodeTest()
                {
                        SqlDouble Test15 = new SqlDouble (15);

                        // FIXME: Better way to test HashCode
                        Assertion.AssertEquals ("#G01", Test15.GetHashCode (), Test15.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
                        SqlDouble Test = new SqlDouble (84);
                        Assertion.AssertEquals ("#H01", "System.Data.SqlTypes.SqlDouble", Test.GetType ().ToString ());
                        Assertion.AssertEquals ("#H02", "System.Double", Test.Value.GetType ().ToString ());
                }

		[Test]
                public void Greaters()
                {
                        SqlDouble Test1 = new SqlDouble (1e100);
                        SqlDouble Test11 = new SqlDouble (1e100);
                        SqlDouble Test2 = new SqlDouble (64e164);

                        // GreateThan ()
                        Assertion.Assert ("#I01", !SqlDouble.GreaterThan (Test1, Test2).Value);
                        Assertion.Assert ("#I02", SqlDouble.GreaterThan (Test2, Test1).Value);
                        Assertion.Assert ("#I03", !SqlDouble.GreaterThan (Test1, Test11).Value);

                        // GreaterTharOrEqual ()
                        Assertion.Assert ("#I04", !SqlDouble.GreaterThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#I05", SqlDouble.GreaterThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#I06", SqlDouble.GreaterThanOrEqual (Test1, Test11).Value);
                }

		[Test]
                public void Lessers()
                {
                        SqlDouble Test1 = new SqlDouble (1.8e100);
                        SqlDouble Test11 = new SqlDouble (1.8e100);
                        SqlDouble Test2 = new SqlDouble (64e164);

                        // LessThan()
                        Assertion.Assert ("#J01", !SqlDouble.LessThan (Test1, Test11).Value);
                        Assertion.Assert ("#J02", !SqlDouble.LessThan (Test2, Test1).Value);
                        Assertion.Assert ("#J03", SqlDouble.LessThan (Test11, Test2).Value);

                        // LessThanOrEqual ()
                        Assertion.Assert ("#J04", SqlDouble.LessThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#J05", !SqlDouble.LessThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#J06", SqlDouble.LessThanOrEqual (Test11, Test1).Value);
                        Assertion.Assert ("#J07", SqlDouble.LessThanOrEqual (Test11, SqlDouble.Null).IsNull);
                }

		[Test]
                public void NotEquals()
                {
                        SqlDouble Test1 = new SqlDouble (1280000000001);
                        SqlDouble Test2 = new SqlDouble (128e10);
                        SqlDouble Test22 = new SqlDouble (128e10);

                        Assertion.Assert ("#K01", SqlDouble.NotEquals (Test1, Test2).Value);
                        Assertion.Assert ("#K02", SqlDouble.NotEquals (Test2, Test1).Value);
                        Assertion.Assert ("#K03", SqlDouble.NotEquals (Test22, Test1).Value);
                        Assertion.Assert ("#K04", !SqlDouble.NotEquals (Test22, Test2).Value);
                        Assertion.Assert ("#K05", !SqlDouble.NotEquals (Test2, Test22).Value);
                        Assertion.Assert ("#K06", SqlDouble.NotEquals (SqlDouble.Null, Test22).IsNull);
                        Assertion.Assert ("#K07", SqlDouble.NotEquals (SqlDouble.Null, Test22).IsNull);
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlDouble.Parse (null);
                                Assertion.Fail ("#L01");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#L02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlDouble.Parse ("not-a-number");
                                Assertion.Fail ("#L03");
                        } catch (Exception e) {

                                Assertion.AssertEquals ("#L04", typeof (FormatException), e.GetType ());
                        }

                         try {
                                SqlDouble.Parse ("9e400");
                                Assertion.Fail ("#L05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#L06", typeof (OverflowException), e.GetType ());
                        }

                        Assertion.AssertEquals("#L07", (double)150, SqlDouble.Parse ("150").Value);
                }

		[Test]
                public void Conversions()
                {
                        SqlDouble Test0 = new SqlDouble (0);
                        SqlDouble Test1 = new SqlDouble (250);
                        SqlDouble Test2 = new SqlDouble (64e64);
                        SqlDouble Test3 = new SqlDouble (64e164);
                        SqlDouble TestNull = SqlDouble.Null;

                        // ToSqlBoolean ()
                        Assertion.Assert ("#M01A", Test1.ToSqlBoolean ().Value);
                        Assertion.Assert ("#M02A", !Test0.ToSqlBoolean ().Value);
                        Assertion.Assert ("#M03A", TestNull.ToSqlBoolean ().IsNull);

                        // ToSqlByte ()
                        Assertion.AssertEquals ("#M01B", (byte)250, Test1.ToSqlByte ().Value);
                        Assertion.AssertEquals ("#M02B", (byte)0, Test0.ToSqlByte ().Value);

                        try {
                                SqlByte b = (byte)Test2.ToSqlByte ();
                                Assertion.Fail ("#M03B");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M04B", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlDecimal ()
                        Assertion.AssertEquals ("#M01C", (decimal)250, Test1.ToSqlDecimal ().Value);
                        Assertion.AssertEquals ("#M02C", (decimal)0, Test0.ToSqlDecimal ().Value);

                        try {
                                SqlDecimal test = Test3.ToSqlDecimal ().Value;
                                Assertion.Fail ("#M03C");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M04C", typeof (OverflowException), e.GetType ());
                        }      

                        // ToSqlInt16 ()
                        Assertion.AssertEquals ("#M01D", (short)250, Test1.ToSqlInt16 ().Value);
                        Assertion.AssertEquals ("#M02D", (short)0, Test0.ToSqlInt16 ().Value);

                        try {
                                SqlInt16 test = Test2.ToSqlInt16().Value;
                                Assertion.Fail ("#M03D");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M04D", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlInt32 ()
                        Assertion.AssertEquals ("#M01E", (int)250, Test1.ToSqlInt32 ().Value);
                        Assertion.AssertEquals ("#M02E", (int)0, Test0.ToSqlInt32 ().Value);

                        try {
                                SqlInt32 test = Test2.ToSqlInt32 ().Value;
                                Assertion.Fail ("#M03E");
                        } catch (Exception e) { 
                                Assertion.AssertEquals ("#M04E", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlInt64 ()
                        Assertion.AssertEquals ("#M01F", (long)250, Test1.ToSqlInt64 ().Value);
                        Assertion.AssertEquals ("#M02F", (long)0, Test0.ToSqlInt64 ().Value);

                        try {        
                                SqlInt64 test = Test2.ToSqlInt64 ().Value;
                                Assertion.Fail ("#M03F");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M04F", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlMoney ()
                        Assertion.AssertEquals ("#M01G", (decimal)250, Test1.ToSqlMoney ().Value);
                        Assertion.AssertEquals ("#M02G", (decimal)0, Test0.ToSqlMoney ().Value);

                        try {
                                SqlMoney test = Test2.ToSqlMoney ().Value;
                                Assertion.Fail ("#M03G");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M04G", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlSingle ()
                        Assertion.AssertEquals ("#M01H", (float)250, Test1.ToSqlSingle ().Value);
                        Assertion.AssertEquals ("#M02H", (float)0, Test0.ToSqlSingle ().Value);

                        try {
                                SqlSingle test = Test2.ToSqlSingle().Value;
                                Assertion.Fail ("#MO3H");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M04H", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlString ()
                        Assertion.AssertEquals ("#M01I", "250", Test1.ToSqlString ().Value);
                        Assertion.AssertEquals ("#M02I", "0", Test0.ToSqlString ().Value);
                        Assertion.AssertEquals ("#M03I", "6,4E+65", Test2.ToSqlString ().Value);

                        // ToString ()
                        Assertion.AssertEquals ("#M01J", "250", Test1.ToString ());
                        Assertion.AssertEquals ("#M02J", "0", Test0.ToString ());
                        Assertion.AssertEquals ("#M03J", "6,4E+65", Test2.ToString ());
                }

                // OPERATORS

		[Test]
                public void ArithmeticOperators()
                {
                        SqlDouble Test0 = new SqlDouble (0);
                        SqlDouble Test1 = new SqlDouble (24E+100);
                        SqlDouble Test2 = new SqlDouble (64E+164);
                        SqlDouble Test3 = new SqlDouble (12E+100);
                        SqlDouble Test4 = new SqlDouble (1E+10);
                        SqlDouble Test5 = new SqlDouble (2E+10);

                        // "+"-operator
                        Assertion.AssertEquals ("#N01", (SqlDouble)3E+10, Test4 + Test5);
     
                        try {
                                SqlDouble test = SqlDouble.MaxValue + SqlDouble.MaxValue;
                                Assertion.Fail ("#N02");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N03", typeof (OverflowException), e.GetType ());
                        }

                        // "/"-operator
                        Assertion.AssertEquals ("#N04", (SqlDouble)2, Test1 / Test3);

                        try {
                                SqlDouble test = Test3 / Test0;
                                Assertion.Fail ("#N05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N06", typeof (DivideByZeroException), e.GetType ());
                        }

                        // "*"-operator
                        Assertion.AssertEquals ("#N07", (SqlDouble)2e20, Test4 * Test5);

                        try {
                                SqlDouble test = SqlDouble.MaxValue * Test1;
                                Assertion.Fail ("#N08");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N09", typeof (OverflowException), e.GetType ());
                        }

                        // "-"-operator
                        Assertion.AssertEquals ("#N10", (SqlDouble)12e100, Test1 - Test3);

                        try {
                                SqlDouble test = SqlDouble.MinValue - SqlDouble.MaxValue;
                                Assertion.Fail ("#N11");
                        } catch  (Exception e) {
                                Assertion.AssertEquals ("#N12", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void ThanOrEqualOperators()
                {
                        SqlDouble Test1 = new SqlDouble (1E+164);
                        SqlDouble Test2 = new SqlDouble (9.7E+100);
                        SqlDouble Test22 = new SqlDouble (9.7E+100);
                        SqlDouble Test3 = new SqlDouble (2E+200);

                        // == -operator
                        Assertion.Assert ("#O01", (Test2 == Test22).Value);
                        Assertion.Assert ("#O02", !(Test1 == Test2).Value);
                        Assertion.Assert ("#O03", (Test1 == SqlDouble.Null).IsNull);
                        
                        // != -operator
                        Assertion.Assert ("#O04", !(Test2 != Test22).Value);
                        Assertion.Assert ("#O05", (Test2 != Test3).Value);
                        Assertion.Assert ("#O06", (Test1 != Test3).Value);
                        Assertion.Assert ("#O07", (Test1 != SqlDouble.Null).IsNull);

                        // > -operator
                        Assertion.Assert ("#O08", (Test1 > Test2).Value);
                        Assertion.Assert ("#O09", !(Test1 > Test3).Value);
                        Assertion.Assert ("#O10", !(Test2 > Test22).Value);
                        Assertion.Assert ("#O11", (Test1 > SqlDouble.Null).IsNull);

                        // >=  -operator
                        Assertion.Assert ("#O12", !(Test1 >= Test3).Value);
                        Assertion.Assert ("#O13", (Test3 >= Test1).Value);
                        Assertion.Assert ("#O14", (Test2 >= Test22).Value);
                        Assertion.Assert ("#O15", (Test1 >= SqlDouble.Null).IsNull);

                        // < -operator
                        Assertion.Assert ("#O16", !(Test1 < Test2).Value);
                        Assertion.Assert ("#O17", (Test1 < Test3).Value);
                        Assertion.Assert ("#O18", !(Test2 < Test22).Value);
                        Assertion.Assert ("#O19", (Test1 < SqlDouble.Null).IsNull);

                        // <= -operator
                        Assertion.Assert ("#O20", (Test1 <= Test3).Value);
                        Assertion.Assert ("#O21", !(Test3 <= Test1).Value);
                        Assertion.Assert ("#O22", (Test2 <= Test22).Value);
                        Assertion.Assert ("#O23", (Test1 <= SqlDouble.Null).IsNull);
                }

		[Test]
                public void UnaryNegation()
                {
                        SqlDouble Test = new SqlDouble (2000000001);
                        SqlDouble TestNeg = new SqlDouble (-3000);

                        SqlDouble Result = -Test;
                        Assertion.AssertEquals ("#P01", (double)(-2000000001), Result.Value);

                        Result = -TestNeg;
                        Assertion.AssertEquals ("#P02", (double)3000, Result.Value);
                }

		[Test]
                public void SqlBooleanToSqlDouble()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);
                        SqlDouble Result;

                        Result = (SqlDouble)TestBoolean;

                        Assertion.AssertEquals ("#Q01", (double)1, Result.Value);

                        Result = (SqlDouble)SqlBoolean.Null;
                        Assertion.Assert ("#Q02", Result.IsNull);
                }

		[Test]
                public void SqlDoubleToDouble()
                {
                        SqlDouble Test = new SqlDouble (12e12);
                        Double Result = (double)Test;
                        Assertion.AssertEquals ("#R01", 12e12, Result);
                }

		[Test]
                public void SqlStringToSqlDouble()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");

                        Assertion.AssertEquals ("#S01", (double)100, ((SqlDouble)TestString100).Value);

                        try {
                                SqlDouble test = (SqlDouble)TestString;
                                Assertion.Fail ("#S02");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#S03", typeof (FormatException), e.GetType ());
                        }
                }

		[Test]
                public void DoubleToSqlDouble()
                {
                        double Test1 = 5e64;
                        SqlDouble Result = (SqlDouble)Test1;
                        Assertion.AssertEquals ("#T01", 5e64, Result.Value);
                }

		[Test]
                public void ByteToSqlDouble()
                {
                        short TestShort = 14;
                        Assertion.AssertEquals ("#U01", (double)14, ((SqlDouble)TestShort).Value);
                }
                
		[Test]
                public void SqlDecimalToSqlDouble()
                {
                        SqlDecimal TestDecimal64 = new SqlDecimal (64);

                        Assertion.AssertEquals ("#V01", (double)64, ((SqlDouble)TestDecimal64).Value);
                        Assertion.AssertEquals ("#V02", SqlDouble.Null, ((SqlDouble)SqlDecimal.Null));
                }

		[Test]
                public void SqlIntToSqlDouble()
                {
                        SqlInt16 Test64 = new SqlInt16 (64);
                        SqlInt32 Test640 = new SqlInt32 (640);
                        SqlInt64 Test64000 = new SqlInt64 (64000);
                        Assertion.AssertEquals ("#W01", (double)64, ((SqlDouble)Test64).Value);
                        Assertion.AssertEquals ("#W02", (double)640, ((SqlDouble)Test640).Value);
                        Assertion.AssertEquals ("#W03", (double)64000, ((SqlDouble)Test64000).Value);
                }

		[Test]
                public void SqlMoneyToSqlDouble()
                {
                        SqlMoney TestMoney64 = new SqlMoney(64);
                        Assertion.AssertEquals ("#X01", (double)64, ((SqlDouble)TestMoney64).Value);
                }

		[Test]
                public void SqlSingleToSqlDouble()
                {
                        SqlSingle TestSingle64 = new SqlSingle (64);
                        Assertion.AssertEquals ("#Y01", (double)64, ((SqlDouble)TestSingle64).Value);
                }
        }
}

