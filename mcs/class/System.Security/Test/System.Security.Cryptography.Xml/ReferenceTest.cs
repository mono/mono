//
// ReferenceTest.cs - NUnit Test Cases for Reference
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
	public class ReferenceTest : Assertion {

		protected Reference reference;

		[SetUp]
		void SetUp () 
		{
			reference = new Reference ();
		}

		[Test]
		public void Properties () 
		{
			AssertNull ("Uri (null)", reference.Uri);
			AssertNotNull ("TransformChain", reference.TransformChain);
			AssertEquals ("ToString()", "System.Security.Cryptography.Xml.Reference", reference.ToString ());
			// test uri constructor
			string uri = "uri";
			reference = new Reference (uri);
			AssertEquals ("DigestMethod", "http://www.w3.org/2000/09/xmldsig#sha1", reference.DigestMethod);
			AssertNull ("DigestValue", reference.DigestValue);
			AssertNull ("Id", reference.Id);
			AssertNull ("Type", reference.Type);
			AssertEquals ("Uri", uri, reference.Uri);
		}

		[Test]
		public void LoadNoTransform () 
		{
			string test = "<Reference URI=\"#MyObjectId\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>/Vvq6sXEVbtZC8GwNtLQnGOy/VI=</DigestValue></Reference>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			reference.LoadXml (doc.DocumentElement);
			AssertEquals ("Load-Xml", test, (reference.GetXml().OuterXml));
			AssertEquals ("Load-URI", "#MyObjectId", reference.Uri);
			byte[] hash = { 0xFD, 0x5B, 0xEA, 0xEA, 0xC5, 0xC4, 0x55, 0xBB, 0x59, 0x0B, 0xC1, 0xB0, 0x36, 0xD2, 0xD0, 0x9C, 0x63, 0xB2, 0xFD, 0x52 };
			AssertCrypto.AssertEquals("Load-Digest", hash, reference.DigestValue);
			AssertEquals ("Load-#Transform", 0, reference.TransformChain.Count);
		}

		[Test]
		public void LoadBase64Transform () 
		{
			string test = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#base64\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			reference.LoadXml (doc.DocumentElement);
			AssertEquals ("Load-Base64", test, (reference.GetXml().OuterXml));
			AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
		}

		[Test]
		public void LoadC14NTransform () 
		{
			string test = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			reference.LoadXml (doc.DocumentElement);
			AssertEquals ("Load-C14N", test, (reference.GetXml().OuterXml));
			AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
		}

		[Test]
		public void LoadC14NWithCommentsTransforms () 
		{
			string test = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			reference.LoadXml (doc.DocumentElement);
			AssertEquals ("Load-C14NWithComments", test, (reference.GetXml().OuterXml));
			AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
		}

		[Test]
		public void LoadEnvelopedSignatureTransforms () 
		{
			string test = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			reference.LoadXml (doc.DocumentElement);
			AssertEquals ("Load-Enveloped", test, (reference.GetXml().OuterXml));
			AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
		}

		[Test]
		public void LoadXPathTransforms () 
		{
			string test = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xpath-19991116\"><XPath></XPath></Transform></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			reference.LoadXml (doc.DocumentElement);
			AssertEquals ("Load-XPath", test, (reference.GetXml().OuterXml));
			AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
		}

		[Test]
		public void LoadXsltTransforms () 
		{
			string test = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms>";
			test += "<Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xslt-19991116\">";
			test += "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns=\"http://www.w3.org/TR/xhtml1/strict\" exclude-result-prefixes=\"foo\" version=\"1.0\">";
			test += "<xsl:output encoding=\"UTF-8\" indent=\"no\" method=\"xml\" />";
			test += "<xsl:template match=\"/\"><html><head><title>Notaries</title>";
			test += "</head><body><table><xsl:for-each select=\"Notaries/Notary\">";
			test += "<tr><th><xsl:value-of select=\"@name\" /></th></tr></xsl:for-each>";
			test += "</table></body></html></xsl:template></xsl:stylesheet></Transform>";
			test += "</Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			reference.LoadXml (doc.DocumentElement);
			AssertEquals ("Load-Xslt", test, (reference.GetXml().OuterXml));
			AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
		}

		[Test]
		public void LoadAllTransforms () 
		{
			string test = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#base64\" /><Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments\" /><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" /><Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xpath-19991116\"><XPath></XPath></Transform>";
			test += "<Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xslt-19991116\">";
			test += "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns=\"http://www.w3.org/TR/xhtml1/strict\" exclude-result-prefixes=\"foo\" version=\"1.0\">";
			test += "<xsl:output encoding=\"UTF-8\" indent=\"no\" method=\"xml\" />";
			test += "<xsl:template match=\"/\"><html><head><title>Notaries</title>";
			test += "</head><body><table><xsl:for-each select=\"Notaries/Notary\">";
			test += "<tr><th><xsl:value-of select=\"@name\" /></th></tr></xsl:for-each>";
			test += "</table></body></html></xsl:template></xsl:stylesheet></Transform>";
			test += "</Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			reference.LoadXml (doc.DocumentElement);
			AssertEquals ("Load-Xml", test, (reference.GetXml().OuterXml));
			AssertEquals ("Load-#Transform", 6, reference.TransformChain.Count);
		}

		[Test]
		public void AddAllTransforms () 
		{
			// adding an empty hash value
			byte[] hash = new byte [20];
			reference.DigestValue = hash;
			XmlElement xel = reference.GetXml ();
			// this is the minimal Reference (DisestValue)!
			AssertNotNull ("GetXml", xel);

			reference.AddTransform (new XmlDsigBase64Transform ());
			reference.AddTransform (new XmlDsigC14NTransform ());
			reference.AddTransform (new XmlDsigC14NWithCommentsTransform ());
			reference.AddTransform (new XmlDsigEnvelopedSignatureTransform ());
			reference.AddTransform (new XmlDsigXPathTransform ());
			reference.AddTransform (new XmlDsigXsltTransform ());

			string value = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#base64\" /><Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments\" /><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" /><Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xpath-19991116\"><XPath></XPath></Transform><Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xslt-19991116\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
			AssertEquals("Get-Xml", value, (reference.GetXml().OuterXml));
			// however this value cannot be loaded as it's missing some transform (xslt) parameters

			// can we add them again ?
			reference.AddTransform (new XmlDsigBase64Transform ());
			reference.AddTransform (new XmlDsigC14NTransform ());
			reference.AddTransform (new XmlDsigC14NWithCommentsTransform ());
			reference.AddTransform (new XmlDsigEnvelopedSignatureTransform ());
			reference.AddTransform (new XmlDsigXPathTransform ());
			reference.AddTransform (new XmlDsigXsltTransform ());

			// seems so ;-)
			AssertEquals ("# Transforms", 12, reference.TransformChain.Count);
		}

		[Test]
		public void Null () 
		{
			// null DigestMethod -> "" DigestMethod !!!
			reference.DigestMethod = null;
			AssertNull ("DigestMethod null", reference.DigestMethod);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Bad1 () 
		{
			reference.Uri = "#MyObjectId";
			// not enough info
			XmlElement bad = reference.GetXml ();
		}

		[Test]
		public void Bad2 () 
		{
			// bad hash - there's no validation!
			reference.DigestMethod = "http://www.w3.org/2000/09/xmldsig#mono";
		}
	}
}
