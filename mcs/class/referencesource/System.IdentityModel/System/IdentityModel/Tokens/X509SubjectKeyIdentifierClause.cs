//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Globalization;
    using System.Security.Cryptography.X509Certificates;

    public class X509SubjectKeyIdentifierClause : BinaryKeyIdentifierClause
    {
        const string SubjectKeyIdentifierOid = "2.5.29.14";
        const int SkiDataOffset = 2;

        public X509SubjectKeyIdentifierClause(byte[] ski)
            : this(ski, true)
        {
        }

        internal X509SubjectKeyIdentifierClause(byte[] ski, bool cloneBuffer)
            : base(null, ski, cloneBuffer)
        {
        }

        static byte[] GetSkiRawData(X509Certificate2 certificate)
        {
            if (certificate == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");

            X509SubjectKeyIdentifierExtension skiExtension =
                certificate.Extensions[SubjectKeyIdentifierOid] as X509SubjectKeyIdentifierExtension;
            if (skiExtension != null)
            {
                return skiExtension.RawData;
            }
            else
            {
                return null;
            }
        }

        public byte[] GetX509SubjectKeyIdentifier()
        {
            return GetBuffer();
        }

        public bool Matches(X509Certificate2 certificate)
        {
            if (certificate == null)
                return false;

            byte[] data = GetSkiRawData(certificate);
            return data != null && Matches(data, SkiDataOffset);
        }

        public static bool TryCreateFrom(X509Certificate2 certificate, out X509SubjectKeyIdentifierClause keyIdentifierClause)
        {
            byte[] data = GetSkiRawData(certificate);
            keyIdentifierClause = null;
            if (data != null)
            {
                byte[] ski = SecurityUtils.CloneBuffer(data, SkiDataOffset, data.Length - SkiDataOffset);
                keyIdentifierClause = new X509SubjectKeyIdentifierClause(ski, false);
            }
            return keyIdentifierClause != null;
        }

        public static bool CanCreateFrom(X509Certificate2 certificate)
        {
            return null != GetSkiRawData(certificate);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X509SubjectKeyIdentifierClause(SKI = 0x{0})", ToHexString());
        }
    }
}
