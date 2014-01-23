<?xml version="1.0"?>

<!--
	mono-ecma-css.xsl: ECMA-style docs to HTML+CSS stylesheet trasformation
	based on mono-ecma.xsl by Joshua Tauberer

	Author: Joshua Tauberer (tauberer@for.net)
	Author: Mario Sopena Novales (mario.sopena@gmail.com)

	TODO:
		split this into multiple files
-->

<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:monodoc="monodoc:///extensions"
	exclude-result-prefixes="monodoc"
	>
	<xsl:include href="mdoc-sections-css.xsl" />
	<xsl:include href="mono-ecma-impl.xsl" />
	
	<xsl:output omit-xml-declaration="yes" />

	<xsl:template name="CreateExpandedToggle">
		<img src="xtree/images/clean/Lminus.gif" border="0" align="top"/>
	</xsl:template>

	<xsl:template name="CreateCodeBlock">
		<xsl:param name="language" />
		<xsl:param name="content" />

		<div class="CodeExample">
			<p><b><xsl:value-of select="$language"/> Example</b></p>
			<div>
			<pre>
				<!--
				<xsl:value-of select="monodoc:Colorize($content, string($language))" 
					disable-output-escaping="yes" />
				  -->
				<xsl:value-of select="$content" />
			</pre>
			</div>
		</div>
	</xsl:template>

</xsl:stylesheet>
