//
// RSAPKCS1SignatureFormatter.cs - Handles PKCS#1 v.1.5 signature encryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography { 

public class RSAPKCS1SignatureFormatter : AsymmetricSignatureFormatter {

	private RSA rsa;
	private HashAlgorithm hash;

	public RSAPKCS1SignatureFormatter () 
	{
		rsa = null;
	}

	public RSAPKCS1SignatureFormatter (AsymmetricAlgorithm key) 
	{
		SetKey (key);
	}

	public override byte[] CreateSignature (byte[] rgbHash) 
	{
		if ((rsa == null) || (hash == null))
			throw new CryptographicUnexpectedOperationException ();
		if (rgbHash == null)
			throw new ArgumentNullException ();

		string oid = CryptoConfig.MapNameToOID (hash.ToString ());
		return PKCS1.Sign_v15 (rsa, oid, rgbHash);
	}

	public override void SetHashAlgorithm (string strName) 
	{
		hash = HashAlgorithm.Create (strName);
	}

	public override void SetKey (AsymmetricAlgorithm key) 
	{
		if (key != null) {
			if (key is RSA) {
				rsa = (RSA)key;
			}
			else
				throw new InvalidCastException ();
		}
		// here null is accepted!
	}
}

}
