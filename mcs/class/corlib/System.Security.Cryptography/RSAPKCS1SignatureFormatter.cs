//
// RSAPKCS1SignatureFormatter.cs - Handles PKCS#1 v.1.5 signature encryption.
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography { 
	
	public class RSAPKCS1SignatureFormatter : AsymmetricSignatureFormatter {
	
		private RSA rsa;
		private HashAlgorithm hash;
	
		public RSAPKCS1SignatureFormatter ()
		{
		}
	
		public RSAPKCS1SignatureFormatter (AsymmetricAlgorithm key) 
		{
			SetKey (key);
		}
	
		public override byte[] CreateSignature (byte[] rgbHash) 
		{
			if (rsa == null) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("No key pair available."));
			}
			if (hash == null) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("Missing hash algorithm."));
			}
			if (rgbHash == null)
				throw new ArgumentNullException ("rgbHash");

			return PKCS1.Sign_v15 (rsa, hash, rgbHash);
		}
	
		public override void SetHashAlgorithm (string strName) 
		{
			hash = HashAlgorithm.Create (strName);
		}
	
		public override void SetKey (AsymmetricAlgorithm key) 
		{
			rsa = (RSA) key;
		}
	}
}
