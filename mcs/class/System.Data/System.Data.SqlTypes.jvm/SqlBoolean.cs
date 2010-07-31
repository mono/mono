// System.Data.SqlTypes.SqlBoolean
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

    public struct SqlBoolean : INullable, IComparable
    {
        private bool _value;
        private bool _isNull;

        public static readonly SqlBoolean Null = new SqlBoolean(true, false);
        public static readonly SqlBoolean True = new SqlBoolean(true);
        public static readonly SqlBoolean False = new SqlBoolean(false);
        public static readonly SqlBoolean One = new SqlBoolean(1);
        public static readonly SqlBoolean Zero = new SqlBoolean(0);

        private SqlBoolean(bool isNull, bool value)
        {
            _isNull = isNull;
            _value = value;
        }
        /**
         * Initializes a new instance of the SqlBoolean instance using the supplied bool value.
         * @param value The value for the new SqlBoolean instance; either true or false.
         */
        public SqlBoolean(bool value) 
        {
            _value = value;
            _isNull = false;
        }

        /**
         * Initializes a new instance of the SqlBoolean instance using the specified integer value.
         * @param value The integer whose value is to be used for the new SqlBoolean instance.
         */
        public SqlBoolean(int value)
        {
            if (value == 0)
                _value = false;
            else
                _value = true;

            _isNull = false;
        }

        

        public int hashCode()
        {
            if (IsNull)
                return 0;
        
            return _value.GetHashCode();
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
         * Gets the value of the SqlBoolean instance as a byte.
         * @return A byte representing the value of the SqlBoolean instance.
         * Byte value will be 1 or 0.
         */
        public int ByteValue
        {
            get
            {
                if (IsNull)
                {
                    throw new SqlNullValueException();
                }

                if (_value)
                    return 1;

                return 0;
            }
        }

        /**
         * Indicates whether the current Value is False.
         * @return true if Value is False, otherwise false.
         */
        public bool IsFalse
        {
            get
            {
                return Value == false;
            }
        }

        /**
         * Indicates whether the current Value is True.
         * @return true if Value is True, otherwise false.
         */
        public bool IsTrue
        {
            get
            {
                return Value == true;
            }
        }

        /**
         * Gets the value of the SqlBoolean instance.
         * @return the value of this instance
         */
        public bool Value
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
         * Computes the bitwise AND of two specified SqlBoolean instances.
         * @param x A SqlBoolean instance
         * @param y A SqlBoolean instance
         * @return The result of the logical AND operation.
         */
        public static SqlBoolean And(SqlBoolean x, SqlBoolean y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            bool res = x.IsTrue && y.IsTrue;
            return new SqlBoolean(res);
        }

        /**
         * Performs a bitwise OR operation on the two specified SqlBoolean instances.
         * @param x A SqlBoolean instance
         * @param y A SqlBoolean instance
         * @return The result of the logical OR operation.
         */
        public static SqlBoolean Or(SqlBoolean x, SqlBoolean y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            bool res = x.IsTrue || y.IsTrue;
            return new SqlBoolean(res);
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

            if ((obj is SqlBoolean) == false)
            {
                throw new ArgumentException("Wrong value type " + 
                    obj.GetType().Name + "in SqlBoolean.CompareTo");
            }

            SqlBoolean val = (SqlBoolean)obj;

            if (this.IsNull)
            {
                if (val.IsNull)
                    return 0;
                return -1;
            }
            else if (val.IsNull)
                return 1;

            if (this.IsTrue && val.IsFalse)
                return 1;

            if (this.IsFalse && val.IsTrue)
                return -1;
        
            return 0;
        }

        /**
         * Compares the supplied object parameter to the Value property of the SqlBoolean object.
         * @param obj The object to be compared.
         * @return true if object is an instance of SqlBoolean and the two are equal; otherwise false.
         */
        public bool equals(Object obj)
        {
            if (obj == null)
                return false;

            if (obj is SqlBoolean)
            {
                SqlBoolean val = (SqlBoolean)obj;

                if (IsNull && val.IsNull)
                    return true;

                if (IsNull || val.IsNull)
                    return false;

                return val._value == _value;
            }

            return false;
        }

        /**
         * Performs a logical comparison on two instances of SqlBoolean to determine if they are equal.
         * @param x A SqlBoolean instance.
         * @param y A SqlBoolean instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlBoolean x, SqlBoolean y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(x._value == y._value);
        }

        /**
         * Compares two instances of SqlBoolean to determine if they are equal.
         * @param x A SqlBoolean instance
         * @param y A SqlBoolean instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlBoolean x, SqlBoolean y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(x._value != y._value);
        }

        public static SqlBoolean OnesComplement(SqlBoolean x)
        {
            if (x.IsNull)
            {
                throw new SqlNullValueException();
            }
            return new SqlBoolean(x._value == false);
        }

        /**
         * Converts the String representation of a number to its bool equivalent.
         * @param s The String to be parsed.
         * @return A SqlBoolean containing the value represented by the String.
         */
        public static SqlBoolean Parse(String s)
        {
            int val = int.Parse(s);
            return new SqlBoolean(val);
        }

        /**
         * Performs a bitwise XOR operation on the two specified SqlBoolean instances.
         * @param x A SqlBoolean instance
         * @param y A SqlBoolean instance
         * @return The result of the logical XOR operation.
         */
        public static SqlBoolean Xor(SqlBoolean x, SqlBoolean y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.IsTrue && y.IsTrue)
                return new SqlBoolean(false);

            if (x.IsTrue || y.IsTrue)
                return new SqlBoolean(true);

            return new SqlBoolean(false); // both args are False

        }

        /**
         * Converts this SqlBoolean instance to SqlByte.
         * @return A SqlByte instance whose Value equals the Value of this SqlDouble instance.
         */
        public SqlByte ToSqlByte()
        {
            if (IsNull)
                return SqlByte.Null;

            return new SqlByte((byte)ByteValue);
        }

        /**
         * Converts this SqlBoolean instance to SqlDecimal.
         * @return A SqlDecimal instance whose Value equals the Value of this SqlBoolean instance.
         */
        public SqlDecimal ToSqlDecimal()
        {
            if (IsNull)
                return SqlDecimal.Null;

            return new SqlDecimal(ByteValue);
        }

        /**
         * Converts this SqlBoolean instance to SqlDouble.
         * @return A SqlDouble instance whose Value equals the Value of this SqlBoolean instance.
         */
        public SqlDouble ToSqlDouble()
        {
            if (IsNull)
                return SqlDouble.Null;

            return new SqlDouble(ByteValue);
        }

        /**
         * Converts this SqlBoolean instance to SqlSingle.
         * @return A SqlSingle instance whose Value equals the Value of this SqlBoolean instance.
         */
        public SqlSingle ToSqlSingle()
        {
            if (IsNull)
                return SqlSingle.Null;

            return new SqlSingle(ByteValue);
        }

        /**
         * Converts this SqlBoolean instance to SqlDouble.
         * @return A SqlDouble instance whose Value equals the Value of this SqlBoolean instance.
         */
        public SqlInt16 ToSqlInt16()
        {
            if (IsNull)
                return SqlInt16.Null;

            return new SqlInt16(ByteValue);
        }

        /**
         * Converts this SqlBoolean instance to SqlDouble.
         * @return A SqlDouble instance whose Value equals the Value of this SqlBoolean instance.
         */
        public SqlInt32 ToSqlInt32()
        {
            if (IsNull)
                return SqlInt32.Null;

            return new SqlInt32(ByteValue);
        }

        /**
         * Converts this SqlBoolean instance to SqlDouble.
         * @return A SqlDouble instance whose Value equals the Value of this SqlBoolean instance.
         */
        public SqlInt64 ToSqlInt64()
        {
            if (IsNull)
                return SqlInt64.Null;

            return new SqlInt64(ByteValue);
        }

        /**
         * Converts this SqlBoolean instance to SqlDecimal.
         * @return A SqlMoney instance whose Value equals the Value of this SqlBoolean instance.
         */
        public SqlMoney ToSqlMoney()
        {
            if (IsNull)
                return SqlMoney.Null;

            return new SqlMoney(ByteValue);
        }

        /**
         * Converts this SqlBoolean structure to SqlString.
         * @return A SqlString structure whose value is a string representing the date and time contained in this SqlBoolean structure.
         */
        public SqlString ToSqlString()
        {
            if (IsNull)
                return SqlString.Null;

            return new SqlString(ToString());
        }

        public override String ToString()
        {
            if (IsNull)
                return "null";

            return _value.ToString();
        }

        public static SqlBoolean op_Implicit(bool x)
        {
            return new SqlBoolean(x);
        }

        public static bool op_Explicit(SqlBoolean x)
        {
            return x.Value;
        }

        public static SqlBoolean op_LogicalNot(SqlBoolean x)
        {
            return OnesComplement(x);
        }

        public static bool op_True(SqlBoolean x)
        {
            return x.IsTrue;
        }

        public static bool op_False(SqlBoolean x)
        {
            return x.IsFalse;
        }

        public static SqlBoolean op_BitwiseAnd(SqlBoolean x, SqlBoolean y)
        {
            return And(x, y);
        }

        public static SqlBoolean op_BitwiseOr(SqlBoolean x, SqlBoolean y)
        {
            return Or(x, y);
        }

        public static SqlBoolean op_OnesComplement(SqlBoolean x)
        {
            return OnesComplement(x);
        }

        public static SqlBoolean op_ExclusiveOr(SqlBoolean x, SqlBoolean y)
        {
            return Xor(x, y);
        }

        public static SqlBoolean op_Explicit(SqlByte x)
        {
            if (x.IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value != 0);
        }

        public static SqlBoolean op_Explicit(SqlInt16 x)
        {
            if (x.IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value != 0);
        }

        public static SqlBoolean op_Explicit(SqlInt32 x)
        {
            if (x.IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value != 0);
        }

        public static SqlBoolean op_Explicit(SqlInt64 x)
        {
            if (x.IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value != 0L);
        }

        public static SqlBoolean op_Explicit(SqlDouble x)
        {
            if (x.IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value != 0.0D);
        }

        public static SqlBoolean op_Explicit(SqlSingle x)
        {
            if (x.IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value != 0.0F);
        }

        public static SqlBoolean op_Explicit(SqlMoney x)
        {
            if (x.IsNull)
                return SqlBoolean.Null;

            return SqlMoney.NotEquals(x, SqlMoney.Zero);
        }

        public static SqlBoolean op_Explicit(SqlDecimal x)
        {
            if (x.IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(x.ToDouble() != 0.0D);
        }

        public static SqlBoolean op_Explicit(SqlString x)
        {
            if (x.IsNull)
                return SqlBoolean.Null;

            return Parse(x.Value);
        }

        public static SqlBoolean op_Equality(SqlBoolean x, SqlBoolean y)
        {
            return Equals(x, y);
        }

        public static SqlBoolean op_Inequality(SqlBoolean x, SqlBoolean y)
        {
            return NotEquals(x, y);
        }
    }}