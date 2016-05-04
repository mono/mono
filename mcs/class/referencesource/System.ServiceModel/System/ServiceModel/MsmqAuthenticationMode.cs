//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    public enum MsmqAuthenticationMode
    {
        None,
        WindowsDomain,
        Certificate,
    }

    static class MsmqAuthenticationModeHelper
    {
        public static bool IsDefined(MsmqAuthenticationMode mode)
        {
            return mode >= MsmqAuthenticationMode.None && mode <= MsmqAuthenticationMode.Certificate;
        }
    }
}
