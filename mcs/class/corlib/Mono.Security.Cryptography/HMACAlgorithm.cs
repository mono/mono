//
// HMACAlgorithm.cs: Handles HMAC with any hash algorithm
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

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
		private BlockProcessor block;
	
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
			block = new BlockProcessor (algo, 8);
		}
	
		public void Dispose () 
		{
			if (key != null)
				Array.Clear (key, 0, key.Length);
		}
	
		public HashAlgorithm Algo {
			get { return algo; }
		}
	
		public string HashName {
			get { return hashName; }
			set { CreateHash (value); }
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
			block.Initialize ();
			byte[] buf = KeySetup (key, 0x36);
			algo.Initialize ();
			block.Core (buf);
			// zeroize key
			Array.Clear (buf, 0, buf.Length);
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
			block.Core (rgb, ib, cb);
		}
	
		public byte[] Final () 
		{
			block.Final ();
			byte[] intermediate = algo.Hash;
	
			byte[] buf = KeySetup (key, 0x5C);
			algo.Initialize ();
			algo.TransformBlock (buf, 0, buf.Length, buf, 0);
			algo.TransformFinalBlock (intermediate, 0, intermediate.Length);
			hash = algo.Hash;
			algo.Clear ();
			// zeroize sensitive data
			Array.Clear (buf, 0, buf.Length);	
			Array.Clear (intermediate, 0, intermediate.Length);
			return hash;
		}
	}
}