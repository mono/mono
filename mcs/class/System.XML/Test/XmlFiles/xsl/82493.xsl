<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ExtObject="urn:e">
<xsl:template match="/">
   <xsl:value-of select="ExtObject:GetPageDimensions()"/>
  <xsl:for-each select="recall/book">
    <xsl:value-of select="title"/> must be returned now!
  </xsl:for-each>
</xsl:template>
</xsl:stylesheet>

