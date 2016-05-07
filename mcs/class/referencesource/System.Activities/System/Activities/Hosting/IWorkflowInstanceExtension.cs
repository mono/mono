//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Hosting
{
    using System.Collections.Generic;

    // overriden by extensions that want to contribute additional
    // extensions and/or get notified when they are being used with a WorkflowInstance
    public interface IWorkflowInstanceExtension
    {
        IEnumerable<object> GetAdditionalExtensions();

        // called with the targe instance under WorkflowInstance.Initialize
        void SetInstance(WorkflowInstanceProxy instance);
    }
}