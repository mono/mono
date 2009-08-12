<?xml version="1.0"?>

<!--
	mdoc-html-format.xsl: HTML pass-through formatting support

	Author: Jonathan Pryor (jpryor@novell.com)

-->

<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	>

	<!-- pass-through any other elements unchanged - they may be HTML -->
	<xsl:template match="//format[@type='text/html']//*">
		<xsl:copy>
			<xsl:copy-of select="@*" />
			<xsl:apply-templates select="*|node()" />
		</xsl:copy>
	</xsl:template>

</xsl:stylesheet>

