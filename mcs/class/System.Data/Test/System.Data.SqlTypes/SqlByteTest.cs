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

using NUnit.Framework;
using System;
using System.Data.SqlTypes;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlByteTest {
		private const string Error = " does not work correctly";

		// Test constructor
		[Test]
		public void Create()
		{
			byte b = 29;
			SqlByte TestByte = new SqlByte(b);
			Assertion.AssertEquals("Constructor 1 does not work correctly", (byte)29, TestByte.Value);
		}

		// Test public fields
		[Test]
		public void PublicFields()
		{
			Assertion.AssertEquals("MaxValue field" + Error, (SqlByte)255, SqlByte.MaxValue);
			Assertion.AssertEquals("MinValue field" + Error, (SqlByte)0, SqlByte.MinValue);
			Assertion.Assert("Null field" + Error, SqlByte.Null.IsNull);
			Assertion.AssertEquals("Zero field" + Error, (byte)0, SqlByte.Zero.Value);
		}

		// Test properties
		[Test]
		public void Properties()
		{

			SqlByte TestByte = new SqlByte(54);
			SqlByte TestByte2 = new SqlByte(1);

			Assertion.Assert("IsNull property" + Error, SqlByte.Null.IsNull);
			Assertion.AssertEquals("Value property 1" + Error, (byte)54, TestByte.Value);
			Assertion.AssertEquals("Value property 2" + Error, (byte)1, TestByte2.Value);

		}

		// PUBLIC STATIC METHODS
		[Test]
		public void AddMethod()
		{

			SqlByte TestByte64 = new SqlByte(64);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte164 = new SqlByte(164);
			SqlByte TestByte255 = new SqlByte(255);

			Assertion.AssertEquals("AddMethod 1" + Error, (byte)64, SqlByte.Add(TestByte64, TestByte0).Value);
			Assertion.AssertEquals("AddMethod 2" + Error, (byte)228, SqlByte.Add(TestByte64, TestByte164).Value);
			Assertion.AssertEquals("AddMethod 3" + Error, (byte)164, SqlByte.Add(TestByte0, TestByte164).Value);
			Assertion.AssertEquals("AddMethod 4" + Error, (byte)255, SqlByte.Add(TestByte255, TestByte0).Value);

			try {
				SqlByte.Add(TestByte255, TestByte64);
				Assertion.Fail ("AddMethod 6" + Error);
			} catch (Exception e) {
				Assertion.AssertEquals("AddMethod 5" + Error, typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void BitwiseAndMethod()
		{

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte1 = new SqlByte(1);
			SqlByte TestByte62 = new SqlByte(62);
			SqlByte TestByte255 = new SqlByte(255);

			Assertion.AssertEquals("BitwiseAnd method 1" + Error,
				     (byte)0, SqlByte.BitwiseAnd(TestByte2, TestByte1).Value);
			Assertion.AssertEquals("BitwiseAnd method 2" + Error,
				     (byte)0, SqlByte.BitwiseAnd(TestByte1, TestByte62).Value);
			Assertion.AssertEquals("BitwiseAnd method 3" + Error,
				     (byte)2, SqlByte.BitwiseAnd(TestByte62, TestByte2).Value);
			Assertion.AssertEquals("BitwiseAnd method 4" + Error,
				     (byte)1, SqlByte.BitwiseAnd(TestByte1, TestByte255).Value);
			Assertion.AssertEquals("BitwiseAnd method 5" + Error,
				     (byte)62, SqlByte.BitwiseAnd(TestByte62, TestByte255).Value);

		}

		[Test]
		public void BitwiseOrMethod()
		{

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte1 = new SqlByte(1);
			SqlByte TestByte62 = new SqlByte(62);
			SqlByte TestByte255 = new SqlByte(255);

			Assertion.AssertEquals("BitwiseOr method 1" + Error,
				     (byte)3, SqlByte.BitwiseOr(TestByte2, TestByte1).Value);
			Assertion.AssertEquals("BitwiseOr method 2" + Error,
				     (byte)63, SqlByte.BitwiseOr(TestByte1, TestByte62).Value);
			Assertion.AssertEquals("BitwiseOr method 3" + Error,
				     (byte)62, SqlByte.BitwiseOr(TestByte62, TestByte2).Value);
			Assertion.AssertEquals("BitwiseOr method 4" + Error,
				     (byte)255, SqlByte.BitwiseOr(TestByte1, TestByte255).Value);
			Assertion.AssertEquals("BitwiseOr method 5" + Error,
				     (byte)255, SqlByte.BitwiseOr(TestByte62, TestByte255).Value);

		}

		[Test]
		public void CompareTo()
		{

			SqlByte TestByte13 = new SqlByte(13);
			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte10II = new SqlByte(10);

			SqlString TestString = new SqlString("This is a test");
			
			Assertion.Assert("CompareTo method 1" + Error, TestByte13.CompareTo(TestByte10) > 0);
			Assertion.Assert("CompareTo method 2" + Error, TestByte10.CompareTo(TestByte13) < 0);
			Assertion.Assert("CompareTo method 3" + Error, TestByte10.CompareTo(TestByte10II) == 0);
			
			try {
				TestByte13.CompareTo(TestString);
				Assertion.Fail("CompareTo method 4" + Error);
			} catch(Exception e) {
				Assertion.AssertEquals("Parse method 5" + Error, typeof(ArgumentException), e.GetType());
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

			Assertion.AssertEquals("Divide method 1" + Error,
				     (byte)6, SqlByte.Divide(TestByte13, TestByte2).Value);
			Assertion.AssertEquals("Divide method 2" + Error,
				     (byte)90, SqlByte.Divide(TestByte180, TestByte2).Value);
			Assertion.AssertEquals("Divide method 3" + Error,
				     (byte)60, SqlByte.Divide(TestByte180, TestByte3).Value);
			Assertion.AssertEquals("Divide method 4" + Error,
				     (byte)0, SqlByte.Divide(TestByte13, TestByte180).Value);
			Assertion.AssertEquals("Divide method 5" + Error,
				     (byte)0, SqlByte.Divide(TestByte13, TestByte180).Value);

			try {
				SqlByte.Divide(TestByte13, TestByte0);
				Assertion.Fail ("Divide method 6" + Error);
			} catch(Exception e) {
				Assertion.AssertEquals("DivideByZeroException", typeof(DivideByZeroException), e.GetType());

			}

		}

		[Test]
		public void EqualsMethod()
		{

			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte158 = new SqlByte(158);
			SqlByte TestByte180 = new SqlByte(180);
			SqlByte TestByte180II = new SqlByte(180);

			Assertion.Assert("Equals method 1" + Error, !TestByte0.Equals(TestByte158));
			Assertion.Assert("Equals method 2" + Error, !TestByte158.Equals(TestByte180));
			Assertion.Assert("Equals method 3" + Error, !TestByte180.Equals(new SqlString("TEST")));
			Assertion.Assert("Equals method 4" + Error, TestByte180.Equals(TestByte180II));

		}

		[Test]
		public void StaticEqualsMethod()
		{

			SqlByte TestByte34 = new SqlByte(34);
			SqlByte TestByte34II = new SqlByte(34);
			SqlByte TestByte15 = new SqlByte(15);
			
			Assertion.Assert("static Equals method 1" + Error, SqlByte.Equals(TestByte34, TestByte34II).Value);
			Assertion.Assert("static Equals method 2" + Error, !SqlByte.Equals(TestByte34, TestByte15).Value);
			Assertion.Assert("static Equals method 3" + Error, !SqlByte.Equals(TestByte15, TestByte34II).Value);

		}

		[Test]
		public void GetHashCodeTest()
		{

			SqlByte TestByte15 = new SqlByte(15);
			SqlByte TestByte216 = new SqlByte(216);
			
			Assertion.AssertEquals("GetHashCode method 1" + Error, 15, TestByte15.GetHashCode());
			Assertion.AssertEquals("GetHashCode method 2" + Error, 216, TestByte216.GetHashCode());

		}

		[Test]
		public void GetTypeTest()
		{

			SqlByte TestByte = new SqlByte(84);

			Assertion.AssertEquals("GetType method" + Error,
				     "System.Data.SqlTypes.SqlByte", TestByte.GetType().ToString());
			
		}

		[Test]
		public void GreaterThan()
		{

			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte10II = new SqlByte(10);
			SqlByte TestByte110 = new SqlByte(110);

			Assertion.Assert("GreaterThan method 1" + Error, !SqlByte.GreaterThan(TestByte10, TestByte110).Value);
			Assertion.Assert("GreaterThan method 2" + Error, SqlByte.GreaterThan(TestByte110, TestByte10).Value);
			Assertion.Assert("GreaterThan method 3" + Error, !SqlByte.GreaterThan(TestByte10II, TestByte10).Value);

		}

		[Test]
		public void GreaterThanOrEqual()
		{

			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte10II = new SqlByte(10);
			SqlByte TestByte110 = new SqlByte(110);

			Assertion.Assert("GreaterThanOrEqual method 1" + Error,
			       !SqlByte.GreaterThanOrEqual(TestByte10, TestByte110).Value);

			Assertion.Assert("GreaterThanOrEqual method 2" + Error,
			       SqlByte.GreaterThanOrEqual(TestByte110, TestByte10).Value);

			Assertion.Assert("GreaterThanOrEqual method 3" + Error,
			       SqlByte.GreaterThanOrEqual(TestByte10II, TestByte10).Value);

		}

		[Test]
		public void LessThan()
		{

			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte10II = new SqlByte(10);
			SqlByte TestByte110 = new SqlByte(110);

			Assertion.Assert("LessThan method 1" + Error,
			       SqlByte.LessThan(TestByte10, TestByte110).Value);

			Assertion.Assert("LessThan method 2" + Error,
			       !SqlByte.LessThan(TestByte110, TestByte10).Value);

			Assertion.Assert("LessThan method 3" + Error,
			       !SqlByte.LessThan(TestByte10II, TestByte10).Value);

		}

		[Test]
		public void LessThanOrEqual()
		{

			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte10II = new SqlByte(10);
			SqlByte TestByte110 = new SqlByte(110);

			Assertion.Assert("LessThanOrEqual method 1" + Error,
			       SqlByte.LessThanOrEqual(TestByte10, TestByte110).Value);

			Assertion.Assert("LessThanOrEqual method 2" + Error,
			       !SqlByte.LessThanOrEqual(TestByte110, TestByte10).Value);

			Assertion.Assert("LessThanOrEqual method 3" + Error,
			       SqlByte.LessThanOrEqual(TestByte10II, TestByte10).Value);

			Assertion.Assert("LessThanOrEqual method 4" + Error,
			       SqlByte.LessThanOrEqual(TestByte10II, SqlByte.Null).IsNull);
		}

		[Test]
		public void Mod()
		{

			SqlByte TestByte132 = new SqlByte(132);
			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte200 = new SqlByte(200);

			Assertion.AssertEquals("Mod method 1" + Error, (SqlByte)2, SqlByte.Mod(TestByte132, TestByte10));
			Assertion.AssertEquals("Mod method 2" + Error,  (SqlByte)10, SqlByte.Mod(TestByte10, TestByte200));
			Assertion.AssertEquals("Mod method 3" + Error,  (SqlByte)0, SqlByte.Mod(TestByte200, TestByte10));
			Assertion.AssertEquals("Mod method 4" + Error,  (SqlByte)68, SqlByte.Mod(TestByte200, TestByte132));

		}

		[Test]
		public void Multiply()
		{

			SqlByte TestByte12 = new SqlByte (12);
			SqlByte TestByte2 = new SqlByte (2);
			SqlByte TestByte128 = new SqlByte (128);

			Assertion.AssertEquals ("Multiply method 1" + Error,
				      (byte)24, SqlByte.Multiply(TestByte12, TestByte2).Value);
			Assertion.AssertEquals ("Multiply method 2" + Error,
				      (byte)24, SqlByte.Multiply(TestByte2, TestByte12).Value);
			
			try {
				SqlByte.Multiply(TestByte128, TestByte2);
				Assertion.Fail ("Multiply method 3");
			} catch(Exception e) {

				Assertion.AssertEquals("OverflowException" + Error, typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void NotEquals()
		{
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);
			SqlByte TestByte128II = new SqlByte(128);

			Assertion.Assert("NotEquals method 1" + Error, SqlByte.NotEquals(TestByte12, TestByte128).Value);
			Assertion.Assert("NotEquals method 2" + Error, SqlByte.NotEquals(TestByte128, TestByte12).Value);
			Assertion.Assert("NotEquals method 3" + Error, SqlByte.NotEquals(TestByte128II, TestByte12).Value);
			Assertion.Assert("NotEquals method 4" + Error, !SqlByte.NotEquals(TestByte128II, TestByte128).Value);
			Assertion.Assert("NotEquals method 5" + Error, !SqlByte.NotEquals(TestByte128, TestByte128II).Value);

		}

		[Test]
		public void OnesComplement()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);

			Assertion.AssertEquals("OnesComplement method 1" + Error,
				     (SqlByte)243, SqlByte.OnesComplement(TestByte12));
			Assertion.AssertEquals("OnesComplement method 2" + Error,
				     (SqlByte)127, SqlByte.OnesComplement(TestByte128));

		}

		[Test]
		public void Parse()
		{
			try {
				SqlByte.Parse(null);
				Assertion.Fail("Parse method 2" + Error);
			}
			catch (Exception e) {
				Assertion.AssertEquals("Parse method 3" + Error, typeof(ArgumentNullException), e.GetType());
			}

			try {
				SqlByte.Parse("not-a-number");
				Assertion.Fail("Parse method 4" + Error);
			}
			catch (Exception e) {
				Assertion.AssertEquals("Parse method 5" + Error, typeof(FormatException), e.GetType());
			}

			try {
				int OverInt = (int)SqlByte.MaxValue + 1;
				SqlByte.Parse(OverInt.ToString());
				Assertion.Fail("Parse method 6" + Error);
			}
			catch (Exception e) {
				Assertion.AssertEquals("Parse method 7" + Error, typeof(OverflowException), e.GetType());
			}

			Assertion.AssertEquals("Parse method 8" + Error, (byte)150, SqlByte.Parse("150").Value);

		}

		[Test]
		public void Subtract()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);
			Assertion.AssertEquals("Subtract method 1" + Error, (byte)116, SqlByte.Subtract(TestByte128, TestByte12).Value);

			try {
				SqlByte.Subtract(TestByte12, TestByte128);
			} catch(Exception e) {

				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void ToSqlBoolean()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByteNull = SqlByte.Null;

			Assertion.Assert("ToSqlBoolean method 1" + Error, TestByte12.ToSqlBoolean().Value);
			Assertion.Assert("ToSqlBoolean method 2" + Error, !TestByte0.ToSqlBoolean().Value);
			Assertion.Assert("ToSqlBoolean method 3" + Error, TestByteNull.ToSqlBoolean().IsNull);
		}

		[Test]
		public void ToSqlDecimal()
		{
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assertion.AssertEquals("ToSqlDecimal method 1" + Error,
				     (decimal)12, TestByte12.ToSqlDecimal().Value);
			Assertion.AssertEquals("ToSqlDecimal method 2" + Error,
				     (decimal)0, TestByte0.ToSqlDecimal().Value);
			Assertion.AssertEquals("ToSqlDecimal method 3" + Error,
				     (decimal)228, TestByte228.ToSqlDecimal().Value);
			
		}

		[Test]
		public void ToSqlDouble()
		{
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assertion.AssertEquals("ToSqlDouble method 1" + Error,
				     (double)12, TestByte12.ToSqlDouble().Value);
			Assertion.AssertEquals("ToSqlDouble method 2" + Error,
				     (double)0, TestByte0.ToSqlDouble().Value);
			Assertion.AssertEquals("ToSqlDouble method 3" + Error,
				     (double)228, TestByte228.ToSqlDouble().Value);

		}

		[Test]
		public void ToSqlInt16()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assertion.AssertEquals("ToSqInt16 method 1" + Error,
				     (short)12, TestByte12.ToSqlInt16().Value);
			Assertion.AssertEquals("ToSqlInt16 method 2" + Error,
				     (short)0, TestByte0.ToSqlInt16().Value);
			Assertion.AssertEquals("ToSqlInt16 method 3" + Error,
				     (short)228, TestByte228.ToSqlInt16().Value);
		}

		[Test]
		public void ToSqlInt32()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);
			
			Assertion.AssertEquals("ToSqInt32 method 1" + Error,
				     (int)12, TestByte12.ToSqlInt32().Value);
			Assertion.AssertEquals("ToSqlInt32 method 2" + Error,
				     (int)0, TestByte0.ToSqlInt32().Value);
			Assertion.AssertEquals("ToSqlInt32 method 3" + Error,
				     (int)228, TestByte228.ToSqlInt32().Value);

		}

		[Test]
		public void ToSqlInt64()
		{
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assertion.AssertEquals("ToSqInt64 method " + Error,
				     (long)12, TestByte12.ToSqlInt64().Value);
			Assertion.AssertEquals("ToSqlInt64 method 2" + Error,
				     (long)0, TestByte0.ToSqlInt64().Value);
			Assertion.AssertEquals("ToSqlInt64 method 3" + Error,
				     (long)228, TestByte228.ToSqlInt64().Value);

		}

		[Test]
		public void ToSqlMoney()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assertion.AssertEquals("ToSqMoney method 1" + Error,
				     (decimal)12, TestByte12.ToSqlMoney().Value);
			Assertion.AssertEquals("ToSqlMoney method 2" + Error,
				     (decimal)0, TestByte0.ToSqlMoney().Value);
			Assertion.AssertEquals("ToSqlMoney method 3" + Error,
				     (decimal)228, TestByte228.ToSqlMoney().Value);
		}

		[Test]
		public void ToSqlSingle()
		{
	    
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assertion.AssertEquals("ToSqlSingle method 1" + Error,
				     (float)12, TestByte12.ToSqlSingle().Value);
			Assertion.AssertEquals("ToSqlSingle method 2" + Error,
				     (float)0, TestByte0.ToSqlSingle().Value);
			Assertion.AssertEquals("ToSqlSingle method 3" + Error,
				     (float)228, TestByte228.ToSqlSingle().Value);

		}

		[Test]
		public void ToSqlString()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);

			Assertion.AssertEquals("ToSqlString method 1" + Error,
				     "12", TestByte12.ToSqlString().Value);
			Assertion.AssertEquals("ToSqlString method 2" + Error,
				     "0", TestByte0.ToSqlString().Value);
			Assertion.AssertEquals("ToSqlString method 3" + Error,
				     "228", TestByte228.ToSqlString().Value);

		}

		[Test]
		public void ToStringTest()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte0 = new SqlByte(0);
			SqlByte TestByte228 = new SqlByte(228);
			
			Assertion.AssertEquals("ToString method 1" + Error,
				     "12", TestByte12.ToString());
			Assertion.AssertEquals("ToString method 2" + Error,
				     "0", TestByte0.ToString());
			Assertion.AssertEquals("ToString method 3" + Error,
				     "228", TestByte228.ToString());
		}

		[Test]
		public void TestXor()
		{

			SqlByte TestByte14 = new SqlByte(14);
			SqlByte TestByte58 = new SqlByte(58);
			SqlByte TestByte130 = new SqlByte(130);

			Assertion.AssertEquals("Xor method 1" + Error, (byte)52, SqlByte.Xor(TestByte14, TestByte58).Value);
			Assertion.AssertEquals("Xor method 2" + Error, (byte)140, SqlByte.Xor(TestByte14, TestByte130).Value);
			Assertion.AssertEquals("Xor method 3" + Error, (byte)184, SqlByte.Xor(TestByte58, TestByte130).Value);

		}

		// OPERATORS
		
		[Test]
		public void AdditionOperator()
		{

			SqlByte TestByte24 = new SqlByte(24);
			SqlByte TestByte64 = new SqlByte(64);
			SqlByte TestByte255 = new SqlByte(255);

			Assertion.AssertEquals("Addition operator" + Error, (SqlByte)88,TestByte24 + TestByte64);

			try {
				SqlByte result = TestByte64 + TestByte255;
				Assertion.Fail("Addition operator 1" + Error);
			} catch (Exception e) {
				Assertion.AssertEquals("Addition operator 2" + Error, typeof(OverflowException), e.GetType());
			}
			
		}

		[Test]
		public void BitwiseAndOperator()
		{

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte4 = new SqlByte(4);
			SqlByte TestByte255 = new SqlByte(255);

			Assertion.AssertEquals("Bitwise and operator 1" + Error, (SqlByte)0,TestByte2 & TestByte4);
			Assertion.AssertEquals("Bitwise and operaror 2" + Error, (SqlByte)2, TestByte2 & TestByte255);
		}

		[Test]
		public void BitwiseOrOperator()
		{

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte4 = new SqlByte(4);
			SqlByte TestByte255 = new SqlByte(255);

			Assertion.AssertEquals("Bitwise or operator 1" + Error, (SqlByte)6,TestByte2 | TestByte4);
			Assertion.AssertEquals("Bitwise or operaror 2" + Error, (SqlByte)255, TestByte2 | TestByte255);
		}

		[Test]
		public void DivisionOperator()
		{

			SqlByte TestByte2 = new SqlByte(2);
			SqlByte TestByte4 = new SqlByte(4);
			SqlByte TestByte255 = new SqlByte(255);
			SqlByte TestByte0 = new SqlByte(0);

			Assertion.AssertEquals("Division operator 1" + Error, (SqlByte)2,TestByte4 / TestByte2);
			Assertion.AssertEquals("Division operaror 2" + Error, (SqlByte)127, TestByte255 / TestByte2);

			try {
				TestByte2 = TestByte255 / TestByte0;
				Assertion.Fail("Division operator 3" + Error);
			} catch (Exception e) {
				Assertion.AssertEquals("DivideByZeroException", typeof(DivideByZeroException), e.GetType());
			}

		}

		[Test]
		public void EqualityOperator()
		{

			SqlByte TestByte15 = new SqlByte(15);
			SqlByte TestByte15II = new SqlByte(15);
			SqlByte TestByte255 = new SqlByte(255);

			Assertion.Assert("== operator" + Error, (TestByte15 == TestByte15II).Value);
			Assertion.Assert("== operator 2" + Error, !(TestByte15 == TestByte255).Value);
			Assertion.Assert("!= operator" + Error, !(TestByte15 != TestByte15II).Value);
			Assertion.Assert("!= operator 2" + Error, (TestByte15 != TestByte255).Value);

		}

		[Test]
		public void ExclusiveOrOperator()
		{

			SqlByte TestByte15 = new SqlByte(15);
			SqlByte TestByte10 = new SqlByte(10);
			SqlByte TestByte255 = new SqlByte(255);

			Assertion.AssertEquals("Exclusive or operator 1" + Error, (SqlByte)5, (TestByte15 ^ TestByte10));
			Assertion.AssertEquals("Exclusive or operator 2" + Error, (SqlByte)240, (TestByte15 ^ TestByte255));
		}

		[Test]
		public void ThanOrEqualOperators()
		{

			SqlByte TestByte165 = new SqlByte(165);
			SqlByte TestByte100 = new SqlByte(100);
			SqlByte TestByte100II = new SqlByte(100);
			SqlByte TestByte255 = new SqlByte(255);

			Assertion.Assert("> operator 1" + Error, (TestByte165 > TestByte100).Value);
			Assertion.Assert("> operator 2" + Error, !(TestByte165 > TestByte255).Value);
			Assertion.Assert("> operator 3" + Error, !(TestByte100 > TestByte100II).Value);
			Assertion.Assert(">= operator 1" + Error, !(TestByte165 >= TestByte255).Value);
			Assertion.Assert(">= operator 2" + Error, (TestByte255 >= TestByte165).Value);
			Assertion.Assert(">= operator 3" + Error, (TestByte100 >= TestByte100II).Value);

			Assertion.Assert("< operator 1" + Error, !(TestByte165 < TestByte100).Value);
			Assertion.Assert("< operator 2" + Error, (TestByte165 < TestByte255).Value);
			Assertion.Assert("< operator 3" + Error, !(TestByte100 < TestByte100II).Value);
			Assertion.Assert("<= operator 1" + Error, (TestByte165 <= TestByte255).Value);
			Assertion.Assert("<= operator 2" + Error, !(TestByte255 <= TestByte165).Value);
			Assertion.Assert("<= operator 3" + Error, (TestByte100 <= TestByte100II).Value);
		}

		[Test]
		public void MultiplicationOperator()
		{

			SqlByte TestByte4 = new SqlByte(4);
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);

			Assertion.AssertEquals("Multiplication operator 1" + Error, (SqlByte)48, TestByte4 * TestByte12);
			try {
				SqlByte test = (TestByte128 * TestByte4);
				Assertion.Fail("Multiplication operator 2" + Error);
			} catch (Exception e) {
				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void OnesComplementOperator()
		{

			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);

			Assertion.AssertEquals("OnesComplement operator 1" + Error,
				     (SqlByte)243, ~TestByte12);
			Assertion.AssertEquals("OnesComplement operator 2" + Error,
				     (SqlByte)127, ~TestByte128);

		}

		[Test]
		public void SubtractionOperator()
		{

			SqlByte TestByte4 = new SqlByte(4);
			SqlByte TestByte12 = new SqlByte(12);
			SqlByte TestByte128 = new SqlByte(128);

			Assertion.AssertEquals("Subtraction operator 1" + Error, (SqlByte)8, TestByte12 - TestByte4);
			try {
				
				SqlByte test = TestByte4 - TestByte128;
				Assertion.Fail("Sybtraction operator 2" + Error);

			} catch (Exception e) {

				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void SqlBooleanToSqlByte()
		{
			SqlBoolean TestBoolean = new SqlBoolean(true);
			SqlByte TestByte;

			TestByte = (SqlByte)TestBoolean;
			
			Assertion.AssertEquals("SqlBooleanToSqlByte op" + Error,
				     (byte)1, TestByte.Value);
		}

		[Test]
		public void SqlByteToByte()
		{
			SqlByte TestByte = new SqlByte(12);
			byte test = (byte)TestByte;
			Assertion.AssertEquals("SqlByteToByte" + Error, (byte)12, test);
		}

		[Test]
		public void SqlDecimalToSqlByte()
		{
			SqlDecimal TestDecimal64 = new SqlDecimal(64);
			SqlDecimal TestDecimal900 = new SqlDecimal(900);

			Assertion.AssertEquals("SqlDecimalToByte" + Error, (byte)64, ((SqlByte)TestDecimal64).Value);

			try {
				SqlByte test = (SqlByte)TestDecimal900;
				Assertion.Fail("SqlDecimalToByte 2" + Error);
			} catch (Exception e) {

				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void SqlDoubleToSqlByte()
		{
			SqlDouble TestDouble64 = new SqlDouble(64);
			SqlDouble TestDouble900 = new SqlDouble(900);

			Assertion.AssertEquals("SqlDecimalToByte" + Error, (byte)64, ((SqlByte)TestDouble64).Value);

			try {
				SqlByte test = (SqlByte)TestDouble900;
				Assertion.Fail("SqlDoubleToByte 2" + Error);
			} catch (Exception e) {

				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void SqlInt16ToSqlByte()
	        {
			SqlInt16 TestInt1664 = new SqlInt16(64);
			SqlInt16 TestInt16900 = new SqlInt16(900);
			
			Assertion.AssertEquals("SqlInt16ToByte" + Error, (byte)64, ((SqlByte)TestInt1664).Value);

			try {
				SqlByte test = (SqlByte)TestInt16900;
				Assertion.Fail("SqlInt16ToByte 2" + Error);
			} catch (Exception e) {

				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void SqlInt32ToSqlByte()
		{
			SqlInt32 TestInt3264 = new SqlInt32(64);
			SqlInt32 TestInt32900 = new SqlInt32(900);

			Assertion.AssertEquals("SqlInt32ToByte" + Error, (byte)64, ((SqlByte)TestInt3264).Value);

			try {
				SqlByte test = (SqlByte)TestInt32900;
				Assertion.Fail("SqlInt32ToByte 2" + Error);
			} catch (Exception e) {

				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void SqlInt64ToSqlByte()
		{
			SqlInt64 TestInt6464 = new SqlInt64(64);
			SqlInt64 TestInt64900 = new SqlInt64(900);

			Assertion.AssertEquals("SqlInt64ToByte" + Error, (byte)64, ((SqlByte)TestInt6464).Value);

			try {
				SqlByte test = (SqlByte)TestInt64900;
				Assertion.Fail("SqlInt64ToByte 2" + Error);
			} catch (Exception e) {

				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void SqlMoneyToSqlByte()
		{
			SqlMoney TestMoney64 = new SqlMoney(64);
			SqlMoney TestMoney900 = new SqlMoney(900);

			Assertion.AssertEquals("SqlMoneyToByte" + Error, (byte)64, ((SqlByte)TestMoney64).Value);

			try {
				SqlByte test = (SqlByte)TestMoney900;
				Assertion.Fail("SqlMoneyToByte 2" + Error);
			} catch (Exception e) {

				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void SqlSingleToSqlByte()
		{
			SqlSingle TestSingle64 = new SqlSingle(64);
			SqlSingle TestSingle900 = new SqlSingle(900);

			Assertion.AssertEquals("SqlSingleToByte" + Error, (byte)64, ((SqlByte)TestSingle64).Value);

			try {
				SqlByte test = (SqlByte)TestSingle900;
				Assertion.Fail("SqlSingleToByte 2" + Error);
			} catch (Exception e) {

				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

		}

		[Test]
		public void SqlStringToSqlByte()
		{
			SqlString TestString = new SqlString("Test string");
			SqlString TestString100 = new SqlString("100");
			SqlString TestString1000 = new SqlString("1000");

			Assertion.AssertEquals ("SqlStringToByte 1" + Error, (byte)100, ((SqlByte)TestString100).Value);

			try {
				SqlByte test = (SqlByte)TestString1000;
			} catch(Exception e) {

				Assertion.AssertEquals("OverflowException", typeof(OverflowException), e.GetType());
			}

			try {
				SqlByte test = (SqlByte)TestString;
				Assertion.Fail("SqlStringToByte 2" + Error);
				
			} catch(Exception e) {
				Assertion.AssertEquals("FormatException", typeof(FormatException), e.GetType());
			}
		}

		[Test]
		public void ByteToSqlByte()
		{
			byte TestByte = 14;
			Assertion.AssertEquals ("ByteToSqlByte" + Error,
				      (byte)14, ((SqlByte)TestByte).Value);
		}
	}
}

