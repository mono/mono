//
// SqlBinaryTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlBinary
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
        public class SqlBinaryTest {
	
		SqlBinary Test1;
		SqlBinary Test2;
		SqlBinary Test3;

		[SetUp]
		public void GetReady() 
		{
                        byte [] b1 = new Byte [2];
                        byte [] b2 = new Byte [3];
                        byte [] b3 = new Byte [2];

			b1 [0] = 240;
			b1 [1] = 15;
			b2 [0] = 10;
			b2 [1] = 10;
			b2 [2] = 10;
			b3 [0] = 240;
			b3 [1] = 15;

			Test1 = new SqlBinary(b1);
			Test2 = new SqlBinary(b2);
			Test3 = new SqlBinary(b3);
		}

		// Test constructor
		[Test]
		public void Create()
		{
			byte [] b = new byte [3];                        
			SqlBinary Test = new SqlBinary (b);
                        Assertion.Assert ("#A01", !(Test.IsNull)); 
		}

		// Test public fields
		[Test]
		public void PublicFields()
		{
			Assertion.Assert ("#B01", SqlBinary.Null.IsNull);
		}

		// Test properties
		[Test]
		public void Properties()
		{
			byte [] b = new byte [2];
			b [0] = 64;
			b [1] = 128;

			SqlBinary TestBinary = new SqlBinary (b);

			// IsNull
			Assertion.Assert ("#C01", SqlBinary.Null.IsNull);

			// Item
                        Assertion.AssertEquals ("#C02", (byte)128, TestBinary [1]);
                        Assertion.AssertEquals ("#C03", (byte)64, TestBinary [0]);

                        // FIXME: MSDN says that should throw SqlNullValueException
                        // but throws IndexOutOfRangeException
                        try {
                                byte test = TestBinary [TestBinary.Length];
				Assertion.Fail ("#C04");
			} catch (Exception e) {
				Assertion.AssertEquals ("#C05", typeof (SqlNullValueException),
					    e.GetType ());
			}
                 
			try {
				byte test = SqlBinary.Null [2];
				Assertion.Fail ("#C06");
			} catch (Exception e) {
				Assertion.AssertEquals ("#C07", typeof (SqlNullValueException),
					    e.GetType ());
			}

			// Length
                        Assertion.AssertEquals ("#C08", 2, TestBinary.Length);    

			try {
				int test = SqlBinary.Null.Length;
				Assertion.Fail ("#C09");
			} catch (Exception e) {
				Assertion.AssertEquals ("#C10", typeof (SqlNullValueException),
					    e.GetType ());
			}

			// Value
                        Assertion.AssertEquals ("#C11", (byte)128, TestBinary [1]);
                        Assertion.AssertEquals ("#C12", (byte)64, TestBinary [0]);               

			try {
                                Byte [] test = SqlBinary.Null.Value;
				Assertion.Fail ("#C13");
			} catch (Exception e) {
				Assertion.AssertEquals ("#C14", typeof (SqlNullValueException),
					    e.GetType ());
			}
		}

		// Methods 
		[Test]
		public void ComparisonMethods()
		{
			// GreaterThan
			Assertion.Assert ("#D01", SqlBinary.GreaterThan (Test1, Test2).Value);
                        Assertion.Assert ("#D02", SqlBinary.GreaterThan (Test3, Test2).Value);
			Assertion.Assert ("#D03", !SqlBinary.GreaterThan (Test2, Test1).Value);
			
			// GreaterThanOrEqual
			Assertion.Assert ("#D04", SqlBinary.GreaterThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#D05", SqlBinary.GreaterThanOrEqual (Test1, Test2).Value);
			Assertion.Assert ("#D06", !SqlBinary.GreaterThanOrEqual (Test2, Test1).Value);

			// LessThan
			Assertion.Assert ("#D07", !SqlBinary.LessThan (Test1, Test2).Value);
			Assertion.Assert ("#D08", !SqlBinary.LessThan (Test3, Test2).Value);
			Assertion.Assert ("#D09", SqlBinary.LessThan (Test2, Test1).Value);

			// LessThanOrEqual
			Assertion.Assert ("#D10", !SqlBinary.LessThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#D11", SqlBinary.LessThanOrEqual (Test3, Test1).Value);
                        Assertion.Assert ("#D12", SqlBinary.LessThanOrEqual (Test2, Test1).Value);

			// Equals
                        Assertion.Assert ("#D13", !Test1.Equals (Test2));
                        Assertion.Assert ("#D14", !Test3.Equals (Test2));
                        Assertion.Assert ("#D15", Test3.Equals (Test1));

			// NotEquals
			Assertion.Assert ("#D16", SqlBinary.NotEquals (Test1, Test2).Value);
                        Assertion.Assert ("#D17", !SqlBinary.NotEquals (Test3, Test1).Value);
			Assertion.Assert ("#D18", SqlBinary.NotEquals (Test2, Test1).Value);
		}

		[Test]
		public void CompareTo()
		{
                        SqlString TestString = new SqlString ("This is a test");
			
                        Assertion.Assert ("#E01", Test1.CompareTo(Test2) > 0);
                        Assertion.Assert ("#E02", Test2.CompareTo(Test1) < 0);
                        Assertion.Assert ("#E03", Test1.CompareTo(Test3) == 0);
			
			try {
                                Test1.CompareTo (TestString);
                                Assertion.Fail ("#E04");
			} catch(Exception e) {
                                Assertion.AssertEquals ("#E05", typeof (ArgumentException), e.GetType ());
			}			
		}

		[Test]
		public void GetHashCodeTest()
		{
			Assertion.AssertEquals ("#F01", Test1.GetHashCode (), Test1.GetHashCode ());
			Assertion.Assert ("#F02", Test2.GetHashCode () !=  Test1.GetHashCode ());
		}

		[Test]
		public void GetTypeTest()
		{
			Assertion.AssertEquals("#G01", "System.Data.SqlTypes.SqlBinary", 
				     Test1.GetType().ToString());			
		}

		[Test]
		public void Concat()
		{			
			SqlBinary TestBinary;

			TestBinary = SqlBinary.Concat (Test2, Test3);
                        Assertion.AssertEquals ("H01", (byte)15, TestBinary [4]);

			TestBinary = SqlBinary.Concat (Test1, Test2);
                        Assertion.AssertEquals ("#H02", (byte)240, TestBinary [0]);
                        Assertion.AssertEquals ("#H03", (byte)15, TestBinary [1]);
		}

		[Test]
		public void ToSqlGuid()
		{
                        SqlBinary TestBinary = new SqlBinary (new byte [16]);
                        SqlGuid TestGuid = TestBinary.ToSqlGuid ();
                        Assertion.Assert ("#I01", !TestGuid.IsNull);
		}

		[Test]
		public void ToStringTest()
		{
                        Assertion.AssertEquals ("#J01", "SqlBinary(3)", Test2.ToString ());
                        Assertion.AssertEquals ("#J02", "SqlBinary(2)", Test1.ToString ());              
		}

		// OPERATORS
		[Test]
		public void AdditionOperator()
		{
			SqlBinary TestBinary = Test1 + Test2;
                        Assertion.AssertEquals ("#K01", (byte)240, TestBinary [0]);
                        Assertion.AssertEquals ("#K02", (byte)15, TestBinary [1]);
		}

		[Test]
		public void ComparisonOperators()
		{
			// Equality
                        Assertion.Assert ("#L01", !(Test1 == Test2).Value);
                        Assertion.Assert ("#L02", (Test3 == Test1).Value);

			// Greater than
                        Assertion.Assert ("#L03", (Test1 > Test2).Value);
                        Assertion.Assert ("#L04", !(Test3 > Test1).Value);

			// Greater than or equal
                        Assertion.Assert ("#L05", (Test1 >= Test2).Value);
                        Assertion.Assert ("#L06", (Test3 >= Test2).Value);

			// Inequality
                        Assertion.Assert ("#L07", (Test1 != Test2).Value);
                        Assertion.Assert ("#L08", !(Test3 != Test1).Value);

			// Less than
                        Assertion.Assert ("#L09", !(Test1 < Test2).Value);
                        Assertion.Assert ("#L10", !(Test3 < Test2).Value);

			// Less than or equal
                        Assertion.Assert ("#L11", !(Test1 <= Test2).Value);
                        Assertion.Assert ("#L12", (Test3 <= Test1).Value);
		}

		[Test]
		public void SqlBinaryToByteArray() 
		{
			byte [] TestByteArray = (Byte[])Test1;
			Assertion.AssertEquals ("#M01", (byte)240, TestByteArray[0]);			
		}

		[Test]
		public void SqlGuidToSqlBinary()
		{
                        byte [] TestByteArray = new Byte [16];
			TestByteArray [0] = 15;
			TestByteArray [1] = 200;
			SqlGuid TestGuid = new SqlGuid (TestByteArray);
			
			SqlBinary TestBinary = (SqlBinary)TestGuid;
                        Assertion.AssertEquals ("#N01", (byte)15, TestBinary [0]);
		}

		[Test]
		public void ByteArrayToSqlBinary()
		{
                        byte [] TestByteArray = new Byte [2];
			TestByteArray [0] = 15;
			TestByteArray [1] = 200;
			SqlBinary TestBinary = (SqlBinary)TestByteArray;
                        Assertion.AssertEquals ("#O1", (byte)15, TestBinary [0]);
		}
	}
}

