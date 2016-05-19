//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IdentityModel.Selectors;
using System.IO;
using System.Security.Cryptography;
using System.ServiceModel.Security;
using System.Text;
using System.Xml;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Token handler for an encrypted <see cref="SecurityToken"/> type.
    /// </summary>
    public class EncryptedSecurityTokenHandler : SecurityTokenHandler
    {
        static string[] _tokenTypeIdentifiers = new string[] { null };
        SecurityTokenSerializer _keyInfoSerializer;
        object _syncObject = new object();

        /// <summary>
        /// Create an instance of <see cref="EncryptedSecurityTokenHandler"/>
        /// </summary>
        public EncryptedSecurityTokenHandler()
        {
        }

        /// <summary>
        /// Indicates if the current XML element is pointing to a KeyIdentifierClause that
        /// can be de-serialized by this instance.
        /// </summary>
        /// <param name="reader">An XML reader positioned at the start element. 
        /// The reader should not be advanced.</param>
        /// <returns>true if the XML reader is positioned at an EncryptedKey xml element 
        /// as defined in section 3.5.1 of 'http://www.w3.org/TR/2002/REC-xmlenc-core-20021210'.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is null.</exception>
        public override bool CanReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // <EncryptedKey>
            return reader.IsStartElement(XmlEncryptionConstants.Elements.EncryptedKey, XmlEncryptionConstants.Namespace);
        }

        /// <summary>
        /// Returns true if the reader is pointing to an EncryptedData element.
        /// </summary>
        /// <param name="reader">The reader positioned at a security token.</param>
        /// <returns>true if the reader is positioned at EncryptedData else false.</returns>
        /// <remarks>Does not move the reader when returning false.</remarks>
        public override bool CanReadToken(XmlReader reader)
        {
            return EncryptedDataElement.CanReadFrom(reader);
        }

        /// <summary>
        /// Overrides the base CanWriteToken and returns true always.
        /// </summary>
        public override bool CanWriteToken
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or Sets a SecurityTokenSerializers that will be used to serialize and deserializer
        /// SecurtyKeyIdentifier of the &lt;xenc:EncryptedData> element.
        /// </summary>
        /// <exception cref="ArgumentNullException">Input parameter 'value' is null.</exception>
        public SecurityTokenSerializer KeyInfoSerializer
        {
            get
            {
                if ( _keyInfoSerializer == null )
                {
                    lock ( _syncObject )
                    {
                        if ( _keyInfoSerializer == null )
                        {
                            SecurityTokenHandlerCollection sthc = ( ContainingCollection != null ) ?
                            ContainingCollection : SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
                            _keyInfoSerializer = new SecurityTokenSerializerAdapter(sthc);
                        }
                    }
                }

                return _keyInfoSerializer;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _keyInfoSerializer = value;
            }
        }

        /// <summary>
        /// Reads the encrypted security token.
        /// </summary>
        /// <param name="reader">The reader from which to read the token.</param>
        /// <returns>An instance of <see cref="SecurityToken"/>.</returns>
        /// <exception cref="ArgumentNullException">Input parameter 'reader' is null.</exception>
        /// <exception cref="InvalidOperationException">One of the properties 'Configuration' or 'Configuration.ServiceTokenResolver' is null. This property is required for obtaining keys for decryption.</exception>
        /// <exception cref="SecurityTokenException">A <see cref="SecurityKeyIdentifier"/> is not found inside the xml pointed to by the reader.</exception>
        /// <exception cref="EncryptedTokenDecryptionFailedException">The <see cref="SecurityKeyIdentifier"/> found inside the xml cannot be resolved by Configuration.ServiceTokenResolver to a <see cref="SecurityKey"/>.</exception>
        /// <exception cref="SecurityTokenException">The <see cref="SecurityKeyIdentifier"/> is not a <see cref="SymmetricSecurityKey"/>.</exception>
        /// <exception cref="InvalidOperationException">The ContainingCollection (<see cref="SecurityTokenHandlerCollection"/>) is unable to find a  <see cref="SecurityTokenHandler"/> that is able to read the decrypted xml and return a <see cref="SecurityToken"/>.</exception>
        public override SecurityToken ReadToken(XmlReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }

            if (this.Configuration.ServiceTokenResolver == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4276));
            }

            //
            // Read the encrypted data element
            //
            EncryptedDataElement encryptedData = new EncryptedDataElement(KeyInfoSerializer);
            encryptedData.ReadXml(XmlDictionaryReader.CreateDictionaryReader(reader));

            //
            // All the clauses in a keyinfo must identify the same key, so we 
            // can try each clause in turn and stop when one resolves.
            //
            SecurityKey decryptionKey = null;
            foreach (SecurityKeyIdentifierClause clause in encryptedData.KeyIdentifier)
            {
                this.Configuration.ServiceTokenResolver.TryResolveSecurityKey(clause, out decryptionKey);

                if (null != decryptionKey)
                {
                    break;
                }
            }

            //
            // Try to use the SKI to create the key instead.
            //
            if (decryptionKey == null && encryptedData.KeyIdentifier.CanCreateKey)
            {
                decryptionKey = encryptedData.KeyIdentifier.CreateKey();
            }

            //
            // Fail if none of the clauses resolved or ski itself cannot create key.
            //
            if (null == decryptionKey)
            {
                EncryptedKeyIdentifierClause encryptedKeyClause;
                if (encryptedData.KeyIdentifier.TryFind<EncryptedKeyIdentifierClause>(out encryptedKeyClause))
                {
                    //
                    // System.IdentityModel.Tokens.EncryptedKeyIdentifierClause.ToString() does not print out 
                    // very good information except the cipher data in this case. We have worked around that
                    // by using the token serializer to serialize the key identifier clause again.
                    //
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new EncryptedTokenDecryptionFailedException(
                            SR.GetString(SR.ID4036, XmlUtil.SerializeSecurityKeyIdentifier(encryptedData.KeyIdentifier, base.ContainingCollection.KeyInfoSerializer))));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new EncryptedTokenDecryptionFailedException(SR.GetString(SR.ID4036, encryptedData.KeyIdentifier.ToString())));
                }
            }

            //
            // Need a symmetric key
            //
            SymmetricSecurityKey symmetricKey = decryptionKey as SymmetricSecurityKey;
            if (null == symmetricKey)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SecurityTokenException(SR.GetString(SR.ID4023)));
            }

            //
            // Do the actual decryption
            //
            byte[] plainText;

            using (SymmetricAlgorithm decrypter = symmetricKey.GetSymmetricAlgorithm(encryptedData.Algorithm))
            {
                plainText = encryptedData.Decrypt(decrypter);
            }

            DebugEncryptedTokenClearText(plainText, Encoding.UTF8);

            //
            // Read and return the plaintext token
            //
            using (XmlReader innerTokenReader = XmlDictionaryReader.CreateTextReader(plainText, XmlDictionaryReaderQuotas.Max))
            {
                if (this.ContainingCollection != null && this.ContainingCollection.CanReadToken(innerTokenReader))
                {
                    return this.ContainingCollection.ReadToken(innerTokenReader);
                }
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4014, innerTokenReader.LocalName, innerTokenReader.NamespaceURI));
            }
        }

        /// <summary>
        /// Reads an EncryptedKeyIdentifierClause from a XML stream.
        /// </summary>
        /// <param name="reader">An XML reader positioned at an EncryptedKey element as defined in 'http://www.w3.org/TR/2002/REC-xmlenc-core-20021210' .</param>
        /// <returns>SecurityKeyIdentifierClause instance of type EncryptedKeyIdentifierClause.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is null.</exception>
        /// <exception cref="InvalidOperationException">If the <paramref name="reader"/> is not positioned at an EncryptedKey element.</exception>
        public override SecurityKeyIdentifierClause ReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            // <EncryptedKey>
            if (reader.IsStartElement(XmlEncryptionConstants.Elements.EncryptedKey, XmlEncryptionConstants.Namespace))
            {
                EncryptedKeyElement encryptedKey = new EncryptedKeyElement(KeyInfoSerializer);
                encryptedKey.ReadXml(XmlDictionaryReader.CreateDictionaryReader(reader));
                return new EncryptedKeyIdentifierClause(encryptedKey.CipherData.CipherValue, encryptedKey.Algorithm, encryptedKey.KeyIdentifier);
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID3275, reader.Name, reader.NamespaceURI)));
        }

        [Conditional("DEBUG")]
        static void DebugEncryptedTokenClearText(byte[] bytes, Encoding encoding)
        {
            string text = encoding.GetString(bytes);
            Debug.WriteLine(text.Substring(0, 40));
        }

        /// <summary>
        /// Gets the System.Type of the token that this SecurityTokenHandler handles.
        /// Returns typeof <see cref="EncryptedSecurityToken"/> by default.
        /// </summary>
        public override Type TokenType
        {
            get { return typeof(EncryptedSecurityToken); }
        }

        /// <summary>
        /// By default returns an array with a single null string as there isn't any specific TokenType identifier that is 
        /// associated with a <see cref="EncryptedSecurityToken"/>.
        /// </summary>
        public override string[] GetTokenTypeIdentifiers()
        {
            return _tokenTypeIdentifiers;
        }

        /// <summary>
        /// Writes a <see cref="EncryptedSecurityToken"/> using the xmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to which the encrypted token is written.</param>
        /// <param name="token">The <see cref="SecurityToken"/> which must be an instance of <see cref="EncryptedSecurityToken"/>.</param>
        /// <exception cref="ArgumentNullException">The input prameter 'writer' is null.</exception>
        /// <exception cref="ArgumentNullException">The input prameter 'token' is null.</exception>
        /// <exception cref="ArgumentException">The <see cref="SecurityToken"/> is not an instance of <see cref="EncryptedSecurityToken"/>.</exception>
        /// <exception cref="InvalidOperationException">The property 'Configuration' is null. This property is required for obtaining keys for encryption.</exception>
        /// <exception cref="InvalidOperationException">The ContaingCollection was unable to find a <see cref="SecurityTokenHandler"/> that is able to write
        /// the <see cref="SecurityToken"/> returned by 'EncryptedSecurityToken.Token'.</exception>
        /// <exception cref="SecurityTokenException">The property 'EncryptinCredentials.SecurityKey is not a <see cref="SymmetricSecurityKey"/></exception>
        public override void WriteToken(XmlWriter writer, SecurityToken token)
        {
            if (null == writer)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (null == token)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }

            EncryptedSecurityToken encryptedToken = token as EncryptedSecurityToken;
            if (null == encryptedToken)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID4024));
            }

            if (this.ContainingCollection == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4279));
            }

            //
            // This implementation simply wraps the token in xenc:EncryptedData
            //
            EncryptedDataElement encryptedData = new EncryptedDataElement(KeyInfoSerializer);
            using (MemoryStream plaintextStream = new MemoryStream())
            {
                //
                // Buffer the plaintext
                //
                using (XmlDictionaryWriter plaintextWriter = XmlDictionaryWriter.CreateTextWriter(plaintextStream, Encoding.UTF8, false))
                {
                    SecurityTokenHandler securityTokenHandler = this.ContainingCollection[encryptedToken.Token.GetType()];
                    if (securityTokenHandler != null)
                    {
                        securityTokenHandler.WriteToken(plaintextWriter, encryptedToken.Token);
                    }
                    else
                    {
                        throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4224, encryptedToken.Token.GetType()));
                    }
                }

                //
                // Set up the EncryptedData element
                //
                EncryptingCredentials encryptingCredentials = encryptedToken.EncryptingCredentials;
                encryptedData.Type = XmlEncryptionConstants.EncryptedDataTypes.Element;
                encryptedData.KeyIdentifier = encryptingCredentials.SecurityKeyIdentifier;
                encryptedData.Algorithm = encryptingCredentials.Algorithm;

                //
                // Get the encryption key, which must be symmetric
                //
                SymmetricSecurityKey encryptingKey = encryptingCredentials.SecurityKey as SymmetricSecurityKey;
                if (encryptingKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID3064)));
                }

                //
                // Do the actual encryption
                //
                using (SymmetricAlgorithm symmetricAlgorithm = encryptingKey.GetSymmetricAlgorithm(encryptingCredentials.Algorithm))
                {
                    byte[] plainTextBytes = plaintextStream.GetBuffer();
                    DebugEncryptedTokenClearText(plainTextBytes, Encoding.UTF8);
                    encryptedData.Encrypt(symmetricAlgorithm, plainTextBytes, 0, (int)plaintextStream.Length);
                }
            }

            //
            // Write the EncryptedData element
            //
            encryptedData.WriteXml(writer, KeyInfoSerializer);
        }
    }
}
