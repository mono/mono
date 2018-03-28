//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------


namespace System.ServiceModel.Security.Tokens
{
    using System.ComponentModel;

    public enum SecurityTokenInclusionMode
    {
        AlwaysToRecipient = 0,
        Never = 1,
        Once = 2,
        AlwaysToInitiator = 3
    }

    static class SecurityTokenInclusionModeHelper
    {
        public static bool IsDefined(SecurityTokenInclusionMode value)
        {
            return (value == SecurityTokenInclusionMode.AlwaysToInitiator
            || value == SecurityTokenInclusionMode.AlwaysToRecipient
            || value == SecurityTokenInclusionMode.Never
            || value == SecurityTokenInclusionMode.Once);
        }

        public static void Validate(SecurityTokenInclusionMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(SecurityTokenInclusionMode)));
            }
        }

    }
}
