<?xml version="1.0" encoding="iso-8859-1"?>


<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:import href="gen-utils.xsl"/>
<xsl:output method="text"/>


<!--
-->

<xsl:template match="/">// Auto-generated file - DO NOT EDIT!
// Please edit md-schema.xml or rows.xsl if you want to make changes.

using System;
using System.IO;

namespace Mono.PEToolkit.Metadata {

<xsl:for-each select="md-schema/tables/table">

	/// &lt;summary&gt;
	///  Represents row in <xsl:value-of select="@name"/> table.
	/// &lt;/summary&gt;
	/// &lt;remarks&gt;
	///  <xsl:if test="@section">See Partition II, Metadata; section <xsl:value-of select="@section"/></xsl:if>
	/// &lt;/remarks&gt;
	public class <xsl:value-of select="@name"/>Row : Row {

		private MDTable table;

		<xsl:for-each select="schema/field">
		public <xsl:call-template name="get-field-type"/><xsl:value-of select="concat(' ',@name)"/>;</xsl:for-each>

		public <xsl:value-of select="@name"/>Row()
		{
		}

		public <xsl:value-of select="@name"/>Row(MDTable parent)
		{
			table = parent;
		}


		/// &lt;summary&gt;
		///  Row in <xsl:value-of select="@name"/> table has <xsl:value-of select="count(schema/field)"/> columns.
		/// &lt;/summary&gt;
		public virtual int NumberOfColumns {
			get {
				return <xsl:value-of select="count(schema/field)"/>;
			}
		}


		/// &lt;summary&gt;
		///  Logical size of this instance in bytes.
		/// &lt;/summary&gt;
		public virtual int Size {
			get {
				return LogicalSize;
			}
		}


		/// &lt;summary&gt;
		/// &lt;/summary&gt;
		public virtual MDTable Table {
			get {
				return table;
			}
		}


		/// &lt;summary&gt;
		///  Logical size of this type of row in bytes.
		/// &lt;/summary&gt;
		unsafe public static int LogicalSize {
			get {
				return <xsl:call-template name="get-expanded-size"/>;
			}
		}


		/// &lt;summary&gt;
		///  Fills the row from the array of bytes.
		/// &lt;/summary&gt;
		unsafe public void FromRawData(byte [] buff, int offs)
		{
			if (buff == null) throw new Exception("buff == null");
			if (offs + Size > buff.Length) throw new Exception("bounds");

		<xsl:for-each select="schema/field">
			this.<xsl:value-of select="@name"/> = <xsl:call-template name="get-field-conversion-code"/>;
			<xsl:if test="position() != last()">
			<xsl:text>offs += </xsl:text>
			<xsl:call-template name="get-expanded-size">
				<xsl:with-param name="fields" select="."/>
			</xsl:call-template>
			<xsl:text>;</xsl:text>
			</xsl:if>
		</xsl:for-each>
		}

		<xsl:variable name="spaces" select="'                                '"/>

		/// &lt;summary&gt;
		/// &lt;/summary&gt;
		public void Dump(TextWriter writer) {
			<xsl:text>string dump = String.Format(</xsl:text>
			<xsl:for-each select="schema/field">
				"<xsl:value-of select="concat(@name,substring($spaces,1,18 - string-length(@name)))"/>: {<xsl:value-of select="position () - 1"/>}" <xsl:text>+ Environment.NewLine</xsl:text>
				<xsl:if test="position() != last()"><xsl:text> + </xsl:text></xsl:if>
				<xsl:if test="position() = last()"><xsl:text>,</xsl:text></xsl:if>
			</xsl:for-each>
			<xsl:for-each select="schema/field">
				<xsl:choose><!-- TODO: do something about ugly expression below -->
				<xsl:when test="contains(@type,'#Strings')">
				(<xsl:text>Table == null) ? </xsl:text>
				<xsl:value-of select="concat(@name, '.ToString()')"/>
				<xsl:text> : "\"" + ((Table.Heap.Stream.Root.Streams["#Strings"] as MDStream).Heap as StringsHeap) [</xsl:text>
				<xsl:value-of select="@name"/><xsl:text>] + "\" (#Strings[0x" + </xsl:text><xsl:value-of select="@name"/><xsl:text>.ToString("X") + "])"</xsl:text>
				</xsl:when>
				<xsl:when test="contains(@type,'index') and not(contains(@type,'coded-index'))">
				&quot;<xsl:call-template name="extract-arg"/>[&quot; + <xsl:value-of select="@name"/><xsl:text>.ToString() + "]"</xsl:text>
				</xsl:when>
				<xsl:otherwise>
				this.<xsl:value-of select="@name"/>
				</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="position() != last()"><xsl:text>,</xsl:text></xsl:if>
			</xsl:for-each>
			);
			writer.WriteLine(dump);
		}


		/// &lt;summary&gt;
		/// &lt;/summary&gt;
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}

</xsl:for-each>


}
</xsl:template>






<!-- ******************************************************************* -->
<xsl:template name="get-field-conversion-code">
	<xsl:param name="field" select="."/>
	<xsl:variable name="type" select="$field/@type"/>

	<xsl:choose>
		<!-- RVA - library type -->
		<xsl:when test="$type = 'RVA'">
			<xsl:text>LEBitConverter.ToUInt32(buff, offs)</xsl:text>
		</xsl:when>

		<!-- table indices -->
		<xsl:when test="starts-with($type,'index')">
			<xsl:text>LEBitConverter.ToInt32(buff, offs)</xsl:text>
		</xsl:when>

		<!-- coded tokens -->
		<xsl:when test="starts-with($type,'coded-index')">
			<xsl:text>TabsDecoder.DecodeToken(CodedTokenId.</xsl:text>
			<xsl:call-template name="extract-arg">
				<xsl:with-param name="expr" select="$type"/>
			</xsl:call-template>
			<xsl:text>, LEBitConverter.ToInt32(buff, offs))</xsl:text>
		</xsl:when>

		<!-- primitive type -->
		<xsl:otherwise>
			<!-- explicitly mapped to library type -->
			<xsl:if test="$field/@cli-type">
				<xsl:value-of select="concat('(', $field/@cli-type, ') ')"/>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="$type = 'byte'">
					<xsl:text>buff [offs]</xsl:text>
				</xsl:when>
				<xsl:when test="$type = 'short'">
					<xsl:text>LEBitConverter.ToInt16(buff, offs)</xsl:text>
				</xsl:when>
				<xsl:when test="$type = 'ushort'">
					<xsl:text>LEBitConverter.ToUInt16(buff, offs)</xsl:text>
				</xsl:when>
				<xsl:when test="$type = 'int'">
					<xsl:text>LEBitConverter.ToInt32(buff, offs)</xsl:text>
				</xsl:when>
				<xsl:when test="$type = 'uint'">
					<xsl:text>LEBitConverter.ToUInt32(buff, offs)</xsl:text>
				</xsl:when>
				<xsl:when test="$type = 'long'">
					<xsl:text>LEBitConverter.ToInt64(buff, offs)</xsl:text>
				</xsl:when>
				<xsl:when test="$type = 'ulong'">
					<xsl:text>LEBitConverter.ToUInt64(buff, offs)</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>/* ERROR! */</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>





</xsl:stylesheet>
