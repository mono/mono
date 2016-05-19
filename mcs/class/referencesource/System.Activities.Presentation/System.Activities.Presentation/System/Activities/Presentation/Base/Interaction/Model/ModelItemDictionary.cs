//------------------------------------------------------------------------------
// <copyright file="ModelItemDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Presentation.Model {

    using System.Activities.Presentation.Internal.Properties;
    using System.Activities.Presentation;
    using System.Runtime;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;

    /// <summary>
    /// ModelItemDictionary derives from ModelItem and implements support for a 
    /// dictionary of key/value pairs.  Both the keys and values are items. 
    /// 
    /// ModelItemDictionary defines an attached property "Key", which is adds 
    /// to all items contained in the dictionary.  The data type of the Key 
    /// property is "ModelItem" and it is marked as non-browsable and 
    /// non-serializable.
    ///
    /// In addition to the Key property, ModelItemDictionary also returns an 
    /// Item property from its properties collection just like ModelItemCollection.  
    /// ModelItemDictionary reuses the ModelProperty defined on ModelItemCollection.  
    /// The value returned is an enumeration of the values in the dictionary.  
    /// The Source property of all items in the dictionary refers to this Item 
    /// property.
    /// </summary>
    public abstract class ModelItemDictionary : ModelItem, IDictionary<ModelItem, ModelItem>, IDictionary, INotifyCollectionChanged {

        /// <summary>
        /// Creates a new ModelItemDictionary.
        /// </summary>
        protected ModelItemDictionary() { }

        /// <summary>
        /// Returns the item at the given key.  Sets the item at the given 
        /// key to the given value.  If there is no item for the given key 
        /// this returns null because null is not a valid item.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">if the dictionary is read only and you set a new value.</exception>
        /// <exception cref="KeyNotFoundException">if the given key is not in the dictionary.</exception>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public abstract ModelItem this[ModelItem key] { get; set; }

        /// <summary>
        /// Returns the item at the given key.  Sets the item at the given 
        /// key to the given value.  If there is no item for the given key 
        /// this returns null because null is not a valid item.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">if the dictionary is read only and you set a new value.</exception>
        /// <exception cref="KeyNotFoundException">if the given key is not in the dictionary.</exception>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public abstract ModelItem this[object key] { get; set; }

        /// <summary>
        /// Returns the count of items in the dictionary.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Returns true if the dictionary is a fixed size.  
        /// The default implementation returns true if the
        /// dictionary is read only.
        /// </summary>
        protected virtual bool IsFixedSize {
            get { return IsReadOnly; }
        }

        /// <summary>
        /// Returns true if the dictionary cannot be modified.
        /// </summary>
        public abstract bool IsReadOnly { get; }

        /// <summary>
        /// Protected access to ICollection.IsSynchronized.
        /// </summary>
        protected virtual bool IsSynchronized {
            get { return false; }
        }

        /// <summary>
        /// Returns the keys of the collection.  The keys are guaranteed to be in 
        /// the same order as the values.  The resulting collection is read-only.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public abstract ICollection<ModelItem> Keys { get; }

        /// <summary>
        /// Protected access to the SyncRoot object used to synchronize
        /// this collection.  The default value returns "this".
        /// </summary>
        protected virtual object SyncRoot {
            get { return this; }
        }

        /// <summary>
        /// Returns the values of the collection.  The values are guaranteed to be 
        /// in the same order as the keys.  The resulting collection is read-only.
        /// </summary>
        [Fx.Tag.KnownXamlExternalAttribute]
        public abstract ICollection<ModelItem> Values { get; }

        /// <summary>
        /// This event is raised when the contents of this collection change.
        /// </summary>
        public abstract event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Adds the item to the dictionary under the given key. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException">if the dictionary is read only.</exception>
        public abstract void Add(ModelItem key, ModelItem value);

        /// <summary>
        /// Adds the value to the dictionary under the given key.  This
        /// will wrap the key and value in an item.  It returns the item
        /// representing the key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>an item representing the key in the dictionary</returns>
        /// <exception cref="InvalidOperationException">if the dictionary is read only.</exception>
        public abstract ModelItem Add(object key, object value);

        /// <summary>
        /// Clears the contents of the dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">if the dictionary is read only.</exception>
        public abstract void Clear();

        /// <summary>
        /// Copies into the given array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        protected virtual void CopyTo(KeyValuePair<ModelItem, ModelItem>[] array, int arrayIndex) {
            foreach (KeyValuePair<ModelItem, ModelItem> kv in this) {
                array[arrayIndex++] = kv;
            }
        }

        /// <summary>
        /// Returns true if the dictionary contains the given key value pair.
        /// </summary>
        protected virtual bool Contains(KeyValuePair<ModelItem, ModelItem> item) {
            ModelItem value;
            return TryGetValue(item.Key, out value) && value == item.Value;
        }

        /// <summary>
        /// Returns true if the dictionary contains the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract bool ContainsKey(ModelItem key);

        /// <summary>
        /// Returns true if the dictionary contains the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract bool ContainsKey(object key);

        //
        // Helper method that verifies that objects can be upcast to
        // the correct type.
        //
        private static ModelItem ConvertType(object value) {
            try {
                return (ModelItem)value;
            }
            catch (InvalidCastException) {
                throw FxTrace.Exception.AsError(new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture,
                        Resources.Error_ArgIncorrectType,
                        "value", typeof(ModelItem).FullName)));
            }
        }

        /// <summary>
        /// Returns an enumerator for the items in the dictionary.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public abstract IEnumerator<KeyValuePair<ModelItem, ModelItem>> GetEnumerator();

        /// <summary>
        /// Removes the item from the dictionary.  This does nothing if the item 
        /// does not exist in the collection.
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="InvalidOperationException">if the dictionary is read only.</exception>
        public abstract bool Remove(ModelItem key);

        /// <summary>
        /// Removes the item from the dictionary.  This does nothing if the item 
        /// does not exist in the collection.
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="InvalidOperationException">if the dictionary is read only.</exception>
        public abstract bool Remove(object key);

        /// <summary>
        /// Retrieves the value for the given key, or returns false if the 
        /// value can’t be found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool TryGetValue(ModelItem key, out ModelItem value);

        /// <summary>
        /// Retrieves the value for the given key, or returns false if the 
        /// value can’t be found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool TryGetValue(object key, out ModelItem value);

        /// <summary>
        /// ModelItemDictionary provides an attached property "Key", which is adds to 
        /// all items contained in the dictionary.  The data type of the Key 
        /// property is "ModelItem".
        /// </summary>
        public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttachedReadOnly(
            "Key",
            typeof(ModelItem),
            typeof(ModelItemDictionary), null).DependencyProperty;

        // IDictionary API is synthesized on top of the generic API.

        #region IDictionary Members

        /// <summary>
        /// IDictionary Implementation maps back to public API.
        /// </summary>
        void IDictionary.Add(object key, object value) {
            Add(key, value);
        }

        /// <summary>
        /// IDictionary Implementation maps back to public API.
        /// </summary>
        void IDictionary.Clear() {
            Clear();
        }

        /// <summary>
        /// IDictionary Implementation maps back to public API.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool IDictionary.Contains(object key) {
            return ContainsKey(key);
        }

        /// <summary>
        /// IDictionary Implementation maps back to public API.
        /// </summary>
        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new DictionaryEnumerator(GetEnumerator());
        }

        /// <summary>
        /// IDictionary Implementation maps back to public API.
        /// </summary>
        bool IDictionary.IsFixedSize {
            get { return IsFixedSize; }
        }

        /// <summary>
        /// IDictionary Implementation maps back to public API.
        /// </summary>
        bool IDictionary.IsReadOnly {
            get { return IsReadOnly; }
        }

        /// <summary>
        /// IDictionary Implementation maps back to public API.
        /// </summary>
        ICollection IDictionary.Keys
        {
            get {
                object[] keys = new object[Count];
                int idx = 0;
                foreach (KeyValuePair<ModelItem, ModelItem> kv in this) {
                    keys[idx++] = kv.Key;
                }
                return keys;
            }
        }

        /// <summary>
        /// IDictionary Implementation maps back to public API.
        /// </summary>
        void IDictionary.Remove(object key) {
            Remove(key);
        }

        /// <summary>
        /// IDictionary Implementation maps back to public API.
        /// </summary>
        ICollection IDictionary.Values
        {
            get {
                object[] values = new object[Count];
                int idx = 0;
                foreach (KeyValuePair<ModelItem, ModelItem> kv in this) {
                    values[idx++] = kv.Value;
                }
                return values;
            }
        }

        /// <summary>
        /// IDictionary Implementation maps back to public API.
        /// </summary>
        object IDictionary.this[object key] {
            get { return this[ConvertType(key)]; }
            set { this[ConvertType(key)] = ConvertType(value); }
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// ICollection Implementation maps back to public API.
        /// </summary>
        void ICollection.CopyTo(Array array, int index) {

            if (Count > 0) {
                int len = array.GetLength(0);
                if (index >= len) {
                    throw FxTrace.Exception.AsError(new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.Error_InvalidArrayIndex, index)));
                }

                KeyValuePair<ModelItem, ModelItem>[] typedArray = new KeyValuePair<ModelItem, ModelItem>[len];

                CopyTo(typedArray, index);

                for (; index < typedArray.Length; index++) {
                    array.SetValue(typedArray[index], index);
                }
            }
        }

        /// <summary>
        /// ICollection Implementation maps back to public API.
        /// </summary>
        int ICollection.Count {
            get { return Count; }
        }

        /// <summary>
        /// ICollection Implementation maps back to public API.
        /// </summary>
        bool ICollection.IsSynchronized {
            get { return IsSynchronized; }
        }

        /// <summary>
        /// ICollection Implementation maps back to public API.
        /// </summary>
        object ICollection.SyncRoot {
            get { return SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// IEnumerator Implementation maps back to public API.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            foreach (KeyValuePair<ModelItem, ModelItem> kv in this) {
                yield return kv;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<ModelItem,ModelItem>> Members

        void ICollection<KeyValuePair<ModelItem, ModelItem>>.Add(KeyValuePair<ModelItem, ModelItem> item) {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<ModelItem, ModelItem>>.Contains(KeyValuePair<ModelItem, ModelItem> item) {
            return Contains(item);
        }

        void ICollection<KeyValuePair<ModelItem, ModelItem>>.CopyTo(KeyValuePair<ModelItem, ModelItem>[] array, int arrayIndex) {
            if (arrayIndex >= array.Length) {
                throw FxTrace.Exception.AsError(new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture,
                    Resources.Error_InvalidArrayIndex, arrayIndex)));
            }

            CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<ModelItem, ModelItem>>.Remove(KeyValuePair<ModelItem, ModelItem> item) {
            ModelItem value;
            if (TryGetValue(item.Key, out value) && value == item.Value) {
                return Remove(item.Key);
            }

            return false;
        }

        #endregion

        //
        // This is a simple struct that implements a dictionary enumerator,
        // since this isn't supported in the iterator pattern.
        //
        private struct DictionaryEnumerator : IDictionaryEnumerator {

            private IEnumerator<KeyValuePair<ModelItem, ModelItem>> _real;

            internal DictionaryEnumerator(IEnumerator<KeyValuePair<ModelItem, ModelItem>> real) {
                _real = real;
            }

            #region IDictionaryEnumerator Members

            public DictionaryEntry Entry {
                get { return new DictionaryEntry(_real.Current.Key, _real.Current.Value); }
            }

            public object Key {
                get { return _real.Current.Key; }
            }

            public object Value {
                get { return _real.Current.Value; }
            }

            #endregion

            #region IEnumerator Members

            public object Current {
                get { return Entry; }
            }

            public bool MoveNext() {
                return _real.MoveNext();
            }

            public void Reset() {
                _real.Reset();
            }

            #endregion
        }
    }
}
