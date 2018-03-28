//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ComponentModel;

    
    static class X509CertificateValidationModeHelper
    {
        public static bool IsDefined(X509CertificateValidationMode validationMode)
        {
            return validationMode == X509CertificateValidationMode.None
                || validationMode == X509CertificateValidationMode.PeerTrust
                || validationMode == X509CertificateValidationMode.ChainTrust
                || validationMode == X509CertificateValidationMode.PeerOrChainTrust
                || validationMode == X509CertificateValidationMode.Custom;
        }

        internal static void Validate(X509CertificateValidationMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(X509CertificateValidationMode)));
            }
        }

    }
}
