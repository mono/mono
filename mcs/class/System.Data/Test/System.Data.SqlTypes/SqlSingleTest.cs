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

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlSingleTest {

                // Test constructor
		[Test]
                public void Create()
                {
                        SqlSingle Test= new SqlSingle ((float)34.87);
                        SqlSingle Test2 = 45.2f;
                        
                        Assertion.AssertEquals ("#A01", 34.87f, Test.Value);
                        Assertion.AssertEquals ("#A02", 45.2f, Test2.Value);

                        Test = new SqlSingle (-9000.6543);
                        Assertion.AssertEquals ("#A03", -9000.6543f, Test.Value);
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        Assertion.AssertEquals ("#B01", 3.40282346638528859E+38f, 
				      SqlSingle.MaxValue.Value);
                        Assertion.AssertEquals ("#B02", -3.40282346638528859E+38f, 
				      SqlSingle.MinValue.Value);
                        Assertion.Assert ("#B03", SqlSingle.Null.IsNull);
                        Assertion.AssertEquals ("#B04", 0f, SqlSingle.Zero.Value);
                }

                // Test properties
		[Test]
                public void Properties()
                {
                        SqlSingle Test = new SqlSingle (5443e12f);
                        SqlSingle Test1 = new SqlSingle (1);

                        Assertion.Assert ("#C01", SqlSingle.Null.IsNull);
                        Assertion.AssertEquals ("#C02", 5443e12f, Test.Value);
                        Assertion.AssertEquals ("#C03", (float)1, Test1.Value);
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
                        Assertion.AssertEquals ("#D01A", 15E+18f, SqlSingle.Add (Test1, Test0).Value);
                        Assertion.AssertEquals ("#D02A", 1.5E+19f, SqlSingle.Add (Test1, Test2).Value);

                        try {			  
                                SqlSingle test = SqlSingle.Add (SqlSingle.MaxValue, 
							 SqlSingle.MaxValue);
                                Assertion.Fail ("#D03A");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#D04A", typeof (OverflowException), e.GetType ());
                        }
                        
                        // Divide()
                        Assertion.AssertEquals ("#D01B", (SqlSingle)3, SqlSingle.Divide (Test1, Test4));
                        Assertion.AssertEquals ("#D02B", -1.3E-23f, SqlSingle.Divide (Test2, Test3).Value);

                        try {
                                SqlSingle test = SqlSingle.Divide(Test1, Test0).Value;
                                Assertion.Fail ("#D03B");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#D04B", typeof (DivideByZeroException), 
					      e.GetType ());
                        }

			// Multiply()
                        Assertion.AssertEquals ("#D01D", (float)(7.5E+37), 
				      SqlSingle.Multiply (Test1, Test4).Value);
                        Assertion.AssertEquals ("#D02D", (float)0, SqlSingle.Multiply (Test1, Test0).Value);

                        try {
                                SqlSingle test = SqlSingle.Multiply (TestMax, Test1);
                                Assertion.Fail ("#D03D");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#D04D", typeof (OverflowException), e.GetType ());
                        }
                                

                        // Subtract()
                        Assertion.AssertEquals ("#D01F", (float)(-5E+30), 
				      SqlSingle.Subtract (Test1, Test3).Value);

                        try {
                                SqlSingle test = SqlSingle.Subtract(
					SqlSingle.MinValue, SqlSingle.MaxValue);
                                Assertion.Fail ("D02F");
                        } catch (Exception e) {			
                                Assertion.AssertEquals ("#D03F", typeof (OverflowException), e.GetType ());
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

                        Assertion.Assert ("#E01", Test1.CompareTo (Test3) > 0);
                        Assertion.Assert ("#E02", Test2.CompareTo (Test3) < 0);
                        Assertion.Assert ("#E03", Test1.CompareTo (Test11) == 0);
                        Assertion.Assert ("#E04", Test11.CompareTo (SqlSingle.Null) > 0);

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
                        SqlSingle Test0 = new SqlSingle (0);
                        SqlSingle Test1 = new SqlSingle (1.58e30);
                        SqlSingle Test2 = new SqlSingle (1.8e32);
                        SqlSingle Test22 = new SqlSingle (1.8e32);

                        Assertion.Assert ("#F01", !Test0.Equals (Test1));
                        Assertion.Assert ("#F02", !Test1.Equals (Test2));
                        Assertion.Assert ("#F03", !Test2.Equals (new SqlString ("TEST")));
                        Assertion.Assert ("#F04", Test2.Equals (Test22));

                        // Static Equals()-method
                        Assertion.Assert ("#F05", SqlSingle.Equals (Test2, Test22).Value);
                        Assertion.Assert ("#F06", !SqlSingle.Equals (Test1, Test2).Value);
                }

		[Test]
                public void GetHashCodeTest()
                {
                        SqlSingle Test15 = new SqlSingle (15);

                        // FIXME: Better way to test HashCode
                        Assertion.AssertEquals ("#G01", Test15.GetHashCode (), Test15.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
                        SqlSingle Test = new SqlSingle (84);
                        Assertion.AssertEquals ("#H01", "System.Data.SqlTypes.SqlSingle", 
				      Test.GetType ().ToString ());
                        Assertion.AssertEquals ("#H02", "System.Single", Test.Value.GetType ().ToString ());
                }

		[Test]
                public void Greaters()
                {
                        SqlSingle Test1 = new SqlSingle (1e10);
                        SqlSingle Test11 = new SqlSingle (1e10);
                        SqlSingle Test2 = new SqlSingle (64e14);

                        // GreateThan ()
                        Assertion.Assert ("#I01", !SqlSingle.GreaterThan (Test1, Test2).Value);
                        Assertion.Assert ("#I02", SqlSingle.GreaterThan (Test2, Test1).Value);
                        Assertion.Assert ("#I03", !SqlSingle.GreaterThan (Test1, Test11).Value);

                        // GreaterTharOrEqual ()
                        Assertion.Assert ("#I04", !SqlSingle.GreaterThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#I05", SqlSingle.GreaterThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#I06", SqlSingle.GreaterThanOrEqual (Test1, Test11).Value);
                }

		[Test]
                public void Lessers()
                {
                        SqlSingle Test1 = new SqlSingle(1.8e10);
                        SqlSingle Test11 = new SqlSingle (1.8e10);
                        SqlSingle Test2 = new SqlSingle (64e14);

                        // LessThan()
                        Assertion.Assert ("#J01", !SqlSingle.LessThan (Test1, Test11).Value);
                        Assertion.Assert ("#J02", !SqlSingle.LessThan (Test2, Test1).Value);
                        Assertion.Assert ("#J03", SqlSingle.LessThan (Test11, Test2).Value);

                        // LessThanOrEqual ()
                        Assertion.Assert ("#J04", SqlSingle.LessThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#J05", !SqlSingle.LessThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#J06", SqlSingle.LessThanOrEqual (Test11, Test1).Value);
                        Assertion.Assert ("#J07", SqlSingle.LessThanOrEqual (Test11, SqlSingle.Null).IsNull);
                }

		[Test]
                public void NotEquals()
                {
                        SqlSingle Test1 = new SqlSingle (12800000000001);
                        SqlSingle Test2 = new SqlSingle (128e10);
                        SqlSingle Test22 = new SqlSingle (128e10);

                        Assertion.Assert ("#K01", SqlSingle.NotEquals (Test1, Test2).Value);
                        Assertion.Assert ("#K02", SqlSingle.NotEquals (Test2, Test1).Value);
                        Assertion.Assert ("#K03", SqlSingle.NotEquals (Test22, Test1).Value);
                        Assertion.Assert ("#K04", !SqlSingle.NotEquals (Test22, Test2).Value);
                        Assertion.Assert ("#K05", !SqlSingle.NotEquals (Test2, Test22).Value);
                        Assertion.Assert ("#K06", SqlSingle.NotEquals (SqlSingle.Null, Test22).IsNull);
                        Assertion.Assert ("#K07", SqlSingle.NotEquals (SqlSingle.Null, Test22).IsNull);
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlSingle.Parse (null);
                                Assertion.Fail ("#L01");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#L02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlSingle.Parse ("not-a-number");
                                Assertion.Fail ("#L03");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#L04", typeof (FormatException), e.GetType ());
                        }

                         try {
                                SqlSingle.Parse ("9e44");
                                Assertion.Fail ("#L05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#L06", typeof (OverflowException), e.GetType ());
                        }

                        Assertion.AssertEquals("#L07", (float)150, SqlSingle.Parse ("150").Value);
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
                                SqlInt64 test = Test3.ToSqlInt64 ().Value;
                                Assertion.Fail ("#M03F");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M04F", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlMoney ()
                        Assertion.AssertEquals ("#M01G", (decimal)250, Test1.ToSqlMoney ().Value);
                        Assertion.AssertEquals ("#M02G", (decimal)0, Test0.ToSqlMoney ().Value);

                        try {
                                SqlMoney test = Test3.ToSqlMoney ().Value;
                                Assertion.Fail ("#M03G");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M04G", typeof (OverflowException), e.GetType ());
                        }        


                        // ToSqlString ()
                        Assertion.AssertEquals ("#M01H", "250", Test1.ToSqlString ().Value);
                        Assertion.AssertEquals ("#M02H", "0", Test0.ToSqlString ().Value);
                        Assertion.AssertEquals ("#M03H", "6,4E+17", Test2.ToSqlString ().Value);

                        // ToString ()
                        Assertion.AssertEquals ("#M01I", "250", Test1.ToString ());
                        Assertion.AssertEquals ("#M02I", "0", Test0.ToString ());
                        Assertion.AssertEquals ("#M03I", "6,4E+17", Test2.ToString ());
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
                        Assertion.AssertEquals ("#N01", (SqlSingle)3E+10, Test4 + Test5);
     
                        try {
                                SqlSingle test = SqlSingle.MaxValue + SqlSingle.MaxValue;
                                Assertion.Fail ("#N02");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N03", typeof (OverflowException), e.GetType ());
                        }

                        try {
                                SqlSingle test = SqlSingle.MaxValue + SqlSingle.MaxValue;
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N03a", typeof (OverflowException), e.GetType ());
                        }

                        // "/"-operator
                        Assertion.AssertEquals ("#N04", (SqlSingle)2, Test1 / Test3);

                        try {
                                SqlSingle test = Test3 / Test0;
                                Assertion.Fail ("#N05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N06", typeof (DivideByZeroException), e.GetType ());
                        }

                        // "*"-operator
                        Assertion.AssertEquals ("#N07", (SqlSingle)2E+20, Test4 * Test5);

                        try {
                                SqlSingle test = SqlSingle.MaxValue * Test1;
                                Assertion.Fail ("#N08");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N09", typeof (OverflowException), e.GetType ());
                        }

                        // "-"-operator
                        Assertion.AssertEquals ("#N10", (SqlSingle)12e11, Test1 - Test3);

                        try {
                                SqlSingle test = SqlSingle.MinValue - SqlSingle.MaxValue;
                                Assertion.Fail ("#N11");
                        } catch  (Exception e) {
                                Assertion.AssertEquals ("#N12", typeof (OverflowException), e.GetType ());
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
                        Assertion.Assert ("#O01", (Test2 == Test22).Value);
                        Assertion.Assert ("#O02", !(Test1 == Test2).Value);
                        Assertion.Assert ("#O03", (Test1 == SqlSingle.Null).IsNull);
                        
                        // != -operator
                        Assertion.Assert ("#O04", !(Test2 != Test22).Value);
                        Assertion.Assert ("#O05", (Test2 != Test3).Value);
                        Assertion.Assert ("#O06", (Test1 != Test3).Value);
                        Assertion.Assert ("#O07", (Test1 != SqlSingle.Null).IsNull);

                        // > -operator
                        Assertion.Assert ("#O08", (Test1 > Test2).Value);
                        Assertion.Assert ("#O09", !(Test1 > Test3).Value);
                        Assertion.Assert ("#O10", !(Test2 > Test22).Value);
                        Assertion.Assert ("#O11", (Test1 > SqlSingle.Null).IsNull);

                        // >=  -operator
                        Assertion.Assert ("#O12", !(Test1 >= Test3).Value);
                        Assertion.Assert ("#O13", (Test3 >= Test1).Value);
                        Assertion.Assert ("#O14", (Test2 >= Test22).Value);
                        Assertion.Assert ("#O15", (Test1 >= SqlSingle.Null).IsNull);

                        // < -operator
                        Assertion.Assert ("#O16", !(Test1 < Test2).Value);
                        Assertion.Assert ("#O17", (Test1 < Test3).Value);
                        Assertion.Assert ("#O18", !(Test2 < Test22).Value);
                        Assertion.Assert ("#O19", (Test1 < SqlSingle.Null).IsNull);

                        // <= -operator
                        Assertion.Assert ("#O20", (Test1 <= Test3).Value);
                        Assertion.Assert ("#O21", !(Test3 <= Test1).Value);
                        Assertion.Assert ("#O22", (Test2 <= Test22).Value);
                        Assertion.Assert ("#O23", (Test1 <= SqlSingle.Null).IsNull);
                }

		[Test]
                public void UnaryNegation()
                {
                        SqlSingle Test = new SqlSingle (2000000001);
                        SqlSingle TestNeg = new SqlSingle (-3000);

                        SqlSingle Result = -Test;
                        Assertion.AssertEquals ("#P01", (float)(-2000000001), Result.Value);

                        Result = -TestNeg;
                        Assertion.AssertEquals ("#P02", (float)3000, Result.Value);
                }

		[Test]
                public void SqlBooleanToSqlSingle()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);
                        SqlSingle Result;

                        Result = (SqlSingle)TestBoolean;

                        Assertion.AssertEquals ("#Q01", (float)1, Result.Value);

                        Result = (SqlSingle)SqlBoolean.Null;
                        Assertion.Assert ("#Q02", Result.IsNull);
                }

		[Test]
		public void SqlDoubleToSqlSingle()
	        {
                        SqlDouble Test = new SqlDouble (12e12);
			SqlSingle TestSqlSingle = (SqlSingle)Test;
			Assertion.AssertEquals ("R01", 12e12f, TestSqlSingle.Value);
		}

		[Test]
                public void SqlSingleToSingle()
                {
                        SqlSingle Test = new SqlSingle (12e12);
                        Single Result = (Single)Test;
                        Assertion.AssertEquals ("#S01", 12e12f, Result);
                }

		[Test]
                public void SqlStringToSqlSingle()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");

                        Assertion.AssertEquals ("#T01", (float)100, ((SqlSingle)TestString100).Value);

                        try {
                                SqlSingle test = (SqlSingle)TestString;
                                Assertion.Fail ("#T02");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#T03", typeof (FormatException), e.GetType ());
                        }
                }

		[Test]
                public void ByteToSqlSingle()
                {
                        short TestShort = 14;
                        Assertion.AssertEquals ("#U01", (float)14, ((SqlSingle)TestShort).Value);
                }
                
		[Test]
                public void SqlDecimalToSqlSingle()
                {
                        SqlDecimal TestDecimal64 = new SqlDecimal (64);

                        Assertion.AssertEquals ("#V01", (float)64, ((SqlSingle)TestDecimal64).Value);
                        Assertion.AssertEquals ("#V02", SqlSingle.Null, ((SqlSingle)SqlDecimal.Null));
                }

		[Test]
                public void SqlIntToSqlSingle()
                {
                        SqlInt16 Test64 = new SqlInt16 (64);
                        SqlInt32 Test640 = new SqlInt32 (640);
                        SqlInt64 Test64000 = new SqlInt64 (64000);
                        Assertion.AssertEquals ("#W01", (float)64, ((SqlSingle)Test64).Value);
                        Assertion.AssertEquals ("#W02", (float)640, ((SqlSingle)Test640).Value);
                        Assertion.AssertEquals ("#W03", (float)64000, ((SqlSingle)Test64000).Value);
                }

		[Test]
                public void SqlMoneyToSqlSingle()
                {
                        SqlMoney TestMoney64 = new SqlMoney(64);
                        Assertion.AssertEquals ("#X01", (float)64, ((SqlSingle)TestMoney64).Value);
                }

		[Test]
                public void SingleToSqlSingle()
                {
                        Single TestSingle64 = 64;
                        Assertion.AssertEquals ("#Y01", (float)64, ((SqlSingle)TestSingle64).Value);
                }
        }
}

