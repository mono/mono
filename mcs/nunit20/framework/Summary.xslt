<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method='text'/>

<xsl:template match="/">
	<xsl:apply-templates/>
</xsl:template>

<xsl:template match="test-results">
<xsl:text>Tests run: </xsl:text>
<xsl:value-of select="@total"/>
<xsl:text>, Failures: </xsl:text>
<xsl:value-of select="@failures"/>
<xsl:text>, Not run: </xsl:text>
<xsl:value-of select="@not-run"/>
<xsl:text>, Time: </xsl:text>
<xsl:value-of select="test-suite/@time"/>
<xsl:text> seconds
</xsl:text>
<xsl:text>
</xsl:text>

<xsl:if test="//test-case[failure]"><xsl:text>Failures:
</xsl:text></xsl:if>
<xsl:apply-templates select="//test-case[failure]"/>
<xsl:if test="//test-case[@executed='False']"><xsl:text>Tests not run:
</xsl:text></xsl:if>
<xsl:apply-templates select="//test-case[@executed='False']"/>
<xsl:text disable-output-escaping='yes'>&#xD;&#xA;</xsl:text>
</xsl:template>

<xsl:template match="test-case">
	<xsl:value-of select="position()"/><xsl:text>) </xsl:text>
	<xsl:value-of select="@name"/>
	<xsl:text> : </xsl:text>
	<xsl:value-of select="child::node()/message"/>
<xsl:text disable-output-escaping='yes'>&#xD;&#xA;</xsl:text>
	<xsl:if test="failure">
		<xsl:value-of select="failure/stack-trace"/>
<xsl:text>
</xsl:text>
	</xsl:if>
</xsl:template>

</xsl:stylesheet>

  