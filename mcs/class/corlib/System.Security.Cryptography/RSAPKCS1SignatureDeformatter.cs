//
// RSAPKCS1SignatureDeformatter.cs - Handles PKCS#1 v.1.5 signature decryption.
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography { 
	
	public class RSAPKCS1SignatureDeformatter : AsymmetricSignatureDeformatter {
	
		private RSA rsa;
		private string hashName;
	
		public RSAPKCS1SignatureDeformatter ()
		{
		}
	
		public RSAPKCS1SignatureDeformatter (AsymmetricAlgorithm key) 
		{
			SetKey (key);
		}
	
		public override void SetHashAlgorithm (string strName) 
		{
			if (strName == null)
				throw new ArgumentNullException ("strName");
			hashName = strName;
		}
	
		public override void SetKey (AsymmetricAlgorithm key) 
		{
			rsa = (RSA) key;
			// here null is accepted without an ArgumentNullException!
		}
	
		public override bool VerifySignature (byte[] rgbHash, byte[] rgbSignature) 
		{
			if (rsa == null) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("No public key available."));
			}
			if (hashName == null) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("Missing hash algorithm."));
			}
			if (rgbHash == null)
				throw new ArgumentNullException ("rgbHash");
			if (rgbSignature == null)
				throw new ArgumentNullException ("rgbSignature");

			return PKCS1.Verify_v15 (rsa, HashAlgorithm.Create (hashName), rgbHash, rgbSignature);
		}
	}
}
