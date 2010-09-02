<?xml version="1.0"?>
<!--
  Converts the "Microsoft Internal XML Documentation Format" into the 
  "Microsoft XML Documentation Format".

  The "Microsoft Internal XML Documentation Format" (msidoc) is whatever XML
  format is used within Microsoft to document the BCL, as deduced from reading
  their ECMA documentation dump.

  The "Microsoft XML Documentation Format" (msxdoc) is what 'gmcs /doc' 
  produces, and is documented in ECMA 334 §E.

  msidoc is similar, but not identical to, msxdoc.  For example, where msxdoc
  uses <see cref="FOO"/>, msidoc uses
  <codeEntityReference>FOO</codeEntityReference>.  They also introduce
  additional "wrapping" elements in various places (e.g. <content/>), useful
  extensions (such as documenting method overload lists), and other oddities.
  -->
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:authoring="http://ddue.schemas.microsoft.com/authoring/2003/5"
  exclude-result-prefixes="msxsl authoring"
  >

  <xsl:output omit-xml-declaration="yes" />

  <xsl:template match="assembly" />

  <xsl:template match="member">
    <!-- skip Overload: members, as these have no msxdoc equivalent. -->
    <xsl:if test="not (starts-with (@name, 'Overload:'))">
      <member name="{@name}">
        <xsl:apply-templates />
      </member>
    </xsl:if>
  </xsl:template>

  <xsl:template match="authoring:dduexml" >
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="authoring:codeEntityReference">
    <see cref="{.}" />
  </xsl:template>

  <xsl:template match="authoring:parameters">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="authoring:parameter">
    <param name="{authoring:parameterReference}">
      <xsl:for-each select="*">
        <xsl:if test="not (position () = 1)">
          <xsl:apply-templates />
        </xsl:if>
      </xsl:for-each>
    </param>
  </xsl:template>

  <xsl:template match="authoring:parameterReference">
    <paramref name="{.}" />
  </xsl:template>

  <xsl:template match="authoring:returnValue">
    <returns>
      <xsl:apply-templates />
    </returns>
  </xsl:template>

  <xsl:template match="authoring:codeExamples">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="authoring:exceptions">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="authoring:exception">
    <exception cref="{authoring:codeEntityReference}">
      <xsl:apply-templates select="authoring:content" />
    </exception>
  </xsl:template>

  <xsl:template match="authoring:overload" />

  <xsl:template match="authoring:codeExample">
    <xsl:choose>
      <xsl:when test="count(authoring:legacy) &gt; 0">
      </xsl:when>
      <xsl:otherwise>
        <example>
          <xsl:apply-templates />
        </example>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="authoring:content">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="authoring:languageKeyword">
    <see langword="{.}" />
  </xsl:template>

  <xsl:template match="authoring:table">
    <list type="table">
      <xsl:apply-templates />
    </list>
  </xsl:template>

  <xsl:template match="authoring:tableHeader">
    <listheader>
      <xsl:for-each select="authoring:row/authoring:entry">
        <xsl:choose>
          <xsl:when test="position() = 1">
            <term>
              <xsl:apply-templates />
            </term>
          </xsl:when>
          <xsl:otherwise>
            <description>
              <xsl:apply-templates />
            </description>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </listheader>
  </xsl:template>

  <xsl:template match="authoring:row">
    <item>
      <xsl:for-each select="authoring:entry">
        <xsl:choose>
          <xsl:when test="position() = 1">
            <term>
              <xsl:apply-templates />
            </term>
          </xsl:when>
          <xsl:otherwise>
            <description>
              <xsl:apply-templates />
            </description>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </item>
  </xsl:template>

  <!-- cute trick to remove the xmlns attributes on copied nodes. -->
  <xsl:template match="*">
    <xsl:element name="{local-name()}">
      <xsl:apply-templates />
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>

