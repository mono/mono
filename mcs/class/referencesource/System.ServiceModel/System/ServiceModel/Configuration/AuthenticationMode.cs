//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    public enum AuthenticationMode
    {
        AnonymousForCertificate,
        AnonymousForSslNegotiated,
        CertificateOverTransport,
        IssuedToken,
        IssuedTokenForCertificate,
        IssuedTokenForSslNegotiated,
        IssuedTokenOverTransport,
        Kerberos,
        KerberosOverTransport,
        MutualCertificate,
        MutualCertificateDuplex,
        MutualSslNegotiated,
        SecureConversation,
        SspiNegotiated,
        UserNameForCertificate,
        UserNameForSslNegotiated,
        UserNameOverTransport,
        SspiNegotiatedOverTransport
    }

}



