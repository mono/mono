<?xml version="1.0" encoding="iso-8859-1"?>


<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="text"/>


<xsl:template match="/">// Auto-generated file - DO NOT EDIT!
// Please edit md-schema.xml or tabs-base.xsl if you want to make changes.

using System;

namespace Mono.PEToolkit.Metadata {

<xsl:text><![CDATA[
	/// <summary>
	/// </summary>
	/// <remarks>
	/// </remarks>
	public abstract class TablesHeapBase : MDHeap {

		internal TablesHeapBase(MDStream stream) : base(stream)
		{
		}

		/// <summary>
		/// Gets or sets bitvector of valid tables (64-bit).
		/// </summary>
		public abstract long Valid {get; set;}

		/// <summary>
		/// Gets or sets bitvector of sorted tables (64-bit).
		/// </summary>
		public abstract long Sorted {get; set;}


		//
		// Accessors to decode Valid bitvector.
		//

]]></xsl:text>

<xsl:for-each select="md-schema/tables/table">
		/// &lt;summary&gt;
		/// True if heap has <xsl:value-of select="@name"/> table.
		/// &lt;/summary&gt;
		public bool Has<xsl:value-of select="@name"/> {
			get {
				return (Valid &amp; (1L &lt;&lt; <xsl:value-of select="@id"/>)) != 0;
			}
			set {
				long mask = (1L &lt;&lt; <xsl:value-of select="@id"/>);
				if (value) {
					Valid |= mask;
				} else {
					Valid &amp;= ~mask;
				}
			}
		}
</xsl:for-each>

	}

}
</xsl:template>


</xsl:stylesheet>
