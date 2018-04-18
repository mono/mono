//
// CommonCrypto Code Generator
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Xamarin {
	
	class Program {
		static void Main (string [] args)
		{
			// mscorlib replacements
			CommonDigest.Generate ("System.Security.Cryptography", "MD5CryptoServiceProvider", "MD5", 1000);
			CommonDigest.Generate ("System.Security.Cryptography", "SHA1CryptoServiceProvider", "SHA1", 1000);
			CommonDigest.Generate ("System.Security.Cryptography", "SHA1Managed", "SHA1", 1000);
			CommonDigest.Generate ("System.Security.Cryptography", "SHA256Managed", "SHA256", 1000);
			CommonDigest.Generate ("System.Security.Cryptography", "SHA384Managed", "SHA384", 1000);
			CommonDigest.Generate ("System.Security.Cryptography", "SHA512Managed", "SHA512", 1000);
			
			// System.Core replacements - not yet in MT profile (4.0 - functional dupes anyway)
			//CommonDigest.Generate ("System.Security.Cryptography", "MD5Cng", "MD5", 1000);
			//CommonDigest.Generate ("System.Security.Cryptography", "SHA256Cng", "SHA256", 1000);
			//CommonDigest.Generate ("System.Security.Cryptography", "SHA384Cng", "SHA384", 1000);
			//CommonDigest.Generate ("System.Security.Cryptography", "SHA512Cng", "SHA512", 1000);
			//CommonDigest.Generate ("System.Security.Cryptography", "SHA256CryptoServiceProvider", "SHA256", 1000);
			//CommonDigest.Generate ("System.Security.Cryptography", "SHA384CryptoServiceProvider", "SHA384", 1000);
			//CommonDigest.Generate ("System.Security.Cryptography", "SHA512CryptoServiceProvider", "SHA512", 1000);
			
			// Mono.Security replacements
			CommonDigest.Generate ("Mono.Security.Cryptography", "MD2Managed", "MD2", 1000, "#if !INSIDE_CORLIB", "#endif");
			CommonDigest.Generate ("Mono.Security.Cryptography", "MD4Managed", "MD4", 1000, "#if !INSIDE_CORLIB", "#endif");
			CommonDigest.Generate ("Mono.Security.Cryptography", "SHA224Managed", "SHA224", 1000);

			// mscorlib replacements
			CommonCryptor.Generate ("System.Security.Cryptography", "DESCryptoServiceProvider", "DES", "DES");
			CommonCryptor.Generate ("System.Security.Cryptography", "TripleDESCryptoServiceProvider", "TripleDES", "TripleDES");

			const string checkUseSalt = "if (UseSalt) throw new NotImplementedException (\"UseSalt=true is not implemented on Mono yet\");";
			CommonCryptor.Generate ("System.Security.Cryptography", "RC2CryptoServiceProvider", "RC2", "RC2",
				ctorInitializers: "LegalKeySizesValue = new[] { new KeySizes(40, 128, 8) };",
				decryptorInitializers: checkUseSalt,
				encryptorInitializers: checkUseSalt,
				properties: "public bool UseSalt { get; set; }");
			// Rijndael supports block sizes that are not available in AES - as such it does not use the same generated code
			// but has it's own version, using AES (128 bits block size) and falling back to managed (192/256 bits block size)

			// System.Core replacements
			CommonCryptor.Generate ("System.Security.Cryptography", "AesManaged", "Aes", "AES128", "128");
			CommonCryptor.Generate ("System.Security.Cryptography", "AesCryptoServiceProvider", "Aes", "AES128");

			// Mono.Security replacements
			// RC4 is a stream (not a block) cipher so it can not reuse the generated code
		}
	}
}