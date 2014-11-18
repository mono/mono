//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Fx.Tag.SecurityNote(Critical = "Class holds static instances used in serializer. "
        + "Static fields are marked SecurityCritical or readonly to prevent "
        + "data from being modified or leaked to other components in appdomain.",
        Safe = "All get-only properties marked safe since they only need to be protected for write.")]
    static class Globals
    {
        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Changes to const could affect code generation logic; any changes should be reviewed.")]
        internal const BindingFlags ScanAllMembers = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        [SecurityCritical]
        static XmlQualifiedName idQualifiedName;
        internal static XmlQualifiedName IdQualifiedName
        {
            [SecuritySafeCritical]
            get
            {
                if (idQualifiedName == null)
                    idQualifiedName = new XmlQualifiedName(Globals.IdLocalName, Globals.SerializationNamespace);
                return idQualifiedName;
            }
        }

        [SecurityCritical]
        static XmlQualifiedName refQualifiedName;
        internal static XmlQualifiedName RefQualifiedName
        {
            [SecuritySafeCritical]
            get
            {
                if (refQualifiedName == null)
                    refQualifiedName = new XmlQualifiedName(Globals.RefLocalName, Globals.SerializationNamespace);
                return refQualifiedName;
            }
        }

        [SecurityCritical]
        static Type typeOfObject;
        internal static Type TypeOfObject
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfObject == null)
                    typeOfObject = typeof(object);
                return typeOfObject;
            }
        }

        [SecurityCritical]
        static Type typeOfValueType;
        internal static Type TypeOfValueType
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfValueType == null)
                    typeOfValueType = typeof(ValueType);
                return typeOfValueType;
            }
        }

        [SecurityCritical]
        static Type typeOfArray;
        internal static Type TypeOfArray
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfArray == null)
                    typeOfArray = typeof(Array);
                return typeOfArray;
            }
        }

        [SecurityCritical]
        static Type typeOfString;
        internal static Type TypeOfString
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfString == null)
                    typeOfString = typeof(string);
                return typeOfString;
            }
        }

        [SecurityCritical]
        static Type typeOfInt;
        internal static Type TypeOfInt
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfInt == null)
                    typeOfInt = typeof(int);
                return typeOfInt;
            }
        }

        [SecurityCritical]
        static Type typeOfULong;
        internal static Type TypeOfULong
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfULong == null)
                    typeOfULong = typeof(ulong);
                return typeOfULong;
            }
        }

        [SecurityCritical]
        static Type typeOfVoid;
        internal static Type TypeOfVoid
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfVoid == null)
                    typeOfVoid = typeof(void);
                return typeOfVoid;
            }
        }

        [SecurityCritical]
        static Type typeOfByteArray;
        internal static Type TypeOfByteArray
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfByteArray == null)
                    typeOfByteArray = typeof(byte[]);
                return typeOfByteArray;
            }
        }

        [SecurityCritical]
        static Type typeOfTimeSpan;
        internal static Type TypeOfTimeSpan
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfTimeSpan == null)
                    typeOfTimeSpan = typeof(TimeSpan);
                return typeOfTimeSpan;
            }
        }

        [SecurityCritical]
        static Type typeOfGuid;
        internal static Type TypeOfGuid
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfGuid == null)
                    typeOfGuid = typeof(Guid);
                return typeOfGuid;
            }
        }

        [SecurityCritical]
        static Type typeOfDateTimeOffset;
        internal static Type TypeOfDateTimeOffset
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfDateTimeOffset == null)
                    typeOfDateTimeOffset = typeof(DateTimeOffset);
                return typeOfDateTimeOffset;
            }
        }

        [SecurityCritical]
        static Type typeOfDateTimeOffsetAdapter;
        internal static Type TypeOfDateTimeOffsetAdapter
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfDateTimeOffsetAdapter == null)
                    typeOfDateTimeOffsetAdapter = typeof(DateTimeOffsetAdapter);
                return typeOfDateTimeOffsetAdapter;
            }
        }

        [SecurityCritical]
        static Type typeOfUri;
        internal static Type TypeOfUri
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfUri == null)
                    typeOfUri = typeof(Uri);
                return typeOfUri;
            }
        }

        [SecurityCritical]
        static Type typeOfTypeEnumerable;
        internal static Type TypeOfTypeEnumerable
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfTypeEnumerable == null)
                    typeOfTypeEnumerable = typeof(IEnumerable<Type>);
                return typeOfTypeEnumerable;
            }
        }

        [SecurityCritical]
        static Type typeOfStreamingContext;
        internal static Type TypeOfStreamingContext
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfStreamingContext == null)
                    typeOfStreamingContext = typeof(StreamingContext);
                return typeOfStreamingContext;
            }
        }

        [SecurityCritical]
        static Type typeOfISerializable;
        internal static Type TypeOfISerializable
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfISerializable == null)
                    typeOfISerializable = typeof(ISerializable);
                return typeOfISerializable;
            }
        }

        [SecurityCritical]
        static Type typeOfIDeserializationCallback;
        internal static Type TypeOfIDeserializationCallback
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIDeserializationCallback == null)
                    typeOfIDeserializationCallback = typeof(IDeserializationCallback);
                return typeOfIDeserializationCallback;
            }
        }

        [SecurityCritical]
        static Type typeOfIObjectReference;
        internal static Type TypeOfIObjectReference
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIObjectReference == null)
                    typeOfIObjectReference = typeof(IObjectReference);
                return typeOfIObjectReference;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlFormatClassWriterDelegate;
        internal static Type TypeOfXmlFormatClassWriterDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlFormatClassWriterDelegate == null)
                    typeOfXmlFormatClassWriterDelegate = typeof(XmlFormatClassWriterDelegate);
                return typeOfXmlFormatClassWriterDelegate;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlFormatCollectionWriterDelegate;
        internal static Type TypeOfXmlFormatCollectionWriterDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlFormatCollectionWriterDelegate == null)
                    typeOfXmlFormatCollectionWriterDelegate = typeof(XmlFormatCollectionWriterDelegate);
                return typeOfXmlFormatCollectionWriterDelegate;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlFormatClassReaderDelegate;
        internal static Type TypeOfXmlFormatClassReaderDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlFormatClassReaderDelegate == null)
                    typeOfXmlFormatClassReaderDelegate = typeof(XmlFormatClassReaderDelegate);
                return typeOfXmlFormatClassReaderDelegate;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlFormatCollectionReaderDelegate;
        internal static Type TypeOfXmlFormatCollectionReaderDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlFormatCollectionReaderDelegate == null)
                    typeOfXmlFormatCollectionReaderDelegate = typeof(XmlFormatCollectionReaderDelegate);
                return typeOfXmlFormatCollectionReaderDelegate;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlFormatGetOnlyCollectionReaderDelegate;
        internal static Type TypeOfXmlFormatGetOnlyCollectionReaderDelegate
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlFormatGetOnlyCollectionReaderDelegate == null)
                    typeOfXmlFormatGetOnlyCollectionReaderDelegate = typeof(XmlFormatGetOnlyCollectionReaderDelegate);
                return typeOfXmlFormatGetOnlyCollectionReaderDelegate;
            }
        }

        [SecurityCritical]
        static Type typeOfKnownTypeAttribute;
        internal static Type TypeOfKnownTypeAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfKnownTypeAttribute == null)
                    typeOfKnownTypeAttribute = typeof(KnownTypeAttribute);
                return typeOfKnownTypeAttribute;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Attribute type used in security decision.")]
        [SecurityCritical]
        static Type typeOfDataContractAttribute;
        internal static Type TypeOfDataContractAttribute
        {
            [Fx.Tag.SecurityNote(Critical = "Accesses critical field for attribute type.",
                Safe = "Controls inputs and logic.")]
            [SecuritySafeCritical]
            get
            {
                if (typeOfDataContractAttribute == null)
                    typeOfDataContractAttribute = typeof(DataContractAttribute);
                return typeOfDataContractAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfContractNamespaceAttribute;
        internal static Type TypeOfContractNamespaceAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfContractNamespaceAttribute == null)
                    typeOfContractNamespaceAttribute = typeof(ContractNamespaceAttribute);
                return typeOfContractNamespaceAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfDataMemberAttribute;
        internal static Type TypeOfDataMemberAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfDataMemberAttribute == null)
                    typeOfDataMemberAttribute = typeof(DataMemberAttribute);
                return typeOfDataMemberAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfEnumMemberAttribute;
        internal static Type TypeOfEnumMemberAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfEnumMemberAttribute == null)
                    typeOfEnumMemberAttribute = typeof(EnumMemberAttribute);
                return typeOfEnumMemberAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfCollectionDataContractAttribute;
        internal static Type TypeOfCollectionDataContractAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfCollectionDataContractAttribute == null)
                    typeOfCollectionDataContractAttribute = typeof(CollectionDataContractAttribute);
                return typeOfCollectionDataContractAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfOptionalFieldAttribute;
        internal static Type TypeOfOptionalFieldAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfOptionalFieldAttribute == null)
                    typeOfOptionalFieldAttribute = typeof(OptionalFieldAttribute);
                return typeOfOptionalFieldAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfObjectArray;
        internal static Type TypeOfObjectArray
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfObjectArray == null)
                    typeOfObjectArray = typeof(object[]);
                return typeOfObjectArray;
            }
        }

        [SecurityCritical]
        static Type typeOfOnSerializingAttribute;
        internal static Type TypeOfOnSerializingAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfOnSerializingAttribute == null)
                    typeOfOnSerializingAttribute = typeof(OnSerializingAttribute);
                return typeOfOnSerializingAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfOnSerializedAttribute;
        internal static Type TypeOfOnSerializedAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfOnSerializedAttribute == null)
                    typeOfOnSerializedAttribute = typeof(OnSerializedAttribute);
                return typeOfOnSerializedAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfOnDeserializingAttribute;
        internal static Type TypeOfOnDeserializingAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfOnDeserializingAttribute == null)
                    typeOfOnDeserializingAttribute = typeof(OnDeserializingAttribute);
                return typeOfOnDeserializingAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfOnDeserializedAttribute;
        internal static Type TypeOfOnDeserializedAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfOnDeserializedAttribute == null)
                    typeOfOnDeserializedAttribute = typeof(OnDeserializedAttribute);
                return typeOfOnDeserializedAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfFlagsAttribute;
        internal static Type TypeOfFlagsAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfFlagsAttribute == null)
                    typeOfFlagsAttribute = typeof(FlagsAttribute);
                return typeOfFlagsAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfSerializableAttribute;
        internal static Type TypeOfSerializableAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfSerializableAttribute == null)
                    typeOfSerializableAttribute = typeof(SerializableAttribute);
                return typeOfSerializableAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfNonSerializedAttribute;
        internal static Type TypeOfNonSerializedAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfNonSerializedAttribute == null)
                    typeOfNonSerializedAttribute = typeof(NonSerializedAttribute);
                return typeOfNonSerializedAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfSerializationInfo;
        internal static Type TypeOfSerializationInfo
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfSerializationInfo == null)
                    typeOfSerializationInfo = typeof(SerializationInfo);
                return typeOfSerializationInfo;
            }
        }

        [SecurityCritical]
        static Type typeOfSerializationInfoEnumerator;
        internal static Type TypeOfSerializationInfoEnumerator
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfSerializationInfoEnumerator == null)
                    typeOfSerializationInfoEnumerator = typeof(SerializationInfoEnumerator);
                return typeOfSerializationInfoEnumerator;
            }
        }

        [SecurityCritical]
        static Type typeOfSerializationEntry;
        internal static Type TypeOfSerializationEntry
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfSerializationEntry == null)
                    typeOfSerializationEntry = typeof(SerializationEntry);
                return typeOfSerializationEntry;
            }
        }

        [SecurityCritical]
        static Type typeOfIXmlSerializable;
        internal static Type TypeOfIXmlSerializable
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIXmlSerializable == null)
                    typeOfIXmlSerializable = typeof(IXmlSerializable);
                return typeOfIXmlSerializable;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlSchemaProviderAttribute;
        internal static Type TypeOfXmlSchemaProviderAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlSchemaProviderAttribute == null)
                    typeOfXmlSchemaProviderAttribute = typeof(XmlSchemaProviderAttribute);
                return typeOfXmlSchemaProviderAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlRootAttribute;
        internal static Type TypeOfXmlRootAttribute
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlRootAttribute == null)
                    typeOfXmlRootAttribute = typeof(XmlRootAttribute);
                return typeOfXmlRootAttribute;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlQualifiedName;
        internal static Type TypeOfXmlQualifiedName
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlQualifiedName == null)
                    typeOfXmlQualifiedName = typeof(XmlQualifiedName);
                return typeOfXmlQualifiedName;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlSchemaType;
        internal static Type TypeOfXmlSchemaType
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlSchemaType == null)
                    typeOfXmlSchemaType = typeof(XmlSchemaType);
                return typeOfXmlSchemaType;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlSerializableServices;
        internal static Type TypeOfXmlSerializableServices
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlSerializableServices == null)
                    typeOfXmlSerializableServices = typeof(XmlSerializableServices);
                return typeOfXmlSerializableServices;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlNodeArray;
        internal static Type TypeOfXmlNodeArray
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlNodeArray == null)
                    typeOfXmlNodeArray = typeof(XmlNode[]);
                return typeOfXmlNodeArray;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlSchemaSet;
        internal static Type TypeOfXmlSchemaSet
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlSchemaSet == null)
                    typeOfXmlSchemaSet = typeof(XmlSchemaSet);
                return typeOfXmlSchemaSet;
            }
        }

        [SecurityCritical]
        static object[] emptyObjectArray;
        internal static object[] EmptyObjectArray
        {
            [SecuritySafeCritical]
            get
            {
                if (emptyObjectArray == null)
                    emptyObjectArray = new object[0];
                return emptyObjectArray;
            }
        }

        [SecurityCritical]
        static Type[] emptyTypeArray;
        internal static Type[] EmptyTypeArray
        {
            [SecuritySafeCritical]
            get
            {
                if (emptyTypeArray == null)
                    emptyTypeArray = new Type[0];
                return emptyTypeArray;
            }
        }

        [SecurityCritical]
        static Type typeOfIPropertyChange;
        internal static Type TypeOfIPropertyChange
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIPropertyChange == null)
                    typeOfIPropertyChange = typeof(INotifyPropertyChanged);
                return typeOfIPropertyChange;
            }
        }

        [SecurityCritical]
        static Type typeOfIExtensibleDataObject;
        internal static Type TypeOfIExtensibleDataObject
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIExtensibleDataObject == null)
                    typeOfIExtensibleDataObject = typeof(IExtensibleDataObject);
                return typeOfIExtensibleDataObject;
            }
        }

        [SecurityCritical]
        static Type typeOfExtensionDataObject;
        internal static Type TypeOfExtensionDataObject
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfExtensionDataObject == null)
                    typeOfExtensionDataObject = typeof(ExtensionDataObject);
                return typeOfExtensionDataObject;
            }
        }

        [SecurityCritical]
        static Type typeOfISerializableDataNode;
        internal static Type TypeOfISerializableDataNode
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfISerializableDataNode == null)
                    typeOfISerializableDataNode = typeof(ISerializableDataNode);
                return typeOfISerializableDataNode;
            }
        }

        [SecurityCritical]
        static Type typeOfClassDataNode;
        internal static Type TypeOfClassDataNode
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfClassDataNode == null)
                    typeOfClassDataNode = typeof(ClassDataNode);
                return typeOfClassDataNode;
            }
        }

        [SecurityCritical]
        static Type typeOfCollectionDataNode;
        internal static Type TypeOfCollectionDataNode
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfCollectionDataNode == null)
                    typeOfCollectionDataNode = typeof(CollectionDataNode);
                return typeOfCollectionDataNode;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlDataNode;
        internal static Type TypeOfXmlDataNode
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlDataNode == null)
                    typeOfXmlDataNode = typeof(XmlDataNode);
                return typeOfXmlDataNode;
            }
        }

        [SecurityCritical]
        static Type typeOfNullable;
        internal static Type TypeOfNullable
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfNullable == null)
                    typeOfNullable = typeof(Nullable<>);
                return typeOfNullable;
            }
        }

        [SecurityCritical]
        static Type typeOfReflectionPointer;
        internal static Type TypeOfReflectionPointer
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfReflectionPointer == null)
                    typeOfReflectionPointer = typeof(System.Reflection.Pointer);
                return typeOfReflectionPointer;
            }
        }

        [SecurityCritical]
        static Type typeOfIDictionaryGeneric;
        internal static Type TypeOfIDictionaryGeneric
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIDictionaryGeneric == null)
                    typeOfIDictionaryGeneric = typeof(IDictionary<,>);
                return typeOfIDictionaryGeneric;
            }
        }

        [SecurityCritical]
        static Type typeOfIDictionary;
        internal static Type TypeOfIDictionary
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIDictionary == null)
                    typeOfIDictionary = typeof(IDictionary);
                return typeOfIDictionary;
            }
        }

        [SecurityCritical]
        static Type typeOfIListGeneric;
        internal static Type TypeOfIListGeneric
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIListGeneric == null)
                    typeOfIListGeneric = typeof(IList<>);
                return typeOfIListGeneric;
            }
        }

        [SecurityCritical]
        static Type typeOfIList;
        internal static Type TypeOfIList
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIList == null)
                    typeOfIList = typeof(IList);
                return typeOfIList;
            }
        }

        [SecurityCritical]
        static Type typeOfICollectionGeneric;
        internal static Type TypeOfICollectionGeneric
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfICollectionGeneric == null)
                    typeOfICollectionGeneric = typeof(ICollection<>);
                return typeOfICollectionGeneric;
            }
        }

        [SecurityCritical]
        static Type typeOfICollection;
        internal static Type TypeOfICollection
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfICollection == null)
                    typeOfICollection = typeof(ICollection);
                return typeOfICollection;
            }
        }

        [SecurityCritical]
        static Type typeOfIEnumerableGeneric;
        internal static Type TypeOfIEnumerableGeneric
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIEnumerableGeneric == null)
                    typeOfIEnumerableGeneric = typeof(IEnumerable<>);
                return typeOfIEnumerableGeneric;
            }
        }

        [SecurityCritical]
        static Type typeOfIEnumerable;
        internal static Type TypeOfIEnumerable
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIEnumerable == null)
                    typeOfIEnumerable = typeof(IEnumerable);
                return typeOfIEnumerable;
            }
        }

        [SecurityCritical]
        static Type typeOfIEnumeratorGeneric;
        internal static Type TypeOfIEnumeratorGeneric
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIEnumeratorGeneric == null)
                    typeOfIEnumeratorGeneric = typeof(IEnumerator<>);
                return typeOfIEnumeratorGeneric;
            }
        }

        [SecurityCritical]
        static Type typeOfIEnumerator;
        internal static Type TypeOfIEnumerator
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIEnumerator == null)
                    typeOfIEnumerator = typeof(IEnumerator);
                return typeOfIEnumerator;
            }
        }

        [SecurityCritical]
        static Type typeOfKeyValuePair;
        internal static Type TypeOfKeyValuePair
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfKeyValuePair == null)
                    typeOfKeyValuePair = typeof(KeyValuePair<,>);
                return typeOfKeyValuePair;
            }
        }

        [SecurityCritical]
        static Type typeOfKeyValue;
        internal static Type TypeOfKeyValue
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfKeyValue == null)
                    typeOfKeyValue = typeof(KeyValue<,>);
                return typeOfKeyValue;
            }
        }

        [SecurityCritical]
        static Type typeOfIDictionaryEnumerator;
        internal static Type TypeOfIDictionaryEnumerator
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfIDictionaryEnumerator == null)
                    typeOfIDictionaryEnumerator = typeof(IDictionaryEnumerator);
                return typeOfIDictionaryEnumerator;
            }
        }

        [SecurityCritical]
        static Type typeOfDictionaryEnumerator;
        internal static Type TypeOfDictionaryEnumerator
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfDictionaryEnumerator == null)
                    typeOfDictionaryEnumerator = typeof(CollectionDataContract.DictionaryEnumerator);
                return typeOfDictionaryEnumerator;
            }
        }

        [SecurityCritical]
        static Type typeOfGenericDictionaryEnumerator;
        internal static Type TypeOfGenericDictionaryEnumerator
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfGenericDictionaryEnumerator == null)
                    typeOfGenericDictionaryEnumerator = typeof(CollectionDataContract.GenericDictionaryEnumerator<,>);
                return typeOfGenericDictionaryEnumerator;
            }
        }

        [SecurityCritical]
        static Type typeOfDictionaryGeneric;
        internal static Type TypeOfDictionaryGeneric
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfDictionaryGeneric == null)
                    typeOfDictionaryGeneric = typeof(Dictionary<,>);
                return typeOfDictionaryGeneric;
            }
        }

        [SecurityCritical]
        static Type typeOfHashtable;
        internal static Type TypeOfHashtable
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfHashtable == null)
                    typeOfHashtable = typeof(Hashtable);
                return typeOfHashtable;
            }
        }

        [SecurityCritical]
        static Type typeOfListGeneric;
        internal static Type TypeOfListGeneric
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfListGeneric == null)
                    typeOfListGeneric = typeof(List<>);
                return typeOfListGeneric;
            }
        }

        [SecurityCritical]
        static Type typeOfXmlElement;
        internal static Type TypeOfXmlElement
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfXmlElement == null)
                    typeOfXmlElement = typeof(XmlElement);
                return typeOfXmlElement;
            }
        }

        [SecurityCritical]
        static Type typeOfDBNull;
        internal static Type TypeOfDBNull
        {
            [SecuritySafeCritical]
            get
            {
                if (typeOfDBNull == null)
                    typeOfDBNull = typeof(DBNull);
                return typeOfDBNull;
            }
        }

        [SecurityCritical]
        static Type typeOfSafeSerializationManager;
        static bool typeOfSafeSerializationManagerSet;
        internal static Type TypeOfSafeSerializationManager
        {
            [SecuritySafeCritical]
            get
            {
                if (!typeOfSafeSerializationManagerSet)
                {
                    typeOfSafeSerializationManager = TypeOfInt.Assembly.GetType("System.Runtime.Serialization.SafeSerializationManager");
                    typeOfSafeSerializationManagerSet = true;
                }
                return typeOfSafeSerializationManager;
            }
        }

        [SecurityCritical]
        static Uri dataContractXsdBaseNamespaceUri;
        internal static Uri DataContractXsdBaseNamespaceUri
        {
            [SecuritySafeCritical]
            get
            {
                if (dataContractXsdBaseNamespaceUri == null)
                    dataContractXsdBaseNamespaceUri = new Uri(DataContractXsdBaseNamespace);
                return dataContractXsdBaseNamespaceUri;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds instance of SecurityPermission that we will Demand for SerializationFormatter."
            + " Should not be modified to something else.")]
        [SecurityCritical]
        static SecurityPermission serializationFormatterPermission;
        public static SecurityPermission SerializationFormatterPermission
        {
            [Fx.Tag.SecurityNote(Critical = "Sets and accesses instance of SecurityPermission that we will Demand for SerializationFormatter.")]
            [SecurityCritical]
            get
            {
                if (serializationFormatterPermission == null)
                    serializationFormatterPermission = new SecurityPermission(SecurityPermissionFlag.SerializationFormatter);
                return serializationFormatterPermission;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds instance of ReflectionPermission that we will Demand for MemberAccess."
            + " Should not be modified to something else.")]
        [SecurityCritical]
        static ReflectionPermission memberAccessPermission;
        public static ReflectionPermission MemberAccessPermission
        {
            [Fx.Tag.SecurityNote(Critical = "Sets and accesses instance of ReflectionPermission that we will Demand for MemberAccess.")]
            [SecurityCritical]
            get
            {
                if (memberAccessPermission == null)
                    memberAccessPermission = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
                return memberAccessPermission;
            }
        }


        public const bool DefaultIsRequired = false;
        public const bool DefaultEmitDefaultValue = true;
        public const int DefaultOrder = 0;
        public const bool DefaultIsReference = false;
        // The value string.Empty aids comparisons (can do simple length checks
        //     instead of string comparison method calls in IL.)
        public readonly static string NewObjectId = string.Empty;
        public const string SimpleSRSInternalsVisiblePattern = @"^[\s]*System\.Runtime\.Serialization[\s]*$";
        public const string FullSRSInternalsVisiblePattern = @"^[\s]*System\.Runtime\.Serialization[\s]*,[\s]*PublicKey[\s]*=[\s]*(?i:00000000000000000400000000000000)[\s]*$";
        public const string SafeSerializationManagerName = "SafeSerializationManager";
        public const string SafeSerializationManagerNamespace = "http://schemas.datacontract.org/2004/07/System.Runtime.Serialization";
        public const string NullObjectId = null;
        public const string Space = " ";
        public const string OpenBracket = "[";
        public const string CloseBracket = "]";
        public const string Comma = ",";
        public const string XsiPrefix = "i";
        public const string XsdPrefix = "x";
        public const string SerPrefix = "z";
        public const string SerPrefixForSchema = "ser";
        public const string ElementPrefix = "q";
        public const string DataContractXsdBaseNamespace = "http://schemas.datacontract.org/2004/07/";
        public const string DataContractXmlNamespace = DataContractXsdBaseNamespace + "System.Xml";
        public const string SchemaInstanceNamespace = XmlSchema.InstanceNamespace;
        public const string SchemaNamespace = XmlSchema.Namespace;
        public const string XsiNilLocalName = "nil";
        public const string XsiTypeLocalName = "type";
        public const string TnsPrefix = "tns";
        public const string OccursUnbounded = "unbounded";
        public const string AnyTypeLocalName = "anyType";
        public const string StringLocalName = "string";
        public const string IntLocalName = "int";
        public const string True = "true";
        public const string False = "false";
        public const string ArrayPrefix = "ArrayOf";
        public const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";
        public const string XmlnsPrefix = "xmlns";
        public const string SchemaLocalName = "schema";
        public const string CollectionsNamespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
        public const string DefaultClrNamespace = "GeneratedNamespace";
        public const string DefaultTypeName = "GeneratedType";
        public const string DefaultGeneratedMember = "GeneratedMember";
        public const string DefaultFieldSuffix = "Field";
        public const string DefaultPropertySuffix = "Property";
        public const string DefaultMemberSuffix = "Member";
        public const string NameProperty = "Name";
        public const string NamespaceProperty = "Namespace";
        public const string OrderProperty = "Order";
        public const string IsReferenceProperty = "IsReference";
        public const string IsRequiredProperty = "IsRequired";
        public const string EmitDefaultValueProperty = "EmitDefaultValue";
        public const string ClrNamespaceProperty = "ClrNamespace";
        public const string ItemNameProperty = "ItemName";
        public const string KeyNameProperty = "KeyName";
        public const string ValueNameProperty = "ValueName";
        public const string SerializationInfoPropertyName = "SerializationInfo";
        public const string SerializationInfoFieldName = "info";
        public const string NodeArrayPropertyName = "Nodes";
        public const string NodeArrayFieldName = "nodesField";
        public const string ExportSchemaMethod = "ExportSchema";
        public const string IsAnyProperty = "IsAny";
        public const string ContextFieldName = "context";
        public const string GetObjectDataMethodName = "GetObjectData";
        public const string GetEnumeratorMethodName = "GetEnumerator";
        public const string MoveNextMethodName = "MoveNext";
        public const string AddValueMethodName = "AddValue";
        public const string CurrentPropertyName = "Current";
        public const string ValueProperty = "Value";
        public const string EnumeratorFieldName = "enumerator";
        public const string SerializationEntryFieldName = "entry";
        public const string ExtensionDataSetMethod = "set_ExtensionData";
        public const string ExtensionDataSetExplicitMethod = "System.Runtime.Serialization.IExtensibleDataObject.set_ExtensionData";
        public const string ExtensionDataObjectPropertyName = "ExtensionData";
        public const string ExtensionDataObjectFieldName = "extensionDataField";
        public const string AddMethodName = "Add";
        public const string ParseMethodName = "Parse";
        public const string GetCurrentMethodName = "get_Current";
        // NOTE: These values are used in schema below. If you modify any value, please make the same change in the schema.
        public const string SerializationNamespace = "http://schemas.microsoft.com/2003/10/Serialization/";
        public const string ClrTypeLocalName = "Type";
        public const string ClrAssemblyLocalName = "Assembly";
        public const string IsValueTypeLocalName = "IsValueType";
        public const string EnumerationValueLocalName = "EnumerationValue";
        public const string SurrogateDataLocalName = "Surrogate";
        public const string GenericTypeLocalName = "GenericType";
        public const string GenericParameterLocalName = "GenericParameter";
        public const string GenericNameAttribute = "Name";
        public const string GenericNamespaceAttribute = "Namespace";
        public const string GenericParameterNestedLevelAttribute = "NestedLevel";
        public const string IsDictionaryLocalName = "IsDictionary";
        public const string ActualTypeLocalName = "ActualType";
        public const string ActualTypeNameAttribute = "Name";
        public const string ActualTypeNamespaceAttribute = "Namespace";
        public const string DefaultValueLocalName = "DefaultValue";
        public const string EmitDefaultValueAttribute = "EmitDefaultValue";
        public const string ISerializableFactoryTypeLocalName = "FactoryType";
        public const string IdLocalName = "Id";
        public const string RefLocalName = "Ref";
        public const string ArraySizeLocalName = "Size";
        public const string KeyLocalName = "Key";
        public const string ValueLocalName = "Value";
        public const string MscorlibAssemblyName = "0";
        public const string MscorlibAssemblySimpleName = "mscorlib";
        public const string MscorlibFileName = MscorlibAssemblySimpleName + ".dll";
        public const string SerializationSchema = @"<?xml version='1.0' encoding='utf-8'?>
<xs:schema elementFormDefault='qualified' attributeFormDefault='qualified' xmlns:tns='http://schemas.microsoft.com/2003/10/Serialization/' targetNamespace='http://schemas.microsoft.com/2003/10/Serialization/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='anyType' nillable='true' type='xs:anyType' />
  <xs:element name='anyURI' nillable='true' type='xs:anyURI' />
  <xs:element name='base64Binary' nillable='true' type='xs:base64Binary' />
  <xs:element name='boolean' nillable='true' type='xs:boolean' />
  <xs:element name='byte' nillable='true' type='xs:byte' />
  <xs:element name='dateTime' nillable='true' type='xs:dateTime' />
  <xs:element name='decimal' nillable='true' type='xs:decimal' />
  <xs:element name='double' nillable='true' type='xs:double' />
  <xs:element name='float' nillable='true' type='xs:float' />
  <xs:element name='int' nillable='true' type='xs:int' />
  <xs:element name='long' nillable='true' type='xs:long' />
  <xs:element name='QName' nillable='true' type='xs:QName' />
  <xs:element name='short' nillable='true' type='xs:short' />
  <xs:element name='string' nillable='true' type='xs:string' />
  <xs:element name='unsignedByte' nillable='true' type='xs:unsignedByte' />
  <xs:element name='unsignedInt' nillable='true' type='xs:unsignedInt' />
  <xs:element name='unsignedLong' nillable='true' type='xs:unsignedLong' />
  <xs:element name='unsignedShort' nillable='true' type='xs:unsignedShort' />
  <xs:element name='char' nillable='true' type='tns:char' />
  <xs:simpleType name='char'>
    <xs:restriction base='xs:int'/>
  </xs:simpleType>  
  <xs:element name='duration' nillable='true' type='tns:duration' />
  <xs:simpleType name='duration'>
    <xs:restriction base='xs:duration'>
      <xs:pattern value='\-?P(\d*D)?(T(\d*H)?(\d*M)?(\d*(\.\d*)?S)?)?' />
      <xs:minInclusive value='-P10675199DT2H48M5.4775808S' />
      <xs:maxInclusive value='P10675199DT2H48M5.4775807S' />
    </xs:restriction>
  </xs:simpleType>
  <xs:element name='guid' nillable='true' type='tns:guid' />
  <xs:simpleType name='guid'>
    <xs:restriction base='xs:string'>
      <xs:pattern value='[\da-fA-F]{8}-[\da-fA-F]{4}-[\da-fA-F]{4}-[\da-fA-F]{4}-[\da-fA-F]{12}' />
    </xs:restriction>
  </xs:simpleType>
  <xs:attribute name='FactoryType' type='xs:QName' />
  <xs:attribute name='Id' type='xs:ID' />
  <xs:attribute name='Ref' type='xs:IDREF' />
</xs:schema>
";
    }
}

