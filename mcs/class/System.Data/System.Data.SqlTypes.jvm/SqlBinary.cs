// System.Data.SqlTypes.SqlBinary
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
    //using clr.exceptions;
    //using clr.compiler.BitConstants;

    /**
     *
     */
    public struct SqlBinary : INullable, IComparable
    {
        public static readonly SqlBinary Null = new SqlBinary(true);
        private bool _isNull;
        private byte[] _value;
        
        private SqlBinary(bool isNull)
        {
            _isNull = isNull;
            _value = null;
        }
        
        /**
         * Initializes a new instance of the SqlBinary instance,
         * setting the Value property to the contents of the supplied byte array.
         * @param value The byte array to be stored or retrieved.
         */
        public SqlBinary(byte[] value)
        {
            if (value != null && value.Length > 0)
            {
                _value = new byte[value.Length];
                Array.Copy (value, 0, _value, 0, value.Length);
            }
            else
                _value = new byte[0];
            
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
         * Gets the single byte from the Value property located at the position indicated by the integer parameter, index.
         * If index indicates a position beyond the end of the byte array, a SqlNullValueException will be raised.
         * @param index The position of the byte to be retrieved.
         * @return The byte located at the position indicated by the integer parameter.
         */
        public int this[int index]
        {
            get
            {
                if (IsNull)
                {
                    throw new SqlNullValueException();
                }
                if (index >= _value.Length)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return _value[index];// & BitConstants.ALL_BYTE;
            }
        }

        /**
         * Gets the length in bytes of the Value property.
         * @return The length of the binary data in the Value property.
         */
        public int Length
        {
            get
            {
                if (IsNull)
                {
                    throw new SqlNullValueException();
                }
                return _value.Length;
            }
        }

        /**
         * Gets the value of the SqlBinary instance.
         * @return the value of this instance
         */
        public byte[] Value
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
         * Compares this instance to the supplied object and returns an indication of their relative values.
         * @param obj The object to compare.
         * @return A signed number indicating the relative values of the instance and the object.
         * Less than zero This instance is less than object.
         * Zero This instance is the same as object.
         * Greater than zero This instance is greater than object -or-
         * object is a null reference.
         */
        public int CompareTo(Object value)
        {
            if (value == null)
                return 1;

            if ((value is SqlBinary) == false)
            {
                throw new ArgumentException("Wrong value type " + 
                    value.GetType().Name + "in SqlBinary.CompareTo");
            }

            SqlBinary obj = (SqlBinary)value;

            if (this.IsNull)
            {
                if (obj.IsNull)
                    return 0;
                return -1;
            }
            else if (obj.IsNull)
                return 1;

            int length = _value.Length > obj._value.Length ? _value.Length : obj._value.Length;

            for (int i = 0; i < length; i++)
            {
                if (_value[i] > obj._value[i])
                    return 1;
                if (_value[i] < obj._value[i])
                    return -1;
            }

            if (_value.Length > obj._value.Length)
                return 1;
            if (_value.Length < obj._value.Length)
                return -1;

            return 0;

        }

        /**
         * Concatenates two SqlBinary instances to create a new SqlBinary instance.
         * @param x A SqlBinary instance.
         * @param y A SqlBinary instance.
         * @return The concatenated values of the x and y parameters.
         */
        public static SqlBinary Concat(SqlBinary x, SqlBinary y)
        {
            if (x.IsNull)
            {
                if (y.IsNull)
                {
                    return SqlBinary.Null;
                }
                else
                    return new SqlBinary((byte[])y._value);
            }
            else if (y.IsNull)
                return new SqlBinary((byte[])x._value);

            byte[] newVal = new byte[x.Length + y.Length];

            java.lang.System.arraycopy(x, 0, newVal, 0, x.Length);
            java.lang.System.arraycopy(y, 0, newVal, x.Length, y.Length);

            return new SqlBinary(newVal);

        }

        /**
         * Performs a logical comparison on two instances of SqlBinary to determine if they are equal.
         * @param x A SqlBinary instance.
         * @param y A SqlBinary instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlBinary x, SqlBinary y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.Equals(y))
                return SqlBoolean.True;

            return SqlBoolean.False;

        }

        public bool equals(Object obj)
        {
            if (obj == null)
                return false;

            if (obj is SqlBinary)
            {
                SqlBinary o = (SqlBinary)obj;

                if (IsNull && o.IsNull)
                    return true;

                if (IsNull || o.IsNull)
                    return false;

                if (_value.Length != o._value.Length)
                    return false;

                for (int i = 0; i < _value.Length; i++)
                {
                    if (_value[i] != o._value[i])
                        return false;
                }

                return true;
            }

            return false;
        }

        /**
         * Compares two instances of SqlBinary to determine if the first is greater than the second.
         * @param x A SqlBinary instance
         * @param y A SqlBinary instance
         * @return A SqlBoolean that is True if the first instance is greater than the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThan(SqlBinary x, SqlBinary y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            int i = x.CompareTo(y);

            if ( i > 0)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlBinary to determine if the first is greater than or equal to the second.
         * @param x A SqlBinary instance
         * @param y A SqlBinary instance
         * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThanOrEqual(SqlBinary x, SqlBinary y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            int i = x.CompareTo(y);

            if ( i < 0)
                return SqlBoolean.False;

            return SqlBoolean.True;
        }

        /**
         * Compares two instances of SqlBinary to determine if the first is less than the second.
         * @param x A SqlBinary instance
         * @param y A SqlBinary instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThan(SqlBinary x, SqlBinary y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            int i = x.CompareTo(y);

            if ( i < 0)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlBinary to determine if the first is less than or equal to the second.
         * @param x A SqlBinary instance
         * @param y A SqlBinary instance
         * @return A SqlBoolean that is True if the first instance is less than or equal to the second instance, otherwise False.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThanOrEqual(SqlBinary x, SqlBinary y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            int i = x.CompareTo(y);

            if ( i > 0)
                return SqlBoolean.False;

            return SqlBoolean.True;
        }

        /**
         * Compares two instances of SqlBinary to determine if they are equal.
         * @param x A SqlBinary instance
         * @param y A SqlBinary instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlBinary x, SqlBinary y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.Equals(y))
                return SqlBoolean.False;

            return SqlBoolean.True;
        }

        public String toString()
        {
            if (IsNull)
                return "null";

            return "SqlBinary(" + _value.ToString() + ")";
        }

        public static SqlBinary op_Implicit(byte[] x)
        {
            return new SqlBinary(x);
        }

        public static byte[] op_Explicit(SqlBinary x)
        {
            return x.Value;
        }

        public static SqlBinary op_Addition(SqlBinary x, SqlBinary y)
        {
            throw new NotImplementedException("The method op_Addition in class SqlBinary is not supported");
        }

        public static SqlBinary op_Explicit(SqlGuid x)
        {
            if(x.IsNull)
            {
                return SqlBinary.Null;
            }

            return new SqlBinary(x.ToByteArray());
        }

        public static SqlBoolean op_Equality(SqlBinary x, SqlBinary y)
        {
            return Equals(x, y);
        }

        public static SqlBoolean op_Inequality(SqlBinary x, SqlBinary y)
        {
            return NotEquals(x, y);
        }

        public static SqlBoolean op_LessThan(SqlBinary x, SqlBinary y)
        {
            return LessThan(x, y);
        }

        public static SqlBoolean op_GreaterThan(SqlBinary x, SqlBinary y)
        {
            return GreaterThan(x, y);
        }

        public static SqlBoolean op_LessThanOrEqual(SqlBinary x, SqlBinary y)
        {
            return LessThanOrEqual(x, y);
        }

        public static SqlBoolean op_GreaterThanOrEqual(SqlBinary x, SqlBinary y)
        {
            return GreaterThanOrEqual(x, y);
        }

        public SqlGuid ToSqlGuid()
        {
            return SqlGuid.op_Explicit(this);
        }
    }}