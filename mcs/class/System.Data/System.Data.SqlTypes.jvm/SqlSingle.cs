// System.Data.SqlTypes.SqlSingle
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

    public struct SqlSingle : INullable, IComparable
    {

        private float _value;

        private static float _minVal = -3.40E+38F;
        private static float _maxVal = 3.40E+38F;

        public static readonly SqlSingle MaxValue = new SqlSingle(3.40E+38F);
        public static readonly SqlSingle MinValue = new SqlSingle(-3.40E+38F);
        public static readonly SqlSingle Null = new SqlSingle(true);
        public static readonly SqlSingle Zero = new SqlSingle(0);

        private bool _isNull;

        private SqlSingle(bool isNull)
        {
            _isNull = isNull;
            _value = 0;
        }

        /**
         * Constructor
         * @param value A float whose value will be used for the new SqlSingle.
         */
        public SqlSingle(float value) 
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
         * Gets the value of the SqlSingle instance.
         * @return the value of this instance
         */
        public float Value
        {
            get
            {
                if(IsNull)
                    throw new SqlNullValueException();
                return _value;
            }
        }

        /**
         * The addition operator computes the sum of the two SqlSingle operands.
         * @param x A SqlSingle structure.
         * @param y A SqlSingle structure.
         * @return The sum of the two SqlSingle operands.
         * If one of the parameters is null or null value - return SqlSingle.Null.
         */
        public static SqlSingle Add(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
                return SqlSingle.Null;

            float xVal = x._value;
            float yVal = y._value;

            if (xVal < 0 && yVal < 0 && (_minVal - xVal > yVal))
                throw new System.OverflowException("Overflow - " + x + " + " + y + " < " + _minVal);
            if (xVal > 0 && yVal > 0 && (_maxVal - xVal < yVal))
                throw new System.OverflowException("Overflow - " + x + " + " + y + " > " + _maxVal);


            return new SqlSingle(xVal + yVal);
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

            if (obj is SqlSingle)
            {
                SqlSingle d = (SqlSingle)obj;

                if (d.IsNull)
                    return 1;
                if (this.IsNull)
                    return -1;

                return this._value.CompareTo(d._value);
            }

            throw new ArgumentException("parameter obj is not SqlSingle : " + obj.GetType().Name);

        }

        /**
         * The division operator divides the first SqlSingle operand by the second.
         * @param x A SqlSingle instance.
         * @param y A SqlSingle instance.
         * @return A SqlSingle structure containing the results of the division operation.
         * If one of the parameters is null or null value - return SqlSingle.Null.
         */
        public static SqlSingle Divide(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
                return SqlSingle.Null;

            float xVal = x._value;
            float yVal = y._value;

            if (java.lang.Math.abs(yVal) < 1)
            {
                if (xVal < 0 && yVal < 0 && (_maxVal * yVal > xVal))
                    throw new System.OverflowException("Overflow - " + x + " / " + y + " > " + _maxVal);
                if (xVal < 0 && yVal > 0 && (_minVal * yVal > xVal))
                    throw new System.OverflowException("Overflow - " + x + " / " + y + " < " + _minVal);
                if (xVal > 0 && yVal < 0 && (_minVal * yVal < xVal))
                    throw new System.OverflowException("Overflow - " + x + " / " + y + " < " + _minVal);
                if (xVal > 0 && yVal > 0 && (_maxVal * yVal < xVal))
                    throw new System.OverflowException("Overflow - " + x + " / " + y + " > " + _maxVal);
            }

            return new SqlSingle(xVal / yVal);
        }

        public override bool Equals(Object obj)
        {
            if (Object.ReferenceEquals(obj, this))
                return true;

            if (obj == null)
                return false;

            if (obj is SqlSingle)
            {
                SqlSingle d = (SqlSingle)obj;

                if (IsNull && d.IsNull)
                    return true;

                if (IsNull || d.IsNull)
                    return false;

                return _value.Equals(d._value);
            }

            return false;
        }

        


        /**
         * Performs a logical comparison on two instances of SqlSingle to determine if they are equal.
         * @param x A SqlSingle instance.
         * @param y A SqlSingle instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.Equals(y))
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlSingle to determine if the first is greater than the second.
         * @param x A SqlSingle instance
         * @param y A SqlSingle instance
         * @return A SqlBoolean that is True if the first instance is greater than the second instance, otherwise False.
         * If either instance of SqlSingle is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThan(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value > y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlSingle to determine if the first is greater than or equal to the second.
         * @param x A SqlSingle instance
         * @param y A SqlSingle instance
         * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
         * If either instance of SqlSingle is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThanOrEqual(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value >= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlSingle to determine if the first is less than the second.
         * @param x A SqlSingle instance
         * @param y A SqlSingle instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlSingle is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThan(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value < y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlSingle to determine if the first is less than or equal to the second.
         * @param x A SqlSingle instance
         * @param y A SqlSingle instance
         * @return A SqlBoolean that is True if the first instance is less than or equal to the second instance, otherwise False.
         * If either instance of SqlSingle is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThanOrEqual(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value <= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * The multiplication operator computes the product of the two SqlSingle operands.
         * @param x A SqlSingle instance
         * @param y A SqlSingle instance
         * @return The product of the two SqlSingle operands.
         */
        public static SqlSingle Multiply(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
                return SqlSingle.Null;

            float xVal = x._value;
            float yVal = y._value;

            if (java.lang.Math.abs(xVal) > 1 && java.lang.Math.abs(yVal) > 1)
            {
                if (xVal < 0 && yVal < 0 && (_maxVal / xVal > yVal))
                    throw new System.OverflowException("Overflow - " + x + " * " + y + " > " + _maxVal);
                if (xVal < 0 && yVal > 0 && (_minVal / xVal < yVal))
                    throw new System.OverflowException("Overflow - " + x + " * " + y + " < " + _minVal);
                if (xVal > 0 && yVal < 0 && (_minVal / xVal > yVal))
                    throw new System.OverflowException("Overflow - " + x + " * " + y + " < " + _minVal);
                if (xVal > 0 && yVal > 0 && (_maxVal / xVal < yVal))
                    throw new System.OverflowException("Overflow - " + x + " * " + y + " > " + _maxVal);
            }

            return new SqlSingle(xVal * yVal);
        }


        /**
         * Compares two instances of SqlSingle to determine if they are equal.
         * @param x A SqlSingle instance
         * @param y A SqlSingle instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlSingle is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlSingle x, SqlSingle y)
        {
            SqlBoolean res = Equals(x, y);

            if (res.IsNull)
                return res;
            if (res.IsFalse)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Converts the String representation of a number to its float-precision floating point number equivalent.
         * @param s The String to be parsed.
         * @return A SqlSingle containing the value represented by the String.
         */
        public static SqlSingle Parse(String s)
        {
            float d = Single.Parse(s);
            return new SqlSingle(d);
        }

        /**
         * The subtraction operator the second SqlSingle operand from the first.
         * @param x A SqlSingle instance
         * @param y A SqlSingle instance
         * @return The results of the subtraction operation.
         */
        public static SqlSingle Subtract(SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y.IsNull)
                return SqlSingle.Null;

            float xVal = x._value;
            float yVal = y._value;

            if (xVal < 0 && yVal > 0 && (java.lang.Math.abs(_minVal - xVal) < yVal))
                throw new System.OverflowException("Overflow - " + x + " - " + y + " < " + _minVal);
            if (xVal > 0 && yVal < 0 && (_maxVal - xVal < java.lang.Math.abs(yVal)))
                throw new System.OverflowException("Overflow - " + x + " - " + y + " > " + _maxVal);


            return new SqlSingle(x._value - y._value);

        }

        /**
         * Converts this SqlSingle structure to SqlBoolean.
         * @return A SqlBoolean structure whose Value will be True if the SqlSingle structure's Value is non-zero, False if the SqlSingle is zero
         * and Null if the SqlSingle structure is Null.
         */
        public SqlBoolean ToSqlBoolean()
        {
            if (_value == 0)
                return new SqlBoolean(0);

            return new SqlBoolean(1);
        }

        /**
         * Converts this SqlSingle structure to SqlByte.
         * @return A SqlByte structure whose Value equals the Value of this SqlSingle structure.
         */
        public SqlByte ToSqlByte()
        {
            int val = (int)_value;

            if (val < 0 || val > 255)
                throw new OverflowException("Can not conver this instance to SqlByte - overflowing : " + val);

            return new SqlByte((byte)val);
        }

        /**
         * Converts this SqlSingle structure to SqlDouble.
         * @return A SqlDouble structure whose Value equals the Value of this SqlSingle structure.
         */
        public SqlDouble ToSqlDouble()
        {
            return new SqlDouble(_value);
        }

        /**
         * Converts this SqlSingle structure to SqlDecimal.
         * @return A SqlDecimal structure whose Value equals the Value of this SqlSingle structure.
         */
        public SqlDecimal ToSqlDecimal()
        {
            return new SqlDecimal(_value);
        }

        /**
         * Converts this SqlSingle structure to SqlInt16.
         * @return A SqlInt16 structure whose Value equals the Value of this SqlSingle structure.
         */
        public SqlInt16 ToSqlInt16()
        {
            return new SqlInt16((short)_value);
        }

        /**
         * Converts this SqlSingle structure to SqlInt32.
         * @return A SqlInt32 structure whose Value equals the Value of this SqlSingle structure.
         */
        public SqlInt32 ToSqlInt32()
        {
            return new SqlInt32((int)_value);
        }

        /**
         * Converts this SqlSingle structure to SqlInt64.
         * @return A SqlInt64 structure whose Value equals the Value of this SqlSingle structure.
         */
        public SqlInt64 ToSqlInt64()
        {
            return new SqlInt64((long)_value);
        }

        /**
         * Converts this SqlSingle instance to SqlSingle.
         * @return A SqlMoney instance whose Value equals the Value of this SqlSingle instance.
         */
        public SqlMoney ToSqlMoney()
        {
            return new SqlMoney(_value);
        }

        /**
         * Converts this SqlSingle structure to SqlString.
         * @return A SqlString structure whose value is a string representing the date and time contained in this SqlSingle structure.
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
        
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static SqlSingle operator + (SqlSingle x, SqlSingle y)
        {
            float f = (float)(x.Value + y.Value);

            if (Single.IsInfinity (f))
                throw new OverflowException ();

            return new SqlSingle (f);
        }

        public static SqlSingle operator / (SqlSingle x, SqlSingle y)
        {
            float f = (float)(x.Value / y.Value);

            if (Single.IsInfinity (f)) 
            {
				
                if (y.Value == 0d) 
                    throw new DivideByZeroException ();
            }

            return new SqlSingle (x.Value / y.Value);
        }

        public static SqlBoolean operator == (SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y .IsNull) 
                return SqlBoolean.Null;
            return new SqlBoolean (x.Value == y.Value);
        }

        public static SqlBoolean operator > (SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y .IsNull) 
                return SqlBoolean.Null;
            return new SqlBoolean (x.Value > y.Value);
        }

        public static SqlBoolean operator >= (SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y .IsNull) 
                return SqlBoolean.Null;
            return new SqlBoolean (x.Value >= y.Value);
        }

        public static SqlBoolean operator != (SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y .IsNull) 
                return SqlBoolean.Null;
            return new SqlBoolean (!(x.Value == y.Value));
        }

        public static SqlBoolean operator < (SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y .IsNull) 
                return SqlBoolean.Null;
            return new SqlBoolean (x.Value < y.Value);
        }

        public static SqlBoolean operator <= (SqlSingle x, SqlSingle y)
        {
            if (x.IsNull || y .IsNull) 
                return SqlBoolean.Null;
            return new SqlBoolean (x.Value <= y.Value);
        }

        public static SqlSingle operator * (SqlSingle x, SqlSingle y)
        {
            float f = (float)(x.Value * y.Value);
			
            if (Single.IsInfinity (f))
                throw new OverflowException ();

            return new SqlSingle (f);
        }

        public static SqlSingle operator - (SqlSingle x, SqlSingle y)
        {
            float f = (float)(x.Value - y.Value);

            if (Single.IsInfinity (f))
                throw new OverflowException ();

            return new SqlSingle (f);
        }

        public static SqlSingle operator - (SqlSingle n)
        {
            return new SqlSingle (-(n.Value));
        }

        public static explicit operator SqlSingle (SqlBoolean x)
        {
            checked 
            {
                if (x.IsNull)
                    return Null;
				
                return new SqlSingle((float)x.ByteValue);
            }
        }

        public static explicit operator SqlSingle (SqlDouble x)
        {
            if (x.IsNull)
                return Null;

            float f = (float)x.Value;

            if (Single.IsInfinity (f))
                throw new OverflowException ();
				
            return new SqlSingle(f);
        }

        public static explicit operator float (SqlSingle x)
        {
            return x.Value;
        }

        public static explicit operator SqlSingle (SqlString x)
        {
            checked 
            {
                if (x.IsNull)
                    return Null;
				
                return SqlSingle.Parse (x.Value);
            }
        }

        public static implicit operator SqlSingle (float x)
        {
            return new SqlSingle (x);
        }

        public static implicit operator SqlSingle (SqlByte x)
        {
            if (x.IsNull) 
                return Null;
            else 
                return new SqlSingle((float)x.Value);
        }

        public static implicit operator SqlSingle (SqlDecimal x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlSingle((float)x.Value);
        }

        public static implicit operator SqlSingle (SqlInt16 x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlSingle((float)x.Value);
        }

        public static implicit operator SqlSingle (SqlInt32 x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlSingle((float)x.Value);
        }

        public static implicit operator SqlSingle (SqlInt64 x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlSingle((float)x.Value);
        }

        public static implicit operator SqlSingle (SqlMoney x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlSingle((float)x.Value);
        }

    }
}