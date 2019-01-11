//
// CryptoConfig.cs: Handles cryptographic implementations and OIDs mappings.
//
// Authors:
//	Sebastien Pouliot (sebastien@xamarin.com)
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004-2007,2011 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin, Inc. (http://www.xamarin.com)
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

#if !FEATURE_CRYPTO_CONFIGURABLE

// This is a special version of CryptoConfig that is not configurable and
// every "choice" is statiscally compiled. As long as CreateFromName is not
// used the linker will be able to eliminate the crypto code from the applications

using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Security.Cryptography {

	[ComVisible (true)]
	public partial class CryptoConfig {

		public static void AddOID (string oid, params string[] names)
		{
			throw new PlatformNotSupportedException ();
		}

		// try to avoid hitting the CreateFromName overloads to help the linker

		public static object CreateFromName (string name)
		{
			return CreateFromName (name, null);
		}

#if MONOTOUCH || XAMMAC
		[PreserveDependencyAttribute (".ctor()", "System.Security.Cryptography.AesManaged", "System.Core")]
#else
		[PreserveDependencyAttribute (".ctor()", "System.Security.Cryptography.AesCryptoServiceProvider", "System.Core")]
#endif
		[PreserveDependencyAttribute (".ctor()", "System.Security.Cryptography.X509Certificates.X509Chain", "System")]
		[PreserveDependencyAttribute (".ctor()", "System.Security.Cryptography.X509Certificates.X509KeyUsageExtension", "System")]
		[PreserveDependencyAttribute (".ctor()", "System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension", "System")]
		[PreserveDependencyAttribute (".ctor()", "System.Security.Cryptography.X509Certificates.X509SubjectKeyIdentifierExtension", "System")]
		[PreserveDependencyAttribute (".ctor()", "System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension", "System")]
		public static object CreateFromName (string name, params object[] args)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			Type algoClass = null;

			// TODO: These ignore args
			switch (name.ToLowerInvariant ()) {
			case "system.security.cryptography.dsacryptoserviceprovider":
			case "system.security.cryptography.dsa":
			case "dsa":
				return new DSACryptoServiceProvider ();
			case "system.security.cryptography.dsasignaturedeformatter":
				return new DSASignatureDeformatter ();
			case "system.security.cryptography.dsasignatureformatter":
				return new DSASignatureFormatter ();
			case "system.security.cryptography.dsasignaturedescription":
			case "http://www.w3.org/2000/09/xmldsig#dsa-sha1":
				return new DSASignatureDescription ();
			case "system.security.cryptography.descryptoserviceprovider":
			case "system.security.cryptography.des":
			case "des":
				return new DESCryptoServiceProvider ();
			case "system.security.cryptography.hmacmd5":
			case "hmacmd5":
				return new HMACMD5 ();
			case "system.security.cryptography.hmacripemd160":
			case "hmacripemd160":
			case "http://www.w3.org/2001/04/xmldsig-more#hmac-ripemd160":
				return new HMACRIPEMD160 ();
			case "system.security.cryptography.keyedhashalgorithm":
			case "system.security.cryptography.hmac":
			case "system.security.cryptography.hmacsha1":
			case "hmacsha1":
			case "http://www.w3.org/2000/09/xmldsig#hmac-sha1":
				return new HMACSHA1 ();
			case "system.security.cryptography.hmacsha256":
			case "hmacsha256":
			case "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256":
				return new HMACSHA256 ();
			case "system.security.cryptography.hmacsha384":
			case "hmacsha384":
			case "http://www.w3.org/2001/04/xmldsig-more#hmac-sha384":
				return new HMACSHA384 ();
			case "system.security.cryptography.hmacsha512":
			case "hmacsha512":
			case "http://www.w3.org/2001/04/xmldsig-more#hmac-sha512":
				return new HMACSHA512 ();
			case "system.security.cryptography.mactripledes":
			case "mactripledes":
				return new MACTripleDES ();
			case "system.security.cryptography.md5cryptoserviceprovider":
			case "system.security.cryptography.md5":
			case "md5":
				return new MD5CryptoServiceProvider ();
			case "system.security.cryptography.rc2cryptoserviceprovider":
			case "system.security.cryptography.rc2":
			case "rc2":
				return new RC2CryptoServiceProvider ();
			case "system.security.cryptography.symmetricalgorithm":
			case "system.security.cryptography.rijndaelmanaged":
			case "system.security.cryptography.rijndael":
			case "rijndael":
				return new RijndaelManaged ();
			case "system.security.cryptography.ripemd160managed":
			case "system.security.cryptography.ripemd160":
			case "ripemd-160":
			case "ripemd160":
				return new RIPEMD160Managed ();
			case "system.security.cryptography.rngcryptoserviceprovider":
			case "system.security.cryptography.randomnumbergenerator":
			case "randomnumbergenerator":
				return new RNGCryptoServiceProvider ();
			case "system.security.cryptography.asymmetricalgorithm":
			case "system.security.cryptography.rsa":
			case "rsa":
				return new RSACryptoServiceProvider ();
			case "system.security.cryptography.rsapkcs1signaturedeformatter":
				return new RSAPKCS1SignatureDeformatter ();
			case "system.security.cryptography.rsapkcs1signatureformatter":
				return new RSAPKCS1SignatureFormatter ();
			case "system.security.cryptography.rsapkcs1sha1signaturedescription":
			case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
				return new RSAPKCS1SHA1SignatureDescription ();
			case "system.security.cryptography.rsapkcs1sha256signaturedescription":
			case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
				return new RSAPKCS1SHA256SignatureDescription ();
			case "system.security.cryptography.rsapkcs1sha384signaturedescription":
			case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384":
				return new RSAPKCS1SHA384SignatureDescription ();
			case "system.security.cryptography.rsapkcs1sha512signaturedescription":
			case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512":
				return new RSAPKCS1SHA512SignatureDescription ();
			case "system.security.cryptography.hashalgorithm":
			case "system.security.cryptography.sha1":
			case "system.security.cryptography.sha1cryptoserviceprovider":
			case "sha1":
			case "system.security.cryptography.sha1cng":
			case "sha":
			case "http://www.w3.org/2000/09/xmldsig#sha1":
				return new SHA1CryptoServiceProvider ();
			case "system.security.cryptography.sha1managed":
				return new SHA1Managed ();
			case "system.security.cryptography.sha256managed":
			case "system.security.cryptography.sha256":
			case "system.security.cryptography.sha256cryptoserviceprovider":
			case "system.security.cryptography.sha256cng":
			case "sha256":
			case "sha-256":
			case "http://www.w3.org/2001/04/xmlenc#sha256":
				return new SHA256Managed ();
			case "system.security.cryptography.sha384managed":
			case "system.security.cryptography.sha384":
			case "system.security.cryptography.sha384cryptoserviceprovider":
			case "system.security.cryptography.sha384cng":
			case "sha384":
			case "sha-384":
			case "http://www.w3.org/2001/04/xmldsig-more#sha384":
				return new SHA384Managed ();
			case "system.security.cryptography.sha512managed":
			case "system.security.cryptography.sha512":
			case "system.security.cryptography.sha512cryptoserviceprovider":
			case "system.security.cryptography.sha512cng":
			case "sha512":
			case "sha-512":
			case "http://www.w3.org/2001/04/xmlenc#sha512":
				return new SHA512Managed ();
			case "system.security.cryptography.tripledescryptoserviceprovider":
			case "system.security.cryptography.tripledes":
			case "triple des":
			case "tripledes":
			case "3des":
				return new TripleDESCryptoServiceProvider ();

			// Use Type.GetType to be linker friendly
			// TODO: This does not work when the assembly is not referenced by the project
			case "x509chain":
				algoClass = Type.GetType ("System.Security.Cryptography.X509Certificates.X509Chain, System");
				break;
			case "2.5.29.15":
				algoClass = Type.GetType ("System.Security.Cryptography.X509Certificates.X509KeyUsageExtension, System");
				break;
			case "2.5.29.19":
				algoClass = Type.GetType ("System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension, System");
				break;
			case "2.5.29.14":
				algoClass = Type.GetType ("System.Security.Cryptography.X509Certificates.X509SubjectKeyIdentifierExtension, System");
				break;
			case "2.5.29.37":
				algoClass = Type.GetType ("System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension, System");
				break;
			case "aes":
#if MONOTOUCH || XAMMAC
				algoClass = Type.GetType ("System.Security.Cryptography.AesManaged, System.Core");
#else
				algoClass = Type.GetType ("System.Security.Cryptography.AesCryptoServiceProvider, System.Core");
#endif
				break;
			}

			if (algoClass == null) {
				lock (lockObject) {
					if (algorithms?.TryGetValue (name, out algoClass) == true) {
						try {
							return Activator.CreateInstance (algoClass, args);
						} catch {
						}
					}
				}

				algoClass = Type.GetType (name);
			}

			try {
				// last resort, the request type might be available (if care is taken for the type not to be linked 
				// away) and that can allow some 3rd party code to work (e.g. extra algorithms) and make a few more
				// unit tests happy
				return Activator.CreateInstance (algoClass, args);
			} catch {
				// method doesn't throw any exception
				return null;
			}
		}

		internal static string MapNameToOID (string name, object arg)
		{
			return MapNameToOID (name);
		}

		public static string MapNameToOID (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			switch (name.ToLowerInvariant ()) {
			case "system.security.cryptography.sha1cryptoserviceprovider":
			case "system.security.cryptography.sha1cng":
			case "system.security.cryptography.sha1managed":
			case "system.security.cryptography.sha1":
			case "sha1":
				return "1.3.14.3.2.26";
			case "system.security.cryptography.md5cryptoserviceprovider":
			case "system.security.cryptography.md5":
			case "md5":
				return "1.2.840.113549.2.5";
			case "system.security.cryptography.sha256cryptoserviceprovider":
			case "system.security.cryptography.sha256cng":
			case "system.security.cryptography.sha256managed":
			case "system.security.cryptography.sha256":
			case "sha256":
				return "2.16.840.1.101.3.4.2.1";
			case "system.security.cryptography.sha384cryptoserviceprovider":
			case "system.security.cryptography.sha384cng":
			case "system.security.cryptography.sha384managed":
			case "system.security.cryptography.sha384":
			case "sha384":
				return "2.16.840.1.101.3.4.2.2";
			case "system.security.cryptography.sha512cryptoserviceprovider":
			case "system.security.cryptography.sha512cng":
			case "system.security.cryptography.sha512managed":
			case "system.security.cryptography.sha512":
			case "sha512":
				return "2.16.840.1.101.3.4.2.3";
			case "system.security.cryptography.ripemd160managed":
			case "system.security.cryptography.ripemd160":
			case "ripemd160":
				return "1.3.36.3.2.1";
			case "tripledeskeywrap":
				return "1.2.840.113549.1.9.16.3.6";
			case "des":
				return "1.3.14.3.2.7";
			case "tripledes":
				return "1.2.840.113549.3.7";
			case "rc2":
				return "1.2.840.113549.3.2";
			default:
				return null;
			}
		}

		static void Initialize ()
		{
			algorithms = new Dictionary<string, Type> (StringComparer.OrdinalIgnoreCase);
		}
	}
}

#endif
