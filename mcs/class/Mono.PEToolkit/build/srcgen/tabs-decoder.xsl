<?xml version="1.0" encoding="iso-8859-1"?>


<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:import href="gen-utils.xsl"/>

<xsl:output method="text"/>


<!-- ******************************************************************* -->

<xsl:template match="/">// Auto-generated file - DO NOT EDIT!
// Please edit md-schema.xml or tabs-decoder.xsl if you want to make changes.

using System;

namespace Mono.PEToolkit.Metadata {

<xsl:text><![CDATA[
	/// <summary>
	/// </summary>
	/// <remarks>
	/// </remarks>
	public sealed class TabsDecoder {

		private TabsDecoder()
		{
		}


		/// <summary>
		/// </summary>
		/// <remarks>
		/// </remarks>
		public static MDToken DecodeToken(CodedTokenId id, int data)
		{
			MDToken res = new MDToken();
			int tag;
			int rid;
			TokenType tok;

			switch (id) {
]]></xsl:text>

<xsl:for-each select="md-schema/coded-tokens/map">
				case CodedTokenId.<xsl:value-of select="@name"/> :
					tag = data &amp; 0x<xsl:value-of select="substring('000103070F1F3F7FFF',1 + (2 * @bits),2)"/>;
					rid = (int) ((uint) data &gt;&gt; <xsl:value-of select="@bits"/>);
					switch (tag) {
<xsl:for-each select="table">
		<xsl:variable name="tok-type">
			<xsl:choose>
				<xsl:when test="boolean(@token-type)">
					<xsl:value-of select="@token-type"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@name"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
						case <xsl:value-of select="@tag"/> :
							tok = TokenType.<xsl:value-of select="$tok-type"/>;
							break;
</xsl:for-each>
						default :
							throw new BadMetaDataException("Invalid coded token for <xsl:value-of select="@name"/>, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;
</xsl:for-each>


<xsl:text><![CDATA[
				default:
					break;
			}
			return res;
		}


		private static int GetCodedIndexSize(TablesHeap heap, CodedTokenId id, int [] rows)
		{
			int res = 0;

			switch (id) {
]]></xsl:text>

<xsl:for-each select="md-schema/coded-tokens/map">
				case CodedTokenId.<xsl:value-of select="@name"/> :
					res = MDUtils.Max(<xsl:call-template name="get-tables-list"/>);
					res = res &lt; (1 &lt;&lt; (16 - <xsl:value-of select="@bits"/>)) ? 2 : 4;
					break;
</xsl:for-each>

<xsl:text><![CDATA[
				default:
					break;
			}

			return res;
		}


		private static int GetIndexSize(TableId tab, int [] rows)
		{
			// Index is 2 bytes wide if table has less than 2^16 rows
			// otherwise it's 4 bytes wide.
			return ((uint) rows [(int) tab]) < (1 << 16) ? 2 : 4;
		}


		private static void AllocBuff(ref byte [] buff, int size)
		{
			if (buff == null || buff.Length < size) {
				buff = new byte [(size + 4) & ~3];
			}
			Array.Clear(buff, 0, size);
		}


		/// <summary>
		/// </summary>
		unsafe public static int DecodePhysicalTables(TablesHeap heap, byte [] data, int offs, int [] rows)
		{
			int rowSize; // expanded row size (all indices are dwords)
			int fldSize; // physical field size
			int dest;
			int nRows;
			byte [] buff = null;
			int si = heap.StringsIndexSize;
			int gi = heap.GUIDIndexSize;
			int bi = heap.BlobIndexSize;
]]></xsl:text>

	<xsl:apply-templates select="md-schema/tables/table"/>

<xsl:text><![CDATA[
			return offs;
		}

	} // end class
} // end namespace
]]></xsl:text>

</xsl:template>



<!-- ******************************************************************* -->
<xsl:template name="get-tables-list">
	<xsl:param name="map-node" select="."/>

	<xsl:for-each select="$map-node/table">
		<xsl:choose>
			<xsl:when test="@name = 'String'"><!-- HACK -->
				<xsl:text>(heap.StringsIndexSize &gt; 2 ? 1 &lt;&lt; 17 : 1)</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>rows [(int) TableId.</xsl:text><xsl:value-of select="@name"/><xsl:text>]</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="position() != last()">
			<xsl:text>, </xsl:text>
		</xsl:if>
	</xsl:for-each>
</xsl:template>





<!-- ******************************************************************* -->
<xsl:template name="get-field-size">
	<xsl:param name="type" select="@type"/>

	<xsl:choose>
		<!-- RVA special case, PE library type -->
		<xsl:when test="$type = 'RVA'">
			<xsl:text>RVA.Size</xsl:text>
		</xsl:when>
		<!-- #Strings, #Blob or #GUID -->
		<xsl:when test="contains($type,'#')">
			<xsl:choose>
				<xsl:when test="contains(substring-after($type, '#'), 'Strings')">
					<xsl:text>si</xsl:text>
				</xsl:when>
				<xsl:when test="contains(substring-after($type, '#'), 'Blob')">
					<xsl:text>bi</xsl:text>
				</xsl:when>
				<xsl:when test="contains(substring-after($type, '#'), 'GUID')">
					<xsl:text>gi</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>ERROR: Unknown index - </xsl:text>
					<xsl:value-of select="$type"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:when>
		<!-- table indices -->
		<xsl:when test="starts-with($type,'index')">
			<xsl:variable name="tab">
				<xsl:call-template name="extract-arg"/>
			</xsl:variable>
			<xsl:value-of select="concat('GetIndexSize(TableId.', $tab, ', rows)')"/>
		</xsl:when>
		<!-- coded tokens -->
		<xsl:when test="starts-with($type,'coded-index')">
			<xsl:variable name="tab">
				<xsl:call-template name="extract-arg"/>
			</xsl:variable>
			<xsl:value-of select="concat('GetCodedIndexSize(heap, CodedTokenId.', $tab, ', rows)')"/>
		</xsl:when>
		<xsl:otherwise>
			<xsl:value-of select="concat('sizeof (', $type, ')')"/>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>




<!-- ******************************************************************* -->
<xsl:template match="md-schema/tables/table">
			if (heap.Has<xsl:value-of select="@name"/>) {
				rowSize = <xsl:call-template name="get-expanded-size"/>;
				nRows = rows [(int) TableId.<xsl:value-of select="@name"/>];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new <xsl:value-of select="@name"/>Table(heap);

				for (int i = nRows; --i >= 0;) {
	<xsl:for-each select="schema/field">
		<xsl:variable name="fld-size">
			<xsl:call-template name="get-field-size"/>
		</xsl:variable>
		<xsl:variable name="exp-fld-size">
			<xsl:call-template name="get-expanded-size">
				<xsl:with-param name="fields" select="."/>
			</xsl:call-template>
		</xsl:variable>
					// <xsl:value-of select="@name"/>, <xsl:value-of select="@type"/>
					fldSize = <xsl:value-of select="$fld-size"/>;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += <xsl:value-of select="$exp-fld-size"/>;
	</xsl:for-each>
				}

				tab.FromRawData(buff, 0, nRows);
			}
</xsl:template>

</xsl:stylesheet>
