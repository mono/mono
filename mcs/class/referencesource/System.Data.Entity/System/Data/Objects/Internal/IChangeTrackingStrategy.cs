//------------------------------------------------------------------------------
// <copyright file="IChangeTrackingStrategy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Objects.DataClasses;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// A strategy interface that defines methods used for different types of change tracking.
    /// Implementors of this interface are used by the EntityWrapper class.
    /// </summary>
    internal interface IChangeTrackingStrategy
    {
        /// <summary>
        /// Sets a change tracker onto an entity, or does nothing if the entity does not support change trackers.
        /// </summary>
        /// <param name="changeTracker">The change tracker to set</param>
        void SetChangeTracker(IEntityChangeTracker changeTracker);

        /// <summary>
        /// Takes a snapshot of the entity contained in the given state entry, or does nothing if
        /// snapshots are not required for the entity.
        /// </summary>
        /// <param name="entry">The state entry representing the entity to snapshot</param>
        void TakeSnapshot(EntityEntry entry);

        /// <summary>
        /// Sets the given value onto the entity with the registered change either handled by the
        /// entity itself or by using the given EntityEntry as the change tracker.
        /// </summary>
        /// <param name="entry">The state entry of the entity to for which a value should be set</param>
        /// <param name="member">State member information indicating the member to set</param>
        /// <param name="ordinal">The ordinal of the member to set</param>
        /// <param name="target">The object onto which the value should be set; may be the entity, or a contained complex value</param>
        /// <param name="value">The value to set</param>
        void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value);

        /// <summary>
        /// Updates the current value records using Shaper.UpdateRecord but with additional change tracking logic
        /// added as required by POCO and proxy entities.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="entry">The existing ObjectStateEntry</param>
        void UpdateCurrentValueRecord(object value, EntityEntry entry);
    }
}
