//---------------------------------------------------------------------
// <copyright file="EntityWrapperFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Data.Objects.DataClasses;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using System.Security.Permissions;
using System.Threading;
using System.Data.Common.Utils;
using System.Runtime.CompilerServices;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// Factory class for creating IEntityWrapper instances.
    /// </summary>
    internal static class EntityWrapperFactory
    {
        // A cache of functions used to create IEntityWrapper instances for a given type
        private static readonly Memoizer<Type, Func<object, IEntityWrapper>> _delegateCache = new Memoizer<Type, Func<object, IEntityWrapper>>(CreateWrapperDelegate, null);

        /// <summary>
        /// The single instance of the NullEntityWrapper.
        /// </summary>
        internal static IEntityWrapper NullWrapper
        {
            get { return NullEntityWrapper.NullWrapper; }
        }

        /// <summary>
        /// Called to create a new wrapper outside of the normal materialization process.
        /// This method is typically used when a new entity is created outside the context and then is
        /// added or attached.  The materializer bypasses this method and calls wrapper constructors
        /// directory for performance reasons.
        /// This method does not check whether or not the wrapper already exists in the context.
        /// </summary>
        /// <param name="entity">The entity for which a wrapper will be created</param>
        /// <param name="key">The key associated with that entity, or null</param>
        /// <returns>The new wrapper instance</returns>
        internal static IEntityWrapper CreateNewWrapper(object entity, EntityKey key)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            if (entity == null)
            {
                return NullEntityWrapper.NullWrapper;
            }
            // We used a cache of functions based on the actual type of entity that we need to wrap.
            // Creatung these functions is slow, but once they are created they are relatively fast.
            IEntityWrapper wrappedEntity = _delegateCache.Evaluate(entity.GetType())(entity);
            wrappedEntity.RelationshipManager.SetWrappedOwner(wrappedEntity, entity);
            // We cast to object here to avoid calling the overridden != operator on EntityKey.
            // This creates a very small perf gain, which is none-the-less significant for lean no-tracking cases.
            if ((object)key != null && (object)wrappedEntity.EntityKey == null)
            {
                wrappedEntity.EntityKey = key;
            }

            // If the entity is a proxy, set the wrapper to match
            EntityProxyTypeInfo proxyTypeInfo;
            if (EntityProxyFactory.TryGetProxyType(entity.GetType(), out proxyTypeInfo))
            {
                proxyTypeInfo.SetEntityWrapper(wrappedEntity);
            }

            return wrappedEntity;
        }

        // Creates a delegate that can then be used to create wrappers for a given type.
        // This is slow which is why we only create the delegate once and then cache it.
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static Func<object, IEntityWrapper> CreateWrapperDelegate(Type entityType)
        {
            // For entities that implement all our interfaces we create a special lightweight wrapper that is both
            // smaller and faster than the strategy-based wrapper.
            // Otherwise, the wrapper is provided with different delegates depending on which interfaces are implemented.
            bool isIEntityWithRelationships = typeof(IEntityWithRelationships).IsAssignableFrom(entityType);
            bool isIEntityWithChangeTracker = typeof(IEntityWithChangeTracker).IsAssignableFrom(entityType);
            bool isIEntityWithKey = typeof(IEntityWithKey).IsAssignableFrom(entityType);
            bool isProxy = EntityProxyFactory.IsProxyType(entityType);
            MethodInfo createDelegate;
            if (isIEntityWithRelationships && isIEntityWithChangeTracker && isIEntityWithKey && !isProxy)
            {
                createDelegate = typeof(EntityWrapperFactory).GetMethod("CreateWrapperDelegateTypedLightweight", BindingFlags.NonPublic | BindingFlags.Static);
            }
            else if (isIEntityWithRelationships)
            {
                // This type of strategy wrapper is used when the entity implements IEntityWithRelationships
                // In this case it is important that the entity itself is used to create the RelationshipManager
                createDelegate = typeof(EntityWrapperFactory).GetMethod("CreateWrapperDelegateTypedWithRelationships", BindingFlags.NonPublic | BindingFlags.Static);
            }
            else
            {
                createDelegate = typeof(EntityWrapperFactory).GetMethod("CreateWrapperDelegateTypedWithoutRelationships", BindingFlags.NonPublic | BindingFlags.Static);
            }
            createDelegate = createDelegate.MakeGenericMethod(entityType);
            return (Func<object, IEntityWrapper>)createDelegate.Invoke(null, new object[0]);
        }

        // Returns a delegate that creates the fast LightweightEntityWrapper
        private static Func<object, IEntityWrapper> CreateWrapperDelegateTypedLightweight<TEntity>()
             where TEntity : IEntityWithRelationships, IEntityWithKey, IEntityWithChangeTracker
        {
            return (entity) => new LightweightEntityWrapper<TEntity>((TEntity)entity);
        }

        // Returns a delegate that creates a strategy-based wrapper for entities that implement IEntityWithRelationships
        private static Func<object, IEntityWrapper> CreateWrapperDelegateTypedWithRelationships<TEntity>()
            where TEntity : IEntityWithRelationships
        {
            Func<object, IPropertyAccessorStrategy> propertyAccessorStrategy;
            Func<object, IEntityKeyStrategy> keyStrategy;
            Func<object, IChangeTrackingStrategy> changeTrackingStrategy;
            CreateStrategies<TEntity>(out propertyAccessorStrategy, out changeTrackingStrategy, out keyStrategy);
            
            return (entity) => new EntityWrapperWithRelationships<TEntity>((TEntity)entity, propertyAccessorStrategy, changeTrackingStrategy, keyStrategy);
        }

        // Returns a delegate that creates a strategy-based wrapper for entities that do not implement IEntityWithRelationships
        private static Func<object, IEntityWrapper> CreateWrapperDelegateTypedWithoutRelationships<TEntity>()
        {
            Func<object, IPropertyAccessorStrategy> propertyAccessorStrategy;
            Func<object, IEntityKeyStrategy> keyStrategy;
            Func<object, IChangeTrackingStrategy> changeTrackingStrategy;
            CreateStrategies<TEntity>(out propertyAccessorStrategy, out changeTrackingStrategy, out keyStrategy);

            return (entity) => new EntityWrapperWithoutRelationships<TEntity>((TEntity)entity, propertyAccessorStrategy, changeTrackingStrategy, keyStrategy);
        }

        // Creates delegates that create strategy objects appropriate for the type of entity.
        private static void CreateStrategies<TEntity>(out Func<object, IPropertyAccessorStrategy> createPropertyAccessorStrategy,
                                                      out Func<object, IChangeTrackingStrategy> createChangeTrackingStrategy,
                                                      out Func<object, IEntityKeyStrategy> createKeyStrategy)
        {
            Type entityType = typeof(TEntity);
            bool isIEntityWithRelationships = typeof(IEntityWithRelationships).IsAssignableFrom(entityType);
            bool isIEntityWithChangeTracker = typeof(IEntityWithChangeTracker).IsAssignableFrom(entityType);
            bool isIEntityWithKey = typeof(IEntityWithKey).IsAssignableFrom(entityType);
            bool isProxy = EntityProxyFactory.IsProxyType(entityType);

            if (!isIEntityWithRelationships || isProxy)
            {
                createPropertyAccessorStrategy = GetPocoPropertyAccessorStrategyFunc();
            }
            else
            {
                createPropertyAccessorStrategy = GetNullPropertyAccessorStrategyFunc();
            }

            if (isIEntityWithChangeTracker)
            {
                createChangeTrackingStrategy = GetEntityWithChangeTrackerStrategyFunc();
            }
            else
            {
                createChangeTrackingStrategy = GetSnapshotChangeTrackingStrategyFunc();
            }

            if (isIEntityWithKey)
            {
                createKeyStrategy = GetEntityWithKeyStrategyStrategyFunc();
            }
            else
            {
                createKeyStrategy = GetPocoEntityKeyStrategyFunc();
            }
        }

        /// <summary>
        /// Convenience function that gets the ObjectStateManager from the context and calls
        /// WrapEntityUsingStateManager.
        /// </summary>
        /// <param name="entity">the entity to wrap</param>
        /// <param name="context">the context in which the entity may exist, or null</param>
        /// <returns>a new or existing wrapper</returns>
        internal static IEntityWrapper WrapEntityUsingContext(object entity, ObjectContext context)
        {
            EntityEntry existingEntry;
            return WrapEntityUsingStateManagerGettingEntry(entity, context == null ? null : context.ObjectStateManager, out existingEntry);
        }

        /// <summary>
        /// Convenience function that gets the ObjectStateManager from the context and calls
        /// WrapEntityUsingStateManager.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="context">The context in which the entity may exist, or null</param>
        /// <param name="existingEntry">Set to the existing state entry if one is found, else null</param>
        /// <returns>a new or existing wrapper</returns>
        internal static IEntityWrapper WrapEntityUsingContextGettingEntry(object entity, ObjectContext context, out EntityEntry existingEntry)
        {
            return WrapEntityUsingStateManagerGettingEntry(entity, context == null ? null : context.ObjectStateManager, out existingEntry);
        }

        /// <summary>
        /// Wraps an entity and returns a new wrapper, or returns an existing wrapper if one
        /// already exists in the ObjectStateManager or in a RelationshipManager associated with
        /// the entity.
        /// </summary>
        /// <param name="entity">the entity to wrap</param>
        /// <param name="context">the state manager  in which the entity may exist, or null</param>
        /// <returns>a new or existing wrapper</returns>
        internal static IEntityWrapper WrapEntityUsingStateManager(object entity, ObjectStateManager stateManager)
        {
            EntityEntry existingEntry;
            return WrapEntityUsingStateManagerGettingEntry(entity, stateManager, out existingEntry);
        }

        /// <summary>
        /// Wraps an entity and returns a new wrapper, or returns an existing wrapper if one
        /// already exists in the ObjectStateManager or in a RelationshipManager associated with
        /// the entity.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="context">The state manager  in which the entity may exist, or null</param>
        /// <param name="existingEntry">The existing state entry for the given entity if one exists, otherwise null</param>
        /// <returns>A new or existing wrapper</returns>
        internal static IEntityWrapper WrapEntityUsingStateManagerGettingEntry(object entity, ObjectStateManager stateManager, out EntityEntry existingEntry)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            IEntityWrapper wrapper = null;
            existingEntry = null;

            if (entity == null)
            {
                return NullEntityWrapper.NullWrapper;
            }
            // First attempt to find an existing wrapper in the ObjectStateMager.
            if (stateManager != null)
            {
                existingEntry = stateManager.FindEntityEntry(entity);
                if (existingEntry != null)
                {
                    return existingEntry.WrappedEntity;
                }
                if (stateManager.TransactionManager.TrackProcessedEntities)
                {
                    if (stateManager.TransactionManager.WrappedEntities.TryGetValue(entity, out wrapper))
                    {
                        return wrapper;
                    }
                }
            }
            // If no entity was found in the OSM, then check if one exists on an associated
            // RelationshipManager.  This only works where the entity implements IEntityWithRelationshops.
            IEntityWithRelationships entityWithRelationships = entity as IEntityWithRelationships;
            if (entityWithRelationships != null)
            {
                RelationshipManager relManager = entityWithRelationships.RelationshipManager;
                if (relManager == null)
                {
                    throw EntityUtil.UnexpectedNullRelationshipManager();
                }
                IEntityWrapper wrappedEntity = relManager.WrappedOwner;
                if (!Object.ReferenceEquals(wrappedEntity.Entity, entity))
                {
                    // This means that the owner of the RelationshipManager must have been set
                    // incorrectly in the call to RelationshipManager.Create().
                    throw EntityUtil.InvalidRelationshipManagerOwner();
                }
                return wrappedEntity;
            }
            else
            {
                // Finally look to see if the instance is a proxy and get the wrapper from the proxy
                EntityProxyFactory.TryGetProxyWrapper(entity, out wrapper);
            }

            // If we could not find an existing wrapper, then go create a new one
            if (wrapper == null)
            {
                IEntityWithKey withKey = entity as IEntityWithKey;
                wrapper = CreateNewWrapper(entity, withKey == null ? null : withKey.EntityKey);
            }
            if (stateManager != null && stateManager.TransactionManager.TrackProcessedEntities)
            {
                stateManager.TransactionManager.WrappedEntities.Add(entity, wrapper);
            }
            return wrapper;
        }

        /// <summary>
        /// When an entity enters Object Services that was retreived with NoTracking, it may not have certain fields set that are in many cases
        /// assumed to be present. This method updates the wrapper with a key and a context.
        /// </summary>
        /// <param name="wrapper">The wrapped entity</param>
        /// <param name="context">The context that will be using this wrapper</param>
        /// <param name="entitySet">The entity set this wrapped entity belongs to</param>
        internal static void UpdateNoTrackingWrapper(IEntityWrapper wrapper, ObjectContext context, EntitySet entitySet)
        {
            if (wrapper.EntityKey == null)
            {
                wrapper.EntityKey = context.ObjectStateManager.CreateEntityKey(entitySet, wrapper.Entity);
            }
            if (wrapper.Context == null)
            {
                wrapper.AttachContext(context, entitySet, MergeOption.NoTracking);
            }
        }

        /// <summary>
        /// Returns a func that will create a PocoPropertyAccessorStrategy object for a given entity.
        /// </summary>
        /// <returns>The func to be used to create the strategy object.</returns>
        internal static Func<object, IPropertyAccessorStrategy> GetPocoPropertyAccessorStrategyFunc()
        {
            return (object entity) => new PocoPropertyAccessorStrategy(entity);
        }

        /// <summary>
        /// Returns a func that will create a null IPropertyAccessorStrategy strategy object for a given entity.
        /// </summary>
        /// <returns>The func to be used to create the strategy object.</returns>
        internal static Func<object, IPropertyAccessorStrategy> GetNullPropertyAccessorStrategyFunc()
        {
            return (object entity) => null;
        }

        /// <summary>
        /// Returns a func that will create a EntityWithChangeTrackerStrategy object for a given entity.
        /// </summary>
        /// <returns>The func to be used to create the strategy object.</returns>
        internal static Func<object, IChangeTrackingStrategy> GetEntityWithChangeTrackerStrategyFunc()
        {
            return (object entity) => new EntityWithChangeTrackerStrategy((IEntityWithChangeTracker)entity);
        }

        /// <summary>
        /// Returns a func that will create a SnapshotChangeTrackingStrategy object for a given entity.
        /// </summary>
        /// <returns>The func to be used to create the strategy object.</returns>
        internal static Func<object, IChangeTrackingStrategy> GetSnapshotChangeTrackingStrategyFunc()
        {
            return (object entity) => SnapshotChangeTrackingStrategy.Instance;
        }

        /// <summary>
        /// Returns a func that will create a EntityWithKeyStrategy object for a given entity.
        /// </summary>
        /// <returns>The func to be used to create the strategy object.</returns>
        internal static Func<object, IEntityKeyStrategy> GetEntityWithKeyStrategyStrategyFunc()
        {
            return (object entity) => new EntityWithKeyStrategy((IEntityWithKey)entity);
        }

        /// <summary>
        /// Returns a func that will create a GetPocoEntityKeyStrategyFunc object for a given entity.
        /// </summary>
        /// <returns>The func to be used to create the strategy object.</returns>
        internal static Func<object, IEntityKeyStrategy> GetPocoEntityKeyStrategyFunc()
        {
            return (object entity) => new PocoEntityKeyStrategy();
        }
    }
}
