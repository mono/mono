//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceEntityTypeFilterConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design.WebControls
{
    internal class EntityDataSourceEntityTypeFilterConverter : StringConverter
    {
        public EntityDataSourceEntityTypeFilterConverter()
            : base()
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            // We can only get a list of possible EntityTypeFilter values if we have:
            //    (1) Connection string so we can load metadata
            //    (2) DefaultContainerName to give scope to the lookup
            //    (3) EntitySetName that exists in DefaultContainerName so we can get its type and derived types 
            // Even if these values are set, it may not be possible to actually find them in metadata, but at least we can try the lookup if requested

            EntityDataSource entityDataSource = context.Instance as EntityDataSource;
            if (entityDataSource != null &&
                !String.IsNullOrEmpty(entityDataSource.ConnectionString) &&
                !String.IsNullOrEmpty(entityDataSource.DefaultContainerName) &&
                !String.IsNullOrEmpty(entityDataSource.EntitySetName))
            {
                List<EntityDataSourceEntityTypeFilterItem> entityTypeFilterItems =
                    new EntityDataSourceDesignerHelper(entityDataSource, false /*interactiveMode*/).GetEntityTypeFilters(
                        entityDataSource.DefaultContainerName, entityDataSource.EntitySetName);

                string[] entityTypeFilters = new string[entityTypeFilterItems.Count];
                for (int i = 0; i < entityTypeFilterItems.Count; i++)
                {
                    entityTypeFilters[i] = entityTypeFilterItems[i].EntityTypeName;
                }
                return new StandardValuesCollection(entityTypeFilters);
            }

            return null;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
