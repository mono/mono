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

using Mono.Security.Cryptography;

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
	
	public class HMACSHA1: KeyedHashAlgorithm {
		private HMACAlgorithm hmac;
		private bool m_disposed;
	
		public HMACSHA1 () : this (KeyBuilder.Key (8)) {}
	
		public HMACSHA1 (byte[] rgbKey) : base ()
		{
			hmac = new HMACAlgorithm ("SHA1");
			HashSizeValue = 160;
			Key = rgbKey;
			m_disposed = false;
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
			set { 
				// only if its not too late for a change
				if (State == 0)
					hmac.HashName = value; 
			}
		}
	
		protected override void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				if (hmac != null)
					hmac.Dispose();
				base.Dispose (disposing);
				m_disposed = true;
			}
		}
	
		public override void Initialize ()
		{
			if (m_disposed)
				throw new ObjectDisposedException ("HMACSHA1");
			// let us throw an exception if hash name is invalid
			// for HMACSHA1 (obviously this can't be done by the 
			// generic HMAC class) 
			if (! (hmac.Algo is SHA1))
				throw new InvalidCastException ();
			State = 0;
			hmac.Initialize ();
		}
	
	        protected override void HashCore (byte[] rgb, int ib, int cb)
		{
			if (m_disposed)
				throw new ObjectDisposedException ("HMACSHA1");
			if (State == 0) {
				Initialize ();
				State = 1;
			}
			hmac.Core (rgb, ib, cb);
		}
	
		protected override byte[] HashFinal ()
		{
			if (m_disposed)
				throw new ObjectDisposedException ("HMACSHA1");
			State = 0;
			return hmac.Final ();
		}
	}
}