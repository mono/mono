// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace System.Json
{
    /// <summary>
    /// A JsonArray is an ordered sequence of zero or more <see cref="System.Json.JsonValue"/> objects.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "Array already conveys the meaning of collection")]
    [DataContract]
    public sealed class JsonArray : JsonValue, IList<JsonValue>
    {
        [DataMember]
        private List<JsonValue> values = new List<JsonValue>();

        /// <summary>
        /// Creates an instance of the <see cref="System.Json.JsonArray"/> class initialized by
        /// an <see cref="System.Collections.Generic.IEnumerable{T}"/> enumeration of
        /// objects of type <see cref="System.Json.JsonValue"/>.
        /// </summary>
        /// <param name="items">The <see cref="System.Collections.Generic.IEnumerable{T}"/> enumeration
        /// of objects of type <see cref="System.Json.JsonValue"/> used to initialize the JavaScript Object Notation (JSON)
        /// array.</param>
        /// <exception cref="System.ArgumentNullException">If items is null.</exception>
        /// <exception cref="System.ArgumentException">If any of the items in the collection
        /// is a <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/> property of
        /// value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public JsonArray(IEnumerable<JsonValue> items)
        {
            AddRange(items);
        }

        /// <summary>
        /// Creates an instance of the <see cref="System.Json.JsonArray"/> class, initialized by an array of type <see cref="System.Json.JsonValue"/>.
        /// </summary>
        /// <param name="items">The array of type <see cref="System.Json.JsonValue"/> used to initialize the
        /// JavaScript Object Notation (JSON) array.</param>
        /// <exception cref="System.ArgumentException">If any of the items in the collection
        /// is a <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/> property of
        /// value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public JsonArray(params JsonValue[] items)
        {
            if (items != null)
            {
                AddRange(items);
            }
        }

        /// <summary>
        /// Gets the JSON type of this <see cref="System.Json.JsonArray"/>. The return value
        /// is always <see cref="F:System.Json.JsonType.Array"/>.
        /// </summary>
        public override JsonType JsonType
        {
            get { return JsonType.Array; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="System.Json.JsonArray"/> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return ((IList)values).IsReadOnly; }
        }

        /// <summary>
        /// Returns the number of <see cref="System.Json.JsonValue"/> elements in the array.
        /// </summary>
        public override int Count
        {
            get { return values.Count; }
        }

        /// <summary>
        /// Gets or sets the JSON value at a specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The <see cref="System.Json.JsonValue"/> element at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">If index is not a valid index for this array.</exception>
        /// <exception cref="System.ArgumentException">The property is set and the value is a
        /// <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/>
        /// property of value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public override JsonValue this[int index]
        {
            get { return values[index]; }

            set
            {
                if (value != null && value.JsonType == JsonType.Default)
                {
                    throw new ArgumentNullException("value", Properties.Resources.UseOfDefaultNotAllowed);
                }

                JsonValue oldValue = values[index];
                RaiseItemChanging(value, JsonValueChange.Replace, index);
                values[index] = value;
                RaiseItemChanged(oldValue, JsonValueChange.Replace, index);
            }
        }

        /// <summary>
        /// Adds the elements from a collection of type <see cref="System.Json.JsonValue"/> to this instance.
        /// </summary>
        /// <param name="items">Collection of items to add.</param>
        /// <exception cref="System.ArgumentNullException">If items is null.</exception>
        /// <exception cref="System.ArgumentException">If any of the items in the collection
        /// is a <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/> property of
        /// value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public void AddRange(IEnumerable<JsonValue> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (ChangingListenersCount > 0)
            {
                int index = Count;
                foreach (JsonValue toBeAdded in items)
                {
                    RaiseItemChanging(toBeAdded, JsonValueChange.Add, index++);
                }
            }

            foreach (JsonValue item in items)
            {
                if (item != null && item.JsonType == JsonType.Default)
                {
                    throw new ArgumentNullException("items", Properties.Resources.UseOfDefaultNotAllowed);
                }

                values.Add(item);
                RaiseItemChanged(item, JsonValueChange.Add, values.Count - 1);
            }
        }

        /// <summary>
        /// Adds the elements from an array of type <see cref="System.Json.JsonValue"/> to this instance.
        /// </summary>
        /// <param name="items">The array of type JsonValue to be added to this instance.</param>
        /// <exception cref="System.ArgumentNullException">If items is null.</exception>
        /// <exception cref="System.ArgumentException">If any of the items in the array
        /// is a <see cref="System.Json.JsonValue"/> with <see cref="System.Json.JsonValue.JsonType"/> property of
        /// value <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public void AddRange(params JsonValue[] items)
        {
            AddRange(items as IEnumerable<JsonValue>);
        }

        /// <summary>
        /// Searches for a specified object and returns the zero-based index of its first
        /// occurrence within this <see cref="System.Json.JsonArray"/>.
        /// </summary>
        /// <param name="item">The <see cref="System.Json.JsonValue"/> object to look up.</param>
        /// <returns>The zero-based index of the first occurrence of item within the
        /// <see cref="System.Json.JsonArray"/>, if found; otherwise, -1.</returns>
        public int IndexOf(JsonValue item)
        {
            return values.IndexOf(item);
        }

        /// <summary>
        /// Insert a JSON CLR type into the array at a specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the item should be inserted.</param>
        /// <param name="item">The <see cref="System.Json.JsonValue"/> object to insert.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">If index is less than zero or larger than
        /// the size of the array.</exception>
        /// <exception cref="System.ArgumentException">If the object to insert has a
        /// <see cref="System.Json.JsonValue.JsonType"/> property of value
        /// <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public void Insert(int index, JsonValue item)
        {
            if (item != null && item.JsonType == JsonType.Default)
            {
                throw new ArgumentNullException("item", Properties.Resources.UseOfDefaultNotAllowed);
            }

            RaiseItemChanging(item, JsonValueChange.Add, index);
            values.Insert(index, item);
            RaiseItemChanged(item, JsonValueChange.Add, index);
        }

        /// <summary>
        /// Remove the JSON value at a specified index of <see cref="System.Json.JsonArray"/>.
        /// </summary>
        /// <param name="index">The zero-based index at which to remove the <see cref="System.Json.JsonValue"/>.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">If index is less than zero or index
        /// is equal or larger than the size of the array.</exception>
        public void RemoveAt(int index)
        {
            JsonValue item = values[index];
            RaiseItemChanging(item, JsonValueChange.Remove, index);
            values.RemoveAt(index);
            RaiseItemChanged(item, JsonValueChange.Remove, index);
        }

        /// <summary>
        /// Adds a <see cref="System.Json.JsonValue"/> object to the end of the array.
        /// </summary>
        /// <param name="item">The <see cref="System.Json.JsonValue"/> object to add.</param>
        /// <exception cref="System.ArgumentException">If the object to add has a
        /// <see cref="System.Json.JsonValue.JsonType"/> property of value
        /// <see cref="F:System.Json.JsonType.Default"/>.</exception>
        public void Add(JsonValue item)
        {
            if (item != null && item.JsonType == JsonType.Default)
            {
                throw new ArgumentNullException("item", Properties.Resources.UseOfDefaultNotAllowed);
            }

            int index = Count;
            RaiseItemChanging(item, JsonValueChange.Add, index);
            values.Add(item);
            RaiseItemChanged(item, JsonValueChange.Add, index);
        }

        /// <summary>
        /// Removes all JSON CLR types from the <see cref="System.Json.JsonArray"/>.
        /// </summary>
        public void Clear()
        {
            RaiseItemChanging(null, JsonValueChange.Clear, 0);
            values.Clear();
            RaiseItemChanged(null, JsonValueChange.Clear, 0);
        }

        /// <summary>
        /// Checks whether a specified JSON CLR type is in the <see cref="System.Json.JsonArray"/>.
        /// </summary>
        /// <param name="item">The <see cref="System.Json.JsonValue"/> to check for in the array.</param>
        /// <returns>true if item is found in the <see cref="System.Json.JsonArray"/>; otherwise, false.</returns>
        public bool Contains(JsonValue item)
        {
            return values.Contains(item);
        }

        /// <summary>
        /// Copies the contents of the current JSON CLR array instance into a specified
        /// destination array beginning at the specified index.
        /// </summary>
        /// <param name="array">The destination array to which the elements of the current
        /// <see cref="System.Json.JsonArray"/> object are copied.</param>
        /// <param name="arrayIndex">The zero-based index in the destination array at which the
        /// copying of the elements of the JSON CLR array begins.</param>
        public void CopyTo(JsonValue[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of the specified JSON value from the array.
        /// </summary>
        /// <param name="item">The <see cref="System.Json.JsonValue"/> to remove from the <see cref="System.Json.JsonArray"/>.</param>
        /// <returns>true if item is successfully removed; otherwise, false. This method
        /// also returns false if item was not found in the <see cref="System.Json.JsonArray"/>.</returns>
        public bool Remove(JsonValue item)
        {
            int index = -1;
            if (ChangingListenersCount > 0 || ChangedListenersCount > 0)
            {
                index = IndexOf(item);
            }

            if (index >= 0)
            {
                RaiseItemChanging(item, JsonValueChange.Remove, index);
            }

            bool result = values.Remove(item);
            if (index >= 0)
            {
                RaiseItemChanged(item, JsonValueChange.Remove, index);
            }

            return result;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="System.Json.JsonValue"/> objects in the array.
        /// </summary>
        /// <returns>Returns an <see cref="System.Collections.IEnumerator"/> object that
        /// iterates through the <see cref="System.Json.JsonValue"/> elements in this <see cref="System.Json.JsonArray"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// Safe indexer for the <see cref="System.Json.JsonValue"/> type. 
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>If the index is within the array bounds and the value corresponding to the
        /// index is not null, then it will return that value. Otherwise it will return a
        /// <see cref="System.Json.JsonValue"/> instance with <see cref="System.Json.JsonValue.JsonType"/>
        /// equals to <see cref="F:System.Json.JsonType.Default"/>.</returns>
        public override JsonValue ValueOrDefault(int index)
        {
            if (index >= 0 && index < Count && this[index] != null)
            {
                return this[index];
            }

            return base.ValueOrDefault(index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="System.Json.JsonValue"/> objects in the array.
        /// </summary>
        /// <returns>Returns an <see cref="System.Collections.Generic.IEnumerator{T}"/> object that
        /// iterates through the <see cref="System.Json.JsonValue"/> elements in this <see cref="System.Json.JsonArray"/>.</returns>
        public new IEnumerator<JsonValue> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator which iterates through the values in this object.
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerator{T}"/> which iterates through the values in this object.</returns>
        /// <remarks>The enumerator returned by this class contains one pair for each element
        /// in this array, whose key is the element index (as a string), and the value is the
        /// element itself.</remarks>
        protected override IEnumerator<KeyValuePair<string, JsonValue>> GetKeyValuePairEnumerator()
        {
            for (int i = 0; i < values.Count; i++)
            {
                yield return new KeyValuePair<string, JsonValue>(i.ToString(CultureInfo.InvariantCulture), values[i]);
            }
        }

        /// <summary>
        /// Callback method called to let an instance write the proper JXML attribute when saving this
        /// instance.
        /// </summary>
        /// <param name="jsonWriter">The JXML writer used to write JSON.</param>
        internal override void WriteAttributeString(XmlDictionaryWriter jsonWriter)
        {
            if (jsonWriter == null)
            {
                throw new ArgumentNullException("jsonWriter");
            }

            jsonWriter.WriteAttributeString(JXmlToJsonValueConverter.TypeAttributeName, JXmlToJsonValueConverter.ArrayAttributeValue);
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
            if (jsonWriter == null)
            {
                throw new ArgumentNullException("jsonWriter");
            }

            jsonWriter.WriteStartElement(JXmlToJsonValueConverter.ItemElementName);
            JsonValue nextValue = this[currentIndex];
            return nextValue;
        }

        private void RaiseItemChanging(JsonValue child, JsonValueChange change, int index)
        {
            if (ChangingListenersCount > 0)
            {
                RaiseChangingEvent(this, new JsonValueChangeEventArgs(child, change, index));
            }
        }

        private void RaiseItemChanged(JsonValue child, JsonValueChange change, int index)
        {
            if (ChangedListenersCount > 0)
            {
                RaiseChangedEvent(this, new JsonValueChangeEventArgs(child, change, index));
            }
        }
    }
}
