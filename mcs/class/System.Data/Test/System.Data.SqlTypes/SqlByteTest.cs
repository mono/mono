//
// SqlByteTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlByte
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
using System.Threading;
using System.Globalization;
#if TARGET_JVM
using DivideByZeroException = System.ArithmeticException;
#endif

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlByteTest
	{
		private const string Error = " does not work correctly";

		[SetUp]
		public void SetUp ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		}

		// Test constructor
		[Test]
		public void Create()
		{
			byte b = 29;
			SqlByte TestByte = new SqlByte(b);
			Assert.AreEqual((byte)29, TestByte.Value, "Constructor 1 does not work correctly");
		}

		// Test public fields
		[Test]
		public void PublicFields()
		{
			Assert.AreEqual((SqlByte)255, SqlByte.MaxValue, "MaxValue field" + Error);
			Assert.AreEqual((SqlByte)0, SqlByte.MinValue, "MinValue field" + Error);
			Assert.IsTrue (SqlByte.Null.IsNull, "Null field" + Error);
			Assert.AreEqual((byte)0, SqlByte.Zero.Value, "Zero field" + Error);
		}

		// Test properties
		[Test]
		public void Properties()
		{

			SqlByte TestByte = new SqlByte(54);
			SqlByte TestByte2 = new SqlByte(1);

			Assert.IsTrue (SqlByte.Null.IsNull, "IsNull property" + Error);
			Assert.AreEqual((byte)54, TestByte.Value, "Value property 1" + Error);
			Assert.AreEqual((byte)1, TestByte2.Value, "Value property 2" + Error);

		}

		// PUBLIC STATIC METHODS
		[Test]
		public void AddMethod()
		{

			SqlByte TestByte64 = new SqlByte(64);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte164 = new SqlByte(164);
			SqlByte TestByte255 = new SqlByte(255);

			Assert.AreEqual((byte)64, SqlByte.Add(TestByte64, TestByte0).Value, "AddMethod 1" + Error);
			Assert.AreEqual((byte)228, SqlByte.Add(TestByte64, TestByte164).Value, "AddMethod 2" + Error);
			Assert.AreEqual((byte)164, SqlByte.Add(TestByte0, TestByte164).Value, "AddMethod 3" + Error);
			Assert.AreEqual((byte)255, SqlByte.Add(TestByte255, TestByte0).Value, "AddMethod 4" + Error);

			try {
				SqlByte.Add(TestByte255, TestByte64);
				Assert.Fail ("AddMethod 6" + Error);
			} catch (Exception e) {
				Assert.AreEqual(typeof(OverflowException), e.GetType(), "AddMethod 5" + Error);
			}

		}

		[Test]
		public void BitwiseAndMethod()
		{

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte1 = new SqlByte(1);
			SqlByte TestByte62 = new SqlByte(62);
			SqlByte TestByte255 = new SqlByte(255);

			Assert.AreEqual((byte)0, SqlByte.BitwiseAnd(TestByte2, TestByte1).Value, "BitwiseAnd method 1" + Error);
			Assert.AreEqual((byte)0, SqlByte.BitwiseAnd(TestByte1, TestByte62).Value, "BitwiseAnd method 2" + Error);
			Assert.AreEqual((byte)2, SqlByte.BitwiseAnd(TestByte62, TestByte2).Value, "BitwiseAnd method 3" + Error);
			Assert.AreEqual((byte)1, SqlByte.BitwiseAnd(TestByte1, TestByte255).Value, "BitwiseAnd method 4" + Error);
			Assert.AreEqual((byte)62, SqlByte.BitwiseAnd(TestByte62, TestByte255).Value, "BitwiseAnd method 5" + Error);

		}

		[Test]
		public void BitwiseOrMethod()
		{

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte1 = new SqlByte(1);
			SqlByte TestByte62 = new SqlByte(62);
			SqlByte TestByte255 = new SqlByte(255);

			Assert.AreEqual((byte)3, SqlByte.BitwiseOr(TestByte2, TestByte1).Value, "BitwiseOr method 1" + Error);
			Assert.AreEqual((byte)63, SqlByte.BitwiseOr(TestByte1, TestByte62).Value, "BitwiseOr method 2" + Error);
			Assert.AreEqual((byte)62, SqlByte.BitwiseOr(TestByte62, TestByte2).Value, "BitwiseOr method 3" + Error);
			Assert.AreEqual((byte)255, SqlByte.BitwiseOr(TestByte1, TestByte255).Value, "BitwiseOr method 4" + Error);
			Assert.AreEqual((byte)255, SqlByte.BitwiseOr(TestByte62, TestByte255).Value, "BitwiseOr method 5" + Error);

		}

		[Test]
		public void CompareTo()
		{

			SqlByte TestByte13 = new SqlByte(13);
			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte10II = new SqlByte(10);

			SqlString TestString = new SqlString("This is a test");
			
			Assert.IsTrue (TestByte13.CompareTo(TestByte10) > 0, "CompareTo method 1" + Error);
			Assert.IsTrue (TestByte10.CompareTo(TestByte13) < 0, "CompareTo method 2" + Error);
			Assert.IsTrue (TestByte10.CompareTo(TestByte10II) == 0, "CompareTo method 3" + Error);
			
			try {
				TestByte13.CompareTo(TestString);
				Assert.Fail("CompareTo method 4" + Error);
			} catch(Exception e) {
				Assert.AreEqual(typeof(ArgumentException), e.GetType(), "Parse method 5" + Error);
			}
			
		}

		[Test]
		public void DivideMethod()
		{

			SqlByte TestByte13 = new SqlByte(13);
			SqlByte TestByte0 = new SqlByte(0);

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte180 = new SqlByte(180);
			SqlByte TestByte3 = new SqlByte(3);

			Assert.AreEqual((byte)6, SqlByte.Divide(TestByte13, TestByte2).Value, "Divide method 1" + Error);
			Assert.AreEqual((byte)90, SqlByte.Divide(TestByte180, TestByte2).Value, "Divide method 2" + Error);
			Assert.AreEqual((byte)60, SqlByte.Divide(TestByte180, TestByte3).Value, "Divide method 3" + Error);
			Assert.AreEqual((byte)0, SqlByte.Divide(TestByte13, TestByte180).Value, "Divide method 4" + Error);
			Assert.AreEqual((byte)0, SqlByte.Divide(TestByte13, TestByte180).Value, "Divide method 5" + Error);

			try {
				SqlByte.Divide(TestByte13, TestByte0);
				Assert.Fail ("Divide method 6" + Error);
			} catch(Exception e) {
				Assert.AreEqual(typeof(DivideByZeroException), e.GetType(), "DivideByZeroException");

			}

		}

		[Test]
		public void EqualsMethod()
		{

			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte158 = new SqlByte(158);
			SqlByte TestByte180 = new SqlByte(180);
			SqlByte TestByte180II = new SqlByte(180);

			Assert.IsTrue (!TestByte0.Equals(TestByte158), "Equals method 1" + Error);
			Assert.IsTrue (!TestByte158.Equals(TestByte180), "Equals method 2" + Error);
			Assert.IsTrue (!TestByte180.Equals(new SqlString("TEST")), "Equals method 3" + Error);
			Assert.IsTrue (TestByte180.Equals(TestByte180II), "Equals method 4" + Error);

		}

		[Test]
		public void StaticEqualsMethod()
		{

			SqlByte TestByte34 = new SqlByte(34);
			SqlByte TestByte34II = new SqlByte(34);
			SqlByte TestByte15 = new SqlByte(15);
			
			Assert.IsTrue (SqlByte.Equals(TestByte34, TestByte34II).Value, "static Equals method 1" + Error);
			Assert.IsTrue (!SqlByte.Equals(TestByte34, TestByte15).Value, "static Equals method 2" + Error);
			Assert.IsTrue (!SqlByte.Equals(TestByte15, TestByte34II).Value, "static Equals method 3" + Error);

		}

		[Test]
		public void GetHashCodeTest()
		{

			SqlByte TestByte15 = new SqlByte(15);
			SqlByte TestByte216 = new SqlByte(216);
			
			Assert.AreEqual(15, TestByte15.GetHashCode(), "GetHashCode method 1" + Error);
			Assert.AreEqual(216, TestByte216.GetHashCode(), "GetHashCode method 2" + Error);

		}

		[Test]
		public void GetTypeTest()
		{

			SqlByte TestByte = new SqlByte(84);

			Assert.AreEqual("System.Data.SqlTypes.SqlByte", TestByte.GetType().ToString(), "GetType method" + Error);
			
		}

		[Test]
		public void GreaterThan()
		{

			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte10II = new SqlByte(10);
			SqlByte TestByte110 = new SqlByte(110);

			Assert.IsTrue (!SqlByte.GreaterThan(TestByte10, TestByte110).Value, "GreaterThan method 1" + Error);
			Assert.IsTrue (SqlByte.GreaterThan(TestByte110, TestByte10).Value, "GreaterThan method 2" + Error);
			Assert.IsTrue (!SqlByte.GreaterThan(TestByte10II, TestByte10).Value, "GreaterThan method 3" + Error);

		}

		[Test]
		public void GreaterThanOrEqual()
		{

			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte10II = new SqlByte(10);
			SqlByte TestByte110 = new SqlByte(110);

			Assert.IsTrue (!SqlByte.GreaterThanOrEqual(TestByte10, TestByte110).Value, "GreaterThanOrEqual method 1" + Error);

			Assert.IsTrue (SqlByte.GreaterThanOrEqual(TestByte110, TestByte10).Value, "GreaterThanOrEqual method 2" + Error);

			Assert.IsTrue (SqlByte.GreaterThanOrEqual(TestByte10II, TestByte10).Value, "GreaterThanOrEqual method 3" + Error);

		}

		[Test]
		public void LessThan()
		{

			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte10II = new SqlByte(10);
			SqlByte TestByte110 = new SqlByte(110);

			Assert.IsTrue (SqlByte.LessThan(TestByte10, TestByte110).Value, "LessThan method 1" + Error);

			Assert.IsTrue (!SqlByte.LessThan(TestByte110, TestByte10).Value, "LessThan method 2" + Error);

			Assert.IsTrue (!SqlByte.LessThan(TestByte10II, TestByte10).Value, "LessThan method 3" + Error);

		}

		[Test]
		public void LessThanOrEqual()
		{

			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte10II = new SqlByte(10);
			SqlByte TestByte110 = new SqlByte(110);

			Assert.IsTrue (SqlByte.LessThanOrEqual(TestByte10, TestByte110).Value, "LessThanOrEqual method 1" + Error);

			Assert.IsTrue (!SqlByte.LessThanOrEqual(TestByte110, TestByte10).Value, "LessThanOrEqual method 2" + Error);

			Assert.IsTrue (SqlByte.LessThanOrEqual(TestByte10II, TestByte10).Value, "LessThanOrEqual method 3" + Error);

			Assert.IsTrue (SqlByte.LessThanOrEqual(TestByte10II, SqlByte.Null).IsNull, "LessThanOrEqual method 4" + Error);
		}

		[Test]
		public void Mod()
		{

			SqlByte TestByte132 = new SqlByte(132);
			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte200 = new SqlByte(200);

			Assert.AreEqual((SqlByte)2, SqlByte.Mod(TestByte132, TestByte10), "Mod method 1" + Error);
			Assert.AreEqual((SqlByte)10, SqlByte.Mod(TestByte10, TestByte200), "Mod method 2" + Error);
			Assert.AreEqual((SqlByte)0, SqlByte.Mod(TestByte200, TestByte10), "Mod method 3" + Error);
			Assert.AreEqual((SqlByte)68, SqlByte.Mod(TestByte200, TestByte132), "Mod method 4" + Error);

		}

		[Test]
		public void Multiply()
		{

			SqlByte TestByte12 = new SqlByte (12);
			SqlByte TestByte2 = new SqlByte (2);
			SqlByte TestByte128 = new SqlByte (128);

			Assert.AreEqual ((byte)24, SqlByte.Multiply(TestByte12, TestByte2).Value, "Multiply method 1" + Error);
			Assert.AreEqual ((byte)24, SqlByte.Multiply(TestByte2, TestByte12).Value, "Multiply method 2" + Error);
			
			try {
				SqlByte.Multiply(TestByte128, TestByte2);
				Assert.Fail ("Multiply method 3");
			} catch(Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException" + Error);
			}

		}

		[Test]
		public void NotEquals()
		{
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);
			SqlByte TestByte128II = new SqlByte(128);

			Assert.IsTrue (SqlByte.NotEquals(TestByte12, TestByte128).Value, "NotEquals method 1" + Error);
			Assert.IsTrue (SqlByte.NotEquals(TestByte128, TestByte12).Value, "NotEquals method 2" + Error);
			Assert.IsTrue (SqlByte.NotEquals(TestByte128II, TestByte12).Value, "NotEquals method 3" + Error);
			Assert.IsTrue (!SqlByte.NotEquals(TestByte128II, TestByte128).Value, "NotEquals method 4" + Error);
			Assert.IsTrue (!SqlByte.NotEquals(TestByte128, TestByte128II).Value, "NotEquals method 5" + Error);

		}

		[Test]
		public void OnesComplement()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);

			Assert.AreEqual((SqlByte)243, SqlByte.OnesComplement(TestByte12), "OnesComplement method 1" + Error);
			Assert.AreEqual((SqlByte)127, SqlByte.OnesComplement(TestByte128), "OnesComplement method 2" + Error);

		}

		[Test]
		public void Parse()
		{
			try {
				SqlByte.Parse(null);
				Assert.Fail("Parse method 2" + Error);
			}
			catch (Exception e) {
				Assert.AreEqual(typeof(ArgumentNullException), e.GetType(), "Parse method 3" + Error);
			}

			try {
				SqlByte.Parse("not-a-number");
				Assert.Fail("Parse method 4" + Error);
			}
			catch (Exception e) {
				Assert.AreEqual(typeof(FormatException), e.GetType(), "Parse method 5" + Error);
			}

			try {
				int OverInt = (int)SqlByte.MaxValue + 1;
				SqlByte.Parse(OverInt.ToString());
				Assert.Fail("Parse method 6" + Error);
			}
			catch (Exception e) {
				Assert.AreEqual(typeof(OverflowException), e.GetType(), "Parse method 7" + Error);
			}

			Assert.AreEqual((byte)150, SqlByte.Parse("150").Value, "Parse method 8" + Error);

		}

		[Test]
		public void Subtract()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);
			Assert.AreEqual((byte)116, SqlByte.Subtract(TestByte128, TestByte12).Value, "Subtract method 1" + Error);

			try {
				SqlByte.Subtract(TestByte12, TestByte128);
			} catch(Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

		}

		[Test]
		public void ToSqlBoolean()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByteNull = SqlByte.Null;

			Assert.IsTrue (TestByte12.ToSqlBoolean().Value, "ToSqlBoolean method 1" + Error);
			Assert.IsTrue (!TestByte0.ToSqlBoolean().Value, "ToSqlBoolean method 2" + Error);
			Assert.IsTrue (TestByteNull.ToSqlBoolean().IsNull, "ToSqlBoolean method 3" + Error);
		}

		[Test]
		public void ToSqlDecimal()
		{
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assert.AreEqual((decimal)12, TestByte12.ToSqlDecimal().Value, "ToSqlDecimal method 1" + Error);
			Assert.AreEqual((decimal)0, TestByte0.ToSqlDecimal().Value, "ToSqlDecimal method 2" + Error);
			Assert.AreEqual((decimal)228, TestByte228.ToSqlDecimal().Value, "ToSqlDecimal method 3" + Error);
			
		}

		[Test]
		public void ToSqlDouble()
		{
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assert.AreEqual((double)12, TestByte12.ToSqlDouble().Value, "ToSqlDouble method 1" + Error);
			Assert.AreEqual((double)0, TestByte0.ToSqlDouble().Value, "ToSqlDouble method 2" + Error);
			Assert.AreEqual((double)228, TestByte228.ToSqlDouble().Value, "ToSqlDouble method 3" + Error);

		}

		[Test]
		public void ToSqlInt16()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assert.AreEqual((short)12, TestByte12.ToSqlInt16().Value, "ToSqInt16 method 1" + Error);
			Assert.AreEqual((short)0, TestByte0.ToSqlInt16().Value, "ToSqlInt16 method 2" + Error);
			Assert.AreEqual((short)228, TestByte228.ToSqlInt16().Value, "ToSqlInt16 method 3" + Error);
		}

		[Test]
		public void ToSqlInt32()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);
			
			Assert.AreEqual((int)12, TestByte12.ToSqlInt32().Value, "ToSqInt32 method 1" + Error);
			Assert.AreEqual((int)0, TestByte0.ToSqlInt32().Value, "ToSqlInt32 method 2" + Error);
			Assert.AreEqual((int)228, TestByte228.ToSqlInt32().Value, "ToSqlInt32 method 3" + Error);

		}

		[Test]
		public void ToSqlInt64()
		{
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assert.AreEqual((long)12, TestByte12.ToSqlInt64().Value, "ToSqInt64 method " + Error);
			Assert.AreEqual((long)0, TestByte0.ToSqlInt64().Value, "ToSqlInt64 method 2" + Error);
			Assert.AreEqual((long)228, TestByte228.ToSqlInt64().Value, "ToSqlInt64 method 3" + Error);

		}

		[Test]
		public void ToSqlMoney()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assert.AreEqual(12.0000M, TestByte12.ToSqlMoney().Value, "ToSqMoney method 1" + Error);
			Assert.AreEqual((decimal)0, TestByte0.ToSqlMoney().Value, "ToSqlMoney method 2" + Error);
			Assert.AreEqual(228.0000M, TestByte228.ToSqlMoney().Value, "ToSqlMoney method 3" + Error);
		}

		[Test]
		public void ToSqlSingle()
		{
	    
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assert.AreEqual((float)12, TestByte12.ToSqlSingle().Value, "ToSqlSingle method 1" + Error);
			Assert.AreEqual((float)0, TestByte0.ToSqlSingle().Value, "ToSqlSingle method 2" + Error);
			Assert.AreEqual((float)228, TestByte228.ToSqlSingle().Value, "ToSqlSingle method 3" + Error);

		}

		[Test]
		public void ToSqlString()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assert.AreEqual("12", TestByte12.ToSqlString().Value, "ToSqlString method 1" + Error);
			Assert.AreEqual("0", TestByte0.ToSqlString().Value, "ToSqlString method 2" + Error);
			Assert.AreEqual("228", TestByte228.ToSqlString().Value, "ToSqlString method 3" + Error);

		}

		[Test]
		public void ToStringTest()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);
			
			Assert.AreEqual("12", TestByte12.ToString(), "ToString method 1" + Error);
			Assert.AreEqual("0", TestByte0.ToString(), "ToString method 2" + Error);
			Assert.AreEqual("228", TestByte228.ToString(), "ToString method 3" + Error);
		}

		[Test]
		public void TestXor()
		{

			SqlByte TestByte14 = new SqlByte(14);
			SqlByte TestByte58 = new SqlByte(58);
			SqlByte TestByte130 = new SqlByte(130);

			Assert.AreEqual((byte)52, SqlByte.Xor(TestByte14, TestByte58).Value, "Xor method 1" + Error);
			Assert.AreEqual((byte)140, SqlByte.Xor(TestByte14, TestByte130).Value, "Xor method 2" + Error);
			Assert.AreEqual((byte)184, SqlByte.Xor(TestByte58, TestByte130).Value, "Xor method 3" + Error);

		}

		// OPERATORS
		
		[Test]
		public void AdditionOperator()
		{

			SqlByte TestByte24 = new SqlByte(24);
			SqlByte TestByte64 = new SqlByte(64);
			SqlByte TestByte255 = new SqlByte(255);

			Assert.AreEqual((SqlByte)88,TestByte24 + TestByte64, "Addition operator" + Error);

			try {
				SqlByte result = TestByte64 + TestByte255;
				Assert.Fail("Addition operator 1" + Error);
			} catch (Exception e) {
				Assert.AreEqual(typeof(OverflowException), e.GetType(), "Addition operator 2" + Error);
			}
			
		}

		[Test]
		public void BitwiseAndOperator()
		{

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte4 = new SqlByte(4);
			SqlByte TestByte255 = new SqlByte(255);

			Assert.AreEqual((SqlByte)0,TestByte2 & TestByte4, "Bitwise and operator 1" + Error);
			Assert.AreEqual((SqlByte)2, TestByte2 & TestByte255, "Bitwise and operaror 2" + Error);
		}

		[Test]
		public void BitwiseOrOperator()
		{

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte4 = new SqlByte(4);
			SqlByte TestByte255 = new SqlByte(255);

			Assert.AreEqual((SqlByte)6,TestByte2 | TestByte4, "Bitwise or operator 1" + Error);
			Assert.AreEqual((SqlByte)255, TestByte2 | TestByte255, "Bitwise or operaror 2" + Error);
		}

		[Test]
		public void DivisionOperator()
		{

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte4 = new SqlByte(4);
			SqlByte TestByte255 = new SqlByte(255);
			SqlByte TestByte0 = new SqlByte(0);

			Assert.AreEqual((SqlByte)2,TestByte4 / TestByte2, "Division operator 1" + Error);
			Assert.AreEqual((SqlByte)127, TestByte255 / TestByte2, "Division operaror 2" + Error);

			try {
				TestByte2 = TestByte255 / TestByte0;
				Assert.Fail("Division operator 3" + Error);
			} catch (Exception e) {
				Assert.AreEqual(typeof(DivideByZeroException), e.GetType(), "DivideByZeroException");
			}

		}

		[Test]
		public void EqualityOperator()
		{

			SqlByte TestByte15 = new SqlByte(15);
			SqlByte TestByte15II = new SqlByte(15);
			SqlByte TestByte255 = new SqlByte(255);

			Assert.IsTrue ((TestByte15 == TestByte15II).Value, "== operator" + Error);
			Assert.IsTrue (!(TestByte15 == TestByte255).Value, "== operator 2" + Error);
			Assert.IsTrue (!(TestByte15 != TestByte15II).Value, "!= operator" + Error);
			Assert.IsTrue ((TestByte15 != TestByte255).Value, "!= operator 2" + Error);

		}

		[Test]
		public void ExclusiveOrOperator()
		{

			SqlByte TestByte15 = new SqlByte(15);
			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte255 = new SqlByte(255);

			Assert.AreEqual((SqlByte)5, (TestByte15 ^ TestByte10), "Exclusive or operator 1" + Error);
			Assert.AreEqual((SqlByte)240, (TestByte15 ^ TestByte255), "Exclusive or operator 2" + Error);
		}

		[Test]
		public void ThanOrEqualOperators()
		{

			SqlByte TestByte165 = new SqlByte(165);
			SqlByte TestByte100 = new SqlByte(100);
			SqlByte TestByte100II = new SqlByte(100);
			SqlByte TestByte255 = new SqlByte(255);

			Assert.IsTrue ((TestByte165 > TestByte100).Value, "> operator 1" + Error);
			Assert.IsTrue (!(TestByte165 > TestByte255).Value, "> operator 2" + Error);
			Assert.IsTrue (!(TestByte100 > TestByte100II).Value, "> operator 3" + Error);
			Assert.IsTrue (!(TestByte165 >= TestByte255).Value, ">= operator 1" + Error);
			Assert.IsTrue ((TestByte255 >= TestByte165).Value, ">= operator 2" + Error);
			Assert.IsTrue ((TestByte100 >= TestByte100II).Value, ">= operator 3" + Error);

			Assert.IsTrue (!(TestByte165 < TestByte100).Value, "< operator 1" + Error);
			Assert.IsTrue ((TestByte165 < TestByte255).Value, "< operator 2" + Error);
			Assert.IsTrue (!(TestByte100 < TestByte100II).Value, "< operator 3" + Error);
			Assert.IsTrue ((TestByte165 <= TestByte255).Value, "<= operator 1" + Error);
			Assert.IsTrue (!(TestByte255 <= TestByte165).Value, "<= operator 2" + Error);
			Assert.IsTrue ((TestByte100 <= TestByte100II).Value, "<= operator 3" + Error);
		}

		[Test]
		public void MultiplicationOperator()
		{

			SqlByte TestByte4 = new SqlByte(4);
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);

			Assert.AreEqual((SqlByte)48, TestByte4 * TestByte12, "Multiplication operator 1" + Error);
			try {
				SqlByte test = (TestByte128 * TestByte4);
				Assert.Fail("Multiplication operator 2" + Error);
			} catch (Exception e) {
				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

		}

		[Test]
		public void OnesComplementOperator()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);

			Assert.AreEqual((SqlByte)243, ~TestByte12, "OnesComplement operator 1" + Error);
			Assert.AreEqual((SqlByte)127, ~TestByte128, "OnesComplement operator 2" + Error);

		}

		[Test]
		public void SubtractionOperator()
		{

			SqlByte TestByte4 = new SqlByte(4);
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);

			Assert.AreEqual((SqlByte)8, TestByte12 - TestByte4, "Subtraction operator 1" + Error);
			try {
				
				SqlByte test = TestByte4 - TestByte128;
				Assert.Fail("Sybtraction operator 2" + Error);

			} catch (Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

		}

		[Test]
		public void SqlBooleanToSqlByte()
		{
			SqlBoolean TestBoolean = new SqlBoolean(true);
			SqlByte TestByte;

			TestByte = (SqlByte)TestBoolean;
			
			Assert.AreEqual((byte)1, TestByte.Value, "SqlBooleanToSqlByte op" + Error);
		}

		[Test]
		public void SqlByteToByte()
		{
			SqlByte TestByte = new SqlByte(12);
			byte test = (byte)TestByte;
			Assert.AreEqual((byte)12, test, "SqlByteToByte" + Error);
		}

		[Test]
		public void SqlDecimalToSqlByte()
		{
			SqlDecimal TestDecimal64 = new SqlDecimal(64);
			SqlDecimal TestDecimal900 = new SqlDecimal(900);

			Assert.AreEqual((byte)64, ((SqlByte)TestDecimal64).Value, "SqlDecimalToByte" + Error);

			try {
				SqlByte test = (SqlByte)TestDecimal900;
				Assert.Fail("SqlDecimalToByte 2" + Error);
			} catch (Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

		}

		[Test]
		public void SqlDoubleToSqlByte()
		{
			SqlDouble TestDouble64 = new SqlDouble(64);
			SqlDouble TestDouble900 = new SqlDouble(900);

			Assert.AreEqual((byte)64, ((SqlByte)TestDouble64).Value, "SqlDecimalToByte" + Error);

			try {
				SqlByte test = (SqlByte)TestDouble900;
				Assert.Fail("SqlDoubleToByte 2" + Error);
			} catch (Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

		}

		[Test]
		public void SqlInt16ToSqlByte()
	        {
			SqlInt16 TestInt1664 = new SqlInt16(64);
			SqlInt16 TestInt16900 = new SqlInt16(900);
			
			Assert.AreEqual((byte)64, ((SqlByte)TestInt1664).Value, "SqlInt16ToByte" + Error);

			try {
				SqlByte test = (SqlByte)TestInt16900;
				Assert.Fail("SqlInt16ToByte 2" + Error);
			} catch (Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

		}

		[Test]
		public void SqlInt32ToSqlByte()
		{
			SqlInt32 TestInt3264 = new SqlInt32(64);
			SqlInt32 TestInt32900 = new SqlInt32(900);

			Assert.AreEqual((byte)64, ((SqlByte)TestInt3264).Value, "SqlInt32ToByte" + Error);

			try {
				SqlByte test = (SqlByte)TestInt32900;
				Assert.Fail("SqlInt32ToByte 2" + Error);
			} catch (Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

		}

		[Test]
		public void SqlInt64ToSqlByte()
		{
			SqlInt64 TestInt6464 = new SqlInt64(64);
			SqlInt64 TestInt64900 = new SqlInt64(900);

			Assert.AreEqual((byte)64, ((SqlByte)TestInt6464).Value, "SqlInt64ToByte" + Error);

			try {
				SqlByte test = (SqlByte)TestInt64900;
				Assert.Fail("SqlInt64ToByte 2" + Error);
			} catch (Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

		}

		[Test]
		public void SqlMoneyToSqlByte()
		{
			SqlMoney TestMoney64 = new SqlMoney(64);
			SqlMoney TestMoney900 = new SqlMoney(900);

			Assert.AreEqual((byte)64, ((SqlByte)TestMoney64).Value, "SqlMoneyToByte" + Error);

			try {
				SqlByte test = (SqlByte)TestMoney900;
				Assert.Fail("SqlMoneyToByte 2" + Error);
			} catch (Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

		}

		[Test]
		public void SqlSingleToSqlByte()
		{
			SqlSingle TestSingle64 = new SqlSingle(64);
			SqlSingle TestSingle900 = new SqlSingle(900);

			Assert.AreEqual((byte)64, ((SqlByte)TestSingle64).Value, "SqlSingleToByte" + Error);

			try {
				SqlByte test = (SqlByte)TestSingle900;
				Assert.Fail("SqlSingleToByte 2" + Error);
			} catch (Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

		}

		[Test]
		public void SqlStringToSqlByte()
		{
			SqlString TestString = new SqlString("Test string");
			SqlString TestString100 = new SqlString("100");
			SqlString TestString1000 = new SqlString("1000");

			Assert.AreEqual ((byte)100, ((SqlByte)TestString100).Value, "SqlStringToByte 1" + Error);

			try {
				SqlByte test = (SqlByte)TestString1000;
			} catch(Exception e) {

				Assert.AreEqual(typeof(OverflowException), e.GetType(), "OverflowException");
			}

			try {
				SqlByte test = (SqlByte)TestString;
				Assert.Fail("SqlStringToByte 2" + Error);
				
			} catch(Exception e) {
				Assert.AreEqual(typeof(FormatException), e.GetType(), "FormatException");
			}
		}

		[Test]
		public void ByteToSqlByte()
		{
			byte TestByte = 14;
			Assert.AreEqual ((byte)14, ((SqlByte)TestByte).Value, "ByteToSqlByte" + Error);
		}
#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlByte.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("unsignedByte", qualifiedName.Name, "#A01");
		}
#endif
	}
}

