<?xml version="1.0" encoding="iso-8859-1"?>


<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="text"/>


<xsl:template match="/">// Auto-generated file - DO NOT EDIT!
// Please edit md-schema.xml or coded-id.xsl if you want to make changes.

using System;

namespace Mono.PEToolkit.Metadata {

<xsl:text><![CDATA[
	/// <summary>
	/// </summary>
	/// <remarks>
	/// </remarks>
	public enum CodedTokenId {

]]></xsl:text>

<xsl:for-each select="md-schema/coded-tokens/map">
<xsl:text>&#9;&#9;</xsl:text><xsl:value-of select="@name"/> = <xsl:value-of select="position() - 1"/>,
</xsl:for-each>
	}

}
</xsl:template>


</xsl:stylesheet>
