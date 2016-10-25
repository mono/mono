namespace System.Security.Cryptography {

	static class CAPI {
		// OID key type.
		internal const uint CRYPT_OID_INFO_OID_KEY   = 1;
		internal const uint CRYPT_OID_INFO_NAME_KEY  = 2;
		internal const uint CRYPT_OID_INFO_ALGID_KEY = 3;
		internal const uint CRYPT_OID_INFO_SIGN_KEY  = 4;

		public static string CryptFindOIDInfoNameFromKey (string key, OidGroup oidGroup)
		{
			// TODO: incomplete
			// TODO: oidGroup is ignored
			switch (key) {
			case "1.2.840.113549.1.1.5":
			case "1.3.14.3.2.29":
			case "1.3.14.3.2.15":
				return "sha1RSA";
			case "1.2.840.113549.1.1.4":
			case "1.3.14.3.2.3":
				return "md5RSA";
			case "1.2.840.10040.4.3":
			case "1.3.14.3.2.13":
				return "sha1DSA";
			case "1.2.840.113549.1.1.2":
			case "1.3.14.7.2.3.1":
				return "md2RSA";
			case "1.2.840.113549.1.1.3":
				return "md4RSA";
			case "1.3.14.3.2.27":
				return "dsaSHA1";
			case "2.16.840.1.101.2.1.1.19":
				return "mosaicUpdatedSig";
			case "1.3.14.3.2.26":
				return "sha1";
			case "1.2.840.113549.2.5":
				return "md5";
			case "2.16.840.1.101.3.4.2.1":
				return "sha256";
			case "2.16.840.1.101.3.4.2.2":
				return "sha384";
			case "2.16.840.1.101.3.4.2.3":
				return "sha512";
			case "1.2.840.113549.1.1.11":
				return "sha256RSA";
			case "1.2.840.113549.1.1.12":
				return "sha384RSA";
			case "1.2.840.113549.1.1.13":
				return "sha512RSA";
			case "1.2.840.113549.1.1.10":
				return "RSASSA-PSS";
			case "1.2.840.10045.4.1":
				return "sha1ECDSA";
			case "1.2.840.10045.4.3.2":
				return "sha256ECDSA";
			case "1.2.840.10045.4.3.3":
				return "sha384ECDSA";
			case "1.2.840.10045.4.3.4":
				return "sha512ECDSA";
			case "1.2.840.10045.4.3":
				return "specifiedECDSA";
			case "1.2.840.113549.1.1.1":
				return "RSA";
			case "1.2.840.113549.1.7.1":
				return "PKCS 7 Data";
			case "1.2.840.113549.1.9.3":
				return "Content Type";
			case "1.2.840.113549.1.9.4":
				return "Message Digest";
			case "1.2.840.113549.1.9.5":
				return "Signing Time";
			case "1.2.840.113549.3.7":
				return "3des";
			case "2.5.29.17":
				return "Subject Alternative Name";
			case "2.16.840.1.101.3.4.1.2":
				return "aes128";
			case "2.16.840.1.101.3.4.1.42":
				return "aes256";
			case "2.16.840.1.113730.1.1":
				return "Netscape Cert Type";
			}

			return null;
		}

		public static string CryptFindOIDInfoKeyFromName (string name, OidGroup oidGroup)
		{
			// TODO: incomplete
			// TODO: oidGroup is ignored			
			switch(name) {
			case "sha1RSA":
				return "1.2.840.113549.1.1.5";
			case "md5RSA":
				return "1.2.840.113549.1.1.4";
			case "sha1DSA":
				return "1.2.840.10040.4.3";
			case "shaRSA":
				return "1.3.14.3.2.29";
			case "md2RSA":
				return "1.2.840.113549.1.1.2";
			case "md4RSA":
				return "1.2.840.113549.1.1.3";
			case "dsaSHA1":
				return "1.3.14.3.2.27";
			case "mosaicUpdatedSig":
				return "2.16.840.1.101.2.1.1.19";
			case "sha1":
				return "1.3.14.3.2.26";
			case "md5":
				return "1.2.840.113549.2.5";
			case "sha256":
				return "2.16.840.1.101.3.4.2.1";
			case "sha384":
				return "2.16.840.1.101.3.4.2.2";
			case "sha512":
				return "2.16.840.1.101.3.4.2.3";
			case "sha256RSA":
				return "1.2.840.113549.1.1.11";
			case "sha384RSA":
				return "1.2.840.113549.1.1.12";
			case "sha512RSA":
				return "1.2.840.113549.1.1.13";
			case "RSASSA-PSS":
				return "1.2.840.113549.1.1.10";
			case "sha1ECDSA":
				return "1.2.840.10045.4.1";
			case "sha256ECDSA":
				return "1.2.840.10045.4.3.2";
			case "sha384ECDSA":
				return "1.2.840.10045.4.3.3";
			case "sha512ECDSA":
				return "1.2.840.10045.4.3.4";
			case "specifiedECDSA":
				return "1.2.840.10045.4.3";
			case "RSA":
				return "1.2.840.113549.1.1.1";
			case "PKCS 7 Data":
				return "1.2.840.113549.1.7.1";
			case "Content Type":
				return "1.2.840.113549.1.9.3";
			case "Message Digest":
				return "1.2.840.113549.1.9.4";
			case "Signing Time":
				return "1.2.840.113549.1.9.5";
			case "3des":
				return "1.2.840.113549.3.7";
			case "Subject Alternative Name":
				return "2.5.29.17";
			case "aes128":
				return "2.16.840.1.101.3.4.1.2";
			case "aes256":
				return "2.16.840.1.101.3.4.1.42";
			case "Netscape Cert Type":
				return "2.16.840.1.113730.1.1";
			}
			return null;
		}
	}
}