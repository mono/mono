//
// RSAManaged.cs - Implements the RSA algorithm.
//
// Authors:
//	Sebastien Pouliot (sebastien@ximian.com)
//	Ben Maurer (bmaurer@users.sf.net)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Portions (C) 2003 Ben Maurer
// Copyright (C) 2004,2006 Novell, Inc (http://www.novell.com)
//
// Key generation translated from Bouncy Castle JCE (http://www.bouncycastle.org/)
// See bouncycastle.txt for license.
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
using System.Text;

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
		private bool keyBlinding = true;
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
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (384, 16384, 8);
			base.KeySize = keySize;
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

		// note: when (if) we generate a keypair then it will have both
		// the public and private keys
		public bool PublicOnly {
			get { return (keypairGenerated && ((d == null) || (n == null))); }
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
			BigInteger r = null;

			// we use key blinding (by default) against timing attacks
			if (keyBlinding) {
				// x = (r^e * g) mod n 
				// *new* random number (so it's timing is also random)
				r = BigInteger.GenerateRandom (n.BitCount ());
				input = r.ModPow (e, n) * input % n;
			}

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
				} else {
					// h = (m1 - m2) * qInv mod p
					h = (m1 - m2) * qInv % p;
					// m = m2 + q * h;
					output = m2 + q * h;
				}
			} else if (!PublicOnly) {
				// m = c^d mod n
				output = input.ModPow (d, n);
			} else {
				throw new CryptographicException (Locale.GetText ("Missing private key to decrypt value."));
			}

			if (keyBlinding) {
				// Complete blinding
				// x^e / r mod n
				output = output * r.ModInverse (n) % n;
				r.Clear ();
			}

			// it's sometimes possible for the results to be a byte short
			// and this can break some software (see #79502) so we 0x00 pad the result
			byte[] result = GetPaddedValue (output, (KeySize >> 3));
			// zeroize values
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
			// it's sometimes possible for the results to be a byte short
			// and this can break some software (see #79502) so we 0x00 pad the result
			byte[] result = GetPaddedValue (output, (KeySize >> 3));
			// zeroize value
			input.Clear ();	
			output.Clear ();
			return result;
		}



		public override RSAParameters ExportParameters (bool includePrivateParameters) 
		{
			if (m_disposed)
				throw new ObjectDisposedException (Locale.GetText ("Keypair was disposed"));

			if (!keypairGenerated)
				GenerateKeyPair ();
	
			RSAParameters param = new RSAParameters ();
			param.Exponent = e.GetBytes ();
			param.Modulus = n.GetBytes ();
			if (includePrivateParameters) {
				// some parameters are required for exporting the private key
				if (d == null)
					throw new CryptographicException ("Missing private key");
				param.D = d.GetBytes ();
				// hack for bugzilla #57941 where D wasn't provided
				if (param.D.Length != param.Modulus.Length) {
					byte[] normalizedD = new byte [param.Modulus.Length];
					Buffer.BlockCopy (param.D, 0, normalizedD, (normalizedD.Length - param.D.Length), param.D.Length);
					param.D = normalizedD;
				}
				// but CRT parameters are optionals
				if ((p != null) && (q != null) && (dp != null) && (dq != null) && (qInv != null)) {
					// and we include them only if we have them all
					int length = (KeySize >> 4);
					param.P = GetPaddedValue (p, length);
					param.Q = GetPaddedValue (q, length);
					param.DP = GetPaddedValue (dp, length);
					param.DQ = GetPaddedValue (dq, length);
					param.InverseQ = GetPaddedValue (qInv, length);
				}
			}
			return param;
		}

		public override void ImportParameters (RSAParameters parameters) 
		{
			if (m_disposed)
				throw new ObjectDisposedException (Locale.GetText ("Keypair was disposed"));

			// if missing "mandatory" parameters
			if (parameters.Exponent == null) 
				throw new CryptographicException (Locale.GetText ("Missing Exponent"));
			if (parameters.Modulus == null)
				throw new CryptographicException (Locale.GetText ("Missing Modulus"));
	
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
			bool privateKey = ((p != null) && (q != null) && (dp != null));
			isCRTpossible = (privateKey && (dq != null) && (qInv != null));

			// check if the public/private keys match
			// the way the check is made allows a bad D to work if CRT is available (like MS does, see unit tests)
			if (!privateKey)
				return;

			// always check n == p * q
			bool ok = (n == (p * q));
			if (ok) {
				// we now know that p and q are correct, so (p - 1), (q - 1) and phi will be ok too
				BigInteger pSub1 = (p - 1);
				BigInteger qSub1 = (q - 1);
				BigInteger phi = pSub1 * qSub1;
				// e is fairly static but anyway we can ensure it makes sense by recomputing d
				BigInteger dcheck = e.ModInverse (phi);

				// now if our new d(check) is different than the d we're provided then we cannot
				// be sure if 'd' or 'e' is invalid... (note that, from experience, 'd' is more 
				// likely to be invalid since it's twice as large as DP (or DQ) and sits at the
				// end of the structure (e.g. truncation).
				ok = (d == dcheck);

				// ... unless we have the pre-computed CRT parameters
				if (!ok && isCRTpossible) {
					// we can override the previous decision since Mono always prefer, for 
					// performance reasons, using the CRT algorithm
					ok = (dp == (dcheck % pSub1)) && (dq == (dcheck % qSub1)) && 
						(qInv == q.ModInverse (p));
				}
			}

			if (!ok)
				throw new CryptographicException (Locale.GetText ("Private/public key mismatch"));
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

		public override string ToXmlString (bool includePrivateParameters) 
		{
			StringBuilder sb = new StringBuilder ();
			RSAParameters rsaParams = ExportParameters (includePrivateParameters);
			try {
				sb.Append ("<RSAKeyValue>");
				
				sb.Append ("<Modulus>");
				sb.Append (Convert.ToBase64String (rsaParams.Modulus));
				sb.Append ("</Modulus>");

				sb.Append ("<Exponent>");
				sb.Append (Convert.ToBase64String (rsaParams.Exponent));
				sb.Append ("</Exponent>");

				if (includePrivateParameters) {
					if (rsaParams.P != null) {
						sb.Append ("<P>");
						sb.Append (Convert.ToBase64String (rsaParams.P));
						sb.Append ("</P>");
					}
					if (rsaParams.Q != null) {
						sb.Append ("<Q>");
						sb.Append (Convert.ToBase64String (rsaParams.Q));
						sb.Append ("</Q>");
					}
					if (rsaParams.DP != null) {
						sb.Append ("<DP>");
						sb.Append (Convert.ToBase64String (rsaParams.DP));
						sb.Append ("</DP>");
					}
					if (rsaParams.DQ != null) {
						sb.Append ("<DQ>");
						sb.Append (Convert.ToBase64String (rsaParams.DQ));
						sb.Append ("</DQ>");
					}
					if (rsaParams.InverseQ != null) {
						sb.Append ("<InverseQ>");
						sb.Append (Convert.ToBase64String (rsaParams.InverseQ));
						sb.Append ("</InverseQ>");
					}
					sb.Append ("<D>");
					sb.Append (Convert.ToBase64String (rsaParams.D));
					sb.Append ("</D>");
				}
				
				sb.Append ("</RSAKeyValue>");
			}
			catch {
				if (rsaParams.P != null)
					Array.Clear (rsaParams.P, 0, rsaParams.P.Length);
				if (rsaParams.Q != null)
					Array.Clear (rsaParams.Q, 0, rsaParams.Q.Length);
				if (rsaParams.DP != null)
					Array.Clear (rsaParams.DP, 0, rsaParams.DP.Length);
				if (rsaParams.DQ != null)
					Array.Clear (rsaParams.DQ, 0, rsaParams.DQ.Length);
				if (rsaParams.InverseQ != null)
					Array.Clear (rsaParams.InverseQ, 0, rsaParams.InverseQ.Length);
				if (rsaParams.D != null)
					Array.Clear (rsaParams.D, 0, rsaParams.D.Length);
				throw;
			}
			
			return sb.ToString ();
		}

		// internal for Mono 1.0.x in order to preserve public contract
		// they are public for Mono 1.1.x (for 1.2) as the API isn't froze ATM

		public bool UseKeyBlinding {
			get { return keyBlinding; }
			// you REALLY shoudn't touch this (true is fine ;-)
			set { keyBlinding = value; }
		}

		public bool IsCrtPossible {
			// either the key pair isn't generated (and will be 
			// generated with CRT parameters) or CRT is (or isn't)
			// possible (in case the key was imported)
			get { return (!keypairGenerated || isCRTpossible); }
		}

		private byte[] GetPaddedValue (BigInteger value, int length)
		{
			byte[] result = value.GetBytes ();
			if (result.Length >= length)
				return result;

			// left-pad 0x00 value on the result (same integer, correct length)
			byte[] padded = new byte[length];
			Buffer.BlockCopy (result, 0, padded, (length - result.Length), result.Length);
			// temporary result may contain decrypted (plaintext) data, clear it
			Array.Clear (result, 0, result.Length);
			return padded;
		}
	}
}
