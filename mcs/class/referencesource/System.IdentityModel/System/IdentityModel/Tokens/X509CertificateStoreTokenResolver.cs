//-----------------------------------------------------------------------
// <copyright file="X509CertificateStoreTokenResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    /// <summary>
    /// Token Resolver that can resolve X509SecurityTokens against a given X.509 Certificate Store.
    /// </summary>
    public class X509CertificateStoreTokenResolver : SecurityTokenResolver
    {
        private string storeName;
        private StoreLocation storeLocation;

        /// <summary>
        /// Initializes an instance of <see cref="X509CertificateStoreTokenResolver"/>
        /// </summary>
        public X509CertificateStoreTokenResolver()
            : this(System.Security.Cryptography.X509Certificates.StoreName.My, StoreLocation.LocalMachine)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="X509CertificateStoreTokenResolver"/>
        /// </summary>
        /// <param name="storeName">StoreName of the X.509 Certificate Store.</param>
        /// <param name="storeLocation">StoreLocation of the X.509 Certificate store.</param>
        public X509CertificateStoreTokenResolver(StoreName storeName, StoreLocation storeLocation)
            : this(Enum.GetName(typeof(System.Security.Cryptography.X509Certificates.StoreName), storeName), storeLocation)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="X509CertificateStoreTokenResolver"/>
        /// </summary>
        /// <param name="storeName">StoreName of the X.509 Certificate Store.</param>
        /// <param name="storeLocation">StoreLocation of the X.509 Certificate store.</param>
        public X509CertificateStoreTokenResolver(string storeName, StoreLocation storeLocation)
        {
            if (string.IsNullOrEmpty(storeName))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("storeName");
            }

            this.storeName = storeName;
            this.storeLocation = storeLocation;
        }

        /// <summary>
        /// Gets the StoreName used by this TokenResolver.
        /// </summary>
        public string StoreName
        {
            get { return this.storeName; }
        }

        /// <summary>
        /// Gets the StoreLocation used by this TokenResolver.
        /// </summary>
        public StoreLocation StoreLocation
        {
            get { return this.storeLocation; }
        }

        /// <summary>
        /// Resolves the given SecurityKeyIdentifierClause to a SecurityKey.
        /// </summary>
        /// <param name="keyIdentifierClause">SecurityKeyIdentifierClause to resolve</param>
        /// <param name="key">The resolved SecurityKey.</param>
        /// <returns>True if successfully resolved.</returns>
        /// <exception cref="ArgumentNullException">The input argument 'keyIdentifierClause' is null.</exception>
        protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }

            key = null;
            EncryptedKeyIdentifierClause encryptedKeyIdentifierClause = keyIdentifierClause as EncryptedKeyIdentifierClause;
            if (encryptedKeyIdentifierClause != null)
            {
                SecurityKeyIdentifier keyIdentifier = encryptedKeyIdentifierClause.EncryptingKeyIdentifier;
                if (keyIdentifier != null && keyIdentifier.Count > 0)
                {
                    for (int i = 0; i < keyIdentifier.Count; i++)
                    {
                        SecurityKey unwrappingSecurityKey = null;
                        if (TryResolveSecurityKey(keyIdentifier[i], out unwrappingSecurityKey))
                        {
                            byte[] wrappedKey = encryptedKeyIdentifierClause.GetEncryptedKey();
                            string wrappingAlgorithm = encryptedKeyIdentifierClause.EncryptionMethod;
                            byte[] unwrappedKey = unwrappingSecurityKey.DecryptKey(wrappingAlgorithm, wrappedKey);
                            key = new InMemorySymmetricSecurityKey(unwrappedKey, false);
                            return true;
                        }
                    }
                }
            }
            else
            {
                SecurityToken token = null;
                if (TryResolveToken(keyIdentifierClause, out token))
                {
                    if (token.SecurityKeys.Count > 0)
                    {
                        key = token.SecurityKeys[0];
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Resolves the given SecurityKeyIdentifier to a SecurityToken.
        /// </summary>
        /// <param name="keyIdentifier">SecurityKeyIdentifier to resolve.</param>
        /// <param name="token">The resolved SecurityToken.</param>
        /// <returns>True if successfully resolved.</returns>
        /// <exception cref="ArgumentNullException">The input argument 'keyIdentifier' is null.</exception>
        protected override bool TryResolveTokenCore(SecurityKeyIdentifier keyIdentifier, out SecurityToken token)
        {
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }

            token = null;
            foreach (SecurityKeyIdentifierClause clause in keyIdentifier)
            {
                if (TryResolveToken(clause, out token))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Resolves the given SecurityKeyIdentifierClause to a SecurityToken.
        /// </summary>
        /// <param name="keyIdentifierClause">SecurityKeyIdentifierClause to resolve.</param>
        /// <param name="token">The resolved SecurityToken.</param>
        /// <returns>True if successfully resolved.</returns>
        /// <exception cref="ArgumentNullException">The input argument 'keyIdentifierClause' is null.</exception>
        protected override bool TryResolveTokenCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityToken token)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }

            token = null;
            X509Store store = null;
            X509Certificate2Collection certs = null;
            try
            {
                store = new X509Store(this.storeName, this.storeLocation);
                store.Open(OpenFlags.ReadOnly);
                certs = store.Certificates;
                foreach (X509Certificate2 cert in certs)
                {
                    X509ThumbprintKeyIdentifierClause thumbprintKeyIdentifierClause = keyIdentifierClause as X509ThumbprintKeyIdentifierClause;
                    if (thumbprintKeyIdentifierClause != null && thumbprintKeyIdentifierClause.Matches(cert))
                    {
                        token = new X509SecurityToken(cert);
                        return true;
                    }

                    X509IssuerSerialKeyIdentifierClause issuerSerialKeyIdentifierClause = keyIdentifierClause as X509IssuerSerialKeyIdentifierClause;
                    if (issuerSerialKeyIdentifierClause != null && issuerSerialKeyIdentifierClause.Matches(cert))
                    {
                        token = new X509SecurityToken(cert);
                        return true;
                    }

                    X509SubjectKeyIdentifierClause subjectKeyIdentifierClause = keyIdentifierClause as X509SubjectKeyIdentifierClause;
                    if (subjectKeyIdentifierClause != null && subjectKeyIdentifierClause.Matches(cert))
                    {
                        token = new X509SecurityToken(cert);
                        return true;
                    }

                    X509RawDataKeyIdentifierClause rawDataKeyIdentifierClause = keyIdentifierClause as X509RawDataKeyIdentifierClause;
                    if (rawDataKeyIdentifierClause != null && rawDataKeyIdentifierClause.Matches(cert))
                    {
                        token = new X509SecurityToken(cert);
                        return true;
                    }
                }
            }
            finally
            {
                if (certs != null)
                {
                    for (int i = 0; i < certs.Count; i++)
                    {
                        certs[i].Reset();
                    }
                }

                if (store != null)
                {
                    store.Close();
                }
            }

            return false;
        }
    }
}
