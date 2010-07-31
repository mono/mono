// System.Data.SqlTypes.SqlByte
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
    using System.Data;

    /**
     *
     */
    public struct SqlByte : INullable, IComparable
    {
        private byte _value; // = -1;
        private bool _isNull;

        public static readonly SqlByte MaxValue = new SqlByte((byte)0xFF);
        public static readonly SqlByte MinValue = new SqlByte((byte)0);
        public static readonly SqlByte Zero = new SqlByte((byte)0);
        public static readonly SqlByte Null = new SqlByte(true);

        
        private SqlByte(bool isNull)
        {
            _isNull = isNull;
            _value = 0;
        }
        /**
         * Initializes a new instance of the SqlByte instance using the specified byte value.
         * @param value A byte value to be stored in the Value property of the new SqlByte instance.
         */
        public SqlByte(byte value)
        {
            _value = value;
            _isNull = false;
        }


        public override int GetHashCode()
        {
            if (IsNull)
                return 0;
        
            return _value;
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
         * Gets the value of the SqlByte instance.
         * @return the value of this instance
         */
        public byte Value
        {
            get
            {
                if (IsNull)
                {
                    throw new SqlNullValueException();
                }

                return _value;
            }
        }

        /**
         * Computes the sum of the two specified SqlByte instances.
         * @param x A SqlByte instance.
         * @param y A SqlByte instance.
         * @return A SqlByte instance whose Value property contains the results of the addition.
         */
        public static SqlByte Add(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlByte.Null;

            int res  = x._value + y._value;

            if (res > 255)
                throw new OverflowException("Arithmetic Overflow");

            return new SqlByte((byte)res);
        }

        /**
         * Computes the bitwise AND of its SqlByte operands.
         * @param x A SqlByte instance.
         * @param y A SqlByte instance.
         * @return The results of the bitwise AND operation.
         */
        public static SqlByte BitwiseAnd(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlByte.Null;

            int res  = x._value & y._value;
            return new SqlByte((byte)res);
        }

        /**
         * Computes the bitwise OR of its SqlByte operands.
         * @param x A SqlByte instance.
         * @param y A SqlByte instance.
         * @return The results of the bitwise OR operation.
         */
        public static SqlByte BitwiseOr(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlByte.Null;

            int res  = x._value | y._value;
            return new SqlByte((byte)res);
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

            if ((obj is SqlByte) == false)
            {
                throw new ArgumentException("Wrong value type " + 
                    obj.GetType().Name + "in SqlByte.CompareTo");
            }

            SqlByte val = (SqlByte)obj;

            if (IsNull)
            {
                if (val.IsNull)
                    return 0;
                return -1;
            }
            else if (val.IsNull)
                return 1;

            if (_value > val._value)
                return 1;

            if (_value < val._value)
                return -1;

            return 0;
        }


        /**
         * The division operator divides the first SqlByte operand by the second.
         * @param x A SqlByte instance.
         * @param y A SqlByte instance.
         * @return A SqlByte instance containing the results of the division operation.
         * If one of the parameters is null or null value - return SqlDouble.Null.
         */
        public static SqlByte Divide(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlByte.Null;

            if (y._value == 0)
                throw new DivideByZeroException("Divide by zero error encountered.");

            int val = x._value / y._value;
            return new SqlByte((byte)val);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (obj is SqlByte)
            {
                if (((SqlByte)obj)._value == this._value)
                    return true;
            }

            return false;
        }

        /**
         * Performs a logical comparison on two instances of SqlByte to determine if they are equal.
         * @param x A SqlByte instance.
         * @param y A SqlByte instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlByte x, SqlByte y)
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
         * If either instance of SqlByte is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThan(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value > y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlByte to determine if the first is greater than or equal to the second.
         * @param x A SqlByte instance
         * @param y A SqlByte instance
         * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
         * If either instance of SqlByte is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThanOrEqual(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value >= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlByte to determine if the first is less than the second.
         * @param x A SqlByte instance
         * @param y A SqlByte instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlByte is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThan(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value < y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlByte to determine if the first is less than or equal to the second.
         * @param x A SqlByte instance
         * @param y A SqlByte instance
         * @return A SqlBoolean that is True if the first instance is less than or equal to the second instance, otherwise False.
         * If either instance of SqlByte is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThanOrEqual(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value <= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Computes the remainder after dividing its first SqlByte operand by its second.
         * @param x A SqlByte instance
         * @param y A SqlByte instance
         * @return A SqlByte instance whose Value contains the remainder.
         */
        public static SqlByte Mod(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlByte.Null;

            if (y._value == 0)
                throw new DivideByZeroException("Divide by zero error encountered.");

            int mod = x._value % y._value;
            return new SqlByte((byte)mod);
        }

        /**
         * The multiplication operator computes the product of the two SqlByte operands.
         * @param x A SqlByte instance
         * @param y A SqlByte instance
         * @return The product of the two SqlByte operands.
         */
        public static SqlByte Multiply(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlByte.Null;

            int res = x._value * y._value;

            if (res > 255)
                throw new OverflowException("Arithmetic Overflow.");

            return new SqlByte((byte)res);
        }

        /**
         * Compares two instances of SqlByte to determine if they are equal.
         * @param x A SqlByte instance
         * @param y A SqlByte instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlByte is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value != y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * The ones complement operator performs a bitwise one's complement operation on its SqlByte operand.
         * @param x A SqlByte instance
         * @return A SqlByte instance whose Value property contains the ones complement of the SqlByte parameter.
         */
        public static SqlByte OnesComplement(SqlByte x)
        {
            if (x.IsNull)
                return SqlByte.Null;

            int res  = x._value ^ 0xFF; /*@TODO Ma ze ??? */

            return new SqlByte((byte)res);
        }

        /**
         * Converts the String representation of a number to its byte equivalent.
         * @param s The String to be parsed.
         * @return A SqlByte containing the value represented by the String.
         */
        public static SqlByte Parse(String s)
        {
            byte val = byte.Parse(s);
            return new SqlByte(val);
        }

        /**
         * The subtraction operator the second SqlByte operand from the first.
         * @param x A SqlByte instance
         * @param y A SqlByte instance
         * @return The results of the subtraction operation.
         */
        public static SqlByte Subtract(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlByte.Null;

            int res = x._value - y._value;

            if (res < 0)
                throw new OverflowException("Arithmetic Overflow.");

            return new SqlByte((byte)res);
        }


        /**
         * Converts this SqlByte instance to SqlBoolean.
         * @return A SqlBoolean instance whose Value will be True if the SqlByte instance's Value is non-zero,
         * False if the SqlByte is zero
         * and Null if the SqlByte instance is Null.
         */
        public SqlBoolean ToSqlBoolean()
        {
            if (IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(Value);
        }

        /**
         * Converts this SqlByte instance to SqlDecimal.
         * @return A SqlDecimal instance whose Value equals the Value of this SqlByte instance.
         */
        public SqlDecimal ToSqlDecimal()
        {
            if (IsNull)
                return SqlDecimal.Null;

            return new SqlDecimal(Value);
        }

        /**
         * Converts this SqlByte instance to SqlDouble.
         * @return A SqlDouble instance whose Value equals the Value of this SqlByte instance.
         */
        public SqlDouble ToSqlDouble()
        {
            if (IsNull)
                return SqlDouble.Null;

            return new SqlDouble(Value);
        }

        /**
         * Converts this SqlByte instance to SqlSingle.
         * @return A SqlSingle instance whose Value equals the Value of this SqlByte instance.
         */
        public SqlSingle ToSqlSingle()
        {
            if (IsNull)
                return SqlSingle.Null;

            return new SqlSingle(Value);
        }

        /**
         * Converts this SqlByte structure to SqlInt32.
         * @return A SqlInt32 structure whose Value equals the Value of this SqlByte structure.
         */
        public SqlInt32 ToSqlInt32()
        {
            if (IsNull)
                return SqlInt32.Null;

            return new SqlInt32(Value);
        }

        /**
         * Converts this SqlByte structure to SqlInt32.
         * @return A SqlInt32 structure whose Value equals the Value of this SqlByte structure.
         */
        public SqlInt64 ToSqlInt64()
        {
            if (IsNull)
                return SqlInt64.Null;

            return new SqlInt64(Value);
        }

        /**
         * Converts this SqlByte structure to SqlInt16.
         * @return A SqlInt16 structure whose Value equals the Value of this SqlByte structure.
         */
        public SqlInt16 ToSqlInt16()
        {
            if (IsNull)
                return SqlInt16.Null;

            return new SqlInt16(Value);
        }

        /**
         * Converts this SqlByte instance to SqlDecimal.
         * @return A SqlDecimal instance whose Value equals the Value of this SqlByte instance.
         */
        public SqlMoney ToSqlMoney()
        {
            if (IsNull)
                return SqlMoney.Null;

            return new SqlMoney(Value);
        }

        /**
         * Converts this SqlByte structure to SqlString.
         * @return A SqlString structure whose value is a string representing the date and time contained in this SqlByte structure.
         */
        public SqlString ToSqlString()
        {
            if (IsNull)
                return SqlString.Null;

            return new SqlString(_value.ToString());
        }

        /**
         * Performs a bitwise exclusive-OR operation on the supplied parameters.
         * @param x A SqlByte instance
         * @param y A SqlByte instance
         * @return The results of the XOR operation.
         */
        public static SqlByte Xor(SqlByte x, SqlByte y)
        {
            if (x.IsNull || y.IsNull)
                return SqlByte.Null;

            int res  = x._value ^ y._value;

            return new SqlByte((byte)res);
        }

        public override String ToString()
        {
            if (IsNull)
                return "null";

            return _value.ToString();
        }

        public static SqlByte op_Implicit(byte x)
        {
            return new SqlByte(x);
        }

        public static int op_Explicit(SqlByte x)
        {
            return x.Value;
        }

        public static SqlByte op_OnesComplement(SqlByte x)
        {
            return OnesComplement(x);
        }

        public static SqlByte op_Addition(SqlByte x, SqlByte y)
        {
            return Add(x, y);
        }

        public static SqlByte op_Subtraction(SqlByte x, SqlByte y)
        {
            return Subtract(x, y);
        }

        public static SqlByte op_Multiply(SqlByte x, SqlByte y)
        {
            return Multiply(x, y);
        }

        public static SqlByte op_Division(SqlByte x, SqlByte y)
        {
            return Divide(x, y);
        }

        public static SqlByte op_Modulus(SqlByte x, SqlByte y)
        {
            return Mod(x, y);
        }

        public static SqlByte op_BitwiseAnd(SqlByte x, SqlByte y)
        {
            return BitwiseAnd(x, y);
        }

        public static SqlByte op_BitwiseOr(SqlByte x, SqlByte y)
        {
            return BitwiseOr(x, y);
        }

        public static SqlByte op_ExclusiveOr(SqlByte x, SqlByte y)
        {
            return Xor(x, y);
        }

        public static SqlByte op_Explicit(SqlBoolean x)
        {
            if(x.IsNull)
                return SqlByte.Null;

            return new SqlByte((byte)x.ByteValue);
        }

        public static SqlByte op_Explicit(SqlMoney x)
        {
            if(x.IsNull)
                return SqlByte.Null;

            int val = x.ToSqlInt32().Value;
            if (val < 0 || val > 255)
                throw new OverflowException("Arithmetic Overflow.");

            return new SqlByte((byte)val);
        }

        public static SqlByte op_Explicit(SqlInt16 x)
        {
            if(x.IsNull)
                return SqlByte.Null;

            short val = x.Value;
            if (val < 0 || val > 255)
                throw new OverflowException("Arithmetic Overflow.");

            return new SqlByte((byte)val);
        }

        public static SqlByte op_Explicit(SqlInt32 x)
        {
            if(x.IsNull)
                return SqlByte.Null;

            int val = x.Value;
            if (val < 0 || val > 255)
                throw new OverflowException("Arithmetic Overflow.");

            return new SqlByte((byte)val);
        }

        public static SqlByte op_Explicit(SqlInt64 x)
        {
            if(x.IsNull)
                return SqlByte.Null;

            long val = x.Value;
            if (val < 0 || val > 255)
                throw new OverflowException("Arithmetic Overflow.");

            return new SqlByte((byte)val);
        }

        public static SqlByte op_Explicit(SqlSingle x)
        {
            if(x.IsNull)
                return SqlByte.Null;

            float val = x.Value;
            if (val < 0 || val > 255)
                throw new OverflowException("Arithmetic Overflow.");

            return new SqlByte((byte)val);
        }

        public static SqlByte op_Explicit(SqlDouble x)
        {
            if(x.IsNull)
                return SqlByte.Null;

            double val = x.Value;
            if (val < 0 || val > 255)
                throw new OverflowException("Arithmetic Overflow.");

            return new SqlByte((byte)val);
        }

        public static SqlByte op_Explicit(SqlDecimal x)
        {
            if(x.IsNull)
                return SqlByte.Null;

            double val = x.ToDouble();
            if (val < 0 || val > 255)
                throw new OverflowException("Arithmetic Overflow.");

            return new SqlByte((byte)val);
        }

        public static SqlByte op_Explicit(SqlString x)
        {
            if (x.IsNull)
                return SqlByte.Null;

            return Parse(x.Value);
        }

        public static SqlBoolean op_Equality(SqlByte x, SqlByte y)
        {
            return Equals(x, y);
        }

        public static SqlBoolean op_Inequality(SqlByte x, SqlByte y)
        {
            return NotEquals(x, y);
        }

        public static SqlBoolean op_LessThan(SqlByte x, SqlByte y)
        {
            return LessThan(x, y);
        }

        public static SqlBoolean op_GreaterThan(SqlByte x, SqlByte y)
        {
            return GreaterThan(x, y);
        }

        public static SqlBoolean op_LessThanOrEqual(SqlByte x, SqlByte y)
        {
            return LessThanOrEqual(x, y);
        }

        public static SqlBoolean op_GreaterThanOrEqual(SqlByte x, SqlByte y)
        {
            return GreaterThanOrEqual(x, y);
        }

        public static void main(String[] args)
        {
            SqlByte b = new SqlByte((byte)1);
            Console.WriteLine(b);

            Console.WriteLine(OnesComplement(b));
        }
    }}
