<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="text" encoding="ascii"/>
	<xsl:param name="test"/>	
	<xsl:param name="doc"/>
	<xsl:template match="/">
		<xsl:value-of select="$test"/>
		<xsl:text>&#x000A;</xsl:text>
		<xsl:value-of select="$doc/element[@id=$test]"/>
		<xsl:text>&#x000A;</xsl:text>
	</xsl:template>
</xsl:stylesheet>

  
