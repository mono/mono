//------------------------------------------------------------------------------
// <copyright file="LOSFormatter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

#if OBJECTSTATEFORMATTER

namespace System.Web.UI {
    using System;
    using System.IO;
    using System.Text;


    /// <devdoc>
    /// Serializes Web Froms view state. The limited object serialization (LOS) 
    /// formatter is designed for ASCII format serialization. This class
    /// supports serializing any object graph, but is optimized for those containing
    /// strings, arrays, and hashtables. It offers second order optimization for many of
    /// the .NET primitive types.
    /// This class has been replaced with a more optimal serialization mechanism implemented
    /// in LosSerializer. LosFormatter itself uses LosSerialization as part of its
    /// implementation to benefit from the highly compact serialization when possible.
    /// </devdoc>
    public sealed class LosFormatter {

        private const int InitialBufferSize = 24;

        private ObjectStateFormatter _formatter;
        private bool _enableMac;


        /// <devdoc>
        ///    <para>Creates a LosFormatter object.</para>
        /// </devdoc>
        public LosFormatter() : this(false, (byte[])null) {
        }


        /// <devdoc>
        ///    <para>Creates a LosFormatter object, specifying whether view state mac should be
        ///         enabled.  If it is, use macKeyModifier to modify the mac key.</para>
        /// </devdoc>
        public LosFormatter(bool enableMac, string macKeyModifier): this (enableMac, GetBytes(macKeyModifier)) {
        }

        public LosFormatter(bool enableMac, byte[] macKeyModifier) {
            _enableMac = enableMac;
            if (enableMac) {
                _formatter = new ObjectStateFormatter(macKeyModifier);
            }
            else {
                _formatter = new ObjectStateFormatter();
            }
        }

        private static byte[] GetBytes(string s) {
            if (s != null && s.Length != 0)
                return Encoding.Unicode.GetBytes(s);
            else
                return null;
        }


        /// <devdoc>
        /// <para> Deserializes a LOS-formatted object from a <see cref='System.IO.Stream'/> object.</para>
        /// </devdoc>
        public object Deserialize(Stream stream) {
            TextReader input = null;
            input = new StreamReader(stream);
            return Deserialize(input);
        }


        /// <devdoc>
        /// <para>Deserializes a LOS-formatted object from a <see cref='System.IO.TextReader'/> object.</para>
        /// </devdoc>
        public object Deserialize(TextReader input) {
            char[] data = new char[128];
            int read = 0;
            int current = 0;
            int blockSize = InitialBufferSize;
            do {
                read = input.Read(data, current, blockSize);
                current += read;
                if (current > data.Length - blockSize) {
                    char[] bigger = new char[data.Length * 2];
                    Array.Copy(data, bigger, data.Length);
                    data = bigger;
                }
            } while (read == blockSize);

            return Deserialize(new String(data, 0, current));
        }


        /// <devdoc>
        ///    <para>Deserializes a LOS formatted object from a string.</para>
        /// </devdoc>
        public object Deserialize(string input) {
            return _formatter.Deserialize(input);
        }


        /// <devdoc>
        ///    <para>Serializes the Web Forms view state value into 
        ///       a <see cref='System.IO.Stream'/> object.</para>
        /// </devdoc>
        public void Serialize(Stream stream, object value) {
            TextWriter output = new StreamWriter(stream);
            SerializeInternal(output, value);
            output.Flush();
        }


        /// <devdoc>
        /// <para>Serializes the view state value into a <see cref='System.IO.TextWriter'/> object.</para>
        /// </devdoc>
        public void Serialize(TextWriter output, object value) {
            SerializeInternal(output, value);
        }


        /// <devdoc>
        ///     Serialized value into the writer.
        /// </devdoc>
        private void SerializeInternal(TextWriter output, object value) {
            string data = _formatter.Serialize(value);
            output.Write(data);
        }
    }
}

#else // !OBJECTSTATEFORMATTER

// uncomment for "human readable" debugging output - no base64 encoding.
//#define NO_BASE64

namespace System.Web.UI {
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization;
    using System;
    using System.IO;
    using System.Security.Principal;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading;
    using System.Text;
    using System.Web.Configuration;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>Serializes Web Froms view state. The limited object serialization (LOS) 
    ///       formatter is designed for highly compact ASCII format serialization. This class
    ///       supports serializing any object graph, but is optimized for those containing
    ///       strings, arrays, and hashtables. It offers second order optimization for many of
    ///       the .NET primitive types.</para>
    /// </devdoc>
    public sealed class LosFormatter : IStateFormatter {

        // NOTE : This formatter is not very fault tolerant, by design. I want
        //      : to avoid a bunch of reduntant checking... Since the format
        //      : is short lived we shouldn't have to worry about it.
        //

        // NOTE : Although hex encoding of numbers would be more effecient, it
        //      : would make it much harder to determine what are numbers vs.
        //      : names & types. Unless this becomes a problem, I suggest we
        //      : keep encoding in decimal.
        //

        // Known Types. There can only be 50 of these... The order shouldn't
        // matter, we store and index into this array... although there is a
        // slight perf advantage being at the top of the list...
        //
        private static readonly Type[] knownTypes = new Type[]
        {
            typeof(object),
            typeof(System.Web.UI.WebControls.Unit),
            typeof(System.Drawing.Color),
            typeof(System.Int16),
            typeof(System.Int64),
        };

        static readonly Encoding EncodingInstance = new UTF8Encoding(false);
        static readonly NumberFormatInfo NumberFormat = NumberFormatInfo.InvariantInfo;

        private const int UntypedTypeId = -1;
        private const int NoTypeId = -2;
        private const int InitialBufferSize = 24;
        private const int BufferGrowth = 48;


        // Constant chars and strings... you can change these and all references to the
        // begin, end, and delimiter chars are fixed up
        private const char leftAngleBracketChar = '<';
        private const char rightAngleBracketChar = '>';
        private const char valueDelimiterChar = ';';

        private static readonly char[] escapedCharacters = { leftAngleBracketChar, rightAngleBracketChar, valueDelimiterChar, '\\' };
        private static CharBufferAllocator _charBufferAllocator = new CharBufferAllocator(256, 16);

        // reusable Temp buffer used for constructing strings from char arrays... this is
        // more performant than using a StringBuilder.
        //
        private char[] _builder;
        private bool _recyclable;

        // Tables used to build up the type and name tables during
        // serialization. Not used during deserilization.
        private IDictionary _typeTable;

        // Deserialization variables. Not used during serialization.
        private ArrayList       _deserializedTypeTable;
        private ListDictionary  _deserializedConverterTable;
        private char[]          _deserializationData;
        private int             _current;

        // MAC authentication 
        private bool _enableViewStateMac;
        private bool EnableViewStateMac {
            get { return _enableViewStateMac; }
        }

        private byte [] _macKey = null;


        /// <devdoc>
        ///    <para>Creates a LosFormatter object.</para>
        /// </devdoc>
        public LosFormatter() {}


        /// <devdoc>
        ///    <para>Creates a LosFormatter object, specifying whether view state mac should be
        ///         enabled.  If it is, use macKeyModifier to modify the mac key.</para>
        /// </devdoc>
        public LosFormatter(bool enableMac, string macKeyModifier) {
            _enableViewStateMac = enableMac;

            if (macKeyModifier != null)
                _macKey = Encoding.Unicode.GetBytes(macKeyModifier);
        }


        /// <devdoc>
        /// <para> Deserializes a LOS-formatted object from a <see cref='System.IO.Stream'/> object.</para>
        /// </devdoc>
        public object Deserialize(Stream stream) {
            TextReader input = null;
            input = new StreamReader(stream);
            return Deserialize(input);
        }


        /// <devdoc>
        /// <para>Deserializes a LOS-formatted object from a <see cref='System.IO.TextReader'/> object.</para>
        /// </devdoc>
        public object Deserialize(TextReader input) {
            char[] data = new char[128];
            int read = 0;
            int current = 0;
            int blockSize = InitialBufferSize;
            do {
                read = input.Read(data, current, blockSize);
                current += read;
                if (current > data.Length - blockSize) {
                    char[] bigger = new char[data.Length * 2];
                    Array.Copy(data, bigger, data.Length);
                    data = bigger;
                }
            } while (read == blockSize);

            return Deserialize(new String(data, 0, current));
        }


        /// <devdoc>
        ///    <para>Deserializes a LOS formatted object from a string.</para>
        /// </devdoc>
        public object Deserialize(string input) {

#if NO_BASE64
            char[] data = input.ToCharArray();
#else
            byte[] dataBytes = Convert.FromBase64String(input);

            int dataLength = -1;
            if (EnableViewStateMac) {

                try {
                    dataBytes = MachineKeySection.GetDecodedData(dataBytes, _macKey, 0, dataBytes.Length, ref dataLength);
                }
                catch (Exception e) {
                    PerfCounters.IncrementCounter(AppPerfCounter.VIEWSTATE_MAC_FAIL);
                    ViewStateException.ThrowMacValidationError(e, input);
                }
            }

            if (dataLength == -1) {
                dataLength = dataBytes.Length;
            }

            char[] data = EncodingInstance.GetChars(dataBytes, 0, dataLength);
#endif


            // clear or allocate name and type tables.
            //
            if (_deserializedTypeTable == null) {
                _deserializedTypeTable = new ArrayList();
                _deserializedConverterTable = new ListDictionary();
            }
            else {
                _deserializedTypeTable.Clear();
                _deserializedConverterTable.Clear();
            }

            _builder = (char[])  _charBufferAllocator.GetBuffer();
            _recyclable = true;

            // DeserializeValueInternal is recursive, so we just kick this off
            // starting at 0
            _current = 0;
            _deserializationData = data;
            object ret = DeserializeValueInternal();

            if (_recyclable)
                _charBufferAllocator.ReuseBuffer(_builder);
            
            return ret;
        }


        /// <devdoc>
        ///     Deserializes a value from tokens, starting at current. When this
        ///     function returns, current will be left at the next token.
        ///
        ///     This function is recursive.
        /// </devdoc>
        private object DeserializeValueInternal() {
            // Determine the data type... possible combinations are:
            //
            //   @<...>     == array of strings
            //   @T<...>    == array of (typeref T)
            //   b<...>     == base64 encoded value
            //   h<...>     == hashtable
            //   l<...>     == arraylist
            //   p<...>     == pair
            //   t<...>     == triplet
            //   i<...>     == int
            //   o<t/f>     == boolean true/false
            //   T<...>     == (typeref T)
            //   ...        == string 
            //

            object value = null;

            string token = ConsumeOneToken();

            if (_current >= _deserializationData.Length || _deserializationData[_current] != leftAngleBracketChar) {
                // just a string - next token is not a left angle bracket
                // we can shortcut here and just return the string
                //_current++; //consume right angle bracket or delimiter
                return token;
            }

            _current++; // consume left angle bracket

            // otherwise, we have typeref followed by value
            if (token.Length == 1) {
                // simple type we recognize
                char ch = token[0];
                if (ch == 'p') {
                    Pair p = new Pair();

                    if (_deserializationData[_current] != valueDelimiterChar) {
                        p.First = DeserializeValueInternal();
                    }
                    _current++; // consume delimeter
                    if (_deserializationData[_current] != rightAngleBracketChar) {
                        p.Second = DeserializeValueInternal();
                    }
                    value = p;
                }
                else if (ch == 't') {
                    Triplet t = new Triplet();

                    if (_deserializationData[_current] != valueDelimiterChar) {
                        t.First = DeserializeValueInternal();
                    }
                    _current++; // consume delimeter
                    if (_deserializationData[_current] != valueDelimiterChar) {
                        t.Second = DeserializeValueInternal();
                    }
                    _current++; // consume delimeter
                    if (_deserializationData[_current] != rightAngleBracketChar) {
                        t.Third = DeserializeValueInternal();
                    }
                    value = t;
                }
                    
                // Parse int32...
                else if (ch == 'i') {
                    value = Int32.Parse(ConsumeOneToken(), NumberFormat);
                }

                else if (ch == 'o') {
                    value = _deserializationData[_current] == 't'; 
                    _current++;  // consume t or f
                }

                // Parse arrayList...
                //
                else if (ch == 'l') {
                    ArrayList data = new ArrayList();

                    while (_deserializationData[_current] != rightAngleBracketChar) {
                        object itemValue = null;
                        if (_deserializationData[_current] != valueDelimiterChar) {
                            itemValue = DeserializeValueInternal();
                        }
                        data.Add(itemValue);
                        _current++; //consume the delimiter
                    }

                    value = data;
                }
                else if (ch == '@') {
                    // if we're here, length == 1 so this is a string array
                    value = ConsumeStringArray();
                }

                // Parse hashtable...
                //
                else if (ch == 'h') {
                    Hashtable data = new Hashtable();

                    while (_deserializationData[_current] != rightAngleBracketChar) {
                        object key;
                        key = DeserializeValueInternal();   // hashtable key cannot be null

                        _current++; // consume delimiter
                        if (_deserializationData[_current] != valueDelimiterChar) {
                            data[key] = DeserializeValueInternal();
                        }
                        else {
                            data[key] = null;
                        }

                        _current++; // consume delimiter
                    }

                    value = data;
                }

                // base64 encoded...
                //
                else if (ch == 'b') {
                    string text = ConsumeOneToken();
                    byte[] serializedData;
                    serializedData = Convert.FromBase64String(text);

                    if (!String.IsNullOrEmpty(serializedData)) {
                        System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        value = formatter.Deserialize(new MemoryStream(serializedData));
                    }
                }

                // Parse typeconverter value ...
                //
                else {
                    // we have a typeref which is only one character long
                    value = ConsumeTypeConverterValue(token);
                }

                
            }
            else {
                // length > 1

                // Parse array...
                //
                if (token[0] == '@') {
                    // if we're here, length > 1 so we must have a type ref after the @
                    Type creatableType = TypeFromTypeRef(token.Substring(1));
                    value = ConsumeArray(creatableType);
                }

                // Parse typeconverter value ...
                //
                else {
                    // we have a typeref which is more than one character long
                    value = ConsumeTypeConverterValue(token);
                }
            }

            _current++; //consume right angle bracket
            return value;
        }



        private string ConsumeOneToken() {
            int locInBuilder = 0;

            while (_current < _deserializationData.Length)
            {
                switch (_deserializationData[_current]) {
                    case '\\':
                        _current++; // skip slash
                        if (_deserializationData[_current] == 'e') {
                            _current++;
                            return String.Empty;
                        }
                        _builder[locInBuilder] = _deserializationData[_current];
                        locInBuilder++;
                        break;
                    case valueDelimiterChar:
                    case leftAngleBracketChar:
                    case rightAngleBracketChar:
                        return new string(_builder, 0, locInBuilder);

                    default:
                        _builder[locInBuilder] = _deserializationData[_current];
                        locInBuilder++;
                        break;
                }

                _current++;

                // Alloc _builder always 2 greater than locInBuilder to make sure
                // we can do nested/escape parsing without error...
                //
                if (locInBuilder >= _builder.Length) {
                    char[] bigger = new char[_builder.Length + BufferGrowth];
                    Array.Copy(_builder, bigger, _builder.Length);
                    _builder = bigger;
                    _recyclable = false;
                }
            }
            return new string(_builder, 0, locInBuilder);
        }

        private object ConsumeStringArray() {
            ArrayList data = new ArrayList();
            while (_deserializationData[_current] != rightAngleBracketChar) {
                object itemValue = null;
                if (_deserializationData[_current] != valueDelimiterChar) {
                    itemValue = ConsumeOneToken();
                }
                data.Add(itemValue);
                _current++; //consume the delimiter
            }

            return data.ToArray(typeof(string));
        }
        
        private object ConsumeArray(Type creatableType) {
            ArrayList data = new ArrayList();
            while (_deserializationData[_current] != rightAngleBracketChar) {
                object itemValue = null;
                if (_deserializationData[_current] != valueDelimiterChar) {
                    itemValue = DeserializeValueInternal();
                }
                data.Add(itemValue);
                _current++; //consume the delimiter
            }

            return data.ToArray(creatableType);
        }

        private object ConsumeTypeConverterValue(string token) {
            int typeref = ParseNumericString(token);
            TypeConverter tc;

            if (typeref != -1) {
                // token is the string representation of the number here
                tc = (TypeConverter) _deserializedConverterTable[token];
                if (tc == null) {
                    // wasn't in the converter table, add it now
                    // we need this case because arrays can add types but not typeconverters
                    Type t = TypeFromTypeCode(typeref);
                    tc = TypeDescriptor.GetConverter(t);
                    _deserializedConverterTable[token] = tc;
                }
            }
            else {
                // it's just a name, lookup type and add to type table
                Type t = Type.GetType(token);
                tc = TypeDescriptor.GetConverter(t);

                // add to type table and converter table.
                _deserializedConverterTable[(_deserializedTypeTable.Count + 50).ToString(NumberFormat)] = tc;
                _deserializedTypeTable.Add(t);
            }
            string text = ConsumeOneToken();
            return tc.ConvertFrom(null, CultureInfo.InvariantCulture, text);
        }


        /// <devdoc>
        ///    <para>Serializes the Web Forms view state value into 
        ///       a <see cref='System.IO.Stream'/> object.</para>
        /// </devdoc>
        public void Serialize(Stream stream, object value) {
            TextWriter output = new StreamWriter(stream);
            SerializeInternal(output, value);
            output.Flush();
        }


        /// <devdoc>
        /// <para>Serializes the view state value into a <see cref='System.IO.TextWriter'/> object.</para>
        /// </devdoc>
        public void Serialize(TextWriter output, object value) {
            SerializeInternal(output, value);
        }


        /// <devdoc>
        ///     Serialized value into the writer.
        /// </devdoc>
        private void SerializeInternal(TextWriter output, object value) {
            if (value == null)
                return;

            if (_typeTable == null) 
                _typeTable = new HybridDictionary();
            else 
                _typeTable.Clear();

#if NO_BASE64
            SerializeValue(output, value);
#else 

            LosWriter writer = new LosWriter();

            SerializeValue(writer, value);

            writer.CompleteTransforms(output, EnableViewStateMac, _macKey); 
            writer.Dispose();
#endif

        }


        /// <devdoc>
        ///     Recursively serializes value into the writer.
        /// </devdoc>
        private void SerializeValue(TextWriter output, object value) {
            if (value == null) 
                return;
            
            // First determine the type... either typeless (string), array,
            // typed array, hashtable, pair, triplet, knowntype, typetable reference, or
            // type...
            //
            
            // serialize string...
            //
            if (value is string) {
                WriteEscapedString(output, (string)value);
            }

            // serialize Int32...
            //
            else if (value is Int32) {
                output.Write('i');
                output.Write(leftAngleBracketChar);
                output.Write(((Int32)value).ToString(NumberFormat));
                output.Write(rightAngleBracketChar);
            }
            else if (value is Boolean) {
                output.Write('o');
                output.Write(leftAngleBracketChar);
                output.Write( ((bool) value) ? 't' : 'f');
                output.Write(rightAngleBracketChar);
            }

            // serialize arraylist...
            //
            else if (value is ArrayList) {
                output.Write('l');
                output.Write(leftAngleBracketChar);

                ArrayList ar = (ArrayList)value;
                int c = ar.Count;
                for (int i=0; i<c; i++) {
                    SerializeValue(output, ar[i]);
                    output.Write(valueDelimiterChar);
                }
                output.Write(rightAngleBracketChar);
            }

            // serialize hashtable...
            //
            else if (value is Hashtable) {
                output.Write('h');
                output.Write(leftAngleBracketChar);

                Hashtable table = (Hashtable)value;

                IDictionaryEnumerator e = table.GetEnumerator();
                while (e.MoveNext()) {
                    SerializeValue(output, e.Key);
                    output.Write(valueDelimiterChar);

                    SerializeValue(output, e.Value);
                    output.Write(valueDelimiterChar);
                }
                output.Write(rightAngleBracketChar);
            }

            else {
                // we'll need the Type object for the last two possibilities
                Type valueType = value.GetType();
                Type strtype = typeof(string);

                // serialize Pair
                if (valueType == typeof(Pair)) {
                    Pair p = (Pair) value;
                    output.Write('p');
                    output.Write(leftAngleBracketChar);

                    SerializeValue(output, p.First);
                    output.Write(valueDelimiterChar);
                    SerializeValue(output, p.Second);
                    output.Write(rightAngleBracketChar);
                }

                // serialize Triplet
                //
                else if (valueType == typeof(Triplet)) {
                    Triplet t = (Triplet) value;
                    output.Write('t');
                    output.Write(leftAngleBracketChar);

                    SerializeValue(output, t.First);
                    output.Write(valueDelimiterChar);
                    SerializeValue(output, t.Second);
                    output.Write(valueDelimiterChar);
                    SerializeValue(output, t.Third);
                    output.Write(rightAngleBracketChar);
                }

                // serialize array...
                //
                else if (valueType.IsArray) {
                    Type underlyingValueType;
                    underlyingValueType = valueType.GetElementType();

                    output.Write('@');

                    if (underlyingValueType != strtype) {
                        // write type of array before elements
                        int typeId = GetTypeId(underlyingValueType);
                        WriteTypeId(output, typeId, underlyingValueType);

                        output.Write(leftAngleBracketChar);
                        Array ar = (Array)value;
                        for (int i=0; i<ar.Length; i++) {
                            SerializeValue(output, ar.GetValue(i));
                            output.Write(valueDelimiterChar);
                        }
                    }
                    else {
                        // optimization: since we know the underlying values are strings, 
                        // we can skip the recursive call to SerializeValue
                        output.Write(leftAngleBracketChar);
                        string[] ar = (string[])value;
                        for (int i=0; i<ar.Length; i++) {
                            WriteEscapedString(output, ar[i]);
                            output.Write(valueDelimiterChar);
                        }
                    }
                    output.Write(rightAngleBracketChar);
                }

                // serialize other value...
                //
                else {
                    int typeId = GetTypeId(valueType);

                    // get the type converter 
                    TypeConverter tc = TypeDescriptor.GetConverter(valueType);

                    bool toString;
                    bool fromString;
                    if (tc == null || tc is ReferenceConverter) { 
                        toString = false;
                        fromString = false;
                    }
                    else {
                        toString = tc.CanConvertTo(strtype);
                        fromString = tc.CanConvertFrom(strtype);
                    }
                    
                    if (toString && fromString) {
                        //we can convert to and from a string
                        WriteTypeId(output, typeId, valueType);
                        
                        output.Write(leftAngleBracketChar);
                        WriteEscapedString(output, tc.ConvertToInvariantString(null, value));
                    }
                    else {
                        // the typeconverter failed us, so we are resorting to binary serialization
                        MemoryStream ms = new MemoryStream();
                        System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter(); 
                        try {
                            formatter.Serialize(ms, value);
                        }
                        catch (SerializationException) {
                            throw new HttpException(SR.GetString(SR.NonSerializableType, value.GetType().FullName));
                        }

                        output.Write('b');
                        output.Write(leftAngleBracketChar);

                        // Since base64 doesn't have any chars that we escape, we can
                        // skip WriteEscapedString
                        output.Write(Convert.ToBase64String(ms.GetBuffer(), 0, (int) ms.Length));
                    }
                    output.Write(rightAngleBracketChar);
                }
            }
        }

        private int GetTypeId(Type valueType) {

            int typeId = NoTypeId;

            // check if it is a known type
            for (int i=0; i<knownTypes.Length; i++) {
                if (valueType == knownTypes[i]) {
                    typeId = i;
                    break;
                }
            }


            if (typeId == NoTypeId) {
                // not a known type, see if it's in the type table
                object found = _typeTable[valueType];
                if (found != null) 
                    typeId = 50 + (int)found;
            }

            return typeId;
        }

        private void WriteTypeId(TextWriter output, int typeId, Type valueType) {
            if (typeId != NoTypeId)
                output.Write(typeId.ToString(NumberFormat));
            else {
                // ASURT 60173: use AssemblyQualifiedName here, not FullName
                WriteEscapedString(output, valueType.AssemblyQualifiedName);
                // 
                _typeTable[valueType] = _typeTable.Count;
            }
        }



        /// <devdoc>
        ///     Takes a typeRef, and converts it to a Type. Either by returning
        ///     Type.GetType(typeRef), or looking it up.
        /// </devdoc>
        private Type TypeFromTypeRef(string typeRef) {

            int number = ParseNumericString(typeRef);

            Type t = TypeFromTypeCode(number);

            if (t != null)
                return t;

            // it's just a name, lookup type and add to type table
            t = Type.GetType(typeRef);
            _deserializedTypeTable.Add(t);
            return t;

        }

        private Type TypeFromTypeCode(int number) {
            if (number != -1) {
                // it is a type id, either in the known table or in our type table
                if (number <= 49)
                    return knownTypes[number];

                return (Type) _deserializedTypeTable[number - 50];

            }
            return null;
        }


        // Note : We have to determine if "typeRef" is a number. The easiest
        //      : and fastest way to do this is to walk the string. While
        //      : we are doing this, lets build up the number... after
        //      : all this is much faster than Int32.Parse
        //
        private int ParseNumericString(string num) {
            int number = 0;
            int len = num.Length;

            for (int i=0; i<len; i++) {
                switch (num[i]) {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        number *= 10;
                        number += (((int)num[i]) - ((int)'0'));
                        break;
                    default:
                        number = -1;
                        i = len;
                        break;
                }
            }
            return number;
        }


        /// <devdoc>
        ///     Escapes and writes the escaped value of str into the writer.
        /// </devdoc>
        private void WriteEscapedString(TextWriter output, string str) {

            if (str == null)
                return;
            
            // need to "escape" the empty string to distinguish it
            // from a null value
            if (str.Length == 0) {
                output.Write('\\');
                output.Write('e');
                return;
            }

            int first = str.IndexOfAny(escapedCharacters);
            if (first == -1) {
                output.Write(str);
            }
            else {
                char[] strData = str.ToCharArray();
                output.Write(strData, 0, first);
                int len = strData.Length;

                for (int i=first; i<len; i++) {
                    char c = strData[i];
                    switch (c) {
                        case '\\':
                            output.Write('\\');
                            output.Write('\\');
                            break;
                        case leftAngleBracketChar:
                            output.Write('\\');
                            output.Write(leftAngleBracketChar);
                            break;
                        case rightAngleBracketChar:
                            output.Write('\\');
                            output.Write(rightAngleBracketChar);
                            break;
                        case valueDelimiterChar:
                            output.Write('\\');
                            output.Write(valueDelimiterChar);
                            break;
                        default:
                            output.Write(c);
                            break;
                    }
                }
            }
        }

        static internal int EstimateSize(object obj) {
            if (obj == null)
                return 0;
            StringWriter sw = new StringWriter();
            LosFormatter formatter = new LosFormatter();
            formatter.Serialize(sw, obj);
            return sw.ToString().Length;
        }

        #region Implementation of IStateFormatter
        object IStateFormatter.Deserialize(string serializedState) {
            return Deserialize(serializedState);
        }

        string IStateFormatter.Serialize(object state) {
            StringWriter writer = new StringWriter();
            Serialize(writer, state);
            return writer.ToString();
        }
        #endregion
    }
}

#endif // OBJECTSTATEFORMATTER
