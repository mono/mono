//
// SqlSingleTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlSingle
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
using System.Threading;
using System.Globalization;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlSingleTest : Assertion {

		[SetUp]
                public void GetReady() 
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		}

                // Test constructor
		[Test]
                public void Create()
                {
                        SqlSingle Test= new SqlSingle ((float)34.87);
                        SqlSingle Test2 = 45.2f;
                        
                        AssertEquals ("#A01", 34.87f, Test.Value);
                        AssertEquals ("#A02", 45.2f, Test2.Value);

                        Test = new SqlSingle (-9000.6543);
                        AssertEquals ("#A03", -9000.6543f, Test.Value);
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        AssertEquals ("#B01", 3.40282346638528859E+38f, 
				      SqlSingle.MaxValue.Value);
                        AssertEquals ("#B02", -3.40282346638528859E+38f, 
				      SqlSingle.MinValue.Value);
                        Assert ("#B03", SqlSingle.Null.IsNull);
                        AssertEquals ("#B04", 0f, SqlSingle.Zero.Value);
                }

                // Test properties
		[Test]
                public void Properties()
                {
                        SqlSingle Test = new SqlSingle (5443e12f);
                        SqlSingle Test1 = new SqlSingle (1);

                        Assert ("#C01", SqlSingle.Null.IsNull);
                        AssertEquals ("#C02", 5443e12f, Test.Value);
                        AssertEquals ("#C03", (float)1, Test1.Value);
                }

                // PUBLIC METHODS

		[Test]
                public void ArithmeticMethods()
                {
                        SqlSingle Test0 = new SqlSingle (0);
                        SqlSingle Test1 = new SqlSingle (15E+18);
                        SqlSingle Test2 = new SqlSingle (-65E+6);
                        SqlSingle Test3 = new SqlSingle (5E+30);
                        SqlSingle Test4 = new SqlSingle (5E+18);
                        SqlSingle TestMax = new SqlSingle (SqlSingle.MaxValue.Value);

                        // Add()
                        AssertEquals ("#D01A", 15E+18f, SqlSingle.Add (Test1, Test0).Value);
                        AssertEquals ("#D02A", 1.5E+19f, SqlSingle.Add (Test1, Test2).Value);

                        try {			  
                                SqlSingle test = SqlSingle.Add (SqlSingle.MaxValue, 
							 SqlSingle.MaxValue);
                                Fail ("#D03A");
                        } catch (Exception e) {
                                AssertEquals ("#D04A", typeof (OverflowException), e.GetType ());
                        }
                        
                        // Divide()
                        AssertEquals ("#D01B", (SqlSingle)3, SqlSingle.Divide (Test1, Test4));
                        AssertEquals ("#D02B", -1.3E-23f, SqlSingle.Divide (Test2, Test3).Value);

                        try {
                                SqlSingle test = SqlSingle.Divide(Test1, Test0).Value;
                                Fail ("#D03B");
                        } catch(Exception e) {
                                AssertEquals ("#D04B", typeof (DivideByZeroException), 
					      e.GetType ());
                        }

			// Multiply()
                        AssertEquals ("#D01D", (float)(7.5E+37), 
				      SqlSingle.Multiply (Test1, Test4).Value);
                        AssertEquals ("#D02D", (float)0, SqlSingle.Multiply (Test1, Test0).Value);

                        try {
                                SqlSingle test = SqlSingle.Multiply (TestMax, Test1);
                                Fail ("#D03D");
                        } catch (Exception e) {
                                AssertEquals ("#D04D", typeof (OverflowException), e.GetType ());
                        }
                                

                        // Subtract()
                        AssertEquals ("#D01F", (float)(-5E+30), 
				      SqlSingle.Subtract (Test1, Test3).Value);

                        try {
                                SqlSingle test = SqlSingle.Subtract(
					SqlSingle.MinValue, SqlSingle.MaxValue);
                                Fail ("D02F");
                        } catch (Exception e) {			
                                AssertEquals ("#D03F", typeof (OverflowException), e.GetType ());
                        }                      
                }

		[Test]
                public void CompareTo()
                {
                        SqlSingle Test1 = new SqlSingle (4E+30);
                        SqlSingle Test11 = new SqlSingle (4E+30);
                        SqlSingle Test2 = new SqlSingle (-9E+30);
                        SqlSingle Test3 = new SqlSingle (10000);
                        SqlString TestString = new SqlString ("This is a test");

                        Assert ("#E01", Test1.CompareTo (Test3) > 0);
                        Assert ("#E02", Test2.CompareTo (Test3) < 0);
                        Assert ("#E03", Test1.CompareTo (Test11) == 0);
                        Assert ("#E04", Test11.CompareTo (SqlSingle.Null) > 0);

                        try {
                                Test1.CompareTo (TestString);
                                Fail("#E05");
                        } catch(Exception e) {
                                AssertEquals ("#E06", typeof (ArgumentException), e.GetType ());
                        }
                }

		[Test]
                public void EqualsMethods()
                {
                        SqlSingle Test0 = new SqlSingle (0);
                        SqlSingle Test1 = new SqlSingle (1.58e30);
                        SqlSingle Test2 = new SqlSingle (1.8e32);
                        SqlSingle Test22 = new SqlSingle (1.8e32);

                        Assert ("#F01", !Test0.Equals (Test1));
                        Assert ("#F02", !Test1.Equals (Test2));
                        Assert ("#F03", !Test2.Equals (new SqlString ("TEST")));
                        Assert ("#F04", Test2.Equals (Test22));

                        // Static Equals()-method
                        Assert ("#F05", SqlSingle.Equals (Test2, Test22).Value);
                        Assert ("#F06", !SqlSingle.Equals (Test1, Test2).Value);
                }

		[Test]
                public void GetHashCodeTest()
                {
                        SqlSingle Test15 = new SqlSingle (15);

                        // FIXME: Better way to test HashCode
                        AssertEquals ("#G01", Test15.GetHashCode (), Test15.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
                        SqlSingle Test = new SqlSingle (84);
                        AssertEquals ("#H01", "System.Data.SqlTypes.SqlSingle", 
				      Test.GetType ().ToString ());
                        AssertEquals ("#H02", "System.Single", Test.Value.GetType ().ToString ());
                }

		[Test]
                public void Greaters()
                {
                        SqlSingle Test1 = new SqlSingle (1e10);
                        SqlSingle Test11 = new SqlSingle (1e10);
                        SqlSingle Test2 = new SqlSingle (64e14);

                        // GreateThan ()
                        Assert ("#I01", !SqlSingle.GreaterThan (Test1, Test2).Value);
                        Assert ("#I02", SqlSingle.GreaterThan (Test2, Test1).Value);
                        Assert ("#I03", !SqlSingle.GreaterThan (Test1, Test11).Value);

                        // GreaterTharOrEqual ()
                        Assert ("#I04", !SqlSingle.GreaterThanOrEqual (Test1, Test2).Value);
                        Assert ("#I05", SqlSingle.GreaterThanOrEqual (Test2, Test1).Value);
                        Assert ("#I06", SqlSingle.GreaterThanOrEqual (Test1, Test11).Value);
                }

		[Test]
                public void Lessers()
                {
                        SqlSingle Test1 = new SqlSingle(1.8e10);
                        SqlSingle Test11 = new SqlSingle (1.8e10);
                        SqlSingle Test2 = new SqlSingle (64e14);

                        // LessThan()
                        Assert ("#J01", !SqlSingle.LessThan (Test1, Test11).Value);
                        Assert ("#J02", !SqlSingle.LessThan (Test2, Test1).Value);
                        Assert ("#J03", SqlSingle.LessThan (Test11, Test2).Value);

                        // LessThanOrEqual ()
                        Assert ("#J04", SqlSingle.LessThanOrEqual (Test1, Test2).Value);
                        Assert ("#J05", !SqlSingle.LessThanOrEqual (Test2, Test1).Value);
                        Assert ("#J06", SqlSingle.LessThanOrEqual (Test11, Test1).Value);
                        Assert ("#J07", SqlSingle.LessThanOrEqual (Test11, SqlSingle.Null).IsNull);
                }

		[Test]
                public void NotEquals()
                {
                        SqlSingle Test1 = new SqlSingle (12800000000001);
                        SqlSingle Test2 = new SqlSingle (128e10);
                        SqlSingle Test22 = new SqlSingle (128e10);

                        Assert ("#K01", SqlSingle.NotEquals (Test1, Test2).Value);
                        Assert ("#K02", SqlSingle.NotEquals (Test2, Test1).Value);
                        Assert ("#K03", SqlSingle.NotEquals (Test22, Test1).Value);
                        Assert ("#K04", !SqlSingle.NotEquals (Test22, Test2).Value);
                        Assert ("#K05", !SqlSingle.NotEquals (Test2, Test22).Value);
                        Assert ("#K06", SqlSingle.NotEquals (SqlSingle.Null, Test22).IsNull);
                        Assert ("#K07", SqlSingle.NotEquals (SqlSingle.Null, Test22).IsNull);
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlSingle.Parse (null);
                                Fail ("#L01");
                        } catch (Exception e) {
                                AssertEquals ("#L02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlSingle.Parse ("not-a-number");
                                Fail ("#L03");
                        } catch (Exception e) {
                                AssertEquals ("#L04", typeof (FormatException), e.GetType ());
                        }

                         try {
                                SqlSingle.Parse ("9e44");
                                Fail ("#L05");
                        } catch (Exception e) {
                                AssertEquals ("#L06", typeof (OverflowException), e.GetType ());
                        }

                        AssertEquals("#L07", (float)150, SqlSingle.Parse ("150").Value);
                }

		[Test]
                public void Conversions()
                {
                        SqlSingle Test0 = new SqlSingle (0);
                        SqlSingle Test1 = new SqlSingle (250);
                        SqlSingle Test2 = new SqlSingle (64E+16);
                        SqlSingle Test3 = new SqlSingle (64E+30);
                        SqlSingle TestNull = SqlSingle.Null;

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
                        AssertEquals ("#M01C", 250.00000000000000M, Test1.ToSqlDecimal ().Value);
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
                                SqlInt64 test = Test3.ToSqlInt64 ().Value;
                                Fail ("#M03F");
                        } catch (Exception e) {
                                AssertEquals ("#M04F", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlMoney ()
                        AssertEquals ("#M01G", 250.0000M, Test1.ToSqlMoney ().Value);
                        AssertEquals ("#M02G", (decimal)0, Test0.ToSqlMoney ().Value);

                        try {
                                SqlMoney test = Test3.ToSqlMoney ().Value;
                                Fail ("#M03G");
                        } catch (Exception e) {
                                AssertEquals ("#M04G", typeof (OverflowException), e.GetType ());
                        }        


                        // ToSqlString ()
                        AssertEquals ("#M01H", "250", Test1.ToSqlString ().Value);
                        AssertEquals ("#M02H", "0", Test0.ToSqlString ().Value);
                        AssertEquals ("#M03H", "6.4E+17", Test2.ToSqlString ().Value);

                        // ToString ()
                        AssertEquals ("#M01I", "250", Test1.ToString ());
                        AssertEquals ("#M02I", "0", Test0.ToString ());
                        AssertEquals ("#M03I", "6.4E+17", Test2.ToString ());
                }

                // OPERATORS

		[Test]
                public void ArithmeticOperators()
                {
                        SqlSingle Test0 = new SqlSingle (0);
                        SqlSingle Test1 = new SqlSingle (24E+11);
                        SqlSingle Test2 = new SqlSingle (64E+32);
                        SqlSingle Test3 = new SqlSingle (12E+11);
                        SqlSingle Test4 = new SqlSingle (1E+10);
                        SqlSingle Test5 = new SqlSingle (2E+10);

                        // "+"-operator
                        AssertEquals ("#N01", (SqlSingle)3E+10, Test4 + Test5);
     
                        try {
                                SqlSingle test = SqlSingle.MaxValue + SqlSingle.MaxValue;
                                Fail ("#N02");
                        } catch (Exception e) {
                                AssertEquals ("#N03", typeof (OverflowException), e.GetType ());
                        }

                        try {
                                SqlSingle test = SqlSingle.MaxValue + SqlSingle.MaxValue;
                        } catch (Exception e) {
                                AssertEquals ("#N03a", typeof (OverflowException), e.GetType ());
                        }

                        // "/"-operator
                        AssertEquals ("#N04", (SqlSingle)2, Test1 / Test3);

                        try {
                                SqlSingle test = Test3 / Test0;
                                Fail ("#N05");
                        } catch (Exception e) {
                                AssertEquals ("#N06", typeof (DivideByZeroException), e.GetType ());
                        }

                        // "*"-operator
                        AssertEquals ("#N07", (SqlSingle)2E+20, Test4 * Test5);

                        try {
                                SqlSingle test = SqlSingle.MaxValue * Test1;
                                Fail ("#N08");
                        } catch (Exception e) {
                                AssertEquals ("#N09", typeof (OverflowException), e.GetType ());
                        }

                        // "-"-operator
                        AssertEquals ("#N10", (SqlSingle)12e11, Test1 - Test3);

                        try {
                                SqlSingle test = SqlSingle.MinValue - SqlSingle.MaxValue;
                                Fail ("#N11");
                        } catch  (Exception e) {
                                AssertEquals ("#N12", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void ThanOrEqualOperators()
                {
                        SqlSingle Test1 = new SqlSingle (1.0E+14f);
                        SqlSingle Test2 = new SqlSingle (9.7E+11);
                        SqlSingle Test22 = new SqlSingle (9.7E+11);
                        SqlSingle Test3 = new SqlSingle (2.0E+22f);

                        // == -operator
                        Assert ("#O01", (Test2 == Test22).Value);
                        Assert ("#O02", !(Test1 == Test2).Value);
                        Assert ("#O03", (Test1 == SqlSingle.Null).IsNull);
                        
                        // != -operator
                        Assert ("#O04", !(Test2 != Test22).Value);
                        Assert ("#O05", (Test2 != Test3).Value);
                        Assert ("#O06", (Test1 != Test3).Value);
                        Assert ("#O07", (Test1 != SqlSingle.Null).IsNull);

                        // > -operator
                        Assert ("#O08", (Test1 > Test2).Value);
                        Assert ("#O09", !(Test1 > Test3).Value);
                        Assert ("#O10", !(Test2 > Test22).Value);
                        Assert ("#O11", (Test1 > SqlSingle.Null).IsNull);

                        // >=  -operator
                        Assert ("#O12", !(Test1 >= Test3).Value);
                        Assert ("#O13", (Test3 >= Test1).Value);
                        Assert ("#O14", (Test2 >= Test22).Value);
                        Assert ("#O15", (Test1 >= SqlSingle.Null).IsNull);

                        // < -operator
                        Assert ("#O16", !(Test1 < Test2).Value);
                        Assert ("#O17", (Test1 < Test3).Value);
                        Assert ("#O18", !(Test2 < Test22).Value);
                        Assert ("#O19", (Test1 < SqlSingle.Null).IsNull);

                        // <= -operator
                        Assert ("#O20", (Test1 <= Test3).Value);
                        Assert ("#O21", !(Test3 <= Test1).Value);
                        Assert ("#O22", (Test2 <= Test22).Value);
                        Assert ("#O23", (Test1 <= SqlSingle.Null).IsNull);
                }

		[Test]
                public void UnaryNegation()
                {
                        SqlSingle Test = new SqlSingle (2000000001);
                        SqlSingle TestNeg = new SqlSingle (-3000);

                        SqlSingle Result = -Test;
                        AssertEquals ("#P01", (float)(-2000000001), Result.Value);

                        Result = -TestNeg;
                        AssertEquals ("#P02", (float)3000, Result.Value);
                }

		[Test]
                public void SqlBooleanToSqlSingle()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);
                        SqlSingle Result;

                        Result = (SqlSingle)TestBoolean;

                        AssertEquals ("#Q01", (float)1, Result.Value);

                        Result = (SqlSingle)SqlBoolean.Null;
                        Assert ("#Q02", Result.IsNull);
                }

		[Test]
		public void SqlDoubleToSqlSingle()
	        {
                        SqlDouble Test = new SqlDouble (12e12);
			SqlSingle TestSqlSingle = (SqlSingle)Test;
			AssertEquals ("R01", 12e12f, TestSqlSingle.Value);
		}

		[Test]
                public void SqlSingleToSingle()
                {
                        SqlSingle Test = new SqlSingle (12e12);
                        Single Result = (Single)Test;
                        AssertEquals ("#S01", 12e12f, Result);
                }

		[Test]
                public void SqlStringToSqlSingle()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");

                        AssertEquals ("#T01", (float)100, ((SqlSingle)TestString100).Value);

                        try {
                                SqlSingle test = (SqlSingle)TestString;
                                Fail ("#T02");
                        } catch(Exception e) {
                                AssertEquals ("#T03", typeof (FormatException), e.GetType ());
                        }
                }

		[Test]
                public void ByteToSqlSingle()
                {
                        short TestShort = 14;
                        AssertEquals ("#U01", (float)14, ((SqlSingle)TestShort).Value);
                }
                
		[Test]
                public void SqlDecimalToSqlSingle()
                {
                        SqlDecimal TestDecimal64 = new SqlDecimal (64);

                        AssertEquals ("#V01", (float)64, ((SqlSingle)TestDecimal64).Value);
                        AssertEquals ("#V02", SqlSingle.Null, ((SqlSingle)SqlDecimal.Null));
                }

		[Test]
                public void SqlIntToSqlSingle()
                {
                        SqlInt16 Test64 = new SqlInt16 (64);
                        SqlInt32 Test640 = new SqlInt32 (640);
                        SqlInt64 Test64000 = new SqlInt64 (64000);
                        AssertEquals ("#W01", (float)64, ((SqlSingle)Test64).Value);
                        AssertEquals ("#W02", (float)640, ((SqlSingle)Test640).Value);
                        AssertEquals ("#W03", (float)64000, ((SqlSingle)Test64000).Value);
                }

		[Test]
                public void SqlMoneyToSqlSingle()
                {
                        SqlMoney TestMoney64 = new SqlMoney(64);
                        AssertEquals ("#X01", (float)64, ((SqlSingle)TestMoney64).Value);
                }

		[Test]
                public void SingleToSqlSingle()
                {
                        Single TestSingle64 = 64;
                        AssertEquals ("#Y01", (float)64, ((SqlSingle)TestSingle64).Value);
                }
        }
}

