//---------------------------------------------------------------------
// <copyright file="SnapshotChangeTrackingStrategy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Objects.DataClasses;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// Implementation of the change tracking strategy for entities that require snapshot change tracking.
    /// These are typically entities that do not implement IEntityWithChangeTracker.
    /// </summary>
    internal sealed class SnapshotChangeTrackingStrategy : IChangeTrackingStrategy
    {
        private static SnapshotChangeTrackingStrategy _instance = new SnapshotChangeTrackingStrategy();

        /// <summary>
        /// Returns the single static instance of this class; a single instance is all that is needed
        /// because the class is stateless.
        /// </summary>
        public static SnapshotChangeTrackingStrategy Instance
        {
            get
            {
                return _instance;
            }
        }

        // Private constructor to help prevent additional instances being created.
        private SnapshotChangeTrackingStrategy()
        {
        }

        // See IChangeTrackingStrategy documentation
        public void SetChangeTracker(IEntityChangeTracker changeTracker)
        {
            // Nothing to do when using snapshots for change tracking
        }

        // See IChangeTrackingStrategy documentation
        public void TakeSnapshot(EntityEntry entry)
        {
            if (entry != null)
            {
                entry.TakeSnapshot(false);
            }
        }

        // See IChangeTrackingStrategy documentation
        public void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value)
        {
            // If the target is the entity, then this is a change to a member on the entity itself rather than
            // a change to some complex type property defined on the entity.  In this case we can use the change tracking
            // API in the normal way.
            if (Object.ReferenceEquals(target, entry.Entity))
            {
                // equivalent of EntityObject.ReportPropertyChanging()
                ((IEntityChangeTracker)entry).EntityMemberChanging(member.CLayerName);
                member.SetValue(target, value);
                // equivalent of EntityObject.ReportPropertyChanged()
                ((IEntityChangeTracker)entry).EntityMemberChanged(member.CLayerName);

                if (member.IsComplex)
                {
                    // This is required because the OSE contains a separate cache of user objects for
                    // complex objects such that original values can be looked up correctly.
                    entry.UpdateComplexObjectSnapshot(member, target, ordinal, value);
                }
            }
            else
            {
                // Must be a complex type.  We would like to do this:
                // ((IEntityChangeTracker)entry).EntityComplexMemberChanging(topLevelMember.CLayerName, target, member.CLayerName);
                // ((IEntityChangeTracker)entry).EntityComplexMemberChanged(topLevelMember.CLayerName, target, member.CLayerName);
                //
                // However, we have no way of getting the topLevelMember.CLayerName.  This is because the value record does not
                // contain any reference to its parent.  (In non-POCO, ComplexObject takes care of this.)
                // Therefore, in this case we are going to just call a localized DetectChanges to make sure that changes in the
                // complex types are found.
                //
                // Note that this case only happens when the entity is POCO and complex types are set through the CurrentValues
                // object.  This is probably not a very common pattern.
                member.SetValue(target, value);
                if (entry.State != EntityState.Added)
                {
                    // Entry is not Detached - checked in ValidateState() in EntityEntry.SetCurrentEntityValue
                    entry.DetectChangesInProperties(true);
                }
            }
        }

        // See IChangeTrackingStrategy documentation
        public void UpdateCurrentValueRecord(object value, EntityEntry entry)
        {
            // No change tracker, but may or may not be a proxy
            entry.UpdateRecordWithoutSetModified(value, entry.CurrentValues);
            entry.DetectChangesInProperties(false);
        }
    }
}
