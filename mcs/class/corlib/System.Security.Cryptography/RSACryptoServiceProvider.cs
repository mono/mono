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

#if !MOONLIGHT

using System.IO;
using System.Runtime.InteropServices;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	[ComVisible (true)]
	public sealed class RSACryptoServiceProvider : RSA, ICspAsymmetricAlgorithm {
		private const int PROV_RSA_FULL = 1;	// from WinCrypt.h

		private KeyPairPersistence store;
		private bool persistKey;
		private bool persisted;
	
		private bool privateKeyExportable = true; 
		private bool m_disposed;

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
	
		public RSACryptoServiceProvider (int dwKeySize, CspParameters parameters) 
		{
			Common (dwKeySize, parameters);
			// no keypair generation done at this stage
		}
	
		private void Common (int dwKeySize, CspParameters p) 
		{
			// Microsoft RSA CSP can do between 384 and 16384 bits keypair
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (384, 16384, 8);
			base.KeySize = dwKeySize;

			rsa = new RSAManaged (KeySize);
			rsa.KeyGenerated += new RSAManaged.KeyGeneratedEventHandler (OnKeyGenerated);

			persistKey = (p != null);
			if (p == null) {
				p = new CspParameters (PROV_RSA_FULL);
#if NET_1_1
				if (useMachineKeyStore)
					p.Flags |= CspProviderFlags.UseMachineKeyStore;
#endif
				store = new KeyPairPersistence (p);
				// no need to load - it cannot exists
			}
			else {
				store = new KeyPairPersistence (p);
				bool exists = store.Load ();
				bool required = (p.Flags & CspProviderFlags.UseExistingKey) != 0;

				if (required && !exists)
					throw new CryptographicException ("Keyset does not exist");

				if (store.KeyValue != null) {
					persisted = true;
					this.FromXmlString (store.KeyValue);
				}
			}
		}

#if NET_1_1
		private static bool useMachineKeyStore = false;

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
		// and 1.1 because there's no method to get a hash algorithm from an OID. 
		// However there's no such limit when using the [De]Formatter class.
		public byte[] SignHash (byte[] rgbHash, string str) 
		{
			if (rgbHash == null)
				throw new ArgumentNullException ("rgbHash");
			// Fx 2.0 defaults to the SHA-1
			string hashName = (str == null) ? "SHA1" : GetHashNameFromOID (str);
			HashAlgorithm hash = HashAlgorithm.Create (hashName);
			return PKCS1.Sign_v15 (this, hash, rgbHash);
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
	
		// LAMESPEC: str is not the hash name but an OID
		// NOTE: this method is LIMITED to SHA1 and MD5 like the MS framework 1.0 
		// and 1.1 because there's no method to get a hash algorithm from an OID. 
		// However there's no such limit when using the [De]Formatter class.
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
			blob [5] = 0xA4;
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
		}
	}
}

#endif

