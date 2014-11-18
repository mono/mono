//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.IO;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using System.Xml;
    using System.Runtime.Serialization.Configuration;
    using DataContractDictionary = System.Collections.Generic.Dictionary<System.Xml.XmlQualifiedName, DataContract>;
    using System.Security;
    using System.Security.Permissions;

    [DataContract(Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
#if USE_REFEMIT
    public struct KeyValue<K, V>
#else
    internal struct KeyValue<K, V>
#endif
    {
        K key;
        V value;

        internal KeyValue(K key, V value)
        {
            this.key = key;
            this.value = value;
        }

        [DataMember(IsRequired = true)]
        public K Key
        {
            get { return key; }
            set { key = value; }
        }

        [DataMember(IsRequired = true)]
        public V Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }

    internal enum CollectionKind : byte
    {
        None,
        GenericDictionary,
        Dictionary,
        GenericList,
        GenericCollection,
        List,
        GenericEnumerable,
        Collection,
        Enumerable,
        Array,
    }

#if USE_REFEMIT
    public sealed class CollectionDataContract : DataContract
#else
    internal sealed class CollectionDataContract : DataContract
#endif
    {
        [Fx.Tag.SecurityNote(Critical = "XmlDictionaryString representing the XML element name for collection items."
            + "Statically cached and used from IL generated code.")]
        [SecurityCritical]
        XmlDictionaryString collectionItemName;

        [Fx.Tag.SecurityNote(Critical = "XmlDictionaryString representing the XML namespace for collection items."
            + "Statically cached and used from IL generated code.")]
        [SecurityCritical]
        XmlDictionaryString childElementNamespace;

        [Fx.Tag.SecurityNote(Critical = "Internal DataContract representing the contract for collection items."
            + "Statically cached and used from IL generated code.")]
        [SecurityCritical]
        DataContract itemContract;

        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization. "
            + "Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        CollectionDataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal CollectionDataContract(CollectionKind kind)
            : base(new CollectionDataContractCriticalHelper(kind))
        {
            InitCollectionDataContract(this);
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal CollectionDataContract(Type type)
            : base(new CollectionDataContractCriticalHelper(type))
        {
            InitCollectionDataContract(this);
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal CollectionDataContract(Type type, DataContract itemContract)
            : base(new CollectionDataContractCriticalHelper(type, itemContract))
        {
            InitCollectionDataContract(this);
        }


        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        CollectionDataContract(Type type, CollectionKind kind, Type itemType, MethodInfo getEnumeratorMethod, string serializationExceptionMessage, string deserializationExceptionMessage)
            : base(new CollectionDataContractCriticalHelper(type, kind, itemType, getEnumeratorMethod, serializationExceptionMessage, deserializationExceptionMessage))
        {
            InitCollectionDataContract(GetSharedTypeContract(type));
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        CollectionDataContract(Type type, CollectionKind kind, Type itemType, MethodInfo getEnumeratorMethod, MethodInfo addMethod, ConstructorInfo constructor)
            : base(new CollectionDataContractCriticalHelper(type, kind, itemType, getEnumeratorMethod, addMethod, constructor))
        {
            InitCollectionDataContract(GetSharedTypeContract(type));
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        CollectionDataContract(Type type, CollectionKind kind, Type itemType, MethodInfo getEnumeratorMethod, MethodInfo addMethod, ConstructorInfo constructor, bool isConstructorCheckRequired)
            : base(new CollectionDataContractCriticalHelper(type, kind, itemType, getEnumeratorMethod, addMethod, constructor, isConstructorCheckRequired))
        {
            InitCollectionDataContract(GetSharedTypeContract(type));
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        CollectionDataContract(Type type, string invalidCollectionInSharedContractMessage)
            : base(new CollectionDataContractCriticalHelper(type, invalidCollectionInSharedContractMessage))
        {
            InitCollectionDataContract(GetSharedTypeContract(type));
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical fields; called from all constructors.")]
        [SecurityCritical]
        void InitCollectionDataContract(DataContract sharedTypeContract)
        {
            this.helper = base.Helper as CollectionDataContractCriticalHelper;
            this.collectionItemName = helper.CollectionItemName;
            if (helper.Kind == CollectionKind.Dictionary || helper.Kind == CollectionKind.GenericDictionary)
            {
                this.itemContract = helper.ItemContract;
            }
            this.helper.SharedTypeContract = sharedTypeContract;
        }

        void InitSharedTypeContract()
        {
        }

        static Type[] KnownInterfaces
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical knownInterfaces property.",
                Safe = "knownInterfaces only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return CollectionDataContractCriticalHelper.KnownInterfaces; }
        }

        internal CollectionKind Kind
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical kind property.",
                Safe = "kind only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.Kind; }
        }

        internal Type ItemType
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical itemType property.",
                Safe = "itemType only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.ItemType; }
        }

        public DataContract ItemContract
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical itemContract property.",
                Safe = "itemContract only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return itemContract ?? helper.ItemContract; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical itemContract property.")]
            [SecurityCritical]
            set
            {
                itemContract = value;
                helper.ItemContract = value;
            }
        }

        internal DataContract SharedTypeContract
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical sharedTypeContract property.",
                Safe = "sharedTypeContract only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.SharedTypeContract; }
        }

        internal string ItemName
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical itemName property.",
                Safe = "itemName only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.ItemName; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical itemName property.")]
            [SecurityCritical]
            set { helper.ItemName = value; }
        }

        public XmlDictionaryString CollectionItemName
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical collectionItemName property.",
                Safe = "collectionItemName only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return this.collectionItemName; }
        }

        internal string KeyName
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical keyName property.",
                Safe = "keyName only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.KeyName; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical keyName property.")]
            [SecurityCritical]
            set { helper.KeyName = value; }
        }

        internal string ValueName
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical valueName property.",
                Safe = "valueName only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.ValueName; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical valueName property.")]
            [SecurityCritical]
            set { helper.ValueName = value; }
        }

        internal bool IsDictionary
        {
            get { return KeyName != null; }
        }

        public XmlDictionaryString ChildElementNamespace
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical childElementNamespace property.",
                Safe = "childElementNamespace only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (this.childElementNamespace == null)
                {
                    lock (this)
                    {
                        if (this.childElementNamespace == null)
                        {
                            if (helper.ChildElementNamespace == null && !IsDictionary)
                            {
                                XmlDictionaryString tempChildElementNamespace = ClassDataContract.GetChildNamespaceToDeclare(this, ItemType, new XmlDictionary());
                                Thread.MemoryBarrier();
                                helper.ChildElementNamespace = tempChildElementNamespace;
                            }
                            this.childElementNamespace = helper.ChildElementNamespace;
                        }
                    }
                }
                return childElementNamespace;
            }
        }

        internal bool IsItemTypeNullable
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical isItemTypeNullable property.",
                Safe = "isItemTypeNullable only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsItemTypeNullable; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical isItemTypeNullable property.")]
            [SecurityCritical]
            set { helper.IsItemTypeNullable = value; }
        }

        internal bool IsConstructorCheckRequired
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical isConstructorCheckRequired property.",
                Safe = "isConstructorCheckRequired only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsConstructorCheckRequired; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical isConstructorCheckRequired property.")]
            [SecurityCritical]
            set { helper.IsConstructorCheckRequired = value; }
        }

        internal MethodInfo GetEnumeratorMethod
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical getEnumeratorMethod property.",
                Safe = "getEnumeratorMethod only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.GetEnumeratorMethod; }
        }

        internal MethodInfo AddMethod
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical addMethod property.",
                Safe = "addMethod only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.AddMethod; }
        }

        internal ConstructorInfo Constructor
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical constructor property.",
                Safe = "constructor only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.Constructor; }
        }

        internal override DataContractDictionary KnownDataContracts
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical knownDataContracts property.",
                Safe = "knownDataContracts only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.KnownDataContracts; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical knownDataContracts property.")]
            [SecurityCritical]
            set { helper.KnownDataContracts = value; }
        }

        internal string InvalidCollectionInSharedContractMessage
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical invalidCollectionInSharedContractMessage property.",
                Safe = "invalidCollectionInSharedContractMessage only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.InvalidCollectionInSharedContractMessage; }
        }

        internal string SerializationExceptionMessage
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical serializationExceptionMessage property.",
                Safe = "serializationExceptionMessage only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.SerializationExceptionMessage; }
        }

        internal string DeserializationExceptionMessage
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical deserializationExceptionMessage property.",
                Safe = "deserializationExceptionMessage only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.DeserializationExceptionMessage; }
        }

        internal bool IsReadOnlyContract
        {
            get { return this.DeserializationExceptionMessage != null; }
        }

        bool ItemNameSetExplicit
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical itemNameSetExplicit property.",
                Safe = "itemNameSetExplicit only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.ItemNameSetExplicit; }
        }

        internal XmlFormatCollectionWriterDelegate XmlFormatWriterDelegate
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical xmlFormatWriterDelegate property.",
                Safe = "xmlFormatWriterDelegate only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (helper.XmlFormatWriterDelegate == null)
                {
                    lock (this)
                    {
                        if (helper.XmlFormatWriterDelegate == null)
                        {
                            XmlFormatCollectionWriterDelegate tempDelegate = new XmlFormatWriterGenerator().GenerateCollectionWriter(this);
                            Thread.MemoryBarrier();
                            helper.XmlFormatWriterDelegate = tempDelegate;
                        }
                    }
                }
                return helper.XmlFormatWriterDelegate;
            }
        }

        internal XmlFormatCollectionReaderDelegate XmlFormatReaderDelegate
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical xmlFormatReaderDelegate property.",
                Safe = "xmlFormatReaderDelegate only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (helper.XmlFormatReaderDelegate == null)
                {
                    lock (this)
                    {
                        if (helper.XmlFormatReaderDelegate == null)
                        {
                            if (this.IsReadOnlyContract)
                            {
                                ThrowInvalidDataContractException(helper.DeserializationExceptionMessage, null /*type*/);
                            }
                            XmlFormatCollectionReaderDelegate tempDelegate = new XmlFormatReaderGenerator().GenerateCollectionReader(this);
                            Thread.MemoryBarrier();
                            helper.XmlFormatReaderDelegate = tempDelegate;
                        }
                    }
                }
                return helper.XmlFormatReaderDelegate;
            }
        }

        internal XmlFormatGetOnlyCollectionReaderDelegate XmlFormatGetOnlyCollectionReaderDelegate
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical xmlFormatGetOnlyCollectionReaderDelegate property.",
                Safe = "xmlFormatGetOnlyCollectionReaderDelegate only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (helper.XmlFormatGetOnlyCollectionReaderDelegate == null)
                {
                    lock (this)
                    {
                        if (helper.XmlFormatGetOnlyCollectionReaderDelegate == null)
                        {
                            if (this.UnderlyingType.IsInterface && (this.Kind == CollectionKind.Enumerable || this.Kind == CollectionKind.Collection || this.Kind == CollectionKind.GenericEnumerable))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.GetOnlyCollectionMustHaveAddMethod, DataContract.GetClrTypeFullName(this.UnderlyingType))));
                            }
                            if (this.IsReadOnlyContract)
                            {
                                ThrowInvalidDataContractException(helper.DeserializationExceptionMessage, null /*type*/);
                            }
                            Fx.Assert(this.AddMethod != null || this.Kind == CollectionKind.Array, "Add method cannot be null if the collection is being used as a get-only property");
                            XmlFormatGetOnlyCollectionReaderDelegate tempDelegate = new XmlFormatReaderGenerator().GenerateGetOnlyCollectionReader(this);
                            Thread.MemoryBarrier();
                            helper.XmlFormatGetOnlyCollectionReaderDelegate = tempDelegate;
                        }
                    }
                }
                return helper.XmlFormatGetOnlyCollectionReaderDelegate;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds all state used for (de)serializing collections. Since the data is cached statically, we lock down access to it.")]
        [SecurityCritical(SecurityCriticalScope.Everything)]
        class CollectionDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            static Type[] _knownInterfaces;

            Type itemType;
            bool isItemTypeNullable;
            CollectionKind kind;
            readonly MethodInfo getEnumeratorMethod, addMethod;
            readonly ConstructorInfo constructor;
            readonly string serializationExceptionMessage, deserializationExceptionMessage;
            DataContract itemContract;
            DataContract sharedTypeContract;
            DataContractDictionary knownDataContracts;
            bool isKnownTypeAttributeChecked;
            string itemName;
            bool itemNameSetExplicit;
            XmlDictionaryString collectionItemName;
            string keyName;
            string valueName;
            XmlDictionaryString childElementNamespace;
            string invalidCollectionInSharedContractMessage;
            XmlFormatCollectionReaderDelegate xmlFormatReaderDelegate;
            XmlFormatGetOnlyCollectionReaderDelegate xmlFormatGetOnlyCollectionReaderDelegate;
            XmlFormatCollectionWriterDelegate xmlFormatWriterDelegate;
            bool isConstructorCheckRequired = false;

            internal static Type[] KnownInterfaces
            {
                get
                {
                    if (_knownInterfaces == null)
                    {
                        // Listed in priority order
                        _knownInterfaces = new Type[]
                    {
                        Globals.TypeOfIDictionaryGeneric,
                        Globals.TypeOfIDictionary,
                        Globals.TypeOfIListGeneric,
                        Globals.TypeOfICollectionGeneric,
                        Globals.TypeOfIList,
                        Globals.TypeOfIEnumerableGeneric,
                        Globals.TypeOfICollection,
                        Globals.TypeOfIEnumerable
                    };
                    }
                    return _knownInterfaces;
                }
            }

            void Init(CollectionKind kind, Type itemType, CollectionDataContractAttribute collectionContractAttribute)
            {
                this.kind = kind;
                if (itemType != null)
                {
                    this.itemType = itemType;
                    this.isItemTypeNullable = DataContract.IsTypeNullable(itemType);

                    bool isDictionary = (kind == CollectionKind.Dictionary || kind == CollectionKind.GenericDictionary);
                    string itemName = null, keyName = null, valueName = null;
                    if (collectionContractAttribute != null)
                    {
                        if (collectionContractAttribute.IsItemNameSetExplicit)
                        {
                            if (collectionContractAttribute.ItemName == null || collectionContractAttribute.ItemName.Length == 0)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidCollectionContractItemName, DataContract.GetClrTypeFullName(UnderlyingType))));
                            itemName = DataContract.EncodeLocalName(collectionContractAttribute.ItemName);
                            itemNameSetExplicit = true;
                        }
                        if (collectionContractAttribute.IsKeyNameSetExplicit)
                        {
                            if (collectionContractAttribute.KeyName == null || collectionContractAttribute.KeyName.Length == 0)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidCollectionContractKeyName, DataContract.GetClrTypeFullName(UnderlyingType))));
                            if (!isDictionary)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidCollectionContractKeyNoDictionary, DataContract.GetClrTypeFullName(UnderlyingType), collectionContractAttribute.KeyName)));
                            keyName = DataContract.EncodeLocalName(collectionContractAttribute.KeyName);
                        }
                        if (collectionContractAttribute.IsValueNameSetExplicit)
                        {
                            if (collectionContractAttribute.ValueName == null || collectionContractAttribute.ValueName.Length == 0)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidCollectionContractValueName, DataContract.GetClrTypeFullName(UnderlyingType))));
                            if (!isDictionary)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.InvalidCollectionContractValueNoDictionary, DataContract.GetClrTypeFullName(UnderlyingType), collectionContractAttribute.ValueName)));
                            valueName = DataContract.EncodeLocalName(collectionContractAttribute.ValueName);
                        }
                    }

                    XmlDictionary dictionary = isDictionary ? new XmlDictionary(5) : new XmlDictionary(3);
                    this.Name = dictionary.Add(this.StableName.Name);
                    this.Namespace = dictionary.Add(this.StableName.Namespace);
                    this.itemName = itemName ?? DataContract.GetStableName(DataContract.UnwrapNullableType(itemType)).Name;
                    this.collectionItemName = dictionary.Add(this.itemName);
                    if (isDictionary)
                    {
                        this.keyName = keyName ?? Globals.KeyLocalName;
                        this.valueName = valueName ?? Globals.ValueLocalName;
                    }
                }
                if (collectionContractAttribute != null)
                {
                    this.IsReference = collectionContractAttribute.IsReference;
                }
            }

            internal CollectionDataContractCriticalHelper(CollectionKind kind)
                : base()
            {
                Init(kind, null, null);
            }

            // array
            internal CollectionDataContractCriticalHelper(Type type)
                : base(type)
            {
                if (type == Globals.TypeOfArray)
                    type = Globals.TypeOfObjectArray;
                if (type.GetArrayRank() > 1)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SupportForMultidimensionalArraysNotPresent)));
                this.StableName = DataContract.GetStableName(type);
                Init(CollectionKind.Array, type.GetElementType(), null);
            }

            // array
            internal CollectionDataContractCriticalHelper(Type type, DataContract itemContract)
                : base(type)
            {
                if (type.GetArrayRank() > 1)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SupportForMultidimensionalArraysNotPresent)));
                this.StableName = CreateQualifiedName(Globals.ArrayPrefix + itemContract.StableName.Name, itemContract.StableName.Namespace);
                this.itemContract = itemContract;
                Init(CollectionKind.Array, type.GetElementType(), null);
            }

            // read-only collection
            internal CollectionDataContractCriticalHelper(Type type, CollectionKind kind, Type itemType, MethodInfo getEnumeratorMethod, string serializationExceptionMessage, string deserializationExceptionMessage)
                : base(type)
            {
                if (getEnumeratorMethod == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.CollectionMustHaveGetEnumeratorMethod, DataContract.GetClrTypeFullName(type))));
                if (itemType == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.CollectionMustHaveItemType, DataContract.GetClrTypeFullName(type))));

                CollectionDataContractAttribute collectionContractAttribute;
                this.StableName = DataContract.GetCollectionStableName(type, itemType, out collectionContractAttribute);

                Init(kind, itemType, collectionContractAttribute);
                this.getEnumeratorMethod = getEnumeratorMethod;
                this.serializationExceptionMessage = serializationExceptionMessage;
                this.deserializationExceptionMessage = deserializationExceptionMessage;
            }

            // collection
            internal CollectionDataContractCriticalHelper(Type type, CollectionKind kind, Type itemType, MethodInfo getEnumeratorMethod, MethodInfo addMethod, ConstructorInfo constructor)
                : this(type, kind, itemType, getEnumeratorMethod, (string)null, (string)null)
            {
                if (addMethod == null && !type.IsInterface)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.CollectionMustHaveAddMethod, DataContract.GetClrTypeFullName(type))));
                this.addMethod = addMethod;
                this.constructor = constructor;
            }

            // collection
            internal CollectionDataContractCriticalHelper(Type type, CollectionKind kind, Type itemType, MethodInfo getEnumeratorMethod, MethodInfo addMethod, ConstructorInfo constructor, bool isConstructorCheckRequired)
                : this(type, kind, itemType, getEnumeratorMethod, addMethod, constructor)
            {
                this.isConstructorCheckRequired = isConstructorCheckRequired;
            }

            internal CollectionDataContractCriticalHelper(Type type, string invalidCollectionInSharedContractMessage)
                : base(type)
            {
                Init(CollectionKind.Collection, null /*itemType*/, null);
                this.invalidCollectionInSharedContractMessage = invalidCollectionInSharedContractMessage;
            }

            internal CollectionKind Kind
            {
                get { return kind; }
            }

            internal Type ItemType
            {
                get { return itemType; }
            }

            internal DataContract ItemContract
            {
                get
                {
                    if (itemContract == null && UnderlyingType != null)
                    {
                        if (IsDictionary)
                        {
                            if (String.CompareOrdinal(KeyName, ValueName) == 0)
                            {
                                DataContract.ThrowInvalidDataContractException(
                                    SR.GetString(SR.DupKeyValueName, DataContract.GetClrTypeFullName(UnderlyingType), KeyName),
                                    UnderlyingType);
                            }
                            itemContract = ClassDataContract.CreateClassDataContractForKeyValue(ItemType, Namespace, new string[] { KeyName, ValueName });
                            // Ensure that DataContract gets added to the static DataContract cache for dictionary items
                            DataContract.GetDataContract(ItemType);
                        }
                        else
                        {
                            itemContract = DataContract.GetDataContract(ItemType);
                        }
                    }
                    return itemContract;
                }
                set
                {
                    itemContract = value;
                }
            }

            internal DataContract SharedTypeContract
            {
                get { return sharedTypeContract; }
                set { sharedTypeContract = value; }
            }

            internal string ItemName
            {
                get { return itemName; }
                set { itemName = value; }
            }

            internal bool IsConstructorCheckRequired
            {
                get { return isConstructorCheckRequired; }
                set { isConstructorCheckRequired = value; }
            }

            public XmlDictionaryString CollectionItemName
            {
                get { return collectionItemName; }
            }

            internal string KeyName
            {
                get { return keyName; }
                set { keyName = value; }
            }

            internal string ValueName
            {
                get { return valueName; }
                set { valueName = value; }
            }

            internal bool IsDictionary
            {
                get { return KeyName != null; }
            }

            public string SerializationExceptionMessage
            {
                get { return serializationExceptionMessage; }
            }

            public string DeserializationExceptionMessage
            {
                get { return deserializationExceptionMessage; }
            }

            public XmlDictionaryString ChildElementNamespace
            {
                get { return childElementNamespace; }
                set { childElementNamespace = value; }
            }

            internal bool IsItemTypeNullable
            {
                get { return isItemTypeNullable; }
                set { isItemTypeNullable = value; }
            }

            internal MethodInfo GetEnumeratorMethod
            {
                get { return getEnumeratorMethod; }
            }

            internal MethodInfo AddMethod
            {
                get { return addMethod; }
            }

            internal ConstructorInfo Constructor
            {
                get { return constructor; }
            }

            internal override DataContractDictionary KnownDataContracts
            {
                get
                {
                    if (!isKnownTypeAttributeChecked && UnderlyingType != null)
                    {
                        lock (this)
                        {
                            if (!isKnownTypeAttributeChecked)
                            {
                                knownDataContracts = DataContract.ImportKnownTypeAttributes(this.UnderlyingType);
                                Thread.MemoryBarrier();
                                isKnownTypeAttributeChecked = true;
                            }
                        }
                    }
                    return knownDataContracts;
                }
                set { knownDataContracts = value; }
            }

            internal string InvalidCollectionInSharedContractMessage
            {
                get { return invalidCollectionInSharedContractMessage; }
            }

            internal bool ItemNameSetExplicit
            {
                get { return itemNameSetExplicit; }
            }

            internal XmlFormatCollectionWriterDelegate XmlFormatWriterDelegate
            {
                get { return xmlFormatWriterDelegate; }
                set { xmlFormatWriterDelegate = value; }
            }

            internal XmlFormatCollectionReaderDelegate XmlFormatReaderDelegate
            {
                get { return xmlFormatReaderDelegate; }
                set { xmlFormatReaderDelegate = value; }
            }

            internal XmlFormatGetOnlyCollectionReaderDelegate XmlFormatGetOnlyCollectionReaderDelegate
            {
                get { return xmlFormatGetOnlyCollectionReaderDelegate; }
                set { xmlFormatGetOnlyCollectionReaderDelegate = value; }
            }
        }

        DataContract GetSharedTypeContract(Type type)
        {
            if (type.IsDefined(Globals.TypeOfCollectionDataContractAttribute, false))
            {
                return this;
            }
            // ClassDataContract.IsNonAttributedTypeValidForSerialization does not need to be called here. It should
            // never pass because it returns false for types that implement any of CollectionDataContract.KnownInterfaces
            if (type.IsSerializable || type.IsDefined(Globals.TypeOfDataContractAttribute, false))
            {
                return new ClassDataContract(type);
            }
            return null;
        }

        internal static bool IsCollectionInterface(Type type)
        {
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();
            return ((IList<Type>)KnownInterfaces).Contains(type);
        }

        internal static bool IsCollection(Type type)
        {
            Type itemType;
            return IsCollection(type, out itemType);
        }

        internal static bool IsCollection(Type type, out Type itemType)
        {
            return IsCollectionHelper(type, out itemType, true /*constructorRequired*/);
        }

        internal static bool IsCollection(Type type, bool constructorRequired, bool skipIfReadOnlyContract)
        {
            Type itemType;
            return IsCollectionHelper(type, out itemType, constructorRequired, skipIfReadOnlyContract);
        }

        static bool IsCollectionHelper(Type type, out Type itemType, bool constructorRequired, bool skipIfReadOnlyContract = false)
        {
            if (type.IsArray && DataContract.GetBuiltInDataContract(type) == null)
            {
                itemType = type.GetElementType();
                return true;
            }
            DataContract dataContract;
            return IsCollectionOrTryCreate(type, false /*tryCreate*/, out dataContract, out itemType, constructorRequired, skipIfReadOnlyContract);
        }

        internal static bool TryCreate(Type type, out DataContract dataContract)
        {
            Type itemType;
            return IsCollectionOrTryCreate(type, true /*tryCreate*/, out dataContract, out itemType, true /*constructorRequired*/);
        }

        internal static bool TryCreateGetOnlyCollectionDataContract(Type type, out DataContract dataContract)
        {
            Type itemType;
            if (type.IsArray)
            {
                dataContract = new CollectionDataContract(type);
                return true;
            }
            else
            {
                return IsCollectionOrTryCreate(type, true /*tryCreate*/, out dataContract, out itemType, false /*constructorRequired*/);
            }
        }

        internal static MethodInfo GetTargetMethodWithName(string name, Type type, Type interfaceType)
        {
            InterfaceMapping mapping = type.GetInterfaceMap(interfaceType);
            for (int i = 0; i < mapping.TargetMethods.Length; i++)
            {
                if (mapping.InterfaceMethods[i].Name == name)
                    return mapping.InterfaceMethods[i];
            }
            return null;
        }

        static bool IsArraySegment(Type t)
        {
            return t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(ArraySegment<>));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.Globalization, FxCop.Rule.DoNotPassLiteralsAsLocalizedParameters, Justification = "Private code.")]
        static bool IsCollectionOrTryCreate(Type type, bool tryCreate, out DataContract dataContract, out Type itemType, bool constructorRequired, bool skipIfReadOnlyContract = false)
        {
            dataContract = null;
            itemType = Globals.TypeOfObject;

            if (DataContract.GetBuiltInDataContract(type) != null)
            {
                return HandleIfInvalidCollection(type, tryCreate, false/*hasCollectionDataContract*/, false/*isBaseTypeCollection*/,
                    SR.CollectionTypeCannotBeBuiltIn, null, ref dataContract);
            }
            MethodInfo addMethod, getEnumeratorMethod;
            bool hasCollectionDataContract = IsCollectionDataContract(type);
            bool isReadOnlyContract = false;
            string serializationExceptionMessage = null, deserializationExceptionMessage = null;
            Type baseType = type.BaseType;
            bool isBaseTypeCollection = (baseType != null && baseType != Globals.TypeOfObject
                && baseType != Globals.TypeOfValueType && baseType != Globals.TypeOfUri) ? IsCollection(baseType) : false;

            // Avoid creating an invalid collection contract for Serializable types since we can create a ClassDataContract instead
            bool createContractWithException = isBaseTypeCollection && !type.IsSerializable;

            if (type.IsDefined(Globals.TypeOfDataContractAttribute, false))
            {
                return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException,
                    SR.CollectionTypeCannotHaveDataContract, null, ref dataContract);
            }

            if (Globals.TypeOfIXmlSerializable.IsAssignableFrom(type) || IsArraySegment(type))
            {
                return false;
            }

            if (!Globals.TypeOfIEnumerable.IsAssignableFrom(type))
            {
                return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException,
                    SR.CollectionTypeIsNotIEnumerable, null, ref dataContract);
            }
            if (type.IsInterface)
            {
                Type interfaceTypeToCheck = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                Type[] knownInterfaces = KnownInterfaces;
                for (int i = 0; i < knownInterfaces.Length; i++)
                {
                    if (knownInterfaces[i] == interfaceTypeToCheck)
                    {
                        addMethod = null;
                        if (type.IsGenericType)
                        {
                            Type[] genericArgs = type.GetGenericArguments();
                            if (interfaceTypeToCheck == Globals.TypeOfIDictionaryGeneric)
                            {
                                itemType = Globals.TypeOfKeyValue.MakeGenericType(genericArgs);
                                addMethod = type.GetMethod(Globals.AddMethodName);
                                getEnumeratorMethod = Globals.TypeOfIEnumerableGeneric.MakeGenericType(Globals.TypeOfKeyValuePair.MakeGenericType(genericArgs)).GetMethod(Globals.GetEnumeratorMethodName);
                            }
                            else
                            {
                                itemType = genericArgs[0];
                                if (interfaceTypeToCheck == Globals.TypeOfICollectionGeneric || interfaceTypeToCheck == Globals.TypeOfIListGeneric)
                                {
                                    addMethod = Globals.TypeOfICollectionGeneric.MakeGenericType(itemType).GetMethod(Globals.AddMethodName);
                                }
                                getEnumeratorMethod = Globals.TypeOfIEnumerableGeneric.MakeGenericType(itemType).GetMethod(Globals.GetEnumeratorMethodName);
                            }
                        }
                        else
                        {
                            if (interfaceTypeToCheck == Globals.TypeOfIDictionary)
                            {
                                itemType = typeof(KeyValue<object, object>);
                                addMethod = type.GetMethod(Globals.AddMethodName);
                            }
                            else
                            {
                                itemType = Globals.TypeOfObject;
                                if (interfaceTypeToCheck == Globals.TypeOfIList)
                                {
                                    addMethod = Globals.TypeOfIList.GetMethod(Globals.AddMethodName);
                                }
                            }
                            getEnumeratorMethod = Globals.TypeOfIEnumerable.GetMethod(Globals.GetEnumeratorMethodName);
                        }
                        if (tryCreate)
                            dataContract = new CollectionDataContract(type, (CollectionKind)(i + 1), itemType, getEnumeratorMethod, addMethod, null/*defaultCtor*/);
                        return true;
                    }
                }
            }
            ConstructorInfo defaultCtor = null;
            if (!type.IsValueType)
            {
                defaultCtor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Globals.EmptyTypeArray, null);
                if (defaultCtor == null && constructorRequired)
                {
                    // All collection types could be considered read-only collections except collection types that are marked [Serializable]. 
                    // Collection types marked [Serializable] cannot be read-only collections for backward compatibility reasons.
                    // DataContract types and POCO types cannot be collection types, so they don't need to be factored in
                    if (type.IsSerializable)
                    {
                        return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException,
                            SR.CollectionTypeDoesNotHaveDefaultCtor, null, ref dataContract);
                    }
                    else
                    {
                        isReadOnlyContract = true;
                        GetReadOnlyCollectionExceptionMessages(type, hasCollectionDataContract, SR.CollectionTypeDoesNotHaveDefaultCtor, null, out serializationExceptionMessage, out deserializationExceptionMessage);
                    }
                }
            }

            Type knownInterfaceType = null;
            CollectionKind kind = CollectionKind.None;
            bool multipleDefinitions = false;
            Type[] interfaceTypes = type.GetInterfaces();
            foreach (Type interfaceType in interfaceTypes)
            {
                Type interfaceTypeToCheck = interfaceType.IsGenericType ? interfaceType.GetGenericTypeDefinition() : interfaceType;
                Type[] knownInterfaces = KnownInterfaces;
                for (int i = 0; i < knownInterfaces.Length; i++)
                {
                    if (knownInterfaces[i] == interfaceTypeToCheck)
                    {
                        CollectionKind currentKind = (CollectionKind)(i + 1);
                        if (kind == CollectionKind.None || currentKind < kind)
                        {
                            kind = currentKind;
                            knownInterfaceType = interfaceType;
                            multipleDefinitions = false;
                        }
                        else if ((kind & currentKind) == currentKind)
                            multipleDefinitions = true;
                        break;
                    }
                }
            }

            if (kind == CollectionKind.None)
            {
                return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException,
                    SR.CollectionTypeIsNotIEnumerable, null, ref dataContract);
            }

            if (kind == CollectionKind.Enumerable || kind == CollectionKind.Collection || kind == CollectionKind.GenericEnumerable)
            {
                if (multipleDefinitions)
                    knownInterfaceType = Globals.TypeOfIEnumerable;
                itemType = knownInterfaceType.IsGenericType ? knownInterfaceType.GetGenericArguments()[0] : Globals.TypeOfObject;
                GetCollectionMethods(type, knownInterfaceType, new Type[] { itemType },
                                     false /*addMethodOnInterface*/,
                                     out getEnumeratorMethod, out addMethod);
                if (addMethod == null)
                {
                    // All collection types could be considered read-only collections except collection types that are marked [Serializable]. 
                    // Collection types marked [Serializable] cannot be read-only collections for backward compatibility reasons.
                    // DataContract types and POCO types cannot be collection types, so they don't need to be factored in.
                    if (type.IsSerializable || skipIfReadOnlyContract)
                    {
                        return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException && !skipIfReadOnlyContract,
                            SR.CollectionTypeDoesNotHaveAddMethod, DataContract.GetClrTypeFullName(itemType), ref dataContract);
                    }
                    else
                    {
                        isReadOnlyContract = true;
                        GetReadOnlyCollectionExceptionMessages(type, hasCollectionDataContract, SR.CollectionTypeDoesNotHaveAddMethod, DataContract.GetClrTypeFullName(itemType), out serializationExceptionMessage, out deserializationExceptionMessage);
                    }
                }

                if (tryCreate)
                {
                    dataContract = isReadOnlyContract ?
                        new CollectionDataContract(type, kind, itemType, getEnumeratorMethod, serializationExceptionMessage, deserializationExceptionMessage) :
                        new CollectionDataContract(type, kind, itemType, getEnumeratorMethod, addMethod, defaultCtor, !constructorRequired);
                }
            }
            else
            {
                if (multipleDefinitions)
                {
                    return HandleIfInvalidCollection(type, tryCreate, hasCollectionDataContract, createContractWithException,
                        SR.CollectionTypeHasMultipleDefinitionsOfInterface, KnownInterfaces[(int)kind - 1].Name, ref dataContract);
                }
                Type[] addMethodTypeArray = null;
                switch (kind)
                {
                    case CollectionKind.GenericDictionary:
                        addMethodTypeArray = knownInterfaceType.GetGenericArguments();
                        bool isOpenGeneric = knownInterfaceType.IsGenericTypeDefinition
                            || (addMethodTypeArray[0].IsGenericParameter && addMethodTypeArray[1].IsGenericParameter);
                        itemType = isOpenGeneric ? Globals.TypeOfKeyValue : Globals.TypeOfKeyValue.MakeGenericType(addMethodTypeArray);
                        break;
                    case CollectionKind.Dictionary:
                        addMethodTypeArray = new Type[] { Globals.TypeOfObject, Globals.TypeOfObject };
                        itemType = Globals.TypeOfKeyValue.MakeGenericType(addMethodTypeArray);
                        break;
                    case CollectionKind.GenericList:
                    case CollectionKind.GenericCollection:
                        addMethodTypeArray = knownInterfaceType.GetGenericArguments();
                        itemType = addMethodTypeArray[0];
                        break;
                    case CollectionKind.List:
                        itemType = Globals.TypeOfObject;
                        addMethodTypeArray = new Type[] { itemType };
                        break;
                }

                if (tryCreate)
                {
                    GetCollectionMethods(type, knownInterfaceType, addMethodTypeArray,
                                     true /*addMethodOnInterface*/,
                                     out getEnumeratorMethod, out addMethod);
                    dataContract = isReadOnlyContract ?
                        new CollectionDataContract(type, kind, itemType, getEnumeratorMethod, serializationExceptionMessage, deserializationExceptionMessage) :
                        new CollectionDataContract(type, kind, itemType, getEnumeratorMethod, addMethod, defaultCtor, !constructorRequired);
                }
            }

            return !(isReadOnlyContract && skipIfReadOnlyContract);
        }

        internal static bool IsCollectionDataContract(Type type)
        {
            return type.IsDefined(Globals.TypeOfCollectionDataContractAttribute, false);
        }

        static bool HandleIfInvalidCollection(Type type, bool tryCreate, bool hasCollectionDataContract, bool createContractWithException, string message, string param, ref DataContract dataContract)
        {
            if (hasCollectionDataContract)
            {
                if (tryCreate)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(GetInvalidCollectionMessage(message, SR.GetString(SR.InvalidCollectionDataContract, DataContract.GetClrTypeFullName(type)), param)));
                return true;
            }

            if (createContractWithException)
            {
                if (tryCreate)
                    dataContract = new CollectionDataContract(type, GetInvalidCollectionMessage(message, SR.GetString(SR.InvalidCollectionType, DataContract.GetClrTypeFullName(type)), param));
                return true;
            }

            return false;
        }

        static void GetReadOnlyCollectionExceptionMessages(Type type, bool hasCollectionDataContract, string message, string param, out string serializationExceptionMessage, out string deserializationExceptionMessage)
        {
            serializationExceptionMessage = GetInvalidCollectionMessage(message, SR.GetString(hasCollectionDataContract ? SR.InvalidCollectionDataContract : SR.InvalidCollectionType, DataContract.GetClrTypeFullName(type)), param);
            deserializationExceptionMessage = GetInvalidCollectionMessage(message, SR.GetString(SR.ReadOnlyCollectionDeserialization, DataContract.GetClrTypeFullName(type)), param);
        }

        static string GetInvalidCollectionMessage(string message, string nestedMessage, string param)
        {
            return (param == null) ? SR.GetString(message, nestedMessage) : SR.GetString(message, nestedMessage, param);
        }

        static void FindCollectionMethodsOnInterface(Type type, Type interfaceType, ref MethodInfo addMethod, ref MethodInfo getEnumeratorMethod)
        {
            InterfaceMapping mapping = type.GetInterfaceMap(interfaceType);
            for (int i = 0; i < mapping.TargetMethods.Length; i++)
            {
                if (mapping.InterfaceMethods[i].Name == Globals.AddMethodName)
                    addMethod = mapping.InterfaceMethods[i];
                else if (mapping.InterfaceMethods[i].Name == Globals.GetEnumeratorMethodName)
                    getEnumeratorMethod = mapping.InterfaceMethods[i];
            }
        }

        static void GetCollectionMethods(Type type, Type interfaceType, Type[] addMethodTypeArray, bool addMethodOnInterface, out MethodInfo getEnumeratorMethod, out MethodInfo addMethod)
        {
            addMethod = getEnumeratorMethod = null;

            if (addMethodOnInterface)
            {
                addMethod = type.GetMethod(Globals.AddMethodName, BindingFlags.Instance | BindingFlags.Public, null, addMethodTypeArray, null);
                if (addMethod == null || addMethod.GetParameters()[0].ParameterType != addMethodTypeArray[0])
                {
                    FindCollectionMethodsOnInterface(type, interfaceType, ref addMethod, ref getEnumeratorMethod);
                    if (addMethod == null)
                    {
                        Type[] parentInterfaceTypes = interfaceType.GetInterfaces();
                        foreach (Type parentInterfaceType in parentInterfaceTypes)
                        {
                            if (IsKnownInterface(parentInterfaceType))
                            {
                                FindCollectionMethodsOnInterface(type, parentInterfaceType, ref addMethod, ref getEnumeratorMethod);
                                if (addMethod == null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // GetMethod returns Add() method with parameter closest matching T in assignability/inheritance chain
                addMethod = type.GetMethod(Globals.AddMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, addMethodTypeArray, null);
            }

            if (getEnumeratorMethod == null)
            {
                getEnumeratorMethod = type.GetMethod(Globals.GetEnumeratorMethodName, BindingFlags.Instance | BindingFlags.Public, null, Globals.EmptyTypeArray, null);
                if (getEnumeratorMethod == null || !Globals.TypeOfIEnumerator.IsAssignableFrom(getEnumeratorMethod.ReturnType))
                {
                    Type ienumerableInterface = interfaceType.GetInterface("System.Collections.Generic.IEnumerable*");
                    if (ienumerableInterface == null)
                        ienumerableInterface = Globals.TypeOfIEnumerable;
                    getEnumeratorMethod = GetTargetMethodWithName(Globals.GetEnumeratorMethodName, type, ienumerableInterface);
                }
            }
        }

        static bool IsKnownInterface(Type type)
        {
            Type typeToCheck = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            foreach (Type knownInterfaceType in KnownInterfaces)
            {
                if (typeToCheck == knownInterfaceType)
                {
                    return true;
                }
            }
            return false;
        }

        [Fx.Tag.SecurityNote(Critical = "Sets critical properties on CollectionDataContract .",
            Safe = "Called during schema import/code generation.")]
        [SecuritySafeCritical]
        internal override DataContract BindGenericParameters(DataContract[] paramContracts, Dictionary<DataContract, DataContract> boundContracts)
        {
            DataContract boundContract;
            if (boundContracts.TryGetValue(this, out boundContract))
                return boundContract;

            CollectionDataContract boundCollectionContract = new CollectionDataContract(Kind);
            boundContracts.Add(this, boundCollectionContract);
            boundCollectionContract.ItemContract = this.ItemContract.BindGenericParameters(paramContracts, boundContracts);
            boundCollectionContract.IsItemTypeNullable = !boundCollectionContract.ItemContract.IsValueType;
            boundCollectionContract.ItemName = ItemNameSetExplicit ? this.ItemName : boundCollectionContract.ItemContract.StableName.Name;
            boundCollectionContract.KeyName = this.KeyName;
            boundCollectionContract.ValueName = this.ValueName;
            boundCollectionContract.StableName = CreateQualifiedName(DataContract.ExpandGenericParameters(XmlConvert.DecodeName(this.StableName.Name), new GenericNameProvider(DataContract.GetClrTypeFullName(this.UnderlyingType), paramContracts)),
                IsCollectionDataContract(UnderlyingType) ? this.StableName.Namespace : DataContract.GetCollectionNamespace(boundCollectionContract.ItemContract.StableName.Namespace));
            return boundCollectionContract;
        }

        internal override DataContract GetValidContract(SerializationMode mode)
        {
            if (mode == SerializationMode.SharedType)
            {
                if (SharedTypeContract == null)
                    DataContract.ThrowTypeNotSerializable(UnderlyingType);
                return SharedTypeContract;
            }

            ThrowIfInvalid();
            return this;
        }

        void ThrowIfInvalid()
        {
            if (InvalidCollectionInSharedContractMessage != null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(InvalidCollectionInSharedContractMessage));
        }

        internal override DataContract GetValidContract()
        {
            if (this.IsConstructorCheckRequired)
            {
                CheckConstructor();
            }
            return this;
        }

        [Fx.Tag.SecurityNote(Critical = "Sets the critical IsConstructorCheckRequired property on CollectionDataContract.",
            Safe = "Does not leak anything.")]
        [SecuritySafeCritical]
        void CheckConstructor()
        {
            if (this.Constructor == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.CollectionTypeDoesNotHaveDefaultCtor, DataContract.GetClrTypeFullName(this.UnderlyingType))));
            }
            else
            {
                this.IsConstructorCheckRequired = false;
            }
        }

        internal override bool IsValidContract(SerializationMode mode)
        {
            if (mode == SerializationMode.SharedType)
                return (SharedTypeContract != null);
            return (InvalidCollectionInSharedContractMessage == null);
        }

        [Fx.Tag.SecurityNote(Miscellaneous =
            "RequiresReview - Calculates whether this collection requires MemberAccessPermission for deserialization."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        internal bool RequiresMemberAccessForRead(SecurityException securityException)
        {
            if (!IsTypeVisible(UnderlyingType))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityException(SR.GetString(
                                SR.PartialTrustCollectionContractTypeNotPublic,
                                DataContract.GetClrTypeFullName(UnderlyingType)),
                            securityException));
                }
                return true;
            }
            if (ItemType != null && !IsTypeVisible(ItemType))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityException(SR.GetString(
                                SR.PartialTrustCollectionContractTypeNotPublic,
                                DataContract.GetClrTypeFullName(ItemType)),
                            securityException));
                }
                return true;
            }
            if (ConstructorRequiresMemberAccess(Constructor))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityException(SR.GetString(
                                SR.PartialTrustCollectionContractNoPublicConstructor,
                                DataContract.GetClrTypeFullName(UnderlyingType)),
                            securityException));
                }
                return true;
            }
            if (MethodRequiresMemberAccess(this.AddMethod))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                           new SecurityException(SR.GetString(
                                   SR.PartialTrustCollectionContractAddMethodNotPublic,
                                   DataContract.GetClrTypeFullName(UnderlyingType),
                                   this.AddMethod.Name),
                               securityException));
                }
                return true;
            }

            return false;
        }

        [Fx.Tag.SecurityNote(Miscellaneous =
            "RequiresReview - Calculates whether this collection requires MemberAccessPermission for serialization."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        internal bool RequiresMemberAccessForWrite(SecurityException securityException)
        {
            if (!IsTypeVisible(UnderlyingType))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityException(SR.GetString(
                                SR.PartialTrustCollectionContractTypeNotPublic,
                                DataContract.GetClrTypeFullName(UnderlyingType)),
                            securityException));
                }
                return true;
            }
            if (ItemType != null && !IsTypeVisible(ItemType))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityException(SR.GetString(
                                SR.PartialTrustCollectionContractTypeNotPublic,
                                DataContract.GetClrTypeFullName(ItemType)),
                            securityException));
                }
                return true;
            }

            return false;
        }

        internal override bool Equals(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            if (IsEqualOrChecked(other, checkedContracts))
                return true;

            if (base.Equals(other, checkedContracts))
            {
                CollectionDataContract dataContract = other as CollectionDataContract;
                if (dataContract != null)
                {
                    bool thisItemTypeIsNullable = (ItemContract == null) ? false : !ItemContract.IsValueType;
                    bool otherItemTypeIsNullable = (dataContract.ItemContract == null) ? false : !dataContract.ItemContract.IsValueType;
                    return ItemName == dataContract.ItemName &&
                        (IsItemTypeNullable || thisItemTypeIsNullable) == (dataContract.IsItemTypeNullable || otherItemTypeIsNullable) &&
                        ItemContract.Equals(dataContract.ItemContract, checkedContracts);
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void WriteXmlValue(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context)
        {
            // IsGetOnlyCollection value has already been used to create current collectiondatacontract, value can now be reset. 
            context.IsGetOnlyCollection = false;
            XmlFormatWriterDelegate(xmlWriter, obj, context, this);
        }

        public override object ReadXmlValue(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context)
        {
            xmlReader.Read();
            object o = null;
            if (context.IsGetOnlyCollection)
            {
                // IsGetOnlyCollection value has already been used to create current collectiondatacontract, value can now be reset. 
                context.IsGetOnlyCollection = false;
                XmlFormatGetOnlyCollectionReaderDelegate(xmlReader, context, CollectionItemName, Namespace, this);
            }
            else
            {
                o = XmlFormatReaderDelegate(xmlReader, context, CollectionItemName, Namespace, this);
            }
            xmlReader.ReadEndElement();
            return o;
        }

        public class DictionaryEnumerator : IEnumerator<KeyValue<object, object>>
        {
            IDictionaryEnumerator enumerator;

            public DictionaryEnumerator(IDictionaryEnumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public KeyValue<object, object> Current
            {
                get { return new KeyValue<object, object>(enumerator.Key, enumerator.Value); }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public void Reset()
            {
                enumerator.Reset();
            }
        }

        public class GenericDictionaryEnumerator<K, V> : IEnumerator<KeyValue<K, V>>
        {
            IEnumerator<KeyValuePair<K, V>> enumerator;

            public GenericDictionaryEnumerator(IEnumerator<KeyValuePair<K, V>> enumerator)
            {
                this.enumerator = enumerator;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public KeyValue<K, V> Current
            {
                get
                {
                    KeyValuePair<K, V> current = enumerator.Current;
                    return new KeyValue<K, V>(current.Key, current.Value);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public void Reset()
            {
                enumerator.Reset();
            }
        }

    }
}
