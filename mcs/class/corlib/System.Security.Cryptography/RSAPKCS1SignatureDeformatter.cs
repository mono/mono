//
// RSAPKCS1SignatureDeformatter.cs - Handles PKCS#1 v.1.5 signature decryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography { 

public class RSAPKCS1SignatureDeformatter : AsymmetricSignatureDeformatter {

	protected RSA rsa;
	protected HashAlgorithm hash;

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

	[MonoTODO()]
	public override bool VerifySignature (byte[] rgbHash, byte[] rgbSignature) 
	{
		if ((rsa == null) || (hash == null))
			throw new CryptographicUnexpectedOperationException ();
		if ((rgbHash == null) || (rgbSignature == null))
			throw new ArgumentNullException ();
		// TODO
		return false;
	}

	[MonoTODO()]
	public override bool VerifySignature (HashAlgorithm hash, byte[] rgbSignature) 
	{
		if ((hash == null) || (rgbSignature == null))
			throw new ArgumentNullException ();
		if ((rsa == null) || (hash == null))
			throw new CryptographicUnexpectedOperationException ();
		// TODO
		return false;
	}
}

}
