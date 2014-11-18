//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization.Diagnostics.Application;
    using System.Security;
    using System.Xml;
    using DataContractDictionary = System.Collections.Generic.Dictionary<System.Xml.XmlQualifiedName, DataContract>;

#if USE_REFEMIT
    public class XmlObjectSerializerContext
#else
    internal class XmlObjectSerializerContext
#endif
    {
        protected XmlObjectSerializer serializer;
        protected DataContract rootTypeDataContract;
        internal ScopedKnownTypes scopedKnownTypes = new ScopedKnownTypes();
        protected DataContractDictionary serializerKnownDataContracts;
        bool isSerializerKnownDataContractsSetExplicit;
        protected IList<Type> serializerKnownTypeList;

        [Fx.Tag.SecurityNote(Critical = "We base the decision whether to Demand SerializationFormatterPermission on this value.")]
        [SecurityCritical]
        bool demandedSerializationFormatterPermission;

        [Fx.Tag.SecurityNote(Critical = "We base the decision whether to Demand MemberAccess on this value.")]
        [SecurityCritical]
        bool demandedMemberAccessPermission;
        int itemCount;
        int maxItemsInObjectGraph;
        StreamingContext streamingContext;
        bool ignoreExtensionDataObject;
        DataContractResolver dataContractResolver;
        KnownTypeDataContractResolver knownTypeResolver;

        internal XmlObjectSerializerContext(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject, DataContractResolver dataContractResolver)
        {
            this.serializer = serializer;
            this.itemCount = 1;
            this.maxItemsInObjectGraph = maxItemsInObjectGraph;
            this.streamingContext = streamingContext;
            this.ignoreExtensionDataObject = ignoreExtensionDataObject;
            this.dataContractResolver = dataContractResolver;
        }

        internal XmlObjectSerializerContext(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject)
            : this(serializer, maxItemsInObjectGraph, streamingContext, ignoreExtensionDataObject, null)
        {
        }

        internal XmlObjectSerializerContext(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver dataContractResolver)
            : this(serializer,
            serializer.MaxItemsInObjectGraph,
            new StreamingContext(StreamingContextStates.All),
            serializer.IgnoreExtensionDataObject,
            dataContractResolver)
        {
            this.rootTypeDataContract = rootTypeDataContract;
            this.serializerKnownTypeList = serializer.knownTypeList;
        }

        internal XmlObjectSerializerContext(NetDataContractSerializer serializer)
            : this(serializer,
            serializer.MaxItemsInObjectGraph,
            serializer.Context,
            serializer.IgnoreExtensionDataObject)
        {
        }

        internal virtual SerializationMode Mode
        {
            get { return SerializationMode.SharedContract; }
        }

        internal virtual bool IsGetOnlyCollection
        {
            get { return false; }
            set { }
        }

        [Fx.Tag.SecurityNote(Critical = "Demands SerializationFormatter permission. demanding the right permission is critical.",
            Safe = "No data or control leaks in or out, must be callable from transparent generated IL.")]
        [SecuritySafeCritical]
        public void DemandSerializationFormatterPermission()
        {
            if (!demandedSerializationFormatterPermission)
            {
                Globals.SerializationFormatterPermission.Demand();
                demandedSerializationFormatterPermission = true;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Demands MemberAccess permission. demanding the right permission is critical.",
            Safe = "No data or control leaks in or out, must be callable from transparent generated IL.")]
        [SecuritySafeCritical]
        public void DemandMemberAccessPermission()
        {
            if (!demandedMemberAccessPermission)
            {
                Globals.MemberAccessPermission.Demand();
                demandedMemberAccessPermission = true;
            }
        }

        public StreamingContext GetStreamingContext()
        {
            return streamingContext;
        }

        static MethodInfo incrementItemCountMethod;
        internal static MethodInfo IncrementItemCountMethod
        {
            get
            {
                if (incrementItemCountMethod == null)
                    incrementItemCountMethod = typeof(XmlObjectSerializerContext).GetMethod("IncrementItemCount", Globals.ScanAllMembers);
                return incrementItemCountMethod;
            }
        }
        public void IncrementItemCount(int count)
        {
            if (count > maxItemsInObjectGraph - itemCount)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ExceededMaxItemsQuota, maxItemsInObjectGraph)));
            itemCount += count;
        }

        internal int RemainingItemCount
        {
            get { return maxItemsInObjectGraph - itemCount; }
        }

        internal bool IgnoreExtensionDataObject
        {
            get { return ignoreExtensionDataObject; }
        }

        protected DataContractResolver DataContractResolver
        {
            get { return dataContractResolver; }
        }

        protected KnownTypeDataContractResolver KnownTypeResolver
        {
            get
            {
                if (knownTypeResolver == null)
                {
                    knownTypeResolver = new KnownTypeDataContractResolver(this);
                }
                return knownTypeResolver;
            }
        }

        internal DataContract GetDataContract(Type type)
        {
            return GetDataContract(type.TypeHandle, type);
        }

        internal virtual DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type)
        {
            if (IsGetOnlyCollection)
            {
                return DataContract.GetGetOnlyCollectionDataContract(DataContract.GetId(typeHandle), typeHandle, type, Mode);
            }
            else
            {
                return DataContract.GetDataContract(typeHandle, type, Mode);
            }
        }

        internal virtual DataContract GetDataContractSkipValidation(int typeId, RuntimeTypeHandle typeHandle, Type type)
        {
            if (IsGetOnlyCollection)
            {
                return DataContract.GetGetOnlyCollectionDataContractSkipValidation(typeId, typeHandle, type);
            }
            else
            {
                return DataContract.GetDataContractSkipValidation(typeId, typeHandle, type);
            }
        }


        internal virtual DataContract GetDataContract(int id, RuntimeTypeHandle typeHandle)
        {
            if (IsGetOnlyCollection)
            {
                return DataContract.GetGetOnlyCollectionDataContract(id, typeHandle, null /*type*/, Mode);
            }
            else
            {
                return DataContract.GetDataContract(id, typeHandle, Mode);
            }
        }

        internal virtual void CheckIfTypeSerializable(Type memberType, bool isMemberTypeSerializable)
        {
            if (!isMemberTypeSerializable)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.TypeNotSerializable, memberType)));
        }

        internal virtual Type GetSurrogatedType(Type type)
        {
            return type;
        }

        DataContractDictionary SerializerKnownDataContracts
        {
            get
            {
                // This field must be initialized during construction by serializers using data contracts.
                if (!this.isSerializerKnownDataContractsSetExplicit)
                {
                    this.serializerKnownDataContracts = serializer.KnownDataContracts;
                    this.isSerializerKnownDataContractsSetExplicit = true;
                }
                return this.serializerKnownDataContracts;
            }
        }

        DataContract GetDataContractFromSerializerKnownTypes(XmlQualifiedName qname)
        {
            DataContractDictionary serializerKnownDataContracts = this.SerializerKnownDataContracts;
            if (serializerKnownDataContracts == null)
                return null;
            DataContract outDataContract;
            return serializerKnownDataContracts.TryGetValue(qname, out outDataContract) ? outDataContract : null;
        }

        internal static DataContractDictionary GetDataContractsForKnownTypes(IList<Type> knownTypeList)
        {
            if (knownTypeList == null) return null;
            DataContractDictionary dataContracts = new DataContractDictionary();
            Dictionary<Type, Type> typesChecked = new Dictionary<Type, Type>();
            for (int i = 0; i < knownTypeList.Count; i++)
            {
                Type knownType = knownTypeList[i];
                if (knownType == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.NullKnownType, "knownTypes")));

                DataContract.CheckAndAdd(knownType, typesChecked, ref dataContracts);
            }
            return dataContracts;
        }

        internal bool IsKnownType(DataContract dataContract, DataContractDictionary knownDataContracts, Type declaredType)
        {
            bool knownTypesAddedInCurrentScope = false;
            if (knownDataContracts != null)
            {
                scopedKnownTypes.Push(knownDataContracts);
                knownTypesAddedInCurrentScope = true;
            }

            bool isKnownType = IsKnownType(dataContract, declaredType);

            if (knownTypesAddedInCurrentScope)
            {
                scopedKnownTypes.Pop();
            }
            return isKnownType;
        }

        internal bool IsKnownType(DataContract dataContract, Type declaredType)
        {
            DataContract knownContract = ResolveDataContractFromKnownTypes(dataContract.StableName.Name, dataContract.StableName.Namespace, null /*memberTypeContract*/, declaredType);
            return knownContract != null && knownContract.UnderlyingType == dataContract.UnderlyingType;
        }

        DataContract ResolveDataContractFromKnownTypes(XmlQualifiedName typeName)
        {
            DataContract dataContract = PrimitiveDataContract.GetPrimitiveDataContract(typeName.Name, typeName.Namespace);
            if (dataContract == null)
            {
                if (typeName.Name == Globals.SafeSerializationManagerName && typeName.Namespace == Globals.SafeSerializationManagerNamespace && Globals.TypeOfSafeSerializationManager != null)
                {
                    return GetDataContract(Globals.TypeOfSafeSerializationManager);
                }
                dataContract = scopedKnownTypes.GetDataContract(typeName);
                if (dataContract == null)
                {
                    dataContract = GetDataContractFromSerializerKnownTypes(typeName);
                }
            }
            return dataContract;
        }

        DataContract ResolveDataContractFromDataContractResolver(XmlQualifiedName typeName, Type declaredType)
        {
            if (TD.DCResolverResolveIsEnabled())
            {
                TD.DCResolverResolve(typeName.Name + ":" + typeName.Namespace);
            }

            Type dataContractType = DataContractResolver.ResolveName(typeName.Name, typeName.Namespace, declaredType, KnownTypeResolver);
            if (dataContractType == null)
            {
                return null;
            }
            else
            {
                return GetDataContract(dataContractType);
            }
        }

        internal Type ResolveNameFromKnownTypes(XmlQualifiedName typeName)
        {
            DataContract dataContract = ResolveDataContractFromKnownTypes(typeName);
            if (dataContract == null)
            {
                return null;
            }
            else
            {
                return dataContract.OriginalUnderlyingType;
            }
        }

        protected DataContract ResolveDataContractFromKnownTypes(string typeName, string typeNs, DataContract memberTypeContract, Type declaredType)
        {
            XmlQualifiedName qname = new XmlQualifiedName(typeName, typeNs);
            DataContract dataContract;
            if (DataContractResolver == null)
            {
                dataContract = ResolveDataContractFromKnownTypes(qname);
            }
            else
            {
                dataContract = ResolveDataContractFromDataContractResolver(qname, declaredType);
            }
            if (dataContract == null)
            {
                if (memberTypeContract != null
                    && !memberTypeContract.UnderlyingType.IsInterface
                    && memberTypeContract.StableName == qname)
                {
                    dataContract = memberTypeContract;
                }
                if (dataContract == null && rootTypeDataContract != null)
                {
                    dataContract = ResolveDataContractFromRootDataContract(qname);
                }
            }
            return dataContract;
        }

        protected virtual DataContract ResolveDataContractFromRootDataContract(XmlQualifiedName typeQName)
        {
            if (rootTypeDataContract.StableName == typeQName)
                return rootTypeDataContract;

            CollectionDataContract collectionContract = rootTypeDataContract as CollectionDataContract;
            while (collectionContract != null)
            {
                DataContract itemContract = GetDataContract(GetSurrogatedType(collectionContract.ItemType));
                if (itemContract.StableName == typeQName)
                {
                    return itemContract;
                }
                collectionContract = itemContract as CollectionDataContract;
            }
            return null;
        }

    }

}
