<?xml version="1.0"?>

<!--
	mdoc-sections-css.xsl: Common CSS implementation of mdoc-html-utils.xsl
	                       required functions.


	Including XSLT files need to provide the following functions:

		- CreateExpandedToggle()

	Author: Jonathan Pryor  <jpryor@novell.com>
-->

<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:monodoc="monodoc:///extensions"
	exclude-result-prefixes="monodoc"
	>

	<xsl:template name="CreateH2Section">
		<xsl:param name="name" />
		<xsl:param name="id" select="''" />
		<xsl:param name="child-id" select="generate-id (.)" />
		<xsl:param name="content" />

		<h2 class="Section">
			<xsl:if test="$id != ''">
				<xsl:attribute name="id">
					<xsl:value-of select="$id" />
				</xsl:attribute>
			</xsl:if>
			<xsl:value-of select="$name" />
		</h2>
		<div class="SectionBox" id="{$child-id}">
			<xsl:copy-of select="$content" />
		</div>
	</xsl:template>

	<xsl:template name="CreateH3Section">
		<xsl:param name="name" />
		<xsl:param name="id" select="''" />
		<xsl:param name="class" select="''" />
		<xsl:param name="child-id" select="generate-id (.)" />
		<xsl:param name="content" />

		<h3>
			<xsl:if test="$class != ''">
				<xsl:attribute name="class">
					<xsl:value-of select="$class" />
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="$id != ''">
				<xsl:attribute name="id">
					<xsl:value-of select="$id" />
				</xsl:attribute>
			</xsl:if>
			<xsl:value-of select="$name" />
		</h3>
		<blockquote id="{$child-id}">
			<xsl:copy-of select="$content" />
		</blockquote>
	</xsl:template>

	<xsl:template name="CreateH4Section">
		<xsl:param name="name" />
		<xsl:param name="id" select="''" />
		<xsl:param name="child-id" select="generate-id (.)" />
		<xsl:param name="content" />

		<h4 class="Subsection">
			<xsl:if test="$id != ''">
				<xsl:attribute name="id">
					<xsl:value-of select="$id" />
				</xsl:attribute>
			</xsl:if>
			<xsl:value-of select="$name" />
		</h4>
		<blockquote class="SubsectionBox" id="{$child-id}">
			<xsl:copy-of select="$content" />
		</blockquote>
	</xsl:template>

	<xsl:template name="CreateEnumerationTable">
		<xsl:param name="content" />
		<table class="Enumeration">
			<tr><th>Member Name</th><th>Description</th></tr>
			<xsl:copy-of select="$content" />
		</table>
	</xsl:template>

	<xsl:template name="CreateHeader">
		<xsl:param name="content" />
		<xsl:copy-of select="$content" />
	</xsl:template>

	<xsl:template name="CreateListTable">
		<xsl:param name="header" />
		<xsl:param name="content" />
		<table class="Documentation">
			<tr><xsl:copy-of select="$header" /></tr>
			<xsl:copy-of select="$content" />
		</table>
	</xsl:template>

	<xsl:template name="CreateMembersTable">
		<xsl:param name="content" />
		<table class="TypeMembers">
			<xsl:copy-of select="$content" />
		</table>
	</xsl:template>

	<xsl:template name="CreateSignature">
		<xsl:param name="content" />
		<xsl:param name="id" />
		<h2>Syntax</h2>
		<div class="Signature">
			<xsl:attribute name="id">
			  <xsl:copy-of select="$id" />
			</xsl:attribute>
			<xsl:copy-of select="$content" />
		</div>
	</xsl:template>

	<xsl:template name="CreateTypeDocumentationTable">
		<xsl:param name="content" />
		<table class="TypeDocumentation">
		<tr><th>Type</th><th>Reason</th></tr>
			<xsl:copy-of select="$content" />
		</table>
	</xsl:template>

</xsl:stylesheet>

