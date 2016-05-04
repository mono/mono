//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.ComponentModel;
    using System.Diagnostics;

    public enum SecurityKeyType
    {
        SymmetricKey,
        AsymmetricKey,
        BearerKey
    }

    static class SecurityKeyTypeHelper
    {
        internal static bool IsDefined(SecurityKeyType value)
        {
            return (value == SecurityKeyType.SymmetricKey
                || value == SecurityKeyType.AsymmetricKey
                || value == SecurityKeyType.BearerKey);
        }

        internal static void Validate(SecurityKeyType value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(SecurityKeyType)));
            }
        }


    }
}
