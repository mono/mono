using NUnit.Framework;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace MonoTests.System.Xml.Xsl
{
	[TestFixture]
	public class XslCompiledTransformTests
	{
		[Test]
		public void GlobalVariableReferencesAnotherGlobalVariable ()
		{
			string xsl = @"<xsl:stylesheet version='1.0'
xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
<xsl:variable name='global2'><xsl:value-of select='root/@attr' /></xsl:variable>
<xsl:variable name='global1'>
	<xsl:for-each select='//foo'>
		<xsl:if test='@attr = $global2'>
			<xsl:value-of select='name(.)' />: <xsl:value-of select='@attr' />
		</xsl:if>
	</xsl:for-each>
</xsl:variable>
<xsl:template match='/'>
	<root>
		<xsl:value-of select='$global1' />
	</root>
</xsl:template>
</xsl:stylesheet>";
			StringWriter sw = new StringWriter ();
			XslCompiledTransform t = new XslCompiledTransform ();
			t.Load (new XPathDocument (new StringReader (xsl)));
			t.Transform (new XPathDocument (new XmlTextReader (new StringReader ("<root attr='B'><foo attr='A'/><foo attr='B'/><foo attr='C'/></root>"))), null, sw);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?><root>foo: B</root>", sw.ToString ());
		}

		[Test]
		public void MSXslNodeSetAcceptsNodeSet ()
		{
			string xsl = @"<xsl:stylesheet version='1.0'
xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:msxsl='urn:schemas-microsoft-com:xslt'>
<xsl:template match='/'>
	<root>
		<!-- msxsl:node-set() accepts a node set -->
		<xsl:for-each select='msxsl:node-set(root/foo)'>
			<xsl:value-of select='name(.)' />: <xsl:value-of select='@attr' />
		</xsl:for-each>
	</root>
</xsl:template>
</xsl:stylesheet>";
			StringWriter sw = new StringWriter ();
			XslCompiledTransform t = new XslCompiledTransform ();
			t.Load (new XPathDocument (new StringReader (xsl)));
			// should transform without an exception
			t.Transform (new XPathDocument (new XmlTextReader (new StringReader ("<root><foo attr='A'/><foo attr='B'/><foo attr='C'/></root>"))), null, sw);
		}

		[Test]
		public void MSXslNodeSetAcceptsEmptyString ()
		{
			string xsl = @"<xsl:stylesheet version='1.0'
xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:msxsl='urn:schemas-microsoft-com:xslt'>
<xsl:template match='/'>
	<root>
		<!-- msxsl:node-set() accepts an empty string -->
		<xsl:variable name='empty'></xsl:variable>
		<xsl:for-each select='msxsl:node-set($empty)'>
			<xsl:value-of select='name(.)' />: <xsl:value-of select='@attr' />
		</xsl:for-each>
	</root>
</xsl:template>
</xsl:stylesheet>";
			StringWriter sw = new StringWriter ();
			XslCompiledTransform t = new XslCompiledTransform ();
			t.Load (new XPathDocument (new StringReader (xsl)));
			// should transform without an exception
			t.Transform (new XPathDocument (new XmlTextReader (new StringReader ("<root><foo attr='A'/><foo attr='B'/><foo attr='C'/></root>"))), null, sw);
		}

		[Test]
		public void ValueOfElementWithInsignificantWhitespace ()
		{
			string xsl = @"<?xml version='1.0' encoding='utf-8'?>
<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
<xsl:template match='/'>
	<root>
		<bar>
			<xsl:if test='root/@attr'>
				<xsl:value-of select='root/@attr'>
				</xsl:value-of>
			</xsl:if>
		</bar>
		<baz>
			<xsl:for-each select='root/foo'>
				<xsl:if test='position() != 1'>
					<xsl:text>,</xsl:text>
				</xsl:if>
				<xsl:value-of select='name(.)' />: <xsl:value-of select='@attr' />
			</xsl:for-each>
		</baz>
	</root>
</xsl:template>
</xsl:stylesheet>";
			StringWriter sw = new StringWriter ();
			XslCompiledTransform t = new XslCompiledTransform ();
			t.Load (new XmlTextReader(new StringReader(xsl)));
			t.Transform (new XPathDocument (new XmlTextReader (new StringReader ("<root attr='D'><foo attr='A'/><foo attr='B'/><foo attr='C'/></root>"))), null, sw);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?><root><bar>D</bar><baz>foo: A,foo: B,foo: C</baz></root>", sw.ToString ());
		}
	}
}
