//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Debugger
{
    // Interface to implement in serializable object containing Workflow
    // to be debuggable with Workflow debugger.
    public interface IDebuggableWorkflowTree
    {
        // Return the root of the workflow tree.
        Activity GetWorkflowRoot(); 
    }

}
