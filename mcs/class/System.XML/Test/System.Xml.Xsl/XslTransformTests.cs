//
// System.Xml.Serialization.XmlSerializer.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Xml;
using System.Xml.Xsl;
using NUnit.Framework;

namespace MonoTests.System.Xml.Xsl
{
	[TestFixture]
	public class XslTransformTests : Assertion
	{
		XmlDocument doc;
		XslTransform xslt;
		XmlDocument result;

		[SetUp]
		public void GetReady()
		{
			doc = new XmlDocument ();
			xslt = new XslTransform ();
			result = new XmlDocument ();
		}

		[Test]
		public void TestBasicTransform ()
		{
			doc.LoadXml ("<root/>");
			xslt.Load ("Test/XmlFiles/xsl/empty.xsl");
			xslt.Transform ("Test/XmlFiles/xsl/empty.xsl", "Test/XmlFiles/xsl/result.xml");
			result.Load ("Test/XmlFiles/xsl/result.xml");
			AssertEquals ("count", 2, result.ChildNodes.Count);
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void InvalidStylesheet ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<xsl:element xmlns:xsl='http://www.w3.org/1999/XSL/Transform' />");
			XslTransform t = new XslTransform ();
			t.Load (doc);
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void EmptyStylesheet ()
		{
			XmlDocument doc = new XmlDocument ();
			XslTransform t = new XslTransform ();
			t.Load (doc);
		}
	}
}
