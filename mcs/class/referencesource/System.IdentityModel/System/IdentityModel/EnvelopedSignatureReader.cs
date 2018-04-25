//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.IdentityModel
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.Xml;

    /// <summary>
    /// Wraps a reader pointing to a enveloped signed XML and provides
    /// a reader that can be used to read the content without having to
    /// process the signature. The Signature is automatically validated
    /// when the last element of the envelope is read.
    /// </summary>
    public sealed class EnvelopedSignatureReader : DelegatingXmlDictionaryReader
    {
        bool _automaticallyReadSignature;
        DictionaryManager _dictionaryManager;
        int _elementCount;
        bool _resolveIntrinsicSigningKeys;
        bool _requireSignature;
        SigningCredentials _signingCredentials;
        SecurityTokenResolver _signingTokenResolver;
        SignedXml _signedXml;
        SecurityTokenSerializer _tokenSerializer;
        WrappedReader _wrappedReader;
        bool _disposed;

        /// <summary>
        /// Initializes an instance of <see cref="EnvelopedSignatureReader"/>
        /// </summary>
        /// <param name="reader">Reader pointing to the enveloped signed XML.</param>
        /// <param name="securityTokenSerializer">Token Serializer to resolve the signing token.</param>
        public EnvelopedSignatureReader(XmlReader reader, SecurityTokenSerializer securityTokenSerializer)
            : this(reader, securityTokenSerializer, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="EnvelopedSignatureReader"/>
        /// </summary>
        /// <param name="reader">Reader pointing to the enveloped signed XML.</param>
        /// <param name="securityTokenSerializer">Token Serializer to deserialize the KeyInfo of the Signature.</param>
        /// <param name="signingTokenResolver">Token Resolver to resolve the signing token.</param>
        /// <exception cref="ArgumentNullException">One of the input parameter is null.</exception>
        public EnvelopedSignatureReader(XmlReader reader, SecurityTokenSerializer securityTokenSerializer, SecurityTokenResolver signingTokenResolver)
            : this(reader, securityTokenSerializer, signingTokenResolver, true, true, true)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="EnvelopedSignatureReader"/>
        /// </summary>
        /// <param name="reader">Reader pointing to the enveloped signed XML.</param>
        /// <param name="securityTokenSerializer">Token Serializer to deserialize the KeyInfo of the Signature.</param>
        /// <param name="signingTokenResolver">Token Resolver to resolve the signing token.</param>
        /// <param name="requireSignature">The value indicates whether the signature is optional.</param>
        /// <param name="automaticallyReadSignature">This value indicates if the Signature should be read 
        /// when the Signature element is encountered or allow the caller to read the Signature manually.</param>
        /// <param name="resolveIntrinsicSigningKeys">A value indicating if intrinsic signing keys should be resolved.</param>
        public EnvelopedSignatureReader(XmlReader reader, SecurityTokenSerializer securityTokenSerializer, SecurityTokenResolver signingTokenResolver, bool requireSignature, bool automaticallyReadSignature, bool resolveIntrinsicSigningKeys)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (securityTokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenSerializer");
            }

            _automaticallyReadSignature = automaticallyReadSignature;
            _dictionaryManager = new DictionaryManager();
            _tokenSerializer = securityTokenSerializer;
            _requireSignature = requireSignature;
            _signingTokenResolver = signingTokenResolver ?? EmptySecurityTokenResolver.Instance;
            _resolveIntrinsicSigningKeys = resolveIntrinsicSigningKeys;

            XmlDictionaryReader dictionaryReader = XmlDictionaryReader.CreateDictionaryReader(reader);
            _wrappedReader = new WrappedReader(dictionaryReader);

            base.InitializeInnerReader(_wrappedReader);
        }

        void OnEndOfRootElement()
        {
            if (null == _signedXml)
            {
                if (_requireSignature)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new CryptographicException(SR.GetString(SR.ID3089)));
                }
            }
            else
            {
                ResolveSigningCredentials();
                _signedXml.StartSignatureVerification(_signingCredentials.SigningKey);
                _wrappedReader.XmlTokens.SetElementExclusion(XD.XmlSignatureDictionary.Signature.Value, XD.XmlSignatureDictionary.Namespace.Value);
                WifSignedInfo signedInfo = _signedXml.Signature.SignedInfo as WifSignedInfo;
                _signedXml.EnsureDigestValidity(signedInfo[0].ExtractReferredId(), _wrappedReader);
                _signedXml.CompleteSignatureVerification();                
            }
        }

        /// <summary>
        /// Returns the SigningCredentials used in the signature after the 
        /// envelope is consumed and when the signature is validated.
        /// </summary>
        public SigningCredentials SigningCredentials
        {
            get
            {
                return _signingCredentials;
            }
        }

        /// <summary>
        /// Gets a XmlBuffer of the envelope that was enveloped signed.
        /// The buffer is available after the XML has been read and
        /// signature validated.
        /// </summary>
        internal XmlTokenStream XmlTokens
        {
            get
            {
                return _wrappedReader.XmlTokens.Trim();
            }
        }

        /// <summary>
        /// Overrides the base Read method. Checks if the end of the envelope is reached and 
        /// validates the signature if requireSignature is enabled. If the reader gets
        /// positioned on a Signature element the whole signature is read in if automaticallyReadSignature
        /// is enabled.
        /// </summary>
        /// <returns>true if the next node was read successfully; false if there are no more nodes</returns>
        public override bool Read()
        {
            if ((base.NodeType == XmlNodeType.Element) && (!base.IsEmptyElement))
            {
                _elementCount++;
            }

            if (base.NodeType == XmlNodeType.EndElement)
            {
                _elementCount--;
                if (_elementCount == 0)
                {
                    OnEndOfRootElement();
                }
            }

            bool result = base.Read();
            if (_automaticallyReadSignature
                && (_signedXml == null)
                && result
                && base.InnerReader.IsLocalName(XD.XmlSignatureDictionary.Signature)
                && base.InnerReader.IsNamespaceUri(XD.XmlSignatureDictionary.Namespace))
            {
                ReadSignature();
            }

            return result;
        }

        void ReadSignature()
        {
            _signedXml = new SignedXml(new WifSignedInfo(_dictionaryManager), _dictionaryManager, _tokenSerializer);
            _signedXml.TransformFactory = ExtendedTransformFactory.Instance;

            _signedXml.ReadFrom(_wrappedReader);

            if (_signedXml.Signature.SignedInfo.ReferenceCount != 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.ID3057)));
            }
        }

        void ResolveSigningCredentials()
        {
            if (_signedXml.Signature == null || _signedXml.Signature.KeyIdentifier == null || _signedXml.Signature.KeyIdentifier.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID3276)));
            }

            SecurityKey signingKey = null;
            if (!_signingTokenResolver.TryResolveSecurityKey(_signedXml.Signature.KeyIdentifier[0], out signingKey))
            {
                if (_resolveIntrinsicSigningKeys && _signedXml.Signature.KeyIdentifier.CanCreateKey)
                {
                    signingKey = _signedXml.Signature.KeyIdentifier.CreateKey();
                }
                else
                {
                    //
                    // we cannot find the signing key to verify the signature
                    //
                    EncryptedKeyIdentifierClause encryptedKeyClause;
                    if (_signedXml.Signature.KeyIdentifier.TryFind<EncryptedKeyIdentifierClause>(out encryptedKeyClause))
                    {
                        //
                        // System.IdentityModel.Tokens.EncryptedKeyIdentifierClause.ToString() does not print out 
                        // very good information except the cipher data in this case. We have worked around that
                        // by using the token serializer to serialize the key identifier clause again.
                        //
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new SignatureVerificationFailedException(
                                SR.GetString(SR.ID4036, XmlUtil.SerializeSecurityKeyIdentifier(_signedXml.Signature.KeyIdentifier, _tokenSerializer))));
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new SignatureVerificationFailedException(SR.GetString(SR.ID4037, _signedXml.Signature.KeyIdentifier.ToString())));
                    }

                }
            }

            WifSignedInfo signedInfo = _signedXml.Signature.SignedInfo as WifSignedInfo;
            _signingCredentials = new SigningCredentials(signingKey, _signedXml.Signature.SignedInfo.SignatureMethod, signedInfo[0].DigestMethod, _signedXml.Signature.KeyIdentifier);
        }

        /// <summary>
        /// Reads the signature if the reader is currently positioned at a Signature element.
        /// </summary>
        /// <returns>true if the signature was successfully read else false.</returns>
        /// <remarks>Does not move the reader when returning false.</remarks>
        public bool TryReadSignature()
        {
            if (IsStartElement(XD.XmlSignatureDictionary.Signature, XD.XmlSignatureDictionary.Namespace))
            {
                ReadSignature();
                return true;
            }
            return false;
        }

        #region IDisposable Members

        /// <summary>
        /// Releases the unmanaged resources used by the System.IdentityModel.Protocols.XmlSignature.EnvelopedSignatureReader and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                //
                // Free all of our managed resources
                //

                if (_wrappedReader != null)
                {
                    _wrappedReader.Close();
                    _wrappedReader = null;
                }
            }

            // Free native resources, if any.

            _disposed = true;
        }

        #endregion
    }
}
