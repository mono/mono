// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
#if !SILVERLIGHT
using System.Diagnostics.Contracts;
#endif // !SILVERLIGHT

namespace System.Security.Cryptography {
    /// <summary>
    ///     Managed implementation of the AES algorithm. AES is esentially Rijndael with a fixed block size
    ///     and iteration count, so we just wrap the RijndaelManaged class and allow only 128 bit blocks to
    ///     be used.
    /// </summary>
    public sealed class AesManaged : Aes {
        private RijndaelManaged m_rijndael;

        public AesManaged() {
#if !SILVERLIGHT
            Contract.Ensures(m_rijndael != null);

            if (CryptoConfig.AllowOnlyFipsAlgorithms) {
                throw new InvalidOperationException(SR.GetString(SR.Cryptography_NonCompliantFIPSAlgorithm));
            }
#endif // !SILVERLIGHT

            m_rijndael = new RijndaelManaged();
            m_rijndael.BlockSize = BlockSize;
            m_rijndael.KeySize = KeySize;
        }

#if !SILVERLIGHT
        public override int FeedbackSize {
            get { return m_rijndael.FeedbackSize; }
            set { m_rijndael.FeedbackSize = value; }
        }
#endif // !SILVERLIGHT

        public override byte[] IV {
            get { return m_rijndael.IV; }
            set { m_rijndael.IV = value; }
        }

        public override byte[] Key {
            get { return m_rijndael.Key; }
            set { m_rijndael.Key = value; }
        }

        public override int KeySize {
            get { return m_rijndael.KeySize; }
            set { m_rijndael.KeySize = value; }
        }

#if !SILVERLIGHT
        public override CipherMode Mode {
            get { return m_rijndael.Mode; }

            set {
                Contract.Ensures(m_rijndael.Mode != CipherMode.CFB && m_rijndael.Mode != CipherMode.OFB);

                // RijndaelManaged will implicitly change the block size of an algorithm to match the number
                // of feedback bits being used. Since AES requires a block size of 128 bits, we cannot allow
                // the user to use the feedback modes, as this will end up breaking that invarient.
                if (value == CipherMode.CFB || value == CipherMode.OFB) {
                    throw new CryptographicException(SR.GetString(SR.Cryptography_InvalidCipherMode));
                }

                m_rijndael.Mode = value;
            }
        }

        public override PaddingMode Padding {
            get { return m_rijndael.Padding; }
            set { m_rijndael.Padding = value; }
        }
#endif // !SILVERLIGHT

        public override ICryptoTransform CreateDecryptor() {
            return m_rijndael.CreateDecryptor();
        }

        public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }
#if !SILVERLIGHT
            if (!ValidKeySize(key.Length * 8)) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidKeySize), "key");
            }
            if (iv != null && iv.Length * 8 != BlockSizeValue) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidIVSize), "iv");
            }
#endif

            return m_rijndael.CreateDecryptor(key, iv);
        }


        public override ICryptoTransform CreateEncryptor() {
            return m_rijndael.CreateEncryptor();
        }

        public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }
#if !SILVERLIGHT
            if (!ValidKeySize(key.Length * 8)) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidKeySize), "key");
            }
            if (iv != null && iv.Length * 8 != BlockSizeValue) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_InvalidIVSize), "iv");
            }
#endif // SILVERLIGHT

            return m_rijndael.CreateEncryptor(key, iv);
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    (m_rijndael as IDisposable).Dispose();
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        public override void GenerateIV() {
            m_rijndael.GenerateIV();
        }

        public override void GenerateKey() {
            m_rijndael.GenerateKey();
        }
    }
}
