//---------------------------------------------------------------------
// <copyright file="EntityWithChangeTrackerStrategy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Objects.DataClasses;
using System.Diagnostics;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// Implementation of the change tracking strategy for entities that support change trackers.
    /// These are typically entities that implement IEntityWithChangeTracker.
    /// </summary>
    internal sealed class EntityWithChangeTrackerStrategy : IChangeTrackingStrategy
    {
        private IEntityWithChangeTracker _entity;

        /// <summary>
        /// Constructs a strategy object that will cause the change tracker to be set onto the
        /// given object.
        /// </summary>
        /// <param name="entity">The object onto which a change tracker will be set</param>
        public EntityWithChangeTrackerStrategy(IEntityWithChangeTracker entity)
        {
            _entity = entity;
        }

        // See IChangeTrackingStrategy documentation
        public void SetChangeTracker(IEntityChangeTracker changeTracker)
        {
            _entity.SetChangeTracker(changeTracker);
        }

        // See IChangeTrackingStrategy documentation
        public void TakeSnapshot(EntityEntry entry)
        {
            if (entry != null && entry.RequiresComplexChangeTracking)
            {
                entry.TakeSnapshot(true);
            }
        }

        // See IChangeTrackingStrategy documentation
        public void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value)
        {
            member.SetValue(target, value);
        }

        // See IChangeTrackingStrategy documentation
        public void UpdateCurrentValueRecord(object value, EntityEntry entry)
        {
            // Has change tracker, but may or may not be a proxy
            bool isProxy = entry.WrappedEntity.IdentityType != _entity.GetType();
            entry.UpdateRecordWithoutSetModified(value, entry.CurrentValues);
            if (isProxy)
            {
                entry.DetectChangesInProperties(true);      // detect only complex property changes
            }
        }
    }
}
