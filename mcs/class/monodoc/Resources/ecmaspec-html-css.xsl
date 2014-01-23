<?xml version="1.0"?>
<xsl:stylesheet 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	version="1.0"
	xmlns:monodoc="monodoc:///extensions"
	exclude-result-prefixes="monodoc"
	>
<xsl:output omit-xml-declaration="yes" />

<xsl:template match="/clause">
	<div class="header" id="ecmaspec">
		<div class="subtitle">ECMA-334 C# Language Specification</div> 
		<div class="title"><xsl:value-of select="@number"/>: <xsl:value-of select="@title"/>
		<xsl:if test="@informative"> (informative) </xsl:if></div>
	</div>
	<xsl:apply-templates />
</xsl:template>

<xsl:template match="paragraph">
	<p>
		<xsl:apply-templates />
	</p>
</xsl:template>

<xsl:template match="keyword">
        <span class="keyword"> <xsl:apply-templates/></span> <xsl:text> </xsl:text>
</xsl:template>

<xsl:template match="hyperlink">
	<a href="ecmaspec:{.}">
		<xsl:value-of select="." />
	</a>
</xsl:template>

<xsl:template match="list">
	<ul>
		<xsl:for-each select="list_item|list">
			<li><xsl:apply-templates /></li>
		</xsl:for-each>
	</ul>
</xsl:template>

<xsl:template match="code_example">
	<div class="code_example">
	   <div class="code_ex_title">Code example</div>
	   <span class="code">
		  <xsl:value-of select="monodoc:Colorize(string(descendant-or-self::text()), string('csharp'))" disable-output-escaping="yes" />
	   </span>
	</div>
</xsl:template>

<xsl:template match="symbol">
	<span class="symbol">
		<xsl:apply-templates />
	</span>
</xsl:template>

<xsl:template match="grammar_production">
	<dl class="nt_{name/non_terminal/.}">
		<dt><xsl:value-of select="name/non_terminal/." /></dt>
		
		<xsl:for-each select="rhs">
		<dd>
			<xsl:apply-templates select="node()" />
		</dd>
		</xsl:for-each>
	</dl>
</xsl:template>

<xsl:template match="non_terminal">
	<span class="non_terminal"><xsl:text> </xsl:text><xsl:value-of select="." /></span>
</xsl:template>

<xsl:template match="terminal">
	<span class="terminal"><xsl:text> </xsl:text><xsl:value-of select="." /></span>
</xsl:template>

<xsl:template match="opt">
	<xsl:text> (</xsl:text><span class="opt">optional</span><xsl:text>) </xsl:text>
</xsl:template>

<xsl:template match="note|example">
	<div class="note">
		<xsl:apply-templates />
	</div>
</xsl:template>

<xsl:template match="table_line">
    <xsl:apply-templates /><br />
</xsl:template>

<xsl:template match="@*|node()">
	<xsl:copy>
		<xsl:apply-templates select="@*|node()"/>
	</xsl:copy>
</xsl:template>

</xsl:stylesheet>
