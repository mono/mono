//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    public class X509IssuerSerialKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        readonly string issuerName;
        readonly string issuerSerialNumber;

        public X509IssuerSerialKeyIdentifierClause(string issuerName, string issuerSerialNumber)
            : base(null)
        {
            if (string.IsNullOrEmpty(issuerName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuerName");
            if (string.IsNullOrEmpty(issuerSerialNumber))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuerSerialNumber");

            this.issuerName = issuerName;
            this.issuerSerialNumber = issuerSerialNumber;
        }

        public X509IssuerSerialKeyIdentifierClause(X509Certificate2 certificate)
            : base(null)
        {
            if (certificate == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");

            this.issuerName = certificate.Issuer;
            this.issuerSerialNumber = Asn1IntegerConverter.Asn1IntegerToDecimalString(certificate.GetSerialNumber());
        }

        public string IssuerName
        {
            get { return this.issuerName; }
        }

        public string IssuerSerialNumber
        {
            get { return this.issuerSerialNumber; }
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            X509IssuerSerialKeyIdentifierClause that = keyIdentifierClause as X509IssuerSerialKeyIdentifierClause;

            // PreSharp 
#pragma warning suppress 56506
            return ReferenceEquals(this, that) || (that != null && that.Matches(this.issuerName, this.issuerSerialNumber));
        }

        public bool Matches(X509Certificate2 certificate)
        {
            if (certificate == null)
                return false;

            return Matches(certificate.Issuer, Asn1IntegerConverter.Asn1IntegerToDecimalString(certificate.GetSerialNumber()));
        }

        public bool Matches(string issuerName, string issuerSerialNumber)
        {
            if (issuerName == null)
            {
                return false;
            }

            // If serial numbers dont match, we can avoid the potentially expensive issuer name comparison
            if (this.issuerSerialNumber != issuerSerialNumber)
            {
                return false;
            }

            // Serial numbers match. Do a string comparison of issuer names
            if (this.issuerName == issuerName)
            {
                return true;
            }

            // String equality comparison for issuer names failed
            // Do a byte-level comparison of the X500 distinguished names corresponding to the issuer names. 
            // X500DistinguishedName constructor can throw for malformed inputs
            bool x500IssuerNameMatch = false;
            try
            {
                if (CryptoHelper.IsEqual(new X500DistinguishedName(this.issuerName).RawData,
                                         new X500DistinguishedName(issuerName).RawData))
                {
                    x500IssuerNameMatch = true;
                }
            }
            catch (CryptographicException e)
            {
                // Absorb and log exception. Fallthrough and return false from method.
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }

            return x500IssuerNameMatch;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X509IssuerSerialKeyIdentifierClause(Issuer = '{0}', Serial = '{1}')",
                this.IssuerName, this.IssuerSerialNumber);
        }
    }
}
