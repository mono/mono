//------------------------------------------------------------------------------
// <copyright file="ModelItemCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Presentation.Model {

    using System.Activities.Presentation.Internal.Properties;
    using System.Activities.Presentation;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Windows;

    /// <summary>
    /// ModelItemCollection derives from ModelItem and implements 
    /// support for a collection of items.  
    ///
    /// ModelItemCollection defines a static attached property called 
    /// Item.  This property is returned from the Properties 
    /// enumeration of the collection, in addition to any properties 
    /// defined on the collection.  The Item property represents all 
    /// the items in the collection and is defined as type 
    /// IEnumerable of ModelItem.  All items in the collection have 
    /// their Source property set to this property.  The property’s metadata 
    /// marks it non browsable and non-serializable.  The Item property is a 
    /// "pseudo" property because it is not actually set on the model.  The 
    /// value it points to is the ModelItemCollection itself.
    /// </summary>
    public abstract class ModelItemCollection : ModelItem, IList<ModelItem>, IList, INotifyCollectionChanged {

        /// <summary>
        /// Creates a new ModelItemCollection.
        /// </summary>
        protected ModelItemCollection() { }

        /// <summary>
        /// Returns the item at the given index.  Sets the item at the 
        /// given index to the given value.
        /// </summary>
        /// <param name="index">The zero-based index into the collection.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if value is null.</exception>
        /// <exception cref="IndexOutOfRangeException">if index is less than 0 or greater than or equal to count.</exception>
        public abstract ModelItem this[int index] { get; set; }

        /// <summary>
        /// Returns the count of items in the collection.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Returns true if the collection is a fixed size.  
        /// The default implementation returns true if the
        /// collection is read only.
        /// </summary>
        protected virtual bool IsFixedSize {
            get { return IsReadOnly; }
        }

        /// <summary>
        /// Returns true if the collection cannot be modified.
        /// </summary>
        public abstract bool IsReadOnly { get; }

        /// <summary>
        /// Protected access to ICollection.IsSynchronized.
        /// </summary>
        protected virtual bool IsSynchronized {
            get { return false; }
        }

        /// <summary>
        /// Protected access to the SyncRoot object used to synchronize
        /// this collection.  The default value returns "this".
        /// </summary>
        protected virtual object SyncRoot {
            get { return this; }
        }

        /// <summary>
        /// This event is raised when the contents of this collection change.
        /// </summary>
        public abstract event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Adds the item to the collection. 
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException">if item is null.</exception>
        /// <exception cref="InvalidOperationException">if the collection is read only.</exception>
        public abstract void Add(ModelItem item);

        /// <summary>
        /// Adds the given value to the collection.  This will create an item
        /// for the value.  It returns the newly created item.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>an item representing the value</returns>
        /// <exception cref="ArgumentNullException">if value is null.</exception>
        /// <exception cref="InvalidOperationException">if the collection is read only.</exception>
        public abstract ModelItem Add(object value);

        /// <summary>
        /// Clears the contents of the collection.
        /// </summary>
        /// <exception cref="InvalidOperationException">if the collection is read only.</exception>
        public abstract void Clear();

        /// <summary>
        /// Returns true if the collection contains the given item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if item is null.</exception>
        public abstract bool Contains(ModelItem item);

        /// <summary>
        /// Returns true if the collection contains the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if value is null.</exception>
        public abstract bool Contains(object value);

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
        /// Copies the contents of the collection into the given array.
        /// </summary>
        /// <exception cref="ArgumentNullException">if array is null.</exception>
        /// <exception cref="IndexOutOfRangeException">
        /// if arrayIndex is outside the bounds of the items array or if there is 
        /// insuffient space in the array to hold the collection.
        /// </exception>
        public abstract void CopyTo(ModelItem[] array, int arrayIndex);

        /// <summary>
        /// Returns an enumerator for the items in the collection.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<ModelItem> GetEnumerator();

        /// <summary>
        /// Returns the index of the given item or -1 if the item does not exist.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if item is null.</exception>
        public abstract int IndexOf(ModelItem item);

        /// <summary>
        /// Inserts the item at the given location.  To
        /// move an item, use Move.  If index is == Count this will insert the item 
        /// at the end.  If it is zero it will insert at the beginning.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException">if item is null.</exception>
        /// <exception cref="IndexOutOfRangeException">if index is less than 0 or greater than count.</exception>
        public abstract void Insert(int index, ModelItem item);

        /// <summary>
        /// Inserts the item at the given location.  To
        /// move an item, use Move.  If index is == Count this will insert the item 
        /// at the end.  If it is zero it will insert at the beginning.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns>an item representing the value</returns>
        /// <exception cref="ArgumentNullException">if value is null.</exception>
        /// <exception cref="IndexOutOfRangeException">if index is less than 0 or greater than count.</exception>
        public abstract ModelItem Insert(int index, object value);

        /// <summary>
        /// Moves the item at fromIndex to toIndex.  The value for toIndex is 
        /// always where you want the item to be according to how the collection 
        /// currently sits.  This means that if you are moving an item to a higher 
        /// index you don’t have to account for the fact that the indexes will 
        /// shuffle when the item is removed from its current location.
        /// </summary>
        /// <param name="fromIndex">
        /// The index of the item to move.
        /// </param>
        /// <param name="toIndex">
        /// The index to move it to.
        /// </param>
        /// <exception cref="IndexOutOfRangeException">
        /// if fromIndex or toIndex is less than zero or greater than or
        /// equal to Count.
        /// </exception>
        public abstract void Move(int fromIndex, int toIndex);

        /// <summary>
        /// Removes the item from the collection.  This does nothing if the 
        /// item does not exist in the collection.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException">if item is null.</exception>
        public abstract bool Remove(ModelItem item);

        /// <summary>
        /// Removes the value from the collection.  This does nothing if the 
        /// value does not exist in the collection.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException">if value is null.</exception>
        public abstract bool Remove(object value);

        /// <summary>
        /// Removes the item at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException">if index is less than 0 or greater than or equal to count.</exception>
        public abstract void RemoveAt(int index);

        /// <summary>
        /// This property is returned from the Properties enumeration of 
        /// the collection, in addition to any properties defined on the 
        /// collection.  The Item property represents all the items in 
        /// the collection and is defined as type IEnumerable of ModelItem.  
        /// All items in the collection have their Source property set to 
        /// this property.  The property’s metadata marks it non browsable 
        /// and non-serializable.  The Item property is a "pseudo"property 
        /// because it is not actually set on the model.  The value it points 
        /// to is the ModelItemCollection itself.
        /// </summary>
        public static readonly DependencyProperty ItemProperty = DependencyProperty.RegisterAttachedReadOnly(
            "Item",
            typeof(IEnumerable<ModelItem>),
            typeof(ModelItemCollection), null).DependencyProperty;

        #region IList Members

        /// <summary>
        /// IList Implementation maps back to public API.
        /// </summary>
        int IList.Add(object value) {
            Add(value);
            return Count - 1;
        }

        /// <summary>
        /// IList Implementation maps back to public API.
        /// </summary>
        void IList.Clear() {
            Clear();
        }

        /// <summary>
        /// IList Implementation maps back to public API.
        /// </summary>
        bool IList.Contains(object value) {
            return Contains(value);
        }

        /// <summary>
        /// IList Implementation maps back to public API.
        /// </summary>
        int IList.IndexOf(object value) {
            return IndexOf(ConvertType(value));
        }

        /// <summary>
        /// IList Implementation maps back to public API.
        /// </summary>
        void IList.Insert(int index, object value) {
            Insert(index, value);
        }

        /// <summary>
        /// IList Implementation maps back to public API.
        /// </summary>
        bool IList.IsFixedSize {
            get { return IsFixedSize; }
        }

        /// <summary>
        /// IList Implementation maps back to public API.
        /// </summary>
        bool IList.IsReadOnly {
            get { return IsReadOnly; }
        }

        /// <summary>
        /// IList Implementation maps back to public API.
        /// </summary>
        void IList.Remove(object value) {
            Remove(value);
        }

        /// <summary>
        /// IList Implementation maps back to public API.
        /// </summary>
        void IList.RemoveAt(int index) {
            RemoveAt(index);
        }

        /// <summary>
        /// IList Implementation maps back to public API.
        /// </summary>
        object IList.this[int index] {
            get { return this[index]; }
            set { this[index] = ConvertType(value); }
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// ICollection Implementation maps back to public API.
        /// </summary>
        void ICollection.CopyTo(Array array, int index) {
            for (int idx = 0; idx < Count; idx++) {
                array.SetValue(this[idx], idx + index);
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
        /// IEnumerable Implementation maps back to public API.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            foreach (object o in this) {
                yield return o;
            }
        }

        #endregion
    }
}
