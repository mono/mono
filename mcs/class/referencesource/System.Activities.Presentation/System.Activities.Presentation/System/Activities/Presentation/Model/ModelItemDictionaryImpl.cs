//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Model
{
    using System.Activities.Presentation.Services;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Windows;
    using System.Windows.Markup;

    internal class ModelItemDictionaryImpl : ModelItemDictionary, IModelTreeItem, ICustomTypeDescriptor
    {
        ModelProperty contentProperty;
        DictionaryWrapper instance;
        Type itemType;
        private NullableKeyDictionary<ModelItem, ModelItem> modelItems;
        internal ModelItem updateKeySavedValue;
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

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "This is internal code with no derived class")]
        public ModelItemDictionaryImpl(ModelTreeManager modelTreeManager, Type itemType, Object instance, ModelItem parent)
        {
            Fx.Assert(modelTreeManager != null, "modelTreeManager cannot be null");
            Fx.Assert(itemType != null, "item type cannot be null");
            Fx.Assert(instance != null, "instance cannot be null");
            this.itemType = itemType;
            this.instance = new DictionaryWrapper(instance);
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
            this.modelItems = new NullableKeyDictionary<ModelItem, ModelItem>();
            UpdateInstance();


            if (ItemsCollectionObject != null)
            {
                ItemsCollectionModelItemCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(itemsCollection_CollectionChanged);
                this.ItemsCollectionObject.ModelDictionary = this;
            }
        }

        Type itemsCollectionKVPType = null;
        Type ItemsCollectionKVPType
        {
            get
            {
                if (itemsCollectionKVPType == null)
                {
                    if (ItemsCollectionModelItemCollection != null)
                    {
                        Type itemsCollectionType = ItemsCollectionModelItemCollection.ItemType;
                        Type[] genericArguments = itemsCollectionType.GetGenericArguments();
                        this.itemsCollectionKVPType = typeof(ModelItemKeyValuePair<,>).MakeGenericType(genericArguments);
                    }
                }
                return itemsCollectionKVPType;
            }
        }

        ModelItemCollection ItemsCollectionModelItemCollection
        {
            get
            {
                return this.Properties["ItemsCollection"].Collection;
            }
        }

        IItemsCollection ItemsCollectionObject
        {
            get
            {
                IItemsCollection itemsCollectionObject = null;
                if (this.ItemsCollectionModelItemCollection != null)
                {
                    itemsCollectionObject = ItemsCollectionModelItemCollection.GetCurrentValue() as IItemsCollection;
                }
                return itemsCollectionObject;
            }
        }

        private bool EditInProgress { get; set; }

        internal void UpdateValue(object keyObj, object valueObj)
        {
            ModelItem key = null;
            bool keyFound = this.KeyAsModelItem(keyObj, false, out key);
            Fx.Assert(keyFound, "The key should already exist in the current dictionary");
            ModelItem value = this.WrapObject(valueObj);
            this.EditCore(key, value, false);
        }

        internal void UpdateKey(object oldKeyObj, object newKeyObj)
        {
            if (oldKeyObj != newKeyObj)
            {
                ModelItem newKey = null;
                this.KeyAsModelItem(newKeyObj, true, out newKey);
                ModelItem oldKey = null;
                bool oldKeyFound = this.KeyAsModelItem(oldKeyObj, false, out oldKey);
                Fx.Assert(oldKeyFound, "The old key should already exist in the current dictionary");
                
                try
                {
                    this.EditInProgress = true;
                    Fx.Assert(this.instance != null, "instance should not be null");

                    bool wasNewKeyInKeysOrValuesCollection = newKey != null && this.IsInKeysOrValuesCollection(newKey);

                    ModelItem value = this.modelItems[oldKey];
                    this.modelItems.Remove(oldKey);

                    this.updateKeySavedValue = value;

                    if (oldKey != null && !this.IsInKeysOrValuesCollection(oldKey))
                    {
                        this.modelTreeManager.OnItemEdgeRemoved(this, oldKey);
                    }

                    this.updateKeySavedValue = null;
                    this.modelItems[newKey] = value;

                    if (newKey != null && !wasNewKeyInKeysOrValuesCollection)
                    {
                        this.modelTreeManager.OnItemEdgeAdded(this, newKey);
                    }

                    if (null != this.CollectionChanged)
                    {
                        this.CollectionChanged(this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                            new KeyValuePair<ModelItem, ModelItem>(newKey, value),
                            new KeyValuePair<ModelItem, ModelItem>(oldKey, value)));
                    }
                }
                finally
                {
                    this.EditInProgress = false;
                }
            }
        }

        void itemsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //If we're in editing, then we don't trigger an update
            if (EditInProgress)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ModelItem item in e.NewItems)
                {
                    ModelItem key = item.Properties["Key"] == null ? null : item.Properties["Key"].Value;
                    ModelItem value = item.Properties["Value"] == null ? null : item.Properties["Value"].Value;
                    this.AddCore(key, value, false);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ModelItem item in e.OldItems)
                {
                    object keyObject = item.Properties["Key"].Value == null ? null : item.Properties["Key"].Value.GetCurrentValue();
                    ModelItem key = null;
                    bool keyFound = KeyAsModelItem(keyObject, false, out key);
                    Fx.Assert(keyFound, "Key should exist in the current dictionary");
                    this.RemoveCore(key, false);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                Fx.Assert(e.NewItems != null && e.OldItems != null && e.NewItems.Count == e.OldItems.Count,
                    "there must be equal number of old and new items");

                foreach (ModelItem item in e.NewItems)
                {
                    object keyObject = item.Properties["Key"].Value == null ? null : item.Properties["Key"].Value.GetCurrentValue();
                    ModelItem key = null;
                    bool keyFound = KeyAsModelItem(keyObject, false, out key);
                    Fx.Assert(keyFound, "Key should exist in the current dictionary");
                    ModelItem value = item.Properties["Value"] == null ? null : item.Properties["Value"].Value;
                    this.EditCore(key, value, false);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UpdateInstance();
                if (this.CollectionChanged != null)
                {
                    this.CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
            //note we do not handle NotifyCollectionChangedAction.Move as we don't expect it nor can Dictionary do a move operation
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public override event PropertyChangedEventHandler PropertyChanged;

        public override int Count
        {
            get { return this.instance.Count; }
        }

        public override bool IsReadOnly
        {
            get { return this.instance.IsReadOnly; }
        }

        public override ICollection<ModelItem> Keys
        {
            get { return this.modelItems.Keys; }
        }

        public override ICollection<ModelItem> Values
        {
            get { return this.modelItems.Values; }
        }

        public override AttributeCollection Attributes
        {
            get
            {
                Fx.Assert(null != this.itemType, "ItemType cannot be null!");
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
                    ContentPropertyAttribute contentAttribute = TypeDescriptor.GetAttributes(this.instance.Value)[typeof(ContentPropertyAttribute)] as ContentPropertyAttribute;
                    if (contentAttribute != null && !String.IsNullOrEmpty(contentAttribute.Name))
                    {
                        this.contentProperty = this.Properties.Find(contentAttribute.Name);
                    }
                }
                return contentProperty;
            }
        }

        public override Type ItemType
        {
            get { return this.itemType; }
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
                if (null != this.NameProperty)
                {
                    this.NameProperty.SetValue(value);
                }
            }
        }

        public override ModelItem Parent
        {
            get
            {
                return (this.Parents.Count() > 0) ? this.Parents.First() : null;
            }

        }

        public override ModelItem Root
        {
            get { return this.modelTreeManager.Root; }
        }

        public override ModelPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new ModelPropertyCollectionImpl(this);
                }
                return this.properties;
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
            get { return this.view; }
        }

        public ModelItem ModelItem
        {
            get { return this; }
        }

        public Dictionary<string, ModelItem> ModelPropertyStore
        {
            get { return this.modelPropertyStore; }
        }

        public ModelTreeManager ModelTreeManager
        {
            get { return this.modelTreeManager; }
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

        protected ModelProperty NameProperty
        {
            get
            {
                if (this.nameProperty == null)
                {
                    Fx.Assert(this.instance != null, "instance cannot be null");
                    RuntimeNamePropertyAttribute runtimeNamePropertyAttribute = TypeDescriptor.GetAttributes(this.instance.Value)[typeof(RuntimeNamePropertyAttribute)] as RuntimeNamePropertyAttribute;
                    if (runtimeNamePropertyAttribute != null && !String.IsNullOrEmpty(runtimeNamePropertyAttribute.Name))
                    {
                        this.nameProperty = this.Properties.Find(runtimeNamePropertyAttribute.Name);
                    }
                    else
                    {
                        this.nameProperty = this.Properties.FirstOrDefault<ModelProperty>(p => (0 == string.Compare(p.Name, "Name", StringComparison.OrdinalIgnoreCase)));
                    }
                }
                return nameProperty;
            }
        }

        public override ModelItem this[ModelItem key]
        {
            get
            {
                return this.modelItems[key];
            }
            set
            {
                if (this.instance.IsReadOnly)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CollectionIsReadOnly));
                }

                ModelItem oldValue = null;
                if (this.modelItems.TryGetValue(key, out oldValue))
                {
                    this.modelTreeManager.DictionaryEdit(this, key, value, oldValue);
                }
                else
                {
                    this.modelTreeManager.DictionaryAdd(this, key, value);
                }
            }
        }

        public override ModelItem this[object key]
        {
            get
            {
                ModelItem keyItem = null;
                bool keyFound = this.KeyAsModelItem(key, false, out keyItem);
                if (!keyFound)
                {
                    throw FxTrace.Exception.AsError(new KeyNotFoundException(key.ToString()));
                }

                return this[keyItem];
            }
            set
            {
                if (this.instance.IsReadOnly)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CollectionIsReadOnly));
                }

                ModelItem keyItem = null;
                this.KeyAsModelItem(key, true, out keyItem);
                this[keyItem] = value;
            }
        }

        public override void Add(ModelItem key, ModelItem value)
        {
            if (this.instance.IsReadOnly)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CollectionIsReadOnly));
            }
            this.modelTreeManager.DictionaryAdd(this, key, value);
        }

        public override ModelItem Add(object key, object value)
        {
            if (this.instance.IsReadOnly)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CollectionIsReadOnly));
            }
            ModelItem keyModelItem = key as ModelItem ?? this.WrapObject(key);
            ModelItem valueModelItem = value as ModelItem ?? this.WrapObject(value);
            this.Add(keyModelItem, valueModelItem);
            return valueModelItem;
        }

        public override void Clear()
        {
            this.modelTreeManager.DictionaryClear(this);
        }

        public override bool ContainsKey(ModelItem key)
        {
            return this.modelItems.Keys.Contains<ModelItem>(key);
        }

        public override bool ContainsKey(object key)
        {
            ModelItem keyItem = key as ModelItem;

            if (keyItem != null)
            {
                return this.ContainsKey(keyItem);
            }

            return this.KeyAsModelItem(key, false, out keyItem);
        }

        public override IEnumerator<KeyValuePair<ModelItem, ModelItem>> GetEnumerator()
        {
            return this.modelItems.GetEnumerator();
        }

        public override bool Remove(ModelItem key)
        {
            this.modelTreeManager.DictionaryRemove(this, key);
            return true;
        }

        public override bool Remove(object key)
        {
            ModelItem keyItem = null;
            if (!this.KeyAsModelItem(key, false, out keyItem))
            {
                return false;
            }

            return this.Remove(keyItem);
        }

        public override bool TryGetValue(ModelItem key, out ModelItem value)
        {
            return this.modelItems.TryGetValue(key, out value);
        }

        public override bool TryGetValue(object key, out ModelItem value)
        {
            ModelItem keyItem = null;
            if (!this.KeyAsModelItem(key, false, out keyItem))
            {
                value = null;
                return false;
            }

            return this.TryGetValue(keyItem, out value);
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

        public override object GetCurrentValue()
        {
            return this.instance.Value;
        }

        #region IModelTreeItem Members

        public void OnPropertyChanged(string propertyName)
        {
            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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

        public void SetCurrentView(DependencyObject view)
        {
            this.view = view;
        }

        #endregion

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

        IEnumerable<ModelItem> IModelTreeItem.ItemBackPointers
        {
            get { return this.parents; }
        }

        List<BackPointer> IModelTreeItem.ExtraPropertyBackPointers
        {
            get { return this.helper.ExtraPropertyBackPointers; }
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

        internal void EditCore(ModelItem key, ModelItem value)
        {
            this.EditCore(key, value, true);
        }

        private void EditCore(ModelItem key, ModelItem value, bool updateInstance)
        {
            try
            {
                ModelItem oldValue = this.modelItems[key];
                this.EditInProgress = true;
                Fx.Assert(this.instance != null, "instance should not be null");

                bool wasValueInKeysOrValuesCollection = this.IsInKeysOrValuesCollection(value);

                if (updateInstance)
                {
                    this.instance[(key == null) ? null : key.GetCurrentValue()] = null != value ? value.GetCurrentValue() : null;
                    //this also makes sure ItemsCollectionModelItemCollection is not null 
                    if (ItemsCollectionObject != null)
                    {
                        try
                        {
                            ItemsCollectionObject.ShouldUpdateDictionary = false;

                            foreach (ModelItem item in ItemsCollectionModelItemCollection)
                            {
                                ModelItem keyInCollection = item.Properties["Key"].Value;
                                bool found = (key == keyInCollection);

                                if (!found && key != null && keyInCollection != null)
                                {
                                    object keyValue = key.GetCurrentValue();

                                    // ValueType do not share ModelItem, a ModelItem is always created for a ValueType
                                    // ModelTreeManager always create a ModelItem even for the same string
                                    // So, we compare object instance instead of ModelItem for above cases.
                                    if (keyValue is ValueType || keyValue is string)
                                    {
                                        found = keyValue.Equals(keyInCollection.GetCurrentValue());
                                    }
                                }

                                if (found)
                                {
                                    ModelPropertyImpl valueImpl = item.Properties["Value"] as ModelPropertyImpl;
                                    if (valueImpl != null)
                                    {
                                        valueImpl.SetValueCore(value);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            ItemsCollectionObject.ShouldUpdateDictionary = true;
                        }
                    }
                }

                this.modelItems[key] = null;
                if (oldValue != null && !this.IsInKeysOrValuesCollection(oldValue))
                {
                    this.modelTreeManager.OnItemEdgeRemoved(this, oldValue);
                }

                this.modelItems[key] = value;
                if (value != null && !wasValueInKeysOrValuesCollection)
                {
                    this.modelTreeManager.OnItemEdgeAdded(this, value);
                }

                if (null != this.CollectionChanged)
                {
                    this.CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                        new KeyValuePair<ModelItem, ModelItem>(key, value),
                        new KeyValuePair<ModelItem, ModelItem>(key, oldValue)));
                }
            }
            finally
            {
                this.EditInProgress = false;
            }
        }

        internal void AddCore(ModelItem key, ModelItem value)
        {
            this.AddCore(key, value, true);
        }

        private void AddCore(ModelItem key, ModelItem value, bool updateInstance)
        {
            try
            {
                this.EditInProgress = true;
                Fx.Assert(this.instance != null, "instance should not be null");
                
                bool wasKeyInKeysOrValuesCollection = key != null && this.IsInKeysOrValuesCollection(key);
                bool wasValueInKeysOrValuesCollection = value != null && this.IsInKeysOrValuesCollection(value);

                if (updateInstance)
                {
                    //no need to [....] if the ItemsCollection is not DictionaryItemsCollection wrapped by ModelItemCollectionImpl
                    ModelItemCollectionImpl itemsCollectionImpl = this.ItemsCollectionModelItemCollection as ModelItemCollectionImpl;
                    if (ItemsCollectionObject != null && itemsCollectionImpl != null)
                    {
                        try
                        {
                            ItemsCollectionObject.ShouldUpdateDictionary = false;
                            object mutableKVPair = Activator.CreateInstance(this.ItemsCollectionKVPType, new object[] { key == null ? null : key.GetCurrentValue(), value != null ? value.GetCurrentValue() : null });
                            ModelItem mutableKVPairItem = this.modelTreeManager.WrapAsModelItem(mutableKVPair);

                            itemsCollectionImpl.AddCore(mutableKVPairItem);
                        }
                        finally
                        {
                            ItemsCollectionObject.ShouldUpdateDictionary = true;
                        }
                    }
                    this.instance.Add(key == null ? null : key.GetCurrentValue(), null != value ? value.GetCurrentValue() : null);
                }

                this.modelItems.Add(key, value);

                if (key != null && !wasKeyInKeysOrValuesCollection)
                {
                    this.modelTreeManager.OnItemEdgeAdded(this, key);
                }

                if (value != null && !wasValueInKeysOrValuesCollection && value != key)
                {
                    this.modelTreeManager.OnItemEdgeAdded(this, value);
                }

                if (null != this.CollectionChanged)
                {
                    this.CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                        new KeyValuePair<ModelItem, ModelItem>(key, value)));
                }
            }
            finally
            {
                this.EditInProgress = false;
            }
        }

        internal void ClearCore()
        {
            this.ClearCore(true);
        }

        private void ClearCore(bool updateInstance)
        {
            try
            {
                this.EditInProgress = true;
                Fx.Assert(this.instance != null, "instance should not be null");
                IList removed = this.modelItems.ToList<KeyValuePair<ModelItem, ModelItem>>();
                if (updateInstance)
                {
                    //no need to [....] if the ItemsCollection is not DictionaryItemsCollection wrapped by ModelItemCollectionImpl
                    ModelItemCollectionImpl itemsCollectionImpl = this.ItemsCollectionModelItemCollection as ModelItemCollectionImpl;
                    if (ItemsCollectionObject != null && itemsCollectionImpl != null)
                    {
                        try
                        {
                            ItemsCollectionObject.ShouldUpdateDictionary = false;
                            itemsCollectionImpl.ClearCore();
                        }
                        finally
                        {
                            ItemsCollectionObject.ShouldUpdateDictionary = true;
                        }
                    }
                    this.instance.Clear();
                }
                List<ModelItem> removedItems = new List<ModelItem>(this.modelItems.Keys.Concat(this.modelItems.Values).Distinct());
                this.modelItems.Clear();

                foreach (ModelItem item in removedItems.Distinct())
                {
                    if (item != null)
                    {
                        this.modelTreeManager.OnItemEdgeRemoved(this, item);
                    }
                }

                if (null != this.CollectionChanged)
                {
                    this.CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
                }
            }
            finally
            {
                this.EditInProgress = false;
            }
        }

        internal void RemoveCore(ModelItem key)
        {
            this.RemoveCore(key, true);
        }

        private bool IsInKeysOrValuesCollection(ModelItem modelItem)
        {
            foreach (ModelItem item in this.modelItems.Keys)
            {
                if (item == modelItem)
                {
                    return true;
                }
            }

            foreach (ModelItem item in this.modelItems.Values)
            {
                if (item == modelItem)
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveCore(ModelItem key, bool updateInstance)
        {
            try
            {
                this.EditInProgress = true;
                Fx.Assert(this.instance != null, "instance should not be null");
                ModelItem value = this.modelItems[key];
                this.modelItems.Remove(key);

                if (key != null && !this.IsInKeysOrValuesCollection(key))
                {
                    this.modelTreeManager.OnItemEdgeRemoved(this, key);
                }

                if (value != null && !this.IsInKeysOrValuesCollection(value) && value != key)
                {
                    this.modelTreeManager.OnItemEdgeRemoved(this, value);
                }

                if (updateInstance)
                {
                    ModelItemCollectionImpl itemsCollectionImpl = ItemsCollectionModelItemCollection as ModelItemCollectionImpl;
                    if (ItemsCollectionObject != null && itemsCollectionImpl != null)
                    {
                        try
                        {
                            ItemsCollectionObject.ShouldUpdateDictionary = false;

                            ModelItem itemToBeRemoved = null;
                            foreach (ModelItem item in itemsCollectionImpl)
                            {
                                ModelItem keyInCollection = item.Properties["Key"].Value;

                                if (key == keyInCollection)
                                {
                                    itemToBeRemoved = item;
                                    break;
                                }

                                if (key != null && keyInCollection != null)
                                {
                                    object keyValue = key.GetCurrentValue();

                                    // ValueType do not share ModelItem, a ModelItem is always created for a ValueType
                                    // ModelTreeManager always create a ModelItem even for the same string
                                    // So, we compare object instance instead of ModelItem for above cases.
                                    if (keyValue is ValueType || keyValue is string)
                                    {
                                        if (keyValue.Equals(keyInCollection.GetCurrentValue()))
                                        {
                                            itemToBeRemoved = item;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (itemToBeRemoved != null)
                            {
                                itemsCollectionImpl.RemoveCore(itemToBeRemoved);
                            }
                        }
                        finally
                        {
                            ItemsCollectionObject.ShouldUpdateDictionary = true;
                        }
                    }
                    this.instance.Remove(key == null ? null : key.GetCurrentValue());
                }
                if (null != this.CollectionChanged)
                {
                    this.CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<ModelItem, ModelItem>(key, value)));
                }
            }
            finally
            {
                this.EditInProgress = false;
            }
        }

        void UpdateInstance()
        {
            IEnumerator dictionaryEnumerator = this.instance.GetEnumerator();
            while (dictionaryEnumerator.MoveNext())
            {
                object current = dictionaryEnumerator.Current;

                object keyObject = instance.GetKeyFromCurrent(current);
                ModelItem key = (keyObject == null) ? null : this.WrapObject(keyObject);
                ModelItem value = this.WrapObject(instance.GetValueFromCurrent(current));

                bool wasKeyInKeysOrValuesCollection = key != null && this.IsInKeysOrValuesCollection(key);
                bool wasValueInKeysOrValuesCollection = value != null && this.IsInKeysOrValuesCollection(value);

                this.modelItems.Add(key, value);

                if (key != null && !wasKeyInKeysOrValuesCollection)
                {
                    this.modelTreeManager.OnItemEdgeAdded(this, key);
                }

                if (value != null && !wasValueInKeysOrValuesCollection && value != key)
                {
                    this.modelTreeManager.OnItemEdgeAdded(this, value);
                }
            }
        }

        ModelItem WrapObject(object value)
        {
            return this.ModelTreeManager.WrapAsModelItem(value);
        }

        // return true if the key already exist, false otherwise.
        private bool KeyAsModelItem(object value, bool createNew, out ModelItem keyModelItem)
        {
            keyModelItem = value as ModelItem;
            if (keyModelItem != null)
            {
                return true;
            }

            bool found = false;
            keyModelItem = this.modelItems.Keys.SingleOrDefault<ModelItem>(p =>
            {
                if ((p == null && value == null) || (p != null && object.Equals(p.GetCurrentValue(), value)))
                {
                    found = true;
                    return true;
                }
                return false;
            });

            if (createNew && keyModelItem == null)
            {
                keyModelItem = WrapObject(value);
            }

            return found;
        }

        sealed class DictionaryWrapper
        {
            object instance;
            bool isDictionary = false;
            PropertyInfo isReadOnlyProperty;
            PropertyInfo countProperty;
            PropertyInfo indexingProperty;
            MethodInfo addMethod;
            MethodInfo removeMethod;
            MethodInfo clearMethod;
            MethodInfo getEnumeratorMethod;
            PropertyInfo keyProperty;
            PropertyInfo valueProperty;

            public DictionaryWrapper(object instance)
            {
                this.instance = instance;
                if (instance is IDictionary)
                {
                    this.isDictionary = true;
                    Type keyValuePairType = typeof(KeyValuePair<object, object>);
                }
                else
                {
                    Type instanceType = instance.GetType();
                    instanceType.FindInterfaces(this.GetDictionaryInterface, null);
                }
            }

            public object Value
            {
                get { return this.instance; }
            }

            public bool IsReadOnly
            {
                get
                {
                    return (this.isDictionary ? ((IDictionary)instance).IsReadOnly : (bool)this.isReadOnlyProperty.GetValue(this.instance, null));
                }
            }

            public int Count
            {
                get
                {
                    return (this.isDictionary ? ((IDictionary)instance).Count : (int)this.countProperty.GetValue(this.instance, null));
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is intended for use through reflection")]
            public object this[object key]
            {
                get
                {
                    if (this.isDictionary)
                    {
                        return ((IDictionary)instance)[key];
                    }
                    else
                    {
                        return this.indexingProperty.GetValue(this.instance, new object[] { key });
                    }
                }
                set
                {
                    if (this.isDictionary)
                    {
                        ((IDictionary)instance)[key] = value;
                    }
                    else
                    {
                        this.indexingProperty.SetValue(this.instance, value, new object[] { key });
                    }
                }
            }

            public object GetKeyFromCurrent(object keyValuePair)
            {
                if (isDictionary)
                {
                    return ((DictionaryEntry)keyValuePair).Key;
                }
                else
                {
                    return this.keyProperty.GetValue(keyValuePair, null);
                }
            }

            public object GetValueFromCurrent(object keyValuePair)
            {
                if (isDictionary)
                {
                    return ((DictionaryEntry)keyValuePair).Value;
                }
                else
                {
                    return this.valueProperty.GetValue(keyValuePair, null);
                }
            }


            bool GetDictionaryInterface(Type type, object dummy)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    this.addMethod = type.GetMethod("Add");
                    this.removeMethod = type.GetMethod("Remove");
                    this.indexingProperty = type.GetProperty("Item");
                    return true;
                }
                if (type.IsGenericType &&
                    type.GetGenericArguments()[0].IsGenericType &&
                    type.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(KeyValuePair<,>) &&
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    Type keyValuePairType = type.GetGenericArguments()[0];
                    this.keyProperty = keyValuePairType.GetProperty("Key");
                    this.valueProperty = keyValuePairType.GetProperty("Value");
                    this.getEnumeratorMethod = type.GetMethod("GetEnumerator");
                    return true;
                }
                if (type.IsGenericType &&
                    type.GetGenericArguments()[0].IsGenericType &&
                    type.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(KeyValuePair<,>) &&
                    type.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    this.isReadOnlyProperty = type.GetProperty("IsReadOnly");
                    this.countProperty = type.GetProperty("Count");
                    this.clearMethod = type.GetMethod("Clear");
                }
                return false;
            }


            public void Add(object key, object value)
            {
                if (this.isDictionary)
                {
                    ((IDictionary)instance).Add(key, value);
                }
                else
                {
                    this.addMethod.Invoke(this.instance, new object[] { key, value });
                }
            }

            public void Clear()
            {
                if (this.isDictionary)
                {
                    ((IDictionary)instance).Clear();
                }
                else
                {
                    this.clearMethod.Invoke(this.instance, null);
                }
            }

            public IEnumerator GetEnumerator()
            {
                if (this.isDictionary)
                {
                    return ((IDictionary)instance).GetEnumerator();
                }
                else
                {
                    return (IEnumerator)this.getEnumeratorMethod.Invoke(this.instance, null);
                }
            }

            public void Remove(object key)
            {
                if (this.isDictionary)
                {
                    ((IDictionary)instance).Remove(key);
                }
                else
                {
                    this.removeMethod.Invoke(this.instance, new object[] { key });
                }
            }


        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return this.Attributes;
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return ModelUtilities.GetConverter(this);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            // we dont support events;
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return ModelUtilities.GetDefaultProperty(this);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            // we dont support editors
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            // we dont support events;
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            // we dont support events;
            return null;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return ModelUtilities.WrapProperties(this);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            // get model properties
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();


            foreach (PropertyDescriptor modelPropertyDescriptor in ModelUtilities.WrapProperties(this))
            {
                properties.Add(modelPropertyDescriptor);
            }

            // try to see if there are pseudo builtin properties for this type.
            AttachedPropertiesService AttachedPropertiesService = this.modelTreeManager.Context.Services.GetService<AttachedPropertiesService>();
            if (AttachedPropertiesService != null)
            {
                foreach (AttachedProperty AttachedProperty in AttachedPropertiesService.GetAttachedProperties(this.itemType))
                {
                    properties.Add(new AttachedPropertyDescriptor(AttachedProperty, this));
                }
            }
            return new PropertyDescriptorCollection(properties.ToArray(), true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }
}
