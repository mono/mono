//
// RSACryptoServiceProvider.cs: Handles an RSA implementation.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Key generation translated from Bouncy Castle JCE (http://www.bouncycastle.org/)
// See bouncycastle.txt for license.
//

using System;
using System.IO;

using Mono.Math;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	public sealed class RSACryptoServiceProvider : RSA {
	
		private CspParameters cspParams;
	
		private bool privateKeyExportable = true; 
		private bool keypairGenerated = false;
		private bool persistKey = false;
		private bool m_disposed = false;
	
		private BigInteger d;
		private BigInteger p;
		private BigInteger q;
		private BigInteger dp;
		private BigInteger dq;
		private BigInteger qInv;
		private BigInteger n;		// modulus
		private BigInteger e;
	
		public RSACryptoServiceProvider ()
		{
			// Here it's not clear if we need to generate a keypair
			// (note: MS implementation generates a keypair in this case).
			// However we:
			// (a) often use this constructor to import an existing keypair.
			// (b) take a LOT of time to generate the RSA keypair
			// So we'll generate the keypair only when (and if) it's being
			// used (or exported). This should save us a lot of time (at 
			// least in the unit tests).
			Common (1024, null);
		}
	
		public RSACryptoServiceProvider (CspParameters parameters) 
		{
			Common (1024, parameters);
			// no keypair generation done at this stage
		}
	
		public RSACryptoServiceProvider (int dwKeySize) 
		{
			// Here it's clear that we need to generate a new keypair
			Common (dwKeySize, null);
			// no keypair generation done at this stage
		}
	
		// FIXME: We currently dont link with MS CAPI. Anyway this makes
		// only sense in Windows - what do we do elsewhere ?
		public RSACryptoServiceProvider (int dwKeySize, CspParameters parameters) 
		{
			Common (dwKeySize, parameters);
			// no keypair generation done at this stage
		}
	
		[MonoTODO("Persistance")]
		// FIXME: We currently dont link with MS CAPI. Anyway this makes
		// only sense in Windows - what do we do elsewhere ?
		private void Common (int dwKeySize, CspParameters p) 
		{
			if (p == null) {
				cspParams = new CspParameters ();
				// TODO: set default values (for keypair persistance)
			}
			else
				cspParams = p;
				// FIXME: We'll need this to support some kind of persistance

			// Microsoft RSA CSP can do between 384 and 16384 bits keypair
			// we limit ourselve to 2048 because (a) BigInteger limits and (b) it's so SLOW
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (384, 2048, 8);
			KeySize = dwKeySize;
		}
	
		private void GenerateKeyPair () 
		{
			// p and q values should have a length of half the strength in bits
			int pbitlength = ((KeySize + 1) >> 1);
			int qbitlength = (KeySize - pbitlength);
			e = new BigInteger (17); // fixed
	
			// generate p, prime and (p-1) relatively prime to e
			for (;;) {
				p = BigInteger.genPseudoPrime (pbitlength, 80);
				if (e.gcd (p - 1) == 1)
					break;
			}
			// generate a modulus of the required length
			for (;;) {
				// generate q, prime and (q-1) relatively prime to e,
				// and not equal to p
				for (;;) {
					q = BigInteger.genPseudoPrime (qbitlength, 80);
					if ((e.gcd (q - 1) == 1) && (p != q)) 
						break;
				}
	
				// calculate the modulus
				n = p * q;
				if (n.bitCount () == KeySize)
					break;
	
				// if we get here our primes aren't big enough, make the largest
				// of the two p and try again
				p = p.max (q);
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
		}
	
		// Zeroize private key
		~RSACryptoServiceProvider() 
		{
			Dispose (false);
		}
	
		public override string KeyExchangeAlgorithm {
			get { return "RSA-PKCS1-KeyEx"; }
		}
	
		public override int KeySize {
			get { return n.bitCount(); }
		}
	
		[MonoTODO("Persistance")]
		public bool PersistKeyInCsp {
			get { return false;  }
			set { throw new NotSupportedException (); }
		}
	
		public override string SignatureAlgorithm {
			get { return "http://www.w3.org/2000/09/xmldsig#rsa-sha1"; }
		}
	
		public byte[] Decrypt (byte[] rgb, bool fOAEP) 
		{
			// choose between OAEP or PKCS#1 v.1.5 padding
			if (fOAEP) {
				SHA1 sha1 = SHA1.Create ();
				return PKCS1.Decrypt_OAEP (this, sha1, null);
			}
			else {
				return PKCS1.Decrypt_v15 (this, rgb);
			}
		}
	
		// NOTE: Unlike MS we need this method
		// LAMESPEC: Not available from MS .NET framework but MS don't tell
		// why! DON'T USE IT UNLESS YOU KNOW WHAT YOU ARE DOING!!! You should
		// only encrypt/decrypt session (secret) key using asymmetric keys. 
		// Using this method to decrypt data IS dangerous (and very slow).
		public override byte[] DecryptValue (byte[] rgb) 
		{
			// it would be stupid to decrypt data with a newly
			// generated keypair - so we return false
			if (!keypairGenerated)
				return null;
	
			BigInteger input = new BigInteger (rgb);
			BigInteger output = input.modPow (d, n);
			return output.getBytes ();
		}
	
		public byte[] Encrypt (byte[] rgb, bool fOAEP) 
		{
			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			// choose between OAEP or PKCS#1 v.1.5 padding
			if (fOAEP) {
				SHA1 sha1 = SHA1.Create ();
				return PKCS1.Encrypt_OAEP (this, sha1, rng, rgb);
			}
			else {
				return PKCS1.Encrypt_v15 (this, rng, rgb) ;
			}
		}
	
		// NOTE: Unlike MS we need this method
		// LAMESPEC: Not available from MS .NET framework but MS don't tell
		// why! DON'T USE IT UNLESS YOU KNOW WHAT YOU ARE DOING!!! You should
		// only encrypt/decrypt session (secret) key using asymmetric keys. 
		// Using this method to encrypt data IS dangerous (and very slow).
		public override byte[] EncryptValue (byte[] rgb) 
		{
			if (!keypairGenerated)
				GenerateKeyPair ();
	
			// TODO: With CRT
			// without CRT
			BigInteger input = new BigInteger (rgb);
			BigInteger output = input.modPow (e, n);
			return output.getBytes ();
		}
	
		public override RSAParameters ExportParameters (bool includePrivateParameters) 
		{
			if ((includePrivateParameters) && (!privateKeyExportable))
				throw new CryptographicException ("cannot export private key");
			if (!keypairGenerated)
				GenerateKeyPair ();
	
			RSAParameters param = new RSAParameters();
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
			if ((parameters.Exponent == null) || (parameters.Modulus == null))
				throw new CryptographicException ();
	
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
		}
	
		private HashAlgorithm GetHash (object halg) 
		{
			if (halg == null)
				throw new ArgumentNullException ();
	
			HashAlgorithm hash = null;
			if (halg is String)
				hash = HashAlgorithm.Create ((String)halg);
			else if (halg is HashAlgorithm)
				hash = (HashAlgorithm) halg;
			else if (halg is Type)
				hash = (HashAlgorithm) Activator.CreateInstance ((Type)halg);
			else
				throw new ArgumentException ();
	
			return hash;
		}
	
		public byte[] SignData (byte[] buffer, object halg) 
		{
			return SignData (buffer, 0, buffer.Length, halg);
		}
	
		public byte[] SignData (Stream inputStream, object halg) 
		{
			HashAlgorithm hash = GetHash (halg);
			byte[] toBeSigned = hash.ComputeHash (inputStream);
	
			string oid = CryptoConfig.MapNameToOID (hash.ToString ());
			return SignHash (toBeSigned, oid);
		}
	
		public byte[] SignData (byte[] buffer, int offset, int count, object halg) 
		{
			HashAlgorithm hash = GetHash (halg);
			byte[] toBeSigned = hash.ComputeHash (buffer, offset, count);
			string oid = CryptoConfig.MapNameToOID (hash.ToString ());
			return SignHash (toBeSigned, oid);
		}
	
		private void ValidateHash (string oid, int length) 
		{
			if (oid == "1.3.14.3.2.26") {
				if (length != 20)
					throw new CryptographicException ("wrong hash size for SHA1");
			}
			else if (oid == "1.2.840.113549.2.5") {
				if (length != 16)
					throw new CryptographicException ("wrong hash size for MD5");
			}
			else
				throw new NotSupportedException (oid + " is an unsupported hash algorithm for RSA signing");
		}
	
		public byte[] SignHash (byte[] rgbHash, string str) 
		{
			if (rgbHash == null)
				throw new ArgumentNullException ();
	
			if (!keypairGenerated)
				GenerateKeyPair ();
	
			ValidateHash (str, rgbHash.Length);
	
			return PKCS1.Sign_v15 (this, str, rgbHash);
		}
	
		public bool VerifyData (byte[] buffer, object halg, byte[] signature) 
		{
			HashAlgorithm hash = GetHash (halg);
			byte[] toBeVerified = hash.ComputeHash (buffer);
			string oid = CryptoConfig.MapNameToOID (hash.ToString ());
			return VerifyHash (toBeVerified, oid, signature);
		}
	
		public bool VerifyHash (byte[] rgbHash, string str, byte[] rgbSignature) 
		{
			if ((rgbHash == null) || (rgbSignature == null))
				throw new ArgumentNullException ();
	
			// it would be stupid to verify a signature with a newly
			// generated keypair - so we return false
			if (!keypairGenerated)
				return false;
	
			ValidateHash (str, rgbHash.Length);
	
			return PKCS1.Verify_v15 (this, str, rgbHash, rgbSignature);
		}
	
		[MonoTODO()]
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
	}
}
