//
// RSACryptoServiceProvider.cs: Handles an RSA implementations.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;

namespace System.Security.Cryptography {

public class RSACryptoServiceProvider : RSA {
	private bool privateKeyExportable = false; // secure by default
	private bool m_disposed = false;

	public RSACryptoServiceProvider ()
	{
		// if no default, create a 1024 bits keypair
		GenerateKeyPair (1024);
	}

	// FIXME: We currently dont link with MS CAPI. Anyway this makes
	// only sense in Windows - what do we do elsewhere ?
	public RSACryptoServiceProvider (CspParameters parameters) 
	{
		throw new NotSupportedException ("CspParameters not supported");
	}

	// Microsoft RSA CSP can do between 512 and 16384 bits keypair
	public RSACryptoServiceProvider (int dwKeySize) 
	{
		GenerateKeyPair (dwKeySize);
	}

	// FIXME: We currently dont link with MS CAPI. Anyway this makes
	// only sense in Windows - what do we do elsewhere ?
	// Note: Microsoft RSA CSP can do between 512 and 16384 bits keypair
	public RSACryptoServiceProvider (int dwKeySize, CspParameters parameters) 
	{
		throw new NotSupportedException ("CspParameters not supported");
	}

	private void GenerateKeyPair (int dwKeySize) 
	{
		LegalKeySizesValue = new KeySizes [1];
		KeySizes ks = new KeySizes (384, 16384, 8);
		LegalKeySizes[0] = ks;
		KeySizeValue = 1024; // default
		// TODO
	}

	// Zeroize private key
	~RSACryptoServiceProvider() 
	{
		Dispose (false);
	}

	public override string KeyExchangeAlgorithm {
		get { return "RSA-PKCS1-KeyEx"; }
	}

	public override int KeySize {
		get { return 0; }
	}

	public bool PersistKeyInCsp {
		get { return false;  }
		set { ; }
	}

	public override string SignatureAlgorithm {
		get { return "http://www.w3.org/2000/09/xmldsig#rsa-sha1"; }
	}

	public byte[] Decrypt (byte[] rgb, bool fOAEP) 
	{
		if ((fOAEP) && (rgb.Length > (KeySize >> 3) - 11))
			throw new CryptographicException ();
		// choose between OAEP or PKCS#1 v.1.5 padding
		return null;
	}

	// NOTE: Unlike MS we need this method
	// LAMESPEC: Not available from MS .NET framework but MS don't tell
	// why! DON'T USE IT UNLESS YOU KNOW WHAT YOU ARE DOING!!! You should
	// only encrypt/decrypt session (secret) key using asymmetric keys. 
	// Using this method to decrypt data IS dangerous (and very slow).
	[MonoTODO()]
	public override byte[] DecryptValue (byte[] rgb) 
	{
		return null;
	}

	[MonoTODO()]
	public byte[] Encrypt (byte[] rgb, bool fOAEP) 
	{
		if ((fOAEP) && (rgb.Length > (KeySize >> 3) - 11))
			throw new CryptographicException ();
		// choose between OAEP or PKCS#1 v.1.5 padding
		return null;
	}

	// NOTE: Unlike MS we need this method
	// LAMESPEC: Not available from MS .NET framework but MS don't tell
	// why! DON'T USE IT UNLESS YOU KNOW WHAT YOU ARE DOING!!! You should
	// only encrypt/decrypt session (secret) key using asymmetric keys. 
	// Using this method to encrypt data IS dangerous (and very slow).
	[MonoTODO()]
	public override byte[] EncryptValue (byte[] rgb) 
	{
		return null;
	}

	[MonoTODO()]
	public override RSAParameters ExportParameters (bool includePrivateParameters) 
	{
		if ((includePrivateParameters) && (!privateKeyExportable))
			throw new CryptographicException ("cannot export private key");
		RSAParameters p = new RSAParameters();
		return p;
	}

	[MonoTODO()]
	public override void ImportParameters (RSAParameters parameters) 
	{
		// if missing parameters
		// throw new CryptographicException ();
	}

	private HashAlgorithm GetHash (object halg) 
	{
		if (halg == null)
			throw new ArgumentNullException ();

		HashAlgorithm hash = null;
		if (halg is String)
			hash = HashAlgorithm.Create ((String)halg);
		else if (halg is HashAlgorithm)
			hash = (HashAlgorithm) halg;
		else if (halg is Type)
			hash = (HashAlgorithm) Activator.CreateInstance ((Type)halg);
		else
			throw new ArgumentException ();

		return hash;
	}

	// better to send OID ?
	private string GetHashName (HashAlgorithm hash) 
	{
		string str = null;
		if (hash is SHA1)
			str = "SHA1";
		else if (hash is MD5)
			str = "MD5";
		return str;
	}

	public byte[] SignData (byte[] buffer, object halg) 
	{
		return SignData (buffer, 0, buffer.Length, halg);
	}

	public byte[] SignData (Stream inputStream, object halg) 
	{
		HashAlgorithm hash = GetHash (halg);
		byte[] toBeSigned = hash.ComputeHash (inputStream);

		string str = GetHashName (hash);
		return SignHash (toBeSigned, str);
	}

	public byte[] SignData (byte[] buffer, int offset, int count, object halg) 
	{
		HashAlgorithm hash = GetHash (halg);
		byte[] toBeSigned = hash.ComputeHash (buffer, offset, count);
		string str = GetHashName (hash);
		return SignHash (toBeSigned, str);
	}

	[MonoTODO()]
	public byte[] SignHash (byte[] rgbHash, string str) 
	{
		if (rgbHash == null)
			throw new ArgumentNullException ();

		if ((str == null) || (str == "SHA1")) {
			if (rgbHash.Length != 20)
				throw new CryptographicException ("wrong hash size for SHA1");
		}
		else if (str == "MD5") {
			if (rgbHash.Length != 16)
				throw new CryptographicException ("wrong hash size for MD5");
		}
		else
			throw new NotSupportedException (str + " is an unsupported hash algorithm for RSA signing");

		return null;
	}

	public bool VerifyData (byte[] buffer, object halg, byte[] signature) 
	{
		HashAlgorithm hash = GetHash (halg);
		byte[] toBeVerified = hash.ComputeHash (buffer);
		string str = GetHashName (hash);
		return VerifyHash (toBeVerified, str, signature);
	}

	[MonoTODO()]
	public bool VerifyHash (byte[] rgbHash, string str, byte[] rgbSignature) 
	{
		if ((rgbHash == null) || (rgbSignature == null))
			throw new ArgumentNullException ();
		// TODO
		return false;
	}

	[MonoTODO()]
	protected override void Dispose (bool disposing) 
	{
		if (!m_disposed) {
			// TODO: always zeroize private key
			if(disposing) {
				// TODO: Dispose managed resources
			}
         
			// TODO: Dispose unmanaged resources
		}
		// call base class 
		// no need as they all are abstract before us
		m_disposed = true;
	}
}

}
