//
// EncryptedKeyTest.cs - NUnit Test Cases for EncryptedKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class EncryptedKeyTest : Assertion {

		public const string key = "<RSAKeyValue><Modulus>4o+Yqa3y34yOy+55ssgAALoxeVOutACyONctHoLCVxsbHBafJcnxxStlObm1B7aZnR5/ge6YTAPrboUTbpkBJfZ5bRb3jBoYl45tWmM0WDAth1MFO3gxUw6PZXxTK85ef2kPZ2jYPX6a0pA5bMWWuyA4y0sg/gv5RG0GrBn1zLk=</Modulus><Exponent>AQAB</Exponent><P>/DPHYnDUBmnKES9gv89w3UUmcSG7HuM6u1ZW7N0uoG2H6toF6M37RPs2DBEIwqNqr96VSyMhgDcvWXDHyiKhoQ==</P><Q>5fj41n0S3h513mdZF+ZLsg1/qQ1StUgRWGCNPI7/lZfteUOVQJPxKokXLoVRtf1RrsnGyVYIDPQKb+OZ8h2EGQ==</Q><DP>HkcpHMxm6A4zUDTb7Ks+5ZGzt8lQ6bhmCK+o5+719hSwSKW8J+Ly06lFJ9Wzs4pi+JJEYizpjLcTXC2KDt5xgQ==</DP><DQ>QyGxqhMqvdJQgBLVLCfa8ugD2xp7iVW3UoAk2oe3zjhiZyK2X/qPzOXl8XuThbej740RTVai1P0sSss69jVtiQ==</DQ><InverseQ>GAhDRgTDIbUUCZmwVv2tq4J7Iyktwsgpp3Hpa6PHM06tA1XCcOzeTV6H9m2+JCJjakcc8V4p2jl7kBzt+P3txQ==</InverseQ><D>No4HX8xwMF5jQD4DdgZs4b/0C9gXuGZ2g/NkfPVVoK35QDE+T1YqbqT9cgIC0oGoycm/QGVYrO1AstM+k6IqQSrWL8YjonGceUzZN71/VjBMpHEQOdtGTEX4/mMdFjGqNxxrsRgTAH/Wl3LAmraHgbnQcsdBcIy8RpmWZAveowE=</D></RSAKeyValue>";

		public const string xml = "<xenc:EncryptedKey Type=\"http://www.w3.org/2001/04/xmlenc#EncryptedKey\" xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\"><xenc:EncryptionMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#rsa-1_5\" /><KeyInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><wsse:SecurityTokenReference><wsse:KeyIdentifier ValueType=\"wsse:X509v3\">MfB7IuMVrXzv++amAm93Tc0Hbas=</wsse:KeyIdentifier></wsse:SecurityTokenReference></KeyInfo><xenc:CipherData><xenc:CipherValue>1WfgPiTV7bpIOCtyKhmYXeTWvl7rykYoeaAqASh6iHQwhS7M61QHEOHw/wS4iphjBGVXsYcNUIFlpZEAkQalxeqLGMJHKkz5Mhd2Ee4N0DWfHlRz5hR7cnwxMKfo/MfzexPfLRcbDuE5iGrYDAb58XQPN3dHLmhCCK+kQ/4KH+E=</xenc:CipherValue></xenc:CipherData><xenc:ReferenceList><xenc:DataReference URI=\"#EncryptedContent-14dac16d-84e9-42bb-aeba-4030da7986d9\" /></xenc:ReferenceList></xenc:EncryptedKey><xenc:EncryptedData Id=\"EncryptedContent-14dac16d-84e9-42bb-aeba-4030da7986d9\" Type=\"http://www.w3.org/2001/04/xmlenc#Content\" xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\"><xenc:EncryptionMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#tripledes-cbc\" /><xenc:CipherData><xenc:CipherValue>A4L9y9NICU8KhOF3ip323wucnpUXbM1De0AOuMaQ2kFRG4wUjw2Wo/u14kQts5uGPNtYWI8/bsBunRr4JVfbMANV+/bPZ3d3v++J+5lrrvvBAaxBu+bCZDwZK45Li/fD</xenc:CipherValue></xenc:CipherData></xenc:EncryptedData>";

		private AsymmetricEncryptionKey GetKey () 
		{
			RSA rsa = RSA.Create ();
			rsa.FromXmlString (key);
			AsymmetricEncryptionKey aek = new AsymmetricEncryptionKey (rsa);
			aek.KeyInfo.AddClause (new RSAKeyValue (rsa));
			return aek;
		}

		[Test]
		public void ConstructorAsymmetricEncryptionKey () 
		{
			EncryptedKey ek = new EncryptedKey (GetKey ());
			AssertNotNull ("EncryptedKey(AsymmetricEncryptionKey)", ek);
			// check default
			AssertEquals ("EncryptionMethod", XmlEncryption.AlgorithmURI.RSA15, ek.EncryptionMethod);
			AssertEquals ("SessionAlgorithmURI", XmlEncryption.AlgorithmURI.TripleDES, ek.SessionAlgorithmURI);
			AssertNotNull ("KeyInfo", ek.KeyInfo);
			Assert ("KeyInfo.Count > 0", (ek.KeyInfo.Count > 0));
			AssertNotNull ("ReferenceList", ek.ReferenceList);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ConstructorAsymmetricEncryptionKeyNull () 
		{
			AsymmetricEncryptionKey aek = null;
			EncryptedKey ek = new EncryptedKey (aek);
		}

		[Test]
		[Ignore("IDecryptionKeyProvider")]
		public void ConstructorXmlElement () 
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			EncryptedKey ek = new EncryptedKey (doc.DocumentElement);
			AssertNotNull ("EncryptedKey(XmlElement)", ek);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void ConstructorXmlElementNull () 
		{
			XmlElement xel = null;
			EncryptedKey ek = new EncryptedKey (xel);
		}

		[Test]
		public void SessionAlgorithmURI_AES128 () 
		{
			EncryptedKey ek = new EncryptedKey (GetKey ());
			ek.SessionAlgorithmURI = XmlEncryption.AlgorithmURI.AES128;
			AssertEquals ("SessionAlgorithmURI==AES128", XmlEncryption.AlgorithmURI.AES128, ek.SessionAlgorithmURI);
		}

		[Test]
		public void SessionAlgorithmURI_AES192 () 
		{
			EncryptedKey ek = new EncryptedKey (GetKey ());
			ek.SessionAlgorithmURI = XmlEncryption.AlgorithmURI.AES192;
			AssertEquals ("SessionAlgorithmURI==AES192", XmlEncryption.AlgorithmURI.AES192, ek.SessionAlgorithmURI);
		}

		[Test]
		public void SessionAlgorithmURI_AES256 () 
		{
			EncryptedKey ek = new EncryptedKey (GetKey ());
			ek.SessionAlgorithmURI = XmlEncryption.AlgorithmURI.AES256;
			AssertEquals ("SessionAlgorithmURI==AES256", XmlEncryption.AlgorithmURI.AES256, ek.SessionAlgorithmURI);
		}

		[Test]
		public void SessionAlgorithmURI_TripleDES () 
		{
			EncryptedKey ek = new EncryptedKey (GetKey ());
			ek.SessionAlgorithmURI = XmlEncryption.AlgorithmURI.TripleDES;
			AssertEquals ("SessionAlgorithmURI==TripleDES", XmlEncryption.AlgorithmURI.TripleDES, ek.SessionAlgorithmURI);
		}

		private void UnsupportedAlgorithm (EncryptedKey ek, string algorithm) 
		{
			try {
				ek.SessionAlgorithmURI = algorithm;
				Fail ("expected SecurityFault but got none");
			}
			catch (Exception e) {
				if (!e.ToString ().StartsWith ("Microsoft.Web.Services.Security.SecurityFault"))
					Fail ("expected SecurityFault but got " + e.ToString ());
			}
		}

		[Test]
		public void SessionAlgorithmURI_Unsupported () 
		{
			EncryptedKey ek = new EncryptedKey (GetKey ());
			UnsupportedAlgorithm (ek, XmlEncryption.AlgorithmURI.AES128KeyWrap);
			UnsupportedAlgorithm (ek, XmlEncryption.AlgorithmURI.AES192KeyWrap);
			UnsupportedAlgorithm (ek, XmlEncryption.AlgorithmURI.AES256KeyWrap);
			// strangely DES is defined but unsupported
			UnsupportedAlgorithm (ek, XmlEncryption.AlgorithmURI.DES);
			UnsupportedAlgorithm (ek, XmlEncryption.AlgorithmURI.RSA15);
			UnsupportedAlgorithm (ek, XmlEncryption.AlgorithmURI.RSAOAEP);
			UnsupportedAlgorithm (ek, XmlEncryption.AlgorithmURI.SHA1);
			UnsupportedAlgorithm (ek, XmlEncryption.AlgorithmURI.SHA256);
			UnsupportedAlgorithm (ek, XmlEncryption.AlgorithmURI.SHA512);
			UnsupportedAlgorithm (ek, XmlEncryption.AlgorithmURI.TripleDESKeyWrap);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void SessionAlgorithmURI_Null () 
		{
			EncryptedKey ek = new EncryptedKey (GetKey ());
			ek.SessionAlgorithmURI = null;
		}

		[Test]
		public void GetXml () 
		{
			EncryptedKey ek = new EncryptedKey (GetKey ());
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = ek.GetXml (doc);
			// output will always be different (new key each time)
			AssertNotNull ("GetXml", xel);
			// TODO: more
		}

		[Test]
		[Ignore("IDecryptionKeyProvider")]
		public void GetLoadXmlRoundtrip () 
		{
			EncryptedKey ek = new EncryptedKey (GetKey ());
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = ek.GetXml (doc);
			// output will always be different (new key each time)
			AssertNotNull ("GetXml", xel);
			ek.LoadXml (xel);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void GetXmlNull () 
		{
			EncryptedKey ek = new EncryptedKey (GetKey ());
			ek.GetXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))] 
		public void LoadXmlNull () 
		{
			XmlElement xel = null;
			EncryptedKey ek = new EncryptedKey (xel);
			ek.LoadXml (null);
		}
	}
}