//
// SignatureTest.cs - NUnit Test Cases for SignedXml
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace MonoTests.System.Security.Cryptography.Xml {

public class SignatureTest : TestCase {

	public SignatureTest () : base ("System.Security.Cryptography.Xml.Signature testsuite") {}
	public SignatureTest (string name) : base (name) {}

	protected Signature signature;

	protected override void SetUp () 
	{
		signature = new Signature ();
	}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (SignatureTest)); 
		}
	}

	public void TestSignature () 
	{
		XmlElement xel = null;
		// empty - missing SignedInfo
		try {
			xel = signature.GetXml ();
			Fail ("Expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("Expected CryptographicException but got :" + e.ToString ());
		}

		SignedInfo info = new SignedInfo ();
		signature.SignedInfo = info;
		info.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
		signature.SignatureValue = new byte [128];
		try {
			xel = signature.GetXml ();
		}
		catch (Exception e) {
			Fail ("Expected ... but got :" + e.ToString ());
		}
	}

	public void TestLoad () 
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