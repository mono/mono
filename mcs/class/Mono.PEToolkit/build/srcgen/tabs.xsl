<?xml version="1.0" encoding="iso-8859-1"?>


<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="text"/>


<xsl:template match="/">// Auto-generated file - DO NOT EDIT!
// Please edit md-schema.xml or tabs.xsl if you want to make changes.

using System;

namespace Mono.PEToolkit.Metadata {

<xsl:for-each select="md-schema/tables/table">
	public class <xsl:value-of select="@name"/>Table : MDTableBase {

		public <xsl:value-of select="@name"/>Table(MDHeap heap)
		: base(heap)
		{
		}


		public override void FromRawData(byte [] buff, int offs, int numRows) {
			for (int i = numRows; --i >= 0;) {
				Row row = new <xsl:value-of select="@name"/>Row(this);
				row.FromRawData(buff, offs);
				Add(row);
				offs += <xsl:value-of select="@name"/>Row.LogicalSize;
			}
		}


		public override string Name {
			get {
				return "<xsl:value-of select="@name"/>";
			}
		}

		public override TableId Id {
			get {
				return TableId.<xsl:value-of select="@name"/>;
			}
		}
	}
</xsl:for-each>

}
</xsl:template>


</xsl:stylesheet>
