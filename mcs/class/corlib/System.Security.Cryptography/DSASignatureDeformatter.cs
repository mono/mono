//
// System.Security.Cryptography DSASignatureDeformatter.cs
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography {

/// <summary>
/// DSA Signature Deformatter
/// </summary>
public class DSASignatureDeformatter : AsymmetricSignatureDeformatter {
	
	private DSA dsa;
	
	public DSASignatureDeformatter () {}

	public DSASignatureDeformatter (AsymmetricAlgorithm key)
	{
		SetKey (key);
	}

	public override void SetHashAlgorithm (string strName)
	{
		if (strName == null)
			throw new ArgumentNullException ("strName");

		try {
			// just to test, we don't need the object
			SHA1 hash = SHA1.Create (strName);
		}
		catch {
			throw new CryptographicUnexpectedOperationException ("DSA requires SHA1");
		}
	}

	public override void SetKey (AsymmetricAlgorithm key)
	{
		if (key != null) {
			// this will throw a InvalidCastException if this isn't
			// a DSA keypair
			dsa = (DSA) key;
		}
		// here null is accepted!
	}

	public override bool VerifySignature (byte[] rgbHash, byte[] rgbSignature)
	{
		if (dsa == null)
			throw new CryptographicUnexpectedOperationException ("missing key");
		return dsa.VerifySignature (rgbHash, rgbSignature);
	}
	
} // DSASignatureDeformatter
	
} // System.Security.Cryptography
