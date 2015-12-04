//------------------------------------------------------------------------------
// <copyright file="EntityConnectionStringBuilderItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

using System.Data.EntityClient;
using System.Diagnostics;

namespace System.Web.UI.Design.WebControls
{
    internal class EntityConnectionStringBuilderItem : IComparable<EntityConnectionStringBuilderItem>
    {
        // Only one of the following should be set. This is enforced through the constructors and the fact that these fields are readonly.
        private readonly EntityConnectionStringBuilder _connectionStringBuilder;
        private readonly string _unknownConnectionString; // used when the string cannot be loaded into a connection string builder or is missing some required keywords

        internal EntityConnectionStringBuilderItem(EntityConnectionStringBuilder connectionStringBuilder)
        {
            // empty connection string builder is allowed, but not null
            Debug.Assert(connectionStringBuilder != null, "null connectionStringBuilder");

            _connectionStringBuilder = connectionStringBuilder;
        }

        internal EntityConnectionStringBuilderItem(string unknownConnectionString)
        {
            // empty is not allowed -- use the constructor that takes a builder if the string is empty
            Debug.Assert(!String.IsNullOrEmpty(unknownConnectionString), "null or empty unknownConnectionString");
            _unknownConnectionString = unknownConnectionString;
        }

        internal string ConnectionString
        {
            get
            {
                if (_connectionStringBuilder != null)
                {
                    return _connectionStringBuilder.ConnectionString;
                }
                else
                {
                    return _unknownConnectionString;
                }
            }
        }

        internal EntityConnectionStringBuilder EntityConnectionStringBuilder
        {
            get
            {
                return _connectionStringBuilder;
            }
        }

        internal bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(this.ConnectionString);
            }
        }

        internal bool IsNamedConnection
        {
            get
            {
                if (_connectionStringBuilder != null)
                {
                    return !String.IsNullOrEmpty(_connectionStringBuilder.Name);
                }
                else
                {
                    // if the connection string is not recognized by a EntityConnectionStringBuilder, it can't be a valid named connection
                    return false;
                }
            }
        }
        
        public override string ToString()
        {
            // Display just the name for named connections, but the full connection string otherwise
            if (_connectionStringBuilder != null)
            {
                if (!String.IsNullOrEmpty(_connectionStringBuilder.Name))
                {
                    return _connectionStringBuilder.Name;
                }
                else
                {
                    return _connectionStringBuilder.ConnectionString;
                }
            }
            else
            {
                return _unknownConnectionString;
            }
        }

        int IComparable<EntityConnectionStringBuilderItem>.CompareTo(EntityConnectionStringBuilderItem other)
        {
            return (String.Compare(this.ToString(), other.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
