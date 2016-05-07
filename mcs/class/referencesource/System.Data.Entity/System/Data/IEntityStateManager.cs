//---------------------------------------------------------------------
// <copyright file="IEntityStateManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       dsimmons
// @backupOwner amirhmy
//---------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Objects;
using System.Data.Metadata.Edm;
using System.Collections.Generic;

namespace System.Data
{
    /// <summary>
    /// Interface allowing an IEntityAdapter to analyze state/change tracking information maintained
    /// by a state manager in order to perform updates on a backing store (and push back the results
    /// of those updates).
    /// </summary>
    internal interface IEntityStateManager {
        IEnumerable<IEntityStateEntry> GetEntityStateEntries(EntityState state);
        IEnumerable<IEntityStateEntry> FindRelationshipsByKey(EntityKey key);
        IEntityStateEntry GetEntityStateEntry(EntityKey key);
        bool TryGetEntityStateEntry(EntityKey key, out IEntityStateEntry stateEntry);
        bool TryGetReferenceKey(EntityKey dependentKey, AssociationEndMember principalRole, out EntityKey principalKey);
    }
}
