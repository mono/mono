<?xml version="1.0"?>

<!--
	mdoc-sections.xsl: Common non-CSS implementation of mdoc-html-utils.xsl
	                   required functions.

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
			<xsl:value-of select="$name" />
		</h2>
		<blockquote id="{$child-id}">
			<xsl:copy-of select="$content" />
		</blockquote>
	</xsl:template>

	<xsl:template name="CreateH3Section">
		<xsl:param name="name" />
		<xsl:param name="id" select="''" />
		<xsl:param name="class" select="''" />
		<xsl:param name="child-id" select="generate-id (.)" />
		<xsl:param name="content" />

		<h3>
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
			<xsl:value-of select="$name" />
		</h4>
		<blockquote class="SubsectionBox" id="{$child-id}">
			<xsl:copy-of select="$content" />
		</blockquote>
	</xsl:template>

	<xsl:template name="CreateEnumerationTable">
		<xsl:param name="content" />
		<table class="EnumerationsTable" border="1" cellpadding="10" width="100%">
			<tr bgcolor="#f2f2f2">
				<th>Member Name</th>
				<th>Description</th>
			</tr>
			<xsl:copy-of select="$content" />
		</table>
	</xsl:template>

	<xsl:template name="CreateHeader">
		<xsl:param name="content" />
		<table class="HeaderTable" width="100%" cellpadding="5">
			<tr bgcolor="#b0c4de">
				<td>
					<xsl:copy-of select="$content" />
				</td>
			</tr>
		</table>
	</xsl:template>

	<xsl:template name="CreateListTable">
		<xsl:param name="header" />
		<xsl:param name="content" />
		<table border="1" cellpadding="3" width="100%">
			<tr bgcolor="#f2f2f2" valign="top">
				<xsl:copy-of select="$header" />
			</tr>
			<xsl:copy-of select="$content" />
		</table>
	</xsl:template>

	<xsl:template name="CreateMembersTable">
		<xsl:param name="content" />
		<table border="1" cellpadding="6" width="100%">
			<xsl:copy-of select="$content" />
		</table>
	</xsl:template>

	<xsl:template name="CreateSignature">
		<xsl:param name="content" />
		<xsl:param name="id" />
		<table class="SignatureTable" bgcolor="#c0c0c0" cellspacing="0" width="100%">
		  <xsl:attribute name="id">
			<xsl:copy-of select="$id" />
		  </xsl:attribute>
		<tr><td>
			<table class="InnerSignatureTable" cellpadding="10" cellspacing="0" width="100%">
			<tr bgcolor="#f2f2f2">
				<td>
				<xsl:copy-of select="$content" />
			</td></tr>
			</table>
		</td></tr>
		</table>
		<br />
	</xsl:template>
	
	<xsl:template name="CreateTypeDocumentationTable">
		<xsl:param name="content" />
		<table class="TypePermissionsTable" border="1" cellpadding="6" width="100%">
		<tr bgcolor="#f2f2f2"><th>Type</th><th>Reason</th></tr>
			<xsl:copy-of select="$content" />
		</table>
	</xsl:template>

</xsl:stylesheet>
