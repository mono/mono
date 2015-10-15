//------------------------------------------------------------------------------
// <copyright file="SQLBinaryStorage.cs" company="Microsoft">
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

    internal sealed class SqlBinaryStorage : DataStorage {

        private SqlBinary[] values;

        public SqlBinaryStorage(DataColumn column)
        : base(column, typeof(SqlBinary), SqlBinary.Null, SqlBinary.Null, StorageType.SqlBinary) {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            try {
                switch (kind) {
                    case AggregateType.First:
                        if (records.Length > 0) {
                            return values[records[0]];
                        }
                        return null;

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
                throw ExprException.Overflow(typeof(SqlBinary));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            return values[recordNo1].CompareTo(values[recordNo2]);
        }

        override public int CompareValueTo(int recordNo, Object value) {
            return values[recordNo].CompareTo((SqlBinary)value);
        }
        
        override public object ConvertValue(object value) {
            if (null != value) {
                return SqlConvert.ConvertToSqlBinary(value);
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
            values[record] = SqlConvert.ConvertToSqlBinary(value);
        }

        override public void SetCapacity(int capacity) {
            SqlBinary[] newValues = new SqlBinary[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
        }

        override public object ConvertXmlToObject(string s) {
            SqlBinary newValue = new SqlBinary();
            string tempStr =string.Concat("<col>", s, "</col>"); // this is done since you can give fragmet to reader, 
            StringReader strReader = new  StringReader(tempStr);

            IXmlSerializable tmp = newValue;
            
            using (XmlTextReader xmlTextReader = new XmlTextReader(strReader)) {
                tmp.ReadXml(xmlTextReader);
            }
            return ((SqlBinary)tmp);
        }

        override public string ConvertObjectToXml(object value) {
            Debug.Assert(!DataStorage.IsObjectNull(value), "we should have null here");
            Debug.Assert((value.GetType() == typeof(SqlBinary)), "wrong input type");
            
            StringWriter strwriter = new StringWriter(FormatProvider);
            
            using (XmlTextWriter xmlTextWriter = new XmlTextWriter (strwriter)) {
                ((IXmlSerializable)value).WriteXml(xmlTextWriter);
            }
            return (strwriter.ToString ());
        }
        
        override protected object GetEmptyStorage(int recordCount) {
            return new SqlBinary[recordCount];
        }
        
        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            SqlBinary[] typedStore = (SqlBinary[]) store; 
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, IsNull(record));
        }
        
        override protected void SetStorage(object store, BitArray nullbits) {
            values = (SqlBinary[]) store; 
            //SetNullStorage(nullbits);
        }        
    }
}
