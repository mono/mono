//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using DataContractDictionary = System.Collections.Generic.Dictionary<System.Xml.XmlQualifiedName, DataContract>;
    using System.Collections.Generic;
    using System.Runtime.Serialization.Diagnostics.Application;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.CompilerServices;

#if USE_REFEMIT
    public class XmlObjectSerializerReadContextComplex : XmlObjectSerializerReadContext
#else
    internal class XmlObjectSerializerReadContextComplex : XmlObjectSerializerReadContext
#endif
    {
        static Hashtable dataContractTypeCache = new Hashtable();

        bool preserveObjectReferences;
        protected IDataContractSurrogate dataContractSurrogate;
        SerializationMode mode;
        SerializationBinder binder;
        ISurrogateSelector surrogateSelector;
        FormatterAssemblyStyle assemblyFormat;
        Hashtable surrogateDataContracts;

        internal XmlObjectSerializerReadContextComplex(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver dataContractResolver)
            : base(serializer, rootTypeDataContract, dataContractResolver)
        {
            this.mode = SerializationMode.SharedContract;
            this.preserveObjectReferences = serializer.PreserveObjectReferences;
            this.dataContractSurrogate = serializer.DataContractSurrogate;
        }

        internal XmlObjectSerializerReadContextComplex(NetDataContractSerializer serializer)
            : base(serializer)
        {
            this.mode = SerializationMode.SharedType;
            this.preserveObjectReferences = true;
            this.binder = serializer.Binder;
            this.surrogateSelector = serializer.SurrogateSelector;
            this.assemblyFormat = serializer.AssemblyFormat;
        }

        internal XmlObjectSerializerReadContextComplex(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject)
            : base(serializer, maxItemsInObjectGraph, streamingContext, ignoreExtensionDataObject)
        {
        }

        internal override SerializationMode Mode
        {
            get { return mode; }
        }

        internal override DataContract GetDataContract(int id, RuntimeTypeHandle typeHandle)
        {
            DataContract dataContract = null;
            if (mode == SerializationMode.SharedType && surrogateSelector != null)
            {
                dataContract = NetDataContractSerializer.GetDataContractFromSurrogateSelector(surrogateSelector, GetStreamingContext(), typeHandle, null /*type*/, ref surrogateDataContracts);
            }

            if (dataContract != null)
            {
                if (this.IsGetOnlyCollection && dataContract is SurrogateDataContract)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser,
                        DataContract.GetClrTypeFullName(dataContract.UnderlyingType))));
                }
                return dataContract;
            }

            return base.GetDataContract(id, typeHandle);
        }

        internal override DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type)
        {
            DataContract dataContract = null;
            if (mode == SerializationMode.SharedType && surrogateSelector != null)
            {
                dataContract = NetDataContractSerializer.GetDataContractFromSurrogateSelector(surrogateSelector, GetStreamingContext(), typeHandle, type, ref surrogateDataContracts);
            }

            if (dataContract != null)
            {
                if (this.IsGetOnlyCollection && dataContract is SurrogateDataContract)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser,
                        DataContract.GetClrTypeFullName(dataContract.UnderlyingType))));
                }
                return dataContract;
            }

            return base.GetDataContract(typeHandle, type);
        }

        public override object InternalDeserialize(XmlReaderDelegator xmlReader, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle, string name, string ns)
        {
            if (mode == SerializationMode.SharedContract)
            {
                if (dataContractSurrogate == null)
                    return base.InternalDeserialize(xmlReader, declaredTypeID, declaredTypeHandle, name, ns);
                else
                    return InternalDeserializeWithSurrogate(xmlReader, Type.GetTypeFromHandle(declaredTypeHandle), null /*surrogateDataContract*/, name, ns);
            }
            else
            {
                return InternalDeserializeInSharedTypeMode(xmlReader, declaredTypeID, Type.GetTypeFromHandle(declaredTypeHandle), name, ns);
            }
        }

        internal override object InternalDeserialize(XmlReaderDelegator xmlReader, Type declaredType, string name, string ns)
        {
            if (mode == SerializationMode.SharedContract)
            {
                if (dataContractSurrogate == null)
                    return base.InternalDeserialize(xmlReader, declaredType, name, ns);
                else
                    return InternalDeserializeWithSurrogate(xmlReader, declaredType, null /*surrogateDataContract*/, name, ns);
            }
            else
            {
                return InternalDeserializeInSharedTypeMode(xmlReader, -1, declaredType, name, ns);
            }
        }

        internal override object InternalDeserialize(XmlReaderDelegator xmlReader, Type declaredType, DataContract dataContract, string name, string ns)
        {
            if (mode == SerializationMode.SharedContract)
            {
                if (dataContractSurrogate == null)
                    return base.InternalDeserialize(xmlReader, declaredType, dataContract, name, ns);
                else
                    return InternalDeserializeWithSurrogate(xmlReader, declaredType, dataContract, name, ns);
            }
            else
            {
                return InternalDeserializeInSharedTypeMode(xmlReader, -1, declaredType, name, ns);
            }
        }

        object InternalDeserializeInSharedTypeMode(XmlReaderDelegator xmlReader, int declaredTypeID, Type declaredType, string name, string ns)
        {
            object retObj = null;
            if (TryHandleNullOrRef(xmlReader, declaredType, name, ns, ref retObj))
                return retObj;

            DataContract dataContract;
            string assemblyName = attributes.ClrAssembly;
            string typeName = attributes.ClrType;
            if (assemblyName != null && typeName != null)
            {
                Assembly assembly;
                Type type;
                dataContract = ResolveDataContractInSharedTypeMode(assemblyName, typeName, out assembly, out type);
                if (dataContract == null)
                {
                    if (assembly == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.AssemblyNotFound, assemblyName)));
                    if (type == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ClrTypeNotFound, assembly.FullName, typeName)));
                }
                //Array covariance is not supported in XSD. If declared type is array, data is sent in format of base array
                if (declaredType != null && declaredType.IsArray)
                    dataContract = (declaredTypeID < 0) ? GetDataContract(declaredType) : GetDataContract(declaredTypeID, declaredType.TypeHandle);
            }
            else
            {
                if (assemblyName != null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(xmlReader, SR.GetString(SR.AttributeNotFound, Globals.SerializationNamespace, Globals.ClrTypeLocalName, xmlReader.NodeType, xmlReader.NamespaceURI, xmlReader.LocalName))));
                else if (typeName != null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(xmlReader, SR.GetString(SR.AttributeNotFound, Globals.SerializationNamespace, Globals.ClrAssemblyLocalName, xmlReader.NodeType, xmlReader.NamespaceURI, xmlReader.LocalName))));
                else if (declaredType == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(xmlReader, SR.GetString(SR.AttributeNotFound, Globals.SerializationNamespace, Globals.ClrTypeLocalName, xmlReader.NodeType, xmlReader.NamespaceURI, xmlReader.LocalName))));
                dataContract = (declaredTypeID < 0) ? GetDataContract(declaredType) : GetDataContract(declaredTypeID, declaredType.TypeHandle);
            }
            return ReadDataContractValue(dataContract, xmlReader);
        }

        object InternalDeserializeWithSurrogate(XmlReaderDelegator xmlReader, Type declaredType, DataContract surrogateDataContract, string name, string ns)
        {
            if (TD.DCDeserializeWithSurrogateStartIsEnabled())
            {
                TD.DCDeserializeWithSurrogateStart(surrogateDataContract.UnderlyingType.FullName);
            }

            DataContract dataContract = surrogateDataContract ??
                GetDataContract(DataContractSurrogateCaller.GetDataContractType(dataContractSurrogate, declaredType));
            if (this.IsGetOnlyCollection && dataContract.UnderlyingType != declaredType)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser,
                    DataContract.GetClrTypeFullName(declaredType))));
            }
            ReadAttributes(xmlReader);
            string objectId = GetObjectId();
            object oldObj = InternalDeserialize(xmlReader, name, ns, declaredType, ref dataContract);
            object obj = DataContractSurrogateCaller.GetDeserializedObject(dataContractSurrogate, oldObj, dataContract.UnderlyingType, declaredType);
            ReplaceDeserializedObject(objectId, oldObj, obj);

            if (TD.DCDeserializeWithSurrogateStopIsEnabled())
            {
                TD.DCDeserializeWithSurrogateStop();
            }

            return obj;
        }

        Type ResolveDataContractTypeInSharedTypeMode(string assemblyName, string typeName, out Assembly assembly)
        {
            assembly = null;
            Type type = null;

            if (binder != null)
                type = binder.BindToType(assemblyName, typeName);
            if (type == null)
            {
                XmlObjectDataContractTypeKey key = new XmlObjectDataContractTypeKey(assemblyName, typeName);
                XmlObjectDataContractTypeInfo dataContractTypeInfo = (XmlObjectDataContractTypeInfo)dataContractTypeCache[key];
                if (dataContractTypeInfo == null)
                {
                    if (assemblyFormat == FormatterAssemblyStyle.Full)
                    {
                        if (assemblyName == Globals.MscorlibAssemblyName)
                        {
                            assembly = Globals.TypeOfInt.Assembly;
                        }
                        else
                        {
                            assembly = Assembly.Load(assemblyName);
                        }
                        if (assembly != null)
                            type = assembly.GetType(typeName);
                    }
                    else
                    {
                        assembly = XmlObjectSerializerReadContextComplex.ResolveSimpleAssemblyName(assemblyName);
                        if (assembly != null)
                        {
                            // Catching any exceptions that could be thrown from a failure on assembly load
                            // This is necessary, for example, if there are generic parameters that are qualified with a version of the assembly that predates the one available
                            try
                            {
                                type = assembly.GetType(typeName);
                            }
                            catch (TypeLoadException) { }
                            catch (FileNotFoundException) { }
                            catch (FileLoadException) { }
                            catch (BadImageFormatException) { }

                            if (type == null)
                            {
                                type = Type.GetType(typeName, XmlObjectSerializerReadContextComplex.ResolveSimpleAssemblyName, new TopLevelAssemblyTypeResolver(assembly).ResolveType, false /* throwOnError */);
                            }
                        }
                    }

                    if (type != null)
                    {
                        CheckTypeForwardedTo(assembly, type.Assembly, type);

                        dataContractTypeInfo = new XmlObjectDataContractTypeInfo(assembly, type);
                        lock (dataContractTypeCache)
                        {
                            if (!dataContractTypeCache.ContainsKey(key))
                            {
                                dataContractTypeCache[key] = dataContractTypeInfo;
                            }
                        }
                    }
                }
                else
                {
                    assembly = dataContractTypeInfo.Assembly;
                    type = dataContractTypeInfo.Type;
                }
            }

            return type;
        }

        DataContract ResolveDataContractInSharedTypeMode(string assemblyName, string typeName, out Assembly assembly, out Type type)
        {
            type = ResolveDataContractTypeInSharedTypeMode(assemblyName, typeName, out assembly);
            if (type != null)
            {
                return GetDataContract(type);
            }

            return null;
        }

        protected override DataContract ResolveDataContractFromTypeName()
        {
            if (mode == SerializationMode.SharedContract)
            {
                return base.ResolveDataContractFromTypeName();
            }
            else
            {
                if (attributes.ClrAssembly != null && attributes.ClrType != null)
                {
                    Assembly assembly;
                    Type type;
                    return ResolveDataContractInSharedTypeMode(attributes.ClrAssembly, attributes.ClrType, out assembly, out type);
                }
            }
            return null;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls the critical methods of ISurrogateSelector", Safe = "Demands for FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        bool CheckIfTypeSerializableForSharedTypeMode(Type memberType)
        {
            Fx.Assert(surrogateSelector != null, "Method should not be called when surrogateSelector is null.");
            ISurrogateSelector surrogateSelectorNotUsed;
            return (surrogateSelector.GetSurrogate(memberType, GetStreamingContext(), out surrogateSelectorNotUsed) != null);
        }

        internal override void CheckIfTypeSerializable(Type memberType, bool isMemberTypeSerializable)
        {
            if (mode == SerializationMode.SharedType && surrogateSelector != null &&
                CheckIfTypeSerializableForSharedTypeMode(memberType))
            {
                return;
            }
            else
            {
                if (dataContractSurrogate != null)
                {
                    while (memberType.IsArray)
                        memberType = memberType.GetElementType();
                    memberType = DataContractSurrogateCaller.GetDataContractType(dataContractSurrogate, memberType);
                    if (!DataContract.IsTypeSerializable(memberType))
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.TypeNotSerializable, memberType)));
                    return;
                }
            }

            base.CheckIfTypeSerializable(memberType, isMemberTypeSerializable);
        }

        internal override Type GetSurrogatedType(Type type)
        {
            if (dataContractSurrogate == null)
            {
                return base.GetSurrogatedType(type);
            }
            else
            {
                type = DataContract.UnwrapNullableType(type);
                Type surrogateType = DataContractSerializer.GetSurrogatedType(dataContractSurrogate, type);
                if (this.IsGetOnlyCollection && surrogateType != type)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser,
                        DataContract.GetClrTypeFullName(type))));
                }
                else
                {
                    return surrogateType;
                }
            }
        }

#if USE_REFEMIT
        public override int GetArraySize()
#else
        internal override int GetArraySize()
#endif
        {
            return preserveObjectReferences ? attributes.ArraySZSize : -1;
        }

        static Assembly ResolveSimpleAssemblyName(AssemblyName assemblyName)
        {
            return ResolveSimpleAssemblyName(assemblyName.FullName);
        }

        static Assembly ResolveSimpleAssemblyName(string assemblyName)
        {
            Assembly assembly;
            if (assemblyName == Globals.MscorlibAssemblyName)
            {
                assembly = Globals.TypeOfInt.Assembly;
            }
            else
            {
                assembly = Assembly.LoadWithPartialName(assemblyName);
                if (assembly == null)
                {
                    AssemblyName an = new AssemblyName(assemblyName);
                    an.Version = null;
                    assembly = Assembly.LoadWithPartialName(an.FullName);
                }
            }
            return assembly;
        }

        [Fx.Tag.SecurityNote(Critical = "Gets the SecurityCritical PermissionSet for sourceAssembly and destinationAssembly.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        static void CheckTypeForwardedTo(Assembly sourceAssembly, Assembly destinationAssembly, Type resolvedType)
        {
            if (sourceAssembly != destinationAssembly && !NetDataContractSerializer.UnsafeTypeForwardingEnabled && !sourceAssembly.IsFullyTrusted)
            {
                // We have a TypeForwardedTo attribute
                if (!destinationAssembly.PermissionSet.IsSubsetOf(sourceAssembly.PermissionSet))
                {
                    // We look for a matching TypeForwardedFrom attribute
                    TypeInformation typeInfo = NetDataContractSerializer.GetTypeInformation(resolvedType);
                    if (typeInfo.HasTypeForwardedFrom)
                    {
                        Assembly typeForwardedFromAssembly = null;
                        try
                        {
                            // if this Assembly.Load fails, we still want to throw security exception
                            typeForwardedFromAssembly = Assembly.Load(typeInfo.AssemblyString);
                        }
                        catch { }

                        if (typeForwardedFromAssembly == sourceAssembly)
                        {
                            return;
                        }
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.CannotDeserializeForwardedType, DataContract.GetClrTypeFullName(resolvedType))));
                }
            }
        }

        sealed class TopLevelAssemblyTypeResolver
        {
            Assembly topLevelAssembly;

            public TopLevelAssemblyTypeResolver(Assembly topLevelAssembly)
            {
                this.topLevelAssembly = topLevelAssembly;
            }
            public Type ResolveType(Assembly assembly, string simpleTypeName, bool ignoreCase)
            {
                if (assembly == null)
                    assembly = topLevelAssembly;

                return assembly.GetType(simpleTypeName, false, ignoreCase);
            }
        }

        class XmlObjectDataContractTypeInfo
        {
            Assembly assembly;
            Type type;
            public XmlObjectDataContractTypeInfo(Assembly assembly, Type type)
            {
                this.assembly = assembly;
                this.type = type;
            }

            public Assembly Assembly
            {
                get
                {
                    return this.assembly;
                }
            }

            public Type Type
            {
                get
                {
                    return this.type;
                }
            }
        }

        class XmlObjectDataContractTypeKey
        {
            string assemblyName;
            string typeName;
            public XmlObjectDataContractTypeKey(string assemblyName, string typeName)
            {
                this.assemblyName = assemblyName;
                this.typeName = typeName;
            }

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this, obj))
                    return true;

                XmlObjectDataContractTypeKey other = obj as XmlObjectDataContractTypeKey;
                if (other == null)
                    return false;

                if (this.assemblyName != other.assemblyName)
                    return false;

                if (this.typeName != other.typeName)
                    return false;

                return true;
            }

            public override int GetHashCode()
            {
                int hashCode = 0;
                if (this.assemblyName != null)
                    hashCode = this.assemblyName.GetHashCode();

                if (this.typeName != null)
                    hashCode ^= this.typeName.GetHashCode();

                return hashCode;
            }
        }
    }
}
