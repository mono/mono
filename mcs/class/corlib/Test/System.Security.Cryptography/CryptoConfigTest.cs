//
// CryptoConfigTest.cs - NUnit Test Cases for CryptoConfig
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2007,2011 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

[TestFixture]
public class CryptoConfigTest {

	void CreateFromName (string name, string objectname)
	{
		object o = CryptoConfig.CreateFromName (name);
		if (objectname == null)
			Assert.IsNull (o, name);
		else
			Assert.AreEqual (objectname, o.ToString (), name);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void CreateFromNameNull () 
	{
		object o = CryptoConfig.CreateFromName (null);
	}

	// validate that CryptoConfig create the exact same implementation between mono and MS
	[Test]
	public void CreateFromName () 
	{
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

		// new HMAC - new base class doesn't return anything with it's short name
		Assert.IsNull (CryptoConfig.CreateFromName ("HMAC"), "HMAC");
		CreateFromName ("System.Security.Cryptography.HMAC", "System.Security.Cryptography.HMACSHA1");
		CreateFromName ("HMACMD5", "System.Security.Cryptography.HMACMD5");
		CreateFromName ("System.Security.Cryptography.HMACMD5", "System.Security.Cryptography.HMACMD5");
		CreateFromName ("HMACRIPEMD160", "System.Security.Cryptography.HMACRIPEMD160");
		CreateFromName ("System.Security.Cryptography.HMACRIPEMD160", "System.Security.Cryptography.HMACRIPEMD160");
		CreateFromName ("HMACSHA256", "System.Security.Cryptography.HMACSHA256");
		CreateFromName ("System.Security.Cryptography.HMACSHA256", "System.Security.Cryptography.HMACSHA256");
		CreateFromName ("HMACSHA384", "System.Security.Cryptography.HMACSHA384");
		CreateFromName ("System.Security.Cryptography.HMACSHA384", "System.Security.Cryptography.HMACSHA384");
		CreateFromName ("HMACSHA512", "System.Security.Cryptography.HMACSHA512");
		CreateFromName ("System.Security.Cryptography.HMACSHA512", "System.Security.Cryptography.HMACSHA512");
		// new hash algorithm
		CreateFromName ("RIPEMD160", "System.Security.Cryptography.RIPEMD160Managed");
		CreateFromName ("RIPEMD-160", "System.Security.Cryptography.RIPEMD160Managed");
		CreateFromName ("System.Security.Cryptography.RIPEMD160", "System.Security.Cryptography.RIPEMD160Managed");
		// x.509 stuff
#if !MOBILE
		CreateFromName ("X509Chain", "System.Security.Cryptography.X509Certificates.X509Chain");

		// note: CryptoConfig can create any object !
		CreateFromName ("System.Security.Cryptography.CryptoConfig", "System.Security.Cryptography.CryptoConfig");
		CreateFromName ("System.IO.MemoryStream", "System.IO.MemoryStream");
#endif
		// non existing algo should return null (without exception)
		Assert.IsNull (CryptoConfig.CreateFromName ("NonExistingAlgorithm"), "NonExistingAlgorithm");
	}

	// additional names (URL) used for XMLDSIG (System.Security.Cryptography.Xml)
	// URL taken from http://www.w3.org/TR/2002/REC-xmldsig-core-20020212/
	[Test]
#if NET_2_1
	[Ignore ("System.Security.dll is not part of Moonlight, MonoTouch and Mono for Android")]
#endif
	public void CreateFromURL () 
	{
		// URL used in SignatureMethod element
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#dsa-sha1", "System.Security.Cryptography.DSASignatureDescription");
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#rsa-sha1", "System.Security.Cryptography.RSAPKCS1SHA1SignatureDescription");
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#hmac-sha1", null);
		// URL used in DigestMethod element 
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#sha1", "System.Security.Cryptography.SHA1CryptoServiceProvider");
#if !TARGET_JVM //TargetJvmNotWorking - algorithms from System.Security assembly
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
#if NET_2_0
		CreateFromName ("http://www.w3.org/2001/04/xmlenc#sha256", "System.Security.Cryptography.SHA256Managed");
		CreateFromName ("http://www.w3.org/2001/04/xmlenc#sha384", null);
		CreateFromName ("http://www.w3.org/2001/04/xmlenc#sha512", "System.Security.Cryptography.SHA512Managed");

		CreateFromName ("http://www.w3.org/2001/04/xmldsig-more#hmac-sha256", "System.Security.Cryptography.HMACSHA256");
		CreateFromName ("http://www.w3.org/2001/04/xmldsig-more#hmac-sha384", "System.Security.Cryptography.HMACSHA384");
		CreateFromName ("http://www.w3.org/2001/04/xmldsig-more#hmac-sha512", "System.Security.Cryptography.HMACSHA512");
		CreateFromName ("http://www.w3.org/2001/04/xmldsig-more#hmac-ripemd160", "System.Security.Cryptography.HMACRIPEMD160");
#endif
#endif
	}

	[Test]
	public void CreateFromName_UpperCase () 
	{
		CreateFromName ("SHA", "System.Security.Cryptography.SHA1CryptoServiceProvider");
#if NET_2_0
		CreateFromName ("SYSTEM.SECURITY.CRYPTOGRAPHY.TRIPLEDES", "System.Security.Cryptography.TripleDESCryptoServiceProvider");  
		CreateFromName ("HTTP://WWW.W3.ORG/2000/09/XMLDSIG#DSA-SHA1", "System.Security.Cryptography.DSASignatureDescription");
#else
		CreateFromName ("SYSTEM.SECURITY.CRYPTOGRAPHY.TRIPLEDES", null);  
		CreateFromName ("HTTP://WWW.W3.ORG/2000/09/XMLDSIG#DSA-SHA1", null);
#endif
	}

	[Test]
	public void CreateFromName_LowerCase () 
	{
#if NET_2_0
		CreateFromName ("sha", "System.Security.Cryptography.SHA1CryptoServiceProvider");
		CreateFromName ("system.security.cryptography.tripledes", "System.Security.Cryptography.TripleDESCryptoServiceProvider");  
#else
		CreateFromName ("sha", null);
		CreateFromName ("system.security.cryptography.tripledes", null);  
#endif
		CreateFromName ("http://www.w3.org/2000/09/xmldsig#dsa-sha1", "System.Security.Cryptography.DSASignatureDescription");
	}

	[Test]
	public void CreateFromName_MixedCase () 
	{
#if NET_2_0
		CreateFromName ("ShA", "System.Security.Cryptography.SHA1CryptoServiceProvider");
		CreateFromName ("SyStEm.SeCuRiTy.CrYpToGrApHy.TrIpLeDeS", "System.Security.Cryptography.TripleDESCryptoServiceProvider");  
		CreateFromName ("hTtP://wWw.W3.oRg/2000/09/xMlDsIg#dSa-sHa1", "System.Security.Cryptography.DSASignatureDescription");
#else
		CreateFromName ("ShA", null);
		CreateFromName ("SyStEm.SeCuRiTy.CrYpToGrApHy.TrIpLeDeS", null);  
		CreateFromName ("hTtP://wWw.W3.oRg/2000/09/xMlDsIg#dSa-sHa1", null);
#endif
	}

	// Tests created using "A Layer Man Guide to ASN.1" from RSA, page 19-20
	// Need to find an OID ? goto http://www.alvestrand.no/~hta/objectid/top.html
	static byte[] oidETSI = { 0x06, 0x03, 0x04, 0x00, 0x00 };
	static byte[] oidSHA1 = { 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1A };
	static byte[] oidASN1CharacterModule = { 0x06, 0x04, 0x51, 0x00, 0x00, 0x00 };
	static byte[] oidmd5withRSAEncryption = { 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x04 };

	[Test]
#if NET_2_0
	[ExpectedException (typeof (ArgumentNullException))]
#else
	[ExpectedException (typeof (NullReferenceException))]
#endif
	public void EncodeOIDNull () 
	{
		byte[] o = CryptoConfig.EncodeOID (null);
	}

	[Test]
	public void EncodeOID () 
	{
		// OID starts with 0, 1 or 2
		Assert.AreEqual (oidETSI, CryptoConfig.EncodeOID ("0.4.0.0"), "OID starting with 0.");
		Assert.AreEqual (oidSHA1, CryptoConfig.EncodeOID ("1.3.14.3.2.26"), "OID starting with 1.");
		Assert.AreEqual (oidASN1CharacterModule, CryptoConfig.EncodeOID ("2.1.0.0.0"), "OID starting with 2.");
		// OID numbers can span multiple bytes
		Assert.AreEqual (oidmd5withRSAEncryption, CryptoConfig.EncodeOID ("1.2.840.113549.1.1.4"), "OID with numbers spanning multiple bytes");
	}

	[Test]
	[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
	// LAMESPEC: OID greater that 0x7F (127) bytes aren't supported by the MS Framework
	public void EncodeOID_BiggerThan127bytes () 
	{
		// "ms"-invalid OID - greater than 127 bytes (length encoding)
		// OID longer than 127 bytes (so length must be encoded on multiple bytes)
		string baseOID = "1.3.6.1.4.1.11071.0.";
		string lastPart = "1111111111"; // must fit in int32
		for (int i = 1; i < 30; i++) {
			baseOID += lastPart + ".";
		}
		baseOID += "0";
		byte[] tooLongOID = CryptoConfig.EncodeOID (baseOID);
	}
		
	[Test]
	[ExpectedException (typeof (OverflowException))]
	// LAMESPEC: OID with numbers > Int32 aren't supported by the MS BCL
	public void EncodeOID_BiggerThanInt32 () 
	{
		// "ms"-invalid OID - where a number of the OID > Int32
		byte[] tooLongOID = CryptoConfig.EncodeOID ("1.1.4294967295");
	}

	[Test]
	public void EncodeOID_InvalidStart () 
	{
		// invalid OID - must start with 0, 1 or 2
		// however it works with MS BCL
		byte[] oid3 = CryptoConfig.EncodeOID ("3.0");
		byte[] res3 = { 0x06, 0x01, 0x78 };
		Assert.AreEqual (res3, oid3, "OID: 3.0");
	}

	[Test]
	[ExpectedException (typeof (CryptographicUnexpectedOperationException))]
	public void EncodeOID_TooShort () 
	{
		// invalid OID - must have at least 2 parts (according to X.208)
		byte[] tooShortOID = CryptoConfig.EncodeOID ("0");
	}

	[Test]
	public void EncodeOID_InvalidSecondPart () 
	{
		// invalid OID - second value < 40 for 0. and 1. (modulo 40)
		// however it works with MS BCL
		byte[] tooBigSecondPartOID = CryptoConfig.EncodeOID ("0.40");
		byte[] tooBigSecondPartRes = { 0x06, 0x01, 0x28 };
		Assert.AreEqual (tooBigSecondPartRes, tooBigSecondPartOID, "OID: 0.40");
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void MapNameToOIDNull () 
	{
		CryptoConfig.MapNameToOID (null);
	}

	private void MapNameToOID (string name, string oid)
	{
		Assert.AreEqual (oid, CryptoConfig.MapNameToOID (name), "oid(" + name + ")");
	}

	// LAMESPEC: doesn't support all names defined in CryptoConfig 
	// non supported names (in MSFW) are commented or null-ed
	// LAMESPEC: undocumented but full class name is supported
	[Test]
	public void MapNameToOID() 
	{
//		MapNameToOID ("SHA", "1.3.14.3.2.26");
		MapNameToOID ("SHA1", "1.3.14.3.2.26");
		MapNameToOID ("System.Security.Cryptography.SHA1", "1.3.14.3.2.26");
//		MapNameToOID ("System.Security.Cryptography.HashAlgorithm", "1.3.14.3.2.26");
		MapNameToOID ("System.Security.Cryptography.SHA1CryptoServiceProvider", "1.3.14.3.2.26");
		MapNameToOID ("System.Security.Cryptography.SHA1Managed", "1.3.14.3.2.26");
		MapNameToOID ("MD5", "1.2.840.113549.2.5");
		MapNameToOID ("System.Security.Cryptography.MD5", "1.2.840.113549.2.5");
		MapNameToOID ("System.Security.Cryptography.MD5CryptoServiceProvider", "1.2.840.113549.2.5");
#if NET_2_0
		MapNameToOID ("SHA256", "2.16.840.1.101.3.4.2.1");
		MapNameToOID ("System.Security.Cryptography.SHA256", "2.16.840.1.101.3.4.2.1");
		MapNameToOID ("System.Security.Cryptography.SHA256Managed", "2.16.840.1.101.3.4.2.1");
		MapNameToOID ("SHA384", "2.16.840.1.101.3.4.2.2");
		MapNameToOID ("System.Security.Cryptography.SHA384", "2.16.840.1.101.3.4.2.2");
		MapNameToOID ("System.Security.Cryptography.SHA384Managed", "2.16.840.1.101.3.4.2.2");
		MapNameToOID ("SHA512", "2.16.840.1.101.3.4.2.3");
		MapNameToOID ("System.Security.Cryptography.SHA512", "2.16.840.1.101.3.4.2.3");
		MapNameToOID ("System.Security.Cryptography.SHA512Managed", "2.16.840.1.101.3.4.2.3");
		MapNameToOID ("RIPEMD160", "1.3.36.3.2.1");
		MapNameToOID ("System.Security.Cryptography.RIPEMD160", "1.3.36.3.2.1");
		MapNameToOID ("System.Security.Cryptography.RIPEMD160Managed", "1.3.36.3.2.1");
#else
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
#endif
		// LAMESPEC: only documentated in ".NET Framework Security" book
		MapNameToOID ("TripleDESKeyWrap", "1.2.840.113549.1.9.16.3.6");
#if NET_2_0
		// new OID defined in Fx 2.0
		MapNameToOID ("DES", "1.3.14.3.2.7");
		MapNameToOID ("TripleDES", "1.2.840.113549.3.7");
		MapNameToOID ("RC2", "1.2.840.113549.3.2");
#else
		// no OID defined before Fx 2.0
		MapNameToOID ("DES", null);
		MapNameToOID ("TripleDES", null);
		MapNameToOID ("RC2", null);
#endif
		MapNameToOID ("RSA", null);
		MapNameToOID ("DSA", null);
		MapNameToOID ("3DES", null);
		// no OID defined ?
		MapNameToOID ("System.Security.Cryptography.RSA", null);
		MapNameToOID ("System.Security.Cryptography.AsymmetricAlgorithm", null);
		MapNameToOID ("System.Security.Cryptography.DSA", null);
		MapNameToOID ("System.Security.Cryptography.DES", null);
		MapNameToOID ("Triple DES", null);
		MapNameToOID ("System.Security.Cryptography.TripleDES", null);
		MapNameToOID ("System.Security.Cryptography.RC2", null);
		MapNameToOID ("Rijndael", null);
		MapNameToOID ("System.Security.Cryptography.Rijndael", null);
		MapNameToOID ("System.Security.Cryptography.SymmetricAlgorithm", null);
		// LAMESPEC Undocumented Names in CryptoConfig
		MapNameToOID ("RandomNumberGenerator", null);
		MapNameToOID ("System.Security.Cryptography.RandomNumberGenerator", null);
		MapNameToOID ("System.Security.Cryptography.KeyedHashAlgorithm", null);
#if NET_2_0
		MapNameToOID ("HMAC", null);
		MapNameToOID ("System.Security.Cryptography.HMAC", null);
		MapNameToOID ("HMACMD5", null);
		MapNameToOID ("System.Security.Cryptography.HMACMD5", null);
		MapNameToOID ("HMACRIPEMD160", null);
		MapNameToOID ("System.Security.Cryptography.HMACRIPEMD160", null);
		MapNameToOID ("HMACSHA256", null);
		MapNameToOID ("System.Security.Cryptography.HMACSHA256", null);
		MapNameToOID ("HMACSHA384", null);
		MapNameToOID ("System.Security.Cryptography.HMACSHA384", null);
		MapNameToOID ("HMACSHA512", null);
		MapNameToOID ("System.Security.Cryptography.HMACSHA512", null);
#endif
		MapNameToOID ("HMACSHA1", null);
		MapNameToOID ("System.Security.Cryptography.HMACSHA1", null);
		MapNameToOID ("MACTripleDES", null);
		MapNameToOID ("System.Security.Cryptography.MACTripleDES", null);
		// non existing algo should return null (without exception)
		MapNameToOID ("NonExistingAlgorithm", null);
	}

	[Test]
	public void MapNameToOID_UpperCase () 
	{
		MapNameToOID ("SHA1", "1.3.14.3.2.26");
#if NET_2_0
		MapNameToOID ("SYSTEM.SECURITY.CRYPTOGRAPHY.MD5CRYPTOSERVICEPROVIDER", "1.2.840.113549.2.5");
#else
		MapNameToOID ("SYSTEM.SECURITY.CRYPTOGRAPHY.MD5CRYPTOSERVICEPROVIDER", null);
#endif
	}

	[Test]
	public void MapNameToOID_LowerCase () 
	{
#if NET_2_0
		MapNameToOID ("sha1", "1.3.14.3.2.26");
		MapNameToOID ("system.security.cryptography.md5cryptoserviceprovider", "1.2.840.113549.2.5");
#else
		MapNameToOID ("sha1", null);
		MapNameToOID ("system.security.cryptography.md5cryptoserviceprovider", null);
#endif
	}

	[Test]
	public void MapNameToOID_MixedCase () 
	{
#if NET_2_0
		MapNameToOID ("sHa1", "1.3.14.3.2.26");
		MapNameToOID ("SySteM.SeCuRiTy.CrYpToGrApHy.Md5cRyPtOsErViCePrOvIdEr", "1.2.840.113549.2.5");
#else
		MapNameToOID ("sHa1", null);
		MapNameToOID ("SySteM.SeCuRiTy.CrYpToGrApHy.Md5cRyPtOsErViCePrOvIdEr", null);
#endif
	}

	[Test]
	public void CCToString () 
	{
		// under normal circumstance there are no need to create a CryptoConfig object
		// because all interesting stuff are in static methods
		CryptoConfig cc = new CryptoConfig ();
		Assert.AreEqual ("System.Security.Cryptography.CryptoConfig", cc.ToString ());
	}
}

}
