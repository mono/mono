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
	}
}
