<?xml version="1.0" encoding="iso-8859-1"?>

<!-- -->


<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">


<!-- ******************************************************************* -->
<!-- extracts arg x from expression of from 'f(x)' -->
<xsl:template name="extract-arg">
	<xsl:param name="expr" select="@type"/>
	<xsl:value-of select="normalize-space(substring-after(substring-before($expr,')'),'('))"/>
</xsl:template>


<!-- ******************************************************************* -->
<xsl:template name="get-expanded-size">
	<xsl:param name="fields" select="schema/field"/>

	<xsl:for-each select="$fields">
		<xsl:choose>
			<!-- RVA special case, PE library type -->
			<xsl:when test="@type = 'RVA'">
				<xsl:text>RVA.Size</xsl:text>
			</xsl:when>
			<!-- table indices -->
			<xsl:when test="starts-with(@type,'index') or starts-with(@type,'coded-index')">
				<xsl:text>4</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat('sizeof (', @type, ')')"/>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="position() != last()">
			<xsl:text> + </xsl:text>
		</xsl:if>
	</xsl:for-each>
</xsl:template>




<!-- ******************************************************************* -->
<xsl:template name="get-field-type">
	<xsl:param name="field" select="."/>

	<xsl:choose>
		<!-- table indices -->
		<xsl:when test="starts-with($field/@type,'index')">
			<xsl:text>int</xsl:text>
		</xsl:when>
		<!-- coded token -->
		<xsl:when test="starts-with($field/@type,'coded-index')">
			<xsl:text>MDToken</xsl:text>
		</xsl:when>
		<!-- explicit library type -->
		<xsl:when test="$field/@cli-type">
			<xsl:value-of select="$field/@cli-type"/>
		</xsl:when>
		<!-- primitive type -->
		<xsl:otherwise>
			<xsl:value-of select="$field/@type"/>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>




</xsl:stylesheet>

