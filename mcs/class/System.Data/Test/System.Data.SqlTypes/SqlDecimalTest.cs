//
// SqlDecimalTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlDecimal
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen
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
        public class SqlDecimalTest {

		private SqlDecimal Test1;
        	private SqlDecimal Test2;
        	private SqlDecimal Test3;
        	private SqlDecimal Test4;
        	
		[SetUp]
                public void GetReady() 
                {
                	Test1 = new SqlDecimal (6464.6464m);
                	Test2 = new SqlDecimal (10000.00m); 
                	Test3 = new SqlDecimal (10000.00m);                 
                	Test4 = new SqlDecimal (-6m);                 
                }

                // Test constructor
		[Test]
                public void Create()
                {
                	// SqlDecimal (decimal)
			SqlDecimal Test = new SqlDecimal (30.3098m);
                	Assertion.AssertEquals ("#A01", (decimal)30.3098, Test.Value);
                	
                	try {
                		SqlDecimal test = new SqlDecimal (Decimal.MaxValue + 1);
                		Assertion.Fail ("#A02");                		
                	} catch (Exception e) {
                		Assertion.AssertEquals ("#A03", typeof (OverflowException), e.GetType ());
                	}
                	
                	// SqlDecimal (double)
                	Test = new SqlDecimal (10E+10d);
                	Assertion.AssertEquals ("#A05", 100000000000m, Test.Value);
                	
                	try {
                		SqlDecimal test = new SqlDecimal (10E+200d);
                		Assertion.Fail ("#A06");                		
                	} catch (Exception e) {
                		Assertion.AssertEquals ("#A07", typeof (OverflowException), e.GetType ());
                	}
                	
                	// SqlDecimal (int)
                	Test = new SqlDecimal (-1);
                	Assertion.AssertEquals ("#A08", -1m, Test.Value);
                
			// SqlDecimal (long)
                	Test = new SqlDecimal ((long)(-99999));
                	Assertion.AssertEquals ("#A09", -99999m, Test.Value);
                
                	// SqlDecimal (byte, byte, bool. int[]
                 	Test = new SqlDecimal (10, 3, false, new int [4] {200, 1, 0, 0});
                	Assertion.AssertEquals ("#A10", -4294967.496m, Test.Value);
                	
                	try {                		
                		Test = new SqlDecimal (100, 100, false, 
                		                       new int [4] {Int32.MaxValue, 
                		                       Int32.MaxValue, Int32.MaxValue, 
                		                       Int32.MaxValue});
                		Assertion.Fail ("#A11");
                	} catch (Exception e) {
                		Assertion.AssertEquals ("#A12", typeof (SqlTypeException), e.GetType ());
                	}

			// sqlDecimal (byte, byte, bool, int, int, int, int)
			Test = new SqlDecimal (12, 2, true, 100, 100, 0, 0);
                	Assertion.AssertEquals ("#A13", 4294967297m, Test.Value);
                	
                	try {                		
                		Test = new SqlDecimal (100, 100, false, 
                		                       Int32.MaxValue, 
                		                       Int32.MaxValue, Int32.MaxValue, 
                		                       Int32.MaxValue);
                		Assertion.Fail ("#A14");
                	} catch (Exception e) {
                		Assertion.AssertEquals ("#A15", typeof (SqlTypeException), e.GetType ());
                	}                	
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        Assertion.AssertEquals ("#B01", (byte)38, SqlDecimal.MaxPrecision);
                        Assertion.AssertEquals ("#B02", (byte)38, SqlDecimal.MaxScale);
                        
                        // FIXME: on windows: Conversion overflow
			Assertion.AssertEquals  ("#B03a", 1262177448, SqlDecimal.MaxValue.Data [3]);


                        Assertion.AssertEquals ("#B04", 1262177448, SqlDecimal.MinValue.Data [3]);
                	Assertion.Assert ("#B05", SqlDecimal.Null.IsNull);
                	Assertion.Assert ("#B06", !Test1.IsNull);
                }

                // Test properties
		[Test]
                public void Properties()
                {
                	byte[] b = Test1.BinData;
                	Assertion.AssertEquals ("#C01", (byte)64, b [0]);
                	
                	int[] i = Test1.Data;
                	Assertion.AssertEquals ("#C02", 64646464, i [0]);
                
                        Assertion.Assert ("#C03", SqlDecimal.Null.IsNull);
                        Assertion.Assert ("#C04", Test1.IsPositive);
                        Assertion.Assert ("#C05", !Test4.IsPositive);
                        Assertion.AssertEquals ("#C06", (byte)8, Test1.Precision);
                	Assertion.AssertEquals ("#C07", (byte)0, Test2.Scale);
                	Assertion.AssertEquals ("#C08", 6464.6464m, Test1.Value); 
                	Assertion.AssertEquals ("#C09", (byte)4, Test1.Scale);
                        Assertion.AssertEquals ("#C06", (byte)5, Test2.Precision);
                        Assertion.AssertEquals ("#C06", (byte)1, Test4.Precision);
                }

                // PUBLIC METHODS
		[Test]
                public void ArithmeticMethods()
                {

			// Abs
			Assertion.AssertEquals ("#D01", (SqlDecimal)6m, SqlDecimal.Abs (Test4));
                	Assertion.AssertEquals ("#D02", new SqlDecimal (6464.6464m).Value, SqlDecimal.Abs (Test1).Value);
                	
                	Assertion.AssertEquals ("#D03", SqlDecimal.Null, SqlDecimal.Abs (SqlDecimal.Null));
                	
                        // Add()
                        Assertion.AssertEquals ("#D04", 16464.6464m, SqlDecimal.Add (Test1, Test2).Value);

                        try {
                                SqlDecimal test = SqlDecimal.Add (SqlDecimal.MaxValue, SqlDecimal.MaxValue);
                                Assertion.Fail ("#D05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#D06", typeof (OverflowException), e.GetType ());
                        }
                        
			Assertion.AssertEquals ("#D07", (SqlDecimal)6465m, SqlDecimal.Ceiling(Test1));
                	Assertion.AssertEquals ("#D08", SqlDecimal.Null, SqlDecimal.Ceiling(SqlDecimal.Null));
                	
                        // Divide()
                        Assertion.AssertEquals ("#D09", (SqlDecimal)(-1077.441066m), SqlDecimal.Divide (Test1, Test4));
                        Assertion.AssertEquals ("#D10", 1.546875015m, SqlDecimal.Divide (Test2, Test1).Value);

                        try {
                                SqlDecimal test = SqlDecimal.Divide(Test1, new SqlDecimal(0)).Value;
                                Assertion.Fail ("#D11");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#D12", typeof (DivideByZeroException), e.GetType ());
                        }

			Assertion.AssertEquals ("#D13", (SqlDecimal)6464m, SqlDecimal.Floor (Test1));
                	
                        // Multiply()
                        Assertion.AssertEquals ("#D14", 64646464m, SqlDecimal.Multiply (Test1, Test2).Value);
                        Assertion.AssertEquals ("#D15", -38787.8784m, SqlDecimal.Multiply (Test1, Test4).Value);

                        try {
                                SqlDecimal test = SqlDecimal.Multiply (SqlDecimal.MaxValue, Test1);
                                Assertion.Fail ("#D16");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#D17", typeof (OverflowException), e.GetType ());
                        }
                        
                        // Power
                        Assertion.AssertEquals ("#D18", (SqlDecimal)41791653.0770m, SqlDecimal.Power (Test1, 2));
                       
                       	// Round
                      	Assertion.AssertEquals ("#D19", (SqlDecimal)6464.65m, SqlDecimal.Round (Test1, 2));
                	
                        // Subtract()
                        Assertion.AssertEquals ("#D20", -3535.3536m, SqlDecimal.Subtract (Test1, Test3).Value);

                        try {
                                SqlDecimal test = SqlDecimal.Subtract(SqlDecimal.MinValue, SqlDecimal.MaxValue);
                                Assertion.Fail ("#D21");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#D22", typeof (OverflowException), e.GetType ());
                        }                           
                        
                        Assertion.AssertEquals ("#D23", (SqlInt32)1, SqlDecimal.Sign (Test1));
                        Assertion.AssertEquals ("#D24", new SqlInt32(-1), SqlDecimal.Sign (Test4));
                }

		[Test]
		public void AdjustScale()
		{
			Assertion.AssertEquals ("#E01", (SqlString)"6464.646400", SqlDecimal.AdjustScale (Test1, 2, false).ToSqlString ());
			Assertion.AssertEquals ("#E02", (SqlString)"6464.65", SqlDecimal.AdjustScale (Test1, -2, true).ToSqlString ());
			Assertion.AssertEquals ("#E03", (SqlString)"6464.64", SqlDecimal.AdjustScale (Test1, -2, false).ToSqlString ());
			Assertion.AssertEquals ("#E01", (SqlString)"10000.0000000000", SqlDecimal.AdjustScale (Test2, 10, false).ToSqlString ());
		}
		
		[Test]
		public void ConvertToPrecScale()
		{
			Assertion.AssertEquals ("#F01", new SqlDecimal(6464.6m).Value, SqlDecimal.ConvertToPrecScale (Test1, 5, 1).Value);
			
			try {
				SqlDecimal test =  SqlDecimal.ConvertToPrecScale (Test1, 6, 5);
				Assertion.Fail ("#F02");
			} catch (Exception e) {
				Assertion.AssertEquals ("#F03", typeof (SqlTruncateException), e.GetType ());
			}
			
			Assertion.AssertEquals ("#F01", (SqlString)"10000.00", SqlDecimal.ConvertToPrecScale (Test2, 7, 2).ToSqlString ());			
		}
		
		[Test]
                public void CompareTo()
                {
                        SqlString TestString = new SqlString ("This is a test");

                        Assertion.Assert ("#G01", Test1.CompareTo (Test3) < 0);
                        Assertion.Assert ("#G02", Test2.CompareTo (Test1) > 0);
                        Assertion.Assert ("#G03", Test2.CompareTo (Test3) == 0);
                        Assertion.Assert ("#G04", Test4.CompareTo (SqlDecimal.Null) > 0);

                        try {
                                Test1.CompareTo (TestString);
                                Assertion.Fail("#G05");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#G06", typeof (ArgumentException), e.GetType ());
                        }
                }

		[Test]
                public void EqualsMethods()
                {
                        Assertion.Assert ("#H01", !Test1.Equals (Test2));
                        Assertion.Assert ("#H02", !Test2.Equals (new SqlString ("TEST")));
                        Assertion.Assert ("#H03", Test2.Equals (Test3));

                        // Static Equals()-method
                        Assertion.Assert ("#H05", SqlDecimal.Equals (Test2, Test2).Value);
                        Assertion.Assert ("#H06", !SqlDecimal.Equals (Test1, Test2).Value);
                	
                	// NotEquals
                        Assertion.Assert ("#H07", SqlDecimal.NotEquals (Test1, Test2).Value);
                        Assertion.Assert ("#H08", SqlDecimal.NotEquals (Test4, Test1).Value);
                        Assertion.Assert ("#H09", !SqlDecimal.NotEquals (Test2, Test3).Value);
                        Assertion.Assert ("#H10", SqlDecimal.NotEquals (SqlDecimal.Null, Test3).IsNull);                 
                }

		[Test]
                public void GetHashCodeTest()
                {
                        // FIXME: Better way to test HashCode
                        Assertion.AssertEquals ("#I01", -1281249885, Test1.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
                        Assertion.AssertEquals ("#J01", "System.Data.SqlTypes.SqlDecimal", 
                                      Test1.GetType ().ToString ());
                        Assertion.AssertEquals ("#J02", "System.Decimal", Test1.Value.GetType ().ToString ());
                }

		[Test]
                public void Greaters()
                {
                        // GreateThan ()
                        Assertion.Assert ("#K01", !SqlDecimal.GreaterThan (Test1, Test2).Value);
                        Assertion.Assert ("#K02", SqlDecimal.GreaterThan (Test2, Test1).Value);
                        Assertion.Assert ("#K03", !SqlDecimal.GreaterThan (Test2, Test3).Value);

                        // GreaterTharOrEqual ()
                        Assertion.Assert ("#K04", !SqlDecimal.GreaterThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#K05", SqlDecimal.GreaterThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#K06", SqlDecimal.GreaterThanOrEqual (Test2, Test3).Value);
                }

		[Test]
                public void Lessers()
                {
                        // LessThan()
                        Assertion.Assert ("#L01", !SqlDecimal.LessThan (Test3, Test2).Value);
                        Assertion.Assert ("#L02", !SqlDecimal.LessThan (Test2, Test1).Value);
                        Assertion.Assert ("#L03", SqlDecimal.LessThan (Test1, Test2).Value);

                        // LessThanOrEqual ()
                        Assertion.Assert ("#L04", SqlDecimal.LessThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#L05", !SqlDecimal.LessThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#L06", SqlDecimal.LessThanOrEqual (Test2, Test3).Value);
                        Assertion.Assert ("#L07", SqlDecimal.LessThanOrEqual (Test1, SqlDecimal.Null).IsNull);
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlDecimal.Parse (null);
                                Assertion.Fail ("#m01");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlDecimal.Parse ("not-a-number");
                                Assertion.Fail ("#M03");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M04", typeof (FormatException), e.GetType ());
                        }

                         try {
                                SqlDecimal test = SqlDecimal.Parse ("9e300");
                                Assertion.Fail ("#M05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M06", typeof (FormatException), e.GetType ());
                        }

                        Assertion.AssertEquals("#M07", 150m, SqlDecimal.Parse ("150").Value);
                }

		[Test]
                public void Conversions()
                {
                	// ToDouble
                	Assertion.AssertEquals ("N01", 6464.6464, Test1.ToDouble ());
                	
                        // ToSqlBoolean ()
                       	Assertion.AssertEquals ("#N02", new SqlBoolean(1), Test1.ToSqlBoolean ());
                        
                        SqlDecimal Test = new SqlDecimal (0);
                        Assertion.Assert ("#N03", !Test.ToSqlBoolean ().Value);
                	
                	Test = new SqlDecimal (0);
                	Assertion.Assert ("#N04", !Test.ToSqlBoolean ().Value);
                        Assertion.Assert ("#N05", SqlDecimal.Null.ToSqlBoolean ().IsNull);

                        // ToSqlByte ()
                        Test = new SqlDecimal (250);
                        Assertion.AssertEquals ("#N06", (byte)250, Test.ToSqlByte ().Value);

                        try {
                                SqlByte b = (byte)Test2.ToSqlByte ();
                                Assertion.Fail ("#N07");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N08", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlDouble ()
                        Assertion.AssertEquals ("#N09", (SqlDouble)6464.6464, Test1.ToSqlDouble ());

                        // ToSqlInt16 ()
                        Assertion.AssertEquals ("#N10", (short)1, new SqlDecimal (1).ToSqlInt16 ().Value);

                        try {
                                SqlInt16 test = SqlDecimal.MaxValue.ToSqlInt16().Value;
                                Assertion.Fail ("#N11");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N12", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlInt32 () 
                        // FIXME: 6464.6464 --> 64646464 ??? with windows
                        Assertion.AssertEquals ("#N13a", (int)64646464, Test1.ToSqlInt32 ().Value);
			Assertion.AssertEquals ("#N13b", (int)1212, new SqlDecimal(12.12m).ToSqlInt32 ().Value);
                	
                        try {
                                SqlInt32 test = SqlDecimal.MaxValue.ToSqlInt32 ().Value;
                                Assertion.Fail ("#N14");
                        } catch (Exception e) { 
                                Assertion.AssertEquals ("#N15", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlInt64 ()
                        Assertion.AssertEquals ("#N16", (long)6464, Test1.ToSqlInt64 ().Value);

                        // ToSqlMoney ()
                        Assertion.AssertEquals ("#N17", (decimal)6464.6464, Test1.ToSqlMoney ().Value);

                        try {
                                SqlMoney test = SqlDecimal.MaxValue.ToSqlMoney ().Value;
                                Assertion.Fail ("#N18");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N19", typeof (OverflowException), e.GetType ());
                        }        

                        // ToSqlSingle ()
                        Assertion.AssertEquals ("#N20", (float)6464.6464, Test1.ToSqlSingle ().Value);

                        // ToSqlString ()
                        Assertion.AssertEquals ("#N21", "6464.6464", Test1.ToSqlString ().Value);

                        // ToString ()
                        Assertion.AssertEquals ("#N22", "6464.6464", Test1.ToString ());                        
			Assertion.AssertEquals ("#N23", (SqlDouble)1E+38, SqlDecimal.MaxValue.ToSqlDouble ());

                }
                
		[Test]
                public void Truncate()
                {
                	Assertion.AssertEquals ("#O01", new SqlDecimal (6464.64m).Value, SqlDecimal.Truncate (Test1, 2).Value);
                }
                
                // OPERATORS

		[Test]
                public void ArithmeticOperators()
                {
                        // "+"-operator
                        Assertion.AssertEquals ("#P01", new SqlDecimal(16464.6464m), Test1 + Test2);
     
                        try {
                                SqlDecimal test = SqlDecimal.MaxValue + SqlDecimal.MaxValue;
                                Assertion.Fail ("#P02");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#P03", typeof (OverflowException), e.GetType ());
                        }

                        // "/"-operator
                        Assertion.AssertEquals ("#P04", (SqlDecimal)1.546875015m, Test2 / Test1);

                        try {
                                SqlDecimal test = Test3 / new SqlDecimal (0);
                                Assertion.Fail ("#P05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#P06", typeof (DivideByZeroException), e.GetType ());
                        }

                        // "*"-operator
                        Assertion.AssertEquals ("#P07", (SqlDecimal)64646464m, Test1 * Test2);

                        try {
                                SqlDecimal test = SqlDecimal.MaxValue * Test1;
                                Assertion.Fail ("#P08");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#P09", typeof (OverflowException), e.GetType ());
                        }

                        // "-"-operator
                        Assertion.AssertEquals ("#P10", (SqlDecimal)3535.3536m, Test2 - Test1);

                        try {
                                SqlDecimal test = SqlDecimal.MinValue - SqlDecimal.MaxValue;
                                Assertion.Fail ("#P11");
                        } catch  (Exception e) {
                                Assertion.AssertEquals ("#P12", typeof (OverflowException), e.GetType ());
                        }
                        
                        Assertion.AssertEquals ("#P13", SqlDecimal.Null, SqlDecimal.Null + Test1);
                }

		[Test]
                public void ThanOrEqualOperators()
                {

                        // == -operator
                        Assertion.Assert ("#Q01", (Test2 == Test3).Value);
                        Assertion.Assert ("#Q02", !(Test1 == Test2).Value);
                        Assertion.Assert ("#Q03", (Test1 == SqlDecimal.Null).IsNull);
                        
                        // != -operator
                        Assertion.Assert ("#Q04", !(Test2 != Test3).Value);
                        Assertion.Assert ("#Q05", (Test1 != Test3).Value);
                        Assertion.Assert ("#Q06", (Test4 != Test3).Value);
                        Assertion.Assert ("#Q07", (Test1 != SqlDecimal.Null).IsNull);

                        // > -operator
                        Assertion.Assert ("#Q08", (Test2 > Test1).Value);
                        Assertion.Assert ("#Q09", !(Test1 > Test3).Value);
                        Assertion.Assert ("#Q10", !(Test2 > Test3).Value);
                        Assertion.Assert ("#Q11", (Test1 > SqlDecimal.Null).IsNull);

                        // >=  -operator
                        Assertion.Assert ("#Q12", !(Test1 >= Test3).Value);
                        Assertion.Assert ("#Q13", (Test3 >= Test1).Value);
                        Assertion.Assert ("#Q14", (Test2 >= Test3).Value);
                        Assertion.Assert ("#Q15", (Test1 >= SqlDecimal.Null).IsNull);

                        // < -operator
                        Assertion.Assert ("#Q16", !(Test2 < Test1).Value);
                        Assertion.Assert ("#Q17", (Test1 < Test3).Value);
                        Assertion.Assert ("#Q18", !(Test2 < Test3).Value);
                        Assertion.Assert ("#Q19", (Test1 < SqlDecimal.Null).IsNull);

                        // <= -operator
                        Assertion.Assert ("#Q20", (Test1 <= Test3).Value);
                        Assertion.Assert ("#Q21", !(Test3 <= Test1).Value);
                        Assertion.Assert ("#Q22", (Test2 <= Test3).Value);
                        Assertion.Assert ("#Q23", (Test1 <= SqlDecimal.Null).IsNull);
                }

		[Test]
                public void UnaryNegation()
                {
                        Assertion.AssertEquals ("#R01", 6m, -Test4.Value);
                        Assertion.AssertEquals ("#R02", -6464.6464m, -Test1.Value);
                        Assertion.AssertEquals ("#R03", SqlDecimal.Null, SqlDecimal.Null);
                }

		[Test]
                public void SqlBooleanToSqlDecimal()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);
                        SqlDecimal Result;

                        Result = (SqlDecimal)TestBoolean;

                        Assertion.AssertEquals ("#S01", 1m, Result.Value);

                        Result = (SqlDecimal)SqlBoolean.Null;
                        Assertion.Assert ("#S02", Result.IsNull);
                	Assertion.AssertEquals ("#S03", SqlDecimal.Null, (SqlDecimal)SqlBoolean.Null);
                }
		
		[Test]
		public void SqlDecimalToDecimal()
		{
			Assertion.AssertEquals ("#T01", 6464.6464m, (Decimal)Test1);
		}

		[Test]
                public void SqlDoubleToSqlDecimal()
                {
                        SqlDouble Test = new SqlDouble (12E+10);
                        Assertion.AssertEquals ("#U01", 120000000000m, ((SqlDecimal)Test).Value);
                }
                
		[Test]
                public void SqlSingleToSqlDecimal()
                {
                	SqlSingle Test = new SqlSingle (1E+9);
                	Assertion.AssertEquals ("#V01", 1000000000m, ((SqlDecimal)Test).Value);
                	
                	try {
                		SqlDecimal test = (SqlDecimal)SqlSingle.MaxValue;
                		Assertion.Fail ("#V02");
                	} catch (Exception e) {
                		Assertion.AssertEquals ("#V03", typeof (OverflowException), e.GetType ());
                	}
                }

		[Test]
                public void SqlStringToSqlDecimal()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("100");

                        Assertion.AssertEquals ("#W01", 100m, ((SqlDecimal)TestString100).Value);

                        try {
                                SqlDecimal test = (SqlDecimal)TestString;
                                Assertion.Fail ("#W02");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#W03", typeof (FormatException), e.GetType ());
                        }
                        
                        try {
                        	SqlDecimal test = (SqlDecimal)new SqlString("9E+100");
                        	Assertion.Fail ("#W04");
                        } catch (Exception e) {
                        	Assertion.AssertEquals ("#W05", typeof (FormatException), e.GetType());
                        }
                }

		[Test]
		public void DecimalToSqlDecimal()
		{
			decimal d = 1000.1m;
			Assertion.AssertEquals ("#X01", (SqlDecimal)1000.1m, (SqlDecimal)d);		
		}
		
		[Test]
                public void ByteToSqlDecimal()
                {                      
                        Assertion.AssertEquals ("#Y01", 255m, ((SqlDecimal)SqlByte.MaxValue).Value);
                }
                
		[Test]
                public void SqlIntToSqlDouble()
                {
                        SqlInt16 Test64 = new SqlInt16 (64);
                        SqlInt32 Test640 = new SqlInt32 (640);
                        SqlInt64 Test64000 = new SqlInt64 (64000);
                        Assertion.AssertEquals ("#Z01", 64m, ((SqlDecimal)Test64).Value);
                        Assertion.AssertEquals ("#Z02", 640m,((SqlDecimal)Test640).Value);
                        Assertion.AssertEquals ("#Z03", 64000m, ((SqlDecimal)Test64000).Value);
                }

		[Test]
                public void SqlMoneyToSqlDecimal()
                {
                        SqlMoney TestMoney64 = new SqlMoney(64);
                        Assertion.AssertEquals ("#AA01", 64M, ((SqlDecimal)TestMoney64).Value);
                }
        }
}

