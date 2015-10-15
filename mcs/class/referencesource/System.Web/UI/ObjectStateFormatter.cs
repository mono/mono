//------------------------------------------------------------------------------
// <copyright file="ObjectStateFormatter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Util;
    using System.Web.Management;
    using System.Web.UI.WebControls;
    using System.Web.Security.Cryptography;

    // 



    /// <devdoc>
    /// ObjectStateFormatter is designed to efficiently serialize arbitrary object graphs
    /// that represent the state of an object (decomposed into simpler types) into
    /// a highly compact binary or ASCII representations.
    /// The formatter contains native support for optimized serialization of a fixed
    /// set of known types such as ints, shorts, booleans, strings, other primitive types
    /// arrays, Pairs, Triplets, ArrayLists, Hashtables etc. In addition it utilizes
    /// TypeConverters for semi-optimized serialization of custom types. Finally, it uses
    /// binary serialization as a fallback mechanism. The formatter is also able to compress
    /// IndexedStrings contained in the object graph.
    /// </devdoc>
    public sealed class ObjectStateFormatter : IStateFormatter, IStateFormatter2, IFormatter {

        // Optimized type tokens
        private const byte Token_Int16 = 1;
        private const byte Token_Int32 = 2;
        private const byte Token_Byte = 3;
        private const byte Token_Char = 4;
        private const byte Token_String = 5;
        private const byte Token_DateTime = 6;
        private const byte Token_Double = 7;
        private const byte Token_Single = 8;
        private const byte Token_Color = 9;
        private const byte Token_KnownColor = 10;
        private const byte Token_IntEnum = 11;
        private const byte Token_EmptyColor = 12;
        private const byte Token_Pair = 15;
        private const byte Token_Triplet = 16;
        private const byte Token_Array = 20;
        private const byte Token_StringArray = 21;
        private const byte Token_ArrayList = 22;
        private const byte Token_Hashtable = 23;
        private const byte Token_HybridDictionary = 24;
        private const byte Token_Type = 25;
        // private const byte Token_Nullable = 26; Removed per DevDiv 165426
        // Background: Used to support nullables as a special case, CLR added support for this
        // but they forgot to remove the deserialization code when they removed the support
        // potentially Beta2 customers could have serialized data (WebParts) which have this token.
        // We removed support since this was broken anyways in RTM.
        private const byte Token_Unit = 27;
        private const byte Token_EmptyUnit = 28;
        private const byte Token_EventValidationStore = 29;

        // String-table optimized strings
        private const byte Token_IndexedStringAdd = 30;
        private const byte Token_IndexedString = 31;

        // Semi-optimized (TypeConverter-based)
        private const byte Token_StringFormatted = 40;

        // Semi-optimized (Types)
        private const byte Token_TypeRefAdd = 41;
        private const byte Token_TypeRefAddLocal = 42;
        private const byte Token_TypeRef = 43;

        // Un-optimized (Binary serialized) types
        private const byte Token_BinarySerialized = 50;

        // Optimized for sparse arrays
        private const byte Token_SparseArray = 60;

        // Constant values
        private const byte Token_Null = 100;
        private const byte Token_EmptyString = 101;
        private const byte Token_ZeroInt32 = 102;
        private const byte Token_True = 103;
        private const byte Token_False = 104;

        // Known types for which we generate short type references
        // rather than assembly qualified names
        // 


        private static readonly Type[] KnownTypes =
            new Type[] {
                typeof(object),
                typeof(int),
                typeof(string),
                typeof(bool)
            };

        // Format and Version
        private const byte Marker_Format = 0xFF;
        private const byte Marker_Version_1 = 0x01;

        // The size of the string table. At most it can be Byte.MaxValue.
        // 
        private const int StringTableSize = Byte.MaxValue;

        // Used during serialization
        private IDictionary _typeTable;
        private IDictionary _stringTable;

        // Used during deserialization
        private IList _typeList;

        // Used during both serialization and deserialization
        private int _stringTableCount;
        private string[] _stringList;

        // Used for performing Mac-encoding when this LosSerializer is used
        // in view state serialization.
        private byte[] _macKeyBytes;
        private readonly bool _forceLegacyCryptography;

        // Combined with Purpose objects which are passed in during serialization / deserialization.
        private List<string> _specificPurposes;

        // If true, this class will throw an exception if it cannot deserialize a type or value.
        // If false, this class will use insert "null" if it cannot deserialize a type or value.
        // Default is true, WebParts Personalization sets this to false.
        private bool _throwOnErrorDeserializing;

        // We use page to determine whether to to encrypt or decrypt based on Page.RequiresViewStateEncryptionInternal or Page.ContainsEncryptedViewstate
        private Page _page;

        /// <devdoc>
        /// Initializes a new instance of the ObjectStateFormatter.
        /// </devdoc>
        public ObjectStateFormatter() : this(null) {
        }

        /// <internalonly/>
        /// <devdoc>
        /// Initializes a new instance of the ObjectStateFormatter. A MAC encoding
        /// key can be specified to have the serialized data encoded for view state
        /// purposes. 
        /// NOTE: this constructor is mainly for LOSFormatter's consumption, not used internally
        /// </devdoc>
        internal ObjectStateFormatter(byte[] macEncodingKey) : this(null, true) {
            _macKeyBytes = macEncodingKey;
            if (macEncodingKey != null) {
                // If the developer explicitly asked for the data to be signed, we must honor that.
                _forceLegacyCryptography = true;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Initializes a new instance of the ObjectStateFormatter. A MAC encoding
        /// key can be specified to have the serialized data encoded for view state
        /// purposes. The Page object is used to determine whether the viewstate will be encrypted
        /// for serialize and deserialize.
        /// </devdoc>

        internal ObjectStateFormatter(Page page, bool throwOnErrorDeserializing) {
            _page = page;
            _throwOnErrorDeserializing = throwOnErrorDeserializing;
        }

        // This will return a list of specific purposes (for cryptographic subkey generation).
        internal List<string> GetSpecificPurposes() {
            if (_specificPurposes == null) {
                // Only generate a specific purpose list if we have a Page
                if (_page == null) {
                    return null;
                }

                // Note: duplicated (somewhat) in GetMacKeyModifier, keep in sync
                // See that method for comments on why these modifiers are in place

                List<string> specificPurposes = new List<string>() {
                    "TemplateSourceDirectory: " + _page.TemplateSourceDirectory.ToUpperInvariant(),
                    "Type: " + _page.GetType().Name.ToUpperInvariant()
                };

                if (_page.ViewStateUserKey != null) {
                    specificPurposes.Add("ViewStateUserKey: " + _page.ViewStateUserKey);
                }

                _specificPurposes = specificPurposes;
            }

            return _specificPurposes;
        }

        // This will return the MacKeyModifier provided in the LOSFormatter constructor or
        // generate one from Page if EnableViewStateMac is true.
        private byte[] GetMacKeyModifier() {
            if (_macKeyBytes == null) {
                // Only generate a MacKeyModifier if we have a page
                if (_page == null) {
                    return null;
                }

                // Note: duplicated (somewhat) in GetSpecificPurposes, keep in sync

                // Use the page's directory and class name as part of the key (ASURT 64044)
                uint pageHashCode = _page.GetClientStateIdentifier();

                string viewStateUserKey = _page.ViewStateUserKey;
                if (viewStateUserKey != null) {
                    // Modify the key with the ViewStateUserKey, if any (ASURT 126375)
                    int count = Encoding.Unicode.GetByteCount(viewStateUserKey);
                    _macKeyBytes = new byte[count + 4];
                    Encoding.Unicode.GetBytes(viewStateUserKey, 0, viewStateUserKey.Length, _macKeyBytes, 4);

                }
                else {
                    _macKeyBytes = new byte[4];
                }

                _macKeyBytes[0] = (byte)pageHashCode;
                _macKeyBytes[1] = (byte)(pageHashCode >> 8);
                _macKeyBytes[2] = (byte)(pageHashCode >> 16);
                _macKeyBytes[3] = (byte)(pageHashCode >> 24);
            }
            return _macKeyBytes;
        }

        /// <devdoc>
        /// Adds a string reference during the deserialization process
        /// to support deserialization of IndexedStrings.
        /// The string is added to the string list on the fly, so it is available
        /// for future reference by index.
        /// </devdoc>
        private void AddDeserializationStringReference(string s) {
            Debug.Assert((s != null) && (s.Length != 0));

            if (_stringTableCount == StringTableSize) {
                // loop around to the start of the table
                _stringTableCount = 0;
            }

            _stringList[_stringTableCount] = s;
            _stringTableCount++;
        }

        /// <devdoc>
        /// Adds a type reference during the deserialization process,
        /// so that it can be referred to later by its index.
        /// </devdoc>
        private void AddDeserializationTypeReference(Type type) {
            // Type may be null, if there is no longer a Type on the system with the saved type name.
            // This is unlikely to happen with a Type stored in ViewState, but more likely with a Type
            // stored in Personalization.
            _typeList.Add(type);
        }

        /// <devdoc>
        /// Adds a string reference during the serialization process to support
        /// the serialization of IndexedStrings.
        /// The string is added to the string list, as well as to a string table
        /// for quick lookup.
        /// </devdoc>
        private void AddSerializationStringReference(string s) {
            Debug.Assert((s != null) && (s.Length != 0));

            if (_stringTableCount == StringTableSize) {
                // loop around to the start of the table
                _stringTableCount = 0;
            }

            string oldString = _stringList[_stringTableCount];
            if (oldString != null) {
                // it means we're looping around, and the existing table entry
                // needs to be removed, as a new one will replace it
                Debug.Assert(_stringTable.Contains(oldString));
                _stringTable.Remove(oldString);
            }

            _stringTable[s] = _stringTableCount;
            _stringList[_stringTableCount] = s;
            _stringTableCount++;
        }

        /// <devdoc>
        /// Adds a type reference during the serialization process, so it
        /// can be later referred to by its index.
        /// </devdoc>
        private void AddSerializationTypeReference(Type type) {
            Debug.Assert(type != null);

            int typeID = _typeTable.Count;
            _typeTable[type] = typeID;
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        internal object DeserializeWithAssert(Stream inputStream) {
            return Deserialize(inputStream);
        }

        /// <devdoc>
        /// Deserializes an object graph from its binary serialized form
        /// contained in the specified stream.
        /// </devdoc>
        public object Deserialize(Stream inputStream) {
            if (inputStream == null) {
                throw new ArgumentNullException("inputStream");
            }

            Exception deserializationException = null;

            InitializeDeserializer();

            SerializerBinaryReader reader = new SerializerBinaryReader(inputStream);
            try {
                byte formatMarker = reader.ReadByte();

                if (formatMarker == Marker_Format) {
                    byte versionMarker = reader.ReadByte();

                    Debug.Assert(versionMarker == Marker_Version_1);
                    if (versionMarker == Marker_Version_1) {
                        return DeserializeValue(reader);
                    }
                }
            }
            catch (Exception e) {
                deserializationException = e;
            }

            // throw an exception if there was an exception during deserialization
            // or if deserialization was skipped because of invalid format or
            // version data in the stream

            throw new ArgumentException(SR.GetString(SR.InvalidSerializedData), deserializationException);
        }


        /// <devdoc>
        /// Deserializes an object graph from its textual serialized form
        /// contained in the specified string.
        /// </devdoc>
        public object Deserialize(string inputString) {
            // If the developer called Deserialize() manually on an ObjectStateFormatter object that was configured
            // for cryptographic operations, he wouldn't have been able to specify a Purpose. We'll just provide
            // a default value for him.
            return Deserialize(inputString, Purpose.User_ObjectStateFormatter_Serialize);
        }

        private object Deserialize(string inputString, Purpose purpose) {
            if (String.IsNullOrEmpty(inputString)) {
                throw new ArgumentNullException("inputString");
            }

            byte[] inputBytes = Convert.FromBase64String(inputString);
            int length = inputBytes.Length;

#if !FEATURE_PAL // FEATURE_PAL does not enable cryptography
            try {
                if (AspNetCryptoServiceProvider.Instance.IsDefaultProvider && !_forceLegacyCryptography) {
                    // If we're configured to use the new crypto providers, call into them if encryption or signing (or both) is requested.

                    if (_page != null && (_page.ContainsEncryptedViewState || _page.EnableViewStateMac)) {
                        Purpose derivedPurpose = purpose.AppendSpecificPurposes(GetSpecificPurposes());
                        ICryptoService cryptoService = AspNetCryptoServiceProvider.Instance.GetCryptoService(derivedPurpose);
                        byte[] clearData = cryptoService.Unprotect(inputBytes);
                        inputBytes = clearData;
                        length = clearData.Length;
                    }
                }
                else {
                    // Otherwise go through legacy crypto mechanisms
#pragma warning disable 618 // calling obsolete methods
                    if (_page != null && _page.ContainsEncryptedViewState) {
                        inputBytes = MachineKeySection.EncryptOrDecryptData(false, inputBytes, GetMacKeyModifier(), 0, length);
                        length = inputBytes.Length;
                    }
                    // We need to decode if the page has EnableViewStateMac or we got passed in some mac key string
                    else if ((_page != null && _page.EnableViewStateMac) || _macKeyBytes != null) {
                        inputBytes = MachineKeySection.GetDecodedData(inputBytes, GetMacKeyModifier(), 0, length, ref length);
                    }
#pragma warning restore 618 // calling obsolete methods
                }
            }
            catch {
                // MSRC 10405: Don't propagate inner exceptions, as they may contain sensitive cryptographic information.
                PerfCounters.IncrementCounter(AppPerfCounter.VIEWSTATE_MAC_FAIL);
                ViewStateException.ThrowMacValidationError(null, inputString);
            }
#endif // !FEATURE_PAL
            object result = null;
            MemoryStream objectStream = GetMemoryStream();
            try {
                objectStream.Write(inputBytes, 0, length);
                objectStream.Position = 0;
                result = Deserialize(objectStream);
            }
            finally {
                ReleaseMemoryStream(objectStream);
            }
            return result;
        }

        /// <devdoc>
        /// Deserializes an IndexedString. An IndexedString can either be the string itself (the
        /// first occurrence), or a reference to it by index into the string table.
        /// </devdoc>
        private IndexedString DeserializeIndexedString(SerializerBinaryReader reader, byte token) {
            Debug.Assert((token == Token_IndexedStringAdd) || (token == Token_IndexedString));

            if (token == Token_IndexedString) {
                // reference to string in the current string table
                int tableIndex = (int)reader.ReadByte();

                Debug.Assert(_stringList[tableIndex] != null);
                return new IndexedString(_stringList[tableIndex]);
            }
            else {
                // first occurrence of this indexed string. Read in the string, and add
                // a reference to it, so future references can be resolved.
                string s = reader.ReadString();

                AddDeserializationStringReference(s);
                return new IndexedString(s);
            }
        }

        /// <devdoc>
        /// Deserializes a Type. A Type can either be its name (the first occurrence),
        /// or a reference to it by index into the type table.  If we cannot load the type,
        /// we throw an exception if _throwOnErrorDeserializing is true, and we return null if
        /// _throwOnErrorDeserializing is false.
        /// </devdoc>
        private Type DeserializeType(SerializerBinaryReader reader) {
            byte token = reader.ReadByte();
            Debug.Assert((token == Token_TypeRef) ||
                         (token == Token_TypeRefAdd) ||
                         (token == Token_TypeRefAddLocal));

            if (token == Token_TypeRef) {
                // reference by index into type table
                int typeID = reader.ReadEncodedInt32();
                return (Type)_typeList[typeID];
            }
            else {
                // first occurrence of this type. Read in the type, resolve it, and
                // add it to the type table
                string typeName = reader.ReadString();

                Type resolvedType = null;
                try {
                    if (token == Token_TypeRefAddLocal) {
                        resolvedType = HttpContext.SystemWebAssembly.GetType(typeName, true);
                    }
                    else {
                        resolvedType = Type.GetType(typeName, true);
                    }
                }
                catch (Exception exception) {
                    if (_throwOnErrorDeserializing) {
                        throw;
                    }
                    else {
                        // Log error message
                        WebBaseEvent.RaiseSystemEvent(
                            SR.GetString(SR.Webevent_msg_OSF_Deserialization_Type, typeName),
                            this, 
                            WebEventCodes.WebErrorObjectStateFormatterDeserializationError, 
                            WebEventCodes.UndefinedEventDetailCode, 
                            exception);
                    }
                }

                AddDeserializationTypeReference(resolvedType);
                return resolvedType;
            }
        }

        /// <devdoc>
        /// Deserializes a single value from the underlying stream.
        /// Essentially a token is read, followed by as much data needed to recreate
        /// the single value.
        /// </devdoc>
        private object DeserializeValue(SerializerBinaryReader reader) {
            byte token = reader.ReadByte();

            // NOTE: Preserve the order here with the order of the logic in
            //       the SerializeValue method.

            switch (token) {
                case Token_Null:
                    return null;
                case Token_EmptyString:
                    return String.Empty;
                case Token_String:
                    return reader.ReadString();
                case Token_ZeroInt32:
                    return 0;
                case Token_Int32:
                    return reader.ReadEncodedInt32();
                case Token_Pair:
                    return new Pair(DeserializeValue(reader),
                                    DeserializeValue(reader));
                case Token_Triplet:
                    return new Triplet(DeserializeValue(reader),
                                       DeserializeValue(reader),
                                       DeserializeValue(reader));
                case Token_IndexedString:
                case Token_IndexedStringAdd:
                    return DeserializeIndexedString(reader, token);
                case Token_ArrayList:
                    {
                        int count = reader.ReadEncodedInt32();
                        ArrayList list = new ArrayList(count);
                        for (int i = 0; i < count; i++) {
                            list.Add(DeserializeValue(reader));
                        }

                        return list;
                    }
                case Token_True:
                    return true;
                case Token_False:
                    return false;
                case Token_Byte:
                    return reader.ReadByte();
                case Token_Char:
                    return reader.ReadChar();
                case Token_DateTime:
                    return DateTime.FromBinary(reader.ReadInt64());
                case Token_Double:
                    return reader.ReadDouble();
                case Token_Int16:
                    return reader.ReadInt16();
                case Token_Single:
                    return reader.ReadSingle();
                case Token_Hashtable:
                case Token_HybridDictionary:
                    {
                        int count = reader.ReadEncodedInt32();

                        IDictionary table;
                        if (token == Token_Hashtable) {
                            table = new Hashtable(count);
                        }
                        else {
                            table = new HybridDictionary(count);
                        }
                        for (int i = 0; i < count; i++) {
                            table.Add(DeserializeValue(reader),
                                      DeserializeValue(reader));
                        }

                        return table;
                    }
                case Token_Type:
                    return DeserializeType(reader);
                case Token_StringArray:
                    {
                        int count = reader.ReadEncodedInt32();

                        string[] array = new string[count];
                        for (int i = 0; i < count; i++) {
                            array[i] = reader.ReadString();
                        }

                        return array;
                    }
                case Token_Array:
                    {
                        Type elementType = DeserializeType(reader);
                        int count = reader.ReadEncodedInt32();

                        Array list = Array.CreateInstance(elementType, count);
                        for (int i = 0; i < count; i++) {
                            list.SetValue(DeserializeValue(reader), i);
                        }

                        return list;
                    }
                case Token_IntEnum:
                    {
                        Type enumType = DeserializeType(reader);
                        int enumValue = reader.ReadEncodedInt32();

                        return Enum.ToObject(enumType, enumValue);
                    }
                case Token_Color:
                    return Color.FromArgb(reader.ReadInt32());
                case Token_EmptyColor:
                    return Color.Empty;
                case Token_KnownColor:
                    return Color.FromKnownColor((KnownColor)reader.ReadEncodedInt32());
                case Token_Unit:
                    return new Unit(reader.ReadDouble(), (UnitType)reader.ReadInt32());
                case Token_EmptyUnit:
                    return Unit.Empty;
                case Token_EventValidationStore:
                    return EventValidationStore.DeserializeFrom(reader.BaseStream);
                case Token_SparseArray:
                    {
                        Type elementType = DeserializeType(reader);
                        int count = reader.ReadEncodedInt32();
                        int itemCount = reader.ReadEncodedInt32();

                        // Guard against bad data
                        if (itemCount > count) {
                            throw new InvalidOperationException(SR.GetString(SR.InvalidSerializedData));
                        }

                        Array list = Array.CreateInstance(elementType, count);
                        for (int i = 0; i < itemCount; ++i) {
                            // Data is encoded as <index, Item>
                            int nextPos = reader.ReadEncodedInt32();

                            // Guard against bad data (nextPos way too big, or nextPos not increasing)
                            if (nextPos >= count || nextPos < 0) {
                                throw new InvalidOperationException(SR.GetString(SR.InvalidSerializedData));
                            }
                            list.SetValue(DeserializeValue(reader), nextPos);
                        }

                        return list;
                    }
                case Token_StringFormatted:
                    {
                        object result = null;

                        Type valueType = DeserializeType(reader);
                        string formattedValue = reader.ReadString();

                        if (valueType != null) {
                            TypeConverter converter = TypeDescriptor.GetConverter(valueType);
                            // TypeDescriptor.GetConverter() will never return null.  The ref docs
                            // for this method are incorrect.
                            try {
                                result = converter.ConvertFromInvariantString(formattedValue);
                            }
                            catch (Exception exception) {
                                if (_throwOnErrorDeserializing) {
                                    throw;
                                }
                                else {
                                    WebBaseEvent.RaiseSystemEvent(
                                        SR.GetString(SR.Webevent_msg_OSF_Deserialization_String, valueType.AssemblyQualifiedName),
                                        this, 
                                        WebEventCodes.WebErrorObjectStateFormatterDeserializationError, 
                                        WebEventCodes.UndefinedEventDetailCode, 
                                        exception);
                                }
                            }
                        }

                        return result;
                    }
                case Token_BinarySerialized:
                    {
                        int length = reader.ReadEncodedInt32();

                        byte[] buffer = new byte[length];
                        if (length != 0) {
                            reader.Read(buffer, 0, length);
                        }

                        object result = null;
                        MemoryStream ms = GetMemoryStream();
                        try {
                            ms.Write(buffer, 0, length);
                            ms.Position = 0;
                            IFormatter formatter = new BinaryFormatter();

                            result = formatter.Deserialize(ms);
                        }
                        catch (Exception exception) {
                            if (_throwOnErrorDeserializing) {
                                throw;
                            }
                            else {
                                WebBaseEvent.RaiseSystemEvent(
                                    SR.GetString(SR.Webevent_msg_OSF_Deserialization_Binary), 
                                    this, 
                                    WebEventCodes.WebErrorObjectStateFormatterDeserializationError, 
                                    WebEventCodes.UndefinedEventDetailCode, 
                                    exception);
                            }
                        }
                        finally {
                            ReleaseMemoryStream(ms);
                        }
                        return result;
                    }
                default:
                    throw new InvalidOperationException(SR.GetString(SR.InvalidSerializedData));
            }
        }

        /// <devdoc>
        /// Retrieves a MemoryStream instance.
        /// </devdoc>
        private static MemoryStream GetMemoryStream() {
            return new MemoryStream(2048);
        }


        /// <devdoc>
        /// Initializes this instance to perform deserialization.
        /// </devdoc>
        private void InitializeDeserializer() {
            _typeList = new ArrayList();

            for (int i = 0; i < KnownTypes.Length; i++) {
                AddDeserializationTypeReference(KnownTypes[i]);
            }

            _stringList = new string[Byte.MaxValue];
            _stringTableCount = 0;
        }

        /// <devdoc>
        /// Initializes this instance to perform serialization.
        /// </devdoc>
        private void InitializeSerializer() {
            _typeTable = new HybridDictionary();

            for (int i = 0; i < KnownTypes.Length; i++) {
                AddSerializationTypeReference(KnownTypes[i]);
            }

            _stringList = new string[Byte.MaxValue];
            _stringTable = new Hashtable(StringComparer.Ordinal);
            _stringTableCount = 0;
        }

        /// <devdoc>
        /// Releases a MemoryStream instance.
        /// </devdoc>
        private static void ReleaseMemoryStream(MemoryStream stream) {
            stream.Dispose();
        }

        /// <devdoc>
        /// Serializes an object graph into a textual serialized form.
        /// </devdoc>
        public string Serialize(object stateGraph) {
            // If the developer called Serialize() manually on an ObjectStateFormatter object that was configured
            // for cryptographic operations, he wouldn't have been able to specify a Purpose. We'll just provide
            // a default value for him.
            return Serialize(stateGraph, Purpose.User_ObjectStateFormatter_Serialize);
        }

        private string Serialize(object stateGraph, Purpose purpose) {
            string result = null;

            MemoryStream ms = GetMemoryStream();
            try {
                Serialize(ms, stateGraph);
                ms.SetLength(ms.Position);

                byte[] buffer = ms.GetBuffer();
                int length = (int)ms.Length;

#if !FEATURE_PAL // FEATURE_PAL does not enable cryptography
                // We only support serialization of encrypted or encoded data through our internal Page constructors

                if (AspNetCryptoServiceProvider.Instance.IsDefaultProvider && !_forceLegacyCryptography) {
                    // If we're configured to use the new crypto providers, call into them if encryption or signing (or both) is requested.

                    if (_page != null && (_page.RequiresViewStateEncryptionInternal || _page.EnableViewStateMac)) {
                        Purpose derivedPurpose = purpose.AppendSpecificPurposes(GetSpecificPurposes());
                        ICryptoService cryptoService = AspNetCryptoServiceProvider.Instance.GetCryptoService(derivedPurpose);
                        byte[] protectedData = cryptoService.Protect(ms.ToArray());
                        buffer = protectedData;
                        length = protectedData.Length;
                    }
                }
                else {
                    // Otherwise go through legacy crypto mechanisms
#pragma warning disable 618 // calling obsolete methods
                    if (_page != null && _page.RequiresViewStateEncryptionInternal) {
                        buffer = MachineKeySection.EncryptOrDecryptData(true, buffer, GetMacKeyModifier(), 0, length);
                        length = buffer.Length;
                    }
                    // We need to encode if the page has EnableViewStateMac or we got passed in some mac key string
                    else if ((_page != null && _page.EnableViewStateMac) || _macKeyBytes != null) {
                        buffer = MachineKeySection.GetEncodedData(buffer, GetMacKeyModifier(), 0, ref length);
                    }
#pragma warning restore 618 // calling obsolete methods
                }

#endif // !FEATURE_PAL
                result = Convert.ToBase64String(buffer, 0, length);
            }
            finally {
                ReleaseMemoryStream(ms);
            }
            return result;
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        internal void SerializeWithAssert(Stream outputStream, object stateGraph) {
            Serialize(outputStream, stateGraph);
        }

        /// <devdoc>
        /// Serializes an object graph into a binary serialized form within
        /// the specified stream.
        /// </devdoc>
        public void Serialize(Stream outputStream, object stateGraph) {
            if (outputStream == null) {
                throw new ArgumentNullException("outputStream");
            }

            InitializeSerializer();

            SerializerBinaryWriter writer = new SerializerBinaryWriter(outputStream);
            writer.Write(Marker_Format);
            writer.Write(Marker_Version_1);
            SerializeValue(writer, stateGraph);
        }

        /// <devdoc>
        /// Serializes an IndexedString. If this is the first occurrence, it is written
        /// out to the underlying stream, and is added to the string table for future
        /// reference. Otherwise, a reference by index is written out.
        /// </devdoc>
        private void SerializeIndexedString(SerializerBinaryWriter writer, string s) {
            object id = _stringTable[s];
            if (id != null) {
                writer.Write(Token_IndexedString);
                writer.Write((byte)(int)id);
                return;
            }

            AddSerializationStringReference(s);

            writer.Write(Token_IndexedStringAdd);
            writer.Write(s);
        }

        /// <devdoc>
        /// Serializes a Type. If this is the first occurrence, the type name is written
        /// out to the underlying stream, and the type is added to the string table for future
        /// reference. Otherwise, a reference by index is written out.
        /// </devdoc>
        private void SerializeType(SerializerBinaryWriter writer, Type type) {
            object id = _typeTable[type];
            if (id != null) {
                writer.Write(Token_TypeRef);
                writer.WriteEncoded((int)id);
                return;
            }

            AddSerializationTypeReference(type);

            if (type.Assembly == HttpContext.SystemWebAssembly) {
                writer.Write(Token_TypeRefAddLocal);
                writer.Write(type.FullName);
            }
            else {
                writer.Write(Token_TypeRefAdd);
                writer.Write(type.AssemblyQualifiedName);
            }
        }

        /// <devdoc>
        /// Serializes a single value using the specified writer.
        /// Handles exceptions to provide more information about the value being serialized.
        /// </devdoc>
        private void SerializeValue(SerializerBinaryWriter writer, object value) {
            try {

                Stack objectStack = new Stack();
                objectStack.Push(value);

                do {
                    value = objectStack.Pop();

                    if (value == null) {
                        writer.Write(Token_Null);
                        continue;
                    }

                    // NOTE: These are ordered roughly in the order of frequency.

                    if (value is string) {
                        string s = (string)value;
                        if (s.Length == 0) {
                            writer.Write(Token_EmptyString);
                        }
                        else {
                            writer.Write(Token_String);
                            writer.Write(s);
                        }
                        continue;
                    }

                    if (value is int) {
                        int i = (int)value;
                        if (i == 0) {
                            writer.Write(Token_ZeroInt32);
                        }
                        else {
                            writer.Write(Token_Int32);
                            writer.WriteEncoded(i);
                        }
                        continue;
                    }

                    if (value is Pair) {
                        writer.Write(Token_Pair);

                        Pair p = (Pair)value;
                        objectStack.Push(p.Second);
                        objectStack.Push(p.First);
                        continue;
                    }

                    if (value is Triplet) {
                        writer.Write(Token_Triplet);

                        Triplet t = (Triplet)value;
                        objectStack.Push(t.Third);
                        objectStack.Push(t.Second);
                        objectStack.Push(t.First);
                        continue;
                    }

                    if (value is IndexedString) {
                        Debug.Assert(((IndexedString)value).Value != null);
                        SerializeIndexedString(writer, ((IndexedString)value).Value);
                        continue;
                    }

                    if (value.GetType() == typeof(ArrayList)) {
                        writer.Write(Token_ArrayList);

                        ArrayList list = (ArrayList)value;

                        writer.WriteEncoded(list.Count);
                        for (int i = list.Count - 1; i >= 0; i--) {
                            objectStack.Push(list[i]);
                        }

                        continue;
                    }

                    if (value is bool) {
                        if (((bool)value)) {
                            writer.Write(Token_True);
                        }
                        else {
                            writer.Write(Token_False);
                        }
                        continue;
                    }
                    if (value is byte) {
                        writer.Write(Token_Byte);
                        writer.Write((byte)value);
                        continue;
                    }
                    if (value is char) {
                        writer.Write(Token_Char);
                        writer.Write((char)value);
                        continue;
                    }
                    if (value is DateTime) {
                        writer.Write(Token_DateTime);
                        writer.Write(((DateTime)value).ToBinary());
                        continue;
                    }
                    if (value is double) {
                        writer.Write(Token_Double);
                        writer.Write((double)value);
                        continue;
                    }
                    if (value is short) {
                        writer.Write(Token_Int16);
                        writer.Write((short)value);
                        continue;
                    }
                    if (value is float) {
                        writer.Write(Token_Single);
                        writer.Write((float)value);
                        continue;
                    }

                    if (value is IDictionary) {
                        bool canSerializeDictionary = false;

                        if (value.GetType() == typeof(Hashtable)) {
                            writer.Write(Token_Hashtable);
                            canSerializeDictionary = true;
                        }
                        else if (value.GetType() == typeof(HybridDictionary)) {
                            writer.Write(Token_HybridDictionary);
                            canSerializeDictionary = true;
                        }

                        if (canSerializeDictionary) {
                            IDictionary table = (IDictionary)value;

                            writer.WriteEncoded(table.Count);
                            if (table.Count != 0) {
                                foreach (DictionaryEntry entry in table) {
                                    objectStack.Push(entry.Value);
                                    objectStack.Push(entry.Key);
                                }
                            }

                            continue;
                        }
                    }

                    if (value is EventValidationStore) {
                        writer.Write(Token_EventValidationStore);
                        ((EventValidationStore)value).SerializeTo(writer.BaseStream);
                        continue;
                    }

                    if (value is Type) {
                        writer.Write(Token_Type);
                        SerializeType(writer, (Type)value);
                        continue;
                    }

                    Type valueType = value.GetType();

                    if (value is Array) {
                        // We only support Arrays with rank 1 (No multi dimensional arrays
                        if (((Array)value).Rank > 1) {
                            continue;
                        }

                        Type underlyingType = valueType.GetElementType();

                        if (underlyingType == typeof(string)) {
                            string[] strings = (string[])value;
                            bool containsNulls = false;
                            for (int i = 0; i < strings.Length; i++) {
                                if (strings[i] == null) {
                                    // Will have to treat these as generic arrays since we
                                    // can't represent nulls in the binary stream, without
                                    // writing out string token markers.
                                    // Generic array writing includes the token markers.
                                    containsNulls = true;
                                    break;
                                }
                            }

                            if (!containsNulls) {
                                writer.Write(Token_StringArray);
                                writer.WriteEncoded(strings.Length);
                                for (int i = 0; i < strings.Length; i++) {
                                    writer.Write(strings[i]);
                                }
                                continue;
                            }
                        }

                        Array values = (Array)value;

                        // Optimize for sparse arrays, if the array is more than 3/4 nulls
                        if (values.Length > 3) {
                            int sparseThreshold = (values.Length / 4) + 1;
                            int numValues = 0;
                            List<int> items = new List<int>(sparseThreshold);
                            for (int i = 0; i < values.Length; ++i) {
                                if (values.GetValue(i) != null) {
                                    ++numValues;
                                    if (numValues >= sparseThreshold) {
                                        break;
                                    }
                                    items.Add(i);
                                }
                            }

                            // We have enough nulls to use sparse array format <index, value, index, value, ...>
                            if (numValues < sparseThreshold) {
                                writer.Write(Token_SparseArray);
                                SerializeType(writer, underlyingType);

                                writer.WriteEncoded(values.Length);
                                writer.WriteEncoded(numValues);

                                // Now we need to just serialize pairs representing the index, and the item
                                foreach (int index in items) {
                                    writer.WriteEncoded(index);
                                    SerializeValue(writer, values.GetValue(index));
                                }

                                continue;
                            }
                        }

                        writer.Write(Token_Array);
                        SerializeType(writer, underlyingType);

                        writer.WriteEncoded(values.Length);
                        for (int i = values.Length - 1; i >= 0; i--) {
                            objectStack.Push(values.GetValue(i));
                        }

                        continue;
                    }

                    if (valueType.IsEnum) {
                        Type underlyingType = Enum.GetUnderlyingType(valueType);
                        if (underlyingType == typeof(int)) {
                            writer.Write(Token_IntEnum);
                            SerializeType(writer, valueType);
                            writer.WriteEncoded((int)value);

                            continue;
                        }
                    }

                    if (valueType == typeof(Color)) {
                        Color c = (Color)value;
                        if (c.IsEmpty) {
                            writer.Write(Token_EmptyColor);
                            continue;
                        }
                        if (!c.IsNamedColor) {
                            writer.Write(Token_Color);
                            writer.Write(c.ToArgb());
                            continue;
                        }
                        else {
                            writer.Write(Token_KnownColor);
                            writer.WriteEncoded((int)c.ToKnownColor());
                            continue;
                        }
                    }

                    if (value is Unit) {
                        Unit uval = (Unit)value;
                        if (uval.IsEmpty) {
                            writer.Write(Token_EmptyUnit);
                        }
                        else {
                            writer.Write(Token_Unit);
                            writer.Write(uval.Value);
                            writer.Write((int)uval.Type);
                        }
                        continue;
                    }

                    // Handle the remaining types
                    // First try to get a type converter, and then resort to
                    // binary serialization if all else fails

                    TypeConverter converter = TypeDescriptor.GetConverter(valueType);
                    bool canConvert = System.Web.UI.Util.CanConvertToFrom(converter, typeof(string));

                    if (canConvert) {
                        writer.Write(Token_StringFormatted);
                        SerializeType(writer, valueType);
                        writer.Write(converter.ConvertToInvariantString(null, value));
                    }
                    else {
                        IFormatter formatter = new BinaryFormatter();
                        MemoryStream ms = new MemoryStream(256);
                        formatter.Serialize(ms, value);

                        byte[] buffer = ms.GetBuffer();
                        int length = (int)ms.Length;

                        writer.Write(Token_BinarySerialized);
                        writer.WriteEncoded(length);
                        if (buffer.Length != 0) {
                            writer.Write(buffer, 0, (int)length);
                        }
                    }
                }
                while (objectStack.Count > 0);
            }
            catch (Exception serializationException) {
                if (value != null)
                    throw new ArgumentException(SR.GetString(SR.ErrorSerializingValue, value.ToString(), value.GetType().FullName),
                                            serializationException);
                throw serializationException;
            }
        }

        #region Implementation of IStateFormatter
        object IStateFormatter.Deserialize(string serializedState) {
            return Deserialize(serializedState);
        }

        string IStateFormatter.Serialize(object state) {
            return Serialize(state);
        }
        #endregion

        #region Implementation of IFormatter

        /// <internalonly/>
        SerializationBinder IFormatter.Binder {
            get {
                return null;
            }
            set {
            }
        }


        /// <internalonly/>
        StreamingContext IFormatter.Context {
            get {
                return new StreamingContext(StreamingContextStates.All);
            }
            set {
            }
        }


        /// <internalonly/>
        ISurrogateSelector IFormatter.SurrogateSelector {
            get {
                return null;
            }
            set {
            }
        }


        /// <internalonly/>
        object IFormatter.Deserialize(Stream serializationStream) {
            return Deserialize(serializationStream);
        }


        /// <internalonly/>
        void IFormatter.Serialize(Stream serializationStream, object stateGraph) {
            Serialize(serializationStream, stateGraph);
        }
        #endregion

        #region IStateFormatter2 Members
        object IStateFormatter2.Deserialize(string serializedState, Purpose purpose) {
            return Deserialize(serializedState, purpose);
        }

        string IStateFormatter2.Serialize(object state, Purpose purpose) {
            return Serialize(state, purpose);
        }
        #endregion

        /// <devdoc>
        /// Custom BinaryReader used during the deserialization.
        /// </devdoc>
        private sealed class SerializerBinaryReader : BinaryReader {

            public SerializerBinaryReader(Stream stream) : base(stream) {
            }

            public int ReadEncodedInt32() {
                return Read7BitEncodedInt();
            }
        }


        /// <devdoc>
        /// Custom BinaryWriter used during the serialization.
        /// </devdoc>
        private sealed class SerializerBinaryWriter : BinaryWriter {

            public SerializerBinaryWriter(Stream stream) : base(stream) {
            }

            public void WriteEncoded(int value) {
                // 

                uint v = (uint)value;
                while (v >= 0x80) {
                    Write((byte)(v | 0x80));
                    v >>= 7;
                }
                Write((byte)v);
            }
        }
    }
}

