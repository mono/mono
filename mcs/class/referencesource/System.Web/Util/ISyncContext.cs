//------------------------------------------------------------------------------
// <copyright file="ISyncContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;

    // Used to perform some pre-processing that needs to take place around a callback
    // in order to set up the appropriate synchronization primitives.

    internal interface ISyncContext {

        // Used by AppVerifier to make sure we're not trying to use a freed request.
        // This property getter must never throw but may return null if the context
        // has been freed or the request has finished.
        HttpContext HttpContext { get; }

        ISyncContextLock Enter();

    }
}
