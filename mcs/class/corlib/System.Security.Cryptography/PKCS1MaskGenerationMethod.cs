//
// PKCS1MaskGenerationMethod.cs: Handles PKCS#1 mask generation.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	// References:
	// a.	PKCS#1: RSA Cryptography Standard 
	//	http://www.rsasecurity.com/rsalabs/pkcs/pkcs-1/index.html
	
	public class PKCS1MaskGenerationMethod : MaskGenerationMethod {

		private string hashName;
	
		public PKCS1MaskGenerationMethod()
		{
			hashName = "SHA1";
		}
	
		public string HashName 
		{
			get { return hashName; }
			set { 
				if (value == null)
					hashName = "SHA1";
				else
					hashName = value; 
			}
		}
	
		public override byte[] GenerateMask (byte[] mgfSeed, int maskLen)
		{
			HashAlgorithm hash = HashAlgorithm.Create (hashName);
			return PKCS1.MGF1 (hash, mgfSeed, maskLen);
		}
	}
}
