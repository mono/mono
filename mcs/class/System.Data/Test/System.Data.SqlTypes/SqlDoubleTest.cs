//
// SqlDoubleTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlDouble
//
// Ville Palo (vi64pa@koti.soon.fi)
//
// (C) Ville Palo 2002
// 

using NUnit.Framework;
using System;
using System.Data.SqlTypes;

namespace MonoTests.System.Data.SqlTypes
{
        public class SqlDoubleTest : TestCase {

                public SqlDoubleTest() : base ("System.Data.SqlTypes.SqlDouble") {}
                public SqlDoubleTest(string name) : base(name) {}

                protected override void TearDown() {}

                protected override void SetUp() {}

                public static ITest Suite {
                        get {
                                return new TestSuite(typeof(SqlDouble));
                        }
                }

                // Test constructor
                public void TestCreate()
                {
                        SqlDouble Test= new SqlDouble ((double)34.87);
                        AssertEquals ("#A01", 34.87D, Test.Value);

                        Test = new SqlDouble (-9000.6543);
                        AssertEquals ("#A02", -9000.6543D, Test.Value);
                }

                // Test public fields
                public void TestPublicFields()
                {
                        AssertEquals ("#B01", 1.7976931348623157e+308, SqlDouble.MaxValue.Value);
                        AssertEquals ("#B02", -1.7976931348623157e+308, SqlDouble.MinValue.Value);
                        Assert ("#B03", SqlDouble.Null.IsNull);
                        AssertEquals ("#B04", 0d, SqlDouble.Zero.Value);
                }

                // Test properties
                public void TestProperties()
                {
                        SqlDouble Test5443 = new SqlDouble (5443e12);
                        SqlDouble Test1 = new SqlDouble (1);

                        Assert ("#C01", SqlDouble.Null.IsNull);
                        AssertEquals ("#C02", 5443e12, Test5443.Value);
                        AssertEquals ("#C03", (double)1, Test1.Value);
                }

                // PUBLIC METHODS

                public void TestArithmeticMethods()
                {
                        SqlDouble Test0 = new SqlDouble (0);
                        SqlDouble Test1 = new SqlDouble (15E+108);
                        SqlDouble Test2 = new SqlDouble (-65E+64);
                        SqlDouble Test3 = new SqlDouble (5E+64);
                        SqlDouble Test4 = new SqlDouble (5E+108);
                        SqlDouble TestMax = new SqlDouble (SqlDouble.MaxValue.Value);

                        // Add()
                        AssertEquals ("#D01A", 15E+108, SqlDouble.Add (Test1, Test0).Value);
                        AssertEquals ("#D02A", 1.5E+109, SqlDouble.Add (Test1, Test2).Value);

                        try {
                                SqlDouble test = SqlDouble.Add (SqlDouble.MaxValue, SqlDouble.MaxValue);
                                Fail ("#D03A");
                        } catch (Exception e) {
                                AssertEquals ("#D04A", typeof (OverflowException), e.GetType ());
                        }
                        
                        // Divide()
                        AssertEquals ("#D01B", (SqlDouble)3, SqlDouble.Divide (Test1, Test4));
                        AssertEquals ("#D02B", -13d, SqlDouble.Divide (Test2, Test3).Value);

                        try {
                                SqlDouble test = SqlDouble.Divide(Test1, Test0).Value;
                                Fail ("#D03B");
                        } catch(Exception e) {
                                AssertEquals ("#D04B", typeof (DivideByZeroException), e.GetType ());
                        }

                        // Multiply()
                        AssertEquals ("#D01D", (double)(75E+216), SqlDouble.Multiply (Test1, Test4).Value);
                        AssertEquals ("#D02D", (double)0, SqlDouble.Multiply (Test1, Test0).Value);

                        try {
                                SqlDouble test = SqlDouble.Multiply (TestMax, Test1);
                                Fail ("#D03D");
                        } catch (Exception e) {
                                AssertEquals ("#D04D", typeof (OverflowException), e.GetType ());
                        }
                                

                        // Subtract()
                        AssertEquals ("#D01F", (double)1.5E+109, SqlDouble.Subtract (Test1, Test3).Value);

                        try {
                                SqlDouble test = SqlDouble.Subtract(SqlDouble.MinValue, SqlDouble.MaxValue);
                                Fail ("D02F");
                        } catch (Exception e) {
                                AssertEquals ("#D03F", typeof (OverflowException), e.GetType ());
                        }                                
                }

                public void TestCompareTo()
                {
                        SqlDouble Test1 = new SqlDouble (4e64);
                        SqlDouble Test11 = new SqlDouble (4e64);
                        SqlDouble Test2 = new SqlDouble (-9e34);
                        SqlDouble Test3 = new SqlDouble (10000);
                        SqlString TestString = new SqlString ("This is a test");

                        Assert ("#E01", Test1.CompareTo (Test3) > 0);
                        Assert ("#E02", Test2.CompareTo (Test3) < 0);
                        Assert ("#E03", Test1.CompareTo (Test11) == 0);
                        Assert ("#E04", Test11.CompareTo (SqlDouble.Null) > 0);

                        try {
                                Test1.CompareTo (TestString);
                                Fail("#E05");
                        } catch(Exception e) {
                                AssertEquals ("#E06", typeof (ArgumentException), e.GetType ());
                        }
                }

                public void TestEqualsMethods()
                {
                        SqlDouble Test0 = new SqlDouble (0);
                        SqlDouble Test1 = new SqlDouble (1.58e30);
                        SqlDouble Test2 = new SqlDouble (1.8e180);
                        SqlDouble Test22 = new SqlDouble (1.8e180);

                        Assert ("#F01", !Test0.Equals (Test1));
                        Assert ("#F02", !Test1.Equals (Test2));
                        Assert ("#F03", !Test2.Equals (new SqlString ("TEST")));
                        Assert ("#F04", Test2.Equals (Test22));

                        // Static Equals()-method
                        Assert ("#F05", SqlDouble.Equals (Test2, Test22).Value);
                        Assert ("#F06", !SqlDouble.Equals (Test1, Test2).Value);
                }

                public void TestGetHashCode()
                {
                        SqlDouble Test15 = new SqlDouble (15);

                        // FIXME: Better way to test HashCode
                        AssertEquals ("#G01", Test15.GetHashCode (), Test15.GetHashCode ());
                }

                public void TestGetType()
                {
                        SqlDouble Test = new SqlDouble (84);
                        AssertEquals ("#H01", "System.Data.SqlTypes.SqlDouble", Test.GetType ().ToString ());
                        AssertEquals ("#H02", "System.Double", Test.Value.GetType ().ToString ());
                }

                public void TestGreaters()
                {
                        SqlDouble Test1 = new SqlDouble (1e100);
                        SqlDouble Test11 = new SqlDouble (1e100);
                        SqlDouble Test2 = new SqlDouble (64e164);

                        // GreateThan ()
                        Assert ("#I01", !SqlDouble.GreaterThan (Test1, Test2).Value);
                        Assert ("#I02", SqlDouble.GreaterThan (Test2, Test1).Value);
                        Assert ("#I03", !SqlDouble.GreaterThan (Test1, Test11).Value);

                        // GreaterTharOrEqual ()
                        Assert ("#I04", !SqlDouble.GreaterThanOrEqual (Test1, Test2).Value);
                        Assert ("#I05", SqlDouble.GreaterThanOrEqual (Test2, Test1).Value);
                        Assert ("#I06", SqlDouble.GreaterThanOrEqual (Test1, Test11).Value);
                }

                public void TestLessers()
                {
                        SqlDouble Test1 = new SqlDouble (1.8e100);
                        SqlDouble Test11 = new SqlDouble (1.8e100);
                        SqlDouble Test2 = new SqlDouble (64e164);

                        // LessThan()
                        Assert ("#J01", !SqlDouble.LessThan (Test1, Test11).Value);
                        Assert ("#J02", !SqlDouble.LessThan (Test2, Test1).Value);
                        Assert ("#J03", SqlDouble.LessThan (Test11, Test2).Value);

                        // LessThanOrEqual ()
                        Assert ("#J04", SqlDouble.LessThanOrEqual (Test1, Test2).Value);
                        Assert ("#J05", !SqlDouble.LessThanOrEqual (Test2, Test1).Value);
                        Assert ("#J06", SqlDouble.LessThanOrEqual (Test11, Test1).Value);
                        Assert ("#J07", SqlDouble.LessThanOrEqual (Test11, SqlDouble.Null).IsNull);
                }

                public void TestNotEquals()
                {
                        SqlDouble Test1 = new SqlDouble (1280000000001);
                        SqlDouble Test2 = new SqlDouble (128e10);
                        SqlDouble Test22 = new SqlDouble (128e10);

                        Assert ("#K01", SqlDouble.NotEquals (Test1, Test2).Value);
                        Assert ("#K02", SqlDouble.NotEquals (Test2, Test1).Value);
                        Assert ("#K03", SqlDouble.NotEquals (Test22, Test1).Value);
                        Assert ("#K04", !SqlDouble.NotEquals (Test22, Test2).Value);
                        Assert ("#K05", !SqlDouble.NotEquals (Test2, Test22).Value);
                        Assert ("#K06", SqlDouble.NotEquals (SqlDouble.Null, Test22).IsNull);
                        Assert ("#K07", SqlDouble.NotEquals (SqlDouble.Null, Test22).IsNull);
                }

                public void TestParse()
                {
                        try {
                                SqlDouble.Parse (null);
                                Fail ("#L01");
                        } catch (Exception e) {
                                AssertEquals ("#L02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlDouble.Parse ("not-a-number");
                                Fail ("#L03");
                        } catch (Exception e) {

                                AssertEquals ("#L04", typeof (FormatException), e.GetType ());
                        }

                         try {
                                SqlDouble.Parse ("9e400");
                                Fail ("#L05");
                        } catch (Exception e) {
                                AssertEquals ("#L06", typeof (OverflowException), e.GetType ());
                        }

                        AssertEquals("#L07", (double)150, SqlDouble.Parse ("150").Value);
                }

                public void TestConversions()
                {
                        SqlDouble Test0 = new SqlDouble (0);
                        SqlDouble Test1 = new SqlDouble (250);
                        SqlDouble Test2 = new SqlDouble (64e64);
                        SqlDouble Test3 = new SqlDouble (64e164);
                        SqlDouble TestNull = SqlDouble.Null;

                        // ToSqlBoolean ()
                        Assert ("#M01A", Test1.ToSqlBoolean ().Value);
                        Assert ("#M02A", !Test0.ToSqlBoolean ().Value);
                        Assert ("#M03A", TestNull.ToSqlBoolean ().IsNull);

                        // ToSqlByte ()
                        AssertEquals ("#M01B", (byte)250, Test1.ToSqlByte ().Value);
                        AssertEquals ("#M02B", (byte)0, Test0.ToSqlByte ().Value);

                        try {
                                SqlByte b = (byte)Test2.ToSqlByte ();
                                Fail ("#M03B");
                        } catch (Exception e) {
                                AssertEquals ("#M04B", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlDecimal ()
                        AssertEquals ("#M01C", (decimal)250, Test1.ToSqlDecimal ().Value);
                        AssertEquals ("#M02C", (decimal)0, Test0.ToSqlDecimal ().Value);

                        try {
                                SqlDecimal test = Test3.ToSqlDecimal ().Value;
                                Fail ("#M03C");
                        } catch (Exception e) {
                                AssertEquals ("#M04C", typeof (OverflowException), e.GetType ());
                        }      

                        // ToSqlInt16 ()
                        AssertEquals ("#M01D", (short)250, Test1.ToSqlInt16 ().Value);
                        AssertEquals ("#M02D", (short)0, Test0.ToSqlInt16 ().Value);

                        try {
                                SqlInt16 test = Test2.ToSqlInt16().Value;
                                Fail ("#M03D");
                        } catch (Exception e) {
                                AssertEquals ("#M04D", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlInt32 ()
                        AssertEquals ("#M01E", (int)250, Test1.ToSqlInt32 ().Value);
                        AssertEquals ("#M02E", (int)0, Test0.ToSqlInt32 ().Value);

                        try {
                                SqlInt32 test = Test2.ToSqlInt32 ().Value;
                                Fail ("#M03E");
                        } catch (Exception e) { 
                                AssertEquals ("#M04E", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlInt64 ()
                        AssertEquals ("#M01F", (long)250, Test1.ToSqlInt64 ().Value);
                        AssertEquals ("#M02F", (long)0, Test0.ToSqlInt64 ().Value);

                        try {        
                                SqlInt64 test = Test2.ToSqlInt64 ().Value;
                                Fail ("#M03F");
                        } catch (Exception e) {
                                AssertEquals ("#M04F", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlMoney ()
                        AssertEquals ("#M01G", (decimal)250, Test1.ToSqlMoney ().Value);
                        AssertEquals ("#M02G", (decimal)0, Test0.ToSqlMoney ().Value);

                        try {
                                SqlMoney test = Test2.ToSqlMoney ().Value;
                                Fail ("#M03G");
                        } catch (Exception e) {
                                AssertEquals ("#M04G", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlSingle ()
                        AssertEquals ("#M01H", (float)250, Test1.ToSqlSingle ().Value);
                        AssertEquals ("#M02H", (float)0, Test0.ToSqlSingle ().Value);

                        try {
                                SqlSingle test = Test2.ToSqlSingle().Value;
                                Fail ("#MO3H");
                        } catch (Exception e) {
                                AssertEquals ("#M04H", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlString ()
                        AssertEquals ("#M01I", "250", Test1.ToSqlString ().Value);
                        AssertEquals ("#M02I", "0", Test0.ToSqlString ().Value);
                        AssertEquals ("#M03I", "6,4E+65", Test2.ToSqlString ().Value);

                        // ToString ()
                        AssertEquals ("#M01J", "250", Test1.ToString ());
                        AssertEquals ("#M02J", "0", Test0.ToString ());
                        AssertEquals ("#M03J", "6,4E+65", Test2.ToString ());
                }

                // OPERATORS

                public void TestArithmeticOperators()
                {
                        SqlDouble Test0 = new SqlDouble (0);
                        SqlDouble Test1 = new SqlDouble (24E+100);
                        SqlDouble Test2 = new SqlDouble (64E+164);
                        SqlDouble Test3 = new SqlDouble (12E+100);
                        SqlDouble Test4 = new SqlDouble (1E+10);
                        SqlDouble Test5 = new SqlDouble (2E+10);

                        // "+"-operator
                        AssertEquals ("#N01", (SqlDouble)3E+10, Test4 + Test5);
     
                        try {
                                SqlDouble test = SqlDouble.MaxValue + SqlDouble.MaxValue;
                                Fail ("#N02");
                        } catch (Exception e) {
                                AssertEquals ("#N03", typeof (OverflowException), e.GetType ());
                        }

                        // "/"-operator
                        AssertEquals ("#N04", (SqlDouble)2, Test1 / Test3);

                        try {
                                SqlDouble test = Test3 / Test0;
                                Fail ("#N05");
                        } catch (Exception e) {
                                AssertEquals ("#N06", typeof (DivideByZeroException), e.GetType ());
                        }

                        // "*"-operator
                        AssertEquals ("#N07", (SqlDouble)2e20, Test4 * Test5);

                        try {
                                SqlDouble test = SqlDouble.MaxValue * Test1;
                                Fail ("#N08");
                        } catch (Exception e) {
                                AssertEquals ("#N09", typeof (OverflowException), e.GetType ());
                        }

                        // "-"-operator
                        AssertEquals ("#N10", (SqlDouble)12e100, Test1 - Test3);

                        try {
                                SqlDouble test = SqlDouble.MinValue - SqlDouble.MaxValue;
                                Fail ("#N11");
                        } catch  (Exception e) {
                                AssertEquals ("#N12", typeof (OverflowException), e.GetType ());
                        }
                }

                public void TestThanOrEqualOperators()
                {
                        SqlDouble Test1 = new SqlDouble (1E+164);
                        SqlDouble Test2 = new SqlDouble (9.7E+100);
                        SqlDouble Test22 = new SqlDouble (9.7E+100);
                        SqlDouble Test3 = new SqlDouble (2E+200);

                        // == -operator
                        Assert ("#O01", (Test2 == Test22).Value);
                        Assert ("#O02", !(Test1 == Test2).Value);
                        Assert ("#O03", (Test1 == SqlDouble.Null).IsNull);
                        
                        // != -operator
                        Assert ("#O04", !(Test2 != Test22).Value);
                        Assert ("#O05", (Test2 != Test3).Value);
                        Assert ("#O06", (Test1 != Test3).Value);
                        Assert ("#O07", (Test1 != SqlDouble.Null).IsNull);

                        // > -operator
                        Assert ("#O08", (Test1 > Test2).Value);
                        Assert ("#O09", !(Test1 > Test3).Value);
                        Assert ("#O10", !(Test2 > Test22).Value);
                        Assert ("#O11", (Test1 > SqlDouble.Null).IsNull);

                        // >=  -operator
                        Assert ("#O12", !(Test1 >= Test3).Value);
                        Assert ("#O13", (Test3 >= Test1).Value);
                        Assert ("#O14", (Test2 >= Test22).Value);
                        Assert ("#O15", (Test1 >= SqlDouble.Null).IsNull);

                        // < -operator
                        Assert ("#O16", !(Test1 < Test2).Value);
                        Assert ("#O17", (Test1 < Test3).Value);
                        Assert ("#O18", !(Test2 < Test22).Value);
                        Assert ("#O19", (Test1 < SqlDouble.Null).IsNull);

                        // <= -operator
                        Assert ("#O20", (Test1 <= Test3).Value);
                        Assert ("#O21", !(Test3 <= Test1).Value);
                        Assert ("#O22", (Test2 <= Test22).Value);
                        Assert ("#O23", (Test1 <= SqlDouble.Null).IsNull);
                }

                public void TestUnaryNegation()
                {
                        SqlDouble Test = new SqlDouble (2000000001);
                        SqlDouble TestNeg = new SqlDouble (-3000);

                        SqlDouble Result = -Test;
                        AssertEquals ("#P01", (double)(-2000000001), Result.Value);

                        Result = -TestNeg;
                        AssertEquals ("#P02", (double)3000, Result.Value);
                }

                public void TestSqlBooleanToSqlDouble()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);
                        SqlDouble Result;

                        Result = (SqlDouble)TestBoolean;

                        AssertEquals ("#Q01", (double)1, Result.Value);

                        Result = (SqlDouble)SqlBoolean.Null;
                        Assert ("#Q02", Result.IsNull);
                }

                public void TestSqlDoubleToDouble()
                {
                        SqlDouble Test = new SqlDouble (12e12);
                        Double Result = (double)Test;
                        AssertEquals ("#R01", 12e12, Result);
                }

                public void TestSqlStringToSqlDouble()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");

                        AssertEquals ("#S01", (double)100, ((SqlDouble)TestString100).Value);

                        try {
                                SqlDouble test = (SqlDouble)TestString;
                                Fail ("#S02");
                        } catch(Exception e) {
                                AssertEquals ("#S03", typeof (FormatException), e.GetType ());
                        }
                }

                public void TestDoubleToSqlDouble()
                {
                        double Test1 = 5e64;
                        SqlDouble Result = (SqlDouble)Test1;
                        AssertEquals ("#T01", 5e64, Result.Value);
                }

                public void TestByteToSqlDouble()
                {
                        short TestShort = 14;
                        AssertEquals ("#U01", (double)14, ((SqlDouble)TestShort).Value);
                }
                
                public void TestSqlDecimalToSqlDouble()
                {
                        SqlDecimal TestDecimal64 = new SqlDecimal (64);

                        AssertEquals ("#V01", (double)64, ((SqlDouble)TestDecimal64).Value);
                        AssertEquals ("#V02", SqlDouble.Null, ((SqlDouble)SqlDecimal.Null));
                }

                public void TestSqlIntToSqlDouble()
                {
                        SqlInt16 Test64 = new SqlInt16 (64);
                        SqlInt32 Test640 = new SqlInt32 (640);
                        SqlInt64 Test64000 = new SqlInt64 (64000);
                        AssertEquals ("#W01", (double)64, ((SqlDouble)Test64).Value);
                        AssertEquals ("#W02", (double)640, ((SqlDouble)Test640).Value);
                        AssertEquals ("#W03", (double)64000, ((SqlDouble)Test64000).Value);
                }


                public void TestSqlMoneyToSqlDouble()
                {
                        SqlMoney TestMoney64 = new SqlMoney(64);
                        AssertEquals ("#X01", (double)64, ((SqlDouble)TestMoney64).Value);
                }

                public void TestSqlSingleToSqlDouble()
                {
                        SqlSingle TestSingle64 = new SqlSingle (64);
                        AssertEquals ("#Y01", (double)64, ((SqlDouble)TestSingle64).Value);
                }
        }
}

