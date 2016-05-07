//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.ServiceModel;
    using System.Collections;
    using DataContractDictionary = System.Collections.Generic.Dictionary<System.Xml.XmlQualifiedName, DataContract>;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class DataContractJsonSerializer : XmlObjectSerializer
    {
        internal IList<Type> knownTypeList;
        internal DataContractDictionary knownDataContracts;
        EmitTypeInformation emitTypeInformation;
        IDataContractSurrogate dataContractSurrogate;
        bool ignoreExtensionDataObject;
        ReadOnlyCollection<Type> knownTypeCollection;
        int maxItemsInObjectGraph;
        DataContract rootContract; // post-surrogate
        XmlDictionaryString rootName;
        bool rootNameRequiresMapping;
        Type rootType;
        bool serializeReadOnlyTypes;
        DateTimeFormat dateTimeFormat;
        bool useSimpleDictionaryFormat;

        public DataContractJsonSerializer(Type type)
            : this(type, (IEnumerable<Type>)null)
        {
        }

        public DataContractJsonSerializer(Type type, string rootName)
            : this(type, rootName, null)
        {
        }

        public DataContractJsonSerializer(Type type, XmlDictionaryString rootName)
            : this(type, rootName, null)
        {
        }

        public DataContractJsonSerializer(Type type, IEnumerable<Type> knownTypes)
            : this(type, knownTypes, int.MaxValue, false, null, false)
        {
        }


        public DataContractJsonSerializer(Type type, string rootName, IEnumerable<Type> knownTypes)
            : this(type, rootName, knownTypes, int.MaxValue, false, null, false)
        {
        }

        public DataContractJsonSerializer(Type type, XmlDictionaryString rootName, IEnumerable<Type> knownTypes)
            : this(type, rootName, knownTypes, int.MaxValue, false, null, false)
        {
        }

        public DataContractJsonSerializer(Type type,
            IEnumerable<Type> knownTypes,
            int maxItemsInObjectGraph,
            bool ignoreExtensionDataObject,
            IDataContractSurrogate dataContractSurrogate,
            bool alwaysEmitTypeInformation)
        {
            EmitTypeInformation emitTypeInformation = alwaysEmitTypeInformation ? EmitTypeInformation.Always : EmitTypeInformation.AsNeeded;
            Initialize(type, knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, dataContractSurrogate, emitTypeInformation, false, null, false);
        }

        public DataContractJsonSerializer(Type type, string rootName,
            IEnumerable<Type> knownTypes,
            int maxItemsInObjectGraph,
            bool ignoreExtensionDataObject,
            IDataContractSurrogate dataContractSurrogate,
            bool alwaysEmitTypeInformation)
        {
            EmitTypeInformation emitTypeInformation = alwaysEmitTypeInformation ? EmitTypeInformation.Always : EmitTypeInformation.AsNeeded;
            XmlDictionary dictionary = new XmlDictionary(2);
            Initialize(type, dictionary.Add(rootName), knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, dataContractSurrogate, emitTypeInformation, false, null, false);
        }

        public DataContractJsonSerializer(Type type, XmlDictionaryString rootName,
            IEnumerable<Type> knownTypes,
            int maxItemsInObjectGraph,
            bool ignoreExtensionDataObject,
            IDataContractSurrogate dataContractSurrogate,
            bool alwaysEmitTypeInformation)
        {
            EmitTypeInformation emitTypeInformation = alwaysEmitTypeInformation ? EmitTypeInformation.Always : EmitTypeInformation.AsNeeded;
            Initialize(type, rootName, knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, dataContractSurrogate, emitTypeInformation, false, null, false);
        }

        public DataContractJsonSerializer(Type type, DataContractJsonSerializerSettings settings)
        {
            if (settings == null)
            {
                settings = new DataContractJsonSerializerSettings();
            }

            XmlDictionaryString rootName = (settings.RootName == null) ? null : new XmlDictionary(1).Add(settings.RootName);
            Initialize(type, rootName, settings.KnownTypes, settings.MaxItemsInObjectGraph, settings.IgnoreExtensionDataObject, settings.DataContractSurrogate,
                settings.EmitTypeInformation, settings.SerializeReadOnlyTypes, settings.DateTimeFormat, settings.UseSimpleDictionaryFormat);
        }

        public IDataContractSurrogate DataContractSurrogate
        {
            get { return dataContractSurrogate; }
        }

        public bool IgnoreExtensionDataObject
        {
            get { return ignoreExtensionDataObject; }
        }

        public ReadOnlyCollection<Type> KnownTypes
        {
            get
            {
                if (knownTypeCollection == null)
                {
                    if (knownTypeList != null)
                    {
                        knownTypeCollection = new ReadOnlyCollection<Type>(knownTypeList);
                    }
                    else
                    {
                        knownTypeCollection = new ReadOnlyCollection<Type>(Globals.EmptyTypeArray);
                    }
                }
                return knownTypeCollection;
            }
        }

        internal override DataContractDictionary KnownDataContracts
        {
            get
            {
                if (this.knownDataContracts == null && this.knownTypeList != null)
                {
                    // This assignment may be performed concurrently and thus is a race condition.
                    // It's safe, however, because at worse a new (and identical) dictionary of 
                    // data contracts will be created and re-assigned to this field.  Introduction 
                    // of a lock here could lead to deadlocks.
                    this.knownDataContracts = XmlObjectSerializerContext.GetDataContractsForKnownTypes(this.knownTypeList);
                }
                return this.knownDataContracts;
            }
        }

        public int MaxItemsInObjectGraph
        {
            get { return maxItemsInObjectGraph; }
        }

        internal bool AlwaysEmitTypeInformation
        {
            get
            {
                return emitTypeInformation == EmitTypeInformation.Always;
            }
        }

        public EmitTypeInformation EmitTypeInformation
        {
            get
            {
                return emitTypeInformation;
            }
        }

        public bool SerializeReadOnlyTypes
        {
            get
            {
                return serializeReadOnlyTypes;
            }
        }

        public DateTimeFormat DateTimeFormat
        {
            get
            {
                return dateTimeFormat;
            }
        }

        public bool UseSimpleDictionaryFormat
        {
            get
            {
                return useSimpleDictionaryFormat;
            }
        }

        DataContract RootContract
        {
            get
            {
                if (rootContract == null)
                {
                    rootContract = DataContract.GetDataContract(((dataContractSurrogate == null) ? rootType :
                        DataContractSerializer.GetSurrogatedType(dataContractSurrogate, rootType)));
                    CheckIfTypeIsReference(rootContract);
                }
                return rootContract;
            }
        }

        XmlDictionaryString RootName
        {
            get
            {
                return rootName ?? JsonGlobals.rootDictionaryString;
            }
        }

        public override bool IsStartObject(XmlReader reader)
        {
            // No need to pass in DateTimeFormat to JsonReaderDelegator: no DateTimes will be read in IsStartObject
            return IsStartObjectHandleExceptions(new JsonReaderDelegator(reader));
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            // No need to pass in DateTimeFormat to JsonReaderDelegator: no DateTimes will be read in IsStartObject
            return IsStartObjectHandleExceptions(new JsonReaderDelegator(reader));
        }

        public override object ReadObject(Stream stream)
        {
            CheckNull(stream, "stream");
            return ReadObject(JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max));
        }

        public override object ReadObject(XmlReader reader)
        {
            return ReadObjectHandleExceptions(new JsonReaderDelegator(reader, this.DateTimeFormat), true);
        }

        public override object ReadObject(XmlReader reader, bool verifyObjectName)
        {
            return ReadObjectHandleExceptions(new JsonReaderDelegator(reader, this.DateTimeFormat), verifyObjectName);
        }

        public override object ReadObject(XmlDictionaryReader reader)
        {
            return ReadObjectHandleExceptions(new JsonReaderDelegator(reader, this.DateTimeFormat), true); // verifyObjectName
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            return ReadObjectHandleExceptions(new JsonReaderDelegator(reader, this.DateTimeFormat), verifyObjectName);
        }

        public override void WriteEndObject(XmlWriter writer)
        {
            // No need to pass in DateTimeFormat to JsonWriterDelegator: no DateTimes will be written in end object
            WriteEndObjectHandleExceptions(new JsonWriterDelegator(writer));
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            // No need to pass in DateTimeFormat to JsonWriterDelegator: no DateTimes will be written in end object
            WriteEndObjectHandleExceptions(new JsonWriterDelegator(writer));
        }


        public override void WriteObject(Stream stream, object graph)
        {
            CheckNull(stream, "stream");
            XmlDictionaryWriter jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, false); //  ownsStream 
            WriteObject(jsonWriter, graph);
            jsonWriter.Flush();
        }

        public override void WriteObject(XmlWriter writer, object graph)
        {
            WriteObjectHandleExceptions(new JsonWriterDelegator(writer, this.DateTimeFormat), graph);
        }

        public override void WriteObject(XmlDictionaryWriter writer, object graph)
        {
            WriteObjectHandleExceptions(new JsonWriterDelegator(writer, this.DateTimeFormat), graph);
        }

        public override void WriteObjectContent(XmlWriter writer, object graph)
        {
            WriteObjectContentHandleExceptions(new JsonWriterDelegator(writer, this.DateTimeFormat), graph);
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            WriteObjectContentHandleExceptions(new JsonWriterDelegator(writer, this.DateTimeFormat), graph);
        }

        public override void WriteStartObject(XmlWriter writer, object graph)
        {
            // No need to pass in DateTimeFormat to JsonWriterDelegator: no DateTimes will be written in start object
            WriteStartObjectHandleExceptions(new JsonWriterDelegator(writer), graph);
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            // No need to pass in DateTimeFormat to JsonWriterDelegator: no DateTimes will be written in start object
            WriteStartObjectHandleExceptions(new JsonWriterDelegator(writer), graph);
        }

        internal static bool CheckIfJsonNameRequiresMapping(string jsonName)
        {
            if (jsonName != null)
            {
                if (!DataContract.IsValidNCName(jsonName))
                {
                    return true;
                }

                for (int i = 0; i < jsonName.Length; i++)
                {
                    if (XmlJsonWriter.CharacterNeedsEscaping(jsonName[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool CheckIfJsonNameRequiresMapping(XmlDictionaryString jsonName)
        {
            return (jsonName == null) ? false : CheckIfJsonNameRequiresMapping(jsonName.Value);
        }

        internal static bool CheckIfXmlNameRequiresMapping(string xmlName)
        {
            return (xmlName == null) ? false : CheckIfJsonNameRequiresMapping(ConvertXmlNameToJsonName(xmlName));
        }

        internal static bool CheckIfXmlNameRequiresMapping(XmlDictionaryString xmlName)
        {
            return (xmlName == null) ? false : CheckIfXmlNameRequiresMapping(xmlName.Value);
        }

        internal static string ConvertXmlNameToJsonName(string xmlName)
        {
            return XmlConvert.DecodeName(xmlName);
        }

        internal static XmlDictionaryString ConvertXmlNameToJsonName(XmlDictionaryString xmlName)
        {
            return (xmlName == null) ? null : new XmlDictionary().Add(ConvertXmlNameToJsonName(xmlName.Value));
        }

        internal static bool IsJsonLocalName(XmlReaderDelegator reader, string elementName)
        {
            string name;
            if (XmlObjectSerializerReadContextComplexJson.TryGetJsonLocalName(reader, out name))
            {
                return (elementName == name);
            }
            return false;
        }

        internal static object ReadJsonValue(DataContract contract, XmlReaderDelegator reader, XmlObjectSerializerReadContextComplexJson context)
        {
            return JsonDataContract.GetJsonDataContract(contract).ReadJsonValue(reader, context);
        }

        internal static void WriteJsonNull(XmlWriterDelegator writer)
        {
            writer.WriteAttributeString(null, JsonGlobals.typeString, null, JsonGlobals.nullString); //  prefix //  namespace 
        }

        internal static void WriteJsonValue(JsonDataContract contract, XmlWriterDelegator writer, object graph, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            contract.WriteJsonValue(writer, graph, context, declaredTypeHandle);
        }

        internal override Type GetDeserializeType()
        {
            return rootType;
        }

        internal override Type GetSerializeType(object graph)
        {
            return (graph == null) ? rootType : graph.GetType();
        }

        internal override bool InternalIsStartObject(XmlReaderDelegator reader)
        {
            if (IsRootElement(reader, RootContract, RootName, XmlDictionaryString.Empty))
            {
                return true;
            }

            return IsJsonLocalName(reader, RootName.Value);
        }

        internal override object InternalReadObject(XmlReaderDelegator xmlReader, bool verifyObjectName)
        {
            if (MaxItemsInObjectGraph == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString(System.Runtime.Serialization.SR.ExceededMaxItemsQuota, MaxItemsInObjectGraph)));
            }

            if (verifyObjectName)
            {
                if (!InternalIsStartObject(xmlReader))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationExceptionWithReaderDetails(System.Runtime.Serialization.SR.GetString(System.Runtime.Serialization.SR.ExpectingElement, XmlDictionaryString.Empty, RootName), xmlReader));
                }
            }
            else if (!IsStartElement(xmlReader))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationExceptionWithReaderDetails(System.Runtime.Serialization.SR.GetString(System.Runtime.Serialization.SR.ExpectingElementAtDeserialize, XmlNodeType.Element), xmlReader));
            }

            DataContract contract = RootContract;
            if (contract.IsPrimitive && object.ReferenceEquals(contract.UnderlyingType, rootType))// handle Nullable<T> differently
            {
                return DataContractJsonSerializer.ReadJsonValue(contract, xmlReader, null);
            }

            XmlObjectSerializerReadContextComplexJson context = XmlObjectSerializerReadContextComplexJson.CreateContext(this, contract);
            return context.InternalDeserialize(xmlReader, rootType, contract, null, null);
        }

        internal override void InternalWriteEndObject(XmlWriterDelegator writer)
        {
            writer.WriteEndElement();
        }

        internal override void InternalWriteObject(XmlWriterDelegator writer, object graph)
        {
            InternalWriteStartObject(writer, graph);
            InternalWriteObjectContent(writer, graph);
            InternalWriteEndObject(writer);
        }

        internal override void InternalWriteObjectContent(XmlWriterDelegator writer, object graph)
        {
            if (MaxItemsInObjectGraph == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString(System.Runtime.Serialization.SR.ExceededMaxItemsQuota, MaxItemsInObjectGraph)));
            }

            DataContract contract = RootContract;
            Type declaredType = contract.UnderlyingType;
            Type graphType = (graph == null) ? declaredType : graph.GetType();

            if (dataContractSurrogate != null)
            {
                graph = DataContractSerializer.SurrogateToDataContractType(dataContractSurrogate, graph, declaredType, ref graphType);
            }

            if (graph == null)
            {
                WriteJsonNull(writer);
            }
            else
            {
                if (declaredType == graphType)
                {
                    if (contract.CanContainReferences)
                    {
                        XmlObjectSerializerWriteContextComplexJson context = XmlObjectSerializerWriteContextComplexJson.CreateContext(this, contract);
                        context.OnHandleReference(writer, graph, true); //  canContainReferences 
                        context.SerializeWithoutXsiType(contract, writer, graph, declaredType.TypeHandle);
                    }
                    else
                    {
                        DataContractJsonSerializer.WriteJsonValue(JsonDataContract.GetJsonDataContract(contract), writer, graph, null, declaredType.TypeHandle); //  XmlObjectSerializerWriteContextComplexJson 
                    }
                }
                else
                {
                    XmlObjectSerializerWriteContextComplexJson context = XmlObjectSerializerWriteContextComplexJson.CreateContext(this, RootContract);
                    contract = DataContractJsonSerializer.GetDataContract(contract, declaredType, graphType);
                    if (contract.CanContainReferences)
                    {
                        context.OnHandleReference(writer, graph, true); //  canContainCyclicReference 
                        context.SerializeWithXsiTypeAtTopLevel(contract, writer, graph, declaredType.TypeHandle, graphType);
                    }
                    else
                    {
                        context.SerializeWithoutXsiType(contract, writer, graph, declaredType.TypeHandle);
                    }
                }
            }
        }

        internal override void InternalWriteStartObject(XmlWriterDelegator writer, object graph)
        {
            if (this.rootNameRequiresMapping)
            {
                writer.WriteStartElement("a", JsonGlobals.itemString, JsonGlobals.itemString);
                writer.WriteAttributeString(null, JsonGlobals.itemString, null, RootName.Value);
            }
            else
            {
                writer.WriteStartElement(RootName, XmlDictionaryString.Empty);
            }
        }

        void AddCollectionItemTypeToKnownTypes(Type knownType)
        {
            Type itemType;
            Type typeToCheck = knownType;
            while (CollectionDataContract.IsCollection(typeToCheck, out itemType))
            {
                if (itemType.IsGenericType && (itemType.GetGenericTypeDefinition() == Globals.TypeOfKeyValue))
                {
                    itemType = Globals.TypeOfKeyValuePair.MakeGenericType(itemType.GetGenericArguments());
                }
                this.knownTypeList.Add(itemType);
                typeToCheck = itemType;
            }
        }

        void Initialize(Type type,
            IEnumerable<Type> knownTypes,
            int maxItemsInObjectGraph,
            bool ignoreExtensionDataObject,
            IDataContractSurrogate dataContractSurrogate,
            EmitTypeInformation emitTypeInformation,
            bool serializeReadOnlyTypes,
            DateTimeFormat dateTimeFormat,
            bool useSimpleDictionaryFormat)
        {
            CheckNull(type, "type");
            this.rootType = type;

            if (knownTypes != null)
            {
                this.knownTypeList = new List<Type>();
                foreach (Type knownType in knownTypes)
                {
                    this.knownTypeList.Add(knownType);
                    if (knownType != null)
                    {
                        AddCollectionItemTypeToKnownTypes(knownType);
                    }
                }
            }

            if (maxItemsInObjectGraph < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxItemsInObjectGraph", System.Runtime.Serialization.SR.GetString(System.Runtime.Serialization.SR.ValueMustBeNonNegative)));
            }
            this.maxItemsInObjectGraph = maxItemsInObjectGraph;
            this.ignoreExtensionDataObject = ignoreExtensionDataObject;
            this.dataContractSurrogate = dataContractSurrogate;
            this.emitTypeInformation = emitTypeInformation;
            this.serializeReadOnlyTypes = serializeReadOnlyTypes;
            this.dateTimeFormat = dateTimeFormat;
            this.useSimpleDictionaryFormat = useSimpleDictionaryFormat;
        }

        void Initialize(Type type,
            XmlDictionaryString rootName,
            IEnumerable<Type> knownTypes,
            int maxItemsInObjectGraph,
            bool ignoreExtensionDataObject,
            IDataContractSurrogate dataContractSurrogate,
            EmitTypeInformation emitTypeInformation,
            bool serializeReadOnlyTypes,
            DateTimeFormat dateTimeFormat,
            bool useSimpleDictionaryFormat)
        {
            Initialize(type, knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, dataContractSurrogate, emitTypeInformation, serializeReadOnlyTypes, dateTimeFormat, useSimpleDictionaryFormat);
            this.rootName = ConvertXmlNameToJsonName(rootName);
            this.rootNameRequiresMapping = CheckIfJsonNameRequiresMapping(this.rootName);
        }

        internal static void CheckIfTypeIsReference(DataContract dataContract)
        {
            if (dataContract.IsReference)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    XmlObjectSerializer.CreateSerializationException(SR.GetString(
                        SR.JsonUnsupportedForIsReference,
                        DataContract.GetClrTypeFullName(dataContract.UnderlyingType),
                        dataContract.IsReference)));
            }
        }

        internal static DataContract GetDataContract(DataContract declaredTypeContract, Type declaredType, Type objectType)
        {
            DataContract contract = DataContractSerializer.GetDataContract(declaredTypeContract, declaredType, objectType);
            CheckIfTypeIsReference(contract);
            return contract;
        }
    }
}
