using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.ServiceModel;
using System.Collections;

namespace System.Runtime.Serialization.Json
{
#if USE_REFEMIT
    public class XmlObjectSerializerWriteContextComplexJson : XmlObjectSerializerWriteContextComplex
#else
    internal class XmlObjectSerializerWriteContextComplexJson : XmlObjectSerializerWriteContextComplex
#endif
    {
        EmitTypeInformation emitXsiType;
        bool perCallXsiTypeAlreadyEmitted;
        bool useSimpleDictionaryFormat;

        public XmlObjectSerializerWriteContextComplexJson(DataContractJsonSerializer serializer, DataContract rootTypeDataContract)
            : base(serializer,
            serializer.MaxItemsInObjectGraph,
            new StreamingContext(StreamingContextStates.All),
            serializer.IgnoreExtensionDataObject)
        {
            this.emitXsiType = serializer.EmitTypeInformation;
            this.rootTypeDataContract = rootTypeDataContract;
            this.serializerKnownTypeList = serializer.knownTypeList;
            this.dataContractSurrogate = serializer.DataContractSurrogate;
            this.serializeReadOnlyTypes = serializer.SerializeReadOnlyTypes;
            this.useSimpleDictionaryFormat = serializer.UseSimpleDictionaryFormat;
        }

        internal static XmlObjectSerializerWriteContextComplexJson CreateContext(DataContractJsonSerializer serializer, DataContract rootTypeDataContract)
        {
            return new XmlObjectSerializerWriteContextComplexJson(serializer, rootTypeDataContract);
        }

        internal IList<Type> SerializerKnownTypeList
        {
            get
            {
                return this.serializerKnownTypeList;
            }
        }

        public bool UseSimpleDictionaryFormat
        {
            get
            {
                return this.useSimpleDictionaryFormat;
            }
        }

        internal override bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, Type dataContractType, string clrTypeName, string clrAssemblyName)
        {
            return false;
        }

        internal override bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, DataContract dataContract)
        {
            return false;
        }

        internal override void WriteArraySize(XmlWriterDelegator xmlWriter, int size)
        {
        }

        protected override void WriteTypeInfo(XmlWriterDelegator writer, string dataContractName, string dataContractNamespace)
        {
            if (this.emitXsiType != EmitTypeInformation.Never)
            {
                if (string.IsNullOrEmpty(dataContractNamespace))
                {
                    WriteTypeInfo(writer, dataContractName);
                }
                else
                {
                    WriteTypeInfo(writer, string.Concat(dataContractName, JsonGlobals.NameValueSeparatorString, TruncateDefaultDataContractNamespace(dataContractNamespace)));
                }
            }
        }

        internal static string TruncateDefaultDataContractNamespace(string dataContractNamespace)
        {
            if (!string.IsNullOrEmpty(dataContractNamespace))
            {
                if (dataContractNamespace[0] == '#')
                {
                    return string.Concat("\\", dataContractNamespace);
                }
                else if (dataContractNamespace[0] == '\\')
                {
                    return string.Concat("\\", dataContractNamespace);
                }
                else if (dataContractNamespace.StartsWith(Globals.DataContractXsdBaseNamespace, StringComparison.Ordinal))
                {
                    return string.Concat("#", dataContractNamespace.Substring(JsonGlobals.DataContractXsdBaseNamespaceLength));
                }
            }

            return dataContractNamespace;
        }

        static bool RequiresJsonTypeInfo(DataContract contract)
        {
            return (contract is ClassDataContract);
        }

        void WriteTypeInfo(XmlWriterDelegator writer, string typeInformation)
        {
            writer.WriteAttributeString(null, JsonGlobals.serverTypeString, null, typeInformation);
        }

        protected override bool WriteTypeInfo(XmlWriterDelegator writer, DataContract contract, DataContract declaredContract)
        {
            if (!((object.ReferenceEquals(contract.Name, declaredContract.Name) &&
                   object.ReferenceEquals(contract.Namespace, declaredContract.Namespace)) ||
                 (contract.Name.Value == declaredContract.Name.Value &&
                 contract.Namespace.Value == declaredContract.Namespace.Value)) &&
                 (contract.UnderlyingType != Globals.TypeOfObjectArray) &&
                 (this.emitXsiType != EmitTypeInformation.Never))
            {
                // We always deserialize collections assigned to System.Object as object[]
                // Because of its common and JSON-specific nature, 
                //    we don't want to validate known type information for object[]

                // Don't validate known type information when emitXsiType == Never because
                // known types are not used without type information in the JSON

                if (RequiresJsonTypeInfo(contract))
                {
                    perCallXsiTypeAlreadyEmitted = true;
                    WriteTypeInfo(writer, contract.Name.Value, contract.Namespace.Value);
                }
                else
                {
                    // check if the declared type is System.Enum and throw because
                    // __type information cannot be written for enums since it results in invalid JSON.
                    // Without __type, the resulting JSON cannot be deserialized since a number cannot be directly assigned to System.Enum.
                    if (declaredContract.UnderlyingType == typeof(Enum))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException
                            (SR.GetString(SR.EnumTypeNotSupportedByDataContractJsonSerializer, declaredContract.UnderlyingType)));
                    }

                }
                // Return true regardless of whether we actually wrote __type information
                // E.g. We don't write __type information for enums, but we still want verifyKnownType
                //      to be true for them.
                return true;
            }
            return false;
        }


#if USE_REFEMIT
        public void WriteJsonISerializable(XmlWriterDelegator xmlWriter, ISerializable obj)
#else
        internal void WriteJsonISerializable(XmlWriterDelegator xmlWriter, ISerializable obj)
#endif
        {
            Type objType = obj.GetType();
            SerializationInfo serInfo = new SerializationInfo(objType, XmlObjectSerializer.FormatterConverter);
            GetObjectData(obj, serInfo, GetStreamingContext()); 
            if (DataContract.GetClrTypeFullName(objType) != serInfo.FullTypeName)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ChangingFullTypeNameNotSupported, serInfo.FullTypeName, DataContract.GetClrTypeFullName(objType))));
            }
            else
            {
                base.WriteSerializationInfo(xmlWriter, objType, serInfo);
            }
        }


#if USE_REFEMIT
        public static DataContract GetRevisedItemContract(DataContract oldItemContract)
#else
        internal static DataContract GetRevisedItemContract(DataContract oldItemContract)
#endif
        {
            if ((oldItemContract != null) &&
                oldItemContract.UnderlyingType.IsGenericType &&
                (oldItemContract.UnderlyingType.GetGenericTypeDefinition() == Globals.TypeOfKeyValue))
            {
                return DataContract.GetDataContract(oldItemContract.UnderlyingType);
            }
            return oldItemContract;
        }

        protected override void WriteDataContractValue(DataContract dataContract, XmlWriterDelegator xmlWriter, object obj, RuntimeTypeHandle declaredTypeHandle)
        {
            JsonDataContract jsonDataContract = JsonDataContract.GetJsonDataContract(dataContract);
            if (emitXsiType == EmitTypeInformation.Always && !perCallXsiTypeAlreadyEmitted && RequiresJsonTypeInfo(dataContract))
            {
                WriteTypeInfo(xmlWriter, jsonDataContract.TypeName);
            }
            perCallXsiTypeAlreadyEmitted = false;
            DataContractJsonSerializer.WriteJsonValue(jsonDataContract, xmlWriter, obj, this, declaredTypeHandle);
        }

        protected override void WriteNull(XmlWriterDelegator xmlWriter)
        {
            DataContractJsonSerializer.WriteJsonNull(xmlWriter);
        }

#if USE_REFEMIT
        public XmlDictionaryString CollectionItemName
#else
        internal XmlDictionaryString CollectionItemName
#endif
        {
            get { return JsonGlobals.itemDictionaryString; }
        }

#if USE_REFEMIT
        public static void WriteJsonNameWithMapping(XmlWriterDelegator xmlWriter, XmlDictionaryString[] memberNames, int index)
#else
        internal static void WriteJsonNameWithMapping(XmlWriterDelegator xmlWriter, XmlDictionaryString[] memberNames, int index)
#endif
        {
            xmlWriter.WriteStartElement("a", JsonGlobals.itemString, JsonGlobals.itemString);
            xmlWriter.WriteAttributeString(null, JsonGlobals.itemString, null, memberNames[index].Value);
        }

        internal override void WriteExtensionDataTypeInfo(XmlWriterDelegator xmlWriter, IDataNode dataNode)
        {
            Type dataType = dataNode.DataType;
            if (dataType == Globals.TypeOfClassDataNode ||
                dataType == Globals.TypeOfISerializableDataNode)
            {
                xmlWriter.WriteAttributeString(null, JsonGlobals.typeString, null, JsonGlobals.objectString);
                base.WriteExtensionDataTypeInfo(xmlWriter, dataNode);
            }
            else if (dataType == Globals.TypeOfCollectionDataNode)
            {
                xmlWriter.WriteAttributeString(null, JsonGlobals.typeString, null, JsonGlobals.arrayString);
                // Don't write __type for collections
            }
            else if (dataType == Globals.TypeOfXmlDataNode)
            {
                // Don't write type or __type for XML types because we serialize them to strings
            }
            else if ((dataType == Globals.TypeOfObject) && (dataNode.Value != null))
            {
                DataContract dc = GetDataContract(dataNode.Value.GetType());
                if (RequiresJsonTypeInfo(dc))
                {
                    base.WriteExtensionDataTypeInfo(xmlWriter, dataNode);
                }
            }
        }

        protected override void SerializeWithXsiType(XmlWriterDelegator xmlWriter, object obj, RuntimeTypeHandle objectTypeHandle, Type objectType, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle, Type declaredType)
        {
            DataContract dataContract;
            bool verifyKnownType = false;
            bool isDeclaredTypeInterface = declaredType.IsInterface;

            if (isDeclaredTypeInterface && CollectionDataContract.IsCollectionInterface(declaredType))
            {
                dataContract = GetDataContract(declaredTypeHandle, declaredType);
            }
            else if (declaredType.IsArray) // If declared type is array do not write __serverType. Instead write__serverType for each item
            {
                dataContract = GetDataContract(declaredTypeHandle, declaredType);
            }
            else
            {
                dataContract = GetDataContract(objectTypeHandle, objectType);
                DataContract declaredTypeContract = (declaredTypeID >= 0)
                    ? GetDataContract(declaredTypeID, declaredTypeHandle)
                    : GetDataContract(declaredTypeHandle, declaredType);
                verifyKnownType = WriteTypeInfo(xmlWriter, dataContract, declaredTypeContract);
                HandleCollectionAssignedToObject(declaredType, ref dataContract, ref obj, ref verifyKnownType);
            }

            if (isDeclaredTypeInterface)
            {
                VerifyObjectCompatibilityWithInterface(dataContract, obj, declaredType);
            }
            SerializeAndVerifyType(dataContract, xmlWriter, obj, verifyKnownType, declaredType.TypeHandle, declaredType);
        }

        static void VerifyObjectCompatibilityWithInterface(DataContract contract, object graph, Type declaredType)
        {
            Type contractType = contract.GetType();
            if ((contractType == typeof(XmlDataContract)) && !Globals.TypeOfIXmlSerializable.IsAssignableFrom(declaredType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.XmlObjectAssignedToIncompatibleInterface, graph.GetType(), declaredType)));
            }

            if ((contractType == typeof(CollectionDataContract)) && !CollectionDataContract.IsCollectionInterface(declaredType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.CollectionAssignedToIncompatibleInterface, graph.GetType(), declaredType)));
            }
        }

        void HandleCollectionAssignedToObject(Type declaredType, ref DataContract dataContract, ref object obj, ref bool verifyKnownType)
        {
            if ((declaredType != dataContract.UnderlyingType) && (dataContract is CollectionDataContract))
            {
                if (verifyKnownType)
                {
                    VerifyType(dataContract, declaredType);
                    verifyKnownType = false;
                }
                if (((CollectionDataContract)dataContract).Kind == CollectionKind.Dictionary)
                {
                    // Convert non-generic dictionary to generic dictionary
                    IDictionary dictionaryObj = obj as IDictionary;
                    Dictionary<object, object> genericDictionaryObj = new Dictionary<object, object>();
                    foreach (DictionaryEntry entry in dictionaryObj)
                    {
                        genericDictionaryObj.Add(entry.Key, entry.Value);
                    }
                    obj = genericDictionaryObj;
                }
                dataContract = GetDataContract(Globals.TypeOfIEnumerable);
            }
        }

        internal override void SerializeWithXsiTypeAtTopLevel(DataContract dataContract, XmlWriterDelegator xmlWriter, object obj, RuntimeTypeHandle originalDeclaredTypeHandle, Type graphType)
        {
            bool verifyKnownType = false;
            Type declaredType = rootTypeDataContract.UnderlyingType;
            bool isDeclaredTypeInterface = declaredType.IsInterface;

            if (!(isDeclaredTypeInterface && CollectionDataContract.IsCollectionInterface(declaredType))
                && !declaredType.IsArray)//Array covariance is not supported in XSD. If declared type is array do not write xsi:type. Instead write xsi:type for each item
            {
                verifyKnownType = WriteTypeInfo(xmlWriter, dataContract, rootTypeDataContract);
                HandleCollectionAssignedToObject(declaredType, ref dataContract, ref obj, ref verifyKnownType);
            }

            if (isDeclaredTypeInterface)
            {
                VerifyObjectCompatibilityWithInterface(dataContract, obj, declaredType);
            }
            SerializeAndVerifyType(dataContract, xmlWriter, obj, verifyKnownType, declaredType.TypeHandle, declaredType);
        }

        void VerifyType(DataContract dataContract, Type declaredType)
        {
            bool knownTypesAddedInCurrentScope = false;
            if (dataContract.KnownDataContracts != null)
            {
                scopedKnownTypes.Push(dataContract.KnownDataContracts);
                knownTypesAddedInCurrentScope = true;
            }

            if (!IsKnownType(dataContract, declaredType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.DcTypeNotFoundOnSerialize, DataContract.GetClrTypeFullName(dataContract.UnderlyingType), dataContract.StableName.Name, dataContract.StableName.Namespace)));
            }

            if (knownTypesAddedInCurrentScope)
            {
                scopedKnownTypes.Pop();
            }
        }

        internal override DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type)
        {
            DataContract dataContract = base.GetDataContract(typeHandle, type);
            DataContractJsonSerializer.CheckIfTypeIsReference(dataContract);
            return dataContract;
        }

        internal override DataContract GetDataContractSkipValidation(int typeId, RuntimeTypeHandle typeHandle, Type type)
        {
            DataContract dataContract = base.GetDataContractSkipValidation(typeId, typeHandle, type);
            DataContractJsonSerializer.CheckIfTypeIsReference(dataContract);
            return dataContract;
        }

        internal override DataContract GetDataContract(int id, RuntimeTypeHandle typeHandle)
        {
            DataContract dataContract = base.GetDataContract(id, typeHandle);
            DataContractJsonSerializer.CheckIfTypeIsReference(dataContract);
            return dataContract;
        }

        internal static DataContract ResolveJsonDataContractFromRootDataContract(XmlObjectSerializerContext context, XmlQualifiedName typeQName, DataContract rootTypeDataContract)
        {
            if (rootTypeDataContract.StableName == typeQName)
                return rootTypeDataContract;

            CollectionDataContract collectionContract = rootTypeDataContract as CollectionDataContract;
            while (collectionContract != null)
            {
                DataContract itemContract;
                if (collectionContract.ItemType.IsGenericType
                    && collectionContract.ItemType.GetGenericTypeDefinition() == typeof(KeyValue<,>))
                {
                    itemContract = context.GetDataContract(Globals.TypeOfKeyValuePair.MakeGenericType(collectionContract.ItemType.GetGenericArguments()));
                }
                else
                {
                    itemContract = context.GetDataContract(context.GetSurrogatedType(collectionContract.ItemType));
                }
                if (itemContract.StableName == typeQName)
                {
                    return itemContract;
                }
                collectionContract = itemContract as CollectionDataContract;
            }
            return null;
        }

        protected override DataContract ResolveDataContractFromRootDataContract(XmlQualifiedName typeQName)
        {
            return XmlObjectSerializerWriteContextComplexJson.ResolveJsonDataContractFromRootDataContract(this, typeQName, rootTypeDataContract);
        }
    }
}
