//
// System.Xml.Xsl.XslTransformTests.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Xml;
using System.Xml.XPath;
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

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void InvalidStylesheet2 ()
		{
			string xml = @"<root>text</root>";
			string xsl = @"<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0'>
	<xsl:template match='/root'>
		<xsl:call-template name='foo'>
			<xsl:with-param name='name' value='text()' />
		</xsl:call-template>
	</xsl:template>
	<xsl:template name='foo'>
		<xsl:param name='name' />
		<result>
			<xsl:if test='1'>
				<xsl:variable name='last' value='text()' />
				<xsl:value-of select='$last' />
			</xsl:if>
		</result>
	</xsl:template>
</xsl:stylesheet>
";
			XslTransform xslt = new XslTransform ();
			xslt.Load (new XPathDocument (new XmlTextReader (xsl, XmlNodeType.Document, null)));
		}

	}
}
