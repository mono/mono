//---------------------------------------------------------------------
// <copyright file="RelationshipSet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class for representing a relationship set
    /// </summary>
    public abstract class RelationshipSet : EntitySetBase
    {
        #region Constructors
        /// <summary>
        /// The constructor for constructing the RelationshipSet with a given name and an relationship type
        /// </summary>
        /// <param name="name">The name of the RelationshipSet</param>
        /// <param name="schema">The db schema</param>
        /// <param name="table">The db table</param>
        /// <param name="definingQuery">The provider specific query that should be used to retrieve the EntitySet</param>
        /// <param name="relationshipType">The entity type of the entities that this entity set type contains</param> 
        /// <exception cref="System.ArgumentNullException">Thrown if the argument name or entityType is null</exception>
        internal RelationshipSet(string name, string schema, string table, string definingQuery, RelationshipType relationshipType)
            : base(name, schema, table, definingQuery, relationshipType)
        {
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the relationship type associated with this relationship set
        /// </summary>
        public new RelationshipType ElementType
        {
            get
            {
                return (RelationshipType)base.ElementType;
            }
        }

        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.RelationshipSet; } }
        #endregion
    }
}
