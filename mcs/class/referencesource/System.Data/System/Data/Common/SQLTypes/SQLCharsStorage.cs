//------------------------------------------------------------------------------
// <copyright file="SQLCharsStorage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {
    using System;
    using System.Xml;
    using System.IO;
    using System.Xml.Serialization;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Globalization;
    using System.Collections;

    internal sealed class SqlCharsStorage : DataStorage {

        private SqlChars[] values;

        public SqlCharsStorage(DataColumn column)
        : base(column, typeof(SqlChars), SqlChars.Null, SqlChars.Null, StorageType.SqlChars) {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            try {
                switch (kind) {
                    case AggregateType.First:
                        if (records.Length > 0) {
                            return values[records[0]];
                        }
                        return null;// no data => null

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
                throw ExprException.Overflow(typeof(SqlChars));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
//            throw ExceptionBuilder.IComparableNotDefined;
            return 0;
        }

        override public int CompareValueTo(int recordNo, Object value) {
//            throw ExceptionBuilder.IComparableNotDefined;
            return 0;
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
            if ((value ==  DBNull.Value) || (value == null)){
                values[record] = SqlChars.Null;
            }
            else {
                values[record] = (SqlChars)value;            
            }
        }

        override public void SetCapacity(int capacity) {
            SqlChars[] newValues = new SqlChars[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
        }

        override public object ConvertXmlToObject(string s) {
            SqlString newValue = new SqlString();

            string tempStr =string.Concat("<col>", s, "</col>"); // this is done since you can give fragmet to reader, bug 98767
            StringReader strReader = new  StringReader(tempStr);

            IXmlSerializable tmp = newValue;
            
            using (XmlTextReader xmlTextReader = new XmlTextReader(strReader)) {
                tmp.ReadXml(xmlTextReader);
            }
            return (new SqlChars((SqlString)tmp));
        }

        override public string ConvertObjectToXml(object value) {
            Debug.Assert(!DataStorage.IsObjectNull(value), "we shouldn't have null here");
            Debug.Assert((value.GetType() == typeof(SqlChars)), "wrong input type");
            
            StringWriter strwriter = new StringWriter(FormatProvider);

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter (strwriter)) {
                ((IXmlSerializable)value).WriteXml(xmlTextWriter);
            }
            return (strwriter.ToString ());
        }
        
        override protected object GetEmptyStorage(int recordCount) {
            return new SqlChars[recordCount];
        }
        
        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            SqlChars[] typedStore = (SqlChars[]) store; 
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, IsNull(record));
        }
        
        override protected void SetStorage(object store, BitArray nullbits) {
            values = (SqlChars[]) store; 
            //SetNullStorage(nullbits);
        }        
    }
}
