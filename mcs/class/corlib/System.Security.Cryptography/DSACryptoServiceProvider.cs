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
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

#if NET_1_0
 	public class DSACryptoServiceProvider : DSA {
#else
	public sealed class DSACryptoServiceProvider : DSA {
#endif
		private const int PROV_DSS = 2;		// from WinCrypt.h

		private KeyPairPersistence store;
		private bool persistKey = true;
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

		public DSACryptoServiceProvider () : this (1024, null) {}

		public DSACryptoServiceProvider (CspParameters parameters) : this (1024, parameters) {}

		public DSACryptoServiceProvider (int dwKeySize) : this (dwKeySize, null) {}

		public DSACryptoServiceProvider (int dwKeySize, CspParameters parameters)
		{
			if (parameters == null) {
				parameters = new CspParameters (PROV_DSS);
#if ! NET_1_0
				if (useMachineKeyStore)
					parameters.Flags |= CspProviderFlags.UseMachineKeyStore;
#endif
				store = new KeyPairPersistence (parameters);
				// no need to load - it cannot exists
			}
			else {
				store = new KeyPairPersistence (parameters);
				store.Load ();
				if (store.KeyValue != null) {
					persisted = true;
					this.FromXmlString (store.KeyValue);
				}
			}

			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (512, 1024, 64);

			// will throw an exception is key size isn't supported
			KeySize = dwKeySize;
			dsa = new DSAManaged (dwKeySize);
			dsa.KeyGenerated += new DSAManaged.KeyGeneratedEventHandler (OnKeyGenerated);
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

		public override KeySizes[] LegalKeySizes {
			get { return LegalKeySizesValue; }
		}

		public bool PersistKeyInCsp {
			get { return persistKey; }
			set {
				if (value) {
					OnKeyGenerated (dsa);
				}
				else {
					// delete the container
					store.Remove ();
				}
				persistKey = value;
			}
		}

#if (NET_1_0 || NET_1_1)
		internal
#else
		public 
#endif
		bool PublicOnly {
			get { return dsa.PublicOnly; }
		}

		public override string SignatureAlgorithm {
			get { return "http://www.w3.org/2000/09/xmldsig#dsa-sha1"; }
		}

#if ! NET_1_0
		private static bool useMachineKeyStore = false;

		public static bool UseMachineKeyStore {
			get { return useMachineKeyStore; }
			set { useMachineKeyStore = value; }
		}
#endif

		public override DSAParameters ExportParameters (bool includePrivateParameters) 
		{
			if ((includePrivateParameters) && (!privateKeyExportable))
				throw new CryptographicException ("cannot export private key");

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

		public byte[] SignData (byte[] data)
		{
			return dsa.CreateSignature (data);
		}

		public byte[] SignData (byte[] data, int offset, int count)
		{
			// right now only SHA1 is supported by FIPS186-2
			HashAlgorithm hash = SHA1.Create ();
			byte[] toBeSigned = hash.ComputeHash (data, offset, count);
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
			if (str.ToUpper () != "SHA1")
				throw new Exception (); // not documented
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
			if (str != "SHA1")
				throw new CryptographicException ();
			return dsa.VerifySignature (rgbHash, rgbSignature);
		}

		public override bool VerifySignature (byte[] rgbHash, byte[] rgbSignature)
		{
			return dsa.VerifySignature (rgbHash, rgbSignature);
		}

		protected override void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				if (dsa != null)
					dsa.Clear ();
				// call base class 
				// no need as they all are abstract before us
				m_disposed = true;
			}
		}

		// private stuff

		private void OnKeyGenerated (object sender) 
		{
			if ((persistKey) && (!persisted)) {
				// save the current keypair
				store.KeyValue = this.ToXmlString (!dsa.PublicOnly);
				store.Save ();
				persisted = true;
			}
		}
	}
}
