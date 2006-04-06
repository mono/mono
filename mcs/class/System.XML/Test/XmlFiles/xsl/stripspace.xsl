<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:strip-space elements="*" />
<xsl:output method="html" indent="no" />

<xsl:template match="/">
<xsl:apply-templates select="Tree/*"/>
</xsl:template>

<xsl:template match="Node">
<xsl:value-of select="@name"/><xsl:text>
</xsl:text>
<xsl:apply-templates/>
</xsl:template>

</xsl:stylesheet>

