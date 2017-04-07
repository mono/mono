//---------------------------------------------------------------------
// <copyright file="PrimitiveSchema.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Metadata.Edm;
    using System.Linq;
    using System.Xml;

    /// <summary>
    /// The virtual schema for primitive data types
    /// </summary>
    internal class PrimitiveSchema : Schema
    {
        public PrimitiveSchema(SchemaManager schemaManager)
            : base(schemaManager)
        {
            Schema = this;

            DbProviderManifest providerManifest = ProviderManifest;
            if (providerManifest == null)
            {
                AddError(new EdmSchemaError(System.Data.Entity.Strings.FailedToRetrieveProviderManifest,
                                                   (int)ErrorCode.FailedToRetrieveProviderManifest,
                                                   EdmSchemaErrorSeverity.Error));
            }
            else
            {
                IList<PrimitiveType> primitiveTypes = providerManifest.GetStoreTypes();

                // EDM Spatial types are only available to V3 and above CSDL.
                if (schemaManager.DataModel == SchemaDataModelOption.EntityDataModel &&
                    schemaManager.SchemaVersion < XmlConstants.EdmVersionForV3)
                {
                    primitiveTypes = primitiveTypes.Where(t => !Helper.IsSpatialType(t))
                                                   .ToList();
                }

                foreach (PrimitiveType entry in primitiveTypes)
                {
                    TryAddType(new ScalarType(this, entry.Name, entry), false /*doNotAddErrorForEmptyName*/);
                }
            }
        }

        /// <summary>
        /// Returns the alias that can be used for type in this 
        /// Namespace instead of the entire namespace name
        /// </summary>
        internal override string Alias
        {
            get
            {
                return ProviderManifest.NamespaceName;
            }
        }

        /// <summary>
        /// Returns the TypeAuthority that is driving this schema
        /// </summary>
        internal override string Namespace
        {
            get
            {
                if (ProviderManifest != null)
                {
                    return ProviderManifest.NamespaceName;
                }
                return string.Empty;
            }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            // don't call the base, we don't have any attributes
            return false;
        }
    }
}
