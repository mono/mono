//
// RSAPKCS1KeyExchangeFormatter.cs: Handles PKCS#1 v.1.5 keyex encryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace System.Security.Cryptography
{

// LAMESPEC: There seems no way to select a hash algorithm. The default 
// algorithm, is SHA1 because the class use the PKCS1MaskGenerationMethod -
// which default to SHA1.
public class RSAPKCS1KeyExchangeFormatter: AsymmetricKeyExchangeFormatter
{
	protected RSA rsa;
	protected RandomNumberGenerator random;

	public RSAPKCS1KeyExchangeFormatter ()
	{
	}

	public RSAPKCS1KeyExchangeFormatter (AsymmetricAlgorithm key)
	{
		SetKey (key);
	}

	public RandomNumberGenerator Rng 
	{
		get { return random; }
		set { random = value; }
	}

	public override string Parameters 
	{
		get { return "<enc:KeyEncryptionMethod enc:Algorithm=\"http://www.microsoft.com/xml/security/algorithm/PKCS1-v1.5-KeyEx\" xmlns:enc=\"http://www.microsoft.com/xml/security/encryption/v1.0\" />"; }
	}

	// I2OSP converts a nonnegative integer to an octet string of a specified length.
	// in this case xLen is always 4 so we simplify the function 
	protected byte[] I2OSP (int x)
	{
		byte[] array = BitConverter.GetBytes (x);
		Array.Reverse (array); // big-little endian issues
		return array;
	}

	[MonoTODO("rsa.EncryptValue throws UnsupportedException on MS framework")]
	public override byte[] CreateKeyExchange (byte[] rgbData)
	{
		int k = rsa.KeySize; // e.g. 128 for 1024 bits keys
		int mLen = rgbData.Length;

		// 1. Length checking: If mLen > k – 11, output “message too long” and stop.
		if (mLen > k - 11)
			throw new CryptographicException ("message too long");

		// 2. EME-PKCS1-v1_5 encoding:
		//	a. Generate an octet string PS of length k – mLen – 3 consisting of 
		//		pseudo-randomly generated nonzero octets. The length of PS will be 
		//		at least eight octets.
		int PSLength = k - mLen - 3;
		if (PSLength < 8)
			throw new CryptographicException ("PS too short");
		byte[] PS = new byte [PSLength];
		if (random == null)
			random = RandomNumberGenerator.Create ();  // create default
		random.GetNonZeroBytes (PS);

		// b. Concatenate PS, the message M, and other padding to form an encoded
		//		message EM of length k octets as
		//		EM = 0x00 || 0x02 || PS || 0x00 || M 
		byte[] EM = new byte [3 + PSLength + mLen];
		EM [0] = 0x00;
		EM [1] = 0x02;
		Array.Copy (PS, 0, EM, 2, PSLength);
		EM [2 + PSLength] = 0x00;
		Array.Copy (rgbData, 0, EM, 3 + PSLength, mLen);

		//	3. RSA encryption:
		// a. Convert the encoded message EM to an integer message representative 
		//		m (see Section 4.2):
		//		m = OS2IP (EM)
		byte[] m = EM;

		//	b. Apply the RSAEP encryption primitive (Section 5.1.1) to the RSA public
		//		key (n, e) and the message representative m to produce an integer
		//		ciphertext representative c:
		//		c = RSAEP ((n, e), m)
		byte[] c = rsa.EncryptValue (m);

		//	c. Convert the ciphertext representative c to a ciphertext C of length k
		//		octets (see Section 4.1):
		//	   C = I2OSP (c, k) 
		byte[] C = c;

		// 4. Output the ciphertext C.
		return C;

		// L is always empty in PKCS#1 v.2.1 - so SHA1(L) is constant
		//byte[] sha1L = { 0xda, 0x39, 0xa3, 0xee, 0x5e, 0x6b, 0x4b, 0x0d, 0x32, 0x55, 0xbf, 0xef, 0x95, 0x60, 0x18, 0x90, 0xaf, 0xd8, 0x07, 0x09 };
	}

	public override byte[] CreateKeyExchange (byte[] rgbData, Type symAlgType)
	{
		// documentation says that symAlgType is not used !?!
		// FIXME: must be the same as previous method ?
		return CreateKeyExchange (rgbData);
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
