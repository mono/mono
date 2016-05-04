//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Model 
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.Properties;
    using System.Runtime;

    // <summary>
    // Collection of ModelPropertyValues used to model collections of ModelItems
    // </summary>
    internal class ModelPropertyValueCollection : PropertyValueCollection 
    {

        private List<ModelPropertyIndexer> _values;
        private bool _listenToCollectionChanges = true;

        // <summary>
        // Basic ctor
        // </summary>
        // <param name="parentValue">Parent PropertyValue</param>
        public ModelPropertyValueCollection(ModelPropertyValue parentValue) : base(parentValue) 
        {
            if (parentValue == null)
            {
                throw FxTrace.Exception.ArgumentNull("parentValue");
            }

            // Wrap each existing item in the collection in ModelPropertyEntryIndexer
            ModelItemCollection collection = this.GetRawCollection();
            if (collection != null && collection.Count > 0) 
            {
                _values = new List<ModelPropertyIndexer>();
                int i = 0;
                foreach (ModelItem item in collection) 
                {
                    _values.Add(CreateModelPropertyIndexer(item, i++));
                }
            }

            // Hook into the collection changed events
            if (collection != null)
            {
                collection.CollectionChanged += new NotifyCollectionChangedEventHandler(OnUnderlyingCollectionChanged);
            }
        }

        // <summary>
        // Gets the number of items in this collection
        // </summary>
        public override int Count 
        {
            get {
                return _values == null ? 0 : _values.Count;
            }
        }

        // <summary>
        // Gets the PropertyValue at the specified index
        // </summary>
        // <param name="index">Index to look up</param>
        // <returns>PropertyValue at the specified index</returns>
        public override PropertyValue this[int index] {
            get {
                VerifyExistingIndex(index);
                return _values[index].PropertyValue;
            }
        }

        // <summary>
        // Adds the specified object to this collection, returning its wrapped version
        // </summary>
        // <param name="value">Value to add and wrap in PropertyValue</param>
        // <returns>Wrapped value</returns>
        public override PropertyValue Add(object value) 
        {
            return Insert(value, this.Count);
        }

        // <summary>
        // Inserts the specified object into this collection, returning its wrapped version
        // </summary>
        // <param name="value">Value to insert and wrap</param>
        // <param name="index">Index to insert at</param>
        // <returns>Wrapped version of the inserted value</returns>
        public override PropertyValue Insert(object value, int index) 
        {
            VerifyNewIndex(index);

            if (_values == null)
            {
                _values = new List<ModelPropertyIndexer>();
            }

            ModelItem item;
            bool previouslyActive = SnoozeListeningToCollectionChanges();
            try 
            {
                item = GetRawCollection().Insert(index, value);
            }
            finally 
            {
                StartListeningToCollectionChanges(previouslyActive);
            }

            return InsertExternal(item, index);
        }

        // Same as Insert(), except it doesn't modify the raw collection, because it assumes
        // that the raw collection was already modified externally.
        private PropertyValue InsertExternal(ModelItem item, int index) 
        {
            if (_values == null)
            {
                _values = new List<ModelPropertyIndexer>();
            }

            PropertyValue insertedValue = InsertHelper(item, index);

            // Fire OnChanged event
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, insertedValue, index));

            return insertedValue;
        }

        // Updates internal structures, but does not fire any notification
        private PropertyValue InsertHelper(ModelItem item, int index) 
        {
            // Only insert the value into the collection, if it's not already there.
            // Say that an ItemAdded event comes in even though this collection has not yet been used.
            // By requesting the instance of this collection for the first time, the collection
            // gets populated correctly and fully.  Now when InsertExternal is called as a result
            // of the ItemAdded event, we would be adding the new item into the collection twice.
            // We need to prevent that from happening.
            if (_values.Count > index &&
                object.Equals(_values[index].ModelItem, item))
            {
                return _values[index].PropertyValue;
            }

            ModelPropertyIndexer indexer = CreateModelPropertyIndexer(item, index);
            _values.Insert(index, indexer);

            // Adjust all indexes of the remaining indexers in the list
            for (int i = index + 1; i < _values.Count; i++)
            {
                _values[i].Index++;
            }

            return indexer.PropertyValue;
        }

        // <summary>
        // Removes the specified PropertyValue from the collection.
        // </summary>
        // <param name="property">Property to remove</param>
        // <returns>True, if the PropertyValue was found and removed, false
        // otherwise.</returns>
        public override bool Remove(PropertyValue propertyValue) 
        {
            if (propertyValue == null)
            {
                throw FxTrace.Exception.ArgumentNull("property");
            }

            if (_values == null)
            {
                return false;
            }

            for (int i = 0; i < _values.Count; i++) 
            {
                if (_values[i].PropertyValue == propertyValue) {
                    this.RemoveAt(i);
                    return true;
                }
            }

            // Call to RemoveAt() already fires the right CollectionChanged events
            return false;
        }

        // Same as Remove, except it doesn't modify the raw collection, because it's
        // assumed that the raw collection was already modified externally.
        private bool RemoveExternal(ModelItem item) 
        {
            Fx.Assert(item != null, "item parameter should not be null");
            Fx.Assert(_values != null, "_values parameter should not be null");

            for (int i = 0; i < _values.Count; i++) 
            {
                if (_values[i].ModelItem == item) {
                    this.RemoveAtExternal(i);
                    return true;
                }
            }

            return false;
        }

        // <summary>
        // Removes the PropertyValue at the specified index.
        // </summary>
        // <param name="index">Index at which to remove the value.</param>
        public override void RemoveAt(int index) 
        {
            VerifyExistingIndex(index);

            bool previouslyActive = SnoozeListeningToCollectionChanges();
            try 
            {
                this.GetRawCollection().RemoveAt(index);
            }
            finally 
            {
                StartListeningToCollectionChanges(previouslyActive);
            }

            RemoveAtExternal(index);
        }

        // Same as RemoveAt, except it doesn't modify the raw collection, because it's
        // assumed that the raw collection was already modified externally.
        private void RemoveAtExternal(int index) 
        {
            VerifyExistingIndex(index);
            PropertyValue removedValue = RemoveAtHelper(index);

            // Fire OnChanged event
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedValue, index));
        }

        // Updates internal structures, but does not fire any notification
        private PropertyValue RemoveAtHelper(int index) 
        {
            // invalidate the ModelPropertyEntryIndexer at the index and adjust all other indexes
            ModelPropertyIndexer indexer = _values[index];
            DestroyModelPropertyIndexer(indexer);

            _values.RemoveAt(index);
            for (int i = index; i < _values.Count; i++)
            {
                _values[i].Index--;
            }

            return indexer.PropertyValue;
        }

        // Replaces the old ModelItem with the new one, assuming that the raw collection
        // has already been verified
        private void ReplaceExternal(ModelItem oldItem, ModelItem newItem) 
        {
            Fx.Assert(_values != null, "_values parameter should not be null");
            Fx.Assert(oldItem != null, "oldItem parameter should not be null");
            Fx.Assert(newItem != null, "newItem parameter should not be null");

            for (int i = 0; i < _values.Count; i++) 
            {
                if (_values[i].ModelItem == oldItem) {
                    this.RemoveAtHelper(i);
                    this.InsertHelper(newItem, i);

                    // Fire OnChanged event
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, _values[i].PropertyValue));
                    return;
                }
            }

            Debug.Fail("Didn't find the expected item to remove");
        }

        // Clears the collection, assuming that the raw collection was already cleared externally
        private void ClearExternal() 
        {
            _values.Clear();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        // <summary>
        // Swaps the items at the specified indexes
        // </summary>
        // <param name="currentIndex">Index of item 1</param>
        // <param name="newIndex">Index of item 2</param>
        public override void SetIndex(int currentIndex, int newIndex) 
        {

            VerifyExistingIndex(currentIndex);
            VerifyExistingIndex(newIndex);

            if (currentIndex == newIndex)
            {
                return;
            }

            ModelItemCollection collection = this.GetRawCollection();
            ModelPropertyIndexer currentIndexer = _values[currentIndex];
            ModelPropertyIndexer newIndexer = _values[newIndex];

            bool previouslyActive = SnoozeListeningToCollectionChanges();
            try 
            {
                // Remove the higher index first (doesn't affect the value of the lower index)
                if (currentIndex < newIndex) 
                {
                    collection.RemoveAt(newIndex);
                    collection.RemoveAt(currentIndex);
                }
                else 
                {
                    collection.RemoveAt(currentIndex);
                    collection.RemoveAt(newIndex);
                }

                // Insert the lower index first (fixes the value of the higher index)
                if (currentIndex < newIndex) 
                {
                    collection.Insert(currentIndex, newIndexer.ModelItem);
                    collection.Insert(newIndex, currentIndexer.ModelItem);
                }
                else 
                {
                    collection.Insert(newIndex, currentIndexer.ModelItem);
                    collection.Insert(currentIndex, newIndexer.ModelItem);
                }
            }
            finally 
            {
                StartListeningToCollectionChanges(previouslyActive);
            }

            SetIndexExternal(currentIndex, newIndex);
        }

        // Same as SetIndex, except it doesn't modify the raw collection, because it's
        // assumed that the raw collection was already modified externally.
        private void SetIndexExternal(int currentIndex, int newIndex) 
        {

            if (currentIndex == newIndex)
            {
                return;
            }

            ModelPropertyIndexer currentIndexer = _values[currentIndex];
            ModelPropertyIndexer newIndexer = _values[newIndex];

            // Remove the higher index first (doesn't affect the value of the lower index)
            if (currentIndex < newIndex) 
            {
                _values.RemoveAt(newIndex);
                _values.RemoveAt(currentIndex);
            }
            else 
            {
                _values.RemoveAt(currentIndex);
                _values.RemoveAt(newIndex);
            }

            // Insert the lower index first (fixes the value of the higher index)
            if (currentIndex < newIndex) 
            {
                _values.Insert(currentIndex, newIndexer);
                _values.Insert(newIndex, currentIndexer);
            }
            else 
            {
                _values.Insert(newIndex, currentIndexer);
                _values.Insert(currentIndex, newIndexer);
            }

            newIndexer.Index = currentIndex;
            currentIndexer.Index = newIndex;

            // Fire OnChanged event
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Move, currentIndexer.PropertyValue, newIndex, currentIndex));
        }

        // <summary>
        // Gets the underlying ModelItemCollection
        // </summary>
        // <returns>The underlying ModelItemCollection</returns>
        internal ModelItemCollection GetRawCollection() 
        {
            ModelPropertyEntry parentAsEntry = ParentValue.ParentProperty as ModelPropertyEntry;
            if (parentAsEntry != null)
            {
                return parentAsEntry.FirstModelProperty.Collection;
            }

            ModelPropertyIndexer parentAsIndexer = ParentValue.ParentProperty as ModelPropertyIndexer;
            if (parentAsIndexer != null) 
            {
                ModelItemCollection modelItemCollection = parentAsIndexer.ModelItem as ModelItemCollection;

                // If the parent is an indexer, that means we are a collection within another collection
                // and the ModelItem of the indexer is really a ModelItemCollection.
                Fx.Assert(modelItemCollection != null, "modelItemCollection should not be null");

                return modelItemCollection;
            }

            Debug.Fail("A new class was introduced that derives from PropertyEntry.  Need to update ModelPropertyValueCollection code as well.");
            return null;
        }

        // <summary>
        // Gets the enumerator over this collection
        // </summary>
        // <returns>Enumerator over this collection</returns>
        public override IEnumerator<PropertyValue> GetEnumerator() 
        {
            if (_values == null)
            {
                yield break;
            }

            foreach (ModelPropertyIndexer value in _values) 
            {
                yield return value.PropertyValue;
            }
        }

        // Handler for all collection changed events that happen through the model
        private void OnUnderlyingCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) 
        {
            if (!_listenToCollectionChanges)
            {
                return;
            }

            switch (e.Action) 
            {
                case NotifyCollectionChangedAction.Add:
                    int startingIndex = e.NewStartingIndex < 0 ? this.Count : e.NewStartingIndex;
                    foreach (ModelItem item in e.NewItems) 
                    {
                        this.InsertExternal(item, startingIndex++);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (ModelItem item in e.OldItems) 
                    {
                        this.RemoveExternal(item);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    int oldIndex = e.OldStartingIndex, newIndex = e.NewStartingIndex;
                    for (int i = 0; i < e.OldItems.Count; i++) 
                    {
                        this.SetIndexExternal(oldIndex++, newIndex++);
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.OldItems.Count; i++) 
                    {
                        ModelItem oldItem = e.OldItems[i] as ModelItem;
                        ModelItem newItem = e.NewItems[i] as ModelItem;
                        this.ReplaceExternal(oldItem, newItem);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    this.ClearExternal();
                    break;
            }
        }

        // Activates the CollectionChanged event handler
        private void StartListeningToCollectionChanges(bool previousValue) 
        {
            _listenToCollectionChanges = previousValue;
        }

        // Suspends the CollectionChanged event handler
        private bool SnoozeListeningToCollectionChanges() 
        {
            bool previousValue = _listenToCollectionChanges;
            _listenToCollectionChanges = false;
            return previousValue;
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private void VerifyExistingIndex(int index) 
        {
            if (_values == null || _values.Count <= index || index < 0)
            {
                throw FxTrace.Exception.AsError(new IndexOutOfRangeException(index.ToString(CultureInfo.InvariantCulture)));
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private void VerifyNewIndex(int index) 
        {
            if ((_values == null && index != 0) ||
                (_values != null && _values.Count < index) ||
                index < 0)
            {
                throw FxTrace.Exception.AsError(new IndexOutOfRangeException(index.ToString(CultureInfo.InvariantCulture)));
            }
        }

        private ModelPropertyIndexer CreateModelPropertyIndexer(ModelItem item, int index) 
        {
            ModelPropertyIndexer indexer = new ModelPropertyIndexer(item, index, this);
            return indexer;
        }

        private static void DestroyModelPropertyIndexer(ModelPropertyIndexer indexer) 
        {
            indexer.Index = -1;
        }
    }
}
