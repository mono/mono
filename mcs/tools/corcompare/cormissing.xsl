<?xml version="1.0" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

	<xsl:output method="html" indent="no"/>
	<!--	<xsl:output method="xml"/>-->
	<xsl:strip-space elements="*"/>

	<xsl:template match="/">
<!--
		<HTML>
			<HEAD>
				<TITLE>
					Mono Class Library Status
				</TITLE>
				<SCRIPT src="cormissing.js"></SCRIPT>
				<LINK rel="stylesheet" type="text/css" href="cormissing.css"></LINK>
			</HEAD>
			<BODY onLoad="onLoad();">
				<P>
					<H1>Mono Class Library Status</H1>
				</P>
-->
				<P>
					<TABLE>
						<TR>
							<TD> <INPUT type="checkbox" ID="missing" onClick="selectMissing();" checked="1"/> </TD>
							<TD> <IMG src="cm/sm.gif"/> </TD>
							<TD> Missing </TD>
						</TR>
						<TR>
							<TD> <INPUT type="checkbox" ID="todo" onClick="selectTodo();" checked="1"/> </TD>
							<TD> <IMG src="cm/st.gif"/> </TD>
							<TD> TODO </TD>
						</TR>
						<TR>
							<TD> </TD>
							<TD> <IMG src="cm/sc.gif"/> </TD>
							<TD> Completed </TD>
						</TR>
					</TABLE>
				</P>
				<DIV ID="ROOT">
					<xsl:apply-templates/>
				</DIV>
<!--
			</BODY>
		</HTML>
-->
	</xsl:template>



	<!-- assembly -->
	<xsl:template match="/assemblies">
		<xsl:apply-templates select="assembly">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="assemblies/assembly[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">y</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>


	<!-- namespace -->
	<xsl:template match="assembly/namespaces">
		<xsl:apply-templates select="namespace">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="namespaces/namespace[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">n</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
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
	<xsl:template match="classes/class[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">c</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates select="constructors"/>
			<xsl:apply-templates select="./*[local-name() != 'constructors']"/>
		</DIV>
	</xsl:template>


	<!-- struct -->
	<xsl:template match="classes/struct[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">s</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>


	<!-- interface -->
	<xsl:template match="classes/interface[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">i</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>


	<!-- delegate -->
	<xsl:template match="classes/delegate[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">d</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>


	<!-- enumeration -->
	<xsl:template match="classes/enum[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">en</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>


	<!-- method -->
	<xsl:template match="methods">
		<xsl:apply-templates select="method">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="methods/method[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">m</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>


	<!-- property -->
	<xsl:template match="properties">
		<xsl:apply-templates select="property">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="properties/property[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">p</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>


	<!-- event -->
	<xsl:template match="events">
		<xsl:apply-templates select="event">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="events/event[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">e</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>


	<!-- constructor -->
	<xsl:template match="constructors">
		<xsl:apply-templates select="constructor">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="constructors/constructor[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">x</xsl:with-param>
				<xsl:with-param name="image">m</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>


	<!-- field -->
	<xsl:template match="fields">
		<xsl:apply-templates select="field">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="fields/field[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">f</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>

	<!-- accessor -->
	<xsl:template match="property/accessors">
		<xsl:apply-templates select="method">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="property[@status != 'complete']/accessors/method">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">o</xsl:with-param>
				<xsl:with-param name="image">m</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>


	<!-- attribute -->
	<xsl:template match="attributes">
		<xsl:apply-templates select="attribute">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="attributes/attribute[@status != 'complete']">
		<DIV>
			<xsl:call-template name="ELEMENT">
				<xsl:with-param name="class">r</xsl:with-param>
			</xsl:call-template>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>



	<!-- support templates -->

	<xsl:template name="ELEMENT">
		<xsl:param name="class"/>
		<xsl:param name="image"/>
			<xsl:attribute name="class">
				<xsl:value-of select="$class"/>
				<xsl:if test="./node() and local-name() != 'assembly'">_</xsl:if>
			</xsl:attribute>
			<xsl:call-template name="toggle"/>
			<xsl:if test="@status">
				<xsl:choose>
					<xsl:when test="@status = 'missing'">
						<img src="cm/sm.gif" class="t"/>
					</xsl:when>
					<xsl:when test="@status = 'todo'">
						<img src="cm/st.gif" class="t"/>
					</xsl:when>
					<xsl:when test="@status = 'complete'">
						<img src="cm/sc.gif" class="t"/>
					</xsl:when>
				</xsl:choose>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="$image">
					<img src="cm/{$image}.gif" class="t"/>
				</xsl:when>
				<xsl:otherwise>
					<img src="cm/{$class}.gif" class="t"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:call-template name="name"/>
			<xsl:call-template name="status"/>
	</xsl:template>

	<xsl:template name="status">
		<xsl:if test="@complete and @complete != 0">
			<SPAN class="st">
				<img src="cm/sc.gif"/>:
				<xsl:value-of select="@complete"/>%
			</SPAN>
		</xsl:if>
		<xsl:if test="@todo and @todo != 0">
			<SPAN class="st">
				<img src="cm/st.gif"/>:
				<xsl:value-of select="@todo"/>
			</SPAN>
		</xsl:if>
		<xsl:if test="@missing and @missing != 0">
			<SPAN class="st">
				<img src="cm/sm.gif"/>:
				<xsl:value-of select="@missing"/>
			</SPAN>
		</xsl:if>
	</xsl:template>

	<xsl:template name="toggle">
		<xsl:choose>
			<xsl:when test=".//*[@status and @status != 'complete'] and local-name() != 'assembly'">
				<IMG src="cm/tp.gif" class="t"/>
			</xsl:when>
			<xsl:when test=".//*[@status and @status != 'complete']">
				<IMG src="cm/tm.gif" class="t"/>
			</xsl:when>
			<xsl:otherwise>
				<IMG src="cm/tb.gif"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="name">
		<xsl:if test="@name">
			<SPAN class="l"><xsl:value-of select="@name"/></SPAN>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
