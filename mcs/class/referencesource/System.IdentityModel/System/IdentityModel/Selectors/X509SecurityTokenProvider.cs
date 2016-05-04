//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography.X509Certificates;

    public class X509SecurityTokenProvider : SecurityTokenProvider, IDisposable
    {
        X509Certificate2 certificate;

        public X509SecurityTokenProvider(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }

            this.certificate = new X509Certificate2(certificate);
        }

        public X509SecurityTokenProvider(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
        {
            if (findValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("findValue");
            }

            X509CertificateStore store = new X509CertificateStore(storeName, storeLocation);
            X509Certificate2Collection certificates = null;
            try
            {
                store.Open(OpenFlags.ReadOnly);
                certificates = store.Find(findType, findValue, false);
                if (certificates.Count < 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.CannotFindCert, storeName, storeLocation, findType, findValue)));
                }
                if (certificates.Count > 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.FoundMultipleCerts, storeName, storeLocation, findType, findValue)));
                }

                this.certificate = new X509Certificate2(certificates[0]);
            }
            finally
            {
                SecurityUtils.ResetAllCertificates(certificates);
                store.Close();
            }
        }

        public X509Certificate2 Certificate
        {
            get { return this.certificate; }
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return new X509SecurityToken(this.certificate);
        }

        public void Dispose()
        {
            SecurityUtils.ResetCertificate(this.certificate);
        }
    }
}
