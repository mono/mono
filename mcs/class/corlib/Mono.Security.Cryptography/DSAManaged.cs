//
// DSAManaged.cs - Implements the DSA algorithm.
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
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

namespace Mono.Security.Cryptography {

	public class DSAManaged : DSA {

		private const int defaultKeySize = 1024;

		private bool keypairGenerated = false;
		private bool m_disposed = false;

		private BigInteger p;
		private BigInteger q;
		private BigInteger g;
		private BigInteger x;	// private key
		private BigInteger y;
		private BigInteger j;
		private BigInteger seed;
		private int counter;

		private RandomNumberGenerator rng;

		public DSAManaged () : this (defaultKeySize) {}

		public DSAManaged (int dwKeySize)
		{
			rng = RandomNumberGenerator.Create ();
			KeySizeValue = dwKeySize;
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (512, 1024, 64);
		}

		~DSAManaged () 
		{
			// Zeroize private key
			Dispose (false);
		}

		// generate both the group and the keypair
		private void Generate () 
		{
			GenerateParams (base.KeySize);
			GenerateKeyPair ();
			keypairGenerated = true;
		}

		// this part is quite fast
		private void GenerateKeyPair () 
		{
			x = BigInteger.genRandom (160);
			while ((x == 0) || (x >= q)) {
				// size of x (private key) isn't affected by the keysize (512-1024)
				x.randomize ();
			}

			// calculate the public key y = g^x % p
			y = g.modPow (x, p);
		}

		private void add (byte[] a, byte[] b, int value) 
		{
			uint x = (uint) ((b [b.Length - 1] & 0xff) + value);

			a [b.Length - 1] = (byte)x;
			x >>= 8;

			for (int i = b.Length - 2; i >= 0; i--) {
				x += (uint) (b [i] & 0xff);
				a [i] = (byte)x;
				x >>= 8;
			}
		}

		private void GenerateParams (int keyLength) 
		{
			byte[] seed = new byte[20];
			byte[] part1 = new byte[20];
			byte[] part2 = new byte[20];
			byte[] u = new byte[20];

			// TODO: a prime generator should be made for this

			SHA1 sha = SHA1.Create ();

			int n = (keyLength - 1) / 160;
			byte[] w = new byte [keyLength / 8];
			bool primesFound = false;

			while (!primesFound) {
				do {
					rng.GetBytes (seed);
					part1 = sha.ComputeHash (seed);
					Array.Copy(seed, 0, part2, 0, seed.Length);

					add (part2, seed, 1);

					part2 = sha.ComputeHash (part2);

					for (int i = 0; i != u.Length; i++)
						u [i] = (byte)(part1 [i] ^ part2 [i]);

					// first bit must be set (to respect key length)
					u[0] |= (byte)0x80;
					// last bit must be set (prime are all odds - except 2)
					u[19] |= (byte)0x01;

					q = new BigInteger (u);
				}
				while (!q.isProbablePrime ());

				counter = 0;
				int offset = 2;
				while (counter < 4096) {
					for (int k = 0; k < n; k++) {
						add(part1, seed, offset + k);
						part1 = sha.ComputeHash (part1);
						Array.Copy (part1, 0, w, w.Length - (k + 1) * part1.Length, part1.Length);
					}

					add(part1, seed, offset + n);
					part1 = sha.ComputeHash (part1);
					Array.Copy (part1, part1.Length - ((w.Length - (n) * part1.Length)), w, 0, w.Length - n * part1.Length);

					w[0] |= (byte)0x80;
					BigInteger x = new BigInteger (w);

					BigInteger c = x % (q * 2);

					p = x - (c - 1);

					if (p.testBit ((uint)(keyLength - 1))) {
						if (p.isProbablePrime ()) {
							primesFound = true;
							break;
						}
					}

					counter += 1;
					offset += n + 1;
				}
			}

			// calculate the generator g
			BigInteger pMinusOneOverQ = (p - 1) / q;
			for (;;) {
				BigInteger h = BigInteger.genRandom (keyLength);
				if ((h <= 1) || (h >= (p - 1)))
					continue;

				g = h.modPow (pMinusOneOverQ, p);
				if (g <= 1)
					continue;
				break;
			}

			this.seed = new BigInteger (seed);
			j = (p - 1) / q;
		}

		[MonoTODO()]
		private bool Validate () 
		{
			// J is optional
			bool okJ = ((j == 0) || (j == ((p - 1) / q)));
			// TODO: Validate the key parameters (P, Q, G, J) using the Seed and Counter
			return okJ;
		}

		// overrides from DSA class

		public override int KeySize {
			get { 
				// in case keypair hasn't been (yet) generated
				if (keypairGenerated)
					return p.bitCount (); 
				else
					return base.KeySize;
			}
		}

		public override string KeyExchangeAlgorithm {
			get { return null; }
		}

		public override string SignatureAlgorithm {
			get { return "http://www.w3.org/2000/09/xmldsig#dsa-sha1"; }
		}

		private byte[] NormalizeArray (byte[] array) 
		{
			int n = (array.Length % 4);
			if (n > 0) {
				byte[] temp = new byte [array.Length + 4 - n];
				Array.Copy (array, 0, temp, (4 - n), array.Length);
				return temp;
			}
			else
				return array;
		}

		public override DSAParameters ExportParameters (bool includePrivateParameters)
		{
			if (m_disposed)
				throw new ObjectDisposedException ("");

			if (!keypairGenerated)
				Generate ();

			if ((includePrivateParameters) && (x == null))
				throw new CryptographicException ("no private key to export");
	
			DSAParameters param = new DSAParameters ();
			// all parameters must be in multiple of 4 bytes arrays
			// this isn't (generally) a problem for most of the parameters
			// except for J (but we won't take a chance)
			param.P = NormalizeArray (p.getBytes ());
			param.Q = NormalizeArray (q.getBytes ());
			param.G = NormalizeArray (g.getBytes ());
			param.Y = NormalizeArray (y.getBytes ());
			param.J = NormalizeArray (j.getBytes ());
			if (seed != 0) {
				param.Seed = NormalizeArray (seed.getBytes ());
				param.Counter = counter;
			}
			if (includePrivateParameters) {
				byte[] privateKey = x.getBytes ();
				if (privateKey.Length == 20) {
					param.X = NormalizeArray (privateKey);
				}
			}
			return param;
		}

		public override void ImportParameters (DSAParameters parameters) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("");

			// if missing "mandatory" parameters
			if ((parameters.P == null) || (parameters.Q == null) || (parameters.G == null) || (parameters.Y == null))
				throw new CryptographicException ();

			p = new BigInteger (parameters.P);
			q = new BigInteger (parameters.Q);
			g = new BigInteger (parameters.G);
			y = new BigInteger (parameters.Y);
			// optional parameter - private key
			if (parameters.X != null)
				x = new BigInteger (parameters.X);
			else
				x = null;
			// optional parameter - pre-computation
			if (parameters.J != null)
				j = new BigInteger (parameters.J);
			else
				j = (p - 1) / q;
			// optional - seed and counter must both be present (or absent)
			if (parameters.Seed != null) {
				seed = new BigInteger (parameters.Seed);
				counter = parameters.Counter;
			}
			else
				seed = 0;

			// we now have a keypair
			keypairGenerated = true;
		}

		public override byte[] CreateSignature (byte[] rgbHash) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("");

			if (rgbHash == null)
				throw new ArgumentNullException ("rgbHash");
			if (rgbHash.Length != 20)
				throw new CryptographicException ("invalid hash length");

			if (!keypairGenerated)
				Generate ();

			// if required key must be generated before checking for X
			if (x == null)
				throw new CryptographicException ("no private key available for signature");
	
			BigInteger m = new BigInteger (rgbHash);
			// (a) Select a random secret integer k; 0 < k < q.
			BigInteger k = BigInteger.genRandom (160);
			while (k >= q)
				k.randomize ();
			// (b) Compute r = (g^k mod p) mod q
			BigInteger r = (g.modPow (k, p)) % q;
			// (c) Compute k -1 mod q (e.g., using Algorithm 2.142).
			// (d) Compute s = k -1 fh(m) +arg mod q.
			BigInteger s = (k.modInverse (q) * (m + x * r)) % q;
			// (e) A's signature for m is the pair (r; s).
			byte[] signature = new byte [40];
			byte[] part1 = r.getBytes ();
			byte[] part2 = s.getBytes ();
			// note: sometime (1/256) we may get less than 20 bytes (if first is 00)
			int start = 20 - part1.Length;
			Array.Copy (part1, 0, signature, start, part1.Length);
			start = 40 - part2.Length;
			Array.Copy (part2, 0, signature, start, part2.Length);
			return signature;
		}

		public override bool VerifySignature (byte[] rgbHash, byte[] rgbSignature) 
		{
			if (m_disposed)
				throw new ObjectDisposedException ("");

			if (rgbHash == null)
				throw new ArgumentNullException ("rgbHash");
			if (rgbSignature == null)
				throw new ArgumentNullException ("rgbSignature");

			if (rgbHash.Length != 20)
				throw new CryptographicException ("invalid hash length");
			// signature is always 40 bytes (no matter the size of the
			// public key). In fact it is 2 times the size of the private
			// key (which is 20 bytes for 512 to 1024 bits DSA keypairs)
			if (rgbSignature.Length != 40)
				throw new CryptographicException ("invalid signature length");

			// it would be stupid to verify a signature with a newly
			// generated keypair - so we return false
			if (!keypairGenerated)
				return false;

			try {
				BigInteger m = new BigInteger (rgbHash);
				byte[] half = new byte [20];
				Array.Copy (rgbSignature, 0, half, 0, 20);
				BigInteger r = new BigInteger (half);
				Array.Copy (rgbSignature, 20, half, 0, 20);
				BigInteger s = new BigInteger (half);

				if ((r < 0) || (q <= r))
					return false;

				if ((s < 0) || (q <= s))
					return false;

				BigInteger w = s.modInverse(q);
				BigInteger u1 = m * w % q;
				BigInteger u2 = r * w % q;

				u1 = g.modPow(u1, p);
				u2 = y.modPow(u2, p);

				BigInteger v = ((u1 * u2 % p) % q);
				return (v == r);
			}
			catch {
				throw new CryptographicException ("couldn't compute signature verification");
			}
		}

		protected override void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				// Always zeroize private key
				if (x != null) {
					x.Clear (); 
					x = null;
				}

				if (disposing) {
					// clear group
					if (p != null) {
						p.Clear (); 
						p = null;
					}
					if (q != null) {
						q.Clear (); 
						q = null;
					}
					if (g != null) {
						g.Clear (); 
						g = null;
					}
					if (j != null) {
						j.Clear (); 
						j = null;
					}
					if (seed != null) {
						seed.Clear (); 
						seed = null;
					}
					// clear public key
					if (y != null) {
						y.Clear (); 
						y = null;
					}
				}
			}
			// call base class 
			// no need as they all are abstract before us
			m_disposed = true;
		}
	}
}
