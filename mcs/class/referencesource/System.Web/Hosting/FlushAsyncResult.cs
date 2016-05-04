//------------------------------------------------------------------------------
// <copyright file="FlushAsyncResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * IAsyncResult for asynchronous flush.
 *
 * Copyright (c) 2010 Microsoft Corporation
 */

namespace System.Web.Hosting {
    using System;
    using System.Threading;
    using System.Web.Util;

    internal class FlushAsyncResult : AsyncResultBase {
        internal FlushAsyncResult(AsyncCallback cb, Object state): base(cb, state) {
        }

        internal override void Complete(int bytesSent, int hresult, IntPtr pAsyncCompletionContext, bool synchronous) {
            Complete(hresult, synchronous);
        }
    }
}
