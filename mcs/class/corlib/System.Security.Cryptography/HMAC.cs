//
// HMAC.cs: Generic HMAC inplementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	// Mostly copied from (internal) Mono.Security.Cryptography.HMACAlgorithm

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

	public abstract class HMAC : KeyedHashAlgorithm {

		private bool _disposed;
		private string _hashName;
		private HashAlgorithm _algo;
		private BlockProcessor _block;
		protected Int32 BlockSizeValue; 

		// constructors

		protected HMAC () 
		{
			_disposed = false;
		}

		// properties

		public string HashName {
			get { return _hashName; }
			set { 
				_hashName = value; 
				_algo = HashAlgorithm.Create (_hashName);
				_block = new BlockProcessor (_algo, 8);
			}
		}

		public override byte[] Key { 
			get { return (byte[]) base.Key.Clone (); }
			set { 
				if ((value != null) && (value.Length > 64))
					base.Key = _algo.ComputeHash (value);
				else
					base.Key = (byte[]) value.Clone();
			}
		}

		// methods

		private byte[] KeySetup (byte[] key, byte padding) 
		{
			byte[] buf = new byte [64];
	
			for (int i = 0; i < key.Length; ++i)
				buf [i] = (byte) ((byte) key [i] ^ padding);
	
			for (int i = key.Length; i < 64; ++i)
				buf [i] = padding;
			
			return buf;
		}

		protected override void Dispose (bool disposing) 
		{
			if (!_disposed) {
				base.Dispose (disposing);
			}
		}

		protected override void HashCore (byte[] rgb, int ib, int cb) 
		{
			if (_disposed)
				throw new ObjectDisposedException ("HMACSHA1");

			if (State == 0) {
				Initialize ();
				State = 1;
			}
			_block.Core (rgb, ib, cb);
		}

		protected override byte[] HashFinal () 
		{
			if (_disposed)
				throw new ObjectDisposedException ("HMAC");
			State = 0;

			_block.Final ();
			byte[] intermediate = _algo.Hash;
	
			byte[] buf = KeySetup (Key, 0x5C);
			_algo.Initialize ();
			_algo.TransformBlock (buf, 0, buf.Length, buf, 0);
			_algo.TransformFinalBlock (intermediate, 0, intermediate.Length);
			byte[] hash = _algo.Hash;
			_algo.Clear ();
			// zeroize sensitive data
			Array.Clear (buf, 0, buf.Length);	
			Array.Clear (intermediate, 0, intermediate.Length);
			return hash;
		}

		public override void Initialize () 
		{
			if (_disposed)
				throw new ObjectDisposedException ("HMAC");

			State = 0;
			_block.Initialize ();
			byte[] buf = KeySetup (Key, 0x36);
			_algo.Initialize ();
			_block.Core (buf);
			// zeroize key
			Array.Clear (buf, 0, buf.Length);
		}

		// static methods

		public static new HMAC Create () 
		{
			return Create ("System.Security.Cryptography.HMAC");
		}

		public static new HMAC Create (string algName) 
		{
			return (HMAC) CryptoConfig.CreateFromName (algName);
		}
	}
}

#endif
