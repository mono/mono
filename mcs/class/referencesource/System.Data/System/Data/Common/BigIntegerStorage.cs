//------------------------------------------------------------------------------
// <copyright file="BigIntStorage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {
    using System;
    using System.Xml;
    using System.Numerics;
    using System.Data.SqlTypes;
    using System.Collections;

    internal sealed class BigIntegerStorage : DataStorage {

        private BigInteger[] values;

        internal BigIntegerStorage(DataColumn column)
            : base(column, typeof(BigInteger), BigInteger.Zero, StorageType.BigInteger)
        {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            BigInteger valueNo1 = values[recordNo1];
            BigInteger valueNo2 = values[recordNo2];

            if (valueNo1.IsZero || valueNo2.IsZero) {
                int bitCheck = CompareBits(recordNo1, recordNo2);
                if (0 != bitCheck) {
                    return bitCheck;
                }
            }

            return valueNo1.CompareTo(valueNo2);
        }

        public override int CompareValueTo(int recordNo, object value) {
            System.Diagnostics.Debug.Assert(0 <= recordNo, "Invalid record");
            System.Diagnostics.Debug.Assert(null != value, "null value");

            if (NullValue == value) {
                return (HasValue(recordNo) ? 1 : 0);
            }

            BigInteger valueNo1 = values[recordNo];
            if (valueNo1.IsZero && !HasValue(recordNo)) {
                return -1;
            }

            return valueNo1.CompareTo((BigInteger)value);
        }

        // supported implict casts
        internal static BigInteger ConvertToBigInteger(object value, IFormatProvider formatProvider) {
            if (value.GetType() == typeof(BigInteger)) { return (BigInteger)value; }
            else if (value.GetType() == typeof(String)) { return BigInteger.Parse((string)value, formatProvider); }
            else if (value.GetType() == typeof(Int64)) { return (BigInteger)(Int64)value; }
            else if (value.GetType() == typeof(Int32)) { return (BigInteger)(Int32)value; }
            else if (value.GetType() == typeof(Int16)) { return (BigInteger)(Int16)value; }
            else if (value.GetType() == typeof(SByte)) { return (BigInteger)(SByte)value; }
            else if (value.GetType() == typeof(UInt64)) { return (BigInteger)(UInt64)value; }
            else if (value.GetType() == typeof(UInt32)) { return (BigInteger)(UInt32)value; }
            else if (value.GetType() == typeof(UInt16)) { return (BigInteger)(UInt16)value; }
            else if (value.GetType() == typeof(Byte)) { return (BigInteger)(Byte)value; }
            else { throw ExceptionBuilder.ConvertFailed(value.GetType(), typeof(System.Numerics.BigInteger)); }
        }

        internal static object ConvertFromBigInteger(BigInteger value, Type type, IFormatProvider formatProvider) {
            if (type == typeof(string)) { return value.ToString("D", formatProvider); }
            else if (type == typeof(SByte)) { return checked((SByte)value); }
            else if (type == typeof(Int16)) { return checked((Int16)value); }
            else if (type == typeof(Int32)) { return checked((Int32)value); }
            else if (type == typeof(Int64)) { return checked((Int64)value); }
            else if (type == typeof(Byte)) { return checked((Byte)value); }
            else if (type == typeof(UInt16)) { return checked((UInt16)value); }
            else if (type == typeof(UInt32)) { return checked((UInt32)value); }
            else if (type == typeof(UInt64)) { return checked((UInt64)value); }
            else if (type == typeof(Single)) { return checked((Single)value); }
            else if (type == typeof(Double)) { return checked((Double)value); }
            else if (type == typeof(Decimal)) { return checked((Decimal)value); }
            else if (type == typeof(System.Numerics.BigInteger)) { return value; }
            else { throw ExceptionBuilder.ConvertFailed(typeof(System.Numerics.BigInteger), type); }
        }

        public override object ConvertValue(object value) {
            if (NullValue != value) {
                if (null != value) {
                    value = ConvertToBigInteger(value, this.FormatProvider);
                }
                else {
                    value = NullValue;
                }
            }
            return value;
        }

        override public void Copy(int recordNo1, int recordNo2) {
            CopyBits(recordNo1, recordNo2);
            values[recordNo2] = values[recordNo1];
        }

        override public Object Get(int record) {
            BigInteger value = values[record];
            if (!value.IsZero) {
                return value;
            }
            return GetBits(record);
        }

        override public void Set(int record, Object value) {
            System.Diagnostics.Debug.Assert(null != value, "null value");
            if (NullValue == value) {
                values[record] = BigInteger.Zero;
                SetNullBit(record, true);
            }
            else {
                values[record] = ConvertToBigInteger(value, this.FormatProvider);
                SetNullBit(record, false);
            }
        }

        override public void SetCapacity(int capacity) {
            BigInteger[] newValues = new BigInteger[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
            base.SetCapacity(capacity);
        }

        override public object ConvertXmlToObject(string s) {
            return BigInteger.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        }

        override public string ConvertObjectToXml(object value) {
            return ((BigInteger)value).ToString("D", System.Globalization.CultureInfo.InvariantCulture);
        }

        override protected object GetEmptyStorage(int recordCount) {
            return new BigInteger[recordCount];
        }

        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            BigInteger[] typedStore = (BigInteger[])store;
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, !HasValue(record));
        }

        override protected void SetStorage(object store, BitArray nullbits) {
            values = (BigInteger[])store;
            SetNullStorage(nullbits);
        }
    }
}
