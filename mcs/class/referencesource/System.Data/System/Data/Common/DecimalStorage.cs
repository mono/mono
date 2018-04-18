//------------------------------------------------------------------------------
// <copyright file="DecimalStorage.cs" company="Microsoft">
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

    internal sealed class DecimalStorage : DataStorage {

        private static readonly Decimal defaultValue = Decimal.Zero;

        private Decimal[] values;

        internal DecimalStorage(DataColumn column)
        : base(column, typeof(Decimal), defaultValue, StorageType.Decimal) {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            bool hasData = false;
            try {
                switch (kind) {
                    case AggregateType.Sum:
                        Decimal sum = defaultValue;
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
                            Decimal mean;
                            checked {mean = (meanSum /(Decimal) meanCount);}
                            return mean;
                        }
                        return NullValue;

                    case AggregateType.Var:
                    case AggregateType.StDev:
                        int count = 0;
                        double var = (double)defaultValue;
                        double prec = (double)defaultValue;
                        double dsum = (double)defaultValue;
                        double sqrsum = (double)defaultValue;

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
                        Decimal min = Decimal.MaxValue;
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
                        Decimal max = Decimal.MinValue;
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
                throw ExprException.Overflow(typeof(Decimal));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            Decimal valueNo1 = values[recordNo1];
            Decimal valueNo2 = values[recordNo2];

            if (valueNo1 == defaultValue || valueNo2 == defaultValue) {
                int bitCheck = CompareBits(recordNo1, recordNo2);
                if (0 != bitCheck)
                    return bitCheck;
            }
            return Decimal.Compare(valueNo1, valueNo2); // InternalCall
        }

        public override int CompareValueTo(int recordNo, object value) {
            System.Diagnostics.Debug.Assert(0 <= recordNo, "Invalid record");
            System.Diagnostics.Debug.Assert(null != value, "null value");

            if (NullValue == value) {
                return (HasValue(recordNo) ? 1 : 0);
            }

            Decimal valueNo1 = values[recordNo];
            if ((defaultValue == valueNo1) && !HasValue(recordNo)) {
                return -1;
            }
            return Decimal.Compare(valueNo1, (Decimal)value);
        }

        public override object ConvertValue(object value) {
            if (NullValue != value) {
                if (null != value) {
                    value = ((IConvertible)value).ToDecimal(FormatProvider);
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
            return (HasValue(record) ? values[record] : NullValue);
        }

        override public void Set(int record, Object value) {
            System.Diagnostics.Debug.Assert(null != value, "null value");
            if (NullValue == value) {
                values[record] = defaultValue;
                SetNullBit(record, true);
            }
            else {
                values[record] = ((IConvertible)value).ToDecimal(FormatProvider);
                SetNullBit(record, false);
            }
        }

        override public void SetCapacity(int capacity) {
            Decimal[] newValues = new Decimal[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
            base.SetCapacity(capacity);
        }

        override public object ConvertXmlToObject(string s) {
            return XmlConvert.ToDecimal(s);
        }

        override public string ConvertObjectToXml(object value) {
            return XmlConvert.ToString((Decimal)value);
        }

        override protected object GetEmptyStorage(int recordCount) {
            return new Decimal[recordCount];
        }

        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            Decimal[] typedStore = (Decimal[]) store;
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, !HasValue(record));
        }

        override protected void SetStorage(object store, BitArray nullbits) {
            values = (Decimal[]) store;
            SetNullStorage(nullbits);
        }
    }
}
