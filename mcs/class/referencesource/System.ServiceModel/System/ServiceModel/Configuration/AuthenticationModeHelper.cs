//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{

    static class AuthenticationModeHelper
    {
        public static bool IsDefined(AuthenticationMode value)
        {
            return value == AuthenticationMode.AnonymousForCertificate
            || value == AuthenticationMode.AnonymousForSslNegotiated
            || value == AuthenticationMode.CertificateOverTransport
            || value == AuthenticationMode.IssuedToken
            || value == AuthenticationMode.IssuedTokenForCertificate
            || value == AuthenticationMode.IssuedTokenForSslNegotiated
            || value == AuthenticationMode.IssuedTokenOverTransport
            || value == AuthenticationMode.Kerberos
            || value == AuthenticationMode.KerberosOverTransport
            || value == AuthenticationMode.MutualCertificate
            || value == AuthenticationMode.MutualCertificateDuplex
            || value == AuthenticationMode.MutualSslNegotiated
            || value == AuthenticationMode.SecureConversation
            || value == AuthenticationMode.SspiNegotiated
            || value == AuthenticationMode.UserNameForCertificate
            || value == AuthenticationMode.UserNameForSslNegotiated
            || value == AuthenticationMode.UserNameOverTransport
            || value == AuthenticationMode.SspiNegotiatedOverTransport;
        }
    }
}



