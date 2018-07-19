//------------------------------------------------------------------------------
// <copyright file="PocoEntityKeyStrategy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Diagnostics;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// Implementor of IEntityKeyStrategy for getting and setting a key on an entity that does not
    /// implement IEntityWithKey.  The key is stored in the strategy object.
    /// </summary>
    internal sealed class PocoEntityKeyStrategy : IEntityKeyStrategy
    {
        private EntityKey _key;

        /// <summary>
        /// Creates a new strategy object; no reference to the actual entity is required.
        /// </summary>
        public PocoEntityKeyStrategy()
        {
        }

        // See IEntityKeyStrategy
        public EntityKey GetEntityKey()
        {
            return _key;
        }

        // See IEntityKeyStrategy
        public void SetEntityKey(EntityKey key)
        {
            _key = key;
        }

        // See IEntityKeyStrategy
        public EntityKey GetEntityKeyFromEntity()
        {
            return null;
        }
    }
}
