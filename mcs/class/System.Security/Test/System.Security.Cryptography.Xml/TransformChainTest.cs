//
// TransformChainTest.cs - NUnit Test Cases for TransformChain
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
	public class TransformChainTest {

		[Test]
		public void EmptyChain () 
		{
			TransformChain chain = new TransformChain ();
			Assertion.AssertEquals ("empty count", 0, chain.Count);
			Assertion.AssertNotNull ("IEnumerator", chain.GetEnumerator ());
			Assertion.AssertEquals ("ToString()", "System.Security.Cryptography.Xml.TransformChain", chain.ToString ());
		}

		[Test]
		public void FullChain () 
		{
			TransformChain chain = new TransformChain ();

			XmlDsigBase64Transform base64 = new XmlDsigBase64Transform ();
			chain.Add (base64);
			Assertion.AssertEquals ("XmlDsigBase64Transform", base64, chain[0]);
			Assertion.AssertEquals ("count 1", 1, chain.Count);

			XmlDsigC14NTransform c14n = new XmlDsigC14NTransform ();
			chain.Add (c14n);
			Assertion.AssertEquals ("XmlDsigC14NTransform", c14n, chain[1]);
			Assertion.AssertEquals ("count 2", 2, chain.Count);

			XmlDsigC14NWithCommentsTransform c14nc = new XmlDsigC14NWithCommentsTransform ();
			chain.Add (c14nc);
			Assertion.AssertEquals ("XmlDsigC14NWithCommentsTransform", c14nc, chain[2]);
			Assertion.AssertEquals ("count 3", 3, chain.Count);

			XmlDsigEnvelopedSignatureTransform esign = new XmlDsigEnvelopedSignatureTransform ();
			chain.Add (esign);
			Assertion.AssertEquals ("XmlDsigEnvelopedSignatureTransform", esign, chain[3]);
			Assertion.AssertEquals ("count 4", 4, chain.Count);

			XmlDsigXPathTransform xpath = new XmlDsigXPathTransform ();
			chain.Add (xpath);
			Assertion.AssertEquals ("XmlDsigXPathTransform", xpath, chain[4]);
			Assertion.AssertEquals ("count 5", 5, chain.Count);

			XmlDsigXsltTransform xslt = new XmlDsigXsltTransform ();
			chain.Add (xslt);
			Assertion.AssertEquals ("XmlDsigXsltTransform", xslt, chain[5]);
			Assertion.AssertEquals ("count 6", 6, chain.Count);
		}
	}
}
