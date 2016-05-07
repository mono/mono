//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Runtime.Serialization.Diagnostics;

#if USE_REFEMIT
    public class XmlObjectSerializerReadContext : XmlObjectSerializerContext
#else
    internal class XmlObjectSerializerReadContext : XmlObjectSerializerContext
#endif
    {
        internal Attributes attributes;
        HybridObjectCache deserializedObjects;
        XmlSerializableReader xmlSerializableReader;
        XmlDocument xmlDocument;
        Attributes attributesInXmlData;
        XmlReaderDelegator extensionDataReader;
        object getOnlyCollectionValue;
        bool isGetOnlyCollection;

        HybridObjectCache DeserializedObjects
        {
            get
            {
                if (deserializedObjects == null)
                    deserializedObjects = new HybridObjectCache();
                return deserializedObjects;
            }
        }

        XmlDocument Document
        {
            get
            {
                if (xmlDocument == null)
                    xmlDocument = new XmlDocument();
                return xmlDocument;
            }
        }

        internal override bool IsGetOnlyCollection
        {
            get { return this.isGetOnlyCollection; }
            set { this.isGetOnlyCollection = value; }
        }


#if USE_REFEMIT
        public object GetCollectionMember()
#else
        internal object GetCollectionMember()
#endif
        {
            return this.getOnlyCollectionValue;
        }

#if USE_REFEMIT
        public void StoreCollectionMemberInfo(object collectionMember)
#else
        internal void StoreCollectionMemberInfo(object collectionMember)
#endif
        {
            this.getOnlyCollectionValue = collectionMember;
            this.isGetOnlyCollection = true;
        }

#if USE_REFEMIT
        public static void ThrowNullValueReturnedForGetOnlyCollectionException(Type type)
#else
        internal static void ThrowNullValueReturnedForGetOnlyCollectionException(Type type)
#endif
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.NullValueReturnedForGetOnlyCollection, DataContract.GetClrTypeFullName(type))));
        }

#if USE_REFEMIT
        public static void ThrowArrayExceededSizeException(int arraySize, Type type)
#else
        internal static void ThrowArrayExceededSizeException(int arraySize, Type type)
#endif
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ArrayExceededSize, arraySize, DataContract.GetClrTypeFullName(type))));
        }

        internal static XmlObjectSerializerReadContext CreateContext(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver dataContractResolver)
        {
            return (serializer.PreserveObjectReferences || serializer.DataContractSurrogate != null)
                ? new XmlObjectSerializerReadContextComplex(serializer, rootTypeDataContract, dataContractResolver)
                : new XmlObjectSerializerReadContext(serializer, rootTypeDataContract, dataContractResolver);
        }

        internal static XmlObjectSerializerReadContext CreateContext(NetDataContractSerializer serializer)
        {
            return new XmlObjectSerializerReadContextComplex(serializer);
        }

        internal XmlObjectSerializerReadContext(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject)
            : base(serializer, maxItemsInObjectGraph, streamingContext, ignoreExtensionDataObject)
        {
        }

        internal XmlObjectSerializerReadContext(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver dataContractResolver)
            : base(serializer, rootTypeDataContract, dataContractResolver)
        {
            this.attributes = new Attributes();
        }

        protected XmlObjectSerializerReadContext(NetDataContractSerializer serializer)
            : base(serializer)
        {
            this.attributes = new Attributes();
        }

        public virtual object InternalDeserialize(XmlReaderDelegator xmlReader, int id, RuntimeTypeHandle declaredTypeHandle, string name, string ns)
        {
            DataContract dataContract = GetDataContract(id, declaredTypeHandle);
            return InternalDeserialize(xmlReader, name, ns, Type.GetTypeFromHandle(declaredTypeHandle), ref dataContract);
        }

        internal virtual object InternalDeserialize(XmlReaderDelegator xmlReader, Type declaredType, string name, string ns)
        {
            DataContract dataContract = GetDataContract(declaredType);
            return InternalDeserialize(xmlReader, name, ns, declaredType, ref dataContract);
        }

        internal virtual object InternalDeserialize(XmlReaderDelegator xmlReader, Type declaredType, DataContract dataContract, string name, string ns)
        {
            if (dataContract == null)
                GetDataContract(declaredType);
            return InternalDeserialize(xmlReader, name, ns, declaredType, ref dataContract);
        }

        protected bool TryHandleNullOrRef(XmlReaderDelegator reader, Type declaredType, string name, string ns, ref object retObj)
        {
            ReadAttributes(reader);

            if (attributes.Ref != Globals.NewObjectId)
            {
                if (this.isGetOnlyCollection)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.IsReferenceGetOnlyCollectionsNotSupported, attributes.Ref, DataContract.GetClrTypeFullName(declaredType))));
                }
                else
                {
                    retObj = GetExistingObject(attributes.Ref, declaredType, name, ns);
                    reader.Skip();
                    return true;
                }
            }
            else if (attributes.XsiNil)
            {
                reader.Skip();
                return true;
            }
            return false;
        }

        protected object InternalDeserialize(XmlReaderDelegator reader, string name, string ns, Type declaredType, ref DataContract dataContract)
        {
            object retObj = null;
            if (TryHandleNullOrRef(reader, dataContract.UnderlyingType, name, ns, ref retObj))
                return retObj;

            bool knownTypesAddedInCurrentScope = false;
            if (dataContract.KnownDataContracts != null)
            {
                scopedKnownTypes.Push(dataContract.KnownDataContracts);
                knownTypesAddedInCurrentScope = true;
            }

            if (attributes.XsiTypeName != null)
            {
                dataContract = ResolveDataContractFromKnownTypes(attributes.XsiTypeName, attributes.XsiTypeNamespace, dataContract, declaredType);
                if (dataContract == null)
                {
                    if (DataContractResolver == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(reader, SR.GetString(SR.DcTypeNotFoundOnDeserialize, attributes.XsiTypeNamespace, attributes.XsiTypeName, reader.NamespaceURI, reader.LocalName))));
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(reader, SR.GetString(SR.DcTypeNotResolvedOnDeserialize, attributes.XsiTypeNamespace, attributes.XsiTypeName, reader.NamespaceURI, reader.LocalName))));
                }
                knownTypesAddedInCurrentScope = ReplaceScopedKnownTypesTop(dataContract.KnownDataContracts, knownTypesAddedInCurrentScope);
            }

            if (dataContract.IsISerializable && attributes.FactoryTypeName != null)
            {
                DataContract factoryDataContract = ResolveDataContractFromKnownTypes(attributes.FactoryTypeName, attributes.FactoryTypeNamespace, dataContract, declaredType);
                if (factoryDataContract != null)
                {
                    if (factoryDataContract.IsISerializable)
                    {
                        dataContract = factoryDataContract;
                        knownTypesAddedInCurrentScope = ReplaceScopedKnownTypesTop(dataContract.KnownDataContracts, knownTypesAddedInCurrentScope);
                    }
                    else
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.FactoryTypeNotISerializable, DataContract.GetClrTypeFullName(factoryDataContract.UnderlyingType), DataContract.GetClrTypeFullName(dataContract.UnderlyingType))));
                }
                else
                {
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        Dictionary<string, string> values = new Dictionary<string, string>(2);
                        values["FactoryType"] = attributes.FactoryTypeNamespace + ":" + attributes.FactoryTypeName;
                        values["ISerializableType"] = dataContract.StableName.Namespace + ":" + dataContract.StableName.Name;
                        TraceUtility.Trace(TraceEventType.Warning, TraceCode.FactoryTypeNotFound,
                            SR.GetString(SR.TraceCodeFactoryTypeNotFound), new DictionaryTraceRecord(values));
                    }
                }
            }

            if (knownTypesAddedInCurrentScope)
            {
                object obj = ReadDataContractValue(dataContract, reader);
                scopedKnownTypes.Pop();
                return obj;
            }
            else
            {
                return ReadDataContractValue(dataContract, reader);
            }
        }

        bool ReplaceScopedKnownTypesTop(Dictionary<XmlQualifiedName, DataContract> knownDataContracts, bool knownTypesAddedInCurrentScope)
        {
            if (knownTypesAddedInCurrentScope)
            {
                scopedKnownTypes.Pop();
                knownTypesAddedInCurrentScope = false;
            }
            if (knownDataContracts != null)
            {
                scopedKnownTypes.Push(knownDataContracts);
                knownTypesAddedInCurrentScope = true;
            }
            return knownTypesAddedInCurrentScope;
        }

        public static bool MoveToNextElement(XmlReaderDelegator xmlReader)
        {
            return (xmlReader.MoveToContent() != XmlNodeType.EndElement);
        }

        public int GetMemberIndex(XmlReaderDelegator xmlReader, XmlDictionaryString[] memberNames, XmlDictionaryString[] memberNamespaces, int memberIndex, ExtensionDataObject extensionData)
        {
            for (int i = memberIndex + 1; i < memberNames.Length; i++)
            {
                if (xmlReader.IsStartElement(memberNames[i], memberNamespaces[i]))
                    return i;
            }
            HandleMemberNotFound(xmlReader, extensionData, memberIndex);
            return memberNames.Length;
        }

        public int GetMemberIndexWithRequiredMembers(XmlReaderDelegator xmlReader, XmlDictionaryString[] memberNames, XmlDictionaryString[] memberNamespaces, int memberIndex, int requiredIndex, ExtensionDataObject extensionData)
        {
            for (int i = memberIndex + 1; i < memberNames.Length; i++)
            {
                if (xmlReader.IsStartElement(memberNames[i], memberNamespaces[i]))
                {
                    if (requiredIndex < i)
                        ThrowRequiredMemberMissingException(xmlReader, memberIndex, requiredIndex, memberNames);
                    return i;
                }
            }
            HandleMemberNotFound(xmlReader, extensionData, memberIndex);
            return memberNames.Length;
        }

        public static void ThrowRequiredMemberMissingException(XmlReaderDelegator xmlReader, int memberIndex, int requiredIndex, XmlDictionaryString[] memberNames)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (requiredIndex == memberNames.Length)
                requiredIndex--;
            for (int i = memberIndex + 1; i <= requiredIndex; i++)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(" | ");
                stringBuilder.Append(memberNames[i].Value);
            }
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(XmlObjectSerializer.TryAddLineInfo(xmlReader, SR.GetString(SR.UnexpectedElementExpectingElements, xmlReader.NodeType, xmlReader.LocalName, xmlReader.NamespaceURI, stringBuilder.ToString()))));
        }

        protected void HandleMemberNotFound(XmlReaderDelegator xmlReader, ExtensionDataObject extensionData, int memberIndex)
        {
            xmlReader.MoveToContent();
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));

            if (IgnoreExtensionDataObject || extensionData == null)
                SkipUnknownElement(xmlReader);
            else
                HandleUnknownElement(xmlReader, extensionData, memberIndex);
        }

        internal void HandleUnknownElement(XmlReaderDelegator xmlReader, ExtensionDataObject extensionData, int memberIndex)
        {
            if (extensionData.Members == null)
                extensionData.Members = new List<ExtensionDataMember>();
            extensionData.Members.Add(ReadExtensionDataMember(xmlReader, memberIndex));
        }

        public void SkipUnknownElement(XmlReaderDelegator xmlReader)
        {
            ReadAttributes(xmlReader);
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.Trace(TraceEventType.Verbose, TraceCode.ElementIgnored,
                    SR.GetString(SR.TraceCodeElementIgnored), new StringTraceRecord("Element", xmlReader.NamespaceURI + ":" + xmlReader.LocalName));
            }
            xmlReader.Skip();
        }

        public string ReadIfNullOrRef(XmlReaderDelegator xmlReader, Type memberType, bool isMemberTypeSerializable)
        {
            if (attributes.Ref != Globals.NewObjectId)
            {
                CheckIfTypeSerializable(memberType, isMemberTypeSerializable);
                xmlReader.Skip();
                return attributes.Ref;
            }
            else if (attributes.XsiNil)
            {
                CheckIfTypeSerializable(memberType, isMemberTypeSerializable);
                xmlReader.Skip();
                return Globals.NullObjectId;
            }
            return Globals.NewObjectId;
        }

#if USE_REFEMIT
        public virtual void ReadAttributes(XmlReaderDelegator xmlReader)
#else
        internal virtual void ReadAttributes(XmlReaderDelegator xmlReader)
#endif
        {
            if (attributes == null)
                attributes = new Attributes();
            attributes.Read(xmlReader);
        }

        public void ResetAttributes()
        {
            if (attributes != null)
                attributes.Reset();
        }

        public string GetObjectId()
        {
            return attributes.Id;
        }

#if USE_REFEMIT
        public virtual int GetArraySize()
#else
        internal virtual int GetArraySize()
#endif
        {
            return -1;
        }

        public void AddNewObject(object obj)
        {
            AddNewObjectWithId(attributes.Id, obj);
        }

        public void AddNewObjectWithId(string id, object obj)
        {
            if (id != Globals.NewObjectId)
                DeserializedObjects.Add(id, obj);
            if (extensionDataReader != null)
                extensionDataReader.UnderlyingExtensionDataReader.SetDeserializedValue(obj);
        }

        public void ReplaceDeserializedObject(string id, object oldObj, object newObj)
        {
            if (object.ReferenceEquals(oldObj, newObj))
                return;

            if (id != Globals.NewObjectId)
            {
                // In certain cases (IObjectReference, SerializationSurrogate or DataContractSurrogate),
                // an object can be replaced with a different object once it is deserialized. If the 
                // object happens to be referenced from within itself, that reference needs to be updated
                // with the new instance. BinaryFormatter supports this by fixing up such references later. 
                // These XmlObjectSerializer implementations do not currently support fix-ups. Hence we 
                // throw in such cases to allow us add fix-up support in the future if we need to.
                if (DeserializedObjects.IsObjectReferenced(id))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.FactoryObjectContainsSelfReference, DataContract.GetClrTypeFullName(oldObj.GetType()), DataContract.GetClrTypeFullName(newObj.GetType()), id)));
                DeserializedObjects.Remove(id);
                DeserializedObjects.Add(id, newObj);
            }
            if (extensionDataReader != null)
                extensionDataReader.UnderlyingExtensionDataReader.SetDeserializedValue(newObj);
        }

        public object GetExistingObject(string id, Type type, string name, string ns)
        {
            object retObj = DeserializedObjects.GetObject(id);
            if (retObj == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.DeserializedObjectWithIdNotFound, id)));
            if (retObj is IDataNode)
            {
                IDataNode dataNode = (IDataNode)retObj;
                retObj = (dataNode.Value != null && dataNode.IsFinalValue) ? dataNode.Value : DeserializeFromExtensionData(dataNode, type, name, ns);
            }
            return retObj;
        }

        object GetExistingObjectOrExtensionData(string id)
        {
            object retObj = DeserializedObjects.GetObject(id);
            if (retObj == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.DeserializedObjectWithIdNotFound, id)));
            return retObj;
        }

        public object GetRealObject(IObjectReference obj, string id)
        {
            object realObj = SurrogateDataContract.GetRealObject(obj, this.GetStreamingContext());
            // If GetRealObject returns null, it indicates that the object could not resolve itself because 
            // it is missing information. This may occur in a case where multiple IObjectReference instances
            // depend on each other. BinaryFormatter supports this by fixing up the references later. These
            // XmlObjectSerializer implementations do not support fix-ups since the format does not contain
            // forward references. However, we throw for this case since it allows us to add fix-up support 
            // in the future if we need to.
            if (realObj == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.GetRealObjectReturnedNull, DataContract.GetClrTypeFullName(obj.GetType()))));
            ReplaceDeserializedObject(id, obj, realObj);
            return realObj;
        }

        object DeserializeFromExtensionData(IDataNode dataNode, Type type, string name, string ns)
        {
            ExtensionDataReader underlyingExtensionDataReader;
            if (extensionDataReader == null)
            {
                underlyingExtensionDataReader = new ExtensionDataReader(this);
                extensionDataReader = CreateReaderDelegatorForReader(underlyingExtensionDataReader);
            }
            else
                underlyingExtensionDataReader = extensionDataReader.UnderlyingExtensionDataReader;
            underlyingExtensionDataReader.SetDataNode(dataNode, name, ns);
            object retObj = InternalDeserialize(extensionDataReader, type, name, ns);
            dataNode.Clear();
            underlyingExtensionDataReader.Reset();
            return retObj;
        }

        public static void Read(XmlReaderDelegator xmlReader)
        {
            if (!xmlReader.Read())
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.UnexpectedEndOfFile)));
        }

        internal static void ParseQualifiedName(string qname, XmlReaderDelegator xmlReader, out string name, out string ns, out string prefix)
        {
            int colon = qname.IndexOf(':');
            prefix = "";
            if (colon >= 0)
                prefix = qname.Substring(0, colon);
            name = qname.Substring(colon + 1);
            ns = xmlReader.LookupNamespace(prefix);
        }

        public static T[] EnsureArraySize<T>(T[] array, int index)
        {
            if (array.Length <= index)
            {
                if (index == Int32.MaxValue)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        XmlObjectSerializer.CreateSerializationException(
                        SR.GetString(SR.MaxArrayLengthExceeded, Int32.MaxValue,
                        DataContract.GetClrTypeFullName(typeof(T)))));
                }
                int newSize = (index < Int32.MaxValue / 2) ? index * 2 : Int32.MaxValue;
                T[] newArray = new T[newSize];
                Array.Copy(array, 0, newArray, 0, array.Length);
                array = newArray;
            }
            return array;
        }

        public static T[] TrimArraySize<T>(T[] array, int size)
        {
            if (size != array.Length)
            {
                T[] newArray = new T[size];
                Array.Copy(array, 0, newArray, 0, size);
                array = newArray;
            }
            return array;
        }

        public void CheckEndOfArray(XmlReaderDelegator xmlReader, int arraySize, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (xmlReader.NodeType == XmlNodeType.EndElement)
                return;
            while (xmlReader.IsStartElement())
            {
                if (xmlReader.IsStartElement(itemName, itemNamespace))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ArrayExceededSizeAttribute, arraySize, itemName.Value, itemNamespace.Value)));
                SkipUnknownElement(xmlReader);
            }
            if (xmlReader.NodeType != XmlNodeType.EndElement)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.EndElement, xmlReader));
        }

        internal object ReadIXmlSerializable(XmlReaderDelegator xmlReader, XmlDataContract xmlDataContract, bool isMemberType)
        {
            if (xmlSerializableReader == null)
                xmlSerializableReader = new XmlSerializableReader();
            return ReadIXmlSerializable(xmlSerializableReader, xmlReader, xmlDataContract, isMemberType);
        }

        internal static object ReadRootIXmlSerializable(XmlReaderDelegator xmlReader, XmlDataContract xmlDataContract, bool isMemberType)
        {
            return ReadIXmlSerializable(new XmlSerializableReader(), xmlReader, xmlDataContract, isMemberType);
        }

        internal static object ReadIXmlSerializable(XmlSerializableReader xmlSerializableReader, XmlReaderDelegator xmlReader, XmlDataContract xmlDataContract, bool isMemberType)
        {
            object obj = null;
            xmlSerializableReader.BeginRead(xmlReader);
            if (isMemberType && !xmlDataContract.HasRoot)
            {
                xmlReader.Read();
                xmlReader.MoveToContent();
            }
            if (xmlDataContract.UnderlyingType == Globals.TypeOfXmlElement)
            {
                if (!xmlReader.IsStartElement())
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));
                XmlDocument xmlDoc = new XmlDocument();
                obj = (XmlElement)xmlDoc.ReadNode(xmlSerializableReader);
            }
            else if (xmlDataContract.UnderlyingType == Globals.TypeOfXmlNodeArray)
            {
                obj = XmlSerializableServices.ReadNodes(xmlSerializableReader);
            }
            else
            {
                IXmlSerializable xmlSerializable = xmlDataContract.CreateXmlSerializableDelegate();
                xmlSerializable.ReadXml(xmlSerializableReader);
                obj = xmlSerializable;
            }
            xmlSerializableReader.EndRead();
            return obj;
        }

        public SerializationInfo ReadSerializationInfo(XmlReaderDelegator xmlReader, Type type)
        {
            SerializationInfo serInfo = new SerializationInfo(type, XmlObjectSerializer.FormatterConverter);
            XmlNodeType nodeType;
            while ((nodeType = xmlReader.MoveToContent()) != XmlNodeType.EndElement)
            {
                if (nodeType != XmlNodeType.Element)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));

                if (xmlReader.NamespaceURI.Length != 0)
                {
                    SkipUnknownElement(xmlReader);
                    continue;
                }
                string name = XmlConvert.DecodeName(xmlReader.LocalName);

                IncrementItemCount(1);
                ReadAttributes(xmlReader);
                object value;
                if (attributes.Ref != Globals.NewObjectId)
                {
                    xmlReader.Skip();
                    value = GetExistingObject(attributes.Ref, null, name, String.Empty);
                }
                else if (attributes.XsiNil)
                {
                    xmlReader.Skip();
                    value = null;
                }
                else
                {
                    value = InternalDeserialize(xmlReader, Globals.TypeOfObject, name, String.Empty);
                }

                serInfo.AddValue(name, value);
            }

            return serInfo;
        }

        protected virtual DataContract ResolveDataContractFromTypeName()
        {
            return (attributes.XsiTypeName == null) ? null : ResolveDataContractFromKnownTypes(attributes.XsiTypeName, attributes.XsiTypeNamespace, null /*memberTypeContract*/, null);
        }

        ExtensionDataMember ReadExtensionDataMember(XmlReaderDelegator xmlReader, int memberIndex)
        {
            ExtensionDataMember member = new ExtensionDataMember();
            member.Name = xmlReader.LocalName;
            member.Namespace = xmlReader.NamespaceURI;
            member.MemberIndex = memberIndex;
            if (xmlReader.UnderlyingExtensionDataReader != null)
            {
                // no need to re-read extension data structure
                member.Value = xmlReader.UnderlyingExtensionDataReader.GetCurrentNode();
            }
            else
                member.Value = ReadExtensionDataValue(xmlReader);
            return member;
        }

        public IDataNode ReadExtensionDataValue(XmlReaderDelegator xmlReader)
        {
            ReadAttributes(xmlReader);
            IncrementItemCount(1);
            IDataNode dataNode = null;
            if (attributes.Ref != Globals.NewObjectId)
            {
                xmlReader.Skip();
                object o = GetExistingObjectOrExtensionData(attributes.Ref);
                dataNode = (o is IDataNode) ? (IDataNode)o : new DataNode<object>(o);
                dataNode.Id = attributes.Ref;
            }
            else if (attributes.XsiNil)
            {
                xmlReader.Skip();
                dataNode = null;
            }
            else
            {
                string dataContractName = null;
                string dataContractNamespace = null;
                if (attributes.XsiTypeName != null)
                {
                    dataContractName = attributes.XsiTypeName;
                    dataContractNamespace = attributes.XsiTypeNamespace;
                }

                if (IsReadingCollectionExtensionData(xmlReader))
                {
                    Read(xmlReader);
                    dataNode = ReadUnknownCollectionData(xmlReader, dataContractName, dataContractNamespace);
                }
                else if (attributes.FactoryTypeName != null)
                {
                    Read(xmlReader);
                    dataNode = ReadUnknownISerializableData(xmlReader, dataContractName, dataContractNamespace);
                }
                else if (IsReadingClassExtensionData(xmlReader))
                {
                    Read(xmlReader);
                    dataNode = ReadUnknownClassData(xmlReader, dataContractName, dataContractNamespace);
                }
                else
                {
                    DataContract dataContract = ResolveDataContractFromTypeName();

                    if (dataContract == null)
                        dataNode = ReadExtensionDataValue(xmlReader, dataContractName, dataContractNamespace);
                    else if (dataContract is XmlDataContract)
                        dataNode = ReadUnknownXmlData(xmlReader, dataContractName, dataContractNamespace);
                    else
                    {
                        if (dataContract.IsISerializable)
                        {
                            Read(xmlReader);
                            dataNode = ReadUnknownISerializableData(xmlReader, dataContractName, dataContractNamespace);
                        }
                        else if (dataContract is PrimitiveDataContract)
                        {
                            if (attributes.Id == Globals.NewObjectId)
                            {
                                Read(xmlReader);
                                xmlReader.MoveToContent();
                                dataNode = ReadUnknownPrimitiveData(xmlReader, dataContract.UnderlyingType, dataContractName, dataContractNamespace);
                                xmlReader.ReadEndElement();
                            }
                            else
                            {
                                dataNode = new DataNode<object>(xmlReader.ReadElementContentAsAnyType(dataContract.UnderlyingType));
                                InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
                            }
                        }
                        else if (dataContract is EnumDataContract)
                        {
                            dataNode = new DataNode<object>(((EnumDataContract)dataContract).ReadEnumValue(xmlReader));
                            InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
                        }
                        else if (dataContract is ClassDataContract)
                        {
                            Read(xmlReader);
                            dataNode = ReadUnknownClassData(xmlReader, dataContractName, dataContractNamespace);
                        }
                        else if (dataContract is CollectionDataContract)
                        {
                            Read(xmlReader);
                            dataNode = ReadUnknownCollectionData(xmlReader, dataContractName, dataContractNamespace);
                        }
                    }
                }
            }
            return dataNode;
        }

        protected virtual void StartReadExtensionDataValue(XmlReaderDelegator xmlReader)
        {
        }

        IDataNode ReadExtensionDataValue(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            StartReadExtensionDataValue(xmlReader);

            if (attributes.UnrecognizedAttributesFound)
                return ReadUnknownXmlData(xmlReader, dataContractName, dataContractNamespace);

            IDictionary<string, string> namespacesInScope = xmlReader.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
            Read(xmlReader);
            xmlReader.MoveToContent();

            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Text:
                    return ReadPrimitiveExtensionDataValue(xmlReader, dataContractName, dataContractNamespace);
                case XmlNodeType.Element:
                    if (xmlReader.NamespaceURI.StartsWith(Globals.DataContractXsdBaseNamespace, StringComparison.Ordinal))
                        return ReadUnknownClassData(xmlReader, dataContractName, dataContractNamespace);
                    else
                        return ReadAndResolveUnknownXmlData(xmlReader, namespacesInScope, dataContractName, dataContractNamespace);

                case XmlNodeType.EndElement:
                    {
                        // NOTE: cannot distinguish between empty class or IXmlSerializable and typeof(object) 
                        IDataNode objNode = ReadUnknownPrimitiveData(xmlReader, Globals.TypeOfObject, dataContractName, dataContractNamespace);
                        xmlReader.ReadEndElement();
                        objNode.IsFinalValue = false;
                        return objNode;
                    }
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));
            }
        }

        protected virtual IDataNode ReadPrimitiveExtensionDataValue(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            Type valueType = xmlReader.ValueType;
            if (valueType == Globals.TypeOfString)
            {
                // NOTE: cannot distinguish other primitives from string (default XmlReader ValueType)
                IDataNode stringNode = new DataNode<object>(xmlReader.ReadContentAsString());
                InitializeExtensionDataNode(stringNode, dataContractName, dataContractNamespace);
                stringNode.IsFinalValue = false;
                xmlReader.ReadEndElement();
                return stringNode;
            }
            else
            {
                IDataNode objNode = ReadUnknownPrimitiveData(xmlReader, valueType, dataContractName, dataContractNamespace);
                xmlReader.ReadEndElement();
                return objNode;
            }
        }

        protected void InitializeExtensionDataNode(IDataNode dataNode, string dataContractName, string dataContractNamespace)
        {
            dataNode.DataContractName = dataContractName;
            dataNode.DataContractNamespace = dataContractNamespace;
            dataNode.ClrAssemblyName = attributes.ClrAssembly;
            dataNode.ClrTypeName = attributes.ClrType;
            AddNewObject(dataNode);
            dataNode.Id = attributes.Id;
        }

        IDataNode ReadUnknownPrimitiveData(XmlReaderDelegator xmlReader, Type type, string dataContractName, string dataContractNamespace)
        {
            IDataNode dataNode = xmlReader.ReadExtensionData(type);
            InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
            return dataNode;
        }

        ClassDataNode ReadUnknownClassData(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            ClassDataNode dataNode = new ClassDataNode();
            InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);

            int memberIndex = 0;
            XmlNodeType nodeType;
            while ((nodeType = xmlReader.MoveToContent()) != XmlNodeType.EndElement)
            {
                if (nodeType != XmlNodeType.Element)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));

                if (dataNode.Members == null)
                    dataNode.Members = new List<ExtensionDataMember>();
                dataNode.Members.Add(ReadExtensionDataMember(xmlReader, memberIndex++));
            }
            xmlReader.ReadEndElement();
            return dataNode;
        }

        CollectionDataNode ReadUnknownCollectionData(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            CollectionDataNode dataNode = new CollectionDataNode();
            InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);

            int arraySize = attributes.ArraySZSize;
            XmlNodeType nodeType;
            while ((nodeType = xmlReader.MoveToContent()) != XmlNodeType.EndElement)
            {
                if (nodeType != XmlNodeType.Element)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));

                if (dataNode.ItemName == null)
                {
                    dataNode.ItemName = xmlReader.LocalName;
                    dataNode.ItemNamespace = xmlReader.NamespaceURI;
                }
                if (xmlReader.IsStartElement(dataNode.ItemName, dataNode.ItemNamespace))
                {
                    if (dataNode.Items == null)
                        dataNode.Items = new List<IDataNode>();
                    dataNode.Items.Add(ReadExtensionDataValue(xmlReader));
                }
                else
                    SkipUnknownElement(xmlReader);
            }
            xmlReader.ReadEndElement();

            if (arraySize != -1)
            {
                dataNode.Size = arraySize;
                if (dataNode.Items == null)
                {
                    if (dataNode.Size > 0)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ArraySizeAttributeIncorrect, arraySize, 0)));
                }
                else if (dataNode.Size != dataNode.Items.Count)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ArraySizeAttributeIncorrect, arraySize, dataNode.Items.Count)));
            }
            else
            {
                if (dataNode.Items != null)
                {
                    dataNode.Size = dataNode.Items.Count;
                }
                else
                {
                    dataNode.Size = 0;
                }
            }

            return dataNode;
        }

        ISerializableDataNode ReadUnknownISerializableData(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            ISerializableDataNode dataNode = new ISerializableDataNode();
            InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);

            dataNode.FactoryTypeName = attributes.FactoryTypeName;
            dataNode.FactoryTypeNamespace = attributes.FactoryTypeNamespace;

            XmlNodeType nodeType;
            while ((nodeType = xmlReader.MoveToContent()) != XmlNodeType.EndElement)
            {
                if (nodeType != XmlNodeType.Element)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));

                if (xmlReader.NamespaceURI.Length != 0)
                {
                    SkipUnknownElement(xmlReader);
                    continue;
                }

                ISerializableDataMember member = new ISerializableDataMember();
                member.Name = xmlReader.LocalName;
                member.Value = ReadExtensionDataValue(xmlReader);
                if (dataNode.Members == null)
                    dataNode.Members = new List<ISerializableDataMember>();
                dataNode.Members.Add(member);
            }
            xmlReader.ReadEndElement();
            return dataNode;
        }

        IDataNode ReadUnknownXmlData(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            XmlDataNode dataNode = new XmlDataNode();
            InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
            dataNode.OwnerDocument = Document;

            if (xmlReader.NodeType == XmlNodeType.EndElement)
                return dataNode;

            IList<XmlAttribute> xmlAttributes = null;
            IList<XmlNode> xmlChildNodes = null;

            XmlNodeType nodeType = xmlReader.MoveToContent();
            if (nodeType != XmlNodeType.Text)
            {
                while (xmlReader.MoveToNextAttribute())
                {
                    string ns = xmlReader.NamespaceURI;
                    if (ns != Globals.SerializationNamespace && ns != Globals.SchemaInstanceNamespace)
                    {
                        if (xmlAttributes == null)
                            xmlAttributes = new List<XmlAttribute>();
                        xmlAttributes.Add((XmlAttribute)Document.ReadNode(xmlReader.UnderlyingReader));
                    }
                }
                Read(xmlReader);
            }

            while ((nodeType = xmlReader.MoveToContent()) != XmlNodeType.EndElement)
            {
                if (xmlReader.EOF)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.UnexpectedEndOfFile)));

                if (xmlChildNodes == null)
                    xmlChildNodes = new List<XmlNode>();
                xmlChildNodes.Add(Document.ReadNode(xmlReader.UnderlyingReader));
            }
            xmlReader.ReadEndElement();

            dataNode.XmlAttributes = xmlAttributes;
            dataNode.XmlChildNodes = xmlChildNodes;
            return dataNode;
        }

        // Pattern-recognition logic: the method reads XML elements into DOM. To recognize as an array, it requires that 
        // all items have the same name and namespace. To recognize as an ISerializable type, it requires that all
        // items be unqualified. If the XML only contains elements (no attributes or other nodes) is recognized as a 
        // class/class hierarchy. Otherwise it is deserialized as XML.
        IDataNode ReadAndResolveUnknownXmlData(XmlReaderDelegator xmlReader, IDictionary<string, string> namespaces,
            string dataContractName, string dataContractNamespace)
        {
            bool couldBeISerializableData = true;
            bool couldBeCollectionData = true;
            bool couldBeClassData = true;
            string elementNs = null, elementName = null;
            IList<XmlNode> xmlChildNodes = new List<XmlNode>();
            IList<XmlAttribute> xmlAttributes = null;
            if (namespaces != null)
            {
                xmlAttributes = new List<XmlAttribute>();
                foreach (KeyValuePair<string, string> prefixNsPair in namespaces)
                {
                    xmlAttributes.Add(AddNamespaceDeclaration(prefixNsPair.Key, prefixNsPair.Value));
                }
            }

            XmlNodeType nodeType;
            while ((nodeType = xmlReader.NodeType) != XmlNodeType.EndElement)
            {
                if (nodeType == XmlNodeType.Element)
                {
                    string ns = xmlReader.NamespaceURI;
                    string name = xmlReader.LocalName;
                    if (couldBeISerializableData)
                        couldBeISerializableData = (ns.Length == 0);
                    if (couldBeCollectionData)
                    {
                        if (elementName == null)
                        {
                            elementName = name;
                            elementNs = ns;
                        }
                        else
                            couldBeCollectionData = (String.CompareOrdinal(elementName, name) == 0) &&
                                (String.CompareOrdinal(elementNs, ns) == 0);
                    }
                }
                else if (xmlReader.EOF)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.UnexpectedEndOfFile)));
                else if (IsContentNode(xmlReader.NodeType))
                    couldBeClassData = couldBeISerializableData = couldBeCollectionData = false;

                if (attributesInXmlData == null) attributesInXmlData = new Attributes();
                attributesInXmlData.Read(xmlReader);

                XmlNode childNode = Document.ReadNode(xmlReader.UnderlyingReader);
                xmlChildNodes.Add(childNode);

                if (namespaces == null)
                {
                    if (attributesInXmlData.XsiTypeName != null)
                        childNode.Attributes.Append(AddNamespaceDeclaration(attributesInXmlData.XsiTypePrefix, attributesInXmlData.XsiTypeNamespace));
                    if (attributesInXmlData.FactoryTypeName != null)
                        childNode.Attributes.Append(AddNamespaceDeclaration(attributesInXmlData.FactoryTypePrefix, attributesInXmlData.FactoryTypeNamespace));
                }
            }
            xmlReader.ReadEndElement();

            if (elementName != null && couldBeCollectionData)
                return ReadUnknownCollectionData(CreateReaderOverChildNodes(xmlAttributes, xmlChildNodes), dataContractName, dataContractNamespace);
            else if (couldBeISerializableData)
                return ReadUnknownISerializableData(CreateReaderOverChildNodes(xmlAttributes, xmlChildNodes), dataContractName, dataContractNamespace);
            else if (couldBeClassData)
                return ReadUnknownClassData(CreateReaderOverChildNodes(xmlAttributes, xmlChildNodes), dataContractName, dataContractNamespace);
            else
            {
                XmlDataNode dataNode = new XmlDataNode();
                InitializeExtensionDataNode(dataNode, dataContractName, dataContractNamespace);
                dataNode.OwnerDocument = Document;
                dataNode.XmlChildNodes = xmlChildNodes;
                dataNode.XmlAttributes = xmlAttributes;
                return dataNode;
            }
        }

        bool IsContentNode(XmlNodeType nodeType)
        {
            switch (nodeType)
            {
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Comment:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.DocumentType:
                    return false;
                default:
                    return true;
            }
        }

        internal XmlReaderDelegator CreateReaderOverChildNodes(IList<XmlAttribute> xmlAttributes, IList<XmlNode> xmlChildNodes)
        {
            XmlNode wrapperElement = CreateWrapperXmlElement(Document, xmlAttributes, xmlChildNodes, null, null, null);
            XmlReaderDelegator nodeReader = CreateReaderDelegatorForReader(new XmlNodeReader(wrapperElement));
            nodeReader.MoveToContent();
            Read(nodeReader);
            return nodeReader;
        }

        internal static XmlNode CreateWrapperXmlElement(XmlDocument document, IList<XmlAttribute> xmlAttributes, IList<XmlNode> xmlChildNodes, string prefix, string localName, string ns)
        {
            localName = localName ?? "wrapper";
            ns = ns ?? String.Empty;
            XmlNode wrapperElement = document.CreateElement(prefix, localName, ns);
            if (xmlAttributes != null)
            {
                for (int i = 0; i < xmlAttributes.Count; i++)
                    wrapperElement.Attributes.Append((XmlAttribute)xmlAttributes[i]);
            }
            if (xmlChildNodes != null)
            {
                for (int i = 0; i < xmlChildNodes.Count; i++)
                    wrapperElement.AppendChild(xmlChildNodes[i]);
            }
            return wrapperElement;
        }

        XmlAttribute AddNamespaceDeclaration(string prefix, string ns)
        {
            XmlAttribute attribute = (prefix == null || prefix.Length == 0) ?
                Document.CreateAttribute(null, Globals.XmlnsPrefix, Globals.XmlnsNamespace) :
                Document.CreateAttribute(Globals.XmlnsPrefix, prefix, Globals.XmlnsNamespace);
            attribute.Value = ns;
            return attribute;
        }

        public static Exception CreateUnexpectedStateException(XmlNodeType expectedState, XmlReaderDelegator xmlReader)
        {
            return XmlObjectSerializer.CreateSerializationExceptionWithReaderDetails(SR.GetString(SR.ExpectingState, expectedState), xmlReader);
        }

        protected virtual object ReadDataContractValue(DataContract dataContract, XmlReaderDelegator reader)
        {
            return dataContract.ReadXmlValue(reader, this);
        }

        protected virtual XmlReaderDelegator CreateReaderDelegatorForReader(XmlReader xmlReader)
        {
            return new XmlReaderDelegator(xmlReader);
        }

        protected virtual bool IsReadingCollectionExtensionData(XmlReaderDelegator xmlReader)
        {
            return (attributes.ArraySZSize != -1);
        }

        protected virtual bool IsReadingClassExtensionData(XmlReaderDelegator xmlReader)
        {
            return false;
        }
    }
}
