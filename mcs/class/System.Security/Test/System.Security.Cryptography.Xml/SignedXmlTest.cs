//
// SignedXmlTest.cs - NUnit Test Cases for SignedXml
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
using System.Text;
using System.Xml;

namespace MonoTests.System.Security.Cryptography.Xml {

public class SignedXmlTest : TestCase {

	public SignedXmlTest () : base ("System.Security.Cryptography.Xml.SignedXml testsuite") {}
	public SignedXmlTest (string name) : base (name) {}

	protected override void SetUp () {}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (SignedXmlTest)); 
		}
	}

	public void TestStatic () 
	{
		AssertEquals ("XmlDsigCanonicalizationUrl", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315", SignedXml.XmlDsigCanonicalizationUrl);
		AssertEquals ("XmlDsigCanonicalizationWithCommentsUrl", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments", SignedXml.XmlDsigCanonicalizationWithCommentsUrl);
		AssertEquals ("XmlDsigDSAUrl", "http://www.w3.org/2000/09/xmldsig#dsa-sha1", SignedXml.XmlDsigDSAUrl);
		AssertEquals ("XmlDsigHMACSHA1Url", "http://www.w3.org/2000/09/xmldsig#hmac-sha1", SignedXml.XmlDsigHMACSHA1Url);
		AssertEquals ("XmlDsigMinimalCanonicalizationUrl", "http://www.w3.org/2000/09/xmldsig#minimal", SignedXml.XmlDsigMinimalCanonicalizationUrl);
		AssertEquals ("XmlDsigNamespaceUrl", "http://www.w3.org/2000/09/xmldsig#", SignedXml.XmlDsigNamespaceUrl);
		AssertEquals ("XmlDsigRSASHA1Url", "http://www.w3.org/2000/09/xmldsig#rsa-sha1", SignedXml.XmlDsigRSASHA1Url);
		AssertEquals ("XmlDsigSHA1Url", "http://www.w3.org/2000/09/xmldsig#sha1", SignedXml.XmlDsigSHA1Url);
	}

	// sample from MSDN (url)
	public SignedXml MSDNSample () 
	{
		// Create example data to sign.
		XmlDocument document = new XmlDocument ();
		XmlNode node = document.CreateNode (XmlNodeType.Element, "", "MyElement", "samples");
		node.InnerText = "This is some text";
		document.AppendChild (node);
 
		// Create the SignedXml message.
		SignedXml signedXml = new SignedXml ();
 
		// Create a data object to hold the data to sign.
		DataObject dataObject = new DataObject ();
		dataObject.Data = document.ChildNodes;
		dataObject.Id = "MyObjectId";

		// Add the data object to the signature.
		signedXml.AddObject (dataObject);
 
		// Create a reference to be able to package everything into the
		// message.
		Reference reference = new Reference ();
		reference.Uri = "#MyObjectId";
 
		// Add it to the message.
		signedXml.AddReference (reference);

		return signedXml;
	}

	public void TestAsymmetricRSASignature () 
	{
		SignedXml signedXml = MSDNSample ();

		RSA key = RSA.Create ();
		signedXml.SigningKey = key;

		// Add a KeyInfo.
		KeyInfo keyInfo = new KeyInfo ();
		keyInfo.AddClause (new RSAKeyValue (key));
		signedXml.KeyInfo = keyInfo;

		// Compute the signature.
		signedXml.ComputeSignature ();

		// Get the XML representation of the signature.
		XmlElement xmlSignature = signedXml.GetXml ();

		// LAMESPEC: we must reload the signature or it won't work
		// MS framework throw a "malformed element"
		SignedXml vrfy = new SignedXml ();
		vrfy.LoadXml (xmlSignature);

		// assert that we can verify our own signature
		Assert ("RSA-Compute/Verify", vrfy.CheckSignature ());
	}

	public void TestAsymmetricDSASignature () 
	{
		SignedXml signedXml = MSDNSample ();

		DSA key = DSA.Create ();
		signedXml.SigningKey = key;
 
		// Add a KeyInfo.
		KeyInfo keyInfo = new KeyInfo ();
		keyInfo.AddClause (new DSAKeyValue (key));
		signedXml.KeyInfo = keyInfo;

		// Compute the signature.
		signedXml.ComputeSignature ();

		// Get the XML representation of the signature.
		XmlElement xmlSignature = signedXml.GetXml ();

		// LAMESPEC: we must reload the signature or it won't work
		// MS framework throw a "malformed element"
		SignedXml vrfy = new SignedXml ();
		vrfy.LoadXml (xmlSignature);

		// assert that we can verify our own signature
		Assert ("DSA-Compute/Verify", vrfy.CheckSignature ());
	}

	public void TestSymmetricHMACSHA1Signature () 
	{
		SignedXml signedXml = MSDNSample ();

		// Compute the signature.
		byte[] secretkey = Encoding.Default.GetBytes ("password");
		HMACSHA1 hmac = new HMACSHA1 (secretkey);
		signedXml.ComputeSignature (hmac);

		// Get the XML representation of the signature.
		XmlElement xmlSignature = signedXml.GetXml ();

		// LAMESPEC: we must reload the signature or it won't work
		// MS framework throw a "malformed element"
		SignedXml vrfy = new SignedXml ();
		vrfy.LoadXml (xmlSignature);

		// assert that we can verify our own signature
		Assert ("HMACSHA1-Compute/Verify", vrfy.CheckSignature (hmac));
	}

	public void TestSymmetricMACTripleDESSignature () 
	{
		SignedXml signedXml = MSDNSample ();

		// Compute the signature.
		byte[] secretkey = Encoding.Default.GetBytes ("password");
		MACTripleDES hmac = new MACTripleDES (secretkey);
		try {
			signedXml.ComputeSignature (hmac);
			Fail ("Expected CryptographicException but none");
		}
		catch (CryptographicException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("Expected CryptographicException but got: " + e.ToString ());
		}
	}

	// Using empty constructor
	// LAMESPEC: The two other constructors don't seems to apply in verifying signatures
	public void TestAsymmetricRSAVerify () 
	{
		string value = "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" /><Reference URI=\"#MyObjectId\"><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>/Vvq6sXEVbtZC8GwNtLQnGOy/VI=</DigestValue></Reference></SignedInfo><SignatureValue>A6XuE8Cy9iOffRXaW9b0+dUcMUJQnlmwLsiqtQnADbCtZXnXAaeJ6nGnQ4Mm0IGi0AJc7/2CoJReXl7iW4hltmFguG1e3nl0VxCyCTHKGOCo1u8R3K+B1rTaenFbSxs42EM7/D9KETsPlzfYfis36yM3PqatiCUOsoMsAiMGzlc=</SignatureValue><KeyInfo><KeyValue xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><RSAKeyValue><Modulus>tI8QYIpbG/m6JLyvP+S3X8mzcaAIayxomyTimSh9UCpEucRnGvLw0P73uStNpiF7wltTZA1HEsv+Ha39dY/0j/Wiy3RAodGDRNuKQao1wu34aNybZ673brbsbHFUfw/o7nlKD2xO84fbajBZmKtBBDy63NHt+QL+grSrREPfCTM=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></KeyInfo><Object Id=\"MyObjectId\"><MyElement xmlns=\"samples\">This is some text</MyElement></Object></Signature>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);

		SignedXml v1 = new SignedXml ();
		v1.LoadXml (doc.DocumentElement);
		Assert ("RSA-CheckSignature()", v1.CheckSignature ());

		SignedXml v2 = new SignedXml ();
		v2.LoadXml (doc.DocumentElement);
		AsymmetricAlgorithm key = null;
		bool vrfy = v2.CheckSignatureReturningKey (out key);
		Assert ("RSA-CheckSignatureReturningKey()", vrfy);

		SignedXml v3 = new SignedXml ();
		v3.LoadXml (doc.DocumentElement);
		Assert ("RSA-CheckSignature(key)", v3.CheckSignature (key));
	}

	// Using empty constructor
	// LAMESPEC: The two other constructors don't seems to apply in verifying signatures
	public void TestAsymmetricDSAVerify () 
	{
		string value = "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#dsa-sha1\" /><Reference URI=\"#MyObjectId\"><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>/Vvq6sXEVbtZC8GwNtLQnGOy/VI=</DigestValue></Reference></SignedInfo><SignatureValue>BYz/qRGjGsN1yMFPxWa3awUZm1y4I/IxOQroMxkOteRGgk1HIwhRYw==</SignatureValue><KeyInfo><KeyValue xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><DSAKeyValue><P>iglVaZ+LsSL8Y0aDXmFMBwva3xHqIypr3l/LtqBH9ziV2Sh1M4JVasAiKqytWIWt/s/Uk8Ckf2tO2Ww1vsNi1NL+Kg9T7FE52sn380/rF0miwGkZeidzm74OWhykb3J+wCTXaIwOzAWI1yN7FoeoN7wzF12jjlSXAXeqPMlViqk=</P><Q>u4sowiJMHilNRojtdmIuQY2YnB8=</Q><G>SdnN7d+wn1n+HH4Hr8MIryIRYgcXdbZ5TH7jAnuWc1koqRc1AZfcYAZ6RDf+orx6Lzn055FTFiN+1NHQfGUtXJCWW0zz0FVV1NJux7WRj8vGTldjJ5ef0oCenkpwDjcIxWsZgVobve4GPoyN1sAc1scnkJB59oupibklmF4y72A=</G><Y>XejzS8Z51yfl0zbYnxSYYbHqreSLjNCoGPB/KjM1TOyV5sMjz0StKtGrFWryTWc7EgvFY7kUth4e04VKf9HbK8z/FifHTXj8+Tszbjzw8GfInnBwLN+vJgbpnjtypmiI5Bm2nLiRbfkdAHP+OrKtr/EauM9GQfYuaxm3/Vj8B84=</Y><J>vGwGg9wqwwWP9xsoPoXu6kHArJtadiNKe9azBiUx5Ob883gd5wlKfEcGuKkBmBySGbgwxyOsIBovd9Kk48hF01ymfQzAAuHR0EdJECSsTsTTKVTLQNBU32O+PRbLYpv4E8kt6rNL83JLJCBY</J><Seed>sqzn8J6fd2gtEyq6YOqiUSHgPE8=</Seed><PgenCounter>sQ==</PgenCounter></DSAKeyValue></KeyValue></KeyInfo><Object Id=\"MyObjectId\"><MyElement xmlns=\"samples\">This is some text</MyElement></Object></Signature>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);

		SignedXml v1 = new SignedXml ();
		v1.LoadXml (doc.DocumentElement);
		Assert ("DSA-CheckSignature()", v1.CheckSignature ());

		SignedXml v2 = new SignedXml ();
		v2.LoadXml (doc.DocumentElement);
		AsymmetricAlgorithm key = null;
		bool vrfy = v2.CheckSignatureReturningKey (out key);
		Assert ("DSA-CheckSignatureReturningKey()", vrfy);

		SignedXml v3 = new SignedXml ();
		v3.LoadXml (doc.DocumentElement);
		Assert ("DSA-CheckSignature(key)", v3.CheckSignature (key));
	}

	public void TestSymmetricHMACSHA1Verify () 
	{
		string value = "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#hmac-sha1\" /><Reference URI=\"#MyObjectId\"><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>/Vvq6sXEVbtZC8GwNtLQnGOy/VI=</DigestValue></Reference></SignedInfo><SignatureValue>e2RxYr5yGbvTqZLCFcgA2RAC0yE=</SignatureValue><Object Id=\"MyObjectId\"><MyElement xmlns=\"samples\">This is some text</MyElement></Object></Signature>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);

		SignedXml v1 = new SignedXml ();
		v1.LoadXml (doc.DocumentElement);

		byte[] secretkey = Encoding.Default.GetBytes ("password");
		HMACSHA1 hmac = new HMACSHA1 (secretkey);

		Assert ("HMACSHA1-CheckSignature(key)", v1.CheckSignature (hmac));
	}
}

}