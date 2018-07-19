//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime;

    public interface IExecutionProperty
    {
        void SetupWorkflowThread();
        void CleanupWorkflowThread();
    }
}


