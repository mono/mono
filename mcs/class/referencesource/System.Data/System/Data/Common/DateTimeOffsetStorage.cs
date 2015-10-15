//------------------------------------------------------------------------------
// <copyright file="DateTimeOffsetStorage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {
    using System;
    using System.Xml;
    using System.Data.SqlTypes;
    using System.Collections;

    internal sealed class DateTimeOffsetStorage : DataStorage {

        private static readonly DateTimeOffset defaultValue = DateTimeOffset.MinValue;

        private DateTimeOffset[] values;

        internal DateTimeOffsetStorage(DataColumn column)
        : base(column, typeof(DateTimeOffset), defaultValue, StorageType.DateTimeOffset) {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            bool hasData = false;
            try {
                switch (kind) {
                    case AggregateType.Min:
                        DateTimeOffset min = DateTimeOffset.MaxValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (HasValue(record)) {
                                min=(DateTimeOffset.Compare(values[record],min) < 0) ? values[record] : min;
                                hasData = true;
                            }
                        }
                        if (hasData) {
                            return min;
                        }
                        return NullValue;

                    case AggregateType.Max:
                        DateTimeOffset max = DateTimeOffset.MinValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (HasValue(record)) {
                                max=(DateTimeOffset.Compare(values[record],max) >= 0) ? values[record] : max;
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
                throw ExprException.Overflow(typeof(DateTimeOffset));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            DateTimeOffset valueNo1 = values[recordNo1];
            DateTimeOffset valueNo2 = values[recordNo2];
            
            if (valueNo1 == defaultValue || valueNo2 == defaultValue) {
                int bitCheck = CompareBits(recordNo1, recordNo2);
                if (0 != bitCheck) {
                    return bitCheck;
                }
            }
            return DateTimeOffset.Compare(valueNo1, valueNo2);
        }

        public override int CompareValueTo(int recordNo, object value) {
            System.Diagnostics.Debug.Assert(0 <= recordNo, "Invalid record");
            System.Diagnostics.Debug.Assert(null != value, "null value");

            if (NullValue == value) {
                return (HasValue(recordNo) ? 1 : 0);
            }

            DateTimeOffset valueNo1 = values[recordNo];
            if ((defaultValue == valueNo1) && !HasValue(recordNo)) {
                return -1;
            }
            return DateTimeOffset.Compare(valueNo1, (DateTimeOffset)value);
        }

        public override object ConvertValue(object value) {
            if (NullValue != value) {
                if (null != value) {
                    value = ((DateTimeOffset)value);
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
            DateTimeOffset value = values[record];
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
                values[record] = (DateTimeOffset)value;
                SetNullBit(record, false);
            }
        }

        override public void SetCapacity(int capacity) {
            DateTimeOffset[] newValues = new DateTimeOffset[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
            base.SetCapacity(capacity);
        }

        override public object ConvertXmlToObject(string s) {
            return XmlConvert.ToDateTimeOffset(s);
        }

        override public string ConvertObjectToXml(object value) {
            return XmlConvert.ToString((DateTimeOffset)value);
        }

        override protected object GetEmptyStorage(int recordCount) {
            return new DateTimeOffset[recordCount];
        }

        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            DateTimeOffset[] typedStore = (DateTimeOffset[]) store;
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, !HasValue(record));
        }

        override protected void SetStorage(object store, BitArray nullbits) {
            values = (DateTimeOffset[]) store;
            SetNullStorage(nullbits);               
        }
    }
}
