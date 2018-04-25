//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    public enum WSFederationHttpSecurityMode
    {
        None,
        Message,
        TransportWithMessageCredential
    }

    static class WSFederationHttpSecurityModeHelper
    {
        internal static bool IsDefined(WSFederationHttpSecurityMode value)
        {
            return (value == WSFederationHttpSecurityMode.None ||
                value == WSFederationHttpSecurityMode.Message ||
                value == WSFederationHttpSecurityMode.TransportWithMessageCredential);
        }
    }
}
