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

	protected RSA rsa;
	protected HashAlgorithm hash;

	public RSAPKCS1SignatureFormatter () 
	{
		rsa = null;
	}

	public RSAPKCS1SignatureFormatter (AsymmetricAlgorithm key) 
	{
		SetKey (key);
	}

	[MonoTODO()]
	public override byte[] CreateSignature (byte[] rgbHash) 
	{
		if ((rsa == null) || (hash == null))
			throw new CryptographicUnexpectedOperationException ();
		if (rgbHash == null)
			throw new ArgumentNullException ();
		// TODO
		return null;
	}

	[MonoTODO()]
	public override byte[] CreateSignature (HashAlgorithm hash) 
	{
		if (hash == null)
			throw new ArgumentNullException ();
		if (rsa == null)
			throw new CryptographicUnexpectedOperationException ();
		// TODO
		return null;
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
