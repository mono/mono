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
    /// This class implements a deserialization for: EncryptedData as defined in section 3.4 of http://www.w3.org/TR/2002/REC-xmlenc-core-2002120
    /// </summary>
    internal class EncryptedDataElement : EncryptedTypeElement
    {
        public static bool CanReadFrom( XmlReader reader )
        {
            return reader != null && reader.IsStartElement(
                XmlEncryptionConstants.Elements.EncryptedData,
                XmlEncryptionConstants.Namespace );
        }

        public EncryptedDataElement()
            : this( null )
        {
        }

        public EncryptedDataElement( SecurityTokenSerializer tokenSerializer )
            : base( tokenSerializer )
        {
            KeyIdentifier = new SecurityKeyIdentifier( new EmptySecurityKeyIdentifierClause() );
        }

        /// <summary>
        /// Decrypts the data
        /// </summary>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">When algorithm is null</exception>
        /// <exception cref="InvalidOperationException">When no cipher data has been read</exception>
        public byte[] Decrypt( SymmetricAlgorithm algorithm )
        {
            if ( algorithm == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "algorithm" );
            }

            if ( CipherData == null || CipherData.CipherValue == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new InvalidOperationException( SR.GetString( SR.ID6000 ) ) );
            }

            byte[] cipherText = CipherData.CipherValue;

            return ExtractIVAndDecrypt( algorithm, cipherText, 0, cipherText.Length );
        }

        public void Encrypt( SymmetricAlgorithm algorithm, byte[] buffer, int offset, int length )
        {
            byte[] iv;
            byte[] cipherText;
            GenerateIVAndEncrypt( algorithm, buffer, offset, length, out iv, out cipherText );
            CipherData.SetCipherValueFragments( iv, cipherText );
        }

        static byte[] ExtractIVAndDecrypt( SymmetricAlgorithm algorithm, byte[] cipherText, int offset, int count )
        {
            byte[] iv = new byte[algorithm.BlockSize / 8];

            //
            // Make sure cipherText has enough bytes after the offset, for Buffer.BlockCopy to copy.
            //
            if ( cipherText.Length - offset < iv.Length )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new InvalidOperationException( SR.GetString( SR.ID6019, cipherText.Length - offset, iv.Length ) ) );
            }

            Buffer.BlockCopy( cipherText, offset, iv, 0, iv.Length );

            algorithm.Padding = PaddingMode.ISO10126;
            algorithm.Mode = CipherMode.CBC;

            ICryptoTransform decrTransform = null;
            byte[] plainText = null;

            try
            {
                decrTransform = algorithm.CreateDecryptor( algorithm.Key, iv );
                plainText = decrTransform.TransformFinalBlock( cipherText, offset + iv.Length, count - iv.Length );
            }
            finally
            {
                if ( decrTransform != null )
                {
                    decrTransform.Dispose();
                }
            }

            return plainText;
        }

        static void GenerateIVAndEncrypt( SymmetricAlgorithm algorithm, byte[] plainText, int offset, int length, out byte[] iv, out byte[] cipherText )
        {
            RandomNumberGenerator random = CryptoHelper.RandomNumberGenerator;
            int ivSize = algorithm.BlockSize / 8;
            iv = new byte[ivSize];
            random.GetBytes( iv );
            algorithm.Padding = PaddingMode.PKCS7;
            algorithm.Mode = CipherMode.CBC;
            ICryptoTransform encrTransform = algorithm.CreateEncryptor( algorithm.Key, iv );
            cipherText = encrTransform.TransformFinalBlock( plainText, offset, length );
            encrTransform.Dispose();
        }

        public override void ReadExtensions( XmlDictionaryReader reader )
        {
            // nothing to do here
        }

        /// <summary>
        /// Reads an EncryptedData element
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">When reader is null</exception>
        /// <exception cref="ArgumentNullException">When securityTokenSerializer is null</exception>
        public override void ReadXml( XmlDictionaryReader reader )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            reader.MoveToContent();
            if ( !reader.IsStartElement( XmlEncryptionConstants.Elements.EncryptedData, XmlEncryptionConstants.Namespace ) )
            {
                throw DiagnosticUtility.ThrowHelperXml( reader, SR.GetString( SR.ID4193 ) );
            }

            // <EncryptedData> extends <EncryptedType>
            // base will read the start element and the end element.
            base.ReadXml( reader );

        }

        /// <summary>
        /// Writes the EncryptedData element
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="securityTokenSerializer"></param>
        /// <exception cref="ArgumentNullException">When securityTokenSerializer is null</exception>
        /// <exception cref="InvalidOperationException">When KeyIdentifier is null</exception>
        public virtual void WriteXml( XmlWriter writer, SecurityTokenSerializer securityTokenSerializer )
        {
            if ( writer == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "writer" );
            }

            if ( securityTokenSerializer == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "securityTokenSerializer" );
            }

            if ( KeyIdentifier == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new InvalidOperationException( SR.GetString( SR.ID6001 ) ) );
            }

            // <EncryptedData>
            writer.WriteStartElement( XmlEncryptionConstants.Prefix, XmlEncryptionConstants.Elements.EncryptedData, XmlEncryptionConstants.Namespace );

            if ( !string.IsNullOrEmpty( Id ) )
            {
                writer.WriteAttributeString( XmlEncryptionConstants.Attributes.Id, null, Id );
            }

            if ( !string.IsNullOrEmpty( Type ) )
            {
                writer.WriteAttributeString( XmlEncryptionConstants.Attributes.Type, null, Type );
            }

            if ( EncryptionMethod != null )
            {
                EncryptionMethod.WriteXml( writer );
            }

            if ( KeyIdentifier != null )
            {
                securityTokenSerializer.WriteKeyIdentifier( XmlDictionaryWriter.CreateDictionaryWriter( writer ), KeyIdentifier );
            }

            CipherData.WriteXml( writer );

            // <EncryptedData> 
            writer.WriteEndElement();
        }
    }
}
