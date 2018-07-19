//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.ComponentModel;

    public enum SecurityTokenReferenceStyle
    {
        Internal = 0,
        External = 1,
    }

    static class TokenReferenceStyleHelper
    {
        public static bool IsDefined(SecurityTokenReferenceStyle value)
        {
            return (value == SecurityTokenReferenceStyle.External || value == SecurityTokenReferenceStyle.Internal);
        }

        public static void Validate(SecurityTokenReferenceStyle value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(SecurityTokenReferenceStyle)));
            }
        }

    }
}

