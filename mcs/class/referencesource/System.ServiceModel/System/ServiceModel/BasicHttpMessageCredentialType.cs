//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    public enum BasicHttpMessageCredentialType
    {
        UserName,
        Certificate,
    }

    static class BasicHttpMessageCredentialTypeHelper
    {
        internal static bool IsDefined(BasicHttpMessageCredentialType value)
        {
            return (value == BasicHttpMessageCredentialType.UserName ||
                value == BasicHttpMessageCredentialType.Certificate);
        }
    }
}
