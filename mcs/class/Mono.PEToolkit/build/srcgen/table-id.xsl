<?xml version="1.0" encoding="iso-8859-1"?>


<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="text"/>


<xsl:template match="/">// Auto-generated file - DO NOT EDIT!
// Please edit md-schema.xml or table-id.xsl if you want to make changes.

using System;

namespace Mono.PEToolkit.Metadata {

<xsl:text><![CDATA[
	/// <summary>
	/// Identifiers for tables in #~ heap.
	/// </summary>
	/// <remarks>
	/// Partition II, 21.x
	/// </remarks>
	public enum TableId {
]]></xsl:text>

<xsl:for-each select="md-schema/tables/table">
<xsl:text>&#9;&#9;</xsl:text><xsl:value-of select="@name"/> = <xsl:value-of select="@id"/>,
</xsl:for-each>

		<!-- NOTE: bound values assigned explicitly based on XML definition,
		     so it's safe to add your own members below/above these lines.
		-->
		MAX = <xsl:value-of select="md-schema/tables/table[position()=last()]/@name"/>,
		Count = MAX + 1
	}

}
</xsl:template>


</xsl:stylesheet>
