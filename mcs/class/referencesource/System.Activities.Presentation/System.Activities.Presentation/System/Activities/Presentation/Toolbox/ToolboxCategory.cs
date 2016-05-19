//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.Toolbox
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    [SuppressMessage(FxCop.Category.Design, "CA1039:ListsAreStronglyTyped",
        Justification = "The nongeneric IList implementation is required for XAML support. It is implmented explicitly.")]
    [SuppressMessage(FxCop.Category.Naming, "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "The collection implemenation is required for XAML support.")]
    public sealed class ToolboxCategory : INotifyPropertyChanged, IList
    {
        string categoryName;
        ObservableCollection<ToolboxItemWrapper> tools = new ObservableCollection<ToolboxItemWrapper>();

        public ToolboxCategory()
            : this(string.Empty)
        {
        }

        public ToolboxCategory(string name)
        {
            this.categoryName = name;
            this.tools.CollectionChanged += this.OnToolCollectionChanged;
        }

        public string CategoryName
        {
            get { return this.categoryName; }
            set
            {
                this.categoryName = value;
                this.OnPropertyChanged("CategoryName");
            }
        }

        public ToolboxItemWrapper this[int index]
        {
            get { return this.tools[index]; }
        }

        [Fx.Tag.KnownXamlExternal]
        public ICollection<ToolboxItemWrapper> Tools
        {
            get { return this.tools; }
        }

        public void Add(ToolboxItemWrapper tool)
        {
            if (null == tool)
            {
                throw FxTrace.Exception.ArgumentNull("tool");
            }
            this.tools.Add(tool);
        }

        public bool Remove(ToolboxItemWrapper tool)
        {
            if (null == tool)
            {
                throw FxTrace.Exception.ArgumentNull("tool");
            }
            return this.tools.Remove(tool);
        }

        internal void HandleToolCollectionNotification(NotifyCollectionChangedEventHandler listener, bool register)
        {
            if (null == listener)
            {
                throw FxTrace.Exception.ArgumentNull("listener");
            }
            if (register)
            {
                this.tools.CollectionChanged += listener;
            }
            else
            {
                this.tools.CollectionChanged -= listener;
            }
        }

        void OnToolCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var tool in e.NewItems)
                    {
                        if (null == tool)
                        {
                            throw FxTrace.Exception.ArgumentNull("tool");
                        }
                    }
                    break;
            }
            this.OnPropertyChanged("Tools");
        }

        void OnPropertyChanged(string propertyName)
        {
            if (null != this.PropertyChanged)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region IList Members

        int IList.Add(object value)
        {
            this.Add((ToolboxItemWrapper)value);
            return this.tools.IndexOf((ToolboxItemWrapper)value);
        }

        void IList.Clear()
        {
            this.tools.Clear();
        }

        bool IList.Contains(object value)
        {
            if (null == value)
            {
                throw FxTrace.Exception.ArgumentNull("value");
            }
            return this.tools.Contains((ToolboxItemWrapper)value);
        }

        int IList.IndexOf(object value)
        {
            if (null == value)
            {
                throw FxTrace.Exception.ArgumentNull("value");
            }
            return this.tools.IndexOf((ToolboxItemWrapper)value);
        }

        void IList.Insert(int index, object value)
        {
            if (null == value)
            {
                throw FxTrace.Exception.ArgumentNull("value");
            }
            this.tools.Insert(index, (ToolboxItemWrapper)value);
        }

        bool IList.IsFixedSize
        {
            get { return ((IList)this.tools).IsFixedSize; }
        }

        bool IList.IsReadOnly
        {
            get { return ((IList)this.tools).IsReadOnly; }
        }

        void IList.Remove(object value)
        {
            this.Remove((ToolboxItemWrapper)value);
        }

        void IList.RemoveAt(int index)
        {
            this.tools.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return this.tools[index];
            }
            set
            {
                if (null == value)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }
                this.tools[index] = (ToolboxItemWrapper)value;
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
            ((ICollection)this.tools).CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return this.tools.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)this.tools).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)this.tools).SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.tools.GetEnumerator();
        }

        #endregion
    }
}
