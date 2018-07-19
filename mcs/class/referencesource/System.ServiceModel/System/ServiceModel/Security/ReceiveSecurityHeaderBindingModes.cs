//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    [Flags]
    enum ReceiveSecurityHeaderBindingModes
    {
        Unknown = 0x0,
        Primary = 0x1,
        Endorsing = 0x2,
        Signed = 0x4,
        SignedEndorsing = 0x8,
        Basic = 0x10,
    }
}
