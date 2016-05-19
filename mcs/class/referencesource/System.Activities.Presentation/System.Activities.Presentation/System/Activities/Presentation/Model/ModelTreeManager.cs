//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Activities.Expressions;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Validation;
    using System.Activities.Presentation.View;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using Microsoft.Activities.Presentation.Xaml;

    // This class manages the model tree, provides the root model item and the modelservice
    // This also provides syncing the model tree with the xaml text
    // The model service is publishes on the editing context passed to the constructor.

    [Fx.Tag.XamlVisible(false)]
    public class ModelTreeManager
    {
        internal ModelServiceImpl modelService;
        EditingContext context;
        // The value of this dictionary is a WeakReference to ModelItem.
        // This need to be a WeakReference because if the ModelItem has a strong reference, it 
        // will have a strong reference to the underlying object instance as well.
        WeakKeyDictionary<object, WeakReference> objectMap;
        ModelItem rootItem;
        ImmediateEditingScope immediateEditingScope;
        Stack<ModelEditingScope> editingScopes;
        int redoUndoInProgressCount = 0;
        FeatureManager featureManager;
        ModelGraphManager graphManager;

        public ModelTreeManager(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }
            this.context = context;
            // We want to check reference equality for keys in ObjectMap. 
            // If the user overrides Equals method for their class we still want to use referential equality.
            objectMap = new WeakKeyDictionary<object, WeakReference>(ObjectReferenceEqualityComparer<object>.Default);
            editingScopes = new Stack<ModelEditingScope>();
            this.graphManager = new ModelGraphManager(this);
        }

        public event EventHandler<EditingScopeEventArgs> EditingScopeCompleted;

        // Private event only for EditingScope.
        private event EventHandler<ModelItemsRemovedEventArgs> ModelItemsRemoved;

        private event EventHandler<ModelItemsAddedEventArgs> ModelItemsAdded;

        public ModelItem Root
        {
            get
            {
                return this.rootItem;
            }
        }

        internal EditingContext Context
        {
            get
            {
                return this.context;
            }
        }

        FeatureManager FeatureManager
        {
            get
            {
                if (this.featureManager == null)
                {
                    this.featureManager = this.context.Services.GetService<FeatureManager>();
                }
                return this.featureManager;
            }
        }

        internal bool RedoUndoInProgress
        {
            get
            {
                return this.redoUndoInProgressCount > 0;
            }
        }

        internal void StartTracking()
        {
            redoUndoInProgressCount--;
        }

        internal void StopTracking()
        {
            redoUndoInProgressCount++;
        }

        public ModelItem CreateModelItem(ModelItem parent, object instance)
        {
            if (instance == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("instance"));
            }
            ModelItem retval;

            Type instanceType = instance.GetType();
            object[] result = new object[2] { false, false };

            Type[] interfaces = instanceType.FindInterfaces(ModelTreeManager.CheckInterface, result);

            bool isList = (bool)result[0];
            bool isDictionary = (bool)result[1];

            if (isDictionary)
            {
                foreach (Type type in interfaces)
                {
                    if (type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    {
                        // To expose one more property, a collection of MutableKeyValuePairs, to the model tree.
                        TypeDescriptor.AddProvider(new DictionaryTypeDescriptionProvider(instanceType), instance);
                        break;
                    }
                }

                ModelItemDictionary modelItem = new ModelItemDictionaryImpl(this, instance.GetType(), instance, parent);
                retval = modelItem;
            }
            else if (isList)
            {
                ModelItemCollectionImpl modelItem = new ModelItemCollectionImpl(this, instance.GetType(), instance, parent);
                retval = modelItem;
            }
            else
            {
                retval = new ModelItemImpl(this, instance.GetType(), instance, parent);
            }
            if (!((instance is ValueType) || (instance is string)))
            {
                //
                // ValueType do not have a concept of shared reference, they are always copied.
                // strings are immutatable, therefore the risk of making all shared string references to different
                // string ModelItems is low.
                // 
                // To special case string is because underlying OM are sharing string objects for DisplayName across
                // Different activity object instances. These shared references is causing memory leak because of bugs.
                // 
                // We will need to fix these issues in Beta2.
                //
                objectMap[instance] = new WeakReference(retval);
            }

            if (this.FeatureManager != null)
            {
                this.FeatureManager.InitializeFeature(instance.GetType());
            }
            return retval;
        }

        static bool CheckInterface(Type type, object result)
        {
            object[] values = (object[])result;
            if (typeof(IList).IsAssignableFrom(type))
            {
                values[0] = true;
                return true;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                values[0] = true;
                return true;
            }
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                values[1] = true;
                return true;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                values[1] = true;
                return true;
            }
            return false;
        }

        public void Load(object rootInstance)
        {
            if (rootInstance == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("rootInstance"));
            }
            objectMap.Clear();

            ModelItem oldRoot = this.rootItem;
            this.rootItem = WrapAsModelItem(rootInstance);
            this.graphManager.OnRootChanged(oldRoot, this.rootItem);
            if (this.modelService == null)
            {
                this.modelService = new ModelServiceImpl(this);
                this.context.Services.Publish<ModelService>(modelService);
            }
        }

        // This methods clears the value of a property , if the property is
        // a reference type then its set to null, if its a value type the 
        // property value is reset to the default. this also clears the sub ModelItem corresponding
        // to the old value from the parent ModelItem's modelPropertyStore.
        internal void ClearValue(ModelPropertyImpl modelProperty)
        {
            Fx.Assert(modelProperty != null, "modelProperty should not be null");
            Fx.Assert(modelProperty.Parent is IModelTreeItem, "modelProperty.Parent should be an IModelTreeItem");
            ModelItem newValueModelItem = null;
            newValueModelItem = WrapAsModelItem(modelProperty.DefaultValue);
            PropertyChange propertyChange = new PropertyChange()
                {
                    Owner = modelProperty.Parent,
                    PropertyName = modelProperty.Name,
                    OldValue = modelProperty.Value,
                    NewValue = newValueModelItem,
                    ModelTreeManager = this
                };
            AddToCurrentEditingScope(propertyChange);
        }

        internal void CollectionAdd(ModelItemCollectionImpl dataModelItemCollection, ModelItem item)
        {
            CollectionInsert(dataModelItemCollection, -1, item);
        }

        internal void CollectionInsert(ModelItemCollectionImpl dataModelItemCollection, int index, ModelItem item)
        {
            Fx.Assert(dataModelItemCollection != null, "collection should not be null");
            CollectionChange change = new CollectionChange()
                {
                    Collection = dataModelItemCollection,
                    Item = item,
                    Index = index,
                    ModelTreeManager = this,
                    Operation = CollectionChange.OperationType.Insert
                };
            AddToCurrentEditingScope(change);
        }

        internal void CollectionClear(ModelItemCollectionImpl modelItemCollectionImpl)
        {
            Fx.Assert(modelItemCollectionImpl != null, "collection should not be null");
            Fx.Assert(this.modelService != null, "modelService should not be null");
            List<ModelItem> removedItems = new List<ModelItem>();
            removedItems.AddRange(modelItemCollectionImpl);
            using (ModelEditingScope editingScope = CreateEditingScope(SR.CollectionClearEditingScopeDescription))
            {
                foreach (ModelItem modelItem in removedItems)
                {
                    this.CollectionRemove(modelItemCollectionImpl, modelItem);
                }
                editingScope.Complete();
            }
            this.modelService.OnModelItemsRemoved(removedItems);
        }

        internal void NotifyCollectionInsert(ModelItem item, ModelChangeInfo changeInfo)
        {
            this.modelService.OnModelItemAdded(item, changeInfo);
        }

        internal void CollectionRemove(ModelItemCollectionImpl dataModelItemCollection, ModelItem item)
        {
            CollectionRemove(dataModelItemCollection, item, -1);
        }

        internal void CollectionRemoveAt(ModelItemCollectionImpl dataModelItemCollection, int index)
        {
            ModelItem item = dataModelItemCollection[index];
            CollectionRemove(dataModelItemCollection, item, index);
        }

        private void CollectionRemove(ModelItemCollectionImpl dataModelItemCollection, ModelItem item, int index)
        {
            Fx.Assert(dataModelItemCollection != null, "collection should not be null");
            CollectionChange change = new CollectionChange()
                {
                    Collection = dataModelItemCollection,
                    Item = item,
                    Index = index,
                    ModelTreeManager = this,
                    Operation = CollectionChange.OperationType.Delete
                };
            AddToCurrentEditingScope(change);
        }

        internal void NotifyCollectionRemove(ModelItem item, ModelChangeInfo changeInfo)
        {
            this.modelService.OnModelItemRemoved(item, changeInfo);
        }

        internal void DictionaryClear(ModelItemDictionaryImpl modelDictionary)
        {
            Fx.Assert(modelDictionary != null, "dictionary should not be null");
            Fx.Assert(this.modelService != null, "modelService should not be null");
            ModelItem[] keys = modelDictionary.Keys.ToArray<ModelItem>();

            using (ModelEditingScope editingScope = CreateEditingScope(SR.DictionaryClearEditingScopeDescription))
            {
                foreach (ModelItem key in keys)
                {
                    this.DictionaryRemove(modelDictionary, key);
                }
                editingScope.Complete();
            }
        }

        internal void DictionaryEdit(ModelItemDictionaryImpl dataModelItemDictionary, ModelItem key, ModelItem newValue, ModelItem oldValue)
        {
            Fx.Assert(dataModelItemDictionary != null, "dictionary should not be null");
            Fx.Assert(this.modelService != null, "modelService should not be null");
            DictionaryEditChange change = new DictionaryEditChange()
                {
                    Dictionary = dataModelItemDictionary,
                    Key = key,
                    NewValue = newValue,
                    OldValue = oldValue,
                    ModelTreeManager = this
                };
            AddToCurrentEditingScope(change);
        }

        internal void DictionaryAdd(ModelItemDictionaryImpl dataModelItemDictionary, ModelItem key, ModelItem value)
        {
            Fx.Assert(dataModelItemDictionary != null, "dictionary should not be null");
            Fx.Assert(this.modelService != null, "modelService should not be null");
            DictionaryChange change = new DictionaryChange()
                {
                    Dictionary = dataModelItemDictionary,
                    Key = key,
                    Value = value,
                    Operation = DictionaryChange.OperationType.Insert,
                    ModelTreeManager = this
                };
            AddToCurrentEditingScope(change);
        }

        internal void OnPropertyEdgeAdded(string propertyName, ModelItem from, ModelItem to)
        {
            this.graphManager.OnPropertyEdgeAdded(propertyName, from, to);
        }

        internal void OnItemEdgeAdded(ModelItem from, ModelItem to)
        {
            this.graphManager.OnItemEdgeAdded(from, to);
        }

        internal void OnPropertyEdgeRemoved(string propertyName, ModelItem from, ModelItem to)
        {
            this.graphManager.OnPropertyEdgeRemoved(propertyName, from, to);
        }

        internal void OnItemEdgeRemoved(ModelItem from, ModelItem to)
        {
            this.graphManager.OnItemEdgeRemoved(from, to);
        }

        internal void DictionaryRemove(ModelItemDictionaryImpl dataModelItemDictionary, ModelItem key)
        {
            Fx.Assert(dataModelItemDictionary != null, "dictionary should not be null");
            Fx.Assert(this.modelService != null, "modelService should not be null");
            ModelItem value = dataModelItemDictionary[key];
            DictionaryChange change = new DictionaryChange()
                {
                    Dictionary = dataModelItemDictionary,
                    Key = key,
                    Value = value,
                    Operation = DictionaryChange.OperationType.Delete,
                    ModelTreeManager = this
                };
            AddToCurrentEditingScope(change);

        }

        internal static IEnumerable<ModelItem> Find(ModelItem startingItem, Predicate<ModelItem> matcher, bool skipCollapsedAndUnrootable)
        {
            Fx.Assert(startingItem != null, "starting item should not be null");
            Fx.Assert(matcher != null, "matching predicate should not be null");
            WorkflowViewService viewService = startingItem.GetEditingContext().Services.GetService<ViewService>() as WorkflowViewService;
            if (skipCollapsedAndUnrootable)
            {
                Fx.Assert(viewService != null, "ViewService must be available in order to skip exploring ModelItems whose views are collapsed.");
            }

            Predicate<ModelItem> shouldSearchThroughProperties = (currentModelItem) => (!skipCollapsedAndUnrootable)
                || (!typeof(WorkflowViewElement).IsAssignableFrom(viewService.GetDesignerType(currentModelItem.ItemType)))
                || (ViewUtilities.IsViewExpanded(currentModelItem, startingItem.GetEditingContext()) && viewService.ShouldAppearOnBreadCrumb(currentModelItem, true));

            List<ModelItem> foundItems = new List<ModelItem>();
            Queue<ModelItem> modelItems = new Queue<ModelItem>();
            modelItems.Enqueue(startingItem);
            HashSet<ModelItem> alreadyVisited = new HashSet<ModelItem>();
            while (modelItems.Count > 0)
            {
                ModelItem currentModelItem = modelItems.Dequeue();
                if (currentModelItem == null)
                {
                    continue;
                }

                if (matcher(currentModelItem))
                {
                    foundItems.Add(currentModelItem);
                }

                List<ModelItem> neighbors = GetNeighbors(currentModelItem, shouldSearchThroughProperties);

                foreach (ModelItem neighbor in neighbors)
                {
                    if (!alreadyVisited.Contains(neighbor))
                    {
                        alreadyVisited.Add(neighbor);
                        modelItems.Enqueue(neighbor);
                    }
                }
            }

            return foundItems;
        }

        private static List<ModelItem> GetNeighbors(ModelItem currentModelItem, Predicate<ModelItem> extraShouldSearchThroughProperties)
        {
            List<ModelItem> neighbors = new List<ModelItem>();

            // do not search through Type and its derivatives
            if (typeof(Type).IsAssignableFrom(currentModelItem.ItemType))
            {
                return neighbors;
            }

            ModelItemCollection collection = currentModelItem as ModelItemCollection;
            if (collection != null)
            {
                foreach (ModelItem modelItem in collection)
                {
                    if (modelItem != null)
                    {
                        neighbors.Add(modelItem);
                    }
                }
            }
            else
            {
                ModelItemDictionary dictionary = currentModelItem as ModelItemDictionary;
                if (dictionary != null)
                {
                    foreach (KeyValuePair<ModelItem, ModelItem> kvp in dictionary)
                    {
                        ModelItem miKey = kvp.Key;
                        if (miKey != null)
                        {
                            neighbors.Add(miKey);
                        }

                        ModelItem miValue = kvp.Value;
                        if (miValue != null)
                        {
                            neighbors.Add(miValue);
                        }
                    }
                }
            }

            if (extraShouldSearchThroughProperties(currentModelItem))
            {
                ModelPropertyCollection modelProperties = currentModelItem.Properties;
                foreach (ModelProperty property in modelProperties)
                {
                    if (currentModelItem is ModelItemDictionary && string.Equals(property.Name, "ItemsCollection"))
                    {
                        // Don't search the item collection since we already search the items above.
                        continue;
                    }

                    // we don't want to even try to get the value for a value type property
                    // because that will create a new ModelItem every time.

                    // System.Type has properties that throw when we try to get value
                    // we don't want to expand system.type further during a search.
                    if (typeof(Type).IsAssignableFrom(property.PropertyType) || property.PropertyType.IsValueType)
                    {
                        continue;
                    }

                    else
                    {
                        if (property.Value != null)
                        {
                            neighbors.Add(property.Value);
                        }
                    }
                }
            }

            return neighbors;
        }

        internal static ModelItem FindFirst(ModelItem startingItem, Predicate<ModelItem> matcher)
        {
            return FindFirst(startingItem, matcher, (m) => true);
        }

        internal static ModelItem FindFirst(ModelItem startingItem, Predicate<ModelItem> matcher, Predicate<ModelItem> extraShouldSearchThroughProperties)
        {
            Fx.Assert(startingItem != null, "starting item should not be null");
            Fx.Assert(matcher != null, "matching predicate should not be null");
            Fx.Assert(extraShouldSearchThroughProperties != null, "extraShouldSearchThroughProperties should not be null");
            ModelItem foundItem = null;
            Queue<ModelItem> modelItems = new Queue<ModelItem>();
            modelItems.Enqueue(startingItem);
            HashSet<ModelItem> alreadyVisited = new HashSet<ModelItem>();
            while (modelItems.Count > 0)
            {
                ModelItem currentModelItem = modelItems.Dequeue();
                if (currentModelItem == null)
                {
                    continue;
                }

                if (matcher(currentModelItem))
                {
                    foundItem = currentModelItem;
                    break;
                }

                List<ModelItem> neighbors = GetNeighbors(currentModelItem, extraShouldSearchThroughProperties);

                foreach (ModelItem neighbor in neighbors)
                {
                    if (!alreadyVisited.Contains(neighbor))
                    {
                        alreadyVisited.Add(neighbor);
                        modelItems.Enqueue(neighbor);
                    }
                }
            }
            return foundItem;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
          Justification = "If the property getter threw here we dont want to crash, we just dont want to wrap that property value")]
        [SuppressMessage("Reliability", "Reliability108:IsFatalRule",
            Justification = "If the property getter threw here we dont want to crash, we just dont want to wrap that property value")]
        internal ModelItem GetValue(ModelPropertyImpl dataModelProperty)
        {
            Fx.Assert(dataModelProperty != null, "modelproperty should not be null");
            Fx.Assert(dataModelProperty.Parent is IModelTreeItem, "modelproperty.Parent should be an IModelTreeItem");
            IModelTreeItem parent = (IModelTreeItem)dataModelProperty.Parent;
            ModelItem value;

            // always reevaluate attached properties. the cache in attached properties case is only to remember the old value.
            if (!dataModelProperty.IsAttached && parent.ModelPropertyStore.ContainsKey(dataModelProperty.Name))
            {
                value = parent.ModelPropertyStore[dataModelProperty.Name];
            }
            // Create a ModelItem on demand for the value of the property.
            else
            {
                try
                {
                    value = WrapAsModelItem(dataModelProperty.PropertyDescriptor.GetValue(parent.ModelItem.GetCurrentValue()));
                }
                catch (System.Exception)
                {
                    // GetValue throws an exception if Value is not available
                    value = null;
                }
                if (value != null)
                {
                    if (!dataModelProperty.IsAttached)
                    {
                        parent.ModelPropertyStore.Add(dataModelProperty.Name, value);
                        this.graphManager.OnPropertyEdgeAdded(dataModelProperty.Name, (ModelItem)parent, value);
                    }
                }
            }
            return value;
        }

        internal ModelItem SetValue(ModelPropertyImpl modelProperty, object value)
        {
            Fx.Assert(modelProperty != null, "modelProperty should not be null");
            ModelItem newValueModelItem = null;

            RefreshPropertiesAttribute refreshPropertiesAttribute = ExtensibilityAccessor.GetAttribute<RefreshPropertiesAttribute>(modelProperty.Attributes);
            if (refreshPropertiesAttribute != null && refreshPropertiesAttribute.RefreshProperties == RefreshProperties.All)
            {
                Dictionary<string, ModelItem> modelPropertyStore = ((IModelTreeItem)modelProperty.Parent).ModelPropertyStore;
                Dictionary<string, ModelItem> removedModelItems = new Dictionary<string, ModelItem>(modelPropertyStore);
                modelPropertyStore.Clear();

                foreach (KeyValuePair<string, ModelItem> kvp in removedModelItems)
                {
                    if (kvp.Value != null)
                    {
                        this.OnPropertyEdgeRemoved(kvp.Key, modelProperty.Parent, kvp.Value);
                    }
                }
            }

            if (value is ModelItem)
            {
                newValueModelItem = (ModelItem)value;
            }
            else
            {
                newValueModelItem = WrapAsModelItem(value);
            }
            // dont do deferred updates for attached properties
            if (modelProperty.IsAttached)
            {
                modelProperty.SetValueCore(newValueModelItem);
            }
            else
            {
                PropertyChange propertyChange = new PropertyChange()
                {
                    Owner = modelProperty.Parent,
                    PropertyName = modelProperty.Name,
                    OldValue = modelProperty.Value,
                    NewValue = newValueModelItem,
                    ModelTreeManager = this
                };
                AddToCurrentEditingScope(propertyChange);
            }


            return newValueModelItem;
        }

        internal void AddToCurrentEditingScope(Change change)
        {
            EditingScope editingScope;
            if (editingScopes.Count > 0)
            {
                editingScope = (EditingScope)editingScopes.Peek();
                // Automatic generated change during apply changes of Redo/Undo should be ignored.
                if (!RedoUndoInProgress)
                {
                    editingScope.Changes.Add(change);
                }
            }
            else
            {
                //edit operation without editingscope create an editing scope and complete it immediately.
                editingScope = CreateEditingScope(change.Description);
                editingScope.Changes.Add(change);
                try
                {
                    editingScope.Complete();
                }
                catch
                {
                    editingScope.Revert();
                    throw;
                }

            }
        }

        internal bool CanCreateImmediateEditingScope()
        {
            return this.editingScopes.Count == 0 && !this.Context.Services.GetService<UndoEngine>().IsBookmarkInPlace;
        }

        internal EditingScope CreateEditingScope(string description, bool shouldApplyChangesImmediately)
        {
            EditingScope editingScope = null;
            EditingScope outerScope = editingScopes.Count > 0 ? (EditingScope)editingScopes.Peek() : null;
            if (shouldApplyChangesImmediately)
            {
                // shouldApplyChangesImmediately won't have any effect if outer scope exists
                if (outerScope != null)
                {
                    return null;
                }
                this.immediateEditingScope = context.Services.GetRequiredService<UndoEngine>().CreateImmediateEditingScope(description, this);
                editingScope = this.immediateEditingScope;
                // ImmediateEditingScope should not be pushed onto the editingScopes,
                //  otherwise it will become delay applied instead of immediate applied.
            }
            else
            {
                editingScope = new EditingScope(this, outerScope);
                editingScopes.Push(editingScope);
            }
            editingScope.Description = description;
            return editingScope;
        }

        internal EditingScope CreateEditingScope(string description)
        {
            return this.CreateEditingScope(description, false);
        }

        internal void NotifyPropertyChange(ModelPropertyImpl dataModelProperty, ModelChangeInfo changeInfo)
        {
            modelService.OnModelPropertyChanged(dataModelProperty, changeInfo);
        }

        internal void SyncModelAndText()
        {
            // Place holder for xaml generation ModelTreeManager now is instance only.
        }

        internal ModelItem WrapAsModelItem(object instance)
        {
            ModelItem modelItem = GetModelItem(instance);

            if (null != instance && null == modelItem)
            {
                modelItem = CreateModelItem(null, instance);
            }
            return modelItem;
        }

        internal ModelItem GetModelItem(object instance)
        {
            return this.GetModelItem(instance, false);
        }

        public ModelItem GetModelItem(object instance, bool shouldExpandModelTree)
        {
            if (instance == null)
            {
                return null;
            }

            ModelItem modelItem = null;
            WeakReference mappedModelItem = null;
            objectMap.TryGetValue(instance, out mappedModelItem);
            if (mappedModelItem != null)
            {
                modelItem = (ModelItem)mappedModelItem.Target;
            }

            if (modelItem == null && shouldExpandModelTree)
            {
                if (instance is ValueType || instance is string)
                {
                    return null;
                }

                modelItem = FindFirst(this.Root, (m) => { return m.GetCurrentValue() == instance; });
            }

            return modelItem;
        }

        internal void RegisterModelTreeChangeEvents(EditingScope editingScope)
        {
            this.ModelItemsAdded += editingScope.EditingScope_ModelItemsAdded;
            this.ModelItemsRemoved += editingScope.EditingScope_ModelItemsRemoved;
        }

        internal void UnregisterModelTreeChangeEvents(EditingScope editingScope)
        {
            this.ModelItemsAdded -= editingScope.EditingScope_ModelItemsAdded;
            this.ModelItemsRemoved -= editingScope.EditingScope_ModelItemsRemoved;
        }

        // The method should be called when an EditingScope completed. But if the EditingScope has an outer EditingScope,
        // Changes are not applied, so itemsAdded and itemsRemoved won't update until the outer EditingScope.Complete()
        internal void OnEditingScopeCompleted(EditingScope modelEditingScopeImpl)
        {
            if (editingScopes.Contains(modelEditingScopeImpl))
            {
                editingScopes.Pop();
            }

            if (editingScopes.Count == 0 && this.immediateEditingScope != null)
            {
                // if immediateEditingScope is in place and last nested normal editing scope completes,
                // we copy the information from the nested normal editing scope to the immediateEditingScope
                // and put the Changes into undo engine, so that when immediateEditingScope completes the undo unit
                // generated by nested editing scope will be collected as one undo unit
                if (this.immediateEditingScope != modelEditingScopeImpl)
                {
                    UndoEngine undoEngine = this.Context.Services.GetService<UndoEngine>();

                    this.immediateEditingScope.Changes.AddRange(modelEditingScopeImpl.Changes);
                    this.immediateEditingScope.HasModelChanges = this.immediateEditingScope.HasModelChanges || modelEditingScopeImpl.HasModelChanges;
                    this.immediateEditingScope.HasEffectiveChanges = this.immediateEditingScope.HasEffectiveChanges || modelEditingScopeImpl.HasEffectiveChanges;
                    this.immediateEditingScope.HandleModelItemsAdded(modelEditingScopeImpl.ItemsAdded);
                    this.immediateEditingScope.HandleModelItemsRemoved(modelEditingScopeImpl.ItemsRemoved);

                    if (modelEditingScopeImpl.HasEffectiveChanges)
                    {
                        if (!this.RedoUndoInProgress && !modelEditingScopeImpl.SuppressUndo && undoEngine != null)
                        {
                            undoEngine.AddUndoUnit(new EditingScopeUndoUnit(this.Context, this, modelEditingScopeImpl));
                        }
                    }
                }
            }

            if (editingScopes.Count == 0 && !(modelEditingScopeImpl is ImmediateEditingScope) && modelEditingScopeImpl.HasModelChanges)
            {
                if (!modelEditingScopeImpl.SuppressValidationOnComplete)
                {
                    ValidationService validationService = this.Context.Services.GetService<ValidationService>();
                    if (validationService != null)
                    {
                        validationService.ValidateWorkflow(ValidationReason.ModelChange);
                    }
                }
            }

            if (this.immediateEditingScope == modelEditingScopeImpl)
            {
                this.immediateEditingScope = null;
            }

            // if the outer most scope completed notify listeners
            if (this.EditingScopeCompleted != null && editingScopes.Count == 0 && this.immediateEditingScope == null)
            {
                this.EditingScopeCompleted(this, new EditingScopeEventArgs() { EditingScope = modelEditingScopeImpl });
            }

            Fx.Assert(editingScopes.Count == 0 || (modelEditingScopeImpl.ItemsAdded.Count == 0 && modelEditingScopeImpl.ItemsRemoved.Count == 0), "Inner editing scope shouldn't have changes applied.");
#if DEBUG
            this.graphManager.VerifyBackPointers();
#endif
        }

        internal bool CanEditingScopeComplete(EditingScope modelEditingScopeImpl)
        {
            ReadOnlyState readOnlyState = this.Context.Items.GetValue<ReadOnlyState>();
            return (modelEditingScopeImpl == editingScopes.Peek()) && (readOnlyState == null || !readOnlyState.IsReadOnly);
        }

        internal void OnEditingScopeReverted(EditingScope modelEditingScopeImpl)
        {
            if (editingScopes.Contains(modelEditingScopeImpl))
            {
                editingScopes.Pop();
            }

            if (this.immediateEditingScope == modelEditingScopeImpl)
            {
                this.immediateEditingScope = null;
            }
        }

        internal static IList<ModelItem> DepthFirstSearch(ModelItem currentItem, Predicate<Type> filter, Predicate<ModelItem> shouldTraverseSubTree, bool preOrder)
        {
            IList<ModelItem> foundItems = new List<ModelItem>();
            HashSet<ModelItem> alreadyVisitedItems = new HashSet<ModelItem>();
            RecursiveDepthFirstSearch(currentItem, filter, shouldTraverseSubTree, foundItems, alreadyVisitedItems, preOrder);
            return foundItems;
        }

        private static void RecursiveDepthFirstSearch(ModelItem currentItem, Predicate<Type> filter, Predicate<ModelItem> shouldTraverseSubTree, IList<ModelItem> foundItems, HashSet<ModelItem> alreadyVisitedItems, bool preOrder)
        {
            if (currentItem == null)
            {
                return;
            }

            if (typeof(Type).IsAssignableFrom(currentItem.ItemType))
            {
                return;
            }

            if (currentItem.ItemType.IsGenericType && currentItem.ItemType.GetGenericTypeDefinition() == typeof(Action<>))
            {
                return;
            }

            if (!shouldTraverseSubTree(currentItem))
            {
                return;
            }

            if (preOrder)
            {
                if (filter(currentItem.ItemType))
                {
                    foundItems.Add(currentItem);
                }
            }

            alreadyVisitedItems.Add(currentItem);

            List<ModelItem> neighbors = GetNeighbors(currentItem, (m) => true);
            foreach (ModelItem neighbor in neighbors)
            {
                if (!alreadyVisitedItems.Contains(neighbor))
                {
                    RecursiveDepthFirstSearch(neighbor, filter, shouldTraverseSubTree, foundItems, alreadyVisitedItems, preOrder);
                }
            }

            if (!preOrder)
            {
                if (filter(currentItem.ItemType))
                {
                    foundItems.Add(currentItem);
                }
            }
        }

        private void OnModelItemsAdded(IEnumerable<ModelItem> addedModelItems)
        {
            Fx.Assert(addedModelItems != null, "addedModelItems should not be null.");
            EventHandler<ModelItemsAddedEventArgs> tempModelItemsAdded = this.ModelItemsAdded;
            if (tempModelItemsAdded != null)
            {
                tempModelItemsAdded(this, new ModelItemsAddedEventArgs(addedModelItems));
            }
        }

        private void OnModelItemsRemoved(IEnumerable<ModelItem> removedModelItems)
        {
            Fx.Assert(removedModelItems != null, "removedModelItems should not be null.");
            EventHandler<ModelItemsRemovedEventArgs> tempModelItemsRemoved = this.ModelItemsRemoved;
            if (tempModelItemsRemoved != null)
            {
                tempModelItemsRemoved(this, new ModelItemsRemovedEventArgs(removedModelItems));
            }
        }

        internal class ModelGraphManager : GraphManager<ModelItem, Edge, BackPointer>
        {
            private ModelTreeManager modelTreeManager;

            public ModelGraphManager(ModelTreeManager modelTreeManager)
            {
                Fx.Assert(modelTreeManager != null, "modelTreeManager should not be null");

                this.modelTreeManager = modelTreeManager;
            }

            public void OnPropertyEdgeAdded(string propertyName, ModelItem from, ModelItem to)
            {
                base.OnEdgeAdded(new Edge(propertyName, from, to));
            }

            public void OnItemEdgeAdded(ModelItem from, ModelItem to)
            {
                base.OnEdgeAdded(new Edge(from, to));
            }

            public void OnPropertyEdgeRemoved(string propertyName, ModelItem from, ModelItem to)
            {
                base.OnEdgeRemoved(new Edge(propertyName, from, to));
            }

            public void OnItemEdgeRemoved(ModelItem from, ModelItem to)
            {
                base.OnEdgeRemoved(new Edge(from, to));
            }

            public new void OnRootChanged(ModelItem oldRoot, ModelItem newRoot)
            {
                base.OnRootChanged(oldRoot, newRoot);
            }

            protected override ModelItem Root
            {
                get { return this.modelTreeManager.Root; }
            }

            protected override IEnumerable<ModelItem> GetVertices()
            {
                foreach (WeakReference weakReference in this.modelTreeManager.objectMap.Values)
                {
                    ModelItem modelItem = weakReference.Target as ModelItem;
                    if (modelItem != null)
                    {
                        yield return modelItem;
                    }
                }
            }

            // This method will not expand any ModelItem
            protected override IEnumerable<Edge> GetOutEdges(ModelItem vertex)
            {
                Fx.Assert(vertex != null, "vertex should not be null");

                List<Edge> edges = new List<Edge>();

                foreach (KeyValuePair<string, ModelItem> kvp in ((IModelTreeItem)vertex).ModelPropertyStore)
                {
                    if (kvp.Value != null)
                    {
                        edges.Add(new Edge(kvp.Key, vertex, kvp.Value));
                    }
                }

                ModelItemCollection collection = vertex as ModelItemCollection;
                if (collection != null)
                {
                    foreach (ModelItem modelItem in collection.Distinct())
                    {
                        if (modelItem != null)
                        {
                            edges.Add(new Edge(vertex, modelItem));
                        }
                    }
                }

                ModelItemDictionaryImpl dictionary = vertex as ModelItemDictionaryImpl;
                if (dictionary != null)
                {
                    List<ModelItem> items = new List<ModelItem>(dictionary.Keys);
                    items.AddRange(dictionary.Values);
                    items.Add(dictionary.updateKeySavedValue);
                    foreach (ModelItem modelItem in items.Distinct())
                    {
                        if (modelItem != null)
                        {
                            edges.Add(new Edge(vertex, modelItem));
                        }
                    }
                }

                return edges;
            }

            protected override IEnumerable<BackPointer> GetBackPointers(ModelItem vertex)
            {
                List<BackPointer> backPointers = new List<BackPointer>();

                foreach (ModelItem parent in ((IModelTreeItem)vertex).ItemBackPointers)
                {
                    backPointers.Add(new BackPointer(vertex, parent));
                }

                foreach (ModelProperty property in vertex.Sources)
                {
                    backPointers.Add(new BackPointer(property.Name, vertex, property.Parent));
                }

                foreach (BackPointer backPointer in ((IModelTreeItem)vertex).ExtraPropertyBackPointers)
                {
                    backPointers.Add(backPointer);
                }

                return backPointers;
            }

            protected override ModelItem GetDestinationVertexFromEdge(Edge edge)
            {
                return edge.DestinationVertex;
            }

            protected override ModelItem GetSourceVertexFromEdge(Edge edge)
            {
                return edge.SourceVertex;
            }

            protected override ModelItem GetDestinationVertexFromBackPointer(BackPointer backPointer)
            {
                return backPointer.DestinationVertex;
            }

            protected override void RemoveAssociatedBackPointer(Edge edge)
            {
                if (edge.LinkType == LinkType.Property)
                {
                    ModelProperty modelProperty = edge.SourceVertex.Properties[edge.PropertyName];
                    if (modelProperty != null)
                    {
                        ((IModelTreeItem)edge.DestinationVertex).RemoveSource(modelProperty);
                    }
                    else
                    {
                        // in case of custom type descriptor, it may return a list of ModelProperties different from IModelTreeItem.ModelPropertyStore
                        // manually manipulate IModelTreeItem.Sources to remove back pointer
                        ((IModelTreeItem)edge.DestinationVertex).RemoveSource(edge.SourceVertex, edge.PropertyName);
                    }
                }
                else
                {
                    Fx.Assert(edge.LinkType == LinkType.Item, "unknown LinkType");
                    ((IModelTreeItem)edge.DestinationVertex).RemoveParent(edge.SourceVertex);
                }
            }

            protected override void AddAssociatedBackPointer(Edge edge)
            {
                if (edge.LinkType == LinkType.Property)
                {
                    ModelProperty modelProperty = edge.SourceVertex.Properties[edge.PropertyName];
                    if (modelProperty != null)
                    {
                        ((IModelTreeItem)edge.DestinationVertex).SetSource(modelProperty);
                    }
                    else
                    {
                        ((IModelTreeItem)edge.DestinationVertex).ExtraPropertyBackPointers.Add(new BackPointer(edge.PropertyName, edge.DestinationVertex, edge.SourceVertex));
                    }
                }
                else
                {
                    Fx.Assert(edge.LinkType == LinkType.Item, "unknown LinkType");
                    ((IModelTreeItem)edge.DestinationVertex).SetParent(edge.SourceVertex);
                }
            }

            protected override bool HasBackPointer(Edge edge)
            {
                ModelItem from = edge.SourceVertex;

                if (edge.LinkType == LinkType.Property)
                {
                    foreach (ModelProperty p in edge.DestinationVertex.Sources)
                    {
                        if (p.Parent == from && p.Name == edge.PropertyName)
                        {
                            return true;
                        }
                    }

                    foreach (BackPointer bp in ((IModelTreeItem)edge.DestinationVertex).ExtraPropertyBackPointers)
                    {
                        if (bp.DestinationVertex == from && bp.PropertyName == edge.PropertyName)
                        {
                            return true;
                        }
                    }

                    return false;
                }
                else
                {
                    Fx.Assert(edge.LinkType == LinkType.Item, "unknown LinkType");
                    return ((IModelTreeItem)edge.DestinationVertex).ItemBackPointers.Contains(from);
                }
            }

            protected override bool HasAssociatedEdge(BackPointer backPointer)
            {
                if (backPointer.LinkType == LinkType.Property)
                {
                    return ((IModelTreeItem)backPointer.DestinationVertex).ModelPropertyStore[backPointer.PropertyName] == backPointer.SourceVertex;
                }
                else
                {
                    Fx.Assert(backPointer.LinkType == LinkType.Item, "unknown LinkType");

                    ModelItemCollection collection = backPointer.DestinationVertex as ModelItemCollection;
                    if (collection != null)
                    {
                        return collection.Contains(backPointer.SourceVertex);
                    }

                    ModelItemDictionary dictionary = (ModelItemDictionary)backPointer.DestinationVertex;
                    return dictionary.Keys.Concat(dictionary.Values).Contains(backPointer.SourceVertex);
                }
            }

            protected override void OnVerticesBecameReachable(IEnumerable<ModelItem> reachableVertices)
            {
                Fx.Assert(reachableVertices != null, "reachableVertices should not be null");
                this.modelTreeManager.OnModelItemsAdded(reachableVertices);
            }

            protected override void OnVerticesBecameUnreachable(IEnumerable<ModelItem> unreachableVertices)
            {
                Fx.Assert(unreachableVertices != null, "unreachableVertices should not be null");
                this.modelTreeManager.OnModelItemsRemoved(unreachableVertices);
            }
        }

        class DictionaryTypeDescriptionProvider : TypeDescriptionProvider
        {
            Type type;
            public DictionaryTypeDescriptionProvider(Type type)
                : base(TypeDescriptor.GetProvider(type))
            {
                this.type = type;
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                ICustomTypeDescriptor defaultDescriptor = base.GetTypeDescriptor(objectType, instance);
                return new DictionaryTypeDescriptor(defaultDescriptor, this.type);
            }
        }

        class DictionaryTypeDescriptor : CustomTypeDescriptor
        {
            Type type;

            public DictionaryTypeDescriptor(ICustomTypeDescriptor parent, Type type)
                : base(parent)
            {
                this.type = type;
            }

            // Expose one more property, a collection of MutableKeyValuePairs,  described by ItemsCollectionPropertyDescriptor
            public override PropertyDescriptorCollection GetProperties()
            {
                return new PropertyDescriptorCollection(base.GetProperties().Cast<PropertyDescriptor>()
                    .Union(new PropertyDescriptor[] { new ItemsCollectionPropertyDescriptor(type) }).ToArray());
            }

            // Expose one more property, a collection of MutableKeyValuePairs,  described by ItemsCollectionPropertyDescriptor
            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                return new PropertyDescriptorCollection(base.GetProperties(attributes).Cast<PropertyDescriptor>()
                    .Union(new PropertyDescriptor[] { new ItemsCollectionPropertyDescriptor(type) }).ToArray());
            }
        }

        class ItemsCollectionPropertyDescriptor : PropertyDescriptor
        {
            Type dictionaryType;
            Type[] genericArguments;
            Type kvpairType;
            Type itemType;
            Type propertyType;

            internal ItemsCollectionPropertyDescriptor(Type type)
                : base("ItemsCollection", null)
            {
                this.dictionaryType = type;
            }

            Type[] GenericArguments
            {
                get
                {
                    if (this.genericArguments == null)
                    {
                        object[] result = new object[2] { false, false };
                        Type[] interfaces = this.ComponentType.FindInterfaces(ModelTreeManager.CheckInterface, result);
                        foreach (Type type in interfaces)
                        {
                            if (type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                            {
                                this.genericArguments = type.GetGenericArguments();
                                Fx.Assert(this.genericArguments.Length == 2, "this.genericArguments.Length should be = 2");
                                return this.genericArguments;
                            }
                        }
                        Debug.Fail("Cannot find generic arguments for IDictionary<,>.");
                    }
                    return this.genericArguments;
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is intended for use through reflection")]
            Type KVPairType
            {
                get
                {
                    if (this.kvpairType == null)
                    {
                        this.kvpairType = typeof(KeyValuePair<,>).MakeGenericType(this.GenericArguments);
                    }
                    return this.kvpairType;
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is intended for use through reflection")]
            Type ItemType
            {
                get
                {
                    if (this.itemType == null)
                    {
                        this.itemType = typeof(ModelItemKeyValuePair<,>).MakeGenericType(this.GenericArguments);
                    }
                    return this.itemType;
                }
            }

            public override Type ComponentType
            {
                get { return this.dictionaryType; }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    if (this.propertyType == null)
                    {
                        this.propertyType = typeof(DictionaryItemsCollection<,>).MakeGenericType(this.GenericArguments);
                    }
                    return this.propertyType;
                }
            }

            public override bool IsBrowsable
            {
                get
                {
                    return false;
                }
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override object GetValue(object component)
            {
                return Activator.CreateInstance(this.PropertyType, new object[] { component });
            }

            public override void ResetValue(object component)
            {
                Debug.Fail("ResetValue is not implemented.");
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public override void SetValue(object component, object value)
            {
                Debug.Fail("SetValue is not implemented.");
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }
    }
}
