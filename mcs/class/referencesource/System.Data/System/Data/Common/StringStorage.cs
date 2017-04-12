//------------------------------------------------------------------------------
// <copyright file="StringStorage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Data.SqlTypes;
    using System.Collections;

    // The string storage does not use BitArrays in DataStorage
    internal sealed class StringStorage : DataStorage {

        private String[] values;

        public StringStorage(DataColumn column)
        : base(column, typeof(String), String.Empty, StorageType.String) {
        }

        override public Object Aggregate(int[] recordNos, AggregateType kind) {
            int i;
            switch (kind) {
                case AggregateType.Min:
                    int min = -1;
                    for (i = 0; i < recordNos.Length; i++) {
                        if (IsNull(recordNos[i]))
                            continue;
                        min = recordNos[i];
                        break;
                    }
                    if (min >= 0) {
                        for (i = i+1; i < recordNos.Length; i++) {
                            if (IsNull(recordNos[i]))
                                continue;
                            if (Compare(min, recordNos[i]) > 0) {
                                min = recordNos[i];
                            }
                        }
                        return Get(min);
                    }
                    return NullValue;

                case AggregateType.Max:
                    int max = -1;
                    for (i = 0; i < recordNos.Length; i++) {
                        if (IsNull(recordNos[i]))
                            continue;
                        max = recordNos[i];
                        break;
                    }
                    if (max >= 0) {
                        for (i = i+1; i < recordNos.Length; i++) {
                            if (Compare(max, recordNos[i]) < 0) {
                                max = recordNos[i];
                            }
                        }
                        return Get(max);
                    }
                    return NullValue;

                case AggregateType.Count:
                    int count = 0;
                    for (i = 0; i < recordNos.Length; i++) {
                        Object value = values[recordNos[i]];
                        if (value != null)
                            count++;
                    }
                    return count;
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            string valueNo1 = values[recordNo1];
            string valueNo2 = values[recordNo2];

            if ((Object)valueNo1 == (Object)valueNo2)
                return 0;

            if (valueNo1 == null)
                return -1;
            if (valueNo2 == null)
                return 1;

            return Table.Compare(valueNo1, valueNo2);
        }

        override public int CompareValueTo(int recordNo, Object value) {
            Debug.Assert(recordNo != -1, "Invalid (-1) parameter: 'recordNo'");
            Debug.Assert(null != value, "null value");
            string valueNo1 = values[recordNo];

            if (null == valueNo1) {
                if (NullValue == value) {
                    return 0;
                }
                else {
                    return -1;
                }
            }
            else if (NullValue == value) {
                return 1;
            }
            return Table.Compare(valueNo1, (string)value);
        }

        public override object ConvertValue(object value) {
            if (NullValue != value) {
                if (null != value) {
                    value = value.ToString();
                }
                else {
                    value = NullValue;
                }
            }
            return value;
        }

        override public void Copy(int recordNo1, int recordNo2) {
            values[recordNo2] = values[recordNo1];
        }

        override public Object Get(int recordNo) {
            String value = values[recordNo];

            if (null != value) {
                return value;
            }
            return NullValue;
        }

        override public int GetStringLength(int record) {
            string value = values[record];
            return ((null != value) ? value.Length : 0);
        }

        override public bool IsNull(int record) {
            return (null == values[record]);
        }

        override public void Set(int record, Object value) {
            System.Diagnostics.Debug.Assert(null != value, "null value");
            if (NullValue == value) {
                values[record] = null;
            }
            else {
                values[record] = value.ToString();
            }
        }

        override public void SetCapacity(int capacity) {
            string[] newValues = new string[capacity];
            if (values != null) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
        }

        override public object ConvertXmlToObject(string s) {
            return s;
        }

        override public string ConvertObjectToXml(object value) {
            return (string)value;
        }

        override protected object GetEmptyStorage(int recordCount) {
            return new String[recordCount];
        }

        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            String[] typedStore = (String[]) store;
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, IsNull(record));
        }

        override protected void SetStorage(object store, BitArray nullbits) {
            values = (String[]) store;
 //           SetNullStorage(nullbits);
        }
    }
}
