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

			StringWriter sw = new StringWriter ();
			transform.Transform (xmlDocument, null, sw);

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"xx", sw.ToString ());
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
#if NET_1_1
		[Category ("NotDotNet")]
#endif
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

		[Test]
		public void Output_Standalone ()
		{
			StringWriter sw = null;
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output {0} />
					<xsl:template match=""/"">
						<root />
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format(xsltFragment, "standalone=\"yes\""))), 
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"" +
				" standalone=\"yes\"?><root />", sw.ToString (), "#1");

			sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "standalone=\"no\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"" +
				" standalone=\"no\"?><root />", sw.ToString (), "#2");

			sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, ""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<root />", sw.ToString (), "#3");
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void Output_Standalone_Invalid ()
		{
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output standalone=""Yes"" />
				</xsl:stylesheet>";
			XslTransform xsltProcessor = new XslTransform ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void Output_Standalone_Empty ()
		{
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output standalone="""" />
				</xsl:stylesheet>";
			XslTransform xsltProcessor = new XslTransform ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
		}

		[Test]
		public void Output_OmitXmlDeclaration ()
		{
			StringWriter sw = null;
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output {0} />
					<xsl:template match=""/"">
						<root />
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "omit-xml-declaration=\"yes\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual ("<root />", sw.ToString (), "#1");

			sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "omit-xml-declaration=\"no\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<root />", sw.ToString (), "#2");
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void Output_OmitXmlDeclaration_Invalid ()
		{
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output omit-xml-declaration=""Yes"" />
				</xsl:stylesheet>";
			XslTransform xsltProcessor = new XslTransform ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void Output_OmitXmlDeclaration_Empty ()
		{
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output omit-xml-declaration="""" />
				</xsl:stylesheet>";
			XslTransform xsltProcessor = new XslTransform ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
		}

		[Test]
		public void Output_DocType_Xml ()
		{
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			// set both doctype-system and doctype-public
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output 
						doctype-public=""-//W3C//DTD XHTML 1.0 Strict//EN"" 
						doctype-system=""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			StringWriter sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<!DOCTYPE test PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">" +
				"<test><abc /></test>", sw.ToString (), "#1");

			// only set doctype-public
			xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output
						doctype-public=""-//W3C//DTD XHTML 1.0 Strict//EN"" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<test><abc /></test>", sw.ToString (), "#2");

			// only set doctype-system
			xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output
						indent=""no""
						doctype-system=""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<!DOCTYPE test SYSTEM \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">" +
				"<test><abc /></test>", sw.ToString (), "#3");

			// set empty doctype-public and empty doctype-system
			xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output
						doctype-public=""""
						doctype-system="""" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<!DOCTYPE test PUBLIC \"\" \"\">" +
				"<test><abc /></test>", sw.ToString (), "#4");

			// set empty doctype-public
			xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output
						indent=""no""
						doctype-public="""" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<test><abc /></test>", sw.ToString (), "#5");

			// set empty doctype-system
			xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output
						doctype-system="""" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<!DOCTYPE test SYSTEM \"\">" +
				"<test><abc /></test>", sw.ToString (), "#6");
		}

		[Test]
		public void Output_DocType_Html ()
		{
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			// set both doctype-system and doctype-public
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output 
						method=""html""
						indent=""no""
						doctype-public=""-//W3C//DTD XHTML 1.0 Strict//EN"" 
						doctype-system=""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			StringWriter sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">" +
				"<test><abc></abc></test>", sw.ToString (), "#1");

			// only set doctype-public
			xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output 
						method=""html""
						indent=""no""
						doctype-public=""-//W3C//DTD XHTML 1.0 Strict//EN"" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" >" +
				"<test><abc></abc></test>", sw.ToString (), "#2");

			// only set doctype-system
			xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output 
						method=""html""
						indent=""no""
						doctype-system=""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<!DOCTYPE html SYSTEM \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">" +
				"<test><abc></abc></test>", sw.ToString (), "#3");

			// set empty doctype-public and empty doctype-system
			xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output
						method=""html""
						indent=""no""
						doctype-public="""" doctype-system="""" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<!DOCTYPE html PUBLIC \"\" \"\">" +
				"<test><abc></abc></test>", sw.ToString (), "#4");

			// set empty doctype-public
			xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output
						method=""html""
						indent=""no""
						doctype-public="""" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<!DOCTYPE html PUBLIC \"\" >" +
				"<test><abc></abc></test>", sw.ToString (), "#5");

			// set empty doctype-system
			xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output
						method=""html""
						indent=""no""
						doctype-system="""" />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""abc"" />
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<!DOCTYPE html SYSTEM \"\">" +
				"<test><abc></abc></test>", sw.ToString (), "#6");
		}

		[Test]
		[Category ("NotWorking")] // bug #77082: mono does not output newline after xml declaration
		public void Output_Indent_Xml ()
		{
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output
						doctype-public=""-//W3C//DTD XHTML 1.0 Strict//EN""
						doctype-system=""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""
						{0} />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""something"">
								<xsl:element name=""else"" />
							</xsl:element>
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			// set indent to yes
			StringWriter sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "indent=\"yes\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<!DOCTYPE test PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">{0}" +
				"<test>{0}" +
				"  <something>{0}" +
				"    <else />{0}" +
				"  </something>{0}" +
				"</test>", Environment.NewLine), sw.ToString (), "#1");

			// set indent to no
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "indent=\"no\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<!DOCTYPE test PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">" +
				"<test><something><else /></something></test>", sw.ToString (),
				"#2");

			// indent not set
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, ""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<!DOCTYPE test PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">" +
				"<test><something><else /></something></test>", sw.ToString (),
				"#3");
		}

		[Test]
		[Category ("NotWorking")] // bug #77081: mono does not output newline and indentation for non-html elements
		public void Output_Indent_Html ()
		{
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output
						method=""html""
						doctype-public=""-//W3C//DTD XHTML 1.0 Strict//EN""
						doctype-system=""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""
						{0} />
					<xsl:template match=""/"">
						<xsl:element name=""test"">
							<xsl:element name=""something"">
								<xsl:element name=""else"" />
							</xsl:element>
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			// set indent to yes
			StringWriter sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "indent=\"yes\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">{0}" +
				"<test>{0}" +
				"  <something>{0}" +
				"    <else>{0}" +
				"    </else>{0}" +
				"  </something>{0}" +
				"</test>", Environment.NewLine), sw.ToString (), "#1");

			// set indent to no
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "indent=\"no\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">" +
				"<test><something><else></else></something></test>",
				Environment.NewLine), sw.ToString (), "#2");

			// indent not set
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, ""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">{0}" +
				"<test>{0}" +
				"  <something>{0}" +
				"    <else>{0}" +
				"    </else>{0}" +
				"  </something>{0}" +
				"</test>", Environment.NewLine), sw.ToString (), "#3");
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void Output_Indent_Invalid ()
		{
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output indent=""Yes"" />
				</xsl:stylesheet>";
			XslTransform xsltProcessor = new XslTransform ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void Output_Indent_Empty ()
		{
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output indent="""" />
				</xsl:stylesheet>";
			XslTransform xsltProcessor = new XslTransform ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
		}

		[Test]
		public void Output_MediaType ()
		{
			StringWriter sw = null;
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output media-type=""whatever"" />
					<xsl:template match=""/"">
						<root />
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<root />", sw.ToString ());
		}

		[Test]
		public void Output_Encoding_TextWriter ()
		{
			StringWriter sw = null;
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output {0} />
					<xsl:template match=""/"">
						<root />
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			// no encoding
			sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, string.Empty))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<root />", sw.ToString (), "#1");

			// valid encoding
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format(xsltFragment, "encoding=\"iso-8859-1\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<root />", sw.ToString (), "#1");

			// invalid encoding
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "encoding=\"doesnotexist\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<root />", sw.ToString (), "#2");

			// empty encoding
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "encoding=\"\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<root />", sw.ToString (), "#3");
		}

		[Test]
		public void Output_Encoding_Stream ()
		{
			MemoryStream ms = null;
			string result = null;
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output {0} />
					<xsl:template match=""/"">
						<root />
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			// no encoding
			ms = new MemoryStream ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, string.Empty))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, ms, new XmlUrlResolver ());
			ms.Position = 0;
			using (StreamReader sr = new StreamReader (ms, true)) {
				result = sr.ReadToEnd ();
			}

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root />", result, "#1");

			// valid encoding
			ms = new MemoryStream ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "encoding=\"iso-8859-1\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, ms, new XmlUrlResolver ());
			ms.Position = 0;
			using (StreamReader sr = new StreamReader (ms, true)) {
				result = sr.ReadToEnd ();
			}

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" +
				"<root />", result, "#2");

			// invalid encoding
			ms = new MemoryStream ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "encoding=\"doesnotexist\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, ms, new XmlUrlResolver ());
			ms.Position = 0;
			using (StreamReader sr = new StreamReader (ms, true)) {
				result = sr.ReadToEnd ();
			}

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root />", result, "#3");

			// empty encoding
			ms = new MemoryStream ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "encoding=\"\""))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, ms, new XmlUrlResolver ());
			ms.Position = 0;
			using (StreamReader sr = new StreamReader (ms, true)) {
				result = sr.ReadToEnd ();
			}

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
				"<root />", result, "#4");
		}

		[Test]
		public void Output_Version ()
		{
			StringWriter sw = null;
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output version=""{0}"" />
					<xsl:template match=""/"">
						<root />
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			// version 1.0
			sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "1.0"))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<root />", sw.ToString (), "#1");

			// version 2.0
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "2.0"))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<root />", sw.ToString (), "#2");

			// version BLABLA
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, "BLABLA"))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<root />", sw.ToString (), "#3");
		}

		[Test]
		public void Output_Method_Html_TextWriter ()
		{
			string options = null;
			StringWriter sw = null;
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output method=""html"" {0} />
					<xsl:template match=""/"">
						<xsl:element name=""html"">
							<xsl:element name=""head"">
								<xsl:element name=""title"">Output Test</xsl:element>
							</xsl:element>
							<xsl:element name=""Body"">
								<xsl:element name=""BR"" />
							</xsl:element>
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			// indent not set, media-type not set
			sw = new StringWriter ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, string.Empty))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"<html>{0}" +
				"{1}<head>{0}" +
				"{1}{1}<META http-equiv=\"Content-Type\" content=\"text/html; charset=utf-16\">{0}" +
				"{1}{1}<title>Output Test</title>{0}" +
				"{1}</head>{0}" +
				"{1}<Body>{0}" +
				"{1}{1}<BR>{0}" +
				"{1}</Body>{0}" +
				"</html>", Environment.NewLine, "  "), sw.ToString (), "#1");

			// indent no, media-type not set
			options = "indent=\"no\"";
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, options))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<html>{0}" +
				"{1}<head>{0}" +
				"{1}{1}<META http-equiv=\"Content-Type\" content=\"text/html; charset=utf-16\">{0}" +
				"{1}{1}<title>Output Test</title>{0}" +
				"{1}</head>{0}" +
				"{1}<Body>{0}" +
				"{1}{1}<BR>{0}" +
				"{1}</Body>{0}" +
				"</html>", string.Empty, string.Empty), sw.ToString (), "#2");

			// indent yes, media-type "bla", omit-xml-declaration "no"
			options = "indent=\"yes\" media-type=\"bla\"" +
						" encoding=\"iso-8859-1\" omit-xml-declaration=\"no\"";
			sw.GetStringBuilder ().Length = 0;
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, options))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, sw, new XmlUrlResolver ());

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<html>{0}" +
				"{1}<head>{0}" +
				"{1}{1}<META http-equiv=\"Content-Type\" content=\"bla; charset=utf-16\">{0}" +
				"{1}{1}<title>Output Test</title>{0}" +
				"{1}</head>{0}" +
				"{1}<Body>{0}" +
				"{1}{1}<BR>{0}" +
				"{1}</Body>{0}" +
				"</html>", Environment.NewLine, "  "), sw.ToString (), "#3");
		}

		[Test]
		public void Output_Method_Html_Stream ()
		{
			string options = null;
			MemoryStream ms = null;
			string result = null;
			XsltArgumentList xsltArgs = new XsltArgumentList ();
			XslTransform xsltProcessor = new XslTransform ();

			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output method=""html"" {0} />
					<xsl:template match=""/"">
						<xsl:element name=""html"">
							<xsl:element name=""head"">
								<xsl:element name=""title"">Output Test</xsl:element>
							</xsl:element>
							<xsl:element name=""Body"">
								<xsl:element name=""BR"" />
							</xsl:element>
						</xsl:element>
					</xsl:template>
				</xsl:stylesheet>";

			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml ("<dummy />");

			// indent not set, media-type not set
			ms = new MemoryStream ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, string.Empty))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, ms, new XmlUrlResolver ());
			ms.Position = 0;
			using (StreamReader sr = new StreamReader (ms, true)) {
				result = sr.ReadToEnd ();
			}

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<html>{0}" +
				"{1}<head>{0}" +
				"{1}{1}<META http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">{0}" +
				"{1}{1}<title>Output Test</title>{0}" +
				"{1}</head>{0}" +
				"{1}<Body>{0}" +
				"{1}{1}<BR>{0}" +
				"{1}</Body>{0}" +
				"</html>", Environment.NewLine, "  "), result, "#1");

			// indent no, media-type not set
			options = "indent=\"no\"";
			ms = new MemoryStream ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, options))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, ms, new XmlUrlResolver ());
			ms.Position = 0;
			using (StreamReader sr = new StreamReader (ms, true)) {
				result = sr.ReadToEnd ();
			}

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<html>{0}" +
				"{1}<head>{0}" +
				"{1}{1}<META http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">{0}" +
				"{1}{1}<title>Output Test</title>{0}" +
				"{1}</head>{0}" +
				"{1}<Body>{0}" +
				"{1}{1}<BR>{0}" +
				"{1}</Body>{0}" +
				"</html>", string.Empty, string.Empty), result, "#2");

			// indent yes, media-type "bla", omit-xml-declaration "no"
			options = "indent=\"yes\" media-type=\"bla\"" +
						" encoding=\"iso-8859-1\" omit-xml-declaration=\"no\"";
			ms = new MemoryStream ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (
				string.Format (xsltFragment, options))),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
			xsltProcessor.Transform (xmlDoc, xsltArgs, ms, new XmlUrlResolver ());
			ms.Position = 0;
			using (StreamReader sr = new StreamReader (ms, true)) {
				result = sr.ReadToEnd ();
			}

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<html>{0}" +
				"{1}<head>{0}" +
				"{1}{1}<META http-equiv=\"Content-Type\" content=\"bla; charset=iso-8859-1\">{0}" +
				"{1}{1}<title>Output Test</title>{0}" +
				"{1}</head>{0}" +
				"{1}<Body>{0}" +
				"{1}{1}<BR>{0}" +
				"{1}</Body>{0}" +
				"</html>", Environment.NewLine, "  "), result, "#3");
		}

		[Test]
		[ExpectedException (typeof (XsltCompileException))]
		public void Output_Unknown_Attribute ()
		{
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
					<xsl:output whatever="""" />
				</xsl:stylesheet>";
			XslTransform xsltProcessor = new XslTransform ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
		}

		[Test]
		public void Output_Unknown_Attribute_NonDefaultNamespace ()
		{
			string xsltFragment = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" xmlns:tst=""something"" version=""1.0"">
					<xsl:output tst:whatever="""" />
				</xsl:stylesheet>";
			XslTransform xsltProcessor = new XslTransform ();
			xsltProcessor.Load (new XmlTextReader (new StringReader (xsltFragment)),
				new XmlUrlResolver (), AppDomain.CurrentDomain.Evidence);
		}
	}
}
