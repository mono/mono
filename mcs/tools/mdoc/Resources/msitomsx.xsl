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
  <xsl:template match="authoring:changeHistory" />
  <xsl:template match="authoring:clsCompliantAlternative" />
  <xsl:template match="authoring:internalOnly" />
  <xsl:template match="authoring:notesForCallers" />
  <xsl:template match="authoring:notesForImplementers" />
  <xsl:template match="authoring:notesForInheritors" />
  <xsl:template match="authoring:overload" />
  <xsl:template match="authoring:relatedTopics" />

  <xsl:template match="member">
    <!-- skip Overload: members, as these have no msxdoc equivalent. -->
    <xsl:if test="not (starts-with (@name, 'Overload:'))">
      <member name="{@name}">
        <xsl:apply-templates />
      </member>
    </xsl:if>
  </xsl:template>

  <xsl:template match="authoring:remarks">
    <remarks>
      <xsl:apply-templates />
      <xsl:for-each select="../authoring:notesForInheritors">
        <block subset="none" type="overrides">
          <xsl:apply-templates />
        </block>
      </xsl:for-each>
      <xsl:for-each select="../authoring:notesForImplementers">
        <block subset="none" type="behaviors">
          <xsl:apply-templates />
        </block>
      </xsl:for-each>
      <xsl:for-each select="../authoring:notesForCallers">
        <block subset="none" type="usage">
          <xsl:apply-templates />
        </block>
      </xsl:for-each>
    </remarks>
  </xsl:template>

  <xsl:template match="authoring:dduexml" >
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="authoring:codeEntityReference">
    <see cref="{.}" />
  </xsl:template>

  <xsl:template match="authoring:equivalentCodeEntity">
    <seealso cref="{authoring:codeEntityReference}" />
  </xsl:template>

  <xsl:template match="authoring:codeInline">
    <c>
      <xsl:apply-templates />
    </c>
  </xsl:template>

  <xsl:template match="authoring:codeReference">
    <code src="{.}" />
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

  <xsl:template match="authoring:genericParameters">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="authoring:genericParameter">
    <typeparam name="{authoring:parameterReference}">
      <xsl:for-each select="*">
        <xsl:if test="not (position () = 1)">
          <xsl:apply-templates />
        </xsl:if>
      </xsl:for-each>
    </typeparam>
  </xsl:template>

  <xsl:template match="authoring:parameterReference">
    <paramref name="{.}" />
  </xsl:template>

  <xsl:template match="authoring:returnValue">
    <returns>
      <xsl:apply-templates />
    </returns>
  </xsl:template>

  <xsl:template match="authoring:exceptions">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="authoring:exception">
    <exception cref="{authoring:codeEntityReference}">
      <xsl:apply-templates select="authoring:content" />
    </exception>
  </xsl:template>

  <xsl:template match="authoring:codeExamples">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="authoring:codeExample">
    <xsl:choose>
      <xsl:when test="count(authoring:legacy) &gt; 0">
      </xsl:when>
      <xsl:otherwise>
        <example>
          <xsl:apply-templates select="authoring:description/authoring:content" />
          <xsl:apply-templates select="authoring:codeReference" />
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

  <xsl:template match="authoring:list">
    <list type="{@class}">
      <xsl:apply-templates />
    </list>
  </xsl:template>

  <xsl:template match="authoring:listItem">
    <item><term>
      <xsl:apply-templates />
    </term></item>
  </xsl:template>

  <xsl:template match="authoring:alert">
    <block subset="none" type="note">
      <xsl:apply-templates />
    </block>
  </xsl:template>

  <xsl:template match="authoring:permissions">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="authoring:permission">
    <permission cref="{authoring:codeEntityReference}">
      <xsl:apply-templates select="authoring:content" />
    </permission>
  </xsl:template>

  <xsl:template match="authoring:threadSafety">
    <threadsafe>
      <xsl:apply-templates />
    </threadsafe>
  </xsl:template>

  <xsl:template match="authoring:embeddedLabel">
    <i>
      <xsl:value-of select="." />
      <xsl:text>:</xsl:text>
    </i>
  </xsl:template>

  <xsl:template match="authoring:externalLink">
    <format type="text/html">
      <a href="{authoring:linkUri}">
        <xsl:value-of select="authoring:linkText" />
      </a>
    </format>
  </xsl:template>

  <xsl:template match="authoring:legacyLink">
    <i>
      <xsl:value-of select="." />
    </i>
  </xsl:template>

  <xsl:template match="authoring:token">
    <xsl:choose>
      <xsl:when test=". = 'compact_v20_long'">
        <xsl:text>.NET Compact Framework version 2.0</xsl:text>
      </xsl:when>
      <xsl:when test=". = 'compact_v35_long'">
        <xsl:text>.NET Compact Framework version 3.5</xsl:text>
      </xsl:when>
      <xsl:when test=". = 'dnprdnext'">
        <xsl:text>.NET Framework version 2.0</xsl:text>
      </xsl:when>
      <xsl:when test=". = 'vbprvbext'">
        <xsl:text>Microsoft Visual Basic 2005</xsl:text>
      </xsl:when>
      <xsl:when test=". = 'vbprvblong'">
        <xsl:text>Visual Basic 2005</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>UNKNOWN_TOKEN(</xsl:text>
        <xsl:value-of select="." />
        <xsl:text>)</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- cute trick to remove the xmlns attributes on copied nodes. -->
  <xsl:template match="*">
    <xsl:element name="{local-name()}">
      <xsl:apply-templates />
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>

