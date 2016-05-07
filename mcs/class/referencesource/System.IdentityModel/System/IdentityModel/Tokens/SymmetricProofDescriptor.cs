//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Protocols.WSTrust;
    using RSTR = System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse;

    /// <summary>
    /// This class can be used for issuing the symmetric key based token.
    /// </summary>
    public class SymmetricProofDescriptor : ProofDescriptor
    {
        byte[] _key;
        int _keySizeInBits;
        byte[] _sourceEntropy;
        byte[] _targetEntropy;
        SecurityKeyIdentifier _ski;

        //
        // It is for encrypting the proof token or the entropy that can decrypted 
        // by the token requestor
        //
        EncryptingCredentials _requestorWrappingCredentials;

        //
        // It is for encrypting the key materials inside the issued token that
        // can be decrypted by the relying party
        //
        EncryptingCredentials _targetWrappingCredentials;

        /// <summary>
        /// Use this constructor if you want the sts to use the given key bytes.
        /// This happens when client sends the entropy, and the sts would just use that 
        /// as the key for the issued token.
        /// </summary>
        /// <param name="key">The symmetric key that are used inside the issued token.</param>
        /// <param name="targetWrappingCredentials">The key encrypting credentials for the relying party.</param>
        /// <exception cref="ArgumentNullException">When the key is null.</exception>
        public SymmetricProofDescriptor(byte[] key, EncryptingCredentials targetWrappingCredentials)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }

            _keySizeInBits = key.Length;
            _key = key;

            _targetWrappingCredentials = targetWrappingCredentials;
        }

        /// <summary>
        /// Use this constructor if you want the sts to use the given <see cref="EncryptingCredentials"/>.
        /// </summary>
        /// <param name="targetWrappingCredentials">The <see cref="EncryptingCredentials"/> to be used.</param>
        public SymmetricProofDescriptor(EncryptingCredentials targetWrappingCredentials)
            : this(SecurityTokenServiceConfiguration.DefaultKeySizeInBitsConstant, targetWrappingCredentials)
        {
        }

        /// <summary>
        /// Use this constructor if you want to the sts to autogenerate key using random number generator and
        /// send it in the proof token as binary secret.
        /// </summary>
        /// <param name="keySizeInBits">The size of the symmetric key.</param>
        /// <param name="targetWrappingCredentials">The key encrypting credentials for the relying party.</param>
        public SymmetricProofDescriptor(int keySizeInBits, EncryptingCredentials targetWrappingCredentials)
            : this(keySizeInBits, targetWrappingCredentials, null)
        {
        }

        /// <summary>
        /// Use this constructor to have the STS autogenerate a key and
        /// send it in the proof token as encrypted key. Two cases are covered here
        /// 1. client sends the entropy, but server rejects it
        /// 2. client did not send a entropy, so just use server's entropy
        /// </summary>
        /// <param name="keySizeInBits">the size of the symmetric key</param>
        /// <param name="targetWrappingCredentials">The key encrypting credentials for the relying party.</param>
        /// <param name="requestorWrappingCredentials">The key encrypting credentials for the requestor.</param>
        /// <exception cref="ArgumentOutOfRangeException">When keySizeInBits is less than or equal to zero.</exception>
        public SymmetricProofDescriptor(int keySizeInBits, EncryptingCredentials targetWrappingCredentials, EncryptingCredentials requestorWrappingCredentials)
            : this(keySizeInBits, targetWrappingCredentials, requestorWrappingCredentials, (string)null)
        {
        }

        /// <summary>
        /// Use this constructor to have the STS autogenerate a key and
        /// send it in the proof token as encrypted key. Two cases are covered here
        /// 1. client sends the entropy, but server rejects it
        /// 2. client did not send a entropy, so just use server's entropy
        /// </summary>
        /// <param name="keySizeInBits">the size of the symmetric key</param>
        /// <param name="targetWrappingCredentials">The key encrypting credentials for the relying party.</param>
        /// <param name="requestorWrappingCredentials">The key encrypting credentials for the requestor.</param>
        /// <param name="encryptWith">The a----thm specified in the EncryptWith element of the RST.</param>
        /// <exception cref="ArgumentOutOfRangeException">When keySizeInBits is less than or equal to zero.</exception>
        /// <remarks>If EncryptWith is a DES algorithm, the key is guaranteed not to be a weak DES key.</remarks>
        public SymmetricProofDescriptor(int keySizeInBits, EncryptingCredentials targetWrappingCredentials,
                                         EncryptingCredentials requestorWrappingCredentials, string encryptWith)
        {
            _keySizeInBits = keySizeInBits;

            if (encryptWith == SecurityAlgorithms.DesEncryption ||
                 encryptWith == SecurityAlgorithms.TripleDesEncryption ||
                 encryptWith == SecurityAlgorithms.TripleDesKeyWrap)
            {
                _key = CryptoHelper.KeyGenerator.GenerateDESKey(_keySizeInBits);
            }
            else
            {
                _key = CryptoHelper.KeyGenerator.GenerateSymmetricKey(_keySizeInBits);
            }

            _requestorWrappingCredentials = requestorWrappingCredentials;
            _targetWrappingCredentials = targetWrappingCredentials;
        }

        /// <summary>
        /// Use this constructor if you want to send combined entropy.
        /// </summary>
        /// <param name="keySizeInBits">The size of the symmetric key.</param>
        /// <param name="targetWrappingCredentials">The encrypting credentials for the relying party used to encrypt the key in the SecurityKeyIdentifier property.</param>
        /// <param name="requestorWrappingCredentials">The encrypting credentials for the requestor used to encrypt the entropy or the proof token.</param>
        /// <param name="sourceEntropy">The requestor's entropy.</param>
        /// <exception cref="ArgumentOutOfRangeException">When keySizeInBits is less than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">When source entorpy is null or is an empty array.</exception>
        public SymmetricProofDescriptor(int keySizeInBits, EncryptingCredentials targetWrappingCredentials,
                                         EncryptingCredentials requestorWrappingCredentials, byte[] sourceEntropy)
            : this(keySizeInBits, targetWrappingCredentials, requestorWrappingCredentials, sourceEntropy, null)
        {
        }

        /// <summary>
        /// Use this constructor to send combined entropy.
        /// </summary>
        /// <param name="keySizeInBits">The size of the symmetric key.</param>
        /// <param name="targetWrappingCredentials">The encrypting credentials for the relying party used to encrypt the key in the SecurityKeyIdentifier property.</param>
        /// <param name="requestorWrappingCredentials">The encrypting credentials for the requestor used to encrypt the entropy or the proof token.</param>
        /// <param name="sourceEntropy">The requestor's entropy.</param>
        /// <param name="encryptWith">The algorithm Uri using which to encrypt the proof key.</param>
        /// <exception cref="ArgumentOutOfRangeException">When keySizeInBits is less than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">When source entorpy is null or is an empty array.</exception>
        public SymmetricProofDescriptor(int keySizeInBits, EncryptingCredentials targetWrappingCredentials,
                                         EncryptingCredentials requestorWrappingCredentials, byte[] sourceEntropy, string encryptWith)
        {
            if (sourceEntropy == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sourceEntropy");
            }

            if (sourceEntropy.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("sourceEntropy", SR.GetString(SR.ID2058));
            }

            _keySizeInBits = keySizeInBits;
            _sourceEntropy = sourceEntropy;
            //
            // Generate proof key using sender entropy
            //
            if (encryptWith == SecurityAlgorithms.DesEncryption ||
                 encryptWith == SecurityAlgorithms.TripleDesEncryption ||
                 encryptWith == SecurityAlgorithms.TripleDesKeyWrap)
            {
                _key = CryptoHelper.KeyGenerator.GenerateDESKey(_keySizeInBits, _sourceEntropy, out _targetEntropy);
            }
            else
            {
                _key = CryptoHelper.KeyGenerator.GenerateSymmetricKey(_keySizeInBits, _sourceEntropy, out _targetEntropy);
            }

            //
            // Set up the wrapping credentials
            //
            _requestorWrappingCredentials = requestorWrappingCredentials;
            _targetWrappingCredentials = targetWrappingCredentials;
        }

        /// <summary>
        /// Gets the key bytes.
        /// </summary>
        public byte[] GetKeyBytes()
        {
            return _key;
        }

        /// <summary>
        /// Gets the requestor's encrypting credentials, which may be used to encrypt the 
        /// requested proof token or the entropy in the response.
        /// </summary>
        protected EncryptingCredentials RequestorEncryptingCredentials
        {
            get { return _requestorWrappingCredentials; }
        }

        /// <summary>
        /// Gets the source entropy in plain bytes.
        /// </summary>
        protected byte[] GetSourceEntropy()
        {
            return _sourceEntropy;
        }

        /// <summary>
        /// Gets the target entropy in plain bytes.
        /// </summary>
        protected byte[] GetTargetEntropy()
        {
            return _targetEntropy;
        }

        /// <summary>
        /// Gets the relying party encrypting credentials, which may be used to encrypt the
        /// requested security token in the response.
        /// </summary>
        protected EncryptingCredentials TargetEncryptingCredentials
        {
            get { return _targetWrappingCredentials; }
        }

        #region ProofDescriptor Overrides

        /// <summary>
        /// Sets the appropriate things, such as requested proof token, inside the RSTR 
        /// based on what is inside the proof descriptor instance.  
        /// </summary>
        /// <param name="response">The RSTR object that this proof descriptor needs to modify.</param>
        /// <exception cref="ArgumentNullException">When the response is null.</exception>
        public override void ApplyTo(RSTR response)
        {
            if (response == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("response");
            }

            if (_targetEntropy != null)
            {
                //
                // When there is target entropy, then we will send back a computedKeyalgorithm
                // in the proof token case and send the entropy as response.Entropy. By default, this
                // class is doing Psha1.
                //
                response.RequestedProofToken = new RequestedProofToken(ComputedKeyAlgorithms.Psha1);
                response.KeySizeInBits = _keySizeInBits;
                response.Entropy = new Entropy(_targetEntropy, _requestorWrappingCredentials);
            }
            else
            {
                //
                // When there is no target entroypy, then we will send back the key either in
                // binary secret format or in the encrypted key format
                //
                response.RequestedProofToken = new RequestedProofToken(_key, _requestorWrappingCredentials);
            }
        }

        /// <summary>
        /// Gets the key identifier that can be used inside issued to define the key.
        /// It is usually the binary secret or the encrypted key. 
        /// </summary>
        public override SecurityKeyIdentifier KeyIdentifier
        {
            get
            {
                if (_ski == null)
                {
                    _ski = CryptoHelper.KeyGenerator.GetSecurityKeyIdentifier(_key, _targetWrappingCredentials);
                }

                return _ski;
            }
        }

        #endregion
    }
}
