//
// SqlGuidTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlGuid
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
        public class SqlGuidTest : TestCase {

		// 00000a01-0000-0000-0000-000000000000
		private SqlGuid Test1;

        	// 00000f64-0000-0000-0000-000000000000
        	private SqlGuid Test2;         	
        	private SqlGuid Test3;

		// 0000fafa-0000-0000-0000-000000000000
		private SqlGuid Test4;
        	
                public SqlGuidTest() : base ("System.Data.SqlTypes.SqlGuid") {}
                public SqlGuidTest(string name) : base(name) {}

                protected override void TearDown() {}

                protected override void SetUp() 
                {
                	byte [] b1 = new byte [16];
                	byte [] b2 = new byte [16];
                	byte [] b3 = new byte [16];
                	byte [] b4 = new byte [16];

                	b1 [0] = 1;
                	b1 [1] = 10;
                	b2 [0] = 100;
                	b2 [1] = 15;
                	b3 [0] = 100;
                	b3 [1] = 15;
                	b4 [0] = 250;
                	b4 [1] = 250;

                   	Test1 = new SqlGuid (b1);
			Test2 = new SqlGuid (b2);
                	Test3 = new SqlGuid (b3);
                	Test4 = new SqlGuid (b4);
                }



                public static ITest Suite {
                        get {
                                return new TestSuite(typeof(SqlGuid));
                        }
                }

                // Test constructor
                public void TestCreate()
                {
			// SqlGuid (Byte[])
			byte [] b = new byte [16];
                	b [0] = 100;
                	b [1] = 200;

                	try {
                		SqlGuid Test = new SqlGuid (b);

				// SqlGuid (Guid)
				Guid TestGuid = new Guid (b);
                		Test = new SqlGuid (TestGuid);

				// SqlGuid (string)
				Test = new SqlGuid ("12345678-1234-1234-1234-123456789012");

				// SqlGuid (int, short, short, byte, byte, byte, byte, byte, byte, byte, byte)
                		Test = new SqlGuid (10, 1, 2, 13, 14, 15, 16, 17, 19, 20 ,21);

                	} catch (Exception e) {
                		Fail ("#A01 " + e);
                	}
                }

                // Test public fields
                public void TestPublicFields()
                {
                        Assert ("#B01", SqlGuid.Null.IsNull);
                }

                // Test properties
                public void TestProperties()
                {
                	Guid ResultGuid = new Guid ("00000f64-0000-0000-0000-000000000000");		       
			Assert ("#C01", !Test1.IsNull);
                	Assert ("#C02", SqlGuid.Null.IsNull);
                	AssertEquals ("#C03", ResultGuid, Test2.Value);
                }

                // PUBLIC METHODS

                public void TestCompareTo()
                {
			String TestString = "This is a test string";
                        Assert ("#D01", Test1.CompareTo (Test3) < 0);
                        Assert ("#D02", Test4.CompareTo (Test1) > 0);
                        Assert ("#D03", Test3.CompareTo (Test2) == 0);
                        Assert ("#D04", Test4.CompareTo (SqlGuid.Null) > 0);

                        try {
                                Test1.CompareTo (TestString);
                                Fail("#D05");
                        } catch(Exception e) {
                                AssertEquals ("#D06", typeof (ArgumentException), e.GetType ());
                        }
                }

                public void TestEqualsMethods()
                {
                        Assert ("#E01", !Test1.Equals (Test2));
                        Assert ("#E02", !Test2.Equals (Test4));
                        Assert ("#E03", !Test2.Equals (new SqlString ("TEST")));
                        Assert ("#E04", Test2.Equals (Test3));

                        // Static Equals()-method
                        Assert ("#E05", SqlGuid.Equals (Test2, Test3).Value);
                        Assert ("#E06", !SqlGuid.Equals (Test1, Test2).Value);
                }

                public void TestGetHashCode()
                {
                        AssertEquals ("#F01", Test1.GetHashCode (), Test1.GetHashCode ());
                	Assert ("#F02", Test1.GetHashCode () != Test2.GetHashCode ());
                        AssertEquals ("#F02", Test3.GetHashCode (), Test2.GetHashCode ());
                }

                public void TestGetType()
                {
                        AssertEquals ("#G01", "System.Data.SqlTypes.SqlGuid", Test1.GetType ().ToString ());
                        AssertEquals ("#G02", "System.Guid", Test3.Value.GetType ().ToString ());
                }

                public void TestGreaters()
                {
                        // GreateThan ()
                        Assert ("#H01", !SqlGuid.GreaterThan (Test1, Test2).Value);
                        Assert ("#H02", SqlGuid.GreaterThan (Test2, Test1).Value);
                        Assert ("#H03", !SqlGuid.GreaterThan (Test2, Test3).Value);
                        // GreaterTharOrEqual ()
                        Assert ("#H04", !SqlGuid.GreaterThanOrEqual (Test1, Test2).Value);
                        Assert ("#H05", SqlGuid.GreaterThanOrEqual (Test2, Test1).Value);
                        Assert ("#H06", SqlGuid.GreaterThanOrEqual (Test2, Test3).Value);
                }

                public void TestLessers()
                {
                        // LessThan()
                        Assert ("#I01", !SqlGuid.LessThan (Test2, Test3).Value);
                        Assert ("#I02", !SqlGuid.LessThan (Test2, Test1).Value);
                        Assert ("#I03", SqlGuid.LessThan (Test1, Test2).Value);

                        // LessThanOrEqual ()
                        Assert ("#I04", SqlGuid.LessThanOrEqual (Test1, Test2).Value);
                        Assert ("#I05", !SqlGuid.LessThanOrEqual (Test2, Test1).Value);
                        Assert ("#I06", SqlGuid.LessThanOrEqual (Test2, Test3).Value);
                        Assert ("#I07", SqlGuid.LessThanOrEqual (Test4, SqlGuid.Null).IsNull);
                }

                public void TestNotEquals()
                {
                        Assert ("#J01", SqlGuid.NotEquals (Test1, Test2).Value);
                        Assert ("#J02", SqlGuid.NotEquals (Test2, Test1).Value);
                        Assert ("#J03", SqlGuid.NotEquals (Test3, Test1).Value);
                        Assert ("#J04", !SqlGuid.NotEquals (Test3, Test2).Value);                      
                        Assert ("#J05", SqlGuid.NotEquals (SqlGuid.Null, Test2).IsNull);
                }

                public void TestParse()
                {
                        try {
                                SqlGuid.Parse (null);
                                Fail ("#K01");
                        } catch (Exception e) {
                                AssertEquals ("#K02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlGuid.Parse ("not-a-number");
                                Fail ("#K03");
                        } catch (Exception e) {
                                AssertEquals ("#K04", typeof (FormatException), e.GetType ());
                        }

                         try {
                                SqlGuid.Parse ("9e400");
                                Fail ("#K05");
                        } catch (Exception e) {
                                AssertEquals ("#K06", typeof (FormatException), e.GetType ());
                        }

                        AssertEquals("#K07", new Guid("87654321-0000-0000-0000-000000000000"), 
                                     SqlGuid.Parse ("87654321-0000-0000-0000-000000000000").Value);
                }

                public void TestConversions()
                {
			// ToByteArray ()
			AssertEquals ("#L01", (byte)1, Test1.ToByteArray () [0]);
			AssertEquals ("#L02", (byte)15, Test2.ToByteArray () [1]);

			// ToSqlBinary ()
			byte [] b = new byte [2]; 
                	b [0] = 100;
                	b [1] = 15;
		       
                        AssertEquals ("#L03", new SqlBinary (b), Test3.ToSqlBinary ());

                        // ToSqlString ()
                        AssertEquals ("#L04", "00000a01-0000-0000-0000-000000000000",  
				      Test1.ToSqlString ().Value);
                        AssertEquals ("#L05", "0000fafa-0000-0000-0000-000000000000", 
                                      Test4.ToSqlString ().Value);

                        // ToString ()
                        AssertEquals ("#L06", "00000a01-0000-0000-0000-000000000000", 
                                      Test1.ToString ());
                        AssertEquals ("#L07", "0000fafa-0000-0000-0000-000000000000", 
                                      Test4.ToString ());
                }

                // OPERATORS

                public void TestThanOrEqualOperators()
                {
                        // == -operator
                        Assert ("#M01", (Test3 == Test2).Value);
                        Assert ("#M02", !(Test1 == Test2).Value);
                        Assert ("#M03", (Test1 == SqlGuid.Null).IsNull);
                        
                        // != -operator
                        Assert ("#M04", !(Test2 != Test3).Value);
                        Assert ("#M05", (Test1 != Test3).Value);
                        Assert ("#M06", (Test1 != SqlGuid.Null).IsNull);

                        // > -operator
                        Assert ("#M07", (Test2 > Test1).Value);
                        Assert ("#M08", !(Test1 > Test3).Value);
                        Assert ("#M09", !(Test3 > Test2).Value);
                        Assert ("#M10", (Test1 > SqlGuid.Null).IsNull);

                        // >=  -operator
                        Assert ("#M12", !(Test1 >= Test3).Value);
                        Assert ("#M13", (Test3 >= Test1).Value);
                        Assert ("#M14", (Test3 >= Test2).Value);
                        Assert ("#M15", (Test1 >= SqlGuid.Null).IsNull);

                        // < -operator
                        Assert ("#M16", !(Test2 < Test1).Value);
                        Assert ("#M17", (Test1 < Test3).Value);
                        Assert ("#M18", !(Test2 < Test3).Value);
                        Assert ("#M19", (Test1 < SqlGuid.Null).IsNull);

                        // <= -operator
                        Assert ("#M20", (Test1 <= Test3).Value);
                        Assert ("#M21", !(Test3 <= Test1).Value);
                        Assert ("#M22", (Test2 <= Test3).Value);
                        Assert ("#M23", (Test1 <= SqlGuid.Null).IsNull);
                }

		public void TestSqlBinaryToSqlGuid()
		{
			byte [] b = new byte [16];
			b [0] = 100;
			b [1] = 200;
			SqlBinary TestBinary = new SqlBinary (b);
			
			AssertEquals ("#N01", new Guid("0000c864-0000-0000-0000-000000000000"), 
				      ((SqlGuid)TestBinary).Value);
		}


		public void TestSqlGuidToGuid()
		{
			AssertEquals ("#O01", new Guid("00000a01-0000-0000-0000-000000000000"), 
			              (Guid)Test1);
			AssertEquals ("#O02", new Guid("00000f64-0000-0000-0000-000000000000"), 
			              (Guid)Test2);
		}		

                public void TestSqlStringToSqlGuid()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("0000c864-0000-0000-0000-000000000000");

                        AssertEquals ("#P01", new Guid("0000c864-0000-0000-0000-000000000000"), 
                                      ((SqlGuid)TestString100).Value);

                        try {
                                SqlGuid test = (SqlGuid)TestString;
                                Fail ("#P02");
                        } catch(Exception e) {
                                AssertEquals ("#P03", typeof (FormatException), e.GetType ());
                        }
                }
		
		public void TestGuidToSqlGuid()
		{
			Guid TestGuid = new Guid("0000c864-0000-0000-0000-000007650000");
			AssertEquals ("#Q01", new SqlGuid("0000c864-0000-0000-0000-000007650000"), 
				      (SqlGuid)TestGuid);
		}
        }
}
