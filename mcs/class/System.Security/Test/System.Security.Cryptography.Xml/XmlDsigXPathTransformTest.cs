//
// XmlDsigXPathTransformTest.cs - NUnit Test Cases for XmlDsigXPathTransform
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
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

	// Note: GetInnerXml is protected in XmlDsigXPathTransform making it
	// difficult to test properly. This class "open it up" :-)
	public class UnprotectedXmlDsigXPathTransform : XmlDsigXPathTransform {

		public XmlNodeList UnprotectedGetInnerXml () 
		{
			return base.GetInnerXml ();
		}
	}

	[TestFixture]
	public class XmlDsigXPathTransformTest : Assertion {

		protected UnprotectedXmlDsigXPathTransform transform;

		[SetUp]
		protected void SetUp () 
		{
			transform = new UnprotectedXmlDsigXPathTransform ();
		}

		[Test]
		public void Properties () 
		{
			AssertEquals ("Algorithm", "http://www.w3.org/TR/1999/REC-xpath-19991116", transform.Algorithm);

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
			bool oxmlnl = false;
			foreach (Type t in output) {
				if (t.ToString () == "System.Xml.XmlNodeList")
					oxmlnl = true;
			}
			Assert ("Output XmlNodeList", oxmlnl);
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
			XmlNodeList xnl = transform.UnprotectedGetInnerXml ();
			AssertEquals ("Default InnerXml.Count", 1, xnl.Count);
			AssertEquals ("Default InnerXml.OuterXml", "<XPath xmlns=\"http://www.w3.org/2000/09/xmldsig#\"></XPath>", xnl [0].OuterXml);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void OnlyInner () 
		{
			XmlNodeList inner = InnerXml (""); // empty
			transform.LoadInnerXml (inner);
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
		}

		private XmlDocument GetDoc () 
		{
			string test = "<catalog><cd><title>Empire Burlesque</title><artist>Bob Dylan</artist><price>10.90</price>";
			test += "<year>1985</year></cd><cd><title>Hide your heart</title><artist>Bonnie Tyler</artist><price>9.90</price>";
			test += "<year>1988</year></cd><cd><title>Greatest Hits</title><artist>Dolly Parton</artist><price>9.90</price>";
			test += "<year>1982</year></cd><cd><title>Still got the blues</title><artist>Gary Moore</artist><price>10.20</price>";
			test += "<year>1990</year></cd><cd><title>Eros</title><artist>Eros Ramazzotti</artist><price>9.90</price>";
			test += "<year>1997</year></cd></catalog>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test);
			return doc;
		}

		private XmlNodeList InnerXml (string xpathExpr) 
		{
			string xpath = "<XPath xmlns=\"http://www.w3.org/2000/09/xmldsig#\">" + xpathExpr + "</XPath>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xpath);
			return doc.ChildNodes;
		}

		[Test]
		public void LoadInputAsXmlDocument () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc);
			XmlNodeList inner = InnerXml ("//*/title");
			transform.LoadInnerXml (inner);
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
			// FIXME: This cannot be the *right* result - I must be missing something :(
			AssertEquals ("XPath result", doc.ChildNodes, xnl);
		}

		[Test]
		public void LoadInputAsXmlDocument_EmptyXPath () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc);
			// empty means no LoadInnerXml
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
			AssertEquals ("Empy Result", 0, xnl.Count);
		}

		[Test]
		public void LoadInputAsXmlNodeList () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc.ChildNodes);
			XmlNodeList inner = InnerXml ("//*/title");
			transform.LoadInnerXml (inner);
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
			// FIXME: This cannot be the *right* result - I must be missing something :(
			AssertEquals ("XPath result", doc.ChildNodes, xnl);
		}

		[Test]
		public void LoadInputAsXmlNodeList_EmptyXPath () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc.ChildNodes);
			// empty means no LoadInnerXml
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
			AssertEquals ("Empy Result", 0, xnl.Count);
		}

		[Test]
		public void LoadInputAsStream () 
		{
			XmlDocument doc = GetDoc ();
			doc.PreserveWhitespace = true;
			MemoryStream ms = new MemoryStream ();
			doc.Save (ms);
			ms.Position = 0;
			transform.LoadInput (ms);
			XmlNodeList inner = InnerXml ("//*/title");
			transform.LoadInnerXml (inner);
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
			// FIXME: This cannot be the *right* result - I must be missing something :(
			AssertEquals ("XPath result", doc.ChildNodes, xnl);
		}

		[Test]
		public void LoadInputAsStream_EmptyXPath () 
		{
			XmlDocument doc = GetDoc ();
			MemoryStream ms = new MemoryStream ();
			doc.Save (ms);
			ms.Position = 0;
			transform.LoadInput (ms);
			// empty means no LoadInnerXml
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
			AssertEquals ("Empy Result", 0, xnl.Count);
		}

		[Test]
		public void LoadInnerXml () 
		{
			XmlNodeList inner = InnerXml ("//*");
			transform.LoadInnerXml (inner);
			XmlNodeList xnl = transform.UnprotectedGetInnerXml ();
			AssertEquals ("LoadInnerXml", inner, xnl);
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
			XmlDocument doc = new XmlDocument ();
			object o = transform.GetOutput (doc.GetType ());
		}

		[Test]
		public void TransformSimple ()
		{
			XmlDsigXPathTransform t = new XmlDsigXPathTransform ();
			XmlDocument xpdoc = new XmlDocument ();
			string ns = "http://www.w3.org/2000/09/xmldsig#";
			string xpath = "<XPath xmlns='" + ns + "' xmlns:x='urn:foo'>*|@*|namespace::*</XPath>"; // not absolute path.. so @* and namespace::* does not make sense.
			xpdoc.LoadXml (xpath);
			t.LoadInnerXml (xpdoc.ChildNodes);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<element xmlns='urn:foo'><foo><bar>test</bar></foo></element>");
			t.LoadInput (doc);
			XmlNodeList nl = (XmlNodeList) t.GetOutput ();
			AssertEquals (XmlNodeType.Document, nl [0].NodeType);
			AssertEquals (XmlNodeType.Element, nl [1].NodeType);
			AssertEquals ("element", nl [1].LocalName);
			AssertEquals (XmlNodeType.Element, nl [2].NodeType);
			AssertEquals ("foo", nl [2].LocalName);
			AssertEquals (XmlNodeType.Element, nl [3].NodeType);
			AssertEquals ("bar", nl [3].LocalName);
			// MS.NET bug - ms.net returns ns node even when the
			// current node is ns node (it is like returning
			// attribute from attribute nodes).
//			AssertEquals (XmlNodeType.Attribute, nl [4].NodeType);
//			AssertEquals ("xmlns", nl [4].LocalName);
		}

		[Test]
		[Ignore ("MS.NET looks incorrect, or something incorrect in this test code")]
		public void FunctionHere ()
		{
			XmlDsigXPathTransform t = new XmlDsigXPathTransform ();
			XmlDocument xpdoc = new XmlDocument ();
			string ns = "http://www.w3.org/2000/09/xmldsig#";
			string xpath = "<XPath xmlns='" + ns + "' xmlns:x='urn:foo'>here()</XPath>";
			xpdoc.LoadXml (xpath);
			t.LoadInnerXml (xpdoc.ChildNodes);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<element xmlns='urn:foo'><foo><bar>test</bar></foo></element>");
			t.LoadInput (doc);
			XmlNodeList nl = (XmlNodeList) t.GetOutput ();
			AssertEquals (0, nl.Count);
		}
	}
}
