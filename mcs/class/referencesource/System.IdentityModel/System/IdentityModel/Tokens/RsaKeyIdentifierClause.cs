//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Xml;

    public class RsaKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        static string clauseType = XmlSignatureStrings.Namespace + XmlSignatureStrings.RsaKeyValue;
        readonly RSA rsa;
        readonly RSAParameters rsaParameters;
        RsaSecurityKey rsaSecurityKey;

        public RsaKeyIdentifierClause(RSA rsa)
            : base(clauseType)
        {
            if (rsa == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rsa");

            this.rsa = rsa;
            this.rsaParameters = rsa.ExportParameters(false);
        }

        public override bool CanCreateKey
        {
            get { return true; }
        }

        public RSA Rsa
        {
            get { return this.rsa; }
        }

        public override SecurityKey CreateKey()
        {
            if (this.rsaSecurityKey == null)
            {
                this.rsaSecurityKey = new RsaSecurityKey(this.rsa);
            }
            return this.rsaSecurityKey;
        }

        public byte[] GetExponent()
        {
            return SecurityUtils.CloneBuffer(this.rsaParameters.Exponent);
        }

        public byte[] GetModulus()
        {
            return SecurityUtils.CloneBuffer(this.rsaParameters.Modulus);
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            RsaKeyIdentifierClause that = keyIdentifierClause as RsaKeyIdentifierClause;

            // PreSharp 
            #pragma warning suppress 56506
            return ReferenceEquals(this, that) || (that != null && that.Matches(this.rsa));
        }

        public bool Matches(RSA rsa)
        {
            if (rsa == null)
                return false;

            RSAParameters rsaParameters = rsa.ExportParameters(false);
            return SecurityUtils.MatchesBuffer(this.rsaParameters.Modulus, rsaParameters.Modulus) &&
                SecurityUtils.MatchesBuffer(this.rsaParameters.Exponent, rsaParameters.Exponent);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "RsaKeyIdentifierClause(Modulus = {0}, Exponent = {1})",
                Convert.ToBase64String(this.rsaParameters.Modulus),
                Convert.ToBase64String(this.rsaParameters.Exponent));
        }

        public void WriteExponentAsBase64(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteBase64(this.rsaParameters.Exponent, 0, this.rsaParameters.Exponent.Length);
        }

        public void WriteModulusAsBase64(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteBase64(this.rsaParameters.Modulus, 0, this.rsaParameters.Modulus.Length);
        }
    }
}
