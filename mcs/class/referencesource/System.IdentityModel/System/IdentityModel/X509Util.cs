//-----------------------------------------------------------------------
// <copyright file="X509Util.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    using Claim = System.Security.Claims.Claim;
    using System.Runtime;

    internal static class X509Util
    {
        internal static RSA EnsureAndGetPrivateRSAKey(X509Certificate2 certificate)
        {
            Fx.Assert(certificate != null, "certificate != null");

            // Reject no private key
            if (!certificate.HasPrivateKey)
            {
#pragma warning suppress 56526 // no validation necessary for value.Thumbprint
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ID1001, certificate.Thumbprint)));
            }

            // Check for accessibility of private key
            AsymmetricAlgorithm privateKey;
            try
            {
                privateKey = certificate.PrivateKey;
            }
            catch (CryptographicException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ID1039, certificate.Thumbprint), e));
            }

            // Reject weird private key
            RSA rsa = privateKey as RSA;
            if (rsa == null)
            {
#pragma warning suppress 56526 // no validation necessary for value.Thumbprint
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ID1002, certificate.Thumbprint)));
            }

            return rsa;
        }

        internal static X509Certificate2 ResolveCertificate(StoreName storeName, StoreLocation storeLocation, X509FindType findType, object findValue)
        {
            X509Certificate2 certificate = null;

            // Throwing InvalidOperationException here, following WCF precedent. 
            // Might be worth introducing a more specific exception here.
            if (!TryResolveCertificate(storeName, storeLocation, findType, findValue, out certificate))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.ID1025, storeName, storeLocation, findType, findValue)));
            }

            return certificate;
        }

        internal static bool TryResolveCertificate(StoreName storeName, StoreLocation storeLocation, X509FindType findType, object findValue, out X509Certificate2 certificate)
        {
            X509Store store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            certificate = null;
            X509Certificate2Collection certs = null;
            X509Certificate2Collection matches = null;
            try
            {
                certs = store.Certificates;
                matches = certs.Find(findType, findValue, false);

                // Throwing InvalidOperationException here, following WCF precedent. 
                // Might be worth introducing a more specific exception here.
                if (matches.Count == 1)
                {
                    certificate = new X509Certificate2(matches[0]);
                    return true;
                }
            }
            finally
            {
                CryptoHelper.ResetAllCertificates(matches);
                CryptoHelper.ResetAllCertificates(certs);
                store.Close();
            }

            return false;
        }

        internal static string GetCertificateId(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }

            string certificateId = certificate.SubjectName.Name;
            if (string.IsNullOrEmpty(certificateId))
            {
                certificateId = certificate.Thumbprint;
            }

            return certificateId;
        }

        internal static string GetCertificateIssuerName(X509Certificate2 certificate, IssuerNameRegistry issuerNameRegistry)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }

            if (issuerNameRegistry == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuerNameRegistry");
            }

            X509Chain chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.Build(certificate);
            X509ChainElementCollection elements = chain.ChainElements;

            string issuer = null;
            if (elements.Count > 1)
            {
                using (X509SecurityToken token = new X509SecurityToken(elements[1].Certificate))
                {
                    issuer = issuerNameRegistry.GetIssuerName(token);
                }
            }
            else
            {
                // This is a self-issued certificate. Use the thumbprint of the current certificate.
                using (X509SecurityToken token = new X509SecurityToken(certificate))
                {
                    issuer = issuerNameRegistry.GetIssuerName(token);
                }
            }

            for (int i = 1; i < elements.Count; ++i)
            {
                // Resets the state of the certificate and frees resources associated with it.
                elements[i].Certificate.Reset();
            }

            return issuer;
        }

        /// <summary>
        /// Creates an X509CertificateValidator using the given parameters.
        /// </summary>
        /// <param name="certificateValidationMode">The certificate validation mode to use.</param>
        /// <param name="revocationMode">The revocation mode to use.</param>
        /// <param name="trustedStoreLocation">The store to use.</param>
        /// <returns>The X509CertificateValidator.</returns>
        /// <remarks>Due to a WCF bug, X509CertificateValidatorEx must be used rather than WCF's validators directly</remarks>
        internal static X509CertificateValidator CreateCertificateValidator(
            System.ServiceModel.Security.X509CertificateValidationMode certificateValidationMode,
            X509RevocationMode revocationMode,
            StoreLocation trustedStoreLocation)
        {
            return new X509CertificateValidatorEx(certificateValidationMode, revocationMode, trustedStoreLocation);
        }


        public static IEnumerable<Claim> GetClaimsFromCertificate(X509Certificate2 certificate, string issuer)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }

            ICollection<Claim> claimsCollection = new Collection<Claim>();

            string thumbprint = Convert.ToBase64String(certificate.GetCertHash());
            claimsCollection.Add(new Claim(ClaimTypes.Thumbprint, thumbprint, ClaimValueTypes.Base64Binary, issuer));

            string value = certificate.SubjectName.Name;
            if (!string.IsNullOrEmpty(value))
            {
                claimsCollection.Add(new Claim(ClaimTypes.X500DistinguishedName, value, ClaimValueTypes.String, issuer));
            }

            value = certificate.GetNameInfo(X509NameType.DnsName, false);
            if (!string.IsNullOrEmpty(value))
            {
                claimsCollection.Add(new Claim(ClaimTypes.Dns, value, ClaimValueTypes.String, issuer));
            }

            value = certificate.GetNameInfo(X509NameType.SimpleName, false);
            if (!string.IsNullOrEmpty(value))
            {
                claimsCollection.Add(new Claim(ClaimTypes.Name, value, ClaimValueTypes.String, issuer));
            }

            value = certificate.GetNameInfo(X509NameType.EmailName, false);
            if (!string.IsNullOrEmpty(value))
            {
                claimsCollection.Add(new Claim(ClaimTypes.Email, value, ClaimValueTypes.String, issuer));
            }

            value = certificate.GetNameInfo(X509NameType.UpnName, false);
            if (!string.IsNullOrEmpty(value))
            {
                claimsCollection.Add(new Claim(ClaimTypes.Upn, value, ClaimValueTypes.String, issuer));
            }

            value = certificate.GetNameInfo(X509NameType.UrlName, false);
            if (!string.IsNullOrEmpty(value))
            {
                claimsCollection.Add(new Claim(ClaimTypes.Uri, value, ClaimValueTypes.String, issuer));
            }

            RSA rsa = certificate.PublicKey.Key as RSA;
            if (rsa != null)
            {
                claimsCollection.Add(new Claim(ClaimTypes.Rsa, rsa.ToXmlString(false), ClaimValueTypes.RsaKeyValue, issuer));
            }

            DSA dsa = certificate.PublicKey.Key as DSA;
            if (dsa != null)
            {
                claimsCollection.Add(new Claim(ClaimTypes.Dsa, dsa.ToXmlString(false), ClaimValueTypes.DsaKeyValue, issuer));
            }

            value = certificate.SerialNumber;
            if (!string.IsNullOrEmpty(value))
            {
                claimsCollection.Add(new Claim(ClaimTypes.SerialNumber, value, ClaimValueTypes.String, issuer));
            }

            return claimsCollection;
        }
    }
}
