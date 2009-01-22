<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="orders">
    <orders>
      <xsl:apply-templates select="order"/>
    </orders>
  </xsl:template>
  <xsl:template match="order">
    <order>
      <customer>
        <id>
          <xsl:value-of select="customer/@id"/>
        </id>
        <firstname>
          <xsl:value-of select="customername/firstn"/>
        </firstname>
        <lastname>
          <xsl:value-of select="customername/lastn"/>
        </lastname>
      </customer>
    </order>
  </xsl:template>
</xsl:stylesheet>