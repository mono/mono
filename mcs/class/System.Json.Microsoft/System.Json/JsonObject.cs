// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Xml;
using WrappedPair = System.Json.NGenWrapper<System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>>;

namespace System.Json
{
    /// <summary>
    /// A JsonObject is an unordered collection of zero or more key/value pairs.
    /// </summary>
    /// <remarks>A JsonObject is an unordered collection of zero or more key/value pairs,
    /// where each key is a String and each value is a <see cref="System.Json.JsonValue"/>, which can be a
    /// <see cref="System.Json.JsonPrimitive"/>, a <see cref="System.Json.JsonArray"/>, or a JsonObject.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "Object in the context of JSON already conveys the meaning of dictionary")]
    [DataContract]
    public sealed class JsonObject : JsonValue, IDictionary<string, JsonValue>
    {
        [DataMember]
        private Dictionary<string, JsonValue> values = new Dictionary<string, JsonValue>(StringComparer.Ordinal);

        private List<WrappedPair> indexedPairs;
        private int instanceSaveCount;
        private object saveLock = new object();

        /// <summary>
        /// Creates an instance of the <see cref="System.Json.JsonObject"/> class initialized with an
        /// <see cref="System.Collections.Generic.IEnumerable{T}"/> collection of key/value pairs.
        /// </summary>
        /// <param name="items">The <see cref="System.Collections.Generic.IEnumerable{T}"/> collection of
        /// <see cref="System.Collections.Generic.KeyValuePair{K, V}"/> used to initialize the
        /// key/value pairs</param>
        /// <exception cref="System.ArgumentNullException">If items is null.</exception>
        /// <exception cref="System.ArgumentException">If any of the values in the collection
        /// is a <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/> property of
        /// value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "There's no complexity using this design because nested generic type is atomic type not another collection")]
        public JsonObject(IEnumerable<KeyValuePair<string, JsonValue>> items)
        {
            AddRange(items);
        }

        /// <summary>
        /// Creates an instance of the <see cref="System.Json.JsonObject"/> class initialized with a collection of key/value pairs.
        /// </summary>
        /// <param name="items">The <see cref="System.Collections.Generic.KeyValuePair{K, V}"/> objects used to initialize the key/value pairs.</param>
        /// <exception cref="System.ArgumentException">If any of the values in the collection
        /// is a <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/> property of
        /// value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public JsonObject(params KeyValuePair<string, JsonValue>[] items)
        {
            if (items != null)
            {
                AddRange(items);
            }
        }

        /// <summary>
        /// Gets the JSON type of this <see cref="System.Json.JsonObject"/>. The return value
        /// is always <see cref="F:System.Json.JsonType.Object"/>.
        /// </summary>
        public override JsonType JsonType
        {
            get { return JsonType.Object; }
        }

        /// <summary>
        /// Gets a collection that contains the keys in this <see cref="System.Json.JsonObject"/>.
        /// </summary>
        public ICollection<string> Keys
        {
            get { return values.Keys; }
        }

        /// <summary>
        /// Gets a collection that contains the values in this <see cref="System.Json.JsonObject"/>.
        /// </summary>
        public ICollection<JsonValue> Values
        {
            get { return values.Values; }
        }

        /// <summary>
        /// Returns the number of key/value pairs in this <see cref="System.Json.JsonObject"/>.
        /// </summary>
        public override int Count
        {
            get { return values.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this JSON CLR object is read-only.
        /// </summary>
        bool ICollection<KeyValuePair<string, JsonValue>>.IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<string, JsonValue>>)values).IsReadOnly; }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> associated to the specified key.</returns>
        /// <exception cref="System.ArgumentNullException">If key is null.</exception>
        /// <exception cref="System.ArgumentException">The property is set and the value is a
        /// <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/>
        /// property of value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public override JsonValue this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                return values[key];
            }

            set
            {
                if (value != null && value.JsonType == JsonType.Default)
                {
                    throw new ArgumentNullException("value", Properties.Resources.UseOfDefaultNotAllowed);
                }

                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                bool replacement = values.ContainsKey(key);
                JsonValue oldValue = null;
                if (replacement)
                {
                    oldValue = values[key];
                    RaiseItemChanging(value, JsonValueChange.Replace, key);
                }
                else
                {
                    RaiseItemChanging(value, JsonValueChange.Add, key);
                }

                values[key] = value;
                if (replacement)
                {
                    RaiseItemChanged(oldValue, JsonValueChange.Replace, key);
                }
                else
                {
                    RaiseItemChanged(value, JsonValueChange.Add, key);
                }
            }
        }

        /// <summary>
        /// Safe string indexer for the <see cref="System.Json.JsonValue"/> type. 
        /// </summary>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>If this instance contains the given key and the value corresponding to
        /// the key is not null, then it will return that value. Otherwise it will return a
        /// <see cref="System.Json.JsonValue"/> instance with <see cref="System.Json.JsonValue.JsonType"/>
        /// equals to <see cref="F:System.Json.JsonType.Default"/>.</returns>
        public override JsonValue ValueOrDefault(string key)
        {
            if (key != null && ContainsKey(key) && this[key] != null)
            {
                return this[key];
            }

            return base.ValueOrDefault(key);
        }

        /// <summary>
        /// Adds a specified collection of key/value pairs to this instance.
        /// </summary>
        /// <param name="items">The collection of key/value pairs to add.</param>
        /// <exception cref="System.ArgumentNullException">If items is null.</exception>
        /// <exception cref="System.ArgumentException">If the value of any of the items in the collection
        /// is a <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/> property of
        /// value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "There's no complexity using this design because nested generic type is atomic type not another collection")]
        public void AddRange(IEnumerable<KeyValuePair<string, JsonValue>> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (ChangingListenersCount > 0)
            {
                foreach (KeyValuePair<string, JsonValue> item in items)
                {
                    RaiseItemChanging(item.Value, JsonValueChange.Add, item.Key);
                }
            }

            foreach (KeyValuePair<string, JsonValue> item in items)
            {
                if (item.Value != null && item.Value.JsonType == JsonType.Default)
                {
                    throw new ArgumentNullException("items", Properties.Resources.UseOfDefaultNotAllowed);
                }

                values.Add(item.Key, item.Value);
                RaiseItemChanged(item.Value, JsonValueChange.Add, item.Key);
            }
        }

        /// <summary>
        /// Adds the elements from an array of type <see cref="System.Json.JsonValue"/> to this instance.
        /// </summary>
        /// <param name="items">The array of key/value paris to be added to this instance.</param>
        /// <exception cref="System.ArgumentException">If the value of any of the items in the array
        /// is a <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/> property of
        /// value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public void AddRange(params KeyValuePair<string, JsonValue>[] items)
        {
            AddRange(items as IEnumerable<KeyValuePair<string, JsonValue>>);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)values).GetEnumerator();
        }

        /// <summary>
        /// Adds a key/value pair to this <see cref="System.Json.JsonObject"/> instance.
        /// </summary>
        /// <param name="key">The key for the element added.</param>
        /// <param name="value">The <see cref="System.Json.JsonValue"/> for the element added.</param>
        /// <exception cref="System.ArgumentException">If the value is a <see cref="System.Json.JsonValue"/>
        /// with <see cref="System.Json.JsonValue.JsonType"/> property of
        /// value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public void Add(string key, JsonValue value)
        {
            if (value != null && value.JsonType == JsonType.Default)
            {
                throw new ArgumentNullException("value", Properties.Resources.UseOfDefaultNotAllowed);
            }

            RaiseItemChanging(value, JsonValueChange.Add, key);
            values.Add(key, value);
            RaiseItemChanged(value, JsonValueChange.Add, key);
        }

        /// <summary>
        /// Adds a key/value pair to this <see cref="System.Json.JsonObject"/> instance.
        /// </summary>
        /// <param name="item">The key/value pair to be added.</param>
        /// <exception cref="System.ArgumentException">If the value of the pair is a
        /// <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/>
        /// property of value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public void Add(KeyValuePair<string, JsonValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Checks whether a key/value pair with a specified key exists in this <see cref="System.Json.JsonObject"/> instance.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>true if this instance contains the key; otherwise, false.</returns>
        public override bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        /// <summary>
        /// Removes the key/value pair with a specified key from this <see cref="System.Json.JsonObject"/> instance.
        /// </summary>
        /// <param name="key">The key of the item to remove.</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false.
        /// This method returns false if key is not found in this <see cref="System.Json.JsonObject"/> instance.</returns>
        public bool Remove(string key)
        {
            JsonValue original = null;
            bool containsKey = false;
            if (ChangingListenersCount > 0 || ChangedListenersCount > 0)
            {
                containsKey = TryGetValue(key, out original);
            }

            if (containsKey && ChangingListenersCount > 0)
            {
                RaiseItemChanging(original, JsonValueChange.Remove, key);
            }

            bool result = values.Remove(key);

            if (containsKey && ChangedListenersCount > 0)
            {
                RaiseItemChanged(original, JsonValueChange.Remove, key);
            }

            return result;
        }

        /// <summary>
        /// Attempts to get the value that corresponds to the specified key.
        /// </summary>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <param name="value">The primitive or structured <see cref="System.Json.JsonValue"/> object that has the key
        /// specified. If this object does not contain a key/value pair with the given key,
        /// this parameter is set to null.</param>
        /// <returns>true if the instance of the <see cref="System.Json.JsonObject"/> contains an element with the
        /// specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out JsonValue value)
        {
            return values.TryGetValue(key, out value);
        }

        /// <summary>
        /// Removes all key/value pairs from this <see cref="System.Json.JsonObject"/> instance.
        /// </summary>
        public void Clear()
        {
            RaiseItemChanging(null, JsonValueChange.Clear, null);
            values.Clear();
            RaiseItemChanged(null, JsonValueChange.Clear, null);
        }

        bool ICollection<KeyValuePair<string, JsonValue>>.Contains(KeyValuePair<string, JsonValue> item)
        {
            return ((ICollection<KeyValuePair<string, JsonValue>>)values).Contains(item);
        }

        /// <summary>
        /// Copies the contents of this <see cref="System.Json.JsonObject"/> instance into a specified
        /// key/value destination array beginning at a specified index.
        /// </summary>
        /// <param name="array">The destination array of type <see cref="System.Collections.Generic.KeyValuePair{K, V}"/>
        /// to which the elements of this <see cref="System.Json.JsonObject"/> are copied.</param>
        /// <param name="arrayIndex">The zero-based index at which to begin the insertion of the
        /// contents from this <see cref="System.Json.JsonObject"/> instance.</param>
        public void CopyTo(KeyValuePair<string, JsonValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, JsonValue>>)values).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, JsonValue>>.Remove(KeyValuePair<string, JsonValue> item)
        {
            if (ChangingListenersCount > 0)
            {
                if (ContainsKey(item.Key) && EqualityComparer<JsonValue>.Default.Equals(item.Value, values[item.Key]))
                {
                    RaiseItemChanging(item.Value, JsonValueChange.Remove, item.Key);
                }
            }

            bool result = ((ICollection<KeyValuePair<string, JsonValue>>)values).Remove(item);
            if (result)
            {
                RaiseItemChanged(item.Value, JsonValueChange.Remove, item.Key);
            }

            return result;
        }

        /// <summary>
        /// Returns an enumerator over the key/value pairs contained in this <see cref="System.Json.JsonObject"/> instance.
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerator{T}"/> which iterates
        /// through the members of this instance.</returns>
        protected override IEnumerator<KeyValuePair<string, JsonValue>> GetKeyValuePairEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// Callback method called when a Save operation is starting for this instance.
        /// </summary>
        protected override void OnSaveStarted()
        {
            lock (saveLock)
            {
                instanceSaveCount++;
                if (indexedPairs == null)
                {
                    indexedPairs = new List<WrappedPair>();

                    foreach (KeyValuePair<string, JsonValue> item in values)
                    {
                        indexedPairs.Add(new WrappedPair(item));
                    }
                }
            }
        }

        /// <summary>
        /// Callback method called when a Save operation is finished for this instance.
        /// </summary>
        protected override void OnSaveEnded()
        {
            lock (saveLock)
            {
                instanceSaveCount--;
                if (instanceSaveCount == 0)
                {
                    indexedPairs = null;
                }
            }
        }

        /// <summary>
        /// Callback method called to let an instance write the proper JXML attribute when saving this
        /// instance.
        /// </summary>
        /// <param name="jsonWriter">The JXML writer used to write JSON.</param>
        internal override void WriteAttributeString(XmlDictionaryWriter jsonWriter)
        {
            jsonWriter.WriteAttributeString(JXmlToJsonValueConverter.TypeAttributeName, JXmlToJsonValueConverter.ObjectAttributeValue);
        }

        /// <summary>
        /// Callback method called during Save operations to let the instance write the start element
        /// and return the next element in the collection.
        /// </summary>
        /// <param name="jsonWriter">The JXML writer used to write JSON.</param>
        /// <param name="currentIndex">The index within this collection.</param>
        /// <returns>The next item in the collection, or null of there are no more items.</returns>
        internal override JsonValue WriteStartElementAndGetNext(XmlDictionaryWriter jsonWriter, int currentIndex)
        {
            KeyValuePair<string, JsonValue> currentPair = indexedPairs[currentIndex];
            string currentKey = currentPair.Key;

            if (currentKey.Length == 0)
            {
                // special case in JXML world
                jsonWriter.WriteStartElement(JXmlToJsonValueConverter.ItemElementName, JXmlToJsonValueConverter.ItemElementName);
                jsonWriter.WriteAttributeString(JXmlToJsonValueConverter.ItemElementName, String.Empty);
            }
            else
            {
                jsonWriter.WriteStartElement(currentKey);
            }

            return currentPair.Value;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Context is required by CLR for this to work.")]
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            saveLock = new object();
        }

        private void RaiseItemChanging(JsonValue child, JsonValueChange change, string key)
        {
            if (ChangingListenersCount > 0)
            {
                RaiseChangingEvent(this, new JsonValueChangeEventArgs(child, change, key));
            }
        }

        private void RaiseItemChanged(JsonValue child, JsonValueChange change, string key)
        {
            if (ChangedListenersCount > 0)
            {
                RaiseChangedEvent(this, new JsonValueChangeEventArgs(child, change, key));
            }
        }
    }
}
