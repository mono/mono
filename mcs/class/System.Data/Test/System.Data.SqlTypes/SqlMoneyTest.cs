//
// SqlMoneyTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlMoney
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
        public class SqlMoneyTest : TestCase {

	        private SqlMoney Test1;
		private SqlMoney Test2;
		private SqlMoney Test3;
		private SqlMoney Test4;

                public SqlMoneyTest() : base ("System.Data.SqlTypes.SqlMoney") {}
                public SqlMoneyTest(string name) : base(name) {}

                protected override void TearDown() {}

                protected override void SetUp() 
		{
			Test1 = new SqlMoney (6464.6464d);
			Test2 = new SqlMoney (90000.0m);
			Test3 = new SqlMoney (90000.0m);
			Test4 = new SqlMoney (-45000.0m);
		}

                public static ITest Suite {
                        get {
                                return new TestSuite(typeof(SqlMoney));
                        }
                }

                // Test constructor
                public void TestCreate()
                {
			try {
				SqlMoney Test = new SqlMoney (1000000000000000m);
				Fail ("#B01");
			} catch (Exception e) {
				AssertEquals ("#A02", typeof (OverflowException),
					      e.GetType ());
			}

                        SqlMoney CreationTest = new SqlMoney ((decimal)913.3);
			AssertEquals ("A03", 913.3m, CreationTest.Value);

			try {
				SqlMoney Test = new SqlMoney (1e200);
				Fail ("#B04");
			} catch (Exception e) {
				AssertEquals ("#A05", typeof (OverflowException),
					      e.GetType ());
			}
                        
                        SqlMoney CreationTest2 = new SqlMoney ((double)913.3);
			AssertEquals ("A06", 913.3m, CreationTest2.Value);

                        SqlMoney CreationTest3 = new SqlMoney ((int)913);
			AssertEquals ("A07", 913m, CreationTest3.Value);

                        SqlMoney CreationTest4 = new SqlMoney ((long)913.3);
                        AssertEquals ("A08", 913m, CreationTest4.Value);
                }

                // Test public fields
                public void TestPublicFields()
                {
                        // FIXME: There is a error in msdn docs, it says thath MaxValue
                        // is 922,337,203,685,475.5807 when the actual value is
                        //    922,337,203,685,477.5807
                        AssertEquals ("#B01", 922337203685477.5807m, SqlMoney.MaxValue.Value);
                        AssertEquals ("#B02", -922337203685477.5808m, SqlMoney.MinValue.Value);
                        Assert ("#B03", SqlMoney.Null.IsNull);
                        AssertEquals ("#B04", 0m, SqlMoney.Zero.Value);
                }

                // Test properties
                public void TestProperties()
                {
			AssertEquals ("#C01", 90000m, Test2.Value);
                        AssertEquals ("#C02", -45000m, Test4.Value);
			Assert ("#C03", SqlMoney.Null.IsNull);
                }

                // PUBLIC METHODS

                public void TestArithmeticMethods()
                {
			SqlMoney TestMoney2 = new SqlMoney (2);

			// Add
                        AssertEquals ("#D01", (SqlMoney)96464.6464m, SqlMoney.Add (Test1, Test2));
                        AssertEquals ("#D02", (SqlMoney)180000m, SqlMoney.Add (Test2, Test2));
                        AssertEquals ("#D03", (SqlMoney)45000m, SqlMoney.Add (Test2, Test4));
			
			try {
				SqlMoney test = SqlMoney.Add(SqlMoney.MaxValue, Test2);
				Fail ("#D04");
			} catch (Exception e) {
				AssertEquals ("#D05", typeof (OverflowException), e.GetType ());
			}

			// Divide
                        AssertEquals ("#D06", (SqlMoney)45000m, SqlMoney.Divide (Test2, TestMoney2));
			try {
				SqlMoney test = SqlMoney.Divide (Test2, SqlMoney.Zero);
				Fail ("#D07");
			} catch (Exception e) {
				AssertEquals ("#D08", typeof (DivideByZeroException), 
					      e.GetType());
			}
				     			
			// Multiply
                        AssertEquals ("#D09", (SqlMoney)581818176m, SqlMoney.Multiply (Test1, Test2));
                        AssertEquals ("#D10", (SqlMoney)(-4050000000m), SqlMoney.Multiply (Test3, Test4));

			try {
				SqlMoney test = SqlMoney.Multiply (SqlMoney.MaxValue, Test2);
				Fail ("#D11");
			} catch (Exception e) {
				AssertEquals ("#D12", typeof (OverflowException), e.GetType ());
			}
				      
			// Subtract
                        AssertEquals ("#D13", (SqlMoney)0m, SqlMoney.Subtract (Test2, Test3));
                        AssertEquals ("#D14", (SqlMoney)83535.3536m, SqlMoney.Subtract (Test2, Test1));
			
			try {
				SqlMoney test = SqlMoney.Subtract (SqlMoney.MinValue, Test2);
			} catch (Exception e) {
				AssertEquals ("#D15", typeof (OverflowException), e.GetType ());
			}
                }

                public void TestCompareTo()
		{
			Assert ("#E01", Test1.CompareTo (Test2) < 0);
 			Assert ("#E02", Test3.CompareTo (Test1) > 0);
 			Assert ("#E03", Test3.CompareTo (Test2) == 0);
                        Assert ("#E04", Test3.CompareTo (SqlMoney.Null) > 0);
                }

                public void TestEqualsMethods()
                {
			Assert ("#F01", !Test1.Equals (Test2));
			Assert ("#F02", Test2.Equals (Test3));
			Assert ("#F03", !SqlMoney.Equals (Test1, Test2).Value);
			Assert ("#F04", SqlMoney.Equals (Test3, Test2).Value);
                }

                public void TestGetHashCode()
                {
                        // FIXME: Better way to test HashCode
                        AssertEquals ("#G01", Test3.GetHashCode (), Test2.GetHashCode ());
                        Assert ("#G02", Test2.GetHashCode () !=  Test1.GetHashCode ());
                }

                public void TestGetType()
                {
			AssertEquals ("#H01", "System.Data.SqlTypes.SqlMoney", 
				      Test1.GetType ().ToString ());
		}

                public void TestGreaters()
                {
                        // GreateThan ()
                        Assert ("#I01", !SqlMoney.GreaterThan (Test1, Test2).Value);
                        Assert ("#I02", SqlMoney.GreaterThan (Test2, Test1).Value);
                        Assert ("#I03", !SqlMoney.GreaterThan (Test2, Test3).Value);
                        Assert ("#I04", SqlMoney.GreaterThan (Test2, SqlMoney.Null).IsNull);

                        // GreaterTharOrEqual ()
                        Assert ("#I05", !SqlMoney.GreaterThanOrEqual (Test1, Test2).Value);
                        Assert ("#I06", SqlMoney.GreaterThanOrEqual (Test2, Test1).Value);
                        Assert ("#I07", SqlMoney.GreaterThanOrEqual (Test3, Test2).Value);
                        Assert ("#I08", SqlMoney.GreaterThanOrEqual (Test3, SqlMoney.Null).IsNull);
                }

                public void TestLessers()
                {
                        // LessThan()
                        Assert ("#J01", !SqlMoney.LessThan (Test2, Test3).Value);
                        Assert ("#J02", !SqlMoney.LessThan (Test2, Test1).Value);
                        Assert ("#J03", SqlMoney.LessThan (Test1, Test2).Value);
                        Assert ("#J04", SqlMoney.LessThan (SqlMoney.Null, Test2).IsNull);

                        // LessThanOrEqual ()
                        Assert ("#J05", SqlMoney.LessThanOrEqual (Test1, Test2).Value);
                        Assert ("#J06", !SqlMoney.LessThanOrEqual (Test2, Test1).Value);
                        Assert ("#J07", SqlMoney.LessThanOrEqual (Test2, Test2).Value);
                        Assert ("#J08", SqlMoney.LessThanOrEqual (Test2, SqlMoney.Null).IsNull);
                }

                public void TestNotEquals()
                {
                        Assert ("#K01", SqlMoney.NotEquals (Test1, Test2).Value);
                        Assert ("#K02", SqlMoney.NotEquals (Test2, Test1).Value);
                        Assert ("#K03", !SqlMoney.NotEquals (Test2, Test3).Value);
                        Assert ("#K04", !SqlMoney.NotEquals (Test3, Test2).Value);
                        Assert ("#K05", SqlMoney.NotEquals (SqlMoney.Null, Test2).IsNull);
                }

                public void TestParse()
                {
                        try {
                                SqlMoney.Parse (null);
                                Fail ("#L01");
                        } catch (Exception e) {
                                AssertEquals ("#L02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlMoney.Parse ("not-a-number");
                                Fail ("#L03");
                        } catch (Exception e) {

                                AssertEquals ("#L04", typeof (FormatException), e.GetType ());
                        }

                         try {
                                SqlMoney.Parse ("1000000000000000");
                                Fail ("#L05");
                        } catch (Exception e) {
                                AssertEquals ("#L06", typeof (OverflowException), e.GetType ());
                        }

                        AssertEquals("#L07", (decimal)150, SqlMoney.Parse ("150").Value);
                }

                public void TestConversions()
                {		      
			SqlMoney TestMoney100 = new SqlMoney (100);

			// ToDecimal
			AssertEquals ("#M01", (decimal)6464.6464, Test1.ToDecimal ());

			// ToDouble
			AssertEquals ("#M02", (double)6464.6464, Test1.ToDouble ());

			// ToInt32
			AssertEquals ("#M03", (int)90000, Test2.ToInt32 ());
                        AssertEquals ("#M04", (int)6465, Test1.ToInt32 ());

			// ToInt64
                        AssertEquals ("#M05", (long)90000, Test2.ToInt64 ());
                        AssertEquals ("#M06", (long)6465, Test1.ToInt64 ());

                        // ToSqlBoolean ()
                        Assert ("#M07", Test1.ToSqlBoolean ().Value);
                        Assert ("#M08", !SqlMoney.Zero.ToSqlBoolean ().Value);
                        Assert ("#M09", SqlMoney.Null.ToSqlBoolean ().IsNull);

                        // ToSqlByte ()
                        AssertEquals ("#M10", (byte)100, TestMoney100.ToSqlByte ().Value);

                        try {
                                SqlByte b = (byte)Test2.ToSqlByte ();
                                Fail ("#M11");
                        } catch (Exception e) {
                                AssertEquals ("#M12", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlDecimal ()
                        AssertEquals ("#M13", (decimal)6464.6464, Test1.ToSqlDecimal ().Value);
                        AssertEquals ("#M14", (decimal)-45000, Test4.ToSqlDecimal ().Value);

                        // ToSqlInt16 ()
                        AssertEquals ("#M15", (short)6465, Test1.ToSqlInt16 ().Value);

                        try {
                                SqlInt16 test = SqlMoney.MaxValue.ToSqlInt16().Value;
                                Fail ("#M17");
                        } catch (Exception e) {
                                AssertEquals ("#M18", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlInt32 ()
                        AssertEquals ("#M19", (int)6465, Test1.ToSqlInt32 ().Value);
                        AssertEquals ("#M20", (int)(-45000), Test4.ToSqlInt32 ().Value);

                        try {
                                SqlInt32 test = SqlMoney.MaxValue.ToSqlInt32 ().Value;
                                Fail ("#M21");
                        } catch (Exception e) { 
                                AssertEquals ("#M22", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlInt64 ()
                        AssertEquals ("#M23", (long)6465, Test1.ToSqlInt64 ().Value);
                        AssertEquals ("#M24", (long)(-45000), Test4.ToSqlInt64 ().Value);

                        // ToSqlSingle ()
                        AssertEquals ("#M25", (float)6464.6464, Test1.ToSqlSingle ().Value);

                        // ToSqlString ()
                        AssertEquals ("#M26", "6464,6464", Test1.ToSqlString ().Value);
                        AssertEquals ("#M27", "90000", Test2.ToSqlString ().Value);

                        // ToString ()
                        AssertEquals ("#M28", "6464,6464", Test1.ToString ());
                        AssertEquals ("#M29", "90000", Test2.ToString ());
                }

                // OPERATORS

                public void TestArithmeticOperators()
                {
                        // "+"-operator
                        AssertEquals ("#N01", (SqlMoney)96464.6464m, Test1 + Test2);
     
                        try {
                                SqlMoney test = SqlMoney.MaxValue + SqlMoney.MaxValue;
                                Fail ("#N02");
                        } catch (Exception e) {
                                AssertEquals ("#N03", typeof (OverflowException), e.GetType ());
                        }

                        // "/"-operator
                        AssertEquals ("#N04", (SqlMoney)13.9219m, Test2 / Test1);

                        try {
                                SqlMoney test = Test3 / SqlMoney.Zero;
                                Fail ("#N05");
                        } catch (Exception e) {
                                AssertEquals ("#N06", typeof (DivideByZeroException), e.GetType ());
                        }

                        // "*"-operator
                        AssertEquals ("#N07", (SqlMoney)581818176m, Test1 * Test2);

                        try {
                                SqlMoney test = SqlMoney.MaxValue * Test1;
                                Fail ("#N08");
                        } catch (Exception e) {
                                AssertEquals ("#N09", typeof (OverflowException), e.GetType ());
                        }

                        // "-"-operator
                        AssertEquals ("#N10", (SqlMoney)83535.3536m, Test2 - Test1);

                        try {
                                SqlMoney test = SqlMoney.MinValue - SqlMoney.MaxValue;
                                Fail ("#N11");
                        } catch  (Exception e) {
                                AssertEquals ("#N12", typeof (OverflowException), e.GetType ());
                        }
                }

                public void TestThanOrEqualOperators()
                {
                        // == -operator
                        Assert ("#O01", (Test2 == Test2).Value);
                        Assert ("#O02", !(Test1 == Test2).Value);
                        Assert ("#O03", (Test1 == SqlMoney.Null).IsNull);
                        
                        // != -operator
                        Assert ("#O04", !(Test2 != Test3).Value);
                        Assert ("#O05", (Test1 != Test3).Value);
                        Assert ("#O06", (Test1 != Test4).Value);
                        Assert ("#O07", (Test1 != SqlMoney.Null).IsNull);

                        // > -operator
                        Assert ("#O08", (Test1 > Test4).Value);
                        Assert ("#O09", (Test2 > Test1).Value);
                        Assert ("#O10", !(Test2 > Test3).Value);
                        Assert ("#O11", (Test1 > SqlMoney.Null).IsNull);

                        // >=  -operator
                        Assert ("#O12", !(Test1 >= Test3).Value);
                        Assert ("#O13", (Test3 >= Test1).Value);
                        Assert ("#O14", (Test2 >= Test3).Value);
                        Assert ("#O15", (Test1 >= SqlMoney.Null).IsNull);

                        // < -operator
                        Assert ("#O16", !(Test2 < Test1).Value);
                        Assert ("#O17", (Test1 < Test3).Value);
                        Assert ("#O18", !(Test2 < Test3).Value);
                        Assert ("#O19", (Test1 < SqlMoney.Null).IsNull);

                        // <= -operator
                        Assert ("#O20", (Test1 <= Test3).Value);
                        Assert ("#O21", !(Test3 <= Test1).Value);
                        Assert ("#O22", (Test2 <= Test3).Value);
                        Assert ("#O23", (Test1 <= SqlMoney.Null).IsNull);
                }

                public void TestUnaryNegation()
                {

                        AssertEquals ("#P01", (decimal)(-6464.6464), -(Test1).Value);
                        AssertEquals ("#P02", (decimal)45000, -(Test4).Value);
                }

                public void TestSqlBooleanToSqlMoney()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);

                        AssertEquals ("#Q01", (decimal)1, ((SqlMoney)TestBoolean).Value);
			Assert ("#Q02", ((SqlDecimal)SqlBoolean.Null).IsNull);
                }
		
		public void TestSqlDecimalToSqlMoney()
		{
			SqlDecimal TestDecimal = new SqlDecimal (4000);
			SqlDecimal TestDecimal2 = new SqlDecimal (1e20);

			SqlMoney TestMoney = (SqlMoney)TestDecimal;
			AssertEquals ("#R01", TestMoney.Value, TestDecimal.Value);

			try {
				SqlMoney test = (SqlMoney)TestDecimal2;
				Fail ("#R02");
			} catch (Exception e) {
				AssertEquals ("#R03", typeof (OverflowException), e.GetType ());
			}
		}
	     
		public void TestSqlDoubleToSqlMoney()
		{
			SqlDouble TestDouble = new SqlDouble (1e9);
			SqlDouble TestDouble2 = new SqlDouble (1e20);
			
			SqlMoney TestMoney = (SqlMoney)TestDouble;
			AssertEquals ("#S01", 1000000000m, TestMoney.Value);

			try {
				SqlMoney test = (SqlMoney)TestDouble2;
				Fail ("#S02");
			} catch (Exception e) {
				AssertEquals ("#S03", typeof (OverflowException), e.GetType ());
			}
		}

		public void SqlMoneyToDecimal()
		{
                        AssertEquals ("#T01", (decimal)6464.6464, (decimal)Test1);
                        AssertEquals ("#T02", (decimal)(-45000), (decimal)Test4);
		}

		public void SqlSingleToSqlMoney()
		{
			SqlSingle TestSingle = new SqlSingle (1e10);
			SqlSingle TestSingle2 = new SqlSingle (1e20);

			AssertEquals ("#U01", 10000000000m, ((SqlMoney)TestSingle).Value);

			try {
				SqlMoney test = (SqlMoney)TestSingle2;
				Fail ("#U02");
			} catch (Exception e) {
				AssertEquals ("#U03", typeof (OverflowException), e.GetType());
			}
		}

                public void TestSqlStringToSqlMoney()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");

                        AssertEquals ("#V01", (decimal)100, ((SqlMoney)TestString100).Value);

                        try {
                                SqlMoney test = (SqlMoney)TestString;
                                Fail ("#V02");
                        } catch(Exception e) {
                                AssertEquals ("#V03", typeof (FormatException), e.GetType ());
                        }
                }

		public void DecimalToSqlMoney()
		{
                        decimal TestDecimal = 1e10m;
                        decimal TestDecimal2 = 1e20m;
			AssertEquals ("#W01", 10000000000, ((SqlMoney)TestDecimal).Value);
			
			try {
				SqlMoney test = (SqlMoney)TestDecimal2;
				Fail ("#W02");
			} catch (Exception e) {
				AssertEquals ("#W03", typeof (OverflowException), e.GetType ());
			}			
		}

                public void SqlByteToSqlMoney() 
   	        {
                        SqlByte TestByte = new SqlByte ((byte)200);               
			AssertEquals ("#X01", 200m, ((SqlMoney)TestByte).Value);
		}

		public void IntsToSqlMoney()
		{
			SqlInt16 TestInt16 = new SqlInt16 (5000);
			SqlInt32 TestInt32 = new SqlInt32 (5000);
			SqlInt64 TestInt64 = new SqlInt64 (5000);
			
			AssertEquals ("#Y01", 5000m, ((SqlMoney)TestInt16).Value);
			AssertEquals ("#Y02", 5000m, ((SqlMoney)TestInt32).Value);
			AssertEquals ("#Y03", 5000m, ((SqlMoney)TestInt64).Value);

			try {
				SqlMoney test = (SqlMoney)SqlInt64.MaxValue;
				Fail ("#Y04");
			} catch (Exception e) {
				AssertEquals ("#Y05", typeof (OverflowException), e.GetType ());
			}
		}
        }
}

