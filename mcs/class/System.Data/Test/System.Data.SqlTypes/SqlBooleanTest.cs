// SqlDataTimeTest.cs - NUnit Test Cases for [explain here]
//
// Authors:
//   Ville Palo (vi64pa@users.sourceforge.net)
//   Martin Willemoes Hansen
//
// (C) Ville Palo
// 

using NUnit.Framework;
using System;
using System.Data.SqlTypes;

namespace MonoTests.System.Data.SqlTypes
{

	[TestFixture]
        public class SqlBooleanTest {
		private SqlBoolean SqlTrue;
		private SqlBoolean SqlFalse;

		[SetUp]
		public void GetReady() {
			SqlTrue = new SqlBoolean(true);
			SqlFalse = new SqlBoolean(false);

		}

		[Test]
		public void Create ()
			{
				SqlBoolean SqlTrue2 = new SqlBoolean(1);
				SqlBoolean SqlFalse2 = new SqlBoolean(0);

				Assertion.Assert("Creation of SqlBoolean failed", SqlTrue.Value);
				Assertion.Assert("Creation of SqlBoolean failed", SqlTrue2.Value);
				Assertion.Assert("Creation of SqlBoolean failed", !SqlFalse.Value);
				Assertion.Assert("Creation of SqlBoolean failed", !SqlFalse2.Value);

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
			Assertion.Assert("And method does not work correctly (true && false)", !sqlResult.Value);
			sqlResult = SqlBoolean.And(SqlFalse, SqlTrue);
			Assertion.Assert("And method does not work correctly (false && true)", !sqlResult.Value);

			// true && true
			sqlResult = SqlBoolean.And(SqlTrue, SqlTrue2);
			Assertion.Assert("And method does not work correctly (true && true)", sqlResult.Value);

			sqlResult = SqlBoolean.And(SqlTrue, SqlTrue);
			Assertion.Assert("And method does not work correctly (true && true2)", sqlResult.Value);

			// false && false
			sqlResult = SqlBoolean.And(SqlFalse, SqlFalse2);
			Assertion.Assert("And method does not work correctly (false && false)", !sqlResult.Value);
			sqlResult = SqlBoolean.And(SqlFalse, SqlFalse);
			Assertion.Assert("And method does not work correctly (false && false2)", !sqlResult.Value);

		}

		// NotEquals
		[Test]
		public void NotEquals() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;

			// true != false
			SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlFalse);
			Assertion.Assert("NotEquals method does not work correctly (true != false)", SqlResult.Value);
			SqlResult = SqlBoolean.NotEquals(SqlFalse, SqlTrue);
			Assertion.Assert("NotEquals method does not work correctly (false != true)", SqlResult.Value);


			// true != true
			SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlTrue);
			Assertion.Assert("NotEquals method does not work correctly (true != true)", !SqlResult.Value);
			SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlTrue2);
			Assertion.Assert("NotEquals method does not work correctly (true != true2)", !SqlResult.Value);
			// false != false
			SqlResult = SqlBoolean.NotEquals(SqlFalse, SqlFalse);
			Assertion.Assert("NotEquals method does not work correctly (false != false)", !SqlResult.Value);
			SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlTrue2);
			Assertion.Assert("NotEquals method does not work correctly (false != false2)", !SqlResult.Value);

			// If either instance of SqlBoolean is null, the Value of the SqlBoolean will be Null.
			SqlResult = SqlBoolean.NotEquals(SqlBoolean.Null, SqlFalse);
			Assertion.Assert("NotEquals method does not work correctly (Null != false)", SqlResult.IsNull);
			SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlBoolean.Null);
			Assertion.Assert("NotEquals method does not work correctly (false != Null)", SqlResult.IsNull);

		}

		// OnesComplement
		[Test]
		public void OnesComplement() {

			SqlBoolean SqlFalse2 = SqlBoolean.OnesComplement(SqlTrue);
			Assertion.Assert("OnesComplement method does not work correctly", !SqlFalse2.Value);

			SqlBoolean SqlTrue2 = SqlBoolean.OnesComplement(SqlFalse);
			Assertion.Assert("OnesComplement method does not work correctly", SqlTrue2.Value);

		}

		// Or
		[Test]
		public void Or() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;

			// true || false
			SqlResult = SqlBoolean.Or(SqlTrue, SqlFalse);
			Assertion.Assert("Or method does not work correctly (true || false)", SqlResult.Value);
			SqlResult = SqlBoolean.Or(SqlFalse, SqlTrue);
			Assertion.Assert("Or method does not work correctly (false || true)", SqlResult.Value);

			// true || true
			SqlResult = SqlBoolean.Or(SqlTrue, SqlTrue);
			Assertion.Assert("Or method does not work correctly (true || true)", SqlResult.Value);
			SqlResult = SqlBoolean.Or(SqlTrue, SqlTrue2);
			Assertion.Assert("Or method does not work correctly (true || true2)", SqlResult.Value);

			// false || false
			SqlResult = SqlBoolean.Or(SqlFalse, SqlFalse);
			Assertion.Assert("Or method does not work correctly (false || false)", !SqlResult.Value);
			SqlResult = SqlBoolean.Or(SqlFalse, SqlFalse2);
			Assertion.Assert("Or method does not work correctly (false || false2)", !SqlResult.Value);

		}


		//  Parse
		[Test]
		public void Parse() {

			String error = "Parse method does not work correctly ";
                                                                         
			Assertion.Assert(error + "(\"True\")", SqlBoolean.Parse("True").Value);
			Assertion.Assert(error + "(\" True\")", SqlBoolean.Parse(" True").Value);
			Assertion.Assert(error + "(\"True \")", SqlBoolean.Parse("True ").Value);
			Assertion.Assert(error + "(\"tRue\")", SqlBoolean.Parse("tRuE").Value);
			Assertion.Assert(error + "(\"False\")", !SqlBoolean.Parse("False").Value);
			Assertion.Assert(error + "(\" False\")", !SqlBoolean.Parse(" False").Value);
			Assertion.Assert(error + "(\"False \")", !SqlBoolean.Parse("False ").Value);
			Assertion.Assert(error + "(\"fAlSe\")", !SqlBoolean.Parse("fAlSe").Value);

		}

		// Xor
		[Test]
		public void Xor() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;

			// true ^ false
			SqlResult = SqlBoolean.Xor(SqlTrue, SqlFalse);
			Assertion.Assert("Xor method does not work correctly (true ^ false)", SqlResult.Value);
			SqlResult = SqlBoolean.Xor(SqlFalse, SqlTrue);
			Assertion.Assert("Xor method does not work correctly (false ^ true)", SqlResult.Value);

			// true ^ true
			SqlResult = SqlBoolean.Xor(SqlTrue, SqlTrue2);
			Assertion.Assert("Xor method does not work correctly (true ^ true)", !SqlResult.Value);

			// false ^ false
			SqlResult = SqlBoolean.Xor(SqlFalse, SqlFalse2);
			Assertion.Assert("Xor method does not work correctly (false ^ false)", !SqlResult.Value);

		}

		// static Equals
		[Test]
		public void StaticEquals() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);
			String error = "Static Equals method does not work correctly ";

			Assertion.Assert(error +  "(true == true)", SqlBoolean.Equals(SqlTrue, SqlTrue2).Value);
			Assertion.Assert(error +  "(false == false)", SqlBoolean.Equals(SqlFalse, SqlFalse2).Value);

			Assertion.Assert(error +  "(true == false)", !SqlBoolean.Equals(SqlTrue, SqlFalse).Value);
			Assertion.Assert(error +  "(false == true)", !SqlBoolean.Equals(SqlFalse, SqlTrue).Value);

			Assertion.AssertEquals(error +  "(null == false)", SqlBoolean.Null, SqlBoolean.Equals(SqlBoolean.Null, SqlFalse));
			Assertion.AssertEquals(error +  "(true == null)", SqlBoolean.Null, SqlBoolean.Equals(SqlTrue, SqlBoolean.Null));

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

			Assertion.Assert(error, (SqlTrue.CompareTo(SqlBoolean.Null) > 0));
			Assertion.Assert(error, (SqlTrue.CompareTo(SqlFalse) > 0));
			Assertion.Assert(error, (SqlFalse.CompareTo(SqlTrue) < 0));
			Assertion.Assert(error, (SqlFalse.CompareTo(SqlFalse) == 0));

		}

		// Equals
		[Test]
		public void Equals() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			String error = "Equals method does not work correctly ";

			Assertion.Assert(error + "(true == true)", SqlTrue.Equals(SqlTrue2));
			Assertion.Assert(error + "(false == false)", SqlFalse.Equals(SqlFalse2));

			Assertion.Assert(error + "(true == false)", !SqlTrue.Equals(SqlFalse));
			Assertion.Assert(error + "(false == true)", !SqlFalse.Equals(SqlTrue));

			Assertion.Assert(error + "(true == false)", !SqlTrue.Equals(null));

		}

		[Test]
		public void GetHashCodeTest() {

			Assertion.AssertEquals("GetHashCode method does not work correctly",
				     1, SqlTrue.GetHashCode());

			Assertion.AssertEquals("GetHashCode method does not work correctly",
				     0, SqlFalse.GetHashCode());

		}

		// GetType
		[Test]
		public void GetTypeTest() {

			Assertion.AssertEquals("GetType method does not work correctly",
				     SqlTrue.GetType().ToString(), "System.Data.SqlTypes.SqlBoolean");
		}

		// ToSqlByte
		[Test]
		public void ToSqlByte() {

			SqlByte SqlTestByte;

			String error = "ToSqlByte method does not work correctly ";

			SqlTestByte = SqlTrue.ToSqlByte();
			Assertion.AssertEquals(error, (byte)1,SqlTestByte.Value);

			SqlTestByte = SqlFalse.ToSqlByte();
			Assertion.AssertEquals(error, (byte)0, SqlTestByte.Value);

		}

		// ToSqlDecimal
		[Test]
		public void ToSqlDecimal() {

			SqlDecimal SqlTestDecimal;

			String error = "ToSqlDecimal method does not work correctly ";

			SqlTestDecimal = SqlTrue.ToSqlDecimal();

			Assertion.AssertEquals(error, (decimal)1, SqlTestDecimal.Value);

			SqlTestDecimal = SqlFalse.ToSqlDecimal();
			Assertion.AssertEquals(error, (decimal)0, SqlTestDecimal.Value);
		}

		// ToSqlDouble
		[Test]
		public void ToSqlDouble() {

			SqlDouble SqlTestDouble;

			String error = "ToSqlDouble method does not work correctly ";

			SqlTestDouble = SqlTrue.ToSqlDouble();
			Assertion.AssertEquals(error, (double)1, SqlTestDouble.Value);

			SqlTestDouble = SqlFalse.ToSqlDouble();
			Assertion.AssertEquals(error, (double)0, SqlTestDouble.Value);
		}

		// ToSqlInt16
		[Test]
		public void ToSqlInt16() {

			SqlInt16 SqlTestInt16;

			String error = "ToSqlInt16 method does not work correctly ";

			SqlTestInt16 = SqlTrue.ToSqlInt16();
			Assertion.AssertEquals(error, (short)1, SqlTestInt16.Value);

			SqlTestInt16 = SqlFalse.ToSqlInt16();
			Assertion.AssertEquals(error, (short)0, SqlTestInt16.Value);

		}

		// ToSqlInt32
		[Test]
		public void ToSqlInt32() {

			SqlInt32 SqlTestInt32;

			String error = "ToSqlInt32 method does not work correctly ";

			SqlTestInt32 = SqlTrue.ToSqlInt32();
			Assertion.AssertEquals(error, (int)1, SqlTestInt32.Value);

			SqlTestInt32 = SqlFalse.ToSqlInt32();
			Assertion.AssertEquals(error, (int)0, SqlTestInt32.Value);

		}

		// ToSqlInt64
		[Test]
		public void ToSqlInt64() {

			SqlInt64 SqlTestInt64;

			String error = "ToSqlInt64 method does not work correctly ";

			SqlTestInt64 = SqlTrue.ToSqlInt64();
			Assertion.AssertEquals(error, (long)1, SqlTestInt64.Value);

			SqlTestInt64 = SqlFalse.ToSqlInt64();
			Assertion.AssertEquals(error, (long)0, SqlTestInt64.Value);

		}

		// ToSqlMoney
		[Test]
		public void ToSqlMoney() {

			SqlMoney SqlTestMoney;

			String error = "ToSqlMoney method does not work correctly ";

			SqlTestMoney = SqlTrue.ToSqlMoney();
			Assertion.AssertEquals(error, (decimal)1, SqlTestMoney.Value);

			SqlTestMoney = SqlFalse.ToSqlMoney();
			Assertion.AssertEquals(error, (decimal)0, SqlTestMoney.Value);

		}

		// ToSqlSingle
		[Test]
		public void ToSqlsingle() {

			SqlSingle SqlTestSingle;

			String error = "ToSqlSingle method does not work correctly ";

			SqlTestSingle = SqlTrue.ToSqlSingle();
			Assertion.AssertEquals(error, (float)1, SqlTestSingle.Value);

			SqlTestSingle = SqlFalse.ToSqlSingle();
			Assertion.AssertEquals(error, (float)0, SqlTestSingle.Value);

		}

		// ToSqlString
		[Test]
		public void ToSqlString() {

			SqlString SqlTestString;

			String error = "ToSqlString method does not work correctly ";

			SqlTestString = SqlTrue.ToSqlString();
			Assertion.AssertEquals(error, "True", SqlTestString.Value);

			SqlTestString = SqlFalse.ToSqlString();
			Assertion.AssertEquals(error, "False", SqlTestString.Value);

		}

		// ToString
		[Test]
		public void ToStringTest() {

			SqlString TestString;

			String error = "ToString method does not work correctly ";

			TestString = SqlTrue.ToString();
			Assertion.AssertEquals(error, "True", TestString.Value);

			TestString = SqlFalse.ToSqlString();
			Assertion.AssertEquals(error, "False", TestString.Value);

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
			Assertion.Assert(error + "(true & false)", !SqlResult.Value);
			SqlResult = SqlFalse & SqlTrue;
			Assertion.Assert(error + "(false & true)", !SqlResult.Value);

			SqlResult = SqlTrue & SqlTrue2;
			Assertion.Assert(error + "(true & true)", SqlResult.Value);

			SqlResult = SqlFalse & SqlFalse2;
			Assertion.Assert(error + "(false & false)", !SqlResult.Value);


		}

		// BitwixeOr operator
		[Test]
		public void BitwiseOrOperator() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;
			String error = "BitwiseOr operator does not work correctly ";

			SqlResult = SqlTrue | SqlFalse;
			Assertion.Assert(error + "(true | false)", SqlResult.Value);
			SqlResult = SqlFalse | SqlTrue;

			Assertion.Assert(error + "(false | true)", SqlResult.Value);

			SqlResult = SqlTrue | SqlTrue2;
			Assertion.Assert(error + "(true | true)", SqlResult.Value);

			SqlResult = SqlFalse | SqlFalse2;
			Assertion.Assert(error + "(false | false)", !SqlResult.Value);

		}

		// Equality operator
		[Test]
		public void EqualityOperator() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;
			String error = "Equality operator does not work correctly ";

			SqlResult = SqlTrue == SqlFalse;
			Assertion.Assert(error + "(true == false)", !SqlResult.Value);
			SqlResult = SqlFalse == SqlTrue;
			Assertion.Assert(error + "(false == true)", !SqlResult.Value);

			SqlResult = SqlTrue == SqlTrue2;
			Assertion.Assert(error + "(true == true)", SqlResult.Value);

			SqlResult = SqlFalse == SqlFalse2;
			Assertion.Assert(error + "(false == false)", SqlResult.Value);

			SqlResult = SqlFalse == SqlBoolean.Null;
			Assertion.Assert(error + "(false == Null)", SqlResult.IsNull);
			SqlResult = SqlBoolean.Null == SqlBoolean.Null;
			Assertion.Assert(error + "(Null == true)", SqlResult.IsNull);

		}

		// ExlusiveOr operator
		[Test]
		public void ExlusiveOrOperator() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			SqlBoolean SqlResult;
			String error = "ExclusiveOr operator does not work correctly ";

			SqlResult = SqlTrue ^ SqlFalse;
			Assertion.Assert(error + "(true ^ false)", SqlResult.Value);
			SqlResult = SqlFalse | SqlTrue;
			Assertion.Assert(error + "(false ^ true)", SqlResult.Value);

			SqlResult = SqlTrue ^ SqlTrue2;
			Assertion.Assert(error + "(true ^ true)", !SqlResult.Value);

			SqlResult = SqlFalse ^ SqlFalse2;
			Assertion.Assert(error + "(false ^ false)", !SqlResult.Value);

		}

		// false operator
		[Test]
		public void FalseOperator() {

			String error = "false operator does not work correctly ";

			Assertion.AssertEquals(error + "(true)", SqlBoolean.False, (!SqlTrue));
			Assertion.AssertEquals(error + "(false)", SqlBoolean.True, (!SqlFalse));

		}

		// Inequality operator
		[Test]
		public void InequalityOperator() {

			SqlBoolean SqlTrue2 = new SqlBoolean(true);
			SqlBoolean SqlFalse2 = new SqlBoolean(false);

			String error = "Inequality operator does not work correctly" ;

			Assertion.AssertEquals(error + "(true != true)",   SqlBoolean.False, SqlTrue != SqlTrue);
			Assertion.AssertEquals(error + "(true != true)",   SqlBoolean.False, SqlTrue != SqlTrue2);
			Assertion.AssertEquals(error + "(false != false)", SqlBoolean.False, SqlFalse != SqlFalse);
			Assertion.AssertEquals(error + "(false != false)", SqlBoolean.False, SqlFalse != SqlFalse2);
			Assertion.AssertEquals(error + "(true != false)",  SqlBoolean.True, SqlTrue != SqlFalse);
			Assertion.AssertEquals(error + "(false != true)",  SqlBoolean.True, SqlFalse != SqlTrue);
			Assertion.AssertEquals(error + "(null != true)",   SqlBoolean.Null, SqlBoolean.Null != SqlTrue);
			Assertion.AssertEquals(error + "(false != null)",  SqlBoolean.Null, SqlFalse != SqlBoolean.Null);

		}

		// Logical Not operator
		[Test]
		public void LogicalNotOperator() {

			String error = "Logical Not operator does not work correctly" ;

			Assertion.AssertEquals(error + "(true)", SqlBoolean.False, !SqlTrue);
			Assertion.AssertEquals(error + "(false)", SqlBoolean.True, !SqlFalse);

		}

		// OnesComplement operator
		[Test]
		public void OnesComplementOperator() {

			String error = "Ones complement operator does not work correctly" ;

			SqlBoolean SqlResult;

			SqlResult = ~SqlTrue;
			Assertion.Assert(error + "(true)", !SqlResult.Value);
			SqlResult = ~SqlFalse;
			Assertion.Assert(error + "(false)", SqlResult.Value);

		}


		// true operator
		[Test]
		public void TrueOperator() {

			String error = "true operator does not work correctly ";

			Assertion.AssertEquals(error + "(true)", SqlBoolean.True, (SqlTrue));
			Assertion.AssertEquals(error + "(false)", SqlBoolean.False, (SqlFalse));

		}

		// SqlBoolean to Boolean
		[Test]
		public void SqlBooleanToBoolean() {

			String error = "SqlBooleanToBoolean operator does not work correctly ";

			Boolean TestBoolean = (Boolean)SqlTrue;
			Assertion.Assert(error + "(true)",  TestBoolean);
			TestBoolean = (Boolean)SqlFalse;
			Assertion.Assert(error + "(false)",  !TestBoolean);

		}

		// SqlByte to SqlBoolean
		[Test]
		public void SqlByteToSqlBoolean() {

			SqlByte SqlTestByte;
			SqlBoolean SqlTestBoolean;
			String error = "SqlByteToSqlBoolean operator does not work correctly ";

			SqlTestByte = new SqlByte(1);
			SqlTestBoolean = (SqlBoolean)SqlTestByte;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTestByte = new SqlByte(2);
			SqlTestBoolean = (SqlBoolean)SqlTestByte;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTestByte = new SqlByte(0);
			SqlTestBoolean = (SqlBoolean)SqlTestByte;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);

		}

		// SqlDecimal to SqlBoolean
		[Test]
		public void SqlDecimalToSqlBoolean() {

			SqlDecimal SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlDecimalToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlDecimal(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlDecimal(19);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlDecimal(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);

		}

		// SqlDouble to SqlBoolean
		[Test]
		public void SqlDoubleToSqlBoolean() {

			SqlDouble SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlDoubleToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlDouble(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlDouble(-19.8);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlDouble(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);

		}

		// SqlIn16 to SqlBoolean
		[Test]
		public void SqlInt16ToSqlBoolean() {

			SqlInt16 SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlInt16ToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlInt16(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlInt16(-143);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlInt16(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);

		}

		// SqlInt32 to SqlBoolean
		[Test]
		public void SqlInt32ToSqlBoolean() {

			SqlInt32 SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlInt32ToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlInt32(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlInt32(1430);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlInt32(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);
		}

		// SqlInt64 to SqlBoolean
		[Test]
		public void SqlInt64ToSqlBoolean() {

			SqlInt64 SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlInt64ToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlInt64(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlInt64(-14305);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlInt64(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);

		}

		// SqlMoney to SqlBoolean
		[Test]
		public void SqlMoneyToSqlBoolean() {

			SqlMoney SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlMoneyToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlMoney(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlMoney(1305);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlMoney(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);

		}

		// SqlSingle to SqlBoolean
		[Test]
		public void SqlSingleToSqlBoolean() {

			SqlSingle SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlSingleToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlSingle(1);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlSingle(1305);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlSingle(-305.3);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlSingle(0);
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);

		}

		// SqlString to SqlBoolean
		[Test]
		public void SqlStringToSqlBoolean() {

			SqlString SqlTest;
			SqlBoolean SqlTestBoolean;
			String error = "SqlSingleToSqlBoolean operator does not work correctly ";

			SqlTest = new SqlString("true");
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlString("TRUE");
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlString("True");
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);

			SqlTest = new SqlString("false");
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);

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
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);
			SqlTestBoolean = (SqlBoolean)btrue;
			Assertion.Assert(error + "(true)", SqlTestBoolean.Value);


			SqlTest = false;
			SqlTestBoolean = (SqlBoolean)SqlTest;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);
			SqlTestBoolean = (SqlBoolean)bfalse;
			Assertion.Assert(error + "(false)", !SqlTestBoolean.Value);

		}

		// END OF OPERATORS
		////

		////
		// PROPERTIES

		// ByteValue property
		[Test]
		public void ByteValueProperty() {

			String error = "ByteValue property does not work correctly ";

			Assertion.AssertEquals(error + "(true)", (byte)1, SqlTrue.ByteValue);
			Assertion.AssertEquals(error + "(false)", (byte)0, SqlFalse.ByteValue);

		}

		// IsFalse property
		[Test]
		public void IsFalseProperty() {

			String error = "IsFalse property does not work correctly ";

			Assertion.Assert(error + "(true)", !SqlTrue.IsFalse);
			Assertion.Assert(error + "(false)", SqlFalse.IsFalse);

		}

		// IsNull property
		[Test]
		public void IsNullProperty() {

			String error = "IsNull property does not work correctly ";

			Assertion.Assert(error + "(true)", !SqlTrue.IsNull);
			Assertion.Assert(error + "(false)", !SqlFalse.IsNull);
			Assertion.Assert(error + "(Null)", SqlBoolean.Null.IsNull);

		}

		// IsTrue property
		[Test]
		public void IsTrueProperty() {

			String error = "IsTrue property does not work correctly ";

			Assertion.Assert(error + "(true)", SqlTrue.IsTrue);
			Assertion.Assert(error + "(false)", !SqlFalse.IsTrue);

		}

		// Value property
		[Test]
		public void ValueProperty() {

			String error = "Value property does not work correctly ";

			Assertion.Assert(error + "(true)", SqlTrue.Value);
			Assertion.Assert(error + "(false)", !SqlFalse.Value);

		}

		// END OF PROPERTIEs
		////

		////
		// FIELDS

		[Test]
		public void FalseField() {

			Assertion.Assert("False field does not work correctly",
			       !SqlBoolean.False.Value);

		}

		[Test]
		public void NullField() {

			Assertion.Assert("Null field does not work correctly",
			       SqlBoolean.Null.IsNull);

		}

		[Test]
		public void OneField() {

			Assertion.AssertEquals("One field does not work correctly",
				     (byte)1, SqlBoolean.One.ByteValue);
		}

		[Test]
		public void TrueField() {

			Assertion.Assert("True field does not work correctly",
			       SqlBoolean.True.Value);

		}

		[Test]
		public void ZeroField() {

			Assertion.AssertEquals("Zero field does not work correctly",
				     (byte)0, SqlBoolean.Zero.ByteValue);

		}
	}
}

