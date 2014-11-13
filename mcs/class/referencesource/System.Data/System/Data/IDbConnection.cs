//------------------------------------------------------------------------------
// <copyright file="IDbConnection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    public interface IDbConnection : IDisposable {

        string ConnectionString {
            get;
            set; 
        }

        int ConnectionTimeout {
            get;
        }

        string Database {
            get;
        }

        ConnectionState State {
            get;
        }

        IDbTransaction BeginTransaction();

        IDbTransaction BeginTransaction(IsolationLevel il); 

        void Close();

        void ChangeDatabase(string databaseName);

        IDbCommand CreateCommand();

        void Open();
    }
}
