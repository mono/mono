//------------------------------------------------------------------------------
// <copyright file="CharStorage.cs" company="Microsoft">
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

    internal sealed class CharStorage : DataStorage {

        private const Char defaultValue = '\0';

        private Char[] values;

        internal CharStorage(DataColumn column)
        : base(column, typeof(Char), defaultValue, StorageType.Char) {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            bool hasData = false;
            try {
                switch (kind) {
                    case AggregateType.Min:
                        Char min = Char.MaxValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (IsNull(record))
                                continue;
                            min=(values[record] < min) ? values[record] : min;
                            hasData = true;
                        }
                        if (hasData) {
                            return min;
                        }
                        return NullValue;

                    case AggregateType.Max:
                        Char max = Char.MinValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (IsNull(record))
                                continue;
                            max=(values[record] > max) ? values[record] : max;
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
                throw ExprException.Overflow(typeof(Char));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

         override public int Compare(int recordNo1, int recordNo2) {
            Char valueNo1 = values[recordNo1];
            Char valueNo2 = values[recordNo2];

            if (valueNo1 == defaultValue || valueNo2 == defaultValue) {
                int bitCheck = CompareBits(recordNo1, recordNo2);
                if (0 != bitCheck)
                    return bitCheck;
            }
            return valueNo1.CompareTo(valueNo2);
            //return (valueNo1-valueNo2); // copied from Char.CompareTo(Char)
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

            Char valueNo1 = values[recordNo];
            if ((defaultValue == valueNo1) && IsNull(recordNo)) {
                return -1;
            }
            return valueNo1.CompareTo((Char)value);
            //return (valueNo1-valueNo2); // copied from Char.CompareTo(Char)
        }

        public override object ConvertValue(object value) {
            if (NullValue != value) {
                if (null != value) {
                    value = ((IConvertible)value).ToChar(FormatProvider);
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
            Char value = values[record];
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
                Char ch = ((IConvertible)value).ToChar(FormatProvider);
                if ((ch >= (char)0xd800 && ch <= (char)0xdfff) || (ch < (char)0x21 && (ch == (char)0x9 || ch == (char)0xa || ch == (char)0xd ))) {
                    throw ExceptionBuilder.ProblematicChars(ch);
                }
                values[record] = ch;
                SetNullBit(record, false);
            }
        }

         override public void SetCapacity(int capacity) {
            Char[] newValues = new Char[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
            base.SetCapacity(capacity);
        }

         override public object ConvertXmlToObject(string s) {
	        return XmlConvert.ToChar(s);
        }

         override public string ConvertObjectToXml(object value) {
            return XmlConvert.ToString((Char) value);
        }

        override protected object GetEmptyStorage(int recordCount) {
            return new Char[recordCount];
        }

        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            Char[] typedStore = (Char[]) store;
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, IsNull(record));
        }

        override protected void SetStorage(object store, BitArray nullbits) {
            values = (Char[]) store;
            SetNullStorage(nullbits);
        }
    }
}
