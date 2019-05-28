// ==++==   
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// AsymmetricAlgorithm.cs
//

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public abstract class AsymmetricAlgorithm : IDisposable {
        protected int KeySizeValue;
        protected KeySizes[] LegalKeySizesValue;

        //
        // public constructors
        //

        protected AsymmetricAlgorithm() {}

        // AsymmetricAlgorithm implements IDisposable

        public void Dispose() {
            Clear();
        }

        public void Clear() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            return;
        }
    
        //
        // public properties
        //
    
        public virtual int KeySize {
            get { return KeySizeValue; }
            set {
                int   i;
                int   j;

                for (i=0; i<LegalKeySizesValue.Length; i++) {
                    if (LegalKeySizesValue[i].SkipSize == 0) {
                        if (LegalKeySizesValue[i].MinSize == value) { // assume MinSize = MaxSize
                            KeySizeValue = value;
                            return;
                        }
                    } else {
                        for (j = LegalKeySizesValue[i].MinSize; j<=LegalKeySizesValue[i].MaxSize;
                             j += LegalKeySizesValue[i].SkipSize) {
                            if (j == value) {
                                KeySizeValue = value;
                                return;
                            }
                        }
                    }
                }
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeySize"));
            }
        }
        
        public virtual KeySizes[] LegalKeySizes { 
            get { return (KeySizes[]) LegalKeySizesValue.Clone(); }
        }
        
        // This method must be implemented by derived classes. In order to conform to the contract, it cannot be abstract.
        public virtual String SignatureAlgorithm {
            get {
                throw new NotImplementedException();
            }
        }

        // This method must be implemented by derived classes. In order to conform to the contract, it cannot be abstract.
        public virtual String KeyExchangeAlgorithm {
            get {
                throw new NotImplementedException();
            }
        }
        
        //
        // public methods
        //

        static public AsymmetricAlgorithm Create() {
#if FULL_AOT_RUNTIME
            return new RSACryptoServiceProvider ();
#else
            // Use the crypto config system to return an instance of
            // the default AsymmetricAlgorithm on this machine
            return Create("System.Security.Cryptography.AsymmetricAlgorithm");
#endif
        }

        static public AsymmetricAlgorithm Create(String algName) {
            return (AsymmetricAlgorithm) CryptoConfig.CreateFromName(algName);
        }

        // This method must be implemented by derived classes. In order to conform to the contract, it cannot be abstract.
        public virtual void FromXmlString(String xmlString) {
            throw new NotImplementedException();
        }

        // This method must be implemented by derived classes. In order to conform to the contract, it cannot be abstract.
        public virtual String ToXmlString(bool includePrivateParameters) {
            throw new NotImplementedException();
        }

#if MONO
        public virtual byte[] ExportEncryptedPkcs8PrivateKey (System.ReadOnlySpan<byte> passwordBytes, System.Security.Cryptography.PbeParameters pbeParameters) => throw new PlatformNotSupportedException ();

        public virtual byte[] ExportEncryptedPkcs8PrivateKey (System.ReadOnlySpan<char> password, System.Security.Cryptography.PbeParameters pbeParameters) => throw new PlatformNotSupportedException ();

        public virtual byte[] ExportPkcs8PrivateKey () => throw new PlatformNotSupportedException ();

        public virtual byte[] ExportSubjectPublicKeyInfo () => throw new PlatformNotSupportedException ();

        public virtual void ImportEncryptedPkcs8PrivateKey (System.ReadOnlySpan<byte> passwordBytes, System.ReadOnlySpan<byte> source, out int bytesRead) => throw new PlatformNotSupportedException ();

        public virtual void ImportEncryptedPkcs8PrivateKey (System.ReadOnlySpan<char> password, System.ReadOnlySpan<byte> source, out int bytesRead) => throw new PlatformNotSupportedException ();

        public virtual void ImportPkcs8PrivateKey (System.ReadOnlySpan<byte> source, out int bytesRead) => throw new PlatformNotSupportedException ();

        public virtual void ImportSubjectPublicKeyInfo (System.ReadOnlySpan<byte> source, out int bytesRead) => throw new PlatformNotSupportedException ();

        public virtual bool TryExportEncryptedPkcs8PrivateKey (System.ReadOnlySpan<byte> passwordBytes, System.Security.Cryptography.PbeParameters pbeParameters, System.Span<byte> destination, out int bytesWritten) => throw new PlatformNotSupportedException ();

        public virtual bool TryExportEncryptedPkcs8PrivateKey (System.ReadOnlySpan<char> password, System.Security.Cryptography.PbeParameters pbeParameters, System.Span<byte> destination, out int bytesWritten) => throw new PlatformNotSupportedException ();

        public virtual bool TryExportPkcs8PrivateKey (System.Span<byte> destination, out int bytesWritten) => throw new PlatformNotSupportedException ();

        public virtual bool TryExportSubjectPublicKeyInfo (System.Span<byte> destination, out int bytesWritten) => throw new PlatformNotSupportedException ();
#endif
    }
}    
