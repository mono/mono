//
// RSAManaged.cs - Implements the RSA algorithm.
//
// Authors:
//	Sebastien Pouliot (sebastien@ximian.com)
//	Ben Maurer (bmaurer@users.sf.net)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Portions (C) 2003 Ben Maurer
// (C) 2004 Novell (http://www.novell.com)
//
// Key generation translated from Bouncy Castle JCE (http://www.bouncycastle.org/)
// See bouncycastle.txt for license.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Security.Cryptography;

using Mono.Math;

// Big chunks of code are coming from the original RSACryptoServiceProvider class.
// The class was refactored to :
// a.	ease integration of new hash algorithm (like MD2, RIPEMD160, ...);
// b.	provide better support for the coming SSL implementation (requires 
//	EncryptValue/DecryptValue) with, or without, Mono runtime/corlib;
// c.	provide an alternative RSA implementation for all Windows (like using 
//	OAEP without Windows XP).

namespace Mono.Security.Cryptography {

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class RSAManaged : RSA {

		private const int defaultKeySize = 1024;

		private bool isCRTpossible = false;
		private bool keypairGenerated = false;
		private bool m_disposed = false;

		private BigInteger d;
		private BigInteger p;
		private BigInteger q;
		private BigInteger dp;
		private BigInteger dq;
		private BigInteger qInv;
		private BigInteger n;		// modulus
		private BigInteger e;

		public RSAManaged () : this (defaultKeySize)
		{
		}

		public RSAManaged (int keySize) 
		{
			KeySizeValue = keySize;
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (384, 16384, 8);
		}

		~RSAManaged () 
		{
			// Zeroize private key
			Dispose (false);
		}

		private void GenerateKeyPair () 
		{
			// p and q values should have a length of half the strength in bits
			int pbitlength = ((KeySize + 1) >> 1);
			int qbitlength = (KeySize - pbitlength);
			const uint uint_e = 17;
			e = uint_e; // fixed
	
			// generate p, prime and (p-1) relatively prime to e
			for (;;) {
				p = BigInteger.GeneratePseudoPrime (pbitlength);
				if (p % uint_e != 1)
					break;
			}
			// generate a modulus of the required length
			for (;;) {
				// generate q, prime and (q-1) relatively prime to e,
				// and not equal to p
				for (;;) {
					q = BigInteger.GeneratePseudoPrime (qbitlength);
					if ((q % uint_e != 1) && (p != q))
						break;
				}
	
				// calculate the modulus
				n = p * q;
				if (n.BitCount () == KeySize)
					break;
	
				// if we get here our primes aren't big enough, make the largest
				// of the two p and try again
				if (p < q)
					p = q;
			}
	
			BigInteger pSub1 = (p - 1);
			BigInteger qSub1 = (q - 1);
			BigInteger phi = pSub1 * qSub1;
	
			// calculate the private exponent
			d = e.ModInverse (phi);
	
			// calculate the CRT factors
			dp = d % pSub1;
			dq = d % qSub1;
			qInv = q.ModInverse (p);
	
			keypairGenerated = true;
			isCRTpossible = true;

			if (KeyGenerated != null)
				KeyGenerated (this, null);
		}
		
		// overrides from RSA class

		public override int KeySize {
			get { 
				// in case keypair hasn't been (yet) generated
				if (keypairGenerated) {
					int ks = n.BitCount ();
					if ((ks & 7) != 0)
						ks = ks + (8 - (ks & 7));
					return ks;
				}
				else
					return base.KeySize;
			}
		}
		public override string KeyExchangeAlgorithm {
			get { return "RSA-PKCS1-KeyEx"; }
		}

		// note: this property will exist in RSACryptoServiceProvider in
		// version 1.2 of the framework
		public bool PublicOnly {
			get { return ((d == null) || (n == null)); }
		}

		public override string SignatureAlgorithm {
			get { return "http://www.w3.org/2000/09/xmldsig#rsa-sha1"; }
		}

		public override byte[] DecryptValue (byte[] rgb) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("private key");

			// decrypt operation is used for signature
			if (!keypairGenerated)
				GenerateKeyPair ();

			BigInteger input = new BigInteger (rgb);
			BigInteger output;
			// decrypt (which uses the private key) can be 
			// optimized by using CRT (Chinese Remainder Theorem)
			if (isCRTpossible) {
				// m1 = c^dp mod p
				BigInteger m1 = input.ModPow (dp, p);
				// m2 = c^dq mod q
				BigInteger m2 = input.ModPow (dq, q);
				BigInteger h;
				if (m2 > m1) {
					// thanks to benm!
					h = p - ((m2 - m1) * qInv % p);
					output = m2 + q * h;
				}
				else {
					// h = (m1 - m2) * qInv mod p
					h = (m1 - m2) * qInv % p;
					// m = m2 + q * h;
					output = m2 + q * h;
				}
			}
			else {
				// m = c^d mod n
				output = input.ModPow (d, n);
			}
			byte[] result = output.GetBytes ();
			// zeroize value
			input.Clear ();	
			output.Clear ();
			return result;
		}

		public override byte[] EncryptValue (byte[] rgb) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("public key");

			if (!keypairGenerated)
				GenerateKeyPair ();

			BigInteger input = new BigInteger (rgb);
			BigInteger output = input.ModPow (e, n);
			byte[] result = output.GetBytes ();
			// zeroize value
			input.Clear ();	
			output.Clear ();
			return result;
		}

		public override RSAParameters ExportParameters (bool includePrivateParameters) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("");

			if (!keypairGenerated)
				GenerateKeyPair ();
	
			RSAParameters param = new RSAParameters ();
			param.Exponent = e.GetBytes ();
			param.Modulus = n.GetBytes ();
			if (includePrivateParameters) {
				// some parameters are required for exporting the private key
				if ((d == null) || (p == null) || (q == null))
					throw new CryptographicException ("Missing private key");
				param.D = d.GetBytes ();
				// hack for bugzilla #57941 where D wasn't provided
				if (param.D.Length != param.Modulus.Length) {
					byte[] normalizedD = new byte [param.Modulus.Length];
					Buffer.BlockCopy (param.D, 0, normalizedD, (normalizedD.Length - param.D.Length), param.D.Length);
					param.D = normalizedD;
				}
				param.P = p.GetBytes ();
				param.Q = q.GetBytes ();
				// but CRT parameters are optionals
				if ((dp != null) && (dq != null) && (qInv != null)) {
					// and we include them only if we have them all
					param.DP = dp.GetBytes ();
					param.DQ = dq.GetBytes ();
					param.InverseQ = qInv.GetBytes ();
				}
			}
			return param;
		}

		public override void ImportParameters (RSAParameters parameters) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("");

			// if missing "mandatory" parameters
			if (parameters.Exponent == null) 
				throw new CryptographicException ("Missing Exponent");
			if (parameters.Modulus == null)
				throw new CryptographicException ("Missing Modulus");
	
			e = new BigInteger (parameters.Exponent);
			n = new BigInteger (parameters.Modulus);
			// only if the private key is present
			if (parameters.D != null)
				d = new BigInteger (parameters.D);
			if (parameters.DP != null)
				dp = new BigInteger (parameters.DP);
			if (parameters.DQ != null)
				dq = new BigInteger (parameters.DQ);
			if (parameters.InverseQ != null)
				qInv = new BigInteger (parameters.InverseQ);
			if (parameters.P != null)
				p = new BigInteger (parameters.P);
			if (parameters.Q != null)
				q = new BigInteger (parameters.Q);
			
			// we now have a keypair
			keypairGenerated = true;
			isCRTpossible = ((p != null) && (q != null) && (dp != null) && (dq != null) && (qInv != null));
		}

		protected override void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				// Always zeroize private key
				if (d != null) {
					d.Clear (); 
					d = null;
				}
				if (p != null) {
					p.Clear (); 
					p = null;
				}
				if (q != null) {
					q.Clear (); 
					q = null;
				}
				if (dp != null) {
					dp.Clear (); 
					dp = null;
				}
				if (dq != null) {
					dq.Clear (); 
					dq = null;
				}
				if (qInv != null) {
					qInv.Clear (); 
					qInv = null;
				}

				if (disposing) {
					// clear public key
					if (e != null) {
						e.Clear (); 
						e = null;
					}
					if (n != null) {
						n.Clear (); 
						n = null;
					}
				}
			}
			// call base class 
			// no need as they all are abstract before us
			m_disposed = true;
		}

		public delegate void KeyGeneratedEventHandler (object sender, EventArgs e);

		public event KeyGeneratedEventHandler KeyGenerated;
	}
}
