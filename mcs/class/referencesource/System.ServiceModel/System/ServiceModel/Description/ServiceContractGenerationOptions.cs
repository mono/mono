//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    [Flags]
    public enum ServiceContractGenerationOptions
    {
        None                    = 0,
        AsynchronousMethods     = 1,
        ChannelInterface        = 2,
        InternalTypes           = 4,
        ClientClass             = 8,
        TypedMessages           = 16,
        EventBasedAsynchronousMethods = 32,
        TaskBasedAsynchronousMethod = 64
    }

}
