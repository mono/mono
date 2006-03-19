<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output method="text" indent="no"/>
	<xsl:strip-space elements="*"/>

	<xsl:template match="/">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="//test-case[@success='False']">
		<xsl:text>		
		</xsl:text>
		<xsl:value-of select="@name"/>
	</xsl:template>

	<xsl:template match="//test-case/reason/message">
	</xsl:template>

	<xsl:template match="test-results">
		<xsl:if test="@failures!='0'">
			<xsl:text>Failures number :</xsl:text>
			<xsl:value-of select="@failures"/>
			<xsl:text>
		Failed tests:</xsl:text>
			<xsl:apply-templates select="//test-case"/>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>