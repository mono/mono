//---------------------------------------------------------------------
// <copyright file="IRelationshipEnd.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Abstracts the properties of an End element in a relationship
    /// </summary>
    internal interface IRelationshipEnd
    {
        /// <summary>
        /// Name of the End
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Type of the End
        /// </summary>
        SchemaEntityType Type { get; }

        /// <summary>
        /// Multiplicity of the End
        /// </summary>
        System.Data.Metadata.Edm.RelationshipMultiplicity? Multiplicity { get; set; }

        /// <summary>
        /// The On&lt;Operation&gt;s defined for the End
        /// </summary>
        ICollection<OnOperation> Operations { get; }

    }
}
