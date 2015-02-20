//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.Workflow.Runtime;

namespace System.Workflow.Runtime
{
    internal interface ISupportInterop
    {
        WorkBatchCollection BatchCollection { get; }
    }
}
