//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------


namespace System.IdentityModel
{
    using System;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;
    using System.Runtime;

    /// <summary>
    /// Wraps a writer and generates a signature automatically when the envelope
    /// is written completely. By default the generated signature is inserted as
    /// the last element in the envelope. This can be modified by explicitily 
    /// calling WriteSignature to indicate the location inside the envelope where
    /// the signature should be inserted.
    /// </summary>
    public sealed class EnvelopedSignatureWriter : DelegatingXmlDictionaryWriter
    {
        DictionaryManager _dictionaryManager;
        XmlWriter _innerWriter;
        SigningCredentials _signingCreds;
        string _referenceId;
        SecurityTokenSerializer _tokenSerializer;
        HashStream _hashStream;
        HashAlgorithm _hashAlgorithm;
        int _elementCount;
        MemoryStream _signatureFragment;
        MemoryStream _endFragment;
        bool _hasSignatureBeenMarkedForInsert;
        MemoryStream _writerStream;
        MemoryStream _preCanonicalTracingStream;
        bool _disposed;

        /// <summary>
        /// Initializes an instance of <see cref="EnvelopedSignatureWriter"/>. The returned writer can be directly used
        /// to write the envelope. The signature will be automatically generated when 
        /// the envelope is completed.
        /// </summary>
        /// <param name="innerWriter">Writer to wrap/</param>
        /// <param name="signingCredentials">SigningCredentials to be used to generate the signature.</param>
        /// <param name="referenceId">The reference Id of the envelope.</param>
        /// <param name="securityTokenSerializer">SecurityTokenSerializer to serialize the signature KeyInfo.</param>
        /// <exception cref="ArgumentNullException">One of he input parameter is null.</exception>
        /// <exception cref="ArgumentException">The string 'referenceId' is either null or empty.</exception>
        public EnvelopedSignatureWriter(XmlWriter innerWriter, SigningCredentials signingCredentials, string referenceId, SecurityTokenSerializer securityTokenSerializer)
        {
            if (innerWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerWriter");
            }

            if (signingCredentials == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signingCredentials");
            }

            if (string.IsNullOrEmpty(referenceId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ID0006), "referenceId"));
            }

            if (securityTokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenSerializer");
            }

            // Remember the user's writer here. We need to finally write out the signed XML
            // into this writer.
            _dictionaryManager = new DictionaryManager();
            _innerWriter = innerWriter;
            _signingCreds = signingCredentials;
            _referenceId = referenceId;
            _tokenSerializer = securityTokenSerializer;

            _signatureFragment = new MemoryStream();
            _endFragment = new MemoryStream();
            _writerStream = new MemoryStream();

            XmlDictionaryWriter effectiveWriter = XmlDictionaryWriter.CreateTextWriter(_writerStream, Encoding.UTF8, false);

            // Initialize the base writer to the newly created writer. The user should write the XML
            // to this.
            base.InitializeInnerWriter(effectiveWriter);
            _hashAlgorithm = CryptoHelper.CreateHashAlgorithm(_signingCreds.DigestAlgorithm);
            _hashStream = new HashStream(_hashAlgorithm);
            base.InnerWriter.StartCanonicalization(_hashStream, false, null);

            //
            // Add tracing for the un-canonicalized bytes
            //
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                _preCanonicalTracingStream = new MemoryStream();
                base.InitializeTracingWriter(new XmlTextWriter(_preCanonicalTracingStream, Encoding.UTF8));
            }
        }

        private void ComputeSignature()
        {
            PreDigestedSignedInfo signedInfo = new PreDigestedSignedInfo(_dictionaryManager);
            signedInfo.AddEnvelopedSignatureTransform = true;
            signedInfo.CanonicalizationMethod = XD.ExclusiveC14NDictionary.Namespace.Value;
            signedInfo.SignatureMethod = _signingCreds.SignatureAlgorithm;
            signedInfo.DigestMethod = _signingCreds.DigestAlgorithm;
            signedInfo.AddReference(_referenceId, _hashStream.FlushHashAndGetValue(_preCanonicalTracingStream));

            SignedXml signedXml = new SignedXml(signedInfo, _dictionaryManager, _tokenSerializer);
            signedXml.ComputeSignature(_signingCreds.SigningKey);
            signedXml.Signature.KeyIdentifier = _signingCreds.SigningKeyIdentifier;
            signedXml.WriteTo(base.InnerWriter);
            ((IDisposable)_hashStream).Dispose();
            _hashStream = null;
        }

        private void OnEndRootElement()
        {
            if (!_hasSignatureBeenMarkedForInsert)
            {
                // Default case. Signature is added as the last child element.
                // We still have to compute the signature. Write end element as a different fragment.

                ((IFragmentCapableXmlDictionaryWriter)base.InnerWriter).StartFragment(_endFragment, false);
                base.WriteEndElement();
                ((IFragmentCapableXmlDictionaryWriter)base.InnerWriter).EndFragment();
            }
            else if (_hasSignatureBeenMarkedForInsert)
            {
                // Signature should be added to the middle between the start and element 
                // elements. Finish the end fragment and compute the signature and 
                // write the signature as a seperate fragment.
                base.WriteEndElement();
                ((IFragmentCapableXmlDictionaryWriter)base.InnerWriter).EndFragment();
            }

            // Stop Canonicalization.
            base.EndCanonicalization();

            // Compute signature and write it into a seperate fragment.
            ((IFragmentCapableXmlDictionaryWriter)base.InnerWriter).StartFragment(_signatureFragment, false);
            ComputeSignature();
            ((IFragmentCapableXmlDictionaryWriter)base.InnerWriter).EndFragment();

            // Put all fragments together. The fragment before the signature is already written into the writer.
            ((IFragmentCapableXmlDictionaryWriter)base.InnerWriter).WriteFragment(_signatureFragment.GetBuffer(), 0, (int)_signatureFragment.Length);
            ((IFragmentCapableXmlDictionaryWriter)base.InnerWriter).WriteFragment(_endFragment.GetBuffer(), 0, (int)_endFragment.Length);

            // _startFragment.Close();
            _signatureFragment.Close();
            _endFragment.Close();

            _writerStream.Position = 0;
            _hasSignatureBeenMarkedForInsert = false;

            // Write the signed stream to the writer provided by the user.
            // We are creating a Text Reader over a stream that we just wrote out. Hence, it is safe to 
            // create a XmlTextReader and not a XmlDictionaryReader.
            // Note: reader will close _writerStream on Dispose.
            XmlReader reader = XmlDictionaryReader.CreateTextReader(_writerStream, XmlDictionaryReaderQuotas.Max);
            reader.MoveToContent();
            _innerWriter.WriteNode(reader, false);
            _innerWriter.Flush();
            reader.Close();
            base.Close();
        }

        /// <summary>
        /// Sets the position of the signature within the envelope. Call this
        /// method while writing the envelope to indicate at which point the 
        /// signature should be inserted.
        /// </summary>
        public void WriteSignature()
        {
            base.Flush();
            if (_writerStream == null || _writerStream.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID6029)));
            }

            if (_signatureFragment.Length != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID6030)));
            }

            Fx.Assert(_endFragment != null && _endFragment.Length == 0, SR.GetString(SR.ID8026));

            // Capture the remaing as a seperate fragment.
            ((IFragmentCapableXmlDictionaryWriter)base.InnerWriter).StartFragment(_endFragment, false);

            _hasSignatureBeenMarkedForInsert = true;
        }

        /// <summary>
        /// Overrides the base class implementation. When the last element of the envelope is written
        /// the signature is automatically computed over the envelope and the signature is inserted at
        /// the appropriate position, if WriteSignature was explicitly called or is inserted at the
        /// end of the envelope.
        /// </summary>
        public override void WriteEndElement()
        {
            _elementCount--;
            if (_elementCount == 0)
            {
                base.Flush();
                OnEndRootElement();
            }
            else
            {
                base.WriteEndElement();
            }
        }

        /// <summary>
        /// Overrides the base class implementation. When the last element of the envelope is written
        /// the signature is automatically computed over the envelope and the signature is inserted at
        /// the appropriate position, if WriteSignature was explicitly called or is inserted at the
        /// end of the envelope.
        /// </summary>
        public override void WriteFullEndElement()
        {
            _elementCount--;
            if (_elementCount == 0)
            {
                base.Flush();
                OnEndRootElement();
            }
            else
            {
                base.WriteFullEndElement();
            }
        }

        /// <summary>
        /// Overrides the base class. Writes the specified start tag and associates
        /// it with the given namespace.
        /// </summary>
        /// <param name="prefix">The namespace prefix of the element.</param>
        /// <param name="localName">The local name of the element.</param>
        /// <param name="ns">The namespace URI to associate with the element.</param>
        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            _elementCount++;
            base.WriteStartElement(prefix, localName, ns);
        }

        #region IDisposable Members

        /// <summary>
        /// Releases the unmanaged resources used by the System.IdentityModel.Protocols.XmlSignature.EnvelopedSignatureWriter and optionally
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
                if (_hashStream != null)
                {
                    _hashStream.Dispose();
                    _hashStream = null;
                }

                if (_hashAlgorithm != null)
                {
                    ((IDisposable)_hashAlgorithm).Dispose();
                    _hashAlgorithm = null;
                }

                if (_signatureFragment != null)
                {
                    _signatureFragment.Dispose();
                    _signatureFragment = null;
                }

                if (_endFragment != null)
                {
                    _endFragment.Dispose();
                    _endFragment = null;
                }

                if (_writerStream != null)
                {
                    _writerStream.Dispose();
                    _writerStream = null;
                }

                if (_preCanonicalTracingStream != null)
                {
                    _preCanonicalTracingStream.Dispose();
                    _preCanonicalTracingStream = null;
                }
            }

            // Free native resources, if any.

            _disposed = true;
        }

        #endregion
    }
}
