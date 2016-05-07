//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceEntitySetNameConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design.WebControls
{
    internal class EntityDataSourceEntitySetNameConverter : StringConverter
    {
        public EntityDataSourceEntitySetNameConverter()
            : base()
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            // We can only get a list of possible EntitySetName values if we have:
            //    (1) Connection string so we can load metadata
            //    (2) DefaultContainerName to give scope to the lookup
            // Even if these values are set, it may not be possible to actually find them in metadata, but at least we can try the lookup if requested

            EntityDataSource entityDataSource = context.Instance as EntityDataSource;
            if (entityDataSource != null &&
                    !String.IsNullOrEmpty(entityDataSource.ConnectionString) &&
                    !String.IsNullOrEmpty(entityDataSource.DefaultContainerName))
            {
                List<EntityDataSourceEntitySetNameItem> entitySetNameItems = new EntityDataSourceDesignerHelper(entityDataSource, false /*interactiveMode*/).GetEntitySets(entityDataSource.DefaultContainerName);
                string[] entitySetNames = new string[entitySetNameItems.Count];
                for (int i = 0; i < entitySetNameItems.Count; i++)
                {
                    entitySetNames[i] = entitySetNameItems[i].EntitySetName;
                }
                return new StandardValuesCollection(entitySetNames);
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
