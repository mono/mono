//
// System.Security.Cryptography.DSACryptoServiceProvider.cs
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
//	Sebastien Pouliot <sebastien@ximian.com>
//	Ben Maurer (bmaurer@users.sf.net)
//
// (C) 2002
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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
using System.Globalization;
using System.Runtime.InteropServices;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	[ComVisible (true)]
	public sealed class DSACryptoServiceProvider : DSA, ICspAsymmetricAlgorithm {
		private const int PROV_DSS_DH = 13;		// from WinCrypt.h

		private KeyPairPersistence store;
		private bool persistKey;
		private bool persisted;

		private bool privateKeyExportable = true;
		private bool m_disposed;

		private DSAManaged dsa;

		// MS implementation generates a keypair everytime a new DSA 
		// object is created (unless an existing key container is 
		// specified in the CspParameters).
		// However we:
		// (a) often use DSA to import an existing keypair.
		// (b) take a LOT of time to generate the DSA group
		// So we'll generate the keypair only when (and if) it's being
		// used (or exported). This should save us a lot of time (at
		// least in the unit tests).

		public DSACryptoServiceProvider ()
			: this (1024)
		{
		}

		public DSACryptoServiceProvider (CspParameters parameters)
			: this (1024, parameters)
		{
		}

		public DSACryptoServiceProvider (int dwKeySize)
		{
			Common (dwKeySize, false);
		}

		public DSACryptoServiceProvider (int dwKeySize, CspParameters parameters)
		{
			bool has_parameters = parameters != null;
			Common (dwKeySize, has_parameters);
			if (has_parameters)
				Common (parameters);
		}

		void Common (int dwKeySize, bool parameters) 
		{
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (512, 1024, 64);

			// will throw an exception is key size isn't supported
			KeySize = dwKeySize;
			dsa = new DSAManaged (dwKeySize);
			dsa.KeyGenerated += new DSAManaged.KeyGeneratedEventHandler (OnKeyGenerated);

			persistKey = parameters;
			if (parameters)
				return;

			var p = new CspParameters (PROV_DSS_DH);
			if (useMachineKeyStore)
				p.Flags |= CspProviderFlags.UseMachineKeyStore;
			store = new KeyPairPersistence (p);
			// no need to load - it cannot exists
		}

		void Common (CspParameters parameters)
		{
			store = new KeyPairPersistence (parameters);
			store.Load ();
			if (store.KeyValue != null) {
				persisted = true;
				this.FromXmlString (store.KeyValue);
			}
		}

		~DSACryptoServiceProvider ()
		{
			Dispose (false);
		}

		// DSA isn't used for key exchange
		public override string KeyExchangeAlgorithm {
			get { return null; }
		}

		public override int KeySize {
			get { return dsa.KeySize; }
		}

		public bool PersistKeyInCsp {
			get { return persistKey; }
			set { persistKey = value; }
		}

		[ComVisible (false)]
		public bool PublicOnly {
			get { return dsa.PublicOnly; }
		}

		public override string SignatureAlgorithm {
			get { return "http://www.w3.org/2000/09/xmldsig#dsa-sha1"; }
		}

		private static bool useMachineKeyStore;

		public static bool UseMachineKeyStore {
			get { return useMachineKeyStore; }
			set { useMachineKeyStore = value; }
		}

		public override DSAParameters ExportParameters (bool includePrivateParameters) 
		{
			if ((includePrivateParameters) && (!privateKeyExportable)) {
				throw new CryptographicException (
					Locale.GetText ("Cannot export private key"));
			}

			return dsa.ExportParameters (includePrivateParameters);
		}

		public override void ImportParameters (DSAParameters parameters) 
		{
			dsa.ImportParameters (parameters);
		}

		public override byte[] CreateSignature (byte[] rgbHash)
		{
			return dsa.CreateSignature (rgbHash);
		}

		public byte[] SignData (byte[] buffer)
		{
			// right now only SHA1 is supported by FIPS186-2
			HashAlgorithm hash = SHA1.Create ();
			byte[] toBeSigned = hash.ComputeHash (buffer);
			return dsa.CreateSignature (toBeSigned);
		}

		public byte[] SignData (byte[] buffer, int offset, int count)
		{
			// right now only SHA1 is supported by FIPS186-2
			HashAlgorithm hash = SHA1.Create ();
			byte[] toBeSigned = hash.ComputeHash (buffer, offset, count);
			return dsa.CreateSignature (toBeSigned);
		}

		public byte[] SignData (Stream inputStream)
		{
			// right now only SHA1 is supported by FIPS186-2
			HashAlgorithm hash = SHA1.Create ();
			byte[] toBeSigned = hash.ComputeHash (inputStream);
			return dsa.CreateSignature (toBeSigned);
		}

		public byte[] SignHash (byte[] rgbHash, string str)
		{
			// right now only SHA1 is supported by FIPS186-2
			if (String.Compare (str, "SHA1", true, CultureInfo.InvariantCulture) != 0) {
				// not documented
				throw new CryptographicException (Locale.GetText ("Only SHA1 is supported."));
			}

			return dsa.CreateSignature (rgbHash);
		}

		public bool VerifyData (byte[] rgbData, byte[] rgbSignature)
		{
			// right now only SHA1 is supported by FIPS186-2
			HashAlgorithm hash = SHA1.Create();
			byte[] toBeVerified = hash.ComputeHash (rgbData);
			return dsa.VerifySignature (toBeVerified, rgbSignature);
		}

		// LAMESPEC: MD5 isn't allowed with DSA
		public bool VerifyHash (byte[] rgbHash, string str, byte[] rgbSignature)
		{
			if (str == null)
				str = "SHA1"; // default value
			if (String.Compare (str, "SHA1", true, CultureInfo.InvariantCulture) != 0) {
				throw new CryptographicException (Locale.GetText ("Only SHA1 is supported."));
			}

			return dsa.VerifySignature (rgbHash, rgbSignature);
		}

		public override bool VerifySignature (byte[] rgbHash, byte[] rgbSignature)
		{
			return dsa.VerifySignature (rgbHash, rgbSignature);
		}

		protected override void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				// the key is persisted and we do not want it persisted
				if ((persisted) && (!persistKey)) {
					store.Remove ();	// delete the container
				}
				if (dsa != null)
					dsa.Clear ();
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
				store.KeyValue = this.ToXmlString (!dsa.PublicOnly);
				store.Save ();
				persisted = true;
			}
		}
		// ICspAsymmetricAlgorithm

		[MonoTODO ("call into KeyPairPersistence to get details")]
		[ComVisible (false)]
		public CspKeyContainerInfo CspKeyContainerInfo {
			get { return null; }
		}

		[ComVisible (false)]
		public byte[] ExportCspBlob (bool includePrivateParameters)
		{
			byte[] blob = null;
			if (includePrivateParameters)
				blob = CryptoConvert.ToCapiPrivateKeyBlob (this);
			else
				blob = CryptoConvert.ToCapiPublicKeyBlob (this);
			return blob;
		}

		[ComVisible (false)]
		public void ImportCspBlob (byte[] keyBlob)
		{
			if (keyBlob == null)
				throw new ArgumentNullException ("keyBlob");
			DSA dsa = CryptoConvert.FromCapiKeyBlobDSA (keyBlob);
			if (dsa is DSACryptoServiceProvider) {
				DSAParameters dsap = dsa.ExportParameters (!(dsa as DSACryptoServiceProvider).PublicOnly);
				ImportParameters (dsap);
			} else {
				// we can't know from DSA if the private key is available
				try {
					// so we try it...
					DSAParameters dsap = dsa.ExportParameters (true);
					ImportParameters (dsap);
				}
				catch {
					// and fall back
					DSAParameters dsap = dsa.ExportParameters (false);
					ImportParameters (dsap);
				}
			}
		}
	}
}
