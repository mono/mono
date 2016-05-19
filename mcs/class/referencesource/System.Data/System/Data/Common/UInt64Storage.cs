//------------------------------------------------------------------------------
// <copyright file="UInt64Storage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {
    using System;
    using System.Xml;
    using System.Data.SqlTypes;
    using System.Collections;

    internal sealed class UInt64Storage : DataStorage {

        private static readonly UInt64 defaultValue = UInt64.MinValue;

        private UInt64[] values;

        public UInt64Storage(DataColumn column)
        : base(column, typeof(UInt64), defaultValue, StorageType.UInt64) {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            bool hasData = false;
            try {
                switch (kind) {
                    case AggregateType.Sum:
                        UInt64 sum = defaultValue;
                        foreach (int record in records) {
                            if (HasValue(record)) {
                                checked { sum += values[record];}
                                hasData = true;
                            }
                        }
                        if (hasData) {
                            return sum;
                        }
                        return NullValue;

                    case AggregateType.Mean:
                        Decimal meanSum = (Decimal)defaultValue;
                        int meanCount = 0;
                        foreach (int record in records) {
                            if (HasValue(record)) {
                                checked { meanSum += (Decimal)values[record];}
                                meanCount++;
                                hasData = true;
                            }
                        }
                        if (hasData) {
                            UInt64 mean;
                            checked {mean = (UInt64)(Decimal)(meanSum / (Decimal)meanCount);}
                            return mean;
                        }
                        return NullValue;

                    case AggregateType.Var:
                    case AggregateType.StDev:
                        int count = 0;
                        double var = 0.0f;
                        double prec = 0.0f;
                        double dsum = 0.0f;
                        double sqrsum = 0.0f;

                        foreach (int record in records) {
                            if (HasValue(record)) {
                                dsum += (double)values[record];
                                sqrsum += (double)values[record]*(double)values[record];
                                count++;
                            }
                        }

                        if (count > 1) {
                            var = ((double)count * sqrsum - (dsum * dsum));
                            prec = var / (dsum * dsum);

                            // we are dealing with the risk of a cancellation error
                            // double is guaranteed only for 15 digits so a difference
                            // with a result less than 1e-15 should be considered as zero

                            if ((prec < 1e-15) || (var <0))
                                var = 0;
                            else
                                var = var / (count * (count -1));

                            if (kind == AggregateType.StDev) {
                                return Math.Sqrt(var);
                            }
                            return var;
                        }
                        return NullValue;

                    case AggregateType.Min:
                        UInt64 min = UInt64.MaxValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (HasValue(record)) {
                                min=Math.Min(values[record], min);
                                hasData = true;
                            }
                        }
                        if (hasData) {
                            return min;
                        }
                        return NullValue;

                    case AggregateType.Max:
                        UInt64 max = UInt64.MinValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (HasValue(record)) {
                                max=Math.Max(values[record], max);
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
                        return base.Aggregate(records, kind);

                }
            }
            catch (OverflowException) {
                throw ExprException.Overflow(typeof(UInt64));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            UInt64 valueNo1 = values[recordNo1];
            UInt64 valueNo2 = values[recordNo2];

            if (valueNo1.Equals(defaultValue) || valueNo2.Equals(defaultValue)) {
                int bitCheck = CompareBits(recordNo1, recordNo2);
                if (0 != bitCheck)
                    return bitCheck;
            }
            //return valueNo1.CompareTo(valueNo2);
            return(valueNo1 < valueNo2 ? -1 : (valueNo1 > valueNo2 ? 1 : 0)); // similar to UInt64.CompareTo(UInt64)
        }

        public override int CompareValueTo(int recordNo, object value) {
            System.Diagnostics.Debug.Assert(0 <= recordNo, "Invalid record");
            System.Diagnostics.Debug.Assert(null != value, "null value");

            if (NullValue == value) {
                return (HasValue(recordNo) ? 1 : 0);
            }

            UInt64 valueNo1 = values[recordNo];
            if ((defaultValue == valueNo1) && !HasValue(recordNo)) {
                return -1;
            }
            return valueNo1.CompareTo((UInt64)value);
            //return(valueNo1 < valueNo2 ? -1 : (valueNo1 > valueNo2 ? 1 : 0)); // similar to UInt64.CompareTo(UInt64)
        }

        public override object ConvertValue(object value) {
            if (NullValue != value) {
                if (null != value) {
                    value = ((IConvertible)value).ToUInt64(FormatProvider);
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
            UInt64 value = values[record];
            if (!value.Equals(defaultValue)) {
                return value;
            }
            return GetBits(record);
        }

        override public void Set(int record, Object value) {
            System.Diagnostics.Debug.Assert(null != value, "null value");
            if (NullValue == value) {
                values[record] = defaultValue;
                SetNullBit(record, true);
            }
            else {
                values[record] = ((IConvertible)value).ToUInt64(FormatProvider);
                SetNullBit(record, false);
            }
        }

        override public void SetCapacity(int capacity) {
            UInt64[] newValues = new UInt64[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
            base.SetCapacity(capacity);
        }

        override public object ConvertXmlToObject(string s) {
            return XmlConvert.ToUInt64(s);
        }

        override public string ConvertObjectToXml(object value) {
            return XmlConvert.ToString((UInt64)value);
        }

        override protected object GetEmptyStorage(int recordCount) {
            return new UInt64[recordCount];
        }

        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            UInt64[] typedStore = (UInt64[]) store;
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, !HasValue(record));
        }

        override protected void SetStorage(object store, BitArray nullbits) {
            values = (UInt64[]) store;
            SetNullStorage(nullbits);
        }
    }
}
