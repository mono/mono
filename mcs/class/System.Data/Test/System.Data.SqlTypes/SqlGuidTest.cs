//
// SqlGuidTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlGuid
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
        public class SqlGuidTest {

		// 00000a01-0000-0000-0000-000000000000
		private SqlGuid Test1;

        	// 00000f64-0000-0000-0000-000000000000
        	private SqlGuid Test2;         	
        	private SqlGuid Test3;

		// 0000fafa-0000-0000-0000-000000000000
		private SqlGuid Test4;
        	
		[SetUp]
                public void GetReady() 
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

                // Test constructor
		[Test]
                public void Create()
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
                		Assertion.Fail ("#A01 " + e);
                	}
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        Assertion.Assert ("#B01", SqlGuid.Null.IsNull);
                }

                // Test properties
		[Test]
                public void Properties()
                {
                	Guid ResultGuid = new Guid ("00000f64-0000-0000-0000-000000000000");		       
			Assertion.Assert ("#C01", !Test1.IsNull);
                	Assertion.Assert ("#C02", SqlGuid.Null.IsNull);
                	Assertion.AssertEquals ("#C03", ResultGuid, Test2.Value);
                }

                // PUBLIC METHODS
		[Test]
                public void CompareTo()
                {
			String TestString = "This is a test string";
                        Assertion.Assert ("#D01", Test1.CompareTo (Test3) < 0);
                        Assertion.Assert ("#D02", Test4.CompareTo (Test1) > 0);
                        Assertion.Assert ("#D03", Test3.CompareTo (Test2) == 0);
                        Assertion.Assert ("#D04", Test4.CompareTo (SqlGuid.Null) > 0);

                        try {
                                Test1.CompareTo (TestString);
                                Assertion.Fail("#D05");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#D06", typeof (ArgumentException), e.GetType ());
                        }
                }

		[Test]
                public void EqualsMethods()
                {
                        Assertion.Assert ("#E01", !Test1.Equals (Test2));
                        Assertion.Assert ("#E02", !Test2.Equals (Test4));
                        Assertion.Assert ("#E03", !Test2.Equals (new SqlString ("TEST")));
                        Assertion.Assert ("#E04", Test2.Equals (Test3));

                        // Static Equals()-method
                        Assertion.Assert ("#E05", SqlGuid.Equals (Test2, Test3).Value);
                        Assertion.Assert ("#E06", !SqlGuid.Equals (Test1, Test2).Value);
                }

		[Test]
                public void GetHashCodeTest()
                {
                        Assertion.AssertEquals ("#F01", Test1.GetHashCode (), Test1.GetHashCode ());
                	Assertion.Assert ("#F02", Test1.GetHashCode () != Test2.GetHashCode ());
                        Assertion.AssertEquals ("#F02", Test3.GetHashCode (), Test2.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
                        Assertion.AssertEquals ("#G01", "System.Data.SqlTypes.SqlGuid", Test1.GetType ().ToString ());
                        Assertion.AssertEquals ("#G02", "System.Guid", Test3.Value.GetType ().ToString ());
                }

		[Test]
                public void Greaters()
                {
                        // GreateThan ()
                        Assertion.Assert ("#H01", !SqlGuid.GreaterThan (Test1, Test2).Value);
                        Assertion.Assert ("#H02", SqlGuid.GreaterThan (Test2, Test1).Value);
                        Assertion.Assert ("#H03", !SqlGuid.GreaterThan (Test2, Test3).Value);
                        // GreaterTharOrEqual ()
                        Assertion.Assert ("#H04", !SqlGuid.GreaterThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#H05", SqlGuid.GreaterThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#H06", SqlGuid.GreaterThanOrEqual (Test2, Test3).Value);
                }

		[Test]
                public void Lessers()
                {
                        // LessThan()
                        Assertion.Assert ("#I01", !SqlGuid.LessThan (Test2, Test3).Value);
                        Assertion.Assert ("#I02", !SqlGuid.LessThan (Test2, Test1).Value);
                        Assertion.Assert ("#I03", SqlGuid.LessThan (Test1, Test2).Value);

                        // LessThanOrEqual ()
                        Assertion.Assert ("#I04", SqlGuid.LessThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#I05", !SqlGuid.LessThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#I06", SqlGuid.LessThanOrEqual (Test2, Test3).Value);
                        Assertion.Assert ("#I07", SqlGuid.LessThanOrEqual (Test4, SqlGuid.Null).IsNull);
                }

		[Test]
                public void NotEquals()
                {
                        Assertion.Assert ("#J01", SqlGuid.NotEquals (Test1, Test2).Value);
                        Assertion.Assert ("#J02", SqlGuid.NotEquals (Test2, Test1).Value);
                        Assertion.Assert ("#J03", SqlGuid.NotEquals (Test3, Test1).Value);
                        Assertion.Assert ("#J04", !SqlGuid.NotEquals (Test3, Test2).Value);                      
                        Assertion.Assert ("#J05", SqlGuid.NotEquals (SqlGuid.Null, Test2).IsNull);
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlGuid.Parse (null);
                                Assertion.Fail ("#K01");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#K02", typeof (ArgumentNullException), e.GetType ());
                        }

                        try {
                                SqlGuid.Parse ("not-a-number");
                                Assertion.Fail ("#K03");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#K04", typeof (FormatException), e.GetType ());
                        }

                         try {
                                SqlGuid.Parse ("9e400");
                                Assertion.Fail ("#K05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#K06", typeof (FormatException), e.GetType ());
                        }

                        Assertion.AssertEquals("#K07", new Guid("87654321-0000-0000-0000-000000000000"), 
                                     SqlGuid.Parse ("87654321-0000-0000-0000-000000000000").Value);
                }

		[Test]
                public void Conversions()
                {
			// ToByteArray ()
			Assertion.AssertEquals ("#L01", (byte)1, Test1.ToByteArray () [0]);
			Assertion.AssertEquals ("#L02", (byte)15, Test2.ToByteArray () [1]);

			// ToSqlBinary ()
			byte [] b = new byte [2]; 
                	b [0] = 100;
                	b [1] = 15;
		       
                        Assertion.AssertEquals ("#L03", new SqlBinary (b), Test3.ToSqlBinary ());

                        // ToSqlString ()
                        Assertion.AssertEquals ("#L04", "00000a01-0000-0000-0000-000000000000",  
				      Test1.ToSqlString ().Value);
                        Assertion.AssertEquals ("#L05", "0000fafa-0000-0000-0000-000000000000", 
                                      Test4.ToSqlString ().Value);

                        // ToString ()
                        Assertion.AssertEquals ("#L06", "00000a01-0000-0000-0000-000000000000", 
                                      Test1.ToString ());
                        Assertion.AssertEquals ("#L07", "0000fafa-0000-0000-0000-000000000000", 
                                      Test4.ToString ());
                }

                // OPERATORS

		[Test]
                public void ThanOrEqualOperators()
                {
                        // == -operator
                        Assertion.Assert ("#M01", (Test3 == Test2).Value);
                        Assertion.Assert ("#M02", !(Test1 == Test2).Value);
                        Assertion.Assert ("#M03", (Test1 == SqlGuid.Null).IsNull);
                        
                        // != -operator
                        Assertion.Assert ("#M04", !(Test2 != Test3).Value);
                        Assertion.Assert ("#M05", (Test1 != Test3).Value);
                        Assertion.Assert ("#M06", (Test1 != SqlGuid.Null).IsNull);

                        // > -operator
                        Assertion.Assert ("#M07", (Test2 > Test1).Value);
                        Assertion.Assert ("#M08", !(Test1 > Test3).Value);
                        Assertion.Assert ("#M09", !(Test3 > Test2).Value);
                        Assertion.Assert ("#M10", (Test1 > SqlGuid.Null).IsNull);

                        // >=  -operator
                        Assertion.Assert ("#M12", !(Test1 >= Test3).Value);
                        Assertion.Assert ("#M13", (Test3 >= Test1).Value);
                        Assertion.Assert ("#M14", (Test3 >= Test2).Value);
                        Assertion.Assert ("#M15", (Test1 >= SqlGuid.Null).IsNull);

                        // < -operator
                        Assertion.Assert ("#M16", !(Test2 < Test1).Value);
                        Assertion.Assert ("#M17", (Test1 < Test3).Value);
                        Assertion.Assert ("#M18", !(Test2 < Test3).Value);
                        Assertion.Assert ("#M19", (Test1 < SqlGuid.Null).IsNull);

                        // <= -operator
                        Assertion.Assert ("#M20", (Test1 <= Test3).Value);
                        Assertion.Assert ("#M21", !(Test3 <= Test1).Value);
                        Assertion.Assert ("#M22", (Test2 <= Test3).Value);
                        Assertion.Assert ("#M23", (Test1 <= SqlGuid.Null).IsNull);
                }

		[Test]
		public void SqlBinaryToSqlGuid()
		{
			byte [] b = new byte [16];
			b [0] = 100;
			b [1] = 200;
			SqlBinary TestBinary = new SqlBinary (b);
			
			Assertion.AssertEquals ("#N01", new Guid("0000c864-0000-0000-0000-000000000000"), 
				      ((SqlGuid)TestBinary).Value);
		}

		[Test]
		public void SqlGuidToGuid()
		{
			Assertion.AssertEquals ("#O01", new Guid("00000a01-0000-0000-0000-000000000000"), 
			              (Guid)Test1);
			Assertion.AssertEquals ("#O02", new Guid("00000f64-0000-0000-0000-000000000000"), 
			              (Guid)Test2);
		}		

		[Test]
                public void SqlStringToSqlGuid()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("0000c864-0000-0000-0000-000000000000");

                        Assertion.AssertEquals ("#P01", new Guid("0000c864-0000-0000-0000-000000000000"), 
                                      ((SqlGuid)TestString100).Value);

                        try {
                                SqlGuid test = (SqlGuid)TestString;
                                Assertion.Fail ("#P02");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#P03", typeof (FormatException), e.GetType ());
                        }
                }
		
		[Test]
		public void GuidToSqlGuid()
		{
			Guid TestGuid = new Guid("0000c864-0000-0000-0000-000007650000");
			Assertion.AssertEquals ("#Q01", new SqlGuid("0000c864-0000-0000-0000-000007650000"), 
				      (SqlGuid)TestGuid);
		}
        }
}
