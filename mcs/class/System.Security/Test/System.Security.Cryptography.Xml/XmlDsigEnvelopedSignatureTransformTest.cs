//
// XmlDsigEnvelopedSignatureTransformTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
//

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.Xml {

	// Note: GetInnerXml is protected in XmlDsigEnvelopedSignatureTransform making it
	// difficult to test properly. This class "open it up" :-)
	public class UnprotectedXmlDsigEnvelopedSignatureTransform : XmlDsigEnvelopedSignatureTransform {

		public XmlNodeList UnprotectedGetInnerXml () 
		{
			return base.GetInnerXml ();
		}
	}

	[TestFixture]
	public class XmlDsigEnvelopedSignatureTransformTest : Assertion {

		protected UnprotectedXmlDsigEnvelopedSignatureTransform transform;

		[SetUp]
		protected void SetUp () 
		{
			transform = new UnprotectedXmlDsigEnvelopedSignatureTransform ();
		}

		[Test]
		public void Properties () 
		{
			AssertEquals ("Algorithm", "http://www.w3.org/2000/09/xmldsig#enveloped-signature", transform.Algorithm);

			Type[] input = transform.InputTypes;
			AssertEquals ("Input Length", 3, input.Length);
			// check presence of every supported input types
			bool istream = false;
			bool ixmldoc = false;
			bool ixmlnl = false;
			foreach (Type t in input) {
				if (t.ToString () == "System.Xml.XmlDocument")
					ixmldoc = true;
				if (t.ToString () == "System.Xml.XmlNodeList")
					ixmlnl = true;
			}
			Assert ("No Input Stream", !istream);
			Assert ("Input XmlDocument", ixmldoc);
			Assert ("Input XmlNodeList", ixmlnl);

			Type[] output = transform.OutputTypes;
			AssertEquals ("Output Length", 2, output.Length);
			// check presence of every supported output types
			bool oxmlnl = false;
			bool oxmldoc = false;
			foreach (Type t in output) {
				if (t == null)
					throw new InvalidOperationException ();
				if (t.ToString () == "System.Xml.XmlNodeList")
					oxmlnl = true;
				if (t.ToString () == "System.Xml.XmlDocument")
					oxmldoc = true;
			}
			Assert ("Output XmlNodeList", oxmlnl);
			Assert ("Output XmlDocument", oxmldoc);
		}

		protected void AssertEquals (string msg, XmlNodeList expected, XmlNodeList actual) 
		{
			for (int i=0; i < expected.Count; i++) {
				if (expected [i].OuterXml != actual [i].OuterXml)
					Fail (msg + " [" + i + "] expected " + expected[i].OuterXml + " bug got " + actual[i].OuterXml);
			}
		}

		[Test]
		public void GetInnerXml () 
		{
			// Always returns null
			AssertNull (transform.UnprotectedGetInnerXml ());
		}

		private XmlDocument GetDoc () 
		{
			string dsig = "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#dsa-sha1\" /><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>fdy6S2NLpnT4fMdokUHSHsmpcvo=</DigestValue></Reference></Signature>";
			string test = "<Envelope> " + dsig + " </Envelope>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			return doc;
		}

		[Test]
		public void LoadInputAsXmlDocument () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc);
			object o = transform.GetOutput ();
			AssertEquals ("EnvelopedSignature result", doc, o);
		}

		[Test]
		public void LoadInputAsXmlNodeList () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc.ChildNodes);
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
			AssertEquals ("EnvelopedSignature result", doc.ChildNodes, xnl);
		}
	}
}
