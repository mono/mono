//
// RSAPKCS1SignatureDeformatter.cs - Handles PKCS#1 v.1.5 signature decryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography { 

public class RSAPKCS1SignatureDeformatter : AsymmetricSignatureDeformatter {

	private RSA rsa;
	private string hashName;

	public RSAPKCS1SignatureDeformatter () 
	{
		rsa = null;
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
		if (key != null)
			rsa = (RSA)key;
		// here null is accepted with an ArgumentNullException!
	}

	public override bool VerifySignature (byte[] rgbHash, byte[] rgbSignature) 
	{
		if ((rsa == null) || (hashName == null))
			throw new CryptographicUnexpectedOperationException ();
		if ((rgbHash == null) || (rgbSignature == null))
			throw new ArgumentNullException ();

		string oid = CryptoConfig.MapNameToOID (hashName);
		return PKCS1.Verify_v15 (rsa, oid, rgbHash, rgbSignature);
	}
}

}
