<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output encoding = "ISO-8859-1"/>
  <xsl:template match="/">
    <xsl:apply-templates select="/Assembly/Interface" />
    <xsl:apply-templates select="/Assembly/Class" />
    <xsl:apply-templates select="/Assembly/Struct" />
    <xsl:apply-templates select="/Assembly/Delegate" />
  </xsl:template>
  <xsl:template match="/Assembly/Interface">
    <xsl:call-template name="file"/>
  </xsl:template>
  <xsl:template match="/Assembly/Class">
    <xsl:call-template name="file"/>
  </xsl:template>
  <xsl:template match="/Assembly/Struct">
    <xsl:call-template name="file"/>
  </xsl:template>
  <xsl:template match="/Assembly/Delegate">
    <xsl:call-template name="file"/>
  </xsl:template>
  <xsl:template name="file">
    <xsl:if test="@Access[.!='private']">
      <xsl:text>&#13;&#10;</xsl:text>
      <xsl:element name="filestart">
        <xsl:attribute name="name">
          <xsl:value-of select="substring(@refid,3)"/>
          <xsl:text>.htm</xsl:text>
        </xsl:attribute>
      </xsl:element>
      <xsl:text>&#13;&#10;</xsl:text>
      <html >
        <head>
          <title>
            <xsl:text>C5 doc: </xsl:text>
            <xsl:value-of select="@Name" />
          </title>
          <link rel="stylesheet" type="text/css" href="../docnet.css" />
        </head>
        <xsl:element name="body">
          <xsl:attribute name="onLoad">
            <xsl:text>parent.document.title ='C5 doc: </xsl:text>
            <xsl:value-of select="@Name" />
            <xsl:text>'</xsl:text>
          </xsl:attribute>
          <h2>
            <xsl:value-of select="name()"/>
            <xsl:text>&#32;</xsl:text>
            <xsl:call-template name="htmlname" />
          </h2>
          <xsl:apply-templates select="summary" />
          <xsl:call-template name="typeparams" />
          <xsl:call-template name="implements" />
          <xsl:call-template name="implementedby" />
          <xsl:call-template name="super" />
          <xsl:apply-templates select="Bases" />
          <xsl:call-template name="baseof" />
          <xsl:call-template name="foverview" />
          <xsl:call-template name="eoverview" />
          <xsl:call-template name="poverview" />
          <xsl:call-template name="coverview" />
          <xsl:call-template name="moverview" />
          <xsl:call-template name="ooverview" />
          <xsl:call-template name="ftable" />
          <xsl:call-template name="etable" />
          <xsl:call-template name="ptable" />
          <xsl:call-template name="ctable" />
          <xsl:call-template name="mtable" />
          <xsl:call-template name="otable" />
        </xsl:element>
      </html>
    </xsl:if>
  </xsl:template>
  <xsl:template name="implements">
    <xsl:for-each select="Implements">
      <xsl:sort select="@refid" />
      <xsl:if test="position()=1">
        <h3>Implements</h3>
      </xsl:if>
      <xsl:call-template name="htmllink" />
      <xsl:if test="position()!=last()">, </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="super">
    <xsl:variable name="leRefid" select="@refid" />
    <xsl:for-each select="/Assembly/Interface[Implements[@refid = $leRefid ] and @Access != 'private']">
      <xsl:sort select="@Name" />
      <xsl:if test="position()=1">
        <h3>Super</h3>
      </xsl:if>
      <xsl:call-template name="htmllink" />
      <xsl:if test="position()!=last()">, </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="implementedby">
    <xsl:variable name="leRefid" select="@refid" />
    <xsl:for-each select="/Assembly/Class[Implements[@refid = $leRefid ] and @Access != 'private']">
      <xsl:sort select="@Name" />
      <xsl:if test="position()=1">
        <h3>Implemented by</h3>
      </xsl:if>
      <xsl:call-template name="htmllink" />
      <xsl:if test="position()!=last()">, </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="Bases">
    <xsl:if test="position()=1">
      <h3>Bases</h3>
    </xsl:if>
    <xsl:call-template name="htmllink" />
    <xsl:if test="position()!=last()">, </xsl:if>
  </xsl:template>
  <xsl:template name="baseof">
    <xsl:variable name="leRefid" select="@refid" />
    <xsl:for-each select="/Assembly/Class[Bases[@refid = $leRefid ] and @Access != 'private']">
      <xsl:sort select="@Name" />
      <xsl:if test="position()=1">
        <h3>Base of</h3>
      </xsl:if>
      <xsl:call-template name="htmllink" />
      <xsl:if test="position()!=last()">, </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <xsl:template match="param">
    <tr>
      <td valign="top">
        <!--code-->
        <xsl:value-of select="@name" />
        <!--/code-->
        <xsl:text>:</xsl:text>
      </td>
      <td valign="top">
        <xsl:apply-templates/>
      </td>
    </tr>
  </xsl:template>
  <xsl:template match="returns">
    <xsl:if test="current()[../@ReturnType!='void']">
      <tr>
        <td valign="top">
          <b>Returns:</b>
        </td>
        <td valign="top">
          <xsl:apply-templates/>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>
  <xsl:template match="Signature">
    <code>
      <xsl:value-of select="." />
    </code>
  </xsl:template>
  <xsl:template match="summary">
    <xsl:apply-templates />
  </xsl:template>
  <xsl:template match="value">
    <p>
      <b>Value:</b>
      <xsl:apply-templates />
    </p>
  </xsl:template>
  <!-- templates for VS 2005 doc tags-->
  <xsl:template match="exception">
    <xsl:choose>
      <xsl:when test="current()[name(..)='summary']">
        <b>/Throws</b>
        <xsl:value-of select="substring(@cref,3)" />
        <xsl:apply-templates />
      </xsl:when>
      <xsl:otherwise>
        <tr>
          <td valign="top">
            <xsl:variable name="leRefid" select="@cref" />
            <xsl:variable name="leExcNode" select="/Assembly/Class[@refid = $leRefid  and @Access != 'private']"/>
            <xsl:choose>
              <xsl:when test="$leExcNode">
                <xsl:for-each select="$leExcNode">
                  <xsl:call-template name="htmllink" />
                </xsl:for-each>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="substring(@cref,3)" />
              </xsl:otherwise>
            </xsl:choose>
          </td>
          <td valign="top">
            <xsl:apply-templates />
          </td>
        </tr>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--xsl:template match="code">
        <code>
            <xsl:apply-templates select="@* | node()" />
        </code>
    </xsl:template-->
  <xsl:template match="item">
    <li>
      <xsl:apply-templates select="@* | node()" />
    </li>
  </xsl:template>
  <!-- also do description and term tags, and other list types?-->
  <xsl:template match="list">
    <xsl:choose>
      <xsl:when test="@type='ordered'">
        <ol>
          <xsl:apply-templates select="@* | node()" />
        </ol>
      </xsl:when>
      <xsl:otherwise>
        <ul>
          <xsl:apply-templates select="@* | node()" />
        </ul>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template match="para">
    <p>
      <xsl:apply-templates select="@* | node()" />
    </p>
  </xsl:template>
  <xsl:template match="seealso">
    <xsl:text>See also</xsl:text>
    <xsl:variable name="leRefid" select="@cref" />
    <xsl:variable name="leNode" select="//*[@refid=$leRefid]" />
    <xsl:variable name="leFile" select="substring(ancestor::*[@refid and not(@Declared)]/@refid,3)" />
    <xsl:choose>
      <xsl:when test ="substring(@cref,1,2) = 'T:'">
        <xsl:element name="a">
          <xsl:attribute name="href">
            <xsl:value-of select="substring(@cref,3)" />
            <xsl:text>.htm</xsl:text>
          </xsl:attribute>
          <xsl:value-of select="$leNode/Signature" />
        </xsl:element>
      </xsl:when>
      <xsl:when test="$leNode/@CDeclared=$leFile">
        <xsl:element name="a">
          <xsl:attribute name="href">
            #<xsl:value-of select="@cref" />
          </xsl:attribute>
          <xsl:value-of select="$leNode/Signature" />
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="a">
          <xsl:attribute name="href">
            <xsl:value-of select="$leNode/@CDeclared" />.htm#<xsl:value-of select="@cref" />
          </xsl:attribute>
          <xsl:value-of select="$leNode/@Declared" />.<xsl:value-of select="$leNode/Signature" />
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template match="see">
    <!--xsl:text>See </xsl:text-->
    <xsl:variable name="leRefid" select="@cref" />
    <xsl:variable name="leNode" select="//*[@refid=$leRefid]" />
    <xsl:variable name="leFile" select="substring(ancestor::*[@refid and not(@Declared)]/@refid,3)" />
    <xsl:choose>
      <xsl:when test ="substring(@cref,1,2) = 'T:'">
        <xsl:element name="a">
          <xsl:attribute name="href">
            <xsl:value-of select="substring(@cref,3)" />
            <xsl:text>.htm</xsl:text>
          </xsl:attribute>
          <xsl:value-of select="$leNode/Signature" />
        </xsl:element>
      </xsl:when>
      <xsl:when test="$leNode/@CDeclared=$leFile">
        <xsl:element name="a">
          <xsl:attribute name="href">
            <xsl:text>#</xsl:text>
            <xsl:value-of select="@cref" />
          </xsl:attribute>
          <xsl:value-of select="$leNode/Signature" />
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:element name="a">
          <xsl:attribute name="href">
            <xsl:value-of select="$leNode/@CDeclared" />.htm#<xsl:value-of select="@cref" />
          </xsl:attribute>
          <xsl:value-of select="$leNode/@Declared" />.<xsl:value-of select="$leNode/Signature" />
        </xsl:element>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template match="typeparam">
    <tr>
      <td valign="top">
        <xsl:value-of select="@name" />
      </td>
      <td valign="top">
        <xsl:apply-templates />
      </td>
    </tr>
  </xsl:template>
  <xsl:template match="constraint">
    <tr>
      <td valign="top"></td>
      <td valign="top">
        <xsl:value-of select="@Value" />
      </td>
    </tr>
  </xsl:template>
  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>
  <!-- end templates for VS 2005 doc tags -->
  <xsl:template name="typeparams">
    <xsl:if test="typeparam">
      <table>
        <tr>
          <td>
            <b>Type parameters:</b>
          </td>
          <td></td>
        </tr>
        <xsl:apply-templates select="typeparam"/>
        <xsl:if test="constraint">
          <tr>
            <td>
              <b>Constraints:</b>
            </td>
            <td></td>
          </tr>
          <xsl:apply-templates select="constraint"/>
        </xsl:if>
      </table>
    </xsl:if>
  </xsl:template>
  <xsl:template name="htmllink">
    <xsl:choose>
      <xsl:when test="@C5">
        <xsl:element name="a">
          <xsl:attribute name="href">
            <xsl:choose>
              <xsl:when test ="substring(@refid,1,2) = 'T:'">
                <xsl:value-of select="substring(@refid,3)" />
                <xsl:text>.htm</xsl:text>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="@CDeclared"/>.htm#<xsl:value-of select="@refid" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:apply-templates select="Signature" />
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="Signature" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="locallink">
    <xsl:choose>
      <xsl:when test="@refid">
        <xsl:element name="a">
          <xsl:attribute name="href">
            <xsl:text>#</xsl:text>
            <xsl:value-of select="concat(../@refid , '|',@refid)" />
          </xsl:attribute>
          <xsl:apply-templates select="Signature" />
        </xsl:element>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="Signature" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="htmlname">
    <xsl:choose>
      <xsl:when test="@refid">
        <xsl:choose>
          <xsl:when test ="not(@Declared)">
            <!-- i.e. a type -->
            <xsl:element name="a">
              <xsl:attribute name="name">
                <xsl:value-of select="@refid" />
              </xsl:attribute>
              <xsl:apply-templates select="Signature" />
            </xsl:element>
          </xsl:when>
          <xsl:otherwise>
            <!-- i.e. a member -->
            <xsl:element name="a">
              <xsl:attribute name="name">
                <xsl:value-of select="concat(../@refid , '|',@refid)" />
              </xsl:attribute>
              <xsl:apply-templates select="Signature" />
            </xsl:element>
            <xsl:if test ="not(@Inherited)">
              <!-- the canonical description -->
              <xsl:element name="a">
                <xsl:attribute name="name">
                  <xsl:value-of select="@refid" />
                </xsl:attribute>
              </xsl:element>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="Signature" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="ftable">
    <xsl:call-template name="table">
      <xsl:with-param name="type">Field</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="etable">
    <xsl:call-template name="table">
      <xsl:with-param name="type">Event</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="ptable">
    <xsl:call-template name="table">
      <xsl:with-param name="type">Property</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="ctable">
    <xsl:call-template name="table">
      <xsl:with-param name="type">Constructor</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="mtable">
    <xsl:call-template name="table">
      <xsl:with-param name="type" select="'Method'"/>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="otable">
    <xsl:call-template name="table">
      <xsl:with-param name="type" select="'Operator'"/>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="table">
    <xsl:param name="type" />
    <xsl:param name="protection" />
    <xsl:variable name="thenodes" select="*[name() = $type and @Access != 'private' and not(@Inherited)]" />
    <xsl:if test="$thenodes">
      <h3>
        <xsl:value-of select="$type" />
        <xsl:text> details</xsl:text>
      </h3>
      <table border="1">
        <xsl:for-each select="$thenodes">
          <xsl:sort select="@Name" />
          <tr>
            <td valign="top">
              <xsl:if test="current()[@Virtual != 'True' and @Static != 'True']">
                <code class="greenbg">N</code>
              </xsl:if>
              <xsl:if test="current()[@Final = 'True' and @Static != 'True']">
                <code class="greenbg">F</code>
              </xsl:if>
              <xsl:if test="current()[@Abstract = 'True']">
                <code class="greenbg">A</code>
              </xsl:if>
              <xsl:if test="current()[@Static = 'True']">
                <code class="greenbg">S</code>
              </xsl:if>
              <xsl:if test="current()[@Access = 'protected']">
                <code class="greenbg">P</code>
              </xsl:if>
              <code>
                <xsl:text>&#32;</xsl:text>
                <xsl:choose>
                  <xsl:when test="@ReturnType">
                    <xsl:value-of select="@ReturnType"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="@Type"/>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:text>&#32;</xsl:text>
              </code>
              <xsl:call-template name="htmlname" />
            </td>
            <td>
              <!--xsl:if test="@Inherited">
                                <xsl:text>Inherited from </xsl:text>
                                <xsl:value-of select="@Declared"/>
                                <xsl:text>: </xsl:text>
                                <xsl:call-template name="htmllink" />
                            </xsl:if-->
              <xsl:if test="not(@Inherited)">
                <xsl:if test="@Get">
                  <b>Access: </b>
                  <xsl:if test="@Get='True' and @Set='True'">Read-Write</xsl:if>
                  <xsl:if test="@Get='True' and @Set='False'">Read-Only</xsl:if>
                  <xsl:if test="@Get='False' and @Set='True'">Write-Only</xsl:if>
                  <br/>
                </xsl:if>
                <xsl:apply-templates select="value" />
                <xsl:apply-templates select="summary" />
                <xsl:if test="exception">
                  <table>
                    <tr>
                      <td>
                        <b>Throws</b>
                      </td>
                      <td></td>
                    </tr>
                    <xsl:apply-templates select="exception" />
                  </table>
                </xsl:if>
                <xsl:call-template name="typeparams"/>
                <xsl:if test="current()[@ReturnType != 'void'] or param">
                  <table>
                    <xsl:apply-templates select="returns" />
                    <xsl:if test="param">
                      <tr>
                        <td>
                          <b>Parameters:</b>
                        </td>
                      </tr>
                      <xsl:apply-templates select="param" />
                    </xsl:if>
                  </table>
                </xsl:if>
              </xsl:if>
            </td>
          </tr>
        </xsl:for-each>
      </table>
    </xsl:if>
  </xsl:template>
  <xsl:template name="foverview">
    <xsl:call-template name="overview">
      <xsl:with-param name="type">Field</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="eoverview">
    <xsl:call-template name="overview">
      <xsl:with-param name="type">Event</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="poverview">
    <xsl:call-template name="overview">
      <xsl:with-param name="type">Property</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="coverview">
    <xsl:call-template name="overview">
      <xsl:with-param name="type">Constructor</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="moverview">
    <xsl:call-template name="overview">
      <xsl:with-param name="type">Method</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="ooverview">
    <xsl:call-template name="overview">
      <xsl:with-param name="type">Operator</xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="overview">
    <xsl:param name="type" select="'[Unknown Locale]'" />
    <xsl:for-each select="*[name() = $type and @Access != 'private']">
      <xsl:sort select="@Name" />
      <xsl:if test="position() = 1">
        <h3>
          <xsl:value-of select="$type" />
          <xsl:text> overview</xsl:text>
        </h3>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="@Inherited">
          <xsl:call-template name="htmllink" />
          <xsl:text>, </xsl:text>
          <xsl:text>Inherited from </xsl:text>
          <xsl:value-of select="@Declared"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="locallink" />
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="position()!=last()">
        ,<br/>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
