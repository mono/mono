//
// CryptoConfig.cs: Handles cryptographic implementations and OIDs.
//
// Author:
//		Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Reflection;

namespace System.Security.Cryptography
{

public class CryptoConfig
{
	static private Hashtable algorithms;
	static private Hashtable oid;
	static Assembly xmldsig;

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
	// LAMESPEC: undocumented names in CryptoConfig
	private const string defaultC14N = defaultNamespace + "Xml.XmlDsigC14NTransform";
	private const string defaultC14NWithComments = defaultNamespace + "Xml.XmlDsigC14NWithCommentsTransform";
	private const string defaultBase64 = defaultNamespace + "Xml.XmlDsigBase64Transform";
	private const string defaultXPath = defaultNamespace + "Xml.XmlDsigXPathTransform";
	private const string defaultXslt = defaultNamespace + "Xml.XmlDsigXsltTransform";
	private const string defaultEnveloped = defaultNamespace + "Xml.XmlDsigEnvelopedSignatureTransform";

	// Oddly OID seems only available for hash algorithms
	private const string oidSHA1 = "1.3.14.3.2.26";
	private const string oidMD5 = "1.2.840.113549.2.5";
	private const string oidSHA256 = "2.16.840.1.101.3.4.1";
	private const string oidSHA384 = "2.16.840.1.101.3.4.2";
	private const string oidSHA512 = "2.16.840.1.101.3.4.3";

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
	// LAMESPEC: undocumented URLs in CryptoConfig
	private const string urlDSASHA1 = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
	private const string urlRSASHA1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
	private const string urlSHA1 = "http://www.w3.org/2000/09/xmldsig#sha1";
	private const string urlC14N = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315"; 
	private const string urlC14NWithComments = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";
	private const string urlBase64 = "http://www.w3.org/2000/09/xmldsig#base64";
	private const string urlXPath = "http://www.w3.org/TR/1999/REC-xpath-19991116";
	private const string urlXslt = "http://www.w3.org/TR/1999/REC-xslt-19991116";
	private const string urlEnveloped = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";

	// ??? must we read from the machine.config each time or just at startup ???
	[MonoTODO ("support machine.config")]
	static CryptoConfig()
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

		oid = new Hashtable ();
		// comments here are to match with MS implementation (but not with doc)
		// LAMESPEC: only HashAlgorithm seems to have their OID included
		// oid.Add (nameSHA1a, oidSHA1);
		oid.Add (nameSHA1b, oidSHA1);
		oid.Add (nameSHA1c, oidSHA1);
		// oid.Add (nameSHA1d, oidSHA1);
		oid.Add (nameMD5a, oidMD5);
		oid.Add (nameMD5b, oidMD5);
		oid.Add (nameSHA256a, oidSHA256);
		// oid.Add (nameSHA256b, oidSHA256);
		oid.Add (nameSHA256c, oidSHA256);
		oid.Add (nameSHA384a, oidSHA384);
		// oid.Add (nameSHA384b, oidSHA384);
		oid.Add (nameSHA384c, oidSHA384);
		oid.Add (nameSHA512a, oidSHA512);
		// oid.Add (nameSHA512b, oidSHA512);
		oid.Add (nameSHA512c, oidSHA512);
	}

	public static object CreateFromName (string name)
	{
		return CreateFromName (name, null);
	}

	public static object CreateFromName (string name, object[] args)
	{
		if (name == null)
			throw new ArgumentNullException ();
	
		try {
			Type algoClass = null;
			string algo = (string)algorithms [name];
			// do we have an entry
			if (algo != null) {
				algoClass = Type.GetType (algo);
				// some classes are in assembly System.Security.Cryptography.Xml
				if ((algoClass == null) && (algo.StartsWith ("System.Security.Cryptography.Xml."))) {
					// second chance !
					if (xmldsig == null)
						xmldsig = Assembly.LoadWithPartialName ("System.Security");
					if (xmldsig != null)
						algoClass = xmldsig.GetType (algo);
				}
			}
			else
				algoClass = Type.GetType (name);
			// call the constructor for the type
			return Activator.CreateInstance (algoClass, args);
		}
		catch {
			return null;
		}
	}

	// Note: Couldn't access private in DefaultConfig so I copied the
	// two required functions.
/*	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	extern private static string get_machine_config_path ();

	private static string GetMachineConfigPath () 
	{
		return get_machine_config_path ();
	}*/

	// encode (7bits array) number greater than 127
	private static byte[] EncodeLongNumber (long x)
	{
		// for MS BCL compatibility
		// comment next two lines to remove restriction
		if ((x > Int32.MaxValue) || (x < Int32.MinValue))
			throw new OverflowException("part of OID doesn't fit in Int32");

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
		if (parts.Length < 2)
			throw new CryptographicUnexpectedOperationException ();
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
			throw new CryptographicUnexpectedOperationException ();
		}
		int j = 3;
		for (int i = 2; i < parts.Length; i++) {
			long x = Convert.ToInt64( parts [i]);
			if (x > 0x7F) {
				byte[] num = EncodeLongNumber (x);
				Array.Copy(num, 0, oid, j, num.Length);
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
			throw new CryptographicUnexpectedOperationException ("OID > 127 bytes");
			// comment exception and uncomment next 3 lines to remove restriction
			//byte[] num = EncodeLongNumber (j);
			//Array.Copy (num, 0, oid, j, num.Length);
			//k = num.Length + 1;
		}
		else
			oid2 [1] = Convert.ToByte (j - 2); 

		System.Array.Copy (oid, k, oid2, k, j - k);
		return oid2;
	}

	public static string MapNameToOID (string name)
	{
		if (name == null)
			throw new ArgumentNullException ();

		return (string)oid [name];
	}
}

}