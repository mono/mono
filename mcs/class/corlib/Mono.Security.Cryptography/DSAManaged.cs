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

namespace Mono.Security.Cryptography {

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class DSAManaged : DSA {

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
		private bool j_missing;

		private RandomNumberGenerator rng;

		public DSAManaged () : this (defaultKeySize) {}

		public DSAManaged (int dwKeySize)
		{
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
			if (KeyGenerated != null)
				KeyGenerated (this, null);
		}

		// this part is quite fast
		private void GenerateKeyPair () 
		{
			x = BigInteger.GenerateRandom (160);
			while ((x == 0) || (x >= q)) {
				// size of x (private key) isn't affected by the keysize (512-1024)
				x.Randomize ();
			}

			// calculate the public key y = g^x % p
			y = g.ModPow (x, p);
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
					Random.GetBytes (seed);
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
				while (!q.IsProbablePrime ());

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

					if (p.TestBit ((uint)(keyLength - 1))) {
						if (p.IsProbablePrime ()) {
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
				BigInteger h = BigInteger.GenerateRandom (keyLength);
				if ((h <= 1) || (h >= (p - 1)))
					continue;

				g = h.ModPow (pMinusOneOverQ, p);
				if (g <= 1)
					continue;
				break;
			}

			this.seed = new BigInteger (seed);
			j = (p - 1) / q;
		}

		private RandomNumberGenerator Random {
			get { 
				if (rng == null)
					rng = RandomNumberGenerator.Create ();
				return rng;
			}
		}

		// overrides from DSA class

		public override int KeySize {
			get { 
				// in case keypair hasn't been (yet) generated
				if (keypairGenerated)
					return p.BitCount (); 
				else
					return base.KeySize;
			}
		}

		public override string KeyExchangeAlgorithm {
			get { return null; }
		}

		// note: when (if) we generate a keypair then it will have both
		// the public and private keys
		public bool PublicOnly {
			get { return ((keypairGenerated) && (x == null)); }
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
				throw new ObjectDisposedException (Locale.GetText ("Keypair was disposed"));

			if (!keypairGenerated)
				Generate ();

			if ((includePrivateParameters) && (x == null))
				throw new CryptographicException ("no private key to export");
	
			DSAParameters param = new DSAParameters ();
			// all parameters must be in multiple of 4 bytes arrays
			// this isn't (generally) a problem for most of the parameters
			// except for J (but we won't take a chance)
			param.P = NormalizeArray (p.GetBytes ());
			param.Q = NormalizeArray (q.GetBytes ());
			param.G = NormalizeArray (g.GetBytes ());
			param.Y = NormalizeArray (y.GetBytes ());
			if (!j_missing) {
				param.J = NormalizeArray (j.GetBytes ());
			}
			if (seed != 0) {
				param.Seed = NormalizeArray (seed.GetBytes ());
				param.Counter = counter;
			}
			if (includePrivateParameters) {
				byte[] privateKey = x.GetBytes ();
				if (privateKey.Length == 20) {
					param.X = NormalizeArray (privateKey);
				}
			}
			return param;
		}

		public override void ImportParameters (DSAParameters parameters) 
		{
			if (m_disposed)
				throw new ObjectDisposedException (Locale.GetText ("Keypair was disposed"));

			// if missing "mandatory" parameters
			if ((parameters.P == null) || (parameters.Q == null) || (parameters.G == null))
				throw new CryptographicException (Locale.GetText ("Missing mandatory DSA parameters (P, Q or G)."));
			// We can calculate Y from X, but both can't be missing
			if ((parameters.X == null) && (parameters.Y == null))
				throw new CryptographicException (Locale.GetText ("Missing both public (Y) and private (X) keys."));

			p = new BigInteger (parameters.P);
			q = new BigInteger (parameters.Q);
			g = new BigInteger (parameters.G);
			// optional parameter - private key
			if (parameters.X != null)
				x = new BigInteger (parameters.X);
			else
				x = null;
			// we can calculate Y from X if required
			if (parameters.Y != null)
				y = new BigInteger (parameters.Y);
			else
				y = g.ModPow (x, p);
			// optional parameter - pre-computation
			if (parameters.J != null) {
				j = new BigInteger (parameters.J);
			} else {
				j = (p - 1) / q;
				j_missing = true;
			}
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
				throw new ObjectDisposedException (Locale.GetText ("Keypair was disposed"));

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
			BigInteger k = BigInteger.GenerateRandom (160);
			while (k >= q)
				k.Randomize ();
			// (b) Compute r = (g^k mod p) mod q
			BigInteger r = (g.ModPow (k, p)) % q;
			// (c) Compute k -1 mod q (e.g., using Algorithm 2.142).
			// (d) Compute s = k -1 fh(m) +arg mod q.
			BigInteger s = (k.ModInverse (q) * (m + x * r)) % q;
			// (e) A's signature for m is the pair (r; s).
			byte[] signature = new byte [40];
			byte[] part1 = r.GetBytes ();
			byte[] part2 = s.GetBytes ();
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
				throw new ObjectDisposedException (Locale.GetText ("Keypair was disposed"));

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

				BigInteger w = s.ModInverse(q);
				BigInteger u1 = m * w % q;
				BigInteger u2 = r * w % q;

				u1 = g.ModPow(u1, p);
				u2 = y.ModPow(u2, p);

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

		public delegate void KeyGeneratedEventHandler (object sender, EventArgs e);

		public event KeyGeneratedEventHandler KeyGenerated;
	}
}
