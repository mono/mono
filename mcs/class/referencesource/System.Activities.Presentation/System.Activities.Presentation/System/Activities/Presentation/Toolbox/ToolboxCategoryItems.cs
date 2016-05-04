//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.Toolbox
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    //This class is responsible for storing information about tools
    //associated with given category item. The public interface is ICollection, the IList implementation is required for XAML support.
    [SuppressMessage(FxCop.Category.Design, "CA1039:ListsAreStronglyTyped",
        Justification = "The nongeneric IList implementation is required for XAML support. It is implmented explicitly.")]
    [SuppressMessage(FxCop.Category.Naming, "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "The collection suffix 'Items' suits better.")]
    public sealed class ToolboxCategoryItems : ICollection<ToolboxCategory>, IList
    {
        ObservableCollection<ToolboxCategory> categories;

        public ToolboxCategoryItems() : this(null)
        {
        }

        internal ToolboxCategoryItems(NotifyCollectionChangedEventHandler listener)
        {
            this.categories = new ObservableCollection<ToolboxCategory>();
            if (null != listener)
            {
                this.categories.CollectionChanged += listener;
            }
        }

        public ToolboxCategory this[int index]
        {
            get
            {
                return this.categories[index];
            }
        }

        #region ICollection<ToolboxCategory> Members
        
        public void Add(ToolboxCategory item)
        {
            if (null == item)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }
            this.categories.Add(item);
        }

        public void Clear()
        {
            this.categories.Clear();
        }

        public bool Contains(ToolboxCategory item)
        {
            if (null == item)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }
            return this.categories.Contains(item);
        }

        public void CopyTo(ToolboxCategory[] array, int arrayIndex)
        {
            if (null == array)
            {
                throw FxTrace.Exception.ArgumentNull("array");
            }
            this.categories.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.categories.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<ToolboxCategory>)this.categories).IsReadOnly; }
        }

        public bool Remove(ToolboxCategory item)
        {
            if (null == item)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }
            return this.categories.Remove(item);
        }

        #endregion

        #region IEnumerable<ToolboxCategory> Members

        public IEnumerator<ToolboxCategory> GetEnumerator()
        {
            return this.categories.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.categories.GetEnumerator();
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            this.Add((ToolboxCategory)value);
            return this.categories.IndexOf((ToolboxCategory)value);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((ToolboxCategory)value);
        }

        int IList.IndexOf(object value)
        {
            return this.categories.IndexOf((ToolboxCategory)value);
        }

        void IList.Insert(int index, object value)
        {
            if (null == value)
            {
                throw FxTrace.Exception.ArgumentNull("value");
            }
            this.categories.Insert(index, (ToolboxCategory)value);
        }

        bool IList.IsFixedSize
        {
            get { return ((IList)this.categories).IsFixedSize; }
        }

        bool IList.IsReadOnly
        {
            get { return ((IList)this.categories).IsReadOnly; }
        }

        void IList.Remove(object value)
        {
            this.Remove((ToolboxCategory)value);
        }

        void IList.RemoveAt(int index)
        {
            this.categories.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return (this)[index];
            }
            set
            {
                if (null == value)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }
                this.categories[index] = (ToolboxCategory)value;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            if (null == array)
            {
                throw FxTrace.Exception.ArgumentNull("array");
            }
            ((ICollection)this.categories).CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return this.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)this.categories).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)this.categories).SyncRoot; }
        }

        #endregion
    }
}
