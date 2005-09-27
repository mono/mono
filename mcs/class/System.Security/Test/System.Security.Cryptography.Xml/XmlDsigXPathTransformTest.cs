//
// XmlDsigXPathTransformTest.cs - NUnit Test Cases for XmlDsigXPathTransform
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;

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
			AssertEquals (expected.Count, actual.Count);
		}

		[Test]
#if NET_2_0
		[Ignore ("throws a NullReferenceException - but it's (kind of internal)")]
#endif
		public void GetInnerXml () 
		{
			XmlNodeList xnl = transform.UnprotectedGetInnerXml ();
			AssertEquals ("Default InnerXml.Count", 1, xnl.Count);
			AssertEquals ("Default InnerXml.OuterXml", "<XPath xmlns=\"http://www.w3.org/2000/09/xmldsig#\"></XPath>", xnl [0].OuterXml);
		}

		[Test]
#if ONLY_1_1
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void OnlyInner () 
		{
			XmlNodeList inner = InnerXml (""); // empty
			transform.LoadInnerXml (inner);
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
#if NET_2_0
			AssertEquals ("Count", 0, xnl.Count);
#endif
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
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void LoadInputAsXmlDocument () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc);
			XmlNodeList inner = InnerXml ("//*/title");
			transform.LoadInnerXml (inner);
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
#if NET_2_0
			AssertEquals (73, xnl.Count);
#else
			AssertEquals (47, xnl.Count);
#endif
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
		[Category ("NotWorking")]
		public void LoadInputAsXmlNodeList () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc.ChildNodes);
			XmlNodeList inner = InnerXml ("//*/title");
			transform.LoadInnerXml (inner);
			XmlNodeList xnl = (XmlNodeList) transform.GetOutput ();
			AssertEquals (1, xnl.Count);
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
#if NET_2_0
		[Category ("NotWorking")]
#endif
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
#if NET_2_0
			AssertEquals (73, xnl.Count);
#else
			AssertEquals (47, xnl.Count);
#endif
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
		[Category ("NotWorking")]
		// MS.NET looks incorrect, or something incorrect in this test code; It turned out nothing to do with function here()
		public void FunctionHereObsolete ()
		{
			XmlDsigXPathTransform t = new XmlDsigXPathTransform ();
			XmlDocument xpdoc = new XmlDocument ();
			string ns = "http://www.w3.org/2000/09/xmldsig#";
//			string xpath = "<XPath xmlns='" + ns + "' xmlns:x='urn:foo'>here()</XPath>";
			string xpath = "<XPath xmlns='" + ns + "' xmlns:x='urn:foo'></XPath>";
			xpdoc.LoadXml (xpath);
			t.LoadInnerXml (xpdoc.ChildNodes);
			XmlDocument doc = new XmlDocument ();

			doc.LoadXml ("<element a='b'><foo><bar>test</bar></foo></element>");
			t.LoadInput (doc);

			XmlNodeList nl = (XmlNodeList) t.GetOutput ();
			AssertEquals ("0", 0, nl.Count);

			doc.LoadXml ("<element xmlns='urn:foo'><foo><bar>test</bar></foo></element>");
			t.LoadInput (doc);
			nl = (XmlNodeList) t.GetOutput ();
#if NET_2_0
			AssertEquals ("1", 0, nl.Count);
#else
			AssertEquals ("1", 1, nl.Count);
#endif

			doc.LoadXml ("<element xmlns='urn:foo'><foo xmlns='urn:bar'><bar>test</bar></foo></element>");
			t.LoadInput (doc);
			nl = (XmlNodeList) t.GetOutput ();
#if NET_2_0
			AssertEquals ("2", 0, nl.Count);
#else
			AssertEquals ("2", 2, nl.Count);
#endif

			doc.LoadXml ("<element xmlns='urn:foo' xmlns:x='urn:x'><foo xmlns='urn:bar'><bar>test</bar></foo></element>");
			t.LoadInput (doc);
			nl = (XmlNodeList) t.GetOutput ();
#if NET_2_0
			AssertEquals ("3", 0, nl.Count);
#else
			AssertEquals ("3", 3, nl.Count);
#endif

			doc.LoadXml ("<envelope><Signature xmlns='http://www.w3.org/2000/09/xmldsig#'><XPath>blah</XPath></Signature></envelope>");
			t.LoadInput (doc);
			nl = (XmlNodeList) t.GetOutput ();
#if NET_2_0
			AssertEquals ("4", 0, nl.Count);
#else
			AssertEquals ("4", 1, nl.Count);
			AssertEquals ("NodeType", XmlNodeType.Attribute, nl [0].NodeType);
#endif
		}
	}
}
