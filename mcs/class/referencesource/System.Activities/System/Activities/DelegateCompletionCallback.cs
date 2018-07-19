//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Collections.Generic;

    public delegate void DelegateCompletionCallback(NativeActivityContext context, ActivityInstance completedInstance, IDictionary<string, object> outArguments);
}
