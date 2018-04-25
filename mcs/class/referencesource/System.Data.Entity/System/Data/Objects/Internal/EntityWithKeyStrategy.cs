//---------------------------------------------------------------------
// <copyright file="EntityWithKeyStrategy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Diagnostics;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// Implementor of IEntityKeyStrategy for entities that implement IEntityWithKey.  Getting and setting
    /// the key is deferred to the entity itself.
    /// </summary>
    internal sealed class EntityWithKeyStrategy : IEntityKeyStrategy
    {
        private IEntityWithKey _entity;

        /// <summary>
        /// Creates a strategy object for the given entity.  Keys will be stored in the entity.
        /// </summary>
        /// <param name="entity">The entity to use</param>
        public EntityWithKeyStrategy(IEntityWithKey entity)
        {
            _entity = entity;
        }

        // See IEntityKeyStrategy
        public EntityKey GetEntityKey()
        {
            return _entity.EntityKey;
        }

        // See IEntityKeyStrategy
        public void SetEntityKey(EntityKey key)
        {
            _entity.EntityKey = key;
        }

        // See IEntityKeyStrategy
        public EntityKey GetEntityKeyFromEntity()
        {
            return _entity.EntityKey;
        }
    }
}
