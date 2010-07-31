// System.Data.SqlTypes.SqlInt64
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

    public struct SqlInt64 : INullable, IComparable
    {

        private long _value;
        private bool _isNull;
        
        public static readonly SqlInt64 Null = new SqlInt64(true);
        public static readonly SqlInt64 MaxValue = new SqlInt64(int.MaxValue);
        public static readonly SqlInt64 MinValue = new SqlInt64(int.MinValue);
        public static readonly SqlInt64 Zero = new SqlInt64(0);

        
        private SqlInt64(bool isNull)
        {
            _value = 0;
            _isNull = isNull;
        }
        /**
         * Constructor
         * @param value A long whose value will be used for the new SqlInt64.
         */
        public SqlInt64(long value) 
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
         * Gets the value of the SqlInt64 instance.
         * @return the value of this instance
         */
        public long Value
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

            if (obj is SqlInt64)
            {
                SqlInt64 i = (SqlInt64)obj;

                if (i.IsNull)
                    return 1;
                if (this.IsNull)
                    return -1;

                return this._value.CompareTo(i._value);
            }

            throw new ArgumentException("parameter obj is not SqlInt64 : " + obj.GetType().Name);

        }

        /**
         * The addition operator computes the sum of the two SqlInt64 operands.
         * @param x A SqlInt64 structure.
         * @param y A SqlInt64 structure.
         * @return The sum of the two SqlInt64 operands.
         * If one of the parameters is null or null value - return SqlInt64.Null.
         */
        public static SqlInt64 Add(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt64.Null;

            
            long sum  = checked(x._value + y._value);

            return new SqlInt64(sum);
        }

        /**
         * Computes the bitwise AND of its SqlInt64 operands.
         * @param x A SqlInt64 instance.
         * @param y A SqlInt64 instance.
         * @return The results of the bitwise AND operation.
         */
        public static SqlInt64 BitwiseAnd(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt64.Null;

            long res  = x._value & y._value;

            return new SqlInt64(res);
        }

        /**
         * Computes the bitwise OR of its SqlInt64 operands.
         * @param x A SqlInt64 instance.
         * @param y A SqlInt64 instance.
         * @return The results of the bitwise OR operation.
         */
        public static SqlInt64 BitwiseOr(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt64.Null;

            long res  = x._value | y._value;

            return new SqlInt64(res);
        }

        /**
         * The division operator divides the first SqlInt64 operand by the second.
         * @param x A SqlInt64 instance.
         * @param y A SqlInt64 instance.
         * @return A SqlInt64 instance containing the results of the division operation.
         * If one of the parameters is null or null value - return SqlInt64.Null.
         */
        public static SqlInt64 Divide(SqlInt64 x, SqlInt64 y)
        {
            long val = x._value / y._value;
            return new SqlInt64(val);
        }

        public override int GetHashCode()
        {
            return (int) (_value ^ (_value >> 32));
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (obj is SqlInt64)
            {
                SqlInt64 i = (SqlInt64)obj;

                if (IsNull && i.IsNull)
                    return true;

                if (IsNull || i.IsNull)
                    return false;

                return _value == i._value;
            }

            return false;
        }

        

        /**
         * Performs a logical comparison on two instances of SqlInt64 to determine if they are equal.
         * @param x A SqlInt64 instance.
         * @param y A SqlInt64 instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value == y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        
        /**
         * Compares two instances of SqlByte to determine if the first is greater than the second.
         * @param x A SqlByte instance
         * @param y A SqlByte instance
         * @return A SqlBoolean that is True if the first instance is greater than the second instance, otherwise False.
         * If either instance of SqlInt64 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThan(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value > y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlInt64 to determine if the first is greater than or equal to the second.
         * @param x A SqlInt64 instance
         * @param y A SqlInt64 instance
         * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
         * If either instance of SqlInt64 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThanOrEqual(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value >= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlInt64 to determine if the first is less than the second.
         * @param x A SqlInt64 instance
         * @param y A SqlInt64 instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlInt64 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThan(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value < y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlInt64 to determine if the first is less than or equal to the second.
         * @param x A SqlInt64 instance
         * @param y A SqlInt64 instance
         * @return A SqlBoolean that is True if the first instance is less than or equal to the second instance, otherwise False.
         * If either instance of SqlInt64 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThanOrEqual(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value <= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Computes the remainder after dividing its first SqlInt64 operand by its second.
         * @param x A SqlInt64 instance
         * @param y A SqlInt64 instance
         * @return A SqlInt64 instance whose Value contains the remainder.
         */
        public static SqlInt64 Mod(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt64.Null;

            long mod = x._value % y._value;

            return new SqlInt64(mod);
        }

        /**
         * The multiplication operator computes the product of the two SqlInt64 operands.
         * @param x A SqlInt64 instance
         * @param y A SqlInt64 instance
         * @return The product of the two SqlInt64 operands.
         */
        public static SqlInt64 Multiply(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt64.Null;

            long xVal = x._value;
            long yVal = y._value;

            long res = checked(xVal * yVal);

            return new SqlInt64(res);
        }

        /**
         * Compares two instances of SqlInt64 to determine if they are equal.
         * @param x A SqlInt64 instance
         * @param y A SqlInt64 instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlInt64 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value != y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * The ones complement operator performs a bitwise one's complement operation on its SqlInt64 operand.
         * @param x A SqlInt64 instance
         * @return A SqlInt64 instance whose Value property contains the ones complement of the SqlInt64 parameter.
         */
        public static SqlInt64 OnesComplement(SqlInt64 x)
        {
            ulong res  = (ulong)x._value ^ 0xFFFFFFFFFFFFFFFF;

            return new SqlInt64((long)res);
        }

        /**
         * Converts the String representation of a number to its byte equivalent.
         * @param s The String to be parsed.
         * @return A SqlInt64 containing the value represented by the String.
         */
        public static SqlInt64 Parse(String s)
        {
            long res = long.Parse(s);

            return new SqlInt64(res);
        }

        /**
         * The subtraction operator the second SqlInt64 operand from the first.
         * @param x A SqlInt64 instance
         * @param y A SqlInt64 instance
         * @return The results of the subtraction operation.
         */
        public static SqlInt64 Subtract(SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt64.Null;

            long xVal = x._value;
            long yVal = y._value;

            
            long res = checked(x._value - y._value);
            return new SqlInt64(res);
        }

        /**
         * Performs a bitwise exclusive-OR operation on the supplied parameters.
         * @param x A SqlInt64 instance
         * @param y A SqlInt64 instance
         * @return The results of the XOR operation.
         */
        public static SqlInt64 Xor(SqlInt64 x, SqlInt64 y)
        {
            long res  = x._value ^ y._value;

            return new SqlInt64(res);
        }

        /**
         * Converts this SqlInt64 structure to SqlBoolean.
         * @return A SqlBoolean structure whose Value will be True if the SqlInt64 structure's Value is non-zero, False if the SqlInt64 is zero
         * and Null if the SqlInt64 structure is Null.
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
         * Converts this SqlInt64 structure to SqlByte.
         * @return A SqlByte structure whose Value equals the Value of this SqlInt64 structure.
         */
        public SqlByte ToSqlByte()
        {
            if (IsNull)
                return SqlByte.Null;

            if (_value < 0 || _value > 255)
                throw new OverflowException("Can not convert this instance to SqlByte - overflowing : " + _value);

            return new SqlByte((byte)_value);
        }

        /**
         * Converts this SqlInt64 structure to SqlDecimal.
         * @return A SqlDecimal structure whose Value equals the Value of this SqlInt64 structure.
         */
        public SqlDecimal ToSqlDecimal()
        {
            if (IsNull)
                return SqlDecimal.Null;

            return new SqlDecimal(_value);
        }

        /**
         * Converts this SqlInt64 structure to SqlDecimal.
         * @return A SqlDouble structure whose Value equals the Value of this SqlInt64 structure.
         */
        public SqlDouble ToSqlDouble()
        {
            if (IsNull)
                return SqlDouble.Null;

            return new SqlDouble((double)_value);
        }

        /**
         * Converts this SqlInt64 structure to SqlInt16.
         * @return A SqlInt16 structure whose Value equals the Value of this SqlInt64 structure.
         */
        public SqlInt16 ToSqlInt16()
        {
            if (IsNull)
                return SqlInt16.Null;
            
            if (_value > short.MaxValue || _value < short.MinValue)
                throw new OverflowException("overflow - can not convert this SqlInt64 to SqlInt16 : " + _value);

            return new SqlInt16((short)_value);
        }

        /**
         * Converts this SqlInt64 structure to SqlInt32.
         * @return A SqlInt32 structure whose Value equals the Value of this SqlInt64 structure.
         */
        public SqlInt32 ToSqlInt32()
        {
            if (IsNull)
                return SqlInt32.Null;

            if (_value > int.MaxValue || _value < int.MinValue)
                throw new OverflowException("overflow - can not convert this SqlInt64 to SqlInt16 : " + _value);

            return new SqlInt32((int)_value);
        }

        /**
         * Converts this SqlInt64 instance to SqlDouble.
         * @return A SqlMoney instance whose Value equals the Value of this SqlInt64 instance.
         */
        public SqlMoney ToSqlMoney()
        {
            if (IsNull)
                return SqlMoney.Null;

            return new SqlMoney(_value);
        }

        /**
         * Converts this SqlIn64 instance to SqlSingle.
         * @return A SqlSingle instance whose Value equals the Value of this SqlInt64 instance.
         */
        public SqlSingle ToSqlSingle()
        {
            if (IsNull)
                return SqlSingle.Null;
            
            return new SqlSingle((float)_value);
        }



        /**
         * Converts this SqlInt64 structure to SqlString.
         * @return A SqlString structure whose value is a string representing the date and time contained in this SqlInt64 structure.
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

        public static SqlInt64 operator + (SqlInt64 x, SqlInt64 y)
        {
            checked 
            {
                return new SqlInt64 (x.Value + y.Value);
            }
        }

        public static SqlInt64 operator & (SqlInt64 x, SqlInt64 y)
        {
            return new SqlInt64 (x.Value & y.Value);
        }

        public static SqlInt64 operator | (SqlInt64 x, SqlInt64 y)
        {
            return new SqlInt64 (x.Value | y.Value);
        }

        public static SqlInt64 operator / (SqlInt64 x, SqlInt64 y)
        {
            checked 
            {
                return new SqlInt64 (x.Value / y.Value);
            }
        }

        public static SqlBoolean operator == (SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value == y.Value);
        }

        public static SqlInt64 operator ^ (SqlInt64 x, SqlInt64 y)
        {
            return new SqlInt64 (x.Value ^ y.Value);
        }

        public static SqlBoolean operator > (SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value > y.Value);
        }

        public static SqlBoolean operator >= (SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value >= y.Value);
        }

        public static SqlBoolean operator != (SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (!(x.Value == y.Value));
        }

        public static SqlBoolean operator < (SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value < y.Value);
        }

        public static SqlBoolean operator <= (SqlInt64 x, SqlInt64 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value <= y.Value);
        }

        public static SqlInt64 operator % (SqlInt64 x, SqlInt64 y)
        {
            return new SqlInt64(x.Value % y.Value);
        }

        public static SqlInt64 operator * (SqlInt64 x, SqlInt64 y)
        {
            checked 
            {
                return new SqlInt64 (x.Value * y.Value);
            }
        }

        public static SqlInt64 operator ~ (SqlInt64 x)
        {
            if (x.IsNull)
                return SqlInt64.Null;

            return new SqlInt64 (~(x.Value));
        }

        public static SqlInt64 operator - (SqlInt64 x, SqlInt64 y)
        {
            checked 
            {
                return new SqlInt64 (x.Value - y.Value);
            }
        }

        public static SqlInt64 operator - (SqlInt64 n)
        {
            return new SqlInt64 (-(n.Value));
        }

        public static explicit operator SqlInt64 (SqlBoolean x)
        {
            if (x.IsNull) 
                return SqlInt64.Null;
            else
                return new SqlInt64 ((long)x.ByteValue);
        }

        public static explicit operator SqlInt64 (SqlDecimal x)
        {
            checked 
            {
                if (x.IsNull) 
                    return SqlInt64.Null;
                else
                    return new SqlInt64 ((long)x.Value);
            }
        }

        public static explicit operator SqlInt64 (SqlDouble x)
        {
            if (x.IsNull) 
                return SqlInt64.Null;
            else 
            {
                checked 
                {
                    return new SqlInt64 ((long)x.Value);
                }
            }
        }

        public static explicit operator long (SqlInt64 x)
        {
            return x.Value;
        }

        public static explicit operator SqlInt64 (SqlMoney x)
        {
            checked 
            {
                if (x.IsNull) 
                    return SqlInt64.Null;
                else
                    return new SqlInt64 ((long)x.Value);
            }
        }

        public static explicit operator SqlInt64 (SqlSingle x)
        {
            if (x.IsNull) 
                return SqlInt64.Null;
            else 
            {
                checked 
                {
                    return new SqlInt64 ((long)x.Value);
                }
            }
        }

        public static explicit operator SqlInt64 (SqlString x)
        {
            checked 
            {
                return SqlInt64.Parse (x.Value);
            }
        }

        public static implicit operator SqlInt64 (long x)
        {
            return new SqlInt64 (x);
        }

        public static implicit operator SqlInt64 (SqlByte x)
        {
            if (x.IsNull) 
                return SqlInt64.Null;
            else
                return new SqlInt64 ((long)x.Value);
        }

        public static implicit operator SqlInt64 (SqlInt16 x)
        {
            if (x.IsNull) 
                return SqlInt64.Null;
            else
                return new SqlInt64 ((long)x.Value);
        }

        public static implicit operator SqlInt64 (SqlInt32 x)
        {
            if (x.IsNull) 
                return SqlInt64.Null;
            else
                return new SqlInt64 ((long)x.Value);
        }

    }}