//
// RSAPKCS1SignatureDeformatter.cs - Handles PKCS#1 v.1.5 signature decryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography { 
	
	public class RSAPKCS1SignatureDeformatter : AsymmetricSignatureDeformatter {
	
		private RSA rsa;
		private string hashName;
	
		public RSAPKCS1SignatureDeformatter () {}
	
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
			if (key != null)
				rsa = (RSA)key;
			// here null is accepted without an ArgumentNullException!
		}
	
		public override bool VerifySignature (byte[] rgbHash, byte[] rgbSignature) 
		{
			if (rsa == null)
				throw new CryptographicUnexpectedOperationException ("missing key");
			if (hashName == null)
				throw new CryptographicUnexpectedOperationException ("missing hash algorithm");
			if (rgbHash == null)
				throw new ArgumentNullException ("rgbHash");
			if (rgbSignature == null)
				throw new ArgumentNullException ("rgbSignature");

			return PKCS1.Verify_v15 (rsa, HashAlgorithm.Create (hashName), rgbHash, rgbSignature);
		}
	}
}
