<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output encoding = "ISO-8859-1" omit-xml-declaration="yes" indent="no"/>
    <xsl:strip-space elements="*"/>
    <xsl:template match="table">
        <xsl:text>\begin{tabular}{lc|*{15}{c}}\hline\hline&#10;</xsl:text>
        <xsl:apply-templates></xsl:apply-templates>
        <xsl:text>\hline\hline&#10;\end{tabular}&#10;</xsl:text>
    </xsl:template>
    <xsl:template match="tr">
        <xsl:choose>
            <xsl:when test="td/i">
                <xsl:text>\hline&#10;%</xsl:text><xsl:value-of select="td/i/font"/><xsl:text>&#10;</xsl:text>
            </xsl:when>
            <xsl:otherwise>
                <xsl:for-each select="th">
                    <xsl:if test="position()!=1">\turned{</xsl:if>
                    <xsl:apply-templates/>
                    <xsl:if test="position()!=1">}</xsl:if>
                    <xsl:if test="position()!=last()"><xsl:text disable-output-escaping="yes" >&#10;&amp;&#32;</xsl:text></xsl:if>
                </xsl:for-each>
                <xsl:for-each select="td">
                    <xsl:if test="position()=1">\texttt{</xsl:if>
                    <xsl:if test="position()!=1">$</xsl:if>
                    <xsl:apply-templates/>
                    <xsl:if test="position()=1">}</xsl:if>
                    <xsl:if test="position()!=1">$</xsl:if>
                    <xsl:if test="position()!=last()"><xsl:text disable-output-escaping="yes" >&#32;&amp;&#32;</xsl:text></xsl:if>
                </xsl:for-each>
                <!---->\\&#10;<!---->
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    <xsl:template match ="th">
        <xsl:apply-templates/>
    </xsl:template>
    <xsl:template match ="td">
        <xsl:apply-templates/>
    </xsl:template>
    <xsl:template match="font">
        <xsl:apply-templates/>
    </xsl:template>
</xsl:stylesheet>