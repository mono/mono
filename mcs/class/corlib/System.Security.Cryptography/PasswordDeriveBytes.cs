//
// PasswordDeriveBytes.cs: Handles PKCS#5 key derivation using password
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Text;

namespace System.Security.Cryptography {

// Reference:
// a.	PKCS #5 - Password-Based Cryptography Standard 
//	http://www.rsasecurity.com/rsalabs/pkcs/pkcs-5/index.html
// b.	IETF RFC2898: PKCS #5: Password-Based Cryptography Specification Version 2.0
//	http://www.rfc-editor.org/rfc/rfc2898.txt

public class PasswordDeriveBytes : DeriveBytes {

	protected string HashNameValue;
	protected string PasswordValue;
	protected byte[] SaltValue;
	protected int IterationsValue;

	private HashAlgorithm hash;
	private int state;
	private byte[] output;
	private int posOut;

	public PasswordDeriveBytes (string strPassword, byte[] rgbSalt) 
	{
		Prepare (strPassword, rgbSalt, "SHA1", 1);
	}

	[MonoTODO("Integrate with CAPI on Windows. Linux?")]
	public PasswordDeriveBytes (string strPassword, byte[] rgbSalt, CspParameters cspParams) 
	{
		throw new NotSupportedException ("CspParameters not supported");
	}

	public PasswordDeriveBytes (string strPassword, byte[] rgbSalt, string strHashName, int iterations) 
	{
		Prepare (strPassword, rgbSalt, strHashName, iterations);
	}

	[MonoTODO("Integrate with CAPI on Windows. Linux?")]
	public PasswordDeriveBytes (string strPassword, byte[] rgbSalt, string strHashName, int iterations, CspParameters cspParams) 
	{
		throw new NotSupportedException ("CspParameters not supported");
	}

	[MonoTODO("Must release unmanaged resources on Windows (CAPI). Linux?")]
	~PasswordDeriveBytes () 
	{
		// zeroize buffer
		if (output != null) {
			Array.Clear (output, 0, output.Length);
			output = null;
		}
		// FIXME: zeroize password - not easy as all string function 
		// returns a string so we never have direct access to it's
		// content - the password :-(
		PasswordValue = null;
	}

	private void Prepare (string strPassword, byte[] rgbSalt, string strHashName, int iterations) 
	{
		HashNameValue = strHashName;
		PasswordValue = strPassword;
		SaltValue = rgbSalt;
		IterationsValue = iterations;
		state = 0;
	}

	public string HashName {
		get { return HashNameValue; } 
		set {
			if (state != 0)
				throw new CryptographicException ();
			HashNameValue = value;
		}
	}

	public int IterationCount {
		get { return IterationsValue; }
		set {
			if (state != 0)
				throw new CryptographicException ();
			IterationsValue = value;
		}
	}

	// FIXME ??? Comments are here to simulate the strange behaviour that
	// happens when we want to assign null to a salt ??? Do we want this
	// "feature" in Mono ? Vote "yes" for compatibility or "no" for
	// functionality!
	public byte[] Salt {
		get { return (byte[]) SaltValue.Clone ();  }
		set {
			if (state != 0)
				throw new CryptographicException ();
//			if (value != null)
				SaltValue = (byte[]) value.Clone ();
//			else
//				value = null;
		}
	}

	// for compatibility with Microsoft CryptoAPI
	[MonoTODO("Integrate with CAPI on Windows. Linux?")]
	public byte[] CryptDeriveKey (string algname, string alghashname, int keySize, byte[] rgbIV) 
	{
		if (keySize > 128)
			throw new CryptographicException ("Key Size can't be greater than 128 bits");
		throw new NotSupportedException ("CspParameters not supported");
	}

	// note: Key is returned - we can't zeroize it ourselve :-(
	[MonoTODO("Doesn't generate keys longer than HashSize")]
	public override byte[] GetBytes (int cb) 
	{
		// must be first (before NotSupportedException) as the Hash
		// object is created in Reset()
		if (state == 0) {
			state = 1;
			// it's now impossible to change the HashName, Salt
			// and IterationCount
			Reset ();
		}

		// FIXME: This version can generate a key up to HashSize length
		// This is normal for PKCS#5 but MS implementation allows longer
		// keys (note that, in this case, longer keys aren't more secure!)
		if (cb > hash.HashSize)
			throw new NotSupportedException ("cb > HashSize");

		hash.Initialize ();
		// the initial hash (in reset) + at least one iteration
		int iter = Math.Max (1, IterationsValue - 1);
		// generate new key material
		for (int i = 0; i < iter; i++)
			output = hash.ComputeHash (output);
		byte[] result = new byte [cb];
		Array.Copy (output, 0, result, 0, cb);
		return result;
	}

	public override void Reset () 
	{
		// note: Reset doesn't change state
		byte[] password = Encoding.UTF8.GetBytes (PasswordValue);
		int len = password.Length;
		if (SaltValue != null)
			len += SaltValue.Length;
		byte[] input = new byte [len];

		Array.Copy (password, 0, input, 0, password.Length);
		// zeroize temporary password storage
		Array.Clear (password, 0, password.Length);
		if (SaltValue != null)
			Array.Copy (SaltValue, 0, input, password.Length, SaltValue.Length);

		hash = HashAlgorithm.Create (HashNameValue);
		output = hash.ComputeHash (input);
		// we start serving key from the first byte
		posOut = 0;
	}
} 
	
} 