//---------------------------------------------------------------------
// <copyright file="EntitySetBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class for representing a entity set
    /// </summary>
    public abstract class EntitySetBase : MetadataItem
    {
        //----------------------------------------------------------------------------------------------
        // Possible Future Enhancement: revisit factoring of EntitySetBase and delta between C constructs and S constructs
        //
        // Currently, we need to have a way to map an entityset or a relationship set in S space
        // to the appropriate structures in the store. In order to address this we said we would
        // add new ItemAttributes (tableName, schemaName and catalogName to the EntitySetBase)... 
        // problem with this is that we are bleading a leaf-level, store specific set of constructs
        // into the object model for things that may exist at either C or S. 
        //
        // We need to do this for now to push forward on enabling the conversion but we need to re-examine
        // whether we should have separate C and S space constructs or some other mechanism for 
        // maintaining this metadata.
        //----------------------------------------------------------------------------------------------

        #region Constructors
        /// <summary>
        /// The constructor for constructing the EntitySet with a given name and an entity type
        /// </summary>
        /// <param name="name">The name of the EntitySet</param>
        /// <param name="schema">The db schema</param>
        /// <param name="table">The db table</param>
        /// <param name="definingQuery">The provider specific query that should be used to retrieve the EntitySet</param>
        /// <param name="entityType">The entity type of the entities that this entity set type contains</param>        
        /// <exception cref="System.ArgumentNullException">Thrown if the name or entityType argument is null</exception>
        internal EntitySetBase(string name, string schema, string table, string definingQuery, EntityTypeBase entityType)
        {
            EntityUtil.GenericCheckArgumentNull(entityType, "entityType");
            EntityUtil.CheckStringArgument(name, "name");
            // SQLBU 480236: catalogName, schemaName & tableName are allowed to be null, empty & non-empty

            _name = name;

            //---- name of the 'schema'
            //---- this is used by the SQL Gen utility to support generation of the correct name in the store
            _schema = schema;

            //---- name of the 'table'
            //---- this is used by the SQL Gen utility to support generation of the correct name in the store
            _table = table;

            //---- the Provider specific query to use to retrieve the EntitySet data
            _definingQuery = definingQuery;
            
            this.ElementType = entityType;
        }
        #endregion

        #region Fields
        private EntityContainer _entityContainer;
        private string _name;
        private EntityTypeBase _elementType;
        private string _table;
        private string _schema;
        private string _definingQuery;
        private string _cachedProviderSql;

        #endregion

        #region Properties
        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.EntitySetBase; } }


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
        /// Gets or sets escaped SQL describing this entity set.
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // referenced by System.Data.Entity.Design.dll
        internal string DefiningQuery
        {
            get { return _definingQuery; }
            set { _definingQuery = value; }
        }

        /// <summary>
        /// Get and set by the provider only as a convientent place to 
        /// store the created sql fragment that represetnts this entity set
        /// </summary>
        internal string CachedProviderSql
        {
            get { return _cachedProviderSql; }
            set { _cachedProviderSql = value; }
        }

        /// <summary>
        /// Gets/Sets the name of this entity set
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown if value passed into setter is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called when EntitySetBase instance is in ReadOnly state</exception>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public String Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Returns the entity container of the entity set
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown if value passed into setter is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called when the EntitySetBase instance or the EntityContainer passed into the setter is in ReadOnly state</exception>
        public EntityContainer EntityContainer
        {
            get
            {
                return _entityContainer;
            }
        }

        /// <summary>
        /// Gets/Sets the entity type of this entity set
        /// </summary>
        /// <exception cref="System.ArgumentNullException">if value passed into setter is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called when EntitySetBase instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.EntityTypeBase, false)]
        public EntityTypeBase ElementType
        {
            get
            {
                return _elementType;
            }
            internal set
            {
                EntityUtil.GenericCheckArgumentNull(value, "value");
                Util.ThrowIfReadOnly(this);
                _elementType = value;
            }
        }

        [MetadataProperty(PrimitiveTypeKind.String, false)]
        internal string Table
        {
            get
            {
                return _table;
            }
        }

        [MetadataProperty(PrimitiveTypeKind.String, false)]
        internal string Schema
        {
            get
            {
                return _schema;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Overriding System.Object.ToString to provide better String representation 
        /// for this type.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Sets this item to be readonly, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!this.IsReadOnly)
            {
                base.SetReadOnly();

                EntityTypeBase elementType = ElementType;
                if (elementType != null)
                {
                    elementType.SetReadOnly();
                }
            }
        }

        /// <summary>
        /// Change the entity container without doing fixup in the entity set collection
        /// </summary>
        internal void ChangeEntityContainerWithoutCollectionFixup(EntityContainer newEntityContainer)
        {
            _entityContainer = newEntityContainer;
        }
        #endregion
    }
}
