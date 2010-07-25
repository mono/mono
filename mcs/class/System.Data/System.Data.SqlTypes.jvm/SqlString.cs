// System.Data.SqlTypes.SqlString
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

    /*
    * CURRENT LIMITATIONS:
    * 1. constructor public SqlString(String, int) not supported
    * 2. constructor public SqlString(int, SqlCompareOption, byte[]) not supported
    * 3. constructor public SqlString(String, int, SqlCompareOption) not supported
    * 4. constructor public SqlString(int, SqlCompareOption, byte[], boolen) not supported
    * 5. constructor public SqlString(int, SqlCompareOption, byte[], boolen) not supported
    * 6. constructor public SqlString(int, SqlCompareOption, byte[], int, int) not supported
    * 7. field IgnoreKanaType is ignored
    * 8. field IgnoreNonSpace is ignored
    * 9. field IgnoreWidth is ignored
    * 10. any Localeinformation is ignored that is the reson for limitation 1-6
    * 11. property CompareInfo not implemented - it relates to the Locale issu
    * 12. property CultureInfo not implemented - it relates to the Locale issu
    * 12. property LCID not implemented - it relates to the Locale issu
    * 13. method GetCompareOptionsFromSqlCompareOptions not implemented - it relates to the Locale issu
    * 14. method GetNonUnicodeBytes() - not implemented. Need more information
    * 15. method GetUnicodeBytes() - implemented - not sure the right way!!!
    */


    public struct SqlString : INullable, IComparable
    {
        private String _value;
        private bool _isNull;
        private SqlCompareOptions _compareOptions;

        public static readonly SqlString Null = new SqlString();
        public static readonly int IgnoreCase = 1;
        public static readonly int IgnoreWidth = 16;
        public static readonly int IgnoreNonSpace = 2;
        public static readonly int IgnoreKanaType = 8;
        public static readonly int BinarySort = 32786;

        
        private SqlString(bool isNull)
        {
            _isNull = isNull;
            _value = String.Empty;
            _compareOptions = SqlCompareOptions.None;
        }

        /**
         * Initializes a new instance of the SqlString structure using the specified string.
         * @param value The string to store.
         */
        public SqlString(String value) 
        { 
            _value = value;
            _isNull = false;
            _compareOptions = SqlCompareOptions.None;
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

        public SqlCompareOptions CompareOptions
        {
            get
            {
                return _compareOptions;
            }
        }

        public String Value
        {
            get
            {
                if(IsNull)
                {
                    throw new SqlNullValueException();
                }
                return _value;
            }
        }

        /**
         * Creates a copy of this SqlString object.
         * @return A new SqlString object in which all property values are the same as the original.
         */
        public SqlString Clone()
        {
            SqlString clone;
            if (_value == null)
                clone = new SqlString();
            else
                clone = new SqlString(_value);

            clone._compareOptions = _compareOptions;
            return clone;
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

            if (Object.ReferenceEquals(obj, this))
                return 0;

            if (obj is SqlString)
            {
                SqlString sqlStr = (SqlString)obj;

                if (sqlStr._value == null && this._value == null)
                    return 0;

                if (sqlStr._value == null)
                    return 1;
                if (this._value == null)
                    return -1;

                if (_compareOptions == SqlCompareOptions.BinarySort ||
                    _compareOptions == SqlCompareOptions.None)
                    return String.Compare(this._value, sqlStr._value);

                if (_compareOptions == SqlCompareOptions.IgnoreCase)
                    return String.Compare(this._value, sqlStr._value, true);
            }

            throw new ArgumentException("parameter obj is not SqlString : " + obj.GetType().Name);

        }

        /**
         * Concatenates the two specified SqlString structures.
         * @param x A SqlString.
         * @param y A SqlString.
         * @return A SqlString containing the newly concatenated value representing the contents of the two SqlString parameters.
         * If any of the parameters or their value equals null the returned value is SqlString.Null.
         */
        public static SqlString Concat(SqlString x, SqlString y)
        {
            if (x.IsNull || y.IsNull)
                return SqlString.Null;

            return new SqlString(x._value + y._value);
        }


//        public bool Equals(Object obj)
//        {
//            if (Object.ReferenceEquals(obj, this))
//                return true;
//
//            if (obj is SqlString)
//            {
//                SqlString s = (SqlString)obj;
//
//                if (IsNull && s.IsNull)
//                    return true;
//
//                if (IsNull || s.IsNull)
//                    return false;
//
//                return _value.Equals(s._value);
//            }
//
//            return false;
//        }

        /**
         * Performs a logical comparison on two instances of SqlString to determine if they are equal.
         * @param x A SqlString instance.
         * @param y A SqlString instance.
         * @return true if the two values are equal, otherwise false.
         * If one of the parameters is null or null value return SqlBoolean.Null.
         */
        public static SqlBoolean Equals(SqlString x, SqlString y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x._value.Equals(y._value))
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        public override bool Equals(object obj)
        {
            if(obj is SqlString)
            {
                SqlString sqlObj = (SqlString)obj;
                if(sqlObj.IsNull && IsNull)
                    return true;
                if(sqlObj.IsNull || IsNull)
                    return false;
                return sqlObj.Value == this.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if(IsNull)
                return -1;

            return Value.GetHashCode();
        }


        /**
         * Gets an array of bytes, containing the contents of the SqlString in Unicode format.
         * @return An byte array, containing the contents of the SqlString in Unicode format.
         */
        public byte[] GetUnicodeString()
        {
            /** @todo check if it works */
            if (IsNull)
                return null;

            return System.Text.Encoding.Default.GetBytes(_value);
        }

        /**
         * Compares two instances of SqlString to determine if the first is greater than the second.
         * @param x A SqlString instance
         * @param y A SqlString instance
         * @return A SqlBoolean that is True if the first instance is greater than the second instance, otherwise False.
         * If either instance of SqlString is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThan(SqlString x, SqlString y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.CompareTo(y) > 0)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlString to determine if the first is greater than or equal to the second.
         * @param x A SqlString instance
         * @param y A SqlString instance
         * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
         * If either instance of SqlString is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean GreaterThanOrEqual(SqlString x, SqlString y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.CompareTo(y) >= 0)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlString to determine if the first is less than the second.
         * @param x A SqlString instance
         * @param y A SqlString instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlString is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThan(SqlString x, SqlString y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.CompareTo(y) < 0)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }

        /**
         * Compares two instances of SqlString to determine if the first is less than the second.
         * @param x A SqlString instance
         * @param y A SqlString instance
         * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
         * If either instance of SqlString is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean LessThanOrEqual(SqlString x, SqlString y)
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            if (x.CompareTo(y) <= 0)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }


        /**
         * Compares two instances of SqlString to determine if they are equal.
         * @param x A SqlString instance
         * @param y A SqlString instance
         * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
         * If either instance of SqlString is null, the Value of the SqlBoolean will be Null.
         */
        public static SqlBoolean NotEquals(SqlString x, SqlString y)
        {
            SqlBoolean res = Equals(x, y);

            if (res.IsNull)
                return res;
            if (res.IsFalse)
                return SqlBoolean.True;

            return SqlBoolean.False;
        }


        /**
         * Converts this SqlString structure to SqlBoolean.
         * @return A SqlBoolean structure whose Value will be True if the SqlString structure's Value is non-zero, False if the SqlString is zero
         * and Null if the SqlString structure is Null.
         */
        public SqlBoolean ToSqlBoolean()
        {
            return SqlBoolean.Parse(_value);
        }

        /**
         * Converts this SqlString structure to SqlByte.
         * @return A SqlByte structure whose Value equals the Value of this SqlString structure.
         */
        public SqlByte ToSqlByte()
        {
            return SqlByte.Parse(_value);
        }

        /**
         * Converts this SqlString structure to SqlDecimal.
         * @return A SqlDecimal structure whose Value equals the Value of this SqlString structure.
         */
        public SqlDecimal ToSqlDecimal()
        {
            return SqlDecimal.Parse(_value);
        }

        /**
         * Converts this SqlString structure to SqlDouble.
         * @return A SqlDouble structure whose Value equals the Value of this SqlString structure.
         */
        public SqlDouble ToSqlDouble()
        {
            return SqlDouble.Parse(_value);
        }

        /**
         * Converts this SqlString structure to SqlSingle.
         * @return A SqlDouble structure whose Value equals the Value of this SqlString structure.
         */
        public SqlSingle ToSqlSingle()
        {
            return SqlSingle.Parse(_value);
        }

        /**
         * Converts this SqlString structure to SqlDateTime.
         * @return A SqlDateTime structure whose Value equals the Value of this SqlString structure.
         */
        public SqlDateTime ToSqlDateTime()
        {
            return SqlDateTime.Parse(_value);
        }

        /**
         * Converts this SqlString structure to SqlInt16.
         * @return A SqlInt16 structure whose Value equals the Value of this SqlString structure.
         */
        public SqlInt16 ToSqlInt16()
        {
            return SqlInt16.Parse(_value);
        }

        /**
         * Converts this SqlString structure to SqlInt32.
         * @return A SqlInt32 structure whose Value equals the Value of this SqlString structure.
         */
        public SqlInt32 ToSqlInt32()
        {
            return SqlInt32.Parse(_value);
        }

        /**
         * Converts this SqlString structure to SqlInt64.
         * @return A SqlInt64 structure whose Value equals the Value of this SqlString structure.
         */
        public SqlInt64 ToSqlInt64()
        {
            return SqlInt64.Parse(_value);
        }

        /**
         * Converts this SqlString instance to SqlString.
         * @return A SqlMoney instance whose Value equals the Value of this SqlString instance.
         */
        public SqlMoney ToSqlMoney()
        {
            return SqlMoney.Parse(_value);
        }

        public override String ToString()
        {
            if(IsNull)
                return "null";
            return _value.ToString();
        }

        // Concatenates
        public static SqlString operator + (SqlString x, SqlString y) 
        {
            if (x.IsNull || y.IsNull)
                return SqlString.Null;

            return new SqlString (x.Value + y.Value);
        }

        // Equality
        public static SqlBoolean operator == (SqlString x, SqlString y) 
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value == y.Value);
        }

        // Greater Than
        public static SqlBoolean operator > (SqlString x, SqlString y) 
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.CompareTo (y) > 0);
        }

        // Greater Than Or Equal
        public static SqlBoolean operator >= (SqlString x, SqlString y) 
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.CompareTo (y) >= 0);
        }

        public static SqlBoolean operator != (SqlString x, SqlString y) 
        { 
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.Value != y.Value);
        }

        // Less Than
        public static SqlBoolean operator < (SqlString x, SqlString y) 
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.CompareTo (y) < 0);
        }

        // Less Than Or Equal
        public static SqlBoolean operator <= (SqlString x, SqlString y) 
        {
            if (x.IsNull || y.IsNull)
                return SqlBoolean.Null;
            else
                return new SqlBoolean (x.CompareTo (y) <= 0);
        }

        // **************************************
        // Type Conversions
        // **************************************

        public static explicit operator SqlString (SqlBoolean x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator SqlString (SqlByte x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator SqlString (SqlDateTime x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator SqlString (SqlDecimal x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator SqlString (SqlDouble x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator SqlString (SqlGuid x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator SqlString (SqlInt16 x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator SqlString (SqlInt32 x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator SqlString (SqlInt64 x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator SqlString (SqlMoney x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator SqlString (SqlSingle x) 
        {
            if (x.IsNull)
                return Null;
            else
                return new SqlString (x.Value.ToString ());
        }

        public static explicit operator string (SqlString x) 
        {
            return x.Value;
        }

        public static implicit operator SqlString (string x) 
        {
            return new SqlString (x);
        }
    }}