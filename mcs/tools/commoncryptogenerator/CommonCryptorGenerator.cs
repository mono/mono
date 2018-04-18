//
// CommonCrypto code generator for symmetric block cipher algorithms
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.
//

using System;
using System.IO;

namespace Xamarin {

	public static class CommonCryptor {
		
		static public void Generate (string namespaceName, string typeName, string baseTypeName, string ccAlgorithmName, string feedbackSize = "8", string ctorInitializers = null, string decryptorInitializers = null, string encryptorInitializers = null, string properties = null)
		{
			string template = @"// Generated file to bind CommonCrypto cipher algorithms - DO NOT EDIT
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2014 Xamarin Inc.

using System;
using System.Security.Cryptography;

using Mono.Security.Cryptography;
using Crimson.CommonCrypto;

namespace %NAMESPACE% {

	public sealed partial class %TYPE% : %BASE% {
		
		public %TYPE% ()
		{
			FeedbackSizeValue = %FEEDBACKSIZE%;
			%CTOR_INIT%
		}

		%PROPERTIES%

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
			%CREATEDECRYPTOR_INIT%

			IntPtr decryptor = IntPtr.Zero;
			switch (Mode) {
			case CipherMode.CBC:
				decryptor = Cryptor.Create (CCOperation.Decrypt, CCAlgorithm.%CCALGORITHM%, CCOptions.None, rgbKey, rgbIV);
				return new FastCryptorTransform (decryptor, this, false, rgbIV);
			case CipherMode.ECB:
				decryptor = Cryptor.Create (CCOperation.Decrypt, CCAlgorithm.%CCALGORITHM%, CCOptions.ECBMode, rgbKey, rgbIV);
				return new FastCryptorTransform (decryptor, this, false, rgbIV);
			case CipherMode.CFB:
#if MONOTOUCH || XAMMAC
				IntPtr encryptor = Cryptor.Create (CCOperation.Encrypt, CCAlgorithm.%CCALGORITHM%, CCOptions.ECBMode, rgbKey, rgbIV);
				decryptor = Cryptor.Create (CCOperation.Decrypt, CCAlgorithm.%CCALGORITHM%, CCOptions.ECBMode, rgbKey, rgbIV);
				return new CryptorTransform (decryptor, encryptor, this, false, rgbIV);
#else
				throw new CryptographicException (""CFB is not supported by Crimson.CommonCrypto"");
#endif
			default:
				throw new CryptographicException (String.Format (""{0} is not supported by the .NET framework"", Mode));
			}
		}
		
		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			%CREATEENCRYPTOR_INIT%
			
			IntPtr encryptor = IntPtr.Zero;
			switch (Mode) {
			case CipherMode.CBC:
				encryptor = Cryptor.Create (CCOperation.Encrypt, CCAlgorithm.%CCALGORITHM%, CCOptions.None, rgbKey, rgbIV);
				return new FastCryptorTransform (encryptor, this, true, rgbIV);
			case CipherMode.ECB:
				encryptor = Cryptor.Create (CCOperation.Encrypt, CCAlgorithm.%CCALGORITHM%, CCOptions.ECBMode, rgbKey, rgbIV);
				return new FastCryptorTransform (encryptor, this, true, rgbIV);
			case CipherMode.CFB:
#if MONOTOUCH || XAMMAC
				encryptor = Cryptor.Create (CCOperation.Encrypt, CCAlgorithm.%CCALGORITHM%, CCOptions.ECBMode, rgbKey, rgbIV);
				return new CryptorTransform (encryptor, IntPtr.Zero, this, true, rgbIV);
#else
				throw new CryptographicException (""CFB is not supported by Crimson.CommonCrypto"");
#endif
			default:
				throw new CryptographicException (String.Format (""{0} is not supported by the .NET framework"", Mode));
			}
		}
	}
}";
			
			File.WriteAllText (typeName + ".g.cs", template.Replace ("%NAMESPACE%", namespaceName).
				Replace ("%TYPE%", typeName).Replace ("%BASE%", baseTypeName).Replace("%FEEDBACKSIZE%", feedbackSize).Replace ("%CTOR_INIT%", ctorInitializers).
				Replace ("%CREATEDECRYPTOR_INIT%", decryptorInitializers).
				Replace ("%CREATEENCRYPTOR_INIT%", encryptorInitializers).
				Replace ("%PROPERTIES%", properties).
				Replace ("%CCALGORITHM%", ccAlgorithmName.ToString ()));
		}
	}
}
