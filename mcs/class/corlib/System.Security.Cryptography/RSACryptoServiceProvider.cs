//
// RSACryptoServiceProvider.cs: Handles an RSA implementation.
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Ben Maurer (bmaurer@users.sf.net)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Portions (C) 2003 Ben Maurer
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Runtime.InteropServices;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	public partial class RSACryptoServiceProvider {
		private const int PROV_RSA_FULL = 1;	// from WinCrypt.h
		private const int AT_KEYEXCHANGE = 1;
		private const int AT_SIGNATURE = 2;

		private KeyPairPersistence store;
		private bool persistKey;
		private bool persisted;
	
		private bool privateKeyExportable = true; 
		private bool m_disposed;

		private RSAManaged rsa;
	
		public RSACryptoServiceProvider ()
			: this (1024)
		{
			// Here it's not clear if we need to generate a keypair
			// (note: MS implementation generates a keypair in this case).
			// However we:
			// (a) often use this constructor to import an existing keypair.
			// (b) take a LOT of time to generate the RSA keypair
			// So we'll generate the keypair only when (and if) it's being
			// used (or exported). This should save us a lot of time (at 
			// least in the unit tests).
		}
	
		public RSACryptoServiceProvider (CspParameters parameters) 
			: this (1024, parameters)
		{
			// no keypair generation done at this stage
		}
	
		public RSACryptoServiceProvider (int dwKeySize) 
		{
			// Here it's clear that we need to generate a new keypair
			Common (dwKeySize, false);
			// no keypair generation done at this stage
		}
	
		public RSACryptoServiceProvider (int dwKeySize, CspParameters parameters) 
		{
			bool has_parameters = parameters != null;
			Common (dwKeySize, has_parameters);
			if (has_parameters)
				Common (parameters);
			// no keypair generation done at this stage
		}
	
		void Common (int dwKeySize, bool parameters) 
		{
			// Microsoft RSA CSP can do between 384 and 16384 bits keypair
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (384, 16384, 8);
			base.KeySize = dwKeySize;

			rsa = new RSAManaged (KeySize);
			rsa.KeyGenerated += new RSAManaged.KeyGeneratedEventHandler (OnKeyGenerated);

			persistKey = parameters;
			if (parameters)
				return;

			// no need to load - it cannot exists
			var p = new CspParameters (PROV_RSA_FULL);
			if (UseMachineKeyStore)
				p.Flags |= CspProviderFlags.UseMachineKeyStore;
			store = new KeyPairPersistence (p);
		}

		void Common (CspParameters p)
		{
			store = new KeyPairPersistence (p);
			bool exists = store.Load ();
			bool required = (p.Flags & CspProviderFlags.UseExistingKey) != 0;
			privateKeyExportable = (p.Flags & CspProviderFlags.UseNonExportableKey) == 0;

			if (required && !exists)
				throw new CryptographicException ("Keyset does not exist");

			if (store.KeyValue != null) {
				persisted = true;
				FromXmlString (store.KeyValue);
			}
		}
	
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

		public bool PersistKeyInCsp {
			get { return persistKey; }
			set {
				persistKey = value;
				if (persistKey)
					OnKeyGenerated (rsa, null);
			}
		}

		[ComVisible (false)]
		public bool PublicOnly {
			get { return rsa.PublicOnly; }
		}

		public byte[] Decrypt (byte[] rgb, bool fOAEP) 
		{
			if (rgb == null)
				throw new ArgumentNullException("rgb");

			// size check -- must be at most the modulus size
			if (rgb.Length > (KeySize / 8))
				throw new CryptographicException(Environment.GetResourceString("Cryptography_Padding_DecDataTooBig", KeySize / 8));
			
			if (m_disposed)
				throw new ObjectDisposedException ("rsa");
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
			if (!rsa.IsCrtPossible)
				throw new CryptographicException ("Incomplete private key - missing CRT.");

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

			var rsaParams = rsa.ExportParameters (includePrivateParameters);
			if (includePrivateParameters) {
				// we want an ArgumentNullException is only the D is missing, but a
				// CryptographicException if other parameters (CRT) are missings
				if (rsaParams.D == null) {
					throw new ArgumentNullException ("Missing D parameter for the private key.");
				} else if ((rsaParams.P == null) || (rsaParams.Q == null) || (rsaParams.DP == null) ||
					(rsaParams.DQ == null) || (rsaParams.InverseQ == null)) {
					// note: we can import a private key, using FromXmlString,
					// without the CRT parameters but we export it using ToXmlString!
					throw new CryptographicException ("Missing some CRT parameters for the private key.");
				}
			}

			return rsaParams;
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
				hash = GetHashFromString ((string) halg);
			else if (halg is HashAlgorithm)
				hash = (HashAlgorithm) halg;
			else if (halg is Type)
				hash = (HashAlgorithm) Activator.CreateInstance ((Type)halg);
			else
				throw new ArgumentException ("halg");

			if (hash == null)
				throw new ArgumentException (
						"Could not find provider for halg='" + halg + "'.",
						"halg");

			return hash;
		}

		private HashAlgorithm GetHashFromString (string name)
		{
			HashAlgorithm hash = HashAlgorithm.Create (name);
			if (hash != null)
				return hash;
			try {
				return HashAlgorithm.Create (GetHashNameFromOID (name));
			} catch (CryptographicException e) {
				throw new ArgumentException (e.Message, "halg", e);
			}
		}
	
		// NOTE: this method can work with ANY configured (OID in machine.config) 
		// HashAlgorithm descendant
		public byte[] SignData (byte[] buffer, object halg) 
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
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
			case "2.16.840.1.101.3.4.2.1":
				return "SHA256";
			case "2.16.840.1.101.3.4.2.2":
				return "SHA384";
			case "2.16.840.1.101.3.4.2.3":
				return "SHA512";
			default:
				throw new CryptographicException (oid + " is an unsupported hash algorithm for RSA signing");
			}
		}

		public byte[] SignHash (byte[] rgbHash, string str) 
		{
			if (rgbHash == null)
				throw new ArgumentNullException ("rgbHash");
			// Fx 2.0 defaults to the SHA-1
			string hashName = (str == null) ? "SHA1" : GetHashNameFromOID (str);
			HashAlgorithm hash = HashAlgorithm.Create (hashName);
			return PKCS1.Sign_v15 (this, hash, rgbHash);
		}

		byte[] SignHash(byte[] rgbHash, int calgHash)
		{
			return PKCS1.Sign_v15 (this, InternalHashToHashAlgorithm (calgHash), rgbHash);
		}

		static HashAlgorithm InternalHashToHashAlgorithm (int calgHash)
		{
			switch (calgHash) {
			case Constants.CALG_MD5:
				return MD5.Create ();
			case Constants.CALG_SHA1:
				return SHA1.Create ();
			case Constants.CALG_SHA_256:
				return SHA256.Create ();
			case Constants.CALG_SHA_384:
				return SHA384.Create ();
			case Constants.CALG_SHA_512:
				return SHA512.Create ();
			}

			throw new NotImplementedException (calgHash.ToString ());
		}

		// NOTE: this method can work with ANY configured (OID in machine.config) 
		// HashAlgorithm descendant
		public bool VerifyData (byte[] buffer, object halg, byte[] signature) 
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (signature == null)
				throw new ArgumentNullException ("signature");

			HashAlgorithm hash = GetHash (halg);
			byte[] toBeVerified = hash.ComputeHash (buffer);
			return PKCS1.Verify_v15 (this, hash, toBeVerified, signature);
		}
	
		public bool VerifyHash (byte[] rgbHash, string str, byte[] rgbSignature) 
		{
			if (rgbHash == null) 
				throw new ArgumentNullException ("rgbHash");
			if (rgbSignature == null)
				throw new ArgumentNullException ("rgbSignature");
			// Fx 2.0 defaults to the SHA-1
			string hashName = (str == null) ? "SHA1" : GetHashNameFromOID (str);
			HashAlgorithm hash = HashAlgorithm.Create (hashName);
			return PKCS1.Verify_v15 (this, hash, rgbHash, rgbSignature);
		}

		bool VerifyHash(byte[] rgbHash, int calgHash, byte[] rgbSignature)
		{
			return PKCS1.Verify_v15 (this, InternalHashToHashAlgorithm (calgHash), rgbHash, rgbSignature);
		}
	
		protected override void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				// the key is persisted and we do not want it persisted
				if ((persisted) && (!persistKey)) {
					store.Remove ();	// delete the container
				}
				if (rsa != null)
					rsa.Clear ();
				// call base class 
				// no need as they all are abstract before us
				m_disposed = true;
			}
		}

		// private stuff

		private void OnKeyGenerated (object sender, EventArgs e) 
		{
			// the key isn't persisted and we want it persisted
			if ((persistKey) && (!persisted)) {
				// save the current keypair
				store.KeyValue = this.ToXmlString (!rsa.PublicOnly);
				store.Save ();
				persisted = true;
			}
		}
		// ICspAsymmetricAlgorithm

		[ComVisible (false)]
		public CspKeyContainerInfo CspKeyContainerInfo {
			get {
				return new CspKeyContainerInfo(store.Parameters);
			}
		}

		[ComVisible (false)]
		public byte[] ExportCspBlob (bool includePrivateParameters)
		{
			byte[] blob = null;
			if (includePrivateParameters)
				blob = CryptoConvert.ToCapiPrivateKeyBlob (this);
			else
				blob = CryptoConvert.ToCapiPublicKeyBlob (this);

			// ALGID (bytes 4-7) - default is KEYX
			// 00 24 00 00 (for CALG_RSA_SIGN)
			// 00 A4 00 00 (for CALG_RSA_KEYX)
			blob [5] = (byte) (((store != null) && (store.Parameters.KeyNumber == AT_SIGNATURE)) ? 0x24 : 0xA4);
			return blob;
		}

		[ComVisible (false)]
		public void ImportCspBlob (byte[] keyBlob)
		{
			if (keyBlob == null)
				throw new ArgumentNullException ("keyBlob");

			RSA rsa = CryptoConvert.FromCapiKeyBlob (keyBlob);
			if (rsa is RSACryptoServiceProvider) {
				// default (if no change are present in machine.config)
				RSAParameters rsap = rsa.ExportParameters (!(rsa as RSACryptoServiceProvider).PublicOnly);
				ImportParameters (rsap);
			} else {
				// we can't know from RSA if the private key is available
				try {
					// so we try it...
					RSAParameters rsap = rsa.ExportParameters (true);
					ImportParameters (rsap);
				}
				catch {
					// and fall back
					RSAParameters rsap = rsa.ExportParameters (false);
					ImportParameters (rsap);
				}
			}

			var p = new CspParameters (PROV_RSA_FULL);
			p.KeyNumber = keyBlob [5] == 0x24 ? AT_SIGNATURE : AT_KEYEXCHANGE;
			if (UseMachineKeyStore)
				p.Flags |= CspProviderFlags.UseMachineKeyStore;
			store = new KeyPairPersistence (p);
		}
	}
}
