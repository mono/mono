//------------------------------------------------------------------------------
// <copyright file="ApplicationIntent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">adoprov</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    /// <summary>
    /// represents the application workload type when connecting to a server
    /// </summary>
    [Serializable]
    public enum ApplicationIntent {
        ReadWrite      = 0,
        ReadOnly       = 1,
    }
}
