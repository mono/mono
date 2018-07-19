//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceContainerNameConverter.cs" company="Microsoft">
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
    internal class EntityDataSourceContainerNameConverter : StringConverter
    {
        
        public EntityDataSourceContainerNameConverter()
            : base()
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            // We can only get a list of possible DefaultContainerName values if we have:
            //    (1) Connection string so we can load metadata
            // Even if this value is set, it may not be possible to actually load the metadata, but at least we can try the lookup if requested

            EntityDataSource entityDataSource = context.Instance as EntityDataSource;
            if (entityDataSource != null && !String.IsNullOrEmpty(entityDataSource.ConnectionString))
            {
                List<EntityDataSourceContainerNameItem> containerNameItems = new EntityDataSourceDesignerHelper(entityDataSource, false /*interactiveMode*/).GetContainerNames(true /*sortResults*/);
                string[] containers = new string[containerNameItems.Count];
                for (int i = 0; i < containerNameItems.Count; i++)
                {
                    containers[i] = containerNameItems[i].ToString();
                }
                return new StandardValuesCollection(containers);                
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
