//
// ReferenceTest.cs - NUnit Test Cases for Reference
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

public class ReferenceTest : TestCase {

	public ReferenceTest () : base ("System.Security.Cryptography.Xml.Reference testsuite") {}
	public ReferenceTest (string name) : base (name) {}

	private Reference reference;

	protected override void SetUp () 
	{
		reference = new Reference ();
	}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (ReferenceTest)); 
		}
	}

	public void TestProperties () 
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

	public void TestLoadNoTransform () 
	{
		string value = "<Reference URI=\"#MyObjectId\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>/Vvq6sXEVbtZC8GwNtLQnGOy/VI=</DigestValue></Reference>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);
		reference.LoadXml (doc.DocumentElement);
		AssertEquals ("Load-Xml", value, (reference.GetXml().OuterXml));
		AssertEquals ("Load-URI", "#MyObjectId", reference.Uri);
		byte[] hash = { 0xFD, 0x5B, 0xEA, 0xEA, 0xC5, 0xC4, 0x55, 0xBB, 0x59, 0x0B, 0xC1, 0xB0, 0x36, 0xD2, 0xD0, 0x9C, 0x63, 0xB2, 0xFD, 0x52 };
		AllTests.AssertEquals("Load-Digest", hash, reference.DigestValue);
		AssertEquals ("Load-#Transform", 0, reference.TransformChain.Count);
	}

	public void TestLoadBase64Transform () 
	{
		string value = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#base64\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);
		reference.LoadXml (doc.DocumentElement);
		AssertEquals ("Load-Base64", value, (reference.GetXml().OuterXml));
		AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
	}

	public void TestLoadC14NTransform () 
	{
		string value = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);
		reference.LoadXml (doc.DocumentElement);
		AssertEquals ("Load-C14N", value, (reference.GetXml().OuterXml));
		AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
	}

	public void TestLoadC14NWithCommentsTransforms () 
	{
		string value = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);
		reference.LoadXml (doc.DocumentElement);
		AssertEquals ("Load-C14NWithComments", value, (reference.GetXml().OuterXml));
		AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
	}

	public void TestLoadEnvelopedSignatureTransforms () 
	{
		string value = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);
		reference.LoadXml (doc.DocumentElement);
		AssertEquals ("Load-Enveloped", value, (reference.GetXml().OuterXml));
		AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
	}

	public void TestLoadXPathTransforms () 
	{
		string value = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xpath-19991116\"><XPath></XPath></Transform></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);
		reference.LoadXml (doc.DocumentElement);
		AssertEquals ("Load-XPath", value, (reference.GetXml().OuterXml));
		AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
	}

	public void TestLoadXsltTransforms () 
	{
		string value = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xslt-19991116\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);
		try {
			reference.LoadXml (doc.DocumentElement);
		}
		catch (Exception e) {
			Fail (e.ToString ());
		}
		AssertEquals ("Load-Xslt", value, (reference.GetXml().OuterXml));
		AssertEquals ("Load-#Transform", 1, reference.TransformChain.Count);
	}

	public void TestLoadAllTransforms () 
	{
		string value = "<Reference xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#base64\" /><Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments\" /><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" /><Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xpath-19991116\"><XPath></XPath></Transform><Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xslt-19991116\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>AAAAAAAAAAAAAAAAAAAAAAAAAAA=</DigestValue></Reference>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);
		reference.LoadXml (doc.DocumentElement);
		AssertEquals ("Load-Xml", value, (reference.GetXml().OuterXml));
		AssertEquals ("Load-#Transform", 6, reference.TransformChain.Count);
	}

	public void TestAllTransform () 
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

	public void TestNull () 
	{
		// null DigestMethod -> "" DigestMethod !!!
		reference.DigestMethod = null;
		AssertNull ("DigestMethod null", reference.DigestMethod);
	}

	public void TestBadness ()
	{
		Reference reference = new Reference ();
		reference.Uri = "#MyObjectId";
		// not enough info
		try {
			XmlElement bad = reference.GetXml ();
			Fail ("Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("Expected NullReferenceException but got: " + e.ToString ());
		}
		// bad hash - there's no validation!
		reference.DigestMethod = "http://www.w3.org/2000/09/xmldsig#mono";
	}
}
}
