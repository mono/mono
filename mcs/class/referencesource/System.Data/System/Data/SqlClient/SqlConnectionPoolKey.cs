//------------------------------------------------------------------------------
// <copyright file="ConnectionPoolKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient
{

    using System;
    using System.Collections;
    using System.Data.Common;

    // SqlConnectionPoolKey: Implementation of a key to connection pool groups for specifically to be used for SqlConnection
    //  Connection string and SqlCredential are used as a key
    internal class SqlConnectionPoolKey : DbConnectionPoolKey, ICloneable
    {
        private SqlCredential _credential;
        private int _hashValue;

        internal SqlConnectionPoolKey(string connectionString, SqlCredential credential) : base(connectionString)
        {
            _credential = credential;
            CalculateHashCode();
        }

        private SqlConnectionPoolKey(SqlConnectionPoolKey key) : base (key)
        {
             _credential = key.Credential;
             CalculateHashCode();
        }

        object ICloneable.Clone()
        {
            return new SqlConnectionPoolKey(this);
        }

        internal override string ConnectionString
        {
            get
            {
                return base.ConnectionString;
            }

            set
            {
                base.ConnectionString = value;
                CalculateHashCode();
            }
        }

        internal SqlCredential Credential
        {
            get
            {
                return _credential;
            }
        }


        public override bool Equals(object obj)
        {
            SqlConnectionPoolKey key = obj as SqlConnectionPoolKey;

            return (key != null && _credential == key._credential && ConnectionString == key.ConnectionString);
        }

        public override int GetHashCode()
        {
            return _hashValue;
        }

        private void CalculateHashCode()
        {
            _hashValue = base.GetHashCode();

            if (_credential != null)
            {
                unchecked
                {
                    _hashValue = _hashValue * 17 + _credential.GetHashCode();
                }
            }
        }
    }
}
