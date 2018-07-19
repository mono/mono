//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security;
    using System.Xml;

    [Fx.Tag.SecurityNote(Critical = "Class holds static instances used for code generation during serialization."
        + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.",
        Safe = "All get-only properties marked safe since they only need to be protected for write.")]
    static class XmlFormatGeneratorStatics
    {
        [SecurityCritical]
        static MethodInfo writeStartElementMethod2;
        internal static MethodInfo WriteStartElementMethod2
        {
            [SecuritySafeCritical]
            get
            {
                if (writeStartElementMethod2 == null)
                    writeStartElementMethod2 = typeof(XmlWriterDelegator).GetMethod("WriteStartElement", Globals.ScanAllMembers, null, new Type[] { typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null);
                return writeStartElementMethod2;
            }
        }

        [SecurityCritical]
        static MethodInfo writeStartElementMethod3;
        internal static MethodInfo WriteStartElementMethod3
        {
            [SecuritySafeCritical]
            get
            {
                if (writeStartElementMethod3 == null)
                    writeStartElementMethod3 = typeof(XmlWriterDelegator).GetMethod("WriteStartElement", Globals.ScanAllMembers, null, new Type[] { typeof(string), typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null);
                return writeStartElementMethod3;
            }
        }

        [SecurityCritical]
        static MethodInfo writeEndElementMethod;
        internal static MethodInfo WriteEndElementMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeEndElementMethod == null)
                    writeEndElementMethod = typeof(XmlWriterDelegator).GetMethod("WriteEndElement", Globals.ScanAllMembers, null, new Type[] { }, null);
                return writeEndElementMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo writeNamespaceDeclMethod;
        internal static MethodInfo WriteNamespaceDeclMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeNamespaceDeclMethod == null)
                    writeNamespaceDeclMethod = typeof(XmlWriterDelegator).GetMethod("WriteNamespaceDecl", Globals.ScanAllMembers, null, new Type[] { typeof(XmlDictionaryString) }, null);
                return writeNamespaceDeclMethod;
            }
        }

        [SecurityCritical]
        static PropertyInfo extensionDataProperty;
        internal static PropertyInfo ExtensionDataProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (extensionDataProperty == null)
                    extensionDataProperty = typeof(IExtensibleDataObject).GetProperty("ExtensionData");
                return extensionDataProperty;
            }
        }

        [SecurityCritical]
        static MethodInfo boxPointer;
        internal static MethodInfo BoxPointer
        {
            [SecuritySafeCritical]
            get
            {
                if (boxPointer == null)
                    boxPointer = typeof(Pointer).GetMethod("Box");
                return boxPointer;
            }
        }

        [SecurityCritical]
        static ConstructorInfo dictionaryEnumeratorCtor;
        internal static ConstructorInfo DictionaryEnumeratorCtor
        {
            [SecuritySafeCritical]
            get
            {
                if (dictionaryEnumeratorCtor == null)
                    dictionaryEnumeratorCtor = Globals.TypeOfDictionaryEnumerator.GetConstructor(Globals.ScanAllMembers, null, new Type[] { Globals.TypeOfIDictionaryEnumerator }, null);
                return dictionaryEnumeratorCtor;
            }
        }

        [SecurityCritical]
        static MethodInfo ienumeratorMoveNextMethod;
        internal static MethodInfo MoveNextMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (ienumeratorMoveNextMethod == null)
                    ienumeratorMoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
                return ienumeratorMoveNextMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo ienumeratorGetCurrentMethod;
        internal static MethodInfo GetCurrentMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (ienumeratorGetCurrentMethod == null)
                    ienumeratorGetCurrentMethod = typeof(IEnumerator).GetProperty("Current").GetGetMethod();
                return ienumeratorGetCurrentMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getItemContractMethod;
        internal static MethodInfo GetItemContractMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getItemContractMethod == null)
                    getItemContractMethod = typeof(CollectionDataContract).GetProperty("ItemContract", Globals.ScanAllMembers).GetGetMethod(true/*nonPublic*/);
                return getItemContractMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo isStartElementMethod2;
        internal static MethodInfo IsStartElementMethod2
        {
            [SecuritySafeCritical]
            get
            {
                if (isStartElementMethod2 == null)
                    isStartElementMethod2 = typeof(XmlReaderDelegator).GetMethod("IsStartElement", Globals.ScanAllMembers, null, new Type[] { typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null);
                return isStartElementMethod2;
            }
        }

        [SecurityCritical]
        static MethodInfo isStartElementMethod0;
        internal static MethodInfo IsStartElementMethod0
        {
            [SecuritySafeCritical]
            get
            {
                if (isStartElementMethod0 == null)
                    isStartElementMethod0 = typeof(XmlReaderDelegator).GetMethod("IsStartElement", Globals.ScanAllMembers, null, new Type[] { }, null);
                return isStartElementMethod0;
            }
        }

        [SecurityCritical]
        static MethodInfo getUninitializedObjectMethod;
        internal static MethodInfo GetUninitializedObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getUninitializedObjectMethod == null)
                    getUninitializedObjectMethod = typeof(XmlFormatReaderGenerator).GetMethod("UnsafeGetUninitializedObject", Globals.ScanAllMembers, null, new Type[] { typeof(int) }, null);
                return getUninitializedObjectMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo onDeserializationMethod;
        internal static MethodInfo OnDeserializationMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (onDeserializationMethod == null)
                    onDeserializationMethod = typeof(IDeserializationCallback).GetMethod("OnDeserialization");
                return onDeserializationMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo unboxPointer;
        internal static MethodInfo UnboxPointer
        {
            [SecuritySafeCritical]
            get
            {
                if (unboxPointer == null)
                    unboxPointer = typeof(Pointer).GetMethod("Unbox");
                return unboxPointer;
            }
        }

        [SecurityCritical]
        static PropertyInfo nodeTypeProperty;
        internal static PropertyInfo NodeTypeProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (nodeTypeProperty == null)
                    nodeTypeProperty = typeof(XmlReaderDelegator).GetProperty("NodeType", Globals.ScanAllMembers);
                return nodeTypeProperty;
            }
        }

        [SecurityCritical]
        static ConstructorInfo serializationExceptionCtor;
        internal static ConstructorInfo SerializationExceptionCtor
        {
            [SecuritySafeCritical]
            get
            {
                if (serializationExceptionCtor == null)
                    serializationExceptionCtor = typeof(SerializationException).GetConstructor(new Type[] { typeof(string) });
                return serializationExceptionCtor;
            }
        }

        [SecurityCritical]
        static ConstructorInfo extensionDataObjectCtor;
        internal static ConstructorInfo ExtensionDataObjectCtor
        {
            [SecuritySafeCritical]
            get
            {
                if (extensionDataObjectCtor == null)
                    extensionDataObjectCtor = typeof(ExtensionDataObject).GetConstructor(Globals.ScanAllMembers, null, new Type[] { }, null);
                return extensionDataObjectCtor;
            }
        }

        [SecurityCritical]
        static ConstructorInfo hashtableCtor;
        internal static ConstructorInfo HashtableCtor
        {
            [SecuritySafeCritical]
            get
            {
                if (hashtableCtor == null)
                    hashtableCtor = Globals.TypeOfHashtable.GetConstructor(Globals.ScanAllMembers, null, Globals.EmptyTypeArray, null);
                return hashtableCtor;
            }
        }

        [SecurityCritical]
        static MethodInfo getStreamingContextMethod;
        internal static MethodInfo GetStreamingContextMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getStreamingContextMethod == null)
                    getStreamingContextMethod = typeof(XmlObjectSerializerContext).GetMethod("GetStreamingContext", Globals.ScanAllMembers);
                return getStreamingContextMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getCollectionMemberMethod;
        internal static MethodInfo GetCollectionMemberMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getCollectionMemberMethod == null)
                    getCollectionMemberMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetCollectionMember", Globals.ScanAllMembers);
                return getCollectionMemberMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo storeCollectionMemberInfoMethod;
        internal static MethodInfo StoreCollectionMemberInfoMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (storeCollectionMemberInfoMethod == null)
                    storeCollectionMemberInfoMethod = typeof(XmlObjectSerializerReadContext).GetMethod("StoreCollectionMemberInfo", Globals.ScanAllMembers, null, new Type[] { typeof(object) }, null);
                return storeCollectionMemberInfoMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo storeIsGetOnlyCollectionMethod;
        internal static MethodInfo StoreIsGetOnlyCollectionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (storeIsGetOnlyCollectionMethod == null)
                    storeIsGetOnlyCollectionMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("StoreIsGetOnlyCollection", Globals.ScanAllMembers);
                return storeIsGetOnlyCollectionMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo throwNullValueReturnedForGetOnlyCollectionExceptionMethod;
        internal static MethodInfo ThrowNullValueReturnedForGetOnlyCollectionExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwNullValueReturnedForGetOnlyCollectionExceptionMethod == null)
                    throwNullValueReturnedForGetOnlyCollectionExceptionMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ThrowNullValueReturnedForGetOnlyCollectionException", Globals.ScanAllMembers);
                return throwNullValueReturnedForGetOnlyCollectionExceptionMethod;
            }
        }

        static MethodInfo throwArrayExceededSizeExceptionMethod;
        internal static MethodInfo ThrowArrayExceededSizeExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwArrayExceededSizeExceptionMethod == null)
                    throwArrayExceededSizeExceptionMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ThrowArrayExceededSizeException", Globals.ScanAllMembers);
                return throwArrayExceededSizeExceptionMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo incrementItemCountMethod;
        internal static MethodInfo IncrementItemCountMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (incrementItemCountMethod == null)
                    incrementItemCountMethod = typeof(XmlObjectSerializerContext).GetMethod("IncrementItemCount", Globals.ScanAllMembers);
                return incrementItemCountMethod;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds instance of SecurityPermission that we will Demand for SerializationFormatter."
            + " Should not be modified to something else.")]
        [SecurityCritical]
        static MethodInfo demandSerializationFormatterPermissionMethod;
        internal static MethodInfo DemandSerializationFormatterPermissionMethod
        {
            [Fx.Tag.SecurityNote(Critical = "Demands SerializationFormatter permission. Demanding the right permission is critical.",
                Safe = "No data or control leaks in or out, must be callable from transparent generated IL.")]
            [SecuritySafeCritical]
            get
            {
                if (demandSerializationFormatterPermissionMethod == null)
                    demandSerializationFormatterPermissionMethod = typeof(XmlObjectSerializerContext).GetMethod("DemandSerializationFormatterPermission", Globals.ScanAllMembers);
                return demandSerializationFormatterPermissionMethod;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds instance of SecurityPermission that we will Demand for MemberAccess."
            + " Should not be modified to something else.")]
        [SecurityCritical]
        static MethodInfo demandMemberAccessPermissionMethod;
        internal static MethodInfo DemandMemberAccessPermissionMethod
        {
            [Fx.Tag.SecurityNote(Critical = "Demands MemberAccess permission. Demanding the right permission is critical.",
                Safe = "No data or control leaks in or out, must be callable from transparent generated IL.")]
            [SecuritySafeCritical]
            get
            {
                if (demandMemberAccessPermissionMethod == null)
                    demandMemberAccessPermissionMethod = typeof(XmlObjectSerializerContext).GetMethod("DemandMemberAccessPermission", Globals.ScanAllMembers);
                return demandMemberAccessPermissionMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo internalDeserializeMethod;
        internal static MethodInfo InternalDeserializeMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (internalDeserializeMethod == null)
                    internalDeserializeMethod = typeof(XmlObjectSerializerReadContext).GetMethod("InternalDeserialize", Globals.ScanAllMembers, null, new Type[] { typeof(XmlReaderDelegator), typeof(int), typeof(RuntimeTypeHandle), typeof(string), typeof(string) }, null);
                return internalDeserializeMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo moveToNextElementMethod;
        internal static MethodInfo MoveToNextElementMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (moveToNextElementMethod == null)
                    moveToNextElementMethod = typeof(XmlObjectSerializerReadContext).GetMethod("MoveToNextElement", Globals.ScanAllMembers);
                return moveToNextElementMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getMemberIndexMethod;
        internal static MethodInfo GetMemberIndexMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getMemberIndexMethod == null)
                    getMemberIndexMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetMemberIndex", Globals.ScanAllMembers);
                return getMemberIndexMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getMemberIndexWithRequiredMembersMethod;
        internal static MethodInfo GetMemberIndexWithRequiredMembersMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getMemberIndexWithRequiredMembersMethod == null)
                    getMemberIndexWithRequiredMembersMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetMemberIndexWithRequiredMembers", Globals.ScanAllMembers);
                return getMemberIndexWithRequiredMembersMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo throwRequiredMemberMissingExceptionMethod;
        internal static MethodInfo ThrowRequiredMemberMissingExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwRequiredMemberMissingExceptionMethod == null)
                    throwRequiredMemberMissingExceptionMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ThrowRequiredMemberMissingException", Globals.ScanAllMembers);
                return throwRequiredMemberMissingExceptionMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo skipUnknownElementMethod;
        internal static MethodInfo SkipUnknownElementMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (skipUnknownElementMethod == null)
                    skipUnknownElementMethod = typeof(XmlObjectSerializerReadContext).GetMethod("SkipUnknownElement", Globals.ScanAllMembers);
                return skipUnknownElementMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo readIfNullOrRefMethod;
        internal static MethodInfo ReadIfNullOrRefMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readIfNullOrRefMethod == null)
                    readIfNullOrRefMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ReadIfNullOrRef", Globals.ScanAllMembers, null, new Type[] { typeof(XmlReaderDelegator), typeof(Type), typeof(bool) }, null);
                return readIfNullOrRefMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo readAttributesMethod;
        internal static MethodInfo ReadAttributesMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readAttributesMethod == null)
                    readAttributesMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ReadAttributes", Globals.ScanAllMembers);
                return readAttributesMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo resetAttributesMethod;
        internal static MethodInfo ResetAttributesMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (resetAttributesMethod == null)
                    resetAttributesMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ResetAttributes", Globals.ScanAllMembers);
                return resetAttributesMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getObjectIdMethod;
        internal static MethodInfo GetObjectIdMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getObjectIdMethod == null)
                    getObjectIdMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetObjectId", Globals.ScanAllMembers);
                return getObjectIdMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getArraySizeMethod;
        internal static MethodInfo GetArraySizeMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getArraySizeMethod == null)
                    getArraySizeMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetArraySize", Globals.ScanAllMembers);
                return getArraySizeMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo addNewObjectMethod;
        internal static MethodInfo AddNewObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (addNewObjectMethod == null)
                    addNewObjectMethod = typeof(XmlObjectSerializerReadContext).GetMethod("AddNewObject", Globals.ScanAllMembers);
                return addNewObjectMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo addNewObjectWithIdMethod;
        internal static MethodInfo AddNewObjectWithIdMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (addNewObjectWithIdMethod == null)
                    addNewObjectWithIdMethod = typeof(XmlObjectSerializerReadContext).GetMethod("AddNewObjectWithId", Globals.ScanAllMembers);
                return addNewObjectWithIdMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo replaceDeserializedObjectMethod;
        internal static MethodInfo ReplaceDeserializedObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (replaceDeserializedObjectMethod == null)
                    replaceDeserializedObjectMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ReplaceDeserializedObject", Globals.ScanAllMembers);
                return replaceDeserializedObjectMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getExistingObjectMethod;
        internal static MethodInfo GetExistingObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getExistingObjectMethod == null)
                    getExistingObjectMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetExistingObject", Globals.ScanAllMembers);
                return getExistingObjectMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getRealObjectMethod;
        internal static MethodInfo GetRealObjectMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getRealObjectMethod == null)
                    getRealObjectMethod = typeof(XmlObjectSerializerReadContext).GetMethod("GetRealObject", Globals.ScanAllMembers);
                return getRealObjectMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo readMethod;
        internal static MethodInfo ReadMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readMethod == null)
                    readMethod = typeof(XmlObjectSerializerReadContext).GetMethod("Read", Globals.ScanAllMembers);
                return readMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo ensureArraySizeMethod;
        internal static MethodInfo EnsureArraySizeMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (ensureArraySizeMethod == null)
                    ensureArraySizeMethod = typeof(XmlObjectSerializerReadContext).GetMethod("EnsureArraySize", Globals.ScanAllMembers);
                return ensureArraySizeMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo trimArraySizeMethod;
        internal static MethodInfo TrimArraySizeMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (trimArraySizeMethod == null)
                    trimArraySizeMethod = typeof(XmlObjectSerializerReadContext).GetMethod("TrimArraySize", Globals.ScanAllMembers);
                return trimArraySizeMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo checkEndOfArrayMethod;
        internal static MethodInfo CheckEndOfArrayMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (checkEndOfArrayMethod == null)
                    checkEndOfArrayMethod = typeof(XmlObjectSerializerReadContext).GetMethod("CheckEndOfArray", Globals.ScanAllMembers);
                return checkEndOfArrayMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getArrayLengthMethod;
        internal static MethodInfo GetArrayLengthMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getArrayLengthMethod == null)
                    getArrayLengthMethod = Globals.TypeOfArray.GetProperty("Length").GetGetMethod();
                return getArrayLengthMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo readSerializationInfoMethod;
        internal static MethodInfo ReadSerializationInfoMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readSerializationInfoMethod == null)
                    readSerializationInfoMethod = typeof(XmlObjectSerializerReadContext).GetMethod("ReadSerializationInfo", Globals.ScanAllMembers);
                return readSerializationInfoMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo createUnexpectedStateExceptionMethod;
        internal static MethodInfo CreateUnexpectedStateExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (createUnexpectedStateExceptionMethod == null)
                    createUnexpectedStateExceptionMethod = typeof(XmlObjectSerializerReadContext).GetMethod("CreateUnexpectedStateException", Globals.ScanAllMembers, null, new Type[] { typeof(XmlNodeType), typeof(XmlReaderDelegator) }, null);
                return createUnexpectedStateExceptionMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo internalSerializeReferenceMethod;
        internal static MethodInfo InternalSerializeReferenceMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (internalSerializeReferenceMethod == null)
                    internalSerializeReferenceMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("InternalSerializeReference", Globals.ScanAllMembers);
                return internalSerializeReferenceMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo internalSerializeMethod;
        internal static MethodInfo InternalSerializeMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (internalSerializeMethod == null)
                    internalSerializeMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("InternalSerialize", Globals.ScanAllMembers);
                return internalSerializeMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo writeNullMethod;
        internal static MethodInfo WriteNullMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeNullMethod == null)
                    writeNullMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("WriteNull", Globals.ScanAllMembers, null, new Type[] { typeof(XmlWriterDelegator), typeof(Type), typeof(bool) }, null);
                return writeNullMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo incrementArrayCountMethod;
        internal static MethodInfo IncrementArrayCountMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (incrementArrayCountMethod == null)
                    incrementArrayCountMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("IncrementArrayCount", Globals.ScanAllMembers);
                return incrementArrayCountMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo incrementCollectionCountMethod;
        internal static MethodInfo IncrementCollectionCountMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (incrementCollectionCountMethod == null)
                    incrementCollectionCountMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("IncrementCollectionCount", Globals.ScanAllMembers, null, new Type[] { typeof(XmlWriterDelegator), typeof(ICollection) }, null);
                return incrementCollectionCountMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo incrementCollectionCountGenericMethod;
        internal static MethodInfo IncrementCollectionCountGenericMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (incrementCollectionCountGenericMethod == null)
                    incrementCollectionCountGenericMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("IncrementCollectionCountGeneric", Globals.ScanAllMembers);
                return incrementCollectionCountGenericMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getDefaultValueMethod;
        internal static MethodInfo GetDefaultValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getDefaultValueMethod == null)
                    getDefaultValueMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("GetDefaultValue", Globals.ScanAllMembers);
                return getDefaultValueMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getNullableValueMethod;
        internal static MethodInfo GetNullableValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getNullableValueMethod == null)
                    getNullableValueMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("GetNullableValue", Globals.ScanAllMembers);
                return getNullableValueMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo throwRequiredMemberMustBeEmittedMethod;
        internal static MethodInfo ThrowRequiredMemberMustBeEmittedMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwRequiredMemberMustBeEmittedMethod == null)
                    throwRequiredMemberMustBeEmittedMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("ThrowRequiredMemberMustBeEmitted", Globals.ScanAllMembers);
                return throwRequiredMemberMustBeEmittedMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getHasValueMethod;
        internal static MethodInfo GetHasValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getHasValueMethod == null)
                    getHasValueMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("GetHasValue", Globals.ScanAllMembers);
                return getHasValueMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo writeISerializableMethod;
        internal static MethodInfo WriteISerializableMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeISerializableMethod == null)
                    writeISerializableMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("WriteISerializable", Globals.ScanAllMembers);
                return writeISerializableMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo writeExtensionDataMethod;
        internal static MethodInfo WriteExtensionDataMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeExtensionDataMethod == null)
                    writeExtensionDataMethod = typeof(XmlObjectSerializerWriteContext).GetMethod("WriteExtensionData", Globals.ScanAllMembers);
                return writeExtensionDataMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo writeXmlValueMethod;
        internal static MethodInfo WriteXmlValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (writeXmlValueMethod == null)
                    writeXmlValueMethod = typeof(DataContract).GetMethod("WriteXmlValue", Globals.ScanAllMembers);
                return writeXmlValueMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo readXmlValueMethod;
        internal static MethodInfo ReadXmlValueMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (readXmlValueMethod == null)
                    readXmlValueMethod = typeof(DataContract).GetMethod("ReadXmlValue", Globals.ScanAllMembers);
                return readXmlValueMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo throwTypeNotSerializableMethod;
        internal static MethodInfo ThrowTypeNotSerializableMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwTypeNotSerializableMethod == null)
                    throwTypeNotSerializableMethod = typeof(DataContract).GetMethod("ThrowTypeNotSerializable", Globals.ScanAllMembers);
                return throwTypeNotSerializableMethod;
            }
        }

        [SecurityCritical]
        static PropertyInfo namespaceProperty;
        internal static PropertyInfo NamespaceProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (namespaceProperty == null)
                    namespaceProperty = typeof(DataContract).GetProperty("Namespace", Globals.ScanAllMembers);
                return namespaceProperty;
            }
        }

        [SecurityCritical]
        static FieldInfo contractNamespacesField;
        internal static FieldInfo ContractNamespacesField
        {
            [SecuritySafeCritical]
            get
            {
                if (contractNamespacesField == null)
                    contractNamespacesField = typeof(ClassDataContract).GetField("ContractNamespaces", Globals.ScanAllMembers);
                return contractNamespacesField;
            }
        }

        [SecurityCritical]
        static FieldInfo memberNamesField;
        internal static FieldInfo MemberNamesField
        {
            [SecuritySafeCritical]
            get
            {
                if (memberNamesField == null)
                    memberNamesField = typeof(ClassDataContract).GetField("MemberNames", Globals.ScanAllMembers);
                return memberNamesField;
            }
        }

        [SecurityCritical]
        static MethodInfo extensionDataSetExplicitMethodInfo;
        internal static MethodInfo ExtensionDataSetExplicitMethodInfo
        {
            [SecuritySafeCritical]
            get
            {
                if (extensionDataSetExplicitMethodInfo == null)
                    extensionDataSetExplicitMethodInfo = typeof(IExtensibleDataObject).GetMethod(Globals.ExtensionDataSetMethod);
                return extensionDataSetExplicitMethodInfo;
            }
        }

        [SecurityCritical]
        static PropertyInfo childElementNamespacesProperty;
        internal static PropertyInfo ChildElementNamespacesProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (childElementNamespacesProperty == null)
                    childElementNamespacesProperty = typeof(ClassDataContract).GetProperty("ChildElementNamespaces", Globals.ScanAllMembers);
                return childElementNamespacesProperty;
            }
        }

        [SecurityCritical]
        static PropertyInfo collectionItemNameProperty;
        internal static PropertyInfo CollectionItemNameProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (collectionItemNameProperty == null)
                    collectionItemNameProperty = typeof(CollectionDataContract).GetProperty("CollectionItemName", Globals.ScanAllMembers);
                return collectionItemNameProperty;
            }
        }

        [SecurityCritical]
        static PropertyInfo childElementNamespaceProperty;
        internal static PropertyInfo ChildElementNamespaceProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (childElementNamespaceProperty == null)
                    childElementNamespaceProperty = typeof(CollectionDataContract).GetProperty("ChildElementNamespace", Globals.ScanAllMembers);
                return childElementNamespaceProperty;
            }
        }

        [SecurityCritical]
        static MethodInfo getDateTimeOffsetMethod;
        internal static MethodInfo GetDateTimeOffsetMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getDateTimeOffsetMethod == null)
                    getDateTimeOffsetMethod = typeof(DateTimeOffsetAdapter).GetMethod("GetDateTimeOffset", Globals.ScanAllMembers);
                return getDateTimeOffsetMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo getDateTimeOffsetAdapterMethod;
        internal static MethodInfo GetDateTimeOffsetAdapterMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (getDateTimeOffsetAdapterMethod == null)
                    getDateTimeOffsetAdapterMethod = typeof(DateTimeOffsetAdapter).GetMethod("GetDateTimeOffsetAdapter", Globals.ScanAllMembers);
                return getDateTimeOffsetAdapterMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo traceInstructionMethod;
        internal static MethodInfo TraceInstructionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (traceInstructionMethod == null)
                    traceInstructionMethod = typeof(SerializationTrace).GetMethod("TraceInstruction", Globals.ScanAllMembers);
                return traceInstructionMethod;
            }
        }

        [SecurityCritical]
        static MethodInfo throwInvalidDataContractExceptionMethod;
        internal static MethodInfo ThrowInvalidDataContractExceptionMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (throwInvalidDataContractExceptionMethod == null)
                    throwInvalidDataContractExceptionMethod = typeof(DataContract).GetMethod("ThrowInvalidDataContractException", Globals.ScanAllMembers, null, new Type[] { typeof(string), typeof(Type) }, null);
                return throwInvalidDataContractExceptionMethod;
            }
        }

        [SecurityCritical]
        static PropertyInfo serializeReadOnlyTypesProperty;
        internal static PropertyInfo SerializeReadOnlyTypesProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (serializeReadOnlyTypesProperty == null)
                    serializeReadOnlyTypesProperty = typeof(XmlObjectSerializerWriteContext).GetProperty("SerializeReadOnlyTypes", Globals.ScanAllMembers);
                return serializeReadOnlyTypesProperty;
            }
        }

        [SecurityCritical]
        static PropertyInfo classSerializationExceptionMessageProperty;
        internal static PropertyInfo ClassSerializationExceptionMessageProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (classSerializationExceptionMessageProperty == null)
                    classSerializationExceptionMessageProperty = typeof(ClassDataContract).GetProperty("SerializationExceptionMessage", Globals.ScanAllMembers);
                return classSerializationExceptionMessageProperty;
            }
        }

        [SecurityCritical]
        static PropertyInfo collectionSerializationExceptionMessageProperty;
        internal static PropertyInfo CollectionSerializationExceptionMessageProperty
        {
            [SecuritySafeCritical]
            get
            {
                if (collectionSerializationExceptionMessageProperty == null)
                    collectionSerializationExceptionMessageProperty = typeof(CollectionDataContract).GetProperty("SerializationExceptionMessage", Globals.ScanAllMembers);
                return collectionSerializationExceptionMessageProperty;
            }
        }
    }
}
