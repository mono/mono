<?xml version="1.0" encoding="iso-8859-1"?>


<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="text"/>


<xsl:template match="/">
// Auto-generated file - DO NOT EDIT!
// Please edit bitconverter.xsl if you want to make changes.

using System;

namespace Mono.PEToolkit {

	/// &lt;summary&gt;
	/// Little-endian bit converter.
	/// &lt;/summary&gt;
	public sealed class LEBitConverter {

		internal interface IConverter {

<xsl:apply-templates select="types/type" mode="ifc"/>
		}

		public static readonly bool Native = System.BitConverter.IsLittleEndian;

		private static readonly IConverter impl = System.BitConverter.IsLittleEndian
		                        ? new LEConverter() as IConverter
		                        : new BEConverter() as IConverter;




		private LEBitConverter()
		{
			// Never instantiated.
		}

<xsl:apply-templates select="types/type" mode="swap"/>



		internal sealed class LEConverter : IConverter {
<xsl:apply-templates select="types/type" mode="le"/>
		}

		internal sealed class BEConverter : IConverter {
<xsl:apply-templates select="types/type" mode="be"/>
		}




<xsl:apply-templates select="types/type" mode="main"/>

	}

}
</xsl:template>



<xsl:template match="types/type" mode="ifc">
	<xsl:value-of select="concat('&#9;&#9;&#9;',@short,' To',@name,'(byte [] val, int idx);&#xD;&#xA;')"/>
</xsl:template>


<xsl:template match="types/type" mode="main">
	<xsl:text>&#9;&#9;///&lt;summary&gt;&lt;/summary&gt;&#xD;&#xA;</xsl:text>
	<xsl:value-of select="concat('&#9;&#9;public static ',@short,' To',@name,'(byte [] val, int idx)&#xD;&#xA;')"/>
	<xsl:text>&#9;&#9;{&#xD;&#xA;</xsl:text>
	<xsl:text>&#9;&#9;&#9;return impl.To</xsl:text>
	<xsl:value-of select="@name"/>
	<xsl:text>(val, idx);&#xD;&#xA;</xsl:text>
	<xsl:text>&#9;&#9;}&#xD;&#xA;&#xD;&#xA;</xsl:text>
</xsl:template>


<xsl:template match="types/type" mode="le">
	<xsl:text>&#9;&#9;&#9;///&lt;summary&gt;&lt;/summary&gt;&#xD;&#xA;</xsl:text>
	<xsl:value-of select="concat('&#9;&#9;&#9;public ',@short,' To',@name,'(byte [] val, int idx)&#xD;&#xA;')"/>
	<xsl:text>&#9;&#9;&#9;{&#xD;&#xA;</xsl:text>
	<xsl:text>&#9;&#9;&#9;&#9;return BitConverter.To</xsl:text>
	<xsl:value-of select="@name"/>
	<xsl:text>(val, idx);&#xD;&#xA;</xsl:text>
	<xsl:text>&#9;&#9;&#9;}&#xD;&#xA;</xsl:text>
</xsl:template>


<xsl:template match="types/type" mode="be">
	<xsl:text>&#9;&#9;&#9;///&lt;summary&gt;&lt;/summary&gt;&#xD;&#xA;</xsl:text>
	<xsl:value-of select="concat('&#9;&#9;&#9;public ',@short,' To',@name,'(byte [] val, int idx)&#xD;&#xA;')"/>
	<xsl:text>&#9;&#9;&#9;{&#xD;&#xA;</xsl:text>
	<xsl:text>&#9;&#9;&#9;&#9;return Swap</xsl:text>
	<xsl:value-of select="@name"/>
	<xsl:text>(BitConverter.To</xsl:text>
	<xsl:value-of select="@name"/>
	<xsl:text>(val, idx));&#xD;&#xA;</xsl:text>
	<xsl:text>&#9;&#9;&#9;}&#xD;&#xA;</xsl:text>
</xsl:template>


<xsl:template match="types/type" mode="swap">
	<xsl:text>&#9;&#9;///&lt;summary&gt;&lt;/summary&gt;&#xD;&#xA;</xsl:text>
	<xsl:value-of select="concat('&#9;&#9;unsafe public static ',@short,' Swap',@name,'(',@short,' x)&#xD;&#xA;')"/>
	<xsl:text>&#9;&#9;{&#xD;&#xA;</xsl:text>
	<xsl:text>&#9;&#9;&#9;</xsl:text><xsl:value-of select="concat(@short,'* p = stackalloc ',@short,' [1];')"/><xsl:text>&#xD;&#xA;</xsl:text>
	<xsl:text>&#9;&#9;&#9;*p = x;&#xD;&#xA;</xsl:text>
	<xsl:text>&#9;&#9;&#9;byte* bp = (byte*) p;&#xD;&#xA;</xsl:text>
	<xsl:choose>
		<xsl:when test="@size = '2'">
			<xsl:text>&#9;&#9;&#9;byte b = bp [0];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [0] = bp [1];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [1] = b;&#xD;&#xA;</xsl:text>
		</xsl:when>
		<xsl:when test="@size = '4'">
			<xsl:text>&#9;&#9;&#9;byte b = bp [0];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [0] = bp [3];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [3] = b;&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;b = bp [1];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [1] = bp [2];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [2] = b;&#xD;&#xA;</xsl:text>
		</xsl:when>
		<xsl:when test="@size = '8'">
			<xsl:text>&#9;&#9;&#9;byte b = bp [0];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [0] = bp [7];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [7] = b;&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;b = bp [1];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [1] = bp [6];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [6] = b;&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;b = bp [2];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [2] = bp [5];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [5] = b;&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;b = bp [3];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [3] = bp [4];&#xD;&#xA;</xsl:text>
			<xsl:text>&#9;&#9;&#9;bp [4] = b;&#xD;&#xA;</xsl:text>
		</xsl:when>
		<xsl:otherwise>
		<xsl:text>&#9;&#9;&#9;// Not implemented&#xD;&#xA;</xsl:text>
		</xsl:otherwise>
	</xsl:choose>
	<xsl:text>&#9;&#9;&#9;return *p;&#xD;&#xA;</xsl:text>
	<xsl:text>&#9;&#9;}&#xD;&#xA;&#xD;&#xA;</xsl:text>
</xsl:template>


</xsl:stylesheet>
