//
// RSACryptoServiceProvider.cs: Handles an RSA implementation.
//
// Authors:
//	Sebastien Pouliot (spouliot@motus.com)
//	Ben Maurer (bmaurer@users.sf.net)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Portions (C) 2003 Ben Maurer
//

using System;
using System.IO;

using Mono.Math;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	public sealed class RSACryptoServiceProvider : RSA {
	
		private CspParameters cspParams;
	
		private bool privateKeyExportable = true; 
		private bool persistKey = false;
		private bool m_disposed = false;

		private RSAManaged rsa;
	
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
	
		[MonoTODO("Missing keypair persistance using CspParameters")]
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
		[MonoTODO("Missing keypair persistance using CspParameters")]
		public RSACryptoServiceProvider (int dwKeySize, CspParameters parameters) 
		{
			Common (dwKeySize, parameters);
			// no keypair generation done at this stage
		}
	
		// FIXME: We currently dont link with MS CAPI. Anyway this makes
		// only sense in Windows - what do we do elsewhere ?
		private void Common (int dwKeySize, CspParameters p) 
		{
			if (p == null) {
				cspParams = new CspParameters ();
#if ! NET_1_0
				if (useMachineKeyStore)
					cspParams.Flags |= CspProviderFlags.UseMachineKeyStore;
#endif
				// TODO: set default values (for keypair persistance)
			}
			else
				cspParams = p;
				// FIXME: We'll need this to support some kind of persistance

			// Microsoft RSA CSP can do between 384 and 16384 bits keypair
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (384, 16384, 8);
			base.KeySize = dwKeySize;

			rsa = new RSAManaged (KeySize);
		}

#if ! NET_1_0
		private static bool useMachineKeyStore = false;

		[MonoTODO("Related to keypair persistance")]
		public static bool UseMachineKeyStore {
			get { return useMachineKeyStore; }
			set { useMachineKeyStore = value; }
		}
#endif
	
		~RSACryptoServiceProvider () 
		{
			// Zeroize private key
			Dispose (false);
		}
	
		public override string KeyExchangeAlgorithm {
			get { return "RSA-PKCS1-KeyEx"; }
		}
	
		public override int KeySize {
			get { 
				if (rsa == null)
				      return KeySizeValue; 
				else
				      return rsa.KeySize;
			}
		}
	
		[MonoTODO("Related to keypair persistance")]
		public bool PersistKeyInCsp {
			get { return persistKey;  }
			set { persistKey = value; }
		}
	
		public override string SignatureAlgorithm {
			get { return "http://www.w3.org/2000/09/xmldsig#rsa-sha1"; }
		}
	
		public byte[] Decrypt (byte[] rgb, bool fOAEP) 
		{
#if NET_1_1
			if (m_disposed)
				throw new ObjectDisposedException ("rsa");
#endif
			// choose between OAEP or PKCS#1 v.1.5 padding
			AsymmetricKeyExchangeDeformatter def = null;
			if (fOAEP)
				def = new RSAOAEPKeyExchangeDeformatter (rsa);
			else
				def = new RSAPKCS1KeyExchangeDeformatter (rsa);

			return def.DecryptKeyExchange (rgb);
		}
	
		// NOTE: Unlike MS we need this method
		// LAMESPEC: Not available from MS .NET framework but MS don't tell
		// why! DON'T USE IT UNLESS YOU KNOW WHAT YOU ARE DOING!!! You should
		// only encrypt/decrypt session (secret) key using asymmetric keys. 
		// Using this method to decrypt data IS dangerous (and very slow).
		public override byte[] DecryptValue (byte[] rgb) 
		{
			return rsa.DecryptValue (rgb);
		}
	
		public byte[] Encrypt (byte[] rgb, bool fOAEP) 
		{
			// choose between OAEP or PKCS#1 v.1.5 padding
			AsymmetricKeyExchangeFormatter fmt = null;
			if (fOAEP)
				fmt = new RSAOAEPKeyExchangeFormatter (rsa);
			else
				fmt = new RSAPKCS1KeyExchangeFormatter (rsa);

			return fmt.CreateKeyExchange (rgb);
		}
	
		// NOTE: Unlike MS we need this method
		// LAMESPEC: Not available from MS .NET framework but MS don't tell
		// why! DON'T USE IT UNLESS YOU KNOW WHAT YOU ARE DOING!!! You should
		// only encrypt/decrypt session (secret) key using asymmetric keys. 
		// Using this method to encrypt data IS dangerous (and very slow).
		public override byte[] EncryptValue (byte[] rgb) 
		{
			return rsa.EncryptValue (rgb);
		}
	
		public override RSAParameters ExportParameters (bool includePrivateParameters) 
		{
			if ((includePrivateParameters) && (!privateKeyExportable))
				throw new CryptographicException ("cannot export private key");

			return rsa.ExportParameters (includePrivateParameters);
		}
	
		public override void ImportParameters (RSAParameters parameters) 
		{
			rsa.ImportParameters (parameters);
		}
	
		private HashAlgorithm GetHash (object halg) 
		{
			if (halg == null)
				throw new ArgumentNullException ("halg");

			HashAlgorithm hash = null;
			if (halg is String)
				hash = HashAlgorithm.Create ((String)halg);
			else if (halg is HashAlgorithm)
				hash = (HashAlgorithm) halg;
			else if (halg is Type)
				hash = (HashAlgorithm) Activator.CreateInstance ((Type)halg);
			else
				throw new ArgumentException ("halg");

			return hash;
		}
	
		// NOTE: this method can work with ANY configured (OID in machine.config) 
		// HashAlgorithm descendant
		public byte[] SignData (byte[] buffer, object halg) 
		{
#if NET_1_1
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
#endif
			return SignData (buffer, 0, buffer.Length, halg);
		}
	
		// NOTE: this method can work with ANY configured (OID in machine.config) 
		// HashAlgorithm descendant
		public byte[] SignData (Stream inputStream, object halg) 
		{
			HashAlgorithm hash = GetHash (halg);
			byte[] toBeSigned = hash.ComputeHash (inputStream);
			return PKCS1.Sign_v15 (this, hash, toBeSigned);
		}
	
		// NOTE: this method can work with ANY configured (OID in machine.config) 
		// HashAlgorithm descendant
		public byte[] SignData (byte[] buffer, int offset, int count, object halg) 
		{
			HashAlgorithm hash = GetHash (halg);
			byte[] toBeSigned = hash.ComputeHash (buffer, offset, count);
			return PKCS1.Sign_v15 (this, hash, toBeSigned);
		}
	
		private string GetHashNameFromOID (string oid) 
		{
			switch (oid) {
				case "1.3.14.3.2.26":
					return "SHA1";
				case "1.2.840.113549.2.5":
					return "MD5";
				default:
					throw new NotSupportedException (oid + " is an unsupported hash algorithm for RSA signing");
			}
		}

		// LAMESPEC: str is not the hash name but an OID
		// NOTE: this method is LIMITED to SHA1 and MD5 like the MS framework 1.0 
		// and 1.1 because there's no mathod to get a hash algorithm from an OID. 
		// However there's no such limit when using the [De]Formatter class.
		public byte[] SignHash (byte[] rgbHash, string str) 
		{
			if (rgbHash == null)
				throw new ArgumentNullException ("rgbHash");
	
			HashAlgorithm hash = HashAlgorithm.Create (GetHashNameFromOID (str));
			return PKCS1.Sign_v15 (this, hash, rgbHash);
		}

		// NOTE: this method can work with ANY configured (OID in machine.config) 
		// HashAlgorithm descendant
		public bool VerifyData (byte[] buffer, object halg, byte[] signature) 
		{
#if NET_1_1
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
#endif
			if (signature == null)
				throw new ArgumentNullException ("signature");

			HashAlgorithm hash = GetHash (halg);
			byte[] toBeVerified = hash.ComputeHash (buffer);
			return PKCS1.Verify_v15 (this, hash, toBeVerified, signature);
		}
	
		// LAMESPEC: str is not the hash name but an OID
		// NOTE: this method is LIMITED to SHA1 and MD5 like the MS framework 1.0 
		// and 1.1 because there's no mathod to get a hash algorithm from an OID. 
		// However there's no such limit when using the [De]Formatter class.
		public bool VerifyHash (byte[] rgbHash, string str, byte[] rgbSignature) 
		{
			if (rgbHash == null) 
				throw new ArgumentNullException ("rgbHash");
			if (rgbSignature == null)
				throw new ArgumentNullException ("rgbSignature");
	
			HashAlgorithm hash = HashAlgorithm.Create (GetHashNameFromOID (str));
			return PKCS1.Verify_v15 (this, hash, rgbHash, rgbSignature);
		}
	
		protected override void Dispose (bool disposing) 
		{
			if (rsa != null)
				rsa.Clear ();
			// call base class 
			// no need as they all are abstract before us
			m_disposed = true;
		}
	}
}
