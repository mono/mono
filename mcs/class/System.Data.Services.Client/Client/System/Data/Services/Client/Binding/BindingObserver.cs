//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
#region Namespaces
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
#endregion    

    internal sealed class BindingObserver
    {
        #region Fields
        
        private BindingGraph bindingGraph;

        #endregion

        #region Constructor
        
        internal BindingObserver(DataServiceContext context, Func<EntityChangedParams, bool> entityChanged, Func<EntityCollectionChangedParams, bool> collectionChanged)
        {
            Debug.Assert(context != null, "Must have been validated during DataServiceCollection construction.");
            this.Context = context;
            this.Context.ChangesSaved += this.OnChangesSaved;
            
            this.EntityChanged = entityChanged;
            this.CollectionChanged = collectionChanged;
            
            this.bindingGraph = new BindingGraph(this);
        }
        
        #endregion

        #region Properties

        internal DataServiceContext Context
        {
            get;
            private set;
        }

        internal bool AttachBehavior
        {
            get;
            set;
        }

        internal bool DetachBehavior
        {
            get;
            set;
        }

        internal Func<EntityChangedParams, bool> EntityChanged
        {
            get;
            private set;
        }

        internal Func<EntityCollectionChangedParams, bool> CollectionChanged
        {
            get;
            private set;
        }

        #endregion

        #region Methods
        
        internal void StartTracking<T>(DataServiceCollection<T> collection, string collectionEntitySet)
        {
            Debug.Assert(collection != null, "Only constructed collections are tracked.");
            
            if (!BindingEntityInfo.IsEntityType(typeof(T)))
            {
                throw new ArgumentException(Strings.DataBinding_DataServiceCollectionArgumentMustHaveEntityType(typeof(T)));
            }

            try
            {
                this.AttachBehavior = true;

                this.bindingGraph.AddCollection(null, null, collection, collectionEntitySet);
            }
            finally
            {
                this.AttachBehavior = false;
            }
        }

        internal void StopTracking()
        {
            this.bindingGraph.Reset();

            this.Context.ChangesSaved -= this.OnChangesSaved;
        }

#if ASTORIA_LIGHT
        internal bool LookupParent<T>(DataServiceCollection<T> collection, out object parentEntity, out string parentProperty)
        {
            string sourceEntitySet;
            string targetEntitySet;
            this.bindingGraph.GetEntityCollectionInfo(collection, out parentEntity, out parentProperty, out sourceEntitySet, out targetEntitySet);

            return parentEntity != null;
        }
#endif

        internal void OnPropertyChanged(object source, PropertyChangedEventArgs eventArgs)
        {
            Util.CheckArgumentNull(source, "source");
            Util.CheckArgumentNull(eventArgs, "eventArgs");

#if DEBUG
            Debug.Assert(this.bindingGraph.IsTracking(source), "Entity must be part of the graph if it has the event notification registered.");
#endif
            string sourceProperty = eventArgs.PropertyName;

            if (String.IsNullOrEmpty(sourceProperty))
            {
                this.HandleUpdateEntity(
                        source,
                        null,
                        null);
            }
            else
            {
                BindingEntityInfo.BindingPropertyInfo bpi;
                
                object sourcePropertyValue = BindingEntityInfo.GetPropertyValue(source, sourceProperty, out bpi);
                
                if (bpi != null)
                {
                    this.bindingGraph.RemoveRelation(source, sourceProperty);

                    switch (bpi.PropertyKind)
                    {
                        case BindingPropertyKind.BindingPropertyKindCollection:
                            if (sourcePropertyValue != null)
                            {
                                try
                                {
                                    typeof(BindingUtils)
                                        .GetMethod("VerifyObserverNotPresent", BindingFlags.NonPublic | BindingFlags.Static)
                                        .MakeGenericMethod(bpi.PropertyInfo.CollectionType)
                                        .Invoke(null, new object[] { sourcePropertyValue, sourceProperty, source.GetType() });
                                }
                                catch (TargetInvocationException tie)
                                {
                                    throw tie.InnerException;
                                }

                                try
                                {
                                    this.AttachBehavior = true;
                                    this.bindingGraph.AddCollection(
                                            source,
                                            sourceProperty,
                                            sourcePropertyValue,
                                            null);
                                }
                                finally
                                {
                                    this.AttachBehavior = false;
                                }
                            }
                            
                            break;

                        case BindingPropertyKind.BindingPropertyKindEntity:
                            this.bindingGraph.AddEntity(
                                    source,
                                    sourceProperty,
                                    sourcePropertyValue,
                                    null,
                                    source);
                            break;

                        default:
                            Debug.Assert(bpi.PropertyKind == BindingPropertyKind.BindingPropertyKindComplex, "Must be complex type if PropertyKind is not entity or collection.");
                            
                            if (sourcePropertyValue != null)
                            {
                                this.bindingGraph.AddComplexProperty(
                                        source, 
                                        sourceProperty, 
                                        sourcePropertyValue);
                            }

                            this.HandleUpdateEntity(
                                    source,
                                    sourceProperty,
                                    sourcePropertyValue);
                            break;
                    }
                }
                else
                {
                    this.HandleUpdateEntity(
                            source, 
                            sourceProperty, 
                            sourcePropertyValue);
                }
            }
        }

        internal void OnCollectionChanged(object collection, NotifyCollectionChangedEventArgs eventArgs)
        {
            Util.CheckArgumentNull(collection, "collection");
            Util.CheckArgumentNull(eventArgs, "eventArgs");

            Debug.Assert(BindingEntityInfo.IsDataServiceCollection(collection.GetType()), "We only register this event for DataServiceCollections.");
#if DEBUG
            Debug.Assert(this.bindingGraph.IsTracking(collection), "Collection must be part of the graph if it has the event notification registered.");
#endif
            object source;
            string sourceProperty;
            string sourceEntitySet;
            string targetEntitySet;

            this.bindingGraph.GetEntityCollectionInfo(
                    collection, 
                    out source, 
                    out sourceProperty, 
                    out sourceEntitySet, 
                    out targetEntitySet);

            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.OnAddToCollection(
                            eventArgs, 
                            source, 
                            sourceProperty, 
                            targetEntitySet, 
                            collection);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    this.OnDeleteFromCollection(
                            eventArgs, 
                            source, 
                            sourceProperty, 
                            collection);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    this.OnDeleteFromCollection(
                            eventArgs, 
                            source, 
                            sourceProperty, 
                            collection);
                            
                    this.OnAddToCollection(
                            eventArgs, 
                            source, 
                            sourceProperty, 
                            targetEntitySet, 
                            collection);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (this.DetachBehavior)
                    {
                        this.RemoveWithDetachCollection(collection);
                    }
                    else
                    {
                        this.bindingGraph.RemoveCollection(collection);
                    }

                    break;

#if !ASTORIA_LIGHT
                case NotifyCollectionChangedAction.Move:
                    break;
#endif

                default:
                    throw new InvalidOperationException(Strings.DataBinding_CollectionChangedUnknownAction(eventArgs.Action));
            }
        }

        internal void HandleAddEntity(
            object source,
            string sourceProperty,
            string sourceEntitySet,
            ICollection collection,
            object target,
            string targetEntitySet)
        {
            if (this.Context.ApplyingChanges)
            {
                return;
            }
            
            Debug.Assert(
                (source == null && sourceProperty == null) || (source != null && !String.IsNullOrEmpty(sourceProperty)), 
                "source and sourceProperty should either both be present or both be absent.");
        
            Debug.Assert(target != null, "target must be provided by the caller.");
            Debug.Assert(BindingEntityInfo.IsEntityType(target.GetType()), "target must be an entity type.");

            if (source != null && this.IsDetachedOrDeletedFromContext(source))
            {
                return;
            }
            
            EntityDescriptor targetDescriptor = this.Context.GetEntityDescriptor(target);

            bool contextOperationRequired = !this.AttachBehavior && 
                                           (targetDescriptor == null ||
                                           (source != null && !this.IsContextTrackingLink(source, sourceProperty, target) && targetDescriptor.State != EntityStates.Deleted));

            if (contextOperationRequired)
            {
                if (this.CollectionChanged != null)
                {
                    EntityCollectionChangedParams args = new EntityCollectionChangedParams(
                            this.Context,
                            source,
                            sourceProperty,
                            sourceEntitySet,
                            collection,
                            target,
                            targetEntitySet,
                            NotifyCollectionChangedAction.Add);

                    if (this.CollectionChanged(args))
                    {
                        return;
                    }
                }
            }

            if (source != null && this.IsDetachedOrDeletedFromContext(source))
            {
                throw new InvalidOperationException(Strings.DataBinding_BindingOperation_DetachedSource);
            }

            targetDescriptor = this.Context.GetEntityDescriptor(target);
            
            if (source != null)
            {
                if (this.AttachBehavior)
                {
                    if (targetDescriptor == null)
                    {
                        BindingUtils.ValidateEntitySetName(targetEntitySet, target);
                        
                        this.Context.AttachTo(targetEntitySet, target);
                        this.Context.AttachLink(source, sourceProperty, target);
                    }
                    else
                    if (targetDescriptor.State != EntityStates.Deleted && !this.IsContextTrackingLink(source, sourceProperty, target))
                    {
                        this.Context.AttachLink(source, sourceProperty, target);
                    }
                }
                else
                {
                    if (targetDescriptor == null)
                    {
                        this.Context.AddRelatedObject(source, sourceProperty, target);
                    }
                    else
                    if (targetDescriptor.State != EntityStates.Deleted && !this.IsContextTrackingLink(source, sourceProperty, target))
                    {
                        this.Context.AddLink(source, sourceProperty, target);
                    }
                }
            }
            else
            if (targetDescriptor == null)
            {
                BindingUtils.ValidateEntitySetName(targetEntitySet, target);
                
                if (this.AttachBehavior)
                {
                    this.Context.AttachTo(targetEntitySet, target);
                }
                else
                {
                    this.Context.AddObject(targetEntitySet, target);
                }
            }
        }

        internal void HandleDeleteEntity(
            object source,
            string sourceProperty,
            string sourceEntitySet,
            ICollection collection,
            object target,
            string targetEntitySet)
        {
            if (this.Context.ApplyingChanges)
            {
                return;
            }

            Debug.Assert(
                (source == null && sourceProperty == null) || (source != null && !String.IsNullOrEmpty(sourceProperty)),
                "source and sourceProperty should either both be present or both be absent.");

            Debug.Assert(target != null, "target must be provided by the caller.");
            Debug.Assert(BindingEntityInfo.IsEntityType(target.GetType()), "target must be an entity type.");

            Debug.Assert(!this.AttachBehavior, "AttachBehavior is only allowed during Construction and Load when this method should never be entered.");

            if (source != null && this.IsDetachedOrDeletedFromContext(source))
            {
                return;
            }

            bool contextOperationRequired = this.IsContextTrackingEntity(target) && !this.DetachBehavior;
            
            if (contextOperationRequired)
            {
                if (this.CollectionChanged != null)
                {
                    EntityCollectionChangedParams args = new EntityCollectionChangedParams(
                            this.Context,
                            source,
                            sourceProperty,
                            sourceEntitySet,
                            collection,
                            target,
                            targetEntitySet,
                            NotifyCollectionChangedAction.Remove);

                    if (this.CollectionChanged(args))
                    {
                        return;
                    }
                }
            }

            if (source != null && !this.IsContextTrackingEntity(source))
            {
                throw new InvalidOperationException(Strings.DataBinding_BindingOperation_DetachedSource);
            }

            if (this.IsContextTrackingEntity(target))
            {
                if (this.DetachBehavior)
                {
                    this.Context.Detach(target);
                }
                else
                {
                    this.Context.DeleteObject(target);
                }
            }
        }

        internal void HandleUpdateEntityReference(
            object source,
            string sourceProperty,
            string sourceEntitySet,
            object target,
            string targetEntitySet)
        {
            if (this.Context.ApplyingChanges)
            {
                return;
            }

            Debug.Assert(source != null, "source can not be null for update operations.");
            Debug.Assert(BindingEntityInfo.IsEntityType(source.GetType()), "source must be an entity with keys.");
            Debug.Assert(!String.IsNullOrEmpty(sourceProperty), "sourceProperty must be a non-empty string for update operations.");
            Debug.Assert(!String.IsNullOrEmpty(sourceEntitySet), "sourceEntitySet must be non-empty string for update operation.");

            if (this.IsDetachedOrDeletedFromContext(source))
            {
                return;
            }

            EntityDescriptor targetDescriptor = target != null ? this.Context.GetEntityDescriptor(target) : null;

            bool contextOperationRequired = !this.AttachBehavior && 
                                            (targetDescriptor == null ||
                                            !this.IsContextTrackingLink(source, sourceProperty, target));

            if (contextOperationRequired)
            {
                if (this.EntityChanged != null)
                {
                    EntityChangedParams args = new EntityChangedParams(
                                                    this.Context,
                                                    source,
                                                    sourceProperty,
                                                    target,
                                                    sourceEntitySet,
                                                    targetEntitySet);

                    if (this.EntityChanged(args))
                    {
                        return;
                    }
                }
            }

            if (this.IsDetachedOrDeletedFromContext(source))
            {
                throw new InvalidOperationException(Strings.DataBinding_BindingOperation_DetachedSource);
            }

            targetDescriptor = target != null ? this.Context.GetEntityDescriptor(target) : null;

            if (target != null)
            {
                if (targetDescriptor == null)
                {
                    BindingUtils.ValidateEntitySetName(targetEntitySet, target);
                    
                    if (this.AttachBehavior)
                    {
                        this.Context.AttachTo(targetEntitySet, target);
                    }
                    else
                    {
                        this.Context.AddObject(targetEntitySet, target);
                    }
                    
                    targetDescriptor = this.Context.GetEntityDescriptor(target);
                }

                if (!this.IsContextTrackingLink(source, sourceProperty, target))
                {
                    if (this.AttachBehavior)
                    {
                        if (targetDescriptor.State != EntityStates.Deleted)
                        {
                            this.Context.AttachLink(source, sourceProperty, target);
                        }
                    }
                    else
                    {
                        this.Context.SetLink(source, sourceProperty, target);
                    }
                }
            }
            else
            {
                Debug.Assert(!this.AttachBehavior, "During attach operations we must never perform operations for null values.");
                
                this.Context.SetLink(source, sourceProperty, null);
            }
        }

        internal bool IsContextTrackingEntity(object entity)
        {
            Debug.Assert(entity != null, "entity must be provided when checking for context tracking.");
            return this.Context.GetEntityDescriptor(entity) != default(EntityDescriptor);
        }

        private void HandleUpdateEntity(object entity, string propertyName, object propertyValue)
        {
            Debug.Assert(!this.AttachBehavior || this.Context.ApplyingChanges, "Entity updates must not happen during Attach or construction phases, deserialization case is the exception.");

            if (this.Context.ApplyingChanges)
            {
                return;
            }

            if (!BindingEntityInfo.IsEntityType(entity.GetType()))
            {
                this.bindingGraph.GetAncestorEntityForComplexProperty(ref entity, ref propertyName, ref propertyValue);
            }

            Debug.Assert(entity != null, "entity must be provided for update operations.");
            Debug.Assert(BindingEntityInfo.IsEntityType(entity.GetType()), "entity must be an entity with keys.");
            Debug.Assert(!String.IsNullOrEmpty(propertyName) || propertyValue == null, "When propertyName is null no propertyValue should be provided.");

            if (this.IsDetachedOrDeletedFromContext(entity))
            {
                return;
            }

            if (this.EntityChanged != null)
            {
                EntityChangedParams args = new EntityChangedParams(
                                                this.Context, 
                                                entity, 
                                                propertyName, 
                                                propertyValue, 
                                                null, 
                                                null);

                if (this.EntityChanged(args))
                {
                    return;
                }
            }

            if (this.IsContextTrackingEntity(entity))
            {
                this.Context.UpdateObject(entity);
            }
        }

        private void OnAddToCollection(
            NotifyCollectionChangedEventArgs eventArgs,
            object source,
            String sourceProperty,
            String targetEntitySet,
            object collection)
        {
            Debug.Assert(collection != null, "Must have a valid collection to which entities are added.");
            
            if (eventArgs.NewItems != null)
            {
                foreach (object target in eventArgs.NewItems)
                {
                    if (target == null)
                    {
                        throw new InvalidOperationException(Strings.DataBinding_BindingOperation_ArrayItemNull("Add"));
                    }

                    if (!BindingEntityInfo.IsEntityType(target.GetType()))
                    {
                        throw new InvalidOperationException(Strings.DataBinding_BindingOperation_ArrayItemNotEntity("Add"));
                    }

                    this.bindingGraph.AddEntity(
                            source, 
                            sourceProperty, 
                            target, 
                            targetEntitySet, 
                            collection);
                }
            }
        }

        private void OnDeleteFromCollection(
            NotifyCollectionChangedEventArgs eventArgs,
            object source,
            String sourceProperty,
            object collection)
        {
            Debug.Assert(collection != null, "Must have a valid collection from which entities are removed.");
            Debug.Assert(
                (source == null && sourceProperty == null) || (source != null && !String.IsNullOrEmpty(sourceProperty)), 
                "source and sourceProperty must both be null or both be non-null.");

            if (eventArgs.OldItems != null)
            {
                this.DeepRemoveCollection(
                        eventArgs.OldItems, 
                        source ?? collection, 
                        sourceProperty, 
                        this.ValidateCollectionItem);
            }
        }

        private void RemoveWithDetachCollection(object collection)
        {
            Debug.Assert(this.DetachBehavior, "Must be detaching each item in collection.");

            object source = null;
            string sourceProperty = null;
            string sourceEntitySet = null;
            string targetEntitySet = null;

            this.bindingGraph.GetEntityCollectionInfo(
                    collection,
                    out source,
                    out sourceProperty,
                    out sourceEntitySet,
                    out targetEntitySet);

            this.DeepRemoveCollection(
                    this.bindingGraph.GetCollectionItems(collection),
                    source ?? collection,
                    sourceProperty,
                    null);
        }

        private void DeepRemoveCollection(IEnumerable collection, object source, string sourceProperty, Action<object> itemValidator)
        {
            foreach (object target in collection)
            {
                if (itemValidator != null)
                {
                    itemValidator(target);
                }

                List<UnTrackingInfo> untrackingInfo = new List<UnTrackingInfo>();

                this.CollectUnTrackingInfo(
                        target,
                        source,
                        sourceProperty,
                        untrackingInfo);

                foreach (UnTrackingInfo info in untrackingInfo)
                {
                    this.bindingGraph.Remove(
                            info.Entity,
                            info.Parent,
                            info.ParentProperty);
                }
            }

            this.bindingGraph.RemoveUnreachableVertices();
        }

        private void OnChangesSaved(object sender, SaveChangesEventArgs eventArgs)
        {
            this.bindingGraph.RemoveNonTrackedEntities();
        }

        private void CollectUnTrackingInfo(
            object currentEntity, 
            object parentEntity, 
            string parentProperty, 
            IList<UnTrackingInfo> entitiesToUnTrack)
        {
            foreach (var ed in this.Context
                                   .Entities
                                   .Where(x => x.ParentEntity == currentEntity && x.State == EntityStates.Added))
            {
                this.CollectUnTrackingInfo(
                        ed.Entity, 
                        ed.ParentEntity, 
                        ed.ParentPropertyForInsert, 
                        entitiesToUnTrack);
            }
            
            entitiesToUnTrack.Add(new UnTrackingInfo 
                                  {
                                    Entity = currentEntity, 
                                    Parent = parentEntity, 
                                    ParentProperty = parentProperty
                                  });
        }

        private bool IsContextTrackingLink(object source, string sourceProperty, object target)
        {
            Debug.Assert(source != null, "source entity must be provided.");
            Debug.Assert(BindingEntityInfo.IsEntityType(source.GetType()), "source must be an entity with keys.");

            Debug.Assert(!String.IsNullOrEmpty(sourceProperty), "sourceProperty must be provided.");

            Debug.Assert(target != null, "target entity must be provided.");
            Debug.Assert(BindingEntityInfo.IsEntityType(target.GetType()), "target must be an entity with keys.");
            
            return this.Context.GetLinkDescriptor(source, sourceProperty, target) != default(LinkDescriptor);
        }
        
        private bool IsDetachedOrDeletedFromContext(object entity)
        {
            Debug.Assert(entity != null, "entity must be provided.");
            Debug.Assert(BindingEntityInfo.IsEntityType(entity.GetType()), "entity must be an entity with keys.");

            EntityDescriptor descriptor = this.Context.GetEntityDescriptor(entity);
            return descriptor == null || descriptor.State == EntityStates.Deleted;
        }

        private void ValidateCollectionItem(object target)
        {
            if (target == null)
            {
                throw new InvalidOperationException(Strings.DataBinding_BindingOperation_ArrayItemNull("Remove"));
            }

            if (!BindingEntityInfo.IsEntityType(target.GetType()))
            {
                throw new InvalidOperationException(Strings.DataBinding_BindingOperation_ArrayItemNotEntity("Remove"));
            }
        }

        #endregion

        private class UnTrackingInfo
        {
            public object Entity 
            { 
                get; 
                set; 
            }
            
            public object Parent 
            { 
                get; 
                set; 
            }
            
            public string ParentProperty 
            { 
                get; 
                set; 
            }
        }
    }
}