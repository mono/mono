//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceContainerNameItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

using System.Data.Metadata.Edm;
using System.Diagnostics;

namespace System.Web.UI.Design.WebControls
{
    internal class EntityDataSourceContainerNameItem : IComparable<EntityDataSourceContainerNameItem>
    {
        // Only one of the following should be set. This is enforced through the constructors and the fact that these fields are readonly.
        private readonly EntityContainer _entityContainer; // used when we have a real EntityContainer backing this item
        private readonly string _unknownContainerName; // used when we have an unknown DefaultContainerName that we still want to include in the list
        
        internal EntityDataSourceContainerNameItem(EntityContainer entityContainer)
        {
            Debug.Assert(entityContainer != null, "null entityContainer");
            _entityContainer = entityContainer;            
        }

        internal EntityDataSourceContainerNameItem(string unknownContainerName)
        {
            Debug.Assert(!String.IsNullOrEmpty(unknownContainerName), "null or empty unknownContainerName");
            _unknownContainerName = unknownContainerName;
        }

        internal string EntityContainerName
        {
            get
            {
                if (_entityContainer != null)
                {
                    return _entityContainer.Name;
                }
                else
                {
                    return _unknownContainerName;
                }
            }
        }

        internal EntityContainer EntityContainer
        {
            get
            {
                // may be null if this represents an unknown container
                return _entityContainer;
            }
        }

        public override string ToString()
        {
            return this.EntityContainerName;
        }
        
        int IComparable<EntityDataSourceContainerNameItem>.CompareTo(EntityDataSourceContainerNameItem other)
        {
            return (String.Compare(this.EntityContainerName, other.EntityContainerName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
