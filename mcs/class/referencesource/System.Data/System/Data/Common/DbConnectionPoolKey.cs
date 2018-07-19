//------------------------------------------------------------------------------
// <copyright file="ConnectionPoolKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common
{

    using System;
    using System.Collections.Generic;
    using System.Data;

    // DbConnectionPoolKey: Base class implementation of a key to connection pool groups
    //  Only connection string is used as a key
    internal class DbConnectionPoolKey : ICloneable
    {
        private string _connectionString;

        internal DbConnectionPoolKey(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected DbConnectionPoolKey(DbConnectionPoolKey key)
        {
            _connectionString = key.ConnectionString;
        }

        object ICloneable.Clone()
        {
            return new DbConnectionPoolKey(this);
        }

        internal virtual string ConnectionString
        {
            get
            {
                return _connectionString;
            }

            set
            {
                _connectionString = value;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(DbConnectionPoolKey))
            {
                return false;
            }

            DbConnectionPoolKey key = obj as DbConnectionPoolKey;

            return (key != null && _connectionString == key._connectionString);
        }

        public override int GetHashCode()
        {
            return _connectionString == null ? 0 : _connectionString.GetHashCode();
        }
    }
}
