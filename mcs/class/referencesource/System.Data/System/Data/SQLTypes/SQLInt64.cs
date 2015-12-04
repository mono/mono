//------------------------------------------------------------------------------
// <copyright file="SqlInt64.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
// <owner current="true" primary="true">junfang</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

//**************************************************************************
// @File: SqlInt64.cs
//
// Create by:    JunFang
//
// Purpose: Implementation of SqlInt64 which is equivalent to
//            data type "bigint" in SQL Server
//
// Notes:
//
// History:
//
//   10/28/99  JunFang    Created.
//
// @EndHeader@
//**************************************************************************

using System;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes {

    /// <devdoc>
    ///    <para>
    ///       Represents a 64-bit signed integer to be stored in
    ///       or retrieved from a database.
    ///    </para>
    /// </devdoc>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [XmlSchemaProvider("GetXsdType")]
    public struct SqlInt64 : INullable, IComparable, IXmlSerializable {
        private bool m_fNotNull; // false if null
        private long m_value;

        private static readonly long x_lLowIntMask  = 0xffffffff;
        private static readonly long x_lHighIntMask = unchecked((long)0xffffffff00000000);


        // constructor
        // construct a Null
        private SqlInt64(bool fNull) {
            m_fNotNull = false;
            m_value = 0;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SqlInt64(long value) {
            m_value = value;
            m_fNotNull = true;
        }

        // INullable
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsNull {
            get { return !m_fNotNull;}
        }

        // property: Value
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public long Value {
            get {
                if (m_fNotNull)
                    return m_value;
                else
                    throw new SqlNullValueException();
            }
        }

        // Implicit conversion from long to SqlInt64
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static implicit operator SqlInt64(long x) {
            return new SqlInt64(x);
        }

        // Explicit conversion from SqlInt64 to long. Throw exception if x is Null.
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static explicit operator long(SqlInt64 x) {
            return x.Value;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override String ToString() {
            return IsNull ? SQLResource.NullString : m_value.ToString((IFormatProvider)null);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 Parse(String s) {
            if (s == SQLResource.NullString)
                return SqlInt64.Null;
            else 
                return new SqlInt64(Int64.Parse(s, (IFormatProvider)null));
        }


        // Unary operators
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 operator -(SqlInt64 x) {
            return x.IsNull ? Null : new SqlInt64(-x.m_value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 operator ~(SqlInt64 x) {
            return x.IsNull ? Null : new SqlInt64(~x.m_value);
        }


        // Binary operators

        // Arithmetic operators
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 operator +(SqlInt64 x, SqlInt64 y) {
            if (x.IsNull || y.IsNull)
                return Null;

            long lResult = x.m_value + y.m_value;
            if (SameSignLong(x.m_value, y.m_value) && !SameSignLong(x.m_value, lResult))
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            else
                return new SqlInt64(lResult);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 operator -(SqlInt64 x, SqlInt64 y) {
            if (x.IsNull || y.IsNull)
                return Null;

            long lResult = x.m_value - y.m_value;
            if (!SameSignLong(x.m_value, y.m_value) && SameSignLong(y.m_value, lResult))
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            else
                return new SqlInt64(lResult);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 operator *(SqlInt64 x, SqlInt64 y) {
            if (x.IsNull || y.IsNull)
                return Null;

            bool    fNeg = false;

            long    lOp1 = x.m_value;
            long    lOp2 = y.m_value;
            long    lResult;
            long    lPartialResult = 0;

            if (lOp1 < 0) {
                fNeg = true;
                lOp1 = - lOp1;
            }

            if (lOp2 < 0) {
                fNeg = !fNeg;
                lOp2 = - lOp2;
            }

            long    lLow1   = lOp1 & x_lLowIntMask;
            long    lHigh1  = (lOp1 >> 32) & x_lLowIntMask;
            long    lLow2   = lOp2 & x_lLowIntMask;
            long    lHigh2  = (lOp2 >> 32) & x_lLowIntMask;

            // if both of the high order dwords are non-zero then overflow results
            if (lHigh1 != 0 && lHigh2 != 0)
                throw new OverflowException(SQLResource.ArithOverflowMessage);

            lResult = lLow1 * lLow2;

            if (lResult < 0)
                throw new OverflowException(SQLResource.ArithOverflowMessage);

            if (lHigh1 != 0) {
                SQLDebug.Check(lHigh2 == 0);
                lPartialResult = lHigh1 * lLow2;
                if (lPartialResult < 0 || lPartialResult > Int64.MaxValue)
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
            }
            else if (lHigh2 != 0) {
                SQLDebug.Check(lHigh1 == 0);
                lPartialResult = lLow1 * lHigh2;
                if (lPartialResult < 0 || lPartialResult > Int64.MaxValue)
                    throw new OverflowException(SQLResource.ArithOverflowMessage);
            }

            lResult += lPartialResult << 32;
            if (lResult < 0)
                throw new OverflowException(SQLResource.ArithOverflowMessage);

            if (fNeg)
                lResult = - lResult;

            return new SqlInt64(lResult);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 operator /(SqlInt64 x, SqlInt64 y) {
            if (x.IsNull || y.IsNull)
                return Null;

            if (y.m_value != 0) {
                if ((x.m_value == Int64.MinValue) && (y.m_value == -1))
                    throw new OverflowException(SQLResource.ArithOverflowMessage);

                return new SqlInt64(x.m_value / y.m_value);
            }
            else
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 operator %(SqlInt64 x, SqlInt64 y) {
            if (x.IsNull || y.IsNull)
                return Null;

            if (y.m_value != 0) {
                if ((x.m_value == Int64.MinValue) && (y.m_value == -1))
                    throw new OverflowException(SQLResource.ArithOverflowMessage);

                return new SqlInt64(x.m_value % y.m_value);
            }
            else
                throw new DivideByZeroException(SQLResource.DivideByZeroMessage);
        }

        // Bitwise operators
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 operator &(SqlInt64 x, SqlInt64 y) {
            return(x.IsNull || y.IsNull) ? Null : new SqlInt64(x.m_value & y.m_value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 operator |(SqlInt64 x, SqlInt64 y) {
            return(x.IsNull || y.IsNull) ? Null : new SqlInt64(x.m_value | y.m_value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlInt64 operator ^(SqlInt64 x, SqlInt64 y) {
            return(x.IsNull || y.IsNull) ? Null : new SqlInt64(x.m_value ^ y.m_value);
        }


        // Implicit conversions

        // Implicit conversion from SqlBoolean to SqlInt64
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static explicit operator SqlInt64(SqlBoolean x) {
            return x.IsNull ? Null : new SqlInt64((long)x.ByteValue);
        }

        // Implicit conversion from SqlByte to SqlInt64
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static implicit operator SqlInt64(SqlByte x) {
            return x.IsNull ? Null : new SqlInt64((long)(x.Value));
        }

        // Implicit conversion from SqlInt16 to SqlInt64
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static implicit operator SqlInt64(SqlInt16 x) {
            return x.IsNull ? Null : new SqlInt64((long)(x.Value));
        }

        // Implicit conversion from SqlInt32 to SqlInt64
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static implicit operator SqlInt64(SqlInt32 x) {
            return x.IsNull ? Null : new SqlInt64((long)(x.Value));
        }


        // Explicit conversions

        // Explicit conversion from SqlSingle to SqlInt64
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static explicit operator SqlInt64(SqlSingle x) {
            if (x.IsNull)
                return Null;

            float value = x.Value;
            if (value > (float)Int64.MaxValue || value < (float)Int64.MinValue)
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            else
                return new SqlInt64((long)value);
        }

        // Explicit conversion from SqlDouble to SqlInt64
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static explicit operator SqlInt64(SqlDouble x) {
            if (x.IsNull)
                return Null;

            double value = x.Value;
            if (value > (double)Int64.MaxValue || value < (double)Int64.MinValue)
                throw new OverflowException(SQLResource.ArithOverflowMessage);
            else
                return new SqlInt64((long)value);
        }

        // Explicit conversion from SqlMoney to SqlInt64
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static explicit operator SqlInt64(SqlMoney x) {
            return x.IsNull ? Null : new SqlInt64(x.ToInt64());
        }

        // Explicit conversion from SqlDecimal to SqlInt64
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static explicit operator SqlInt64(SqlDecimal x) {
            if (x.IsNull)
                return SqlInt64.Null;

            SqlDecimal  ssnumTemp = x;
            long        llRetVal;

            // Throw away decimal portion
            ssnumTemp.AdjustScale (-ssnumTemp.m_bScale, false);

            // More than 8 bytes of data will always overflow
            if (ssnumTemp.m_bLen > 2)
                throw new OverflowException(SQLResource.ConversionOverflowMessage);

            // If 8 bytes of data, see if fits in LONGLONG
            if (ssnumTemp.m_bLen == 2) {
                ulong dwl = SqlDecimal.DWL(ssnumTemp.m_data1, ssnumTemp.m_data2);
                if (dwl > SqlDecimal.x_llMax && (ssnumTemp.IsPositive || dwl != 1 + SqlDecimal.x_llMax))
                    throw new OverflowException(SQLResource.ConversionOverflowMessage);
                llRetVal = (long) dwl;
            }
            // 4 bytes of data always fits in a LONGLONG
            else
                llRetVal = (long) ssnumTemp.m_data1;

            //negate result if ssnumTemp negative
            if (!ssnumTemp.IsPositive)
                llRetVal = -llRetVal;

            return new SqlInt64(llRetVal);
        }

        // Explicit conversion from SqlString to SqlInt
        // Throws FormatException or OverflowException if necessary.
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static explicit operator SqlInt64(SqlString x) {
            return x.IsNull ? Null : new SqlInt64(Int64.Parse(x.Value, (IFormatProvider)null));
        }

        // Utility functions
        private static bool SameSignLong(long x, long y) {
            return((x ^ y) & unchecked((long)0x8000000000000000L)) == 0;
        }

        // Overloading comparison operators
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlBoolean operator==(SqlInt64 x, SqlInt64 y) {
            return(x.IsNull || y.IsNull) ? SqlBoolean.Null : new SqlBoolean(x.m_value == y.m_value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlBoolean operator!=(SqlInt64 x, SqlInt64 y) {
            return ! (x == y);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlBoolean operator<(SqlInt64 x, SqlInt64 y) {
            return(x.IsNull || y.IsNull) ? SqlBoolean.Null : new SqlBoolean(x.m_value < y.m_value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlBoolean operator>(SqlInt64 x, SqlInt64 y) {
            return(x.IsNull || y.IsNull) ? SqlBoolean.Null : new SqlBoolean(x.m_value > y.m_value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlBoolean operator<=(SqlInt64 x, SqlInt64 y) {
            return(x.IsNull || y.IsNull) ? SqlBoolean.Null : new SqlBoolean(x.m_value <= y.m_value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SqlBoolean operator>=(SqlInt64 x, SqlInt64 y) {
            return(x.IsNull || y.IsNull) ? SqlBoolean.Null : new SqlBoolean(x.m_value >= y.m_value);
        }

        //--------------------------------------------------
        // Alternative methods for overloaded operators
        //--------------------------------------------------

        // Alternative method for operator ~
        public static SqlInt64 OnesComplement(SqlInt64 x) {
            return ~x;
        }

        // Alternative method for operator +
        public static SqlInt64 Add(SqlInt64 x, SqlInt64 y) {
            return x + y;
        }
        // Alternative method for operator -
        public static SqlInt64 Subtract(SqlInt64 x, SqlInt64 y) {
            return x - y;
        }

        // Alternative method for operator *
        public static SqlInt64 Multiply(SqlInt64 x, SqlInt64 y) {
            return x * y;
        }

        // Alternative method for operator /
        public static SqlInt64 Divide(SqlInt64 x, SqlInt64 y) {
            return x / y;
        }

        // Alternative method for operator %
        public static SqlInt64 Mod(SqlInt64 x, SqlInt64 y) {
            return x % y;
        }
        
        public static SqlInt64 Modulus(SqlInt64 x, SqlInt64 y) {
            return x % y;
        }

        // Alternative method for operator &
        public static SqlInt64 BitwiseAnd(SqlInt64 x, SqlInt64 y) {
            return x & y;
        }

        // Alternative method for operator |
        public static SqlInt64 BitwiseOr(SqlInt64 x, SqlInt64 y) {
            return x | y;
        }

        // Alternative method for operator ^
        public static SqlInt64 Xor(SqlInt64 x, SqlInt64 y) {
            return x ^ y;
        }

        // Alternative method for operator ==
        public static SqlBoolean Equals(SqlInt64 x, SqlInt64 y) {
            return (x == y);
        }

        // Alternative method for operator !=
        public static SqlBoolean NotEquals(SqlInt64 x, SqlInt64 y) {
            return (x != y);
        }

        // Alternative method for operator <
        public static SqlBoolean LessThan(SqlInt64 x, SqlInt64 y) {
            return (x < y);
        }

        // Alternative method for operator >
        public static SqlBoolean GreaterThan(SqlInt64 x, SqlInt64 y) {
            return (x > y);
        }

        // Alternative method for operator <=
        public static SqlBoolean LessThanOrEqual(SqlInt64 x, SqlInt64 y) {
            return (x <= y);
        }

        // Alternative method for operator >=
        public static SqlBoolean GreaterThanOrEqual(SqlInt64 x, SqlInt64 y) {
            return (x >= y);
        }

        // Alternative method for conversions.

        public SqlBoolean ToSqlBoolean() {
            return (SqlBoolean)this;
        }

        public SqlByte ToSqlByte() {
            return (SqlByte)this;
        }

        public SqlDouble ToSqlDouble() {
            return (SqlDouble)this;
        }

        public SqlInt16 ToSqlInt16() {
            return (SqlInt16)this;
        }

        public SqlInt32 ToSqlInt32() {
            return (SqlInt32)this;
        }

        public SqlMoney ToSqlMoney() {
            return (SqlMoney)this;
        }

        public SqlDecimal ToSqlDecimal() {
            return (SqlDecimal)this;
        }

        public SqlSingle ToSqlSingle() {
            return (SqlSingle)this;
        }

        public SqlString ToSqlString() {
            return (SqlString)this;
        }


        // IComparable
        // Compares this object to another object, returning an integer that
        // indicates the relationship.
        // Returns a value less than zero if this < object, zero if this = object,
        // or a value greater than zero if this > object.
        // null is considered to be less than any instance.
        // If object is not of same type, this method throws an ArgumentException.
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int CompareTo(Object value) {
            if (value is SqlInt64) {
                SqlInt64 i = (SqlInt64)value;

                return CompareTo(i);
            }
            throw ADP.WrongType(value.GetType(), typeof(SqlInt64));
        }

        public int CompareTo(SqlInt64 value) {
            // If both Null, consider them equal.
            // Otherwise, Null is less than anything.
            if (IsNull)
                return value.IsNull ? 0  : -1;
            else if (value.IsNull)
                return 1;

            if (this < value) return -1;
            if (this > value) return 1;
            return 0;
        }

        // Compares this instance with a specified object
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool Equals(Object value) {
            if (!(value is SqlInt64)) {
                return false;
            }

            SqlInt64 i = (SqlInt64)value;

            if (i.IsNull || IsNull)
                return (i.IsNull && IsNull);
            else
                return (this == i).Value;
        }

        // For hashing purpose
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode() {
            return IsNull ? 0 : Value.GetHashCode();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        XmlSchema IXmlSerializable.GetSchema() { return null; }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void IXmlSerializable.ReadXml(XmlReader reader) {
            string isNull = reader.GetAttribute("nil", XmlSchema.InstanceNamespace);
            if (isNull != null && XmlConvert.ToBoolean(isNull)) {
                // VSTFDevDiv# 479603 - SqlTypes read null value infinitely and never read the next value. Fix - Read the next value.
                reader.ReadElementString();
                m_fNotNull = false;
            }
            else {
                m_value = XmlConvert.ToInt64(reader.ReadElementString());
                m_fNotNull = true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void IXmlSerializable.WriteXml(XmlWriter writer) {
            if (IsNull) {
                writer.WriteAttributeString("xsi", "nil", XmlSchema.InstanceNamespace, "true");
            }
            else {
                writer.WriteString(XmlConvert.ToString(m_value));
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet) {
            return new XmlQualifiedName("long", XmlSchema.Namespace);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly SqlInt64 Null        = new SqlInt64(true);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly SqlInt64 Zero        = new SqlInt64(0);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly SqlInt64 MinValue    = new SqlInt64(Int64.MinValue);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly SqlInt64 MaxValue    = new SqlInt64(Int64.MaxValue);

    } // SqlInt64

} // namespace System.Data.SqlTypes
