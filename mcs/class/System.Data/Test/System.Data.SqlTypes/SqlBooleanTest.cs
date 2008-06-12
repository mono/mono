// SqlDataTimeTest.cs - NUnit Test Cases for [explain here]
//
// Authors:
//   Ville Palo (vi64pa@users.sourceforge.net)
//   Martin Willemoes Hansen
//
// (C) Ville Palo
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

namespace MonoTests.System.Data.SqlTypes
{

	[TestFixture]
        public class SqlBooleanTest {
		private SqlBoolean SqlTrue;
		private SqlBoolean SqlFalse;

		[SetUp]
		public void GetReady() {
			
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
			SqlTrue = new SqlBoolean(true);
			SqlFalse = new SqlBoolean(false);

		}

		[Test]
		public void Create ()
		{
			SqlBoolean SqlTrue2 = new SqlBoolean(1);
			SqlBoolean SqlFalse2 = new SqlBoolean(0);

			Assert.IsTrue (SqlTrue.Value, "Creation of SqlBoolean failed");
			Assert.IsTrue (SqlTrue2.Value, "Creation of SqlBoolean failed");
			Assert.IsTrue (!SqlFalse.Value, "Creation of SqlBoolean failed");
			Assert.IsTrue (!SqlFalse2.Value, "Creation of SqlBoolean failed");

		}

		////
		// PUBLIC STATIC METHODS
		//

		// And
		[Test]
		public void And() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			// One result value
			SqlBoolean sqlResult;

			// true && false
			sqlResult = SqlBoolean.And(SqlTrue, SqlFalse);
			Assert.IsTrue (!sqlResult.Value, "And method does not work correctly (true && false)");
			sqlResult = SqlBoolean.And(SqlFalse, SqlTrue);
			Assert.IsTrue (!sqlResult.Value, "And method does not work correctly (false && true)");

			// true && true
			sqlResult = SqlBoolean.And(SqlTrue, SqlTrue2);
			Assert.IsTrue (sqlResult.Value, "And method does not work correctly (true && true)");

			sqlResult = SqlBoolean.And(SqlTrue, SqlTrue);
			Assert.IsTrue (sqlResult.Value, "And method does not work correctly (true && true2)");

			// false && false
			sqlResult = SqlBoolean.And(SqlFalse, SqlFalse2);
			Assert.IsTrue (!sqlResult.Value, "And method does not work correctly (false && false)");
			sqlResult = SqlBoolean.And(SqlFalse, SqlFalse);
			Assert.IsTrue (!sqlResult.Value, "And method does not work correctly (false && false2)");

		}

		// NotEquals
		[Test]
		public void NotEquals() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;

			// true != false
			SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlFalse);
			Assert.IsTrue (SqlResult.Value, "NotEquals method does not work correctly (true != false)");
			SqlResult = SqlBoolean.NotEquals(SqlFalse, SqlTrue);
			Assert.IsTrue (SqlResult.Value, "NotEquals method does not work correctly (false != true)");


			// true != true
			SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlTrue);
			Assert.IsTrue (!SqlResult.Value, "NotEquals method does not work correctly (true != true)");
			SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlTrue2);
			Assert.IsTrue (!SqlResult.Value, "NotEquals method does not work correctly (true != true2)");
			// false != false
			SqlResult = SqlBoolean.NotEquals(SqlFalse, SqlFalse);
			Assert.IsTrue (!SqlResult.Value, "NotEquals method does not work correctly (false != false)");
			SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlTrue2);
			Assert.IsTrue (!SqlResult.Value, "NotEquals method does not work correctly (false != false2)");

			// If either instance of SqlBoolean is null, the Value of the SqlBoolean will be Null.
			SqlResult = SqlBoolean.NotEquals(SqlBoolean.Null, SqlFalse);
			Assert.IsTrue (SqlResult.IsNull, "NotEquals method does not work correctly (Null != false)");
			SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlBoolean.Null);
			Assert.IsTrue (SqlResult.IsNull, "NotEquals method does not work correctly (false != Null)");

		}

		// OnesComplement
		[Test]
		public void OnesComplement() {

			SqlBoolean SqlFalse2 = SqlBoolean.OnesComplement(SqlTrue);
			Assert.IsTrue (!SqlFalse2.Value, "OnesComplement method does not work correctly");

			SqlBoolean SqlTrue2 = SqlBoolean.OnesComplement(SqlFalse);
			Assert.IsTrue (SqlTrue2.Value, "OnesComplement method does not work correctly");

		}

		// Or
		[Test]
		public void Or() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;

			// true || false
			SqlResult = SqlBoolean.Or(SqlTrue, SqlFalse);
			Assert.IsTrue (SqlResult.Value, "Or method does not work correctly (true || false)");
			SqlResult = SqlBoolean.Or(SqlFalse, SqlTrue);
			Assert.IsTrue (SqlResult.Value, "Or method does not work correctly (false || true)");

			// true || true
			SqlResult = SqlBoolean.Or(SqlTrue, SqlTrue);
			Assert.IsTrue (SqlResult.Value, "Or method does not work correctly (true || true)");
			SqlResult = SqlBoolean.Or(SqlTrue, SqlTrue2);
			Assert.IsTrue (SqlResult.Value, "Or method does not work correctly (true || true2)");

			// false || false
			SqlResult = SqlBoolean.Or(SqlFalse, SqlFalse);
			Assert.IsTrue (!SqlResult.Value, "Or method does not work correctly (false || false)");
			SqlResult = SqlBoolean.Or(SqlFalse, SqlFalse2);
			Assert.IsTrue (!SqlResult.Value, "Or method does not work correctly (false || false2)");

		}


		//  Parse
		[Test]
		public void Parse() {

			String error = "Parse method does not work correctly ";
                                                                         
			Assert.IsTrue (SqlBoolean.Parse("True").Value, "#1 " + error);
			Assert.IsTrue (SqlBoolean.Parse(" True").Value, "#2 " + error);
			Assert.IsTrue (SqlBoolean.Parse("True ").Value, "#3 " + error);
			Assert.IsTrue (SqlBoolean.Parse("tRuE").Value, "#4 " + error);
			Assert.IsTrue (!SqlBoolean.Parse("False").Value, "#5 " + error);
			Assert.IsTrue (!SqlBoolean.Parse(" False").Value, "#6 " + error);
			Assert.IsTrue (!SqlBoolean.Parse("False ").Value, "#7 " + error);
			Assert.IsTrue (!SqlBoolean.Parse("fAlSe").Value, "#8 " + error);

		}

		// Xor
		[Test]
		public void Xor() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;

			// true ^ false
			SqlResult = SqlBoolean.Xor(SqlTrue, SqlFalse);
			Assert.IsTrue (SqlResult.Value, "Xor method does not work correctly (true ^ false)");
			SqlResult = SqlBoolean.Xor(SqlFalse, SqlTrue);
			Assert.IsTrue (SqlResult.Value, "Xor method does not work correctly (false ^ true)");

			// true ^ true
			SqlResult = SqlBoolean.Xor(SqlTrue, SqlTrue2);
			Assert.IsTrue (!SqlResult.Value, "Xor method does not work correctly (true ^ true)");

			// false ^ false
			SqlResult = SqlBoolean.Xor(SqlFalse, SqlFalse2);
			Assert.IsTrue (!SqlResult.Value, "Xor method does not work correctly (false ^ false)");

		}

		// static Equals
		[Test]
		public void StaticEquals() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);
			String error = "Static Equals method does not work correctly ";

			Assert.IsTrue (SqlBoolean.Equals(SqlTrue, SqlTrue2).Value, error +  "(true == true)");
			Assert.IsTrue (SqlBoolean.Equals(SqlFalse, SqlFalse2).Value, error +  "(false == false)");

			Assert.IsTrue (!SqlBoolean.Equals(SqlTrue, SqlFalse).Value, error +  "(true == false)");
			Assert.IsTrue (!SqlBoolean.Equals(SqlFalse, SqlTrue).Value, error +  "(false == true)");

			Assert.AreEqual (SqlBoolean.Null, SqlBoolean.Equals(SqlBoolean.Null, SqlFalse), error +  "(null == false)");
			Assert.AreEqual (SqlBoolean.Null, SqlBoolean.Equals(SqlTrue, SqlBoolean.Null), error +  "(true == null)");

		}

		//
		// END OF STATIC METHODS
		////

		////
		// PUBLIC METHODS
		//

		// CompareTo
		[Test]
		public void CompareTo() {

			String error = "CompareTo method does not work correctly";

			Assert.IsTrue ((SqlTrue.CompareTo(SqlBoolean.Null) > 0), error);
			Assert.IsTrue ((SqlTrue.CompareTo(SqlFalse) > 0), error);
			Assert.IsTrue ((SqlFalse.CompareTo(SqlTrue) < 0), error);
			Assert.IsTrue ((SqlFalse.CompareTo(SqlFalse) == 0), error);

		}

		// Equals
		[Test]
		public void Equals() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			String error = "Equals method does not work correctly ";
			Assert.IsTrue (SqlTrue.Equals(SqlTrue2), error + "(true == true)");
			Assert.IsTrue (SqlFalse.Equals(SqlFalse2), error + "(false == false)");

			Assert.IsTrue (!SqlTrue.Equals(SqlFalse), error + "(true == false)");
			Assert.IsTrue (!SqlFalse.Equals(SqlTrue), error + "(false == true)");

			Assert.IsFalse (SqlTrue.Equals(SqlBoolean.Null), error + "(true != null)");
			Assert.IsFalse (SqlFalse.Equals(SqlBoolean.Null), error + "(false != null)");

			Assert.IsTrue (!SqlTrue.Equals(null), error + "(true == false)");
			Assert.IsTrue (SqlBoolean.Null.Equals (SqlBoolean.Null), "null == null");
			Assert.IsFalse (SqlBoolean.Null.Equals (SqlTrue), "null != true");
			Assert.IsFalse (SqlBoolean.Null.Equals (SqlFalse), "null != false");
		}

		[Test]
		public void GetHashCodeTest() {

			Assert.AreEqual (1, SqlTrue.GetHashCode(), "GetHashCode method does not work correctly");

			Assert.AreEqual (0, SqlFalse.GetHashCode(), "GetHashCode method does not work correctly");

		}

		// GetType
		[Test]
		public void GetTypeTest() {

			Assert.AreEqual ("System.Data.SqlTypes.SqlBoolean", SqlTrue.GetType().ToString(), "GetType method does not work correctly");
		}

		// ToSqlByte
		[Test]
		public void ToSqlByte() {

			SqlByte SqlTestByte;

			String error = "ToSqlByte method does not work correctly ";

			SqlTestByte = SqlTrue.ToSqlByte();
			Assert.AreEqual ((byte)1,SqlTestByte.Value, error);

			SqlTestByte = SqlFalse.ToSqlByte();
			Assert.AreEqual ((byte)0, SqlTestByte.Value, error);

		}

		// ToSqlDecimal
		[Test]
		public void ToSqlDecimal() {

			SqlDecimal SqlTestDecimal;

			String error = "ToSqlDecimal method does not work correctly ";
			SqlTestDecimal = SqlTrue.ToSqlDecimal();

			Assert.AreEqual ((decimal)1, SqlTestDecimal.Value, error);

			SqlTestDecimal = SqlFalse.ToSqlDecimal();
			Assert.AreEqual ((decimal)0, SqlTestDecimal.Value, error);
		}

		// ToSqlDouble
		[Test]
		public void ToSqlDouble() {

			SqlDouble SqlTestDouble;
			
			String error = "ToSqlDouble method does not work correctly ";
			SqlTestDouble = SqlTrue.ToSqlDouble();
			Assert.AreEqual ((double)1, SqlTestDouble.Value, error);

			SqlTestDouble = SqlFalse.ToSqlDouble();
			Assert.AreEqual ((double)0, SqlTestDouble.Value, error);
		}

		// ToSqlInt16
		[Test]
		public void ToSqlInt16() {

			SqlInt16 SqlTestInt16;

			String error = "ToSqlInt16 method does not work correctly ";
			SqlTestInt16 = SqlTrue.ToSqlInt16();
			Assert.AreEqual ((short)1, SqlTestInt16.Value, error);

			SqlTestInt16 = SqlFalse.ToSqlInt16();
			Assert.AreEqual ((short)0, SqlTestInt16.Value, error);

		}

		// ToSqlInt32
		[Test]
		public void ToSqlInt32() {

			SqlInt32 SqlTestInt32;

			String error = "ToSqlInt32 method does not work correctly ";
			SqlTestInt32 = SqlTrue.ToSqlInt32();
			Assert.AreEqual ((int)1, SqlTestInt32.Value, error);

			SqlTestInt32 = SqlFalse.ToSqlInt32();
			Assert.AreEqual ((int)0, SqlTestInt32.Value, error);

		}

		// ToSqlInt64
		[Test]
		public void ToSqlInt64() {

			SqlInt64 SqlTestInt64;

			String error = "ToSqlInt64 method does not work correctly ";
			
			SqlTestInt64 = SqlTrue.ToSqlInt64();
			Assert.AreEqual ((long)1, SqlTestInt64.Value, error);

			SqlTestInt64 = SqlFalse.ToSqlInt64();
			Assert.AreEqual ((long)0, SqlTestInt64.Value, error);

		}

		// ToSqlMoney
		[Test]
		public void ToSqlMoney() {

			SqlMoney SqlTestMoney;

			String error = "ToSqlMoney method does not work correctly ";
			SqlTestMoney = SqlTrue.ToSqlMoney();
			Assert.AreEqual (1.0000M, SqlTestMoney.Value, error);

			SqlTestMoney = SqlFalse.ToSqlMoney();
			Assert.AreEqual ((decimal)0, SqlTestMoney.Value, error);

		}

		// ToSqlSingle
		[Test]
		public void ToSqlsingle() {

			SqlSingle SqlTestSingle;

			String error = "ToSqlSingle method does not work correctly ";
			SqlTestSingle = SqlTrue.ToSqlSingle();
			Assert.AreEqual ((float)1, SqlTestSingle.Value, error);

			SqlTestSingle = SqlFalse.ToSqlSingle();
			Assert.AreEqual ( (float) 0, SqlTestSingle.Value, error);

		}

		// ToSqlString
		[Test]
		public void ToSqlString() {

			SqlString SqlTestString;

			String error = "ToSqlString method does not work correctly ";
			SqlTestString = SqlTrue.ToSqlString();
			Assert.AreEqual ("True", SqlTestString.Value, error);

			SqlTestString = SqlFalse.ToSqlString();
			Assert.AreEqual ("False", SqlTestString.Value, error);

		}

		// ToString
		[Test]
		public void ToStringTest() {

			SqlString TestString;

			String error = "ToString method does not work correctly ";

			TestString = SqlTrue.ToString();
			Assert.AreEqual ("True", TestString.Value, error);

			TestString = SqlFalse.ToSqlString();
			Assert.AreEqual ("False", TestString.Value, error);

		}

		// END OF PUBLIC METHODS
		////

		////
		// OPERATORS

		// BitwixeAnd operator
		[Test]
		public void BitwiseAndOperator() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;
			String error = "BitwiseAnd operator does not work correctly ";

			SqlResult = SqlTrue & SqlFalse;
			Assert.IsTrue (!SqlResult.Value, error + "(true & false)");
			SqlResult = SqlFalse & SqlTrue;
			Assert.IsTrue (!SqlResult.Value, error + "(false & true)");

			SqlResult = SqlTrue & SqlTrue2;
			Assert.IsTrue (SqlResult.Value, error + "(true & true)");

			SqlResult = SqlFalse & SqlFalse2;
			Assert.IsTrue (!SqlResult.Value, error + "(false & false)");


		}

		// BitwixeOr operator
		[Test]
		public void BitwiseOrOperator() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;
			String error = "BitwiseOr operator does not work correctly ";

			SqlResult = SqlTrue | SqlFalse;
			Assert.IsTrue (SqlResult.Value, error + "(true | false)");
			SqlResult = SqlFalse | SqlTrue;

			Assert.IsTrue (SqlResult.Value, error + "(false | true)");

			SqlResult = SqlTrue | SqlTrue2;
			Assert.IsTrue (SqlResult.Value, error + "(true | true)");

			SqlResult = SqlFalse | SqlFalse2;
			Assert.IsTrue (!SqlResult.Value, error + "(false | false)");

		}

		// Equality operator
		[Test]
		public void EqualityOperator() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;
			String error = "Equality operator does not work correctly ";

			SqlResult = SqlTrue == SqlFalse;
			Assert.IsTrue (!SqlResult.Value, error + "(true == false)");
			SqlResult = SqlFalse == SqlTrue;
			Assert.IsTrue (!SqlResult.Value, error + "(false == true)");

			SqlResult = SqlTrue == SqlTrue2;
			Assert.IsTrue (SqlResult.Value, error + "(true == true)");

			SqlResult = SqlFalse == SqlFalse2;
			Assert.IsTrue (SqlResult.Value, error + "(false == false)");

			SqlResult = SqlFalse == SqlBoolean.Null;
			Assert.IsTrue (SqlResult.IsNull, error + "(false == Null)");
			//SqlResult = SqlBoolean.Null == SqlBoolean.Null;
			Assert.IsTrue (SqlResult.IsNull, error + "(Null == true)");

		}

		// ExlusiveOr operator
		[Test]
		public void ExlusiveOrOperator() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;
			String error = "ExclusiveOr operator does not work correctly ";

			SqlResult = SqlTrue ^ SqlFalse;
			Assert.IsTrue (SqlResult.Value, error + "(true ^ false)");
			SqlResult = SqlFalse | SqlTrue;
			Assert.IsTrue (SqlResult.Value, error + "(false ^ true)");

			SqlResult = SqlTrue ^ SqlTrue2;
			Assert.IsTrue (!SqlResult.Value, error + "(true ^ true)");

			SqlResult = SqlFalse ^ SqlFalse2;
			Assert.IsTrue (!SqlResult.Value, error + "(false ^ false)");

		}

		// false operator
		[Test]
		public void FalseOperator() {

			String error = "false operator does not work correctly ";

			Assert.AreEqual (SqlBoolean.False, (!SqlTrue), error + "(true)");
			Assert.AreEqual (SqlBoolean.True, (!SqlFalse), error + "(false)");

		}

		// Inequality operator
		[Test]
		public void InequalityOperator() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			String error = "Inequality operator does not work correctly" ;

			Assert.AreEqual (SqlBoolean.False, SqlTrue != true, error + "(true != true)");
			Assert.AreEqual (SqlBoolean.False, SqlTrue != SqlTrue2, error + "(true != true)");
			Assert.AreEqual (SqlBoolean.False, SqlFalse != false, error + "(false != false)");
			Assert.AreEqual (SqlBoolean.False, SqlFalse != SqlFalse2, error + "(false != false)");
			Assert.AreEqual (SqlBoolean.True, SqlTrue != SqlFalse, error + "(true != false)");
			Assert.AreEqual (SqlBoolean.True, SqlFalse != SqlTrue, error + "(false != true)");
			Assert.AreEqual (SqlBoolean.Null, SqlBoolean.Null != SqlTrue, error + "(null != true)");
			Assert.AreEqual (SqlBoolean.Null, SqlFalse != SqlBoolean.Null, error + "(false != null)");

		}

		// Logical Not operator
		[Test]
		public void LogicalNotOperator() {

			String error = "Logical Not operator does not work correctly" ;

			Assert.AreEqual (SqlBoolean.False, !SqlTrue, error + "(true)");
			Assert.AreEqual (SqlBoolean.True, !SqlFalse, error + "(false)");

		}

		// OnesComplement operator
		[Test]
		public void OnesComplementOperator() {

			String error = "Ones complement operator does not work correctly" ;

			SqlBoolean SqlResult;

			SqlResult = ~SqlTrue;
			Assert.IsTrue (!SqlResult.Value, error + "(true)");
			SqlResult = ~SqlFalse;
			Assert.IsTrue (SqlResult.Value, error + "(false)");

		}


		// true operator
		[Test]
		public void TrueOperator() {

			String error = "true operator does not work correctly ";

			Assert.AreEqual (SqlBoolean.True, (SqlTrue), error + "(true)");
			Assert.AreEqual (SqlBoolean.False, (SqlFalse), error + "(false)");

		}

		// SqlBoolean to Boolean
		[Test]
		public void SqlBooleanToBoolean() {

			String error = "SqlBooleanToBoolean operator does not work correctly ";

			Boolean TestBoolean = (Boolean)SqlTrue;
			Assert.IsTrue ( TestBoolean, error + "(true)");
			TestBoolean = (Boolean)SqlFalse;
			Assert.IsTrue ( !TestBoolean, error + "(false)");

		}

		// SqlByte to SqlBoolean
		[Test]
		public void SqlByteToSqlBoolean() {

			SqlByte SqlTestByte;
			SqlBoolean SqlTestBoolean;
			String error = "SqlByteToSqlBoolean operator does not work correctly ";

			SqlTestByte = new SqlByte(1);
			SqlTestBoolean = (SqlBoolean)SqlTestByte;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTestByte = new SqlByte(2);
			SqlTestBoolean = (SqlBoolean)SqlTestByte;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTestByte = new SqlByte(0);
			SqlTestBoolean = (SqlBoolean)SqlTestByte;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");

		}

		// SqlDecimal to SqlBoolean
		[Test]
		public void SqlDecimalToSqlBoolean() {

			SqlDecimal SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlDecimalToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlDecimal(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlDecimal(19);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlDecimal(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");

		}

		// SqlDouble to SqlBoolean
		[Test]
		public void SqlDoubleToSqlBoolean() {

			SqlDouble SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlDoubleToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlDouble(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlDouble(-19.8);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlDouble(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");

		}

		// SqlIn16 to SqlBoolean
		[Test]
		public void SqlInt16ToSqlBoolean() {

			SqlInt16 SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlInt16ToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlInt16(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlInt16(-143);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlInt16(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");

		}

		// SqlInt32 to SqlBoolean
		[Test]
		public void SqlInt32ToSqlBoolean() {

			SqlInt32 SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlInt32ToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlInt32(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlInt32(1430);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlInt32(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");
		}

		// SqlInt64 to SqlBoolean
		[Test]
		public void SqlInt64ToSqlBoolean() {

			SqlInt64 SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlInt64ToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlInt64(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlInt64(-14305);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlInt64(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");

		}

		// SqlMoney to SqlBoolean
		[Test]
		public void SqlMoneyToSqlBoolean() {

			SqlMoney SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlMoneyToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlMoney(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlMoney(1305);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlMoney(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");

		}

		// SqlSingle to SqlBoolean
		[Test]
		public void SqlSingleToSqlBoolean() {

			SqlSingle SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlSingleToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlSingle(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlSingle(1305);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlSingle(-305.3);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlSingle(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");

		}

		// SqlString to SqlBoolean
		[Test]
		public void SqlStringToSqlBoolean() {

			SqlString SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlSingleToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlString("true");
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlString("TRUE");
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlString("True");
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");

			SqlTest = new SqlString("false");
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");

		}

		// Boolean to SqlBoolean
		[Test]
		public void BooleanToSqlBoolean() {

			SqlBoolean SqlTestBoolean;
			bool btrue = true;
			bool bfalse = false;
			String error = "BooleanToSqlBoolean operator does not work correctly ";

			Boolean SqlTest = true;
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");
			SqlTestBoolean = (SqlBoolean)btrue;
			Assert.IsTrue (SqlTestBoolean.Value, error + "(true)");


			SqlTest = false;
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");
			SqlTestBoolean = (SqlBoolean)bfalse;
			Assert.IsTrue (!SqlTestBoolean.Value, error + "(false)");

		}

		// END OF OPERATORS
		////

		////
		// PROPERTIES

		// ByteValue property
		[Test]
		public void ByteValueProperty() {

			String error = "ByteValue property does not work correctly ";

			Assert.AreEqual ((byte)1, SqlTrue.ByteValue, error + "(true)");
			Assert.AreEqual ((byte)0, SqlFalse.ByteValue, error + "(false)");

		}

		// IsFalse property
		[Test]
		public void IsFalseProperty() {

			String error = "IsFalse property does not work correctly ";

			Assert.IsTrue (!SqlTrue.IsFalse, error + "(true)");
			Assert.IsTrue (SqlFalse.IsFalse, error + "(false)");

		}

		// IsNull property
		[Test]
		public void IsNullProperty() {

			String error = "IsNull property does not work correctly ";

			Assert.IsTrue (!SqlTrue.IsNull, error + "(true)");
			Assert.IsTrue (!SqlFalse.IsNull, error + "(false)");
			Assert.IsTrue (SqlBoolean.Null.IsNull, error + "(Null)");

		}

		// IsTrue property
		[Test]
		public void IsTrueProperty() {

			String error = "IsTrue property does not work correctly ";

			Assert.IsTrue (SqlTrue.IsTrue, error + "(true)");
			Assert.IsTrue (!SqlFalse.IsTrue, error + "(false)");

		}

		// Value property
		[Test]
		public void ValueProperty() {

			String error = "Value property does not work correctly ";

			Assert.IsTrue (SqlTrue.Value, error + "(true)");
			Assert.IsTrue (!SqlFalse.Value, error + "(false)");

		}

		// END OF PROPERTIEs
		////

		////
		// FIELDS

		[Test]
		public void FalseField() {

			Assert.IsTrue (!SqlBoolean.False.Value, "False field does not work correctly");

		}

		[Test]
		public void NullField() {

			Assert.IsTrue (SqlBoolean.Null.IsNull, "Null field does not work correctly");

		}

		[Test]
		public void OneField() {

			Assert.AreEqual ((byte)1, SqlBoolean.One.ByteValue, "One field does not work correctly");
		}

		[Test]
		public void TrueField() {

			Assert.IsTrue (SqlBoolean.True.Value, "True field does not work correctly");

		}

		[Test]
		public void ZeroField() {

			Assert.AreEqual ((byte)0, SqlBoolean.Zero.ByteValue, "Zero field does not work correctly");

		}
#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlBoolean.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("boolean", qualifiedName.Name, "#A01");
		}

		[Test]
		public void GreaterThanTest ()
		{
			SqlBoolean x = new SqlBoolean (-1);
			SqlBoolean y = new SqlBoolean (true);
			SqlBoolean z = new SqlBoolean ();
			SqlBoolean z1 = new SqlBoolean (0);

			NUnit.Framework.Assert.IsFalse ((x > y).Value, "#A01");
			NUnit.Framework.Assert.AreEqual (x > z, SqlBoolean.Null, "#A02");
			NUnit.Framework.Assert.IsTrue ((x > z1).Value, "#A03");
			NUnit.Framework.Assert.AreEqual (y > z, SqlBoolean.Null, "#A04");
			NUnit.Framework.Assert.IsFalse ((y > x).Value, "#A05");
			NUnit.Framework.Assert.IsTrue ((y > z1).Value, "#A06");
			NUnit.Framework.Assert.AreEqual (z > z1, SqlBoolean.Null, "#A07");
			NUnit.Framework.Assert.AreEqual (z > x, SqlBoolean.Null, "#A08");
			NUnit.Framework.Assert.AreEqual (z > y, SqlBoolean.Null, "#A09");
			NUnit.Framework.Assert.AreEqual (z1 > z, SqlBoolean.Null, "#A10");
			NUnit.Framework.Assert.IsFalse ((z1 > x).Value, "#A11");
			NUnit.Framework.Assert.IsFalse ((z1 > y).Value, "#A12");
		}

		[Test]
		public void GreaterThanOrEqualTest ()
		{
			SqlBoolean x = new SqlBoolean (-1);
			SqlBoolean y = new SqlBoolean (true);
			SqlBoolean z = new SqlBoolean ();
			SqlBoolean z1 = new SqlBoolean (0);

			NUnit.Framework.Assert.IsTrue ((x >= y).Value, "#A01");
			NUnit.Framework.Assert.AreEqual (x >= z, SqlBoolean.Null, "#A02");
			NUnit.Framework.Assert.IsTrue ((x >= z1).Value, "#A03");
			NUnit.Framework.Assert.AreEqual (y >= z, SqlBoolean.Null, "#A04");
			NUnit.Framework.Assert.IsTrue ((y >= x).Value, "#A05");
			NUnit.Framework.Assert.IsTrue ((y >= z1).Value, "#A06");
			NUnit.Framework.Assert.AreEqual (z >= z1, SqlBoolean.Null, "#A07");
			NUnit.Framework.Assert.AreEqual (z >= x, SqlBoolean.Null, "#A08");
			NUnit.Framework.Assert.AreEqual (z >= y, SqlBoolean.Null, "#A09");
			NUnit.Framework.Assert.AreEqual (z1 >= z, SqlBoolean.Null, "#A10");
			NUnit.Framework.Assert.IsFalse ((z1 >= x).Value, "#A11");
			NUnit.Framework.Assert.IsFalse ((z1 >= y).Value, "#A12");
		}

		[Test]
		public void LessThanTest ()
		{
			SqlBoolean x = new SqlBoolean (-1);
			SqlBoolean y = new SqlBoolean (true);
			SqlBoolean z = new SqlBoolean ();
			SqlBoolean z1 = new SqlBoolean (0);

			NUnit.Framework.Assert.IsFalse ((x < y).Value, "#A01");
			NUnit.Framework.Assert.AreEqual (x < z, SqlBoolean.Null, "#A02");
			NUnit.Framework.Assert.IsFalse ((x < z1).Value, "#A03");
			NUnit.Framework.Assert.AreEqual (y < z, SqlBoolean.Null, "#A04");
			NUnit.Framework.Assert.IsFalse ((y < x).Value, "#A05");
			NUnit.Framework.Assert.IsFalse ((y < z1).Value, "#A06");
			NUnit.Framework.Assert.AreEqual (z < z1, SqlBoolean.Null, "#A07");
			NUnit.Framework.Assert.AreEqual (z < x, SqlBoolean.Null, "#A08");
			NUnit.Framework.Assert.AreEqual (z < y, SqlBoolean.Null, "#A09");
			NUnit.Framework.Assert.AreEqual (z1 < z, SqlBoolean.Null, "#A10");
			NUnit.Framework.Assert.IsTrue ((z1 < x).Value, "#A11");
			NUnit.Framework.Assert.IsTrue ((z1 < y).Value, "#A12");
		}

		[Test]
		public void LessThanOrEqualTest ()
		{
			SqlBoolean x = new SqlBoolean (-1);
			SqlBoolean y = new SqlBoolean (true);
			SqlBoolean z = new SqlBoolean ();
			SqlBoolean z1 = new SqlBoolean (0);

			NUnit.Framework.Assert.IsTrue ((x <= y).Value, "#A01");
			NUnit.Framework.Assert.AreEqual (x <= z, SqlBoolean.Null, "#A02");
			NUnit.Framework.Assert.IsFalse ((x <= z1).Value, "#A03");
			NUnit.Framework.Assert.AreEqual (y <= z, SqlBoolean.Null, "#A04");
			NUnit.Framework.Assert.IsTrue ((y <= x).Value, "#A05");
			NUnit.Framework.Assert.IsFalse ((y <= z1).Value, "#A06");
			NUnit.Framework.Assert.AreEqual (z <= z1, SqlBoolean.Null, "#A07");
			NUnit.Framework.Assert.AreEqual (z <= x, SqlBoolean.Null, "#A08");
			NUnit.Framework.Assert.AreEqual (z <= y, SqlBoolean.Null, "#A09");
			NUnit.Framework.Assert.AreEqual (z1 <= z, SqlBoolean.Null, "#A10");
			NUnit.Framework.Assert.IsTrue ((z1 <= x).Value, "#A11");
			NUnit.Framework.Assert.IsTrue ((z1 <= y).Value, "#A12");
		}
#endif
	}
}

