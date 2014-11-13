//------------------------------------------------------------------------------
// <copyright file="MachineKeySection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.Security.Cryptography;
    using System.Web.Util;
    using System.Xml;

    /******************************************************************
     * !! NOTICE !!                                                   *
     * The cryptographic code in this class is a legacy code base.    *
     * New code should not call into these crypto APIs; use the APIs  *
     * provided by AspNetCryptoServiceProvider instead.               *
     ******************************************************************/

    /******************************************************************
     * !! WARNING !!                                                  *
     * This class contains cryptographic code. If you make changes to *
     * this class, please have it reviewed by the appropriate people. *
     ******************************************************************/

    /*
            <!--  validation="[SHA1|MD5|3DES|AES|HMACSHA256|HMACSHA384|HMACSHA512|alg:algorithm_name]" decryption="[AES|EDES" -->
        <machineKey validationKey="AutoGenerate,IsolateApps" decryptionKey="AutoGenerate,IsolateApps" decryption="[AES|3DES]" validation="HMACSHA256" compatibilityMode="[Framework20SP1|Framework20SP2]" />
    */

    public sealed class MachineKeySection : ConfigurationSection
    {
        private const string OBSOLETE_CRYPTO_API_MESSAGE = "This API exists only for backward compatibility; new framework features that require cryptographic services MUST NOT call it. New features should use the AspNetCryptoServiceProvider class instead.";

        // If the default validation algorithm changes, be sure to update the _HashSize and _AutoGenValidationKeySize fields also.
        internal const string DefaultValidationAlgorithm = "HMACSHA256";
        internal const MachineKeyValidation DefaultValidation = MachineKeyValidation.SHA1;
        internal const string DefaultDataProtectorType = "";
        internal const string DefaultApplicationName = "";

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propValidationKey =
            new ConfigurationProperty("validationKey", typeof(string), "AutoGenerate,IsolateApps", StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDecryptionKey =
            new ConfigurationProperty("decryptionKey", typeof(string),"AutoGenerate,IsolateApps",StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDecryption =
            new ConfigurationProperty("decryption", typeof(string), "Auto", StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propValidation =
            new ConfigurationProperty("validation", typeof(string), DefaultValidationAlgorithm, StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDataProtectorType =
            new ConfigurationProperty("dataProtectorType", typeof(string), DefaultDataProtectorType, StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propApplicationName =
            new ConfigurationProperty("applicationName", typeof(string), DefaultApplicationName, StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCompatibilityMode =
            new ConfigurationProperty("compatibilityMode", typeof(MachineKeyCompatibilityMode), MachineKeyCompatibilityMode.Framework20SP1, null, null, ConfigurationPropertyOptions.None);

        private static object s_initLock = new object();
        private static bool s_initComplete = false;
        private static MachineKeySection s_config;
        private static RNGCryptoServiceProvider s_randomNumberGenerator;
        private static SymmetricAlgorithm s_oSymAlgoDecryption;
        private static SymmetricAlgorithm s_oSymAlgoValidation;
        private static byte[] s_validationKey;
        private static byte[] s_inner = null;
        private static byte[] s_outer = null;
        internal static bool IsDecryptionKeyAutogenerated { get { EnsureConfig(); return s_config.AutogenKey; } }
        private bool _AutogenKey;
        internal bool AutogenKey { get { RuntimeDataInitialize(); return _AutogenKey; } }
        private byte[] _ValidationKey;
        private byte[] _DecryptionKey;
        private bool DataInitialized = false;
        private static bool _CustomValidationTypeIsKeyed;
        private static string _CustomValidationName;
        private static int _IVLengthDecryption = 64;
        private static int _IVLengthValidation = 64;
        private static int _HashSize = HMACSHA256_HASH_SIZE;
        private static int _AutoGenValidationKeySize = HMACSHA256_KEY_SIZE;
        private static int _AutoGenDecryptionKeySize = 24;
        private static bool _UseHMACSHA = true;
        private static bool _UsingCustomEncryption = false;
        private static SymmetricAlgorithm s_oSymAlgoLegacy;

        private const int MD5_KEY_SIZE          = 64;
        private const int MD5_HASH_SIZE         = 16;
        private const int SHA1_KEY_SIZE         = 64;
        private const int HMACSHA256_KEY_SIZE       = 64;
        private const int HMACSHA384_KEY_SIZE       = 128;
        private const int HMACSHA512_KEY_SIZE       = 128;
        private const int SHA1_HASH_SIZE        = 20;
        private const int HMACSHA256_HASH_SIZE      = 32;
        private const int HMACSHA384_HASH_SIZE      = 48;
        private const int HMACSHA512_HASH_SIZE      = 64;
        private const string ALGO_PREFIX        = "alg:";

        internal byte[] ValidationKeyInternal { get { RuntimeDataInitialize();  return (byte[])_ValidationKey.Clone(); } }
        internal byte[] DecryptionKeyInternal { get { RuntimeDataInitialize(); return (byte[])_DecryptionKey.Clone(); } }
        internal static int HashSize { get { s_config.RuntimeDataInitialize(); return _HashSize; } }
        internal static int ValidationKeySize { get { s_config.RuntimeDataInitialize(); return _AutoGenValidationKeySize; } }

        static MachineKeySection()
        {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propValidationKey);
            _properties.Add(_propDecryptionKey);
            _properties.Add(_propValidation);
            _properties.Add(_propDecryption);
            _properties.Add(_propCompatibilityMode);
            _properties.Add(_propDataProtectorType);
            _properties.Add(_propApplicationName);
        }

        public MachineKeySection()
        {
        }

        internal static MachineKeyCompatibilityMode CompatMode
        {
            get
            {
                return GetApplicationConfig().CompatibilityMode;
            }
        }


        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("validationKey", DefaultValue = "AutoGenerate,IsolateApps")]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        [StringValidator(MinLength = 1)]
        public string ValidationKey
        {
            get
            {
                return (string)base[_propValidationKey];
            }
            set
            {
                base[_propValidationKey] = value;
            }
        }

        [ConfigurationProperty("decryptionKey", DefaultValue = "AutoGenerate,IsolateApps")]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        [StringValidator(MinLength = 1)]
        public string DecryptionKey
        {
            get
            {
                return (string)base[_propDecryptionKey];
            }
            set
            {
                base[_propDecryptionKey] = value;
            }
        }

        [ConfigurationProperty("decryption", DefaultValue = "Auto")]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        [StringValidator(MinLength = 1)]
        public string Decryption {
            get {
                string s = GetDecryptionAttributeSkipValidation();
                if (s != "Auto" && s != "AES" && s != "3DES" && s != "DES" && !s.StartsWith(ALGO_PREFIX, StringComparison.Ordinal))
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_decryption_enum), ElementInformation.Properties["decryption"].Source, ElementInformation.Properties["decryption"].LineNumber);
                return s;
            }
            set {
                if (value != "AES" && value != "3DES" && value != "Auto" && value != "DES" && !value.StartsWith(ALGO_PREFIX, StringComparison.Ordinal))
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_decryption_enum), ElementInformation.Properties["decryption"].Source, ElementInformation.Properties["decryption"].LineNumber);
                base[_propDecryption] = value;
            }
        }

        // returns the value in the 'decryption' attribute (or the default value if null) without throwing an exception if the value is malformed
        internal string GetDecryptionAttributeSkipValidation() {
            return (string)base[_propDecryption] ?? "Auto";
        }

        private bool _validationIsCached;
        private string _cachedValidation;
        private MachineKeyValidation _cachedValidationEnum;

        [ConfigurationProperty("validation", DefaultValue = DefaultValidationAlgorithm)]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        [StringValidator(MinLength = 1)]
        public string ValidationAlgorithm
        {
            get {
                if (!_validationIsCached)
                    CacheValidation();
                return _cachedValidation;
            } set {
                if (_validationIsCached && value == _cachedValidation)
                    return;
                if (value == null)
                    value = DefaultValidationAlgorithm;
                _cachedValidationEnum = MachineKeyValidationConverter.ConvertToEnum(value);
                _cachedValidation = value;
                base[_propValidation] = value;
                _validationIsCached = true;
            }
        }

        // returns the value in the 'validation' attribute (or the default value if null) without throwing an exception if the value is malformed
        internal string GetValidationAttributeSkipValidation() {
            return (string)base[_propValidation] ?? DefaultValidationAlgorithm;
        }

        private void CacheValidation()
        {
            _cachedValidation = GetValidationAttributeSkipValidation();
            _cachedValidationEnum = MachineKeyValidationConverter.ConvertToEnum(_cachedValidation);
            _validationIsCached = true;
        }

        public MachineKeyValidation Validation {
            get {
                if (_validationIsCached == false)
                    CacheValidation();
                return _cachedValidationEnum;
            } set {
                if (_validationIsCached && value == _cachedValidationEnum)
                    return;
                _cachedValidation = MachineKeyValidationConverter.ConvertFromEnum(value);
                _cachedValidationEnum = value;
                base[_propValidation] = _cachedValidation;
                _validationIsCached = true;
            }
        }

        [ConfigurationProperty("dataProtectorType", DefaultValue = DefaultDataProtectorType)]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        public string DataProtectorType {
            get {
                return (string)base[_propDataProtectorType];
            }
            set {
                base[_propDataProtectorType] = value;
            }
        }

        [ConfigurationProperty("applicationName", DefaultValue = DefaultApplicationName)]
        [TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        public string ApplicationName {
            get {
                return (string)base[_propApplicationName];
            }
            set {
                base[_propApplicationName] = value;
            }
        }

        private MachineKeyCompatibilityMode _compatibilityMode = (MachineKeyCompatibilityMode)(-1); // dummy value used to mean uninitialized

        [ConfigurationProperty("compatibilityMode", DefaultValue = MachineKeyCompatibilityMode.Framework20SP1)]
        public MachineKeyCompatibilityMode CompatibilityMode
        {
            get
            {
                // the compatibility mode is cached since it's queried frequently
                if (_compatibilityMode < 0) {
                    _compatibilityMode = (MachineKeyCompatibilityMode)base[_propCompatibilityMode];
                }
                return _compatibilityMode;
            }
            set
            {
                base[_propCompatibilityMode] = value;
                _compatibilityMode = value;
            }
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            MachineKeySection parent = parentElement as MachineKeySection;
            base.Reset(parentElement);
            // copy the privates from the parent.
            if (parent != null)
            {
//                _ValidationKey = parent.ValidationKeyInternal;
//                _DecryptionKey = parent.DecryptionKeyInternal;
//                _AutogenKey = parent.AutogenKey;
            }
        }

        private void RuntimeDataInitialize()
        {
            if (DataInitialized == false)
            {
                byte [] bKeysRandom = null;
                bool fNonHttpApp = false;
                string strKey = ValidationKey;
                string appName = HttpRuntime.AppDomainAppVirtualPath;
                string appId = HttpRuntime.AppDomainAppId;

                InitValidationAndEncyptionSizes();

                if( appName == null )
                {
#if !FEATURE_PAL // FEATURE_PAL does not enable cryptography
			// FEATURE_PAL 

                    appName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;

                    if( ValidationKey.Contains( "AutoGenerate" ) ||
                        DecryptionKey.Contains( "AutoGenerate" ) )
                    {
                        fNonHttpApp = true;

                        bKeysRandom = new byte[ _AutoGenValidationKeySize + _AutoGenDecryptionKeySize ];
                        // Gernerate random keys
                        RandomNumberGenerator.GetBytes(bKeysRandom);
                    }
#endif // !FEATURE_PAL
                }

                bool fAppIdSpecific = StringUtil.StringEndsWith(strKey, ",IsolateByAppId");
                if (fAppIdSpecific)
                {
                    strKey = strKey.Substring(0, strKey.Length - ",IsolateByAppId".Length);
                }
                bool fAppSpecific = StringUtil.StringEndsWith(strKey, ",IsolateApps");
                if (fAppSpecific)
                {
                    strKey = strKey.Substring(0, strKey.Length - ",IsolateApps".Length);
                }
                if (strKey == "AutoGenerate")
                { // case sensitive
                    _ValidationKey = new byte[_AutoGenValidationKeySize];

                    if( fNonHttpApp )
                    {
                        Buffer.BlockCopy( bKeysRandom, 0, _ValidationKey, 0, _AutoGenValidationKeySize);
                    }
                    else
                    {
                        Buffer.BlockCopy(HttpRuntime.s_autogenKeys, 0, _ValidationKey, 0, _AutoGenValidationKeySize);
                    }
                }
                else
                {
                    if (strKey.Length < 40 || (strKey.Length & 0x1) == 1)
                        throw new ConfigurationErrorsException(SR.GetString(SR.Unable_to_get_cookie_authentication_validation_key, strKey.Length.ToString(CultureInfo.InvariantCulture)), ElementInformation.Properties["validationKey"].Source, ElementInformation.Properties["validationKey"].LineNumber);

#pragma warning disable 618 // obsolete
                    _ValidationKey = HexStringToByteArray(strKey);
#pragma warning restore 618
                    if (_ValidationKey == null)
                        throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_validation_key), ElementInformation.Properties["validationKey"].Source, ElementInformation.Properties["validationKey"].LineNumber);
                }
                if (fAppSpecific)
                {
                    int dwCode = StringComparer.InvariantCultureIgnoreCase.GetHashCode( appName );
                    _ValidationKey[0] = (byte)(dwCode & 0xff);
                    _ValidationKey[1] = (byte)((dwCode & 0xff00) >> 8);
                    _ValidationKey[2] = (byte)((dwCode & 0xff0000) >> 16);
                    _ValidationKey[3] = (byte)((dwCode & 0xff000000) >> 24);
                }
                if (fAppIdSpecific)
                {
                    int dwCode = StringComparer.InvariantCultureIgnoreCase.GetHashCode( appId );
                    _ValidationKey[4] = (byte)(dwCode & 0xff);
                    _ValidationKey[5] = (byte)((dwCode & 0xff00) >> 8);
                    _ValidationKey[6] = (byte)((dwCode & 0xff0000) >> 16);
                    _ValidationKey[7] = (byte)((dwCode & 0xff000000) >> 24);
                }

                strKey = DecryptionKey;
                fAppIdSpecific = StringUtil.StringEndsWith(strKey, ",IsolateByAppId");
                if (fAppIdSpecific)
                {
                    strKey = strKey.Substring(0, strKey.Length - ",IsolateByAppId".Length);
                }
                fAppSpecific = StringUtil.StringEndsWith(strKey, ",IsolateApps");
                if (fAppSpecific)
                {
                    strKey = strKey.Substring(0, strKey.Length - ",IsolateApps".Length);
                }

                if (strKey == "AutoGenerate")
                { // case sensitive
                    _DecryptionKey = new byte[_AutoGenDecryptionKeySize];

                    if( fNonHttpApp )
                    {
                        Buffer.BlockCopy( bKeysRandom, _AutoGenValidationKeySize, _DecryptionKey, 0, _AutoGenDecryptionKeySize);
                    }
                    else
                    {
                        Buffer.BlockCopy(HttpRuntime.s_autogenKeys, _AutoGenValidationKeySize, _DecryptionKey, 0, _AutoGenDecryptionKeySize);
                    }

                    _AutogenKey = true;
                }
                else
                {
                    _AutogenKey = false;
                    if ((strKey.Length & 1) != 0)
                        throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_decryption_key), ElementInformation.Properties["decryptionKey"].Source, ElementInformation.Properties["decryptionKey"].LineNumber);

#pragma warning disable 618 // obsolete
                    _DecryptionKey = HexStringToByteArray(strKey);
#pragma warning restore 618
                    if (_DecryptionKey == null)
                        throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_decryption_key), ElementInformation.Properties["decryptionKey"].Source, ElementInformation.Properties["decryptionKey"].LineNumber);
                }
                if (fAppSpecific)
                {
                    int dwCode = StringComparer.InvariantCultureIgnoreCase.GetHashCode(appName);
                    _DecryptionKey[0] = (byte)(dwCode & 0xff);
                    _DecryptionKey[1] = (byte)((dwCode & 0xff00) >> 8);
                    _DecryptionKey[2] = (byte)((dwCode & 0xff0000) >> 16);
                    _DecryptionKey[3] = (byte)((dwCode & 0xff000000) >> 24);
                }
                if (fAppIdSpecific)
                {
                    int dwCode = StringComparer.InvariantCultureIgnoreCase.GetHashCode(appId);
                    _DecryptionKey[4] = (byte)(dwCode & 0xff);
                    _DecryptionKey[5] = (byte)((dwCode & 0xff00) >> 8);
                    _DecryptionKey[6] = (byte)((dwCode & 0xff0000) >> 16);
                    _DecryptionKey[7] = (byte)((dwCode & 0xff000000) >> 24);
                }
                DataInitialized = true;
            }
        }

        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        internal static byte[] EncryptOrDecryptData(bool fEncrypt, byte[] buf, byte[] modifier, int start, int length)
        {
            // MSRC 10405: IVType.Hash has been removed; new default behavior is to use IVType.Random.
            return EncryptOrDecryptData(fEncrypt, buf, modifier, start, length, false, false, IVType.Random);
        }

        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        internal static byte[] EncryptOrDecryptData(bool fEncrypt, byte[] buf, byte[] modifier, int start, int length, bool useValidationSymAlgo)
        {
            // MSRC 10405: IVType.Hash has been removed; new default behavior is to use IVType.Random.
            return EncryptOrDecryptData(fEncrypt, buf, modifier, start, length, useValidationSymAlgo, false, IVType.Random);
        }

        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        internal static byte[] EncryptOrDecryptData(bool fEncrypt, byte[] buf, byte[] modifier, int start, int length,
                                                    bool useValidationSymAlgo, bool useLegacyMode, IVType ivType)
        {
            // MSRC 10405: Encryption is not sufficient to prevent a malicious user from tampering with the data, and the result of decryption can
            // be used to discover information about the plaintext (such as via a padding or decryption oracle). We must sign anything that we
            // encrypt to ensure that end users can't abuse our encryption routines.

            // the new encrypt-then-sign behavior for everything EXCEPT Membership / MachineKey. We need to make it very clear that setting this
            // to 'false' is a Very Bad Thing(tm).
            return EncryptOrDecryptData(fEncrypt, buf, modifier, start, length, useValidationSymAlgo, useLegacyMode, ivType, !AppSettings.UseLegacyEncryption);
        }

        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        internal static byte[] EncryptOrDecryptData(bool fEncrypt, byte[] buf, byte[] modifier, int start, int length,
                                                    bool useValidationSymAlgo, bool useLegacyMode, IVType ivType, bool signData)
        {
            /* This algorithm is used to perform encryption or decryption of a buffer, along with optional signing (for encryption)
             * or signature verification (for decryption). Possible operation modes are:
             * 
             * ENCRYPT + SIGN DATA (fEncrypt = true, signData = true)
             * Input: buf represents plaintext to encrypt, modifier represents data to be appended to buf (but isn't part of the plaintext itself)
             * Output: E(iv + buf + modifier) + HMAC(E(iv + buf + modifier))
             * 
             * ONLY ENCRYPT DATA (fEncrypt = true, signData = false)
             * Input: buf represents plaintext to encrypt, modifier represents data to be appended to buf (but isn't part of the plaintext itself)
             * Output: E(iv + buf + modifier)
             * 
             * VERIFY + DECRYPT DATA (fEncrypt = false, signData = true)
             * Input: buf represents ciphertext to decrypt, modifier represents data to be removed from the end of the plaintext (since it's not really plaintext data)
             * Input (buf): E(iv + m + modifier) + HMAC(E(iv + m + modifier))
             * Output: m
             * 
             * ONLY DECRYPT DATA (fEncrypt = false, signData = false)
             * Input: buf represents ciphertext to decrypt, modifier represents data to be removed from the end of the plaintext (since it's not really plaintext data)
             * Input (buf): E(iv + plaintext + modifier)
             * Output: m
             * 
             * The 'iv' in the above descriptions isn't an actual IV. Rather, if ivType = IVType.Random, we'll prepend random bytes ('iv')
             * to the plaintext before feeding it to the crypto algorithms. Introducing randomness early in the algorithm prevents users
             * from inspecting two ciphertexts to see if the plaintexts are related. If ivType = IVType.None, then 'iv' is simply
             * an empty string. If ivType = IVType.Hash, we use a non-keyed hash of the plaintext.
             * 
             * The 'modifier' in the above descriptions is a piece of metadata that should be encrypted along with the plaintext but
             * which isn't actually part of the plaintext itself. It can be used for storing things like the user name for whom this
             * plaintext was generated, the page that generated the plaintext, etc. On decryption, the modifier parameter is compared
             * against the modifier stored in the crypto stream, and it is stripped from the message before the plaintext is returned.
             * 
             * In all cases, if something goes wrong (e.g. invalid padding, invalid signature, invalid modifier, etc.), a generic exception is thrown.
             */

            try {
                EnsureConfig();

                if (!fEncrypt && signData) {
                    if (start != 0 || length != buf.Length) {
                        // These transformations assume that we're operating on buf in its entirety and
                        // not on any subset of buf, so we'll just replace buf with the particular subset
                        // we're interested in.
                        byte[] bTemp = new byte[length];
                        Buffer.BlockCopy(buf, start, bTemp, 0, length);
                        buf = bTemp;
                        start = 0;
                    }

                    // buf actually contains E(iv + m + modifier) + HMAC(E(iv + m + modifier)), so we need to verify and strip off the signature
                    buf = GetUnHashedData(buf);
                    // At this point, buf contains only E(iv + m + modifier) if the signature check succeeded.

                    if (buf == null) {
                        // signature verification failed
                        throw new HttpException(SR.GetString(SR.Unable_to_validate_data));
                    }

                    // need to fix up again since GetUnhashedData() returned a different array
                    length = buf.Length;
                }

                if (useLegacyMode)
                    useLegacyMode = _UsingCustomEncryption; // only use legacy mode for custom algorithms

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ICryptoTransform cryptoTransform = GetCryptoTransform(fEncrypt, useValidationSymAlgo, useLegacyMode);
                CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);

                // DevDiv Bugs 137864: Add IV to beginning of data to be encrypted.
                // IVType.None is used by MembershipProvider which requires compatibility even in SP2 mode (and will set signData = false).
                // MSRC 10405: If signData is set to true, we must generate an IV.
                bool createIV = signData || ((ivType != IVType.None) && (CompatMode > MachineKeyCompatibilityMode.Framework20SP1));

                if (fEncrypt && createIV)
                {
                    int ivLength = (useValidationSymAlgo ? _IVLengthValidation : _IVLengthDecryption);
                    byte[] iv = null;

                    switch (ivType) {
                        case IVType.Hash:
                            // iv := H(buf)
                            iv = GetIVHash(buf, ivLength);
                            break;

                        case IVType.Random:
                            // iv := [random]
                            iv = new byte[ivLength];
                            RandomNumberGenerator.GetBytes(iv);
                            break;
                    }

                    Debug.Assert(iv != null, "Invalid value for IVType: " + ivType.ToString("G"));
                    cs.Write(iv, 0, iv.Length);
                }

                cs.Write(buf, start, length);
                if (fEncrypt && modifier != null)
                {
                    cs.Write(modifier, 0, modifier.Length);
                }

                cs.FlushFinalBlock();
                byte[] paddedData = ms.ToArray();

                // At this point:
                // If fEncrypt = true (encrypting), paddedData := Enc(iv + buf + modifier)
                // If fEncrypt = false (decrypting), paddedData := iv + plaintext + modifier

                byte[] bData;
                cs.Close();

                // In ASP.NET 2.0, we pool ICryptoTransform objects, and this returns that ICryptoTransform
                // to the pool. In ASP.NET 4.0, this just disposes of the ICryptoTransform object.
                ReturnCryptoTransform(fEncrypt, cryptoTransform, useValidationSymAlgo, useLegacyMode);

                // DevDiv Bugs 137864: Strip IV from beginning of unencrypted data
                if (!fEncrypt && createIV)
                {
                    // strip off the first bytes that were random bits
                    int ivLength = (useValidationSymAlgo ? _IVLengthValidation : _IVLengthDecryption);
                    int bDataLength = paddedData.Length - ivLength;
                    if (bDataLength < 0) {
                        throw new HttpException(SR.GetString(SR.Unable_to_validate_data));
                    }

                    bData = new byte[bDataLength];
                    Buffer.BlockCopy(paddedData, ivLength, bData, 0, bDataLength);
                }
                else
                {
                    bData = paddedData;
                }

                // At this point:
                // If fEncrypt = true (encrypting), bData := Enc(iv + buf + modifier)
                // If fEncrypt = false (decrypting), bData := plaintext + modifier

                if (!fEncrypt && modifier != null && modifier.Length > 0)
                {
                    // MSRC 10405: Crypto board suggests blinding where signature failed
                    // to prevent timing attacks.
                    bool modifierCheckFailed = false;
                    for(int iter=0; iter<modifier.Length; iter++)
                        if (bData[bData.Length - modifier.Length + iter] != modifier[iter])
                            modifierCheckFailed = true;
                    if (modifierCheckFailed) {
                        throw new HttpException(SR.GetString(SR.Unable_to_validate_data));
                    }

                    byte[] bData2 = new byte[bData.Length - modifier.Length];
                    Buffer.BlockCopy(bData, 0, bData2, 0, bData2.Length);
                    bData = bData2;
                }

                // At this point:
                // If fEncrypt = true (encrypting), bData := Enc(iv + buf + modifier)
                // If fEncrypt = false (decrypting), bData := plaintext

                if (fEncrypt && signData) {
                    byte[] hmac = HashData(bData, null, 0, bData.Length);
                    byte[] bData2 = new byte[bData.Length + hmac.Length];

                    Buffer.BlockCopy(bData, 0, bData2, 0, bData.Length);
                    Buffer.BlockCopy(hmac, 0, bData2, bData.Length, hmac.Length);
                    bData = bData2;
                }

                // At this point:
                // If fEncrypt = true (encrypting), bData := Enc(iv + buf + modifier) + HMAC(Enc(iv + buf + modifier))
                // If fEncrypt = false (decrypting), bData := plaintext

                // And we're done
                return bData;
            } catch {
                // It's important that we don't propagate the original exception here as we don't want a production
                // server which has unintentionally left YSODs enabled to leak cryptographic information.
                throw new HttpException(SR.GetString(SR.Unable_to_validate_data));
            }
        }

        private static byte[] GetIVHash(byte[] buf, int ivLength)
        {
            // return an IV that is computed as a hash of the buffer
            int bytesToWrite = ivLength;
            int bytesWritten = 0;
            byte[] iv = new byte[ivLength];

            // get SHA1 hash of the buffer and copy to the IV.
            // if hash length is less than IV length, re-hash the hash and
            // append until IV is full.
            byte[] hash = buf;
            while (bytesWritten < ivLength)
            {
                byte[] newHash = new byte[_HashSize];
                int hr = UnsafeNativeMethods.GetSHA1Hash(hash, hash.Length, newHash, newHash.Length);
                Marshal.ThrowExceptionForHR(hr);
                hash = newHash;

                int bytesToCopy = Math.Min(_HashSize, bytesToWrite);
                Buffer.BlockCopy(hash, 0, iv, bytesWritten, bytesToCopy);

                bytesWritten += bytesToCopy;
                bytesToWrite -= bytesToCopy;
            }
            return iv;
        }

        private static RNGCryptoServiceProvider RandomNumberGenerator {
            get {
                if (s_randomNumberGenerator == null) {
                    s_randomNumberGenerator = new RNGCryptoServiceProvider();
                }
                return s_randomNumberGenerator;
            }
        }


        private static void SetInnerOuterKeys(byte[] validationKey, ref byte[] inner, ref byte[] outer) {
            byte[] key = null;
            if (validationKey.Length > _AutoGenValidationKeySize)
            {
                key = new byte[_HashSize];
                int hr = UnsafeNativeMethods.GetSHA1Hash(validationKey, validationKey.Length, key, key.Length);
                Marshal.ThrowExceptionForHR(hr);
            }

            if (inner == null)
                inner = new byte[_AutoGenValidationKeySize];
            if (outer == null)
                outer = new byte[_AutoGenValidationKeySize];

            int i;
            for (i = 0; i < _AutoGenValidationKeySize; i++) {
                inner[i] = 0x36;
                outer[i] = 0x5C;
            }
            for (i=0; i < validationKey.Length; i++) {
                inner[i] ^= validationKey[i];
                outer[i] ^= validationKey[i];
            }
        }

        private static byte[] GetHMACSHA1Hash(byte[] buf, byte[] modifier, int start, int length) {
            if (start < 0 || start > buf.Length)
                throw new ArgumentException(SR.GetString(SR.InvalidArgumentValue, "start"));
            if (length < 0 || buf == null || (start + length) > buf.Length)
                throw new ArgumentException(SR.GetString(SR.InvalidArgumentValue, "length"));
            byte[] hash = new byte[_HashSize];
            int hr = UnsafeNativeMethods.GetHMACSHA1Hash(buf, start, length,
                                                         modifier, (modifier == null) ? 0 : modifier.Length,
                                                         s_inner, s_inner.Length, s_outer, s_outer.Length,
                                                         hash, hash.Length);
            if (hr == 0)
                return hash;
            _UseHMACSHA = false;
            return null;
        }

        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        internal static string HashAndBase64EncodeString(string s)
        {
            byte[] ab;
            byte[] hash;
            string result;

            ab = Encoding.Unicode.GetBytes(s);
            hash = HashData(ab, null, 0, ab.Length);
            result = Convert.ToBase64String(hash);

            return result;
        }

        static internal void DestroyByteArray(byte[] buf)
        {
            if (buf == null || buf.Length < 1)
                return;
            for (int iter = 0; iter < buf.Length; iter++)
                buf[iter] = (byte)0;
        }

        internal void DestroyKeys()
        {
            MachineKeySection.DestroyByteArray(_ValidationKey);
            MachineKeySection.DestroyByteArray(_DecryptionKey);
        }

        static void EnsureConfig()
        {
            if (!s_initComplete)
            {
                lock (s_initLock)
                {
                    if (!s_initComplete)
                    {
                        GetApplicationConfig(); // sets s_config field
                        s_config.ConfigureEncryptionObject();
                        s_initComplete = true;
                    }
                }
            }
        }

        // gets the application-level MachineKeySection
        internal static MachineKeySection GetApplicationConfig() {
            if (s_config == null) {
                lock (s_initLock) {
                    if (s_config == null) {
                        s_config = RuntimeConfig.GetAppConfig().MachineKey;
                    }
                }
            }
            return s_config;
        }

        // NOTE: When encoding the data, this method *may* return the same reference to the input "buf" parameter
        // with the hash appended in the end if there's enough space.  The "length" parameter would also be
        // appropriately adjusted in those cases.  This is an optimization to prevent unnecessary copying of
        // buffers.
        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        internal static byte[] GetEncodedData(byte[] buf, byte[] modifier, int start, ref int length)
        {
            EnsureConfig();

            byte[] bHash = HashData(buf, modifier, start, length);
            byte[] returnBuffer;

            if (buf.Length - start - length >= bHash.Length)
            {
                // Append hash to end of buffer if there's space
                Buffer.BlockCopy(bHash, 0, buf, start + length, bHash.Length);
                returnBuffer = buf;
            }
            else
            {
                returnBuffer = new byte[length + bHash.Length];
                Buffer.BlockCopy(buf, start, returnBuffer, 0, length);
                Buffer.BlockCopy(bHash, 0, returnBuffer, length, bHash.Length);
                start = 0;
            }
            length += bHash.Length;

            if (s_config.Validation == MachineKeyValidation.TripleDES || s_config.Validation == MachineKeyValidation.AES) {
                returnBuffer = EncryptOrDecryptData(true, returnBuffer, modifier, start, length, true);
                length = returnBuffer.Length;
            }
            return returnBuffer;
        }

        // NOTE: When decoding the data, this method *may* return the same reference to the input "buf" parameter
        // with the "dataLength" parameter containing the actual length of the data in the "buf" (i.e. length of actual
        // data is (total length of data - hash length)). This is an optimization to prevent unnecessary copying of buffers.
        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        internal static byte[] GetDecodedData(byte[] buf, byte[] modifier, int start, int length, ref int dataLength)
        {
            EnsureConfig();

            if (s_config.Validation == MachineKeyValidation.TripleDES || s_config.Validation == MachineKeyValidation.AES) {
                buf = EncryptOrDecryptData(false, buf, modifier, start, length, true);
                if (buf == null || buf.Length < _HashSize)
                    throw new HttpException(SR.GetString(SR.Unable_to_validate_data));
                length = buf.Length;
                start = 0;
            }

            if (length < _HashSize || start < 0 || start >= length)
                throw new HttpException(SR.GetString(SR.Unable_to_validate_data));
            byte[] bHash = HashData(buf, modifier, start, length - _HashSize);
            for (int iter = 0; iter < bHash.Length; iter++)
                if (bHash[iter] != buf[start + length - _HashSize + iter])
                    throw new HttpException(SR.GetString(SR.Unable_to_validate_data));

            dataLength = length - _HashSize;
            return buf;
        }

        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        internal static byte[] HashData(byte[] buf, byte[] modifier, int start, int length)
        {
            EnsureConfig();

            if (s_config.Validation == MachineKeyValidation.MD5)
                return HashDataUsingNonKeyedAlgorithm(null, buf, modifier, start, length, s_validationKey);
            if (_UseHMACSHA) {
                byte [] hash = GetHMACSHA1Hash(buf, modifier, start, length);
                if (hash != null)
                    return hash;
            }
            if (_CustomValidationTypeIsKeyed) {
                return HashDataUsingKeyedAlgorithm(KeyedHashAlgorithm.Create(_CustomValidationName),
                                                   buf, modifier, start, length, s_validationKey);
            } else {
                return HashDataUsingNonKeyedAlgorithm(HashAlgorithm.Create(_CustomValidationName),
                                                      buf, modifier, start, length, s_validationKey);
            }
        }


        private void ConfigureEncryptionObject()
        {
            // We suppress CS0618 since some of the algorithms we support are marked with [Obsolete].
            // These deprecated algorithms are *not* enabled by default. Developers must opt-in to
            // them, so we're secure by default.
#pragma warning disable 618
            using (new ApplicationImpersonationContext())  {
                s_validationKey = ValidationKeyInternal;
                byte[] dKey = DecryptionKeyInternal;
                if (_UseHMACSHA)
                    SetInnerOuterKeys(s_validationKey, ref s_inner, ref s_outer);
                DestroyKeys();

                switch (Decryption)
                {
                case "3DES":
                    s_oSymAlgoDecryption = CryptoAlgorithms.CreateTripleDES();
                    break;
                case "DES":
                    s_oSymAlgoDecryption = CryptoAlgorithms.CreateDES();
                    break;
                case "AES":
                    s_oSymAlgoDecryption = CryptoAlgorithms.CreateAes();
                    break;
                case "Auto":
                    if (dKey.Length == 8) {
                        s_oSymAlgoDecryption = CryptoAlgorithms.CreateDES();
                    } else {
                        s_oSymAlgoDecryption = CryptoAlgorithms.CreateAes();
                    }
                    break;
                }

                if (s_oSymAlgoDecryption == null) // Shouldn't happen!
                    InitValidationAndEncyptionSizes();

                switch(Validation)
                {
                case MachineKeyValidation.TripleDES:
                    if (dKey.Length == 8) {
                        s_oSymAlgoValidation = CryptoAlgorithms.CreateDES();
                    } else {
                        s_oSymAlgoValidation = CryptoAlgorithms.CreateTripleDES();
                    }
                    break;
                case MachineKeyValidation.AES:
                    s_oSymAlgoValidation = CryptoAlgorithms.CreateAes();
                    break;
                }

                // The IV lengths should actually be equal to the block sizes rather than the key
                // sizes, but we shipped with this code and unfortunately cannot change it without
                // breaking back-compat.
                if (s_oSymAlgoValidation != null) {
                    SetKeyOnSymAlgorithm(s_oSymAlgoValidation, dKey);
                    _IVLengthValidation = RoundupNumBitsToNumBytes(s_oSymAlgoValidation.KeySize);
                }
                SetKeyOnSymAlgorithm(s_oSymAlgoDecryption, dKey);
                _IVLengthDecryption = RoundupNumBitsToNumBytes(s_oSymAlgoDecryption.KeySize);
                InitLegacyEncAlgorithm(dKey);
                DestroyByteArray(dKey);
            }
#pragma warning restore 618
        }

        private void SetKeyOnSymAlgorithm(SymmetricAlgorithm symAlgo, byte[] dKey)
        {
            try {
                if (dKey.Length > 8 && symAlgo is DESCryptoServiceProvider) {
                    byte[] bTemp = new byte[8];
                    Buffer.BlockCopy(dKey, 0, bTemp, 0, 8);
                    symAlgo.Key = bTemp;
                    DestroyByteArray(bTemp);
                } else {
                    symAlgo.Key = dKey;
                }
                symAlgo.GenerateIV();
                symAlgo.IV = new byte[symAlgo.IV.Length];
            } catch (Exception e) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Bad_machine_key, e.Message), ElementInformation.Properties["decryptionKey"].Source, ElementInformation.Properties["decryptionKey"].LineNumber);
            }
        }

        private static ICryptoTransform GetCryptoTransform(bool fEncrypt, bool useValidationSymAlgo, bool legacyMode)
        {
            SymmetricAlgorithm algo = (legacyMode ? s_oSymAlgoLegacy : (useValidationSymAlgo ? s_oSymAlgoValidation : s_oSymAlgoDecryption));
            lock(algo)
                return (fEncrypt ? algo.CreateEncryptor() : algo.CreateDecryptor());
        }

        private static void ReturnCryptoTransform(bool fEncrypt, ICryptoTransform ct, bool useValidationSymAlgo, bool legacyMode)
        {
            ct.Dispose();
        }

        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        static byte[] s_ahexval;

        // This API is obsolete because it is insecure: invalid hex chars are silently replaced with '0',
        // which can reduce the overall security of the system. But unfortunately, some code is dependent
        // on this broken behavior.
        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        static internal byte[] HexStringToByteArray(String str)
        {
            if (((uint)str.Length & 0x1) == 0x1) // must be 2 nibbles per byte
            {
                return null;
            }
            byte[] ahexval = s_ahexval; // initialize a table for faster lookups
            if (ahexval == null)
            {
                ahexval = new byte['f' + 1];
                for (int i = ahexval.Length; --i >= 0; )
                {
                    if ('0' <= i && i <= '9')
                    {
                        ahexval[i] = (byte)(i - '0');
                    }
                    else if ('a' <= i && i <= 'f')
                    {
                        ahexval[i] = (byte)(i - 'a' + 10);
                    }
                    else if ('A' <= i && i <= 'F')
                    {
                        ahexval[i] = (byte)(i - 'A' + 10);
                    }
                }

                s_ahexval = ahexval;
            }

            byte[] result = new byte[str.Length / 2];
            int istr = 0, ir = 0;
            int n = result.Length;
            while (--n >= 0)
            {
                int c1, c2;
                try
                {
                    c1 = ahexval[str[istr++]];
                }
                catch (ArgumentNullException)
                {
                    c1 = 0;
                    return null;// Inavlid char
                }
                catch (ArgumentException)
                {
                    c1 = 0;
                    return null;// Inavlid char
                }
                catch (IndexOutOfRangeException)
                {
                    c1 = 0;
                    return null;// Inavlid char
                }

                try
                {
                    c2 = ahexval[str[istr++]];
                }
                catch (ArgumentNullException)
                {
                    c2 = 0;
                    return null;// Inavlid char
                }
                catch (ArgumentException)
                {
                    c2 = 0;
                    return null;// Inavlid char
                }
                catch (IndexOutOfRangeException)
                {
                    c2 = 0;
                    return null;// Inavlid char
                }

                result[ir++] = (byte)((c1 << 4) + c2);
            }

            return result;
        }


        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private void InitValidationAndEncyptionSizes()
        {
            _CustomValidationName = ValidationAlgorithm;
            _CustomValidationTypeIsKeyed = true;
            switch(ValidationAlgorithm)
            {
            case "AES":
            case "3DES":
                _UseHMACSHA = true;
                _HashSize = SHA1_HASH_SIZE;
                _AutoGenValidationKeySize = SHA1_KEY_SIZE;
                break;
            case "SHA1":
                _UseHMACSHA = true;
                _HashSize = SHA1_HASH_SIZE;
                _AutoGenValidationKeySize = SHA1_KEY_SIZE;
                break;
            case "MD5":
                _CustomValidationTypeIsKeyed = false;
                _UseHMACSHA = false;
                _HashSize = MD5_HASH_SIZE;
                _AutoGenValidationKeySize = MD5_KEY_SIZE;
                break;
            case "HMACSHA256":
                _UseHMACSHA = true;
                _HashSize = HMACSHA256_HASH_SIZE;
                _AutoGenValidationKeySize = HMACSHA256_KEY_SIZE;
                break;
            case "HMACSHA384":
                _UseHMACSHA = true;
                _HashSize = HMACSHA384_HASH_SIZE;
                _AutoGenValidationKeySize = HMACSHA384_KEY_SIZE;
                break;
            case "HMACSHA512":
                _UseHMACSHA = true;
                _HashSize = HMACSHA512_HASH_SIZE;
                _AutoGenValidationKeySize = HMACSHA512_KEY_SIZE;
                break;
            default:
                _UseHMACSHA = false;
                if (!_CustomValidationName.StartsWith(ALGO_PREFIX, StringComparison.Ordinal)) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_validation_enum),
                                                           ElementInformation.Properties["validation"].Source,
                                                           ElementInformation.Properties["validation"].LineNumber);
                }
                _CustomValidationName = _CustomValidationName.Substring(ALGO_PREFIX.Length);
                HashAlgorithm alg = null;
                try {
                    _CustomValidationTypeIsKeyed = false;
                    alg = HashAlgorithm.Create(_CustomValidationName);
                } catch (Exception e) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_validation_enum), e,
                                                           ElementInformation.Properties["validation"].Source,
                                                           ElementInformation.Properties["validation"].LineNumber);
                }
                if (alg == null)
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_validation_enum),
                                                           ElementInformation.Properties["validation"].Source,
                                                           ElementInformation.Properties["validation"].LineNumber);

                _AutoGenValidationKeySize = 0;
                _HashSize = 0;
                _CustomValidationTypeIsKeyed = (alg is KeyedHashAlgorithm);
                if (!_CustomValidationTypeIsKeyed) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_validation_enum),
                                                           ElementInformation.Properties["validation"].Source,
                                                           ElementInformation.Properties["validation"].LineNumber);
                }

                try {
                    _HashSize = RoundupNumBitsToNumBytes(alg.HashSize);
                    if (_CustomValidationTypeIsKeyed)
                        _AutoGenValidationKeySize = ((KeyedHashAlgorithm) alg).Key.Length;
                    if (_AutoGenValidationKeySize < 1)
                        _AutoGenValidationKeySize = RoundupNumBitsToNumBytes(alg.InputBlockSize);
                    if (_AutoGenValidationKeySize < 1)
                        _AutoGenValidationKeySize = RoundupNumBitsToNumBytes(alg.OutputBlockSize);
                } catch {}

                if (_HashSize < 1 || _AutoGenValidationKeySize < 1) {
                    // If we didn't get the hash-size or key-size, perform a hash and get the sizes
                    byte [] buf = new byte[10];
                    byte [] buf2 = new byte[512];
                    RandomNumberGenerator.GetBytes(buf);
                    RandomNumberGenerator.GetBytes(buf2);
                    byte [] bHash = alg.ComputeHash(buf);

                    _HashSize = bHash.Length;

                    if (_AutoGenValidationKeySize < 1) {
                        if (_CustomValidationTypeIsKeyed)
                            _AutoGenValidationKeySize = ((KeyedHashAlgorithm) alg).Key.Length;
                        else
                            _AutoGenValidationKeySize = RoundupNumBitsToNumBytes(alg.InputBlockSize);
                    }
                    alg.Clear();
                }
                if (_HashSize < 1)
                    _HashSize = HMACSHA512_HASH_SIZE;
                if (_AutoGenValidationKeySize < 1)
                    _AutoGenValidationKeySize = HMACSHA512_KEY_SIZE;
                break;
            }


            _AutoGenDecryptionKeySize = 0;
            switch(Decryption) {
            case "AES":
                _AutoGenDecryptionKeySize = 24;
                break;
            case "3DES":
                _AutoGenDecryptionKeySize = 24;
                break;
            case "Auto":
                _AutoGenDecryptionKeySize = 24;
                break;
            case "DES":
                if (ValidationAlgorithm == "AES" || ValidationAlgorithm == "3DES")
                    _AutoGenDecryptionKeySize = 24;
                else
                    _AutoGenDecryptionKeySize = 8;
                break;
            default:
                _UsingCustomEncryption = true;
                if (!Decryption.StartsWith(ALGO_PREFIX, StringComparison.Ordinal)) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_decryption_enum),
                                                           ElementInformation.Properties["decryption"].Source,
                                                           ElementInformation.Properties["decryption"].LineNumber);
                }
                try {
                    s_oSymAlgoDecryption = SymmetricAlgorithm.Create(Decryption.Substring(ALGO_PREFIX.Length));
                } catch(Exception e) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_decryption_enum), e,
                                                           ElementInformation.Properties["decryption"].Source,
                                                           ElementInformation.Properties["decryption"].LineNumber);
                }
                if (s_oSymAlgoDecryption == null)
                    throw new ConfigurationErrorsException(SR.GetString(SR.Wrong_decryption_enum),
                                                           ElementInformation.Properties["decryption"].Source,
                                                           ElementInformation.Properties["decryption"].LineNumber);

                _AutoGenDecryptionKeySize = RoundupNumBitsToNumBytes(s_oSymAlgoDecryption.KeySize);
                break;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        internal static int RoundupNumBitsToNumBytes(int numBits) {
            if (numBits < 0)
                return 0;
            return (numBits / 8) + (((numBits & 7) != 0) ? 1 : 0);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private static byte[] HashDataUsingNonKeyedAlgorithm(HashAlgorithm hashAlgo, byte[] buf, byte[] modifier,
                                                             int start, int length, byte[] validationKey)
        {
            int     totalLength = length + validationKey.Length + ((modifier != null) ? modifier.Length : 0);
            byte [] bAll        = new byte[totalLength];

            Buffer.BlockCopy(buf, start, bAll, 0, length);
            if (modifier != null) {
                Buffer.BlockCopy(modifier, 0, bAll, length, modifier.Length);
            }
            Buffer.BlockCopy(validationKey, 0, bAll, length, validationKey.Length);
            if (hashAlgo != null) {
                return hashAlgo.ComputeHash(bAll);
            } else {
                byte[] newHash = new byte[MD5_HASH_SIZE];
                int hr = UnsafeNativeMethods.GetSHA1Hash(bAll, bAll.Length, newHash, newHash.Length);
                Marshal.ThrowExceptionForHR(hr);
                return newHash;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private static byte[] HashDataUsingKeyedAlgorithm(KeyedHashAlgorithm hashAlgo, byte[] buf, byte[] modifier,
                                                          int start, int length, byte[] validationKey)
        {
            int     totalLength = length + ((modifier != null) ? modifier.Length : 0);
            byte [] bAll        = new byte[totalLength];

            Buffer.BlockCopy(buf, start, bAll, 0, length);
            if (modifier != null) {
                Buffer.BlockCopy(modifier, 0, bAll, length, modifier.Length);
            }
            hashAlgo.Key = validationKey;
            return hashAlgo.ComputeHash(bAll);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        internal static byte[] GetUnHashedData(byte[] bufHashed)
        {
            if (!VerifyHashedData(bufHashed))
                return null;

            byte[] buf2 = new byte[bufHashed.Length - _HashSize];
            Buffer.BlockCopy(bufHashed, 0, buf2, 0, buf2.Length);
           return buf2;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        [Obsolete(OBSOLETE_CRYPTO_API_MESSAGE)]
        internal static bool VerifyHashedData(byte[] bufHashed)
        {
            EnsureConfig();

            //////////////////////////////////////////////////////////////////////
            // Step 1: Get the MAC: Last [HashSize] bytes
            if (bufHashed.Length <= _HashSize)
                return false;

            byte[] bMac = HashData(bufHashed, null, 0, bufHashed.Length - _HashSize);

            //////////////////////////////////////////////////////////////////////
            // Step 2: Make sure the MAC has expected length
            if (bMac == null || bMac.Length != _HashSize)
                return false;
            int lastPos = bufHashed.Length - _HashSize;

            // From Tolga: To prevent a timing attack, we should verify the entire hash instead of failing
            // early the first time we see a mismatched byte.
            bool hashCheckFailed = false;
            for (int iter = 0; iter < _HashSize; iter++)
                if (bMac[iter] != bufHashed[lastPos + iter])
                    hashCheckFailed = true;
            return !hashCheckFailed;
        }

        internal static bool UsingCustomEncryption {
            get {
                EnsureConfig();

                return _UsingCustomEncryption;
            }
        }
        private static void InitLegacyEncAlgorithm(byte [] dKey)
        {
            if (!_UsingCustomEncryption)
                return;

            s_oSymAlgoLegacy = CryptoAlgorithms.CreateAes();
            try {
                s_oSymAlgoLegacy.Key = dKey;
            } catch {
                if (dKey.Length <= 24)
                    throw;
                byte [] buf = new byte[24];
                Buffer.BlockCopy(dKey, 0, buf, 0, buf.Length);
                dKey = buf;
                s_oSymAlgoLegacy.Key = dKey;
            }
        }

        // This is called as the last step of the deserialization process before the newly created section is seen by the consumer.
        // We can use it to change defaults on-the-fly.
        protected override void SetReadOnly() {
            // Unless overridden, set <machineKey compatibilityMode="Framework45" />
            ConfigUtil.SetFX45DefaultValue(this, _propCompatibilityMode, MachineKeyCompatibilityMode.Framework45);

            base.SetReadOnly();
        }
    }
}
