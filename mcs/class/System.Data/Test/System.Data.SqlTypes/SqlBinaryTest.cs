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
        public class SqlBinaryTest : Assertion {
	
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
                        Assert ("#A01", !(Test.IsNull)); 
		}

		// Test public fields
		[Test]
		public void PublicFields()
		{
			Assert ("#B01", SqlBinary.Null.IsNull);
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
			Assert ("#C01", SqlBinary.Null.IsNull);

			// Item
                        AssertEquals ("#C02", (byte)128, TestBinary [1]);
                        AssertEquals ("#C03", (byte)64, TestBinary [0]);

                        // FIXME: MSDN says that should throw SqlNullValueException
                        // but throws IndexOutOfRangeException
                        try {
                                byte test = TestBinary [TestBinary.Length];
				Fail ("#C04");
			} catch (Exception e) {
				AssertEquals ("#C05", typeof (SqlNullValueException),
					    e.GetType ());
			}
                 
			try {
				byte test = SqlBinary.Null [2];
				Fail ("#C06");
			} catch (Exception e) {
				AssertEquals ("#C07", typeof (SqlNullValueException),
					    e.GetType ());
			}

			// Length
                        AssertEquals ("#C08", 2, TestBinary.Length);    

			try {
				int test = SqlBinary.Null.Length;
				Fail ("#C09");
			} catch (Exception e) {
				AssertEquals ("#C10", typeof (SqlNullValueException),
					    e.GetType ());
			}

			// Value
                        AssertEquals ("#C11", (byte)128, TestBinary [1]);
                        AssertEquals ("#C12", (byte)64, TestBinary [0]);               

			try {
                                Byte [] test = SqlBinary.Null.Value;
				Fail ("#C13");
			} catch (Exception e) {
				AssertEquals ("#C14", typeof (SqlNullValueException),
					    e.GetType ());
			}
		}

		// Methods 
		[Test]
		public void ComparisonMethods()
		{
			// GreaterThan
			Assert ("#D01", SqlBinary.GreaterThan (Test1, Test2).Value);
                        Assert ("#D02", SqlBinary.GreaterThan (Test3, Test2).Value);
			Assert ("#D03", !SqlBinary.GreaterThan (Test2, Test1).Value);
			
			// GreaterThanOrEqual
			Assert ("#D04", SqlBinary.GreaterThanOrEqual (Test1, Test2).Value);
                        Assert ("#D05", SqlBinary.GreaterThanOrEqual (Test1, Test2).Value);
			Assert ("#D06", !SqlBinary.GreaterThanOrEqual (Test2, Test1).Value);

			// LessThan
			Assert ("#D07", !SqlBinary.LessThan (Test1, Test2).Value);
			Assert ("#D08", !SqlBinary.LessThan (Test3, Test2).Value);
			Assert ("#D09", SqlBinary.LessThan (Test2, Test1).Value);

			// LessThanOrEqual
			Assert ("#D10", !SqlBinary.LessThanOrEqual (Test1, Test2).Value);
                        Assert ("#D11", SqlBinary.LessThanOrEqual (Test3, Test1).Value);
                        Assert ("#D12", SqlBinary.LessThanOrEqual (Test2, Test1).Value);

			// Equals
                        Assert ("#D13", !Test1.Equals (Test2));
                        Assert ("#D14", !Test3.Equals (Test2));
                        Assert ("#D15", Test3.Equals (Test1));

			// NotEquals
			Assert ("#D16", SqlBinary.NotEquals (Test1, Test2).Value);
                        Assert ("#D17", !SqlBinary.NotEquals (Test3, Test1).Value);
			Assert ("#D18", SqlBinary.NotEquals (Test2, Test1).Value);
		}

		[Test]
		public void CompareTo()
		{
                        SqlString TestString = new SqlString ("This is a test");
			
                        Assert ("#E01", Test1.CompareTo(Test2) > 0);
                        Assert ("#E02", Test2.CompareTo(Test1) < 0);
                        Assert ("#E03", Test1.CompareTo(Test3) == 0);
			
			try {
                                Test1.CompareTo (TestString);
                                Fail ("#E04");
			} catch(Exception e) {
                                AssertEquals ("#E05", typeof (ArgumentException), e.GetType ());
			}			
		}

		[Test]
		public void GetHashCodeTest()
		{
			AssertEquals ("#F01", Test1.GetHashCode (), Test1.GetHashCode ());
			Assert ("#F02", Test2.GetHashCode () !=  Test1.GetHashCode ());
		}

		[Test]
		public void GetTypeTest()
		{
			AssertEquals("#G01", "System.Data.SqlTypes.SqlBinary", 
				     Test1.GetType().ToString());			
		}

		[Test]
		public void Concat()
		{			
			SqlBinary TestBinary;

			TestBinary = SqlBinary.Concat (Test2, Test3);
                        AssertEquals ("H01", (byte)15, TestBinary [4]);

			TestBinary = SqlBinary.Concat (Test1, Test2);
                        AssertEquals ("#H02", (byte)240, TestBinary [0]);
                        AssertEquals ("#H03", (byte)15, TestBinary [1]);
		}

		[Test]
		public void ToSqlGuid()
		{
                        SqlBinary TestBinary = new SqlBinary (new byte [16]);
                        SqlGuid TestGuid = TestBinary.ToSqlGuid ();
                        Assert ("#I01", !TestGuid.IsNull);
		}

		[Test]
		public void ToStringTest()
		{
                        AssertEquals ("#J01", "SqlBinary(3)", Test2.ToString ());
                        AssertEquals ("#J02", "SqlBinary(2)", Test1.ToString ());              
		}

		// OPERATORS
		[Test]
		public void AdditionOperator()
		{
			SqlBinary TestBinary = Test1 + Test2;
                        AssertEquals ("#K01", (byte)240, TestBinary [0]);
                        AssertEquals ("#K02", (byte)15, TestBinary [1]);
		}

		[Test]
		public void ComparisonOperators()
		{
			// Equality
                        Assert ("#L01", !(Test1 == Test2).Value);
                        Assert ("#L02", (Test3 == Test1).Value);

			// Greater than
                        Assert ("#L03", (Test1 > Test2).Value);
                        Assert ("#L04", !(Test3 > Test1).Value);

			// Greater than or equal
                        Assert ("#L05", (Test1 >= Test2).Value);
                        Assert ("#L06", (Test3 >= Test2).Value);

			// Inequality
                        Assert ("#L07", (Test1 != Test2).Value);
                        Assert ("#L08", !(Test3 != Test1).Value);

			// Less than
                        Assert ("#L09", !(Test1 < Test2).Value);
                        Assert ("#L10", !(Test3 < Test2).Value);

			// Less than or equal
                        Assert ("#L11", !(Test1 <= Test2).Value);
                        Assert ("#L12", (Test3 <= Test1).Value);
		}

		[Test]
		public void SqlBinaryToByteArray() 
		{
			byte [] TestByteArray = (Byte[])Test1;
			AssertEquals ("#M01", (byte)240, TestByteArray[0]);			
		}

		[Test]
		public void SqlGuidToSqlBinary()
		{
                        byte [] TestByteArray = new Byte [16];
			TestByteArray [0] = 15;
			TestByteArray [1] = 200;
			SqlGuid TestGuid = new SqlGuid (TestByteArray);
			
			SqlBinary TestBinary = (SqlBinary)TestGuid;
                        AssertEquals ("#N01", (byte)15, TestBinary [0]);
		}

		[Test]
		public void ByteArrayToSqlBinary()
		{
                        byte [] TestByteArray = new Byte [2];
			TestByteArray [0] = 15;
			TestByteArray [1] = 200;
			SqlBinary TestBinary = (SqlBinary)TestByteArray;
                        AssertEquals ("#O1", (byte)15, TestBinary [0]);
		}
	}
}

