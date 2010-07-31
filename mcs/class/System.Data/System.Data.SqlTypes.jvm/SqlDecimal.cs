// System.Data.SqlTypes.SqlDecimal
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

    /**
     * <p>Title: </p>
     * <p>Description: </p>
     * <p>Copyright: Copyright (c) 2002</p>
     * <p>Company: MainSoft</p>
     * @author Pavel Sandler
     * @version 1.0
     */

    using System;

    using java.math;

    /*
    * CURRENT LIMITATIONS:
    * 1. public byte[] Data not implemented.
    * 2. public byte[] BinData not implemented.
    * 3. Precision value is ignored.
    * 4. public SqlDecimal AdjustScale(SqlDecimal n, int position) not implemented.
    * 5. public SqlDecimal ConvertToPrecScale(SqlDecimal n, int precision, int scale) not implemented.
    */


    public struct SqlDecimal : INullable
    {

        private Decimal _value;
        private bool _isNull;

        public static readonly SqlDecimal MaxValue = new SqlDecimal(Decimal.MaxValue);
        public static readonly SqlDecimal MinValue = new SqlDecimal(Decimal.MinValue);
        public static readonly int MaxPrecision = 38;
        public static readonly int MaxScale = MaxPrecision;
        public static readonly SqlDecimal Null = new SqlDecimal(true);

        private int _precision;
        private int _scale;
        

        private SqlDecimal(bool isNull)
        {
            _value = Decimal.Zero;
            _isNull = isNull;
            _precision = 38;
            _scale = 0;
        }
        /**
         * Initializes a new instance of the SqlDecimal instance using the supplied Decimal value.
         * @param value The Decimal value to be stored as a SqlDecimal instance.
         */
        public SqlDecimal(Decimal value) 
        {
            _value = value;
            _isNull = false;
            int[] bits = Decimal.GetBits(value);
            int i = bits[3] & 0xff0000;
            _scale = i >> 16;
            _precision = 38;
        }

        /**
         * Initializes a new instance of the SqlDecimal instance using the supplied double value.
         * @param value The double value to be stored as a SqlDecimal instance.
         */
        public SqlDecimal(double value) 
        { 
            _value = new Decimal(value);
            _isNull = false; 
            int[] bits = Decimal.GetBits(_value);
            int i = bits[3] & 0xff0000;
            _scale = i >> 16;
            _precision = 38;
        }

        /**
         * Initializes a new instance of the SqlDecimal instance using the supplied int value.
         * @param value The int value to be stored as a SqlDecimal instance.
         */
        public SqlDecimal(int value) 
        {
            _value = new Decimal(value);
            _isNull = false;
            int[] bits = Decimal.GetBits(_value);
            int i = bits[3] & 0xff0000;
            _scale = i >> 16;
            _precision = 38;
        }

        /**
         * Initializes a new instance of the SqlDecimal instance using the supplied long value.
         * @param value The long value to be stored as a SqlDecimal instance.
         */
        public SqlDecimal(long value) 
        {
            _value = new Decimal(value);
            _isNull = false;
            int[] bits = Decimal.GetBits(_value);
            int i = bits[3] & 0xff0000;
            _scale = i >> 16;
            _precision = 38;
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
         * Gets the value of the SqlDecimal instance.
         * @return the value of this instance
         */
        public Decimal Value
        {
            get
            {
                if(IsNull)
                    throw new SqlNullValueException();
                return _value;
            }
        }

        public byte[] BinData
        {
            get
            {
                /** @todo implement this method */
                throw new NotImplementedException();
            }
        }

        public byte[] Data
        {
            get
            {
                /** @todo implement this method */
                throw new NotImplementedException();
            }
        }

        /**
         * Indicates whether or not the Value of this SqlDecimal instance is greater than zero.
         * @return true if the Value is assigned to null, otherwise false.
         */
        public bool IsPositive
        {
            get
            {
                if (!IsNull)
                {
                    if (_value >= 0)
                        return true;

                    return false;
                }
            
                throw new SqlNullValueException("The value of this instance is null");
            }
        }

        /**
         * Gets the maximum number of digits used to represent the Value property.
         * @return The maximum number of digits used to represent the Value of this SqlDecimal instance.
         */
        public int Precision
        {
            get
            {
                return _precision;
            }
        }

        /**
         * Gets the number of decimal places to which Value is resolved.
         * @return The number of decimal places to which the Value property is resolved.
         */
        public int Scale
        {
            get
            {
                return  _precision;
            }
        }

        /**
         * The Abs member function gets the absolute value of the SqlDecimal parameter.
         * @param n A SqlDecimal instance.
         * @return A SqlDecimal instance whose Value property contains the unsigned number representing the absolute value of the SqlDecimal parameter.
         */
        public static SqlDecimal Abs(SqlDecimal n)
        {
            if (n.IsNull)
                return new SqlDecimal();

            Decimal val;

            if (n.IsPositive)
                val = n.Value;
            else
                val = Decimal.Negate(n._value);

            return new SqlDecimal(val);

        }

        /**
         * Calcuates the sum of the two SqlDecimal operators.
         * @param x A SqlDecimal instance.
         * @param y A SqlDecimal instance.
         * @return A new SqlDecimal instance whose Value property contains the sum.
         * If one of the parameters or their value is null return SqlDecimal.Null.
         */
        public static SqlDecimal Add(SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull)
                return SqlDecimal.Null;

            Decimal res = Decimal.Add(x._value, y._value);

            return new SqlDecimal(res);
        }

        public static SqlDecimal AdjustScale(SqlDecimal n, int digits, bool fround)
        {
            /** @todo find out what the logic */
            throw new NotImplementedException();
        }

        /**
         * Returns the smallest whole number greater than or equal to the specified SqlDecimal instance.
         * @param n The SqlDecimal instance for which the ceiling value is to be calculated.
         * @return A SqlDecimal representing the smallest whole number greater than or equal to the specified SqlDecimal instance.
         */
        public static SqlDecimal Ceiling(SqlDecimal n)
        {
            if (n.IsNull)
                return SqlDecimal.Null;

            double d = Math.Ceiling((double)n._value);
            return new SqlDecimal(d);
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

            if (obj is SqlDecimal)
            {
                SqlDecimal value = (SqlDecimal)obj;
                
                if (IsNull)
                    return -1;

                if (value.IsNull)
                    return 1;

                if (_value == value._value)
                    return 0;

                return Decimal.Compare(_value, value._value);
            }

            throw new ArgumentException("parameter obj is not SqlDecimal : " + obj.GetType().Name);


        }


        public SqlDecimal ConvertToPrecScale(SqlDecimal n, int precision, int scale)
        {
            /** @todo find out what the logic */
            throw new NotImplementedException();
        }

        /**
         * The division operator divides the first SqlDecimal operand by the second.
         * @param x A SqlDecimal instance.
         * @param y A SqlDecimal instance.
         * @return A SqlDecimal instance containing the results of the division operation.
         * If one of the parameters is null or null value - return SqlDouble.Null.
         */
        public static SqlDecimal Divide(SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull)
                return SqlDecimal.Null;

            Decimal res = Decimal.Divide(x._value, y._value);

            return new SqlDecimal(res);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (obj is SqlDecimal)
            {
                SqlDecimal dec = (SqlDecimal)obj;

                return Decimal.Equals(_value, dec._value);
            }

            return false;
        }

        
        /**
         * Performs a logical comparison on two instances of SqlDecimal to determine if they are equal.
         * @param x A SqlDecimal instance.
         * @param y A SqlDecimal instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.Equals(y))
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Rounds a specified SqlDecimal number to the next lower whole number.
         * @param n The SqlDecimal instance for which the floor value is to be calculated.
         * @return A SqlDecimal instance containing the whole number portion of this SqlDecimal instance.
         */
        public static SqlDecimal Floor(SqlDecimal n)
        {
            Decimal res = Decimal.Floor(n._value);

            return new SqlDecimal(res);
        }

        /**
         * Compares two instances of SqlDecimal to determine if the first is greater than the second.
         * @param x A SqlDecimal instance
         * @param y A SqlDecimal instance
         * @return A SqlBoolean that is True if the first instance is greater than the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThan(SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.CompareTo(y) > 0)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlDecimal to determine if the first is greater than or equal to the second.
         * @param x A SqlDecimal instance
         * @param y A SqlDecimal instance
         * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThanOrEqual(SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.CompareTo(y) >= 0)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlDecimal to determine if the first is less than the second.
         * @param x A SqlDecimal instance
         * @param y A SqlDecimal instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThan(SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.CompareTo(y) < 0)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlDecimal to determine if the first is less than the second.
         * @param x A SqlDecimal instance
         * @param y A SqlDecimal instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThanOrEqual(SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.CompareTo(y) <= 0)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * The multiplication operator computes the product of the two SqlDecimal operands.
         * @param x A SqlDecimal instance
         * @param y A SqlDecimal instance
         * @return The product of the two SqlDecimal operands.
         */
        public static SqlDecimal Multiply(SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull)
                return SqlDecimal.Null;

            Decimal res = Decimal.Multiply(x._value, y._value);

            return new SqlDecimal(res);
        }

        /**
         * Compares two instances of SqlDecimal to determine if they are equal.
         * @param x A SqlDecimal instance
         * @param y A SqlDecimal instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlDecimal x, SqlDecimal y)
        {
            SqlBoolean eVal = Equals(x, y);

            if (eVal.IsNull)
                return eVal;
            if (eVal.IsTrue)
                return SqlBoolean.False;

            return SqlBoolean.True;
        }

        /**
         * Converts the String representation of a number to its Decimal number equivalent.
         * @param s The String to be parsed.
         * @return A SqlDecimal containing the value represented by the String.
         */
        public static SqlDecimal Parse(String s)
        {
            Decimal val = Decimal.Parse(s);
            SqlDecimal retVal = new SqlDecimal(val);

            if (GreaterThan(retVal, MaxValue).IsTrue || LessThan(retVal, MinValue).IsTrue)
                throw new OverflowException("The parse of this string is overflowing : " + val);

            return retVal;

        }

        /**
         * Raises the value of the specified SqlDecimal instance to the specified exponential power.
         * @param n The SqlDecimal instance to be raised to a power.
         * @param exponent A double value indicating the power to which the number should be raised.
         * @return A SqlDecimal instance containing the results.
         */
        public static SqlDecimal Power(SqlDecimal n, double exponent)
        {
            /** @todo decide if we treat the Decimal as a double and use Math.pow() */
            
            double d = (double)n._value;

            d = java.lang.Math.pow(d, exponent);

            return new SqlDecimal(d);
        }

        /**
         * Gets the number nearest the specified SqlDecimal instance's value with the specified precision.
         * @param n The SqlDecimal instance to be rounded.
         * @param position The number of significant fractional digits (precision) in the return value.
         * @return A SqlDecimal instance containing the results of the rounding operation.
         */
        public static SqlDecimal Round(SqlDecimal n, int position)
        {
            Decimal val = Decimal.Round(n._value, position);

            return new SqlDecimal(val);
        }

        /**
         * Gets a value indicating the sign of a SqlDecimal instance's Value property.
         * @param n The SqlDecimal instance whose sign is to be evaluated.
         * @return A number indicating the sign of the SqlDecimal instance.
         */
        public static int Sign(SqlDecimal n)
        {
            if (n._value < 0)
                return -1;
            if(n._value > 0)
                return 1;
            return 0;
        }

        /**
         * The subtraction operator the second SqlDecimal operand from the first.
         * @param x A SqlDecimal instance
         * @param y A SqlDecimal instance
         * @return The results of the subtraction operation.
         */
        public static SqlDecimal Subtract(SqlDecimal x, SqlDecimal y)
        {
            Decimal val = Decimal.Subtract(x._value, y._value);
            SqlDecimal retVal = new SqlDecimal(val);

            return retVal;

        }

        /**
         * Returns the a double equal to the contents of the Value property of this instance.
         * @return The decimal representation of the Value property.
         */
        public double ToDouble()
        {
            return Decimal.ToDouble(_value);
        }

        /**
         * Converts this SqlDecimal instance to SqlBoolean.
         * @return A SqlBoolean instance whose Value will be True if the SqlDecimal instance's Value is non-zero,
         * False if the SqlDecimal is zero
         * and Null if the SqlDecimal instance is Null.
         */
        public SqlBoolean ToSqlBoolean()
        {
            if (IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(!_value.Equals(Decimal.Zero));
        }

        /**
         * Converts this SqlDecimal instance to SqlByte.
         * @return A SqlByte instance whose Value equals the Value of this SqlDouble instance.
         */
        public SqlByte ToSqlByte()
        {
            if (IsNull)
                return SqlByte.Null;

            return new SqlByte(checked((byte)_value));
        }

        /**
         * Converts this SqlDecimal instance to SqlDouble.
         * @return A SqlDouble instance whose Value equals the Value of this SqlDecimal instance.
         */
        public SqlDouble ToSqlDouble()
        {
            if (IsNull)
                return SqlDouble.Null;

            return new SqlDouble((double)_value);
        }

        /**
         * Converts this SqlDouble structure to SqlInt16.
         * @return A SqlInt16 structure whose Value equals the Value of this SqlDouble structure.
         */
        public SqlInt16 ToSqlInt16()
        {
            if (IsNull)
                return SqlInt16.Null;

            return new SqlInt16(checked((short)_value));
        }

        /**
         * Converts this SqlDouble structure to SqlInt32.
         * @return A SqlInt32 structure whose Value equals the Value of this SqlDouble structure.
         */
        public SqlInt32 ToSqlInt32()
        {
            if (IsNull)
                return SqlInt32.Null;

            return new SqlInt32(checked((int)_value));
        }

        /**
         * Converts this SqlDecimal structure to SqlInt64.
         * @return A SqlInt64 structure whose Value equals the Value of this SqlDecimal structure.
         */
        public SqlInt64 ToSqlInt64()
        {
            if (IsNull)
                return SqlInt64.Null;

            return new SqlInt64(checked((long)_value));
        }

        /**
         * Converts this SqlDecimal instance to SqlDouble.
         * @return A SqlMoney instance whose Value equals the Value of this SqlDecimal instance.
         */
        public SqlMoney ToSqlMoney()
        {
            if (IsNull)
                return SqlMoney.Null;

            return new SqlMoney(_value);
        }

        /**
         * Converts this SqlDecimal instance to SqlSingle.
         * @return A SqlSingle instance whose Value equals the Value of this SqlDecimal instance.
         */
        public SqlSingle ToSqlSingle()
        {
            if (IsNull)
                return SqlSingle.Null;

            return new SqlSingle(checked((float)_value));
        }

        /**
         * Converts this SqlDecimal structure to SqlString.
         * @return A SqlString structure whose value is a string representing the date and time contained in this SqlDecimal structure.
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

        /**
         * Truncates the specified SqlDecimal instance's value to the desired position.
         * @param n The SqlDecimal instance to be truncated.
         * @param position The decimal position to which the number will be truncated.
         * @return Supply a negative value for the position parameter in order to truncate the value to the corresponding positon to the left of the decimal point.
         */
        public static SqlDecimal Truncate(SqlDecimal n, int position)
        {
            if (n.IsNull)
                return n;
            
            Decimal tmp = Decimal.Round(n._value, position);

            return new SqlDecimal(tmp);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
        
        public static SqlDecimal operator + (SqlDecimal x, SqlDecimal y)
        {
            if(x.IsNull || y.IsNull)
                return SqlDecimal.Null;

            return new SqlDecimal(x.Value + y.Value);
        }

        public static SqlDecimal operator / (SqlDecimal x, SqlDecimal y)
        {
            if(x.IsNull || y.IsNull)
                return SqlDecimal.Null;
            return new SqlDecimal (x.Value / y.Value);
        }

        public static SqlBoolean operator == (SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value == y.Value);
        }

        public static SqlBoolean operator > (SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value > y.Value);
        }

        public static SqlBoolean operator >= (SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value >= y.Value);
        }

        public static SqlBoolean operator != (SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value != y.Value);
        }

        public static SqlBoolean operator < (SqlDecimal x, SqlDecimal y)
        {

            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value < y.Value);

        }

        public static SqlBoolean operator <= (SqlDecimal x, SqlDecimal y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value <= y.Value);
        }

        public static SqlDecimal operator * (SqlDecimal x, SqlDecimal y)
        {
            // adjust the scale to the smaller of the two beforehand
            if (x.Scale > y.Scale)
                x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);
            else if (y.Scale > x.Scale)
                y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);

            return new SqlDecimal(x.Value * y.Value);
        }

        public static SqlDecimal operator - (SqlDecimal x, SqlDecimal y)
        {
            if(x.IsNull || y.IsNull)
                return SqlDecimal.Null;

            return new SqlDecimal(x.Value - y.Value);
        }

        public static SqlDecimal operator - (SqlDecimal n)
        {
            if(n.IsNull)
                return n;
            return new SqlDecimal (Decimal.Negate(n.Value));
        }

        public static explicit operator SqlDecimal (SqlBoolean x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDecimal ((decimal)x.ByteValue);
        }

        public static explicit operator Decimal (SqlDecimal n)
        {
            return n.Value;
        }

        public static explicit operator SqlDecimal (SqlDouble x)
        {
            checked 
            {
                if (x.IsNull) 
                    return Null;
                else
                    return new SqlDecimal ((decimal)x.Value);
            }
        }

        public static explicit operator SqlDecimal (SqlSingle x)
        {
            checked 
            {
                if (x.IsNull) 
                    return Null;
                else
                    return new SqlDecimal ((decimal)x.Value);
            }
        }

        public static explicit operator SqlDecimal (SqlString x)
        {
            checked 
            {
                return Parse (x.Value);
            }
        }

        public static implicit operator SqlDecimal (decimal x)
        {
            return new SqlDecimal (x);
        }

        public static implicit operator SqlDecimal (SqlByte x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDecimal ((decimal)x.Value);
        }

        public static implicit operator SqlDecimal (SqlInt16 x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDecimal ((decimal)x.Value);
        }

        public static implicit operator SqlDecimal (SqlInt32 x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDecimal ((decimal)x.Value);
        }

        public static implicit operator SqlDecimal (SqlInt64 x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDecimal ((decimal)x.Value);
        }

        public static implicit operator SqlDecimal (SqlMoney x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDecimal ((decimal)x.Value);
        }

    }
}