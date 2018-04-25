//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    public enum WorkflowUnhandledExceptionAction
    {
        Abandon,
        Cancel,
        Terminate,
        AbandonAndSuspend,
    }

    static class WorkflowUnhandledExceptionActionHelper
    {
        internal static bool IsDefined(WorkflowUnhandledExceptionAction value)
        {
            return (value == WorkflowUnhandledExceptionAction.Abandon ||
                value == WorkflowUnhandledExceptionAction.Cancel ||
                value == WorkflowUnhandledExceptionAction.Terminate ||
                value == WorkflowUnhandledExceptionAction.AbandonAndSuspend);
        }
    }
}
