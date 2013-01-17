<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:output omit-xml-declaration="yes" />

<xsl:template match="/clause">
	<table width="100%" cellpadding="5">
		<tr bgcolor="#b0c4de"><td>
		<i>ECMA-334 C# Language Specification</i>

		<h3>
			<xsl:value-of select="@number"/>: <xsl:value-of select="@title"/>
			
			<xsl:if test="@informative">
				(informative)
			</xsl:if>
		</h3>
		</td></tr>
	</table>
	
	<xsl:apply-templates />
</xsl:template>

<xsl:template match="paragraph">
	<p>
		<xsl:apply-templates />
	</p>
</xsl:template>

<xsl:template match="keyword">
        <i> <xsl:apply-templates/></i> <xsl:text> </xsl:text>
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
  <table bgcolor="#f5f5dd" border="1" cellpadding="5">
	<tr>
	  <td>
	    <pre>
		  <xsl:apply-templates />
	    </pre>
	  </td>
	</tr>
  </table>
</xsl:template>

<xsl:template match="symbol">
	<code>
		<xsl:apply-templates />
	</code>
</xsl:template>

<xsl:template match="grammar_production">
	<dl id="nt_{name/non_terminal/.}">
		<dt><xsl:value-of select="name/non_terminal/." /></dt>
		
		<xsl:for-each select="rhs">
		<dd>
			<xsl:apply-templates select="node()" />
		</dd>
		</xsl:for-each>
	</dl>
</xsl:template>

<xsl:template match="non_terminal">

	<code><xsl:text> </xsl:text><xsl:value-of select="." /></code>
</xsl:template>

<xsl:template match="terminal">
	<code><xsl:text> </xsl:text><xsl:value-of select="." /></code>
</xsl:template>

<xsl:template match="opt">
	<sub>opt</sub>
</xsl:template>

<xsl:template match="@*|node()">
	<xsl:copy>
		<xsl:apply-templates select="@*|node()"/>
	</xsl:copy>
</xsl:template>

</xsl:stylesheet>
