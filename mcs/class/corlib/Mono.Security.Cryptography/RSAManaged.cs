//
// RSAManaged.cs - Implements the RSA algorithm.
//
// Authors:
//	Sebastien Pouliot (spouliot@motus.com)
//	Ben Maurer (bmaurer@users.sf.net)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Portions (C) 2003 Ben Maurer
//
// Key generation translated from Bouncy Castle JCE (http://www.bouncycastle.org/)
// See bouncycastle.txt for license.
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

	public class RSAManaged	: RSA {

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

		public RSAManaged () : this (defaultKeySize) {}

		public RSAManaged (int dwKeySize) 
		{
			KeySizeValue = dwKeySize;
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
				p = BigInteger.genPseudoPrime (pbitlength);
				if (p % uint_e != 1)
					break;
			}
			// generate a modulus of the required length
			for (;;) {
				// generate q, prime and (q-1) relatively prime to e,
				// and not equal to p
				for (;;) {
					q = BigInteger.genPseudoPrime (qbitlength);
					if ((q % uint_e != 1) && (p != q))
						break;
				}
	
				// calculate the modulus
				n = p * q;
				if (n.bitCount () == KeySize)
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
			d = e.modInverse (phi);
	
			// calculate the CRT factors
			dp = d % pSub1;
			dq = d % qSub1;
			qInv = q.modInverse (p);
	
			keypairGenerated = true;
			isCRTpossible = true;
		}
		
		// overrides from RSA class

		public override int KeySize {
			get { 
				// in case keypair hasn't been (yet) generated
				if (keypairGenerated)
					return n.bitCount (); 
				else
					return base.KeySize;
			}
		}
		public override string KeyExchangeAlgorithm {
			get { return "RSA-PKCS1-KeyEx"; }
		}

		public override string SignatureAlgorithm {
			get { return "http://www.w3.org/2000/09/xmldsig#rsa-sha1"; }
		}

		public override byte[] DecryptValue (byte[] rgb) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("private key");

			// it would be stupid to decrypt data with a newly
			// generated keypair - so we return null
			if (!keypairGenerated)
				return null;

			BigInteger input = new BigInteger (rgb);
			BigInteger output;
			// decrypt (which uses the private key) can be 
			// optimized by using CRT (Chinese Remainder Theorem)
			if (isCRTpossible) {
				// m1 = c^dp mod p
				BigInteger m1 = input.modPow (dp, p);
				// m2 = c^dq mod q
				BigInteger m2 = input.modPow (dq, q);
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
				output = input.modPow (d, n);
			}
			byte[] result = output.getBytes ();
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
			BigInteger output = input.modPow (e, n);
			byte[] result = output.getBytes ();
			// zeroize value
			input.Clear ();	
			output.Clear ();
			return result;
		}

		public override RSAParameters ExportParameters (bool includePrivateParameters) 
		{
			if (!keypairGenerated)
				GenerateKeyPair ();
	
			RSAParameters param = new RSAParameters ();
			param.Exponent = e.getBytes ();
			param.Modulus = n.getBytes ();
			if (includePrivateParameters) {
				param.D = d.getBytes ();
				param.DP = dp.getBytes ();
				param.DQ = dq.getBytes ();
				param.InverseQ = qInv.getBytes ();
				param.P = p.getBytes ();
				param.Q = q.getBytes ();
			}
			return param;
		}

		public override void ImportParameters (RSAParameters parameters) 
		{
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
	}
}
