//
// CryptoConfig.cs: Handles cryptographic implementations and OIDs mappings.
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// (C) 2004 Novell (http://www.novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using Mono.Xml;

namespace System.Security.Cryptography {

public class CryptoConfig {

	static private object lockObject;
	static private Hashtable algorithms;
	static private Hashtable oid;

	private const string defaultNamespace = "System.Security.Cryptography.";
	private const string defaultSHA1 = defaultNamespace + "SHA1CryptoServiceProvider";
	private const string defaultMD5 = defaultNamespace + "MD5CryptoServiceProvider";
	private const string defaultSHA256 = defaultNamespace + "SHA256Managed";
	private const string defaultSHA384 = defaultNamespace + "SHA384Managed";
	private const string defaultSHA512 = defaultNamespace + "SHA512Managed";
	private const string defaultRSA = defaultNamespace + "RSACryptoServiceProvider";
	private const string defaultDSA = defaultNamespace + "DSACryptoServiceProvider";
	private const string defaultDES = defaultNamespace + "DESCryptoServiceProvider";
	private const string default3DES = defaultNamespace + "TripleDESCryptoServiceProvider";
	private const string defaultRC2 = defaultNamespace + "RC2CryptoServiceProvider";
	private const string defaultAES = defaultNamespace + "RijndaelManaged";
	// LAMESPEC: undocumented names in CryptoConfig
	private const string defaultRNG = defaultNamespace + "RNGCryptoServiceProvider";
	private const string defaultHMAC = defaultNamespace + "HMACSHA1";
	private const string defaultMAC3DES = defaultNamespace + "MACTripleDES";
	// LAMESPEC: undocumented classes (also undocumented in CryptoConfig ;-)
	private const string defaultDSASigDesc = defaultNamespace + "DSASignatureDescription";
	private const string defaultRSASigDesc = defaultNamespace + "RSAPKCS1SHA1SignatureDescription";
#if NET_2_0
	private const string defaultRIPEMD160 = defaultNamespace + "RIPEMD160Managed";
#endif

	// LAMESPEC: undocumented names in CryptoConfig
#if (NET_2_0)
	private const string xmlAssembly = ", System.Security, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#elif (NET_1_1)
	private const string xmlAssembly = ", System.Security, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#elif (NET_1_0)
	private const string xmlAssembly = ", System.Security, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#else
	private const string xmlAssembly = ", System.Security, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
#endif
	private const string defaultC14N = defaultNamespace + "Xml.XmlDsigC14NTransform" + xmlAssembly;
	private const string defaultC14NWithComments = defaultNamespace + "Xml.XmlDsigC14NWithCommentsTransform" + xmlAssembly;
	private const string defaultBase64 = defaultNamespace + "Xml.XmlDsigBase64Transform" + xmlAssembly;
	private const string defaultXPath = defaultNamespace + "Xml.XmlDsigXPathTransform" + xmlAssembly;
	private const string defaultXslt = defaultNamespace + "Xml.XmlDsigXsltTransform" + xmlAssembly;
	private const string defaultEnveloped = defaultNamespace + "Xml.XmlDsigEnvelopedSignatureTransform" + xmlAssembly;
#if NET_2_0
	private const string defaultXmlDecryption = defaultNamespace + "Xml.XmlDecryptionTransform" + xmlAssembly;
	private const string defaultExcC14N = defaultNamespace + "Xml.XmlDsigExcC14NTransform" + xmlAssembly;
	private const string defaultExcC14NWithComments = defaultNamespace + "Xml.XmlDsigExcC14NWithCommentsTransform" + xmlAssembly;
#endif

	// LAMESPEC: only documentated in ".NET Framework Security" book
	private const string defaultX509Data = defaultNamespace + "Xml.KeyInfoX509Data" + xmlAssembly;
	private const string defaultKeyName = defaultNamespace + "Xml.KeyInfoName" + xmlAssembly;
	private const string defaultKeyValueDSA = defaultNamespace + "Xml.DSAKeyValue" + xmlAssembly;
	private const string defaultKeyValueRSA = defaultNamespace + "Xml.RSAKeyValue" + xmlAssembly;
	private const string defaultRetrievalMethod = defaultNamespace + "Xml.KeyInfoRetrievalMethod" + xmlAssembly;

	private const string managedSHA1 = defaultNamespace + "SHA1Managed";

	// Oddly OID seems only available for hash algorithms
	private const string oidSHA1 = "1.3.14.3.2.26";
	private const string oidMD5 = "1.2.840.113549.2.5";
	private const string oidSHA256 = "2.16.840.1.101.3.4.1";
	private const string oidSHA384 = "2.16.840.1.101.3.4.2";
	private const string oidSHA512 = "2.16.840.1.101.3.4.3";
	// LAMESPEC: only documentated in ".NET Framework Security" book
	private const string oid3DESKeyWrap = "1.2.840.113549.1.9.16.3.6";

	private const string nameSHA1a = "SHA";
	private const string nameSHA1b = "SHA1";
	private const string nameSHA1c = "System.Security.Cryptography.SHA1";
	private const string nameSHA1d = "System.Security.Cryptography.HashAlgorithm";
	private const string nameMD5a = "MD5";
	private const string nameMD5b = "System.Security.Cryptography.MD5";
	private const string nameSHA256a = "SHA256";
	private const string nameSHA256b = "SHA-256";
	private const string nameSHA256c = "System.Security.Cryptography.SHA256";
	private const string nameSHA384a = "SHA384";
	private const string nameSHA384b = "SHA-384";
	private const string nameSHA384c = "System.Security.Cryptography.SHA384";
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
	private const string nameHMACa = "HMACSHA1";
	private const string nameHMACb = "System.Security.Cryptography.HMACSHA1";
	private const string nameMAC3DESa = "MACTripleDES";
	private const string nameMAC3DESb = "System.Security.Cryptography.MACTripleDES";
	// LAMESPEC: only documentated in ".NET Framework Security" book
	private const string name3DESKeyWrap = "TripleDESKeyWrap";
#if NET_2_0
	private const string nameRIPEMD160a = "RIPEMD160";
	private const string nameRIPEMD160b = "RIPEMD-160";
	private const string nameRIPEMD160c = "System.Security.Cryptography.RIPEMD160";
#endif

	private const string urlXmlDsig = "http://www.w3.org/2000/09/xmldsig#";
	// LAMESPEC: undocumented URLs in CryptoConfig
	private const string urlDSASHA1 = urlXmlDsig + "dsa-sha1";			// no space
	private const string urlRSASHA1 = urlXmlDsig + "rsa-sha1";			// no space
	private const string urlSHA1 = urlXmlDsig + "sha1";				// no space
	private const string urlC14N = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315"; 
	private const string urlC14NWithComments = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";
	private const string urlBase64 = "http://www.w3.org/2000/09/xmldsig#base64";
	private const string urlXPath = "http://www.w3.org/TR/1999/REC-xpath-19991116";
	private const string urlXslt = "http://www.w3.org/TR/1999/REC-xslt-19991116";
	private const string urlEnveloped = urlXmlDsig + "enveloped-signature";		// no space
#if NET_2_0
	private const string urlXmlDecryption = "http://www.w3.org/2002/07/decrypt#XML";
	private const string urlExcC14N = "http://www.w3.org/2001/10/xml-exc-c14n#WithComments";
	private const string urlExcC14NWithComments = "http://www.w3.org/2001/10/xml-exc-c14n#";
#endif

	// LAMESPEC: only documentated in ".NET Framework Security" book
	private const string urlX509Data = urlXmlDsig + " X509Data";			// space is required
	private const string urlKeyName = urlXmlDsig + " KeyName";			// space is required
	private const string urlKeyValueDSA = urlXmlDsig + " KeyValue/DSAKeyValue";	// space is required
	private const string urlKeyValueRSA = urlXmlDsig + " KeyValue/RSAKeyValue";	// space is required
	private const string urlRetrievalMethod = urlXmlDsig + " RetrievalMethod";	// space is required

	static CryptoConfig () 
	{
		// lock(this) is bad
		// http://msdn.microsoft.com/library/en-us/dnaskdr/html/askgui06032003.asp?frame=true
		lockObject = new object ();
	}

	private static void Initialize () 
	{
		algorithms = new Hashtable ();
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
		algorithms.Add (nameHMACa, defaultHMAC);
		algorithms.Add (nameHMACb, defaultHMAC);
		algorithms.Add (nameMAC3DESa, defaultMAC3DES);
		algorithms.Add (nameMAC3DESb, defaultMAC3DES);
#if NET_2_0
		algorithms.Add (nameRIPEMD160a, defaultRIPEMD160);
		algorithms.Add (nameRIPEMD160b, defaultRIPEMD160);
		algorithms.Add (nameRIPEMD160c, defaultRIPEMD160);
#endif

		// LAMESPEC These URLs aren't documented but (hint) installing the WSDK
		// add some of the XMLDSIG urls into machine.config (and they make a LOT
		// of sense for implementing XMLDSIG in System.Security.Cryptography.Xml)
		algorithms.Add (urlDSASHA1, defaultDSASigDesc); 
		algorithms.Add (urlRSASHA1, defaultRSASigDesc);
		algorithms.Add (urlSHA1, defaultSHA1);
		algorithms.Add (urlC14N, defaultC14N);
		algorithms.Add (urlC14NWithComments, defaultC14NWithComments);
		algorithms.Add (urlBase64, defaultBase64);
		algorithms.Add (urlXPath, defaultXPath);
		algorithms.Add (urlXslt, defaultXslt);
		algorithms.Add (urlEnveloped, defaultEnveloped);
#if NET_2_0
		algorithms.Add (urlExcC14N, defaultExcC14N);
		algorithms.Add (urlExcC14NWithComments, defaultExcC14NWithComments);
		algorithms.Add (urlXmlDecryption, defaultXmlDecryption);
#endif
		// LAMESPEC: only documentated in ".NET Framework Security" book
		algorithms.Add (urlX509Data, defaultX509Data);
		algorithms.Add (urlKeyName, defaultKeyName);
		algorithms.Add (urlKeyValueDSA, defaultKeyValueDSA);
		algorithms.Add (urlKeyValueRSA, defaultKeyValueRSA);
		algorithms.Add (urlRetrievalMethod, defaultRetrievalMethod);

		oid = new Hashtable ();
		// comments here are to match with MS implementation (but not with doc)
		// LAMESPEC: only HashAlgorithm seems to have their OID included
		oid.Add (defaultSHA1, oidSHA1);
		oid.Add (managedSHA1, oidSHA1);
		oid.Add (nameSHA1b, oidSHA1);
		oid.Add (nameSHA1c, oidSHA1);

		oid.Add (defaultMD5, oidMD5);
		oid.Add (nameMD5a, oidMD5);
		oid.Add (nameMD5b, oidMD5);

		oid.Add (defaultSHA256, oidSHA256);
		oid.Add (nameSHA256a, oidSHA256);
		oid.Add (nameSHA256c, oidSHA256);

		oid.Add (defaultSHA384, oidSHA384);
		oid.Add (nameSHA384a, oidSHA384);
		oid.Add (nameSHA384c, oidSHA384);

		oid.Add (defaultSHA512, oidSHA512);
		oid.Add (nameSHA512a, oidSHA512);
		oid.Add (nameSHA512c, oidSHA512);

		// surprise! documented in ".NET Framework Security" book
		oid.Add (name3DESKeyWrap, oid3DESKeyWrap);

		// Add/modify the config as specified by machine.config
		string config = Environment.GetMachineConfigPath ();
		LoadConfig (config);
	}

	private static void LoadConfig (string filename) 
	{
		if (!File.Exists (filename))
			return;

		SecurityParser sp = new SecurityParser ();
		StreamReader sr = new StreamReader (filename);
		try {
			sp.LoadXml (sr.ReadToEnd ());
		}
		finally {
			sr.Close ();
		}

		try {
			SecurityElement root = sp.ToXml ();
			SecurityElement mscorlib = root.SearchForChildByTag ("mscorlib");
			if (mscorlib == null)
				return;
			SecurityElement cryptographySettings = mscorlib.SearchForChildByTag ("cryptographySettings");
			if (cryptographySettings == null)
				return;

			// algorithms
			SecurityElement cryptoNameMapping = cryptographySettings.SearchForChildByTag ("cryptoNameMapping");
			if (cryptoNameMapping != null) {
				SecurityElement cryptoClasses = cryptoNameMapping.SearchForChildByTag ("cryptoClasses");
				if (cryptoClasses != null) {
					foreach (SecurityElement nameEntry in cryptoNameMapping.Children) {
						if (nameEntry.Tag == "nameEntry") {
							string name = (string) nameEntry.Attributes ["name"];
							string clas = (string) nameEntry.Attributes ["class"];
							foreach (SecurityElement cryptoClass in cryptoClasses.Children) {
								string fullname = (string) cryptoClass.Attributes [clas];
								if (fullname != null) {
									if (algorithms.ContainsKey (name)) 
										algorithms.Remove (name);
									algorithms.Add (name, fullname);
									break;
								}
							}
						}
					}
				}
			}

			// oid
			SecurityElement oidMap = cryptographySettings.SearchForChildByTag ("oidMap");
			if (oidMap == null)
				return;
			foreach (SecurityElement oidEntry in oidMap.Children) {
				if (oidEntry.Tag == "oidEntry") {
					string oids = (string) oidEntry.Attributes ["OID"];
					if (oid.ContainsKey (oids)) 
						oid.Remove (oids);
					oid.Add (oids, oidEntry.Attributes ["name"]);
				}
			}
		}
		catch {
			// there's no error/warning in case the machine.config contains bad entries
		}
	}

	public static object CreateFromName (string name)
	{
		return CreateFromName (name, null);
	}

	public static object CreateFromName (string name, object[] args)
	{
		if (name == null)
			throw new ArgumentNullException ("name");

		if (algorithms == null) {
			lock (lockObject) {
				Initialize ();
			}
		}
	
		try {
			Type algoClass = null;
			string algo = (string) algorithms [name];
			// do we have an entry
			if (algo == null)
				algo = name;
			algoClass = Type.GetType (algo);
			// call the constructor for the type
			return Activator.CreateInstance (algoClass, args);
		}
		catch {
			// method doesn't throw any exception
			return null;
		}
	}

	// encode (7bits array) number greater than 127
	private static byte[] EncodeLongNumber (long x)
	{
		// for MS BCL compatibility
		// comment next two lines to remove restriction
		if ((x > Int32.MaxValue) || (x < Int32.MinValue))
			throw new OverflowException (Locale.GetText ("Part of OID doesn't fit in Int32"));

		long y = x;
		// number of bytes required to encode this number
		int n = 1;
		while (y > 0x7F) {
			y = y >> 7;
			n++;
		}
		byte[] num = new byte [n];
		// encode all bytes 
		for (int i = 0; i < n; i++) {
			y = x >> (7 * i);
			y = y & 0x7F;
			if (i != 0)
				y += 0x80;
			num[n-i-1] = Convert.ToByte (y);
		}
		return num;
	}

	public static byte[] EncodeOID (string str)
	{
		char[] delim = { '.' };
		string[] parts = str.Split (delim);
		// according to X.208 n is always at least 2
		if (parts.Length < 2) {
			throw new CryptographicUnexpectedOperationException (
				Locale.GetText ("OID must have at least two parts"));
		}

		// we're sure that the encoded OID is shorter than its string representation
		byte[] oid = new byte [str.Length];
		// now encoding value
		try {
			byte part0 = Convert.ToByte (parts [0]);
			// OID[0] > 2 is invalid but "supported" in MS BCL
			// uncomment next line to trap this error
			// if (part0 > 2) throw new CryptographicUnexpectedOperationException ();
			byte part1 = Convert.ToByte (parts [1]);
			// OID[1] >= 40 is illegal for OID[0] < 2 because of the % 40
			// however the syntax is "supported" in MS BCL
			// uncomment next 2 lines to trap this error
			//if ((part0 < 2) && (part1 >= 40))
			//	throw new CryptographicUnexpectedOperationException ();
			oid[2] = Convert.ToByte (part0 * 40 + part1);
		}
		catch {
			throw new CryptographicUnexpectedOperationException (
				Locale.GetText ("Invalid OID"));
		}
		int j = 3;
		for (int i = 2; i < parts.Length; i++) {
			long x = Convert.ToInt64 (parts [i]);
			if (x > 0x7F) {
				byte[] num = EncodeLongNumber (x);
				Buffer.BlockCopy (num, 0, oid, j, num.Length);
				j += num.Length;
			}
			else
				oid[j++] = Convert.ToByte (x);
		}

		int k = 2;
		// copy the exact number of byte required
		byte[] oid2 = new byte [j];
		oid2[0] = 0x06; // always - this tag means OID
		// Length (of value)
		if (j > 0x7F) {
			// for compatibility with MS BCL
			throw new CryptographicUnexpectedOperationException (
				Locale.GetText ("OID > 127 bytes"));
			// comment exception and uncomment next 3 lines to remove restriction
			//byte[] num = EncodeLongNumber (j);
			//Buffer.BlockCopy (num, 0, oid, j, num.Length);
			//k = num.Length + 1;
		}
		else
			oid2 [1] = Convert.ToByte (j - 2); 

		Buffer.BlockCopy (oid, k, oid2, k, j - k);
		return oid2;
	}

	public static string MapNameToOID (string name)
	{
		if (name == null)
			throw new ArgumentNullException ("name");

		if (oid == null) {
			lock (lockObject) {
				Initialize ();
			}
		}

		return (string)oid [name];
	}
}

}
