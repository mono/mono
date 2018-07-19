//------------------------------------------------------------------------------
// <copyright file="DateTimeStorage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {
    using System;
    using System.Xml;
    using System.Data.SqlTypes;
    using System.Collections;

    internal sealed class DateTimeStorage : DataStorage {

        private static readonly DateTime defaultValue = DateTime.MinValue;

        private DateTime[] values;

        internal DateTimeStorage(DataColumn column)
        : base(column, typeof(DateTime), defaultValue, StorageType.DateTime) {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            bool hasData = false;
            try {
                switch (kind) {
                    case AggregateType.Min:
                        DateTime min = DateTime.MaxValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (HasValue(record)) {
                                min=(DateTime.Compare(values[record],min) < 0) ? values[record] : min;
                                hasData = true;
                            }
                        }
                        if (hasData) {
                            return min;
                        }
                        return NullValue;

                    case AggregateType.Max:
                        DateTime max = DateTime.MinValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (HasValue(record)) {
                                max=(DateTime.Compare(values[record],max) >= 0) ? values[record] : max;
                                hasData = true;
                            }
                        }
                        if (hasData) {
                            return max;
                        }
                        return NullValue;

                    case AggregateType.First:
                        if (records.Length > 0) {
                            return values[records[0]];
                        }
                        return null;

                    case AggregateType.Count:
                        int count = 0;
                        for (int i = 0; i < records.Length; i++) {
                            if (HasValue(records[i])) {
                                count++;
                            }
                        }
                        return count;
                }
            }
            catch (OverflowException) {
                throw ExprException.Overflow(typeof(DateTime));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            DateTime valueNo1 = values[recordNo1];
            DateTime valueNo2 = values[recordNo2];
            
            if (valueNo1 == defaultValue || valueNo2 == defaultValue) {
                int bitCheck = CompareBits(recordNo1, recordNo2);
                if (0 != bitCheck) {
                    return bitCheck;
                }
            }
            return DateTime.Compare(valueNo1, valueNo2);
        }

        public override int CompareValueTo(int recordNo, object value) {
            System.Diagnostics.Debug.Assert(0 <= recordNo, "Invalid record");
            System.Diagnostics.Debug.Assert(null != value, "null value");

            if (NullValue == value) {
                return (HasValue(recordNo) ? 1 : 0);
            }

            DateTime valueNo1 = values[recordNo];
            if ((defaultValue == valueNo1) && !HasValue(recordNo)) {
                return -1;
            }
            return DateTime.Compare(valueNo1, (DateTime)value);
        }

        public override object ConvertValue(object value) {
            if (NullValue != value) {
                if (null != value) {
                    value = ((IConvertible)value).ToDateTime(FormatProvider);
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
            // SQLBU 408233: two equal DateTime with different .Kind are equal
            DateTime value = values[record];
            if ((value != defaultValue) || HasValue(record)) {
                return value;
            }
            return NullValue;
        }

        override public void Set(int record, Object value) {
            System.Diagnostics.Debug.Assert(null != value, "null value");
            if (NullValue == value) {
                values[record] = defaultValue;
                SetNullBit(record, true);
            }
            else {
                DateTime retVal;
                DateTime tmpValue = ((IConvertible)value).ToDateTime(FormatProvider);
                switch (DateTimeMode) {
                    case DataSetDateTime.Utc:
                        if (tmpValue.Kind == DateTimeKind.Utc) {
                            retVal = tmpValue;
                        }
                        else if (tmpValue.Kind == DateTimeKind.Local) {
                            retVal = tmpValue.ToUniversalTime();
                        }
                        else {
                            retVal = DateTime.SpecifyKind(tmpValue, DateTimeKind.Utc);
                        }
                        break;
                    case DataSetDateTime.Local:
                        if (tmpValue.Kind == DateTimeKind.Local) {
                            retVal = tmpValue;
                        }
                        else if (tmpValue.Kind == DateTimeKind.Utc) {
                            retVal = tmpValue.ToLocalTime();
                        }
                        else {
                            retVal =  DateTime.SpecifyKind(tmpValue, DateTimeKind.Local);
                        }
                        break;
                    case DataSetDateTime.Unspecified:
                    case DataSetDateTime.UnspecifiedLocal:
                        retVal = DateTime.SpecifyKind(tmpValue, DateTimeKind.Unspecified); 
                        break;
                    default:
                        throw ExceptionBuilder.InvalidDateTimeMode(DateTimeMode);
                }
                values[record] = retVal;
                SetNullBit(record, false);
            }
        }

        override public void SetCapacity(int capacity) {
            DateTime[] newValues = new DateTime[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
            base.SetCapacity(capacity);
        }

        override public object ConvertXmlToObject(string s) {
            object retValue;
            if (DateTimeMode == DataSetDateTime.UnspecifiedLocal) {
                retValue = XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.Unspecified);
            }
            else {
                retValue = XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.RoundtripKind);
            }
            return retValue;
        }

        override public string ConvertObjectToXml(object value) {
            string retValue;
            if (DateTimeMode == DataSetDateTime.UnspecifiedLocal) {
                retValue = XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Local);
            }
            else {
                retValue = XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind);
            }
            return retValue;
        }

        override protected object GetEmptyStorage(int recordCount) {
            return new DateTime[recordCount];
        }

        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            DateTime[] typedStore = (DateTime[]) store;
            bool isnull = !HasValue(record);
            if (isnull || ( 0 == (DateTimeMode & DataSetDateTime.Local))) {
                typedStore[storeIndex] = values[record]; // already universal time
            }
            else {
                typedStore[storeIndex] = values[record].ToUniversalTime();
            }
            nullbits.Set(storeIndex, isnull);
        }

        override protected void SetStorage(object store, BitArray nullbits) {
            values = (DateTime[]) store; 
            SetNullStorage(nullbits);
            if (DateTimeMode == DataSetDateTime.UnspecifiedLocal) {
                for (int i = 0; i < values.Length; i++) {
                    if (HasValue(i)) {
                        values[i] = DateTime.SpecifyKind(values[i].ToLocalTime(), DateTimeKind.Unspecified); //Strip the kind for UnspecifiedLocal.
                    }
                }                
            }
            else if (DateTimeMode == DataSetDateTime.Local) {
                for (int i = 0; i < values.Length; i++) {
                    if (HasValue(i)) {
                        values[i] = values[i].ToLocalTime();
                    }
                }                
            }
        }
    }
}
