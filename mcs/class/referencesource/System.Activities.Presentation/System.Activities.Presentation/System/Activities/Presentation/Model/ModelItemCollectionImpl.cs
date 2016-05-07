//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Markup;
    using System.Activities.Presentation.Services;
    using System.Linq;
    using System.Runtime;

    // This class provides the implementation for the ModelItemCollection. This provides
    // a container for the child modelItems for the entries in a collection.

    class ModelItemCollectionImpl : ModelItemCollection, IModelTreeItem
    {
        ModelProperty contentProperty;
        object instance;
        Type itemType;
        List<ModelItem> modelItems;
        Dictionary<string, ModelItem> modelPropertyStore;
        ModelTreeManager modelTreeManager;
        ModelProperty nameProperty;
        List<ModelItem> parents;
        ModelPropertyCollectionImpl properties;
        List<ModelProperty> sources;
        ModelTreeItemHelper helper;
        List<ModelItem> subTreeNodesThatNeedBackLinkPatching;
        DependencyObject view;
        ModelItem manuallySetParent;

        public ModelItemCollectionImpl(ModelTreeManager modelTreeManager, Type itemType, Object instance, ModelItem parent)
        {
            Fx.Assert(modelTreeManager != null, "modelTreeManager cannot be null");
            Fx.Assert(itemType != null, "item type cannot be null");
            Fx.Assert(instance != null, "instance cannot be null");
            this.itemType = itemType;
            this.instance = instance;
            this.modelTreeManager = modelTreeManager;
            this.parents = new List<ModelItem>(1);
            this.sources = new List<ModelProperty>(1);
            this.helper = new ModelTreeItemHelper();
            if (parent != null)
            {
                this.manuallySetParent = parent;
            }
            this.modelPropertyStore = new Dictionary<string, ModelItem>();
            this.subTreeNodesThatNeedBackLinkPatching = new List<ModelItem>();
            this.modelItems = new List<ModelItem>();
            UpdateInstance(instance);
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public override event PropertyChangedEventHandler PropertyChanged;

        public override AttributeCollection Attributes
        {
            get
            {
                Fx.Assert(this.itemType != null, "item type cannot be null");
                return TypeDescriptor.GetAttributes(this.itemType);
            }
        }

        public override ModelProperty Content
        {
            get
            {
                if (this.contentProperty == null)
                {
                    Fx.Assert(this.instance != null, "instance cannot be null");
                    ContentPropertyAttribute contentAttribute = TypeDescriptor.GetAttributes(this.instance)[typeof(ContentPropertyAttribute)] as ContentPropertyAttribute;
                    if (contentAttribute != null && !String.IsNullOrEmpty(contentAttribute.Name))
                    {
                        this.contentProperty = this.Properties.Find(contentAttribute.Name);
                    }
                }
                return contentProperty;
            }
        }

        public override int Count
        {
            get
            {
                Fx.Assert(instance != null, "instance cannot be null");
                if (instance != null)
                {
                    return ((ICollection)instance).Count;
                }
                return 0;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                IList instanceList = instance as IList;
                Fx.Assert(instanceList != null, "instance should be IList");
                return instanceList.IsReadOnly;
            }
        }

        public override Type ItemType
        {
            get
            {
                return this.itemType;
            }
        }

        public override string Name
        {
            get
            {
                string name = null;
                if ((this.NameProperty != null) && (this.NameProperty.Value != null))
                {
                    name = (string)this.NameProperty.Value.GetCurrentValue();
                }
                return name;
            }
            set
            {
                this.NameProperty.SetValue(value);
            }
        }

        public override ModelItem Parent
        {
            get
            {
                return (this.Parents.Count() > 0) ? this.Parents.First() : null;
            }

        }

        public override ModelPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    properties = new ModelPropertyCollectionImpl(this);
                }
                return properties;
            }
        }

        public override ModelItem Root
        {
            get
            {
                return this.modelTreeManager.Root;
            }
        }

        public override ModelProperty Source
        {
            get
            {
                return (this.sources.Count > 0) ? this.sources.First() : null;
            }
        }

        public override DependencyObject View
        {
            get
            {
                return this.view;
            }
        }

        public override IEnumerable<ModelItem> Parents
        {
            get
            {
                if (this.manuallySetParent != null)
                {
                    List<ModelItem> list = new List<ModelItem>();
                    list.Add(this.manuallySetParent);
                    return list.Concat(this.parents).Concat(
                        from source in this.sources
                        select source.Parent);
                }

                return this.parents.Concat(
                    from source in this.sources
                    select source.Parent);
            }
        }

        public override IEnumerable<ModelProperty> Sources
        {
            get
            {
                return this.sources;
            }
        }

        internal List<ModelItem> Items
        {
            get
            {
                return modelItems;
            }
        }

        internal Dictionary<string, ModelItem> ModelPropertyStore
        {
            get
            {
                return modelPropertyStore;
            }
        }

        protected ModelProperty NameProperty
        {
            get
            {
                if (this.nameProperty == null)
                {
                    Fx.Assert(this.instance != null, "instance cannot be null");
                    RuntimeNamePropertyAttribute runtimeNamePropertyAttribute = TypeDescriptor.GetAttributes(this.instance)[typeof(RuntimeNamePropertyAttribute)] as RuntimeNamePropertyAttribute;
                    if (runtimeNamePropertyAttribute != null && !String.IsNullOrEmpty(runtimeNamePropertyAttribute.Name))
                    {
                        this.nameProperty = this.Properties.Find(runtimeNamePropertyAttribute.Name);
                    }
                }
                return nameProperty;
            }
        }

        ModelItem IModelTreeItem.ModelItem
        {
            get
            {
                return this;
            }
        }

        Dictionary<string, ModelItem> IModelTreeItem.ModelPropertyStore
        {
            get
            {
                return modelPropertyStore;
            }
        }


        ModelTreeManager IModelTreeItem.ModelTreeManager
        {
            get
            {
                return modelTreeManager;
            }
        }

        public override ModelItem this[int index]
        {
            get
            {
                return this.modelItems[index];
            }
            set
            {
                this.Insert(index, value);
            }
        }

        public void SetCurrentView(DependencyObject view)
        {
            this.view = view;
        }

        public override ModelItem Add(object value)
        {
            ModelItem item = value as ModelItem;
            if (item == null)
            {
                item = this.modelTreeManager.WrapAsModelItem(value);
            }
            if (item == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("value"));
            }
            Add(item);
            return item;
        }

        public override void Add(ModelItem item)
        {
            if (item == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
            }
            this.modelTreeManager.CollectionAdd(this, item);
        }


        public override ModelEditingScope BeginEdit(string description, bool shouldApplyChangesImmediately)
        {
            return ModelItemHelper.ModelItemBeginEdit(this.modelTreeManager, description, shouldApplyChangesImmediately);
        }

        public override ModelEditingScope BeginEdit(bool shouldApplyChangesImmediately)
        {
            return this.BeginEdit(null, shouldApplyChangesImmediately);
        }

        public override ModelEditingScope BeginEdit(string description)
        {
            return this.BeginEdit(description, false);
        }

        public override ModelEditingScope BeginEdit()
        {
            return this.BeginEdit(null);
        }

        public override void Clear()
        {
            if (!this.IsReadOnly && (this.modelItems.Count > 0))
            {
                this.modelTreeManager.CollectionClear(this);
            }
        }

        public override bool Contains(object value)
        {
            ModelItem item = value as ModelItem;
            if (item == null)
            {
                return ((IList)instance).Contains(value);
            }
            return Contains(item);
        }

        public override bool Contains(ModelItem item)
        {
            return this.Items.Contains(item);
        }

        public override void CopyTo(ModelItem[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("array"));
            }
            if (this.Items != null)
            {
                this.Items.CopyTo(array, arrayIndex);
            }
        }

        public override object GetCurrentValue()
        {
            return this.instance;
        }

        public override IEnumerator<ModelItem> GetEnumerator()
        {
            foreach (ModelItem modelItem in modelItems)
            {
                yield return modelItem;
            }
        }

        void IModelTreeItem.OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        IEnumerable<ModelItem> IModelTreeItem.ItemBackPointers
        {
            get { return this.parents; }
        }

        List<BackPointer> IModelTreeItem.ExtraPropertyBackPointers
        {
            get { return this.helper.ExtraPropertyBackPointers; }
        }

        void IModelTreeItem.SetParent(ModelItem dataModelItem)
        {
            if (this.manuallySetParent == dataModelItem)
            {
                this.manuallySetParent = null;
            }

            if (!this.parents.Contains(dataModelItem))
            {
                this.parents.Add(dataModelItem);
            }
        }

        void IModelTreeItem.SetSource(ModelProperty property)
        {
            if (!this.sources.Contains(property))
            {
                // also check if the same parent.property is in the list as a different instance of oldModelProperty
                ModelProperty foundProperty = this.sources.Find((modelProperty) => modelProperty.Name.Equals(property.Name) && property.Parent == modelProperty.Parent);
                if (foundProperty == null)
                {
                    this.sources.Add(property);
                }
            }
        }

        public override int IndexOf(ModelItem item)
        {
            if (item == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
            }
            return this.modelItems.IndexOf(item);
        }

        public override ModelItem Insert(int index, object value)
        {
            ModelItem item = value as ModelItem;
            if (item == null)
            {
                item = this.modelTreeManager.WrapAsModelItem(value);
            }
            if (item == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("value"));
            }
            Insert(index, item);
            return item;
        }

        public override void Insert(int index, ModelItem item)
        {
            if (item == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
            }
            this.modelTreeManager.CollectionInsert(this, index, item);
        }

        public override void Move(int fromIndex, int toIndex)
        {
            if (fromIndex > this.Items.Count)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("fromIndex"));
            }
            if (toIndex > this.Items.Count)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("toIndex"));
            }
            ModelItem movingItem = this.Items[fromIndex];
            this.RemoveAt(fromIndex);
            this.Insert(toIndex, movingItem);
        }

        public override bool Remove(object value)
        {
            ModelItem item = value as ModelItem;
            if (item == null)
            {
                Fx.Assert(this.Items != null, "Items collection is null when trying to iterate over it");
                foreach (ModelItem childItem in this.Items)
                {
                    if (childItem.GetCurrentValue() == value)
                    {
                        item = childItem;
                        break;
                    }
                }
                if (item == null)
                {
                    return false;
                }
            }
            return Remove(item);
        }

        public override bool Remove(ModelItem item)
        {
            this.modelTreeManager.CollectionRemove(this, item);
            return true;
        }

        public override void RemoveAt(int index)
        {
            if (index >= this.Count)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("index"));
            }
            this.modelTreeManager.CollectionRemoveAt(this, index);
        }



        internal void AddCore(ModelItem item)
        {
            Fx.Assert(instance is IList, "instance should be IList");
            Fx.Assert(instance != null, "instance should not be null");

            IList instanceList = (IList)instance;
            instanceList.Add(item.GetCurrentValue());
            bool wasInCollection = this.modelItems.Contains(item);
            this.modelItems.Add(item);
            if (!wasInCollection)
            {
                this.modelTreeManager.OnItemEdgeAdded(this, item);
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, item, this.Count - 1));
            }
        }



        internal void ClearCore()
        {
            Fx.Assert(instance is IList, " Instance needs to be Ilist for clear to work");
            Fx.Assert(instance != null, "Instance should not be null");

            IList instanceList = (IList)instance;
            instanceList.Clear();

            List<ModelItem> modelItemsRemoved = new List<ModelItem>(this.modelItems);
            this.modelItems.Clear();

            foreach (ModelItem item in modelItemsRemoved.Distinct())
            {
                if (item != null)
                {
                    this.modelTreeManager.OnItemEdgeRemoved(this, item);
                }
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }
        }


        internal void InsertCore(int index, ModelItem item)
        {
            Fx.Assert(instance is IList, "instance needs to be IList");
            Fx.Assert(instance != null, "instance should not be null");
            IList instanceList = (IList)instance;
            instanceList.Insert(index, item.GetCurrentValue());

            bool wasInCollection = this.modelItems.Contains(item);
            
            this.modelItems.Insert(index, item);

            if (!wasInCollection)
            {
                this.modelTreeManager.OnItemEdgeAdded(this, item);
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, item, index));
            }
        }

        internal void RemoveCore(ModelItem item)
        {

            Fx.Assert(instance is IList, "Instance needs to be IList for remove to work");
            Fx.Assert(instance != null, "instance should not be null");

            IList instanceList = (IList)instance;
            int index = instanceList.IndexOf(item.GetCurrentValue());
            instanceList.Remove(item.GetCurrentValue());
            this.modelItems.Remove(item);

            if (!this.modelItems.Contains(item))
            {
                this.modelTreeManager.OnItemEdgeRemoved(this, item);
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }

            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Remove, item, index));
            }
        }

        internal void RemoveAtCore(int index)
        {
            Fx.Assert(instance is IList, "Instance needs to be IList for remove to work");
            Fx.Assert(instance != null, "instance should not be null");

            IList instanceList = (IList)instance;
            ModelItem item = this.modelItems[index];
            instanceList.RemoveAt(index);
            this.modelItems.RemoveAt(index);
            if (!this.modelItems.Contains(item))
            {
                this.modelTreeManager.OnItemEdgeRemoved(this, item);
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs("Count"));
            }

            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Remove, item, index));
            }
        }

        void UpdateInstance(object instance)
        {
            this.instance = instance;
            IEnumerable instanceCollection = this.instance as IEnumerable;
            if (instanceCollection != null)
            {
                foreach (object item in instanceCollection)
                {
                    ModelItem modelItem = this.modelTreeManager.WrapAsModelItem(item);
                    bool wasInCollection = item != null && this.modelItems.Contains(item);
                    this.modelItems.Add(modelItem);
                    if (item != null && !wasInCollection)
                    {
                        this.modelTreeManager.OnItemEdgeAdded(this, modelItem);
                    }
                }
            }
        }

        void IModelTreeItem.RemoveParent(ModelItem oldParent)
        {
            if (this.manuallySetParent == oldParent)
            {
                this.manuallySetParent = null;
            }

            if (this.parents.Contains(oldParent))
            {
                this.parents.Remove(oldParent);
            }
        }

        void IModelTreeItem.RemoveSource(ModelProperty oldModelProperty)
        {
            if (this.sources.Contains(oldModelProperty))
            {
                this.sources.Remove(oldModelProperty);
            }
            else
            {
                ((IModelTreeItem)this).RemoveSource(oldModelProperty.Parent, oldModelProperty.Name);
            }
        }

        void IModelTreeItem.RemoveSource(ModelItem parent, string propertyName)
        {
            // also check if the same parent.property is in the list as a different instance of oldModelProperty
            ModelProperty foundProperty = this.sources.FirstOrDefault<ModelProperty>((modelProperty) => modelProperty.Name.Equals(propertyName) && modelProperty.Parent == parent);
            if (foundProperty != null)
            {
                this.sources.Remove(foundProperty);
            }
            else
            {
                this.helper.RemoveExtraPropertyBackPointer(parent, propertyName);
            }
        }
    }
}
