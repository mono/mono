// System.Data.SqlTypes.SqlInt32
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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

namespace System.Data.SqlTypes
{

    using System;

    public struct SqlInt32 : INullable, IComparable
    {

        private int _value;
        private bool _isNull;

        public static readonly SqlInt32 Null = new SqlInt32(true);
        public static readonly SqlInt32 MaxValue = new SqlInt32(int.MaxValue);
        public static readonly SqlInt32 MinValue = new SqlInt32(int.MinValue);
        public static readonly SqlInt32 Zero = new SqlInt32(0);

        

        private SqlInt32(bool isNull)
        {
            _isNull = isNull;
            _value = 0;
        }
        /**
         * Constructor
         * @param value A int whose value will be used for the new SqlInt32.
         */
        public SqlInt32(int value) 
        {
            _value = value;
            _isNull = false;
        }

        

        /**
         * Indicates whether or not Value is null.
         * @return true if Value is null, otherwise false.
         */
        public bool IsNull
        {
            get
            {
                return _isNull;
            }
        }



        /**
         * Gets the value of the SqlInt32 instance.
         * @return the value of this instance
         */
        public int Value
        {
            get
            {
                if(IsNull)
                    throw new SqlNullValueException();
                return _value;
            }
        }

        /**
         * Compares this instance to the supplied object and returns an indication of their relative values.
         * @param obj The object to compare.
         * @return A signed number indicating the relative values of the instance and the object.
         * Less than zero This instance is less than object.
         * Zero This instance is the same as object.
         * Greater than zero This instance is greater than object -or-
         * object is a null reference.
         */
        public int CompareTo(Object obj)
        {
            if (obj == null)
                return 1;

            if (obj is SqlInt32)
            {
                SqlInt32 i = (SqlInt32)obj;

                if (i.IsNull)
                    return 1;
                if (this.IsNull)
                    return -1;

                return this._value.CompareTo(i._value);
            }

            throw new ArgumentException("parameter obj is not SqlInt32 : " + obj.GetType().Name);

        }

        /**
         * The addition operator computes the sum of the two SqlInt32 operands.
         * @param x A SqlInt32 structure.
         * @param y A SqlInt32 structure.
         * @return The sum of the two SqlInt32 operands.
         * If one of the parameters is null or null value - return SqlInt32.Null.
         */
        public static SqlInt32 Add(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt32.Null;

            int xVal = x._value;
            int yVal = y._value;

            int sum  = checked(xVal + yVal);

            return new SqlInt32(sum);
        }

        /**
         * Computes the bitwise AND of its SqlInt32 operands.
         * @param x A SqlInt32 instance.
         * @param y A SqlInt32 instance.
         * @return The results of the bitwise AND operation.
         */
        public static SqlInt32 BitwiseAnd(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt32.Null;

            int res  = x._value & y._value;

            return new SqlInt32(res);
        }

        /**
         * Computes the bitwise OR of its SqlInt32 operands.
         * @param x A SqlInt32 instance.
         * @param y A SqlInt32 instance.
         * @return The results of the bitwise OR operation.
         */
        public static SqlInt32 BitwiseOr(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt32.Null;

            int res  = x._value | y._value;

            return new SqlInt32(res);
        }

        /**
         * The division operator divides the first SqlInt32 operand by the second.
         * @param x A SqlInt32 instance.
         * @param y A SqlInt32 instance.
         * @return A SqlInt32 instance containing the results of the division operation.
         * If one of the parameters is null or null value - return SqlInt32.Null.
         */
        public static SqlInt32 Divide(SqlInt32 x, SqlInt32 y)
        {
            int val = x._value / y._value;
            return new SqlInt32(val);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (obj is SqlInt32)
            {
                SqlInt32 i = (SqlInt32)obj;

                if (IsNull && i.IsNull)
                    return true;

                if (IsNull || i.IsNull)
                    return false;

                return _value == i._value;
            }

            return false;
        }


        /**
         * Performs a logical comparison on two instances of SqlInt32 to determine if they are equal.
         * @param x A SqlInt32 instance.
         * @param y A SqlInt32 instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value == y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Returns the hash code for this SqlInt32 instance.
         * @return A signed integer hash code.
         */
        public override int GetHashCode()
        {
            return _value;
        }

        /**
         * Compares two instances of SqlByte to determine if the first is greater than the second.
         * @param x A SqlByte instance
         * @param y A SqlByte instance
         * @return A SqlBoolean that is True if the first instance is greater than the second instance, otherwise False.
         * If either instance of SqlInt32 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThan(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value > y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlInt32 to determine if the first is greater than or equal to the second.
         * @param x A SqlInt32 instance
         * @param y A SqlInt32 instance
         * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
         * If either instance of SqlInt32 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThanOrEqual(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value >= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlInt32 to determine if the first is less than the second.
         * @param x A SqlInt32 instance
         * @param y A SqlInt32 instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlInt32 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThan(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value < y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlInt32 to determine if the first is less than or equal to the second.
         * @param x A SqlInt32 instance
         * @param y A SqlInt32 instance
         * @return A SqlBoolean that is True if the first instance is less than or equal to the second instance, otherwise False.
         * If either instance of SqlInt32 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThanOrEqual(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value <= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Computes the remainder after dividing its first SqlInt32 operand by its second.
         * @param x A SqlInt32 instance
         * @param y A SqlInt32 instance
         * @return A SqlInt32 instance whose Value contains the remainder.
         */
        public static SqlInt32 Mod(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt32.Null;

            int mod = x._value % y._value;

            return new SqlInt32(mod);
        }

        /**
         * The multiplication operator computes the product of the two SqlInt32 operands.
         * @param x A SqlInt32 instance
         * @param y A SqlInt32 instance
         * @return The product of the two SqlInt32 operands.
         */
        public static SqlInt32 Multiply(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt32.Null;

            int xVal = x._value;
            int yVal = y._value;

            int res = checked(xVal * yVal);

            return new SqlInt32(res);
        }

        /**
         * Compares two instances of SqlInt32 to determine if they are equal.
         * @param x A SqlInt32 instance
         * @param y A SqlInt32 instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlInt32 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value != y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * The ones complement operator performs a bitwise one's complement operation on its SqlInt32 operand.
         * @param x A SqlInt32 instance
         * @return A SqlInt32 instance whose Value property contains the ones complement of the SqlInt32 parameter.
         */
        public static SqlInt32 OnesComplement(SqlInt32 x)
        {
            int res  = (int)(x._value ^ 0xFFFFFFFF);

            return new SqlInt32(res);
        }

        /**
         * Converts the String representation of a number to its byte equivalent.
         * @param s The String to be parsed.
         * @return A SqlInt32 containing the value represented by the String.
         */
        public static SqlInt32 Parse(String s)
        {
            int res = int.Parse(s);

            return new SqlInt32(res);
        }

        /**
         * The subtraction operator the second SqlInt32 operand from the first.
         * @param x A SqlInt32 instance
         * @param y A SqlInt32 instance
         * @return The results of the subtraction operation.
         */
        public static SqlInt32 Subtract(SqlInt32 x, SqlInt32 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt32.Null;

            int res = checked(x._value - y._value);
            return new SqlInt32(res);
        }

        /**
         * Performs a bitwise exclusive-OR operation on the supplied parameters.
         * @param x A SqlInt32 instance
         * @param y A SqlInt32 instance
         * @return The results of the XOR operation.
         */
        public static SqlInt32 Xor(SqlInt32 x, SqlInt32 y)
        {
            int res  = x._value ^ y._value;

            return new SqlInt32(res);
        }

        /**
         * Converts this SqlInt32 structure to SqlBoolean.
         * @return A SqlBoolean structure whose Value will be True if the SqlInt32 structure's Value is non-zero, False if the SqlInt32 is zero
         * and Null if the SqlInt32 structure is Null.
         */
        public SqlBoolean ToSqlBoolean()
        {
            if (IsNull)
                return SqlBoolean.Null;

            if (_value == 0)
                return new SqlBoolean(0);

            return new SqlBoolean(1);
        }

        /**
         * Converts this SqlInt32 structure to SqlByte.
         * @return A SqlByte structure whose Value equals the Value of this SqlInt32 structure.
         */
        public SqlByte ToSqlByte()
        {
            if (IsNull)
                return SqlByte.Null;

            if (_value < 0 || _value > 255)
                throw new OverflowException("Can not onvert this instance to SqlByte - overflowing : " + _value);

            return new SqlByte((byte)_value);
        }

        /**
         * Converts this SqlInt32 structure to SqlDecimal.
         * @return A SqlDecimal structure whose Value equals the Value of this SqlInt32 structure.
         */
        public SqlDecimal ToSqlDecimal()
        {
            if (IsNull)
                return SqlDecimal.Null;

            return new SqlDecimal(_value);
        }

        /**
         * Converts this SqlInt32 structure to SqlDecimal.
         * @return A SqlDouble structure whose Value equals the Value of this SqlInt32 structure.
         */
        public SqlDouble ToSqlDouble()
        {
            if (IsNull)
                return SqlDouble.Null;

            return new SqlDouble(_value);
        }

        /**
         * Converts this SqlInt32 structure to SqlInt16.
         * @return A SqlInt16 structure whose Value equals the Value of this SqlInt32 structure.
         */
        public SqlInt16 ToSqlInt16()
        {
            if (IsNull)
                return SqlInt16.Null;

            return new SqlInt16(_value);
        }

        /**
         * Converts this SqlInt32 structure to SqlInt64.
         * @return A SqlInt64 structure whose Value equals the Value of this SqlInt32 structure.
         */
        public SqlInt64 ToSqlInt64()
        {
            if (IsNull)
                return SqlInt64.Null;

            return new SqlInt64(_value);
        }

        /**
         * Converts this SqlInt32 instance to SqlDouble.
         * @return A SqlMoney instance whose Value equals the Value of this SqlInt32 instance.
         */
        public SqlMoney ToSqlMoney()
        {
            if (IsNull)
                return SqlMoney.Null;

            return new SqlMoney(_value);
        }

        /**
         * Converts this SqlInt32 instance to SqlSingle.
         * @return A SqlSingle instance whose Value equals the Value of this SqlInt32 instance.
         */
        public SqlSingle ToSqlSingle()
        {
            if (IsNull)
                return SqlSingle.Null;

            return new SqlSingle((float)_value);
        }

        /**
         * Converts this SqlInt32 structure to SqlString.
         * @return A SqlString structure whose value is a string representing the date and time contained in this SqlInt32 structure.
         */
        public SqlString ToSqlString()
        {
            return new SqlString(ToString());
        }



        public override String ToString()
        {
            if (IsNull)
                return "null";
            return _value.ToString();
        }

        // Compute Addition
        public static SqlInt32 operator + (SqlInt32 x, SqlInt32 y) 
        {
            checked 
            {
                return new SqlInt32 (x.Value + y.Value);
            }
        }

        // Bitwise AND
        public static SqlInt32 operator & (SqlInt32 x, SqlInt32 y) 
        {
            return new SqlInt32 (x.Value & y.Value);
        }

        // Bitwise OR
        public static SqlInt32 operator | (SqlInt32 x, SqlInt32 y) 
        {
            checked 
            {
                return new SqlInt32 (x.Value | y.Value);
            }
        }

        // Compute Division
        public static SqlInt32 operator / (SqlInt32 x, SqlInt32 y) 
        {
            checked 
            {
                return new SqlInt32 (x.Value / y.Value);
            }
        }

        // Compare Equality
        public static SqlBoolean operator == (SqlInt32 x, SqlInt32 y) 
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value == y.Value);
        }

        // Bitwise Exclusive-OR (XOR)
        public static SqlInt32 operator ^ (SqlInt32 x, SqlInt32 y) 
        {
            return new SqlInt32 (x.Value ^ y.Value);
        }

        // > Compare
        public static SqlBoolean operator >(SqlInt32 x, SqlInt32 y) 
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value > y.Value);
        }

        // >= Compare
        public static SqlBoolean operator >= (SqlInt32 x, SqlInt32 y) 
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value >= y.Value);
        }

        // != Inequality Compare
        public static SqlBoolean operator != (SqlInt32 x, SqlInt32 y) 
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value != y.Value);
        }
		
        // < Compare
        public static SqlBoolean operator < (SqlInt32 x, SqlInt32 y) 
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value < y.Value);
        }

        // <= Compare
        public static SqlBoolean operator <= (SqlInt32 x, SqlInt32 y) 
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value <= y.Value);
        }

        // Compute Modulus
        public static SqlInt32 operator % (SqlInt32 x, SqlInt32 y) 
        {
            return new SqlInt32 (x.Value % y.Value);
        }

        // Compute Multiplication
        public static SqlInt32 operator * (SqlInt32 x, SqlInt32 y) 
        {
            checked 
            {
                return new SqlInt32 (x.Value * y.Value);
            }
        }

        // Ones Complement
        public static SqlInt32 operator ~ (SqlInt32 x) 
        {
            return new SqlInt32 (~x.Value);
        }

        // Subtraction
        public static SqlInt32 operator - (SqlInt32 x, SqlInt32 y) 
        {
            checked 
            {
                return new SqlInt32 (x.Value - y.Value);
            }
        }

        // Negates the Value
        public static SqlInt32 operator - (SqlInt32 x) 
        {
            return new SqlInt32 (-x.Value);
        }

        // Type Conversions
        public static explicit operator SqlInt32 (SqlBoolean x) 
        {
            if (x.IsNull) 
                return Null;
            else 
                return new SqlInt32 ((int)x.ByteValue);
        }

        public static explicit operator SqlInt32 (SqlDecimal x) 
        {
            checked 
            {
                if (x.IsNull) 
                    return Null;
                else 
                    return new SqlInt32 ((int)x.Value);
            }
        }

        public static explicit operator SqlInt32 (SqlDouble x) 
        {
            checked 
            {
                if (x.IsNull) 
                    return Null;
                else 
                    return new SqlInt32 ((int)x.Value);
            }
        }

        public static explicit operator int (SqlInt32 x)
        {
            return x.Value;
        }

        public static explicit operator SqlInt32 (SqlInt64 x) 
        {
            checked 
            {
                if (x.IsNull) 
                    return Null;
                else 
                    return new SqlInt32 ((int)x.Value);
            }
        }

        public static explicit operator SqlInt32(SqlMoney x) 
        {
            checked 
            {
                if (x.IsNull) 
                    return Null;
                else 
                    return new SqlInt32 ((int)x.Value);
            }
        }

        public static explicit operator SqlInt32(SqlSingle x) 
        {
            checked 
            {
                if (x.IsNull) 
                    return Null;
                else 
                    return new SqlInt32 ((int)x.Value);
            }
        }

        public static explicit operator SqlInt32(SqlString x) 
        {
            checked 
            {
                return SqlInt32.Parse (x.Value);
            }
        }

        public static implicit operator SqlInt32(int x) 
        {
            return new SqlInt32 (x);
        }

        public static implicit operator SqlInt32(SqlByte x) 
        {
            if (x.IsNull) 
                return Null;
            else 
                return new SqlInt32 ((int)x.Value);
        }

        public static implicit operator SqlInt32(SqlInt16 x) 
        {
            if (x.IsNull) 
                return Null;
            else 
                return new SqlInt32 ((int)x.Value);
        }


    }
}