<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
>
        <xsl:param name="doc" select="document('')"/>
        <xsl:template match="/">
                <xsl:choose>
                        <xsl:when test="$doc or
count($doc)>0"><xsl:text>Document found</xsl:text></xsl:when>
                        <xsl:otherwise><xsl:text>Document not
found</xsl:text></xsl:otherwise>
                </xsl:choose>
        </xsl:template>
</xsl:stylesheet>

