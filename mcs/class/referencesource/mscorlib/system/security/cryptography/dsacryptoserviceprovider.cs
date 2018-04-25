// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// DSACryptoServiceProvider.cs
//
// CSP-based implementation of DSA
//

namespace System.Security.Cryptography {
    using System;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;

    // Object layout of the DSAParameters structure
    internal class DSACspObject {
        internal byte[] P;
        internal byte[] Q;
        internal byte[] G;
        internal byte[] Y;
        internal byte[] J;
        internal byte[] X;
        internal byte[] Seed;
        internal int Counter;
    }

    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class DSACryptoServiceProvider : DSA, ICspAsymmetricAlgorithm {
        private int _dwKeySize;
        private CspParameters _parameters;
        private bool _randomKeyContainer;
        [System.Security.SecurityCritical] // auto-generated
        private SafeProvHandle _safeProvHandle;
        [System.Security.SecurityCritical] // auto-generated
        private SafeKeyHandle _safeKeyHandle;
        private SHA1CryptoServiceProvider _sha1;

        private static volatile CspProviderFlags s_UseMachineKeyStore = 0;

        //
        // public constructors
        //

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public DSACryptoServiceProvider() 
            : this(0, new CspParameters(Constants.PROV_DSS_DH, null, null, s_UseMachineKeyStore)) {
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public DSACryptoServiceProvider(int dwKeySize)
            : this(dwKeySize, new CspParameters(Constants.PROV_DSS_DH, null, null, s_UseMachineKeyStore)) {
        }

        public DSACryptoServiceProvider(CspParameters parameters) 
            : this(0, parameters) {
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public DSACryptoServiceProvider(int dwKeySize, CspParameters parameters) {
            if (dwKeySize < 0)
                throw new ArgumentOutOfRangeException("dwKeySize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            Contract.EndContractBlock();

            _parameters = Utils.SaveCspParameters(CspAlgorithmType.Dss, parameters, s_UseMachineKeyStore, ref _randomKeyContainer);
            LegalKeySizesValue = new KeySizes[] { new KeySizes(512, 1024, 64) }; // per the DSS spec
            _dwKeySize = dwKeySize;
            _sha1 = new SHA1CryptoServiceProvider();

            // If this is not a random container we generate, create it eagerly 
            // in the constructor so we can report any errors now.
            if (!_randomKeyContainer || Environment.GetCompatibilityFlag(CompatibilityFlag.EagerlyGenerateRandomAsymmKeys))
                GetKeyPair();
        }

        //
        // private methods
        //

        [System.Security.SecurityCritical]  // auto-generated
        private void GetKeyPair () {
            if (_safeKeyHandle == null) {
                lock (this) {
                    if (_safeKeyHandle == null)
                        Utils.GetKeyPairHelper(CspAlgorithmType.Dss, _parameters, _randomKeyContainer, _dwKeySize, ref _safeProvHandle, ref _safeKeyHandle);
                }
            }
        }

        [System.Security.SecuritySafeCritical] // overrides public transparent member
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (_safeKeyHandle != null && !_safeKeyHandle.IsClosed)
                _safeKeyHandle.Dispose();
            if (_safeProvHandle != null && !_safeProvHandle.IsClosed)
                _safeProvHandle.Dispose();
        }

        //
        // public properties
        //

        [System.Runtime.InteropServices.ComVisible(false)]
        public bool PublicOnly {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                GetKeyPair();
                byte[] publicKey = (byte[]) Utils._GetKeyParameter(_safeKeyHandle, Constants.CLR_PUBLICKEYONLY);
                return (publicKey[0] == 1);
            }
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        public CspKeyContainerInfo CspKeyContainerInfo {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                GetKeyPair();
                return new CspKeyContainerInfo(_parameters, _randomKeyContainer);
            }
        }

        public override int KeySize {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                GetKeyPair();
                byte[] keySize = (byte[]) Utils._GetKeyParameter(_safeKeyHandle, Constants.CLR_KEYLEN);
                _dwKeySize = (keySize[0] | (keySize[1] << 8) | (keySize[2] << 16) | (keySize[3] << 24));
                return _dwKeySize;
            }
        }

        public override string KeyExchangeAlgorithm {
            get { return null; }
        }

        public override string SignatureAlgorithm {
            get { return "http://www.w3.org/2000/09/xmldsig#dsa-sha1"; }
        }

        public static bool UseMachineKeyStore {
            get { return (s_UseMachineKeyStore == CspProviderFlags.UseMachineKeyStore); }
            set { s_UseMachineKeyStore = (value ? CspProviderFlags.UseMachineKeyStore : 0); }
        }

        public bool PersistKeyInCsp {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                if (_safeProvHandle == null) {
                    lock (this) {
                        if (_safeProvHandle == null)
                            _safeProvHandle = Utils.CreateProvHandle(_parameters, _randomKeyContainer);
                    }
                }
                return Utils.GetPersistKeyInCsp(_safeProvHandle);
            }
            [System.Security.SecuritySafeCritical]  // auto-generated
            set {
                bool oldPersistKeyInCsp = this.PersistKeyInCsp;
                if (value == oldPersistKeyInCsp)
                    return;

                KeyContainerPermission kp = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                if (!value) {
                    KeyContainerPermissionAccessEntry entry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Delete);
                    kp.AccessEntries.Add(entry);
                } else {
                    KeyContainerPermissionAccessEntry entry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Create);
                    kp.AccessEntries.Add(entry);
                }
                kp.Demand();

                Utils.SetPersistKeyInCsp(_safeProvHandle, value);
            }
        }

        //
        // public methods
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override DSAParameters ExportParameters (bool includePrivateParameters) {
            GetKeyPair();
            if (includePrivateParameters) {
                KeyContainerPermission kp = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry entry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Export);
                kp.AccessEntries.Add(entry);
                kp.Demand();
            }
            DSACspObject dsaCspObject = new DSACspObject();
            int blobType = includePrivateParameters ? Constants.PRIVATEKEYBLOB : Constants.PUBLICKEYBLOB;
            // _ExportKey will check for failures and throw an exception
            Utils._ExportKey(_safeKeyHandle, blobType, dsaCspObject);
            return DSAObjectToStruct(dsaCspObject);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [System.Runtime.InteropServices.ComVisible(false)]
        public byte[] ExportCspBlob (bool includePrivateParameters) {
            GetKeyPair();
            return Utils.ExportCspBlobHelper(includePrivateParameters, _parameters, _safeKeyHandle);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override void ImportParameters(DSAParameters parameters) {
            DSACspObject dsaCspObject = DSAStructToObject(parameters);
            // Free the current key handle
            if (_safeKeyHandle != null && !_safeKeyHandle.IsClosed)
                _safeKeyHandle.Dispose();
            _safeKeyHandle = SafeKeyHandle.InvalidHandle;

            if (IsPublic(parameters)) {
                // Use our CRYPT_VERIFYCONTEXT handle, CRYPT_EXPORTABLE is not applicable to public only keys, so pass false
                Utils._ImportKey(Utils.StaticDssProvHandle, Constants.CALG_DSS_SIGN, (CspProviderFlags) 0, dsaCspObject, ref _safeKeyHandle);
            } else {
                KeyContainerPermission kp = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry entry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Import);
                kp.AccessEntries.Add(entry);
                kp.Demand();
                if (_safeProvHandle == null)
                    _safeProvHandle = Utils.CreateProvHandle(_parameters, _randomKeyContainer);
                // Now, import the key into the CSP; _ImportKey will check for failures.
                Utils._ImportKey(_safeProvHandle, Constants.CALG_DSS_SIGN, _parameters.Flags, dsaCspObject, ref _safeKeyHandle);
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [System.Runtime.InteropServices.ComVisible(false)]
        public void ImportCspBlob (byte[] keyBlob) {
            Utils.ImportCspBlobHelper(CspAlgorithmType.Dss, keyBlob, IsPublic(keyBlob), ref _parameters, _randomKeyContainer, ref _safeProvHandle, ref _safeKeyHandle);
        }

        public byte[] SignData(Stream inputStream) {
            byte[] hashVal = _sha1.ComputeHash(inputStream);
            return SignHash(hashVal, null);
        }

        public byte[] SignData(byte[] buffer) {
            byte[] hashVal = _sha1.ComputeHash(buffer);
            return SignHash(hashVal, null);
        }

        public byte[] SignData(byte[] buffer, int offset, int count) {
            byte[] hashVal = _sha1.ComputeHash(buffer, offset, count);
            return SignHash(hashVal, null);
        }

        public bool VerifyData(byte[] rgbData, byte[] rgbSignature) {
            byte[] hashVal = _sha1.ComputeHash(rgbData);
            return VerifyHash(hashVal, null, rgbSignature);
        }

        override public byte[] CreateSignature(byte[] rgbHash) {
            return SignHash(rgbHash, null);
        }

        override public bool VerifySignature(byte[] rgbHash, byte[] rgbSignature) {
            return VerifyHash(rgbHash, null, rgbSignature);
        }

        protected override byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm)
        {
            // we're sealed and the base should have checked this before calling us
            Contract.Assert(data != null);
            Contract.Assert(offset >= 0 && offset <= data.Length);
            Contract.Assert(count >= 0 && count <= data.Length - offset);
            Contract.Assert(!String.IsNullOrEmpty(hashAlgorithm.Name));

            if (hashAlgorithm != HashAlgorithmName.SHA1)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_UnknownHashAlgorithm", hashAlgorithm.Name));
            }

            return _sha1.ComputeHash(data, offset, count);
        }

        protected override byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm)
        {
            // we're sealed and the base should have checked this before calling us
            Contract.Assert(data != null);
            Contract.Assert(!String.IsNullOrEmpty(hashAlgorithm.Name));

            if (hashAlgorithm != HashAlgorithmName.SHA1)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_UnknownHashAlgorithm", hashAlgorithm.Name));
            }

            return _sha1.ComputeHash(data);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public byte[] SignHash(byte[] rgbHash, string str) {
            if (rgbHash == null)
                throw new ArgumentNullException("rgbHash");
            Contract.EndContractBlock();
            if (PublicOnly)
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NoPrivateKey"));

            int calgHash = X509Utils.NameOrOidToAlgId(str, OidGroup.HashAlgorithm);
            if (rgbHash.Length != _sha1.HashSize / 8)
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHashSize", "SHA1", _sha1.HashSize / 8));

            GetKeyPair();
            if (!CspKeyContainerInfo.RandomlyGenerated) {
                KeyContainerPermission kp = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry entry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Sign);
                kp.AccessEntries.Add(entry);
                kp.Demand();
            }
            return Utils.SignValue(_safeKeyHandle, _parameters.KeyNumber, Constants.CALG_DSS_SIGN, calgHash, rgbHash);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public bool VerifyHash(byte[] rgbHash, string str, byte[] rgbSignature) {
            if (rgbHash == null)
                throw new ArgumentNullException("rgbHash");
            if (rgbSignature == null)
                throw new ArgumentNullException("rgbSignature");
            Contract.EndContractBlock();

            int calgHash = X509Utils.NameOrOidToAlgId(str, OidGroup.HashAlgorithm);
            if (rgbHash.Length != _sha1.HashSize / 8) 
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHashSize", "SHA1", _sha1.HashSize / 8));

            GetKeyPair();
            return Utils.VerifySign(_safeKeyHandle, Constants.CALG_DSS_SIGN, calgHash, rgbHash, rgbSignature);
        }

        //
        // private static methods
        //

        private static DSAParameters DSAObjectToStruct (DSACspObject dsaCspObject) {
            DSAParameters dsaParams = new DSAParameters();
            dsaParams.P = dsaCspObject.P;
            dsaParams.Q = dsaCspObject.Q;
            dsaParams.G = dsaCspObject.G;
            dsaParams.Y = dsaCspObject.Y;
            dsaParams.J = dsaCspObject.J;
            dsaParams.X = dsaCspObject.X;
            dsaParams.Seed = dsaCspObject.Seed;
            dsaParams.Counter = dsaCspObject.Counter;
            return dsaParams;
        }

        private static DSACspObject DSAStructToObject (DSAParameters dsaParams) {
            DSACspObject dsaCspObject = new DSACspObject();
            dsaCspObject.P = dsaParams.P;
            dsaCspObject.Q = dsaParams.Q;
            dsaCspObject.G = dsaParams.G;
            dsaCspObject.Y = dsaParams.Y;
            dsaCspObject.J = dsaParams.J;
            dsaCspObject.X = dsaParams.X;
            dsaCspObject.Seed = dsaParams.Seed;
            dsaCspObject.Counter = dsaParams.Counter;
            return dsaCspObject;
        }

        private static bool IsPublic (DSAParameters dsaParams) {
            return (dsaParams.X == null);
        }

        // find whether a DSS key blob is public.
        private static bool IsPublic (byte[] keyBlob) {
            if (keyBlob == null)
                throw new ArgumentNullException("keyBlob");
            Contract.EndContractBlock();

            // The CAPI DSS public key representation consists of the following sequence:
            //  - BLOBHEADER
            //  - DSSPUBKEY
            //  - rgbP[cbKey]
            //  - rgbQ[20]
            //  - rgbG[cbKey]
            //  - rgbY[cbKey]
            //  - DSSSEED

            // The first should be PUBLICKEYBLOB and magic should be DSS_MAGIC "DSS1" or DSS_PUB_MAGIC_VER3 "DSS3"
            if (keyBlob[0] != Constants.PUBLICKEYBLOB)
                return false;

            if ((keyBlob[11] != 0x31 && keyBlob[11] != 0x33) || keyBlob[10] != 0x53 || keyBlob[9] != 0x53 || keyBlob[8] != 0x44)
                return false;

            return true;
        }
    }
}
