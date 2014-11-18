//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;

#if USE_REFEMIT
    public class XmlObjectSerializerReadContextComplexJson : XmlObjectSerializerReadContextComplex
#else
    class XmlObjectSerializerReadContextComplexJson : XmlObjectSerializerReadContextComplex
#endif
    {
        string extensionDataValueType;
        DateTimeFormat dateTimeFormat;
        bool useSimpleDictionaryFormat;

        public XmlObjectSerializerReadContextComplexJson(DataContractJsonSerializer serializer, DataContract rootTypeDataContract)
            : base(serializer, serializer.MaxItemsInObjectGraph,
                new StreamingContext(StreamingContextStates.All),
                serializer.IgnoreExtensionDataObject)
        {
            this.rootTypeDataContract = rootTypeDataContract;
            this.serializerKnownTypeList = serializer.knownTypeList;
            this.dataContractSurrogate = serializer.DataContractSurrogate;
            this.dateTimeFormat = serializer.DateTimeFormat;
            this.useSimpleDictionaryFormat = serializer.UseSimpleDictionaryFormat;
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

        protected override void StartReadExtensionDataValue(XmlReaderDelegator xmlReader)
        {
            extensionDataValueType = xmlReader.GetAttribute(JsonGlobals.typeString);
        }

        protected override IDataNode ReadPrimitiveExtensionDataValue(XmlReaderDelegator xmlReader, string dataContractName, string dataContractNamespace)
        {
            IDataNode dataNode;

            switch (extensionDataValueType)
            {
                case null:
                case JsonGlobals.stringString:
                    dataNode = new DataNode<string>(xmlReader.ReadContentAsString());
                    break;
                case JsonGlobals.booleanString:
                    dataNode = new DataNode<bool>(xmlReader.ReadContentAsBoolean());
                    break;
                case JsonGlobals.numberString:
                    dataNode = ReadNumericalPrimitiveExtensionDataValue(xmlReader);
                    break;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.JsonUnexpectedAttributeValue, extensionDataValueType)));
            }

            xmlReader.ReadEndElement();
            return dataNode;
        }

        IDataNode ReadNumericalPrimitiveExtensionDataValue(XmlReaderDelegator xmlReader)
        {
            TypeCode type;
            object numericalValue = JsonObjectDataContract.ParseJsonNumber(xmlReader.ReadContentAsString(), out type);
            switch (type)
            {
                case TypeCode.Byte:
                    return new DataNode<byte>((byte)numericalValue);
                case TypeCode.SByte:
                    return new DataNode<sbyte>((sbyte)numericalValue);
                case TypeCode.Int16:
                    return new DataNode<short>((short)numericalValue);
                case TypeCode.Int32:
                    return new DataNode<int>((int)numericalValue);
                case TypeCode.Int64:
                    return new DataNode<long>((long)numericalValue);
                case TypeCode.UInt16:
                    return new DataNode<ushort>((ushort)numericalValue);
                case TypeCode.UInt32:
                    return new DataNode<uint>((uint)numericalValue);
                case TypeCode.UInt64:
                    return new DataNode<ulong>((ulong)numericalValue);
                case TypeCode.Single:
                    return new DataNode<float>((float)numericalValue);
                case TypeCode.Double:
                    return new DataNode<double>((double)numericalValue);
                case TypeCode.Decimal:
                    return new DataNode<decimal>((decimal)numericalValue);
                default:
                    throw Fx.AssertAndThrow("JsonObjectDataContract.ParseJsonNumber shouldn't return a TypeCode that we're not expecting");
            }
        }

        internal static XmlObjectSerializerReadContextComplexJson CreateContext(DataContractJsonSerializer serializer, DataContract rootTypeDataContract)
        {
            return new XmlObjectSerializerReadContextComplexJson(serializer, rootTypeDataContract);
        }

#if USE_REFEMIT
        public override int GetArraySize()
#else
        internal override int GetArraySize()
#endif
        {
            return -1;
        }

        protected override object ReadDataContractValue(DataContract dataContract, XmlReaderDelegator reader)
        {
            return DataContractJsonSerializer.ReadJsonValue(dataContract, reader, this);
        }

#if USE_REFEMIT
        public override void ReadAttributes(XmlReaderDelegator xmlReader)
#else
        internal override void ReadAttributes(XmlReaderDelegator xmlReader)
#endif
        {
            if (attributes == null)
                attributes = new Attributes();
            attributes.Reset();

            if (xmlReader.MoveToAttribute(JsonGlobals.typeString) && xmlReader.Value == JsonGlobals.nullString)
            {
                attributes.XsiNil = true;
            }
            else if (xmlReader.MoveToAttribute(JsonGlobals.serverTypeString))
            {
                XmlQualifiedName qualifiedTypeName = JsonReaderDelegator.ParseQualifiedName(xmlReader.Value);
                attributes.XsiTypeName = qualifiedTypeName.Name;

                string serverTypeNamespace = qualifiedTypeName.Namespace;

                if (!string.IsNullOrEmpty(serverTypeNamespace))
                {
                    switch (serverTypeNamespace[0])
                    {
                        case '#':
                            serverTypeNamespace = string.Concat(Globals.DataContractXsdBaseNamespace, serverTypeNamespace.Substring(1));
                            break;
                        case '\\':
                            if (serverTypeNamespace.Length >= 2)
                            {
                                switch (serverTypeNamespace[1])
                                {
                                    case '#':
                                    case '\\':
                                        serverTypeNamespace = serverTypeNamespace.Substring(1);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                attributes.XsiTypeNamespace = serverTypeNamespace;
            }
            xmlReader.MoveToElement();
        }

        public int GetJsonMemberIndex(XmlReaderDelegator xmlReader, XmlDictionaryString[] memberNames, int memberIndex, ExtensionDataObject extensionData)
        {
            int length = memberNames.Length;
            if (length != 0)
            {
                for (int i = 0, index = (memberIndex + 1) % length; i < length; i++, index = (index + 1) % length)
                {
                    if (xmlReader.IsStartElement(memberNames[index], XmlDictionaryString.Empty))
                    {
                        return index;
                    }
                }
                string name;
                if (TryGetJsonLocalName(xmlReader, out name))
                {
                    for (int i = 0, index = (memberIndex + 1) % length; i < length; i++, index = (index + 1) % length)
                    {
                        if (memberNames[index].Value == name)
                        {
                            return index;
                        }
                    }
                }
            }
            HandleMemberNotFound(xmlReader, extensionData, memberIndex);
            return length;
        }

        internal static bool TryGetJsonLocalName(XmlReaderDelegator xmlReader, out string name)
        {
            if (xmlReader.IsStartElement(JsonGlobals.itemDictionaryString, JsonGlobals.itemDictionaryString))
            {
                if (xmlReader.MoveToAttribute(JsonGlobals.itemString))
                {
                    name = xmlReader.Value;
                    return true;
                }
            }
            name = null;
            return false;
        }

        public static string GetJsonMemberName(XmlReaderDelegator xmlReader)
        {
            string name;
            if (!TryGetJsonLocalName(xmlReader, out name))
            {
                name = xmlReader.LocalName;
            }
            return name;
        }

        public static void ThrowMissingRequiredMembers(object obj, XmlDictionaryString[] memberNames, byte[] expectedElements, byte[] requiredElements)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int missingMembersCount = 0;
            for (int i = 0; i < memberNames.Length; i++)
            {
                if (IsBitSet(expectedElements, i) && IsBitSet(requiredElements, i))
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(memberNames[i]);
                    missingMembersCount++;
                }
            }

            if (missingMembersCount == 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(
                 SR.JsonOneRequiredMemberNotFound, DataContract.GetClrTypeFullName(obj.GetType()), stringBuilder.ToString())));
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(
                    SR.JsonRequiredMembersNotFound, DataContract.GetClrTypeFullName(obj.GetType()), stringBuilder.ToString())));
            }
        }



        public static void ThrowDuplicateMemberException(object obj, XmlDictionaryString[] memberNames, int memberIndex)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(
                SR.GetString(SR.JsonDuplicateMemberInInput, DataContract.GetClrTypeFullName(obj.GetType()), memberNames[memberIndex])));
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'BitFlagsGenerator'.",
            Safe = "This method is safe to call.")]
        [SecuritySafeCritical]
        static bool IsBitSet(byte[] bytes, int bitIndex)
        {
            return BitFlagsGenerator.IsBitSet(bytes, bitIndex);
        }

        protected override bool IsReadingCollectionExtensionData(XmlReaderDelegator xmlReader)
        {
            return xmlReader.GetAttribute(JsonGlobals.typeString) == JsonGlobals.arrayString;
        }

        protected override bool IsReadingClassExtensionData(XmlReaderDelegator xmlReader)
        {
            return xmlReader.GetAttribute(JsonGlobals.typeString) == JsonGlobals.objectString;
        }

        protected override XmlReaderDelegator CreateReaderDelegatorForReader(XmlReader xmlReader)
        {
            return new JsonReaderDelegator(xmlReader, this.dateTimeFormat);
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

        protected override DataContract ResolveDataContractFromRootDataContract(XmlQualifiedName typeQName)
        {
            return XmlObjectSerializerWriteContextComplexJson.ResolveJsonDataContractFromRootDataContract(this, typeQName, rootTypeDataContract);
        }
    }
}
