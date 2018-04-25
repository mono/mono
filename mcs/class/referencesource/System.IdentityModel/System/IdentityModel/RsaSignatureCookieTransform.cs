//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace System.IdentityModel
{
    /// <summary>
    /// Provides cookie integrity using <see cref="RSA"/> signature.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="RsaSignatureCookieTransform"/> adds an RSA MAC to 
    /// the cookie data. This provides integrity but not confidentiality. By
    /// default the MAC uses SHA-256, but SHA-1 may be requested.
    /// </para>
    /// <para>
    /// Cookies signed with this transform may be read 
    /// by any machine that shares the same RSA private key (generally 
    /// associated with an X509 certificate).
    /// </para>
    /// </remarks>
    public class RsaSignatureCookieTransform : CookieTransform
    {
        RSA _signingKey;
        List<RSA> _verificationKeys = new List<RSA>();
        string _hashName = "SHA256";

         /// <summary>
        /// Creates a new instance of <see cref="RsaSignatureCookieTransform"/>.
        /// </summary>
        /// <param name="key">The provided key will be used as the signing and verification key by default.</param>
        /// <exception cref="ArgumentNullException">When the key is null.</exception>
        public RsaSignatureCookieTransform(RSA key)
        {
            if (null == key)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            _signingKey = key;
            _verificationKeys.Add(_signingKey);
        }

        /// <summary>
        /// Creates a new instance of <see cref="RsaSignatureCookieTransform"/>
        /// </summary>
        /// <param name="certificate">Certificate whose private key is used to sign and verify.</param>
        /// <exception cref="ArgumentNullException">When certificate is null.</exception>
        /// <exception cref="ArgumentException">When the certificate has no private key.</exception>
        /// <exception cref="ArgumentException">When the certificate's key is not RSA.</exception>
        public RsaSignatureCookieTransform(X509Certificate2 certificate)
        {
            if (null == certificate)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            _signingKey = X509Util.EnsureAndGetPrivateRSAKey(certificate);
            _verificationKeys.Add(_signingKey);
        }

        /// <summary>
        /// Gets or sets the name of the hash algorithm to use.
        /// </summary>
        /// <remarks>
        /// SHA256 is the default algorithm. This may require a minimum platform of Windows Server 2003 and .NET 3.5 SP1.
        /// If SHA256 is not supported, set HashName to "SHA1".
        /// </remarks>
        public string HashName
        {
            get { return _hashName; }
            set
            {
                using (HashAlgorithm algorithm = CryptoHelper.CreateHashAlgorithm(value))
                {
                    if (algorithm == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID6034, value));
                    }
                    _hashName = value;
                }
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="RsaSignatureCookieTransform"/>.
        /// The instance created by this constructor is not usable until the signing and verification keys are set.
        /// </summary>
        internal RsaSignatureCookieTransform()
        {
        }

        /// <summary>
        /// Gets or sets the RSA key used for signing
        /// </summary>
        public virtual RSA SigningKey
        {
            get { return _signingKey; }
            set
            {
                _signingKey = value;
                _verificationKeys = new List<RSA>(new RSA[] { _signingKey });
            }
        }

        /// <summary>
        /// Gets the collection of keys used for signature verification.
        /// By default, this property returns a list containing only the signing key.
        /// </summary>
        protected virtual ReadOnlyCollection<RSA> VerificationKeys
        {
            get
            {
                return _verificationKeys.AsReadOnly();
            }
        }

        // Format:
        //   SignatureLength : 4-byte big-endian integer
        //   Signature       : Octet stream, length is SignatureLength
        //   CookieValue     : Octet stream, remainder of message

        /// <summary>
        /// Verifies the signature.  All keys in the collection VerificationKeys will be attempted.
        /// </summary>
        /// <param name="encoded">Data previously returned from <see cref="Encode"/></param>
        /// <returns>The originally signed data.</returns>
        /// <exception cref="ArgumentNullException">The argument 'encoded' is null.</exception>
        /// <exception cref="ArgumentException">The argument 'encoded' contains zero bytes.</exception>
        /// <exception cref="FormatException">The data is in the wrong format.</exception>
        /// <exception cref="CryptographicException">The signature is invalid.</exception>
        /// <exception cref="NotSupportedException">The platform does not support the requested algorithm.</exception>
        /// <exception cref="InvalidOperationException">There are no verification keys.</exception>
        public override byte[] Decode(byte[] encoded)
        {
            if (null == encoded)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoded");
            }

            if (0 == encoded.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("encoded", SR.GetString(SR.ID6045));
            }

            ReadOnlyCollection<RSA> verificationKeys = VerificationKeys;

            if (0 == verificationKeys.Count)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID6036));
            }

            // Decode the message ...
            int currentIndex = 0;

            // SignatureLength : 4-byte big-endian integer
            if (encoded.Length < sizeof(Int32))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.ID1012)));
            }
            Int32 signatureLength = BitConverter.ToInt32(encoded, currentIndex);

            if (signatureLength < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.ID1005, signatureLength)));
            }

            if (signatureLength >= encoded.Length - sizeof(Int32))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.ID1013)));
            }
            currentIndex += sizeof(Int32);

            // Signature        : Octet stream, length is SignatureLength
            byte[] signature = new byte[signatureLength];
            Array.Copy(encoded, currentIndex, signature, 0, signature.Length);
            currentIndex += signature.Length;

            // CookieValue      : Octet stream, remainder of message
            byte[] cookieValue = new byte[encoded.Length - currentIndex];
            Array.Copy(encoded, currentIndex, cookieValue, 0, cookieValue.Length);

            bool verified = false;
            try
            {
                // Verify the signature
                using (HashAlgorithm hash = CryptoHelper.CreateHashAlgorithm(HashName))
                {
                    hash.ComputeHash(cookieValue);

                    foreach (RSA rsa in verificationKeys)
                    {
                        AsymmetricSignatureDeformatter verifier = GetSignatureDeformatter(rsa);
                        if ((isSha256() && CryptoHelper.VerifySignatureForSha256(verifier, hash, signature)) ||
                              verifier.VerifySignature(hash, signature))
                        {
                            verified = true;
                            break;
                        }
                    }
                }
            }

            // Not all algorithms are supported on all OS
            catch (CryptographicException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ID6035, HashName, verificationKeys[0].GetType().FullName), e));
            }

            if (!verified)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.ID1014)));
            }

            return cookieValue;
        }

        /// <summary>
        /// Signs data.
        /// </summary>
        /// <param name="value">Data to be signed.</param>
        /// <returns>Signed data.</returns>
        /// <exception cref="ArgumentNullException">The argument 'value' is null.</exception>
        /// <exception cref="ArgumentException">The argument 'value' contains zero bytes.</exception>
        /// <exception cref="InvalidOperationException">The SigningKey is null.</exception>
        /// <exception cref="NotSupportedException">The platform does not support the requested algorithm.</exception>
        /// <exception cref="InvalidOperationException">The SigningKey is null, is not an RSACryptoServiceProvider, or does not contain a private key.</exception>
        /// <remarks>The SigningKey must include the private key in order to sign.</remarks>
        public override byte[] Encode(byte[] value)
        {
            if (null == value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }

            if (0 == value.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID6044));
            }

            RSA signingKey = SigningKey;
            if (null == signingKey)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID6042));
            }

            RSACryptoServiceProvider rsaCryptoServiceProvider = signingKey as RSACryptoServiceProvider;
            if (rsaCryptoServiceProvider == null && LocalAppContextSwitches.DisableCngCertificates)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID6042));
            }

            if (rsaCryptoServiceProvider != null && rsaCryptoServiceProvider.PublicOnly)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID6046));
            }

            // Compute the signature
            byte[] signature;
            using (HashAlgorithm hash = CryptoHelper.CreateHashAlgorithm(HashName))
            {
                try
                {
                    hash.ComputeHash(value);
                    AsymmetricSignatureFormatter signer = GetSignatureFormatter(signingKey);

                    if (isSha256())
                    {
                        signature = CryptoHelper.CreateSignatureForSha256(signer, hash);
                    }
                    else
                    {
                        signature = signer.CreateSignature(hash);
                    }
                }
                // Not all algorithms are supported on all OS
                catch (CryptographicException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ID6035, HashName, signingKey.GetType().FullName), e));
                }
            }

            // Get the signature length as a big-endian integer
            byte[] signatureLength = BitConverter.GetBytes(signature.Length);

            // Assemble the message ...
            int currentIndex = 0;
            byte[] message = new byte[signatureLength.Length + signature.Length + value.Length];

            // SignatureLength : 4-byte big endian integer
            Array.Copy(signatureLength, 0, message, currentIndex, signatureLength.Length);
            currentIndex += signatureLength.Length;

            // Signature       : Octet stream, length is SignatureLength
            Array.Copy(signature, 0, message, currentIndex, signature.Length);
            currentIndex += signature.Length;

            // CookieValue     : Octet stream, remainder of message
            Array.Copy(value, 0, message, currentIndex, value.Length);

            return message;
        }

        /// <summary>
        /// The default RSACryptoServiceProvider does not support signatures for SHA256. If this is desired, it's necessary to construct a new one.
        /// </summary>
        AsymmetricSignatureFormatter GetSignatureFormatter(RSA rsa)
        {
            RSACryptoServiceProvider rsaProvider = rsa as RSACryptoServiceProvider;
            if (isSha256() && null != rsaProvider)
            {
                return CryptoHelper.GetSignatureFormatterForSha256(rsaProvider);
            }
            else
            {
                //
                // If it's SHA-1 or the RSA is not an RsaCSP, just create a formatter using the original RSA.
                //
                return new RSAPKCS1SignatureFormatter(rsa);
            }
        }

        AsymmetricSignatureDeformatter GetSignatureDeformatter(RSA rsa)
        {
            RSACryptoServiceProvider rsaProvider = rsa as RSACryptoServiceProvider;
            if (isSha256() && null != rsaProvider)
            {
                return CryptoHelper.GetSignatureDeFormatterForSha256(rsaProvider);
            }
            else
            {
                //
                // If it's SHA-1 or the RSA is not an RsaCSP, just create a deformatter using the original RSA.
                //
                return new RSAPKCS1SignatureDeformatter(rsa);
            }
        }

        /// <summary>
        /// Returns true if the hash algorithm is set to SHA256, false otherwise.
        /// </summary>
        /// <returns></returns>
        bool isSha256()
        {
            return (StringComparer.OrdinalIgnoreCase.Equals(HashName, "SHA256")
            || StringComparer.OrdinalIgnoreCase.Equals(HashName, "SHA-256")
            || StringComparer.OrdinalIgnoreCase.Equals(HashName, "System.Security.Cryptography.SHA256"));
        }
    }
}
