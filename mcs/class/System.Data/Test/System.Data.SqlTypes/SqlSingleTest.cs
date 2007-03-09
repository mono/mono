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

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlSingleTest
	{

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
                        
                        Assert.AreEqual (34.87f, Test.Value, "#A01");
                        Assert.AreEqual (45.2f, Test2.Value, "#A02");

                        Test = new SqlSingle (-9000.6543);
                        Assert.AreEqual (-9000.6543f, Test.Value, "#A03");
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        Assert.AreEqual (3.40282346638528859E+38f, 
				      SqlSingle.MaxValue.Value, "#B01");
                        Assert.AreEqual (-3.40282346638528859E+38f, 
				      SqlSingle.MinValue.Value, "#B02");
                        Assert.IsTrue (SqlSingle.Null.IsNull, "#B03");
                        Assert.AreEqual (0f, SqlSingle.Zero.Value, "#B04");
                }

                // Test properties
		[Test]
                public void Properties()
                {
                        SqlSingle Test = new SqlSingle (5443e12f);
                        SqlSingle Test1 = new SqlSingle (1);

                        Assert.IsTrue (SqlSingle.Null.IsNull, "#C01");
                        Assert.AreEqual (5443e12f, Test.Value, "#C02");
                        Assert.AreEqual ((float)1, Test1.Value, "#C03");
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
                        Assert.AreEqual (15E+18f, SqlSingle.Add (Test1, Test0).Value, "#D01A");
                        Assert.AreEqual (1.5E+19f, SqlSingle.Add (Test1, Test2).Value, "#D02A");

                        try {			  
                                SqlSingle test = SqlSingle.Add (SqlSingle.MaxValue, 
							 SqlSingle.MaxValue);
                                Assert.Fail ("#D03A");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D04A");
                        }
                        
                        // Divide()
                        Assert.AreEqual ((SqlSingle)3, SqlSingle.Divide (Test1, Test4), "#D01B");
                        Assert.AreEqual (-1.3E-23f, SqlSingle.Divide (Test2, Test3).Value, "#D02B");

                        try {
                                SqlSingle test = SqlSingle.Divide(Test1, Test0).Value;
                                Assert.Fail ("#D03B");
                        } catch(Exception e) {
                                Assert.AreEqual (typeof (DivideByZeroException), 
					      e.GetType (), "#D04B");
                        }

			// Multiply()
                        Assert.AreEqual ((float)(7.5E+37), 
				      SqlSingle.Multiply (Test1, Test4).Value, "#D01D");
                        Assert.AreEqual ((float)0, SqlSingle.Multiply (Test1, Test0).Value, "#D02D");

                        try {
                                SqlSingle test = SqlSingle.Multiply (TestMax, Test1);
                                Assert.Fail ("#D03D");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D04D");
                        }
                                

                        // Subtract()
                        Assert.AreEqual ((float)(-5E+30), 
				      SqlSingle.Subtract (Test1, Test3).Value, "#D01F");

                        try {
                                SqlSingle test = SqlSingle.Subtract(
					SqlSingle.MinValue, SqlSingle.MaxValue);
                                Assert.Fail ("D02F");
                        } catch (Exception e) {			
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D03F");
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

                        Assert.IsTrue (Test1.CompareTo (Test3) > 0, "#E01");
                        Assert.IsTrue (Test2.CompareTo (Test3) < 0, "#E02");
                        Assert.IsTrue (Test1.CompareTo (Test11) == 0, "#E03");
                        Assert.IsTrue (Test11.CompareTo (SqlSingle.Null) > 0, "#E04");

                        try {
                                Test1.CompareTo (TestString);
                                Assert.Fail("#E05");
                        } catch(Exception e) {
                                Assert.AreEqual (typeof (ArgumentException), e.GetType (), "#E06");
                        }
                }

		[Test]
                public void EqualsMethods()
                {
                        SqlSingle Test0 = new SqlSingle (0);
                        SqlSingle Test1 = new SqlSingle (1.58e30);
                        SqlSingle Test2 = new SqlSingle (1.8e32);
                        SqlSingle Test22 = new SqlSingle (1.8e32);

                        Assert.IsTrue (!Test0.Equals (Test1), "#F01");
                        Assert.IsTrue (!Test1.Equals (Test2), "#F02");
                        Assert.IsTrue (!Test2.Equals (new SqlString ("TEST")), "#F03");
                        Assert.IsTrue (Test2.Equals (Test22), "#F04");

                        // Static Equals()-method
                        Assert.IsTrue (SqlSingle.Equals (Test2, Test22).Value, "#F05");
                        Assert.IsTrue (!SqlSingle.Equals (Test1, Test2).Value, "#F06");
                }

		[Test]
                public void GetHashCodeTest()
                {
                        SqlSingle Test15 = new SqlSingle (15);

                        // FIXME: Better way to test HashCode
                        Assert.AreEqual (Test15.GetHashCode (), Test15.GetHashCode (), "#G01");
                }

		[Test]
                public void GetTypeTest()
                {
                        SqlSingle Test = new SqlSingle (84);
                        Assert.AreEqual ("System.Data.SqlTypes.SqlSingle", 
				      Test.GetType ().ToString (), "#H01");
                        Assert.AreEqual ("System.Single", Test.Value.GetType ().ToString (), "#H02");
                }

		[Test]
                public void Greaters()
                {
                        SqlSingle Test1 = new SqlSingle (1e10);
                        SqlSingle Test11 = new SqlSingle (1e10);
                        SqlSingle Test2 = new SqlSingle (64e14);

                        // GreateThan ()
                        Assert.IsTrue (!SqlSingle.GreaterThan (Test1, Test2).Value, "#I01");
                        Assert.IsTrue (SqlSingle.GreaterThan (Test2, Test1).Value, "#I02");
                        Assert.IsTrue (!SqlSingle.GreaterThan (Test1, Test11).Value, "#I03");

                        // GreaterTharOrEqual ()
                        Assert.IsTrue (!SqlSingle.GreaterThanOrEqual (Test1, Test2).Value, "#I04");
                        Assert.IsTrue (SqlSingle.GreaterThanOrEqual (Test2, Test1).Value, "#I05");
                        Assert.IsTrue (SqlSingle.GreaterThanOrEqual (Test1, Test11).Value, "#I06");
                }

		[Test]
                public void Lessers()
                {
                        SqlSingle Test1 = new SqlSingle(1.8e10);
                        SqlSingle Test11 = new SqlSingle (1.8e10);
                        SqlSingle Test2 = new SqlSingle (64e14);

                        // LessThan()
                        Assert.IsTrue (!SqlSingle.LessThan (Test1, Test11).Value, "#J01");
                        Assert.IsTrue (!SqlSingle.LessThan (Test2, Test1).Value, "#J02");
                        Assert.IsTrue (SqlSingle.LessThan (Test11, Test2).Value, "#J03");

                        // LessThanOrEqual ()
                        Assert.IsTrue (SqlSingle.LessThanOrEqual (Test1, Test2).Value, "#J04");
                        Assert.IsTrue (!SqlSingle.LessThanOrEqual (Test2, Test1).Value, "#J05");
                        Assert.IsTrue (SqlSingle.LessThanOrEqual (Test11, Test1).Value, "#J06");
                        Assert.IsTrue (SqlSingle.LessThanOrEqual (Test11, SqlSingle.Null).IsNull, "#J07");
                }

		[Test]
                public void NotEquals()
                {
                        SqlSingle Test1 = new SqlSingle (12800000000001);
                        SqlSingle Test2 = new SqlSingle (128e10);
                        SqlSingle Test22 = new SqlSingle (128e10);

                        Assert.IsTrue (SqlSingle.NotEquals (Test1, Test2).Value, "#K01");
                        Assert.IsTrue (SqlSingle.NotEquals (Test2, Test1).Value, "#K02");
                        Assert.IsTrue (SqlSingle.NotEquals (Test22, Test1).Value, "#K03");
                        Assert.IsTrue (!SqlSingle.NotEquals (Test22, Test2).Value, "#K04");
                        Assert.IsTrue (!SqlSingle.NotEquals (Test2, Test22).Value, "#K05");
                        Assert.IsTrue (SqlSingle.NotEquals (SqlSingle.Null, Test22).IsNull, "#K06");
                        Assert.IsTrue (SqlSingle.NotEquals (SqlSingle.Null, Test22).IsNull, "#K07");
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlSingle.Parse (null);
                                Assert.Fail ("#L01");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "#L02");
                        }

                        try {
                                SqlSingle.Parse ("not-a-number");
                                Assert.Fail ("#L03");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (FormatException), e.GetType (), "#L04");
                        }

                         try {
                                SqlSingle.Parse ("9e44");
                                Assert.Fail ("#L05");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#L06");
                        }

                        Assert.AreEqual((float)150, SqlSingle.Parse ("150").Value, "#L07");
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
                        Assert.IsTrue (Test1.ToSqlBoolean ().Value, "#M01A");
                        Assert.IsTrue (!Test0.ToSqlBoolean ().Value, "#M02A");
                        Assert.IsTrue (TestNull.ToSqlBoolean ().IsNull, "#M03A");

                        // ToSqlByte ()
                        Assert.AreEqual ((byte)250, Test1.ToSqlByte ().Value, "#M01B");
                        Assert.AreEqual ((byte)0, Test0.ToSqlByte ().Value, "#M02B");

                        try {
                                SqlByte b = (byte)Test2.ToSqlByte ();
                                Assert.Fail ("#M03B");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04B");
                        }

                        // ToSqlDecimal ()
                        Assert.AreEqual (250.00000000000000M, Test1.ToSqlDecimal ().Value, "#M01C");
                        Assert.AreEqual ((decimal)0, Test0.ToSqlDecimal ().Value, "#M02C");

                        try {
                                SqlDecimal test = Test3.ToSqlDecimal ().Value;
                                Assert.Fail ("#M03C");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04C");
                        }      

                        // ToSqlInt16 ()
                        Assert.AreEqual ((short)250, Test1.ToSqlInt16 ().Value, "#M01D");
                        Assert.AreEqual ((short)0, Test0.ToSqlInt16 ().Value, "#M02D");

                        try {
                                SqlInt16 test = Test2.ToSqlInt16().Value;
                                Assert.Fail ("#M03D");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04D");
                        }        

                        // ToSqlInt32 ()
                        Assert.AreEqual ((int)250, Test1.ToSqlInt32 ().Value, "#M01E");
                        Assert.AreEqual ((int)0, Test0.ToSqlInt32 ().Value, "#M02E");

                        try {
                                SqlInt32 test = Test2.ToSqlInt32 ().Value;
                                Assert.Fail ("#M03E");
                        } catch (Exception e) { 
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04E");
                        }

                        // ToSqlInt64 ()
                        Assert.AreEqual ((long)250, Test1.ToSqlInt64 ().Value, "#M01F");
                        Assert.AreEqual ((long)0, Test0.ToSqlInt64 ().Value, "#M02F");

                        try {        
                                SqlInt64 test = Test3.ToSqlInt64 ().Value;
                                Assert.Fail ("#M03F");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04F");
                        }        

                        // ToSqlMoney ()
                        Assert.AreEqual (250.0000M, Test1.ToSqlMoney ().Value, "#M01G");
                        Assert.AreEqual ((decimal)0, Test0.ToSqlMoney ().Value, "#M02G");

                        try {
                                SqlMoney test = Test3.ToSqlMoney ().Value;
                                Assert.Fail ("#M03G");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04G");
                        }        


                        // ToSqlString ()
                        Assert.AreEqual ("250", Test1.ToSqlString ().Value, "#M01H");
                        Assert.AreEqual ("0", Test0.ToSqlString ().Value, "#M02H");
                        Assert.AreEqual ("6.4E+17", Test2.ToSqlString ().Value, "#M03H");

                        // ToString ()
                        Assert.AreEqual ("250", Test1.ToString (), "#M01I");
                        Assert.AreEqual ("0", Test0.ToString (), "#M02I");
                        Assert.AreEqual ("6.4E+17", Test2.ToString (), "#M03I");
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
                        Assert.AreEqual ((SqlSingle)3E+10, Test4 + Test5, "#N01");
     
                        try {
                                SqlSingle test = SqlSingle.MaxValue + SqlSingle.MaxValue;
                                Assert.Fail ("#N02");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N03");
                        }

                        try {
                                SqlSingle test = SqlSingle.MaxValue + SqlSingle.MaxValue;
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N03a");
                        }

                        // "/"-operator
                        Assert.AreEqual ((SqlSingle)2, Test1 / Test3, "#N04");

                        try {
                                SqlSingle test = Test3 / Test0;
                                Assert.Fail ("#N05");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (DivideByZeroException), e.GetType (), "#N06");
                        }

                        // "*"-operator
                        Assert.AreEqual ((SqlSingle)2E+20, Test4 * Test5, "#N07");

                        try {
                                SqlSingle test = SqlSingle.MaxValue * Test1;
                                Assert.Fail ("#N08");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N09");
                        }

                        // "-"-operator
                        Assert.AreEqual ((SqlSingle)12e11, Test1 - Test3, "#N10");

                        try {
                                SqlSingle test = SqlSingle.MinValue - SqlSingle.MaxValue;
                                Assert.Fail ("#N11");
                        } catch  (Exception e) {
                                Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N12");
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
                        Assert.IsTrue ((Test2 == Test22).Value, "#O01");
                        Assert.IsTrue (!(Test1 == Test2).Value, "#O02");
                        Assert.IsTrue ((Test1 == SqlSingle.Null).IsNull, "#O03");
                        
                        // != -operator
                        Assert.IsTrue (!(Test2 != Test22).Value, "#O04");
                        Assert.IsTrue ((Test2 != Test3).Value, "#O05");
                        Assert.IsTrue ((Test1 != Test3).Value, "#O06");
                        Assert.IsTrue ((Test1 != SqlSingle.Null).IsNull, "#O07");

                        // > -operator
                        Assert.IsTrue ((Test1 > Test2).Value, "#O08");
                        Assert.IsTrue (!(Test1 > Test3).Value, "#O09");
                        Assert.IsTrue (!(Test2 > Test22).Value, "#O10");
                        Assert.IsTrue ((Test1 > SqlSingle.Null).IsNull, "#O11");

                        // >=  -operator
                        Assert.IsTrue (!(Test1 >= Test3).Value, "#O12");
                        Assert.IsTrue ((Test3 >= Test1).Value, "#O13");
                        Assert.IsTrue ((Test2 >= Test22).Value, "#O14");
                        Assert.IsTrue ((Test1 >= SqlSingle.Null).IsNull, "#O15");

                        // < -operator
                        Assert.IsTrue (!(Test1 < Test2).Value, "#O16");
                        Assert.IsTrue ((Test1 < Test3).Value, "#O17");
                        Assert.IsTrue (!(Test2 < Test22).Value, "#O18");
                        Assert.IsTrue ((Test1 < SqlSingle.Null).IsNull, "#O19");

                        // <= -operator
                        Assert.IsTrue ((Test1 <= Test3).Value, "#O20");
                        Assert.IsTrue (!(Test3 <= Test1).Value, "#O21");
                        Assert.IsTrue ((Test2 <= Test22).Value, "#O22");
                        Assert.IsTrue ((Test1 <= SqlSingle.Null).IsNull, "#O23");
                }

		[Test]
                public void UnaryNegation()
                {
                        SqlSingle Test = new SqlSingle (2000000001);
                        SqlSingle TestNeg = new SqlSingle (-3000);

                        SqlSingle Result = -Test;
                        Assert.AreEqual ((float)(-2000000001), Result.Value, "#P01");

                        Result = -TestNeg;
                        Assert.AreEqual ((float)3000, Result.Value, "#P02");
                }

		[Test]
                public void SqlBooleanToSqlSingle()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);
                        SqlSingle Result;

                        Result = (SqlSingle)TestBoolean;

                        Assert.AreEqual ((float)1, Result.Value, "#Q01");

                        Result = (SqlSingle)SqlBoolean.Null;
                        Assert.IsTrue (Result.IsNull, "#Q02");
                }

		[Test]
		public void SqlDoubleToSqlSingle()
	        {
                        SqlDouble Test = new SqlDouble (12e12);
			SqlSingle TestSqlSingle = (SqlSingle)Test;
			Assert.AreEqual (12e12f, TestSqlSingle.Value, "R01");
		}

		[Test]
                public void SqlSingleToSingle()
                {
                        SqlSingle Test = new SqlSingle (12e12);
                        Single Result = (Single)Test;
                        Assert.AreEqual (12e12f, Result, "#S01");
                }

		[Test]
                public void SqlStringToSqlSingle()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");

                        Assert.AreEqual ((float)100, ((SqlSingle)TestString100).Value, "#T01");

                        try {
                                SqlSingle test = (SqlSingle)TestString;
                                Assert.Fail ("#T02");
                        } catch(Exception e) {
                                Assert.AreEqual (typeof (FormatException), e.GetType (), "#T03");
                        }
                }

		[Test]
                public void ByteToSqlSingle()
                {
                        short TestShort = 14;
                        Assert.AreEqual ((float)14, ((SqlSingle)TestShort).Value, "#U01");
                }
                
		[Test]
                public void SqlDecimalToSqlSingle()
                {
                        SqlDecimal TestDecimal64 = new SqlDecimal (64);

                        Assert.AreEqual ((float)64, ((SqlSingle)TestDecimal64).Value, "#V01");
                        Assert.AreEqual (SqlSingle.Null, ((SqlSingle)SqlDecimal.Null), "#V02");
                }

		[Test]
                public void SqlIntToSqlSingle()
                {
                        SqlInt16 Test64 = new SqlInt16 (64);
                        SqlInt32 Test640 = new SqlInt32 (640);
                        SqlInt64 Test64000 = new SqlInt64 (64000);
                        Assert.AreEqual ((float)64, ((SqlSingle)Test64).Value, "#W01");
                        Assert.AreEqual ((float)640, ((SqlSingle)Test640).Value, "#W02");
                        Assert.AreEqual ((float)64000, ((SqlSingle)Test64000).Value, "#W03");
                }

		[Test]
                public void SqlMoneyToSqlSingle()
                {
                        SqlMoney TestMoney64 = new SqlMoney(64);
                        Assert.AreEqual ((float)64, ((SqlSingle)TestMoney64).Value, "#X01");
                }

		[Test]
                public void SingleToSqlSingle()
                {
                        Single TestSingle64 = 64;
                        Assert.AreEqual ((float)64, ((SqlSingle)TestSingle64).Value, "#Y01");
                }
#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlSingle.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("float", qualifiedName.Name, "#A01");
		}
#endif
        }
}

