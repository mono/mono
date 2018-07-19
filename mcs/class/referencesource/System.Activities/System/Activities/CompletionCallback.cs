//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    public delegate void CompletionCallback(NativeActivityContext context, ActivityInstance completedInstance);
    public delegate void CompletionCallback<TResult>(NativeActivityContext context, ActivityInstance completedInstance, TResult result);
}
