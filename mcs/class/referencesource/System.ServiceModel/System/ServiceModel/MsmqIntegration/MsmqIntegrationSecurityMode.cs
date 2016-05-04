//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    public enum MsmqIntegrationSecurityMode
    {
        None,
        Transport
    }

    static class MsmqIntegrationSecurityModeHelper
    {
        internal static bool IsDefined(MsmqIntegrationSecurityMode value)
        {
            return (value == MsmqIntegrationSecurityMode.Transport 
                || value == MsmqIntegrationSecurityMode.None);
        }
    }
}


