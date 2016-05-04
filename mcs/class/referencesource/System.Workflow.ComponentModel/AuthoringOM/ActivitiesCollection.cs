namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Serialization;

    #region Class ActivityCollectionItemList
    [DesignerSerializer(typeof(ActivityCollectionMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityCollection : List<Activity>, IList<Activity>, IList
    {
        private Activity owner = null;

        internal event EventHandler<ActivityCollectionChangeEventArgs> ListChanging;
        public event EventHandler<ActivityCollectionChangeEventArgs> ListChanged;

        public ActivityCollection(Activity owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (!(owner is Activity))
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "owner");

            this.owner = owner;
        }

        private void FireListChanging(ActivityCollectionChangeEventArgs eventArgs)
        {
            if (this.ListChanging != null)
                this.ListChanging(this, eventArgs);
        }

        private void FireListChanged(ActivityCollectionChangeEventArgs eventArgs)
        {
            if (this.ListChanged != null)
                this.ListChanged(this, eventArgs);
        }

        internal Activity Owner
        {
            get
            {
                return this.owner;
            }
        }

        internal void InnerAdd(Activity activity)
        {
            base.Add(activity);
        }

        #region IList<Activity> Members

        void IList<Activity>.RemoveAt(int index)
        {
            if (index < 0 || index >= base.Count)
                throw new ArgumentOutOfRangeException("Index");

            Activity item = base[index];

            ActivityCollectionChangeEventArgs args = new ActivityCollectionChangeEventArgs(index, item, null, this.owner, ActivityCollectionChangeAction.Remove);
            FireListChanging(args);
            base.RemoveAt(index);
            FireListChanged(args);
        }

        void IList<Activity>.Insert(int index, Activity item)
        {
            if (index < 0 || index > base.Count)
                throw new ArgumentOutOfRangeException("index");
            if (item == null)
                throw new ArgumentNullException("item");

            ActivityCollectionChangeEventArgs args = new ActivityCollectionChangeEventArgs(index, null, item, this.owner, ActivityCollectionChangeAction.Add);
            FireListChanging(args);
            base.Insert(index, item);
            FireListChanged(args);
        }

        Activity IList<Activity>.this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("item");

                Activity oldItem = base[index];
                ActivityCollectionChangeEventArgs args = new ActivityCollectionChangeEventArgs(index, oldItem, value, this.owner, ActivityCollectionChangeAction.Replace);
                FireListChanging(args);
                base[index] = value;
                FireListChanged(args);
            }
        }
        int IList<Activity>.IndexOf(Activity item)
        {
            return base.IndexOf(item);
        }

        #endregion

        #region ICollection<Activity> Members
        bool ICollection<Activity>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection<Activity>.Contains(Activity item)
        {
            return base.Contains(item);
        }

        bool ICollection<Activity>.Remove(Activity item)
        {
            if (!base.Contains(item))
                return false;

            int index = base.IndexOf(item);
            if (index >= 0)
            {
                ActivityCollectionChangeEventArgs args = new ActivityCollectionChangeEventArgs(index, item, null, this.owner, ActivityCollectionChangeAction.Remove);
                FireListChanging(args);
                base.Remove(item);
                FireListChanged(args);
                return true;
            }
            return false;
        }

        void ICollection<Activity>.Clear()
        {
            ICollection<Activity> children = base.GetRange(0, base.Count);
            ActivityCollectionChangeEventArgs args = new ActivityCollectionChangeEventArgs(-1, children, null, this.owner, ActivityCollectionChangeAction.Remove);
            FireListChanging(args);
            base.Clear();
            FireListChanged(args);
        }

        void ICollection<Activity>.Add(Activity item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            ActivityCollectionChangeEventArgs args = new ActivityCollectionChangeEventArgs(base.Count, null, item, this.owner, ActivityCollectionChangeAction.Add);
            FireListChanging(args);
            base.Add(item);
            FireListChanged(args);
        }

        int ICollection<Activity>.Count
        {
            get
            {
                return base.Count;
            }
        }
        void ICollection<Activity>.CopyTo(Activity[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        #endregion

        #region IEnumerable<Activity> Members

        IEnumerator<Activity> IEnumerable<Activity>.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        #endregion

        #region Member Implementations
        public new int Count
        {
            get
            {
                return ((ICollection<Activity>)this).Count;
            }
        }

        public new void Add(Activity item)
        {
            ((IList<Activity>)this).Add(item);
        }

        public new void Clear()
        {
            ((IList<Activity>)this).Clear();
        }

        public new void Insert(int index, Activity item)
        {
            ((IList<Activity>)this).Insert(index, item);
        }

        public new bool Remove(Activity item)
        {
            return ((IList<Activity>)this).Remove(item);
        }

        public new void RemoveAt(int index)
        {
            ((IList<Activity>)this).RemoveAt(index);
        }

        public new Activity this[int index]
        {
            get
            {
                return ((IList<Activity>)this)[index];
            }
            set
            {
                ((IList<Activity>)this)[index] = value;
            }
        }

        public Activity this[string key]
        {
            get
            {
                for (int index = 0; index < this.Count; index++)
                    if ((this[index].Name.Equals(key) || this[index].QualifiedName.Equals(key)))
                        return this[index];
                return null;
            }
        }

        public new int IndexOf(Activity item)
        {
            return ((IList<Activity>)this).IndexOf(item);
        }

        public new bool Contains(Activity item)
        {
            return ((IList<Activity>)this).Contains(item);
        }

        public new IEnumerator<Activity> GetEnumerator()
        {
            return ((IList<Activity>)this).GetEnumerator();
        }
        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            if (!(value is Activity))
                throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
            ((IList<Activity>)this).Add((Activity)value);
            return this.Count - 1;
        }

        void IList.Clear()
        {
            ((IList<Activity>)this).Clear();
        }

        bool IList.Contains(object value)
        {
            if (!(value is Activity))
                throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
            return (((IList<Activity>)this).Contains((Activity)value));
        }

        int IList.IndexOf(object value)
        {
            if (!(value is Activity))
                throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
            return ((IList<Activity>)this).IndexOf((Activity)value);
        }

        void IList.Insert(int index, object value)
        {
            if (!(value is Activity))
                throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
            ((IList<Activity>)this).Insert(index, (Activity)value);
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
                return ((IList<Activity>)this).IsReadOnly;
            }
        }

        void IList.Remove(object value)
        {
            if (!(value is Activity))
                throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
            ((IList<Activity>)this).Remove((Activity)value);
        }
        object IList.this[int index]
        {
            get
            {
                return ((IList<Activity>)this)[index];
            }

            set
            {
                if (!(value is Activity))
                    throw new Exception(SR.GetString(SR.Error_InvalidListItem, this.GetType().GetGenericArguments()[0].FullName));
                ((IList<Activity>)this)[index] = (Activity)value;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            for (int loop = 0; loop < this.Count; loop++)
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
            return (IEnumerator)((IList<Activity>)this).GetEnumerator();
        }

        #endregion

    }
    #endregion

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum ActivityCollectionChangeAction
    {
        Add = 0x00,
        Remove = 0x01,
        Replace = 0x02
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityCollectionChangeEventArgs : EventArgs
    {
        private int index = 0;
        private ICollection<Activity> addedItems = null;
        private ICollection<Activity> removedItems = null;
        private object owner = null;
        private ActivityCollectionChangeAction action = ActivityCollectionChangeAction.Add;

        public ActivityCollectionChangeEventArgs(int index, ICollection<Activity> removedItems, ICollection<Activity> addedItems, object owner, ActivityCollectionChangeAction action)
        {
            this.index = index;
            this.removedItems = removedItems;
            this.addedItems = addedItems;
            this.action = action;
            this.owner = owner;
        }

        public ActivityCollectionChangeEventArgs(int index, Activity removedActivity, Activity addedActivity, object owner, ActivityCollectionChangeAction action)
        {
            this.index = index;
            if (removedActivity != null)
            {
                this.removedItems = new List<Activity>();
                ((List<Activity>)this.removedItems).Add(removedActivity);
            }
            if (addedActivity != null)
            {
                this.addedItems = new List<Activity>();
                ((List<Activity>)this.addedItems).Add(addedActivity);
            }
            this.action = action;
            this.owner = owner;
        }

        public IList<Activity> RemovedItems
        {
            get
            {
                return (this.removedItems != null) ? new List<Activity>(this.removedItems).AsReadOnly() : new List<Activity>().AsReadOnly();
            }
        }

        public IList<Activity> AddedItems
        {
            get
            {
                return (this.addedItems != null) ? new List<Activity>(this.addedItems).AsReadOnly() : new List<Activity>().AsReadOnly();
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

        public ActivityCollectionChangeAction Action
        {
            get
            {
                return this.action;
            }
        }
    }
}
