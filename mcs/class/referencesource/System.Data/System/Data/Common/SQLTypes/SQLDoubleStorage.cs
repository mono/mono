//------------------------------------------------------------------------------
// <copyright file="SQLDoubleStorage.cs" company="Microsoft">
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

    internal sealed class SqlDoubleStorage : DataStorage {

        private SqlDouble[] values;

        public SqlDoubleStorage(DataColumn column)
        : base(column, typeof(SqlDouble), SqlDouble.Null, SqlDouble.Null, StorageType.SqlDouble) {
        }
        
        override public Object Aggregate(int[] records, AggregateType kind) {
            bool hasData = false;
            try {
                switch (kind) {
                    case AggregateType.Sum:
                        SqlDouble sum =  0.0d;
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
                        SqlDouble meanSum = 0.0d;
                        int meanCount = 0;
                        foreach (int record in records) {
                            if (IsNull(record))
                                continue;
                            checked { meanSum += values[record];}
                            meanCount++;
                            hasData = true;
                        }
                        if (hasData) {
                            SqlDouble mean = 0.0d;
                            checked {mean = (meanSum / (SqlDouble) meanCount);}
                            return mean;
                        }
                        return NullValue;

                    case AggregateType.Var:
                    case AggregateType.StDev:
                        int count = 0;
                        SqlDouble var = (SqlDouble)0.0d;
                        SqlDouble prec = (SqlDouble)0.0d;
                        SqlDouble dsum = (SqlDouble)0.0d;
                        SqlDouble sqrsum = (SqlDouble)0.0d;

                        foreach (int record in records) {
                            if (IsNull(record))
                                continue;
                            dsum += values[record];
                            sqrsum += values[record]*values[record];
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
                        SqlDouble min = SqlDouble.MaxValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (IsNull(record))
                                continue;
                            if ((SqlDouble.LessThan(values[record], min)).IsTrue)
                                min = values[record];                        
                            hasData = true;
                        }
                        if (hasData) {
                            return min;
                        }
                        return NullValue;

                    case AggregateType.Max:
                        SqlDouble max = SqlDouble.MinValue;
                        for (int i = 0; i < records.Length; i++) {
                            int record = records[i];
                            if (IsNull(record))
                                continue;
                            if ((SqlDouble.GreaterThan(values[record], max)).IsTrue)
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
                throw ExprException.Overflow(typeof(SqlDouble));
            }
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            return values[recordNo1].CompareTo(values[recordNo2]);
        }

        override public int CompareValueTo(int recordNo, Object value) {
            return values[recordNo].CompareTo((SqlDouble)value);
        }

        override public object ConvertValue(object value) {
            if (null != value) {
                return SqlConvert.ConvertToSqlDouble(value);
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
            return(values[record].IsNull);
        }

        override public void Set(int record, Object value) {
            values[record] = SqlConvert.ConvertToSqlDouble( value);
        }

        override public void SetCapacity(int capacity) {
            SqlDouble[] newValues = new SqlDouble[capacity];
            if (null != values) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
        }

        override public object ConvertXmlToObject(string s) {
            SqlDouble newValue = new SqlDouble();
            string tempStr =string.Concat("<col>", s, "</col>"); // this is done since you can give fragmet to reader, bug 98767
            StringReader strReader = new  StringReader(tempStr);

            IXmlSerializable tmp = newValue;
            
            using (XmlTextReader xmlTextReader = new XmlTextReader(strReader)) {
                tmp.ReadXml(xmlTextReader);
            }
            return ((SqlDouble)tmp);
        }

        override public string ConvertObjectToXml(object value) {
            Debug.Assert(!DataStorage.IsObjectNull(value), "we shouldn't have null here");
            Debug.Assert((value.GetType() == typeof(SqlDouble)), "wrong input type");

            StringWriter strwriter = new StringWriter(FormatProvider);

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter (strwriter)) {
                ((IXmlSerializable)value).WriteXml(xmlTextWriter);
            }
            return (strwriter.ToString ());
        }
        
        override protected object GetEmptyStorage(int recordCount) {
            return new SqlDouble[recordCount];
        }
        
        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            SqlDouble[] typedStore = (SqlDouble[]) store; 
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, IsNull(record));
        }
        
        override protected void SetStorage(object store, BitArray nullbits) {
            values = (SqlDouble[]) store; 
            //SetNullStorage(nullbits);
        }          
    }
}
