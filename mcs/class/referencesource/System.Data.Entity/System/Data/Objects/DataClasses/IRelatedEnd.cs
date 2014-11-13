//---------------------------------------------------------------------
// <copyright file="IRelatedEnd.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Data;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Metadata.Edm;

namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// Represents one end of a relationship.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public interface IRelatedEnd
    {
        // ----------
        // Properties
        // ----------

        /// <summary>
        /// IsLoaded returns true if and only if Load was called.
        /// </summary>
        bool IsLoaded { get;}

        /// <summary>
        /// Name of the relationship in which this IRelatedEnd is participating        
        /// </summary>
        string RelationshipName { get; }

        /// <summary>
        /// Name of the relationship source role used to generate this IRelatedEnd
        /// </summary>
        string SourceRoleName { get; }

        /// <summary>
        /// Name of the relationship target role used to generate this IRelatedEnd
        /// </summary>
        string TargetRoleName { get; }

        /// <summary>
        /// The relationship metadata cooresponding to this IRelatedEnd
        /// </summary>
        RelationshipSet RelationshipSet { get; }

        // -------
        // Methods
        // -------

        /// <summary>
        /// Loads the related entity or entities into the related end using the default merge option
        /// </summary>
        void Load();

        /// <summary>
        /// Loads the related entity or entities into the related end using the specified merge option
        /// </summary>
        void Load(MergeOption mergeOption);

        /// <summary>
        ///   Adds an entity to the related end.  If the owner is
        ///   attached to a cache then the all the connected ends are
        ///   added to the object cache and their corresponding relationships 
        ///   are also added to the ObjectStateManager. The RelatedEnd of the
        ///   relationship is also fixed.
        /// </summary>
        /// <param name="entity">
        ///   Entity instance to add to the related end
        /// </param>
        void Add(IEntityWithRelationships entity);

        /// <summary>
        ///   Adds an entity to the related end.  If the owner is
        ///   attached to a cache then the all the connected ends are
        ///   added to the object cache and their corresponding relationships 
        ///   are also added to the ObjectStateManager. The RelatedEnd of the
        ///   relationship is also fixed.
        /// 
        /// This overload is meant to be used by classes that do not implement IEntityWithRelationships.
        /// </summary>
        /// <param name="entity">
        ///   Entity instance to add to the related end
        /// </param>
        void Add(object entity);

        /// <summary>
        ///   Removes an entity from the related end.  If owner is
        ///   attached to a cache, marks relationship for deletion and if
        ///   the relationship is composition also marks the entity for deletion.
        /// </summary>
        /// <param name="entity">
        ///   Entity instance to remove from the related end
        /// </param>
        /// <returns>Returns true if the entity was successfully removed, false if the entity was not part of the IRelatedEnd.</returns>
        bool Remove(IEntityWithRelationships entity);

        /// <summary>
        ///   Removes an entity from the related end.  If owner is
        ///   attached to a cache, marks relationship for deletion and if
        ///   the relationship is composition also marks the entity for deletion.
        /// 
        /// This overload is meant to be used by classes that do not implement IEntityWithRelationships.
        /// </summary>
        /// <param name="entity">
        ///   Entity instance to remove from the related end
        /// </param>
        /// <returns>Returns true if the entity was successfully removed, false if the entity was not part of the IRelatedEnd.</returns>
        bool Remove(object entity);

        /// <summary>
        /// Attaches an entity to the related end. If the related end is already filled
        /// or partially filled, this merges the existing entities with the given entity. The given
        /// entity is not assumed to be the complete set of related entities.
        /// 
        /// Owner and all entities passed in must be in Unchanged or Modified state. 
        /// Deleted elements are allowed only when the state manager is already tracking the relationship
        /// instance.
        /// </summary>
        /// <param name="entity">The entity to attach to the related end</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity cannot be related via the current relationship end.</exception>
        void Attach(IEntityWithRelationships entity);

        /// <summary>
        /// Attaches an entity to the related end. If the related end is already filled
        /// or partially filled, this merges the existing entities with the given entity. The given
        /// entity is not assumed to be the complete set of related entities.
        /// 
        /// Owner and all entities passed in must be in Unchanged or Modified state. 
        /// Deleted elements are allowed only when the state manager is already tracking the relationship
        /// instance.
        /// 
        /// This overload is meant to be used by classes that do not implement IEntityWithRelationships.
        /// </summary>
        /// <param name="entity">The entity to attach to the related end</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity cannot be related via the current relationship end.</exception>
        void Attach(object entity);

        /// <summary>
        /// This is the query which represents the source of the
        /// related end.  It is constructed on demand using the
        /// _connection and _cache fields and a query string based on
        /// the type of relationship end and the metadata passed into its
        /// constructor.
        /// </summary>
        IEnumerable CreateSourceQuery();

        /// <summary>
        /// Returns an enumerator of all of the values contained within this related end
        /// </summary>        
        IEnumerator GetEnumerator();
    }
}


