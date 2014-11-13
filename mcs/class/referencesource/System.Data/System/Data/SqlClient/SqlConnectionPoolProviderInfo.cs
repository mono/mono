//------------------------------------------------------------------------------
// <copyright file="SqlConnectionPoolProviderInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using System;
    using System.Data.ProviderBase;
    using System.Diagnostics;

    internal sealed class SqlConnectionPoolProviderInfo : DbConnectionPoolProviderInfo {
        private string _instanceName;

        internal string InstanceName {
            get {
                return _instanceName;
            }
            set {
                _instanceName = value;
            }
        }        
    }
}
