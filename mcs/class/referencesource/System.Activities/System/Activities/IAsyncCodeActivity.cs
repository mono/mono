//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    // shared interface by AsyncCodeActivity and AsyncCodeActivity<TResult> to facilitate internal code sharing
    internal interface IAsyncCodeActivity
    {
        void FinishExecution(AsyncCodeActivityContext context, IAsyncResult result);
    }
}
