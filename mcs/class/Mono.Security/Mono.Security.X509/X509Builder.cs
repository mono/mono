using System;
using System.Security.Cryptography;

using Mono.Security;

namespace Mono.Security.X509 {

	public abstract class X509Builder {

		private const string defaultHash = "SHA1";
		private string hashName;

		public X509Builder ()
		{
			hashName = defaultHash;
		}

		protected abstract ASN1 ToBeSigned (string hashName);

		// move to PKCS1
		protected string GetOID (string hashName) 
		{
			switch (hashName.ToLower ()) {
				case "md2":
					// md2withRSAEncryption (1 2 840 113549 1 1 2)
					return "1.2.840.113549.1.1.2";
				case "md4":
					// md4withRSAEncryption (1 2 840 113549 1 1 3)
					return "1.2.840.113549.1.1.3";
				case "md5":
					// md5withRSAEncryption (1 2 840 113549 1 1 4)
					return "1.2.840.113549.1.1.4";
				case "sha1":
					// sha1withRSAEncryption (1 2 840 113549 1 1 5)
					return "1.2.840.113549.1.1.5";
				case "sha256":
					// sha256WithRSAEncryption 	OBJECT IDENTIFIER ::= { pkcs-1 11 }
					return "1.2.840.113549.1.1.11";
				case "sha384":
					// sha384WithRSAEncryption 	OBJECT IDENTIFIER ::= { pkcs-1 12 }
					return "1.2.840.113549.1.1.12";
				case "sha512":
					// sha512WithRSAEncryption 	OBJECT IDENTIFIER ::= { pkcs-1 13 }
					return "1.2.840.113549.1.1.13";
				default:
					throw new NotSupportedException ("Unknown hash algorithm " + hashName);
			}
		}

		public string Hash {
			get { return hashName; }
			set { 
				if (hashName == null)
					hashName = defaultHash;
				else
					hashName = value;
			}
		}

		public virtual byte[] Sign (AsymmetricAlgorithm aa) 
		{
			if (aa is RSA)
				return Sign (aa as RSA);
			else if (aa is DSA)
				return Sign (aa as DSA);
			else
				throw new NotSupportedException ("Unknown Asymmetric Algorithm " + aa.ToString());
		}

		private byte[] Build (ASN1 tbs, string hashoid, byte[] signature) 
		{
			ASN1 builder = new ASN1 (0x30);
			builder.Add (tbs);
			builder.Add (PKCS7.AlgorithmIdentifier (hashoid));
			// first byte of BITSTRING is the number of unused bits in the first byte
			byte[] bitstring = new byte [signature.Length + 1];
			Array.Copy (signature, 0, bitstring, 1, signature.Length);
			builder.Add (new ASN1 (0x03, bitstring));
			return builder.GetBytes ();
		}

		public virtual byte[] Sign (RSA key)
		{
			string oid = GetOID (hashName);
			ASN1 tbs = ToBeSigned (oid);
			HashAlgorithm ha = HashAlgorithm.Create (hashName);
			byte[] hash = ha.ComputeHash (tbs.GetBytes ());

			RSAPKCS1SignatureFormatter pkcs1 = new RSAPKCS1SignatureFormatter (key);
			pkcs1.SetHashAlgorithm (hashName);
			byte[] signature = pkcs1.CreateSignature (hash);

			return Build (tbs, oid, signature);
		}

		public virtual byte[] Sign (DSA key) 
		{
			string oid = "1.2.840.10040.4.3";
			ASN1 tbs = ToBeSigned (oid);
			HashAlgorithm ha = HashAlgorithm.Create (hashName);
			if (!(ha is SHA1))
				throw new NotSupportedException ("Only SHA-1 is supported for DSA");
			byte[] hash = ha.ComputeHash (tbs.GetBytes ());

			DSASignatureFormatter dsa = new DSASignatureFormatter (key);
			dsa.SetHashAlgorithm (hashName);
			byte[] rs = dsa.CreateSignature (hash);

			// split R and S
			byte[] r = new byte [20];
			Array.Copy (rs, 0, r, 0, 20);
			byte[] s = new byte [20];
			Array.Copy (rs, 20, s, 0, 20);
			ASN1 signature = new ASN1 (0x30);
			signature.Add (new ASN1 (0x02, r));
			signature.Add (new ASN1 (0x02, s));

			// dsaWithSha1 (1 2 840 10040 4 3)
			return Build (tbs, oid, signature.GetBytes ());
		}
	}
}
