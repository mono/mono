//------------------------------------------------------------------------------
// <copyright file="SQLBooleanStorage.cs" company="Microsoft">
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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Xml.Serialization;
    using System.Collections;

    internal sealed class SqlBooleanStorage : DataStorage {

        private SqlBoolean[] values;

        public SqlBooleanStorage(DataColumn column)
        : base(column, typeof(SqlBoolean), SqlBoolean.Null, SqlBoolean.Null, StorageType.SqlBoolean) {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            bool hasData = false;
            try {
                switch (kind) {
                    case AggregateType.Min:
                        SqlBoolean min = true;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (IsNull(record))
                                continue;
                            min= SqlBoolean.And(values[record], min);
                            hasData = true;
                        }
                        if (hasData) {
                            return min;
                        }
                        return NullValue;

                    case AggregateType.Max:
                        SqlBoolean max = false;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (IsNull(record))
                                continue;
                            max= SqlBoolean.Or(values[record], max);
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
                        return NullValue;

                    case AggregateType.Count:
                        int count = 0;
                        for (int i = 0; i < records.Length; i++) {
                            if (!IsNull(records[i]))
                                count++;
                        }
                        return count;
                }
            }
            catch (OverflowException) {
                throw ExprException.Overflow(typeof(SqlBoolean));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            return values[recordNo1].CompareTo(values[recordNo2]);
        }

        override public int CompareValueTo(int recordNo, Object value) {
            return values[recordNo].CompareTo((SqlBoolean)value);
        }
        
        override public object ConvertValue(object value) {
            if (null != value) {
                return SqlConvert.ConvertToSqlBoolean(value);
            }
            return NullValue;
        }

        override public void Copy(int recordNo1, int recordNo2) {
            values[recordNo2] = values[recordNo1];
        }

        override public Object Get(int record) {
            return values[record];
        }

        override public bool IsNull(int record) {
            return (values[record].IsNull);
        }

        override public void Set(int record, Object value) {
            values[record] = SqlConvert.ConvertToSqlBoolean(value);
        }
        
        override public void SetCapacity(int capacity) {
            SqlBoolean[] newValues = new SqlBoolean[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
        }

        override public object ConvertXmlToObject(string s) {
            SqlBoolean newValue = new SqlBoolean();
            string tempStr =string.Concat("<col>", s, "</col>"); // this is done since you can give fragmet to reader, 
            StringReader strReader = new  StringReader(tempStr);

            IXmlSerializable tmp = newValue;

            using (XmlTextReader xmlTextReader = new XmlTextReader(strReader)) {
                tmp.ReadXml(xmlTextReader);
            }
            return ((SqlBoolean)tmp);
        }

        override public string ConvertObjectToXml(object value) {
            Debug.Assert(!DataStorage.IsObjectNull(value), "we should have null here");
            Debug.Assert((value.GetType() == typeof(SqlBoolean)), "wrong input type");

            StringWriter strwriter = new StringWriter(FormatProvider);

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter (strwriter)) {
                ((IXmlSerializable)value).WriteXml(xmlTextWriter);
            }
            return (strwriter.ToString ());
        }
        
        override protected object GetEmptyStorage(int recordCount) {
            return new SqlBoolean[recordCount];
        }
        
        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            SqlBoolean[] typedStore = (SqlBoolean[]) store; 
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, IsNull(record));
        }
        
        override protected void SetStorage(object store, BitArray nullbits) {
            values = (SqlBoolean[]) store; 
            //SetNullStorage(nullbits);
        }        
    }
}
