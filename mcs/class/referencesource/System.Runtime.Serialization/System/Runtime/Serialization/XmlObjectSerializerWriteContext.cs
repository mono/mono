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
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using System.ServiceModel.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Diagnostics;

#if USE_REFEMIT
    public class XmlObjectSerializerWriteContext : XmlObjectSerializerContext
#else
    internal class XmlObjectSerializerWriteContext : XmlObjectSerializerContext
#endif
    {
        ObjectReferenceStack byValObjectsInScope = new ObjectReferenceStack();
        XmlSerializableWriter xmlSerializableWriter;
        const int depthToCheckCyclicReference = 512;
        protected bool preserveObjectReferences;
        ObjectToIdCache serializedObjects;
        bool isGetOnlyCollection;
        readonly bool unsafeTypeForwardingEnabled;
        protected bool serializeReadOnlyTypes;

        internal static XmlObjectSerializerWriteContext CreateContext(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver dataContractResolver)
        {
            return (serializer.PreserveObjectReferences || serializer.DataContractSurrogate != null)
                ? new XmlObjectSerializerWriteContextComplex(serializer, rootTypeDataContract, dataContractResolver)
                : new XmlObjectSerializerWriteContext(serializer, rootTypeDataContract, dataContractResolver);
        }

        internal static XmlObjectSerializerWriteContext CreateContext(NetDataContractSerializer serializer, Hashtable surrogateDataContracts)
        {
            return new XmlObjectSerializerWriteContextComplex(serializer, surrogateDataContracts);
        }

        protected XmlObjectSerializerWriteContext(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver resolver)
            : base(serializer, rootTypeDataContract, resolver)
        {
            this.serializeReadOnlyTypes = serializer.SerializeReadOnlyTypes;
            // Known types restricts the set of types that can be deserialized
            this.unsafeTypeForwardingEnabled = true;
        }

        protected XmlObjectSerializerWriteContext(NetDataContractSerializer serializer)
            : base(serializer)
        {
            this.unsafeTypeForwardingEnabled = NetDataContractSerializer.UnsafeTypeForwardingEnabled;
        }

        internal XmlObjectSerializerWriteContext(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject)
            : base(serializer, maxItemsInObjectGraph, streamingContext, ignoreExtensionDataObject)
        {
            // Known types restricts the set of types that can be deserialized
            this.unsafeTypeForwardingEnabled = true;
        }

#if USE_REFEMIT
        internal ObjectToIdCache SerializedObjects
#else
        protected ObjectToIdCache SerializedObjects
#endif
        {
            get
            {
                if (serializedObjects == null)
                    serializedObjects = new ObjectToIdCache();
                return serializedObjects;
            }
        }

        internal override bool IsGetOnlyCollection
        {
            get { return this.isGetOnlyCollection; }
            set { this.isGetOnlyCollection = value; }
        }

        internal bool SerializeReadOnlyTypes
        {
            get { return this.serializeReadOnlyTypes; }
        }

        internal bool UnsafeTypeForwardingEnabled
        {
            get { return this.unsafeTypeForwardingEnabled; }
        }

#if USE_REFEMIT
        public void StoreIsGetOnlyCollection()
#else
        internal void StoreIsGetOnlyCollection()
#endif
        {
            this.isGetOnlyCollection = true;
        }

        public void InternalSerializeReference(XmlWriterDelegator xmlWriter, object obj, bool isDeclaredType, bool writeXsiType, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle)
        {
            if (!OnHandleReference(xmlWriter, obj, true /*canContainCyclicReference*/))
                InternalSerialize(xmlWriter, obj, isDeclaredType, writeXsiType, declaredTypeID, declaredTypeHandle);
            OnEndHandleReference(xmlWriter, obj, true /*canContainCyclicReference*/);
        }

        public virtual void InternalSerialize(XmlWriterDelegator xmlWriter, object obj, bool isDeclaredType, bool writeXsiType, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle)
        {
            if (writeXsiType)
            {
                Type declaredType = Globals.TypeOfObject;
                SerializeWithXsiType(xmlWriter, obj, Type.GetTypeHandle(obj), null/*type*/, -1, declaredType.TypeHandle, declaredType);
            }
            else if (isDeclaredType)
            {
                DataContract contract = GetDataContract(declaredTypeID, declaredTypeHandle);
                SerializeWithoutXsiType(contract, xmlWriter, obj, declaredTypeHandle);
            }
            else
            {
                RuntimeTypeHandle objTypeHandle = Type.GetTypeHandle(obj);
                if (declaredTypeHandle.Equals(objTypeHandle))
                {
                    DataContract dataContract = (declaredTypeID >= 0)
                        ? GetDataContract(declaredTypeID, declaredTypeHandle)
                        : GetDataContract(declaredTypeHandle, null /*type*/);
                    SerializeWithoutXsiType(dataContract, xmlWriter, obj, declaredTypeHandle);
                }
                else
                {
                    SerializeWithXsiType(xmlWriter, obj, objTypeHandle, null /*type*/, declaredTypeID, declaredTypeHandle, Type.GetTypeFromHandle(declaredTypeHandle));
                }
            }
        }

        internal void SerializeWithoutXsiType(DataContract dataContract, XmlWriterDelegator xmlWriter, object obj, RuntimeTypeHandle declaredTypeHandle)
        {
            if (OnHandleIsReference(xmlWriter, dataContract, obj))
                return;
            if (dataContract.KnownDataContracts != null)
            {
                scopedKnownTypes.Push(dataContract.KnownDataContracts);
                WriteDataContractValue(dataContract, xmlWriter, obj, declaredTypeHandle);
                scopedKnownTypes.Pop();
            }
            else
            {
                WriteDataContractValue(dataContract, xmlWriter, obj, declaredTypeHandle);
            }
        }

        internal virtual void SerializeWithXsiTypeAtTopLevel(DataContract dataContract, XmlWriterDelegator xmlWriter, object obj, RuntimeTypeHandle originalDeclaredTypeHandle, Type graphType)
        {
            bool verifyKnownType = false;
            Type declaredType = rootTypeDataContract.OriginalUnderlyingType;

            if (declaredType.IsInterface && CollectionDataContract.IsCollectionInterface(declaredType))
            {
                if (DataContractResolver != null)
                {
                    WriteResolvedTypeInfo(xmlWriter, graphType, declaredType);
                }
            }
            else if (!declaredType.IsArray) //Array covariance is not supported in XSD. If declared type is array do not write xsi:type. Instead write xsi:type for each item
            {
                verifyKnownType = WriteTypeInfo(xmlWriter, dataContract, rootTypeDataContract);
            }
            SerializeAndVerifyType(dataContract, xmlWriter, obj, verifyKnownType, originalDeclaredTypeHandle, declaredType);
        }

        protected virtual void SerializeWithXsiType(XmlWriterDelegator xmlWriter, object obj, RuntimeTypeHandle objectTypeHandle, Type objectType, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle, Type declaredType)
        {
            DataContract dataContract;
            bool verifyKnownType = false;
            if (declaredType.IsInterface && CollectionDataContract.IsCollectionInterface(declaredType))
            {
                dataContract = GetDataContractSkipValidation(DataContract.GetId(objectTypeHandle), objectTypeHandle, objectType);
                if (OnHandleIsReference(xmlWriter, dataContract, obj))
                    return;
                if (this.Mode == SerializationMode.SharedType && dataContract.IsValidContract(this.Mode))
                    dataContract = dataContract.GetValidContract(this.Mode);
                else
                    dataContract = GetDataContract(declaredTypeHandle, declaredType);
                if (!WriteClrTypeInfo(xmlWriter, dataContract) && DataContractResolver != null)
                {
                    if (objectType == null)
                    {
                        objectType = Type.GetTypeFromHandle(objectTypeHandle);
                    }
                    WriteResolvedTypeInfo(xmlWriter, objectType, declaredType);
                }
            }
            else if (declaredType.IsArray)//Array covariance is not supported in XSD. If declared type is array do not write xsi:type. Instead write xsi:type for each item
            {
                // A call to OnHandleIsReference is not necessary here -- arrays cannot be IsReference
                dataContract = GetDataContract(objectTypeHandle, objectType);
                WriteClrTypeInfo(xmlWriter, dataContract);
                dataContract = GetDataContract(declaredTypeHandle, declaredType);
            }
            else
            {
                dataContract = GetDataContract(objectTypeHandle, objectType);
                if (OnHandleIsReference(xmlWriter, dataContract, obj))
                    return;
                if (!WriteClrTypeInfo(xmlWriter, dataContract))
                {
                    DataContract declaredTypeContract = (declaredTypeID >= 0)
                        ? GetDataContract(declaredTypeID, declaredTypeHandle)
                        : GetDataContract(declaredTypeHandle, declaredType);
                    verifyKnownType = WriteTypeInfo(xmlWriter, dataContract, declaredTypeContract);
                }
            }

            SerializeAndVerifyType(dataContract, xmlWriter, obj, verifyKnownType, declaredTypeHandle, declaredType);
        }

        internal bool OnHandleIsReference(XmlWriterDelegator xmlWriter, DataContract contract, object obj)
        {
            if (preserveObjectReferences || !contract.IsReference || isGetOnlyCollection)
            {
                return false;
            }

            bool isNew = true;
            int objectId = SerializedObjects.GetId(obj, ref isNew);
            byValObjectsInScope.EnsureSetAsIsReference(obj);
            if (isNew)
            {
                xmlWriter.WriteAttributeString(Globals.SerPrefix, DictionaryGlobals.IdLocalName,
                                            DictionaryGlobals.SerializationNamespace, string.Format(CultureInfo.InvariantCulture, "{0}{1}", "i", objectId));
                return false;
            }
            else
            {
                xmlWriter.WriteAttributeString(Globals.SerPrefix, DictionaryGlobals.RefLocalName, DictionaryGlobals.SerializationNamespace, string.Format(CultureInfo.InvariantCulture, "{0}{1}", "i", objectId));
                return true;
            }
        }

        protected void SerializeAndVerifyType(DataContract dataContract, XmlWriterDelegator xmlWriter, object obj, bool verifyKnownType, RuntimeTypeHandle declaredTypeHandle, Type declaredType)
        {
            bool knownTypesAddedInCurrentScope = false;
            if (dataContract.KnownDataContracts != null)
            {
                scopedKnownTypes.Push(dataContract.KnownDataContracts);
                knownTypesAddedInCurrentScope = true;
            }

            if (verifyKnownType)
            {
                if (!IsKnownType(dataContract, declaredType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.DcTypeNotFoundOnSerialize, DataContract.GetClrTypeFullName(dataContract.UnderlyingType), dataContract.StableName.Name, dataContract.StableName.Namespace)));
                }
            }
            WriteDataContractValue(dataContract, xmlWriter, obj, declaredTypeHandle);

            if (knownTypesAddedInCurrentScope)
            {
                scopedKnownTypes.Pop();
            }
        }

        internal virtual bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, DataContract dataContract)
        {
            return false;
        }

        internal virtual bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, Type dataContractType, string clrTypeName, string clrAssemblyName)
        {
            return false;
        }

        internal virtual bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, Type dataContractType, SerializationInfo serInfo)
        {
            return false;
        }

        public virtual void WriteAnyType(XmlWriterDelegator xmlWriter, object value)
        {
            xmlWriter.WriteAnyType(value);
        }

        public virtual void WriteString(XmlWriterDelegator xmlWriter, string value)
        {
            xmlWriter.WriteString(value);
        }
        public virtual void WriteString(XmlWriterDelegator xmlWriter, string value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
                WriteNull(xmlWriter, typeof(string), true/*isMemberTypeSerializable*/, name, ns);
            else
            {
                xmlWriter.WriteStartElementPrimitive(name, ns);
                xmlWriter.WriteString(value);
                xmlWriter.WriteEndElementPrimitive();
            }
        }

        public virtual void WriteBase64(XmlWriterDelegator xmlWriter, byte[] value)
        {
            xmlWriter.WriteBase64(value);
        }
        public virtual void WriteBase64(XmlWriterDelegator xmlWriter, byte[] value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
                WriteNull(xmlWriter, typeof(byte[]), true/*isMemberTypeSerializable*/, name, ns);
            else
            {
                xmlWriter.WriteStartElementPrimitive(name, ns);
                xmlWriter.WriteBase64(value);
                xmlWriter.WriteEndElementPrimitive();
            }
        }

        public virtual void WriteUri(XmlWriterDelegator xmlWriter, Uri value)
        {
            xmlWriter.WriteUri(value);
        }
        public virtual void WriteUri(XmlWriterDelegator xmlWriter, Uri value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
                WriteNull(xmlWriter, typeof(Uri), true/*isMemberTypeSerializable*/, name, ns);
            else
            {
                xmlWriter.WriteStartElementPrimitive(name, ns);
                xmlWriter.WriteUri(value);
                xmlWriter.WriteEndElementPrimitive();
            }
        }

        public virtual void WriteQName(XmlWriterDelegator xmlWriter, XmlQualifiedName value)
        {
            xmlWriter.WriteQName(value);
        }
        public virtual void WriteQName(XmlWriterDelegator xmlWriter, XmlQualifiedName value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
                WriteNull(xmlWriter, typeof(XmlQualifiedName), true/*isMemberTypeSerializable*/, name, ns);
            else
            {
                if (ns != null && ns.Value != null && ns.Value.Length > 0)
                    xmlWriter.WriteStartElement(Globals.ElementPrefix, name, ns);
                else
                    xmlWriter.WriteStartElement(name, ns);
                xmlWriter.WriteQName(value);
                xmlWriter.WriteEndElement();
            }
        }

        internal void HandleGraphAtTopLevel(XmlWriterDelegator writer, object obj, DataContract contract)
        {
            writer.WriteXmlnsAttribute(Globals.XsiPrefix, DictionaryGlobals.SchemaInstanceNamespace);
            if (contract.IsISerializable)
                writer.WriteXmlnsAttribute(Globals.XsdPrefix, DictionaryGlobals.SchemaNamespace);
            OnHandleReference(writer, obj, true /*canContainReferences*/);
        }

        internal virtual bool OnHandleReference(XmlWriterDelegator xmlWriter, object obj, bool canContainCyclicReference)
        {
            if (xmlWriter.depth < depthToCheckCyclicReference)
                return false;
            if (canContainCyclicReference)
            {
                if (byValObjectsInScope.Count == 0 && DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.Trace(TraceEventType.Warning, TraceCode.ObjectWithLargeDepth, SR.GetString(SR.TraceCodeObjectWithLargeDepth));
                }
                if (byValObjectsInScope.Contains(obj))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.CannotSerializeObjectWithCycles, DataContract.GetClrTypeFullName(obj.GetType()))));
                byValObjectsInScope.Push(obj);
            }
            return false;
        }

        internal virtual void OnEndHandleReference(XmlWriterDelegator xmlWriter, object obj, bool canContainCyclicReference)
        {
            if (xmlWriter.depth < depthToCheckCyclicReference)
                return;
            if (canContainCyclicReference)
            {
                byValObjectsInScope.Pop(obj);
            }
        }

        public void WriteNull(XmlWriterDelegator xmlWriter, Type memberType, bool isMemberTypeSerializable)
        {
            CheckIfTypeSerializable(memberType, isMemberTypeSerializable);
            WriteNull(xmlWriter);
        }

        internal void WriteNull(XmlWriterDelegator xmlWriter, Type memberType, bool isMemberTypeSerializable, XmlDictionaryString name, XmlDictionaryString ns)
        {
            xmlWriter.WriteStartElement(name, ns);
            WriteNull(xmlWriter, memberType, isMemberTypeSerializable);
            xmlWriter.WriteEndElement();
        }

        public void IncrementArrayCount(XmlWriterDelegator xmlWriter, Array array)
        {
            IncrementCollectionCount(xmlWriter, array.GetLength(0));
        }

        public void IncrementCollectionCount(XmlWriterDelegator xmlWriter, ICollection collection)
        {
            IncrementCollectionCount(xmlWriter, collection.Count);
        }

        public void IncrementCollectionCountGeneric<T>(XmlWriterDelegator xmlWriter, ICollection<T> collection)
        {
            IncrementCollectionCount(xmlWriter, collection.Count);
        }

        void IncrementCollectionCount(XmlWriterDelegator xmlWriter, int size)
        {
            IncrementItemCount(size);
            WriteArraySize(xmlWriter, size);
        }

        internal virtual void WriteArraySize(XmlWriterDelegator xmlWriter, int size)
        {
        }

        public static T GetDefaultValue<T>()
        {
            return default(T);
        }

        public static T GetNullableValue<T>(Nullable<T> value) where T : struct
        {
            // value.Value will throw if hasValue is false
            return value.Value;
        }

        public static void ThrowRequiredMemberMustBeEmitted(string memberName, Type type)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.RequiredMemberMustBeEmitted, memberName, type.FullName)));
        }

        public static bool GetHasValue<T>(Nullable<T> value) where T : struct
        {
            return value.HasValue;
        }

        internal void WriteIXmlSerializable(XmlWriterDelegator xmlWriter, object obj)
        {
            if (xmlSerializableWriter == null)
                xmlSerializableWriter = new XmlSerializableWriter();
            WriteIXmlSerializable(xmlWriter, obj, xmlSerializableWriter);
        }

        internal static void WriteRootIXmlSerializable(XmlWriterDelegator xmlWriter, object obj)
        {
            WriteIXmlSerializable(xmlWriter, obj, new XmlSerializableWriter());
        }

        static void WriteIXmlSerializable(XmlWriterDelegator xmlWriter, object obj, XmlSerializableWriter xmlSerializableWriter)
        {
            xmlSerializableWriter.BeginWrite(xmlWriter.Writer, obj);
            IXmlSerializable xmlSerializable = obj as IXmlSerializable;
            if (xmlSerializable != null)
                xmlSerializable.WriteXml(xmlSerializableWriter);
            else
            {
                XmlElement xmlElement = obj as XmlElement;
                if (xmlElement != null)
                    xmlElement.WriteTo(xmlSerializableWriter);
                else
                {
                    XmlNode[] xmlNodes = obj as XmlNode[];
                    if (xmlNodes != null)
                        foreach (XmlNode xmlNode in xmlNodes)
                            xmlNode.WriteTo(xmlSerializableWriter);
                    else
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.UnknownXmlType, DataContract.GetClrTypeFullName(obj.GetType()))));
                }
            }
            xmlSerializableWriter.EndWrite();
        }

        [Fx.Tag.SecurityNote(Critical = "Calls the critical methods of ISerializable", 
            Safe = "Demanding Serialization formatter permission is enough.")]
        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void GetObjectData(ISerializable obj, SerializationInfo serInfo, StreamingContext context)
        {
            // Demand the serialization formatter permission every time
            Globals.SerializationFormatterPermission.Demand();
            obj.GetObjectData(serInfo, context);
        }

        public void WriteISerializable(XmlWriterDelegator xmlWriter, ISerializable obj)
        {
            Type objType = obj.GetType();
            SerializationInfo serInfo = new SerializationInfo(objType, XmlObjectSerializer.FormatterConverter, !this.UnsafeTypeForwardingEnabled);
            GetObjectData(obj, serInfo, GetStreamingContext());

            if (!this.UnsafeTypeForwardingEnabled && serInfo.AssemblyName == Globals.MscorlibAssemblyName)
            {
                // Throw if a malicious type tries to set its assembly name to "0" to get deserialized in mscorlib
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ISerializableAssemblyNameSetToZero, DataContract.GetClrTypeFullName(obj.GetType()))));
            }

            WriteSerializationInfo(xmlWriter, objType, serInfo);
        }

        internal void WriteSerializationInfo(XmlWriterDelegator xmlWriter, Type objType, SerializationInfo serInfo)
        {
            if (DataContract.GetClrTypeFullName(objType) != serInfo.FullTypeName)
            {
                if (DataContractResolver != null)
                {
                    XmlDictionaryString typeName, typeNs;
                    if (ResolveType(serInfo.ObjectType, objType, out typeName, out typeNs))
                    {
                        xmlWriter.WriteAttributeQualifiedName(Globals.SerPrefix, DictionaryGlobals.ISerializableFactoryTypeLocalName, DictionaryGlobals.SerializationNamespace, typeName, typeNs);
                    }
                }
                else
                {
                    string typeName, typeNs;
                    DataContract.GetDefaultStableName(serInfo.FullTypeName, out typeName, out typeNs);
                    xmlWriter.WriteAttributeQualifiedName(Globals.SerPrefix, DictionaryGlobals.ISerializableFactoryTypeLocalName, DictionaryGlobals.SerializationNamespace, DataContract.GetClrTypeString(typeName), DataContract.GetClrTypeString(typeNs));
                }
            }

            WriteClrTypeInfo(xmlWriter, objType, serInfo);
            IncrementItemCount(serInfo.MemberCount);
            foreach (SerializationEntry serEntry in serInfo)
            {
                XmlDictionaryString name = DataContract.GetClrTypeString(DataContract.EncodeLocalName(serEntry.Name));
                xmlWriter.WriteStartElement(name, DictionaryGlobals.EmptyString);
                object obj = serEntry.Value;
                if (obj == null)
                    WriteNull(xmlWriter);
                else
                    InternalSerializeReference(xmlWriter, obj, false /*isDeclaredType*/, false /*writeXsiType*/, -1, Globals.TypeOfObject.TypeHandle);
                xmlWriter.WriteEndElement();
            }
        }

        public void WriteExtensionData(XmlWriterDelegator xmlWriter, ExtensionDataObject extensionData, int memberIndex)
        {
            if (IgnoreExtensionDataObject || extensionData == null)
                return;

            IList<ExtensionDataMember> members = extensionData.Members;
            if (members != null)
            {
                for (int i = 0; i < extensionData.Members.Count; i++)
                {
                    ExtensionDataMember member = extensionData.Members[i];
                    if (member.MemberIndex == memberIndex)
                    {
                        WriteExtensionDataMember(xmlWriter, member);
                    }
                }
            }
        }

        void WriteExtensionDataMember(XmlWriterDelegator xmlWriter, ExtensionDataMember member)
        {
            xmlWriter.WriteStartElement(member.Name, member.Namespace);
            IDataNode dataNode = member.Value;
            WriteExtensionDataValue(xmlWriter, dataNode);
            xmlWriter.WriteEndElement();
        }

        internal virtual void WriteExtensionDataTypeInfo(XmlWriterDelegator xmlWriter, IDataNode dataNode)
        {
            if (dataNode.DataContractName != null)
                WriteTypeInfo(xmlWriter, dataNode.DataContractName, dataNode.DataContractNamespace);

            WriteClrTypeInfo(xmlWriter, dataNode.DataType, dataNode.ClrTypeName, dataNode.ClrAssemblyName);
        }

        internal void WriteExtensionDataValue(XmlWriterDelegator xmlWriter, IDataNode dataNode)
        {
            IncrementItemCount(1);
            if (dataNode == null)
            {
                WriteNull(xmlWriter);
                return;
            }

            if (dataNode.PreservesReferences
                && OnHandleReference(xmlWriter, (dataNode.Value == null ? dataNode : dataNode.Value), true /*canContainCyclicReference*/))
                return;

            Type dataType = dataNode.DataType;
            if (dataType == Globals.TypeOfClassDataNode)
                WriteExtensionClassData(xmlWriter, (ClassDataNode)dataNode);
            else if (dataType == Globals.TypeOfCollectionDataNode)
                WriteExtensionCollectionData(xmlWriter, (CollectionDataNode)dataNode);
            else if (dataType == Globals.TypeOfXmlDataNode)
                WriteExtensionXmlData(xmlWriter, (XmlDataNode)dataNode);
            else if (dataType == Globals.TypeOfISerializableDataNode)
                WriteExtensionISerializableData(xmlWriter, (ISerializableDataNode)dataNode);
            else
            {
                WriteExtensionDataTypeInfo(xmlWriter, dataNode);

                if (dataType == Globals.TypeOfObject)
                {
                    // NOTE: serialize value in DataNode<object> since it may contain non-primitive 
                    // deserialized object (ex. empty class)
                    object o = dataNode.Value;
                    if (o != null)
                        InternalSerialize(xmlWriter, o, false /*isDeclaredType*/, false /*writeXsiType*/, -1, o.GetType().TypeHandle);
                }
                else
                    xmlWriter.WriteExtensionData(dataNode);
            }
            if (dataNode.PreservesReferences)
                OnEndHandleReference(xmlWriter, (dataNode.Value == null ? dataNode : dataNode.Value), true  /*canContainCyclicReference*/);
        }

        internal bool TryWriteDeserializedExtensionData(XmlWriterDelegator xmlWriter, IDataNode dataNode)
        {
            object o = dataNode.Value;
            if (o == null)
                return false;

            Type declaredType = (dataNode.DataContractName == null) ? o.GetType() : Globals.TypeOfObject;
            InternalSerialize(xmlWriter, o, false /*isDeclaredType*/, false /*writeXsiType*/, -1, declaredType.TypeHandle);
            return true;
        }

        void WriteExtensionClassData(XmlWriterDelegator xmlWriter, ClassDataNode dataNode)
        {
            if (!TryWriteDeserializedExtensionData(xmlWriter, dataNode))
            {
                WriteExtensionDataTypeInfo(xmlWriter, dataNode);

                IList<ExtensionDataMember> members = dataNode.Members;
                if (members != null)
                {
                    for (int i = 0; i < members.Count; i++)
                    {
                        WriteExtensionDataMember(xmlWriter, members[i]);
                    }
                }
            }
        }

        void WriteExtensionCollectionData(XmlWriterDelegator xmlWriter, CollectionDataNode dataNode)
        {
            if (!TryWriteDeserializedExtensionData(xmlWriter, dataNode))
            {
                WriteExtensionDataTypeInfo(xmlWriter, dataNode);

                WriteArraySize(xmlWriter, dataNode.Size);

                IList<IDataNode> items = dataNode.Items;
                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        xmlWriter.WriteStartElement(dataNode.ItemName, dataNode.ItemNamespace);
                        WriteExtensionDataValue(xmlWriter, items[i]);
                        xmlWriter.WriteEndElement();
                    }
                }
            }
        }

        void WriteExtensionISerializableData(XmlWriterDelegator xmlWriter, ISerializableDataNode dataNode)
        {
            if (!TryWriteDeserializedExtensionData(xmlWriter, dataNode))
            {
                WriteExtensionDataTypeInfo(xmlWriter, dataNode);

                if (dataNode.FactoryTypeName != null)
                    xmlWriter.WriteAttributeQualifiedName(Globals.SerPrefix, DictionaryGlobals.ISerializableFactoryTypeLocalName, DictionaryGlobals.SerializationNamespace, dataNode.FactoryTypeName, dataNode.FactoryTypeNamespace);

                IList<ISerializableDataMember> members = dataNode.Members;
                if (members != null)
                {
                    for (int i = 0; i < members.Count; i++)
                    {
                        ISerializableDataMember member = members[i];
                        xmlWriter.WriteStartElement(member.Name, String.Empty);
                        WriteExtensionDataValue(xmlWriter, member.Value);
                        xmlWriter.WriteEndElement();
                    }
                }
            }
        }

        void WriteExtensionXmlData(XmlWriterDelegator xmlWriter, XmlDataNode dataNode)
        {
            if (!TryWriteDeserializedExtensionData(xmlWriter, dataNode))
            {
                IList<XmlAttribute> xmlAttributes = dataNode.XmlAttributes;
                if (xmlAttributes != null)
                {
                    foreach (XmlAttribute attribute in xmlAttributes)
                        attribute.WriteTo(xmlWriter.Writer);
                }
                WriteExtensionDataTypeInfo(xmlWriter, dataNode);

                IList<XmlNode> xmlChildNodes = dataNode.XmlChildNodes;
                if (xmlChildNodes != null)
                {
                    foreach (XmlNode node in xmlChildNodes)
                        node.WriteTo(xmlWriter.Writer);
                }
            }
        }

        protected virtual void WriteDataContractValue(DataContract dataContract, XmlWriterDelegator xmlWriter, object obj, RuntimeTypeHandle declaredTypeHandle)
        {
            dataContract.WriteXmlValue(xmlWriter, obj, this);
        }

        protected virtual void WriteNull(XmlWriterDelegator xmlWriter)
        {
            XmlObjectSerializer.WriteNull(xmlWriter);
        }

        void WriteResolvedTypeInfo(XmlWriterDelegator writer, Type objectType, Type declaredType)
        {
            XmlDictionaryString typeName, typeNamespace;
            if (ResolveType(objectType, declaredType, out typeName, out typeNamespace))
            {
                WriteTypeInfo(writer, typeName, typeNamespace);
            }
        }

        bool ResolveType(Type objectType, Type declaredType, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            if (!DataContractResolver.TryResolveType(objectType, declaredType, KnownTypeResolver, out typeName, out typeNamespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ResolveTypeReturnedFalse, DataContract.GetClrTypeFullName(DataContractResolver.GetType()), DataContract.GetClrTypeFullName(objectType))));
            }
            if (typeName == null)
            {
                if (typeNamespace == null)
                {
                    return false;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ResolveTypeReturnedNull, DataContract.GetClrTypeFullName(DataContractResolver.GetType()), DataContract.GetClrTypeFullName(objectType))));
                }
            }
            if (typeNamespace == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ResolveTypeReturnedNull, DataContract.GetClrTypeFullName(DataContractResolver.GetType()), DataContract.GetClrTypeFullName(objectType))));
            }
            return true;
        }

        protected virtual bool WriteTypeInfo(XmlWriterDelegator writer, DataContract contract, DataContract declaredContract)
        {
            if (!XmlObjectSerializer.IsContractDeclared(contract, declaredContract))
            {
                if (DataContractResolver == null)
                {
                    WriteTypeInfo(writer, contract.Name, contract.Namespace);
                    return true;
                }
                else
                {
                    WriteResolvedTypeInfo(writer, contract.OriginalUnderlyingType, declaredContract.OriginalUnderlyingType);
                    return false;
                }
            }
            return false;
        }

        protected virtual void WriteTypeInfo(XmlWriterDelegator writer, string dataContractName, string dataContractNamespace)
        {
            writer.WriteAttributeQualifiedName(Globals.XsiPrefix, DictionaryGlobals.XsiTypeLocalName, DictionaryGlobals.SchemaInstanceNamespace, dataContractName, dataContractNamespace);
        }

        protected virtual void WriteTypeInfo(XmlWriterDelegator writer, XmlDictionaryString dataContractName, XmlDictionaryString dataContractNamespace)
        {
            writer.WriteAttributeQualifiedName(Globals.XsiPrefix, DictionaryGlobals.XsiTypeLocalName, DictionaryGlobals.SchemaInstanceNamespace, dataContractName, dataContractNamespace);
        }
    }
}

