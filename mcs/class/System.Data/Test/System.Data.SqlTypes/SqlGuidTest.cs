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
                		Assert.Fail ("#A01 " + e);
                	}
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        Assert.IsTrue (SqlGuid.Null.IsNull, "#B01");
                }

                // Test properties
		[Test]
                public void Properties()
                {
                	Guid ResultGuid = new Guid ("00000f64-0000-0000-0000-000000000000");		       
			Assert.IsTrue (!Test1.IsNull, "#C01");
                	Assert.IsTrue (SqlGuid.Null.IsNull, "#C02");
                	Assert.AreEqual (ResultGuid, Test2.Value, "#C03");
                }

                // PUBLIC METHODS
		[Test]
                public void CompareTo()
                {
			String TestString = "This is a test string";
			SqlGuid test1 = new SqlGuid("1AAAAAAA-BBBB-CCCC-DDDD-3EEEEEEEEEEE");
			SqlGuid test2 = new SqlGuid("1AAAAAAA-BBBB-CCCC-DDDD-2EEEEEEEEEEE");
			SqlGuid test3 = new SqlGuid("1AAAAAAA-BBBB-CCCC-DDDD-1EEEEEEEEEEE");
                        Assert.IsTrue (Test1.CompareTo (Test3) <  0, "#D01");
                        Assert.IsTrue (Test4.CompareTo (Test1) > 0, "#D02");
                        Assert.IsTrue (Test3.CompareTo (Test2) == 0, "#D03");
                        Assert.IsTrue (Test4.CompareTo (SqlGuid.Null) > 0, "#D04");
			Assert.IsTrue (test1.CompareTo (test2) >  0, "#D05");
			Assert.IsTrue (test3.CompareTo (test2) <  0, "#D06");
			
                        try {
                                Test1.CompareTo (TestString);
                                Assert.Fail("#D05");
                        } catch(Exception e) {
                                Assert.AreEqual (typeof (ArgumentException), e.GetType (), "#D06");
                        }
                }

		[Test]
                public void EqualsMethods()
                {
                        Assert.IsTrue (!Test1.Equals (Test2), "#E01");
                        Assert.IsTrue (!Test2.Equals (Test4), "#E02");
                        Assert.IsTrue (!Test2.Equals (new SqlString ("TEST")), "#E03");
                        Assert.IsTrue (Test2.Equals (Test3), "#E04");

                        // Static Equals()-method
                        Assert.IsTrue (SqlGuid.Equals (Test2, Test3).Value, "#E05");
                        Assert.IsTrue (!SqlGuid.Equals (Test1, Test2).Value, "#E06");
                }

		[Test]
                public void GetHashCodeTest()
                {
                        Assert.AreEqual (Test1.GetHashCode (), Test1.GetHashCode (), "#F01");
                	Assert.IsTrue (Test1.GetHashCode () != Test2.GetHashCode (), "#F02");
                        Assert.AreEqual (Test3.GetHashCode (), Test2.GetHashCode (), "#F02");
                }

		[Test]
                public void GetTypeTest()
                {
                        Assert.AreEqual ("System.Data.SqlTypes.SqlGuid", Test1.GetType ().ToString (), "#G01");
                        Assert.AreEqual ("System.Guid", Test3.Value.GetType ().ToString (), "#G02");
                }

		[Test]
                public void Greaters()
                {
                        // GreateThan ()
                        Assert.IsTrue (!SqlGuid.GreaterThan (Test1, Test2).Value, "#H01");
                        Assert.IsTrue (SqlGuid.GreaterThan (Test2, Test1).Value, "#H02");
                        Assert.IsTrue (!SqlGuid.GreaterThan (Test2, Test3).Value, "#H03");
                        // GreaterTharOrEqual ()
                        Assert.IsTrue (!SqlGuid.GreaterThanOrEqual (Test1, Test2).Value, "#H04");
                        Assert.IsTrue (SqlGuid.GreaterThanOrEqual (Test2, Test1).Value, "#H05");
                        Assert.IsTrue (SqlGuid.GreaterThanOrEqual (Test2, Test3).Value, "#H06");
                }

		[Test]
                public void Lessers()
                {
                        // LessThan()
                        Assert.IsTrue (!SqlGuid.LessThan (Test2, Test3).Value, "#I01");
                        Assert.IsTrue (!SqlGuid.LessThan (Test2, Test1).Value, "#I02");
                        Assert.IsTrue (SqlGuid.LessThan (Test1, Test2).Value, "#I03");

                        // LessThanOrEqual ()
                        Assert.IsTrue (SqlGuid.LessThanOrEqual (Test1, Test2).Value, "#I04");
                        Assert.IsTrue (!SqlGuid.LessThanOrEqual (Test2, Test1).Value, "#I05");
                        Assert.IsTrue (SqlGuid.LessThanOrEqual (Test2, Test3).Value, "#I06");
                        Assert.IsTrue (SqlGuid.LessThanOrEqual (Test4, SqlGuid.Null).IsNull, "#I07");
                }

		[Test]
                public void NotEquals()
                {
                        Assert.IsTrue (SqlGuid.NotEquals (Test1, Test2).Value, "#J01");
                        Assert.IsTrue (SqlGuid.NotEquals (Test2, Test1).Value, "#J02");
                        Assert.IsTrue (SqlGuid.NotEquals (Test3, Test1).Value, "#J03");
                        Assert.IsTrue (!SqlGuid.NotEquals (Test3, Test2).Value, "#J04");
                        Assert.IsTrue (SqlGuid.NotEquals (SqlGuid.Null, Test2).IsNull, "#J05");
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlGuid.Parse (null);
                                Assert.Fail ("#K01");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "#K02");
                        }

                        try {
                                SqlGuid.Parse ("not-a-number");
                                Assert.Fail ("#K03");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (FormatException), e.GetType (), "#K04");
                        }

                         try {
                                SqlGuid.Parse ("9e400");
                                Assert.Fail ("#K05");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (FormatException), e.GetType (), "#K06");
                        }

                        Assert.AreEqual(new Guid("87654321-0000-0000-0000-000000000000"), SqlGuid.Parse ("87654321-0000-0000-0000-000000000000").Value, "#K07");
                }

		[Test]
                public void Conversions()
                {
			// ToByteArray ()
			Assert.AreEqual ((byte)1, Test1.ToByteArray () [0], "#L01");
			Assert.AreEqual ((byte)15, Test2.ToByteArray () [1], "#L02");

			// ToSqlBinary ()
			byte [] b = new byte [2]; 
                	b [0] = 100;
                	b [1] = 15;
		       
                        Assert.AreEqual (new SqlBinary (b), Test3.ToSqlBinary (), "#L03");

                        // ToSqlString ()
                        Assert.AreEqual ("00000a01-0000-0000-0000-000000000000",  Test1.ToSqlString ().Value, "#L04");
                        Assert.AreEqual ("0000fafa-0000-0000-0000-000000000000", Test4.ToSqlString ().Value, "#L05");

                        // ToString ()
                        Assert.AreEqual ("00000a01-0000-0000-0000-000000000000", Test1.ToString (), "#L06");
                        Assert.AreEqual ("0000fafa-0000-0000-0000-000000000000", Test4.ToString (), "#L07");
                }

                // OPERATORS

		[Test]
                public void ThanOrEqualOperators()
                {
                        // == -operator
                        Assert.IsTrue ((Test3 == Test2).Value, "#M01");
                        Assert.IsTrue (!(Test1 == Test2).Value, "#M02");
                        Assert.IsTrue ((Test1 == SqlGuid.Null).IsNull, "#M03");
                        
                        // != -operator
                        Assert.IsTrue (!(Test2 != Test3).Value, "#M04");
                        Assert.IsTrue ((Test1 != Test3).Value, "#M05");
                        Assert.IsTrue ((Test1 != SqlGuid.Null).IsNull, "#M06");

                        // > -operator
                        Assert.IsTrue ((Test2 > Test1).Value, "#M07");
                        Assert.IsTrue (!(Test1 > Test3).Value, "#M08");
                        Assert.IsTrue (!(Test3 > Test2).Value, "#M09");
                        Assert.IsTrue ((Test1 > SqlGuid.Null).IsNull, "#M10");

                        // >=  -operator
                        Assert.IsTrue (!(Test1 >= Test3).Value, "#M12");
                        Assert.IsTrue ((Test3 >= Test1).Value, "#M13");
                        Assert.IsTrue ((Test3 >= Test2).Value, "#M14");
                        Assert.IsTrue ((Test1 >= SqlGuid.Null).IsNull, "#M15");

                        // < -operator
                        Assert.IsTrue (!(Test2 < Test1).Value, "#M16");
                        Assert.IsTrue ((Test1 < Test3).Value, "#M17");
                        Assert.IsTrue (!(Test2 < Test3).Value, "#M18");
                        Assert.IsTrue ((Test1 < SqlGuid.Null).IsNull, "#M19");

                        // <= -operator
                        Assert.IsTrue ((Test1 <= Test3).Value, "#M20");
                        Assert.IsTrue (!(Test3 <= Test1).Value, "#M21");
                        Assert.IsTrue ((Test2 <= Test3).Value, "#M22");
                        Assert.IsTrue ((Test1 <= SqlGuid.Null).IsNull, "#M23");
                }

		[Test]
		public void SqlBinaryToSqlGuid()
		{
			byte [] b = new byte [16];
			b [0] = 100;
			b [1] = 200;
			SqlBinary TestBinary = new SqlBinary (b);
			
			Assert.AreEqual (new Guid("0000c864-0000-0000-0000-000000000000"), ((SqlGuid)TestBinary).Value, "#N01");
		}

		[Test]
		public void SqlGuidToGuid()
		{
			Assert.AreEqual (new Guid("00000a01-0000-0000-0000-000000000000"), (Guid)Test1, "#O01");
			Assert.AreEqual (new Guid("00000f64-0000-0000-0000-000000000000"), (Guid)Test2, "#O02");
		}		

		[Test]
                public void SqlStringToSqlGuid()
                {
                        SqlString TestString = new SqlString ("Test string");
                        SqlString TestString100 = new SqlString ("0000c864-0000-0000-0000-000000000000");

                        Assert.AreEqual (new Guid("0000c864-0000-0000-0000-000000000000"), ((SqlGuid)TestString100).Value, "#P01");

                        try {
                                SqlGuid test = (SqlGuid)TestString;
                                Assert.Fail ("#P02");
                        } catch(Exception e) {
                                Assert.AreEqual (typeof (FormatException), e.GetType (), "#P03");
                        }
                }
		
		[Test]
		public void GuidToSqlGuid()
		{
			Guid TestGuid = new Guid("0000c864-0000-0000-0000-000007650000");
			Assert.AreEqual (new SqlGuid("0000c864-0000-0000-0000-000007650000"), (SqlGuid)TestGuid, "#Q01");
		}
#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlGuid.GetXsdType (null);
			Assert.AreEqual ("string", qualifiedName.Name, "#A01");
		}
#endif
        }
}
