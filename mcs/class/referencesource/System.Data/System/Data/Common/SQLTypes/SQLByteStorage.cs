//------------------------------------------------------------------------------
// <copyright file="SQLByteStorage.cs" company="Microsoft">
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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Xml.Serialization;
    using System.Collections;
    
    internal sealed class SqlByteStorage : DataStorage {

        private SqlByte[] values;

        public SqlByteStorage(DataColumn column)
        : base(column, typeof(SqlByte), SqlByte.Null, SqlByte.Null, StorageType.SqlByte) {
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            bool hasData = false;
            try {
                switch (kind) {
                    case AggregateType.Sum:
                        SqlInt64 sum =  0;
                        foreach (int record in records) {
                            if (IsNull(record))
                                continue;
                            checked { sum += values[record];}
                            hasData = true;
                        }
                        if (hasData) {
                            return sum;
                        }
                        return NullValue;

                    case AggregateType.Mean:
                        SqlInt64 meanSum = 0;
                        int meanCount = 0;
                        foreach (int record in records) {
                            if (IsNull(record))
                                continue;
                            checked { meanSum += values[record].ToSqlInt64();}
                            meanCount++;
                            hasData = true;
                        }
                        if (hasData) {
                            SqlByte mean = 0;
                            checked {mean = (meanSum / (SqlInt64)meanCount).ToSqlByte();}
                            return mean;
                        }
                        return NullValue;

                    case AggregateType.Var:
                    case AggregateType.StDev:
                        int count = 0;
                        SqlDouble var = (SqlDouble)0;
                        SqlDouble prec = (SqlDouble)0;
                        SqlDouble dsum = (SqlDouble)0;
                        SqlDouble sqrsum = (SqlDouble)0;

                        foreach (int record in records) {
                            if (IsNull(record))
                                continue;
                            dsum += values[record].ToSqlDouble();
                            sqrsum += values[record].ToSqlDouble() * values[record].ToSqlDouble();
                            count++;
                        }

                        if (count > 1) {
                            var = ((SqlDouble)count * sqrsum - (dsum * dsum));
                            prec = var / (dsum * dsum);
                            
                            // we are dealing with the risk of a cancellation error
                            // double is guaranteed only for 15 digits so a difference 
                            // with a result less than 1e-15 should be considered as zero

                            if ((prec < 1e-15) || (var <0))
                                var = 0;
                            else
                                var = var / (count * (count -1));
                            
                            if (kind == AggregateType.StDev) {
                               return  Math.Sqrt(var.Value);
                            }
                            return var;
                        }
                        return NullValue;

                    case AggregateType.Min:
                        SqlByte min = SqlByte.MaxValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (IsNull(record))
                                continue;
                            if ((SqlByte.LessThan(values[record], min)).IsTrue)
                                min = values[record];                        
                            hasData = true;
                        }
                        if (hasData) {
                            return min;
                        }
                        return NullValue;

                    case AggregateType.Max:
                        SqlByte max = SqlByte.MinValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (IsNull(record))
                                continue;
                            if ((SqlByte.GreaterThan(values[record], max)).IsTrue)
                                max = values[record];
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
                        return null;// no data => null

                    case AggregateType.Count:
                        count = 0;
                        for (int i = 0; i < records.Length; i++) {
                            if (!IsNull(records[i]))
                                count++;
                        }
                        return count;
                }
            }
            catch (OverflowException) {
                throw ExprException.Overflow(typeof(SqlByte));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            return values[recordNo1].CompareTo(values[recordNo2]);
        }

        override public int CompareValueTo(int recordNo, Object value) {
            return values[recordNo].CompareTo((SqlByte)value);
        }
        
        override public object ConvertValue(object value) {
            if (null != value) {
                return SqlConvert.ConvertToSqlByte(value);
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
            values[record] =  SqlConvert.ConvertToSqlByte(value);
        }

        override public void SetCapacity(int capacity) {
            SqlByte[] newValues = new SqlByte[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
        }

        override public object ConvertXmlToObject(string s) {
            SqlByte newValue = new SqlByte();
            string tempStr =string.Concat("<col>", s, "</col>"); // this is done since you can give fragmet to reader, bug 98767
            StringReader strReader = new  StringReader(tempStr);

            IXmlSerializable tmp = newValue;
            
            using (XmlTextReader xmlTextReader = new XmlTextReader(strReader)) {
                tmp.ReadXml(xmlTextReader);
            }
            return ((SqlByte)tmp);
        }

        override public string ConvertObjectToXml(object value) {
            Debug.Assert(!DataStorage.IsObjectNull(value), "we shouldn't have null here");
            Debug.Assert((value.GetType() == typeof(SqlByte)), "wrong input type");

            StringWriter strwriter = new StringWriter(FormatProvider);

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter (strwriter)) {
                ((IXmlSerializable)value).WriteXml(xmlTextWriter);
            }
            return (strwriter.ToString ());
        }
        
        override protected object GetEmptyStorage(int recordCount) {
            return new SqlByte[recordCount];
        }
        
        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            SqlByte[] typedStore = (SqlByte[]) store; 
            typedStore[storeIndex] = values[record];
            nullbits.Set(record, IsNull(record));
        }
        
        override protected void SetStorage(object store, BitArray nullbits) {
            values = (SqlByte[]) store; 
            //SetNullStorage(nullbits);
        }          
    }
}
