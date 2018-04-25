//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel
{
    public enum NetNamedPipeSecurityMode
    {
        None,
        Transport
    }

    static class NetNamedPipeSecurityModeHelper
    {
        internal static bool IsDefined(NetNamedPipeSecurityMode value)
        {
            return
                value == NetNamedPipeSecurityMode.Transport ||
                value == NetNamedPipeSecurityMode.None;
        }
    }
}


