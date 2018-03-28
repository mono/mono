//------------------------------------------------------------------------------
// <copyright file="ISyncContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;

    // Used to perform some post-processing that needs to take place around a callback
    // in order to set up the appropriate synchronization primitives.

    internal interface ISyncContextLock {

        void Leave();

    }
}
