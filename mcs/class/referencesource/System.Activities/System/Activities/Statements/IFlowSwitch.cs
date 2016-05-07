//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Statements
{
    interface IFlowSwitch
    {
        bool Execute(NativeActivityContext context, Flowchart parent);
        FlowNode GetNextNode(object value);
    }
}
