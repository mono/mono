//------------------------------------------------------------------------------
// <copyright file="SqlUDTStorage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Xml;
    using System.IO;
    using System.Xml.Serialization;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal sealed class SqlUdtStorage : DataStorage {

        private object[] values;
        private readonly bool  implementsIXmlSerializable = false;
        private readonly bool  implementsIComparable = false;

        private static readonly Dictionary<Type,object> TypeToNull = new Dictionary<Type,object>();

        public SqlUdtStorage(DataColumn column, Type type)
        : this(column, type, GetStaticNullForUdtType(type)) {
        }

        private SqlUdtStorage(DataColumn column, Type type, object nullValue)
        : base(column, type, nullValue, nullValue, typeof(ICloneable).IsAssignableFrom(type), GetStorageType(type)) {
            implementsIXmlSerializable =  typeof(IXmlSerializable).IsAssignableFrom(type);
            implementsIComparable = typeof(IComparable).IsAssignableFrom(type);
        }

      // Webdata 104340, to support oracle types and other INUllable types that have static Null as field
        internal static object GetStaticNullForUdtType(Type type) {
            object value;
            if (!TypeToNull.TryGetValue(type, out value)) {
                System.Reflection.PropertyInfo propInfo = type.GetProperty("Null", System.Reflection.BindingFlags.Public |System.Reflection.BindingFlags.Static);
                if (propInfo  != null)
                    value = propInfo.GetValue(null, null);
                else {
                    System.Reflection.FieldInfo fieldInfo = type.GetField("Null", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (fieldInfo != null) {
                        value =  fieldInfo.GetValue(null);
                    }
                    else {
                        throw ExceptionBuilder.INullableUDTwithoutStaticNull(type.AssemblyQualifiedName);
                    }
                }
                lock(TypeToNull) {
                    //if(50 < TypeToNull.Count) {
                    //    TypeToNull.Clear();
                    //}
                    TypeToNull[type] = value;
                }
            }
            return value;
        }

        override public bool IsNull(int record) {
            return (((INullable)values[record]).IsNull);
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            return (CompareValueTo(recordNo1, values[recordNo2]));
        }

        override public int CompareValueTo(int recordNo1, Object value) {
            if (DBNull.Value == value) { // it is not meaningful compare UDT with DBNull.Value   WebData 113372
                value = NullValue;
            }
            if(implementsIComparable) {
                IComparable comparable = (IComparable)values[recordNo1];
                return comparable.CompareTo(value);
            }
            else if (NullValue == value) {
                INullable nullableValue = (INullable)values[recordNo1];
                return nullableValue.IsNull ? 0 : 1; // left may be null, right is null
            }
            // else 
            throw ExceptionBuilder.IComparableNotImplemented(DataType.AssemblyQualifiedName);
        }

        override public void Copy(int recordNo1, int recordNo2) {
            CopyBits(recordNo1, recordNo2);
            values[recordNo2] = values[recordNo1];
        }

        override public Object Get(int recordNo) {
            return (values[recordNo]);
        }

        override public void Set(int recordNo, Object value) {
            if (DBNull.Value == value) {
                values[recordNo] = NullValue;
                SetNullBit(recordNo, true);
            }
            else if (null == value) {
                if (IsValueType) {
                    throw ExceptionBuilder.StorageSetFailed();
                }
                else {
                    values[recordNo] = NullValue;
                    SetNullBit(recordNo, true);
                }
            }
            else if (!DataType.IsInstanceOfType(value)) {
                throw ExceptionBuilder.StorageSetFailed();
            }
            else { // WebData  113331 do not clone the value
                values[recordNo] = value;
                SetNullBit(recordNo, false);
            }
        }

        override public void SetCapacity(int capacity) {
            object[] newValues = new object[capacity];
            if (values != null) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
            base.SetCapacity(capacity);
        }

        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)]
        override public object ConvertXmlToObject(string s) {
            if (implementsIXmlSerializable) {
                object Obj = System.Activator.CreateInstance (DataType, true);

                string tempStr =string.Concat("<col>", s, "</col>"); // this is done since you can give fragmet to reader, bug 98767
                StringReader strReader = new  StringReader(tempStr);

                using (XmlTextReader xmlTextReader = new XmlTextReader(strReader)) {
                    ((IXmlSerializable)Obj).ReadXml(xmlTextReader);
                }
                return Obj;
            }

            StringReader strreader = new  StringReader(s);
            XmlSerializer deserializerWithOutRootAttribute = ObjectStorage.GetXmlSerializer(DataType);
            return(deserializerWithOutRootAttribute.Deserialize(strreader));
        }
        
        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override object ConvertXmlToObject(XmlReader xmlReader, XmlRootAttribute xmlAttrib) {
            if (null == xmlAttrib) {
                string typeName = xmlReader.GetAttribute(Keywords.MSD_INSTANCETYPE, Keywords.MSDNS);
                if (typeName == null) {
                    string xsdTypeName = xmlReader.GetAttribute(Keywords.MSD_INSTANCETYPE, Keywords.XSINS); // this xsd type
                    if (null != xsdTypeName) {
                        typeName = XSDSchema.XsdtoClr(xsdTypeName).FullName;
                    }
                }
                Type type = (typeName == null)? DataType : Type.GetType(typeName);
                object Obj = System.Activator.CreateInstance (type, true);
                Debug.Assert(xmlReader is DataTextReader, "Invalid DataTextReader is being passed to customer");
                ((IXmlSerializable)Obj).ReadXml(xmlReader);
                return Obj;
            }
            else{
                XmlSerializer deserializerWithRootAttribute = ObjectStorage.GetXmlSerializer(DataType, xmlAttrib);
                return(deserializerWithRootAttribute.Deserialize(xmlReader));
            }
        } 


        override public string ConvertObjectToXml(object value) {
            StringWriter strwriter = new StringWriter(FormatProvider);
            if (implementsIXmlSerializable) {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter (strwriter)) {
                    ((IXmlSerializable)value).WriteXml(xmlTextWriter);
                }
             }
            else {
                XmlSerializer serializerWithOutRootAttribute = ObjectStorage.GetXmlSerializer(value.GetType());
                serializerWithOutRootAttribute.Serialize(strwriter, value);
            }
            return (strwriter.ToString ());
        }

        public override void ConvertObjectToXml(object value, XmlWriter xmlWriter, XmlRootAttribute xmlAttrib) {
            if (null == xmlAttrib) {
                Debug.Assert(xmlWriter is DataTextWriter, "Invalid DataTextWriter is being passed to customer");
                ((IXmlSerializable)value).WriteXml(xmlWriter);
            }
            else {
                // we support polymorphism only for types that implements IXmlSerializable.
                // Assumption: value is the same type as DataType

                XmlSerializer serializerWithRootAttribute = ObjectStorage.GetXmlSerializer(DataType, xmlAttrib);
                serializerWithRootAttribute.Serialize(xmlWriter, value);
            }
        }
        
        override protected object GetEmptyStorage(int recordCount) {
            return new Object[recordCount];
        }

        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            Object[] typedStore = (Object[]) store;
            typedStore[storeIndex] = values[record];
            nullbits.Set(storeIndex, IsNull(record));
        }

        override protected void SetStorage(object store, BitArray nullbits) {
            values = (Object[]) store;
            //SetNullStorage(nullbits);
        }
    }
}
