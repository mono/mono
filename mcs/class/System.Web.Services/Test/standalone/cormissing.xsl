<?xml version="1.0" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

	<xsl:output method="text" indent="no"/>
	<!--	<xsl:output method="xml"/>-->
	<xsl:strip-space elements="*"/>

	<!-- assembly -->
	<xsl:template match="/assemblies">
		<xsl:apply-templates select="assembly">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="assemblies/assembly">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">y</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>


	<!-- namespace -->
	<xsl:template match="assembly/namespaces">
		<xsl:apply-templates select="namespace">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="namespaces/namespace">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">n</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>


	<xsl:template match="namespace/classes">
		<xsl:apply-templates select="interface">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
		<xsl:apply-templates select="class">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
		<xsl:apply-templates select="struct">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
		<xsl:apply-templates select="delegate">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
		<xsl:apply-templates select="enum">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>


	<!-- class -->
	<xsl:template match="classes/class">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">c</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="(@missing_total or @todo_total or @extra_total or @warning_total or @error) and not(@presence)">
				<xsl:apply-templates select="attributes"/>
				<xsl:apply-templates select="interfaces"/>
				<xsl:apply-templates select="constructors"/>
				<xsl:apply-templates select="./*[local-name() != 'attributes' and local-name() != 'constructors' and local-name() != 'interfaces']"/>
			</xsl:if>
	</xsl:template>


	<!-- struct -->
	<xsl:template match="classes/struct[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">s</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>



	<!-- interface -->
	<xsl:template match="interfaces">
		<xsl:apply-templates select="interface">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="interface[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">i</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>


	<!-- delegate -->
	<xsl:template match="classes/delegate[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">d</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>


	<!-- enumeration -->
	<xsl:template match="classes/enum[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">en</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>


	<!-- method -->
	<xsl:template match="methods">
		<xsl:apply-templates select="method">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="methods/method[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">m</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>


	<!-- property -->
	<xsl:template match="properties">
		<xsl:apply-templates select="property">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="properties/property[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">p</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>


	<!-- event -->
	<xsl:template match="events">
		<xsl:apply-templates select="event">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="events/event[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">e</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>


	<!-- constructor -->
	<xsl:template match="constructors">
		<xsl:apply-templates select="constructor">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="constructors/constructor[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">x</xsl:with-param>
				<xsl:with-param name="image">m</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>


	<!-- field -->
	<xsl:template match="fields">
		<xsl:apply-templates select="field">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="fields/field[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">f</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>

	<!-- accessor -->
	<xsl:template match="property/accessors">
		<xsl:apply-templates select="method">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="property[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]/accessors/method[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">o</xsl:with-param>
				<xsl:with-param name="image">m</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>


	<!-- attribute -->
	<xsl:template match="attributes">
		<xsl:apply-templates select="attribute">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="attributes/attribute[@missing_total or @todo_total or @extra_total or @warning_total or @error or @presence]">
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">r</xsl:with-param>
			</xsl:call-template>
			<xsl:if test="not(@presence)">
				<xsl:apply-templates/>
			</xsl:if>
	</xsl:template>



	<!-- support templates -->

	<xsl:template name="ELEMENT">
		<xsl:param name="class"/>
		<xsl:param name="image"/>
		<xsl:for-each select="ancestor::node()"><xsl:text> </xsl:text></xsl:for-each>
		<xsl:choose>
			<xsl:when test="@error != 'todo'">[E] </xsl:when>
			<xsl:when test="@error = 'todo'">[T] </xsl:when>
			<xsl:when test="@presence = 'missing'">[-] </xsl:when>
			<xsl:when test="@presence = 'extra'">[+] </xsl:when>
			<xsl:otherwise>[ ] </xsl:otherwise>
		</xsl:choose>
		<xsl:value-of select="$image"/>
		<xsl:call-template name="name"/>
		<xsl:if test="not(@presence)">
			<xsl:call-template name="status"/>
		</xsl:if>
		<xsl:text>
</xsl:text>
	</xsl:template>

	<xsl:template name="status">
		<xsl:if test="@complete_total and @complete_total != 0">
				<xsl:text>:</xsl:text>
				<xsl:value-of select="@complete_total"/>
				<xsl:text>%</xsl:text>
		</xsl:if>
		<xsl:if test="@todo_total"> TODO:<xsl:value-of select="@todo_total"/></xsl:if>
		<xsl:if test="@missing_total"> MISSING:<xsl:value-of select="@missing_total"/></xsl:if>
		<xsl:if test="@extra_total"> EXTRA:<xsl:value-of select="@extra_total"/></xsl:if>
		<xsl:if test="@warning_total"> WARNING:<xsl:value-of select="@warning_total"/></xsl:if>
	</xsl:template>

	<xsl:template name="name">
		<xsl:if test="@name">
			<xsl:value-of select="@name"/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="warnings/warning" mode="hover">
		<xsl:text>WARNING: </xsl:text>
		<xsl:value-of select="@text"/>
	</xsl:template>

</xsl:stylesheet>
