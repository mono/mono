//
// HMACSHA1.cs: Handles HMAC with SHA-1
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;

namespace System.Security.Cryptography {

// References:
// a.	FIPS PUB 198: The Keyed-Hash Message Authentication Code (HMAC), 2002 March.
//	http://csrc.nist.gov/publications/fips/fips198/fips-198a.pdf
// b.	Internet RFC 2104, HMAC, Keyed-Hashing for Message Authentication
//	(include C source for HMAC-MD5)
//	http://www.ietf.org/rfc/rfc2104.txt
// c.	IETF RFC2202: Test Cases for HMAC-MD5 and HMAC-SHA-1
//	(include C source for HMAC-MD5 and HAMAC-SHA1)
//	http://www.ietf.org/rfc/rfc2202.txt
// d.	ANSI X9.71, Keyed Hash Message Authentication Code.
//	not free :-(
//	http://webstore.ansi.org/ansidocstore/product.asp?sku=ANSI+X9%2E71%2D2000

// Generic HMAC mechanisms - most of HMAC work is done in here.
// It should work with any hash function e.g. MD5 for HMACMD5 (RFC2104)
internal class HMACAlgorithm {
	private byte[] key;
	private byte[] hash;
	private HashAlgorithm algo;
	private string hashName;
	private CryptoStream stream;

	public HMACAlgorithm (string algoName) 
	{
		CreateHash (algoName);
	}

	~HMACAlgorithm () 
	{
		Dispose ();
	}

	private void CreateHash (string algoName) 
	{
		algo = HashAlgorithm.Create (algoName);
		hashName = algoName;
	}

	public void Dispose () 
	{
		ZeroizeKey ();
	}

	public HashAlgorithm Algo {
		get { return algo; }
	}

	public string HashName {
		get { return hashName; }
		set { 
			// only if its not too late for a change
			if (stream == null)
				CreateHash (value);
		}
	}

	public byte[] HashValue {
		get { return hash; }
	}

	public byte[] Key {
		get { return key; }
		set {
			if ((value != null) && (value.Length > 64))
				key = algo.ComputeHash (value);
			else
				key = (byte[]) value.Clone();
		}
	}

	public void Initialize () 
	{
		hash = null;
	}

	private byte[] KeySetup (byte[] key, byte padding) 
	{
		byte[] buf = new byte [64];

		for (int i = 0; i < key.Length; ++i)
			buf [i] = (byte) ((byte) key [i] ^ padding);

		for (int i = key.Length; i < 64; ++i)
			buf [i] = padding;
		
		return buf;
	}

	public void Core (byte[] rgb, int ib, int cb) 
	{
		if (stream == null) {
			byte[] buf = KeySetup (key, 0x36);
			algo.Initialize ();
			stream = new CryptoStream (Stream.Null, algo, CryptoStreamMode.Write);
			stream.Write (buf, 0, buf.Length);
		}
		stream.Write (rgb, ib, cb);
	}

	public byte[] Final () 
	{
		stream.Close ();
		stream = null;
		byte[] intermediate = algo.Hash;
		byte[] buf = KeySetup (key, 0x5C);

		algo.Initialize ();
		stream = new CryptoStream (Stream.Null, algo, CryptoStreamMode.Write);
		stream.Write (buf, 0, buf.Length);
		stream.Write (intermediate, 0, intermediate.Length);
		stream.Close ();
		stream = null;

		hash = algo.Hash;
		algo.Clear ();
		return hash;
	}

	// Note: this key is different (well most of the time) from the key
	// used in KeyHashAlgorithm (this one may be padded or hashed). So
	// it need to be zeroized independently.
	public void ZeroizeKey () 
	{
		if (key != null)
			Array.Clear (key, 0, key.Length);
	}
}

public class HMACSHA1: KeyedHashAlgorithm {
	private HMACAlgorithm hmac;

	public HMACSHA1 () : base ()
	{
		hmac = new HMACAlgorithm ("SHA1");
		HashSizeValue = 160;
		Key = KeyBuilder.Key (8);
	}

	public HMACSHA1 (byte[] rgbKey) 
	{
		hmac = new HMACAlgorithm ("SHA1");
		HashSizeValue = 160;
		hmac.Key = rgbKey;
	}

	~HMACSHA1 () 
	{
		Dispose (false);
	}

	public override byte[] Key {
		get { return base.Key; }
		set { 
			hmac.Key = value; 
			base.Key = value;
		}
	} 

	public string HashName {
		get { return hmac.HashName; }
		set { hmac.HashName = value; }
	}

	protected override void Dispose (bool disposing) 
	{
		if (hmac != null)
			hmac.Dispose();
		base.Dispose (disposing);
	}

	public override void Initialize ()
	{
		State = 0;
		hmac.Initialize ();
	}

        protected override void HashCore (byte[] rgb, int ib, int cb)
	{
		if (State == 0) {
			// let us throw an exception if hash name is invalid
			// for HMACSHA1 (obviously this can't be done by the 
			// generic HMAC class) 
			if (! (hmac.Algo is SHA1))
				throw new InvalidCastException ();
		}
		State = 1;
		hmac.Core (rgb, ib, cb);
	}

	protected override byte[] HashFinal ()
	{
		State = 0;
		return hmac.Final ();
	}
}

}