//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace System.IdentityModel
{
    /// <summary>
    /// Encrypts a cookie using <see cref="RSA"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Cookies encrypted with this transform may be decrypted 
    /// by any machine that shares the same RSA private key (generally 
    /// associated with an X509 certificate).
    /// </para>
    /// <para>
    /// The given data is encrypted using a random AES256 key.  This key is
    /// then encrypted using RSA, and the RSA public key is sent in plain text
    /// so that when decoding the class knows which RSA key to use.
    /// </para>
    /// </remarks>
    public class RsaEncryptionCookieTransform : CookieTransform
    {
        //
        // Produces an encrypted stream as follows:
        // 
        // Hashsha?( RSA.ToString( false ) ) +
        // Length( EncryptRSA( Key + IV )    +
        // EncryptRSA( Key + IV )            +
        // Length( EncryptAES( Data )        +
        // EncryptAES( Data )
        // 

        RSA _encryptionKey;
        List<RSA> _decryptionKeys = new List<RSA>();
        string _hashName = "SHA256";

        /// <summary>
        /// Creates a new instance of <see cref="RsaEncryptionCookieTransform"/>.
        /// </summary>
        /// <param name="key">The provided key will be used as the encryption and decryption key by default.</param>
        /// <exception cref="ArgumentNullException">When the key is null.</exception>
        public RsaEncryptionCookieTransform( RSA key )
        {
            if ( null == key )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "key" );
            }
            _encryptionKey = key;
            _decryptionKeys.Add( _encryptionKey );
        }

        /// <summary>
        /// Creates a new instance of <see cref="RsaEncryptionCookieTransform"/>
        /// </summary>
        /// <param name="certificate">Certificate whose private key is used to encrypt and decrypt.</param>
        /// <exception cref="ArgumentNullException">When certificate is null.</exception>
        /// <exception cref="ArgumentException">When the certificate has no private key.</exception>
        /// <exception cref="ArgumentException">When the certificate's key is not RSA.</exception>
        public RsaEncryptionCookieTransform( X509Certificate2 certificate )
        {
            if ( null == certificate )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "certificate" );
            }
            _encryptionKey = X509Util.EnsureAndGetPrivateRSAKey( certificate );
            _decryptionKeys.Add( _encryptionKey );
        }

        /// <summary>
        /// Creates a new instance of <see cref="RsaEncryptionCookieTransform"/>.
        /// The instance created by this constructor is not usable until the signing and verification keys are set.
        /// </summary>
        internal RsaEncryptionCookieTransform()
        {
        }

        /// <summary>
        /// Gets or sets the RSA key used for encryption
        /// </summary>
        public virtual RSA EncryptionKey
        {
            get { return _encryptionKey; }
            set
            {
                _encryptionKey = value;
                _decryptionKeys = new List<RSA>( new RSA[] { _encryptionKey });
            }
        }

        /// <summary>
        /// Gets the keys used for decryption
        /// By default, this property returns a list containing only the encryption key.
        /// </summary>
        protected virtual ReadOnlyCollection<RSA> DecryptionKeys
        {
            get
            {
                return _decryptionKeys.AsReadOnly();
            }
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
                using ( HashAlgorithm algorithm = CryptoHelper.CreateHashAlgorithm( value ) )
                {
                    if ( algorithm == null )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "value", SR.GetString( SR.ID6034, value ) );
                    }
                    _hashName = value;
                }
            }
        }

        /// <summary>
        /// Decrypts data using the provided RSA key(s) to decrypt an AES key, which decrypts the cookie.
        /// </summary>
        /// <param name="encoded">The encoded data</param>
        /// <returns>The decoded data</returns>
        /// <exception cref="ArgumentNullException">The argument 'encoded' is null.</exception>
        /// <exception cref="ArgumentException">The argument 'encoded' contains zero bytes.</exception>
        /// <exception cref="NotSupportedException">The platform does not support the requested algorithm.</exception>
        /// <exception cref="InvalidOperationException">There are no decryption keys or none of the keys match.</exception>
        public override byte[] Decode( byte[] encoded )
        {
            if ( null == encoded )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "encoded" );
            }

            if ( 0 == encoded.Length )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "encoded", SR.GetString( SR.ID6045 ) );
            }

            ReadOnlyCollection<RSA> decryptionKeys = DecryptionKeys;

            if ( 0 == decryptionKeys.Count )
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID6039 ) );
            }

            byte[] encryptedKeyAndIV;
            byte[] encryptedData;
            byte[] rsaHash;
            RSA rsaDecryptionKey = null;

            using ( HashAlgorithm hash = CryptoHelper.CreateHashAlgorithm( _hashName ) )
            {
                int hashSizeInBytes = hash.HashSize / 8;
                using ( BinaryReader br = new BinaryReader( new MemoryStream( encoded ) ) )
                {
                    rsaHash = br.ReadBytes( hashSizeInBytes );
                    int encryptedKeyAndIVSize = br.ReadInt32();
                    if ( encryptedKeyAndIVSize < 0 )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new FormatException( SR.GetString( SR.ID1006, encryptedKeyAndIVSize ) ) );
                    }
                    //
                    // Enforce upper limit on key size to prevent large buffer allocation in br.ReadBytes()
                    //

                    if ( encryptedKeyAndIVSize > encoded.Length )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new FormatException( SR.GetString( SR.ID1007 ) ) );
                    }
                    encryptedKeyAndIV = br.ReadBytes( encryptedKeyAndIVSize );

                    int encryptedDataSize = br.ReadInt32();
                    if ( encryptedDataSize < 0 )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new FormatException( SR.GetString( SR.ID1008, encryptedDataSize ) ) );
                    }
                    //
                    // Enforce upper limit on data size to prevent large buffer allocation in br.ReadBytes()
                    //
                    if ( encryptedDataSize > encoded.Length )
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new FormatException( SR.GetString( SR.ID1009 ) ) );
                    }

                    encryptedData = br.ReadBytes( encryptedDataSize );
                }

                //
                // Find the decryption key matching the one in XML
                //
                foreach ( RSA key in decryptionKeys )
                {
                    byte[] hashedKey = hash.ComputeHash( Encoding.UTF8.GetBytes( key.ToXmlString( false ) ) );
                    if ( CryptoHelper.IsEqual( hashedKey, rsaHash ) )
                    {
                        rsaDecryptionKey = key;
                        break;
                    }
                }
            }

            if ( rsaDecryptionKey == null )
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID6040 ) );
            }

            RSACryptoServiceProvider rsaProvider = rsaDecryptionKey as RSACryptoServiceProvider;

            if ( rsaProvider == null )
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID6041 ) );
            }

            byte[] decryptedKeyAndIV = rsaProvider.Decrypt( encryptedKeyAndIV, true );

            using (SymmetricAlgorithm symmetricAlgorithm = CryptoHelper.NewDefaultEncryption())
            {

                byte[] decryptionKey = new byte[symmetricAlgorithm.KeySize / 8];

                //
                // Ensure there is sufficient length in the descrypted key and IV buffer for an IV.
                //
                if (decryptedKeyAndIV.Length < decryptionKey.Length)
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID6047, decryptedKeyAndIV.Length, decryptionKey.Length));
                }

                byte[] decryptionIV = new byte[decryptedKeyAndIV.Length - decryptionKey.Length];

                //
                // Copy key into its own buffer.
                // The remaining bytes are the IV copy those into a buffer as well.
                //
                Array.Copy(decryptedKeyAndIV, decryptionKey, decryptionKey.Length);
                Array.Copy(decryptedKeyAndIV, decryptionKey.Length, decryptionIV, 0, decryptionIV.Length);

                using (ICryptoTransform decryptor = symmetricAlgorithm.CreateDecryptor(decryptionKey, decryptionIV))
                {
                    return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                }
            }
        }

        /// <summary>
        /// Encode the data.  The data is encrypted using the default encryption algorithm (AES-256), 
        /// then the AES key is encrypted using RSA and the RSA public key is appended.
        /// </summary>
        /// <param name="value">The data to encode</param>
        /// <exception cref="ArgumentNullException">The argument 'value' is null.</exception>
        /// <exception cref="ArgumentException">The argument 'value' contains zero bytes.</exception>
        /// <exception cref="InvalidOperationException">The EncryptionKey is null.</exception>
        /// <returns>Encoded data</returns>
        public override byte[] Encode( byte[] value )
        {
            if ( null == value )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "value" );
            }

            if ( 0 == value.Length )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument( "value", SR.GetString( SR.ID6044 ) );
            }

            RSA encryptionKey = EncryptionKey;

            if ( null == encryptionKey )
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID6043 ) );
            }

            byte[] rsaHash;
            byte[] encryptedKeyAndIV;
            byte[] encryptedData;

            using ( HashAlgorithm hash = CryptoHelper.CreateHashAlgorithm( _hashName ) )
            {
                rsaHash = hash.ComputeHash( Encoding.UTF8.GetBytes( encryptionKey.ToXmlString( false ) ) );
            }

            using ( SymmetricAlgorithm encryptionAlgorithm = CryptoHelper.NewDefaultEncryption() )
            {
                encryptionAlgorithm.GenerateIV();
                encryptionAlgorithm.GenerateKey();

                using (ICryptoTransform encryptor = encryptionAlgorithm.CreateEncryptor())
                {
                    encryptedData = encryptor.TransformFinalBlock(value, 0, value.Length);
                }

                RSACryptoServiceProvider provider = encryptionKey as RSACryptoServiceProvider;

                if ( provider == null )
                {
                    throw DiagnosticUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID6041 ) );
                }

                //
                // Concatenate the Key and IV in an attempt to avoid two minimum block lengths in the cookie
                //
                byte[] keyAndIV = new byte[encryptionAlgorithm.Key.Length + encryptionAlgorithm.IV.Length];
                Array.Copy( encryptionAlgorithm.Key, keyAndIV, encryptionAlgorithm.Key.Length );
                Array.Copy( encryptionAlgorithm.IV, 0, keyAndIV, encryptionAlgorithm.Key.Length, encryptionAlgorithm.IV.Length );

                encryptedKeyAndIV = provider.Encrypt( keyAndIV, true );
            }

            using ( MemoryStream ms = new MemoryStream() )
            {
                using ( BinaryWriter bw = new BinaryWriter( ms ) )
                {
                    bw.Write( rsaHash );
                    bw.Write( encryptedKeyAndIV.Length );
                    bw.Write( encryptedKeyAndIV );
                    bw.Write( encryptedData.Length );
                    bw.Write( encryptedData );
                    bw.Flush();
                }

                return ms.ToArray();
            }
        }
    }
}
