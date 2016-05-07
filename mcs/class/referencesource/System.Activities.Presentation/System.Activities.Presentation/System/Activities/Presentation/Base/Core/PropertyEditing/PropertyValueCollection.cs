namespace System.Activities.Presentation.PropertyEditing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Activities.Presentation;

    /// <summary>
    /// This class encapsulates a collection of PropertyValue instances.  
    /// It is used for properties whose type is a collection.
    /// </summary>
    public abstract class PropertyValueCollection : IEnumerable<PropertyValue>, INotifyCollectionChanged
    {

        /// <summary>
        /// INotifyCollectionChanged event
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private PropertyValue _parentValue;

        /// <summary>
        /// Creates a PropertyValueCollection.
        /// </summary>
        /// <param name="parentValue">The parent PropertyValue.  This will be the property whose
        /// Type is a collection</param>
        /// <exception cref="ArgumentNullException">When parentValue is null</exception>
        protected PropertyValueCollection(PropertyValue parentValue)
        {
            if (parentValue == null)
                throw FxTrace.Exception.ArgumentNull("parentValue");

            _parentValue = parentValue;
        }

        /// <summary>
        /// Gets the parent PropertyValue.
        /// </summary>
        public PropertyValue ParentValue { get { return _parentValue; } }

        /// <summary>
        /// Indexer that returns a PropertyValue for the given index.
        /// </summary>
        /// <param name="index">The index of the item in the collection</param>
        /// <returns>The PropertyValue at that index</returns>
        public abstract PropertyValue this[int index] { get; }

        /// <summary>
        /// Adds an object into the collection.
        /// </summary>
        /// <param name="value">The object to add to the collection</param>
        /// <returns>The PropertyValue for the added object</returns>
        public abstract PropertyValue Add(object value);

        /// <summary>
        /// Inserts an object into the collection at the specified index
        /// </summary>
        /// <param name="value">The object to add to the collection</param>
        /// <param name="index">The index of where to add that object</param>
        /// <returns>A PropertyValue for the added object</returns>
        public abstract PropertyValue Insert(object value, int index);

        /// <summary>
        /// Removes the object from the collection.  The host needs to ensure that
        /// the PropertyValue is invalid after the object is removed.
        /// </summary>
        /// <param name="propertyValue">The item to remove from the collection</param>
        /// <returns>true if successful, otherwise false.</returns>
        public abstract bool Remove(PropertyValue propertyValue);

        /// <summary>
        /// Removes the object from the collection at the specified index.  The host needs to ensure that
        /// the PropertyValue is invalid after the object is removed.
        /// </summary>
        /// <param name="index">the index of the item to remove</param>
        public abstract void RemoveAt(int index);

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Used to reorder items in the collection (remove and add will invalidate the PropertyValue)
        /// when an item is moved to a new index, the items below the new index and the item at the new 
        /// index will slide down.
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="newIndex"></param>
        public abstract void SetIndex(int currentIndex, int newIndex);

        /// <summary>
        /// Returns a strongly typed IEnumerator for the collection of PropertyValues
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<PropertyValue> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Used to raise the CollectionChanged event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(this, e ?? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
