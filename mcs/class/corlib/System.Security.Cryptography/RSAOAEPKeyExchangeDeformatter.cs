//
// RSAOAEPKeyExchangeDeformatter.cs - Handles OAEP keyex decryption.
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography { 

	public class RSAOAEPKeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter {
	
		private RSA rsa;
		private string param;
	
		public RSAOAEPKeyExchangeDeformatter ()
		{
			rsa = null;
		}
	
		public RSAOAEPKeyExchangeDeformatter (AsymmetricAlgorithm key) 
		{
			SetKey (key);
		}
	
		public override string Parameters {
			get { return null; }
			set { ; }
		}
	
		public override byte[] DecryptKeyExchange (byte[] rgbData) 
		{
			SHA1 sha1 = SHA1.Create ();
			return PKCS1.Decrypt_OAEP (rsa, sha1, rgbData);
		}
	
		public override void SetKey (AsymmetricAlgorithm key) 
		{
			rsa = (RSA)key;
		}
	}
}
