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
                        Assert.IsTrue (!(Test.IsNull) , "#A01");
		}

		// Test public fields
		[Test]
		public void PublicFields()
		{
			Assert.IsTrue (SqlBinary.Null.IsNull, "#B01");
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
			Assert.IsTrue (SqlBinary.Null.IsNull, "#C01");

			// Item
                        Assert.AreEqual ((byte)128, TestBinary [1], "#C02");
                        Assert.AreEqual ((byte)64, TestBinary [0], "#C03");

                        // FIXME: MSDN says that should throw SqlNullValueException
                        // but throws IndexOutOfRangeException
                        try {
                                byte test = TestBinary [TestBinary.Length];
				Assert.Fail ("#C04");
			} catch (Exception e) {
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType (), "#C05");
			}
                 
			try {
				byte test = SqlBinary.Null [2];
				Assert.Fail ("#C06");
			} catch (Exception e) {
				Assert.AreEqual (typeof (SqlNullValueException), e.GetType (), "#C07");
			}

			// Length
                        Assert.AreEqual (2, TestBinary.Length, "#C08");

			try {
				int test = SqlBinary.Null.Length;
				Assert.Fail ("#C09");
			} catch (Exception e) {
				Assert.AreEqual (typeof (SqlNullValueException), e.GetType (), "#C10");
			}

			// Value
                        Assert.AreEqual ((byte)128, TestBinary [1], "#C11");
                        Assert.AreEqual ((byte)64, TestBinary [0], "#C12");

			try {
                                Byte [] test = SqlBinary.Null.Value;
				Assert.Fail ("#C13");
			} catch (Exception e) {
				Assert.AreEqual (typeof (SqlNullValueException), e.GetType (), "#C14");
			}
		}

		// Methods 
		[Test]
		public void ComparisonMethods()
		{
			// GreaterThan
			Assert.IsTrue (SqlBinary.GreaterThan (Test1, Test2).Value, "#D01");
                        Assert.IsTrue (SqlBinary.GreaterThan (Test3, Test2).Value, "#D02");
			Assert.IsTrue (!SqlBinary.GreaterThan (Test2, Test1).Value, "#D03");
			
			// GreaterThanOrEqual
			Assert.IsTrue (SqlBinary.GreaterThanOrEqual (Test1, Test2).Value, "#D04");
                        Assert.IsTrue (SqlBinary.GreaterThanOrEqual (Test1, Test2).Value, "#D05");
			Assert.IsTrue (!SqlBinary.GreaterThanOrEqual (Test2, Test1).Value, "#D06");

			// LessThan
			Assert.IsTrue (!SqlBinary.LessThan (Test1, Test2).Value, "#D07");
			Assert.IsTrue (!SqlBinary.LessThan (Test3, Test2).Value, "#D08");
			Assert.IsTrue (SqlBinary.LessThan (Test2, Test1).Value, "#D09");

			// LessThanOrEqual
			Assert.IsTrue (!SqlBinary.LessThanOrEqual (Test1, Test2).Value, "#D10");
                        Assert.IsTrue (SqlBinary.LessThanOrEqual (Test3, Test1).Value, "#D11");
                        Assert.IsTrue (SqlBinary.LessThanOrEqual (Test2, Test1).Value, "#D12");

			// Equals
                        Assert.IsTrue (!Test1.Equals (Test2), "#D13");
                        Assert.IsTrue (!Test3.Equals (Test2), "#D14");
                        Assert.IsTrue (Test3.Equals (Test1), "#D15");

			// NotEquals
			Assert.IsTrue (SqlBinary.NotEquals (Test1, Test2).Value, "#D16");
                        Assert.IsTrue (!SqlBinary.NotEquals (Test3, Test1).Value, "#D17");
			Assert.IsTrue (SqlBinary.NotEquals (Test2, Test1).Value, "#D18");
		}

		[Test]
		public void CompareTo()
		{
                        SqlString TestString = new SqlString ("This is a test");
			
                        Assert.IsTrue (Test1.CompareTo(Test2) > 0, "#E01");
                        Assert.IsTrue (Test2.CompareTo(Test1) < 0, "#E02");
                        Assert.IsTrue (Test1.CompareTo(Test3) == 0, "#E03");
			
			try {
                                Test1.CompareTo (TestString);
                                Assert.Fail ("#E04");
			} catch(Exception e) {
                                Assert.AreEqual (typeof (ArgumentException), e.GetType (), "#E05");
			}
		}

		[Test]
		public void GetHashCodeTest()
		{
			Assert.AreEqual (Test1.GetHashCode (), Test1.GetHashCode (), "#F01");
			Assert.IsTrue (Test2.GetHashCode () !=  Test1.GetHashCode (), "#F02");
		}

		[Test]
		public void GetTypeTest()
		{
			Assert.AreEqual ("System.Data.SqlTypes.SqlBinary", Test1.GetType().ToString(), "#G01");
		}

		[Test]
		public void Concat()
		{			
			SqlBinary TestBinary;

			TestBinary = SqlBinary.Concat (Test2, Test3);
                        Assert.AreEqual ((byte)15, TestBinary [4], "H01");

			TestBinary = SqlBinary.Concat (Test1, Test2);
                        Assert.AreEqual ((byte)240, TestBinary [0], "#H02");
                        Assert.AreEqual ((byte)15, TestBinary [1], "#H03");
		}

		[Test]
		public void ToSqlGuid()
		{
                        SqlBinary TestBinary = new SqlBinary (new byte [16]);
                        SqlGuid TestGuid = TestBinary.ToSqlGuid ();
                        Assert.IsTrue (!TestGuid.IsNull, "#I01");
		}

		[Test]
		public void ToStringTest()
		{
                        Assert.AreEqual ("SqlBinary(3)", Test2.ToString (), "#J01");
                        Assert.AreEqual ("SqlBinary(2)", Test1.ToString (), "#J02");
		}

		// OPERATORS
		[Test]
		public void AdditionOperator()
		{
			SqlBinary TestBinary = Test1 + Test2;
                        Assert.AreEqual ((byte)240, TestBinary [0], "#K01");
                        Assert.AreEqual ((byte)15, TestBinary [1], "#K02");
		}

		[Test]
		public void ComparisonOperators()
		{
			// Equality
                        Assert.IsTrue (!(Test1 == Test2).Value, "#L01");
                        Assert.IsTrue ((Test3 == Test1).Value, "#L02");

			// Greater than
                        Assert.IsTrue ((Test1 > Test2).Value, "#L03");
                        Assert.IsTrue (!(Test3 > Test1).Value, "#L04");

			// Greater than or equal
                        Assert.IsTrue ((Test1 >= Test2).Value, "#L05");
                        Assert.IsTrue ((Test3 >= Test2).Value, "#L06");

			// Inequality
                        Assert.IsTrue ((Test1 != Test2).Value, "#L07");
                        Assert.IsTrue (!(Test3 != Test1).Value, "#L08");

			// Less than
                        Assert.IsTrue (!(Test1 < Test2).Value, "#L09");
                        Assert.IsTrue (!(Test3 < Test2).Value, "#L10");

			// Less than or equal
                        Assert.IsTrue (!(Test1 <= Test2).Value, "#L11");
                        Assert.IsTrue ((Test3 <= Test1).Value, "#L12");
		}

		[Test]
		public void SqlBinaryToByteArray() 
		{
			byte [] TestByteArray = (Byte[])Test1;
			Assert.AreEqual ((byte)240, TestByteArray[0], "#M01");
		}

		[Test]
		public void SqlGuidToSqlBinary()
		{
                        byte [] TestByteArray = new Byte [16];
			TestByteArray [0] = 15;
			TestByteArray [1] = 200;
			SqlGuid TestGuid = new SqlGuid (TestByteArray);
			
			SqlBinary TestBinary = (SqlBinary)TestGuid;
                        Assert.AreEqual ((byte)15, TestBinary [0], "#N01");
		}

		[Test]
		public void ByteArrayToSqlBinary()
		{
                        byte [] TestByteArray = new Byte [2];
			TestByteArray [0] = 15;
			TestByteArray [1] = 200;
			SqlBinary TestBinary = (SqlBinary)TestByteArray;
                        Assert.AreEqual ((byte)15, TestBinary [0], "#O1");
		}
#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlBinary.GetXsdType (null);
			Assert.AreEqual ("base64Binary", qualifiedName.Name, "#A01");
		}
#endif
	}
}

