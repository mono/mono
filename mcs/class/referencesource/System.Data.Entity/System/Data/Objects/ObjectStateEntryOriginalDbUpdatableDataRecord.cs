//---------------------------------------------------------------------
// <copyright file="ObjectStateEntryOriginalDbUpdatableDataRecord.cs" company="Microsoft">
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
using System.Diagnostics;
using System.Reflection;

namespace System.Data.Objects
{
    // Internal version of writeable original values record is used by all internal operations that need to set original values, such as PreserveChanges queries
    // This version should never be returned to the user, because it doesn't enforce any necessary restrictions.
    // See ObjectStateEntryOriginalDbUpdatableDataRecord_Public for user scenarios.
    internal class ObjectStateEntryOriginalDbUpdatableDataRecord_Internal : OriginalValueRecord
    {
        internal ObjectStateEntryOriginalDbUpdatableDataRecord_Internal(EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject)
            : base(cacheEntry, metadata, userObject)
        {
            EntityUtil.CheckArgumentNull(cacheEntry, "cacheEntry");
            EntityUtil.CheckArgumentNull(userObject, "userObject");
            EntityUtil.CheckArgumentNull(metadata, "metadata");
            Debug.Assert(!cacheEntry.IsKeyEntry, "Cannot create an ObjectStateEntryOriginalDbUpdatableDataRecord_Internal for a key entry");
            switch (cacheEntry.State)
            {
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Deleted:
                    break;
                default:
                    Debug.Assert(false, "An OriginalValueRecord cannot be created for an object in an added or detached state.");
                    break;
            }
        }
        protected override object GetRecordValue(int ordinal)
        {
            Debug.Assert(!_cacheEntry.IsRelationship, "should not be relationship");
            return (_cacheEntry as EntityEntry).GetOriginalEntityValue(_metadata, ordinal, _userObject, ObjectStateValueRecord.OriginalUpdatableInternal);
        }
        protected override void SetRecordValue(int ordinal, object value)
        {
            Debug.Assert(!_cacheEntry.IsRelationship, "should not be relationship");
            (_cacheEntry as EntityEntry).SetOriginalEntityValue(_metadata, ordinal, _userObject, value);
        }
    }

    // Public version of writable original values record that is to be returned to the user for setting original values directly.
    // Although this class is actually internal, it is the version that implements the writeable original values functionality returned through the public surface.
    // This version must maintain information about the index of the top-level entity property that corresponds to this record, because the record
    // may represent a complex type somewhere in an entity hierarchy and this is the only way we know which entity property it is associated with.
    // This version also does minimal necessary validation on the values that the user is trying to set.
    internal sealed class ObjectStateEntryOriginalDbUpdatableDataRecord_Public : ObjectStateEntryOriginalDbUpdatableDataRecord_Internal
    {
        // Will be EntityEntry.s_EntityRoot for entities and for complex types will be the index of the top-level entity property related to this complex type
        int _parentEntityPropertyIndex;

        internal ObjectStateEntryOriginalDbUpdatableDataRecord_Public(EntityEntry cacheEntry, StateManagerTypeMetadata metadata, object userObject, int parentEntityPropertyIndex)
            : base(cacheEntry, metadata, userObject)
        {
            _parentEntityPropertyIndex = parentEntityPropertyIndex;
        }

        protected override object GetRecordValue(int ordinal)
        {
            Debug.Assert(!_cacheEntry.IsRelationship, "should not be relationship");
            return (_cacheEntry as EntityEntry).GetOriginalEntityValue(_metadata, ordinal, _userObject, ObjectStateValueRecord.OriginalUpdatablePublic, GetPropertyIndex(ordinal));
        }

        protected override void SetRecordValue(int ordinal, object value)
        {
            StateManagerMemberMetadata member = _metadata.Member(ordinal);

            // We do not allow setting complex properties through writeable original values.
            // Instead individual scalar properties can be set on a data record that represents the complex type.
            if (member.IsComplex)
            {
                throw EntityUtil.SetOriginalComplexProperties(member.CLayerName);
            }

            // Null values are represented in data records as DBNull.Value, so translate appropriately
            object fieldValue = value ?? DBNull.Value;

            EntityEntry entry = _cacheEntry as EntityEntry;
            EntityState oldState = entry.State;

            // Only update the original values if the new value is different from the value currently set on the entity
            if (entry.HasRecordValueChanged(this, ordinal, fieldValue))
            {
                // Since the original value is going to be set, validate that is doesn't violate any restrictions

                // Throw if trying to change the original value of the primary key
                if (member.IsPartOfKey)
                {
                    throw EntityUtil.SetOriginalPrimaryKey(member.CLayerName);
                }

                // Verify non-nullable EDM members are not being set to null
                // Need to continue allowing CLR reference types to be set to null for backwards compatibility
                Type memberClrType = member.ClrType;
                if ((object)DBNull.Value == fieldValue &&
                    memberClrType.IsValueType &&
                    !member.CdmMetadata.Nullable)
                {
                    // Throw if the underlying CLR type of this property is not nullable, and it is being set to null
                    throw EntityUtil.NullOriginalValueForNonNullableProperty(member.CLayerName, member.ClrMetadata.Name, member.ClrMetadata.DeclaringType.FullName);
                }

                base.SetRecordValue(ordinal, value);

                // Update the state of the ObjectStateEntry if it has been marked as Modified
                if (oldState == EntityState.Unchanged && entry.State == EntityState.Modified)
                {
                    entry.ObjectStateManager.ChangeState(entry, oldState, EntityState.Modified);
                }

                // Set the individual property to modified
                entry.SetModifiedPropertyInternal(GetPropertyIndex(ordinal));
            }
        }

        // For entities the property index is the specified ordinal, but otherwise it's the top-level entity property index that we have saved
        private int GetPropertyIndex(int ordinal)
        {
            return _parentEntityPropertyIndex == EntityEntry.s_EntityRoot ? ordinal : _parentEntityPropertyIndex;
        }
    }
}
