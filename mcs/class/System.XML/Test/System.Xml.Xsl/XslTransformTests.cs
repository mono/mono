//
// System.Xml.Xsl.XslTransformTests.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Globalization;
using System.IO;
using System.Text;
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

		[Test()]
		[Category ("NotWorking")] // it depends on "mcs" existence
		public void MsxslTest() {
			string _styleSheet = @"
			<xslt:stylesheet xmlns:xslt=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"" xmlns:msxsl=""urn:schemas-microsoft-com:xslt"" xmlns:stringutils=""urn:schemas-sourceforge.net-blah"">
				<xslt:output method=""text"" />
    				<msxsl:script language=""C#"" implements-prefix=""stringutils"">
    					<![CDATA[
					        string PadRight( string str, int padding) {
					            return str.PadRight(padding);
					        }
				        ]]>
				</msxsl:script>
    				<xslt:template match=""test"">
        				<xslt:value-of select=""stringutils:PadRight(@name, 20)"" />
    				</xslt:template>
			</xslt:stylesheet>";

			StringReader stringReader = new StringReader(_styleSheet);
			
            		XslTransform transform = new XslTransform();
            		XmlTextReader reader = new XmlTextReader(stringReader);
            		transform.Load(reader, new XmlUrlResolver(), AppDomain.CurrentDomain.Evidence);

            		StringBuilder sb = new StringBuilder();
            		StringWriter writer = new StringWriter(sb, CultureInfo.InvariantCulture);
            		XsltArgumentList arguments = new XsltArgumentList();

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<test name=\"test\" />");

            		// Do transformation
            		transform.Transform(xmlDoc, new XsltArgumentList(), writer, new XmlUrlResolver());


			AssertEquals("test".PadRight(20), sb.ToString());
		}

		[Test]
		public void MSXslNodeSet ()
		{
			string xsl = @"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:msxsl='urn:schemas-microsoft-com:xslt'>
<xsl:template match='/'>
<root>
	<xsl:variable name='var'>
		<xsl:copy-of select='root/foo' />
	</xsl:variable>
	<xsl:for-each select='msxsl:node-set($var)/foo'>
		<xsl:value-of select='name(.)' />: <xsl:value-of select='@attr' />
	</xsl:for-each>
</root>
</xsl:template>
</xsl:stylesheet>";
			StringWriter sw = new StringWriter ();
			XslTransform t = new XslTransform ();
			t.Load (new XPathDocument (new StringReader (xsl)));
			t.Transform (new XPathDocument (new XmlTextReader (new StringReader ("<root><foo attr='A'/><foo attr='B'/><foo attr='C'/></root>"))), null, sw);
			AssertEquals (@"<?xml version=""1.0"" encoding=""utf-16""?><root xmlns:msxsl=""urn:schemas-microsoft-com:xslt"">foo: Afoo: Bfoo: C</root>", sw.ToString ());
		}

		[Test]
		[Category ("NotDotNet")]
		// Actually MS.NET here throws XsltException, but Mono returns
		// XPathException (since XPath evaluation engine generally
		// catches (should catch) static error. It is implementation 
		// dependent matter.
		[ExpectedException (typeof (XPathException))]
		public void MSXslNodeSetRejectsNodeSet ()
		{
			string xsl = @"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:msxsl='urn:schemas-microsoft-com:xslt'>
<xsl:template match='/'>
<root>
	<!-- msxsl:node-set() does not accept a node set -->
	<xsl:for-each select='msxsl:node-set(root/foo)'>
		<xsl:value-of select='name(.)' />: <xsl:value-of select='@attr' />
	</xsl:for-each>
</root>
</xsl:template>
</xsl:stylesheet>";
			StringWriter sw = new StringWriter ();
			XslTransform t = new XslTransform ();
			t.Load (new XPathDocument (new StringReader (xsl)));
			t.Transform (new XPathDocument (new XmlTextReader (new StringReader ("<root><foo attr='A'/><foo attr='B'/><foo attr='C'/></root>"))), null, sw);
		}

		[Test]
		public void EvaluateEmptyVariableAsBoolean ()
		{
			string xsl = @"<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0'>
<xsl:template match='/'>
<xsl:variable name='var'><empty /></xsl:variable>
  <root><xsl:if test='$var'>true</xsl:if></root>
</xsl:template>
</xsl:stylesheet>";
			XslTransform t = new XslTransform ();
			t.Load (new XPathDocument (new StringReader (xsl)));
			StringWriter sw = new StringWriter ();
			t.Transform (
				new XPathDocument (new StringReader ("<root/>")),
				null,
				sw);
			Assert (sw.ToString ().IndexOf ("true") > 0);
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void NotAllowedPatternAxis ()
		{
			string xsl = @"<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0'>
<xsl:template match='/descendant-or-self::node()/elem'>
<ERROR/>
</xsl:template>
</xsl:stylesheet>";
			new XslTransform ().Load (new XPathDocument (
				new StringReader (xsl)));
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void ImportIncorrectlyLocated ()
		{
			string xsl = @"<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0'>
<xsl:template match='/'></xsl:template>
<xsl:import href='dummy.xsl' />
</xsl:transform>";
			new XslTransform ().Load (new XPathDocument (
				new StringReader (xsl)));
		}
	}
}
