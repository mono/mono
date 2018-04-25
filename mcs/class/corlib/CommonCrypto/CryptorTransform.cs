// ICryptoTransform implementation on top of CommonCrypto and SymmetricTransform
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.

using System;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace Crimson.CommonCrypto {

	class CryptorTransform : SymmetricTransform {
		
		IntPtr handle;
		IntPtr handle_e;
		bool encryption;
		
		public CryptorTransform (IntPtr cryptor, IntPtr special, SymmetricAlgorithm algo, bool encryption, byte[] iv)
			: base (algo, encryption, iv)
		{
			handle = cryptor;
			// for CFB we need to encrypt data while decrypting
			handle_e = special;
			this.encryption = encryption;
		}
		
		~CryptorTransform ()
		{
			Dispose (false);
		}
		
		// PRO: doing this ensure all cipher modes and padding modes supported by .NET will be available with CommonCrypto (drop-in replacements)
		// CON: doing this will only process one block at the time, so it's not ideal for performance, but still a lot better than managed
		protected override void ECB (byte[] input, byte[] output)
		{
			IntPtr len = IntPtr.Zero;
			CCCryptorStatus s = Cryptor.CCCryptorUpdate ((encrypt == encryption) ? handle : handle_e, 
				input, (IntPtr) input.Length, output, (IntPtr) output.Length, ref len);
			if (((int) len != output.Length) || (s != CCCryptorStatus.Success))
				throw new CryptographicUnexpectedOperationException (s.ToString ());
		}
		
		protected override void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				Cryptor.CCCryptorRelease (handle);
				handle = IntPtr.Zero;
			}
			if (handle_e != IntPtr.Zero) {
				Cryptor.CCCryptorRelease (handle_e);
				handle_e = IntPtr.Zero;
			}
			base.Dispose (disposing);
			GC.SuppressFinalize (this);
		}
	}
}