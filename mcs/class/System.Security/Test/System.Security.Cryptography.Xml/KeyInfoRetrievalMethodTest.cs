//
// KeyInfoRetrievalMethodTest.cs - NUnit Test Cases for KeyInfoRetrievalMethod
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

public class KeyInfoRetrievalMethodTest : TestCase {

	public KeyInfoRetrievalMethodTest () : base ("System.Security.Cryptography.Xml.KeyInfoRetrievalMethod testsuite") {}
	public KeyInfoRetrievalMethodTest (string name) : base (name) {}

	protected override void SetUp () {}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (KeyInfoRetrievalMethodTest)); 
		}
	}

	public void TestNewKeyNode () 
	{
		string uri = "http://www.go-mono.com/";
		KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();

		// verify empty XML
		AssertEquals ("Empty", "<RetrievalElement xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (uri1.GetXml ().OuterXml));

		uri1.Uri = uri;
		XmlElement xel = uri1.GetXml ();

		KeyInfoRetrievalMethod uri2 = new KeyInfoRetrievalMethod (uri1.Uri);
		uri2.LoadXml (xel);

		AssertEquals ("uri1==uri2", (uri1.GetXml ().OuterXml), (uri2.GetXml ().OuterXml));
		AssertEquals ("uri==Uri", uri, uri1.Uri);
	}

	public void TestImportKeyNode () 
	{
		string value = "<RetrievalElement URI=\"http://www.go-mono.com/\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);

		KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();
		uri1.LoadXml (doc.DocumentElement);

		// verify that proper XML is generated (equals to original)
		string s = (uri1.GetXml ().OuterXml);
		AssertEquals ("Xml", value, s);

		// verify that property is parsed correctly
		AssertEquals ("Uri", "http://www.go-mono.com/", uri1.Uri);
	}

	public void TestInvalidKeyNode () 
	{
		KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();
		try {
			uri1.LoadXml (null);
			Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}

		string bad = "<Test></Test>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (bad);
		// no exception is thrown
		uri1.LoadXml (doc.DocumentElement);
		// note that URI="" is present (unlike a empty Uri)
		AssertEquals("invalid", "<RetrievalElement URI=\"\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (uri1.GetXml ().OuterXml));
	}
}

}