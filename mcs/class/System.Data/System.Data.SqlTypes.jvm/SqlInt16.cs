// System.Data.SqlTypes.SqlInt16
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

    public struct SqlInt16 : INullable, IComparable
    {

        private short _value;
        private bool _isNull;

        public static readonly SqlInt16 Null = new SqlInt16(true);
        public static readonly SqlInt16 MaxValue = new SqlInt16(short.MaxValue);
        public static readonly SqlInt16 MinValue = new SqlInt16(short.MinValue);
        public static readonly SqlInt16 Zero = new SqlInt16(0);

        
        private SqlInt16(bool isNull)
        {
            _value = 0;
            _isNull = isNull;
        }
        /**
         * Constructor
         * @param value A short whose value will be used for the new SqlInt16.
         */
        public SqlInt16(short value) 
        {
            _value = value;
            _isNull = false;
        }

        /**
         * Constructor
         * @param value A int whose value will be used for the new SqlInt16.
         */
        public SqlInt16(int value) 
        {
            if (value > short.MaxValue || value < short.MinValue)
                throw new OverflowException("the value is not legal - overflowing : " + value);

            _value = (short)value;
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
         * Gets the value of the SqlInt16 instance.
         * @return the value of this instance
         */
        public short Value
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

            if (obj is SqlInt16)
            {
                SqlInt16 i = (SqlInt16)obj;

                if (i.IsNull)
                    return 1;
                if (this.IsNull)
                    return -1;

                return this._value.CompareTo(i._value);
            }

            throw new ArgumentException("parameter obj is not SqlInt16 : " + obj.GetType().Name);

        }

        /**
         * The addition operator computes the sum of the two SqlInt16 operands.
         * @param x A SqlInt16 structure.
         * @param y A SqlInt16 structure.
         * @return The sum of the two SqlInt16 operands.
         * If one of the parameters is null or null value - return SqlInt16.Null.
         */
        public static SqlInt16 Add(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt16.Null;

            int sum  = checked(x._value + y._value);

            return new SqlInt16(sum);
        }

        /**
         * Computes the bitwise AND of its SqlInt16 operands.
         * @param x A SqlInt16 instance.
         * @param y A SqlInt16 instance.
         * @return The results of the bitwise AND operation.
         */
        public static SqlInt16 BitwiseAnd(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt16.Null;

            int res  = x._value & y._value;

            return new SqlInt16(res);
        }

        /**
         * Computes the bitwise OR of its SqlInt16 operands.
         * @param x A SqlInt16 instance.
         * @param y A SqlInt16 instance.
         * @return The results of the bitwise OR operation.
         */
        public static SqlInt16 BitwiseOr(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt16.Null;

            int res  = (ushort)x._value | (ushort)y._value;

            return new SqlInt16(res);
        }

        /**
         * The division operator divides the first SqlInt16 operand by the second.
         * @param x A SqlInt16 instance.
         * @param y A SqlInt16 instance.
         * @return A SqlInt16 instance containing the results of the division operation.
         * If one of the parameters is null or null value - return SqlInt16.Null.
         */
        public static SqlInt16 Divide(SqlInt16 x, SqlInt16 y)
        {
            int val = x._value / y._value;
            return new SqlInt16(val);
        }

        public override bool Equals(Object obj)
        {
            
            if (obj == null)
                return false;

            if (obj is SqlInt16)
            {
                SqlInt16 i = (SqlInt16)obj;

                if (IsNull && i.IsNull)
                    return true;

                if (IsNull || i.IsNull)
                    return false;

                return _value == i._value;
            }

            return false;
        }

        

        /**
         * Performs a logical comparison on two instances of SqlInt16 to determine if they are equal.
         * @param x A SqlInt16 instance.
         * @param y A SqlInt16 instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlInt16 x, SqlInt16 y)
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
         * If either instance of SqlInt16 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThan(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value > y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlInt16 to determine if the first is greater than or equal to the second.
         * @param x A SqlInt16 instance
         * @param y A SqlInt16 instance
         * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
         * If either instance of SqlInt16 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThanOrEqual(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value >= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlInt16 to determine if the first is less than the second.
         * @param x A SqlInt16 instance
         * @param y A SqlInt16 instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlInt16 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThan(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value < y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlInt16 to determine if the first is less than or equal to the second.
         * @param x A SqlInt16 instance
         * @param y A SqlInt16 instance
         * @return A SqlBoolean that is True if the first instance is less than or equal to the second instance, otherwise False.
         * If either instance of SqlInt16 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThanOrEqual(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value <= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Computes the remainder after dividing its first SqlInt16 operand by its second.
         * @param x A SqlInt16 instance
         * @param y A SqlInt16 instance
         * @return A SqlInt16 instance whose Value contains the remainder.
         */
        public static SqlInt16 Mod(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt16.Null;

            int mod = x._value % y._value;

            return new SqlInt16(mod);
        }

        /**
         * The multiplication operator computes the product of the two SqlInt16 operands.
         * @param x A SqlInt16 instance
         * @param y A SqlInt16 instance
         * @return The product of the two SqlInt16 operands.
         */
        public static SqlInt16 Multiply(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlInt16.Null;

            int res = checked(x._value * y._value);

            return new SqlInt16(res);
        }

        /**
         * Compares two instances of SqlInt16 to determine if they are equal.
         * @param x A SqlInt16 instance
         * @param y A SqlInt16 instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlInt16 is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value != y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * The ones complement operator performs a bitwise one's complement operation on its SqlInt16 operand.
         * @param x A SqlInt16 instance
         * @return A SqlInt16 instance whose Value property contains the ones complement of the SqlInt16 parameter.
         */
        public static SqlInt16 OnesComplement(SqlInt16 x)
        {
            int res  = x._value ^ 0xFFFF;

            return new SqlInt16(res);
        }

        /**
         * Converts the String representation of a number to its byte equivalent.
         * @param s The String to be parsed.
         * @return A SqlInt16 containing the value represented by the String.
         */
        public static SqlInt16 Parse(String s)
        {
            int res = short.Parse(s);
            return new SqlInt16(res);
        }

        /**
         * The subtraction operator the second SqlInt16 operand from the first.
         * @param x A SqlInt16 instance
         * @param y A SqlInt16 instance
         * @return The results of the subtraction operation.
         */
        public static SqlInt16 Subtract(SqlInt16 x, SqlInt16 y)
        {
            int res = x._value - y._value;

            return new SqlInt16(res);
        }

        /**
         * Performs a bitwise exclusive-OR operation on the supplied parameters.
         * @param x A SqlInt16 instance
         * @param y A SqlInt16 instance
         * @return The results of the XOR operation.
         */
        public static SqlInt16 Xor(SqlInt16 x, SqlInt16 y)
        {
            int res  = x._value ^ y._value;

            return new SqlInt16(res);
        }

        /**
         * Converts this SqlInt16 structure to SqlBoolean.
         * @return A SqlBoolean structure whose Value will be True if the SqlInt16 structure's Value is non-zero, False if the SqlInt16 is zero
         * and Null if the SqlInt16 structure is Null.
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
         * Converts this SqlInt16 structure to SqlByte.
         * @return A SqlByte structure whose Value equals the Value of this SqlInt16 structure.
         */
        public SqlByte ToSqlByte()
        {
            if (IsNull)
                return SqlByte.Null;

            int val = _value;

            if (val < 0 || val > 255)
                throw new OverflowException("Can not conver this instance to SqlByte - overflowing : " + val);

            return new SqlByte((byte)val);
        }

        /**
         * Converts this SqlInt16 structure to SqlDecimal.
         * @return A SqlDecimal structure whose Value equals the Value of this SqlInt16 structure.
         */
        public SqlDecimal ToSqlDecimal()
        {
            if (IsNull)
                return SqlDecimal.Null;

            return new SqlDecimal(_value);
        }

        /**
         * Converts this SqlInt16 structure to SqlDecimal.
         * @return A SqlDouble structure whose Value equals the Value of this SqlInt16 structure.
         */
        public SqlDouble ToSqlDouble()
        {
            if (IsNull)
                return SqlDouble.Null;

            return new SqlDouble(_value);
        }

        /**
         * Converts this SqlInt16 structure to SqlInt32.
         * @return A SqlInt32 structure whose Value equals the Value of this SqlDouble structure.
         */
        public SqlInt32 ToSqlInt32()
        {
            if (IsNull)
                return SqlInt32.Null;

            return new SqlInt32(_value);
        }


        /**
         * Converts this SqlInt16 structure to SqlInt64.
         * @return A SqlInt64 structure whose Value equals the Value of this SqlInt16 structure.
         */
        public SqlInt64 ToSqlInt64()
        {
            if (IsNull)
                return SqlInt64.Null;

            return new SqlInt64(_value);
        }

        /**
         * Converts this SqlInt16 instance to SqlDouble.
         * @return A SqlMoney instance whose Value equals the Value of this SqlInt16 instance.
         */
        public SqlMoney ToSqlMoney()
        {
            if (IsNull)
                return SqlMoney.Null;

            return new SqlMoney(_value);
        }

        /**
         * Converts this SqlInt16 instance to SqlSingle.
         * @return A SqlSingle instance whose Value equals the Value of this SqlInt16 instance.
         */
        public SqlSingle ToSqlSingle()
        {
            if (IsNull)
                return SqlSingle.Null;

            return new SqlSingle(_value);
        }

        /**
         * Converts this SqlInt16 structure to SqlString.
         * @return A SqlString structure whose value is a string representing the date and time contained in this SqlInt16 structure.
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

        public static void main(String[] args)
        {
            SqlInt16 i1 = new SqlInt16(-2);
            SqlInt16 i2 = new SqlInt16(-2);

            Console.WriteLine(BitwiseAnd(i1, i2));

        }
    
        public override int GetHashCode()
        {
            return _value;
        }

        public static SqlInt16 operator + (SqlInt16 x, SqlInt16 y)
        {
            checked 
            {
                return new SqlInt16 ((short) (x.Value + y.Value));
            }
        }

        public static SqlInt16 operator & (SqlInt16 x, SqlInt16 y)
        {
            return new SqlInt16 ((short) (x.Value & y.Value));
        }

        public static SqlInt16 operator | (SqlInt16 x, SqlInt16 y)
        {
            return new SqlInt16 ((short) ( x.Value | y.Value));
        }

        public static SqlInt16 operator / (SqlInt16 x, SqlInt16 y)
        {
            checked 
            {
                return new SqlInt16 ((short) (x.Value / y.Value));
            }
        }

        public static SqlBoolean operator == (SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value == y.Value);
        }

        public static SqlInt16 operator ^ (SqlInt16 x, SqlInt16 y)
        {
            return new SqlInt16 ((short) (x.Value ^ y.Value));
        }

        public static SqlBoolean operator > (SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value > y.Value);
        }

        public static SqlBoolean operator >= (SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value >= y.Value);
        }

        public static SqlBoolean operator != (SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else 
                return new SqlBoolean (!(x.Value == y.Value));
        }

        public static SqlBoolean operator < (SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value < y.Value);
        }

        public static SqlBoolean operator <= (SqlInt16 x, SqlInt16 y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value <= y.Value);
        }

        public static SqlInt16 operator % (SqlInt16 x, SqlInt16 y)
        {
            return new SqlInt16 ((short) (x.Value % y.Value));
        }

        public static SqlInt16 operator * (SqlInt16 x, SqlInt16 y)
        {
            checked 
            {
                return new SqlInt16 ((short) (x.Value * y.Value));
            }
        }

        public static SqlInt16 operator ~ (SqlInt16 x)
        {
            if (x.IsNull)
                return Null;
			
            return new SqlInt16 ((short) (~x.Value));
        }

        public static SqlInt16 operator - (SqlInt16 x, SqlInt16 y)
        {
            checked 
            {
                return new SqlInt16 ((short) (x.Value - y.Value));
            }
        }

        public static SqlInt16 operator - (SqlInt16 n)
        {
            checked 
            {
                return new SqlInt16 ((short) (-n.Value));
            }
        }

        public static explicit operator SqlInt16 (SqlBoolean x)
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlInt16 ((short)x.ByteValue);
        }

        public static explicit operator SqlInt16 (SqlDecimal x)
        {		
            checked 
            {
                if (x.IsNull)
                    return Null;
                else 
                    return new SqlInt16 ((short)x.Value);
            }
        }

        public static explicit operator SqlInt16 (SqlDouble x)
        {
            if (x.IsNull)
                return Null;
            else 
                return new SqlInt16 (checked ((short)x.Value));
        }

        public static explicit operator short (SqlInt16 x)
        {
            return x.Value; 
        }

        public static explicit operator SqlInt16 (SqlInt32 x)
        {
            checked 
            {
                if (x.IsNull)
                    return Null;
                else 
                    return new SqlInt16 ((short)x.Value);
            }
        }

        public static explicit operator SqlInt16 (SqlInt64 x)
        {
            if (x.IsNull)
                return Null;
            else 
            {
                checked 
                {
                    return new SqlInt16 ((short)x.Value);
                }
            }
        }

        public static explicit operator SqlInt16 (SqlMoney x)
        {
            checked 
            {
                if (x.IsNull)
                    return Null;
                else 
                    return new SqlInt16 ((short)x.Value);
            }			
        }


        public static explicit operator SqlInt16 (SqlSingle x)
        {
            if (x.IsNull)
                return Null;
            else 
            {
                checked 
                {
                    return new SqlInt16 ((short)x.Value);
                }
            }
        }

        public static explicit operator SqlInt16 (SqlString x)
        {	
            if (x.IsNull)
                return Null;

            return SqlInt16.Parse (x.Value);
        }

        public static implicit operator SqlInt16 (short x)
        {
            return new SqlInt16 (x);
        }

        public static implicit operator SqlInt16 (SqlByte x)
        {
            return new SqlInt16 ((short)x.Value);
        }


    }}