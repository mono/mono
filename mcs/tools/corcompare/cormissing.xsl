<?xml version="1.0" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

	<xsl:output method="html" indent="no"/>
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
							<TD> <IMG src="cm/missing.gif"/> </TD>
							<TD> Missing </TD>
						</TR>
						<TR>
							<TD> <INPUT type="checkbox" ID="todo" onClick="selectTodo();" checked="1"/> </TD>
							<TD> <IMG src="cm/todo.gif"/> </TD>
							<TD> TODO </TD>
						</TR>
						<TR>
							<TD> </TD>
							<TD> <IMG src="cm/complete.gif"/> </TD>
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

	<xsl:template match="assemblies/assembly">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">assembly</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<!-- namespace -->
	<xsl:template match="assembly/namespaces">
		<xsl:apply-templates select="namespace">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="namespaces/namespace">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">namespace</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<!-- class -->
	<xsl:template match="namespace/classes">
		<xsl:apply-templates select="class">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="classes/class">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">class</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<!-- struct -->
	<xsl:template match="namespace/structs">
		<xsl:apply-templates select="struct">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="structs/struct">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">struct</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<!-- delegate -->
	<xsl:template match="namespace/delegates">
		<xsl:apply-templates select="delegate">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="delegates/delegate">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">delegate</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<!-- enumeration -->
	<xsl:template match="namespace/enumerations">
		<xsl:apply-templates select="enumeration">
			<xsl:sort select="@name"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="enumerations/enumeration">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">enumeration</xsl:with-param>
			<xsl:with-param name="image">enum</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<!-- method -->
	<xsl:template match="class/methods">
		<DIV class="events">
			<xsl:apply-templates select="method">
				<xsl:sort select="@name"/>
			</xsl:apply-templates>
		</DIV>
	</xsl:template>

	<xsl:template match="methods/method">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">method</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<!-- property -->
	<xsl:template match="class/properties">
		<DIV class="properties">
			<xsl:apply-templates select="property">
				<xsl:sort select="@name"/>
			</xsl:apply-templates>
		</DIV>
	</xsl:template>

	<xsl:template match="properties/property">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">property</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<!-- event -->
	<xsl:template match="class/events">
		<DIV class="events">
			<xsl:apply-templates select="event">
				<xsl:sort select="@name"/>
			</xsl:apply-templates>
		</DIV>
	</xsl:template>

	<xsl:template match="events/event">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">event</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<!-- constructor -->
	<xsl:template match="class/constructors">
		<DIV class="constructors">
			<xsl:apply-templates select="constructor">
				<xsl:sort select="@name"/>
			</xsl:apply-templates>
		</DIV>
	</xsl:template>

	<xsl:template match="constructors/constructor">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">constructor</xsl:with-param>
			<xsl:with-param name="image">method</xsl:with-param>
		</xsl:call-template>
	</xsl:template>


	<!-- field -->
	<xsl:template match="class/fields">
		<DIV class="fields">
			<xsl:apply-templates select="field">
				<xsl:sort select="@name"/>
			</xsl:apply-templates>
		</DIV>
	</xsl:template>

	<xsl:template match="fields/field">
		<xsl:call-template name="ELEMENT">
			<xsl:with-param name="type">field</xsl:with-param>
		</xsl:call-template>
	</xsl:template>



	<!-- support templates -->

	<xsl:template name="ELEMENT">
		<xsl:param name="type"/>
		<xsl:param name="image"></xsl:param>
		<DIV class="{$type}">
			<xsl:call-template name="toggle"/>
			<xsl:if test="@status">
				<img src="cm/{@status}.gif" name="status" class="toggle"/>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="$image">
					<img src="cm/{$image}.gif" class="toggle"/>
				</xsl:when>
				<xsl:otherwise>
					<img src="cm/{$type}.gif" class="toggle"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:call-template name="name"/>
			<xsl:call-template name="status"/>
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>

	<xsl:template name="status">
		<xsl:if test="(@complete and @complete!=0) or (@todo and @todo!=0) or (@missing and @missing!=0)">
			<SPAN class="status">
				<xsl:if test="@complete and @complete!=0">
					<img src="cm/complete.gif"/>:
				<xsl:value-of select="@complete"/>
				</xsl:if>
			</SPAN>
			<SPAN class="status">
				<xsl:if test="@todo and @todo!=0">
					<img src="cm/todo.gif"/>:
					<xsl:value-of select="@todo"/>
				</xsl:if>
			</SPAN>
			<SPAN class="status">
				<xsl:if test="@missing and @missing!=0">
					<img src="cm/missing.gif"/>:
					<xsl:value-of select="@missing"/>
				</xsl:if>
			</SPAN>
		</xsl:if>
	</xsl:template>

	<xsl:template name="toggle">
		<xsl:choose>
			<xsl:when test="./node()">
				<IMG src="cm/toggle_minus.gif" class="toggle"/>
			</xsl:when>
			<xsl:otherwise>
				<IMG src="cm/toggle_blank.gif" class=""/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="name">
		<xsl:if test="@name">
			<SPAN class="name"><xsl:value-of select="@name"/></SPAN>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
