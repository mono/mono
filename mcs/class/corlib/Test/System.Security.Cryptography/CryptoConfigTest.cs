//
// CryptoConfigTest.cs - NUnit Test Cases for CryptoConfig
//
// Author:
//		Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography
{

public class CryptoConfigTest : TestCase 
{
	protected override void SetUp () {}

	protected override void TearDown () {}

	public void AssertEquals (string msg, byte[] array1, byte[] array2)
	{
		AllTests.AssertEquals (msg, array1, array2);
	}

	void CreateFromName (string name, string objectname)
	{
		object o = CryptoConfig.CreateFromName (name);
		if (objectname == null)
			AssertNull (name, o);
		else
			AssertEquals (name, o.ToString(), objectname);
	}

	// validate that CryptoConfig create the exact same implementation between mono and MS
	public void TestCreateFromName () 
	{
		try {
			object o = CryptoConfig.CreateFromName (null);
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("ArgumentNullException not thrown: " + e.ToString());
		}
		CreateFromName ("SHA", "System.Security.Cryptography.SHA1CryptoServiceProvider");
		// FIXME: We need to support the machine.config file to get exact same results
		// with the MS .NET Framework
		CreateFromName ("SHA1", "System.Security.Cryptography.SHA1CryptoServiceProvider");
		CreateFromName( "System.Security.Cryptography.SHA1", "System.Security.Cryptography.SHA1CryptoServiceProvider");
		// after installing the WSDK - changes to the machine.config file (not documented)
//		CreateFromName ("SHA1", "System.Security.Cryptography.SHA1Managed");
//		CreateFromName ("System.Security.Cryptography.SHA1", "System.Security.Cryptography.SHA1Managed");
		CreateFromName ("System.Security.Cryptography.HashAlgorithm", "System.Security.Cryptography.SHA1CryptoServiceProvider");
		CreateFromName ("System.Security.Cryptography.SHA1CryptoServiceProvider", "System.Security.Cryptography.SHA1CryptoServiceProvider");
		CreateFromName ("MD5", "System.Security.Cryptography.MD5CryptoServiceProvider");  
		CreateFromName ("System.Security.Cryptography.MD5", "System.Security.Cryptography.MD5CryptoServiceProvider");  
		CreateFromName ("System.Security.Cryptography.MD5CryptoServiceProvider", "System.Security.Cryptography.MD5CryptoServiceProvider");
		CreateFromName ("SHA256", "System.Security.Cryptography.SHA256Managed");  
		CreateFromName ("SHA-256", "System.Security.Cryptography.SHA256Managed");  
		CreateFromName ("System.Security.Cryptography.SHA256", "System.Security.Cryptography.SHA256Managed");  
		CreateFromName ("SHA384", "System.Security.Cryptography.SHA384Managed");  
		CreateFromName ("SHA-384", "System.Security.Cryptography.SHA384Managed");  
		CreateFromName ("System.Security.Cryptography.SHA384", "System.Security.Cryptography.SHA384Managed");  
		CreateFromName ("SHA512", "System.Security.Cryptography.SHA512Managed");  
		CreateFromName ("SHA-512", "System.Security.Cryptography.SHA512Managed");  
		CreateFromName ("System.Security.Cryptography.SHA512", "System.Security.Cryptography.SHA512Managed");  
		CreateFromName ("RSA", "System.Security.Cryptography.RSACryptoServiceProvider");  
		CreateFromName ("System.Security.Cryptography.RSA", "System.Security.Cryptography.RSACryptoServiceProvider");  
		CreateFromName ("System.Security.Cryptography.AsymmetricAlgorithm", "System.Security.Cryptography.RSACryptoServiceProvider");  
		CreateFromName ("DSA", "System.Security.Cryptography.DSACryptoServiceProvider");  
		CreateFromName ("System.Security.Cryptography.DSA", "System.Security.Cryptography.DSACryptoServiceProvider");  
		CreateFromName ("DES", "System.Security.Cryptography.DESCryptoServiceProvider");  
		CreateFromName ("System.Security.Cryptography.DES", "System.Security.Cryptography.DESCryptoServiceProvider");  
		CreateFromName ("3DES", "System.Security.Cryptography.TripleDESCryptoServiceProvider");  
		CreateFromName ("TripleDES", "System.Security.Cryptography.TripleDESCryptoServiceProvider");  
		CreateFromName ("Triple DES", "System.Security.Cryptography.TripleDESCryptoServiceProvider");  
		CreateFromName ("System.Security.Cryptography.TripleDES", "System.Security.Cryptography.TripleDESCryptoServiceProvider");  
		// LAMESPEC SymmetricAlgorithm documented as TripleDESCryptoServiceProvider
		CreateFromName ("System.Security.Cryptography.SymmetricAlgorithm", "System.Security.Cryptography.RijndaelManaged");  
		CreateFromName ("RC2", "System.Security.Cryptography.RC2CryptoServiceProvider");  
		CreateFromName ("System.Security.Cryptography.RC2", "System.Security.Cryptography.RC2CryptoServiceProvider");  
		CreateFromName ("Rijndael", "System.Security.Cryptography.RijndaelManaged");  
		CreateFromName ("System.Security.Cryptography.Rijndael", "System.Security.Cryptography.RijndaelManaged");
		// LAMESPEC Undocumented Names in CryptoConfig
		CreateFromName ("RandomNumberGenerator", "System.Security.Cryptography.RNGCryptoServiceProvider");
		CreateFromName ("System.Security.Cryptography.RandomNumberGenerator", "System.Security.Cryptography.RNGCryptoServiceProvider");
		CreateFromName ("System.Security.Cryptography.KeyedHashAlgorithm", "System.Security.Cryptography.HMACSHA1");
		CreateFromName ("HMACSHA1", "System.Security.Cryptography.HMACSHA1");
		CreateFromName ("System.Security.Cryptography.HMACSHA1", "System.Security.Cryptography.HMACSHA1");
		CreateFromName ("MACTripleDES", "System.Security.Cryptography.MACTripleDES");
		CreateFromName ("System.Security.Cryptography.MACTripleDES", "System.Security.Cryptography.MACTripleDES");
		// note: CryptoConfig can create any object !
		CreateFromName ("System.Security.Cryptography.CryptoConfig", "System.Security.Cryptography.CryptoConfig");
		CreateFromName ("System.IO.MemoryStream", "System.IO.MemoryStream");
		// non existing algo should return null (without exception)
		AssertNull ("NonExistingAlgorithm", CryptoConfig.CreateFromName("NonExistingAlgorithm"));
	}

	// additional names (URL) used for XMLDSIG (System.Security.Cryptography.Xml)
	// URL taken from http://www.w3.org/TR/2002/REC-xmldsig-core-20020212/
	public void TestCreateFromURL () 
	{
		// URL used in SignatureMethod element
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#dsa-sha1", "System.Security.Cryptography.DSASignatureDescription");
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#rsa-sha1", "System.Security.Cryptography.RSAPKCS1SHA1SignatureDescription");
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#hmac-sha1", null);
		// URL used in DigestMethod element 
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#sha1", "System.Security.Cryptography.SHA1CryptoServiceProvider");
		// URL used in Canonicalization or Transform elements 
		CreateFromName ("http://www.w3.org/TR/2001/REC-xml-c14n-20010315", "System.Security.Cryptography.Xml.XmlDsigC14NTransform");
		CreateFromName ("http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments", "System.Security.Cryptography.Xml.XmlDsigC14NWithCommentsTransform");
		// URL used in Transform element 
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#base64", "System.Security.Cryptography.Xml.XmlDsigBase64Transform");
		// after installing the WSDK - changes to the machine.config file (not documented)
//		CreateFromName ("http://www.w3.org/TR/1999/REC-xpath-19991116", "Microsoft.WSDK.Security.XmlDsigXPathTransform");
		CreateFromName ("http://www.w3.org/TR/1999/REC-xpath-19991116", "System.Security.Cryptography.Xml.XmlDsigXPathTransform");
		CreateFromName ("http://www.w3.org/TR/1999/REC-xslt-19991116", "System.Security.Cryptography.Xml.XmlDsigXsltTransform");
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#enveloped-signature", "System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform");
		// URL used in Reference element 
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#Object", null);
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#Manifest", null);
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#SignatureProperties", null);
		// LAMESPEC: only documentated in ".NET Framework Security" book
		CreateFromName ("http://www.w3.org/2000/09/xmldsig# X509Data", "System.Security.Cryptography.Xml.KeyInfoX509Data");
		CreateFromName ("http://www.w3.org/2000/09/xmldsig# KeyName", "System.Security.Cryptography.Xml.KeyInfoName");
		CreateFromName ("http://www.w3.org/2000/09/xmldsig# KeyValue/DSAKeyValue", "System.Security.Cryptography.Xml.DSAKeyValue");
		CreateFromName ("http://www.w3.org/2000/09/xmldsig# KeyValue/RSAKeyValue", "System.Security.Cryptography.Xml.RSAKeyValue");
		CreateFromName ("http://www.w3.org/2000/09/xmldsig# RetrievalMethod", "System.Security.Cryptography.Xml.KeyInfoRetrievalMethod");
	}

	// Tests created using "A Layer Man Guide to ASN.1" from RSA, page 19-20
	// Need to find an OID ? goto http://www.alvestrand.no/~hta/objectid/top.html
	static byte[] oidETSI = { 0x06, 0x03, 0x04, 0x00, 0x00 };
	static byte[] oidSHA1 = { 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1A };
	static byte[] oidASN1CharacterModule = { 0x06, 0x04, 0x51, 0x00, 0x00, 0x00 };
	static byte[] oidmd5withRSAEncryption = { 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x04 };

	// LAMESPEC NullReferenceException is thrown (not ArgumentNullException) if parameter is NULL
	public void TestEncodeOID () 
	{
		try {
			byte[] o = CryptoConfig.EncodeOID (null);
		}
		catch (NullReferenceException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("NullReferenceException not thrown: " + e.ToString());
		}
		// OID starts with 0, 1 or 2
		AssertEquals ("OID starting with 0.", oidETSI, CryptoConfig.EncodeOID ("0.4.0.0"));
		AssertEquals ("OID starting with 1.", oidSHA1, CryptoConfig.EncodeOID ("1.3.14.3.2.26"));
		AssertEquals ("OID starting with 2.", oidASN1CharacterModule, CryptoConfig.EncodeOID ("2.1.0.0.0"));
		// OID numbers can span multiple bytes
		AssertEquals ("OID with numbers spanning multiple bytes", oidmd5withRSAEncryption, CryptoConfig.EncodeOID ("1.2.840.113549.1.1.4"));
		
		// "ms"-invalid OID - greater than 127 bytes (length encoding)
		// OID longer than 127 bytes (so length must be encoded on multiple bytes)
		// LAMESPEC: OID greater that 0x7F (127) bytes aren't supported by the MS Framework
		string baseOID = "1.3.6.1.4.1.11071.0.";
		string lastPart = "1111111111"; // must fit in int32
		for (int i = 1; i < 30; i++)
		{
			baseOID += lastPart + ".";
		}
		baseOID += "0";
		try {
			byte[] tooLongOID = CryptoConfig.EncodeOID (baseOID);
		}
		catch (CryptographicUnexpectedOperationException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("CryptographicUnexpectedOperationException not thrown: " + e.ToString());
		}
		
		// "ms"-invalid OID - where a number of the OID > Int32
		// LAMESPEC: OID with numbers > Int32 aren't supported by the MS BCL
		try {
			byte[] tooLongOID = CryptoConfig.EncodeOID ("1.1.4294967295");
		}
		catch (OverflowException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail( "OverflowException not thrown: " + e.ToString ());
		}

		// invalid OID - must start with 0, 1 or 2
		// however it works with MS BCL
		byte[] oid3 = CryptoConfig.EncodeOID ("3.0");
		byte[] res3 = { 0x06, 0x01, 0x78 };
		AssertEquals ("OID: 3.0", res3, oid3);

		// invalid OID - must have at least 2 parts (according to X.208)
		try {
			byte[] tooShortOID = CryptoConfig.EncodeOID ("0");
		}
		catch (CryptographicUnexpectedOperationException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail("CryptographicUnexpectedOperationException not thrown: " + e.ToString());
		}

		// invalid OID - second value < 40 for 0. and 1. (modulo 40)
		// however it works with MS BCL
		byte[] tooBigSecondPartOID = CryptoConfig.EncodeOID ("0.40");
		byte[] tooBigSecondPartRes = { 0x06, 0x01, 0x28 };
		AssertEquals ("OID: 0.40", tooBigSecondPartRes, tooBigSecondPartOID);
	}

	private void MapNameToOID (string name, string oid)
	{
		AssertEquals ("oid(" + name + ")", oid, CryptoConfig.MapNameToOID (name));
	}

	// LAMESPEC: doesn't support all names defined in CryptoConfig 
	// non supported names (in MSFW) are commented or null-ed
	// LAMESPEC: undocumented but full class name is supported
	public void TestMapNameToOID() 
	{
		try {
			CryptoConfig.MapNameToOID (null);
		}
		catch (ArgumentNullException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail( "ArgumentNullException not thrown: " + e.ToString ());
		}
//		MapNameToOID ("SHA", "1.3.14.3.2.26");
		MapNameToOID ("SHA1", "1.3.14.3.2.26");
		MapNameToOID ("System.Security.Cryptography.SHA1", "1.3.14.3.2.26");
//		MapNameToOID ("System.Security.Cryptography.HashAlgorithm", "1.3.14.3.2.26");
		MapNameToOID ("System.Security.Cryptography.SHA1CryptoServiceProvider", "1.3.14.3.2.26");
		MapNameToOID ("System.Security.Cryptography.SHA1Managed", "1.3.14.3.2.26");
		MapNameToOID ("MD5", "1.2.840.113549.2.5");
		MapNameToOID ("System.Security.Cryptography.MD5", "1.2.840.113549.2.5");
		MapNameToOID ("System.Security.Cryptography.MD5CryptoServiceProvider", "1.2.840.113549.2.5");
		MapNameToOID ("SHA256", "2.16.840.1.101.3.4.1");
//		MapNameToOID ("SHA-256", "2.16.840.1.101.3.4.1");
		MapNameToOID ("System.Security.Cryptography.SHA256", "2.16.840.1.101.3.4.1");
		MapNameToOID ("System.Security.Cryptography.SHA256Managed", "2.16.840.1.101.3.4.1");
		MapNameToOID ("SHA384", "2.16.840.1.101.3.4.2");
//		MapNameToOID ("SHA-384", "2.16.840.1.101.3.4.2");
		MapNameToOID ("System.Security.Cryptography.SHA384", "2.16.840.1.101.3.4.2");
		MapNameToOID ("System.Security.Cryptography.SHA384Managed", "2.16.840.1.101.3.4.2");
		MapNameToOID ("SHA512", "2.16.840.1.101.3.4.3");
//		MapNameToOID ("SHA-512", "2.16.840.1.101.3.4.3");
		MapNameToOID ("System.Security.Cryptography.SHA512", "2.16.840.1.101.3.4.3");
		MapNameToOID ("System.Security.Cryptography.SHA512Managed", "2.16.840.1.101.3.4.3");
		// LAMESPEC: only documentated in ".NET Framework Security" book
		MapNameToOID ("TripleDESKeyWrap", "1.2.840.113549.1.9.16.3.6");
		// no OID defined ?
		MapNameToOID ("RSA", null);
		MapNameToOID ("System.Security.Cryptography.RSA", null);
		MapNameToOID ("System.Security.Cryptography.AsymmetricAlgorithm", null);
		MapNameToOID ("DSA", null);
		MapNameToOID ("System.Security.Cryptography.DSA", null);
		MapNameToOID ("DES", null);
		MapNameToOID ("System.Security.Cryptography.DES", null);
		MapNameToOID ("3DES", null);
		MapNameToOID ("TripleDES", null);
		MapNameToOID ("Triple DES", null);
		MapNameToOID ("System.Security.Cryptography.TripleDES", null);
		MapNameToOID ("RC2", null);
		MapNameToOID ("System.Security.Cryptography.RC2", null);
		MapNameToOID ("Rijndael", null);
		MapNameToOID ("System.Security.Cryptography.Rijndael", null);
		MapNameToOID ("System.Security.Cryptography.SymmetricAlgorithm", null);
		// LAMESPEC Undocumented Names in CryptoConfig
		MapNameToOID ("RandomNumberGenerator", null);
		MapNameToOID ("System.Security.Cryptography.RandomNumberGenerator", null);
		MapNameToOID ("System.Security.Cryptography.KeyedHashAlgorithm", null);
		MapNameToOID ("HMACSHA1", null);
		MapNameToOID ("System.Security.Cryptography.HMACSHA1", null);
		MapNameToOID ("MACTripleDES", null);
		MapNameToOID ("System.Security.Cryptography.MACTripleDES", null);
		// non existing algo should return null (without exception)
		MapNameToOID ("NonExistingAlgorithm", null);
	}

	public void TestToString() 
	{
		// under normal circumstance there are no need to create a CryptoConfig object
		// because all interesting stuff are in static methods
		CryptoConfig cc = new CryptoConfig ();
		AssertEquals ("System.Security.Cryptography.CryptoConfig", cc.ToString ());
	}
}

}
