<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output encoding = "ISO-8859-1"/>
    <xsl:template match="/">
        <html>
            <head>
                <title>
                    <xsl:text>DocNet documentation for</xsl:text>
                    <xsl:value-of select="Assembly/@Name" /></title>
                <link rel="stylesheet" type="text/css" href="docnet.css" />
            </head>
            <body>
                <h3>Interfaces overview</h3>
                <xsl:for-each select="/Assembly/Interface[@Access != 'private']">
                    <xsl:sort select="@Name" />
                    <xsl:call-template name="htmllink" />
                    <xsl:if test="position()!=last()">,<br/> </xsl:if>
                </xsl:for-each>
                <h3>Classes overview</h3>
                <xsl:for-each select="/Assembly/Class[@Access != 'private']">
                    <xsl:sort select="@Name" />
                    <xsl:call-template name="htmllink" />
                    <xsl:if test="position()!=last()">,<br/> </xsl:if>
                </xsl:for-each>
                <h3>Value Types overview</h3>
                <xsl:for-each select="/Assembly/Struct[@Access != 'private']">
                    <xsl:sort select="@Name" />
                    <xsl:call-template name="htmllink" />
                    <xsl:if test="position()!=last()">,<br/> </xsl:if>
                </xsl:for-each>
                <h3>Delegates overview</h3>
                <xsl:for-each select="/Assembly/Delegate[@Access != 'private']">
                    <xsl:sort select="@Name" />
                    <xsl:call-template name="htmllink" />
                    <xsl:if test="position()!=last()">,<br/> </xsl:if>
                </xsl:for-each>
            </body>
        </html>
    </xsl:template>
    <xsl:template match="Signature">
        <code>
            <xsl:value-of select="." />
        </code>
    </xsl:template>
    <xsl:template name="htmllink">
        <xsl:choose>
            <xsl:when test="@refid">
                <xsl:element name="a">
                    <xsl:attribute name="href">
                        <!--xsl:text>main.htm#</xsl:text>
                        <xsl:value-of select="@refid" /-->
                        <xsl:text>types/</xsl:text><xsl:value-of select="substring(@refid,3)" /><xsl:text>.htm</xsl:text>
                    </xsl:attribute>
                    <xsl:attribute name="target">
                        <xsl:text>main</xsl:text>
                    </xsl:attribute>
                    <xsl:apply-templates select="Signature" />
                </xsl:element>
            </xsl:when>
            <xsl:otherwise>
                <xsl:apply-templates select="Signature" />
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
</xsl:stylesheet>
