//---------------------------------------------------------------------
// <copyright file="EntityStoreSchemaFilterEntry.cs" company="Microsoft">
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
using System.Data.Entity.Design.Common;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// This class represent a single filter entry
    /// </summary>
    public class EntityStoreSchemaFilterEntry
    {
        private string _catalog;
        private string _schema;
        private string _name;
        private EntityStoreSchemaFilterObjectTypes _types;
        private EntityStoreSchemaFilterEffect _effect;

        /// <summary>
        /// Creates a EntityStoreSchemaFilterEntry
        /// </summary>
        /// <param name="catalog">The pattern to use to select the appropriate catalog or null to not limit by catalog.</param>
        /// <param name="schema">The pattern to use to select the appropriate schema or null to not limit by schema.</param>
        /// <param name="name">The pattern to use to select the appropriate name or null to not limit by name.</param>
        /// <param name="types">The type of objects to apply this filter to.</param>
        /// <param name="effect">The effect that this filter should have on the results.</param>
        public EntityStoreSchemaFilterEntry(string catalog, string schema, string name, EntityStoreSchemaFilterObjectTypes types, EntityStoreSchemaFilterEffect effect)
        {
            if (types == EntityStoreSchemaFilterObjectTypes.None)
            {
                throw EDesignUtil.Argument("types");
            }
            _catalog = catalog;
            _schema = schema;
            _name = name;
            _types = types;
            _effect = effect;
        }

        /// <summary>
        /// Creates a EntityStoreSchemaFilterEntry
        /// </summary>
        /// <param name="catalog">The pattern to use to select the appropriate catalog or null to not limit by catalog.</param>
        /// <param name="schema">The pattern to use to select the appropriate schema or null to not limit by schema.</param>
        /// <param name="name">The pattern to use to select the appropriate name or null to not limit by name.</param>
        public EntityStoreSchemaFilterEntry(string catalog, string schema, string name)
            :this(catalog, schema, name, EntityStoreSchemaFilterObjectTypes.All, EntityStoreSchemaFilterEffect.Allow)
        {
        }

        /// <summary>
        /// Gets the pattern that will be used to select the appropriate catalog.
        /// </summary>
        public string Catalog
        {
            [DebuggerStepThroughAttribute]
            get { return _catalog; }
        }

        /// <summary>
        /// Gets the pattern that will be used to select the appropriate schema.
        /// </summary>
        public string Schema
        {
            [DebuggerStepThroughAttribute]
            get { return _schema; }
        }

        /// <summary>
        /// Gets the pattern that will be used to select the appropriate name.
        /// </summary>
        public string Name
        {
            [DebuggerStepThroughAttribute]
            get { return _name; }
        }

        /// <summary>
        /// Gets the types of objects that this filter applies to.
        /// </summary>
        public EntityStoreSchemaFilterObjectTypes Types
        {
            [DebuggerStepThroughAttribute]
            get { return _types; }
        }

        /// <summary>
        /// Gets the effect that this filter has on results.
        /// </summary>
        public EntityStoreSchemaFilterEffect Effect
        {
            [DebuggerStepThroughAttribute]
            get { return _effect; }
        }
    }
}
