//---------------------------------------------------------------------
// <copyright file="ObjectStateEntryDbUpdatableDataRecord.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Diagnostics;
using System.Reflection;

namespace System.Data.Objects
{
    internal sealed class ObjectStateEntryDbUpdatableDataRecord : CurrentValueRecord
    {
        internal ObjectStateEntryDbUpdatableDataRecord(EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
            : base(cacheEntry, metadata, userObject)
        {
            EntityUtil.CheckArgumentNull(cacheEntry, "cacheEntry");
            EntityUtil.CheckArgumentNull(userObject, "userObject");
            EntityUtil.CheckArgumentNull(metadata, "metadata");
            Debug.Assert(!cacheEntry.IsKeyEntry, "Cannot create an ObjectStateEntryDbUpdatableDataRecord for a key entry");
            switch (cacheEntry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Added:
                    break;
                default:
                    Debug.Assert(false, "A CurrentValueRecord cannot be created for an entity object that is in a deleted or detached state.");
                    break;
            }
        }
        internal ObjectStateEntryDbUpdatableDataRecord(RelationshipEntry cacheEntry)
            : base(cacheEntry)
        {
            EntityUtil.CheckArgumentNull(cacheEntry, "cacheEntry");
            switch (cacheEntry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Added:
                    break;
                default:
                    Debug.Assert(false, "A CurrentValueRecord cannot be created for an entity object that is in a deleted or detached state.");
                    break;
            }
        }
        protected override object GetRecordValue(int ordinal)
        {
            if (_cacheEntry.IsRelationship)
            {
                return (_cacheEntry as RelationshipEntry).GetCurrentRelationValue(ordinal);
            }
            else
            {
                return (_cacheEntry as EntityEntry).GetCurrentEntityValue(_metadata, ordinal, _userObject, ObjectStateValueRecord.CurrentUpdatable);
            }
        }
        protected override void SetRecordValue(int ordinal, object value)
        {
            if (_cacheEntry.IsRelationship)
            {
                // Cannot modify relation values from the public API
                throw EntityUtil.CantModifyRelationValues();
            }
            else
            {
                (_cacheEntry as EntityEntry).SetCurrentEntityValue(_metadata, ordinal, _userObject, value);
            }
        }
    }
}
