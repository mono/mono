namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    [Flags]
    internal enum ItemListChangeAction
    {
        Add = 0x01,
        Remove = 0x2,
        Replace = Add | Remove
    }

    internal class ItemListChangeEventArgs<T> : EventArgs
    {
        private int index = 0;
        private ICollection<T> addedItems = null;
        private ICollection<T> removedItems = null;
        private object owner = null;
        private ItemListChangeAction action = ItemListChangeAction.Add;

        public ItemListChangeEventArgs(int index, ICollection<T> removedItems, ICollection<T> addedItems, object owner, ItemListChangeAction action)
        {
            this.index = index;
            this.removedItems = removedItems;
            this.addedItems = addedItems;
            this.action = action;
            this.owner = owner;
        }

        public ItemListChangeEventArgs(int index, T removedActivity, T addedActivity, object owner, ItemListChangeAction action)
        {
            this.index = index;
            if ((object)removedActivity != null)
            {
                this.removedItems = new List<T>();
                ((List<T>)this.removedItems).Add(removedActivity);
            }
            if ((object)addedActivity != null)
            {
                this.addedItems = new List<T>();
                ((List<T>)this.addedItems).Add(addedActivity);
            }
            this.action = action;
            this.owner = owner;
        }

        public IList<T> RemovedItems
        {
            get
            {
                return (this.removedItems != null) ? new List<T>(this.removedItems).AsReadOnly() : new List<T>().AsReadOnly();
            }
        }

        public IList<T> AddedItems
        {
            get
            {
                return (this.addedItems != null) ? new List<T>(this.addedItems).AsReadOnly() : new List<T>().AsReadOnly();
            }
        }

        public object Owner
        {
            get
            {
                return this.owner;
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }
        }

        public ItemListChangeAction Action
        {
            get
            {
                return this.action;
            }
        }
    }

    internal delegate void ItemListChangeEventHandler<T>(object sender, ItemListChangeEventArgs<T> e);

    internal class ItemList<T> : List<T>, IList<T>, IList
    {
        internal event ItemListChangeEventHandler<T> ListChanging;
        private object owner = null;

        internal ItemList(object owner)
        {
            this.owner = owner;
        }

        protected object Owner
        {
            get
            {
                return this.owner;
            }
        }

        bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        #region ItemList<T> Members

        public event ItemListChangeEventHandler<T> ListChanged;

        #endregion

        #region IList<T> Members

        void IList<T>.RemoveAt(int index)
        {
            if (index < 0 || index > base.Count)
                throw new ArgumentOutOfRangeException();

            T item = base[index];

            FireListChanging(new ItemListChangeEventArgs<T>(index, item, default(T), this.owner, ItemListChangeAction.Remove));
            base.RemoveAt(index);
            FireListChanged(new ItemListChangeEventArgs<T>(index, item, default(T), this.owner, ItemListChangeAction.Remove));
        }

        void IList<T>.Insert(int index, T item)
        {
            if (index < 0 || index > base.Count)
                throw new ArgumentOutOfRangeException();
            if ((object)item == null)
                throw new ArgumentNullException("item");

            FireListChanging(new ItemListChangeEventArgs<T>(index, default(T), item, this.owner, ItemListChangeAction.Add));
            base.Insert(index, item);
            FireListChanged(new ItemListChangeEventArgs<T>(index, default(T), item, this.owner, ItemListChangeAction.Add));
        }
        T IList<T>.this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException("item");

                T oldItem = base[index];
                FireListChanging(new ItemListChangeEventArgs<T>(index, oldItem, value, this.owner, ItemListChangeAction.Replace));
                base[index] = value;
                FireListChanged(new ItemListChangeEventArgs<T>(index, oldItem, value, this.owner, ItemListChangeAction.Replace));
            }
        }
        int IList<T>.IndexOf(T item)
        {
            return base.IndexOf(item);
        }

        #endregion

        #region ICollection<T> Members

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection<T>.Contains(T item)
        {
            return base.Contains(item);
        }

        bool ICollection<T>.Remove(T item)
        {
            if (!base.Contains(item))
                return false;

            int index = base.IndexOf(item);
            if (index >= 0)
            {
                FireListChanging(new ItemListChangeEventArgs<T>(index, item, default(T), this.owner, ItemListChangeAction.Remove));
                base.Remove(item);
                FireListChanged(new ItemListChangeEventArgs<T>(index, item, default(T), this.owner, ItemListChangeAction.Remove));
                return true;
            }
            return false;
        }

        void ICollection<T>.Clear()
        {
            ICollection<T> children = this.GetRange(0, this.Count);
            FireListChanging(new ItemListChangeEventArgs<T>(-1, children, null, this.owner, ItemListChangeAction.Remove));
            base.Clear();
            FireListChanged(new ItemListChangeEventArgs<T>(-1, children, null, this.owner, ItemListChangeAction.Remove));
        }

        void ICollection<T>.Add(T item)
        {
            if ((object)item == null)
                throw new ArgumentNullException("item");

            FireListChanging(new ItemListChangeEventArgs<T>(base.Count, default(T), item, this.owner, ItemListChangeAction.Add));
            base.Add(item);
            FireListChanged(new ItemListChangeEventArgs<T>(base.Count, default(T), item, this.owner, ItemListChangeAction.Add));
        }

        int ICollection<T>.Count
        {
            get
            {
                return base.Count;
            }
        }
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        #endregion

        public new void Add(T item)
        {
            ((IList<T>)this).Add(item);
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            FireListChanging(new ItemListChangeEventArgs<T>(-1, null, new List<T>(collection), this.owner, ItemListChangeAction.Add));
            base.AddRange(collection);
            FireListChanged(new ItemListChangeEventArgs<T>(base.Count, null, new List<T>(collection), this.owner, ItemListChangeAction.Add));
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            if (index < 0 || index > base.Count)
                throw new ArgumentOutOfRangeException();
            if (collection == null)
                throw new ArgumentNullException("collection");

            FireListChanging(new ItemListChangeEventArgs<T>(index, null, new List<T>(collection), this.owner, ItemListChangeAction.Add));
            base.InsertRange(index, collection);
            FireListChanged(new ItemListChangeEventArgs<T>(index, null, new List<T>(collection), this.owner, ItemListChangeAction.Add));
        }

        public new void Clear()
        {
            ((IList<T>)this).Clear();
        }

        public new void Insert(int index, T item)
        {
            ((IList<T>)this).Insert(index, item);
        }

        public new bool Remove(T item)
        {
            return ((IList<T>)this).Remove(item);
        }

        public new void RemoveAt(int index)
        {
            ((IList<T>)this).RemoveAt(index);
        }

        public new T this[int index]
        {
            get
            {
                return ((IList<T>)this)[index];
            }
            set
            {
                ((IList<T>)this)[index] = value;
            }
        }


        #region Helper methods

        protected virtual void FireListChanging(ItemListChangeEventArgs<T> eventArgs)
        {
            if (this.ListChanging != null)
                this.ListChanging(this, eventArgs);
        }

        protected virtual void FireListChanged(ItemListChangeEventArgs<T> eventArgs)
        {
            if (this.ListChanged != null)
                this.ListChanged(this, eventArgs);
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            if (!(value is T))
                throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
            ((IList<T>)this).Add((T)value);
            return this.Count - 1;
        }

        void IList.Clear()
        {
            ((IList<T>)this).Clear();
        }

        bool IList.Contains(object value)
        {
            if (!(value is T))
                throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
            return ((IList<T>)this).Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            if (!(value is T))
                throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
            return ((IList<T>)this).IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            if (!(value is T))
                throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
            ((IList<T>)this).Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return ((IList<T>)this).IsReadOnly;
            }
        }

        void IList.Remove(object value)
        {
            if (!(value is T))
                throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
            ((IList<T>)this).Remove((T)value);
        }
        object IList.this[int index]
        {
            get
            {
                return ((IList<T>)this)[index];
            }

            set
            {
                if (!(value is T))
                    throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
                ((IList<T>)this)[index] = (T)value;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            for (int loop = 0; loop < Count; loop++)
                array.SetValue(this[loop], loop + index);
        }
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        #endregion
    }

}
