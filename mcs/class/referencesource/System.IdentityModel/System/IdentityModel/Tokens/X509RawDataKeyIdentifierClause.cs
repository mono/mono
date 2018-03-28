//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Globalization;
    using System.Security.Cryptography.X509Certificates;

    public class X509RawDataKeyIdentifierClause : BinaryKeyIdentifierClause
    {
        X509Certificate2 certificate;
        X509AsymmetricSecurityKey key;

        public X509RawDataKeyIdentifierClause(X509Certificate2 certificate)
            : this(GetRawData(certificate), false)
        {
            this.certificate = certificate;
        }

        public X509RawDataKeyIdentifierClause(byte[] certificateRawData)
            : this(certificateRawData, true)
        {
        }

        internal X509RawDataKeyIdentifierClause(byte[] certificateRawData, bool cloneBuffer)
            : base(null, certificateRawData, cloneBuffer)
        {
        }

        public override bool CanCreateKey
        {
            get { return true; }
        }

        public override SecurityKey CreateKey()
        {
            if (this.key == null)
            {
                if (this.certificate == null)
                {
                    this.certificate = new X509Certificate2(GetBuffer());
                }
                this.key = new X509AsymmetricSecurityKey(this.certificate);
            }
            return this.key;
        }

        static byte[] GetRawData(X509Certificate certificate)
        {
            if (certificate == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");

            return certificate.GetRawCertData();
        }

        public byte[] GetX509RawData()
        {
            return GetBuffer();
        }

        public bool Matches(X509Certificate2 certificate)
        {
            if (certificate == null)
                return false;

            return Matches(GetRawData(certificate));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X509RawDataKeyIdentifierClause(RawData = {0})", ToBase64String());
        }
    }
}
