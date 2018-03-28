//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel
{

    public enum PeerTransportCredentialType
    {
        Password,
        Certificate
    }

    static class PeerTransportCredentialTypeHelper
    {
        internal static bool IsDefined(PeerTransportCredentialType value)
        {
            return (
                value == PeerTransportCredentialType.Password ||
                value == PeerTransportCredentialType.Certificate);
        }
    }
}

