//------------------------------------------------------------------------------
// <copyright file="EntityDataSourceState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//
// Temporary storage for properties set via the wizard
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.WebControls
{
    internal class EntityDataSourceState
    {
        private string _connectionString;
        private string _defaultContainerName;
        private string _entityTypeFilter;
        private string _entitySetName;
        private string _select;

        public string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    return String.Empty;
                }
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        public string DefaultContainerName
        {
            get
            {
                if (_defaultContainerName == null)
                {
                    return String.Empty;
                }
                return _defaultContainerName;
            }
            set
            {
                _defaultContainerName = value;
            }
        }

        public bool EnableDelete
        {
            get;
            set;
        }

        public bool EnableInsert
        {
            get;
            set;
        }

        public bool EnableUpdate
        {
            get;
            set;
        }

        public string EntitySetName
        {
            get
            {
                if (_entitySetName == null)
                {
                    return String.Empty;
                }
                return _entitySetName;
            }
            set
            {
                _entitySetName = value;
            }
        }

        public string EntityTypeFilter
        {
            get
            {
                if (_entityTypeFilter == null)
                {
                    return String.Empty;
                }
                return _entityTypeFilter;
            }
            set
            {
                _entityTypeFilter = value;
            }
        }

        public string Select
        {
            get
            {
                if (_select == null)
                {
                    return String.Empty;
                }
                return _select;
            }
            set
            {
                _select = value;
            }
        }

        public bool EnableFlattening
        {
            get;
            set;
        }        
    }
}
