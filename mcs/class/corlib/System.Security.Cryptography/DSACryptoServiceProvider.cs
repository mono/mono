//
// System.Security.Cryptography.DSACryptoServiceProvider.cs
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Key generation translated from Bouncy Castle JCE (http://www.bouncycastle.org/)
// See bouncycastle.txt for license.
//

using System;
using System.IO;

namespace System.Security.Cryptography {

public class DSACryptoServiceProvider : DSA {

	private CspParameters cspParams;
	private RandomNumberGenerator rng;

	private bool privateKeyExportable = true;
	private bool m_disposed = false;
	private bool keypairGenerated = false;
	private bool persistKey = false;

	private BigInteger p;
	private BigInteger q;
	private BigInteger g;
	private BigInteger x;	// private key
	private BigInteger y;
	private BigInteger j;
	private BigInteger seed;
	private int counter;

	public DSACryptoServiceProvider () 
	{
		// Here it's not clear if we need to generate a keypair
		// (note: MS implementation generates a keypair in this case).
		// However we:
		// (a) often use this constructor to import an existing keypair.
		// (b) take a LOT of time to generate the DSA group
		// So we'll generate the keypair only when (and if) it's being
		// used (or exported). This should save us a lot of time (at 
		// least in the unit tests).
		Common (null);
	}

	public DSACryptoServiceProvider (CspParameters parameters) 
	{
		Common (parameters);
		// no keypair generation done at this stage
	}

	// This constructor will generate a new keypair
	public DSACryptoServiceProvider (int dwKeySize) 
	{
		// Here it's clear that we need to generate a new keypair
		Common (null);
		Generate (dwKeySize);
	}

	// This constructor will generate a new keypair
	public DSACryptoServiceProvider (int dwKeySize, CspParameters parameters) 
	{
		Common (parameters);
		Generate (dwKeySize);
	}

	~DSACryptoServiceProvider () 
	{
		Dispose (false);
	}

	[MonoTODO("Persistance")]
	private void Common (CspParameters p) 
	{
		rng = RandomNumberGenerator.Create ();
		cspParams = new CspParameters ();
		if (p == null) {
			// TODO: set default values (for keypair persistance)
		}
		else {
			cspParams = p;
			// FIXME: We'll need this to support some kind of persistance
			throw new NotSupportedException ("CspParameters not supported");
		}
		LegalKeySizesValue = new KeySizes [1];
		LegalKeySizesValue [0] = new KeySizes (512, 1024, 64);
	}

	// generate both the group and the keypair
	private void Generate (int keyLength) 
	{
		// will throw an exception is key size isn't supported
		base.KeySize = keyLength;
		GenerateParams (keyLength);
		GenerateKeyPair ();
		keypairGenerated = true;
	}

	// this part is quite fast
	private void GenerateKeyPair () 
	{
		x = new BigInteger ();
		do {
			// size of x (private key) isn't affected by the keysize (512-1024)
			x.genRandomBits (160);
		}
		while ((x == 0) || (x >= q));

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

		SHA1 sha = SHA1.Create ();

		int n = (keyLength - 1) / 160;
		byte[] w = new byte [keyLength / 8];
		bool primesFound = false;
		int certainty = 80; // FIPS186-2

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
			while (!q.isProbablePrime (certainty));

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
					if (p.isProbablePrime (certainty)) {
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
			BigInteger h = new BigInteger ();
			h.genRandomBits (keyLength);
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

	// DSA isn't used for key exchange
	public override string KeyExchangeAlgorithm {
		get { return ""; }
	}

	public override int KeySize {
		get { return p.bitCount (); }
	}

	public override KeySizes[] LegalKeySizes {
		get { return LegalKeySizesValue; }
	}

	public override string SignatureAlgorithm {
		get { return "http://www.w3.org/2000/09/xmldsig#dsa-sha1"; }
	}

	[MonoTODO("Persistance")]
	public bool PersistKeyInCsp {
		get { return persistKey; }
		set { 
			persistKey = value;
			// FIXME: We'll need this to support some kind of persistance
			if (value)
				throw new NotSupportedException ("CspParameters not supported");
		}
	}
	
	protected override void Dispose (bool disposing) 
	{
		if (!m_disposed) {
			// TODO: always zeroize private key
			if(disposing) {
				// TODO: Dispose managed resources
			}
         
			// TODO: Dispose unmanaged resources
		}
		// call base class 
		// no need as they all are abstract before us
		m_disposed = true;
	}

	public override byte[] CreateSignature (byte[] rgbHash) 
	{
		return SignHash (rgbHash, "SHA1");
	}
	
	public byte[] SignData (byte[] data) 
	{
		return SignData (data, 0, data.Length);
	}

	public byte[] SignData (byte[] data, int offset, int count) 
	{
		// right now only SHA1 is supported by FIPS186-2
		HashAlgorithm hash = SHA1.Create ();
		byte[] toBeSigned = hash.ComputeHash (data, offset, count);
		return SignHash (toBeSigned, "SHA1");
	}

	public byte[] SignData (Stream inputStream) 
	{
		// right now only SHA1 is supported by FIPS186-2
		HashAlgorithm hash = SHA1.Create ();
		byte[] toBeSigned = hash.ComputeHash (inputStream);
		return SignHash (toBeSigned, "SHA1");
	}

	public byte[] SignHash (byte[] rgbHash, string str) 
	{
		if (rgbHash == null)
			throw new ArgumentNullException ();
		if (x.ToString() == "0")
			throw new CryptographicException ("no private key available for signature");
		// right now only SHA1 is supported by FIPS186-2
		if (str.ToUpper () != "SHA1")
			throw new Exception (); // not documented
		if (rgbHash.Length != 20)
			throw new Exception (); // not documented

		if (!keypairGenerated)
			Generate (1024);

		BigInteger m = new BigInteger (rgbHash);
		// (a) Select a random secret integer k; 0 < k < q.
		BigInteger k = new BigInteger ();
		k.genRandomBits (160);
		while (k >= q)
			k.genRandomBits (160);
		// (b) Compute r = ( k mod p) mod q
		BigInteger r = (g.modPow (k, p)) % q;
		// (c) Compute k -1 mod q (e.g., using Algorithm 2.142).
		// (d) Compute s = k -1 fh(m) +arg mod q.
		BigInteger s = (k.modInverse (q) * (m + x * r)) % q;
		// (e) A’s signature for m is the pair (r; s).
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

	public bool VerifyData (byte[] rgbData, byte[] rgbSignature) 
	{
		// signature is always 40 bytes (no matter the size of the 
		// public key). In fact it is 2 times the size of the private
		// key (which is 20 bytes for 512 to 1024 bits DSA keypairs)
		if (rgbSignature.Length != 40)
			throw new Exception(); // not documented
		// right now only SHA1 is supported by FIPS186-2
		HashAlgorithm hash = SHA1.Create();
		byte[] toBeVerified = hash.ComputeHash (rgbData);
		return VerifyHash (toBeVerified, "SHA1", rgbSignature);
	}

	// LAMESPEC: MD5 isn't allowed with DSA
	public bool VerifyHash (byte[] rgbHash, string str, byte[] rgbSignature) 
	{
		if (rgbHash == null)
			throw new ArgumentNullException ("rgbHash");
		if (rgbSignature == null)
			throw new ArgumentNullException ("rgbSignature");
		if (str == null)
			str = "SHA1"; // default value
		if (str != "SHA1")
			throw new CryptographicException ();

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
			throw new CryptographicException ();
		}
	}

	public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature) 
	{
		return VerifyHash (rgbHash, "SHA1", rgbSignature);
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
		if ((includePrivateParameters) && (!privateKeyExportable))
			throw new CryptographicException ("cannot export private key");
		if (!keypairGenerated)
			Generate (1024);

		DSAParameters param = new DSAParameters();
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
			x = new BigInteger (0);
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
			seed = new BigInteger (0);

		// we now have a keypair
		keypairGenerated = true;
	}
}

}
