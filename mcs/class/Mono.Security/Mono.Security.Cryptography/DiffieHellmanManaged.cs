//
// DiffieHellmanManaged.cs: Implements the Diffie-Hellman key agreement algorithm
//
// Author:
//	Pieter Philippaerts (Pieter@mentalis.org)
//
// (C) 2003 The Mentalis.org Team (http://www.mentalis.org/)
//
//   References:
//     - PKCS#3  [http://www.rsasecurity.com/rsalabs/pkcs/pkcs-3/]
//

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
	/// <summary>
	/// Implements the Diffie-Hellman algorithm.
	/// </summary>
	public sealed class DiffieHellmanManaged : DiffieHellman {
		/// <summary>
		/// Initializes a new <see cref="DiffieHellmanManaged"/> instance.
		/// </summary>
		/// <remarks>The default length of the shared secret is 1024 bits.</remarks>
		public DiffieHellmanManaged() : this(1024, 160, DHKeyGeneration.Static) {}
		/// <summary>
		/// Initializes a new <see cref="DiffieHellmanManaged"/> instance.
		/// </summary>
		/// <param name="bitLength">The length, in bits, of the public P parameter.</param>
		/// <param name="l">The length, in bits, of the secret value X. This parameter can be set to 0 to use the default size.</param>
		/// <param name="method">One of the <see cref="DHKeyGeneration"/> values.</param>
		/// <remarks>The larger the bit length, the more secure the algorithm is. The default is 1024 bits. The minimum bit length is 128 bits.<br/>The size of the private value will be one fourth of the bit length specified.</remarks>
		/// <exception cref="ArgumentException">The specified bit length is invalid.</exception>
		public DiffieHellmanManaged(int bitLength, int l, DHKeyGeneration method) {
			if (bitLength < 256 || l < 0)
				throw new ArgumentException();
			BigInteger p, g;
			GenerateKey (bitLength, method, out p, out g);
			Initialize(p, g, null, l, false);
		}
		/// <summary>
		/// Initializes a new <see cref="DiffieHellmanManaged"/> instance.
		/// </summary>
		/// <param name="p">The P parameter of the Diffie-Hellman algorithm. This is a public parameter.</param>
		/// <param name="g">The G parameter of the Diffie-Hellman algorithm. This is a public parameter.</param>
		/// <param name="x">The X parameter of the Diffie-Hellman algorithm. This is a private parameter. If this parameters is a null reference (<b>Nothing</b> in Visual Basic), a secret value of the default size will be generated.</param>
		/// <exception cref="ArgumentNullException"><paramref name="p"/> or <paramref name="g"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="CryptographicException"><paramref name="p"/> or <paramref name="g"/> is invalid.</exception>
		public DiffieHellmanManaged(byte[] p, byte[] g, byte[] x) {
			if (p == null || g == null)
				throw new ArgumentNullException();
			if (x == null)
				Initialize(new BigInteger(p), new BigInteger(g), null, 0, true);
			else
				Initialize(new BigInteger(p), new BigInteger(g), new BigInteger(x), 0, true);
		}
		/// <summary>
		/// Initializes a new <see cref="DiffieHellmanManaged"/> instance.
		/// </summary>
		/// <param name="p">The P parameter of the Diffie-Hellman algorithm.</param>
		/// <param name="g">The G parameter of the Diffie-Hellman algorithm.</param>
		/// <param name="l">The length, in bits, of the private value. If 0 is specified, the default value will be used.</param>
		/// <exception cref="ArgumentNullException"><paramref name="p"/> or <paramref name="g"/> is a null reference (<b>Nothing</b> in Visual Basic).</exception>
		/// <exception cref="ArgumentException"><paramref name="l"/> is invalid.</exception>
		/// <exception cref="CryptographicException"><paramref name="p"/> or <paramref name="g"/> is invalid.</exception>
		public DiffieHellmanManaged(byte[] p, byte[] g, int l) {
			if (p == null || g == null)
				throw new ArgumentNullException();
			if (l < 0)
				throw new ArgumentException();
			Initialize(new BigInteger(p), new BigInteger(g), null, l, true);
		}

		// initializes the private variables (throws CryptographicException)
		private void Initialize(BigInteger p, BigInteger g, BigInteger x, int secretLen, bool checkInput) {
			if (checkInput) {
				if (!p.IsProbablePrime() || g <= 0 || g >= p || (x != null && (x <= 0 || x > p - 2)))
					throw new CryptographicException();
			}
			// default is to generate a number as large as the prime this
			// is usually overkill, but it's the most secure thing we can
			// do if the user doesn't specify a desired secret length ...
			if (secretLen == 0)
				secretLen = p.BitCount();
			m_P = p;
			m_G = g;
			if (x == null) {
				BigInteger pm1 = m_P - 1;
				for(m_X = BigInteger.GenerateRandom(secretLen); m_X >= pm1 || m_X == 0; m_X = BigInteger.GenerateRandom(secretLen)) {}
			} else {
				m_X = x;
			}
		}
		/// <summary>
		/// Creates the key exchange data.
		/// </summary>
		/// <returns>The key exchange data to be sent to the intended recipient.</returns>
		public override byte[] CreateKeyExchange() {
			BigInteger y = m_G.ModPow(m_X, m_P);
			byte[] ret = y.GetBytes();
			y.Clear();
			return ret;
		}
		/// <summary>
		/// Extracts secret information from the key exchange data.
		/// </summary>
		/// <param name="keyEx">The key exchange data within which the shared key is hidden.</param>
		/// <returns>The shared key derived from the key exchange data.</returns>
		public override byte[] DecryptKeyExchange(byte[] keyEx) {
			BigInteger pvr = new BigInteger(keyEx);
			BigInteger z = pvr.ModPow(m_X, m_P);
			byte[] ret = z.GetBytes();
			z.Clear();
			return ret;
		}
		/// <summary>
		/// Gets the name of the key exchange algorithm.
		/// </summary>
		/// <value>The name of the key exchange algorithm.</value>
		public override string KeyExchangeAlgorithm {
			get {
				return "1.2.840.113549.1.3"; // PKCS#3 OID
			}
		}
		/// <summary>
		/// Gets the name of the signature algorithm.
		/// </summary>
		/// <value>The name of the signature algorithm.</value>
		public override string SignatureAlgorithm {
			get {
				return null;
			}
		}
		// clear keys
		protected override void Dispose(bool disposing) {
			if (!m_Disposed) {
				m_P.Clear();
				m_G.Clear();
				m_X.Clear();
			}
			m_Disposed = true;
		}
		/// <summary>
		/// Exports the <see cref="DHParameters"/>.
		/// </summary>
		/// <param name="includePrivateParameters"><b>true</b> to include private parameters; otherwise, <b>false</b>.</param>
		/// <returns>The parameters for <see cref="DiffieHellman"/>.</returns>
		public override DHParameters ExportParameters(bool includePrivateParameters) {
			DHParameters ret = new DHParameters();
			ret.P = m_P.GetBytes();
			ret.G = m_G.GetBytes();
			if (includePrivateParameters) {
				ret.X = m_X.GetBytes();
			}
			return ret;
		}
		/// <summary>
		/// Imports the specified <see cref="DHParameters"/>.
		/// </summary>
		/// <param name="parameters">The parameters for <see cref="DiffieHellman"/>.</param>
		/// <exception cref="CryptographicException"><paramref name="P"/> or <paramref name="G"/> is a null reference (<b>Nothing</b> in Visual Basic) -or- <paramref name="P"/> is not a prime number.</exception>
		public override void ImportParameters(DHParameters parameters) {
			if (parameters.P == null)
				throw new CryptographicException("Missing P value.");
			if (parameters.G == null)
				throw new CryptographicException("Missing G value.");

			BigInteger p = new BigInteger(parameters.P), g = new BigInteger(parameters.G), x = null;
			if (parameters.X != null) {
				x = new BigInteger(parameters.X);
			}
			Initialize(p, g, x, 0, true);
		}
		~DiffieHellmanManaged() {
			Dispose(false);
		}

		//TODO: implement DH key generation methods
		private void GenerateKey(int bitlen, DHKeyGeneration keygen, out BigInteger p, out BigInteger g) {
			if (keygen == DHKeyGeneration.Static) {
				if (bitlen == 768)
					p = new BigInteger(m_OAKLEY768);
				else if (bitlen == 1024)
					p = new BigInteger(m_OAKLEY1024);
				else if (bitlen == 1536)
					p = new BigInteger(m_OAKLEY1536);
				else
					throw new ArgumentException("Invalid bit size.");
				g = new BigInteger(22); // all OAKLEY keys use 22 as generator
			//} else if (keygen == DHKeyGeneration.SophieGermain) {
			//	throw new NotSupportedException(); //TODO
			//} else if (keygen == DHKeyGeneration.DSA) {
				// 1. Let j = (p - 1)/q.
				// 2. Set h = any integer, where 1 < h < p - 1
				// 3. Set g = h^j mod p
				// 4. If g = 1 go to step 2
			//	BigInteger j = (p - 1) / q;
			} else { // random
				p = BigInteger.GeneratePseudoPrime(bitlen);
				g = new BigInteger(3); // always use 3 as a generator
			}
		}

		private BigInteger m_P;
		private BigInteger m_G;
		private BigInteger m_X;
		private bool m_Disposed;

		private static byte[] m_OAKLEY768 = new byte[] {
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC9, 0x0F, 0xDA, 0xA2,
			0x21, 0x68, 0xC2, 0x34, 0xC4, 0xC6, 0x62, 0x8B, 0x80, 0xDC, 0x1C, 0xD1,
			0x29, 0x02, 0x4E, 0x08, 0x8A, 0x67, 0xCC, 0x74, 0x02, 0x0B, 0xBE, 0xA6,
			0x3B, 0x13, 0x9B, 0x22, 0x51, 0x4A, 0x08, 0x79, 0x8E, 0x34, 0x04, 0xDD,
			0xEF, 0x95, 0x19, 0xB3, 0xCD, 0x3A, 0x43, 0x1B, 0x30, 0x2B, 0x0A, 0x6D,
			0xF2, 0x5F, 0x14, 0x37, 0x4F, 0xE1, 0x35, 0x6D, 0x6D, 0x51, 0xC2, 0x45,
			0xE4, 0x85, 0xB5, 0x76, 0x62, 0x5E, 0x7E, 0xC6, 0xF4, 0x4C, 0x42, 0xE9,
			0xA6, 0x3A, 0x36, 0x20, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
		};
		private static byte[] m_OAKLEY1024 = new byte[] {
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC9, 0x0F, 0xDA, 0xA2,
			0x21, 0x68, 0xC2, 0x34, 0xC4, 0xC6, 0x62, 0x8B, 0x80, 0xDC, 0x1C, 0xD1,
			0x29, 0x02, 0x4E, 0x08, 0x8A, 0x67, 0xCC, 0x74, 0x02, 0x0B, 0xBE, 0xA6,
			0x3B, 0x13, 0x9B, 0x22, 0x51, 0x4A, 0x08, 0x79, 0x8E, 0x34, 0x04, 0xDD,
			0xEF, 0x95, 0x19, 0xB3, 0xCD, 0x3A, 0x43, 0x1B, 0x30, 0x2B, 0x0A, 0x6D,
			0xF2, 0x5F, 0x14, 0x37, 0x4F, 0xE1, 0x35, 0x6D, 0x6D, 0x51, 0xC2, 0x45,
			0xE4, 0x85, 0xB5, 0x76, 0x62, 0x5E, 0x7E, 0xC6, 0xF4, 0x4C, 0x42, 0xE9,
			0xA6, 0x37, 0xED, 0x6B, 0x0B, 0xFF, 0x5C, 0xB6, 0xF4, 0x06, 0xB7, 0xED,
			0xEE, 0x38, 0x6B, 0xFB, 0x5A, 0x89, 0x9F, 0xA5, 0xAE, 0x9F, 0x24, 0x11,
			0x7C, 0x4B, 0x1F, 0xE6, 0x49, 0x28, 0x66, 0x51, 0xEC, 0xE6, 0x53, 0x81,
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
		};
		private static byte[] m_OAKLEY1536 = new byte[] {
			0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC9, 0x0F, 0xDA, 0xA2,
			0x21, 0x68, 0xC2, 0x34, 0xC4, 0xC6, 0x62, 0x8B, 0x80, 0xDC, 0x1C, 0xD1,
			0x29, 0x02, 0x4E, 0x08, 0x8A, 0x67, 0xCC, 0x74, 0x02, 0x0B, 0xBE, 0xA6,
			0x3B, 0x13, 0x9B, 0x22, 0x51, 0x4A, 0x08, 0x79, 0x8E, 0x34, 0x04, 0xDD,
			0xEF, 0x95, 0x19, 0xB3, 0xCD, 0x3A, 0x43, 0x1B, 0x30, 0x2B, 0x0A, 0x6D,
			0xF2, 0x5F, 0x14, 0x37, 0x4F, 0xE1, 0x35, 0x6D, 0x6D, 0x51, 0xC2, 0x45,
			0xE4, 0x85, 0xB5, 0x76, 0x62, 0x5E, 0x7E, 0xC6, 0xF4, 0x4C, 0x42, 0xE9,
			0xA6, 0x37, 0xED, 0x6B, 0x0B, 0xFF, 0x5C, 0xB6, 0xF4, 0x06, 0xB7, 0xED,
			0xEE, 0x38, 0x6B, 0xFB, 0x5A, 0x89, 0x9F, 0xA5, 0xAE, 0x9F, 0x24, 0x11,
			0x7C, 0x4B, 0x1F, 0xE6, 0x49, 0x28, 0x66, 0x51, 0xEC, 0xE4, 0x5B, 0x3D,
			0xC2, 0x00, 0x7C, 0xB8, 0xA1, 0x63, 0xBF, 0x05, 0x98, 0xDA, 0x48, 0x36,
			0x1C, 0x55, 0xD3, 0x9A, 0x69, 0x16, 0x3F, 0xA8, 0xFD, 0x24, 0xCF, 0x5F,
			0x83, 0x65, 0x5D, 0x23, 0xDC, 0xA3, 0xAD, 0x96, 0x1C, 0x62, 0xF3, 0x56,
			0x20, 0x85, 0x52, 0xBB, 0x9E, 0xD5, 0x29, 0x07, 0x70, 0x96, 0x96, 0x6D,
			0x67, 0x0C, 0x35, 0x4E, 0x4A, 0xBC, 0x98, 0x04, 0xF1, 0x74, 0x6C, 0x08,
			0xCA, 0x23, 0x73, 0x27, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
		};
	}
}