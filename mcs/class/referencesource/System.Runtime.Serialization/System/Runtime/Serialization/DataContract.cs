//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Configuration;
    using System.Runtime.Serialization.Diagnostics.Application;
    using System.Security;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using DataContractDictionary = System.Collections.Generic.Dictionary<System.Xml.XmlQualifiedName, DataContract>;
    using System.Text.RegularExpressions;

#if USE_REFEMIT
    public abstract class DataContract
#else
    internal abstract class DataContract
#endif
    {
        [Fx.Tag.SecurityNote(Critical = "XmlDictionaryString representing the type name. Statically cached and used from IL generated code.")]
        [SecurityCritical]
        XmlDictionaryString name;

        [Fx.Tag.SecurityNote(Critical = "XmlDictionaryString representing the type name. Statically cached and used from IL generated code.")]
        [SecurityCritical]
        XmlDictionaryString ns;

        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        DataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        protected DataContract(DataContractCriticalHelper helper)
        {
            this.helper = helper;
            this.name = helper.Name;
            this.ns = helper.Namespace;
        }

        internal static DataContract GetDataContract(Type type)
        {
            return GetDataContract(type.TypeHandle, type, SerializationMode.SharedContract);
        }

        internal static DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type, SerializationMode mode)
        {
            int id = GetId(typeHandle);
            return GetDataContract(id, typeHandle, mode);
        }

        internal static DataContract GetDataContract(int id, RuntimeTypeHandle typeHandle, SerializationMode mode)
        {
            DataContract dataContract = GetDataContractSkipValidation(id, typeHandle, null);
            dataContract = dataContract.GetValidContract(mode);

            return dataContract;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up DataContract .",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        internal static DataContract GetDataContractSkipValidation(int id, RuntimeTypeHandle typeHandle, Type type)
        {
            return DataContractCriticalHelper.GetDataContractSkipValidation(id, typeHandle, type);
        }

        internal static DataContract GetGetOnlyCollectionDataContract(int id, RuntimeTypeHandle typeHandle, Type type, SerializationMode mode)
        {
            DataContract dataContract = GetGetOnlyCollectionDataContractSkipValidation(id, typeHandle, type);
            dataContract = dataContract.GetValidContract(mode);
            if (dataContract is ClassDataContract)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.ClassDataContractReturnedForGetOnlyCollection, DataContract.GetClrTypeFullName(dataContract.UnderlyingType))));
            }
            return dataContract;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up DataContract .",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        internal static DataContract GetGetOnlyCollectionDataContractSkipValidation(int id, RuntimeTypeHandle typeHandle, Type type)
        {
            return DataContractCriticalHelper.GetGetOnlyCollectionDataContractSkipValidation(id, typeHandle, type);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up DataContract .",
            Safe = "Read only access; doesn't modify any static information.")]
        [SecuritySafeCritical]
        internal static DataContract GetDataContractForInitialization(int id)
        {
            return DataContractCriticalHelper.GetDataContractForInitialization(id);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up id for DataContract .",
            Safe = "Read only access; doesn't modify any static information.")]
        [SecuritySafeCritical]
        internal static int GetIdForInitialization(ClassDataContract classContract)
        {
            return DataContractCriticalHelper.GetIdForInitialization(classContract);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up id assigned to a particular type.",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        internal static int GetId(RuntimeTypeHandle typeHandle)
        {
            return DataContractCriticalHelper.GetId(typeHandle);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up DataContract.",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        public static DataContract GetBuiltInDataContract(Type type)
        {
            return DataContractCriticalHelper.GetBuiltInDataContract(type);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up DataContract.",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        public static DataContract GetBuiltInDataContract(string name, string ns)
        {
            return DataContractCriticalHelper.GetBuiltInDataContract(name, ns);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up DataContract.",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        public static DataContract GetBuiltInDataContract(string typeName)
        {
            return DataContractCriticalHelper.GetBuiltInDataContract(typeName);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up string reference to use for a namespace string.",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        internal static string GetNamespace(string key)
        {
            return DataContractCriticalHelper.GetNamespace(key);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up XmlDictionaryString for a string.",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        internal static XmlDictionaryString GetClrTypeString(string key)
        {
            return DataContractCriticalHelper.GetClrTypeString(key);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to remove invalid DataContract if it has been added to cache.",
            Safe = "Doesn't leak any information.")]
        [SecuritySafeCritical]
        internal static void ThrowInvalidDataContractException(string message, Type type)
        {
            DataContractCriticalHelper.ThrowInvalidDataContractException(message, type);
        }

#if USE_REFEMIT
        internal DataContractCriticalHelper Helper
#else
        protected DataContractCriticalHelper Helper
#endif
        {
            [Fx.Tag.SecurityNote(Critical = "holds instance of CriticalHelper which keeps state that is cached statically for serialization."
                + " Static fields are marked SecurityCritical or readonly to prevent"
                + " data from being modified or leaked to other components in appdomain.")]
            [SecurityCritical]
            get { return helper; }
        }

        internal Type UnderlyingType
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical UnderlyingType field.",
                Safe = "Get-only properties only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.UnderlyingType; }
        }

        internal Type OriginalUnderlyingType
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical OriginalUnderlyingType property.",
                Safe = "OrginalUnderlyingType only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.OriginalUnderlyingType; }
        }


        internal virtual bool IsBuiltInDataContract
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical isBuiltInDataContract property.",
                Safe = "isBuiltInDataContract only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsBuiltInDataContract; }
        }

        internal Type TypeForInitialization
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical typeForInitialization property.",
                Safe = "typeForInitialization only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.TypeForInitialization; }
        }

        public virtual void WriteXmlValue(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context)
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.UnexpectedContractType, DataContract.GetClrTypeFullName(this.GetType()), DataContract.GetClrTypeFullName(UnderlyingType))));
        }

        public virtual object ReadXmlValue(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context)
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.UnexpectedContractType, DataContract.GetClrTypeFullName(this.GetType()), DataContract.GetClrTypeFullName(UnderlyingType))));
        }

        internal bool IsValueType
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical IsValueType property.",
                Safe = "IsValueType only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsValueType; }
            [Fx.Tag.SecurityNote(Critical = "Sets the critical IsValueType property.")]
            [SecurityCritical]
            set { helper.IsValueType = value; }
        }

        internal bool IsReference
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical IsReference property.",
                Safe = "IsReference only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsReference; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical IsReference property.")]
            [SecurityCritical]
            set { helper.IsReference = value; }
        }

        internal XmlQualifiedName StableName
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical StableName property.",
                Safe = "StableName only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.StableName; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical StableName property.")]
            [SecurityCritical]
            set { helper.StableName = value; }
        }

        internal GenericInfo GenericInfo
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical GenericInfo property.",
                Safe = "GenericInfo only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.GenericInfo; }
            [Fx.Tag.SecurityNote(Critical = "Sets the critical GenericInfo property.",
                Safe = "Protected for write if contract has underlyingType .")]
            [SecurityCritical]
            set { helper.GenericInfo = value; }
        }

        internal virtual DataContractDictionary KnownDataContracts
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical KnownDataContracts property.",
                Safe = "KnownDataContracts only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.KnownDataContracts; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical KnownDataContracts property.")]
            [SecurityCritical]
            set { helper.KnownDataContracts = value; }
        }

        internal virtual bool IsISerializable
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical IsISerializable property.",
                Safe = "IsISerializable only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsISerializable; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical IsISerializable property.")]
            [SecurityCritical]
            set { helper.IsISerializable = value; }
        }

        internal XmlDictionaryString Name
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical Name property.",
                Safe = "Name only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return this.name; }
        }

        public virtual XmlDictionaryString Namespace
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical Namespace property.",
                Safe = "Namespace only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return this.ns; }
        }

        internal virtual bool HasRoot
        {
            get { return true; }
            set { }
        }

        internal virtual XmlDictionaryString TopLevelElementName
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical Name property.",
                Safe = "Name only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.TopLevelElementName; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical Name property.")]
            [SecurityCritical]
            set { helper.TopLevelElementName = value; }
        }

        internal virtual XmlDictionaryString TopLevelElementNamespace
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical Namespace property.",
                Safe = "Namespace only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.TopLevelElementNamespace; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical Namespace property.")]
            [SecurityCritical]
            set { helper.TopLevelElementNamespace = value; }
        }

        internal virtual bool CanContainReferences
        {
            get { return true; }
        }

        internal virtual bool IsPrimitive
        {
            get { return false; }
        }

        internal virtual void WriteRootElement(XmlWriterDelegator writer, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (object.ReferenceEquals(ns, DictionaryGlobals.SerializationNamespace) && !IsPrimitive)
                writer.WriteStartElement(Globals.SerPrefix, name, ns);
            else
                writer.WriteStartElement(name, ns);
        }

        internal virtual DataContract BindGenericParameters(DataContract[] paramContracts, Dictionary<DataContract, DataContract> boundContracts)
        {
            return this;
        }

        internal virtual DataContract GetValidContract(SerializationMode mode)
        {
            return this;
        }

        internal virtual DataContract GetValidContract()
        {
            return this;
        }

        internal virtual bool IsValidContract(SerializationMode mode)
        {
            return true;
        }

        internal MethodInfo ParseMethod
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical ParseMethod field.",
                Safe = "Get-only properties only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.ParseMethod; }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds all state used for (de)serializing types."
            + " Since the data is cached statically, we lock down access to it.")]
        [SecurityCritical(SecurityCriticalScope.Everything)]
#if USE_REFEMIT
        public class DataContractCriticalHelper
#else
        protected class DataContractCriticalHelper
#endif
        {
            static Dictionary<TypeHandleRef, IntRef> typeToIDCache;
            static DataContract[] dataContractCache;
            static int dataContractID;
            static Dictionary<Type, DataContract> typeToBuiltInContract;
            static Dictionary<XmlQualifiedName, DataContract> nameToBuiltInContract;
            static Dictionary<string, DataContract> typeNameToBuiltInContract;
            static Dictionary<string, string> namespaces;
            static Dictionary<string, XmlDictionaryString> clrTypeStrings;
            static XmlDictionary clrTypeStringsDictionary;
            static TypeHandleRef typeHandleRef = new TypeHandleRef();

            static object cacheLock = new object();
            static object createDataContractLock = new object();
            static object initBuiltInContractsLock = new object();
            static object namespacesLock = new object();
            static object clrTypeStringsLock = new object();

            readonly Type underlyingType;
            Type originalUnderlyingType;
            bool isReference;
            bool isValueType;
            XmlQualifiedName stableName;
            GenericInfo genericInfo;
            XmlDictionaryString name;
            XmlDictionaryString ns;

            [Fx.Tag.SecurityNote(Critical = "In deserialization, we initialize an object instance passing this Type to GetUninitializedObject method.")]
            Type typeForInitialization;

            MethodInfo parseMethod;
            bool parseMethodSet;

            static DataContractCriticalHelper()
            {
                typeToIDCache = new Dictionary<TypeHandleRef, IntRef>(new TypeHandleRefEqualityComparer());
                dataContractCache = new DataContract[32];
                dataContractID = 0;
            }

            internal static DataContract GetDataContractSkipValidation(int id, RuntimeTypeHandle typeHandle, Type type)
            {
                DataContract dataContract = dataContractCache[id];
                if (dataContract == null)
                {
                    dataContract = CreateDataContract(id, typeHandle, type);
                }
                else
                {
                    return dataContract.GetValidContract();
                }
                return dataContract;
            }

            internal static DataContract GetGetOnlyCollectionDataContractSkipValidation(int id, RuntimeTypeHandle typeHandle, Type type)
            {
                DataContract dataContract = dataContractCache[id];
                if (dataContract == null)
                {
                    dataContract = CreateGetOnlyCollectionDataContract(id, typeHandle, type);
                    dataContractCache[id] = dataContract;
                }
                return dataContract;
            }

            internal static DataContract GetDataContractForInitialization(int id)
            {
                DataContract dataContract = dataContractCache[id];
                if (dataContract == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.DataContractCacheOverflow)));
                }
                return dataContract;
            }

            internal static int GetIdForInitialization(ClassDataContract classContract)
            {
                int id = DataContract.GetId(classContract.TypeForInitialization.TypeHandle);
                if (id < dataContractCache.Length && ContractMatches(classContract, dataContractCache[id]))
                {
                    return id;
                }
                for (int i = 0; i < DataContractCriticalHelper.dataContractID; i++)
                {
                    if (ContractMatches(classContract, dataContractCache[i]))
                    {
                        return i;
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.DataContractCacheOverflow)));
            }

            static bool ContractMatches(DataContract contract, DataContract cachedContract)
            {
                return (cachedContract != null && cachedContract.UnderlyingType == contract.UnderlyingType);
            }

            internal static int GetId(RuntimeTypeHandle typeHandle)
            {
                lock (cacheLock)
                {
                    IntRef id;
                    typeHandle = GetDataContractAdapterTypeHandle(typeHandle);
                    typeHandleRef.Value = typeHandle;
                    if (!typeToIDCache.TryGetValue(typeHandleRef, out id))
                    {
                        id = GetNextId();
                        try
                        {
                            typeToIDCache.Add(new TypeHandleRef(typeHandle), id);
                        }
                        catch (Exception ex)
                        {
                            if (Fx.IsFatal(ex))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(ex.Message, ex);
                        }
                    }
                    return id.Value;
                }
            }

            // Assumed that this method is called under a lock
            static IntRef GetNextId()
            {
                int value = dataContractID++;
                if (value >= dataContractCache.Length)
                {
                    int newSize = (value < Int32.MaxValue / 2) ? value * 2 : Int32.MaxValue;
                    if (newSize <= value)
                    {
                        Fx.Assert("DataContract cache overflow");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.DataContractCacheOverflow)));
                    }
                    Array.Resize<DataContract>(ref dataContractCache, newSize);
                }
                return new IntRef(value);
            }

            // check whether a corresponding update is required in ClassDataContract.IsNonAttributedTypeValidForSerialization
            static DataContract CreateDataContract(int id, RuntimeTypeHandle typeHandle, Type type)
            {
                lock (createDataContractLock)
                {
                    DataContract dataContract = dataContractCache[id];
                    if (dataContract == null)
                    {
                        if (type == null)
                            type = Type.GetTypeFromHandle(typeHandle);
                        type = UnwrapNullableType(type);
                        type = GetDataContractAdapterType(type);
                        dataContract = GetBuiltInDataContract(type);
                        if (dataContract == null)
                        {
                            if (type.IsArray)
                                dataContract = new CollectionDataContract(type);
                            else if (type.IsEnum)
                                dataContract = new EnumDataContract(type);
                            else if (type.IsGenericParameter)
                                dataContract = new GenericParameterDataContract(type);
                            else if (Globals.TypeOfIXmlSerializable.IsAssignableFrom(type))
                                dataContract = new XmlDataContract(type);
                            else
                            {
                                //if (type.ContainsGenericParameters)
                                //    ThrowInvalidDataContractException(SR.GetString(SR.TypeMustNotBeOpenGeneric, type), type);
                                if (type.IsPointer)
                                    type = Globals.TypeOfReflectionPointer;

                                if (!CollectionDataContract.TryCreate(type, out dataContract))
                                {
                                    if (type.IsSerializable || type.IsDefined(Globals.TypeOfDataContractAttribute, false) || ClassDataContract.IsNonAttributedTypeValidForSerialization(type))
                                    {
                                        dataContract = new ClassDataContract(type);
                                    }
                                    else
                                    {
                                        ThrowInvalidDataContractException(SR.GetString(SR.TypeNotSerializable, type), type);
                                    }
                                }
                            }
                        }
                    }
                    dataContractCache[id] = dataContract;
                    return dataContract;
                }
            }

            static DataContract CreateGetOnlyCollectionDataContract(int id, RuntimeTypeHandle typeHandle, Type type)
            {
                DataContract dataContract = null;
                lock (createDataContractLock)
                {
                    dataContract = dataContractCache[id];
                    if (dataContract == null)
                    {
                        if (type == null)
                            type = Type.GetTypeFromHandle(typeHandle);
                        type = UnwrapNullableType(type);
                        type = GetDataContractAdapterType(type);
                        if (!CollectionDataContract.TryCreateGetOnlyCollectionDataContract(type, out dataContract))
                        {
                            ThrowInvalidDataContractException(SR.GetString(SR.TypeNotSerializable, type), type);
                        }
                    }
                }
                return dataContract;
            }

            // Any change to this method should be reflected in GetDataContractOriginalType
            internal static Type GetDataContractAdapterType(Type type)
            {
                // Replace the DataTimeOffset ISerializable type passed in with the internal DateTimeOffsetAdapter DataContract type.
                // DateTimeOffsetAdapter is used for serialization/deserialization purposes to bypass the ISerializable implementation
                // on DateTimeOffset; which does not work in partial trust and to ensure correct schema import/export scenarios.
                if (type == Globals.TypeOfDateTimeOffset)
                {
                    return Globals.TypeOfDateTimeOffsetAdapter;
                }
                return type;
            }

            // Maps adapted types back to the original type
            // Any change to this method should be reflected in GetDataContractAdapterType
            internal static Type GetDataContractOriginalType(Type type)
            {
                if (type == Globals.TypeOfDateTimeOffsetAdapter)
                {
                    return Globals.TypeOfDateTimeOffset;
                }
                return type;
            }

            static RuntimeTypeHandle GetDataContractAdapterTypeHandle(RuntimeTypeHandle typeHandle)
            {
                if (Globals.TypeOfDateTimeOffset.TypeHandle.Equals(typeHandle))
                {
                    return Globals.TypeOfDateTimeOffsetAdapter.TypeHandle;
                }
                return typeHandle;
            }

            [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "typeToBuiltInContract", Justification = "No need to support type equivalence here.")]
            public static DataContract GetBuiltInDataContract(Type type)
            {
                if (type.IsInterface && !CollectionDataContract.IsCollectionInterface(type))
                    type = Globals.TypeOfObject;

                lock (initBuiltInContractsLock)
                {
                    if (typeToBuiltInContract == null)
                        typeToBuiltInContract = new Dictionary<Type, DataContract>();

                    DataContract dataContract = null;
                    if (!typeToBuiltInContract.TryGetValue(type, out dataContract))
                    {
                        TryCreateBuiltInDataContract(type, out dataContract);
                        typeToBuiltInContract.Add(type, dataContract);
                    }
                    return dataContract;
                }
            }

            public static DataContract GetBuiltInDataContract(string name, string ns)
            {
                lock (initBuiltInContractsLock)
                {
                    if (nameToBuiltInContract == null)
                        nameToBuiltInContract = new Dictionary<XmlQualifiedName, DataContract>();

                    DataContract dataContract = null;
                    XmlQualifiedName qname = new XmlQualifiedName(name, ns);
                    if (!nameToBuiltInContract.TryGetValue(qname, out dataContract))
                    {
                        TryCreateBuiltInDataContract(name, ns, out dataContract);
                        nameToBuiltInContract.Add(qname, dataContract);
                    }
                    return dataContract;
                }
            }

            public static DataContract GetBuiltInDataContract(string typeName)
            {
                if (!typeName.StartsWith("System.", StringComparison.Ordinal))
                    return null;

                lock (initBuiltInContractsLock)
                {
                    if (typeNameToBuiltInContract == null)
                        typeNameToBuiltInContract = new Dictionary<string, DataContract>();

                    DataContract dataContract = null;
                    if (!typeNameToBuiltInContract.TryGetValue(typeName, out dataContract))
                    {
                        Type type = null;
                        string name = typeName.Substring(7);
                        if (name == "Char")
                            type = typeof(Char);
                        else if (name == "Boolean")
                            type = typeof(Boolean);
                        else if (name == "SByte")
                            type = typeof(SByte);
                        else if (name == "Byte")
                            type = typeof(Byte);
                        else if (name == "Int16")
                            type = typeof(Int16);
                        else if (name == "UInt16")
                            type = typeof(UInt16);
                        else if (name == "Int32")
                            type = typeof(Int32);
                        else if (name == "UInt32")
                            type = typeof(UInt32);
                        else if (name == "Int64")
                            type = typeof(Int64);
                        else if (name == "UInt64")
                            type = typeof(UInt64);
                        else if (name == "Single")
                            type = typeof(Single);
                        else if (name == "Double")
                            type = typeof(Double);
                        else if (name == "Decimal")
                            type = typeof(Decimal);
                        else if (name == "DateTime")
                            type = typeof(DateTime);
                        else if (name == "String")
                            type = typeof(String);
                        else if (name == "Byte[]")
                            type = typeof(byte[]);
                        else if (name == "Object")
                            type = typeof(Object);
                        else if (name == "TimeSpan")
                            type = typeof(TimeSpan);
                        else if (name == "Guid")
                            type = typeof(Guid);
                        else if (name == "Uri")
                            type = typeof(Uri);
                        else if (name == "Xml.XmlQualifiedName")
                            type = typeof(XmlQualifiedName);
                        else if (name == "Enum")
                            type = typeof(Enum);
                        else if (name == "ValueType")
                            type = typeof(ValueType);
                        else if (name == "Array")
                            type = typeof(Array);
                        else if (name == "Xml.XmlElement")
                            type = typeof(XmlElement);
                        else if (name == "Xml.XmlNode[]")
                            type = typeof(XmlNode[]);

                        if (type != null)
                            TryCreateBuiltInDataContract(type, out dataContract);

                        typeNameToBuiltInContract.Add(typeName, dataContract);
                    }
                    return dataContract;
                }
            }

            static public bool TryCreateBuiltInDataContract(Type type, out DataContract dataContract)
            {
                if (type.IsEnum) // Type.GetTypeCode will report Enums as TypeCode.IntXX
                {
                    dataContract = null;
                    return false;
                }
                dataContract = null;
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        dataContract = new BooleanDataContract();
                        break;
                    case TypeCode.Byte:
                        dataContract = new UnsignedByteDataContract();
                        break;
                    case TypeCode.Char:
                        dataContract = new CharDataContract();
                        break;
                    case TypeCode.DateTime:
                        dataContract = new DateTimeDataContract();
                        break;
                    case TypeCode.Decimal:
                        dataContract = new DecimalDataContract();
                        break;
                    case TypeCode.Double:
                        dataContract = new DoubleDataContract();
                        break;
                    case TypeCode.Int16:
                        dataContract = new ShortDataContract();
                        break;
                    case TypeCode.Int32:
                        dataContract = new IntDataContract();
                        break;
                    case TypeCode.Int64:
                        dataContract = new LongDataContract();
                        break;
                    case TypeCode.SByte:
                        dataContract = new SignedByteDataContract();
                        break;
                    case TypeCode.Single:
                        dataContract = new FloatDataContract();
                        break;
                    case TypeCode.String:
                        dataContract = new StringDataContract();
                        break;
                    case TypeCode.UInt16:
                        dataContract = new UnsignedShortDataContract();
                        break;
                    case TypeCode.UInt32:
                        dataContract = new UnsignedIntDataContract();
                        break;
                    case TypeCode.UInt64:
                        dataContract = new UnsignedLongDataContract();
                        break;
                    default:
                        if (type == typeof(byte[]))
                            dataContract = new ByteArrayDataContract();
                        else if (type == typeof(object))
                            dataContract = new ObjectDataContract();
                        else if (type == typeof(Uri))
                            dataContract = new UriDataContract();
                        else if (type == typeof(XmlQualifiedName))
                            dataContract = new QNameDataContract();
                        else if (type == typeof(TimeSpan))
                            dataContract = new TimeSpanDataContract();
                        else if (type == typeof(Guid))
                            dataContract = new GuidDataContract();
                        else if (type == typeof(Enum) || type == typeof(ValueType))
                        {
                            dataContract = new SpecialTypeDataContract(type, DictionaryGlobals.ObjectLocalName, DictionaryGlobals.SchemaNamespace);
                        }
                        else if (type == typeof(Array))
                            dataContract = new CollectionDataContract(type);
                        else if (type == typeof(XmlElement) || type == typeof(XmlNode[]))
                            dataContract = new XmlDataContract(type);
                        break;
                }
                return dataContract != null;
            }

            static public bool TryCreateBuiltInDataContract(string name, string ns, out DataContract dataContract)
            {
                dataContract = null;
                if (ns == DictionaryGlobals.SchemaNamespace.Value)
                {
                    if (DictionaryGlobals.BooleanLocalName.Value == name)
                        dataContract = new BooleanDataContract();
                    else if (DictionaryGlobals.SignedByteLocalName.Value == name)
                        dataContract = new SignedByteDataContract();
                    else if (DictionaryGlobals.UnsignedByteLocalName.Value == name)
                        dataContract = new UnsignedByteDataContract();
                    else if (DictionaryGlobals.ShortLocalName.Value == name)
                        dataContract = new ShortDataContract();
                    else if (DictionaryGlobals.UnsignedShortLocalName.Value == name)
                        dataContract = new UnsignedShortDataContract();
                    else if (DictionaryGlobals.IntLocalName.Value == name)
                        dataContract = new IntDataContract();
                    else if (DictionaryGlobals.UnsignedIntLocalName.Value == name)
                        dataContract = new UnsignedIntDataContract();
                    else if (DictionaryGlobals.LongLocalName.Value == name)
                        dataContract = new LongDataContract();
                    else if (DictionaryGlobals.integerLocalName.Value == name)
                        dataContract = new IntegerDataContract();
                    else if (DictionaryGlobals.positiveIntegerLocalName.Value == name)
                        dataContract = new PositiveIntegerDataContract();
                    else if (DictionaryGlobals.negativeIntegerLocalName.Value == name)
                        dataContract = new NegativeIntegerDataContract();
                    else if (DictionaryGlobals.nonPositiveIntegerLocalName.Value == name)
                        dataContract = new NonPositiveIntegerDataContract();
                    else if (DictionaryGlobals.nonNegativeIntegerLocalName.Value == name)
                        dataContract = new NonNegativeIntegerDataContract();
                    else if (DictionaryGlobals.UnsignedLongLocalName.Value == name)
                        dataContract = new UnsignedLongDataContract();
                    else if (DictionaryGlobals.FloatLocalName.Value == name)
                        dataContract = new FloatDataContract();
                    else if (DictionaryGlobals.DoubleLocalName.Value == name)
                        dataContract = new DoubleDataContract();
                    else if (DictionaryGlobals.DecimalLocalName.Value == name)
                        dataContract = new DecimalDataContract();
                    else if (DictionaryGlobals.DateTimeLocalName.Value == name)
                        dataContract = new DateTimeDataContract();
                    else if (DictionaryGlobals.StringLocalName.Value == name)
                        dataContract = new StringDataContract();
                    else if (DictionaryGlobals.timeLocalName.Value == name)
                        dataContract = new TimeDataContract();
                    else if (DictionaryGlobals.dateLocalName.Value == name)
                        dataContract = new DateDataContract();
                    else if (DictionaryGlobals.hexBinaryLocalName.Value == name)
                        dataContract = new HexBinaryDataContract();
                    else if (DictionaryGlobals.gYearMonthLocalName.Value == name)
                        dataContract = new GYearMonthDataContract();
                    else if (DictionaryGlobals.gYearLocalName.Value == name)
                        dataContract = new GYearDataContract();
                    else if (DictionaryGlobals.gMonthDayLocalName.Value == name)
                        dataContract = new GMonthDayDataContract();
                    else if (DictionaryGlobals.gDayLocalName.Value == name)
                        dataContract = new GDayDataContract();
                    else if (DictionaryGlobals.gMonthLocalName.Value == name)
                        dataContract = new GMonthDataContract();
                    else if (DictionaryGlobals.normalizedStringLocalName.Value == name)
                        dataContract = new NormalizedStringDataContract();
                    else if (DictionaryGlobals.tokenLocalName.Value == name)
                        dataContract = new TokenDataContract();
                    else if (DictionaryGlobals.languageLocalName.Value == name)
                        dataContract = new LanguageDataContract();
                    else if (DictionaryGlobals.NameLocalName.Value == name)
                        dataContract = new NameDataContract();
                    else if (DictionaryGlobals.NCNameLocalName.Value == name)
                        dataContract = new NCNameDataContract();
                    else if (DictionaryGlobals.XSDIDLocalName.Value == name)
                        dataContract = new IDDataContract();
                    else if (DictionaryGlobals.IDREFLocalName.Value == name)
                        dataContract = new IDREFDataContract();
                    else if (DictionaryGlobals.IDREFSLocalName.Value == name)
                        dataContract = new IDREFSDataContract();
                    else if (DictionaryGlobals.ENTITYLocalName.Value == name)
                        dataContract = new ENTITYDataContract();
                    else if (DictionaryGlobals.ENTITIESLocalName.Value == name)
                        dataContract = new ENTITIESDataContract();
                    else if (DictionaryGlobals.NMTOKENLocalName.Value == name)
                        dataContract = new NMTOKENDataContract();
                    else if (DictionaryGlobals.NMTOKENSLocalName.Value == name)
                        dataContract = new NMTOKENDataContract();
                    else if (DictionaryGlobals.ByteArrayLocalName.Value == name)
                        dataContract = new ByteArrayDataContract();
                    else if (DictionaryGlobals.ObjectLocalName.Value == name)
                        dataContract = new ObjectDataContract();
                    else if (DictionaryGlobals.TimeSpanLocalName.Value == name)
                        dataContract = new XsDurationDataContract();
                    else if (DictionaryGlobals.UriLocalName.Value == name)
                        dataContract = new UriDataContract();
                    else if (DictionaryGlobals.QNameLocalName.Value == name)
                        dataContract = new QNameDataContract();
                }
                else if (ns == DictionaryGlobals.SerializationNamespace.Value)
                {
                    if (DictionaryGlobals.TimeSpanLocalName.Value == name)
                        dataContract = new TimeSpanDataContract();
                    else if (DictionaryGlobals.GuidLocalName.Value == name)
                        dataContract = new GuidDataContract();
                    else if (DictionaryGlobals.CharLocalName.Value == name)
                        dataContract = new CharDataContract();
                    else if ("ArrayOfanyType" == name)
                        dataContract = new CollectionDataContract(typeof(Array));
                }
                else if (ns == DictionaryGlobals.AsmxTypesNamespace.Value)
                {
                    if (DictionaryGlobals.CharLocalName.Value == name)
                        dataContract = new AsmxCharDataContract();
                    else if (DictionaryGlobals.GuidLocalName.Value == name)
                        dataContract = new AsmxGuidDataContract();
                }
                else if (ns == Globals.DataContractXmlNamespace)
                {
                    if (name == "XmlElement")
                        dataContract = new XmlDataContract(typeof(XmlElement));
                    else if (name == "ArrayOfXmlNode")
                        dataContract = new XmlDataContract(typeof(XmlNode[]));
                }
                return dataContract != null;
            }

            internal static string GetNamespace(string key)
            {
                lock (namespacesLock)
                {
                    if (namespaces == null)
                        namespaces = new Dictionary<string, string>();
                    string value;
                    if (namespaces.TryGetValue(key, out value))
                        return value;
                    try
                    {
                        namespaces.Add(key, key);
                    }
                    catch (Exception ex)
                    {
                        if (Fx.IsFatal(ex))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(ex.Message, ex);
                    }
                    return key;
                }
            }

            internal static XmlDictionaryString GetClrTypeString(string key)
            {
                lock (clrTypeStringsLock)
                {
                    if (clrTypeStrings == null)
                    {
                        clrTypeStringsDictionary = new XmlDictionary();
                        clrTypeStrings = new Dictionary<string, XmlDictionaryString>();
                        try
                        {
                            clrTypeStrings.Add(Globals.TypeOfInt.Assembly.FullName, clrTypeStringsDictionary.Add(Globals.MscorlibAssemblyName));
                        }
                        catch (Exception ex)
                        {
                            if (Fx.IsFatal(ex))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(ex.Message, ex);
                        }
                    }
                    XmlDictionaryString value;
                    if (clrTypeStrings.TryGetValue(key, out value))
                        return value;
                    value = clrTypeStringsDictionary.Add(key);
                    try
                    {
                        clrTypeStrings.Add(key, value);
                    }
                    catch (Exception ex)
                    {
                        if (Fx.IsFatal(ex))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(ex.Message, ex);
                    }
                    return value;
                }
            }

            internal static void ThrowInvalidDataContractException(string message, Type type)
            {
                if (type != null)
                {
                    lock (cacheLock)
                    {
                        typeHandleRef.Value = GetDataContractAdapterTypeHandle(type.TypeHandle);
                        try
                        {
                            typeToIDCache.Remove(typeHandleRef);
                        }
                        catch (Exception ex)
                        {
                            if (Fx.IsFatal(ex))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(ex.Message, ex);
                        }
                    }
                }

                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(message));
            }

            internal DataContractCriticalHelper()
            {
            }

            internal DataContractCriticalHelper(Type type)
            {
                underlyingType = type;
                SetTypeForInitialization(type);
                isValueType = type.IsValueType;
            }

            internal Type UnderlyingType
            {
                get { return underlyingType; }
            }

            internal Type OriginalUnderlyingType
            {
                get
                {
                    if (this.originalUnderlyingType == null)
                    {
                        this.originalUnderlyingType = GetDataContractOriginalType(this.underlyingType);
                    }
                    return this.originalUnderlyingType;
                }
            }

            internal virtual bool IsBuiltInDataContract
            {
                get
                {
                    return false;
                }
            }

            internal Type TypeForInitialization
            {
                get { return this.typeForInitialization; }
            }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical typeForInitialization property.",
                Safe = "Validates input data, sets field correctly.")]
            [SecuritySafeCritical]
            void SetTypeForInitialization(Type classType)
            {
                if (classType.IsSerializable || classType.IsDefined(Globals.TypeOfDataContractAttribute, false))
                {
                    this.typeForInitialization = classType;
                }
            }

            internal bool IsReference
            {
                get { return isReference; }
                set
                {
                    isReference = value;
                }
            }

            internal bool IsValueType
            {
                get { return isValueType; }
                set { isValueType = value; }
            }

            internal XmlQualifiedName StableName
            {
                get { return stableName; }
                set { stableName = value; }
            }

            internal GenericInfo GenericInfo
            {
                get { return genericInfo; }
                set { genericInfo = value; }
            }

            internal virtual DataContractDictionary KnownDataContracts
            {
                get { return null; }
                set { /* do nothing */ }
            }

            internal virtual bool IsISerializable
            {
                get { return false; }
                set { ThrowInvalidDataContractException(SR.GetString(SR.RequiresClassDataContractToSetIsISerializable)); }
            }

            internal XmlDictionaryString Name
            {
                get { return name; }
                set { name = value; }
            }

            public XmlDictionaryString Namespace
            {
                get { return ns; }
                set { ns = value; }
            }

            internal virtual bool HasRoot
            {
                get { return true; }
                set { }
            }

            internal virtual XmlDictionaryString TopLevelElementName
            {
                get { return name; }
                set { name = value; }
            }

            internal virtual XmlDictionaryString TopLevelElementNamespace
            {
                get { return ns; }
                set { ns = value; }
            }

            internal virtual bool CanContainReferences
            {
                get { return true; }
            }

            internal virtual bool IsPrimitive
            {
                get { return false; }
            }

            internal MethodInfo ParseMethod
            {
                get 
                {
                    if (!parseMethodSet)
                    {
                        MethodInfo method = UnderlyingType.GetMethod(Globals.ParseMethodName, BindingFlags.Public | BindingFlags.Static, null, new Type[] { Globals.TypeOfString }, null);

                        if (method != null && method.ReturnType == UnderlyingType)
                        {
                            parseMethod = method;
                        }

                        parseMethodSet = true;
                    }
                    return parseMethod; }
            }

            internal virtual void WriteRootElement(XmlWriterDelegator writer, XmlDictionaryString name, XmlDictionaryString ns)
            {
                if (object.ReferenceEquals(ns, DictionaryGlobals.SerializationNamespace) && !IsPrimitive)
                    writer.WriteStartElement(Globals.SerPrefix, name, ns);
                else
                    writer.WriteStartElement(name, ns);
            }

            internal void SetDataContractName(XmlQualifiedName stableName)
            {
                XmlDictionary dictionary = new XmlDictionary(2);
                this.Name = dictionary.Add(stableName.Name);
                this.Namespace = dictionary.Add(stableName.Namespace);
                this.StableName = stableName;
            }

            internal void SetDataContractName(XmlDictionaryString name, XmlDictionaryString ns)
            {
                this.Name = name;
                this.Namespace = ns;
                this.StableName = CreateQualifiedName(name.Value, ns.Value);
            }

            internal void ThrowInvalidDataContractException(string message)
            {
                ThrowInvalidDataContractException(message, UnderlyingType);
            }
        }

        static internal bool IsTypeSerializable(Type type)
        {
            return IsTypeSerializable(type, new Dictionary<Type, object>());
        }

        static bool IsTypeSerializable(Type type, Dictionary<Type, object> previousCollectionTypes)
        {
            Type itemType;
            if (type.IsSerializable ||
                type.IsDefined(Globals.TypeOfDataContractAttribute, false) ||
                type.IsInterface ||
                type.IsPointer ||
                Globals.TypeOfIXmlSerializable.IsAssignableFrom(type))
            {
                return true;
            }
            if (CollectionDataContract.IsCollection(type, out itemType))
            {
                ValidatePreviousCollectionTypes(type, itemType, previousCollectionTypes);
                if (IsTypeSerializable(itemType, previousCollectionTypes))
                {
                    return true;
                }
            }
            return (DataContract.GetBuiltInDataContract(type) != null || ClassDataContract.IsNonAttributedTypeValidForSerialization(type));
        }

        [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "previousCollectionTypes", Justification = "No need to support type equivalence here.")]
        static void ValidatePreviousCollectionTypes(Type collectionType, Type itemType, Dictionary<Type, object> previousCollectionTypes)
        {
            previousCollectionTypes.Add(collectionType, collectionType);
            while (itemType.IsArray)
            {
                itemType = itemType.GetElementType();
            }

            // Do a breadth first traversal of the generic type tree to 
            // produce the closure of all generic argument types and
            // check that none of these is in the previousCollectionTypes            
            
            List<Type> itemTypeClosure = new List<Type>();
            Queue<Type> itemTypeQueue = new Queue<Type>();

            itemTypeQueue.Enqueue(itemType);
            itemTypeClosure.Add(itemType);
                       
            while (itemTypeQueue.Count > 0)
            {
                itemType = itemTypeQueue.Dequeue();
                if (previousCollectionTypes.ContainsKey(itemType))
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.RecursiveCollectionType, DataContract.GetClrTypeFullName(itemType))));
                }
                if (itemType.IsGenericType)
                {
                    foreach (Type argType in itemType.GetGenericArguments())
                    {
                        if (!itemTypeClosure.Contains(argType))
                        {
                            itemTypeQueue.Enqueue(argType);
                            itemTypeClosure.Add(argType);
                        }
                    }
                }
            }
        }

        internal static Type UnwrapRedundantNullableType(Type type)
        {
            Type nullableType = type;
            while (type.IsGenericType && type.GetGenericTypeDefinition() == Globals.TypeOfNullable)
            {
                nullableType = type;
                type = type.GetGenericArguments()[0];
            }
            return nullableType;
        }

        internal static Type UnwrapNullableType(Type type)
        {
            while (type.IsGenericType && type.GetGenericTypeDefinition() == Globals.TypeOfNullable)
                type = type.GetGenericArguments()[0];
            return type;
        }

        static bool IsAlpha(char ch)
        {
            return (ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z');
        }

        static bool IsDigit(char ch)
        {
            return (ch >= '0' && ch <= '9');
        }

        static bool IsAsciiLocalName(string localName)
        {
            if (localName.Length == 0)
                return false;
            if (!IsAlpha(localName[0]))
                return false;
            for (int i = 1; i < localName.Length; i++)
            {
                char ch = localName[i];
                if (!IsAlpha(ch) && !IsDigit(ch))
                    return false;
            }
            return true;
        }

        static internal string EncodeLocalName(string localName)
        {
            if (IsAsciiLocalName(localName))
                return localName;

            if (IsValidNCName(localName))
                return localName;

            return XmlConvert.EncodeLocalName(localName);
        }

        internal static bool IsValidNCName(string name)
        {
            try
            {
                XmlConvert.VerifyNCName(name);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }

        internal static XmlQualifiedName GetStableName(Type type)
        {
            bool hasDataContract;
            return GetStableName(type, out hasDataContract);
        }

        internal static XmlQualifiedName GetStableName(Type type, out bool hasDataContract)
        {
            return GetStableName(type, new Dictionary<Type, object>(), out hasDataContract);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Callers may need to depend on hasDataContract for a security decision."
            + " hasDataContract must be calculated correctly."
            + " GetStableName is factored into sub-methods so as to isolate the DataContractAttribute calculation and reduce SecurityCritical surface area.",
            Safe = "Does not let caller influence hasDataContract calculation; no harm in leaking value.")]
        static XmlQualifiedName GetStableName(Type type, Dictionary<Type, object> previousCollectionTypes, out bool hasDataContract)
        {
            type = UnwrapRedundantNullableType(type);
            XmlQualifiedName stableName;
            if (TryGetBuiltInXmlAndArrayTypeStableName(type, previousCollectionTypes, out stableName))
            {
                hasDataContract = false;
            }
            else
            {
                DataContractAttribute dataContractAttribute;
                if (TryGetDCAttribute(type, out dataContractAttribute))
                {
                    stableName = GetDCTypeStableName(type, dataContractAttribute);
                    hasDataContract = true;
                }
                else
                {
                    stableName = GetNonDCTypeStableName(type, previousCollectionTypes);
                    hasDataContract = false;
                }
            }

            return stableName;
        }

        static XmlQualifiedName GetDCTypeStableName(Type type, DataContractAttribute dataContractAttribute)
        {
            string name = null, ns = null;
            if (dataContractAttribute.IsNameSetExplicit)
            {
                name = dataContractAttribute.Name;
                if (name == null || name.Length == 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidDataContractName, DataContract.GetClrTypeFullName(type))));
                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                    name = ExpandGenericParameters(name, type);
                name = DataContract.EncodeLocalName(name);
            }
            else
                name = GetDefaultStableLocalName(type);

            if (dataContractAttribute.IsNamespaceSetExplicit)
            {
                ns = dataContractAttribute.Namespace;
                if (ns == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidDataContractNamespace, DataContract.GetClrTypeFullName(type))));
                CheckExplicitDataContractNamespaceUri(ns, type);
            }
            else
                ns = GetDefaultDataContractNamespace(type);

            return CreateQualifiedName(name, ns);
        }

        static XmlQualifiedName GetNonDCTypeStableName(Type type, Dictionary<Type, object> previousCollectionTypes)
        {
            string name = null, ns = null;

            Type itemType;
            CollectionDataContractAttribute collectionContractAttribute;
            if (CollectionDataContract.IsCollection(type, out itemType))
            {
                ValidatePreviousCollectionTypes(type, itemType, previousCollectionTypes);
                return GetCollectionStableName(type, itemType, previousCollectionTypes, out collectionContractAttribute);
            }
            name = GetDefaultStableLocalName(type);

            // ensures that ContractNamespaceAttribute is honored when used with non-attributed types
            if (ClassDataContract.IsNonAttributedTypeValidForSerialization(type))
            {
                ns = GetDefaultDataContractNamespace(type);
            }
            else
            {
                ns = GetDefaultStableNamespace(type);
            }
            return CreateQualifiedName(name, ns);
        }

        static bool TryGetBuiltInXmlAndArrayTypeStableName(Type type, Dictionary<Type, object> previousCollectionTypes, out XmlQualifiedName stableName)
        {
            stableName = null;

            DataContract builtInContract = GetBuiltInDataContract(type);
            if (builtInContract != null)
            {
                stableName = builtInContract.StableName;
            }
            else if (Globals.TypeOfIXmlSerializable.IsAssignableFrom(type))
            {
                bool hasRoot;
                XmlSchemaType xsdType;
                XmlQualifiedName xmlTypeStableName;
                SchemaExporter.GetXmlTypeInfo(type, out xmlTypeStableName, out xsdType, out hasRoot);
                stableName = xmlTypeStableName;
            }
            else if (type.IsArray)
            {
                Type itemType = type.GetElementType();
                ValidatePreviousCollectionTypes(type, itemType, previousCollectionTypes);
                CollectionDataContractAttribute collectionContractAttribute;
                stableName = GetCollectionStableName(type, itemType, previousCollectionTypes, out collectionContractAttribute);
            }
            return stableName != null;
        }

        [Fx.Tag.SecurityNote(Critical = "Marked SecurityCritical because callers may need to base security decisions on the presence (or absence) of the DC attribute.",
            Safe = "Does not let caller influence calculation and the result is not a protected value.")]
        [SecuritySafeCritical]
        internal static bool TryGetDCAttribute(Type type, out DataContractAttribute dataContractAttribute)
        {
            dataContractAttribute = null;

            object[] dataContractAttributes = type.GetCustomAttributes(Globals.TypeOfDataContractAttribute, false);
            if (dataContractAttributes != null && dataContractAttributes.Length > 0)
            {
#if DEBUG
                if (dataContractAttributes.Length > 1)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.TooManyDataContracts, DataContract.GetClrTypeFullName(type))));
#endif
                dataContractAttribute = (DataContractAttribute)dataContractAttributes[0];
            }

            return dataContractAttribute != null;
        }

        internal static XmlQualifiedName GetCollectionStableName(Type type, Type itemType, out CollectionDataContractAttribute collectionContractAttribute)
        {
            return GetCollectionStableName(type, itemType, new Dictionary<Type, object>(), out collectionContractAttribute);
        }

        static XmlQualifiedName GetCollectionStableName(Type type, Type itemType, Dictionary<Type, object> previousCollectionTypes, out CollectionDataContractAttribute collectionContractAttribute)
        {
            string name, ns;
            object[] collectionContractAttributes = type.GetCustomAttributes(Globals.TypeOfCollectionDataContractAttribute, false);
            if (collectionContractAttributes != null && collectionContractAttributes.Length > 0)
            {
#if DEBUG
                if (collectionContractAttributes.Length > 1)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.TooManyCollectionContracts, DataContract.GetClrTypeFullName(type))));
#endif
                collectionContractAttribute = (CollectionDataContractAttribute)collectionContractAttributes[0];
                if (collectionContractAttribute.IsNameSetExplicit)
                {
                    name = collectionContractAttribute.Name;
                    if (name == null || name.Length == 0)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidCollectionContractName, DataContract.GetClrTypeFullName(type))));
                    if (type.IsGenericType && !type.IsGenericTypeDefinition)
                        name = ExpandGenericParameters(name, type);
                    name = DataContract.EncodeLocalName(name);
                }
                else
                    name = GetDefaultStableLocalName(type);

                if (collectionContractAttribute.IsNamespaceSetExplicit)
                {
                    ns = collectionContractAttribute.Namespace;
                    if (ns == null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidCollectionContractNamespace, DataContract.GetClrTypeFullName(type))));
                    CheckExplicitDataContractNamespaceUri(ns, type);
                }
                else
                    ns = GetDefaultDataContractNamespace(type);
            }
            else
            {
                collectionContractAttribute = null;
                string arrayOfPrefix = Globals.ArrayPrefix + GetArrayPrefix(ref itemType);
                bool hasDataContract;
                XmlQualifiedName elementStableName = GetStableName(itemType, previousCollectionTypes, out hasDataContract);
                name = arrayOfPrefix + elementStableName.Name;
                ns = GetCollectionNamespace(elementStableName.Namespace);
            }
            return CreateQualifiedName(name, ns);
        }

        private static string GetArrayPrefix(ref Type itemType)
        {
            string arrayOfPrefix = string.Empty;
            while (itemType.IsArray)
            {
                if (DataContract.GetBuiltInDataContract(itemType) != null)
                    break;
                arrayOfPrefix += Globals.ArrayPrefix;
                itemType = itemType.GetElementType();
            }
            return arrayOfPrefix;
        }

        internal XmlQualifiedName GetArrayTypeName(bool isNullable)
        {
            XmlQualifiedName itemName;
            if (this.IsValueType && isNullable)
            {
                GenericInfo genericInfo = new GenericInfo(DataContract.GetStableName(Globals.TypeOfNullable), Globals.TypeOfNullable.FullName);
                genericInfo.Add(new GenericInfo(this.StableName, null));
                genericInfo.AddToLevel(0, 1);
                itemName = genericInfo.GetExpandedStableName();
            }
            else
                itemName = this.StableName;
            string ns = GetCollectionNamespace(itemName.Namespace);
            string name = Globals.ArrayPrefix + itemName.Name;
            return new XmlQualifiedName(name, ns);
        }

        internal static string GetCollectionNamespace(string elementNs)
        {
            return IsBuiltInNamespace(elementNs) ? Globals.CollectionsNamespace : elementNs;
        }

        internal static XmlQualifiedName GetDefaultStableName(Type type)
        {
            return CreateQualifiedName(GetDefaultStableLocalName(type), GetDefaultStableNamespace(type));
        }

        static string GetDefaultStableLocalName(Type type)
        {
            if (type.IsGenericParameter)
                return "{" + type.GenericParameterPosition + "}";
            string typeName;
            string arrayPrefix = null;
            if (type.IsArray)
                arrayPrefix = GetArrayPrefix(ref type);
            if (type.DeclaringType == null)
                typeName = type.Name;
            else
            {
                int nsLen = (type.Namespace == null) ? 0 : type.Namespace.Length;
                if (nsLen > 0)
                    nsLen++; //include the . following namespace
                typeName = DataContract.GetClrTypeFullName(type).Substring(nsLen).Replace('+', '.');
            }
            if (arrayPrefix != null)
                typeName = arrayPrefix + typeName;
            if (type.IsGenericType)
            {
                StringBuilder localName = new StringBuilder();
                StringBuilder namespaces = new StringBuilder();
                bool parametersFromBuiltInNamespaces = true;
                int iParam = typeName.IndexOf('[');
                if (iParam >= 0)
                    typeName = typeName.Substring(0, iParam);
                IList<int> nestedParamCounts = GetDataContractNameForGenericName(typeName, localName);
                bool isTypeOpenGeneric = type.IsGenericTypeDefinition;
                Type[] genParams = type.GetGenericArguments();
                for (int i = 0; i < genParams.Length; i++)
                {
                    Type genParam = genParams[i];
                    if (isTypeOpenGeneric)
                        localName.Append("{").Append(i).Append("}");
                    else
                    {
                        XmlQualifiedName qname = DataContract.GetStableName(genParam);
                        localName.Append(qname.Name);
                        namespaces.Append(" ").Append(qname.Namespace);
                        if (parametersFromBuiltInNamespaces)
                            parametersFromBuiltInNamespaces = IsBuiltInNamespace(qname.Namespace);
                    }
                }
                if (isTypeOpenGeneric)
                    localName.Append("{#}");
                else if (nestedParamCounts.Count > 1 || !parametersFromBuiltInNamespaces)
                {
                    foreach (int count in nestedParamCounts)
                        namespaces.Insert(0, count).Insert(0, " ");
                    localName.Append(GetNamespacesDigest(namespaces.ToString()));
                }
                typeName = localName.ToString();
            }
            return DataContract.EncodeLocalName(typeName);
        }

        static string GetDefaultDataContractNamespace(Type type)
        {
            string clrNs = type.Namespace;
            if (clrNs == null)
                clrNs = String.Empty;
            string ns = GetGlobalDataContractNamespace(clrNs, type.Module);
            if (ns == null)
                ns = GetGlobalDataContractNamespace(clrNs, type.Assembly);

            if (ns == null)
                ns = GetDefaultStableNamespace(type);
            else
                CheckExplicitDataContractNamespaceUri(ns, type);
            return ns;
        }

        internal static IList<int> GetDataContractNameForGenericName(string typeName, StringBuilder localName)
        {
            List<int> nestedParamCounts = new List<int>();
            for (int startIndex = 0, endIndex;;)
            {
                endIndex = typeName.IndexOf('`', startIndex);
                if (endIndex < 0)
                {
                    if (localName != null)
                        localName.Append(typeName.Substring(startIndex));
                    nestedParamCounts.Add(0);
                    break;
                }
                if (localName != null)
                    localName.Append(typeName.Substring(startIndex, endIndex - startIndex));
                while ((startIndex = typeName.IndexOf('.', startIndex + 1, endIndex - startIndex - 1)) >= 0)
                    nestedParamCounts.Add(0);
                startIndex = typeName.IndexOf('.', endIndex);
                if (startIndex < 0)
                {
                    nestedParamCounts.Add(Int32.Parse(typeName.Substring(endIndex + 1), CultureInfo.InvariantCulture));
                    break;
                }
                else
                    nestedParamCounts.Add(Int32.Parse(typeName.Substring(endIndex + 1, startIndex - endIndex - 1), CultureInfo.InvariantCulture));
            }
            if (localName != null)
                localName.Append("Of");
            return nestedParamCounts;
        }

        internal static bool IsBuiltInNamespace(string ns)
        {
            return (ns == Globals.SchemaNamespace || ns == Globals.SerializationNamespace);
        }

        internal static string GetDefaultStableNamespace(Type type)
        {
            if (type.IsGenericParameter)
                return "{ns}";
            return GetDefaultStableNamespace(type.Namespace);
        }

        internal static XmlQualifiedName CreateQualifiedName(string localName, string ns)
        {
            return new XmlQualifiedName(localName, GetNamespace(ns));
        }

        internal static string GetDefaultStableNamespace(string clrNs)
        {
            if (clrNs == null) clrNs = String.Empty;
            return new Uri(Globals.DataContractXsdBaseNamespaceUri, clrNs).AbsoluteUri;
        }

        static void CheckExplicitDataContractNamespaceUri(string dataContractNs, Type type)
        {
            if (dataContractNs.Length > 0)
            {
                string trimmedNs = dataContractNs.Trim();
                // Code similar to XmlConvert.ToUri (string.Empty is a valid uri but not "   ")
                if (trimmedNs.Length == 0 || trimmedNs.IndexOf("##", StringComparison.Ordinal) != -1)
                    ThrowInvalidDataContractException(SR.GetString(SR.DataContractNamespaceIsNotValid, dataContractNs), type);
                dataContractNs = trimmedNs;
            }
            Uri uri;
            if (Uri.TryCreate(dataContractNs, UriKind.RelativeOrAbsolute, out uri))
            {
                if (uri.ToString() == Globals.SerializationNamespace)
                    ThrowInvalidDataContractException(SR.GetString(SR.DataContractNamespaceReserved, Globals.SerializationNamespace), type);
            }
            else
                ThrowInvalidDataContractException(SR.GetString(SR.DataContractNamespaceIsNotValid, dataContractNs), type);
        }

        internal static string GetClrTypeFullName(Type type)
        {
            return !type.IsGenericTypeDefinition && type.ContainsGenericParameters ? String.Format(CultureInfo.InvariantCulture, "{0}.{1}", type.Namespace, type.Name) : type.FullName;
        }

        internal static string GetClrAssemblyName(Type type, out bool hasTypeForwardedFrom)
        {
            hasTypeForwardedFrom = false;
            object[] typeAttributes = type.GetCustomAttributes(typeof(TypeForwardedFromAttribute), false);
            if (typeAttributes != null && typeAttributes.Length > 0)
            {
                TypeForwardedFromAttribute typeForwardedFromAttribute = (TypeForwardedFromAttribute)typeAttributes[0];
                hasTypeForwardedFrom = true;
                return typeForwardedFromAttribute.AssemblyFullName;
            }
            else
            {
                return type.Assembly.FullName;
            }
        }

        internal static string GetClrTypeFullNameUsingTypeForwardedFromAttribute(Type type)
        {
            if (type.IsArray)
            {
                return GetClrTypeFullNameForArray(type);
            }
            else
            {
                return GetClrTypeFullNameForNonArrayTypes(type);
            }
        }

        static string GetClrTypeFullNameForArray(Type type)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
                GetClrTypeFullNameUsingTypeForwardedFromAttribute(type.GetElementType()), Globals.OpenBracket, Globals.CloseBracket);
        }

        static string GetClrTypeFullNameForNonArrayTypes(Type type)
        {
            if (!type.IsGenericType)
            {
                return DataContract.GetClrTypeFullName(type);
            }

            Type[] genericArguments = type.GetGenericArguments();
            StringBuilder builder = new StringBuilder(type.GetGenericTypeDefinition().FullName).Append(Globals.OpenBracket);

            foreach (Type genericArgument in genericArguments)
            {
                bool hasTypeForwardedFrom;
                builder.Append(Globals.OpenBracket).Append(GetClrTypeFullNameUsingTypeForwardedFromAttribute(genericArgument)).Append(Globals.Comma);
                builder.Append(Globals.Space).Append(GetClrAssemblyName(genericArgument, out hasTypeForwardedFrom));
                builder.Append(Globals.CloseBracket).Append(Globals.Comma);
            }

            //remove the last comma and close typename for generic with a close bracket
            return builder.Remove(builder.Length - 1, 1).Append(Globals.CloseBracket).ToString();
        }

        internal static void GetClrNameAndNamespace(string fullTypeName, out string localName, out string ns)
        {
            int nsEnd = fullTypeName.LastIndexOf('.');
            if (nsEnd < 0)
            {
                ns = String.Empty;
                localName = fullTypeName.Replace('+', '.');
            }
            else
            {
                ns = fullTypeName.Substring(0, nsEnd);
                localName = fullTypeName.Substring(nsEnd + 1).Replace('+', '.');
            }
            int iParam = localName.IndexOf('[');
            if (iParam >= 0)
                localName = localName.Substring(0, iParam);
        }

        internal static void GetDefaultStableName(string fullTypeName, out string localName, out string ns)
        {
            CodeTypeReference typeReference = new CodeTypeReference(fullTypeName);
            GetDefaultStableName(typeReference, out localName, out ns);
        }

        static void GetDefaultStableName(CodeTypeReference typeReference, out string localName, out string ns)
        {
            string fullTypeName = typeReference.BaseType;
            DataContract dataContract = GetBuiltInDataContract(fullTypeName);
            if (dataContract != null)
            {
                localName = dataContract.StableName.Name;
                ns = dataContract.StableName.Namespace;
                return;
            }
            GetClrNameAndNamespace(fullTypeName, out localName, out ns);
            if (typeReference.TypeArguments.Count > 0)
            {
                StringBuilder localNameBuilder = new StringBuilder();
                StringBuilder argNamespacesBuilder = new StringBuilder();
                bool parametersFromBuiltInNamespaces = true;
                IList<int> nestedParamCounts = GetDataContractNameForGenericName(localName, localNameBuilder);
                foreach (CodeTypeReference typeArg in typeReference.TypeArguments)
                {
                    string typeArgName, typeArgNs;
                    GetDefaultStableName(typeArg, out typeArgName, out typeArgNs);
                    localNameBuilder.Append(typeArgName);
                    argNamespacesBuilder.Append(" ").Append(typeArgNs);
                    if (parametersFromBuiltInNamespaces)
                        parametersFromBuiltInNamespaces = IsBuiltInNamespace(typeArgNs);
                }
                if (nestedParamCounts.Count > 1 || !parametersFromBuiltInNamespaces)
                {
                    foreach (int count in nestedParamCounts)
                        argNamespacesBuilder.Insert(0, count).Insert(0, " ");
                    localNameBuilder.Append(GetNamespacesDigest(argNamespacesBuilder.ToString()));
                }
                localName = localNameBuilder.ToString();
            }
            localName = DataContract.EncodeLocalName(localName);
            ns = GetDefaultStableNamespace(ns);
        }

        internal static string GetDataContractNamespaceFromUri(string uriString)
        {
            return uriString.StartsWith(Globals.DataContractXsdBaseNamespace, StringComparison.Ordinal) ? uriString.Substring(Globals.DataContractXsdBaseNamespace.Length) : uriString;
        }

        static string GetGlobalDataContractNamespace(string clrNs, ICustomAttributeProvider customAttribuetProvider)
        {
            object[] nsAttributes = customAttribuetProvider.GetCustomAttributes(typeof(ContractNamespaceAttribute), false);
            string dataContractNs = null;
            for (int i = 0; i < nsAttributes.Length; i++)
            {
                ContractNamespaceAttribute nsAttribute = (ContractNamespaceAttribute)nsAttributes[i];
                string clrNsInAttribute = nsAttribute.ClrNamespace;
                if (clrNsInAttribute == null)
                    clrNsInAttribute = String.Empty;
                if (clrNsInAttribute == clrNs)
                {
                    if (nsAttribute.ContractNamespace == null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidGlobalDataContractNamespace, clrNs)));
                    if (dataContractNs != null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.DataContractNamespaceAlreadySet, dataContractNs, nsAttribute.ContractNamespace, clrNs)));
                    dataContractNs = nsAttribute.ContractNamespace;
                }
            }
            return dataContractNs;
        }

        private static string GetNamespacesDigest(string namespaces)
        {
            byte[] namespaceBytes = Encoding.UTF8.GetBytes(namespaces);
            byte[] digestBytes = HashHelper.ComputeHash(namespaceBytes);
            char[] digestChars = new char[24];
            const int digestLen = 6;
            int digestCharsLen = Convert.ToBase64CharArray(digestBytes, 0, digestLen, digestChars, 0);
            StringBuilder digest = new StringBuilder();
            for (int i = 0; i < digestCharsLen; i++)
            {
                char ch = digestChars[i];
                switch (ch)
                {
                    case '=':
                        break;
                    case '/':
                        digest.Append("_S");
                        break;
                    case '+':
                        digest.Append("_P");
                        break;
                    default:
                        digest.Append(ch);
                        break;
                }
            }
            return digest.ToString();
        }

        private static string ExpandGenericParameters(string format, Type type)
        {
            GenericNameProvider genericNameProviderForType = new GenericNameProvider(type);
            return ExpandGenericParameters(format, genericNameProviderForType);
        }

        internal static string ExpandGenericParameters(string format, IGenericNameProvider genericNameProvider)
        {
            string digest = null;
            StringBuilder typeName = new StringBuilder();
            IList<int> nestedParameterCounts = genericNameProvider.GetNestedParameterCounts();
            for (int i = 0; i < format.Length; i++)
            {
                char ch = format[i];
                if (ch == '{')
                {
                    i++;
                    int start = i;
                    for (; i < format.Length; i++)
                        if (format[i] == '}')
                            break;
                    if (i == format.Length)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.GenericNameBraceMismatch, format, genericNameProvider.GetGenericTypeName())));
                    if (format[start] == '#' && i == (start + 1))
                    {
                        if (nestedParameterCounts.Count > 1 || !genericNameProvider.ParametersFromBuiltInNamespaces)
                        {
                            if (digest == null)
                            {
                                StringBuilder namespaces = new StringBuilder(genericNameProvider.GetNamespaces());
                                foreach (int count in nestedParameterCounts)
                                    namespaces.Insert(0, count).Insert(0, " ");
                                digest = GetNamespacesDigest(namespaces.ToString());
                            }
                            typeName.Append(digest);
                        }
                    }
                    else
                    {
                        int paramIndex;
                        if (!Int32.TryParse(format.Substring(start, i - start), out paramIndex) || paramIndex < 0 || paramIndex >= genericNameProvider.GetParameterCount())
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.GenericParameterNotValid, format.Substring(start, i - start), genericNameProvider.GetGenericTypeName(), genericNameProvider.GetParameterCount() - 1)));
                        typeName.Append(genericNameProvider.GetParameterName(paramIndex));
                    }
                }
                else
                    typeName.Append(ch);
            }
            return typeName.ToString();
        }

        static internal bool IsTypeNullable(Type type)
        {
            return !type.IsValueType ||
                    (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == Globals.TypeOfNullable);
        }

        public static void ThrowTypeNotSerializable(Type type)
        {
            ThrowInvalidDataContractException(SR.GetString(SR.TypeNotSerializable, type), type);
        }

        [Fx.Tag.SecurityNote(Critical = "configSection value is fetched under an elevation; need to protected access to it.")]
        [SecurityCritical]
        static DataContractSerializerSection configSection;
        static DataContractSerializerSection ConfigSection
        {
            [Fx.Tag.SecurityNote(Critical = "Calls Security Critical method DataContractSerializerSection.UnsafeGetSection and stores result in"
                + " SecurityCritical field configSection.")]
            [SecurityCritical]
            get
            {
                if (configSection == null)
                    configSection = DataContractSerializerSection.UnsafeGetSection();
                return configSection;
            }
        }

        internal static DataContractDictionary ImportKnownTypeAttributes(Type type)
        {
            DataContractDictionary knownDataContracts = null;
            Dictionary<Type, Type> typesChecked = new Dictionary<Type, Type>();
            ImportKnownTypeAttributes(type, typesChecked, ref knownDataContracts);
            return knownDataContracts;
        }

        [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "typesChecked", Justification = "No need to support type equivalence here.")]
        static void ImportKnownTypeAttributes(Type type, Dictionary<Type, Type> typesChecked, ref DataContractDictionary knownDataContracts)
        {
            if (TD.ImportKnownTypesStartIsEnabled())
            {
                TD.ImportKnownTypesStart();
            }

            while (type != null && DataContract.IsTypeSerializable(type))
            {
                if (typesChecked.ContainsKey(type))
                    return;

                typesChecked.Add(type, type);
                object[] knownTypeAttributes = type.GetCustomAttributes(Globals.TypeOfKnownTypeAttribute, false);
                if (knownTypeAttributes != null)
                {
                    KnownTypeAttribute kt;
                    bool useMethod = false, useType = false;
                    for (int i = 0; i < knownTypeAttributes.Length; ++i)
                    {
                        kt = (KnownTypeAttribute)knownTypeAttributes[i];
                        if (kt.Type != null)
                        {
                            if (useMethod)
                            {
                                DataContract.ThrowInvalidDataContractException(SR.GetString(SR.KnownTypeAttributeOneScheme, DataContract.GetClrTypeFullName(type)), type);
                            }

                            CheckAndAdd(kt.Type, typesChecked, ref knownDataContracts);
                            useType = true;
                        }
                        else
                        {
                            if (useMethod || useType)
                            {
                                DataContract.ThrowInvalidDataContractException(SR.GetString(SR.KnownTypeAttributeOneScheme, DataContract.GetClrTypeFullName(type)), type);
                            }

                            string methodName = kt.MethodName;
                            if (methodName == null)
                            {
                                DataContract.ThrowInvalidDataContractException(SR.GetString(SR.KnownTypeAttributeNoData, DataContract.GetClrTypeFullName(type)), type);
                            }

                            if (methodName.Length == 0)
                                DataContract.ThrowInvalidDataContractException(SR.GetString(SR.KnownTypeAttributeEmptyString, DataContract.GetClrTypeFullName(type)), type);

                            MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, Globals.EmptyTypeArray, null);
                            if (method == null)
                                DataContract.ThrowInvalidDataContractException(SR.GetString(SR.KnownTypeAttributeUnknownMethod, methodName, DataContract.GetClrTypeFullName(type)), type);

                            if (!Globals.TypeOfTypeEnumerable.IsAssignableFrom(method.ReturnType))
                                DataContract.ThrowInvalidDataContractException(SR.GetString(SR.KnownTypeAttributeReturnType, DataContract.GetClrTypeFullName(type), methodName), type);

                            object types = method.Invoke(null, Globals.EmptyObjectArray);
                            if (types == null)
                            {
                                DataContract.ThrowInvalidDataContractException(SR.GetString(SR.KnownTypeAttributeMethodNull, DataContract.GetClrTypeFullName(type)), type);
                            }

                            foreach (Type ty in (IEnumerable<Type>)types)
                            {
                                if (ty == null)
                                    DataContract.ThrowInvalidDataContractException(SR.GetString(SR.KnownTypeAttributeValidMethodTypes, DataContract.GetClrTypeFullName(type)), type);

                                CheckAndAdd(ty, typesChecked, ref knownDataContracts);
                            }

                            useMethod = true;
                        }
                    }
                }

                LoadKnownTypesFromConfig(type, typesChecked, ref knownDataContracts);

                type = type.BaseType;
            }

            if (TD.ImportKnownTypesStopIsEnabled())
            {
                TD.ImportKnownTypesStop();
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical property ConfigSection.",
            Safe = "Completely processes ConfigSection and only makes available the processed result."
            + " The ConfigSection instance is not leaked.")]
        [SecuritySafeCritical]
        static void LoadKnownTypesFromConfig(Type type, Dictionary<Type, Type> typesChecked, ref DataContractDictionary knownDataContracts)
        {
            // Pull known types from config
            if (ConfigSection != null)
            {
                DeclaredTypeElementCollection elements = ConfigSection.DeclaredTypes;

                Type rootType = type;
                Type[] genArgs = null;

                CheckRootTypeInConfigIsGeneric(type, ref rootType, ref genArgs);

                DeclaredTypeElement elem = elements[rootType.AssemblyQualifiedName];
                if (elem != null)
                {
                    if (IsElemTypeNullOrNotEqualToRootType(elem.Type, rootType))
                    {
                        elem = null;
                    }
                }

                if (elem == null)
                {
                    for (int i = 0; i < elements.Count; ++i)
                    {
                        if (IsCollectionElementTypeEqualToRootType(elements[i].Type, rootType))
                        {
                            elem = elements[i];
                            break;
                        }
                    }
                }

                if (elem != null)
                {
                    for (int i = 0; i < elem.KnownTypes.Count; ++i)
                    {
                        Type knownType = elem.KnownTypes[i].GetType(elem.Type, genArgs);
                        if (knownType != null)
                        {
                            CheckAndAdd(knownType, typesChecked, ref knownDataContracts);
                        }
                    }
                }
            }
        }

        private static void CheckRootTypeInConfigIsGeneric(Type type, ref Type rootType, ref Type[] genArgs)
        {
            if (rootType.IsGenericType)
            {
                if (!rootType.ContainsGenericParameters)
                {
                    genArgs = rootType.GetGenericArguments();
                    rootType = rootType.GetGenericTypeDefinition();
                }
                else
                {
                    DataContract.ThrowInvalidDataContractException(SR.GetString(SR.TypeMustBeConcrete, type), type);
                }
            }
        }

        private static bool IsElemTypeNullOrNotEqualToRootType(string elemTypeName, Type rootType)
        {
            Type t = Type.GetType(elemTypeName, false);
            if (t == null || !rootType.Equals(t))
            {
                return true;
            }
            return false;
        }

        private static bool IsCollectionElementTypeEqualToRootType(string collectionElementTypeName, Type rootType)
        {
            if (collectionElementTypeName.StartsWith(DataContract.GetClrTypeFullName(rootType), StringComparison.Ordinal))
            {
                Type t = Type.GetType(collectionElementTypeName, false);
                if (t != null)
                {
                    if (t.IsGenericType && !IsOpenGenericType(t))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.KnownTypeConfigClosedGenericDeclared, collectionElementTypeName)));
                    }
                    else if (rootType.Equals(t))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [Fx.Tag.SecurityNote(Critical = "Fetches the critical dataContractAdapterType.",
            Safe = "The critical dataContractAdapterType is only used for local comparison and is not leaked beyond this method.")]
        [SecurityCritical, SecurityTreatAsSafe]
        internal static void CheckAndAdd(Type type, Dictionary<Type, Type> typesChecked, ref DataContractDictionary nameToDataContractTable)
        {
            type = DataContract.UnwrapNullableType(type);
            DataContract dataContract = DataContract.GetDataContract(type);
            DataContract alreadyExistingContract;
            if (nameToDataContractTable == null)
            {
                nameToDataContractTable = new DataContractDictionary();
            }
            else if (nameToDataContractTable.TryGetValue(dataContract.StableName, out alreadyExistingContract))
            {
                if (alreadyExistingContract.UnderlyingType != DataContractCriticalHelper.GetDataContractAdapterType(type))
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.DupContractInKnownTypes, type, alreadyExistingContract.UnderlyingType, dataContract.StableName.Namespace, dataContract.StableName.Name)));
                return;
            }
            nameToDataContractTable.Add(dataContract.StableName, dataContract);
            ImportKnownTypeAttributes(type, typesChecked, ref nameToDataContractTable);
        }

        static bool IsOpenGenericType(Type t)
        {
            Type[] args = t.GetGenericArguments();
            for (int i = 0; i < args.Length; ++i)
                if (!args[i].IsGenericParameter)
                    return false;

            return true;
        }

        public sealed override bool Equals(object other)
        {
            if ((object)this == other)
                return true;
            return Equals(other, new Dictionary<DataContractPairKey, object>());
        }

        internal virtual bool Equals(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            DataContract dataContract = other as DataContract;
            if (dataContract != null)
            {
                return (StableName.Name == dataContract.StableName.Name && StableName.Namespace == dataContract.StableName.Namespace && IsReference == dataContract.IsReference);
            }
            return false;
        }

        internal bool IsEqualOrChecked(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            if ((object)this == other)
                return true;

            if (checkedContracts != null)
            {
                DataContractPairKey contractPairKey = new DataContractPairKey(this, other);
                if (checkedContracts.ContainsKey(contractPairKey))
                    return true;
                checkedContracts.Add(contractPairKey, null);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal void ThrowInvalidDataContractException(string message)
        {
            ThrowInvalidDataContractException(message, UnderlyingType);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - checks type visibility to calculate if access to it requires MemberAccessPermission."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        static internal bool IsTypeVisible(Type t)
        {
            // Generic parameters are always considered visible.
            if (t.IsGenericParameter)
            {
                return true;
            }

            // The normal Type.IsVisible check requires all nested types to be IsNestedPublic.
            // This does not comply with our convention where they can also have InternalsVisibleTo
            // with our assembly.   The following method performs a recursive walk back the declaring
            // type hierarchy to perform this enhanced IsVisible check.
            if (!IsTypeAndDeclaringTypeVisible(t))
            {
                return false;
            }

            // All generic argument types must also be visible.
            // Nested types perform this test recursively for all their declaring types.
            foreach (Type genericType in t.GetGenericArguments())
            {
                if (!IsTypeVisible(genericType))
                {
                    return false;
                }
            }

            return true;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - checks type visibility to calculate if access to it requires MemberAccessPermission."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        static internal bool IsTypeAndDeclaringTypeVisible(Type t)
        {
            // Arrays, etc. must consider the underlying element type because the
            // non-element type does not reflect the same type nesting.  For example,
            // MyClass[] would not show as a nested type, even when MyClass is nested.
            if (t.HasElementType)
            {
                return IsTypeVisible(t.GetElementType());
            }

            // Nested types are not visible unless their declaring type is visible.
            // Additionally, they must be either IsNestedPublic or in an assembly with InternalsVisibleTo this current assembly.
            // Non-nested types must be public or have this same InternalsVisibleTo relation.
            return t.IsNested
                    ? (t.IsNestedPublic || IsTypeVisibleInSerializationModule(t)) && IsTypeVisible(t.DeclaringType)
                    : t.IsPublic || IsTypeVisibleInSerializationModule(t);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - checks constructor visibility to calculate if access to it requires MemberAccessPermission."
            + " note: does local check for visibility, assuming that the declaring Type visibility has been checked."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        static internal bool ConstructorRequiresMemberAccess(ConstructorInfo ctor)
        {
            return ctor != null && !ctor.IsPublic && !IsMemberVisibleInSerializationModule(ctor);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - checks method visibility to calculate if access to it requires MemberAccessPermission."
            + " note: does local check for visibility, assuming that the declaring Type visibility has been checked."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        static internal bool MethodRequiresMemberAccess(MethodInfo method)
        {
            return method != null && !method.IsPublic && !IsMemberVisibleInSerializationModule(method);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - checks field visibility to calculate if access to it requires MemberAccessPermission."
            + " note: does local check for visibility, assuming that the declaring Type visibility has been checked."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        static internal bool FieldRequiresMemberAccess(FieldInfo field)
        {
            return field != null && !field.IsPublic && !IsMemberVisibleInSerializationModule(field);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - checks type visibility to calculate if access to it requires MemberAccessPermission."
            + " note: does local check for visibility, assuming that the declaring Type visibility has been checked."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        static bool IsTypeVisibleInSerializationModule(Type type)
        {
            return (type.Module.Equals(typeof(CodeGenerator).Module) || IsAssemblyFriendOfSerialization(type.Assembly)) && !type.IsNestedPrivate;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - checks member visibility to calculate if access to it requires MemberAccessPermission."
            + " note: does local check for visibility, assuming that the declaring Type visibility has been checked."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        static bool IsMemberVisibleInSerializationModule(MemberInfo member)
        {
            if (!IsTypeVisibleInSerializationModule(member.DeclaringType))
                return false;

            if (member is MethodInfo)
            {
                MethodInfo method = (MethodInfo)member;
                return (method.IsAssembly || method.IsFamilyOrAssembly);
            }
            else if (member is FieldInfo)
            {
                FieldInfo field = (FieldInfo)member;
                return (field.IsAssembly || field.IsFamilyOrAssembly) && IsTypeVisible(field.FieldType);
            }
            else if (member is ConstructorInfo)
            {
                ConstructorInfo constructor = (ConstructorInfo)member;
                return (constructor.IsAssembly || constructor.IsFamilyOrAssembly);
            }

            return false;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - checks member visibility to calculate if access to it requires MemberAccessPermission."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        internal static bool IsAssemblyFriendOfSerialization(Assembly assembly)
        {
            InternalsVisibleToAttribute[] internalsVisibleAttributes = (InternalsVisibleToAttribute[])assembly.GetCustomAttributes(typeof(InternalsVisibleToAttribute), false);
            foreach (InternalsVisibleToAttribute internalsVisibleAttribute in internalsVisibleAttributes)
            {
                string internalsVisibleAttributeAssemblyName = internalsVisibleAttribute.AssemblyName;

                if (Regex.IsMatch(internalsVisibleAttributeAssemblyName, Globals.SimpleSRSInternalsVisiblePattern) ||
                    Regex.IsMatch(internalsVisibleAttributeAssemblyName, Globals.FullSRSInternalsVisiblePattern))
                {
                    return true;
                }
            }
            return false;
        }
    }

    interface IGenericNameProvider
    {
        int GetParameterCount();
        IList<int> GetNestedParameterCounts();
        string GetParameterName(int paramIndex);
        string GetNamespaces();
        string GetGenericTypeName();
        bool ParametersFromBuiltInNamespaces { get; }
    }

    class GenericNameProvider : IGenericNameProvider
    {
        string genericTypeName;
        object[] genericParams; //Type or DataContract
        IList<int> nestedParamCounts;
        internal GenericNameProvider(Type type)
            : this(DataContract.GetClrTypeFullName(type.GetGenericTypeDefinition()), type.GetGenericArguments())
        {
        }

        internal GenericNameProvider(string genericTypeName, object[] genericParams)
        {
            this.genericTypeName = genericTypeName;
            this.genericParams = new object[genericParams.Length];
            genericParams.CopyTo(this.genericParams, 0);

            string name, ns;
            DataContract.GetClrNameAndNamespace(genericTypeName, out name, out ns);
            this.nestedParamCounts = DataContract.GetDataContractNameForGenericName(name, null);
        }

        public int GetParameterCount()
        {
            return genericParams.Length;
        }

        public IList<int> GetNestedParameterCounts()
        {
            return nestedParamCounts;
        }

        public string GetParameterName(int paramIndex)
        {
            return GetStableName(paramIndex).Name;
        }

        public string GetNamespaces()
        {
            StringBuilder namespaces = new StringBuilder();
            for (int j = 0; j < GetParameterCount(); j++)
                namespaces.Append(" ").Append(GetStableName(j).Namespace);
            return namespaces.ToString();
        }

        public string GetGenericTypeName()
        {
            return genericTypeName;
        }

        public bool ParametersFromBuiltInNamespaces
        {
            get
            {
                bool parametersFromBuiltInNamespaces = true;
                for (int j = 0; j < GetParameterCount(); j++)
                {
                    if (parametersFromBuiltInNamespaces)
                        parametersFromBuiltInNamespaces = DataContract.IsBuiltInNamespace(GetStableName(j).Namespace);
                    else
                        break;
                }
                return parametersFromBuiltInNamespaces;
            }
        }

        XmlQualifiedName GetStableName(int i)
        {
            object o = genericParams[i];
            XmlQualifiedName qname = o as XmlQualifiedName;
            if (qname == null)
            {
                Type paramType = o as Type;
                if (paramType != null)
                    genericParams[i] = qname = DataContract.GetStableName(paramType);
                else
                    genericParams[i] = qname = ((DataContract)o).StableName;
            }
            return qname;
        }
    }

    class GenericInfo : IGenericNameProvider
    {
        string genericTypeName;
        XmlQualifiedName stableName;
        List<GenericInfo> paramGenericInfos;
        List<int> nestedParamCounts;

        internal GenericInfo(XmlQualifiedName stableName, string genericTypeName)
        {
            this.stableName = stableName;
            this.genericTypeName = genericTypeName;
            this.nestedParamCounts = new List<int>();
            this.nestedParamCounts.Add(0);
        }

        internal void Add(GenericInfo actualParamInfo)
        {
            if (paramGenericInfos == null)
                paramGenericInfos = new List<GenericInfo>();
            paramGenericInfos.Add(actualParamInfo);
        }

        internal void AddToLevel(int level, int count)
        {
            if (level >= nestedParamCounts.Count)
            {
                do
                {
                    nestedParamCounts.Add((level == nestedParamCounts.Count) ? count : 0);
                } while (level >= nestedParamCounts.Count);
            }
            else
                nestedParamCounts[level] = nestedParamCounts[level] + count;
        }

        internal XmlQualifiedName GetExpandedStableName()
        {
            if (paramGenericInfos == null)
                return stableName;
            return new XmlQualifiedName(DataContract.EncodeLocalName(DataContract.ExpandGenericParameters(XmlConvert.DecodeName(stableName.Name), this)), stableName.Namespace);
        }

        internal string GetStableNamespace()
        {
            return stableName.Namespace;
        }

        internal XmlQualifiedName StableName
        {
            get { return stableName; }
        }

        internal IList<GenericInfo> Parameters
        {
            get { return paramGenericInfos; }
        }

        public int GetParameterCount()
        {
            return paramGenericInfos.Count;
        }

        public IList<int> GetNestedParameterCounts()
        {
            return nestedParamCounts;
        }

        public string GetParameterName(int paramIndex)
        {
            return paramGenericInfos[paramIndex].GetExpandedStableName().Name;
        }

        public string GetNamespaces()
        {
            StringBuilder namespaces = new StringBuilder();
            for (int j = 0; j < paramGenericInfos.Count; j++)
                namespaces.Append(" ").Append(paramGenericInfos[j].GetStableNamespace());
            return namespaces.ToString();
        }

        public string GetGenericTypeName()
        {
            return genericTypeName;
        }

        public bool ParametersFromBuiltInNamespaces
        {
            get
            {
                bool parametersFromBuiltInNamespaces = true;
                for (int j = 0; j < paramGenericInfos.Count; j++)
                {
                    if (parametersFromBuiltInNamespaces)
                        parametersFromBuiltInNamespaces = DataContract.IsBuiltInNamespace(paramGenericInfos[j].GetStableNamespace());
                    else
                        break;
                }
                return parametersFromBuiltInNamespaces;
            }
        }

    }

    internal class DataContractPairKey
    {
        object object1;
        object object2;

        public DataContractPairKey(object object1, object object2)
        {
            this.object1 = object1;
            this.object2 = object2;
        }

        public override bool Equals(object other)
        {
            DataContractPairKey otherKey = other as DataContractPairKey;
            if (otherKey == null)
                return false;
            return ((otherKey.object1 == object1 && otherKey.object2 == object2) || (otherKey.object1 == object2 && otherKey.object2 == object1));
        }

        public override int GetHashCode()
        {
            return object1.GetHashCode() ^ object2.GetHashCode();
        }
    }

    class TypeHandleRefEqualityComparer : IEqualityComparer<TypeHandleRef>
    {
        public bool Equals(TypeHandleRef x, TypeHandleRef y)
        {
            return x.Value.Equals(y.Value);
        }

        public int GetHashCode(TypeHandleRef obj)
        {
            return obj.Value.GetHashCode();
        }
    }

    class TypeHandleRef
    {
        RuntimeTypeHandle value;

        public TypeHandleRef()
        {
        }

        public TypeHandleRef(RuntimeTypeHandle value)
        {
            this.value = value;
        }

        public RuntimeTypeHandle Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }

    class IntRef
    {
        int value;

        public IntRef(int value)
        {
            this.value = value;
        }

        public int Value
        {
            get
            {
                return this.value;
            }
        }
    }
}
