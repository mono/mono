//---------------------------------------------------------------------
// <copyright file="IEntityStateEntry.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       dsimmons
// @backupOwner amirhmy
//---------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Common;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.Metadata.Edm;
using System.Collections.Generic;
using System.Collections;

namespace System.Data
{
    /// <summary>
    /// This is the interface to a particular entry in an IEntityStateManager.  It provides
    /// information about the state of the entity in question and the ability to modify that state
    /// as appropriate for an entity adapter to function in performing updates to a backing store.
    /// </summary>
    internal interface IEntityStateEntry
    {
        IEntityStateManager StateManager { get; }
        EntityKey EntityKey { get; }
        EntitySetBase EntitySet { get; }
        bool IsRelationship { get; }
        bool IsKeyEntry { get; }
        EntityState State { get; }
        DbDataRecord OriginalValues { get; }
        CurrentValueRecord CurrentValues { get; }
        BitArray ModifiedProperties { get; }

        void AcceptChanges();
        void Delete();
        void SetModified();
        void SetModifiedProperty(string propertyName);
        IEnumerable<string> GetModifiedProperties();
    }
}
