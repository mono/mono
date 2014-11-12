using System;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Known result codes for signature verification. HRESULTS are generally returned by CAPI, while
    ///     the others are set by managed verification.
    /// </summary>
    public enum SignatureVerificationResult {
        Valid                           = 0x00000000,                   // S_OK

        // When adding a verification results which does not directly map to an HRESULT, a mapping needs to be
        // added in CapiNative.HResultForVerificationResult
        AssemblyIdentityMismatch        = 0x00000001,
        ContainingSignatureInvalid      = 0x00000002,
        PublicKeyTokenMismatch          = 0x00000003,
        PublisherMismatch               = 0x00000004,

        SystemError                     = unchecked((int)0x80096001),   // TRUST_E_SYSTEM_ERROR
        InvalidSignerCertificate        = unchecked((int)0x80096002),   // TRUST_E_NO_SIGNER_CERT
        InvalidCountersignature         = unchecked((int)0x80096003),   // TRUST_E_COUNTER_SIGNER
        InvalidCertificateSignature     = unchecked((int)0x80096004),   // TRUST_E_CERT_SIGNATURE
        InvalidTimestamp                = unchecked((int)0x80096005),   // TRUST_E_TIME_STAMP
        BadDigest                       = unchecked((int)0x80096010),   // TRUST_E_BAD_DIGEST
        BasicConstraintsNotObserved     = unchecked((int)0x80096019),   // TRUST_E_BASIC_CONSTRAINTS
        UnknownTrustProvider            = unchecked((int)0x800b0001),   // TRUST_E_PROVIDER_UNKNOWN
        UnknownVerificationAction       = unchecked((int)0x800b0002),   // TRUST_E_ACTION_UNKNOWN
        BadSignatureFormat              = unchecked((int)0x800b0003),   // TRUST_E_SUBJECT_FORM_UNKNOWN
        CertificateNotExplicitlyTrusted = unchecked((int)0x800b0004),   // TRUST_E_SUBJECT_NOT_TRUSTED
        MissingSignature                = unchecked((int)0x800b0100),   // TRUST_E_NO_SIGNATURE
        CertificateExpired              = unchecked((int)0x800b0101),   // CERT_E_EXPIRED
        InvalidTimePeriodNesting        = unchecked((int)0x800b0102),   // CERT_E_VALIDITYPERIODNESTING
        InvalidCertificateRole          = unchecked((int)0x800b0103),   // CERT_E_ROLE
        PathLengthConstraintViolated    = unchecked((int)0x800b0104),   // CERT_E_PATHLENCONST
        UnknownCriticalExtension        = unchecked((int)0x800b0105),   // CERT_E_
        CertificateUsageNotAllowed      = unchecked((int)0x800b0106),   // CERT_E_PURPOSE
        IssuerChainingError             = unchecked((int)0x800b0107),   // CERT_E_ISSUERCHAINING
        CertificateMalformed            = unchecked((int)0x800b0108),   // CERT_E_MALFORMED
        UntrustedRootCertificate        = unchecked((int)0x800b0109),   // CERT_E_UNTRUSTEDROOT
        CouldNotBuildChain              = unchecked((int)0x800b010a),   // CERT_E_CHAINING
        GenericTrustFailure             = unchecked((int)0x800b010b),   // TRUST_E_FAIL
        CertificateRevoked              = unchecked((int)0x800b010c),   // CERT_E_REVOKED
        UntrustedTestRootCertificate    = unchecked((int)0x800b010d),   // CERT_E_UNTRUSTEDTESTROOT
        RevocationCheckFailure          = unchecked((int)0x800b010e),   // CERT_E_REVOCATION_FAILURE
        InvalidCertificateUsage         = unchecked((int)0x800b0110),   // CERT_E_WRONG_USAGE
        CertificateExplicitlyDistrusted = unchecked((int)0x800b0111),   // CERT_E_EXPLICIT_DISTRUST
        UntrustedCertificationAuthority = unchecked((int)0x800b0112),   // CERT_E_UNTRUSTEDCA
        InvalidCertificatePolicy        = unchecked((int)0x800b0113),   // CERT_E_INVALID_POLICY
        InvalidCertificateName          = unchecked((int)0x800b0114)    // CERT_E_INVALID_NAME
    }
}
