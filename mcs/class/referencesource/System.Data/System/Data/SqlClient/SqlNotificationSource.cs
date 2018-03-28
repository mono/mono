//------------------------------------------------------------------------------
// <copyright file="SqlNotificationSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">mithomas</owner>
// <owner current="true" primary="false">blained</owner>
// <owner current="false" primary="false">ramp</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    public enum SqlNotificationSource {
        Data        = 0,
        Timeout     = 1,
        Object      = 2,
        Database    = 3,
        System      = 4,
        Statement   = 5,
        Environment = 6,
        Execution   = 7,
        Owner       = 8,

        // use negative values for client-only-generated values
        Unknown     = -1,
        Client      = -2
    }
}

