<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="html" indent="no" />

<xsl:template match="/">
<xsl:for-each select="Tree/AddIns/AddIn">
	== <xsl:value-of select="@name"/> ==
	<xsl:for-each select="/Tree/Node//*[@add-in=current()/@name]">
		<xsl:value-of select="@name"/>,
	</xsl:for-each>
</xsl:for-each>
</xsl:template>

</xsl:stylesheet>

