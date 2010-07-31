// System.Data.SqlTypes.SqlDouble
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

    public struct SqlDouble : INullable, IComparable
    {

        private Double _value;
        private bool _isNull;

        //private static double _maxVal = 1.79E+308;
        //private static double _minVal = -1.79E+308;

        public static readonly SqlDouble MaxValue = new SqlDouble(1.79E+308);
        public static readonly SqlDouble MinValue = new SqlDouble(-1.79E+308);
        public static readonly SqlDouble Null = new SqlDouble(true);
        public static readonly SqlDouble Zero = new SqlDouble(0.0);

        
        private SqlDouble(bool isNull)
        {
            _value = 0;
            _isNull = isNull;
        }

        /**
         * Constructor
         * @param value A double whose value will be used for the new SqlDouble.
         */
        public SqlDouble(double value) 
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
         * Gets the value of the SqlDouble instance.
         * @return the value of this instance
         */
        public double Value
        {
            get
            {
                if(IsNull)
                    throw new SqlNullValueException();
                return _value;
            }
        }

        /**
         * The addition operator computes the sum of the two SqlDouble operands.
         * @param x A SqlDouble structure.
         * @param y A SqlDouble structure.
         * @return The sum of the two SqlDouble operands.
         * If one of the parameters is null or null value - return SqlDouble.Null.
         */
        public static SqlDouble Add(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
                return SqlDouble.Null;

            double xVal = x._value;
            double yVal = y._value;

            return new SqlDouble(checked(xVal + yVal));
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

            if (obj is SqlDouble)
            {
                SqlDouble d = (SqlDouble)obj;

                if (d.IsNull)
                    return 1;
                if (this.IsNull)
                    return -1;

                return this._value.CompareTo(d._value);
            }

            throw new ArgumentException("parameter obj is not SqlDouble : " + obj.GetType().Name);

        }

        /**
         * The division operator divides the first SqlDouble operand by the second.
         * @param x A SqlDouble instance.
         * @param y A SqlDouble instance.
         * @return A SqlDouble structure containing the results of the division operation.
         * If one of the parameters is null or null value - return SqlDouble.Null.
         */
        public static SqlDouble Divide(SqlDouble x, SqlDouble y)
        {

            if (x.IsNull || y.IsNull)
                return SqlDouble.Null;

            double xVal = x._value;
            double yVal = y._value;

            return new SqlDouble(checked(xVal / yVal));

        }

        public override bool Equals(Object obj)
        {
            
            if (obj == null)
                return false;

            if (obj is SqlDouble)
            {
                SqlDouble d = (SqlDouble)obj;

                if (IsNull && d.IsNull)
                    return true;

                if (IsNull || d.IsNull)
                    return false;

                return _value == d._value;
            }

            return false;
        }

        /**
         * Performs a logical comparison on two instances of SqlDouble to determine if they are equal.
         * @param x A SqlDouble instance.
         * @param y A SqlDouble instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.Equals(y))
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlDouble to determine if the first is greater than the second.
         * @param x A SqlDouble instance
         * @param y A SqlDouble instance
         * @return A SqlBoolean that is True if the first instance is greater than the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThan(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value > y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlDouble to determine if the first is greater than or equal to the second.
         * @param x A SqlDouble instance
         * @param y A SqlDouble instance
         * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThanOrEqual(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value >= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlDouble to determine if the first is less than the second.
         * @param x A SqlDouble instance
         * @param y A SqlDouble instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThan(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value < y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlDouble to determine if the first is less than or equal to the second.
         * @param x A SqlDouble instance
         * @param y A SqlDouble instance
         * @return A SqlBoolean that is True if the first instance is less than or equal to the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThanOrEqual(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value <= y._value)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * The multiplication operator computes the product of the two SqlDouble operands.
         * @param x A SqlDouble instance
         * @param y A SqlDouble instance
         * @return The product of the two SqlDouble operands.
         */
        public static SqlDouble Multiply(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
                return SqlDouble.Null;

            double xVal = x._value;
            double yVal = y._value;

            return new SqlDouble(checked(xVal * yVal));
        }


        /**
         * Compares two instances of SqlDouble to determine if they are equal.
         * @param x A SqlDouble instance
         * @param y A SqlDouble instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlDouble x, SqlDouble y)
        {
            SqlBoolean res = Equals(x, y);

            if (res.IsNull)
                return res;
            if (res.IsFalse)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Converts the String representation of a number to its double-precision floating point number equivalent.
         * @param s The String to be parsed.
         * @return A SqlDouble containing the value represented by the String.
         */
        public static SqlDouble Parse(String s)
        {
            double d = double.Parse(s);
            return new SqlDouble(d);
        }

        /**
         * The subtraction operator the second SqlDouble operand from the first.
         * @param x A SqlDouble instance
         * @param y A SqlDouble instance
         * @return The results of the subtraction operation.
         */
        public static SqlDouble Subtract(SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull)
                return SqlDouble.Null;

            double xVal = x._value;
            double yVal = y._value;

            return new SqlDouble(checked(xVal - yVal));
        }

        /**
         * Converts this SqlDouble structure to SqlBoolean.
         * @return A SqlBoolean structure whose Value will be True if the SqlDouble structure's Value is non-zero, False if the SqlDouble is zero
         * and Null if the SqlDouble structure is Null.
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
         * Converts this SqlDouble structure to SqlByte.
         * @return A SqlByte structure whose Value equals the Value of this SqlDouble structure.
         */
        public SqlByte ToSqlByte()
        {
            if (IsNull)
                return SqlByte.Null;

            return new SqlByte(checked((byte)_value));
        }

        /**
         * Converts this SqlDouble structure to SqlDecimal.
         * @return A SqlDecimal structure whose Value equals the Value of this SqlDouble structure.
         */
        public SqlDecimal ToSqlDecimal()
        {
            if (IsNull)
                return SqlDecimal.Null;

            return new SqlDecimal(_value);
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
         * Converts this SqlDouble structure to SqlInt64.
         * @return A SqlInt64 structure whose Value equals the Value of this SqlDouble structure.
         */
        public SqlInt64 ToSqlInt64()
        {
            if (IsNull)
                return SqlInt64.Null;

            return new SqlInt64(checked((long)_value));
        }

        /**
         * Converts this SqlDouble instance to SqlDouble.
         * @return A SqlMoney instance whose Value equals the Value of this SqlDouble instance.
         */
        public SqlMoney ToSqlMoney()
        {
            if (IsNull)
                return SqlMoney.Null;

            return new SqlMoney(_value);
        }

        /**
         * Converts this SqlMoney instance to SqlSingle.
         * @return A SqlSingle instance whose Value equals the Value of this SqlMoney instance.
         */
        public SqlSingle ToSqlSingle()
        {
            if (IsNull)
                return SqlSingle.Null;

            return new SqlSingle(checked((float)_value));
        }


        /**
         * Converts this SqlDouble structure to SqlString.
         * @return A SqlString structure whose value is a string representing the date and time contained in this SqlDouble structure.
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

        public static SqlDouble operator + (SqlDouble x, SqlDouble y)
        {
            double d = 0;
            d = x.Value + y.Value;
			
            if (Double.IsInfinity (d)) 
                throw new OverflowException ();

            return new SqlDouble (d);
        }

        public static SqlDouble operator / (SqlDouble x, SqlDouble y)
        {
            double d = x.Value / y.Value;

            if (Double.IsInfinity (d)) 
            {
                if (y.Value == 0) 
                    throw new DivideByZeroException ();
            }
				
            return new SqlDouble (d);
        }

        public static SqlBoolean operator == (SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull) 	
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value == y.Value);
        }

        public static SqlBoolean operator > (SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value > y.Value);
        }

        public static SqlBoolean operator >= (SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value >= y.Value);
        }

        public static SqlBoolean operator != (SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (!(x.Value == y.Value));
        }

        public static SqlBoolean operator < (SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value < y.Value);
        }

        public static SqlBoolean operator <= (SqlDouble x, SqlDouble y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value <= y.Value);
        }

        public static SqlDouble operator * (SqlDouble x, SqlDouble y)
        {
            double d = x.Value * y.Value;
			
            if (Double.IsInfinity (d))
                throw new OverflowException ();

            return new SqlDouble (d);

        }

        public static SqlDouble operator - (SqlDouble x, SqlDouble y)
        {
            double d = x.Value - y.Value;
			
            if (Double.IsInfinity (d))
                throw new OverflowException ();

            return new SqlDouble (d);
        }

        public static SqlDouble operator - (SqlDouble n)
        {			
            return new SqlDouble (-(n.Value));
        }

        public static explicit operator SqlDouble (SqlBoolean x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDouble ((double)x.ByteValue);
        }

        public static explicit operator double (SqlDouble x)
        {
            return x.Value;
        }

        public static explicit operator SqlDouble (SqlString x)
        {
            checked 
            {
                return SqlDouble.Parse (x.Value);
            }
        }

        public static implicit operator SqlDouble (double x)
        {
            return new SqlDouble (x);
        }

        public static implicit operator SqlDouble (SqlByte x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDouble ((double)x.Value);
        }

        public static implicit operator SqlDouble (SqlDecimal x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDouble (x.ToDouble ());
        }

        public static implicit operator SqlDouble (SqlInt16 x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDouble ((double)x.Value);
        }

        public static implicit operator SqlDouble (SqlInt32 x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDouble ((double)x.Value);
        }

        public static implicit operator SqlDouble (SqlInt64 x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDouble ((double)x.Value);
        }

        public static implicit operator SqlDouble (SqlMoney x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDouble ((double)x.Value);
        }

        public static implicit operator SqlDouble (SqlSingle x)
        {
            if (x.IsNull) 
                return Null;
            else
                return new SqlDouble ((double)x.Value);
        }
   }
}