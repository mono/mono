//---------------------------------------------------------------------
// <copyright file="EntitySet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Data.Common.Utils;
using System.Collections.ObjectModel;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// concrete Class for representing a entity set
    /// </summary>
    public class EntitySet : EntitySetBase
    {
        #region Constructors
        /// <summary>
        /// The constructor for constructing the EntitySet with a given name and an entity type
        /// </summary>
        /// <param name="name">The name of the EntitySet</param>
        /// <param name="schema">The db schema</param>
        /// <param name="table">The db table</param>
        /// <param name="definingQuery">The provider specific query that should be used to retrieve the EntitySet</param>
        /// <param name="entityType">The entity type of the entities that this entity set type contains</param> 
        /// <exception cref="System.ArgumentNullException">Thrown if the argument name or entityType is null</exception>
        internal EntitySet(string name, string schema, string table, string definingQuery, EntityType entityType)
            : base(name, schema, table, definingQuery, entityType)
        {
        }
        #endregion

        #region Fields
        private ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> _foreignKeyDependents;
        private ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> _foreignKeyPrincipals;
        private volatile bool _hasForeignKeyRelationships;
        private volatile bool _hasIndependentRelationships;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.EntitySet; } }

        /// <summary>
        /// Gets/Sets the entity type of this entity set
        /// </summary>
        public new EntityType ElementType
        {
            get
            {
                return (EntityType)base.ElementType;
            }
        }

        /// <summary>
        /// Returns the associations and constraints where "this" EntitySet particpates as the Principal end. 
        /// From the results of this list, you can retrieve the Dependent IRelatedEnds
        /// </summary>
        internal ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> ForeignKeyDependents
        {
            get
            {
                if (_foreignKeyDependents == null)
                {
                    InitializeForeignKeyLists();
                }
                return _foreignKeyDependents;
            }
        }

        /// <summary>
        /// Returns the associations and constraints where "this" EntitySet particpates as the Dependent end. 
        /// From the results of this list, you can retrieve the Principal IRelatedEnds
        /// </summary>
        internal ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> ForeignKeyPrincipals
        {
            get
            {
                if (_foreignKeyPrincipals == null)
                {
                    InitializeForeignKeyLists();
                }
                return _foreignKeyPrincipals;
            }
        }

        /// <summary>
        /// True if this entity set participates in any foreign key relationships, otherwise false.
        /// </summary>
        internal bool HasForeignKeyRelationships
        {
            get
            {
                if (_foreignKeyPrincipals == null)
                {
                    InitializeForeignKeyLists();
                }
                return _hasForeignKeyRelationships;
            }
        }

        /// <summary>
        /// True if this entity set participates in any independent relationships, otherwise false.
        /// </summary>
        internal bool HasIndependentRelationships
        {
            get
            {
                if (_foreignKeyPrincipals == null)
                {
                    InitializeForeignKeyLists();
                }
                return _hasIndependentRelationships;
            }
        }

        #endregion

        #region Methods
        private void InitializeForeignKeyLists()
        {
            var dependents = new List<Tuple<AssociationSet, ReferentialConstraint>>();
            var principals = new List<Tuple<AssociationSet, ReferentialConstraint>>();
            bool foundFkRelationship = false;
            bool foundIndependentRelationship = false;
            foreach (AssociationSet associationSet in MetadataHelper.GetAssociationsForEntitySet(this))
            {
                if (associationSet.ElementType.IsForeignKey)
                {
                    foundFkRelationship = true;
                    Debug.Assert(associationSet.ElementType.ReferentialConstraints.Count == 1, "Expected exactly one constraint for FK");
                    ReferentialConstraint constraint = associationSet.ElementType.ReferentialConstraints[0];
                    if (constraint.ToRole.GetEntityType().IsAssignableFrom(this.ElementType) ||
                        this.ElementType.IsAssignableFrom(constraint.ToRole.GetEntityType()))
                    {
                        // Dependents
                        dependents.Add(new Tuple<AssociationSet, ReferentialConstraint>(associationSet, constraint));
                    }
                    if (constraint.FromRole.GetEntityType().IsAssignableFrom(this.ElementType) ||
                        this.ElementType.IsAssignableFrom(constraint.FromRole.GetEntityType()))
                    {
                        // Principals
                        principals.Add(new Tuple<AssociationSet, ReferentialConstraint>(associationSet, constraint));
                    }
                }
                else
                {
                    foundIndependentRelationship = true;
                }
            }

            _hasForeignKeyRelationships = foundFkRelationship;
            _hasIndependentRelationships = foundIndependentRelationship;

            var readOnlyDependents = dependents.AsReadOnly();
            var readOnlyPrincipals = principals.AsReadOnly();

            Interlocked.CompareExchange(ref _foreignKeyDependents, readOnlyDependents, null);
            Interlocked.CompareExchange(ref _foreignKeyPrincipals, readOnlyPrincipals, null);
        }
        #endregion
    }
}
