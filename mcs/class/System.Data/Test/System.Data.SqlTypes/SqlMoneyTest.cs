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

using NUnit.Framework;
using System;
using System.Data.SqlTypes;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlMoneyTest {

	        private SqlMoney Test1;
		private SqlMoney Test2;
		private SqlMoney Test3;
		private SqlMoney Test4;

		[SetUp]
                public void GetReady() 
		{
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
				Assertion.Fail ("#B01");
			} catch (Exception e) {
				Assertion.AssertEquals ("#A02", typeof (OverflowException),
					      e.GetType ());
			}

                        SqlMoney CreationTest = new SqlMoney ((decimal)913.3);
			Assertion.AssertEquals ("A03", 913.3m, CreationTest.Value);

			try {
				SqlMoney Test = new SqlMoney (1e200);
				Assertion.Fail ("#B04");
			} catch (Exception e) {
				Assertion.AssertEquals ("#A05", typeof (OverflowException),
					      e.GetType ());
			}
                        
                        SqlMoney CreationTest2 = new SqlMoney ((double)913.3);
			Assertion.AssertEquals ("A06", 913.3m, CreationTest2.Value);

                        SqlMoney CreationTest3 = new SqlMoney ((int)913);
			Assertion.AssertEquals ("A07", 913m, CreationTest3.Value);

                        SqlMoney CreationTest4 = new SqlMoney ((long)913.3);
                        Assertion.AssertEquals ("A08", 913m, CreationTest4.Value);
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        // FIXME: There is a error in msdn docs, it says thath MaxValue
                        // is 922,337,203,685,475.5807 when the actual value is
                        //    922,337,203,685,477.5807
                        Assertion.AssertEquals ("#B01", 922337203685477.5807m, SqlMoney.MaxValue.Value);
                        Assertion.AssertEquals ("#B02", -922337203685477.5808m, SqlMoney.MinValue.Value);
                        Assertion.Assert ("#B03", SqlMoney.Null.IsNull);
                        Assertion.AssertEquals ("#B04", 0m, SqlMoney.Zero.Value);
                }

                // Test properties
		[Test]
                public void Properties()
                {
			Assertion.AssertEquals ("#C01", 90000m, Test2.Value);
                        Assertion.AssertEquals ("#C02", -45000m, Test4.Value);
			Assertion.Assert ("#C03", SqlMoney.Null.IsNull);
                }

                // PUBLIC METHODS

		[Test]
                public void ArithmeticMethods()
                {
			SqlMoney TestMoney2 = new SqlMoney (2);

			// Add
                        Assertion.AssertEquals ("#D01", (SqlMoney)96464.6464m, SqlMoney.Add (Test1, Test2));
                        Assertion.AssertEquals ("#D02", (SqlMoney)180000m, SqlMoney.Add (Test2, Test2));
                        Assertion.AssertEquals ("#D03", (SqlMoney)45000m, SqlMoney.Add (Test2, Test4));
			
			try {
				SqlMoney test = SqlMoney.Add(SqlMoney.MaxValue, Test2);
				Assertion.Fail ("#D04");
			} catch (Exception e) {
				Assertion.AssertEquals ("#D05", typeof (OverflowException), e.GetType ());
			}

			// Divide
                        Assertion.AssertEquals ("#D06", (SqlMoney)45000m, SqlMoney.Divide (Test2, TestMoney2));
			try {
				SqlMoney test = SqlMoney.Divide (Test2, SqlMoney.Zero);
				Assertion.Fail ("#D07");
			} catch (Exception e) {
				Assertion.AssertEquals ("#D08", typeof (DivideByZeroException), 
					      e.GetType());
			}
				     			
			// Multiply
                        Assertion.AssertEquals ("#D09", (SqlMoney)581818176m, SqlMoney.Multiply (Test1, Test2));
                        Assertion.AssertEquals ("#D10", (SqlMoney)(-4050000000m), SqlMoney.Multiply (Test3, Test4));

			try {
				SqlMoney test = SqlMoney.Multiply (SqlMoney.MaxValue, Test2);
				Assertion.Fail ("#D11");
			} catch (Exception e) {
				Assertion.AssertEquals ("#D12", typeof (OverflowException), e.GetType ());
			}
				      
			// Subtract
                        Assertion.AssertEquals ("#D13", (SqlMoney)0m, SqlMoney.Subtract (Test2, Test3));
                        Assertion.AssertEquals ("#D14", (SqlMoney)83535.3536m, SqlMoney.Subtract (Test2, Test1));
			
			try {
				SqlMoney test = SqlMoney.Subtract (SqlMoney.MinValue, Test2);
			} catch (Exception e) {
				Assertion.AssertEquals ("#D15", typeof (OverflowException), e.GetType ());
			}
                }

		[Test]
                public void CompareTo()
		{
			Assertion.Assert ("#E01", Test1.CompareTo (Test2) < 0);
 			Assertion.Assert ("#E02", Test3.CompareTo (Test1) > 0);
 			Assertion.Assert ("#E03", Test3.CompareTo (Test2) == 0);
                        Assertion.Assert ("#E04", Test3.CompareTo (SqlMoney.Null) > 0);
                }

		[Test]
                public void EqualsMethods()
                {
			Assertion.Assert ("#F01", !Test1.Equals (Test2));
			Assertion.Assert ("#F02", Test2.Equals (Test3));
			Assertion.Assert ("#F03", !SqlMoney.Equals (Test1, Test2).Value);
			Assertion.Assert ("#F04", SqlMoney.Equals (Test3, Test2).Value);
                }

		[Test]
                public void GetHashCodeTest()
                {
                        // FIXME: Better way to test HashCode
                        Assertion.AssertEquals ("#G01", Test3.GetHashCode (), Test2.GetHashCode ());
                        Assertion.Assert ("#G02", Test2.GetHashCode () !=  Test1.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
			Assertion.AssertEquals ("#H01", "System.Data.SqlTypes.SqlMoney", 
				      Test1.GetType ().ToString ());
		}

		[Test]
                public void Greaters()
                {
                        // GreateThan ()
                        Assertion.Assert ("#I01", !SqlMoney.GreaterThan (Test1, Test2).Value);
                        Assertion.Assert ("#I02", SqlMoney.GreaterThan (Test2, Test1).Value);
                        Assertion.Assert ("#I03", !SqlMoney.GreaterThan (Test2, Test3).Value);
                        Assertion.Assert ("#I04", SqlMoney.GreaterThan (Test2, SqlMoney.Null).IsNull);

                        // GreaterTharOrEqual ()
                        Assertion.Assert ("#I05", !SqlMoney.GreaterThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#I06", SqlMoney.GreaterThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#I07", SqlMoney.GreaterThanOrEqual (Test3, Test2).Value);
                        Assertion.Assert ("#I08", SqlMoney.GreaterThanOrEqual (Test3, SqlMoney.Null).IsNull);
                }

		[Test]
                public void Lessers()
                {
                        // LessThan()
                        Assertion.Assert ("#J01", !SqlMoney.LessThan (Test2, Test3).Value);
                        Assertion.Assert ("#J02", !SqlMoney.LessThan (Test2, Test1).Value);
                        Assertion.Assert ("#J03", SqlMoney.LessThan (Test1, Test2).Value);
                        Assertion.Assert ("#J04", SqlMoney.LessThan (SqlMoney.Null, Test2).IsNull);

                        // LessThanOrEqual ()
                        Assertion.Assert ("#J05", SqlMoney.LessThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#J06", !SqlMoney.LessThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#J07", SqlMoney.LessThanOrEqual (Test2, Test2).Value);
                        Assertion.Assert ("#J08", SqlMoney.LessThanOrEqual (Test2, SqlMoney.Null).IsNull);
                }

		[Test]
                public void NotEquals()
                {
                        Assertion.Assert ("#K01", SqlMoney.NotEquals (Test1, Test2).Value);
                        Assertion.Assert ("#K02", SqlMoney.NotEquals (Test2, Test1).Value);
                        Assertion.Assert ("#K03", !SqlMoney.NotEquals (Test2, Test3).Value);
                        Assertion.Assert ("#K04", !SqlMoney.NotEquals (Test3, Test2).Value);
                        Assertion.Assert ("#K05", SqlMoney.NotEquals (SqlMoney.Null, Test2).IsNull);
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlMoney.Parse (null);
                                Assertion.Fail ("#L01");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#L02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlMoney.Parse ("not-a-number");
                                Assertion.Fail ("#L03");
                        } catch (Exception e) {

                                Assertion.AssertEquals ("#L04", typeof (FormatException), e.GetType ());
                        }

                         try {
                                SqlMoney.Parse ("1000000000000000");
                                Assertion.Fail ("#L05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#L06", typeof (OverflowException), e.GetType ());
                        }

                        Assertion.AssertEquals("#L07", (decimal)150, SqlMoney.Parse ("150").Value);
                }

		[Test]
                public void Conversions()
                {		      
			SqlMoney TestMoney100 = new SqlMoney (100);

			// ToDecimal
			Assertion.AssertEquals ("#M01", (decimal)6464.6464, Test1.ToDecimal ());

			// ToDouble
			Assertion.AssertEquals ("#M02", (double)6464.6464, Test1.ToDouble ());

			// ToInt32
			Assertion.AssertEquals ("#M03", (int)90000, Test2.ToInt32 ());
                        Assertion.AssertEquals ("#M04", (int)6465, Test1.ToInt32 ());

			// ToInt64
                        Assertion.AssertEquals ("#M05", (long)90000, Test2.ToInt64 ());
                        Assertion.AssertEquals ("#M06", (long)6465, Test1.ToInt64 ());

                        // ToSqlBoolean ()
                        Assertion.Assert ("#M07", Test1.ToSqlBoolean ().Value);
                        Assertion.Assert ("#M08", !SqlMoney.Zero.ToSqlBoolean ().Value);
                        Assertion.Assert ("#M09", SqlMoney.Null.ToSqlBoolean ().IsNull);

                        // ToSqlByte ()
                        Assertion.AssertEquals ("#M10", (byte)100, TestMoney100.ToSqlByte ().Value);

                        try {
                                SqlByte b = (byte)Test2.ToSqlByte ();
                                Assertion.Fail ("#M11");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M12", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlDecimal ()
                        Assertion.AssertEquals ("#M13", (decimal)6464.6464, Test1.ToSqlDecimal ().Value);
                        Assertion.AssertEquals ("#M14", (decimal)-45000, Test4.ToSqlDecimal ().Value);

                        // ToSqlInt16 ()
                        Assertion.AssertEquals ("#M15", (short)6465, Test1.ToSqlInt16 ().Value);

                        try {
                                SqlInt16 test = SqlMoney.MaxValue.ToSqlInt16().Value;
                                Assertion.Fail ("#M17");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M18", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlInt32 ()
                        Assertion.AssertEquals ("#M19", (int)6465, Test1.ToSqlInt32 ().Value);
                        Assertion.AssertEquals ("#M20", (int)(-45000), Test4.ToSqlInt32 ().Value);

                        try {
                                SqlInt32 test = SqlMoney.MaxValue.ToSqlInt32 ().Value;
                                Assertion.Fail ("#M21");
                        } catch (Exception e) { 
                                Assertion.AssertEquals ("#M22", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlInt64 ()
                        Assertion.AssertEquals ("#M23", (long)6465, Test1.ToSqlInt64 ().Value);
                        Assertion.AssertEquals ("#M24", (long)(-45000), Test4.ToSqlInt64 ().Value);

                        // ToSqlSingle ()
                        Assertion.AssertEquals ("#M25", (float)6464.6464, Test1.ToSqlSingle ().Value);

                        // ToSqlString ()
                        Assertion.AssertEquals ("#M26", "6464,6464", Test1.ToSqlString ().Value);
                        Assertion.AssertEquals ("#M27", "90000", Test2.ToSqlString ().Value);

                        // ToString ()
                        Assertion.AssertEquals ("#M28", "6464,6464", Test1.ToString ());
                        Assertion.AssertEquals ("#M29", "90000", Test2.ToString ());
                }

                // OPERATORS

		[Test]
                public void ArithmeticOperators()
                {
                        // "+"-operator
                        Assertion.AssertEquals ("#N01", (SqlMoney)96464.6464m, Test1 + Test2);
     
                        try {
                                SqlMoney test = SqlMoney.MaxValue + SqlMoney.MaxValue;
                                Assertion.Fail ("#N02");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N03", typeof (OverflowException), e.GetType ());
                        }

                        // "/"-operator
                        Assertion.AssertEquals ("#N04", (SqlMoney)13.9219m, Test2 / Test1);

                        try {
                                SqlMoney test = Test3 / SqlMoney.Zero;
                                Assertion.Fail ("#N05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N06", typeof (DivideByZeroException), e.GetType ());
                        }

                        // "*"-operator
                        Assertion.AssertEquals ("#N07", (SqlMoney)581818176m, Test1 * Test2);

                        try {
                                SqlMoney test = SqlMoney.MaxValue * Test1;
                                Assertion.Fail ("#N08");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N09", typeof (OverflowException), e.GetType ());
                        }

                        // "-"-operator
                        Assertion.AssertEquals ("#N10", (SqlMoney)83535.3536m, Test2 - Test1);

                        try {
                                SqlMoney test = SqlMoney.MinValue - SqlMoney.MaxValue;
                                Assertion.Fail ("#N11");
                        } catch  (Exception e) {
                                Assertion.AssertEquals ("#N12", typeof (OverflowException), e.GetType ());
                        }
                }

		[Test]
                public void ThanOrEqualOperators()
                {
                        // == -operator
                        Assertion.Assert ("#O01", (Test2 == Test2).Value);
                        Assertion.Assert ("#O02", !(Test1 == Test2).Value);
                        Assertion.Assert ("#O03", (Test1 == SqlMoney.Null).IsNull);
                        
                        // != -operator
                        Assertion.Assert ("#O04", !(Test2 != Test3).Value);
                        Assertion.Assert ("#O05", (Test1 != Test3).Value);
                        Assertion.Assert ("#O06", (Test1 != Test4).Value);
                        Assertion.Assert ("#O07", (Test1 != SqlMoney.Null).IsNull);

                        // > -operator
                        Assertion.Assert ("#O08", (Test1 > Test4).Value);
                        Assertion.Assert ("#O09", (Test2 > Test1).Value);
                        Assertion.Assert ("#O10", !(Test2 > Test3).Value);
                        Assertion.Assert ("#O11", (Test1 > SqlMoney.Null).IsNull);

                        // >=  -operator
                        Assertion.Assert ("#O12", !(Test1 >= Test3).Value);
                        Assertion.Assert ("#O13", (Test3 >= Test1).Value);
                        Assertion.Assert ("#O14", (Test2 >= Test3).Value);
                        Assertion.Assert ("#O15", (Test1 >= SqlMoney.Null).IsNull);

                        // < -operator
                        Assertion.Assert ("#O16", !(Test2 < Test1).Value);
                        Assertion.Assert ("#O17", (Test1 < Test3).Value);
                        Assertion.Assert ("#O18", !(Test2 < Test3).Value);
                        Assertion.Assert ("#O19", (Test1 < SqlMoney.Null).IsNull);

                        // <= -operator
                        Assertion.Assert ("#O20", (Test1 <= Test3).Value);
                        Assertion.Assert ("#O21", !(Test3 <= Test1).Value);
                        Assertion.Assert ("#O22", (Test2 <= Test3).Value);
                        Assertion.Assert ("#O23", (Test1 <= SqlMoney.Null).IsNull);
                }

		[Test]
                public void UnaryNegation()
                {

                        Assertion.AssertEquals ("#P01", (decimal)(-6464.6464), -(Test1).Value);
                        Assertion.AssertEquals ("#P02", (decimal)45000, -(Test4).Value);
                }

		[Test]
                public void SqlBooleanToSqlMoney()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);

                        Assertion.AssertEquals ("#Q01", (decimal)1, ((SqlMoney)TestBoolean).Value);
			Assertion.Assert ("#Q02", ((SqlDecimal)SqlBoolean.Null).IsNull);
                }
		
		[Test]
		public void SqlDecimalToSqlMoney()
		{
			SqlDecimal TestDecimal = new SqlDecimal (4000);
			SqlDecimal TestDecimal2 = new SqlDecimal (1E+20);

			SqlMoney TestMoney = (SqlMoney)TestDecimal;
			Assertion.AssertEquals ("#R01", TestMoney.Value, TestDecimal.Value);

			try {
				SqlMoney test = (SqlMoney)TestDecimal2;
				Assertion.Fail ("#R02");
			} catch (Exception e) {
				Assertion.AssertEquals ("#R03", typeof (OverflowException), e.GetType ());
			}
		}
	     
		[Test]
		public void SqlDoubleToSqlMoney()
		{
			SqlDouble TestDouble = new SqlDouble (1E+9);
			SqlDouble TestDouble2 = new SqlDouble (1E+20);
			
			SqlMoney TestMoney = (SqlMoney)TestDouble;
			Assertion.AssertEquals ("#S01", 1000000000m, TestMoney.Value);

			try {
				SqlMoney test = (SqlMoney)TestDouble2;
				Assertion.Fail ("#S02");
			} catch (Exception e) {
				Assertion.AssertEquals ("#S03", typeof (OverflowException), e.GetType ());
			}
		}

		[Test]
		public void SqlMoneyToDecimal()
		{
                        Assertion.AssertEquals ("#T01", (decimal)6464.6464, (decimal)Test1);
                        Assertion.AssertEquals ("#T02", (decimal)(-45000), (decimal)Test4);
		}

		[Test]
		public void SqlSingleToSqlMoney()
		{
			SqlSingle TestSingle = new SqlSingle (1e10);
			SqlSingle TestSingle2 = new SqlSingle (1e20);

			Assertion.AssertEquals ("#U01", 10000000000m, ((SqlMoney)TestSingle).Value);

			try {
				SqlMoney test = (SqlMoney)TestSingle2;
				Assertion.Fail ("#U02");
			} catch (Exception e) {
				Assertion.AssertEquals ("#U03", typeof (OverflowException), e.GetType());
			}
		}

		[Test]
                public void SqlStringToSqlMoney()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");

                        Assertion.AssertEquals ("#V01", (decimal)100, ((SqlMoney)TestString100).Value);

                        try {
                                SqlMoney test = (SqlMoney)TestString;
                                Assertion.Fail ("#V02");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#V03", typeof (FormatException), e.GetType ());
                        }
                }

		[Test]
		public void DecimalToSqlMoney()
		{
                        decimal TestDecimal = 1e10m;
                        decimal TestDecimal2 = 1e20m;
			Assertion.AssertEquals ("#W01", 10000000000, ((SqlMoney)TestDecimal).Value);
			
			try {
				SqlMoney test = (SqlMoney)TestDecimal2;
				Assertion.Fail ("#W02");
			} catch (Exception e) {
				Assertion.AssertEquals ("#W03", typeof (OverflowException), e.GetType ());
			}			
		}

		[Test]
                public void SqlByteToSqlMoney() 
   	        {
                        SqlByte TestByte = new SqlByte ((byte)200);               
			Assertion.AssertEquals ("#X01", 200m, ((SqlMoney)TestByte).Value);
		}

		[Test]
		public void IntsToSqlMoney()
		{
			SqlInt16 TestInt16 = new SqlInt16 (5000);
			SqlInt32 TestInt32 = new SqlInt32 (5000);
			SqlInt64 TestInt64 = new SqlInt64 (5000);
			
			Assertion.AssertEquals ("#Y01", 5000m, ((SqlMoney)TestInt16).Value);
			Assertion.AssertEquals ("#Y02", 5000m, ((SqlMoney)TestInt32).Value);
			Assertion.AssertEquals ("#Y03", 5000m, ((SqlMoney)TestInt64).Value);

			try {
				SqlMoney test = (SqlMoney)SqlInt64.MaxValue;
				Assertion.Fail ("#Y04");
			} catch (Exception e) {
				Assertion.AssertEquals ("#Y05", typeof (OverflowException), e.GetType ());
			}
		}
        }
}

