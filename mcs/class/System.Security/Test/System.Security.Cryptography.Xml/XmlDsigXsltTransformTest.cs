//
// XmlDsigXsltTransformTest.cs - NUnit Test Cases for XmlDsigXsltTransform
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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

	[TestFixture]
	public class XmlDsigXsltTransformTest : Assertion {

		protected XmlDsigXsltTransform transform;

		[SetUp]
		protected void SetUp () 
		{
			transform = new XmlDsigXsltTransform ();
		}

		[Test]
		public void Properties () 
		{
			AssertEquals ("Algorithm", "http://www.w3.org/TR/1999/REC-xslt-19991116", transform.Algorithm);

			Type[] input = transform.InputTypes;
			Assert ("Input #", (input.Length == 3));
			// check presence of every supported input types
			bool istream = false;
			bool ixmldoc = false;
			bool ixmlnl = false;
			foreach (Type t in input) {
				if (t.ToString () == "System.IO.Stream")
					istream = true;
				if (t.ToString () == "System.Xml.XmlDocument")
					ixmldoc = true;
				if (t.ToString () == "System.Xml.XmlNodeList")
					ixmlnl = true;
			}
			Assert ("Input Stream", istream);
			Assert ("Input XmlDocument", ixmldoc);
			Assert ("Input XmlNodeList", ixmlnl);

			Type[] output = transform.OutputTypes;
			Assert ("Output #", (output.Length == 1));
			// check presence of every supported output types
			bool ostream = false;
			foreach (Type t in input) {
				if (t.ToString () == "System.IO.Stream")
					ostream = true;
			}
			Assert ("Output Stream", ostream);
		}

		private string Stream2Array (Stream s) 
		{
			StringBuilder sb = new StringBuilder ();
			int b = s.ReadByte ();
			while (b != -1) {
				sb.Append (b.ToString("X2"));
				b = s.ReadByte ();
			}
			return sb.ToString ();
		}

		[Test]
		// can't use ExpectedException as it doesn't have a constructor with 0 parameters
		// [ExpectedException (typeof (XsltCompileException))]
		public void InvalidXslt () 
		{
			string test = "<Test>XmlDsigXsltTransform</Test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);

			transform.LoadInnerXml (doc.ChildNodes);
			try {
				Stream s = (Stream) transform.GetOutput ();
				Fail ("Expected XsltCompileException but got none");
			}
			catch (XsltCompileException) {
				// expected
			}
			catch (Exception e) {
				Fail ("Expected XsltCompileException but got :" + e.ToString ());
			}
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void OnlyInner () 
		{
			string test = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns=\"http://www.w3.org/TR/xhtml1/strict\" version=\"1.0\">";
			test += "<xsl:output encoding=\"UTF-8\" indent=\"no\" method=\"xml\" />";
			test += "<xsl:template match=\"/\"><html><head><title>Notaries</title>";
			test += "</head><body><table><xsl:for-each select=\"Notaries/Notary\">";
			test += "<tr><th><xsl:value-of select=\"@name\" /></th></tr></xsl:for-each>";
			test += "</table></body></html></xsl:template></xsl:stylesheet>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);

			transform.LoadInnerXml (doc.ChildNodes);
			Stream s = (Stream) transform.GetOutput ();
			string output = Stream2Array (s);
		}

		private XmlDocument GetDoc () 
		{
			string test = "<Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xslt-19991116\">";
			test += "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns=\"http://www.w3.org/TR/xhtml1/strict\" version=\"1.0\">";
			test += "<xsl:output encoding=\"UTF-8\" indent=\"no\" method=\"xml\" />";
			test += "<xsl:template match=\"/\"><html><head><title>Notaries</title>";
			test += "</head><body><table><xsl:for-each select=\"Notaries/Notary\">";
			test += "<tr><th><xsl:value-of select=\"@name\" /></th></tr></xsl:for-each>";
			test += "</table></body></html></xsl:template></xsl:stylesheet></Transform>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			return doc;
		}

		[Test]
		[Ignore ("not working")]
		public void LoadInputAsXmlDocument () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc);
			Stream s = (Stream) transform.GetOutput ();
			string output = Stream2Array (s);
		}

		[Test]
		[Ignore ("not working")]
		public void LoadInputAsXmlNodeList () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc.ChildNodes);
			Stream s = (Stream) transform.GetOutput ();
			string output = Stream2Array (s);
		}

		[Test]
		[Ignore ("not working")]
		public void LoadInputAsStream () 
		{
			XmlDocument doc = GetDoc ();
			MemoryStream ms = new MemoryStream ();
			doc.Save (ms);
			ms.Position = 0;
			transform.LoadInput (ms);
			Stream s = (Stream) transform.GetOutput ();
			string output = Stream2Array (s);
		}

		protected void AssertEquals (string msg, XmlNodeList expected, XmlNodeList actual) 
		{
			for (int i=0; i < expected.Count; i++) {
				if (expected[i].OuterXml != actual[i].OuterXml)
					Fail (msg + " [" + i + "] expected " + expected[i].OuterXml + " bug got " + actual[i].OuterXml);
			}
		}

		[Test]
		public void LoadInnerXml () 
		{
			string value = "<Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xslt-19991116\" />";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (value);

			transform.LoadInnerXml (doc.ChildNodes);
			// note: GetInnerXml is protected so we can't AssertEquals :-(
			// unless we use reflection (making it a lot more complicated)
		}

		[Test]
		[Ignore ("not working")]
		public void Load2 () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInnerXml (doc.ChildNodes);
			transform.LoadInput (doc);
			Stream s = (Stream) transform.GetOutput ();
			string output = Stream2Array (s);
		}

		[Test]
		public void UnsupportedInput () 
		{
			byte[] bad = { 0xBA, 0xD };
			// LAMESPEC: input MUST be one of InputType - but no exception is thrown (not documented)
			transform.LoadInput (bad);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnsupportedOutput () 
		{
			XmlDocument doc = new XmlDocument();
			object o = transform.GetOutput (doc.GetType ());
		}
	}
}
