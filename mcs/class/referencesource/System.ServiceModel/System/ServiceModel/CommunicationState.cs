//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;

    public enum CommunicationState
    {
        Created,
        Opening,
        Opened,
        Closing,
        Closed,
        Faulted
    }
}
