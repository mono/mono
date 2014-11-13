//---------------------------------------------------------------------
// <copyright file="IMember.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Data.Objects.DataClasses;
using System.Collections.Generic;

namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Abstracts the properties of a relationship element
    /// </summary>
    internal interface IRelationship
    {
        /// <summary>
        /// Name of the Relationship
        /// </summary>
        string Name { get; }

        string FQName { get; }

        /// <summary>
        /// The list of ends defined in the Relationship.
        /// </summary>
        IList<IRelationshipEnd> Ends { get; }
        
        /// <summary>
        /// Returns the list of constraints on this relation
        /// </summary>
        IList<ReferentialConstraint> Constraints { get; }

        /// <summary>
        /// Finds an end given the roleName
        /// </summary>
        /// <param name="roleName">The role name of the end you want to find</param>
        /// <param name="end">The relationship end reference to set if the end is found</param>
        /// <returns>True if the end was found, and the passed in reference was set, False otherwise.</returns>
        bool TryGetEnd( string roleName, out IRelationshipEnd end );

        /// <summary>
        /// Is this an Association, or ...
        /// </summary>
        RelationshipKind RelationshipKind { get; }

        /// <summary>
        /// Is this a foreign key (FK) relationship?
        /// </summary>
        bool IsForeignKey { get; }
    }
}
