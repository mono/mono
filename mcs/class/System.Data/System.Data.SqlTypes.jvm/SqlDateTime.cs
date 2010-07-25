// System.Data.SqlTypes.SqlDateTime
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


    /*
    * CURRENT LIMITATIONS:
    * 1. Doesn't support billiseconds
    * 2. public int DayTicks not implemented (I do not understand the logic)
    */

    public struct SqlDateTime : INullable, IComparable
    {
        private DateTime _value;
        private bool _isNull;

        public static readonly SqlDateTime MaxValue = new SqlDateTime(9999, 12, 31);
        public static readonly SqlDateTime MinValue = new SqlDateTime(1753, 1, 1);
        public static readonly SqlDateTime Null = new SqlDateTime(true);
        public static readonly int SqlTicksPerSecond = 300;
        public static readonly int SqlTicksPerMinute = 18000;
        public static readonly int SqlTicksPerHour = 1080000;
        
        private SqlDateTime(bool isNull)
        {
            _isNull = isNull;
            _value = DateTime.MinValue;
        }

        public SqlDateTime(DateTime value) 
        {
            _value = value;
            _isNull = false;
        }

        public SqlDateTime(int DateTicks, int TimeTicks)
        {
            throw new NotImplementedException("ctor SqlDateTime(int DateTicks, int TimeTicks) not implemented.");
        }

        /**
         * Constract a new SqlDateTime object
         * @param year the year
         * @param month the month (1-12)
         * @param day the day in the month
         */
        public SqlDateTime(int year, int month, int day) 
        : this(year, month, day, 0, 0, 0, 0)
        {
        }

        /**
         * Constract a new SqlDateTime object
         * @param year the year
         * @param month the month (1-12)
         * @param day the day in the month
         * @param hour the number of hours
         */
        public SqlDateTime(int year, int month, int day, int hour)
            : this(year, month, day, hour, 0, 0, 0)
        {
        }

        /**
         * Constract a new SqlDateTime object
         * @param year the year
         * @param month the month (1-12)
         * @param day the day in the month
         * @param hour the number of hours
         * @param minute the number of minutes
         */
        public SqlDateTime(int year, int month, int day, int hour, int minute)
            : this(year, month, day, hour, minute, 0, 0)
        { 
        }
        /**
         * Constract a new SqlDateTime object
         * @param year the year
         * @param month the month (1-12)
         * @param day the day in the month
         * @param hour the number of hours
         * @param minute the number of minutes
         * @param second the number of seconds
         */
        public SqlDateTime(int year, int month, int day, int hour, int minute, int second)
            : this(year, month, day, hour, minute, second, 0)
        {
        }

        /**
         * Constract a new SqlDateTime object
         * @param year the year
         * @param month the month (1-12)
         * @param day the day in the month
         * @param hour the number of hours
         * @param minute the number of minutes
         * @param second the number of seconds
         * @param millisecond the number of milliseconds
         */
        public SqlDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            _value = new DateTime(year, month, day, hour, minute, second, millisecond);
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
         * Gets the value of the SqlDateTime instance.
         * @return the value of this instance
         */
        public DateTime Value
        {
            get
            {
                if(IsNull)
                    throw new SqlNullValueException();
                return _value;
            }
        }

        public int DayTicks
        {
            get
            {
                /** @todo find the logic */
                return -1;
            }
        }

        /**
         * Gets the number of ticks representing the time of this SqlDateTime structure.
         * @return The number of ticks representing the time of this SqlDateTime structure.
         */
        public int TimeTicks
        {
            get
            {
                if (IsNull)
                {
                    int res = _value.Hour * SqlTicksPerHour;
                    res += (_value.Minute * SqlTicksPerMinute);
                    res += (_value.Second * SqlTicksPerSecond);
                    res += (_value.Millisecond * SqlTicksPerSecond * 1000);

                    return res;

                }

                throw new SqlNullValueException("The value of this instance is null");
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

            if (obj is SqlDateTime)
            {
                SqlDateTime value = (SqlDateTime)obj;

                if (value.IsNull)
                    return 1;
                if (IsNull)
                    return -1;

                return _value.CompareTo(value._value);
            }

            throw new ArgumentException("parameter obj is not SqlDateTime : " + obj.GetType().Name);

        }

        public override bool Equals(Object obj)
        {
            if (obj is SqlDateTime)
            {
                SqlDateTime d = (SqlDateTime)obj;
                
                if (d.IsNull || d.IsNull)
                    return false;

                if (d._value  == _value)
                    return true;

                return _value.Equals(d._value);
            }

            return false;
        }

        
        /**
         * Performs a logical comparison on two instances of SqlDateTime to determine if they are equal.
         * @param x A SqlDateTime instance.
         * @param y A SqlDateTime instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlDateTime d1, SqlDateTime d2)
        {
            return d1 == d2;
        }

        
        /**
         * Compares two instances of SqlDateTime to determine if the first is greater than the second.
         * @param x A SqlDateTime instance
         * @param y A SqlDateTime instance
         * @return A SqlBoolean that is True if the first instance is greater than the second instance, otherwise False.
         * If either instance of SqlDateTime is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThan(SqlDateTime d1, SqlDateTime d2)
        {
            return d1 > d2;
        }

        /**
         * Compares two instances of SqlDateTime to determine if the first is greater than or equal to the second.
         * @param x A SqlDateTime instance
         * @param y A SqlDateTime instance
         * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
         * If either instance of SqlDateTime is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThanOrEqual(SqlDateTime d1, SqlDateTime d2)
        {
            return d1 >= d2;
        }

        /**
         * Compares two instances of SqlDecimal to determine if the first is less than the second.
         * @param x A SqlDateTime instance
         * @param y A SqlDateTime instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThan(SqlDateTime d1, SqlDateTime d2)
        {
            return d1 < d2;
        }

        /**
         * Compares two instances of SqlDateTime to determine if the first is less than the second.
         * @param x A SqlDateTime instance
         * @param y A SqlDateTime instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlDateTime is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThanOrEqual(SqlDateTime d1, SqlDateTime d2)
        {
            return d1 <= d2;
        }

        /**
         * Compares two instances of SqlDateTime to determine if they are equal.
         * @param x A SqlDateTime instance
         * @param y A SqlDateTime instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlDateTime d1, SqlDateTime d2)
        {
            return d1 != d2;
        }

        /**
         * Converts the String representation of a number to its DateTime equivalent.
         * @param s The String to be parsed.
         * @return A SqlDateTime containing the value represented by the String.
         */
        public static SqlDateTime Parse(String s)
        {
            DateTime val = DateTime.Parse(s);

            SqlDateTime res = new SqlDateTime(val);

            if (LessThanOrEqual(res, MinValue).IsTrue || GreaterThanOrEqual(res, MinValue).IsTrue)
                throw new ArgumentException("The DateTime is not valid");

            return res;

        }


        public override String ToString()
        {
            if (IsNull)
                return "null";

            return _value.ToString();
        }

        /**
         * Converts this SqlDateTime structure to SqlString.
         * @return A SqlString structure whose value is a string representing the date and time contained in this SqlDateTime structure.
         */
        public SqlString ToSqlString()
        {
            return new SqlString(ToString());
        }
        
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static SqlDateTime operator + (SqlDateTime x, TimeSpan t)
        {
            if (x.IsNull)
                return SqlDateTime.Null;
			
            return new SqlDateTime (x.Value + t);
        }

        public static SqlBoolean operator == (SqlDateTime x, SqlDateTime y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value == y.Value);
        }

        public static SqlBoolean operator > (SqlDateTime x, SqlDateTime y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean(x.CompareTo(y) > 0);
        }

        public static SqlBoolean operator >= (SqlDateTime x, SqlDateTime y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean(x.CompareTo(y) >= 0);
        }

        public static SqlBoolean operator != (SqlDateTime x, SqlDateTime y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean(!(x.Value == y.Value));
        }

        public static SqlBoolean operator < (SqlDateTime x, SqlDateTime y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean(x.CompareTo(y) < 0);
        }

        public static SqlBoolean operator <= (SqlDateTime x, SqlDateTime y)
        {
            if (x.IsNull || y.IsNull) 
                return SqlBoolean.Null;
            else
                return new SqlBoolean(x.CompareTo(y) <= 0);
        }

        public static SqlDateTime operator - (SqlDateTime x, TimeSpan t)
        {
            return new SqlDateTime(x.Value - t);
        }

        public static explicit operator DateTime (SqlDateTime x)
        {
            return x.Value;
        }

        public static explicit operator SqlDateTime (SqlString x)
        {
            return SqlDateTime.Parse(x.Value);
        }

        public static implicit operator SqlDateTime (DateTime x)
        {
            return new SqlDateTime(x);
        }

    }
}
