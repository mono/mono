<?xml version="1.0"?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<html>
		<body>
			<table width="600">
			<tr>
				<td>
				<xsl:apply-templates/>
				</td>
			</tr>
			</table>
		</body>
		</html>
	</xsl:template>
	<xsl:template match="class">
		<h2>Class: <xsl:value-of select="name"/></h2>
		<h3>Inherits from: <xsl:value-of select="inheritfull"/></h3>
		<h3>Attributes:
		<xsl:for-each select="attribute">
			<xsl:value-of select="."/><xsl:if test="position()!=last()">, </xsl:if>
		</xsl:for-each>
		</h3>
		<hr></hr>
		<h3>Members:</h3>
		<h4>Constructors:</h4>
		<xsl:apply-templates select="member" mode="constructors">
			<xsl:sort select="name"/>
		</xsl:apply-templates>
		<h4>Methods:</h4>
		<xsl:apply-templates select="member" mode="methods">
			<xsl:sort select="name"/>
		</xsl:apply-templates>
		<h4>Properties:</h4>
		<xsl:apply-templates select="member" mode="properties">
			<xsl:sort select="name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="member" mode="constructors">
			<xsl:if test="type='constructor'">
				<xsl:value-of select="name"/>(<xsl:apply-templates select="param"/>)<br/>
				<dl>
					<xsl:apply-templates select="attribute"/>
					<xsl:apply-templates select="inheritfull"/>
				</dl>
			</xsl:if>
	</xsl:template>

	<xsl:template match="member" mode="methods">
			<xsl:if test="type='method'">
				<xsl:value-of select="name"/>(<xsl:apply-templates select="param"/>)<br/>
				<dl>
					<xsl:apply-templates select="attribute"/>
					<xsl:apply-templates select="inheritfull"/>
					<xsl:apply-templates select="return"/>
				</dl>
			</xsl:if>
	</xsl:template>

	<xsl:template match="member" mode="properties">
			<xsl:if test="type='property'">
				<xsl:value-of select="name"/><br/>
				<dl>
					<xsl:apply-templates select="attribute"/>
					<xsl:apply-templates select="inheritfull"/>
					<xsl:apply-templates select="property"/>
				</dl>
			</xsl:if>
	</xsl:template>

	<xsl:template match="param">
		<xsl:value-of select="type"/>&#160;&#160;<xsl:value-of select="name"/>
		<xsl:if test="position()!=last()">, </xsl:if>
	</xsl:template>

	<xsl:template match="attribute">
		<xsl:if test="position() = 1"><dd>Attributes:</dd></xsl:if>
		<dd><dl>
				<dd><xsl:value-of select="."/></dd>
			</dl>
		</dd>
	</xsl:template>

	<xsl:template match="inheritfull">
		<xsl:if test="position() = 1"><dd>Inherits from:</dd></xsl:if>
		<dd><dl>
				<dd><xsl:value-of select="."/></dd>
			</dl>
		</dd>
	</xsl:template>

	<xsl:template match="return">
		<xsl:if test="position() = 1"><dd>Return Type:</dd></xsl:if>
		<dd><dl>
				<dd><xsl:value-of select="."/></dd>
			</dl>
		</dd>
	</xsl:template>

	<xsl:template match="property">
		<xsl:if test="position() = 1"><dd>Property:</dd></xsl:if>
		<dd><dl>
				<dd><xsl:value-of select="."/></dd>
			</dl>
		</dd>
	</xsl:template>
</xsl:stylesheet>
