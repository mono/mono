//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;

    class DictionaryItemsCollection<TKey, TValue> : Collection<ModelItemKeyValuePair<TKey, TValue>>, IItemsCollection
    {
        IDictionary<TKey, TValue> dictionary;
        public bool ShouldUpdateDictionary { get; set; }
        public ModelItemDictionaryImpl ModelDictionary { get; set; }

        public DictionaryItemsCollection(object dictionary)
        {
            this.ShouldUpdateDictionary = true;
            this.dictionary = dictionary as IDictionary<TKey, TValue>;
            Fx.Assert(this.dictionary != null, "dictionary should be instantiated");
            foreach (KeyValuePair<TKey, TValue> kvpair in this.dictionary)
            {
                ModelItemKeyValuePair<TKey, TValue> item = new ModelItemKeyValuePair<TKey, TValue>(kvpair.Key, kvpair.Value);
                item.collection = this;
                base.InsertItem(this.Count, item);
            }
        }

        internal void PostUpdateKey()
        {
            this.UpdateDictionary();
        }

        internal void PreUpdateKey(TKey oldKey, TKey newKey)
        {
            this.dictionary.Remove(oldKey);
            if (this.dictionary.ContainsKey(newKey))
            {
                this.UpdateDictionary();
                throw FxTrace.Exception.AsError(new ArgumentException(SR.DuplicateKey));
            }
            if (this.ModelDictionary != null)
            {
                this.ModelDictionary.UpdateKey(oldKey, newKey);
            }
        }

        internal void UpdateValue(TKey key, TValue value)
        {
            if (ShouldUpdateDictionary)
            {
                this.dictionary[key] = value;
                if (this.ModelDictionary != null)
                {
                    this.ModelDictionary.UpdateValue(key, value);
                }
            }
        }

        protected override void ClearItems()
        {
            if (ShouldUpdateDictionary)
            {
                this.dictionary.Clear();
            }
            base.ClearItems();
        }

        protected override void InsertItem(int index, ModelItemKeyValuePair<TKey, TValue> item)
        {
            if (item == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
            }

            if (ShouldUpdateDictionary && this.dictionary.ContainsKey(item.Key))
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.DuplicateKey));
            }

            item.collection = this;
            base.InsertItem(index, item);

            this.UpdateDictionary();
        }

        protected override void RemoveItem(int index)
        {
            ModelItemKeyValuePair<TKey, TValue> item = this[index];
            Fx.Assert(item != null, "Item should not be null.");
            if (ShouldUpdateDictionary)
            {
                this.dictionary.Remove(item.Key);
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, ModelItemKeyValuePair<TKey, TValue> item)
        {
            if (item == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
            }
            item.collection = this;
            ModelItemKeyValuePair<TKey, TValue> oldItem = this[index];
            Fx.Assert(oldItem != null, "Item should not be null.");
            this.PreUpdateKey(oldItem.Key, item.Key);
            base.SetItem(index, item);
            this.PostUpdateKey();
        }

        void UpdateDictionary()
        {
            if (ShouldUpdateDictionary)
            {
                // Make sure the order of KVPairs in the dictionary is the same as the order of items in the collection
                this.dictionary.Clear();
                foreach (ModelItemKeyValuePair<TKey, TValue> item in this)
                {
                    this.dictionary.Add(new KeyValuePair<TKey, TValue>(item.Key, item.Value));
                }
            }
        }

    }
}
