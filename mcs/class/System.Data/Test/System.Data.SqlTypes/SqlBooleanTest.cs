// SqlDataTimeTest.cs - NUnit Test Cases for [explain here]
//
// Ville Palo (vi64pa@users.sourceforge.net)
//
// (C) Ville Palo
// 

using NUnit.Framework;
using System;
using System.Data.SqlTypes;

namespace MonoTests.System.Data.SqlTypes
{
  public class SqlBooleanTest : TestCase {

    private SqlBoolean SqlTrue;
    private SqlBoolean SqlFalse;

    public SqlBooleanTest() : base ("System.Data.SqlTypes.SqlBoolean") {}
    public SqlBooleanTest(string name) : base(name) {}

    protected override void TearDown() {}

    protected override void SetUp() {
      SqlTrue = new SqlBoolean(true);
      SqlFalse = new SqlBoolean(false);

    }

    public static ITest Suite {
      get {
         return new TestSuite(typeof(SqlBoolean));
      }
    }

    public void TestCreate ()
    {
      SqlBoolean SqlTrue2 = new SqlBoolean(1);
      SqlBoolean SqlFalse2 = new SqlBoolean(0);

      Assert("Creation of SqlBoolean failed", SqlTrue.Value);
      Assert("Creation of SqlBoolean failed", SqlTrue2.Value);
      Assert("Creation of SqlBoolean failed", !SqlFalse.Value);
      Assert("Creation of SqlBoolean failed", !SqlFalse2.Value);

    }

    ////
    // PUBLIC STATIC METHODS
    //

    // And
    public void TestAnd() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);

      // One result value
      SqlBoolean sqlResult;

      // true && false
      sqlResult = SqlBoolean.And(SqlTrue, SqlFalse);
      Assert("And method does not work correctly (true && false)", !sqlResult.Value);
      sqlResult = SqlBoolean.And(SqlFalse, SqlTrue);
      Assert("And method does not work correctly (false && true)", !sqlResult.Value);

      // true && true
      sqlResult = SqlBoolean.And(SqlTrue, SqlTrue2);
      Assert("And method does not work correctly (true && true)", sqlResult.Value);

      sqlResult = SqlBoolean.And(SqlTrue, SqlTrue);
      Assert("And method does not work correctly (true && true2)", sqlResult.Value);

      // false && false
      sqlResult = SqlBoolean.And(SqlFalse, SqlFalse2);
      Assert("And method does not work correctly (false && false)", !sqlResult.Value);
      sqlResult = SqlBoolean.And(SqlFalse, SqlFalse);
      Assert("And method does not work correctly (false && false2)", !sqlResult.Value);

    }

    // NotEquals
    public void TestNotEquals() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);

      SqlBoolean SqlResult;

      // true != false
      SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlFalse);
      Assert("NotEquals method does not work correctly (true != false)", SqlResult.Value);
      SqlResult = SqlBoolean.NotEquals(SqlFalse, SqlTrue);
      Assert("NotEquals method does not work correctly (false != true)", SqlResult.Value);


      // true != true
      SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlTrue);
      Assert("NotEquals method does not work correctly (true != true)", !SqlResult.Value);
      SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlTrue2);
      Assert("NotEquals method does not work correctly (true != true2)", !SqlResult.Value);
                 // false != false
      SqlResult = SqlBoolean.NotEquals(SqlFalse, SqlFalse);
      Assert("NotEquals method does not work correctly (false != false)", !SqlResult.Value);
      SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlTrue2);
      Assert("NotEquals method does not work correctly (false != false2)", !SqlResult.Value);

      // If either instance of SqlBoolean is null, the Value of the SqlBoolean will be Null.
      SqlResult = SqlBoolean.NotEquals(SqlBoolean.Null, SqlFalse);
      Assert("NotEquals method does not work correctly (Null != false)", SqlResult.IsNull);
      SqlResult = SqlBoolean.NotEquals(SqlTrue, SqlBoolean.Null);
      Assert("NotEquals method does not work correctly (false != Null)", SqlResult.IsNull);

    }

    // OnesComplement
    public void TestOnesComplement() {

      SqlBoolean SqlFalse2 = SqlBoolean.OnesComplement(SqlTrue);
      Assert("OnesComplement method does not work correctly", !SqlFalse2.Value);

      SqlBoolean SqlTrue2 = SqlBoolean.OnesComplement(SqlFalse);
      Assert("OnesComplement method does not work correctly", SqlTrue2.Value);

    }

    // Or
    public void TestOr() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);

      SqlBoolean SqlResult;

      // true || false
      SqlResult = SqlBoolean.Or(SqlTrue, SqlFalse);
      Assert("Or method does not work correctly (true || false)", SqlResult.Value);
      SqlResult = SqlBoolean.Or(SqlFalse, SqlTrue);
      Assert("Or method does not work correctly (false || true)", SqlResult.Value);

      // true || true
      SqlResult = SqlBoolean.Or(SqlTrue, SqlTrue);
      Assert("Or method does not work correctly (true || true)", SqlResult.Value);
      SqlResult = SqlBoolean.Or(SqlTrue, SqlTrue2);
      Assert("Or method does not work correctly (true || true2)", SqlResult.Value);

      // false || false
      SqlResult = SqlBoolean.Or(SqlFalse, SqlFalse);
      Assert("Or method does not work correctly (false || false)", !SqlResult.Value);
      SqlResult = SqlBoolean.Or(SqlFalse, SqlFalse2);
      Assert("Or method does not work correctly (false || false2)", !SqlResult.Value);

    }


    //  Parse
    public void TestParse() {

      String error = "Parse method does not work correctly ";
                                                                         
      Assert(error + "(\"True\")", SqlBoolean.Parse("True").Value);
      Assert(error + "(\" True\")", SqlBoolean.Parse(" True").Value);
      Assert(error + "(\"True \")", SqlBoolean.Parse("True ").Value);
      Assert(error + "(\"tRue\")", SqlBoolean.Parse("tRuE").Value);
      Assert(error + "(\"False\")", !SqlBoolean.Parse("False").Value);
      Assert(error + "(\" False\")", !SqlBoolean.Parse(" False").Value);
      Assert(error + "(\"False \")", !SqlBoolean.Parse("False ").Value);
      Assert(error + "(\"fAlSe\")", !SqlBoolean.Parse("fAlSe").Value);

    }

    // Xor
    public void TestXor() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);

      SqlBoolean SqlResult;

      // true ^ false
      SqlResult = SqlBoolean.Xor(SqlTrue, SqlFalse);
      Assert("Xor method does not work correctly (true ^ false)", SqlResult.Value);
      SqlResult = SqlBoolean.Xor(SqlFalse, SqlTrue);
      Assert("Xor method does not work correctly (false ^ true)", SqlResult.Value);

      // true ^ true
      SqlResult = SqlBoolean.Xor(SqlTrue, SqlTrue2);
      Assert("Xor method does not work correctly (true ^ true)", !SqlResult.Value);

      // false ^ false
      SqlResult = SqlBoolean.Xor(SqlFalse, SqlFalse2);
      Assert("Xor method does not work correctly (false ^ false)", !SqlResult.Value);

    }

    // static Equals
    public void TestStaticEquals() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);
      String error = "Static Equals method does not work correctly ";

      Assert(error +  "(true == true)", SqlBoolean.Equals(SqlTrue, SqlTrue2).Value);
      Assert(error +  "(false == false)", SqlBoolean.Equals(SqlFalse, SqlFalse2).Value);

      Assert(error +  "(true == false)", !SqlBoolean.Equals(SqlTrue, SqlFalse).Value);
      Assert(error +  "(false == true)", !SqlBoolean.Equals(SqlFalse, SqlTrue).Value);

      AssertEquals(error +  "(null == false)", SqlBoolean.Null, SqlBoolean.Equals(SqlBoolean.Null, SqlFalse));
      AssertEquals(error +  "(true == null)", SqlBoolean.Null, SqlBoolean.Equals(SqlTrue, SqlBoolean.Null));

    }

    //
    // END OF STATIC METHODS
    ////

    ////
    // PUBLIC METHODS
    //

    // CompareTo
    public void TestCompareTo() {

      String error = "CompareTo method does not work correctly";

      Assert(error, (SqlTrue.CompareTo(SqlBoolean.Null) > 0));
      Assert(error, (SqlTrue.CompareTo(SqlFalse) > 0));
      Assert(error, (SqlFalse.CompareTo(SqlTrue) < 0));
      Assert(error, (SqlFalse.CompareTo(SqlFalse) == 0));

    }

    // Equals
    public void TestEquals() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);

      String error = "Equals method does not work correctly ";

      Assert(error + "(true == true)", SqlTrue.Equals(SqlTrue2));
      Assert(error + "(false == false)", SqlFalse.Equals(SqlFalse2));

      Assert(error + "(true == false)", !SqlTrue.Equals(SqlFalse));
      Assert(error + "(false == true)", !SqlFalse.Equals(SqlTrue));

      Assert(error + "(true == false)", !SqlTrue.Equals(null));

    }

    public void TestGetHashCode() {

      AssertEquals("GetHashCode method does not work correctly",
          1, SqlTrue.GetHashCode());

      AssertEquals("GetHashCode method does not work correctly",
          0, SqlFalse.GetHashCode());

    }

    // GetType
    public void TestGetType() {

      AssertEquals("GetType method does not work correctly",
          SqlTrue.GetType().ToString(), "System.Data.SqlTypes.SqlBoolean");
    }

    // ToSqlByte
    public void TestToSqlByte() {

      SqlByte SqlTestByte;

      String error = "ToSqlByte method does not work correctly ";

      SqlTestByte = SqlTrue.ToSqlByte();
      AssertEquals(error, (byte)1,SqlTestByte.Value);

      SqlTestByte = SqlFalse.ToSqlByte();
      AssertEquals(error, (byte)0, SqlTestByte.Value);

    }

    // ToSqlDecimal
    public void TestToSqlDecimal() {

      SqlDecimal SqlTestDecimal;

      String error = "ToSqlDecimal method does not work correctly ";

      SqlTestDecimal = SqlTrue.ToSqlDecimal();

      AssertEquals(error, (decimal)1, SqlTestDecimal.Value);

      SqlTestDecimal = SqlFalse.ToSqlDecimal();
      AssertEquals(error, (decimal)0, SqlTestDecimal.Value);
    }

    // ToSqlDouble
    public void TestToSqlDouble() {

      SqlDouble SqlTestDouble;

      String error = "ToSqlDouble method does not work correctly ";

      SqlTestDouble = SqlTrue.ToSqlDouble();
      AssertEquals(error, (double)1, SqlTestDouble.Value);

      SqlTestDouble = SqlFalse.ToSqlDouble();
      AssertEquals(error, (double)0, SqlTestDouble.Value);
    }

    // ToSqlInt16
    public void TestToSqlInt16() {

      SqlInt16 SqlTestInt16;

      String error = "ToSqlInt16 method does not work correctly ";

      SqlTestInt16 = SqlTrue.ToSqlInt16();
      AssertEquals(error, (short)1, SqlTestInt16.Value);

      SqlTestInt16 = SqlFalse.ToSqlInt16();
      AssertEquals(error, (short)0, SqlTestInt16.Value);

    }

    // ToSqlInt32
    public void TestToSqlInt32() {

      SqlInt32 SqlTestInt32;

      String error = "ToSqlInt32 method does not work correctly ";

      SqlTestInt32 = SqlTrue.ToSqlInt32();
      AssertEquals(error, (int)1, SqlTestInt32.Value);

      SqlTestInt32 = SqlFalse.ToSqlInt32();
      AssertEquals(error, (int)0, SqlTestInt32.Value);

    }

    // ToSqlInt64
    public void TestToSqlInt64() {

      SqlInt64 SqlTestInt64;

      String error = "ToSqlInt64 method does not work correctly ";

      SqlTestInt64 = SqlTrue.ToSqlInt64();
      AssertEquals(error, (long)1, SqlTestInt64.Value);

      SqlTestInt64 = SqlFalse.ToSqlInt64();
      AssertEquals(error, (long)0, SqlTestInt64.Value);

    }

    // ToSqlMoney
    public void TestToSqlMoney() {

      SqlMoney SqlTestMoney;

      String error = "ToSqlMoney method does not work correctly ";

      SqlTestMoney = SqlTrue.ToSqlMoney();
      AssertEquals(error, (decimal)1, SqlTestMoney.Value);

      SqlTestMoney = SqlFalse.ToSqlMoney();
      AssertEquals(error, (decimal)0, SqlTestMoney.Value);

    }

    // ToSqlSingle
    public void TestToSqlsingle() {

      SqlSingle SqlTestSingle;

      String error = "ToSqlSingle method does not work correctly ";

      SqlTestSingle = SqlTrue.ToSqlSingle();
      AssertEquals(error, (float)1, SqlTestSingle.Value);

      SqlTestSingle = SqlFalse.ToSqlSingle();
      AssertEquals(error, (float)0, SqlTestSingle.Value);

    }

    // ToSqlString
    public void TestToSqlString() {

      SqlString SqlTestString;

      String error = "ToSqlString method does not work correctly ";

      SqlTestString = SqlTrue.ToSqlString();
      AssertEquals(error, "True", SqlTestString.Value);

      SqlTestString = SqlFalse.ToSqlString();
      AssertEquals(error, "False", SqlTestString.Value);

    }

    // ToString
    public void TestToString() {

      SqlString TestString;

      String error = "ToString method does not work correctly ";

      TestString = SqlTrue.ToString();
      AssertEquals(error, "True", TestString.Value);

      TestString = SqlFalse.ToSqlString();
      AssertEquals(error, "False", TestString.Value);

    }

    // END OF PUBLIC METHODS
    ////

    ////
    // OPERATORS

    // BitwixeAnd operator
    public void TestBitwiseAndOperator() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);

      SqlBoolean SqlResult;
      String error = "BitwiseAnd operator does not work correctly ";

      SqlResult = SqlTrue & SqlFalse;
      Assert(error + "(true & false)", !SqlResult.Value);
      SqlResult = SqlFalse & SqlTrue;
      Assert(error + "(false & true)", !SqlResult.Value);

      SqlResult = SqlTrue & SqlTrue2;
      Assert(error + "(true & true)", SqlResult.Value);

      SqlResult = SqlFalse & SqlFalse2;
      Assert(error + "(false & false)", !SqlResult.Value);


    }

    // BitwixeOr operator
    public void TestBitwiseOrOperator() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);

      SqlBoolean SqlResult;
      String error = "BitwiseOr operator does not work correctly ";

      SqlResult = SqlTrue | SqlFalse;
      Assert(error + "(true | false)", SqlResult.Value);
      SqlResult = SqlFalse | SqlTrue;

      Assert(error + "(false | true)", SqlResult.Value);

      SqlResult = SqlTrue | SqlTrue2;
      Assert(error + "(true | true)", SqlResult.Value);

      SqlResult = SqlFalse | SqlFalse2;
      Assert(error + "(false | false)", !SqlResult.Value);

    }

    // Equality operator
    public void TestEqualityOperator() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);

      SqlBoolean SqlResult;
      String error = "Equality operator does not work correctly ";

      SqlResult = SqlTrue == SqlFalse;
      Assert(error + "(true == false)", !SqlResult.Value);
      SqlResult = SqlFalse == SqlTrue;
      Assert(error + "(false == true)", !SqlResult.Value);

      SqlResult = SqlTrue == SqlTrue2;
      Assert(error + "(true == true)", SqlResult.Value);

      SqlResult = SqlFalse == SqlFalse2;
      Assert(error + "(false == false)", SqlResult.Value);

      SqlResult = SqlFalse == SqlBoolean.Null;
      Assert(error + "(false == Null)", SqlResult.IsNull);
      SqlResult = SqlBoolean.Null == SqlBoolean.Null;
      Assert(error + "(Null == true)", SqlResult.IsNull);

    }

    // ExlusiveOr operator
    public void TestExlusiveOrOperator() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);

      SqlBoolean SqlResult;
      String error = "ExclusiveOr operator does not work correctly ";

      SqlResult = SqlTrue ^ SqlFalse;
      Assert(error + "(true ^ false)", SqlResult.Value);
      SqlResult = SqlFalse | SqlTrue;
      Assert(error + "(false ^ true)", SqlResult.Value);

      SqlResult = SqlTrue ^ SqlTrue2;
      Assert(error + "(true ^ true)", !SqlResult.Value);

      SqlResult = SqlFalse ^ SqlFalse2;
      Assert(error + "(false ^ false)", !SqlResult.Value);

    }

    // false operator
    public void TestFalseOperator() {

      String error = "false operator does not work correctly ";

      AssertEquals(error + "(true)", SqlBoolean.False, (!SqlTrue));
      AssertEquals(error + "(false)", SqlBoolean.True, (!SqlFalse));

    }

    // Inequality operator
    public void TestInequalityOperator() {

      SqlBoolean SqlTrue2 = new SqlBoolean(true);
      SqlBoolean SqlFalse2 = new SqlBoolean(false);

      String error = "Inequality operator does not work correctly" ;

      AssertEquals(error + "(true != true)",   SqlBoolean.False, SqlTrue != SqlTrue);
      AssertEquals(error + "(true != true)",   SqlBoolean.False, SqlTrue != SqlTrue2);
      AssertEquals(error + "(false != false)", SqlBoolean.False, SqlFalse != SqlFalse);
      AssertEquals(error + "(false != false)", SqlBoolean.False, SqlFalse != SqlFalse2);
      AssertEquals(error + "(true != false)",  SqlBoolean.True, SqlTrue != SqlFalse);
      AssertEquals(error + "(false != true)",  SqlBoolean.True, SqlFalse != SqlTrue);
      AssertEquals(error + "(null != true)",   SqlBoolean.Null, SqlBoolean.Null != SqlTrue);
      AssertEquals(error + "(false != null)",  SqlBoolean.Null, SqlFalse != SqlBoolean.Null);

    }

    // Logical Not operator
    public void TestLogicalNotOperator() {

      String error = "Logical Not operator does not work correctly" ;

      AssertEquals(error + "(true)", SqlBoolean.False, !SqlTrue);
      AssertEquals(error + "(false)", SqlBoolean.True, !SqlFalse);

    }

    // OnesComplement operator
    public void TestOnesComplementOperator() {

      String error = "Ones complement operator does not work correctly" ;

      SqlBoolean SqlResult;

      SqlResult = ~SqlTrue;
      Assert(error + "(true)", !SqlResult.Value);
      SqlResult = ~SqlFalse;
      Assert(error + "(false)", SqlResult.Value);

    }


    // true operator
    public void TestTrueOperator() {

      String error = "true operator does not work correctly ";

      AssertEquals(error + "(true)", SqlBoolean.True, (SqlTrue));
      AssertEquals(error + "(false)", SqlBoolean.False, (SqlFalse));

    }

    // SqlBoolean to Boolean
    public void TestSqlBooleanToBoolean() {

      String error = "SqlBooleanToBoolean operator does not work correctly ";

      Boolean TestBoolean = (Boolean)SqlTrue;
      Assert(error + "(true)",  TestBoolean);
      TestBoolean = (Boolean)SqlFalse;
      Assert(error + "(false)",  !TestBoolean);

    }

    // SqlByte to SqlBoolean
    public void TestSqlByteToSqlBoolean() {

      SqlByte SqlTestByte;
      SqlBoolean SqlTestBoolean;
      String error = "SqlByteToSqlBoolean operator does not work correctly ";

      SqlTestByte = new SqlByte(1);
      SqlTestBoolean = (SqlBoolean)SqlTestByte;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTestByte = new SqlByte(2);
      SqlTestBoolean = (SqlBoolean)SqlTestByte;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTestByte = new SqlByte(0);
      SqlTestBoolean = (SqlBoolean)SqlTestByte;
      Assert(error + "(false)", !SqlTestBoolean.Value);

    }

    // SqlDecimal to SqlBoolean
    public void TestSqlDecimalToSqlBoolean() {

      SqlDecimal SqlTest;
      SqlBoolean SqlTestBoolean;
      String error = "SqlDecimalToSqlBoolean operator does not work correctly ";

      SqlTest = new SqlDecimal(1);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlDecimal(19);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlDecimal(0);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(false)", !SqlTestBoolean.Value);

    }

    // SqlDouble to SqlBoolean
    public void TestSqlDoubleToSqlBoolean() {

      SqlDouble SqlTest;
      SqlBoolean SqlTestBoolean;
      String error = "SqlDoubleToSqlBoolean operator does not work correctly ";

      SqlTest = new SqlDouble(1);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlDouble(-19.8);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlDouble(0);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(false)", !SqlTestBoolean.Value);

    }

    // SqlIn16 to SqlBoolean
    public void TestSqlInt16ToSqlBoolean() {

      SqlInt16 SqlTest;
      SqlBoolean SqlTestBoolean;
      String error = "SqlInt16ToSqlBoolean operator does not work correctly ";

      SqlTest = new SqlInt16(1);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlInt16(-143);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlInt16(0);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(false)", !SqlTestBoolean.Value);

    }

    // SqlInt32 to SqlBoolean
    public void TestSqlInt32ToSqlBoolean() {

      SqlInt32 SqlTest;
      SqlBoolean SqlTestBoolean;
      String error = "SqlInt32ToSqlBoolean operator does not work correctly ";

      SqlTest = new SqlInt32(1);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlInt32(1430);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlInt32(0);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(false)", !SqlTestBoolean.Value);
    }

    // SqlInt64 to SqlBoolean
    public void TestSqlInt64ToSqlBoolean() {

      SqlInt64 SqlTest;
      SqlBoolean SqlTestBoolean;
      String error = "SqlInt64ToSqlBoolean operator does not work correctly ";

      SqlTest = new SqlInt64(1);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlInt64(-14305);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlInt64(0);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(false)", !SqlTestBoolean.Value);

    }

    // SqlMoney to SqlBoolean
    public void TestSqlMoneyToSqlBoolean() {

      SqlMoney SqlTest;
      SqlBoolean SqlTestBoolean;
      String error = "SqlMoneyToSqlBoolean operator does not work correctly ";

      SqlTest = new SqlMoney(1);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlMoney(1305);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlMoney(0);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(false)", !SqlTestBoolean.Value);

    }

    // SqlSingle to SqlBoolean
    public void TestSqlSingleToSqlBoolean() {

      SqlSingle SqlTest;
      SqlBoolean SqlTestBoolean;
      String error = "SqlSingleToSqlBoolean operator does not work correctly ";

      SqlTest = new SqlSingle(1);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlSingle(1305);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlSingle(-305.3);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlSingle(0);
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(false)", !SqlTestBoolean.Value);

    }

    // SqlString to SqlBoolean
    public void TestSqlStringToSqlBoolean() {

      SqlString SqlTest;
      SqlBoolean SqlTestBoolean;
      String error = "SqlSingleToSqlBoolean operator does not work correctly ";

      SqlTest = new SqlString("true");
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlString("TRUE");
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlString("True");
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);

      SqlTest = new SqlString("false");
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(false)", !SqlTestBoolean.Value);

    }

    // Boolean to SqlBoolean
    public void BooleanToSqlBoolean() {

      SqlBoolean SqlTestBoolean;
      bool btrue = true;
      bool bfalse = false;
      String error = "BooleanToSqlBoolean operator does not work correctly ";

      Boolean SqlTest = true;
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(true)", SqlTestBoolean.Value);
      SqlTestBoolean = (SqlBoolean)btrue;
      Assert(error + "(true)", SqlTestBoolean.Value);


      SqlTest = false;
      SqlTestBoolean = (SqlBoolean)SqlTest;
      Assert(error + "(false)", !SqlTestBoolean.Value);
      SqlTestBoolean = (SqlBoolean)bfalse;
      Assert(error + "(false)", !SqlTestBoolean.Value);

    }

    // END OF OPERATORS
    ////

    ////
    // PROPERTIES

    // ByteValue property
    public void TestByteValueProperty() {

      String error = "ByteValue property does not work correctly ";

      AssertEquals(error + "(true)", (byte)1, SqlTrue.ByteValue);
      AssertEquals(error + "(false)", (byte)0, SqlFalse.ByteValue);

    }

    // IsFalse property
    public void TestIsFalseProperty() {

      String error = "IsFalse property does not work correctly ";

      Assert(error + "(true)", !SqlTrue.IsFalse);
      Assert(error + "(false)", SqlFalse.IsFalse);

    }

    // IsNull property
    public void TestIsNullProperty() {

      String error = "IsNull property does not work correctly ";

      Assert(error + "(true)", !SqlTrue.IsNull);
      Assert(error + "(false)", !SqlFalse.IsNull);
      Assert(error + "(Null)", SqlBoolean.Null.IsNull);

    }

    // IsTrue property
    public void TestIsTrueProperty() {

      String error = "IsTrue property does not work correctly ";

      Assert(error + "(true)", SqlTrue.IsTrue);
      Assert(error + "(false)", !SqlFalse.IsTrue);

    }

    // Value property
    public void TestValueProperty() {

      String error = "Value property does not work correctly ";

      Assert(error + "(true)", SqlTrue.Value);
      Assert(error + "(false)", !SqlFalse.Value);

    }

    // END OF PROPERTIEs
    ////

    ////
    // FIELDS

    public void TestFalseField() {

      Assert("False field does not work correctly",
        !SqlBoolean.False.Value);

    }

    public void TestNullField() {

      Assert("Null field does not work correctly",
        SqlBoolean.Null.IsNull);

    }

    public void TestOneField() {

      AssertEquals("One field does not work correctly",
        (byte)1, SqlBoolean.One.ByteValue);
    }


    public void TestTrueField() {

      Assert("True field does not work correctly",
        SqlBoolean.True.Value);

    }

    public void TestZeroField() {

      AssertEquals("Zero field does not work correctly",
        (byte)0, SqlBoolean.Zero.ByteValue);

    }
  }
}

