//
// PKCS12.cs: PKCS 12 - Personal Information Exchange Syntax
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Key derivation translated from Bouncy Castle JCE (http://www.bouncycastle.org/)
// See bouncycastle.txt for license.
//

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Mono.Security;
using Mono.Security.Cryptography;

namespace Mono.Security.X509 {

#if INSIDE_CORLIB
	internal
#else
	public 
#endif
	class PKCS5 {

		public const string pbeWithMD2AndDESCBC = "1.2.840.113549.1.5.1";
		public const string pbeWithMD5AndDESCBC = "1.2.840.113549.1.5.3";
		public const string pbeWithMD2AndRC2CBC = "1.2.840.113549.1.5.4";
		public const string pbeWithMD5AndRC2CBC = "1.2.840.113549.1.5.6";
		public const string pbeWithSHA1AndDESCBC = "1.2.840.113549.1.5.10";
		public const string pbeWithSHA1AndRC2CBC = "1.2.840.113549.1.5.11";

		public PKCS5 () {}
	}

#if INSIDE_CORLIB
	internal
#else
	public 
#endif
	class PKCS12 {

		public const string pbeWithSHAAnd128BitRC4 = "1.2.840.113549.1.12.1.1";
		public const string pbeWithSHAAnd40BitRC4 = "1.2.840.113549.1.12.1.2";
		public const string pbeWithSHAAnd3KeyTripleDESCBC = "1.2.840.113549.1.12.1.3";
		public const string pbeWithSHAAnd2KeyTripleDESCBC = "1.2.840.113549.1.12.1.4";
		public const string pbeWithSHAAnd128BitRC2CBC = "1.2.840.113549.1.12.1.5";
		public const string pbeWithSHAAnd40BitRC2CBC = "1.2.840.113549.1.12.1.6";

		// bags
		public const string keyBag  = "1.2.840.113549.1.12.10.1.1";
		public const string pkcs8ShroudedKeyBag  = "1.2.840.113549.1.12.10.1.2";
		public const string certBag  = "1.2.840.113549.1.12.10.1.3";
		public const string crlBag  = "1.2.840.113549.1.12.10.1.4";
		public const string secretBag  = "1.2.840.113549.1.12.10.1.5";
		public const string safeContentsBag  = "1.2.840.113549.1.12.10.1.6";

		// types
		public const string x509Certificate = "1.2.840.113549.1.9.22.1";
 		public const string sdsiCertificate = "1.2.840.113549.1.9.22.2";
		public const string x509Crl = "1.2.840.113549.1.9.23.1";

		// Adapted from BouncyCastle PKCS12ParametersGenerator.java
		public class DeriveBytes {

			public enum Purpose {
				Key,
				IV,
				MAC
			}

			static private byte[] keyDiversifier = { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 };
			static private byte[] ivDiversifier  = { 2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2 };
			static private byte[] macDiversifier = { 3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3 };

			private string _hashName;
			private int _iterations;
			private byte[] _password;
			private byte[] _salt;

			public DeriveBytes () {}

			public string HashName {
				get { return _hashName; } 
				set { _hashName = value; }
			}

			public int IterationCount {
				get { return _iterations; }
				set { _iterations = value; }
			}

			public byte[] Password {
				get { return (byte[]) _password.Clone (); }
				set { 
					if (value == null)
						_password = new byte [0];
					else
						_password = (byte[]) value.Clone ();
				}
			}

			public byte[] Salt {
				get { return (byte[]) _salt.Clone ();  }
				set {
					if (value != null)
						_salt = (byte[]) value.Clone ();
					else
						_salt = null;
				}
			}

			private void Adjust (byte[] a, int aOff, byte[] b) 
			{
				int x = (b[b.Length - 1] & 0xff) + (a [aOff + b.Length - 1] & 0xff) + 1;

				a [aOff + b.Length - 1] = (byte) x;
				x >>= 8;

				for (int i = b.Length - 2; i >= 0; i--) {
					x += (b [i] & 0xff) + (a [aOff + i] & 0xff);
					a [aOff + i] = (byte) x;
					x >>= 8;
				}
			}

			private byte[] Derive (byte[] diversifier, int n) 
			{
				HashAlgorithm digest = HashAlgorithm.Create (_hashName);
				int u = (digest.HashSize >> 3); // div 8
				int v = 64;
				byte[] dKey = new byte [n];

				byte[] S;
				if ((_salt != null) && (_salt.Length != 0)) {
					S = new byte[v * ((_salt.Length + v - 1) / v)];

					for (int i = 0; i != S.Length; i++) {
						S[i] = _salt[i % _salt.Length];
					}
				}
				else {
					S = new byte[0];
				}

				byte[] P;
				if ((_password != null) && (_password.Length != 0)) {
					P = new byte[v * ((_password.Length + v - 1) / v)];

					for (int i = 0; i != P.Length; i++) {
						P[i] = _password[i % _password.Length];
					}
				}
				else {
					P = new byte[0];
				}

				byte[] I = new byte [S.Length + P.Length];

				Buffer.BlockCopy (S, 0, I, 0, S.Length);
				Buffer.BlockCopy (P, 0, I, S.Length, P.Length);

				byte[]  B = new byte[v];
				int     c = (n + u - 1) / u;

				for (int i = 1; i <= c; i++) {
					digest.TransformBlock (diversifier, 0, diversifier.Length, diversifier, 0);
					digest.TransformFinalBlock (I, 0, I.Length);
					byte[] A = digest.Hash;
					digest.Initialize ();
					for (int j = 1; j != _iterations; j++) {
						A = digest.ComputeHash (A, 0, A.Length);
					}

					for (int j = 0; j != B.Length; j++) {
						B [j] = A [j % A.Length];
					}

					for (int j = 0; j != I.Length / v; j++) {
						Adjust (I, j * v, B);
					}

					if (i == c) {
						Buffer.BlockCopy(A, 0, dKey, (i - 1) * u, dKey.Length - ((i - 1) * u));
					}
					else {
						Buffer.BlockCopy(A, 0, dKey, (i - 1) * u, A.Length);
					}
				}

				return dKey;
			}

			public byte[] DeriveKey (int size) 
			{
				return Derive (keyDiversifier, size);
			}

			public byte[] DeriveIV (int size) 
			{
				return Derive (ivDiversifier, size);
			}

			public byte[] DeriveMAC (int size) 
			{
				return Derive (macDiversifier, size);
			}
		}

		static private int recommendedIterationCount = 2000;

		private int _version;
		private byte[] _password;
		private ArrayList _keyBags;
		private X509CertificateCollection _certs;
		private int _iterations;
		private RandomNumberGenerator _rng;

		// constructors

		public PKCS12 () 
		{
			_iterations = recommendedIterationCount;
			_keyBags = new ArrayList ();
			_certs = new X509CertificateCollection ();
		}

		public PKCS12 (byte[] data) : this (data, null) {}

		/*
		 * PFX ::= SEQUENCE {
		 *	version INTEGER {v3(3)}(v3,...),
		 *	authSafe ContentInfo,
		 *	macData MacData OPTIONAL
		 * }
		 * 
		 * MacData ::= SEQUENCE {
		 *	mac DigestInfo,
		 *	macSalt OCTET STRING,
		 *	iterations INTEGER DEFAULT 1
		 *	-- Note: The default is for historical reasons and its use is deprecated. A higher
		 *	-- value, like 1024 is recommended.
		 * }
		 * 
		 * SafeContents ::= SEQUENCE OF SafeBag
		 * 
		 * SafeBag ::= SEQUENCE {
		 *	bagId BAG-TYPE.&id ({PKCS12BagSet}),
		 *	bagValue [0] EXPLICIT BAG-TYPE.&Type({PKCS12BagSet}{@bagId}),
		 *	bagAttributes SET OF PKCS12Attribute OPTIONAL
		 * }
		 */
		public PKCS12 (byte[] data, string password) : this ()
		{
			Password = password;

			ASN1 pfx = new ASN1 (data);
			if (pfx.Tag != 0x30)
				throw new ArgumentException ("invalid data");
			
			ASN1 version = pfx [0];
			if (version.Tag != 0x02)
				throw new ArgumentException ("invalid PFX version");
			_version = version.Value [0];

			PKCS7.ContentInfo authSafe = new PKCS7.ContentInfo (pfx [1]);
			if (authSafe.ContentType != PKCS7.data)
				throw new ArgumentException ("invalid authenticated safe");

			// now that we know it's a PKCS#12 file, check the (optional) MAC
			// before decoding anything else in the file
			if (pfx.Count > 2) {
				ASN1 macData = pfx [2];
				if (macData.Tag != 0x30)
					throw new ArgumentException ("invalid MAC");
				
				ASN1 mac = macData [0];
				if (mac.Tag != 0x30)
					throw new ArgumentException ("invalid MAC");
				ASN1 macAlgorithm = mac [0];
				string macOid = ASN1Convert.ToOID (macAlgorithm [0]);
				if (macOid != "1.3.14.3.2.26")
					throw new ArgumentException ("unsupported HMAC");
				byte[] macValue = mac [1].Value;

				ASN1 macSalt = macData [1];
				if (macSalt.Tag != 0x04)
					throw new ArgumentException ("missing MAC salt");

				_iterations = 1; // default value
				if (macData.Count > 2) {
					ASN1 iters = macData [2];
					if (iters.Tag != 0x02)
						throw new ArgumentException ("invalid MAC iteration");
					_iterations = ASN1Convert.ToInt32 (iters);
				}

				byte[] authSafeData = authSafe.Content [0].Value;
				byte[] calculatedMac = MAC (_password, macSalt.Value, _iterations, authSafeData);
				if (!Compare (macValue, calculatedMac))
					throw new CryptographicException ("Invalid MAC - file may have been tampered!");
			}

			// we now returns to our original presentation - PFX
			ASN1 authenticatedSafe = new ASN1 (authSafe.Content [0].Value);
			for (int i=0; i < authenticatedSafe.Count; i++) {
				PKCS7.ContentInfo ci = new PKCS7.ContentInfo (authenticatedSafe [i]);
				switch (ci.ContentType) {
					case PKCS7.data:
						// unencrypted (by PKCS#12)
						ASN1 safeContents = new ASN1 (ci.Content [0].Value);
						for (int j=0; j < safeContents.Count; j++) {
							ASN1 safeBag = safeContents [j];
							ReadSafeBag (safeBag);
						}
						break;
					case PKCS7.encryptedData:
						// password encrypted
						PKCS7.EncryptedData ed = new PKCS7.EncryptedData (ci.Content [0]);
						ASN1 decrypted = new ASN1 (Decrypt (ed));
						for (int j=0; j < decrypted.Count; j++) {
							ASN1 safeBag = decrypted [j];
							ReadSafeBag (safeBag);
						}
						break;
					case PKCS7.envelopedData:
						// public key encrypted
						throw new NotImplementedException ("public key encrypted");
					default:
						throw new ArgumentException ("unknown authenticatedSafe");
				}
			}
		}

		~PKCS12 () 
		{
			if (_password != null) {
				Array.Clear (_password, 0, _password.Length);
			}
		}

		// properties

		public string Password {
			set {
				if (value != null) {
					if (value.EndsWith ("\0"))
						_password = Encoding.BigEndianUnicode.GetBytes (value);	
					else
						_password = Encoding.BigEndianUnicode.GetBytes (value + "\0");					
				}
				else
					_password = null;	// no password
			}
		}

		public int IterationCount {
			get { return _iterations; }
			set { _iterations = value; }
		}

		public ArrayList Keys {
			get { return _keyBags; }
		}

		public X509CertificateCollection Certificates {
			get { return _certs; }
		}

		internal RandomNumberGenerator RNG {
			get {
				if (_rng == null)
					_rng = RandomNumberGenerator.Create ();
				return _rng;
			}
		}

		// private methods

		private bool Compare (byte[] expected, byte[] actual) 
		{
			bool compare = false;
			if (expected.Length == actual.Length) {
				for (int i=0; i < expected.Length; i++) {
					if (expected [i] != actual [i])
						return false;
				}
				compare = true;
			}
			return compare;
		}

		private SymmetricAlgorithm GetSymmetricAlgorithm (string algorithmOid, byte[] salt, int iterationCount)
		{
			string algorithm = null;
			int keyLength = 8;	// 64 bits (default)
			int ivLength = 8;	// 64 bits (default)

			PKCS12.DeriveBytes pd = new PKCS12.DeriveBytes ();
			pd.Password = _password; 
			pd.Salt = salt;
			pd.IterationCount = iterationCount;

			switch (algorithmOid) {
				case PKCS5.pbeWithMD2AndDESCBC:			// no unit test available
					pd.HashName = "MD2";
					algorithm = "DES";
					break;
				case PKCS5.pbeWithMD5AndDESCBC:			// no unit test available
					pd.HashName = "MD5";
					algorithm = "DES";
					break;
				case PKCS5.pbeWithMD2AndRC2CBC:			// no unit test available
					// TODO - RC2-CBC-Parameter (PKCS5)
					// if missing default to 32 bits !!!
					pd.HashName = "MD2";
					algorithm = "RC2";
					keyLength = 4;		// default
					break;
				case PKCS5.pbeWithMD5AndRC2CBC:			// no unit test available
					// TODO - RC2-CBC-Parameter (PKCS5)
					// if missing default to 32 bits !!!
					pd.HashName = "MD5";
					algorithm = "RC2";
					keyLength = 4;		// default
					break;
				case PKCS5.pbeWithSHA1AndDESCBC: 		// no unit test available
					pd.HashName = "SHA1";
					algorithm = "DES";
					break;
				case PKCS5.pbeWithSHA1AndRC2CBC:		// no unit test available
					// TODO - RC2-CBC-Parameter (PKCS5)
					// if missing default to 32 bits !!!
					pd.HashName = "SHA1";
					algorithm = "RC2";
					keyLength = 4;		// default
					break;
				case PKCS12.pbeWithSHAAnd128BitRC4: 		// no unit test available
					pd.HashName = "SHA1";
					algorithm = "RC4";
					keyLength = 16;
					ivLength = 0;		// N/A
					break;
				case PKCS12.pbeWithSHAAnd40BitRC4: 		// no unit test available
					pd.HashName = "SHA1";
					algorithm = "RC4";
					keyLength = 5;
					ivLength = 0;		// N/A
					break;
				case PKCS12.pbeWithSHAAnd3KeyTripleDESCBC: 
					pd.HashName = "SHA1";
					algorithm = "TripleDES";
					keyLength = 24;
					break;
				case PKCS12.pbeWithSHAAnd2KeyTripleDESCBC:	// no unit test available
					pd.HashName = "SHA1";
					algorithm = "TripleDES";
					keyLength = 16;
					break;
				case PKCS12.pbeWithSHAAnd128BitRC2CBC: 		// no unit test available
					pd.HashName = "SHA1";
					algorithm = "RC2";
					keyLength = 16;
					break;
				case PKCS12.pbeWithSHAAnd40BitRC2CBC: 
					pd.HashName = "SHA1";
					algorithm = "RC2";
					keyLength = 5;
					break;
				default:
					throw new NotSupportedException ("unknown oid " + algorithm);
			}

			SymmetricAlgorithm sa = SymmetricAlgorithm.Create (algorithm);
			sa.Key = pd.DeriveKey (keyLength);
			// IV required only for block ciphers (not stream ciphers)
			if (ivLength > 0) {
				sa.IV = pd.DeriveIV (ivLength);
				sa.Mode = CipherMode.CBC;
			}
			return sa;
		}

		public byte[] Decrypt (string algorithmOid, byte[] salt, int iterationCount, byte[] encryptedData) 
		{
			SymmetricAlgorithm sa = null;
			byte[] result = null;
			try {
				sa = GetSymmetricAlgorithm (algorithmOid, salt, iterationCount);
				ICryptoTransform ct = sa.CreateDecryptor ();
				result = ct.TransformFinalBlock (encryptedData, 0, encryptedData.Length);
			}
			finally {
				if (sa != null)
					sa.Clear ();
			}
			return result;
		}

		public byte[] Decrypt (PKCS7.EncryptedData ed)
		{
			return Decrypt (ed.EncryptionAlgorithm.ContentType, 
				ed.EncryptionAlgorithm.Content [0].Value, 
				ASN1Convert.ToInt32 (ed.EncryptionAlgorithm.Content [1]),
				ed.EncryptedContent);
		}

		public byte[] Encrypt (string algorithmOid, byte[] salt, int iterationCount, byte[] data) 
		{
			byte[] result = null;
			using (SymmetricAlgorithm sa = GetSymmetricAlgorithm (algorithmOid, salt, iterationCount)) {
				ICryptoTransform ct = sa.CreateEncryptor ();
				result = ct.TransformFinalBlock (data, 0, data.Length);
			}
			return result;
		}

		private void AddPrivateKey (PKCS8.PrivateKeyInfo pki) 
		{
			byte[] privateKey = pki.PrivateKey;
			switch (privateKey [0]) {
				case 0x02:
					DSAParameters p = new DSAParameters (); // FIXME
					_keyBags.Add (PKCS8.PrivateKeyInfo.DecodeDSA (privateKey, p));
					break;
				case 0x30:
					_keyBags.Add (PKCS8.PrivateKeyInfo.DecodeRSA (privateKey));
					break;
				default:
					Array.Clear (privateKey, 0, privateKey.Length);
					throw new CryptographicException ("Unknown private key format");
			}
			Array.Clear (privateKey, 0, privateKey.Length);
		}

		private void ReadSafeBag (ASN1 safeBag) 
		{
			if (safeBag.Tag != 0x30)
				throw new ArgumentException ("invalid safeBag");

			ASN1 bagId = safeBag [0];
			if (bagId.Tag != 0x06)
				throw new ArgumentException ("invalid safeBag id");

			ASN1 bagValue = safeBag [1];
			string oid = ASN1Convert.ToOID (bagId);
			switch (oid) {
				case keyBag:
					// NEED UNIT TEST
					AddPrivateKey (new PKCS8.PrivateKeyInfo (bagValue.Value));
					break;
				case pkcs8ShroudedKeyBag:
					PKCS8.EncryptedPrivateKeyInfo epki = new PKCS8.EncryptedPrivateKeyInfo (bagValue.Value);
					byte[] decrypted = Decrypt (epki.Algorithm, epki.Salt, epki.IterationCount, epki.EncryptedData);
					AddPrivateKey (new PKCS8.PrivateKeyInfo (decrypted));
					Array.Clear (decrypted, 0, decrypted.Length);
					break;
				case certBag:
					PKCS7.ContentInfo cert = new PKCS7.ContentInfo (bagValue.Value);
					if (cert.ContentType != x509Certificate)
						throw new NotSupportedException ("unsupport certificate type");
					X509Certificate x509 = new X509Certificate (cert.Content [0].Value);
					_certs.Add (x509);
					break;
				case crlBag:
					// TODO
					break;
				case secretBag: 
					// TODO
					break;
				case safeContentsBag:
					// TODO - ? recurse ?
					break;
				default:
					throw new ArgumentException ("unknown safeBag oid");
			}
		}

		private ASN1 Pkcs8ShroudedKeyBag (AsymmetricAlgorithm aa) 
		{
			PKCS8.PrivateKeyInfo pki = new PKCS8.PrivateKeyInfo ();
			if (aa is RSA) {
				pki.Algorithm = "1.2.840.113549.1.1.1";
				pki.PrivateKey = PKCS8.PrivateKeyInfo.Encode ((RSA)aa);
			}
			else if (aa is DSA) {
				pki.Algorithm = null;
				pki.PrivateKey = PKCS8.PrivateKeyInfo.Encode ((DSA)aa);
			}
			else
				throw new CryptographicException ("Unknown asymmetric algorithm {0}", aa.ToString ());

			PKCS8.EncryptedPrivateKeyInfo epki = new PKCS8.EncryptedPrivateKeyInfo ();
			epki.Algorithm = pbeWithSHAAnd3KeyTripleDESCBC;
			epki.IterationCount = _iterations;
			epki.EncryptedData = Encrypt (pbeWithSHAAnd3KeyTripleDESCBC, epki.Salt, _iterations, pki.GetBytes ());

			ASN1 seqEncryptedWithParams = new ASN1 (0x30);
			seqEncryptedWithParams.Add (new ASN1 (epki.GetBytes ()));

			return seqEncryptedWithParams;
		}

		private ASN1 CertificateSafeBag (X509Certificate x509) 
		{
			ASN1 encapsulatedCertificate = new ASN1 (0x04, x509.RawData);

			PKCS7.ContentInfo ci = new PKCS7.ContentInfo ();
			ci.ContentType = x509Certificate;
			ci.Content.Add (encapsulatedCertificate);

			ASN1 bagValue = new ASN1 (0xA0);
			bagValue.Add (ci.ASN1);

			ASN1 safeBag = new ASN1 (0x30);
			safeBag.Add (ASN1Convert.FromOID (certBag));
			safeBag.Add (bagValue);

			return safeBag;
		}

		private byte[] MAC (byte[] password, byte[] salt, int iterations, byte[] data) 
		{
			PKCS12.DeriveBytes pd = new PKCS12.DeriveBytes ();
			pd.HashName = "SHA1";
			pd.Password = password;
			pd.Salt = salt;
			pd.IterationCount = iterations;

			HMACSHA1 hmac = (HMACSHA1) HMACSHA1.Create ();
			hmac.Key = pd.DeriveMAC (20);
			return hmac.ComputeHash (data, 0, data.Length);
		}

                /*
		 * SafeContents ::= SEQUENCE OF SafeBag
		 * 
		 * SafeBag ::= SEQUENCE {
		 *	bagId BAG-TYPE.&id ({PKCS12BagSet}),
		 *	bagValue [0] EXPLICIT BAG-TYPE.&Type({PKCS12BagSet}{@bagId}),
		 *	bagAttributes SET OF PKCS12Attribute OPTIONAL
		 * }
		 */
		public byte[] GetBytes () 
		{
			// TODO (incomplete)
			ASN1 safeBagSequence = new ASN1 (0x30);

			if (_keyBags.Count > 0) {
				ASN1 keysSafeBag = new ASN1 (0x30);
				foreach (AsymmetricAlgorithm key in _keyBags) {
					keysSafeBag.Add (ASN1Convert.FromOID (pkcs8ShroudedKeyBag));
					keysSafeBag.Add (Pkcs8ShroudedKeyBag (key));
				}

				ASN1 seq = new ASN1 (0x30);
				seq.Add (keysSafeBag);

				ASN1 data = new ASN1 (0x30);
				data.Add (seq);

				ASN1 keyContent = new ASN1 (0xA0);
				keyContent.Add (data);

				PKCS7.ContentInfo keyBag = new PKCS7.ContentInfo (PKCS7.data);
				keyBag.Content = keyContent;
				safeBagSequence.Add (keyBag.ASN1);
			}

			if (_certs.Count > 0) {
				byte[] certsSalt = new byte [8];
				RNG.GetBytes (certsSalt);

				ASN1 seqParams = new ASN1 (0x30);
				seqParams.Add (new ASN1 (0x04, certsSalt));
				seqParams.Add (ASN1Convert.FromInt32 (_iterations));

				ASN1 seqPbe = new ASN1 (0x30);
				seqPbe.Add (ASN1Convert.FromOID (pbeWithSHAAnd3KeyTripleDESCBC));
				seqPbe.Add (seqParams);

				ASN1 certsSafeBag = new ASN1 (0x30);
				foreach (X509Certificate x in _certs) {
					ASN1 certSafeBag = CertificateSafeBag (x);
					certsSafeBag.Add (certSafeBag);
				}
				byte[] encrypted = Encrypt (pbeWithSHAAnd3KeyTripleDESCBC, certsSalt, _iterations, certsSafeBag.GetBytes ());
				ASN1 encryptedCerts = new ASN1 (0x80, encrypted);

				ASN1 seq = new ASN1 (0x30);
				seq.Add (ASN1Convert.FromOID (PKCS7.data));
				seq.Add (seqPbe);
				seq.Add (encryptedCerts);

				ASN1 certsVersion = new ASN1 (0x02, new byte [1] { 0x00 });
				ASN1 encData = new ASN1 (0x30);
				encData.Add (certsVersion);
				encData.Add (seq);

				ASN1 certsContent = new ASN1 (0xA0);
				certsContent.Add (encData);

				PKCS7.ContentInfo bag = new PKCS7.ContentInfo (PKCS7.encryptedData);
				bag.Content = certsContent;
				safeBagSequence.Add (bag.ASN1);
			}

			ASN1 encapsulates = new ASN1 (0x04, safeBagSequence.GetBytes ());
			ASN1 ci = new ASN1 (0xA0);
			ci.Add (encapsulates);
			PKCS7.ContentInfo authSafe = new PKCS7.ContentInfo (PKCS7.data);
			authSafe.Content = ci;
			
			byte[] salt = new byte [20];
			RNG.GetBytes (salt);

			ASN1 macData = new ASN1 (0x30);
			byte[] macValue = MAC (_password, salt, _iterations, authSafe.Content [0].Value);
			if (macValue != null) {
				// only for password based encryption
				ASN1 oidSeq = new ASN1 (0x30);
				oidSeq.Add (ASN1Convert.FromOID ("1.3.14.3.2.26"));	// SHA1

				ASN1 mac = new ASN1 (0x30);
				mac.Add (oidSeq);
				mac.Add (new ASN1 (0x04, macValue));

				macData.Add (mac);
				macData.Add (new ASN1 (0x04, salt));
				macData.Add (ASN1Convert.FromInt32 (_iterations));
			}
			
			ASN1 version = new ASN1 (0x02, new byte [1] { 0x03 });
			
			ASN1 pfx = new ASN1 (0x30);
			pfx.Add (version);
			pfx.Add (authSafe.ASN1);
			if (macValue != null) {
				// only for password based encryption
				pfx.Add (macData);
			}

			byte[] result = pfx.GetBytes ();
/* DEBUG
			using (FileStream fs = File.OpenWrite (@"c:\my.pfx")) {
				fs.Write (result, 0, result.Length);
				fs.Close ();
			}
*/
			return result;
		}

		// static methods

		static private byte[] LoadFile (string filename) 
		{
			byte[] data = null;
			using (FileStream fs = File.OpenRead (filename)) {
				data = new byte [fs.Length];
				fs.Read (data, 0, data.Length);
				fs.Close ();
			}
			return data;
		}

		static public PKCS12 LoadFromFile (string filename) 
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");

			return new PKCS12 (LoadFile (filename));
		}

		static public PKCS12 LoadFromFile (string filename, string password) 
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");

			return new PKCS12 (LoadFile (filename), password);
		}
	}
}
