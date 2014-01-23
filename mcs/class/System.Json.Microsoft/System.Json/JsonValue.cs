// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
#if FEATURE_DYNAMIC
using System.Dynamic;
#endif
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace System.Json
{
    /// <summary>
    /// This is the base class for JavaScript Object Notation (JSON) common language runtime (CLR) types. 
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "JsonValue is by definition either a collection or a single object.")]
    [DataContract]
#if FEATURE_DYNAMIC
    public class JsonValue : IEnumerable<KeyValuePair<string, JsonValue>>, IDynamicMetaObjectProvider
#else
    public class JsonValue : IEnumerable<KeyValuePair<string, JsonValue>>
#endif
    {
        private static JsonValue defaultInstance = new JsonValue();
        private int changingListenersCount;
        private int changedListenersCount;

        internal JsonValue()
        {
        }

        /// <summary>
        /// Raised when this <see cref="System.Json.JsonValue"/> or any of its members have changed.
        /// </summary>
        /// <remarks><p>Events are raised when elements are added or removed to <see cref="System.Json.JsonValue"/>
        /// instances. It applies to both complex descendants of <see cref="System.Json.JsonValue"/>: <see cref="System.Json.JsonArray"/>
        /// and <see cref="System.Json.JsonObject"/>.</p>
        /// <p>You should be careful when modifying a <see cref="System.Json.JsonValue"/> tree within one of these events,
        /// because doing this might lead to unexpected results. For example, if you receive a Changing event, and while
        /// the event is being processed you remove the node from the tree, you might not receive the Changed event. When
        /// an event is being processed, it is valid to modify a tree other than the one that contains the node that is
        /// receiving the event; it is even valid to modify the same tree provided the modifications do not affect the
        /// specific nodes on which the event was raised. However, if you modify the area of the tree that contains the
        /// node receiving the event, the events that you receive and the impact to the tree are undefined.</p></remarks>
        public event EventHandler<JsonValueChangeEventArgs> Changed
        {
            add
            {
                changedListenersCount++;
                OnChanged += value;
            }

            remove
            {
                changedListenersCount--;
                OnChanged -= value;
            }
        }

        /// <summary>
        /// Raised when this <see cref="System.Json.JsonValue"/> or any of its members are about to be changed.
        /// </summary>
        /// <remarks><p>Events are raised when elements are added or removed to <see cref="System.Json.JsonValue"/>
        /// instances. It applies to both complex descendants of <see cref="System.Json.JsonValue"/>: <see cref="System.Json.JsonArray"/>
        /// and <see cref="System.Json.JsonObject"/>.</p>
        /// <p>You should be careful when modifying a <see cref="System.Json.JsonValue"/> tree within one of these events,
        /// because doing this might lead to unexpected results. For example, if you receive a Changing event, and while
        /// the event is being processed you remove the node from the tree, you might not receive the Changed event. When
        /// an event is being processed, it is valid to modify a tree other than the one that contains the node that is
        /// receiving the event; it is even valid to modify the same tree provided the modifications do not affect the
        /// specific nodes on which the event was raised. However, if you modify the area of the tree that contains the
        /// node receiving the event, the events that you receive and the impact to the tree are undefined.</p></remarks>
        public event EventHandler<JsonValueChangeEventArgs> Changing
        {
            add
            {
                changingListenersCount++;
                OnChanging += value;
            }

            remove
            {
                changingListenersCount--;
                OnChanging -= value;
            }
        }

        private event EventHandler<JsonValueChangeEventArgs> OnChanged;
        private event EventHandler<JsonValueChangeEventArgs> OnChanging;

        /// <summary>
        /// Gets the JSON CLR type represented by this instance.
        /// </summary>
        public virtual JsonType JsonType
        {
            get { return JsonType.Default; }
        }

        /// <summary>
        /// Gets the number of items in this object.
        /// </summary>
        public virtual int Count
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the number of listeners to the <see cref="Changing"/> event for this instance.
        /// </summary>
        protected int ChangingListenersCount
        {
            get { return changingListenersCount; }
        }

        /// <summary>
        /// Gets the number of listeners to the <see cref="Changed"/> event for this instance.
        /// </summary>
        protected int ChangedListenersCount
        {
            get { return changedListenersCount; }
        }

        /// <summary>
        /// Gets the default JsonValue instance.  
        /// This instance enables safe-chaining of JsonValue operations and resolves to 'null'
        /// when this instance is used as dynamic, mapping to the JavaScript 'null' value.
        /// </summary>
        private static JsonValue DefaultInstance
        {
            get { return defaultInstance; }
        }

        /// <summary>
        /// This indexer is not supported for this base class and throws an exception.
        /// </summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns><see cref="System.Json.JsonValue"/>.</returns>
        /// <remarks>The exception thrown is the <see cref="System.InvalidOperationException"/>.
        /// This method is overloaded in the implementation of the <see cref="System.Json.JsonObject"/>
        /// class, which inherits from this class.</remarks>
        public virtual JsonValue this[string key]
        {
            get { throw new InvalidOperationException(RS.Format(Properties.Resources.IndexerNotSupportedOnJsonType, typeof(string), JsonType)); }

            set { throw new InvalidOperationException(RS.Format(Properties.Resources.IndexerNotSupportedOnJsonType, typeof(string), JsonType)); }
        }

        /// <summary>
        /// This indexer is not supported for this base class and throws an exception.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns><see cref="System.Json.JsonValue"/>.</returns>
        /// <remarks>The exception thrown is the <see cref="System.InvalidOperationException"/>.
        /// This method is overloaded in the implementation of the <see cref="System.Json.JsonArray"/>
        /// class, which inherits from this class.</remarks>
        public virtual JsonValue this[int index]
        {
            get { throw new InvalidOperationException(RS.Format(Properties.Resources.IndexerNotSupportedOnJsonType, typeof(int), JsonType)); }

            set { throw new InvalidOperationException(RS.Format(Properties.Resources.IndexerNotSupportedOnJsonType, typeof(int), JsonType)); }
        }

        /// <summary>
        /// Deserializes text-based JSON into a JSON CLR type.
        /// </summary>
        /// <param name="json">The text-based JSON to be parsed into a JSON CLR type.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> object that represents the parsed
        /// text-based JSON as a CLR type.</returns>
        /// <exception cref="System.ArgumentException">The length of jsonString is zero.</exception>
        /// <exception cref="System.ArgumentNullException">jsonString is null.</exception>
        /// <remarks>The result will be an instance of either <see cref="System.Json.JsonArray"/>,
        /// <see cref="System.Json.JsonObject"/> or <see cref="System.Json.JsonPrimitive"/>,
        /// depending on the text-based JSON supplied to the method.</remarks>
        public static JsonValue Parse(string json)
        {
            return JXmlToJsonValueConverter.JXMLToJsonValue(json);
        }

        /// <summary>
        /// Deserializes text-based JSON from a text reader into a JSON CLR type.
        /// </summary>
        /// <param name="textReader">A <see cref="System.IO.TextReader"/> over text-based JSON content.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> object that represents the parsed
        /// text-based JSON as a CLR type.</returns>
        /// <exception cref="System.ArgumentNullException">textReader is null.</exception>
        /// <remarks>The result will be an instance of either <see cref="System.Json.JsonArray"/>,
        /// <see cref="System.Json.JsonObject"/> or <see cref="System.Json.JsonPrimitive"/>,
        /// depending on the text-based JSON supplied to the method.</remarks>
        public static JsonValue Load(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }

            return JsonValue.Parse(textReader.ReadToEnd());
        }

        /// <summary>
        /// Deserializes text-based JSON from a stream into a JSON CLR type.
        /// </summary>
        /// <param name="stream">A <see cref="System.IO.Stream"/> that contains text-based JSON content.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> object that represents the parsed
        /// text-based JSON as a CLR type.</returns>
        /// <exception cref="System.ArgumentNullException">stream is null.</exception>
        /// <remarks>The result will be an instance of either <see cref="System.Json.JsonArray"/>,
        /// <see cref="System.Json.JsonObject"/> or <see cref="System.Json.JsonPrimitive"/>,
        /// depending on the text-based JSON supplied to the method.</remarks>
        public static JsonValue Load(Stream stream)
        {
            return JXmlToJsonValueConverter.JXMLToJsonValue(stream);
        }

        /// <summary>
        /// Performs a cast operation from a <see cref="JsonValue"/> instance into the specified type parameter./>
        /// </summary>
        /// <typeparam name="T">The type to cast the instance to.</typeparam>
        /// <param name="value">The <see cref="System.Json.JsonValue"/> instance.</param>
        /// <returns>An object of type T initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        /// <remarks>This method is to support the framework and is not intended to be used externally, use explicit type cast instead.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "The generic parameter is used to specify the output type")]
        public static T CastValue<T>(JsonValue value)
        {
            Type typeofT = typeof(T);

            if ((value != null && typeofT.IsAssignableFrom(value.GetType())) || typeofT == typeof(object))
            {
                return (T)(object)value;
            }

            if (value == null || value.JsonType == JsonType.Default)
            {
                if (typeofT.IsValueType)
                {
                    throw new InvalidCastException(RS.Format(Properties.Resources.InvalidCastNonNullable, typeofT.FullName));
                }
                else
                {
                    return default(T);
                }
            }

            try
            {
                return value.ReadAs<T>();
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is NotSupportedException || ex is InvalidCastException)
                {
                    throw new InvalidCastException(RS.Format(Properties.Resources.CannotCastJsonValue, value.GetType().FullName, typeofT.FullName), ex);
                }

                throw;
            }
        }

        /// <summary>
        /// Returns an enumerator which iterates through the values in this object.
        /// </summary>
        /// <returns>An enumerator which which iterates through the values in this object.</returns>
        /// <remarks>The enumerator returned by this class is empty; subclasses will override this method to return appropriate enumerators for themselves.</remarks>
        public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator()
        {
            return GetKeyValuePairEnumerator();
        }

        /// <summary>
        /// Returns an enumerator which iterates through the values in this object.
        /// </summary>
        /// <returns>An <see cref="System.Collections.IEnumerator"/> which iterates through the values in this object.</returns>
        /// <remarks>The enumerator returned by this class is empty; subclasses will override this method to return appropriate enumerators for themselves.</remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetKeyValuePairEnumerator();
        }

#if FEATURE_DYNAMIC
        /// <summary>
        /// Gets this instance as a <code>dynamic</code> object.
        /// </summary>
        /// <returns>This instance as <code>dynamic</code>.</returns>
        public dynamic AsDynamic()
        {
            return this;
        }
#endif

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance into the type T.
        /// </summary>
        /// <typeparam name="T">The type to which the conversion is being performed.</typeparam>
        /// <param name="valueOfT">An instance of T initialized with this instance, or the default value of T if the conversion cannot be performed.</param>
        /// <returns>true if this <see cref="System.Json.JsonValue"/> instance can be read as type T; otherwise, false.</returns>
        public bool TryReadAs<T>(out T valueOfT)
        {
            object value;
            if (TryReadAs(typeof(T), out value))
            {
                valueOfT = (T)value;
                return true;
            }

            valueOfT = default(T);
            return false;
        }

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance into the type T.
        /// </summary>
        /// <typeparam name="T">The type to which the conversion is being performed.</typeparam>
        /// <returns>An instance of T initialized with the value from the conversion of this instance.</returns>
        /// <exception cref="System.NotSupportedException">If this <see cref="System.Json.JsonValue"/> value cannot be converted into the type T.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "The generic parameter is used to specify the output type")]
        public T ReadAs<T>()
        {
            return (T)ReadAs(typeof(T));
        }

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance into the type T.
        /// </summary>
        /// <typeparam name="T">The type to which the conversion is being performed.</typeparam>
        /// <param name="fallback">The fallback value to be returned if the conversion cannot be made.</param>
        /// <returns>An instance of T initialized with the value from the conversion of this instance, or the specified fallback value if the conversion cannot be made.</returns>
        public T ReadAs<T>(T fallback)
        {
            return (T)ReadAs(typeof(T), fallback);
        }

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance to an instance of the specified type.
        /// </summary>
        /// <param name="type">The type to which the conversion is being performed.</param>
        /// <param name="fallback">The fallback value to be returned if the conversion cannot be made.</param>
        /// <returns>An instance of the specified type initialized with the value from the conversion of this instance, or the specified fallback value if the conversion cannot be made.</returns>
        public object ReadAs(Type type, object fallback)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            object result;
            if (JsonType != JsonType.Default && TryReadAs(type, out result))
            {
                return result;
            }
            else
            {
                return fallback;
            }
        }

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance into an instance of the specified type.
        /// </summary>
        /// <param name="type">The type to which the conversion is being performed.</param>
        /// <returns>An instance of the specified type initialized with the value from the conversion of this instance.</returns>
        /// <exception cref="System.NotSupportedException">If this <see cref="System.Json.JsonValue"/> value cannot be converted into the type T.</exception>
        public virtual object ReadAs(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            object result;
            if (TryReadAs(type, out result))
            {
                return result;
            }

            throw new NotSupportedException(RS.Format(Properties.Resources.CannotReadAsType, GetType().FullName, type.FullName));
        }

        /// <summary>
        /// Attempts to convert this <see cref="System.Json.JsonValue"/> instance into an instance of the specified type.
        /// </summary>
        /// <param name="type">The type to which the conversion is being performed.</param>
        /// <param name="value">An object to be initialized with this instance or null if the conversion cannot be performed.</param>
        /// <returns>true if this <see cref="System.Json.JsonValue"/> instance can be read as the specified type; otherwise, false.</returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate",
            Justification = "This is the non-generic version of the method.")]
        public virtual bool TryReadAs(Type type, out object value)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsAssignableFrom(GetType()) || type == typeof(object))
            {
                value = this;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Writes this <see cref="System.Json.JsonValue"/> instance to a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="stream">Stream to which to write text-based JSON.</param>
        public void Save(Stream stream)
        {
            if (JsonType == JsonType.Default)
            {
                throw new InvalidOperationException(Properties.Resources.UseOfDefaultNotAllowed);
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (XmlDictionaryWriter jsonWriter = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, false))
            {
                jsonWriter.WriteStartElement(JXmlToJsonValueConverter.RootElementName);
                Save(jsonWriter);
                jsonWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes <see cref="System.Json.JsonValue"/> instance to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="System.IO.TextWriter"/> used to write text-based JSON.</param>
        public void Save(TextWriter textWriter)
        {
            if (JsonType == JsonType.Default)
            {
                throw new InvalidOperationException(Properties.Resources.UseOfDefaultNotAllowed);
            }

            if (textWriter == null)
            {
                throw new ArgumentNullException("textWriter");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Save(ms);
                ms.Position = 0;
                textWriter.Write(new StreamReader(ms).ReadToEnd());
            }
        }

        /// <summary>
        /// Provides a textual representation of this <see cref="System.Json.JsonValue"/> instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> containing text-based JSON.</returns>
        public override string ToString()
        {
            if (JsonType == JsonType.Default)
            {
                return "Default";
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Save(ms);
                ms.Position = 0;
                return new StreamReader(ms).ReadToEnd();
            }
        }

        /// <summary>
        /// Checks whether a key/value pair with a specified key exists in the JSON CLR object type.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>false in this class; subclasses may override this method to return other values.</returns>
        /// <remarks>This method is overloaded in the implementation of the <see cref="System.Json.JsonObject"/>
        /// class, which inherits from this class.</remarks>
        public virtual bool ContainsKey(string key)
        {
            return false;
        }

        /// <summary>
        /// Returns the value returned by the safe string indexer for this instance.
        /// </summary>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>If this is an instance of <see cref="System.Json.JsonObject"/>, it contains
        /// the given key and the value corresponding to the key is not null, then it will return that value.
        /// Otherwise it will return a <see cref="System.Json.JsonValue"/> instance with <see cref="System.Json.JsonValue.JsonType"/>
        /// equals to <see cref="F:System.Json.JsonType.Default"/>.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual JsonValue GetValue(string key)
        {
            return ValueOrDefault(key);
        }

        /// <summary>
        /// Returns the value returned by the safe int indexer for this instance.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>If this is an instance of <see cref="System.Json.JsonArray"/>, the index is within the array
        /// bounds, and the value corresponding to the index is not null, then it will return that value.
        /// Otherwise it will return a <see cref="System.Json.JsonValue"/> instance with <see cref="System.Json.JsonValue.JsonType"/>
        /// equals to <see cref="F:System.Json.JsonType.Default"/>.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual JsonValue GetValue(int index)
        {
            return ValueOrDefault(index);
        }

        /// <summary>
        /// Sets the value and returns it.
        /// </summary>
        /// <param name="key">The key of the element to set.</param>
        /// <param name="value">The value to be set.</param>
        /// <returns>The value, converted into a JsonValue, set in this collection.</returns>
        /// <exception cref="System.ArgumentException">If the value cannot be converted into a JsonValue.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual JsonValue SetValue(string key, object value)
        {
            this[key] = ResolveObject(value);
            return this[key];
        }

        /// <summary>
        /// Sets the value and returns it.
        /// </summary>
        /// <param name="index">The zero-based index of the element to set.</param>
        /// <param name="value">The value to be set.</param>
        /// <returns>The value, converted into a JsonValue, set in this collection.</returns>
        /// <exception cref="System.ArgumentException">If the value cannot be converted into a JsonValue.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual JsonValue SetValue(int index, object value)
        {
            this[index] = ResolveObject(value);
            return this[index];
        }

        /// <summary>
        /// Safe string indexer for the <see cref="System.Json.JsonValue"/> type. 
        /// </summary>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>If this is an instance of <see cref="System.Json.JsonObject"/>, it contains
        /// the given key and the value corresponding to the key is not null, then it will return that value.
        /// Otherwise it will return a <see cref="System.Json.JsonValue"/> instance with <see cref="System.Json.JsonValue.JsonType"/>
        /// equals to <see cref="F:System.Json.JsonType.Default"/>.</returns>
        public virtual JsonValue ValueOrDefault(string key)
        {
            return JsonValue.DefaultInstance;
        }

        /// <summary>
        /// Safe indexer for the <see cref="System.Json.JsonValue"/> type. 
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>If this is an instance of <see cref="System.Json.JsonArray"/>, the index is within the array
        /// bounds, and the value corresponding to the index is not null, then it will return that value.
        /// Otherwise it will return a <see cref="System.Json.JsonValue"/> instance with <see cref="System.Json.JsonValue.JsonType"/>
        /// equals to <see cref="F:System.Json.JsonType.Default"/>.</returns>
        public virtual JsonValue ValueOrDefault(int index)
        {
            return JsonValue.DefaultInstance;
        }

        /// <summary>
        /// Safe deep indexer for the <see cref="JsonValue"/> type.
        /// </summary>
        /// <param name="indexes">The indices to index this type. The indices can be
        /// of type <see cref="System.Int32"/> or <see cref="System.String"/>.</param>
        /// <returns>A <see cref="JsonValue"/> which is equivalent to calling<see cref="ValueOrDefault(int)"/> or
        /// <see cref="ValueOrDefault(string)"/> on the first index, then calling it again on the result
        /// for the second index and so on.</returns>
        /// <exception cref="System.ArgumentException">If any of the indices is not of type
        /// <see cref="System.Int32"/> or <see cref="System.String"/>.</exception>
        public JsonValue ValueOrDefault(params object[] indexes)
        {
            if (indexes == null)
            {
                return JsonValue.DefaultInstance;
            }

            if (indexes.Length == 0)
            {
                return this;
            }

            JsonValue result = this;
            for (int i = 0; i < indexes.Length; i++)
            {
                object index = indexes[i];
                if (index == null)
                {
                    result = JsonValue.DefaultInstance;
                    continue;
                }

                Type indexType = index.GetType();

                switch (Type.GetTypeCode(indexType))
                {
                    case TypeCode.Char:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                        index = System.Convert.ChangeType(index, typeof(int), CultureInfo.InvariantCulture);
                        goto case TypeCode.Int32;

                    case TypeCode.Int32:
                        result = result.ValueOrDefault((int)index);
                        break;

                    case TypeCode.String:
                        result = result.ValueOrDefault((string)index);
                        break;

                    default:
                        throw new ArgumentException(RS.Format(Properties.Resources.InvalidIndexType, index.GetType()), "indexes");
                }
            }

            return result;
        }

#if FEATURE_DYNAMIC
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "Cannot make this class sealed, it needs to have subclasses. But its subclasses are sealed themselves.")]
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            return new JsonValueDynamicMetaObject(parameter, this);
        }
#endif

        /// <summary>
        /// Resolves the specified object to an appropriate JsonValue instance.
        /// </summary>
        /// <param name="value">The object to resolve.</param>
        /// <returns>A <see cref="JsonValue"/> instance resolved from the specified object.</returns>
        internal static JsonValue ResolveObject(object value)
        {
            JsonPrimitive primitive;

            if (value == null)
            {
                return null;
            }

            JsonValue jsonValue = value as JsonValue;

            if (jsonValue != null)
            {
                return jsonValue;
            }

            if (JsonPrimitive.TryCreate(value, out primitive))
            {
                return primitive;
            }

            throw new ArgumentException(Properties.Resources.TypeNotSupported, "value");
        }

        /// <summary>
        /// Determines whether an explicit cast to JsonValue is provided from the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if an explicit cast exists for the specified type, false otherwise.</returns>
        internal static bool IsSupportedExplicitCastType(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.String:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;

                default:
                    return type == typeof(DateTimeOffset) || type == typeof(Guid) || type == typeof(Uri) ||
                           type == typeof(List<object>) || type == typeof(Array) || type == typeof(object[]) ||
                           type == typeof(Dictionary<string, object>);
            }
        }

        /// <summary>
        /// Returns the value this object wraps (if any).
        /// </summary>
        /// <returns>The value wrapped by this instance or null if none.</returns>
        internal virtual object Read()
        {
            return null;
        }

        /// <summary>
        /// Serializes this object into the specified <see cref="XmlDictionaryWriter"/> instance.
        /// </summary>
        /// <param name="jsonWriter">An <see cref="XmlDictionaryWriter"/> instance to serialize this instance into.</param>
        internal virtual void Save(XmlDictionaryWriter jsonWriter)
        {
            if (jsonWriter == null)
            {
                throw new ArgumentNullException("jsonWriter");
            }

            Stack<JsonValue> objectStack = new Stack<JsonValue>();
            Stack<int> indexStack = new Stack<int>();
            int currentIndex = 0;
            JsonValue currentValue = this;

            OnSaveStarted();

            WriteAttributeString(jsonWriter);

            while (currentIndex < currentValue.Count || objectStack.Count > 0)
            {
                if (currentValue.Count > currentIndex)
                {
                    JsonValue nextValue = currentValue.WriteStartElementAndGetNext(jsonWriter, currentIndex);

                    if (JsonValue.IsJsonCollection(nextValue))
                    {
                        nextValue.OnSaveStarted();
                        nextValue.WriteAttributeString(jsonWriter);

                        objectStack.Push(currentValue);
                        indexStack.Push(currentIndex);
                        currentValue = nextValue;
                        currentIndex = 0;
                    }
                    else
                    {
                        if (nextValue == null)
                        {
                            jsonWriter.WriteAttributeString(JXmlToJsonValueConverter.TypeAttributeName, JXmlToJsonValueConverter.NullAttributeValue);
                        }
                        else
                        {
                            nextValue.Save(jsonWriter);
                        }

                        currentIndex++;
                        jsonWriter.WriteEndElement();
                    }
                }
                else
                {
                    if (objectStack.Count > 0)
                    {
                        currentValue.OnSaveEnded();
                        jsonWriter.WriteEndElement();

                        currentValue = objectStack.Pop();
                        currentIndex = indexStack.Pop() + 1;
                    }
                }
            }

            OnSaveEnded();
        }

        /// <summary>
        /// Returns an enumerator which iterates through the values in this object.
        /// </summary>
        /// <returns>An <see cref="System.Collections.IEnumerator"/> which iterates through the values in this object.</returns>
        /// <remarks>This method is the virtual version of the IEnumerator.GetEnumerator method and is provided to allow derived classes to implement the 
        /// appropriate version of the generic interface (enumerator of values or key/value pairs).</remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "This method is a virtual version of the IEnumerable.GetEnumerator method.")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This class is a collection that is properly represented by the nested generic type.")]
        protected virtual IEnumerator<KeyValuePair<string, JsonValue>> GetKeyValuePairEnumerator()
        {
            yield break;
        }

        /// <summary>
        /// Callback method called during Save operations to let the instance write the start element
        /// and return the next element in the collection.
        /// </summary>
        /// <param name="jsonWriter">The JXML writer used to write JSON.</param>
        /// <param name="index">The index within this collection.</param>
        /// <returns>The next item in the collection, or null of there are no more items.</returns>
        internal virtual JsonValue WriteStartElementAndGetNext(XmlDictionaryWriter jsonWriter, int index)
        {
            return null;
        }

        /// <summary>
        /// Callback method called to let an instance write the proper JXML attribute when saving this
        /// instance.
        /// </summary>
        /// <param name="jsonWriter">The JXML writer used to write JSON.</param>
        internal virtual void WriteAttributeString(XmlDictionaryWriter jsonWriter)
        {
        }

        /// <summary>
        /// Callback method called when a Save operation is starting for this instance.
        /// </summary>
        protected virtual void OnSaveStarted()
        {
        }

        /// <summary>
        /// Callback method called when a Save operation is finished for this instance.
        /// </summary>
        protected virtual void OnSaveEnded()
        {
        }

        /// <summary>
        /// Called internally to raise the <see cref="Changing"/> event.
        /// </summary>
        /// <param name="sender">The object which caused the event to be raised.</param>
        /// <param name="eventArgs">The arguments to the event.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
            Justification = "This is a helper function used to raise the event.")]
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers",
            Justification = "This is not externally visible, since the constructor for this class is internal (cannot be directly derived) and all its subclasses are sealed.")]
        protected void RaiseChangingEvent(object sender, JsonValueChangeEventArgs eventArgs)
        {
            EventHandler<JsonValueChangeEventArgs> changing = OnChanging;
            if (changing != null)
            {
                changing(sender, eventArgs);
            }
        }

        /// <summary>
        /// Called internally to raise the <see cref="Changed"/> event.
        /// </summary>
        /// <param name="sender">The object which caused the event to be raised.</param>
        /// <param name="eventArgs">The arguments to the event.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
            Justification = "This is a helper function used to raise the event.")]
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers",
            Justification = "This is not externally visible, since the constructor for this class is internal (cannot be directly derived) and all its subclasses are sealed.")]
        protected void RaiseChangedEvent(object sender, JsonValueChangeEventArgs eventArgs)
        {
            EventHandler<JsonValueChangeEventArgs> changed = OnChanged;
            if (changed != null)
            {
                changed(sender, eventArgs);
            }
        }

        private static bool IsJsonCollection(JsonValue value)
        {
            return value != null && (value.JsonType == JsonType.Array || value.JsonType == JsonType.Object);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.String"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.String"/> object.</param>
        /// <returns>The <see cref="System.String"/> initialized with the <see cref="System.Json.JsonValue"/> value specified or null if value is null.</returns>
        public static explicit operator string(JsonValue value)
        {
            return CastValue<string>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Double"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Double"/> object.</param>
        /// <returns>The <see cref="System.Double"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator double(JsonValue value)
        {
            return CastValue<double>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Single"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Single"/> object.</param>
        /// <returns>The <see cref="System.Single"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator float(JsonValue value)
        {
            return CastValue<float>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Decimal"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Decimal"/> object.</param>
        /// <returns>The <see cref="System.Decimal"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator decimal(JsonValue value)
        {
            return CastValue<decimal>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Int64"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Int64"/> object.</param>
        /// <returns>The <see cref="System.Int64"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator long(JsonValue value)
        {
            return CastValue<long>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.UInt64"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.UInt64"/> object.</param>
        /// <returns>The <see cref="System.UInt64"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        [CLSCompliant(false)]
        public static explicit operator ulong(JsonValue value)
        {
            return CastValue<ulong>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Int32"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Int32"/> object.</param>
        /// <returns>The <see cref="System.Int32"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator int(JsonValue value)
        {
            return CastValue<int>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.UInt32"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.UInt32"/> object.</param>
        /// <returns>The <see cref="System.UInt32"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        [CLSCompliant(false)]
        public static explicit operator uint(JsonValue value)
        {
            return CastValue<uint>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Int16"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Int16"/> object.</param>
        /// <returns>The <see cref="System.Int16"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator short(JsonValue value)
        {
            return CastValue<short>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.UInt16"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.UInt16"/> object.</param>
        /// <returns>The <see cref="System.UInt16"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        [CLSCompliant(false)]
        public static explicit operator ushort(JsonValue value)
        {
            return CastValue<ushort>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.SByte"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.SByte"/> object.</param>
        /// <returns>The <see cref="System.SByte"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        [CLSCompliant(false)]
        public static explicit operator sbyte(JsonValue value)
        {
            return CastValue<sbyte>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Byte"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Byte"/> object.</param>
        /// <returns>The <see cref="System.Byte"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator byte(JsonValue value)
        {
            return CastValue<byte>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Uri"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Uri"/> object.</param>
        /// <returns>The <see cref="System.Uri"/> initialized with the <see cref="System.Json.JsonValue"/> value specified or null if value is null.</returns>
        public static explicit operator Uri(JsonValue value)
        {
            return CastValue<Uri>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Guid"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Guid"/> object.</param>
        /// <returns>The <see cref="System.Guid"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator Guid(JsonValue value)
        {
            return CastValue<Guid>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.DateTime"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.DateTime"/> object.</param>
        /// <returns>The <see cref="System.DateTime"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator DateTime(JsonValue value)
        {
            return CastValue<DateTime>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Char"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Char"/> object.</param>
        /// <returns>The <see cref="System.Char"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator char(JsonValue value)
        {
            return CastValue<char>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.Boolean"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.Boolean"/> object.</param>
        /// <returns>The <see cref="System.Boolean"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator bool(JsonValue value)
        {
            return CastValue<bool>(value);
        }

        /// <summary>
        /// Enables explicit casts from an instance of type <see cref="System.Json.JsonValue"/> to a <see cref="System.DateTimeOffset"/> object.
        /// </summary>
        /// <param name="value">The instance of <see cref="System.Json.JsonValue"/> used to initialize the <see cref="System.DateTimeOffset"/> object.</param>
        /// <returns>The <see cref="System.DateTimeOffset"/> initialized with the <see cref="System.Json.JsonValue"/> value specified.</returns>
        public static explicit operator DateTimeOffset(JsonValue value)
        {
            return CastValue<DateTimeOffset>(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Boolean"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Boolean"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Boolean"/> specified.</returns>
        public static implicit operator JsonValue(bool value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Byte"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Byte"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Byte"/> specified.</returns>
        public static implicit operator JsonValue(byte value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Decimal"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Decimal"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Decimal"/> specified.</returns>
        public static implicit operator JsonValue(decimal value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Double"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Double"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Double"/> specified.</returns>
        public static implicit operator JsonValue(double value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Int16"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Int16"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Int16"/> specified.</returns>
        public static implicit operator JsonValue(short value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Int32"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Int32"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Int32"/> specified.</returns>
        public static implicit operator JsonValue(int value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Int64"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Int64"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Int64"/> specified.</returns>
        public static implicit operator JsonValue(long value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Single"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Single"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Single"/> specified.</returns>
        public static implicit operator JsonValue(float value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.String"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.String"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.String"/> specified, or null if the value is null.</returns>
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads",
            Justification = "This operator does not intend to represent a Uri overload.")]
        public static implicit operator JsonValue(string value)
        {
            return value == null ? null : new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Char"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Char"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Char"/> specified.</returns>
        public static implicit operator JsonValue(char value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.DateTime"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.DateTime"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.DateTime"/> specified.</returns>
        public static implicit operator JsonValue(DateTime value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Guid"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Guid"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Guid"/> specified.</returns>
        public static implicit operator JsonValue(Guid value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.Uri"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Uri"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.Uri"/> specified, or null if the value is null.</returns>
        public static implicit operator JsonValue(Uri value)
        {
            return value == null ? null : new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.SByte"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.SByte"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.SByte"/> specified.</returns>
        [CLSCompliant(false)]
        public static implicit operator JsonValue(sbyte value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.UInt16"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.UInt16"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.UInt16"/> specified.</returns>
        [CLSCompliant(false)]
        public static implicit operator JsonValue(ushort value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.UInt32"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.UInt32"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.UInt32"/> specified.</returns>
        [CLSCompliant(false)]
        public static implicit operator JsonValue(uint value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.UInt64"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.UInt64"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.UInt64"/> specified.</returns>
        [CLSCompliant(false)]
        public static implicit operator JsonValue(ulong value)
        {
            return new JsonPrimitive(value);
        }

        /// <summary>
        /// Enables implicit casts from type <see cref="System.DateTimeOffset"/> to a <see cref="System.Json.JsonPrimitive"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.DateTimeOffset"/> instance used to initialize the <see cref="System.Json.JsonPrimitive"/>.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> initialized with the <see cref="System.DateTimeOffset"/> specified.</returns>
        public static implicit operator JsonValue(DateTimeOffset value)
        {
            return new JsonPrimitive(value);
        }
    }
}
