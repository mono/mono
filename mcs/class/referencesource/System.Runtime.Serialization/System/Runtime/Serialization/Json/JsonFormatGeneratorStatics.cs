//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Serialization.Json;
    using System.Security;
    using System.Xml;

    [Fx.Tag.SecurityNote(Critical = "Class holds static instances used for code generation during serialization."
        + "Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.",
        Safe = "All get-only properties marked safe since they only need to be protected for write.")]
#if USE_REFEMIT
    public static class JsonFormatGeneratorStatics
#else
    internal static class JsonFormatGeneratorStatics
#endif
    {

        [SecurityCritical]
        static MethodInfo boxPointer;
        [SecurityCritical]
        static PropertyInfo collectionItemNameProperty;

        [SecurityCritical]
        static ConstructorInfo extensionDataObjectCtor;

        [SecurityCritical]
        static PropertyInfo extensionDataProperty;

        [SecurityCritical]
        static MethodInfo getItemContractMethod;

        [SecurityCritical]
        static MethodInfo getJsonDataContractMethod;

        [SecurityCritical]
        static MethodInfo getJsonMemberIndexMethod;

        [SecurityCritical]
        static MethodInfo getRevisedItemContractMethod;

        [SecurityCritical]
        static MethodInfo getUninitializedObjectMethod;

        [SecurityCritical]
        static MethodInfo ienumeratorGetCurrentMethod;

        [SecurityCritical]
        static MethodInfo ienumeratorMoveNextMethod;

        [SecurityCritical]
        static MethodInfo isStartElementMethod0;

        [SecurityCritical]
        static MethodInfo isStartElementMethod2;

        [SecurityCritical]
        static PropertyInfo localNameProperty;

        [SecurityCritical]
        static PropertyInfo namespaceProperty;

        [SecurityCritical]
        static MethodInfo moveToContentMethod;

        [SecurityCritical]
        static PropertyInfo nodeTypeProperty;

        [SecurityCritical]
        static MethodInfo onDeserializationMethod;

        [SecurityCritical]
        static MethodInfo readJsonValueMethod;

        [SecurityCritical]
        static ConstructorInfo serializationExceptionCtor;

        [SecurityCritical]
        static Type[] serInfoCtorArgs;

        [SecurityCritical]
        static MethodInfo throwDuplicateMemberExceptionMethod;

        [SecurityCritical]
        static MethodInfo throwMissingRequiredMembersMethod;

        [SecurityCritical]
        static PropertyInfo typeHandleProperty;

        [SecurityCritical]
        static MethodInfo unboxPointer;

        [SecurityCritical]
        static PropertyInfo useSimpleDictionaryFormatReadProperty;

        [SecurityCritical]
        static PropertyInfo useSimpleDictionaryFormatWriteProperty;

        [SecurityCritical]
        static MethodInfo writeAttributeStringMethod;

        [SecurityCritical]
        static MethodInfo writeEndElementMethod;

        [SecurityCritical]
        static MethodInfo writeJsonISerializableMethod;

        [SecurityCritical]
        static MethodInfo writeJsonNameWithMappingMethod;

        [SecurityCritical]
        static MethodInfo writeJsonValueMethod;

        [SecurityCritical]
        static MethodInfo writeStartElementMethod;

        [SecurityCritical]
        static MethodInfo writeStartElementStringMethod;

        [SecurityCritical]
        static MethodInfo parseEnumMethod;

        [SecurityCritical]
        static MethodInfo getJsonMemberNameMethod;

        public static MethodInfo BoxPointer
        {
            [SecuritySafeCritical]
            get
            {
                if (boxPointer == null)
                {
                    boxPointer = typeof(Pointer).GetMethod("Box");
                }
                return boxPointer;
            }
        }
        public static PropertyInfo CollectionItemNameProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (collectionItemNameProperty == null)
                {
                    collectionItemNameProperty = typeof(XmlObjectSerializerWriteContextComplexJson).GetProperty("CollectionItemName", Globals.ScanAllMembers);
                }
                return collectionItemNameProperty;
            }
        }
        public static ConstructorInfo ExtensionDataObjectCtor
        {
            [SecuritySafeCritical]
            get
            {
                if (extensionDataObjectCtor == null)
                {
                    extensionDataObjectCtor = typeof(ExtensionDataObject).GetConstructor(Globals.ScanAllMembers, null, new Type[] { }, null);
                }
                return extensionDataObjectCtor;
            }
        }
        public static PropertyInfo ExtensionDataProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (extensionDataProperty == null)
                {
                    extensionDataProperty = typeof(IExtensibleDataObject).GetProperty("ExtensionData");
                }
                return extensionDataProperty;
            }
        }
        public static MethodInfo GetCurrentMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (ienumeratorGetCurrentMethod == null)
                {
                    ienumeratorGetCurrentMethod = typeof(IEnumerator).GetProperty("Current").GetGetMethod();
                }
                return ienumeratorGetCurrentMethod;
            }
        }
        public static MethodInfo GetItemContractMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getItemContractMethod == null)
                {
                    getItemContractMethod = typeof(CollectionDataContract).GetProperty("ItemContract", Globals.ScanAllMembers).GetGetMethod(true); // nonPublic
                }
                return getItemContractMethod;
            }
        }
        public static MethodInfo GetJsonDataContractMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getJsonDataContractMethod == null)
                {
                    getJsonDataContractMethod = typeof(JsonDataContract).GetMethod("GetJsonDataContract", Globals.ScanAllMembers);
                }
                return getJsonDataContractMethod;
            }
        }
        public static MethodInfo GetJsonMemberIndexMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getJsonMemberIndexMethod == null)
                {
                    getJsonMemberIndexMethod = typeof(XmlObjectSerializerReadContextComplexJson).GetMethod("GetJsonMemberIndex", Globals.ScanAllMembers);
                }
                return getJsonMemberIndexMethod;
            }
        }
        public static MethodInfo GetRevisedItemContractMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getRevisedItemContractMethod == null)
                {
                    getRevisedItemContractMethod = typeof(XmlObjectSerializerWriteContextComplexJson).GetMethod("GetRevisedItemContract", Globals.ScanAllMembers);
                }
                return getRevisedItemContractMethod;
            }
        }
        public static MethodInfo GetUninitializedObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getUninitializedObjectMethod == null)
                {
                    getUninitializedObjectMethod = typeof(XmlFormatReaderGenerator).GetMethod("UnsafeGetUninitializedObject", Globals.ScanAllMembers, null, new Type[] { typeof(int) }, null);
                }
                return getUninitializedObjectMethod;
            }
        }
        public static MethodInfo IsStartElementMethod0
        {
            [SecuritySafeCritical]
            get
            {
                if (isStartElementMethod0 == null)
                {
                    isStartElementMethod0 = typeof(XmlReaderDelegator).GetMethod("IsStartElement", Globals.ScanAllMembers, null, new Type[] { }, null);
                }
                return isStartElementMethod0;
            }
        }
        public static MethodInfo IsStartElementMethod2
        {
            [SecuritySafeCritical]
            get
            {
                if (isStartElementMethod2 == null)
                {
                    isStartElementMethod2 = typeof(XmlReaderDelegator).GetMethod("IsStartElement", Globals.ScanAllMembers, null, new Type[] { typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null);
                }
                return isStartElementMethod2;
            }
        }
        public static PropertyInfo LocalNameProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (localNameProperty == null)
                {
                    localNameProperty = typeof(XmlReaderDelegator).GetProperty("LocalName", Globals.ScanAllMembers);
                }
                return localNameProperty;
            }
        }
        public static PropertyInfo NamespaceProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (namespaceProperty == null)
                {
                    namespaceProperty = typeof(XmlReaderDelegator).GetProperty("NamespaceProperty", Globals.ScanAllMembers);
                }
                return namespaceProperty;
            }
        }
        public static MethodInfo MoveNextMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (ienumeratorMoveNextMethod == null)
                {
                    ienumeratorMoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
                }
                return ienumeratorMoveNextMethod;
            }
        }
        public static MethodInfo MoveToContentMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (moveToContentMethod == null)
                {
                    moveToContentMethod = typeof(XmlReaderDelegator).GetMethod("MoveToContent", Globals.ScanAllMembers);
                }
                return moveToContentMethod;
            }
        }
        public static PropertyInfo NodeTypeProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (nodeTypeProperty == null)
                {
                    nodeTypeProperty = typeof(XmlReaderDelegator).GetProperty("NodeType", Globals.ScanAllMembers);
                }
                return nodeTypeProperty;
            }
        }
        public static MethodInfo OnDeserializationMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (onDeserializationMethod == null)
                {
                    onDeserializationMethod = typeof(IDeserializationCallback).GetMethod("OnDeserialization");
                }
                return onDeserializationMethod;
            }
        }
        public static MethodInfo ReadJsonValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readJsonValueMethod == null)
                {
                    readJsonValueMethod = typeof(DataContractJsonSerializer).GetMethod("ReadJsonValue", Globals.ScanAllMembers);
                }
                return readJsonValueMethod;
            }
        }
        public static ConstructorInfo SerializationExceptionCtor
        {
            [SecuritySafeCritical]
            get
            {
                if (serializationExceptionCtor == null)
                {
                    serializationExceptionCtor = typeof(SerializationException).GetConstructor(new Type[] { typeof(string) });
                }
                return serializationExceptionCtor;
            }
        }
        public static Type[] SerInfoCtorArgs
        {
            [SecuritySafeCritical]
            get
            {
                if (serInfoCtorArgs == null)
                {
                    serInfoCtorArgs = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
                }
                return serInfoCtorArgs;
            }
        }
        public static MethodInfo ThrowDuplicateMemberExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwDuplicateMemberExceptionMethod == null)
                {
                    throwDuplicateMemberExceptionMethod = typeof(XmlObjectSerializerReadContextComplexJson).GetMethod("ThrowDuplicateMemberException", Globals.ScanAllMembers);
                }
                return throwDuplicateMemberExceptionMethod;
            }
        }
        public static MethodInfo ThrowMissingRequiredMembersMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwMissingRequiredMembersMethod == null)
                {
                    throwMissingRequiredMembersMethod = typeof(XmlObjectSerializerReadContextComplexJson).GetMethod("ThrowMissingRequiredMembers", Globals.ScanAllMembers);
                }
                return throwMissingRequiredMembersMethod;
            }
        }
        public static PropertyInfo TypeHandleProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (typeHandleProperty == null)
                {
                    typeHandleProperty = typeof(Type).GetProperty("TypeHandle");
                }
                return typeHandleProperty;
            }
        }
        public static MethodInfo UnboxPointer
        {
            [SecuritySafeCritical]
            get
            {
                if (unboxPointer == null)
                {
                    unboxPointer = typeof(Pointer).GetMethod("Unbox");
                }
                return unboxPointer;
            }
        }
        public static PropertyInfo UseSimpleDictionaryFormatReadProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (useSimpleDictionaryFormatReadProperty == null)
                {
                    useSimpleDictionaryFormatReadProperty = typeof(XmlObjectSerializerReadContextComplexJson).GetProperty("UseSimpleDictionaryFormat", Globals.ScanAllMembers);
                }
                return useSimpleDictionaryFormatReadProperty;
            }
        }
        public static PropertyInfo UseSimpleDictionaryFormatWriteProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (useSimpleDictionaryFormatWriteProperty == null)
                {
                    useSimpleDictionaryFormatWriteProperty = typeof(XmlObjectSerializerWriteContextComplexJson).GetProperty("UseSimpleDictionaryFormat", Globals.ScanAllMembers);
                }
                return useSimpleDictionaryFormatWriteProperty;
            }
        }
        public static MethodInfo WriteAttributeStringMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeAttributeStringMethod == null)
                {
                    writeAttributeStringMethod = typeof(XmlWriterDelegator).GetMethod("WriteAttributeString", Globals.ScanAllMembers, null, new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) }, null);
                }
                return writeAttributeStringMethod;
            }
        }
        public static MethodInfo WriteEndElementMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeEndElementMethod == null)
                {
                    writeEndElementMethod = typeof(XmlWriterDelegator).GetMethod("WriteEndElement", Globals.ScanAllMembers, null, new Type[] { }, null);
                }
                return writeEndElementMethod;
            }
        }
        public static MethodInfo WriteJsonISerializableMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeJsonISerializableMethod == null)
                {
                    writeJsonISerializableMethod = typeof(XmlObjectSerializerWriteContextComplexJson).GetMethod("WriteJsonISerializable", Globals.ScanAllMembers);
                }
                return writeJsonISerializableMethod;
            }
        }
        public static MethodInfo WriteJsonNameWithMappingMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeJsonNameWithMappingMethod == null)
                {
                    writeJsonNameWithMappingMethod = typeof(XmlObjectSerializerWriteContextComplexJson).GetMethod("WriteJsonNameWithMapping", Globals.ScanAllMembers);
                }
                return writeJsonNameWithMappingMethod;
            }
        }
        public static MethodInfo WriteJsonValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeJsonValueMethod == null)
                {
                    writeJsonValueMethod = typeof(DataContractJsonSerializer).GetMethod("WriteJsonValue", Globals.ScanAllMembers);
                }
                return writeJsonValueMethod;
            }
        }
        public static MethodInfo WriteStartElementMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeStartElementMethod == null)
                {
                    writeStartElementMethod = typeof(XmlWriterDelegator).GetMethod("WriteStartElement", Globals.ScanAllMembers, null, new Type[] { typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null);
                }
                return writeStartElementMethod;
            }
        }

        public static MethodInfo WriteStartElementStringMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeStartElementStringMethod == null)
                {
                    writeStartElementStringMethod = typeof(XmlWriterDelegator).GetMethod("WriteStartElement", Globals.ScanAllMembers, null, new Type[] { typeof(string), typeof(string) }, null);
                }
                return writeStartElementStringMethod;
            }
        }

        public static MethodInfo ParseEnumMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (parseEnumMethod == null)
                {
                    parseEnumMethod = typeof(Enum).GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Type), typeof(string) }, null);
                }
                return parseEnumMethod;
            }
        }

        public static MethodInfo GetJsonMemberNameMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getJsonMemberNameMethod == null)
                {
                    getJsonMemberNameMethod = typeof(XmlObjectSerializerReadContextComplexJson).GetMethod("GetJsonMemberName", Globals.ScanAllMembers, null, new Type[] { typeof(XmlReaderDelegator) }, null);
                }
                return getJsonMemberNameMethod;
            }
        }
    }
}
