//
// RijndaelManaged.cs: Use CommonCrypto AES when possible, 
//	fallback on RijndaelManagedTransform otherwise
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.
//

using System;
using System.Security.Cryptography;

using Mono.Security.Cryptography;
using Crimson.CommonCrypto;

namespace System.Security.Cryptography {
	
	public sealed class RijndaelManaged : Rijndael {
		
		public RijndaelManaged ()
		{
		}
		
		public override void GenerateIV ()
		{
			IVValue = KeyBuilder.IV (BlockSizeValue >> 3);
		}
		
		public override void GenerateKey ()
		{
			KeyValue = KeyBuilder.Key (KeySizeValue >> 3);
		}
		
		public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			// AES is Rijndael with a 128 bits block size, so we can use CommonCrypto in this case
			if (BlockSize == 128) {
				IntPtr decryptor = IntPtr.Zero;
				switch (Mode) {
				case CipherMode.CBC:
					decryptor = Cryptor.Create (CCOperation.Decrypt, CCAlgorithm.AES128, CCOptions.None, rgbKey, rgbIV);
					return new FastCryptorTransform (decryptor, this, false, rgbIV);
				case CipherMode.ECB:
					decryptor = Cryptor.Create (CCOperation.Decrypt, CCAlgorithm.AES128, CCOptions.ECBMode, rgbKey, rgbIV);
					return new FastCryptorTransform (decryptor, this, false, rgbIV);
				default:
					// CFB cipher mode is not supported by the (old) API we used (for compatibility) so we fallback for them
					// FIXME: benchmark if we're better with RijndaelManagedTransform or CryptorTransform for CFB mode
					break;
				}
			}

            return NewEncryptor(rgbKey,
                                ModeValue,
                                rgbIV,
                                FeedbackSizeValue,
                                RijndaelManagedTransformMode.Decrypt);
		}
		
		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			if (BlockSize == 128) {
				IntPtr encryptor = IntPtr.Zero;
				switch (Mode) {
				case CipherMode.CBC:
					encryptor = Cryptor.Create (CCOperation.Encrypt, CCAlgorithm.AES128, CCOptions.None, rgbKey, rgbIV);
					return new FastCryptorTransform (encryptor, this, true, rgbIV);
				case CipherMode.ECB:
					encryptor = Cryptor.Create (CCOperation.Encrypt, CCAlgorithm.AES128, CCOptions.ECBMode, rgbKey, rgbIV);
					return new FastCryptorTransform (encryptor, this, true, rgbIV);
				default:
					// CFB cipher mode is not supported by the (old) API we used (for compatibility) so we fallback for them
					// FIXME: benchmark if we're better with RijndaelManagedTransform or CryptorTransform for CFB mode
					break;
				}
			}

            return NewEncryptor(rgbKey,
                                ModeValue,
                                rgbIV,
                                FeedbackSizeValue,
                                RijndaelManagedTransformMode.Encrypt);
        }


        private ICryptoTransform NewEncryptor (byte[] rgbKey,
                                               CipherMode mode,
                                               byte[] rgbIV,
                                               int feedbackSize,
                                               RijndaelManagedTransformMode encryptMode) {
            // Build the key if one does not already exist
            if (rgbKey == null) {
                rgbKey = Utils.GenerateRandom(KeySizeValue / 8);
            }

            // If not ECB mode, make sure we have an IV. In CoreCLR we do not support ECB, so we must have
            // an IV in all cases.
#if !FEATURE_CRYPTO
            if (mode != CipherMode.ECB) {
#endif // !FEATURE_CRYPTO
                if (rgbIV == null) {
                    rgbIV = Utils.GenerateRandom(BlockSizeValue / 8);
                }
#if !FEATURE_CRYPTO
            }
#endif // !FEATURE_CRYPTO

            // Create the encryptor/decryptor object
            return new RijndaelManagedTransform (rgbKey,
                                                 mode,
                                                 rgbIV,
                                                 BlockSizeValue,
                                                 feedbackSize,
                                                 PaddingValue,
                                                 encryptMode);
        }                            
	}
}