//
// SqlDecimalTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlDecimal
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
        public class SqlDecimalTest : TestCase {

		private SqlDecimal Test1;
        	private SqlDecimal Test2;
        	private SqlDecimal Test3;
        	private SqlDecimal Test4;
        	
                public SqlDecimalTest() : base ("System.Data.SqlTypes.SqlDecimal") {}
                public SqlDecimalTest(string name) : base(name) {}

                protected override void TearDown() {}

                protected override void SetUp() 
                {
                	Test1 = new SqlDecimal (6464.6464m);
                	Test2 = new SqlDecimal (10000.00m); 
                	Test3 = new SqlDecimal (10000.00m);                 
                	Test4 = new SqlDecimal (-6m);                 
                }

                public static ITest Suite {
                        get {
                                return new TestSuite(typeof(SqlDecimal));
                        }
                }

                // Test constructor
                public void TestCreate()
                {
                	// SqlDecimal (decimal)
			SqlDecimal Test = new SqlDecimal (30.3098m);
                	AssertEquals ("#A01", (decimal)30.3098, Test.Value);
                	
                	try {
                		SqlDecimal test = new SqlDecimal (Decimal.MaxValue + 1);
                		Fail ("#A02");                		
                	} catch (Exception e) {
                		AssertEquals ("#A03", typeof (OverflowException), e.GetType ());
                	}
                	
                	// SqlDecimal (double)
                	Test = new SqlDecimal (10e10d);
                	AssertEquals ("#A05", 100000000000m, Test.Value);
                	
                	try {
                		SqlDecimal test = new SqlDecimal (10e200d);
                		Fail ("#A06");                		
                	} catch (Exception e) {
                		AssertEquals ("#A07", typeof (OverflowException), e.GetType ());
                	}
                	
                	// SqlDecimal (int)
                	Test = new SqlDecimal (-1);
                	AssertEquals ("#A08", -1m, Test.Value);
                
			// SqlDecimal (long)
                	Test = new SqlDecimal ((long)(-99999));
                	AssertEquals ("#A09", -99999m, Test.Value);
                
                	// SqlDecimal (byte, byte, bool. int[]
                 	Test = new SqlDecimal (10, 3, false, new int [4] {200, 1, 0, 0});
                	AssertEquals ("#A10", -4294967.496m, Test.Value);
                	
                	try {                		
                		Test = new SqlDecimal (100, 100, false, 
                		                       new int [4] {Int32.MaxValue, 
                		                       Int32.MaxValue, Int32.MaxValue, 
                		                       Int32.MaxValue});
                		Fail ("#A11");
                	} catch (Exception e) {
                		AssertEquals ("#A12", typeof (SqlTypeException), e.GetType ());
                	}

			// sqlDecimal (byte, byte, bool, int, int, int, int)
			Test = new SqlDecimal (12, 2, true, 100, 100, 0, 0);
                	AssertEquals ("#A13", 4294967297m, Test.Value);
                	
                	try {                		
                		Test = new SqlDecimal (100, 100, false, 
                		                       Int32.MaxValue, 
                		                       Int32.MaxValue, Int32.MaxValue, 
                		                       Int32.MaxValue);
                		Fail ("#A14");
                	} catch (Exception e) {
                		AssertEquals ("#A15", typeof (SqlTypeException), e.GetType ());
                	}                	
                }

                // Test public fields
                public void TestPublicFields()
                {
                        AssertEquals ("#B01", (byte)38, SqlDecimal.MaxPrecision);
                        AssertEquals ("#B02", (byte)38, SqlDecimal.MaxScale);
                        
                        // FIXME: on windows: Conversion overflow
                        AssertEquals  ("#B03", Decimal.MaxValue, SqlDecimal.MaxValue.Value);
                        AssertEquals ("#B04", Decimal.MinValue, SqlDecimal.MinValue.Value);
                	Assert ("#B05", SqlDecimal.Null.IsNull);
                	Assert ("#B06", !Test1.IsNull);
                }

                // Test properties
                public void TestProperties()
                {
                	byte[] b = Test1.BinData;
                	AssertEquals ("#C01", (byte)64, b [0]);
                	
                	int[] i = Test1.Data;
                	AssertEquals ("#C02", 64646464, i [0]);
                
                        Assert ("#C03", SqlDecimal.Null.IsNull);
                        Assert ("#C04", Test1.IsPositive);
                        Assert ("#C05", !Test4.IsPositive);
                        AssertEquals ("#C06", (byte)8, Test1.Precision);
                	AssertEquals ("#C07", (byte)0, Test2.Scale);
                	AssertEquals ("#C08", 6464.6464m, Test1.Value); 
                	AssertEquals ("#C09", (byte)4, Test1.Scale);
                        AssertEquals ("#C06", (byte)5, Test2.Precision);
                        AssertEquals ("#C06", (byte)1, Test4.Precision);
                }

                // PUBLIC METHODS

                public void TestArithmeticMethods()
                {

			// Abs
			AssertEquals ("#D01", (SqlDecimal)6m, SqlDecimal.Abs (Test4));
                	AssertEquals ("#D02", (SqlDecimal)6464.6464m, SqlDecimal.Abs (Test1));
                	
                	AssertEquals ("#D03", SqlDecimal.Null, SqlDecimal.Abs (SqlDecimal.Null));
                	
                        // Add()
                        AssertEquals ("#D04", 16464.6464m, SqlDecimal.Add (Test1, Test2).Value);

                        try {
                                SqlDecimal test = SqlDecimal.Add (SqlDecimal.MaxValue, SqlDecimal.MaxValue);
                                Fail ("#D05");
                        } catch (Exception e) {
                                AssertEquals ("#D06", typeof (OverflowException), e.GetType ());
                        }
                        
			AssertEquals ("#D07", (SqlDecimal)6465m, SqlDecimal.Ceiling(Test1));
                	AssertEquals ("#D08", SqlDecimal.Null, SqlDecimal.Ceiling(SqlDecimal.Null));
                	
                        // Divide()
                        AssertEquals ("#D09", (SqlDecimal)(-1077.441066m), SqlDecimal.Divide (Test1, Test4));
                        AssertEquals ("#D10", 1.546875015m, SqlDecimal.Divide (Test2, Test1).Value);

                        try {
                                SqlDecimal test = SqlDecimal.Divide(Test1, new SqlDecimal(0)).Value;
                                Fail ("#D11");
                        } catch(Exception e) {
                                AssertEquals ("#D12", typeof (DivideByZeroException), e.GetType ());
                        }

			AssertEquals ("#D13", (SqlDecimal)6464m, SqlDecimal.Floor (Test1));
                	
                        // Multiply()
                        AssertEquals ("#D14", 64646464m, SqlDecimal.Multiply (Test1, Test2).Value);
                        AssertEquals ("#D15", -38787.8784m, SqlDecimal.Multiply (Test1, Test4).Value);

                        try {
                                SqlDecimal test = SqlDecimal.Multiply (SqlDecimal.MaxValue, Test1);
                                Fail ("#D16");
                        } catch (Exception e) {
                                AssertEquals ("#D17", typeof (OverflowException), e.GetType ());
                        }
                        
                        // Power
                        AssertEquals ("#D18", (SqlDecimal)41791653.0770m, SqlDecimal.Power (Test1, 2));
                       
                       	// Round
                      	AssertEquals ("#D19", (SqlDecimal)6464.65m, SqlDecimal.Round (Test1, 2));
                	
                        // Subtract()
                        AssertEquals ("#D20", -3535.3536m, SqlDecimal.Subtract (Test1, Test3).Value);

                        try {
                                SqlDecimal test = SqlDecimal.Subtract(SqlDecimal.MinValue, SqlDecimal.MaxValue);
                                Fail ("#D21");
                        } catch (Exception e) {
                                AssertEquals ("#D22", typeof (OverflowException), e.GetType ());
                        }                           
                        
                        AssertEquals ("#D23", (SqlInt32)1, SqlDecimal.Sign (Test1));
                        AssertEquals ("#D24", new SqlInt32(-1), SqlDecimal.Sign (Test4));
                }

		public void TestAdjustScale()
		{
			AssertEquals ("#E01", (SqlString)"6464.646400", SqlDecimal.AdjustScale (Test1, 2, false).ToSqlString ());
			AssertEquals ("#E02", (SqlString)"6464.65", SqlDecimal.AdjustScale (Test1, -2, true).ToSqlString ());
			AssertEquals ("#E03", (SqlString)"6464.64", SqlDecimal.AdjustScale (Test1, -2, false).ToSqlString ());
			AssertEquals ("#E01", (SqlString)"10000.0000000000", SqlDecimal.AdjustScale (Test2, 10, false).ToSqlString ());
		}
		
		public void TestConvertToPrecScale()
		{
			AssertEquals ("#F01", new SqlDecimal(6464.6m), SqlDecimal.ConvertToPrecScale (Test1, 5, 1));
			
			try {
				SqlDecimal test =  SqlDecimal.ConvertToPrecScale (Test1, 6, 5);
				Fail ("#F02");
			} catch (Exception e) {
				AssertEquals ("#F03", typeof (SqlTruncateException), e.GetType ());
			}
			
			AssertEquals ("#F01", (SqlString)"10000.00", SqlDecimal.ConvertToPrecScale (Test2, 7, 2).ToSqlString ());			
		}
		
                public void TestCompareTo()
                {
                        SqlString TestString = new SqlString ("This is a test");

                        Assert ("#G01", Test1.CompareTo (Test3) < 0);
                        Assert ("#G02", Test2.CompareTo (Test1) > 0);
                        Assert ("#G03", Test2.CompareTo (Test3) == 0);
                        Assert ("#G04", Test4.CompareTo (SqlDecimal.Null) > 0);

                        try {
                                Test1.CompareTo (TestString);
                                Fail("#G05");
                        } catch(Exception e) {
                                AssertEquals ("#G06", typeof (ArgumentException), e.GetType ());
                        }
                }

                public void TestEqualsMethods()
                {
                        Assert ("#H01", !Test1.Equals (Test2));
                        Assert ("#H02", !Test2.Equals (new SqlString ("TEST")));
                        Assert ("#H03", Test2.Equals (Test3));

                        // Static Equals()-method
                        Assert ("#H05", SqlDecimal.Equals (Test2, Test2).Value);
                        Assert ("#H06", !SqlDecimal.Equals (Test1, Test2).Value);
                	
                	// NotEquals
                        Assert ("#H07", SqlDecimal.NotEquals (Test1, Test2).Value);
                        Assert ("#H08", SqlDecimal.NotEquals (Test4, Test1).Value);
                        Assert ("#H09", !SqlDecimal.NotEquals (Test2, Test3).Value);
                        Assert ("#H10", SqlDecimal.NotEquals (SqlDecimal.Null, Test3).IsNull);                 
                }

                public void TestGetHashCode()
                {
                        // FIXME: Better way to test HashCode
                        AssertEquals ("#I01", -1281249885, Test1.GetHashCode ());
                }

                public void TestGetType()
                {
                        AssertEquals ("#J01", "System.Data.SqlTypes.SqlDecimal", 
                                      Test1.GetType ().ToString ());
                        AssertEquals ("#J02", "System.Decimal", Test1.Value.GetType ().ToString ());
                }

                public void TestGreaters()
                {
                        // GreateThan ()
                        Assert ("#K01", !SqlDecimal.GreaterThan (Test1, Test2).Value);
                        Assert ("#K02", SqlDecimal.GreaterThan (Test2, Test1).Value);
                        Assert ("#K03", !SqlDecimal.GreaterThan (Test2, Test3).Value);

                        // GreaterTharOrEqual ()
                        Assert ("#K04", !SqlDecimal.GreaterThanOrEqual (Test1, Test2).Value);
                        Assert ("#K05", SqlDecimal.GreaterThanOrEqual (Test2, Test1).Value);
                        Assert ("#K06", SqlDecimal.GreaterThanOrEqual (Test2, Test3).Value);
                }

                public void TestLessers()
                {
                        // LessThan()
                        Assert ("#L01", !SqlDecimal.LessThan (Test3, Test2).Value);
                        Assert ("#L02", !SqlDecimal.LessThan (Test2, Test1).Value);
                        Assert ("#L03", SqlDecimal.LessThan (Test1, Test2).Value);

                        // LessThanOrEqual ()
                        Assert ("#L04", SqlDecimal.LessThanOrEqual (Test1, Test2).Value);
                        Assert ("#L05", !SqlDecimal.LessThanOrEqual (Test2, Test1).Value);
                        Assert ("#L06", SqlDecimal.LessThanOrEqual (Test2, Test3).Value);
                        Assert ("#L07", SqlDecimal.LessThanOrEqual (Test1, SqlDecimal.Null).IsNull);
                }

                public void TestParse()
                {
                        try {
                                SqlDecimal.Parse (null);
                                Fail ("#m01");
                        } catch (Exception e) {
                                AssertEquals ("#M02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlDecimal.Parse ("not-a-number");
                                Fail ("#M03");
                        } catch (Exception e) {
                                AssertEquals ("#M04", typeof (FormatException), e.GetType ());
                        }

                         try {
                                SqlDecimal test = SqlDecimal.Parse ("9e300");
                                Fail ("#M05");
                        } catch (Exception e) {
                                AssertEquals ("#M06", typeof (FormatException), e.GetType ());
                        }

                        AssertEquals("#M07", 150m, SqlDecimal.Parse ("150").Value);
                }

                public void TestConversions()
                {
                	// ToDouble
                	AssertEquals ("N01", 6464.6464, Test1.ToDouble ());
                	
                        // ToSqlBoolean ()
                       	AssertEquals ("#N02", new SqlBoolean(1), Test1.ToSqlBoolean ());
                        
                        SqlDecimal Test = new SqlDecimal (0);
                        Assert ("#N03", !Test.ToSqlBoolean ().Value);
                	
                	Test = new SqlDecimal (0);
                	Assert ("#N04", !Test.ToSqlBoolean ().Value);
                        Assert ("#N05", SqlDecimal.Null.ToSqlBoolean ().IsNull);

                        // ToSqlByte ()
                        Test = new SqlDecimal (250);
                        AssertEquals ("#N06", (byte)250, Test.ToSqlByte ().Value);

                        try {
                                SqlByte b = (byte)Test2.ToSqlByte ();
                                Fail ("#N07");
                        } catch (Exception e) {
                                AssertEquals ("#N08", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlDouble ()
                        AssertEquals ("#N09", (SqlDouble)6464.6464, Test1.ToSqlDouble ());

                        // ToSqlInt16 ()
                        AssertEquals ("#N10", (short)1, new SqlDecimal (1).ToSqlInt16 ().Value);

                        try {
                                SqlInt16 test = SqlDecimal.MaxValue.ToSqlInt16().Value;
                                Fail ("#N11");
                        } catch (Exception e) {
                                AssertEquals ("#N12", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlInt32 () 
                        // FIXME: 6464.6464 --> 64646464 ??? with windows
                        AssertEquals ("#N13a", (int)64646464, Test1.ToSqlInt32 ().Value);
			AssertEquals ("#N13b", (int)1212, new SqlDecimal(12.12m).ToSqlInt32 ().Value);
                	
                        try {
                                SqlInt32 test = SqlDecimal.MaxValue.ToSqlInt32 ().Value;
                                Fail ("#N14");
                        } catch (Exception e) { 
                                AssertEquals ("#N15", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlInt64 ()
                        AssertEquals ("#N16", (long)6464, Test1.ToSqlInt64 ().Value);

                        // ToSqlMoney ()
                        AssertEquals ("#N17", (decimal)6464.6464, Test1.ToSqlMoney ().Value);

                        try {
                                SqlMoney test = SqlDecimal.MaxValue.ToSqlMoney ().Value;
                                Fail ("#N18");
                        } catch (Exception e) {
                                AssertEquals ("#N19", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlSingle ()
                        AssertEquals ("#N20", (float)6464.6464, Test1.ToSqlSingle ().Value);

                        // ToSqlString ()
                        AssertEquals ("#N21", "6464.6464", Test1.ToSqlString ().Value);

                        // ToString ()
                        AssertEquals ("#N22", "6464.6464", Test1.ToString ());                        
                }
                
                public void TestTruncate()
                {
                	AssertEquals ("#O01", (SqlDecimal)6464.64m, SqlDecimal.Truncate (Test1, 2));
                }
                
                // OPERATORS

                public void TestArithmeticOperators()
                {
                        // "+"-operator
                        AssertEquals ("#P01", new SqlDecimal(16464.6464m), Test1 + Test2);
     
                        try {
                                SqlDecimal test = SqlDecimal.MaxValue + SqlDecimal.MaxValue;
                                Fail ("#P02");
                        } catch (Exception e) {
                                AssertEquals ("#P03", typeof (OverflowException), e.GetType ());
                        }

                        // "/"-operator
                        AssertEquals ("#P04", (SqlDecimal)1.546875015m, Test2 / Test1);

                        try {
                                SqlDecimal test = Test3 / new SqlDecimal (0);
                                Fail ("#P05");
                        } catch (Exception e) {
                                AssertEquals ("#P06", typeof (DivideByZeroException), e.GetType ());
                        }

                        // "*"-operator
                        AssertEquals ("#P07", (SqlDecimal)64646464m, Test1 * Test2);

                        try {
                                SqlDecimal test = SqlDecimal.MaxValue * Test1;
                                Fail ("#P08");
                        } catch (Exception e) {
                                AssertEquals ("#P09", typeof (OverflowException), e.GetType ());
                        }

                        // "-"-operator
                        AssertEquals ("#P10", (SqlDecimal)3535.3536m, Test2 - Test1);

                        try {
                                SqlDecimal test = SqlDecimal.MinValue - SqlDecimal.MaxValue;
                                Fail ("#P11");
                        } catch  (Exception e) {
                                AssertEquals ("#P12", typeof (OverflowException), e.GetType ());
                        }
                        
                        AssertEquals ("#P13", SqlDecimal.Null, SqlDecimal.Null + Test1);
                }

                public void TestThanOrEqualOperators()
                {

                        // == -operator
                        Assert ("#Q01", (Test2 == Test3).Value);
                        Assert ("#Q02", !(Test1 == Test2).Value);
                        Assert ("#Q03", (Test1 == SqlDecimal.Null).IsNull);
                        
                        // != -operator
                        Assert ("#Q04", !(Test2 != Test3).Value);
                        Assert ("#Q05", (Test1 != Test3).Value);
                        Assert ("#Q06", (Test4 != Test3).Value);
                        Assert ("#Q07", (Test1 != SqlDecimal.Null).IsNull);

                        // > -operator
                        Assert ("#Q08", (Test2 > Test1).Value);
                        Assert ("#Q09", !(Test1 > Test3).Value);
                        Assert ("#Q10", !(Test2 > Test3).Value);
                        Assert ("#Q11", (Test1 > SqlDecimal.Null).IsNull);

                        // >=  -operator
                        Assert ("#Q12", !(Test1 >= Test3).Value);
                        Assert ("#Q13", (Test3 >= Test1).Value);
                        Assert ("#Q14", (Test2 >= Test3).Value);
                        Assert ("#Q15", (Test1 >= SqlDecimal.Null).IsNull);

                        // < -operator
                        Assert ("#Q16", !(Test2 < Test1).Value);
                        Assert ("#Q17", (Test1 < Test3).Value);
                        Assert ("#Q18", !(Test2 < Test3).Value);
                        Assert ("#Q19", (Test1 < SqlDecimal.Null).IsNull);

                        // <= -operator
                        Assert ("#Q20", (Test1 <= Test3).Value);
                        Assert ("#Q21", !(Test3 <= Test1).Value);
                        Assert ("#Q22", (Test2 <= Test3).Value);
                        Assert ("#Q23", (Test1 <= SqlDecimal.Null).IsNull);
                }

                public void TestUnaryNegation()
                {
                        AssertEquals ("#R01", 6m, -Test4.Value);
                        AssertEquals ("#R02", -6464.6464m, -Test1.Value);
                        AssertEquals ("#R03", SqlDecimal.Null, SqlDecimal.Null);
                }

                public void TestSqlBooleanToSqlDecimal()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);
                        SqlDecimal Result;

                        Result = (SqlDecimal)TestBoolean;

                        AssertEquals ("#S01", 1m, Result.Value);

                        Result = (SqlDecimal)SqlBoolean.Null;
                        Assert ("#S02", Result.IsNull);
                	AssertEquals ("#S03", SqlDecimal.Null, (SqlDecimal)SqlBoolean.Null);
                }
		
		public void TestSqlDecimalToDecimal()
		{
			AssertEquals ("#T01", 6464.6464m, (Decimal)Test1);
		}

                public void TestSqlDoubleToSqlDecimal()
                {
                        SqlDouble Test = new SqlDouble (12e10);
                        AssertEquals ("#U01", 120000000000m, ((SqlDecimal)Test).Value);
                }
                
                public void TestSqlSingleToSqlDecimal()
                {
                	SqlSingle Test = new SqlSingle (1e9);
                	AssertEquals ("#V01", 1000000000m, ((SqlDecimal)Test).Value);
                	
                	try {
                		SqlDecimal test = (SqlDecimal)SqlSingle.MaxValue;
                		Fail ("#V02");
                	} catch (Exception e) {
                		AssertEquals ("#V03", typeof (OverflowException), e.GetType ());
                	}
                }

                public void TestSqlStringToSqlDecimal()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");

                        AssertEquals ("#W01", 100m, ((SqlDecimal)TestString100).Value);

                        try {
                                SqlDecimal test = (SqlDecimal)TestString;
                                Fail ("#W02");
                        } catch(Exception e) {
                                AssertEquals ("#W03", typeof (FormatException), e.GetType ());
                        }
                        
                        try {
                        	SqlDecimal test = (SqlDecimal)new SqlString("9e100");
                        	Fail ("#W04");
                        } catch (Exception e) {
                        	AssertEquals ("#W05", typeof (FormatException), e.GetType());
                        }
                }

		public void TestDecimalToSqlDecimal()
		{
			decimal d = 1000.1m;
			AssertEquals ("#X01", (SqlDecimal)1000.1m, (SqlDecimal)d);		
		}
		
                public void TestByteToSqlDecimal()
                {                      
                        AssertEquals ("#Y01", 255m, ((SqlDecimal)SqlByte.MaxValue).Value);
                }
                

                public void TestSqlIntToSqlDouble()
                {
                        SqlInt16 Test64 = new SqlInt16 (64);
                        SqlInt32 Test640 = new SqlInt32 (640);
                        SqlInt64 Test64000 = new SqlInt64 (64000);
                        AssertEquals ("#Z01", 64m, ((SqlDecimal)Test64).Value);
                        AssertEquals ("#Z02", 640m,((SqlDecimal)Test640).Value);
                        AssertEquals ("#Z03", 64000m, ((SqlDecimal)Test64000).Value);
                }


                public void TestSqlMoneyToSqlDecimal()
                {
                        SqlMoney TestMoney64 = new SqlMoney(64);
                        AssertEquals ("#AA01", 64M, ((SqlDecimal)TestMoney64).Value);
                }
        }
}

