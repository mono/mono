//
// SignatureTest.cs - NUnit Test Cases for SignedXml
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.Xml {

	[TestFixture]
	public class SignatureTest : Assertion {

		protected Signature signature;

		[SetUp]
		protected void SetUp () 
		{
			signature = new Signature ();
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Signature1 () 
		{
			// empty - missing SignedInfo
			XmlElement xel = signature.GetXml ();
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Signature2 () 
		{
			SignedInfo info = new SignedInfo ();
			signature.SignedInfo = info;
			info.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
			signature.SignatureValue = new byte [128];
			// no reference element are present
			XmlElement xel = signature.GetXml ();
		}

		[Test]
		public void Load () 
		{
			string value = "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" /><Reference URI=\"#MyObjectId\"><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>/Vvq6sXEVbtZC8GwNtLQnGOy/VI=</DigestValue></Reference></SignedInfo><SignatureValue>A6XuE8Cy9iOffRXaW9b0+dUcMUJQnlmwLsiqtQnADbCtZXnXAaeJ6nGnQ4Mm0IGi0AJc7/2CoJReXl7iW4hltmFguG1e3nl0VxCyCTHKGOCo1u8R3K+B1rTaenFbSxs42EM7/D9KETsPlzfYfis36yM3PqatiCUOsoMsAiMGzlc=</SignatureValue><KeyInfo><KeyValue xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><RSAKeyValue><Modulus>tI8QYIpbG/m6JLyvP+S3X8mzcaAIayxomyTimSh9UCpEucRnGvLw0P73uStNpiF7wltTZA1HEsv+Ha39dY/0j/Wiy3RAodGDRNuKQao1wu34aNybZ673brbsbHFUfw/o7nlKD2xO84fbajBZmKtBBDy63NHt+QL+grSrREPfCTM=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></KeyInfo><Object Id=\"MyObjectId\"><MyElement xmlns=\"samples\">This is some text</MyElement></Object></Signature>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (value);
			signature.LoadXml (doc.DocumentElement);
			string s = signature.GetXml ().OuterXml;
			AssertEquals ("Load", value, s);
		}
	}
}
