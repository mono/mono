//------------------------------------------------------------------------------
// <copyright file="IIS7WorkerRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;

    public enum RequestNotificationStatus {
        Continue = 0,
        Pending,
        FinishRequest,
    }
}   
