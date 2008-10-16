<?xml version="1.0"?>

<!--
	mono-ecma.xsl: ECMA-style docs to HTML stylesheet trasformation

	Author: Joshua Tauberer (tauberer@for.net)

	TODO:
		split this into multiple files
-->

<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:monodoc="monodoc:///extensions"
	exclude-result-prefixes="monodoc"
	>
	<xsl:import href="mdoc-sections.xsl" />
	<xsl:import href="mono-ecma-impl.xsl" />
	
	<xsl:output omit-xml-declaration="yes" />

	<xsl:template name="CreateCodeBlock">
		<xsl:param name="language" />
		<xsl:param name="content" />
		<table class="CodeExampleTable" bgcolor="#f5f5dd" border="1" cellpadding="5" width="100%">
			<tr><td><b><xsl:value-of select="$language"/> Example</b></td></tr>
			<tr>
				<td>
					<!--
					<xsl:value-of select="monodoc:Colorize($content, string($language))" 
						disable-output-escaping="yes" />
						-->
					<pre>
						<xsl:value-of select="$content" />
					</pre>
				</td>
			</tr>
		</table>
	</xsl:template>

</xsl:stylesheet>
