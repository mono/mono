//---------------------------------------------------------------------
// <copyright file="EntityContainer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class for representing an entity container
    /// </summary>
    public sealed class EntityContainer : GlobalItem
    {
        #region Constructors
        /// <summary>
        /// The constructor for constructing the EntityContainer object with the name, namespaceName, and version.
        /// </summary>
        /// <param name="name">The name of this entity container</param>
        /// <param name="dataSpace">dataSpace in which this entity container belongs to</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the name argument is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the name argument is empty string</exception>
        internal EntityContainer(string name, DataSpace dataSpace)
        {
            EntityUtil.CheckStringArgument(name, "name");

            _name = name;
            this.DataSpace = dataSpace;
            _baseEntitySets = new ReadOnlyMetadataCollection<EntitySetBase>(new EntitySetBaseCollection(this));
            _functionImports = new ReadOnlyMetadataCollection<EdmFunction>(new MetadataCollection<EdmFunction>());
        }
        #endregion

        #region Fields
        private readonly string _name;
        private readonly ReadOnlyMetadataCollection<EntitySetBase> _baseEntitySets;
        private readonly ReadOnlyMetadataCollection<EdmFunction> _functionImports;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.EntityContainer; } }

        /// <summary>
        /// Gets the identity for this item as a string
        /// </summary>
        internal override string Identity
        {
            get
            {
                return this.Name;
            }
        }

        /// <summary>
        /// Get the name of this EntityContainer object
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public String Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the collection of entity sets
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EntitySetBase, true)]
        public ReadOnlyMetadataCollection<EntitySetBase> BaseEntitySets
        {
            get
            {
                return _baseEntitySets;
            }
        }

        /// <summary>
        /// Gets the collection of function imports for this entity container
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EdmFunction, true)]
        public ReadOnlyMetadataCollection<EdmFunction> FunctionImports
        {
            get
            {
                return _functionImports;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets this item to be readonly, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();
                this.BaseEntitySets.Source.SetReadOnly();
                this.FunctionImports.Source.SetReadOnly();
            }
        }

        /// <summary>
        /// Get the entity set with the given name
        /// </summary>
        /// <param name="name">name of the entity set to look up for</param>
        /// <param name="ignoreCase">true if you want to do a case-insensitive lookup</param>
        /// <returns></returns>
        public EntitySet GetEntitySetByName(string name, bool ignoreCase)
        {
            EntitySet entitySet = (BaseEntitySets.GetValue(name, ignoreCase) as EntitySet);
            if (null != entitySet)
            {
                return entitySet;
            }
            throw EntityUtil.InvalidEntitySetName(name);
        }

        /// <summary>
        /// Get the entity set with the given name or return null if not found
        /// </summary>
        /// <param name="name">name of the entity set to look up for</param>
        /// <param name="ignoreCase">true if you want to do a case-insensitive lookup</param>
        /// <param name="entitySet">out parameter that will contain the result</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if name argument is null</exception>
        public bool TryGetEntitySetByName(string name, bool ignoreCase, out EntitySet entitySet)
        {
            EntityUtil.CheckArgumentNull(name, "name");
            EntitySetBase baseEntitySet = null;
            entitySet = null;
            if (this.BaseEntitySets.TryGetValue(name, ignoreCase, out baseEntitySet))
            {
                if (Helper.IsEntitySet(baseEntitySet))
                {
                    entitySet = (EntitySet)baseEntitySet;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the relationship set with the given name
        /// </summary>
        /// <param name="name">name of the relationship set to look up for</param>
        /// <param name="ignoreCase">true if you want to do a case-insensitive lookup</param>
        /// <returns></returns>
        public RelationshipSet GetRelationshipSetByName(string name, bool ignoreCase)
        {
            RelationshipSet relationshipSet;
            if (!this.TryGetRelationshipSetByName(name, ignoreCase, out relationshipSet))
            {
                throw EntityUtil.InvalidRelationshipSetName(name);
            }
            return relationshipSet;

        }

        /// <summary>
        /// Get the relationship set with the given name
        /// </summary>
        /// <param name="name">name of the relationship set to look up for</param>
        /// <param name="ignoreCase">true if you want to do a case-insensitive lookup</param>
        /// <param name="relationshipSet">out parameter that will have the result</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if name argument is null</exception>
        public bool TryGetRelationshipSetByName(string name, bool ignoreCase, out RelationshipSet relationshipSet)
        {
            EntityUtil.CheckArgumentNull(name, "name");
            EntitySetBase baseEntitySet = null;
            relationshipSet = null;
            if (this.BaseEntitySets.TryGetValue(name, ignoreCase, out baseEntitySet))
            {
                if (Helper.IsRelationshipSet(baseEntitySet))
                {
                    relationshipSet = (RelationshipSet)baseEntitySet;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Overriding System.Object.ToString to provide better String representation 
        /// for this type.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        internal void AddEntitySetBase(EntitySetBase entitySetBase)
        {
            _baseEntitySets.Source.Add(entitySetBase);
        }

        internal void AddFunctionImport(EdmFunction function)
        {
            Debug.Assert(function != null, "function != null");
            Debug.Assert(function.IsFunctionImport, "function.IsFunctionImport");
            _functionImports.Source.Add(function);
        }
        #endregion
    }
}
