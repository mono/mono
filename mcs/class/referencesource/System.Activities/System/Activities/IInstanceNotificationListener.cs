//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;

    interface IInstanceNotificationListener
    {
        void AbortInstance(Exception reason, bool isWorkflowThread);
        void OnIdle();
        bool OnUnhandledException(Exception exception, Activity exceptionSource);
    }
}
