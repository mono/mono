<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" />

  <xsl:template match="/toc">
	<table bgcolor="#b0c4de" width="100%" cellpadding="5"><tr><td><h3><xsl:value-of select="@title" /></h3></td></tr></table>
	<xsl:apply-templates />
  </xsl:template>

  <xsl:template match="description">
	<p><xsl:value-of select="." /></p>
  </xsl:template>

  <xsl:template match="list">
	<ul>
	  <xsl:apply-templates />
	</ul>
  </xsl:template>

  <xsl:template match="item">
	<xsl:choose>
	  <xsl:when test="list">
		<li>
		<xsl:apply-templates select="list" />
		</li>
	  </xsl:when>
	  <xsl:otherwise>
		<li><a href="{@url}"><xsl:value-of select="." /></a></li>
	  </xsl:otherwise>
	</xsl:choose>
  </xsl:template>
</xsl:stylesheet>