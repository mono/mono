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
	public class XslTransformTests
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
			Assert.AreEqual (2, result.ChildNodes.Count, "count");
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
			<xslt:stylesheet xmlns:xslt=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"" 
xmlns:msxsl=""urn:schemas-microsoft-com:xslt"" xmlns:stringutils=""urn:schemas-sourceforge.net-blah"">
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

			Assert.AreEqual ("test".PadRight(20), sb.ToString());
		}

		[Test]
		public void MSXslNodeSet ()
		{
			string xsl = @"<xsl:stylesheet version='1.0' 
xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:msxsl='urn:schemas-microsoft-com:xslt'>
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
			Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?><root xmlns:msxsl=""urn:schemas-microsoft-com:xslt"">foo: Afoo: Bfoo: C</root>", sw.ToString ());
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
			string xsl = @"<xsl:stylesheet version='1.0' 
xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:msxsl='urn:schemas-microsoft-com:xslt'>
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
			Assert.IsTrue (sw.ToString ().IndexOf ("true") > 0);
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

		private WeakReference StylesheetLoad (XslTransform t, string xsl)
		{
			XPathDocument doc = new XPathDocument (
				new StringReader (xsl));
			WeakReference wr = new WeakReference (doc);
			t.Load (doc);
			return wr;
		}

		private WeakReference StylesheetTransform (XslTransform t, string xml)
		{
			XPathDocument doc = new XPathDocument (
				new StringReader (xml));
			WeakReference wr = new WeakReference (doc);
			t.Transform (doc, null, TextWriter.Null, null);
			return wr;
		}

		[Test]
		// bug #75663.
		public void ErrorOnDocumentResolution ()
		{
			// XslTransform recovers from errors on document resolution.
			string xslText = @"<xsl:stylesheet
				xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
				version='1.0'>
				<xsl:variable name='n'
					select='document(""notexist.xml"")' />
				<xsl:template match='/'>xx</xsl:template>
				</xsl:stylesheet>";
			string xmlText = @"<root />";
			XslTransform transform = new XslTransform ();
			XPathDocument doc = new XPathDocument (
				new XmlTextReader ("a.xsl", new StringReader (xslText)));
			transform.Load (doc);
			XPathDocument xmlDocument = new XPathDocument (new StringReader (xmlText));
			transform.Transform (xmlDocument, null, TextWriter.Null);
		}

		// bug #76046
		[Test]
		public void LoadStyleFromNonRoot ()
		{
			XmlDocument doc = new XmlDocument ();
			XslTransform xslt = new XslTransform ();
			doc.LoadXml ("<root><dummy /><xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' /></root>");
			XmlNode node = doc.ChildNodes [0].ChildNodes [1];
			xslt.Load (node, null, null);
		}

		[Test]
		public void ReturnEmptyResultsAsXmlReader ()
		{
			// bug #76115
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' />");
			XslTransform xslt = new XslTransform ();
			xslt.Load (doc, null, null);
			XmlReader reader = xslt.Transform(doc, null, new XmlUrlResolver ());
			reader.Read ();

			// another case - with xsl:output standalone='yes'
			doc.LoadXml ("<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0' ><xsl:output standalone='yes' indent='no'/><xsl:template match='/'><foo/></xsl:template></xsl:transform>");
			xslt = new XslTransform ();
			xslt.Load (doc, null, null);
			reader = xslt.Transform (doc, null, new XmlUrlResolver ());
			while (!reader.EOF)
				reader.Read (); // btw no XMLdecl output.
		}

		[Test] // bug #76530
		// http://www.w3.org/TR/xslt#section-Creating-Elements-with-xsl:element
		// "If the namespace attribute is not present then the QName
		// is expanded into an expanded-name using the namespace
		// declarations in effect for the xsl:element element,
		// including any default namespace declaration."
		public void LREDefaultNamespace ()
		{
			string xsl = @"<xsl:stylesheet version='1.0' xmlns='urn:foo' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
				<xsl:template match='/*'>
					<xsl:element name='{local-name()}' />
				</xsl:template>
			</xsl:stylesheet>";
			string xml = "<root/>";
			XslTransform t = new XslTransform ();
			t.Load (new XPathDocument (new StringReader (xsl)));
			StringWriter sw = new StringWriter ();
			XmlTextWriter xw = new XmlTextWriter (sw);
			t.Transform (
				new XPathDocument (new StringReader (xml)),
				null, xw);
			Assert.AreEqual ("<root xmlns=\"urn:foo\" />",
				sw.ToString ());

			string xsl2 = @"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns='urn:foo'>
				<xsl:template match='/*'>
					<root>
						<xsl:element name='{local-name()}' />
					</root>
				</xsl:template>
			</xsl:stylesheet>";
			string xml2 = "<page/>";
			t.Load (new XPathDocument (new StringReader (xsl2)));
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			t.Transform (
				new XPathDocument (new StringReader (xml2)),
				null, xw);
			Assert.AreEqual ("<root xmlns=\"urn:foo\"><page /></root>",
				sw.ToString ());
		}

		[Test]
		// http://lists.ximian.com/pipermail/mono-devel-list/2005-November/015812.html
		public void WhitespaceHandling ()
		{
			string ref_out = @"XML 
        Extensible Markup language
         Great stuffs 
    XSLT  
        Extensible Markup language
         Great stuffs 
    XPATH 
        Extensible Markup language
         Great stuffs 
    XSD 
        Extensible Markup language
         Great stuffs 
    ";

			XmlDocument d = new XmlDocument ();
			d.Load ("Test/XmlFiles/xsl/91834.xml");

			XslTransform t = new XslTransform ();
			t.Load ("Test/XmlFiles/xsl/91834.xsl");

			StringWriter sw_raw = new StringWriter ();
			t.Transform (d, null, sw_raw);

			Assert.AreEqual (ref_out, sw_raw.ToString ().Replace ("\r\n", "\n"));
		}

		// http://support.microsoft.com/default.aspx?scid=kb;en-us;829014
		[Test]
		public void EmptyNodeSetSort ()
		{
			string xmlFragment = @"<?xml version=""1.0"" encoding=""utf-8""?>
				<EMPLOYEES>
					<EMPLOYEE>
						<NAME>Steve</NAME>
						<DEPT>IT</DEPT>
						<SKILL>C++</SKILL>
						<SKILL>C#</SKILL>
					</EMPLOYEE>
					<EMPLOYEE>
						<NAME>John</NAME>
						<DEPT>IT</DEPT>
						<SKILL>VB.NET</SKILL>
						<SKILL>SQl Server</SKILL>
					</EMPLOYEE>
				</EMPLOYEES>";

			string xsltFragment = @"<?xml version=""1.0""?>
				<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
					<xsl:output omit-xml-declaration=""yes"" />
					<xsl:preserve-space elements=""*"" />
					<xsl:template match=""/EMPLOYEES"">
						<xsl:for-each select=""EMPLOYEE[DEPT='Finance']"">
							<xsl:sort select=""NAME""/>
							<xsl:value-of select=""NAME""/>
						</xsl:for-each>
					</xsl:template>
				</xsl:stylesheet>";

			XmlTextReader xmlRdr = new XmlTextReader (new StringReader (xmlFragment));
			XmlTextReader xsltRdr = new XmlTextReader (new StringReader (xsltFragment));

			XslTransform stylesheet = new XslTransform ();
			stylesheet.Load (xsltRdr, new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);

			StringWriter sw = new StringWriter ();

			stylesheet.Transform (new XPathDocument (xmlRdr), new XsltArgumentList (),
				sw, new XmlUrlResolver ());

			Assert.AreEqual (0, sw.ToString ().Length);
		}

		// http://support.microsoft.com/default.aspx?scid=kb;en-us;834667
		[Test]
		public void LocalParameter ()
		{
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:param name=""param1"" select=""'global-param1-default'"" />
					<xsl:param name=""param2"" select=""'global-param2-default'"" />
					<xsl:output method=""text"" encoding=""ascii"" />
					<xsl:template match=""/"">
						<xsl:call-template name=""Test"">
							<xsl:with-param name=""param1"" select=""'local-param1-arg'"" />
							<xsl:with-param name=""param2"" select=""'local-param2-arg'"" />
						</xsl:call-template>
					</xsl:template>
					<xsl:template name=""Test"">
						<xsl:param name=""param1"" select=""'local-param1-default'"" />
						<xsl:param name=""param2"" select=""'local-param2-default'"" />
						<xsl:value-of select=""$param1"" /><xsl:text>/</xsl:text><xsl:value-of select=""$param2"" />
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			XslTransform xsltProcessor = new XslTransform ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);

			StringWriter sw = new StringWriter ();

			XsltArgumentList xsltArgs = new XsltArgumentList ();
			xsltArgs.AddParam ("param1", string.Empty, "global-param1-arg");
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual ("local-param1-arg/local-param2-arg", sw.ToString ());
		}
	}
}
