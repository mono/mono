//------------------------------------------------------------------------------
// <copyright file="SqlNotificationType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">mithomas</owner>
// <owner current="true" primary="false">blained</owner>
// <owner current="false" primary="false">ramp</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    public enum SqlNotificationType {
        Change      = 0,
        Subscribe   = 1,

        // use negative values for client-only-generated values
        Unknown     = -1
    }
}

