<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" 
>
<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:template match="doc/bibref" >
<xsl:variable name="name" select="."/> 
<xsl:value-of select="$name"/>
    <xsl:for-each select="document('91834a.xml')">
        <xsl:apply-templates select="key('bib', 'XML')"/>
    </xsl:for-each>
</xsl:template>

<xsl:key name="bib" match="entry" use="@name"/>

</xsl:stylesheet>
