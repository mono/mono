//------------------------------------------------------------------------------
// <copyright file="ObjectStorage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {
    using System;
    using System.Data;
    using System.Xml;
    using System.IO;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal sealed class ObjectStorage : DataStorage {

        static private readonly Object defaultValue = null;

        private enum Families { DATETIME, NUMBER, STRING, BOOLEAN, ARRAY };

        private object[] values;
        private readonly bool implementsIXmlSerializable;

        internal ObjectStorage(DataColumn column, Type type)
        : base(column, type, defaultValue, DBNull.Value, typeof(ICloneable).IsAssignableFrom(type), GetStorageType(type)) {
            implementsIXmlSerializable =  typeof(IXmlSerializable).IsAssignableFrom(type);
        }

        override public Object Aggregate(int[] records, AggregateType kind) {
            throw ExceptionBuilder.AggregateException(kind, DataType);
        }

        override public int Compare(int recordNo1, int recordNo2) {
            object valueNo1 = values[recordNo1];
            object valueNo2 = values[recordNo2];

            if (valueNo1 == valueNo2)
                return 0;
            if (valueNo1 == null)
                return -1;
            if (valueNo2 == null)
                return 1;

            IComparable icomparable = (valueNo1 as IComparable);
            if (null != icomparable) {
                try {
                    return icomparable.CompareTo(valueNo2);
                }
                catch(ArgumentException e) {
                    ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                }
            }
            return CompareWithFamilies(valueNo1, valueNo2);
        }

        override public int CompareValueTo(int recordNo1, Object value) {
            object valueNo1 = Get(recordNo1);

            if (valueNo1 is IComparable) {
                if (value.GetType() == valueNo1.GetType())
                    return((IComparable) valueNo1).CompareTo(value);
            }

            if (valueNo1 == value)
                return 0;

            if (valueNo1 == null) {
                if (NullValue == value) {
                    return 0;
                }
                return -1;
            }
            if ((NullValue == value) || (null == value)) {
                return 1;
            }

            return CompareWithFamilies(valueNo1, value);
        }


       private int CompareTo(object valueNo1, object valueNo2) {
           if (valueNo1 == null)
               return -1;
           if (valueNo2 == null)
               return 1;
           if (valueNo1 == valueNo2)
                return 0;
           if (valueNo1 == NullValue)
                return -1;
           if (valueNo2 == NullValue)
                return 1;

           if (valueNo1 is IComparable) {
                try{
                    return ((IComparable) valueNo1).CompareTo(valueNo2);
                }
                catch(ArgumentException e) {
                    ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                }
            }
            return CompareWithFamilies(valueNo1, valueNo2);
        }

        private int CompareWithFamilies(Object valueNo1, Object valueNo2) {
            Families Family1 = GetFamily(valueNo1.GetType());
            Families Family2 = GetFamily(valueNo2.GetType());
            if (Family1 < Family2)
                return -1;
            else
                if (Family1 > Family2)
                    return 1;
            else {
                switch (Family1) {
                    case Families.BOOLEAN :
                        valueNo1 = Convert.ToBoolean(valueNo1, FormatProvider);
                        valueNo2 = Convert.ToBoolean(valueNo2, FormatProvider);
                        break;
                    case Families.DATETIME:
                        valueNo1 = Convert.ToDateTime(valueNo1, FormatProvider);
                        valueNo2 = Convert.ToDateTime(valueNo1, FormatProvider);
                        break;
                    case Families.NUMBER :
                        valueNo1 = Convert.ToDouble(valueNo1, FormatProvider);
                        valueNo2 = Convert.ToDouble(valueNo2, FormatProvider);
                        break;
                     case Families.ARRAY :{
                              Array arr1 = (Array) valueNo1;
                              Array arr2 = (Array) valueNo2;
                              if (arr1.Length  > arr2.Length)
                                  return 1;
                              else if (arr1.Length  < arr2.Length)
                                  return -1;
                              else { // same number of elements
                                   for (int i = 0; i < arr1.Length; i++){
                                         int c = CompareTo(arr1.GetValue(i),arr2.GetValue(i));
                                         if (c != 0)
                                            return  c ;
                                   }
                               }
                              return 0;
                     }
                    default :
                        valueNo1 = valueNo1.ToString();
                        valueNo2 = valueNo2.ToString();
                        break;

                }
                return ((IComparable) valueNo1).CompareTo(valueNo2);
            }
        }

        override public void Copy(int recordNo1, int recordNo2) {
            values[recordNo2] = values[recordNo1];
        }

        override public Object Get(int recordNo) {
            Object value = values[recordNo];
            if (null != value) {
                return value;
            }
            return NullValue;
        }

        private Families GetFamily(Type dataType) {
            switch (Type.GetTypeCode(dataType)) {
                case TypeCode.Boolean:   return Families.BOOLEAN;
                case TypeCode.Char:      return Families.STRING;
                case TypeCode.SByte:     return Families.STRING;
                case TypeCode.Byte:      return Families.STRING;
                case TypeCode.Int16:     return Families.NUMBER;
                case TypeCode.UInt16:    return Families.NUMBER;
                case TypeCode.Int32:     return Families.NUMBER;
                case TypeCode.UInt32:    return Families.NUMBER;
                case TypeCode.Int64:     return Families.NUMBER;
                case TypeCode.UInt64:    return Families.NUMBER;
                case TypeCode.Single:    return Families.NUMBER;
                case TypeCode.Double:    return Families.NUMBER;
                case TypeCode.Decimal:   return Families.NUMBER;
                case TypeCode.DateTime:  return Families.DATETIME;
                case TypeCode.String:    return Families.STRING;
                default:
                    if (typeof(TimeSpan) == dataType) {
                         return Families.DATETIME;
                     }
                    else  if(dataType.IsArray) {
                    	    return Families.ARRAY;
                    	}
                      else{
                              return Families.STRING;
                    	  }
                     }
            }

        override public bool IsNull(int record) {
            return (null == values[record]);
        }

        override public void Set(int recordNo, Object value) {
            System.Diagnostics.Debug.Assert(null != value, "null value");
            if (NullValue == value) {
                values[recordNo] = null;
            }
            else if (DataType == typeof(Object) || DataType.IsInstanceOfType(value)) {
                values[recordNo] = value;
            }
            else {
                Type valType = value.GetType();
                if (DataType == typeof(Guid) && valType == typeof(string)){
                    values[recordNo] = new Guid((string)value);
                }
                else if (DataType == typeof(byte[])) {
                    if (valType == typeof(Boolean)){
                        values[recordNo] = BitConverter.GetBytes((Boolean)value);
                    }
                    else if (valType == typeof(Char)){
                        values[recordNo] = BitConverter.GetBytes((Char)value);
                    }
                    else if (valType == typeof(Int16)){
                        values[recordNo] = BitConverter.GetBytes((Int16)value);
                    }
                    else if (valType == typeof(Int32)){
                        values[recordNo] = BitConverter.GetBytes((Int32)value);
                    }
                    else if (valType == typeof(Int64)){
                        values[recordNo] = BitConverter.GetBytes((Int64)value);
                    }
                    else if (valType == typeof(UInt16)){
                        values[recordNo] = BitConverter.GetBytes((UInt16)value);
                    }
                    else if (valType == typeof(UInt32)){
                        values[recordNo] = BitConverter.GetBytes((UInt32)value);
                    }
                    else if (valType == typeof(UInt64)){
                        values[recordNo] = BitConverter.GetBytes((UInt64)value);
                    }
                    else if (valType == typeof(Single)){
                        values[recordNo] = BitConverter.GetBytes((Single)value);
                    }
                    else if (valType == typeof(Double)){
                        values[recordNo] = BitConverter.GetBytes((Double)value);
                    }
                    else {
                        throw ExceptionBuilder.StorageSetFailed();
                    }
                }
                else {
                    throw ExceptionBuilder.StorageSetFailed();
                }
            }
        }

        override public void SetCapacity(int capacity) {
            object[] newValues = new object[capacity];
            if (values != null) {
                Array.Copy(values, 0, newValues, 0, Math.Min(capacity, values.Length));
            }
            values = newValues;
        }

        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)]
        override public object ConvertXmlToObject(string s) {

            Type type = DataType; // real type of objects in this column

            if (type == typeof(byte[])) {
                return Convert.FromBase64String(s);
            }
            if (type == typeof(Type)){
                return Type.GetType(s);
            }
            if (type == typeof (Guid)){
                return (new Guid(s));
            }
            if (type == typeof (Uri)){
                return (new Uri(s));
            }


            if (implementsIXmlSerializable) {
                object Obj = System.Activator.CreateInstance(DataType, true);
                StringReader strReader = new  StringReader(s);
                using (XmlTextReader xmlTextReader = new XmlTextReader(strReader)) {
                    ((IXmlSerializable)Obj).ReadXml(xmlTextReader);
                }
                return Obj;
            }

            StringReader strreader = new  StringReader(s);
            XmlSerializer deserializerWithOutRootAttribute = ObjectStorage.GetXmlSerializer(type);
            return(deserializerWithOutRootAttribute.Deserialize(strreader));
        }

        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override object ConvertXmlToObject(XmlReader xmlReader, XmlRootAttribute xmlAttrib) {
            object retValue = null;
            bool isBaseCLRType = false;
            bool legacyUDT = false; // in 1.0 and 1.1 we used to call ToString on CDT obj. so if we have the same case
            // we need to handle the case when we have column type as object.
            if (null == xmlAttrib) { // this means type implements IXmlSerializable
                Type type = null;
                string typeName = xmlReader.GetAttribute(Keywords.MSD_INSTANCETYPE, Keywords.MSDNS);
                if (typeName == null || typeName.Length == 0) { // No CDT polumorphism
                    string xsdTypeName = xmlReader.GetAttribute(Keywords.TYPE, Keywords.XSINS); // this xsd type: Base type polymorphism
                    if (null != xsdTypeName && xsdTypeName.Length > 0) {
                        string [] _typename = xsdTypeName.Split(':');
                        if (_typename.Length == 2) { // split will return aray of size 1 if ":" is not there
                            if (xmlReader.LookupNamespace(_typename[0]) == Keywords.XSDNS) {
                                xsdTypeName = _typename[1]; // trim the prefix and just continue with
                            }
                        } // for other case, let say we have two ':' in type, the we throws (as old behavior)
                        type = XSDSchema.XsdtoClr(xsdTypeName);
                        isBaseCLRType = true;
                    }
                    else if (DataType == typeof(object)) {// there is no Keywords.MSD_INSTANCETYPE and no Keywords.TYPE
                            legacyUDT = true;             // see if our type is object
                    }
                }

                if (legacyUDT) { // if Everett UDT, just read it and return string
                    retValue = xmlReader.ReadString();
                }
                else {
                    if (typeName == Keywords.TYPEINSTANCE) {
                        retValue = Type.GetType(xmlReader.ReadString());
                        xmlReader.Read(); // need to move to next node
                    }
                    else {
                        if (null == type) {
                            type = (typeName == null)? DataType : DataStorage.GetType(typeName);
                        }

                        if (type == typeof(char) || type == typeof(Guid)) { //msdata:char and msdata:guid imply base types.
                            isBaseCLRType=true;
                        }

                        if (type == typeof(object))
                            throw ExceptionBuilder.CanNotDeserializeObjectType();
                        if (!isBaseCLRType){
                            retValue = System.Activator.CreateInstance (type, true);
                            Debug.Assert(xmlReader is DataTextReader, "Invalid DataTextReader is being passed to customer");
                            ((IXmlSerializable)retValue).ReadXml(xmlReader);
                        }
                        else {  // Process Base CLR type
                        // for Element Node, if it is Empty, ReadString does not move to End Element; we need to move it
                            if(type == typeof(string) && xmlReader.NodeType == XmlNodeType.Element && xmlReader.IsEmptyElement) {
                                retValue = string.Empty;
                            }
                            else {
                                retValue = xmlReader.ReadString();
                                if (type != typeof(byte[])) {
                                    retValue = SqlConvert.ChangeTypeForXML(retValue, type);
                                }
                                else {
                                    retValue = Convert.FromBase64String(retValue.ToString());
                                }
                            }
                            xmlReader.Read();
                        }
                    }
                }
            }
            else{
                XmlSerializer deserializerWithRootAttribute = ObjectStorage.GetXmlSerializer(DataType, xmlAttrib);
                retValue = deserializerWithRootAttribute.Deserialize(xmlReader);
            }
            return retValue;
        }

        override public string ConvertObjectToXml(object value) {
            if ((value == null) || (value == NullValue))// this case wont happen,  this is added in case if code in xml saver changes
            	return String.Empty;

            Type type = DataType;
            if (type == typeof(byte[]) || (type == typeof(object) && (value is byte[]))) {
                return Convert.ToBase64String((byte[])value);
            }
            if ((type == typeof(Type)) || ((type == typeof(Object)) && (value is Type))) {
                return ((Type)value).AssemblyQualifiedName;
            }

            if (!IsTypeCustomType(value.GetType())){ // Guid and Type had TypeCode.Object
               return (string)SqlConvert.ChangeTypeForXML(value, typeof(string));
            }

            if (Type.GetTypeCode(value.GetType()) != TypeCode.Object) {
            	return value.ToString();
            }

            StringWriter strwriter = new StringWriter(FormatProvider);
            if (implementsIXmlSerializable) {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter (strwriter)) {
                    ((IXmlSerializable)value).WriteXml(xmlTextWriter);
                }
                return (strwriter.ToString());
             }
            
            XmlSerializer serializerWithOutRootAttribute = ObjectStorage.GetXmlSerializer(value.GetType());
            serializerWithOutRootAttribute.Serialize( strwriter, value);
            return strwriter.ToString();
        }

        public override void ConvertObjectToXml(object value, XmlWriter xmlWriter, XmlRootAttribute xmlAttrib) {
            if (null == xmlAttrib) { // implements IXmlSerializable
                Debug.Assert(xmlWriter is DataTextWriter, "Invalid DataTextWriter is being passed to customer");
                ((IXmlSerializable)value).WriteXml(xmlWriter);
            }
            else {
                XmlSerializer serializerWithRootAttribute = ObjectStorage.GetXmlSerializer(value.GetType(), xmlAttrib);
                serializerWithRootAttribute.Serialize(xmlWriter, value);
            }
        }

        override protected object GetEmptyStorage(int recordCount) {
            return new Object[recordCount];
        }

        override protected void CopyValue(int record, object store, BitArray nullbits, int storeIndex) {
            Object[] typedStore = (Object[]) store;
            typedStore[storeIndex] = values[record];
            bool isNull = IsNull(record);
            nullbits.Set(storeIndex, isNull);

            if (!isNull && (typedStore[storeIndex] is DateTime)) {
                DateTime dt = (DateTime)typedStore[storeIndex];
                if (dt.Kind == DateTimeKind.Local) {
                    typedStore[storeIndex] = DateTime.SpecifyKind(dt.ToUniversalTime(), DateTimeKind.Local);
                }
            }
        }

        override protected void SetStorage(object store, BitArray nullbits) {
            values = (Object[]) store;
            for(int i = 0; i < values.Length; i++) {
                if (values[i] is DateTime) {
                    DateTime dt = (DateTime) values[i];
                    if (dt.Kind == DateTimeKind.Local) {
                        values[i] = (DateTime.SpecifyKind(dt, DateTimeKind.Utc)).ToLocalTime();
                     }
                }
            }
//            SetNullStorage(nullbits); -> No need to set bits,
        }

        // SQLBU 431443: dynamically generated assemblies not cached for XmlSerialization when serializable Udt does not implement IXmlSerializable, "memeory leak"
        private static readonly object _tempAssemblyCacheLock = new object();
        private static Dictionary<KeyValuePair<Type,XmlRootAttribute>, XmlSerializer> _tempAssemblyCache;
        private static readonly XmlSerializerFactory _serializerFactory = new XmlSerializerFactory();

        /// <summary>
        /// throw an InvalidOperationException if type implements IDynamicMetaObjectProvider and not IXmlSerializable
        /// because XmlSerializerFactory will only serialize the type's declared properties, not its dynamic properties
        /// </summary>
        /// <param name="type">type to test for IDynamicMetaObjectProvider</param>
        /// <exception cref="InvalidOperationException">DataSet will not serialize types that implement IDynamicMetaObjectProvider but do not also implement IXmlSerializable</exception>
        /// <remarks>IDynamicMetaObjectProvider was introduced in .Net Framework V4.0 into System.Core</remarks>
        internal static void VerifyIDynamicMetaObjectProvider(Type type)
        {
            if (typeof(System.Dynamic.IDynamicMetaObjectProvider).IsAssignableFrom(type) &&
               !typeof(System.Xml.Serialization.IXmlSerializable).IsAssignableFrom(type))
            {
                throw ADP.InvalidOperation(Res.GetString(Res.Xml_DynamicWithoutXmlSerializable));
            }
        }

        internal static XmlSerializer GetXmlSerializer(Type type)
        {
            // Dev10 671061: prevent writing an instance which implements IDynamicMetaObjectProvider and not IXmlSerializable
            // the check here prevents the instance data from being written
            VerifyIDynamicMetaObjectProvider(type);

            // use factory which caches XmlSerializer as necessary
            XmlSerializer serializer = _serializerFactory.CreateSerializer(type);
            return serializer;
        }

        internal static XmlSerializer GetXmlSerializer(Type type, XmlRootAttribute attribute)
        {
            XmlSerializer serializer = null;
            KeyValuePair<Type,XmlRootAttribute> key = new KeyValuePair<Type,XmlRootAttribute>(type,attribute);

            // _tempAssemblyCache is a readonly instance, lock on write to copy & add then replace the original instance.
            Dictionary<KeyValuePair<Type,XmlRootAttribute>, XmlSerializer> cache = _tempAssemblyCache;
            if ((null == cache) || !cache.TryGetValue(key, out serializer))
            {   // not in cache, try again with lock because it may need to grow
                lock(_tempAssemblyCacheLock)
                {   
                    cache = _tempAssemblyCache;
                    if ((null == cache) || !cache.TryGetValue(key, out serializer))
                    {
                        // Dev10 671061: prevent writing an instance which implements IDynamicMetaObjectProvider and not IXmlSerializable
                        // the check here prevents the instance data from being written
                        VerifyIDynamicMetaObjectProvider(type);

                        // if still not in cache, make cache larger and add new XmlSerializer
                        if (null != cache)
                        {   // create larger cache, because dictionary is not reader/writer safe
                            // copy cache so that all readers don't take lock - only potential new writers
                            // same logic used by DbConnectionFactory
                            Dictionary<KeyValuePair<Type,XmlRootAttribute>, XmlSerializer> tmp =
                                new Dictionary<KeyValuePair<Type,XmlRootAttribute>, XmlSerializer>(
                                    1+cache.Count, TempAssemblyComparer.Default);
                            foreach (KeyValuePair<KeyValuePair<Type,XmlRootAttribute>, XmlSerializer> entry in cache)
                            {   // copy contents from old cache to new cache
                                tmp.Add(entry.Key, entry.Value);
                            }
                            cache = tmp;
                        }
                        else
                        {   // initial creation of cache
                            cache = new Dictionary<KeyValuePair<Type,XmlRootAttribute>, XmlSerializer>(
                                TempAssemblyComparer.Default);
                        }

                        // attribute is modifiable - but usuage from XmlSaver & XmlDataLoader & XmlDiffLoader 
                        // the instances are not modified, but to be safe - copy the XmlRootAttribute before caching
                        key = new KeyValuePair<Type,XmlRootAttribute>(type, new XmlRootAttribute());
                        key.Value.ElementName = attribute.ElementName;
                        key.Value.Namespace = attribute.Namespace;
                        key.Value.DataType = attribute.DataType;
                        key.Value.IsNullable = attribute.IsNullable;

                        serializer = _serializerFactory.CreateSerializer(type, attribute);
                        cache.Add(key, serializer);
                        _tempAssemblyCache = cache;
                    }
                }
            }
            return serializer;
        }

        private class TempAssemblyComparer : IEqualityComparer<KeyValuePair<Type,XmlRootAttribute>>
        {
            internal static readonly IEqualityComparer<KeyValuePair<Type,XmlRootAttribute>> Default = new TempAssemblyComparer();

            private TempAssemblyComparer() { }

            public bool Equals(KeyValuePair<Type,XmlRootAttribute> x, KeyValuePair<Type,XmlRootAttribute> y)
            {
                return ((x.Key == y.Key) &&                                         // same type
                        (((x.Value == null) && (y.Value == null)) ||                // XmlRootAttribute both null
                         ((x.Value != null) && (y.Value != null) &&                 // XmlRootAttribute both with value
                          (x.Value.ElementName == y.Value.ElementName) &&           // all attribute elements are equal
                          (x.Value.Namespace == y.Value.Namespace) &&
                          (x.Value.DataType == y.Value.DataType) &&
                          (x.Value.IsNullable == y.Value.IsNullable))));
            }

            public int GetHashCode(KeyValuePair<Type,XmlRootAttribute> obj)
            {
                return unchecked(obj.Key.GetHashCode() + obj.Value.ElementName.GetHashCode());
            }
        }
    }
}

