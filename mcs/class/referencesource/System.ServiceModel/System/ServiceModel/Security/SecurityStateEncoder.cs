//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    // The rationale for making abstract methods protected instead of public is following:
    // 1. No scenarios for making them public.
    // 2. Reduction of threat area (other assemblies on the channel can't call these methods other than through reflection).
    // 3. Reduction of test area (feature is testable only through other high-level features).
    public abstract class SecurityStateEncoder
    {
        protected SecurityStateEncoder() { }

        protected internal abstract byte[] DecodeSecurityState(byte[] data);
        protected internal abstract byte[] EncodeSecurityState(byte[] data);
    }
}

