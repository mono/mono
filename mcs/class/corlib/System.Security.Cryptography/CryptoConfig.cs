//
// CryptoConfig.cs: Handles cryptographic implementations and OIDs mappings.
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004-2007,2011 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Xamarin Inc. http://www.xamarin.com
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

#if FEATURE_CRYPTO_CONFIGURABLE

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Security.Cryptography.X509Certificates;

using Mono.Xml;

namespace System.Security.Cryptography {

[ComVisible (true)]
public partial class CryptoConfig {

	static private Dictionary<string,string> unresolved_algorithms;
	static private Dictionary<string,string> oids;

	private const string defaultNamespace = "System.Security.Cryptography.";
	static Type defaultSHA1 = typeof (SHA1CryptoServiceProvider);
	static Type defaultMD5 = typeof (MD5CryptoServiceProvider);
	static Type defaultSHA256 = typeof (SHA256Managed);
	static Type defaultSHA384 = typeof (SHA384Managed);
	static Type defaultSHA512 = typeof (SHA512Managed);
	static Type defaultRSA = typeof (RSACryptoServiceProvider);
	static Type defaultDSA = typeof (DSACryptoServiceProvider);
	static Type defaultDES = typeof (DESCryptoServiceProvider);
	static Type default3DES = typeof (TripleDESCryptoServiceProvider);
	static Type defaultRC2 = typeof (RC2CryptoServiceProvider);
	static Type defaultAES = typeof (RijndaelManaged);
	// LAMESPEC: undocumented names in CryptoConfig
	static Type defaultRNG = typeof (RNGCryptoServiceProvider);
	static Type defaultHMAC = typeof (HMACSHA1);
	static Type defaultMAC3DES = typeof (MACTripleDES);
	// LAMESPEC: undocumented classes (also undocumented in CryptoConfig ;-)
	static Type defaultDSASigDesc = typeof (DSASignatureDescription);
	static Type defaultRSAPKCS1SHA1SigDesc = typeof (RSAPKCS1SHA1SignatureDescription);
	static Type defaultRSAPKCS1SHA256SigDesc = typeof (RSAPKCS1SHA256SignatureDescription);
	static Type defaultRSAPKCS1SHA384SigDesc = typeof (RSAPKCS1SHA384SignatureDescription);
	static Type defaultRSAPKCS1SHA512SigDesc = typeof (RSAPKCS1SHA512SignatureDescription);
	static Type defaultRIPEMD160 = typeof (RIPEMD160Managed);
	static Type defaultHMACMD5 = typeof (HMACMD5);
	static Type defaultHMACRIPEMD160 = typeof (HMACRIPEMD160);
	static Type defaultHMACSHA256 = typeof (HMACSHA256);
	static Type defaultHMACSHA384 = typeof (HMACSHA384);
	static Type defaultHMACSHA512 = typeof (HMACSHA512);

	// LAMESPEC: undocumented names in CryptoConfig
	private const string defaultC14N = defaultNamespace + "Xml.XmlDsigC14NTransform, " + Consts.AssemblySystem_Security;
	private const string defaultC14NWithComments = defaultNamespace + "Xml.XmlDsigC14NWithCommentsTransform, " + Consts.AssemblySystem_Security;
	private const string defaultBase64 = defaultNamespace + "Xml.XmlDsigBase64Transform, " + Consts.AssemblySystem_Security;
	private const string defaultXPath = defaultNamespace + "Xml.XmlDsigXPathTransform, " + Consts.AssemblySystem_Security;
	private const string defaultXslt = defaultNamespace + "Xml.XmlDsigXsltTransform, " + Consts.AssemblySystem_Security;
	private const string defaultEnveloped = defaultNamespace + "Xml.XmlDsigEnvelopedSignatureTransform, " + Consts.AssemblySystem_Security;
	private const string defaultXmlDecryption = defaultNamespace + "Xml.XmlDecryptionTransform, " + Consts.AssemblySystem_Security;
	private const string defaultExcC14N = defaultNamespace + "Xml.XmlDsigExcC14NTransform, " + Consts.AssemblySystem_Security;
	private const string defaultExcC14NWithComments = defaultNamespace + "Xml.XmlDsigExcC14NWithCommentsTransform, " + Consts.AssemblySystem_Security;

	// LAMESPEC: only documentated in ".NET Framework Security" book
	private const string defaultX509Data = defaultNamespace + "Xml.KeyInfoX509Data, " + Consts.AssemblySystem_Security;
	private const string defaultKeyName = defaultNamespace + "Xml.KeyInfoName, " + Consts.AssemblySystem_Security;
	private const string defaultKeyValueDSA = defaultNamespace + "Xml.DSAKeyValue, " + Consts.AssemblySystem_Security;
	private const string defaultKeyValueRSA = defaultNamespace + "Xml.RSAKeyValue, " + Consts.AssemblySystem_Security;
	private const string defaultRetrievalMethod = defaultNamespace + "Xml.KeyInfoRetrievalMethod, " + Consts.AssemblySystem_Security;

	private const string managedSHA1 = defaultNamespace + "SHA1Managed";

	// Oddly OID seems only available for hash algorithms
	private const string oidSHA1 = "1.3.14.3.2.26";
	private const string oidMD5 = "1.2.840.113549.2.5";
	// changed in 2.0
	private const string oidSHA256 = "2.16.840.1.101.3.4.2.1";
	private const string oidSHA384 = "2.16.840.1.101.3.4.2.2";
	private const string oidSHA512 = "2.16.840.1.101.3.4.2.3";
	private const string oidRIPEMD160 = "1.3.36.3.2.1";
	// new in 2.0
//	private const string oidRSA = "1.2.840.113549.1.1.1";
//	private const string oidDSA = "1.2.840.10040.4.1";
	private const string oidDES = "1.3.14.3.2.7";
	private const string oid3DES = "1.2.840.113549.3.7";
	private const string oidRC2 = "1.2.840.113549.3.2";
	// LAMESPEC: only documentated in ".NET Framework Security" book
	private const string oid3DESKeyWrap = "1.2.840.113549.1.9.16.3.6";

	private const string nameSHA1 = "System.Security.Cryptography.SHA1CryptoServiceProvider";
	private const string nameSHA1a = "SHA";
	private const string nameSHA1b = "SHA1";
	private const string nameSHA1c = "System.Security.Cryptography.SHA1";
	private const string nameSHA1d = "System.Security.Cryptography.HashAlgorithm";
	private const string nameMD5 = "System.Security.Cryptography.MD5CryptoServiceProvider";
	private const string nameMD5a = "MD5";
	private const string nameMD5b = "System.Security.Cryptography.MD5";
	private const string nameSHA256 = "System.Security.Cryptography.SHA256Managed";
	private const string nameSHA256a = "SHA256";
	private const string nameSHA256b = "SHA-256";
	private const string nameSHA256c = "System.Security.Cryptography.SHA256";
	private const string nameSHA384 = "System.Security.Cryptography.SHA384Managed";
	private const string nameSHA384a = "SHA384";
	private const string nameSHA384b = "SHA-384";
	private const string nameSHA384c = "System.Security.Cryptography.SHA384";
	private const string nameSHA512 = "System.Security.Cryptography.SHA512Managed";
	private const string nameSHA512a = "SHA512";
	private const string nameSHA512b = "SHA-512";
	private const string nameSHA512c = "System.Security.Cryptography.SHA512";
	private const string nameRSAa = "RSA";
	private const string nameRSAb = "System.Security.Cryptography.RSA";
	private const string nameRSAc = "System.Security.Cryptography.AsymmetricAlgorithm";
	private const string nameDSAa = "DSA";
	private const string nameDSAb = "System.Security.Cryptography.DSA";
	private const string nameDESa = "DES";
	private const string nameDESb = "System.Security.Cryptography.DES";
	private const string name3DESa = "3DES";
	private const string name3DESb = "TripleDES";
	private const string name3DESc = "Triple DES";
	private const string name3DESd = "System.Security.Cryptography.TripleDES";
	private const string nameRC2a = "RC2";
	private const string nameRC2b = "System.Security.Cryptography.RC2";
	private const string nameAESa = "Rijndael";
	private const string nameAESb = "System.Security.Cryptography.Rijndael";
	private const string nameAESc = "System.Security.Cryptography.SymmetricAlgorithm";
	// LAMESPEC: undocumented names in CryptoConfig
	private const string nameRNGa = "RandomNumberGenerator";
	private const string nameRNGb = "System.Security.Cryptography.RandomNumberGenerator";
	private const string nameKeyHasha = "System.Security.Cryptography.KeyedHashAlgorithm";
	private const string nameHMACSHA1a = "HMACSHA1";
	private const string nameHMACSHA1b = "System.Security.Cryptography.HMACSHA1";
	private const string nameMAC3DESa = "MACTripleDES";
	private const string nameMAC3DESb = "System.Security.Cryptography.MACTripleDES";
	// LAMESPEC: only documentated in ".NET Framework Security" book
	private const string name3DESKeyWrap = "TripleDESKeyWrap";
	private const string nameRIPEMD160 = "System.Security.Cryptography.RIPEMD160Managed";
	private const string nameRIPEMD160a = "RIPEMD160";
	private const string nameRIPEMD160b = "RIPEMD-160";
	private const string nameRIPEMD160c = "System.Security.Cryptography.RIPEMD160";
//	private const string nameHMACa = "HMAC";
	private const string nameHMACb = "System.Security.Cryptography.HMAC";
	private const string nameHMACMD5a = "HMACMD5";
	private const string nameHMACMD5b = "System.Security.Cryptography.HMACMD5";
	private const string nameHMACRIPEMD160a = "HMACRIPEMD160";
	private const string nameHMACRIPEMD160b = "System.Security.Cryptography.HMACRIPEMD160";
	private const string nameHMACSHA256a = "HMACSHA256";
	private const string nameHMACSHA256b = "System.Security.Cryptography.HMACSHA256";
	private const string nameHMACSHA384a = "HMACSHA384";
	private const string nameHMACSHA384b = "System.Security.Cryptography.HMACSHA384";
	private const string nameHMACSHA512a = "HMACSHA512";
	private const string nameHMACSHA512b = "System.Security.Cryptography.HMACSHA512";

	private const string urlXmlDsig = "http://www.w3.org/2000/09/xmldsig#";
	// LAMESPEC: undocumented URLs in CryptoConfig
	private const string urlDSASHA1 = urlXmlDsig + "dsa-sha1";			// no space
	private const string urlRSASHA1 = urlXmlDsig + "rsa-sha1";			// no space
	private const string urlRSASHA256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";			// no space
	private const string urlRSASHA384 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384";			// no space
	private const string urlRSASHA512 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";			// no space
	private const string urlSHA1 = urlXmlDsig + "sha1";				// no space
	private const string urlC14N = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315"; 
	private const string urlC14NWithComments = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";
	private const string urlBase64 = "http://www.w3.org/2000/09/xmldsig#base64";
	private const string urlXPath = "http://www.w3.org/TR/1999/REC-xpath-19991116";
	private const string urlXslt = "http://www.w3.org/TR/1999/REC-xslt-19991116";
	private const string urlEnveloped = urlXmlDsig + "enveloped-signature";		// no space
	private const string urlXmlDecryption = "http://www.w3.org/2002/07/decrypt#XML";
	private const string urlExcC14NWithComments = "http://www.w3.org/2001/10/xml-exc-c14n#WithComments";
	private const string urlExcC14N = "http://www.w3.org/2001/10/xml-exc-c14n#";
	private const string urlSHA256 = "http://www.w3.org/2001/04/xmlenc#sha256";
	private const string urlSHA384 = "http://www.w3.org/2001/04/xmldsig-more#sha384";
	private const string urlSHA512 = "http://www.w3.org/2001/04/xmlenc#sha512";
	private const string urlHMACSHA1 = "http://www.w3.org/2000/09/xmldsig#hmac-sha1";
	private const string urlHMACSHA256 = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
	private const string urlHMACSHA384 = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha384";
	private const string urlHMACSHA512 = "http://www.w3.org/2001/04/xmldsig-more#hmac-sha512";
	private const string urlHMACRIPEMD160 = "http://www.w3.org/2001/04/xmldsig-more#hmac-ripemd160";

	// LAMESPEC: only documentated in ".NET Framework Security" book
	private const string urlX509Data = urlXmlDsig + " X509Data";			// space is required
	private const string urlKeyName = urlXmlDsig + " KeyName";			// space is required
	private const string urlKeyValueDSA = urlXmlDsig + " KeyValue/DSAKeyValue";	// space is required
	private const string urlKeyValueRSA = urlXmlDsig + " KeyValue/RSAKeyValue";	// space is required
	private const string urlRetrievalMethod = urlXmlDsig + " RetrievalMethod";	// space is required

	// new (2.0) X509 certificate extensions
	private const string oidX509SubjectKeyIdentifier = "2.5.29.14";
	private const string oidX509KeyUsage = "2.5.29.15";
	private const string oidX509BasicConstraints = "2.5.29.19";
	private const string oidX509EnhancedKeyUsage = "2.5.29.37";

	private const string nameX509SubjectKeyIdentifier = defaultNamespace + "X509Certificates.X509SubjectKeyIdentifierExtension, " + Consts.AssemblySystem;
	private const string nameX509KeyUsage = defaultNamespace + "X509Certificates.X509KeyUsageExtension, " + Consts.AssemblySystem;
	private const string nameX509BasicConstraints = defaultNamespace + "X509Certificates.X509BasicConstraintsExtension, " + Consts.AssemblySystem;
	private const string nameX509EnhancedKeyUsage = defaultNamespace + "X509Certificates.X509EnhancedKeyUsageExtension, " + Consts.AssemblySystem;

	// new (2.0) X509 Chain
	private const string nameX509Chain = "X509Chain";
	private const string defaultX509Chain = defaultNamespace + "X509Certificates.X509Chain, " + Consts.AssemblySystem;
	// AES
	const string system_core_assembly = ", System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
	const string nameAES_1 = "AES";
	const string nameAES_2 = "System.Security.Cryptography.AesCryptoServiceProvider";
	const string defaultAES_1 = "System.Security.Cryptography.AesCryptoServiceProvider" + system_core_assembly;
	
	const string nameAESManaged_1 = "AesManaged";
	const string nameAESManaged_2 = "System.Security.Cryptography.AesManaged";
	const string defaultAESManaged = "System.Security.Cryptography.AesManaged" + system_core_assembly;

	// ECDiffieHellman - not implemented in System.Core at the moment
	const string nameECDiffieHellman_1 = "ECDH";
	const string nameECDiffieHellman_2 = "ECDiffieHellman";
	const string nameECDiffieHellman_3 = "ECDiffieHellmanCng";
	const string nameECDiffieHellman_4 = "System.Security.Cryptography.ECDiffieHellmanCng";
	const string defaultECDiffieHellman = "System.Security.Cryptography.ECDiffieHellmanCng" + system_core_assembly;

	// ECDsa - not implemented in System.Core at the moment
	const string nameECDsa_1 = "ECDsa";
	const string nameECDsa_2 = "ECDsaCng";
	const string nameECDsa_3 = "System.Security.Cryptography.ECDsaCng";
	const string defaultECDsa = "System.Security.Cryptography.ECDsaCng" + system_core_assembly;

	// SHA1Cng
	const string nameSHA1Cng = "System.Security.Cryptography.SHA1Cng";
	const string defaultSHA1Cng = "System.Security.Cryptography.SHA1Cng" + system_core_assembly;

	// SHA256Cng
	const string nameSHA256Cng = "System.Security.Cryptography.SHA256Cng";
	const string defaultSHA256Cng = "System.Security.Cryptography.SHA256Cng" + system_core_assembly;

	// SHA256 provider
	const string nameSHA256Provider = "System.Security.Cryptography.SHA256CryptoServiceProvider";
	const string defaultSHA256Provider = "System.Security.Cryptography.SHA256CryptoServiceProvider" + system_core_assembly;

	// SHA384Cng
	const string nameSHA384Cng = "System.Security.Cryptography.SHA384Cng";
	const string defaultSHA384Cng = "System.Security.Cryptography.SHA384Cng" + system_core_assembly;

	// SHA384 provider
	const string nameSHA384Provider = "System.Security.Cryptography.SHA384CryptoServiceProvider";
	const string defaultSHA384Provider = "System.Security.Cryptography.SHA384CryptoServiceProvider" + system_core_assembly;

	// SHA512Cng
	const string nameSHA512Cng = "System.Security.Cryptography.SHA512Cng";
	const string defaultSHA512Cng = "System.Security.Cryptography.SHA512Cng" + system_core_assembly;

	// SHA512 provider
	const string nameSHA512Provider = "System.Security.Cryptography.SHA512CryptoServiceProvider";
	const string defaultSHA512Provider = "System.Security.Cryptography.SHA512CryptoServiceProvider" + system_core_assembly;

	private static void Initialize () 
	{
		Dictionary<string,Type> algorithms = new Dictionary<string, Type> (StringComparer.OrdinalIgnoreCase);
		// see list @ http://msdn.microsoft.com/library/en-us/cpref/html/
		// frlrfSystemSecurityCryptographyCryptoConfigClassTopic.asp
		algorithms.Add (nameSHA1a, defaultSHA1);
		algorithms.Add (nameSHA1b, defaultSHA1);
		algorithms.Add (nameSHA1c, defaultSHA1);
		algorithms.Add (nameSHA1d, defaultSHA1);

		algorithms.Add (nameMD5a, defaultMD5);
		algorithms.Add (nameMD5b, defaultMD5);

		algorithms.Add (nameSHA256a, defaultSHA256);
		algorithms.Add (nameSHA256b, defaultSHA256);
		algorithms.Add (nameSHA256c, defaultSHA256);

		algorithms.Add (nameSHA384a, defaultSHA384);
		algorithms.Add (nameSHA384b, defaultSHA384);
		algorithms.Add (nameSHA384c, defaultSHA384);

		algorithms.Add (nameSHA512a, defaultSHA512);
		algorithms.Add (nameSHA512b, defaultSHA512);
		algorithms.Add (nameSHA512c, defaultSHA512);

		algorithms.Add (nameRSAa, defaultRSA);
		algorithms.Add (nameRSAb, defaultRSA); 
		algorithms.Add (nameRSAc, defaultRSA);

		algorithms.Add (nameDSAa, defaultDSA);  
		algorithms.Add (nameDSAb, defaultDSA);  

		algorithms.Add (nameDESa, defaultDES);
		algorithms.Add (nameDESb, defaultDES);

		algorithms.Add (name3DESa, default3DES);
		algorithms.Add (name3DESb, default3DES);
		algorithms.Add (name3DESc, default3DES);
		algorithms.Add (name3DESd, default3DES);

		algorithms.Add (nameRC2a, defaultRC2);
		algorithms.Add (nameRC2b, defaultRC2);

		algorithms.Add (nameAESa, defaultAES);  
		algorithms.Add (nameAESb, defaultAES);
		// LAMESPEC SymmetricAlgorithm documented as TripleDESCryptoServiceProvider
		algorithms.Add (nameAESc, defaultAES);

		// LAMESPEC These names aren't documented but (hint) the classes also have
		// static Create methods. So logically they should (and are) here.
		algorithms.Add (nameRNGa, defaultRNG);
		algorithms.Add (nameRNGb, defaultRNG);
		algorithms.Add (nameKeyHasha, defaultHMAC);
		algorithms.Add (nameHMACSHA1a, defaultHMAC);
		algorithms.Add (nameHMACSHA1b, defaultHMAC);
		algorithms.Add (nameMAC3DESa, defaultMAC3DES);
		algorithms.Add (nameMAC3DESb, defaultMAC3DES);
		algorithms.Add (nameRIPEMD160a, defaultRIPEMD160);
		algorithms.Add (nameRIPEMD160b, defaultRIPEMD160);
		algorithms.Add (nameRIPEMD160c, defaultRIPEMD160);
		algorithms.Add (nameHMACb, defaultHMAC);
		algorithms.Add (nameHMACMD5a, defaultHMACMD5);
		algorithms.Add (nameHMACMD5b, defaultHMACMD5);
		algorithms.Add (nameHMACRIPEMD160a, defaultHMACRIPEMD160);
		algorithms.Add (nameHMACRIPEMD160b, defaultHMACRIPEMD160);
		algorithms.Add (nameHMACSHA256a, defaultHMACSHA256);
		algorithms.Add (nameHMACSHA256b, defaultHMACSHA256);
		algorithms.Add (nameHMACSHA384a, defaultHMACSHA384);
		algorithms.Add (nameHMACSHA384b, defaultHMACSHA384);
		algorithms.Add (nameHMACSHA512a, defaultHMACSHA512);
		algorithms.Add (nameHMACSHA512b, defaultHMACSHA512);
			
		// we do not want to load the types (and assemblies) unless we really need them
		// so we keep those names as strings
		Dictionary<string,string> unresolved_algorithms = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);
			
		// LAMESPEC These URLs aren't documented but (hint) installing the WSDK
		// add some of the XMLDSIG urls into machine.config (and they make a LOT
		// of sense for implementing XMLDSIG in System.Security.Cryptography.Xml)
		algorithms.Add (urlDSASHA1, defaultDSASigDesc); 
		algorithms.Add (urlRSASHA1, defaultRSAPKCS1SHA1SigDesc);
		algorithms.Add (urlRSASHA256, defaultRSAPKCS1SHA256SigDesc);
		algorithms.Add (urlRSASHA384, defaultRSAPKCS1SHA384SigDesc);
		algorithms.Add (urlRSASHA512, defaultRSAPKCS1SHA512SigDesc);
		algorithms.Add (urlSHA1, defaultSHA1);
		unresolved_algorithms.Add (urlC14N, defaultC14N);
		unresolved_algorithms.Add (urlC14NWithComments, defaultC14NWithComments);
		unresolved_algorithms.Add (urlBase64, defaultBase64);
		unresolved_algorithms.Add (urlXPath, defaultXPath);
		unresolved_algorithms.Add (urlXslt, defaultXslt);
		unresolved_algorithms.Add (urlEnveloped, defaultEnveloped);
		unresolved_algorithms.Add (urlExcC14N, defaultExcC14N);
		unresolved_algorithms.Add (urlExcC14NWithComments, defaultExcC14NWithComments);
		unresolved_algorithms.Add (urlXmlDecryption, defaultXmlDecryption);
		algorithms.Add (urlSHA256, defaultSHA256);
		algorithms.Add (urlSHA384, defaultSHA384);
		algorithms.Add (urlSHA512, defaultSHA512);
		algorithms.Add (urlHMACSHA1, defaultHMAC);
		algorithms.Add (urlHMACSHA256, defaultHMACSHA256);
		algorithms.Add (urlHMACSHA384, defaultHMACSHA384);
		algorithms.Add (urlHMACSHA512, defaultHMACSHA512);
		algorithms.Add (urlHMACRIPEMD160, defaultHMACRIPEMD160);
		// LAMESPEC: only documentated in ".NET Framework Security" book
		unresolved_algorithms.Add (urlX509Data, defaultX509Data);
		unresolved_algorithms.Add (urlKeyName, defaultKeyName);
		unresolved_algorithms.Add (urlKeyValueDSA, defaultKeyValueDSA);
		unresolved_algorithms.Add (urlKeyValueRSA, defaultKeyValueRSA);
		unresolved_algorithms.Add (urlRetrievalMethod, defaultRetrievalMethod);

		// note: X.509 extensions aren't part of OID but names
		unresolved_algorithms.Add (oidX509SubjectKeyIdentifier, nameX509SubjectKeyIdentifier);
		unresolved_algorithms.Add (oidX509KeyUsage, nameX509KeyUsage);
		unresolved_algorithms.Add (oidX509BasicConstraints, nameX509BasicConstraints);
		unresolved_algorithms.Add (oidX509EnhancedKeyUsage, nameX509EnhancedKeyUsage);
		// note: the default X.509Chain can also be created this way
		unresolved_algorithms.Add (nameX509Chain, defaultX509Chain);
		unresolved_algorithms.Add (nameAES_1, defaultAES_1);
		unresolved_algorithms.Add (nameAES_2, defaultAES_1);
		unresolved_algorithms.Add (nameAESManaged_1, defaultAESManaged);
		unresolved_algorithms.Add (nameAESManaged_2, defaultAESManaged);

		unresolved_algorithms.Add (nameECDiffieHellman_1, defaultECDiffieHellman);
		unresolved_algorithms.Add (nameECDiffieHellman_2, defaultECDiffieHellman);
		unresolved_algorithms.Add (nameECDiffieHellman_3, defaultECDiffieHellman);
		unresolved_algorithms.Add (nameECDiffieHellman_4, defaultECDiffieHellman);

		unresolved_algorithms.Add (nameECDsa_1, defaultECDsa);
		unresolved_algorithms.Add (nameECDsa_2, defaultECDsa);
		unresolved_algorithms.Add (nameECDsa_3, defaultECDsa);

#if MONODROID || XAMARIN_MODERN 
		algorithms.Add (nameSHA1Cng, defaultSHA1);
		algorithms.Add (nameSHA256Cng, defaultSHA256);
		algorithms.Add (nameSHA256Provider, defaultSHA256);
		algorithms.Add (nameSHA384Cng, defaultSHA384);
		algorithms.Add (nameSHA384Provider, defaultSHA384);
		algorithms.Add (nameSHA512Cng, defaultSHA512);
		algorithms.Add (nameSHA512Provider, defaultSHA512);
#else
		unresolved_algorithms.Add (nameSHA1Cng, defaultSHA1Cng);
		unresolved_algorithms.Add (nameSHA256Cng, defaultSHA256Cng);
		unresolved_algorithms.Add (nameSHA256Provider, defaultSHA256Provider);
		unresolved_algorithms.Add (nameSHA384Cng, defaultSHA384Cng);
		unresolved_algorithms.Add (nameSHA384Provider, defaultSHA384Provider);
		unresolved_algorithms.Add (nameSHA512Cng, defaultSHA512Cng);
		unresolved_algorithms.Add (nameSHA512Provider, defaultSHA512Provider);
#endif
		Dictionary<string,string> oid = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);

		// comments here are to match with MS implementation (but not with doc)
		// LAMESPEC: only HashAlgorithm seems to have their OID included
		oid.Add (nameSHA1, oidSHA1);
		oid.Add (managedSHA1, oidSHA1);
		oid.Add (nameSHA1b, oidSHA1);
		oid.Add (nameSHA1c, oidSHA1);
		oid.Add (nameSHA1Cng, oidSHA1);

		oid.Add (nameMD5, oidMD5);
		oid.Add (nameMD5a, oidMD5);
		oid.Add (nameMD5b, oidMD5);

		oid.Add (nameSHA256, oidSHA256);
		oid.Add (nameSHA256a, oidSHA256);
		oid.Add (nameSHA256c, oidSHA256);
		oid.Add (nameSHA256Cng, oidSHA256);
		oid.Add (nameSHA256Provider, oidSHA256);

		oid.Add (nameSHA384, oidSHA384);
		oid.Add (nameSHA384a, oidSHA384);
		oid.Add (nameSHA384c, oidSHA384);
		oid.Add (nameSHA384Cng, oidSHA384);
		oid.Add (nameSHA384Provider, oidSHA384);

		oid.Add (nameSHA512, oidSHA512);
		oid.Add (nameSHA512a, oidSHA512);
		oid.Add (nameSHA512c, oidSHA512);
		oid.Add (nameSHA512Cng, oidSHA512);
		oid.Add (nameSHA512Provider, oidSHA512);

		oid.Add (nameRIPEMD160, oidRIPEMD160);
		oid.Add (nameRIPEMD160a, oidRIPEMD160);
		oid.Add (nameRIPEMD160c, oidRIPEMD160);

		// surprise! documented in ".NET Framework Security" book
		oid.Add (name3DESKeyWrap, oid3DESKeyWrap);

		oid.Add (nameDESa, oidDES);
		oid.Add (name3DESb, oid3DES);
		oid.Add (nameRC2a, oidRC2);

		// Add/modify the config as specified by machine.config
		string config = Environment.GetMachineConfigPath ();
		LoadConfig (config, algorithms, oid);

		// update
		CryptoConfig.algorithms = algorithms;
		CryptoConfig.unresolved_algorithms = unresolved_algorithms;
		CryptoConfig.oids = oid;
	}

	[FileIOPermission (SecurityAction.Assert, Unrestricted = true)]
	private static void LoadConfig (string filename, IDictionary<string,Type> algorithms, IDictionary<string,string> oid)
	{
		if (!File.Exists (filename))
			return;

		try {
			using (TextReader reader = new StreamReader (filename)) {
				CryptoHandler handler = new CryptoHandler (algorithms, oid);
				SmallXmlParser parser = new SmallXmlParser ();
				parser.Parse (reader, handler);
			}
		}
		catch {
		}
	}

	public static object CreateFromName (string name)
	{
		return CreateFromName (name, null);
	}

	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public static object CreateFromName (string name, params object[] args)
	{
		if (name == null)
			throw new ArgumentNullException ("name");

		lock (lockObject) {
			if (algorithms == null) {
				Initialize ();
			}
		}
	
		try {
			Type algoClass = null;
			if (!algorithms.TryGetValue (name, out algoClass)) {
				string algo = null;
				if (!unresolved_algorithms.TryGetValue (name, out algo))
						algo = name;
				algoClass = Type.GetType (algo);
			}
			if (algoClass == null)
				return null;
			// call the constructor for the type
			return Activator.CreateInstance (algoClass, args);
		}
		catch {
			// method doesn't throw any exception
			return null;
		}
	}

	public static string MapNameToOID (string name)
	{
		if (name == null)
			throw new ArgumentNullException ("name");

		lock (lockObject) {
			if (oids == null) {
				Initialize ();
			}
		}
			
		string result = null;
		oids.TryGetValue (name, out result);
		return result;
	}

	public static void AddOID (string oid, params string[] names)
	{
		if (oid == null)
				throw new ArgumentNullException ("oid");
		if (names  == null)
				throw new ArgumentNullException ("names");
			
		foreach (string name in names) {
				if (String.IsNullOrWhiteSpace (name))
					throw new ArithmeticException ("names");
				oids [oid] = name;
		}
	}

	class CryptoHandler: SmallXmlParser.IContentHandler {

		IDictionary<string,Type> algorithms;
		IDictionary<string,string> oid;
		Dictionary<string,string> names;
		Dictionary<string,string> classnames;
		int level;

		public CryptoHandler (IDictionary<string,Type> algorithms, IDictionary<string,string> oid)
		{
			this.algorithms = algorithms;
			this.oid = oid;
			// temporary tables to reconstruct algorithms
			names = new Dictionary<string,string> ();
			classnames = new Dictionary<string,string> ();
		}

		public void OnStartParsing (SmallXmlParser parser)
		{
			// don't care
		}

		public void OnEndParsing (SmallXmlParser parser)
		{
			foreach (var kpv in names) {
				try {
					algorithms [kpv.Key] = Type.GetType (classnames [kpv.Value]);
				}
				catch {
				}
			}
			// matching is done, data no more required
			names.Clear ();
			classnames.Clear ();
		}

		private string Get (SmallXmlParser.IAttrList attrs, string name)
		{
			for (int i = 0; i < attrs.Names.Length; i++) {
				if (attrs.Names[i] == name)
					return attrs.Values[i];
			}
			return String.Empty;
		}

		public void OnStartElement (string name, SmallXmlParser.IAttrList attrs)
		{
			switch (level) {
			case 0:
				if (name == "configuration")
					level++;
				break;
			case 1:
				if (name == "mscorlib")
					level++;
				break;
			case 2:
				if (name == "cryptographySettings")
					level++;
				break;
			case 3:
				if (name == "oidMap")
					level++;
				else if (name == "cryptoNameMapping")
					level++;
				break;
			case 4:
				if (name == "oidEntry") {
					oid [Get (attrs, "name")] = Get (attrs, "OID");
				} else if (name == "nameEntry") {
					names [Get (attrs, "name")] = Get (attrs, "class");
				} else if (name == "cryptoClasses") {
					level++;
				}
				break;
			case 5:
				if (name == "cryptoClass")
					classnames [attrs.Names[0]] = attrs.Values[0];
				break;
			}
		}

		public void OnEndElement (string name)
		{
			// parser will make sure the XML structure is respected
			switch (level) {
			case 1:
				if (name == "configuration")
					level--;
				break;
			case 2:
				if (name == "mscorlib")
					level--;
				break;
			case 3:
				if (name == "cryptographySettings")
					level--;
				break;
			case 4:
				if ((name == "oidMap") || (name == "cryptoNameMapping"))
					level--;
				break;
			case 5:
				if (name == "cryptoClasses")
					level--;
				break;
			}
		}

		public void OnProcessingInstruction (string name, string text)
		{
			// don't care
		}

		public void OnChars (string text)
		{
			// don't care
		}

		public void OnIgnorableWhitespace (string text)
		{
			// don't care
		}
	}
}
}

#endif

