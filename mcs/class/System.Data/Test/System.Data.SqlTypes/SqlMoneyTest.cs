//
// SqlMoneyTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlMoney
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
        public class SqlMoneyTest 
	{

	        private SqlMoney Test1;
		private SqlMoney Test2;
		private SqlMoney Test3;
		private SqlMoney Test4;

		[SetUp]
                public void GetReady() 
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");			
			Test1 = new SqlMoney (6464.6464d);
			Test2 = new SqlMoney (90000.0m);
			Test3 = new SqlMoney (90000.0m);
			Test4 = new SqlMoney (-45000.0m);
		}

                // Test constructor
		[Test]
                public void Create()
                {
			try {
				SqlMoney Test = new SqlMoney (1000000000000000m);
				Assert.Fail ("#B01");
			} catch (Exception e) {
				Assert.AreEqual ( typeof (OverflowException),
					      e.GetType (), "#A02");
			}

                        SqlMoney CreationTest = new SqlMoney ((decimal)913.3);
			Assert.AreEqual ( 913.3000m, CreationTest.Value, "A03");

			try {
				SqlMoney Test = new SqlMoney (1e200);
				Assert.Fail ("#B04");
			} catch (Exception e) {
				Assert.AreEqual ( typeof (OverflowException),
					      e.GetType (), "#A05");
			}
                        
                        SqlMoney CreationTest2 = new SqlMoney ((double)913.3);
			Assert.AreEqual ( 913.3000m, CreationTest2.Value, "A06");

                        SqlMoney CreationTest3 = new SqlMoney ((int)913);
			Assert.AreEqual ( 913.0000m, CreationTest3.Value, "A07");

                        SqlMoney CreationTest4 = new SqlMoney ((long)913.3);
                        Assert.AreEqual ( 913.0000m, CreationTest4.Value, "A08");
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        // FIXME: There is a error in msdn docs, it says thath MaxValue
                        // is 922,337,203,685,475.5807 when the actual value is
                        //    922,337,203,685,477.5807
                        Assert.AreEqual ( 922337203685477.5807m, SqlMoney.MaxValue.Value, "#B01");
                        Assert.AreEqual ( -922337203685477.5808m, SqlMoney.MinValue.Value, "#B02");
                        Assert.IsTrue (SqlMoney.Null.IsNull, "#B03");
                        Assert.AreEqual ( 0m, SqlMoney.Zero.Value, "#B04");
                }

                // Test properties
		[Test]
                public void Properties()
                {
			Assert.AreEqual ( 90000.0000m, Test2.Value, "#C01");
                        Assert.AreEqual ( -45000.0000m, Test4.Value, "#C02");
			Assert.IsTrue (SqlMoney.Null.IsNull, "#C03");
                }

                // PUBLIC METHODS

		[Test]
                public void ArithmeticMethods()
                {
			SqlMoney TestMoney2 = new SqlMoney (2);

			// Add
                        Assert.AreEqual ( (SqlMoney)96464.6464m, SqlMoney.Add (Test1, Test2), "#D01");
                        Assert.AreEqual ( (SqlMoney)180000m, SqlMoney.Add (Test2, Test2), "#D02");
                        Assert.AreEqual ( (SqlMoney)45000m, SqlMoney.Add (Test2, Test4), "#D03");
			
			try {
				SqlMoney test = SqlMoney.Add(SqlMoney.MaxValue, Test2);
				Assert.Fail ("#D04");
			} catch (Exception e) {
				Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#D05");
			}

			// Divide
                        Assert.AreEqual ( (SqlMoney)45000m, SqlMoney.Divide (Test2, TestMoney2), "#D06");
			try {
				SqlMoney test = SqlMoney.Divide (Test2, SqlMoney.Zero);
				Assert.Fail ("#D07");
			} catch (Exception e) {
				Assert.AreEqual ( typeof (DivideByZeroException), 
					      e.GetType(), "#D08");
			}
				     			
			// Multiply
                        Assert.AreEqual ( (SqlMoney)581818176m, SqlMoney.Multiply (Test1, Test2), "#D09");
                        Assert.AreEqual ( (SqlMoney)(-4050000000m), SqlMoney.Multiply (Test3, Test4), "#D10");

			try {
				SqlMoney test = SqlMoney.Multiply (SqlMoney.MaxValue, Test2);
				Assert.Fail ("#D11");
			} catch (Exception e) {
				Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#D12");
			}
				      
			// Subtract
                        Assert.AreEqual ( (SqlMoney)0m, SqlMoney.Subtract (Test2, Test3), "#D13");
                        Assert.AreEqual ( (SqlMoney)83535.3536m, SqlMoney.Subtract (Test2, Test1), "#D14");
			
			try {
				SqlMoney test = SqlMoney.Subtract (SqlMoney.MinValue, Test2);
			} catch (Exception e) {
				Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#D15");
			}
                }

		[Test]
                public void CompareTo()
		{
			Assert.IsTrue (Test1.CompareTo (Test2) < 0, "#E01");
 			Assert.IsTrue (Test3.CompareTo (Test1) > 0, "#E02");
 			Assert.IsTrue (Test3.CompareTo (Test2) == 0, "#E03");
                        Assert.IsTrue (Test3.CompareTo (SqlMoney.Null) > 0, "#E04");
                }

		[Test]
                public void EqualsMethods()
                {
			Assert.IsTrue (!Test1.Equals (Test2), "#F01");
			Assert.IsTrue (Test2.Equals (Test3), "#F02");
			Assert.IsTrue (!SqlMoney.Equals (Test1, Test2).Value, "#F03");
			Assert.IsTrue (SqlMoney.Equals (Test3, Test2).Value, "#F04");
                }

		[Test]
                public void GetHashCodeTest()
                {
                        // FIXME: Better way to test HashCode
                        Assert.AreEqual ( Test3.GetHashCode (), Test2.GetHashCode (), "#G01");
                        Assert.IsTrue (Test2.GetHashCode () !=  Test1.GetHashCode (), "#G02");
                }

		[Test]
                public void GetTypeTest()
                {
			Assert.AreEqual ( "System.Data.SqlTypes.SqlMoney", 
				      Test1.GetType ().ToString (), "#H01");
		}

		[Test]
                public void Greaters()
                {
                        // GreateThan ()
                        Assert.IsTrue (!SqlMoney.GreaterThan (Test1, Test2).Value, "#I01");
                        Assert.IsTrue (SqlMoney.GreaterThan (Test2, Test1).Value, "#I02");
                        Assert.IsTrue (!SqlMoney.GreaterThan (Test2, Test3).Value, "#I03");
                        Assert.IsTrue (SqlMoney.GreaterThan (Test2, SqlMoney.Null).IsNull, "#I04");

                        // GreaterTharOrEqual ()
                        Assert.IsTrue (!SqlMoney.GreaterThanOrEqual (Test1, Test2).Value, "#I05");
                        Assert.IsTrue (SqlMoney.GreaterThanOrEqual (Test2, Test1).Value, "#I06");
                        Assert.IsTrue (SqlMoney.GreaterThanOrEqual (Test3, Test2).Value, "#I07");
                        Assert.IsTrue (SqlMoney.GreaterThanOrEqual (Test3, SqlMoney.Null).IsNull, "#I08");
                }

		[Test]
                public void Lessers()
                {
                        // LessThan()
                        Assert.IsTrue (!SqlMoney.LessThan (Test2, Test3).Value, "#J01");
                        Assert.IsTrue (!SqlMoney.LessThan (Test2, Test1).Value, "#J02");
                        Assert.IsTrue (SqlMoney.LessThan (Test1, Test2).Value, "#J03");
                        Assert.IsTrue (SqlMoney.LessThan (SqlMoney.Null, Test2).IsNull, "#J04");

                        // LessThanOrEqual ()
                        Assert.IsTrue (SqlMoney.LessThanOrEqual (Test1, Test2).Value, "#J05");
                        Assert.IsTrue (!SqlMoney.LessThanOrEqual (Test2, Test1).Value, "#J06");
                        Assert.IsTrue (SqlMoney.LessThanOrEqual (Test2, Test2).Value, "#J07");
                        Assert.IsTrue (SqlMoney.LessThanOrEqual (Test2, SqlMoney.Null).IsNull, "#J08");
                }

		[Test]
                public void NotEquals()
                {
                        Assert.IsTrue (SqlMoney.NotEquals (Test1, Test2).Value, "#K01");
                        Assert.IsTrue (SqlMoney.NotEquals (Test2, Test1).Value, "#K02");
                        Assert.IsTrue (!SqlMoney.NotEquals (Test2, Test3).Value, "#K03");
                        Assert.IsTrue (!SqlMoney.NotEquals (Test3, Test2).Value, "#K04");
                        Assert.IsTrue (SqlMoney.NotEquals (SqlMoney.Null, Test2).IsNull, "#K05");
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlMoney.Parse (null);
                                Assert.Fail ("#L01");
                        } catch (Exception e) {
                                Assert.AreEqual ( typeof (ArgumentNullException), e.GetType (), "#L02");
                        }

                        try {
                                SqlMoney.Parse ("not-a-number");
                                Assert.Fail ("#L03");
                        } catch (Exception e) {

                                Assert.AreEqual ( typeof (FormatException), e.GetType (), "#L04");
                        }

                         try {
                                SqlMoney.Parse ("1000000000000000");
                                Assert.Fail ("#L05");
                        } catch (Exception e) {
                                Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#L06");
                        }

                        Assert.AreEqual( 150.0000M, SqlMoney.Parse ("150").Value, "#L07");
                }

		[Test]
                public void Conversions()
                {		      
			SqlMoney TestMoney100 = new SqlMoney (100);

			// ToDecimal
			Assert.AreEqual ( (decimal)6464.6464, Test1.ToDecimal (), "#M01");

			// ToDouble
			Assert.AreEqual ( (double)6464.6464, Test1.ToDouble (), "#M02");

			// ToInt32
			Assert.AreEqual ( (int)90000, Test2.ToInt32 (), "#M03");
                        Assert.AreEqual ( (int)6465, Test1.ToInt32 (), "#M04");

			// ToInt64
                        Assert.AreEqual ( (long)90000, Test2.ToInt64 (), "#M05");
                        Assert.AreEqual ( (long)6465, Test1.ToInt64 (), "#M06");

                        // ToSqlBoolean ()
                        Assert.IsTrue (Test1.ToSqlBoolean ().Value, "#M07");
                        Assert.IsTrue (!SqlMoney.Zero.ToSqlBoolean ().Value, "#M08");
                        Assert.IsTrue (SqlMoney.Null.ToSqlBoolean ().IsNull, "#M09");

                        // ToSqlByte ()
                        Assert.AreEqual ( (byte)100, TestMoney100.ToSqlByte ().Value, "#M10");

                        try {
                                SqlByte b = (byte)Test2.ToSqlByte ();
                                Assert.Fail ("#M11");
                        } catch (Exception e) {
                                Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#M12");
                        }

                        // ToSqlDecimal ()
                        Assert.AreEqual ( (decimal)6464.6464, Test1.ToSqlDecimal ().Value, "#M13");
                        Assert.AreEqual ( -45000.0000m, Test4.ToSqlDecimal ().Value, "#M14");

                        // ToSqlInt16 ()
                        Assert.AreEqual ( (short)6465, Test1.ToSqlInt16 ().Value, "#M15");

                        try {
                                SqlInt16 test = SqlMoney.MaxValue.ToSqlInt16().Value;
                                Assert.Fail ("#M17");
                        } catch (Exception e) {
                                Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#M18");
                        }        

                        // ToSqlInt32 ()
                        Assert.AreEqual ( (int)6465, Test1.ToSqlInt32 ().Value, "#M19");
                        Assert.AreEqual ( (int)(-45000), Test4.ToSqlInt32 ().Value, "#M20");

                        try {
                                SqlInt32 test = SqlMoney.MaxValue.ToSqlInt32 ().Value;
                                Assert.Fail ("#M21");
                        } catch (Exception e) { 
                                Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#M22");
                        }

                        // ToSqlInt64 ()
                        Assert.AreEqual ( (long)6465, Test1.ToSqlInt64 ().Value, "#M23");
                        Assert.AreEqual ( (long)(-45000), Test4.ToSqlInt64 ().Value, "#M24");

                        // ToSqlSingle ()
                        Assert.AreEqual ( (float)6464.6464, Test1.ToSqlSingle ().Value, "#M25");

                        // ToSqlString ()
                        Assert.AreEqual ( "6464.6464", Test1.ToSqlString ().Value, "#M26");
                        Assert.AreEqual ( "90000.0000", Test2.ToSqlString ().Value, "#M27");

                        // ToString ()
                        Assert.AreEqual ( "6464.6464", Test1.ToString (), "#M28");
                        Assert.AreEqual ( "90000.0000", Test2.ToString (), "#M29");
                }

                // OPERATORS

		[Test]
                public void ArithmeticOperators()
                {
                        // "+"-operator
                        Assert.AreEqual ( (SqlMoney)96464.6464m, Test1 + Test2, "#N01");
     
                        try {
                                SqlMoney test = SqlMoney.MaxValue + SqlMoney.MaxValue;
                                Assert.Fail ("#N02");
                        } catch (Exception e) {
                                Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#N03");
                        }

                        // "/"-operator
                        Assert.AreEqual ( (SqlMoney)13.9219m, Test2 / Test1, "#N04");

                        try {
                                SqlMoney test = Test3 / SqlMoney.Zero;
                                Assert.Fail ("#N05");
                        } catch (Exception e) {
                                Assert.AreEqual ( typeof (DivideByZeroException), e.GetType (), "#N06");
                        }

                        // "*"-operator
                        Assert.AreEqual ( (SqlMoney)581818176m, Test1 * Test2, "#N07");

                        try {
                                SqlMoney test = SqlMoney.MaxValue * Test1;
                                Assert.Fail ("#N08");
                        } catch (Exception e) {
                                Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#N09");
                        }

                        // "-"-operator
                        Assert.AreEqual ( (SqlMoney)83535.3536m, Test2 - Test1, "#N10");

                        try {
                                SqlMoney test = SqlMoney.MinValue - SqlMoney.MaxValue;
                                Assert.Fail ("#N11");
                        } catch  (Exception e) {
                                Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#N12");
                        }
                }

		[Test]
                public void ThanOrEqualOperators()
                {
                        // == -operator
                        Assert.IsTrue ((Test2 == Test2).Value, "#O01");
                        Assert.IsTrue (!(Test1 == Test2).Value, "#O02");
                        Assert.IsTrue ((Test1 == SqlMoney.Null).IsNull, "#O03");
                        
                        // != -operator
                        Assert.IsTrue (!(Test2 != Test3).Value, "#O04");
                        Assert.IsTrue ((Test1 != Test3).Value, "#O05");
                        Assert.IsTrue ((Test1 != Test4).Value, "#O06");
                        Assert.IsTrue ((Test1 != SqlMoney.Null).IsNull, "#O07");

                        // > -operator
                        Assert.IsTrue ((Test1 > Test4).Value, "#O08");
                        Assert.IsTrue ((Test2 > Test1).Value, "#O09");
                        Assert.IsTrue (!(Test2 > Test3).Value, "#O10");
                        Assert.IsTrue ((Test1 > SqlMoney.Null).IsNull, "#O11");

                        // >=  -operator
                        Assert.IsTrue (!(Test1 >= Test3).Value, "#O12");
                        Assert.IsTrue ((Test3 >= Test1).Value, "#O13");
                        Assert.IsTrue ((Test2 >= Test3).Value, "#O14");
                        Assert.IsTrue ((Test1 >= SqlMoney.Null).IsNull, "#O15");

                        // < -operator
                        Assert.IsTrue (!(Test2 < Test1).Value, "#O16");
                        Assert.IsTrue ((Test1 < Test3).Value, "#O17");
                        Assert.IsTrue (!(Test2 < Test3).Value, "#O18");
                        Assert.IsTrue ((Test1 < SqlMoney.Null).IsNull, "#O19");

                        // <= -operator
                        Assert.IsTrue ((Test1 <= Test3).Value, "#O20");
                        Assert.IsTrue (!(Test3 <= Test1).Value, "#O21");
                        Assert.IsTrue ((Test2 <= Test3).Value, "#O22");
                        Assert.IsTrue ((Test1 <= SqlMoney.Null).IsNull, "#O23");
                }

		[Test]
                public void UnaryNegation()
                {

                        Assert.AreEqual ( (decimal)(-6464.6464), -(Test1).Value, "#P01");
                        Assert.AreEqual ( 45000.0000M, -(Test4).Value, "#P02");
                }

		[Test]
                public void SqlBooleanToSqlMoney()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);

                        Assert.AreEqual ( 1.0000M, ((SqlMoney)TestBoolean).Value, "#Q01");
			Assert.IsTrue (((SqlDecimal)SqlBoolean.Null).IsNull, "#Q02");
                }
		
		[Test]
		public void SqlDecimalToSqlMoney()
		{
			SqlDecimal TestDecimal = new SqlDecimal (4000);
			SqlDecimal TestDecimal2 = new SqlDecimal (1E+20);

			SqlMoney TestMoney = (SqlMoney)TestDecimal;
			Assert.AreEqual ( 4000.0000M,TestMoney.Value, "#R01");

			try {
				SqlMoney test = (SqlMoney)TestDecimal2;
				Assert.Fail ("#R02");
			} catch (Exception e) {
				Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#R03");
			}
		}
	     
		[Test]
		public void SqlDoubleToSqlMoney()
		{
			SqlDouble TestDouble = new SqlDouble (1E+9);
			SqlDouble TestDouble2 = new SqlDouble (1E+20);
			
			SqlMoney TestMoney = (SqlMoney)TestDouble;
			Assert.AreEqual ( 1000000000.0000m, TestMoney.Value, "#S01");

			try {
				SqlMoney test = (SqlMoney)TestDouble2;
				Assert.Fail ("#S02");
			} catch (Exception e) {
				Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#S03");
			}
		}

		[Test]
		public void SqlMoneyToDecimal()
		{
                        Assert.AreEqual ( (decimal)6464.6464, (decimal)Test1, "#T01");
                        Assert.AreEqual ( -45000.0000M, (decimal)Test4, "#T02");
		}

		[Test]
		public void SqlSingleToSqlMoney()
		{
			SqlSingle TestSingle = new SqlSingle (1e10);
			SqlSingle TestSingle2 = new SqlSingle (1e20);

			Assert.AreEqual ( 10000000000.0000m, ((SqlMoney)TestSingle).Value, "#U01");

			try {
				SqlMoney test = (SqlMoney)TestSingle2;
				Assert.Fail ("#U02");
			} catch (Exception e) {
				Assert.AreEqual ( typeof (OverflowException), e.GetType(), "#U03");
			}
		}

		[Test]
                public void SqlStringToSqlMoney()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");

                        Assert.AreEqual ( 100.0000M, ((SqlMoney)TestString100).Value, "#V01");

                        try {
                                SqlMoney test = (SqlMoney)TestString;
                                Assert.Fail ("#V02");
                        } catch(Exception e) {
                                Assert.AreEqual ( typeof (FormatException), e.GetType (), "#V03");
                        }
                }

		[Test]
		public void DecimalToSqlMoney()
		{
                        decimal TestDecimal = 1e10m;
                        decimal TestDecimal2 = 1e20m;
			Assert.AreEqual ( 10000000000.0000M, ((SqlMoney)TestDecimal).Value, "#W01");
			
			try {
				SqlMoney test = (SqlMoney)TestDecimal2;
				Assert.Fail ("#W02");
			} catch (Exception e) {
				Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#W03");
			}			
		}

		[Test]
                public void SqlByteToSqlMoney() 
   	        {
                        SqlByte TestByte = new SqlByte ((byte)200);               
			Assert.AreEqual ( 200.0000m, ((SqlMoney)TestByte).Value, "#X01");
		}

		[Test]
		public void IntsToSqlMoney()
		{
			SqlInt16 TestInt16 = new SqlInt16 (5000);
			SqlInt32 TestInt32 = new SqlInt32 (5000);
			SqlInt64 TestInt64 = new SqlInt64 (5000);
			
			Assert.AreEqual ( 5000.0000m, ((SqlMoney)TestInt16).Value, "#Y01");
			Assert.AreEqual ( 5000.0000m, ((SqlMoney)TestInt32).Value, "#Y02");
			Assert.AreEqual ( 5000.0000m, ((SqlMoney)TestInt64).Value, "#Y03");

			try {
				SqlMoney test = (SqlMoney)SqlInt64.MaxValue;
				Assert.Fail ("#Y04");
			} catch (Exception e) {
				Assert.AreEqual ( typeof (OverflowException), e.GetType (), "#Y05");
			}
		}
#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlMoney.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("decimal", qualifiedName.Name, "#A01");
		}
#endif
        }
}

