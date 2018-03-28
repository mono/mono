//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ComponentModel;

    public enum SecurityKeyEntropyMode
    {
        ClientEntropy,
        ServerEntropy,
        CombinedEntropy
    }

    sealed class SecurityKeyEntropyModeHelper
    {
        internal static bool IsDefined(SecurityKeyEntropyMode value)
        {
            return (value == SecurityKeyEntropyMode.ClientEntropy
                || value == SecurityKeyEntropyMode.ServerEntropy
                || value == SecurityKeyEntropyMode.CombinedEntropy);
        }

        internal static void Validate(SecurityKeyEntropyMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(SecurityKeyEntropyMode)));
            }
        }
    }
}
