//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.ComponentModel;
    using System.Diagnostics;
    
    public enum SecurityKeyUsage
    {
        Exchange,
        Signature
    }

    static class SecurityKeyUsageHelper
    {
        internal static bool IsDefined(SecurityKeyUsage value)
        {
            return (value == SecurityKeyUsage.Exchange
                || value == SecurityKeyUsage.Signature);
        }

        internal static void Validate(SecurityKeyUsage value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(SecurityKeyUsage)));
            }
        }
    }
}
