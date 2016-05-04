//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Security.Cryptography.X509Certificates;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// This class defines the encrypted key encrypting credentials. They are usually used 
    /// as data encrypting credentials to encrypt things like token. 
    /// </summary>
    public class EncryptedKeyEncryptingCredentials : EncryptingCredentials
    {
        EncryptingCredentials _wrappingCredentials;
        byte[] _keyBytes;

        /// <summary>
        /// Use this constructor if user wants to use the default wrapping algorithm and encryption algorithm, 
        /// which are RSA-OAEP and AES256 respectively.
        /// </summary>
        /// <param name="certificate">The certificate used to encrypt the key.</param>
        public EncryptedKeyEncryptingCredentials( X509Certificate2 certificate )
            : this( new X509EncryptingCredentials( certificate ), SecurityAlgorithms.DefaultSymmetricKeyLength, SecurityAlgorithms.DefaultEncryptionAlgorithm )
        {
        }

        /// <summary>
        /// Use this contructor if users want to supply their own wrapping algorithm and encryption algorithm
        /// and wrapping credentials is x509 certificate.
        /// </summary>
        /// <param name="certificate">The certificate used to encrypt the session key.</param>
        /// <param name="keyWrappingAlgorithm">The key wrapping algorithm. This should be asymmetric algorithm.</param>
        /// <param name="keySizeInBits">The key size of the wrapped session key.</param>
        /// <param name="encryptionAlgorithm">The encryption algorithm when session key is used. This should be symmetric key algorithm.</param>
        public EncryptedKeyEncryptingCredentials( X509Certificate2 certificate, string keyWrappingAlgorithm, int keySizeInBits, string encryptionAlgorithm )
            : this( new X509EncryptingCredentials( certificate, keyWrappingAlgorithm ), keySizeInBits, encryptionAlgorithm )
        {
        }

        /// <summary>
        /// Use this constructor if users already have an encryting credentials and want to use that as a wrapping credentials.
        /// </summary>
        /// <param name="wrappingCredentials">The key wrapping credentials used to encrypt the session key.</param>
        /// <param name="keySizeInBits">The key size of the wrapped session key.</param>
        /// <param name="encryptionAlgorithm">The encryption algorithm when session key is used. This should be symmetric key algorithm.</param>
        /// <exception cref="ArgumentNullException">When the wrappingCredentials is null.</exception>
        public EncryptedKeyEncryptingCredentials( EncryptingCredentials wrappingCredentials, int keySizeInBits, string encryptionAlgorithm )
        {
            if ( wrappingCredentials == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "wrappingCredentials" );
            }

            //
            // Key materials
            //
            if ( encryptionAlgorithm == SecurityAlgorithms.DesEncryption ||
                 encryptionAlgorithm == SecurityAlgorithms.TripleDesEncryption ||
                 encryptionAlgorithm == SecurityAlgorithms.TripleDesKeyWrap )
            {
                _keyBytes = CryptoHelper.KeyGenerator.GenerateDESKey( keySizeInBits );
            }
            else
            {
                _keyBytes = CryptoHelper.KeyGenerator.GenerateSymmetricKey( keySizeInBits );
            }
            base.SecurityKey = new InMemorySymmetricSecurityKey( _keyBytes );

            //
            // Wrapping key
            //
            _wrappingCredentials = wrappingCredentials;

            //
            // key identifier
            //
            byte[] encryptedKey = _wrappingCredentials.SecurityKey.EncryptKey( _wrappingCredentials.Algorithm, _keyBytes );
            base.SecurityKeyIdentifier = new SecurityKeyIdentifier( new EncryptedKeyIdentifierClause( encryptedKey, _wrappingCredentials.Algorithm, _wrappingCredentials.SecurityKeyIdentifier ) );

            //
            // encryption algorithm
            //
            base.Algorithm = encryptionAlgorithm;
        }

        /// <summary>
        /// Gets the key wrapping credentials used to encrypt the session key, for example,
        /// X509EncryptingCredentials.
        /// </summary>
        public EncryptingCredentials WrappingCredentials
        {
            get
            {
                return _wrappingCredentials;
            }
        }
    }
}
