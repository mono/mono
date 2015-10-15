//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Diagnostics
{
    // Order is important here. The order must match the order of strings in src\ndp\cdf\src\WCF\EventLog\EventLog.mc
    enum EventLogCategory : ushort
    {
        ServiceAuthorization = 1,  // reserved
        MessageAuthentication,     // reserved
        ObjectAccess,              // reserved
        Tracing,
        WebHost,
        FailFast,
        MessageLogging,
        PerformanceCounter,
        Wmi,
        ComPlus,
        StateMachine,
        Wsat,
        SharingService,
        ListenerAdapter
    }

}