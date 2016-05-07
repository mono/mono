//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    public enum WSDualHttpSecurityMode
    {
        None,
        Message,
    }

    static class WSDualHttpSecurityModeHelper
    {
        internal static bool IsDefined(WSDualHttpSecurityMode value)
        {
            return (value == WSDualHttpSecurityMode.None ||
                value == WSDualHttpSecurityMode.Message);
        }
    }
}
