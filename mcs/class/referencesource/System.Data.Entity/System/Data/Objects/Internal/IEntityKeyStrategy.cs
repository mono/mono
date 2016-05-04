//---------------------------------------------------------------------
// <copyright file="IEntityKeyStrategy.cs" company="Microsoft">
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
    /// A strategy interface that defines methods used for setting and getting EntityKey values on an entity.
    /// Implementors of this interface are used by the EntityWrapper class.
    /// </summary>
    internal interface IEntityKeyStrategy
    {
        /// <summary>
        /// Gets the entity key.
        /// </summary>
        /// <returns>The key</returns>
        EntityKey GetEntityKey();

        /// <summary>
        /// Sets the entity key
        /// </summary>
        /// <param name="key">The key</param>
        void SetEntityKey(EntityKey key);

        /// <summary>
        /// Returns the entity key directly from the entity
        /// </summary>
        /// <returns>the key</returns>
        EntityKey GetEntityKeyFromEntity();
    }
}
