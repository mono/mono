<?xml version='1.0' ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output indent="yes"/>
	<xsl:template match="/">
		<tests>
			<xsl:for-each select="test-suite/test-catalog/test-case[scenario/@operation='standard']">
<xsl:if test="@id != 'Keys_PerfRepro3'">
				<xsl:element name="test">
					<xsl:attribute name="id">
						<xsl:value-of select="@id"/>
					</xsl:attribute>
					<path>
						<!-- quick fix -->
						<xsl:choose>
						<xsl:when test="file-path = 'Value-of'">
							<xsl:text>Valueof</xsl:text>
						</xsl:when>
						<xsl:otherwise>
						<xsl:value-of select="file-path"/>
						</xsl:otherwise>
						</xsl:choose>
					</path>
					<data>
						<xsl:value-of select="scenario/input-file[@role='principal-data']"/>
					</data>
					<stylesheet>
						<xsl:value-of select="scenario/input-file[@role='principal-stylesheet']"/>					</stylesheet>
					<output>
						<xsl:value-of select="scenario/output-file"/>
					</output>
				</xsl:element>
</xsl:if>
			</xsl:for-each>
		</tests>
	</xsl:template>
</xsl:stylesheet>
