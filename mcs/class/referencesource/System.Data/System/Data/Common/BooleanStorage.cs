//------------------------------------------------------------------------------
// <copyright file="BooleanStorage.cs" company="Microsoft">
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

    internal sealed class BooleanStorage : DataStorage {

        private const Boolean defaultValue = false;

        private Boolean[] values;

        internal BooleanStorage(DataColumn column)
        : base(column, typeof(Boolean), defaultValue, StorageType.Boolean) {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            bool hasData = false;
            try {
                switch (kind) {
                    case AggregateType.Min:
                        Boolean min = true;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (IsNull(record))
                                continue;
                            min=values[record] && min;
                            hasData = true;
                        }
                        if (hasData) {
                            return min;
                        }
                        return NullValue;

                    case AggregateType.Max:
                        Boolean max = false;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (IsNull(record))
                                continue;
                            max=values[record] || max;
                            hasData = true;
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
                throw ExprException.Overflow(typeof(Boolean));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            Boolean valueNo1 = values[recordNo1];
            Boolean valueNo2 = values[recordNo2];

            if (valueNo1 == defaultValue || valueNo2 == defaultValue) {
                int bitCheck = CompareBits(recordNo1, recordNo2);
                if (0 != bitCheck)
                    return bitCheck;
            }
            return valueNo1.CompareTo(valueNo2);
            //return ((valueNo1 == valueNo2) ? 0 : ((false == valueNo1) ? -1 : 1)); // similar to Boolean.CompareTo(Boolean)
        }

        public override int CompareValueTo(int recordNo, object value) {
            System.Diagnostics.Debug.Assert(0 <= recordNo, "Invalid record");
            System.Diagnostics.Debug.Assert(null != value, "null value");

            if (NullValue == value) {
                if (IsNull(recordNo)) {
                    return 0;
                }
                return 1;
            }

            Boolean valueNo1 = values[recordNo];
            if ((defaultValue == valueNo1) && IsNull(recordNo)) {
                return -1;
            }
            return valueNo1.CompareTo((Boolean)value);
            //return ((valueNo1 == valueNo2) ? 0 : ((false == valueNo1) ? -1 : 1)); // similar to Boolean.CompareTo(Boolean)
        }

        public override object ConvertValue(object value) {
            if (NullValue != value) {
                if (null != value) {
                    value = ((IConvertible)value).ToBoolean(FormatProvider);
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
            Boolean value = values[record];
            if (value != defaultValue) {
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
                values[record] = ((IConvertible)value).ToBoolean(FormatProvider);
                SetNullBit(record, false);
            }
        }

         override public void SetCapacity(int capacity) {
            Boolean[] newValues = new Boolean[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
            base.SetCapacity(capacity);
        }

         override public object ConvertXmlToObject(string s) {
            return XmlConvert.ToBoolean(s);
        }

         override public string ConvertObjectToXml(object value) {
            return XmlConvert.ToString((Boolean) value);
        }

        override protected object GetEmptyStorage(int recordCount) {
            return new Boolean[recordCount];
        }

        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            Boolean[] typedStore = (Boolean[]) store;
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, IsNull(record));
        }

        override protected void SetStorage(object store, BitArray nullbits) {
            values = (Boolean[]) store;
            SetNullStorage(nullbits);
        }
    }
}
