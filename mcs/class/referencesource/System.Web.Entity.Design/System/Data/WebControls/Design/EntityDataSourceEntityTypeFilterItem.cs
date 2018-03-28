//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceEntityTypeFilterItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Data.Metadata.Edm;

namespace System.Web.UI.Design.WebControls
{

    internal class EntityDataSourceEntityTypeFilterItem : IComparable<EntityDataSourceEntityTypeFilterItem>
    {
        // Only one of the following should be set. This is enforced through the constructors and the fact that these fields are readonly.
        private readonly EntityType _entityType; // used when we have a real EntityType backing this item
        private readonly string _unknownEntityTypeName; // used when we have an unknown EntityTypeFilter that we still want to include in the list

        internal EntityDataSourceEntityTypeFilterItem(EntityType entityType)
        {
            _entityType = entityType;
        }

        internal EntityDataSourceEntityTypeFilterItem(string unknownEntityTypeName)
        {
            _unknownEntityTypeName = unknownEntityTypeName;
        }

        internal string EntityTypeName
        {
            get
            {
                if (_entityType != null)
                {
                    return _entityType.Name;
                }
                else
                {
                    return _unknownEntityTypeName;
                }
            }
        }

        internal EntityType EntityType
        {
            get
            {
                return _entityType;
            }
        }

        public override string ToString()
        {
            return EntityTypeName;
        }

        int IComparable<EntityDataSourceEntityTypeFilterItem>.CompareTo(EntityDataSourceEntityTypeFilterItem other)
        {
            return (String.Compare(this.EntityTypeName, other.EntityTypeName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
