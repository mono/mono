//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ServiceModel.Channels;

    static class WebHttpSecurityModeHelper
    {
        internal static bool IsDefined(WebHttpSecurityMode value)
        {
            return (value == WebHttpSecurityMode.None ||
                value == WebHttpSecurityMode.Transport ||
                value == WebHttpSecurityMode.TransportCredentialOnly);
        }
    }
}
