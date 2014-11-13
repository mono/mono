//------------------------------------------------------------------------------
// <copyright file="IDbTransaction.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data{
    using System;

    public interface IDbTransaction : IDisposable {

        IDbConnection Connection { // MDAC 66655
            get;
        }

        IsolationLevel IsolationLevel {
            get;
        }

        void Commit();

        //IDbCommand CreateCommand(); // MDAC 68309

        void Rollback();
    }
}    

