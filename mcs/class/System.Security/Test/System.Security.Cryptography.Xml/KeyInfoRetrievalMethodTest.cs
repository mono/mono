//
// KeyInfoRetrievalMethodTest.cs - NUnit Test Cases for KeyInfoRetrievalMethod
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
	public class KeyInfoRetrievalMethodTest {

		[Test]
		public void TestNewKeyNode () 
		{
			string uri = "http://www.go-mono.com/";
			KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();

			// verify empty XML
			Assertion.AssertEquals ("Empty", "<RetrievalElement xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (uri1.GetXml ().OuterXml));

			uri1.Uri = uri;
			XmlElement xel = uri1.GetXml ();

			KeyInfoRetrievalMethod uri2 = new KeyInfoRetrievalMethod (uri1.Uri);
			uri2.LoadXml (xel);

			Assertion.AssertEquals ("uri1==uri2", (uri1.GetXml ().OuterXml), (uri2.GetXml ().OuterXml));
			Assertion.AssertEquals ("uri==Uri", uri, uri1.Uri);
		}

		[Test]
		public void TestImportKeyNode () 
		{
			string value = "<RetrievalElement URI=\"http://www.go-mono.com/\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (value);

			KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();
			uri1.LoadXml (doc.DocumentElement);

			// verify that proper XML is generated (equals to original)
			string s = (uri1.GetXml ().OuterXml);
			Assertion.AssertEquals ("Xml", value, s);

			// verify that property is parsed correctly
			Assertion.AssertEquals ("Uri", "http://www.go-mono.com/", uri1.Uri);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InvalidKeyNode1 () 
		{
			KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();
			uri1.LoadXml (null);
		}

		[Test]
		public void InvalidKeyNode2 () 
		{
			string bad = "<Test></Test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (bad);

			KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();
			// no exception is thrown
			uri1.LoadXml (doc.DocumentElement);
			// note that URI="" is present (unlike a empty Uri)
			Assertion.AssertEquals("invalid", "<RetrievalElement URI=\"\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (uri1.GetXml ().OuterXml));
		}
	}
}