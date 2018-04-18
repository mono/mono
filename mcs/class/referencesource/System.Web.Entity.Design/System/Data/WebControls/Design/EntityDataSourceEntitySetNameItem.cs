//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceEntitySetNameItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Data.Metadata.Edm;

namespace System.Web.UI.Design.WebControls
{

    internal class EntityDataSourceEntitySetNameItem : IComparable<EntityDataSourceEntitySetNameItem>
    {
        // Only one of the following should be set. This is enforced through the constructors and the fact that these fields are readonly.
        private readonly EntitySet _entitySet; // used when we have a real EntitySet backing this item
        private readonly string _unknownEntitySetName; // used when we have an unknown EntitySetName that we still want to include in the list
        
        internal EntityDataSourceEntitySetNameItem(EntitySet entitySet)
        {
            _entitySet = entitySet;            
        }

        internal EntityDataSourceEntitySetNameItem(string unknownEntitySetName)
        {
            _unknownEntitySetName = unknownEntitySetName;
        }

        internal string EntitySetName
        {
            get
            {
                if (_entitySet != null)
                {
                    return _entitySet.Name;
                }
                else
                {
                    return _unknownEntitySetName;
                }

            }
        }

        internal EntitySet EntitySet
        {
            get
            {
                return _entitySet;
            }
        }

        public override string ToString()
        {
            return EntitySetName;
        }

        int IComparable<EntityDataSourceEntitySetNameItem>.CompareTo(EntityDataSourceEntitySetNameItem other)
        {
            return (String.Compare(this.EntitySetName, other.EntitySetName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
