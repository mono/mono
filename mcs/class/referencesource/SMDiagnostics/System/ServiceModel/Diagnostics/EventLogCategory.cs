//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    // 

    [Obsolete("This has been replaced by System.Runtime.Diagnostics.EventLogCategory")]
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
